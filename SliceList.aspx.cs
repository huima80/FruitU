using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using LitJson;

public partial class SliceList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int topSellingIDWeekly, topSellingIDMonthly;
        List<Fruit> fruitList, sliceList;

        //获取水果切片数据，并渲染模板显示
        sliceList = Fruit.FindFruitByCategoryID("44", out topSellingIDWeekly, out topSellingIDMonthly);
        this.divSlices.InnerHtml = this.RenderTmpl(sliceList, topSellingIDWeekly);

        //获取水果原料数据，并使用数据绑定控件显示
        fruitList = Fruit.FindFruitByCategoryID("28", out topSellingIDWeekly, out topSellingIDMonthly);
        fruitList.Sort();
        this.dlFruits.DataSource = fruitList;
        this.dlFruits.DataBind();

        //把所有切片数据写入前端JS变量
        JsonData jSliceList = JsonMapper.ToJson(sliceList);
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsSliceList", string.Format("var sliceList={0};", jSliceList.ToString()), true);

    }

    private string RenderTmpl(List<Fruit> data, int topSellingIDWeekly)
    {
        StringBuilder sbFruitList = new StringBuilder();
        string tmplFruit = "<div class=\"col-xs-{{:ColWidth}}\" onclick=\"openModal({{:ID}});\"><img src=\"images/{{:ImgName}}\" alt=\"{{:ImgDesc}}\">{{今日售罄}}{{团}}</div>";
        string tmplSliceMenu = "<div class=\"col-xs-12\"><img src=\"images/SliceMenu.jpg\" /></div>";
        string resultStr;

        data.Sort();

        data.ForEach(fruit =>
        {
            if (fruit.ID == 97 && fruit.InventoryQty != 0)
            {
                resultStr = tmplFruit.Replace("{{:ColWidth}}", "12").Replace("{{:ID}}", fruit.ID.ToString()).Replace("{{今日售罄}}", string.Empty);
                resultStr += tmplSliceMenu;
            }
            else
            {
                if (fruit.ID == 97 && fruit.InventoryQty == 0)
                {
                    resultStr = tmplFruit.Replace("{{:ColWidth}}", "12").Replace("onclick=\"openModal({{:ID}});\"", string.Empty).Replace("{{今日售罄}}", "<span class=\"label label-danger sell-out\">今日售罄</span>");
                    resultStr += tmplSliceMenu;
                }
                else
                {
                    if (fruit.ID != 97 && fruit.InventoryQty != 0)
                    {
                        resultStr = tmplFruit.Replace("{{:ColWidth}}", "6").Replace("{{:ID}}", fruit.ID.ToString()).Replace("{{今日售罄}}", string.Empty);
                    }
                    else
                    {
                        resultStr = tmplFruit.Replace("{{:ColWidth}}", "6").Replace("onclick=\"openModal({{:ID}});\"", string.Empty).Replace("{{今日售罄}}", "<span class=\"label label-danger sell-out\">今日售罄</span>");
                    }
                }
            }

            fruit.FruitImgList.ForEach(img =>
            {
                if (img.MainImg)
                {
                    resultStr = resultStr.Replace("{{:ImgName}}", img.ImgName).Replace("{{:ImgDesc}}", img.ImgDesc);
                    return;
                }
            });

            if (fruit.ActiveGroupPurchase != null)
            {
                resultStr = resultStr.Replace("{{团}}", "<span class=\"label label-warning group-purchase-label\"><i class=\"fa fa-group\"></i> 团购</span>");
            }
            else
            {
                resultStr = resultStr.Replace("{{团}}", string.Empty);
            }

            sbFruitList.Append(resultStr);
        });

        return sbFruitList.ToString();
    }
}