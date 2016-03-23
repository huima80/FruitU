<%@ Page Title="撞果运" Language="C#" MasterPageFile="~/FruitU.master" AutoEventWireup="true" CodeFile="RandomJuice.aspx.cs" Inherits="RandomJuice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/RandomJuice.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row random-category">
            <div class="col-xs-12 col-sm-4 col-md-4 col-lg-4">
                <img src="images/random-banner.gif" />
            </div>
            <div class="col-xs-12 col-sm-4 col-md-4 col-lg-4">
                <a href="#">
                    <img src="images/selective-barrier.gif" />
                </a>
                <div id="slider1_container" style="position: relative; top: 0px; left: 0px; width: 600px; height: 300px;">
                    <!-- Slides Container -->
                    <div u="slides" style="cursor: move; position: absolute; overflow: hidden; left: 0px; top: 0px; width: 600px; height: 300px;">
                        <div>
                            <img u="image" src="image1.jpg" /></div>
                        <div>
                            <img u="image" src="image2.jpg" /></div>
                    </div>
                    <!-- Trigger -->
                    <script>jssor_slider1_starter('slider1_container');</script>
                </div>
            </div>
            <div class="col-xs-12 col-sm-4 col-md-4 col-lg-4">
                <img src="images/delivery-area.gif" />
            </div>
        </div>
    </div>

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplJuicePage" type="text/x-jsrender">
        <div class="col-xs-6" onclick="addToCart({{:ID}},'{{:FruitName}}','{{:FruitDesc}}',{{:FruitPrice}});">
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
                jssor_slider1_starter = function (containerId) {
                    var jssor_slider1 = new $JssorSlider$(containerId, { $AutoPlay: true });
                };
            });
        });

    </script>
</asp:Content>

