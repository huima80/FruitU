using System;
using System.Collections.Generic;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// HttpService 的摘要说明
/// </summary>
public class HttpService
{

    public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
    {
        //直接确认，否则打不开    
        return true;
    }

    /// <summary>
    /// 发起HTTP POST请求，默认ContentType=text/xml
    /// </summary>
    /// <param name="content">提交内容</param>
    /// <param name="url">服务器URL</param>
    /// <param name="isUseCert">是否使用证书</param>
    /// <param name="timeout">服务器超时等待时间</param>
    /// <returns></returns>
    public static string Post(string content, string url, bool isUseCert, int timeout)
    {
        return Post(content, url, "text/xml", isUseCert, timeout);
    }

    /// <summary>
    /// 发起HTTP POST请求
    /// </summary>
    /// <param name="content">提交内容</param>
    /// <param name="url">服务器URL</param>
    /// <param name="contentType">数据格式</param>
    /// <param name="isUseCert">是否使用证书</param>
    /// <param name="timeout">服务器超时等待时间</param>
    /// <returns></returns>
    public static string Post(string content, string url, string contentType, bool isUseCert, int timeout)
    {
        //System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接

        //返回结果
        string result = string.Empty;

        HttpWebRequest request = null;
        //HttpWebResponse response = null;

        try
        {
            //设置最大连接数
            ServicePointManager.DefaultConnectionLimit = 200;
            //设置https验证方式
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            /***************************************************************
            * 下面设置HttpWebRequest的相关属性
            * ************************************************************/
            request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.Timeout = timeout * 1000;
            request.ContentType = contentType;
            byte[] bContent = System.Text.Encoding.UTF8.GetBytes(content);
            request.ContentLength = bContent.Length;

            //设置代理服务器
            if (!string.IsNullOrEmpty(Config.PROXY_URL))
            {
                WebProxy proxy = new WebProxy();
                proxy.Address = new Uri(Config.PROXY_URL);
                request.Proxy = proxy;
            }

            //是否使用证书
            if (isUseCert)
            {
                string path = HttpContext.Current.Request.PhysicalApplicationPath;
                X509Certificate2 cert = new X509Certificate2(path + Config.SSLCertPath, Config.SSLCERT_PASSWORD);
                request.ClientCertificates.Add(cert);
                Log.Debug("HttpService", "PostXml used cert");
            }

            //往服务器写入数据
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bContent, 0, bContent.Length);
                reqStream.Close();
            }

            //获取服务端返回
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                //获取服务端返回数据
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = sr.ReadToEnd().Trim();
                    sr.Close();
                }
            }

        }
        catch (System.Threading.ThreadAbortException e)
        {
            Log.Error("HttpService", "Thread - caught ThreadAbortException - resetting.");
            Log.Error("Exception message: {0}", e.Message);
            System.Threading.Thread.ResetAbort();
        }
        catch (WebException e)
        {
            Log.Error("HttpService", e.ToString());
            if (e.Status == WebExceptionStatus.ProtocolError)
            {
                Log.Error("HttpService", "StatusCode : " + ((HttpWebResponse)e.Response).StatusCode);
                Log.Error("HttpService", "StatusDescription : " + ((HttpWebResponse)e.Response).StatusDescription);
            }
        }
        catch (Exception e)
        {
            Log.Error("HttpService", e.ToString());
        }
        finally
        {
            //关闭连接和流
            //if (response != null)
            //{
            //    response.Close();
            //}
            if (request != null)
            {
                request.Abort();
            }
        }
        return result;
    }

    /// <summary>
    /// 处理http GET请求，返回数据
    /// </summary>
    /// <param name="url">请求的url地址</param>
    /// <returns>http GET成功后返回的数据，失败抛WebException异常</returns>
    public static string Get(string url)
    {
        //System.GC.Collect();
        string result = "";

        HttpWebRequest request = null;
        //HttpWebResponse response = null;

        //请求url以获取数据
        try
        {
            //设置最大连接数
            ServicePointManager.DefaultConnectionLimit = 200;
            //设置https验证方式
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback =
                        new RemoteCertificateValidationCallback(CheckValidationResult);
            }

            /***************************************************************
            * 下面设置HttpWebRequest的相关属性
            * ************************************************************/
            request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";

            //设置代理服务器
            if (!string.IsNullOrEmpty(Config.PROXY_URL))
            {
                WebProxy proxy = new WebProxy();
                proxy.Address = new Uri(Config.PROXY_URL);
                request.Proxy = proxy;
            }

            //获取服务器返回
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                //获取HTTP返回数据
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    result = sr.ReadToEnd().Trim();
                    sr.Close();
                }
            }

        }
        catch (System.Threading.ThreadAbortException e)
        {
            Log.Error("HttpService", "Thread - caught ThreadAbortException - resetting.");
            Log.Error("Exception message: {0}", e.Message);
            System.Threading.Thread.ResetAbort();
        }
        catch (WebException e)
        {
            Log.Error("HttpService", e.ToString());
            if (e.Status == WebExceptionStatus.ProtocolError)
            {
                Log.Error("HttpService", "StatusCode : " + ((HttpWebResponse)e.Response).StatusCode);
                Log.Error("HttpService", "StatusDescription : " + ((HttpWebResponse)e.Response).StatusDescription);
            }
        }
        catch (Exception e)
        {
            Log.Error("HttpService", e.ToString());
        }
        finally
        {
            //关闭连接和流
            //if (response != null)
            //{
            //    response.Close();
            //}
            if (request != null)
            {
                request.Abort();
            }
        }
        return result;
    }
}

