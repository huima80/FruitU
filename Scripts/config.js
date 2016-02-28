(function ($) {
    $.webConfig = {
        siteName: 'FruitU',
        siteUrl: 'mahui.me',
        defaultImg: 'images/FruitU.jpg',
        init: function () {
            document.title = siteName + ' -- ' + document.title;
        },

        //根据屏幕大小设置合适的分页记录数
        setSuitablePageSize: function () {
            var ps = 0, winWidth;
            winWidth = $(document).width();

            if (winWidth < 768) {
                //手机屏幕
                ps = 10;
            }
            else {
                if (winWidth >= 768 && winWidth < 992) {
                    //平板屏幕
                    ps = 30;
                }
                else {
                    if (winWidth >= 992 && winWidth < 1200) {
                        //中等桌面显示器
                        ps = 50;
                    }
                    else {
                        if (winWidth >= 1200) {
                            //大型桌面显示器
                            ps = 70;
                        }
                    }
                }
            }

            return ps;
        }

    };

    $.browser = {
        version: function () {
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
        }()
    };

    $(function () {
        if (!$.browser.version.wechat) {

        }

    });



})(jQuery);

//document.writeln(" 是否为微信浏览器 " + browser.versions.wechat);
//document.writeln(" 是否为移动终端: " + browser.versions.mobile);
//document.writeln(" ios终端: "+browser.versions.ios);
//document.writeln(" android终端: "+browser.versions.android);
//document.writeln(" 是否为iPhone: "+browser.versions.iPhone);
//document.writeln(" 是否iPad: "+browser.versions.iPad);
//document.writeln(navigator.userAgent);
