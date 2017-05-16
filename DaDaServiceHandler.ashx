<%@ WebHandler Language="C#" Class="DaDaServiceHandler" %>

using System;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 达达订单处理控制器
/// </summary>
public class DaDaServiceHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
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

                ProductOrder po;
                DaDaOrder dadaOrder;

                switch (Action)
                {
                    case "AddOrder":
                    case "ReAddOrder":
                    case "QueryDeliverFee":
                        //根据OrderID加载业务订单
                        po = new ProductOrder(OrderID);
                        if (po == null)
                        {
                            throw new Exception("没有找到OrderID对应的业务订单");
                        }

                        decimal tips = 0;
                        DateTime expectedFetchTime;
                        DateTime? expectedFinishTime = null;
                        double receiverLat = 0, receiverLng = 0;

                        if (!decimal.TryParse(context.Request.Form["tips"], out tips))
                        {
                            tips = 0;
                        }
                        if (DateTime.TryParse(context.Request.Form["expected_fetch_time"], out expectedFetchTime))
                        {
                            if (expectedFetchTime <= DateTime.Now)
                            {
                                expectedFetchTime = expectedFetchTime.AddDays(1);
                            }
                        }
                        else
                        {
                            expectedFetchTime = expectedFetchTime.AddDays(1);
                        }
                        if (context.Request.Form["expected_finish_time"] != null)
                        {
                            DateTime dtExpectedFinishTime;
                            if (DateTime.TryParse(context.Request.Form["expected_finish_time"], out dtExpectedFinishTime))
                            {
                                if (dtExpectedFinishTime <= DateTime.Now)
                                {
                                    expectedFinishTime = DateTime.Now.AddHours(2);
                                }
                                else
                                {
                                    expectedFinishTime = dtExpectedFinishTime;
                                }
                            }
                        }
                        if (context.Request.Form["receiver_lat"] != null)
                        {
                            if (!double.TryParse(context.Request.Form["receiver_lat"], out receiverLat))
                            {
                                receiverLat = 0;
                            }
                        }
                        if (context.Request.Form["receiver_lng"] != null)
                        {
                            if (!double.TryParse(context.Request.Form["receiver_lng"], out receiverLng))
                            {
                                receiverLng = 0;
                            }
                        }
                        dadaOrder = new DaDaOrder();
                        dadaOrder.ProductOrder = po;
                        dadaOrder.ShopNo = context.Request.Form["shop_no"];
                        dadaOrder.CityCode = context.Request.Form["city_code"];
                        dadaOrder.Tips = tips;
                        dadaOrder.CreateTime = DateTime.Now;
                        dadaOrder.Info = context.Request.Form["info"];
                        dadaOrder.CargoType = 2;
                        dadaOrder.CargoPrice = po.OrderPrice;
                        dadaOrder.CargoNum = po.OrderDetailCount;
                        dadaOrder.IsPrepay = context.Request.Form["is_prepay"] == "true" ? true : false;
                        dadaOrder.ExpectedFetchTime = expectedFetchTime;
                        dadaOrder.ExpectedFinishTime = expectedFinishTime;
                        dadaOrder.InvoiceTitle = Config.SiteTitle;
                        dadaOrder.ReceiverName = po.DeliverName;
                        dadaOrder.ReceiverAddress = po.DeliverAddress;
                        dadaOrder.ReceiverPhone = po.DeliverPhone;
                        dadaOrder.ReceiverLat = receiverLat;
                        dadaOrder.ReceiverLng = receiverLng;

                        //向达达发送新增订单请求
                        switch (Action)
                        {
                            case "AddOrder":
                                jRet = DaDaBiz.AddOrder(dadaOrder, 1);
                                break;
                            case "ReAddOrder":
                                jRet = DaDaBiz.AddOrder(dadaOrder, 2);
                                break;
                            case "QueryDeliverFee":
                                jRet = DaDaBiz.AddOrder(dadaOrder, 3);
                                break;
                        }

                        if (jRet["code"] != null)
                        {
                            if (jRet["code"].ToString() == "0")
                            {
                                switch (Action)
                                {
                                    case "AddOrder":
                                    case "ReAddOrder":
                                        //发单成功，待接单状态
                                        dadaOrder.OrderStatus = DaDaOrderStatus.Accepting;
                                        break;
                                    case "QueryDeliverFee":
                                        //查询订单运费成功，预发布状态
                                        dadaOrder.OrderStatus = DaDaOrderStatus.PreRelease;
                                        break;
                                }
                                //在本地数据库插入或更新达达订单
                                DaDaOrder.AddDaDaOrder(dadaOrder);
                            }
                        }
                        else
                        {
                            throw new Exception("达达新增订单接口没有返回值");
                        }

                        break;
                    case "AddAfterQuery":
                        if (!string.IsNullOrEmpty(context.Request.Form["deliveryNo"]))
                        {
                            //调用达达查询后发单接口
                            jRet = DaDaBiz.AddAfterQuery(context.Request.Form["deliveryNo"]);
                            if (jRet["code"] != null)
                            {
                                //达达查询运费后订单发布成功
                                if (jRet["code"].ToString() == "0")
                                {
                                    //设置预发布订单为待接单状态
                                    DaDaOrder.ReleaseOrder(context.Request.Form["deliveryNo"]);
                                }
                            }
                            else
                            {
                                throw new Exception("达达查询运费后发布订单接口没有返回值");
                            }
                        }
                        else
                        {
                            throw new Exception("达达平台订单号deliveryNo错误");
                        }
                        break;
                    case "AddTip":
                        decimal addTips;
                        if (decimal.TryParse(context.Request.Form["addTips"], out addTips))
                        {
                            string cityCode = context.Request.Form["city_code"];
                            string info = context.Request.Form["info"];
                            //调用达达增加小费接口
                            jRet = DaDaBiz.AddTip(OrderID, addTips, cityCode, info);
                            if (jRet["code"] != null)
                            {
                                //达达增加小费成功
                                if (jRet["code"].ToString() == "0")
                                {
                                    //增加本地达达小费
                                    DaDaOrder.AddTips(OrderID, addTips);
                                    //返回新加的小费
                                    jRet.Add("addTips", new JValue(addTips));
                                }
                            }
                            else
                            {
                                throw new Exception("达达增加小费接口没有返回值");
                            }
                        }
                        else
                        {
                            throw new Exception("小费金额错误");
                        }
                        break;
                    case "CancelOrder":
                        int cancelOrderID;
                        if (!string.IsNullOrEmpty(context.Request.Form["cancel_reason_id"]))
                        {
                            if (int.TryParse(context.Request.Form["cancel_reason_id"], out cancelOrderID))
                            {
                                string cancelReason = context.Request.Form["cancel_reason"];
                                //调用达达取消订单接口
                                jRet = DaDaBiz.CancelOrder(OrderID, cancelOrderID, cancelReason);
                                if (jRet["code"] != null)
                                {
                                    //达达取消订单成功
                                    if (jRet["code"].ToString() == "0")
                                    {
                                        decimal deductFee = 0;
                                        if (jRet["result"]["deduct_fee"] != null && !decimal.TryParse(jRet["result"]["deduct_fee"].ToString(), out deductFee))
                                        {
                                            deductFee = 0;
                                        }
                                        dadaOrder = new DaDaOrder();
                                        dadaOrder.ProductOrder = new ProductOrder()
                                        {
                                            OrderID = OrderID
                                        };
                                        dadaOrder.CancelReason = cancelReason;
                                        dadaOrder.DeductFee = deductFee;
                                        //取消本地达达订单
                                        DaDaOrder.CancelOrder(dadaOrder);
                                    }
                                }
                                else
                                {
                                    throw new Exception("达达取消订单接口没有返回值");
                                }
                            }
                            else
                            {
                                throw new Exception("订单取消原因ID错误");
                            }
                        }

                        break;
                    case "GetCancelReason":
                        //调用达达获取取消原因接口
                        jRet = DaDaBiz.GetCancelReason();
                        if (jRet["code"] == null)
                        {
                            throw new Exception("获取达达取消订单原因接口没有返回值");
                        }
                        break;
                    case "GetCityCode":
                        //调用达达获取城市编码接口
                        jRet = DaDaBiz.GetCityCode();
                        if (jRet["code"] == null)
                        {
                            throw new Exception("获取达达城市编码接口没有返回值");
                        }
                        break;
                    case "GetWxPOIList":
                        //获取微信门店列表
                        jRet = JObject.Parse(WxJSAPI.GetPOIList());
                        break;
                    case "QueryOrder":
                        //前台定时调用此Action，实时获取订单详情(本地库)、订单状态(达达接口)、骑手坐标(达达接口)
                        //查询本地的达达订单，如果不存在，则是尚未发单
                        dadaOrder = DaDaOrder.FindDaDaOrderByOrderID(OrderID);
                        if (dadaOrder == null)
                        {
                            dadaOrder = new DaDaOrder();
                            //加载对应的业务订单
                            dadaOrder.ProductOrder = new ProductOrder(OrderID);
                        }
                        //调用查询达达订单接口，得到最新的订单状态、骑手信息（姓名、坐标），并更新dadaOrder对象
                        jRet = DaDaBiz.QueryOrder(dadaOrder);
                        if (jRet["code"] != null)
                        {
                            //把达达订单加入到返回值
                            jRet.Add("dadaOrder", JToken.FromObject(dadaOrder, new JsonSerializer
                            {
                                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                            }));
                        }
                        else
                        {
                            throw new Exception("达达订单查询接口没有返回值");
                        }

                        break;
                    case "GetComplaintReason":
                        //获取投诉达达的原因列表
                        jRet = DaDaBiz.GetComplaintReason();
                        if (jRet["code"] == null)
                        {
                            throw new Exception("获取投诉达达原因接口没有返回值");
                        }
                        break;
                    case "ComplaintDaDa":
                        //投诉达达
                        int complaintReasonID;
                        if (!string.IsNullOrEmpty(context.Request.Form["ComplaintReasonID"]))
                        {
                            if (int.TryParse(context.Request.Form["ComplaintReasonID"], out complaintReasonID))
                            {
                                jRet = DaDaBiz.ComplaintDaDa(OrderID, complaintReasonID);
                                if (jRet["code"] == null)
                                {
                                    throw new Exception("投诉达达接口没有返回值");
                                }
                            }
                            else
                            {
                                throw new Exception("投诉达达原因ID错误");
                            }
                        }
                        else
                        {
                            throw new Exception("投诉达达原因ID错误");
                        }
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
            context.Response.Write(jRet.ToString());
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