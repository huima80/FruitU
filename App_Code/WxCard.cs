using System;
using System.Collections.Generic;
using LitJson;

/// <summary>
/// 微信卡券类
/// </summary>
public class WxCard
{
    /// <summary>
    /// 查看卡券详情接口
    /// </summary>
    public const string GET_CARD_API = @"https://api.weixin.qq.com/card/get?access_token=";

    /// <summary>
    /// 获取用户已领取卡券接口
    /// </summary>
    public const string GET_CARD_LIST_API = @"https://api.weixin.qq.com/card/user/getcardlist?access_token=";

    /// <summary>
    /// Code解码接口
    /// </summary>
    public const string DECRYPT_CODE_API = @"https://api.weixin.qq.com/card/code/decrypt?access_token=";

    /// <summary>
    /// 查询Code接口
    /// </summary>
    public const string GET_CODE_API = @"https://api.weixin.qq.com/card/code/get?access_token=";

    /// <summary>
    /// 核销Code接口
    /// </summary>
    public const string CONSUME_CODE_API = @"https://api.weixin.qq.com/card/code/consume?access_token=";

    /// <summary>
    /// 领取卡券的微信用户ID
    /// </summary>
    public string OpenID { get; set; }

    /// <summary>
    /// 卡券ID
    /// </summary>
    public string CardID { get; set; }

    /// <summary>
    /// 卡券CODE
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// 卡券类型
    /// </summary>
    public WxCardType CardType { get; set; }

    /// <summary>
    /// 卡券的商户logo
    /// </summary>
    public string LogoUrl { get; set; }

    /// <summary>
    /// Code类型
    /// </summary>
    public WxCodeType CodeType { get; set; }

    /// <summary>
    /// 商户名字
    /// </summary>
    public string BrandName { get; set; }

    /// <summary>
    /// 第三方来源名，例如同程旅游、大众点评。
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// 卡券名
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 卡券名的副标题
    /// </summary>
    public string SubTitle { get; set; }

    /// <summary>
    /// 使用提醒
    /// </summary>
    public string Notice { get; set; }

    /// <summary>
    /// 使用说明
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 卡券现有库存的数量
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// 卡券全部库存的数量
    /// </summary>
    public int TotalQuantity { get; set; }

    /// <summary>
    /// 卡券状态
    /// </summary>
    public CardStatus Status { get; set; }

    /// <summary>
    /// 每人可领券的数量限制
    /// </summary>
    public int GetLimit { get; set; }

    /// <summary>
    /// 卡券原生领取页面是否可分享
    /// </summary>
    public bool CanShare { get; set; }

    /// <summary>
    /// 卡券是否可转赠
    /// </summary>
    public bool CanGiveFriend { get; set; }

    /// <summary>
    /// 卡券有效期类型
    /// </summary>
    public AvailableDateType DateType { get; set; }

    /// <summary>
    /// 表示起用时间。从1970年1月1日00:00:00至起用时间的秒数，最终需转换为字符串形态传入。（单位为秒）
    /// </summary>
    public DateTime BeginTime { get; set; }

    /// <summary>
    /// 表示结束时间。从1970年1月1日00:00:00至起用时间的秒数，最终需转换为字符串形态传入。（单位为秒）
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 表示自领取后多少天内有效，领取后当天有效填写0。（单位为天）
    /// </summary>
    public int FixedTerm { get; set; }

    /// <summary>
    /// 表示自领取后多少天开始生效。（单位为天）
    /// </summary>
    public int FixedBeginTerm { get; set; }

    /// <summary>
    /// code对应卡券的状态
    /// </summary>
    public UserCardStatus CodeStatus { get; set; }

    /// <summary>
    /// 是否可以核销
    /// </summary>
    public bool CanConsume { get; set; }

    /// <summary>
    /// 代金券专用，表示起用金额（单位为元）
    /// </summary>
    public decimal LeastCost { get; set; }

    /// <summary>
    /// 代金券专用，表示减免金额（单位为元）
    /// </summary>
    public decimal ReduceCost { get; set; }

