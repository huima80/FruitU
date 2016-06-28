using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using LitJson;

public partial class login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        this.SiteTitle.InnerText = Config.SiteTitle;

        //QQ用户已登录，但不是管理员
        if (User.Identity.IsAuthenticated &&
            !User.IsInRole(Config.AdminRoleName) &&
            Session["QQUser"] != null)
        {
            QQUser qqUser = Session["QQUser"] as QQUser;
            this.lblMsg.Text = qqUser.NickName + "，您没有权限登录，请联系管理员。";
            this.lblMsg.Visible = true;
        }
        else
        {
            if(!string.IsNullOrEmpty(Request.QueryString["ErrMsg"]))
            {
                this.lblMsg.Text = Request.QueryString["ErrMsg"];
                this.lblMsg.Visible = true;
            }
        }
    }
}