using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// 闪送业务类
/// </summary>
public static class ShanSongBiz
{
    static ShanSongBiz()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    /// <summary>
    /// 计算费用
    /// </summary>
    /// <param name="ssOrder">闪送订单</param>
    /// <param name="addOrderType">1：计算费用；2：下单</param>
    /// <returns></returns>
    public static JObject AddOrder(ShanSongOrder ssOrder, int addOrderType)
    {
        if (ssOrder.ProductOrder == null || string.IsNullOrEmpty(ssOrder.ProductOrder.OrderID))
        {
            throw new Exception("业务订单不能为空");
        }

        JObject jReqParam = new JObject(
            new JProperty("partnerNo", Config.ShanSongPartnerNo),
            new JProperty("signature", GetSign(Config.ShanSongPartnerNo, ssOrder.ProductOrder.OrderID, Config.ShanSongMerchantMobile)),
            new JProperty("order", new JObject(
                new JProperty("orderNo", ssOrder.ProductOrder.OrderID),
                new JProperty("merchant", new JObject(
                    new JProperty("id", Config.ShanSongMerchantID),
                    new JProperty("mobile", Config.ShanSongMerchantMobile),
                    new JProperty("name", Config.ShanSongMerchantName),
                    new JProperty("token", ssOrder.MerchantToken))),
                new JProperty("sender", new JObject(
                    new JProperty("mobile", ssOrder.SenderMobile),
                    new JProperty("name", ssOrder.SenderName),
                    new JProperty("city", ssOrder.SenderCity),
                    new JProperty("addr", ssOrder.SenderAddr),
                    new JProperty("addrDetail", ssOrder.SenderAddrDetail),
                    new JProperty("lng", ssOrder.SenderLng),
                    new JProperty("lat", ssOrder.SenderLat))),
                new JProperty("receiverList", new JArray(new JObject(
                    new JProperty("mobile", ssOrder.ReceiverMobile),
                    new JProperty("name", ssOrder.ReceiverName),
                    new JProperty("city", ssOrder.ReceiverCity),
                    new JProperty("addr", ssOrder.ReceiverAddr),
                    new JProperty("addrDetail", ssOrder.ReceiverAddrDetail),
                    new JProperty("lng", ssOrder.ReceiverLng),
                    new JProperty("lat", ssOrder.ReceiverLat)))),
                new JProperty("goods", ssOrder.ProductOrder.OrderDetails),
                new JProperty("weight", ssOrder.Weight),
                new JProperty("addition", ssOrder.Addition),
                new JProperty("appointTime", ssOrder.AppointTime),
                new JProperty("remark", ssOrder.Remark))));

        string strRspParam;
        switch (addOrderType)
        {
            case 1:
                strRspParam = SendHttpRequest("POST", "/openapi/order/v2/calc", jReqParam.ToString());
                break;
            case 2:
                strRspParam = SendHttpRequest("POST", "/openapi/order/v2/save", jReqParam.ToString());
                break;
            default:
                throw new Exception("订单发布方式错误");
        }

        JObject jRsp = JObject.Parse(strRspParam);
        if (jRsp["errCode"].ToString() == "0" && jRsp["data"] != null)
        {
            if (jRsp["data"]["amount"] != null)
            {
                decimal amount;
                if (decimal.TryParse(jRsp["data"]["amount"].ToString(), out amount))
                {
                    ssOrder.Amount = amount;
                }
                else
                {
                    ssOrder.Amount = 0;
                }
            }

            //获取距离
            if (jRsp["data"]["distance"] != null)
            {
                int distance;
                if (int.TryParse(jRsp["data"]["distance"].ToString(), out distance))
                {
                    ssOrder.Distance = distance;
                }
                else
                {
                    ssOrder.Distance = 0;
                }
            }

            //获取重量
            if (jRsp["data"]["weight"] != null)
            {
                int weight;
                if (int.TryParse(jRsp["data"]["weight"].ToString(), out weight))
                {
                    ssOrder.Weight = weight;
                }
                else
                {
                    ssOrder.Weight = 0;
                }
            }

            //获取优惠金额
            if (jRsp["data"]["cutAmount"] != null)
            {
                decimal cutAmount;
                if (decimal.TryParse(jRsp["data"]["cutAmount"].ToString(), out cutAmount))
                {
                    ssOrder.CutAmount = cutAmount;
                }
                else
                {
                    ssOrder.CutAmount = 0;
                }
            }

            //获取闪送平台的订单号
            if (!jRsp["data"].HasValues)
            {
                ssOrder.ISSOrderNo = jRsp["data"].ToString();
            }
        }

        return jRsp;
    }

