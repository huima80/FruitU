<%@ WebHandler Language="C#" Class="CancelOrder" %>

using System;
using System.Web;
using LitJson;

public class CancelOrder : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{

    public void ProcessRequest(HttpContext context)
    {
        //微信统一下单API返回的错误码
        JsonData jStateCode = new JsonData();

        try
        {
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            int poID = 0;

            if (wxUser != null && !string.IsNullOrEmpty(context.Request.QueryString["PoID"]))
            {
                if (int.TryParse(context.Request.QueryString["PoID"], out poID))
                {
                    //根据POID加载订单信息，并注册订单变动事件处理函数
                    ProductOrder po = new ProductOrder(poID);

                    if (wxUser.OpenID != po.Purchaser.OpenID)
                    {
                        throw new Exception("您只能撤销自己的订单");
                    }

                    //校验订单状态是否允许撤单
                    if (po.TradeState == TradeState.SUCCESS || po.TradeState == TradeState.CASHPAID || po.TradeState == TradeState.AP_TRADE_SUCCESS || po.TradeState == TradeState.AP_TRADE_FINISHED)
                    {
                        throw new Exception("此订单已经支付，撤单失败");
                    }
                    if (po.IsDelivered)
                    {
                        throw new Exception("此订单已经发货，撤单失败");
                    }
                    if (po.IsAccept)
                    {
                        throw new Exception("此订单已经签收，撤单失败");
                    }

                    po.IsCancel = true;
                    po.CancelDate = DateTime.Now;
                    po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);
                    if (ProductOrder.CancelOrder(po) == 1)
                    {
                        jStateCode["result_code"] = "SUCCESS";
                        jStateCode["po_id"] = poID;
                    }
                    else
                    {
                        throw new Exception("撤单操作失败");
                    }
                }
                else
                {
                    throw new Exception("订单ID错误");
                }
            }
            else
            {
                throw new Exception("请指定订单ID");
            }
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
            jStateCode["result_code"] = "FAIL";
            jStateCode["err_code_des"] = ex.Message;
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.Write(jStateCode.ToJson());
            context.Response.End();
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