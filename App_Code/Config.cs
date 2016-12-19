using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;

/// <summary>
/// 全局配置信息
/// </summary>
public static class Config
{
    /// <summary>
    /// 数据库链接字符串
    /// </summary>
    public static readonly string ConnStr;

    /// <summary>
    /// 网站Title
    /// </summary>
    public static readonly string SiteTitle;

    /// <summary>
    /// 网站简介
    /// </summary>
    public static readonly string SiteDesc;

    /// <summary>
    /// 网站搜索引擎关键字
    /// </summary>
    public static readonly string SiteKeywords;

    /// <summary>
    /// 网站图标
    /// </summary>
    public static readonly string SiteIcon;

    /// <summary>
    /// 网站版权
    /// </summary>
    public static readonly string SiteCopyrights;

    /// <summary>
    /// 微信支付订单（prepay_id）有效期（分钟）
    /// </summary>
    public static readonly int WeChatOrderExpire;

    /// <summary>
    /// 微信支付统一下单接口超时（秒）
    /// </summary>
    public static readonly int WeChatAPITimeout;

    /// <summary>
    /// 图片文件路径
    /// </summary>
    public static readonly string ImgPath;

    /// <summary>
    /// 微信公众号APP ID
    /// </summary>
    public const string APPID = "wxa1ce5cd8d4bc5b3f";

    /// <summary>
    /// 微信公众号APP SECRET
    /// </summary>
    public const string APPSECRET = "1bab5bf7c078f90843b536eb934cd7dc";

    /// <summary>
    /// 微信支付商户ID
    /// </summary>
    public const string MCHID = "1296478401";

    /// <summary>
    /// 微信支付商户密钥
    /// </summary>
    public const string MCHKEY = "sjeu763hd82ks11xbchdj83hcns1kdb2";

    /// <summary>
    /// 微信支付数字证书（仅退款、撤销订单时需要）
    /// </summary>
    public static readonly string SSLCertPath;

    /// <summary>
    /// 微信支付数字证书密码，同商户号
    /// </summary>
    public const string SSLCERT_PASSWORD = "1296478401";

    /// <summary>
    /// 支付结果通知回调url，用于商户接收支付结果
    /// </summary>
    public static readonly string PayNotifyUrl;

    //=======【商户系统后台机器IP】===================================== 
    /* 此参数可手动配置也可在程序中自动获取
    */
    public const string IP = "8.8.8.8";


    //=======【代理服务器设置】===================================
    /* 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
    */
    public const string PROXY_URL = "http://10.152.18.220:8080";

    //=======【上报信息配置】===================================
    /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
    */
    public const int REPORT_LEVENL = 1;

    //=======【日志级别】===================================
    /* 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息
    */
    public static readonly int LogLevel;

    /// <summary>
    /// 微信公众号开发者验证Token
    /// </summary>
    public static readonly string TestToken;

    /// <summary>
    /// 商品列表PageSize
    /// </summary>
    public static readonly int ProductListPageSize;

    /// <summary>
    /// 订单列表PageSize
    /// </summary>
    public static readonly int OrderListPageSize;

    /// <summary>
    /// 用户列表PageSize
    /// </summary>
    public static readonly int UserListPageSize;

    /// <summary>
    /// 允许上传的文件扩展名
    /// </summary>
    public static readonly string AllowedUploadFileExt;

    /// <summary>
    /// 默认图片
    /// </summary>
    public static readonly string DefaultImg;

    /// <summary>
    /// 微信模板消息接收者openid
    /// </summary>
    public static readonly List<string> WxTmplMsgReceiver;

    /// <summary>
    /// 商品库存量最低警戒线
    /// </summary>
    public const int ProductInventoryWarn = 10;

    /////////////////////////// QQ互联平台：http://connect.qq.com
    /// <summary>
    /// QQ互联APP ID
    /// </summary>
    public const string QQAppID = "101296477";