    /// <summary>
    /// 折扣券专用字段，表示打折额度（百分比），例：填30为七折团购详情。
    /// </summary>
    public int Discount { get; set; }

    /// <summary>
    /// 团购券专用字段，团购详情。
    /// </summary>
    public string DealDetail { get; set; }

    /// <summary>
    /// 礼品券专用，表示礼品名字。
    /// </summary>
    public string Gift { get; set; }

    /// <summary>
    /// 会员卡专属字段，表示是否支持积分，填写true或false，如填写true，积分相关字段均为必填，会员卡专用。
    /// </summary>
    public bool SupplyBalance { get; set; }

    /// <summary>
    /// 会员卡专属字段，表示否支持储值，填写true或false，如填写true，储值相关字段均为必填，会员卡专用。
    /// </summary>
    public bool SupplyBonus { get; set; }

    /// <summary>
    /// 积分清零规则，会员卡专用。
    /// </summary>
    public string BonusCleared { get; set; }

    /// <summary>
    /// 积分规则，会员卡专用。
    /// </summary>
    public string BonusRules { get; set; }

    /// <summary>
    /// 储值规则，会员卡专用。
    /// </summary>
    public string BalanceRules { get; set; }

    /// <summary>
    /// 会员卡专属字段，表示特权说明，会员卡专用。
    /// </summary>
    public string Prerogative { get; set; }

    /// <summary>
    /// 绑定旧卡的url，会员卡专用。
    /// </summary>
    public string BindOldCardUrl { get; set; }

    /// <summary>
    /// 激活会员卡，会员卡专用。
    /// </summary>
    public string ActivateUrl { get; set; }

    /// <summary>
    /// 飞机票的起点，上限为18个汉字，机票专用。
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// 飞机票的终点，上限为18个汉字，机票专用。
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// 航班，机票专用。
    /// </summary>
    public string Flight { get; set; }

    /// <summary>
    /// 起飞时间，机票专用。
    /// </summary>
    public DateTime? DepartureTime { get; set; }

    /// <summary>
    /// 降落时间，机票专用。
    /// </summary>
    public DateTime? LandingTime { get; set; }

    /// <summary>
    /// 登机时间，只显示“时分”不显示日期，机票专用。
    /// </summary>
    public DateTime? BoardingTime { get; set; }

    /// <summary>
    /// 在线值机的链接，机票专用。
    /// </summary>
    public string CheckInUrl { get; set; }

    /// <summary>
    /// 登机口。如发生登机口变更，建议商家实时调用该接口变更，机票专用。
    /// </summary>
    public string Gate { get; set; }

    /// <summary>
    /// 会议详情，会议门票专用。
    /// </summary>
    public string MeetingDetail { get; set; }
    /// <summary>
    /// 会场导览图，会议门票专用。
    /// </summary>
    public string MapUrl { get; set; }


    public WxCard()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public event EventHandler WxCodeConsumed;

    /// <summary>
    /// 获取用户已领取卡券，属于该appid下所有可用卡券，包括正常状态和未生效状态。
    /// 只能获取普通优惠券，不能获取“朋友的券”
    /// </summary>
    /// <param name="openID"></param>
    /// <param name="cardID"></param>
    /// <returns></returns>
    public static List<WxCard> GetCardList(string openID, string cardID = null)
    {
        List<WxCard> cardList = new List<WxCard>();
        WxCard wxCard;

        try
        {
            JsonData jCard = new JsonData(), jCardInfo;
            jCard["openid"] = openID;
            if (!string.IsNullOrEmpty(cardID))
            {
                jCard["card_id"] = cardID;
            }
            string getCardListUrl = GET_CARD_LIST_API + WxJSAPI.GetAccessToken();
            string recvMsg = HttpService.Post(jCard.ToJson(), getCardListUrl, false, Config.WeChatAPITimeout);
            jCardInfo = JsonMapper.ToObject(recvMsg);
            if (jCardInfo.Keys.Contains("errcode") && jCardInfo["errcode"].ToString() == "0" && jCardInfo.Keys.Contains("errmsg") && jCardInfo["errmsg"].ToString() == "ok")
            {
                if (jCardInfo.Keys.Contains("card_list") && jCardInfo["card_list"].IsArray)
                {
                    for (int i = 0; i < jCardInfo["card_list"].Count; i++)
                    {
                        wxCard = GetCard(jCardInfo["card_list"][i]["card_id"].ToString());
                        if (wxCard != null)
                        {
                            wxCard.Code = jCardInfo["card_list"][i]["code"].ToString();
                        }

                        cardList.Add(wxCard);
                    }
                }
            }
            else
            {
                throw new Exception("errcode:" + jCardInfo["errcode"].ToString() + ";" + "errmsg:" + jCardInfo["errmsg"].ToString());
            }
        }
        catch (Exception ex)
        {
            Log.Error("WxCard", ex.Message);
        }
        return cardList;

    }

