<%@ Page Title="结账" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Checkout.aspx.cs" Inherits="Checkout" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/Checkout.css" rel="stylesheet" />
    <link href="Scripts/ladda/ladda-themeless.min.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="prod-items">
        </div>
        <div class="row">
            <div class="col-xs-12">
                <div class="panel panel-info">
                    <div id="divPanelHeading" class="panel-heading" onclick="wxOpenAddress();">
                        <i class="fa fa-map-marker"></i>&nbsp;&nbsp;收货人信息 >>
                    </div>
                    <div id="divWxAddrInfo" class="panel-body wx-user-addr-info">
                        <strong><span class="wx-user-name"></span></strong>，<span class="wx-tel-number"></span>
                        <br />
                        <span class="wx-user-address"></span>
                    </div>
                    <div id="divCustomizeAddrInfo" class="panel-body customize-addr-info">
                        <div class="form-group">
                            <label for="txtDeliverName" class="sr-only control-label">收货人姓名</label>
                            <input type="text" class="form-control" id="txtDeliverName" required="required" placeholder="您的姓名" />
                        </div>
                        <div class="form-group">
                            <label for="txtDeliverPhone" class="sr-only control-label">收货人电话</label>
                            <input type="text" class="form-control" id="txtDeliverPhone" required="required" placeholder="您的电话" />
                        </div>
                        <div class="form-group">
                            <label for="txtDeliverAddress" class="sr-only control-label">详细收货地址</label>
                            <textarea class="form-control" id="txtDeliverAddress" required="required" placeholder="详细收货地址"></textarea>
                        </div>
                        <%--<div class="radio">
                            <label>
                                <input type="radio" name="rdAddr" id="rdAddr1" value="上海电影集团办公楼" />
                                上海电影集团办公楼
                            </label>
                        </div>
                        <div class="radio">
                            <label>
                                <input type="radio" name="rdAddr" id="rdAddr2" value="汇智大厦" />
                                汇智大厦
                            </label>
                        </div>
                        <div class="radio">
                            <label>
                                <input type="radio" name="rdAddr" id="rdAddr3" value="新东方（漕溪北路中心）" />
                                新东方（漕溪北路中心）
                            </label>
                        </div>
                        <div class="radio">
                            <label>
                                <input type="radio" name="rdAddr" id="rdAddr4" value="复旦大学附属肿瘤医院" />
                                复旦大学附属肿瘤医院
                            </label>
                        </div>--%>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-10">
                <div class="form-group order-memo">
                    <label for="txtMemo" class="sr-only control-label">订单备注</label>
                    <textarea name="txtMemo" id="txtMemo" class="form-control" placeholder="订单备注..."></textarea>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-8 ">
                <div class="sub-price">
                    <div><i class="fa fa-check-circle"></i>&nbsp;商品价格：￥<span id="spSubTotal"></span>元</div>
                    <div><i class="fa fa-check-circle"></i>&nbsp;运费：￥<span id="spFreight"></span>元（订单满<span id="spFreightFreeCondition"></span>元包邮）</div>
                    <div class="checkbox">
                        <label>
                            <input id="cbWxCard" type="checkbox" />
                            <img src="images/wxcard.png" class="wxcard-logo" />
                        </label>
                        <span id="spWxCard" class="label label-danger"></span>
                    </div>
                    <div class="checkbox">
                        <label>
                            <input id="cbMemberPoints" type="checkbox" />
                            使用积分
                        </label>
                        <span class="form-group">
                            <label class="sr-only" for="txtUsedMemberPoints">会员积分</label>
                            <input id="txtUsedMemberPoints" type="number" class="form-control input-sm" disabled="disabled" value="0" />
                            <span id="spMemberPointsDiscount" class="label label-danger"></span>
                        </span>
                        <div class="help-block">* 您现有<span id="spValidMemberPoints"></span>积分，本订单最多可使用<span id="spMaxDiscountMemberPoints"></span>积分</div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row sub-total">
            <div class="col-xs-12">
                <div>总金额：￥<span class="sub-total-price" id="spOrderPrice"></span>元</div>
            </div>
        </div>
        <div class="row pay-button">
            <div class="col-xs-12">
                <p>
                    <button id="btnWxPay" type="button" class="btn btn-lg btn-block btn-wxpay ladda-button" data-style="zoom-in" onclick="wxPay();"><i class="fa fa-wechat fa-lg fa-fw"></i>微信支付</button>
                </p>
                <p>
                    <button id="btnAliPay" type="button" class="btn btn-lg btn-block btn-alipay ladda-button" data-style="zoom-in" onclick="aliPay();">
                        <img src="images/alipay.png" />
                        支 付 宝</button>
                </p>
                <%--                                <p>
                    <button id="btnPayCash" type="button" class="btn btn-lg btn-block btn-warning ladda-button" data-style="zoom-in" onclick="payCash();"><i class="fa fa-truck fa-lg fa-fw"></i>货到付款</button>
                </p>--%>
            </div>
        </div>
    </div>
    <div class="md-modal md-effect-9" id="divModal">
        <div class="md-content">
            <img id="imgDetailImg" src="images/SubmitOrderCompleteTip.gif" />
            <h5 class="text-center">分享给好友或朋友圈，有积分现金奖励哦！</h5>
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <script>

        //Ladda loading控制按钮
        var lBtnWxPay, lBtnPayCash, lBtnAliPay;

        requirejs(['jquery', 'ladda', 'jweixin', 'encoder'], function ($, ladda, wx) {

            $(function () {

                requirejs(['cart'], function () {
                    try {
                        if ($.cart == "undefined" || $.cart.prodAmount() == 0) {
                            alert("您的购物车空空的哦，先去买点什么吧。");
                            location.href = ".";
                        }

                        //显示购物车里的商品项
                        showProdItems();

                        //初始化购物车里的微信卡券类型枚举值
                        if (typeof wxCardType == "object") {
                            $.cart.updateWxCardType(wxCardType);
                        }
                        else {
                            throw new Error("参数错误：微信卡券类型");
                        }

                        //初始化购物车里的使用会员积分点数，默认不使用积分
                        $.cart.updateUsedMemberPoints(0);

                        //初始化购物车里的微信卡券，默认不使用优惠券
                        var wxCard = new $.cart.WxCard();
                        $.cart.updateWxCard(wxCard);

                        //初始化购物车里的运费条款
                        if (typeof freight != "undefined" && !isNaN(freight) && freight >= 0 && typeof freightFreeCondition != "undefined" && !isNaN(freightFreeCondition) && freightFreeCondition >= 0) {
                            $.cart.updateFreightTerm(freight, freightFreeCondition);
                        }
                        else {
                            throw new Error("参数错误：运费标准");
                        }

                        //校验支付方式JS全局变量
                        if (typeof paymentTerm == "undefined") {
                            throw new Error("参数错误：支付方式");
                        }

                        //校验支付宝网关JS全局变量
                        if (typeof apGateway == "undefined") {
                            throw new Error("参数错误：支付宝网关");
                        }

                        //初始化购物车里的会员积分兑换比率
                        if (typeof memberPointsExchangeRate != "undefined" && !isNaN(memberPointsExchangeRate) && memberPointsExchangeRate > 0) {
                            $.cart.updateMemberPointsExchangeRate(memberPointsExchangeRate);
                        }
                        else {
                            throw new Error("参数错误：会员积分兑换比率");
                        }

                        //初始化购物车里的会员积分余额
                        if (typeof validMemberPoints != "undefined" && !isNaN(validMemberPoints) && validMemberPoints >= 0) {
                            $.cart.updateValidMemberPoints(validMemberPoints);

                            if (validMemberPoints > 0) {
                                //注册使用积分单选框点击事件处理函数
                                $("#cbMemberPoints").on("click", switchMemberPoints);
                                //注册积分变动事件处理函数
                                $("#txtUsedMemberPoints").on({ "focus": function () { this.select(); }, "change": useMemberPoints });
                            }
                            else {
                                $("#cbMemberPoints").attr({ disabled: "disabled" });
                            }
                        }
                        else {
                            throw new Error("参数错误：会员积分余额");
                        }

                        //注册购物车的事件处理函数，用户点击使用积分事件、点击使用微信卡券事件
                        $($.cart).on({
                            "onUsedMemberPointsUpdated onWxCardUpdated": refreshOrderPrice,
                            "onUsedMemberPointsUpdated": refreshMemberPointsDiscount,
                            "onWxCardUpdated": refreshWxCardDiscount
                        });

                        //显示商品价格、运费、包邮条件、账户积分余额、订单最大可抵扣金额、订单总价
                        $("#spSubTotal").text($.cart.subTotal().toFixed(2));
                        $("#spFreight").text($.cart.calFreight());
                        $("#spFreightFreeCondition").text($.cart.freightTerm.freightFreeCondition);
                        $("#spValidMemberPoints").text(validMemberPoints);
                        $("#spMaxDiscountMemberPoints").text($.cart.getMaxDiscountMemberPoints());
                        $("#spOrderPrice").text($.cart.orderPrice().toFixed(2));

                    }
                    catch (error) {
                        alert(error.message);
                        return false;
                    }

                    //加载显示购物车里的收货人信息
                    //if ($.cart) {
                    //    var deliverInfo = $.cart.getDeliverInfo();
                    //    $("#txtDeliverName").val(deliverInfo.name);
                    //    $("#txtDeliverPhone").val(deliverInfo.phone);
                    //    if (!!deliverInfo.address) {
                    //        if (deliverInfo.address.indexOf("上海电影集团办公楼") != -1) {
                    //            $("#rdAddr1").prop("checked", "checked");
                    //            deliverInfo.address = deliverInfo.address.replace("上海电影集团办公楼，", "");
                    //        }
                    //        else {
                    //            if (deliverInfo.address.indexOf("汇智大厦") != -1) {
                    //                $("#rdAddr2").prop("checked", "checked");
                    //                deliverInfo.address = deliverInfo.address.replace("汇智大厦，", "");
                    //            }
                    //            else {
                    //                if (deliverInfo.address.indexOf("新东方（漕溪北路中心）") != -1) {
                    //                    $("#rdAddr3").prop("checked", "checked");
                    //                    deliverInfo.address = deliverInfo.address.replace("新东方（漕溪北路中心），", "");
                    //                }
                    //                else {
                    //                    if (deliverInfo.address.indexOf("复旦大学附属肿瘤医院") != -1) {
                    //                        $("#rdAddr4").prop("checked", "checked");
                    //                        deliverInfo.address = deliverInfo.address.replace("复旦大学附属肿瘤医院，", "");
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //    $("#txtDeliverAddress").val(deliverInfo.address);
                    //    $("#txtMemo").val(deliverInfo.memo);
                    //}
                });

                try {
                    //服务端生成的微信卡券JSAPI参数
                    if (typeof wxCardParam == "object" && wxCardParam.cardSign != undefined) {
                        $.extend(wxCardParam, {
                            success: getWxCardInfo,
                            fail: getWxCardInfo,
                            cancel: getWxCardInfo
                        });

                        //注册选择微信卡券点击事件处理函数
                        $("#cbWxCard").on("click", wx, wxChooseCard);
                    }
                    else {
                        throw new Error("参数错误：微信卡券参数");
                    }

                    //注册点击遮罩层关闭模式窗口事件处理函数
                    $(".md-overlay").on("click", closeModal);

                    //Ladda button
                    lBtnWxPay = ladda.create(document.querySelector('#btnWxPay'));
                    lBtnAliPay = ladda.create(document.querySelector('#btnAliPay'));
                    //lBtnPayCash = ladda.create(document.querySelector('#btnPayCash'));
                }
                catch (error) {
                    alert(error.message);
                    return false;
                }
            });
        });

        //展示购物车里的商品
        function showProdItems() {
            var htmlItem = "", groupPurchaseLabel;
            //遍历购物车，显示所有的商品项
            $.cart.getProdItems().each(function () {
                if (!!this["groupPurchase"]) {
                    //如果此商品项参加了团购，则显示团购标志
                    groupPurchaseLabel = "<span class=\"label label-warning group-purchase-label\"><i class=\"fa fa-group\"></i> 团购</span>";
                } else {
                    groupPurchaseLabel = "";
                }
                htmlItem += '<div class="row prod-item">' + groupPurchaseLabel + '<div class="col-xs-4 prod-item-left"><span class="cart-prod-img"><img src="' + this["prodImg"] + '"/></span></div>'
                    + '<div class="col-xs-6 prod-item-middle"><div class="prod-name">' + this["prodName"] + '</div><div class="prod-desc">' + this["prodDesc"] + '</div></div>'
                    + '<div class="col-xs-2 prod-item-right"><div class="prod-price">￥' + this["price"] + '</div><div class="prod-qty">x' + this["qty"] + '</div></div></div>';
            });
            $("div.prod-items").append(htmlItem);
        }

        //购物车事件处理函数，刷新界面上的订单总金额
        function refreshOrderPrice(event, data) {
            $("#spOrderPrice").text(data.orderPrice.toFixed(2));
        }

        //购物车事件处理函数，刷新界面上的积分优惠
        function refreshMemberPointsDiscount(event, data) {
            if (data.memberPointsDiscount != 0) {
                $("#spMemberPointsDiscount").text("优惠￥" + data.memberPointsDiscount.toFixed(2) + "元");
            }
            else {
                $("#spMemberPointsDiscount").text("");
            }
        }

        //购物车事件处理函数，刷新界面上的微信卡券优惠
        function refreshWxCardDiscount(event, data) {
            if (data.wxCard.title != undefined) {
                $("#spWxCard").text(data.wxCard.title);
            }
            else {
                $("#spWxCard").text("");
                $("#cbWxCard").removeAttr("checked");
                if (data.isMeetCondition != undefined && !data.isMeetCondition) {
                    alert("未达到优惠券使用条件");
                }
                if (data.isSupported != undefined && !data.isSupported) {
                    alert("只能使用微信代金券、折扣券");
                }
            }
        }

        //点击使用积分按钮时，填写当前可用的最大积分
        function switchMemberPoints(event) {
            try {
                var $txtUsedMemberPoints = $("#txtUsedMemberPoints");
                if ($("#cbMemberPoints").is(":checked")) {
                    if ($("#cbWxCard").is(":checked")) {
                        $("#cbWxCard").click();
                    }
                    var maxDiscountMemberPoints = $.cart.getMaxDiscountMemberPoints();
                    $txtUsedMemberPoints.val(maxDiscountMemberPoints);
                    $.cart.updateUsedMemberPoints(maxDiscountMemberPoints);
                    $txtUsedMemberPoints.removeAttr("disabled");
                }
                else {
                    $txtUsedMemberPoints.val(0);
                    $.cart.updateUsedMemberPoints(0);
                    $txtUsedMemberPoints.attr("disabled", "disabled");
                }
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        }

        //积分文本框change事件处理函数：校验用户输入的积分数，并更新购物车里的会员积分，触发积分变动事件，回调刷新显示订单总价格
        function useMemberPoints() {
            try {
                var $txtUsedMemberPoints = $("#txtUsedMemberPoints");
                var usedMemberPoints = parseInt($txtUsedMemberPoints.val().trim());
                var maxDiscountMemberPoints = $.cart.getMaxDiscountMemberPoints();
                if (!isNaN(usedMemberPoints) && usedMemberPoints >= 0 && usedMemberPoints <= maxDiscountMemberPoints) {
                    $.cart.updateUsedMemberPoints(usedMemberPoints);
                }
                else {
                    alert("本订单可使用的积分范围：0~" + maxDiscountMemberPoints);
                    $txtUsedMemberPoints.val(maxDiscountMemberPoints);
                    $.cart.updateUsedMemberPoints(maxDiscountMemberPoints);
                }
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        }

        //微信用户地址信息
        var wxUserName = "", wxTelNumber = "", wxAddrProvince = "", wxAddrCity = "", wxAddrCounty = "", wxAddrDetailInfo = "", wxPostalCode = "";

        //获取微信地址信息的JSSDK接口，调用微信JS函数openAddress
        function wxOpenAddress() {
            requirejs(['jweixin'], function (wx) {
                wx.openAddress({
                    success: function (res) {
                        // 用户成功拉出地址 
                        if (res.errMsg.indexOf("ok") != -1) {
                            wxUserName = res.userName;
                            wxTelNumber = res.telNumber;
                            wxAddrProvince = res.provinceName;
                            wxAddrCity = res.cityName;
                            wxAddrCounty = res.countryName;
                            wxAddrDetailInfo = res.detailInfo;
                            wxPostalCode = res.postalCode;
                            //对于直辖市，则省略省份信息
                            if (wxAddrProvince == wxAddrCity) {
                                wxAddrProvince = '';
                            }
                            if (wxPostalCode != undefined && wxPostalCode != '') {
                                wxPostalCode = "[" + wxPostalCode + "]";
                            }
                            $("span.wx-user-name").text(wxUserName);
                            $("span.wx-tel-number").text(wxTelNumber);
                            $("span.wx-user-address").text(wxAddrProvince + wxAddrCity + wxAddrCounty + wxAddrDetailInfo + wxPostalCode);
                            $("#divCustomizeAddrInfo").hide();
                            $("#divWxAddrInfo").slideDown();
                        }
                        else {
                            alert("无法获取您的地址，请手工填写收货地址。");
                            $("#divWxAddrInfo").hide();
                            $("#divCustomizeAddrInfo").slideDown();
                            console.warn(res.errMsg);
                        }
                    },
                    cancel: function () {
                        // 用户取消拉出地址
                        alert("请选择您的收货地址");
                    }
                });
            });
        }

        //[已过时]获取微信用户地址，调用微信JS内置函数editAddress
        function editAddress() {
            WeixinJSBridge.invoke(
                'editAddress',
                wxEditAddrParam,   //后端获取的参数
                function (res) {
                    //显示微信地址信息，如获取不到，则显示手工填写地址栏
                    if (res.err_msg.indexOf("ok") != -1) {
                        wxUserName = res.userName;
                        wxTelNumber = res.telNumber;
                        wxAddrProvince = res.proviceFirstStageName;
                        wxAddrCity = res.addressCitySecondStageName;
                        wxAddrCounty = res.addressCountiesThirdStageName;
                        wxAddrDetailInfo = res.addressDetailInfo;
                        wxPostalCode = res.addressPostalCode;
                        //对于直辖市，则省略省份信息
                        if (wxAddrProvince == wxAddrCity) {
                            wxAddrProvince = '';
                        }
                        if (wxPostalCode != undefined && wxPostalCode != '') {
                            wxPostalCode = "[" + wxPostalCode + "]";
                        }
                        $("span.wx-user-name").text(wxUserName);
                        $("span.wx-tel-number").text(wxTelNumber);
                        $("span.wx-user-address").text(wxAddrProvince + wxAddrCity + wxAddrCounty + wxAddrDetailInfo + wxPostalCode);
                        $("#divCustomizeAddrInfo").hide();
                        $("#divWxAddrInfo").slideDown();
                    }
                    else {
                        if (res.err_msg.indexOf("function_not_exist") != -1) {
                            alert("您的微信版本过低，请填写收货地址。");
                            $("#divWxAddrInfo").hide();
                            $("#divCustomizeAddrInfo").slideDown();
                        }
                        else {
                            if (res.err_msg.indexOf("fail") != -1 || res.err_msg.indexOf("access_denied") != -1 || res.err_msg.indexOf("unkonwPermission") != -1 || res.err_msg.indexOf("domain") != -1) {
                                alert("无法获取您的地址，请填写收货地址。");
                                $("#divWxAddrInfo").hide();
                                $("#divCustomizeAddrInfo").slideDown();
                                console.warn(res.err_msg);
                            }
                        }
                    }
                }
            );
        }

        //[已过时]调用微信用户地址信息JS-SDK
        function selectWxAddress() {
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

        //微信支付JSSDK参数，点击“微信支付”由后端生成，供前端JS拉起微信支付密码框
        var wxPayParam;

        //调用微信JS api 支付，后台调用统一下单接口生成所需参数后，在微信浏览器中调用此函数发起支付
        function onBridgeReady() {
            WeixinJSBridge.invoke(
                'getBrandWCPayRequest',
                wxPayParam,
                function (res) {
                    WeixinJSBridge.log(res.err_msg);
                    //alert(res.err_code + res.err_desc + res.err_msg);
                    if (res.err_msg.indexOf("ok") != -1) {
                        //如果购物车里的订单项有团购，则提示用户到我的订单页面去分享团购链接
                        var groupProds = "";
                        $.cart.getProdItems().each(function () {
                            if (this.groupPurchase) {
                                if (groupProds == "") {
                                    groupProds = this.groupPurchase.name;
                                } else {
                                    groupProds += "，" + this.groupPurchase.name;
                                }
                            }
                        });
                        if (groupProds != "") {
                            alert("您团购了“" + groupProds + "”，请在“我的订单”中分享给朋友、微信群、朋友圈，团购达到人数后即发货。");
                        }
                        //订单提交成功后清空购物车
                        $.cart.clearProdItems();
                        //alert("付款成功！我们将为您送货上门。");
                        //打开模式窗口，3秒后关闭
                        openModal();
                        setTimeout("closeModal();", 3000);
                    }
                    else {
                        if (res.err_msg.indexOf("cancel") != -1) {
                            alert("您取消了支付，请继续完成支付。");
                        }
                        else {
                            alert("支付失败，请重新下单。");
                        }
                    }

                    //停止按钮loading动画
                    lBtnWxPay.stop();

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

        //微信支付
        function wxPay() {
            try {
                //微信支付参数不为空场景，则说明已经统一下单且获取到了prepay_id，可直接发起微信支付
                if (wxPayParam != undefined && wxPayParam["package"] != undefined) {
                    lBtnWxPay.start();
                    JsApiPay();
                }
                else {
                    lBtnWxPay.start();

                    //使用订单收货人信息更新购物车
                    updateDeliverInfo();

                    //设置微信支付标记
                    $.cart.updatePaymentTerm(paymentTerm.wechat);

                    //根据购物车生成JSON格式订单信息，包含订单收货人和商品项信息
                    var orderInfo = $.cart.makeOrderInfo();
                    if (orderInfo != undefined) {
                        //提交订单，获取prepay_id后前台发起微信支付
                        $.ajax({
                            url: "PlaceOrder.ashx",
                            data: orderInfo,
                            type: "POST",
                            dataType: "json",
                            cache: false,
                            success: function (jWxPayParam) {
                                if (jWxPayParam["package"] != undefined)  //统一下单正常，取到了prepay_id，发起支付
                                {
                                    wxPayParam = jWxPayParam;
                                    JsApiPay();
                                }
                                else {
                                    if (jWxPayParam["return_code"] != undefined)  //可能是签名错误或参数格式错误
                                    {
                                        alert(jWxPayParam["return_msg"]);
                                    }
                                    else {
                                        if (jWxPayParam["result_code"] != undefined)  //可能是订单已支付、已关闭、订单号重复等错误
                                        {
                                            alert(jWxPayParam["err_code_des"]);
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
                }
            }
            catch (error) {
                alert(error.message);
                lBtnWxPay.stop();   //停止按钮loading动画
                return false;
            }
        }

        //支付宝
        function aliPay() {
            try {
                //微信支付参数不为空场景，则说明已经统一下单且获取到了prepay_id，必须继续完成微信支付
                if (wxPayParam != undefined && wxPayParam["package"] != undefined) {
                    throw new Error("您已微信下单，请继续完成微信支付。");
                }
                else {
                    lBtnAliPay.start();

                    //使用订单收货人信息更新购物车
                    updateDeliverInfo();

                    //设置支付宝标记
                    $.cart.updatePaymentTerm(paymentTerm.alipay);

                    //根据购物车生成JSON格式订单信息，包含订单收货人和商品项信息
                    var orderInfo = $.cart.makeOrderInfo();
                    //提交订单
                    $.ajax({
                        url: "PlaceOrder.ashx",
                        data: orderInfo,
                        type: "POST",
                        dataType: "text",
                        cache: false,
                        success: function (alipayParam) {
                            if (typeof encoder == "object" && apGateway != undefined && alipayParam != undefined && alipayParam.indexOf("sign") != -1) {
                                //后台下单成功后，清空购物车
                                $.cart.clearProdItems();
                                //后台获取到支付宝请求参数，前台再跳转到支付宝进行付款
                                var aliPayUrl = apGateway + "?" + alipayParam;
                                //加密支付宝网关地址，在重定向页面中提示用户，在外部浏览器中跳转到支付宝
                                location.href = "AliPayTip.aspx?goto=" + encoder.encode(aliPayUrl);
                            }
                            else {
                                alert("没有获取到支付宝参数");
                                lBtnAliPay.stop();
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert(errorThrown + ":" + textStatus);
                            lBtnAliPay.stop();
                        }
                    });
                }
            }
            catch (error) {
                alert(error.message);
                lBtnAliPay.stop();
                return false;
            }
        }

        //货到付款
        function payCash() {
            try {
                //微信支付参数不为空场景，则说明已经统一下单且获取到了prepay_id，必须继续完成微信支付
                if (wxPayParam != undefined && wxPayParam["package"] != undefined) {
                    throw new Error("您已微信下单，请继续完成微信支付。");
                }
                else {
                    lBtnPayCash.start();

                    //使用订单收货人信息更新购物车
                    updateDeliverInfo();

                    //设置货到付款标志
                    $.cart.updatePaymentTerm(paymentTerm.cash);

                    //根据购物车生成JSON格式订单信息，包含订购人和商品项信息
                    var orderInfo = $.cart.makeOrderInfo();

                    //提交订单
                    $.ajax({
                        url: "PlaceOrder.ashx",
                        data: orderInfo,
                        type: "POST",
                        dataType: "json",
                        cache: false,
                        success: function (jPoID) {
                            if (jPoID["NewPOID"] != undefined) {
                                //如果购物车里的订单项有团购，则提示用户到我的订单页面去分享团购链接
                                var groupProds = "";
                                $.cart.getProdItems().each(function () {
                                    if (this.groupPurchase) {
                                        if (groupProds == "") {
                                            groupProds = this.groupPurchase.name;
                                        } else {
                                            groupProds += "，" + this.groupPurchase.name;
                                        }
                                    }
                                });
                                if (groupProds != "") {
                                    alert("您团购了“{0}”，请分享您的团购链接给微信好友、群、朋友圈，团购达到人数后即发货。", groupProds);
                                }
                                $.cart.clearProdItems();
                                //打开模式窗口，3秒后关闭
                                openModal();
                                setTimeout("closeModal();", 3000);
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
            }
            catch (error) {
                alert(error.message);
                lBtnPayCash.stop();
                return false;
            }
        }

        //把订单的收货人信息写入到购物车
        function updateDeliverInfo() {
            try {
                var rdAddr = "", txtName = "", txtPhone = "", txtAddress = "", txtMemo = "";

                //判断是否弹出了手工地址栏
                if (!$("#divCustomizeAddrInfo").is(":visible")) {
                    //判断是否选择了微信用户地址
                    if (wxUserName == "" || wxTelNumber == "" || wxAddrDetailInfo == "") {
                        throw new Error("请选择收货地址。");
                    }
                    else {
                        //获取微信地址信息
                        txtName = wxUserName;
                        txtPhone = wxTelNumber;
                        txtAddress = wxAddrProvince + wxAddrCity + wxAddrCounty + wxAddrDetailInfo + wxPostalCode;
                    }
                } else {
                    //获取手工输入的地址信息
                    //rdAddr = $("#divCustomizeAddrInfo :radio:checked").val();
                    txtName = $("#txtDeliverName").val().trim();
                    txtPhone = $("#txtDeliverPhone").val().trim();
                    txtAddress = $("#txtDeliverAddress").val().trim();
                }

                if (!txtName) {
                    $("#txtDeliverName").focus();
                    throw new Error("请填写收货人姓名。");
                }
                if (!txtPhone) {
                    $("#txtDeliverPhone").focus();
                    throw new Error("请填写收货人电话。");
                }
                //if (!rdAddr) {
                //    alert("请选择收货地点。");
                //    $("#divCustomizeAddrInfo :radio:first").focus();
                //    return false;
                //}
                if (!txtAddress) {
                    $("#txtDeliverAddress").focus();
                    throw new Error("请填写详细地址。");
                }

                //订单备注信息
                txtMemo = $("#txtMemo").val().trim();

                $.cart.updateDeliverInfo(txtName, txtPhone, txtAddress, txtMemo);
            }
            catch (error) {
                throw error;
            }
        }

        //显示模式窗口
        function openModal() {
            $("#divModal").addClass("md-show");
        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");

            location.href = ".";
        }

        //使用微信JSSDK在客户端拉取微信卡券列表
        function wxChooseCard(event) {
            var wx = event.data;
            try {
                if ($(this).is(":checked")) {
                    //微信卡券和会员积分不能同时使用
                    if ($("#cbMemberPoints").is(":checked")) {
                        $("#cbMemberPoints").click();
                    }
                    //调用微信卡券JS
                    wx.chooseCard(wxCardParam);
                }
                else {
                    //取消卡券，同时更新购物车里的卡券信息
                    var wxCard = new $.cart.WxCard();
                    $.cart.updateWxCard(wxCard);
                }
            }
            catch (error) {
                alert(error.message);
            }
        }

        //根据客户端用户选择的微信卡券，向服务端查询此卡券的详细信息，并更新购物车里的卡券信息，再触发事件更新界面
        function getWxCardInfo(res) {
            try {
                if (res["errMsg"].indexOf("ok") != -1) {
                    //微信返回的是字符串，需要eval转换为对象
                    var cardList = eval(res.cardList);
                    if (Array.isArray(cardList) && cardList.length == 1 && cardList[0]["card_id"] != undefined && cardList[0]["encrypt_code"] != undefined) {
                        //根据CardID查询卡券详情信息
                        $.ajax({
                            url: "WxCard.ashx",
                            data: { "CardID": cardList[0]["card_id"] },
                            type: "GET",
                            dataType: "json",
                            cache: false,
                            success: function (jCardInfo) {
                                //把用户选择的微信卡券写入购物车
                                var wxCard = new $.cart.WxCard(jCardInfo["CardID"], cardList[0]["encrypt_code"], jCardInfo["CardType"], jCardInfo["Title"], jCardInfo["LeastCost"], jCardInfo["ReduceCost"], jCardInfo["Discount"]);
                                $.cart.updateWxCard(wxCard);
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                alert(errorThrown + ":" + textStatus);
                            }
                        });
                    }
                    else {
                        alert("没有获取到微信优惠券，请尝试刷新页面");
                    }
                }
                else {
                    //在微信卡券列表中点击取消后，iOS系统返回cancel，android系统返回fail
                    if (res["errMsg"].indexOf("cancel") != -1 || res["errMsg"].indexOf("fail") != -1) {
                        var wxCard = new $.cart.WxCard();
                        $.cart.updateWxCard(wxCard);
                    }
                }
            }
            catch (error) {
                alert(error.message);
            }
        }

    </script>
</asp:Content>
