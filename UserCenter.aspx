<%@ Page Title="个人中心" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="UserCenter.aspx.cs" Inherits="UserCenter" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/UserCenter.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row header">
            <div class="col-xs-4 user-portrait">
                <asp:Image ID="imgPortrait" runat="server" />
            </div>
            <div class="col-xs-8 user-info">
                <asp:Label ID="lblNickName" runat="server" CssClass="nick-name"></asp:Label>
                <br />
                <span class="my-points">我的积分：<asp:Label ID="lblMemberPoints" CssClass="member-points" runat="server"></asp:Label></span>
                <br />
                <asp:Label ID="lblPrivilege" runat="server"></asp:Label>
            </div>
        </div>
        <div class="row text-center">
            <div id="divWxCard" class="col-xs-12">
                <div class="btn btn-block btn-info">
                    <img src="images/wxcard1.png" class="wx-card" />&nbsp;&nbsp;微信卡券<span id="spWxCardCount"></span></div>
            </div>
        </div>
        <div class="row text-center">
            <div id="divWxAddress" class="col-xs-12" onclick="selectDeliverAddress();">
                <div class="btn btn-block btn-primary"><i class="fa fa-map-marker"></i>&nbsp;&nbsp;收货地址</div>
            </div>
        </div>
        <div class="row text-center">
            <div class="col-xs-12" onclick="openModal('GroupPurchase');">
                <div class="btn btn-block btn-warning"><i class="fa fa-group"></i>&nbsp;&nbsp;团购规则</div>
            </div>
        </div>
        <div class="row text-center">
            <div class="col-xs-12" onclick="openModal('MemberPoints');">
                <div class="btn btn-block btn-danger"><i class="fa fa-line-chart"></i>&nbsp;&nbsp;积分规则</div>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12" onclick="openModal('QRCode');">
                <div class="btn btn-block btn-wx-qrcode"><i class="fa fa-wechat"></i>&nbsp;&nbsp;微信客服</div>
            </div>
        </div>
        <%--        <div class="row">
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
        </div>--%>
    </div>
    <div class="md-modal md-effect-1" id="divModal">
        <div class="md-content">
            <div id="QRCode">
                <img id="imgQRCode" src="images/FruitUQRCode.jpg" />
                <h5 class="text-danger">长按二维码关注我们，接收订单即时消息</h5>
            </div>
            <div id="MemberPoints" class="text-left member-points-rule">
                <h4 class="text-center">积分规则</h4>
                <ul>
                    <li>您在Fruit U消费成功后，消费金额将自动转化为积分，每消费1元可累计1积分，积分永久有效。</li>
                    <li>积分可在下次消费时直接抵扣现金，<asp:Label CssClass="member-points-exchage-rate" ID="lblMemberPointsExchageRate" runat="server" Text=""></asp:Label>积分抵扣1元，单次消费最多可抵扣订单总金额的50%。</li>
                    <li>您可以分享页面给朋友、微信群，或者发布在朋友圈，好友点击消费后您可获得100积分（5元）奖励。</li>
                    <li>您的会员积分可在个人中心查看。</li>
                </ul>
            </div>
            <div id="GroupPurchase" class="text-left group-purchase-rule">
                <h4 class="text-center">团购规则</h4>
                <ul>
                    <li>您可以在有团购标签的商品中，选择发起团购，享受团购价；或者以零售价单独购买。</li>
                    <li>团购商品订单支付成功后，您可以在“我的订单”中点击“邀请拼团”，分享给朋友、微信群、朋友圈，邀请拼团。</li>
                    <li>当您的团购达到规定人数后，我们将发货给所有参团人员，请注意Fruit U公众号消息。</li>
                    <li>如果您的团购在活动期限内没有达到规定人数，我们将退款给所有参团人员，请注意Fruit U公众号消息。</li>
                </ul>
            </div>
        </div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <script>

        requirejs(['jquery', 'jweixin110'], function ($, wx) {

            $(function () {
                //注册选择收货人信息单击事件处理函数
                //$("#divWxAddress").on("click", wx, wxOpenAddress);

                if (typeof cardParam == "object" && cardParam.cardSign != "undefined") {
                    $.extend(cardParam, {
                        complete: function () {
                            alert("微信优惠券可在下单时使用");
                        }
                    });

                    //注册选择微信卡券点击事件处理函数
                    $("#divWxCard").on("click", function () {
                        wx.chooseCard(cardParam);
                    });
                }
                else {
                    console.warn("微信卡券参数错误");
                }

                //显示微信卡券数量
                if (typeof wxCard != undefined && wxCard.length > 0) {
                    $("#spWxCardCount").text("(" + wxCard.length + ")");
                }

                //点击遮罩层关闭模式窗口
                $(".md-overlay").on("click", closeModal);

            });
        });

        //显示模式窗口
        function openModal(content) {
            switch (content) {
                case 'MemberPoints':
                    $("#MemberPoints").show();
                    $("#QRCode").hide();
                    $("#GroupPurchase").hide();
                    break;
                case 'QRCode':
                    $("#MemberPoints").hide();
                    $("#QRCode").show();
                    $("#GroupPurchase").hide();
                    break;
                case 'GroupPurchase':
                    $("#MemberPoints").hide();
                    $("#QRCode").hide();
                    $("#GroupPurchase").show();
                    break;
            }
            $("#divModal").addClass("md-show");
        }

        //关闭模式对话框
        function closeModal() {
            $("#divModal").removeClass("md-show");
        }

        //获取微信地址信息的JSSDK接口，调用微信JS函数openAddress
        function wxOpenAddress(event) {
            var wx = event.data;
            wx.openAddress({
                success: function (res) {
                    // 用户成功拉出地址 
                    if (res.errMsg.indexOf("ok") != -1) {
                        alert("收货地址可在下单时选用");
                    }
                    else {
                        alert("无法获取您的地址");
                    }
                },
                cancel: function () {
                    // 用户取消拉出地址
                    alert("收货地址可在下单时选用");
                }
            });
        }

        //获取微信用户地址
        function editAddress() {
            WeixinJSBridge.invoke(
                'editAddress',
                wxEditAddrParam,   //后端获取的参数
                  function (res) {
                      if (res.err_msg.indexOf("ok") != -1) {
                          alert("收货地址可在下单时选用");
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

        function selectDeliverAddress() {
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

    </script>
</asp:Content>
