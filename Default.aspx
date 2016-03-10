<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" lang="zh-CN">
<head>
    <title>FruitU</title>
    <link rel="shortcut icon" href="images/FruitU.ico" type="image/x-icon" />
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta name="keywords" content="fruit,slice,juice,水果,果汁,切片" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="black" />
    <meta name="format-detection" content="telephone=no" />
    <meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1.0, maximum-scale=1.0" />
    <link href="http://apps.bdimg.com/libs/bootstrap/3.3.0/css/bootstrap.min.css" rel="stylesheet" />
    <link href="css/font-awesome.min.css" rel="stylesheet" />
    <link href="css/flexslider.css" rel="stylesheet" media="screen" />
    <link href="css/common.css" rel="stylesheet" />
    <link href="css/default.css" rel="stylesheet" />
    <link href="css/loading.css" rel="stylesheet" />
</head>
<body>
    <form runat="server" id="frmProdList">
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
                    <div class="flexslider">
                        <ul class="slides">
                            <li>
                                <img src="images/b2.jpg" />
                            </li>
                            <li>
                                <img src="images/b3.jpg" />
                            </li>
                        </ul>
                    </div>
                </div>
            </div>
            <div class="row category">
                <div class="col-xs-12">
                    <ul class="category">
                        <li id="liFruit" onclick="findProductByCategoryID(28)">
                            <img src="images/b1.jpg" /><br />
                            精选水果
                        </li>
                        <li id="liJuice" onclick="findProductByCategoryID(1)">
                            <img src="images/guozhi1.jpg" /><br />
                            鲜榨果汁
                        </li>
                        <li id="liJuiceDIY" onclick="findProductByCategoryID(3)">
                            <img src="images/guozhi4.png" /><br />
                            果汁DIY
                        </li>
                    </ul>
                </div>
            </div>
            <div id="divProdItems" class="row prod-item-row">
            </div>
        </div>
        <!-- #include file="footer.html" -->
    </form>
</body>

<!-- Declare a JsRender template, in a script block: -->
<script id="tmplProdPage" type="text/x-jsrender">
    <div class="col-xs-12 col-sm-4 col-md-3 col-lg-2" onclick="location.href='ProductDetail.aspx?ProdID={{:ID}}'">
        <div class="prod-item">
            {{for FruitImgList}}
                    {{if MainImg}}
                        <img class="img-responsive img-circle main-img" src="images/{{:ImgName}}" alt="{{:ImgDesc}}" />
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

<script>

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

    requirejs(['jquery'], function ($) {
        $(function () {

            //轮播图
            requirejs(['jquery', 'flexslider'], function ($) {
                $('.flexslider').flexslider({
                    animation: "slide"
                });
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

        });
    });

    function findProductByCategoryID(categoryID) {
        //根据当前类别高亮菜单项
        switch (categoryID) {
            case 28:
                $("#liFruit").addClass("category-selected");
                $("#liJuice").removeClass("category-selected");
                $("#liJuiceDIY").removeClass("category-selected");
                break;
            case 1:
                $("#liFruit").removeClass("category-selected");
                $("#liJuice").addClass("category-selected");
                $("#liJuiceDIY").removeClass("category-selected");
                break;
            case 3:
                $("#liFruit").removeClass("category-selected");
                $("#liJuice").removeClass("category-selected");
                $("#liJuiceDIY").addClass("category-selected");
                break;
            default:
                $("#liFruit").removeClass("category-selected");
                $("#liJuice").removeClass("category-selected");
                $("#liJuiceDIY").removeClass("category-selected");
                break;
        }

        $.pager.loadPage({ pageQueryCriteria: { CategoryID: categoryID }, pageIndex: 1 });
    }
</script>

</html>
