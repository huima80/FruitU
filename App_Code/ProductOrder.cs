using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// ProductOrder 的摘要说明
/// </summary>
public class ProductOrder
{
    public int ID { get; set; }

    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderID { get; set; }

    /// <summary>
    /// 收货人微信OpenID
    /// </summary>
    public string OpenID { get; set; }

    /// <summary>
    /// 订单商品明细
    /// </summary>
    public List<OrderDetail> OrderDetailList { get; set; }

    /// <summary>
    /// 根据订单明细，计算总价格
    /// </summary>
    public decimal OrderPrice
    {
        get
        {
            decimal sum = 0;
            this.OrderDetailList.ForEach(f =>
            {
                sum += f.PurchasePrice * f.PurchaseQty;
            });
            return sum;
        }
    }

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
    /// 订单中的所有商品名称
    /// </summary>
    public string ProductNames
    {
        get
        {
            string prodNames = "";
            this.OrderDetailList.ForEach(od =>
            {
                prodNames += od.OrderProductName + ",";
            });

            prodNames = prodNames.Trim(',');

            //微信的统一下单API的body参数必填，且要求字符串长度不超过128
            if (prodNames.Length > 128) prodNames = prodNames.Substring(0, 128);

            if (string.IsNullOrEmpty(prodNames)) prodNames = "FruitU商品";

            return prodNames;
        }
    }

    /// <summary>
    /// 微信支付统一下单API返回的预支付回话标示，有效期由统一下单时的参数start_time和end_time决定
    /// </summary>
    public string PrepayID { get; set; }

    public ProductOrder()
    {
        this.OrderDetailList = new List<OrderDetail>();
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

                        foreach (SqlParameter param in cmdAddOrder.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        //插入订单表
                        cmdAddOrder.CommandText = "INSERT INTO [dbo].[ProductOrder] ([OrderID], [OpenID], [DeliverName], [DeliverPhone], [DeliverDate], [DeliverAddress], [OrderMemo], [OrderDate], [TradeState], [TradeStateDesc], [IsDelivered], [IsAccept], [AcceptDate], [PrepayID], [PaymentTerm], [ClientIP]) VALUES (@OrderID,@OpenID,@DeliverName,@DeliverPhone,@DeliverDate,@DeliverAddress,@OrderMemo,@OrderDate,@TradeState,@TradeStateDesc,@IsDelivered,@IsAccept,@AcceptDate,@PrepayID,@PaymentTerm,@ClientIP);select SCOPE_IDENTITY() as 'NewOrderID'";

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
                    if(conn.State == ConnectionState.Open)
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
                        cmdOrder.CommandText = "spSqlPageByRowNum";
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
    /// 查询订单表记录数
    /// </summary>
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
    public static ProductOrder FindOrderByID(int id)
    {
        ProductOrder po = null;

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

                                po.OrderDetailList = FindOrderDetailByPoID(conn, po.ID);

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

        return po;

    }


    /// <summary>
    /// 根据订单OrderID查询特定订单
    /// </summary>
    /// <param name="orderID"></param>
    /// <returns></returns>
    public static ProductOrder FindOrderByOrderID(string orderID)
    {
        if (string.IsNullOrEmpty(orderID))
        {
            throw new ArgumentException("缺少参数OrderID");
        }

        ProductOrder po = null;

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

                                po.OrderDetailList = FindOrderDetailByPoID(conn, po.ID);


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

        return po;

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
                        od.FruitName = sdrOrderDetail["ProductName"].ToString();
                        od.FruitDesc = sdrOrderDetail["ProductDesc"].ToString();
                        od.FruitPrice = decimal.Parse(sdrOrderDetail["ProductPrice"].ToString());
                        od.OnSale = bool.Parse(sdrOrderDetail["ProductOnSale"].ToString());
                        od.InventoryQty = int.Parse(sdrOrderDetail["InventoryQty"].ToString());
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
    /// PrepayID超时后，需要重新发起统一下单，得到新的PrepayID，并更新数据库的“PrepayID、微信支付方式”字段
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prepayID"></param>
    /// <returns></returns>
    public static int UpdatePrepayID(int id, string prepayID)
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

                        cmdOrderID.CommandText = "update ProductOrder set PrepayID = @PrepayID, PaymentTerm = @PaymentTerm where Id=@Id";

                        SqlParameter paramId;
                        paramId = cmdOrderID.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = id;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramPrepayID;
                        paramPrepayID = cmdOrderID.CreateParameter();
                        paramPrepayID.ParameterName = "@PrepayID";
                        paramPrepayID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramPrepayID.Size = 100;
                        paramPrepayID.SqlValue = prepayID;
                        cmdOrderID.Parameters.Add(paramPrepayID);

                        SqlParameter paramPaymentTerm;
                        paramPaymentTerm = cmdOrderID.CreateParameter();
                        paramPaymentTerm.ParameterName = "@PaymentTerm";
                        paramPaymentTerm.SqlDbType = System.Data.SqlDbType.Int;
                        paramPaymentTerm.SqlValue = (int)PaymentTerm.WECHAT;
                        cmdOrderID.Parameters.Add(paramPaymentTerm);

                        if (cmdOrderID.ExecuteNonQuery() == 1)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
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

        return result;
    }

    /// <summary>
    /// 设置订单的发货状态和日期
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isDelivered"></param>
    /// <returns></returns>
    public static int UpdateOrderDeliver(int id, bool isDelivered)
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
                        paramId.SqlValue = id;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramIsDelivered;
                        paramIsDelivered = cmdOrderID.CreateParameter();
                        paramIsDelivered.ParameterName = "@IsDelivered";
                        paramIsDelivered.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsDelivered.SqlValue = isDelivered;
                        cmdOrderID.Parameters.Add(paramIsDelivered);

                        SqlParameter paramDeliverDate = cmdOrderID.CreateParameter();
                        paramDeliverDate.ParameterName = "@DeliverDate";
                        paramDeliverDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramDeliverDate.SqlValue = DateTime.Now;
                        cmdOrderID.Parameters.Add(paramDeliverDate);

                        if (cmdOrderID.ExecuteNonQuery() == 1)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
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

        return result;
    }

    /// <summary>
    /// 更新订单签收状态和日期
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isAccept"></param>
    /// <returns></returns>
    public static int UpdateOrderAcceptance(int id, bool isAccept)
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

                        cmdOrderID.CommandText = "update ProductOrder set IsAccept = @IsAccept, AcceptDate = @AcceptDate where Id=@Id";

                        SqlParameter paramId;
                        paramId = cmdOrderID.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = id;
                        cmdOrderID.Parameters.Add(paramId);

                        SqlParameter paramIsAccept;
                        paramIsAccept = cmdOrderID.CreateParameter();
                        paramIsAccept.ParameterName = "@IsAccept";
                        paramIsAccept.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsAccept.SqlValue = isAccept;
                        cmdOrderID.Parameters.Add(paramIsAccept);

                        SqlParameter paramAcceptDate = cmdOrderID.CreateParameter();
                        paramAcceptDate.ParameterName = "@AcceptDate";
                        paramAcceptDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramAcceptDate.SqlValue = DateTime.Now;
                        cmdOrderID.Parameters.Add(paramAcceptDate);

                        if (cmdOrderID.ExecuteNonQuery() == 1)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
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

        return result;
    }


