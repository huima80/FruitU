using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

/// <summary>
/// 达达物流订单类
/// </summary>
public class DaDaOrder
{
    public int ID { get; set; }

    /// <summary>
    /// 门店编号
    /// </summary>
    public string ShopNo { get; set; }

    /// <summary>
    /// 第三方订单
    /// </summary>
    public ProductOrder ProductOrder { get; set; }

    /// <summary>
    /// 订单所在城市的code
    /// https://newopen.imdada.cn/#/development/file/cityList
    /// </summary>
    public string CityCode { get; set; }

    /// <summary>
    /// 应付商家金额（单位：元）
    /// </summary>
    public decimal PayForSupplierFee { get; set; }

    /// <summary>
    /// 应收用户金额（单位：元）
    /// </summary>
    public decimal FetchFromReceiverFee { get; set; }

    /// <summary>
    /// 第三方平台补贴运费金额（单位：元）
    /// </summary>
    public decimal DeliverFee { get; set; }

    /// <summary>
    /// 小费（单位：元，精确小数点后一位）
    /// </summary>
    public decimal Tips { get; set; }

    /// <summary>
    /// 订单创建时间
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
    /// 送达时间，可空
    /// </summary>
    public DateTime? FinishTime { get; set; }

    /// <summary>
    /// 取消时间，可空
    /// </summary>
    public DateTime? CancelTime { get; set; }

    /// <summary>
    /// 每次订单状态发生变化时，达达都会回调更新订单状态，此为更新时间，可空
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 订单备注
    /// </summary>
    public string Info { get; set; }

    /// <summary>
    /// 订单商品类型：1、餐饮 2、饮 料 3、鲜花 4、票 务 5、其他 8、印刷品 9、便利店 10、学校餐饮 11、校园便利 12、生鲜 13、水果
    /// </summary>
    public int CargoType { get; set; }

