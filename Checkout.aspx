﻿<%@ Page Title="确定订单" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Checkout.aspx.cs" Inherits="Checkout" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/Checkout.css" rel="stylesheet" />
    <link href="css/ladda-themeless.min.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="prod-items">
        </div>
        <hr />
        <div class="row">
            <div class="col-xs-12">
                <div class="panel panel-info">
                    <div class="panel-heading" onclick="SelectDeliverAddress();">
                        <i class="fa fa-map-marker"></i>&nbsp;&nbsp;选择收货地址 >>
                    </div>
                    <div id="divWxAddrInfo" class="panel-body wx-user-addr-info">
                        <strong><span class="wx-user-name"></span></strong>，<span class="wx-tel-number"></span>
                        <br />
                        <span class="wx-user-address"></span>
                    </div>
                    <div id="divCustomizeAddrInfo" class="panel-body customize-addr-info">
                        <div class="form-group">
                            <label for="txtDeliverName" class="sr-only control-label">您的姓名</label>
                            <input type="text" class="form-control" id="txtDeliverName" required="required" placeholder="您的姓名" />
                        </div>
                        <div class="form-group">
                            <label for="txtDeliverPhone" class="sr-only control-label">您的电话</label>
                            <input type="text" class="form-control" id="txtDeliverPhone" required="required" placeholder="您的电话" />
                        </div>
                        <div class="form-group">
                            <label for="txtDeliverAddress" class="sr-only control-label">收货地址</label>
                            <textarea class="form-control" id="txtDeliverAddress" required="required" placeholder="收货地址"></textarea>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <hr />
        <div class="row">
            <div class="col-sm-10">
                <div class="form-group order-memo">
                    <label for="txtMemo" class="sr-only control-label">订单备注</label>
                    <textarea name="txtMemo" id="txtMemo" class="form-control" placeholder="订单备注..."></textarea>
                </div>
            </div>
        </div>
        <hr />
        <div class="row sub-total">
            <div class="col-xs-12">
                <span>总价：<span class="sub-total-price" id="spSubTotalPrice"></span></span>
            </div>
        </div>
        <div class="row pay-button">
            <div class="col-xs-12">
                <p>
                    <button id="btnWxPay" type="button" class="btn btn-lg btn-block btn-wxpay ladda-button" data-style="zoom-in" onclick="makeOrder(true);"><i class="fa fa-wechat fa-lg fa-fw"></i>微信支付</button>
                </p>
                <p>
                    <button id="btnPayCash" type="button" class="btn btn-lg btn-block btn-info ladda-button" data-style="zoom-in" onclick="makeOrder(false);"><i class="fa fa-truck fa-lg fa-fw"></i>货到付款</button>
                </p>
            </div>
        </div>
    </div>

    <script>

        //Ladda loading控制按钮
        var lBtnWxPay, lBtnPayCash;

        requirejs(['jquery', 'ladda'], function ($, ladda) {

            $(function () {

                requirejs(['cart'], function () {
                    displayCart();
                });

                lBtnWxPay = ladda.create(document.querySelector('#btnWxPay'));
                lBtnPayCash = ladda.create(document.querySelector('#btnPayCash'));

            });
        });

        //展示购物车里的商品
        function displayCart() {
            var htmlItem = "";

            //遍历购物车，显示所有的商品项
            $.cart.getProdItems().each(function () {
                htmlItem += '<div class="row prod-item"><div class="col-xs-4 prod-item-left"><span class="cart-prod-img"><img src="' + this["prodImg"] + '"/></span></div>'
                    + '<div class="col-xs-6 prod-item-middle"><div class="prod-name">' + this["prodName"] + '</div><div class="prod-desc">' + this["prodDesc"] + '</div></div>'
                    + '<div class="col-xs-2 prod-item-right"><div class="prod-price">￥' + this["price"] + '</div><div class="prod-qty">x' + this["qty"] + '</div></div></div>';
            });

            $("div.prod-items").append(htmlItem);

            //显示总价格
            $("#spSubTotalPrice").text("￥" + $.cart.subTotal());

        }

        //微信用户地址信息
        var wxUserName = "", wxTelNumber = "", wxAddrProvince = "", wxAddrCity = "", wxAddrCounty = "", wxAddrDetailInfo = "", wxPostalCode = "";

        //获取微信用户地址
        function editAddress() {
            WeixinJSBridge.invoke(
                'editAddress',
                wxEditAddrParam,   //后端获取的参数
                  function (res) {
                      if (res.err_msg.indexOf("ok") != -1) {
                          wxUserName = res.userName;
                          wxTelNumber = res.telNumber;
                          wxAddrProvince = res.proviceFirstStageName;
                          wxAddrCity = res.addressCitySecondStageName;
                          wxAddrCounty = res.addressCountiesThirdStageName;
                          wxAddrDetailInfo = res.addressDetailInfo;
                          wxPostalCode = res.addressPostalCode;
                          $("span.wx-user-name").text(wxUserName);
                          $("span.wx-tel-number").text(wxTelNumber);
                          $("span.wx-user-address").text(wxAddrProvince + wxAddrCity + wxAddrCounty + wxAddrDetailInfo + "[" + wxPostalCode + "]");
                          $("#divCustomizeAddrInfo").hide();
                          $("#divWxAddrInfo").slideDown();
                      }
                      else {
                          if (res.err_msg.indexOf("function_not_exist") != -1) {
                              alert("您的微信版本过低，请升级到最新版，也可以手工填写收货地址。");
                              $("#divWxAddrInfo").hide();
                              $("#divCustomizeAddrInfo").slideDown();
                          }
                          else {
                              if (res.err_msg.indexOf("fail") != -1 || res.err_msg.indexOf("access_denied") != -1 || res.err_msg.indexOf("unkonwPermission") != -1 || res.err_msg.indexOf("domain") != -1) {
                                  alert("无法获取您的地址，请手工填写收货地址。");
                                  $("#divWxAddrInfo").hide();
                                  $("#divCustomizeAddrInfo").slideDown();
                                  console.log(res.err_msg);
                              }
                          }
                      }
                  }
              );
        }

        function SelectDeliverAddress() {
            if (typeof WeixinJSBridge == "undefined") {
                if (document.addEventListener) {
                    document.addEventListener('WeixinJSBridgeReady', editAddress, false);
                }
                else if (document.attachEvent) {
                    document.attachEvent('WeixinJSBridgeReady', editAddress);
                    document.attachEvent('onWeixinJSBridgeReady', editAddress);
                }
            }
            else {
                editAddress();
            }
        }

        var wxJsApiParam = "";

        //调用微信JS api 支付，后台调用统一下单接口生成所需参数后，在微信浏览器中调用此函数发起支付
        function onBridgeReady() {
            WeixinJSBridge.invoke(
                'getBrandWCPayRequest',
                 wxJsApiParam,
                 function (res) {
                     WeixinJSBridge.log(res.err_msg);
                     //alert(res.err_code + res.err_desc + res.err_msg);
                     if (res.err_msg.indexOf("ok") != -1) {
                         //订单提交成功后清空购物车，返回首页
                         $.cart.clearProdItems();
                         alert("付款成功！我们将为您送货上门。");
                         location.href = ".";
                     }
                     else {
                         if (res.err_msg.indexOf("cancel") != -1) {
                             alert("您取消了支付，请继续完成支付。");
                         }
                         else {
                             alert("支付失败，请重新下单。");
                         }
                     }
                     lBtnWxPay.stop();   //停止按钮loading动画

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
        function makeOrder(wxPay) {

            //JS支付参数不为空场景，则说明已经统一下单且获取到了prepay_id，可直接发起支付
            if (wxJsApiParam != "" && wxJsApiParam["package"] != undefined) {

                //已经生成了JS支付参数，但没有输入密码就取消了支付，之后用户再次点击微信支付按钮的场景
                if (wxPay == true) {
                    lBtnWxPay.start();
                    JsApiPay();
                } else {
                    //用户点击过微信支付，订单已入库，且获取了prepay_id，但取消了输入密码，又点击了货到付款的场景，引导用户继续完成微信支付
                    alert("您已微信下单，请继续完成微信支付。");
                }
            }
            else {
                //JS支付参数为空场景

                //判断购物车里的商品项是否为空
                if ($.cart != undefined && $.cart.prodAmount() != 0) {


                    ////////////////处理订单收货人信息////////////////////
                    var txtName = "", txtPhone = "", txtAddress = "", txtMemo = "";

                    //判断是否选择了微信用户地址
                    if (wxUserName == "" || wxTelNumber == "" || wxAddrDetailInfo == "") {

                        //判断是否弹出了手工地址栏
                        if ($("#divCustomizeAddrInfo").is(":visible")) {
                            //获取手工输入的地址信息
                            txtName = $("#txtDeliverName").val().trim();
                            txtPhone = $("#txtDeliverPhone").val().trim();
                            txtAddress = $("#txtDeliverAddress").val().trim();
                            if (txtName == "") {
                                alert("请填写收货人姓名。");
                                $("#txtDeliverName").focus();
                                return false;
                            }
                            if (txtPhone == "") {
                                alert("请填写收货人电话。");
                                $("#txtDeliverPhone").focus();
                                return false;
                            }
                            if (txtAddress == "") {
                                alert("请填写收货人地址。");
                                $("#txtDeliverAddress").focus();
                                return false;
                            }
                        } else {
                            alert("请选择收货地址。");
                            return false;
                        }
                    }
                    else {
                        //获取微信地址信息
                        txtName = wxUserName;
                        txtPhone = wxTelNumber;
                        txtAddress = wxAddrProvince + wxAddrCity + wxAddrCounty + wxAddrDetailInfo + "[" + wxPostalCode + "]";
                    }
                    //订单备注信息
                    txtMemo = $("#txtMemo").val().trim();

                    ////////////////处理订单收货人信息////////////////////


                    ////////////////提交订单////////////////////
                    var orderInfo;
                    if (wxPay)  //选择微信支付
                    {
                        lBtnWxPay.start();

                        //使用订单收货人信息更新购物车
                        $.cart.updateDeliverInfo(txtName, txtPhone, txtAddress, txtMemo, 1);

                        //根据购物车生成JSON格式订单信息，包含订单收货人和商品项信息
                        orderInfo = $.cart.makeOrderInfo();

                        //提交订单，获取prepay_id后前台发起微信支付
                        $.ajax({
                            url: "JSAPIPay.ashx",
                            data: orderInfo,
                            type: "POST",
                            dataType: "json",
                            success: function (jWxJsParam) {
                                if (jWxJsParam["package"] != undefined)  //统一下单正常，取到了prepay_id，发起支付
                                {
                                    wxJsApiParam = jWxJsParam;
                                    JsApiPay();
                                }
                                else {
                                    if (jWxJsParam["return_code"] != undefined)  //可能是签名错误或参数格式错误
                                    {
                                        alert(jWxJsParam["return_msg"]);
                                    }
                                    else {
                                        if (jWxJsParam["result_code"] != undefined)  //可能是订单已支付、已关闭、订单号重复等错误
                                        {
                                            alert(jWxJsParam["err_code_des"]);
                                        }
                                    }

                                    lBtnWxPay.stop();   //停止按钮loading动画

                                }

                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert(errorThrown + ":" + textStatus);
                                lBtnWxPay.stop();   //停止按钮loading动画
                            }
                        });
                    }
                    else {  //选择货到付款

                        lBtnPayCash.start();

                        $.cart.updateDeliverInfo(txtName, txtPhone, txtAddress, txtMemo, 2);

                        //根据购物车生成JSON格式订单信息，包含订购人和商品项信息
                        orderInfo = $.cart.makeOrderInfo();

                        //提交订单
                        $.ajax({
                            url: "JSAPIPay.ashx",
                            data: orderInfo,
                            type: "POST",
                            dataType: "json",
                            success: function (jPoID) {
                                if (jPoID["NewPoID"] != undefined) {
                                    alert("下单成功！我们将为您送货上门收款。");
                                    $.cart.clearProdItems();
                                    location.href = ".";
                                }
                                else {
                                    if (jPoID["result_code"] != undefined)  //提交值校验错误
                                    {
                                        alert(jPoID["err_code_des"]);
                                    }
                                }

                                lBtnPayCash.stop();

                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert(errorThrown + ":" + textStatus);
                                lBtnPayCash.stop();
                            }
                        });
                    }
                    ////////////////提交订单////////////////////


                }
                else {
                    alert("您的购物车里没有商品哦，请先选购吧。");
                    location.href = ".";
                }
            }
        }

    </script>
</asp:Content>
