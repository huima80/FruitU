CREATE PROCEDURE [dbo].[spOrderQueryPager]
	@StartRowIndex int,                --开始行索引，首行从0开始
	@MaximumRows int,                --每页行数
	@strWhere varchar(3000),    --查询条件
	@strOrder varchar(1000),    --排序条件
	@TotalRows int output,            --返回总记录数
	@PayingOrderCount int output,            --返回未支付订单数
	@DeliveringOrderCount int output,            --返回未发货订单数
	@AcceptingOrderCount int output,            --返回未签收订单数
	@CancelledOrderCount int output,            --返回已撤单订单数
	@TotalOrderPrice decimal(18,2) output            --返回订单总金额
AS
declare @strSqlPayingOrderCount nvarchar(max)--查询未支付订单数主语句
declare @strSqlDeliveringOrderCount nvarchar(max)--查询未发货订单数主语句
declare @strSqlAcceptingOrderCount nvarchar(max)--查询未签收订单数主语句
declare @strSqlCancelledOrderCount nvarchar(max)--查询已撤单订单数语句
declare @strSqlOrderPrice nvarchar(max)--查询订单总金额语句
declare @strWhr nvarchar(max),@strWhr1 nvarchar(max),@strWhr2 nvarchar(max),@strWhr3 nvarchar(max),@strWhr4 nvarchar(max)

--------------条件字段---------------
if @strWhere <> '' and (@strWhere is not null)
	begin
		set @strWhr = ' where ' + @strWhere
		set @strWhr1 = ' where ' + @strWhere + ' and TradeState not in (1,8,12,14)'
		set @strWhr2 = ' where ' + @strWhere + ' and IsDelivered=0'
		set @strWhr3 = ' where ' + @strWhere + ' and IsAccept=0'
		set @strWhr4 = ' where ' + @strWhere + ' and IsCancel=1'
	end
else
	begin
		set @strWhr = ''
		set @strWhr1 = ' where TradeState not in (1,8,12,14)'
		set @strWhr2 = ' where IsDelivered=0'
		set @strWhr3 = ' where IsAccept=0'
		set @strWhr4 = ' where IsCancel=1'
	end

--------------未支付订单数---------------
set @strSqlPayingOrderCount='Select @TotalCount=count(*) from ProductOrder ' + @strWhr1
exec sp_executesql @strSqlPayingOrderCount,N'@TotalCount int output',@PayingOrderCount output

--------------未配送订单数---------------
set @strSqlDeliveringOrderCount='Select @TotalCount=count(*) from ProductOrder ' + @strWhr2
exec sp_executesql @strSqlDeliveringOrderCount,N'@TotalCount int output',@DeliveringOrderCount output

--------------未签收订单数---------------
set @strSqlAcceptingOrderCount='Select @TotalCount=count(*) from ProductOrder ' + @strWhr3
exec sp_executesql @strSqlAcceptingOrderCount,N'@TotalCount int output',@AcceptingOrderCount output

--------------已撤单订单数---------------
set @strSqlCancelledOrderCount='Select @TotalCount=count(*) from ProductOrder ' + @strWhr4
exec sp_executesql @strSqlCancelledOrderCount,N'@TotalCount int output',@CancelledOrderCount output

--------------订单总金额---------------
set @strSqlOrderPrice='select @TotalPrice=SUM(OrderPrice) from (select ((select sum(PurchasePrice*PurchaseQty) from OrderDetail where OrderDetail.PoID = ProductOrder.Id) + Freight - MemberPointsDiscount - WxCardDiscount) as OrderPrice from ProductOrder '+ @strWhr +') t'
exec sp_executesql @strSqlOrderPrice,N'@TotalPrice decimal(18,2) output',@TotalOrderPrice output

--------------订单记录集、总订单数---------------
exec spSqlPageByRowNum 'ProductOrder','ProductOrder.Id','ProductOrder.*',@StartRowIndex,@MaximumRows,@strWhere,@strOrder,@TotalRows output