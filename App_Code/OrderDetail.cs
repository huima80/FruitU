using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// 订单商品项
/// </summary>
public class OrderDetail : Fruit
{
    public new int ID { get; set; }

    /// <summary>
    /// 订单明细对应的商品ID
    /// </summary>
    public int ProductID { get; set; }

    /// <summary>
    /// 订单商品名称
    /// </summary>
    public string OrderProductName { get; set; }

    /// <summary>
    /// 订单购买商品数量
    /// </summary>
    public int PurchaseQty { get; set; }

    /// <summary>
    /// 订单购买商品价格
    /// </summary>
    public decimal PurchasePrice { get; set; }

    /// <summary>
    /// 订单购买商品单位
    /// </summary>
    public string PurchaseUnit { get; set; }

    /// <summary>
    /// 订单商品项对应的团购活动
    /// </summary>
    public GroupPurchaseEvent GroupPurchaseEvent { get; set; }

    public OrderDetail()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public OrderDetail(Fruit fruit) : base(fruit.ID, fruit.FruitName, fruit.Category, fruit.FruitPrice, fruit.FruitUnit, fruit.FruitImgList, fruit.FruitDesc, fruit.InventoryQty, fruit.OnSale, fruit.IsSticky, fruit.Priority)
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }
}