<%@ Page Language="C#" AutoEventWireup="true" EnableViewStateMac="false" EnableEventValidation="false" CodeFile="ManageProduct.aspx.cs" Inherits="ManageProduct" Debug="True" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>商品管理</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link href="../css/ManageProduct.css" rel="stylesheet" />
    <link href="../Scripts/gridstack/gridstack-0.2.4.css" rel="stylesheet" />
    <%--    <link rel="stylesheet" href="//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.2.4/gridstack.min.css" />--%>
</head>
<body>
    <!-- #include file="header.html" -->
    <form id="form1" runat="server" enctype="multipart/form-data" class="form-inline">
        <div class="container-fluid">
            <div class="panel panel-info">
                <div class="panel-heading text-center">
                    <h2 class="panel-title">商品查询</h2>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-lg-2">
                            <asp:Button ID="btnAddFruit" runat="server" Text="新增商品" OnClick="btnAddFruit_Click" CssClass="btn btn-danger" OnClientClick="theForm.__EVENTARGUMENT.value='Add';" />
                        </div>
                        <div class="col-lg-10 text-right" id="divCriterias">
                            <div class="form-group">
                                <label for="ddlCategory" class="sr-only">按商品类别查询</label>
                                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-control">
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
                            <asp:BoundField DataField="FruitName" HeaderText="商品名称" SortExpression="FruitName" />
                            <asp:BoundField DataField="Category.CategoryName" HeaderText="商品类别" SortExpression="Category" />
                            <asp:BoundField DataField="FruitPrice" HeaderText="商品价格" SortExpression="FruitPrice" DataFormatString="{0:c2}" />
                            <asp:BoundField DataField="FruitUnit" HeaderText="商品单位" SortExpression="FruitUnit" />
                            <asp:BoundField DataField="FruitDesc" HeaderText="商品描述" SortExpression="FruitDesc" />
                            <asp:BoundField DataField="InventoryQty" HeaderText="库存数量" SortExpression="InventoryQty" />
                            <asp:CheckBoxField DataField="OnSale" HeaderText="是否上架" SortExpression="OnSale" />
                            <asp:CheckBoxField DataField="IsSticky" HeaderText="是否置顶" SortExpression="IsSticky" />
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
                    <asp:DetailsView CssClass="table table-responsive table-condensed" ID="dvFruit" runat="server" AutoGenerateRows="False" DataSourceID="odsFruit" DataKeyNames="ID" GridLines="Horizontal" OnItemInserted="dvFruit_ItemInserted" OnItemInserting="dvFruit_ItemInserting" OnItemUpdating="dvFruit_ItemUpdating" OnItemUpdated="dvFruit_ItemUpdated">
                        <FieldHeaderStyle CssClass="col-xs-2 col-sm-2 col-md-2 col-lg-2"></FieldHeaderStyle>
                        <Fields>
                            <asp:BoundField DataField="ID" HeaderText="ID" InsertVisible="False" ReadOnly="True" SortExpression="ID" ControlStyle-CssClass="form-control">
                                <ControlStyle CssClass="form-control"></ControlStyle>
                            </asp:BoundField>
                            <asp:BoundField DataField="FruitName" HeaderText="商品名称" SortExpression="FruitName" ControlStyle-CssClass="form-control">
                                <ControlStyle CssClass="form-control"></ControlStyle>
                            </asp:BoundField>
                            <asp:TemplateField HeaderText="商品类别" SortExpression="Category">
                                <EditItemTemplate>
                                    <asp:DropDownList ID="ddlCategoryEdit" runat="server" DataSource="<%# MakeCategoryDataSource() %>" DataTextField="Value" DataValueField="Key" SelectedValue='<%# Eval("Category.ID") %>' CssClass="form-control">
                                    </asp:DropDownList>
                                </EditItemTemplate>
                                <InsertItemTemplate>
                                    <asp:DropDownList ID="ddlCategoryInsert" runat="server" DataSource="<%# MakeCategoryDataSource() %>" DataTextField="Value" DataValueField="Key" CssClass="form-control">
                                    </asp:DropDownList>
                                </InsertItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="Label2" runat="server" Text='<%# Eval("Category.CategoryName") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="FruitPrice" HeaderText="商品价格" SortExpression="FruitPrice" DataFormatString="{0:c2}" ControlStyle-CssClass="form-control">
                                <ControlStyle CssClass="form-control"></ControlStyle>
                            </asp:BoundField>
                            <asp:BoundField DataField="FruitUnit" HeaderText="商品单位" SortExpression="FruitUnit" ControlStyle-CssClass="form-control">
                                <ControlStyle CssClass="form-control"></ControlStyle>
                            </asp:BoundField>
                            <asp:BoundField DataField="FruitDesc" HeaderText="商品描述" SortExpression="FruitDesc" ControlStyle-CssClass="form-control">
                                <ControlStyle CssClass="form-control"></ControlStyle>
                            </asp:BoundField>
                            <asp:BoundField DataField="InventoryQty" HeaderText="库存数量" SortExpression="InventoryQty" ControlStyle-CssClass="form-control">
                                <ControlStyle CssClass="form-control"></ControlStyle>
                            </asp:BoundField>
                            <asp:CheckBoxField DataField="OnSale" HeaderText="是否上架" SortExpression="OnSale" />
                            <asp:CheckBoxField DataField="IsSticky" HeaderText="是否置顶" SortExpression="IsSticky" />
                            <asp:BoundField DataField="Priority" HeaderText="优先级" SortExpression="Priority" ControlStyle-CssClass="form-control">
                                <ControlStyle CssClass="form-control"></ControlStyle>
                            </asp:BoundField>
                            <asp:TemplateField HeaderText="商品图片">
                                <EditItemTemplate>
                                    <div class="panel panel-default">
                                        <div class="panel-heading">
                                            <span class="panel-title">上传图片（<%# Config.AllowedUploadFileExt %>）</span>
                                            <div class="form-group">
                                                <label for="btnAddImgEdit" class="sr-only">增加图片</label>
                                                <button id="btnAddImgEdit" type="button" class="btn btn-default" onclick="addImg('Edit')">增加图片</button>
                                            </div>
                                            <div class="form-group">
                                                <label for="btnDelImgEdit" class="sr-only">减少图片</label>
                                                <button id="btnDelImgEdit" type="button" class="btn btn-default" onclick="removeImg('Edit')" disabled="disabled">减少图片</button>
                                            </div>
                                            <a href="http://zhitu.isux.us/" target="_blank">腾讯图片处理工具</a>
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
                                                                    <asp:RadioButton ID="rbMainImg" runat="server" Checked='<%# Eval("MainImg") %>' Text="主图" onclick="selectMainImg(this.id)" />
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
                                                <button id="btnAddImgInsert" type="button" class="btn btn-default btn-sm" onclick="addImg('Insert')">增加图片</button>
                                            </div>
                                            <div class="form-group">
                                                <label for="btnDelImgInsert" class="sr-only">减少图片</label>
                                                <button id="btnDelImgInsert" type="button" class="btn btn-default btn-sm" onclick="removeImg('Insert')" disabled="disabled">减少图片</button>
                                            </div>
                                            <a href="http://zhitu.isux.us/" target="_blank">腾讯图片处理工具</a>
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
    </form>
