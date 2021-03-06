﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using LitJson;

/// <summary>
/// Fruit 的摘要说明
/// </summary>
public class Fruit : IComparable<Fruit> 
{
    /// <summary>
    /// 产品ID
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// 产品类别
    /// </summary>
    public Category Category { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string FruitName { get; set; }

    /// <summary>
    /// 产品价格
    /// </summary>
    public decimal FruitPrice { get; set; }

    /// <summary>
    /// 产品价格单位
    /// </summary>
    public string FruitUnit { get; set; }

    /// <summary>
    /// 产品图片
    /// </summary>
    public List<FruitImg> FruitImgList { get; set; }

    /// <summary>
    /// 产品描述
    /// </summary>
    public string FruitDesc { get; set; }

    /// <summary>
    /// 库存数量
    /// </summary>
    public int InventoryQty { get; set; }

    /// <summary>
    /// 是否上架
    /// </summary>
    public bool OnSale { get; set; }

    /// <summary>
    /// 是否置顶
    /// </summary>
    public bool IsSticky { get; set; }

    /// <summary>
    /// 商品展示优先级
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 商品包含的团购列表
    /// </summary>
    public List<GroupPurchase> GroupPurchaseList { get; set; }

    /// <summary>
    /// 当前有效的团购，在有效期内最新的一个团购
    /// </summary>
    public GroupPurchase ActiveGroupPurchase
    {
        get
        {
            if (GroupPurchaseList != null)
            {
                DateTime nowTime = DateTime.Now;
                GroupPurchaseList.Sort();
                GroupPurchase activeGroupPurchase = GroupPurchaseList.Find(groupPurchase => nowTime >= groupPurchase.StartDate && nowTime <= groupPurchase.EndDate);
                return activeGroupPurchase;
            }
            else
            {
                return null;
            }
        }
    }

    public Fruit()
    {
        this.Category = new Category();
        this.FruitImgList = new List<FruitImg>();
        this.GroupPurchaseList = new List<GroupPurchase>();
    }

    public Fruit(int id, string fruitName, Category category, decimal fruitPrice, string fruitUnit, List<FruitImg> fruitImgList, string fruitDesc, int inventoryQty, bool onSale, bool isSticky, int priority)
    {
        this.ID = id;
        this.Category = category;
        this.FruitName = fruitName;
        this.FruitPrice = fruitPrice;
        this.FruitUnit = fruitUnit;
        this.FruitImgList = fruitImgList;
        this.FruitDesc = fruitDesc;
        this.InventoryQty = inventoryQty;
        this.OnSale = onSale;
        this.IsSticky = isSticky;
        this.Priority = priority;
    }

    /// <summary>
    /// 商品库存量报警事件
    /// </summary>
    public event EventHandler InventoryWarn;

    /// <summary>
    /// 同步触发商品库存量报警事件
    /// </summary>
    public void OnInventoryWarn()
    {
        if (this.InventoryWarn != null)
        {
            this.InventoryWarn(this, new EventArgs());
        }
    }

    /// <summary>
    /// 异步触发商品库存量报警事件
    /// </summary>
    public void OnInventoryWarnAsyn()
    {
        //商品库存量报警事件异步回调函数，不阻塞主流程
        if (this.InventoryWarn != null)
        {
            IAsyncResult ar = this.InventoryWarn.BeginInvoke(this, new EventArgs(), InventoryWarnComplete, this.InventoryWarn);
        }
    }

    /// <summary>
    /// 事件完成异步回调
    /// </summary>
    /// <param name="ar"></param>
    private void InventoryWarnComplete(IAsyncResult ar)
    {
        if (ar != null)
        {
            (ar.AsyncState as EventHandler).EndInvoke(ar);
        }
    }

    /// <summary>
    /// 根据类别ID查询所有的商品
    /// </summary>
    /// <param name="categoryID"></param>
    /// <returns></returns>
    public static List<Fruit> FindFruitByCategoryID(int categoryID)
    {
        List<Fruit> fruitList;
        int topSellingIDWeekly, topSellingIDMonthly;

        try
        {
            fruitList = FindFruitByCategoryID(categoryID.ToString(), out topSellingIDWeekly, out topSellingIDMonthly);
        }
        catch (Exception ex)
        {
            Log.Error("查询指定水果", ex.ToString());
            throw ex;
        }

        return fruitList;

    }

    /// <summary>
    /// 按商品类别查询商品，并返回周/月销量最高的商品ID
    /// </summary>
    /// <param name="categoryID"></param>
    /// <param name="topSellingIDWeekly"></param>
    /// <param name="topSellingIDMonthly"></param>
    /// <returns></returns>
    public static List<Fruit> FindFruitByCategoryID(string categoryID, out int topSellingIDWeekly, out int topSellingIDMonthly)
    {
        List<Fruit> prodList = new List<Fruit>();
        Fruit product;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdFruit = conn.CreateCommand())
                    {
                        cmdFruit.CommandText = "spProductQuery";
                        cmdFruit.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramCategoryID = cmdFruit.CreateParameter();
                        paramCategoryID.ParameterName = "@CategoryID";
                        paramCategoryID.SqlDbType = SqlDbType.VarChar;
                        paramCategoryID.Size = 500;
                        paramCategoryID.Direction = ParameterDirection.Input;
                        paramCategoryID.SqlValue = categoryID;
                        cmdFruit.Parameters.Add(paramCategoryID);

                        SqlParameter paramTopSellingIDWeekly = cmdFruit.CreateParameter();
                        paramTopSellingIDWeekly.ParameterName = "@TopSellingIDWeekly";
                        paramTopSellingIDWeekly.SqlDbType = SqlDbType.Int;
                        paramTopSellingIDWeekly.Direction = ParameterDirection.Output;
                        cmdFruit.Parameters.Add(paramTopSellingIDWeekly);

                        SqlParameter paramTopSellingIDMonthly = cmdFruit.CreateParameter();
                        paramTopSellingIDMonthly.ParameterName = "@TopSellingIDMonthly";
                        paramTopSellingIDMonthly.SqlDbType = SqlDbType.Int;
                        paramTopSellingIDMonthly.Direction = ParameterDirection.Output;
                        cmdFruit.Parameters.Add(paramTopSellingIDMonthly);

                        using (SqlDataReader sdrFruit = cmdFruit.ExecuteReader())
                        {
                            while (sdrFruit.Read())
                            {
                                product = new Fruit();

                                product.ID = int.Parse(sdrFruit["Id"].ToString());
                                product.FruitName = sdrFruit["ProductName"].ToString();
                                product.FruitPrice = decimal.Parse(sdrFruit["ProductPrice"].ToString());
                                product.FruitUnit = sdrFruit["ProductUnit"].ToString();
                                product.InventoryQty = int.Parse(sdrFruit["InventoryQty"].ToString());
                                product.OnSale = bool.Parse(sdrFruit["ProductOnSale"].ToString());
                                product.FruitDesc = sdrFruit["ProductDesc"].ToString();
                                product.IsSticky = bool.Parse(sdrFruit["IsSticky"].ToString());
                                product.Priority = int.Parse(sdrFruit["Priority"].ToString());

                                //fruit包含的团购信息，使用已有的数据库链接对象，但不加载团购活动和活动成员，避免前端页面加载数据过多
                                product.GroupPurchaseList = GroupPurchase.FindGroupPurchaseByProductID(conn, product.ID, false, false);

                                //fruit所属的category信息
                                product.Category.ID = int.Parse(sdrFruit["CategoryID"].ToString());
                                product.Category.ParentID = int.Parse(sdrFruit["ParentID"].ToString());
                                product.Category.CategoryName = sdrFruit["CategoryName"].ToString();

                                //fruit包含的图片信息
                                product.FruitImgList = FindFruitImgByProdID(conn, product.ID);

                                prodList.Add(product);

                            }
                            sdrFruit.Close();
                        }

                        if (!int.TryParse(paramTopSellingIDWeekly.SqlValue.ToString(), out topSellingIDWeekly))
                        {
                            topSellingIDWeekly = 0;
                        }

                        if (!int.TryParse(paramTopSellingIDMonthly.SqlValue.ToString(), out topSellingIDMonthly))
                        {
                            topSellingIDMonthly = 0;
                        }


                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("FindProductList", ex.ToString());
            throw ex;
        }

        return prodList;

    }

