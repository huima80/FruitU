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

    /// <summary>
    /// Hash an input string and return the hash as a 32 character hexadecimal string.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetMd5Hash(string input)
    {
        // Create a new instance of the MD5CryptoServiceProvider object.
        MD5 md5Hasher = MD5.Create();

        // Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    /// <summary>
    /// Verify a hash against a string.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static bool VerifyMd5Hash(string input, string hash)
    {
        // Hash the input.
        string hashOfInput = GetMd5Hash(input);

        // Create a StringComparer an compare the hashes.
        StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        if (0 == comparer.Compare(hashOfInput, hash))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 时间戳转为C#格式时间
    /// </summary>
    /// <param name="timeStamp">Unix时间戳格式</param>
    /// <returns>C#格式时间</returns>
    public static DateTime GetTime(string timeStamp)
    {
        DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        long lTime = long.Parse(timeStamp + "0000000");
        TimeSpan toNow = new TimeSpan(lTime);
        return dtStart.Add(toNow);
    }

    /// <summary>
    /// DateTime时间格式转换为Unix时间戳格式
    /// </summary>
    /// <param name="time"> DateTime时间格式</param>
    /// <returns>Unix时间戳格式</returns>
    public static int ConvertDateTimeInt(DateTime time)
    {
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
        return (int)(time - startTime).TotalSeconds;
    }
    #endregion
}
