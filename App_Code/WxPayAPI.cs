using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Runtime.Serialization;
using System.IO;
using System.Text;
using System.Net;
using System.Web.Security;
using LitJson;


/// <summary>
/// 微信支付API接口调用公共类
/// </summary>
public class WxPayAPI
{
    /// <summary>
    /// 微信支付统一下单，接口参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_1
    /// 除被扫支付场景以外，商户系统先调用该接口在微信支付服务后台生成预支付交易单，返回正确的预支付交易回话标识prepay_id，有效期2小时，再按扫码、JSAPI、APP等不同场景生成交易串调起支付。
    /// </summary>
    /// <param name="po"></param>
    /// <param name="jStateCode"></param>
    /// <returns>正常返回prepay_id，jStateCode为空；如果有错误发生，则prepay_id为空，jStateCode有值</returns>
    public static string CallUnifiedOrderAPI(ProductOrder po, out WeChatPayData stateCode)
    {
        string prepayID = string.Empty;

        //统一下单API返回的状态码
        stateCode = new WeChatPayData();

        try
        {
            WeChatPayData sendPayData;
            WeChatPayData recvPayData;

            //生成统一下单接口报文
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            sendPayData = new WeChatPayData();
            sendPayData.SetValue("appid", Config.APPID);    //必填
            sendPayData.SetValue("mch_id", Config.MCHID);   //必填
            sendPayData.SetValue("device_info", "WEB");
            sendPayData.SetValue("nonce_str", WeChatPayData.MakeNonceStr());    //必填
            sendPayData.SetValue("body", po.ProductNames);  //必填
            sendPayData.SetValue("detail", JsonMapper.ToJson(po.OrderDetailList));
            sendPayData.SetValue("attach", "订单自定义数据");
            sendPayData.SetValue("out_trade_no", po.OrderID);   //必填
            sendPayData.SetValue("fee_type", "CNY");
            sendPayData.SetValue("total_fee", (po.OrderPrice * 100).ToString("F0"));    //必填，单位为【分】
            sendPayData.SetValue("spbill_create_ip", po.ClientIP);    //必填
            sendPayData.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
            sendPayData.SetValue("time_expire", DateTime.Now.AddMinutes(Config.WeChatOrderExpire).ToString("yyyyMMddHHmmss"));    //最短失效时间间隔必须大于5分钟，可为空，默认2小时，也即prepay_id有效期
            sendPayData.SetValue("goods_tag", "商品标记，代金券或立减优惠功能的参数");
            sendPayData.SetValue("notify_url", Config.PayNotifyUrl);   //必填，微信支付成功后异步通知url
            sendPayData.SetValue("trade_type", "JSAPI");    //必填
            sendPayData.SetValue("openid", po.OpenID); //trade_type = JSAPI，此参数必传

            //生成报文签名
            sendPayData.SetValue("sign", sendPayData.MakeSign());   //必填

            //生成接口所需的XML报文格式
            string sendMsg = sendPayData.ToXml();

            var start = DateTime.Now;

            Log.Debug("JsApiPay", "UnfiedOrder request" + sendMsg);
            //调用微信支付统一下单接口，并获取返回报文
            string recvMsg = HttpService.Post(sendMsg, url, false, Config.WeChatAPITimeout);
            Log.Debug("JsApiPay", "UnfiedOrder response" + recvMsg);

            var end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);

            Log.Debug("JsApiPay", "UnfiedOrder time cost" + timeCost.ToString());

            //recvMsg = "<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg><appid><![CDATA[wxa1ce5cd8d4bc5b3f]]></appid><mch_id><![CDATA[1296478401]]></mch_id><device_info><![CDATA[WEB]]></device_info><nonce_str><![CDATA[vjqH6K12oBv9iEyW]]></nonce_str><sign><![CDATA[42916A4F50AAD1AA1B9A3D222371EBAE]]></sign><result_code><![CDATA[SUCCESS]]></result_code><prepay_id><![CDATA[wx20151223230752785abc1a610183113624]]></prepay_id><trade_type><![CDATA[JSAPI]]></trade_type></xml>";

            if (string.IsNullOrEmpty(recvMsg))
            {
                Log.Error("JsApiPay", "UnfiedOrder接口没有返回数据");

                throw new Exception("UnfiedOrder接口没有返回数据");
            }

            recvPayData = new WeChatPayData();
            //根据XML返回报文生成返回报文对象
            recvPayData.FromXml(recvMsg);

            //校验通信标示
            if (recvPayData.IsSet("return_code") && recvPayData.GetValue("return_code").ToString().ToUpper() == "SUCCESS")
            {
                //当return_code为SUCCESS，才有sign参数返回，校验对方报文签名
                if (!recvPayData.CheckSign())
                {
                    stateCode.SetValue("return_code", "FAIL");
                    stateCode.SetValue("return_msg", "微信支付返回的签名错误，请在安全的网络环境中进行支付！");
                    Log.Error("CallUnifiedOrderAPI::SIGN签名错误", recvPayData.GetValue("sign").ToString());
                }
                else
                {
                    //校验业务结果result_code也是SUCCESS，才有prepay_id返回
                    if (recvPayData.IsSet("result_code") && recvPayData.GetValue("result_code").ToString().ToUpper() == "SUCCESS" && recvPayData.IsSet("prepay_id"))
                    {
                        //获取prepay_id
                        prepayID = recvPayData.GetValue("prepay_id").ToString();
                    }
                    else  //可能是订单已支付、已关闭、订单号重复等错误原因
                    {
                        stateCode.SetValue("result_code", "FAIL");
                        stateCode.SetValue("err_code", recvPayData.GetValue("err_code").ToString());
                        stateCode.SetValue("err_code_des", recvPayData.GetValue("err_code_des").ToString());
                        Log.Error("CallUnifiedOrderAPI", stateCode.ToJson());

                    }
                }
            }
            else //可能为我方报文sign签名错误或参数格式错误
            {
                stateCode.SetValue("return_code", "FAIL");
                stateCode.SetValue("return_msg", recvPayData.GetValue("return_msg").ToString());
                Log.Error("CallUnifiedOrderAPI", stateCode.ToJson());
            }

        }
        catch (Exception ex)
        {
            Log.Error("WxPayAPI", ex.Message);
            throw ex;
        }

