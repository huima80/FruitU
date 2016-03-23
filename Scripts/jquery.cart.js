//购物车模块
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
        root.cart = factory(jQuery);
    }
}
(window, function ($) {
    function Cart(options) {

        //购物车cookies
        var _storage;

        //购物车参数
        this.params = {
            cartName: "MyCart",         //String:  指定购物车在storage中的key
            isPersist: true       //Boolean: 指定购物车的生命周期是会话级还是永久
        };

        $.extend(this.params, options);

        if (this.params.isPersist) {
            _storage = window.localStorage;
        }
        else {
            _storage = window.sessionStorage;
        }

        if (_storage.getItem(this.params.cartName) == undefined || _storage.getItem(this.params.cartName) == null) {
            var cartInfo = {
                name: "",
                phone: "",
                address: "",
                memo: "",
                paymentTerm: "",
                prodItems: []
            };

            _storage.setItem(this.params.cartName, JSON.stringify(cartInfo));
        }

        //增加商品项
        Cart.prototype.addProdItem = function (prodID, prodName, prodDesc, prodImg, price, qty) {
            var cartInfo, exist = false;
            if (isNaN(prodID)) {
                throw new TypeError("商品ID不正确");
            }
            if (isNaN(price)) {
                throw new TypeError("商品价格不正确");
            }
            if (isNaN(qty)) {
                throw new TypeError("商品数量不正确");
            }

            //触发商品信息变动前事件
            $(this).trigger("onProdItemsChanging");

            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            for (var i = 0; i < cartInfo.prodItems.length; i++) {
                if (cartInfo.prodItems[i].prodID == prodID) {
                    //如果购物车里已有此商品，则数量累加
                    cartInfo.prodItems[i].qty = parseInt(cartInfo.prodItems[i].qty) + parseInt(qty);
                    exist = true;
                    break;
                }
            }

            //如果购物车里没有此商品，则加入
            if (!exist) {
                var prodItem = {
                    prodID: prodID,
                    prodName: prodName,
                    prodDesc: prodDesc,
                    prodImg: prodImg,
                    price: price,
                    qty: qty
                }
                cartInfo.prodItems.push(prodItem);
            }

            _storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

            //触发商品信息变动后事件
            $(this).trigger("onProdItemsChanged", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

        };

        //更新商品项
        Cart.prototype.updateProdItem = function (prodID, qty) {
            var cartInfo;
            if (isNaN(prodID)) {
                throw new TypeError("商品ID不正确");
            }
            if (isNaN(qty)) {
                throw new TypeError("商品数量不正确");
            }

            //触发商品信息变动前事件
            $(this).trigger("onProdItemsChanging");

            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            for (var i = 0; i < cartInfo.prodItems.length; i++) {
                if (cartInfo.prodItems[i].prodID == prodID) {
                    cartInfo.prodItems[i].qty = parseInt(qty);
                    break;
                }
            }
            _storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

            //触发商品信息变动后事件
            $(this).trigger("onProdItemsChanged", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

        };

        //更新收件人信息
        Cart.prototype.updateDeliverInfo = function (name, phone, address, memo, paymentTerm) {
            var cartInfo;
            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            cartInfo.name = name;
            cartInfo.phone = phone;
            cartInfo.address = address;
            cartInfo.memo = memo;
            cartInfo.paymentTerm = paymentTerm;
            _storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

        };

        //删除商品项
        Cart.prototype.removeProdItem = function (prodID) {
            var cartInfo;
            if (isNaN(prodID)) {
                throw new TypeError("商品ID不正确");
            }

            //触发商品信息变动前事件
            $(this).trigger("onProdItemsChanging");

            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            for (var i = 0; i < cartInfo.prodItems.length; i++) {
                if (cartInfo.prodItems[i].prodID == prodID) {
                    cartInfo.prodItems.splice(i, 1);
                }
            }
            _storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

            //触发商品信息变动后事件
            $(this).trigger("onProdItemsChanged", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

        };

        //清空购物车商品项 
        Cart.prototype.clearProdItems = function () {
            var cartInfo;

            //触发商品信息变动前事件
            $(this).trigger("onProdItemsChanging");

            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            cartInfo.prodItems.length = 0;
            _storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

            //触发商品信息变动后事件
            $(this).trigger("onProdItemsChanged", { prodQty: 0, subTotal: 0 });

        };

        //购物车商品总价
        Cart.prototype.subTotal = function () {
            var cartInfo, subTotal = 0;
            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            for (var i = 0; i < cartInfo.prodItems.length; i++) {
                subTotal += parseFloat(cartInfo.prodItems[i].price) * parseInt(cartInfo.prodItems[i].qty);
            }
            return parseFloat(subTotal.toFixed(2));
        };

        //购物车商品总数
        Cart.prototype.prodAmount = function () {
            var cartInfo, prodAmount = 0;
            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            for (var i = 0; i < cartInfo.prodItems.length; i++) {
                prodAmount += parseInt(cartInfo.prodItems[i].qty);
            }
            return prodAmount;
        };

        //获取购物车商品项
        Cart.prototype.getProdItems = function () {
            var cartInfo;
            cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            return $(cartInfo.prodItems);
        };

        //生成订单信息
        Cart.prototype.makeOrderInfo = function () {
            var orderInfo;
            orderInfo = JSON.parse(_storage.getItem(this.params.cartName));
            for (var i = 0; i < orderInfo.prodItems.length; i++) {
                delete orderInfo.prodItems[i].prodName;
                delete orderInfo.prodItems[i].prodDesc;
                delete orderInfo.prodItems[i].prodImg;
                delete orderInfo.prodItems[i].price;
            }
            return JSON.stringify(orderInfo);
        };
    }

    if (!$.cart) {
        $.cart = new Cart();
    }

    return $.cart;

}));