using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Web.Security;

public partial class admin_ManageUser : System.Web.UI.Page
{
    /// <summary>
    /// 查询条件框的背景色
    /// </summary>
    protected static readonly System.Drawing.Color CRITERIA_BG_COLOR = System.Drawing.Color.Pink;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            this.odsUserList.SelectParameters.Add("tableName", DbType.String, "WeChatUsers");
            this.odsUserList.SelectParameters.Add("pk", DbType.String, "WeChatUsers.Id");
            this.odsUserList.SelectParameters.Add("fieldsName", DbType.String, "WeChatUsers.*,(select count(*) from ProductOrder where WeChatUsers.OpenID = ProductOrder.OpenID) as OrderCount");
            this.odsUserList.SelectParameters.Add("strWhere", DbType.String, string.Empty);
            this.odsUserList.SelectParameters.Add("strOrder", DbType.String, string.Empty);
            this.odsUserList.SelectParameters[this.odsUserList.SelectParameters.Add("totalRecords", DbType.Int32, "0")].Direction = ParameterDirection.Output;

            this.gvUserList.PageIndex = 0;
            this.gvUserList.PageSize = Config.UserListPageSize;

        }
    }

    protected void cbIsApproved_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cbIsApproved = sender as CheckBox;
        Guid userId = Guid.Parse(cbIsApproved.Attributes["UserId"]);
        MembershipUser mUser = Membership.GetUser(userId);
        if (mUser != null)
        {
            mUser.IsApproved = cbIsApproved.Checked;
            Membership.UpdateUser(mUser);
        }
        this.gvUserList.DataBind();
    }

    protected void odsUserList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
        //自定义查询时，GridView控件会调用两次数据源的SelectMethod，第一次调用SelectMethod，第二次调用SelectCountMethod。每次调用都会把SelectParameters集合复制给e.InputParameters集合。SelectParameters也会在每次的post时保存在viewstate中，维持状态。
        if (e.ExecutingSelectCount)
        {
            //第二次调用SelectCountMethod时，只需要tableName，strWhere实参。
            e.InputParameters.Clear();
            e.InputParameters.Add("tableName", this.odsUserList.SelectParameters["tableName"].DefaultValue);
            e.InputParameters.Add("strWhere", this.odsUserList.SelectParameters["strWhere"].DefaultValue);
        }
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string strWhere = string.Empty, strTableName = string.Empty;
        List<string> listWhere = new List<string>();
        bool isJoinMembership = false, isJoinUsers = false;
        try
        {
            //查询条件：是否在线
            if (this.ddlIsOnline.SelectedIndex == 0)
            {
                this.ddlIsOnline.Style.Clear();
            }
            else
            {
                if (this.ddlIsOnline.SelectedValue == "1")
                {
                    UtilityHelper.AntiSQLInjection(this.ddlIsOnline.SelectedValue);
                    listWhere.Add(string.Format("DATEADD(mi,-{0},GETDATE())<=aspnet_Users.LastActivityDate", Membership.UserIsOnlineTimeWindow + 480));
                    this.ddlIsOnline.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
                    isJoinUsers = true;
                }
                else
                {
                    UtilityHelper.AntiSQLInjection(this.ddlIsOnline.SelectedValue);
                    listWhere.Add(string.Format("DATEADD(mi,-{0},GETDATE())>aspnet_Users.LastActivityDate", Membership.UserIsOnlineTimeWindow + 480));
                    this.ddlIsOnline.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
                    isJoinUsers = true;
                }
            }

            //查询条件：是否订阅微信公众号
            if (this.ddlIsSubscribe.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlIsSubscribe.SelectedValue);
                listWhere.Add(string.Format("WeChatUsers.IsSubscribe = {0}", this.ddlIsSubscribe.SelectedValue));
                this.ddlIsSubscribe.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlIsSubscribe.Style.Clear();
            }

            //查询条件：性别
            if (this.ddlSex.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlSex.SelectedValue);
                listWhere.Add(string.Format("WeChatUsers.Sex = {0}", this.ddlSex.SelectedValue));
                this.ddlSex.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlSex.Style.Clear();
            }

            //查询条件：是否允许登录
            if (this.ddlIsApproved.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlIsApproved.SelectedValue);
                listWhere.Add(string.Format("aspnet_Membership.IsApproved = {0}", this.ddlIsApproved.SelectedValue));
                this.ddlIsApproved.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
                isJoinMembership = true;
            }
            else
            {
                this.ddlIsApproved.Style.Clear();
            }

            //查询条件：微信昵称
            if (!string.IsNullOrEmpty(this.txtNickName.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtNickName.Text);
                listWhere.Add(string.Format("WeChatUsers.NickName like '%{0}%'", this.txtNickName.Text.Trim()));
                this.txtNickName.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtNickName.Style.Clear();
            }

            //查询条件：国家
            if (!string.IsNullOrEmpty(this.txtCountry.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtCountry.Text);
                listWhere.Add(string.Format("WeChatUsers.Country like '%{0}%'", this.txtCountry.Text.Trim()));
                this.txtCountry.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtCountry.Style.Clear();
            }

            //查询条件：省份
            if (!string.IsNullOrEmpty(this.txtProvince.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtProvince.Text);
                listWhere.Add(string.Format("WeChatUsers.Province like '%{0}%'", this.txtProvince.Text.Trim()));
                this.txtProvince.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtProvince.Style.Clear();
            }

            //查询条件：城市
            if (!string.IsNullOrEmpty(this.txtCity.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtCountry.Text);
                listWhere.Add(string.Format("WeChatUsers.City like '%{0}%'", this.txtCity.Text.Trim()));
                this.txtCity.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtCity.Style.Clear();
            }

            //查询条件：开始注册日期
            if (!string.IsNullOrEmpty(this.txtStartCreationDate.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtStartCreationDate.Text);
                listWhere.Add(string.Format("CONVERT(varchar(8), aspnet_Membership.CreateDate, 112) >= '{0}'", this.txtStartCreationDate.Text.Trim().Replace("-", "")));
                this.txtStartCreationDate.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
                isJoinMembership = true;
            }
            else
            {
                this.txtStartCreationDate.Style.Clear();
            }

            //查询条件：结束注册日期
            if (!string.IsNullOrEmpty(this.txtEndCreationDate.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtEndCreationDate.Text);
                listWhere.Add(string.Format("CONVERT(varchar(8), aspnet_Membership.CreateDate, 112) <= '{0}'", this.txtEndCreationDate.Text.Trim().Replace("-", "")));
                this.txtEndCreationDate.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
                isJoinMembership = true;
            }
            else
            {
                this.txtEndCreationDate.Style.Clear();
            }

            //查询条件：开始活跃时间
            if (!string.IsNullOrEmpty(this.txtStartLastActivityDate.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtStartLastActivityDate.Text);
                listWhere.Add(string.Format("CONVERT(varchar(8), DATEADD(hh,8,aspnet_Users.LastActivityDate), 112) >= '{0}'", this.txtStartLastActivityDate.Text.Trim().Replace("-", "")));
                this.txtStartLastActivityDate.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
                isJoinUsers = true;
            }
            else
            {
                this.txtStartLastActivityDate.Style.Clear();
            }

            //查询条件：结束活跃时间
            if (!string.IsNullOrEmpty(this.txtEndLastActivityDate.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtEndLastActivityDate.Text);
                listWhere.Add(string.Format("CONVERT(varchar(8), DATEADD(hh,8,aspnet_Users.LastActivityDate), 112) <= '{0}'", this.txtEndLastActivityDate.Text.Trim().Replace("-", "")));
                this.txtEndLastActivityDate.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
                isJoinUsers = true;
            }
            else
            {
                this.txtEndLastActivityDate.Style.Clear();
            }

            strWhere = string.Join<string>(" and ", listWhere);
            this.odsUserList.SelectParameters["strWhere"].DefaultValue = strWhere;
            //根据查询涉及表做关联
            strTableName = "WeChatUsers";
            if (isJoinMembership)
            {
                strTableName += " left join aspnet_Membership on WeChatUsers.UserId = aspnet_Membership.UserId";
            }
            if (isJoinUsers)
            {
                strTableName += " left join aspnet_Users on WeChatUsers.UserId = aspnet_Users.UserId";
            }
            this.odsUserList.SelectParameters["tableName"].DefaultValue = strTableName;

            this.gvUserList.PageIndex = 0;
            this.gvUserList.DataBind();
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWarn", string.Format("alert('{0}');", ex.Message), true);
        }
    }

    protected void btnShowAll_Click(object sender, EventArgs e)
    {
        this.ddlIsOnline.SelectedIndex = 0;
        this.ddlIsOnline.Style.Clear();

        this.ddlIsSubscribe.SelectedIndex = 0;
        this.ddlIsSubscribe.Style.Clear();

        this.ddlSex.SelectedIndex = 0;
        this.ddlSex.Style.Clear();

        this.ddlIsApproved.SelectedIndex = 0;
        this.ddlIsApproved.Style.Clear();

        this.txtNickName.Text = string.Empty;
        this.txtNickName.Style.Clear();

        this.txtCountry.Text = string.Empty;
        this.txtCountry.Style.Clear();

        this.txtProvince.Text = string.Empty;
        this.txtProvince.Style.Clear();

        this.txtCity.Text = string.Empty;
        this.txtCity.Style.Clear();

        this.txtStartCreationDate.Text = string.Empty;
        this.txtStartCreationDate.Style.Clear();

        this.txtEndCreationDate.Text = string.Empty;
        this.txtEndCreationDate.Style.Clear();

        this.txtStartLastActivityDate.Text = string.Empty;
        this.txtStartLastActivityDate.Style.Clear();

        this.txtEndLastActivityDate.Text = string.Empty;
        this.txtEndLastActivityDate.Style.Clear();

        this.odsUserList.SelectParameters["tableName"].DefaultValue = "WeChatUsers";
        this.odsUserList.SelectParameters["strWhere"].DefaultValue = string.Empty;
        this.gvUserList.PageIndex = 0;
        this.gvUserList.DataBind();
    }


    protected void odsUserList_Selected(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.ReturnValue is List<WeChatUser> && e.OutputParameters.Count != 0 && e.OutputParameters["totalRecords"] != null)
        {
            this.lblSearchResult.Text = e.OutputParameters["totalRecords"].ToString();

            //更新此页用户的微信信息
            WeChatUserDAO.RefreshWxUserInfo(e.ReturnValue as List<WeChatUser>);

        }
    }

    protected void gvUserList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            WeChatUser wxUser = (WeChatUser)e.Row.DataItem;

            //是否在线图标
            HtmlContainerControl divIsOnline = e.Row.FindControl("divIsOnline") as HtmlContainerControl;
            if (wxUser.IsOnline)
            {
                divIsOnline.InnerHtml = "<i class=\"fa fa-check\"></i>";
                divIsOnline.Attributes["title"] = "在线";
            }
            else
            {
                divIsOnline.InnerHtml = "<i class=\"fa fa-close\"></i>";
                divIsOnline.Attributes["title"] = "离线";
            }

            //是否关注公众号图标
            HtmlContainerControl divIsSubscribe = e.Row.FindControl("divIsSubscribe") as HtmlContainerControl;
            if (wxUser.IsSubscribe)
            {
                divIsSubscribe.InnerHtml = "<i class=\"fa fa-check\"></i>";
                divIsSubscribe.Attributes["title"] = "已关注";
            }
            else
            {
                divIsSubscribe.InnerHtml = "<i class=\"fa fa-close\"></i>";
                divIsSubscribe.Attributes["title"] = "未关注";
            }

            //是否允许登录
            CheckBox cbIsApproved = e.Row.FindControl("cbIsApproved") as CheckBox;
            if (wxUser.ProviderUserKey != null)
            {
                cbIsApproved.Attributes["UserId"] = wxUser.ProviderUserKey.ToString();
                if (wxUser.IsApproved)
                {
                    cbIsApproved.Attributes["onclick"] = "if(!confirm('是否禁止此用户登录？')){return false;}";
                }
                else
                {
                    cbIsApproved.Attributes["onclick"] = "if(!confirm('是否允许此用户登录？')){return false;}";
                }
            }
            else
            {
                cbIsApproved.Enabled = false;
                cbIsApproved.ToolTip = "此用户没有成员资格信息，请核查。";
            }

        }
    }

}