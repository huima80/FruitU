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
            //获取数据库中指定时间范围的订单
            poList = ProductOrder.FindAllOrders(startOrderDate, endOrderDate);

            for (int i = 0; i < poList.Count; i++)
            {
                try
                {
                    //针对数据库中每个订单查询微信支付状态
                    queryOrderPayData = WxPayAPI.OrderQueryByOutTradeNo(poList[i].OrderID);

                    if (queryOrderPayData != null)
                    {
                        Log.Info(this.GetType().ToString(), "向微信查询订单实际支付状态：" + queryOrderPayData.ToXml());

                        //校验微信支付返回的OpenID与原订单OpenID是否相符
                        if (queryOrderPayData.IsSet("openid"))
                        {
                            if (poList[i].OpenID != queryOrderPayData.GetValue("openid").ToString())
                            {
                                throw new Exception(string.Format("微信支付返回的OpenID：{0}与原订单OpenID：{1}不符", queryOrderPayData.GetValue("openid").ToString(), poList[i].OpenID));
                            }
                        }
                        else
                        {
                            throw new Exception("微信支付没有返回OpenID");
                        }

                        //校验微信支付返回的总金额与原订单金额是否相符
                        if (queryOrderPayData.IsSet("total_fee"))
                        {
                            decimal totalFee;
                            if (decimal.TryParse(queryOrderPayData.GetValue("total_fee").ToString(), out totalFee))
                            {
                                totalFee = totalFee / 100;
                                if (poList[i].OrderPrice != totalFee)
                                {
                                    throw new Exception(string.Format("微信支付返回的订单总金额{0}元与原订单金额{1}元不符", totalFee, poList[i].OrderPrice));
                                }
                            }
                            else
                            {
                                throw new Exception("微信支付返回的订单总金额格式转换错误total_fee");
                            }
                        }
                        else
                        {
                            throw new Exception("微信支付没有返回订单总金额total_fee");
                        }

                        //获取返回的微信支付状态
                        if (queryOrderPayData.IsSet("trade_state"))
                        {
                            //根据查询返回的trade_state设置数据库中用户订单的微信支付实际状态
                            switch (queryOrderPayData.GetValue("trade_state").ToString().ToUpper())
                            {
                                case "SUCCESS": //支付成功
                                    poList[i].TradeState = TradeState.SUCCESS;
                                    break;
                                case "REFUND":  //转入退款
                                    poList[i].TradeState = TradeState.REFUND;
                                    break;
                                case "NOTPAY":  //未支付状态，就没有交易时间time_end
                                    poList[i].TradeState = TradeState.NOTPAY;
                                    break;
                                case "CLOSED":  //已关闭
                                    poList[i].TradeState = TradeState.CLOSED;
                                    break;
                                case "REVOKED": //已撤销（刷卡支付）
                                    poList[i].TradeState = TradeState.REVOKED;
                                    break;
                                case "USERPAYING":  //用户支付中
                                    poList[i].TradeState = TradeState.USERPAYING;
                                    break;
                                case "PAYERROR":    //支付失败(其他原因，如银行返回失败)
                                    poList[i].TradeState = TradeState.PAYERROR;
                                    break;
                                default:    //未知状态，默认为未支付
                                    poList[i].TradeState = TradeState.NOTPAY;
                                    break;
                            }
                        }
                        else
                        {
                            throw new Exception("微信支付没有返回支付状态trade_state");
                        }

                        //获取微信支付交易流水号、交易时间、交易描述
                        poList[i].TransactionID = queryOrderPayData.IsSet("transaction_id") ? queryOrderPayData.GetValue("transaction_id").ToString() : string.Empty;
                        poList[i].TransactionTime = queryOrderPayData.IsSet("time_end") ? (DateTime?)DateTime.ParseExact(queryOrderPayData.GetValue("time_end").ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture) : null;
                        poList[i].TradeStateDesc = queryOrderPayData.IsSet("trade_state_desc") ? queryOrderPayData.GetValue("trade_state_desc").ToString() : string.Empty;

                        //更新订单支付状态
                        ProductOrder.UpdateTradeState(poList[i]);
                    }
                }
                catch (Exception ex)
                {
                    //对每个订单的异常只记录，不退出循环
                    Log.Error(this.GetType().ToString(), "RefreshTradeState : " + ex.Message);
                }

                handledRows++;
            }

        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), "RefreshTradeState : " + ex.Message);
        }

        return handledRows;

    }

}
