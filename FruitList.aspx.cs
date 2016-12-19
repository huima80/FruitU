using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using LitJson;

public partial class FruitList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int topSellingIDWeekly, topSellingIDMonthly;
        List<Fruit> fruitList;
        //获取数据
        fruitList = Fruit.FindFruitByCategoryID("46", out topSellingIDWeekly, out topSellingIDMonthly);
        //渲染模板并赋给前端HTML
        this.divFruitList.InnerHtml = RenderTmpl(fruitList, topSellingIDWeekly);
        //生成JSON数据，并生成JS全局数组
        JsonData jFruitList = JsonMapper.ToJson(fruitList);
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsJuiceList", string.Format("var fruitList={0};", jFruitList.ToString()), true);
    }

    private string RenderTmpl(List<Fruit> data, int topSellingIDWeekly)
    {
        StringBuilder sbFruitList = new StringBuilder();
        string tmplFruit = "<div class=\"col-xs-12\" onclick=\"openModal({{:ID}});\"><img src=\"images/{{:ImgName}}\" alt=\"{{:ImgDesc}}\">{{今日售罄}}{{本周爆款}}{{团}}</div>";
        string resultStr = string.Empty;

        data.Sort();

        data.ForEach(fruit =>
        {
            if (fruit.InventoryQty != 0)
            {
                resultStr = tmplFruit.Replace("{{:ID}}", fruit.ID.ToString()).Replace("{{今日售罄}}", string.Empty);
            }
            else
            {
                resultStr = tmplFruit.Replace("onclick=\"openModal({{:ID}});\"", string.Empty).Replace("{{今日售罄}}", "<span class=\"label label-danger sell-out\">今日售罄</span>");
            }

            fruit.FruitImgList.ForEach(img =>
            {
                if (img.MainImg)
                {
                    resultStr = resultStr.Replace("{{:ImgName}}", img.ImgName).Replace("{{:ImgDesc}}", img.ImgDesc);
                    return;
                }
            });

            if (fruit.ID == topSellingIDWeekly)
            {
                resultStr = resultStr.Replace("{{本周爆款}}", "<span class=\"label label-danger top-selling-week-prod\"><i class=\"fa fa-trophy fa-lg\"></i>本周爆款</span>");
            }
            else
            {
                resultStr = resultStr.Replace("{{本周爆款}}", string.Empty);
            }

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