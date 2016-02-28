using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;

public partial class ManageCategory : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindCategoryTree();
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        Button btnSubmit = sender as Button;
        Category category;

        try
        {

            switch (this.hfFlag.Value)
            {
                case "add":
                    category = new Category();
                    category.ParentID = int.Parse(this.hfParentID.Value);
                    category.CategoryName = this.txtCategoryName.Text.Trim();
                    Category.AddCategory(category);
                    this.lblMsg.Text = string.Format("类别【{0}】新增成功。", this.txtCategoryName.Text.Trim());
                    break;
                case "update":
                    category = new Category();
                    category.ID = int.Parse(this.hfCategoryID.Value);
                    category.ParentID = int.Parse(this.hfParentID.Value);
                    category.CategoryName = this.txtCategoryName.Text.Trim();
                    Category.UpdateCategory(category);
                    this.lblMsg.Text = string.Format("类别【{0}】修改成功。", this.txtCategoryName.Text.Trim());
                    break;
                case "del":
                    int categoryID = int.Parse(this.hfCategoryID.Value);
                    bool hasSubCategory = Category.HasSubCategoryByID(categoryID);

                    if (!hasSubCategory)
                    {
                        List<Fruit> fruitList = Fruit.FindFruitByCategoryID(categoryID);

                        if (fruitList.Count == 0)
                        {
                            Category.DelCategory(categoryID);
                            this.lblMsg.Text = string.Format("类别【{0}】删除成功。", this.txtCategoryName.Text.Trim());
                        }
                        else
                        {
                            this.lblMsg.Text = string.Format("类别【{0}】下还有<span style='color:red;'>{1}</span>个商品，不能删除。", this.txtCategoryName.Text.Trim(), fruitList.Count);
                        }
                    }
                    else
                    {
                        this.lblMsg.Text = string.Format("类别【{0}】下还有子类别，不能删除。", this.txtCategoryName.Text.Trim());
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            this.lblMsg.Text = ex.Message;
        }
        finally
        {
            this.hfFlag.Value = string.Empty;

            BindCategoryTree();
        }


    }

    /// <summary>
    /// 获取全部目录List<Category>列表，并返回JSON字符串，给前端JS生成目录树
    /// </summary>
    public void BindCategoryTree()
    {
        string categoryJSON = string.Empty;

        //商品类别根节点
        Category categoryRoot = new Category();
        categoryRoot.ID = 0;
        categoryRoot.ParentID = -1;
        categoryRoot.CategoryName = "商品类别";

        List<Category> categoryList = Category.FindAllCategory();
        categoryList.Insert(0, categoryRoot);
        categoryJSON = JsonMapper.ToJson(categoryList);

        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsTree", string.Format("var data={0};", categoryJSON), true);

    }
}