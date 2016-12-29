using LitJson;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// 团购活动
/// </summary>
public class GroupPurchaseEvent
{
    public int ID { get; set; }

    /// <summary>
    /// 团购活动所属的团购
    /// </summary>
    public GroupPurchase GroupPurchase { get; set; }

    /// <summary>
    /// 团购活动发起者
    /// </summary>
    public WeChatUser Organizer { get; set; }

    /// <summary>
    /// 团购活动发起日期
    /// </summary>
    public DateTime LaunchDate { get; set; }

    /// <summary>
    /// 团购活动参与者列表
    /// </summary>
    public List<GroupPurchaseEventMember> GroupPurchaseEventMembers { get; set; }

    /// <summary>
    /// 团购活动状态
    /// </summary>
    public GroupEventStatus GroupEventStatus
    {
        get
        {
            return CheckGroupPurchaseEventSuccess(this);
        }
    }

    /// <summary>
    /// 团购活动事件参数类
    /// </summary>
    public class GroupPurchaseEventEventArgs : EventArgs
    {
        /// <summary>
        /// 团购活动成功标志
        /// </summary>
        public bool isGroupEventSuccess { get; set; }

        /// <summary>
        /// 团购活动所属的订单商品项
        /// </summary>
        public OrderDetail orderDetail { get; set; }

        /// <summary>
        /// 参加团购活动的日期
        /// </summary>
        public DateTime joinGroupEventDate { get; set; }

        public GroupPurchaseEventEventArgs(bool isGroupEventSuccess)
        {
            this.isGroupEventSuccess = isGroupEventSuccess;
        }
    }

    /// <summary>
    /// 团购活动成功事件
    /// </summary>
    public event EventHandler<GroupPurchaseEventEventArgs> GroupPurchaseEventSuccess;

    /// <summary>
    /// 同步触发团购活动成功事件
    /// </summary>
    protected void OnGroupPurchaseEventSuccess(OrderDetail orderDetail, DateTime joinGroupEventDate)
    {
        if (GroupPurchaseEventSuccess != null)
        {
            GroupPurchaseEventEventArgs gpe = new GroupPurchaseEventEventArgs(true);
            gpe.orderDetail = orderDetail;
            gpe.joinGroupEventDate = joinGroupEventDate;
            this.GroupPurchaseEventSuccess(this, gpe);
        }
    }

    /// <summary>
    /// 团购活动失败事件
    /// </summary>
    public event EventHandler<GroupPurchaseEventEventArgs> GroupPurchaseEventFail;

    /// <summary>
    /// 同步触发团购活动失败事件
    /// </summary>
    protected void OnGroupPurchaseEventFail()
    {
        if (GroupPurchaseEventFail != null)
        {
            GroupPurchaseEventEventArgs gpe = new GroupPurchaseEventEventArgs(false);
            this.GroupPurchaseEventFail(this, gpe);
        }
    }

    public GroupPurchaseEvent()
    {
        GroupPurchaseEventMembers = new List<GroupPurchaseEventMember>();
    }

    /// <summary>
    /// 根据ID查询指定的团购活动，默认加载其团购活动成员
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static GroupPurchaseEvent FindGroupPurchaseEventByID(int id)
    {
        return FindGroupPurchaseEventByID(id, true);
    }

    /// <summary>
    /// 根据ID查询指定的团购活动
    /// </summary>
    /// <param name="id">团购活动ID</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <returns></returns>
    public static GroupPurchaseEvent FindGroupPurchaseEventByID(int id, bool isLoadGroupEventMember)
    {
        GroupPurchaseEvent groupEvent = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupEvent = FindGroupPurchaseEventByID(conn, id, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("根据ID查询指定团购活动", ex.ToString());
            throw ex;
        }

        return groupEvent;
    }