    /// <summary>
    /// 查询订单状态
    /// </summary>
    /// <param name="ssOrder">闪送订单</param>
    /// <returns></returns>
    public static JObject Info(ShanSongOrder ssOrder)
    {
        if (ssOrder.ProductOrder == null || string.IsNullOrEmpty(ssOrder.ProductOrder.OrderID))
        {
            throw new Exception("业务订单不能为空");
        }
        if (string.IsNullOrEmpty(ssOrder.ISSOrderNo))
        {
            throw new Exception("闪送订单号不能为空");
        }

        string strReqParam;
        strReqParam = string.Format("partnerno={0}&orderno={1}&mobile={2}&signature={3}&issorderno={4}", Config.ShanSongPartnerNo, ssOrder.ProductOrder.OrderID, Config.ShanSongMerchantMobile, GetSign(Config.ShanSongPartnerNo, ssOrder.ProductOrder.OrderID, Config.ShanSongMerchantMobile), ssOrder.ISSOrderNo);
        string strRspParam = SendHttpRequest("GET", "/openapi/order/v2/info", strReqParam);

        JObject jRsp = JObject.Parse(strRspParam);

        if (jRsp["errCode"].ToString() == "0" && jRsp["data"] != null)
        {
            if (jRsp["data"]["courier"] != null)
            {
                ssOrder.Courier = jRsp["data"]["courier"].ToString();
            }

            if (jRsp["data"]["courierName"] != null)
            {
                ssOrder.CourierName = jRsp["data"]["courierName"].ToString();
            }

            if (jRsp["data"]["pickupPassword"] != null)
            {
                ssOrder.PickupPassword = jRsp["data"]["pickupPassword"].ToString();
            }

            if (jRsp["data"]["orderStatus"] != null)
            {
                switch (jRsp["data"]["orderStatus"].ToString())
                {
                    case "20":
                        ssOrder.OrderStatus = ShanSongOrderStatus.Accepting;
                        break;
                    case "30":
                        ssOrder.OrderStatus = ShanSongOrderStatus.Fetching;
                        break;
                    case "42":
                        ssOrder.OrderStatus = ShanSongOrderStatus.Fetched;
                        break;
                    case "44":
                        ssOrder.OrderStatus = ShanSongOrderStatus.Delivering;
                        break;
                    case "60":
                        ssOrder.OrderStatus = ShanSongOrderStatus.Finished;
                        break;
                    case "64":
                        ssOrder.OrderStatus = ShanSongOrderStatus.Cancelled;
                        break;
                    default:
                        throw new Exception("闪送订单状态错误");
                }
            }
        }

        return jRsp;
    }

    /// <summary>
    /// 查询闪送员轨迹
    /// </summary>
    /// <param name="ssOrder">闪送订单</param>
    /// <returns></returns>
    public static JObject Trail(ShanSongOrder ssOrder)
    {
        if (ssOrder.ProductOrder == null || string.IsNullOrEmpty(ssOrder.ProductOrder.OrderID))
        {
            throw new Exception("业务订单不能为空");
        }
        if (string.IsNullOrEmpty(ssOrder.ISSOrderNo))
        {
            throw new Exception("闪送订单号不能为空");
        }

        string strReqParam;
        strReqParam = string.Format("partnerno={0}&orderno={1}&mobile={2}&signature={3}&issorderno={4}", Config.ShanSongPartnerNo, ssOrder.ProductOrder.OrderID, Config.ShanSongMerchantMobile, GetSign(Config.ShanSongPartnerNo, ssOrder.ProductOrder.OrderID, Config.ShanSongMerchantMobile), ssOrder.ISSOrderNo);
        string strRspParam = SendHttpRequest("GET", "/openapi/order/v2/trail", strReqParam);

        JObject jRsp = JObject.Parse(strRspParam);

        if (jRsp["errCode"].ToString() == "0" && jRsp["data"] != null)
        {
            if (jRsp["data"]["courier"] != null)
            {
                ssOrder.Courier = jRsp["data"]["courier"].ToString();
            }

            if (jRsp["data"]["trialMessage"] != null)
            {
                ssOrder.TrialMessage = new List<ShanSongTrialMessage>();

                foreach (var item in jRsp["data"]["trialMessage"])
                {
                    ShanSongTrialMessage tm = new ShanSongTrialMessage();
                    double lng;
                    if (double.TryParse(item["lng"].ToString(), out lng))
                    {
                        tm.Lng = lng;
                    }
                    else
                    {
                        tm.Lng = 0;
                    }
                    double lat;
                    if (double.TryParse(item["lat"].ToString(), out lat))
                    {
                        tm.Lat = lat;
                    }
                    else
                    {
                        tm.Lat = 0;
                    }
                    DateTime time;
                    if (DateTime.TryParse(item["time"].ToString(), out time))
                    {
                        tm.Time = time;
                    }
                    else
                    {
                        tm.Time = null;
                    }

                    ssOrder.TrialMessage.Add(tm);
                }
            }
        }

        return jRsp;
    }

