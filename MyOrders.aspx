<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyOrders.aspx.cs" Inherits="MyOrders" %>

<!DOCTYPE html>
<html>
<head>
    <title>我的订单</title>
    <link rel="shortcut icon" href="images/FruitU.ico" type="image/x-icon" />
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="keywords" content="Fruit,slice,juice,水果,果汁,切片" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="black" />
    <meta name="format-detection" content="telephone=no" />
    <meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1.0, maximum-scale=1.0" />
    <link href="http://apps.bdimg.com/libs/bootstrap/3.3.0/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/font-awesome.min.css" rel="stylesheet" />
    <link href="css/common.css" rel="stylesheet" />
    <link href="css/MyOrders.css" rel="stylesheet" />
    <link href="css/loading.css" rel="stylesheet" />
    <link href="css/ladda-themeless.min.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div id="divOrderItems" class="row">
            </div>
        </div>
        <!-- #include file="footer.html" -->
    </form>
</body>

<!-- Declare a JsRender template, in a script block: -->
<script id="tmplOrderPage" type="text/x-jsrender">
    <div class="col-xs-12 col-sm-4">
        <div class="order-item">
            <div><span class="order-id">订单号：{{:OrderID}}</span><span class="order-state">{{:TradeStateText}}</span></div>
            <ul>
                {{for OrderDetailList}}
                <li>
                    <img src="images/{{:FruitImgList[0].ImgName}}" class="prod-img">
                    <span class="order-product-name">{{:OrderProductName}}</span>  <span class="purchase-price">￥{{:PurchasePrice}}</span><span class="purchase-unit">元/{{:PurchaseUnit}}</span> <span class="purchase-qty">x {{:PurchaseQty}}</span></li>
                {{/for}}
            </ul>
            <div class="order-date">下单时间：{{:OrderDate}}</div>
            <div class="deliver-date">配送时间：{{:DeliverDate}}</div>
            <div class="order-price">
                合计：￥<span class="order-price">{{:OrderPrice}}</span>元
                {{if TradeState==3 || TradeState==7}}
                <button class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="WxPay({{:ID}});"><span class="ladda-label"><i class="fa fa-wechat fa-fw"></i>微信支付</span><span class="ladda-spinner"></span></button>
                {{/if}}
            </div>
        </div>
    </div>
</script>

