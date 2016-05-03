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
        fruitList = Fruit.FindFruitByCategoryID("28", out topSellingIDWeekly, out topSellingIDMonthly);

        this.divFruitList.InnerHtml = RenderTmpl(fruitList, topSellingIDWeekly);

        JsonData jFruitList = JsonMapper.ToJson(fruitList);
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsJuiceList", string.Format("var fruitList={0};", jFruitList.ToString()), true);
    }

    private string RenderTmpl(List<Fruit> data, int topSellingIDWeekly)
    {
        StringBuilder sbFruitList = new StringBuilder();
        string tmplFruit = "<div class=\"col-xs-12\" onclick=\"openModal({{:ID}});\"><img src=\"images/{{:ImgName}}\" alt=\"{{:ImgDesc}}\">{{已售罄}}</div>";
        string tmplWeeklyMenu = "<div class=\"col-xs-12\"><img src=\"images/WeeklyMenu.gif\" /></div>";
        string resultStr;
        int inventoryOfFruitAWeek = 0;

        data.Sort();

        //查询一周水果套餐的库存量
        data.ForEach(fruit =>
        {
            if (fruit.ID == 88)
            {
                inventoryOfFruitAWeek = fruit.InventoryQty;
                return;
            }
        });

        data.ForEach(fruit =>
        {
            resultStr = string.Copy(tmplFruit);

            if (fruit.ID == 88 && fruit.InventoryQty != 0)
            {
                resultStr = resultStr.Replace("{{:ID}}", fruit.ID.ToString());
                resultStr = resultStr.Replace("{{已售罄}}", string.Empty);
                resultStr += tmplWeeklyMenu;
            }
            else
            {
                if (fruit.ID == 88 && fruit.InventoryQty == 0)
                {
                    resultStr = resultStr.Replace("onclick=\"openModal({{:ID}});\"", string.Empty);
                    resultStr = resultStr.Replace("{{已售罄}}", "<span class=\"sell-out\">已售罄</span>");
                    resultStr += tmplWeeklyMenu;
                }
                else
                {
                    if (fruit.ID != 88 && inventoryOfFruitAWeek != 0)
                    {
                        resultStr = resultStr.Replace("{{:ID}}", "88");
                        resultStr = resultStr.Replace("{{已售罄}}", string.Empty);
                    }
                    else
                    {
                        resultStr = resultStr.Replace("onclick=\"openModal({{:ID}});\"", string.Empty);
                        resultStr = resultStr.Replace("{{已售罄}}", "<span class=\"sell-out\">已售罄</span>");
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
            //if (fruit.ID == topSellingIDWeekly)
            //{
            //    resultStr = resultStr.Replace("{{本周爆款}}", "<span class=\"top-selling-week-prod\"><i class=\"fa fa-trophy fa-lg\"></i>本周爆款</span>");
            //}
            //else
            //{
            //    resultStr = resultStr.Replace("{{本周爆款}}", string.Empty);
            //}

            sbFruitList.Append(resultStr);
        });

        return sbFruitList.ToString();
    }
}