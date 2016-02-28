<%@ WebHandler Language="C#" Class="OrderListPager" %>

using System;
using System.Web;
using System.Collections.Generic;
using LitJson;

public class OrderListPager : IHttpHandler, System.Web.SessionState.IRequiresSessionState {

    public void ProcessRequest (HttpContext context) {

        string openID = string.Empty;
        int pageIndex, pageSize, totalRows;
        List<ProductOrder> orderListPerPage;
        JsonData jOrderListPerPage, jUserInfo;

        try
        {
            if (context.Session["UserInfo"] != null && context.Session["UserInfo"] is JsonData)
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

                jUserInfo = context.Session["UserInfo"] as JsonData;

                int startRowIndex, maximumRows;
                maximumRows = pageSize;
                startRowIndex = (pageIndex - 1) * maximumRows;

                //查询分页数据
                orderListPerPage = ProductOrder.FindProductOrderPager(string.Format("OpenID='{0}'", jUserInfo["openid"]), string.Empty, out totalRows, startRowIndex, maximumRows);

                //把List<>对象集合转换成JSON数据格式，返回给前端展示
                jOrderListPerPage = JsonMapper.ToObject(JsonMapper.ToJson(orderListPerPage));

                JsonData jdTotalRows = new JsonData();
                jdTotalRows["TotalRows"] = totalRows;
                jOrderListPerPage.Add(jdTotalRows);

                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.Write(jOrderListPerPage.ToJson());
                context.Response.End();
            }
        }
        catch (System.Threading.ThreadAbortException)
        { }
        catch(Exception ex)
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