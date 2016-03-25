<%@ WebHandler Language="C#" Class="ProdListPager" %>

using System;
using System.Web;
using System.Collections.Generic;
using LitJson;

public class ProdListPager : IHttpHandler {

    public void ProcessRequest (HttpContext context) {

        string strWhere, strOrder;
        int pageIndex, pageSize, totalRows;
        List<Fruit> prodListPerPage;
        JsonData jProdListPerPage;

        try
        {
            if (context.Request.QueryString["PageIndex"] == null)
            {
                throw new Exception("缺少分页参数PageIndex");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(context.Request.QueryString["PageIndex"]);
                pageIndex = int.Parse(context.Request.QueryString["PageIndex"]);
            }

            if (context.Request.QueryString["PageSize"] == null)
            {
                throw new Exception("缺少分页参数PageSize");
            }
            else
            {
                UtilityHelper.AntiSQLInjection(context.Request.QueryString["PageSize"]);
                pageSize = int.Parse(context.Request.QueryString["PageSize"]);
            }

            //选择条件：上架的商品
            strWhere = "ProductOnSale=1";

            //选择条件：商品类别
            if (!string.IsNullOrEmpty(context.Request.QueryString["CategoryID"]))
            {
                UtilityHelper.AntiSQLInjection(context.Request.QueryString["CategoryID"]);

                strWhere += string.Format(" and CategoryID={0}", context.Request.QueryString["CategoryID"]);
            }

            //选择条件：商品名称模糊查询
            if (!string.IsNullOrEmpty(context.Request.QueryString["ProdName"]))
            {
                UtilityHelper.AntiSQLInjection(context.Request.QueryString["ProdName"]);

                strWhere += string.Format(" and ProductName like '%{0}%'", context.Request.QueryString["ProdName"].Trim());
            }

            //排序条件：是否置顶、优先级、商品ID
            strOrder = "IsSticky desc, Priority desc";

            int startRowIndex, maximumRows;
            maximumRows = pageSize;
            startRowIndex = (pageIndex - 1) * maximumRows;

            //查询分页数据
            prodListPerPage = Fruit.FindFruitPager(strWhere, strOrder, out totalRows, startRowIndex, maximumRows);

            //把List<>对象集合转换成JSON数据格式
            jProdListPerPage = JsonMapper.ToObject(JsonMapper.ToJson(prodListPerPage));

            //查询当月爆款商品
            int fruitTopSellingOnMonth = Fruit.FindTopSelling(DateTime.Now);

            //在JsonData中标记当月、当年销量最高的商品
            for (int i = 0; i < jProdListPerPage.Count; i++)
            {
                if (jProdListPerPage[i]["ID"].ToString() == fruitTopSellingOnMonth.ToString())
                {
                    jProdListPerPage[i]["TopSellingOnMonth"] = "1";
                    break;
                }
            }

            //在JsonData中写入本次查询的全部记录数，用于前端分页
            JsonData jdTotalRows = new JsonData();
            jdTotalRows["TotalRows"] = totalRows;
            jProdListPerPage.Add(jdTotalRows);

            //向客户端返回JSON格式的查询结果
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(jProdListPerPage.ToJson());
            context.Response.End();
        }
        catch (System.Threading.ThreadAbortException)
        { }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
        }

    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}