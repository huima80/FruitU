using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Security;
using LitJson;

/// <summary>
/// WeChatUserDAO 的摘要说明
/// </summary>
public static class WeChatUserDAO
{
    /// <summary>
    /// 查询总记录数
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="strWhere"></param>
    /// <returns></returns>
    public static int FindUserCount(string tableName, string strWhere)
    {
        int totalRows = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUserCount = conn.CreateCommand())
                    {
                        if (string.IsNullOrEmpty(strWhere))
                        {
                            cmdUserCount.CommandText = string.Format("select count(*) from {0}", tableName);
                        }
                        else
                        {
                            cmdUserCount.CommandText = string.Format("select count(*) from {0} where {1}", tableName, strWhere);
                        }

                        Log.Debug("FindUserCount", cmdUserCount.CommandText);

                        totalRows = int.Parse(cmdUserCount.ExecuteScalar().ToString());

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
            Log.Error("FindUserCount", ex.ToString());
            throw ex;
        }

        return totalRows;
    }

    /// <summary>
    /// 分页查询，指定：表名、主键、字段名、条件字句、排序字句、起始行数、每页行数、总记录数（out）
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="pk"></param>
    /// <param name="fieldsName"></param>
    /// <param name="strWhere"></param>
    /// <param name="strOrder"></param>
    /// <param name="totalRecords"></param>
    /// <param name="startRowIndex"></param>
    /// <param name="maximumRows"></param>
    /// <returns></returns>
    public static List<WeChatUser> FindUsersPager(string tableName, string pk, string fieldsName, string strWhere, string strOrder, out int totalRecords, int startRowIndex, int maximumRows = 10)
    {
        List<WeChatUser> userPerPage = new List<WeChatUser>();
        WeChatUser wxUser;

        totalRecords = 0;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUser = conn.CreateCommand())
                    {
                        cmdUser.CommandText = "spSqlPageByRowNum";
                        cmdUser.CommandType = CommandType.StoredProcedure;

                        SqlParameter paramTableName = cmdUser.CreateParameter();
                        paramTableName.ParameterName = "@tbName";
                        paramTableName.SqlDbType = SqlDbType.VarChar;
                        paramTableName.Size = 255;
                        paramTableName.Direction = ParameterDirection.Input;
                        paramTableName.SqlValue = tableName;
                        cmdUser.Parameters.Add(paramTableName);

                        SqlParameter paramPK = cmdUser.CreateParameter();
                        paramPK.ParameterName = "@PK";
                        paramPK.SqlDbType = SqlDbType.VarChar;
                        paramPK.Size = 50;
                        paramPK.Direction = ParameterDirection.Input;
                        paramPK.SqlValue = pk;
                        cmdUser.Parameters.Add(paramPK);

                        SqlParameter paramFields = cmdUser.CreateParameter();
                        paramFields.ParameterName = "@tbFields";
                        paramFields.SqlDbType = SqlDbType.VarChar;
                        paramFields.Size = 1000;
                        paramFields.Direction = ParameterDirection.Input;
                        paramFields.SqlValue = fieldsName;
                        cmdUser.Parameters.Add(paramFields);

                        SqlParameter paramWhere = cmdUser.CreateParameter();
                        paramWhere.ParameterName = "@strWhere";
                        paramWhere.SqlDbType = SqlDbType.VarChar;
                        paramWhere.Size = 1000;
                        paramWhere.Direction = ParameterDirection.Input;
                        paramWhere.SqlValue = strWhere;
                        cmdUser.Parameters.Add(paramWhere);

                        SqlParameter paramOrder = cmdUser.CreateParameter();
                        paramOrder.ParameterName = "@strOrder";
                        paramOrder.SqlDbType = SqlDbType.VarChar;
                        paramOrder.Size = 1000;
                        paramOrder.Direction = ParameterDirection.Input;
                        paramOrder.SqlValue = strOrder;
                        cmdUser.Parameters.Add(paramOrder);

                        SqlParameter paramMaximumRows = cmdUser.CreateParameter();
                        paramMaximumRows.ParameterName = "@MaximumRows";
                        paramMaximumRows.SqlDbType = SqlDbType.Int;
                        paramMaximumRows.Direction = ParameterDirection.Input;
                        paramMaximumRows.SqlValue = maximumRows;
                        cmdUser.Parameters.Add(paramMaximumRows);

                        SqlParameter paramStartRowIndex = cmdUser.CreateParameter();
                        paramStartRowIndex.ParameterName = "@StartRowIndex";
                        paramStartRowIndex.SqlDbType = SqlDbType.Int;
                        paramStartRowIndex.Direction = ParameterDirection.Input;
                        paramStartRowIndex.SqlValue = startRowIndex;
                        cmdUser.Parameters.Add(paramStartRowIndex);

                        SqlParameter paramTotalRows = cmdUser.CreateParameter();
                        paramTotalRows.ParameterName = "@TotalRows";
                        paramTotalRows.SqlDbType = SqlDbType.Int;
                        paramTotalRows.Direction = ParameterDirection.Output;
                        cmdUser.Parameters.Add(paramTotalRows);

                        foreach (SqlParameter param in cmdUser.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        using (SqlDataReader sdrUser = cmdUser.ExecuteReader())
                        {
                            while (sdrUser.Read())
                            {
                                //加载此用户的成员资格信息
                                MembershipUser mUser = Membership.GetUser(sdrUser["OpenID"].ToString());
                                if (mUser != null)
                                {
                                    //使用成员资格对象初始化微信用户对象，并追加微信用户信息表数据
                                    wxUser = new WeChatUser(mUser);
                                    wxUser.ID = int.Parse(sdrUser["Id"].ToString());
                                    wxUser.OpenID = sdrUser["OpenID"].ToString();
                                    wxUser.NickName = sdrUser["NickName"].ToString();
                                    wxUser.Sex = sdrUser["Sex"] != DBNull.Value ? bool.Parse(sdrUser["Sex"].ToString()) : true;
                                    wxUser.Country = sdrUser["Country"].ToString();
                                    wxUser.Province = sdrUser["Province"].ToString();
                                    wxUser.City = sdrUser["City"].ToString();
                                    wxUser.HeadImgUrl = sdrUser["HeadImgUrl"].ToString();
                                    wxUser.Privilege = sdrUser["Privilege"].ToString();
                                    wxUser.ClientIP = sdrUser["ClientIP"].ToString();
                                    wxUser.IsSubscribe = sdrUser["IsSubscribe"] != null ? bool.Parse(sdrUser["IsSubscribe"].ToString()) : false;
                                    wxUser.OrderCount = int.Parse(sdrUser["OrderCount"].ToString());

                                    userPerPage.Add(wxUser);
                                }
                            }
                            sdrUser.Close();
                        }

                        if (!int.TryParse(paramTotalRows.SqlValue.ToString(), out totalRecords))
                        {
                            totalRecords = 0;
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
            Log.Error("FindUsersPager", ex.ToString());
            throw ex;
        }

        return userPerPage;
    }

    public static WeChatUser FindUserByUserID(Guid userID)
    {
        WeChatUser wxUser = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUser = conn.CreateCommand())
                    {
                        SqlParameter paramUserID = cmdUser.CreateParameter();
                        paramUserID.ParameterName = "@UserId";
                        paramUserID.SqlDbType = System.Data.SqlDbType.UniqueIdentifier;
                        paramUserID.SqlValue = userID;
                        cmdUser.Parameters.Add(paramUserID);

                        cmdUser.CommandText = "select WeChatUsers.*,(select count(*) from ProductOrder where WeChatUsers.OpenID = ProductOrder.OpenID) as OrderCount from WeChatUsers where UserId = @UserId";

                        using (SqlDataReader sdrUser = cmdUser.ExecuteReader())
                        {
                            while (sdrUser.Read())
                            {
                                //加载此用户的成员资格信息
                                MembershipUser mUser = Membership.GetUser(sdrUser["OpenID"].ToString());
                                if (mUser != null)
                                {
                                    //使用新建的成员资格对象初始化微信用户对象
                                    wxUser = new WeChatUser(mUser);
                                }
                                else
                                {
                                    wxUser = new WeChatUser();
                                }

                                wxUser.ID = int.Parse(sdrUser["Id"].ToString());
                                wxUser.OpenID = sdrUser["OpenID"].ToString();
                                wxUser.NickName = sdrUser["NickName"].ToString();
                                wxUser.Sex = sdrUser["Sex"] != DBNull.Value ? bool.Parse(sdrUser["Sex"].ToString()) : true;
                                wxUser.Country = sdrUser["Country"].ToString();
                                wxUser.Province = sdrUser["Province"].ToString();
                                wxUser.City = sdrUser["City"].ToString();
                                wxUser.HeadImgUrl = sdrUser["HeadImgUrl"].ToString();
                                wxUser.Privilege = sdrUser["Privilege"].ToString();
                                wxUser.ClientIP = sdrUser["ClientIP"].ToString();
                                wxUser.IsSubscribe = sdrUser["IsSubscribe"] != null ? bool.Parse(sdrUser["IsSubscribe"].ToString()) : false;

                            }
                            sdrUser.Close();
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
            Log.Error("FindUserByUserID", ex.ToString());
            throw ex;
        }

        return wxUser;
    }

    public static WeChatUser FindUserByOpenID(string openID)
    {
        WeChatUser wxUser = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUser = conn.CreateCommand())
                    {
                        SqlParameter paramUserID = cmdUser.CreateParameter();
                        paramUserID.ParameterName = "@OpenID";
                        paramUserID.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramUserID.SqlValue = openID;
                        cmdUser.Parameters.Add(paramUserID);

                        cmdUser.CommandText = "select WeChatUsers.*,(select count(*) from ProductOrder where WeChatUsers.OpenID = ProductOrder.OpenID) as OrderCount from WeChatUsers where OpenID = @OpenID";

                        using (SqlDataReader sdrUser = cmdUser.ExecuteReader())
                        {
                            while (sdrUser.Read())
                            {
                                //加载此用户的成员资格信息
                                MembershipUser mUser = Membership.GetUser(sdrUser["OpenID"].ToString(), true);
                                if (mUser != null)
                                {
                                    //使用新建的成员资格对象初始化微信用户对象
                                    wxUser = new WeChatUser(mUser);
                                }
                                else
                                {
                                    wxUser = new WeChatUser();
                                }

                                wxUser.ID = int.Parse(sdrUser["Id"].ToString());
                                wxUser.OpenID = sdrUser["OpenID"].ToString();
                                wxUser.NickName = sdrUser["NickName"].ToString();
                                wxUser.Sex = sdrUser["Sex"] != DBNull.Value ? bool.Parse(sdrUser["Sex"].ToString()) : true;
                                wxUser.Country = sdrUser["Country"].ToString();
                                wxUser.Province = sdrUser["Province"].ToString();
                                wxUser.City = sdrUser["City"].ToString();
                                wxUser.HeadImgUrl = sdrUser["HeadImgUrl"].ToString();
                                wxUser.Privilege = sdrUser["Privilege"].ToString();
                                wxUser.ClientIP = sdrUser["ClientIP"].ToString();
                                wxUser.IsSubscribe = sdrUser["IsSubscribe"] != null ? bool.Parse(sdrUser["IsSubscribe"].ToString()) : false;
                                wxUser.OrderCount = int.Parse(sdrUser["OrderCount"].ToString());

                            }
                            sdrUser.Close();
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
            Log.Error("FindUserByOpenID", ex.ToString());
            throw ex;
        }

        return wxUser;
    }

    public static List<WeChatUser> FindUsersByNickName(string nickName)
    {
        List<WeChatUser> userList = new List<WeChatUser>();
        WeChatUser wxUser = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUser = conn.CreateCommand())
                    {
                        SqlParameter paramNickName = cmdUser.CreateParameter();
                        paramNickName.ParameterName = "@NickName";
                        paramNickName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramNickName.SqlValue = nickName;
                        cmdUser.Parameters.Add(paramNickName);

                        cmdUser.CommandText = "select WeChatUsers.*,(select count(*) from ProductOrder where WeChatUsers.OpenID = ProductOrder.OpenID) as OrderCount from WeChatUsers where NickName like '%@NickName%'";

                        using (SqlDataReader sdrUser = cmdUser.ExecuteReader())
                        {
                            while (sdrUser.Read())
                            {
                                //加载此用户的成员资格信息
                                MembershipUser mUser = Membership.GetUser(sdrUser["OpenID"].ToString());
                                if (mUser != null)
                                {
                                    //使用新建的成员资格对象初始化微信用户对象
                                    wxUser = new WeChatUser(mUser);
                                }
                                else
                                {
                                    wxUser = new WeChatUser();
                                }

                                wxUser.ID = int.Parse(sdrUser["Id"].ToString());
                                wxUser.OpenID = sdrUser["OpenID"].ToString();
                                wxUser.NickName = sdrUser["NickName"].ToString();
                                wxUser.Sex = sdrUser["Sex"] != DBNull.Value ? bool.Parse(sdrUser["Sex"].ToString()) : true;
                                wxUser.Country = sdrUser["Country"].ToString();
                                wxUser.Province = sdrUser["Province"].ToString();
                                wxUser.City = sdrUser["City"].ToString();
                                wxUser.HeadImgUrl = sdrUser["HeadImgUrl"].ToString();
                                wxUser.Privilege = sdrUser["Privilege"].ToString();
                                wxUser.ClientIP = sdrUser["ClientIP"].ToString();
                                wxUser.IsSubscribe = sdrUser["IsSubscribe"] != null ? bool.Parse(sdrUser["IsSubscribe"].ToString()) : false;

                                userList.Add(wxUser);
                            }
                            sdrUser.Close();
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
            Log.Error("FindUsersByNickName", ex.ToString());
            throw ex;
        }

        return userList;
    }

    public static void InsertUser(WeChatUser user)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdInserUser = conn.CreateCommand())
                    {
                        SqlParameter paramUserID = cmdInserUser.CreateParameter();
                        paramUserID.ParameterName = "@UserId";
                        paramUserID.SqlDbType = System.Data.SqlDbType.UniqueIdentifier;
                        paramUserID.SqlValue = user.ProviderUserKey;
                        cmdInserUser.Parameters.Add(paramUserID);

                        SqlParameter paramOpenID = cmdInserUser.CreateParameter();
                        paramOpenID.ParameterName = "@OpenID";
                        paramOpenID.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramOpenID.Size = 50;
                        paramOpenID.SqlValue = user.OpenID;
                        cmdInserUser.Parameters.Add(paramOpenID);

                        SqlParameter paramNickName = cmdInserUser.CreateParameter();
                        paramNickName.ParameterName = "@NickName";
                        paramNickName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramNickName.Size = 256;
                        paramNickName.SqlValue = user.NickName;
                        cmdInserUser.Parameters.Add(paramNickName);

                        SqlParameter paramSex = cmdInserUser.CreateParameter();
                        paramSex.ParameterName = "@Sex";
                        paramSex.SqlDbType = System.Data.SqlDbType.Bit;
                        paramSex.SqlValue = user.Sex;
                        cmdInserUser.Parameters.Add(paramSex);

                        SqlParameter paramCountry = cmdInserUser.CreateParameter();
                        paramCountry.ParameterName = "@Country";
                        paramCountry.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramCountry.SqlValue = user.Country;
                        cmdInserUser.Parameters.Add(paramCountry);

                        SqlParameter paramProvince = cmdInserUser.CreateParameter();
                        paramProvince.ParameterName = "@Province";
                        paramProvince.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramProvince.SqlValue = user.Province;
                        cmdInserUser.Parameters.Add(paramProvince);

                        SqlParameter paramCity = cmdInserUser.CreateParameter();
                        paramCity.ParameterName = "@City";
                        paramCity.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramCity.SqlValue = user.City;
                        cmdInserUser.Parameters.Add(paramCity);

                        SqlParameter paramHeadImgUrl = cmdInserUser.CreateParameter();
                        paramHeadImgUrl.ParameterName = "@HeadImgUrl";
                        paramHeadImgUrl.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramHeadImgUrl.SqlValue = user.HeadImgUrl;
                        cmdInserUser.Parameters.Add(paramHeadImgUrl);

                        SqlParameter paramPrivilege = cmdInserUser.CreateParameter();
                        paramPrivilege.ParameterName = "@Privilege";
                        paramPrivilege.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramPrivilege.SqlValue = user.Privilege;
                        cmdInserUser.Parameters.Add(paramPrivilege);

                        SqlParameter paramClientIP = cmdInserUser.CreateParameter();
                        paramClientIP.ParameterName = "@ClientIP";
                        paramClientIP.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramClientIP.SqlValue = user.ClientIP;
                        cmdInserUser.Parameters.Add(paramClientIP);

                        SqlParameter paramIsSubscribe = cmdInserUser.CreateParameter();
                        paramIsSubscribe.ParameterName = "@IsSubscribe";
                        paramIsSubscribe.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsSubscribe.SqlValue = user.IsSubscribe;
                        cmdInserUser.Parameters.Add(paramIsSubscribe);

                        foreach (SqlParameter param in cmdInserUser.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdInserUser.CommandText = "insert into WeChatUsers(UserId,OpenID,NickName,Sex,Country,Province,City,HeadImgUrl,Privilege,ClientIP,IsSubscribe) values(@UserId,@OpenID,@NickName,@Sex,@Country,@Province,@City,@HeadImgUrl,@Privilege,@ClientIP,@IsSubscribe);select SCOPE_IDENTITY() as 'NewID'";

                        var newID = cmdInserUser.ExecuteScalar();

                        //新增的商品ID
                        if (newID != DBNull.Value)
                        {
                            user.ID = int.Parse(newID.ToString());
                        }
                        else
                        {
                            throw new Exception("插入记录错误");
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("InsertUser", ex.ToString());
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

    }

    public static void UpdateUser(WeChatUser user)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUpdateUser = conn.CreateCommand())
                    {
                        SqlParameter paramID = cmdUpdateUser.CreateParameter();
                        paramID.ParameterName = "@Id";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = user.ID;
                        cmdUpdateUser.Parameters.Add(paramID);

                        SqlParameter paramUserID = cmdUpdateUser.CreateParameter();
                        paramUserID.ParameterName = "@UserId";
                        paramUserID.SqlDbType = System.Data.SqlDbType.UniqueIdentifier;
                        paramUserID.SqlValue = user.ProviderUserKey;
                        cmdUpdateUser.Parameters.Add(paramUserID);

                        SqlParameter paramOpenID = cmdUpdateUser.CreateParameter();
                        paramOpenID.ParameterName = "@OpenID";
                        paramOpenID.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramOpenID.SqlValue = user.OpenID;
                        cmdUpdateUser.Parameters.Add(paramOpenID);

                        SqlParameter paramNickName = cmdUpdateUser.CreateParameter();
                        paramNickName.ParameterName = "@NickName";
                        paramNickName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramNickName.SqlValue = user.NickName;
                        cmdUpdateUser.Parameters.Add(paramNickName);

                        SqlParameter paramSex = cmdUpdateUser.CreateParameter();
                        paramSex.ParameterName = "@Sex";
                        paramSex.SqlDbType = System.Data.SqlDbType.Bit;
                        paramSex.SqlValue = user.Sex;
                        cmdUpdateUser.Parameters.Add(paramSex);

                        SqlParameter paramCountry = cmdUpdateUser.CreateParameter();
                        paramCountry.ParameterName = "@Country";
                        paramCountry.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramCountry.SqlValue = user.Country;
                        cmdUpdateUser.Parameters.Add(paramCountry);

                        SqlParameter paramProvince = cmdUpdateUser.CreateParameter();
                        paramProvince.ParameterName = "@Province";
                        paramProvince.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramProvince.SqlValue = user.Province;
                        cmdUpdateUser.Parameters.Add(paramProvince);

                        SqlParameter paramCity = cmdUpdateUser.CreateParameter();
                        paramCity.ParameterName = "@City";
                        paramCity.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramCity.SqlValue = user.City;
                        cmdUpdateUser.Parameters.Add(paramCity);

                        SqlParameter paramHeadImgUrl = cmdUpdateUser.CreateParameter();
                        paramHeadImgUrl.ParameterName = "@HeadImgUrl";
                        paramHeadImgUrl.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramHeadImgUrl.SqlValue = user.HeadImgUrl;
                        cmdUpdateUser.Parameters.Add(paramHeadImgUrl);

                        SqlParameter paramPrivilege = cmdUpdateUser.CreateParameter();
                        paramPrivilege.ParameterName = "@Privilege";
                        paramPrivilege.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramPrivilege.SqlValue = user.Privilege;
                        cmdUpdateUser.Parameters.Add(paramPrivilege);

                        SqlParameter paramClientIP = cmdUpdateUser.CreateParameter();
                        paramClientIP.ParameterName = "@ClientIP";
                        paramClientIP.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramClientIP.SqlValue = user.ClientIP;
                        cmdUpdateUser.Parameters.Add(paramClientIP);

                        SqlParameter paramIsSubscribe = cmdUpdateUser.CreateParameter();
                        paramIsSubscribe.ParameterName = "@IsSubscribe";
                        paramIsSubscribe.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsSubscribe.SqlValue = user.IsSubscribe;
                        cmdUpdateUser.Parameters.Add(paramIsSubscribe);

                        foreach (SqlParameter param in cmdUpdateUser.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdUpdateUser.CommandText = "update WeChatUsers set UserId = @UserId,OpenID = @OpenID,NickName = @NickName,Sex = @Sex,Country = @Country,Province = @Province,City=@City,HeadImgUrl=@HeadImgUrl,Privilege=@Privilege,ClientIP=@ClientIP,IsSubscribe=@IsSubscribe where Id = @Id";

                        if (cmdUpdateUser.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("更新记录失败");
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("UpdateUser", ex.ToString());
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
            Log.Error("UpdateUser", ex.ToString());
            throw ex;
        }
    }

    public static bool DeleteUserByOpenID(string openID)
    {
        bool isDel = false;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdDelUser = conn.CreateCommand())
                    {
                        SqlParameter paramOpenID = cmdDelUser.CreateParameter();
                        paramOpenID.ParameterName = "@OpenID";
                        paramOpenID.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramOpenID.SqlValue = openID;
                        cmdDelUser.Parameters.Add(paramOpenID);

                        cmdDelUser.CommandText = "delete from WeChatUsers where OpenID = @OpenID";

                        if (cmdDelUser.ExecuteNonQuery() != -1)
                        {
                            isDel = true;
                        }
                        else
                        {
                            isDel = false;
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
            Log.Error("DeleteUserByOpenID", ex.ToString());
            throw ex;
        }

        return isDel;
    }

    public static bool DeleteUserByUserID(Guid userID)
    {
        bool isDel = false;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdDelUser = conn.CreateCommand())
                    {
                        SqlParameter paramUserID = cmdDelUser.CreateParameter();
                        paramUserID.ParameterName = "@UserId";
                        paramUserID.SqlDbType = System.Data.SqlDbType.UniqueIdentifier;
                        paramUserID.SqlValue = userID;
                        cmdDelUser.Parameters.Add(paramUserID);

                        cmdDelUser.CommandText = "delete from WeChatUsers where UserId = @UserId";

                        if (cmdDelUser.ExecuteNonQuery() == 1)
                        {
                            isDel = true;
                        }
                        else
                        {
                            isDel = false;
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
            Log.Error("DeleteUserByUserID", ex.ToString());
            throw ex;
        }

        return isDel;
    }

    /// <summary>
    /// 获取微信用户基本信息
    /// API参考：https://mp.weixin.qq.com/wiki/14/bb5031008f1494a59c6f71fa0f319c66.html
    /// </summary>
    /// <param name="wxUsers"></param>
    public static void RefreshWxUserInfo(List<WeChatUser> wxUsers)
    {
        string wxUserInfoAPI = string.Format(@"https://api.weixin.qq.com/cgi-bin/user/info/batchget?access_token={0}", WxJSAPI.GetAccessToken());
        string recvMsg;
        JsonData jUserList = new JsonData(), jUserInfoList = new JsonData(), jUser;

        jUserList["user_list"] = new JsonData();
        for (int i = 0; i < wxUsers.Count; i++)
        {
            jUser = new JsonData();
            jUser["openid"] = wxUsers[i].OpenID;
            jUserList["user_list"].Add(jUser);

            //一次最多拉取100个用户的微信信息
            if (jUserList["user_list"].Count == 100 || i == (wxUsers.Count - 1))
            {
                recvMsg = HttpService.Post(jUserList.ToJson(), wxUserInfoAPI, false, Config.WeChatAPITimeout);
                jUserInfoList = JsonMapper.ToObject(recvMsg);

                //用获取的信息刷新形参
                if (jUserInfoList != null && jUserInfoList.Keys.Contains("user_info_list") && jUserInfoList["user_info_list"].IsArray)
                {
                    //遍历拉取的微信用户JSON消息包，并刷新形参
                    for (int j = 0; j < jUserInfoList["user_info_list"].Count; j++)
                    {
                        if (jUserInfoList["user_info_list"][j].Keys.Contains("subscribe") && jUserInfoList["user_info_list"][j].Keys.Contains("openid"))
                        {
                            WeChatUser user = wxUsers.Find(delegate (WeChatUser wxUser)
                            {
                                if (wxUser.OpenID == jUserInfoList["user_info_list"][j]["openid"].ToString())
                                {
                                    return true;
                                }
                                return false;
                            });

                            if (user != default(WeChatUser))
                            {
                                //此用户关注公众号，则返回详细信息
                                if (jUserInfoList["user_info_list"][j]["subscribe"].ToString() == "1")
                                {
                                    user.IsSubscribe = true;
                                    user.NickName = jUserInfoList["user_info_list"][j].Keys.Contains("nickname") ? jUserInfoList["user_info_list"][j]["nickname"].ToString() : user.NickName;
                                    user.Sex = jUserInfoList["user_info_list"][j].Keys.Contains("sex") ? (jUserInfoList["user_info_list"][j]["sex"].ToString() == "1" ? true : false) : user.Sex;
                                    user.Country = jUserInfoList["user_info_list"][j].Keys.Contains("country") ? jUserInfoList["user_info_list"][j]["country"].ToString() : user.Country;
                                    user.Province = jUserInfoList["user_info_list"][j].Keys.Contains("province") ? jUserInfoList["user_info_list"][j]["province"].ToString() : user.Province;
                                    user.City = jUserInfoList["user_info_list"][j].Keys.Contains("city") ? jUserInfoList["user_info_list"][j]["city"].ToString() : user.City;
                                    user.HeadImgUrl = jUserInfoList["user_info_list"][j].Keys.Contains("headimgurl") ? jUserInfoList["user_info_list"][j]["headimgurl"].ToString() : user.HeadImgUrl;
                                    user.UnionID = jUserInfoList["user_info_list"][j].Keys.Contains("unionid") ? jUserInfoList["user_info_list"][j]["unionid"].ToString() : user.UnionID;
                                }
                                else
                                {
                                    user.IsSubscribe = false;
                                    user.UnionID = jUserInfoList["user_info_list"][j].Keys.Contains("unionid") ? jUserInfoList["user_info_list"][j]["unionid"].ToString() : user.UnionID;
                                }
                            }
                        }
                    }
                }

                jUserList.Clear();
            }
        }

    }

}