<%@ Page Title="购物车" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="MyCart.aspx.cs" Inherits="MyCart" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/MyCart.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
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

    <script>
        requirejs(['jquery'], function ($) {

            $(function () {

                requirejs(['cart'], function () {
                    displayCart();

                    //挂接购物车商品数量变动后事件处理函数
                    $($.cart).on("onProdItemUpdated onProdItemDeleted", refreshSubTotal);
                });
            });
        });

        //刷新购物车里的商品总价、结算按钮状态
        function refreshSubTotal(event, data) {

            $("span.sub-total-price").text("￥" + data.subTotal);

            //如果购物车为空，则禁用结算按钮
            if (data.prodQty == 0) {
                $("#btnBuynow").attr({ disabled: "disabled" });
                $("div.prod-items").html('<div class="cart-empty-hint">带点新鲜的走吧！</div>');
            }
        }

        //展示购物车里的商品
        function displayCart() {
            var htmlItem = "";

            //遍历购物车，显示所有的商品项
            $.cart.getProdItems().each(function () {
                htmlItem += '<div class="row prod-item" id="ProdItem' + this["prodID"] + '"><div class="col-xs-5 prod-item-left"><i class="fa fa-minus-square-o fa-lg remove-prod-item" onclick="removeProdItem(' + this["prodID"] + ');"></i><span class="cart-prod-img"><img src="' + this["prodImg"] + '"/></span></div>'
                    + '<div class="col-xs-7 prod-item-right"><div class="prod-name">' + this["prodName"] + '</div><div class="prod-desc">' + this["prodDesc"] + '</div><div class="prod-price">￥' + this["price"] + '</div>'
                    + '<div><span class="input-group"><span id="btnDec" class="input-group-addon" onclick="decQty(' + this["prodID"] + ');">-</span><input class="form-control" type="text" id="txtQty' + this["prodID"] + '" value="' + this["qty"] + '" onchange="checkIsNaN(' + this["prodID"] + ');"/><span id="btnInc" class="input-group-addon" onclick="incQty(' + this["prodID"] + ');">+</span></span></div></div></div>';
            });

            $("div.prod-items").append(htmlItem);

            refreshSubTotal(null, { prodQty: $.cart.prodAmount(), subTotal: $.cart.subTotal() });
        }

        //递增商品数量
        function incQty(prodId) {
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
            $.cart.updateProdItem(prodId, currQty);
        }

        //递减商品数量
        function decQty(prodId) {
            var $txtQty = $("#txtQty" + prodId);
            if (!isNaN($txtQty.val())) {
                //当前数量递减
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
            $.cart.updateProdItem(prodId, currQty);
        }

        //校验是否输入数值
        function checkIsNaN(prodId) {
            var $txtQty = $("#txtQty" + prodId);

            //如果输入不是数值，则修改数量为1
            if (isNaN($txtQty.val())) {
                $txtQty.val(1);

                //更新购物车里的商品数量
                $.cart.updateProdItem(prodId, 1);
            }
        }

        //移除商品项
        function removeProdItem(prodId) {

            //删除购物车界面商品项
            $("#ProdItem" + prodId).fadeOut("slow");

            //删除购物车里的商品项
            $.cart.deleteProdItem(prodId);

        }

    </script>
</asp:Content>