    /// <summary>
    /// QQ互联APP KEY
    /// </summary>
    public const string QQAppKey = "864b70ac0e208b75527191a9f69d0c0f";

    /// <summary>
    /// 管理员角色名称
    /// </summary>
    public const string AdminRoleName = "administrators";

    /// <summary>
    /// 访客角色名称
    /// </summary>
    public const string GuestRoleName = "guests";

    /// <summary>
    /// 会员积分兑换比率
    /// </summary>
    public static readonly int MemberPointsExchangeRate;

    /// <summary>
    /// 运费标准
    /// </summary>
    public static readonly decimal Freight;

    /// <summary>
    /// 免运费条件
    /// </summary>
    public static readonly decimal FreightFreeCondition;

    //////////////////////支付宝配置参数open.alipay.com////////////////////
    /// <summary>
    /// 支付宝开放平台网关
    /// </summary>
    public const string AlipayGateway = "https://openapi.alipay.com/gateway.do";

    /// <summary>
    /// 支付宝开放平台应用ID，查看地址：https://open.alipay.com
    /// </summary>
    public static readonly string AlipayAPPID;

    /// <summary>
    // 支付宝合作身份者ID，签约账号，以2088开头由16位纯数字组成的字符串，查看地址：https://b.alipay.com/order/pidAndKey.htm
    /// </summary>
    public static readonly string Partner;

    /// <summary>
    // 收款支付宝账号，以2088开头由16位纯数字组成的字符串，一般情况下收款账号就是签约账号
    /// </summary>
    public static readonly string SellerID = Partner;

    /// <summary>
    //商户的私钥文件路径,原始格式，RSA公私钥生成：https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.nBDxfy&treeId=58&articleId=103242&docType=1
    /// </summary>
    public static readonly string PrivateKey;

    /// <summary>
    //支付宝的公钥文件路径，查看地址：https://b.alipay.com/order/pidAndKey.htm 
    /// </summary>
    public static readonly string AlipayPublicKey;

    /// <summary>
    // 服务器异步通知页面路径，需http://格式的完整路径，不能加?id=123这类自定义参数,必须外网可以正常访问
    /// </summary>
    public static readonly string AlipayNotifyUrl;

    /// <summary>
    // 页面跳转同步通知页面路径，需http://格式的完整路径，不能加?id=123这类自定义参数，必须外网可以正常访问
    /// </summary>
    public static readonly string AlipayReturnUrl;

    //////////////////////支付宝配置参数////////////////////