    /// <summary>
    /// 订单重量（单位：Kg）
    /// </summary>
    public decimal CargoWeight { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    public decimal CargoPrice { get; set; }

    /// <summary>
    /// 订单商品数量
    /// </summary>
    public int CargoNum { get; set; }

    /// <summary>
    /// 是否需要垫付
    /// </summary>
    public bool IsPrepay { get; set; }

    /// <summary>
    /// 期望取货时间，不能超过24小时，同时，超过期望取货时间30分钟，系统将取消订单
    /// </summary>
    public DateTime ExpectedFetchTime { get; set; }

    /// <summary>
    /// 期望完成时间，可空
    /// </summary>
    public DateTime? ExpectedFinishTime { get; set; }

    /// <summary>
    /// 发票抬头
    /// </summary>
    public string InvoiceTitle { get; set; }

    /// <summary>
    /// 收货人姓名
    /// </summary>
    public string ReceiverName { get; set; }

    /// <summary>
    /// 收货人地址
    /// </summary>
    public string ReceiverAddress { get; set; }

    /// <summary>
    /// 收货人手机号
    /// </summary>
    public string ReceiverPhone { get; set; }

    /// <summary>
    /// 收货人地址维度（高德坐标系）
    /// </summary>
    public double ReceiverLat { get; set; }

    /// <summary>
    /// 收货人地址经度（高德坐标系）
    /// </summary>
    public double ReceiverLng { get; set; }

    /// <summary>
    /// 送货开箱码
    /// </summary>
    public string DeliverLockerCode { get; set; }

    /// <summary>
    /// 取货开箱码
    /// </summary>
    public string PickupLockerCode { get; set; }

    /// <summary>
    /// 配送距离(单位：米)
    /// </summary>
    public float Distance { get; set; }

    /// <summary>
    /// 运费(单位：元)
    /// </summary>
    public decimal Fee { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    public DaDaOrderStatus OrderStatus { get; set; }

    /// <summary>
    /// 订单取消原因,其他状态下默认值为空字符串
    /// </summary>
    public string CancelReason { get; set; }

    /// <summary>
    /// 达达配送员id，接单以后会传
    /// </summary>
    public int DMID { get; set; }

    /// <summary>
    /// 配送员姓名，接单以后会传
    /// </summary>
    public string DMName { get; set; }

    /// <summary>
    /// 配送员手机号，接单以后会传
    /// </summary>
    public string DMMobile { get; set; }

    /// <summary>
    /// 骑手经度
    /// </summary>
    public double TransporterLng { get; set; }

    /// <summary>
    /// 骑手纬度
    /// </summary>
    public double TransporterLat { get; set; }

    /// <summary>
    /// 订单接单后1-15分钟取消订单，会扣除相应费用补贴给接单达达(单位：元)
    /// </summary>
    public decimal DeductFee { get; set; }

    /// <summary>
    /// 订单预发布得到的平台订单编号，注意：该平台订单编号有效期为3分钟。
    /// </summary>
    public string DeliveryNo { get; set; }

    public DaDaOrder()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    /// <summary>
    /// 根据业务订单ID，查询本地达达订单
    /// </summary>
    /// <param name="orderID">业务订单ID</param>
    /// <returns>达达订单</returns>
    public static DaDaOrder FindDaDaOrderByOrderID(string orderID)
    {
        DaDaOrder dadaOrder = null;
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdDaDaOrder = conn.CreateCommand())
                {

                    SqlParameter paramOrderID = cmdDaDaOrder.CreateParameter();
                    paramOrderID.ParameterName = "@OrderID";
                    paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramOrderID.SqlValue = orderID;
                    cmdDaDaOrder.Parameters.Add(paramOrderID);

                    cmdDaDaOrder.CommandText = "select * from DaDaOrder where OrderID = @OrderID";

                    using (SqlDataReader sdrDaDaOrder = cmdDaDaOrder.ExecuteReader())
                    {
                        if (sdrDaDaOrder.Read())
                        {
                            dadaOrder = new DaDaOrder();

                            //加载对应的业务订单
                            dadaOrder.ProductOrder = new ProductOrder(sdrDaDaOrder["OrderID"].ToString());

                            dadaOrder.ID = int.Parse(sdrDaDaOrder["Id"].ToString());
                            dadaOrder.ShopNo = sdrDaDaOrder["ShopNo"] as string;
                            dadaOrder.CityCode = sdrDaDaOrder["CityCode"] as string;
                            dadaOrder.PayForSupplierFee = sdrDaDaOrder["PayForSupplierFee"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["PayForSupplierFee"].ToString()) : 0;
                            dadaOrder.FetchFromReceiverFee = sdrDaDaOrder["FetchFromReceiverFee"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["FetchFromReceiverFee"].ToString()) : 0;
                            dadaOrder.DeliverFee = sdrDaDaOrder["DeliverFee"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["DeliverFee"].ToString()) : 0;
                            dadaOrder.Tips = sdrDaDaOrder["Tips"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["Tips"].ToString()) : 0;
                            dadaOrder.CreateTime = DateTime.Parse(sdrDaDaOrder["CreateTime"].ToString());
                            dadaOrder.AcceptTime = sdrDaDaOrder["AcceptTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrDaDaOrder["AcceptTime"].ToString()) : null;
                            dadaOrder.FetchTime = sdrDaDaOrder["FetchTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrDaDaOrder["FetchTime"].ToString()) : null;
                            dadaOrder.FinishTime = sdrDaDaOrder["FinishTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrDaDaOrder["FinishTime"].ToString()) : null;
                            dadaOrder.CancelTime = sdrDaDaOrder["CancelTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrDaDaOrder["CancelTime"].ToString()) : null;
                            dadaOrder.UpdateTime = sdrDaDaOrder["UpdateTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(sdrDaDaOrder["UpdateTime"].ToString()) : null;
                            dadaOrder.Info = sdrDaDaOrder["Info"] as string;
                            dadaOrder.CargoType = sdrDaDaOrder["CargoType"] != DBNull.Value ? int.Parse(sdrDaDaOrder["CargoType"].ToString()) : 0;
                            dadaOrder.CargoWeight = sdrDaDaOrder["CargoWeight"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["CargoWeight"].ToString()) : 0;
                            dadaOrder.CargoPrice = sdrDaDaOrder["CargoPrice"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["CargoPrice"].ToString()) : 0;
                            dadaOrder.CargoNum = sdrDaDaOrder["CargoNum"] != DBNull.Value ? int.Parse(sdrDaDaOrder["CargoNum"].ToString()) : 0;
                            dadaOrder.IsPrepay = bool.Parse(sdrDaDaOrder["IsPrepay"].ToString());
                            dadaOrder.ExpectedFetchTime = DateTime.Parse(sdrDaDaOrder["ExpectedFetchTime"].ToString());
                            dadaOrder.ExpectedFinishTime = sdrDaDaOrder["ExpectedFinishTime"] != DBNull.Value ? (DateTime?)(sdrDaDaOrder["ExpectedFinishTime"]) : null;
                            dadaOrder.InvoiceTitle = sdrDaDaOrder["InvoiceTitle"] as string;
                            dadaOrder.ReceiverName = sdrDaDaOrder["ReceiverName"] as string;
                            dadaOrder.ReceiverAddress = sdrDaDaOrder["ReceiverAddress"] as string;
                            dadaOrder.ReceiverPhone = sdrDaDaOrder["ReceiverPhone"] as string;
                            dadaOrder.ReceiverLat = sdrDaDaOrder["ReceiverLat"] != DBNull.Value ? double.Parse(sdrDaDaOrder["ReceiverLat"].ToString()) : 0;
                            dadaOrder.ReceiverLng = sdrDaDaOrder["ReceiverLng"] != DBNull.Value ? double.Parse(sdrDaDaOrder["ReceiverLng"].ToString()) : 0;
                            dadaOrder.DeliverLockerCode = sdrDaDaOrder["DeliverLockerCode"] as string;
                            dadaOrder.PickupLockerCode = sdrDaDaOrder["PickupLockerCode"] as string;
                            dadaOrder.Distance = sdrDaDaOrder["Distance"] != DBNull.Value ? float.Parse(sdrDaDaOrder["Distance"].ToString()) : 0;
                            dadaOrder.Fee = sdrDaDaOrder["Fee"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["Fee"].ToString()) : 0;
                            dadaOrder.OrderStatus = sdrDaDaOrder["OrderStatus"] != DBNull.Value ? (DaDaOrderStatus)sdrDaDaOrder["OrderStatus"] : DaDaOrderStatus.Accepting;
                            dadaOrder.CancelReason = sdrDaDaOrder["CancelReason"] as string;
                            dadaOrder.DMID = sdrDaDaOrder["DMID"] != DBNull.Value ? int.Parse(sdrDaDaOrder["DMID"].ToString()) : 0;
                            dadaOrder.DMName = sdrDaDaOrder["DMName"] as string;
                            dadaOrder.DMMobile = sdrDaDaOrder["DMMobile"] as string;
                            dadaOrder.DeductFee = sdrDaDaOrder["DeductFee"] != DBNull.Value ? decimal.Parse(sdrDaDaOrder["DeductFee"].ToString()) : 0;
                            dadaOrder.DeliveryNo = sdrDaDaOrder["DeliveryNo"] as string;
                        }
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("DaDaOrder", ex.ToString());
            throw ex;
        }

        return dadaOrder;

    }

    /// <summary>
    /// 本地新增达达订单入库
    /// </summary>
    /// <param name="dadaOrder"></param>
    public static void AddDaDaOrder(DaDaOrder dadaOrder)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdAddDaDaOrder = conn.CreateCommand())
                {
                    SqlParameter paramShopNo = cmdAddDaDaOrder.CreateParameter();
                    paramShopNo.ParameterName = "@ShopNo";
                    paramShopNo.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramShopNo.SqlValue = dadaOrder.ShopNo;
                    cmdAddDaDaOrder.Parameters.Add(paramShopNo);

                    SqlParameter paramOrderID = cmdAddDaDaOrder.CreateParameter();
                    paramOrderID.ParameterName = "@OrderID";
                    paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramOrderID.SqlValue = dadaOrder.ProductOrder.OrderID;
                    cmdAddDaDaOrder.Parameters.Add(paramOrderID);

                    SqlParameter paramCityCode = cmdAddDaDaOrder.CreateParameter();
                    paramCityCode.ParameterName = "@CityCode";
                    paramCityCode.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramCityCode.SqlValue = dadaOrder.CityCode;
                    cmdAddDaDaOrder.Parameters.Add(paramCityCode);

                    SqlParameter paramPayForSupplierFee = cmdAddDaDaOrder.CreateParameter();
                    paramPayForSupplierFee.ParameterName = "@PayForSupplierFee";
                    paramPayForSupplierFee.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramPayForSupplierFee.SqlValue = dadaOrder.PayForSupplierFee;
                    cmdAddDaDaOrder.Parameters.Add(paramPayForSupplierFee);

                    SqlParameter paramFetchFromReceiverFee = cmdAddDaDaOrder.CreateParameter();
                    paramFetchFromReceiverFee.ParameterName = "@FetchFromReceiverFee";
                    paramFetchFromReceiverFee.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramFetchFromReceiverFee.SqlValue = dadaOrder.FetchFromReceiverFee;
                    cmdAddDaDaOrder.Parameters.Add(paramFetchFromReceiverFee);

                    SqlParameter paramDeliverFee = cmdAddDaDaOrder.CreateParameter();
                    paramDeliverFee.ParameterName = "@DeliverFee";
                    paramDeliverFee.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramDeliverFee.SqlValue = dadaOrder.DeliverFee;
                    cmdAddDaDaOrder.Parameters.Add(paramDeliverFee);

                    SqlParameter paramTips = cmdAddDaDaOrder.CreateParameter();
                    paramTips.ParameterName = "@Tips";
                    paramTips.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramTips.SqlValue = dadaOrder.Tips;
                    cmdAddDaDaOrder.Parameters.Add(paramTips);

                    SqlParameter paramCreateTime = cmdAddDaDaOrder.CreateParameter();
                    paramCreateTime.ParameterName = "@CreateTime";
                    paramCreateTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramCreateTime.SqlValue = dadaOrder.CreateTime;
                    cmdAddDaDaOrder.Parameters.Add(paramCreateTime);

                    SqlParameter paramInfo = cmdAddDaDaOrder.CreateParameter();
                    paramInfo.ParameterName = "@Info";
                    paramInfo.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramInfo.SqlValue = dadaOrder.Info;
                    cmdAddDaDaOrder.Parameters.Add(paramInfo);

                    SqlParameter paramCargoType = cmdAddDaDaOrder.CreateParameter();
                    paramCargoType.ParameterName = "@CargoType";
                    paramCargoType.SqlDbType = System.Data.SqlDbType.Int;
                    paramCargoType.SqlValue = dadaOrder.CargoType;
                    cmdAddDaDaOrder.Parameters.Add(paramCargoType);

                    SqlParameter paramCargoWeight = cmdAddDaDaOrder.CreateParameter();
                    paramCargoWeight.ParameterName = "@CargoWeight";
                    paramCargoWeight.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramCargoWeight.SqlValue = dadaOrder.CargoWeight;
                    cmdAddDaDaOrder.Parameters.Add(paramCargoWeight);

                    SqlParameter paramCargoPrice = cmdAddDaDaOrder.CreateParameter();
                    paramCargoPrice.ParameterName = "@CargoPrice";
                    paramCargoPrice.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramCargoPrice.SqlValue = dadaOrder.CargoPrice;
                    cmdAddDaDaOrder.Parameters.Add(paramCargoPrice);

                    SqlParameter paramCargoNum = cmdAddDaDaOrder.CreateParameter();
                    paramCargoNum.ParameterName = "@CargoNum";
                    paramCargoNum.SqlDbType = System.Data.SqlDbType.Int;
                    paramCargoNum.SqlValue = dadaOrder.CargoNum;
                    cmdAddDaDaOrder.Parameters.Add(paramCargoNum);

                    SqlParameter paramIsPrepay = cmdAddDaDaOrder.CreateParameter();
                    paramIsPrepay.ParameterName = "@IsPrepay";
                    paramIsPrepay.SqlDbType = System.Data.SqlDbType.Bit;
                    paramIsPrepay.SqlValue = dadaOrder.IsPrepay;
                    cmdAddDaDaOrder.Parameters.Add(paramIsPrepay);

                    SqlParameter paramExpectedFetchTime = cmdAddDaDaOrder.CreateParameter();
                    paramExpectedFetchTime.ParameterName = "@ExpectedFetchTime";
                    paramExpectedFetchTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramExpectedFetchTime.SqlValue = dadaOrder.ExpectedFetchTime;
                    cmdAddDaDaOrder.Parameters.Add(paramExpectedFetchTime);

                    SqlParameter paramExpectedFinishTime = cmdAddDaDaOrder.CreateParameter();
                    paramExpectedFinishTime.ParameterName = "@ExpectedFinishTime";
                    paramExpectedFinishTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramExpectedFinishTime.SqlValue = dadaOrder.ExpectedFinishTime;
                    cmdAddDaDaOrder.Parameters.Add(paramExpectedFinishTime);

                    SqlParameter paramInvoiceTitle = cmdAddDaDaOrder.CreateParameter();
                    paramInvoiceTitle.ParameterName = "@InvoiceTitle";
                    paramInvoiceTitle.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramInvoiceTitle.SqlValue = dadaOrder.InvoiceTitle;
                    cmdAddDaDaOrder.Parameters.Add(paramInvoiceTitle);

                    SqlParameter paramReceiverName = cmdAddDaDaOrder.CreateParameter();
                    paramReceiverName.ParameterName = "@ReceiverName";
                    paramReceiverName.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramReceiverName.SqlValue = dadaOrder.ReceiverName;
                    cmdAddDaDaOrder.Parameters.Add(paramReceiverName);

                    SqlParameter paramReceiverAddress = cmdAddDaDaOrder.CreateParameter();
                    paramReceiverAddress.ParameterName = "@ReceiverAddress";
                    paramReceiverAddress.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramReceiverAddress.SqlValue = dadaOrder.ReceiverAddress;
                    cmdAddDaDaOrder.Parameters.Add(paramReceiverAddress);

                    SqlParameter paramReceiverPhone = cmdAddDaDaOrder.CreateParameter();
                    paramReceiverPhone.ParameterName = "@ReceiverPhone";
                    paramReceiverPhone.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramReceiverPhone.SqlValue = dadaOrder.ReceiverPhone;
                    cmdAddDaDaOrder.Parameters.Add(paramReceiverPhone);

                    SqlParameter paramReceiverLat = cmdAddDaDaOrder.CreateParameter();
                    paramReceiverLat.ParameterName = "@ReceiverLat";
                    paramReceiverLat.SqlDbType = System.Data.SqlDbType.Real;
                    paramReceiverLat.SqlValue = dadaOrder.ReceiverLat;
                    cmdAddDaDaOrder.Parameters.Add(paramReceiverLat);

                    SqlParameter paramReceiverLng = cmdAddDaDaOrder.CreateParameter();
                    paramReceiverLng.ParameterName = "@ReceiverLng";
                    paramReceiverLng.SqlDbType = System.Data.SqlDbType.Real;
                    paramReceiverLng.SqlValue = dadaOrder.ReceiverLng;
                    cmdAddDaDaOrder.Parameters.Add(paramReceiverLng);

                    SqlParameter paramDeliverLockerCode = cmdAddDaDaOrder.CreateParameter();
                    paramDeliverLockerCode.ParameterName = "@DeliverLockerCode";
                    paramDeliverLockerCode.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramDeliverLockerCode.SqlValue = dadaOrder.DeliverLockerCode;
                    cmdAddDaDaOrder.Parameters.Add(paramDeliverLockerCode);

                    SqlParameter paramPickupLockerCode = cmdAddDaDaOrder.CreateParameter();
                    paramPickupLockerCode.ParameterName = "@PickupLockerCode";
                    paramPickupLockerCode.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramPickupLockerCode.SqlValue = dadaOrder.PickupLockerCode;
                    cmdAddDaDaOrder.Parameters.Add(paramPickupLockerCode);

                    SqlParameter paramDistance = cmdAddDaDaOrder.CreateParameter();
                    paramDistance.ParameterName = "@Distance";
                    paramDistance.SqlDbType = System.Data.SqlDbType.Float;
                    paramDistance.SqlValue = dadaOrder.Distance;
                    cmdAddDaDaOrder.Parameters.Add(paramDistance);

                    SqlParameter paramFee = cmdAddDaDaOrder.CreateParameter();
                    paramFee.ParameterName = "@Fee";
                    paramFee.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramFee.SqlValue = dadaOrder.Fee;
                    cmdAddDaDaOrder.Parameters.Add(paramFee);

                    SqlParameter paramDeliveryNo = cmdAddDaDaOrder.CreateParameter();
                    paramDeliveryNo.ParameterName = "@DeliveryNo";
                    paramDeliveryNo.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramDeliveryNo.SqlValue = dadaOrder.DeliveryNo;
                    cmdAddDaDaOrder.Parameters.Add(paramDeliveryNo);

                    SqlParameter paramOrderStatus = cmdAddDaDaOrder.CreateParameter();
                    paramOrderStatus.ParameterName = "@OrderStatus";
                    paramOrderStatus.SqlDbType = System.Data.SqlDbType.Int;
                    paramOrderStatus.SqlValue = (int)dadaOrder.OrderStatus;
                    cmdAddDaDaOrder.Parameters.Add(paramOrderStatus);

                    foreach (SqlParameter param in cmdAddDaDaOrder.Parameters)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                    }

                    //判断本地是否已有OrderID对应的达达订单
                    cmdAddDaDaOrder.CommandText = "select Id from DaDaOrder where OrderID = @OrderID";
                    var objID = cmdAddDaDaOrder.ExecuteScalar();

                    if(objID == null)
                    {
                        cmdAddDaDaOrder.CommandText = "insert into DaDaOrder(ShopNo, OrderID, CityCode, PayForSupplierFee, FetchFromReceiverFee, DeliverFee, Tips, CreateTime, Info, CargoType, CargoWeight, CargoPrice, CargoNum, IsPrepay, ExpectedFetchTime, ExpectedFinishTime, InvoiceTitle, ReceiverName, ReceiverAddress, ReceiverPhone, ReceiverLat, ReceiverLng, DeliverLockerCode, PickupLockerCode, Distance, Fee, DeliveryNo, OrderStatus) values(@ShopNo, @OrderID, @CityCode, @PayForSupplierFee, @FetchFromReceiverFee, @DeliverFee, @Tips, @CreateTime, @Info, @CargoType, @CargoWeight, @CargoPrice, @CargoNum, @IsPrepay, @ExpectedFetchTime, @ExpectedFinishTime, @InvoiceTitle, @ReceiverName, @ReceiverAddress, @ReceiverPhone, @ReceiverLat, @ReceiverLng, @DeliverLockerCode, @PickupLockerCode, @Distance, @Fee, @DeliveryNo, @OrderStatus);select SCOPE_IDENTITY() as 'NewID'";

                        var newID = cmdAddDaDaOrder.ExecuteScalar();

                        //新增的达达订单ID
                        if (newID != null)
                        {
                            dadaOrder.ID = int.Parse(newID.ToString());
                        }
                        else
                        {
                            throw new Exception("增加本地达达订单错误");
                        }
                    }
                    else
                    {
                        cmdAddDaDaOrder.CommandText = "update DaDaOrder set ShopNo = @ShopNo, CityCode = @CityCode, PayForSupplierFee = @PayForSupplierFee, FetchFromReceiverFee = @FetchFromReceiverFee, DeliverFee = @DeliverFee, Tips = @Tips, CreateTime = @CreateTime, Info = @Info, CargoType = @CargoType, CargoWeight = @CargoWeight, CargoPrice = @CargoPrice, CargoNum = @CargoNum, IsPrepay = @IsPrepay, ExpectedFetchTime = @ExpectedFetchTime, ExpectedFinishTime = @ExpectedFinishTime, InvoiceTitle = @InvoiceTitle, ReceiverName = @ReceiverName, ReceiverAddress = @ReceiverAddress, ReceiverPhone = @ReceiverPhone, ReceiverLat = @ReceiverLat, ReceiverLng = @ReceiverLng, DeliverLockerCode = @DeliverLockerCode, PickupLockerCode = @PickupLockerCode, Distance = @Distance, Fee = @Fee, DeliveryNo = @DeliveryNo, OrderStatus = @OrderStatus where OrderID = @OrderID";
                        if (cmdAddDaDaOrder.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("更新本地达达订单错误");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("DaDaOrder", ex.ToString());
            throw ex;
        }
    }

    /// <summary>
    /// 设置本地达达订单状态为已发布
    /// </summary>
    /// <param name="deliveryNo">平台订单编号</param>
    public static void ReleaseOrder(string deliveryNo)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdReleaseDaDaOrder = conn.CreateCommand())
                {
                    SqlParameter paramDeliveryNo = cmdReleaseDaDaOrder.CreateParameter();
                    paramDeliveryNo.ParameterName = "@DeliveryNo";
                    paramDeliveryNo.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramDeliveryNo.SqlValue = deliveryNo;
                    cmdReleaseDaDaOrder.Parameters.Add(paramDeliveryNo);

                    SqlParameter paramOrderStatus = cmdReleaseDaDaOrder.CreateParameter();
                    paramOrderStatus.ParameterName = "@OrderStatus";
                    paramOrderStatus.SqlDbType = System.Data.SqlDbType.Int;
                    paramOrderStatus.SqlValue = (int)DaDaOrderStatus.Accepting;
                    cmdReleaseDaDaOrder.Parameters.Add(paramOrderStatus);

                    foreach (SqlParameter param in cmdReleaseDaDaOrder.Parameters)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                    }

                    cmdReleaseDaDaOrder.CommandText = "update DaDaOrder set OrderStatus = @OrderStatus where DeliveryNo = @DeliveryNo";

                    if (cmdReleaseDaDaOrder.ExecuteNonQuery() != 1)
                    {
                        throw new Exception("发布达达订单错误");
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("DaDaOrder", ex.ToString());
            throw ex;
        }
    }

    /// <summary>
    /// 更新本地达达订单状态
    /// </summary>
    /// <param name="dadaOrder"></param>
    public static void UpdateOrderStatus(DaDaOrder dadaOrder)
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
                    paramOrderID.SqlValue = dadaOrder.ProductOrder.OrderID;
                    cmdUpdateOrderStatus.Parameters.Add(paramOrderID);

                    SqlParameter paramOrderStatus = cmdUpdateOrderStatus.CreateParameter();
                    paramOrderStatus.ParameterName = "@OrderStatus";
                    paramOrderStatus.SqlDbType = System.Data.SqlDbType.Int;
                    paramOrderStatus.SqlValue = (int)dadaOrder.OrderStatus;
                    cmdUpdateOrderStatus.Parameters.Add(paramOrderStatus);

                    SqlParameter paramDMID = cmdUpdateOrderStatus.CreateParameter();
                    paramDMID.ParameterName = "@DMID";
                    paramDMID.SqlDbType = System.Data.SqlDbType.Int;
                    paramDMID.SqlValue = dadaOrder.DMID;
                    cmdUpdateOrderStatus.Parameters.Add(paramDMID);

                    SqlParameter paramDMName = cmdUpdateOrderStatus.CreateParameter();
                    paramDMName.ParameterName = "@DMName";
                    paramDMName.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramDMName.SqlValue = dadaOrder.DMName;
                    cmdUpdateOrderStatus.Parameters.Add(paramDMName);

                    SqlParameter paramDMMobile = cmdUpdateOrderStatus.CreateParameter();
                    paramDMMobile.ParameterName = "@DMMobile";
                    paramDMMobile.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramDMMobile.SqlValue = dadaOrder.DMMobile;
                    cmdUpdateOrderStatus.Parameters.Add(paramDMMobile);

                    switch (dadaOrder.OrderStatus)
                    {
                        case DaDaOrderStatus.Accepting:
                            //待接单状态
                            cmdUpdateOrderStatus.CommandText = "update DaDaOrder set OrderStatus = @OrderStatus, DMID = @DMID, DMName = @DMName, DMMobile = @DMMobile where OrderID = @OrderID";

                            break;
                        case DaDaOrderStatus.Fetching:
                            //待取货状态
                            SqlParameter paramAcceptTime = cmdUpdateOrderStatus.CreateParameter();
                            paramAcceptTime.ParameterName = "@AcceptTime";
                            paramAcceptTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramAcceptTime.SqlValue = dadaOrder.AcceptTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramAcceptTime);

                            cmdUpdateOrderStatus.CommandText = "update DaDaOrder set OrderStatus = @OrderStatus, DMID = @DMID, DMName = @DMName, DMMobile = @DMMobile, AcceptTime = @AcceptTime where OrderID = @OrderID";

                            break;
                        case DaDaOrderStatus.Delivering:
                            //配送中状态
                            SqlParameter paramFetchTime = cmdUpdateOrderStatus.CreateParameter();
                            paramFetchTime.ParameterName = "@FetchTime";
                            paramFetchTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramFetchTime.SqlValue = dadaOrder.FetchTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramFetchTime);

                            cmdUpdateOrderStatus.CommandText = "update DaDaOrder set OrderStatus = @OrderStatus, DMID = @DMID, DMName = @DMName, DMMobile = @DMMobile, FetchTime = @FetchTime where OrderID = @OrderID";

                            break;
                        case DaDaOrderStatus.Finished:
                            //已完成状态
                            SqlParameter paramFinishTime = cmdUpdateOrderStatus.CreateParameter();
                            paramFinishTime.ParameterName = "@FinishTime";
                            paramFinishTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramFinishTime.SqlValue = dadaOrder.FinishTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramFinishTime);

                            cmdUpdateOrderStatus.CommandText = "update DaDaOrder set OrderStatus = @OrderStatus, DMID = @DMID, DMName = @DMName, DMMobile = @DMMobile, FinishTime = @FinishTime where OrderID = @OrderID";

                            break;
                        case DaDaOrderStatus.Cancelled:
                            //取消状态
                            SqlParameter paramCancelTime = cmdUpdateOrderStatus.CreateParameter();
                            paramCancelTime.ParameterName = "@CancelTime";
                            paramCancelTime.SqlDbType = System.Data.SqlDbType.DateTime;
                            paramCancelTime.SqlValue = dadaOrder.CancelTime;
                            cmdUpdateOrderStatus.Parameters.Add(paramCancelTime);

                            SqlParameter paramCancelReason = cmdUpdateOrderStatus.CreateParameter();
                            paramCancelReason.ParameterName = "@CancelReason";
                            paramCancelReason.SqlDbType = System.Data.SqlDbType.NVarChar;
                            paramCancelReason.SqlValue = dadaOrder.CancelReason;
                            cmdUpdateOrderStatus.Parameters.Add(paramCancelReason);

                            cmdUpdateOrderStatus.CommandText = "update DaDaOrder set OrderStatus = @OrderStatus, DMID = @DMID, DMName = @DMName, DMMobile = @DMMobile, CancelTime = @CancelTime, CancelReason = @CancelReason where OrderID = @OrderID";

                            break;
                        default:
                            throw new Exception("达达订单状态错误");
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
                        throw new Exception("更新达达订单状态错误");
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("DaDaOrder", ex.ToString());
            throw ex;
        }
    }

