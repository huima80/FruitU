<%@ WebHandler Language="C#" Class="PayResultHandler" %>
#undef DEBUG
using System;
using System.Web;
using System.Text;

/// <summary>
/// 微信支付结果通用通知，参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_7
/// 对后台通知交互时，如果微信收到商户的应答不是成功或超时，微信认为通知失败，微信会通过一定的策略定期重新发起通知，尽可能提高通知的成功率，但微信不保证通知最终能成功。 （通知频率为15/15/30/180/1800/1800/1800/1800/3600，单位：秒）
/// </summary>
public class PayResultHandler : IHttpHandler {

    public void ProcessRequest (HttpContext context) {

        Log.Info(this.GetType().ToString(), "微信支付通知回调开始...");

        //接收微信支付的通知消息
        WeChatPayData notifyPayData = new WeChatPayData();

        //向微信支付返回消息
        WeChatPayData respPayData = new WeChatPayData();

        try
        {
            StringBuilder notifyMsg = new StringBuilder();

#if DEBUG
            notifyMsg = new StringBuilder(@"<xml><appid><![CDATA[wxa1ce5cd8d4bc5b3f]]></appid>" +
                "<attach><![CDATA[订单自定义数据]]></attach>" +
                "<bank_type><![CDATA[CMB_DEBIT]]></bank_type>" +
                "<cash_fee><![CDATA[1]]></cash_fee>" +
                "<device_info><![CDATA[WEB]]></device_info>" +
                "<fee_type><![CDATA[CNY]]></fee_type>" +
                "<is_subscribe><![CDATA[Y]]></is_subscribe>" +
                "<mch_id><![CDATA[1296478401]]></mch_id>" +
                "<nonce_str><![CDATA[f6e4a49e7e884a7ba598f99f26556c47]]></nonce_str>" +
                "<openid><![CDATA[o5gbrsixFkd1G6eszfG5mN-WbMeE]]></openid>" +
                "<out_trade_no><![CDATA[129647840120151229152257279]]></out_trade_no>" +
                "<result_code><![CDATA[SUCCESS]]></result_code>" +
                "<return_code><![CDATA[SUCCESS]]></return_code>" +
                "<sign><![CDATA[580674F3EF50CEE63BDC925CC980B891]]></sign>" +
                "<time_end><![CDATA[20151230001313]]></time_end>" +
                "<total_fee>1</total_fee>" +
                "<trade_type><![CDATA[JSAPI]]></trade_type>" +
                "<transaction_id><![CDATA[1007390169201512302392096459]]></transaction_id></xml>");

#else
            //接收微信支付结果通知数据
            System.IO.Stream s = context.Request.InputStream;
            int count = 0;
            byte[] buffer = new byte[1024];
            while ((count = s.Read(buffer, 0, 1024)) > 0)
            {
                notifyMsg.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
            s.Flush();
            s.Close();
            s.Dispose();

#endif       

            Log.Info(this.GetType().ToString(), "微信支付回调通知内容：" + notifyMsg.ToString());

            //把微信发送的XML支付通知消息转换成对象
            notifyPayData.FromXml(notifyMsg.ToString());

            //校验返回状态码
            if (notifyPayData.GetValue("return_code").ToString() != "SUCCESS")
            {
                throw new Exception(notifyPayData.GetValue("return_msg").ToString());
            }

            //校验业务结果
            if (notifyPayData.GetValue("result_code").ToString() == "SUCCESS")
            {

                //当return_code为SUCCESS，才有sign参数返回
                if (!notifyPayData.CheckSign())
                {
                    throw new Exception("sign签名校验错误");
                }

                string orderID = notifyPayData.GetValue("out_trade_no").ToString();

                //根据订单OrderID再向微信发起支付订单查询，校验订单实际支付状态
                WeChatPayData queryOrderPayData;
                queryOrderPayData = WxPayAPI.OrderQueryByOutTradeNo(orderID);

                Log.Info(this.GetType().ToString(), "向微信查询订单" + orderID + "的实际支付状态：" + queryOrderPayData.GetValue("trade_state").ToString().ToUpper());

                //根据查询返回的trade_state设置数据库中用户订单的微信支付实际状态
                switch (queryOrderPayData.GetValue("trade_state").ToString().ToUpper())
                {
                    case "SUCCESS": //支付成功
                        ProductOrder.UpdateTradeState(orderID,
                                TradeState.SUCCESS,
                                null,
                                queryOrderPayData.GetValue("transaction_id").ToString(),
                                DateTime.ParseExact(queryOrderPayData.GetValue("time_end").ToString(),
                                    "yyyyMMddHHmmss",
                                    System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case "REFUND":  //转入退款
                        ProductOrder.UpdateTradeState(orderID,
                                TradeState.REFUND,
                                queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                queryOrderPayData.GetValue("transaction_id").ToString(),
                                DateTime.ParseExact(queryOrderPayData.GetValue("time_end").ToString(),
                                    "yyyyMMddHHmmss",
                                    System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    case "NOTPAY":  //TODO:未支付状态，就没有交易时间time_end
                        ProductOrder.UpdateTradeState(orderID,
                                TradeState.NOTPAY,
                                queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                null,
                                null);
                        break;
                    case "CLOSED":  //已关闭
                        ProductOrder.UpdateTradeState(orderID,
                                TradeState.CLOSED,
                                queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                null,
                                null);
                        break;
                    case "REVOKED": //已撤销（刷卡支付）
                        ProductOrder.UpdateTradeState(orderID,
                                TradeState.REVOKED,
                                queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                null,
                                null);
                        break;
                    case "USERPAYING":  //用户支付中
                        ProductOrder.UpdateTradeState(orderID,
                                TradeState.USERPAYING,
                                queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                null,
                                null);
                        break;
                    case "PAYERROR":    //支付失败(其他原因，如银行返回失败)
                        ProductOrder.UpdateTradeState(orderID,
                                TradeState.PAYERROR,
                                queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                null,
                                null);
                        break;
                    default:    //未知状态
                        break;
                }

                //微信支付通知消息中的return_code、result_code都是SUCCESS，且存在transaction_id订单，则向微信支付返回接收通知成功
                respPayData.SetValue("return_code", "SUCCESS");
                respPayData.SetValue("return_msg", "OK");
                Log.Info(this.GetType().ToString(), "向微信支付通知返回成功: " + respPayData.ToXml());
            }
            else
            {
                ProductOrder.UpdateTradeState(notifyPayData.GetValue("out_trade_no").ToString(),
                    TradeState.NOTPAY,
                    notifyPayData.GetValue("err_code_des").ToString(),
                    null,
                    null);

                throw new Exception(notifyPayData.GetValue("err_code").ToString() + ":" + notifyPayData.GetValue("err_code_des").ToString());

            }
        }
        catch (Exception ex)
        {
            //发生异常，也反馈给微信通知接口
            respPayData.SetValue("return_code", "FAIL");
            respPayData.SetValue("return_msg", ex.Message);
            Log.Error(this.GetType().ToString(), "微信支付通知出错 : " + ex.Message + ex.StackTrace);
        }

        //向微信通知接口同步返回消息
        context.Response.Clear();
        context.Response.ContentType = "text/xml";
        context.Response.Write(respPayData.ToXml());
        context.Response.End();

    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}