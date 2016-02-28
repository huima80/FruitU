<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ManageOrder.aspx.cs" Inherits="ManageOrder" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>订单管理</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link href="../css/ManageOrder.css" rel="stylesheet" />
</head>
<body>
    <!-- #include file="header.html" -->
    <form id="form1" class="form-inline" runat="server">
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
                                            <asp:ListItem Selected="True" Value="-1">===支付状态===</asp:ListItem>
                                            <asp:ListItem Value="1">支付成功</asp:ListItem>
                                            <asp:ListItem Value="2">转入退款</asp:ListItem>
                                            <asp:ListItem Value="3">未支付</asp:ListItem>
                                            <asp:ListItem Value="4">已关闭</asp:ListItem>
                                            <asp:ListItem Value="5">已撤销（刷卡支付）</asp:ListItem>
                                            <asp:ListItem Value="6">用户支付中</asp:ListItem>
                                            <asp:ListItem Value="7">支付失败</asp:ListItem>
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
                                        <asp:TextBox ID="txtStartOrderDate" runat="server" size="10" placeholder="订单开始日期" CssClass="form-control"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label for="EndOrderDate" class="sr-only">订单结束日期</label>
                                        <asp:TextBox ID="txtEndOrderDate" runat="server" size="10" placeholder="订单结束日期" CssClass="form-control"></asp:TextBox>
                                    </div>

                                </div>
                                <div class="col-lg-2 center-block">
                                    <asp:Button ID="btnSearch" runat="server" Text="查询" CssClass="btn btn-info" OnClick="btnSearch_Click" OnClientClick="return verifyCriteria();" />
                                    <asp:Button ID="btnShowAll" runat="server" Text="全部订单" CssClass="btn btn-warning" OnClick="btnShowAll_Click" />
                                    <div class="search-result" id="divSearchResult">查询订单数量：<asp:Label ID="lblSearchResult" runat="server" CssClass="badge" Text=""></asp:Label></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="row">
                <div class="col-lg-12">
                    <asp:GridView ID="gvOrderList" runat="server" AutoGenerateColumns="False" DataSourceID="odsOrderList" AllowPaging="True" DataKeyNames="ID" OnRowDataBound="gvOrderList_RowDataBound" CssClass="table table-striped table-hover table-responsive" PagerSettings-Mode="NumericFirstLast" AllowCustomPaging="True">
                        <Columns>
                            <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="ID" />
                            <asp:BoundField DataField="OrderID" HeaderText="订单ID" SortExpression="OrderID" ReadOnly="True" />
                            <asp:BoundField DataField="OrderDate" HeaderText="订单日期" SortExpression="OrderDate" ReadOnly="True" />
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
                            <asp:BoundField DataField="OrderPrice" DataFormatString="{0:c}" HeaderText="订单金额" ReadOnly="True" SortExpression="OrderPrice" ItemStyle-CssClass="order-price" />
                            <asp:TemplateField HeaderText="收货人名称" SortExpression="DeliverName">
                                <ItemTemplate>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Eval("DeliverName") %>' ToolTip='<%# Eval("DeliverPhone")+"\n"+Eval("DeliverAddress")+"\n备注："+Eval("OrderMemo") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="支付方式" SortExpression="PaymentTerm">
                                <ItemTemplate>
                                    <asp:Label ID="lblPaymentTerm" runat="server" Text='<%# paymentTerm((PaymentTerm)Eval("PaymentTerm")) %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="支付状态" SortExpression="TradeState">
                                <ItemTemplate>
                                    <asp:Label ID="lblTradeState" runat="server" Text='<%# tradeState((TradeState)Eval("TradeState")) %>' ToolTip='<%# Eval("TradeStateDesc") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="TransactionID" HeaderText="微信支付流水号" SortExpression="TransactionID" ReadOnly="True" />
                            <asp:BoundField DataField="TransactionTime" HeaderText="微信支付交易日期" SortExpression="TransactionTime" ReadOnly="True" />
                            <asp:TemplateField HeaderText="是否发货" SortExpression="IsDelivered">
                                <ItemTemplate>
                                    <div class="radio">
                                        <label>
                                            <asp:CheckBox ID="cbIsDelivery" runat="server" Checked='<%# Bind("IsDelivered") %>' AutoPostBack="True" onclick="if(!confirm('点击发货后将不能修改，确认发货吗？')){return false;}" OnCheckedChanged="cbIsDelivery_CheckedChanged" ToolTip='<%# Eval("ID") %>' />
                                        </label>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="DeliverDate" HeaderText="发货日期" SortExpression="DeliverDate" ReadOnly="True" />
                            <asp:CheckBoxField DataField="IsAccept" HeaderText="是否签收" ReadOnly="True" SortExpression="IsAccept" />
                            <asp:BoundField DataField="AcceptDate" HeaderText="签收日期" ReadOnly="True" SortExpression="AcceptDate" />
                        </Columns>
                        <PagerSettings Mode="NumericFirstLast"></PagerSettings>
                        <PagerStyle CssClass="pager" />
                    </asp:GridView>
                </div>
            </div>
            <asp:ObjectDataSource ID="odsOrderList" runat="server" SelectMethod="FindProductOrderPager" TypeName="ProductOrder" EnablePaging="True" OnSelecting="odsOrderList_Selecting" OnSelected="odsOrderList_Selected" SelectCountMethod="FindProductOrderCount" UpdateMethod="UpdateOrderDeliver"></asp:ObjectDataSource>
        </div>
    </form>
</body>

<script>

    $(function () {

        //http://api.jqueryui.com/datepicker/
        $.datepicker.setDefaults($.datepicker.regional["zh-TW"]);

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

</script>

</html>
