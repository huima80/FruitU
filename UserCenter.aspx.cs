using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;

public partial class UserCenter : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        WeChatUser wxUser;
        string cardSign = string.Empty, timeStamp = string.Empty, nonceStr = string.Empty;

        try
        {
            wxUser = Session["WxUser"] as WeChatUser;

            //string authUrl;
            //string redirectUri = Request.Url.AbsoluteUri;

            ////如果wxUser中不包含snsapi_base模式授权的token或token已超时，则发起snsapi_base授权
            //if (string.IsNullOrEmpty(wxUser.AccessTokenForBase) || DateTime.Now >= wxUser.ExpireOfAccessTokenForBase)
            //{
            //    if (Request.QueryString["CODE"] == null)
            //    {
            //        authUrl = String.Format(@"https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state=STATE#wechat_redirect",
            //            Config.APPID,
            //            HttpUtility.UrlEncode(redirectUri),
            //            "snsapi_base");

            //        Response.Redirect(authUrl);
            //    }
            //    else
            //    {
            //        authUrl = String.Format(@"https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code",
            //            Config.APPID,
            //            Config.APPSECRET,
            //            Request.QueryString["CODE"]);

            //        string strAuth = HttpService.Get(authUrl);
            //        JsonData jAccessToken = JsonMapper.ToObject(strAuth);

            //        if (jAccessToken != null && jAccessToken is JsonData && jAccessToken.Keys.Contains("access_token") && jAccessToken.Keys.Contains("refresh_token") && jAccessToken.Keys.Contains("expires_in"))
            //        {
            //            wxUser.AccessTokenForBase = jAccessToken["access_token"].ToString();
            //            wxUser.RefreshTokenForBase = jAccessToken["refresh_token"].ToString();
            //            wxUser.ExpireOfAccessTokenForBase = DateTime.Now.AddSeconds(double.Parse(jAccessToken["expires_in"].ToString()));
            //        }
            //        else
            //        {
            //            throw new Exception("snsapi_base模式认证失败");
            //        }

            //    }
            //}

            ////获取“收货地址共享接口参数”，传给前端JS
            //wxEditAddrParam = WxJSAPI.MakeEditAddressJsParam(wxUser.AccessTokenForBase, redirectUri);

            //获取最新的用户积分信息
            wxUser.MemberPoints = WeChatUserDAO.FindMemberPointsByOpenID(wxUser.OpenID);

            //生成微信卡券签名，用于客户端调用微信卡券JSSDK
            string apiTicket;
            apiTicket = WxJSAPI.GetAPITicket();
            cardSign = WxJSAPI.MakeCardSign(apiTicket, out nonceStr, out timeStamp);

            ////开始：显示当前微信用户信息：用户头像、昵称、特权、积分
            this.imgPortrait.ImageUrl = wxUser.HeadImgUrl;
            this.lblNickName.Text = wxUser.NickName;
            this.lblPrivilege.Text = wxUser.Privilege;
            this.lblMemberPoints.Text = string.Format("{0}（={1}元）", wxUser.MemberPoints, (decimal)wxUser.MemberPoints / Config.MemberPointsExchangeRate);
            this.lblMemberPointsExchageRate.Text = Config.MemberPointsExchangeRate.ToString();
            ////结束：显示auth.ashx鉴权获取的微信用户信息

            //定义前端JS全局变量：微信卡券JS参数
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWxCard", string.Format("var wxCardParam={{cardSign:'{0}',timestamp:'{1}',nonceStr:'{2}',signType:'SHA1'}};", cardSign, timeStamp, nonceStr), true);

        }
        catch (System.Threading.ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
            throw ex;
        }
    }
}