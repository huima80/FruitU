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

    //购物车类
    function Cart(options) {

        //购物车cookies
        var _storage;

        //购物车参数
        this.params = {
            cartName: location.host + "_Cart",         //String:  指定购物车在storage中的key
            isPersist: true       //Boolean: 指定购物车的生命周期是会话级还是永久
        };

        //收货人姓名
        this.name = "";
        //收货人电话
        this.phone = "";
        //收货人地址
        this.address = "";
        //订单备注
        this.memo = "";
        //订单付款方式
        this.paymentTerm = "";
        //订单运费
        this.freight = 0;
        //订单商品信息
        this.prodItems = [];

        $.extend(this.params, options);

        if (this.params.isPersist) {
            _storage = window.localStorage;
        }
        else {
            _storage = window.sessionStorage;
        }

        //保存购物车信息到cookies
        Cart.prototype.save = function () {
            _storage.setItem(this.params.cartName, JSON.stringify(this));
        }

        //从cookies加载购物车信息
        Cart.prototype.load = function () {
            var cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            $.extend(true, this, cartInfo);
        }

        //购物车商品项类
        Cart.prototype.ProdItem = function (prodID, prodName, prodDesc, prodImg, price, qty, inventoryQty) {
            //商品ID
            this.prodID = prodID;
            //商品名称
            this.prodName = prodName;
            //商品描述
            this.prodDesc = prodDesc;
            //商品图片
            this.prodImg = prodImg;
            //商品价格
            this.price = price;
            //商品数量
            this.qty = qty;
            //商品库存
            this.inventoryQty = inventoryQty;
        };

        //增加商品项
        Cart.prototype.insertProdItem = function (prodItem) {
            var exist = false;

            if (!(prodItem instanceof this.ProdItem)) {
                throw new TypeError("参数应该是ProdItem类的实例");
            }
            else {
                //校验参数
                if (!prodItem.prodID || isNaN(prodItem.prodID)) {
                    throw new TypeError("商品ID不正确");
                }
                if (isNaN(prodItem.price) || prodItem.price < 0) {
                    throw new TypeError("商品价格不正确");
                }
                if (isNaN(prodItem.qty) || prodItem.qty < 0) {
                    throw new TypeError("商品数量不正确");
                }
                if (isNaN(prodItem.inventoryQty)) {
                    throw new TypeError("商品库存量不正确");
                }
                //库存量-1表示不限量
                if (prodItem.inventoryQty != -1 && prodItem.qty > prodItem.inventoryQty) {
                    throw new Error("购买数量不能大于库存量");
                }
            }

            //触发商品信息变动前事件onProdItemInserting
            var prodItemInsertingEventArgs = { cancel: false };
            $(this).trigger("onProdItemInserting", prodItemInsertingEventArgs);
            if (prodItemInsertingEventArgs.cancel) {
                return false;
            }

            //加载cookies值到当前对象
            this.load();

            for (var i = 0; i < this.prodItems.length; i++) {
                if (this.prodItems[i].prodID == prodItem.prodID) {
                    exist = true;

                    //如果购物车里已有此商品，且已有数量+增加数量>库存量，则触发超出库存事件onOutOfStock
                    if (prodItem.inventoryQty != -1 && (parseInt(this.prodItems[i].qty) + parseInt(prodItem.qty)) > parseInt(prodItem.inventoryQty)) {
                        var outOfStockEventArgs = { insertingItem: prodItem, existItem: this.prodItems[i] };
                        $(this).trigger("onOutOfStock", outOfStockEventArgs);
                        return false;
                    }
                    else {
                        //如果购物车里已有此商品，且没有超出库存量，则数量累加，并更新库存量
                        this.prodItems[i].qty = parseInt(this.prodItems[i].qty) + parseInt(prodItem.qty);
                        this.prodItems[i].inventoryQty = prodItem.inventoryQty;
                        break;
                    }
                }
            }

            //如果购物车里没有此商品，则新增
            if (!exist) {
                this.prodItems.push(prodItem);
            }

            this.save();

            //触发商品信息变动后事件
            $(this).trigger("onProdItemInserted", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

            return true;
        };

        //更新商品项
        Cart.prototype.updateProdItem = function (prodItem) {
            if (!(prodItem instanceof this.ProdItem)) {
                throw new TypeError("参数应该是ProdItem类的实例");
            }
            if (isNaN(prodItem.prodID)) {
                throw new TypeError("商品ID不正确");
            }
            if (isNaN(prodItem.qty) || prodItem.qty < 0) {
                throw new TypeError("商品数量不正确");
            }

            //触发商品信息变动前事件onProdItemUpdating
            var prodItemUpdatingEventArgs = { cancel: false };
            $(this).trigger("onProdItemUpdating", prodItemUpdatingEventArgs);
            if (prodItemUpdatingEventArgs.cancel) {
                return false;
            }

            this.load();

            for (var i = 0; i < this.prodItems.length; i++) {
                if (this.prodItems[i].prodID == prodItem.prodID) {
                    //如果购物车里已有此商品，且待更新数量>库存量，则触发超出库存事件onOutOfStock
                    if (parseInt(this.prodItems[i].inventoryQty) != -1 && parseInt(prodItem.qty) > parseInt(this.prodItems[i].inventoryQty)) {
                        var outOfStockEventArgs = { updatingItem: prodItem, existItem: this.prodItems[i] };
                        $(this).trigger("onOutOfStock", outOfStockEventArgs);
                        return false;
                    }
                    else {
                        this.prodItems[i].qty = parseInt(prodItem.qty);
                        break;
                    }
                }
            }

            this.save();

            //触发商品信息变动后事件
            $(this).trigger("onProdItemUpdated", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

            return true;
        };

        //删除商品项
        Cart.prototype.deleteProdItem = function (prodID) {
            var cartInfo;
            if (isNaN(prodID)) {
                throw new TypeError("商品ID不正确");
            }

            //触发商品信息变动前事件onProdItemDeleting
            var prodItemDeletingEventArgs = { cancel: false };
            $(this).trigger("onProdItemDeleting", prodItemDeletingEventArgs);
            if (prodItemDeletingEventArgs.cancel) {
                return false;
            }

            this.load();
            for (var i = 0; i < this.prodItems.length; i++) {
                if (this.prodItems[i].prodID == prodID) {
                    this.prodItems.splice(i, 1);
                }
            }
            this.save();

            //触发商品信息变动后事件
            $(this).trigger("onProdItemDeleted", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

            return true;

        };

        //清空购物车商品项 
        Cart.prototype.clearProdItems = function () {
            var cartInfo;

            //触发商品信息变动前事件onProdItemClearing
            var prodItemClearingEventArgs = { cancel: false };
            $(this).trigger("onProdItemClearing", prodItemClearingEventArgs);
            if (prodItemClearingEventArgs.cancel) {
                return false;
            }

            this.load();
            this.prodItems.length = 0;
            this.save();

            //触发商品信息变动后事件
            $(this).trigger("onProdItemCleared", { prodQty: 0, subTotal: 0 });

            return true;

        };

        //购物车商品总价，不含运费
        Cart.prototype.subTotal = function () {
            var subTotal = 0;
            this.load();
            for (var i = 0; i < this.prodItems.length; i++) {
                subTotal += parseFloat(this.prodItems[i].price) * parseInt(this.prodItems[i].qty);
            }
            return parseFloat(subTotal.toFixed(2));
        };

        //购物车订单总价，含运费
        Cart.prototype.orderPrice = function () {
            this.load();
            return this.subTotal() + this.freight;
        }

        //购物车商品总数
        Cart.prototype.prodAmount = function () {
            var prodAmount = 0;
            this.load();
            for (var i = 0; i < this.prodItems.length; i++) {
                prodAmount += parseInt(this.prodItems[i].qty);
            }
            return prodAmount;
        };

        //获取购物车商品项
        Cart.prototype.getProdItems = function () {
            this.load();
            return $(this.prodItems);
        };

        //获取购物车收件人信息
        Cart.prototype.getDeliverInfo = function () {
            this.load();
            return { name: this.name, phone: this.phone, address: this.address, memo: this.memo };
        };

        //更新收件人信息
        Cart.prototype.updateDeliverInfo = function (name, phone, address, memo, paymentTerm) {
            this.load();
            this.name = name;
            this.phone = phone;
            this.address = address;
            this.memo = memo;
            this.paymentTerm = paymentTerm;
            this.save();
        };

        //更新订单运费
        Cart.prototype.updateFreight = function (freight) {
            this.load();
            this.freight = freight;
            this.save();
        };

        //生成JSON格式订单信息
        Cart.prototype.makeOrderInfo = function () {
            this.load();
            return JSON.stringify(this);
        };
    }

    if (!$.cart) {
        $.cart = new Cart();
    }

    return $.cart;

}));