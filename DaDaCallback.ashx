<%@ WebHandler Language="C#" Class="DaDaCallback" %>

using System;
using System.Web;
using System.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 每次订单状态发生变化时，会对添加订单接口中callback的URL进行回调。
/// https://newopen.imdada.cn/#/development/file/order
/// 1. 参数以application/json方式传递。若回调服务器响应失败（响应非200），会每隔1分钟重试一次，最多重试10次。
/// 2. 每次订单状态变化都会发生回调，如果出现订单状态回调顺序不一致，请根据回调参数中的时间戳进行判断。
/// 3. 在线上环境中，订单状态的变化会主动触发订单的回调；而在测试环境中，需要模拟订单的各个状态来触发回调。
/// </summary>
public class DaDaCallback : IHttpHandler
{

    public void ProcessRequest(HttpContext context)
    {
        try
        {
            JObject jRet;
            StringBuilder sbRet = new StringBuilder();
            using (System.IO.Stream s = context.Request.InputStream)
            {
                int count = 0;
                byte[] buffer = new byte[1024];
                while ((count = s.Read(buffer, 0, 1024)) > 0)
                {
                    sbRet.Append(Encoding.UTF8.GetString(buffer, 0, count));
                }
                s.Flush();
                s.Close();
                s.Dispose();
            }
            if (sbRet.Length > 0)
            {
                jRet = JObject.Parse(sbRet.ToString());
            }
            else
            {
                throw new Exception("达达回调没有返回值");
            }

            //判断是否有带返回参数
            if (jRet != null)
            {
                string signature, client_id, order_id, update_time;

                if (jRet["signature"] != null)
                {
                    signature = jRet["signature"].ToString();
                }
                else
                {
                    throw new Exception("缺少参数：signature");
                }

                if (jRet["client_id"] != null)
                {
                    client_id = jRet["client_id"].ToString();
                }
                else
                {
                    throw new Exception("缺少参数：client_id");
                }

                if (jRet["order_id"] != null)
                {
                    order_id = jRet["order_id"].ToString();
                }
                else
                {
                    throw new Exception("缺少参数：order_id");
                }

                if (jRet["update_time"] != null)
                {
                    update_time = jRet["update_time"].ToString();
                }
                else
                {
                    throw new Exception("缺少参数：update_time");
                }

                //验证达达报文签名
                if (!DaDaBiz.VerifyDaDaSign(signature, client_id, order_id, update_time))
                {
                    throw new Exception("达达返回的报文签名错误！");
                }

                //查找达达回调指定的订单
                DaDaOrder dadaOrder = DaDaOrder.FindDaDaOrderByOrderID(order_id);
                if (dadaOrder != null)
                {
                    //达达订单状态
                    if (jRet["order_status"] != null)
                    {
                        int orderStatus;
                        if (int.TryParse(jRet["order_status"].ToString(), out orderStatus))
                        {
                            dadaOrder.OrderStatus = (DaDaOrderStatus)orderStatus;
                        }
                        else
                        {
                            throw new Exception("达达返回的订单状态order_status错误");
                        }
                    }

                    //达达配送员id，接单以后会传
                    if (jRet["dm_id"] != null)
                    {
                        int dmID;
                        if (int.TryParse(jRet["dm_id"].ToString(), out dmID))
                        {
                            dadaOrder.DMID = dmID;
                        }
                        else
                        {
                            throw new Exception("达达返回的配送员dm_id错误");
                        }
                    }

                    //配送员姓名，接单以后会传
                    if (jRet["dm_name"] != null)
                    {
                        dadaOrder.DMName = jRet["dm_name"].ToString();
                    }

                    //配送员手机号，接单以后会传
                    if (jRet["dm_mobile"] != null)
                    {
                        dadaOrder.DMMobile = jRet["dm_mobile"].ToString();
                    }

                    if (jRet["update_time"] != null)
                    {
                        //更新时间戳，视同为订单状态变化时间
                        DateTime updateTime = UtilityHelper.TimestampToDateTime(jRet["update_time"].ToString());
                        switch (dadaOrder.OrderStatus)
                        {
                            case DaDaOrderStatus.Accepting:
                                //待接单状态
                                break;
                            case DaDaOrderStatus.Fetching:
                                //待取货状态
                                dadaOrder.AcceptTime = updateTime;
                                break;
                            case DaDaOrderStatus.Delivering:
                                //配送中状态
                                dadaOrder.FetchTime = updateTime;
                                break;
                            case DaDaOrderStatus.Finished:
                                //已完成状态
                                dadaOrder.FinishTime = updateTime;
                                break;
                            case DaDaOrderStatus.Cancelled:
                                //取消状态
                                dadaOrder.CancelTime = updateTime;
                                //撤单原因
                                if (jRet["cancel_reason"] != null)
                                {
                                    dadaOrder.CancelReason = jRet["cancel_reason"].ToString();
                                }
                                break;
                            default:
                                throw new Exception("达达订单状态错误");
                        }
                    }

                    //更新达达订单状态
                    DaDaOrder.UpdateOrderStatus(dadaOrder);

                    //告知达达，请求成功
                    context.Response.Clear();
                    context.Response.StatusCode = 200;
                    context.Response.Write("OK");
                }
                else
                {
                    throw new Exception(string.Format("没有找到达达指定的订单：{0}", order_id));
                }
            }
            else
            {
                throw new Exception("无达达通知返回参数");
            }
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), string.Format("达达回调出错 :{0}\n请求方IP：{1}", ex.Message + ex.StackTrace, context.Request.UserHostAddress));

            //告知达达，服务器处理错误
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = ex.Message + ex.StackTrace;
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