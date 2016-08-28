<%@ WebHandler Language="C#" Class="alipay_notify" %>

using System;
using System.Web;
using System.Globalization;
using System.Collections.Generic;
using System.Collections.Specialized;
using Com.Alipay;

/// <summary>
/// 创建该页面文件时，请留心该页面文件中无任何HTML代码及空格。
/// 该页面不能在本机电脑测试，请到服务器上做测试。请确保外部可以访问该页面。
/// 该页面调试工具请使用写文本函数logResult。
/// 如果没有收到该页面返回的 success 信息，支付宝会在24小时内按一定的时间策略重发通知
/// 该方式的作用主要防止订单丢失，即页面跳转同步通知没有处理订单更新，它则去处理；
/// 当商户收到服务器异步通知并打印出success时，服务器异步通知参数notify_id才会失效。也就是说在支付宝发送同一条异步通知时（包含商户并未成功打印出success导致支付宝重发数次通知），服务器异步通知参数notify_id是不变的。
/// </summary>
public class alipay_notify : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {

        string replyStr = string.Empty;

        try
        {
            SortedDictionary<string, string> sdParam = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = context.Request.Form;

            // Get names of all forms into a string array.
            string[] requestItem = coll.AllKeys;

            for (int i = 0; i < requestItem.Length; i++)
            {
                sdParam.Add(requestItem[i], context.Request.Form[requestItem[i]]);
            }

            //判断是否有带返回参数
            if (sdParam.Count > 0)
            {
                if (sdParam.ContainsKey("notify_id") && sdParam.ContainsKey("sign"))
                {
                    Notify aliNotify = new Notify();
                    bool verifyResult = aliNotify.Verify(sdParam, sdParam["notify_id"], sdParam["sign"]);

                    //验证成功
                    if (verifyResult)
                    {
                        //商户订单号
                        string orderID;
                        if (sdParam.ContainsKey("out_trade_no") && !string.IsNullOrEmpty(sdParam["out_trade_no"]))
                        {
                            orderID = sdParam["out_trade_no"];
                        }
                        else
                        {
                            throw new Exception("缺少商户订单号");
                        }

                        //卖家支付宝账户号
                        string sellerId;
                        if (sdParam.ContainsKey("seller_id") && !string.IsNullOrEmpty(sdParam["seller_id"]))
                        {
                            sellerId = sdParam["seller_id"];
                        }
                        else
                        {
                            throw new Exception("缺少卖家支付宝账户号");
                        }

                        //交易金额
                        decimal totalFee;
                        if (sdParam.ContainsKey("total_fee") && !string.IsNullOrEmpty(sdParam["total_fee"]))
                        {
                            if (!decimal.TryParse(sdParam["total_fee"], out totalFee))
                            {
                                throw new Exception("支付宝交易金额错误");
                            }
                        }
                        else
                        {
                            throw new Exception("缺少支付宝交易金额");
                        }

                        //根据订单ID加载ProductOrder对象
                        ProductOrder po = new ProductOrder(orderID);

                        if (!string.IsNullOrEmpty(po.OrderID))
                        {
                            //如果此订单已处理过，则直接返回success
                            if (po.TradeState == TradeState.AP_TRADE_FINISHED || po.TradeState == TradeState.AP_TRADE_SUCCESS)
                            {
                                replyStr = "success";
                                Log.Info(this.GetType().ToString(), string.Format("订单{0}已处理过", po.OrderID));
                            }
                            else
                            {
                                //校验卖家ID
                                if (po.AP_SellerID == sellerId)
                                {
                                    //校验订单金额
                                    if (po.OrderPrice == totalFee)
                                    {
                                        //支付宝支付方式
                                        po.PaymentTerm = PaymentTerm.ALIPAY;

                                        //交易状态
                                        if (sdParam.ContainsKey("trade_status") && !string.IsNullOrEmpty(sdParam["trade_status"]))
                                        {
                                            //支付宝交易状态
                                            switch (sdParam["trade_status"].ToUpper())
                                            {
                                                case "TRADE_FINISHED":
                                                    po.TradeState = TradeState.AP_TRADE_FINISHED;
                                                    break;
                                                case "TRADE_SUCCESS":
                                                    po.TradeState = TradeState.AP_TRADE_SUCCESS;
                                                    break;
                                                case "WAIT_BUYER_PAY":
                                                    po.TradeState = TradeState.AP_WAIT_BUYER_PAY;
                                                    break;
                                                case "TRADE_CLOSED":
                                                    po.TradeState = TradeState.AP_TRADE_CLOSED;
                                                    break;
                                                case "TRADE_PENDING":
                                                    po.TradeState = TradeState.AP_TRADE_PENDING;
                                                    break;
                                                default:
                                                    throw new Exception(string.Format("未知的支付宝交易状态：{0}", sdParam["trade_status"]));
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("缺少支付宝交易状态");
                                        }

                                        //支付宝交易号
                                        if (sdParam.ContainsKey("trade_no") && !string.IsNullOrEmpty(sdParam["trade_no"]))
                                        {
                                            po.AP_TradeNo = sdParam["trade_no"];
                                        }
                                        else
                                        {
                                            throw new Exception("缺少支付宝交易号");
                                        }

                                        //卖家支付宝账号
                                        if (sdParam.ContainsKey("seller_email"))
                                        {
                                            po.AP_SellerEmail = sdParam["seller_email"];
                                        }

                                        //买家支付宝账户号
                                        if (sdParam.ContainsKey("buyer_id"))
                                        {
                                            po.AP_BuyerID = sdParam["buyer_id"];
                                        }

                                        //买家支付宝账号
                                        if (sdParam.ContainsKey("buyer_email"))
                                        {
                                            po.AP_BuyerEmail = sdParam["buyer_email"];
                                        }

                                        //通知时间
                                        DateTime notifyTime;
                                        if (sdParam.ContainsKey("notify_time") && !string.IsNullOrEmpty(sdParam["notify_time"]))
                                        {
                                            if (DateTime.TryParseExact(sdParam["notify_time"], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out notifyTime))
                                            {
                                                po.AP_Notify_Time = notifyTime;
                                            }
                                            else
                                            {
                                                po.AP_Notify_Time = null;
                                            }
                                        }
                                        else
                                        {
                                            po.AP_Notify_Time = null;
                                        }

                                        //通知类型
                                        if (sdParam.ContainsKey("notify_type"))
                                        {
                                            po.AP_Notify_Type = sdParam["notify_type"];
                                        }

                                        //交易创建时间
                                        DateTime gmtCreate;
                                        if (sdParam.ContainsKey("gmt_create") && !string.IsNullOrEmpty(sdParam["gmt_create"]))
                                        {
                                            if (DateTime.TryParseExact(sdParam["gmt_create"], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out gmtCreate))
                                            {
                                                po.AP_GMT_Create = gmtCreate;
                                            }
                                            else
                                            {
                                                po.AP_GMT_Create = null;
                                            }
                                        }
                                        else
                                        {
                                            po.AP_GMT_Create = null;
                                        }

                                        //交易付款时间
                                        DateTime gmtPayment;
                                        if (sdParam.ContainsKey("gmt_payment") && !string.IsNullOrEmpty(sdParam["gmt_payment"]))
                                        {
                                            if (DateTime.TryParseExact(sdParam["gmt_payment"], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out gmtPayment))
                                            {
                                                po.AP_GMT_Payment = gmtPayment;
                                            }
                                            else
                                            {
                                                po.AP_GMT_Payment = null;
                                            }
                                        }
                                        else
                                        {
                                            po.AP_GMT_Payment = null;
                                        }

                                        //交易关闭时间
                                        DateTime gmtClose;
                                        if (sdParam.ContainsKey("gmt_close") && !string.IsNullOrEmpty(sdParam["gmt_close"]))
                                        {
                                            if (DateTime.TryParseExact(sdParam["gmt_close"], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out gmtClose))
                                            {
                                                po.AP_GMT_Close = gmtClose;
                                            }
                                            else
                                            {
                                                po.AP_GMT_Close = null;
                                            }
                                        }
                                        else
                                        {
                                            po.AP_GMT_Close = null;
                                        }

                                        //退款状态
                                        if (sdParam.ContainsKey("refund_status") && !string.IsNullOrEmpty(sdParam["refund_status"]))
                                        {
                                            switch (sdParam["refund_status"].ToUpper())
                                            {
                                                case "REFUND_SUCCESS":
                                                    po.AP_RefundStatus = RefundStatus.AP_REFUND_SUCCESS;
                                                    break;
                                                case "REFUND_CLOSED":
                                                    po.AP_RefundStatus = RefundStatus.AP_REFUND_CLOSED;
                                                    break;
                                                default:
                                                    po.AP_RefundStatus = null;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            po.AP_RefundStatus = null;
                                        }

                                        //卖家退款时间
                                        DateTime gmtRefund;
                                        if (sdParam.ContainsKey("gmt_refund") && !string.IsNullOrEmpty(sdParam["gmt_refund"]))
                                        {
                                            if (DateTime.TryParseExact(sdParam["gmt_refund"], "yyyy-MM-dd HH:mm:ss", null, DateTimeStyles.None, out gmtRefund))
                                            {
                                                po.AP_GMT_Refund = gmtRefund;
                                            }
                                            else
                                            {
                                                po.AP_GMT_Refund = null;
                                            }
                                        }
                                        else
                                        {
                                            po.AP_GMT_Refund = null;
                                        }

                                        //注册订单的支付宝支付状态变动事件处理函数，通知管理员
                                        po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);

                                        //注册订单的支付宝支付状态变动事件处理函数，核销微信卡券
                                        po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxCard.ConsumeCodeOnOrderStateChanged);

                                        //更新订单的支付宝支付状态
                                        ProductOrder.UpdateTradeState(po);

                                        //程序执行完后必须打印输出“success”（不包含引号）。如果商户反馈给支付宝的字符不是success这7个字符，支付宝服务器会不断重发通知，直到超过24小时22分钟。
                                        replyStr = "success";
                                    }
                                    else
                                    {
                                        throw new Exception(string.Format("支付宝返回的订单总金额{0}元与原订单金额{1}元不符", totalFee, po.OrderPrice));
                                    }
                                }
                                else
                                {
                                    throw new Exception(string.Format("支付宝返回的卖家ID({0})与原订单({1})不符", sellerId, po.AP_SellerID));
                                }
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("没有找到支付宝返回的订单：{0}", orderID));
                        }
                    }
                    else
                    {
                        throw new Exception("支付宝通知验证失败");
                    }
                }
                else
                {
                    throw new Exception("无支付宝notify_id和sign");
                }
            }
            else
            {
                throw new Exception("无支付宝通知返回参数");
            }
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), string.Format("支付宝通知出错 :{0}\n请求方IP：{1}", ex.Message + ex.StackTrace, context.Request.UserHostAddress));
            replyStr = "fail";
        }
        finally
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write(replyStr);
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