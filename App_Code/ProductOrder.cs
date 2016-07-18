using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using LitJson;
using Com.Alipay;

/// <summary>
/// ProductOrder 的摘要说明
/// </summary>
public class ProductOrder : IComparable<ProductOrder>
{
    public int ID { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderID { get; set; }

    /// <summary>
    /// 下单人微信信息
    /// </summary>
    public WeChatUser Purchaser { get; set; }

    /// <summary>
    /// 推荐人微信信息
    /// </summary>
    public WeChatUser Agent { get; set; }

    /// <summary>
    /// 订单商品明细
    /// </summary>
    public List<OrderDetail> OrderDetailList { get; set; }

    /// <summary>
    /// 收货人姓名
    /// </summary>
    public string DeliverName { get; set; }

    /// <summary>
    /// 收货人电话
    /// </summary>
    public string DeliverPhone { get; set; }

    /// <summary>
    /// 收货人地址
    /// </summary>
    public string DeliverAddress { get; set; }

    /// <summary>
    /// 收货人备注
    /// </summary>
    public string OrderMemo { get; set; }

    /// <summary>
    /// 订单日期
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// 订单支付状态
    /// </summary>
    public TradeState TradeState { get; set; }

    /// <summary>
    /// 订单支付状态描述
    /// </summary>
    public string TradeStateDesc { get; set; }

    /// <summary>
    /// 是否取消订单
    /// </summary>
    public bool IsCancel { get; set; }

    /// <summary>
    /// 订单取消时间
    /// </summary>
    public DateTime? CancelDate { get; set; }

    /// <summary>
    /// 是否发货
    /// </summary>
    public bool IsDelivered { get; set; }

    /// <summary>
    /// 实际发货时间，可空
    /// </summary>
    public DateTime? DeliverDate { get; set; }

    /// <summary>
    /// 是否签收
    /// </summary>
    public bool IsAccept { get; set; }

    /// <summary>
    /// 签收日期，可空
    /// </summary>
    public DateTime? AcceptDate { get; set; }

    /// <summary>
    /// 货到付款日期，可空
    /// </summary>
    public DateTime? PayCashDate { get; set; }

    /// <summary>
    /// 微信支付交易号
    /// </summary>
    public string TransactionID { get; set; }

    /// <summary>
    /// 微信支付交易完成时间，可空
    /// </summary>
    public DateTime? TransactionTime { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    public PaymentTerm PaymentTerm { get; set; }

    /// <summary>
    /// 订单运费
    /// </summary>
    public decimal Freight { get; set; }

    /// <summary>
    /// 客户端IP
    /// </summary>
    public string ClientIP { get; set; }

    /// <summary>
    /// 微信支付统一下单API返回的预支付回话标示，有效期由统一下单时的参数start_time和end_time决定
    /// </summary>
    public string PrepayID { get; set; }

    /// <summary>
    /// 订单中的所有商品名称
    /// </summary>
    public string ProductNames
    {
        get
        {
            string strProdNames;
            List<string> listProdNames = new List<string>();
            this.OrderDetailList.ForEach(od =>
            {
                listProdNames.Add(od.OrderProductName);
            });

            strProdNames = string.Join<string>(",", listProdNames);

            //微信的统一下单API的body参数必填，且要求字符串长度不超过128
            if (strProdNames.Length > 128) strProdNames = strProdNames.Substring(0, 125) + "...";

            if (string.IsNullOrEmpty(strProdNames)) strProdNames = "FruitU商品";

            return strProdNames;
        }
    }

    /// <summary>
    /// 会员积分抵扣的金额
    /// </summary>
    public decimal MemberPointsDiscount { get; set; }

    /// <summary>
    /// 订单使用的会员积分点数
    /// </summary>
    public int UsedMemberPoints { get; set; }

    /// <summary>
    /// 订单金额是否计算过会员积分
    /// </summary>
    public bool IsCalMemberPoints { get; set; }

    /// <summary>
    /// 订单使用的微信优惠券
    /// </summary>
    public WxCard WxCard { get; set; }

    /// <summary>
    /// 订单使用的微信优惠券折扣金额
    /// </summary>
    public decimal WxCardDiscount { get; set; }

    /// <summary>
    /// 订单总价格 = 商品价格+运费-会员积分抵扣-微信优惠券抵扣
    /// </summary>
    public decimal OrderPrice
    {
        get
        {
            decimal orderPrice = OrderDetailPrice + Freight - MemberPointsDiscount - WxCardDiscount;
            if (orderPrice > 0)
            {
                return orderPrice;
            }
            else
            {
                throw new Exception("订单价格异常");
            }
        }
    }

    /// <summary>
    /// 订单商品项总价格，不含运费
    /// </summary>
    public decimal OrderDetailPrice
    {
        get
        {
            decimal sum = 0;
            this.OrderDetailList.ForEach(od =>
            {
                sum += od.PurchasePrice * od.PurchaseQty;
            });
            return sum;
        }
    }

    /// <summary>
    /// 订单商品总数量
    /// </summary>
    public int OrderDetailCount
    {
        get
        {
            int count = 0;
            this.OrderDetailList.ForEach(od =>
            {
                count += od.PurchaseQty;
            });
            return count;
        }
    }

    /// <summary>
    /// 订单商品详情
    /// </summary>
    public string OrderDetails
    {
        get
        {
            List<string> ods = new List<string>();
            this.OrderDetailList.ForEach(od =>
            {
                ods.Add(string.Format("{0} x{1}", od.OrderProductName, od.PurchaseQty));
            });
            return string.Join<string>(",", ods);
        }
    }

    /// <summary>
    /// 支付宝交易号，最长64位
    /// </summary>
    public string AP_TradeNo { get; set; }

    /// <summary>
    /// 卖家支付宝账户号，以2088开头的纯16位数字。
    /// </summary>
    public string AP_SellerID { get; set; }

    /// <summary>
    /// 卖家支付宝账号，可以是Email或手机号码
    /// </summary>
    public string AP_SellerEmail { get; set; }

    /// <summary>
    /// 买家支付宝账户号，以2088开头的纯16位数字。
    /// </summary>
    public string AP_BuyerID { get; set; }

    /// <summary>
    /// 买家支付宝账号，可以是Email或手机号码
    /// </summary>
    public string AP_BuyerEmail { get; set; }

    /// <summary>
    /// 支付宝通知时间
    /// </summary>
    public DateTime? AP_Notify_Time { get; set; }

    /// <summary>
    /// 支付宝通知类型
    /// </summary>
    public string AP_Notify_Type { get; set; }

    /// <summary>
    /// 支付宝交易创建时间
    /// </summary>
    public DateTime? AP_GMT_Create { get; set; }

    /// <summary>
    /// 支付宝交易付款时间
    /// </summary>
    public DateTime? AP_GMT_Payment { get; set; }

    /// <summary>
    /// 支付宝交易关闭时间
    /// </summary>
    public DateTime? AP_GMT_Close { get; set; }

    /// <summary>
    /// 支付宝退款状态
    /// </summary>
    public RefundStatus? AP_RefundStatus { get; set; }

    /// <summary>
    /// 支付宝退款时间
    /// </summary>
    public DateTime? AP_GMT_Refund { get; set; }

    public ProductOrder()
    {
        this.OrderDetailList = new List<OrderDetail>();
    }

    public ProductOrder(int poID)
    {
        this.FindOrderByID(poID);
    }

    public ProductOrder(string orderID)
    {
        this.FindOrderByOrderID(orderID);
    }

    public delegate JsonData OrderStateChangedEventHandler(ProductOrder sender, OrderStateEventArgs e);

    /// <summary>
    /// 订单状态变动事件
    /// </summary>
    public event OrderStateChangedEventHandler OrderStateChanged;

    /// <summary>
    /// 订单状态变动事件参数类
    /// </summary>
    public class OrderStateEventArgs : EventArgs
    {
        /// <summary>
        /// 订单当前状态
        /// </summary>
        public OrderState OrderState;

        public OrderStateEventArgs()
        {

        }

        public OrderStateEventArgs(OrderState os)
        {
            this.OrderState = os;
        }
    }

    /// <summary>
    /// 订单的会员积分被计入会员积分账户事件
    /// </summary>
    public event EventHandler<MemberPointsCalculatedEventArgs> MemberPointsCalculated;

    public class MemberPointsCalculatedEventArgs : EventArgs
    {
        /// <summary>
        /// 新增的会员积分
        /// </summary>
        public int increasedMemberPoints { get; set; }

        /// <summary>
        /// 消耗的会员积分
        /// </summary>
        public int usedMemberPoints { get; set; }

        /// <summary>
        /// 下单人的会员积分余额
        /// </summary>
        public int newMemberPoints { get; set; }

        /// <summary>
        /// 推荐人的会员积分余额
        /// </summary>
        public int agentNewMemberPoints { get; set; }

        public MemberPointsCalculatedEventArgs()
        {

        }

        public MemberPointsCalculatedEventArgs(int increasedMemberPoints, int usedMemberPoints, int newMemberPoints, int agentNewMemberPoints)
        {
            this.increasedMemberPoints = increasedMemberPoints;
            this.usedMemberPoints = usedMemberPoints;
            this.newMemberPoints = newMemberPoints;
            this.agentNewMemberPoints = agentNewMemberPoints;
        }
    }

    public void OnMemberPointsCalculated(MemberPointsCalculatedEventArgs e)
    {
        if (this.MemberPointsCalculated != null)
        {
            this.MemberPointsCalculated(this, e);
        }
    }


    /// <summary>
    /// 提交新订单，微信支付，调用微信支付统一下单API、获取prepay_id，订单入库，生成JS支付参数
    /// </summary>
    /// <param name="po">待处理订单</param>
    /// <param name="wxPayParam">根据prepay_id生成的JS支付参数</param>
    /// <param name="jStateCode">微信支付返回的错误码</param>
    /// <returns>处理后的订单</returns>
    public static ProductOrder SubmitOrder(ProductOrder po, out string wxPayParam, out WeChatPayData stateCode)
    {
        if (po == null)
        {
            throw new ArgumentNullException("ProductOrder对象不能为null");
        }

        //根据prepay_id生成的JS支付参数
        wxPayParam = string.Empty;

        //微信统一下单API返回的错误码
        stateCode = new WeChatPayData();

        //统一下单，获取prepay_id，如果有错误发生，则jStateCode有返回值
        po.PrepayID = WxPayAPI.CallUnifiedOrderAPI(po, out stateCode);

        if (stateCode.Count == 0)
        {
            if (!string.IsNullOrEmpty(po.PrepayID))
            {
                //订单入库
                ProductOrder.AddOrder(po);

                //根据prepay_id生成JS支付参数
                wxPayParam = WxPayAPI.MakeWXPayJsParam(po.PrepayID);
            }
            else
            {
                throw new Exception("未能获取微信支付统一下单prepay_id");
            }
        }

        //触发订单提交状态事件
        po.OnOrderStateChanged(OrderState.Submitted);

        return po;

    }

    /// <summary>
    /// 提交新订单，支付宝支付
    /// </summary>
    /// <param name="po"></param>
    /// <param name="requestPara">支付宝请求参数</param>
    /// <returns></returns>
    public static ProductOrder SubmitOrder(ProductOrder po, out string requestPara)
    {
        if (po == null)
        {
            throw new ArgumentNullException("ProductOrder对象不能为null");
        }

        //订单入库
        ProductOrder.AddOrder(po);

        //生成支付宝请求参数
        SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
        sParaTemp.Add("partner", AliPayConfig.partner);
        sParaTemp.Add("seller_id", AliPayConfig.seller_id);
        sParaTemp.Add("_input_charset", AliPayConfig.input_charset.ToLower());
        sParaTemp.Add("service", AliPayConfig.service);
        sParaTemp.Add("payment_type", AliPayConfig.payment_type);
        sParaTemp.Add("notify_url", AliPayConfig.notify_url);
        sParaTemp.Add("return_url", AliPayConfig.return_url);
        sParaTemp.Add("out_trade_no", po.OrderID);
        sParaTemp.Add("subject", po.ProductNames);
        sParaTemp.Add("total_fee", po.OrderPrice.ToString());
        sParaTemp.Add("show_url", "http://" + HttpContext.Current.Request.Url.Host);
        sParaTemp.Add("body", po.OrderDetails);

        requestPara = Submit.BuildRequestPara(sParaTemp);

        //触发订单提交状态事件
        po.OnOrderStateChanged(OrderState.Submitted);

        return po;

    }

    /// <summary>
    /// 提交新订单，货到付款
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static ProductOrder SubmitOrder(ProductOrder po)
    {
        if (po == null)
        {
            throw new ArgumentNullException("ProductOrder对象不能为null");
        }

        //订单入库
        ProductOrder.AddOrder(po);

        //触发订单提交状态事件
        po.OnOrderStateChanged(OrderState.Submitted);

        return po;

    }

    /// <summary>
    /// 处理已有订单，支付宝
    /// </summary>
    /// <param name="poID"></param>
    /// <param name="requestPara"></param>
    /// <returns></returns>
    public static ProductOrder SubmitOrder(int poID, out string requestPara)
    {
        //加载完整的订单信息
        ProductOrder po = new ProductOrder(poID);

        //对于已有订单，用户在我的订单中点击“支付宝”后，需要把此订单的支付方式改为“支付宝支付”、支付状态改为“未支付”
        po.PaymentTerm = PaymentTerm.ALIPAY;
        po.TradeState = TradeState.AP_WAIT_BUYER_PAY;

        ProductOrder.UpdatePaymentTerm(po);

        requestPara = string.Empty;
        if (!string.IsNullOrEmpty(po.OrderID))
        {
            //生成支付宝请求参数
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("partner", AliPayConfig.partner);
            sParaTemp.Add("seller_id", AliPayConfig.seller_id);
            sParaTemp.Add("_input_charset", AliPayConfig.input_charset.ToLower());
            sParaTemp.Add("service", AliPayConfig.service);
            sParaTemp.Add("payment_type", AliPayConfig.payment_type);
            sParaTemp.Add("notify_url", AliPayConfig.notify_url);
            sParaTemp.Add("return_url", AliPayConfig.return_url);
            sParaTemp.Add("out_trade_no", po.OrderID);
            sParaTemp.Add("subject", po.ProductNames);
            sParaTemp.Add("total_fee", po.OrderPrice.ToString());
            sParaTemp.Add("show_url", "http://" + HttpContext.Current.Request.Url.Host);
            sParaTemp.Add("body", po.OrderDetails);

            requestPara = Submit.BuildRequestPara(sParaTemp);

        }
        else
        {
            throw new Exception(string.Format("订单“{0}”不存在", poID));
        }

        return po;

    }

    /// <summary>
    /// 处理已有订单，微信支付
    /// 1，根据订单ID查询订单，如果prepay_id有值，则校验其有效期，如果过期则重新发起统一下单获取prepay_id，并生成JS支付参数
    /// 2，如果没有prepay_id值，则上次下单时为货到付款，则重新发起微信支付统一下单并获取JS支付参数
    /// </summary>
    /// <param name="poID">订单ID</param>
    /// <param name="wxPayParam">微信支付JS参数</param>
    /// <param name="jStateCode">微信支付返回的错误码</param>
    /// <returns></returns>
    public static ProductOrder SubmitOrder(int poID, out string wxPayParam, out WeChatPayData stateCode)
    {
        //根据prepay_id生成的JS支付参数
        wxPayParam = string.Empty;

        //微信统一下单API返回的错误码
        stateCode = new WeChatPayData();

        //加载完整的订单信息
        ProductOrder po = new ProductOrder(poID);

        //对于已有订单，用户在我的订单中点击“微信支付”后，需要把此订单的支付方式改为“微信支付”、支付状态改为“未支付”
        po.PaymentTerm = PaymentTerm.WECHAT;
        po.TradeState = TradeState.NOTPAY;

        if (!string.IsNullOrEmpty(po.OrderID))
        {
            //如果此订单的PrepayID不为空（下单时选择的微信支付）
            if (!string.IsNullOrEmpty(po.PrepayID))
            {
                //根据PrepayID中的时间段，计算最近一次获取PrepayID到现在的时间间隔，以此判断PrepayID是否过期。因为OrderDate是下单时间，不会变化，所以不能用其判断。
                DateTime timeOfPrepayID;
                if (!DateTime.TryParseExact(po.PrepayID.Substring(2, 14), "yyyyMMddHHmmss", null, DateTimeStyles.None, out timeOfPrepayID))
                {
                    //如果PrepayID时间截取转换失败，则依据订单时间计算
                    timeOfPrepayID = po.OrderDate;
                }

                //如果上次获取的PrepayID已超过有效期，需要重新统一下单，获取新的prepay_id
                if (DateTime.Now >= timeOfPrepayID.AddMinutes(Config.WeChatOrderExpire))
                {
                    //重新发起统一下单，获取新的prepay_id
                    po.PrepayID = WxPayAPI.CallUnifiedOrderAPI(po, out stateCode);

                    if (!string.IsNullOrEmpty(po.PrepayID) && stateCode.Count == 0)
                    {
                        //更新订单的支付方式、支付状态、新的prepay_id
                        ProductOrder.UpdatePaymentTerm(po);
                    }
                }
            }
            else
            {
                //如果PrepayID为空（上次下单时没有选择微信支付），这里使用OrderID发起统一下单，首次获取prepay_id
                po.PrepayID = WxPayAPI.CallUnifiedOrderAPI(po, out stateCode);

                if (!string.IsNullOrEmpty(po.PrepayID) && stateCode.Count == 0)
                {
                    //更新订单的支付方式、支付状态、新的prepay_id
                    ProductOrder.UpdatePaymentTerm(po);
                }
            }

            if (!string.IsNullOrEmpty(po.PrepayID))
            {
                //根据新的prepay_id生成前端JS支付参数
                wxPayParam = WxPayAPI.MakeWXPayJsParam(po.PrepayID);
            }
            else
            {
                throw new Exception("未能获取微信支付统一下单prepay_id");
            }
        }
        else
        {
            throw new Exception(string.Format("订单“{0}”不存在", poID));
        }

        return po;

    }

    /// <summary>
    /// 根据微信支付商户ID、系统时间、随机数生成唯一订单号
    /// </summary>
    /// <returns></returns>
    public static string MakeOrderID()
    {
        var ran = new Random();
        return string.Format("{0}{1}{2}", Config.MCHID, DateTime.Now.ToString("yyyyMMddHHmmss"), ran.Next(100, 1000));
    }

    /// <summary>
    /// 增加订单，插入订单表、订单明细表
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static ProductOrder AddOrder(ProductOrder po)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    using (SqlCommand cmdAddOrder = conn.CreateCommand())
                    {
                        cmdAddOrder.Transaction = trans;

                        SqlParameter paramOrderID = cmdAddOrder.CreateParameter();
                        paramOrderID.ParameterName = "@OrderID";
                        paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOrderID.Size = 50;
                        paramOrderID.SqlValue = po.OrderID;
                        cmdAddOrder.Parameters.Add(paramOrderID);

                        SqlParameter paramOpenID = cmdAddOrder.CreateParameter();
                        paramOpenID.ParameterName = "@OpenID";
                        paramOpenID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOpenID.Size = 50;
                        paramOpenID.SqlValue = po.Purchaser.OpenID;
                        cmdAddOrder.Parameters.Add(paramOpenID);

                        SqlParameter paramAgentOpenID = cmdAddOrder.CreateParameter();
                        paramAgentOpenID.ParameterName = "@AgentOpenID";
                        paramAgentOpenID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramAgentOpenID.Size = 50;
                        paramAgentOpenID.SqlValue = po.Agent != null ? po.Agent.OpenID : null;
                        cmdAddOrder.Parameters.Add(paramAgentOpenID);

                        SqlParameter paramDeliverName = cmdAddOrder.CreateParameter();
                        paramDeliverName.ParameterName = "@DeliverName";
                        paramDeliverName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramDeliverName.Size = 50;
                        paramDeliverName.SqlValue = po.DeliverName;
                        cmdAddOrder.Parameters.Add(paramDeliverName);

                        SqlParameter paramDeliverPhone = cmdAddOrder.CreateParameter();
                        paramDeliverPhone.ParameterName = "@DeliverPhone";
                        paramDeliverPhone.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramDeliverPhone.Size = 50;
                        paramDeliverPhone.SqlValue = po.DeliverPhone;
                        cmdAddOrder.Parameters.Add(paramDeliverPhone);

                        SqlParameter paramDeliverAddress = cmdAddOrder.CreateParameter();
                        paramDeliverAddress.ParameterName = "@DeliverAddress";
                        paramDeliverAddress.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramDeliverAddress.Size = 1000;
                        paramDeliverAddress.SqlValue = po.DeliverAddress;
                        cmdAddOrder.Parameters.Add(paramDeliverAddress);

                        SqlParameter paramOrderMemo = cmdAddOrder.CreateParameter();
                        paramOrderMemo.ParameterName = "@OrderMemo";
                        paramOrderMemo.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramOrderMemo.Size = 4000;
                        paramOrderMemo.SqlValue = po.OrderMemo;
                        cmdAddOrder.Parameters.Add(paramOrderMemo);

                        SqlParameter paramOrderDate = cmdAddOrder.CreateParameter();
                        paramOrderDate.ParameterName = "@OrderDate";
                        paramOrderDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramOrderDate.SqlValue = po.OrderDate;
                        cmdAddOrder.Parameters.Add(paramOrderDate);

                        SqlParameter paramTradeState = cmdAddOrder.CreateParameter();
                        paramTradeState.ParameterName = "@TradeState";
                        paramTradeState.SqlDbType = System.Data.SqlDbType.Int;
                        paramTradeState.SqlValue = (int)po.TradeState;
                        cmdAddOrder.Parameters.Add(paramTradeState);

                        SqlParameter paramTradeStateDesc = cmdAddOrder.CreateParameter();
                        paramTradeStateDesc.ParameterName = "@TradeStateDesc";
                        paramTradeStateDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramTradeStateDesc.Size = 4000;
                        paramTradeStateDesc.SqlValue = po.TradeStateDesc;
                        cmdAddOrder.Parameters.Add(paramTradeStateDesc);

                        SqlParameter paramIsDelivered = cmdAddOrder.CreateParameter();
                        paramIsDelivered.ParameterName = "@IsDelivered";
                        paramIsDelivered.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsDelivered.SqlValue = po.IsDelivered;
                        cmdAddOrder.Parameters.Add(paramIsDelivered);

                        SqlParameter paramDeliverDate = cmdAddOrder.CreateParameter();
                        paramDeliverDate.ParameterName = "@DeliverDate";
                        paramDeliverDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramDeliverDate.SqlValue = po.DeliverDate;
                        cmdAddOrder.Parameters.Add(paramDeliverDate);

                        SqlParameter paramIsAccept = cmdAddOrder.CreateParameter();
                        paramIsAccept.ParameterName = "@IsAccept";
                        paramIsAccept.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsAccept.SqlValue = po.IsAccept;
                        cmdAddOrder.Parameters.Add(paramIsAccept);

                        SqlParameter paramAcceptDate = cmdAddOrder.CreateParameter();
                        paramAcceptDate.ParameterName = "@AcceptDate";
                        paramAcceptDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramAcceptDate.SqlValue = po.AcceptDate;
                        cmdAddOrder.Parameters.Add(paramAcceptDate);

                        SqlParameter paramPrepayID = cmdAddOrder.CreateParameter();
                        paramPrepayID.ParameterName = "@PrepayID";
                        paramPrepayID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramPrepayID.Size = 100;
                        paramPrepayID.SqlValue = po.PrepayID;
                        cmdAddOrder.Parameters.Add(paramPrepayID);

                        SqlParameter paramPaymentTerm = cmdAddOrder.CreateParameter();
                        paramPaymentTerm.ParameterName = "@PaymentTerm";
                        paramPaymentTerm.SqlDbType = System.Data.SqlDbType.Int;
                        paramPaymentTerm.SqlValue = (int)po.PaymentTerm;
                        cmdAddOrder.Parameters.Add(paramPaymentTerm);

                        SqlParameter paramClientIP = cmdAddOrder.CreateParameter();
                        paramClientIP.ParameterName = "@ClientIP";
                        paramClientIP.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramClientIP.Size = 50;
                        paramClientIP.SqlValue = po.ClientIP;
                        cmdAddOrder.Parameters.Add(paramClientIP);

                        SqlParameter paramIsCancel = cmdAddOrder.CreateParameter();
                        paramIsCancel.ParameterName = "@IsCancel";
                        paramIsCancel.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsCancel.SqlValue = po.IsCancel;
                        cmdAddOrder.Parameters.Add(paramIsCancel);

                        SqlParameter paramCancelDate = cmdAddOrder.CreateParameter();
                        paramCancelDate.ParameterName = "@CancelDate";
                        paramCancelDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramCancelDate.SqlValue = po.CancelDate;
                        cmdAddOrder.Parameters.Add(paramCancelDate);

                        SqlParameter paramFreight = cmdAddOrder.CreateParameter();
                        paramFreight.ParameterName = "@Freight";
                        paramFreight.SqlDbType = System.Data.SqlDbType.Decimal;
                        paramFreight.SqlValue = po.Freight;
                        cmdAddOrder.Parameters.Add(paramFreight);

                        SqlParameter paramMemberPointsDiscount = cmdAddOrder.CreateParameter();
                        paramMemberPointsDiscount.ParameterName = "@MemberPointsDiscount";
                        paramMemberPointsDiscount.SqlDbType = System.Data.SqlDbType.Decimal;
                        paramMemberPointsDiscount.SqlValue = po.MemberPointsDiscount;
                        cmdAddOrder.Parameters.Add(paramMemberPointsDiscount);

                        SqlParameter paramUsedMemberPoints = cmdAddOrder.CreateParameter();
                        paramUsedMemberPoints.ParameterName = "@UsedMemberPoints";
                        paramUsedMemberPoints.SqlDbType = System.Data.SqlDbType.Int;
                        paramUsedMemberPoints.SqlValue = po.UsedMemberPoints;
                        cmdAddOrder.Parameters.Add(paramUsedMemberPoints);

                        SqlParameter paramIsCalMemberPoints = cmdAddOrder.CreateParameter();
                        paramIsCalMemberPoints.ParameterName = "@IsCalMemberPoints";
                        paramIsCalMemberPoints.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsCalMemberPoints.SqlValue = po.IsCalMemberPoints;
                        cmdAddOrder.Parameters.Add(paramIsCalMemberPoints);

                        SqlParameter paramSellerID = cmdAddOrder.CreateParameter();
                        paramSellerID.ParameterName = "@AP_SellerID";
                        paramSellerID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramSellerID.SqlValue = po.AP_SellerID;
                        cmdAddOrder.Parameters.Add(paramSellerID);

                        SqlParameter paramWxCardID = cmdAddOrder.CreateParameter();
                        paramWxCardID.ParameterName = "@WxCardID";
                        paramWxCardID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramWxCardID.SqlValue = (po.WxCard != null) ? po.WxCard.CardID : null;
                        cmdAddOrder.Parameters.Add(paramWxCardID);

                        SqlParameter paramWxCardCode = cmdAddOrder.CreateParameter();
                        paramWxCardCode.ParameterName = "@WxCardCode";
                        paramWxCardCode.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramWxCardCode.SqlValue = (po.WxCard != null) ? po.WxCard.Code : null;
                        cmdAddOrder.Parameters.Add(paramWxCardCode);

                        SqlParameter paramWxCardDiscount = cmdAddOrder.CreateParameter();
                        paramWxCardDiscount.ParameterName = "@WxCardDiscount";
                        paramWxCardDiscount.SqlDbType = System.Data.SqlDbType.Decimal;
                        paramWxCardDiscount.SqlValue = po.WxCardDiscount;
                        cmdAddOrder.Parameters.Add(paramWxCardDiscount);

                        foreach (SqlParameter param in cmdAddOrder.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        //插入订单表
                        cmdAddOrder.CommandText = "INSERT INTO [dbo].[ProductOrder] ([OrderID], [OpenID], [AgentOpenID], [DeliverName], [DeliverPhone], [DeliverDate], [DeliverAddress], [OrderMemo], [OrderDate], [TradeState], [TradeStateDesc], [IsDelivered], [IsAccept], [AcceptDate], [PrepayID], [PaymentTerm], [ClientIP], [IsCancel], [CancelDate], [Freight], [MemberPointsDiscount], [UsedMemberPoints], [IsCalMemberPoints], [AP_SellerID], [WxCardID], [WxCardCode], [WxCardDiscount]) VALUES (@OrderID,@OpenID,@AgentOpenID,@DeliverName,@DeliverPhone,@DeliverDate,@DeliverAddress,@OrderMemo,@OrderDate,@TradeState,@TradeStateDesc,@IsDelivered,@IsAccept,@AcceptDate,@PrepayID,@PaymentTerm,@ClientIP,@IsCancel,@CancelDate,@Freight,@MemberPointsDiscount,@UsedMemberPoints,@IsCalMemberPoints,@AP_SellerID,@WxCardID,@WxCardCode,@WxCardDiscount);select SCOPE_IDENTITY() as 'NewOrderID'";

                        Log.Debug("插入订单表", cmdAddOrder.CommandText);

                        var newOrderID = cmdAddOrder.ExecuteScalar();

                        //新增的订单ID
                        if (newOrderID != DBNull.Value)
                        {
                            po.ID = int.Parse(newOrderID.ToString());
                        }
                        else
                        {
                            throw new Exception(string.Format("插入“{0}”订单错误", po.ProductNames));
                        }
                    }

                    using (SqlCommand cmdAddDetail = conn.CreateCommand())
                    {
                        cmdAddDetail.Transaction = trans;

                        SqlParameter paramDetailPoID = cmdAddDetail.CreateParameter();
                        paramDetailPoID.ParameterName = "@PoID";
                        paramDetailPoID.SqlDbType = System.Data.SqlDbType.Int;
                        paramDetailPoID.SqlValue = po.ID;
                        cmdAddDetail.Parameters.Add(paramDetailPoID);

                        SqlParameter paramProductID = cmdAddDetail.CreateParameter();
                        paramProductID.ParameterName = "@ProductID";
                        paramProductID.SqlDbType = System.Data.SqlDbType.Int;
                        cmdAddDetail.Parameters.Add(paramProductID);

                        SqlParameter paramOrderProductName = cmdAddDetail.CreateParameter();
                        paramOrderProductName.ParameterName = "@OrderProductName";
                        paramOrderProductName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramOrderProductName.Size = 200;
                        cmdAddDetail.Parameters.Add(paramOrderProductName);

                        SqlParameter paramPurchaseQty = cmdAddDetail.CreateParameter();
                        paramPurchaseQty.ParameterName = "@PurchaseQty";
                        paramPurchaseQty.SqlDbType = System.Data.SqlDbType.Int;
                        cmdAddDetail.Parameters.Add(paramPurchaseQty);

                        SqlParameter paramPurchasePrice = cmdAddDetail.CreateParameter();
                        paramPurchasePrice.ParameterName = "@PurchasePrice";
                        paramPurchasePrice.SqlDbType = System.Data.SqlDbType.Decimal;
                        cmdAddDetail.Parameters.Add(paramPurchasePrice);

                        SqlParameter paramPurchaseUnit = cmdAddDetail.CreateParameter();
                        paramPurchaseUnit.ParameterName = "@PurchaseUnit";
                        paramPurchaseUnit.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramPurchaseUnit.Size = 50;
                        cmdAddDetail.Parameters.Add(paramPurchaseUnit);

                        po.OrderDetailList.ForEach(od =>
                        {
                            paramProductID.SqlValue = od.ProductID;
                            paramOrderProductName.SqlValue = od.OrderProductName;
                            paramPurchaseQty.SqlValue = od.PurchaseQty;
                            paramPurchasePrice.SqlValue = od.PurchasePrice;
                            paramPurchaseUnit.SqlValue = od.PurchaseUnit;

                            //插入订单明细表
                            cmdAddDetail.CommandText = "INSERT INTO [dbo].[OrderDetail] ([PoID], [ProductID], [OrderProductName], [PurchaseQty], [PurchasePrice], [PurchaseUnit]) VALUES (@PoID,@ProductID,@OrderProductName,@PurchaseQty,@PurchasePrice,@PurchaseUnit);select SCOPE_IDENTITY() as 'NewOrderDetailID'";

                            var newOrderDetailID = cmdAddDetail.ExecuteScalar();

                            //新增的订单详情ID
                            if (newOrderDetailID != DBNull.Value)
                            {
                                od.ID = int.Parse(newOrderDetailID.ToString());
                            }
                            else
                            {
                                throw new Exception(string.Format("插入订单{0}明细表错误", po.ID));
                            }
                        });
                    }

                    trans.Commit();

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder:AddOrder", ex.ToString());
            throw ex;
        }

        return po;
    }

    /// <summary>
    /// 查询特定日期范围内的订单
    /// </summary>
    /// <param name="orderDate"></param>
    /// <returns></returns>
    public static List<ProductOrder> FindAllOrders(params DateTime[] orderDate)
    {
        List<ProductOrder> poList = new List<ProductOrder>();
        ProductOrder po;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        switch (orderDate.Length)
                        {
                            case 0:
                                cmdOrder.CommandText = "select * from ProductOrder order by OrderDate desc";
                                break;
                            case 1:
                                SqlParameter paramOrderDate = cmdOrder.CreateParameter();
                                paramOrderDate.ParameterName = "@OrderDate";
                                paramOrderDate.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDate.Size = 8;
                                paramOrderDate.SqlValue = orderDate[0].ToString("yyyyMMdd");
                                cmdOrder.Parameters.Add(paramOrderDate);

                                cmdOrder.CommandText = "select * from ProductOrder where CONVERT(varchar(8),OrderDate,112) = @OrderDate order by OrderDate desc";
                                break;
                            case 2:
                                SqlParameter paramOrderDateStart = cmdOrder.CreateParameter();
                                paramOrderDateStart.ParameterName = "@OrderDateStart";
                                paramOrderDateStart.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateStart.Size = 8;
                                paramOrderDateStart.SqlValue = orderDate[0].ToString("yyyyMMdd");
                                cmdOrder.Parameters.Add(paramOrderDateStart);

                                SqlParameter paramOrderDateEnd = cmdOrder.CreateParameter();
                                paramOrderDateEnd.ParameterName = "@OrderDateEnd";
                                paramOrderDateEnd.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateEnd.Size = 8;
                                paramOrderDateEnd.SqlValue = orderDate[1].ToString("yyyyMMdd");
                                cmdOrder.Parameters.Add(paramOrderDateEnd);

                                cmdOrder.CommandText = "select * from ProductOrder where CONVERT(varchar(8),OrderDate,112) >= @OrderDateStart and CONVERT(varchar(8),OrderDate,112) <= @OrderDateEnd order by OrderDate desc";
                                break;
                            default:
                                throw new Exception("参数orderDate只能接收0~2个参数");
                        }

                        Log.Debug("查询特定日期订单", cmdOrder.CommandText);

                        using (SqlDataReader sdrOrder = cmdOrder.ExecuteReader())
                        {
                            while (sdrOrder.Read())
                            {
                                po = new ProductOrder();

                                SDR2PO(po, sdrOrder);

                                po.Purchaser = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["OpenID"].ToString(), false);

                                if (sdrOrder["AgentOpenID"] != null)
                                {
                                    po.Agent = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["AgentOpenID"].ToString(), false);
                                }

                                po.OrderDetailList = FindOrderDetailByPoID(conn, po.ID);

                                poList.Add(po);

                            }
                            sdrOrder.Close();
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("查询所有订单", ex.ToString());
            throw ex;
        }

