//网站配置模块
; (function (root, factory) {

    // CommonJS
    if (typeof exports === 'object') {
        module.exports = factory(root);
    }
        // AMD module
    else if (typeof define === 'function' && define.amd) {
        define(factory());
    }
        // Browser global
    else {
        root.webConfig = factory();
    }
}(window, function () {
    function WebConfig() {
        this.siteDomain = 'mahui.me';
        this.siteTitle = 'FruitU';
        this.siteDesc = '鲜果切';
        this.siteKeywords = 'fruit,slice,juice,水果,果汁,切片';
        this.siteIcon = 'images/FruitU.ico';
        this.siteCopyrights = 'FruitU';
        this.defaultImg = 'FruitU.jpg';
    }

    //初始化网站配置
    WebConfig.prototype.init = function () {
        document.title = this.siteTitle + ' -- ' + document.title;
    };

    //客户端浏览器信息
    WebConfig.prototype.browserVersion = (function () {
        var u = navigator.userAgent, app = navigator.appVersion;
        return {
            wechat: u.indexOf("MicroMessenger") > -1, //微信内置浏览器
            trident: u.indexOf("Trident") > -1, //IE内核
            presto: u.indexOf("Presto") > -1, //opera内核
            webKit: u.indexOf("AppleWebKit") > -1, //苹果、谷歌内核
            gecko: u.indexOf("Gecko") > -1 && u.indexOf("KHTML") == -1, //火狐内核
            mobile: !!u.match(/AppleWebKit.*Mobile.*/) || !!u.match(/AppleWebKit/), //是否为移动终端
            ios: !!u.match(/(i[^;])+;(U;)? CPU.+Mac OS X/), //ios终端
            android: u.indexOf("Android") > -1 || u.indexOf("Linux") > -1, //android终端或者uc浏览器
            iPhone: u.indexOf("iPhone") > -1 || u.indexOf("Mac") > -1, //是否为iPhone或者QQHD浏览器
            iPad: u.indexOf("iPad") > -1, //是否iPad
            webApp: u.indexOf("Safari") == -1 //是否web应该程序，没有头部与底部
        };
    })();

    if (!window.webConfig) {
        window.webConfig = new WebConfig();
        window.onload = webConfig.init.bind(webConfig);
    }

    return window.webConfig;
}));