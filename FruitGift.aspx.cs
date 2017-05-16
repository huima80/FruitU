using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;
using System.Text;

public partial class FruitGift : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int topSellingIDWeekly, topSellingIDMonthly;
        List<Fruit> fruitGiftList;

        //获取水果礼盒类别数据，并渲染模板显示
        fruitGiftList = Fruit.FindFruitByCategoryID("45", out topSellingIDWeekly, out topSellingIDMonthly);

        //渲染模板并显示
        this.divFruitGift.InnerHtml = RenderTmpl(fruitGiftList, topSellingIDWeekly);

        //把所有水果礼盒数据写入前端JS变量
        JsonData jFruitGiftList = JsonMapper.ToJson(fruitGiftList);
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsSliceList", string.Format("var fruitGiftList={0};", jFruitGiftList.ToString()), true);

    }

    private string RenderTmpl(List<Fruit> dataList, int topSellingIDWeekly)
    {
        StringBuilder sbJuiceList = new StringBuilder();
        string tmplJuice = "<div class=\"col-xs-12\" onclick=\"openModal({{:ID}});\"><img src=\"images/{{:ImgName}}\" alt=\"{{:ImgDesc}}\">{{本周爆款}}{{今日售罄}}{{团}}</div>";
        string resultStr;

        dataList.Sort();
        dataList.ForEach(data =>
        {
            if (data.InventoryQty != 0)
            {
                resultStr = tmplJuice.Replace("{{:ID}}", data.ID.ToString()).Replace("{{今日售罄}}", string.Empty);
            }
            else
            {
                resultStr = tmplJuice.Replace("onclick=\"openModal({{:ID}});\"", string.Empty).Replace("{{今日售罄}}", "<span class=\"label label-danger sell-out\">今日售罄</span>");
            }

            if (data.ActiveGroupPurchase != null)
            {
                resultStr = resultStr.Replace("{{团}}", "<span class=\"label label-warning group-purchase-label\"><i class=\"fa fa-group\"></i> 团购</span>");
            }
            else
            {
                resultStr = resultStr.Replace("{{团}}", string.Empty);
            }

            data.FruitImgList.ForEach(fruitImg =>
            {
                if (fruitImg.MainImg)
                {
                    resultStr = resultStr.Replace("{{:ImgName}}", fruitImg.ImgName).Replace("{{:ImgDesc}}", fruitImg.ImgDesc);
                    return;
                }
            });

            if (data.ID == topSellingIDWeekly)
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