    /// <summary>
    /// 根据CardID获取微信卡券的详细信息
    /// </summary>
    /// <param name="cardID">微信卡券ID</param>
    /// <returns></returns>
    public static WxCard GetCard(string cardID)
    {
        WxCard wxCard = null;

        try
        {
            JsonData jCardReq = new JsonData(), jCardInfo, jBaseInfo = null;
            jCardReq["card_id"] = cardID;
            string getCardUrl = GET_CARD_API + WxJSAPI.GetAccessToken();
            string cardInfo = HttpService.Post(jCardReq.ToJson(), getCardUrl, false, Config.WeChatAPITimeout);
            jCardInfo = JsonMapper.ToObject(cardInfo);
            if (jCardInfo["errcode"].ToString() == "0" && jCardInfo["errmsg"].ToString() == "ok")
            {
                wxCard = new WxCard();
                switch (jCardInfo["card"]["card_type"].ToString().ToUpper())
                {
                    //代金券
                    case "CASH":
                        JsonData jCash = jCardInfo["card"]["cash"];
                        jBaseInfo = jCash["base_info"];
                        wxCard.CardType = WxCardType.CASH;
                        decimal leastCost, reduceCost;

                        //代金券减免金额
                        if (jCash.Keys.Contains("reduce_cost"))
                        {
                            if (decimal.TryParse(jCash["reduce_cost"].ToString(), out reduceCost))
                            {
                                wxCard.ReduceCost = reduceCost / 100;
                            }
                            else
                            {
                                wxCard.ReduceCost = 0;
                            }
                        }
                        else
                        {
                            wxCard.ReduceCost = 0;
                        }

                        //代金券起用金额
                        if (jCash.Keys.Contains("advanced_info") && jCash["advanced_info"].Keys.Contains("use_condition") && jCash["advanced_info"]["use_condition"].Keys.Contains("least_cost"))
                        {
                            if (decimal.TryParse(jCash["advanced_info"]["use_condition"]["least_cost"].ToString(), out leastCost))
                            {
                                wxCard.LeastCost = leastCost / 100;
                            }
                            else
                            {
                                wxCard.LeastCost = 0;
                            }
                        }
                        else
                        {
                            //无门槛代金券没有起用金额字段least_cost
                            wxCard.LeastCost = 0;
                        }

                        break;
                    //折扣券
                    case "DISCOUNT":
                        JsonData jDiscount = jCardInfo["card"]["discount"];
                        jBaseInfo = jDiscount["base_info"];
                        wxCard.CardType = WxCardType.DISCOUNT;
                        int discount;
                        if (int.TryParse(jDiscount["discount"].ToString(), out discount))
                        {
                            wxCard.Discount = discount;
                        }
                        else
                        {
                            wxCard.Discount = 0;
                        }
                        break;
                    //团购券
                    case "GROUPON":
                        JsonData jGroupon = jCardInfo["card"]["groupon"];
                        jBaseInfo = jGroupon["base_info"];
                        wxCard.CardType = WxCardType.GROUPON;
                        wxCard.DealDetail = jGroupon["deal_detail"].ToString();
                        break;
                    //礼品券
                    case "GIFT":
                        JsonData jGift = jCardInfo["card"]["gift"];
                        jBaseInfo = jGift["base_info"];
                        wxCard.CardType = WxCardType.GIFT;
                        wxCard.Gift = jGift["gift"].ToString();
                        break;
                    //通用券
                    case "GENERAL_COUPON":
                        JsonData jGeneralCoupon = jCardInfo["card"]["general_coupon"];
                        jBaseInfo = jGeneralCoupon["base_info"];
                        wxCard.CardType = WxCardType.GENERAL_COUPON;
                        break;
                    //会员卡
                    case "MEMBER_CARD":
                        JsonData jMemberCard = jCardInfo["card"]["member_card"];
                        jBaseInfo = jMemberCard["base_info"];
                        wxCard.CardType = WxCardType.MEMBER_CARD;
                        //wxCard.SupplyBalance = jMemberCard["supply_balance"].ToString() == "true" ? true : false;
                        //wxCard.SupplyBonus = jMemberCard["supply_bonus"].ToString() == "true" ? true : false;
                        //wxCard.BonusCleared = jMemberCard["bonus_cleared"].ToString();
                        //wxCard.BonusRules = jMemberCard["bonus_rules"].ToString();
                        //wxCard.BalanceRules = jMemberCard["balance_rules"].ToString();
                        //wxCard.Prerogative = jMemberCard["prerogative"].ToString();
                        //wxCard.BindOldCardUrl = jMemberCard["bind_old_card_url"].ToString();
                        //wxCard.ActivateUrl = jMemberCard["activate_url"].ToString();
                        break;
                    //景点门票
                    case "SCENIC_TICKET":
                        JsonData jScenicTicket = jCardInfo["card"]["scenic_ticket"];
                        jBaseInfo = jScenicTicket["base_info"];
                        wxCard.CardType = WxCardType.SCENIC_TICKET;
                        break;
                    //电影票
                    case "MOVIE_TICKET":
                        JsonData jMovieTicket = jCardInfo["card"]["movie_ticket"];
                        jBaseInfo = jMovieTicket["base_info"];
                        wxCard.CardType = WxCardType.MOVIE_TICKET;
                        break;
                    //飞机票
                    case "BOARDING_PASS":
                        JsonData jBoardingPass = jCardInfo["card"]["boarding_pass"];
                        jBaseInfo = jBoardingPass["base_info"];
                        wxCard.CardType = WxCardType.BOARDING_PASS;
                        break;
                    //会议门票
                    case "MEETING_TICKET":
                        JsonData jMeetingTicket = jCardInfo["card"]["meeting_ticket"];
                        jBaseInfo = jMeetingTicket["base_info"];
                        wxCard.CardType = WxCardType.MEETING_TICKET;
                        break;
                    //汽车票
                    case "BUS_TICKET":
                        JsonData jBusTicket = jCardInfo["card"]["bus_ticket"];
                        jBaseInfo = jBusTicket["base_info"];
                        wxCard.CardType = WxCardType.BUS_TICKET;
                        break;
                    default:
                        throw new Exception("未知的微信卡券类型：" + jCardInfo["card"]["card_type"].ToString());
                }

                if (jBaseInfo != null)
                {
                    wxCard.CardID = cardID;
                    wxCard.LogoUrl = jBaseInfo["logo_url"].ToString();
                    wxCard.BrandName = jBaseInfo["brand_name"].ToString();
                    wxCard.Title = jBaseInfo["title"].ToString();
                    wxCard.SubTitle = jBaseInfo["sub_title"].ToString();
                    wxCard.CanShare = jBaseInfo["can_share"].ToString() == "true" ? true : false;
                    wxCard.CanGiveFriend = jBaseInfo["can_give_friend"].ToString() == "true" ? true : false;
                    int getLimit, quantity, totalQuantity;
                    if (int.TryParse(jBaseInfo["get_limit"].ToString(), out getLimit))
                    {
                        wxCard.GetLimit = getLimit;
                    }
                    else
                    {
                        wxCard.GetLimit = 1;
                    }
                    if (int.TryParse(jBaseInfo["sku"]["quantity"].ToString(), out quantity))
                    {
                        wxCard.Quantity = quantity;
                    }
                    else
                    {
                        wxCard.Quantity = 0;
                    }
                    if (int.TryParse(jBaseInfo["sku"]["total_quantity"].ToString(), out totalQuantity))
                    {
                        wxCard.TotalQuantity = totalQuantity;
                    }
                    else
                    {
                        wxCard.TotalQuantity = 0;
                    }
                    switch (jBaseInfo["code_type"].ToString().ToUpper())
                    {
                        case "CODE_TYPE_TEXT":
                            wxCard.CodeType = WxCodeType.CODE_TYPE_TEXT;
                            break;
                        case "CODE_TYPE_BARCODE":
                            wxCard.CodeType = WxCodeType.CODE_TYPE_BARCODE;
                            break;
                        case "CODE_TYPE_QRCODE":
                            wxCard.CodeType = WxCodeType.CODE_TYPE_QRCODE;
                            break;
                        case "CODE_TYPE_ONLY_QRCODE":
                            wxCard.CodeType = WxCodeType.CODE_TYPE_ONLY_QRCODE;
                            break;
                        case "CODE_TYPE_ONLY_BARCODE":
                            wxCard.CodeType = WxCodeType.CODE_TYPE_ONLY_BARCODE;
                            break;
                        case "CODE_TYPE_NONE":
                            wxCard.CodeType = WxCodeType.CODE_TYPE_NONE;
                            break;
                    }
                    switch (jBaseInfo["status"].ToString().ToUpper())
                    {
                        case "CARD_STATUS_NOT_VERIFY":
                            wxCard.Status = CardStatus.CARD_STATUS_NOT_VERIFY;
                            break;
                        case "CARD_STATUS_VERIFY_FAIL":
                            wxCard.Status = CardStatus.CARD_STATUS_VERIFY_FAIL;
                            break;
                        case "CARD_STATUS_VERIFY_OK":
                            wxCard.Status = CardStatus.CARD_STATUS_VERIFY_OK;
                            break;
                        case "CARD_STATUS_USER_DELETE":
                            wxCard.Status = CardStatus.CARD_STATUS_USER_DELETE;
                            break;
                        case "CARD_STATUS_DISPATCH":
                            wxCard.Status = CardStatus.CARD_STATUS_DISPATCH;
                            break;
                    }
                    switch (jBaseInfo["date_info"]["type"].ToString().ToUpper())
                    {
                        case "DATE_TYPE_FIX_TIME_RANGE":
                            wxCard.DateType = AvailableDateType.DATE_TYPE_FIX_TIME_RANGE;
                            wxCard.BeginTime = UtilityHelper.GetTime(jBaseInfo["date_info"]["begin_timestamp"].ToString());
                            wxCard.EndTime = UtilityHelper.GetTime(jBaseInfo["date_info"]["end_timestamp"].ToString());
                            break;
                        case "DATE_TYPE_FIX_TERM":
                            wxCard.DateType = AvailableDateType.DATE_TYPE_FIX_TERM;
                            int fixedTerm, fixedBeginTerm;
                            if (int.TryParse(jBaseInfo["date_info"]["fixed_term"].ToString(), out fixedTerm))
                            {
                                wxCard.FixedTerm = fixedTerm;
                            }
                            else
                            {
                                wxCard.FixedTerm = 0;
                            }
                            if (int.TryParse(jBaseInfo["date_info"]["fixed_begin_term"].ToString(), out fixedBeginTerm))
                            {
                                wxCard.FixedBeginTerm = fixedBeginTerm;
                            }
                            else
                            {
                                wxCard.FixedBeginTerm = 0;
                            }
                            break;
                        case "DATE_TYPE_PERMANENT":
                            wxCard.DateType = AvailableDateType.DATE_TYPE_PERMANENT;
                            break;
                    }
                }
                else
                {
                    throw new Exception("未能获取微信卡券base_info");
                }
            }
            else
            {
                throw new Exception("errcode:" + jCardInfo["errcode"].ToString() + ";" + "errmsg:" + jCardInfo["errmsg"].ToString());
            }
        }
        catch (Exception ex)
        {
            Log.Error("WxCard", ex.Message);
            return null;
        }

        return wxCard;
    }

