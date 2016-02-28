; (function ($) {
    $.Cart = function (options) {
        this.params = $.extend({}, $.Cart.defaults, options);

        this.init();

    };

    $.Cart.defaults = {
        cartName: "MyCart",         //String:  指定购物车在storage中的key
        isPersist: false       //Boolean: 指定购物车的生命周期是会话级还是永久
    };

    $.Cart.prototype.storage = {};

    $.Cart.prototype.init = function () {
        if (this.params.isPersist) {
            this.storage = window.localStorage;
        }
        else {
            this.storage = window.sessionStorage;
        }

        if (this.storage.getItem(this.params.cartName) == undefined) {
            var cartInfo = {
                name: "",
                phone: "",
                address: "",
                memo: "",
                paymentTerm: "",
                prodItems: []
            };

            this.storage.setItem(this.params.cartName, JSON.stringify(cartInfo));
        }
    };

    $.Cart.prototype.addProdItem = function (prodID, prodName, prodDesc, prodImg, price, qty) {
        var cartInfo, exist = false;

        //触发商品信息变动前事件
        $(this).trigger("prodItemsChanging");

        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
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

        this.storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

        //触发商品信息变动后事件
        $(this).trigger("prodItemsChanged", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

    };

    $.Cart.prototype.updateProdItem = function (prodID, qty) {
        var cartInfo;

        //触发商品信息变动前事件
        $(this).trigger("prodItemsChanging");

        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        for (var i = 0; i < cartInfo.prodItems.length; i++) {
            if (cartInfo.prodItems[i].prodID == prodID) {
                cartInfo.prodItems[i].qty = parseInt(qty);
                break;
            }
        }
        this.storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

        //触发商品信息变动后事件
        $(this).trigger("prodItemsChanged", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

    };

    $.Cart.prototype.updateDeliverInfo = function (name, phone, address, memo, paymentTerm) {
        var cartInfo;
        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        cartInfo.name = name;
        cartInfo.phone = phone;
        cartInfo.address = address;
        cartInfo.memo = memo;
        cartInfo.paymentTerm = paymentTerm;
        this.storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

    };

    $.Cart.prototype.removeProdItem = function (prodID) {
        var cartInfo;

        //触发商品信息变动前事件
        $(this).trigger("prodItemsChanging");

        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        for (var i = 0; i < cartInfo.prodItems.length; i++) {
            if (cartInfo.prodItems[i].prodID == prodID) {
                cartInfo.prodItems.splice(i, 1);
            }
        }
        this.storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

        //触发商品信息变动后事件
        $(this).trigger("prodItemsChanged", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

    };

    $.Cart.prototype.clearProdItems = function () {
        var cartInfo;

        //触发商品信息变动前事件
        $(this).trigger("prodItemsChanging");

        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        cartInfo.prodItems.length = 0;
        this.storage.setItem(this.params.cartName, JSON.stringify(cartInfo));

        //触发商品信息变动后事件
        $(this).trigger("prodItemsChanged", { prodQty: 0, subTotal: 0 });

    };

    $.Cart.prototype.subTotal = function () {
        var cartInfo, subTotal = 0;
        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        for (var i = 0; i < cartInfo.prodItems.length; i++) {
            subTotal += parseFloat(cartInfo.prodItems[i].price) * parseInt(cartInfo.prodItems[i].qty);
        }
        return parseFloat(subTotal.toFixed(2));
    };

    $.Cart.prototype.prodAmount = function () {
        var cartInfo, prodAmount = 0;
        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        for (var i = 0; i < cartInfo.prodItems.length; i++) {
            prodAmount += parseInt(cartInfo.prodItems[i].qty);
        }
        return prodAmount;
    };

    $.Cart.prototype.getProdItems = function () {
        var cartInfo;
        cartInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        return $(cartInfo.prodItems);
    };

    $.Cart.prototype.makeOrderInfo = function () {
        var orderInfo;
        orderInfo = JSON.parse(this.storage.getItem(this.params.cartName));
        for (var i = 0; i < orderInfo.prodItems.length; i++) {
            delete orderInfo.prodItems[i].prodName;
            delete orderInfo.prodItems[i].prodDesc;
            delete orderInfo.prodItems[i].prodImg;
            delete orderInfo.prodItems[i].price;
        }
        return JSON.stringify(orderInfo);
    };


})(jQuery);