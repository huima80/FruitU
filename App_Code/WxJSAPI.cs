using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Caching;
using LitJson;

/// <summary>
/// WeChatAPI 的摘要说明
/// </summary>
public class WxJSAPI
{
    public WxJSAPI()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    /// <summary>
    /// access_token是公众号的全局唯一票据，公众号调用各接口时都需使用access_token。开发者需要进行妥善保存。access_token的存储至少要保留512个字符空间。access_token的有效期目前为2个小时，需定时刷新，重复获取将导致上次获取的access_token失效。
    /// API参考：http://mp.weixin.qq.com/wiki/15/54ce45d8d30b6bf6758f68d2e95bc627.html
    /// </summary>
    /// <returns></returns>
    public static string GetAccessToken()
    {
        string token = string.Empty;

        try
        {
            if (HttpRuntime.Cache["AccessToken"] == null)
            {
                string tokenUrl = string.Format(@"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", Config.APPID, Config.APPSECRET);

                string strToken = HttpService.Get(tokenUrl);

                JsonData jToken = JsonMapper.ToObject(strToken);

                if (jToken["access_token"] != null)
                {

                    //根据微信返回的token和有效期，存入Cache
                    double tokenExpiration = double.Parse(jToken["expires_in"].ToString());
                    HttpRuntime.Cache.Insert("AccessToken", jToken["access_token"].ToString(), null, DateTime.Now.AddSeconds(tokenExpiration), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

                    token = jToken["access_token"].ToString();
                }
                else
                {
                    token = strToken;
                    throw new Exception(strToken);
                }
            }
            else
            {
                token = HttpRuntime.Cache["AccessToken"].ToString();

            }
        }
        catch (Exception ex)
        {
            Log.Error("GetAccessToken", ex.Message);
        }

        return token;

    }

    /// <summary>
    /// jsapi_ticket是公众号用于调用微信JS接口的临时票据。正常情况下，jsapi_ticket的有效期为7200秒，通过access_token来获取。由于获取jsapi_ticket的api调用次数非常有限，频繁刷新jsapi_ticket会导致api调用受限，影响自身业务，开发者必须在自己的服务全局缓存jsapi_ticket
    /// API参考：http://mp.weixin.qq.com/wiki/7/aaa137b55fb2e0456bf8dd9148dd613f.html#.E9.99.84.E5.BD.951-JS-SDK.E4.BD.BF.E7.94.A8.E6.9D.83.E9.99.90.E7.AD.BE.E5.90.8D.E7.AE.97.E6.B3.95
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static string GetJsAPITicket(string token)
    {
        string jsAPITicket = string.Empty;

        try
        {
            if (HttpRuntime.Cache["JsAPITicket"] == null)
            {

                string ticketUrl = String.Format(@"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", token);

                string strTicket = HttpService.Get(ticketUrl);

                JsonData jTicket = JsonMapper.ToObject(strTicket);

                if (jTicket["ticket"] != null)
                {
                    //根据微信返回的ticket和有效期，存入Cache
                    double ticketExpiration = double.Parse(jTicket["expires_in"].ToString());
                    HttpRuntime.Cache.Insert("JsAPITicket", jTicket["ticket"].ToString(), null, DateTime.Now.AddSeconds(ticketExpiration), Cache.NoSlidingExpiration, CacheItemPriority.Default, null);

                    jsAPITicket = jTicket["ticket"].ToString();

                }
                else
                {
                    throw new Exception(string.Format("errcode:{0},errmsg:{1}", jTicket["errcode"], jTicket["errmsg"]));
                }
            }
            else
            {
                jsAPITicket = HttpRuntime.Cache["JsAPITicket"].ToString();

            }
        }
        catch (Exception ex)
        {
            Log.Error("GetJsAPITicket", ex.Message);
        }

        return jsAPITicket;
    }

    /// <summary>
    /// JS-SDK使用权限签名
    /// 签名生成规则如下：参与签名的字段包括noncestr（随机字符串）, 有效的jsapi_ticket, timestamp（时间戳）, url（当前网页的URL，不包含#及其后面部分） 。对所有待签名参数按照字段名的ASCII 码从小到大排序（字典序）后，使用URL键值对的格式（即key1=value1&key2=value2…）拼接成字符串string1。这里需要注意的是所有参数名均为小写字符。对string1作sha1加密，字段名和字段值都采用原始值，不进行URL 转义。
    /// API参考：http://mp.weixin.qq.com/wiki/7/aaa137b55fb2e0456bf8dd9148dd613f.html#.E9.99.84.E5.BD.951-JS-SDK.E4.BD.BF.E7.94.A8.E6.9D.83.E9.99.90.E7.AD.BE.E5.90.8D.E7.AE.97.E6.B3.95
    /// 签名算法验证工具：http://mp.weixin.qq.com/debug/cgi-bin/sandbox?t=jsapisign
    /// </summary>
    /// <param name="jsapi_ticket"></param>
    /// <param name="url"></param>
    /// <param name="noncestr"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public static string MakeJsAPISign(string jsapi_ticket, string url, out string noncestr, out string timestamp)
    {
        string jsSign = string.Empty;
        try
        {
            noncestr = WeChatPayData.MakeNonceStr();
            timestamp = WeChatPayData.MakeTimeStamp();
            //参与加密的参数key全部小写
            WeChatPayData signData = new WeChatPayData();
            signData.SetValue("jsapi_ticket", jsapi_ticket);
            signData.SetValue("timestamp", timestamp);
            signData.SetValue("noncestr", noncestr);
            signData.SetValue("url", url);
            string param = signData.ToSignStr();

            Log.Debug("MakeJsAPISign", "SHA1 encrypt param : " + param);
            //SHA1加密
            jsSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");
            Log.Debug("MakeJsAPISign", "SHA1 encrypt result : " + jsSign);

        }
        catch (Exception ex)
        {
            Log.Error("MakeJsAPISign", ex.ToString());
            throw ex;
        }

        return jsSign;
    }

    /// <summary>
    ///  获取收货地址js函数入口参数,详情请参考收货地址共享接口：http://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_9
    /// </summary>
    /// <param name="accessToken"></param>
    /// <param name="url"></param>
    /// <returns>共享收货地址js函数需要的参数，json格式可以直接做参数使用</returns>
    public static string MakeEditAddressJsParam(string accessToken, string url)
    {
        string editAddrParam = string.Empty;
        try
        {
            //参与加密的参数key全部小写
            WeChatPayData signData = new WeChatPayData();
            signData.SetValue("appid", Config.APPID);
            signData.SetValue("url", url);
            signData.SetValue("timestamp", WeChatPayData.MakeTimeStamp());
            signData.SetValue("noncestr", WeChatPayData.MakeNonceStr());
            signData.SetValue("accesstoken", accessToken);
            string param = signData.ToSignStr();

            Log.Debug("MakeEditAddressJsParam", "SHA1 encrypt param : " + param);
            //SHA1加密
            string addrSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");
            Log.Debug("MakeEditAddressJsParam", "SHA1 encrypt result : " + addrSign);

            //构造收货地址js函数入口参数
            WeChatPayData paramsData = new WeChatPayData();
            paramsData.SetValue("appId", Config.APPID);
            paramsData.SetValue("scope", "jsapi_address");
            paramsData.SetValue("signType", "sha1");
            paramsData.SetValue("addrSign", addrSign);
            paramsData.SetValue("timeStamp", signData.GetValue("timestamp"));
            paramsData.SetValue("nonceStr", signData.GetValue("noncestr"));

            //转为json格式
            editAddrParam = paramsData.ToJson();
            Log.Debug("MakeEditAddressJsParam", editAddrParam);
        }
        catch (Exception ex)
        {
            Log.Error("MakeEditAddressJsParam", ex.ToString());
            throw ex;
        }

        return editAddrParam;
    }


}