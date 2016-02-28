<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ManageCategory.aspx.cs" Inherits="ManageCategory" %>

<!DOCTYPE html>
<html>
<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>商品类别管理</title>
    <link href="../Scripts/JTree/saas.css" rel="stylesheet" type="text/css" />
    <link href="../Scripts/JTree/mask.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <!-- #include file="header.html" -->
    <form id="frmCategory" runat="server" class="form-horizontal">
        <div id="divCategory" class="container">
            <div id="divCategoryTree">
                <ul id="ulCategoryTree" class="treeview filetree">
                </ul>
            </div>
            <div id="divCategoryManage" class="panel panel-info">
                <div class="panel-heading">
                    <h3 class="panel-title">商品类别</h3>
                </div>
                <div class="panel-body">
                    <div id="divCategoryID" class="form-group">
                        <label for="lblCategoryID" class="col-xs-2 col-sm-2 col-md-2 col-lg-2 control-label">类别ID：</label>
                        <div class="col-xs-10 col-sm-10 col-md-10 col-lg-10" style="padding-top: 7px;">
                            <asp:Label ID="lblCategoryID" runat="server" ClientIDMode="Static" CssClass="control-label"></asp:Label>
                            <asp:HiddenField ID="hfCategoryID" runat="server" ClientIDMode="Static" />
                        </div>
                    </div>
                    <div id="divParentID" class="form-group">
                        <label for="lblParentID" class="col-xs-2 col-sm-2 col-md-2 col-lg-2 control-label">上级类别：</label>
                        <div class="col-xs-10 col-sm-10 col-md-10 col-lg-10" style="padding-top: 7px;">
                            <asp:Label ID="lblParentID" CssClass="control-label" runat="server" Text="" ClientIDMode="Static"></asp:Label>
                            <asp:HiddenField ID="hfParentID" runat="server" ClientIDMode="Static" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label for="txtCategoryName" class="col-xs-2 col-sm-2 col-md-2 col-lg-2 control-label">类别名称：</label>
                        <div class="col-xs-10 col-sm-10 col-md-10 col-lg-10">
                            <asp:TextBox ID="txtCategoryName" CssClass="form-control" runat="server" ClientIDMode="Static" ToolTip="类别名称不超过50个字符" required="required"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-xs-offset-2 col-sm-offset-2 col-md-offset-2 col-lg-offset-2 col-xs-10 col-sm-10 col-md-10 col-lg-10">
                            <asp:Button ID="btnSubmit" CssClass="btn btn-info" runat="server" Text="" ClientIDMode="Static" OnClick="btnSubmit_Click" />
                            <input id="btnCancel" type="button" class="btn btn-warning" value="取消" onclick="HidePanel()" /><asp:HiddenField ID="hfFlag" runat="server" />
                        </div>
                    </div>
                </div>
            </div>
            <p>
                <asp:Label ID="lblMsg" runat="server" EnableViewState="False"></asp:Label>
            </p>
        </div>
    </form>
</body>

<script src="../Scripts/JTree/jquery-1.2.3.js"></script>
<script src="../Scripts/JTree/jquery.mask.js"></script>
<script src="../Scripts/JTree/jquery.contextmenu.js"></script>
<script src="../Scripts/JTree/saas.js"></script>
<script src="../Scripts/JTree/jqDnR.js"></script>

<script>

    $(function () {

        $("#divCategoryManage").hide();

        $('#ulCategoryTree').showTree({
            data: data, rootID: 0, bindings: {
                'pg_update': {
                    val: '修改',
                    ismy: 1,
                    cb: function (t) {
                        if ($(t).attr("val") != 0) {
                            $("#divCategoryID").show();
                            $("#lblCategoryID").text($(t).attr("val"));
                            $("#hfCategoryID").val($(t).attr("val"));

                            $("#divParentID").hide();
                            $("#hfParentID").val($(t).attr("parentid"));

                            $("#txtCategoryName").val($(t).text());
                            $("#btnSubmit").val("修改");
                            $("#hfFlag").val("update");

                            $("#divCategoryManage").show();
                        }
                    }
                },
                'pg_delete': {
                    val: '删除',
                    ismy: 1,
                    cb: function (t) {
                        if ($(t).attr("val") != 0) {

                            $("#hfCategoryID").val($(t).attr("val"));

                            $("#txtCategoryName").val($(t).text());

                            $("#btnSubmit").val("删除");
                            $("#hfFlag").val("del");
                            if (window.confirm("确认删除【" + $(t).text() + "】吗？")) {
                                $("#btnSubmit").click();
                            }
                        }
                    }
                },
                'pg_add': {
                    val: '新增',
                    ismy: 1,
                    cb: function (t) {
                        $("#divCategoryID").hide();

                        $("#divParentID").show();
                        $("#lblParentID").text($(t).text());
                        $("#hfParentID").val($(t).attr("val"));

                        $("#txtCategoryName").val("");

                        $("#btnSubmit").val("新增");
                        $("#hfFlag").val("add");

                        $("#divCategoryManage").show();
                    }
                }

            }, callback: function (t) { //叶子节点单击回调函数
                //alert(t);

            }
        });
    });

    function HidePanel() {
        $("#divCategoryManage").hide();
    }

</script>

</html>
