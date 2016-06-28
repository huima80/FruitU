<%@ WebHandler Language="C#" Class="WxCardHandler" %>

using System;
using System.Web;
using LitJson;

public class WxCardHandler : IHttpHandler {


    public void ProcessRequest(HttpContext context)
    {
        WxCard wxCard;
        string cardID, cardInfo = string.Empty;
        cardID = context.Request.QueryString["CardID"];
        if (!string.IsNullOrEmpty(cardID))
        {
            wxCard = WxCard.GetCard(cardID);
            cardInfo = JsonMapper.ToJson(wxCard);
        }
        context.Response.Clear();
        context.Response.ContentType = "text/plain";
        context.Response.Write(cardInfo);
        context.Response.End();
   }


    public bool IsReusable {
        get {
            return false;
        }
    }

}