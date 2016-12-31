<%@ Page Language="C#" AutoEventWireup="true" CodeFile="login.aspx.cs" Inherits="login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" lang="zh-CN">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1.0, maximum-scale=1.0" />
    <link href="http://apps.bdimg.com/libs/bootstrap/3.3.0/css/bootstrap.min.css" rel="stylesheet" />
    <title>登录</title>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="jumbotron">
                <h1 id="SiteTitle" runat="server"></h1>
                <p id="SiteDesc" runat="server">Fresh Fruits & Juice</p>
                <p>
                    <a href="qqauth.ashx">
                        <img alt="" src="images/Connect_logo_5.png" /></a>
                </p>
            </div>
            <asp:Label ID="lblMsg" runat="server" Text="" class="alert alert-danger" role="alert" Visible="False"></asp:Label>
        </div>
    </form>
</body>
</html>
