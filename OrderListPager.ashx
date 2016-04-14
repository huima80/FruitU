<%@ WebHandler Language="C#" Class="OrderListPager" %>

using System;
using System.Web;
using System.Collections.Generic;
using LitJson;

public class OrderListPager : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{

    public void ProcessRequest(HttpContext context)
    {

        int pageIndex, pageSize;
        int totalRows, payingOrderCount, deliveringOrderCount, acceptingOrderCount;
        List<ProductOrder> orderListPerPage;
        JsonData jOrderListPerPage;

        try
        {
            if (context.Session["WxUser"] != null)
            {
                WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

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

                bool isJoinOrderDetail = false;

                string strWhere, strTableName;
                List<string> listWhere = new List<string>();

                //查询条件：只能查询本人的订单
                listWhere.Add(string.Format("OpenID='{0}'", wxUser.OpenID));

                //查询条件：订单ID
                if (!string.IsNullOrEmpty(context.Request.QueryString["OrderID"]))
                {
                    UtilityHelper.AntiSQLInjection(context.Request.QueryString["OrderID"]);
                    listWhere.Add(string.Format("OrderID like '%{0}%'", context.Request.QueryString["OrderID"].Trim()));
                }

                //查询条件：商品名称模糊查询
                if (!string.IsNullOrEmpty(context.Request.QueryString["ProdName"]))
                {
                    UtilityHelper.AntiSQLInjection(context.Request.QueryString["ProdName"]);
                    listWhere.Add(string.Format("OrderDetail.OrderProductName like '%{0}%'", context.Request.QueryString["ProdName"].Trim()));
                    isJoinOrderDetail = true;
                }

                //查询条件：订单开始日期
                if (!string.IsNullOrEmpty(context.Request.QueryString["StartOrderDate"]))
                {
                    UtilityHelper.AntiSQLInjection(context.Request.QueryString["StartOrderDate"].Trim());
                    listWhere.Add(string.Format("CONVERT(varchar(8), OrderDate, 112) >= '{0}'", context.Request.QueryString["StartOrderDate"].Trim().Replace("-", "")));
                }

                //查询条件：订单结束日期
                if (!string.IsNullOrEmpty(context.Request.QueryString["EndOrderDate"]))
                {
                    UtilityHelper.AntiSQLInjection(context.Request.QueryString["EndOrderDate"].Trim());
                    listWhere.Add(string.Format("CONVERT(varchar(8), OrderDate, 112) <= '{0}'", context.Request.QueryString["EndOrderDate"].Trim().Replace("-", "")));
                }

                strWhere = string.Join<string>(" and ", listWhere);

                //如果查询涉及到订单商品详情，则需要关联表OrderDetail
                if (isJoinOrderDetail)
                {
                    strTableName = "ProductOrder left join OrderDetail on ProductOrder.Id = OrderDetail.PoID";
                }
                else
                {
                    strTableName = "ProductOrder";
                }

                int startRowIndex, maximumRows;
                maximumRows = pageSize;
                startRowIndex = (pageIndex - 1) * maximumRows;

                //分页查询订单数据，不需要加载下单人信息
                orderListPerPage = ProductOrder.FindProductOrderPager(false, strTableName, "ProductOrder.Id", "ProductOrder.*", strWhere, null, out totalRows, out payingOrderCount, out deliveringOrderCount, out acceptingOrderCount, startRowIndex, maximumRows);

                //把List<>对象集合转换成JSON数据格式
                jOrderListPerPage = JsonMapper.ToObject(JsonMapper.ToJson(orderListPerPage));

                //预处理订单JSON数据，便于前端显示
                for (int i = 0; i < jOrderListPerPage.Count; i++)
                {
                    //显示订单ID的后18位，代表时分秒，和3位随机数
                    if (jOrderListPerPage[i]["OrderID"] != null)
                    {
                        jOrderListPerPage[i]["OrderID"] = jOrderListPerPage[i]["OrderID"].ToString().Substring(18);
                    }

                    //格式化下单时间
                    if (jOrderListPerPage[i]["OrderDate"] != null)
                    {
                        DateTime orderDate = DateTime.Parse(jOrderListPerPage[i]["OrderDate"].ToString());
                        jOrderListPerPage[i]["OrderDate"] = string.Format("{0}/{1}/{2} {3}", orderDate.Year, orderDate.Month, orderDate.Day, orderDate.ToLongTimeString());
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
                            case TradeState.CASHPAID:
                                jOrderListPerPage[i]["TradeStateText"] = "已付现金";
                                break;
                            case TradeState.CASHNOTPAID:
                                jOrderListPerPage[i]["TradeStateText"] = "未付现金";
                                break;
                            default:
                                jOrderListPerPage[i]["TradeStateText"] = "未知状态";
                                break;
                        }
                    }

                    //校验订单中每个订单项对应的商品项是否下架
                    if (jOrderListPerPage[i]["OrderDetailList"] != null && jOrderListPerPage[i]["OrderDetailList"].IsArray)
                    {
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

                //本次查询总订单数
                JsonData jTotalRows = new JsonData();
                jTotalRows["TotalRows"] = totalRows;
                jOrderListPerPage.Add(jTotalRows);

                //本次查询待支付订单数
                JsonData jPayingOrder = new JsonData();
                jPayingOrder["PayingOrderCount"] = payingOrderCount;
                jOrderListPerPage.Add(jPayingOrder);

                //本次查询待配送订单数
                JsonData jDeliveringOrder = new JsonData();
                jDeliveringOrder["DeliveringOrderCount"] = deliveringOrderCount;
                jOrderListPerPage.Add(jDeliveringOrder);

                //本次查询待签收订单数
                JsonData jAcceptingOrder = new JsonData();
                jAcceptingOrder["AcceptingOrderCount"] = acceptingOrderCount;
                jOrderListPerPage.Add(jAcceptingOrder);

                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.Write(jOrderListPerPage.ToJson());
                context.Response.End();
            }
        }
        catch (System.Threading.ThreadAbortException)
        { }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}