    /// <summary>
    /// 根据订单号更新订单支付状态
    /// </summary>
    /// <param name="orderID"></param>
    /// <param name="tradeState"></param>
    /// <param name="tradeStateDesc"></param>
    /// <param name="transactionID"></param>
    /// <param name="transactionTime"></param>
    /// <returns></returns>
    public static int UpdateTradeState(string orderID, TradeState tradeState, string tradeStateDesc, string transactionID, DateTime? transactionTime)
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
                        cmdTradeState.CommandText = "update ProductOrder set TradeState = @TradeState, TradeStateDesc=@TradeStateDesc, TransactionID = @TransactionID, TransactionTime=@TransactionTime where OrderID=@OrderID";

                        SqlParameter paramOrderID;
                        paramOrderID = cmdTradeState.CreateParameter();
                        paramOrderID.ParameterName = "@OrderID";
                        paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                        paramOrderID.Size = 50;
                        paramOrderID.SqlValue = orderID;
                        cmdTradeState.Parameters.Add(paramOrderID);

                        SqlParameter paramTradeState;
                        paramTradeState = cmdTradeState.CreateParameter();
                        paramTradeState.ParameterName = "@TradeState";
                        paramTradeState.SqlDbType = System.Data.SqlDbType.Int;
                        paramTradeState.SqlValue = (int)tradeState;
                        cmdTradeState.Parameters.Add(paramTradeState);

                        SqlParameter paramTradeStateDesc;
                        paramTradeStateDesc = cmdTradeState.CreateParameter();
                        paramTradeStateDesc.ParameterName = "@TradeStateDesc";
                        paramTradeStateDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramTradeStateDesc.Size = 1000;
                        paramTradeStateDesc.SqlValue = tradeStateDesc;
                        cmdTradeState.Parameters.Add(paramTradeStateDesc);

                        SqlParameter paramTransactionID;
                        paramTransactionID = cmdTradeState.CreateParameter();
                        paramTransactionID.ParameterName = "@TransactionID";
                        paramTransactionID.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramTransactionID.Size = 50;
                        paramTransactionID.SqlValue = transactionID;
                        cmdTradeState.Parameters.Add(paramTransactionID);

                        SqlParameter paramTransactionTime;
                        paramTransactionTime = cmdTradeState.CreateParameter();
                        paramTransactionTime.ParameterName = "@TransactionTime";
                        paramTransactionTime.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramTransactionTime.SqlValue = transactionTime;
                        cmdTradeState.Parameters.Add(paramTransactionTime);

                        foreach (SqlParameter param in cmdTradeState.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        Log.Debug("根据微信支付通知，更新数据库中的订单支付状态", cmdTradeState.CommandText);

                        if (cmdTradeState.ExecuteNonQuery() == 1)
                        {
                            result = 1;
                        }
                        else
                        {
                            result = 0;
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

        return result;
    }
}

/// <summary>
/// 订单状态
/// </summary>
public enum OrderState
{
    /// <summary>
    /// 已下单
    /// </summary>
    Booked = 1,

    /// <summary>
    /// 已支付
    /// </summary>
    Paid = 2,

    /// <summary>
    /// 商家已发货
    /// </summary>
    Delivered = 3,

    /// <summary>
    /// 买家已签收
    /// </summary>
    Accepted = 4
}

/// <summary>
/// 微信支付状态，参考：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_2
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
    PAYERROR = 7
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