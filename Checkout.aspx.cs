using System;
using System.Web;
using System.Web.UI;
using LitJson;

public partial class Checkout : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //string authUrl;
            //string redirectUri = Request.Url.AbsoluteUri;

            ////当前session中的认证信息
            //WeChatUser wxUser = Session["WxUser"] as WeChatUser;

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

            //        if (jAccessToken != null && jAccessToken is JsonData && jAccessToken["access_token"] != null)
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
            //string wxEditAddrParam = WxJSAPI.MakeEditAddressJsParam(wxUser.AccessTokenForBase, redirectUri);

            //ScriptManager.RegisterStartupScript(Page, this.GetType(), "wxAddrParam", string.Format("var wxEditAddrParam = {0};", wxEditAddrParam), true);

        }
        catch (System.Threading.ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
        }
    }
}