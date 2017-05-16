using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// 闪送订单类
/// </summary>
public class ShanSongOrder
{
    public ShanSongOrder()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public int ID { get; set; }

    /// <summary>
    /// 闪送订单状态
    /// </summary>
    public ShanSongOrderStatus OrderStatus { get; set; }

    /// <summary>
    /// 业务订单
    /// </summary>
    public ProductOrder ProductOrder { get; set; }

    /// <summary>
    /// 闪送平台订单号
    /// </summary>
    public string ISSOrderNo { get; set; }

    /// <summary>
    /// 微信门店ID
    /// </summary>
    public int POIID { get; set; }

    /// <summary>
    /// 商户ID
    /// </summary>
    public string MerchantID { get; set; }

    /// <summary>
    /// 商户手机号
    /// </summary>
    public string MerchantMobile { get; set; }

    /// <summary>
    /// 商户名称
    /// </summary>
    public string MerchantName { get; set; }

    /// <summary>
    /// 商户在线支付凭据，如不使用在线支付，可为null，即为寄付
    /// </summary>
    public string MerchantToken { get; set; }

    /// <summary>
    /// 寄件人姓名
    /// </summary>
    public string SenderName { get; set; }

    /// <summary>
    /// 寄件人手机
    /// </summary>
    public string SenderMobile { get; set; }

    /// <summary>
    /// 寄件人城市
    /// </summary>
    public string SenderCity { get; set; }

    /// <summary>
    /// 寄件人地址
    /// </summary>
    public string SenderAddr { get; set; }

    /// <summary>
    /// 寄件人地址详情
    /// </summary>
    public string SenderAddrDetail { get; set; }

    /// <summary>
    /// 寄件人位置经度
    /// </summary>
    public double SenderLng { get; set; }

    /// <summary>
    /// 寄件人位置纬度
    /// </summary>
    public double SenderLat { get; set; }

    /// <summary>
    /// 收件人姓名
    /// </summary>
    public string ReceiverName { get; set; }

    /// <summary>
    /// 收件人手机
    /// </summary>
    public string ReceiverMobile { get; set; }

    /// <summary>
    /// 收件人城市
    /// </summary>
    public string ReceiverCity { get; set; }

    /// <summary>
    /// 收件人地址
    /// </summary>
    public string ReceiverAddr { get; set; }

    /// <summary>
    /// 收件人地址详情
    /// </summary>
    public string ReceiverAddrDetail { get; set; }

    /// <summary>
    /// 收件人位置经度
    /// </summary>
    public double ReceiverLng { get; set; }

    /// <summary>
    /// 收件人地址纬度
    /// </summary>
    public double ReceiverLat { get; set; }

    /// <summary>
    /// 物品
    /// </summary>
    public string Goods { get; set; }

    /// <summary>
    /// 重量，单位：KG，不足整公斤则向上取整
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// 加价费，单位：元，支持5元，10元，15元，20元，不加价则为0
    /// </summary>
    public decimal Addition { get; set; }

    /// <summary>
    /// 备注信息，最长250字符
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 订单金额，单位：元
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 距离，单位：米
    /// </summary>
    public int Distance { get; set; }

    /// <summary>
    /// 优惠金额，单位：元
    /// </summary>
    public decimal CutAmount { get; set; }

    /// <summary>
    /// 取消订单扣费金额，单位：元
    /// </summary>
    public decimal DeductFee { get; set; }

    /// <summary>
    /// 闪送员手机号
    /// </summary>
    public string Courier { get; set; }

    /// <summary>
    /// 闪送员姓名
    /// </summary>
    public string CourierName { get; set; }

    /// <summary>
    /// 取件密码
    /// </summary>
    public string PickupPassword { get; set; }

    /// <summary>
    /// 闪送员轨迹
    /// </summary>
    public List<ShanSongTrialMessage> TrialMessage { get; set; }

    /// <summary>
    /// 预约时间，支持2小时以后，两天以内。非预约则为null
    /// </summary>
    public DateTime? AppointTime { get; set; }

