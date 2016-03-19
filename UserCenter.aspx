<%@ Page Title="个人中心" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="UserCenter.aspx.cs" Inherits="UserCenter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/UserCenter.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row header">
            <div class="col-xs-12">
                <asp:Image ID="imgPortrait" runat="server" CssClass="user-portrait" />
                &nbsp;<asp:Label ID="lblNickName" runat="server"></asp:Label>
                <br />
                <asp:Label ID="lblPrivilege" runat="server"></asp:Label>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-6" onclick="SelectDeliverAddress();">
                <div class="info-block">
                    <i class="fa fa-truck"></i>
                    <div>收货地址</div>
                </div>
            </div>
            <div class="col-xs-6" onclick="location.href='MyOrders.aspx'">
                <div class="info-block">
                    <i class="fa fa-list"></i>
                    <div>我的订单</div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-6">
                <div class="info-block">
                    <i class="fa fa-money"></i>
                    <div>会员积分</div>
                </div>
            </div>
            <div class="col-xs-6" onclick="switchQRCode();">
                <div class="info-block">
                    <i class="fa fa-comments"></i>
                    <div>微信客服</div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-6" onclick="location.href='tel:4008888888'">
                <div class="info-block">
                    <i class="fa fa-phone"></i>
                    <div>400热线</div>
                </div>
            </div>
            <div class="col-xs-6" onclick="location.href='http://wpa.qq.com/msgrd?v=3&uin=58721717&site=qq&menu=yes'">
                <div class="info-block">
                    <i class="fa fa-qq"></i>
                    <div>QQ客服</div>
                </div>
            </div>
        </div>
    </div>
    <div class="qrcode">
        <img src="images/FruitUQRCode.jpg" />
        <h5>长按二维码关注我们</h5>
    </div>

    <script>
        //获取微信用户地址
        function editAddress() {
            WeixinJSBridge.invoke(
                'editAddress',
                wxEditAddrParam,   //后端获取的参数
                  function (res) {
                      if (res.err_msg.indexOf("ok") != -1) {
                          alert("收货地址编辑成功，可在下单时选用。");
                      }
                      else {
                          if (res.err_msg.indexOf("function_not_exist") != -1) {
                              alert("您的微信版本过低，请升级到最新版。");
                          }
                          else {
                              if (res.err_msg.indexOf("fail") != -1 || res.err_msg.indexOf("access_denied") != -1 || res.err_msg.indexOf("unkonwPermission") != -1 || res.err_msg.indexOf("domain") != -1) {
                                  alert("无法获取您的地址，请通知我们处理。");
                                  console.log(res.err_msg);
                              }
                          }
                      }
                  }
              );
        }

        function SelectDeliverAddress() {
            if (typeof WeixinJSBridge == "undefined") {
                if (document.addEventListener) {
                    document.addEventListener('WeixinJSBridgeReady', editAddress, false);
                }
                else if (document.attachEvent) {
                    document.attachEvent('WeixinJSBridgeReady', editAddress);
                    document.attachEvent('onWeixinJSBridgeReady', editAddress);
                }
            }
            else {
                editAddress();
            }
        }

        function switchQRCode() {
            $("div.qrcode").slideToggle();
        }

    </script>
</asp:Content>
