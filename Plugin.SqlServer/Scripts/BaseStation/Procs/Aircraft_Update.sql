IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Update]
    @AircraftID       INT
   ,@FirstCreated     DATETIME2 = NULL
   ,@LastModified     DATETIME2
   ,@ModeS            NVARCHAR(6)
   ,@ModeSCountry     NVARCHAR(80)
   ,@Country          NVARCHAR(80)
   ,@Registration     NVARCHAR(20)
   ,@CurrentRegDate   NVARCHAR(20)
   ,@PreviousID       NVARCHAR(20)
   ,@FirstRegDate     NVARCHAR(20)
   ,@Status           NVARCHAR(10)
   ,@DeRegDate        NVARCHAR(10)
   ,@Manufacturer     NVARCHAR(80)
   ,@ICAOTypeCode     NVARCHAR(10)
   ,@Type             NVARCHAR(80)
   ,@SerialNo         NVARCHAR(30)
   ,@PopularName      NVARCHAR(80)
   ,@GenericName      NVARCHAR(80)
   ,@AircraftClass    NVARCHAR(80)
   ,@Engines          NVARCHAR(40)
   ,@OwnershipStatus  NVARCHAR(20)
   ,@RegisteredOwners NVARCHAR(100)
   ,@MTOW             NVARCHAR(20)
   ,@TotalHours       NVARCHAR(20)
   ,@YearBuilt        NVARCHAR(4)
   ,@CofACategory     NVARCHAR(30)
   ,@CofAExpiry       NVARCHAR(20)
   ,@UserNotes        NVARCHAR(300)
   ,@Interested       BIT
   ,@UserTag          NVARCHAR(80)
   ,@InfoURL          NVARCHAR(150)
   ,@PictureURL1      NVARCHAR(150)
   ,@PictureURL2      NVARCHAR(150)
   ,@PictureURL3      NVARCHAR(150)
   ,@UserBool1        BIT
   ,@UserBool2        BIT
   ,@UserBool3        BIT
   ,@UserBool4        BIT
   ,@UserBool5        BIT
   ,@UserString1      NVARCHAR(40)
   ,@UserString2      NVARCHAR(40)
   ,@UserString3      NVARCHAR(40)
   ,@UserString4      NVARCHAR(40)
   ,@UserString5      NVARCHAR(40)
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
