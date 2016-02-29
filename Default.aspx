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
        <div class="loading">
            <img src="images/loading.gif" />
        </div>
        <div class="loading-text">木有了噢，最后一页了！</div>
        <!-- #include file="footer.html" -->
    </form>


    <!-- 包括所有已编译的插件 -->
    <script src="http://apps.bdimg.com/libs/bootstrap/3.3.0/js/bootstrap.min.js"></script>
    <!-- HTML5 Shim 和 Respond.js 用于让 IE8 支持 HTML5元素和媒体查询 -->
    <!-- 注意： 如果通过 file://  引入 Respond.js 文件，则该文件无法起效果 -->
    <!--[if lt IE 9]>
         <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
         <script src="https://oss.maxcdn.com/libs/respond.js/1.3.0/respond.min.js"></script>
      <![endif]-->

    <!-- Modernizr -->
    <script src="Scripts/modernizr.js"></script>

    <!-- FlexSlider -->
    <script defer="defer" src="Scripts/jquery.flexslider.js"></script>

    <!-- weixin JS-SDK -->
    <script src="http://res.wx.qq.com/open/js/jweixin-1.0.0.js"></script>

<%--    <!-- http://masonry.desandro.com/ -->
    <script src="https://npmcdn.com/masonry-layout@4.0/dist/masonry.pkgd.min.js"></script>--%>
 
    <script src="Scripts/config.js"></script>

    <script>

        //所有需要使用JS-SDK的页面必须先注入配置信息，否则将无法调用（同一个url仅需调用一次，对于变化url的SPA的web app可在每次url变化时进行调用,目前Android微信客户端不支持pushState的H5新特性，所以使用pushState来实现web app的页面会导致签名失败，此问题会在Android6.2中修复）。
        wx.config({
            debug: false, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
            appId: appId, // 必填，公众号的唯一标识
            timestamp: timestamp, // 必填，生成签名的时间戳
            nonceStr: nonceStr, // 必填，生成签名的随机串
            signature: signature,// 必填，签名，见附录1
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
                                            $("#divProdItems").empty();
                                            pageIndex = 1;
                                            categoryID = "";
                                            prodName = text;
                                            loadProdList();
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

        //定义分页变量
        var pageIndex = 1, totalRows, totalPages = 1, categoryID, prodName;
        var docHeight = 0; //页面总长度  
        var scrollTop = 0;   //页面上滚高度  
        var winHeight = $(window).height(); //窗口可视高度
        var isPaging = false;   //是否正在分页处理

        $(function () {

            //轮播图
            $('.flexslider').flexslider({
                animation: "slide"
            });

            //根据初始屏幕大小设置合适的分页记录数
            pageSize = $.webConfig.setSuitablePageSize();

            //显示第一页
            loadProdList();

            //绑定浏览器滑动事件
            $(window).on("scroll", scrollUpPager);

            //根据窗口大小变动，调整分页记录数
            $(window).on("resize", function () {
                pageSize = $.webConfig.setSuitablePageSize();
            });

        });

        //浏览器向上滑动分页
        function scrollUpPager() {

            if (isPaging) {
                return;
            }

            // 判断窗口的滚动条是否接近页面底部
            docHeight = $(document).height();
            scrollTop = $(document).scrollTop();
            if (scrollTop + winHeight >= docHeight) {

                // 判断是否最后一页
                if (pageIndex <= totalPages) {

                    //标示正在处理分页，避免并发
                    isPaging = true;

                    loadProdList();

                } else {
                    $("div.loading-text").fadeIn("fast");
                    setTimeout("$('div.loading-text').fadeOut()", 2000);
                }
            }
        }

        function loadProdList() {

            // 显示加载进度条
            $("div.loading").fadeIn("fast");

            // ajax查询下一页数据并附加在底部
            $.ajax({
                url: "ProdListPager.ashx",
                data: { CategoryID: categoryID, ProdName: prodName, PageIndex: pageIndex, PageSize: pageSize, R: Math.random() },
                type: "GET",
                dataType: "json",
                success: function (jProdListPager) {

                    //当前商品
                    var htmlItem = "";

                    //遍历展示当前页所有商品信息，并添加到新增的div中
                    $.each(jProdListPager, function (iProd, nProd) {

                        if (this["ID"] != undefined) {

                            htmlItem += '<div class="col-xs-12 col-sm-4 col-md-3 col-lg-2 prod-item-col" onclick="location.href=\'ProductDetail.aspx?ProdID=' + this["ID"] + '\'" style="display:none;"><div class="prod-item">';

                            //商品主图
                            if (this["FruitImgList"] != null) {
                                $(this["FruitImgList"]).each(function (iImg, nImg) {
                                    if (this["MainImg"]) {
                                        htmlItem += '<img class="img-responsive img-circle main-img" src="images/' + this["ImgName"] + '" alt="' + this["ImgDesc"] + '"/>';
                                        return;
                                    }
                                });
                            }

                            //商品名称、价格、单位、描述
                            htmlItem += '<div class="prod-name">' + this["FruitName"] + '</div><span class="prod-price">' + this["FruitPrice"] + '</span><span class="prod-unit">元/' + this["FruitUnit"] + '</span><div class="prod-desc">' + this["FruitDesc"] + '</div>';

                            //置顶商品标识
                            if (this["IsSticky"]) {
                                htmlItem += '<span class="sticky-prod"><i class="fa fa-thumbs-up fa-lg"></i>掌柜推荐</span>';
                            }

                            //当月爆款商品标识
                            if (this["TopSellingOnMonth"]) {
                                htmlItem += '<span class="top-selling-month-prod"><i class="fa fa-trophy fa-lg"></i>本月爆款</span>';
                            }

                            //商品配图
                            if (this["FruitImgList"] != null && this["FruitImgList"].length > 1) {
                                var showHR = false;
                                $(this["FruitImgList"]).each(function (iImg, nImg) {
                                    if (!this["MainImg"] && !this["DetailImg"]) {
                                        if (!showHR) {
                                            htmlItem += '<hr />';
                                            showHR = true;
                                        }
                                        htmlItem += '<img class="img-responsive img-rounded side-img" src="images/' + this["ImgName"] + '" alt="' + this["ImgDesc"] + '"/>';
                                    }
                                });
                            }

                            htmlItem += '</div></div>';

                        }
                        else {
                            //如果是第一页（totalRows == undefined）；或用户在翻页过程中，如商品数有变动，则更新当前的记录总数，并重新计算页数
                            if (totalRows != this["TotalRows"]) {
                                totalRows = this["TotalRows"];

                                //计算总页数
                                if (totalRows % pageSize == 0) {
                                    totalPages = parseInt(totalRows / pageSize);
                                }
                                else {
                                    totalPages = parseInt(totalRows / pageSize) + 1;
                                }
                            }
                        }
                    });

                    //追加并淡出新增的div
                    $(htmlItem).appendTo($("#divProdItems")).fadeIn(1000);

                    //$(".prod-item-row").masonry({
                    //    // options
                    //    itemSelector: '.prod-item-col',
                    //    columnWidth: '.col-xs-12 .col-sm-4 .col-md-3 .col-lg-2',
                    //    percentPosition: true
                    //});

                    //加载成功后，页数+1
                    pageIndex++;

                    // 隐藏加载进度条
                    $("div.loading").fadeOut();

                    //模糊搜索结果后给出提示信息，回到首页展示全部商品
                    if (prodName != undefined && prodName != "") {
                        $("#divProdItems").append('<p style="text-align:center;"><a href="default.aspx" style="color:red;">不是您想找的？点我看看其他的。</a></p>');
                    }

                    //标示已结束处理分页
                    isPaging = false;

                },
                error: function (xhr, err_msg, e) {
                    console.log(e + ":" + err_msg);

                    //标示已结束处理分页
                    isPaging = false;

                    // 隐藏加载进度条
                    $("div.loading").fadeOut();

                }
            });
        }

        function findProductByCategoryID(cID) {
            //赋值全局变量
            pageIndex = 1;
            categoryID = cID;
            prodName = "";

            //清空现有商品列表
            $("#divProdItems").empty();

            //加载指定类别的商品列表
            loadProdList();

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
        }
    </script>

</body>
</html>
