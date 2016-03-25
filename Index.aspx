<%@ Page Title="首页" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Index.aspx.cs" Inherits="Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/index.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-xs-12">
                <img class="img-responsive center-block banner" alt="FruitU" src="images/banner.gif" />
            </div>
        </div>
        <div class="row category-items text-center">
            <div class="col-xs-12">
                <a href="RandomJuice.aspx">
                    <img src="images/random-category.gif" />
                </a>
            </div>
            <div class="col-xs-12">
                <a href="JuiceList.aspx">
                    <img src="images/juice-category.gif" />
                </a>
            </div>
            <div class="col-xs-12">
                <a href="FruitList.aspx">
                    <img src="images/fruit-category.gif" />
                </a>
            </div>
        </div>
    </div>


    <script>

        requirejs(['jquery'], function ($) {
            $(function () {

            });
        });

    </script>

</asp:Content>
