﻿using LitJson;
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
    /// 团购活动是否成功，条件：达到团购人数、且所有团购订单支付成功
    /// </summary>
    public bool IsSuccessful
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
    /// 检查订单中所有相关团购活动是否成功（即团购活动达到规定人数、且团购相关订单都支付成功）
    /// </summary>
    /// <param name="groupEvent">团购活动</param>
    /// <returns></returns>
    public static bool CheckGroupPurchaseEventSuccess(GroupPurchaseEvent groupEvent)
    {
        bool isSuccessful;
        //如果此订单项有团购活动，则检查此活动的参团人数是否满足团购条件，且所有订单是否都已支付成功
        if (groupEvent != null && groupEvent.GroupPurchaseEventMembers != null && groupEvent.GroupPurchaseEventMembers.Count >= groupEvent.GroupPurchase.RequiredNumber)
        {
            //查询此订单项的团购活动关联的所有订单
            List<ProductOrder> poList = ProductOrder.FindOrderByGroupEventID(groupEvent.ID);
            bool notPaidPOExist = false;
            notPaidPOExist = poList.Exists(poWithGroupEvent =>
            {
                if (poWithGroupEvent.TradeState != TradeState.SUCCESS
                && poWithGroupEvent.TradeState != TradeState.CASHPAID
                && poWithGroupEvent.TradeState != TradeState.AP_TRADE_FINISHED
                && poWithGroupEvent.TradeState != TradeState.AP_TRADE_SUCCESS)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });

            isSuccessful = notPaidPOExist ? false : true;
        }
        else
        {
            isSuccessful = false;
        }

        return isSuccessful;
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
        bool isSuccessful;

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
                        isSuccessful = CheckGroupPurchaseEventSuccess(od.GroupPurchaseEvent);
                        if (isSuccessful)
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