    public static GroupPurchaseEvent FindGroupPurchaseEventByID(SqlConnection conn, int id, bool isLoadGroupEventMember)
    {
        GroupPurchaseEvent groupEvent = null;

        try
        {
            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramID = cmdGroup.CreateParameter();
                paramID.ParameterName = "@Id";
                paramID.SqlDbType = System.Data.SqlDbType.Int;
                paramID.SqlValue = id;
                cmdGroup.Parameters.Add(paramID);

                cmdGroup.CommandText = "select * from GroupPurchaseEvent where Id = @Id";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        groupEvent = new GroupPurchaseEvent();
                        groupEvent.ID = int.Parse(sdr["Id"].ToString());
                        groupEvent.Organizer = WeChatUserDAO.FindUserByOpenID(conn, sdr["Organizer"].ToString(), false);
                        groupEvent.LaunchDate = DateTime.Parse(sdr["LaunchDate"].ToString());
                        groupEvent.GroupPurchase = GroupPurchase.FindGroupPurchaseByID(conn, int.Parse(sdr["GroupID"].ToString()), false, false);
                        if (isLoadGroupEventMember)
                        {
                            groupEvent.GroupPurchaseEventMembers = GroupPurchaseEventMember.FindGroupPurchaseEventMembers(conn, groupEvent.ID);
                        }
                        else
                        {
                            groupEvent.GroupPurchaseEventMembers = null;
                        }
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("根据ID查询指定团购活动", ex.ToString());
            throw ex;
        }

        return groupEvent;
    }

    /// <summary>
    /// 查询指定团购下的所有活动，默认加载其团购活动成员
    /// </summary>
    /// <param name="groupID"></param>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByGroupID(int groupID)
    {
        return FindGroupPurchaseEventByGroupID(groupID, true);
    }

