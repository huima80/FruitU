﻿<%@ Page Title="新鲜水果切片" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="SliceList.aspx.cs" Inherits="SliceList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/FruitList.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container text-center fruit-list">
        <div class="row fruit-list text-center">
            <div class="col-xs-12">
                <img src="images/fruit-banner.gif" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/ShareIncentive.jpg" />
            </div>
        </div>
        <div id="divSlices" runat="server" class="row"></div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/TodayFruitMenu.jpg" />
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <asp:DataList ID="dlFruits" runat="server" RepeatColumns="3" RepeatDirection="Horizontal" CssClass="table">
                    <ItemTemplate>
                        <li runat="server"><%# Eval("FruitName") %></li>
                    </ItemTemplate>
                </asp:DataList>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <img src="images/FruitTip.jpg" />
            </div>
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
             <div id="divLaunchGroupEvent" class="launch-group-event">
                <button id="btnLaunchGroupEvent" class="btn btn-block btn-warning" type="button"></button>
            </div>
           <div id="btnClose" class="btn-close"><i class="fa fa-close fa-2x"></i></div>
            <div id="btnShare" class="btn-share" onclick="alert('点击右上角分享给好友或朋友圈，好友消费后有100积分(5元)奖励哦！');">分享有好礼<i class="fa fa-share-alt fa-2x"></i></div>
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplFruitPage" type="text/x-jsrender">
        <div class="col-xs-12" 
            {{if InventoryQty!=0 }}
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
        //var fruitList = [];

        //加入购物车的按钮图标
        var addToCartLabel = "<i class='fa fa-cart-plus fa-lg fa-fw'></i>";
        //发起团购活动的按钮图标
        var launchGroupEventLabel = "<i class='fa fa-group fa-lg fa-fw'></i>";

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
                    //    pageQueryCriteria: { CategoryID: 28 },
                    //    pageTemplate: '#tmplFruitPage',
                    //    pageContainer: '#divFruitList'
                    //});

                    //$($.pager).on("onPageLoaded", function (event, data) {
                    //    //设置分页数据到全局数组变量，便于前端操作
                    //    if (fruitList && Array.isArray(fruitList) && data.originalDataPerPage && Array.isArray(data.originalDataPerPage)) {
                    //        fruitList = fruitList.concat(data.originalDataPerPage);
                    //    }
                    //});

                    //$.pager.loadPage();

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

            if (sliceList && Array.isArray(sliceList)) {
                jLen = sliceList.length;
                for (var i = 0; i < jLen; i++) {
                    if (sliceList[i]["ID"] == prodID && sliceList[i]["FruitImgList"] && Array.isArray(sliceList[i]["FruitImgList"])) {
                        //查找商品详图
                        for (var j = 0; j < sliceList[i]["FruitImgList"].length; j++) {
                            if (sliceList[i]["FruitImgList"][j]["MainImg"]) {
                                mainImg = sliceList[i]["FruitImgList"][j]["ImgName"];
                            }
                            if (sliceList[i]["FruitImgList"][j]["DetailImg"]) {
                                detailImg = sliceList[i]["FruitImgList"][j]["ImgName"];
                            }
                        }

                        if (!detailImg) {
                            detailImg = webConfig.defaultImg;
                        }

                        //清空现有图片再重新加载，避免和上次图片一样时，微信不会触发img.onload事件
                        $("#imgDetailImg").attr("src", "").attr("src", "images/" + detailImg).hide();
                        $("#faLoading").show();

                        //商品库存量
                        $("#spanInventory").text(sliceList[i]["InventoryQty"] == -1 ? "不限量" : sliceList[i]["InventoryQty"]);
                        $("#hfInventory").val(sliceList[i]["InventoryQty"]);

                        //商品单价
                        $("span.prod-price").text("￥" + sliceList[i]["FruitPrice"] + "元/" + sliceList[i]["FruitUnit"]);

                        //商品购买数量默认为1
                        $("input#txtQty").val(1);

                        //去掉上次注册的按钮单击事件函数，注册新的事件函数，并传递当前选中的商品
                        $("#btnAddToCart").off("click").on("click", sliceList[i], addToCart);

                        //如果此商品支持团购，则显示团购按钮、设置按钮文字、按钮单击事件函数
                        if (!!sliceList[i]["ActiveGroupPurchase"]) {
                            $("#btnAddToCart").html(addToCartLabel + "&nbsp;单独购买");
                            $("#btnLaunchGroupEvent").html(launchGroupEventLabel + "&nbsp;团购价：" + sliceList[i]["ActiveGroupPurchase"]["GroupPrice"] + "元/" + sliceList[i]["FruitUnit"] + " " + sliceList[i]["ActiveGroupPurchase"]["RequiredNumber"] + "人团").off("click").on("click", sliceList[i], launchGroupEvent);
                            $("#divLaunchGroupEvent").show();
                        } else {
                            $("#btnAddToCart").html(addToCartLabel + "&nbsp;加入购物车");
                            $("#divLaunchGroupEvent").hide();
                        }

                        //显示模式窗口
                        $("#divModal").addClass("md-show");

                        //设置微信分享参数
                        requirejs(['jweixin'], function (wx) {
                            wxShareInfo.desc = '我买了【' + sliceList[i].FruitName + '】' + sliceList[i].FruitDesc;
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
                console.warn("var sliceList=" + sliceList);
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
                var prodItem = new $.cart.ProdItem(prod.ID, prod.FruitName, prod.FruitDesc, "images/" + mainImg, prod.FruitPrice, parseInt($("input#txtQty").val()), prod.InventoryQty, null, null);
                if ($.cart.insertProdItem(prodItem)) {
                    closeModal();
                }

            }
            else {
                alert("商品数据异常");
                console.warn("var fruitList=" + fruitList);
            }

        }

        //新发起团购活动
        function launchGroupEvent(event) {
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

                //根据商品里的团购信息构造JS团购对象
                if (!!prod.ActiveGroupPurchase) {
                    var groupPurchase = new $.cart.GroupPurchase(prod.ActiveGroupPurchase.ID, prod.ActiveGroupPurchase.Name, prod.ActiveGroupPurchase.Description, prod.ActiveGroupPurchase.StartDate, prod.ActiveGroupPurchase.EndDate, prod.ActiveGroupPurchase.RequiredNumber, prod.ActiveGroupPurchase.GroupPrice);
                    //根据商品信息构造JS商品对象，商品价格为团购价
                    var prodItem = new $.cart.ProdItem(prod.ID, prod.FruitName, prod.FruitDesc, "images/" + mainImg, groupPurchase.groupPrice, parseInt($("input#txtQty").val()), prod.InventoryQty, groupPurchase, null);
                    //把商品对象插入到购物车里
                    if ($.cart.insertProdItem(prodItem)) {
                        closeModal();
                    }
                } else {
                    alert("商品团购信息异常");
                    console.warn("prod=" + prod.ActiveGroupPurchase);
                }
            }
            else {
                alert("商品数据异常");
                console.warn("var juiceList=" + juiceList);
            }
        }

    </script>

</asp:Content>
