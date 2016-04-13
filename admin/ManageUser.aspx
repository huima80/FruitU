<%@ Page Title="用户管理" Language="C#" MasterPageFile="~/admin/MasterPage.master" AutoEventWireup="true" CodeFile="ManageUser.aspx.cs" Inherits="admin_ManageUser" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style>
        .head-img img {
            width: 64px;
            height: 64px;
        }

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
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server" ClientIDMode="Static">
    <div class="container-fluid">
        <div class="row">
            <div class="col-lg-12">
                <div class="panel panel-info">
                    <div class="panel-heading text-center">
                        <h2 class="panel-title">用户查询</h2>
                    </div>
                    <div class="panel-body" id="divCriterias">
                        <div class="row">
                            <div class="col-lg-10">
                                <div class="form-group">
                                    <label for="ddlIsSubscribe" class="sr-only">是否关注公众号</label>
                                    <asp:DropDownList ID="ddlIsSubscribe" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===是否关注公众号===</asp:ListItem>
                                        <asp:ListItem Value="1">是</asp:ListItem>
                                        <asp:ListItem Value="2">否</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlSex" class="sr-only">性别</label>
                                    <asp:DropDownList ID="ddlSex" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===性别===</asp:ListItem>
                                        <asp:ListItem Value="1">男</asp:ListItem>
                                        <asp:ListItem Value="0">女</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlIsOnline" class="sr-only">是否在线</label>
                                    <asp:DropDownList ID="ddlIsOnline" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===是否在线===</asp:ListItem>
                                        <asp:ListItem Value="1">在线</asp:ListItem>
                                        <asp:ListItem Value="0">不在线</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="ddlIsApproved" class="sr-only">是否允许登录</label>
                                    <asp:DropDownList ID="ddlIsApproved" runat="server" CssClass="form-control">
                                        <asp:ListItem Selected="True" Value="-1">===是否允许登录===</asp:ListItem>
                                        <asp:ListItem Value="1">允许</asp:ListItem>
                                        <asp:ListItem Value="0">不允许</asp:ListItem>
                                    </asp:DropDownList>
                                </div>
                                <div class="form-group">
                                    <label for="txtNickName" class="sr-only">微信昵称</label>
                                    <asp:TextBox ID="txtNickName" runat="server" placeholder="微信昵称" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtCountry" class="sr-only">国家</label>
                                    <asp:TextBox ID="txtCountry" runat="server" placeholder="国家" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtProvince" class="sr-only">省份</label>
                                    <asp:TextBox ID="txtProvince" runat="server" placeholder="省份" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtCity" class="sr-only">城市</label>
                                    <asp:TextBox ID="txtCity" runat="server" placeholder="城市" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtStartCreationDate" class="sr-only">开始注册时间</label>
                                    <asp:TextBox ID="txtStartCreationDate" runat="server" placeholder="开始注册时间" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtEndCreationDate" class="sr-only">结束注册时间</label>
                                    <asp:TextBox ID="txtEndCreationDate" runat="server" placeholder="结束注册时间" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtStartLastActivityDate" class="sr-only">开始活跃时间</label>
                                    <asp:TextBox ID="txtStartLastActivityDate" runat="server" placeholder="开始活跃时间" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                                <div class="form-group">
                                    <label for="txtEndLastActivityDate" class="sr-only">结束活跃时间</label>
                                    <asp:TextBox ID="txtEndLastActivityDate" runat="server" placeholder="结束活跃时间" CssClass="form-control" Width="150"></asp:TextBox>
                                </div>
                            </div>
                            <div class="col-lg-2 center-block">
                                <asp:Button ID="btnSearch" runat="server" Text="查询" CssClass="btn btn-info" OnClick="btnSearch_Click" OnClientClick="return verifyCriteria();" />
                                <asp:Button ID="btnShowAll" runat="server" Text="全部用户" CssClass="btn btn-warning" OnClick="btnShowAll_Click" />
                                <div class="search-result" id="divSearchResult">查询用户数量：<asp:Label ID="lblSearchResult" runat="server" CssClass="badge" Text=""></asp:Label></div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-12">
                <asp:GridView ID="gvUserList" runat="server" AllowCustomPaging="True" AllowPaging="True" AutoGenerateColumns="False" CssClass="table table-striped table-hover table-responsive" DataKeyNames="ProviderUserKey" DataSourceID="odsUserList" OnRowDataBound="gvUserList_RowDataBound" PagerSettings-Mode="NumericFirstLast" PagerStyle-CssClass="pager">
                    <Columns>
                        <asp:TemplateField HeaderText="微信头像">
                            <ItemTemplate>
                                <asp:HyperLink ID="HyperLink1" runat="server" ImageUrl='<%# Eval("HeadImgUrl") %>' NavigateUrl='<%# Eval("HeadImgUrl") %>' Target="_blank" CssClass="head-img"></asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="微信昵称" SortExpression="NickName">
                            <ItemTemplate>
                                <asp:Label ID="lblNickName" runat="server" Text='<%# ((bool)Eval("Sex")?"<i class=\"fa fa-mars\" style=\"color:blue;\"></i>&nbsp;":"<i class=\"fa fa-venus\" style=\"color:deeppink;\"></i>&nbsp;")+ Eval("NickName").ToString()+(!string.IsNullOrEmpty(Eval("Privilege").ToString())?string.Format("<br/>【{0}】",Eval("Privilege").ToString()):string.Empty) %>' ToolTip='<%# "微信OpenID:"+Eval("OpenID") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="地区" SortExpression="Country">
                            <ItemTemplate>
                                <p title='<%# Eval("ClientIP") %>'>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Eval("Country") %>'></asp:Label>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Eval("Province") %>'></asp:Label>
                                    <asp:Label ID="Label4" runat="server" Text='<%# Eval("City") %>'></asp:Label>
                                </p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="注册时间/活跃时间" SortExpression="CreationDate">
                            <ItemTemplate>
                                <p title="注册时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="Label2" runat="server" Text='<%# Eval("CreationDate") %>'></asp:Label></p>
                                <p title="最近活跃时间"><i class="fa fa-clock-o"></i>&nbsp;<asp:Label ID="Label5" runat="server" Text='<%# Eval("LastActivityDate") %>'></asp:Label></p>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否在线" SortExpression="IsOnline">
                            <ItemTemplate>
                                <div id="divIsOnline" runat="server"></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否关注公众号" SortExpression="IsSubscribe">
                            <ItemTemplate>
                                <div id="divIsSubscribe" runat="server"></div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="是否允许登录" SortExpression="IsApproved">
                            <ItemTemplate>
                                <div class="checkbox">
                                    <label>
                                        <asp:CheckBox ID="cbIsApproved" runat="server" Checked='<%# Bind("IsApproved") %>' AutoPostBack="True" OnCheckedChanged="cbIsApproved_CheckedChanged" />
                                    </label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:HyperLinkField DataNavigateUrlFields="OpenID" DataNavigateUrlFormatString="ManageOrder.aspx?OpenID={0}" DataTextField="OrderCount" HeaderText="订单数" />
                        <asp:TemplateField HeaderText="发送微信模板消息">
                            <ItemTemplate>
                                <asp:TextBox CssClass="form-control" ID="tbTmplMsg" runat="server" Rows="3"></asp:TextBox>
                                <asp:Button ID="btnTmplMsg" runat="server" Text="发送" CssClass="btn btn-default btn-sm" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                    <PagerSettings Mode="NumericFirstLast"></PagerSettings>

                    <PagerStyle CssClass="pager"></PagerStyle>
                </asp:GridView>
                <asp:ObjectDataSource ID="odsUserList" runat="server" DataObjectTypeName="WeChatUser" SelectMethod="FindUsersPager" TypeName="WeChatUserDAO" EnablePaging="True" OnSelecting="odsUserList_Selecting" SelectCountMethod="FindUserCount" OnSelected="odsUserList_Selected"></asp:ObjectDataSource>
            </div>
        </div>
    </div>

    <script>

        requirejs(['jquery', 'jqueryui'], function ($) {

            $(function () {

                requirejs(['datepickerCN'], function () {

                    //http://api.jqueryui.com/datepicker/
                    $("#txtStartCreationDate").datepicker({ dateFormat: 'yy-mm-dd' });
                    $("#txtEndCreationDate").datepicker({ dateFormat: 'yy-mm-dd' });
                    $("#txtStartLastActivityDate").datepicker({ dateFormat: 'yy-mm-dd' });
                    $("#txtEndLastActivityDate").datepicker({ dateFormat: 'yy-mm-dd' });

                    $("#txtStartCreationDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtEndCreationDate").val() != "")
                            if ($(this).val() > $("#txtEndCreationDate").val()) {
                                alert("开始时间必须早于结束时间。");
                                $(this).val("");
                            }
                    });
                    $("#txtEndCreationDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtStartCreationDate").val() != "")
                            if ($(this).val() < $("#txtStartCreationDate").val()) {
                                alert("结束时间必须晚于开始时间。");
                                $(this).val("");
                            }
                    });

                    $("#txtStartLastActivityDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtEndLastActivityDate").val() != "")
                            if ($(this).val() > $("#txtEndLastActivityDate").val()) {
                                alert("开始时间必须早于结束时间。");
                                $(this).val("");
                            }
                    });

                    $("#txtEndLastActivityDate").on("change", function () {
                        if ($(this).val() != "" && $("#txtStartLastActivityDate").val() != "")
                            if ($(this).val() < $("#txtStartLastActivityDate").val()) {
                                alert("结束时间必须晚于开始时间。");
                                $(this).val("");
                            }
                    });

                });
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

</asp:Content>
