using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections.Specialized;
using System.Collections.Generic;
using Com.Alipay;

/// <summary>
/// 功能：页面跳转同步通知页面
/// 版本：3.3
/// 日期：2012-07-10
/// 说明：
/// 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
/// 该代码仅供学习和研究支付宝接口使用，只是提供一个参考。
/// 
/// ///////////////////////页面功能说明///////////////////////
/// 该页面可在本机电脑测试
/// 可放入HTML等美化页面的代码、商户业务逻辑程序代码
/// 该页面可以使用ASP.NET开发工具调试，也可以使用写文本函数LogResult进行调试
/// </summary>
public partial class alipay_return : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            SortedDictionary<string, string> sdParam = GetRequestGet();
            //判断是否有带返回参数
            if (sdParam.Count > 0)
            {
                if (sdParam.ContainsKey("notify_id") && sdParam.ContainsKey("sign"))
                {
                    Notify aliNotify = new Notify();
                    bool verifyResult = aliNotify.Verify(sdParam, sdParam["notify_id"], sdParam["sign"]);

                    //验证成功
                    if (verifyResult)
                    {
                        //成功标识
                        if (sdParam.ContainsKey("is_success") && !string.IsNullOrEmpty(sdParam["is_success"]))
                        {
                            if (sdParam["is_success"] != "T")
                            {
                                throw new Exception("接口调用失败");
                            }
                        }
                        else
                        {
                            throw new Exception("缺少成功标识is_success");
                        }

                        //商户订单号
                        string orderID;
                        if (sdParam.ContainsKey("out_trade_no") && !string.IsNullOrEmpty(sdParam["out_trade_no"]))
                        {
                            orderID = sdParam["out_trade_no"];
                        }
                        else
                        {
                            throw new Exception("缺少商户订单号");
                        }

                        //卖家支付宝账户号
                        string sellerId;
                        if (sdParam.ContainsKey("seller_id") && !string.IsNullOrEmpty(sdParam["seller_id"]))
                        {
                            sellerId = sdParam["seller_id"];
                        }
                        else
                        {
                            throw new Exception("缺少卖家支付宝账户号");
                        }

                        //交易金额
                        decimal totalFee;
                        if (sdParam.ContainsKey("total_fee") && !string.IsNullOrEmpty(sdParam["total_fee"]))
                        {
                            if (!decimal.TryParse(sdParam["total_fee"], out totalFee))
                            {
                                throw new Exception("支付宝交易金额错误");
                            }
                        }
                        else
                        {
                            throw new Exception("缺少支付宝交易金额");
                        }

                        //根据订单ID加载ProductOrder对象
                        ProductOrder po = new ProductOrder(orderID);

                        if (!string.IsNullOrEmpty(po.OrderID))
                        {
                            //校验卖家ID
                            if (po.AP_SellerID == sellerId)
                            {
                                //校验订单金额
                                if (po.OrderPrice == totalFee)
                                {
                                    if (sdParam.ContainsKey("trade_status") && !string.IsNullOrEmpty(sdParam["trade_status"]))
                                    {
                                        //支付宝交易状态
                                        switch (sdParam["trade_status"].ToUpper())
                                        {
                                            case "TRADE_SUCCESS":
                                                this.divAliPayMsg.Attributes["class"] = "alert alert-success";
                                                this.divAliPayMsg.InnerText = "支付宝支付成功，请返回微信FruitU";
                                                break;
                                            case "TRADE_FINISHED":
                                                this.divAliPayMsg.Attributes["class"] = "alert alert-success";
                                                this.divAliPayMsg.InnerText = "支付宝支付结束，请返回微信FruitU";
                                                break;
                                            case "WAIT_BUYER_PAY":
                                                this.divAliPayMsg.Attributes["class"] = "alert alert-warning";
                                                this.divAliPayMsg.InnerText = "交易创建，等待买家付款";
                                                break;
                                            case "TRADE_CLOSED":
                                                this.divAliPayMsg.Attributes["class"] = "alert alert-info";
                                                this.divAliPayMsg.InnerText = "支付宝交易已关闭，请返回微信FruitU";
                                                break;
                                            case "TRADE_PENDING":
                                                this.divAliPayMsg.Attributes["class"] = "alert alert-info";
                                                this.divAliPayMsg.InnerText = "等待卖家收款，请返回微信FruitU";
                                                break;
                                            default:
                                                this.divAliPayMsg.Attributes["class"] = "alert alert-danger";
                                                this.divAliPayMsg.InnerText = "未知支付宝状态";
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception(string.Format("支付宝返回的订单总金额{0}元与原订单金额{1}元不符", totalFee, po.OrderPrice));
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("支付宝返回的卖家ID({0})与原订单({1})不符", sellerId, po.AP_SellerID));
                            }
                        }
                        else
                        {
                            throw new Exception(string.Format("没有找到支付宝返回的订单：{0}", orderID));
                        }
                    }
                    else
                    {
                        throw new Exception("支付宝通知验证失败");
                    }
                }
                else
                {
                    throw new Exception("无支付宝notify_id和sign");
                }
            }
            else
            {
                throw new Exception("无支付宝通知返回参数");
            }
        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), string.Format("支付宝通知出错 :{0}\n请求方IP：{1}", ex.Message + ex.StackTrace, Request.UserHostAddress));
            this.divAliPayMsg.Attributes["class"] = "alert alert-danger";
            this.divAliPayMsg.InnerHtml = string.Format("提示：{0}<br>请返回FruitU查看订单", ex.Message);
        }
    }

    /// <summary>
    /// 获取支付宝GET过来通知消息，并以“参数名=参数值”的形式组成数组
    /// </summary>
    /// <returns>request回来的信息组成的数组</returns>
    public SortedDictionary<string, string> GetRequestGet()
    {
        SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
        NameValueCollection coll;
        //Load Form variables into NameValueCollection variable.
        coll = Request.QueryString;

        // Get names of all forms into a string array.
        String[] requestItem = coll.AllKeys;

        for (int i = 0; i < requestItem.Length; i++)
        {
            sArray.Add(requestItem[i], Request.QueryString[requestItem[i]]);
        }

        return sArray;
    }
}