<%@ Page Title="商品详情" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="ProductDetail.aspx.cs" Inherits="ProductDetail" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/jquery-ui-1.11.4.css" rel="stylesheet" />
    <link href="css/ProductDetail.css" rel="stylesheet" />
    <!--搜狐畅言PC和WAP自适应版-->
    <script type="text/javascript"> 
        (function () {
            var appid = 'cysdSoonk';
            var conf = '5dda3cb88b38b86854c484d8ce91b27c';
            var width = window.innerWidth || document.documentElement.clientWidth;
            if (width < 960) {
                window.document.write('<script id="changyan_mobile_js" charset="utf-8" type="text/javascript" src="https://changyan.sohu.com/upload/mobile/wap-js/changyan_mobile.js?client_id=' + appid + '&conf=' + conf + '"><\/script>');
            } else { var loadJs = function (d, a) { var c = document.getElementsByTagName("head")[0] || document.head || document.documentElement; var b = document.createElement("script"); b.setAttribute("type", "text/javascript"); b.setAttribute("charset", "UTF-8"); b.setAttribute("src", d); if (typeof a === "function") { if (window.attachEvent) { b.onreadystatechange = function () { var e = b.readyState; if (e === "loaded" || e === "complete") { b.onreadystatechange = null; a() } } } else { b.onload = a } } c.appendChild(b) }; loadJs("https://changyan.sohu.com/upload/changyan.js", function () { window.changyan.api.config({ appid: appid, conf: conf }) }); }
        })(); </script>
    <script type="text/javascript" src="https://assets.changyan.sohu.com/upload/plugins/plugins.count.js">
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-xs-12">
                <asp:Image ID="imgMainImg" runat="server" />
                <%--                <div id="jssorSliderContainer" class="center-block" style="position: relative; top: 0px; left: 0px; min-width: 100%; height: 150px;">
                    <!-- Slides Container -->
                    <div id="divSlides" runat="server" u="slides" style="cursor: grab; position: absolute; overflow: hidden; left: 0px; top: 0px; min-width: 100%; height: 150px;"></div>
                </div>--%>
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
                    <asp:Label CssClass="sales-volume" ID="lblSalesVolume" runat="server" Text="" Visible="false"></asp:Label>
                </div>
                <div class="buy-button">
                    <button id="btnLaunchGroupEvent" class="btn btn-warning" type="button" runat="server"></button>
                    <button id="btnAddToCart" class="btn btn-danger add-cart" type="button" runat="server"></button>
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
        requirejs(['jquery', 'jweixin', 'jqueryui', 'jssorslider', 'cart'], function ($, wx) {
            $(function () {

                //轮播图
                //var jssor_slider1 = new $JssorSlider$('jssorSliderContainer',
                //    {
                //        $AutoPlay: true,
                //        $ThumbnailNavigatorOptions: true,
                //        $FillMode: 2,
                //    });

                $("#btnAddToCart").on("click", addToCart);
                $("#btnLaunchGroupEvent").on("click", launchGroupEvent);

                //tab页插件
                $("#tabs").tabs();

                //超出库存事件处理函数
                $($.cart).on("onOutOfStock", function (event, data) {
                    alert("您购买的数量超过库存数了哦。");
                });

                //requirejs(['cart'], function () {
                //    //挂接购物车商品数量变动前的事件处理函数
                //    $($.cart).on("onProdItemInserting", flyToCart);
                //});


            });

            //设置微信分享参数
            try {
                if (typeof wxShareInfo == "undefined") {
                    throw new Error("微信分享参数错误");
                }

                //查找此商品的主图
                var mainImg = webConfig.defaultImg;
                for (var i = 0; i < prod.FruitImgList.length; i++) {
                    if (prod.FruitImgList[i]["MainImg"]) {
                        mainImg = prod.FruitImgList[i]["ImgName"];
                    }
                }

                //设置微信分享参数
                wxShareInfo.desc = '我买了【' + prod.FruitName + '】' + prod.FruitDesc;
                wxShareInfo.link = location.href + '&AgentOpenID=' + openID;
                wxShareInfo.imgUrl = location.origin + '/images/' + mainImg;

                wx.ready(function () {
                    //分享到朋友圈
                    wx.onMenuShareTimeline(wxShareInfo);

                    //分享给朋友
                    wx.onMenuShareAppMessage($.extend({}, wxShareInfo, { type: 'link', dataUrl: '' }));

                    //分享到QQ
                    wx.onMenuShareQQ(wxShareInfo);

                    //分享到腾讯微博
                    wx.onMenuShareWeibo(wxShareInfo);

                    //分享到QQ空间
                    wx.onMenuShareQZone(wxShareInfo);
                });

            } catch (error) {
                alert(error.message);
                return false;
            }

            //递减数量
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

            //递增数量
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
                if (prod) {
                    //查找商品主图
                    for (var i = 0; i < prod.FruitImgList.length; i++) {
                        if (prod.FruitImgList[i]["MainImg"]) {
                            mainImg = prod.FruitImgList[i]["ImgName"];
                            break;
                        }
                    }

                    if (!mainImg) {
                        mainImg = webConfig.defaultImg;
                    }

                    var prodItem = new $.cart.ProdItem(prod.ID, prod.FruitName, prod.FruitDesc, "images/" + mainImg, prod.FruitPrice, parseInt($("input#txtQty").val()), prod.InventoryQty, null, null);
                    //购物车里添加商品
                    $.cart.insertProdItem(prodItem);
                }
                else {
                    alert("商品数据异常");
                    console.warn("var prod=" + prod);
                }
            }

            //新发起团购活动
            function launchGroupEvent() {
                if (prod) {
                    //查找商品主图
                    for (var i = 0; i < prod.FruitImgList.length; i++) {
                        if (prod.FruitImgList[i]["MainImg"]) {
                            mainImg = prod.FruitImgList[i]["ImgName"];
                            break;
                        }
                    }

                    if (!mainImg) {
                        mainImg = webConfig.defaultImg;
                    }

                    //根据商品里的团购信息构造JS团购对象
                    if (!!prod.ActiveGroupPurchase) {
                        var groupPurchase = new $.cart.GroupPurchase(prod.ActiveGroupPurchase.ID, prod.ActiveGroupPurchase.Name, prod.ActiveGroupPurchase.Description, prod.ActiveGroupPurchase.StartDate, prod.ActiveGroupPurchase.EndDate, prod.ActiveGroupPurchase.RequiredNumber, prod.ActiveGroupPurchase.GroupPrice);
                        //根据商品信息构造JS商品对象，商品价格为团购价
                        var prodItem = new $.cart.ProdItem(prod.ID, prod.FruitName, prod.FruitDesc, "images/" + mainImg, groupPurchase.groupPrice, parseInt($("input#txtQty").val()), prod.InventoryQty, groupPurchase, null);
                        //把商品对象插入到购物车里
                        $.cart.insertProdItem(prodItem);
                    } else {
                        alert("商品团购信息异常");
                        console.warn("prod=" + prod.ActiveGroupPurchase);
                    }
                }
                else {
                    alert("商品数据异常");
                    console.warn("var prod=" + prod);
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
</asp:Content>
