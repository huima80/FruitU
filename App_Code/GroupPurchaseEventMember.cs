using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// 团购活动成员
/// </summary>
public class GroupPurchaseEventMember
{
    public int ID { get; set; }

    /// <summary>
    /// 团购活动成员所属团购活动
    /// </summary>
    public GroupPurchaseEvent GroupPurchaseEvent { get; set; }

    /// <summary>
    /// 团购活动成员
    /// </summary>
    public WeChatUser GroupMember { get; set; }

    /// <summary>
    /// 团购活动成员参团日期
    /// </summary>
    public DateTime JoinDate { get; set; }

    /// <summary>
    /// 团购活动成员在此团购活动下的所有订单是否都已支付
    /// </summary>
    public bool IsPaid { get; set; }

    public GroupPurchaseEventMember()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }


}