    /// <summary>
    /// 新建订单时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 接单时间，可空
    /// </summary>
    public DateTime? AcceptTime { get; set; }

    /// <summary>
    /// 取货时间，可空
    /// </summary>
    public DateTime? FetchTime { get; set; }

    /// <summary>
    /// 送货完成时间，可空
    /// </summary>
    public DateTime? FinishTime { get; set; }

    /// <summary>
    /// 取消订单时间，可空
    /// </summary>
    public DateTime? CancelTime { get; set; }

    /// <summary>
    /// 根据业务订单ID查询闪送订单
    /// </summary>
    /// <param name="orderID"></param>
    /// <returns></returns>
    public static ShanSongOrder FindShanSongOrderByOrderID(string orderID)
    {
        ShanSongOrder ssOrder = null;
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdSSOrder = conn.CreateCommand())
                {

                    SqlParameter paramOrderID = cmdSSOrder.CreateParameter();
                    paramOrderID.ParameterName = "@OrderID";
                    paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramOrderID.SqlValue = orderID;
                    cmdSSOrder.Parameters.Add(paramOrderID);

                    cmdSSOrder.CommandText = "select * from ShanSongOrder where OrderID = @OrderID";

                    using (SqlDataReader sdrSSOrder = cmdSSOrder.ExecuteReader())
                    {
                        if (sdrSSOrder.Read())
                        {
                            ssOrder = new ShanSongOrder();

                            //加载对应的业务订单
                            ssOrder.ProductOrder = new ProductOrder(sdrSSOrder["OrderID"].ToString());

                            ssOrder.ID = int.Parse(sdrSSOrder["Id"].ToString());
                            ssOrder.POIID = int.Parse(sdrSSOrder["POIID"].ToString());
                            ssOrder.OrderStatus = (ShanSongOrderStatus)sdrSSOrder["OrderStatus"];
                            ssOrder.ISSOrderNo = sdrSSOrder["ISSOrderNo"] as string;

                            ssOrder.Goods = sdrSSOrder["Goods"] as string;
                            ssOrder.Weight = sdrSSOrder["Weight"] != DBNull.Value ? int.Parse(sdrSSOrder["Weight"].ToString()) : 0;
                            ssOrder.Addition = sdrSSOrder["Addition"] != DBNull.Value ? decimal.Parse(sdrSSOrder["Addition"].ToString()) : 0;
                            ssOrder.Amount = sdrSSOrder["Amount"] != DBNull.Value ? decimal.Parse(sdrSSOrder["Amount"].ToString()) : 0;
                            ssOrder.CutAmount = sdrSSOrder["CutAmount"] != DBNull.Value ? decimal.Parse(sdrSSOrder["CutAmount"].ToString()) : 0;
                            ssOrder.DeductFee = sdrSSOrder["DeductFee"] != DBNull.Value ? decimal.Parse(sdrSSOrder["DeductFee"].ToString()) : 0;
                            ssOrder.Distance = sdrSSOrder["Distance"] != DBNull.Value ? int.Parse(sdrSSOrder["Distance"].ToString()) : 0;
                            ssOrder.PickupPassword = sdrSSOrder["PickupPassword"] as string;
                            ssOrder.Remark = sdrSSOrder["Remark"] as string;

                            ssOrder.SenderName = sdrSSOrder["SenderName"] as string;
                            ssOrder.SenderMobile = sdrSSOrder["SenderMobile"] as string;
                            ssOrder.SenderCity = sdrSSOrder["SenderCity"] as string;
                            ssOrder.SenderAddr = sdrSSOrder["SenderAddr"] as string;
                            ssOrder.SenderAddrDetail = sdrSSOrder["SenderAddrDetail"] as string;
                            ssOrder.SenderLng = sdrSSOrder["SenderLng"] != DBNull.Value ? double.Parse(sdrSSOrder["SenderLng"].ToString()) : 0;
                            ssOrder.SenderLat = sdrSSOrder["SenderLat"] != DBNull.Value ? double.Parse(sdrSSOrder["SenderLat"].ToString()) : 0;
                            ssOrder.ReceiverName = sdrSSOrder["ReceiverName"] as string;
                            ssOrder.ReceiverMobile = sdrSSOrder["ReceiverMobile"] as string;
                            ssOrder.ReceiverCity = sdrSSOrder["ReceiverCity"] as string;
                            ssOrder.ReceiverAddr = sdrSSOrder["ReceiverAddr"] as string;
                            ssOrder.ReceiverAddrDetail = sdrSSOrder["ReceiverAddrDetail"] as string;
                            ssOrder.ReceiverLng = sdrSSOrder["ReceiverLng"] != DBNull.Value ? double.Parse(sdrSSOrder["ReceiverLng"].ToString()) : 0;
                            ssOrder.ReceiverLat = sdrSSOrder["ReceiverLat"] != DBNull.Value ? double.Parse(sdrSSOrder["ReceiverLat"].ToString()) : 0;

                            ssOrder.Courier = sdrSSOrder["Courier"] as string;
                            ssOrder.CourierName = sdrSSOrder["CourierName"] as string;

                            ssOrder.CreateTime = DateTime.Parse(sdrSSOrder["CreateTime"].ToString());
                            ssOrder.AcceptTime = sdrSSOrder["AcceptTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrSSOrder["AcceptTime"].ToString()) : null;
                            ssOrder.FetchTime = sdrSSOrder["FetchTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrSSOrder["FetchTime"].ToString()) : null;
                            ssOrder.FinishTime = sdrSSOrder["FinishTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrSSOrder["FinishTime"].ToString()) : null;
                            ssOrder.CancelTime = sdrSSOrder["CancelTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrSSOrder["CancelTime"].ToString()) : null;
                            ssOrder.AppointTime = sdrSSOrder["AppointTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrSSOrder["AppointTime"].ToString()) : null;

                        }
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("ShanSongOrder", ex.ToString());
            throw ex;
        }

        return ssOrder;

    }

    /// <summary>
    /// 新增闪送订单
    /// </summary>
    /// <param name="ssOrder"></param>
    public static void AddShanSongOrder(ShanSongOrder ssOrder)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdAddSSOrder = conn.CreateCommand())
                {
                    SqlParameter paramPOIID = cmdAddSSOrder.CreateParameter();
                    paramPOIID.ParameterName = "@POIID";
                    paramPOIID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramPOIID.SqlValue = ssOrder.POIID;
                    cmdAddSSOrder.Parameters.Add(paramPOIID);

                    SqlParameter paramOrderID = cmdAddSSOrder.CreateParameter();
                    paramOrderID.ParameterName = "@OrderID";
                    paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramOrderID.SqlValue = ssOrder.ProductOrder.OrderID;
                    cmdAddSSOrder.Parameters.Add(paramOrderID);

                    SqlParameter paramOrderStatus = cmdAddSSOrder.CreateParameter();
                    paramOrderStatus.ParameterName = "@OrderStatus";
                    paramOrderStatus.SqlDbType = System.Data.SqlDbType.Int;
                    paramOrderStatus.SqlValue = (int)ssOrder.OrderStatus;
                    cmdAddSSOrder.Parameters.Add(paramOrderStatus);

                    SqlParameter paramISSOrderNo = cmdAddSSOrder.CreateParameter();
                    paramISSOrderNo.ParameterName = "@ISSOrderNo";
                    paramISSOrderNo.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramISSOrderNo.SqlValue = ssOrder.ISSOrderNo;
                    cmdAddSSOrder.Parameters.Add(paramISSOrderNo);

                    SqlParameter paramGoods = cmdAddSSOrder.CreateParameter();
                    paramGoods.ParameterName = "@Goods";
                    paramGoods.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramGoods.SqlValue = ssOrder.Goods;
                    cmdAddSSOrder.Parameters.Add(paramGoods);

                    SqlParameter paramWeight = cmdAddSSOrder.CreateParameter();
                    paramWeight.ParameterName = "@Weight";
                    paramWeight.SqlDbType = System.Data.SqlDbType.Int;
                    paramWeight.SqlValue = ssOrder.Weight;
                    cmdAddSSOrder.Parameters.Add(paramWeight);

                    SqlParameter paramAddition = cmdAddSSOrder.CreateParameter();
                    paramAddition.ParameterName = "@Addition";
                    paramAddition.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramAddition.SqlValue = ssOrder.Addition;
                    cmdAddSSOrder.Parameters.Add(paramAddition);

                    SqlParameter paramAmount = cmdAddSSOrder.CreateParameter();
                    paramAmount.ParameterName = "@Amount";
                    paramAmount.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramAmount.SqlValue = ssOrder.Amount;
                    cmdAddSSOrder.Parameters.Add(paramAmount);

                    SqlParameter paramDistance = cmdAddSSOrder.CreateParameter();
                    paramDistance.ParameterName = "@Distance";
                    paramDistance.SqlDbType = System.Data.SqlDbType.Int;
                    paramDistance.SqlValue = ssOrder.Distance;
                    cmdAddSSOrder.Parameters.Add(paramDistance);

                    SqlParameter paramCutAmount = cmdAddSSOrder.CreateParameter();
                    paramCutAmount.ParameterName = "@CutAmount";
                    paramCutAmount.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramCutAmount.SqlValue = ssOrder.CutAmount;
                    cmdAddSSOrder.Parameters.Add(paramCutAmount);

                    SqlParameter paramDeductFee = cmdAddSSOrder.CreateParameter();
                    paramDeductFee.ParameterName = "@DeductFee";
                    paramDeductFee.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramDeductFee.SqlValue = ssOrder.DeductFee;
                    cmdAddSSOrder.Parameters.Add(paramDeductFee);

                    SqlParameter paramRemark = cmdAddSSOrder.CreateParameter();
                    paramRemark.ParameterName = "@Remark";
                    paramRemark.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramRemark.SqlValue = ssOrder.Remark;
                    cmdAddSSOrder.Parameters.Add(paramRemark);

                    SqlParameter paramSenderName = cmdAddSSOrder.CreateParameter();
                    paramSenderName.ParameterName = "@SenderName";
                    paramSenderName.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramSenderName.SqlValue = ssOrder.SenderName;
                    cmdAddSSOrder.Parameters.Add(paramSenderName);

                    SqlParameter paramSenderMobile = cmdAddSSOrder.CreateParameter();
                    paramSenderMobile.ParameterName = "@SenderMobile";
                    paramSenderMobile.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramSenderMobile.SqlValue = ssOrder.SenderMobile;
                    cmdAddSSOrder.Parameters.Add(paramSenderMobile);

                    SqlParameter paramSenderCity = cmdAddSSOrder.CreateParameter();
                    paramSenderCity.ParameterName = "@SenderCity";
                    paramSenderCity.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramSenderCity.SqlValue = ssOrder.SenderCity;
                    cmdAddSSOrder.Parameters.Add(paramSenderCity);

                    SqlParameter paramSenderAddr = cmdAddSSOrder.CreateParameter();
                    paramSenderAddr.ParameterName = "@SenderAddr";
                    paramSenderAddr.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramSenderAddr.SqlValue = ssOrder.SenderAddr;
                    cmdAddSSOrder.Parameters.Add(paramSenderAddr);

                    SqlParameter paramSenderAddrDetail = cmdAddSSOrder.CreateParameter();
                    paramSenderAddrDetail.ParameterName = "@SenderAddrDetail";
                    paramSenderAddrDetail.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramSenderAddrDetail.SqlValue = ssOrder.SenderAddrDetail;
                    cmdAddSSOrder.Parameters.Add(paramSenderAddrDetail);

                    SqlParameter paramSenderLng = cmdAddSSOrder.CreateParameter();
                    paramSenderLng.ParameterName = "@SenderLng";
                    paramSenderLng.SqlDbType = System.Data.SqlDbType.Real;
                    paramSenderLng.SqlValue = ssOrder.SenderLng;
                    cmdAddSSOrder.Parameters.Add(paramSenderLng);

                    SqlParameter paramSenderLat = cmdAddSSOrder.CreateParameter();
                    paramSenderLat.ParameterName = "@SenderLat";
                    paramSenderLat.SqlDbType = System.Data.SqlDbType.Real;
                    paramSenderLat.SqlValue = ssOrder.SenderLat;
                    cmdAddSSOrder.Parameters.Add(paramSenderLat);

                    SqlParameter paramReceiverName = cmdAddSSOrder.CreateParameter();
                    paramReceiverName.ParameterName = "@ReceiverName";
                    paramReceiverName.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramReceiverName.SqlValue = ssOrder.ReceiverName;
                    cmdAddSSOrder.Parameters.Add(paramReceiverName);

                    SqlParameter paramReceiverMobile = cmdAddSSOrder.CreateParameter();
                    paramReceiverMobile.ParameterName = "@ReceiverMobile";
                    paramReceiverMobile.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramReceiverMobile.SqlValue = ssOrder.ReceiverMobile;
                    cmdAddSSOrder.Parameters.Add(paramReceiverMobile);

                    SqlParameter paramReceiverCity = cmdAddSSOrder.CreateParameter();
                    paramReceiverCity.ParameterName = "@ReceiverCity";
                    paramReceiverCity.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramReceiverCity.SqlValue = ssOrder.ReceiverCity;
                    cmdAddSSOrder.Parameters.Add(paramReceiverCity);

                    SqlParameter paramReceiverAddr = cmdAddSSOrder.CreateParameter();
                    paramReceiverAddr.ParameterName = "@ReceiverAddr";
                    paramReceiverAddr.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramReceiverAddr.SqlValue = ssOrder.ReceiverAddr;
                    cmdAddSSOrder.Parameters.Add(paramReceiverAddr);

                    SqlParameter paramReceiverAddrDetail = cmdAddSSOrder.CreateParameter();
                    paramReceiverAddrDetail.ParameterName = "@ReceiverAddrDetail";
                    paramReceiverAddrDetail.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramReceiverAddrDetail.SqlValue = ssOrder.ReceiverAddrDetail;
                    cmdAddSSOrder.Parameters.Add(paramReceiverAddrDetail);

                    SqlParameter paramReceiverLng = cmdAddSSOrder.CreateParameter();
                    paramReceiverLng.ParameterName = "@ReceiverLng";
                    paramReceiverLng.SqlDbType = System.Data.SqlDbType.Real;
                    paramReceiverLng.SqlValue = ssOrder.ReceiverLng;
                    cmdAddSSOrder.Parameters.Add(paramReceiverLng);

                    SqlParameter paramReceiverLat = cmdAddSSOrder.CreateParameter();
                    paramReceiverLat.ParameterName = "@ReceiverLat";
                    paramReceiverLat.SqlDbType = System.Data.SqlDbType.Real;
                    paramReceiverLat.SqlValue = ssOrder.ReceiverLat;
                    cmdAddSSOrder.Parameters.Add(paramReceiverLat);

                    SqlParameter paramAppointTime = cmdAddSSOrder.CreateParameter();
                    paramAppointTime.ParameterName = "@AppointTime";
                    paramAppointTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramAppointTime.SqlValue = ssOrder.AppointTime;
                    cmdAddSSOrder.Parameters.Add(paramAppointTime);

                    SqlParameter paramCreateTime = cmdAddSSOrder.CreateParameter();
                    paramCreateTime.ParameterName = "@CreateTime";
                    paramCreateTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramCreateTime.SqlValue = ssOrder.CreateTime;
                    cmdAddSSOrder.Parameters.Add(paramCreateTime);

                    SqlParameter paramAcceptTime = cmdAddSSOrder.CreateParameter();
                    paramAcceptTime.ParameterName = "@AcceptTime";
                    paramAcceptTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramAcceptTime.SqlValue = ssOrder.AcceptTime;
                    cmdAddSSOrder.Parameters.Add(paramAcceptTime);

                    SqlParameter paramFetchTime = cmdAddSSOrder.CreateParameter();
                    paramFetchTime.ParameterName = "@FetchTime";
                    paramFetchTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramFetchTime.SqlValue = ssOrder.FetchTime;
                    cmdAddSSOrder.Parameters.Add(paramFetchTime);

                    SqlParameter paramFinishTime = cmdAddSSOrder.CreateParameter();
                    paramFinishTime.ParameterName = "@FinishTime";
                    paramFinishTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramFinishTime.SqlValue = ssOrder.FinishTime;
                    cmdAddSSOrder.Parameters.Add(paramFinishTime);

                    SqlParameter paramCancelTime = cmdAddSSOrder.CreateParameter();
                    paramCancelTime.ParameterName = "@CancelTime";
                    paramCancelTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramCancelTime.SqlValue = ssOrder.CancelTime;
                    cmdAddSSOrder.Parameters.Add(paramCancelTime);

                    SqlParameter paramCourier = cmdAddSSOrder.CreateParameter();
                    paramCourier.ParameterName = "@Courier";
                    paramCourier.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramCourier.SqlValue = ssOrder.Courier;
                    cmdAddSSOrder.Parameters.Add(paramCourier);

                    SqlParameter paramCourierName = cmdAddSSOrder.CreateParameter();
                    paramCourierName.ParameterName = "@CourierName";
                    paramCourierName.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramCourierName.SqlValue = ssOrder.CourierName;
                    cmdAddSSOrder.Parameters.Add(paramCourierName);

                    foreach (SqlParameter param in cmdAddSSOrder.Parameters)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                    }

                    //判断本地是否已有OrderID对应的闪送订单
                    cmdAddSSOrder.CommandText = "select Id from ShanSongOrder where OrderID = @OrderID";
                    var objID = cmdAddSSOrder.ExecuteScalar();

                    if (objID == null)
                    {
                        cmdAddSSOrder.CommandText = "insert into ShanSongOrder(OrderID, OrderStatus, ISSOrderNo, POIID, SenderName, SenderMobile, SenderMobile, SenderAddr, SenderAddrDetail, SenderLng, SenderLat, ReceiverName, ReceiverMobile, ReceiverCity, ReceiverAddr, ReceiverAddrDetail, ReceiverLng, ReceiverLat, Goods, Weight, Addition, AppointTime, CreateTime, AcceptTime, FetchTime, FinishTime, CancelTime, Remark, Amount, Distance, CutAmount, DeductFee, Courier, CourierName, PickupPassword) values(@OrderID, @OrderStatus, @ISSOrderNo, @POIID, @SenderName, @SenderMobile, @SenderMobile, @SenderAddr, @SenderAddrDetail, @SenderLng, @SenderLat, @ReceiverName, @ReceiverMobile, @ReceiverCity, @ReceiverAddr, @ReceiverAddrDetail, @ReceiverLng, @ReceiverLat, @Goods, @Weight, @Addition, @AppointTime, @CreateTime, @AcceptTime, @FetchTime, @FinishTime, @CancelTime, @Remark, @Amount, @Distance, @CutAmount, @DeductFee, @Courier, @CourierName, @PickupPassword);select SCOPE_IDENTITY() as 'NewID'";

                        var newID = cmdAddSSOrder.ExecuteScalar();

                        //新增的闪送订单ID
                        if (newID != null)
                        {
                            ssOrder.ID = int.Parse(newID.ToString());
                        }
                        else
                        {
                            throw new Exception("增加本地闪送订单错误");
                        }
                    }
                    else
                    {
                        cmdAddSSOrder.CommandText = "update ShanSongOrder set OrderStatus = @OrderStatus, ISSOrderNo = @ISSOrderNo, POIID = @POIID, SenderName= @SenderName, SenderMobile = @SenderMobile, SenderMobile = @SenderMobile, SenderAddr = @SenderAddr, SenderAddrDetail = @SenderAddrDetail, SenderLng = @SenderLng, SenderLat = @SenderLat, ReceiverName = @ReceiverName, ReceiverMobile = @ReceiverMobile, ReceiverCity = @ReceiverCity, ReceiverAddr = @ReceiverAddr, ReceiverAddrDetail = @ReceiverAddrDetail, ReceiverLng = @ReceiverLng, ReceiverLat = @ReceiverLat, Goods = @Goods, Weight = @Weight, Addition = @Addition, AppointTime = @AppointTime, CreateTime = @CreateTime, AcceptTime = @AcceptTime, FetchTime = @FetchTime, FinishTime = @FinishTime, CancelTime = @CancelTime, Remark = @Remark, Amount = @Amount, Distance = @Distance, CutAmount = @CutAmount, DeductFee = @DeductFee, Courier = @Courier, CourierName = @CourierName, PickupPassword = @PickupPassword where OrderID = @OrderID";
                        if (cmdAddSSOrder.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("更新本地闪送订单错误");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("ShanSongOrder", ex.ToString());
            throw ex;
        }
    }

    /// <summary>
    /// 更新闪送订单
    /// </summary>
    /// <param name="ssOrder"></param>
    public static void UpdateOrderStatus(ShanSongOrder ssOrder)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdUpdateOrderStatus = conn.CreateCommand())
                {

                    SqlParameter paramOrderID = cmdUpdateOrderStatus.CreateParameter();
                    paramOrderID.ParameterName = "@OrderID";
                    paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramOrderID.SqlValue = ssOrder.ProductOrder.OrderID;
                    cmdUpdateOrderStatus.Parameters.Add(paramOrderID);

                    SqlParameter paramOrderStatus = cmdUpdateOrderStatus.CreateParameter();
                    paramOrderStatus.ParameterName = "@OrderStatus";
                    paramOrderStatus.SqlDbType = System.Data.SqlDbType.Int;
                    paramOrderStatus.SqlValue = (int)ssOrder.OrderStatus;
                    cmdUpdateOrderStatus.Parameters.Add(paramOrderStatus);

                    switch (ssOrder.OrderStatus)
                    {
                        case ShanSongOrderStatus.Accepting:
                            //待接单状态
                            cmdUpdateOrderStatus.CommandText = "update ShanSongOrder set OrderStatus = @OrderStatus where OrderID = @OrderID";

                            break;
                        case ShanSongOrderStatus.Fetching:
                            //待取货状态
                            SqlParameter paramAcceptTime = cmdUpdateOrderStatus.CreateParameter();
                            paramAcceptTime.ParameterName = "@AcceptTime";
                            paramAcceptTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramAcceptTime.SqlValue = ssOrder.AcceptTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramAcceptTime);

                            SqlParameter paramCourier = cmdUpdateOrderStatus.CreateParameter();
                            paramCourier.ParameterName = "@Courier";
                            paramCourier.SqlDbType = System.Data.SqlDbType.VarChar;
                            paramCourier.SqlValue = ssOrder.Courier;
                            cmdUpdateOrderStatus.Parameters.Add(paramCourier);

                            SqlParameter paramCourierName = cmdUpdateOrderStatus.CreateParameter();
                            paramCourierName.ParameterName = "@CourierName";
                            paramCourierName.SqlDbType = System.Data.SqlDbType.NVarChar;
                            paramCourierName.SqlValue = ssOrder.CourierName;
                            cmdUpdateOrderStatus.Parameters.Add(paramCourierName);

                            SqlParameter paramPickupPassword = cmdUpdateOrderStatus.CreateParameter();
                            paramPickupPassword.ParameterName = "@PickupPassword";
                            paramPickupPassword.SqlDbType = System.Data.SqlDbType.VarChar;
                            paramPickupPassword.SqlValue = ssOrder.PickupPassword;
                            cmdUpdateOrderStatus.Parameters.Add(paramPickupPassword);

                            cmdUpdateOrderStatus.CommandText = "update ShanSongOrder set OrderStatus = @OrderStatus, AcceptTime = @AcceptTime, Courier = @Courier, CourierName = @CourierName, PickupPassword = @PickupPassword where OrderID = @OrderID";

                            break;
                        case ShanSongOrderStatus.Delivering:
                            //配送中状态
                            SqlParameter paramFetchTime = cmdUpdateOrderStatus.CreateParameter();
                            paramFetchTime.ParameterName = "@FetchTime";
                            paramFetchTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramFetchTime.SqlValue = ssOrder.FetchTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramFetchTime);

                            cmdUpdateOrderStatus.CommandText = "update ShanSongOrder set OrderStatus = @OrderStatus, FetchTime = @FetchTime where OrderID = @OrderID";

                            break;
                        case ShanSongOrderStatus.Finished:
                            //已完成状态
                            SqlParameter paramFinishTime = cmdUpdateOrderStatus.CreateParameter();
                            paramFinishTime.ParameterName = "@FinishTime";
                            paramFinishTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramFinishTime.SqlValue = ssOrder.FinishTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramFinishTime);

                            cmdUpdateOrderStatus.CommandText = "update ShanSongOrder set OrderStatus = @OrderStatus, FinishTime = @FinishTime where OrderID = @OrderID";

                            break;
                        case ShanSongOrderStatus.Cancelled:
                            //取消状态
                            SqlParameter paramCancelTime = cmdUpdateOrderStatus.CreateParameter();
                            paramCancelTime.ParameterName = "@CancelTime";
                            paramCancelTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramCancelTime.SqlValue = ssOrder.CancelTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramCancelTime);

                            SqlParameter paramDeductFee = cmdUpdateOrderStatus.CreateParameter();
                            paramDeductFee.ParameterName = "@DeductFee";
                            paramDeductFee.SqlDbType = System.Data.SqlDbType.Decimal;
                            paramDeductFee.SqlValue = ssOrder.DeductFee;
                            cmdUpdateOrderStatus.Parameters.Add(paramDeductFee);

                            cmdUpdateOrderStatus.CommandText = "update ShanSongOrder set OrderStatus = @OrderStatus, CancelTime = @CancelTime, DeductFee = @DeductFee where OrderID = @OrderID";

                            break;
                        default:
                            throw new Exception("闪送订单状态错误");
                    }

