<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Index.aspx.cs" Inherits="Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/index.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row">
            <div class="col-xs-12">
                <img class="img-responsive center-block banner" alt="FruitU" src="images/FruitU.png" />
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
                <a href="SliceList.aspx">
                    <img src="images/fruit-category.gif" />
                </a>
            </div>
             <div class="col-xs-12">
                <a href="FruitGift.aspx">
                    <img src="images/fruit-gift.gif" />
                </a>
            </div>
            <div class="col-xs-12">
                <a href="FruitList.aspx">
                    <img src="images/pure-fruit.gif" />
                </a>
            </div>
           <div class="col-xs-12">
                <img src="images/OnDutyTime.png" />
            </div>
        </div>
    </div>
    <div class="md-modal md-effect-3" id="divModal">
        <div class="md-content">
            <p>系统升级中</p>
            <p>敬请期待  <i class="fa fa-smile-o fa-lg"></i></p>
            <div id="btnClose" class="btn-close"><i class="fa fa-close fa-3x"></i></div>
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <script>

        requirejs(['jquery'], function ($) {
            $(function () {
                //$(".category-items a").on("click", switchModalShow);

                $(".md-overlay").on("click", closeModal);

                $("#btnClose").on("click", function () {
                    closeModal();
                    event.stopPropagation();
                });


            });
        })


        //显示模式窗口
        function switchModalShow() {
            $("#divModal").addClass("md-show");
            event.preventDefault();
        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");
        }


    </script>

</asp:Content>
