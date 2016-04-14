using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LitJson;

/// <summary>
/// 微信模板消息类
//成功后微信返回同步消息
//{
//"errcode":0,
//"errmsg":"ok",
//"msgid":200228332
//}

//成功后微信返回异步消息
//< xml >
//< ToUserName >< ![CDATA[gh_7f083739789a]] ></ ToUserName >
//< FromUserName >< ![CDATA[oia2TjuEGTNoeX76QEjQNrcURxG8]] & g;</ FromUserName >
//< CreateTime > 1395658920 </ CreateTime >
//< MsgType >< ![CDATA[event]]></MsgType>
//<Event><![CDATA[TEMPLATESENDJOBFINISH]]></Event>
//<MsgID>200163836</MsgID>
//<Status><![CDATA[success]]></Status>
//</xml>
/// </summary>
public static class WxTmplMsg
{
    /// <summary>
    /// 微信模板消息发送开关
    /// </summary>
    public static Dictionary<OrderState, bool> SendTmplMsgSwitch;

    /// <summary>
    /// 微信模板消息API接口
    /// </summary>
    private static readonly string WxTmplMsgUrl = @"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=" + WxJSAPI.GetAccessToken();

    /// <summary>
    /// 订单提交成功通知模板
    /// {{first.DATA}}
    //下单时间：{{keyword1.DATA}}
    //订单总金额：{{keyword2.DATA}}
    //订单详情：{{keyword3.DATA}}
    //详细地址：{{keyword4.DATA}}
    //{{remark.DATA}}
    /// </summary>
    private const string TMPL_ORDER_SUCCESS = "zYBc-cvsf_7cllTNbXE_Y-55jU4owKAhqfEHAxVhDbU";

    /// <summary>
    /// 收到微信支付成功订单模板
    //{{first.DATA
    //}}
    //消费金额：{{keyword1.DATA}}
    //付款用户：{{keyword2.DATA}}
    //成功支付码：{{keyword3.DATA}}
    //消费时间：{{keyword4.DATA}}
    //商户单号：{{keyword5.DATA}}
    //{{remark.DATA}}
    /// </summary>
    private const string TMPL_WXPAY = "xyxsgmpRd5dOJD4Cn-WUV6ZQJ6wBbQADhXkrN2gJ-0Q";

    /// <summary>
    /// 发货通知
    //{{first.DATA}}
    //品名：{{keyword1.DATA}}
    //数量：{{keyword2.DATA}}
    //金额：{{keyword3.DATA}}
    //配送员姓名：{{keyword4.DATA}}
    //联系电话：{{keyword5.DATA}}
    //{{remark.DATA}}    
    /// </summary>
    private const string TMPL_DELIVER = "j7vyhylFGhIHoFfPe6S7dgLIyojTIBb2oBSS_iswo1c";

    /// <summary>
    /// 消费撤销通知
    //{{first.DATA}}
    //撤销时间：{{keynote1.DATA}}
    //撤销金额：{{keynote2.DATA}}
    //{{remark.DATA}}
    /// </summary>
    private const string TMPL_CANCEL = "kIuIdGvvwUyOTYjv0jFM2lICQZkgQRT4zW8bb20mL-A";

    /// <summary>
    /// 消息头颜色
    /// </summary>
    private const string MSG_HEAD_COLOR = "#FF0000";

    /// <summary>
    ///  消息体内容颜色
    /// </summary>
    private const string MSG_BODY_COLOR = "#173177";

    static WxTmplMsg()
    {
        SendTmplMsgSwitch = new Dictionary<OrderState, bool>();
        LoadSendTmplMsgSwitch();
    }

    private static void LoadSendTmplMsgSwitch()
    {
        SendTmplMsgSwitch.Add(OrderState.Submitted, true);
        SendTmplMsgSwitch.Add(OrderState.Paid, true);
        SendTmplMsgSwitch.Add(OrderState.Delivered, true);
        SendTmplMsgSwitch.Add(OrderState.Accepted, true);
        SendTmplMsgSwitch.Add(OrderState.Cancelled, true);
    }

