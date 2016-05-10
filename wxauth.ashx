<%@ WebHandler Language="C#" Class="WxAuth" %>

using System;
using System.Web;
using System.Web.Security;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using LitJson;

/// <summary>
/// 微信支付认证：
/// 1，向微信支付oauth认证接口发出CODE请求，微信支付返回CODE回调本页面
/// 2，用CODE向微信支付的access token接口发出请求，获取openid和access token，存入session
/// 3，如果是snsapi_userinfo授权模式，则使用openid和access token向微信支付user_info接口发出请求，获取userinfo，存入session
/// 4，根据state参数跳回原页面
/// </summary>
public class WxAuth : IHttpHandler, System.Web.SessionState.IRequiresSessionState
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

                //微信返回错误的json数据
                //{
                //    "errcode":40029,"errmsg":"invalid code, hints: [ req_id: FJ7dwa0036ns57 ]"
                //}

                strAuth = HttpService.Get(authUrl);

                JsonData jAuthInfo = JsonMapper.ToObject(strAuth);

                Log.Info("strAuth", strAuth);

                if (jAuthInfo != null && jAuthInfo.Keys.Contains("openid") && jAuthInfo.Keys.Contains("access_token"))
                {
                    bool isNewUser;

                    //在微信用户表和成员资格表中查找此用户信息，并刷新最近活动时间
                    WeChatUser wxUser = WeChatUserDAO.FindUserByOpenID(jAuthInfo["openid"].ToString(), true);

                    //此用户在微信用户表中存在，但在成员资格表中不存在，则删除微信用户表数据，后续再重建
                    if (wxUser != null && wxUser.ProviderUserKey == null)
                    {
                        WeChatUserDAO.DeleteUserByOpenID(jAuthInfo["openid"].ToString());
                    }

                    //如果此微信用户在微信用户表中不存在，则新建用户
                    if (wxUser == null)
                    {
                        isNewUser = true;

                        //使用微信用户的openid和随机密码新建成员资格用户
                        MembershipUser mUser = Membership.CreateUser(jAuthInfo["openid"].ToString(), Membership.GeneratePassword(10, 1));

                        if (mUser != null)
                        {
                            //使用新建的成员资格对象初始化微信用户对象
                            wxUser = new WeChatUser(mUser);
                        }
                        else
                        {
                            wxUser = new WeChatUser();
                        }
                    }
                    else
                    {
                        //是老用户，在微信用户表和成员资格表中都有其数据
                        isNewUser = false;
                        if (!wxUser.IsApproved)
                        {
                            throw new Exception("您没有权限登录，请联系微信客服申诉。");
                        }
                    }

                    //无论新老用户，都要使用当前的微信用户信息刷新数据库。微信用户关键信息：OpenID和客户端IP，后续微信支付时需要！！！
                    wxUser.OpenID = jAuthInfo["openid"].ToString();
                    wxUser.ClientIP = !string.IsNullOrEmpty(context.Request.UserHostAddress) ? context.Request.UserHostAddress : "127.0.0.1";

                    //处理用户的推荐人，排除自己推荐自己的情况
                    wxUser.AgentOpenID = string.Empty;
                    Match rxAgentOpenID = Regex.Match(urlReferrer, @"AgentOpenID=([^&]*)");
                    if (rxAgentOpenID != null && rxAgentOpenID.Groups.Count == 2 && rxAgentOpenID.Groups[1].Value != wxUser.OpenID)
                    {
                        wxUser.AgentOpenID = rxAgentOpenID.Groups[1].Value;
                    }

                    if (jAuthInfo["scope"].ToString().ToLower() == "snsapi_base")
                    {
                        //微信用户网页授权access_token，后续调用编辑地址信息JSSDK时需要
                        wxUser.AccessTokenForBase = jAuthInfo.Keys.Contains("access_token") ? jAuthInfo["access_token"].ToString() : string.Empty;
                        wxUser.RefreshTokenForBase = jAuthInfo.Keys.Contains("refresh_token") ? jAuthInfo["refresh_token"].ToString() : string.Empty;
                        wxUser.ExpireOfAccessTokenForBase = jAuthInfo.Keys.Contains("expires_in") ? DateTime.Now.AddSeconds(double.Parse(jAuthInfo["expires_in"].ToString())) : DateTime.Now;
                        wxUser.Scope = WeChatAuthScope.snsapi_base;
                    }
                    else
                    {
                        wxUser.AccessTokenForUserInfo = jAuthInfo.Keys.Contains("access_token") ? jAuthInfo["access_token"].ToString() : string.Empty;
                        wxUser.RefreshTokenForUserInfo = jAuthInfo.Keys.Contains("refresh_token") ? jAuthInfo["refresh_token"].ToString() : string.Empty;
                        wxUser.ExpireOfAccessTokenForUserInfo = jAuthInfo.Keys.Contains("expires_in") ? DateTime.Now.AddSeconds(double.Parse(jAuthInfo["expires_in"].ToString())) : DateTime.Now;
                        wxUser.Scope = WeChatAuthScope.snsapi_userinfo;

                        //如果是snsapi_userinfo授权模式，则进一步获取用户信息

                        authUrl = String.Format(@"https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN",
                                            jAuthInfo["access_token"].ToString(),
                                            jAuthInfo["openid"].ToString());

                        //微信返回正确的json数据
                        //{
                        //"openid":" OPENID",
                        //"nickname": NICKNAME,
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

                        if (jUserInfo != null && jUserInfo.Keys.Contains("openid"))
                        {
                            //使用微信用户信息进一步赋值给微信用户对象
                            wxUser.NickName = jUserInfo.Keys.Contains("nickname") ? jUserInfo["nickname"].ToString() : string.Empty;
                            if (jUserInfo["sex"] != null)
                            {
                                if (jUserInfo["sex"].ToString() == "1")
                                {
                                    wxUser.Sex = true;
                                }
                                else
                                {
                                    wxUser.Sex = false;
                                }
                            }
                            else
                            {
                                wxUser.Sex = true;
                            }
                            wxUser.Country = jUserInfo.Keys.Contains("country") ? jUserInfo["country"].ToString() : string.Empty;
                            wxUser.Province = jUserInfo.Keys.Contains("province") ? jUserInfo["province"].ToString() : string.Empty;
                            wxUser.City = jUserInfo.Keys.Contains("city") ? jUserInfo["city"].ToString() : string.Empty;
                            wxUser.HeadImgUrl = jUserInfo.Keys.Contains("headimgurl") ? jUserInfo["headimgurl"].ToString() : string.Empty;
                            if (jUserInfo.Keys.Contains("privilege") && jUserInfo["privilege"].IsArray && jUserInfo["privilege"].Count > 0)
                            {
                                List<string> listPrivilege = new List<string>();
                                for (int i = 0; i < jUserInfo["privilege"].Count; i++)
                                {
                                    listPrivilege.Add(jUserInfo["privilege"][i].ToString());
                                }
                                wxUser.Privilege = string.Join<string>(",", listPrivilege);
                            }
                        }
                    }

                    if (isNewUser)
                    {
                        //新用户的会员积分默认为0
                        wxUser.MemberPoints = 0;
                        //如果是新用户则插入微信用户表
                        WeChatUserDAO.InsertUser(wxUser);
                    }
                    else
                    {
                        //如果是老用户则更新微信用户表
                        WeChatUserDAO.UpdateUser(wxUser);
                    }

                    if (!Roles.IsUserInRole(wxUser.OpenID, Config.GuestRoleName))
                    {
                        //加入访客组
                        Roles.AddUserToRole(wxUser.OpenID, Config.GuestRoleName);
                    }

                    //存入session
                    context.Session["WxUser"] = wxUser;

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
                    context.Response.Redirect(".");
                }
            }
        }
        catch (System.Threading.ThreadAbortException)
        { }
        catch (Exception ex)
        {
            context.Response.Write(ex.Message);
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