    /// <summary>
    /// 查询Code
    /// </summary>
    /// <param name="cardID"></param>
    /// <param name="code"></param>
    /// <param name="checkConsume">是否校验code核销状态</param>
    /// <returns></returns>
    public static UserCardStatus GetCode(string cardID, string code, bool checkConsume)
    {
        UserCardStatus userCardStatus = UserCardStatus.NORMAL;

        try
        {
            JsonData jCardReq = new JsonData(), jCardInfo;
            jCardReq["card_id"] = cardID;
            jCardReq["code"] = code;
            jCardReq["check_consume"] = checkConsume;
            string getCardUrl = GET_CODE_API + WxJSAPI.GetAccessToken();
            string cardInfo = HttpService.Post(jCardReq.ToJson(), getCardUrl, false, Config.WeChatAPITimeout);
            jCardInfo = JsonMapper.ToObject(cardInfo);
            if (checkConsume)
            {
                if (jCardInfo["errcode"].ToString() == "0" && jCardInfo["errmsg"].ToString() == "ok")
                {
                    switch (jCardInfo["user_card_status"].ToString().ToUpper())
                    {
                        case "NORMAL":
                            userCardStatus = UserCardStatus.NORMAL;
                            break;
                        case "CONSUMED":
                            userCardStatus = UserCardStatus.CONSUMED;
                            break;
                        case "EXPIRE":
                            userCardStatus = UserCardStatus.EXPIRE;
                            break;
                        case "GIFTING":
                            userCardStatus = UserCardStatus.GIFTING;
                            break;
                        case "GIFT_TIMEOUT":
                            userCardStatus = UserCardStatus.GIFT_TIMEOUT;
                            break;
                        case "DELETE":
                            userCardStatus = UserCardStatus.DELETE;
                            break;
                        case "UNAVAILABLE":
                            userCardStatus = UserCardStatus.UNAVAILABLE;
                            break;
                        default:
                            userCardStatus = UserCardStatus.INVALID_SERIAL_CODE;
                            break;
                    }
                }
                else
                {
                    userCardStatus = UserCardStatus.INVALID_SERIAL_CODE;
                }
            }
            else
            {
                switch (jCardInfo["user_card_status"].ToString().ToUpper())
                {
                    case "NORMAL":
                        userCardStatus = UserCardStatus.NORMAL;
                        break;
                    case "CONSUMED":
                        userCardStatus = UserCardStatus.CONSUMED;
                        break;
                    case "EXPIRE":
                        userCardStatus = UserCardStatus.EXPIRE;
                        break;
                    case "GIFTING":
                        userCardStatus = UserCardStatus.GIFTING;
                        break;
                    case "GIFT_TIMEOUT":
                        userCardStatus = UserCardStatus.GIFT_TIMEOUT;
                        break;
                    case "DELETE":
                        userCardStatus = UserCardStatus.DELETE;
                        break;
                    case "UNAVAILABLE":
                        userCardStatus = UserCardStatus.UNAVAILABLE;
                        break;
                    default:
                        userCardStatus = UserCardStatus.INVALID_SERIAL_CODE;
                        break;
                }

            }
        }
        catch (Exception ex)
        {
            Log.Error("WxCard", ex.Message);
        }

        return userCardStatus;
    }

