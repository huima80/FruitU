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
        JsonData jOrderListPerPage, jWxUserInfo;

        try
        {
            if (context.Session["WxUserInfo"] != null && context.Session["WxUserInfo"] is JsonData)
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

                jWxUserInfo = context.Session["WxUserInfo"] as JsonData;

                int startRowIndex, maximumRows;
                maximumRows = pageSize;
                startRowIndex = (pageIndex - 1) * maximumRows;

                //查询分页数据
                orderListPerPage = ProductOrder.FindProductOrderPager(string.Format("OpenID='{0}'", jWxUserInfo["openid"]), string.Empty, out totalRows, startRowIndex, maximumRows);

                //把List<>对象集合转换成JSON数据格式，返回给前端展示
                jOrderListPerPage = JsonMapper.ToObject(JsonMapper.ToJson(orderListPerPage));

                //处理JSON值，便于前端显示
                for (int i = 0; i < jOrderListPerPage.Count; i++)
                {
                    jOrderListPerPage[i]["OrderID"] = jOrderListPerPage[i]["OrderID"].ToString().Substring(18);

                    jOrderListPerPage[i]["OrderDate"] = jOrderListPerPage[i]["OrderDate"].ToString().ToString();

                    switch (jOrderListPerPage[i]["TradeState"].ToString())
                    {
                        case "1":
                            jOrderListPerPage[i]["TradeStateText"] = "支付成功";
                            break;
                        case "2":
                            jOrderListPerPage[i]["TradeStateText"] = "转入退款";
                            break;
                        case "3":
                            jOrderListPerPage[i]["TradeStateText"] = "未支付";
                            break;
                        case "4":
                            jOrderListPerPage[i]["TradeStateText"] = "已关闭";
                            break;
                        case "5":
                            jOrderListPerPage[i]["TradeStateText"] = "已撤销（刷卡支付）";
                            break;
                        case "6":
                            jOrderListPerPage[i]["TradeStateText"] = "用户支付中";
                            break;
                        case "7":
                            jOrderListPerPage[i]["TradeStateText"] = "支付失败";
                            break;
                    }
                }

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