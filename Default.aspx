<%@ Page Title="首页" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <link href="css/default.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div class="container">
        <div class="row header">
            <div class="col-xs-2 logo">
                <img src="images/FruitU.jpg" class="logo-img" />
            </div>
            <div class="col-xs-8 search-input">
                <input id="searchInput" type="text" placeholder="按话筒，说出你想吃啥..." />
            </div>
            <div class="col-xs-2 search-button">
                <button type="button" class="fa fa-microphone" id="btnMic"></button>
            </div>
        </div>
        <div class="row">
            <div class="col-xs-12">
                <div id="jssorSliderContainer" class="center-block" style="position: relative; top: 0px; left: 0px; min-width: 100%; max-width:100%; height: 150px;">
                    <!-- Slides Container -->
                    <div id="divSlides" runat="server" u="slides" style="cursor: grab; position: absolute; overflow: hidden; left: 0px; top: 0px; min-width: 100%; height: 150px;"></div>
                </div>
            </div>
        </div>
        <div class="row category">
            <div class="col-xs-12">
                <ul class="category">
                    <li id="liFruit" data-categoryid="28">
                        <img src="images/b1.jpg" /><br />
                        精选水果
                    </li>
                    <li id="liJuice" data-categoryid="1">
                        <img src="images/guozhi1.jpg" /><br />
                        鲜榨果汁
                    </li>
                    <li id="liJuiceDIY" data-categoryid="3">
                        <img src="images/guozhi4.png" /><br />
                        果汁DIY
                    </li>
                </ul>
            </div>
        </div>
        <div id="divProdItems" class="row prod-item-row">
        </div>
        <div class="qq-login">
            <a href="login.aspx"><img src="images/Connect_logo_7.png" /></a>
            <div id="wb_connect_btn"></div>
        </div>
    </div>

    <!-- Declare a JsRender template, in a script block: -->
    <script id="tmplProdPage" type="text/x-jsrender">
        <div class="col-xs-12 col-sm-4 col-md-3 col-lg-2 prod-item-col" onclick="location.href='ProductDetail.aspx?ProdID={{:ID}}'">
            <div class="prod-item">
                {{for FruitImgList}}
                    {{if MainImg}}
                        <img class="img-responsive main-img" src="images/{{:ImgName}}" alt="{{:ImgDesc}}" />
                {{/if}}   
                {{/for}}            
                <div class="prod-name">{{:FruitName}}</div>
                <span class="prod-price">{{:FruitPrice}}</span><span class="prod-unit">元/{{:FruitUnit}}</span><div class="prod-desc">{{:FruitDesc}}</div>
                {{if IsSticky}}
                    <span class="sticky-prod"><i class="fa fa-thumbs-up fa-lg"></i>掌柜推荐</span>
                {{/if}}
                {{if TopSellingOnMonth}}
                    <span class="top-selling-month-prod"><i class="fa fa-trophy fa-lg"></i>本月爆款</span>
                {{/if}}
                <hr />
                {{for FruitImgList}}
                {{if !MainImg && !DetailImg}}
                    <img class="img-responsive img-rounded side-img" src="images/{{:ImgName}}" alt="{{:ImgDesc}}" />
                {{/if}}
            {{/for}}
            </div>
        </div>
    </script>

    <script src="http://tjs.sjs.sinajs.cn/open/api/js/wb.js?appkey=3535172822" type="text/javascript" charset="utf-8"></script>

    <script>

        requirejs(['jquery', 'jssorslider'], function ($) {
            $(function () {

                //轮播图
                var jssor_slider1 = new $JssorSlider$('jssorSliderContainer',
                    {
                        $AutoPlay: true,
                        $ThumbnailNavigatorOptions: true,
                        $FillMode: 2,
                        $BulletNavigatorOptions:
                        {
                            $Class: '$JssorBulletNavigator$',
                            $ChanceToShow: 2
                        }
                    });

                requirejs(['pager'], function () {

                    $.pager.init({
                        pageSize: pageSize,
                        pageQueryURL: 'ProdListPager.ashx',
                        pageTemplate: '#tmplProdPage',
                        pageContainer: '#divProdItems',
                    });

                    $.pager.loadPage();
                });

                $("ul.category li").on("click", findProductByCategoryID);

            });
        });

        //根据当前类别高亮菜单项
        function findProductByCategoryID() {
            var $li = $(event.currentTarget);
            var categoryID = $li.data("categoryid");
            $li.addClass("category-selected").siblings().each(function () {
                $(this).removeClass("category-selected");
            });

            $.pager.loadPage({ pageQueryCriteria: { CategoryID: categoryID }, pageIndex: 1 });
        }

        requirejs(['jquery', 'jweixin'],
        function ($, wx) {
            $.extend(wxJsApiParam, {
                debug: false,
                jsApiList: [
            'checkJsApi',
            'onMenuShareTimeline',
            'onMenuShareAppMessage',
            'onMenuShareQQ',
            'onMenuShareWeibo',
            'onMenuShareQZone',
            'hideMenuItems',
            'showMenuItems',
            'hideAllNonBaseMenuItem',
            'showAllNonBaseMenuItem',
            'translateVoice',
            'startRecord',
            'stopRecord',
            'onVoiceRecordEnd',
            'playVoice',
            'onVoicePlayEnd',
            'pauseVoice',
            'stopVoice',
            'uploadVoice',
            'downloadVoice',
            'chooseImage',
            'previewImage',
            'uploadImage',
            'downloadImage',
            'getNetworkType',
            'openLocation',
            'getLocation',
            'hideOptionMenu',
            'showOptionMenu',
            'closeWindow',
            'scanQRCode',
            'chooseWXPay',
            'openProductSpecificView',
            'addCard',
            'chooseCard',
            'openCard'
                ]
            });

            //所有需要使用JS-SDK的页面必须先注入配置信息，否则将无法调用（同一个url仅需调用一次，对于变化url的SPA的web app可在每次url变化时进行调用,目前Android微信客户端不支持pushState的H5新特性，所以使用pushState来实现web app的页面会导致签名失败，此问题会在Android6.2中修复）。
            wx.config(wxJsApiParam);

            var voice = {
                localId: '',
                serverId: ''
            };

            // 4 音频接口
            // 4.2 开始录音
            $("#btnMic").on("click", function () {
                wx.startRecord({
                    success: function () {
                        //3秒钟后停止录音
                        setTimeout(function () {
                            wx.stopRecord({
                                success: function (res) {
                                    //语音识别
                                    wx.translateVoice({
                                        localId: res.localId,
                                        complete: function (res) {
                                            if (res.hasOwnProperty('translateResult')) {
                                                var text = res.translateResult;
                                                text = text.replace("。", "").substr(text.indexOf("吃") + 1);
                                                $("#searchInput").val(text);

                                                $($.pager).one("onPageLoaded", function (event, data) {
                                                    //模糊搜索结果后给出提示信息，回到首页展示全部商品
                                                    $($.pager.settings.pageContainer).append('<h5 class="text-center"><a href="default.aspx" class="text-danger">不是您想找的？点我看看其他的。</a></h5>');
                                                });

                                                $.pager.loadPage({ pageQueryCriteria: { ProdName: text }, pageIndex: 1 });

                                            } else {
                                                alert('听不清哦，再说一遍吧。');
                                            }
                                        }
                                    });
                                },
                                fail: function (res) {
                                    alert('听不清哦，再说一遍吧。');
                                }
                            });
                        }, 3000);
                    },
                    cancel: function () {
                        alert('不要拒绝呀');
                    }
                });
            });
        });

        WB2.anyWhere(function (W) {
            W.widget.connectButton({
                id: "wb_connect_btn",
                type: '3,2',
                callback: {
                    login: function (o) { //登录后的回调函数
                        alert("login: " + o.screen_name)
                    },
                    logout: function () { //退出后的回调函数
                        alert('logout');
                    }
                }
            });
        });
    </script>

</asp:Content>
