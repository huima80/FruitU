//分页模块
; (function (root, factory) {

    // CommonJS
    if (typeof exports === 'object') {
        module.exports = factory(require('jquery.js'));
    }
        // AMD module
    else if (typeof define === 'function' && define.amd) {
        define(['jquery', 'jsrender', 'masonry'], factory(jQuery));
    }
        // Browser global
    else {
        root.pager = factory(jQuery);
    }
}
(window, function ($) {
    function Pager() {

        ///////////////////构造函数私有变量/////////////////////

        var _totalRows = 0;     //总记录数
        var _totalPages = 1;    //总页数
        var _isPaging = false;   //是否正在分页处理
        var _pageTemplate;
        var _pageLoading = '<div id="divLoading" class="loading"></div>';
        var _loadingImg = '<img class="loading-img" width="16px" height="16px" src="images/loading.gif" />';
        var _loadingText = '<div class="loading-text">木有了噢，最后一页了！</div>';
        var $_pageLoading, $_loadingImg, $_loadingText;

        //////////////////实例成员变量///////////////////////////
        this.settings = {
            pageIndex: 1,
            pageSize: this.setSuitablePageSize(),
            pageQueryURL: '',
            pageQueryCriteria: {},
            pageTemplate: '',
            pageContainer: '',
            pageItemFadeTimer: 1000,
            itemLayout: 0,
            masonryItemSelector: ''
        };

        ////////////////原型成员对象/////////////////////

        //设置分页基本参数
        Pager.prototype.init = function (pageSettings) {
            if (typeof pageSettings === "object") {

                $.extend(this.settings, pageSettings);

                if (!this.settings.pageContainer) {
                    this.settings.pageContainer = "body";
                }

                //生成分页Loading容器
                $_pageLoading = $(_pageLoading).insertAfter(this.settings.pageContainer);
                $_loadingImg = $(_loadingImg).appendTo($_pageLoading);
                $_loadingText = $(_loadingText).appendTo($_pageLoading);

                //生成模板对象
                _pageTemplate = $.templates(this.settings.pageTemplate);
            }
        };

        //总记录数
        Pager.prototype.totalRows = function () {
            return _totalRows;
        };

        //总页数
        Pager.prototype.totalPages = function () {
            return _totalPages;
        };

        //浏览器向上滑动分页
        Pager.prototype.scrollUpPager = function () {
            if (_isPaging) {
                return;
            }

            // 判断窗口的滚动条是否接近页面底部
            var winHeight = $(window).height(); //窗口可视高度
            var docHeight = $(document).height(); //页面总长度
            var scrollTop = $(document).scrollTop(); //页面上滚长度
            if (scrollTop + winHeight >= docHeight) {

                // 判断是否最后一页
                if (this.settings.pageIndex <= _totalPages) {

                    //标示正在处理分页，避免并发
                    _isPaging = true;

                    this.loadPage();

                } else {
                    $_loadingText.fadeIn("fast").fadeOut(2000);
                }
            }
        };

        //加载下一页数据
        Pager.prototype.loadPage = function (pageSettings) {

            var thisObj = this;

            if (typeof pageSettings === "object") {
                $.extend(this.settings, pageSettings);
            }

            //触发分页前事件
            var pageLoadingEventArgs = { cancel: false };
            $(thisObj).trigger("onPageLoading", pageLoadingEventArgs);
            if (pageLoadingEventArgs.cancel) {
                return false;
            }

            // 显示加载进度条
            $_loadingImg.fadeIn("fast");

            var settings = thisObj.settings;

            // ajax查询下一页数据并附加在底部
            $.ajax({
                url: settings.pageQueryURL,
                data: $.extend({}, settings.pageQueryCriteria, { PageIndex: settings.pageIndex, PageSize: settings.pageSize, R: Math.random() }),
                type: "GET",
                dataType: "json",
                success: function (jDataPerPage) {

                    //当前页数据
                    var htmlItem = "";
                    
                    //遍历展示当前页所有项数据，并使用模板渲染后加入HTML容器
                    $.each(jDataPerPage, function (i, n) {

                        if (this["ID"] != undefined) {
                            htmlItem += _pageTemplate.render(this);
                        }
                        else {
                            //如果是第一页（totalRows == 0）；或用户在翻页过程中，如商品数有变动，则更新当前的记录总数，并重新计算页数
                            if (_totalRows != this["TotalRows"]) {
                                _totalRows = this["TotalRows"];

                                //计算总页数
                                if (_totalRows % settings.pageSize == 0) {
                                    _totalPages = parseInt(_totalRows / settings.pageSize);
                                }
                                else {
                                    _totalPages = parseInt(_totalRows / settings.pageSize) + 1;
                                }
                            }
                        }
                    });

                    //如果是第一页则先清空HTML容器
                    if (settings.pageIndex == 1) {
                        $(settings.pageContainer).empty();
                    }

                    //追加并淡出新增的div
                    $(htmlItem).appendTo(settings.pageContainer).css({ display: "none" }).fadeIn(settings.pageItemFadeTimer);

                    //瀑布流布局
                    if (settings.itemLayout == 1) {
                        $(settings.pageContainer).masonry({
                            // options
                            itemSelector: settings.masonryItemSelector,
                        });
                    }

                    //触发分页后事件
                    var pageLoadedEventArgs = { pageIndex: settings.pageIndex, htmlResult: htmlItem };
                    $(thisObj).trigger("onPageLoaded", pageLoadedEventArgs);

                    //加载成功后，页数+1
                    settings.pageIndex++;

                    // 隐藏加载进度条
                    $_loadingImg.fadeOut();

                    //标示已结束处理分页
                    _isPaging = false;
                },
                error: function (xhr, err_msg, e) {
                    console.log(e + ":" + err_msg);

                    // 隐藏加载进度条
                    $_loadingImg.fadeOut();

                    //标示已结束处理分页
                    _isPaging = false;
                }
            });
        };
    }

    //根据屏幕大小设置合适的分页记录数
    Pager.prototype.setSuitablePageSize = function () {
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

    if (!$.pager) {
        $.pager = new Pager();

        //绑定浏览器滑动事件
        $(window).on("scroll", $.pager.scrollUpPager.bind($.pager));

        //根据窗口大小变动，调整分页记录数
        $(window).on("resize", function () {
            $.pager.settings.pageSize = $.pager.setSuitablePageSize();
        });
    }

    return $.pager;

}));