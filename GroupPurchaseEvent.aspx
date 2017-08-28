<%@ Page Title="团购活动" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="GroupPurchaseEvent.aspx.cs" Inherits="GroupPurchaseEventInfo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/GroupPurchaseEvent.css" rel="stylesheet" />
    <link href="Scripts/modal/component.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container text-center" id="divContainer" runat="server">
        <div class="row prod-item">
            <div class="col-xs-4 prod-item-left">
                <asp:Image ID="imgProdImg" runat="server" CssClass="prod-img" />
            </div>
            <div class="col-xs-8 text-left prod-item-right">
                <div class="group-name">
                    <asp:Label ID="lblGroupName" runat="server" Text=""></asp:Label>
                </div>
                <div class="group-desc">
                    <asp:Label ID="lblGroupDesc" runat="server" Text=""></asp:Label>
                </div>
                <div class="prod-price">
                    零售价：￥<del><asp:Label ID="lblProdPrice" runat="server" Text=""></asp:Label></del>元/<asp:Label ID="lblProdPriceUnit" runat="server" Text=""></asp:Label>
                </div>
                <div>
                    <asp:Label ID="lblRequiredNumber" runat="server" Text=""></asp:Label>人团：￥<asp:Label ID="lblGroupPrice" CssClass="group-price" runat="server" Text=""></asp:Label>元/<asp:Label ID="lblGroupPriceUnit" runat="server" Text=""></asp:Label>
                </div>
                <div class="group-period">
                    开始时间：<asp:Label ID="lblEventStardDate" runat="server" Text=""></asp:Label><br />
                    结束时间：<asp:Label ID="lblEventEndDate" runat="server" Text=""></asp:Label>
                </div>
            </div>
        </div>
        <hr />
        <div class="row">
            <div class="col-xs-12" runat="server" id="divCountDown">
                <span id="spCountDown" class="count-down"></span>
            </div>
        </div>
        <hr />
        <div class="row">
            <div runat="server" class="col-xs-12 user-portrait" id="divGroupEventMemberHeadImg">
            </div>
            <div class="col-xs-12" runat="server" id="divLeftNumber">
                还差<asp:Label ID="lblLeftNumber" runat="server" Text=""></asp:Label>人，快来拼团吧~
            </div>
        </div>
        <hr />
        <div runat="server" class="row text-left" id="divGroupEventMember">
        </div>
        <div class="col-xs-12 share-group-event" runat="server" id="divShareGroupEvent">
            <button type="button" class="btn btn-warning" onclick="openModal()"><i class="fa fa-share-alt fa-lg fa-fw"></i>邀请拼团</button>
        </div>
        <div class="row cart-footer" runat="server" id="divJoinGroupEvent">
            <div class="col-xs-12 join-group-event">
                <label class="sr-only" for="txtQty">购买数量</label>
                <span class="input-group">
                    <span id="btnDesc" class="input-group-addon">-</span>
                    <input class="form-control" type="text" id="txtQty" value="1" />
                    <span id="btnAsc" class="input-group-addon">+</span>
                </span>
                <button type="button" class="btn btn-danger" onclick="addToCart()"><i class="fa fa-group fa-lg fa-fw"></i>参加拼团</button>
            </div>
        </div>
    </div>
    <asp:Label ID="lblMsg" runat="server" Text="" class="alert alert-danger center-block text-center" role="alert" Visible="False"></asp:Label>
    <div class="md-wxshare" id="divModal">
        <div class="weixin-tip-arrow"><i class="fa fa-hand-o-up fa-3x"></i></div>
        <div class="weixin-tip-text">把团购分享给朋友、微信群、朋友圈吧！</div>
    </div>

    <div class="md-overlay"></div>
    <!-- the overlay element -->

    <script>
        //倒计时定时器
        var timerID = null;
        var timerRunning = false;

        //团购活动的截止时间，用于倒计时
        var groupEventEndDate;

        requirejs(['jquery'], function ($) {
            $(function () {
                if (!!groupEvent) {
                    groupEventEndDate = new Date(groupEvent.GroupPurchase.EndDate);
                    //设置微信分享JS参数
                    setWxShareParam();

                    if ($("#spCountDown").is(":visible")) {
                        //启动倒计时
                        startClock();
                    }
                } else {
                    throw new Error("团购活动数据错误");
                }

                //递减商品数量
                $("#btnDesc").on("click", function () {
                    var currQty = $("#txtQty").val();
                    if (!isNaN(currQty)) {
                        currQty = parseInt(currQty);
                        if (currQty > 1) {
                            currQty--;
                        }
                        else {
                            currQty = 1;
                        }
                    }
                    else {
                        currQty = 1;
                    }

                    //显示更新的数量
                    $("#txtQty").val(currQty);
                });

                //递增商品数量
                $("#btnAsc").on("click", function () {
                    var currQty = $("#txtQty").val();
                    var inventory = groupEvent.GroupPurchase.Product.InventoryQty;
                    if (!isNaN(currQty)) {
                        currQty = parseInt(currQty);
                        inventory = parseInt(inventory);
                        if (inventory == -1 || currQty < inventory) {
                            currQty++;
                        }
                    }
                    else {
                        currQty = 1;
                    }

                    //显示更新的数量
                    $("#txtQty").val(currQty);

                });

                //点击遮罩层时关闭弹出层
                $(".md-overlay").on("click", closeModal);
            });
        });

        //启动定时器
        function startClock() {
            stopClock();
            countDown();
        }

        //停止定时器
        function stopClock() {
            if (timerRunning) {
                clearTimeout(timerID);
            }
            timerRunning = false;
        }

        //显示倒计时
        function countDown() {
            var today = new Date();
            //如果客户端时间不是北京时间，则转换为北京时间
            if (today.getTimezoneOffset() != -480) {
                today = transToBeijingTime(today);
            }

            var nowYear = today.getFullYear();
            var nowMonth = today.getMonth() + 1;
            var nowDate = today.getDate();
            var nowHour = today.getHours();
            var nowMinute = today.getMinutes();
            var nowSecond = today.getSeconds();

            var yearLeft = groupEventEndDate.getFullYear() - nowYear;
            var monthLeft = groupEventEndDate.getMonth() + 1 - nowMonth;
            var dateLeft = groupEventEndDate.getDate() - nowDate;
            var hourLeft = groupEventEndDate.getHours() - nowHour;
            var minuteLeft = groupEventEndDate.getMinutes() - nowMinute;
            var secondLeft = groupEventEndDate.getSeconds() - nowSecond;

            if (secondLeft < 0) {
                secondLeft = 60 + secondLeft;
                minuteLeft--;
            }
            if (minuteLeft < 0) {
                minuteLeft = 60 + minuteLeft;
                hourLeft--;
            }
            if (hourLeft < 0) {
                hourLeft = 24 + hourLeft;
                dateLeft--;
            }
            if (dateLeft < 0) {
                switch (groupEventEndDate.getMonth() + 1 - 1) {
                    case 1:
                    case 3:
                    case 5:
                    case 7:
                    case 8:
                    case 10:
                    case 12:
                        dateLeft = 31 + dateLeft;
                        break;
                    case 4:
                    case 6:
                    case 9:
                    case 11:
                        dateLeft = 30 + dateLeft;
                        break;
                    case 2:
                        dateLeft = 28 + dateLeft;
                        break;
                }
                monthLeft--;
            }
            if (monthLeft < 0) {
                monthLeft = 12 + monthLeft;
                yearLeft--;
            }

            var countDownText = '剩余';
            if (yearLeft != 0) {
                countDownText = ' <span class="spYearLeft"> ' + yearLeft + ' </span>年';
            }
            if (monthLeft != 0) {
                countDownText += '<span class="spMonthLeft"> ' + monthLeft + ' </span>月';
            }
            if (dateLeft != 0) {
                countDownText += '<span class="spDateLeft"> ' + dateLeft + ' </span>天';
            }
            if (hourLeft != 0) {
                countDownText += '<span class="spHourLeft"> ' + hourLeft + ' </span>小时';
            }
            if (minuteLeft != 0) {
                countDownText += '<span class="spMinuteLeft"> ' + minuteLeft + ' </span>分';
            }
            if (secondLeft >= 0) {
                countDownText += '<span class="spSecondLeft"> ' + secondLeft + ' </span>秒, 结束';
            }
            if (secondLeft != 0 || minuteLeft != 0 || hourLeft != 0 || dateLeft != 0 || monthLeft != 0 || yearLeft != 0) {
                document.getElementById('spCountDown').innerHTML = countDownText;
                timerID = setTimeout("countDown()", 1000);
                timerRunning = true;
            } else {
                document.getElementById('spCountDown').innerHTML = "团购活动已经结束，欢迎下次参加！";
                stopClock();
            }
        }

        //转换为北京时间
        function transToBeijingTime(localDate) {
            if (!(localDate instanceof Date)) {
                throw new TypeError("参数必须为Date类型");
            }
            var localTime = localDate.getTime();
            var localOffset = localTime.getTimezoneOffset() * 60000; //获得当地时间偏移UTC的毫秒数
            var beijingTime = (localTime + localOffset) + (3600000 * 8);   //转换为北京时间
            return new Date(beijingTime);
        }

        //加入购物车
        function addToCart() {
            if (!!groupEvent) {
                var mainImg;
                var prod = groupEvent.GroupPurchase.Product;
                //查找商品主图
                for (var i = 0; i < prod.FruitImgList.length; i++) {
                    if (prod.FruitImgList[i]["MainImg"]) {
                        mainImg = prod.FruitImgList[i]["ImgName"];
                        break;
                    }
                }

                if (!mainImg) {
                    mainImg = webConfig.defaultImg;
                }
                var groupPurchase = new $.cart.GroupPurchase(groupEvent.GroupPurchase.ID, groupEvent.GroupPurchase.Name, groupEvent.GroupPurchase.Description, groupEvent.GroupPurchase.StartDate, groupEvent.GroupPurchase.EndDate, groupEvent.GroupPurchase.RequiredNumber, groupEvent.GroupPurchase.GroupPrice);
                //根据商品信息构造JS商品对象，包含团购和团购活动ID
                var prodItem = new $.cart.ProdItem(prod.ID, prod.FruitName, prod.FruitDesc, "images/" + mainImg, groupEvent.GroupPurchase.GroupPrice, parseInt($("input#txtQty").val()), prod.InventoryQty, groupPurchase, groupEvent.ID);
                //把商品对象插入到购物车里，并跳转到购物车页面
                if ($.cart.insertProdItem(prodItem)) {
                    location.href = 'MyCart.aspx';
                }
            }
            else {
                alert("团购活动数据异常");
                console.warn("var groupEvent=" + groupEvent);
            }
        }

        //设置微信分享的JS参数
        function setWxShareParam() {
            var mainImg;
            //查找商品主图
            for (var i = 0; i < groupEvent.GroupPurchase.Product.FruitImgList.length; i++) {
                if (groupEvent.GroupPurchase.Product.FruitImgList[i]["MainImg"]) {
                    mainImg = groupEvent.GroupPurchase.Product.FruitImgList[i]["ImgName"];
                    break;
                }
            }
            if (!mainImg) {
                mainImg = webConfig.defaultImg;
            }

            //设置微信分享参数
            requirejs(['jweixin'], function (wx) {
                wx.ready(function () {

                    wxShareInfo.title = '团购惊爆价' + groupEvent.GroupPurchase.GroupPrice + '元【' + groupEvent.GroupPurchase.Name + '】';
                    wxShareInfo.desc = groupEvent.GroupPurchase.Description;
                    wxShareInfo.link = location.origin + '/GroupPurchaseEvent.aspx?EventID=' + groupEvent.ID;
                    wxShareInfo.imgUrl = location.origin + '/images/' + mainImg;
                    wxShareInfo.success = shareTipForGroupEvent;
                    wxShareInfo.cancel = shareTipForGroupEvent;

                    //分享到朋友圈
                    wx.onMenuShareTimeline(wxShareInfo);

                    //分享给朋友
                    wx.onMenuShareAppMessage($.extend({}, wxShareInfo, { type: 'link', dataUrl: '' }));

                    //分享到QQ
                    wx.onMenuShareQQ(wxShareInfo);

                    //分享到腾讯微博
                    wx.onMenuShareWeibo(wxShareInfo);

                    //分享到QQ空间
                    wx.onMenuShareQZone(wxShareInfo);
                });
            });
        }

        //控制弹出层提示微信分享团购活动
        function shareTipForGroupEvent() {
            alert("团购达到人数后即发货。");
        }

        //显示模式窗口
        function openModal() {
            $("#divModal").addClass("md-show");
        }

        //关闭模式窗口
        function closeModal() {
            $("#divModal").removeClass("md-show");
        }

    </script>

</asp:Content>
