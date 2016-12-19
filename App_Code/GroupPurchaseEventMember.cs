using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// 团购活动参与者
/// </summary>
public class GroupPurchaseEventMember
{
    public int ID { get; set; }

    /// <summary>
    /// 参与者所属团购活动
    /// </summary>
    public GroupPurchaseEvent GroupPurchaseEvent { get; set; }

    /// <summary>
    /// 团购参与者
    /// </summary>
    public WeChatUser GroupMember { get; set; }

    /// <summary>
    /// 团购参与者日期
    /// </summary>
    public DateTime JoinDate { get; set; }

    public GroupPurchaseEventMember()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    /// <summary>
    /// 查询指定团购活动中的所有用户
    /// </summary>
    /// <param name="groupEventID"></param>
    /// <returns></returns>
    public static List<GroupPurchaseEventMember> FindGroupPurchaseEventMembers(int groupEventID)
    {
        List<GroupPurchaseEventMember> groupEventMemberList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupEventMemberList = FindGroupPurchaseEventMembers(conn, groupEventID);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购活动中的所有用户", ex.ToString());
            throw ex;
        }

        return groupEventMemberList;
    }

    public static List<GroupPurchaseEventMember> FindGroupPurchaseEventMembers(SqlConnection conn, int groupEventID)
    {
        List<GroupPurchaseEventMember> groupEventMemberList = new List<GroupPurchaseEventMember>();

        try
        {
            //查询此团购活动成员所属的团购活动
            GroupPurchaseEvent groupEvent = GroupPurchaseEvent.FindGroupPurchaseEventByID(conn, groupEventID, false);

            using (SqlCommand cmdGroupID = conn.CreateCommand())
            {
                SqlParameter paramID = cmdGroupID.CreateParameter();
                paramID.ParameterName = "@GroupEventID";
                paramID.SqlDbType = System.Data.SqlDbType.Int;
                paramID.SqlValue = groupEventID;
                cmdGroupID.Parameters.Add(paramID);

                cmdGroupID.CommandText = "select * from GroupPurchaseEventMember where GroupEventID = @GroupEventID";

                using (SqlDataReader sdr = cmdGroupID.ExecuteReader())
                {
                    GroupPurchaseEventMember eventMember;

                    while (sdr.Read())
                    {
                        eventMember = new GroupPurchaseEventMember();
                        eventMember.ID = int.Parse(sdr["Id"].ToString());
                        eventMember.GroupMember = WeChatUserDAO.FindUserByOpenID(conn, sdr["GroupMember"].ToString(), false);
                        eventMember.JoinDate = DateTime.Parse(sdr["JoinDate"].ToString());
                        eventMember.GroupPurchaseEvent = groupEvent;

                        groupEventMemberList.Add(eventMember);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购活动中的所有用户", ex.ToString());
            throw ex;
        }

        return groupEventMemberList;
    }

}