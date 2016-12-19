using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

/// <summary>
/// 团购类
/// </summary>
public class GroupPurchase
{
    public int ID { get; set; }

    /// <summary>
    /// 团购所属商品
    /// </summary>
    public Fruit Product { get; set; }

    /// <summary>
    /// 团购名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 团购开始时间
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 团购结束时间
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 团购要求参加人数
    /// </summary>
    public int RequiredNumber { get; set; }

    /// <summary>
    /// 团购描述信息
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 团购价
    /// </summary>
    public decimal GroupPrice { get; set; }

    /// <summary>
    /// 此团购包含的所有活动
    /// </summary>
    public List<GroupPurchaseEvent> GroupEvents { get; set; }

    public GroupPurchase()
    {
        this.GroupEvents = new List<GroupPurchaseEvent>();
    }

    /// <summary>
    /// 按条件查询团购记录数
    /// </summary>
    /// <param name="strWhere"></param>
    /// <returns></returns>
    public static int FindGroupCount(string strWhere)
    {
        int totalRows = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdGroup = conn.CreateCommand())
                    {
                        if (string.IsNullOrEmpty(strWhere))
                        {
                            cmdGroup.CommandText = string.Format("select count(*) from GroupPurchase");
                        }
                        else
                        {
                            cmdGroup.CommandText = string.Format("select count(*) from GroupPurchase where {0}", strWhere);
                        }

                        Log.Debug("查询团购表记录数", cmdGroup.CommandText);

                        totalRows = int.Parse(cmdGroup.ExecuteScalar().ToString());

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
            Log.Error("指定团购表记录数", ex.ToString());
            throw ex;
        }

