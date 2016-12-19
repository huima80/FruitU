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
                    <div class="col-lg-2">
                    </div>
                    <div class="col-lg-10 text-right" id="divCriterias">
                        <div class="form-group">
                            <label for="btnSearch" class="sr-only">按团购名称查询</label>
                            <asp:TextBox ID="txtGroupPurchaseName" runat="server" placeholder="请输入团购名称..." CssClass="form-control"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-info" Text="查询" OnClientClick="return verifyCriteria();" OnClick="btnSearch_Click" />
                        <asp:Button ID="btnShowAll" runat="server" Text="全部团购" CssClass="btn btn-warning" OnClick="btnShowAll_Click" />
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12">
                <asp:GridView ID="gvGroupPurchaseList" runat="server" AutoGenerateColumns="False" DataSourceID="odsGroupPurchaseList" AllowPaging="True" CssClass="table table-striped table-hover table-responsive" AllowCustomPaging="True" PagerStyle-CssClass="pager" OnRowDataBound="gvGroupPurchaseList_RowDataBound" DataKeyNames="ID" OnRowDeleted="gvGroupPurchaseList_RowDeleted" OnSelectedIndexChanged="gvGroupPurchaseList_SelectedIndexChanged">
                    <Columns>
                        <asp:CommandField ShowDeleteButton="True" />
                        <asp:TemplateField HeaderText="团购商品" SortExpression="Product">
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# "【"+Eval("Product.Category.CategoryName")+"】"+Eval("Product.FruitName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Name" HeaderText="团购名称" SortExpression="Name" />
                        <asp:BoundField DataField="Description" HeaderText="团购描述" SortExpression="Description" />
                        <asp:BoundField DataField="GroupPrice" HeaderText="团购价格" SortExpression="GroupPrice" DataFormatString="{0:c}" />
                        <asp:TemplateField HeaderText="团购起止日期" SortExpression="StartDate">
                            <ItemTemplate>
                                <p title="团购开始时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="lblStartDate" runat="server" Text='<%# Eval("StartDate")%>'></asp:Label></p>
                                <p title="团购结束时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="Label3" runat="server" Text='<%# Eval("EndDate") %>'></asp:Label></p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="RequiredNumber" HeaderText="团购要求人数" SortExpression="RequiredNumber" />
                        <asp:TemplateField HeaderText="建团数量" SortExpression="GroupEvents">
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Eval("GroupEvents.Count") %>'></asp:Label>
                                <asp:DataList ID="dlGroupEvents" runat="server" RepeatLayout="Flow" ShowFooter="False" DataSource='<%# Eval("GroupEvents") %>'>
                                    <ItemTemplate>
                                        <asp:DataList ID="dlGroupPurchaseEventMembers" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" ShowFooter="False" DataSource='<%# Eval("GroupPurchaseEventMembers") %>' OnItemDataBound="dlGroupPurchaseEventMembers_ItemDataBound">
                                            <ItemTemplate>
                                                <p class="user-portrait">
                                                    <asp:Image ID="imgEventMember" runat="server" ImageUrl='<%# Eval("GroupMember.HeadImgUrl") %>' AlternateText='<%# Eval("GroupMember.NickName") %>' ToolTip='<%# "参团时间："+Eval("JoinDate") %>' /><span runat="server" id="spPaid" class="event-member-paid"></span>
                                                </p>
                                                <p>
                                                    <asp:Label ID="lblNickName" runat="server" Text='<%# Eval("GroupMember.NickName") %>'></asp:Label>
                                                </p>
                                            </ItemTemplate>
                                        </asp:DataList>
                                        <asp:HyperLink ID="hlViewPORelated" runat="server" NavigateUrl='<%# "ManageOrder.aspx?GroupEventID="+Eval("ID") %>' ToolTip="查看此团购活动的所有订单"><i class="fa fa-files-o fa-lg"></i></asp:HyperLink>
                                    </ItemTemplate>
                                </asp:DataList>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:CommandField ButtonType="Button" ShowSelectButton="True">
                            <ControlStyle CssClass="btn btn-default" />
                        </asp:CommandField>
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
            <div class="col-lg-12">
                <asp:DetailsView ID="dvGroupPurchase" CssClass="table table-responsive table-condensed" runat="server" AutoGenerateRows="False" DataSourceID="odsGroupPurchase" GridLines="Horizontal" OnItemInserted="dvGroupPurchase_ItemInserted" OnItemInserting="dvGroupPurchase_ItemInserting" OnItemUpdating="dvGroupPurchase_ItemUpdating" OnItemUpdated="dvGroupPurchase_ItemUpdated" OnItemCommand="dvGroupPurchase_ItemCommand">
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
                        <asp:TemplateField HeaderText="团购起始日期" SortExpression="StartDate">
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
                <asp:Label ID="lblErrMsg" runat="server"></asp:Label>
            </div>
        </div>
    </div>
    <script>
        requirejs(['jquery', 'jqueryui'], function ($) {

            $(function () {
                theForm.onsubmit = jumpToGrid;

                requirejs(['datepickerCN'], function () {

                    //http://api.jqueryui.com/datepicker/
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

            if (event && event.currentTarget && (event.currentTarget.value == '选择')) {
                var dvGroupPurchase = $("table[id*=dvGroupPurchase]").attr("id");
                theForm.action += "#" + dvGroupPurchase;
            }
        }

    </script>
</asp:Content>

