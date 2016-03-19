<%@ WebHandler Language="C#" Class="qqauth" %>

using System;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Security;
using LitJson;

/// <summary>
/// QQ互联接口，参考文档：http://connect.qq.com/
/// 1，向QQ互联oauth认证接口发出CODE请求，QQ返回CODE回调本页面
/// 2，使用CODE向QQ的access token接口发出请求，获取access token
/// 3，使用access_token向QQ的openid接口发出请求，获取openid，把access_token, openid, client_id都存入session["QQAuthInfo"]
/// 4，使用openid、access token和APP ID向QQ get_user_info接口发出请求，获取userinfo，存入session["QQUserInfo"]，使用ASP.NET表单认证，生成cookies，跳回请求页
/// </summary>
public class qqauth : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    public void ProcessRequest (HttpContext context) {
        string redirectUri, code, display, authUrl, strAccessToken, strOpenID, strUserInfo;
        redirectUri = context.Request.Url.AbsoluteUri.Split('?')[0];
        code = context.Request.QueryString["code"];

        //用于展示的样式。不传则默认展示为PC下的样式。如果传入“mobile”，则展示为mobile端下的样式。
        display = context.Request.QueryString["display"];

        //QQ互联API接口列表，参考：http://wiki.connect.qq.com/api列表
        string scope = "get_user_info";

        try
        {
            //第一次请求授权页，直接跳转到QQ oauth网页授权
            if (string.IsNullOrEmpty(code))
            {
                //随机生成state，防止CSRF攻击
                context.Session["state"] = UtilityHelper.GetMd5Hash((new Random()).ToString());

                authUrl = String.Format(@"https://graph.qq.com/oauth2.0/authorize?response_type=code&client_id={0}&redirect_uri={1}&state={2}&scope={3}&display={4}",
                    Config.QQAppID,
                    HttpUtility.UrlEncode(redirectUri),
                    context.Session["state"],
                    scope,
                    display);

                context.Response.Redirect(authUrl);

            }
            else
            {
                //QQ回调redirect_uri?code=CODE&state=STATE

                Log.Info("QQ CODE", code);
                Log.Info("state in session", context.Session["state"].ToString());

                if (context.Request.QueryString["state"] == context.Session["state"].ToString())
                {
                    JsonData jAuthInfo = new JsonData();

                    //根据APP ID、APP KEY和CODE获取access_token
                    authUrl = String.Format(@"https://graph.qq.com/oauth2.0/token?grant_type=authorization_code&client_id={0}&client_secret={1}&code={2}&redirect_uri={3}",
                        Config.QQAppID,
                        Config.QQAppKey,
                        code,
                        redirectUri);

                    //返回数据包格式：access_token=FE04************************CCE2&expires_in=7776000&refresh_token=88E4************************BE14
                    //接口调用有错误时，会返回code和msg字段，以url参数对的形式返回，value部分会进行url编码（UTF-8）。
                    strAccessToken = HttpService.Get(authUrl);

                    Log.Info("QQ AccessToken", strAccessToken);

                    Match rxAccessToken = Regex.Match(strAccessToken, "access_token=([A-Za-z0-9]*)&", RegexOptions.IgnoreCase);
                    if (rxAccessToken != null && rxAccessToken.Groups.Count == 2)
                    {
                        jAuthInfo["access_token"] = rxAccessToken.Groups[1].Value;

                        //根据access_token获取openid
                        authUrl = String.Format(@"https://graph.qq.com/oauth2.0/me?access_token={0}",
                        jAuthInfo["access_token"].ToString());

                        //返回的数据包格式：callback( {"client_id":"YOUR_APPID","openid":"YOUR_OPENID"} );
                        strOpenID = HttpService.Get(authUrl);

                        Log.Info("QQ OpenID", strOpenID);

                        strOpenID = Regex.Match(strOpenID, @"\{.*\}").Value;

                        JsonData jOpenID = JsonMapper.ToObject(strOpenID);

                        if (jOpenID != null && jOpenID["openid"] != null)
                        {
                            jAuthInfo["openid"] = jOpenID["openid"];
                        }
                        else
                        {
                            //根据access_token获取openid错误
                            throw new Exception(strOpenID);
                        }

                        //在json中添加用户的IP信息
                        jAuthInfo["client_ip"] = context.Request.UserHostAddress;

                        ////QQ用户的access_token、openid和客户端IP存入session
                        //context.Session["QQAuthInfo"] = jAuthInfo;

                        //根据access_token和openid查询QQ用户信息
                        authUrl = String.Format(@"https://graph.qq.com/user/get_user_info?access_token={0}&oauth_consumer_key={1}&openid={2}",
                        jAuthInfo["access_token"].ToString(),
                        Config.QQAppID,
                        jAuthInfo["openid"].ToString());

                        strUserInfo = HttpService.Get(authUrl);
                        Log.Info("QQ UserInfo", strUserInfo);

                        JsonData jUserInfo = JsonMapper.ToObject(strUserInfo);

                        if (jUserInfo != null && jUserInfo["ret"].ToString() == "0" && jUserInfo["nickname"] != null)
                        {
                            //QQ用户信息存入session
                            context.Session["QQUserInfo"] = jUserInfo;

                            //验证是否credentials中指定的QQ用户
                            if (FormsAuthentication.Authenticate(jAuthInfo["openid"].ToString(), jAuthInfo["openid"].ToString()))
                            {
                                //如果credentials中指定了此QQ用户，则为其生成认证凭证和cookies，跳转到原请求页
                                FormsAuthentication.RedirectFromLoginPage(jUserInfo["nickname"].ToString(), false);
                            }
                            else
                            {
                                //如果此QQ用户没有认证，即跳回登录页
                                context.Response.Redirect("login.aspx");
                            }
                        }
                        else
                        {
                            //根据access_token和openid获取QQ用户信息错误
                            throw new Exception(strUserInfo);
                        }
                    }
                    else
                    {
                        //根据CODE获取access_token错误
                        throw new Exception(strAccessToken);
                    }
                }
                else
                {
                    //state不符，可能被CSRF
                    throw new Exception("请求的state和session中不符");
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

    public bool IsReusable {
        get {
            return false;
        }
    }

}