CREATE proc [dbo].[spSqlPageByMaxTop]
@tbName varchar(255),        --表名
@PK varchar(50),			--主键字段
@tbFields varchar(1000),      --返回字段
@PageSize int,                --页尺寸
@PageIndex int,                --页码
@strWhere varchar(1000),    --查询条件
@strOrder varchar(1000),    --排序条件
@TotalRows int output            --返回总记录数
as
declare @strSql nvarchar(max)    --主语句
declare @strSqlCount nvarchar(max)--查询记录总数主语句
declare @PrevIndex int

--------------总记录数---------------
if @strWhere !=''
	begin
		set @strSqlCount='Select @TotalCout=count(*) from  ' + @tbName + ' where '+ @strWhere
	end
else
	begin
		set @strSqlCount='Select @TotalCout=count(*) from  ' + @tbName
	end

--------------排序字段---------------
if @strOrder != ''
	begin
		set @strOrder = @strOrder + ',' + @PK + ' desc'
	end
else
	begin
		set @strOrder = @PK + ' desc'
	end

--------------分页------------
if @PageIndex < 2	--查询第一页
	begin
		if @strWhere != ''
			begin
				set @strSql='select top ' + str(@PageSize) + ' ' + @tbFields + ' from ' + @tbName + ' where '+ @strWhere + ' order by ' + @strOrder
			end
		else
			begin
				set @strSql='select top ' + str(@PageSize) + ' ' + @tbFields + ' from ' + @tbName + ' order by ' + @strOrder
			end
	end
else	--查询非第一页
	begin
		set @PrevIndex = @PageIndex - 1

		if @strWhere != ''
			begin
				set @strSql='select top ' + str(@PageSize) + ' ' + @tbFields + ' from ' + @tbName + ' where ' + @PK + '<(select min(ID) from (select top '+str(@PrevIndex*@PageSize)+' ' + @PK + ' as ID from ' + @tbName + ' where '+ @strWhere + ' order by ' + @strOrder + ')a) and ' + @strWhere + ' order by ' + @strOrder
			end
		else
			begin
				set @strSql='select top ' + str(@PageSize) + ' ' + @tbFields + ' from ' + @tbName + ' where ' + @PK + '<(select min(ID) from (select top '+str(@PrevIndex*@PageSize)+' ' + @PK + ' as ID from ' + @tbName+' order by ' + @strOrder +')a) order by ' + @strOrder
			end
	end



exec sp_executesql @strSqlCount,N'@TotalCout int output',@TotalRows output
exec(@strSql)