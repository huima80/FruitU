<%@ WebHandler Language="C#" Class="test" %>

using System;
using System.Web;

public class test : IHttpHandler {

    public void ProcessRequest (HttpContext context) {

        string str = context.Request.Form[0];


        context.Response.ContentType = "text/plain";
        context.Response.Write("Hello World");
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}