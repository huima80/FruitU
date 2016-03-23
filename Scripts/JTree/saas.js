; (function (root, factory) {

    // CommonJS
    if (typeof exports === 'object') {
        module.exports = factory(require('jquery.js'));
    }
        // AMD module
    else if (typeof define === 'function' && define.amd) {
        define(['jquery'], factory(jQuery));
    }
        // Browser global
    else {
        //root.saas = factory(jQuery);
    }
}
(window, function ($) {

    $.fn.extend({
        showTree: function (opt) {
            opt = opt || {};
            var showDataObj = $(this).prev();
            var fn, bindings, title, callbackAny, inputFlag;
            var _self = this;
            var dataList = opt.data || {};
            var rootID = opt.rootID ? opt.rootID : 0;
            opt.callback ? fn = opt.callback : fn = function (t, o) {
                showDataObj.val($(t).html());
                $.rmwin();
            };
            opt.bindings ? bindings = opt.bindings : null;
            opt.callbackAny ? callbackAny = opt.callbackAny : callbackAny = false;
            opt.inputFlag ? inputFlag = opt.inputFlag : inputFlag = false;
            opt.title ? title = opt.title : title = '选择分类';
            var width = opt.width || 210;
            var height = opt.height || 460;
            var funcBefore = opt.before || function () { };
            if (opt.alert == 'alert') {
                $(this).click(function () {
                    var html = '<div style="height:450px;width:200px;overflow:auto"><ul class="treeview filetree" id="showTreeSelectParent">';

                    $.each(dataList, function (i, n) {
                        if (n.ID == rootID) {
                            html += '<li class="expandable"><div class="hitarea"></div><span class="folder" parentid="' + n.ParentID + '" val="' + n.ID + '" loaded="false">' + n.CategoryName + '</span></li>';
                        }
                    });
                    html += '</ul></div>';
                    html = $(html);
                    var win = $.win({ title: title, html: html, width: width, height: height });
                    initTree($('#showTreeSelectParent'));
                    funcBefore(html);
                });
            }
            else {
                $.each(dataList, function (i, n) {
                    if (n.ID == rootID) { //根节点的ID==0, ParentID==-1
                        $(_self).append('<li class="expandable"><div class="hitarea"></div><span class="folder" parentid="' + n.ParentID + '" val="' + n.ID + '" loaded="false">' + n.CategoryName + '</span></li>');
                    }
                });
                initTree($(_self));
            }

            //初始化时打开一级商品类别
            $(_self).find('span:first').click();

            if (!inputFlag) {
                showDataObj.focus(function () {
                    $(this).next().click();
                });
            }

            function initTree(t) {
                t.find('span[loaded=false]').each(function () {
                    if (bindings) {
                        $(this).unbind('contextmenu').contextMenu(this, 'myTreeMenuGroup', {
                            bindings: bindings,
                            clickFlag: true
                        });
                    }

                    var id = $(this).attr('val');
                    var flag = false;
                    //判断是不是有子元素
                    $.each(dataList, function (i, n) {
                        if (n.ParentID == id) {
                            flag = true;
                            return false;
                        }
                    });
                    //没有子元素
                    if (!flag) {
                        $(this).unbind('click').bind('click', function () {
                            fn(this, _self);
                        }).attr('loaded', 'true').removeClass('folder').addClass('file');
                        if ($(this).parent().next().html()) {
                            $(this).parent().removeClass();
                        }
                        else {
                            $(this).parent().removeClass().addClass('last');
                        }
                        return true;
                    }

                    //在每个span的单击事件上绑定一次增加树枝节点数据函数
                    $(this).one('click', this, addChindList);

                    //在每个span的单击事件上绑定切换树枝节点显示函数
                    $(this).on('click', switchSubTree);

                    $(this).prev().bind('click', function () {
                        $(this).next().click();
                    });
                });
                var o = t.find('>ul >li:last');
                (o.html() == null) ? o = t.find('>li:last') : null;
                if (o.hasClass('expandable')) {
                    o.addClass('lastExpandable').removeClass('expandable');
                }
                else {
                    o.addClass('lastCollapsable').removeClass('collapsable');
                }
                t.find('.last').removeClass().addClass('last');
            }

            //切换子类别树显示
            function switchSubTree() {
                var o = $(this).parent();
                var $ul = o.find('>ul');
                if ($ul.is(":visible")) {
                    $ul.hide();
                    if (o.hasClass('lastCollapsable')) {
                        o.addClass('lastExpandable').removeClass('lastCollapsable');
                    }
                    else {
                        o.addClass('expandable').removeClass('collapsable');
                    }
                }
                else {
                    $ul.show();
                    if (o.hasClass('lastExpandable')) {
                        o.addClass('lastCollapsable').removeClass('lastExpandable');
                    }
                    else {
                        o.addClass('collapsable').removeClass('expandable');
                    }
                    o.siblings().each(function () {
                        if ($(this).hasClass('collapsable') || $(this).hasClass('lastCollapsable')) {
                            $(this).find('>span:first').click();
                        }
                    });
                }
            }

            //添加树枝节点
            function addChindList(evt) {
                if (callbackAny) {
                    fn(evt.data, _self);
                }
                var id = $(evt.data).attr('val');
                var str = '';
                $.each(dataList, function (i, n) {
                    if (n.ParentID == id) {
                        str += '<li class="expandable"><div class="hitarea"></div><span class="folder" parentid="' + n.ParentID + '" val="' + n.ID + '" loaded="false">' + n.CategoryName + '</span></li>';
                    }
                });
                if (str) {
                    $(evt.data).parent().append('<ul style="display:none;">' + str + '</ul>');
                }
                $(evt.data).attr('loaded', 'true');
                initTree($(evt.data).parent());
            }
        }
    });
}));