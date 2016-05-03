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
        List<Fruit> juiceList, freshJuiceList = new List<Fruit>(), allFruitJuiceList = new List<Fruit>(), fruitVeggiesJuiceList = new List<Fruit>();
        juiceList = Fruit.FindFruitByCategoryID("41,42,43", out topSellingIDWeekly, out topSellingIDMonthly);
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
            }
        });

        this.divFreshJuiceList.InnerHtml = RenderTmpl(freshJuiceList, topSellingIDWeekly);
        this.divAllFruitJuiceList.InnerHtml = RenderTmpl(allFruitJuiceList, topSellingIDWeekly);
        this.divFruitVeggiesJuiceList.InnerHtml = RenderTmpl(fruitVeggiesJuiceList, topSellingIDWeekly);

        JsonData jJuiceList = JsonMapper.ToJson(juiceList);
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsJuiceList", string.Format("var juiceList={0};", jJuiceList.ToString()), true);
    }

    private string RenderTmpl(List<Fruit> data, int topSellingIDWeekly)
    {
        StringBuilder sbJuiceList = new StringBuilder();
        string tmplJuice = "<div class=\"col-xs-6\" onclick=\"openModal({{:ID}});\"><img src=\"images/{{:ImgName}}\" alt=\"{{:ImgDesc}}\">{{本周爆款}}{{已售罄}}</div>";
        string resultStr;

        data.Sort();
        data.ForEach(juice =>
        {
            resultStr = string.Copy(tmplJuice);

            if (juice.InventoryQty != 0)
            {
                resultStr = resultStr.Replace("{{:ID}}", juice.ID.ToString());
            }
            else
            {
                resultStr = resultStr.Replace("onclick=\"openModal({{:ID}});\"", string.Empty);
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
                resultStr = resultStr.Replace("{{本周爆款}}", "<span class=\"top-selling-week-prod\"><i class=\"fa fa-trophy fa-lg\"></i>本周爆款</span>");
            }
            else
            {
                resultStr = resultStr.Replace("{{本周爆款}}", string.Empty);
            }
            if (juice.InventoryQty == 0)
            {
                resultStr = resultStr.Replace("{{已售罄}}", "<span class=\"sell-out\">已售罄</span>");
            }
            else
            {
                resultStr = resultStr.Replace("{{已售罄}}", string.Empty);
            }

            sbJuiceList.Append(resultStr);
        });

        return sbJuiceList.ToString();
    }
}