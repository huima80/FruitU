using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using LitJson;

public partial class admin_MasterPage : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (HttpContext.Current.User.Identity.IsAuthenticated && Roles.IsUserInRole(Config.AdminRoleName))
        {
            Label lblNickName = this.LoginView1.FindControl("lblNickName") as Label;
            Image imgQQImg = this.LoginView1.FindControl("imgQQImg") as Image;

            if (lblNickName != null && imgQQImg != null && Session["QQUser"] != null)
            {
                QQUser qqUser = Session["QQUser"] as QQUser;
                if (qqUser != null)
                {
                    imgQQImg.ImageUrl = qqUser.FigureUrlQQ1;
                    lblNickName.Text = qqUser.NickName;
                }
            }
        }
        else
        {
            FormsAuthentication.RedirectToLoginPage();
        }
    }

    protected void lbLogout_Click(object sender, EventArgs e)
    {
        Session["QQUser"] = null;
        FormsAuthentication.SignOut();
        FormsAuthentication.RedirectToLoginPage();
    }
}