    static Config()
    {
        Config.ConnStr = ConfigurationManager.ConnectionStrings["FruitU"].ToString();
        Config.SiteTitle = ConfigurationManager.AppSettings["SiteTitle"].ToString();
        Config.SiteDesc = ConfigurationManager.AppSettings["SiteDesc"].ToString();
        Config.SiteKeywords = ConfigurationManager.AppSettings["SiteKeywords"].ToString();
        Config.SiteCopyrights = ConfigurationManager.AppSettings["SiteCopyrights"].ToString();
        Config.SiteIcon = ConfigurationManager.AppSettings["SiteIcon"].ToString();
        Config.SSLCertPath = ConfigurationManager.AppSettings["SSLCertPath"].ToString();
        Config.PayNotifyUrl = ConfigurationManager.AppSettings["PayNotifyUrl"].ToString();
        Config.ImgPath = ConfigurationManager.AppSettings["ImgPath"].ToString();
        Config.TestToken = ConfigurationManager.AppSettings["TestToken"].ToString();
        Config.AllowedUploadFileExt = ConfigurationManager.AppSettings["AllowedUploadFileExt"].ToString();
        Config.DefaultImg = ConfigurationManager.AppSettings["DefaultImg"].ToString();
        Config.AlipayAPPID = ConfigurationManager.AppSettings["AlipayAPPID"].ToString();
        Config.Partner = ConfigurationManager.AppSettings["Partner"].ToString();
        Config.PrivateKey = ConfigurationManager.AppSettings["PrivateKey"].ToString();
        Config.AlipayPublicKey = ConfigurationManager.AppSettings["AlipayPublicKey"].ToString();
        Config.AlipayNotifyUrl = ConfigurationManager.AppSettings["AlipayNotifyUrl"].ToString();
        Config.AlipayReturnUrl = ConfigurationManager.AppSettings["AlipayReturnUrl"].ToString();

        int productListPageSize;
        if (int.TryParse(ConfigurationManager.AppSettings["ProductListPageSize"].ToString(), out productListPageSize))
        {
            Config.ProductListPageSize = productListPageSize;
        }
        else
        {
            //默认pagesize
            Config.ProductListPageSize = 10;
        }

        int orderListPageSize;
        if (int.TryParse(ConfigurationManager.AppSettings["OrderListPageSize"].ToString(), out orderListPageSize))
        {
            Config.OrderListPageSize = orderListPageSize;
        }
        else
        {
            //默认pagesize
            Config.OrderListPageSize = 10;
        }

        int userListPageSize;
        if (int.TryParse(ConfigurationManager.AppSettings["UserListPageSize"].ToString(), out userListPageSize))
        {
            Config.UserListPageSize = userListPageSize;
        }
        else
        {
            //默认pagesize
            Config.UserListPageSize = 10;
        }

        int uoTimeout;
        if (int.TryParse(ConfigurationManager.AppSettings["WeChatAPITimeout"].ToString(), out uoTimeout))
        {
            Config.WeChatAPITimeout = uoTimeout;
        }
        else
        {
            //默认超时5秒
            Config.WeChatAPITimeout = 5;
        }

        int weChatOrderExpire;
        if (int.TryParse(ConfigurationManager.AppSettings["WeChatOrderExpire"].ToString(), out weChatOrderExpire))
        {
            Config.WeChatOrderExpire = weChatOrderExpire;
        }
        else
        {
            //默认超时2小时
            Config.WeChatOrderExpire = 120;
        }

        int logLevel;
        if (int.TryParse(ConfigurationManager.AppSettings["LogLevel"].ToString(), out logLevel))
        {
            Config.LogLevel = logLevel;
        }
        else
        {
            //默认不输出日志
            Config.LogLevel = 0;
        }

        Config.WxTmplMsgReceiver = new List<string>();
        string strWxTmplMsgReceiver = ConfigurationManager.AppSettings["WxTmplMsgReceiver"].ToString();
        if (!string.IsNullOrEmpty(strWxTmplMsgReceiver))
        {
            string[] arrWxTmplMsgReceiver = strWxTmplMsgReceiver.Split('|');
            for (int i = 0; i < arrWxTmplMsgReceiver.Length; i++)
            {
                Config.WxTmplMsgReceiver.Add(arrWxTmplMsgReceiver[i]);
            }
        }

        int memberPointsExchangeRate;
        if (int.TryParse(ConfigurationManager.AppSettings["MemberPointsExchangeRate"].ToString(), out memberPointsExchangeRate))
        {
            Config.MemberPointsExchangeRate = memberPointsExchangeRate;
        }
        else
        {
            //默认兑换比率
            Config.MemberPointsExchangeRate = 20;
        }

        decimal freight;
        if (decimal.TryParse(ConfigurationManager.AppSettings["Freight"].ToString(), out freight))
        {
            Config.Freight = freight;
        }
        else
        {
            //默认运费标准
            Config.Freight = 0;
        }

        decimal freightFreeCondition;
        if (decimal.TryParse(ConfigurationManager.AppSettings["FreightFreeCondition"].ToString(), out freightFreeCondition))
        {
            Config.FreightFreeCondition = freightFreeCondition;
        }
        else
        {
            //默认包邮条件
            Config.FreightFreeCondition = 0;
        }

    }
}