    /// <summary>
    /// 取消订单
    /// </summary>
    /// <param name="ssOrder">闪送订单</param>
    /// <returns></returns>
    public static JObject Cancel(ShanSongOrder ssOrder)
    {
        if (ssOrder.ProductOrder == null || string.IsNullOrEmpty(ssOrder.ProductOrder.OrderID))
        {
            throw new Exception("业务订单不能为空");
        }
        if (string.IsNullOrEmpty(ssOrder.ISSOrderNo))
        {
            throw new Exception("闪送订单号不能为空");
        }

        string strReqParam;
        strReqParam = string.Format("partnerno={0}&orderno={1}&mobile={2}&signature={3}&issorderno={4}", Config.ShanSongPartnerNo, ssOrder.ProductOrder.OrderID, Config.ShanSongMerchantMobile, GetSign(Config.ShanSongPartnerNo, ssOrder.ProductOrder.OrderID, Config.ShanSongMerchantMobile), ssOrder.ISSOrderNo);
        string strRspParam = SendHttpRequest("GET", "/openapi/order/v2/cancel", strReqParam);

        JObject jRsp = JObject.Parse(strRspParam);

        if (jRsp["errCode"].ToString() == "0" && jRsp["data"] != null)
        {
            if (jRsp["data"]["amount"] != null)
            {
                decimal deductFee;
                if (decimal.TryParse(jRsp["data"]["amount"].ToString(), out deductFee))
                {
                    ssOrder.DeductFee = deductFee;
                }
                else
                {
                    ssOrder.DeductFee = 0;
                }
            }
        }

        return jRsp;
    }

    /// <summary>
    /// 账户余额查询
    /// </summary>
    /// <returns></returns>
    public static JObject Account()
    {
        string strReqParam;
        strReqParam = string.Format("partnerno={0}&mobile={1}", Config.ShanSongPartnerNo, Config.ShanSongMerchantMobile);
        string strRspParam = SendHttpRequest("GET", "/openapi/order/v2/account", strReqParam);

        JObject jRsp = JObject.Parse(strRspParam);

        return jRsp;
    }

    /// <summary>
    /// 发送HTTP请求
    /// </summary>
    /// <param name="httpVerb">请求动词：POST, GET</param>
    /// <param name="api">API地址</param>
    /// <param name="reqParam">请求参数</param>
    /// <returns></returns>
    private static string SendHttpRequest(string httpVerb, string api, string reqParam)
    {
        string rspParam = string.Empty;
        switch (httpVerb)
        {
            case "GET":
                rspParam = HttpService.Get(Config.ShanSongServer + api + "?" + reqParam);
                break;
            case "POST":
                rspParam = HttpService.Post(reqParam, Config.ShanSongServer + api, "application/json", false, Config.DaDaAPITimeout);
                break;
            default:
                throw new Exception("HTTP请求方法错误");
        }
        return rspParam;
    }

    /// <summary>
    /// 闪送报文签名
    /// </summary>
    /// <param name="partnerNo">合作伙伴编号</param>
    /// <param name="orderNo">合作伙伴订单号</param>
    /// <param name="mobile">下单人手机号码</param>
    /// <returns></returns>
    private static string GetSign(string partnerNo, string orderNo, string mobile)
    {
        string sign = string.Format("{0}{1}{2}{3}", partnerNo, orderNo, mobile, Config.ShanSongMD5Key);
        sign = UtilityHelper.GetMD5Hash(sign).ToUpper();

        return sign;
    }

}