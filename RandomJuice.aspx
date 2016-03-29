<%@ Page Title="撞果运" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="RandomJuice.aspx.cs" Inherits="RandomJuice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/RandomJuice.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container text-center">
        <div class="row random-category">
            <div class="col-xs-12">
                <img src="images/random-banner.gif" />
            </div>
            <div class="col-xs-12">
                <img id="imgRandomJuice" src="images/selective-barrier.gif" />
            </div>
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
            <div>
                <label class="sr-only" for="txtQty">购买数量</label>
                <span class="input-group">
                    <span id="btnDesc" class="input-group-addon">-</span>
                    <input class="form-control" type="text" id="txtQty" value="1" />
                    <span id="btnAsc" class="input-group-addon">+</span>
                </span>
                <button id="btnAddToCart" class="btn btn-danger" type="button" data-prodid="" onclick="addToCart();"><i class="fa fa-cart-plus fa-lg fa-fw"></i>加入购物车</button>
            </div>
            <div id="btnClose" class="btn-close"><i class="fa fa-close fa-3x"></i></div>
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <script>
        requirejs(['jquery'], function ($) {
            $(function () {
                $("#imgRandomJuice").on("click", randomJuice);
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
                    //显示更新的数量
                    $("#txtQty").val(currQty);
                }
                else {
                    $("#txtQty").val(1);
                }
            });

            //递增商品数量
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

            $(".md-overlay").on("click", closeModal);

            $("#btnClose").on("click", function () {
                closeModal();
                event.stopPropagation();
            });

            $("#imgDetailImg").on("load", switchModalShow);

        });

        //在所有商品中随机选一个，并淡入
        function randomJuice() {
            var ri, thisImg = this, mainImg, jLen;

            if (juiceList && Array.isArray(juiceList)) {
                jLen = juiceList.length;

                do {
                    //在商品数组中随机选择
                    ri = Math.floor(Math.random() * jLen);
                } while (!juiceList[ri])

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
                                    "href": "javascript:openModal(juiceList[" + ri + "].ID);"
                                });
                            }).animate(
                                { opacity: 1 },
                                { duration: "fast" });
                        }
                    }
                );
            }
        }

        //根据传入的商品ID，在全局数组中查找对应商品项，并设置modal窗口中的图片src和数量框
        function openModal(prodID) {
            var detailImg, jLen;
            //把当前商品ProdID放入btn按钮
            $("#btnAddToCart").data("prodid", prodID);

            if (juiceList && Array.isArray(juiceList)) {
                jLen = juiceList.length;
                for (var i = 0; i < jLen; i++) {
                    if (juiceList[i]["ID"] == prodID && juiceList[i]["FruitImgList"] && Array.isArray(juiceList[i]["FruitImgList"])) {
                        //查找商品详图
                        for (var j = 0; j < juiceList[i]["FruitImgList"].length; j++) {
                            if (juiceList[i]["FruitImgList"][j]["DetailImg"]) {
                                detailImg = juiceList[i]["FruitImgList"][j]["ImgName"];
                                break;
                            }
                        }

                        if (!detailImg) {
                            detailImg = webConfig.defaultImg;
                        }

                        //清空现有图片再重新加载，避免和上次图片一样时，微信不会触发img.onload事件
                        $("#imgDetailImg").attr("src", "").attr("src", "images/" + detailImg);

                        $("input#txtQty").val(1);

                        break;
                    }
                }
            }
            else {
                alert("商品数据异常");
                console.warn("var juiceList=" + juiceList);
            }
        }

        //显示模式窗口，在图片load事件完成后回调
        function switchModalShow() {
            $("#divModal").addClass("md-show");
            if ($(event.target).hasClass("md-setperspective")) {
                setTimeout(function () {
                    $(document).addClass("md-perspective");
                }, 25);
            }
        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");

            if ($(event.target).hasClass("md-setperspective")) {
                $(document).removeClass("md-perspective");
            }
        }

        //加入购物车
        function addToCart() {
            var prodID, mainImg, qty, jLen;
            prodID = $("#btnAddToCart").data("prodid");

            if (prodID && juiceList && Array.isArray(juiceList)) {
                jLen = juiceList.length;

                for (var i = 0; i < jLen; i++) {
                    if (juiceList[i]["ID"] == prodID && juiceList[i]["FruitImgList"] && Array.isArray(juiceList[i]["FruitImgList"])) {
                        //查找商品主图
                        for (var j = 0; j < juiceList[i]["FruitImgList"].length; j++) {
                            if (juiceList[i]["FruitImgList"][j]["MainImg"]) {
                                mainImg = juiceList[i]["FruitImgList"][j]["ImgName"];
                                break;
                            }
                        }

                        if (!mainImg) {
                            mainImg = webConfig.defaultImg;
                        }

                        //商品数量
                        qty = $("input#txtQty").val();

                        //购物车里添加商品
                        $.cart.insertProdItem(prodID, juiceList[i]["FruitName"], juiceList[i]["FruitDesc"], "images/" + mainImg, juiceList[i]["FruitPrice"], qty);

                        break;
                    }
                }
            }
            else {
                alert("商品数据异常");
                console.warn("var juiceList=" + juiceList);
            }
        }

    </script>
</asp:Content>