    /// <summary>
    /// 增加本地达达订单小费
    /// </summary>
    /// <param name="orderID">订单ID</param>
    /// <param name="addTips">增加的小费金额，必须大于上次的金额</param>
    public static void AddTips(string orderID, decimal addTips)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdAddTips = conn.CreateCommand())
                {
                    SqlParameter paramOrderID = cmdAddTips.CreateParameter();
                    paramOrderID.ParameterName = "@OrderID";
                    paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramOrderID.SqlValue = orderID;
                    cmdAddTips.Parameters.Add(paramOrderID);

                    foreach (SqlParameter param in cmdAddTips.Parameters)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                    }

                    //获取上次的小费金额
                    cmdAddTips.CommandText = "select Tips from DaDaOrder where OrderID = @OrderID";
                    var objLastTips = cmdAddTips.ExecuteScalar();

                    decimal lastTips;
                    if(objLastTips == null || !(objLastTips is decimal))
                    {
                        lastTips = 0;
                    }
                    else
                    {
                        lastTips = (decimal)objLastTips;
                    }

                    //如果增加的金额大于上次的金额则写入库，否则报错
                    if (addTips > lastTips)
                    {
                        SqlParameter paramOrderStatus = cmdAddTips.CreateParameter();
                        paramOrderStatus.ParameterName = "@Tips";
                        paramOrderStatus.SqlDbType = System.Data.SqlDbType.Decimal;
                        paramOrderStatus.SqlValue = addTips;
                        cmdAddTips.Parameters.Add(paramOrderStatus);

                        cmdAddTips.CommandText = "update DaDaOrder set Tips = @Tips where OrderID = @OrderID";

                        if (cmdAddTips.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("增加小费错误");
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("增加的小费金额必须大于上次的金额：{0}元", lastTips));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("DaDaOrder", ex.ToString());
            throw ex;
        }
    }

    /// <summary>
    /// 取消本地达达订单
    /// </summary>
    /// <param name="dadaOrder"></param>
    public static void CancelOrder(DaDaOrder dadaOrder)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdCancelOrder = conn.CreateCommand())
                {
                    SqlParameter paramOrderID = cmdCancelOrder.CreateParameter();
                    paramOrderID.ParameterName = "@OrderID";
                    paramOrderID.SqlDbType = System.Data.SqlDbType.VarChar;
                    paramOrderID.SqlValue = dadaOrder.ProductOrder.OrderID;
                    cmdCancelOrder.Parameters.Add(paramOrderID);

                    SqlParameter paramOrderStatus = cmdCancelOrder.CreateParameter();
                    paramOrderStatus.ParameterName = "@OrderStatus";
                    paramOrderStatus.SqlDbType = System.Data.SqlDbType.Int;
                    paramOrderStatus.SqlValue = (int)DaDaOrderStatus.Cancelled;
                    cmdCancelOrder.Parameters.Add(paramOrderStatus);

                    SqlParameter paramCancelReason = cmdCancelOrder.CreateParameter();
                    paramCancelReason.ParameterName = "@CancelReason";
                    paramCancelReason.SqlDbType = System.Data.SqlDbType.NVarChar;
                    paramCancelReason.SqlValue = dadaOrder.CancelReason;
                    cmdCancelOrder.Parameters.Add(paramCancelReason);

                    SqlParameter paramDeductFee = cmdCancelOrder.CreateParameter();
                    paramDeductFee.ParameterName = "@DeductFee";
                    paramDeductFee.SqlDbType = System.Data.SqlDbType.Decimal;
                    paramDeductFee.SqlValue = dadaOrder.DeductFee;
                    cmdCancelOrder.Parameters.Add(paramDeductFee);

                    SqlParameter paramCancelTime = cmdCancelOrder.CreateParameter();
                    paramCancelTime.ParameterName = "@CancelTime";
                    paramCancelTime.SqlDbType = System.Data.SqlDbType.DateTime;
                    paramCancelTime.SqlValue = DateTime.Now;
                    cmdCancelOrder.Parameters.Add(paramCancelTime);

                    foreach (SqlParameter param in cmdCancelOrder.Parameters)
                    {
                        if (param.Value == null)
                        {
                            param.Value = DBNull.Value;
                        }
                    }

                    cmdCancelOrder.CommandText = "update DaDaOrder set CancelReason = @CancelReason,DeductFee = @DeductFee,OrderStatus = @OrderStatus,CancelTime = @CancelTime where OrderID = @OrderID";

                    if (cmdCancelOrder.ExecuteNonQuery() != 1)
                    {
                        throw new Exception("取消达达订单错误");
                    }

                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("DaDaOrder", ex.ToString());
            throw ex;
        }
    }


}

/// <summary>
/// 达达订单状态枚举类
/// </summary>
public enum DaDaOrderStatus
{
    /// <summary>
    /// 预发布
    /// </summary>
    PreRelease = 0,
    /// <summary>
    /// 待接单
    /// </summary>
    Accepting = 1,
    /// <summary>
    /// 待取货
    /// </summary>
    Fetching = 2,
    /// <summary>
    /// 配送中
    /// </summary>
    Delivering = 3,
    /// <summary>
    /// 已完成
    /// </summary>
    Finished = 4,
    /// <summary>
    /// 已取消：包括配送员取消、商户取消、客服取消、系统取消（待接单、待取货、配送中的订单，3天后自动取消）
    /// </summary>
    Cancelled = 5,
    /// <summary>
    /// 已过期：包括发单后30分钟无人接单，自动过期
    /// </summary>
    Expired = 7
}
