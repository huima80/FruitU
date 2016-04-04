using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LitJson;

/// <summary>
/// 微信模板消息类
/// </summary>
public static class WxTmplMsg
{
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
    /// 消息头颜色
    /// </summary>
    private const string MSG_HEAD_COLOR = "#FF0000";

    /// <summary>
    ///  消息体内容颜色
    /// </summary>
    private const string MSG_BODY_COLOR = "#173177";

    /// <summary>
    /// 发送微信模板消息
    /// </summary>
    public static JsonData SendOrderMsg(List<string> receivers, ProductOrder po)
    {
        JsonData jRet = new JsonData();

        try
        {
            if (receivers != null && po != null)
            {
                receivers.ForEach(recv =>
                {
                    JsonData jTmplMsg = new JsonData(), jTmplMsgData = new JsonData(), jTmplMsgDataValue;
                    jTmplMsg["touser"] = recv;
                    jTmplMsg["template_id"] = TMPL_ORDER_SUCCESS;
                    jTmplMsg["url"] = @"http://mahui.me/admin/ManageOrder.aspx";
                    jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "订单提交成功通知";
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["first"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = po.OrderDate.ToString();
                    jTmplMsgDataValue["color"] = MSG_BODY_COLOR;
                    jTmplMsgData["keyword1"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = string.Format("￥{0}元", po.OrderPrice.ToString());
                    jTmplMsgDataValue["color"] = MSG_HEAD_COLOR;
                    jTmplMsgData["keyword2"] = jTmplMsgDataValue;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = po.ProductNames;
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
                    string recvMsg = HttpService.Post(jTmplMsg.ToJson(), WxTmplMsg.WxTmplMsgUrl, false, Config.WeChatAPITimeout);
                    Log.Debug("WxTmplMsg", "SendOrderMsg response" + recvMsg);

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

                    if (!string.IsNullOrEmpty(recvMsg))
                    {
                        jRet.Add(new JsonData(recvMsg));
                    }
                });
            }

            return jRet;
        }
        catch (Exception ex)
        {
            Log.Error("SendOrderMsg", ex.Message);
            throw ex;
        }

    }

    /// <summary>
    /// 发送微信模板消息
    /// </summary>
    public static JsonData SendWxPayMsg(List<string> receivers, ProductOrder po)
    {
        JsonData jRet = new JsonData();

        try
        {
            if (receivers != null && po != null)
            {
                receivers.ForEach(recv =>
                {
                    JsonData jTmplMsg = new JsonData(), jTmplMsgData = new JsonData(), jTmplMsgDataValue;
                    jTmplMsg["touser"] = recv;
                    jTmplMsg["template_id"] = TMPL_ORDER_SUCCESS;
                    jTmplMsg["url"] = @"http://mahui.me/admin/ManageOrder.aspx";
                    jTmplMsg["topcolor"] = MSG_HEAD_COLOR;

                    jTmplMsgDataValue = new JsonData();
                    jTmplMsgDataValue["value"] = "订单微信支付成功";
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
                    Log.Debug("JsApiPay", "SenWxPayMsg request" + jTmplMsg.ToJson());
                    string recvMsg = HttpService.Post(jTmplMsg.ToJson(), WxTmplMsg.WxTmplMsgUrl, false, Config.WeChatAPITimeout);
                    Log.Debug("JsApiPay", "SenWxPayMsg response" + recvMsg);

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

                    if (!string.IsNullOrEmpty(recvMsg))
                    {
                        jRet.Add(new JsonData(recvMsg));
                    }
                });
            }

            return jRet;
        }
        catch (Exception ex)
        {
            Log.Error("WxTmplMsg", ex.Message);
            throw ex;
        }

    }

}