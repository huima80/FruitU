<%@ Page Title="我的订单" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="MyOrders.aspx.cs" Inherits="MyOrders" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/MyOrders.css" rel="stylesheet" />
    <link href="css/loading.css" rel="stylesheet" />
    <link href="css/ladda-themeless.min.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div id="divOrderItems" class="row">
        </div>
    </div>

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplOrderPage" type="text/x-jsrender">
        <div class="col-xs-12 col-sm-4">
            <div class="order-item">
                <div>
                    <span class="order-id">订单号：{{:OrderID}}</span><span class="order-state
                {{if TradeState==1}}
                 pay-success
                {{else}}
                 not-pay
                {{/if}}
                ">{{:TradeStateText}}</span>
                </div>
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
    </script>
</asp:Content>
