<%@ WebHandler Language="C#" Class="AcceptOrder" %>

using System;
using System.Web;
using LitJson;

public class AcceptOrder : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
    public void ProcessRequest(HttpContext context)
    {
        //微信统一下单API返回的错误码
        JsonData jStateCode = new JsonData();
        int poID = 0;

        try
        {
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            if (wxUser != null && !string.IsNullOrEmpty(context.Request.QueryString["PoID"]))
            {
                if (int.TryParse(context.Request.QueryString["PoID"], out poID))
                {
                    //根据POID加载订单信息，并注册订单变动事件处理函数
                    ProductOrder po = new ProductOrder(poID);

                    if (wxUser.OpenID != po.Purchaser.OpenID)
                    {
                        throw new Exception("您只能签收自己的订单");
                    }

                    //校验订单状态是否允许签收
                    if (po.IsCancel)
                    {
                        throw new Exception("此订单已经撤单，签收失败");
                    }

                    po.IsAccept = true;
                    po.AcceptDate = DateTime.Now;
                    po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);
                    if (ProductOrder.AcceptOrder(po) == 1)
                    {
                        jStateCode["result_code"] = "SUCCESS";
                        jStateCode["po_id"] = poID;
                    }
                    else
                    {
                        throw new Exception("签收操作失败");
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
            context.Response.ContentType = "text/plain";
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