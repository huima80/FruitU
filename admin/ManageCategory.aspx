<%@ Page Title="商品类别管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="ManageCategory.aspx.cs" Inherits="ManageCategory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="../Scripts/JTree/saas.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server" ClientIDMode="Static">
    <div id="divCategory" class="container">
        <div id="divCategoryTree">
            <ul id="ulCategoryTree" class="treeview filetree">
            </ul>
        </div>
        <div id="divCategoryManage" class="panel panel-info" style="display: none;">
            <div class="panel-heading">
                <h3 class="panel-title">商品类别</h3>
            </div>
            <div class="panel-body form-inline">
                <div id="divCategoryID" class="form-group">
                    <label for="lblCategoryID" class="control-label">类别ID：</label>
                    <p id="lblCategoryID" class="form-control-static"></p>
                    <asp:HiddenField ID="hfCategoryID" runat="server" ClientIDMode="Static" />
                </div>
                <div id="divParentID" class="form-group">
                    <label for="lblParentID" class="control-label">上级类别：</label>
                    <p id="lblParentID" class="form-control-static"></p>
                    <asp:HiddenField ID="hfParentID" runat="server" ClientIDMode="Static" />
                </div>
                <div class="form-group">
                    <label for="txtCategoryName" class="control-label">类别名称：</label>
                    <asp:TextBox ID="txtCategoryName" CssClass="form-control" runat="server" ClientIDMode="Static" ToolTip="类别名称不超过50个字符" required="required"></asp:TextBox>
                </div>
                <div class="form-group">
                    <asp:Button ID="btnSubmit" CssClass="btn btn-info" runat="server" Text="" ClientIDMode="Static" OnClick="btnSubmit_Click" />
                    <input id="btnCancel" type="button" class="btn btn-warning" value="取消" onclick="HidePanel()" /><asp:HiddenField ID="hfFlag" runat="server" />
                </div>
            </div>
        </div>
        <p>
            <asp:Label ID="lblMsg" runat="server" CssClass="text-danger" EnableViewState="False"></asp:Label>
        </p>
    </div>

    <script>

        requirejs(['jquery'], function ($) {

            $(function () {

                $("#divCategoryManage").hide();

                requirejs(['JTreeSaaS', 'JTreeContextmenu'], function () {

                    $('#ulCategoryTree').showTree({
                        data: data, rootID: 0, bindings: {
                            'pg_update': {
                                val: '修改',
                                ismy: 1,
                                cb: function (t) {
                                    if ($(t).attr("val") != 0) {
                                        $("#divCategoryManage").show();
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
                                    $("#divCategoryManage").show();
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
            });
        });

        function HidePanel() {
            $("#divCategoryManage").hide();
        }

    </script>

</asp:Content>
