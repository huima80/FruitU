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
            <div class="col-xs-12">
                <img src="images/delivery-area.gif" />
            </div>
            <%--            <div class="col-xs-12">
                <img src="images/buy2get1free.jpg" />
            </div>--%>
        </div>
        <div id="divJuiceList" class="row">
        </div>
    </div>
    <div class="md-modal md-effect-3" id="divModal">
        <div class="md-content">
            <img id="imgDetailImg" src="" />
            <div>
                 <div class="prod-info"><i class="fa fa-check-circle"></i>&nbsp;单价：<span class="prod-price"></span>&nbsp;&nbsp;<i class="fa fa-check-circle"></i>&nbsp;库存数：<span id="spanInventory" class="inventory"></span><input type="hidden" id="hfInventory" /></div>
                <hr />
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
                {{if TopSellingOnWeek}}
                    <span class="top-selling-week-prod"><i class="fa fa-trophy fa-lg"></i>本周爆款</span>
            {{/if}}
            {{if InventoryQty==0}}
                    <span class="sell-out">已售罄</span>
            {{/if}}
        </div>
    </script>

    <script>
        //存放分页获取的所有数据
        var juiceList = [];

        requirejs(['jquery'], function ($) {
            $(function () {
                requirejs(['pager', 'cart'], function () {

                    //超出库存事件处理函数
                    $($.cart).on("onOutOfStock", function (event, data) {
                        alert("您已购买最大库存数了哦。");
                    });

                    $.pager.init({
                        pagerMode: 1,
                        pageSize: 10,
                        pageQueryURL: 'ProdListPager.ashx',
                        pageQueryCriteria: { CategoryID: 1 },
                        pageTemplate: '#tmplJuicePage',
                        pageContainer: '#divJuiceList'
                    });

                    $($.pager).on("onPageLoaded", function (event, data) {
                        //设置分页数据到全局数组变量，便于前端操作
                        if (juiceList && Array.isArray(juiceList) && data.originalDataPerPage && Array.isArray(data.originalDataPerPage)) {
                            juiceList = juiceList.concat(data.originalDataPerPage);
                        }
                    });

                    $.pager.loadPage();

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

                $("#imgDetailImg").on("load", switchModalShow);

            });
        });

        //根据传入的商品ID，在全局数组中查找对应商品项，并设置modal窗口中的图片src和数量框
        function openModal(prodID) {
            var detailImg, jLen;

            //把当前商品ProdID放入btn按钮
            $("#btnAddToCart").data("prodid", prodID);

            if (juiceList && Array.isArray(juiceList)) {
                jLen = juiceList.length;

                for (var i = 0; i < jLen; i++) {
                    //查找用户选择的商品ID
                    if (juiceList[i]["ID"] == prodID && juiceList[i]["FruitImgList"] && Array.isArray(juiceList[i]["FruitImgList"])) {
                        //查找此商品的详图
                        for (var j = 0; j < juiceList[i]["FruitImgList"].length; j++) {
                            if (juiceList[i]["FruitImgList"][j]["DetailImg"]) {
                                detailImg = juiceList[i]["FruitImgList"][j]["ImgName"];
                                break;
                            }
                        }

                        if (!detailImg) {
                            detailImg = webConfig.defaultImg;
                        }

                        //商品库存量
                        $("#spanInventory").text(juiceList[i]["InventoryQty"] == -1 ? "无限量" : juiceList[i]["InventoryQty"]);
                        $("#hfInventory").val(juiceList[i]["InventoryQty"]);

                        //商品单价
                        $("span.prod-price").text("￥" + juiceList[i]["FruitPrice"] + "元/" + juiceList[i]["FruitUnit"]);

                        //商品购买数量
                        $("input#txtQty").val(1);

                        //清空现有图片再重新加载，避免和上次图片一样时，微信不会触发img.onload事件
                        $("#imgDetailImg").attr("src", "").attr("src", "images/" + detailImg);

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
        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");
        }

        //加入购物车
        function addToCart() {
            var prodID, mainImg, jLen;
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

                        //购物车里添加商品
                        var prodItem = new $.cart.ProdItem(prodID, juiceList[i]["FruitName"], juiceList[i]["FruitDesc"], "images/" + mainImg, juiceList[i]["FruitPrice"], parseInt($("input#txtQty").val()), juiceList[i]["InventoryQty"]);
                        $.cart.insertProdItem(prodItem);
                        
                        break;
                    }
                }
            }
            else {
                alert("商品数据异常");
                console.warn("var juiceList=" + juiceList);
            }

            closeModal();
        }

    </script>

</asp:Content>

