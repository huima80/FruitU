using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;
using Com.Alipay;

public partial class MyOrders : System.Web.UI.Page
{
    
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //在前端页面定义pageSize变量用于分页操作
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsPager", string.Format("var pageSize={0}, defaultImg='{1}';", Config.OrderListPageSize, Config.DefaultImg), true);
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsPaymentTerm", string.Format("var paymentTerm={{wechat:{0},alipay:{1},cash:{2}}}, apGateway = '{3}';", (int)PaymentTerm.WECHAT, (int)PaymentTerm.ALIPAY, (int)PaymentTerm.CASH, AliPayConfig.AliPayGateway), true);
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
            throw ex;
        }

    }

}