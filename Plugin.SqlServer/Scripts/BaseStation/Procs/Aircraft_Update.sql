IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Update]
    @AircraftID       BIGINT
   ,@FirstCreated     DATETIME = NULL
   ,@LastModified     DATETIME
   ,@ModeS            VARCHAR(6)
   ,@ModeSCountry     NVARCHAR(24)
   ,@Country          NVARCHAR(24)
   ,@Registration     NVARCHAR(20)
   ,@CurrentRegDate   NVARCHAR(10)
   ,@PreviousID       NVARCHAR(10)
   ,@FirstRegDate     NVARCHAR(10)
   ,@Status           NVARCHAR(10)
   ,@DeRegDate        NVARCHAR(10)
   ,@Manufacturer     NVARCHAR(60)
   ,@ICAOTypeCode     NVARCHAR(10)
   ,@Type             NVARCHAR(40)
   ,@SerialNo         NVARCHAR(30)
   ,@PopularName      NVARCHAR(20)
   ,@GenericName      NVARCHAR(20)
   ,@AircraftClass    NVARCHAR(20)
   ,@Engines          NVARCHAR(40)
   ,@OwnershipStatus  NVARCHAR(10)
   ,@RegisteredOwners NVARCHAR(100)
   ,@MTOW             NVARCHAR(10)
   ,@TotalHours       NVARCHAR(20)
   ,@YearBuilt        NVARCHAR(4)
   ,@CofACategory     NVARCHAR(30)
   ,@CofAExpiry       NVARCHAR(10)
   ,@UserNotes        NVARCHAR(300)
   ,@Interested       BIT
   ,@UserTag          NVARCHAR(5)
   ,@InfoURL          NVARCHAR(150)
   ,@PictureURL1      NVARCHAR(150)
   ,@PictureURL2      NVARCHAR(150)
   ,@PictureURL3      NVARCHAR(150)
   ,@UserBool1        BIT
   ,@UserBool2        BIT
   ,@UserBool3        BIT
   ,@UserBool4        BIT
   ,@UserBool5        BIT
   ,@UserString1      NVARCHAR(20)
   ,@UserString2      NVARCHAR(20)
   ,@UserString3      NVARCHAR(20)
   ,@UserString4      NVARCHAR(20)
   ,@UserString5      NVARCHAR(20)
   ,@UserInt1         BIGINT
   ,@UserInt2         BIGINT
   ,@UserInt3         BIGINT
   ,@UserInt4         BIGINT
   ,@UserInt5         BIGINT
   ,@OperatorFlagCode NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Aircraft]
    SET    [FirstCreated]        = ISNULL(@FirstCreated, [FirstCreated])
          ,[LastModified]        = @LastModified
          ,[ModeS]               = @ModeS
          ,[ModeSCountry]        = @ModeSCountry
          ,[Country]             = @Country
          ,[Registration]        = @Registration
          ,[CurrentRegDate]      = @CurrentRegDate
          ,[PreviousID]          = @PreviousID
          ,[FirstRegDate]        = @FirstRegDate
          ,[Status]              = @Status
          ,[DeRegDate]           = @DeRegDate
          ,[Manufacturer]        = @Manufacturer
          ,[ICAOTypeCode]        = @ICAOTypeCode
          ,[Type]                = @Type
          ,[SerialNo]            = @SerialNo
          ,[PopularName]         = @PopularName
          ,[GenericName]         = @GenericName
          ,[AircraftClass]       = @AircraftClass
          ,[Engines]             = @Engines
          ,[OwnershipStatus]     = @OwnershipStatus
          ,[RegisteredOwners]    = @RegisteredOwners
          ,[MTOW]                = @MTOW
          ,[TotalHours]          = @TotalHours
          ,[YearBuilt]           = @YearBuilt
          ,[CofACategory]        = @CofACategory
          ,[CofAExpiry]          = @CofAExpiry
          ,[UserNotes]           = @UserNotes
          ,[Interested]          = @Interested
          ,[UserTag]             = @UserTag
          ,[InfoURL]             = @InfoURL
          ,[PictureURL1]         = @PictureURL1
          ,[PictureURL2]         = @PictureURL2
          ,[PictureURL3]         = @PictureURL3
          ,[UserBool1]           = @UserBool1
          ,[UserBool2]           = @UserBool2
          ,[UserBool3]           = @UserBool3
          ,[UserBool4]           = @UserBool4
          ,[UserBool5]           = @UserBool5
          ,[UserString1]         = @UserString1
          ,[UserString2]         = @UserString2
          ,[UserString3]         = @UserString3
          ,[UserString4]         = @UserString4
          ,[UserString5]         = @UserString5
          ,[UserInt1]            = @UserInt1
          ,[UserInt2]            = @UserInt2
          ,[UserInt3]            = @UserInt3
          ,[UserInt4]            = @UserInt4
          ,[UserInt5]            = @UserInt5
          ,[OperatorFlagCode]    = @OperatorFlagCode
    WHERE [AircraftID] = @AircraftID;
END;
GO