    /// <summary>
    /// 微信卡券CODE解码
    /// 应用场景：
    /// 1.商家获取choos_card_info后，将card_id和encrypt_code字段通过解码接口，获取真实code。 
    /// 2.卡券内跳转外链的签名中会对code进行加密处理，通过调用解码接口获取真实code。
    /// 注意事项：
    /// 1.只能解码本公众号卡券获取的加密code。 
    /// 2.开发者若从url上获取到加密code,请注意先进行urldecode，否则报错。
    /// 3.encrypt_code是卡券的code码经过加密处理得到的加密code码，与code一一对应。
    /// 4.开发者只能解密本公众号的加密code，否则报错。
    /// </summary>
    /// <param name="encryptCode"></param>
    /// <returns></returns>
    public static string DecryptCode(string encryptCode)
    {
        string decryptCode = string.Empty;

        try
        {
            JsonData jCardReq = new JsonData(), jDecryptCode;
            jCardReq["encrypt_code"] = encryptCode;
            string decryptCodeUrl = DECRYPT_CODE_API + WxJSAPI.GetAccessToken();
            string cardInfo = HttpService.Post(jCardReq.ToJson(), decryptCodeUrl, false, Config.WeChatAPITimeout);
            jDecryptCode = JsonMapper.ToObject(cardInfo);
            if (jDecryptCode["errcode"].ToString() == "0" && jDecryptCode["errmsg"].ToString() == "ok")
            {
                decryptCode = jDecryptCode["code"].ToString();
            }
            else
            {
                throw new Exception("errcode:" + jDecryptCode["errcode"].ToString() + ";" + "errmsg:" + jDecryptCode["errmsg"].ToString());
            }
        }
        catch (Exception ex)
        {
            Log.Error("WxCard", ex.Message);
        }

        return decryptCode;
    }