                    foreach (SqlParameter param in cmdUpdateOrderStatus.Parameters)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                    }

                    if (cmdUpdateOrderStatus.ExecuteNonQuery() != 1)
                    {
                        throw new Exception("更新闪送订单状态错误");
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("ShanSongOrder", ex.ToString());
            throw ex;
        }
    }

}

/// <summary>
/// 闪送员轨迹类
/// </summary>
public class ShanSongTrialMessage
{
    /// <summary>
    /// 闪送员位置经度
    /// </summary>
    public double Lng { get; set; }

    /// <summary>
    /// 闪送员位置纬度
    /// </summary>
    public double Lat { get; set; }

    /// <summary>
    /// 闪送员位置时间点
    /// </summary>
    public DateTime? Time { get; set; }
}

/// <summary>
/// 闪送订单状态
/// </summary>
public enum ShanSongOrderStatus
{
    /// <summary>
    /// 待抢单
    /// </summary>
    Accepting = 20,
    /// <summary>
    /// 已抢单（待取件）
    /// </summary>
    Fetching = 30,
    /// <summary>
    /// 已就位（到达寄件人地址）
    /// </summary>
    Fetched = 42,
    /// <summary>
    /// 派送中
    /// </summary>
    Delivering = 44,
    /// <summary>
    /// 已完成
    /// </summary>
    Finished = 60,
    /// <summary>
    /// 已取消
    /// </summary>
    Cancelled = 64
}