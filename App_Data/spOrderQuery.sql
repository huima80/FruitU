CREATE PROCEDURE [dbo].[spOrderQuery]
	@tbName varchar(3000),        --表名
	@PK varchar(50),			--主键字段
	@tbFields varchar(3000),      --返回字段
	@StartRowIndex int,                --开始行索引，首行从0开始
	@MaximumRows int,                --每页行数
	@strWhere varchar(3000),    --查询条件
	@strOrder varchar(1000),    --排序条件
	@TotalRows int output,            --返回总记录数
	@PayingOrderCount int output,            --返回未支付订单数
	@DeliveringOrderCount int output,            --返回未配送订单数
	@AcceptingOrderCount int output            --返回未签收订单数
AS
declare @strSqlPayingOrderCount nvarchar(max)--查询记录总数主语句
declare @strSqlDeliveringOrderCount nvarchar(max)--查询记录总数主语句
declare @strSqlAcceptingOrderCount nvarchar(max)--查询记录总数主语句
declare @strWhr1 nvarchar(max),@strWhr2 nvarchar(max),@strWhr3 nvarchar(max)

--------------条件字段---------------
if @strWhere <> '' and (@strWhere is not null)
	begin
		set @strWhr1 = ' where ' + @strWhere + ' and TradeState<>1 and TradeState<>8'
		set @strWhr2 = ' where ' + @strWhere + ' and IsDelivered=0'
		set @strWhr3 = ' where ' + @strWhere + ' and IsAccept=0'
	end
else
	begin
		set @strWhr1 = ' where TradeState<>1 and TradeState<>8'
		set @strWhr2 = ' where IsDelivered=0'
		set @strWhr3 = ' where IsAccept=0'
	end

--------------未支付订单数---------------
set @strSqlPayingOrderCount='Select @TotalCout=count(*) from ' + @tbName + @strWhr1
exec sp_executesql @strSqlPayingOrderCount,N'@TotalCout int output',@PayingOrderCount output

--------------未配送订单数---------------
set @strSqlDeliveringOrderCount='Select @TotalCout=count(*) from ' + @tbName + @strWhr2
exec sp_executesql @strSqlDeliveringOrderCount,N'@TotalCout int output',@DeliveringOrderCount output

--------------未签收订单数---------------
set @strSqlAcceptingOrderCount='Select @TotalCout=count(*) from ' + @tbName + @strWhr3
exec sp_executesql @strSqlAcceptingOrderCount,N'@TotalCout int output',@AcceptingOrderCount output

--------------订单记录集、总订单数---------------
exec spSqlPageByRowNum @tbName,@PK,@tbFields,@StartRowIndex,@MaximumRows,@strWhere,@strOrder,@TotalRows output
