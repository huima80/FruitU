<%@ WebHandler Language="C#" Class="CheckGroupPurchaseEvent" %>

using System;
using System.Web;
using System.Collections.Generic;
using LitJson;

public class CheckGroupPurchaseEvent : IHttpHandler, System.Web.SessionState.IReadOnlySessionState
{

    public void ProcessRequest(HttpContext context)
    {
        int eventSuccessCount = 0, eventGoingCount = 0, eventFailCount = 0;

        try
        {
            WeChatUser wxUser = context.Session["WxUser"] as WeChatUser;

            if (wxUser == null || string.IsNullOrEmpty(wxUser.OpenID))
            {
                throw new Exception("请登录");
            }

            //查询用户在进行中的团购活动信息
            List<GroupPurchaseEvent> groupEventList = GroupPurchaseEvent.FindGroupPurchaseEventByOpenID(wxUser.OpenID);
            groupEventList.ForEach(groupEvent =>
            {
                switch (groupEvent.GroupEventStatus)
                {
                    case GroupEventStatus.EVENT_SUCCESS:
                        eventSuccessCount++;
                        break;
                    case GroupEventStatus.EVENT_GOING:
                        eventGoingCount++;
                        break;
                    case GroupEventStatus.EVENT_FAIL:
                        eventFailCount++;
                        break;
                }
            });

        }
        catch (Exception ex)
        {
            Log.Error(this.GetType().ToString(), ex.Message);
        }
        finally
        {
            context.Response.Clear();
            context.Response.ContentType = "application/json";
            JsonData jGroupEvent = new JsonData();
            jGroupEvent["eventSuccessCount"] = eventSuccessCount;
            jGroupEvent["eventGoingCount"] = eventGoingCount;
            jGroupEvent["eventFailCount"] = eventFailCount;
            context.Response.Write(jGroupEvent.ToJson());
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