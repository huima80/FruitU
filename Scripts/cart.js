//购物车模块，作为jQuery的子对象$.cart
//使用storage存储订单和商品项数据
//支持购物车数据的增删改，并触发响应的ing和ed事件，用于回调页面更新函数
//在页面加载时使用服务端的参数来初始化购物车里的参数，如运费、微信卡券、积分、积分兑换比率
//购物车最后要提交给服务端，使用JSON格式，并进行encode编码
; (function (root, factory) {

    // CommonJS
    if (typeof exports === 'object') {
        module.exports = factory(require('jquery.js'));
    }
        // AMD module
    else if (typeof define === 'function' && define.amd) {
        define(['jquery'], factory);
    }
        // Browser global
    else {
        root.cart = factory(jQuery);
    }
}
(this, function ($) {

    //购物车类
    function Cart(options) {

        //购物车cookies
        var _storage;

        //购物车参数
        this.params = {
            //购物车在cookies中的key
            cartName: location.host + "_Cart",
            //指定购物车的生命周期是会话级还是永久
            isPersist: true
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
        //微信卡券
        this.wxCard = {};
        //订单商品信息
        this.prodItems = [];

        $.extend(this.params, options);
        if (this.params.isPersist) {
            _storage = window.localStorage;
        } else {
            _storage = window.sessionStorage;
        }

        //微信卡券类型枚举值，从服务器端获取值
        Cart.prototype.WxCardType = undefined;

        //微信卡券类
        Cart.prototype.WxCard = function (cardId, encryptCode, cardType, title, leastCost, reduceCost, discount) {
            //卡券ID
            this.cardId = cardId;
            //卡券加密CODE
            this.encryptCode = encryptCode;
            //卡券类型，枚举类型WxCardType
            this.cardType = cardType;
            //卡券名
            this.title = title;
            //代金券专用，表示起用金额
            this.leastCost = leastCost;
            //代金券专用，表示减免金额
            this.reduceCost = reduceCost;
            //折扣券专用，表示打折额度（百分比），例：填30为七折团购详情。
            this.discount = discount;
        };

        //团购类
        Cart.prototype.GroupPurchase = function (id, name, description, startDate, endDate, requiredNumber, groupPrice) {
            try {
                //团购ID
                this.id = id;
                //团购名
                this.name = name;
                //团购说明
                this.description = description;
                //团购开始日期
                this.startDate = startDate;
                //团购结束日期
                this.endDate = endDate;
                //团购需要人数
                this.requiredNumber = requiredNumber;
                //团购价
                this.groupPrice = groupPrice;
            } catch (error) {
                alert(error.message);
                return false;
            }
        };

        //购物车商品项类
        Cart.prototype.ProdItem = function (prodID, prodName, prodDesc, prodImg, price, qty, inventoryQty, groupPurchase, groupPurchaseEventID) {
            try {
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
                //团购对象
                if (groupPurchase == null || (groupPurchase instanceof Cart.prototype.GroupPurchase)) {
                    this.groupPurchase = groupPurchase;
                }
                else {
                    throw new TypeError("参数groupPurchase不是团购对象");
                }
                //团购活动ID
                this.groupPurchaseEventID = groupPurchaseEventID;
            } catch (error) {
                alert(error.message);
                return false;
            }
        };

        //保存购物车信息到cookies
        Cart.prototype.save = function () {
            try {
                if (!!_storage && (_storage == window.localStorage || _storage == window.sessionStorage)) {
                    _storage.setItem(this.params.cartName, JSON.stringify(this));
                } else {
                    throw new Error("请先初始化购物车");
                }
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //从cookies加载购物车信息
        Cart.prototype.load = function () {
            try {
                if (!!_storage && (_storage == window.localStorage || _storage == window.sessionStorage)) {
                    var cartInfo = JSON.parse(_storage.getItem(this.params.cartName));
                    $.extend(true, this, cartInfo);
                } else {
                    throw new Error("请先初始化购物车");
                }
            }
            catch (error) {
                alert(error.message);
                return false;
            }
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
                    if (isNaN(prodItem.qty) || prodItem.qty < 1) {
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
                            //如果购物车里已有此商品，且没有超出库存量，则数量累加
                            this.prodItems[i].qty = parseInt(this.prodItems[i].qty) + parseInt(prodItem.qty);
                            //更新商品库存量，库存量变动的情况：后台改变了库存量
                            this.prodItems[i].inventoryQty = prodItem.inventoryQty;
                            //更新商品单价，单价变动的情况：
                            //1，后台改变了商品单价
                            //2，用户选择了团购价
                            this.prodItems[i].price = prodItem.price;
                            //更新此商品项的团购情况
                            this.prodItems[i].groupPurchase = prodItem.groupPurchase;
                            this.prodItems[i].groupPurchaseEventID = prodItem.groupPurchaseEventID;
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
                    if (isNaN(prodItem.qty) || prodItem.qty < 1) {
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

        //购物车订单总价格=商品价格+运费-积分抵扣金额-微信卡券优惠
        Cart.prototype.orderPrice = function () {
            try {
                if (this.WxCardType == undefined) {
                    throw new TypeError("微信卡券类型枚举值不能为空");
                }
                var orderPrice, subTotal, freight, memberPointsDiscount;
                this.load();
                subTotal = this.subTotal();
                freight = this.calFreight();
                //根据使用的会员积分和兑换比率计算积分抵扣
                memberPointsDiscount = this.usedMemberPoints / this.memberPointsExchangeRate;
                //计算微信卡券优惠
                switch (this.wxCard.cardType) {
                    //代金券
                    case this.WxCardType.cash:
                        if (!isNaN(this.wxCard.leastCost) && !isNaN(this.wxCard.reduceCost)) {
                            //订单总价格=商品价格+运费-积分抵扣金额-微信卡券优惠
                            if ((subTotal + freight - memberPointsDiscount) >= this.wxCard.leastCost && (subTotal + freight - memberPointsDiscount) > this.wxCard.reduceCost) {
                                orderPrice = subTotal + freight - memberPointsDiscount - this.wxCard.reduceCost;
                            }
                            else {
                                throw new Error("价格异常：订单价格不能小于卡券优惠价格");
                            }
                        }
                        else {
                            throw new Error("价格异常：微信卡券起用金额和优惠金额不正确");
                        }
                        break;
                        //折扣券
                    case this.WxCardType.discount:
                        if (!isNaN(this.wxCard.discount) && this.wxCard.discount > 0 && this.wxCard.discount < 100) {
                            //订单总价格=商品价格+运费-积分抵扣金额-微信卡券优惠
                            orderPrice = (subTotal + freight - memberPointsDiscount) * ((100 - this.wxCard.discount) / 100);
                        }
                        else {
                            throw new Error("价格异常：折扣比例不正确");
                        }
                        break;
                    default:
                        //订单总价格=商品价格+运费-积分抵扣金额
                        orderPrice = subTotal + freight - memberPointsDiscount;
                        break;
                }
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

        //更新微信卡券类型枚举值
        Cart.prototype.updateWxCardType = function (wxCardType) {
            try {
                //校验参数
                if (typeof wxCardType != "object") {
                    throw new TypeError("微信卡券类型枚举值错误");
                }
                this.load();
                this.WxCardType = wxCardType;
                this.save();
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //把用户选择的微信卡券更新到购物车里
        Cart.prototype.updateWxCard = function (wxCard) {
            try {
                if (!(wxCard instanceof this.WxCard)) {
                    throw new TypeError("参数应该是WxCard类的实例");
                }
                if (this.WxCardType == {}) {
                    throw new TypeError("微信卡券类型枚举值不能为空");
                }
                var orderPrice, subTotal, freight, isMeetCondition, isSupported;
                this.load();

                //微信卡券为空的场景：
                //1.用户取消勾选“微信卡券”checkbox
                //2.在卡券列表中点击“取消”
                if (wxCard.cardId == undefined) {
                    this.wxCard = wxCard;
                }
                else {
                    switch (wxCard.cardType) {
                        //代金券
                        case this.WxCardType.cash:
                            subTotal = this.subTotal();
                            freight = this.calFreight();
                            //有门槛的代金券：商品价格+运费>=微信优惠券起用金额，才能使用优惠券
                            if (wxCard.leastCost > 0) {
                                if (subTotal + freight >= wxCard.leastCost) {
                                    this.wxCard = wxCard;
                                    isMeetCondition = true;
                                    isSupported = true;
                                }
                                else {
                                    this.wxCard = new this.WxCard();
                                    isMeetCondition = false;
                                    isSupported = true;
                                }
                            }
                            else {
                                //无门槛的代金券：商品价格+运费>微信优惠券优惠金额，才能使用优惠券
                                if (subTotal + freight > wxCard.reduceCost) {
                                    this.wxCard = wxCard;
                                    isMeetCondition = true;
                                    isSupported = true;
                                }
                                else {
                                    this.wxCard = new this.WxCard();
                                    isMeetCondition = false;
                                    isSupported = true;
                                }
                            }
                            break;
                            //折扣券
                        case this.WxCardType.discount:
                            this.wxCard = wxCard;
                            isMeetCondition = true;
                            isSupported = true;
                            break;
                        default:
                            this.wxCard = new this.WxCard();
                            isMeetCondition = undefined;
                            isSupported = false;
                            break;
                    }
                }
                this.save();

                //触发使用微信卡券更新后事件，传递参数：微信卡券名称，订单总金额，是否满足卡券使用条件，是否支持的卡券
                $(this).trigger("onWxCardUpdated", { wxCard: this.wxCard, orderPrice: this.orderPrice(), isMeetCondition: isMeetCondition, isSupported: isSupported });
            }
            catch (error) {
                alert(error.message);
            }
        };

        //生成JSON格式订单信息
        Cart.prototype.makeOrderInfo = function () {
            try {
                var orderInfo;
                this.load();
                //对特殊字符编码，如;/?:@&=+$,#
                orderInfo = encodeURIComponent(JSON.stringify(this));
                return orderInfo;
            }
            catch (error) {
                alert(error.message);
                return undefined;
            }
        };

    }

    if (!$.cart) {
        $.cart = new Cart();
    }

    return $.cart;

}));