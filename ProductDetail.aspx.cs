using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Newtonsoft.Json;

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
                fruit.FruitImgList.ForEach(fi =>
                {
                    if (fi.MainImg)
                    {
                        this.imgMainImg.ImageUrl = "~/images/" + fi.ImgName;
                        this.imgMainImg.AlternateText = fi.ImgDesc;
                        this.imgMainImg.CssClass = "img-responsive";
                    }
                    else
                    {
                        //this.divSlides.InnerHtml += string.Format("<div><img u=\"image\" src=\"images/{0}\" alt=\"{1}:{2}\" /></div>", fi.ImgName, fruit.FruitName, fruit.FruitDesc);
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
                this.lblSalesVolume.Text = "累计销量：" + Fruit.SalesVolume(prodID).ToString();

                if (fruit.InventoryQty == 0)
                {
                    this.btnAddToCart.Disabled = true;
                    this.btnLaunchGroupEvent.Disabled = true;
                    this.lblProdState.Text = "商品已售罄，我们正在补货ing...";
                }

                if(fruit.ActiveGroupPurchase == null)
                {
                    this.btnLaunchGroupEvent.Visible = false;
                    this.btnAddToCart.InnerHtml = "<i class='fa fa-cart-plus fa-lg fa-fw'></i>&nbsp;加入购物车";
                }
                else
                {
                    this.btnLaunchGroupEvent.Visible = true;
                    this.btnLaunchGroupEvent.InnerHtml = string.Format("<i class='fa fa-group fa-lg fa-fw'></i>&nbsp;团购价：{0}元/{1} {2}人团", fruit.ActiveGroupPurchase.GroupPrice, fruit.FruitUnit, fruit.ActiveGroupPurchase.RequiredNumber);
                    this.btnAddToCart.InnerHtml = "<i class='fa fa-cart-plus fa-lg fa-fw'></i>&nbsp;单独购买";
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

                //生成商品信息JS对象，用于前端JS操作
                JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                {
                    JsonSerializerSettings jSetting = new JsonSerializerSettings();
                    jSetting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                    jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                    jSetting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    return jSetting;
                });
                string jProd = JsonConvert.SerializeObject(fruit);
                ScriptManager.RegisterStartupScript(Page, this.GetType(), "jProdInfo", string.Format("var prod={0};", jProd), true);

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
            throw ex;
        }
    }
}