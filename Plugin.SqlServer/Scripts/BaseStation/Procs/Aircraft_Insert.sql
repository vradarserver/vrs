IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Insert]
    @FirstCreated     DATETIME
   ,@LastModified     DATETIME
   ,@ModeS            VARCHAR(6)
   ,@ModeSCountry     NVARCHAR(80) = NULL
   ,@Country          NVARCHAR(80) = NULL
   ,@Registration     NVARCHAR(20) = NULL
   ,@CurrentRegDate   NVARCHAR(20) = NULL
   ,@PreviousID       NVARCHAR(20) = NULL
   ,@FirstRegDate     NVARCHAR(20) = NULL
   ,@Status           NVARCHAR(10) = NULL
   ,@DeRegDate        NVARCHAR(10) = NULL
   ,@Manufacturer     NVARCHAR(80) = NULL
   ,@ICAOTypeCode     NVARCHAR(10) = NULL
   ,@Type             NVARCHAR(80) = NULL
   ,@SerialNo         NVARCHAR(30) = NULL
   ,@PopularName      NVARCHAR(80) = NULL
   ,@GenericName      NVARCHAR(80) = NULL
   ,@AircraftClass    NVARCHAR(80) = NULL
   ,@Engines          NVARCHAR(40) = NULL
   ,@OwnershipStatus  NVARCHAR(20) = NULL
   ,@RegisteredOwners NVARCHAR(100) = NULL
   ,@MTOW             NVARCHAR(20) = NULL
   ,@TotalHours       NVARCHAR(20) = NULL
   ,@YearBuilt        NVARCHAR(4) = NULL
   ,@CofACategory     NVARCHAR(30) = NULL
   ,@CofAExpiry       NVARCHAR(20) = NULL
   ,@UserNotes        NVARCHAR(300) = NULL
   ,@Interested       BIT = 0
   ,@UserTag          NVARCHAR(80) = NULL
   ,@InfoURL          NVARCHAR(150) = NULL
   ,@PictureURL1      NVARCHAR(150) = NULL
   ,@PictureURL2      NVARCHAR(150) = NULL
   ,@PictureURL3      NVARCHAR(150) = NULL
   ,@UserBool1        BIT = 0
   ,@UserBool2        BIT = 0
   ,@UserBool3        BIT = 0
   ,@UserBool4        BIT = 0
   ,@UserBool5        BIT = 0
   ,@UserString1      NVARCHAR(40) = NULL
   ,@UserString2      NVARCHAR(40) = NULL
   ,@UserString3      NVARCHAR(40) = NULL
   ,@UserString4      NVARCHAR(40) = NULL
   ,@UserString5      NVARCHAR(40) = NULL
   ,@UserInt1         BIGINT = 0
   ,@UserInt2         BIGINT = 0
   ,@UserInt3         BIGINT = 0
   ,@UserInt4         BIGINT = 0
   ,@UserInt5         BIGINT = 0
   ,@OperatorFlagCode NVARCHAR(20) = NULL
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
