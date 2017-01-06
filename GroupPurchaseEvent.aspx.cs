using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

public partial class GroupPurchaseEventInfo : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        WeChatUser wxUser = Session["WxUser"] as WeChatUser;

        try
        {
            if (!string.IsNullOrEmpty(Request.QueryString["EventID"]))
            {
                int eventID;
                if (int.TryParse(Request.QueryString["EventID"], out eventID))
                {
                    GroupPurchaseEvent groupEvent;
                    groupEvent = GroupPurchaseEvent.FindGroupPurchaseEventByID(eventID);
                    if (groupEvent != null)
                    {
                        //显示团购活动对应的团购信息
                        this.lblGroupName.Text = groupEvent.GroupPurchase.Name;
                        this.lblGroupDesc.Text = groupEvent.GroupPurchase.Description;
                        FruitImg mainImg = groupEvent.GroupPurchase.Product.FruitImgList.Find(img => img.MainImg);
                        if (mainImg != default(FruitImg))
                        {
                            this.imgProdImg.ImageUrl = "images/" + mainImg.ImgName;
                        }
                        else
                        {
                            this.imgProdImg.ImageUrl = "images/" + Config.DefaultImg;
                        }
                        this.lblRequiredNumber.Text = groupEvent.GroupPurchase.RequiredNumber.ToString();
                        this.lblProdPrice.Text = groupEvent.GroupPurchase.Product.FruitPrice.ToString();
                        this.lblProdPriceUnit.Text = groupEvent.GroupPurchase.Product.FruitUnit;
                        this.lblGroupPrice.Text = groupEvent.GroupPurchase.GroupPrice.ToString();
                        this.lblGroupPriceUnit.Text = groupEvent.GroupPurchase.Product.FruitUnit;
                        this.lblEventStardDate.Text = groupEvent.GroupPurchase.StartDate.ToString();
                        this.lblEventEndDate.Text = groupEvent.GroupPurchase.EndDate.ToString();

                        //查询此团购活动中已支付的成员
                        List<GroupPurchaseEventMember> paidEventMembers = groupEvent.GroupPurchaseEventMembers.FindAll(member => member.IsPaid);

                        //此团购活动还缺的人数
                        int leftNumber;
                        if (groupEvent.GroupPurchaseEventMembers != null)
                        {
                            if (groupEvent.GroupPurchase.RequiredNumber > paidEventMembers.Count)
                            {
                                leftNumber = groupEvent.GroupPurchase.RequiredNumber - paidEventMembers.Count;
                            }
                            else
                            {
                                leftNumber = 0;
                            }
                        }
                        else
                        {
                            leftNumber = groupEvent.GroupPurchase.RequiredNumber;
                        }
                        if (leftNumber > 0)
                        {
                            this.lblLeftNumber.Text = leftNumber.ToString();
                        }
                        else
                        {
                            this.divLeftNumber.Visible = false;
                        }

                        //只显示已支付成功的团购活动成员头像和列表
                        if (paidEventMembers != null)
                        {
                            string strEventMemberList = string.Empty, strEventMemberHeadImg = string.Empty;
                            //团购活动的成员
                            paidEventMembers.ForEach(member =>
                            {
                                strEventMemberHeadImg += string.Format("<img src='{0}'/>", member.GroupMember.HeadImgUrl);
                                strEventMemberList += string.Format("<div class='col-xs-12 user-portrait {3}'><img src='{0}'/> 【{1}】 {2} {4}{5}</div>", member.GroupMember.HeadImgUrl, member.GroupMember.NickName, member.JoinDate.ToString(), !member.IsPaid ? "text-muted" : string.Empty, (member.GroupMember.OpenID == member.GroupPurchaseEvent.Organizer.OpenID) ? "开团" : "参团", !member.IsPaid ? "（未支付）" : string.Empty);
                            });
                            //尚缺的团购活动成员头像
                            for (int i = 0; i < leftNumber; i++)
                            {
                                strEventMemberHeadImg += "<span class='empty-user-portrait'>?</span>";
                                if ((groupEvent.GroupPurchaseEventMembers.Count + i + 1) % 8 == 0)
                                {
                                    strEventMemberHeadImg += "<br/>";
                                }
                            }
                            this.divGroupEventMemberHeadImg.InnerHtml = strEventMemberHeadImg;
                            this.divGroupEventMember.InnerHtml = strEventMemberList;
                        }

                        DateTime nowTime = DateTime.Now;
                        if (nowTime >= groupEvent.GroupPurchase.StartDate && nowTime <= groupEvent.GroupPurchase.EndDate)
                        {
                            //如果已支付成员数未达到要求人数，则允许新用户加入团购活动，否则不允许加入
                            if (paidEventMembers.Count < groupEvent.GroupPurchase.RequiredNumber)
                            {
                                this.divShareGroupEvent.Visible = true;
                                this.divJoinGroupEvent.Visible = true;
                            }
                            else
                            {
                                this.divShareGroupEvent.Visible = false;
                                this.divJoinGroupEvent.Visible = false;
                                this.divCountDown.InnerHtml = "团购已经结束，欢迎下次参加！";
                            }
                        }
                        else
                        {
                            this.divShareGroupEvent.Visible = false;
                            this.divJoinGroupEvent.Visible = false;

                            if (nowTime < groupEvent.GroupPurchase.StartDate)
                            {
                                this.divCountDown.InnerHtml = "团购还未开始，敬请期待！";
                            }
                            else
                            {
                                this.divCountDown.InnerHtml = "团购已经结束，欢迎下次参加！";
                            }
                        }

                        //把团购活动对象序列化为JSON对象，格式化其中的日期属性
                        JsonConvert.DefaultSettings = new Func<JsonSerializerSettings>(() =>
                        {
                            JsonSerializerSettings jSetting = new JsonSerializerSettings();
                            jSetting.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
                            jSetting.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                            jSetting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                            return jSetting;
                        });
                        string strGroupEvent = JsonConvert.SerializeObject(groupEvent);

                        //设置团购商品信息，用于添加入购物车
                        ScriptManager.RegisterStartupScript(Page, this.GetType(), "jsGroupEvent", string.Format("var groupEvent={0};", strGroupEvent), true);

                    }
                    else
                    {
                        throw new Exception("不能找到ID对应的团购活动");
                    }
                }
                else
                {
                    throw new Exception("团购活动ID错误");
                }
            }
            else
            {
                throw new Exception("请指定团购ID");
            }
        }
        catch (Exception ex)
        {
            this.lblMsg.Text = ex.Message;
            this.lblMsg.Visible = true;
            this.divContainer.Visible = false;
        }
    }

}