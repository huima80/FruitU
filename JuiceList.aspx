<%@ Page Title="鲜榨果汁" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="JuiceList.aspx.cs" Inherits="JuiceList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/JuiceList.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container text-center juice-list">
        <div class="row">
            <div class="col-xs-12">
                <img src="images/juice-banner.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/99DeliveryFree.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/ShareIncentive.jpg" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/FreshJuiceBanner.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/FreshJuice.gif" />
            </div>
        </div>
        <div runat="server" id="divFreshJuiceList" class="row">
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/Juice-AllFruitJuice.gif" />
            </div>
        </div>
        <div runat="server" id="divAllFruitJuiceList" class="row">
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/Juice-FruitVeggiesJuice.gif" />
            </div>
        </div>
        <div runat="server" id="divFruitVeggiesJuiceList" class="row">
        </div>
       <div class="row">
            <div class="col-xs-12">
                <img src="images/delivery_area_tip.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/delivery-area.gif" />
            </div>
        </div>
         <div class="row">
            <div class="col-xs-12">
                <img src="images/delivery_area_vip.gif" />
            </div>
        </div>
   </div>
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

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplJuicePage" type="text/x-jsrender">
        <div class="col-xs-6" 
            {{if InventoryQty!=0}}
                onclick="openModal({{:ID}});"
            {{/if}}
            >
            {{for FruitImgList}}
                {{if MainImg}}
               <img src="images/{{:ImgName}}" alt="{{:ImgDesc}}" />
            {{/if}}   
                {{/for}}        
                {{if TopSellingWeekly}}
                    <span class="top-selling-week-prod"><i class="fa fa-trophy fa-lg"></i>本周爆款</span>
            {{/if}}
            {{if InventoryQty==0}}
                    <span class="sell-out">今日售罄</span>
            {{/if}}
        </div>
    </script>

    <script>
        //存放分页获取的所有数据
        //var juiceList = [];

        requirejs(['jquery'], function ($) {
            $(function () {
                requirejs(['cart'], function () {

                    //超出库存事件处理函数
                    $($.cart).on("onOutOfStock", function (event, data) {
                        alert("您购买的数量超过库存数了哦。");
                    });

                    //$.pager.init({
                    //    pagerMode: 1,
                    //    pageSize: 10,
                    //    pageQueryURL: 'ProdListPager.ashx',
                    //    pageQueryCriteria: { CategoryID: 1 },
                    //    pageTemplate: '#tmplJuicePage',
                    //    pageContainer: '#divJuiceList'
                    //});

                    //$($.pager).on("onPageLoaded", function (event, data) {
                    //    //设置分页数据到全局数组变量，便于前端操作
                    //    if (juiceList && Array.isArray(juiceList) && data.originalDataPerPage && Array.isArray(data.originalDataPerPage)) {
                    //        juiceList = juiceList.concat(data.originalDataPerPage);
                    //    }
                    //});

                    //$.pager.loadPage();

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
        });

        //根据传入的商品ID，在全局数组中查找对应商品项，并设置modal窗口中的图片src和数量框
        function openModal(prodID) {
            var mainImg, detailImg, jLen;

            if (juiceList && Array.isArray(juiceList)) {
                jLen = juiceList.length;

                for (var i = 0; i < jLen; i++) {
                    //查找用户选择的商品ID
                    if (juiceList[i]["ID"] == prodID && juiceList[i]["FruitImgList"] && Array.isArray(juiceList[i]["FruitImgList"])) {
                        //查找此商品的详图
                        for (var j = 0; j < juiceList[i]["FruitImgList"].length; j++) {
                            if (juiceList[i]["FruitImgList"][j]["MainImg"]) {
                                mainImg = juiceList[i]["FruitImgList"][j]["ImgName"];
                            }
                            if (juiceList[i]["FruitImgList"][j]["DetailImg"]) {
                                detailImg = juiceList[i]["FruitImgList"][j]["ImgName"];
                            }
                        }

                        if (!detailImg) {
                            detailImg = webConfig.defaultImg;
                        }

                        //清空现有图片再重新加载，避免和上次图片一样时，微信不会触发img.onload事件
                        $("#imgDetailImg").attr("src", "").attr("src", "images/" + detailImg).hide();
                        $("#faLoading").show();

                        //商品库存量
                        $("#spanInventory").text(juiceList[i]["InventoryQty"] == -1 ? "不限量" : juiceList[i]["InventoryQty"]);
                        $("#hfInventory").val(juiceList[i]["InventoryQty"]);

                        //商品单价
                        $("span.prod-price").text("￥" + juiceList[i]["FruitPrice"] + "元/" + juiceList[i]["FruitUnit"]);

                        //商品购买数量默认为1
                        $("input#txtQty").val(1);

                        //解除上次注册的按钮单击事件函数，注册新的事件函数，并传递当前选中的商品
                        $("#btnAddToCart").off("click").on("click", juiceList[i], addToCart);

                        //显示模式窗口
                        $("#divModal").addClass("md-show");

                        //设置微信分享参数
                        requirejs(['jweixin110'], function (wx) {
                            wxShareInfo.desc = '我买了【' + juiceList[i].FruitName + '】' + juiceList[i].FruitDesc;
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
                console.warn("var juiceList=" + juiceList);
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
                console.warn("var juiceList=" + juiceList);
            }

        }

    </script>

</asp:Content>

