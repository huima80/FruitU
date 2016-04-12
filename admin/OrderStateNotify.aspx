<%@ Page Title="订单通知管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="OrderStateNotify.aspx.cs" Inherits="admin_OrderStateNotify" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="progress text-center">
        <div class="progress-bar" style="width: 20%">
            <span class="sr-only">20% Complete (success)</span><i class="fa fa-file-o"></i>&nbsp;下单
        </div>
        <div class="progress-bar progress-bar-info" style="width: 20%">
            <span class="sr-only">20% Complete (success)</span><i class="fa fa-close"></i>&nbsp;撤单
        </div>
        <div class="progress-bar progress-bar-success progress-bar-striped" style="width: 20%">
            <span class="sr-only">20% Complete (success)</span><i class="fa fa-credit-card"></i>&nbsp;支付
        </div>
        <div class="progress-bar progress-bar-warning" style="width: 20%">
            <span class="sr-only">20% Complete (success)</span><i class="fa fa-truck"></i>&nbsp;配送
        </div>
        <div class="progress-bar progress-bar-danger" style="width: 20%">
            <span class="sr-only">20% Complete (success)</span><i class="fa fa-pencil-square-o"></i>&nbsp;签收
        </div>
    </div>
    <div class="row">
        <div class="col-lg-3">
            <div class="panel panel-info">
                <div class="panel-heading">
                    <h3 class="panel-title">下单消息</h3>
                </div>
                <div class="panel-body">
                    <pre>
{{first.DATA}}
下单时间：2016年1月1日
订单总金额：88.88元
订单详情：商品名称
详细地址：上海市
{{remark.DATA}}
                    </pre></div>
            </div>
        </div>
        <div class="col-lg-3">
            <div class="panel panel-success">
                <div class="panel-heading">
                    <h3 class="panel-title">支付消息</h3>
                </div>
                <div class="panel-body">
                    Panel content
                </div>
            </div>
        </div>
        <div class="col-lg-3">
            <div class="panel panel-warning">
                <div class="panel-heading">
                    <h3 class="panel-title">配送消息</h3>
                </div>
                <div class="panel-body">
                    Panel content
                </div>
            </div>
        </div>
        <div class="col-lg-3">
            <div class="panel panel-danger">
                <div class="panel-heading">
                    <h3 class="panel-title">签收消息</h3>
                </div>
                <div class="panel-body">
                    Panel content
                </div>
            </div>
        </div>
    </div>

    <script>

        requirejs(['jquery', 'jqueryui'], function ($) {

            $(function () {

            });
        });
    </script>
</asp:Content>
