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
        JsonData jOrderListPerPage;

        try
        {
            if (context.Session["WxUser"] != null)
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

                WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

                int startRowIndex, maximumRows;
                maximumRows = pageSize;
                startRowIndex = (pageIndex - 1) * maximumRows;

                //查询分页数据
                orderListPerPage = ProductOrder.FindProductOrderPager(string.Format("OpenID='{0}'", wxUser.OpenID), string.Empty, out totalRows, startRowIndex, maximumRows);

                //把List<>对象集合转换成JSON数据格式，返回给前端展示
                jOrderListPerPage = JsonMapper.ToObject(JsonMapper.ToJson(orderListPerPage));

                //预处理订单数据，便于前端显示
                for (int i = 0; i < jOrderListPerPage.Count; i++)
                {
                    if (jOrderListPerPage[i]["OrderID"] != null)
                    {
                        //显示订单ID的后18位，代表时分秒，和3位随机数
                        jOrderListPerPage[i]["OrderID"] = jOrderListPerPage[i]["OrderID"].ToString().Substring(18);
                    }

                    //格式化下单时间、支付时间、配送时间、签收时间
                    if(jOrderListPerPage[i]["OrderDate"] != null)
                    {
                        DateTime orderDate = DateTime.Parse(jOrderListPerPage[i]["OrderDate"].ToString());
                        jOrderListPerPage[i]["OrderDate"] = string.Format("{0}/{1}/{2} {3}", orderDate.Year, orderDate.Month, orderDate.Day, orderDate.ToLongTimeString());
                    }

                    if(jOrderListPerPage[i]["TransactionTime"] != null)
                    {
                        DateTime transactionTime = DateTime.Parse(jOrderListPerPage[i]["TransactionTime"].ToString());
                        jOrderListPerPage[i]["TransactionTime"] = string.Format("{0}/{1}/{2} {3}", transactionTime.Year, transactionTime.Month, transactionTime.Day, transactionTime.ToLongTimeString());
                    }

                    if (jOrderListPerPage[i]["DeliverDate"] != null)
                    {
                        DateTime deliverDate = DateTime.Parse(jOrderListPerPage[i]["DeliverDate"].ToString());
                        jOrderListPerPage[i]["DeliverDate"] = string.Format("{0}/{1}/{2} {3}", deliverDate.Year, deliverDate.Month, deliverDate.Day, deliverDate.ToLongTimeString());
                    }

                    if(jOrderListPerPage[i]["AcceptDate"] != null)
                    {
                        DateTime acceptDate = DateTime.Parse(jOrderListPerPage[i]["AcceptDate"].ToString());
                        jOrderListPerPage[i]["AcceptDate"] = string.Format("{0}/{1}/{2} {3}", acceptDate.Year, acceptDate.Month, acceptDate.Day, acceptDate.ToLongTimeString());
                    }

                    if (jOrderListPerPage[i]["TradeState"] != null)
                    {
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
                    }

                    if (jOrderListPerPage[i]["OrderDetailList"] != null && jOrderListPerPage[i]["OrderDetailList"].IsArray)
                    {
                        //校验订单中每个订单项对应的商品项是否下架
                        for (int j = 0; j < jOrderListPerPage[i]["OrderDetailList"].Count; j++)
                        {
                            if (string.IsNullOrEmpty(jOrderListPerPage[i]["OrderDetailList"][j]["FruitName"].ToString()))
                            {
                                //已下架商品的图片已被删除，所以需要显示默认图片
                                jOrderListPerPage[i]["OrderDetailList"][j]["FruitName"] = "此商品已下架";
                                JsonData jFruitImg = new JsonData();
                                jFruitImg["ImgName"] = Config.DefaultImg;
                                jOrderListPerPage[i]["OrderDetailList"][j]["FruitImgList"].Add(jFruitImg);
                            }
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