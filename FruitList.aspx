<%@ Page Title="新鲜水果切片" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="FruitList.aspx.cs" Inherits="FruitList" %>

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
            <div class="col-xs-12">
                <img src="images/delivery-area.gif" />
            </div>
       </div>
        <div id="divFruitList" class="row">
        </div>
    </div>
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

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplFruitPage" type="text/x-jsrender">
        <div class="col-xs-12" onclick="openModal({{:ID}});">
            {{for FruitImgList}}
                    {{if MainImg}}
               <img src="images/{{:ImgName}}" alt="{{:ImgDesc}}" />
            {{/if}}   
                {{/for}}            
        </div>
    </script>

    <script>
        var fruitList = [];

        requirejs(['jquery', 'webConfig'], function ($) {
            $(function () {
                requirejs(['pager', 'cart'], function () {

                    $.pager.init({
                        pagerMode: 1,
                        pageSize: 10,
                        pageQueryURL: 'ProdListPager.ashx',
                        pageQueryCriteria: { CategoryID: 28 },
                        pageTemplate: '#tmplFruitPage',
                        pageContainer: '#divFruitList'
                    });

                    $($.pager).on("onPageLoaded", function (event, data) {
                        //设置分页数据到全局数组变量，便于前端操作
                        if (fruitList && Array.isArray(fruitList) && data.originalDataPerPage && Array.isArray(data.originalDataPerPage)) {
                            fruitList = fruitList.concat(data.originalDataPerPage);
                        }
                    });

                    $.pager.loadPage();

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

            if (fruitList && Array.isArray(fruitList)) {
                jLen = fruitList.length;
                for (var i = 0; i < jLen; i++) {
                    if (fruitList[i]["ID"] == prodID && fruitList[i]["FruitImgList"] && Array.isArray(fruitList[i]["FruitImgList"])) {
                        //查找商品详图
                        for (var j = 0; j < fruitList[i]["FruitImgList"].length; j++) {
                            if (fruitList[i]["FruitImgList"][j]["DetailImg"]) {
                                detailImg = fruitList[i]["FruitImgList"][j]["ImgName"];
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
                console.warn("var fruitList=" + fruitList);
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
            var prodID, mainImg, qty, jLen;
            prodID = $("#btnAddToCart").data("prodid");

            if (prodID && fruitList && Array.isArray(fruitList)) {
                jLen = fruitList.length;
                for (var i = 0; i < jLen; i++) {
                    if (fruitList[i]["ID"] == prodID && fruitList[i]["FruitImgList"] && Array.isArray(fruitList[i]["FruitImgList"])) {
                        //查找商品主图
                        for (var j = 0; j < fruitList[i]["FruitImgList"].length; j++) {
                            if (fruitList[i]["FruitImgList"][j]["MainImg"]) {
                                mainImg = fruitList[i]["FruitImgList"][j]["ImgName"];
                                break;
                            }
                        }

                        if (!mainImg) {
                            mainImg = webConfig.defaultImg;
                        }

                        //商品数量
                        qty = $("input#txtQty").val();

                        //购物车里添加商品
                        $.cart.insertProdItem(prodID, fruitList[i]["FruitName"], fruitList[i]["FruitDesc"], "images/" + mainImg, fruitList[i]["FruitPrice"], qty);

                        break;
                    }
                }
            }
            else {
                alert("商品数据异常");
                console.warn("var fruitList=" + fruitList);
            }

            closeModal();
        }

    </script>

</asp:Content>
