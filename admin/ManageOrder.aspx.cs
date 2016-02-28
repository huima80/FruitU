using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Data;

public partial class ManageOrder : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            this.odsOrderList.TypeName = "ProductOrder";
            this.odsOrderList.EnablePaging = true;

            this.odsOrderList.SelectParameters.Add("tableName", DbType.String, "ProductOrder");
            this.odsOrderList.SelectParameters.Add("pk", DbType.String, "ProductOrder.Id");
            this.odsOrderList.SelectParameters.Add("fieldsName", DbType.String, "ProductOrder.*");
            this.odsOrderList.SelectParameters.Add("strWhere", DbType.String, string.Empty);
            this.odsOrderList.SelectParameters.Add("strOrder", DbType.String, string.Empty);
            this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("totalRows", DbType.String, string.Empty)].Direction = ParameterDirection.Output;

            this.gvOrderList.AllowPaging = true;
            this.gvOrderList.AllowCustomPaging = true;
            this.gvOrderList.PageIndex = 0;
            this.gvOrderList.PageSize = Config.OrderListPageSize;
        }
    }

    protected void odsOrderList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
        //自定义查询时，GridView控件会调用两次数据源的SelectMethod，第一次调用SelectMethod，第二次调用SelectCountMethod。每次调用都会把SelectParameters集合复制给e.InputParameters集合。SelectParameters也会在每次的post时保存在viewstate中，维持状态。
        if (e.ExecutingSelectCount)
        {
            //第二次调用SelectCountMethod时，只需要tableName，strWhere实参。
            e.InputParameters.Clear();
            e.InputParameters.Add("tableName", this.odsOrderList.SelectParameters["tableName"].DefaultValue);
            e.InputParameters.Add("strWhere", this.odsOrderList.SelectParameters["strWhere"].DefaultValue);
        }
    }

    protected void odsOrderList_Selected(object sender, ObjectDataSourceStatusEventArgs e)
    {
        //this.totalRows = int.Parse(e.OutputParameters["totalRows"].ToString());
        if (e.OutputParameters.Count != 0 && e.OutputParameters["totalRows"] != null)
        {
            this.lblSearchResult.Text = e.OutputParameters["totalRows"].ToString();
        }
    }

    protected string paymentTerm(PaymentTerm pt)
    {
        switch(pt)
        {
            case PaymentTerm.WECHAT:
                return "微信支付";
            case PaymentTerm.CASH:
                return "货到付款";
            default:
                return "未知方式";
        }
    }

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
            default:
                return "未知状态";
        }
    }


    protected void gvOrderList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //绑定订单商品详情
            ProductOrder po = (ProductOrder)e.Row.DataItem;

            //根据微信支付状态，标识表格栏底色
            switch(po.TradeState)
            {
                case TradeState.SUCCESS:
                    e.Row.CssClass = "success";
                    break;
                case TradeState.NOTPAY:
                    e.Row.CssClass = "danger";
                    break;
            }

            //如果已发货，则发货按钮不可用
            CheckBox cbIsDelivery = e.Row.FindControl("cbIsDelivery") as CheckBox;
            if(po.IsDelivered)
            {
                cbIsDelivery.Enabled = false;
            }
            else
            {
                cbIsDelivery.Enabled = true;
            }

        }

    }

    protected void cbIsDelivery_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cbIsDelivery = sender as CheckBox;
        this.odsOrderList.UpdateParameters.Add("id", DbType.Int32, cbIsDelivery.ToolTip);
        this.odsOrderList.UpdateParameters.Add("isDelivered", DbType.Boolean, cbIsDelivery.Checked ? "true" : "false");
        this.odsOrderList.Update();
        this.gvOrderList.DataBind();

    }


    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string strWhere = string.Empty, tableName = string.Empty;
        List<string> listWhere = new List<string>();
        bool hasProductDetailCriteria = false;

        try
        {
            //查询条件：支付方式
            if (this.ddlPaymentTerm.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlPaymentTerm.SelectedValue);
                listWhere.Add(string.Format("PaymentTerm = {0}", this.ddlPaymentTerm.SelectedValue));
                this.ddlPaymentTerm.Style.Add("background-color", "pink");
            }
            else
            {
                this.ddlPaymentTerm.Style.Clear();
            }

            //查询条件：微信支付状态
            if (this.ddlTradeState.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlTradeState.SelectedValue);
                listWhere.Add(string.Format("TradeState = {0}", this.ddlTradeState.SelectedValue));
                this.ddlTradeState.Style.Add("background-color", "pink");
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
                this.ddlIsDelivery.Style.Add("background-color", "pink");
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
                this.ddlIsAccept.Style.Add("background-color", "pink");
            }
            else
            {
                this.ddlIsAccept.Style.Clear();
            }

            //查询条件：收货人姓名
            if (!string.IsNullOrEmpty(this.txtDeliverName.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtDeliverName.Text);
                listWhere.Add(string.Format("DeliverName like '%{0}%'", this.txtDeliverName.Text.Trim()));
                this.txtDeliverName.Style.Add("background-color", "pink");
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
                this.txtDeliverPhone.Style.Add("background-color", "pink");
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
                this.txtOrderID.Style.Add("background-color", "pink");
            }
            else
            {
                this.txtOrderID.Style.Clear();
            }

            //查询条件：订单商品详情
            if (!string.IsNullOrEmpty(this.txtOrderDetail.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtOrderDetail.Text);
                listWhere.Add(string.Format("OrderDetail.OrderProductName like '%{0}%'", this.txtOrderDetail.Text.Trim()));
                this.txtOrderDetail.Style.Add("background-color", "pink");
                hasProductDetailCriteria = true;
            }
            else
            {
                this.txtOrderDetail.Style.Clear();
            }

            //查询条件：微信支付流水号
            if (!string.IsNullOrEmpty(this.txtTransactionID.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtTransactionID.Text);
                listWhere.Add(string.Format("TransactionID like '%{0}%'", this.txtTransactionID.Text.Trim()));
                this.txtTransactionID.Style.Add("background-color", "pink");
            }
            else
            {
                this.txtTransactionID.Style.Clear();
            }

            //查询条件：订单开始日期
            if (!string.IsNullOrEmpty(this.txtStartOrderDate.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtStartOrderDate.Text);
                listWhere.Add(string.Format("CONVERT(varchar(8), OrderDate, 112) >= '{0}'", this.txtStartOrderDate.Text.Trim().Replace("-", "")));
                this.txtStartOrderDate.Style.Add("background-color", "pink");
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
                this.txtEndOrderDate.Style.Add("background-color", "pink");
            }
            else
            {
                this.txtEndOrderDate.Style.Clear();
            }

            listWhere.ForEach(w =>
            {
                if (string.IsNullOrEmpty(strWhere))
                {
                    strWhere = w;
                }
                else
                {
                    strWhere += " and " + w;
                }
            });

            this.gvOrderList.PageIndex = 0;
            this.odsOrderList.SelectParameters["strWhere"].DefaultValue = strWhere;

            //如果查询涉及到订单商品详情，则需要关联表OrderDetail
            if (hasProductDetailCriteria)
            {
                this.odsOrderList.SelectParameters["tableName"].DefaultValue = "ProductOrder left join OrderDetail on ProductOrder.Id = OrderDetail.PoID";
            }
            else
            {
                this.odsOrderList.SelectParameters["tableName"].DefaultValue = "ProductOrder";
            }

            this.gvOrderList.DataBind();
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWarn", string.Format("alert('{0}');", ex.Message), true);
        }

    }

    protected void btnShowAll_Click(object sender, EventArgs e)
    {
        this.ddlIsDelivery.SelectedIndex = 0;
        this.ddlIsDelivery.Style.Clear();

        this.ddlIsAccept.SelectedIndex = 0;
        this.ddlIsAccept.Style.Clear();

        this.ddlPaymentTerm.SelectedIndex = 0;
        this.ddlPaymentTerm.Style.Clear();

        this.ddlTradeState.SelectedIndex = 0;
        this.ddlTradeState.Style.Clear();

        this.txtDeliverName.Text = string.Empty;
        this.txtDeliverName.Style.Clear();

        this.txtDeliverPhone.Text = string.Empty;
        this.txtDeliverPhone.Style.Clear();

        this.txtOrderID.Text = string.Empty;
        this.txtOrderID.Style.Clear();

        this.txtOrderDetail.Text = string.Empty;
        this.txtOrderDetail.Style.Clear();

        this.txtTransactionID.Text = string.Empty;
        this.txtTransactionID.Style.Clear();

        this.txtStartOrderDate.Text = string.Empty;
        this.txtEndOrderDate.Text = string.Empty;
        this.txtStartOrderDate.Style.Clear();
        this.txtEndOrderDate.Style.Clear();

        this.gvOrderList.PageIndex = 0;
        this.odsOrderList.SelectParameters["tableName"].DefaultValue = "ProductOrder";
        this.odsOrderList.SelectParameters["strWhere"].DefaultValue = string.Empty;
        this.gvOrderList.DataBind();
    }
}