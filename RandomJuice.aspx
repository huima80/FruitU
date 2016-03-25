<%@ Page Title="撞果运" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="RandomJuice.aspx.cs" Inherits="RandomJuice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/RandomJuice.css" rel="stylesheet" />
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
    <script>
        requirejs(['jquery', 'jssorslider'], function ($) {
            $(function () {
                $("#imgRandomJuice").on("click", randomJuice);

            });
        });

        //在所有商品中随机选一个，并淡入
        function randomJuice() {
            var ri, thisImg = this;
            
            if (juiceList) {
                do {
                    //在商品数组中随机选择
                    ri = Math.floor(Math.random() * juiceList.length);
                } while (!juiceList[ri])

                $(thisImg).animate(
                    { opacity: 0.3 },
                    {
                        duration: "fast",
                        complete: function () {

                            //用随机选出的图片替换当前图片
                            $(thisImg).attr({
                                "src": "images/" + juiceList[ri].prodImg,
                                "usemap": "#buyButton"
                            }).ready(function () {
                                //根据当前图片设置热点坐标
                                var imgWidth, imgHeight, x, y, coords;
                                imgWidth = $(thisImg).width();
                                imgHeight = $(thisImg).height();
                                x = (imgWidth * 3 / 4).toFixed(0);
                                y = (imgHeight * 3 / 4).toFixed(0);
                                coords = x + "," + y + "," + imgWidth + "," + imgHeight;
                                $("map#buyButton area").attr({
                                    "coords": coords,
                                    "href": "javascript:addToCart(juiceList[" + ri + "]);"
                                });
                            });

                            //淡出新图片
                            $(thisImg).animate(
                                { opacity: 1 },
                                { duration: "fast" });
                        }
                    }
                );
            }
        }

        //把商品加入购物车
        function addToCart(prod) {
            if (prod) {
                //购物车里添加商品
                $.cart.insertProdItem(prod.prodID, prod.prodName, prod.prodDesc, "images/" + prod.prodImg, prod.price, 1);
            }
        }

    </script>
</asp:Content>

