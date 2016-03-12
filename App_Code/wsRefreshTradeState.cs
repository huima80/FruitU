using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

/// <summary>
/// 定期刷新数据库中订单的微信支付状态，建议通过数据库计划任务定期调用
/// </summary>
[WebService(Namespace = "http://mahui.me/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
// [System.Web.Script.Services.ScriptService]
public class wsRefreshTradeState : System.Web.Services.WebService
{

    public wsRefreshTradeState()
    {

        //如果使用设计的组件，请取消注释以下行 
        //InitializeComponent(); 
    }

    [WebMethod]
    public int RefreshTradeState(DateTime startOrderDate, DateTime endOrderDate)
    {
        List<ProductOrder> poList;
        WeChatPayData queryOrderPayData;
        int handledRows = 0;

        try
        {
            poList = ProductOrder.FindAllOrders(startOrderDate, endOrderDate);

            for (int i = 0; i < poList.Count; i++)
            {
                //针对数据库中每个订单查询微信支付状态
                queryOrderPayData = WxPayAPI.OrderQueryByOutTradeNo(poList[i].OrderID);

                //校验返回状态码，如果返回FAIL，可能通信错误
                if (queryOrderPayData.GetValue("return_code").ToString() == "SUCCESS")
                {
                    //当return_code为SUCCESS，才有sign参数返回
                    if (!queryOrderPayData.CheckSign())
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                //校验业务结果
                if (queryOrderPayData.GetValue("result_code").ToString() == "SUCCESS")
                {

                    Log.Info(this.GetType().ToString(), "向微信查询订单" + poList[i].OrderID + "的实际支付状态：" + queryOrderPayData.GetValue("trade_state").ToString().ToUpper());

                    //根据trade_state判断微信支付订单的详细状态，并设置数据库中订单支付状态
                    switch (queryOrderPayData.GetValue("trade_state").ToString().ToUpper())
                    {
                        case "SUCCESS": //支付成功
                            ProductOrder.UpdateTradeState(poList[i].OrderID,
                                    TradeState.SUCCESS,
                                    null,
                                    queryOrderPayData.GetValue("transaction_id").ToString(),
                                    DateTime.ParseExact(queryOrderPayData.GetValue("time_end").ToString(),
                                        "yyyyMMddHHmmss",
                                        System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case "REFUND":  //转入退款
                            ProductOrder.UpdateTradeState(poList[i].OrderID,
                                    TradeState.REFUND,
                                    queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                    queryOrderPayData.GetValue("transaction_id").ToString(),
                                    DateTime.ParseExact(queryOrderPayData.GetValue("time_end").ToString(),
                                        "yyyyMMddHHmmss",
                                        System.Globalization.CultureInfo.InvariantCulture));
                            break;
                        case "NOTPAY":  //未支付状态，就没有微信支付交易ID和时间
                            ProductOrder.UpdateTradeState(poList[i].OrderID,
                                    TradeState.NOTPAY,
                                    queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                    null,
                                    null);
                            break;
                        case "CLOSED":  //已关闭
                            ProductOrder.UpdateTradeState(poList[i].OrderID,
                                    TradeState.CLOSED,
                                    queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                    null,
                                    null);
                            break;
                        case "REVOKED": //已撤销（刷卡支付）
                            ProductOrder.UpdateTradeState(poList[i].OrderID,
                                    TradeState.REVOKED,
                                    queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                    null,
                                    null);
                            break;
                        case "USERPAYING":  //用户支付中
                            ProductOrder.UpdateTradeState(poList[i].OrderID,
                                    TradeState.USERPAYING,
                                    queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                    null,
                                    null);
                            break;
                        case "PAYERROR":    //支付失败(其他原因，如银行返回失败)
                            ProductOrder.UpdateTradeState(poList[i].OrderID,
                                    TradeState.PAYERROR,
                                    queryOrderPayData.GetValue("trade_state_desc").ToString(),
                                    null,
                                    null);
                            break;
                        default:    //未知状态
                            break;
                    }
                }
                else  //result_code返回FAIL，可能微信支付中此订单号不存在、微信支付系统异常
                {
                    ProductOrder.UpdateTradeState(poList[i].OrderID,
                            TradeState.NOTPAY,
                            queryOrderPayData.GetValue("err_code_des").ToString(),
                            null,
                            null);
                }

                handledRows++;

            }

        }
        catch(Exception ex)
        {
            Log.Error(this.GetType().ToString(), "RefreshTradeState : " + ex.Message);
        }
        return handledRows;

    }

}
