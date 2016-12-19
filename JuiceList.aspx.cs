using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using LitJson;

public partial class JuiceList : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int topSellingIDWeekly, topSellingIDMonthly;
        List<Fruit> juiceList, freshJuiceList = new List<Fruit>(), allFruitJuiceList = new List<Fruit>(), fruitVeggiesJuiceList = new List<Fruit>(), fruitMilkList = new List<Fruit>();
        juiceList = Fruit.FindFruitByCategoryID("41,42,43,47", out topSellingIDWeekly, out topSellingIDMonthly);
        juiceList.ForEach(juice =>
        {
            switch (juice.Category.ID)
            {
                case 41:
                    freshJuiceList.Add(juice);
                    break;
                case 42:
                    allFruitJuiceList.Add(juice);
                    break;
                case 43:
                    fruitVeggiesJuiceList.Add(juice);
                    break;
                case 47:
                    fruitMilkList.Add(juice);
                    break;
            }
        });

        this.divFreshJuiceList.InnerHtml = RenderTmpl(freshJuiceList, topSellingIDWeekly);
        this.divAllFruitJuiceList.InnerHtml = RenderTmpl(allFruitJuiceList, topSellingIDWeekly);
        this.divFruitVeggiesJuiceList.InnerHtml = RenderTmpl(fruitVeggiesJuiceList, topSellingIDWeekly);
        this.divFruitMilkList.InnerHtml = RenderTmpl(fruitMilkList, topSellingIDWeekly);

        JsonData jJuiceList = JsonMapper.ToJson(juiceList);
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsJuiceList", string.Format("var juiceList={0};", jJuiceList.ToString()), true);
    }

    private string RenderTmpl(List<Fruit> data, int topSellingIDWeekly)
    {
        StringBuilder sbJuiceList = new StringBuilder();
        string tmplJuice = "<div class=\"col-xs-6\" onclick=\"openModal({{:ID}});\"><img src=\"images/{{:ImgName}}\" alt=\"{{:ImgDesc}}\">{{本周爆款}}{{今日售罄}}{{团}}</div>";
        string resultStr;

        data.Sort();
        data.ForEach(juice =>
        {
            if (juice.InventoryQty != 0)
            {
                resultStr = tmplJuice.Replace("{{:ID}}", juice.ID.ToString()).Replace("{{今日售罄}}", string.Empty);
            }
            else
            {
                resultStr = tmplJuice.Replace("onclick=\"openModal({{:ID}});\"", string.Empty).Replace("{{今日售罄}}", "<span class=\"label label-danger sell-out\">今日售罄</span>");
            }

            if (juice.ActiveGroupPurchase != null)
            {
                resultStr = resultStr.Replace("{{团}}", "<span class=\"label label-warning group-purchase-label\"><i class=\"fa fa-group\"></i> 团购</span>");
            }
            else
            {
                resultStr = resultStr.Replace("{{团}}", string.Empty);
            }

            juice.FruitImgList.ForEach(fruitImg =>
            {
                if (fruitImg.MainImg)
                {
                    resultStr = resultStr.Replace("{{:ImgName}}", fruitImg.ImgName).Replace("{{:ImgDesc}}", fruitImg.ImgDesc);
                    return;
                }
            });

            if (juice.ID == topSellingIDWeekly)
            {
                resultStr = resultStr.Replace("{{本周爆款}}", "<span class=\"label label-danger top-selling-week-prod\"><i class=\"fa fa-trophy fa-lg\"></i>本周爆款</span>");
            }
            else
            {
                resultStr = resultStr.Replace("{{本周爆款}}", string.Empty);
            }

            sbJuiceList.Append(resultStr);
        });

        return sbJuiceList.ToString();
    }
}