    /// <summary>
    /// 核销Code，是核销卡券的唯一接口,开发者可以调用当前接口将用户的优惠券进行核销，该过程不可逆。
    /// 1.仅支持核销有效状态的卡券，若卡券处于异常状态，均不可核销。（异常状态包括：卡券删除、未生效、过期、转赠中、转赠退回、失效）
    /// 2.自定义Code码（use_custom_code为true）的优惠券，在code被核销时，必须调用此接口。用于将用户客户端的code状态变更。自定义code的卡券调用接口时， post数据中需包含card_id，否则报invalid serial code，非自定义code不需上报。
    /// </summary>
    /// <param name="code"></param>
    /// <param name="cardID"></param>
    /// <returns></returns>
    public static bool ConsumeCode(string code, string cardID = null)
    {
        bool isConsumed = false;

        try
        {
            JsonData jCardReq = new JsonData(), jCardInfo;
            jCardReq["code"] = code;
            if (!string.IsNullOrEmpty(cardID))
            {
                jCardReq["card_id"] = cardID;
            }
            string getCardUrl = CONSUME_CODE_API + WxJSAPI.GetAccessToken();
            string cardInfo = HttpService.Post(jCardReq.ToJson(), getCardUrl, false, Config.WeChatAPITimeout);
            jCardInfo = JsonMapper.ToObject(cardInfo);
            if (jCardInfo["errcode"].ToString() == "0" && jCardInfo["errmsg"].ToString() == "ok")
            {
                isConsumed = true;
            }
            else
            {
                isConsumed = false;
            }
        }
        catch (Exception ex)
        {
            Log.Error("WxCard", ex.Message);
            isConsumed = false;
        }

        return isConsumed;
    }

