using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Text.RegularExpressions;

public static class UtilityHelper
{
    #region 字段/属性
    #endregion

    #region 构造函数

    static UtilityHelper()
    {
    }

    #endregion

    #region 方法

    #region 检查加密签名是否一致 - public static bool CheckSignature(string signature, string timestamp, string nonce)

    /// <summary>
    /// 检查加密签名是否一致
    /// </summary>
    /// <param name="signature">微信加密签名</param>
    /// <param name="timestamp">时间戳</param>
    /// <param name="nonce">随机数</param>
    /// <returns></returns>
    public static bool CheckSignature(string signature, string timestamp, string nonce)
    {
        List<string> stringList = new List<string> { Config.TestToken, timestamp, nonce };
        // 字典排序
        stringList.Sort();
        return Sha1Encrypt(string.Join("", stringList)) == signature;
    }

    #endregion

    #region 对字符串SHA1加密 - public static string Sha1Encrypt(string targetString)

    /// <summary>
    /// 对字符串SHA1加密
    /// </summary>
    /// <param name="targetString">源字符串</param>
    /// <returns>加密后的十六进制字符串</returns>
    private static string Sha1Encrypt(string targetString)
    {
        byte[] byteArray = Encoding.Default.GetBytes(targetString);
        HashAlgorithm hashAlgorithm = new SHA1CryptoServiceProvider();
        byteArray = hashAlgorithm.ComputeHash(byteArray);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (byte item in byteArray)
        {
            stringBuilder.AppendFormat("{0:x2}", item);
        }
        return stringBuilder.ToString();
    }

    #endregion

    #region 根据加密类型对字符串SHA1加密 - public static string Sha1Encrypt(string targetString, string encryptType)

    /// <summary>
    /// 根据加密类型对字符串SHA1加密
    /// </summary>
    /// <param name="targetString">源字符串</param>
    /// <param name="encryptType">加密类型：MD5/SHA1</param>
    /// <returns>加密后的字符串</returns>
    private static string Sha1Encrypt(string targetString, string encryptType)
    {
        return FormsAuthentication.HashPasswordForStoringInConfigFile(targetString, encryptType);
    }

    #endregion

    public static void AntiSQLInjection(string str)
    {
        if(Regex.IsMatch(str, "(select|insert|update|delete|create|alter|drop|exec|\'|--)", RegexOptions.IgnoreCase))
        {
            throw new Exception("有非法SQL字符");
        }
    }

    #endregion
}
