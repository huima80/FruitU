CREATE PROCEDURE [dbo].[spProductQueryPager]
	@StartRowIndex int,                --开始行索引，首行从0开始
	@MaximumRows int,                --每页行数
	@strWhere varchar(3000),    --查询条件
	@strOrder varchar(1000),    --排序条件
	@categoryOfTopSelling int,    --爆款的商品类别
	@TotalRows int output,            --返回总记录数
	@TopSellingIDWeekly int output,            --返回本周爆款商品ID
	@TopSellingIDMonthly int output            --返回本月爆款商品ID
AS
declare @strSqlTopSellingIDWeekly nvarchar(max)--查询本周爆款商品ID语句
declare @strSqlTopSellingIDMonthly nvarchar(max)--查询本月爆款商品ID语句

--------------设置周一为本周第一天---------------
SET DATEFIRST 1

--------------本周爆款商品ID---------------
set @strSqlTopSellingIDWeekly='select top 1 @TopSellingID=ProductID from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id left join Product on OrderDetail.ProductID = Product.Id where ProductOnSale=1 and (datediff(dd,OrderDate,getdate()) <= datepart(dw,getdate())) and CategoryID=' + STR(@categoryOfTopSelling) + ' group by ProductID order by sum(PurchaseQty) desc'
exec sp_executesql @strSqlTopSellingIDWeekly,N'@TopSellingID int output',@TopSellingIDWeekly output

--------------本月爆款商品ID---------------
set @strSqlTopSellingIDMonthly='select top 1 @TopSellingID=ProductID from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id left join Product on OrderDetail.ProductID = Product.Id where ProductOnSale=1 and (datepart(mm,OrderDate) = datepart(mm,getdate())) and CategoryID=' + STR(@categoryOfTopSelling) + ' group by ProductID order by sum(PurchaseQty) desc'
exec sp_executesql @strSqlTopSellingIDMonthly,N'@TopSellingID int output',@TopSellingIDMonthly output

--------------订单记录集、总订单数---------------
exec spSqlPageByRowNum 'Product left join Category on Product.CategoryID = Category.Id','Product.Id','Product.*,Category.ParentID,Category.CategoryName',@StartRowIndex,@MaximumRows,@strWhere,@strOrder,@TotalRows output