    /// <summary>
    /// 根据条件查询商品记录数
    /// </summary>
    /// <param name="strWhere"></param>
    /// <returns></returns>
    public static int FindProductCount(string strWhere)
    {
        int totalRows = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdFruit = conn.CreateCommand())
                    {
                        if (string.IsNullOrEmpty(strWhere))
                        {
                            cmdFruit.CommandText = string.Format("select count(*) from Product");
                        }
                        else
                        {
                            cmdFruit.CommandText = string.Format("select count(*) from Product where {0}", strWhere);
                        }

                        Log.Debug("查询商品表记录数", cmdFruit.CommandText);

                        totalRows = int.Parse(cmdFruit.ExecuteScalar().ToString());

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
            Log.Error("指定商品表记录数", ex.ToString());
            throw ex;
        }

        return totalRows;

    }

    /// <summary>
    /// 分页查询商品，本周、本月爆款商品，用于前端查询商品页面调用
    /// </summary>
    /// <param name="strWhere"></param>
    /// <param name="strOrder"></param>
    /// <param name="categoryOfTopSelling">查询爆款商品的类别</param>
    /// <param name="topSellingIDWeekly">本周爆款商品ID</param>
    /// <param name="topSellingIDMonthly">本月爆款商品ID</param>
    /// <param name="totalRows"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="maximumRows"></param>
    /// <returns></returns>
    public static List<Fruit> FindFruitPager(string strWhere, string strOrder, int categoryOfTopSelling, out int topSellingIDWeekly, out int topSellingIDMonthly, out int totalRows, int startRowIndex, int maximumRows = 10)
    {
        List<Fruit> fruitPerPage = new List<Fruit>();
        Fruit fruit;

        totalRows = 0;
        topSellingIDWeekly = 0;
        topSellingIDMonthly = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdFruit = conn.CreateCommand())
                    {
                        cmdFruit.CommandText = "spProductQueryPager";
                        cmdFruit.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramMaximumRows = cmdFruit.CreateParameter();
                        paramMaximumRows.ParameterName = "@MaximumRows";
                        paramMaximumRows.SqlDbType = SqlDbType.Int;
                        paramMaximumRows.Direction = ParameterDirection.Input;
                        paramMaximumRows.SqlValue = maximumRows;
                        cmdFruit.Parameters.Add(paramMaximumRows);

                        SqlParameter paramStartRowIndex = cmdFruit.CreateParameter();
                        paramStartRowIndex.ParameterName = "@StartRowIndex";
                        paramStartRowIndex.SqlDbType = SqlDbType.Int;
                        paramStartRowIndex.Direction = ParameterDirection.Input;
                        paramStartRowIndex.SqlValue = startRowIndex;
                        cmdFruit.Parameters.Add(paramStartRowIndex);

                        SqlParameter paramWhere = cmdFruit.CreateParameter();
                        paramWhere.ParameterName = "@strWhere";
                        paramWhere.SqlDbType = SqlDbType.VarChar;
                        paramWhere.Size = 1000;
                        paramWhere.Direction = ParameterDirection.Input;
                        paramWhere.SqlValue = strWhere;
                        cmdFruit.Parameters.Add(paramWhere);

                        SqlParameter paramOrder = cmdFruit.CreateParameter();
                        paramOrder.ParameterName = "@strOrder";
                        paramOrder.SqlDbType = SqlDbType.VarChar;
                        paramOrder.Size = 1000;
                        paramOrder.Direction = ParameterDirection.Input;
                        paramOrder.SqlValue = strOrder;
                        cmdFruit.Parameters.Add(paramOrder);

                        SqlParameter paramCategoryOfTopSelling = cmdFruit.CreateParameter();
                        paramCategoryOfTopSelling.ParameterName = "@categoryOfTopSelling";
                        paramCategoryOfTopSelling.SqlDbType = SqlDbType.Int;
                        paramCategoryOfTopSelling.Direction = ParameterDirection.Input;
                        paramCategoryOfTopSelling.SqlValue = categoryOfTopSelling;
                        cmdFruit.Parameters.Add(paramCategoryOfTopSelling);

                        SqlParameter paramTotalRows = cmdFruit.CreateParameter();
                        paramTotalRows.ParameterName = "@TotalRows";
                        paramTotalRows.SqlDbType = SqlDbType.Int;
                        paramTotalRows.Direction = ParameterDirection.Output;
                        cmdFruit.Parameters.Add(paramTotalRows);

                        SqlParameter paramTopSellingIDWeekly = cmdFruit.CreateParameter();
                        paramTopSellingIDWeekly.ParameterName = "@TopSellingIDWeekly";
                        paramTopSellingIDWeekly.SqlDbType = SqlDbType.Int;
                        paramTopSellingIDWeekly.Direction = ParameterDirection.Output;
                        cmdFruit.Parameters.Add(paramTopSellingIDWeekly);

                        SqlParameter paramTopSellingIDMonthly = cmdFruit.CreateParameter();
                        paramTopSellingIDMonthly.ParameterName = "@TopSellingIDMonthly";
                        paramTopSellingIDMonthly.SqlDbType = SqlDbType.Int;
                        paramTopSellingIDMonthly.Direction = ParameterDirection.Output;
                        cmdFruit.Parameters.Add(paramTopSellingIDMonthly);

                        foreach (SqlParameter param in cmdFruit.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        using (SqlDataReader sdrFruit = cmdFruit.ExecuteReader())
                        {
                            while (sdrFruit.Read())
                            {
                                fruit = new Fruit();

                                fruit.ID = int.Parse(sdrFruit["Id"].ToString());
                                fruit.FruitName = sdrFruit["ProductName"].ToString();
                                fruit.FruitPrice = decimal.Parse(sdrFruit["ProductPrice"].ToString());
                                fruit.FruitUnit = sdrFruit["ProductUnit"].ToString();
                                fruit.InventoryQty = int.Parse(sdrFruit["InventoryQty"].ToString());
                                fruit.OnSale = bool.Parse(sdrFruit["ProductOnSale"].ToString());
                                fruit.FruitDesc = sdrFruit["ProductDesc"].ToString();
                                fruit.IsSticky = bool.Parse(sdrFruit["IsSticky"].ToString());
                                fruit.Priority = int.Parse(sdrFruit["Priority"].ToString());

                                //fruit包含的团购信息，使用已有的数据库链接对象，但不加载团购活动和活动成员，避免前端页面加载数据过多
                                fruit.GroupPurchaseList = GroupPurchase.FindGroupPurchaseByProductID(conn, fruit.ID, false, false);

                                //fruit所属的category信息
                                fruit.Category.ID = int.Parse(sdrFruit["CategoryID"].ToString());
                                fruit.Category.ParentID = int.Parse(sdrFruit["ParentID"].ToString());
                                fruit.Category.CategoryName = sdrFruit["CategoryName"].ToString();

                                //fruit包含的图片信息
                                fruit.FruitImgList = FindFruitImgByProdID(conn, fruit.ID);

                                fruitPerPage.Add(fruit);

                            }
                            sdrFruit.Close();
                        }

                        if (!int.TryParse(paramTotalRows.SqlValue.ToString(), out totalRows))
                        {
                            totalRows = 0;
                        }

                        if (!int.TryParse(paramTopSellingIDWeekly.SqlValue.ToString(), out topSellingIDWeekly))
                        {
                            topSellingIDWeekly = 0;
                        }

                        if (!int.TryParse(paramTopSellingIDMonthly.SqlValue.ToString(), out topSellingIDMonthly))
                        {
                            topSellingIDMonthly = 0;
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
            Log.Error("分页查询指定商品", ex.ToString());
            throw ex;
        }

        return fruitPerPage;

    }

    /// <summary>
    /// 根据ID查询商品，默认加载其团购对象
    /// </summary>
    /// <param name="fruitID">商品ID</param>
    /// <returns></returns>
    public static Fruit FindFruitByID(int fruitID)
    {
        return FindFruitByID(fruitID, true);
    }

    /// <summary>
    /// 根据ID查询商品
    /// </summary>
    /// <param name="fruitID">商品ID</param>
    /// <param name="isLoadGroupPurchase">是否加载此商品的团购信息，如果是团购对象里的方法调用，查询团购所属的商品对象，则此参数必须为false，否则会循环调用</param>
    /// <returns></returns>
    public static Fruit FindFruitByID(int fruitID, bool isLoadGroupPurchase)
    {

        Fruit fruit = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                fruit = FindFruitByID(conn, fruitID, isLoadGroupPurchase);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定水果", ex.ToString());
            throw ex;
        }

        return fruit;

    }

    /// <summary>
    /// 根据ID查询商品
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="fruitID">商品ID</param>
    /// <param name="isLoadGroupPurchase">是否加载此商品的团购信息，如果是团购对象里的方法调用，查询团购所属的商品对象，则此参数必须为false，否则会循环调用</param>
    /// <returns></returns>
    public static Fruit FindFruitByID(SqlConnection conn, int fruitID, bool isLoadGroupPurchase)
    {

        Fruit fruit = null;

        try
        {
            using (SqlCommand cmdFruit = conn.CreateCommand())
            {
                SqlParameter paramID = cmdFruit.CreateParameter();
                paramID.ParameterName = "@Id";
                paramID.SqlDbType = System.Data.SqlDbType.Int;
                paramID.SqlValue = fruitID;
                cmdFruit.Parameters.Add(paramID);

                cmdFruit.CommandText = "select Product.*,Category.ParentID,Category.CategoryName from Product left join Category on Product.CategoryID = Category.Id where Product.Id = @Id";

                using (SqlDataReader sdrFruit = cmdFruit.ExecuteReader())
                {
                    while (sdrFruit.Read())
                    {
                        fruit = new Fruit();

                        fruit.ID = int.Parse(sdrFruit["Id"].ToString());
                        fruit.FruitName = sdrFruit["ProductName"].ToString();
                        fruit.FruitPrice = decimal.Parse(sdrFruit["ProductPrice"].ToString());
                        fruit.FruitUnit = sdrFruit["ProductUnit"].ToString();
                        fruit.InventoryQty = int.Parse(sdrFruit["InventoryQty"].ToString());
                        fruit.OnSale = bool.Parse(sdrFruit["ProductOnSale"].ToString());
                        fruit.FruitDesc = sdrFruit["ProductDesc"].ToString();
                        fruit.IsSticky = bool.Parse(sdrFruit["IsSticky"].ToString());
                        fruit.Priority = int.Parse(sdrFruit["Priority"].ToString());

                        //fruit所属的category信息
                        fruit.Category.ID = int.Parse(sdrFruit["CategoryID"].ToString());
                        fruit.Category.ParentID = int.Parse(sdrFruit["ParentID"].ToString());
                        fruit.Category.CategoryName = sdrFruit["CategoryName"].ToString();

                        //fruit包含的图片信息
                        fruit.FruitImgList = FindFruitImgByProdID(conn, fruit.ID);

                        if (isLoadGroupPurchase)
                        {
                            fruit.GroupPurchaseList = GroupPurchase.FindGroupPurchaseByProductID(conn, fruit.ID, true, true);
                        }
                        else
                        {
                            fruit.GroupPurchaseList = null;
                        }
                    }
                    sdrFruit.Close();
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定水果", ex.ToString());
            throw ex;
        }

        return fruit;

    }

    public static List<FruitImg> FindFruitImgByProdID(int prodID)
    {
        List<FruitImg> fruitImgList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                fruitImgList = FindFruitImgByProdID(conn, prodID);
            }
        }
        catch (Exception ex)
        {
            Log.Error("根据水果ID查询所有相关图片", ex.ToString());
            throw ex;

        }
        return fruitImgList;

    }

    /// <summary>
    /// 根据产品ID查询相关的图片
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="prodID"></param>
    /// <returns></returns>
    public static List<FruitImg> FindFruitImgByProdID(SqlConnection conn, int prodID)
    {
        List<FruitImg> fruitImgList = new List<FruitImg>();
        FruitImg fruitImg;

        try
        {
            using (SqlCommand cmdFruitImg = conn.CreateCommand())
            {
                cmdFruitImg.CommandText = "select * from ProductImg where ProductID=@ProductID";

                SqlParameter paramProductID = cmdFruitImg.CreateParameter();
                paramProductID.ParameterName = "@ProductID";
                paramProductID.SqlDbType = System.Data.SqlDbType.Int;
                paramProductID.SqlValue = prodID;
                cmdFruitImg.Parameters.Add(paramProductID);

                using (SqlDataReader sdrFruitImg = cmdFruitImg.ExecuteReader())
                {
                    while (sdrFruitImg.Read())
                    {
                        fruitImg = new FruitImg();

                        fruitImg.ImgID = int.Parse(sdrFruitImg["Id"].ToString());
                        fruitImg.ImgName = sdrFruitImg["ImgName"].ToString();
                        fruitImg.ImgDesc = sdrFruitImg["ImgDesc"].ToString();
                        fruitImg.MainImg = sdrFruitImg["MainImg"] == DBNull.Value ? false : bool.Parse(sdrFruitImg["MainImg"].ToString());
                        fruitImg.DetailImg = sdrFruitImg["DetailImg"] == DBNull.Value ? false : bool.Parse(sdrFruitImg["DetailImg"].ToString());
                        fruitImg.ImgSeqX = (sdrFruitImg["ImgSeqX"] == DBNull.Value) ? 0 : int.Parse(sdrFruitImg["ImgSeqX"].ToString());
                        fruitImg.ImgSeqY = (sdrFruitImg["ImgSeqY"] == DBNull.Value) ? 0 : int.Parse(sdrFruitImg["ImgSeqY"].ToString());

                        fruitImgList.Add(fruitImg);

                    }
                }
            }

            fruitImgList.Sort();

        }
        catch (Exception ex)
        {
            Log.Error("根据水果ID查询所有相关图片", ex.ToString());
            throw ex;
        }

        return fruitImgList;


    }


    public static Fruit AddFruit(Fruit fruit)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    using (SqlCommand cmdAddFruit = conn.CreateCommand())
                    {
                        cmdAddFruit.Transaction = trans;

                        SqlParameter paramFruitName = cmdAddFruit.CreateParameter();
                        paramFruitName.ParameterName = "@ProductName";
                        paramFruitName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramFruitName.Size = 200;
                        paramFruitName.SqlValue = fruit.FruitName;
                        cmdAddFruit.Parameters.Add(paramFruitName);

                        SqlParameter paramFruitPrice = cmdAddFruit.CreateParameter();
                        paramFruitPrice.ParameterName = "@ProductPrice";
                        paramFruitPrice.SqlDbType = System.Data.SqlDbType.Decimal;
                        paramFruitPrice.SqlValue = fruit.FruitPrice;
                        cmdAddFruit.Parameters.Add(paramFruitPrice);

                        SqlParameter paramFruitUnit = cmdAddFruit.CreateParameter();
                        paramFruitUnit.ParameterName = "@ProductUnit";
                        paramFruitUnit.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramFruitUnit.Size = 50;
                        paramFruitUnit.SqlValue = fruit.FruitUnit;
                        cmdAddFruit.Parameters.Add(paramFruitUnit);

                        SqlParameter paramFruitDesc = cmdAddFruit.CreateParameter();
                        paramFruitDesc.ParameterName = "@ProductDesc";
                        paramFruitDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramFruitDesc.Size = 4000;
                        paramFruitDesc.SqlValue = fruit.FruitDesc;
                        cmdAddFruit.Parameters.Add(paramFruitDesc);

                        SqlParameter paramInventoryQty = cmdAddFruit.CreateParameter();
                        paramInventoryQty.ParameterName = "@InventoryQty";
                        paramInventoryQty.SqlDbType = System.Data.SqlDbType.Int;
                        paramInventoryQty.SqlValue = fruit.InventoryQty;
                        cmdAddFruit.Parameters.Add(paramInventoryQty);

                        SqlParameter paramOnSale = cmdAddFruit.CreateParameter();
                        paramOnSale.ParameterName = "@ProductOnSale";
                        paramOnSale.SqlDbType = System.Data.SqlDbType.Bit;
                        paramOnSale.SqlValue = fruit.OnSale;
                        cmdAddFruit.Parameters.Add(paramOnSale);

                        SqlParameter paramCategoryID = cmdAddFruit.CreateParameter();
                        paramCategoryID.ParameterName = "@CategoryID";
                        paramCategoryID.SqlDbType = System.Data.SqlDbType.Int;
                        paramCategoryID.SqlValue = fruit.Category.ID;
                        cmdAddFruit.Parameters.Add(paramCategoryID);

                        SqlParameter paramIsSticky = cmdAddFruit.CreateParameter();
                        paramIsSticky.ParameterName = "@IsSticky";
                        paramIsSticky.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsSticky.SqlValue = fruit.IsSticky;
                        cmdAddFruit.Parameters.Add(paramIsSticky);

                        SqlParameter paramPriority = cmdAddFruit.CreateParameter();
                        paramPriority.ParameterName = "@Priority";
                        paramPriority.SqlDbType = System.Data.SqlDbType.Int;
                        paramPriority.SqlValue = fruit.Priority;
                        cmdAddFruit.Parameters.Add(paramPriority);

                        foreach (SqlParameter param in cmdAddFruit.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdAddFruit.CommandText = "insert into Product(ProductName,ProductPrice,ProductUnit,ProductDesc,InventoryQty,ProductOnSale,CategoryID,IsSticky,Priority) values(@ProductName,@ProductPrice,@ProductUnit,@ProductDesc,@InventoryQty,@ProductOnSale,@CategoryID,@IsSticky,@Priority);select SCOPE_IDENTITY() as 'NewProdID'";

                        var newProdID = cmdAddFruit.ExecuteScalar();

                        //新增的商品ID
                        if (newProdID != DBNull.Value)
                        {
                            fruit.ID = int.Parse(newProdID.ToString());
                        }
                        else
                        {
                            throw new Exception("插入商品错误");
                        }
                    }

                    if (fruit.FruitImgList.Count != 0)
                    {
                        using (SqlCommand cmdAddFruitImg = conn.CreateCommand())
                        {
                            cmdAddFruitImg.Transaction = trans;

                            SqlParameter paramProductID = cmdAddFruitImg.CreateParameter();
                            paramProductID.ParameterName = "@ProductID";
                            paramProductID.SqlDbType = System.Data.SqlDbType.Int;
                            cmdAddFruitImg.Parameters.Add(paramProductID);

                            SqlParameter paramImgName = cmdAddFruitImg.CreateParameter();
                            paramImgName.ParameterName = "@ImgName";
                            paramImgName.SqlDbType = System.Data.SqlDbType.NVarChar;
                            paramImgName.Size = 200;
                            cmdAddFruitImg.Parameters.Add(paramImgName);

                            SqlParameter paramImgDesc = cmdAddFruitImg.CreateParameter();
                            paramImgDesc.ParameterName = "@ImgDesc";
                            paramImgDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                            paramImgDesc.Size = 4000;
                            cmdAddFruitImg.Parameters.Add(paramImgDesc);

                            SqlParameter paramMainImg = cmdAddFruitImg.CreateParameter();
                            paramMainImg.ParameterName = "@MainImg";
                            paramMainImg.SqlDbType = System.Data.SqlDbType.Bit;
                            cmdAddFruitImg.Parameters.Add(paramMainImg);

                            SqlParameter paramDetailImg = cmdAddFruitImg.CreateParameter();
                            paramDetailImg.ParameterName = "@DetailImg";
                            paramDetailImg.SqlDbType = System.Data.SqlDbType.Bit;
                            cmdAddFruitImg.Parameters.Add(paramDetailImg);

                            SqlParameter paramImgSeqX = cmdAddFruitImg.CreateParameter();
                            paramImgSeqX.ParameterName = "@ImgSeqX";
                            paramImgSeqX.SqlDbType = System.Data.SqlDbType.Int;
                            cmdAddFruitImg.Parameters.Add(paramImgSeqX);

                            SqlParameter paramImgSeqY = cmdAddFruitImg.CreateParameter();
                            paramImgSeqY.ParameterName = "@ImgSeqY";
                            paramImgSeqY.SqlDbType = System.Data.SqlDbType.Int;
                            cmdAddFruitImg.Parameters.Add(paramImgSeqY);

                            fruit.FruitImgList.ForEach(fi =>
                            {
                                paramProductID.SqlValue = fruit.ID;
                                paramImgName.SqlValue = fi.ImgName;
                                paramImgDesc.SqlValue = fi.ImgDesc;
                                paramMainImg.SqlValue = fi.MainImg;
                                paramDetailImg.SqlValue = fi.DetailImg;
                                paramImgSeqX.SqlValue = fi.ImgSeqX;
                                paramImgSeqY.SqlValue = fi.ImgSeqY;

                                foreach (SqlParameter param in cmdAddFruitImg.Parameters)
                                {
                                    if (param.Value == null)
                                    {
                                        param.Value = DBNull.Value;
                                    }
                                }

                                cmdAddFruitImg.CommandText = "insert into ProductImg(ProductID,ImgName,ImgDesc,MainImg,DetailImg,ImgSeqX,ImgSeqY) values(@ProductID,@ImgName,@ImgDesc,@MainImg,@DetailImg,@ImgSeqX,@ImgSeqY);select SCOPE_IDENTITY() as 'NewImgID'";

                                var newImgID = cmdAddFruitImg.ExecuteScalar();

                                //新增的商品图片ID
                                if (newImgID != DBNull.Value)
                                {
                                    fi.ImgID = int.Parse(newImgID.ToString());
                                }
                                else
                                {
                                    throw new Exception("插入商品图片错误");
                                }
                            });
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Log.Error("增加一个水果错误", ex.ToString());
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("数据库连接错误", ex.ToString());
            throw ex;
        }

        return fruit;
    }

    public static List<FruitImg> AddFruitImg(int prodID, List<FruitImg> fruitImgList)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdAddFruitImg = conn.CreateCommand())
                    {
                        SqlParameter paramProductID = cmdAddFruitImg.CreateParameter();
                        paramProductID.ParameterName = "@ProductID";
                        paramProductID.SqlDbType = System.Data.SqlDbType.Int;
                        cmdAddFruitImg.Parameters.Add(paramProductID);

                        SqlParameter paramImgName = cmdAddFruitImg.CreateParameter();
                        paramImgName.ParameterName = "@ImgName";
                        paramImgName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramImgName.Size = 200;
                        cmdAddFruitImg.Parameters.Add(paramImgName);

                        SqlParameter paramImgDesc = cmdAddFruitImg.CreateParameter();
                        paramImgDesc.ParameterName = "@ImgDesc";
                        paramImgDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramImgDesc.Size = 4000;
                        cmdAddFruitImg.Parameters.Add(paramImgDesc);

                        SqlParameter paramMainImg = cmdAddFruitImg.CreateParameter();
                        paramMainImg.ParameterName = "@MainImg";
                        paramMainImg.SqlDbType = System.Data.SqlDbType.Bit;
                        cmdAddFruitImg.Parameters.Add(paramMainImg);

                        SqlParameter paramDetailImg = cmdAddFruitImg.CreateParameter();
                        paramDetailImg.ParameterName = "@DetailImg";
                        paramDetailImg.SqlDbType = System.Data.SqlDbType.Bit;
                        cmdAddFruitImg.Parameters.Add(paramDetailImg);

                        SqlParameter paramImgSeqX = cmdAddFruitImg.CreateParameter();
                        paramImgSeqX.ParameterName = "@ImgSeqX";
                        paramImgSeqX.SqlDbType = System.Data.SqlDbType.Int;
                        cmdAddFruitImg.Parameters.Add(paramImgSeqX);

                        SqlParameter paramImgSeqY = cmdAddFruitImg.CreateParameter();
                        paramImgSeqY.ParameterName = "@ImgSeqY";
                        paramImgSeqY.SqlDbType = System.Data.SqlDbType.Int;
                        cmdAddFruitImg.Parameters.Add(paramImgSeqY);

                        fruitImgList.ForEach(fi =>
                        {
                            paramProductID.SqlValue = prodID;
                            paramImgName.SqlValue = fi.ImgName;
                            paramImgDesc.SqlValue = fi.ImgDesc;
                            paramMainImg.SqlValue = fi.MainImg;
                            paramDetailImg.SqlValue = fi.DetailImg;
                            paramImgSeqX.SqlValue = fi.ImgSeqX;
                            paramImgSeqY.SqlValue = fi.ImgSeqY;

                            foreach (SqlParameter param in cmdAddFruitImg.Parameters)
                            {
                                if (param.Value == null)
                                {
                                    param.Value = DBNull.Value;
                                }
                            }

                            cmdAddFruitImg.CommandText = "insert into ProductImg(ProductID,ImgName,ImgDesc,MainImg,DetailImg,ImgSeqX,ImgSeqY) values(@ProductID,@ImgName,@ImgDesc,@MainImg,@DetailImg,@ImgSeqX,@ImgSeqY);select SCOPE_IDENTITY() as 'NewImgID'";

                            var newImgID = cmdAddFruitImg.ExecuteScalar();

                            //新增的商品图片ID
                            if (newImgID != DBNull.Value)
                            {
                                fi.ImgID = int.Parse(newImgID.ToString());
                            }
                            else
                            {
                                throw new Exception("插入商品图片错误");
                            }

                        });
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("水果图片表插入错误", ex.ToString());
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("水果图片表插入错误", ex.ToString());
            throw ex;
        }

        return fruitImgList;
    }

    public static void UpdateFruit(Fruit fruit)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    using (SqlCommand cmdUpdateFruit = conn.CreateCommand())
                    {
                        cmdUpdateFruit.Transaction = trans;

                        SqlParameter paramId = cmdUpdateFruit.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = fruit.ID;
                        cmdUpdateFruit.Parameters.Add(paramId);

                        SqlParameter paramFruitName = cmdUpdateFruit.CreateParameter();
                        paramFruitName.ParameterName = "@ProductName";
                        paramFruitName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramFruitName.Size = 200;
                        paramFruitName.SqlValue = fruit.FruitName;
                        cmdUpdateFruit.Parameters.Add(paramFruitName);

                        SqlParameter paramFruitPrice = cmdUpdateFruit.CreateParameter();
                        paramFruitPrice.ParameterName = "@ProductPrice";
                        paramFruitPrice.SqlDbType = System.Data.SqlDbType.Decimal;
                        paramFruitPrice.SqlValue = fruit.FruitPrice;
                        cmdUpdateFruit.Parameters.Add(paramFruitPrice);

                        SqlParameter paramFruitUnit = cmdUpdateFruit.CreateParameter();
                        paramFruitUnit.ParameterName = "@ProductUnit";
                        paramFruitUnit.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramFruitUnit.Size = 50;
                        paramFruitUnit.SqlValue = fruit.FruitUnit;
                        cmdUpdateFruit.Parameters.Add(paramFruitUnit);

                        SqlParameter paramFruitDesc = cmdUpdateFruit.CreateParameter();
                        paramFruitDesc.ParameterName = "@ProductDesc";
                        paramFruitDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramFruitDesc.Size = 4000;
                        paramFruitDesc.SqlValue = fruit.FruitDesc;
                        cmdUpdateFruit.Parameters.Add(paramFruitDesc);

                        SqlParameter paramInventoryQty = cmdUpdateFruit.CreateParameter();
                        paramInventoryQty.ParameterName = "@InventoryQty";
                        paramInventoryQty.SqlDbType = System.Data.SqlDbType.Int;
                        paramInventoryQty.SqlValue = fruit.InventoryQty;
                        cmdUpdateFruit.Parameters.Add(paramInventoryQty);

                        SqlParameter paramOnSale = cmdUpdateFruit.CreateParameter();
                        paramOnSale.ParameterName = "@ProductOnSale";
                        paramOnSale.SqlDbType = System.Data.SqlDbType.Bit;
                        paramOnSale.SqlValue = fruit.OnSale;
                        cmdUpdateFruit.Parameters.Add(paramOnSale);

                        SqlParameter paramCategoryID = cmdUpdateFruit.CreateParameter();
                        paramCategoryID.ParameterName = "@CategoryID";
                        paramCategoryID.SqlDbType = System.Data.SqlDbType.Int;
                        paramCategoryID.SqlValue = fruit.Category.ID;
                        cmdUpdateFruit.Parameters.Add(paramCategoryID);

                        SqlParameter paramIsSticky = cmdUpdateFruit.CreateParameter();
                        paramIsSticky.ParameterName = "@IsSticky";
                        paramIsSticky.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsSticky.SqlValue = fruit.IsSticky;
                        cmdUpdateFruit.Parameters.Add(paramIsSticky);

                        SqlParameter paramPriority = cmdUpdateFruit.CreateParameter();
                        paramPriority.ParameterName = "@Priority";
                        paramPriority.SqlDbType = System.Data.SqlDbType.Int;
                        paramPriority.SqlValue = fruit.Priority;
                        cmdUpdateFruit.Parameters.Add(paramPriority);

                        foreach (SqlParameter param in cmdUpdateFruit.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdUpdateFruit.CommandText = "update Product set ProductName = @ProductName,ProductPrice = @ProductPrice,ProductUnit = @ProductUnit,ProductDesc = @ProductDesc,InventoryQty = @InventoryQty,ProductOnSale = @ProductOnSale,CategoryID=@CategoryID,IsSticky=@IsSticky,Priority=@Priority where Id = @Id";

                        if (cmdUpdateFruit.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("更新水果失败");
                        }
                    }

                    if (fruit.FruitImgList.Count != 0)
                    {
                        using (SqlCommand cmdUpdateFruitImg = conn.CreateCommand())
                        {
                            cmdUpdateFruitImg.Transaction = trans;

                            SqlParameter paramImgID = cmdUpdateFruitImg.CreateParameter();
                            paramImgID.ParameterName = "@Id";
                            paramImgID.SqlDbType = System.Data.SqlDbType.Int;
                            cmdUpdateFruitImg.Parameters.Add(paramImgID);

                            SqlParameter paramImgName = cmdUpdateFruitImg.CreateParameter();
                            paramImgName.ParameterName = "@ImgName";
                            paramImgName.SqlDbType = System.Data.SqlDbType.NVarChar;
                            paramImgName.Size = 200;
                            cmdUpdateFruitImg.Parameters.Add(paramImgName);

                            SqlParameter paramImgDesc = cmdUpdateFruitImg.CreateParameter();
                            paramImgDesc.ParameterName = "@ImgDesc";
                            paramImgDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                            paramImgDesc.Size = 4000;
                            cmdUpdateFruitImg.Parameters.Add(paramImgDesc);

                            SqlParameter paramMainImg = cmdUpdateFruitImg.CreateParameter();
                            paramMainImg.ParameterName = "@MainImg";
                            paramMainImg.SqlDbType = System.Data.SqlDbType.Bit;
                            cmdUpdateFruitImg.Parameters.Add(paramMainImg);

                            SqlParameter paramDetailImg = cmdUpdateFruitImg.CreateParameter();
                            paramDetailImg.ParameterName = "@DetailImg";
                            paramDetailImg.SqlDbType = System.Data.SqlDbType.Bit;
                            cmdUpdateFruitImg.Parameters.Add(paramDetailImg);

                            SqlParameter paramImgSeqX = cmdUpdateFruitImg.CreateParameter();
                            paramImgSeqX.ParameterName = "@ImgSeqX";
                            paramImgSeqX.SqlDbType = System.Data.SqlDbType.Int;
                            cmdUpdateFruitImg.Parameters.Add(paramImgSeqX);

                            SqlParameter paramImgSeqY = cmdUpdateFruitImg.CreateParameter();
                            paramImgSeqY.ParameterName = "@ImgSeqY";
                            paramImgSeqY.SqlDbType = System.Data.SqlDbType.Int;
                            cmdUpdateFruitImg.Parameters.Add(paramImgSeqY);

                            fruit.FruitImgList.ForEach(fi =>
                            {
                                paramImgID.SqlValue = fi.ImgID;
                                paramImgName.SqlValue = fi.ImgName;
                                paramImgDesc.SqlValue = fi.ImgDesc;
                                paramMainImg.SqlValue = fi.MainImg;
                                paramDetailImg.SqlValue = fi.DetailImg;
                                paramImgSeqX.SqlValue = fi.ImgSeqX;
                                paramImgSeqY.SqlValue = fi.ImgSeqY;

                                foreach (SqlParameter param in cmdUpdateFruitImg.Parameters)
                                {
                                    if (param.Value == null)
                                    {
                                        param.Value = DBNull.Value;
                                    }
                                }

                                cmdUpdateFruitImg.CommandText = "update ProductImg set ImgName = @ImgName,ImgDesc = @ImgDesc,MainImg = @MainImg,DetailImg = @DetailImg,ImgSeqX = @ImgSeqX,ImgSeqY = @ImgSeqY where Id = @Id";

                                cmdUpdateFruitImg.ExecuteNonQuery();
                            });
                        }
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    Log.Error("更新一个水果", ex.ToString());
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("更新一个水果", ex.ToString());
            throw ex;
        }
    }

    public static void UpdateFruitImg(FruitImg fruitImg)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUpdateFruitImg = conn.CreateCommand())
                    {
                        SqlParameter paramImgID = cmdUpdateFruitImg.CreateParameter();
                        paramImgID.ParameterName = "@Id";
                        paramImgID.SqlDbType = System.Data.SqlDbType.Int;
                        paramImgID.SqlValue = fruitImg.ImgID;
                        cmdUpdateFruitImg.Parameters.Add(paramImgID);

                        SqlParameter paramImgName = cmdUpdateFruitImg.CreateParameter();
                        paramImgName.ParameterName = "@ImgName";
                        paramImgName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramImgName.Size = 200;
                        paramImgName.SqlValue = fruitImg.ImgName;
                        cmdUpdateFruitImg.Parameters.Add(paramImgName);

                        SqlParameter paramImgDesc = cmdUpdateFruitImg.CreateParameter();
                        paramImgDesc.ParameterName = "@ImgDesc";
                        paramImgDesc.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramImgDesc.Size = 4000;
                        paramImgDesc.SqlValue = fruitImg.ImgDesc;
                        cmdUpdateFruitImg.Parameters.Add(paramImgDesc);

                        SqlParameter paramMainImg = cmdUpdateFruitImg.CreateParameter();
                        paramMainImg.ParameterName = "@MainImg";
                        paramMainImg.SqlDbType = System.Data.SqlDbType.Bit;
                        paramMainImg.SqlValue = fruitImg.MainImg;
                        cmdUpdateFruitImg.Parameters.Add(paramMainImg);

                        SqlParameter paramDetailImg = cmdUpdateFruitImg.CreateParameter();
                        paramDetailImg.ParameterName = "@DetailImg";
                        paramDetailImg.SqlDbType = System.Data.SqlDbType.Bit;
                        cmdUpdateFruitImg.Parameters.Add(paramDetailImg);

                        SqlParameter paramImgSeqX = cmdUpdateFruitImg.CreateParameter();
                        paramImgSeqX.ParameterName = "@ImgSeqX";
                        paramImgSeqX.SqlDbType = System.Data.SqlDbType.Int;
                        paramImgSeqX.SqlValue = fruitImg.ImgSeqX;
                        cmdUpdateFruitImg.Parameters.Add(paramImgSeqX);

                        SqlParameter paramImgSeqY = cmdUpdateFruitImg.CreateParameter();
                        paramImgSeqY.ParameterName = "@ImgSeqY";
                        paramImgSeqY.SqlDbType = System.Data.SqlDbType.Int;
                        paramImgSeqY.SqlValue = fruitImg.ImgSeqY;
                        cmdUpdateFruitImg.Parameters.Add(paramImgSeqY);

                        foreach (SqlParameter param in cmdUpdateFruitImg.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdUpdateFruitImg.CommandText = "update ProductImg set ImgName = @ImgName,ImgDesc = @ImgDesc,MainImg = @MainImg,DetailImg = @DetailImg,ImgSeqX = @ImgSeqX,ImgSeqY = @ImgSeqY where Id = @Id";

                        cmdUpdateFruitImg.ExecuteNonQuery();
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("更新一个水果图片", ex.ToString());
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("更新一个水果图片", ex.ToString());
            throw ex;
        }
    }

    public static void DelFruit(int id, out int delFruitCount, out int delImgCount)
    {
        delFruitCount = 0;
        delImgCount = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    using (SqlCommand cmdDelFruit = conn.CreateCommand())
                    {
                        cmdDelFruit.Transaction = trans;

                        SqlParameter paramID = cmdDelFruit.CreateParameter();
                        paramID.ParameterName = "@Id";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = id;
                        cmdDelFruit.Parameters.Add(paramID);

                        cmdDelFruit.CommandText = "delete from Product where Id = @Id";

                        delFruitCount = cmdDelFruit.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdDelFruitImg = conn.CreateCommand())
                    {
                        cmdDelFruitImg.Transaction = trans;

                        SqlParameter paramID = cmdDelFruitImg.CreateParameter();
                        paramID.ParameterName = "@ProductID";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = id;
                        cmdDelFruitImg.Parameters.Add(paramID);

                        cmdDelFruitImg.CommandText = "delete from ProductImg where ProductID = @ProductID";

                        delImgCount = cmdDelFruitImg.ExecuteNonQuery();
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
                    if(conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("删除一个水果", ex.ToString());
            throw ex;
        }

    }

    public static void DelFruitImg(int fruitImgID)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdDelFruitImg = conn.CreateCommand())
                    {
                        SqlParameter paramFruitImgID = cmdDelFruitImg.CreateParameter();
                        paramFruitImgID.ParameterName = "@Id";
                        paramFruitImgID.SqlDbType = System.Data.SqlDbType.Int;
                        paramFruitImgID.SqlValue = fruitImgID;
                        cmdDelFruitImg.Parameters.Add(paramFruitImgID);

                        cmdDelFruitImg.CommandText = "delete from ProductImg where Id = @Id";

                        cmdDelFruitImg.ExecuteNonQuery();
                    }
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

        }
        catch (Exception ex)
        {
            Log.Error("删除一个水果图片", ex.ToString());
            throw ex;

        }
    }

    /// <summary>
    /// 查询指定商品的销售量
    /// </summary>
    /// <param name="prodID"></param>
    /// <param name="orderDate"></param>
    /// <returns></returns>
    public static int SalesVolume(int prodID, params DateTime[] orderDate)
    {
        int salesVolume = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdFruit = conn.CreateCommand())
                    {
                        SqlParameter paramID = cmdFruit.CreateParameter();
                        paramID.ParameterName = "@ProductID";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = prodID;
                        cmdFruit.Parameters.Add(paramID);

                        switch (orderDate.Length)
                        {
                            case 0:
                                cmdFruit.CommandText = "select sum(PurchaseQty) from OrderDetail where ProductID = @ProductID";
                                break;
                            case 1:
                                SqlParameter paramOrderDate = cmdFruit.CreateParameter();
                                paramOrderDate.ParameterName = "@OrderDate";
                                paramOrderDate.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDate.Size = 6;
                                paramOrderDate.SqlValue = orderDate[0].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDate);

                                cmdFruit.CommandText = "select sum(PurchaseQty) from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id where OrderDetail.ProductID = @ProductID and CONVERT(varchar(6),OrderDate,112) = @OrderDate";
                                break;
                            case 2:
                                SqlParameter paramOrderDateStart = cmdFruit.CreateParameter();
                                paramOrderDateStart.ParameterName = "@OrderDateStart";
                                paramOrderDateStart.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateStart.Size = 6;
                                paramOrderDateStart.SqlValue = orderDate[0].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDateStart);

                                SqlParameter paramOrderDateEnd = cmdFruit.CreateParameter();
                                paramOrderDateEnd.ParameterName = "@OrderDateEnd";
                                paramOrderDateEnd.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateEnd.Size = 6;
                                paramOrderDateEnd.SqlValue = orderDate[1].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDateEnd);

                                cmdFruit.CommandText = "select sum(PurchaseQty) from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id where OrderDetail.ProductID = @ProductID and CONVERT(varchar(6),OrderDate,112) >= @OrderDateStart and CONVERT(varchar(6),OrderDate,112) <= @OrderDateEnd";
                                break;
                            default:
                                throw new Exception("参数orderDate只能接收0~2个参数");
                        }

                        var result = cmdFruit.ExecuteScalar();
                        if(result != DBNull.Value)
                        {
                            salesVolume = int.Parse(result.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定商品的销售量", ex.ToString());
            throw ex;
        }

        return salesVolume;
    }

    /// <summary>
    /// 在指定时间范围内查询销量最高的商品
    /// </summary>
    /// <param name="orderDate"></param>
    /// <returns></returns>
    public static int FindTopSelling(params DateTime[] orderDate)
    {
        int prodID = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdFruit = conn.CreateCommand())
                    {
                        switch (orderDate.Length)
                        {
                            case 0:
                                cmdFruit.CommandText = "select top 1 ProductID, sum(PurchaseQty) from OrderDetail left join Product on OrderDetail.ProductID = Product.Id group by ProductID order by sum(PurchaseQty) desc";
                                break;
                            case 1:
                                SqlParameter paramOrderDate = cmdFruit.CreateParameter();
                                paramOrderDate.ParameterName = "@OrderDate";
                                paramOrderDate.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDate.Size = 6;
                                paramOrderDate.SqlValue = orderDate[0].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDate);

                                cmdFruit.CommandText = "select top 1 ProductID, sum(PurchaseQty) from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id where CONVERT(varchar(6), OrderDate, 112) = @OrderDate group by ProductID order by sum(PurchaseQty) desc";
                                break;
                            case 2:
                                SqlParameter paramOrderDateStart = cmdFruit.CreateParameter();
                                paramOrderDateStart.ParameterName = "@OrderDateStart";
                                paramOrderDateStart.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateStart.Size = 6;
                                paramOrderDateStart.SqlValue = orderDate[0].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDateStart);

                                SqlParameter paramOrderDateEnd = cmdFruit.CreateParameter();
                                paramOrderDateEnd.ParameterName = "@OrderDateEnd";
                                paramOrderDateEnd.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateEnd.Size = 6;
                                paramOrderDateEnd.SqlValue = orderDate[1].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDateEnd);

                                cmdFruit.CommandText = "select top 1 ProductID, sum(PurchaseQty) from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id where CONVERT(varchar(6),OrderDate,112) >= @OrderDateStart and CONVERT(varchar(6),OrderDate,112) <= @OrderDateEnd group by ProductID order by sum(PurchaseQty) desc";
                                break;
                            default:
                                throw new Exception("参数orderDate只能接收0~2个参数");
                        }

                        using (SqlDataReader sdrFruit = cmdFruit.ExecuteReader())
                        {
                            while (sdrFruit.Read())
                            {
                                prodID = int.Parse(sdrFruit["ProductID"].ToString());
                            }

                            sdrFruit.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("FindTopSelling", ex.ToString());
            throw ex;
        }

        return prodID;
    }

    /// <summary>
    /// 在指定时间范围内查询销量最高的商品
    /// </summary>
    /// <param name="categoryID"></param>
    /// <param name="orderDate"></param>
    /// <returns></returns>
    public static int FindTopSelling(int categoryID, params DateTime[] orderDate)
    {
        int prodID = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdFruit = conn.CreateCommand())
                    {
                        SqlParameter paramCategoryID = cmdFruit.CreateParameter();
                        paramCategoryID.ParameterName = "@CategoryID";
                        paramCategoryID.SqlDbType = System.Data.SqlDbType.Int;
                        paramCategoryID.SqlValue = categoryID;
                        cmdFruit.Parameters.Add(paramCategoryID);

                        switch (orderDate.Length)
                        {
                            case 0:
                                cmdFruit.CommandText = "select top 1 ProductID, sum(PurchaseQty) from OrderDetail left join Product on OrderDetail.ProductID = Product.Id where ProductOnSale=1 and CategoryID=@CategoryID group by ProductID order by sum(PurchaseQty) desc";
                                break;
                            case 1:
                                SqlParameter paramOrderDate = cmdFruit.CreateParameter();
                                paramOrderDate.ParameterName = "@OrderDate";
                                paramOrderDate.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDate.Size = 6;
                                paramOrderDate.SqlValue = orderDate[0].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDate);

                                cmdFruit.CommandText = "select top 1 ProductID, sum(PurchaseQty) from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id left join Product on OrderDetail.ProductID = Product.Id where ProductOnSale=1 and CONVERT(varchar(6), OrderDate, 112) = @OrderDate and CategoryID=@CategoryID group by ProductID order by sum(PurchaseQty) desc";
                                break;
                            case 2:
                                SqlParameter paramOrderDateStart = cmdFruit.CreateParameter();
                                paramOrderDateStart.ParameterName = "@OrderDateStart";
                                paramOrderDateStart.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateStart.Size = 6;
                                paramOrderDateStart.SqlValue = orderDate[0].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDateStart);

                                SqlParameter paramOrderDateEnd = cmdFruit.CreateParameter();
                                paramOrderDateEnd.ParameterName = "@OrderDateEnd";
                                paramOrderDateEnd.SqlDbType = System.Data.SqlDbType.VarChar;
                                paramOrderDateEnd.Size = 6;
                                paramOrderDateEnd.SqlValue = orderDate[1].ToString("yyyyMM");
                                cmdFruit.Parameters.Add(paramOrderDateEnd);

                                cmdFruit.CommandText = "select top 1 ProductID, sum(PurchaseQty) from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id left join Product on OrderDetail.ProductID = Product.Id where ProductOnSale=1 and CONVERT(varchar(6),OrderDate,112) >= @OrderDateStart and CONVERT(varchar(6),OrderDate,112) <= @OrderDateEnd and CategoryID=@CategoryID group by ProductID order by sum(PurchaseQty) desc";
                                break;
                            default:
                                throw new Exception("参数orderDate只能接收0~2个参数");
                        }

                        using (SqlDataReader sdrFruit = cmdFruit.ExecuteReader())
                        {
                            while (sdrFruit.Read())
                            {
                                prodID = int.Parse(sdrFruit["ProductID"].ToString());
                            }

                            sdrFruit.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("FindTopSelling", ex.ToString());
            throw ex;
        }

        return prodID;
    }

    /// <summary>
    /// 根据是否置顶、优先级、ID判断大小
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    int IComparable<Fruit>.CompareTo(Fruit other)
    {
        if (this.IsSticky && !other.IsSticky)
        {
            return -1;
        }
        else
        {
            if (!this.IsSticky && other.IsSticky)
            {
                return 1;
            }
            else
            {
                if (this.Priority > other.Priority)
                {
                    return -1;
                }
                else
                {
                    if (this.Priority < other.Priority)
                    {
                        return 1;
                    }
                    else
                    {
                        if (this.ID > other.ID)
                        {
                            return -1;
                        }
                        else
                        {
                            if (this.ID < other.ID)
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                    }
                }
            }
        }
    }
}


