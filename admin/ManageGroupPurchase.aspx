<%@ Page Title="团购管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="ManageGroupPurchase.aspx.cs" Inherits="admin_ManageGroupPurchase" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/ManageGroupPurchase.css" rel="stylesheet" />
    <link href="../Scripts/gridstack/gridstack-0.2.4.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container-fluid">
        <div class="panel panel-info">
            <div class="panel-heading text-center">
                <h2 class="panel-title">团购查询</h2>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-lg-7">
                        <ul>
                            <li>每个商品可以创建多个团购，但只有在有效期内最新的一个团购<span class="bg-success">（绿底色）</span>有效。</li>
                            <li>用户可在每个团购下创建多个团购活动，并在团购有效期内拼团。</li>
                            <li>团购活动拼团成功为<span class="bg-success">绿底色</span>，进行中为<span class="bg-info">蓝底色</span>，失败为<span class="bg-danger">粉红底色</span>。</li>
                            <li>当拼团成功，或超过有效期拼团失败，将给管理员和用户发送微信消息。</li>
                        </ul>
                    </div>
                    <div class="col-lg-5 text-right" id="divCriterias">
                        <div class="form-group">
                            <label for="txtGroupPurchaseName" class="sr-only">按团购名称查询</label>
                            <asp:TextBox ID="txtGroupPurchaseName" runat="server" placeholder="请输入团购名称..." CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="form-group">
                            <label for="txtGroupEventID" class="sr-only">按团购活动ID查询</label>
                            <asp:TextBox ID="txtGroupEventID" runat="server" placeholder="请输入团购活动ID..." CssClass="form-control"></asp:TextBox>
                        </div>
                        <br />
                        <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-info" Text="查询" OnClientClick="return verifyCriteria();" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnShowAll" runat="server" Text="全部团购" CssClass="btn btn-warning" OnClick="btnShowAll_Click" />
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12 table-responsive">
                <asp:GridView ID="gvGroupPurchaseList" runat="server" AutoGenerateColumns="False" DataSourceID="odsGroupPurchaseList" AllowPaging="True" CssClass="table" AllowCustomPaging="True" PagerStyle-CssClass="pager" OnRowDataBound="gvGroupPurchaseList_RowDataBound" DataKeyNames="ID" OnRowDeleted="gvGroupPurchaseList_RowDeleted" OnSelectedIndexChanged="gvGroupPurchaseList_SelectedIndexChanged">
                    <Columns>
                        <asp:TemplateField ItemStyle-CssClass="col-xs-11">
                            <HeaderTemplate>
                                <div class="row">
                                    <div class="col-xs-3">团购商品</div>
                                    <div class="col-xs-5">团购信息</div>
                                    <div class="col-xs-2">起止日期</div>
                                    <div class="col-xs-2">团购活动</div>
                                </div>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <div class="row">
                                    <div class="col-xs-3">
                                        <asp:Label ID="Label2" runat="server" Text='<%# "【"+Eval("Product.Category.CategoryName")+"】"+Eval("Product.FruitName") %>'></asp:Label>
                                    </div>
                                    <div class="col-xs-5">
                                        <div>
                                            <asp:Label ID="Label9" runat="server" Text='<%# Eval("Name") %>' CssClass="group-event-name"></asp:Label>
                                        </div>
                                        <div>
                                            <asp:Label ID="Label1" runat="server" Text='<%# Eval("Description") %>'></asp:Label>
                                        </div>
                                        <div>
                                            <asp:Label ID="Label4" CssClass="group-price" runat="server" Text='<%# Eval("RequiredNumber") +"人团 " + Eval("GroupPrice", "{0:c2}") + "元/"+Eval("Product.FruitUnit")%>'></asp:Label>
                                        </div>
                                    </div>
                                    <div class="col-xs-2">
                                        <p title="团购开始时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="Label12" runat="server" Text='<%# Eval("StartDate")%>'></asp:Label></p>
                                        <p title="团购结束时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="Label13" runat="server" Text='<%# Eval("EndDate") %>'></asp:Label></p>
                                    </div>
                                    <div class="col-xs-2">
                                        <button type="button" class="btn btn-warning" onclick="showGroupEvents()" <%# Eval("GroupEvents.Count").ToString() == "0" ? "disabled='disabled'" : "" %>>
                                            团购活动
                                                <span class="badge"><%# Eval("GroupEvents.Count") %></span>
                                        </button>
                                    </div>
                                </div>
                                <ul class="list-group">
                                    <asp:Repeater ID="rpGroupEvents" runat="server" DataSource='<%# Eval("GroupEvents") %>' OnItemDataBound="rpGroupEvents_ItemDataBound">
                                        <ItemTemplate>
                                            <li class="list-group-item" runat="server" id="liGroupItem">
                                                <asp:Label ID="lblEventID" runat="server" Text='<%# "#"+Eval("ID") %>' CssClass="badge"></asp:Label>
                                                <asp:Repeater ID="rpGroupEventMembers" runat="server" DataSource='<%# Eval("GroupPurchaseEventMembers") %>'>
                                                    <ItemTemplate>
                                                        <asp:Image ID="imgEventMember" runat="server" CssClass="img-thumbnail user-portrait" ImageUrl='<%# Eval("GroupMember.HeadImgUrl") %>' AlternateText='<%# Eval("GroupMember.NickName") %>' ToolTip='<%# Eval("GroupMember.NickName")+"\n"+"参团时间："+Eval("JoinDate")+"\n"+((bool)Eval("IsPaid")?"已支付":"未支付") %>' />
                                                    </ItemTemplate>
                                                </asp:Repeater>
                                                <asp:Label ID="lblEventStatus" runat="server" CssClass="event-status"></asp:Label>
                                                <asp:Label ID="lblLaunchDate" runat="server" CssClass="launch-date" Text='<%# "活动发起时间："+Eval("LaunchDate") %>'></asp:Label>
                                                <asp:HyperLink ID="hlViewPORelated" runat="server" CssClass="view-po-related" NavigateUrl='<%# "ManageOrder.aspx?GroupEventID="+Eval("ID") %>' ToolTip="查看此团购活动的所有订单"><i class="fa fa-files-o fa-lg"></i></asp:HyperLink>
                                            </li>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </ul>
                            </ItemTemplate>
                            <ItemStyle CssClass="col-xs-11"></ItemStyle>
                        </asp:TemplateField>
                        <asp:TemplateField ItemStyle-CssClass="col-xs-1" HeaderText="团购操作">
                            <ItemTemplate>
                                <asp:Button ID="btnDel" runat="server" CausesValidation="False" CommandName="Delete" Text="删除" CssClass="btn btn-sm btn-block btn-default" />
                                <asp:Button ID="btnModify" runat="server" CausesValidation="False" CommandName="Select" Text="修改" CssClass="btn btn-sm btn-block btn-default" OnClientClick="jumpToGrid();" />
                            </ItemTemplate>
                            <ItemStyle CssClass="col-xs-1"></ItemStyle>
                        </asp:TemplateField>
                    </Columns>
                    <PagerStyle CssClass="pager"></PagerStyle>
                    <SelectedRowStyle CssClass="danger" />
                </asp:GridView>
                <asp:ObjectDataSource ID="odsGroupPurchaseList" runat="server" SelectMethod="FindGroupPurchasePager" TypeName="GroupPurchase" EnablePaging="True" SelectCountMethod="FindGroupCount" DeleteMethod="DelGroupPurchase" OnSelecting="odsGroupPurchaseList_Selecting" OnDeleted="odsGroupPurchaseList_Deleted">
                    <DeleteParameters>
                        <asp:Parameter Name="id" Type="Int32" />
                    </DeleteParameters>
                    <SelectParameters>
                        <asp:Parameter Name="strWhere" Type="String" />
                        <asp:Parameter Name="strOrder" Type="String" />
                        <asp:Parameter Name="totalRows" Type="Int32" Direction="Output" />
                        <asp:Parameter Name="startRowIndex" Type="Int32" />
                        <asp:Parameter Name="maximumRows" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12 table-responsive">
                <asp:DetailsView ID="dvGroupPurchase" CssClass="table table-condensed" runat="server" AutoGenerateRows="False" DataSourceID="odsGroupPurchase" GridLines="Horizontal" OnItemInserted="dvGroupPurchase_ItemInserted" OnItemInserting="dvGroupPurchase_ItemInserting" OnItemUpdating="dvGroupPurchase_ItemUpdating" OnItemUpdated="dvGroupPurchase_ItemUpdated" OnItemCommand="dvGroupPurchase_ItemCommand">
                    <FieldHeaderStyle CssClass="col-xs-2 col-sm-2 col-md-2 col-lg-2"></FieldHeaderStyle>
                    <Fields>
                        <asp:BoundField DataField="ID" HeaderText="ID" SortExpression="ID" InsertVisible="False" ReadOnly="True" />
                        <asp:TemplateField HeaderText="团购商品">
                            <EditItemTemplate>
                                <asp:Label ID="lblProductName" runat="server" Text='<%# "【"+Eval("Product.Category.CategoryName")+"】"+Eval("Product.FruitName") %>'></asp:Label>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:HiddenField ID="hfProductID" runat="server" Value='<%# Bind("ProductID") %>' />
                                <asp:Label ID="lblProductName" runat="server" Text=""></asp:Label>
                            </InsertItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Name" HeaderText="团购名称" SortExpression="Name">
                            <ControlStyle CssClass="form-control" />
                        </asp:BoundField>
                        <asp:BoundField DataField="Description" HeaderText="团购描述" SortExpression="Description">
                            <ControlStyle CssClass="form-control" />
                        </asp:BoundField>
                        <asp:TemplateField HeaderText="团购起止日期" SortExpression="StartDate">
                            <EditItemTemplate>
                                <asp:TextBox ID="txtStartDate" runat="server" size="10" Text='<%# Bind("StartDate") %>' placeholder="团购开始日期" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                                至
                                <asp:TextBox ID="txtEndDate" runat="server" size="10" Text='<%# Bind("EndDate") %>' placeholder="团购结束日期" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:TextBox ID="txtStartDate" runat="server" size="10" Text='<%# Bind("StartDate") %>' placeholder="团购开始日期" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                                至
                               <asp:TextBox ID="txtEndDate" runat="server" size="10" Text='<%# Bind("EndDate") %>' placeholder="团购结束日期" CssClass="form-control" ClientIDMode="Static"></asp:TextBox>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblStartDate" runat="server" Text='<%# Eval("StartDate") %>'></asp:Label>
                                至
                                <asp:Label ID="lblEndDate" runat="server" Text='<%# Eval("EndDate") %>'></asp:Label>
                            </ItemTemplate>
                            <ControlStyle CssClass="form-control" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="RequiredNumber" HeaderText="团购要求人数" SortExpression="RequiredNumber">
                            <ControlStyle CssClass="form-control" />
                        </asp:BoundField>
                        <asp:BoundField DataField="GroupPrice" HeaderText="团购价格" SortExpression="GroupPrice">
                            <ControlStyle CssClass="form-control" />
                        </asp:BoundField>
                    </Fields>
                </asp:DetailsView>
                <asp:ObjectDataSource ID="odsGroupPurchase" runat="server" DataObjectTypeName="GroupPurchase" InsertMethod="AddGroupPurchase" SelectMethod="FindGroupPurchaseByID" TypeName="GroupPurchase" UpdateMethod="UpdateGroupPurchase" OnInserted="odsGroupPurchase_Inserted" OnUpdated="odsGroupPurchase_Updated">
                    <SelectParameters>
                        <asp:Parameter Name="id" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
                <asp:Label ID="lblErrMsg" runat="server" EnableViewState="False" ForeColor="Red"></asp:Label>
            </div>
        </div>
    </div>
    <script>
        requirejs(['jquery', 'jqueryui'], function ($) {

            $(function () {

                //http://api.jqueryui.com/datepicker/
                requirejs(['datepickerCN'], function () {
                    $("#txtStartDate").datepicker({ dateFormat: 'yy-mm-dd' });
                    $("#txtEndDate").datepicker({ dateFormat: 'yy-mm-dd' });

                    $("#txtStartDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtEndDate").val() != "")
                            if ($(this).val() > $("#txtEndDate").val()) {
                                alert("开始时间必须早于结束时间。");
                                $(this).val("");
                            }
                    });
                    $("#txtEndDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtStartDate").val() != "")
                            if ($(this).val() < $("#txtStartDate").val()) {
                                alert("结束时间必须晚于开始时间。");
                                $(this).val("");
                            }
                    });
                });
            });

        });

        //点击新增商品或选择按钮时，页面跳到DetailView
        function jumpToGrid() {
            if (event && event.currentTarget && (event.currentTarget.value == '修改')) {
                var dvGroupPurchase = $("table[id*=dvGroupPurchase]").attr("id");
                theForm.action += "#" + dvGroupPurchase;
            }
        }

        //显示团购活动列表
        function showGroupEvents() {
            var rpGroupEvents = $(event.target).parents("div.row:first").next();
            if (!!rpGroupEvents) {
                rpGroupEvents.slideToggle();
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
</asp:Content>