<script>
    var Ladda;

    requirejs(['jquery'], function ($) {
        $(function () {

            requirejs(['pager', 'ladda'], function (pager, ladda) {

                //设为全局对象，方便后面调用
                window.Ladda = ladda;

                $.pager.init({
                    pageSize: pageSize,
                    pageQueryURL: 'OrderListPager.ashx',
                    pageTemplate: '#tmplOrderPage',
                    pageContainer: '#divOrderItems',
                });

                $($.pager).on("onPageLoaded", function () {
                    //绑定页面上所有的ladda button
                    Ladda.bind('button.ladda-button');
                });

                $.pager.loadPage();
            });

        });
    });

    var wxJsApiParam = "", lastPoID = "";

    //调用微信JS api 支付，后台调用统一下单接口生成所需参数后，在微信浏览器中调用此函数发起支付
    function onBridgeReady() {
        WeixinJSBridge.invoke(
            'getBrandWCPayRequest',
             wxJsApiParam,
             function (res) {
                 WeixinJSBridge.log(res.err_msg);
                 //alert(res.err_code + res.err_desc + res.err_msg);
                 if (res.err_msg.indexOf("ok") != -1) {
                     alert("付款成功！我们将为您送货上门。");
                     location.href = "MyOrders.aspx";
                 }
                 else {
                     if (res.err_msg.indexOf("cancel") != -1) {
                         alert("您取消了支付，请继续完成支付。");
                     }
                     else {
                         alert("支付失败，请重新下单。");
                     }
                 }
                 Ladda.stopAll();   //停止按钮loading动画

             });
    }

    function JsApiPay() {
        if (typeof WeixinJSBridge == "undefined") {
            if (document.addEventListener) {
                document.addEventListener('WeixinJSBridgeReady', onBridgeReady, false);
            }
            else if (document.attachEvent) {
                document.attachEvent('WeixinJSBridgeReady', onBridgeReady);
                document.attachEvent('onWeixinJSBridgeReady', onBridgeReady);
            }
        }
        else {
            onBridgeReady();
        }
    }

    //微信支付按钮单击事件
    function WxPay(poID) {

        //lastPoID是最近一次点击微信支付指向的订单ID，如果当前函数传入的poID和lastPoID不一致，这说明用户点击了两个不同订单的微信支付按钮，第二次需要清空变量wxJsApiParam，重新向后台发起统一下单
        if (lastPoID != "" && lastPoID != poID) {
            wxJsApiParam = "";   //清空上次统一下单返回的JS支付参数
        }

        //记录当前点击的订单ID
        lastPoID = poID;

        //如果JS支付参数不为空，说明已经获取到prepay_id，则直接发起支付
        if (wxJsApiParam != "" && wxJsApiParam.package != undefined) {

            JsApiPay();
        }
        else {
            $.get("JSAPIPay.ashx", { PoID: poID }, function (response) {
                if (response["package"] != null)  //统一下单正常，取到了prepay_id，发起支付
                {
                    wxJsApiParam = response;
                    JsApiPay();
                }
                else {
                    if (response["return_code"] != null)  //可能是签名错误或参数格式错误
                    {
                        alert(response["return_msg"]);
                    }
                    else {
                        if (response["result_code"] != null)  //可能是订单已支付、已关闭、订单号重复等错误
                        {
                            alert("错误代码：" + response["err_code"] + "\n错误信息：" + response["err_code_des"]);
                        }
                    }

                    Ladda.stopAll();   //停止按钮loading动画

                }
            }, "json");
        }
    }

    ////定义分页变量
    //var pageIndex = 1, totalRows, totalPages = 1;
    //var docHeight = 0; //页面总长度  
    //var scrollTop = 0;   //页面上滚高度  
    //var winHeight = $(window).height(); //窗口可视高度
    //var isPaging = false;   //是否正在分页处理


    ////浏览器向上滑动分页
    //function scrollUpPager() {
    //    if (isPaging) {
    //        return;
    //    }

    //    // 判断窗口的滚动条是否接近页面底部
    //    docHeight = $(document).height();
    //    scrollTop = $(document).scrollTop();
    //    if (scrollTop + winHeight >= docHeight) {

    //        // 判断是否最后一页
    //        if (pageIndex <= totalPages) {

    //            //标示正在处理分页，避免并发
    //            isPaging = true;

    //            loadOrderList();

    //        } else {
    //            $("div.loading-text").fadeIn("fast");
    //            setTimeout("$('div.loading-text').fadeOut()", 2000);
    //        }
    //    }
    //}

    ////ajax加载订单列表
    //function loadOrderList() {
    //    // 显示加载进度条
    //    $("div.loading").fadeIn("fast");

    //    // ajax查询下一页数据并附加在底部
    //    $.ajax({
    //        url: "OrderListPager.ashx",
    //        data: { PageIndex: pageIndex, PageSize: pageSize, R: Math.random() },
    //        type: "GET",
    //        dataType: "json",
    //        success: function (jOrderListPager) {

    //            if (jOrderListPager == undefined || jOrderListPager == null || jOrderListPager == "") {
    //                return false;
    //            }

    //            var htmlItem = "";

    //            //遍历展示当前页所有订单信息，并添加到新增的div中
    //            $.each(jOrderListPager, function (iOL, nOL) {

    //                if (this["ID"] != undefined) {

    //                    htmlItem += '<div class="col-xs-12 col-sm-4" style="display:none;"><div class="order-item">';

    //                    htmlItem += '<div><span class="order-id">订单号：' + this["OrderID"].substr(18) + '</span>';

    //                    //根据TradeState显示订单支付状态，对于未支付或支付失败的订单，再显示微信支付按钮
    //                    switch (this["TradeState"]) {
    //                        case 1: //支付成功
    //                            htmlItem += '<span class="order-state pay-success">支付成功</span>';
    //                            break;
    //                        case 2:   //转入退款
    //                            htmlItem += '<span class="order-state not-pay">转入退款</span>';
    //                            break;
    //                        case 3:   //未支付
    //                            htmlItem += '<span class="order-state not-pay">未支付</span>';
    //                            break;
    //                        case 4:   //已关闭
    //                            htmlItem += '<span class="order-state not-pay">已关闭</span>';
    //                            break;
    //                        case 5:   //已撤销（刷卡支付）
    //                            htmlItem += '<span class="order-state not-pay">已撤销（刷卡支付）</span>';
    //                            break;
    //                        case 6:   //用户支付中
    //                            htmlItem += '<span class="order-state not-pay">用户支付中</span>';
    //                            break;
    //                        case 7:   //支付失败
    //                            htmlItem += '<span class="order-state not-pay">支付失败</span>';
    //                            break;
    //                    }

    //                    htmlItem += '</div><ul>';

    //                    $.each(this["OrderDetailList"], function (iODL, nODL) {
    //                        htmlItem += '<li><img src="images/' + (this["FruitImgList"][0] != null ? this["FruitImgList"][0]["ImgName"] : defaultImg) + '" class="prod-img">  <span class="order-product-name">' + this["OrderProductName"] + '</span>  <span class="purchase-price">￥' + this["PurchasePrice"] + '</span><span class="purchase-unit">元/' + this["PurchaseUnit"] + '</span> <span class="purchase-qty">x ' + this["PurchaseQty"] + '</span></li>';
    //                    });

    //                    htmlItem += '</ul><div class="order-date">下单时间：' + (new Date(this["OrderDate"])).toLocaleDateString("zh-cn") + '</div>';
    //                    htmlItem += '<div class="deliver-date">' + (this["IsDelivered"] == true ? "配送时间：" + (new Date(this["DeliverDate"])).toLocaleDateString("zh-cn") : "未配送") + '</div>';
    //                    htmlItem += '<div class="order-price">合计：￥<span class="order-price">' + this["OrderPrice"] + '</span>元';

    //                    switch (this["TradeState"]) {
    //                        case 3:   //未支付
    //                        case 7:   //支付失败
    //                            htmlItem += '<button class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="WxPay(' + this["ID"] + ');"><i class="fa fa-wechat fa-fw"></i>微信支付</button>';
    //                            break;
    //                    }

    //                    htmlItem += "</div></div></div>";

    //                }
    //                else {
    //                    //如果是第一页（totalRows == undefined）；或用户在翻页过程中，如商品数有变动，则更新当前的记录总数，并重新计算页数
    //                    if (totalRows != this["TotalRows"]) {
    //                        totalRows = this["TotalRows"];

    //                        //计算总页数
    //                        if (totalRows % pageSize == 0) {
    //                            totalPages = parseInt(totalRows / pageSize);
    //                        }
    //                        else {
    //                            totalPages = parseInt(totalRows / pageSize) + 1;
    //                        }
    //                    }
    //                }
    //            });

    //            //追加并淡出新增的div
    //            $(htmlItem).appendTo($("#divOrderItems")).fadeIn(1000);

    //            //下一页加载成功后，页数+1
    //            pageIndex++;

    //            // 隐藏加载进度条
    //            $("div.loading").fadeOut();

    //            //绑定页面上所有的button动画事件
    //            Ladda.bind('button.ladda-button');

    //            //标示分页处理结束
    //            isPaging = false;

    //        },
    //        error: function (xhr, err_msg, e) {
    //            console.log(e + ":" + err_msg);

    //            //标示分页处理结束
    //            isPaging = false;

    //            // 隐藏加载进度条
    //            $("div.loading").fadeOut();

    //        }
    //    });

    //}

</script>
</html>
