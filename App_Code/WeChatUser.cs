using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

/// <summary>
/// 微信用户信息类
/// </summary>
public class WeChatUser : User
{
    //是否关注微信公众号
    public bool IsSubscribe { get; set; }

    //微信用户头像
    public string HeadImgUrl { get; set; }

    //微信用户特权信息
    public string Privilege { get; set; }

    //snsapi_base授权模式的Access Token
    public string AccessTokenForBase { get; set; }

    //snsapi_base授权模式的Refresh Token
    public string RefreshTokenForBase { get; set; }

    //snsapi_base授权模式的过期时间
    public DateTime ExpireOfAccessTokenForBase { get; set; }

    //snsapi_userinfo授权模式的Access Token
    public string AccessTokenForUserInfo { get; set; }

    //snsapi_userinfo授权模式的Refresh Token
    public string RefreshTokenForUserInfo { get; set; }

    //snsapi_userinfo授权模式的过期时间
    public DateTime ExpireOfAccessTokenForUserInfo { get; set; }

    //微信网页授权模式
    public WeChatAuthScope Scope { get; set; }

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

    //微信用户登录后事件
    public event UserLoggedEventHandler UserLogged;

}

/// <summary>
/// 微信网页授权范围
/// </summary>
public enum WeChatAuthScope
{
    snsapi_base = 1,
    snsapi_userinfo = 2
}