    public static JsonData ConsumeCodeOnOrderStateChanged(ProductOrder po, ProductOrder.OrderStateEventArgs e)
    {
        if (po == null || e == null)
        {
            throw new ArgumentNullException("sender或事件参数对象不能为null");
        }
        JsonData jRet = new JsonData();

        try
        {
            //只有支付成功才核销微信优惠券
            if ((po.PaymentTerm == PaymentTerm.WECHAT && po.TradeState == TradeState.SUCCESS)
              || (po.PaymentTerm == PaymentTerm.ALIPAY && (po.TradeState == TradeState.AP_TRADE_SUCCESS || po.TradeState == TradeState.AP_TRADE_FINISHED))
              || (po.PaymentTerm == PaymentTerm.CASH && po.TradeState == TradeState.CASHPAID))
            {
                if (po.WxCard != null && !string.IsNullOrEmpty(po.WxCard.Code))
                {
                    if (ConsumeCode(po.WxCard.Code))
                    {
                        jRet["IsConsumed"] = true;
                    }
                    else
                    {
                        jRet["IsConsumed"] = false;
                    }
                }
                else
                {
                    jRet["IsConsumed"] = false;
                }
            }
            else
            {
                jRet["IsConsumed"] = false;
            }
        }
        catch (Exception ex)
        {
            Log.Error("ConsumeCodeOnOrderStateChanged", ex.Message);
            throw ex;
        }

        return jRet;
    }

}

