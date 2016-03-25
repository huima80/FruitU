<%@ Page Title="新鲜水果切片" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="FruitList.aspx.cs" Inherits="FruitList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/FruitList.css" rel="stylesheet" />
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

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplFruitPage" type="text/x-jsrender">
        <div class="col-xs-12" onclick="addToCart({{:ID}},'{{:FruitName}}','{{:FruitDesc}}',{{:FruitPrice}});">
            {{for FruitImgList}}
                    {{if MainImg}}
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
                        pageQueryCriteria: { CategoryID: 28 },
                        pageTemplate: '#tmplFruitPage',
                        pageContainer: '#divFruitList'
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