    /// <summary>
    /// 查询指定团购下的所有活动
    /// </summary>
    /// <param name="groupID">团购ID</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByGroupID(int groupID, bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupEventList = FindGroupPurchaseEventByGroupID(conn, groupID, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购下的所有活动", ex.ToString());
            throw ex;
        }

        return groupEventList;
    }

    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByGroupID(SqlConnection conn, int groupID, bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList = new List<GroupPurchaseEvent>();
        GroupPurchaseEvent groupEvent;

        try
        {
            GroupPurchase groupPurchase = GroupPurchase.FindGroupPurchaseByID(conn, groupID, false, false);

            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramID = cmdGroup.CreateParameter();
                paramID.ParameterName = "@GroupID";
                paramID.SqlDbType = System.Data.SqlDbType.Int;
                paramID.SqlValue = groupID;
                cmdGroup.Parameters.Add(paramID);

                cmdGroup.CommandText = "select * from GroupPurchaseEvent where GroupID = @GroupID";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        groupEvent = new GroupPurchaseEvent();
                        groupEvent.ID = int.Parse(sdr["Id"].ToString());
                        groupEvent.Organizer = WeChatUserDAO.FindUserByOpenID(conn, sdr["Organizer"].ToString(), false);
                        groupEvent.LaunchDate = DateTime.Parse(sdr["LaunchDate"].ToString());
                        groupEvent.GroupPurchase = groupPurchase;
                        if (isLoadGroupEventMember)
                        {
                            groupEvent.GroupPurchaseEventMembers = GroupPurchaseEventMember.FindGroupPurchaseEventMembers(conn, groupEvent.ID);
                        }
                        else
                        {
                            groupEvent.GroupPurchaseEventMembers = null;
                        }

                        groupEventList.Add(groupEvent);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定团购下的所有活动", ex.ToString());
            throw ex;
        }

        return groupEventList;
    }

    /// <summary>
    /// 查询指定用户参加的所有团购活动，默认加载其所有团购活动成员
    /// </summary>
    /// <param name="openID"></param>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByOpenID(string openID)
    {
        return FindGroupPurchaseEventByOpenID(openID, true);
    }

    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByOpenID(string openID, bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupEventList = FindGroupPurchaseEventByOpenID(conn, openID, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定用户参加的所有团购活动", ex.ToString());
            throw ex;
        }

        return groupEventList;
    }

    /// <summary>
    /// 查询指定用户参加的所有团购活动
    /// </summary>
    /// <param name="openID"></param>
    /// <param name="isLoadGroupEventMember"></param>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByOpenID(SqlConnection conn, string openID, bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList = new List<GroupPurchaseEvent>();
        GroupPurchaseEvent groupEvent;

        try
        {
            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramID = cmdGroup.CreateParameter();
                paramID.ParameterName = "@OpenID";
                paramID.SqlDbType = System.Data.SqlDbType.NVarChar;
                paramID.SqlValue = openID;
                cmdGroup.Parameters.Add(paramID);

                cmdGroup.CommandText = "select * from GroupPurchaseEvent where Id in (select GroupEventID from GroupPurchaseEventMember where GroupMember = @OpenID)";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        groupEvent = new GroupPurchaseEvent();
                        groupEvent.ID = int.Parse(sdr["Id"].ToString());
                        groupEvent.Organizer = WeChatUserDAO.FindUserByOpenID(sdr["Organizer"].ToString());
                        groupEvent.LaunchDate = DateTime.Parse(sdr["LaunchDate"].ToString());
                        if (isLoadGroupEventMember)
                        {
                            groupEvent.GroupPurchaseEventMembers = GroupPurchaseEventMember.FindGroupPurchaseEventMembers(conn, groupEvent.ID);
                        }
                        else
                        {
                            groupEvent.GroupPurchaseEventMembers = null;
                        }

                        groupEventList.Add(groupEvent);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("查询指定用户参加的所有团购活动", ex.ToString());
            throw ex;
        }

        return groupEventList;
    }

    /// <summary>
    /// 增加团购活动
    /// </summary>
    /// <param name="groupEvent">团购活动对象</param>
    /// <returns></returns>
    public static GroupPurchaseEvent AddGroupPurchaseEvent(GroupPurchaseEvent groupEvent)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    using (SqlCommand cmdAddGroupEvent = conn.CreateCommand())
                    {
                        cmdAddGroupEvent.Transaction = trans;

                        SqlParameter paramGroupID = cmdAddGroupEvent.CreateParameter();
                        paramGroupID.ParameterName = "@GroupID";
                        paramGroupID.SqlDbType = System.Data.SqlDbType.Int;
                        paramGroupID.SqlValue = groupEvent.GroupPurchase.ID;
                        cmdAddGroupEvent.Parameters.Add(paramGroupID);

                        SqlParameter paramOrganizer = cmdAddGroupEvent.CreateParameter();
                        paramOrganizer.ParameterName = "@Organizer";
                        paramOrganizer.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramOrganizer.SqlValue = groupEvent.Organizer;
                        cmdAddGroupEvent.Parameters.Add(paramOrganizer);

                        SqlParameter paramLaunchDate = cmdAddGroupEvent.CreateParameter();
                        paramLaunchDate.ParameterName = "@LaunchDate";
                        paramLaunchDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        paramLaunchDate.SqlValue = groupEvent.LaunchDate;
                        cmdAddGroupEvent.Parameters.Add(paramLaunchDate);

                        foreach (SqlParameter param in cmdAddGroupEvent.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        //插入订单表
                        cmdAddGroupEvent.CommandText = "INSERT INTO [dbo].[GroupPurchaseEvent] ([GroupID], [Organizer], [LaunchDate]) VALUES (@GroupID,@Organizer,@LaunchDate);select SCOPE_IDENTITY() as 'NewGroupEventID'";

                        Log.Debug("插入团购活动表", cmdAddGroupEvent.CommandText);

                        var newGroupEventID = cmdAddGroupEvent.ExecuteScalar();

                        //新增的订单ID
                        if (newGroupEventID != DBNull.Value)
                        {
                            groupEvent.ID = int.Parse(newGroupEventID.ToString());
                        }
                        else
                        {
                            throw new Exception("插入团购活动错误");
                        }
                    }

                    using (SqlCommand cmdAddGroupEventMember = conn.CreateCommand())
                    {
                        cmdAddGroupEventMember.Transaction = trans;

                        SqlParameter paramGroupEventID = cmdAddGroupEventMember.CreateParameter();
                        paramGroupEventID.ParameterName = "@GroupEventID";
                        paramGroupEventID.SqlDbType = System.Data.SqlDbType.Int;
                        paramGroupEventID.SqlValue = groupEvent.ID;
                        cmdAddGroupEventMember.Parameters.Add(paramGroupEventID);

                        SqlParameter paramGroupMember = cmdAddGroupEventMember.CreateParameter();
                        paramGroupMember.ParameterName = "@GroupMember";
                        paramGroupMember.SqlDbType = System.Data.SqlDbType.NVarChar;
                        cmdAddGroupEventMember.Parameters.Add(paramGroupMember);

                        SqlParameter paramJoinDate = cmdAddGroupEventMember.CreateParameter();
                        paramJoinDate.ParameterName = "@JoinDate";
                        paramJoinDate.SqlDbType = System.Data.SqlDbType.DateTime;
                        cmdAddGroupEventMember.Parameters.Add(paramJoinDate);

                        groupEvent.GroupPurchaseEventMembers.ForEach(member =>
                        {
                            paramGroupMember.SqlValue = member.GroupMember.OpenID;
                            paramJoinDate.SqlValue = member.JoinDate;

                            //插入团购活动成员表
                            cmdAddGroupEventMember.CommandText = "INSERT INTO [dbo].[GroupPurchaseEventMember] ([GroupEventID], [GroupMember], [JoinDate]) VALUES (@GroupEventID,@GroupMember,@JoinDate);select SCOPE_IDENTITY() as 'NewGroupEventMemberID'";

                            var newGroupEventMemberID = cmdAddGroupEventMember.ExecuteScalar();

                            //新增的订单详情ID
                            if (newGroupEventMemberID != DBNull.Value)
                            {
                                member.ID = int.Parse(newGroupEventMemberID.ToString());
                            }
                            else
                            {
                                throw new Exception("插入团购活动成员表错误");
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
            Log.Error("GroupPurchaseEvent:AddGroupPurchaseEvent", ex.ToString());
            throw ex;
        }

        return groupEvent;

    }

    /// <summary>
    /// 检查订单中所有相关团购活动的状态
    /// 1，成功：团购活动在有效期内达到规定人数，且活动相关订单都支付成功
    /// 2，进行中：团购活动在有效期内尚未达到规定人数，或者尚有订单没有支付成功
    /// 3，失败：团购活动超过有效期尚未达到规定人数，或者尚有订单没有支付成功
    /// </summary>
    /// <param name="groupEvent">团购活动</param>
    /// <returns></returns>
    public static GroupEventStatus CheckGroupPurchaseEventSuccess(GroupPurchaseEvent groupEvent)
    {
        GroupEventStatus eventStatus;
        DateTime nowTime = DateTime.Now;
        if (groupEvent != null)
        {
            if (groupEvent.LaunchDate >= groupEvent.GroupPurchase.StartDate && groupEvent.LaunchDate <= groupEvent.GroupPurchase.EndDate)
            {
                if (groupEvent.GroupPurchaseEventMembers != null && groupEvent.GroupPurchaseEventMembers.Count >= groupEvent.GroupPurchase.RequiredNumber)
                {
                    //已支付的成员数量
                    int paidMemberCount = 0;
                    //查询团购活动关联的所有订单
                    List<ProductOrder> poList = ProductOrder.FindOrderByGroupEventID(groupEvent.ID);
                    //属于某个成员的订单
                    List<ProductOrder> poListOfOneMember;
                    groupEvent.GroupPurchaseEventMembers.ForEach(member =>
                    {
                        poListOfOneMember = poList.FindAll(po => po.Purchaser.OpenID == member.GroupMember.OpenID);
                        //检查当前成员是否有在活动有效期内未支付的订单
                        bool existNotPaidPO = poListOfOneMember.Exists(poOfOneMember =>
                            {
                                if ((poOfOneMember.OrderDate >= groupEvent.GroupPurchase.StartDate && poOfOneMember.OrderDate <= groupEvent.GroupPurchase.EndDate)
                                && poOfOneMember.TradeState != TradeState.SUCCESS
                                && poOfOneMember.TradeState != TradeState.CASHPAID
                                && poOfOneMember.TradeState != TradeState.AP_TRADE_FINISHED
                                && poOfOneMember.TradeState != TradeState.AP_TRADE_SUCCESS)
                                {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            });
                        if (!existNotPaidPO)
                        {
                            paidMemberCount++;
                        }
                    });

                    //如果已支付的成员数达到团购要求的成员数，则团购成功
                    if (paidMemberCount >= groupEvent.GroupPurchase.RequiredNumber)
                    {
                        eventStatus = GroupEventStatus.EVENT_SUCCESS;
                    }
                    else
                    {
                        //如果已支付的成员数未达到要求人数，则根据当前时间是否在活动有效期内，决定团购是否进行中或失败
                        if (nowTime >= groupEvent.GroupPurchase.StartDate && nowTime <= groupEvent.GroupPurchase.EndDate)
                        {
                            eventStatus = GroupEventStatus.EVENT_GOING;
                        }
                        else
                        {
                            eventStatus = GroupEventStatus.EVENT_FAIL;
                        }
                    }
                }
                else
                {
                    if (nowTime >= groupEvent.GroupPurchase.StartDate && nowTime <= groupEvent.GroupPurchase.EndDate)
                    {
                        eventStatus = GroupEventStatus.EVENT_GOING;
                    }
                    else
                    {
                        eventStatus = GroupEventStatus.EVENT_FAIL;
                    }
                }
            }
            else
            {
                eventStatus = GroupEventStatus.EVENT_FAIL;
            }
        }
        else
        {
            throw new Exception("团购活动对象为NULL");
        }

        return eventStatus;
    }

    /// <summary>
    /// 检测订单中所有相关团购活动是否成功，成功则通知管理员和用户，用于订单支付成功后的回调
    /// </summary>
    /// <param name="po"></param>
    /// <param name="ose"></param>
    /// <returns></returns>
    public static JsonData GroupPurchaseEventSuccessHandler(ProductOrder po, ProductOrder.OrderStateEventArgs ose)
    {
        if (po == null || ose == null)
        {
            throw new ArgumentNullException("sender或事件参数对象不能为null");
        }
        JsonData jRet = new JsonData();

        try
        {
            //如果此订单支付成功
            if ((po.PaymentTerm == PaymentTerm.WECHAT && po.TradeState == TradeState.SUCCESS)
                || (po.PaymentTerm == PaymentTerm.ALIPAY && (po.TradeState == TradeState.AP_TRADE_SUCCESS || po.TradeState == TradeState.AP_TRADE_FINISHED))
                || (po.PaymentTerm == PaymentTerm.CASH && po.TradeState == TradeState.CASHPAID))
            {
                po.OrderDetailList.ForEach(od =>
                {
                    if (od.GroupPurchaseEvent != null)
                    {
                        //如果拼团活动成功，则触发拼团成功事件，通知管理员和所有拼团用户
                        GroupEventStatus eventStatus = CheckGroupPurchaseEventSuccess(od.GroupPurchaseEvent);
                        if (eventStatus == GroupEventStatus.EVENT_SUCCESS)
                        {
                            od.GroupPurchaseEvent.GroupPurchaseEventSuccess += WxTmplMsg.GroupPurchaseEventNotify;
                            od.GroupPurchaseEvent.OnGroupPurchaseEventSuccess(od, po.OrderDate);
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error("CheckGroupPurchaseEventStatus", ex.Message);
            throw ex;
        }

        return jRet;

    }


}

/// <summary>
/// 团购活动状态
/// </summary>
public enum GroupEventStatus
{
    /// <summary>
    /// 团购活动成功
    /// </summary>
    EVENT_SUCCESS = 1,
    /// <summary>
    /// 团购活动进行中
    /// </summary>
    EVENT_GOING = 2,
    /// <summary>
    /// 团购活动失败
    /// </summary>
    EVENT_FAIL = 3
}