/// <summary>
/// 微信卡券类型
/// </summary>
public enum WxCardType
{
    /// <summary>
    /// 代金券
    /// </summary>
    CASH = 1,
    /// <summary>
    /// 折扣券
    /// </summary>
    DISCOUNT = 2,
    /// <summary>
    /// 团购券
    /// </summary>
    GROUPON = 3,
    /// <summary>
    /// 礼品券
    /// </summary>
    GIFT = 4,
    /// <summary>
    /// 通用券
    /// </summary>
    GENERAL_COUPON = 5,
    /// <summary>
    /// 会员卡
    /// </summary>
    MEMBER_CARD = 6,
    /// <summary>
    /// 景点门票
    /// </summary>
    SCENIC_TICKET = 7,
    /// <summary>
    /// 电影票
    /// </summary>
    MOVIE_TICKET = 8,
    /// <summary>
    /// 飞机票
    /// </summary>
    BOARDING_PASS = 9,
    /// <summary>
    /// 会议门票
    /// </summary>
    MEETING_TICKET = 10,
    /// <summary>
    /// 汽车票
    /// </summary>
    BUS_TICKET = 11
}

/// <summary>
/// 微信卡券Code类型
/// </summary>
public enum WxCodeType
{
    /// <summary>
    /// 文本
    /// </summary>
    CODE_TYPE_TEXT = 1,
    /// <summary>
    /// 一维码
    /// </summary>
    CODE_TYPE_BARCODE = 2,
    /// <summary>
    /// 二维码
    /// </summary>
    CODE_TYPE_QRCODE = 3,
    /// <summary>
    /// 二维码无code显示
    /// </summary>
    CODE_TYPE_ONLY_QRCODE = 4,
    /// <summary>
    /// 一维码无code显示
    /// </summary>
    CODE_TYPE_ONLY_BARCODE = 5,
    /// <summary>
    /// 无code类型
    /// </summary>
    CODE_TYPE_NONE = 6
}

/// <summary>
/// 卡券状态
/// </summary>
public enum CardStatus
{
    /// <summary>
    /// 待审核
    /// </summary>
    CARD_STATUS_NOT_VERIFY = 1,
    /// <summary>
    /// 审核失败
    /// </summary>
    CARD_STATUS_VERIFY_FAIL = 2,
    /// <summary>
    /// 通过审核
    /// </summary>
    CARD_STATUS_VERIFY_OK = 3,
    /// <summary>
    /// 卡券被商户删除
    /// </summary>
    CARD_STATUS_USER_DELETE = 4,
    /// <summary>
    /// 在公众平台投放过的卡券
    /// </summary>
    CARD_STATUS_DISPATCH = 5
}

/// <summary>
/// 当前code对应卡券的状态
/// </summary>
public enum UserCardStatus
{
    /// <summary>
    /// 正常
    /// </summary>
    NORMAL = 1,
    /// <summary>
    /// 已核销
    /// </summary>
    CONSUMED = 2,
    /// <summary>
    /// 已过期
    /// </summary>
    EXPIRE = 3,
    /// <summary>
    /// 转赠中
    /// </summary>
    GIFTING = 4,
    /// <summary>
    /// 转赠超时
    /// </summary>
    GIFT_TIMEOUT = 5,
    /// <summary>
    /// 已删除
    /// </summary>
    DELETE = 6,
    /// <summary>
    /// 已失效
    /// </summary>
    UNAVAILABLE = 7,
    /// <summary>
    /// code未被添加或被转赠领取
    /// </summary>
    INVALID_SERIAL_CODE = 8
}

/// <summary>
/// 卡券有效期类型
/// </summary>
public enum AvailableDateType
{
    /// <summary>
    /// 表示固定日期区间
    /// </summary>
    DATE_TYPE_FIX_TIME_RANGE = 1,
    /// <summary>
    /// 表示固定时长（自领取后按天算）
    /// </summary>
    DATE_TYPE_FIX_TERM = 2,
    /// <summary>
    /// 表示永久有效（会员卡类型专用）
    /// </summary>
    DATE_TYPE_PERMANENT = 3
}
