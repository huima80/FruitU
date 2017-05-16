<%@ WebHandler Language="C#" Class="OrderHandler" %>

using System;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 后台订单管理的处理程序
/// </summary>
public class OrderHandler : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
        JObject jRet = new JObject();

        try
        {
            string Action = context.Request.QueryString["Action"];
            string OrderID = context.Request.Form["OrderID"];

            if (!string.IsNullOrEmpty(Action))
            {
                if (!string.IsNullOrEmpty(OrderID))
                {
                    UtilityHelper.AntiSQLInjection(OrderID);
                }

                ProductOrder po = new ProductOrder();

                switch (Action)
                {
                    case "SelfDelivery":
                        //根据订单ID加载完整订单信息
                        po.FindOrderByOrderID(OrderID);
                        po.IsDelivered = true;
                        po.DeliverDate = DateTime.Now;
                        po.DeliveryType = DeliveryType.Self;
                        //注册订单发货事件处理函数，通知用户
                        po.OrderStateChanged += new ProductOrder.OrderStateChangedEventHandler(WxTmplMsg.SendMsgOnOrderStateChanged);
                        po.OrderDetailList.ForEach(od =>
                        {
                            //注册商品库存量报警事件处理函数，通知管理员
                            od.InventoryWarn += new EventHandler(WxTmplMsg.SendMsgOnInventoryWarn);
                        });

                        jRet["code"] = 0;
                        jRet["result"] = ProductOrder.DeliverOrder(po);

                        break;
                    default:
                        throw new Exception("错误的Action");
                }
            }
            else
            {
                throw new Exception("错误的Action");
            }

        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
            jRet["code"] = -1;
            jRet["msg"] = ex.Message;
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.Write(jRet);
            context.Response.End();
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}