<%@ WebHandler Language="C#" Class="PlaceOrder" %>

using System;
using System.Web;
using System.Globalization;
using System.Collections.Generic;
using LitJson;
using Com.Alipay;

/// <summary>
/// 1，处理新订单流程：校验客户端表单提交值->构造PO对象->校验PO对象值->注册PO对象事件处理函数->调用SubmitOrder(po)方法提交订单
/// 2，处理已有订单流程：校验客户端提交的POID值->调用SubmitOrder(POID)，在其中校验此订单的prepay_id是否有效，无效则重新发起统一下单获取prepay_id，并生成JS支付字符串
/// 3，根据新订单或已有订单流程，返回JS支付字符串或新订单ID给客户端，如有异常则返回stateCode
/// </summary>
public class PlaceOrder : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{

    public void ProcessRequest(HttpContext context)
    {
        ProductOrder newPO = null, existPO = null;

        //根据prepay_id生成的微信支付参数
        string wxPayParam = string.Empty;

        //微信统一下单API返回的错误码
        WeChatPayData stateCode = new WeChatPayData();

        //支付宝请求字符串
        string alipayParam = string.Empty;

        try
        {
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            if (wxUser == null || string.IsNullOrEmpty(wxUser.OpenID))
            {
                throw new Exception("请登录");
            }

            //1，处理新订单流程
            if (context.Request.Form != null && string.IsNullOrEmpty(context.Request.QueryString["PoID"]))
            {
                //客户端提交的订单信息
                string strOrderInfo = HttpUtility.UrlDecode(context.Request.Form.ToString());
                if (!string.IsNullOrEmpty(strOrderInfo))
                {
                    //--------------校验表单信息关键字段-------------------
                    JsonData jOrderInfo = JsonMapper.ToObject(strOrderInfo);
                    if (jOrderInfo == null || !jOrderInfo.Keys.Contains("name") || !jOrderInfo.Keys.Contains("phone") || !jOrderInfo.Keys.Contains("address")
                            || !jOrderInfo.Keys.Contains("memo") || !jOrderInfo.Keys.Contains("paymentTerm") || !jOrderInfo.Keys.Contains("usedMemberPoints")
                            || !jOrderInfo.Keys.Contains("wxCard")
                            || !jOrderInfo.Keys.Contains("prodItems") || !jOrderInfo["prodItems"].IsArray || jOrderInfo["prodItems"].Count < 1)
                    {
                        throw new Exception("订单姓名、电话、地址、支付方式、会员积分、微信优惠券、备注、商品项信息不完整。");
                    }

                    //--------------生成订单业务对象START-----------------
                    newPO = new ProductOrder();
                    newPO.OrderID = ProductOrder.MakeOrderID();    //生成OrderID
                    newPO.Purchaser = wxUser;
                    newPO.ClientIP = wxUser.ClientIP;
                    newPO.DeliverName = jOrderInfo["name"].ToString().Trim();
                    newPO.DeliverPhone = jOrderInfo["phone"].ToString().Trim();
                    newPO.DeliverAddress = jOrderInfo["address"].ToString().Trim();
                    newPO.OrderMemo = jOrderInfo["memo"].ToString().Trim();
                    newPO.OrderDate = DateTime.Now;
                    newPO.AP_SellerID = AliPayConfig.seller_id;
                    //订单初始状态：未发货、未签收、未撤单
                    newPO.IsDelivered = false;
                    newPO.IsAccept = false;
                    newPO.IsCancel = false;

                    //处理订单支付方式和支付状态
                    int paymentTerm;
                    if (int.TryParse(jOrderInfo["paymentTerm"].ToString(), out paymentTerm))
                    {
                        switch (paymentTerm)
                        {
                            case (int)PaymentTerm.WECHAT:
                                newPO.PaymentTerm = PaymentTerm.WECHAT;
                                newPO.TradeState = TradeState.NOTPAY;
                                break;
                            case (int)PaymentTerm.CASH:
                                newPO.PaymentTerm = PaymentTerm.CASH;
                                newPO.TradeState = TradeState.CASHNOTPAID;
                                break;
                            case (int)PaymentTerm.ALIPAY:
                                newPO.PaymentTerm = PaymentTerm.ALIPAY;
                                newPO.TradeState = TradeState.AP_WAIT_BUYER_PAY;
                                break;
                            default:
                                throw new Exception("未知的支付方式PaymentTerm");
                        }
                    }
                    else
                    {
                        throw new Exception("未知的支付方式PaymentTerm");
                    }

                    //查找下单用户的推荐人，且不是下单人，则写入订单对象
                    if (!string.IsNullOrEmpty(wxUser.AgentOpenID) && wxUser.AgentOpenID != wxUser.OpenID)
                    {
                        newPO.Agent = WeChatUserDAO.FindUserByOpenID(wxUser.AgentOpenID);
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
                                    //如果此商品是不限库存量，或者购买量不超过库存量，则可以下单
                                    if (fruit.InventoryQty == -1 || fruit.InventoryQty >= qty)
                                    {
                                        od = new OrderDetail(fruit);

                                        od.ProductID = prodID;
                                        od.PurchaseQty = qty;
                                        od.OrderProductName = fruit.FruitName;
                                        od.PurchasePrice = fruit.FruitPrice;
                                        od.PurchaseUnit = fruit.FruitUnit;

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
                        throw new Exception(string.Format("抱歉，商品“{0}”库存量不足，请调整购买数量后重新下单。", string.Join<string>(",", outOfStockProdItems)));
                    }

                    //根据订单商品项金额计算运费
                    if (newPO.OrderDetailPrice < Config.FreightFreeCondition)
                    {
                        newPO.Freight = Config.Freight;
                    }
                    else
                    {
                        newPO.Freight = 0;
                    }

                    //订单所需支付金额=商品价格+运费
                    decimal subTotalAndFreight = newPO.OrderDetailPrice + newPO.Freight;
                    //订单所需支付金额折算成会员积分，积分使用上限为订单实付金额的50%
                    int equivalentMemberPointsOfOrder = (int)Math.Floor(subTotalAndFreight / 2 * Config.MemberPointsExchangeRate);
                    //判断此订单最大可使用积分与用户积分账户的可使用积分相比，孰小者决定了用户可使用的积分上限
                    int maxDiscountMemberPoints = equivalentMemberPointsOfOrder < wxUser.MemberPoints ? equivalentMemberPointsOfOrder : wxUser.MemberPoints;

                    //根据用户选择的会员积分点数，校验可用积分，计算积分折扣
                    int usedMemberPoints;
                    if (int.TryParse(jOrderInfo["usedMemberPoints"].ToString(), out usedMemberPoints) && (usedMemberPoints <= maxDiscountMemberPoints && usedMemberPoints >= 0))
                    {
                        newPO.UsedMemberPoints = usedMemberPoints;
                        //由于会员积分兑换规则可能发生变动，所以需要单独记录每笔订单的积分抵扣金额
                        newPO.MemberPointsDiscount = (decimal)usedMemberPoints / Config.MemberPointsExchangeRate;
                        //新订单的积分默认为没有累加到积分账户余额，待订单确认支付后，再计入积分账户余额
                        newPO.IsCalMemberPoints = false;
                    }
                    else
                    {
                        throw new Exception(string.Format("本订单可使用的积分范围：0~{0}", maxDiscountMemberPoints));
                    }

                    //订单使用的微信优惠券
                    if (jOrderInfo["wxCard"].Keys.Contains("cardId") && !string.IsNullOrEmpty(jOrderInfo["wxCard"]["cardId"].ToString()) && !string.IsNullOrEmpty(jOrderInfo["wxCard"]["encryptCode"].ToString()))
                    {
                        WxCard wxCard = WxCard.GetCard(jOrderInfo["wxCard"]["cardId"].ToString());
                        if (wxCard != null)
                        {
                            string wxCode = WxCard.DecryptCode(jOrderInfo["wxCard"]["encryptCode"].ToString());
                            if (!string.IsNullOrEmpty(wxCode))
                            {
                                if ((newPO.OrderDetailPrice + newPO.Freight) >= wxCard.LeastCost)
                                {
                                    //只有微信卡券CODE解码正确，且满足使用条件，此优惠券才有效
                                    newPO.WxCard = wxCard;
                                    newPO.WxCard.Code = wxCode;
                                    newPO.WxCardDiscount = wxCard.ReduceCost;
                                }
                                else
                                {
                                    newPO.WxCard = null;
                                    newPO.WxCardDiscount = 0;
                                }
                            }
                            else
                            {
                                newPO.WxCard = null;
                                newPO.WxCardDiscount = 0;
                            }
                        }
                        else
                        {
                            newPO.WxCard = null;
                            newPO.WxCardDiscount = 0;
                        }
                    }
                    else
                    {
                        newPO.WxCard = null;
                        newPO.WxCardDiscount = 0;
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
                    switch (newPO.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT:
                            ProductOrder.SubmitOrder(newPO, out wxPayParam, out stateCode);
                            break;
                        case PaymentTerm.ALIPAY:
                            ProductOrder.SubmitOrder(newPO, out alipayParam);
                            break;
                        case PaymentTerm.CASH:
                            ProductOrder.SubmitOrder(newPO);
                            break;
                    }

                }
                else
                {
                    throw new Exception("没有提交订单信息");
                }
            }
            else
            {
                //2，现有订单处理流程

                int poID, paymentTerm;
                if (int.TryParse(context.Request.QueryString["PoID"], out poID))
                {
                    if (int.TryParse(context.Request.QueryString["PaymentTerm"], out paymentTerm))
                    {
                        switch (paymentTerm)
                        {
                            case (int)PaymentTerm.WECHAT:
                                existPO = ProductOrder.SubmitOrder(poID, out wxPayParam, out stateCode);
                                break;
                            case (int)PaymentTerm.ALIPAY:
                                existPO = ProductOrder.SubmitOrder(poID, out alipayParam);
                                break;
                            default:
                                throw new Exception("未知的支付方式PaymentTerm");
                        }
                    }
                    else
                    {
                        throw new Exception("未知的支付方式PaymentTerm");
                    }
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
                            context.Response.Write(wxPayParam);
                            break;
                        case PaymentTerm.ALIPAY: //如是支付宝支付，返回支付宝请求参数
                            context.Response.Write(alipayParam);
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
                        switch (existPO.PaymentTerm)
                        {
                            case PaymentTerm.WECHAT: //如是微信支付，返回JS支付参数
                                context.Response.Write(wxPayParam);
                                break;
                            case PaymentTerm.ALIPAY: //如是支付宝支付，返回支付宝请求参数
                                context.Response.Write(alipayParam);
                                break;
                        }
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