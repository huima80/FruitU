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
        if (HttpContext.Current.User.Identity.IsAuthenticated)
        {
            this.lblNickName.Text = HttpContext.Current.User.Identity.Name;

            if (Session["QQUserInfo"] != null)
            {
                JsonData jUserInfo = Session["QQUserInfo"] as JsonData;
                this.imgQQImg.ImageUrl = jUserInfo["figureurl_qq_1"].ToString();
            }
        }
        else
        {
            FormsAuthentication.RedirectToLoginPage();
        }

    }

    protected void lbLogout_Click(object sender, EventArgs e)
    {
        Session["QQUserInfo"] = null;
        FormsAuthentication.SignOut();
        FormsAuthentication.RedirectToLoginPage();
    }
}
