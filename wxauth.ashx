<%@ WebHandler Language="C#" Class="Auth" %>

using System;
using System.Web;
using LitJson;

/// <summary>
/// 微信支付认证：
/// 1，向微信支付oauth认证接口发出CODE请求，微信支付返回CODE回调本页面
/// 2，用CODE向微信支付的access token接口发出请求，获取openid和access token，存入session
/// 3，如果是snsapi_userinfo授权模式，则使用openid和access token向微信支付user_info接口发出请求，获取userinfo，存入session
/// 4，根据state参数跳回原页面
/// </summary>
public class Auth : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    public void ProcessRequest(HttpContext context)
    {

        string redirectUri, code, scope, urlReferrer, authUrl, strAuth, strUserInfo;
        redirectUri = context.Request.Url.AbsoluteUri.Split('?')[0];
        code = context.Request.QueryString["code"];
        scope = context.Request.QueryString["scope"];
        urlReferrer = context.Request.QueryString["state"];

        try
        {
            //第一次请求授权页，直接跳转到微信oauth网页授权，并指定本页面为回调页面
            if (string.IsNullOrEmpty(code))
            {
                if (string.IsNullOrEmpty(scope))
                {
                    scope = "snsapi_userinfo"; //显式授权方式，需要用户确认。但对于已关注公众号的用户从公众号会话或菜单中进入本页，即使指定了snsapi_userinfo，也任然是静默授权
                }
                else
                {
                    scope = scope.ToLower();
                }

                authUrl = String.Format(@"https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state={3}#wechat_redirect",
                    Config.APPID,
                    HttpUtility.UrlEncode(redirectUri),
                    scope,
                    urlReferrer);

                context.Response.Redirect(authUrl);

            }
            else
            {
                Log.Info("CODE", code);

                //微信回调redirect_uri?code=CODE&state=STATE
                //静默授权或用户同意授权获取oauth code后，进一步获取网页授权access token和openid
                authUrl = String.Format(@"https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code",
                    Config.APPID,
                    Config.APPSECRET,
                    code);

                //微信返回正确的json数据
                //{
                //"access_token":"ACCESS_TOKEN",
                //"expires_in":7200,
                //"refresh_token":"REFRESH_TOKEN",
                //"openid":"OPENID",
                //"scope":"SCOPE",
                //"unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
                //}

                strAuth = HttpService.Get(authUrl);

                JsonData jAuthInfo = JsonMapper.ToObject(strAuth);

                Log.Info("strAuth", strAuth);

                if (jAuthInfo != null && jAuthInfo["openid"] != null && jAuthInfo["access_token"] != null)
                {
                    //在json中添加用户的IP信息
                    jAuthInfo["client_ip"] = context.Request.UserHostAddress;

                    //openid和access token存入session
                    context.Session["WxAuthInfo"] = jAuthInfo;

                    //进一步获取用户信息
                    if (jAuthInfo["scope"].ToString().ToLower() == "snsapi_userinfo")
                    {
                        authUrl = String.Format(@"https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN",
                            jAuthInfo["access_token"].ToString(),
                            jAuthInfo["openid"].ToString());


                        //微信返回正确的json数据
                        //{
                        //"openid":" OPENID",
                        //" nickname": NICKNAME,
                        //"sex":"1",
                        //"province":"PROVINCE"
                        //"city":"CITY",
                        //"country":"COUNTRY",
                        //"headimgurl":"http://wx.qlogo.cn/mmopen/g3MonUZtNHkdmzicIlibx6iaFqAc56vxLSUfpb6n5WKSYVY0ChQKkiaJSgQ1dZuTOgvLLrhJbERQQ4eMsv84eavHiaiceqxibJxCfHe/46", 
                        //"privilege":[
                        //"PRIVILEGE1"
                        //"PRIVILEGE2"
                        //],
                        //"unionid": "o6_bmasdasdsad6_2sgVt7hMZOPfL"
                        //}

                        strUserInfo = HttpService.Get(authUrl);

                        JsonData jUserInfo = JsonMapper.ToObject(strUserInfo);

                        Log.Info("strUserInfo", strUserInfo);

                        if (jUserInfo != null && jUserInfo["openid"] != null)
                        {
                            context.Session["WxUserInfo"] = jUserInfo;
                        }
                        else
                        {
                            //根据token获取用户信息错误
                            throw new Exception(strUserInfo);
                        }

                    }
                }
                else
                {
                    //根据CODE获取token错误
                    throw new Exception(strAuth);
                }

                //跳转到授权前的业务页面
                if (!string.IsNullOrEmpty(urlReferrer))
                {
                    context.Response.Redirect(urlReferrer);
                }
                else
                {
                    context.Response.Redirect("default.aspx");
                }
            }
        }
        catch (System.Threading.ThreadAbortException)
        { }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
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