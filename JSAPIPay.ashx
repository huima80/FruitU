<%@ WebHandler Language="C#" Class="JSAPIPay" %>

using System;
using System.Web;
using System.Globalization;
using LitJson;

/// <summary>
/// 1，处理新订单流程：生成订单ID->调用统一下单API生成prepay_id->prepay_id和订单信息一起入库->生成JS支付参数返回给客户端->如果用户取消了支付，当再次点击支付时直接使用JS参数发起支付->如果用户未支付就离开当前页面，则转入流程2
/// 2，处理已有订单流程：根据订单ID查询订单->比较订单时间和当前时间是否超过prepay_id的有效期2小时->如果未超过则使用prepay_id生成JS支付参数->如果超过则重新发起统一下单获取新的prepay_id->更新数据库->生成JS支付参数->返回给客户发起支付->如果微信仍然返回订单ID重复，则提示用户重新下单，走流程1
/// 3，支付成功后，微信回调系统，更新库中的tradeState, tradeStateDesc, transactionID, transactionTime字段，系统也要定期向微信查询订单状态，主动更新数据库，保持订单状态同步。
/// </summary>
public class JSAPIPay : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    public void ProcessRequest(HttpContext context)
    {

        //微信统一下单API返回的预支付回话标示
        string prepayID = string.Empty;

        //根据prepay_id生产的JS支付参数
        string wxJsApiParam = string.Empty;

        //货到付款时返回的新生成的订单ID
        string newPoID = string.Empty;

        //微信统一下单API返回的错误码
        string stateCode = string.Empty;

        try
        {

            int poID = 0;

            if (!string.IsNullOrEmpty(context.Request.QueryString["PoID"]))
            {
                poID = int.Parse(context.Request.QueryString["PoID"]);
            }


            JsonData jAuthInfo = new JsonData();
            jAuthInfo = context.Session["AuthInfo"] as JsonData;

            if (poID == 0)  //1，新订单处理流程
            {
                //客户端提交的订单信息
                string orderInfo = context.Request.Form[0];
                if (!string.IsNullOrEmpty(orderInfo))
                {
                    //订单信息转换成JSON对象
                    JsonData jOrderInfo = JsonMapper.ToObject(orderInfo);

                    //--------------生成订单业务对象START-----------------
                    ProductOrder po = new ProductOrder();
                    po.OrderID = ProductOrder.MakeOrderID();    //生成OrderID
                    po.OpenID = jAuthInfo["openid"].ToString();
                    po.ClientIP = jAuthInfo["client_ip"].ToString();
                    po.DeliverName = jOrderInfo["name"].ToString();
                    po.DeliverPhone = jOrderInfo["phone"].ToString();
                    po.DeliverAddress = jOrderInfo["address"].ToString();
                    po.OrderMemo = jOrderInfo["memo"].ToString();
                    po.OrderDate = DateTime.Now;
                    po.PaymentTerm = (PaymentTerm)int.Parse(jOrderInfo["paymentTerm"].ToString());

                    //订单初始状态：未支付、未发货、未签收
                    po.TradeState = TradeState.NOTPAY;
                    po.IsDelivered = false;
                    po.IsAccept = false;

                    //订单商品信息
                    OrderDetail od;
                    for (int i = 0; i < jOrderInfo["prodItems"].Count; i++)
                    {
                        od = new OrderDetail();

                        int prodID = int.Parse(jOrderInfo["prodItems"][i]["prodID"].ToString());
                        int qty = int.Parse(jOrderInfo["prodItems"][i]["qty"].ToString());

                        //根据订单中每个商品ID查询其详细信息，并放入OrderDetail对象
                        Fruit fruit = Fruit.FindFruitByID(prodID);

                        if (fruit != null)
                        {
                            od.ProductID = prodID;
                            od.OrderProductName = fruit.FruitName;
                            od.PurchasePrice = fruit.FruitPrice;
                            od.PurchaseQty = qty;
                            od.PurchaseUnit = fruit.FruitUnit;

                            po.OrderDetailList.Add(od);
                        }

                    }
                    //--------------生成订单业务对象END-----------------


                    //--------------校验订单业务对象START-----------------
                    if(string.IsNullOrEmpty(po.DeliverName))
                    {
                        stateCode += "收货人名称，";
                    }
                    if(string.IsNullOrEmpty(po.DeliverPhone))
                    {
                        stateCode += "收货人电话，";
                    }
                    if(string.IsNullOrEmpty(po.DeliverAddress))
                    {
                        stateCode += "收货人地址，";
                    }
                    if (po.OrderDetailList.Count == 0)
                    {
                        stateCode += "商品信息，";
                    }
                    if(!string.IsNullOrEmpty(stateCode))
                    {
                        stateCode = string.Format("请填写：{0}", stateCode.Trim('，'));
                        throw new Exception(stateCode);
                    }
                    //--------------校验订单业务对象END-----------------



                    //--------------统一下单、订单入库START-----------------
                    switch (po.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT:    //付款方式为微信支付

                            //统一下单，获取prepay_id，如果有错误发生，则stateCode有返回值
                            prepayID = WxPayAPI.CallUnifiedOrderAPI(jAuthInfo, po, out stateCode);

                            if (!string.IsNullOrEmpty(prepayID))
                            {
                                //保存prepay_id，一起入库
                                po.PrepayID = prepayID;

                                ProductOrder.AddOrder(po);

                                //根据prepay_id生成JS支付参数
                                wxJsApiParam = WxPayAPI.MakeWXPayJsParam(prepayID);
                            }
                            break;
                        case PaymentTerm.CASH:  //付款方式为货到付款，不统一下单，直接入库

                            ProductOrder.AddOrder(po);
                            newPoID = string.Format("{{\"NewPoID\":\"{0}\"}}", po.ID);
                            break;
                        default:
                            throw new Exception("不支持的支付方式。");
                    }
                    //--------------统一下单、订单入库END-----------------
                }
                else
                {
                    throw new Exception("没有提交订单信息");
                }
            }
            else  //2，现有订单处理流程
            {
                ProductOrder po;

                //加载完整的订单信息
                po = ProductOrder.FindOrderByID(poID);

                if (po != null)
                {
                    //取出此订单的prepay_id，后面校验其是否在有效期
                    prepayID = po.PrepayID;

                    //如果此订单的PrepayID不为空（下单时选择的微信支付）
                    if (!string.IsNullOrEmpty(prepayID))
                    {
                        //根据PrepayID中的时间段，计算最近一次获取PrepayID到现在的时间间隔，以此判断PrepayID是否过期。因为OrderDate是下单时间，不会变化，所以不能用其判断。
                        DateTime timeOfPrepayID;
                        if(!DateTime.TryParseExact(po.PrepayID.Substring(2, 14), "yyyyMMddHHmmss", null, DateTimeStyles.None, out timeOfPrepayID))
                        {
                            //如果PrepayID时间截取转换失败，则依据订单时间计算
                            timeOfPrepayID = po.OrderDate;
                        }

                        //如果上次获取的PrepayID已超过有效期，需要重新统一下单，获取新的prepay_id
                        if (DateTime.Now >= timeOfPrepayID.AddMinutes(Config.WeChatOrderExpire))
                        {
                            //重新发起统一下单，获取新的prepay_id
                            prepayID = WxPayAPI.CallUnifiedOrderAPI(jAuthInfo, po, out stateCode);

                            //使用刷新的PrepayID更新数据库
                            ProductOrder.UpdatePrepayID(po.ID, prepayID);
                        }
                    }
                    else
                    {
                        //如果PrepayID为空（上次下单时选择的货到付款），这里使用OrderID发起统一下单，首次获取prepay_id
                        prepayID = WxPayAPI.CallUnifiedOrderAPI(jAuthInfo, po, out stateCode);

                        //使用首次获得的PrepayID更新数据库
                        ProductOrder.UpdatePrepayID(po.ID, prepayID);
                    }

                    if (!string.IsNullOrEmpty(prepayID))
                    {
                        //根据新的prepay_id生成前端JS支付参数
                        wxJsApiParam = WxPayAPI.MakeWXPayJsParam(prepayID);
                    }
                }
                else
                {
                    throw new Exception(string.Format("订单“{0}”不存在", poID));
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("JsApiPay", ex.Message);
            stateCode = string.Format("{{\"result_code\":\"FAIL\",\"err_code_des\":\"{0}\"}}", ex.Message);
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            if (!string.IsNullOrEmpty(stateCode))
            {
                context.Response.Write(stateCode);  //提交值校验错误，或者统一下单有错误发生
            }
            else
            {
                if (!string.IsNullOrEmpty(wxJsApiParam)) //wxJsApiParam不为空，则统一下单成功，获取到了prepay_id
                {
                    context.Response.Write(wxJsApiParam);
                }
                else
                {
                    if (!string.IsNullOrEmpty(newPoID)) //newPoID不为空，表示货到付款，订单入库成功
                    {
                        context.Response.Write(newPoID);
                    }
                }
            }
            context.Response.End();
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}