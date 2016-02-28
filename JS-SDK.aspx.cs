using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class JS_SDK : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string token, jsTicket, jsSign, timestamp, nonceStr, url;

        url = Request.Url.ToString().Split('#')[0];
        token = WxJSAPI.GetAccessToken();
        jsTicket = WxJSAPI.GetJsAPITicket(token);
        jsSign = WxJSAPI.MakeJsAPISign(jsTicket, url, out nonceStr, out timestamp);

        //向前端页面注册JS变量，用于调用微信客户端JS-API
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "wxJSAPI", string.Format("var appId = '{0}', timestamp = '{1}', nonceStr = '{2}', signature = '{3}';", Config.APPID, timestamp, nonceStr, jsSign), true);
    }
}