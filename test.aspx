<%@ Page Language="C#" AutoEventWireup="true" CodeFile="test.aspx.cs" Inherits="test" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1.0, maximum-scale=1.0" />
    <meta http-equiv="content-type" content="text/html;charset=utf-8" />
    <title>测试</title>
    <script src="Scripts/require.js"></script>
    <link href="css/MyOrders.css" rel="stylesheet" />
    <link href="css/bootstrap.min-3.3.5.css" rel="stylesheet" />
    <link href="Scripts/ladda/ladda-themeless.min.css" rel="stylesheet" />
    <link href="css/font-awesome.min.css" rel="stylesheet" />
    <link href="css/MasterPage.css" rel="stylesheet" />
    <link href="css/shake.css" rel="stylesheet" />
</head>
<body>
    <form runat="server">
    <div class="container">
        <div class="row" id="divWxCard">
            <div class="col-sm-10">
                微信优惠券
            </div>
        </div>
        收件人号码：<asp:TextBox ID="txtTo" runat="server"></asp:TextBox>
        <br />
        信息内容：<asp:TextBox ID="txtBody" runat="server" Rows="4" TextMode="MultiLine"></asp:TextBox>
        <br />
        <asp:Button ID="btnSendSMS" runat="server" OnClick="btnSendSMS_Click" Text="发送短信" />
    </div>
        <asp:Button ID="btnGetCityCode" runat="server" OnClick="btnGetCityCode_Click" Text="获取达达城市编码" />
        <asp:DropDownList ID="ddlCityCode" runat="server">
        </asp:DropDownList>
        <asp:TextBox ID="txtOrderID" runat="server" ToolTip="业务订单ID"></asp:TextBox>
        <asp:RadioButtonList ID="rblAddOrderType" runat="server">
            <asp:ListItem Selected="True" Value="1">新增订单</asp:ListItem>
            <asp:ListItem Value="2">重新发单</asp:ListItem>
            <asp:ListItem Value="3">订单预发布</asp:ListItem>
        </asp:RadioButtonList>
        <br />
        <asp:Button ID="btnSendOrder" runat="server" OnClick="btnSendOrder_Click" Text="新增达达订单" />
        <asp:Button ID="btnAccept" runat="server" OnClick="btnAccept_Click" Text="模拟接单" />
        <asp:Button ID="btnFetch" runat="server" OnClick="btnFetch_Click" Text="模拟完成取货" />
        <asp:Button ID="btnFinish" runat="server" OnClick="btnFinish_Click" Text="模拟完成订单" />
        <asp:Button ID="btnCancel" runat="server" OnClick="btnCancel_Click" Text="模拟取消订单" />
        <asp:Button ID="btnExpire" runat="server" OnClick="btnExpire_Click" Text="模拟订单过期" />
        <asp:Label ID="lblMsg" runat="server" Text=""></asp:Label>

        <textarea id="txtTest" rows="3"></textarea>
        <button onclick="sub();">提交</button>
    </form>
</body>


<script>
    requirejs.config({
        //By default load any module IDs from js/lib
        baseUrl: 'Scripts',
        //except, if the module ID starts with "app",
        //load it from the js/app directory. paths
        //config is relative to the baseUrl, and
        //never includes a ".js" extension since
        //the paths config could be for a directory.
        paths: {
            jquery: ['jquery/jquery-1.12.2.min', 'http://apps.bdimg.com/libs/jquery/2.1.4/jquery.min'],
            jqueryui: ['jquery/jquery-ui-1.11.4.min', 'http://apps.bdimg.com/libs/jqueryui/1.10.4/jquery-ui.min'],
            bootstrap: ['bootstrap-3.3.6.min', 'http://cdn.bootcss.com/bootstrap/3.3.6/js/bootstrap.min'],
            easyui: 'easyui-1.4.4',
            modernizr: 'modernizr',
            gridstack: ['gridstack/gridstack-0.2.4.min', '//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.2.4/gridstack.min'],
            ladda: 'ladda/ladda',
            spin: 'ladda/spin',
            masonry: ['masonry.pkgd.min', 'https://npmcdn.com/masonry-layout@4.0/dist/masonry.pkgd.min'],
            jweixin: 'http://res.wx.qq.com/open/js/jweixin-1.0.0',
            lodash: 'lodash.min',
            html5shiv: 'http://apps.bdimg.com/libs/html5shiv/3.7/html5shiv.min',
            respond: 'http://apps.bdimg.com/libs/respond.js/1.4.2/respond',
            jsrender: 'jsrender',
            flexslider: 'jquery.flexslider-min',
            jssorslider: 'jssor.slider.mini',
            cart: 'cart',
            pager: 'pager',
            webConfig: 'webConfig',
            //高德地图
            amap: "http://webapi.amap.com/maps?v=1.3&key=aee0e92073edb1ecddc7303ece02eba5&callback=init",
        }
    });
</script>


    <script>
        requirejs(['jquery', 'jweixin'], function ($, wx) {

            $(function () {
                if (typeof cardParam == "object") {
                    $.extend(cardParam, {
                        success: function (res) {
                            var cardList = res.cardList; // 用户选中的卡券列表信息
                            alert(JSON.stringify(cardList));
                        }
                    });

                    //注册选择微信卡券点击事件处理函数
                    $("#divWxCard").on("click", wx, wxChooseCard);
                }
                else {
                    console.warn("微信卡券参数错误");
                }

                //requirejs(['dada'], function (dada) {
                //    var dadaClient = new dada.DaDaClient();
                //    dadaClient.cityCode(function (jRet) {
                //        console.log(jRet);
                //    });
                //});

            });

            
        });

        function sub(){
            var orderInfo = {
                a: $("#txtTest").val().trim()
            };
            
            $.ajax({
                data: JSON.stringify(orderInfo),
                method: "POST",
                url: "test.ashx"
            });
        }

        requirejs(['amap'], function () {
            window.init = function () {
                require(['require-initMap'], function (mapIniter) {
                    mapIniter.init();
                })
            }

            AMap.service('AMap.Geocoder', function () {
                //实例化Geocoder
                geocoder = new AMap.Geocoder({
                    city: "021"//城市，默认：“全国”
                });
                //TODO: 使用geocoder 对象完成相关功能
                geocoder.getLocation("北京市海淀区苏州街", function (status, result) {
                    if (status === 'complete' && result.info === 'OK') {
                        //TODO:获得了有效经纬度，可以做一些展示工作
                        //比如在获得的经纬度上打上一个Marker
                    } else {
                        //获取经纬度失败
                    }
                });
            });
        });


        //拉取微信卡券列表并获取用户选择信息
        function wxChooseCard(event) {
            var wx = event.data;
            try {
                wx.chooseCard(cardParam);
            }
            catch (error) {
                alert(error.message);
            }
        }

</script>
</html>
