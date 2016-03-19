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
            string authUrl;
            string redirectUri = Request.Url.AbsoluteUri;
            if (Session["WxAuthInfo"] == null)
            {
                Response.Redirect("default.aspx");
            }

            JsonData jWxAuthInfo = Session["WxAuthInfo"] as JsonData;
            //如果Session["WxAuthInfo"]中不包含snsapi_base模式授权的token，则发起授权，并存入session键access_token_base，并记录获取的时间
            if (!jWxAuthInfo.Keys.Contains("access_token_base") || string.IsNullOrEmpty(jWxAuthInfo["access_token_base"].ToString()))
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

                    if (jAccessToken != null && jAccessToken is JsonData && jAccessToken["access_token"] != null)
                    {
                        jWxAuthInfo["access_token_base"] = jAccessToken["access_token"].ToString();
                        jWxAuthInfo["token_base_time"] = DateTime.Now.ToString("yyyyMMddHHmmss");
                    }
                    else
                    {
                        throw new Exception("snsapi_base模式认证失败");
                    }

                }
            }

            //校验session中的access_token_base是否超时
            double tokenExpire = double.Parse(jWxAuthInfo["expires_in"].ToString());
            DateTime tokenTime = DateTime.ParseExact(jWxAuthInfo["token_base_time"].ToString(), "yyyyMMddHHmmss", null);
            if (DateTime.Now >= tokenTime.AddSeconds(tokenExpire))
            {
                //如果token超时，则清除token，重定向到本页，授权获取新的token
                jWxAuthInfo["access_token_base"] = string.Empty;
                Response.Redirect(Request.Url.ToString());
            }
            else
            {
                //获取“收货地址共享接口参数”，传给前端JS
                string wxEditAddrParam = WxJSAPI.MakeEditAddressJsParam(jWxAuthInfo["access_token_base"].ToString(), redirectUri);

                ScriptManager.RegisterStartupScript(Page, this.GetType(), "wxParam", string.Format("var wxEditAddrParam = {0};", wxEditAddrParam), true);
            }

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