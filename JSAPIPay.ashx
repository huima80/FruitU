<%@ WebHandler Language="C#" Class="JSAPIPay" %>

using System;
using System.Web;
using System.Globalization;
using System.Collections.Generic;
using LitJson;

/// <summary>
/// 1，处理新订单流程：校验客户端表单提交值->构造PO对象->校验PO对象值->注册PO对象事件处理函数->调用SubmitOrder(po)方法提交订单
/// 2，处理已有订单流程：校验客户端提交的POID值->调用SubmitOrder(POID)，在其中校验此订单的prepay_id是否有效，无效则重新发起统一下单获取prepay_id，并生成JS支付字符串
/// 3，根据新订单或已有订单流程，返回JS支付字符串或新订单ID给客户端，如有异常则返回stateCode
/// </summary>
public class JSAPIPay : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{

    public void ProcessRequest(HttpContext context)
    {
        ProductOrder newPO = null, existPO = null;

        //根据prepay_id生成的JS支付参数
        string wxJsApiParam = string.Empty;

        //微信统一下单API返回的错误码
        WeChatPayData stateCode = new WeChatPayData();

        try
        {
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            if (wxUser == null || string.IsNullOrEmpty(wxUser.OpenID))
            {
                throw new Exception("请登录");
            }

            //1，处理新订单流程
            if (context.Request.Form.Count != 0 && string.IsNullOrEmpty(context.Request.QueryString["PoID"]))
            {
                //客户端提交的订单信息
                string orderInfo = context.Request.Form[0];
                if (!string.IsNullOrEmpty(orderInfo))
                {
                    //--------------校验表单信息关键字段-------------------
                    JsonData jOrderInfo = JsonMapper.ToObject(orderInfo);
                    if (jOrderInfo == null || !jOrderInfo.Keys.Contains("name") || !jOrderInfo.Keys.Contains("phone") || !jOrderInfo.Keys.Contains("address")
                            || !jOrderInfo.Keys.Contains("memo") || !jOrderInfo.Keys.Contains("paymentTerm") || !jOrderInfo.Keys.Contains("freight")
                            || !jOrderInfo.Keys.Contains("prodItems") || !jOrderInfo["prodItems"].IsArray || jOrderInfo["prodItems"].Count < 1)
                    {
                        throw new Exception("订单姓名、电话、地址、支付方式、运费、备注、商品项信息不完整。");
                    }

                    //--------------生成订单业务对象START-----------------
                    newPO = new ProductOrder();
                    newPO.OrderID = ProductOrder.MakeOrderID();    //生成OrderID
                    newPO.Purchaser = wxUser;
                    newPO.ClientIP = wxUser.ClientIP;
                    newPO.DeliverName = jOrderInfo["name"].ToString();
                    newPO.DeliverPhone = jOrderInfo["phone"].ToString();
                    newPO.DeliverAddress = jOrderInfo["address"].ToString();
                    newPO.OrderMemo = jOrderInfo["memo"].ToString();
                    newPO.OrderDate = DateTime.Now;
                    newPO.PaymentTerm = (PaymentTerm)int.Parse(jOrderInfo["paymentTerm"].ToString());

                    //订单初始状态：未支付、未发货、未签收、未撤单
                    newPO.IsDelivered = false;
                    newPO.IsAccept = false;
                    newPO.IsCancel = false;
                    switch(newPO.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT:
                            newPO.TradeState = TradeState.NOTPAY;
                            break;
                        case PaymentTerm.CASH:
                            newPO.TradeState = TradeState.CASHNOTPAID;
                            break;
                    }

                    //订单商品项信息
                    OrderDetail od;
                    int prodID, qty;
                    List<string> outOfStockProdItems = new List<string>();

                    for (int i = 0; i < jOrderInfo["prodItems"].Count; i++)
                    {
                        if (jOrderInfo["prodItems"][i].Keys.Contains("prodID") && jOrderInfo["prodItems"][i].Keys.Contains("qty"))
                        {
                            if (int.TryParse(jOrderInfo["prodItems"][i]["prodID"].ToString(), out prodID))
                            {
                                if (!int.TryParse(jOrderInfo["prodItems"][i]["qty"].ToString(), out qty))
                                {
                                    qty = 1;
                                }

                                //根据订单中每个商品ID查询其详细信息，并放入OrderDetail对象
                                Fruit fruit = Fruit.FindFruitByID(prodID);

                                if (fruit != null)
                                {
                                    //如果此商品是无限库存量，或者购买量不超过库存量，则可以下单
                                    if (fruit.InventoryQty == -1 || fruit.InventoryQty >= qty)
                                    {
                                        od = new OrderDetail(fruit);

                                        od.ProductID = prodID;
                                        od.PurchaseQty = qty;
                                        od.OrderProductName = fruit.FruitName;
                                        od.PurchasePrice = fruit.FruitPrice;
                                        od.PurchaseUnit = fruit.FruitUnit;

                                        //注册商品库存量事件，通知管理员
                                        od.InventoryWarn += new EventHandler(WxTmplMsg.SendMsgOnInventoryWarn);

                                        newPO.OrderDetailList.Add(od);
                                    }
                                    else
                                    {
                                        outOfStockProdItems.Add(fruit.FruitName);
                                    }
                                }
                            }
                        }
                    }

                    //校验订单中是否有超出库存量的商品
                    if (outOfStockProdItems.Count > 0)
                    {
                        throw new Exception(string.Format("抱歉，商品“{0}”库存量不足，请调整后重新下单。", string.Join<string>(",", outOfStockProdItems)));
                    }

                    //根据订单商品项金额计算运费，满99元包邮
                    if(newPO.OrderDetailPrice < 99)
                    {
                        newPO.Freight = 10;
                    }
                    else
                    {
                        newPO.Freight = 0;
                    }

                    //--------------生成订单业务对象END-----------------


                    //--------------校验订单业务对象START-----------------
                    string missInfo = string.Empty;
                    if (string.IsNullOrEmpty(newPO.DeliverName))
                    {
                        missInfo += "收货人名称，";
                    }
                    if (string.IsNullOrEmpty(newPO.DeliverPhone))
                    {
                        missInfo += "收货人电话，";
                    }
                    if (string.IsNullOrEmpty(newPO.DeliverAddress))
                    {
                        missInfo += "收货人地址，";
                    }
                    if (newPO.OrderDetailList.Count == 0)
                    {
                        missInfo += "商品信息，";
                    }
                    if (!string.IsNullOrEmpty(missInfo))
                    {
                        missInfo = string.Format("请填写：{0}", missInfo.Trim('，'));
                        throw new Exception(missInfo);
                    }
                    //--------------校验订单业务对象END-----------------

                    //注册订单状态变动事件处理函数，发送微信模板消息给管理员
                    newPO.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);

                    //提交新订单
                    ProductOrder.SubmitOrder(newPO, out wxJsApiParam, out stateCode);
                }
                else
                {
                    throw new Exception("没有提交订单信息");
                }
            }
            else
            {
                //2，现有订单处理流程

                int poID;
                if (int.TryParse(context.Request.QueryString["PoID"], out poID))
                {
                    //处理已有订单
                    existPO = ProductOrder.SubmitOrder(poID, out wxJsApiParam, out stateCode);
                }
                else
                {
                    throw new Exception("订单ID错误");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
            stateCode.SetValue("result_code", "FAIL");
            stateCode.SetValue("err_code", "-1");
            stateCode.SetValue("err_code_des", ex.Message);
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = "text/plain";

            //提交值校验错误，或者统一下单有错误发生
            if (stateCode != null && stateCode.Count != 0)
            {
                context.Response.Write(stateCode.ToJson());
            }
            else
            {
                if (newPO != null)
                {
                    switch (newPO.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT: //如是微信支付，返回JS支付参数
                            context.Response.Write(wxJsApiParam);
                            break;
                        case PaymentTerm.CASH:  //如是货到付款，返回新订单ID
                            JsonData jNewPOID = new JsonData();
                            jNewPOID["NewPOID"] = newPO.ID;
                            context.Response.Write(jNewPOID.ToJson());
                            break;
                    }
                }
                else
                {
                    if (existPO != null)
                    {
                        context.Response.Write(wxJsApiParam);
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