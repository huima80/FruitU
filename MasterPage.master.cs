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
}
