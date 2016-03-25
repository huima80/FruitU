<%@ Application Language="C#"%>
<%@ Import Namespace="System.Web.Security" %>
<%@ Import Namespace="LitJson" %>

<script runat="server">

    void Application_Start(object sender, EventArgs e)
    {
        // 在应用程序启动时运行的代码
        if(!Roles.RoleExists(Config.AdminRoleName))
        {
            Roles.CreateRole(Config.AdminRoleName);
        }
        if(!Roles.RoleExists(Config.GuestRoleName))
        {
            Roles.CreateRole(Config.GuestRoleName);
        }

    }

    void Application_End(object sender, EventArgs e)
    {
        //  在应用程序关闭时运行的代码

    }

    void Application_Error(object sender, EventArgs e)
    {
        // 在出现未处理的错误时运行的代码

    }

    void Application_BeginRequest(object sender, EventArgs e)
    {
    }

    void Session_Start(object sender, EventArgs e)
    {
        // 在新会话启动时运行的代码

        if (Session["WxAuthInfo"] == null || Session["WxUserInfo"] == null)
        {
            if (Request.Url.Host.ToLower().IndexOf("localhost") != -1)
            {
                string strAuth = "{\"access_token\":\"ACCESS_TOKEN\",\"expires_in\":7200,\"refresh_token\":\"REFRESH_TOKEN\",\"openid\":\"o5gbrsixFkd1G6eszfG5mN-WbMeE\",\"scope\":\"snsapi_userinfo\",\"unionid\": \"o6_bmasdasdsad6_2sgVt7hMZOPfL\"}";
                JsonData jAuthInfo = JsonMapper.ToObject(strAuth);
                //在json中添加用户的IP信息
                jAuthInfo["client_ip"] = "127.0.0.1";
                jAuthInfo["access_token_base"] = "ACCESS_TOKEN_BASE";
                jAuthInfo["token_base_time"] = DateTime.Now.ToString("yyyyMMddHHmmss");
                Session["WxAuthInfo"] = jAuthInfo;

                string strUserInfo = "{\"openid\":\"o5gbrsixFkd1G6eszfG5mN-WbMeE\",\"nickname\":\"Mehmet_DEBUG\",\"sex\":1,\"language\":\"zh_CN\",\"city\":\"南京\",\"province\":\"江苏\",\"country\":\"中国\",\"headimgurl\":\"http://wx.qlogo.cn/mmopen/ajNVdqHZLLAp3L8QaUmuaQtNibOMeJBmnEd5mu6WSWKx0YNWHiaGcVEkgPN8rhLPO7ZUe4JHQ09K2Ug5ibPWQARGw/0\",\"privilege\":[]}";
                JsonData jUserInfo = JsonMapper.ToObject(strUserInfo);
                Session["WxUserInfo"] = jUserInfo;
            }
            else
            {
                Response.Redirect("~/wxauth.ashx?scope=snsapi_userinfo&state=" + Request.Url.ToString());
            }
        }
    }

    void Session_End(object sender, EventArgs e)
    {
        // 在会话结束时运行的代码。 
        // 注意: 只有在 Web.config 文件中的 sessionstate 模式设置为
        // InProc 时，才会引发 Session_End 事件。如果会话模式设置为 StateServer
        // 或 SQLServer，则不引发该事件。

    }

</script>