        return totalRows;

    }

    /// <summary>
    /// 分页查询团购，默认加载团购活动和成员
    /// </summary>
    /// <param name="strWhere"></param>
    /// <param name="strOrder"></param>
    /// <param name="totalRows"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="maximumRows"></param>
    /// <returns></returns>
    public static List<GroupPurchase> FindGroupPurchasePager(string strWhere, string strOrder, out int totalRows, int startRowIndex, int maximumRows = 10)
    {
        return FindGroupPurchasePager(true, true, strWhere, strOrder, out totalRows, startRowIndex, maximumRows);
    }

    /// <summary>
    /// 分页查询团购
    /// </summary>
    /// <param name="isLoadGroupEvents">是否加载团购活动</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <param name="strWhere"></param>
    /// <param name="strOrder"></param>
    /// <param name="totalRows"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="maximumRows"></param>
    /// <returns></returns>
    public static List<GroupPurchase> FindGroupPurchasePager(bool isLoadGroupEvents, bool isLoadGroupEventMember, string strWhere, string strOrder, out int totalRows, int startRowIndex, int maximumRows = 10)
    {
        List<GroupPurchase> groupPerPage = new List<GroupPurchase>();
        GroupPurchase groupPurchase;

        totalRows = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdGroup = conn.CreateCommand())
                    {
                        cmdGroup.CommandText = "spGroupQueryPager";
                        cmdGroup.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramMaximumRows = cmdGroup.CreateParameter();
                        paramMaximumRows.ParameterName = "@MaximumRows";
                        paramMaximumRows.SqlDbType = SqlDbType.Int;
                        paramMaximumRows.Direction = ParameterDirection.Input;
                        paramMaximumRows.SqlValue = maximumRows;
                        cmdGroup.Parameters.Add(paramMaximumRows);

                        SqlParameter paramStartRowIndex = cmdGroup.CreateParameter();
                        paramStartRowIndex.ParameterName = "@StartRowIndex";
                        paramStartRowIndex.SqlDbType = SqlDbType.Int;
                        paramStartRowIndex.Direction = ParameterDirection.Input;
                        paramStartRowIndex.SqlValue = startRowIndex;
                        cmdGroup.Parameters.Add(paramStartRowIndex);

                        SqlParameter paramWhere = cmdGroup.CreateParameter();
                        paramWhere.ParameterName = "@strWhere";
                        paramWhere.SqlDbType = SqlDbType.VarChar;
                        paramWhere.Size = 1000;
                        paramWhere.Direction = ParameterDirection.Input;
                        paramWhere.SqlValue = strWhere;
                        cmdGroup.Parameters.Add(paramWhere);

                        SqlParameter paramOrder = cmdGroup.CreateParameter();
                        paramOrder.ParameterName = "@strOrder";
                        paramOrder.SqlDbType = SqlDbType.VarChar;
                        paramOrder.Size = 1000;
                        paramOrder.Direction = ParameterDirection.Input;
                        paramOrder.SqlValue = strOrder;
                        cmdGroup.Parameters.Add(paramOrder);

                        SqlParameter paramTotalRows = cmdGroup.CreateParameter();
                        paramTotalRows.ParameterName = "@TotalRows";
                        paramTotalRows.SqlDbType = SqlDbType.Int;
                        paramTotalRows.Direction = ParameterDirection.Output;
                        cmdGroup.Parameters.Add(paramTotalRows);

                        foreach (SqlParameter param in cmdGroup.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        using (SqlDataReader sdrGroup = cmdGroup.ExecuteReader())
                        {
                            while (sdrGroup.Read())
                            {
                                groupPurchase = new GroupPurchase();

                                groupPurchase.ID = int.Parse(sdrGroup["Id"].ToString());
                                groupPurchase.Name = sdrGroup["Name"].ToString();
                                groupPurchase.StartDate = DateTime.Parse(sdrGroup["StartDate"].ToString());
                                groupPurchase.EndDate = DateTime.Parse(sdrGroup["EndDate"].ToString());
                                groupPurchase.RequiredNumber = int.Parse(sdrGroup["RequiredNumber"].ToString());
                                groupPurchase.Description = sdrGroup["Description"].ToString();
                                groupPurchase.GroupPrice = decimal.Parse(sdrGroup["GroupPrice"].ToString());
                                groupPurchase.Product = Fruit.FindFruitByID(conn, int.Parse(sdrGroup["ProductID"].ToString()), false);
                                if (isLoadGroupEvents)
                                {
                                    groupPurchase.GroupEvents = GroupPurchaseEvent.FindGroupPurchaseEventByGroupID(conn, groupPurchase.ID, isLoadGroupEventMember);
                                }
                                else
                                {
                                    groupPurchase.GroupEvents = null;
                                }
                                groupPerPage.Add(groupPurchase);

                            }
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
            Log.Error("分页查询指定团购", ex.ToString());
            throw ex;
        }

        return groupPerPage;

    }

    /// <summary>
    /// 根据ID查询团购信息，默认加载其团购活动和成员
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static GroupPurchase FindGroupPurchaseByID(int id)
    {
        return FindGroupPurchaseByID(id, true, true);
    }

    /// <summary>
    /// 根据ID查询团购信息，指定是否加载团购活动和成员
    /// </summary>
    /// <param name="id"></param>
    /// <param name="isLoadGroupEvent">是否加载团购活动</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <returns></returns>
    public static GroupPurchase FindGroupPurchaseByID(int id, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        GroupPurchase groupPurchase = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupPurchase = FindGroupPurchaseByID(conn, id, isLoadGroupEvent, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购", ex.ToString());
            throw ex;
        }

        return groupPurchase;
    }

    public static GroupPurchase FindGroupPurchaseByID(SqlConnection conn, int id, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        GroupPurchase groupPurchase = null;

        try
        {
            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramID = cmdGroup.CreateParameter();
                paramID.ParameterName = "@Id";
                paramID.SqlDbType = System.Data.SqlDbType.Int;
                paramID.SqlValue = id;
                cmdGroup.Parameters.Add(paramID);

                cmdGroup.CommandText = "select * from GroupPurchase where Id = @Id";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        groupPurchase = new GroupPurchase();

                        groupPurchase.ID = int.Parse(sdr["Id"].ToString());
                        groupPurchase.Name = sdr["Name"].ToString();
                        groupPurchase.StartDate = DateTime.Parse(sdr["StartDate"].ToString());
                        groupPurchase.EndDate = DateTime.Parse(sdr["EndDate"].ToString());
                        groupPurchase.RequiredNumber = int.Parse(sdr["RequiredNumber"].ToString());
                        groupPurchase.Description = sdr["Description"].ToString();
                        groupPurchase.GroupPrice = decimal.Parse(sdr["GroupPrice"].ToString());
                        groupPurchase.Product = Fruit.FindFruitByID(conn, int.Parse(sdr["ProductID"].ToString()), false);
                        if (isLoadGroupEvent)
                        {
                            groupPurchase.GroupEvents = GroupPurchaseEvent.FindGroupPurchaseEventByGroupID(conn, groupPurchase.ID, isLoadGroupEventMember);
                        }
                        else
                        {
                            groupPurchase.GroupEvents = null;
                        }

                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购", ex.ToString());
            throw ex;
        }

        return groupPurchase;
    }


    /// <summary>
    /// 根据团购名称查询，默认加载团购活动
    /// </summary>
    /// <param name="name">团购名称</param>
    /// <returns></returns>
    public static List<GroupPurchase> FindGroupPurchaseByName(string name)
    {
        return FindGroupPurchaseByName(name, true, true);
    }

    public static List<GroupPurchase> FindGroupPurchaseByName(string name, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        List<GroupPurchase> groupPurchaseList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupPurchaseList = FindGroupPurchaseByName(conn, name, isLoadGroupEvent, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购", ex.ToString());
            throw ex;
        }

        return groupPurchaseList;
    }

    /// <summary>
    /// 根据团购名称查询
    /// </summary>
    /// <param name="name">团购名称</param>
    /// <param name="isLoadGroupEvent">是否加载团购活动</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <returns></returns>
    public static List<GroupPurchase> FindGroupPurchaseByName(SqlConnection conn, string name, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        List<GroupPurchase> groupPurchaseList = new List<GroupPurchase>();

        try
        {
            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramName = cmdGroup.CreateParameter();
                paramName.ParameterName = "@Name";
                paramName.SqlDbType = System.Data.SqlDbType.NVarChar;
                paramName.SqlValue = name;
                cmdGroup.Parameters.Add(paramName);

                cmdGroup.CommandText = "select * from GroupPurchase where Name like '%' + @Name + '%'";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    GroupPurchase groupPurchase;

                    while (sdr.Read())
                    {
                        groupPurchase = new GroupPurchase();

                        groupPurchase.ID = int.Parse(sdr["Id"].ToString());
                        groupPurchase.Name = sdr["Name"].ToString();
                        groupPurchase.StartDate = DateTime.Parse(sdr["StartDate"].ToString());
                        groupPurchase.EndDate = DateTime.Parse(sdr["EndDate"].ToString());
                        groupPurchase.RequiredNumber = int.Parse(sdr["RequiredNumber"].ToString());
                        groupPurchase.Description = sdr["Description"].ToString();
                        groupPurchase.GroupPrice = decimal.Parse(sdr["GroupPrice"].ToString());
                        groupPurchase.Product = Fruit.FindFruitByID(conn, int.Parse(sdr["ProductID"].ToString()), false);
                        if (isLoadGroupEvent)
                        {
                            groupPurchase.GroupEvents = GroupPurchaseEvent.FindGroupPurchaseEventByGroupID(conn, groupPurchase.ID, isLoadGroupEventMember);
                        }
                        else
                        {
                            groupPurchase.GroupEvents = null;
                        }

                        groupPurchaseList.Add(groupPurchase);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购", ex.ToString());
            throw ex;
        }

        return groupPurchaseList;
    }

    /// <summary>
    /// 加载指定商品所属的所有团购，默认加载团购活动和活动成员
    /// </summary>
    /// <param name="productID">商品ID</param>
    /// <returns></returns>
    public static List<GroupPurchase> FindGroupPurchaseByProductID(int productID)
    {
        return FindGroupPurchaseByProductID(productID, true, true);
    }

    /// <summary>
    /// 加载指定商品所属的所有团购，并指定是否加载团购活动，是否加载团购活动成员
    /// </summary>
    /// <param name="productID">商品ID</param>
    /// <param name="isLoadGroupEvent">是否加载团购活动</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <returns></returns>
    public static List<GroupPurchase> FindGroupPurchaseByProductID(int productID, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        List<GroupPurchase> groupPurchaseList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();
                groupPurchaseList = FindGroupPurchaseByProductID(conn, productID, isLoadGroupEvent, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购", ex.ToString());
            throw ex;
        }

        return groupPurchaseList;
    }

    public static List<GroupPurchase> FindGroupPurchaseByProductID(SqlConnection conn, int productID, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        List<GroupPurchase> groupPurchaseList = new List<GroupPurchase>();

        try
        {
            //根据商品ID，找到对应商品对象，作为所有团购对象的父对象，必须指定不加载团购对象，否则会发送循环调用
            Fruit product = Fruit.FindFruitByID(conn, productID, false);

            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramProductID = cmdGroup.CreateParameter();
                paramProductID.ParameterName = "@ProductID";
                paramProductID.SqlDbType = System.Data.SqlDbType.Int;
                paramProductID.SqlValue = productID;
                cmdGroup.Parameters.Add(paramProductID);

                cmdGroup.CommandText = "select * from GroupPurchase where ProductID = @ProductID";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    GroupPurchase groupPurchase;

                    while (sdr.Read())
                    {
                        groupPurchase = new GroupPurchase();

                        groupPurchase.ID = int.Parse(sdr["Id"].ToString());
                        groupPurchase.Name = sdr["Name"].ToString();
                        groupPurchase.StartDate = DateTime.Parse(sdr["StartDate"].ToString());
                        groupPurchase.EndDate = DateTime.Parse(sdr["EndDate"].ToString());
                        groupPurchase.RequiredNumber = int.Parse(sdr["RequiredNumber"].ToString());
                        groupPurchase.Description = sdr["Description"].ToString();
                        groupPurchase.GroupPrice = decimal.Parse(sdr["GroupPrice"].ToString());
                        groupPurchase.Product = product;
                        if (isLoadGroupEvent)
                        {
                            groupPurchase.GroupEvents = GroupPurchaseEvent.FindGroupPurchaseEventByGroupID(conn, groupPurchase.ID, isLoadGroupEventMember);
                        }
                        else
                        {
                            groupPurchase.GroupEvents = null;
                        }

                        groupPurchaseList.Add(groupPurchase);
                    }

                }

            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return groupPurchaseList;
    }


    /// <summary>
    /// 查询指定商品的最新团购，默认加载团购活动和成员
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    public static GroupPurchase FindActiveGroupPurchase(int productID)
    {
        return FindActiveGroupPurchase(productID, true, true);
    }

    /// <summary>
    /// 查询指定商品的最新团购
    /// </summary>
    /// <param name="productID">商品ID</param>
    /// <param name="isLoadGroupEvent">是否加载团购活动</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <returns></returns>
    public static GroupPurchase FindActiveGroupPurchase(int productID, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        GroupPurchase groupPurchase = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupPurchase = FindActiveGroupPurchase(conn, productID, isLoadGroupEvent, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定商品的最新团购", ex.ToString());
            throw ex;
        }

        return groupPurchase;

    }

    public static GroupPurchase FindActiveGroupPurchase(SqlConnection conn, int productID, bool isLoadGroupEvent, bool isLoadGroupEventMember)
    {
        GroupPurchase groupPurchase = null;

        try
        {
            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramProductID = cmdGroup.CreateParameter();
                paramProductID.ParameterName = "@ProductID";
                paramProductID.SqlDbType = System.Data.SqlDbType.Int;
                paramProductID.SqlValue = productID;
                cmdGroup.Parameters.Add(paramProductID);

                SqlParameter paramNowDate = cmdGroup.CreateParameter();
                paramNowDate.ParameterName = "@NowDate";
                paramNowDate.SqlDbType = System.Data.SqlDbType.DateTime;
                paramNowDate.SqlValue = DateTime.Now;
                cmdGroup.Parameters.Add(paramNowDate);

                cmdGroup.CommandText = "select top 1 * from GroupPurchase where ProductID = @ProductID and (@NowDate >= StartDate and @NowDate <= EndDate) order by Id desc";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        groupPurchase = new GroupPurchase();

                        groupPurchase.ID = int.Parse(sdr["Id"].ToString());
                        groupPurchase.Name = sdr["Name"].ToString();
                        groupPurchase.StartDate = DateTime.Parse(sdr["StartDate"].ToString());
                        groupPurchase.EndDate = DateTime.Parse(sdr["EndDate"].ToString());
                        groupPurchase.RequiredNumber = int.Parse(sdr["RequiredNumber"].ToString());
                        groupPurchase.Description = sdr["Description"].ToString();
                        groupPurchase.GroupPrice = decimal.Parse(sdr["GroupPrice"].ToString());
                        groupPurchase.Product = Fruit.FindFruitByID(conn, productID, false);
                        if (isLoadGroupEvent)
                        {
                            groupPurchase.GroupEvents = GroupPurchaseEvent.FindGroupPurchaseEventByGroupID(conn, groupPurchase.ID, isLoadGroupEventMember);
                        }
                        else
                        {
                            groupPurchase.GroupEvents = null;
                        }

                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定商品的最新团购", ex.ToString());
            throw ex;
        }

        return groupPurchase;

    }

    /// <summary>
    /// 增加团购信息
    /// </summary>
    /// <param name="groupPurchase"></param>
    /// <returns></returns>
    public static GroupPurchase AddGroupPurchase(GroupPurchase groupPurchase)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdGroup = conn.CreateCommand())
                    {
                        cmdGroup.CommandText = "insert into GroupPurchase(Name, StartDate, EndDate, RequiredNumber, Description, ProductID, GroupPrice) values(@Name, @StartDate, @EndDate, @RequiredNumber, @Description, @ProductID, @GroupPrice);select SCOPE_IDENTITY() as 'NewID'";
                        cmdGroup.CommandType = CommandType.Text;

                        SqlParameter paramproductID = cmdGroup.CreateParameter();
                        paramproductID.ParameterName = "@ProductID";
                        paramproductID.SqlDbType = SqlDbType.Int;
                        paramproductID.SqlValue = groupPurchase.Product.ID;
                        cmdGroup.Parameters.Add(paramproductID);

                        SqlParameter paramName = cmdGroup.CreateParameter();
                        paramName.ParameterName = "@Name";
                        paramName.SqlDbType = SqlDbType.NVarChar;
                        paramName.Size = 200;
                        paramName.SqlValue = groupPurchase.Name;
                        cmdGroup.Parameters.Add(paramName);

                        SqlParameter paramStartDate = cmdGroup.CreateParameter();
                        paramStartDate.ParameterName = "@StartDate";
                        paramStartDate.SqlDbType = SqlDbType.DateTime;
                        paramStartDate.SqlValue = groupPurchase.StartDate;
                        cmdGroup.Parameters.Add(paramStartDate);

                        SqlParameter paramEndDate = cmdGroup.CreateParameter();
                        paramEndDate.ParameterName = "@EndDate";
                        paramEndDate.SqlDbType = SqlDbType.DateTime;
                        paramEndDate.SqlValue = groupPurchase.EndDate;
                        cmdGroup.Parameters.Add(paramEndDate);

                        SqlParameter paramRequiredNumber = cmdGroup.CreateParameter();
                        paramRequiredNumber.ParameterName = "@RequiredNumber";
                        paramRequiredNumber.SqlDbType = SqlDbType.Int;
                        paramRequiredNumber.SqlValue = groupPurchase.RequiredNumber;
                        cmdGroup.Parameters.Add(paramRequiredNumber);

                        SqlParameter paramDescription = cmdGroup.CreateParameter();
                        paramDescription.ParameterName = "@Description";
                        paramDescription.SqlDbType = SqlDbType.NVarChar;
                        paramDescription.Size = 4000;
                        paramDescription.SqlValue = groupPurchase.Description;
                        cmdGroup.Parameters.Add(paramDescription);

                        SqlParameter paramGroupPrice = cmdGroup.CreateParameter();
                        paramGroupPrice.ParameterName = "@GroupPrice";
                        paramGroupPrice.SqlDbType = SqlDbType.Decimal;
                        paramGroupPrice.SqlValue = groupPurchase.GroupPrice;
                        cmdGroup.Parameters.Add(paramGroupPrice);

                        var newID = cmdGroup.ExecuteScalar();

                        //新增的ID
                        if (newID != DBNull.Value)
                        {
                            groupPurchase.ID = int.Parse(newID.ToString());
                        }
                        else
                        {
                            throw new Exception("插入团购错误");
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
            Log.Error("AddGroupPurchase", ex.ToString());
            throw ex;
        }

        return groupPurchase;
    }

    /// <summary>
    /// 更新团购信息
    /// </summary>
    /// <param name="groupPurchase"></param>
    public static void UpdateGroupPurchase(GroupPurchase groupPurchase)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdGroup = conn.CreateCommand())
                    {
                        SqlParameter paramId = cmdGroup.CreateParameter();
                        paramId.ParameterName = "@Id";
                        paramId.SqlDbType = System.Data.SqlDbType.Int;
                        paramId.SqlValue = groupPurchase.ID;
                        cmdGroup.Parameters.Add(paramId);

                        SqlParameter paramName = cmdGroup.CreateParameter();
                        paramName.ParameterName = "@Name";
                        paramName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramName.SqlValue = groupPurchase.Name;
                        cmdGroup.Parameters.Add(paramName);

                        SqlParameter paramStartDate = cmdGroup.CreateParameter();
                        paramStartDate.ParameterName = "@StartDate";
                        paramStartDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramStartDate.SqlValue = groupPurchase.StartDate;
                        cmdGroup.Parameters.Add(paramStartDate);

                        SqlParameter paramEndDate = cmdGroup.CreateParameter();
                        paramEndDate.ParameterName = "@EndDate";
                        paramEndDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramEndDate.SqlValue = groupPurchase.EndDate;
                        cmdGroup.Parameters.Add(paramEndDate);

                        SqlParameter paramRequiredNumber = cmdGroup.CreateParameter();
                        paramRequiredNumber.ParameterName = "@RequiredNumber";
                        paramRequiredNumber.SqlDbType = System.Data.SqlDbType.Int;
                        paramRequiredNumber.SqlValue = groupPurchase.RequiredNumber;
                        cmdGroup.Parameters.Add(paramRequiredNumber);

                        SqlParameter paramDescription = cmdGroup.CreateParameter();
                        paramDescription.ParameterName = "@Description";
                        paramDescription.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramDescription.SqlValue = groupPurchase.Description;
                        cmdGroup.Parameters.Add(paramDescription);

                        SqlParameter paramGroupPrice = cmdGroup.CreateParameter();
                        paramGroupPrice.ParameterName = "@GroupPrice";
                        paramGroupPrice.SqlDbType = SqlDbType.Decimal;
                        paramGroupPrice.SqlValue = groupPurchase.GroupPrice;
                        cmdGroup.Parameters.Add(paramGroupPrice);

                        foreach (SqlParameter param in cmdGroup.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdGroup.CommandText = "update GroupPurchase set Name = @Name, StartDate = @StartDate, EndDate = @EndDate, RequiredNumber = @RequiredNumber, Description = @Description, GroupPrice = @GroupPrice where Id = @Id";

                        if (cmdGroup.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("更新团购失败");
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("更新一个团购", ex.ToString());
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
            Log.Error("更新一个团购", ex.ToString());
            throw ex;
        }

    }

    /// <summary>
    /// 删除指定的团购，如果此团购下已有团购活动，则不能删除
    /// </summary>
    /// <param name="id">团购ID</param>
    public void DelGroupPurchase(int id)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    //如果已有人参加此团购，则不能删除团购
                    List<GroupPurchaseEvent> groupEvents = GroupPurchaseEvent.FindGroupPurchaseEventByGroupID(id, false);
                    if (groupEvents.Count != 0)
                    {
                        throw new Exception("已有人参加此团购，不能删除。");
                    }
                    else
                    {
                        using (SqlCommand cmdDelGroup = conn.CreateCommand())
                        {
                            SqlParameter paramID = cmdDelGroup.CreateParameter();
                            paramID.ParameterName = "@Id";
                            paramID.SqlDbType = System.Data.SqlDbType.Int;
                            paramID.SqlValue = id;
                            cmdDelGroup.Parameters.Add(paramID);

                            cmdDelGroup.CommandText = "delete from GroupPurchase where Id = @Id";

                            if (cmdDelGroup.ExecuteNonQuery() != 1)
                            {
                                throw new Exception("删除团购失败");
                            }
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
            Log.Error("删除一个团购", ex.ToString());
            throw ex;
        }

    }
}