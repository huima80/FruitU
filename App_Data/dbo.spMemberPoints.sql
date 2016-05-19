CREATE PROCEDURE [dbo].[spMemberPoints]
	@OrderID varchar(50),
	@IncreasedMemberPoints int,
	@UsedMemberPoints int,
	@NewMemberPoints	int output,
	@AgentNewMemberPoints	int output
AS
BEGIN
    DECLARE @CurrentMemberPoints	int
    DECLARE @IsCalMemberPoints	bit
    DECLARE @OpenID	varchar(50), @AgentOpenID varchar(50)
	DECLARE @AgentCount int
    DECLARE @ErrorCode     int

	SET @AgentNewMemberPoints = -1
	SET @AgentCount = 0
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

	--查询此订单是否计算过积分标志、此订单的下单人和推荐人OpenID
	SELECT @IsCalMemberPoints = IsCalMemberPoints,
		   @OpenID = OpenID,
		   @AgentOpenID = AgentOpenID
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

		SET @NewMemberPoints = @CurrentMemberPoints + @IncreasedMemberPoints - @UsedMemberPoints

		--避免积分更新后小于0
		IF ( @NewMemberPoints < 0 )
		BEGIN
			SET @NewMemberPoints = 0
		END

		--更新下单人的会员积分
		UPDATE WeChatUsers
		SET MemberPoints = @NewMemberPoints
		WHERE OpenID = @OpenID

		IF( @@ERROR <> 0 )
		BEGIN
			SET @ErrorCode = -1
			GOTO Cleanup
		END

		--更新推荐人的会员积分，排除推荐人给自己推荐的情况
		IF ( @AgentOpenID <> '' and @AgentOpenID is not null and @AgentOpenID <> @OpenID )
		BEGIN
			--查询下单人是否被推荐过，同一个下单人只能被推荐一次
			SELECT @AgentCount = count(*)
			FROM ProductOrder
			WHERE OpenID = @OpenID AND AgentOpenID IS NOT NULL

			--下单人是第一次被推荐，才给推荐人积分
			IF ( @AgentCount = 1 )
			BEGIN
				UPDATE WeChatUsers
				SET MemberPoints = MemberPoints + 100
				WHERE OpenID = @AgentOpenID

				IF( @@ERROR <> 0 )
				BEGIN
					SET @ErrorCode = -1
					GOTO Cleanup
				END

				--查询推荐人的新会员积分
				SELECT @AgentNewMemberPoints = MemberPoints
				FROM WeChatUsers
				WHERE OpenID = @AgentOpenID
			END
		END

		--设置此订单已计算过积分标志，避免重复计算
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

		IF ( @AgentOpenID <> '' and @AgentOpenID is not null )
		BEGIN
			--查询推荐人的新会员积分
			SELECT @AgentNewMemberPoints = MemberPoints
			FROM WeChatUsers
			WHERE OpenID = @AgentOpenID

			IF ( @@rowcount = 0 )
			BEGIN
				SET @ErrorCode = 1
				GOTO Cleanup
			END
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