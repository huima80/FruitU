﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="WX-JSSDK.aspx.cs" Inherits="WX_JSSDK" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>微信JS-SDK</title>
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=Edge" />
    <meta name="apple-mobile-web-app-capable" content="yes" />
    <meta name="apple-mobile-web-app-status-bar-style" content="black" />
    <meta name="format-detection" content="telephone=no" />
    <meta name="viewport" content="user-scalable=no, width=device-width, initial-scale=1.0, maximum-scale=1.0" />
    <link href="http://203.195.235.76/jssdk/css/style.css" rel="stylesheet" />
</head>
<body>
    <form runat="server" id="form1">
        <div class="wxapi_container">
            <div class="wxapi_index_container">
                <ul class="label_box lbox_close wxapi_index_list">
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-basic">基础接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-share">分享接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-image">图像接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-voice">音频接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-smart">智能接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-device">设备信息接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-location">地理位置接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-webview">界面操作接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-scan">微信扫一扫接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-shopping">微信小店接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-card">微信卡券接口</a></li>
                    <li class="label_item wxapi_index_item"><a class="label_inner" href="#menu-pay">微信支付接口</a></li>
                </ul>
            </div>
            <div class="lbox_close wxapi_form">
                <h3 id="menu-basic">基础接口</h3>
                <span class="desc">判断当前客户端是否支持指定JS接口</span>
                <button type="button" class="btn btn_primary" id="checkJsApi">checkJsApi</button>

                <h3 id="menu-share">分享接口</h3>
                <span class="desc">获取“分享到朋友圈”按钮点击状态及自定义分享内容接口</span>
                <button type="button" class="btn btn_primary" id="onMenuShareTimeline">onMenuShareTimeline</button>
                <span class="desc">获取“分享给朋友”按钮点击状态及自定义分享内容接口</span>
                <button type="button" class="btn btn_primary" id="onMenuShareAppMessage">onMenuShareAppMessage</button>
                <span class="desc">获取“分享到QQ”按钮点击状态及自定义分享内容接口</span>
                <button type="button" class="btn btn_primary" id="onMenuShareQQ">onMenuShareQQ</button>
                <span class="desc">获取“分享到腾讯微博”按钮点击状态及自定义分享内容接口</span>
                <button type="button" class="btn btn_primary" id="onMenuShareWeibo">onMenuShareWeibo</button>
                <span class="desc">获取“分享到QZone”按钮点击状态及自定义分享内容接口</span>
                <button type="button" class="btn btn_primary" id="onMenuShareQZone">onMenuShareQZone</button>

                <h3 id="menu-image">图像接口</h3>
                <span class="desc">拍照或从手机相册中选图接口</span>
                <button type="button" class="btn btn_primary" id="chooseImage">chooseImage</button>
                <span class="desc">预览图片接口</span>
                <button type="button" class="btn btn_primary" id="previewImage">previewImage</button>
                <span class="desc">上传图片接口</span>
                <button type="button" class="btn btn_primary" id="uploadImage">uploadImage</button>
                <span class="desc">下载图片接口</span>
                <button type="button" class="btn btn_primary" id="downloadImage">downloadImage</button>

                <h3 id="menu-voice">音频接口</h3>
                <span class="desc">开始录音接口</span>
                <button type="button" class="btn btn_primary" id="startRecord">startRecord</button>
                <span class="desc">停止录音接口</span>
                <button type="button" class="btn btn_primary" id="stopRecord">stopRecord</button>
                <span class="desc">播放语音接口</span>
                <button type="button" class="btn btn_primary" id="playVoice">playVoice</button>
                <span class="desc">暂停播放接口</span>
                <button type="button" class="btn btn_primary" id="pauseVoice">pauseVoice</button>
                <span class="desc">停止播放接口</span>
                <button type="button" class="btn btn_primary" id="stopVoice">stopVoice</button>
                <span class="desc">上传语音接口</span>
                <button type="button" class="btn btn_primary" id="uploadVoice">uploadVoice</button>
                <span class="desc">下载语音接口</span>
                <button type="button" class="btn btn_primary" id="downloadVoice">downloadVoice</button>

                <h3 id="menu-smart">智能接口</h3>
                <span class="desc">识别音频并返回识别结果接口</span>
                <button type="button" class="btn btn_primary" id="translateVoice">translateVoice</button>

                <h3 id="menu-device">设备信息接口</h3>
                <span class="desc">获取网络状态接口</span>
                <button type="button" class="btn btn_primary" id="getNetworkType">getNetworkType</button>

                <h3 id="menu-location">地理位置接口</h3>
                <span class="desc">使用微信内置地图查看位置接口</span>
                <button type="button" class="btn btn_primary" id="openLocation">openLocation</button>
                <span class="desc">获取地理位置接口</span>
                <button type="button" class="btn btn_primary" id="getLocation">getLocation</button>

                <h3 id="menu-webview">界面操作接口</h3>
                <span class="desc">隐藏右上角菜单接口</span>
                <button type="button" class="btn btn_primary" id="hideOptionMenu">hideOptionMenu</button>
                <span class="desc">显示右上角菜单接口</span>
                <button type="button" class="btn btn_primary" id="showOptionMenu">showOptionMenu</button>
                <span class="desc">关闭当前网页窗口接口</span>
                <button type="button" class="btn btn_primary" id="closeWindow">closeWindow</button>
                <span class="desc">批量隐藏功能按钮接口</span>
                <button type="button" class="btn btn_primary" id="hideMenuItems">hideMenuItems</button>
                <span class="desc">批量显示功能按钮接口</span>
                <button type="button" class="btn btn_primary" id="showMenuItems">showMenuItems</button>
                <span class="desc">隐藏所有非基础按钮接口</span>
                <button type="button" class="btn btn_primary" id="hideAllNonBaseMenuItem">hideAllNonBaseMenuItem</button>
                <span class="desc">显示所有功能按钮接口</span>
                <button type="button" class="btn btn_primary" id="showAllNonBaseMenuItem">showAllNonBaseMenuItem</button>

                <h3 id="menu-scan">微信扫一扫</h3>
                <span class="desc">调起微信扫一扫接口</span>
                <button type="button" class="btn btn_primary" id="scanQRCode0">scanQRCode(微信处理结果)</button>
                <button type="button" class="btn btn_primary" id="scanQRCode1">scanQRCode(直接返回结果)</button>

                <h3 id="menu-shopping">微信小店接口</h3>
                <span class="desc">跳转微信商品页接口</span>
                <button type="button" class="btn btn_primary" id="openProductSpecificView">openProductSpecificView</button>

                <h3 id="menu-card">微信卡券接口</h3>
                <span class="desc">批量添加卡券接口</span>
                <button type="button" class="btn btn_primary" id="addCard">addCard</button>
                <span class="desc">调起适用于门店的卡券列表并获取用户选择列表</span>
                <button type="button" class="btn btn_primary" id="chooseCard">chooseCard</button>
                <span class="desc">查看微信卡包中的卡券接口</span>
                <button type="button" class="btn btn_primary" id="openCard">openCard</button>

                <h3 id="menu-pay">微信支付接口</h3>
                <span class="desc">发起一个微信支付请求</span>
                <button type="button" class="btn btn_primary" id="chooseWXPay">chooseWXPay</button>
            </div>
        </div>
    </form>
    <script src="http://apps.bdimg.com/libs/jquery/1.10.2/jquery.min.js"></script>

    <script src="http://res.wx.qq.com/open/js/jweixin-1.0.0.js"></script>

    <%--摇一摇周边功能--%>
    <script src="http://res.wx.qq.com/open/js/jweixin-1.1.0.js"></script>

    <script>

        //所有需要使用JS-SDK的页面必须先注入配置信息，否则将无法调用（同一个url仅需调用一次，对于变化url的SPA的web app可在每次url变化时进行调用,目前Android微信客户端不支持pushState的H5新特性，所以使用pushState来实现web app的页面会导致签名失败，此问题会在Android6.2中修复）。
        wx.config({
            debug: true, // 开启调试模式,调用的所有api的返回值会在客户端alert出来，若要查看传入的参数，可以在pc端打开，参数信息会通过log打出，仅在pc端时才会打印。
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

    </script>

    <script src="http://203.195.235.76/jssdk/js/zepto.min.js"></script>
    <script src="http://203.195.235.76/jssdk/js/demo.js"> </script>
</body>
</html>