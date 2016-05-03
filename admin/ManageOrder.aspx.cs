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

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            try
            {
                string openID, strWhere = string.Empty;

                if (Request.QueryString["OpenID"] != null)
                {
                    UtilityHelper.AntiSQLInjection(Request.QueryString["OpenID"]);
                    openID = Request.QueryString["OpenID"];
                    strWhere = string.Format("OpenID='{0}'", openID);
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
                this.odsOrderList.SelectParameters[this.odsOrderList.SelectParameters.Add("orderPrice", DbType.Decimal, "0")].Direction = ParameterDirection.Output;

                this.gvOrderList.AllowPaging = true;
                this.gvOrderList.AllowCustomPaging = true;
                this.gvOrderList.PageIndex = 0;
                this.gvOrderList.PageSize = Config.OrderListPageSize;
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
            this.lblOrderPrice.Text = string.Format("￥{0}元", decimal.Parse(e.OutputParameters["orderPrice"].ToString()));
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
                        e.Row.CssClass = "success";
                        break;
                    default:
                        e.Row.CssClass = "danger";
                        break;
                }
            }

            //处理订单支付状态
            Label lblPaymentTerm = e.Row.FindControl("lblPaymentTerm") as Label;
            Label lblWxTradeState = e.Row.FindControl("lblWxTradeState") as Label;
            Label lblCashTradeState = e.Row.FindControl("lblCashTradeState") as Label;
            HtmlGenericControl pTransactionID = e.Row.FindControl("pTransactionID") as HtmlGenericControl;
            HtmlGenericControl pTransactionTime = e.Row.FindControl("pTransactionTime") as HtmlGenericControl;
            HtmlGenericControl divCashTradeState = e.Row.FindControl("divCashTradeState") as HtmlGenericControl;
            CheckBox cbCashTradeState = e.Row.FindControl("cbCashTradeState") as CheckBox;
            HtmlGenericControl faCashTradeState = e.Row.FindControl("faCashTradeState") as HtmlGenericControl;

            //显示支付方式文本值
            lblPaymentTerm.Text = paymentTerm(po.PaymentTerm);

            //根据支付方式（微信支付、货到付款），分别显示不同的支付状态，如果是货到付款还需要提供CheckBox供修改
            switch (po.PaymentTerm)
            {
                case PaymentTerm.WECHAT:
                    lblWxTradeState.Visible = true;
                    lblWxTradeState.Text = tradeState(po.TradeState);
                    pTransactionID.Visible = !string.IsNullOrEmpty(po.TransactionID);
                    pTransactionTime.Visible = po.TransactionTime.HasValue;
                    divCashTradeState.Visible = false;
                    faCashTradeState.Visible = false;
                    break;
                case PaymentTerm.CASH:
                    lblWxTradeState.Visible = false;
                    pTransactionID.Visible = false;
                    pTransactionTime.Visible = false;
                    lblCashTradeState.Text = tradeState(po.TradeState);
                    switch (po.TradeState)
                    {
                        case TradeState.CASHPAID:
                            cbCashTradeState.Visible = false;
                            faCashTradeState.Visible = true;
                            break;
                        case TradeState.CASHNOTPAID:
                            faCashTradeState.Visible = false;
                            cbCashTradeState.Visible = true;
                            cbCashTradeState.Attributes["RowIndex"] = e.Row.RowIndex.ToString();
                            cbCashTradeState.Checked = false;
                            break;
                    }
                    break;
            }

            //如果已发货，则绑定发货时间
            CheckBox cbIsDelivery = e.Row.FindControl("cbIsDelivery") as CheckBox;
            HtmlGenericControl faIsDelivery = e.Row.FindControl("faIsDelivery") as HtmlGenericControl;
            if (po.IsDelivered)
            {
                cbIsDelivery.Visible = false;
                faIsDelivery.Visible = true;
                faIsDelivery.Attributes.Add("title", string.Format("发货时间：{0}", po.DeliverDate));
            }
            else
            {
                cbIsDelivery.Visible = true;
                cbIsDelivery.ToolTip = "点击发货";
                cbIsDelivery.Attributes["RowIndex"] = e.Row.RowIndex.ToString();
                faIsDelivery.Visible = false;
            }

            //如果已签收，则绑定签收时间
            CheckBox cbIsAccept = e.Row.FindControl("cbIsAccept") as CheckBox;
            HtmlGenericControl faIsAccept = e.Row.FindControl("faIsAccept") as HtmlGenericControl;
            if (po.IsAccept)
            {
                cbIsAccept.Visible = false;
                faIsAccept.Visible = true;
                faIsAccept.Attributes.Add("title", string.Format("签收时间：{0}", po.AcceptDate));
            }
            else
            {
                cbIsAccept.Enabled = true;
                cbIsAccept.ToolTip = "点击签收";
                cbIsAccept.Attributes["RowIndex"] = e.Row.RowIndex.ToString();
                faIsAccept.Visible = false;
            }

            //如果已撤单则屏蔽发货、签收按钮，显示撤单时间
            HtmlGenericControl pCancelDate = e.Row.FindControl("pCancelDate") as HtmlGenericControl;
            if (po.IsCancel)
            {
                //屏蔽发货按钮
                cbIsDelivery.Enabled = false;
                cbIsDelivery.ToolTip = "已撤单，不能发货";

                //屏蔽签收按钮
                cbIsAccept.Enabled = false;
                cbIsAccept.ToolTip = "已撤单，不能签收";

                //显示撤单时间
                pCancelDate.Visible = true;
            }
            else
            {
                pCancelDate.Visible = false;
            }

        }

    }

    protected void gvOrderList_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        if (odsOrderList.UpdateMethod == "UpdateTradeState")
        {
            //由于cbCashTradeState控件不是Bind绑定控件，所以GridView提交时不会自动生成更新值，需要根据是否选中现金收讫状态，在GridView的NewValues集合中增加值
            GridViewRow gvr = ((GridView)sender).Rows[e.RowIndex];
            CheckBox cbCashTradeState = gvr.FindControl("cbCashTradeState") as CheckBox;
            //根据是否选中checkbox设置现金收款状态
            if (cbCashTradeState != null && cbCashTradeState.Checked)
            {
                e.NewValues.Add("TradeState", TradeState.CASHPAID);
            }
            else
            {
                e.NewValues.Add("TradeState", TradeState.CASHNOTPAID);
            }
        }
    }

    /// <summary>
    /// 根据ObjectDataSource控件反射生成的ProductOrder业务对象的订单ID，加载完整的订单数据，并注册事件处理函数，用于订单处理后回调事件处理函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void odsOrderList_Updating(object sender, ObjectDataSourceMethodEventArgs e)
    {
        if (e.InputParameters.Count == 1 && e.InputParameters[0] is ProductOrder)
        {
            ProductOrder po = e.InputParameters[0] as ProductOrder;

            //保存用户更改的checkbox值
            bool isAccept = po.IsAccept;
            bool isDelivered = po.IsDelivered;
            TradeState tradeState = po.TradeState;

            //根据订单ID加载完整订单信息，并赋值用户更改值
            po.FindOrderByID(po.ID);

            switch (odsOrderList.UpdateMethod)
            {
                case "AcceptOrder":
                    po.IsAccept = isAccept;
                    po.AcceptDate = DateTime.Now;
                    break;
                case "DeliverOrder":
                    po.IsDelivered = isDelivered;
                    po.DeliverDate = DateTime.Now;
                    //注册订单发货事件处理函数，通知用户
                    po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);
                    po.OrderDetailList.ForEach(od =>
                    {
                        //注册商品库存量报警事件，通知管理员
                        od.InventoryWarn += new EventHandler(WxTmplMsg.SendMsgOnInventoryWarn);
                    });
                    break;
                case "UpdateTradeState":
                    po.TradeState = tradeState;
                    //注册订单的货到付款状态变动事件处理函数，给予下单人会员积分
                    po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WeChatUserDAO.EarnMemberPoints);
                    //注册会员积分变动事件处理函数，通知用户
                    po.Purchaser.MemberPointsChanged += WxTmplMsg.SendMsgOnMemberPoints;
                    break;
                default:
                    throw new Exception("不能识别的更新方法");
            }
        }
    }

    protected void cbIsDelivery_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cbIsDelivery = sender as CheckBox;
        int rowIndex = int.Parse(cbIsDelivery.Attributes["RowIndex"]);
        this.odsOrderList.UpdateMethod = "DeliverOrder";
        this.gvOrderList.UpdateRow(rowIndex, false);
        this.gvOrderList.DataBind();
    }

    protected void cbIsAccept_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cbIsAccept = sender as CheckBox;
        int rowIndex = int.Parse(cbIsAccept.Attributes["RowIndex"]);
        this.odsOrderList.UpdateMethod = "AcceptOrder";
        this.gvOrderList.UpdateRow(rowIndex, false);
        this.gvOrderList.DataBind();
    }

    protected void cbCashTradeState_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cbCashTradeState = sender as CheckBox;
        int rowIndex = int.Parse(cbCashTradeState.Attributes["RowIndex"]);
        this.odsOrderList.UpdateMethod = "UpdateTradeState";
        this.gvOrderList.UpdateRow(rowIndex, false);
        this.gvOrderList.DataBind();
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

            //查询条件：微信支付状态
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

            //查询条件：微信支付流水号
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

        this.txtOrderDetail.Text = string.Empty;
        this.txtOrderDetail.Style.Clear();

        this.txtTransactionID.Text = string.Empty;
        this.txtTransactionID.Style.Clear();

        this.txtStartOrderDate.Text = string.Empty;
        this.txtEndOrderDate.Text = string.Empty;
        this.txtStartOrderDate.Style.Clear();
        this.txtEndOrderDate.Style.Clear();

        this.odsOrderList.SelectParameters["strWhere"].DefaultValue = string.Empty;
        this.gvOrderList.PageIndex = 0;
        this.gvOrderList.DataBind();
    }

}