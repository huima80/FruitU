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
        try
        {
            string authUrl;
            string redirectUri = Request.Url.AbsoluteUri;

            if (Session["AuthInfo"] != null && Session["AuthInfo"] is JsonData)
            {
                JsonData jAuthInfo = Session["AuthInfo"] as JsonData;

                //如果Session["AuthInfo"]中不包含snsapi_base模式授权的token，则发起授权，并存入session键access_token_base，并记录获取的时间
                if (!jAuthInfo.Keys.Contains("access_token_base") || string.IsNullOrEmpty(jAuthInfo["access_token_base"].ToString()))
                {
                    ////开始：鉴权获取微信用户地址编辑JS参数
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
                            jAuthInfo["access_token_base"] = jAccessToken["access_token"].ToString();
                            jAuthInfo["token_base_time"] = DateTime.Now.ToString("yyyyMMddHHmmss");
                        }
                        else
                        {
                            throw new Exception("snsapi_base模式认证失败");
                        }

                    }
                    ////结束：鉴权获取微信用户地址编辑JS参数////
                }

                //校验session中的access_token_base是否超时
                double tokenExpire = double.Parse(jAuthInfo["expires_in"].ToString());
                DateTime tokenTime = DateTime.ParseExact(jAuthInfo["token_base_time"].ToString(), "yyyyMMddHHmmss", null);
                if (DateTime.Now >= tokenTime.AddSeconds(tokenExpire))
                {
                    //如果token超时，则清除token，重定向到本页，授权获取新的token
                    jAuthInfo["access_token_base"] = string.Empty;
                    Response.Redirect(Request.Url.ToString());
                }
                else
                {
                    //如果session中的token有效，生成“收货地址共享接口参数”，传给前端JS
                    string wxEditAddrParam = WxJSAPI.MakeEditAddressJsParam(jAuthInfo["access_token_base"].ToString(), redirectUri);

                    ScriptManager.RegisterStartupScript(Page, this.GetType(), "wxParam", string.Format("var wxEditAddrParam = {0};", wxEditAddrParam), true);
                }
            }
            else
            {
                Response.Redirect("auth.ashx?scope=snsapi_userinfo&state=" + Request.Url.ToString());
            }

            ////开始：显示auth.ashx鉴权获取的微信用户信息
            if (Session["UserInfo"] != null && Session["UserInfo"] is JsonData)
            {
                JsonData jUserInfo = Session["UserInfo"] as JsonData;

                //显示用户头像、昵称、特权
                this.imgPortrait.ImageUrl = jUserInfo["headimgurl"].ToString();
                this.lblNickName.Text = jUserInfo["nickname"].ToString();
                if (jUserInfo["privilege"].IsArray && jUserInfo["privilege"].Count != 0)
                {
                    string privilege = "";
                    for (int i = 0; i < jUserInfo["privilege"].Count; i++)
                    {
                        privilege += jUserInfo["privilege"][i] + ",";
                    }
                    this.lblPrivilege.Text = "【" + privilege.TrimEnd(',') + "】";
                }
            }
            else
            {
                Response.Redirect("auth.ashx?scope=snsapi_userinfo&state=" + Request.Url.ToString());
            }
            ////结束：显示auth.ashx鉴权获取的微信用户信息

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