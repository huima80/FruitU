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
        List<Fruit> juiceList;
        FruitImg mainImg;
        string jJuiceList;
        List<string> juiceStrList = new List<string>();
        List<string> imgStrList = new List<string>();

        juiceList = Fruit.FindFruitByCategoryID(1);
        jJuiceList = JsonMapper.ToJson(juiceList);
        ScriptManager.RegisterClientScriptBlock(Page, this.GetType(), "jsJuiceList", string.Format("var juiceList={0};", jJuiceList), true);


        //juiceList.ForEach(prod =>
        //{
        //    mainImg = prod.FruitImgList.Find(img =>
        //    {
        //        imgStrList.Add(string.Format("{{ImgID:{0},ImgName:{1},MainImg:{2},DetailImg:{3},ImgDesc:{4},ImgSeq:{5}}}", img.ImgID, img.ImgName, img.MainImg, img.DetailImg, img.ImgDesc, img.ImgSeq));
        //        if (img.MainImg)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    });
        //    if(mainImg != default(FruitImg))
        //    {
        //        juiceStrList.Add(string.Format("{{prodID:{0},prodName:\"{1}\",prodDesc:\"{2}\",prodImg:\"{3}\",price:{4}}}", prod.ID, prod.FruitName, prod.FruitDesc, mainImg.ImgName, prod.FruitPrice));
        //    }
        //});
        //if (juiceStrList.Count != 0)
        //{
        //    //生成客户端JS对象数组
        //    jsJuiceList = string.Join<string>(",", juiceStrList);
        //    jsJuiceList = string.Format("var juiceList=[{0}];", jsJuiceList);
        //    ScriptManager.RegisterClientScriptBlock(Page, this.GetType(), "jsJuiceList", jsJuiceList, true);
        //}

    }
}