    /// <summary>
    /// 发送微信模板消息
    /// </summary>
    public static JsonData SendMsgOnOrderStateChanged(ProductOrder po, ProductOrder.OrderStateEventArgs e)
    {
        if (po == null || e == null)
        {
            throw new ArgumentNullException("sender或事件参数对象不能为null");
        }
        JsonData jRet = new JsonData();

        try
        {
            //模板消息接收者，1下单通知管理员；2微信支付成功通知管理员；3配送通知用户；4签收不通知；5撤单通知管理员和用户
            List<string> listReceiver;
            listReceiver = new List<string>(Config.WxTmplMsgReceiver.ToArray());

            JsonData jTmplMsg = new JsonData(), jTmplMsgData = new JsonData(), jTmplMsgDataValue;
            string recvMsg;

            //根据不同的订单状态事件，发送不同的微信模板消息
            switch (e.OrderState)
            {
                case OrderState.Submitted:
                    //订单提交时，发送模板消息给管理员
                    if (!SendTmplMsgSwitch[OrderState.Submitted])
                    {
                        return false;
                    }
                    string paymentTerm = string.Empty;
                    switch(po.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT:
                            paymentTerm = "微信支付";
                            break;
                        case PaymentTerm.CASH:
                            paymentTerm = "货到付款";
                            break;
                    }
                    for (int i = 0; i < listReceiver.Count; i++)
                    {
                        jTmplMsg["touser"] = listReceiver[i];
                        jTmplMsg["template_id"] = TMPL_ORDER_SUCCESS;
                        jTmplMsg["url"] = @"http://mahui.me/admin/ManageOrder.aspx";
                        jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("您的订单{0}提交成功", po.OrderID.Substring(18));
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["first"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.OrderDate.ToString();
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword1"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("￥{0}元【{1}】", po.OrderPrice.ToString(), paymentTerm);
                        jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                        jTmplMsgData["keyword2"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.OrderDetails;
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword3"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("{0}({1}) {2}", po.DeliverName, po.DeliverPhone, po.DeliverAddress);
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword4"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("订单备注：{0}", po.OrderMemo);
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["remark"] = jTmplMsgDataValue;

                        jTmplMsg["data"] = jTmplMsgData;

                        //调用微信模板消息接口，并获取返回报文
                        Log.Debug("WxTmplMsg", "SendOrderMsg request" + jTmplMsg.ToJson());
                        recvMsg = HttpService.Post(jTmplMsg.ToJson(), WxTmplMsg.WxTmplMsgUrl, false, Config.WeChatAPITimeout);
                        Log.Debug("WxTmplMsg", "SendOrderMsg response" + recvMsg);

                        if (!string.IsNullOrEmpty(recvMsg))
                        {
                            jRet.Add(new JsonData(recvMsg));
                        }
                    }
                    break;
                case OrderState.Paid:
                    //订单微信支付时，发送模板消息给管理员
                    if (!SendTmplMsgSwitch[OrderState.Paid])
                    {
                        return false;
                    }

                    //只有微信支付成功才发送模板消息
                    if (po.PaymentTerm != PaymentTerm.WECHAT || po.TradeState != TradeState.SUCCESS)
                    {
                        return false;
                    }

                    for (int i = 0; i < listReceiver.Count; i++)
                    {
                        jTmplMsg["touser"] = listReceiver[i];
                        jTmplMsg["template_id"] = TMPL_WXPAY;
                        jTmplMsg["url"] = @"http://mahui.me/admin/ManageOrder.aspx";
                        jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = "微信支付成功";
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["first"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("￥{0}元", po.OrderPrice.ToString());
                        jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                        jTmplMsgData["keyword1"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.DeliverName;
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword2"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.TransactionID;
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword3"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = (po.TransactionTime.HasValue ? po.TransactionTime.ToString() : string.Empty);
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword4"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.OrderID;
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword5"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = "微信支付成功，请核实对账";
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["remark"] = jTmplMsgDataValue;

                        jTmplMsg["data"] = jTmplMsgData;

                        //调用微信模板消息接口，并获取返回报文
                        Log.Debug("JsApiPay", "SendWxPayMsg request" + jTmplMsg.ToJson());
                        recvMsg = HttpService.Post(jTmplMsg.ToJson(), WxTmplMsg.WxTmplMsgUrl, false, Config.WeChatAPITimeout);
                        Log.Debug("JsApiPay", "SendWxPayMsg response" + recvMsg);

                        if (!string.IsNullOrEmpty(recvMsg))
                        {
                            jRet.Add(new JsonData(recvMsg));
                        }
                    }
                    break;
                case OrderState.Delivered:
                    //订单发货事件，发送模板消息给客户
                    if (!SendTmplMsgSwitch[OrderState.Delivered])
                    {
                        return false;
                    }
                    string tradeState = string.Empty;
                    switch(po.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT:
                            if(po.TradeState == TradeState.SUCCESS)
                            {
                                tradeState = "微信支付成功";
                            }
                            else
                            {
                                tradeState = "尚未微信支付";
                            }
                            break;
                        case PaymentTerm.CASH:
                            tradeState = "货到付款";
                            break;
                    }

                    jTmplMsg["touser"] = po.Purchaser.OpenID;
                    jTmplMsg["template_id"] = TMPL_DELIVER;
                    jTmplMsg["url"] = @"http://mahui.me/";
                    jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = string.Format("您的订单{0}已配送", po.OrderID.Substring(18));
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["first"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = po.OrderDetails;
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword1"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = po.OrderDetailCount;
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword2"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = string.Format("￥{0}元【{1}】", po.OrderPrice.ToString(), tradeState);
                    jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                    jTmplMsgData["keyword3"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "小U";
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword4"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "13585702012";
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword5"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "请您注意查收。";
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["remark"] = jTmplMsgDataValue;

                    jTmplMsg["data"] = jTmplMsgData;

                    //调用微信模板消息接口，并获取返回报文
                    Log.Debug("JsApiPay", "SendWxPayMsg request" + jTmplMsg.ToJson());
                    recvMsg = HttpService.Post(jTmplMsg.ToJson(), WxTmplMsg.WxTmplMsgUrl, false, Config.WeChatAPITimeout);
                    Log.Debug("JsApiPay", "SendWxPayMsg response" + recvMsg);

                    if (!string.IsNullOrEmpty(recvMsg))
                    {
                        jRet.Add(new JsonData(recvMsg));
                    }

                    break;
                case OrderState.Accepted:
                    //订单签收事件，发送模板消息给用户
                    if (!SendTmplMsgSwitch[OrderState.Accepted])
                    {
                        return false;
                    }
                    break;
                case OrderState.Cancelled:
                    //订单撤单事件，发送模板消息给用户
                    if (!SendTmplMsgSwitch[OrderState.Cancelled])
                    {
                        return false;
                    }

                    //通知管理员和用户
                    listReceiver.Add(po.Purchaser.OpenID);

                    for (int i = 0; i < listReceiver.Count; i++)
                    {
                        jTmplMsg["touser"] = listReceiver[i];
                        jTmplMsg["template_id"] = TMPL_CANCEL;
                        jTmplMsg["url"] = @"http://mahui.me/MyOrders.aspx";
                        jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("您的订单{0}已撤销", po.OrderID.Substring(18));
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["first"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.CancelDate.HasValue ? po.CancelDate.ToString() : DateTime.Now.ToString();
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keynote1"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("￥{0}元", po.OrderPrice.ToString());
                        jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                        jTmplMsgData["keynote2"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = "查看我的订单";
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["remark"] = jTmplMsgDataValue;

                        jTmplMsg["data"] = jTmplMsgData;

                        //调用微信模板消息接口，并获取返回报文
                        Log.Debug("WxTmplMsg", "SendOrderMsg request" + jTmplMsg.ToJson());
                        recvMsg = HttpService.Post(jTmplMsg.ToJson(), WxTmplMsg.WxTmplMsgUrl, false, Config.WeChatAPITimeout);
                        Log.Debug("WxTmplMsg", "SendOrderMsg response" + recvMsg);

                        if (!string.IsNullOrEmpty(recvMsg))
                        {
                            jRet.Add(new JsonData(recvMsg));
                        }
                    }

                    break;
                default:
                    break;
            }

        }
        catch (Exception ex)
        {
            Log.Error("SendMsgOnOrderStateChanged", ex.Message);
            throw ex;
        }

        return jRet;

    }

}