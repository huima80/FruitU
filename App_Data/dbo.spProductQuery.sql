CREATE PROCEDURE [dbo].[spProductQuery]
	@CategoryID varchar(500),
	@TopSellingIDWeekly int output,            --返回本周爆款商品ID
	@TopSellingIDMonthly int output            --返回本月爆款商品ID
AS
declare @strSqlProductQuery nvarchar(max)--查询符合条件的商品记录
declare @strSqlTopSellingIDWeekly nvarchar(max)--查询本周爆款商品ID语句
declare @strSqlTopSellingIDMonthly nvarchar(max)--查询本月爆款商品ID语句

--------------设置周一为本周第一天---------------
SET DATEFIRST 1

--------------本周爆款商品ID---------------
set @strSqlTopSellingIDWeekly = 'select top 1 @TopSellingID=ProductID from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id left join Product on OrderDetail.ProductID = Product.Id where ProductOnSale=1 and (datediff(dd,OrderDate,getdate()) <= datepart(dw,getdate())) and CategoryID in ('+ @CategoryID +') group by ProductID order by sum(PurchaseQty) desc'
exec sp_executesql @strSqlTopSellingIDWeekly,N'@TopSellingID int output',@TopSellingIDWeekly output

--------------本月爆款商品ID---------------
set @strSqlTopSellingIDMonthly = 'select top 1 @TopSellingID=ProductID from OrderDetail left join ProductOrder on OrderDetail.PoID = ProductOrder.Id left join Product on OrderDetail.ProductID = Product.Id where ProductOnSale=1 and (datepart(mm,OrderDate) = datepart(mm,getdate())) and CategoryID in ('+ @CategoryID +') group by ProductID order by sum(PurchaseQty) desc'
exec sp_executesql @strSqlTopSellingIDMonthly,N'@TopSellingID int output',@TopSellingIDMonthly output

--------------订单所有果汁商品---------------
set @strSqlProductQuery = 'select Product.*,Category.ParentID,Category.CategoryName from Product left join Category on Product.CategoryID = Category.Id where ProductOnSale=1 and CategoryID in ('+ @CategoryID +')'
exec sp_executesql @strSqlProductQuery