/// <summary>
/// 产品图片类
/// </summary>
public class FruitImg : IComparable<FruitImg>
{
    public int ImgID { get; set; }

    /// <summary>
    /// 图片文件名
    /// </summary>
    public string ImgName { get; set; }

    /// <summary>
    /// 图片描述
    /// </summary>
    public string ImgDesc { get; set; }

    /// <summary>
    /// 是否主图
    /// </summary>
    public bool MainImg { get; set; }

    /// <summary>
    /// 是否详图
    /// </summary>
    public bool DetailImg { get; set; }

    /// <summary>
    /// 图片位置序列号（Y轴+X轴）
    /// </summary>
    public int ImgSeq
    {
        get
        {
            return int.Parse(this.ImgSeqY.ToString() + this.ImgSeqX.ToString());
        }
    }

    /// <summary>
    /// 图片位置序列号X轴
    /// </summary>
    public int ImgSeqX { get; set; }

    /// <summary>
    /// 图片位置序列号Y轴
    /// </summary>
    public int ImgSeqY { get; set; }

    public FruitImg() { }

    public FruitImg(string imgName, string imgDesc)
    {
        this.ImgName = ImgName;
        this.ImgDesc = ImgDesc;
    }

    /// <summary>
    /// 根据图片的X/Y轴位置进行排序
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    int IComparable<FruitImg>.CompareTo(FruitImg other)
    {
        if (this.ImgSeq > other.ImgSeq)
        {
            return 1;
        }
        else
        {
            if (this.ImgSeq < other.ImgSeq)
            {
                return -1;
            }
            else
            {
                if (this.ImgID > other.ImgID)
                {
                    return 1;
                }
                else
                {
                    if (this.ImgID < other.ImgID)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }
}