</body>

<script src="../Scripts/lodash.min.js"></script>
<script src='//cdnjs.cloudflare.com/ajax/libs/gridstack.js/0.2.4/gridstack.min.js'></script>

<script>

    $(function () {
        var options = {
        };
        $('.grid-stack').gridstack(options);

    });

    theForm.onsubmit = setImgSeq;
    $(theForm).on("submit", setImgSeq);

    function setImgSeq() {
        $(".grid-stack-item").each(function () {
            $(this).find("input[id*='hfImgSeqX']").attr("value", $(this).data("gs-x"));
            $(this).find("input[id*='hfImgSeqY']").attr("value", $(this).data("gs-y"));
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
    function selectMainImg(currentImgID) {
        $(":radio").each(function () {
            if ($(this).prop("id") != currentImgID) {
                $(this).removeAttr("checked");
            }
        });
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

    $(theForm).on("submit", function () {
        if (theForm.__EVENTARGUMENT.value.indexOf("Add") != -1 || theForm.__EVENTARGUMENT.value.indexOf("Select") != -1) {
            theForm.action += "#dvFruit";
        }
    });


    //theForm.onsubmit = function () {
    //    if (theForm.__EVENTARGUMENT.value.indexOf("Add") != -1 || theForm.__EVENTARGUMENT.value.indexOf("Select") != -1) {
    //        theForm.action += "#dvFruit";
    //    }
    //    return true;
    //};

</script>
</html>
