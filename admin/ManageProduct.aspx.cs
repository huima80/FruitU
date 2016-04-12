using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;

public partial class ManageProduct : System.Web.UI.Page
{
    /// <summary>
    /// gridstack.js定义的网格列数
    /// </summary>
    protected const int GRID_COL = 12;

    /// <summary>
    /// gridstack.js定义的每个网格的宽度
    /// </summary>
    protected const int GRID_ITEM_WIDTH = 3;

    /// <summary>
    /// gridstack.js定义的每个网格的高度
    /// </summary>
    protected const int GRID_ITEM_HEIGHT = 4;

    /// <summary>
    /// 查询条件框的背景色
    /// </summary>
    protected static readonly System.Drawing.Color CRITERIA_BG_COLOR = System.Drawing.Color.Pink;

    protected void Page_Load(object sender, EventArgs e)
    {
        if(!Page.IsPostBack)
        {
            this.odsFruitList.TypeName = "Fruit";
            this.odsFruitList.EnablePaging = true;

            this.odsFruitList.SelectParameters.Add("tableName", DbType.String, "Product");
            this.odsFruitList.SelectParameters.Add("pk", DbType.String, "Product.Id");
            this.odsFruitList.SelectParameters.Add("fieldsName", DbType.String, "Product.*");
            this.odsFruitList.SelectParameters.Add("strWhere", DbType.String, string.Empty);
            this.odsFruitList.SelectParameters.Add("strOrder", DbType.String, string.Empty);
            this.odsFruitList.SelectParameters[this.odsFruitList.SelectParameters.Add("totalRows", DbType.Int32, "0")].Direction = ParameterDirection.Output;

            this.gvFruitList.AllowPaging = true;
            this.gvFruitList.AllowCustomPaging = true;
            this.gvFruitList.PageIndex = 0;
            this.gvFruitList.PageSize = Config.ProductListPageSize;

            BindCategoryDDL();

        }
    }

    /// <summary>
    /// 绑定按类别查询下拉框数据源
    /// </summary>
    protected void BindCategoryDDL()
    {
        this.ddlCategory.DataSource = MakeCategoryDataSource();
        this.ddlCategory.DataValueField = "Key";
        this.ddlCategory.DataTextField = "Value";
        this.ddlCategory.DataBind();
    }

    protected void gvFruitList_SelectedIndexChanged(object sender, EventArgs e)
    {
        //根据用户选择的商品条目，显示商品详情
        this.odsFruit.SelectParameters[0].DefaultValue = this.gvFruitList.SelectedDataKey[0].ToString();

        this.dvFruit.ChangeMode(DetailsViewMode.Edit);
        this.dvFruit.AutoGenerateInsertButton = false;
        this.dvFruit.AutoGenerateEditButton = true;
        
        this.dvFruit.DataBind();
        this.gvFruitList.DataBind();

    }

    protected void btnAddFruit_Click(object sender, EventArgs e)
    {
        this.gvFruitList.SelectedIndex = -1;

        this.odsFruit.SelectParameters[0].DefaultValue = string.Empty;

        this.dvFruit.ChangeMode(DetailsViewMode.Insert);
        this.dvFruit.AutoGenerateInsertButton = true;
        this.dvFruit.AutoGenerateEditButton = false;

        this.gvFruitList.DataBind();
    }

