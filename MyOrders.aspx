<%@ Page Title="我的订单" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="MyOrders.aspx.cs" Inherits="MyOrders" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/MyOrders.css" rel="stylesheet" />
    <link href="Scripts/ladda/ladda-themeless.min.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
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
        <div id="divOrderItems" class="row">
        </div>
    </div>

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplOrderPage" type="text/x-jsrender">
        <div class="col-xs-12 col-sm-4">
            <div class="order-item">
                <div>
                    <span class="order-id">订单号：{{:OrderID}}</span>
                    <span class="order-date">下单时间：{{:OrderDate}}</span>
                </div>
                <ul>
                    {{for OrderDetailList}}
                <li>
                    <img src="images/{{:FruitImgList[0].ImgName}}" class="prod-img" />
                    <span class="order-product-name">{{:OrderProductName}}</span>  <span class="purchase-price">￥{{:PurchasePrice}}</span><span class="purchase-unit">元/{{:PurchaseUnit}}</span> <span class="purchase-qty">x {{:PurchaseQty}}</span></li>
                    {{/for}}
                </ul>
                <div class="order-price-freight">
                    <div class="order-total">
                        合计：￥<span class="order-price">{{:OrderPrice}}</span>元
                    </div>
                    <div class="freight">
                        (含运费{{:Freight}}元{{if MemberPointsDiscount != 0}}，积分优惠{{:MemberPointsDiscount}}元{{/if}}{{if WxCardDiscount != 0}}，微信优惠券优惠{{:WxCardDiscount}}元{{/if}})
                    </div>
                </div>
                {{if IsCancel==0 && (TradeState!=1 && TradeState!=8 && TradeState!=12 && TradeState!=14)}}
                <div class="btn-pay">
                    <button id="btnWxPay{{:ID}}" class="btn btn-wxpay ladda-button" type="button" data-style="zoom-in" onclick="wxPay({{:ID}});"><i class="fa fa-wechat fa-fw"></i>微信支付</button>
                    <button id="btnAliPay{{:ID}}" class="btn btn-alipay ladda-button" type="button" data-style="zoom-in" onclick="aliPay({{:ID}});">
                        <img src="images/alipay.png" />
                        支付宝</button>
                </div>
                {{/if}}
                <hr />
                <div class="order-state">
                    <span class="done">
                        <i class="fa fa-file-o"></i>&nbsp;下单
                    </span>
                    {{if IsCancel==0 && (TradeState!=1 && TradeState!=8 && TradeState!=12 && TradeState!=14) && IsDelivered==0 && IsAccept==0}}
                       <span id="CancelOrder{{:ID}}" class="doing btn-cancel" onclick="cancelOrder({{:ID}});">&nbsp;取消订单&nbsp;{{else IsCancel==1}}<span class="done">(已撤单){{else TradeState==1 || TradeState==8 || TradeState==12 || TradeState==14 || IsDelivered==1 || IsAccept==1}}<span>{{/if}}</span>
                           <span class="done">—</span>
                           {{if TradeState==1 || TradeState==8 || TradeState==12 || TradeState==14}}
                     <span class="done">{{else}}
                       <span id="PayOrder{{:ID}}" class="doing">{{/if}}
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
                </div>
            </div>
        </div>
    </script>

    <script>
        var Ladda;

        requirejs(['jquery', 'bootstrap', 'encoder'], function ($) {
            $(function () {

                if (typeof paymentTerm == undefined) {
                    alert("参数错误：支付方式");
                    return false;
                }

                if (typeof apGateway == undefined) {
                    alert("参数错误：支付宝网关");
                    return false;
                }

                requirejs(['pager', 'ladda'], function (pager, ladda) {

                    //设为全局对象，方便后面调用
                    Ladda = ladda;

                    $.pager.init({
                        pageSize: pageSize,
                        pageQueryURL: 'OrderListPager.ashx',
                        pageTemplate: '#tmplOrderPage',
                        pageContainer: '#divOrderItems',
                    });

                    $($.pager).on("onPageLoaded", function (event, data) {
                        //刷新订单各状态统计数
                        if (data.originalDataPerPage && Array.isArray(data.originalDataPerPage)) {
                            $(data.originalDataPerPage).each(function () {
                                if (this.TotalRows != undefined) {
                                    $("#spanOrderSubmitted").text(this.TotalRows);
                                }
                                if (this.PayingOrderCount != undefined) {
                                    $("#spanOrderPaid").text(this.PayingOrderCount);
                                }
                                if (this.DeliveringOrderCount != undefined) {
                                    $("#spanOrderDelivered").text(this.DeliveringOrderCount);
                                }
                                if (this.AcceptingOrderCount != undefined) {
                                    $("#spanOrderAccepted").text(this.AcceptingOrderCount);
                                }
                            });
                        }

                        //绑定分页新生成的ladda button
                        var $lButtons = $("button.ladda-button").filter(function (index, element) {
                            return $(".ladda-spinner", this).length == 0;
                        });
                        $lButtons.each(function () {
                            Ladda.bind(this);
                        });
                    });

                    $.pager.loadPage();

                });

                $("#btnSearch").on("click", searchOrder);
                $("#btnAllOrder").on("click", searchAllOrder);

                $("#txtStartOrderDate").on("change", function () {
                    if ($(this).val() != "" && $("#txtEndOrderDate").val() != "")
                        if ($(this).val() > $("#txtEndOrderDate").val()) {
                            alert("开始时间必须早于结束时间。");
                            $(this).val("");
                        }
                });
                $("#txtEndOrderDate").on("change", function () {
                    if ($(this).val() != "" && $("#txtStartOrderDate").val() != "")
                        if ($(this).val() < $("#txtStartOrderDate").val()) {
                            alert("结束时间必须晚于开始时间。");
                            $(this).val("");
                        }
                });

            });
        });

        var wxPayParam = "", lastPoID = "";

        //调用微信JS api 支付，后台调用统一下单接口生成所需参数后，在微信浏览器中调用此函数发起支付
        function onBridgeReady() {
            WeixinJSBridge.invoke(
                'getBrandWCPayRequest',
                 wxPayParam,
                 function (res) {
                     WeixinJSBridge.log(res.err_msg);
                     //alert(res.err_code + res.err_desc + res.err_msg);
                     if (res.err_msg.indexOf("ok") != -1) {
                         alert("付款成功！我们将为您送货上门。");

                         //付款成功后，隐藏撤单、修改订单状态、隐藏微信支付按钮
                         $("#CancelOrder" + lastPoID).hide();
                         $("#PayOrder" + lastPoID).removeClass("doing").addClass("done");
                         $("#btnWxPay" + lastPoID).hide();
                         $("#btnAliPay" + lastPoID).hide();
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
        function wxPay(poID) {

            //lastPoID是最近一次点击微信支付指向的订单ID，如果当前函数传入的poID和lastPoID不一致，这说明用户点击了两个不同订单的微信支付按钮，第二次需要清空变量wxPayParam，重新向后台发起统一下单
            if (lastPoID != "" && lastPoID != poID) {
                wxPayParam = "";   //清空上次统一下单返回的JS支付参数
            }

            //记录当前点击的订单ID
            lastPoID = poID;

            //如果JS支付参数不为空，说明已经获取到prepay_id，则直接发起支付
            if (wxPayParam != "" && wxPayParam.package != undefined) {

                JsApiPay();
            }
            else {
                $.ajax({
                    url: "PlaceOrder.ashx",
                    data: { PoID: poID, PaymentTerm: paymentTerm.wechat },
                    type: "GET",
                    dataType: "json",
                    cache: false,
                    success: function (response) {
                        if (response["package"] != null)  //统一下单正常，取到了prepay_id，发起支付
                        {
                            wxPayParam = response;
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

        //支付宝支付
        function aliPay(poID) {

            //lastPoID是最近一次点击微信支付指向的订单ID，如果当前函数传入的poID和lastPoID不一致，这说明用户点击了两个不同订单的微信支付按钮，第二次需要清空变量wxPayParam，重新向后台发起统一下单
            if (lastPoID != "" && lastPoID != poID) {
                wxPayParam = "";   //清空上次统一下单返回的JS支付参数
            }

            //记录当前点击的订单ID
            lastPoID = poID;

            //提交订单
            $.ajax({
                url: "PlaceOrder.ashx",
                data: { PoID: poID, PaymentTerm: paymentTerm.alipay },
                type: "GET",
                dataType: "text",
                cache: false,
                success: function (alipayParam) {
                    if (typeof encoder == "object" && apGateway != undefined && alipayParam != undefined && alipayParam.indexOf("sign") != -1) {
                        //后台获取到支付宝请求参数，前台再跳转到支付宝进行付款
                        var aliPayUrl = apGateway + "?" + alipayParam;
                        location.href = "AliPayTip.aspx?goto=" + encoder.encode(aliPayUrl);
                    }
                    else {
                        alert("没有获取到支付宝参数");
                        Ladda.stopAll();   //停止按钮loading动画
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(errorThrown + ":" + textStatus);
                    Ladda.stopAll();   //停止按钮loading动画
                }
            });
        }

        //撤单
        function cancelOrder(poID) {
            if (!confirm("撤单后不能恢复，您确认吗？")) {
                return false;
            }
            else {
                $("#CancelOrder" + poID).prop("onclick", "").text("撤单中...");
                $.ajax({
                    url: "CancelOrder.ashx",
                    data: { PoID: poID },
                    type: "GET",
                    dataType: "json",
                    cache: false,
                    success: function (response) {
                        if (response["result_code"] == "SUCCESS") {
                            alert("撤单成功");
                            $("#CancelOrder" + poID).prop("onclick", "").text("(已撤单)").removeClass("doing btn-cancel").addClass("done");
                            $("#AcceptOrder" + poID).prop("onclick", "");
                            $("#btnWxPay" + poID).hide();
                            $("#btnAliPay" + poID).hide();
                        }
                        else {
                            alert("撤单失败：" + response["err_code_des"]);
                            $("#CancelOrder" + poID).prop("onclick", "cancelOrder(" + poID + ");").html('(<i class="fa fa-close"></i>&nbsp;撤单)').removeClass("done").addClass("doing");
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert("撤单失败：" + textStatus);
                        $("#CancelOrder" + poID).prop("onclick", "cancelOrder(" + poID + ");").html('(<i class="fa fa-close"></i>&nbsp;撤单)').removeClass("done").addClass("doing");
                        console.warn(errorThrown + ":" + textStatus);
                    }
                });
            }

            return false;
        }

        //签收订单
        function acceptOrder(poID) {
            if (!confirm("您确认签收吗？")) {
                return false;
            }
            else {
                $("#AcceptOrder" + poID).prop("onclick", "").text("签收中...");
                $.ajax({
                    url: "AcceptOrder.ashx",
                    data: { PoID: poID },
                    type: "GET",
                    dataType: "json",
                    cache: false,
                    success: function (response) {
                        if (response["result_code"] == "SUCCESS") {
                            alert("签收成功");
                            $("#AcceptOrder" + poID).prop("onclick", "").text("签收").removeClass("doing").addClass("done");
                            $("#CancelOrder" + poID).prop("onclick", "");
                        }
                        else {
                            alert("签收失败：" + response["err_code_des"]);
                            $("#AcceptOrder" + poID).prop("onclick", "acceptOrder(" + poID + ");").removeClass("done").addClass("doing");
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert("签收失败：" + textStatus);
                        $("#AcceptOrder" + poID).prop("onclick", "acceptOrder(" + poID + ");").removeClass("done").addClass("doing");
                        console.warn(errorThrown + ":" + textStatus);
                    }
                });
            }

            return false;
        }

        //查询订单
        function searchOrder() {
            var pageQueryCriteria = {};
            if ($("#txtOrderID").val().trim() != "") {
                pageQueryCriteria.OrderID = $("#txtOrderID").val().trim();
            }
            else {
                pageQueryCriteria.OrderID = '';
            }
            if ($("#txtProdName").val().trim() != "") {
                pageQueryCriteria.ProdName = $("#txtProdName").val().trim();
            }
            else {
                pageQueryCriteria.ProdName = '';
            }
            if ($("#txtStartOrderDate").val().trim() != "") {
                pageQueryCriteria.StartOrderDate = $("#txtStartOrderDate").val().trim();
            }
            else {
                pageQueryCriteria.StartOrderDate = '';
            }
            if ($("#txtEndOrderDate").val().trim() != "") {
                pageQueryCriteria.EndOrderDate = $("#txtEndOrderDate").val().trim();
            }
            else {
                pageQueryCriteria.EndOrderDate = '';
            }
            $($.pager).on("onPageLoaded", showSearchTip);
            $.pager.loadPage({ pageQueryCriteria: pageQueryCriteria, pageIndex: 1 });
            $("#btnShowMenu").click();
        }

        //显示所有订单
        function searchAllOrder() {
            $("#txtOrderID").val("");
            $("#txtProdName").val("");
            $("#txtStartOrderDate").val("");
            $("#txtEndOrderDate").val("");
            //如果此函数被其他地方调用时，查询窗口还未显示，则不操作
            if ($("#divOrderSearchCriteria").is(":visible")) {
                $("#btnShowMenu").click();
            }
            $($.pager).off("onPageLoaded", showSearchTip);
            $.pager.loadPage({ pageQueryCriteria: { OrderID: '', ProdName: '', StartOrderDate: '', EndOrderDate: '' }, pageIndex: 1 });
        }

        //显示搜索后的提示
        function showSearchTip(event, data) {
            if ((this.totalRows() != 0) && (data.pageIndex == this.totalPages()) && !$("#pTips").is(":visible")) {
                //搜索结果最后一页显示提示信息
                $(this.settings.pageContainer).append('<p id="pTips" class="text-center text-danger" onclick="searchAllOrder();">不是您想找的？点我查看全部订单。</p>');
            }
            if (this.totalRows() == 0 && !$("#pTips").is(":visible")) {
                $(this.settings.pageContainer).append('<p id="pTips" class="text-center text-danger" onclick="searchAllOrder();">啥也没找到哦！点我查看全部订单。</p>');
            }
        }

    </script>
</asp:Content>
