<%@ Page Title="精选水果礼盒" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="FruitGift.aspx.cs" Inherits="FruitGift" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/FruitList.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
    <style>
        area {
            outline: none;
        }

        .sell-out-198 {
            position: absolute;
            top: 82px;
            left: 65px;
        }

        .sell-out-298 {
            position: absolute;
            top: 82px;
            left: 198px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container text-center fruit-list">
        <div class="row fruit-list text-center">
            <div class="col-xs-12">
                <img src="images/FruitGift_01.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/FruitGift_02.gif" usemap="#buyButton" hidefocus="true" />
                <span id="spanSellout198" runat="server" class="label label-danger sell-out-198">今日售罄</span>
                <span id="spanSellout298" runat="server" class="label label-danger sell-out-298">今日售罄</span>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/FruitGift_03.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/FruitGift_04.gif" />
            </div>
        </div>
    </div>
    <map name="buyButton" id="buyButton">
        <area runat="server" id="gift198" shape="rect" coords="70,80,200,260" href="" alt="购买" />
        <area runat="server" id="gift298" shape="rect" coords="200,80,340,310" href="" alt="购买" />
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
            <div id="btnShare" class="btn-share" onclick="alert('点击右上角分享给好友或朋友圈，好友消费后有100积分(5元)奖励哦！');">分享有好礼<i class="fa fa-share-alt fa-2x"></i></div>
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->
    <script>

        requirejs(['jquery'], function ($) {
            $(function () {
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

        //根据传入的商品ID，在全局数组中查找对应商品项，并设置modal窗口中的图片src和数量框
        function openModal(prodID) {
            var mainImg, detailImg, jLen;

            if (fruitGiftList && Array.isArray(fruitGiftList)) {
                jLen = fruitGiftList.length;
                for (var i = 0; i < jLen; i++) {
                    if (fruitGiftList[i]["ID"] == prodID && fruitGiftList[i]["FruitImgList"] && Array.isArray(fruitGiftList[i]["FruitImgList"])) {
                        //查找商品详图
                        for (var j = 0; j < fruitGiftList[i]["FruitImgList"].length; j++) {
                            if (fruitGiftList[i]["FruitImgList"][j]["MainImg"]) {
                                mainImg = fruitGiftList[i]["FruitImgList"][j]["ImgName"];
                            }
                            if (fruitGiftList[i]["FruitImgList"][j]["DetailImg"]) {
                                detailImg = fruitGiftList[i]["FruitImgList"][j]["ImgName"];
                            }
                        }

                        if (!detailImg) {
                            detailImg = webConfig.defaultImg;
                        }

                        //清空现有图片再重新加载，避免和上次图片一样时，微信不会触发img.onload事件
                        $("#imgDetailImg").attr("src", "").attr("src", "images/" + detailImg).hide();
                        $("#faLoading").show();

                        //商品库存量
                        $("#spanInventory").text(fruitGiftList[i]["InventoryQty"] == -1 ? "不限量" : fruitGiftList[i]["InventoryQty"]);
                        $("#hfInventory").val(fruitGiftList[i]["InventoryQty"]);

                        //商品单价
                        $("span.prod-price").text("￥" + fruitGiftList[i]["FruitPrice"] + "元/" + fruitGiftList[i]["FruitUnit"]);

                        //商品购买数量默认为1
                        $("input#txtQty").val(1);

                        //去掉上次注册的按钮单击事件函数，注册新的事件函数，并传递当前选中的商品
                        $("#btnAddToCart").off("click").on("click", fruitGiftList[i], addToCart);

                        //显示模式窗口
                        $("#divModal").addClass("md-show");

                        //设置微信分享参数
                        requirejs(['jweixin110'], function (wx) {
                            wxShareInfo.desc = '我买了【' + fruitGiftList[i].FruitName + '】' + fruitGiftList[i].FruitDesc;
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

                        break;
                    }
                }
            }
            else {
                alert("商品数据异常");
                console.warn("var fruitGiftList=" + fruitGiftList);
            }
        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");
        }

        //加入购物车
        function addToCart(event) {
            var mainImg;
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
                console.warn("var fruitList=" + fruitList);
            }

        }
        
    </script>
</asp:Content>

