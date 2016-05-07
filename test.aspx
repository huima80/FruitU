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
    <link href="css/ladda-themeless.min.css" rel="stylesheet" />
    <link href="css/font-awesome.min.css" rel="stylesheet" />
    <link href="css/MasterPage.css" rel="stylesheet" />
    <link href="css/shake.css" rel="stylesheet" />
</head>
<body>
        <nav class="navbar navbar-default navbar-fixed-top">
        <div class="container-fluid">
            <!-- Brand and toggle get grouped for better mobile display -->
            <div class="navbar-header">
                <button id="btnShowMenu" type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#divOrderSearchCriteria" aria-expanded="false">
                    <span class="sr-only">Toggle navigation</span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                </button>
                <p class="navbar-text">
                    <i class="fa fa-file-o"></i>&nbsp;订单数<span id="spanOrderSubmitted"></span>
                    <i class="fa fa-credit-card"></i>&nbsp;未支付<span id="spanOrderPaid"></span>
                    <i class="fa fa-truck"></i>&nbsp;未发货<span id="spanOrderDelivered"></span>
                    <i class="fa fa-pencil-square-o"></i>&nbsp;未签收<span id="spanOrderAccepted"></span>
                </p>
            </div>
            <!-- Collect the nav links, forms, and other content for toggling -->
            <div class="collapse navbar-collapse" id="divOrderSearchCriteria">
                <div class="form-group">
                    <input id="txtOrderID" type="text" class="form-control" placeholder="订单号" />
                    <input id="txtProdName" type="text" class="form-control" placeholder="商品名称" />
                    <input id="txtStartOrderDate" type="date" class="form-control" placeholder="开始下单时间" />
                    <input id="txtEndOrderDate" type="date" class="form-control" placeholder="结束下单时间" />
                </div>
                <button id="btnSearch" type="button" class="btn btn-info">查找订单</button>
                <button id="btnAllOrder" type="button" class="btn btn-warning">全部订单</button>
            </div>
            <!-- /.navbar-collapse -->
        </div>
    </nav>

    <div class="container">
        <div id="" class="row">
        <div class="col-xs-12 col-sm-4">
            <div class="order-item">

                <div class="order-price-freight">
                    <div class="order-total">

                        <button id="btnWxPay1" class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="WxPay(1);"><i class="fa fa-wechat fa-fw"></i>微信支付</button>
                        <button id="btnWxPay2" class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="WxPay(2);"><i class="fa fa-wechat fa-fw"></i>微信支付</button>
                        <button id="btnWxPay3" class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="WxPay(3);"><i class="fa fa-wechat fa-fw"></i>微信支付</button>
                        <button id="btnWxPay303" class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="WxPay(303);"><span class="ladda-label"><i class="fa fa-wechat fa-fw"></i>微信支付</span><span class="ladda-spinner"></span></button>
                        <button id="btnStop" class="btn btn-info" type="button" onclick="stop();">停止</button>
                    </div>
                </div>

            </div>
        </div>


    </div>
   </div>
        <div class="container">
        <div id="divOrderItems" class="row">
        </div>
    </div>

</body>

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplOrderPage" type="text/x-jsrender">
        <div class="col-xs-12 col-sm-4">
            <div class="order-item">
<%--                <div>
                    <span class="order-id">订单号：{{:OrderID}}</span>
                    <span class="order-date">下单时间：{{:OrderDate}}</span>
                </div>--%>
<%--                <ul>
                    {{for OrderDetailList}}
                <li>
                    <img src="images/{{:FruitImgList[0].ImgName}}" class="prod-img" />
                    <span class="order-product-name">{{:OrderProductName}}</span>  <span class="purchase-price">￥{{:PurchasePrice}}</span><span class="purchase-unit">元/{{:PurchaseUnit}}</span> <span class="purchase-qty">x {{:PurchaseQty}}</span></li>
                    {{/for}}
                </ul>--%>
                <div class="order-price-freight">
                    <div class="order-total">
                        合计：￥<span class="">{{:OrderPrice}}</span>元
                {{if IsCancel==0 && (TradeState!=1 && TradeState!=8)}}
                <button id="btnWxPay{{:ID}}" class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="WxPay({{:ID}});"><i class="fa fa-wechat fa-fw"></i>微信支付</button>
                        {{/if}}
                    </div>
<%--                    <div><span class="">(含运费{{:Freight}}元，积分优惠{{:MemberPointsDiscount}}元)</span></div>--%>
                </div>
                <hr />
<%--                <div class="order-state">
                    <span class="done">
                        <i class="fa fa-file-o"></i>&nbsp;下单
                    </span>
                    {{if IsCancel==0 && (TradeState!=1 && TradeState!=8) && IsDelivered==0 && IsAccept==0}}
                       <span id="CancelOrder{{:ID}}" class="doing btn-cancel" onclick="cancelOrder({{:ID}});">
                           取消订单
                           {{else IsCancel==1}}
                        <span class="done">(已撤单)
                           {{else TradeState==1 || IsDelivered==1 || IsAccept==1}}
                         <span>{{/if}}
                         </span><span class="done">—</span>
                            {{if TradeState==1 || TradeState==8}}
                     <span class="done">{{else}}
                       <span id="WxPayOrder{{:ID}}" class="doing">{{/if}}
                    <i class="fa fa-credit-card"></i>&nbsp;支付—
                       </span>
                         {{if IsDelivered==1}}
                       <span class="done">{{else}}
                       <span class="doing">{{/if}}
                   <i class="fa fa-truck"></i>&nbsp;发货—
                       </span>
                           {{if IsAccept==1}}
                        <span class="done">{{else IsCancel==1}}
                            <span class="doing">{{else IsCancel!=1}}
                       <span id="AcceptOrder{{:ID}}" class="doing" onclick="acceptOrder({{:ID}});">{{/if}}
                   <i class="fa fa-pencil-square-o"></i>&nbsp;签收
                       </span>
                </div>--%>
            </div>
        </div>
    </script>

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
            webConfig: 'webConfig'
        }
    });
</script>


    <script>

    var Ladda;

    requirejs(['jquery'], function ($) {
        require(['bootstrap']);

        $(function () {
            requirejs(['pager', 'ladda'], function (pager, ladda) {
                Ladda = ladda;
                $.pager.init({
                    pageSize: 10,
                    pageQueryURL: 'OrderListPager.ashx',
                    pageTemplate: '#tmplOrderPage',
                    pageContainer: '#divOrderItems',
                });

                $($.pager).on("onPageLoaded", function (event, data) {

                    Ladda.bind('button.ladda-button');
                });
                //Ladda.bind('button.ladda-button');

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

                     //付款成功后，隐藏撤单、修改订单状态、隐藏微信支付按钮
                     $("#CancelOrder" + lastPoID).hide();
                     $("#WxPayOrder" + lastPoID).removeClass("doing").addClass("done");
                     $("#btnWxPay" + lastPoID).hide();
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
            $.ajax({
                url: "JSAPIPay.ashx",
                data: { PoID: poID },
                type: "GET",
                dataType: "json",
                cache: false,
                success: function (response) {
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
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    Ladda.stopAll();   //停止按钮loading动画
                    console.warn(errorThrown + ":" + textStatus);
                }
            });
        }
    }

    function stop() {
        Ladda.stopAll();   //停止按钮loading动画
    }

</script>
</html>
