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

</script>
</html>
