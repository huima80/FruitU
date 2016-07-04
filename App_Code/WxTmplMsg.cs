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
    private const string WxTmplMsgUrl = @"https://api.weixin.qq.com/cgi-bin/message/template/send?access_token=";

    /// <summary>
    /// 订单提交成功通知模板
    /// {{first.DATA}}
    /// 下单时间：{{keyword1.DATA}}
    /// 订单总金额：{{keyword2.DATA}}
    /// 订单详情：{{keyword3.DATA}}
    /// 详细地址：{{keyword4.DATA}}
    /// {{remark.DATA}}
    /// </summary>
    private const string TMPL_ORDER_SUCCESS = "zYBc-cvsf_7cllTNbXE_Y-55jU4owKAhqfEHAxVhDbU";

    /// <summary>
    /// 订单支付成功通知
    //{{first.DATA}}
    //订单编号：{{keyword1.DATA}}
    //订单商品：{{keyword2.DATA}}
    //支付金额：{{keyword3.DATA}}
    //支付时间：{{keyword4.DATA}}
    //{{remark.DATA}} 
    /// </summary>
    private const string TMPL_PAY_SUCCESS = "pSqI0iYqqh8BRuEt40JtIbQxSgWV_l6sDCxdorH3KPo";

    /// <summary>
    /// 发货通知
    /// {{first.DATA}}
    /// 品名：{{keyword1.DATA}}
    /// 数量：{{keyword2.DATA}}
    /// 金额：{{keyword3.DATA}}
    /// 配送员姓名：{{keyword4.DATA}}
    /// 联系电话：{{keyword5.DATA}}
    /// {{remark.DATA}}    
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
    /// 商品库存量提醒
    /// {{first.DATA}}
    /// 商品编码：{{keyword1.DATA}}
    /// 商品名称：{{keyword2.DATA}}
    /// 现有库存量：{{keyword3.DATA}}
    /// {{remark.DATA}} 
    /// </summary>
    private const string TMPL_INVENTORY_WARN = "u4JPyu1tlv_JUlM4U7hN0__iUT8NzswQMCvf1DEU7pM";

    /// <summary>
    /// 积分通知
    //{{first.DATA}}
    //会员姓名：{{keyword1.DATA}}
    //会员账号：{{keyword2.DATA}}
    //积分变更：{{keyword3.DATA}}
    //剩余积分：{{keyword4.DATA}}
    //{{remark.DATA}} 
    /// </summary>
    private const string TMPL_MEMBER_POINTS_NOTIFY = "iSX4_ghQqpXxS3GxtbQWdDC7wE5V5D_BiBJR13ICEso";

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
    /// 模板消息接收者，1下单通知管理员；2微信支付成功通知管理员；3配送通知用户；4签收不通知；5撤单通知管理员和用户
    /// </summary>
    /// <param name="po"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public static JsonData SendMsgOnOrderStateChanged(ProductOrder po, ProductOrder.OrderStateEventArgs e)
    {
        if (po == null || e == null)
        {
            throw new ArgumentNullException("sender或事件参数对象不能为null");
        }
        JsonData jRet = new JsonData();

        try
        {
            List<string> listReceiver;

            JsonData jTmplMsg = new JsonData(), jTmplMsgData = new JsonData(), jTmplMsgDataValue;

            //根据不同的订单状态事件，发送不同的微信模板消息
            switch (e.OrderState)
            {
                case OrderState.Submitted:
                    //订单下单事件，发送模板消息给管理员
                    if (!SendTmplMsgSwitch[OrderState.Submitted])
                    {
                        return false;
                    }

                    //通知管理员
                    listReceiver = new List<string>(Config.WxTmplMsgReceiver.ToArray());

                    string paymentTerm = string.Empty;
                    switch (po.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT:
                            paymentTerm = "微信支付";
                            break;
                        case PaymentTerm.ALIPAY:
                            paymentTerm = "支付宝";
                            break;
                        case PaymentTerm.CASH:
                            paymentTerm = "货到付款";
                            break;
                    }

                    //构造模板消息
                    jTmplMsg["touser"] = string.Empty;
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
                    jTmplMsgDataValue["value"] = string.Format("{0}元【{1}】", po.OrderPrice.ToString("C"), paymentTerm);
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

                    //发送模板消息，通知管理员
                    jRet = SendTmplMsg(listReceiver, jTmplMsg);

                    //发送模板消息，通知用户
                    listReceiver = new List<string>(new string[] { po.Purchaser.OpenID });
                    jTmplMsg["url"] = @"http://mahui.me/MyOrders.aspx";
                    jRet.Add(SendTmplMsg(listReceiver, jTmplMsg));

                    break;
                case OrderState.Paid:
                    //订单支付事件，发送模板消息给管理员
                    if (!SendTmplMsgSwitch[OrderState.Paid])
                    {
                        return false;
                    }

                    //只有收款成功才发送模板消息
                    if ((po.PaymentTerm == PaymentTerm.WECHAT && po.TradeState == TradeState.SUCCESS)
                        || (po.PaymentTerm == PaymentTerm.ALIPAY && (po.TradeState == TradeState.AP_TRADE_SUCCESS || po.TradeState == TradeState.AP_TRADE_FINISHED))
                        || (po.PaymentTerm == PaymentTerm.CASH && po.TradeState == TradeState.CASHPAID))
                    {
                        //通知管理员
                        listReceiver = new List<string>(Config.WxTmplMsgReceiver.ToArray());

                        //构造模板消息
                        jTmplMsg["touser"] = string.Empty;
                        jTmplMsg["template_id"] = TMPL_PAY_SUCCESS;
                        jTmplMsg["url"] = @"http://mahui.me/admin/ManageOrder.aspx";
                        jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                        switch (po.PaymentTerm)
                        {
                            case PaymentTerm.WECHAT:
                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = "【微信】支付成功";
                                jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                                jTmplMsgData["first"] = jTmplMsgDataValue;

                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = string.Format("微信支付交易号：{0}", po.TransactionID);
                                jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                                jTmplMsgData["remark"] = jTmplMsgDataValue;

                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = (po.TransactionTime.HasValue ? po.TransactionTime.ToString() : string.Empty);
                                jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                                jTmplMsgData["keyword4"] = jTmplMsgDataValue;

                                break;
                            case PaymentTerm.ALIPAY:
                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = "【支付宝】支付成功";
                                jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                                jTmplMsgData["first"] = jTmplMsgDataValue;

                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = string.Format("支付宝交易号：{0}", po.AP_TradeNo);
                                jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                                jTmplMsgData["remark"] = jTmplMsgDataValue;

                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = (po.AP_GMT_Payment.HasValue ? po.AP_GMT_Payment.ToString() : string.Empty);
                                jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                                jTmplMsgData["keyword4"] = jTmplMsgDataValue;

                                break;
                            case PaymentTerm.CASH:
                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = "【货到付款】收款成功";
                                jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                                jTmplMsgData["first"] = jTmplMsgDataValue;

                                jTmplMsgDataValue = new JsonData();
                                jTmplMsgDataValue["value"] = (po.PayCashDate.HasValue ? po.PayCashDate.ToString() : string.Empty);
                                jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                                jTmplMsgData["keyword4"] = jTmplMsgDataValue;

                                break;

                        }
                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.OrderID.Substring(18);
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword1"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = po.OrderDetails;
                        jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                        jTmplMsgData["keyword2"] = jTmplMsgDataValue;

                        jTmplMsgDataValue = new JsonData();
                        jTmplMsgDataValue["value"] = string.Format("{0}元", po.OrderPrice.ToString("C"));
                        jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                        jTmplMsgData["keyword3"] = jTmplMsgDataValue;

                        jTmplMsg["data"] = jTmplMsgData;

                        //发送模板消息，通知管理员
                        jRet = SendTmplMsg(listReceiver, jTmplMsg);

                        //发送模板消息，通知用户
                        listReceiver = new List<string>(new string[] { po.Purchaser.OpenID });
                        jTmplMsg["url"] = @"http://mahui.me/MyOrders.aspx";
                        jRet.Add(SendTmplMsg(listReceiver, jTmplMsg));

                    }

                    break;
                case OrderState.Delivered:
                    //订单发货事件，发送模板消息给用户
                    if (!SendTmplMsgSwitch[OrderState.Delivered])
                    {
                        return false;
                    }

                    //发货消息只通知用户本人
                    listReceiver = new List<string>(new string[] { po.Purchaser.OpenID });

                    string tradeState = string.Empty;
                    switch (po.PaymentTerm)
                    {
                        case PaymentTerm.WECHAT:
                            if (po.TradeState == TradeState.SUCCESS)
                            {
                                tradeState = "微信支付成功";
                            }
                            else
                            {
                                tradeState = "未支付";
                            }
                            break;
                        case PaymentTerm.ALIPAY:
                            if (po.TradeState == TradeState.AP_TRADE_SUCCESS || po.TradeState == TradeState.AP_TRADE_FINISHED)
                            {
                                tradeState = "支付宝支付成功";
                            }
                            else
                            {
                                tradeState = "未支付";
                            }
                            break;
                        case PaymentTerm.CASH:
                            tradeState = "货到付款";
                            break;
                    }

                    //构造模板消息
                    jTmplMsg["touser"] = string.Empty;
                    jTmplMsg["template_id"] = TMPL_DELIVER;
                    jTmplMsg["url"] = @"http://mahui.me/MyOrders.aspx";
                    jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = string.Format("您的订单{0}已发货，请注意查收。", po.OrderID.Substring(18));
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
                    jTmplMsgDataValue["value"] = string.Format("{0}元【{1}】", po.OrderPrice.ToString("C"), tradeState);
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

                    jTmplMsg["data"] = jTmplMsgData;

                    //发送模板消息
                    jRet = SendTmplMsg(listReceiver, jTmplMsg);

                    break;
                case OrderState.Accepted:
                    //订单签收事件，发送模板消息给用户
                    if (!SendTmplMsgSwitch[OrderState.Accepted])
                    {
                        return false;
                    }

                    break;
                case OrderState.Cancelled:
                    //订单撤单事件，发送模板消息
                    if (!SendTmplMsgSwitch[OrderState.Cancelled])
                    {
                        return false;
                    }

                    //通知管理员
                    listReceiver = new List<string>(Config.WxTmplMsgReceiver.ToArray());

                    jTmplMsg["touser"] = string.Empty;
                    jTmplMsg["template_id"] = TMPL_CANCEL;
                    jTmplMsg["url"] = @"http://mahui.me/admin/ManageOrder.aspx";
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
                    jTmplMsgDataValue["value"] = string.Format("{0}元", po.OrderPrice.ToString("C"));
                    jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                    jTmplMsgData["keynote2"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "查看我的订单";
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["remark"] = jTmplMsgDataValue;

                    jTmplMsg["data"] = jTmplMsgData;

                    //发送模板消息，通知管理员
                    jRet = SendTmplMsg(listReceiver, jTmplMsg);

                    //发送模板消息，通知用户
                    listReceiver = new List<string>(new string[] { po.Purchaser.OpenID });
                    jTmplMsg["url"] = @"http://mahui.me/MyOrders.aspx";
                    jRet.Add(SendTmplMsg(listReceiver, jTmplMsg));

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

    /// <summary>
    /// 商品库存量报警事件处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public static void SendMsgOnInventoryWarn(object sender, EventArgs e)
    {
        if (sender == null || !(sender is OrderDetail) || e == null)
        {
            throw new ArgumentNullException("sender或事件参数对象不能为null");
        }

        JsonData jRet;
        OrderDetail od = sender as OrderDetail;

        try
        {
            List<string> listReceiver;
            listReceiver = new List<string>(Config.WxTmplMsgReceiver.ToArray());

            JsonData jTmplMsg = new JsonData(), jTmplMsgData = new JsonData(), jTmplMsgDataValue;
            int restInventory = od.InventoryQty - od.PurchaseQty;
            restInventory = restInventory >= 0 ? restInventory : 0;

            //构造模板消息
            jTmplMsg["touser"] = string.Empty;
            jTmplMsg["template_id"] = TMPL_INVENTORY_WARN;
            jTmplMsg["url"] = @"http://mahui.me/admin/ManageProduct.aspx";
            jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = "商品库存量不足，请尽快补货";
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["first"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = od.ProductID;
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["keyword1"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = od.OrderProductName;
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["keyword2"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = restInventory;
            jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
            jTmplMsgData["keyword3"] = jTmplMsgDataValue;

            jTmplMsg["data"] = jTmplMsgData;

            //发送模板消息
            jRet = SendTmplMsg(listReceiver, jTmplMsg);
        }
        catch (Exception ex)
        {
            Log.Error("SendMsgOnInventoryWarn", ex.Message);
            throw ex;
        }

        if (jRet != null)
        {
            Log.Info("SendMsgOnInventoryWarn", jRet.ToJson());
        }
    }

    public static void SendMsgOnMemberPoints(object sender, ProductOrder.MemberPointsCalculatedEventArgs e)
    {
        if (sender == null || !(sender is ProductOrder) || e == null)
        {
            throw new ArgumentNullException("sender或事件参数对象不能为null");
        }

        ProductOrder po = sender as ProductOrder;

        JsonData jRet;

        try
        {
            List<string> listReceiver;
            JsonData jTmplMsg, jTmplMsgData, jTmplMsgDataValue;

            //给订单的下单人发送积分通知消息
            listReceiver = new List<string>(new string[] { po.Purchaser.OpenID });
            //构造模板消息
            jTmplMsg = new JsonData();
            jTmplMsgData = new JsonData();

            jTmplMsg["touser"] = string.Empty;
            jTmplMsg["template_id"] = TMPL_MEMBER_POINTS_NOTIFY;
            jTmplMsg["url"] = @"http://mahui.me/Index.aspx";
            jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = "您的FruitU会员积分信息变更如下";
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["first"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = po.Purchaser.NickName;
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["keyword1"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = "微信用户";
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["keyword2"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = string.Format("本次消费增加{0}积分，使用{1}积分", e.increasedMemberPoints, e.usedMemberPoints);
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["keyword3"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = string.Format("{0}积分（={1}元）", e.newMemberPoints, (decimal)e.newMemberPoints / Config.MemberPointsExchangeRate);
            jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
            jTmplMsgData["keyword4"] = jTmplMsgDataValue;

            jTmplMsgDataValue = new JsonData();
            jTmplMsgDataValue["value"] = "在FruitU页面中分享给好友或朋友圈，好友消费后您有100积分(5元)奖励哦，现在就分享吧！";
            jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
            jTmplMsgData["remark"] = jTmplMsgDataValue;

            jTmplMsg["data"] = jTmplMsgData;

            //发送模板消息
            jRet = SendTmplMsg(listReceiver, jTmplMsg);


            //如果此订单有推荐人，且推荐人获得了积分奖励，则也给推荐人发送积分奖励消息
            if (po.Agent != null && !string.IsNullOrEmpty(po.Agent.OpenID) && po.Purchaser.OpenID != po.Agent.OpenID && e.agentNewMemberPoints != -1)
            {
                WeChatUser wxAgent = WeChatUserDAO.FindUserByOpenID(po.Agent.OpenID);
                if (wxAgent != null)
                {
                    listReceiver = new List<string>(new string[] { po.Agent.OpenID });

                    //构造模板消息
                    jTmplMsg = new JsonData();
                    jTmplMsgData = new JsonData();

                    jTmplMsg["touser"] = string.Empty;
                    jTmplMsg["template_id"] = TMPL_MEMBER_POINTS_NOTIFY;
                    jTmplMsg["url"] = @"http://mahui.me/Index.aspx";
                    jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = string.Format("您推荐好友“{0}”消费，获得了积分奖励哦！", po.Purchaser.NickName);
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["first"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = wxAgent.NickName;
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword1"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "微信用户";
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword2"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = string.Format("本次推荐奖励{0}积分", 100);
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword3"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = string.Format("{0}积分（={1}元）", e.agentNewMemberPoints, (decimal)e.agentNewMemberPoints / Config.MemberPointsExchangeRate);
                    jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                    jTmplMsgData["keyword4"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "如有疑问，请及时与FruitU微信客服联系";
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["remark"] = jTmplMsgDataValue;

                    jTmplMsg["data"] = jTmplMsgData;

                    //发送模板消息
                    jRet = SendTmplMsg(listReceiver, jTmplMsg);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("SendMsgOnMemberPoints", ex.Message);
            throw ex;
        }

        if (jRet != null)
        {
            Log.Info("SendMsgOnMemberPoints", jRet.ToJson());
        }

    }


    /// <summary>
    /// 发送模板消息
    /// </summary>
    /// <param name="listReceiver">消息接收者</param>
    /// <param name="jTmplMsg">模板消息体</param>
    /// <returns>微信模板消息返回值</returns>
    public static JsonData SendTmplMsg(List<string> listReceiver, JsonData jTmplMsg)
    {
        string recvMsg;
        bool isSuccess = false;
        JsonData jRet = new JsonData(), jRecv;
        int tryTimes = 0;

        listReceiver.ForEach(receiver =>
        {
            jTmplMsg["touser"] = receiver;

            do
            {
                //调用微信模板消息接口，并获取返回报文
                Log.Debug("WxTmplMsg", "SendOrderMsg request" + jTmplMsg.ToJson());
                recvMsg = HttpService.Post(jTmplMsg.ToJson(), WxTmplMsg.WxTmplMsgUrl + WxJSAPI.GetAccessToken(), false, Config.WeChatAPITimeout);
                Log.Debug("WxTmplMsg", "SendOrderMsg response" + recvMsg);

                if (!string.IsNullOrEmpty(recvMsg))
                {
                    jRecv = JsonMapper.ToObject(recvMsg);
                    if (jRecv.Keys.Contains("errcode") && jRecv["errcode"] != null)
                    {
                        switch (jRecv["errcode"].ToString())
                        {
                            case "0":
                                isSuccess = true;
                                break;
                            case "40001":
                            case "40002":
                                isSuccess = false;
                                WxJSAPI.RefreshAccessToken();
                                break;
                            default:
                                isSuccess = false;
                                break;
                        }
                    }

                    jRet.Add(jRecv);
                }

                tryTimes++;

            } while (!isSuccess && tryTimes < 10);
        });

        return jRet;
    }

}