//达达客户端模块
//https://newopen.imdada.cn/#/development/file
; (function (root, factory) {

    // CommonJS
    if (typeof exports === 'object') {
        module.exports = factory(require('jquery.js'));
    }
        // AMD module
    else if (typeof define === 'function' && define.amd) {
        define(['jquery', 'md5'], factory);
    }
        // Browser global
    else {
        root.cart = factory(jQuery);
    }
}(this, function ($, MD5) {
    
    //达达客户端类
    function DaDaClient() {
        //达达服务器测试环境：newopen.qa.imdada.cn 线上环境：newopen.imdada.cn
        const dadaServer = 'http://newopen.qa.imdada.cn';
        //达达开发者APP KEY
        const appKey = 'dadad2531cd8c66c679';
        //达达开发者APP SECRET
        const appSecret = '44d197085446ff965a577d537f0d7468';
        //商户ID，测试环境：73753
        const sourceID = '73753';
        //门店编号，测试环境：11047059
        const shopNo = '11047059';
        //ajax请求超时
        const ajaxTimeout = 10 * 1000;
        //达达服务器端回调
        const serverCallback = 'http://mahui.me/DaDaCallback.ashx';

        //达达API，新增订单
        const addOrderAPI = '/api/order/addOrder';
        //达达API，重新发单
        const reAddOrderAPI = '/api/order/reAddOrder';
        //达达API，查询订单运费
        const queryDeliverFeeAPI = '/api/order/queryDeliverFee';
        //达达API，查询运费后发单
        const addAfterQueryAPI = '/api/order/addAfterQuery';
        //达达API，查询订单
        const queryOrderAPI = '/api/order/status/query';
        //达达API，取消订单
        const formalCancelAPI = '/api/order/formalCancel';
        //达达API，增加小费
        const addTipAPI = '/api/order/addTip';
        //达达API，获取订单取消原因
        const cancelReasonAPI = '/api/order/cancel/reasons';
        //达达API，获取城市信息列表
        const cityCodeAPI = '/api/cityCode/list';

        //达达API，模拟接受订单
        const acceptOrderAPI = '/api/order/accept';
        //达达API，模拟完成取货
        const fetchOrderAPI = '/api/order/fetch';
        //达达API，模拟完成订单
        const finishOrderAPI = '/api/order/finish';
        //达达API，模拟取消订单
        const cancelOrderAPI = '/api/order/cancel';
        //达达API，模拟订单过期
        const expireOrderAPI = '/api/order/expire';

        //生成请求参数
        function getRequestParam(body) {
            try {
                var reqParam = {
                    'app_key': appKey,
                    'body': (typeof body == "object") ? JSON.stringify(body) : body,
                    'format': 'json',
                    'source_id': sourceID,
                    'timestamp': Date.parse(new Date()) / 1000,
                    'v': '1.0'
                };

                //生成签名
                var sign = appSecret + 'app_key' + reqParam.app_key
                + 'body' + reqParam.body
                + 'format' + reqParam.format
                + 'source_id' + reqParam.source_id
                + 'timestamp' + reqParam.timestamp
                + 'v' + reqParam.v
                + appSecret;

                reqParam.signature = MD5(sign).toString().toUpperCase();

                return reqParam;
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        }

        //发送HTTP请求
        function sendHttpRequest(api, reqParam, callback) {
            $.ajax({
                url: dadaServer + api,
                data: reqParam,
                type: "POST",
                dataType: "json",
                cache: false,
                timeout: ajaxTimeout,
                done: function (data, textStatus, jqXHR) {
                    callback(data);
                },
                fail: function (jqXHR, textStatus, errorThrown) {
                    alert(errorThrown + ":" + textStatus);
                }
            });
        }

        //增加达达订单
        DaDaClient.prototype.addOrder = function (dadaOrder, addType, addOrderCallback) {
            try {
                if (!(dadaOrder instanceof DaDaOrder)) {
                    throw new Error('参数dadaOrder类型错误');
                }
                if (isNaN(addType)) {
                    throw new Error('参数addType类型错误');
                }
                if ((typeof addOrderCallback) != 'function') {
                    throw new Error('缺少回调函数addOrderCallback');
                }
                if (!dadaOrder.ShopNo) {
                    throw new Error('门店编号不能为空');
                }
                if (!dadaOrder.OrderID) {
                    throw new Error('第三方订单ID不能为空');
                }
                if (!dadaOrder.CityCode) {
                    throw new Error('城市编码不能为空');
                }
                if (!dadaOrder.CargoPrice) {
                    throw new Error('订单金额不能为空');
                }
                if (!dadaOrder.IsPrepay) {
                    throw new Error('是否需要垫付不能为空');
                }
                if (!dadaOrder.ExpectedFetchTime) {
                    throw new Error('期望取货时间不能为空');
                }
                if (!dadaOrder.ReceiverName) {
                    throw new Error('收货人姓名不能为空');
                }
                if (!dadaOrder.ReceiverAddress) {
                    throw new Error('收货人地址不能为空');
                }
                //if (!dadaOrder.ReceiverLat) {
                //    throw new Error('收货人地址纬度不能为空');
                //}
                //if (!dadaOrder.ReceiverLng) {
                //    throw new Error('收货人地址经度不能为空');
                //}

                dadaOrder.callback = serverCallback;

                var reqParam = getRequestParam(dadaOrder);

                switch (addType) {
                    case 1:
                        var retVal = sendHttpRequest(addOrderAPI, reqParam, addOrderCallback);
                        break;
                    case 2:
                        var retVal = sendHttpRequest(reAddOrderAPI, reqParam, addOrderCallback);
                        break;
                    case 3:
                        var retVal = sendHttpRequest(queryDeliverFeeAPI, reqParam, addOrderCallback);
                        break;
                    default:
                        throw new Error('增加订单类型错误');
                }

            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //异步查询达达订单状态
        DaDaClient.prototype.queryOrder = function (orderID, queryOrderCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if ((typeof queryOrderCallback) != 'function') {
                    throw new Error('缺少回调函数queryOrderCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(queryOrderAPI, reqParam, queryOrderCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //异步获取达达订单取消原因
        DaDaClient.prototype.cancelReason = function (cancelReasonCallback) {
            try {
                if ((typeof cancelReasonCallback) != 'function') {
                    throw new Error('缺少回调函数cancelReasonCallback');
                }

                var reqParam = getRequestParam('');
                var retVal = sendHttpRequest(cancelReasonAPI, reqParam, cancelReasonCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //异步取消达达订单
        DaDaClient.prototype.formalCancelOrder = function (orderID, cancelReasonID, otherCancelReason, formalCancelOrderCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if (!cancelReasonID || isNaN(cancelReasonID)) {
                    throw new Error('参数cancelReasonID错误');
                }
                if (cancelReasonID == 10000 && !otherCancelReason) {
                    throw new Error('当取消原因ID为其他时，必须指定其他原因)');
                }
                if ((typeof formalCancelOrderCallback) != 'function') {
                    throw new Error('缺少回调函数formalCancelOrderCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(formalCancelOrderAPI, reqParam, formalCancelOrderCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //异步增加达达订单小费
        DaDaClient.prototype.addTip = function (orderID, tips, cityCode, info, addTipCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if (!tips || isNaN(tips)) {
                    throw new Error('参数tips错误');
                }
                if (!cityCode) {
                    throw new Error('缺少参数cityCode');
                }
                if (!info || info.toString().length > 512) {
                    throw new Error('参数info最大长度512');
                }
                if ((typeof addTipCallback) != 'function') {
                    throw new Error('缺少回调函数addTipCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(addTipAPI, reqParam, addTipCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //异步获取城市信息列表
        DaDaClient.prototype.cityCode = function (cityCodeCallback) {
            try {
                if ((typeof cityCodeCallback) != 'function') {
                    throw new Error('缺少回调函数cityCodeCallback');
                }

                var reqParam = getRequestParam('');
                var retVal = sendHttpRequest(cityCodeAPI, reqParam, cityCodeCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //接受订单(仅在测试环境供调试使用)
        DaDaClient.prototype.acceptOrder = function (orderID, acceptOrderCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if ((typeof acceptOrderCallback) != 'function') {
                    throw new Error('缺少回调函数acceptOrderCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(acceptOrderAPI, reqParam, acceptOrderCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //完成取货(仅在测试环境供调试使用)
        DaDaClient.prototype.fetchOrder = function (orderID, fetchOrderCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if ((typeof fetchOrderCallback) != 'function') {
                    throw new Error('缺少回调函数acceptOrderCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(fetchOrderAPI, reqParam, fetchOrderCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //完成订单(仅在测试环境供调试使用)
        DaDaClient.prototype.finishOrder = function (orderID, finishOrderCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if ((typeof finishOrderCallback) != 'function') {
                    throw new Error('缺少回调函数finishOrderCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(finishOrderAPI, reqParam, finishOrderCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //取消订单(仅在测试环境供调试使用)
        DaDaClient.prototype.cancelOrder = function (orderID, cancelOrderCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if ((typeof cancelOrderCallback) != 'function') {
                    throw new Error('缺少回调函数cancelOrderCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(cancelOrderAPI, reqParam, cancelOrderCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };

        //订单过期(仅在测试环境供调试使用)
        DaDaClient.prototype.expireOrder = function (orderID, expireOrderCallback) {
            try {
                if (!orderID) {
                    throw new Error('缺少参数OrderID');
                }
                if ((typeof expireOrderCallback) != 'function') {
                    throw new Error('缺少回调函数expireOrderCallback');
                }

                var reqParam = getRequestParam({ order_id: orderID });
                var retVal = sendHttpRequest(expireOrderAPI, reqParam, expireOrderCallback);
            }
            catch (error) {
                alert(error.message);
                return false;
            }
        };
    }

    //达达订单类
    function DaDaOrder() {
        //门店编号
        this.ShopNo = '';
        //第三方订单ID
        this.OrderID = '';
        //订单所在城市的code
        this.CityCode = '';
        //应付商家金额（单位：元）
        this.PayForSupplierFee = 0;
        //应收用户金额（单位：元）
        this.FetchFromReceiverFee = 0;
        //第三方平台补贴运费金额（单位：元）
        this.DeliverFee = 0;
        //小费（单位：元，精确小数点后一位）
        this.Tips = 0;
        //订单创建时间（时间戳,以秒计算时间，即unix-timestamp）
        this.CreateTime = Date.parse(new Date()) / 1000;
        //订单备注
        this.Info = '';
        //订单商品类型：1、餐饮 2、饮 料 3、鲜花 4、票 务 5、其他 8、印刷品 9、便利店 10、学校餐饮 11、校园便利 12、生鲜 13、水果
        this.CargoType = 0;
        //订单重量（单位：Kg）
        this.CargoWeight = 0;
        //订单金额
        this.CargoPrice = 0;
        //订单商品数量
        this.CargoNum = 0;
        //是否需要垫付 1:是 0:否
        this.IsPrepay = 0;
        //期望取货时间（不能超过24小时，同时，超过期望取货时间30分钟，系统将取消订单;时间戳,以秒计算时间，即unix-timestamp）
        this.ExpectedFetchTime = (new Date()).setHours((new Date()).getMinutes() + 30);
        //期望完成时间（时间戳,以秒计算时间，即unix-timestamp）
        this.ExpectedFinishTime = (new Date()).setHours((new Date()).getHours() + 2);
        //发票抬头
        this.InvoiceTitle = '';
        //收货人姓名
        this.ReceiverName = '';
        //收货人地址
        this.ReceiverAddress = '';
        //收货人手机号
        this.ReceiverPhone = '';
        //收货人地址维度（高德坐标系）
        this.ReceiverLat = '';
        //收货人地址经度（高德坐标系）
        this.ReceiverLng = '';
        //送货开箱码
        this.DeliverLockerCode = '';
        //取货开箱码
        this.PickupLockerCode = '';
        //配送距离(单位：米)
        this.Distance = 0;
        //运费(单位：元)
        this.Fee = 0;
        //订单状态
        this.DaDaOrderStatus = DaDaOrderStatus.Unknown;
        //取消原因
        this.CancelReason = '';
        //送货人ID
        this.DMID = '';
        //送货人姓名
        this.DMName = '';
        //送货人手机
        this.DMMobile = '';
        //订单更新时间
        this.UpdateTime = '';
        //送货人经度
        this.TransporterLng = '';
        //送货人纬度
        this.TransporterLat = '';
        //扣除的违约金(单位：元)
        this.DeductFee = 0;
        //平台订单编号
        this.DeliveryNo = '';
    }

    //达达订单状态枚举类型
    DaDaOrderStatus = {
        //未知状态
        Unknown: 0,
        //待接单
        Accepting: 1,
        //待取货
        Fetching: 2,
        //配送中
        Delivering: 3,
        //已完成
        Finished: 4,
        //已取消：包括配送员取消、商户取消、客服取消、系统取消（待接单、待取货、配送中的订单，3天后自动取消）
        Cancelled: 5,
        //已过期：包括发单后30分钟无人接单，自动过期
        Expired: 6
    };

    return {
        DaDaClient: DaDaClient,
        DaDaOrder: DaDaOrder,
        DaDaOrderStatus: DaDaOrderStatus
    };

}));