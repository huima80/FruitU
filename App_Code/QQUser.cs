using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

/// <summary>
/// QQUser 的摘要说明
/// </summary>
public class QQUser : User
{
    public DateTime? Birthday { get; set; }

    public string FigureUrl { get; set; }

    public string FigureUrl1 { get; set; }

    public string FigureUrl2 { get; set; }

    public string FigureUrlQQ1 { get; set; }

    public string FigureUrlQQ2 { get; set; }

    public bool IsVip { get; set; }

    public int VipLevel { get; set; }

    public bool IsYellowVip { get; set; }

    public int YellowVipLevel { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public DateTime ExpiresIn { get; set; }

    public QQUser()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public QQUser(MembershipUser mUser) : base(mUser)
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

}