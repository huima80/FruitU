using System;
using System.Web;
using System.Web.UI;
using LitJson;

public partial class Checkout : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //获取“微信收货地址共享接口参数”
        string wxEditAddrParam = string.Empty;
        WeChatUser wxUser = Session["WxUser"] as WeChatUser;

        try
        {
            string authUrl;
            string redirectUri = Request.Url.AbsoluteUri;

            //在微信用户表和成员资格表中查找此用户信息，并刷新最近活动时间，获取最新的用户积分信息
            Session["WxUser"] = wxUser = WeChatUserDAO.FindUserByOpenID(wxUser.OpenID, true);

            //如果wxUser中不包含snsapi_base模式授权的token或token已超时，则发起snsapi_base授权
            if (string.IsNullOrEmpty(wxUser.AccessTokenForBase) || DateTime.Now >= wxUser.ExpireOfAccessTokenForBase)
            {
                if (Request.QueryString["CODE"] == null)
                {
                    authUrl = String.Format(@"https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope={2}&state=STATE#wechat_redirect",
                        Config.APPID,
                        HttpUtility.UrlEncode(redirectUri),
                        "snsapi_base");

                    Response.Redirect(authUrl);
                }
                else
                {
                    authUrl = String.Format(@"https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code",
                        Config.APPID,
                        Config.APPSECRET,
                        Request.QueryString["CODE"]);

                    string strAuth = HttpService.Get(authUrl);
                    JsonData jAccessToken = JsonMapper.ToObject(strAuth);

                    if (jAccessToken != null && jAccessToken is JsonData && jAccessToken.Keys.Contains("access_token") && jAccessToken.Keys.Contains("refresh_token") && jAccessToken.Keys.Contains("expires_in"))
                    {
                        wxUser.AccessTokenForBase = jAccessToken["access_token"].ToString();
                        wxUser.RefreshTokenForBase = jAccessToken["refresh_token"].ToString();
                        wxUser.ExpireOfAccessTokenForBase = DateTime.Now.AddSeconds(double.Parse(jAccessToken["expires_in"].ToString()));
                    }
                    else
                    {
                        throw new Exception("snsapi_base模式认证失败");
                    }

                }
            }

            wxEditAddrParam = WxJSAPI.MakeEditAddressJsParam(wxUser.AccessTokenForBase, redirectUri);

        }
        catch (System.Threading.ThreadAbortException)
        {
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
        }
        finally
        {
            //定义前端JS全局变量：收货地址接口参数、会议积分兑换比率、会员积分余额
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsVar", string.Format("var wxEditAddrParam = {0}, memberPointsExchangeRate = {1}, validMemberPoints = {2};", !string.IsNullOrEmpty(wxEditAddrParam) ? wxEditAddrParam : "undefined", Config.MemberPointsExchangeRate, wxUser.MemberPoints), true);
        }
    }
}