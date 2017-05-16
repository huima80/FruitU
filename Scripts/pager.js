//分页模块，依赖jQuery, jsrender, Bootstrap, Font-Awesome
; (function (root, factory) {

    // CommonJS
    if (typeof exports === 'object') {
        module.exports = factory(require('jquery.js'));
    }
        // AMD module
    else if (typeof define === 'function' && define.amd) {
        define(['jquery', 'jsrender'], factory);
    }
        // jquery plugin
    else {
        factory(jQuery);
    }
}
(this, function ($) {

    //分页类
    function Pager() {

        ///////////////////构造函数私有变量/////////////////////

        //总记录数
        var _totalRows = 0;
        //总页数
        var _totalPages = 1;
        //是否正在分页处理
        var _isPaging = false;
        //分页内容模板
        var _pageTemplate;
        //分页页码模板
        var _pagerTemplate;
        //分页加载提示
        var $_loadingHints, $_loadedHints;

        //////////////////实例成员变量///////////////////////////

        //分页参数
        this.settings = {
            //分页模式：1-瀑布流分页；2-传统分页
            pagerMode: 1,
            //当前页码
            pageIndex: 1,
            //每页记录数，根据屏幕自适应：手机-10；平板-30；小PC-50；大PC-70
            pageSize: 10,
            //页面数据查询URL
            pageQueryURL: '',
            //页面数据查询条件，object类型
            pageQueryCriteria: {},
            //页面内容jsrender模板
            pageTemplate: '',
            //分页页码jsrender模板
            pagerTemplate: '',
            //页面内容HTML容器，默认body
            pageContainer: 'body',
            //页面内容淡入时间，默认1000毫秒
            pageItemFadeTimer: 1000,
            //是否应用masonry，默认false
            isMasonry: false,
            //加载时loading提示，默认使用Font-Awesome图标，CSS3动画，不支持IE8-IE9
            loadingHints: '<div style="text-align:center;display:none;"><i class="fa fa-spinner fa-pulse"</i></div>',
            //加载完成后提示
            loadedHints: '<div style="text-align:center;font-size:12px;color:orange;display:none;"></div>',
            //分页页码参数
            pagerSettings: {
                //分页页码HTML容器
                pagerContainer: '',
                //第一页文本
                firstPageText: '第一页',
                //第一页图片
                firstPageImageUrl: '',
                //上一页文本
                previousPageText: '上一页',
                //上一页图片
                previousPageImageUrl: '',
                //下一页文本
                nextPageText: '下一页',
                //下一页图片
                nextPageImageUrl: '',
                //最后一页文本
                lastPageText: '最后一页',
                //最后一页图片
                lastPageImageUrl: '',
                //分页页码CSS样式类，默认使用bootstrap样式pagination
                pagerCssClass: 'pagination',
                //分页页码按钮数量
                pagerButtonCount: 10,
                //分页条显示位置：1-下方（默认）；2-上方；3-上下方
                pagerPosition: 1
            }
        };

        ////////////////原型成员对象/////////////////////

        //设置分页基本参数
        Pager.prototype.init = function (pageSettings) {
            var _this = this;

            if (typeof pageSettings === "object") {
                $.extend(true, _this.settings, pageSettings);
            }

            if (_this.settings.pageContainer) {
                //在分页内容后面附加分页Loading
                $_loadingHints = $(_this.settings.loadingHints).insertAfter(_this.settings.pageContainer);
                $_loadedHints = $(_this.settings.loadedHints).insertAfter(_this.settings.pageContainer);
            }
            else {
                throw new Error("未指定页面内容容器");
            }

            _this.setSuitablePageSize();

            if (_this.settings.pageTemplate) {
                //生成页面内容模板对象
                _pageTemplate = $.templates(_this.settings.pageTemplate);
            }
            else {
                throw new Error("未指定页面内容模板");
            }

            switch (_this.settings.pagerMode) {
                //瀑布流分页模式
                case 1:
                    //绑定浏览器事件：1，监听滚动事件进行分页。2，监听窗口大小变动事件调整PageSize
                    $(window).on({
                        "scroll": scrollUpEventHandler.bind(_this),
                        "resize": _this.setSuitablePageSize.bind(_this)
                    });

                    break;

                    //传统分页模式
                case 2:
                    //如果设置了分页页码模板，则生成分页页码模板对象
                    if (_this.settings.pagerTemplate) {
                        _pagerTemplate = $.templates(_this.settings.pagerTemplate);
                    }

                    break;
                default:
                    throw new Error("未知分页模式。参数：pagerMode:1（瀑布流分页）; pagerMode:2（传统分页）");
                    break;
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

        //页面向上滚动事件监听函数
        function scrollUpEventHandler() {
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
                    $_loadedHints.fadeIn("fast").fadeOut(2000);
                }
            }
        }

        //加载下一页数据
        Pager.prototype.loadPage = function (pageSettings) {

            var _this = this;

            if (typeof pageSettings === "object") {
                $.extend(true, _this.settings, pageSettings);
            }

            // 显示加载进度条
            $_loadingHints.fadeIn("fast");

            //触发分页前事件
            var pageLoadingEventArgs = { cancel: false };
            $(_this).trigger("onPageLoading", pageLoadingEventArgs);
            if (pageLoadingEventArgs.cancel) {
                return false;
            }

            //如果是传统分页模式，则从触发事件的HTML对象上获取页码
            if (_this.settings.pagerMode == 2) {
                var $li = $(event.target), piFromli;
                if ($li && $li.attr("pi") != null && !isNaN($li.attr("pi"))) {
                    piFromli = $li.attr("pi");
                    if (piFromli >= 1 && piFromli <= _totalPages) {
                        _this.settings.pageIndex = piFromli;
                    }
                    else {
                        _this.settings.pageIndex = 1;
                    }
                }
                else {
                    _this.settings.pageIndex = 1;
                }
            }

            // ajax异步查询数据
            $.ajax({
                url: _this.settings.pageQueryURL,
                data: $.extend({}, _this.settings.pageQueryCriteria, { PageIndex: _this.settings.pageIndex, PageSize: _this.settings.pageSize }),
                type: "GET",
                dataType: "json",
                cache: false,
                success: function (jDataPerPage) {

                    if (!!jDataPerPage && Array.isArray(jDataPerPage)) {
                        //当前页数据
                        var htmlItem = "";

                        //遍历获取到的当前页JSON数据，并用jsrender模板渲染生成HTML字符串
                        $.each(jDataPerPage, function (i, n) {

                            if (this["ID"] != undefined) {
                                htmlItem += _pageTemplate.render(this);
                            }
                            else {
                                //如果是第一页（totalRows == 0）；或用户在翻页过程中，如商品数有变动，则更新当前的记录总数，并重新计算页数
                                if (this["TotalRows"] != undefined && this["TotalRows"] != _totalRows) {
                                    _totalRows = this["TotalRows"];

                                    //计算总页数
                                    if (_totalRows % _this.settings.pageSize == 0) {
                                        _totalPages = parseInt(_totalRows / _this.settings.pageSize);
                                    }
                                    else {
                                        _totalPages = parseInt(_totalRows / _this.settings.pageSize) + 1;
                                    }
                                }
                            }
                        });

                        //根据不同分页模式，在页面容器中显示当前页内容的HTML字符串
                        switch (_this.settings.pagerMode) {
                            //瀑布流分页模式
                            case 1:
                                //如果是第一页则先清空页容器，后面页追加数据
                                if (_this.settings.pageIndex == 1) {
                                    $(_this.settings.pageContainer).empty();
                                }

                                if (htmlItem != "") {
                                    //追加并淡出新增的div
                                    $(htmlItem).appendTo(_this.settings.pageContainer).css({ display: "none" }).fadeIn(_this.settings.pageItemFadeTimer);
                                }

                                //应用masonry
                                if (_this.settings.isMasonry) {
                                    requirejs(['masonry'],
                                        function (Masonry) {
                                            new Masonry(_this.settings.pageContainer, {});
                                        });
                                }

                                break;

                                //传统分页模式
                            case 2:
                                var strPager;

                                //每一页都要先清空页容器，再显示本页数据
                                $(_this.settings.pageContainer).empty();
                                if (htmlItem != "") {
                                    //追加并淡出新增的div
                                    $(htmlItem).appendTo(_this.settings.pageContainer).css({ display: "none" }).fadeIn(_this.settings.pageItemFadeTimer);
                                }

                                //如果指定了分页页码模板，则渲染模板
                                if (_pagerTemplate) {
                                    strPager = _pagerTemplate.render(_this.settings.pagerSettings);
                                }
                                else {
                                    //如果未指定分页页码，则生成默认分页页码HTML
                                    strPager = '<nav><ul class="' + _this.settings.pagerSettings.pagerCssClass + '">';
                                    if (1 == _this.settings.pageIndex) {
                                        strPager += '<li class="disabled">';
                                    }
                                    else {
                                        strPager += '<li>';
                                    }
                                    strPager += '<a href="#" aria-label="Previous"><span aria-hidden="true">' + _this.settings.pagerSettings.firstPageText + '</span></a></li>';
                                    for (var pi = 1; pi <= _totalPages; pi++) {
                                        if (pi == _this.settings.pageIndex) {
                                            strPager += '<li class="active"><a href="#">' + pi + '</a></li>';
                                        }
                                        else {
                                            strPager += '<li pi="' + pi + '"><a href="#">' + pi + '</a></li>';
                                        }
                                    }
                                    if (_totalPages == _this.settings.pageIndex) {
                                        strPager += '<li class="disabled">';
                                    }
                                    else {
                                        strPager += '<li>';
                                    }
                                    strPager += '<a href="#" aria-label="Next"><span aria-hidden="true">' + _this.settings.pagerSettings.lastPageText + '</span></a></li>';
                                    strPager += '</ul></nav>';
                                }

                                //如果设置了分页页码容器，则把分页页码加入容器。否则根据参数pagerPosition，确定加在page容器的位置。
                                if (_this.settings.pagerSettings.pagerContainer != '') {
                                    $(_this.settings.pagerSettings.pagerContainer).append(strPager);
                                }
                                else {
                                    switch (_this.settings.pagerSettings.pagerPosition) {
                                        case 1:
                                            $(_this.settings.pageContainer).after(strPager);
                                            break;
                                        case 2:
                                            $(_this.settings.pageContainer).before(strPager);
                                            break;
                                        case 3:
                                            $(_this.settings.pageContainer).before(strPager);
                                            $(_this.settings.pageContainer).after(strPager);
                                            break;
                                        default:
                                            $(_this.settings.pageContainer).after(strPager);
                                            break;
                                    }
                                }

                                //挂接分页按钮单击事件函数
                                $(strPager).on("click", "li", _this.loadPage.bind(_this));

                                break;
                            default:
                                throw new Error("未知分页模式。参数：pagerMode:1（瀑布流分页）; pagerMode:2（传统分页）");
                                break;
                        }

                        //触发分页后事件，传入参数：当前页码、当前页的JSON数据
                        var pageLoadedEventArgs = { pageIndex: _this.settings.pageIndex, originalDataPerPage: jDataPerPage };
                        $(_this).trigger("onPageLoaded", pageLoadedEventArgs);

                        //加载成功后，页数+1
                        _this.settings.pageIndex++;
                    }

                    // 隐藏加载进度条
                    $_loadingHints.fadeOut();

                    //标示已结束处理分页
                    _isPaging = false;
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.warn(errorThrown + ":" + textStatus);

                    // 隐藏加载进度条
                    $_loadingHints.fadeOut();

                    //标示已结束处理分页
                    _isPaging = false;
                }
            });

        };

        //根据屏幕大小设置合适的分页记录数
        Pager.prototype.setSuitablePageSize = function () {
            var ps, winWidth;
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

            this.settings.pageSize = ps;
        };


    }

    if (!$.pager) {
        $.pager = new Pager();
    }

    return $.pager;

}));