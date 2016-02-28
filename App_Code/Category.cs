using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using LitJson;

/// <summary>
/// 产品类别
/// </summary>
public class Category
{
    /// <summary>
    /// 类别ID
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// 父节点ID
    /// </summary>
    public int ParentID { get; set; }

    /// <summary>
    /// 类别名称
    /// </summary>
    public string CategoryName { get; set; }

    public Category()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    /// <summary>
    /// 查询所有产品目录
    /// </summary>
    /// <returns></returns>
    public static List<Category> FindAllCategory()
    {

        List<Category> categoryList = new List<Category>();
        Category category;

        try
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FruitU"].ToString();

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("没有配置数据库链接字符串");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdCategory = conn.CreateCommand())
                    {
                        cmdCategory.CommandText = "select * from Category";

                        using (SqlDataReader sdrCategory = cmdCategory.ExecuteReader())
                        {
                            while (sdrCategory.Read())
                            {
                                category = new Category();

                                category.ID = int.Parse(sdrCategory["Id"].ToString());
                                category.ParentID = int.Parse(sdrCategory["ParentID"].ToString());
                                category.CategoryName = sdrCategory["CategoryName"].ToString();

                                categoryList.Add(category);

                            }
                            sdrCategory.Close();
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
            Log.Error("查询所有产品目录", ex.ToString());
            throw ex;
        }

        return categoryList;

    }

    /// <summary>
    /// 根据ID查询商品类别
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Category FindCategoryByID(int id)
    {
        Category category = new Category();

        try
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FruitU"].ToString();

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("没有配置数据库链接字符串");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdCategory = conn.CreateCommand())
                    {
                        SqlParameter paramID = cmdCategory.CreateParameter();
                        paramID.ParameterName = "@Id";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = id;
                        cmdCategory.Parameters.Add(paramID);

                        cmdCategory.CommandText = "select * from Category where Id=@Id";

                        using (SqlDataReader sdrCategory = cmdCategory.ExecuteReader())
                        {
                            while (sdrCategory.Read())
                            {
                                category.ID = int.Parse(sdrCategory["Id"].ToString());
                                category.ParentID = int.Parse(sdrCategory["ParentID"].ToString());
                                category.CategoryName = sdrCategory["CategoryName"].ToString();

                            }
                            sdrCategory.Close();
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
            Log.Error("根据ID查询商品类别", ex.ToString());
            throw ex;
        }

        return category;

    }

    /// <summary>
    /// 查询指定的目录ID是否有子目录
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static bool HasSubCategoryByID(int id)
    {
        try
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FruitU"].ToString();

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("没有配置数据库链接字符串");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdCategory = conn.CreateCommand())
                    {
                        SqlParameter paramID = cmdCategory.CreateParameter();
                        paramID.ParameterName = "@Id";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = id;
                        cmdCategory.Parameters.Add(paramID);

                        cmdCategory.CommandText = "select count(*) from Category where ParentID=@Id";

                        var categoryCount = cmdCategory.ExecuteScalar();

                        if (categoryCount != DBNull.Value)
                        {
                            if (int.Parse(categoryCount.ToString()) > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
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
            Log.Error("根据ID查询是否有子类别", ex.ToString());
            throw ex;
        }
    }

    public static Category AddCategory(Category category)
    {
        try
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FruitU"].ToString();

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("没有配置数据库链接字符串");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdAddCategory = conn.CreateCommand())
                    {
                        SqlParameter paramParentID = cmdAddCategory.CreateParameter();
                        paramParentID.ParameterName = "@ParentID";
                        paramParentID.SqlDbType = System.Data.SqlDbType.Int;
                        paramParentID.SqlValue = category.ParentID;
                        cmdAddCategory.Parameters.Add(paramParentID);

                        SqlParameter paramCategoryName = cmdAddCategory.CreateParameter();
                        paramCategoryName.ParameterName = "@CategoryName";
                        paramCategoryName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramCategoryName.Size = 50;
                        paramCategoryName.SqlValue = category.CategoryName;
                        cmdAddCategory.Parameters.Add(paramCategoryName);

                        foreach (SqlParameter param in cmdAddCategory.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdAddCategory.CommandText = "insert into Category(ParentID,CategoryName) values(@ParentID,@CategoryName);select SCOPE_IDENTITY() as 'NewCategoryID'";

                        var NewCategoryID = cmdAddCategory.ExecuteScalar();

                        //新增的商品目录ID
                        if (NewCategoryID != DBNull.Value)
                        {
                            category.ID = int.Parse(NewCategoryID.ToString());
                        }
                        else
                        {
                            throw new Exception("插入商品目录表错误");
                        }
                    }

                }
                catch (Exception ex)
                {
                    Log.Error("增加一个目录错误", ex.ToString());
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

        return category;
    }

    public static void UpdateCategory(Category category)
    {
        try
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FruitU"].ToString();

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("没有配置数据库链接字符串");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdUpdateCategory = conn.CreateCommand())
                    {
                        SqlParameter paramID = cmdUpdateCategory.CreateParameter();
                        paramID.ParameterName = "@Id";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = category.ID;
                        cmdUpdateCategory.Parameters.Add(paramID);

                        SqlParameter paramParentID = cmdUpdateCategory.CreateParameter();
                        paramParentID.ParameterName = "@ParentID";
                        paramParentID.SqlDbType = System.Data.SqlDbType.Int;
                        paramParentID.SqlValue = category.ParentID;
                        cmdUpdateCategory.Parameters.Add(paramParentID);

                        SqlParameter paramCategoryName = cmdUpdateCategory.CreateParameter();
                        paramCategoryName.ParameterName = "@CategoryName";
                        paramCategoryName.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramCategoryName.Size = 50;
                        paramCategoryName.SqlValue = category.CategoryName;
                        cmdUpdateCategory.Parameters.Add(paramCategoryName);

                        foreach (SqlParameter param in cmdUpdateCategory.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        cmdUpdateCategory.CommandText = "update Category set ParentID = @ParentID,CategoryName = @CategoryName where Id = @Id";

                        if (cmdUpdateCategory.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("更新商品目录失败");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("更新一个目录", ex.ToString());
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
            Log.Error("更新一个目录", ex.ToString());
            throw ex;
        }
    }

    public static void DelCategory(int id)
    {
        try
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["FruitU"].ToString();

            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("没有配置数据库链接字符串");
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                try
                {
                    using (SqlCommand cmdDelCategory = conn.CreateCommand())
                    {
                        SqlParameter paramID = cmdDelCategory.CreateParameter();
                        paramID.ParameterName = "@Id";
                        paramID.SqlDbType = System.Data.SqlDbType.Int;
                        paramID.SqlValue = id;
                        cmdDelCategory.Parameters.Add(paramID);

                        cmdDelCategory.CommandText = "delete from Category where Id = @Id";

                        cmdDelCategory.ExecuteNonQuery();
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
            Log.Error("删除一个目录", ex.ToString());
            throw ex;
        }

    }

}