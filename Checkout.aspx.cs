using System;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using LitJson;
using Com.Alipay;
using System.Web.UI.WebControls;

public partial class Checkout : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        WeChatUser wxUser;
        string cardSign = string.Empty, timeStamp = string.Empty, nonceStr = string.Empty;

        try
        {
            wxUser = Session["WxUser"] as WeChatUser;

            //获取“微信收货地址共享接口参数”，已过时，改用JSAPI openAddress
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

            //定义前端JS全局变量：会员积分兑换比率、会员积分余额
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsMemberPoints", string.Format("var memberPointsExchangeRate = {0}, validMemberPoints = {1};", Config.MemberPointsExchangeRate, wxUser.MemberPoints), true);
            //定义前端JS全局变量：运费标准、免运费条件
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsFreightTerm", string.Format("var freight = {0}, freightFreeCondition = {1};", Config.Freight, Config.FreightFreeCondition), true);
            //定义前端JS全局变量：支付方式枚举值、支付宝网关
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsPaymentTerm", string.Format("var paymentTerm={{wechat:{0},alipay:{1},cash:{2}}}, apGateway = '{3}';", (int)PaymentTerm.WECHAT, (int)PaymentTerm.ALIPAY, (int)PaymentTerm.CASH, AliPayConfig.AliPayGateway), true);
            //定义前端JS全局变量：微信卡券JS参数、微信地址JS参数
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWxJSParam", string.Format("var wxCardParam={{cardSign:'{0}',timestamp:'{1}',nonceStr:'{2}',signType:'SHA1'}};", cardSign, timeStamp, nonceStr), true);
            //定义前端JS全局变量：微信卡券CardType枚举值
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsCardType", string.Format("var wxCardType={{cash:{0},discount:{1},groupon:{2},gift:{3},generalCoupon:{4},memberCard:{5},scenicTicket:{6},movieTicket:{7},boardingPass:{8},meetingTicket:{9},busTicket:{10}}};", (int)WxCardType.CASH, (int)WxCardType.DISCOUNT, (int)WxCardType.GROUPON, (int)WxCardType.GIFT, (int)WxCardType.GENERAL_COUPON, (int)WxCardType.MEMBER_CARD, (int)WxCardType.SCENIC_TICKET, (int)WxCardType.MOVIE_TICKET, (int)WxCardType.BOARDING_PASS, (int)WxCardType.MEETING_TICKET, (int)WxCardType.BUS_TICKET), true);

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