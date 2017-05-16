using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 达达业务类
/// 参考达达API接口：https://newopen.imdada.cn/#/development/file/index
/// </summary>
public static class DaDaBiz
{
    static DaDaBiz()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    /// <summary>
    /// 查询订单状态
    /// https://newopen.imdada.cn/#/development/file/statusQuery
    /// </summary>
    /// <param name="orderID">订单ID</param>
    /// <returns></returns>
    public static JObject QueryOrder(DaDaOrder dadaOrder)
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", dadaOrder.ProductOrder.OrderID);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/status/query", strReqParam);
        JObject jRsp = JObject.Parse(strRspParam);
        if (jRsp["result"] != null)
        {
            if (jRsp["result"]["statusCode"] != null)
            {
                dadaOrder.OrderStatus = (DaDaOrderStatus)int.Parse((string)jRsp["result"]["statusCode"]);
            }
            if (jRsp["result"]["transporterName"] != null)
            {
                dadaOrder.DMName = jRsp["result"]["transporterName"].ToString();
            }
            if (jRsp["result"]["transporterPhone"] != null)
            {
                dadaOrder.DMMobile = jRsp["result"]["transporterPhone"].ToString();
            }
            if (jRsp["result"]["transporterLng"] != null)
            {
                double transporterLng;
                if (double.TryParse(jRsp["result"]["transporterLng"].ToString(), out transporterLng))
                {
                    dadaOrder.TransporterLng = transporterLng;
                }
                else
                {
                    dadaOrder.TransporterLng = 0;
                }
            }
            if (jRsp["result"]["transporterLat"] != null)
            {
                double transporterLat;
                if (double.TryParse(jRsp["result"]["transporterLat"].ToString(), out transporterLat))
                {
                    dadaOrder.TransporterLat = transporterLat;
                }
                else
                {
                    dadaOrder.TransporterLat = 0;
                }
            }
            if (jRsp["result"]["acceptTime"] != null)
            {
                DateTime acceptTime;
                if (DateTime.TryParse(jRsp["result"]["acceptTime"].ToString(), out acceptTime))
                {
                    dadaOrder.AcceptTime = acceptTime;
                }
                else
                {
                    dadaOrder.AcceptTime = null;
                }
            }
            if (jRsp["result"]["fetchTime"] != null)
            {
                DateTime fetchTime;
                if (DateTime.TryParse(jRsp["result"]["fetchTime"].ToString(), out fetchTime))
                {
                    dadaOrder.FetchTime = fetchTime;
                }
                else
                {
                    dadaOrder.FetchTime = null;
                }
            }
            if (jRsp["result"]["finishTime"] != null)
            {
                DateTime finishTime;
                if (DateTime.TryParse(jRsp["result"]["finishTime"].ToString(), out finishTime))
                {
                    dadaOrder.FinishTime = finishTime;
                }
                else
                {
                    dadaOrder.FinishTime = null;
                }
            }
            if (jRsp["result"]["cancelTime"] != null)
            {
                DateTime cancelTime;
                if (DateTime.TryParse(jRsp["result"]["cancelTime"].ToString(), out cancelTime))
                {
                    dadaOrder.CancelTime = cancelTime;
                }
                else
                {
                    dadaOrder.CancelTime = null;
                }
            }
        }

        return jRsp;
    }

    /// <summary>
    /// 发布订单
    /// </summary>
    /// <param name="dadaOrder">达达订单</param>
    /// <param name="addOrderType">
    /// 订单发布方式：
    /// 1，新增订单：商户调用该接口将配送单信息发送至达达系统。https://newopen.imdada.cn/#/development/file/add
    /// 2，重新发单：在调用新增订单后，订单被取消、过期或者投递异常的情况下，调用此接口，可以在达达平台重新发布订单。https://newopen.imdada.cn/#/development/file/reAdd
    /// 3，订单预发布：只有新订单或者状态为已取消、已过期以及投递异常的情况下可以进行订单预发布。同时返回一个唯一的平台订单编号，注意：该平台订单编号有效期为3分钟。https://newopen.imdada.cn/#/development/file/readyAdd
    /// </param>
    /// <returns></returns>
    public static JObject AddOrder(DaDaOrder dadaOrder, int addOrderType)
    {
        if (dadaOrder == null)
        {
            throw new Exception("参数dadaOrder不能为null");
        }

        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("shop_no", dadaOrder.ShopNo);
        bizParam.Add("origin_id", dadaOrder.ProductOrder.OrderID);
        bizParam.Add("city_code", dadaOrder.CityCode);
        bizParam.Add("pay_for_supplier_fee", dadaOrder.PayForSupplierFee);
        bizParam.Add("fetch_from_receiver_fee", dadaOrder.FetchFromReceiverFee);
        bizParam.Add("deliver_fee", dadaOrder.DeliverFee);
        bizParam.Add("tips", dadaOrder.Tips);
        bizParam.Add("create_time", UtilityHelper.DateTimeToTimestamp(dadaOrder.CreateTime));
        bizParam.Add("info", dadaOrder.Info);
        bizParam.Add("cargo_type", dadaOrder.CargoType);
        bizParam.Add("cargo_weight", dadaOrder.CargoWeight);
        bizParam.Add("cargo_price", dadaOrder.CargoPrice);
        bizParam.Add("cargo_num", dadaOrder.CargoNum);
        bizParam.Add("is_prepay", dadaOrder.IsPrepay ? "1" : "0");
        bizParam.Add("expected_fetch_time", UtilityHelper.DateTimeToTimestamp(dadaOrder.ExpectedFetchTime));
        bizParam.Add("expected_finish_time", (dadaOrder.ExpectedFinishTime.HasValue ? UtilityHelper.DateTimeToTimestamp(dadaOrder.ExpectedFinishTime.Value) : string.Empty));
        bizParam.Add("invoice_title", dadaOrder.InvoiceTitle);
        bizParam.Add("receiver_name", dadaOrder.ReceiverName);
        bizParam.Add("receiver_address", dadaOrder.ReceiverAddress);
        bizParam.Add("receiver_phone", dadaOrder.ReceiverPhone);
        bizParam.Add("receiver_lat", dadaOrder.ReceiverLat);
        bizParam.Add("receiver_lng", dadaOrder.ReceiverLng);
        bizParam.Add("deliver_locker_code", dadaOrder.DeliverLockerCode);
        bizParam.Add("pickup_locker_code", dadaOrder.PickupLockerCode);
        bizParam.Add("callback", Config.DaDaCallback);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam;

        switch (addOrderType)
        {
            case 1:
                //新增订单
                strRspParam = SendHttpRequest("/api/order/addOrder", strReqParam);
                break;
            case 2:
                //重新发单
                strRspParam = SendHttpRequest("/api/order/reAddOrder", strReqParam);
                break;
            case 3:
                //订单预发布
                strRspParam = SendHttpRequest("/api/order/queryDeliverFee", strReqParam);
                break;
            default:
                throw new Exception("订单发布方式错误");
        }

        JObject jRsp = JObject.Parse(strRspParam);
        if (jRsp["result"] != null)
        {
            if (jRsp["result"]["distance"] != null)
            {
                float distance;
                if (float.TryParse(jRsp["result"]["distance"].ToString(), out distance))
                {
                    dadaOrder.Distance = distance;
                }
                else
                {
                    throw new Exception("达达新增订单接口返回的配送距离错误");
                }
            }

            if (jRsp["result"]["fee"] != null)
            {
                decimal fee;
                if (decimal.TryParse(jRsp["result"]["fee"].ToString(), out fee))
                {
                    dadaOrder.Fee = fee;
                }
                else
                {
                    throw new Exception("达达新增订单接口返回的运费错误");
                }
            }

            if (jRsp["result"]["deliveryNo"] != null)
            {
                dadaOrder.DeliveryNo = jRsp["result"]["deliveryNo"].ToString();
            }

        }

        return jRsp;
    }

    /// <summary>
    /// 根据【查询订单运费接口】返回的平台订单编号进行发单
    /// https://newopen.imdada.cn/#/development/file/readyAdd
    /// </summary>
    /// <param name="deliveryNo">平台订单编号</param>
    /// <returns></returns>
    public static JObject AddAfterQuery(string deliveryNo)
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("deliveryNo", deliveryNo);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/addAfterQuery", strReqParam);
        return JObject.Parse(strRspParam);
    }

    /// <summary>
    /// 获取达达城市信息
    /// https://newopen.imdada.cn/#/development/file/cityList
    /// </summary>
    /// <returns></returns>
    public static JObject GetCityCode()
    {
        string strReqParam = GetRequestParam();
        string strRspParam = SendHttpRequest("/api/cityCode/list", strReqParam);
        return JObject.Parse(strRspParam);
    }

    /// <summary>
    /// 获取达达订单取消原因
    /// https://newopen.imdada.cn/#/development/file/reasonList
    /// </summary>
    /// <returns></returns>
    public static JObject GetCancelReason()
    {
        string strReqParam = GetRequestParam();
        string strRspParam = SendHttpRequest("/api/order/cancel/reasons", strReqParam);
        return JObject.Parse(strRspParam);
    }

    /// <summary>
    /// 取消达达订单
    /// https://newopen.imdada.cn/#/development/file/formalCancel
    /// </summary>
    /// <param name="orderID">订单编号</param>
    /// <param name="cancelReasonID">取消原因ID</param>
    /// <param name="cancelReason">自定义取消原因，可选</param>
    /// <returns></returns>
    public static JObject CancelOrder(string orderID, int cancelReasonID, string cancelReason = "")
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);
        bizParam.Add("cancel_reason_id", cancelReasonID);
        bizParam.Add("cancel_reason", cancelReason);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/formalCancel", strReqParam);

        return JObject.Parse(strRspParam);
    }

    /// <summary>
    /// 增加小费
    /// https://newopen.imdada.cn/#/development/file/addTip
    /// </summary>
    /// <param name="orderID">订单ID</param>
    /// <param name="tips">小费金额(精确到小数点后一位，单位：元)</param>
    /// <param name="cityCode">订单城市区号</param>
    /// <param name="info">备注(字段最大长度：512)</param>
    /// <returns></returns>
    public static JObject AddTip(string orderID, decimal tips, string cityCode, string info = "")
    {
        if (!string.IsNullOrEmpty(info) && info.Length > 512)
        {
            throw new Exception("备注最长512个字符");
        }
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);
        bizParam.Add("tips", tips);
        bizParam.Add("city_code", cityCode);
        bizParam.Add("info", info);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/addTip", strReqParam);
        return JObject.Parse(strRspParam);
    }

    /// <summary>
    /// 获取商家投诉达达原因
    /// </summary>
    /// <returns></returns>
    public static JObject GetComplaintReason()
    {
        string strReqParam = GetRequestParam();
        string strRspParam = SendHttpRequest("/api/complaint/reasons", strReqParam);
        return JObject.Parse(strRspParam);
    }

    /// <summary>
    /// 商家投诉达达
    /// </summary>
    /// <param name="orderID">第三方订单编号</param>
    /// <param name="ReasonID">投诉原因ID</param>
    /// <returns></returns>
    public static JObject ComplaintDaDa(string orderID, int ReasonID)
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);
        bizParam.Add("reason_id", ReasonID);
        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/complaint/dada", strReqParam);
        return JObject.Parse(strRspParam);
    }

    /// <summary>
    /// 接受订单(仅在测试环境供调试使用)
    /// https://newopen.imdada.cn/#/development/file/accept
    /// 在测试环境中，可调用此接口接受订单，检测回调。
    /// 接口仅限于测试环境调试使用，且触发回调URL成功后，接口直接返回成功；否则，重复三次触发，每次间隔3秒，最后返回成功。
    /// </summary>
    /// <param name="orderID">第三方订单编号</param>
    /// <returns></returns>
    public static string AcceptOrder(string orderID)
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/accept", strReqParam);
        return strRspParam;
    }

    /// <summary>
    /// 完成取货(仅在测试环境供调试使用)
    /// https://newopen.imdada.cn/#/development/file/fetch
    /// 在测试环境中，可调用此接口完成取货，检测回调。
    /// 接口仅限于测试环境调试使用，且触发回调URL成功后，接口直接返回成功；否则，重复三次触发，每次间隔3秒，最后返回成功。
    /// </summary>
    /// <param name="orderID">第三方订单编号</param>
    /// <returns></returns>
    public static string FetchOrder(string orderID)
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/fetch", strReqParam);
        return strRspParam;
    }

    /// <summary>
    /// 完成订单(仅在测试环境供调试使用)
    /// https://newopen.imdada.cn/#/development/file/finish
    /// 在测试环境中，可调用此接口完成订单，检测回调。
    /// 接口仅限于测试环境调试使用，且触发回调URL成功后，接口直接返回成功；否则，重复三次触发，每次间隔3秒，最后返回成功。
    /// </summary>
    /// <param name="orderID">第三方订单编号</param>
    /// <returns></returns>
    public static string FinishOrder(string orderID)
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/finish", strReqParam);
        return strRspParam;
    }

    /// <summary>
    /// 取消订单(仅在测试环境供调试使用)
    /// https://newopen.imdada.cn/#/development/file/cancel
    /// 在测试环境中，可调用此接口取消订单，检测回调。
    /// 接口仅限于测试环境调试使用，且触发回调URL成功后，接口直接返回成功；否则，重复三次触发，每次间隔3秒，最后返回成功。
    /// </summary>
    /// <param name="orderID">第三方订单编号</param>
    /// <param name="reason">取消原因</param>
    /// <returns></returns>
    public static string CancelOrder(string orderID, string reason = "")
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);
        bizParam.Add("reason", reason);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/cancel", strReqParam);
        return strRspParam;
    }

    /// <summary>
    /// 订单过期(仅在测试环境供调试使用)
    /// https://newopen.imdada.cn/#/development/file/exipre
    /// 在测试环境中，可调用此接口模拟订单过期，检测回调。
    /// 接口仅限于测试环境调试使用，且触发回调URL成功后，接口直接返回成功；否则，重复三次触发，每次间隔3秒，最后返回成功。
    /// </summary>
    /// <param name="orderID">第三方订单编号</param>
    /// <returns></returns>
    public static string ExpireOrder(string orderID)
    {
        Dictionary<string, object> bizParam = new Dictionary<string, object>();
        bizParam.Add("order_id", orderID);

        string strReqParam = GetRequestParam(bizParam);
        string strRspParam = SendHttpRequest("/api/order/expire", strReqParam);
        return strRspParam;
    }

    /// <summary>
    /// 对指定的API发送HTTP请求
    /// </summary>
    /// <param name="api">API接口地址</param>
    /// <param name="reqParam">请求参数</param>
    /// <returns>API返回结果</returns>
    private static string SendHttpRequest(string api, string reqParam)
    {
        string rspParam = string.Empty;
        rspParam = HttpService.Post(reqParam, Config.DaDaServer + api, "application/json", false, Config.DaDaAPITimeout);
        return rspParam;
    }

    /// <summary>
    /// 生成达达请求参数
    /// </summary>
    /// <param name="bizParam">业务参数，可为null</param>
    /// <returns>报文请求字符串，JSON字符串</returns>
    private static string GetRequestParam(Dictionary<string, object> bizParam = null)
    {
        string body;
        body = bizParam != null ? JsonConvert.SerializeObject(bizParam) : string.Empty;
        SortedDictionary<string, object> reqParam = new SortedDictionary<string, object>();
        reqParam.Add("app_key", Config.DaDaAppKey);
        reqParam.Add("body", body);
        reqParam.Add("format", "json");
        reqParam.Add("source_id", Config.DaDaSourceID);
        reqParam.Add("timestamp", UtilityHelper.MakeTimeStamp());
        reqParam.Add("v", "1.0");
        //加入签名
        reqParam.Add("signature", GetSign(reqParam));
        return JsonConvert.SerializeObject(reqParam);
    }

    /// <summary>
    /// 生成达达报文签名
    /// https://newopen.imdada.cn/#/development/file/safety
    /// 第一步：将参与签名的参数按照键值(key)进行排序。
    /// 第二步：将排序过后的参数进行key value字符串拼接。
    /// 第三步：将拼接后的字符串首尾加上app_secret秘钥，合成签名字符串。
    /// 第四步：对签名字符串进行MD5加密，生成32位的字符串。
    /// 第五步：将签名生成的32位字符串转换为大写。
    /// </summary>
    /// <param name="reqParam">待签名对象</param>
    /// <returns>签名字符串</returns>
    private static string GetSign(SortedDictionary<string, object> reqParam)
    {
        StringBuilder sbParam = new StringBuilder();
        foreach (KeyValuePair<string, object> kvp in reqParam)
        {
            sbParam.Append(kvp.Key + kvp.Value);
        }
        string sign = string.Format("{0}{1}{0}", Config.DaDaAppSecret, sbParam.ToString());
        sign = UtilityHelper.GetMD5Hash(sign).ToUpper();

        return sign;
    }

    /// <summary>
    /// 校验达达返回的签名
    /// 对client_id, order_id, update_time的值进行字符串升序排列，再连接字符串，取md5值
    /// </summary>
    /// <param name="dadaSign">达达返回的报文签名</param>
    /// <param name="clientID"></param>
    /// <param name="orderID"></param>
    /// <param name="updateTime"></param>
    /// <returns>达达签名是否正确</returns>
    public static bool VerifyDaDaSign(string dadaSign, string clientID, string orderID, string updateTime)
    {
        SortedDictionary<string, object> sdDaDaSign = new SortedDictionary<string, object>();
        sdDaDaSign.Add(clientID, clientID);
        sdDaDaSign.Add(orderID, orderID);
        sdDaDaSign.Add(updateTime, updateTime);

        StringBuilder sbParam = new StringBuilder();
        foreach (KeyValuePair<string, object> kvp in sdDaDaSign)
        {
            sbParam.Append(kvp.Value);
        }
        string sign = sbParam.ToString();
        sign = UtilityHelper.GetMD5Hash(sign);

        return (dadaSign.ToUpper() == sign.ToUpper()) ? true : false;
    }


}