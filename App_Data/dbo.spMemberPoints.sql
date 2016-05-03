CREATE PROCEDURE [dbo].[spMemberPoints]
	@OrderID varchar(50),
	@UpdatedMemberPoints	int,
	@NewMemberPoints	int output
AS
BEGIN
    DECLARE @CurrentMemberPoints	int
    DECLARE @IsCalMemberPoints	bit
    DECLARE @OpenID	varchar(50)

    DECLARE @ErrorCode     int
    SET @ErrorCode = 0

    DECLARE @TranStarted   bit
    SET @TranStarted = 0

	--开始事务
    IF( @@TRANCOUNT = 0 )
    BEGIN
	    BEGIN TRANSACTION
	    SET @TranStarted = 1
    END
    ELSE
    	SET @TranStarted = 0

	--查询此订单是否计算过积分
	SELECT @IsCalMemberPoints = IsCalMemberPoints,
		   @OpenID = OpenID
	FROM ProductOrder
	WHERE OrderID = @OrderID

	IF ( @@rowcount = 0 )
	BEGIN
		SET @ErrorCode = 1
		GOTO Cleanup
	END
	
	--如果此订单未计算过积分，再进行计算，避免重复计算
	IF ( @IsCalMemberPoints = 0 )
	BEGIN
		--查询现有的会员积分
		SELECT @CurrentMemberPoints = MemberPoints
		FROM WeChatUsers
		WHERE OpenID = @OpenID

		SET @NewMemberPoints = @CurrentMemberPoints + @UpdatedMemberPoints

		--避免积分更新后小于0
		IF ( @NewMemberPoints < 0 )
		BEGIN
			SET @NewMemberPoints = 0
		END

		--更新此用户的会员积分
		UPDATE WeChatUsers
		SET MemberPoints = @NewMemberPoints
		WHERE OpenID = @OpenID

		IF( @@ERROR <> 0 )
		BEGIN
			SET @ErrorCode = -1
			GOTO Cleanup
		END

		--设置此订单已计算过积分，避免重复计算
		UPDATE ProductOrder
		SET IsCalMemberPoints = 1
		WHERE OrderID = @OrderID

		IF( @@ERROR <> 0 )
		BEGIN
			SET @ErrorCode = -1
			GOTO Cleanup
		END
	END
	ELSE
	BEGIN
		--获取现有的会员积分
		SELECT @NewMemberPoints = MemberPoints
		FROM WeChatUsers
		WHERE OpenID = @OpenID

		IF ( @@rowcount = 0 )
		BEGIN
			SET @ErrorCode = 1
			GOTO Cleanup
		END
	END


	IF( @TranStarted = 1 )
    BEGIN
		SET @TranStarted = 0
		COMMIT TRANSACTION
    END

    RETURN @ErrorCode

Cleanup:

    IF( @TranStarted = 1 )
    BEGIN
        SET @TranStarted = 0
    	ROLLBACK TRANSACTION
    END

    RETURN @ErrorCode

END