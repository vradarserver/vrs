IF NOT EXISTS (
    SELECT 1
    FROM   [sys].[table_types] AS [tt]
    JOIN   [sys].[schemas]     AS [s]  ON [tt].[schema_id] = [s].[schema_id]
    WHERE  [s].[name] =  'BaseStation'
    AND    [tt].[name] = 'BaseStationAircraftUpsert'
)
BEGIN
    CREATE TYPE [BaseStation].[BaseStationAircraftUpsert] AS TABLE
    (
        [ModeS]             VARCHAR(6) NOT NULL PRIMARY KEY
       ,[FirstCreated]      DATETIME NOT NULL
       ,[LastModified]      DATETIME NOT NULL
       ,[ModeSCountry]      NVARCHAR(80)
       ,[Country]           NVARCHAR(80)
       ,[Registration]      NVARCHAR(20)
       ,[CurrentRegDate]    NVARCHAR(20)
       ,[PreviousID]        NVARCHAR(20)
       ,[FirstRegDate]      NVARCHAR(20)
       ,[Status]            NVARCHAR(10)
       ,[DeRegDate]         NVARCHAR(10)
       ,[Manufacturer]      NVARCHAR(80)
       ,[ICAOTypeCode]      NVARCHAR(10)
       ,[Type]              NVARCHAR(80)
       ,[SerialNo]          NVARCHAR(30)
       ,[PopularName]       NVARCHAR(80)
       ,[GenericName]       NVARCHAR(80)
       ,[AircraftClass]     NVARCHAR(80)
       ,[Engines]           NVARCHAR(40)
       ,[OwnershipStatus]   NVARCHAR(20)
       ,[RegisteredOwners]  NVARCHAR(100)
       ,[MTOW]              NVARCHAR(20)
       ,[TotalHours]        NVARCHAR(20)
       ,[YearBuilt]         NVARCHAR(4)
       ,[CofACategory]      NVARCHAR(30)
       ,[CofAExpiry]        NVARCHAR(20)
       ,[UserNotes]         NVARCHAR(300)
       ,[Interested]        BIT NOT NULL
       ,[UserTag]           NVARCHAR(80)
       ,[InfoURL]           NVARCHAR(150)
       ,[PictureURL1]       NVARCHAR(150)
       ,[PictureURL2]       NVARCHAR(150)
       ,[PictureURL3]       NVARCHAR(150)
       ,[UserBool1]         BIT NOT NULL
       ,[UserBool2]         BIT NOT NULL
       ,[UserBool3]         BIT NOT NULL
       ,[UserBool4]         BIT NOT NULL
       ,[UserBool5]         BIT NOT NULL
       ,[UserString1]       NVARCHAR(40)
       ,[UserString2]       NVARCHAR(40)
       ,[UserString3]       NVARCHAR(40)
       ,[UserString4]       NVARCHAR(40)
       ,[UserString5]       NVARCHAR(40)
       ,[UserInt1]          BIGINT
       ,[UserInt2]          BIGINT
       ,[UserInt3]          BIGINT
       ,[UserInt4]          BIGINT
       ,[UserInt5]          BIGINT
       ,[OperatorFlagCode]  NVARCHAR(20)
    );
END;
GO
