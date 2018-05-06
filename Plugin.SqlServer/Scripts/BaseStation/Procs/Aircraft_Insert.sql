IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Insert]
    @FirstCreated     DATETIME2
   ,@LastModified     DATETIME2
   ,@ModeS            NVARCHAR(20)
   ,@ModeSCountry     NVARCHAR(80) = NULL
   ,@Country          NVARCHAR(80) = NULL
   ,@Registration     NVARCHAR(80) = NULL
   ,@CurrentRegDate   NVARCHAR(80) = NULL
   ,@PreviousID       NVARCHAR(80) = NULL
   ,@FirstRegDate     NVARCHAR(80) = NULL
   ,@Status           NVARCHAR(80) = NULL
   ,@DeRegDate        NVARCHAR(80) = NULL
   ,@Manufacturer     NVARCHAR(80) = NULL
   ,@ICAOTypeCode     NVARCHAR(80) = NULL
   ,@Type             NVARCHAR(80) = NULL
   ,@SerialNo         NVARCHAR(80) = NULL
   ,@PopularName      NVARCHAR(80) = NULL
   ,@GenericName      NVARCHAR(80) = NULL
   ,@AircraftClass    NVARCHAR(80) = NULL
   ,@Engines          NVARCHAR(80) = NULL
   ,@OwnershipStatus  NVARCHAR(80) = NULL
   ,@RegisteredOwners NVARCHAR(100) = NULL
   ,@MTOW             NVARCHAR(80) = NULL
   ,@TotalHours       NVARCHAR(80) = NULL
   ,@YearBuilt        NVARCHAR(80) = NULL
   ,@CofACategory     NVARCHAR(80) = NULL
   ,@CofAExpiry       NVARCHAR(80) = NULL
   ,@UserNotes        NVARCHAR(300) = NULL
   ,@Interested       BIT = 0
   ,@UserTag          NVARCHAR(300) = NULL
   ,@InfoURL          NVARCHAR(300) = NULL
   ,@PictureURL1      NVARCHAR(300) = NULL
   ,@PictureURL2      NVARCHAR(300) = NULL
   ,@PictureURL3      NVARCHAR(300) = NULL
   ,@UserBool1        BIT = 0
   ,@UserBool2        BIT = 0
   ,@UserBool3        BIT = 0
   ,@UserBool4        BIT = 0
   ,@UserBool5        BIT = 0
   ,@UserString1      NVARCHAR(MAX) = NULL
   ,@UserString2      NVARCHAR(MAX) = NULL
   ,@UserString3      NVARCHAR(MAX) = NULL
   ,@UserString4      NVARCHAR(MAX) = NULL
   ,@UserString5      NVARCHAR(MAX) = NULL
   ,@UserInt1         BIGINT = 0
   ,@UserInt2         BIGINT = 0
   ,@UserInt3         BIGINT = 0
   ,@UserInt4         BIGINT = 0
   ,@UserInt5         BIGINT = 0
   ,@OperatorFlagCode NVARCHAR(80) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[Aircraft] (
         [FirstCreated]
        ,[LastModified]
        ,[ModeS]
        ,[ModeSCountry]
        ,[Country]
        ,[Registration]
        ,[CurrentRegDate]
        ,[PreviousID]
        ,[FirstRegDate]
        ,[Status]
        ,[DeRegDate]
        ,[Manufacturer]
        ,[ICAOTypeCode]
        ,[Type]
        ,[SerialNo]
        ,[PopularName]
        ,[GenericName]
        ,[AircraftClass]
        ,[Engines]
        ,[OwnershipStatus]
        ,[RegisteredOwners]
        ,[MTOW]
        ,[TotalHours]
        ,[YearBuilt]
        ,[CofACategory]
        ,[CofAExpiry]
        ,[UserNotes]
        ,[Interested]
        ,[UserTag]
        ,[InfoURL]
        ,[PictureURL1]
        ,[PictureURL2]
        ,[PictureURL3]
        ,[UserBool1]
        ,[UserBool2]
        ,[UserBool3]
        ,[UserBool4]
        ,[UserBool5]
        ,[UserString1]
        ,[UserString2]
        ,[UserString3]
        ,[UserString4]
        ,[UserString5]
        ,[UserInt1]
        ,[UserInt2]
        ,[UserInt3]
        ,[UserInt4]
        ,[UserInt5]
        ,[OperatorFlagCode]
    ) VALUES (
         @FirstCreated
        ,@LastModified
        ,@ModeS
        ,@ModeSCountry
        ,@Country
        ,@Registration
        ,@CurrentRegDate
        ,@PreviousID
        ,@FirstRegDate
        ,@Status
        ,@DeRegDate
        ,@Manufacturer
        ,@ICAOTypeCode
        ,@Type
        ,@SerialNo
        ,@PopularName
        ,@GenericName
        ,@AircraftClass
        ,@Engines
        ,@OwnershipStatus
        ,@RegisteredOwners
        ,@MTOW
        ,@TotalHours
        ,@YearBuilt
        ,@CofACategory
        ,@CofAExpiry
        ,@UserNotes
        ,@Interested
        ,@UserTag
        ,@InfoURL
        ,@PictureURL1
        ,@PictureURL2
        ,@PictureURL3
        ,@UserBool1
        ,@UserBool2
        ,@UserBool3
        ,@UserBool4
        ,@UserBool5
        ,@UserString1
        ,@UserString2
        ,@UserString3
        ,@UserString4
        ,@UserString5
        ,@UserInt1
        ,@UserInt2
        ,@UserInt3
        ,@UserInt4
        ,@UserInt5
        ,@OperatorFlagCode
    );

    SELECT SCOPE_IDENTITY() AS [AircraftID];
END;
GO
