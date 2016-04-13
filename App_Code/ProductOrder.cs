using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using LitJson;

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
    /// 下单人微信OpenID
    /// </summary>
    public string OpenID { get; set; }

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
    /// 根据订单明细，计算总价格
    /// </summary>
    public decimal OrderPrice
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
    /// 提交新订单
    /// 1，如果付款方式是微信支付，则调用微信支付统一下单API、获取prepay_id，订单入库，生成JS支付参数
    /// 2，如果付款方式是货到付款，则直接订单入库
    /// </summary>
    /// <param name="po">待处理订单</param>
    /// <param name="wxJsApiParam">根据prepay_id生成的JS支付参数</param>
    /// <param name="jStateCode">微信支付返回的错误码</param>
    /// <returns>处理后的订单</returns>
    public static ProductOrder SubmitOrder(ProductOrder po, out string wxJsApiParam, out WeChatPayData stateCode)
    {
        if (po == null)
        {
            throw new ArgumentNullException("ProductOrder对象不能为null");
        }

        //根据prepay_id生成的JS支付参数
        wxJsApiParam = string.Empty;

        //微信统一下单API返回的错误码
        stateCode = new WeChatPayData();

        switch (po.PaymentTerm)
        {
            case PaymentTerm.WECHAT:    //付款方式为微信支付，先统一下单、再订单入库

                //统一下单，获取prepay_id，如果有错误发生，则jStateCode有返回值
                po.PrepayID = WxPayAPI.CallUnifiedOrderAPI(po, out stateCode);

                if (stateCode.Count == 0)
                {
                    if (!string.IsNullOrEmpty(po.PrepayID))
                    {
                        //订单入库
                        ProductOrder.AddOrder(po);

                        //根据prepay_id生成JS支付参数
                        wxJsApiParam = WxPayAPI.MakeWXPayJsParam(po.PrepayID);
                    }
                    else
                    {
                        throw new Exception("未能获取微信支付统一下单prepay_id");
                    }
                }

                break;
            case PaymentTerm.CASH:  //付款方式为货到付款，不统一下单，直接入库

                //订单入库
                ProductOrder.AddOrder(po);
                break;
            default:
                throw new Exception("不支持的支付方式。");
        }

        po.OnOrderStateChanged(OrderState.Submitted);

        return po;

    }

    /// <summary>
    /// 处理已有订单
    /// 1，根据订单ID查询订单，如果prepay_id有值，则校验其有效期，如果过期则重新发起统一下单获取prepay_id，并生成JS支付参数
    /// 2，如果没有prepay_id值，则上次下单时为货到付款，则重新发起微信支付统一下单并获取JS支付参数
    /// </summary>
    /// <param name="poID">订单ID</param>
    /// <param name="wxJsApiParam">JS支付参数</param>
    /// <param name="jStateCode">微信支付返回的错误码</param>
    /// <returns></returns>
    public static ProductOrder SubmitOrder(int poID, out string wxJsApiParam, out WeChatPayData stateCode)
    {
        //根据prepay_id生成的JS支付参数
        wxJsApiParam = string.Empty;

        //微信统一下单API返回的错误码
        stateCode = new WeChatPayData();

        //加载完整的订单信息
        ProductOrder po = new ProductOrder(poID);

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
                        //使用新获取的PrepayID更新数据库
                        ProductOrder.UpdatePrepayID(po);
                    }
                }
            }
            else
            {
                //如果PrepayID为空（上次下单时选择的货到付款），这里使用OrderID发起统一下单，首次获取prepay_id
                po.PrepayID = WxPayAPI.CallUnifiedOrderAPI(po, out stateCode);

                if (!string.IsNullOrEmpty(po.PrepayID) && stateCode.Count == 0)
                {
                    //使用首次获得的PrepayID更新数据库
                    ProductOrder.UpdatePrepayID(po);
                }
            }

            if (!string.IsNullOrEmpty(po.PrepayID))
            {
                //根据新的prepay_id生成前端JS支付参数
                wxJsApiParam = WxPayAPI.MakeWXPayJsParam(po.PrepayID);
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
    /// 把订单加入数据库
    /// </summary>
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
                        paramOpenID.SqlValue = po.OpenID;
                        cmdAddOrder.Parameters.Add(paramOpenID);

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

                        foreach (SqlParameter param in cmdAddOrder.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        //插入订单表
                        cmdAddOrder.CommandText = "INSERT INTO [dbo].[ProductOrder] ([OrderID], [OpenID], [DeliverName], [DeliverPhone], [DeliverDate], [DeliverAddress], [OrderMemo], [OrderDate], [TradeState], [TradeStateDesc], [IsDelivered], [IsAccept], [AcceptDate], [PrepayID], [PaymentTerm], [ClientIP], [IsCancel], [CancelDate]) VALUES (@OrderID,@OpenID,@DeliverName,@DeliverPhone,@DeliverDate,@DeliverAddress,@OrderMemo,@OrderDate,@TradeState,@TradeStateDesc,@IsDelivered,@IsAccept,@AcceptDate,@PrepayID,@PaymentTerm,@ClientIP,@IsCancel,@CancelDate);select SCOPE_IDENTITY() as 'NewOrderID'";

                        Log.Debug("插入订单表", cmdAddOrder.CommandText);

                        var newOrderID = cmdAddOrder.ExecuteScalar();

                        //新增的订单ID
                        if (newOrderID != DBNull.Value)
                        {
                            po.ID = int.Parse(newOrderID.ToString());
                        }
                        else
                        {
                            throw new Exception("插入订单错误");
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

                            Log.Debug("插入订单明细表", cmdAddDetail.CommandText);

                            var newOrderDetailID = cmdAddDetail.ExecuteScalar();

                            //新增的订单详情ID
                            if (newOrderDetailID != DBNull.Value)
                            {
                                od.ID = int.Parse(newOrderDetailID.ToString());
                            }
                            else
                            {
                                trans.Rollback();
                                throw new Exception("订单明细表插入错误");
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

                                po.ID = int.Parse(sdrOrder["Id"].ToString());
                                po.OrderID = sdrOrder["OrderID"].ToString();
                                po.OpenID = sdrOrder["OpenID"].ToString();
                                po.DeliverName = sdrOrder["DeliverName"].ToString();
                                po.DeliverPhone = sdrOrder["DeliverPhone"].ToString();
                                po.DeliverAddress = sdrOrder["DeliverAddress"].ToString();
                                po.OrderMemo = sdrOrder["OrderMemo"].ToString();
                                po.OrderDate = DateTime.Parse(sdrOrder["OrderDate"].ToString());
                                po.TradeState = (TradeState)sdrOrder["TradeState"];
                                po.TradeStateDesc = sdrOrder["TradeStateDesc"].ToString();
                                po.IsDelivered = bool.Parse(sdrOrder["IsDelivered"].ToString());
                                po.DeliverDate = sdrOrder["DeliverDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["DeliverDate"].ToString()) : null;
                                po.IsAccept = bool.Parse(sdrOrder["IsAccept"].ToString());
                                po.AcceptDate = sdrOrder["AcceptDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["AcceptDate"].ToString()) : null;
                                po.TransactionID = sdrOrder["TransactionID"].ToString();
                                po.TransactionTime = sdrOrder["TransactionTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["TransactionTime"].ToString()) : null;
                                po.PrepayID = sdrOrder["PrepayID"].ToString();
                                po.PaymentTerm = (PaymentTerm)sdrOrder["PaymentTerm"];
                                po.ClientIP = sdrOrder["ClientIP"].ToString();
                                po.IsCancel = bool.Parse(sdrOrder["IsCancel"].ToString());
                                po.CancelDate = sdrOrder["CancelDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["CancelDate"].ToString()) : null;
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
    /// 分页查询订单，指定：条件字句、排序字句、起始行数、每页行数、总记录数（out）
    /// </summary>
    /// <param name="strWhere"></param>
    /// <param name="strOrder"></param>
    /// <param name="totalRows"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="maximumRows"></param>
    /// <returns>符合条件的记录集</returns>
    public static List<ProductOrder> FindProductOrderPager(string strWhere, string strOrder, out int totalRows, int startRowIndex, int maximumRows = 10)
    {
        return FindProductOrderPager("ProductOrder", "Id", "*", strWhere, strOrder, out totalRows, startRowIndex, maximumRows);
    }

    /// <summary>
    /// 分页查询订单，指定：表名、主键、字段名、条件字句、排序字句、起始行数、每页行数、总记录数（out）
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="pk"></param>
    /// <param name="fieldsName"></param>
    /// <param name="strWhere"></param>
    /// <param name="strOrder"></param>
    /// <param name="totalRows"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="maximumRows"></param>
    /// <returns>符合条件的记录集</returns>
    public static List<ProductOrder> FindProductOrderPager(string tableName, string pk, string fieldsName, string strWhere, string strOrder, out int totalRows, int startRowIndex, int maximumRows = 10)
    {
        List<ProductOrder> poPerPage = new List<ProductOrder>();
        ProductOrder po;

        totalRows = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        cmdOrder.CommandText = "spOrderQuery";
                        cmdOrder.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramTableName = cmdOrder.CreateParameter();
                        paramTableName.ParameterName = "@tbName";
                        paramTableName.SqlDbType = SqlDbType.VarChar;
                        paramTableName.Size = 255;
                        paramTableName.Direction = ParameterDirection.Input;
                        paramTableName.SqlValue = tableName;
                        cmdOrder.Parameters.Add(paramTableName);

                        SqlParameter paramPK = cmdOrder.CreateParameter();
                        paramPK.ParameterName = "@PK";
                        paramPK.SqlDbType = SqlDbType.VarChar;
                        paramPK.Size = 50;
                        paramPK.Direction = ParameterDirection.Input;
                        paramPK.SqlValue = pk;
                        cmdOrder.Parameters.Add(paramPK);

                        SqlParameter paramFields = cmdOrder.CreateParameter();
                        paramFields.ParameterName = "@tbFields";
                        paramFields.SqlDbType = SqlDbType.VarChar;
                        paramFields.Size = 1000;
                        paramFields.Direction = ParameterDirection.Input;
                        paramFields.SqlValue = fieldsName;
                        cmdOrder.Parameters.Add(paramFields);

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

                                po.ID = int.Parse(sdrOrder["Id"].ToString());
                                po.OrderID = sdrOrder["OrderID"].ToString();
                                po.OpenID = sdrOrder["OpenID"].ToString();
                                po.ClientIP = sdrOrder["ClientIP"].ToString();
                                po.DeliverName = sdrOrder["DeliverName"].ToString();
                                po.DeliverPhone = sdrOrder["DeliverPhone"].ToString();
                                po.DeliverAddress = sdrOrder["DeliverAddress"].ToString();
                                po.OrderMemo = sdrOrder["OrderMemo"].ToString();
                                po.OrderDate = DateTime.Parse(sdrOrder["OrderDate"].ToString());
                                po.PaymentTerm = (PaymentTerm)sdrOrder["PaymentTerm"];
                                po.TradeState = (TradeState)sdrOrder["TradeState"];
                                po.TradeStateDesc = sdrOrder["TradeStateDesc"].ToString();
                                po.PrepayID = sdrOrder["PrepayID"].ToString();
                                po.TransactionID = sdrOrder["TransactionID"].ToString();
                                po.TransactionTime = sdrOrder["TransactionTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["TransactionTime"].ToString()) : null;
                                po.IsDelivered = sdrOrder["IsDelivered"] != DBNull.Value ? bool.Parse(sdrOrder["IsDelivered"].ToString()) : false;
                                po.DeliverDate = sdrOrder["DeliverDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["DeliverDate"].ToString()) : null;
                                po.IsAccept = sdrOrder["IsAccept"] != DBNull.Value ? bool.Parse(sdrOrder["IsAccept"].ToString()) : false;
                                po.AcceptDate = sdrOrder["AcceptDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["AcceptDate"].ToString()) : null;
                                po.IsCancel = sdrOrder["IsCancel"] != DBNull.Value ? bool.Parse(sdrOrder["IsCancel"].ToString()) : false;
                                po.CancelDate = sdrOrder["CancelDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["CancelDate"].ToString()) : null;

                                po.OrderDetailList = FindOrderDetailByPoID(conn, po.ID);

                                poPerPage.Add(po);

                            }
                            sdrOrder.Close();
                        }

                        if (!int.TryParse(paramTotalRows.SqlValue.ToString(), out totalRows))
                        {
                            totalRows = 0;
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
    /// 分页查询订单
    /// </summary>
    /// <param name="tableName">待查询表名，可关联</param>
    /// <param name="pk">主键</param>
    /// <param name="fieldsName">待查询字段名</param>
    /// <param name="strWhere">条件SQL字句</param>
    /// <param name="strOrder">排序SQL字句</param>
    /// <param name="totalRows">总记录数</param>
    /// <param name="payingOrderCount">未支付订单数</param>
    /// <param name="deliveringOrderCount">未配送订单数</param>
    /// <param name="acceptingOrderCount">未签收订单数</param>
    /// <param name="startRowIndex">每页开始行号</param>
    /// <param name="maximumRows">每页行数</param>
    /// <returns></returns>
    public static List<ProductOrder> FindProductOrderPager(string tableName, string pk, string fieldsName, string strWhere, string strOrder, out int totalRows, out int payingOrderCount, out int deliveringOrderCount, out int acceptingOrderCount, int startRowIndex, int maximumRows = 10)
    {
        List<ProductOrder> poPerPage = new List<ProductOrder>();
        ProductOrder po;

        totalRows = 0;
        payingOrderCount = 0;
        deliveringOrderCount = 0;
        acceptingOrderCount = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdOrder = conn.CreateCommand())
                    {
                        cmdOrder.CommandText = "spOrderQuery";
                        cmdOrder.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramTableName = cmdOrder.CreateParameter();
                        paramTableName.ParameterName = "@tbName";
                        paramTableName.SqlDbType = SqlDbType.VarChar;
                        paramTableName.Size = 255;
                        paramTableName.Direction = ParameterDirection.Input;
                        paramTableName.SqlValue = tableName;
                        cmdOrder.Parameters.Add(paramTableName);

                        SqlParameter paramPK = cmdOrder.CreateParameter();
                        paramPK.ParameterName = "@PK";
                        paramPK.SqlDbType = SqlDbType.VarChar;
                        paramPK.Size = 50;
                        paramPK.Direction = ParameterDirection.Input;
                        paramPK.SqlValue = pk;
                        cmdOrder.Parameters.Add(paramPK);

                        SqlParameter paramFields = cmdOrder.CreateParameter();
                        paramFields.ParameterName = "@tbFields";
                        paramFields.SqlDbType = SqlDbType.VarChar;
                        paramFields.Size = 1000;
                        paramFields.Direction = ParameterDirection.Input;
                        paramFields.SqlValue = fieldsName;
                        cmdOrder.Parameters.Add(paramFields);

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

                                po.ID = int.Parse(sdrOrder["Id"].ToString());
                                po.OrderID = sdrOrder["OrderID"].ToString();
                                po.OpenID = sdrOrder["OpenID"].ToString();
                                po.ClientIP = sdrOrder["ClientIP"].ToString();
                                po.DeliverName = sdrOrder["DeliverName"].ToString();
                                po.DeliverPhone = sdrOrder["DeliverPhone"].ToString();
                                po.DeliverAddress = sdrOrder["DeliverAddress"].ToString();
                                po.OrderMemo = sdrOrder["OrderMemo"].ToString();
                                po.OrderDate = DateTime.Parse(sdrOrder["OrderDate"].ToString());
                                po.PaymentTerm = (PaymentTerm)sdrOrder["PaymentTerm"];
                                po.TradeState = (TradeState)sdrOrder["TradeState"];
                                po.TradeStateDesc = sdrOrder["TradeStateDesc"].ToString();
                                po.PrepayID = sdrOrder["PrepayID"].ToString();
                                po.TransactionID = sdrOrder["TransactionID"].ToString();
                                po.TransactionTime = sdrOrder["TransactionTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["TransactionTime"].ToString()) : null;
                                po.IsDelivered = sdrOrder["IsDelivered"] != DBNull.Value ? bool.Parse(sdrOrder["IsDelivered"].ToString()) : false;
                                po.DeliverDate = sdrOrder["DeliverDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["DeliverDate"].ToString()) : null;
                                po.IsAccept = sdrOrder["IsAccept"] != DBNull.Value ? bool.Parse(sdrOrder["IsAccept"].ToString()) : false;
                                po.AcceptDate = sdrOrder["AcceptDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["AcceptDate"].ToString()) : null;
                                po.IsCancel = sdrOrder["IsCancel"] != DBNull.Value ? bool.Parse(sdrOrder["IsCancel"].ToString()) : false;
                                po.CancelDate = sdrOrder["CancelDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["CancelDate"].ToString()) : null;

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
    /// <param name="tableName"></param>
    /// <param name="strWhere"></param>
    /// <returns></returns>
    public static int FindProductOrderCount(string tableName, string strWhere)
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
                            cmdOrder.CommandText = string.Format("select count(*) from {0}", tableName);
                        }
                        else
                        {
                            cmdOrder.CommandText = string.Format("select count(*) from {0} where {1}", tableName, strWhere);
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
                                this.ID = int.Parse(sdrOrder["Id"].ToString());
                                this.OrderID = sdrOrder["OrderID"].ToString();
                                this.OpenID = sdrOrder["OpenID"].ToString();
                                this.DeliverName = sdrOrder["DeliverName"].ToString();
                                this.DeliverPhone = sdrOrder["DeliverPhone"].ToString();
                                this.DeliverAddress = sdrOrder["DeliverAddress"].ToString();
                                this.OrderMemo = sdrOrder["OrderMemo"].ToString();
                                this.OrderDate = DateTime.Parse(sdrOrder["OrderDate"].ToString());
                                this.TradeState = (TradeState)sdrOrder["TradeState"];
                                this.TradeStateDesc = sdrOrder["TradeStateDesc"].ToString();
                                this.IsDelivered = bool.Parse(sdrOrder["IsDelivered"].ToString());
                                this.DeliverDate = sdrOrder["DeliverDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["DeliverDate"].ToString()) : null;
                                this.IsAccept = bool.Parse(sdrOrder["IsAccept"].ToString());
                                this.AcceptDate = sdrOrder["AcceptDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["AcceptDate"].ToString()) : null;
                                this.TransactionID = sdrOrder["TransactionID"].ToString();
                                this.TransactionTime = sdrOrder["TransactionTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["TransactionTime"].ToString()) : null;
                                this.PrepayID = sdrOrder["PrepayID"].ToString();
                                this.PaymentTerm = (PaymentTerm)sdrOrder["PaymentTerm"];
                                this.ClientIP = sdrOrder["ClientIP"].ToString();
                                this.IsCancel = bool.Parse(sdrOrder["IsCancel"].ToString());
                                this.CancelDate = sdrOrder["CancelDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["CancelDate"].ToString()) : null;

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
                                this.ID = int.Parse(sdrOrder["Id"].ToString());
                                this.OrderID = sdrOrder["OrderID"].ToString();
                                this.OpenID = sdrOrder["OpenID"].ToString();
                                this.DeliverName = sdrOrder["DeliverName"].ToString();
                                this.DeliverPhone = sdrOrder["DeliverPhone"].ToString();
                                this.DeliverAddress = sdrOrder["DeliverAddress"].ToString();
                                this.OrderMemo = sdrOrder["OrderMemo"].ToString();
                                this.OrderDate = DateTime.Parse(sdrOrder["OrderDate"].ToString());
                                this.TradeState = (TradeState)sdrOrder["TradeState"];
                                this.TradeStateDesc = sdrOrder["TradeStateDesc"].ToString();
                                this.IsDelivered = bool.Parse(sdrOrder["IsDelivered"].ToString());
                                this.DeliverDate = sdrOrder["DeliverDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["DeliverDate"].ToString()) : null;
                                this.IsAccept = bool.Parse(sdrOrder["IsAccept"].ToString());
                                this.AcceptDate = sdrOrder["AcceptDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["AcceptDate"].ToString()) : null;
                                this.TransactionID = sdrOrder["TransactionID"].ToString();
                                this.TransactionTime = sdrOrder["TransactionTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["TransactionTime"].ToString()) : null;
                                this.PrepayID = sdrOrder["PrepayID"].ToString();
                                this.PaymentTerm = (PaymentTerm)sdrOrder["PaymentTerm"];
                                this.ClientIP = sdrOrder["ClientIP"].ToString();
                                this.IsCancel = bool.Parse(sdrOrder["IsCancel"].ToString());
                                this.CancelDate = sdrOrder["CancelDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["CancelDate"].ToString()) : null;

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

                                po.ID = int.Parse(sdrOrder["Id"].ToString());
                                po.OrderID = sdrOrder["OrderID"].ToString();
                                po.OpenID = sdrOrder["OpenID"].ToString();
                                po.DeliverName = sdrOrder["DeliverName"].ToString();
                                po.DeliverPhone = sdrOrder["DeliverPhone"].ToString();
                                po.DeliverAddress = sdrOrder["DeliverAddress"].ToString();
                                po.OrderMemo = sdrOrder["OrderMemo"].ToString();
                                po.OrderDate = DateTime.Parse(sdrOrder["OrderDate"].ToString());
                                po.TradeState = (TradeState)sdrOrder["TradeState"];
                                po.TradeStateDesc = sdrOrder["TradeStateDesc"].ToString();
                                po.IsDelivered = bool.Parse(sdrOrder["IsDelivered"].ToString());
                                po.DeliverDate = sdrOrder["DeliverDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["DeliverDate"].ToString()) : null;
                                po.IsAccept = bool.Parse(sdrOrder["IsAccept"].ToString());
                                po.AcceptDate = sdrOrder["AcceptDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["AcceptDate"].ToString()) : null;
                                po.TransactionID = sdrOrder["TransactionID"].ToString();
                                po.TransactionTime = sdrOrder["TransactionTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["TransactionTime"].ToString()) : null;
                                po.PrepayID = sdrOrder["PrepayID"].ToString();
                                po.PaymentTerm = (PaymentTerm)sdrOrder["PaymentTerm"];
                                po.ClientIP = sdrOrder["ClientIP"].ToString();
                                po.IsCancel = bool.Parse(sdrOrder["IsCancel"].ToString());
                                po.CancelDate = sdrOrder["CancelDate"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrOrder["CancelDate"].ToString()) : null;

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
    /// PrepayID超时后，需要重新发起统一下单，得到新的PrepayID，并更新数据库的“PrepayID、PaymentTerm微信支付方式、TradeState未支付”字段
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static int UpdatePrepayID(ProductOrder po)
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

                        cmdOrderID.CommandText = "update ProductOrder set PrepayID = @PrepayID, PaymentTerm = @PaymentTerm, TradeState = @TradeState where Id=@Id";

                        SqlParameter paramId;
                        paramId = cmdOrderID.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = po.ID;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramPrepayID;
                        paramPrepayID = cmdOrderID.CreateParameter();
                        paramPrepayID.ParameterName = "@PrepayID";
                        paramPrepayID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramPrepayID.Size = 100;
                        paramPrepayID.SqlValue = po.PrepayID;
                        cmdOrderID.Parameters.Add(paramPrepayID);

                        SqlParameter paramPaymentTerm;
                        paramPaymentTerm = cmdOrderID.CreateParameter();
                        paramPaymentTerm.ParameterName = "@PaymentTerm";
                        paramPaymentTerm.SqlDbType = System.Data.SqlDbType.Int;
                        paramPaymentTerm.SqlValue = (int)PaymentTerm.WECHAT;
                        cmdOrderID.Parameters.Add(paramPaymentTerm);

                        SqlParameter paramTradeState;
                        paramTradeState = cmdOrderID.CreateParameter();
                        paramTradeState.ParameterName = "@TradeState";
                        paramTradeState.SqlDbType = System.Data.SqlDbType.Int;
                        paramTradeState.SqlValue = TradeState.NOTPAY;
                        cmdOrderID.Parameters.Add(paramTradeState);

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
    public static int UpdateOrderCancel(ProductOrder po)
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
                        paramOpenID.SqlValue = po.OpenID;
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
    /// 更新订单发货状态和发货时间
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    public static int UpdateOrderDeliver(ProductOrder po)
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
    public static int UpdateOrderAccept(ProductOrder po)
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
                        paramOpenID.SqlValue = po.OpenID;
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
    /// 更新订单的微信支付状态
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

                        //微信支付方式需要额外更新支付方式描述、交易ID、交易时间字段
                        switch (po.PaymentTerm)
                        {
                            case PaymentTerm.WECHAT:
                                cmdTradeState.CommandText = "update ProductOrder set PaymentTerm = @PaymentTerm, TradeState = @TradeState, TradeStateDesc=@TradeStateDesc, TransactionID = @TransactionID, TransactionTime=@TransactionTime where OrderID=@OrderID";

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

                                break;

                            case PaymentTerm.CASH:
                                cmdTradeState.CommandText = "update ProductOrder set PaymentTerm = @PaymentTerm, TradeState = @TradeState where OrderID=@OrderID";

                                break;

                        }


                        foreach (SqlParameter param in cmdTradeState.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        Log.Debug("根据微信支付通知，更新数据库中的订单支付状态", cmdTradeState.CommandText);

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
    /// 订单状态变动事件
    /// </summary>
    /// <param name="os"></param>
    protected void OnOrderStateChanged(OrderState os)
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
/// 微信支付状态，参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_2
/// 1~7是微信支付状态
/// 8~9是现金支付状态
/// </summary>
public enum TradeState
{
    /// <summary>
    /// 支付成功
    /// </summary>
    SUCCESS = 1,

    /// <summary>
    /// 转入退款
    /// </summary>
    REFUND = 2,

    /// <summary>
    /// 未支付
    /// </summary>
    NOTPAY = 3,

    /// <summary>
    /// 已关闭
    /// </summary>
    CLOSED = 4,

    /// <summary>
    /// 已撤销（刷卡支付）
    /// </summary>
    REVOKED = 5,

    /// <summary>
    /// 用户支付中
    /// </summary>
    USERPAYING = 6,

    /// <summary>
    /// 支付失败(其他原因，如银行返回失败)
    /// </summary>
    PAYERROR = 7,

    /// <summary>
    /// 已付现金
    /// </summary>
    CASHPAID = 8,

    /// <summary>
    /// 未付现金
    /// </summary>
    CASHNOTPAID = 9
}

/// <summary>
/// 付款方式
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
    CASH = 2
}