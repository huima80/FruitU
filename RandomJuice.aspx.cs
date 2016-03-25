using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class RandomJuice : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        List<Fruit> juiceList;
        FruitImg detailImg;
        string jsJuiceList;
        List<string> juiceStrList = new List<string>();

        juiceList = Fruit.FindFruitByCategoryID(1);
        juiceList.ForEach(prod =>
        {
            detailImg = prod.FruitImgList.Find(img =>
            {
                if(img.DetailImg)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
            if(detailImg != default(FruitImg))
            {
                juiceStrList.Add(string.Format("{{prodID:{0},prodName:\"{1}\",prodDesc:\"{2}\",prodImg:\"{3}\",price:{4}}}", prod.ID, prod.FruitName, prod.FruitDesc, detailImg.ImgName, prod.FruitPrice));
            }
        });
        if (juiceStrList.Count != 0)
        {
            //生成客户端JS对象数组
            jsJuiceList = string.Join<string>(",", juiceStrList);
            jsJuiceList = string.Format("var juiceList=[{0}];", jsJuiceList);
            ScriptManager.RegisterClientScriptBlock(Page, this.GetType(), "jsJuiceList", jsJuiceList, true);
        }

    }
}