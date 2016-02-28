CREATE proc [dbo].[spSqlPageByRowNum]
@tbName varchar(255),        --表名
@PK varchar(50),			--主键字段
@tbFields varchar(1000),      --返回字段
@StartRowIndex int,                --开始行索引，首行从0开始
@MaximumRows int,                --每页行数
@strWhere varchar(1000),    --查询条件
@strOrder varchar(1000),    --排序条件
@TotalRows int output            --返回总记录数
as
declare @strSql nvarchar(max)    --主语句
declare @strSqlCount nvarchar(max)--查询记录总数主语句

--------------排序字段---------------
if @strOrder <> '' and (@strOrder is not null)
	begin
		set @strOrder = @strOrder + ',' + @PK + ' desc'
	end
else
	begin
		set @strOrder = @PK + ' desc'
	end

--------------条件字段---------------
if @strWhere <> '' and (@strWhere is not null)
	begin
		set @strWhere = ' where ' + @strWhere
	end
else
	begin
		set @strWhere = ''
	end

--------------总记录数---------------
set @strSqlCount='Select @TotalCout=count(*) from ' + @tbName + @strWhere


--------------分页------------

set @strSql='select * from (select ROW_NUMBER() OVER(order by ' + @strOrder + ') as rownum , ' + @tbFields + ' from ' + @tbName + @strWhere + ') T where T.rownum > ' + str(@StartRowIndex) + ' and T.rownum <= ' + str(@StartRowIndex+@MaximumRows)

exec sp_executesql @strSqlCount,N'@TotalCout int output',@TotalRows output
exec(@strSql)