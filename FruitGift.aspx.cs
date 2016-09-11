using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;

public partial class FruitGift : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int topSellingIDWeekly, topSellingIDMonthly;
        List<Fruit> fruitGiftList;

        //获取水果切片数据，并渲染模板显示
        fruitGiftList = Fruit.FindFruitByCategoryID("45", out topSellingIDWeekly, out topSellingIDMonthly);
        fruitGiftList.ForEach(fruit =>
        {
            switch (fruit.ID)
            {
                case 114:
                    if (fruit.InventoryQty != 0)
                    {
                        this.gift198.Href = "javascript:openModal(114);";
                        this.spanSellout198.Visible = false;
                    }
                    else
                    {
                        this.spanSellout198.Visible = true;
                    }
                    break;
                case 115:
                    if (fruit.InventoryQty != 0)
                    {
                        this.gift298.Href = "javascript:openModal(115);";
                        this.spanSellout298.Visible = false;
                    }
                    else
                    {
                        this.spanSellout298.Visible = true;
                    }
                    break;
            }
        });
        //把所有切片数据写入前端JS变量
        JsonData jFruitGiftList = JsonMapper.ToJson(fruitGiftList);
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsSliceList", string.Format("var fruitGiftList={0};", jFruitGiftList.ToString()), true);

    }
}