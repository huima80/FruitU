<%@ WebHandler Language="C#" Class="PayResultHandler" %>
#undef DEBUG
using System;
using System.Web;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// 微信支付结果通用通知，参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_7
/// 阶段1：接收微信的“支付结果通知”回调，对后台通知交互时，如果微信收到商户的应答不是成功或超时，微信认为通知失败，微信会通过一定的策略定期重新发起通知，尽可能提高通知的成功率，但微信不保证通知最终能成功。 （通知频率为15/15/30/180/1800/1800/1800/1800/3600，单位：秒）
/// 阶段2：主动调用微信的“查询订单”接口，查询此订单的实际支付状态，并更新数据库
/// </summary>
public class PayResultHandler : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {

        Log.Info(this.GetType().ToString(), "微信支付通知回调开始...请求方IP：" + context.Request.UserHostAddress);

        //微信的“支付结果通知”回调消息
        WeChatPayData notifyPayData = new WeChatPayData();

        //向微信的“支付结果通知”返回消息
        WeChatPayData replyPayData = new WeChatPayData();

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
            //阶段1：获取微信的“支付结果通知”回调
            using (System.IO.Stream s = context.Request.InputStream)
            {
                int count = 0;
                byte[] buffer = new byte[1024];
                while ((count = s.Read(buffer, 0, 1024)) > 0)
                {
                    notifyMsg.Append(Encoding.UTF8.GetString(buffer, 0, count));
                }
                s.Flush();
                s.Close();
                s.Dispose();
            }

#endif

            Log.Info(this.GetType().ToString(), "微信支付回调通知内容：" + notifyMsg.ToString());

            //把微信发送的XML支付通知消息转换成SortedDictionary集合
            notifyPayData.FromXml(notifyMsg.ToString());

            if (!notifyPayData.IsSet("return_code"))
            {
                throw new Exception("本接口用于接收微信“支付结果通知”回调");
            }

