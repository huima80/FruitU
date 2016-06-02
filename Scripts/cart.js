﻿//购物车模块
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
            //购物车在cookies中的key
            cartName: location.host + "_Cart",
            //指定购物车的生命周期是会话级还是永久
            isPersist: true,
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
        //使用积分，默认为0
        this.usedMemberPoints = 0;
        //用户积分账户余额，默认为0
        this.validMemberPoints = 0;
        //积分兑换比率，默认100积分兑换1元
        this.memberPointsExchangeRate = 100;
        //运费条款
        this.freightTerm = {
            //运费标准，默认0元
            freight: 0,
            //包邮条件，默认不包邮
            freightFreeCondition: 0
        };
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
        };

        //从cookies加载购物车信息
        Cart.prototype.load = function () {
            var cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
            $.extend(true, this, cartInfo);
        };

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
            //商品购买数量
            this.qty = qty;
            //商品库存量
            this.inventoryQty = inventoryQty;
        };

        //增加商品项
        Cart.prototype.insertProdItem = function (prodItem) {
            try {
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
                        throw new TypeError("商品购买数量不正确");
                    }
                    if (isNaN(prodItem.inventoryQty)) {
                        throw new TypeError("商品库存量不正确");
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
                    //如果购物车里没有此商品，但购买数量>库存量，则触发超出库存事件onOutOfStock
                    if (prodItem.inventoryQty != -1 && parseInt(prodItem.qty) > parseInt(prodItem.inventoryQty)) {
                        var outOfStockEventArgs = { insertingItem: prodItem };
                        $(this).trigger("onOutOfStock", outOfStockEventArgs);
                        return false;
                    }
                    else {
                        this.prodItems.push(prodItem);
                    }
                }

                this.save();

                //触发商品信息变动后事件
                $(this).trigger("onProdItemInserted", { prodQty: this.prodAmount(), subTotal: this.subTotal() });

                return true;
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //更新商品项
        Cart.prototype.updateProdItem = function (prodItem) {
            try {
                if (!(prodItem instanceof this.ProdItem)) {
                    throw new TypeError("参数应该是ProdItem类的实例");
                }
                else {
                    if (isNaN(prodItem.prodID)) {
                        throw new TypeError("商品ID不正确");
                    }
                    if (isNaN(prodItem.qty) || prodItem.qty < 0) {
                        throw new TypeError("商品购买数量不正确");
                    }
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
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //删除商品项
        Cart.prototype.deleteProdItem = function (prodID) {
            try {
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
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //清空购物车商品项 
        Cart.prototype.clearProdItems = function () {
            try {
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
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //购物车商品总价，不含运费
        Cart.prototype.subTotal = function () {
            try {
                var subTotal = 0;
                this.load();
                for (var i = 0; i < this.prodItems.length; i++) {
                    subTotal += parseFloat(this.prodItems[i].price) * parseInt(this.prodItems[i].qty);
                }
                return subTotal;
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //根据订单商品价格、运费条款计算运费
        Cart.prototype.calFreight = function () {
            try {
                var subTotal, freight;
                subTotal = this.subTotal();
                //根据购物车中的商品价格达到免运费条件，则免运费
                if (subTotal >= this.freightTerm.freightFreeCondition) {
                    freight = 0;
                }
                else {
                    freight = this.freightTerm.freight;
                }
                return freight;
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //购物车订单总价格=商品价格+运费-积分抵扣金额
        Cart.prototype.orderPrice = function () {
            try {
                var orderPrice, subTotal, freight, memberPointsDiscount;
                subTotal = this.subTotal();
                freight = this.calFreight();
                //根据使用的会员积分和兑换比率计算积分抵扣
                memberPointsDiscount = this.usedMemberPoints / this.memberPointsExchangeRate;
                //订单总价格=商品价格+运费-积分抵扣金额
                orderPrice = subTotal + freight - memberPointsDiscount;
                return orderPrice;
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //购物车商品总数
        Cart.prototype.prodAmount = function () {
            try {
                var prodAmount = 0;
                this.load();
                for (var i = 0; i < this.prodItems.length; i++) {
                    prodAmount += parseInt(this.prodItems[i].qty);
                }
                return prodAmount;
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //获取购物车商品项
        Cart.prototype.getProdItems = function () {
            try {
                this.load();
                return $(this.prodItems);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //获取购物车收件人信息
        Cart.prototype.getDeliverInfo = function () {
            try {
                this.load();
                return { name: this.name, phone: this.phone, address: this.address, memo: this.memo };
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //更新收件人信息
        Cart.prototype.updateDeliverInfo = function (name, phone, address, memo) {
            try {
                this.load();
                this.name = name;
                this.phone = phone;
                this.address = address;
                this.memo = memo;
                this.save();
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //更新支付方式
        Cart.prototype.updatePaymentTerm = function (paymentTerm) {
            try {
                this.load();
                this.paymentTerm = paymentTerm;
                this.save();
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //更新会员积分兑换比率
        Cart.prototype.updateMemberPointsExchangeRate = function (memberPointsExchangeRate) {
            try {
                //校验参数
                if (isNaN(memberPointsExchangeRate) || memberPointsExchangeRate <= 0) {
                    throw new TypeError("会员积分兑换比率不正确");
                }
                this.load();
                this.memberPointsExchangeRate = memberPointsExchangeRate;
                this.save();
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //更新使用的积分数
        Cart.prototype.updateUsedMemberPoints = function (usedMemberPoints) {
            try {
                //校验参数
                if (isNaN(usedMemberPoints) || usedMemberPoints < 0) {
                    throw new TypeError("使用的积分数不正确");
                }
                this.load();
                this.usedMemberPoints = usedMemberPoints;
                this.save();

                //触发使用积分更新后事件，传递参数：使用积分优惠金额、订单总金额
                $(this).trigger("onUsedMemberPointsUpdated", { memberPointsDiscount: usedMemberPoints / this.memberPointsExchangeRate, orderPrice: this.orderPrice() });
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //更新用户积分账户余额
        Cart.prototype.updateValidMemberPoints = function (validMemberPoints) {
            try {
                //校验参数
                if (isNaN(validMemberPoints) || validMemberPoints < 0) {
                    throw new TypeError("积分账户余额不正确");
                }
                this.load();
                this.validMemberPoints = validMemberPoints;
                this.save();
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //当前订单金额折算最大可抵扣的会员积分
        Cart.prototype.getMaxDiscountMemberPoints = function () {
            try {
                //此订单最大可抵扣金额=商品价格+运费
                var subTotal = this.subTotal();
                var freight = this.calFreight();
                //此订单最大可使用积分=(此订单最大可抵扣金额/2)*积分兑换比率
                var equivalentMemberPointsOfOrder = Math.floor((subTotal + freight) / 2 * this.memberPointsExchangeRate);
                //判断订单可抵扣金额与积分账户金额，孰小决定了用户最大可使用积分数
                if (equivalentMemberPointsOfOrder < this.validMemberPoints) {
                    return equivalentMemberPointsOfOrder;
                }
                else {
                    return this.validMemberPoints;
                }
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //更新运费条款
        Cart.prototype.updateFreightTerm = function (freight, freightFreeCondition) {
            try {
                //校验参数
                if (isNaN(freight) || freight < 0) {
                    throw new TypeError("运费标准不正确");
                }
                if (isNaN(freightFreeCondition) || freightFreeCondition < 0) {
                    throw new TypeError("包邮条件不正确");
                }
                this.load();
                this.freightTerm.freight = freight;
                this.freightTerm.freightFreeCondition = freightFreeCondition;
                this.save();
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //生成JSON格式订单信息
        Cart.prototype.makeOrderInfo = function () {
            try {
                this.load();
                return JSON.stringify(this);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };
    }

    if (!$.cart) {
        $.cart = new Cart();
    }

    return $.cart;

}));