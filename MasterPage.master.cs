using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;

public partial class MasterPage : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //引导用户进行微信登录授权
        if (Session["WxUser"] == null)
        {
            if (Request.Url.Host.ToLower().IndexOf("localhost") != -1)
            {
                //测试数据
                string strAuth = "{\"access_token\":\"ACCESS_TOKEN\",\"expires_in\":7200,\"refresh_token\":\"REFRESH_TOKEN\",\"openid\":\"o5gbrsixFkd1G6eszfG5mN-WbMeE\",\"scope\":\"snsapi_userinfo\",\"unionid\": \"o6_bmasdasdsad6_2sgVt7hMZOPfL\"}";
                JsonData jAuthInfo = JsonMapper.ToObject(strAuth);
                string strUserInfo = "{\"openid\":\"o5gbrsixFkd1G6eszfG5mN-WbMeE\",\"nickname\":\"Mehmet_DEBUG\",\"sex\":\"1\",\"language\":\"zh_CN\",\"city\":\"南京\",\"province\":\"江苏\",\"country\":\"中国\",\"headimgurl\":\"http://wx.qlogo.cn/mmopen/ajNVdqHZLLAp3L8QaUmuaQtNibOMeJBmnEd5mu6WSWKx0YNWHiaGcVEkgPN8rhLPO7ZUe4JHQ09K2Ug5ibPWQARGw/0\",\"privilege\":[]}";
                JsonData jUserInfo = JsonMapper.ToObject(strUserInfo);

                WeChatUser wxUser = new WeChatUser();
                wxUser.OpenID = jUserInfo["openid"].ToString();
                wxUser.NickName = jUserInfo["nickname"].ToString();
                wxUser.Sex = jUserInfo["sex"].ToString() == "1" ? true : false;
                wxUser.Country = jUserInfo["country"].ToString();
                wxUser.Province = jUserInfo["province"].ToString();
                wxUser.City = jUserInfo["city"].ToString();
                wxUser.HeadImgUrl = jUserInfo["headimgurl"].ToString();
                wxUser.Privilege = jUserInfo["privilege"].ToString();
                wxUser.ClientIP = "127.0.0.1";

                wxUser.AccessTokenForBase = jAuthInfo["access_token"].ToString();
                wxUser.RefreshTokenForBase = jAuthInfo["refresh_token"].ToString();
                wxUser.ExpireOfAccessTokenForBase = DateTime.Now.AddSeconds(double.Parse(jAuthInfo["expires_in"].ToString()));
                wxUser.Scope = WeChatAuthScope.snsapi_userinfo;

                Session["WxUser"] = wxUser;
            }
            else
            {
                Response.Redirect("~/wxauth.ashx?scope=snsapi_userinfo&state=" + Request.Url.ToString());
            }
        }

    }
}
