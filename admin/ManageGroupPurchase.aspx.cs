using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

public partial class admin_ManageGroupPurchase : System.Web.UI.Page
{
    /// <summary>
    /// 查询条件框的背景色
    /// </summary>
    protected static readonly System.Drawing.Color CRITERIA_BG_COLOR = System.Drawing.Color.Pink;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            this.gvGroupPurchaseList.PageSize = Config.ProductListPageSize;

            //如果指定了商品ID，则显示指定商品下的所有团购列表信息
            if (!string.IsNullOrEmpty(Request.QueryString["ProductID"]))
            {
                string productID = Request.QueryString["ProductID"];
                UtilityHelper.AntiSQLInjection(productID);

                string strWhere = string.Format("ProductID = {0}", productID);
                this.odsGroupPurchaseList.SelectParameters["strWhere"].DefaultValue = strWhere;
                this.gvGroupPurchaseList.DataBind();

            }

            //从商品管理页面跳转来，新增团购
            if (Request.QueryString["Action"] == "add" && !string.IsNullOrEmpty(Request.QueryString["ProductID"]))
            {
                UtilityHelper.AntiSQLInjection(Request.QueryString["ProductID"]);
                int productID;
                if (int.TryParse(Request.QueryString["ProductID"], out productID))
                {
                    Fruit product = Fruit.FindFruitByID(productID);
                    if (product != null)
                    {
                        //设置DetailsView插入模式
                        this.dvGroupPurchase.ChangeMode(DetailsViewMode.Insert);
                        this.dvGroupPurchase.AutoGenerateInsertButton = true;
                        HiddenField hfProductID = this.dvGroupPurchase.FindControl("hfProductID") as HiddenField;
                        Label lblProductName = this.dvGroupPurchase.FindControl("lblProductName") as Label;
                        //团购所属的商品信息
                        hfProductID.Value = product.ID.ToString();
                        lblProductName.Text = string.Format("【{0}】{1}", product.Category.CategoryName, product.FruitName);
                    }
                    else
                    {
                        throw new Exception("没有此商品");
                    }
                }
                else
                {
                    throw new Exception("没有指定商品ID");
                }
            }
        }

    }

    protected void odsGroupPurchaseList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
        //自定义查询时，GridView控件会调用两次数据源的SelectMethod，第一次调用SelectMethod，第二次调用SelectCountMethod。每次调用都会把SelectParameters集合复制给e.InputParameters集合。SelectParameters也会在每次的post时保存在viewstate中，维持状态。
        if (e.ExecutingSelectCount)
        {
            //第二次调用SelectCountMethod时，只需要strWhere实参。
            e.InputParameters.Clear();
            e.InputParameters.Add("strWhere", this.odsGroupPurchaseList.SelectParameters["strWhere"].DefaultValue);
        }

    }

    protected void dvGroupPurchase_ItemInserting(object sender, DetailsViewInsertEventArgs e)
    {
        //DetailView会把所有绑定字段的键值放入e.Values集合，非绑定字段则不会自动处理，需要手工处理。
        try
        {
            Fruit fruit = new Fruit();
            fruit.ID = int.Parse(e.Values["ProductID"].ToString());
            e.Values.Remove("ProductID");
            e.Values.Add("Product", fruit);

            if (e.Values["Name"] == null)
            {
                throw new Exception("请输入团购名称");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["Name"].ToString());
            }
            if (e.Values["Description"] == null)
            {
                throw new Exception("请输入团购描述");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["Description"].ToString());
            }
            if (e.Values["RequiredNumber"] == null)
            {
                throw new Exception("请输入团购人数");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["RequiredNumber"].ToString());
            }
            if (e.Values["GroupPrice"] == null)
            {
                throw new Exception("请输入团购价格");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["GroupPrice"].ToString());
            }
            if (e.Values["StartDate"] == null)
            {
                throw new Exception("请输入团购开始日期");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["StartDate"].ToString());
            }
            if (e.Values["EndDate"] == null)
            {
                throw new Exception("请输入团购结束日期");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["EndDate"].ToString());
            }

        }
        catch (Exception ex)
        {
            this.lblErrMsg.Text = ex.Message;
            e.Cancel = true;
        }

    }

    protected void dvGroupPurchase_ItemInserted(object sender, DetailsViewInsertedEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
        else
        {
            this.dvGroupPurchase.Visible = false;
            //新增后刷新团购列表
            this.odsGroupPurchaseList.SelectParameters["strWhere"].DefaultValue = string.Empty;
            this.gvGroupPurchaseList.DataBind();
        }

    }

    protected void odsGroupPurchase_Inserted(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }

    }

    protected void dvGroupPurchase_ItemUpdating(object sender, DetailsViewUpdateEventArgs e)
    {
        if (e.NewValues["Name"] == null)
        {
            throw new Exception("请输入团购名称");
        }
        else
        {
            UtilityHelper.AntiSQLInjection(e.NewValues["Name"].ToString());
        }
        if (e.NewValues["Description"] == null)
        {
            throw new Exception("请输入团购描述");
        }
        else
        {
            UtilityHelper.AntiSQLInjection(e.NewValues["Description"].ToString());
        }
        if (e.NewValues["RequiredNumber"] == null)
        {
            throw new Exception("请输入团购人数");
        }
        else
        {
            UtilityHelper.AntiSQLInjection(e.NewValues["RequiredNumber"].ToString());
        }
        if (e.NewValues["GroupPrice"] == null)
        {
            throw new Exception("请输入团购价格");
        }
        else
        {
            UtilityHelper.AntiSQLInjection(e.NewValues["GroupPrice"].ToString());
        }
        if (e.NewValues["StartDate"] == null)
        {
            throw new Exception("请输入团购开始日期");
        }
        else
        {
            UtilityHelper.AntiSQLInjection(e.NewValues["StartDate"].ToString());
        }
        if (e.NewValues["EndDate"] == null)
        {
            throw new Exception("请输入团购结束日期");
        }
        else
        {
            UtilityHelper.AntiSQLInjection(e.NewValues["EndDate"].ToString());
        }
    }

    protected void dvGroupPurchase_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
        else
        {
            //更新后刷新团购列表
            this.odsGroupPurchase.SelectParameters[0].DefaultValue = string.Empty;
            this.dvGroupPurchase.DataBind();
            this.odsGroupPurchaseList.SelectParameters["strWhere"].DefaultValue = string.Empty;
            this.gvGroupPurchaseList.SelectedIndex = -1;
            this.gvGroupPurchaseList.DataBind();
        }

    }

    protected void odsGroupPurchase_Updated(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }

    }

    protected void odsGroupPurchaseList_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
    }

    protected void gvGroupPurchaseList_RowDeleted(object sender, GridViewDeletedEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
        else
        {
            //删除后刷新团购列表
            this.gvGroupPurchaseList.DataBind();
        }

    }

    protected void gvGroupPurchaseList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            GroupPurchase groupPurchase = e.Row.DataItem as GroupPurchase;
            Button btnDel = e.Row.FindControl("btnDel") as Button;

            //检测当前行是否有效的团购，并标记表格行底色
            GroupPurchase activeGroupPurchase = GroupPurchase.FindActiveGroupPurchase(groupPurchase.Product.ID, false, false);
            if (activeGroupPurchase != null && groupPurchase.ID == activeGroupPurchase.ID)
            {
                e.Row.CssClass = "success";
            }

            //只有此团购下尚未建立团购活动，才能删除团购
            if (groupPurchase.GroupEvents.Count == 0)
            {
                btnDel.Attributes.Add("onclick", "return confirm('您是否要删除这条团购？');");
            }
            else
            {
                btnDel.ToolTip = "此团购下已有活动，不能删除。";
                btnDel.Enabled = false;
            }
        }
    }

    protected void gvGroupPurchaseList_SelectedIndexChanged(object sender, EventArgs e)
    {
        ////根据用户选择的商品条目，显示商品详情
        this.odsGroupPurchase.SelectParameters[0].DefaultValue = this.gvGroupPurchaseList.SelectedDataKey[0].ToString();

        this.dvGroupPurchase.ChangeMode(DetailsViewMode.Edit);
        this.dvGroupPurchase.AutoGenerateInsertButton = false;
        this.dvGroupPurchase.AutoGenerateEditButton = true;

        this.dvGroupPurchase.DataBind();
        this.dvGroupPurchase.Visible = true;
        this.gvGroupPurchaseList.DataBind();

    }


    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string strWhere = string.Empty, tableName = string.Empty;
        List<string> listWhere = new List<string>();

        try
        {
            //查询条件：团购名称
            if (!string.IsNullOrEmpty(this.txtGroupPurchaseName.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtGroupPurchaseName.Text);
                listWhere.Add(string.Format("Name like '%{0}%'", this.txtGroupPurchaseName.Text.Trim()));
                this.txtGroupPurchaseName.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtGroupPurchaseName.Style.Clear();
            }

            //查询条件：团购活动ID
            if (!string.IsNullOrEmpty(this.txtGroupEventID.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtGroupEventID.Text);
                listWhere.Add(string.Format("Id in (select GroupID from GroupPurchaseEvent where Id = {0})", this.txtGroupEventID.Text.Trim()));
                this.txtGroupEventID.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtGroupEventID.Style.Clear();
            }

            strWhere = string.Join<string>(" and ", listWhere);

            this.gvGroupPurchaseList.PageIndex = 0;
            this.odsGroupPurchaseList.SelectParameters["strWhere"].DefaultValue = strWhere;

            this.gvGroupPurchaseList.DataBind();
            this.dvGroupPurchase.Visible = false;
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWarn", string.Format("alert('{0}');", ex.Message), true);
        }

    }

    protected void btnShowAll_Click(object sender, EventArgs e)
    {
        this.txtGroupPurchaseName.Text = string.Empty;
        this.txtGroupPurchaseName.Style.Clear();
        this.odsGroupPurchaseList.SelectParameters["strWhere"].DefaultValue = string.Empty;
        this.gvGroupPurchaseList.PageIndex = 0;
        this.gvGroupPurchaseList.DataBind();
        this.dvGroupPurchase.Visible = false;
    }

    protected void dvGroupPurchase_ItemCommand(object sender, DetailsViewCommandEventArgs e)
    {
        if (e.CommandName == "Cancel")
        {
            this.odsGroupPurchase.SelectParameters[0].DefaultValue = string.Empty;
            this.dvGroupPurchase.DataBind();
            this.odsGroupPurchaseList.SelectParameters["strWhere"].DefaultValue = string.Empty;
            this.gvGroupPurchaseList.SelectedIndex = -1;
            this.gvGroupPurchaseList.DataBind();
        }
    }

    protected void rpGroupEvents_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            GroupPurchaseEvent groupEvent = e.Item.DataItem as GroupPurchaseEvent;
            if (groupEvent != null)
            {
                HtmlContainerControl liGroupItem = e.Item.Controls[1] as HtmlContainerControl;
                Label lblEventStatus = liGroupItem.FindControl("lblEventStatus") as Label;

                //判断当前团购活动状态，并标记页面颜色
                switch (GroupPurchaseEvent.CheckGroupPurchaseEventStatus(groupEvent))
                {
                    case GroupEventStatus.EVENT_SUCCESS:
                        liGroupItem.Attributes["class"] += " list-group-item-success";
                        lblEventStatus.Text = "团购成功";
                        break;
                    case GroupEventStatus.EVENT_GOING:
                        liGroupItem.Attributes["class"] += " list-group-item-info";
                        lblEventStatus.Text = "团购进行中";
                        break;
                    case GroupEventStatus.EVENT_FAIL:
                        liGroupItem.Attributes["class"] += " list-group-item-danger";
                        lblEventStatus.Text = "团购失败";
                        break;
                }
            }
        }
    }

}