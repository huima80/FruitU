using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using System.Data;

public partial class ManageOrder : System.Web.UI.Page
{
    /// <summary>
    /// 查询条件框的背景色
    /// </summary>
    protected static readonly System.Drawing.Color CRITERIA_BG_COLOR = System.Drawing.Color.Pink;

    protected const string BTN_DONE = "btn btn-sm btn-block btn-success ladda-button";
    protected const string BTN_DOING = "btn btn-sm btn-block btn-danger ladda-button";

    protected const string BTN_CASH_DONE = "btn btn-xs btn-success ladda-button";
    protected const string BTN_CASH_DOING = "btn btn-xs btn-danger ladda-button";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            try
            {
                string openID, agentOpenID, groupEventID, strWhere = string.Empty;

                if (Request.QueryString["OpenID"] != null)
                {
                    UtilityHelper.AntiSQLInjection(Request.QueryString["OpenID"]);
                    openID = Request.QueryString["OpenID"];
                    strWhere = string.Format("OpenID='{0}'", openID);
                }

                if (Request.QueryString["AgentOpenID"] != null)
                {
                    UtilityHelper.AntiSQLInjection(Request.QueryString["AgentOpenID"]);
                    agentOpenID = Request.QueryString["AgentOpenID"];
                    strWhere = string.Format("AgentOpenID='{0}'", agentOpenID);
                }

                if (Request.QueryString["GroupEventID"] != null)
                {
                    UtilityHelper.AntiSQLInjection(Request.QueryString["GroupEventID"]);
                    groupEventID = Request.QueryString["GroupEventID"];
                    this.txtGroupEventID.Text = groupEventID;
                    strWhere = string.Format("Id in (select PoID from OrderDetail where GroupEventID = {0})", groupEventID);
                }

                this.odsOrderList.TypeName = "ProductOrder";
                this.odsOrderList.EnablePaging = true;

                this.odsOrderList.SelectParameters.Add("strWhere", DbType.String, strWhere);
                this.odsOrderList.SelectParameters.Add("strOrder", DbType.String, string.Empty);
                this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("totalRows", DbType.Int32, "0")].Direction = ParameterDirection.Output;
                this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("payingOrderCount", DbType.Int32, "0")].Direction = ParameterDirection.Output;
                this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("deliveringOrderCount", DbType.Int32, "0")].Direction = ParameterDirection.Output;
                this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("acceptingOrderCount", DbType.Int32, "0")].Direction = ParameterDirection.Output;
                this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("cancelledOrderCount", DbType.Int32, "0")].Direction = ParameterDirection.Output;
                this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("totalOrderPrice", DbType.Decimal, "0")].Direction = ParameterDirection.Output;

                this.gvOrderList.AllowPaging = true;
                this.gvOrderList.AllowCustomPaging = true;
                this.gvOrderList.PageIndex = 0;
                this.gvOrderList.PageSize = Config.OrderListPageSize;

                this.ddlPaymentTerm.Items.Add(new ListItem("微信支付", ((int)PaymentTerm.WECHAT).ToString()));
                this.ddlPaymentTerm.Items.Add(new ListItem("支付宝", ((int)PaymentTerm.ALIPAY).ToString()));
                this.ddlPaymentTerm.Items.Add(new ListItem("货到付款", ((int)PaymentTerm.CASH).ToString()));

                this.ddlTradeState.Items.Add(new ListItem("===微信支付状态===", "-1"));
                this.ddlTradeState.Items.Add(new ListItem("支付成功", ((int)TradeState.SUCCESS).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("转入退款", ((int)TradeState.REFUND).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("未支付", ((int)TradeState.NOTPAY).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("已关闭", ((int)TradeState.CLOSED).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("已撤销（刷卡支付）", ((int)TradeState.REVOKED).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("用户支付中", ((int)TradeState.USERPAYING).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("支付失败", ((int)TradeState.PAYERROR).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("===支付宝状态===", "-1"));
                this.ddlTradeState.Items.Add(new ListItem("等待买家付款", ((int)TradeState.AP_WAIT_BUYER_PAY).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("等待卖家收款", ((int)TradeState.AP_TRADE_PENDING).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("交易成功", ((int)TradeState.AP_TRADE_SUCCESS).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("交易成功且结束", ((int)TradeState.AP_TRADE_FINISHED).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("未支付已关闭", ((int)TradeState.AP_TRADE_CLOSED).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("===货到付款状态===", "-1"));
                this.ddlTradeState.Items.Add(new ListItem("已付现金", ((int)TradeState.CASHPAID).ToString()));
                this.ddlTradeState.Items.Add(new ListItem("未付现金", ((int)TradeState.CASHNOTPAID).ToString()));
            }
            catch (Exception ex)
            {
                Response.Write(string.Format("<script>alert('{0}');history.back();</script>", ex.Message));
                Response.End();
            }
        }
    }

    protected void odsOrderList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
        //自定义查询时，GridView控件会调用两次数据源的SelectMethod，第一次调用SelectMethod，第二次调用SelectCountMethod。每次调用都会把SelectParameters集合复制给e.InputParameters集合。SelectParameters也会在每次的post时保存在viewstate中，维持状态。
        if (e.ExecutingSelectCount)
        {
            //第二次调用SelectCountMethod时，只需要strWhere实参。
            e.InputParameters.Clear();
            e.InputParameters.Add("strWhere", this.odsOrderList.SelectParameters["strWhere"].DefaultValue);
        }
    }

    protected void odsOrderList_Selected(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.OutputParameters.Count != 0 && e.OutputParameters["totalRows"] != null && e.OutputParameters["payingOrderCount"] != null && e.OutputParameters["deliveringOrderCount"] != null && e.OutputParameters["acceptingOrderCount"] != null)
        {
            this.lblOrderPrice.Text = string.Format("￥{0}元", decimal.Parse(e.OutputParameters["totalOrderPrice"].ToString()));
            this.lblTotalRows.Text = e.OutputParameters["totalRows"].ToString();
            this.lblPayingOrderCount.Text = e.OutputParameters["payingOrderCount"].ToString();
            this.lblDeliveringOrderCount.Text = e.OutputParameters["deliveringOrderCount"].ToString();
            this.lblAcceptingOrderCount.Text = e.OutputParameters["acceptingOrderCount"].ToString();
            this.lblCancelledOrderCount.Text = e.OutputParameters["cancelledOrderCount"].ToString();
        }
    }

    /// <summary>
    /// 用于GridView控件中的订单支付方式显示值
    /// </summary>
    /// <param name="pt"></param>
    /// <returns></returns>
    protected string paymentTerm(PaymentTerm pt)
    {
        switch (pt)
        {
            case PaymentTerm.WECHAT:
                return "<i class=\"fa fa-wechat\"></i>&nbsp;微信支付";
            case PaymentTerm.CASH:
                return "<i class=\"fa fa-money\"></i>&nbsp;货到付款";
            case PaymentTerm.ALIPAY:
                return "<img src=\"../images/alipay.png\" />&nbsp;支付宝";
            default:
                return "未知方式";
        }
    }

    /// <summary>
    /// 用于GridView控件中的订单支付状态显示值
    /// </summary>
    /// <param name="ts"></param>
    /// <returns></returns>
    protected string tradeState(TradeState ts)
    {
        switch (ts)
        {
            case TradeState.SUCCESS:
                return "支付成功";
            case TradeState.REFUND:
                return "转入退款";
            case TradeState.NOTPAY:
                return "未支付";
            case TradeState.CLOSED:
                return "已关闭";
            case TradeState.REVOKED:
                return "已撤销（刷卡支付）";
            case TradeState.USERPAYING:
                return "用户支付中";
            case TradeState.PAYERROR:
                return "支付失败(其他原因，如银行返回失败)";
            case TradeState.CASHPAID:
                return "已付现金";
            case TradeState.CASHNOTPAID:
                return "未付现金";
            case TradeState.AP_WAIT_BUYER_PAY:
                return "等待买家付款";
            case TradeState.AP_TRADE_SUCCESS:
                return "交易成功";
            case TradeState.AP_TRADE_FINISHED:
                return "交易结束";
            case TradeState.AP_TRADE_PENDING:
                return "等待卖家收款";
            case TradeState.AP_TRADE_CLOSED:
                return "交易未支付已关闭";
            default:
                return "未知状态";
        }
    }


    protected void gvOrderList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //当前行绑定的订单对象
            ProductOrder po = (ProductOrder)e.Row.DataItem;

            //根据微信支付状态，标识表格栏底色
            if (po.IsCancel)
            {
                e.Row.CssClass = "default";
            }
            else
            {
                switch (po.TradeState)
                {
                    case TradeState.SUCCESS:
                    case TradeState.CASHPAID:
                    case TradeState.AP_TRADE_SUCCESS:
                    case TradeState.AP_TRADE_FINISHED:
                        e.Row.CssClass = "success";
                        break;
                    default:
                        e.Row.CssClass = "danger";
                        break;
                }
            }

            HtmlGenericControl pAgent = e.Row.FindControl("pAgent") as HtmlGenericControl;
            if (po.Agent != null)
            {
                Label lblAgent = e.Row.FindControl("lblAgent") as Label;
                string faSex = po.Agent.Sex ? "<i class=\"fa fa-mars\" style=\"color:blue;\"></i>" : "<i class=\"fa fa-venus\" style=\"color:deeppink;\"></i>";
                lblAgent.Text = string.Format("{0}&nbsp;{1}", po.Agent.NickName, faSex);
                pAgent.Visible = true;
            }
            else
            {
                pAgent.Visible = false;
            }

            //控制显示积分优惠和微信优惠券优惠
            HtmlGenericControl pMemberPointsDiscount = e.Row.FindControl("pMemberPointsDiscount") as HtmlGenericControl;
            HtmlGenericControl pWxCardDiscount = e.Row.FindControl("pWxCardDiscount") as HtmlGenericControl;
            pMemberPointsDiscount.Visible = (po.MemberPointsDiscount != 0) ? true : false;
            pWxCardDiscount.Visible = (po.WxCardDiscount != 0) ? true : false;

            //支付方式和支付状态控件
            Label lblPaymentTerm = e.Row.FindControl("lblPaymentTerm") as Label;
            Label lblWxTradeState = e.Row.FindControl("lblWxTradeState") as Label;
            Label lblAlipayTradeState = e.Row.FindControl("lblAlipayTradeState") as Label;
            Button btnPayCash = e.Row.FindControl("btnPayCash") as Button;
            HtmlGenericControl pTransactionID = e.Row.FindControl("pTransactionID") as HtmlGenericControl;
            HtmlGenericControl pTransactionTime = e.Row.FindControl("pTransactionTime") as HtmlGenericControl;
            HtmlGenericControl pAP_TradeNo = e.Row.FindControl("pAP_TradeNo") as HtmlGenericControl;
            HtmlGenericControl pAP_GMT_Payment = e.Row.FindControl("pAP_GMT_Payment") as HtmlGenericControl;
            HtmlGenericControl pPayCashDate = e.Row.FindControl("pPayCashDate") as HtmlGenericControl;

            //显示支付方式文本值
            lblPaymentTerm.Text = paymentTerm(po.PaymentTerm);

            //根据支付方式（微信支付、支付宝、货到付款），分别显示不同的支付状态，如果是货到付款还需要提供CheckBox供修改
            switch (po.PaymentTerm)
            {
                case PaymentTerm.WECHAT:
                    lblWxTradeState.Visible = true;
                    lblWxTradeState.Text = tradeState(po.TradeState);
                    lblAlipayTradeState.Visible = false;
                    btnPayCash.Visible = false;
                    pTransactionID.Visible = !string.IsNullOrEmpty(po.TransactionID);
                    pTransactionTime.Visible = po.TransactionTime.HasValue;
                    pAP_TradeNo.Visible = false;
                    pAP_GMT_Payment.Visible = false;
                    pPayCashDate.Visible = false;
                    break;
                case PaymentTerm.ALIPAY:
                    lblAlipayTradeState.Visible = true;
                    lblAlipayTradeState.Text = tradeState(po.TradeState);
                    lblWxTradeState.Visible = false;
                    btnPayCash.Visible = false;
                    pTransactionID.Visible = false;
                    pTransactionTime.Visible = false;
                    pAP_TradeNo.Visible = !string.IsNullOrEmpty(po.AP_TradeNo);
                    pAP_GMT_Payment.Visible = po.AP_GMT_Payment.HasValue;
                    pPayCashDate.Visible = false;
                    break;
                case PaymentTerm.CASH:
                    btnPayCash.Visible = true;
                    btnPayCash.Text = tradeState(po.TradeState);
                    lblWxTradeState.Visible = false;
                    lblAlipayTradeState.Visible = false;
                    pTransactionID.Visible = false;
                    pTransactionTime.Visible = false;
                    pAP_TradeNo.Visible = false;
                    pAP_GMT_Payment.Visible = false;
                    switch (po.TradeState)
                    {
                        case TradeState.CASHPAID:
                            btnPayCash.Enabled = false;
                            btnPayCash.CssClass = BTN_CASH_DONE;
                            pPayCashDate.Visible = po.PayCashDate.HasValue;
                            break;
                        case TradeState.CASHNOTPAID:
                            btnPayCash.Enabled = true;
                            btnPayCash.CssClass = BTN_CASH_DOING;
                            pPayCashDate.Visible = false;
                            break;
                    }
                    break;
            }

            //发货、签收、发放积分按钮
            Button btnDeliver = e.Row.FindControl("btnDeliver") as Button;
            Button btnAccept = e.Row.FindControl("btnAccept") as Button;
            Button btnCalMemberPoints = e.Row.FindControl("btnCalMemberPoints") as Button;
            if (po.IsDelivered)
            {
                btnDeliver.Enabled = false;
                btnDeliver.CssClass = BTN_DONE;
                btnDeliver.ToolTip = po.DeliverDate.HasValue ? "发货时间：" + po.DeliverDate.ToString() : string.Empty;
            }
            else
            {
                btnDeliver.CssClass = BTN_DOING;
            }
            if (po.IsAccept)
            {
                btnAccept.Enabled = false;
                btnAccept.CssClass = BTN_DONE;
                btnAccept.ToolTip = po.AcceptDate.HasValue ? "签收时间：" + po.AcceptDate.ToString() : string.Empty;
            }
            else
            {
                btnAccept.CssClass = BTN_DOING;
            }
            if (po.IsCalMemberPoints)
            {
                btnCalMemberPoints.Enabled = false;
                btnCalMemberPoints.CssClass = BTN_DONE;
            }
            else
            {
                btnCalMemberPoints.CssClass = BTN_DOING;
            }

            //如果已撤单则屏蔽发货、签收按钮、发放积分按钮，显示撤单时间
            HtmlGenericControl pCancelDate = e.Row.FindControl("pCancelDate") as HtmlGenericControl;
            if (po.IsCancel)
            {
                //屏蔽发货按钮
                btnDeliver.Enabled = false;
                btnDeliver.ToolTip = "已撤单，不能发货";

                //屏蔽签收按钮
                btnAccept.Enabled = false;
                btnAccept.ToolTip = "已撤单，不能签收";

                //屏蔽发放积分按钮
                btnCalMemberPoints.Enabled = false;
                btnCalMemberPoints.ToolTip = "已撤单，不能发放积分";

                //显示撤单时间
                pCancelDate.Visible = true;
            }
            else
            {
                pCancelDate.Visible = false;
            }

        }

    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string strWhere = string.Empty, tableName = string.Empty;
        List<string> listWhere = new List<string>();

        try
        {
            //查询条件：支付方式
            if (this.ddlPaymentTerm.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlPaymentTerm.SelectedValue);
                listWhere.Add(string.Format("PaymentTerm = {0}", this.ddlPaymentTerm.SelectedValue));
                this.ddlPaymentTerm.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlPaymentTerm.Style.Clear();
            }

            //查询条件：支付状态
            if (this.ddlTradeState.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlTradeState.SelectedValue);
                listWhere.Add(string.Format("TradeState = {0}", this.ddlTradeState.SelectedValue));
                this.ddlTradeState.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlTradeState.Style.Clear();
            }

            //查询条件：发货状态
            if (this.ddlIsDelivery.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlIsDelivery.SelectedValue);
                listWhere.Add(string.Format("IsDelivered = {0}", this.ddlIsDelivery.SelectedValue));
                this.ddlIsDelivery.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlIsDelivery.Style.Clear();
            }

            //查询条件：签收状态
            if (this.ddlIsAccept.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlIsAccept.SelectedValue);
                listWhere.Add(string.Format("IsAccept = {0}", this.ddlIsAccept.SelectedValue));
                this.ddlIsAccept.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlIsAccept.Style.Clear();
            }

            //查询条件：撤单状态
            if (this.ddlIsCancel.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlIsCancel.SelectedValue);
                listWhere.Add(string.Format("IsCancel = {0}", this.ddlIsCancel.SelectedValue));
                this.ddlIsCancel.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlIsCancel.Style.Clear();
            }

            //查询条件：收货人姓名
            if (!string.IsNullOrEmpty(this.txtDeliverName.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtDeliverName.Text);
                listWhere.Add(string.Format("DeliverName like '%{0}%'", this.txtDeliverName.Text.Trim()));
                this.txtDeliverName.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtDeliverName.Style.Clear();
            }

            //查询条件：收货人电话
            if (!string.IsNullOrEmpty(this.txtDeliverPhone.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtDeliverPhone.Text);
                listWhere.Add(string.Format("DeliverPhone like '%{0}%'", this.txtDeliverPhone.Text.Trim()));
                this.txtDeliverPhone.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtDeliverPhone.Style.Clear();
            }

            //查询条件：订单ID
            if (!string.IsNullOrEmpty(this.txtOrderID.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtOrderID.Text);
                listWhere.Add(string.Format("OrderID like '%{0}%'", this.txtOrderID.Text.Trim()));
                this.txtOrderID.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtOrderID.Style.Clear();
            }

            //查询条件：团购活动ID
            if (!string.IsNullOrEmpty(this.txtGroupEventID.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtGroupEventID.Text);
                listWhere.Add(string.Format("Id in (select PoID from OrderDetail where GroupEventID = {0})", this.txtGroupEventID.Text.Trim()));
                this.txtGroupEventID.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtGroupEventID.Style.Clear();
            }

            //查询条件：订单商品详情
            if (!string.IsNullOrEmpty(this.txtOrderDetail.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtOrderDetail.Text);
                listWhere.Add(string.Format("Id in (select PoID from OrderDetail where OrderProductName like '%{0}%')", this.txtOrderDetail.Text.Trim()));
                this.txtOrderDetail.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtOrderDetail.Style.Clear();
            }

            //查询条件：微信支付交易号
            if (!string.IsNullOrEmpty(this.txtTransactionID.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtTransactionID.Text);
                listWhere.Add(string.Format("TransactionID like '%{0}%'", this.txtTransactionID.Text.Trim()));
                this.txtTransactionID.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtTransactionID.Style.Clear();
            }

            //查询条件：支付宝交易号
            if (!string.IsNullOrEmpty(this.txtTradeNo.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtTradeNo.Text);
                listWhere.Add(string.Format("AP_TradeNo like '%{0}%'", this.txtTradeNo.Text.Trim()));
                this.txtTransactionID.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtTradeNo.Style.Clear();
            }

            //查询条件：订单开始日期
            if (!string.IsNullOrEmpty(this.txtStartOrderDate.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtStartOrderDate.Text);
                listWhere.Add(string.Format("CONVERT(varchar(8), OrderDate, 112) >= '{0}'", this.txtStartOrderDate.Text.Trim().Replace("-", "")));
                this.txtStartOrderDate.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtStartOrderDate.Style.Clear();
            }

            //查询条件：订单结束日期
            if (!string.IsNullOrEmpty(this.txtEndOrderDate.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtEndOrderDate.Text);
                listWhere.Add(string.Format("CONVERT(varchar(8), OrderDate, 112) <= '{0}'", this.txtEndOrderDate.Text.Trim().Replace("-", "")));
                this.txtEndOrderDate.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtEndOrderDate.Style.Clear();
            }

            strWhere = string.Join<string>(" and ", listWhere);

            this.gvOrderList.PageIndex = 0;
            this.odsOrderList.SelectParameters["strWhere"].DefaultValue = strWhere;

            this.gvOrderList.DataBind();
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWarn", string.Format("alert('{0}');", ex.Message), true);
        }

    }

    protected void btnShowAll_Click(object sender, EventArgs e)
    {
        this.ddlPaymentTerm.SelectedIndex = 0;
        this.ddlPaymentTerm.Style.Clear();

        this.ddlTradeState.SelectedIndex = 0;
        this.ddlTradeState.Style.Clear();

        this.ddlIsDelivery.SelectedIndex = 0;
        this.ddlIsDelivery.Style.Clear();

        this.ddlIsAccept.SelectedIndex = 0;
        this.ddlIsAccept.Style.Clear();

        this.ddlIsCancel.SelectedIndex = 0;
        this.ddlIsCancel.Style.Clear();

        this.txtDeliverName.Text = string.Empty;
        this.txtDeliverName.Style.Clear();

        this.txtDeliverPhone.Text = string.Empty;
        this.txtDeliverPhone.Style.Clear();

        this.txtOrderID.Text = string.Empty;
        this.txtOrderID.Style.Clear();

        this.txtGroupEventID.Text = string.Empty;
        this.txtGroupEventID.Style.Clear();

        this.txtOrderDetail.Text = string.Empty;
        this.txtOrderDetail.Style.Clear();

        this.txtTransactionID.Text = string.Empty;
        this.txtTransactionID.Style.Clear();

        this.txtTradeNo.Text = string.Empty;
        this.txtTradeNo.Style.Clear();

        this.txtStartOrderDate.Text = string.Empty;
        this.txtEndOrderDate.Text = string.Empty;
        this.txtStartOrderDate.Style.Clear();
        this.txtEndOrderDate.Style.Clear();

        this.odsOrderList.SelectParameters["strWhere"].DefaultValue = string.Empty;
        this.gvOrderList.PageIndex = 0;
        this.gvOrderList.DataBind();
    }

    protected void gvOrderList_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        ProductOrder po = new ProductOrder();
        int poID;
        //根据命令值修改订单
        switch (e.CommandName)
        {
            case "PayCash":
                //根据订单ID加载完整订单信息
                if (int.TryParse(e.CommandArgument.ToString(), out poID))
                {
                    //根据订单ID加载完整订单信息
                    po.FindOrderByID(poID);
                }
                else
                {
                    throw new Exception("订单ID错误");
                }

                po.TradeState = TradeState.CASHPAID;
                po.PayCashDate = DateTime.Now;
                //注册订单的支付状态变动事件处理函数，通知管理员
                po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);
                //注册订单的支付状态变动事件处理函数，核销微信卡券
                po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxCard.ConsumeCodeOnOrderStateChanged);
                //注册订单的支付宝支付状态变动事件处理函数，检测订单中的商品团购活动是否成功
                po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(GroupPurchaseEvent.GroupPurchaseEventSuccessHandler);
                ProductOrder.UpdateTradeState(po);
                gvOrderList.DataBind();
                break;
            case "Deliver":
                //根据订单ID加载完整订单信息
                if (int.TryParse(e.CommandArgument.ToString(), out poID))
                {
                    //根据订单ID加载完整订单信息
                    po.FindOrderByID(poID);
                }
                else
                {
                    throw new Exception("订单ID错误");
                }

                po.IsDelivered = true;
                po.DeliverDate = DateTime.Now;
                //注册订单发货事件处理函数，通知用户
                po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);
                po.OrderDetailList.ForEach(od =>
                {
                    //注册商品库存量报警事件处理函数，通知管理员
                    od.InventoryWarn += new EventHandler(WxTmplMsg.SendMsgOnInventoryWarn);
                });
                ProductOrder.DeliverOrder(po);
                gvOrderList.DataBind();
                break;
            case "Accept":
                //根据订单ID加载完整订单信息
                if (int.TryParse(e.CommandArgument.ToString(), out poID))
                {
                    //根据订单ID加载完整订单信息
                    po.FindOrderByID(poID);
                }
                else
                {
                    throw new Exception("订单ID错误");
                }

                po.IsAccept = true;
                po.AcceptDate = DateTime.Now;
                ProductOrder.AcceptOrder(po);
                gvOrderList.DataBind();
                break;
            case "CalMemberPoints":
                //根据订单ID加载完整订单信息
                if (int.TryParse(e.CommandArgument.ToString(), out poID))
                {
                    //根据订单ID加载完整订单信息
                    po.FindOrderByID(poID);
                }
                else
                {
                    throw new Exception("订单ID错误");
                }

                //注册订单的发放积分事件处理函数，通知下单人和推荐人
                po.MemberPointsCalculated += new EventHandler<ProductOrder.MemberPointsCalculatedEventArgs>(WxTmplMsg.SendMsgOnMemberPoints);
                ProductOrder.EarnMemberPoints(po);
                gvOrderList.DataBind();
                break;
        }

    }

    protected void dlOrderDetail_ItemDataBound(object sender, DataListItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item)
        {
            OrderDetail od = e.Item.DataItem as OrderDetail;
            if (od != null && od.GroupPurchaseEvent != null)
            {
                HyperLink hlGroupPurchaseEventStatus = e.Item.FindControl("hlGroupPurchaseEventStatus") as HyperLink;
                hlGroupPurchaseEventStatus.NavigateUrl = Request.Url.AbsolutePath + "?GroupEventID=" + od.GroupPurchaseEvent.ID;
                hlGroupPurchaseEventStatus.ToolTip = "查看此团购活动的所有订单";
                switch (GroupPurchaseEvent.CheckGroupPurchaseEventSuccess(od.GroupPurchaseEvent))
                {
                    case GroupEventStatus.EVENT_SUCCESS:
                        hlGroupPurchaseEventStatus.Text = "<i class=\"fa fa-group fa-fw\"></i>团购成功";
                        break;
                    case GroupEventStatus.EVENT_GOING:
                        hlGroupPurchaseEventStatus.Text = "<i class=\"fa fa-group fa-fw\"></i>团购进行中";
                        break;
                    case GroupEventStatus.EVENT_FAIL:
                        hlGroupPurchaseEventStatus.Text = "<i class=\"fa fa-group fa-fw\"></i>团购失败";
                        break;
                }
            }
        }
    }
}