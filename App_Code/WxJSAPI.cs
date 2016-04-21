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
    /// 从Cache缓存中获取Access Token，如果缓存为空，则重新向微信请求Access Token
    /// </summary>
    /// <returns></returns>
    public static string GetAccessToken()
    {
        string token = string.Empty;

        try
        {
            if (HttpRuntime.Cache["AccessToken"] == null)
            {
                token = RefreshAccessToken();
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
    /// access_token是公众号的全局唯一票据，公众号调用各接口时都需使用access_token。开发者需要进行妥善保存。access_token的存储至少要保留512个字符空间。access_token的有效期目前为2个小时，需定时刷新，重复获取将导致上次获取的access_token失效。
    /// API参考：http://mp.weixin.qq.com/wiki/15/54ce45d8d30b6bf6758f68d2e95bc627.html
    /// </summary>
    /// <returns></returns>
    public static string RefreshAccessToken()
    {
        string token = string.Empty;
        string tokenUrl = string.Format(@"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}", Config.APPID, Config.APPSECRET);
        string strToken;
        JsonData jToken;
        double tokenExpiration;

        while (string.IsNullOrEmpty(token))
        {
            strToken = HttpService.Get(tokenUrl);
            jToken = JsonMapper.ToObject(strToken);
            if (jToken.Keys.Contains("access_token") && jToken.Keys.Contains("expires_in") && jToken["access_token"] != null && jToken["expires_in"] != null)
            {
                if (double.TryParse(jToken["expires_in"].ToString(), out tokenExpiration))
                {
                    token = jToken["access_token"].ToString();

                    //把微信返回的token存入Cache，设置cache项有效期为expires_in秒数再提前10分钟，且不允许.net自动回收
                    HttpRuntime.Cache.Insert("AccessToken", token, null, DateTime.Now.AddSeconds(tokenExpiration - 600), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, onAccessTokenRemovedCallBack);
                }
            }
        }

        Log.Info("RefreshAccessToken", token);

        return token;
    }

    /// <summary>
    /// Cache删除过期的Access Token缓存后，重新向微信请求缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    private static void onAccessTokenRemovedCallBack(string key, object value, CacheItemRemovedReason reason)
    {
        if (key == "AccessToken")
        {
            RefreshAccessToken();
        }
    }

    /// <summary>
    /// jsapi_ticket是公众号用于调用微信JS接口的临时票据。正常情况下，jsapi_ticket的有效期为7200秒，通过access_token来获取。由于获取jsapi_ticket的api调用次数非常有限，频繁刷新jsapi_ticket会导致api调用受限，影响自身业务，开发者必须在自己的服务全局缓存jsapi_ticket
    /// API参考：http://mp.weixin.qq.com/wiki/7/aaa137b55fb2e0456bf8dd9148dd613f.html#.E9.99.84.E5.BD.951-JS-SDK.E4.BD.BF.E7.94.A8.E6.9D.83.E9.99.90.E7.AD.BE.E5.90.8D.E7.AE.97.E6.B3.95
    /// </summary>
    /// <returns></returns>
    public static string GetJsAPITicket()
    {
        string jsAPITicket = string.Empty;

        try
        {
            if (HttpRuntime.Cache["JsAPITicket"] == null)
            {
                jsAPITicket = RefreshJsAPITicket();
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
    /// 刷新jsapi_ticket
    /// </summary>
    /// <returns></returns>
    public static string RefreshJsAPITicket()
    {
        string jsAPITicket = string.Empty;
        string ticketUrl = String.Format(@"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi", GetAccessToken());
        string strTicket;
        JsonData jTicket;
        double ticketExpiration;

        while (string.IsNullOrEmpty(jsAPITicket))
        {
            strTicket = HttpService.Get(ticketUrl);
            jTicket = JsonMapper.ToObject(strTicket);
            if (jTicket.Keys.Contains("ticket") && jTicket.Keys.Contains("expires_in") && jTicket["ticket"] != null && jTicket["expires_in"] != null)
            {
                //根据微信返回的ticket和有效期，存入Cache
                if (double.TryParse(jTicket["expires_in"].ToString(), out ticketExpiration))
                {
                    jsAPITicket = jTicket["ticket"].ToString();
                    HttpRuntime.Cache.Insert("JsAPITicket", jsAPITicket, null, DateTime.Now.AddSeconds(ticketExpiration - 600), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, onJsAPITicketRemovedCallBack);
                }
            }
        }

        Log.Info("RefreshJsAPITicket", jsAPITicket);

        return jsAPITicket;
    }

    /// <summary>
    /// Cache删除过期的JsAPITicket缓存后，重新向微信请求缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    private static void onJsAPITicketRemovedCallBack(string key, object value, CacheItemRemovedReason reason)
    {
        if (key == "JsAPITicket")
        {
            RefreshJsAPITicket();
        }
    }

    /// <summary>
    /// 获取微信卡券APITicket
    /// </summary>
    /// <returns></returns>
    public static string GetAPITicket()
    {
        string apiTicket = string.Empty;

        try
        {
            if (HttpRuntime.Cache["APITicket"] == null)
            {
                apiTicket = RefreshAPITicket();
            }
            else
            {
                apiTicket = HttpRuntime.Cache["APITicket"].ToString();

            }
        }
        catch (Exception ex)
        {
            Log.Error("GetAPITicket", ex.Message);
        }

        return apiTicket;
    }

    /// <summary>
    /// 刷新微信票据APITicket
    /// </summary>
    /// <returns></returns>
    public static string RefreshAPITicket()
    {
        string apiTicket = string.Empty;
        string ticketUrl = String.Format(@"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=wx_card", GetAccessToken());
        string strTicket;
        JsonData jTicket;
        double ticketExpiration;

        while (string.IsNullOrEmpty(apiTicket))
        {
            strTicket = HttpService.Get(ticketUrl);
            jTicket = JsonMapper.ToObject(strTicket);
            if (jTicket.Keys.Contains("ticket") && jTicket.Keys.Contains("expires_in") && jTicket["ticket"] != null && jTicket["expires_in"] != null)
            {
                //根据微信返回的ticket和有效期，存入Cache
                if (double.TryParse(jTicket["expires_in"].ToString(), out ticketExpiration))
                {
                    apiTicket = jTicket["ticket"].ToString();
                    HttpRuntime.Cache.Insert("APITicket", apiTicket, null, DateTime.Now.AddSeconds(ticketExpiration - 600), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, onAPITicketRemovedCallBack);
                }
            }
        }

        Log.Info("RefreshAPITicket", apiTicket);

        return apiTicket;
    }

    /// <summary>
    /// Cache删除过期的APITicket缓存后，重新向微信请求缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="reason"></param>
    private static void onAPITicketRemovedCallBack(string key, object value, CacheItemRemovedReason reason)
    {
        if (key == "APITicket")
        {
            RefreshAPITicket();
        }
    }

    public static string MakeCardSign(string apiTicket, string url, out string noncestr, out string timestamp)
    {
        string cardSign = string.Empty;
        try
        {
            noncestr = WeChatPayData.MakeNonceStr();
            timestamp = WeChatPayData.MakeTimeStamp();
            //参与加密的参数key全部小写
            WeChatPayData signData = new WeChatPayData();
            signData.SetValue("api_ticket", apiTicket);
            signData.SetValue("timestamp", timestamp);
            signData.SetValue("noncestr", noncestr);
            signData.SetValue("url", url);
            string param = signData.ToSignStr();

            Log.Debug("MakeJsAPISign", "SHA1 encrypt param : " + param);
            //SHA1加密
            cardSign = FormsAuthentication.HashPasswordForStoringInConfigFile(param, "SHA1");
            Log.Debug("MakeJsAPISign", "SHA1 encrypt result : " + cardSign);

        }
        catch (Exception ex)
        {
            Log.Error("MakeJsAPISign", ex.ToString());
            throw ex;
        }

        return cardSign;
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