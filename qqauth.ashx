<%@ WebHandler Language="C#" Class="QQAuth" %>

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
/// 4，使用openid、access token和APP ID向QQ get_user_info接口发出请求，获取userinfo，存入session["QQUser"]，使用ASP.NET表单认证，生成cookies，跳回请求页
/// </summary>
public class QQAuth : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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

                if (context.Session["state"] == null)
                {
                    //如果自上次请求CODE后，session 已超时，则重新请求CODE
                    context.Response.Redirect(redirectUri);
                }

                if (context.Request.QueryString["state"] == context.Session["state"].ToString())
                {
                    JsonData jAuthInfo = new JsonData();

                    //根据APP ID、APP KEY和CODE获取access_token
                    authUrl = String.Format(@"https://graph.qq.com/oauth2.0/token?grant_type=authorization_code&client_id={0}&client_secret={1}&code={2}&redirect_uri={3}",
                        Config.QQAppID,
                        Config.QQAppKey,
                        code,
                        HttpUtility.UrlEncode(redirectUri));

                    //返回数据包格式：access_token=FE04************************CCE2&expires_in=7776000&refresh_token=88E4************************BE14
                    //接口调用有错误时，会返回code和msg字段，以url参数对的形式返回，value部分会进行url编码（UTF-8）。
                    strAccessToken = HttpService.Get(authUrl);

                    Log.Info(this.GetType().ToString(), "QQ AccessToken:" + strAccessToken);

                    Match rxAccessToken = Regex.Match(strAccessToken, "access_token=([A-Za-z0-9]*)&expires_in=([A-Za-z0-9]*)&refresh_token=([A-Za-z0-9]*)", RegexOptions.IgnoreCase);
                    if (rxAccessToken != null && rxAccessToken.Groups.Count == 4)
                    {
                        jAuthInfo["access_token"] = rxAccessToken.Groups[1].Value;
                        jAuthInfo["expires_in"] = rxAccessToken.Groups[2].Value;
                        jAuthInfo["refresh_token"] = rxAccessToken.Groups[3].Value;

                        //根据access_token获取openid
                        authUrl = String.Format(@"https://graph.qq.com/oauth2.0/me?access_token={0}",
                        jAuthInfo["access_token"].ToString());

                        //返回的数据包格式：callback( {"client_id":"YOUR_APPID","openid":"YOUR_OPENID"} );
                        strOpenID = HttpService.Get(authUrl);

                        Log.Info(this.GetType().ToString(), "QQ OpenID:" + strOpenID);

                        strOpenID = Regex.Match(strOpenID, @"\{.*\}").Value;

                        if (string.IsNullOrEmpty(strOpenID))
                        {
                            //正则解析openid错误
                            throw new Exception("正则解析openid错误");
                        }

                        JsonData jOpenID = JsonMapper.ToObject(strOpenID);

                        if (jOpenID != null && jOpenID.Keys.Contains("openid") && jOpenID["openid"] != null)
                        {
                            jAuthInfo["openid"] = jOpenID["openid"];
                        }
                        else
                        {
                            //openid为空
                            throw new Exception(strOpenID);
                        }

                        //根据access_token和openid查询QQ用户信息
                        authUrl = String.Format(@"https://graph.qq.com/user/get_user_info?access_token={0}&oauth_consumer_key={1}&openid={2}",
                                            jAuthInfo["access_token"].ToString(),
                                            Config.QQAppID,
                                            jAuthInfo["openid"].ToString());

                        strUserInfo = HttpService.Get(authUrl);
                        Log.Info(this.GetType().ToString(), "QQ User:" + strUserInfo);

                        //{
                        //    "ret": 0,
                        //    "msg": "",
                        //    "is_lost":0,
                        //    "nickname": "Mehmet",
                        //    "gender": "男",
                        //    "province": "江苏",
                        //    "city": "南京",
                        //    "year": "1986",
                        //    "figureurl": "http:\/\/qzapp.qlogo.cn\/qzapp\/101296477\/0065B13A1E75740D1B048850D9A3E31D\/30",
                        //    "figureurl_1": "http:\/\/qzapp.qlogo.cn\/qzapp\/101296477\/0065B13A1E75740D1B048850D9A3E31D\/50",
                        //    "figureurl_2": "http:\/\/qzapp.qlogo.cn\/qzapp\/101296477\/0065B13A1E75740D1B048850D9A3E31D\/100",
                        //    "figureurl_qq_1": "http:\/\/q.qlogo.cn\/qqapp\/101296477\/0065B13A1E75740D1B048850D9A3E31D\/40",
                        //    "figureurl_qq_2": "http:\/\/q.qlogo.cn\/qqapp\/101296477\/0065B13A1E75740D1B048850D9A3E31D\/100",
                        //    "is_yellow_vip": "0",
                        //    "vip": "0",
                        //    "yellow_vip_level": "0",
                        //    "level": "0",
                        //    "is_yellow_year_vip": "0"
                        //}

                        if (string.IsNullOrEmpty(strUserInfo))
                        {
                            //QQ用户信息为空
                            throw new Exception("QQ用户信息为空");
                        }

                        JsonData jUserInfo = JsonMapper.ToObject(strUserInfo);

                        if (jUserInfo != null && jUserInfo.Keys.Contains("ret") && jUserInfo["ret"] != null && jUserInfo["ret"].ToString() == "0" && jUserInfo.Keys.Contains("nickname") && jUserInfo["nickname"] != null)
                        {
                            bool isNewUser;
                            QQUser qqUser;
                            //在成员资格数据库中查找此QQ用户信息
                            MembershipUser mUser = Membership.GetUser(jAuthInfo["openid"].ToString(), true);
                            if (mUser == null)
                            {
                                isNewUser = true;
                                //使用QQ用户的openid和随机密码新建成员资格用户
                                mUser = Membership.CreateUser(jAuthInfo["openid"].ToString(), Membership.GeneratePassword(10, 1));
                                if (mUser != null)
                                {
                                    qqUser = new QQUser(mUser);
                                }
                                else
                                {
                                    qqUser = new QQUser();
                                }
                            }
                            else
                            {
                                isNewUser = false;
                                qqUser = new QQUser(mUser);
                            }

                            qqUser.OpenID = jAuthInfo["openid"].ToString();
                            qqUser.NickName = jUserInfo["nickname"] != null ? jUserInfo["nickname"].ToString() : string.Empty;
                            qqUser.Birthday = jUserInfo["year"] != null ? (DateTime?)DateTime.Parse(jUserInfo["year"].ToString() + "-1-1") : null;
                            qqUser.Sex = jUserInfo["gender"] != null ? (jUserInfo["gender"].ToString() == "男" ? true : false) : true;
                            qqUser.Province = jUserInfo["province"] != null ? jUserInfo["province"].ToString() : string.Empty;
                            qqUser.City = jUserInfo["city"] != null ? jUserInfo["city"].ToString() : string.Empty;
                            qqUser.FigureUrl = jUserInfo["figureurl"] != null ? jUserInfo["figureurl"].ToString() : string.Empty;
                            qqUser.FigureUrl1 = jUserInfo["figureurl_1"] != null ? jUserInfo["figureurl_1"].ToString() : string.Empty;
                            qqUser.FigureUrl2 = jUserInfo["figureurl_2"] != null ? jUserInfo["figureurl_2"].ToString() : string.Empty;
                            qqUser.FigureUrlQQ1 = jUserInfo["figureurl_qq_1"] != null ? jUserInfo["figureurl_qq_1"].ToString() : string.Empty;
                            qqUser.FigureUrlQQ2 = jUserInfo["figureurl_qq_2"] != null ? jUserInfo["figureurl_qq_2"].ToString() : string.Empty;
                            qqUser.IsVip = jUserInfo["vip"] != null ? (jUserInfo["vip"].ToString() == "1" ? true : false) : false;
                            qqUser.VipLevel = jUserInfo["level"] != null ? int.Parse(jUserInfo["level"].ToString()) : 0;
                            qqUser.IsYellowVip = jUserInfo["is_yellow_vip"] != null ? (jUserInfo["is_yellow_vip"].ToString() == "1" ? true : false) : false;
                            qqUser.YellowVipLevel = jUserInfo["yellow_vip_level"] != null ? int.Parse(jUserInfo["yellow_vip_level"].ToString()) : 0;
                            qqUser.AccessToken = jAuthInfo["access_token"] != null ? jAuthInfo["access_token"].ToString() : string.Empty;
                            qqUser.RefreshToken = jAuthInfo["refresh_token"] != null ? jAuthInfo["refresh_token"].ToString() : string.Empty;
                            qqUser.ExpiresIn = jAuthInfo["expires_in"] != null ? DateTime.Now.AddMinutes(double.Parse(jAuthInfo["expires_in"].ToString())) : DateTime.Now;
                            qqUser.ClientIP = context.Request.UserHostAddress;

                            context.Session["QQUser"] = qqUser;

                            //如果是新用户
                            if (isNewUser)
                            {
                                //如果credentials中指定了此用户的QQ openid则加入管理员组，并设置认证凭据跳转
                                if (FormsAuthentication.Authenticate(qqUser.OpenID, qqUser.OpenID))
                                {
                                    if (!Roles.IsUserInRole(qqUser.OpenID, Config.AdminRoleName))
                                    {
                                        Roles.AddUserToRole(qqUser.OpenID, Config.AdminRoleName);
                                    }
                                    //用QQ用户的openid生成认证凭证和cookies，跳转到defaultUrl
                                    FormsAuthentication.RedirectFromLoginPage(qqUser.OpenID, false);
                                }
                                else
                                {
                                    //如果credentials中没有指定此QQ用户，则不做认证凭据，加入访客组，并跳转到登录页面
                                    if (!Roles.IsUserInRole(qqUser.OpenID, Config.GuestRoleName))
                                    {
                                        Roles.AddUserToRole(qqUser.OpenID, Config.GuestRoleName);
                                    }
                                    FormsAuthentication.RedirectToLoginPage();
                                }
                            }
                            else
                            {
                                //如果是老用户，已有角色身份，则直接跳转到defaultUrl
                                FormsAuthentication.RedirectFromLoginPage(qqUser.OpenID, false);
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
            FormsAuthentication.RedirectToLoginPage("ErrMsg=" + ex.Message);
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}