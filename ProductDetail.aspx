<%@ Page Title="商品详情" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="ProductDetail.aspx.cs" Inherits="ProductDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/jquery-ui-1.11.4.css" rel="stylesheet" />
    <link href="css/ProductDetail.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-xs-12">
                <div id="jssorSliderContainer" class="center-block" style="position: relative; top: 0px; left: 0px; min-width: 100%; height: 150px;">
                    <!-- Slides Container -->
                    <div id="divSlides" runat="server" u="slides" style="cursor: grab; position: absolute; overflow: hidden; left: 0px; top: 0px; min-width: 100%; height: 150px;"></div>
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

    <script>
        requirejs(['jquery', 'jqueryui', 'jssorslider', 'webConfig', 'cart'], function ($) {
            $(function () {

                //轮播图
                var jssor_slider1 = new $JssorSlider$('jssorSliderContainer',
                    {
                        $AutoPlay: true,
                        $ThumbnailNavigatorOptions: true,
                        $FillMode: 2,
                    });

                //tab页插件
                $("#tabs").tabs();

                $('#btnAddCart').on('click', addToCart);

                $('#btnBuynow').on('click', function () {
                    addToCart();
                    location.href = "Checkout.aspx";
                });

                //requirejs(['cart'], function () {
                //    //挂接购物车商品数量变动前的事件处理函数
                //    $($.cart).on("onProdItemInserting", flyToCart);
                //});

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

            //加入购物车
            function addToCart() {
                if (prodInfo) {
                    //购物车里添加商品
                    $.cart.insertProdItem(prodInfo.prodID, prodInfo.prodName, prodInfo.prodDesc, "images/" + prodInfo.prodImg, prodInfo.price, $("#txtQty").val());
                }
                else {
                    alert("获取商品信息失败");
                }
            }

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

    <!-- 搜狐畅言 -->
    <script src="Scripts/sohucs.js"></script>
    <script type="text/javascript" src="http://assets.changyan.sohu.com/upload/plugins/plugins.count.js">
    </script>
</asp:Content>
