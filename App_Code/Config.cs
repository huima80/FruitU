using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;


/**
* 	全局配置信息
*/
public static class Config
{
    /// <summary>
    /// 网站Title
    /// </summary>
    public static readonly string SiteTitle;

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
    /// 允许上传的文件扩展名
    /// </summary>
    public static readonly string AllowedUploadFileExt;

    /// <summary>
    /// 默认图片
    /// </summary>
    public static readonly string DefaultImg;

    static Config()
    {
        Config.SiteTitle = ConfigurationManager.AppSettings["SiteTitle"].ToString();
        Config.SSLCertPath = ConfigurationManager.AppSettings["SSLCertPath"].ToString();
        Config.PayNotifyUrl = ConfigurationManager.AppSettings["PayNotifyUrl"].ToString();
        Config.ImgPath = ConfigurationManager.AppSettings["ImgPath"].ToString();
        Config.TestToken = ConfigurationManager.AppSettings["TestToken"].ToString();
        Config.AllowedUploadFileExt = ConfigurationManager.AppSettings["AllowedUploadFileExt"].ToString();
        Config.DefaultImg = ConfigurationManager.AppSettings["DefaultImg"].ToString();

        int productListPageSize;
        if (!int.TryParse(ConfigurationManager.AppSettings["ProductListPageSize"].ToString(), out productListPageSize))
        {
            //默认pagesize
            Config.ProductListPageSize = 10;
        }
        else
        {
            Config.ProductListPageSize = productListPageSize;
        }

        int orderListPageSize;
        if (!int.TryParse(ConfigurationManager.AppSettings["OrderListPageSize"].ToString(),out orderListPageSize))
        {
            //默认pagesize
            Config.OrderListPageSize = 10;
        }
        else
        {
            Config.OrderListPageSize = orderListPageSize;
        }

        int uoTimeout;
        if(!int.TryParse(ConfigurationManager.AppSettings["WeChatAPITimeout"].ToString(),out uoTimeout))
        {
            //默认超时5秒
            Config.WeChatAPITimeout = 5;
        }
        else
        {
            Config.WeChatAPITimeout = uoTimeout;
        }

        int weChatOrderExpire;
        if (!int.TryParse(ConfigurationManager.AppSettings["WeChatOrderExpire"].ToString(), out weChatOrderExpire))
        {
            //默认超时2小时
            Config.WeChatOrderExpire = 120;
        }
        else
        {
            Config.WeChatOrderExpire = weChatOrderExpire;
        }

        int logLevel;
        if (!int.TryParse(ConfigurationManager.AppSettings["LogLevel"].ToString(), out logLevel))
        {
            //默认不输出日志
            Config.LogLevel = 0;
        }
        else
        {
            Config.LogLevel = logLevel;
        }

    }
}
