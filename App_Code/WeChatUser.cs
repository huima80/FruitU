using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

/// <summary>
/// 微信用户信息类
/// </summary>
public class WeChatUser : User, IComparable<WeChatUser>
{
    /// <summary>
    /// 是否关注微信公众号
    /// </summary>
    public bool IsSubscribe { get; set; }

    /// <summary>
    /// 微信用户头像
    /// </summary>
    public string HeadImgUrl { get; set; }

    /// <summary>
    /// 微信用户特权信息
    /// </summary>
    public string Privilege { get; set; }

    /// <summary>
    /// snsapi_base授权模式的Access Token
    /// </summary>
    public string AccessTokenForBase { get; set; }

    /// <summary>
    /// snsapi_base授权模式的Refresh Token
    /// </summary>
    public string RefreshTokenForBase { get; set; }

    /// <summary>
    /// snsapi_base授权模式的过期时间
    /// </summary>
    public DateTime ExpireOfAccessTokenForBase { get; set; }

    /// <summary>
    /// snsapi_userinfo授权模式的Access Token
    /// </summary>
    public string AccessTokenForUserInfo { get; set; }

    /// <summary>
    /// snsapi_userinfo授权模式的Refresh Token
    /// </summary>
    public string RefreshTokenForUserInfo { get; set; }

    /// <summary>
    /// snsapi_userinfo授权模式的过期时间
    /// </summary>
    public DateTime ExpireOfAccessTokenForUserInfo { get; set; }

    /// <summary>
    /// 微信网页授权模式
    /// </summary>
    public WeChatAuthScope Scope { get; set; }

    /// <summary>
    /// 同一用户，对同一个微信开放平台下的不同应用，unionid是相同的
    /// </summary>
    public string UnionID { get; set; }

    /// <summary>
    /// 推荐人微信OpenID
    /// </summary>
    public string AgentOpenID { get; set; }

    public WeChatUser()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public WeChatUser(MembershipUser mUser) : base(mUser)
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public delegate void UserLoggedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// 微信用户登录后事件
    /// </summary>
    public event UserLoggedEventHandler UserLogged;


    public delegate JsonData MemberPointsChangedEventHandler(object sender, MemberPointsChangedEventArgs e);

    /// <summary>
    /// 会员积分变动事件
    /// </summary>
    public event MemberPointsChangedEventHandler MemberPointsChanged;

    public class MemberPointsChangedEventArgs
    {
        /// <summary>
        /// 新增的会员积分
        /// </summary>
        public int increasedMemberPoints { get; set; }

        /// <summary>
        /// 消耗的会员积分
        /// </summary>
        public int usedMemberPoints { get; set; }

        /// <summary>
        /// 下单人的会员积分余额
        /// </summary>
        public int newMemberPoints { get; set; }

        /// <summary>
        /// 推荐人的会员积分余额
        /// </summary>
        public int agentNewMemberPoints { get; set; }

        public MemberPointsChangedEventArgs()
        {

        }

        public MemberPointsChangedEventArgs(int increasedMemberPoints, int usedMemberPoints, int newMemberPoints, int agentNewMemberPoints)
        {
            this.increasedMemberPoints = increasedMemberPoints;
            this.usedMemberPoints = usedMemberPoints;
            this.newMemberPoints = newMemberPoints;
            this.agentNewMemberPoints = agentNewMemberPoints;
        }
    }

    /// <summary>
    /// 同步触发会员积分变动事件处理函数
    /// </summary>
    /// <param name="increasedMemberPoints"></param>
    /// <param name="usedMemberPoints"></param>
    /// <param name="balance"></param>
    public void OnMemberPointsChanged(int increasedMemberPoints, int usedMemberPoints, int newMemberPoints, int agentNewMemberPoints)
    {
        if (this.MemberPointsChanged != null)
        {
            MemberPointsChangedEventArgs e = new MemberPointsChangedEventArgs(increasedMemberPoints, usedMemberPoints, newMemberPoints, agentNewMemberPoints);
            this.MemberPointsChanged(this, e);
        }
    }

    /// <summary>
    /// 异步触发会员积分变动事件处理函数
    /// </summary>
    /// <param name="increasedMemberPoints"></param>
    /// <param name="usedMemberPoints"></param>
    /// <param name="balance"></param>
    public void OnMemberPointsChangedAsyn(int increasedMemberPoints, int usedMemberPoints, int newMemberPoints, int agentNewMemberPoints)
    {
        //订单状态变化事件异步回调函数，不阻塞主流程
        if (this.MemberPointsChanged != null)
        {
            MemberPointsChangedEventArgs e = new MemberPointsChangedEventArgs(increasedMemberPoints, usedMemberPoints, newMemberPoints, agentNewMemberPoints);
            IAsyncResult ar = this.MemberPointsChanged.BeginInvoke(this, e, MemberPointsChangedComplete, this.MemberPointsChanged);
        }
    }

    /// <summary>
    /// 事件完成异步回调
    /// </summary>
    /// <param name="ar"></param>
    private void MemberPointsChangedComplete(IAsyncResult ar)
    {
        if (ar != null)
        {
            JsonData jRet;
            jRet = (ar.AsyncState as MemberPointsChangedEventHandler).EndInvoke(ar);
            if (jRet != null)
            {
                Log.Info("event MemberPointsChanged", jRet.ToJson());
            }
        }
    }


    public int CompareTo(WeChatUser other)
    {
        if(this.ProviderUserKey == other.ProviderUserKey && this.OpenID == other.OpenID)
        {
            return 0;
        }
        else
        {
            return -1;
        }
    }
}

/// <summary>
/// 微信网页授权范围
/// </summary>
public enum WeChatAuthScope
{
    snsapi_base = 1,
    snsapi_userinfo = 2
}