        return prepayID;
    }

    /// <summary>
    /// 根据预支付回话标示prepay_id构造JS参数
    /// </summary>
    /// <param name="prepayID"></param>
    /// <returns></returns>
    public static string MakeWXPayJsParam(string prepayID)
    {
        //返回微信客户端调用JS支付所需的参数
        string wxJsApiParam = string.Empty;

        try
        {
            //* 从统一下单成功返回的数据中获取微信浏览器调起jsapi支付所需的参数，
            //* 微信浏览器调起JSAPI时的输入参数格式如下：
            //* {
            //*   "appId" : "wx2421b1c4370ec43b",     //公众号名称，由商户传入     
            //*   "timeStamp":" 1395712654",         //时间戳，自1970年以来的秒数     
            //*   "nonceStr" : "e61463f8efa94090b1f366cccfbbb444", //随机串     
            //*   "package" : "prepay_id=u802345jgfjsdfgsdg888",     
            //*   "signType" : "MD5",         //微信签名方式:    
            //*   "paySign" : "70EA570631E4BB79628FBCA90534C63FF7FADD89" //微信签名 
            //* }
            //* @return string 微信浏览器调起JSAPI时的输入参数，json格式可以直接做参数用
            //* 更详细的说明请参考网页端调起支付API：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7

            WeChatPayData jsPayData = new WeChatPayData();
            jsPayData.SetValue("appId", Config.APPID);
            jsPayData.SetValue("timeStamp", WeChatPayData.MakeTimeStamp());
            jsPayData.SetValue("nonceStr", WeChatPayData.MakeNonceStr());
            jsPayData.SetValue("package", "prepay_id=" + prepayID);
            jsPayData.SetValue("signType", "MD5");
            jsPayData.SetValue("paySign", jsPayData.MakeSign());

            wxJsApiParam = jsPayData.ToJson();

            Log.Debug("jsApiParam", wxJsApiParam);

        }
        catch (Exception ex)
        {
            Log.Error("WxPayAPI", ex.Message);
            throw ex;
        }

        return wxJsApiParam;

    }


    /// <summary>
    /// 根据微信支付交易号查询订单接口，参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_2
    /// 该接口提供所有微信支付订单的查询，商户可以通过该接口主动查询订单状态，完成下一步的业务逻辑。
    /// </summary>
    /// <param name="transaction_id"></param>
    /// <returns></returns>
    public static WeChatPayData OrderQueryByTransID(string transaction_id)
    {
        if (string.IsNullOrEmpty(transaction_id))
        {
            throw new ArgumentNullException("查询订单，缺少transaction_id");
        }

        //微信支付消息发送报文
        WeChatPayData sendPayData = new WeChatPayData();

        //微信支付消息接收报文
        WeChatPayData recvPayData = new WeChatPayData();

        try
        {
            string url = "https://api.mch.weixin.qq.com/pay/orderquery";

            sendPayData.SetValue("appid", Config.APPID);//公众账号ID
            sendPayData.SetValue("mch_id", Config.MCHID);//商户号
            sendPayData.SetValue("transaction_id", transaction_id);//微信订单号
            sendPayData.SetValue("nonce_str", WeChatPayData.MakeNonceStr());//随机字符串
            sendPayData.SetValue("sign", sendPayData.MakeSign());//签名

            string sendMsg = sendPayData.ToXml();

            var start = DateTime.Now;

            Log.Debug("JsApiPay", "OrderQuery request : " + sendMsg);
            string recvMsg = HttpService.Post(sendMsg, url, false, Config.WeChatAPITimeout);//调用HTTP通信接口提交数据
            Log.Debug("JsApiPay", "OrderQuery response : " + recvMsg);

            var end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时
            Log.Debug("JsApiPay", "OrderQuery 接口耗时 : " + timeCost);

            //将xml格式的数据转化为对象以返回
            recvPayData.FromXml(recvMsg);

            //校验微信接口返回值return_code和result_code
            if (recvPayData.IsSet("return_code") && recvPayData.GetValue("return_code").ToString().ToUpper() == "SUCCESS")
            {
                if (!recvPayData.CheckSign())
                {
                    throw new Exception("微信“按transaction_id查询订单”消息sign签名校验错误");
                }

                //return_code和result_code同时为SUCCESS，且sign签名正确，则返回此消息数据
                if (recvPayData.IsSet("result_code") && recvPayData.GetValue("result_code").ToString().ToUpper() == "SUCCESS")
                {
                    return recvPayData;
                }
                else
                {
                    if (recvPayData.IsSet("err_code") && recvPayData.IsSet("err_code_des"))
                    {
                        throw new Exception(recvPayData.GetValue("err_code").ToString() + ":" + recvPayData.GetValue("err_code_des").ToString());
                    }
                }
            }
            else
            {
                if (recvPayData.IsSet("return_msg"))
                {
                    throw new Exception(recvPayData.GetValue("return_msg").ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("WxPayAPI", ex.Message);
        }

        return null;
    }

    /// <summary>
    /// 根据商户订单号查询订单接口，参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_2
    /// 该接口提供所有微信支付订单的查询，商户可以通过该接口主动查询订单状态，完成下一步的业务逻辑。
    /// </summary>
    /// <param name="out_trade_no"></param>
    /// <returns></returns>
    public static WeChatPayData OrderQueryByOutTradeNo(string out_trade_no)
    {
        if (string.IsNullOrEmpty(out_trade_no))
        {
            throw new ArgumentNullException("查询订单，缺少out_trade_no");
        }

        //微信支付消息发送报文
        WeChatPayData sendPayData = new WeChatPayData();

        //微信支付消息接收报文
        WeChatPayData recvPayData = new WeChatPayData();

        try
        {

            string url = "https://api.mch.weixin.qq.com/pay/orderquery";

            sendPayData.SetValue("appid", Config.APPID);//公众账号ID
            sendPayData.SetValue("mch_id", Config.MCHID);//商户号
            sendPayData.SetValue("out_trade_no", out_trade_no);//商户订单号
            sendPayData.SetValue("nonce_str", WeChatPayData.MakeNonceStr());//随机字符串
            sendPayData.SetValue("sign", sendPayData.MakeSign());//签名

            string sendMsg = sendPayData.ToXml();

            var start = DateTime.Now;

            Log.Debug("JsApiPay", "OrderQuery request : " + sendMsg);
            string recvMsg = HttpService.Post(sendMsg, url, false, Config.WeChatAPITimeout);//调用HTTP通信接口提交数据
            Log.Debug("JsApiPay", "OrderQuery response : " + recvMsg);

            var end = DateTime.Now;
            int timeCost = (int)((end - start).TotalMilliseconds);//获得接口耗时
            Log.Debug("JsApiPay", "OrderQuery 接口耗时 : " + timeCost);

            //将xml格式的数据转化为对象以返回
            recvPayData.FromXml(recvMsg);

            //校验微信接口返回值return_code和result_code
            if (recvPayData.IsSet("return_code") && recvPayData.GetValue("return_code").ToString().ToUpper() == "SUCCESS")
            {
                if (!recvPayData.CheckSign())
                {
                    throw new Exception("微信“按out_trade_no查询订单”消息sign签名校验错误");
                }

                //return_code和result_code同时为SUCCESS，且sign签名正确，则返回此消息数据
                if (recvPayData.IsSet("result_code") && recvPayData.GetValue("result_code").ToString().ToUpper() == "SUCCESS")
                {
                    return recvPayData;
                }
                else  //result_code返回FAIL，可能微信支付中此订单号不存在、微信支付系统异常
                {
                    if (recvPayData.IsSet("err_code") && recvPayData.IsSet("err_code_des"))
                    {
                        throw new Exception(recvPayData.GetValue("err_code").ToString() + ":" + recvPayData.GetValue("err_code_des").ToString());
                    }
                }
            }
            else  //return_code返回FAIL，可能签名失败、参数格式校验错误
            {
                if (recvPayData.IsSet("return_msg"))
                {
                    throw new Exception(recvPayData.GetValue("return_msg").ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("WxPayAPI", ex.Message);
        }

        return null;
    }

    /// <summary>
    /// 微信支付，关闭订单接口
    /// 以下情况需要调用关单接口：商户订单支付失败需要生成新单号重新发起支付，要对原订单号调用关单，避免重复支付；系统下单后，用户支付超时，系统退出不再受理，避免用户继续，请调用关单接口。
    /// 注意：订单生成后不能马上调用关单接口，最短调用时间间隔为5分钟。
    /// API接口参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_3
    /// </summary>
    /// <param name="orderID"></param>
    /// <returns></returns>
    public static WeChatPayData CloseOrder(string orderID)
    {
        if (string.IsNullOrEmpty(orderID))
        {
            throw new ArgumentNullException("orderID不能为空");
        }

        //微信支付消息发送报文
        WeChatPayData sendPayData = new WeChatPayData();

        //微信支付消息接收报文
        WeChatPayData recvPayData = new WeChatPayData();

        try
        {
            string url = "https://api.mch.weixin.qq.com/pay/closeorder";

            sendPayData.SetValue("appid", Config.APPID);//公众账号ID
            sendPayData.SetValue("mch_id", Config.MCHID);//商户号
            sendPayData.SetValue("nonce_str", WeChatPayData.MakeNonceStr());//随机字符串		
            sendPayData.SetValue("out_trade_no", orderID);//商户订单号		
            sendPayData.SetValue("sign", sendPayData.MakeSign());//签名
            string sendMsg = sendPayData.ToXml();

            string recvMsg = HttpService.Post(sendMsg, url, false, Config.WeChatAPITimeout);

            recvPayData.FromXml(recvMsg);

            //校验返回状态码
            if (recvPayData.IsSet("return_code") && recvPayData.GetValue("return_code").ToString().ToUpper() == "SUCCESS")
            {
                //当return_code为SUCCESS，才有sign参数返回
                if (!recvPayData.CheckSign())
                {
                    throw new Exception("sign签名校验错误");
                }

                //return_code和result_code同时为SUCCESS，且sign签名正确，则返回此消息数据
                if (recvPayData.IsSet("result_code") && recvPayData.GetValue("result_code").ToString().ToUpper() == "SUCCESS")
                {
                    return recvPayData;
                }
                else  //result_code返回FAIL，可能微信支付中此订单号不存在、微信支付系统异常
                {
                    if (recvPayData.IsSet("err_code") && recvPayData.IsSet("err_code_des"))
                    {
                        throw new Exception(recvPayData.GetValue("err_code").ToString() + ":" + recvPayData.GetValue("err_code_des").ToString());
                    }
                }
            }
            else  //return_code返回FAIL，可能签名失败、参数格式校验错误
            {
                if (recvPayData.IsSet("return_msg"))
                {
                    throw new Exception(recvPayData.GetValue("return_msg").ToString());
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("WxPayAPI", ex.Message);
        }

        return recvPayData;
    }

}