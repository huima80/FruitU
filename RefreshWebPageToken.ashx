<%@ WebHandler Language="C#" Class="RefreshWebPageToken" %>

using System;
using System.Web;
using LitJson;

public class RefreshWebPageToken : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    public void ProcessRequest(HttpContext context)
    {
        if (context.Session["WxUser"] != null)
        {
            string url, strAuthInfo;
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            url = String.Format(@"https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={0}&grant_type=refresh_token&refresh_token={1}",
                Config.APPID,
                wxUser.RefreshTokenForUserInfo);

            strAuthInfo = HttpService.Get(url);

            //微信返回正确的json数据
            //{
            //"access_token":"ACCESS_TOKEN",
            //"expires_in":7200,
            //"refresh_token":"REFRESH_TOKEN",
            //"openid":"OPENID",
            //"scope":"SCOPE"
            //}
            JsonData jAuthInfo = JsonMapper.ToObject(strAuthInfo);
            if (jAuthInfo["access_token"] != null)
            {
                wxUser.AccessTokenForUserInfo = jAuthInfo["access_token"].ToString();

                if (wxUser.Scope == WeChatAuthScope.snsapi_userinfo)
                {
                    //进一步刷新用户信息
                }

                context.Session["WxUser"] = wxUser;
            }
            else
            {
                //refresh_token失效后，需要用户重新授权
                context.Response.Redirect("auth.ashx?scope=user_info");
            }

        }
        else
        {
            //session已过期，重新获取oauth code
            context.Response.Redirect("auth.ashx?scope=user_info");

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