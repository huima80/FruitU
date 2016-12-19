<%@ Page Title="订单管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="ManageOrder.aspx.cs" Inherits="ManageOrder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/ManageOrder.css" rel="stylesheet" />
    <link href="../Scripts/ladda/ladda-themeless.min.css" rel="stylesheet" />
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
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlTradeState" class="sr-only">支付状态</label>
                                    <asp:DropDownList ID="ddlTradeState" runat="server" CssClass="form-control">
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
                                    <label for="txtGroupEventID" class="sr-only">团购活动ID</label>
                                    <asp:TextBox ID="txtGroupEventID" runat="server" placeholder="团购活动ID" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtOrderDetail" class="sr-only">订单商品</label>
                                    <asp:TextBox ID="txtOrderDetail" runat="server" placeholder="商品详情" CssClass="form-control" Width="200"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtTransactionID" class="sr-only">微信支付交易号</label>
                                    <asp:TextBox ID="txtTransactionID" runat="server" placeholder="微信支付交易号" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtTradeNo" class="sr-only">支付宝交易号</label>
                                    <asp:TextBox ID="txtTradeNo" runat="server" placeholder="支付宝交易号" CssClass="form-control" Width="150"></asp:TextBox>
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
                <asp:GridView ID="gvOrderList" runat="server" AutoGenerateColumns="False" DataSourceID="odsOrderList" AllowPaging="True" DataKeyNames="ID" OnRowDataBound="gvOrderList_RowDataBound" CssClass="table table-hover table-responsive" PagerSettings-Mode="NumericFirstLast" AllowCustomPaging="True" OnRowCommand="gvOrderList_RowCommand">
                    <Columns>
                        <asp:TemplateField HeaderText="订单信息" SortExpression="OrderID">
                            <ItemTemplate>
                                <p title="下单人微信昵称"><i class="fa fa-wechat"></i>&nbsp;<asp:Label ID="Label5" runat="server" Text='<%# (Eval("Purchaser")!=null?Eval("Purchaser.Nickname"):string.Empty) + (Eval("Purchaser")!=null?((bool)Eval("Purchaser.Sex")?"&nbsp;<i class=\"fa fa-mars\" style=\"color:blue;\"></i>":"&nbsp;<i class=\"fa fa-venus\" style=\"color:deeppink;\"></i>"):string.Empty) %>'></asp:Label></p>
                                <p title="推荐人微信昵称" id="pAgent" runat="server"><i class="fa fa-thumbs-o-up"></i>&nbsp;<asp:Label ID="lblAgent" runat="server"></asp:Label></p>
                                <p title="订单编号"><i class="fa fa-file-text-o"></i>&nbsp;<asp:Label ID="Label1" runat="server" CssClass="order-id" Text='<%# Eval("OrderID") %>'></asp:Label></p>
                                <p title="下单时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="Label4" runat="server" Text='<%# Eval("OrderDate") %>'></asp:Label></p>
                                <p id="pCancelDate" runat="server" title="撤单时间"><i class="fa fa-close"></i>&nbsp;<asp:Label ID="Label2" runat="server" Text='<%# Eval("CancelDate") %>'></asp:Label></p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="收货人信息" SortExpression="DeliverName">
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# "<p title=\"收货人\"><i class=\"fa fa-user\"></i>&nbsp;"+Server.HtmlEncode(Eval("DeliverName").ToString())+"("+Server.HtmlEncode(Eval("DeliverPhone").ToString())+")</p><p title=\"收货地址\"><i class=\"fa fa-map-marker\"></i>&nbsp;"+Server.HtmlEncode(Eval("DeliverAddress").ToString())+"</p><p title=\"订单备注\"><i class=\"fa fa-info-circle\"></i>&nbsp;" + Server.HtmlEncode(Eval("OrderMemo").ToString()) + "</p>" %>'></asp:Label>
                            </ItemTemplate>
                            <ItemStyle CssClass="col-lg-3"></ItemStyle>
                        </asp:TemplateField>
                        <asp:TemplateField ConvertEmptyStringToNull="False" HeaderText="订单商品详情" SortExpression="OrderDetailList">
                            <ItemTemplate>
                                <asp:DataList ID="dlOrderDetail" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" ShowFooter="False" DataSource='<%# Eval("OrderDetailList") %>' OnItemDataBound="dlOrderDetail_ItemDataBound">
                                    <ItemTemplate>
                                        <li>
                                            <asp:Label ID="OrderProductNameLabel" runat="server" Text='<%# Eval("OrderProductName") %>' />
                                            <asp:Label ID="PurchasePriceLabel" runat="server" Text='<%# Eval("PurchasePrice", "{0:C}") %>' CssClass="purchase-price" />
                                            元/<asp:Label ID="PurchaseUnitLabel" runat="server" Text='<%# Eval("PurchaseUnit") %>' />
                                            <asp:Label ID="PurchaseQtyLabel" runat="server" Text='<%# "x "+Eval("PurchaseQty") %>' CssClass="purchase-qty" />
                                            <asp:HyperLink ID="hlGroupPurchaseEventStatus" runat="server" CssClass="label label-warning"></asp:HyperLink>
                                        </li>
                                    </ItemTemplate>
                                </asp:DataList>
                            </ItemTemplate>
                            <ItemStyle CssClass="col-lg-2"></ItemStyle>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="订单金额" SortExpression="OrderPrice">
                            <ItemTemplate>
                                <p>
                                    <asp:Label ID="Label6" runat="server" Text='<%# Eval("OrderPrice", "{0:c}") %>' CssClass="order-price"></asp:Label>
                                </p>
                                <p>
                                    <asp:Label ID="Label7" runat="server" Text='<%# "含运费："+Eval("Freight", "{0:c}") %>' CssClass="freight"></asp:Label>
                                </p>
                                <p id="pMemberPointsDiscount" runat="server">
                                    <asp:Label ID="Label8" runat="server" Text='<%# "积分优惠：- "+Eval("MemberPointsDiscount", "{0:c}") %>' CssClass="member-points-discount"></asp:Label>
                                </p>
                                <p id="pWxCardDiscount" runat="server">
                                    <asp:Label ID="Label9" runat="server" Text='<%# "微信优惠券：- "+Eval("WxCardDiscount", "{0:c}") %>' CssClass="wxcard-discount"></asp:Label><br />
                                    <asp:Label ID="Label10" runat="server" CssClass="wxcard-code" Text='<%# "【"+Eval("WxCard.Title")+"】" %>' ToolTip='<%# "卡券Code:"+Eval("WxCard.Code") %>'></asp:Label>
                                </p>
                            </ItemTemplate>
                            <ItemStyle CssClass="order-price" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="支付状态" SortExpression="PaymentTerm">
                            <ItemTemplate>
                                <p>
                                    <asp:Label ID="lblPaymentTerm" CssClass="payment-term" runat="server"></asp:Label>：<asp:Label ID="lblWxTradeState" runat="server" ToolTip='<%# Eval("TradeStateDesc") %>'></asp:Label><asp:Label ID="lblAlipayTradeState" runat="server"></asp:Label>
                                    <asp:Button ID="btnPayCash" runat="server" Text="" data-style="zoom-in" CommandName="PayCash" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirm('确认现金收讫吗？');" />
                                </p>
                                <p id="pTransactionID" runat="server" title="微信支付交易号"><i class="fa fa-file-text-o"></i>&nbsp;<asp:Label ID="lblTransactionID" runat="server" CssClass="transaction-id" Text='<%# Eval("TransactionID") %>'></asp:Label></p>
                                <p id="pTransactionTime" runat="server" title="微信支付时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblTransactionTime" runat="server" Text='<%# Eval("TransactionTime") %>'></asp:Label></p>
                                <p id="pAP_TradeNo" runat="server" title="支付宝交易号"><i class="fa fa-file-text-o"></i>&nbsp;<asp:Label ID="lblAP_TradeNo" runat="server" Text='<%# Eval("AP_TradeNo") %>'></asp:Label></p>
                                <p id="pAP_GMT_Payment" runat="server" title="支付宝支付时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblAP_GMT_Payment" runat="server" Text='<%# Eval("AP_GMT_Payment") %>'></asp:Label></p>
                                <p id="pPayCashDate" runat="server" title="货到付款时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblPayCashDate" runat="server" Text='<%# Eval("PayCashDate") %>'></asp:Label></p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="订单操作">
                            <ItemTemplate>
                                <asp:Button ID="btnDeliver" runat="server" Text="发货" CssClass="" data-style="zoom-in" CommandName="Deliver" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirm('确认发货吗？');" />
                                <asp:Button ID="btnAccept" runat="server" Text="签收" CssClass="" data-style="zoom-in" CommandName="Accept" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirm('确认签收吗？');" />
                                <asp:Button ID="btnCalMemberPoints" runat="server" Text="发放积分" CssClass="" data-style="zoom-in" CommandName="CalMemberPoints" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirm('确认发放积分吗？');" />
                                <button id="btnPrintPreview" class="<%#BTN_DOING %>" type="button" data-style="zoom-in" onclick='printPreview(<%# Eval("ID") %>);'><i class="fa fa-print"></i>&nbsp;打印</button>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <PagerSettings Mode="NumericFirstLast"></PagerSettings>
                    <PagerStyle CssClass="pager" />
                </asp:GridView>
            </div>
        </div>
        <asp:ObjectDataSource ID="odsOrderList" runat="server" SelectMethod="FindProductOrderPager" TypeName="ProductOrder" EnablePaging="True" OnSelecting="odsOrderList_Selecting" OnSelected="odsOrderList_Selected" SelectCountMethod="FindProductOrderCount" DataObjectTypeName="ProductOrder"></asp:ObjectDataSource>
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
                                <td class="text-center">数量</td>
                                <td class="text-center">单价</td>
                            </tr>
                            {{for OrderDetailList}}
                    <tr>
                        <td class="text-center">{{:OrderProductName}}</td>
                        <td class="text-center">{{:PurchaseQty}}{{:PurchaseUnit}}</td>
                        <td class="text-center">{{:PurchasePrice}}元</td>
                    </tr>
                            {{/for}}
                            <tr>
                                <td class="text-center">小计：</td>
                                <td class="text-right" colspan="2">{{:OrderDetailPrice}}元</td>
                            </tr>
                        </table>
                    </p>
                    <p class="text-right">运费：￥<span>{{:Freight}}</span></p>
                    {{if MemberPointsDiscount != 0}}
                    <p class="text-right">积分优惠：-￥<span>{{:MemberPointsDiscount}}</span></p>
                    {{/if}}
                     {{if WxCardDiscount != 0}}
                    <p class="text-right">
                        微信优惠券优惠：-￥<span>{{:WxCardDiscount}}</span>
                        {{if WxCard.Title != ""}}
                        <br />
                        <span>【{{:WxCard.Title}}】</span>
                        {{/if}}
                    </p>
                    {{/if}}
                 <p class="order-price">订单总金额：￥<span>{{:OrderPrice}}</span></p>
                    <p class="text-right">
                        支付方式：<span>
                {{if PaymentTerm == 1}}
                微信支付
                {{else PaymentTerm == 2}}
                货到付款
                 {{else PaymentTerm == 3}}
                支付宝
               {{/if}}
                        </span>
                    </p>
                </div>
            </div>
            <hr />
            <div class="row text-center">
                <div class="col-lg-10">
                    <h4>Fresh Fruits & Juice</h4>
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
                Ladda = ladda;

                requirejs(['jsrender'], function () {
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
                            throw new Error("请设置浏览器允许弹出打印窗口。");
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
