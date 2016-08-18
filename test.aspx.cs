using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;
using SMS;

public partial class test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string apiTicket, cardSign, nonceStr, timeStamp;
        apiTicket = WxJSAPI.GetAPITicket();
        cardSign = WxJSAPI.MakeCardSign(apiTicket, out nonceStr, out timeStamp);

        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWxCard", string.Format("var cardParam={{cardSign:'{0}',timestamp:'{1}',nonceStr:'{2}',signType:'SHA1'}};", cardSign, timeStamp, nonceStr), true);

        string str = "{\"params\":{\"cartName\":\"mahui.me_Cart\",\"isPersist\":true},\"name\":\"mahui\",\"phone\":\"133\",\"address\":\"SH\",\"memo\":\"\",\"paymentTerm\":1,\"usedMemberPoints\":0,\"validMemberPoints\":451,\"memberPointsExchangeRate\":20,\"freightTerm\":{\"freight\":0.01,\"freightFreeCondition\":99},\"wxCard\":{\"cardId\":\"p5gbrsgA9Z4-dGkUR3RvKRGJ1-dE\",\"encryptCode\":\"Yr71SOCw4ZK3msiqR0g8etVfH9RmdrWEk2gAY8RxzDo=\",\"cardType\":4,\"title\":\"满2元立减1元\",\"leastCost\":2,\"reduceCost\":1},\"prodItems\":[{\"prodID\":54,\"prodName\":\"测试商品\",\"prodDesc\":\"测试商品描述\",\"prodImg\":\"images/FruitU.jpg\",\"price\":2,\"qty\":1,\"inventoryQty\":-1}]}";
        JsonData jd = JsonMapper.ToObject(str);
    }

    protected void btnSendSMS_Click(object sender, EventArgs e)
    {
        SMS.HWSMS hwSMS = new HWSMS();
        string returnCode, description;
        List<HWSMS> listHWSMS;
        listHWSMS = hwSMS.SendSMS(this.txtBody.Text.Trim(), out returnCode, out description, null, this.txtTo.Text.Trim());

    }
}