using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

public partial class ProductDetail : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int prodID;

        try
        {
            if (string.IsNullOrEmpty(Request.QueryString["ProdID"]))
            {
                throw new Exception("缺少商品参数ProdID");
            }

            UtilityHelper.AntiSQLInjection(Request.QueryString["ProdID"]);

            prodID = int.Parse(Request.QueryString["ProdID"]);

            Fruit fruit = Fruit.FindFruitByID(prodID);

            if (fruit != null)
            {
                this.hfProdID.Value = prodID.ToString();

                HtmlGenericControl liFruitImg;
                fruit.FruitImgList.ForEach(fi =>
                {
                    if (!fi.DetailImg)
                    {
                        liFruitImg = new HtmlGenericControl("li");
                        liFruitImg.InnerHtml = string.Format("<img src=\"images/{0}\" />", fi.ImgName);
                        this.ulFlexSlider.Controls.Add(liFruitImg);
                    }
                });

                this.lblProdName.Text = fruit.FruitName;
                if (fruit.IsSticky)
                {
                    this.lblStickyProd.Text = "<i class=\"fa fa-thumbs-up fa-lg\"></i>掌柜推荐";
                }
                else
                {
                    this.lblStickyProd.Visible = false;
                }

                if (fruit.ID == Fruit.FindTopSelling(DateTime.Now))
                {
                    this.lblTopSelling.Text = "<i class=\"fa fa-trophy fa-lg\"></i>本月爆款";
                }
                else
                {
                    this.lblTopSelling.Visible = false;
                }

                this.lblProdDesc.Text = fruit.FruitDesc;
                this.lblProdPrice.Text = fruit.FruitPrice.ToString();
                this.lblProdUnit.Text = "元/" + fruit.FruitUnit;
                this.lblOriginalPrice.Text = "原价格：" + (fruit.FruitPrice * (decimal)1.5).ToString("F02");
                this.lblSalesVolume.Text = "累计销量：" + Fruit.SalesVolume(prodID).ToString();

                if (fruit.InventoryQty == 0)
                {
                    this.btnAddCart.Disabled = true;
                    this.btnBuynow.Disabled = true;
                    this.lblProdState.Text = "商品已售罄，我们正在补货ing...";
                }

                HtmlImage hiDetailImg;
                fruit.FruitImgList.ForEach(fi =>
                {
                    if (fi.DetailImg)
                    {
                        hiDetailImg = new HtmlImage();
                        hiDetailImg.Src = "~/images/" + fi.ImgName;
                        hiDetailImg.Alt = fi.ImgDesc;
                        hiDetailImg.Attributes["class"] = "img-responsive";
                        this.divDetailImg.Controls.Add(hiDetailImg);
                    }
                });

                //搜狐畅言所需的页面文章ID
                //参考：http://changyan.kuaizhan.com/help/f-source-id.html
                this.SOHUCS.Attributes["sid"] = Request.Url.GetHashCode().ToString();
            }
            else
            {
                Response.Write("<script>alert('商品已下架：（');history.back();</script>");
            }
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
        }
    }
}