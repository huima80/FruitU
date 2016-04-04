<%@ WebHandler Language="C#" Class="CancelOrder" %>

using System;
using System.Web;

public class CancelOrder : IHttpHandler, System.Web.SessionState.IRequiresSessionState {

    public void ProcessRequest(HttpContext context)
    {
        //微信统一下单API返回的错误码
        string stateCode = string.Empty;

        try
        {
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            int poID = 0;

            if (wxUser != null && !string.IsNullOrEmpty(context.Request.QueryString["PoID"]))
            {
                poID = int.Parse(context.Request.QueryString["PoID"]);

                if (ProductOrder.UpdateOrderCancel(poID, wxUser.OpenID, true) == 1)
                {
                    stateCode = string.Format("{{\"result_code\":\"SUCCESS\",\"po_id\":\"{0}\"}}", poID);
                }
                else
                {
                    stateCode = string.Format("{{\"result_code\":\"FAIL\",\"err_code_des\":\"撤单失败\"}}");
                }
            }
            else
            {
                stateCode = string.Format("{{\"result_code\":\"FAIL\",\"err_code_des\":\"请指定订单ID\"}}");
            }
        }
        catch (Exception ex)
        {
            Log.Error("CancelOrder", ex.Message);
            stateCode = string.Format("{{\"result_code\":\"FAIL\",\"err_code_des\":\"{0}\"}}", ex.Message);
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.Write(stateCode);
            context.Response.End();
        }
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}