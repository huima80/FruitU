﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// OrderDetail 的摘要说明
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

    public OrderDetail()
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
    }

    public OrderDetail(int purchaseQty, decimal purchasePrice)
    {
        //
        // TODO: 在此处添加构造函数逻辑
        //
        this.PurchaseQty = purchaseQty;
        this.PurchasePrice = purchasePrice;
    }
}