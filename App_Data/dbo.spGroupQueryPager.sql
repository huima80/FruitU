CREATE PROCEDURE [dbo].[spGroupQueryPager]
	@StartRowIndex int,                --开始行索引，首行从0开始
	@MaximumRows int,                --每页行数
	@strWhere varchar(3000),    --查询条件
	@strOrder varchar(1000),    --排序条件
	@TotalRows int output            --返回总记录数

AS
--------------团购记录集、总团购数---------------
exec spSqlPageByRowNum 'GroupPurchase','Id','*',@StartRowIndex,@MaximumRows,@strWhere,@strOrder,@TotalRows output