            //校验返回状态码
            if (notifyPayData.GetValue("return_code").ToString().ToUpper() == "SUCCESS")
            {
                //当return_code为SUCCESS，才有sign参数返回
                if (!notifyPayData.CheckSign())
                {
                    throw new Exception("微信“支付结果通知”消息sign签名校验错误");
                }

                //校验业务结果
                if (notifyPayData.GetValue("result_code").ToString().ToUpper() == "SUCCESS")
                {
                    string orderID;
                    if (notifyPayData.IsSet("out_trade_no"))
                    {
                        //获取微信支付通知的订单ID，用于阶段2的查询
                        orderID = notifyPayData.GetValue("out_trade_no").ToString();
                        if (!string.IsNullOrEmpty(orderID))
                        {
                            //根据微信支付“查询订单”接口的结果构造ProductOrder对象
                            ProductOrder po = new ProductOrder(orderID);

                            //如果此订单已经通知处理过，则直接向微信返回SUCCESS
                            if (po.TradeState == TradeState.SUCCESS && !string.IsNullOrEmpty(po.TransactionID))
                            {
                                replyPayData.SetValue("return_code", "SUCCESS");
                                replyPayData.SetValue("return_msg", "OK");
                                Log.Info(this.GetType().ToString(), string.Format("订单{0}已处理过", orderID));
                            }
                            else
                            {
                                //如果此订单没有处理过，则调用微信“查询订单”接口，获取订单实际支付状态
                                WeChatPayData queryOrderPayData;
                                queryOrderPayData = WxPayAPI.OrderQueryByOutTradeNo(orderID);

                                if (queryOrderPayData != null)
                                {
                                    Log.Info(this.GetType().ToString(), "向微信查询订单实际支付状态：" + queryOrderPayData.ToXml());

                                    //校验微信支付返回的OpenID与原订单OpenID是否相符
                                    if (queryOrderPayData.IsSet("openid"))
                                    {
                                        if (po.Purchaser.OpenID != queryOrderPayData.GetValue("openid").ToString())
                                        {
                                            throw new Exception(string.Format("微信支付返回的OpenID：{0}与原订单OpenID：{1}不符", queryOrderPayData.GetValue("openid").ToString(), po.Purchaser.OpenID));
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("微信支付没有返回OpenID");
                                    }

                                    //校验微信支付返回的总金额与原订单金额是否相符
                                    if (queryOrderPayData.IsSet("total_fee"))
                                    {
                                        decimal totalFee;
                                        if (decimal.TryParse(queryOrderPayData.GetValue("total_fee").ToString(), out totalFee))
                                        {
                                            totalFee = totalFee / 100;
                                            if (po.OrderPrice != totalFee)
                                            {
                                                throw new Exception(string.Format("微信支付返回的订单总金额{0}元与原订单金额{1}元不符", totalFee, po.OrderPrice));
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("微信支付返回的订单总金额格式转换错误total_fee");
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("微信支付没有返回订单总金额total_fee");
                                    }

                                    //获取返回的微信支付状态
                                    if (queryOrderPayData.IsSet("trade_state"))
                                    {
                                        switch (queryOrderPayData.GetValue("trade_state").ToString().ToUpper())
                                        {
                                            case "SUCCESS": //支付成功
                                                po.TradeState = TradeState.SUCCESS;
                                                break;
                                            case "REFUND":  //转入退款
                                                po.TradeState = TradeState.REFUND;
                                                break;
                                            case "NOTPAY":  //未支付状态，就没有交易时间time_end
                                                po.TradeState = TradeState.NOTPAY;
                                                break;
                                            case "CLOSED":  //已关闭
                                                po.TradeState = TradeState.CLOSED;
                                                break;
                                            case "REVOKED": //已撤销（刷卡支付）
                                                po.TradeState = TradeState.REVOKED;
                                                break;
                                            case "USERPAYING":  //用户支付中
                                                po.TradeState = TradeState.USERPAYING;
                                                break;
                                            case "PAYERROR":    //支付失败(其他原因，如银行返回失败)
                                                po.TradeState = TradeState.PAYERROR;
                                                break;
                                            default:    //未知状态，默认为未支付
                                                po.TradeState = TradeState.NOTPAY;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception("微信支付没有返回支付状态trade_state");
                                    }

                                    //获取微信支付交易流水号、交易时间、交易描述
                                    po.TransactionID = queryOrderPayData.IsSet("transaction_id") ? queryOrderPayData.GetValue("transaction_id").ToString() : string.Empty;
                                    po.TransactionTime = queryOrderPayData.IsSet("time_end") ? (DateTime?)DateTime.ParseExact(queryOrderPayData.GetValue("time_end").ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) : null;
                                    po.TradeStateDesc = queryOrderPayData.IsSet("trade_state_desc") ? queryOrderPayData.GetValue("trade_state_desc").ToString() : string.Empty;

                                    //更新为微信支付方式
                                    po.PaymentTerm = PaymentTerm.WECHAT;

                                    //注册订单的微信支付状态变动事件处理函数，通知管理员
                                    po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);

                                    //注册订单的微信支付状态变动事件处理函数，核销微信卡券
                                    po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxCard.ConsumeCodeOnOrderStateChanged);

                                    //注册订单的微信支付状态变动事件处理函数，检测订单中的商品团购活动是否成功
                                    po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(GroupPurchaseEvent.GroupPurchaseEventSuccessHandler);

                                    //更新订单的微信支付状态
                                    ProductOrder.UpdateTradeState(po);

                                    //微信支付通知消息中的return_code、result_code都是SUCCESS，且存在transaction_id订单，则向微信支付返回接收通知成功
                                    replyPayData.SetValue("return_code", "SUCCESS");
                                    replyPayData.SetValue("return_msg", "OK");
                                    Log.Info(this.GetType().ToString(), "向微信支付通知返回成功: " + replyPayData.ToXml());
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("微信支付通知参数out_trade_no为空");
                        }
                    }
                    else
                    {
                        throw new Exception("微信支付通知缺少参数out_trade_no");
                    }
                }
                else
                {
                    if (notifyPayData.IsSet("err_code") && notifyPayData.IsSet("err_code_des"))
                    {
                        throw new Exception(notifyPayData.GetValue("err_code").ToString() + ":" + notifyPayData.GetValue("err_code_des").ToString());
                    }
                }
            }
            else
            {
                if (notifyPayData.IsSet("return_msg"))
                {
                    throw new Exception(notifyPayData.GetValue("return_msg").ToString());
                }
            }
        }
        catch (Exception ex)
        {
            //发生异常，也反馈给微信通知接口
            replyPayData.SetValue("return_code", "FAIL");
            replyPayData.SetValue("return_msg", ex.Message);
            Log.Error(this.GetType().ToString(), string.Format("微信支付通知出错 :{0}\n请求方IP：{1}", ex.Message + ex.StackTrace, context.Request.UserHostAddress));
        }

        //向微信通知接口同步返回消息
        context.Response.Clear();
        context.Response.ContentType = "text/xml";
        context.Response.Write(replyPayData.ToXml());
        context.Response.End();

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}