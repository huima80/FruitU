<%@ Page Title="撞果运" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="RandomJuice.aspx.cs" Inherits="RandomJuice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/RandomJuice.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container text-center random-category">
        <div class="row">
            <div class="col-xs-12">
                <img src="images/random-banner.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img id="imgRandomJuice" src="images/selective-barrier.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/delivery-area.gif" />
            </div>
        </div>
    </div>
    <map name="buyButton" id="buyButton">
        <area shape="rect" coords="" href="" alt="购买" />
    </map>
    <div class="md-modal md-effect-3" id="divModal">
        <div class="md-content">
            <img id="imgDetailImg" src="" />
            <i id="faLoading" class="fa fa-refresh fa-spin"></i>
            <div class="prod-info"><i class="fa fa-check-circle"></i>&nbsp;单价：<span class="prod-price"></span>&nbsp;&nbsp;<i class="fa fa-check-circle"></i>&nbsp;库存数：<span id="spanInventory" class="inventory"></span><input type="hidden" id="hfInventory" /></div>
            <hr />
            <div>
                <label class="sr-only" for="txtQty">购买数量</label>
                <span class="input-group">
                    <span id="btnDesc" class="input-group-addon">-</span>
                    <input class="form-control" type="text" id="txtQty" value="1" />
                    <span id="btnAsc" class="input-group-addon">+</span>
                </span>
                <div class="add-to-cart-button">
                    <button id="btnAddToCart" class="btn btn-danger" type="button"><i class="fa fa-cart-plus fa-lg fa-fw"></i>加入购物车</button>
                </div>
            </div>
            <div id="btnClose" class="btn-close"><i class="fa fa-close fa-2x"></i></div>
            <div id="btnShare" class="btn-share" onclick="alert('点击右上角分享给好友或朋友圈，好友消费后有100积分(5元)奖励哦！');">分享有好礼！<i class="fa fa-share-alt fa-2x"></i></div>
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <script>
        requirejs(['jquery'], function ($) {
            $(function () {
                $("#imgRandomJuice").on("click", randomJuice);

                requirejs(['cart'], function () {
                    //超出库存事件处理函数
                    $($.cart).on("onOutOfStock", function (event, data) {
                        alert("您购买的数量超过库存数了哦。");
                    });
                });
            });

            //递减商品数量
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
                }
                else {
                    currQty = 1;
                }

                //显示更新的数量
                $("#txtQty").val(currQty);

            });

            //递增商品数量
            $("#btnAsc").on("click", function () {
                var currQty = $("#txtQty").val();
                var inventory = $("#hfInventory").val();
                if (!isNaN(currQty)) {
                    currQty = parseInt(currQty);
                    inventory = parseInt(inventory);
                    if (inventory == -1 || currQty < inventory) {
                        currQty++;
                    }
                }
                else {
                    currQty = 1;
                }

                //显示更新的数量
                $("#txtQty").val(currQty);

            });

            //校验是否输入数值
            $('#txtQty').on('change', function () {
                var currQty = $("#txtQty").val();
                if (isNaN(currQty)) {
                    $("#txtQty").val(1);
                }
            });

            $(".md-overlay").on("click", closeModal);

            $("#btnClose").on("click", function () {
                closeModal();
                event.stopPropagation();
            });

            $("#imgDetailImg").on("load", function () {
                $("#faLoading").hide();
                $("#imgDetailImg").show();
            });

        });

        //在所有商品中随机选一个
        function randomJuice() {
            var ri, thisImg = this, mainImg, jLen;

            if (juiceList && Array.isArray(juiceList) && juiceList.length > 0) {
                jLen = juiceList.length;

                do {
                    //在商品数组中随机选择
                    ri = Math.floor(Math.random() * jLen);
                } while (!juiceList[ri]);

                //查找随机商品的主图
                if (juiceList[ri]["FruitImgList"] && Array.isArray(juiceList[ri]["FruitImgList"])) {
                    for (var j = 0; j < juiceList[ri]["FruitImgList"].length; j++) {
                        if (juiceList[ri]["FruitImgList"][j]["MainImg"]) {
                            mainImg = juiceList[ri]["FruitImgList"][j]["ImgName"];
                            break;
                        }
                    }

                    if (!mainImg) {
                        mainImg = webConfig.defaultImg;
                    }
                }

                //动画替换商品图片
                $(thisImg).animate(
                    { opacity: 0.3 },
                    {
                        duration: "fast",
                        complete: function () {

                            //用随机选出的图片替换当前图片，并设置热点
                            $(thisImg).attr({
                                "src": "images/" + mainImg,
                                "usemap": "#buyButton"
                            }).ready(function () {
                                var imgWidth, imgHeight, x, y, coords;
                                imgWidth = $(thisImg).width();
                                imgHeight = $(thisImg).height();
                                x = (imgWidth * 3 / 4).toFixed(0);
                                y = (imgHeight * 3 / 4).toFixed(0);
                                coords = x + "," + y + "," + imgWidth + "," + imgHeight;
                                $("map#buyButton area").attr({
                                    "coords": coords,
                                    "href": "javascript:openModal(juiceList[" + ri + "]);"
                                });
                            }).animate(
                                { opacity: 1 },
                                { duration: "fast" });
                        }
                    }
                );
            }
            else {
                alert("商品数据异常");
                console.warn("var juiceList=" + juiceList);
            }
        }

        //根据选中的商品，设置modal窗口中的图片src、库存数、单价、购买数量，注册购买按钮单击事件函数
        function openModal(prod) {
            var mainImg, detailImg;
            //去掉上次注册的按钮单击事件函数，注册新的事件函数，并传递当前选中的商品
            $("#btnAddToCart").off("click").on("click", prod, addToCart);

            //查找商品详图
            for (var i = 0; i < prod.FruitImgList.length; i++) {
                if (prod.FruitImgList[i]["MainImg"]) {
                    mainImg = prod.FruitImgList[i]["ImgName"];
                }
                if (prod.FruitImgList[i]["DetailImg"]) {
                    detailImg = prod.FruitImgList[i]["ImgName"];
                }
            }

            if (!detailImg) {
                detailImg = webConfig.defaultImg;
            }

            //清空现有图片再重新加载，避免和上次图片一样时，微信不会触发img.onload事件
            $("#imgDetailImg").attr("src", "").attr("src", "images/" + detailImg).hide();
            $("#faLoading").show();

            //商品库存量
            $("#spanInventory").text(prod.InventoryQty == -1 ? "不限量" : prod.InventoryQty);
            $("#hfInventory").val(prod.InventoryQty);

            //商品单价
            $("span.prod-price").text("￥" + prod.FruitPrice + "元/" + prod.FruitUnit);

            //默认购买数量默认为1
            $("input#txtQty").val(1);

            //显示模式窗口
            $("#divModal").addClass("md-show");

            //设置微信分享参数
            requirejs(['jweixin'], function (wx) {
                wxShareInfo.desc = '我买了【' + prod.FruitName + '】' + prod.FruitDesc;
                wxShareInfo.link = location.href + '?AgentOpenID=' + openID;
                wxShareInfo.imgUrl = location.origin + '/images/' + mainImg;

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

        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");
        }

        //加入购物车
        function addToCart(event) {
            var prod, mainImg;
            prod = event.data;

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

                //购物车里添加商品
                var prodItem = new $.cart.ProdItem(prod.ID, prod.FruitName, prod.FruitDesc, "images/" + mainImg, prod.FruitPrice, parseInt($("input#txtQty").val()), prod.InventoryQty);
                if ($.cart.insertProdItem(prodItem)) {
                    closeModal();
                }
            }
            else {
                alert("商品数据异常");
                console.warn("var juiceList=" + fruitList);
            }

        }

    </script>
</asp:Content>

