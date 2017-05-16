using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;

public partial class test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //string apiTicket, cardSign, nonceStr, timeStamp;
        //apiTicket = WxJSAPI.GetAPITicket();
        //cardSign = WxJSAPI.MakeCardSign(apiTicket, out nonceStr, out timeStamp);

        //ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWxCard", string.Format("var cardParam={{cardSign:'{0}',timestamp:'{1}',nonceStr:'{2}',signType:'SHA1'}};", cardSign, timeStamp, nonceStr), true);

        //string str = "{\"params\":{\"cartName\":\"mahui.me_Cart\",\"isPersist\":true},\"name\":\"mahui\",\"phone\":\"133\",\"address\":\"SH\",\"memo\":\"\",\"paymentTerm\":1,\"usedMemberPoints\":0,\"validMemberPoints\":451,\"memberPointsExchangeRate\":20,\"freightTerm\":{\"freight\":0.01,\"freightFreeCondition\":99},\"wxCard\":{\"cardId\":\"p5gbrsgA9Z4-dGkUR3RvKRGJ1-dE\",\"encryptCode\":\"Yr71SOCw4ZK3msiqR0g8etVfH9RmdrWEk2gAY8RxzDo=\",\"cardType\":4,\"title\":\"满2元立减1元\",\"leastCost\":2,\"reduceCost\":1},\"prodItems\":[{\"prodID\":54,\"prodName\":\"测试商品\",\"prodDesc\":\"测试商品描述\",\"prodImg\":\"images/FruitU.jpg\",\"price\":2,\"qty\":1,\"inventoryQty\":-1}]}";
        //JsonData jd = JsonMapper.ToObject(str);
    }

    protected void btnSendSMS_Click(object sender, EventArgs e)
    {
        //SMS.HWSMS hwSMS = new HWSMS();
        //string returnCode, description;
        //List<HWSMS> listHWSMS;
        //listHWSMS = hwSMS.SendSMS(this.txtBody.Text.Trim(), out returnCode, out description, null, this.txtTo.Text.Trim());

    }

    protected void btnGetCityCode_Click(object sender, EventArgs e)
    {
        JObject jCityCode = DaDaBiz.GetCityCode();
        if (jCityCode["code"]!=null && jCityCode["code"].ToString() == "0" && jCityCode["result"]!=null)
        {
            this.ddlCityCode.Items.Clear();
            foreach (var city in jCityCode["result"])
            {
                ListItem li = new ListItem(city["cityName"].ToString(), city["cityCode"].ToString());
                li.Selected = (li.Value == "021") ? true : false;
                this.ddlCityCode.Items.Add(li);
            }
        }
    }

    protected void btnSendOrder_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(this.txtOrderID.Text))
        {
            ProductOrder po = new ProductOrder(this.txtOrderID.Text);

            if (po != null && !string.IsNullOrEmpty(po.OrderID))
            {
                DaDaOrder dadaOrder = new DaDaOrder();
                //在测试环境，使用统一商户和门店进行发单。其中，商户id：73753，门店编号：11047059
                dadaOrder.ShopNo = "11047059";
                dadaOrder.ProductOrder = po;
                dadaOrder.CityCode = this.ddlCityCode.SelectedValue;
                dadaOrder.CreateTime = DateTime.Now;
                dadaOrder.Info = po.OrderMemo;
                dadaOrder.CargoType = 2;
                dadaOrder.CargoWeight = 1;
                dadaOrder.CargoPrice = po.OrderPrice;
                dadaOrder.CargoNum = po.OrderDetailCount;
                dadaOrder.IsPrepay = false;
                dadaOrder.ExpectedFetchTime = DateTime.Now.AddHours(1);
                dadaOrder.InvoiceTitle = "xx公司";
                dadaOrder.ReceiverName = po.DeliverName;
                dadaOrder.ReceiverAddress = po.DeliverAddress;
                dadaOrder.ReceiverPhone = po.DeliverPhone;
                dadaOrder.ReceiverLat = 31.191142;
                dadaOrder.ReceiverLng = 121.453541;

                JObject jDaDaOrder = DaDaBiz.AddOrder(dadaOrder, int.Parse(this.rblAddOrderType.SelectedValue));
                this.lblMsg.Text = jDaDaOrder.ToString();
            }
        }

    }


    protected void btnAccept_Click(object sender, EventArgs e)
    {
        this.lblMsg.Text = DaDaBiz.AcceptOrder(this.txtOrderID.Text);
    }

    protected void btnFetch_Click(object sender, EventArgs e)
    {
        this.lblMsg.Text = DaDaBiz.FetchOrder(this.txtOrderID.Text);
    }

    protected void btnFinish_Click(object sender, EventArgs e)
    {
        this.lblMsg.Text = DaDaBiz.FinishOrder(this.txtOrderID.Text);
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        this.lblMsg.Text = DaDaBiz.CancelOrder(this.txtOrderID.Text, "测试取消订单");
    }

    protected void btnExpire_Click(object sender, EventArgs e)
    {
        this.lblMsg.Text = DaDaBiz.ExpireOrder(this.txtOrderID.Text);
    }

    /// <summary>
    ///  调用阿里大于模板短信接口
    /// </summary>
    /// <param name="url"></param>
    /// <param name="appKey"></param>
    /// <param name="secret"></param>
    /// <param name="toPhoneNumber"></param>
    /// <returns></returns>
    public string AliSMS(string url, string appKey, string secret, string toPhoneNumber)
    {
        ITopClient client = new DefaultTopClient(url, appKey, secret);
        AlibabaAliqinFcSmsNumSendRequest req = new AlibabaAliqinFcSmsNumSendRequest();
        req.Extend = "";
        req.SmsType = "normal";
        req.SmsFreeSignName = "";
        req.SmsParam = "";
        req.RecNum = toPhoneNumber;
        req.SmsTemplateCode = "";
        AlibabaAliqinFcSmsNumSendResponse rsp = client.Execute(req);

        return rsp.Body;
    }
}