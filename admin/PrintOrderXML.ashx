<%@ WebHandler Language="C#" Class="PrintOrder" %>

using System;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Xsl;
using System.Text;

public class PrintOrder : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
    public void ProcessRequest(HttpContext context)
    {

        string errMsg = string.Empty;
        string strPO = string.Empty;
        string contentType = string.Empty;

        try
        {
            //if (!HttpContext.Current.User.Identity.IsAuthenticated || !Roles.IsUserInRole(Config.AdminRoleName))
            //{
            //    throw new Exception("请先登录");
            //}

            int poID;
            if (!string.IsNullOrEmpty(context.Request.QueryString["POID"]))
            {
                if (int.TryParse(context.Request.QueryString["POID"], out poID))
                {
                    ProductOrder po = new ProductOrder(poID);
                    if (po != null)
                    {
                        if(context.Request["JSON"] != null)
                        {
                            strPO = PO2JSON(po);
                            contentType = "application/json";
                        }

                        if (context.Request["XML"] != null)
                        {
                            strPO = PO2XML(po);
                            contentType = "text/html";
                        }
                    }
                    else
                    {
                        throw new Exception("此订单不存在");
                    }
                }
                else
                {
                    throw new Exception("订单ID错误");
                }
            }
            else
            {
                throw new Exception("请指定订单ID");
            }
        }
        catch (Exception ex)
        {
            errMsg = string.Format("{{\"err_code_des\":\"{0}\"}}", ex.Message);
            Log.Error(this.GetType().ToString(), ex.Message);
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = contentType;
            if (!string.IsNullOrEmpty(strPO))
            {
                context.Response.Write(strPO);
            }
            else
            {
                context.Response.Write(errMsg);
            }
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

    /// <summary>
    /// 生成JSON格式订单
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public string PO2JSON(ProductOrder po)
    {
        string strPO = string.Empty;

        JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
        {
            JsonSerializerSettings jSetting = new JsonSerializerSettings();
            jSetting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            jSetting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            return jSetting;
        });
        po.OrderID = po.OrderID.Substring(18);
        strPO = JsonConvert.SerializeObject(po);
        return strPO;
    }

    /// <summary>
    /// 生成XML格式订单
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public string PO2XML(ProductOrder po)
    {
        StringBuilder sbRet = new StringBuilder();
        System.IO.MemoryStream memStream;
        using (memStream = new System.IO.MemoryStream())
        {
            //序列化订单对象
            XmlSerializer serializer = new XmlSerializer(typeof(ProductOrder));
            serializer.Serialize(memStream, po);
            XmlDocument xml = new XmlDocument();
            xml.Load(memStream);
            //加载样式表并处理XML
            XslCompiledTransform xslTransform = new XslCompiledTransform();
            xslTransform.Load("PrintOrder.xslt");
            xslTransform.Transform(xml, null, memStream);
            int count = 0;
            byte[] buffer = new byte[1024];
            while ((count = memStream.Read(buffer, 0, 1024)) > 0)
            {
                sbRet.Append(Encoding.UTF8.GetString(buffer, 0, count));
            }
        }
        return sbRet.ToString();

    }

}