        return poList;

    }

    /// <summary>
    /// 按条件加载订单信息、订单所属下单人信息
    /// 用于ManageOrder.aspx后台订单管理页面里的分页显示订单，需要加载订单的下单人信息，用于显示订单的下单人微信昵称等信息
    /// </summary>
    /// <param name="strWhere"></param>
    /// <param name="strOrder"></param>
    /// <param name="totalRows"></param>
    /// <param name="payingOrderCount"></param>
    /// <param name="deliveringOrderCount"></param>
    /// <param name="acceptingOrderCount"></param>
    /// <param name="cancelledOrderCount"></param>
    /// <param name="totalOrderPrice"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="maximumRows"></param>
    /// <returns></returns>
    public static List<ProductOrder> FindProductOrderPager(string strWhere, string strOrder, out int totalRows, out int payingOrderCount, out int deliveringOrderCount, out int acceptingOrderCount, out int cancelledOrderCount, out decimal totalOrderPrice, int startRowIndex, int maximumRows = 10)
    {
        //默认加载每个订单所属的用户信息
        return FindProductOrderPager(true, strWhere, strOrder, out totalRows, out payingOrderCount, out deliveringOrderCount, out acceptingOrderCount, out cancelledOrderCount, out totalOrderPrice, startRowIndex, maximumRows);
    }

    /// <summary>
    /// 分页查询订单，可指定是否加载订单的下单人信息，用于前台MyOrders.aspx我的订单页面，不需要加载订单的下单人信息，避免对象转换JSON数据出错
    /// </summary>
    /// <param name="isLoadPurchaser">是否加载订单所属的下单人信息</param>
    /// <param name="strWhere">条件SQL字句</param>
    /// <param name="strOrder">排序SQL字句</param>
    /// <param name="totalRows">总记录数</param>
    /// <param name="payingOrderCount">未支付订单数</param>
    /// <param name="deliveringOrderCount">未配送订单数</param>
    /// <param name="acceptingOrderCount">未签收订单数</param>
    /// <param name="cancelledOrderCount">已撤单订单数</param>
    /// <param name="totalOrderPrice">订单总金额</param>
    /// <param name="startRowIndex">每页开始行号</param>
    /// <param name="maximumRows">每页行数</param>
    /// <returns></returns>
    public static List<ProductOrder> FindProductOrderPager(bool isLoadPurchaser, string strWhere, string strOrder, out int totalRows, out int payingOrderCount, out int deliveringOrderCount, out int acceptingOrderCount, out int cancelledOrderCount, out decimal totalOrderPrice, int startRowIndex, int maximumRows = 10)
    {
        List<ProductOrder> poPerPage = new List<ProductOrder>();
        ProductOrder po;

        totalRows = 0;
        payingOrderCount = 0;
        deliveringOrderCount = 0;
        acceptingOrderCount = 0;
        cancelledOrderCount = 0;
        totalOrderPrice = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        cmdOrder.CommandText = "spOrderQueryPager";
                        cmdOrder.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramMaximumRows = cmdOrder.CreateParameter();
                        paramMaximumRows.ParameterName = "@MaximumRows";
                        paramMaximumRows.SqlDbType = SqlDbType.Int;
                        paramMaximumRows.Direction = ParameterDirection.Input;
                        paramMaximumRows.SqlValue = maximumRows;
                        cmdOrder.Parameters.Add(paramMaximumRows);

                        SqlParameter paramStartRowIndex = cmdOrder.CreateParameter();
                        paramStartRowIndex.ParameterName = "@StartRowIndex";
                        paramStartRowIndex.SqlDbType = SqlDbType.Int;
                        paramStartRowIndex.Direction = ParameterDirection.Input;
                        paramStartRowIndex.SqlValue = startRowIndex;
                        cmdOrder.Parameters.Add(paramStartRowIndex);

                        SqlParameter paramWhere = cmdOrder.CreateParameter();
                        paramWhere.ParameterName = "@strWhere";
                        paramWhere.SqlDbType = SqlDbType.VarChar;
                        paramWhere.Size = 1000;
                        paramWhere.Direction = ParameterDirection.Input;
                        paramWhere.SqlValue = strWhere;
                        cmdOrder.Parameters.Add(paramWhere);

                        SqlParameter paramOrder = cmdOrder.CreateParameter();
                        paramOrder.ParameterName = "@strOrder";
                        paramOrder.SqlDbType = SqlDbType.VarChar;
                        paramOrder.Size = 1000;
                        paramOrder.Direction = ParameterDirection.Input;
                        paramOrder.SqlValue = strOrder;
                        cmdOrder.Parameters.Add(paramOrder);

                        SqlParameter paramTotalRows = cmdOrder.CreateParameter();
                        paramTotalRows.ParameterName = "@TotalRows";
                        paramTotalRows.SqlDbType = SqlDbType.Int;
                        paramTotalRows.Direction = ParameterDirection.Output;
                        cmdOrder.Parameters.Add(paramTotalRows);

                        SqlParameter paramPayingOrderCount = cmdOrder.CreateParameter();
                        paramPayingOrderCount.ParameterName = "@PayingOrderCount";
                        paramPayingOrderCount.SqlDbType = SqlDbType.Int;
                        paramPayingOrderCount.Direction = ParameterDirection.Output;
                        cmdOrder.Parameters.Add(paramPayingOrderCount);

                        SqlParameter paramDeliveringOrderCount = cmdOrder.CreateParameter();
                        paramDeliveringOrderCount.ParameterName = "@DeliveringOrderCount";
                        paramDeliveringOrderCount.SqlDbType = SqlDbType.Int;
                        paramDeliveringOrderCount.Direction = ParameterDirection.Output;
                        cmdOrder.Parameters.Add(paramDeliveringOrderCount);

                        SqlParameter paramAcceptingOrderCount = cmdOrder.CreateParameter();
                        paramAcceptingOrderCount.ParameterName = "@AcceptingOrderCount";
                        paramAcceptingOrderCount.SqlDbType = SqlDbType.Int;
                        paramAcceptingOrderCount.Direction = ParameterDirection.Output;
                        cmdOrder.Parameters.Add(paramAcceptingOrderCount);

                        SqlParameter paramCancelledOrderCount = cmdOrder.CreateParameter();
                        paramCancelledOrderCount.ParameterName = "@CancelledOrderCount";
                        paramCancelledOrderCount.SqlDbType = SqlDbType.Int;
                        paramCancelledOrderCount.Direction = ParameterDirection.Output;
                        cmdOrder.Parameters.Add(paramCancelledOrderCount);

                        SqlParameter paramTotalOrderPrice = cmdOrder.CreateParameter();
                        paramTotalOrderPrice.ParameterName = "@TotalOrderPrice";
                        paramTotalOrderPrice.SqlDbType = SqlDbType.Decimal;
                        paramTotalOrderPrice.Precision = (byte)18;
                        paramTotalOrderPrice.Scale = (byte)2;
                        paramTotalOrderPrice.Direction = ParameterDirection.Output;
                        cmdOrder.Parameters.Add(paramTotalOrderPrice);

                        foreach (SqlParameter param in cmdOrder.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        Log.Debug("分页查询订单", cmdOrder.CommandText);

                        using (SqlDataReader sdrOrder = cmdOrder.ExecuteReader())
                        {
                            while (sdrOrder.Read())
                            {
                                po = new ProductOrder();

                                SDR2PO(po, sdrOrder);

                                if (isLoadPurchaser)
                                {
                                    //此订单的下单人信息，不需要再加载下单人的所有订单列表信息，也不需要刷新下单人活动时间
                                    po.Purchaser = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["OpenID"].ToString(), false);
                                    if (sdrOrder["AgentOpenID"] != null)
                                    {
                                        po.Agent = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["AgentOpenID"].ToString(), false);
                                    }
                                }

                                //此订单的商品详情
                                po.OrderDetailList = FindOrderDetailByPoID(conn, po.ID);

                                poPerPage.Add(po);

                            }
                            sdrOrder.Close();
                        }

                        if (!int.TryParse(paramTotalRows.SqlValue.ToString(), out totalRows))
                        {
                            totalRows = 0;
                        }

                        if (!int.TryParse(paramPayingOrderCount.SqlValue.ToString(), out payingOrderCount))
                        {
                            payingOrderCount = 0;
                        }

                        if (!int.TryParse(paramDeliveringOrderCount.SqlValue.ToString(), out deliveringOrderCount))
                        {
                            deliveringOrderCount = 0;
                        }

                        if (!int.TryParse(paramAcceptingOrderCount.SqlValue.ToString(), out acceptingOrderCount))
                        {
                            acceptingOrderCount = 0;
                        }

                        if (!int.TryParse(paramCancelledOrderCount.SqlValue.ToString(), out cancelledOrderCount))
                        {
                            cancelledOrderCount = 0;
                        }

                        if (!decimal.TryParse(paramTotalOrderPrice.SqlValue.ToString(), out totalOrderPrice))
                        {
                            totalOrderPrice = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("分页查询指定订单", ex.ToString());
            throw ex;
        }

        return poPerPage;

    }

    /// <summary>
    /// 查询订单表记录数
    /// </summary>
    /// <param name="strWhere"></param>
    /// <returns></returns>
    public static int FindProductOrderCount(string strWhere)
    {
        int totalRows = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        if (string.IsNullOrEmpty(strWhere))
                        {
                            cmdOrder.CommandText = string.Format("select count(*) from ProductOrder");
                        }
                        else
                        {
                            cmdOrder.CommandText = string.Format("select count(*) from ProductOrder where {0}", strWhere);
                        }

                        Log.Debug("查询订单表记录数", cmdOrder.CommandText);

                        totalRows = int.Parse(cmdOrder.ExecuteScalar().ToString());

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("指定订单表记录数", ex.ToString());
            throw ex;
        }

        return totalRows;

    }

    /// <summary>
    /// 根据订单ID查询订单
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ProductOrder FindOrderByID(int id)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        cmdOrder.CommandText = "select * from ProductOrder where Id=@Id";

                        Log.Debug("ProductOrder::根据订单ID查询订单", cmdOrder.CommandText);

                        SqlParameter paramID = cmdOrder.CreateParameter();
                        paramID.ParameterName = "@Id";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = id;
                        cmdOrder.Parameters.Add(paramID);

                        using (SqlDataReader sdrOrder = cmdOrder.ExecuteReader())
                        {
                            while (sdrOrder.Read())
                            {
                                SDR2PO(this, sdrOrder);

                                this.Purchaser = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["OpenID"].ToString(), false);
                                if (sdrOrder["AgentOpenID"] != null)
                                {
                                    this.Agent = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["AgentOpenID"].ToString(), false);
                                }

                                this.OrderDetailList = FindOrderDetailByPoID(conn, this.ID);

                            }
                            sdrOrder.Close();
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return this;

    }

    /// <summary>
    /// 根据订单OrderID查询特定订单
    /// </summary>
    /// <param name="orderID"></param>
    /// <returns></returns>
    public ProductOrder FindOrderByOrderID(string orderID)
    {
        if (string.IsNullOrEmpty(orderID))
        {
            throw new ArgumentException("缺少参数OrderID");
        }

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        cmdOrder.CommandText = "select * from ProductOrder where OrderID=@OrderID";

                        Log.Debug("ProductOrder::根据订单OrderID查询订单", cmdOrder.CommandText);

                        SqlParameter paramOrderID = cmdOrder.CreateParameter();
                        paramOrderID.ParameterName = "@OrderID";
                        paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOrderID.Size = 50;
                        paramOrderID.SqlValue = orderID;
                        cmdOrder.Parameters.Add(paramOrderID);

                        using (SqlDataReader sdrOrder = cmdOrder.ExecuteReader())
                        {
                            while (sdrOrder.Read())
                            {
                                SDR2PO(this, sdrOrder);

                                this.Purchaser = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["OpenID"].ToString(), false);
                                if (sdrOrder["AgentOpenID"] != null)
                                {
                                    this.Agent = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["AgentOpenID"].ToString(), false);
                                }

                                this.OrderDetailList = FindOrderDetailByPoID(conn, this.ID);

                            }
                            sdrOrder.Close();
                        }

                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return this;

    }

    /// <summary>
    /// 根据用户OpenID查找所有订单
    /// </summary>
    /// <param name="openID"></param>
    /// <returns></returns>
    public static List<ProductOrder> FindOrderByOpenID(string openID)
    {
        if (string.IsNullOrEmpty(openID))
        {
            throw new ArgumentException("缺少参数OpenID");
        }

        List<ProductOrder> poList = new List<ProductOrder>();
        ProductOrder po;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        cmdOrder.CommandText = "select * from ProductOrder where OpenID=@OpenID order by OrderDate desc";

                        SqlParameter paramOpenID;
                        paramOpenID = cmdOrder.CreateParameter();
                        paramOpenID.ParameterName = "@OpenID";
                        paramOpenID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOpenID.Size = 50;
                        paramOpenID.SqlValue = openID;
                        cmdOrder.Parameters.Add(paramOpenID);

                        Log.Debug("ProductOrder::根据用户OpenID查找所有订单", cmdOrder.CommandText);

                        using (SqlDataReader sdrOrder = cmdOrder.ExecuteReader())
                        {
                            //根据OpenID，在Order表里查询相关订单
                            while (sdrOrder.Read())
                            {
                                po = new ProductOrder();

                                SDR2PO(po, sdrOrder);

                                po.Purchaser = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["OpenID"].ToString(), false);
                                if (sdrOrder["AgentOpenID"] != null)
                                {
                                    po.Agent = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["AgentOpenID"].ToString(), false);
                                }

                                po.OrderDetailList = FindOrderDetailByPoID(conn, po.ID);

                                poList.Add(po);

                            }
                            sdrOrder.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return poList;
    }

    /// <summary>
    /// 根据订单ID查询订单明细
    /// 重要：如果订单项对应的商品项被删除，则会导致OrderDetail关联的Product和ProductImg表相关字段为null值。
    /// 此时对应的业务场景为此订单项对应的商品已下架。
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="poID"></param>
    /// <returns></returns>
    public static List<OrderDetail> FindOrderDetailByPoID(SqlConnection conn, int poID)
    {
        List<OrderDetail> odList = new List<OrderDetail>();
        OrderDetail od;

        try
        {
            using (SqlCommand cmdOrderDetail = conn.CreateCommand())
            {
                cmdOrderDetail.CommandText = "select OrderDetail.*, Product.* from OrderDetail left join Product on OrderDetail.ProductID = Product.Id where PoID=@PoID";

                SqlParameter paramPoID;
                paramPoID = cmdOrderDetail.CreateParameter();
                paramPoID.ParameterName = "@PoID";
                paramPoID.SqlDbType = System.Data.SqlDbType.Int;
                paramPoID.SqlValue = poID;
                cmdOrderDetail.Parameters.Add(paramPoID);

                Log.Debug("根据PoID查询订单明细", cmdOrderDetail.CommandText);

                using (SqlDataReader sdrOrderDetail = cmdOrderDetail.ExecuteReader())
                {
                    //根据指定的订单PoID，在OrderDetail表里查询所有的订单详情
                    while (sdrOrderDetail.Read())
                    {
                        od = new OrderDetail();

                        od.ID = int.Parse(sdrOrderDetail["Id"].ToString());
                        od.ProductID = int.Parse(sdrOrderDetail["ProductID"].ToString());
                        od.FruitName = sdrOrderDetail["ProductName"] != DBNull.Value ? sdrOrderDetail["ProductName"].ToString() : string.Empty;
                        od.FruitDesc = sdrOrderDetail["ProductDesc"] != DBNull.Value ? sdrOrderDetail["ProductDesc"].ToString() : string.Empty;
                        od.FruitPrice = sdrOrderDetail["ProductPrice"] != DBNull.Value ? decimal.Parse(sdrOrderDetail["ProductPrice"].ToString()) : 0;
                        od.OnSale = sdrOrderDetail["ProductOnSale"] != DBNull.Value ? bool.Parse(sdrOrderDetail["ProductOnSale"].ToString()) : false;
                        od.InventoryQty = sdrOrderDetail["InventoryQty"] != DBNull.Value ? int.Parse(sdrOrderDetail["InventoryQty"].ToString()) : 0;
                        od.OrderProductName = sdrOrderDetail["OrderProductName"].ToString();
                        od.PurchasePrice = decimal.Parse(sdrOrderDetail["PurchasePrice"].ToString());
                        od.PurchaseQty = int.Parse(sdrOrderDetail["PurchaseQty"].ToString());
                        od.PurchaseUnit = sdrOrderDetail["PurchaseUnit"].ToString();
                        od.FruitImgList = Fruit.FindFruitImgByProdID(conn, od.ProductID);

                        odList.Add(od);
                    }
                    sdrOrderDetail.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("根据订单PoID查询所有订单详情", ex.ToString());
            throw ex;
        }

        return odList;
    }

    /// <summary>
    /// 根据订单ID查询所有订单明细
    /// </summary>
    /// <param name="poID"></param>
    /// <returns></returns>
    public static List<OrderDetail> FindOrderDetailByPoID(int poID)
    {
        List<OrderDetail> odList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                try
                {
                    conn.Open();

                    odList = FindOrderDetailByPoID(conn, poID);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("根据订单PoID查询所有订单详情", ex.ToString());
            throw ex;
        }

        return odList;
    }

    /// <summary>
    /// 更新订单的支付方式，用于在我的订单页面中，用户对已有订单重新发起支付时，更新此订单的支付方式和支付状态字段
    /// 对于微信支付，在PrepayID超时后，需要重新发起统一下单，得到新的PrepayID，并更新数据库的“PrepayID、PaymentTerm微信支付方式、TradeState未支付”字段
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static int UpdatePaymentTerm(ProductOrder po)
    {
        int result;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrderID = conn.CreateCommand())
                    {
                        SqlParameter paramId;
                        paramId = cmdOrderID.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = po.ID;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramPaymentTerm;
                        paramPaymentTerm = cmdOrderID.CreateParameter();
                        paramPaymentTerm.ParameterName = "@PaymentTerm";
                        paramPaymentTerm.SqlDbType = System.Data.SqlDbType.Int;
                        paramPaymentTerm.SqlValue = (int)po.PaymentTerm;
                        cmdOrderID.Parameters.Add(paramPaymentTerm);

                        SqlParameter paramTradeState;
                        paramTradeState = cmdOrderID.CreateParameter();
                        paramTradeState.ParameterName = "@TradeState";
                        paramTradeState.SqlDbType = System.Data.SqlDbType.Int;
                        paramTradeState.SqlValue = (int)po.TradeState;
                        cmdOrderID.Parameters.Add(paramTradeState);

                        switch (po.PaymentTerm)
                        {
                            case PaymentTerm.WECHAT:
                                cmdOrderID.CommandText = "update ProductOrder set PaymentTerm = @PaymentTerm, TradeState = @TradeState, PrepayID = @PrepayID where Id=@Id";

                                SqlParameter paramPrepayID;
                                paramPrepayID = cmdOrderID.CreateParameter();
                                paramPrepayID.ParameterName = "@PrepayID";
                                paramPrepayID.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramPrepayID.Size = 100;
                                paramPrepayID.SqlValue = po.PrepayID;
                                cmdOrderID.Parameters.Add(paramPrepayID);

                                break;
                            case PaymentTerm.ALIPAY:
                                cmdOrderID.CommandText = "update ProductOrder set PaymentTerm = @PaymentTerm, TradeState = @TradeState, AP_SellerID = @AP_SellerID where Id=@Id";

                                SqlParameter paramAP_SellerID;
                                paramAP_SellerID = cmdOrderID.CreateParameter();
                                paramAP_SellerID.ParameterName = "@AP_SellerID";
                                paramAP_SellerID.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramAP_SellerID.SqlValue = AliPayConfig.seller_id;
                                cmdOrderID.Parameters.Add(paramAP_SellerID);

                                break;
                        }

                        foreach (SqlParameter param in cmdOrderID.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        result = cmdOrderID.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return result;
    }

    /// <summary>
    /// 更新订单撤单状态和撤单时间
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static int CancelOrder(ProductOrder po)
    {
        int result;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrderID = conn.CreateCommand())
                    {
                        cmdOrderID.CommandText = "update ProductOrder set IsCancel = @IsCancel, CancelDate = @CancelDate where Id=@Id and OpenID=@OpenID";

                        SqlParameter paramId;
                        paramId = cmdOrderID.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = po.ID;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramOpenID;
                        paramOpenID = cmdOrderID.CreateParameter();
                        paramOpenID.ParameterName = "@OpenID";
                        paramOpenID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOpenID.SqlValue = po.Purchaser.OpenID;
                        cmdOrderID.Parameters.Add(paramOpenID);

                        SqlParameter paramIsCancel;
                        paramIsCancel = cmdOrderID.CreateParameter();
                        paramIsCancel.ParameterName = "@IsCancel";
                        paramIsCancel.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsCancel.SqlValue = po.IsCancel;
                        cmdOrderID.Parameters.Add(paramIsCancel);

                        SqlParameter paramCancelDate = cmdOrderID.CreateParameter();
                        paramCancelDate.ParameterName = "@CancelDate";
                        paramCancelDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramCancelDate.SqlValue = po.CancelDate;
                        cmdOrderID.Parameters.Add(paramCancelDate);

                        foreach (SqlParameter param in cmdOrderID.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        result = cmdOrderID.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            if (result == 1)
            {
                po.OnOrderStateChanged(OrderState.Cancelled);
            }

        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return result;
    }

    /// <summary>
    /// 更新订单发货状态和发货时间，并消耗订单商品的库存量
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static int DeliverOrder(ProductOrder po)
    {
        int result;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    using (SqlCommand cmdOrderID = conn.CreateCommand())
                    {
                        cmdOrderID.Transaction = trans;
                        cmdOrderID.CommandText = "update ProductOrder set IsDelivered = @IsDelivered, DeliverDate = @DeliverDate where Id=@Id";

                        SqlParameter paramId;
                        paramId = cmdOrderID.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = po.ID;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramIsDelivered;
                        paramIsDelivered = cmdOrderID.CreateParameter();
                        paramIsDelivered.ParameterName = "@IsDelivered";
                        paramIsDelivered.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsDelivered.SqlValue = po.IsDelivered;
                        cmdOrderID.Parameters.Add(paramIsDelivered);

                        SqlParameter paramDeliverDate = cmdOrderID.CreateParameter();
                        paramDeliverDate.ParameterName = "@DeliverDate";
                        paramDeliverDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramDeliverDate.SqlValue = po.DeliverDate;
                        cmdOrderID.Parameters.Add(paramDeliverDate);

                        foreach (SqlParameter param in cmdOrderID.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        result = cmdOrderID.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdConsumeInventory = conn.CreateCommand())
                    {
                        cmdConsumeInventory.Transaction = trans;
                        cmdConsumeInventory.CommandText = "update Product set InventoryQty=@InventoryQty where Id=@Id";

                        SqlParameter paramId = cmdConsumeInventory.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        cmdConsumeInventory.Parameters.Add(paramId);

                        SqlParameter paramInventoryQty = cmdConsumeInventory.CreateParameter();
                        paramInventoryQty.ParameterName = "@InventoryQty";
                        paramInventoryQty.SqlDbType = System.Data.SqlDbType.Int;
                        cmdConsumeInventory.Parameters.Add(paramInventoryQty);

                        //发货后，消耗商品库存量
                        po.OrderDetailList.ForEach(od =>
                        {
                            //如果不是无限量，且库存量>=购买量，则消耗商品库存数
                            if (od.InventoryQty != -1)
                            {
                                paramId.Value = od.ProductID;
                                if (od.InventoryQty >= od.PurchaseQty)
                                {
                                    paramInventoryQty.Value = od.InventoryQty - od.PurchaseQty;
                                }
                                else
                                {
                                    paramInventoryQty.Value = 0;
                                }

                                if (cmdConsumeInventory.ExecuteNonQuery() != 1)
                                {
                                    throw new Exception(string.Format("更新商品“{0}”库存量错误", od.FruitName));
                                }
                            }
                        });
                    }

                    trans.Commit();

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            po.OrderDetailList.ForEach(od =>
            {
                //如果发货后商品剩余库存量不足，则触发报警事件
                if (od.InventoryQty != -1 && ((od.InventoryQty - od.PurchaseQty) <= Config.ProductInventoryWarn))
                {
                    od.OnInventoryWarn();
                }
            });

            if (result == 1)
            {
                po.OnOrderStateChanged(OrderState.Delivered);
            }

        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return result;
    }

    /// <summary>
    /// 更新订单签收状态和签收日期
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static int AcceptOrder(ProductOrder po)
    {
        int result;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrderID = conn.CreateCommand())
                    {

                        cmdOrderID.CommandText = "update ProductOrder set IsAccept = @IsAccept, AcceptDate = @AcceptDate where Id=@Id and OpenID=@OpenID";

                        SqlParameter paramId;
                        paramId = cmdOrderID.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = po.ID;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramOpenID;
                        paramOpenID = cmdOrderID.CreateParameter();
                        paramOpenID.ParameterName = "@OpenID";
                        paramOpenID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOpenID.SqlValue = po.Purchaser.OpenID;
                        cmdOrderID.Parameters.Add(paramOpenID);

                        SqlParameter paramIsAccept;
                        paramIsAccept = cmdOrderID.CreateParameter();
                        paramIsAccept.ParameterName = "@IsAccept";
                        paramIsAccept.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsAccept.SqlValue = po.IsAccept;
                        cmdOrderID.Parameters.Add(paramIsAccept);

                        SqlParameter paramAcceptDate = cmdOrderID.CreateParameter();
                        paramAcceptDate.ParameterName = "@AcceptDate";
                        paramAcceptDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramAcceptDate.SqlValue = po.AcceptDate;
                        cmdOrderID.Parameters.Add(paramAcceptDate);

                        foreach (SqlParameter param in cmdOrderID.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        result = cmdOrderID.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            if (result == 1)
            {
                po.OnOrderStateChanged(OrderState.Accepted);
            }

        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return result;
    }


    /// <summary>
    /// 更新订单的支付状态，根据不同支付方式，更新不同的字段
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static int UpdateTradeState(ProductOrder po)
    {
        int result = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdTradeState = conn.CreateCommand())
                    {
                        SqlParameter paramOrderID;
                        paramOrderID = cmdTradeState.CreateParameter();
                        paramOrderID.ParameterName = "@OrderID";
                        paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOrderID.Size = 50;
                        paramOrderID.SqlValue = po.OrderID;
                        cmdTradeState.Parameters.Add(paramOrderID);

                        SqlParameter paramPaymentTerm;
                        paramPaymentTerm = cmdTradeState.CreateParameter();
                        paramPaymentTerm.ParameterName = "@PaymentTerm";
                        paramPaymentTerm.SqlDbType = System.Data.SqlDbType.Int;
                        paramPaymentTerm.SqlValue = (int)po.PaymentTerm;
                        cmdTradeState.Parameters.Add(paramPaymentTerm);

                        SqlParameter paramTradeState;
                        paramTradeState = cmdTradeState.CreateParameter();
                        paramTradeState.ParameterName = "@TradeState";
                        paramTradeState.SqlDbType = System.Data.SqlDbType.Int;
                        paramTradeState.SqlValue = (int)po.TradeState;
                        cmdTradeState.Parameters.Add(paramTradeState);

                        //不同的支付方式需要更新不同的字段
                        switch (po.PaymentTerm)
                        {
                            case PaymentTerm.WECHAT:
                                SqlParameter paramTradeStateDesc;
                                paramTradeStateDesc = cmdTradeState.CreateParameter();
                                paramTradeStateDesc.ParameterName = "@TradeStateDesc";
                                paramTradeStateDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                                paramTradeStateDesc.Size = 1000;
                                paramTradeStateDesc.SqlValue = po.TradeStateDesc;
                                cmdTradeState.Parameters.Add(paramTradeStateDesc);

                                SqlParameter paramTransactionID;
                                paramTransactionID = cmdTradeState.CreateParameter();
                                paramTransactionID.ParameterName = "@TransactionID";
                                paramTransactionID.SqlDbType = System.Data.SqlDbType.NVarChar;
                                paramTransactionID.Size = 50;
                                paramTransactionID.SqlValue = po.TransactionID;
                                cmdTradeState.Parameters.Add(paramTransactionID);

                                SqlParameter paramTransactionTime;
                                paramTransactionTime = cmdTradeState.CreateParameter();
                                paramTransactionTime.ParameterName = "@TransactionTime";
                                paramTransactionTime.SqlDbType = System.Data.SqlDbType.DateTime;
                                paramTransactionTime.SqlValue = po.TransactionTime;
                                cmdTradeState.Parameters.Add(paramTransactionTime);

                                cmdTradeState.CommandText = "update ProductOrder set PaymentTerm = @PaymentTerm, TradeState = @TradeState, TradeStateDesc=@TradeStateDesc, TransactionID = @TransactionID, TransactionTime=@TransactionTime where OrderID=@OrderID";

                                break;

                            case PaymentTerm.ALIPAY:
                                SqlParameter paramAP_TradeNo;
                                paramAP_TradeNo = cmdTradeState.CreateParameter();
                                paramAP_TradeNo.ParameterName = "@AP_TradeNo";
                                paramAP_TradeNo.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramAP_TradeNo.SqlValue = po.AP_TradeNo;
                                cmdTradeState.Parameters.Add(paramAP_TradeNo);

                                SqlParameter paramAP_SellerEmail;
                                paramAP_SellerEmail = cmdTradeState.CreateParameter();
                                paramAP_SellerEmail.ParameterName = "@AP_SellerEmail";
                                paramAP_SellerEmail.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramAP_SellerEmail.SqlValue = po.AP_SellerEmail;
                                cmdTradeState.Parameters.Add(paramAP_SellerEmail);

                                SqlParameter paramAP_BuyerID;
                                paramAP_BuyerID = cmdTradeState.CreateParameter();
                                paramAP_BuyerID.ParameterName = "@AP_BuyerID";
                                paramAP_BuyerID.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramAP_BuyerID.SqlValue = po.AP_BuyerID;
                                cmdTradeState.Parameters.Add(paramAP_BuyerID);

                                SqlParameter paramAP_BuyerEmail;
                                paramAP_BuyerEmail = cmdTradeState.CreateParameter();
                                paramAP_BuyerEmail.ParameterName = "@AP_BuyerEmail";
                                paramAP_BuyerEmail.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramAP_BuyerEmail.SqlValue = po.AP_BuyerEmail;
                                cmdTradeState.Parameters.Add(paramAP_BuyerEmail);

                                SqlParameter paramAP_Notify_Time;
                                paramAP_Notify_Time = cmdTradeState.CreateParameter();
                                paramAP_Notify_Time.ParameterName = "@AP_Notify_Time";
                                paramAP_Notify_Time.SqlDbType = System.Data.SqlDbType.DateTime;
                                paramAP_Notify_Time.SqlValue = po.AP_Notify_Time;
                                cmdTradeState.Parameters.Add(paramAP_Notify_Time);

                                SqlParameter paramAP_GMT_Create;
                                paramAP_GMT_Create = cmdTradeState.CreateParameter();
                                paramAP_GMT_Create.ParameterName = "@AP_GMT_Create";
                                paramAP_GMT_Create.SqlDbType = System.Data.SqlDbType.DateTime;
                                paramAP_GMT_Create.SqlValue = po.AP_GMT_Create;
                                cmdTradeState.Parameters.Add(paramAP_GMT_Create);

                                SqlParameter paramAP_GMT_Payment;
                                paramAP_GMT_Payment = cmdTradeState.CreateParameter();
                                paramAP_GMT_Payment.ParameterName = "@AP_GMT_Payment";
                                paramAP_GMT_Payment.SqlDbType = System.Data.SqlDbType.DateTime;
                                paramAP_GMT_Payment.SqlValue = po.AP_GMT_Payment;
                                cmdTradeState.Parameters.Add(paramAP_GMT_Payment);

                                SqlParameter paramAP_GMT_Close;
                                paramAP_GMT_Close = cmdTradeState.CreateParameter();
                                paramAP_GMT_Close.ParameterName = "@AP_GMT_Close";
                                paramAP_GMT_Close.SqlDbType = System.Data.SqlDbType.DateTime;
                                paramAP_GMT_Close.SqlValue = po.AP_GMT_Close;
                                cmdTradeState.Parameters.Add(paramAP_GMT_Close);

                                SqlParameter paramAP_GMT_Refund;
                                paramAP_GMT_Refund = cmdTradeState.CreateParameter();
                                paramAP_GMT_Refund.ParameterName = "@AP_GMT_Refund";
                                paramAP_GMT_Refund.SqlDbType = System.Data.SqlDbType.DateTime;
                                paramAP_GMT_Refund.SqlValue = po.AP_GMT_Refund;
                                cmdTradeState.Parameters.Add(paramAP_GMT_Refund);

                                SqlParameter paramAP_RefundStatus;
                                paramAP_RefundStatus = cmdTradeState.CreateParameter();
                                paramAP_RefundStatus.ParameterName = "@AP_RefundStatus";
                                paramAP_RefundStatus.SqlDbType = System.Data.SqlDbType.Int;
                                paramAP_RefundStatus.SqlValue = po.AP_RefundStatus.HasValue ? po.AP_RefundStatus : null;
                                cmdTradeState.Parameters.Add(paramAP_RefundStatus);

                                cmdTradeState.CommandText = "update ProductOrder set PaymentTerm = @PaymentTerm, TradeState = @TradeState, AP_TradeNo = @AP_TradeNo, AP_Notify_Time = @AP_Notify_Time, AP_GMT_Create = @AP_GMT_Create, AP_GMT_Payment = @AP_GMT_Payment, AP_GMT_Close = @AP_GMT_Close, AP_SellerEmail = @AP_SellerEmail, AP_BuyerID = @AP_BuyerID, AP_BuyerEmail = @AP_BuyerEmail, AP_RefundStatus = @AP_RefundStatus, AP_GMT_Refund = @AP_GMT_Refund where OrderID=@OrderID";

                                break;

                            case PaymentTerm.CASH:
                                SqlParameter paramPayCashDate;
                                paramPayCashDate = cmdTradeState.CreateParameter();
                                paramPayCashDate.ParameterName = "@PayCashDate";
                                paramPayCashDate.SqlDbType = System.Data.SqlDbType.DateTime;
                                paramPayCashDate.SqlValue = po.PayCashDate;
                                cmdTradeState.Parameters.Add(paramPayCashDate);

                                cmdTradeState.CommandText = "update ProductOrder set PaymentTerm = @PaymentTerm, TradeState = @TradeState, PayCashDate = @PayCashDate where OrderID=@OrderID";

                                break;

                            default:
                                throw new Exception("不支持的支付方式PaymentTerm");
                        }

                        foreach (SqlParameter param in cmdTradeState.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        Log.Debug("更新数据库中的订单支付状态", cmdTradeState.CommandText);

                        result = cmdTradeState.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            if (result == 1)
            {
                po.OnOrderStateChanged(OrderState.Paid);
            }
        }
        catch (Exception ex)
        {
            Log.Error("ProductOrder", ex.ToString());
            throw ex;
        }

        return result;
    }

    /// <summary>
    /// 同步触发订单状态变动事件
    /// </summary>
    /// <param name="os"></param>
    protected void OnOrderStateChanged(OrderState os)
    {
        if (this.OrderStateChanged != null)
        {
            OrderStateEventArgs ose = new OrderStateEventArgs(os);
            this.OrderStateChanged(this, ose);
        }
    }

    /// <summary>
    /// 异步触发订单状态变动事件
    /// </summary>
    /// <param name="os"></param>
    protected void OnOrderStateChangedAsyn(OrderState os)
    {
        //订单状态变化事件异步回调函数，不阻塞主流程
        if (this.OrderStateChanged != null)
        {
            OrderStateEventArgs ose = new OrderStateEventArgs(os);
            IAsyncResult ar = this.OrderStateChanged.BeginInvoke(this, ose, OrderStateChangedComplete, this.OrderStateChanged);
        }
    }

    /// <summary>
    /// 事件完成异步回调
    /// </summary>
    /// <param name="ar"></param>
    private void OrderStateChangedComplete(IAsyncResult ar)
    {
        if (ar != null)
        {
            JsonData jRet;
            jRet = (ar.AsyncState as ProductOrder.OrderStateChangedEventHandler).EndInvoke(ar);
            if (jRet != null)
            {
                Log.Info("event OrderStateChanged", jRet.ToJson());
            }
        }
    }

    /// <summary>
    /// 订单支付成功后，给予下单人会员积分
    /// </summary>
    /// <param name="po"></param>
    public static void EarnMemberPoints(ProductOrder po)
    {
        try
        {
            if (po == null)
            {
                throw new ArgumentNullException("po对象不能为null");
            }

            //如果订单是支付成功状态才发放积分
            if (po.TradeState == TradeState.SUCCESS ||
                po.TradeState == TradeState.CASHPAID ||
                po.TradeState == TradeState.AP_TRADE_FINISHED || po.TradeState == TradeState.AP_TRADE_SUCCESS)
            {
                //判断此订单是否计算过会员积分
                if (po.Purchaser != null && !po.IsCalMemberPoints)
                {
                    int increasedMemberPoints, newMemberPoints, agentNewMemberPoints;
                    //计算按订单总金额新增的会员积分，1元=1分，从低舍入取整
                    increasedMemberPoints = (int)Math.Floor(po.OrderPrice);

                    //更新会员积分，并获取会员积分余额
                    WeChatUserDAO.UpdateMemberPoints(po, increasedMemberPoints, po.UsedMemberPoints, out newMemberPoints, out agentNewMemberPoints);

                    //触发发放积分事件
                    ProductOrder.MemberPointsCalculatedEventArgs mpce = new ProductOrder.MemberPointsCalculatedEventArgs(increasedMemberPoints, po.UsedMemberPoints, newMemberPoints, agentNewMemberPoints);
                    po.OnMemberPointsCalculated(mpce);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("EarnMemberPoints", ex.ToString());
            throw ex;
        }
    }

    /// <summary>
    /// 订单的数据库字段转换为订单对象
    /// </summary>
    /// <param name="po"></param>
    /// <param name="sdr"></param>
    private static ProductOrder SDR2PO(ProductOrder po, SqlDataReader sdr)
    {
        po.ID = int.Parse(sdr["Id"].ToString());
        po.OrderID = sdr["OrderID"].ToString();
        po.DeliverName = sdr["DeliverName"].ToString();
        po.DeliverPhone = sdr["DeliverPhone"].ToString();
        po.DeliverAddress = sdr["DeliverAddress"].ToString();
        po.OrderMemo = sdr["OrderMemo"].ToString();
        po.OrderDate = DateTime.Parse(sdr["OrderDate"].ToString());
        po.TradeState = (TradeState)sdr["TradeState"];
        po.TradeStateDesc = sdr["TradeStateDesc"].ToString();
        po.IsDelivered = bool.Parse(sdr["IsDelivered"].ToString());
        po.DeliverDate = sdr["DeliverDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["DeliverDate"].ToString()) : null;
        po.IsAccept = bool.Parse(sdr["IsAccept"].ToString());
        po.AcceptDate = sdr["AcceptDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["AcceptDate"].ToString()) : null;
        po.TransactionID = sdr["TransactionID"].ToString();
        po.TransactionTime = sdr["TransactionTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["TransactionTime"].ToString()) : null;
        po.PrepayID = sdr["PrepayID"].ToString();
        po.PaymentTerm = (PaymentTerm)sdr["PaymentTerm"];
        po.ClientIP = sdr["ClientIP"].ToString();
        po.IsCancel = bool.Parse(sdr["IsCancel"].ToString());
        po.CancelDate = sdr["CancelDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["CancelDate"].ToString()) : null;
        po.Freight = sdr["Freight"] != DBNull.Value ? decimal.Parse(sdr["Freight"].ToString()) : 0;
        po.MemberPointsDiscount = sdr["MemberPointsDiscount"] != DBNull.Value ? decimal.Parse(sdr["MemberPointsDiscount"].ToString()) : 0;
        po.UsedMemberPoints = sdr["UsedMemberPoints"] != DBNull.Value ? int.Parse(sdr["UsedMemberPoints"].ToString()) : 0;
        po.IsCalMemberPoints = sdr["IsCalMemberPoints"] != DBNull.Value ? bool.Parse(sdr["IsCalMemberPoints"].ToString()) : false;
        po.PayCashDate = sdr["PayCashDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["PayCashDate"].ToString()) : null;
        po.AP_GMT_Create = sdr["AP_GMT_Create"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["AP_GMT_Create"].ToString()) : null;
        po.AP_GMT_Payment = sdr["AP_GMT_Payment"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["AP_GMT_Payment"].ToString()) : null;
        po.AP_GMT_Close = sdr["AP_GMT_Close"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["AP_GMT_Close"].ToString()) : null;
        po.AP_GMT_Refund = sdr["AP_GMT_Refund"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["AP_GMT_Refund"].ToString()) : null;
        po.AP_Notify_Time = sdr["AP_Notify_Time"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdr["AP_Notify_Time"].ToString()) : null;
        po.AP_TradeNo = sdr["AP_TradeNo"].ToString();
        po.AP_SellerID = sdr["AP_SellerID"].ToString();
        po.AP_SellerEmail = sdr["AP_SellerEmail"].ToString();
        po.AP_BuyerID = sdr["AP_BuyerID"].ToString();
        po.AP_BuyerEmail = sdr["AP_BuyerEmail"].ToString();
        po.AP_RefundStatus = sdr["AP_RefundStatus"] != DBNull.Value ? sdr["AP_RefundStatus"] as RefundStatus? : null;
        //加载微信卡券
        if (sdr["WxCardID"] != DBNull.Value)
        {
            WxCard wxCard = WxCard.GetCard(sdr["WxCardID"].ToString());
            if (wxCard != null)
            {
                po.WxCard = wxCard;
                po.WxCard.Code = sdr["WxCardCode"].ToString();
                po.WxCardDiscount = sdr["WxCardDiscount"] != DBNull.Value ? decimal.Parse(sdr["WxCardDiscount"].ToString()) : 0;
            }
            else
            {
                po.WxCard = null;
                po.WxCardDiscount = 0;
            }
        }
        else
        {
            po.WxCard = null;
            po.WxCardDiscount = 0;
        }

        return po;
    }

    public int CompareTo(ProductOrder other)
    {
        if (this.ID == other.ID && this.OrderID == other.OrderID)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }

}


/// <summary>
/// 订单状态
/// </summary>
public enum OrderState
{
    /// <summary>
    /// 买家已下单
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// 买家已支付
    /// </summary>
    Paid = 2,

    /// <summary>
    /// 商家已发货
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// 买家已签收
    /// </summary>
    Accepted = 4,

    /// <summary>
    /// 买家已撤单
    /// </summary>
    Cancelled = 5
}

/// <summary>
/// 支付方式
/// </summary>
public enum PaymentTerm
{
    /// <summary>
    /// 微信支付
    /// </summary>
    WECHAT = 1,

    /// <summary>
    /// 现金支付
    /// </summary>
    CASH = 2,

    /// <summary>
    /// 支付宝
    /// </summary>
    ALIPAY = 3
}

/// <summary>
/// 订单支付状态
/// 1~7是微信支付状态，参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_2
/// 8~9是现金支付状态
/// 10~14是支付宝状态，参考：https://doc.open.alipay.com/doc2/detail.htm?treeId=60&articleId=103698&docType=1
/// </summary>
public enum TradeState
{
    /// <summary>
    /// 微信支付：支付成功
    /// </summary>
    SUCCESS = 1,

    /// <summary>
    /// 微信支付：转入退款
    /// </summary>
    REFUND = 2,

    /// <summary>
    /// 微信支付：未支付
    /// </summary>
    NOTPAY = 3,

    /// <summary>
    /// 微信支付：已关闭
    /// </summary>
    CLOSED = 4,

    /// <summary>
    /// 微信支付：已撤销（刷卡支付）
    /// </summary>
    REVOKED = 5,

    /// <summary>
    /// 微信支付：用户支付中
    /// </summary>
    USERPAYING = 6,

    /// <summary>
    /// 微信支付：支付失败(其他原因，如银行返回失败)
    /// </summary>
    PAYERROR = 7,

    /// <summary>
    /// 货到付款：已付现金
    /// </summary>
    CASHPAID = 8,

    /// <summary>
    /// 货到付款：未付现金
    /// </summary>
    CASHNOTPAID = 9,

    /// <summary>
    /// 支付宝：交易创建，等待买家付款。
    /// </summary>
    AP_WAIT_BUYER_PAY = 10,

    /// <summary>
    /// 支付宝：在指定时间段内未支付时关闭的交易；在交易完成全额退款成功时关闭的交易
    /// </summary>
    AP_TRADE_CLOSED = 11,

    /// <summary>
    /// 支付宝：交易成功，且可对该交易做操作，如：多级分润、退款等。
    /// 触发条件是商户签约的产品支持退款功能的前提下，买家付款成功。
    /// </summary>
    AP_TRADE_SUCCESS = 12,

    /// <summary>
    /// 支付宝：等待卖家收款（买家付款后，如果卖家账号被冻结）。
    /// </summary>
    AP_TRADE_PENDING = 13,

    /// <summary>
    /// 支付宝：交易成功且结束，即不可再做任何操作。
    /// 触发条件是商户签约的产品不支持退款功能的前提下，买家付款成功；或者，商户签约的产品支持退款功能的前提下，交易已经成功并且已经超过可退款期限。
    /// </summary>
    AP_TRADE_FINISHED = 14

}

/// <summary>
/// 支付宝退款状态，参考：https://doc.open.alipay.com/doc2/detail.htm?treeId=60&articleId=103673&docType=1
/// </summary>
public enum RefundStatus
{
    /// <summary>
    /// 支付宝：退款成功。
    /// 全额退款情况：trade_status= TRADE_CLOSED，而refund_status=REFUND_SUCCESS；
    /// 非全额退款情况：trade_status= TRADE_SUCCESS，而refund_status=REFUND_SUCCESS
    /// </summary>
    AP_REFUND_SUCCESS = 1,

    /// <summary>
    /// 支付宝：退款关闭
    /// </summary>
    AP_REFUND_CLOSED = 2
}

