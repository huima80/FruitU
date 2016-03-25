﻿<%@ Page Title="鲜榨果汁" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="JuiceList.aspx.cs" Inherits="JuiceList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/JuiceList.css" rel="stylesheet" />
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
        </div>
        <div id="divJuiceList" class="row">
        </div>
    </div>

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplJuicePage" type="text/x-jsrender">
        <div class="col-xs-6" onclick="addToCart({{:ID}},'{{:FruitName}}','{{:FruitDesc}}',{{:FruitPrice}});">
            {{for FruitImgList}}
                    {{if DetailImg}}
               <img src="images/{{:ImgName}}" alt="{{:ImgDesc}}" />
            {{/if}}   
                {{/for}}            
        </div>
    </script>

    <script>
        requirejs(['jquery'], function ($) {
            $(function () {
                requirejs(['pager'], function () {

                    $.pager.init({
                        pagerMode: 1,
                        pageSize: 10,
                        pageQueryURL: 'ProdListPager.ashx',
                        pageQueryCriteria: { CategoryID: 1 },
                        pageTemplate: '#tmplJuicePage',
                        pageContainer: '#divJuiceList',
                        //isMasonry: true
                    });

                    $.pager.loadPage();

                });

            });
        });

        function addToCart(prodID, prodName, prodDesc, price) {

            var $div = $(event.currentTarget);
            var prodImg = $div.find("img").attr("src");

            //购物车里添加商品
            $.cart.insertProdItem(prodID, prodName, prodDesc, prodImg, price, 1);

        }

    </script>
</asp:Content>
