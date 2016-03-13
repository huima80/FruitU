<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ProductDetail.aspx.cs" Inherits="ProductDetail" %>
<%@OutputCache Duration="60" VaryByParam="ProdID" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>商品详情</title>
    <link rel="shortcut icon" href="images/FruitU.ico" type="image/x-icon" />
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="keywords" content="fruit,slice,juice,水果,果汁,切片" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="black" />
    <meta name="format-detection" content="telephone=no" />
    <meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1.0, maximum-scale=1.0" />
    <link href="css/jquery-ui-1.11.4.css" rel="stylesheet" />
    <link href="css/bootstrap.min-3.3.5.css" rel="stylesheet" />
    <link href="css/font-awesome.min.css" rel="stylesheet" />
    <link href="css/flexslider.css" rel="stylesheet" media="screen" />
    <link href="css/common.css" rel="stylesheet" />
    <link href="css/ProductDetail.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="row">
                <div class="col-xs-12">
                    <div class="flexslider">
                        <ul class="slides" id="ulFlexSlider" runat="server">
                        </ul>
                    </div>
                </div>
            </div>
            <div class="row prod-detail">
                <div class="col-xs-12 form-group">
                    <div>
                        <asp:Label ID="lblStickyProd" runat="server" Text="" CssClass="sticky-prod"></asp:Label>
                        <asp:Label ID="lblTopSelling" runat="server" Text="" CssClass="top-selling-month-prod"></asp:Label>
                        <asp:Label ID="lblProdName" runat="server" Text="" CssClass="prod-name" ClientIDMode="Static"></asp:Label>
                    </div>
                    <div class="prod-desc">
                        <asp:Label ID="lblProdDesc" runat="server" Text="" ClientIDMode="Static"></asp:Label>
                    </div>
                    <div>
                        ￥<asp:Label CssClass="prod-price" ID="lblProdPrice" runat="server" ClientIDMode="Static"></asp:Label>
                        <asp:Label CssClass="prod-unit" ID="lblProdUnit" runat="server" Text=""></asp:Label>
                        <label class="sr-only" for="txtQty">购买数量</label>
                        <span class="input-group">
                            <span id="btnDesc" class="input-group-addon">-</span>
                            <input class="form-control" type="text" id="txtQty" value="1" />
                            <span id="btnAsc" class="input-group-addon">+</span>
                        </span>
                    </div>
                    <div>
                        <asp:Label CssClass="original-price" ID="lblOriginalPrice" runat="server" Text=""></asp:Label><br />
                        <asp:Label CssClass="sales-volume" ID="lblSalesVolume" runat="server" Text=""></asp:Label>
                    </div>
                    <div class="buy-button">
                        <button class="btn btn-warning add-cart" type="button" id="btnAddCart" runat="server"><i class="fa fa-cart-plus fa-lg fa-fw"></i>加入购物车</button>
                        <button class="btn btn-danger buynow" type="button" id="btnBuynow" runat="server">立即购买</button>
                    </div>
                    <asp:Label CssClass="prod-state" ID="lblProdState" runat="server" Text=""></asp:Label>
                    <asp:HiddenField ID="hfProdID" runat="server" ClientIDMode="Static" />
                </div>
            </div>
            <div id="tabs">
                <ul>
                    <li><a href="#tabs-1">商品详情</a></li>
                    <li><a href="#tabs-2">网友热评</a><a href="#SOHUCS" class="badge" id="changyan_count_unit"></a></li>
                </ul>
                <div id="tabs-1" class="detail-img">
                    <div class="row">
                        <div class="col-xs-12" runat="server" id="divDetailImg">
                        </div>
                    </div>
                </div>
                <div id="tabs-2">
                    <div id="SOHUCS" runat="server"></div>
                </div>
            </div>
        </div>
        <!-- #include file="footer.html" -->
    </form>

    <!-- 搜狐畅言 -->
    <script src="Scripts/sohucs.js"></script>
    <script type="text/javascript" src="http://assets.changyan.sohu.com/upload/plugins/plugins.count.js">
    </script>

    <script>
        requirejs(['flexslider', 'jqueryui', 'webConfig'], function () {
            $(function () {

                //轮播图
                $('.flexslider').flexslider({
                    animation: "slide"
                });

                //tab页插件
                $("#tabs").tabs();

                requirejs(['cart'], function () {
                    //挂接购物车商品数量变动前的事件处理函数
                    $($.cart).on("onProdItemsChanging", flyToCart);
                });

            });

            //递减果汁数量
            $("#btnDesc").on("click", function () {
                var currQty = $("#txtQty").val();
                if (!isNaN(currQty)) {
                    currQty = parseInt(currQty);
                    if (currQty > 1) {
                        currQty--;
                    }
                    else {
                        currQty = 1;
                    }
                    //显示更新的数量
                    $("#txtQty").val(currQty);
                }
                else {
                    $("#txtQty").val(1);
                }
            });

            //递增果汁数量
            $("#btnAsc").on("click", function () {
                var currQty = $("#txtQty").val();
                if (!isNaN(currQty)) {
                    currQty = parseInt(currQty);
                    currQty++;

                    //显示更新的数量
                    $("#txtQty").val(currQty);
                }
                else {
                    $("#txtQty").val(1);
                }
            });

            //校验是否输入数值
            $('#txtQty').on('change', function () {
                var currQty = $("#txtQty").val();
                if (isNaN(currQty)) {
                    $("#txtQty").val(1);
                }
            });

            $('#btnAddCart').on('click', function () {

                var $prodImg = $("#ulFlexSlider img:first");
                var imgName = $prodImg.attr("src");

                //购物车里添加商品
                $.cart.addProdItem($("#hfProdID").val(), $("#lblProdName").text(), $("#lblProdDesc").text(), imgName, $("#lblProdPrice").text(), $("#txtQty").val());

            });

            $('#btnBuynow').on('click', function () {

                var $prodImg = $("#ulFlexSlider img:first");
                var imgName = $prodImg.attr("src");

                //购物车里添加商品
                $.cart.addProdItem($("#hfProdID").val(), $("#lblProdName").text(), $("#lblProdDesc").text(), imgName, $("#lblProdPrice").text(), $("#txtQty").val());

                location.href = "Checkout.aspx";
            });

            //添加商品到购物车的动画
            function flyToCart(event) {
                var $cartIcon = $(".fa-shopping-cart");
                var $imgFly = $("#ulFlexSlider img:first");
                var $beginPos = $("div.flexslider");
                if ($imgFly) {
                    var $imgClone = $imgFly.clone().offset({
                        top: $beginPos.offset().top + $imgFly.height() * 1 / 3,
                        left: $beginPos.offset().left + $imgFly.width() * 1 / 3
                    }).css({
                        'opacity': '0.5',
                        'position': 'absolute',
                        'height': '150px',
                        'width': '150px',
                        'z-index': parseInt($("footer.footer").css("z-index")) + 1000
                    }).appendTo($('body')).animate({
                        'top': (webConfig.browserVersion.wechat ? ($cartIcon.offset().top + $(document).scrollTop()) : $cartIcon.offset().top),
                        'left': $cartIcon.offset().left + $cartIcon.width() / 2,
                        'width': 75,
                        'height': 75
                    }, 1000, 'swing', function () {
                        //$cartIcon.addClass("shake-run");
                        //setTimeout(function () {
                        //    $cartIcon.removeClass("shake-run");
                        //}, 500);

                        $imgClone.animate({
                            'width': 0,
                            'height': 0
                        }, function () {
                            $(this).detach();
                        });

                    });

                }
            };
        });

    </script>
</body>
</html>
