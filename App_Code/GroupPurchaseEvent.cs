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
    /// 是否通知用户
    /// </summary>
    public bool IsNotify { get; set; }

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
            return CheckGroupPurchaseEventStatus(this);
        }
    }

    /// <summary>
    /// 团购活动事件参数类
    /// </summary>
    public class GroupPurchaseEventEventArgs : EventArgs
    {

    }

    /// <summary>
    /// 团购活动成功事件
    /// </summary>
    public event EventHandler<GroupPurchaseEventEventArgs> GroupPurchaseEventSuccess;

    /// <summary>
    /// 同步触发团购活动成功事件
    /// </summary>
    protected void OnGroupPurchaseEventSuccess()
    {
        if (GroupPurchaseEventSuccess != null)
        {
            GroupPurchaseEventEventArgs gpe = new GroupPurchaseEventEventArgs();
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
            GroupPurchaseEventEventArgs gpe = new GroupPurchaseEventEventArgs();
            this.GroupPurchaseEventFail(this, gpe);
        }
    }

    public GroupPurchaseEvent()
    {
        GroupPurchaseEventMembers = new List<GroupPurchaseEventMember>();
    }

    /// <summary>
    /// 查询未通知用户的团购活动，默认加载活动成员
    /// </summary>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventForNotify()
    {
        return FindGroupPurchaseEventForNotify(true);
    }

    /// <summary>
    /// 查询未通知用户的团购活动
    /// </summary>
    /// <param name="isLoadGroupEventMember">是否加载活动成员</param>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventForNotify(bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList = null;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                groupEventList = FindGroupPurchaseEventForNotify(conn, isLoadGroupEventMember);
            }
        }
        catch (Exception ex)
        {
            Log.Error("查询全部团购活动", ex.ToString());
            throw ex;
        }

        return groupEventList;

    }

    /// <summary>
    /// 查询未通知用户的团购活动
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="isLoadGroupEventMember"></param>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventForNotify(SqlConnection conn, bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList = new List<GroupPurchaseEvent>();
        GroupPurchaseEvent groupEvent;

        try
        {
            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                cmdGroup.CommandText = "select * from GroupPurchaseEvent where IsNotify = 0";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        groupEvent = new GroupPurchaseEvent();
                        groupEvent.ID = int.Parse(sdr["Id"].ToString());
                        groupEvent.Organizer = WeChatUserDAO.FindUserByOpenID(conn, sdr["Organizer"].ToString(), false);
                        groupEvent.LaunchDate = DateTime.Parse(sdr["LaunchDate"].ToString());
                        groupEvent.GroupPurchase = GroupPurchase.FindGroupPurchaseByID(conn, int.Parse(sdr["GroupID"].ToString()), false, false);
                        groupEvent.IsNotify = bool.Parse(sdr["IsNotify"].ToString());
                        if (isLoadGroupEventMember)
                        {
                            groupEvent.GroupPurchaseEventMembers = groupEvent.FindGroupPurchaseEventMembers(conn);
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
            Log.Error("查询所有团购活动", ex.ToString());
            throw ex;
        }

        return groupEventList;
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
                        groupEvent.IsNotify = bool.Parse(sdr["IsNotify"].ToString());
                        if (isLoadGroupEventMember)
                        {
                            groupEvent.GroupPurchaseEventMembers = groupEvent.FindGroupPurchaseEventMembers(conn);
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

    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByOpenID(string openID)
    {
        return FindGroupPurchaseEventByOpenID(openID, true);
    }

    /// <summary>
    /// 根据ID查询指定的团购活动
    /// </summary>
    /// <param name="id">团购活动ID</param>
    /// <param name="isLoadGroupEventMember">是否加载团购活动成员</param>
    /// <returns></returns>
    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByOpenID(string openID, bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList = null;

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
            Log.Error("根据ID查询指定团购活动", ex.ToString());
            throw ex;
        }

        return groupEventList;
    }

    public static List<GroupPurchaseEvent> FindGroupPurchaseEventByOpenID(SqlConnection conn, string openID, bool isLoadGroupEventMember)
    {
        List<GroupPurchaseEvent> groupEventList = new List<GroupPurchaseEvent>();
        GroupPurchaseEvent groupEvent = null;

        try
        {
            using (SqlCommand cmdGroup = conn.CreateCommand())
            {
                SqlParameter paramID = cmdGroup.CreateParameter();
                paramID.ParameterName = "@GroupMember";
                paramID.SqlDbType = System.Data.SqlDbType.NVarChar;
                paramID.SqlValue = openID;
                cmdGroup.Parameters.Add(paramID);

                cmdGroup.CommandText = "select * from GroupPurchaseEvent where Id in (select GroupEventID from GroupPurchaseEventMember where GroupMember = @GroupMember)";

                using (SqlDataReader sdr = cmdGroup.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        groupEvent = new GroupPurchaseEvent();
                        groupEvent.ID = int.Parse(sdr["Id"].ToString());
                        groupEvent.Organizer = WeChatUserDAO.FindUserByOpenID(conn, sdr["Organizer"].ToString(), false);
                        groupEvent.LaunchDate = DateTime.Parse(sdr["LaunchDate"].ToString());
                        groupEvent.GroupPurchase = GroupPurchase.FindGroupPurchaseByID(conn, int.Parse(sdr["GroupID"].ToString()), false, false);
                        groupEvent.IsNotify = bool.Parse(sdr["IsNotify"].ToString());
                        if (isLoadGroupEventMember)
                        {
                            groupEvent.GroupPurchaseEventMembers = groupEvent.FindGroupPurchaseEventMembers(conn);
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
            Log.Error("根据ID查询指定团购活动", ex.ToString());
            throw ex;
        }

        return groupEventList;
    }

    public List<GroupPurchaseEventMember> FindGroupPurchaseEventMembers(SqlConnection conn)
    {
        List<GroupPurchaseEventMember> groupEventMemberList = new List<GroupPurchaseEventMember>();

        try
        {
            //查询团购活动关联的所有订单，用于检测此用户是否全部支付成功
            List<ProductOrder> poList = this.FindOrderByGroupEventID(conn);

            using (SqlCommand cmdGroupID = conn.CreateCommand())
            {
                SqlParameter paramID = cmdGroupID.CreateParameter();
                paramID.ParameterName = "@GroupEventID";
                paramID.SqlDbType = System.Data.SqlDbType.Int;
                paramID.SqlValue = this.ID;
                cmdGroupID.Parameters.Add(paramID);

                cmdGroupID.CommandText = "select * from GroupPurchaseEventMember where GroupEventID = @GroupEventID order by Id";

                using (SqlDataReader sdr = cmdGroupID.ExecuteReader())
                {
                    GroupPurchaseEventMember eventMember;

                    while (sdr.Read())
                    {
                        eventMember = new GroupPurchaseEventMember();
                        eventMember.ID = int.Parse(sdr["Id"].ToString());
                        eventMember.GroupMember = WeChatUserDAO.FindUserByOpenID(conn, sdr["GroupMember"].ToString(), false);
                        eventMember.JoinDate = DateTime.Parse(sdr["JoinDate"].ToString());
                        eventMember.GroupPurchaseEvent = this;

                        //检查当前成员是否有未支付的订单，必须全部订单支付成功才认为此用户参加的团购活动支付成功
                        bool existNotPaidPO = poList.Exists(po =>
                        {
                            if (po.Purchaser.OpenID == eventMember.GroupMember.OpenID
                            && po.TradeState != TradeState.SUCCESS
                            && po.TradeState != TradeState.CASHPAID
                            && po.TradeState != TradeState.AP_TRADE_FINISHED
                            && po.TradeState != TradeState.AP_TRADE_SUCCESS)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        });
                        //设置当前成员的支付标记
                        eventMember.IsPaid = !existNotPaidPO ? true : false;

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

    public List<ProductOrder> FindOrderByGroupEventID()
    {
        List<ProductOrder> poList;

        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                poList = FindOrderByGroupEventID(conn);
            }
        }
        catch (Exception ex)
        {
            Log.Error("根据ID查询指定团购活动", ex.ToString());
            throw ex;
        }

        return poList;

    }

    /// <summary>
    /// 查询指定团购活动ID包含的所有订单，用于检测活动成员是否全部支付成功。不需要加载订单明细，以及订单明细项对应的团购活动
    /// </summary>
    /// <param name="conn"></param>
    /// <returns></returns>
    public List<ProductOrder> FindOrderByGroupEventID(SqlConnection conn)
    {
        List<ProductOrder> poList = new List<ProductOrder>();
        ProductOrder po;

        try
        {
            using (SqlCommand cmdOrder = conn.CreateCommand())
            {
                cmdOrder.CommandText = "select * from ProductOrder where Id in (select ProductOrder.Id from ProductOrder left join OrderDetail on ProductOrder.Id = OrderDetail.PoID where OrderDetail.GroupEventID=@EventID) order by Id";

                SqlParameter paramEventID = cmdOrder.CreateParameter();
                paramEventID.ParameterName = "@EventID";
                paramEventID.SqlDbType = System.Data.SqlDbType.Int;
                paramEventID.SqlValue = this.ID;
                cmdOrder.Parameters.Add(paramEventID);

                using (SqlDataReader sdrOrder = cmdOrder.ExecuteReader())
                {
                    while (sdrOrder.Read())
                    {
                        po = new ProductOrder();

                        ProductOrder.SDR2PO(po, sdrOrder);

                        po.Purchaser = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["OpenID"].ToString(), false);
                        if (sdrOrder["AgentOpenID"] != DBNull.Value)
                        {
                            po.Agent = WeChatUserDAO.FindUserByOpenID(conn, sdrOrder["AgentOpenID"].ToString(), false);
                        }

                        poList.Add(po);
                    }
                }

            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return poList;

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

                        SqlParameter paramIsNotify = cmdAddGroupEvent.CreateParameter();
                        paramIsNotify.ParameterName = "@IsNotify";
                        paramIsNotify.SqlDbType = System.Data.SqlDbType.Bit;
                        paramIsNotify.SqlValue = groupEvent.LaunchDate;
                        cmdAddGroupEvent.Parameters.Add(paramIsNotify);

                        foreach (SqlParameter param in cmdAddGroupEvent.Parameters)
                        {
                            if (param.Value == null)
                            {
                                param.Value = DBNull.Value;
                            }
                        }

                        //插入订单表
                        cmdAddGroupEvent.CommandText = "INSERT INTO [dbo].[GroupPurchaseEvent] ([GroupID], [Organizer], [LaunchDate], [IsNotify]) VALUES (@GroupID,@Organizer,@LaunchDate,@IsNotify);select SCOPE_IDENTITY() as 'NewGroupEventID'";

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
    /// 更新团购活动的通知状态
    /// </summary>
    /// <param name="groupEvent"></param>
    public static void UpdateGroupPurchaseEventNotify(GroupPurchaseEvent groupEvent)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(Config.ConnStr))
            {
                conn.Open();

                using (SqlCommand cmdUpdateGroupEvent = conn.CreateCommand())
                {
                    SqlParameter paramID = cmdUpdateGroupEvent.CreateParameter();
                    paramID.ParameterName = "@ID";
                    paramID.SqlDbType = System.Data.SqlDbType.Int;
                    paramID.SqlValue = groupEvent.ID;
                    cmdUpdateGroupEvent.Parameters.Add(paramID);

                    SqlParameter paramIsNotify = cmdUpdateGroupEvent.CreateParameter();
                    paramIsNotify.ParameterName = "@IsNotify";
                    paramIsNotify.SqlDbType = System.Data.SqlDbType.Bit;
                    paramIsNotify.SqlValue = groupEvent.IsNotify;
                    cmdUpdateGroupEvent.Parameters.Add(paramIsNotify);

                    //更新团购活动表
                    cmdUpdateGroupEvent.CommandText = "Update GroupPurchaseEvent set IsNotify = @IsNotify where Id = @ID";

                    Log.Debug("更新团购活动表", cmdUpdateGroupEvent.CommandText);

                    if (cmdUpdateGroupEvent.ExecuteNonQuery() != 1)
                    {
                        throw new Exception("更新团购活动表");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error("GroupPurchaseEvent:UpdateGroupPurchaseEventNotify", ex.ToString());
            throw ex;
        }
    }

    /// <summary>
    /// 检查指定团购活动的状态
    /// 1，成功：团购活动在有效期内达到规定人数，且活动相关订单都支付成功
    /// 2，进行中：团购活动在有效期内，已支付的成员数尚未达到规定人数
    /// 3，失败：团购活动超过有效期，已支付的成员数尚未达到规定人数
    /// </summary>
    /// <param name="groupEvent">团购活动</param>
    /// <param name="wxUser">可选，当指定团购活动中的用户时，要进一步判断此用户是否支付成功，其支付顺序是否在要求人数内，再决定对此用户来说团购活动是否成功</param>
    /// <returns></returns>
    public static GroupEventStatus CheckGroupPurchaseEventStatus(GroupPurchaseEvent groupEvent, WeChatUser wxUser = null)
    {
        GroupEventStatus eventStatus;
        DateTime nowTime = DateTime.Now;
        if (groupEvent != null)
        {
            if (groupEvent.LaunchDate >= groupEvent.GroupPurchase.StartDate && groupEvent.LaunchDate <= groupEvent.GroupPurchase.EndDate)
            {
                if (groupEvent.GroupPurchaseEventMembers != null && groupEvent.GroupPurchaseEventMembers.Count >= groupEvent.GroupPurchase.RequiredNumber)
                {
                    //已支付的团购成员
                    List<GroupPurchaseEventMember> membersPaid = groupEvent.GroupPurchaseEventMembers.FindAll(member => member.IsPaid);

                    //如果已支付的成员数达到团购要求的成员数
                    if (membersPaid.Count >= groupEvent.GroupPurchase.RequiredNumber)
                    {
                        //如果没有指定用户，则对于管理员来说，此团购活动成功
                        if (wxUser == null)
                        {
                            eventStatus = GroupEventStatus.EVENT_SUCCESS;
                        }
                        else
                        {
                            //如果指定的用户支付成功
                            if (membersPaid.Exists(member => member.GroupMember.OpenID == wxUser.OpenID))
                            {
                                //再判断此用户是第几个支付成功的，按先后顺序，如果此用户在要求人数以内支付成功，则此团购活动成功，否则团购活动失败
                                int theMemberPaidIndex = membersPaid.FindIndex(member => member.GroupMember.OpenID == wxUser.OpenID);
                                if (theMemberPaidIndex + 1 <= groupEvent.GroupPurchase.RequiredNumber)
                                {
                                    eventStatus = GroupEventStatus.EVENT_SUCCESS;
                                }
                                else
                                {
                                    //如果指定的用户虽然支付成功，但按先后顺序，已超过了要求人数，则对于此用户来说，此团购活动失败
                                    eventStatus = GroupEventStatus.EVENT_FAIL;
                                }
                            }
                            else
                            {
                                //如果指定的用户未支付成功，由于此团购已支付人数已达到要求，则对于此用户来说，此团购活动失败
                                eventStatus = GroupEventStatus.EVENT_FAIL;
                            }
                        }
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
                    if (od.GroupPurchaseEvent != null && !od.GroupPurchaseEvent.IsNotify)
                    {
                        //如果拼团成功，则触发拼团成功事件，通知管理员和所有拼团用户
                        GroupEventStatus eventStatus = CheckGroupPurchaseEventStatus(od.GroupPurchaseEvent);
                        if (eventStatus == GroupEventStatus.EVENT_SUCCESS)
                        {
                            od.GroupPurchaseEvent.GroupPurchaseEventSuccess += WxTmplMsg.GroupPurchaseEventNotify;
                            od.GroupPurchaseEvent.OnGroupPurchaseEventSuccess();

                            //设置此团购活动通知标记为true
                            od.GroupPurchaseEvent.IsNotify = true;
                            UpdateGroupPurchaseEventNotify(od.GroupPurchaseEvent);
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

    public static void CheckGroupEventFailHandler(object sender, System.Timers.ElapsedEventArgs e)
    {
        CheckGroupEventFail();
    }

    /// <summary>
    /// 查找过期的团购活动，并通知管理员和用户
    /// </summary>
    public static void CheckGroupEventFail()
    {
        List<GroupPurchaseEvent> groupEventList;
        groupEventList = FindGroupPurchaseEventForNotify();
        groupEventList.ForEach(groupEvent =>
        {
            switch (groupEvent.GroupEventStatus)
            {
                case GroupEventStatus.EVENT_FAIL:
                    //拼团失败，提醒用户查收退款
                    groupEvent.GroupPurchaseEventFail += WxTmplMsg.GroupPurchaseEventNotify;
                    groupEvent.OnGroupPurchaseEventFail();
                    break;
                case GroupEventStatus.EVENT_GOING:
                    //团购进行中，提醒用户邀请拼团
                    break;
            }

            //设置此团购活动通知标记为true
            groupEvent.IsNotify = true;
            UpdateGroupPurchaseEventNotify(groupEvent);

        });
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