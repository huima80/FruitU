<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MyCart.aspx.cs" Inherits="MyCart" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>购物车</title>
    <link rel="shortcut icon" href="images/FruitU.ico" type="image/x-icon" />
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="keywords" content="fruit,slice,juice,水果,果汁,切片" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="black" />
    <meta name="format-detection" content="telephone=no" />
    <meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1.0, maximum-scale=1.0" />
    <link href="http://apps.bdimg.com/libs/bootstrap/3.3.0/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/font-awesome.min.css" rel="stylesheet" />
    <link href="css/common.css" rel="stylesheet" />
    <link href="css/MyCart.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="prod-items">
            </div>
            <div class="row cart-footer">
                <div class="col-xs-12 sub-total">
                    总计：<span class="sub-total-price"></span>
                    <button class="btn btn-danger" type="button" id="btnBuynow" onclick="location.href='Checkout.aspx'"><i class="fa fa-shopping-cart fa-lg fa-fw"></i>去结算</button>
                </div>
            </div>
        </div>
        <!-- #include file="footer.html" -->
    </form>
</body>

<script>
    $(function () {

        displayCart();

        //挂接购物车商品数量变动后事件处理函数
        $(cart).on("prodItemsChanged", refreshSubTotal);

    });

    function refreshSubTotal(event, data) {

        //显示购物车里的总价
        $("span.sub-total-price").text("￥" + data.subTotal);

        //如果购物车为空，则禁用结算按钮
        if (data.prodQty == 0) {
            $("#btnBuynow").attr({ disabled: "disabled" });
            $("div.container").html('<div class="cart-empty-hint">购物车空空如也 :-(</div>');
        }
    }

    //展示购物车里的商品
    function displayCart() {
        var htmlItem = "";

        //遍历购物车，显示所有的商品项
        cart.getProdItems().each(function () {
            htmlItem += '<div class="row prod-item" id="ProdItem' + this["prodID"] + '"><div class="col-xs-5 prod-item-left"><i class="fa fa-minus-square-o fa-lg remove-prod-item" onclick="removeProdItem(' + this["prodID"] + ');"></i><span class="cart-prod-img"><img src="' + this["prodImg"] + '"/></span></div>'
                + '<div class="col-xs-7 prod-item-right"><div class="prod-name">' + this["prodName"] + '</div><div class="prod-desc">' + this["prodDesc"] + '</div><div class="prod-price">￥' + this["price"] + '</div>'
                + '<div><span class="input-group"><span id="btnDec" class="input-group-addon" onclick="DecQty(' + this["prodID"] + ');">-</span><input class="form-control" type="text" id="txtQty' + this["prodID"] + '" value="' + this["qty"] + '" onchange="CheckIsNaN(' + this["prodID"] + ');"/><span id="btnInc" class="input-group-addon" onclick="IncQty(' + this["prodID"] + ');">+</span></span></div></div></div>';
        });

        $("div.prod-items").append($(htmlItem));

        refreshSubTotal(null, { prodQty: cart.prodAmount(), subTotal: cart.subTotal() });
    }

    //递增商品数量
    function IncQty(prodId) {
        var $txtQty = $("#txtQty" + prodId);
        if (!isNaN($txtQty.val())) {
            //当前数量递增
            var currQty = parseInt($txtQty.val());
            currQty++;

            //显示更新的数量
            $txtQty.val(currQty);
        }
        else {
            currQty = 1;
            $txtQty.val(currQty);
        }
        //更新购物车里的商品数量
        cart.updateProdItem(prodId, currQty);
    }

    //递减商品数量
    function DecQty(prodId) {
        var $txtQty = $("#txtQty" + prodId);
        if (!isNaN($txtQty.val())) {
            //当前数量递增
            var currQty = parseInt($txtQty.val());

            if (currQty > 1) {
                currQty--;
            }
            else {
                currQty = 1;
            }

            //显示更新的数量
            $txtQty.val(currQty);
        }
        else {
            currQty = 1;
            $txtQty.val(currQty);
        }
        //更新购物车里的商品数量
        cart.updateProdItem(prodId, currQty);
    }

    //校验是否输入数值
    function CheckIsNaN(prodId) {
        var $txtQty = $("#txtQty" + prodId);
        if (isNaN($txtQty.val())) {
            $txtQty.val(1);

            //更新购物车里的商品数量
            cart.updateProdItem(prodId, 1);
        }
    }

    //移除商品项
    function removeProdItem(prodId) {

        //删除购物车界面商品项
        $("#ProdItem" + prodId).fadeOut("slow");

        //删除购物车里的商品项
        cart.removeProdItem(prodId);

    }

</script>
</html>
