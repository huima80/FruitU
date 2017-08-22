<%@ Page Title="订单管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="ManageOrder.aspx.cs" Inherits="ManageOrder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/ManageOrder.css" rel="stylesheet" />
    <link href="../Scripts/ladda/ladda-themeless.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="http://cache.amap.com/lbs/static/main1119.css" />
    <script type="text/javascript" src="http://webapi.amap.com/maps?v=1.3&key=aee0e92073edb1ecddc7303ece02eba5"></script>
    <%--    <script type="text/javascript" src="http://api.map.baidu.com/api?v=2.0&ak=IcCARel5ElCGEYQTIK5Dot0oZ7ZCFd1K"></script>--%>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-12">
                <div class="panel panel-info">
                    <div class="panel-heading text-center">
                        <h2 class="panel-title">订单查询</h2>
                    </div>
                    <div class="panel-body form-inline">
                        <div class="row">
                            <div class="col-lg-10" id="divCriterias">
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
                                    <asp:TextBox ID="txtReceiverName" runat="server" placeholder="收货人姓名" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtDeliverPhone" class="sr-only">收货人电话</label>
                                    <asp:TextBox ID="txtReceiverPhone" runat="server" placeholder="收货人电话" CssClass="form-control" Width="150"></asp:TextBox>
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
                                    <asp:TextBox ID="txtStartOrderDate" runat="server" size="10" placeholder="订单开始日期" CssClass="form-control" TextMode="Date" onchange="checkStartOrderDate()"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="EndOrderDate" class="sr-only">订单结束日期</label>
                                    <asp:TextBox ID="txtEndOrderDate" runat="server" size="10" placeholder="订单结束日期" CssClass="form-control" TextMode="Date" onchange="checkEndOrderDate()"></asp:TextBox>
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
            <div class="col-lg-12 table-responsive">
                <asp:GridView ID="gvOrderList" runat="server" AutoGenerateColumns="False" DataSourceID="odsOrderList" AllowPaging="True" DataKeyNames="ID" OnRowDataBound="gvOrderList_RowDataBound" CssClass="table table-hover" PagerSettings-Mode="NumericFirstLast" AllowCustomPaging="True" OnRowCommand="gvOrderList_RowCommand">
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
                                            <asp:HyperLink ID="hlGroupPurchaseEventStatus" runat="server"></asp:HyperLink>
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
                                <p id="pFreight" runat="server">
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
                                <p id="pTransactionID" runat="server" title="微信支付交易号"><i class="fa fa-file-text-o"></i>&nbsp;<asp:HyperLink ID="hlTransactionID" runat="server" NavigateUrl='<%# "https://pay.weixin.qq.com/index.php/core/refundapply?autoWXOrderNum="+Eval("TransactionID") %>' Target="_blank" ToolTip="转到微信商户平台退款" CssClass="transaction-id"><%# Eval("TransactionID") %></asp:HyperLink></p>
                                <p id="pTransactionTime" runat="server" title="微信支付时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblTransactionTime" runat="server" Text='<%# Eval("TransactionTime") %>'></asp:Label></p>
                                <p id="pAP_TradeNo" runat="server" title="支付宝交易号"><i class="fa fa-file-text-o"></i>&nbsp;<asp:HyperLink ID="hlAP_TradeNo" runat="server" NavigateUrl='<%# "https://tradeeportlet.alipay.com/refund/fpSingleRefund.htm?action=refund&tradeNo="+Eval("AP_TradeNo") %>' Target="_blank" ToolTip="转到支付宝退款"><%# Eval("AP_TradeNo") %></asp:HyperLink></p>
                                <p id="pAP_GMT_Payment" runat="server" title="支付宝支付时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblAP_GMT_Payment" runat="server" Text='<%# Eval("AP_GMT_Payment") %>'></asp:Label></p>
                                <p id="pPayCashDate" runat="server" title="货到付款时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblPayCashDate" runat="server" Text='<%# Eval("PayCashDate") %>'></asp:Label></p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="订单操作">
                            <ItemTemplate>
                                <button runat="server" type="button" id="btnDelivery" class="btn btn-sm btn-block btn-success ladda-button" data-style="zoom-in">配送</button>
                                <%--                                <asp:Button ID="btnDeliver" runat="server" Text="发货" CssClass="" data-style="zoom-in" CommandName="Deliver" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirm('确认发货吗？');" />--%>
                                <asp:Button ID="btnAccept" runat="server" Text="签收" CssClass="" data-style="zoom-in" CommandName="Accept" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirm('确认签收吗？');" />
                                <asp:Button ID="btnCalMemberPoints" runat="server" Text="发放积分" CssClass="" data-style="zoom-in" CommandName="CalMemberPoints" CommandArgument='<%# Eval("ID") %>' OnClientClick="return confirm('确认发放积分吗？');" />
                                <button id="btnPrintPreview" class="<%#BTN_DOING_CSS %>" type="button" data-style="zoom-in" onclick='printPreview(<%# Eval("ID") %>);'><i class="fa fa-print"></i>&nbsp;打印</button>
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

    <!-- 配送弹出层 -->
    <div class="md-container md-effect-1" id="divModal" data-orderid="">
        <div class="md-content">
            <!-- Nav tabs -->
            <ul class="nav nav-pills nav-justified" role="tablist">
                <li role="presentation" class="active"><a id="aSelfDelivery" href="#divSelfDelivery" role="tab" data-toggle="pill">自主配送</a></li>
                <li role="presentation"><a id="aDaDa" href="#divDaDa" role="tab" data-toggle="pill">达达配送</a></li>
                <li role="presentation"><a id="aShanSong" href="#divShanSong" role="tab" data-toggle="pill">闪送</a></li>
            </ul>
            <!-- Tab panes -->
            <div class="tab-content">
                <p class="text-primary">订单号：<span id="spOrderID"></span><i id="iRefresh" class="fa fa-refresh pull-right" style="cursor: pointer" title="刷新配送状态"></i></p>
                <div class="progress" title="订单配送状态">
                    <div id="divAcceptOrderBar" class="progress-bar progress-bar-success progress-bar-striped" style="width: 25%; display: none;">
                        <span class="sr-only">25% Complete (success)</span>
                    </div>
                    <div id="divFetchOrderBar" class="progress-bar progress-bar-info progress-bar-striped" style="width: 25%; display: none;">
                        <span class="sr-only">25% Complete (info)</span>
                    </div>
                    <div id="divDeliveryOrderBar" class="progress-bar progress-bar-warning progress-bar-striped" style="width: 25%; display: none;">
                        <span class="sr-only">25% Complete (warning)</span>
                    </div>
                    <div id="divFinishOrderBar" class="progress-bar progress-bar-danger progress-bar-striped" style="width: 25%; display: none;">
                        <span class="sr-only">25% Complete (danger)</span>
                    </div>
                </div>
                <div role="tabpanel" class="tab-pane fade in active" id="divSelfDelivery">
                    <div class="form-horizontal" role="form">
                        <div class="form-group">
                            <label for="txtDeliveryName" class="col-sm-4 control-label">送货人</label>
                            <div class="col-sm-8">
                                <asp:TextBox ID="txtDeliveryName" runat="server" class="form-control" required ClientIDMode="Static"></asp:TextBox>
                            </div>
                        </div>
                        <div class="form-group">
                            <label for="txtDeliveryPhone" class="col-sm-4 control-label">送货人电话</label>
                            <div class="col-sm-8">
                                <asp:TextBox ID="txtDeliveryPhone" runat="server" class="form-control" required TextMode="Phone" ClientIDMode="Static"></asp:TextBox>
                            </div>
                        </div>
                        <div id="divDeliveryDate" class="form-group" style="display: none;">
                            <label for="lblDeliveryDate" class="col-sm-4 control-label">发货时间</label>
                            <div class="col-sm-8">
                                <p id="lblDeliveryDate" class="form-control-static"></p>
                            </div>
                        </div>
                        <div id="divSelfDeliveryBtn" class="form-group">
                            <div class="col-sm-12 text-center">
                                <button type="button" id="btnSelfDelivery" class="btn btn-info ladda-button" data-style="zoom-in" onclick="if(confirm('确认发货吗？'))selfDelivery();">发货</button>
                            </div>
                        </div>
                    </div>
                </div>
                <div role="tabpanel" class="tab-pane fade" id="divDaDa">
                    <div class="form-horizontal" role="form">
                        <div id="divAddDaDaOrder" class="dada-order-panel" data-delivery-no="" style="display: none;">
                            <fieldset>
                                <legend>发布配送订单</legend>
                                <div class="form-group">
                                    <label for="ddlWxPOIListForDaDa" class="col-sm-4 control-label">门店</label>
                                    <div class="col-sm-8">
                                        <select id="ddlWxPOIListForDaDa" class="form-control" title="用于送货员取货"></select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="ddlCityCode" class="col-sm-4 control-label">可配送城市</label>
                                    <div class="col-sm-8">
                                        <select id="ddlCityCode" class="form-control"></select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtExpectedFetchTime" class="col-sm-4 control-label">期望取货时间</label>
                                    <div class="col-sm-8">
                                        <input id="txtExpectedFetchTime" type="time" class="form-control" title="超过期望取货时间30分钟，达达将取消订单" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtExpectedFinishTime" class="col-sm-4 control-label">期望完成时间</label>
                                    <div class="col-sm-8">
                                        <input id="txtExpectedFinishTime" type="datetime-local" class="form-control" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="cbIsPrepay" class="col-sm-4 control-label">是否需要垫付</label>
                                    <div class="col-sm-8">
                                        <input type="checkbox" id="cbIsPrepay" value="1" title="送货员先垫付给商户，再收取客户的钱" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtTips" class="col-sm-4 control-label">小费（元）</label>
                                    <div class="col-sm-8">
                                        <input id="txtTips" type="number" class="form-control" onchange="verifyTips()" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtInfo" class="col-lg-4 control-label">配送备注</label>
                                    <div class="col-lg-8">
                                        <textarea id="txtInfo" class="form-control" rows="3"></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtReceiverAddress" class="col-lg-4 control-label">配送地址关键字</label>
                                    <div class="col-lg-8">
                                        <input type="text" id="txtReceiverAddress" class="form-control" data-lng="" data-lat="" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-lg-12">
                                        <div id="divMapForReceiver" class="dada-map"></div>
                                    </div>
                                </div>
                                <div class="form-group" id="divDistance" style="display: none;">
                                    <label for="lblDistance" class="col-sm-4 control-label">配送距离</label>
                                    <div class="col-sm-8">
                                        <p id="lblDistance" class="form-control-static"></p>
                                    </div>
                                </div>
                                <div class="form-group" id="divFee" style="display: none;">
                                    <label for="lblFee" class="col-sm-4 control-label">运费</label>
                                    <div class="col-sm-8">
                                        <p id="lblFee" class="form-control-static"></p>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-12 text-center">
                                        <button type="button" class="btn btn-info ladda-button" data-style="zoom-in" id="btnAddDaDaOrder" onclick="addOrder(1)">配送下单</button>
                                        <button type="button" class="btn btn-info ladda-button" data-style="zoom-in" id="btnReAddDaDaOrder" onclick="addOrder(2)">重新发单</button>
                                        <button type="button" class="btn btn-warning ladda-button" data-style="zoom-in" id="btnQueryDeliverFee" onclick="addOrder(3)">查询运费</button>
                                        <button type="button" class="btn btn-info ladda-button" data-style="zoom-in" id="btnAddAfterQuery" onclick="addAfterQuery()">查询运费后发单</button>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div id="divAddTips" class="dada-order-panel" style="display: none;">
                            <fieldset>
                                <legend>增加小费</legend>
                                <div class="form-group">
                                    <label for="lblTips" class="col-sm-4 control-label">目前小费</label>
                                    <div class="col-sm-8">
                                        <p id="lblTips" class="form-control-static"></p>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtAddTips" class="col-sm-4 control-label">新小费（元）</label>
                                    <div class="col-sm-8">
                                        <input id="txtAddTips" type="number" class="form-control" onchange="verifyTips()" title="以最新一次加小费动作的金额为准，故下一次增加小费额必须大于上一次小费额。" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtInfoForAddTips" class="col-lg-4 control-label">备注</label>
                                    <div class="col-lg-8">
                                        <textarea id="txtInfoForAddTips" class="form-control" rows="3" maxlength="512"></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-12 text-center">
                                        <button type="button" class="btn btn-danger ladda-button" data-style="zoom-in" id="btnAddTips" onclick="addTips()">增加小费</button>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div id="divDaDaOrderStatus" class="dada-order-panel" style="display: none;">
                            <div class="form-group">
                                <label for="pTransporterInfo" class="col-sm-4 control-label">骑手信息</label>
                                <div class="col-sm-8">
                                    <p class="form-control-static" id="pTransporterInfo"></p>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-sm-12">
                                    <div id="divMapForTransporter" class="dada-map"></div>
                                </div>
                            </div>
                        </div>
                        <div id="divCancelOrder" class="dada-order-panel" style="display: none;">
                            <fieldset>
                                <legend>取消订单</legend>
                                <div id="divCancelOrderWarning" class="alert alert-danger" role="alert" style="display: none;"><i class="fa fa-exclamation-circle"></i>&nbsp;<span id="spAcceptTime"></span>订单接单后1-15分钟取消订单，会扣除相应费用补贴给接单达达</div>
                                <div class="form-group">
                                    <label for="ddlCancelReason" class="col-sm-4 control-label">订单取消原因</label>
                                    <div class="col-sm-8">
                                        <select id="ddlCancelReason" class="form-control" onchange="showOtherReason()"></select>
                                    </div>
                                </div>
                                <div class="form-group" id="divOtherReason" style="display: none;">
                                    <label for="txtOtherReason" class="col-sm-4 control-label">其他原因</label>
                                    <div class="col-sm-8">
                                        <textarea id="txtOtherReason" class="form-control" rows="3"></textarea>
                                    </div>
                                </div>
                                <div class="form-group" id="divDeductFee" style="display: none;">
                                    <label for="pDeductFee" class="col-sm-4 control-label">扣除的违约金</label>
                                    <div class="col-sm-8">
                                        <p class="form-control-static" id="pDeductFee"></p>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-12 text-center">
                                        <button type="button" class="btn btn-default ladda-button" data-style="zoom-in" id="btnCancelDaDaOrder" onclick="formalCancel()">取消订单</button>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div id="divComplaintDaDa" class="dada-order-panel" style="display: none;">
                            <fieldset>
                                <legend>投诉达达</legend>
                                <div class="form-group">
                                    <label for="ddlComplaintReason" class="col-sm-4 control-label">投诉原因</label>
                                    <div class="col-sm-8">
                                        <select id="ddlComplaintReason" class="form-control"></select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-12 text-center">
                                        <button type="button" class="btn btn-default ladda-button" data-style="zoom-in" id="btnComplaintDaDa" onclick="complaintDaDa()">投诉达达</button>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div class="row">
                            <div class="col-sm-12 text-left"><i class="fa fa-question-circle-o"></i>&nbsp;<a href="https://newopen.imdada.cn/#/page/feeCriteria" target="_blank">达达物流收费标准</a></div>
                        </div>
                    </div>
                </div>
                <div role="tabpanel" class="tab-pane fade" id="divShanSong">
                    <div class="form-horizontal" role="form">
                        <div id="divAddShanSongOrder" class="dada-order-panel" data-issorderno="" style="display: none;">
                            <fieldset>
                                <legend>发布配送订单</legend>
                                <div class="form-group">
                                    <label for="ddlWxPOIListForShanSong" class="col-sm-4 control-label">门店</label>
                                    <div class="col-sm-8">
                                        <select id="ddlWxPOIListForShanSong" class="form-control" title="用于送货员取货"></select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="lblGoods" class="col-sm-4 control-label">物品</label>
                                    <div class="col-sm-8">
                                        <p id="lblGoods" class="form-control-static"></p>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtWeight" class="col-sm-4 control-label">重量（公斤，向上取整）</label>
                                    <div class="col-sm-8">
                                        <input id="txtWeight" type="number" class="form-control" onchange="verifyTips()" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtAddition" class="col-sm-4 control-label">小费（元）</label>
                                    <div class="col-sm-8">
                                        <input id="txtAddition" type="number" class="form-control" onchange="verifyTips()" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">支付方式</label>
                                    <div class="col-sm-8">
                                        <label class="radio-inline">
                                            <input type="radio" name="inlineRadioOptions" id="inlineRadio1" value="1" />
                                            现金
                                        </label>
                                        <label class="radio-inline">
                                            <input type="radio" name="inlineRadioOptions" id="inlineRadio2" value="2" />
                                            闪送账户
                                        </label>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtAppointTime" class="col-sm-4 control-label">预约时间</label>
                                    <div class="col-sm-8">
                                        <input id="txtAppointTime" type="datetime" class="form-control" title="支持2小时以后，两天以内" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtRemark" class="col-lg-4 control-label">配送备注</label>
                                    <div class="col-lg-8">
                                        <textarea id="txtRemark" class="form-control" rows="3" maxlength="250"></textarea>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtReceiverAddressForShanSong" class="col-lg-4 control-label">配送地址关键字</label>
                                    <div class="col-lg-8">
                                        <input type="text" id="txtReceiverAddressForShanSong" class="form-control" data-lng="" data-lat="" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-lg-12">
                                        <div id="divMapForReceiverForShanSong" class="shansong-map"></div>
                                    </div>
                                </div>
                                <div class="form-group" id="divDistanceForShanSong" style="display: none;">
                                    <label for="lblDistanceForShanSong" class="col-sm-4 control-label">配送距离</label>
                                    <div class="col-sm-8">
                                        <p id="lblDistanceForShanSong" class="form-control-static"></p>
                                    </div>
                                </div>
                                <div class="form-group" id="divAmount" style="display: none;">
                                    <label for="lblAmount" class="col-sm-4 control-label">运费</label>
                                    <div class="col-sm-8">
                                        <p id="lblAmount" class="form-control-static"></p>
                                    </div>
                                </div>
                                <div class="form-group" id="divCutAmount" style="display: none;">
                                    <label for="lblCutAmount" class="col-sm-4 control-label">优惠金额</label>
                                    <div class="col-sm-8">
                                        <p id="lblCutAmount" class="form-control-static"></p>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-12 text-center">
                                        <button type="button" class="btn btn-warning ladda-button" data-style="zoom-in" id="btnShanSongCalc" onclick="">查询运费</button>
                                        <button type="button" class="btn btn-info ladda-button" data-style="zoom-in" id="btnShanSongSave" onclick="">下单</button>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div id="divShanSongOrderStatus" class="dada-order-panel" style="display: none;">
                            <div class="form-group">
                                <label for="pCourier" class="col-sm-4 control-label">闪送员信息</label>
                                <div class="col-sm-8">
                                    <p class="form-control-static" id="pCourier"></p>
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-sm-12">
                                    <div id="divMapCourier" class="dada-map"></div>
                                </div>
                            </div>
                        </div>
                        <div id="divCancelShanSongOrder" class="dada-order-panel" style="display: none;">
                            <fieldset>
                                <legend>取消订单</legend>
                                <div id="divCancelShanSongOrderWarning" class="alert alert-danger" role="alert" style="display: none;"><i class="fa fa-exclamation-circle"></i>&nbsp;</div>
                                <div class="form-group" id="divDeductFeeForShanSong" style="display: none;">
                                    <label for="pDeductFeeForShanSong" class="col-sm-4 control-label">扣除的违约金</label>
                                    <div class="col-sm-8">
                                        <p class="form-control-static" id="pDeductFeeForShanSong"></p>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-12 text-center">
                                        <button type="button" class="btn btn-default ladda-button" data-style="zoom-in" id="btnCancelShanSongOrder" onclick="cancel()">取消订单</button>
                                    </div>
                                </div>
                            </fieldset>
                        </div>
                        <div class="row">
                            <div class="col-sm-12"><span id="spShanSongPrice" class="text-left"><i class="fa fa-question-circle-o"></i>&nbsp;<a href="http://www.ishansong.com/price" target="_blank">闪送收费标准</a></span><span id="spShanSongAccount" class="text-right"></span></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="md-overlay"></div>

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
                    <p class="text-left">收货人：<span>{{>DeliverName}}</span>(<span>{{>DeliverPhone}}</span>)</p>
                    <p class="text-left">地址：<span>{{>DeliverAddress}}</span></p>
                    <p class="text-left">订单备注：<span>{{>OrderMemo}}</span></p>
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
        var hCheckDaDaOrderStatus;
        var lBtnAddDaDaOrder, lBtnReAddDaDaOrder, lBtnQueryDeliverFee, lBtnAddAfterQuery, lBtnAddTips, lBtnCancelDaDaOrder, lBtnComplaintDaDa;
        var lBtnShanSongCalc, lBtnShanSongSave, lBtnCancelShanSongOrder;

        //送货人位置地图
        var mapForTransporter = new AMap.Map('divMapForTransporter', {
            resizeEnable: true,
            zoom: 15
        });
        //收货人位置标记、送货人位置标记
        var markerForReceiver, markerForTransporter;

        requirejs(['jquery', 'ladda'], function ($, ladda) {
            $(function () {
                //设为全局对象，方便后面调用
                Ladda = ladda;

                requirejs(['jsrender'], function () {
                    tmplPO = $.templates("#tmplPrintOrder");
                });

                //切换到达达tab页时，初始化数据
                $('a#aDaDa').on('show.bs.tab', function (e) {
                    e.target // newly activated tab
                    e.relatedTarget // previous active tab

                    //获取微信门店列表
                    getWxPOIList();

                    //获取达达开通城市
                    getDaDaCityCode();

                    //获取达达订单取消原因
                    getDaDaCancelReason();

                    //获取投诉达达原因
                    getComplaintReason()

                    //查询达达订单状态
                    checkDaDaOrderStatus();

                });

                $("#iRefresh").on("click", checkDaDaOrderStatus);

                //Ladda button
                Ladda.bind("#btnPrintPreview");
                Ladda.bind("#btnDelivery");

                //自主配送按钮
                lBtnSelfDelivery = ladda.create(document.querySelector('#btnSelfDelivery'));

                //达达按钮
                lBtnAddDaDaOrder = ladda.create(document.querySelector('#btnAddDaDaOrder'));
                lBtnReAddDaDaOrder = ladda.create(document.querySelector('#btnReAddDaDaOrder'));
                lBtnQueryDeliverFee = ladda.create(document.querySelector('#btnQueryDeliverFee'));
                lBtnAddAfterQuery = ladda.create(document.querySelector('#btnAddAfterQuery'));
                lBtnAddTips = ladda.create(document.querySelector('#btnAddTips'));
                lBtnCancelDaDaOrder = ladda.create(document.querySelector('#btnCancelDaDaOrder'));
                lBtnComplaintDaDa = ladda.create(document.querySelector('#btnComplaintDaDa'));

                //闪送按钮
                lBtnShanSongCalc = ladda.create(document.querySelector('#btnShanSongCalc'));
                lBtnShanSongSave = ladda.create(document.querySelector('#btnShanSongSave'));
                lBtnCancelShanSongOrder = ladda.create(document.querySelector('#btnCancelShanSongOrder'));

            });

        });

        ////加载高德地图
        //requirejs(['amap'], function () {
        //    window.init = function () {
        //        require(['require-initMap'], function (mapIniter) {
        //            mapIniter.init();
        //        });
        //    };
        //});

        //显示配送模态窗
        function showDelivery(orderID) {

            //设置并显示当前处理的订单ID
            $("#divModal").data("orderid", orderID);
            $("#spOrderID").text(orderID);

            //清空地图上的所有覆盖物
            mapForTransporter.clearMap();
            //清空当前的收货人和骑手的地图头像标记
            markerForReceiver = null;
            markerForTransporter = null;

            //打开模态窗时，如果当前页是达达，则刷新达达订单状态
            if ($("#divDaDa").hasClass("active")) {
                //查询订单状态
                checkDaDaOrderStatus();
            }

            requirejs(['niftymodals'], function () {
                $('#divModal').niftyModal({
                    overlaySelector: '.md-overlay',//Modal overlay class
                    closeSelector: '.md-close',//Modal close element class
                    classAddAfterOpen: 'md-show',//Body control class
                    //This object will be available in the modal events
                    data: {

                    },
                    //This option allow to attach a callback to a button with the class 'md-close'
                    buttons: [
                        {
                            class: 'btn-ok',
                            callback: function (btn, modal, event) {
                                //You can cancel the modal hide event by returning false
                                alert("You need to check your info!");
                                return false;
                            }
                        },
                        {
                            class: 'btn-cancel',
                            callback: function (btn, modal, event) {
                                //You can access to the modal data here
                                var modal_data = modal.data.some_data;
                            }
                        }
                    ],
                    beforeOpen: function (modal) {
                        //You can cancel the modal show event by returning false
                    },
                    afterOpen: function (modal) {
                        //Executed after show event
                    },
                    beforeClose: function (modal) {
                        //You can cancel the hide event by returning false
                    },
                    afterClose: function (modal) {
                        //Executed after hide event
                        Ladda.stopAll();

                        //清空当前的定时器
                        if (!!hCheckDaDaOrderStatus) {
                            clearTimeout(hCheckDaDaOrderStatus);
                        }
                    }
                })
            });
        }

        //检查查询开始日期
        function checkStartOrderDate() {
            if ($(this).val() != "" && $("#txtEndOrderDate").val() != "")
                if ($(this).val() > $("#txtEndOrderDate").val()) {
                    alert("开始时间必须早于结束时间。");
                    $(this).val("");
                }
        }

        //检查查询结束日期
        function checkEndOrderDate() {
            if ($(this).val() != "" && $("#txtStartOrderDate").val() != "")
                if ($(this).val() < $("#txtStartOrderDate").val()) {
                    alert("结束时间必须晚于开始时间。");
                    $(this).val("");
                }
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

            $("#divCriterias input").each(function () {
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
                        if (!jPO) {
                            throw new Error("没有获取到订单信息");
                        }
                        if (!!jPO["err_code_des"]) {
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
        }

        //自主配送
        function selfDelivery() {
            lBtnSelfDelivery.start();
            $.ajax({
                url: "OrderHandler.ashx?Action=SelfDelivery",
                data: { OrderID: $("#divModal").data("orderid") },
                type: "POST",
                dataType: "json",
                cache: false,
                success: function (jRet) {
                    try {
                        if (!!jRet && jRet["result"] == 1) {
                            alert("发货成功");
                            location.reload();
                        }
                        else {
                            throw new Error("发货错误");
                        }
                    }
                    catch (error) {
                        alert(error.message);
                    }
                    finally {
                        Ladda.stopAll();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert(textStatus + ":" + errorThrown);
                    Ladda.stopAll();
                }
            });

        }

        //达达订单小费不能小于0
        function verifyTips() {
            if (event.target.value < 0) {
                event.target.value = 0;
            }
        }

        //切换显示其他取消原因
        function showOtherReason() {
            event.target.value == "10000" ? $("#divOtherReason").show() : $("#divOtherReason").hide();
        }

        //获取达达开通的城市
        function getDaDaCityCode() {
            if ($("#ddlCityCode option").length == 0) {
                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=GetCityCode",
                    type: "POST",
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet && !!jRet["result"] && Array.isArray(jRet["result"])) {
                                var options = "";
                                for (var i = 0; i < jRet["result"].length; i++) {
                                    options += "<option value='" + jRet["result"][i]["cityCode"] + "'>" + jRet["result"][i]["cityName"] + "</option>";
                                }
                                $("#ddlCityCode").append(options);
                            }
                            else {
                                throw new Error("获取达达开通城市错误");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                    }
                });
            }
        }

        //获取达达订单取消原因
        function getDaDaCancelReason() {
            if ($("#ddlCancelReason option").length == 0) {
                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=GetCancelReason",
                    type: "POST",
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet && !!jRet["result"] && Array.isArray(jRet["result"])) {
                                var options = "";
                                for (var i = 0; i < jRet["result"].length; i++) {
                                    options += "<option value='" + jRet["result"][i]["id"] + "'>" + jRet["result"][i]["reason"] + "</option>";
                                }
                                $("#ddlCancelReason").append(options);
                            }
                            else {
                                throw new Error("获取达达订单取消原因错误");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                    }
                });
            }
        }

        //获取投诉达达原因
        function getComplaintReason() {
            if ($("#ddlComplaintReason option").length == 0) {
                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=GetComplaintReason",
                    type: "POST",
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet && !!jRet["result"] && Array.isArray(jRet["result"])) {
                                var options = "";
                                for (var i = 0; i < jRet["result"].length; i++) {
                                    options += "<option value='" + jRet["result"][i]["id"] + "'>" + jRet["result"][i]["reason"] + "</option>";
                                }
                                $("#ddlComplaintReason").append(options);
                            }
                            else {
                                throw new Error("获取投诉达达原因错误");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                    }
                });
            }
        }

        //投诉达达
        function complaintDaDa() {
            try {
                lBtnComplaintDaDa.start();
                //校验输入项
                var $ddlComplaintReason = $("#ddlComplaintReason option:checked");

                if (!$ddlComplaintReason.val()) {
                    $ddlComplaintReason.focus();
                    throw new Error("请选择投诉达达原因");
                }

                var complaintDaDaInfo = {
                    OrderID: $("#divModal").data("orderid"),
                    ComplaintReasonID: $ddlComplaintReason.val(),
                };

                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=ComplaintDaDa",
                    type: "POST",
                    data: complaintDaDaInfo,
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet) {
                                switch (jRet["code"]) {
                                    case 0:
                                        alert("投诉达达成功");
                                        break;
                                    default:
                                        throw new Error(jRet["code"] + ":" + jRet["msg"]);
                                        break;
                                }
                            }
                            else {
                                throw new Error("没有获取到投诉达达返回信息");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                        finally {
                            //刷新显示达达订单状态
                            checkDaDaOrderStatus();
                            Ladda.stopAll();
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                        Ladda.stopAll();
                    }
                });
            }
            catch (error) {
                alert(error.message);
                Ladda.stopAll();
            }
        }

        //获取微信门店列表
        function getWxPOIList() {
            if ($("#ddlWxPOIListForDaDa option").length == 0) {
                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=GetWxPOIList",
                    type: "POST",
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet && jRet["errcode"] == "0" && Array.isArray(jRet["business_list"])) {
                                var options = "";
                                options += "<option value='11047059'>测试门店</option>";
                                for (var i = 0; i < jRet["business_list"].length; i++) {
                                    var strData = "";
                                    strData += " data-city=" + jRet["business_list"][i]["base_info"]["city"];
                                    strData += " data-telephone=" + jRet["business_list"][i]["base_info"]["telephone"];
                                    strData += " data-address=" + jRet["business_list"][i]["base_info"]["address"];
                                    strData += " data-lng=" + jRet["business_list"][i]["base_info"]["lng"];
                                    strData += " data-lat=" + jRet["business_list"][i]["base_info"]["lat"];
                                    var strBranchName = !!jRet["business_list"][i]["base_info"]["branch_name"] ? "(" + jRet["business_list"][i]["base_info"]["branch_name"] + ")" : "";
                                    options += "<option value='" + jRet["business_list"][i]["base_info"]["poi_id"] + "'" + strData + "'>" + jRet["business_list"][i]["base_info"]["business_name"] + strBranchName + "</option>";
                                }
                                $("#ddlWxPOIListForDaDa").empty().append(options);
                                $("#ddlWxPOIListForShanSong").empty().append(options);
                            }
                            else {
                                throw new Error("获取微信门店列表错误");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                    }
                });
            }
        }

        //查询指定订单ID的达达状态
        function checkDaDaOrderStatus() {
            try {
                var thisFunc = arguments.callee;

                var orderID = $("#divModal").data("orderid");
                if (!!orderID) {
                    $.ajax({
                        url: "../DaDaServiceHandler.ashx?Action=QueryOrder",
                        data: { OrderID: orderID },
                        type: "POST",
                        dataType: "json",
                        cache: false,
                        success: function (jRet) {
                            try {
                                if (!!jRet) {
                                    switch (jRet["code"]) {
                                        case 0:
                                            //达达订单存在，进一步判断订单状态
                                            if (!!jRet["result"]) {
                                                //根据达达订单状态，设置进度条
                                                switch (jRet["result"]["statusCode"]) {
                                                    case 1: //已下单，待接单
                                                        //显示待接单进度条
                                                        $("#divAcceptOrderBar").text(jRet["result"]["statusMsg"]).addClass("active").attr("title", "下单时间：" + jRet["dadaOrder"]["CreateTime"]).show();
                                                        //关闭待取货进度条
                                                        $("#divFetchOrderBar").hide();
                                                        //关闭配送中进度条
                                                        $("#divDeliveryOrderBar").hide();
                                                        //关闭已完成进度条
                                                        $("#divFinishOrderBar").hide();

                                                        //关闭增加订单界面
                                                        $("#divAddDaDaOrder").hide();

                                                        //关闭订单状态界面
                                                        $("#divDaDaOrderStatus").hide();

                                                        //显示增加小费界面
                                                        $("#divAddTips").show();
                                                        //显示当前小费金额
                                                        $("#lblTips").text(jRet["dadaOrder"]["Tips"] + "元");

                                                        //显示取消订单界面
                                                        $("#divCancelOrder").show();
                                                        $("#divCancelOrderWarning").hide();

                                                        //关闭投诉达达界面
                                                        $("#divComplaintDaDa").hide();

                                                        break;
                                                    case 2: //已接单，待取货
                                                        //显示已下单进度条
                                                        $("#divAcceptOrderBar").text("已接单").removeClass("active").show();
                                                        //显示待取货进度条
                                                        $("#divFetchOrderBar").text(jRet["result"]["statusMsg"]).addClass("active").attr("title", "接单时间:" + jRet["dadaOrder"]["AcceptTime"]).show();
                                                        //关闭配送中进度条
                                                        $("#divDeliveryOrderBar").hide();
                                                        //关闭已完成进度条
                                                        $("#divFinishOrderBar").hide();

                                                        //关闭增加订单界面
                                                        $("#divAddDaDaOrder").hide();

                                                        //关闭增加小费界面
                                                        $("#divAddTips").hide();

                                                        //显示订单状态界面，配送人信息
                                                        $("#divDaDaOrderStatus").show();
                                                        showTransporterInfo(jRet);

                                                        //显示取消订单界面，接单时间
                                                        $("#divCancelOrder").show();
                                                        $("#divCancelOrderWarning").show();
                                                        if (!!jRet["dadaOrder"]["AcceptTime"]) {
                                                            requirejs(["moment"], function (moment) {
                                                                $("#spAcceptTime").text("接单时间：" + moment(jRet["dadaOrder"]["AcceptTime"]).format('HH:mm') + "，");
                                                            });
                                                        }

                                                        //显示投诉达达界面
                                                        $("#divComplaintDaDa").show();

                                                        break;
                                                    case 3: //已取货，配送中
                                                        //显示已下单进度条
                                                        $("#divAcceptOrderBar").text("已接单").removeClass("active").show();
                                                        //显示已接单进度条
                                                        $("#divFetchOrderBar").text("已取货").removeClass("active").show();
                                                        //显示配送中进度条
                                                        $("#divDeliveryOrderBar").text(jRet["result"]["statusMsg"]).addClass("active").attr("title", "取货时间：" + jRet["dadaOrder"]["FetchTime"]).show();
                                                        //关闭已完成进度条
                                                        $("#divFinishOrderBar").hide();

                                                        //关闭增加订单界面
                                                        $("#divAddDaDaOrder").hide();

                                                        //关闭取消订单界面
                                                        $("#divCancelOrder").hide();

                                                        //关闭增加小费界面
                                                        $("#divAddTips").hide();

                                                        //显示订单状态界面，配送人信息
                                                        $("#divDaDaOrderStatus").show();
                                                        showTransporterInfo(jRet);

                                                        //显示投诉达达界面
                                                        $("#divComplaintDaDa").show();

                                                        break;
                                                    case 4: //配送已完成
                                                        //显示已接单进度条
                                                        $("#divAcceptOrderBar").text("已接单").removeClass("active").show();
                                                        //显示已取货进度条
                                                        $("#divFetchOrderBar").text("已取货").removeClass("active").show();
                                                        //显示配送中进度条
                                                        $("#divDeliveryOrderBar").text("已配送").removeClass("active").show();
                                                        //显示已完成进度条
                                                        $("#divFinishOrderBar").text(jRet["result"]["statusMsg"]).addClass("active").attr("title", "完成时间：" + jRet["dadaOrder"]["FinishTime"]).show();

                                                        //关闭增加订单界面
                                                        $("#divAddDaDaOrder").hide();

                                                        //关闭取消订单界面
                                                        $("#divCancelOrder").hide();

                                                        //关闭增加小费界面
                                                        $("#divAddTips").hide();

                                                        //显示订单状态界面，配送人信息
                                                        $("#divDaDaOrderStatus").show();
                                                        showTransporterInfo(jRet);

                                                        //显示投诉达达界面
                                                        $("#divComplaintDaDa").show();

                                                        break;
                                                    case 5: //已取消
                                                        //关闭已下单进度条
                                                        $("#divAcceptOrderBar").hide();
                                                        //关闭已接单进度条
                                                        $("#divFetchOrderBar").hide();
                                                        //关闭配送中进度条
                                                        $("#divDeliveryOrderBar").hide();
                                                        //关闭配送完成进度条
                                                        $("#divFinishOrderBar").hide();

                                                        //显示发单界面
                                                        $("#divAddDaDaOrder").show({ complete: initAddDaDaOrder });

                                                        //关闭新发单按钮
                                                        $("#btnAddDaDaOrder").hide();
                                                        //显示重新发单按钮
                                                        $("#btnReAddDaDaOrder").show();
                                                        //显示查询运费按钮
                                                        $("#btnQueryDeliverFee").show();
                                                        //关闭查询运费后发单按钮
                                                        $("#btnAddAfterQuery").hide();

                                                        //关闭订单状态界面
                                                        $("#divDaDaOrderStatus").hide();

                                                        //关闭取消订单界面
                                                        $("#divCancelOrder").hide();

                                                        //关闭增加小费界面
                                                        $("#divAddTips").hide();

                                                        //显示投诉达达界面
                                                        $("#divComplaintDaDa").show();

                                                        break;
                                                    case 7: //已过期
                                                        //关闭已下单进度条
                                                        $("#divAcceptOrderBar").hide();
                                                        //关闭已接单进度条
                                                        $("#divFetchOrderBar").hide();
                                                        //关闭配送中进度条
                                                        $("#divDeliveryOrderBar").hide();
                                                        //关闭配送完成进度条
                                                        $("#divFinishOrderBar").hide();

                                                        //显示发单界面
                                                        $("#divAddDaDaOrder").show({ complete: initAddDaDaOrder });

                                                        //关闭新发单按钮
                                                        $("#btnAddDaDaOrder").hide();
                                                        //显示重新发单按钮
                                                        $("#btnReAddDaDaOrder").show();
                                                        //显示查询运费按钮
                                                        $("#btnQueryDeliverFee").show();
                                                        //关闭查询运费后发单按钮
                                                        $("#btnAddAfterQuery").hide();

                                                        //关闭订单状态界面
                                                        $("#divDaDaOrderStatus").hide();

                                                        //关闭取消订单界面
                                                        $("#divCancelOrder").hide();

                                                        //关闭增加小费界面
                                                        $("#divAddTips").hide();

                                                        //显示投诉达达界面
                                                        $("#divComplaintDaDa").show();

                                                        break;
                                                    default:
                                                        throw new Error("达达订单状态错误");
                                                }

                                                switch (jRet["result"]["statusCode"]) {
                                                    case 1:
                                                    case 2:
                                                    case 3:
                                                    case 4:
                                                        //待接单、待取货、配送中、已完成状态时，定时刷新达达订单状态
                                                        hCheckDaDaOrderStatus = setTimeout(thisFunc, 5000);
                                                        break;
                                                    default:
                                                        //其他状态，则关闭定时刷新
                                                        if (!!hCheckDaDaOrderStatus) {
                                                            clearTimeout(hCheckDaDaOrderStatus);
                                                        }
                                                        break;
                                                }
                                            } else {
                                                throw new Error("没有返回业务参数result");
                                            }
                                            break;
                                        case 2005:
                                            //达达订单不存在，显示新发单界面
                                            //关闭已下单进度条
                                            $("#divAcceptOrderBar").hide();
                                            //关闭已接单进度条
                                            $("#divFetchOrderBar").hide();
                                            //关闭配送中进度条
                                            $("#divDeliveryOrderBar").hide();
                                            //关闭配送完成进度条
                                            $("#divFinishOrderBar").hide();

                                            //显示发单界面
                                            $("#divAddDaDaOrder").show({ complete: initAddDaDaOrder });
                                            //显示新发单按钮
                                            $("#btnAddDaDaOrder").show();
                                            //关闭重新发单按钮
                                            $("#btnReAddDaDaOrder").hide();
                                            //显示查询运费按钮
                                            $("#btnQueryDeliverFee").show();
                                            //关闭查询运费后发单按钮
                                            $("#btnAddAfterQuery").hide();

                                            //关闭订单状态界面
                                            $("#divDaDaOrderStatus").hide();
                                            //关闭增加小费界面
                                            $("#divAddTips").hide();
                                            //关闭取消订单界面
                                            $("#divCancelOrder").hide();
                                            //关闭投诉达达界面
                                            $("#divComplaintDaDa").hide();

                                            //关闭定时刷新
                                            if (!!hCheckDaDaOrderStatus) {
                                                clearTimeout(hCheckDaDaOrderStatus);
                                            }
                                            break;
                                        default:
                                            //其他错误，显示新发单界面
                                            //关闭已下单进度条
                                            $("#divAcceptOrderBar").hide();
                                            //关闭已接单进度条
                                            $("#divFetchOrderBar").hide();
                                            //关闭配送中进度条
                                            $("#divDeliveryOrderBar").hide();
                                            //关闭配送完成进度条
                                            $("#divFinishOrderBar").hide();

                                            //显示发单界面
                                            $("#divAddDaDaOrder").show({ complete: initAddDaDaOrder });
                                            //显示新发单按钮
                                            $("#btnAddDaDaOrder").show();
                                            //关闭重新发单按钮
                                            $("#btnReAddDaDaOrder").hide();
                                            //显示查询运费按钮
                                            $("#btnQueryDeliverFee").show();
                                            //关闭查询运费后发单按钮
                                            $("#btnAddAfterQuery").hide();

                                            //关闭订单状态界面
                                            $("#divDaDaOrderStatus").hide();
                                            //关闭增加小费界面
                                            $("#divAddTips").hide();
                                            //关闭取消订单界面
                                            $("#divCancelOrder").hide();
                                            //关闭投诉达达界面
                                            $("#divComplaintDaDa").hide();

                                            //关闭定时刷新
                                            if (!!hCheckDaDaOrderStatus) {
                                                clearTimeout(hCheckDaDaOrderStatus);
                                            }

                                            throw new Error(jRet["code"] + ":" + jRet["msg"]);
                                            break;
                                    }

                                }
                                else {
                                    throw new Error("没有获取到达达订单信息");
                                }
                            }
                            catch (error) {
                                alert(error.message);
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert(textStatus + ":" + errorThrown);
                        }
                    });
                }
                else {
                    throw new Error("没有指定订单ID");
                }
            }
            catch (error) {
                alert(error.message);
            }

        }

        //在地图上显示收货人、骑手信息
        function showTransporterInfo(jRet) {
            try {
                //显示收货人头像标记
                if (!markerForReceiver) {
                    if (!!jRet["dadaOrder"]["ProductOrder"]["Purchaser"]) {
                        var iconForReceiver = new AMap.Icon({
                            image: jRet["dadaOrder"]["ProductOrder"]["Purchaser"]["HeadImgUrl"],
                            imageSize: new AMap.Size(36, 36)
                        });
                        markerForReceiver = new AMap.Marker({
                            icon: iconForReceiver,
                            offset: new AMap.Pixel(-18, -18),
                            position: [jRet["dadaOrder"]["ReceiverLng"], jRet["dadaOrder"]["ReceiverLat"]],
                            title: jRet["dadaOrder"]["ProductOrder"]["DeliverName"],
                            map: mapForTransporter
                        });
                    }
                }

                //显示骑手姓名电话
                if (!!jRet["result"]["transporterName"] && !!jRet["result"]["transporterPhone"]) {
                    $("#pTransporterInfo").text(jRet["result"]["transporterName"] + "(" + jRet["result"]["transporterPhone"] + ")");
                } else {
                    if (!!jRet["result"]["transporterName"] && !jRet["result"]["transporterPhone"]) {
                        $("#pTransporterInfo").text(jRet["result"]["transporterName"]);
                    }
                }

                //显示骑手头像标记，骑手位置会动态变化，所以每次调用时需要刷新地图位置
                if (!!jRet["result"]["transporterLng"] && !!jRet["result"]["transporterLat"]) {
                    mapForTransporter.panTo(new AMap.LngLat(jRet["result"]["transporterLng"], jRet["result"]["transporterLat"]));

                    if (!markerForTransporter) {
                        var iconForTransporter = new AMap.Icon({
                            image: '../images/dada.png',
                            imageSize: new AMap.Size(36, 36)
                        });
                        markerForTransporter = new AMap.Marker({
                            icon: iconForTransporter,
                            offset: new AMap.Pixel(-18, -18),
                            animation: "AMAP_ANIMATION_BOUNCE",
                            position: [jRet["result"]["transporterLng"], jRet["result"]["transporterLat"]],
                            title: !!jRet["result"]["transporterName"] ? jRet["result"]["transporterName"] : "达达骑手",
                            map: mapForTransporter
                        });
                    } else {
                        markerForTransporter.setPosition(new AMap.LngLat(jRet["result"]["transporterLng"], jRet["result"]["transporterLat"]));
                    }
                }
            }
            catch (error) {
                alert(error.message);
            }
        }

        //新增订单
        function addOrder(addOrderType) {
            try {
                //根据新增订单类型，设置Action
                var addOrderAction;
                switch (addOrderType) {
                    case 1:
                        //新增订单
                        addOrderAction = "AddOrder";
                        lBtnAddDaDaOrder.start();
                        break;
                    case 2:
                        //重新发单
                        addOrderAction = "ReAddOrder";
                        lBtnReAddDaDaOrder.start();
                        break;
                    case 3:
                        //查询运费后发单
                        addOrderAction = "QueryDeliverFee";
                        lBtnQueryDeliverFee.start();
                        break;
                    default:
                        throw new Error("新增订单类型错误");
                }

                //校验输入项
                if (!$("#ddlWxPOIListForDaDa").val()) {
                    $("#ddlWxPOIListForDaDa").focus();
                    throw new Error("请选择门店");
                }

                if (!$("#ddlCityCode").val()) {
                    $("#ddlCityCode").focus();
                    throw new Error("请选择城市");
                }

                if (!!$("#txtTips").val() && isNaN($("#txtTips").val())) {
                    $("#txtTips").focus();
                    throw new Error("请输入正确的小费金额");
                }

                if (!$("#txtExpectedFetchTime").val()) {
                    $("#txtExpectedFetchTime").focus();
                    throw new Error("请输入期望取货时间");
                }

                if (!$("#txtReceiverAddress").data("lng") || !$("#txtReceiverAddress").data("lat")) {
                    $("#txtReceiverAddress").focus();
                    throw new Error("请选择配送地址，并在地图上标示");
                }

                //达达订单提交信息
                var dadaOrderInfo = {
                    OrderID: $("#divModal").data("orderid"),
                    shop_no: $("#ddlWxPOIListForDaDa").val(),
                    city_code: $("#ddlCityCode").val(),
                    tips: $("#txtTips").val().trim(),
                    info: $("#txtInfo").val().trim(),
                    is_prepay: $("#cbIsPrepay").is(":checked") ? true : false,
                    expected_fetch_time: $("#txtExpectedFetchTime").val(),
                    expected_finish_time: $("#txtExpectedFinishTime").val(),
                    receiver_lat: $("#txtReceiverAddress").data("lat"),
                    receiver_lng: $("#txtReceiverAddress").data("lng")
                };

                //调用新增订单API
                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=" + addOrderAction,
                    type: "POST",
                    data: dadaOrderInfo,
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet) {
                                switch (jRet["code"]) {
                                    case 0:
                                        if (!!jRet["result"]) {
                                            if (!isNaN(jRet["result"]["distance"])) {
                                                $("#lblDistance").text((jRet["result"]["distance"] / 1000).toFixed(2) + "公里");
                                                $("#divDistance").show();
                                            } else {
                                                throw new Error("送货距离错误");
                                            }
                                            if (!isNaN(jRet["result"]["fee"])) {
                                                $("#lblFee").text(jRet["result"]["fee"] + "元");
                                                $("#divFee").show();
                                            } else {
                                                throw new Error("运费错误");
                                            }

                                            //如果是订单预发布，则保存达达返回的平台订单编号，并显示查询后发单按钮
                                            if (!!jRet["result"]["deliveryNo"]) {
                                                $("#divAddDaDaOrder").data("delivery-no", jRet["result"]["deliveryNo"]);
                                                $("#btnAddDaDaOrder").hide();
                                                $("#btnReAddDaDaOrder").hide();
                                                $("#btnQueryDeliverFee").show();
                                                $("#btnAddAfterQuery").show();
                                            }

                                            switch (addOrderType) {
                                                case 1:
                                                    alert("新增订单成功");
                                                    //新增订单后，3秒后刷新显示达达订单状态
                                                    setTimeout(checkDaDaOrderStatus, 3000);
                                                    break;
                                                case 2:
                                                    alert("重新发单成功");
                                                    //重新发单后，3秒后刷新显示达达订单状态
                                                    setTimeout(checkDaDaOrderStatus, 3000);
                                                    break;
                                                case 3:
                                                    alert("查询运费成功，有效期3分钟");
                                                    $("#btnAddAfterQuery").focus();
                                                    break;
                                            }

                                        } else {
                                            throw new Error("没有获取到新增订单返回信息result");
                                        }

                                        break;
                                    default:
                                        throw new Error(jRet["code"] + ":" + jRet["msg"]);
                                        break;
                                }
                            }
                            else {
                                throw new Error("没有获取到新增订单返回信息");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                        finally {
                            Ladda.stopAll();
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                        Ladda.stopAll();
                    }
                });
            }
            catch (error) {
                alert(error.message);
                Ladda.stopAll();
            }
        }

        //查询运费后发单
        function addAfterQuery() {
            try {
                lBtnAddAfterQuery.start();
                var deliveryNo = $("#divAddDaDaOrder").data("delivery-no");
                if (!!deliveryNo) {
                    $.ajax({
                        url: "../DaDaServiceHandler.ashx?Action=AddAfterQuery",
                        type: "POST",
                        data: { deliveryNo: deliveryNo },
                        dataType: "json",
                        cache: false,
                        success: function (jRet) {
                            try {
                                if (!!jRet) {
                                    switch (jRet["code"]) {
                                        case 0:
                                            alert("发单成功");
                                            //发单成功后，3秒后刷新显示达达订单状态
                                            setTimeout(checkDaDaOrderStatus, 3000);
                                            break;
                                        default:
                                            throw new Error(jRet["code"] + ":" + jRet["msg"]);
                                            break;
                                    }
                                }
                                else {
                                    throw new Error("没有获取到新增订单返回信息");
                                }
                            }
                            catch (error) {
                                alert(error.message);
                            }
                            finally {
                                Ladda.stopAll();
                            }
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            alert(textStatus + ":" + errorThrown);
                            Ladda.stopAll();
                        }
                    });
                }
                else {
                    throw new Error("没有获取到达达平台订单编号，请重新查询");
                }
            }
            catch (error) {
                alert(error.message);
                Ladda.stopAll();
            }
        }

        //取消订单
        function formalCancel() {
            try {
                lBtnCancelDaDaOrder.start();
                //校验输入项
                var $ddlCancelReason = $("#ddlCancelReason option:checked");
                var $txtOtherReason = $("#txtOtherReason");

                if (!$ddlCancelReason.val()) {
                    $ddlCancelReason.focus();
                    throw new Error("请选择取消订单原因");
                }

                if ($ddlCancelReason.val() == "10000" && !$txtOtherReason.val()) {
                    $txtOtherReason.focus();
                    throw new Error("请填写其他原因");
                }

                var formalCancelInfo = {
                    OrderID: $("#divModal").data("orderid"),
                    cancel_reason_id: $ddlCancelReason.val(),
                    cancel_reason: $ddlCancelReason.val() != "10000" ? $ddlCancelReason.text() : $txtOtherReason.val().trim()
                };

                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=CancelOrder",
                    type: "POST",
                    data: formalCancelInfo,
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet) {
                                switch (jRet["code"]) {
                                    case 0:
                                        $("#pDeductFee").text(jRet["result"]["deduct_fee"] + "元");
                                        $("#divDeductFee").show();
                                        alert("订单取消成功");
                                        break;
                                    default:
                                        throw new Error(jRet["code"] + ":" + jRet["msg"]);
                                        break;
                                }
                            }
                            else {
                                throw new Error("没有获取到取消订单返回信息");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                        finally {
                            //刷新显示达达订单状态
                            checkDaDaOrderStatus();
                            Ladda.stopAll();
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                        Ladda.stopAll();
                    }
                });
            }
            catch (error) {
                alert(error.message);
                Ladda.stopAll();
            }
        }

        //增加小费
        function addTips() {
            try {
                lBtnAddTips.start();
                //校验输入项
                var addTips = $("#txtAddTips").val().trim();
                var tips = $("#lblTips").text();

                if (isNaN(addTips) || addTips <= tips) {
                    $("#txtAddTips").val("").focus();
                    throw new Error("新小费必须大于原小费");
                }

                var addTipInfo = {
                    OrderID: $("#divModal").data("orderid"),
                    addTips: $("#txtAddTips").val().trim(),
                    city_code: $("#ddlCityCode").val(),
                    info: $("#txtInfoForAddTips").val().trim().substr(0, 512)
                };

                $.ajax({
                    url: "../DaDaServiceHandler.ashx?Action=AddTip",
                    type: "POST",
                    data: addTipInfo,
                    dataType: "json",
                    cache: false,
                    success: function (jRet) {
                        try {
                            if (!!jRet) {
                                switch (jRet["code"]) {
                                    case 0:
                                        //显示新小费
                                        alert("增加小费成功");
                                        $("#lblTips").text(jRet["addTips"] + "元");
                                        break;
                                    default:
                                        throw new Error(jRet["code"] + ":" + jRet["msg"]);
                                        break;
                                }
                            }
                            else {
                                throw new Error("没有获取到增加小费返回信息");
                            }
                        }
                        catch (error) {
                            alert(error.message);
                        }
                        finally {
                            Ladda.stopAll();
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(textStatus + ":" + errorThrown);
                        Ladda.stopAll();
                    }
                });
            }
            catch (error) {
                alert(error.message);
                Ladda.stopAll();
            }
        }

        //初始化达达发单界面
        function initAddDaDaOrder() {

            //预置达达订单取货时间
            requirejs(["moment"], function (moment) {
                $("#txtExpectedFetchTime").val(moment().add(30, 'minute').format('HH:mm'));
            });

            //清空上次选择的收货人地址经纬度
            $("#txtReceiverAddress").data("lng", "").data("lat", "");

            //清空上次下单返回的送货距离和运费
            $("#lblDistance").text("");
            $("#divDistance").hide();
            $("#lblFee").text("");
            $("#divFee").hide();

            //清空上次撤销订单返回的扣除违约金
            $("#pDeductFee").text("");
            $("#divDeductFee").hide();

            var autoComplete, placeSearch, marker;

            //初始化收货人地图
            var map = new AMap.Map("divMapForReceiver", {
                resizeEnable: true,
                zoom: 15
            });

            //为地图注册click事件获取鼠标点击出的经纬度坐标
            map.on('click', function (e) {
                $("#txtReceiverAddress").data("lng", e.lnglat.getLng()).data("lat", e.lnglat.getLat());
                if (!marker) {
                    marker = new AMap.Marker({
                        position: [e.lnglat.getLng(), e.lnglat.getLat()],
                        title: '收货人地址',
                        map: map
                    });
                } else {
                    marker.setPosition(new AMap.LngLat(e.lnglat.getLng(), e.lnglat.getLat()));
                }
            });

            AMap.plugin(['AMap.Autocomplete', 'AMap.PlaceSearch'], function () {
                //实例化Autocomplete
                autoComplete = new AMap.Autocomplete({
                    city: "上海", //城市，默认全国
                    input: "txtReceiverAddress"//使用联想输入的input的id
                });

                placeSearch = new AMap.PlaceSearch({
                    city: "上海",
                    map: map
                });

                //注册监听，当选中某条记录时会触发
                autoComplete.on("select", function (e) {
                    if (e.poi && e.poi.location) {
                        map.setZoom(15);
                        map.setCenter(e.poi.location);
                        placeSearch.search(e.poi.name);

                        marker = new AMap.Marker({
                            position: e.lnglat,
                            title: '收货人地址',
                            map: map
                        });
                    }
                });
            });

            //根据选择城市设置地图中心点
            $("#ddlCityCode").on("change", function () {
                map.setCity(event.target.value);
            });

        }

    </script>
</asp:Content>
