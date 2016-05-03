<%@ Page Title="订单管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="ManageOrder.aspx.cs" Inherits="ManageOrder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/ManageOrder.css" rel="stylesheet" />
    <link href="../css/ladda-themeless.min.css" rel="stylesheet" />
    <link href="../Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-12">
                <div class="panel panel-info">
                    <div class="panel-heading text-center">
                        <h2 class="panel-title">订单查询</h2>
                    </div>
                    <div class="panel-body" id="divCriterias">
                        <div class="row">
                            <div class="col-lg-10">
                                <div class="form-group">
                                    <label for="ddlPaymentTerm" class="sr-only">支付方式</label>
                                    <asp:DropDownList ID="ddlPaymentTerm" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===支付方式===</asp:ListItem>
                                        <asp:ListItem Value="1">微信支付</asp:ListItem>
                                        <asp:ListItem Value="2">货到付款</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlTradeState" class="sr-only">支付状态</label>
                                    <asp:DropDownList ID="ddlTradeState" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===微信支付状态===</asp:ListItem>
                                        <asp:ListItem Value="1">支付成功</asp:ListItem>
                                        <asp:ListItem Value="2">转入退款</asp:ListItem>
                                        <asp:ListItem Value="3">未支付</asp:ListItem>
                                        <asp:ListItem Value="4">已关闭</asp:ListItem>
                                        <asp:ListItem Value="5">已撤销（刷卡支付）</asp:ListItem>
                                        <asp:ListItem Value="6">用户支付中</asp:ListItem>
                                        <asp:ListItem Value="7">支付失败</asp:ListItem>
                                        <asp:ListItem Value="-1">===货到付款状态===</asp:ListItem>
                                        <asp:ListItem Value="8">已付现金</asp:ListItem>
                                        <asp:ListItem Value="9">未付现金</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlIsDelivery" class="sr-only">发货状态</label>
                                    <asp:DropDownList ID="ddlIsDelivery" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===发货状态===</asp:ListItem>
                                        <asp:ListItem Value="1">已发货</asp:ListItem>
                                        <asp:ListItem Value="0">未发货</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlIsAccept" class="sr-only">签收状态</label>
                                    <asp:DropDownList ID="ddlIsAccept" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===签收状态===</asp:ListItem>
                                        <asp:ListItem Value="1">已签收</asp:ListItem>
                                        <asp:ListItem Value="0">未签收</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlIsCancel" class="sr-only">撤单状态</label>
                                    <asp:DropDownList ID="ddlIsCancel" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===撤单状态===</asp:ListItem>
                                        <asp:ListItem Value="1">已撤单</asp:ListItem>
                                        <asp:ListItem Value="0">未撤单</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="txtDeliverName" class="sr-only">收货人姓名</label>
                                    <asp:TextBox ID="txtDeliverName" runat="server" placeholder="收货人姓名" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtDeliverPhone" class="sr-only">收货人电话</label>
                                    <asp:TextBox ID="txtDeliverPhone" runat="server" placeholder="收货人电话" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtOrderID" class="sr-only">订单ID</label>
                                    <asp:TextBox ID="txtOrderID" runat="server" placeholder="订单ID" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtOrderDetail" class="sr-only">订单商品</label>
                                    <asp:TextBox ID="txtOrderDetail" runat="server" placeholder="订单商品详情" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtTransactionID" class="sr-only">微信支付流水号</label>
                                    <asp:TextBox ID="txtTransactionID" runat="server" placeholder="微信支付流水号" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtStartOrderDate" class="sr-only">订单开始日期</label>
                                    <asp:TextBox ID="txtStartOrderDate" runat="server" size="10" placeholder="订单开始日期" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="EndOrderDate" class="sr-only">订单结束日期</label>
                                    <asp:TextBox ID="txtEndOrderDate" runat="server" size="10" placeholder="订单结束日期" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                                </div>

                            </div>
                            <div class="col-lg-2 center-block">
                                <asp:Button ID="btnSearch" runat="server" Text="查询" CssClass="btn btn-info" OnClick="btnSearch_Click" OnClientClick="return verifyCriteria();" />
                                <asp:Button ID="btnShowAll" runat="server" Text="全部订单" CssClass="btn btn-warning" OnClick="btnShowAll_Click" />
                            </div>
                        </div>
                    </div>
                    <div class="panel-footer">
                        查询订单总金额：<asp:Label ID="lblOrderPrice" runat="server" CssClass="badge" Text=""></asp:Label>
                        查询订单总数：<asp:Label ID="lblTotalRows" runat="server" CssClass="badge" Text=""></asp:Label>
                        待支付：<asp:Label ID="lblPayingOrderCount" runat="server" CssClass="badge" Text=""></asp:Label>
                        待发货：<asp:Label ID="lblDeliveringOrderCount" runat="server" CssClass="badge" Text=""></asp:Label>
                        待签收：<asp:Label ID="lblAcceptingOrderCount" runat="server" CssClass="badge" Text=""></asp:Label>
                        已撤单：<asp:Label ID="lblCancelledOrderCount" runat="server" CssClass="badge" Text=""></asp:Label>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12">
                <asp:GridView ID="gvOrderList" runat="server" AutoGenerateColumns="False" DataSourceID="odsOrderList" AllowPaging="True" DataKeyNames="ID" OnRowDataBound="gvOrderList_RowDataBound" CssClass="table table-hover table-responsive" PagerSettings-Mode="NumericFirstLast" AllowCustomPaging="True" OnRowUpdating="gvOrderList_RowUpdating">
                    <Columns>
                        <asp:TemplateField HeaderText="订单信息" SortExpression="OrderID">
                            <ItemTemplate>
                                <p title="下单人微信昵称"><i class="fa fa-wechat"></i>&nbsp;<asp:Label ID="Label5" runat="server" Text='<%# (Eval("Purchaser")!=null?Eval("Purchaser.Nickname"):string.Empty) + (Eval("Purchaser")!=null?((bool)Eval("Purchaser.Sex")?"&nbsp;<i class=\"fa fa-mars\" style=\"color:blue;\"></i>":"&nbsp;<i class=\"fa fa-venus\" style=\"color:deeppink;\"></i>"):string.Empty) %>'></asp:Label></p>
                                <p title="订单编号"><i class="fa fa-file-text-o"></i>&nbsp;<asp:Label ID="Label1" runat="server" CssClass="order-id" Text='<%# Eval("OrderID") %>'></asp:Label></p>
                                <p title="下单时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="Label4" runat="server" Text='<%# Eval("OrderDate") %>'></asp:Label></p>
                                <p id="pCancelDate" runat="server" title="撤单时间"><i class="fa fa-close"></i>&nbsp;<asp:Label ID="Label2" runat="server" Text='<%# Eval("CancelDate") %>'></asp:Label></p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="收货人信息" SortExpression="DeliverName">
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# "<p title=\"收货人\"><i class=\"fa fa-user\"></i>&nbsp;"+Server.HtmlEncode(Eval("DeliverName").ToString())+"("+Server.HtmlEncode(Eval("DeliverPhone").ToString())+")</p><p title=\"收货地址\"><i class=\"fa fa-map-marker\"></i>&nbsp;"+Server.HtmlEncode(Eval("DeliverAddress").ToString())+"</p><p title=\"订单备注\"><i class=\"fa fa-pencil-square-o\"></i>&nbsp;"+Server.HtmlEncode(Eval("OrderMemo").ToString())+"</p>" %>'></asp:Label>
                            </ItemTemplate>
                            <ItemStyle CssClass="col-lg-2"></ItemStyle>
                        </asp:TemplateField>
                        <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="订单商品详情" SortExpression="OrderDetailList">
                            <ItemTemplate>
                                <asp:DataList ID="dlOrderDetail" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" ShowFooter="False" DataSource='<%# Eval("OrderDetailList") %>'>
                                    <ItemTemplate>
                                        <li>
                                            <asp:Label ID="OrderProductNameLabel" runat="server" Text='<%# Eval("OrderProductName") %>' />
                                            <asp:Label ID="PurchasePriceLabel" runat="server" Text='<%# Eval("PurchasePrice", "{0:C}") %>' CssClass="purchase-price" />
                                            元/<asp:Label ID="PurchaseUnitLabel" runat="server" Text='<%# Eval("PurchaseUnit") %>' />
                                            <asp:Label ID="PurchaseQtyLabel" runat="server" Text='<%# "x "+Eval("PurchaseQty") %>' CssClass="purchase-qty" />
                                        </li>
                                    </ItemTemplate>
                                </asp:DataList>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="订单金额" SortExpression="OrderPrice">
                            <ItemTemplate>
                                <asp:Label ID="Label6" runat="server" Text='<%# Eval("OrderPrice", "{0:c}") %>' CssClass="order-price"></asp:Label>
                                <li>
                                    <asp:Label ID="Label7" runat="server" Text='<%# "含运费："+Eval("Freight", "{0:c}") %>' CssClass="freight"></asp:Label></li>
                                <li>
                                    <asp:Label ID="Label8" runat="server" Text='<%# "积分优惠：- "+Eval("MemberPointsDiscount", "{0:c}") %>' CssClass="member-points-discount"></asp:Label></li>
                            </ItemTemplate>
                            <ItemStyle CssClass="order-price" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="支付状态" SortExpression="PaymentTerm">
                            <ItemTemplate>
                                <p>
                                    <asp:Label ID="lblPaymentTerm" runat="server"></asp:Label>：<asp:Label ID="lblWxTradeState" runat="server" ToolTip='<%# Eval("TradeStateDesc") %>'></asp:Label>
                                    <span id="divCashTradeState" runat="server" class="checkbox">
                                        <label>
                                            <i runat="server" id="faCashTradeState" class="fa fa-check"></i>
                                            <asp:CheckBox ID="cbCashTradeState" runat="server" OnCheckedChanged="cbCashTradeState_CheckedChanged" AutoPostBack="True" onclick="if(!confirm('点击后将不能修改现金收讫状态，确认吗？')){return false;}" /><asp:Label ID="lblCashTradeState" runat="server"></asp:Label>
                                        </label>
                                    </span>
                                </p>
                                <p id="pTransactionID" runat="server" title="微信支付流水号"><i class="fa fa-file-text-o"></i>&nbsp;<asp:Label ID="lblTransactionID" runat="server" CssClass="transaction-id" Text='<%# Eval("TransactionID") %>'></asp:Label></p>
                                <p id="pTransactionTime" runat="server" title="微信支付时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblTransactionTime" runat="server" Text='<%# Eval("TransactionTime") %>'></asp:Label></p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否发货" SortExpression="IsDelivered">
                            <ItemTemplate>
                                <div class="checkbox">
                                    <label>
                                        <asp:CheckBox ID="cbIsDelivery" runat="server" Checked='<%# Bind("IsDelivered") %>' AutoPostBack="True" onclick="if(!confirm('点击发货后将不能修改，确认发货吗？')){return false;}" OnCheckedChanged="cbIsDelivery_CheckedChanged" POID='<%# Eval("ID") %>' />
                                    </label>
                                </div>
                                <i runat="server" id="faIsDelivery" class="fa fa-check"></i>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否签收" SortExpression="IsAccept">
                            <ItemTemplate>
                                <div class="checkbox">
                                    <label>
                                        <asp:CheckBox ID="cbIsAccept" runat="server" Checked='<%# Bind("IsAccept") %>' AutoPostBack="True" onclick="if(!confirm('点击签收后将不能修改，确认签收吗？')){return false;}" POID='<%# Eval("ID") %>' OnCheckedChanged="cbIsAccept_CheckedChanged" />
                                    </label>
                                </div>
                                <i runat="server" id="faIsAccept" class="fa fa-check"></i>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="打印订单">
                            <ItemTemplate>
                                <button id="btnPrintPreview" class="btn btn-danger ladda-button" type="button" data-style="zoom-in" onclick='printPreview(<%# Eval("ID") %>);'><i class="fa fa-print"></i>&nbsp;打印</button>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerSettings Mode="NumericFirstLast"></PagerSettings>
                    <PagerStyle CssClass="pager" />
                </asp:GridView>
            </div>
        </div>
        <asp:ObjectDataSource ID="odsOrderList" runat="server" SelectMethod="FindProductOrderPager" TypeName="ProductOrder" EnablePaging="True" OnSelecting="odsOrderList_Selecting" OnSelected="odsOrderList_Selected" SelectCountMethod="FindProductOrderCount" UpdateMethod="UpdateOrderDeliver" DataObjectTypeName="ProductOrder" OnUpdating="odsOrderList_Updating"></asp:ObjectDataSource>
    </div>
    <div class="md-modal md-effect-3" id="divModal">
        <div class="md-content">
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplPrintOrder" type="text/x-jsrender">
        <link href="../css/PrintPO.css" rel="stylesheet" />
        <link href="http://apps.bdimg.com/libs/bootstrap/3.3.0/css/bootstrap.min.css" rel="stylesheet" />
        <title>订单打印</title>
        <div class="container-fluid print-po">
            <h1 class="text-center">Fruit U</h1>
            <div class="row">
                <div class="col-lg-10">
                    <p class="text-left">订单号：<span>{{:OrderID}}</span></p>
                    <p class="text-left">下单时间：<span>{{:OrderDate}}</span></p>
                    <p class="text-left">收货人：<span>{{:DeliverName}}</span>(<span>{{:DeliverPhone}}</span>)</p>
                    <p class="text-left">地址：<span>{{:DeliverAddress}}</span></p>
                    <p class="text-left">订单备注：<span>{{:OrderMemo}}</span></p>
                    <p class="text-center">
                        <table class="table table-condensed">
                            <tr>
                                <td class="text-center">商品名称</td>
                                <td class="text-center">单价</td>
                                <td class="text-center">数量</td>
                            </tr>
                            {{for OrderDetailList}}
                    <tr>
                        <td class="text-center">{{:OrderProductName}}</td>
                        <td class="text-center">{{:PurchasePrice}}元</td>
                        <td class="text-center">{{:PurchaseQty}}</td>
                    </tr>
                            {{/for}}
                        </table>
                    </p>
                    <p class="text-right">运费：￥<span>{{:Freight}}</span></p>
                    <p class="text-right">积分优惠：￥<span>{{:MemberPointsDiscount}}</span></p>
                    <p class="order-price">订单总金额：￥<span>{{:OrderPrice}}</span></p>
                    <p class="text-right">
                        支付方式：<span>
                {{if PaymentTerm == 1}}
                微信支付
                {{else PaymentTerm == 2}}
                货到付款
                {{/if}}
                        </span>
                    </p>
                </div>
            </div>
            <hr />
            <div class="row text-center">
                <div class="col-lg-10">
                    <h4>Fresh Fruit & Juice</h4>
                    <h5>关注FruitU官方微信，尽享鲜果饮品！</h5>
                    <img class="qr-code" src="../images/FruitUQRCode430.jpg" />
                </div>
            </div>
            <div class="row text-center hidden-print">
                <div class="col-lg-10">
                    <input id="btnPrint" type="button" value="打印" class="btn btn-danger" onclick="window.print();" />
                    <input id="btnClose" type="button" value="关闭" class="btn btn-info" onclick="window.close();" />
                </div>
            </div>
        </div>
    </script>

    <script>
        var Ladda;
        var tmplPO;

        requirejs(['jquery', 'ladda', 'jqueryui'], function ($, ladda) {

            $(function () {
                //设为全局对象，方便后面调用
                window.Ladda = ladda;

                require(['jsrender'], function () {
                    tmplPO = $.templates("#tmplPrintOrder");
                });

                requirejs(['datepickerCN'], function () {

                    //http://api.jqueryui.com/datepicker/
                    $("#txtStartOrderDate").datepicker({ dateFormat: 'yy-mm-dd' });
                    $("#txtEndOrderDate").datepicker({ dateFormat: 'yy-mm-dd' });

                    $("#txtStartOrderDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtEndOrderDate").val() != "")
                            if ($(this).val() > $("#txtEndOrderDate").val()) {
                                alert("开始时间必须早于结束时间。");
                                $(this).val("");
                            }
                    });
                    $("#txtEndOrderDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtStartOrderDate").val() != "")
                            if ($(this).val() < $("#txtStartOrderDate").val()) {
                                alert("结束时间必须晚于开始时间。");
                                $(this).val("");
                            }
                    });
                });

                //绑定页面上所有的ladda button
                Ladda.bind('button.ladda-button');

            });

            $(".md-overlay").on("click", closeModal);

            $("#btnClose").on("click", function () {
                closeModal();
                event.stopPropagation();
            });

        });

        //显示模式窗口
        function openModal() {
            $("#divModal").addClass("md-show");
        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");
        }


        //校验是否输入了查询条件
        function verifyCriteria() {
            var hasCriteria = false;

            $("#divCriterias select").each(function () {
                if (this.selectedIndex != 0) {
                    hasCriteria = true;
                    return;
                }
            });

            $("#divCriterias input:text").each(function () {
                if (this.value.trim() != "") {
                    hasCriteria = true;
                    return;
                }
            });

            if (!hasCriteria) {
                alert("请先选择查询条件。");
                return false;
            }
            else {
                return true;
            }
        }

        //打印预览
        function printPreview(poID) {

            $.ajax({
                url: "PrintOrder.ashx",
                data: { POID: poID },
                type: "GET",
                dataType: "json",
                cache: false,
                success: function (jPO) {
                    try {
                        if (jPO == null) {
                            throw new Error("没有获取到订单信息");
                        }
                        if (jPO["err_code_des"] != null) {
                            throw new Error(jPO["err_code_des"]);
                        }
                        var htmlPO = tmplPO.render(jPO);
                        var width = 300, height = 500;
                        var top = Math.round((window.screen.height - height) / 2);
                        var left = Math.round((window.screen.width - width) / 2);
                        var winPO = window.open('', 'winPO', 'width=' + width + ',height=' + height + ',top=' + top + ',left=' + left + ' toolbar=no, menubar=no, scrollbars=yes, resizable=no, location=no, status=yes');
                        if (winPO) {
                            winPO.document.write(htmlPO);
                            winPO.focus();
                            $(winPO).on("unload", function () { Ladda.stopAll(); });
                        }
                        else {
                            throw new Error("请设置浏览器允许弹出打印窗口");
                        }
                    }
                    catch (error) {
                        alert(error.message);
                        Ladda.stopAll();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(errorThrown + ":" + textStatus);
                    Ladda.stopAll();
                }
            });

            return false;
        }

    </script>
</asp:Content>
