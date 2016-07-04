using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;
using System.Text.RegularExpressions;

public partial class MasterPage : System.Web.UI.MasterPage
{
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);

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

                wxUser.AccessTokenForUserInfo = jAuthInfo["access_token"].ToString();
                wxUser.RefreshTokenForUserInfo = jAuthInfo["refresh_token"].ToString();
                wxUser.ExpireOfAccessTokenForUserInfo = DateTime.Now.AddSeconds(double.Parse(jAuthInfo["expires_in"].ToString()));
                wxUser.Scope = WeChatAuthScope.snsapi_userinfo;

                Session["WxUser"] = wxUser;
            }
            else
            {
                //如果请求页面是支付宝支付，需要在外部浏览器打开页面，则不进行微信认证
                if (Request.Url.AbsolutePath.ToLower().IndexOf("alipay") == -1)
                {
                    Response.Redirect("~/wxauth.ashx?scope=snsapi_userinfo&state=" + Request.Url.ToString());
                }
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["WxUser"] != null)
        {
            WeChatUser wxUser = Session["WxUser"] as WeChatUser;
            string jsTicket, jsSign, timestamp, nonceStr, url, jWxCard;

            url = Request.Url.ToString().Split('#')[0];
            jsTicket = WxJSAPI.GetJsAPITicket();
            jsSign = WxJSAPI.MakeJsAPISign(jsTicket, url, out nonceStr, out timestamp);

            //处理用户的推荐人
            string agentOpenID = Request.QueryString["AgentOpenID"];
            if (!string.IsNullOrEmpty(agentOpenID) && agentOpenID != wxUser.OpenID)
            {
                wxUser.AgentOpenID = agentOpenID;
            }

            //查询用户的微信卡券
            List<WxCard> wxCard = WxCard.GetCardList(wxUser.OpenID);
            jWxCard = JsonMapper.ToJson(wxCard);

            //注册JS变量openID，用于用户分享页面时带上自己的OpenID；微信卡券
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "openID", string.Format("var openID = '{0}';", wxUser.OpenID), true);
            //注册JS变量wxCard，微信卡券
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "wxCard", string.Format("var wxCard = {0};", jWxCard), true);
            //注册JS变量wxJsApiParam，用于调用微信的JS SDK
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "wxJSAPI", string.Format("var wxJsApiParam = {{appId:'{0}', timestamp:'{1}', nonceStr:'{2}', signature:'{3}'}};", Config.APPID, timestamp, nonceStr, jsSign), true);
            //注册JS变量webConfigServer，用于用户分享页面时设置页面title等信息
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "webConfig", string.Format("var webConfigServer = {{siteDomain:'{0}',siteTitle:'{1}',siteDesc:'{2}',siteKeywords:'{3}',siteIcon:'{4}',siteCopyrights:'{5}',defaultImg:'{6}'}};", Request.Url.Scheme + "://" + Request.Url.Host, Config.SiteTitle, Config.SiteDesc, Config.SiteKeywords, Config.SiteIcon, Config.SiteCopyrights, Config.DefaultImg), true);
        }
    }
}
