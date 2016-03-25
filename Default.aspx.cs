using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string token, jsTicket, jsSign, timestamp, nonceStr, url;

        url = Request.Url.ToString().Split('#')[0];
        token = WxJSAPI.GetAccessToken();
        jsTicket = WxJSAPI.GetJsAPITicket(token);
        jsSign = WxJSAPI.MakeJsAPISign(jsTicket, url, out nonceStr, out timestamp);

        //向前端页面注册JS变量，用于调用微信客户端JS-API
        ScriptManager.RegisterStartupScript(Page, this.GetType(), "wxJSAPI", string.Format("var wxJsApiParam={{appId:'{0}', timestamp:'{1}', nonceStr:'{2}', signature:'{3}'}}, pageSize={4};", Config.APPID, timestamp, nonceStr, jsSign, Config.ProductListPageSize), true);

        List<Category> categoryList;
        List<Fruit> fruitList;
        FruitImg mainImg;

        //从每个商品类别中选取第一个商品的图片，作为轮播图图片
        categoryList = Category.FindAllCategory();
        categoryList.ForEach(c =>
        {
            fruitList = Fruit.FindFruitByCategoryID(c.ID);
            if (fruitList != null && fruitList.Count > 0)
            {
                mainImg = fruitList[0].FruitImgList.Find(img =>
                {
                    if (img.MainImg)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                });
                if (mainImg != default(FruitImg))
                {
                    this.divSlides.InnerHtml += string.Format("<div><img u=\"image\" src=\"images/{0}\" alt=\"{1}:{2}\" /></div>", mainImg.ImgName, fruitList[0].FruitName, fruitList[0].FruitDesc);
                }
            }
        });

    }

}