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

                //预处理订单数据，便于前端显示
                for (int i = 0; i < jOrderListPerPage.Count; i++)
                {
                    //显示订单ID的后18位，代表时分秒，和3位随机数
                    jOrderListPerPage[i]["OrderID"] = jOrderListPerPage[i]["OrderID"].ToString().Substring(18);
                    
                    //显示下单时间、配送时间、签收时间的中文日期
                    if(jOrderListPerPage[i]["OrderDate"] != null)
                    {
                        jOrderListPerPage[i]["OrderDate"] = DateTime.Parse(jOrderListPerPage[i]["OrderDate"].ToString()).ToLongDateString();
                    }

                    if(jOrderListPerPage[i]["DeliverDate"] != null)
                    {
                        jOrderListPerPage[i]["DeliverDate"] = DateTime.Parse(jOrderListPerPage[i]["DeliverDate"].ToString()).ToLongDateString();
                    }

                    if(jOrderListPerPage[i]["AcceptDate"] != null)
                    {
                        jOrderListPerPage[i]["AcceptDate"] = DateTime.Parse(jOrderListPerPage[i]["AcceptDate"].ToString()).ToLongDateString();
                    }

                    //显示订单的微信支付状态
                    switch ((TradeState)int.Parse(jOrderListPerPage[i]["TradeState"].ToString()))
                    {
                        case TradeState.SUCCESS:
                            jOrderListPerPage[i]["TradeStateText"] = "支付成功";
                            break;
                        case TradeState.REFUND:
                            jOrderListPerPage[i]["TradeStateText"] = "转入退款";
                            break;
                        case TradeState.NOTPAY:
                            jOrderListPerPage[i]["TradeStateText"] = "未支付";
                            break;
                        case TradeState.CLOSED:
                            jOrderListPerPage[i]["TradeStateText"] = "已关闭";
                            break;
                        case TradeState.REVOKED:
                            jOrderListPerPage[i]["TradeStateText"] = "已撤销（刷卡支付）";
                            break;
                        case TradeState.USERPAYING:
                            jOrderListPerPage[i]["TradeStateText"] = "用户支付中";
                            break;
                        case TradeState.PAYERROR:
                            jOrderListPerPage[i]["TradeStateText"] = "支付失败";
                            break;
                        default:
                            jOrderListPerPage[i]["TradeStateText"] = "未知状态";
                            break;
                    }

                    //校验订单中每个订单项对应的商品项是否下架
                    for (int j = 0; j < jOrderListPerPage[i]["OrderDetailList"].Count; j++)
                    {
                        if (string.IsNullOrEmpty(jOrderListPerPage[i]["OrderDetailList"][j]["FruitName"].ToString()))
                        {
                            jOrderListPerPage[i]["OrderDetailList"][j]["FruitName"] = "此商品已下架";
                            JsonData jFruitImg = new JsonData();
                            jFruitImg["ImgName"] = Config.DefaultImg;
                            jOrderListPerPage[i]["OrderDetailList"][j]["FruitImgList"].Add(jFruitImg);
                        }
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