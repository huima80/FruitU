using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using LitJson;

public partial class RandomJuice : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //查询果汁类商品，并去掉未上架、零库存的商品
        int topSellingIDWeekly, topSellingIDMonthly;
        List<Fruit> juiceList;
        juiceList = Fruit.FindFruitByCategoryID("41,42,43", out topSellingIDWeekly, out topSellingIDMonthly);
        juiceList.RemoveAll(juice =>
        {
            return (!juice.OnSale || juice.InventoryQty <= 0);
        });

        JsonData jJuiceList = JsonMapper.ToJson(juiceList);
        ScriptManager.RegisterClientScriptBlock(Page, this.GetType(), "jsJuiceList", string.Format("var juiceList={0};", jJuiceList), true);

    }
}