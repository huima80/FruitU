<%@ WebHandler Language="C#" Class="CheckWxCard" %>

using System;
using System.Web;
using System.Collections.Generic;
using LitJson;

public class CheckWxCard : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
    public void ProcessRequest(HttpContext context)
    {
        string jWxCard = string.Empty;

        try
        {
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            if (wxUser == null || string.IsNullOrEmpty(wxUser.OpenID))
            {
                throw new Exception("请登录");
            }

            //查询用户的微信卡券
            List<WxCard> wxCard = WxCard.GetCardList(wxUser.OpenID);
            jWxCard = JsonMapper.ToJson(wxCard);
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.Write(jWxCard);
            context.Response.End();
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