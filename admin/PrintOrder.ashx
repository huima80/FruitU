<%@ WebHandler Language="C#" Class="PrintOrder" %>

using System;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;

public class PrintOrder : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{
    public void ProcessRequest(HttpContext context)
    {

        string errMsg = string.Empty;
        string strPO = string.Empty;

        try
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated || !Roles.IsUserInRole(Config.AdminRoleName))
            {
                throw new Exception("请先登录");
            }

            int poID;
            if (!string.IsNullOrEmpty(context.Request.QueryString["POID"]))
            {
                if (int.TryParse(context.Request.QueryString["POID"], out poID))
                {
                    ProductOrder po = new ProductOrder(poID);
                    if (po != null)
                    {
                        JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                        {
                            JsonSerializerSettings jSetting = new JsonSerializerSettings();
                            jSetting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                            jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                            return jSetting;
                        });
                        po.OrderID = po.OrderID.Substring(18);
                        strPO = JsonConvert.SerializeObject(po);
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
            context.Response.ContentType = "text/plain";
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

}