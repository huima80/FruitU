using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

/// <summary>
/// User 的摘要说明
/// </summary>
public abstract class User : MembershipUser, IComparable<User>
{
    public int ID { get; set; }

    public string OpenID { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public bool Sex { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// 省份
    /// </summary>
    public string Province { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    public string City { get; set; }

    /// <summary>
    /// 客户端IP
    /// </summary>
    public string ClientIP { get; set; }

    /// <summary>
    /// 订单数
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// 用户订单列表
    /// </summary>
    public List<ProductOrder> ProductOrderList { get; set; }

    public User()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public User(MembershipUser mUser) : base(mUser.ProviderName, mUser.UserName, mUser.ProviderUserKey, mUser.Email, mUser.PasswordQuestion, mUser.Comment, mUser.IsApproved, mUser.IsLockedOut, mUser.CreationDate, mUser.LastLoginDate, mUser.LastActivityDate, mUser.LastPasswordChangedDate, mUser.LastLockoutDate)
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public virtual int CompareTo(User other)
    {
        Guid thisUserID = (Guid)this.ProviderUserKey;
        Guid otherUserID = (Guid)other.ProviderUserKey;

        if (thisUserID == otherUserID || this.OpenID == other.OpenID)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }
}