    protected void odsFruitList_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
        //自定义查询时，GridView控件会调用两次数据源的SelectMethod，第一次调用SelectMethod，第二次调用SelectCountMethod。每次调用都会把SelectParameters集合复制给e.InputParameters集合。SelectParameters也会在每次的post时保存在viewstate中，维持状态。
        if (e.ExecutingSelectCount)
        {
            //第二次调用SelectCountMethod时，只需要tableName，strWhere实参。
            e.InputParameters.Clear();
            e.InputParameters.Add("tableName", this.odsFruitList.SelectParameters["tableName"].DefaultValue);
            e.InputParameters.Add("strWhere", this.odsFruitList.SelectParameters["strWhere"].DefaultValue);
        }

    }

    protected void gvFruitList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton btnDel = (LinkButton)e.Row.Controls[0].Controls[0];
            btnDel.Attributes.Add("onclick", "return confirm('您是否要删除这条信息？');");


        }
    }

    protected void odsFruit_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
        if(e.InputParameters["fruitID"] == null)
        {
            e.Cancel = true;
        }
    }

    protected void odsFruitList_Deleted(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
        else
        {
            int delFruitCount, delImgCount;
            delFruitCount = int.Parse(e.OutputParameters["delFruitCount"].ToString());
            delImgCount = int.Parse(e.OutputParameters["delImgCount"].ToString());
            this.lblErrMsg.Text = string.Format("删除了{0}个商品、{1}个图片", delFruitCount, delImgCount);
        }

    }

    protected void odsFruit_Inserted(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }

    }

    protected void odsFruit_Updated(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
    }

    protected void odsFruit_Inserting(object sender, ObjectDataSourceMethodEventArgs e)
    {
        //此时数据源控件已把数据绑定控件传来的值根据TypeName指定的业务实体类，生成了e.InputParameters参数集合，其实就是Insert方法的形参。
    }

    protected void dvFruit_ItemInserting(object sender, DetailsViewInsertEventArgs e)
    {
        //DetailView会把所有绑定字段的键值放入e.Values集合，其他模板列则不会自动处理，需要手工处理。
        try
        {
            if (e.Values["FruitName"] == null)
            {
                throw new Exception("请输入商品名称");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["FruitName"].ToString());
            }

            DropDownList ddlCategoryInsert = ((DetailsView)sender).FindControl("ddlCategoryInsert") as DropDownList;

            if (ddlCategoryInsert.SelectedIndex == 0)
            {
                throw new Exception("请选择商品类别");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(ddlCategoryInsert.SelectedValue);
            }

            if (e.Values["FruitPrice"] == null)
            {
                throw new Exception("请输入商品价格");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["FruitPrice"].ToString());
            }

            if (e.Values["FruitUnit"] == null)
            {
                throw new Exception("请输入商品单位");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["FruitUnit"].ToString());
            }

            if (e.Values["FruitDesc"] == null)
            {
                throw new Exception("请输入商品描述");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["FruitDesc"].ToString());
            }

            if (e.Values["InventoryQty"] == null)
            {
                throw new Exception("请输入商品库存数量");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["InventoryQty"].ToString());
            }

            if (e.Values["OnSale"] == null)
            {
                throw new Exception("请输入是否上架");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["OnSale"].ToString());
            }

            if (e.Values["IsSticky"] == null)
            {
                throw new Exception("请输入是否置顶");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["IsSticky"].ToString());
            }

            if (e.Values["Priority"] == null)
            {
                e.Values["Priority"] = 0;
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.Values["Priority"].ToString());
            }

            //获取商品类别信息，添加入e.Values集合
            Category category = new Category();
            category.ID = int.Parse(ddlCategoryInsert.SelectedValue);
            e.Values.Add("Category", category);

            HttpFileCollection imgFiles = Request.Files;
            if (imgFiles.Count != 0)
            {
                List<FruitImg> fruitImgList = new List<FruitImg>();
                FruitImg fruitImg;

                //新上传图片的备注信息，如果是多个图片，则以,分割
                TextBox txtImgDesc = ((DetailsView)sender).FindControl("txtImgDescInsert") as TextBox;

                //根据gridstack.js网格数、网格项宽度参数计算每行的网格项数量，用于后续计算每个网格项的X/Y坐标值
                int gridItemCount = GRID_COL / GRID_ITEM_WIDTH, currentIndex;

                //遍历文件上传框
                for (int i = 0; i < imgFiles.Count; i++)
                {
                    HttpPostedFile imgFile = imgFiles[i];
                    if (imgFile.ContentLength == 0) continue;
                    string fileName, fileExtension;
                    fileName = System.IO.Path.GetFileName(imgFile.FileName);
                    fileExtension = System.IO.Path.GetExtension(fileName);
                    if (Regex.IsMatch(fileExtension, string.Format("({0})", Config.AllowedUploadFileExt), RegexOptions.IgnoreCase))
                    {
                        fruitImg = new FruitImg();
                        fruitImgList.Add(fruitImg);

                        fruitImg.ImgName = fileName;
                        fruitImg.ImgDesc = txtImgDesc.Text.Split(',')[i];
                        fruitImg.DetailImg = false;

                        //根据上传图片的序列号计算其gridstack的X/Y坐标值
                        currentIndex = (fruitImgList.Count - 1);
                        fruitImg.ImgSeqX = (currentIndex - currentIndex / gridItemCount * gridItemCount) * GRID_ITEM_WIDTH;
                        fruitImg.ImgSeqY = currentIndex / gridItemCount * GRID_ITEM_HEIGHT;

                        //保存新图片文件
                        imgFile.SaveAs(Request.MapPath("~/images/") + fileName);

                    }
                }

                if (fruitImgList.Count > 0)
                {
                    //默认新上传的第一个是主图
                    fruitImgList[0].MainImg = true;

                    //把需要上传的图片信息加入DetailView控件的e.Values集合中，留待数据源控件odsFruit调用Fruit对象的insert方法一起插入
                    e.Values.Add("FruitImgList", fruitImgList);
                }
            }
            else
            {
                throw new Exception("请选择商品图片");
            }
        }
        catch (Exception ex)
        {
            this.lblErrMsg.Text = ex.Message;
            e.Cancel = true;
        }
    }

    protected void dvFruit_ItemInserted(object sender, DetailsViewInsertedEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
        else
        {
            this.gvFruitList.DataBind();
        }

    }

    protected void dvFruit_ItemUpdating(object sender, DetailsViewUpdateEventArgs e)
    {
        try
        {
            //当前的DetailView控件对象和当前记录主键值
            DetailsView dvFruit = sender as DetailsView;
            int fruitID = int.Parse(dvFruit.DataKey.Value.ToString());

            if (e.NewValues["FruitName"] == null)
            {
                throw new Exception("请输入商品名称");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["FruitName"].ToString());
            }

            DropDownList ddlCategory = dvFruit.FindControl("ddlCategoryEdit") as DropDownList;

            if (ddlCategory.SelectedIndex == 0)
            {
                throw new Exception("请选择商品类别");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(ddlCategory.SelectedValue);
            }

            if (e.NewValues["FruitPrice"] == null)
            {
                throw new Exception("请输入商品价格");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["FruitPrice"].ToString());
            }

            if (e.NewValues["FruitUnit"] == null)
            {
                throw new Exception("请输入商品单位");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["FruitUnit"].ToString());
            }

            if (e.NewValues["FruitDesc"] == null)
            {
                throw new Exception("请输入商品描述");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["FruitDesc"].ToString());
            }

            if (e.NewValues["InventoryQty"] == null)
            {
                throw new Exception("请输入商品库存数量");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["InventoryQty"].ToString());
            }

            if (e.NewValues["OnSale"] == null)
            {
                throw new Exception("请输入是否上架");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["OnSale"].ToString());
            }

            if (e.NewValues["IsSticky"] == null)
            {
                throw new Exception("请输入是否置顶");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["IsSticky"].ToString());
            }

            if (e.NewValues["Priority"] == null)
            {
                e.NewValues["Priority"] = 0;
            }
            else
            {
                UtilityHelper.AntiSQLInjection(e.NewValues["Priority"].ToString());
            }

            //获取商品类别信息，添加入e.Values集合
            Category category = new Category();
            category.ID = int.Parse(ddlCategory.SelectedValue);
            e.NewValues.Add("Category", category);

            List<FruitImg> fruitImgList = new List<FruitImg>();
            FruitImg fruitImg;

            //处理原有的图片：在DetailsView嵌套的Repeater中查找所有的子控件，并新建FruitImg对象，添加到集合中
            Repeater rpFruitImgList = dvFruit.FindControl("rpFruitImgList") as Repeater;
            for (int i = 0; i < rpFruitImgList.Items.Count; i++)
            {
                HiddenField hfImgID = rpFruitImgList.Items[i].FindControl("hfImgID") as HiddenField;
                HiddenField hfImgSeqX = rpFruitImgList.Items[i].FindControl("hfImgSeqX") as HiddenField;
                HiddenField hfImgSeqY = rpFruitImgList.Items[i].FindControl("hfImgSeqY") as HiddenField;
                HyperLink hlOriginalImg = rpFruitImgList.Items[i].FindControl("hlOriginalImg") as HyperLink;
                RadioButton rbMainImg = rpFruitImgList.Items[i].FindControl("rbMainImg") as RadioButton;
                CheckBox cbDetailImg = rpFruitImgList.Items[i].FindControl("cbDetailImg") as CheckBox;
                TextBox txtImgDescEditOriginal = rpFruitImgList.Items[i].FindControl("txtImgDescEditOriginal") as TextBox;

                fruitImg = new FruitImg();
                fruitImgList.Add(fruitImg);

                fruitImg.ImgID = int.Parse(hfImgID.Value);
                fruitImg.ImgName = hlOriginalImg.Text;
                fruitImg.ImgDesc = txtImgDescEditOriginal.Text.Trim();
                fruitImg.MainImg = rbMainImg.Checked;
                fruitImg.DetailImg = cbDetailImg.Checked;

                int imgSeqX, imgSeqY;
                if (!int.TryParse(hfImgSeqX.Value, out imgSeqX))
                {
                    imgSeqX = 0;
                }
                if (!int.TryParse(hfImgSeqY.Value, out imgSeqY))
                {
                    imgSeqY = 0;
                }
                fruitImg.ImgSeqX = imgSeqX;
                fruitImg.ImgSeqY = imgSeqY;

            }
            //把FruitImgList对象附加到DetailView控件的e.NewValues键值对中，留待数据源控件odsFruit调用Fruit对象的update方法一起更新
            e.NewValues.Add("FruitImgList", fruitImgList);

            //处理新上传的图片：把新上传的图片信息先行入库，并保存到服务器磁盘
            HttpFileCollection imgFiles = Request.Files;
            if (imgFiles.Count != 0)
            {
                //存放新上传的图片
                List<FruitImg> newFruitImgList = new List<FruitImg>();
                FruitImg newFruitImg;

                //新上传图片的备注信息，如果是多个图片，则以,分割
                TextBox txtImgDesc = dvFruit.FindControl("txtImgDescEdit") as TextBox;

                int gridItemCount = GRID_COL / GRID_ITEM_WIDTH, currentIndex = fruitImgList.Count;

                for (int i = 0; i < imgFiles.Count; i++)
                {
                    HttpPostedFile imgFile = imgFiles[i];
                    if (imgFile.ContentLength == 0) continue;
                    string fileName, fileExtension;
                    fileName = System.IO.Path.GetFileName(imgFile.FileName);
                    fileExtension = System.IO.Path.GetExtension(fileName);
                    if (Regex.IsMatch(fileExtension, string.Format("({0})", Config.AllowedUploadFileExt), RegexOptions.IgnoreCase))
                    {
                        newFruitImg = new FruitImg();
                        newFruitImgList.Add(newFruitImg);

                        newFruitImg.ImgName = fileName;
                        newFruitImg.ImgDesc = txtImgDesc.Text.Split(',')[i];
                        newFruitImg.MainImg = false;
                        newFruitImg.DetailImg = false;

                        //根据上传图片的序列号计算其gridstack的X/Y坐标值
                        newFruitImg.ImgSeqX = (currentIndex - currentIndex / gridItemCount * gridItemCount) * GRID_ITEM_WIDTH;
                        newFruitImg.ImgSeqY = currentIndex / gridItemCount * GRID_ITEM_HEIGHT;

                        currentIndex++;

                        //保存新图片文件
                        imgFile.SaveAs(Request.MapPath("~/images/") + fileName);

                    }
                }

                if (newFruitImgList.Count > 0)
                {
                    //新图片信息入库
                    Fruit.AddFruitImg(fruitID, newFruitImgList);
                }
            }

        }
        catch (Exception ex)
        {
            this.lblErrMsg.Text = ex.Message;
            e.Cancel = true;
        }
    }

    protected void dvFruit_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
        else
        {
            this.gvFruitList.DataBind();
        }
    }


    protected void odsFruit_Selected(object sender, ObjectDataSourceStatusEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }

    }

    protected void gvFruitList_RowDeleted(object sender, GridViewDeletedEventArgs e)
    {
        if (e.Exception != null)
        {
            this.lblErrMsg.Text = e.Exception.Message;
            e.ExceptionHandled = true;
        }
        else
        {
            //删除记录后，清空选定的已被删除的记录，并重新绑定dvFruit控件
            this.odsFruit.SelectParameters[0].DefaultValue = string.Empty;
            this.dvFruit.DataBind();

            this.gvFruitList.DataBind();
        }
    }

    protected void odsFruitList_Deleting(object sender, ObjectDataSourceMethodEventArgs e)
    {
    }

    protected void gvFruitList_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
    }

    protected void btnDelImg_Click(object sender, EventArgs e)
    {
        Button btnDelImg = sender as Button;
        int imgID = int.Parse(btnDelImg.CommandArgument.ToString());
        Fruit.DelFruitImg(imgID);

        this.dvFruit.DataBind();

    }

    public List<KeyValuePair<int, string>> MakeCategoryDataSource()
    {
        //所有的目录列表
        List<Category> categoryList = Category.FindAllCategory();

        //目录树键值对，用作绑定DDL
        List<KeyValuePair<int, string>> kvpList = new List<KeyValuePair<int, string>>();

        int totalWidth = 0;

        kvpList.Add(new KeyValuePair<int, string>(0, "===请选择类别==="));

        this.GetChildCategory(0, categoryList, kvpList, totalWidth);

        return kvpList;
    }

    private void GetChildCategory(int parentID, List<Category> categoryList, List<KeyValuePair<int, string>> kvpList, int totalWidth)
    {
        List<Category> childCategoryList;
        childCategoryList = categoryList.FindAll((category) =>
        {
            return category.ParentID == parentID;
        });

        if (childCategoryList.Count != 0)
        {
            totalWidth += 2;

            if (parentID == 0)
            {
                childCategoryList.ForEach(childCategory =>
                {
                    kvpList.Add(new KeyValuePair<int, string>(childCategory.ID, "╋".PadLeft(totalWidth, (char)0xA0) + childCategory.CategoryName));

                    this.GetChildCategory(childCategory.ID, categoryList, kvpList, totalWidth);
                });

            }
            else
            {
                childCategoryList.ForEach(childCategory =>
                {
                    kvpList.Add(new KeyValuePair<int, string>(childCategory.ID, "├".PadLeft(totalWidth, (char)0xA0) + childCategory.CategoryName));

                    this.GetChildCategory(childCategory.ID, categoryList, kvpList, totalWidth);
                });
            }
        }
    }



    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string strWhere = string.Empty, tableName = string.Empty;
        List<string> listWhere = new List<string>();

        try
        {
            //查询条件：商品类别
            if (this.ddlCategory.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlCategory.SelectedValue);
                listWhere.Add(string.Format("CategoryID = {0}", this.ddlCategory.SelectedValue));
                this.ddlCategory.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlCategory.Style.Clear();
            }

            //查询条件：是否置顶
            if (this.ddlIsSticky.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlIsSticky.SelectedValue);
                listWhere.Add(string.Format("IsSticky = {0}", this.ddlIsSticky.SelectedValue));
                this.ddlIsSticky.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlIsSticky.Style.Clear();
            }

            //查询条件：是否上架
            if (this.ddlIsOnSale.SelectedIndex != 0)
            {
                UtilityHelper.AntiSQLInjection(this.ddlIsOnSale.SelectedValue);
                listWhere.Add(string.Format("ProductOnSale = {0}", this.ddlIsOnSale.SelectedValue));
                this.ddlIsOnSale.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.ddlIsOnSale.Style.Clear();
            }

            //查询条件：商品名称
            if (!string.IsNullOrEmpty(this.txtProdName.Text.Trim()))
            {
                UtilityHelper.AntiSQLInjection(this.txtProdName.Text);
                listWhere.Add(string.Format("ProductName like '%{0}%'", this.txtProdName.Text.Trim()));
                this.txtProdName.Style.Add("background-color", CRITERIA_BG_COLOR.Name);
            }
            else
            {
                this.txtProdName.Style.Clear();
            }

            strWhere = string.Join<string>(" and ", listWhere);

            this.gvFruitList.PageIndex = 0;
            this.odsFruitList.SelectParameters["strWhere"].DefaultValue = strWhere;
            this.odsFruitList.SelectParameters["tableName"].DefaultValue = "Product";

            this.gvFruitList.DataBind();
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsWarn", string.Format("alert('{0}');", ex.Message), true);
        }
    }

    protected void btnShowAll_Click(object sender, EventArgs e)
    {
        this.ddlCategory.SelectedIndex = 0;
        this.ddlCategory.Style.Clear();

        this.ddlIsSticky.SelectedIndex = 0;
        this.ddlIsSticky.Style.Clear();

        this.ddlIsOnSale.SelectedIndex = 0;
        this.ddlIsOnSale.Style.Clear();

        this.txtProdName.Text = string.Empty;
        this.txtProdName.Style.Clear();

        this.gvFruitList.PageIndex = 0;
        this.odsFruitList.SelectParameters["tableName"].DefaultValue = "Product";
        this.odsFruitList.SelectParameters["strWhere"].DefaultValue = string.Empty;
        this.gvFruitList.DataBind();

    }


    protected void rpFruitImgList_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if(e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            FruitImg fruitImg = e.Item.DataItem as FruitImg;
            HtmlContainerControl divGridStackItem = e.Item.FindControl("divGridStackItem") as HtmlContainerControl;

            //把每个图片的gridstack.js的X/Y坐标值写入前端的HTML属性中
            divGridStackItem.Attributes["data-gs-x"] = fruitImg.ImgSeqX.ToString();
            divGridStackItem.Attributes["data-gs-y"] = fruitImg.ImgSeqY.ToString();
            divGridStackItem.Attributes["data-gs-width"] = GRID_ITEM_WIDTH.ToString();
            divGridStackItem.Attributes["data-gs-height"] = GRID_ITEM_HEIGHT.ToString();
        }
    }

    protected void dvFruit_ModeChanged(object sender, EventArgs e)
    {
        this.gvFruitList.DataBind();
    }
}