IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Update]
    @AircraftID       INT
   ,@FirstCreated     DATETIME2 = NULL
   ,@LastModified     DATETIME2
   ,@ModeS            NVARCHAR(20)
   ,@ModeSCountry     NVARCHAR(80)
   ,@Country          NVARCHAR(80)
   ,@Registration     NVARCHAR(80)
   ,@CurrentRegDate   NVARCHAR(80)
   ,@PreviousID       NVARCHAR(80)
   ,@FirstRegDate     NVARCHAR(80)
   ,@Status           NVARCHAR(80)
   ,@DeRegDate        NVARCHAR(80)
   ,@Manufacturer     NVARCHAR(80)
   ,@ICAOTypeCode     NVARCHAR(80)
   ,@Type             NVARCHAR(80)
   ,@SerialNo         NVARCHAR(80)
   ,@PopularName      NVARCHAR(80)
   ,@GenericName      NVARCHAR(80)
   ,@AircraftClass    NVARCHAR(80)
   ,@Engines          NVARCHAR(80)
   ,@OwnershipStatus  NVARCHAR(80)
   ,@RegisteredOwners NVARCHAR(100)
   ,@MTOW             NVARCHAR(80)
   ,@TotalHours       NVARCHAR(80)
   ,@YearBuilt        NVARCHAR(80)
   ,@CofACategory     NVARCHAR(80)
   ,@CofAExpiry       NVARCHAR(80)
   ,@UserNotes        NVARCHAR(300)
   ,@Interested       BIT
   ,@UserTag          NVARCHAR(300)
   ,@InfoURL          NVARCHAR(300)
   ,@PictureURL1      NVARCHAR(300)
   ,@PictureURL2      NVARCHAR(300)
   ,@PictureURL3      NVARCHAR(300)
   ,@UserBool1        BIT
   ,@UserBool2        BIT
   ,@UserBool3        BIT
   ,@UserBool4        BIT
   ,@UserBool5        BIT
   ,@UserString1      NVARCHAR(MAX)
   ,@UserString2      NVARCHAR(MAX)
   ,@UserString3      NVARCHAR(MAX)
   ,@UserString4      NVARCHAR(MAX)
   ,@UserString5      NVARCHAR(MAX)
   ,@UserInt1         BIGINT
   ,@UserInt2         BIGINT
   ,@UserInt3         BIGINT
   ,@UserInt4         BIGINT
   ,@UserInt5         BIGINT
   ,@OperatorFlagCode NVARCHAR(80)
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
