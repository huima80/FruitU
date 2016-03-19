using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;

public partial class login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        this.SiteTitle.InnerText = Config.SiteTitle;
        this.SiteDesc.InnerText = Config.SiteDesc;

        //QQ用户已登录，但没有访问权限
        if (Session["QQUserInfo"] != null && !HttpContext.Current.User.Identity.IsAuthenticated)
        {
            JsonData jQQUserInfo = Session["QQUserInfo"] as JsonData;
            this.lblMsg.Text = jQQUserInfo["nickname"].ToString() + "，您没有权限登录，请联系管理员。";
            this.lblMsg.Visible = true;
        }

    }
}