<%@ Page Title="商品管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="ManageProduct.aspx.cs" Inherits="ManageProduct" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="../css/ManageProduct.css" rel="stylesheet" />
    <link href="../Scripts/gridstack/gridstack-0.2.4.css" rel="stylesheet" />
    <style>
        .fa-check {
            color: red;
            font-size: 20px;
        }

        .fa-close {
            color: grey;
            font-size: 20px;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container-fluid">
        <div class="panel panel-info">
            <div class="panel-heading text-center">
                <h2 class="panel-title">商品查询</h2>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-lg-2">
                        <asp:Button ID="btnAddFruit" runat="server" Text="新增商品" OnClick="btnAddFruit_Click" CssClass="btn btn-danger" OnClientClick="jumpToGrid();" />
                    </div>
                    <div class="col-lg-10 text-right" id="divCriterias">
                        <div class="form-group">
                            <label for="ddlCategory" class="sr-only">按商品类别查询</label>
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <label for="ddlOutOfStock" class="sr-only">按是否缺货查询</label>
                            <asp:DropDownList ID="ddlOutOfStock" runat="server" CssClass="form-control">
                                <asp:ListItem Selected="True" Value="-1">===是否缺货===</asp:ListItem>
                                <asp:ListItem Value="1">缺货</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <label for="ddlIsOnSale" class="sr-only">按是否上架查询</label>
                            <asp:DropDownList ID="ddlIsOnSale" runat="server" CssClass="form-control">
                                <asp:ListItem Selected="True" Value="-1">===是否上架===</asp:ListItem>
                                <asp:ListItem Value="1">已上架</asp:ListItem>
                                <asp:ListItem Value="0">未上架</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <label for="ddlIsSticky" class="sr-only">按是否置顶查询</label>
                            <asp:DropDownList ID="ddlIsSticky" runat="server" CssClass="form-control">
                                <asp:ListItem Selected="True" Value="-1">===是否置顶===</asp:ListItem>
                                <asp:ListItem Value="1">置顶</asp:ListItem>
                                <asp:ListItem Value="0">未置顶</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="form-group">
                            <label for="btnSearch" class="sr-only">按商品名称查询</label>
                            <asp:TextBox ID="txtProdName" runat="server" placeholder="请输入商品名称..." CssClass="form-control"></asp:TextBox>
                        </div>
                        <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-info" OnClick="btnSearch_Click" Text="查询" OnClientClick="return verifyCriteria();" />
                        <asp:Button ID="btnShowAll" runat="server" Text="全部商品" CssClass="btn btn-warning" OnClick="btnShowAll_Click" />
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12">
                <asp:GridView ID="gvFruitList" runat="server" AutoGenerateColumns="False" DataSourceID="odsFruitList" DataKeyNames="ID" AllowPaging="True" OnSelectedIndexChanged="gvFruitList_SelectedIndexChanged" OnRowDataBound="gvFruitList_RowDataBound" OnRowDeleted="gvFruitList_RowDeleted" OnRowDeleting="gvFruitList_RowDeleting" CssClass="table table-striped table-hover table-responsive" AllowCustomPaging="True" PagerStyle-CssClass="pager">
                    <Columns>
                        <asp:CommandField ShowDeleteButton="True" />
                        <asp:TemplateField HeaderText="商品名称" SortExpression="FruitName">
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# "【"+ Eval("Category.CategoryName")+"】"+Eval("FruitName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="FruitDesc" HeaderText="商品描述" SortExpression="FruitDesc" ItemStyle-CssClass="col-lg-4" />
                        <asp:TemplateField HeaderText="商品价格" SortExpression="FruitPrice">
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Eval("FruitPrice", "{0:c2}")+"元/"+Eval("FruitUnit") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="库存数量" SortExpression="InventoryQty">
                            <ItemTemplate>
                                <asp:Label ID="lblInventoryQty" runat="server" Text='<%# Eval("InventoryQty") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否上架" SortExpression="OnSale">
                            <ItemTemplate>
                                <div id="divOnSale" runat="server"></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否置顶" SortExpression="IsSticky">
                            <ItemTemplate>
                                <div id="divIsSticky" runat="server"></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Priority" HeaderText="优先级" SortExpression="Priority" />
                        <asp:CommandField ButtonType="Button" ShowSelectButton="True">
                            <ControlStyle CssClass="btn btn-default" />
                        </asp:CommandField>
                    </Columns>
                    <PagerStyle CssClass="pager"></PagerStyle>
                    <SelectedRowStyle CssClass="danger" />
                </asp:GridView>
                <asp:ObjectDataSource ID="odsFruitList" runat="server" SelectMethod="FindFruitPager" TypeName="Fruit" DeleteMethod="DelFruit" OnDeleted="odsFruitList_Deleted" OnDeleting="odsFruitList_Deleting" EnablePaging="True" OnSelecting="odsFruitList_Selecting" SelectCountMethod="FindProductCount">
                    <DeleteParameters>
                        <asp:Parameter Direction="Output" Name="delFruitCount" Type="Int32" />
                        <asp:Parameter Direction="Output" Name="delImgCount" Type="Int32" />
                    </DeleteParameters>
                </asp:ObjectDataSource>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12">
                <asp:DetailsView CssClass="table table-responsive table-condensed" ID="dvFruit" runat="server" AutoGenerateRows="False" DataSourceID="odsFruit" DataKeyNames="ID" GridLines="Horizontal" OnItemInserted="dvFruit_ItemInserted" OnItemInserting="dvFruit_ItemInserting" OnItemUpdating="dvFruit_ItemUpdating" OnItemUpdated="dvFruit_ItemUpdated" OnModeChanged="dvFruit_ModeChanged" OnDataBound="dvFruit_DataBound">
                    <FieldHeaderStyle CssClass="col-xs-2 col-sm-2 col-md-2 col-lg-2"></FieldHeaderStyle>
                    <Fields>
                        <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="ID"></asp:BoundField>
                        <asp:TemplateField HeaderText="商品名称" SortExpression="FruitName">
                            <EditItemTemplate>
                                <asp:TextBox ID="txtFruitName" runat="server" Text='<%# Bind("FruitName") %>' CausesValidation="True" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="请填写商品名称" ControlToValidate="txtFruitName" SetFocusOnError="True" Display="Dynamic"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:TextBox ID="txtFruitName" runat="server" Text='<%# Bind("FruitName") %>' CausesValidation="True" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="请填写商品名称" ControlToValidate="txtFruitName" SetFocusOnError="True" Display="Dynamic"></asp:RequiredFieldValidator>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label5" runat="server" Text='<%# Bind("FruitName") %>'></asp:Label>
                            </ItemTemplate>
                            <ControlStyle CssClass="form-control" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="商品类别" SortExpression="Category">
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlCategoryEdit" runat="server" DataSource="<%# MakeCategoryDataSource() %>" DataTextField="Value" DataValueField="Key" SelectedValue='<%# Eval("Category.ID") %>' CssClass="form-control">
                                </asp:DropDownList><asp:CustomValidator ID="CustomValidator1" runat="server" ErrorMessage="请选择商品类别" ControlToValidate="ddlCategoryEdit" ClientValidationFunction="validateCategory" Display="Dynamic"></asp:CustomValidator>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:DropDownList ID="ddlCategoryInsert" runat="server" DataSource="<%# MakeCategoryDataSource() %>" DataTextField="Value" DataValueField="Key" CssClass="form-control">
                                </asp:DropDownList><asp:CustomValidator ID="CustomValidator2" runat="server" ErrorMessage="请选择商品类别" ControlToValidate="ddlCategoryInsert" ClientValidationFunction="validateCategory" Display="Dynamic"></asp:CustomValidator>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label2" runat="server" Text='<%# Eval("Category.CategoryName") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="商品价格" SortExpression="FruitPrice">
                            <EditItemTemplate>
                                <asp:TextBox ID="txtFruitPrice" runat="server" Text='<%# Bind("FruitPrice") %>' CssClass="form-control"></asp:TextBox>元<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="请输入有效的数字" ControlToValidate="txtFruitPrice" SetFocusOnError="True" ValidationExpression="[1-9]\d*.\d*|0.\d*[1-9]\d*|[1-9]\d*" Display="Dynamic"></asp:RegularExpressionValidator>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="请输入商品价格" SetFocusOnError="True" ControlToValidate="txtFruitPrice" Display="Dynamic"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:TextBox ID="txtFruitPrice" runat="server" Text='<%# Bind("FruitPrice", "{0:c2}") %>' CssClass="form-control"></asp:TextBox>元<asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server" ErrorMessage="请输入有效的数字" ControlToValidate="txtFruitPrice" SetFocusOnError="True" ValidationExpression="[1-9]\d*.\d*|0.\d*[1-9]\d*|[1-9]\d*" Display="Dynamic"></asp:RegularExpressionValidator>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="请输入商品价格" SetFocusOnError="True" ControlToValidate="txtFruitPrice" Display="Dynamic"></asp:RequiredFieldValidator>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label1" runat="server" Text='<%# Bind("FruitPrice", "{0:c2}") %>'></asp:Label>
                            </ItemTemplate>
                            <ControlStyle CssClass="form-control" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="商品单位" SortExpression="FruitUnit">
                            <EditItemTemplate>
                                <asp:TextBox ID="txtFruitUnit" runat="server" Text='<%# Bind("FruitUnit") %>' CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="请输入商品单位" SetFocusOnError="True" ControlToValidate="txtFruitUnit" Display="Dynamic"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:TextBox ID="txtFruitUnit" runat="server" Text='<%# Bind("FruitUnit") %>' CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="请输入商品单位" SetFocusOnError="True" ControlToValidate="txtFruitUnit" Display="Dynamic"></asp:RequiredFieldValidator>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label6" runat="server" Text='<%# Bind("FruitUnit") %>'></asp:Label>
                            </ItemTemplate>
                            <ControlStyle CssClass="form-control" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="商品描述" SortExpression="FruitDesc">
                            <EditItemTemplate>
                                <asp:TextBox ID="txtFruitDesc" runat="server" Text='<%# Bind("FruitDesc") %>' Rows="3" TextMode="MultiLine" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="请输入商品描述" ControlToValidate="txtFruitDesc" SetFocusOnError="True" Display="Dynamic"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:TextBox ID="txtFruitDesc" runat="server" Text='<%# Bind("FruitDesc") %>' Rows="3" TextMode="MultiLine" CssClass="form-control"></asp:TextBox><asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="请输入商品描述" ControlToValidate="txtFruitDesc" SetFocusOnError="True" Display="Dynamic"></asp:RequiredFieldValidator>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label3" runat="server" Text='<%# Bind("FruitDesc") %>'></asp:Label>
                            </ItemTemplate>
                            <ControlStyle CssClass="form-control" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="库存数量" SortExpression="InventoryQty">
                            <EditItemTemplate>
                                <asp:TextBox ID="txtInventoryQty" runat="server" Text='<%# Bind("InventoryQty") %>' ClientIDMode="Static" onchange="validateInventory();" CssClass="form-control"></asp:TextBox>
                                <div class="checkbox">
                                    <label>
                                        <asp:CheckBox ID="cbInventoryQty" runat="server" onclick="switchInventoryInfinite();" ClientIDMode="Static" Text="不限量" />
                                    </label>
                                </div>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:TextBox ID="txtInventoryQty" runat="server" Style="display: none;" Text='<%# Bind("InventoryQty") %>' ClientIDMode="Static" onchange="validateInventory();" CssClass="form-control"></asp:TextBox>
                                <div class="checkbox">
                                    <label>
                                        <asp:CheckBox ID="cbInventoryQty" runat="server" Checked="true" onclick="switchInventoryInfinite();" ClientIDMode="Static" Text="不限量" />
                                    </label>
                                </div>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="lblInventoryQty" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否上架" SortExpression="OnSale" ControlStyle-CssClass="checkbox">
                            <EditItemTemplate>
                                <asp:CheckBox ID="cbOnSale" runat="server" Checked='<%# Bind("OnSale") %>' />
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:CheckBox ID="cbOnSale" runat="server" Checked='<%# Bind("OnSale") %>' />
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:CheckBox ID="cbOnSale" runat="server" Checked='<%# Bind("OnSale") %>' Enabled="false" />
                            </ItemTemplate>
                            <ControlStyle CssClass="checkbox" />
                        </asp:TemplateField>
                        <asp:CheckBoxField DataField="IsSticky" HeaderText="是否置顶" SortExpression="IsSticky" ControlStyle-CssClass="checkbox"></asp:CheckBoxField>
                        <asp:TemplateField HeaderText="优先级" SortExpression="Priority">
                            <EditItemTemplate>
                                <asp:TextBox ID="txtPriority" runat="server" Text='<%# Bind("Priority") %>' CssClass="form-control"></asp:TextBox><asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ErrorMessage="请输入有效的数字" ControlToValidate="txtPriority" SetFocusOnError="True" ValidationExpression="\d*" Display="Dynamic"></asp:RegularExpressionValidator>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="请输入优先级" ControlToValidate="txtPriority" Display="Dynamic"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <asp:TextBox ID="txtPriority" runat="server" Text='<%# Bind("Priority") %>' CssClass="form-control"></asp:TextBox><asp:RegularExpressionValidator ID="RegularExpressionValidator2" runat="server" ErrorMessage="请输入有效的数字" ControlToValidate="txtPriority" SetFocusOnError="True" ValidationExpression="\d*" Display="Dynamic"></asp:RegularExpressionValidator>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="请输入优先级" ControlToValidate="txtPriority" Display="Dynamic"></asp:RequiredFieldValidator>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <asp:Label ID="Label4" runat="server" Text='<%# Bind("Priority") %>'></asp:Label>
                            </ItemTemplate>
                            <ControlStyle CssClass="form-control" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="商品图片">
                            <EditItemTemplate>
                                <div class="panel panel-default">
                                    <div class="panel-heading">
                                        <span class="panel-title">上传图片（<%# Config.AllowedUploadFileExt %>）</span>
                                        <div class="form-group">
                                            <label for="btnAddImgEdit" class="sr-only">增加图片</label>
                                            <button id="btnAddImgEdit" type="button" class="btn btn-default" onclick="addImg('Edit')"><i class="fa fa-plus fa-lg fa-fw"></i>增加图片</button>
                                        </div>
                                        <div class="form-group">
                                            <label for="btnDelImgEdit" class="sr-only">减少图片</label>
                                            <button id="btnDelImgEdit" type="button" class="btn btn-default" onclick="removeImg('Edit')" disabled="disabled"><i class="fa fa-minus fa-lg fa-fw"></i>减少图片</button>
                                        </div>
                                        <a href="http://zhitu.isux.us/" target="_blank">腾讯图片处理工具</a>
                                        <a href="http://www.uupoop.com/" target="_blank">在线PS</a>
                                    </div>
                                    <div class="panel-body">
                                        <ul id="ulFileUploadEdit">
                                            <li>
                                                <div class="form-group">
                                                    <label for="fuImg" class="sr-only">选择图片</label>
                                                    <asp:FileUpload ID="fuImg" runat="server" CssClass="form-control" />
                                                </div>
                                                <div class="form-group">
                                                    <label for="txtImgDescEdit" class="sr-only">图片说明</label>
                                                    <asp:TextBox ID="txtImgDescEdit" CssClass="form-control" runat="server" placeholder="图片说明..."></asp:TextBox>
                                                </div>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                                <div class="grid-stack">
                                    <asp:Repeater ID="rpFruitImgList" runat="server" DataSource='<%# Eval("FruitImgList") %>' OnItemDataBound="rpFruitImgList_ItemDataBound">
                                        <ItemTemplate>
                                            <div runat="server" id="divGridStackItem" class="grid-stack-item" data-gs-no-resize="true">
                                                <div class="grid-stack-item-content thumbnail">
                                                    <asp:HiddenField ID="hfImgID" runat="server" Value='<%# Eval("ImgID") %>' />
                                                    <asp:HiddenField ID="hfImgSeqX" runat="server" />
                                                    <asp:HiddenField ID="hfImgSeqY" runat="server" />
                                                    <asp:HyperLink ID="hlOriginalImg" CssClass="" runat="server" ImageUrl='<%# "~/images/"+Eval("ImgName") %>' NavigateUrl='<%# "~/images/"+Eval("ImgName") %>' Target="_blank" Text='<%# Eval("ImgName") %>'></asp:HyperLink>
                                                    <div class="caption">
                                                        <div class="radio">
                                                            <label>
                                                                <asp:RadioButton ID="rbMainImg" runat="server" Checked='<%# Eval("MainImg") %>' Text="主图" onclick="selectMainImg()" />
                                                            </label>
                                                        </div>
                                                        <div class="checkbox">
                                                            <label>
                                                                <asp:CheckBox ID="cbDetailImg" runat="server" Checked='<%# Eval("DetailImg") %>' Text="详图" />
                                                            </label>
                                                        </div>
                                                        <div class="form-group">
                                                            <asp:Button ID="btnDelImg" runat="server" Text="删除图片" CommandArgument='<%# Eval("ImgID") %>' OnClick="btnDelImg_Click" OnClientClick="return confirm('您是否要删除这个图片？');" CssClass="btn btn-default btn-sm" />
                                                            <label for="txtImgDescEditOriginal" class="sr-only">图片说明</label>
                                                            <asp:TextBox ID="txtImgDescEditOriginal" CssClass="form-control input-sm" runat="server" Text='<%# Eval("ImgDesc") %>' placeholder="图片说明" Width="100"></asp:TextBox>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </EditItemTemplate>
                            <InsertItemTemplate>
                                <div class="panel panel-default">
                                    <div class="panel-heading">
                                        <span class="panel-title">上传图片（<%# Config.AllowedUploadFileExt %>）</span>
                                        <div class="form-group">
                                            <label for="btnAddImgInsert" class="sr-only">增加图片</label>
                                            <button id="btnAddImgInsert" type="button" class="btn btn-default btn-sm" onclick="addImg('Insert')"><i class="fa fa-plus fa-lg fa-fw"></i>增加图片</button>
                                        </div>
                                        <div class="form-group">
                                            <label for="btnDelImgInsert" class="sr-only">减少图片</label>
                                            <button id="btnDelImgInsert" type="button" class="btn btn-default btn-sm" onclick="removeImg('Insert')" disabled="disabled"><i class="fa fa-minus fa-lg fa-fw"></i>减少图片</button>
                                        </div>
                                        <a href="http://zhitu.isux.us/" target="_blank">腾讯图片处理工具</a>
                                        <a href="http://www.uupoop.com/" target="_blank">在线PS</a>
                                    </div>
                                    <div class="panel-body">
                                        <ul id="ulFileUploadInsert">
                                            <li>
                                                <div class="form-group">
                                                    <label for="fuImg" class="sr-only">选择图片</label>
                                                    <asp:FileUpload ID="fuImg" runat="server" CssClass="form-control" />
                                                </div>
                                                <div class="form-group">
                                                    <label for="txtImgDescInsert" class="sr-only">图片说明</label>
                                                    <asp:TextBox ID="txtImgDescInsert" CssClass="form-control" runat="server" placeholder="图片说明..."></asp:TextBox>
                                                </div>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                            </InsertItemTemplate>
                            <ItemTemplate>
                                <div class="grid-stack">
                                    <asp:Repeater ID="rpFruitImgList" runat="server" DataSource='<%# Eval("FruitImgList") %>' OnItemDataBound="rpFruitImgList_ItemDataBound">
                                        <ItemTemplate>
                                            <div runat="server" id="divGridStackItem" class="grid-stack-item" data-gs-locked="true" data-gs-no-resize="true" data-gs-no-move="true">
                                                <div class="grid-stack-item-content thumbnail">
                                                    <asp:HyperLink ID="hlOriginalImg" runat="server" ImageUrl='<%# "~/images/"+Eval("ImgName") %>' NavigateUrl='<%# "~/images/"+Eval("ImgName") %>' Target="_blank" Text='<%# Eval("ImgName") %>'></asp:HyperLink>
                                                    <div class="caption">
                                                        <div class="radio">
                                                            <label>
                                                                <asp:RadioButton ID="rbMainImg" runat="server" Checked='<%# Eval("MainImg") %>' Text="主图" Enabled="False" />
                                                            </label>
                                                        </div>
                                                        <div class="checkbox">
                                                            <label>
                                                                <asp:CheckBox ID="cbDetailImg" runat="server" Checked='<%# Eval("DetailImg") %>' Text="详图" Enabled="False" />
                                                            </label>
                                                        </div>
                                                        <label for="txtImgDescEditOriginal" class="sr-only">图片说明</label>
                                                        <asp:TextBox ID="txtImgDescEditOriginal" CssClass="form-control input-sm" runat="server" Text='<%# Eval("ImgDesc") %>' Width="100" Enabled="False"></asp:TextBox>
                                                    </div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Fields>
                </asp:DetailsView>
                <asp:Label ID="lblErrMsg" runat="server" ForeColor="Red" EnableViewState="False"></asp:Label>
                <asp:ObjectDataSource ID="odsFruit" runat="server" SelectMethod="FindFruitByID" TypeName="Fruit" DataObjectTypeName="Fruit" InsertMethod="AddFruit" UpdateMethod="UpdateFruit" OnSelecting="odsFruit_Selecting" OnInserted="odsFruit_Inserted" OnInserting="odsFruit_Inserting" OnUpdated="odsFruit_Updated" OnSelected="odsFruit_Selected">
                    <SelectParameters>
                        <asp:Parameter Name="fruitID" Type="Int32" />
                    </SelectParameters>
                </asp:ObjectDataSource>
            </div>
        </div>
    </div>

    <script>

        requirejs(['jquery', 'gridstack'], function ($) {

            $(function () {
                var options = {
                };
                $('.grid-stack').gridstack(options);

                theForm.onsubmit = jumpToGrid;

            });
        });

        //点击新增商品或选择按钮时，页面跳到DetailView
        function jumpToGrid() {

            if (event && event.currentTarget && (event.currentTarget.value == '新增商品' || event.currentTarget.value == '选择')) {
                var dvFruitID = $("table[id*=dvFruit]").attr("id");
                theForm.action += "#" + dvFruitID;
            }

            setImgSeq();
        }

        //设置客户端调整后的商品图片gridstack.js的X/Y值
        function setImgSeq() {

            $(".grid-stack-item").each(function () {
                $(this).find("input[id*='hfImgSeqX']").val($(this).data("gs-x"));
                $(this).find("input[id*='hfImgSeqY']").val($(this).data("gs-y"));
            });

        }

        var fuNum = 1;

        //增减图片上传框
        function addImg(whichTemplate) {
            fuNum++;
            $("#ulFileUpload" + whichTemplate).append("<li id='li" + fuNum + whichTemplate + "'><div class='form-group'><input type='file' class='form-control' name='dvFruit$fuImg" + fuNum + "' id='fuImg" + fuNum + "'/></div> <div class='form-group'><input type='text' class='form-control' name='dvFruit$txtImgDesc" + whichTemplate + "' id='txtImgDesc" + fuNum + whichTemplate + "' placeholder='图片说明...'/></div></li>");
            if (fuNum > 1) {
                $("#btnDelImg" + whichTemplate).removeAttr("disabled");
            }
        }

        function removeImg(whichTemplate) {
            $("#li" + fuNum + whichTemplate).remove();
            fuNum--;
            if (fuNum == 1) {
                $("#btnDelImg" + whichTemplate).attr("disabled", "disabled");
            }
        }

        //遍历dlFruit中的主图单选按钮，确保唯一选择
        function selectMainImg() {
            $rb = $(event.currentTarget);
            $(".grid-stack :radio").removeAttr("checked");
            $rb.prop("checked", "checked");
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

        //切换商品是否不限库存量
        function switchInventoryInfinite(event) {
            if ($("#cbInventoryQty").is(":checked")) {
                $("#txtInventoryQty").val(-1).hide();
            }
            else {
                $("#txtInventoryQty").val("").show();
            }
        }

        //校验商品库存量是否数值
        function validateInventory() {
            var inventoryQty = $("#txtInventoryQty").val();
            if (isNaN(inventoryQty) || inventoryQty < 0) {
                alert("请输入>=0数值");
                $("#txtInventoryQty").val("").focus();
            }
        }

        //校验是否选择了商品类别
        function validateCategory(sender, args) {
            if (args.Value == "0") {
                args.IsValid = false;
            }
            else {
                args.IsValid = true;
            }
        }
    </script>

</asp:Content>
