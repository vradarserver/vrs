PRINT 'Running schema upgrade on ' + @@SERVERNAME + ' in ' + DB_NAME() + ' as user ' + SUSER_NAME();
PRINT '';
GO

-------------------------------------------------------------------------------
-- VRS/Schema/VRS.sql
-------------------------------------------------------------------------------
PRINT 'VRS/Schema/VRS.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'VRS')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE SCHEMA [VRS]';
END;
GO




-------------------------------------------------------------------------------
-- VRS/Types/Icao24.sql
-------------------------------------------------------------------------------
PRINT 'VRS/Types/Icao24.sql';
GO

IF NOT EXISTS (
    SELECT 1
    FROM   [sys].[table_types] AS [tt]
    JOIN   [sys].[schemas]     AS [s]  ON [tt].[schema_id] = [s].[schema_id]
    WHERE  [s].[name] =  'VRS'
    AND    [tt].[name] = 'Icao24'
)
BEGIN
    CREATE TYPE [VRS].[Icao24] AS TABLE
    (
        [ModeS] VARCHAR(6) NOT NULL PRIMARY KEY
    );
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Schema/BaseStation.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Schema/BaseStation.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'BaseStation')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE SCHEMA [BaseStation]';
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Tables/Aircraft.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Tables/Aircraft.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Aircraft')
BEGIN
    CREATE TABLE [BaseStation].[Aircraft]
    (
        [AircraftID]        BIGINT IDENTITY
       ,[FirstCreated]      DATETIME NOT NULL
       ,[LastModified]      DATETIME NOT NULL
       ,[ModeS]             VARCHAR(6) NOT NULL
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
       ,[Interested]        BIT NOT NULL CONSTRAINT [DF_Aircraft_Interested] DEFAULT 0
       ,[UserTag]           NVARCHAR(80)
       ,[InfoURL]           NVARCHAR(150)
       ,[PictureURL1]       NVARCHAR(150)
       ,[PictureURL2]       NVARCHAR(150)
       ,[PictureURL3]       NVARCHAR(150)
       ,[UserBool1]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool1] DEFAULT 0
       ,[UserBool2]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool2] DEFAULT 0
       ,[UserBool3]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool3] DEFAULT 0
       ,[UserBool4]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool4] DEFAULT 0
       ,[UserBool5]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool5] DEFAULT 0
       ,[UserString1]       NVARCHAR(40)
       ,[UserString2]       NVARCHAR(40)
       ,[UserString3]       NVARCHAR(40)
       ,[UserString4]       NVARCHAR(40)
       ,[UserString5]       NVARCHAR(40)
       ,[UserInt1]          BIGINT CONSTRAINT [DF_Aircraft_UserInt1] DEFAULT 0
       ,[UserInt2]          BIGINT CONSTRAINT [DF_Aircraft_UserInt2] DEFAULT 0
       ,[UserInt3]          BIGINT CONSTRAINT [DF_Aircraft_UserInt3] DEFAULT 0
       ,[UserInt4]          BIGINT CONSTRAINT [DF_Aircraft_UserInt4] DEFAULT 0
       ,[UserInt5]          BIGINT CONSTRAINT [DF_Aircraft_UserInt5] DEFAULT 0
       ,[OperatorFlagCode]  NVARCHAR(20)

       ,CONSTRAINT [PK_Aircraft] PRIMARY KEY ([AircraftID])
    );

    CREATE UNIQUE INDEX [IX_Aircraft_ModeS]     ON [BaseStation].[Aircraft] ([ModeS]);

    CREATE INDEX [IX_Aircraft_AircraftClass]    ON [BaseStation].[Aircraft] ([AircraftClass]);
    CREATE INDEX [IX_Aircraft_Country]          ON [BaseStation].[Aircraft] ([Country]);
    CREATE INDEX [IX_Aircraft_GenericName]      ON [BaseStation].[Aircraft] ([GenericName]);
    CREATE INDEX [IX_Aircraft_ICAOTypeCode]     ON [BaseStation].[Aircraft] ([ICAOTypeCode]);
    CREATE INDEX [IX_Aircraft_Interested]       ON [BaseStation].[Aircraft] ([Interested]);
    CREATE INDEX [IX_Aircraft_Manufacturer]     ON [BaseStation].[Aircraft] ([Manufacturer]);
    CREATE INDEX [IX_Aircraft_ModeSCountry]     ON [BaseStation].[Aircraft] ([ModeSCountry]);
    CREATE INDEX [IX_Aircraft_PopularName]      ON [BaseStation].[Aircraft] ([PopularName]);
    CREATE INDEX [IX_Aircraft_RegisteredOwners] ON [BaseStation].[Aircraft] ([RegisteredOwners]);
    CREATE INDEX [IX_Aircraft_Registration]     ON [BaseStation].[Aircraft] ([Registration]);
    CREATE INDEX [IX_Aircraft_SerialNo]         ON [BaseStation].[Aircraft] ([SerialNo]);
    CREATE INDEX [IX_Aircraft_Type]             ON [BaseStation].[Aircraft] ([Type]);
    CREATE INDEX [IX_Aircraft_UserTag]          ON [BaseStation].[Aircraft] ([UserTag]);
    CREATE INDEX [IX_Aircraft_YearBuilt]        ON [BaseStation].[Aircraft] ([YearBuilt]);
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Tables/DBHistory.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Tables/DBHistory.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'DBHistory')
BEGIN
    CREATE TABLE [BaseStation].[DBHistory]
    (
        [DBHistoryID]   BIGINT IDENTITY
       ,[TimeStamp]     DATETIME NOT NULL
       ,[Description]   NVARCHAR(100) NOT NULL

       ,CONSTRAINT [PK_DBHistory] PRIMARY KEY ([DBHistoryID])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[DBHistory])
BEGIN
    INSERT INTO [BaseStation].[DBHistory] (
        [TimeStamp]
       ,[Description]
    ) VALUES (
        GETUTCDATE()
       ,'Schema created by ' + SUSER_NAME()
    );

    -- This one is required by BaseStationDBHistory.IsCreationOfDatabaseByVirtualRadarServer
    INSERT INTO [BaseStation].[DBHistory] (
        [TimeStamp]
       ,[Description]
    ) VALUES (
        GETUTCDATE()
       ,'Database autocreated by Virtual Radar Server'
    );
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Tables/DBInfo.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Tables/DBInfo.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'DBInfo')
BEGIN
    CREATE TABLE [BaseStation].[DBInfo]
    (
        [OriginalVersion]   SMALLINT NOT NULL
       ,[CurrentVersion]    SMALLINT NOT NULL

       ,CONSTRAINT [PK_DBInfo] PRIMARY KEY ([OriginalVersion], [CurrentVersion])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[DBInfo])
BEGIN
    INSERT INTO [BaseStation].[DBInfo] (
        [OriginalVersion]
       ,[CurrentVersion]
    ) VALUES (
        2
       ,2
    );
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Tables/Locations.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Tables/Locations.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Locations')
BEGIN
    CREATE TABLE [BaseStation].[Locations]
    (
        [LocationID]    BIGINT IDENTITY
       ,[LocationName]  NVARCHAR(80) NOT NULL
       ,[Latitude]      REAL NOT NULL
       ,[Longitude]     REAL NOT NULL
       ,[Altitude]      REAL NOT NULL

       ,CONSTRAINT [PK_Locations] PRIMARY KEY ([LocationID])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[Locations])
BEGIN
    INSERT INTO [BaseStation].[Locations] (
        [LocationName]
       ,[Latitude]
       ,[Longitude]
       ,[Altitude]
    ) VALUES (
        'Home'
       ,51.4
       ,-0.6
       ,25.0
    );
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Tables/Sessions.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Tables/Sessions.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Sessions')
BEGIN
    CREATE TABLE [BaseStation].[Sessions]
    (
        [SessionID]     BIGINT IDENTITY
       ,[LocationID]    BIGINT NOT NULL CONSTRAINT [FK_Sessions_Location] FOREIGN KEY REFERENCES [BaseStation].[Locations] ([LocationID])
       ,[StartTime]     DATETIME NOT NULL
       ,[EndTime]       DATETIME NULL

       ,CONSTRAINT [PK_Sessions] PRIMARY KEY ([SessionID])
    );

    CREATE INDEX [IX_Sessions_EndTime]      ON [BaseStation].[Sessions]([EndTime]);
    CREATE INDEX [IX_Sessions_LocationID]   ON [BaseStation].[Sessions]([LocationID]);
    CREATE INDEX [IX_Sessions_StartTime]    ON [BaseStation].[Sessions]([StartTime]);
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Tables/Flights.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Tables/Flights.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Flights')
BEGIN
    CREATE TABLE [BaseStation].[Flights]
    (
        [FlightID]              BIGINT IDENTITY
       ,[SessionID]             BIGINT NOT NULL CONSTRAINT [FK_Flights_Session]  FOREIGN KEY REFERENCES [BaseStation].[Sessions] ([SessionID]) ON DELETE CASCADE
       ,[AircraftID]            BIGINT NOT NULL CONSTRAINT [FK_Flights_Aircraft] FOREIGN KEY REFERENCES [BaseStation].[Aircraft] ([AircraftID]) ON DELETE CASCADE
       ,[StartTime]             DATETIME NOT NULL
       ,[EndTime]               DATETIME
       ,[Callsign]              NVARCHAR(20)
       ,[NumPosMsgRec]          INTEGER
       ,[NumADSBMsgRec]         INTEGER
       ,[NumModeSMsgRec]        INTEGER
       ,[NumIDMsgRec]           INTEGER
       ,[NumSurPosMsgRec]       INTEGER
       ,[NumAirPosMsgRec]       INTEGER
       ,[NumAirVelMsgRec]       INTEGER
       ,[NumSurAltMsgRec]       INTEGER
       ,[NumSurIDMsgRec]        INTEGER
       ,[NumAirToAirMsgRec]     INTEGER
       ,[NumAirCallRepMsgRec]   INTEGER
       ,[FirstIsOnGround]       BIT NOT NULL CONSTRAINT [DF_Flights_FirstIsOnGround] DEFAULT 0
       ,[LastIsOnGround]        BIT NOT NULL CONSTRAINT [DF_Flights_LastIsOnGround] DEFAULT 0
       ,[FirstLat]              REAL
       ,[LastLat]               REAL
       ,[FirstLon]              REAL
       ,[LastLon]               REAL
       ,[FirstGroundSpeed]      REAL
       ,[LastGroundSpeed]       REAL
       ,[FirstAltitude]         INTEGER
       ,[LastAltitude]          INTEGER
       ,[FirstVerticalRate]     INTEGER
       ,[LastVerticalRate]      INTEGER
       ,[FirstTrack]            REAL
       ,[LastTrack]             REAL
       ,[FirstSquawk]           INTEGER
       ,[LastSquawk]            INTEGER
       ,[HadAlert]              BIT NOT NULL CONSTRAINT [DF_Flights_HadAlert] DEFAULT 0
       ,[HadEmergency]          BIT NOT NULL CONSTRAINT [DF_Flights_HadEmergency] DEFAULT 0
       ,[HadSPI]                BIT NOT NULL CONSTRAINT [DF_Flights_HadSPI] DEFAULT 0
       ,[UserNotes]             NVARCHAR(300)

       ,CONSTRAINT [PK_Flights] PRIMARY KEY ([FlightID])
    );

    CREATE INDEX [IX_Flights_AircraftID]    ON [BaseStation].[Flights] ([AircraftID]);
    CREATE INDEX [IX_Flights_Callsign]      ON [BaseStation].[Flights] ([Callsign]);
    CREATE INDEX [IX_Flights_EndTime]       ON [BaseStation].[Flights] ([EndTime]);
    CREATE INDEX [IX_Flights_SessionID]     ON [BaseStation].[Flights] ([SessionID]);
    CREATE INDEX [IX_Flights_StartTime]     ON [BaseStation].[Flights] ([StartTime]);
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Tables/SystemEvents.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Tables/SystemEvents.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'SystemEvents')
BEGIN
    CREATE TABLE [BaseStation].[SystemEvents]
    (
        [SystemEventsID]    BIGINT IDENTITY
       ,[TimeStamp]         DATETIME NOT NULL
       ,[App]               NVARCHAR(15) NOT NULL
       ,[Msg]               NVARCHAR(100) NOT NULL

       ,CONSTRAINT [PK_SystemEvents] PRIMARY KEY ([SystemEventsID])
    );

    CREATE INDEX [IX_SystemEvents_App]          ON [BaseStation].[SystemEvents] ([App]);
    CREATE INDEX [IX_SystemEvents_TimeStamp]    ON [BaseStation].[SystemEvents] ([TimeStamp]);
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Types/BaseStationAircraftUpsertLookup.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Types/BaseStationAircraftUpsertLookup.sql';
GO

IF NOT EXISTS (
    SELECT 1
    FROM   [sys].[table_types] AS [tt]
    JOIN   [sys].[schemas]     AS [s]  ON [tt].[schema_id] = [s].[schema_id]
    WHERE  [s].[name] =  'BaseStation'
    AND    [tt].[name] = 'BaseStationAircraftUpsertLookup'
)
BEGIN
    CREATE TYPE [BaseStation].[BaseStationAircraftUpsertLookup] AS TABLE
    (
        [ModeS]             VARCHAR(6) NOT NULL PRIMARY KEY
       ,[LastModified]      DATETIME NOT NULL
       ,[Registration]      NVARCHAR(20)
       ,[Country]           NVARCHAR(80)
       ,[ModeSCountry]      NVARCHAR(80)
       ,[Manufacturer]      NVARCHAR(80)
       ,[Type]              NVARCHAR(80)
       ,[ICAOTypeCode]      NVARCHAR(10)
       ,[RegisteredOwners]  NVARCHAR(100)
       ,[OperatorFlagCode]  NVARCHAR(20)
       ,[SerialNo]          NVARCHAR(30)
       ,[YearBuilt]         NVARCHAR(4)
    );
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_Delete.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_Delete.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Delete]
    @AircraftID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Aircraft]
    WHERE  [AircraftID] = @AircraftID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_GetAll.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_GetAll.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Aircraft];
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_GetByCodes.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_GetByCodes.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetByCodes')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetByCodes] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetByCodes]
    @Codes AS [VRS].[Icao24] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT    [aircraft].*
    FROM      @Codes                   AS [code]
    JOIN      [BaseStation].[Aircraft] AS [aircraft] ON [code].[ModeS] = [aircraft].[ModeS];
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_GetByID.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_GetByID.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetByID')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetByID] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetByID]
    @AircraftID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Aircraft]
    WHERE  [AircraftID] = @AircraftID;
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_GetByModeS.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_GetByModeS.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetByModeS')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetByModeS] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetByModeS]
    @ModeS VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Aircraft]
    WHERE  [ModeS] = @ModeS;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_GetByRegistration.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_GetByRegistration.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetByRegistration')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetByRegistration] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetByRegistration]
    @Registration NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Aircraft]
    WHERE  [Registration] = @Registration;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_GetOrCreate.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_GetOrCreate.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetOrCreate')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetOrCreate] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetOrCreate]
    @Created        BIT OUTPUT
   ,@ModeS          VARCHAR(6)
   ,@LocalNow       DATETIME = NULL
   ,@ModeSCountry   NVARCHAR(24) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SET @LocalNow = ISNULL(@LocalNow, GETDATE());

    INSERT INTO [BaseStation].[Aircraft] (
        [ModeS]
       ,[FirstCreated]
       ,[LastModified]
       ,[ModeSCountry]
    )
    SELECT @ModeS
          ,@LocalNow
          ,@LocalNow
          ,@ModeSCountry
    WHERE NOT EXISTS (
        SELECT 1
        FROM   [BaseStation].[Aircraft]
        WHERE  [ModeS] = @ModeS
    );
    SET @Created = CASE WHEN @@ROWCOUNT = 0 THEN 0 ELSE 1 END;

    SELECT *
    FROM   [BaseStation].[Aircraft]
    WHERE  [ModeS] = @ModeS;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_Insert.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_Insert.sql';
GO

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




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_MarkManyMissing.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_MarkManyMissing.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_MarkManyMissing')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_MarkManyMissing] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_MarkManyMissing]
    @Codes    AS [VRS].[Icao24] READONLY
   ,@LocalNow AS DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @missingMarker AS NVARCHAR(20) = 'Missing';

    -- Insert stubs for aircraft that don't exist
    -- All aircraft inserted here will later be updated, which is redundant... but at the same
    -- time if we did the update first we could miss out an aircraft that was inserted between
    -- the update and the later insert. This should guarantee that no aircraft will be missed.
    INSERT INTO [BaseStation].[Aircraft] (
        [ModeS]
       ,[FirstCreated]
       ,[LastModified]
       ,[UserString1]
    )
    SELECT    [code].[ModeS]
             ,@LocalNow
             ,@LocalNow
             ,@missingMarker
    FROM      @Codes                   AS [code]
    LEFT JOIN [BaseStation].[Aircraft] AS [aircraft] ON [code].[ModeS] = [aircraft].[ModeS]
    WHERE     [aircraft].[AircraftID] IS NULL;

    -- Update existing aircraft but only if they have no details
    UPDATE [aircraft]
    SET    [LastModified] = @LocalNow
          ,[UserString1] = @missingMarker
    FROM   [BaseStation].[Aircraft] AS [aircraft]
    JOIN   @Codes                   AS [code]     ON [aircraft].[ModeS] = [code].[ModeS]
    WHERE  ISNULL([aircraft].[Registration], '') = ''
    AND    ISNULL([aircraft].[Manufacturer], '') = ''
    AND    ISNULL([aircraft].[Type], '') = ''
    AND    ISNULL([aircraft].[RegisteredOwners], '') = '';
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_Update.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_Update.sql';
GO

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




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_UpdateModeSCountry.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_UpdateModeSCountry.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_UpdateModeSCountry')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_UpdateModeSCountry] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_UpdateModeSCountry]
    @AircraftID   BIGINT
   ,@LastModified DATETIME
   ,@ModeSCountry NVARCHAR(80)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Aircraft]
    SET    [LastModified] = @LastModified
          ,[ModeSCountry] = @ModeSCountry
    WHERE [AircraftID] = @AircraftID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Aircraft_UpsertLookups.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Aircraft_UpsertLookups.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_UpsertLookups')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_UpsertLookups] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_UpsertLookups]
    @Lookups [BaseStation].[BaseStationAircraftUpsertLookup] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @action AS TABLE (
        [AircraftID] BIGINT PRIMARY KEY
       ,[Action]     VARCHAR(7)             -- 'Created' or 'Updated'
    );

    INSERT INTO [BaseStation].[Aircraft] (
        [ModeS]
       ,[FirstCreated]
       ,[LastModified]
       ,[Registration]
       ,[Country]
       ,[ModeSCountry]
       ,[Manufacturer]
       ,[Type]
       ,[ICAOTypeCode]
       ,[RegisteredOwners]
       ,[OperatorFlagCode]
       ,[SerialNo]
       ,[YearBuilt]
    )
    OUTPUT    INSERTED.[AircraftID]
             ,'Created'
    INTO      @action
    SELECT    [lookup].[ModeS]
             ,[lookup].[LastModified]
             ,[lookup].[LastModified]
             ,[lookup].[Registration]
             ,[lookup].[Country]
             ,[lookup].[ModeSCountry]
             ,[lookup].[Manufacturer]
             ,[lookup].[Type]
             ,[lookup].[ICAOTypeCode]
             ,[lookup].[RegisteredOwners]
             ,[lookup].[OperatorFlagCode]
             ,[lookup].[SerialNo]
             ,[lookup].[YearBuilt]
    FROM      @Lookups                 AS [lookup]
    LEFT JOIN [BaseStation].[Aircraft] AS [aircraft] ON [lookup].[ModeS] = [aircraft].[ModeS]
    WHERE     [aircraft].[AircraftID] IS NULL;

    UPDATE [aircraft]
    SET    [LastModified] =     [lookup].[LastModified]
          ,[Registration] =     [lookup].[Registration]
          ,[Country] =          [lookup].[Country]
          ,[ModeSCountry] =     [lookup].[ModeSCountry]
          ,[Manufacturer] =     [lookup].[Manufacturer]
          ,[Type] =             [lookup].[Type]
          ,[ICAOTypeCode] =     [lookup].[ICAOTypeCode]
          ,[RegisteredOwners] = [lookup].[RegisteredOwners]
          ,[OperatorFlagCode] = [lookup].[OperatorFlagCode]
          ,[SerialNo] =         [lookup].[SerialNo]
          ,[YearBuilt] =        [lookup].[YearBuilt]
          ,[UserString1] =      CASE WHEN [aircraft].[UserString1] = 'Missing' THEN NULL ELSE [aircraft].[UserString1] END
    OUTPUT INSERTED.[AircraftID]
          ,'Updated'
    INTO   @action
    FROM   @Lookups                 AS [lookup]
    JOIN   [BaseStation].[Aircraft] AS [aircraft] ON [lookup].[ModeS] = [aircraft].[ModeS]
    WHERE  [aircraft].[LastModified] <> [lookup].[LastModified]
    OR     ISNULL([aircraft].[Registration], '') <>     ISNULL([lookup].[Registration], '')
    OR     ISNULL([aircraft].[Country], '') <>          ISNULL([lookup].[Country], '')
    OR     ISNULL([aircraft].[ModeSCountry], '') <>     ISNULL([lookup].[ModeSCountry], '')
    OR     ISNULL([aircraft].[Manufacturer], '') <>     ISNULL([lookup].[Manufacturer], '')
    OR     ISNULL([aircraft].[Type], '') <>             ISNULL([lookup].[Type], '')
    OR     ISNULL([aircraft].[ICAOTypeCode], '') <>     ISNULL([lookup].[ICAOTypeCode], '')
    OR     ISNULL([aircraft].[RegisteredOwners], '') <> ISNULL([lookup].[RegisteredOwners], '')
    OR     ISNULL([aircraft].[OperatorFlagCode], '') <> ISNULL([lookup].[OperatorFlagCode], '')
    OR     ISNULL([aircraft].[SerialNo], '') <>         ISNULL([lookup].[SerialNo], '')
    OR     ISNULL([aircraft].[YearBuilt], '') <>        ISNULL([lookup].[YearBuilt], '')
    OR     [aircraft].[UserString1] = 'Missing';

    SELECT [aircraft].*
    FROM   [BaseStation].[Aircraft] AS [aircraft]
    JOIN   @Lookups                 AS [lookup]   ON [aircraft].[ModeS] = [lookup].[ModeS];

    SELECT *
    FROM   @action;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/AircraftAndFlightsCount_GetByCodes.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/AircraftAndFlightsCount_GetByCodes.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'AircraftAndFlightsCount_GetByCodes')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[AircraftAndFlightsCount_GetByCodes] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[AircraftAndFlightsCount_GetByCodes]
    @Codes AS [VRS].[Icao24] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    WITH [FlightCounts] AS (
        SELECT   [aircraft].[AircraftID]
                ,COUNT(*) AS [FlightsCount]
        FROM     @Codes                   AS [code]
        JOIN     [BaseStation].[Aircraft] AS [aircraft] ON [code].[ModeS] = [aircraft].[ModeS]
        JOIN     [BaseStation].[Flights]  AS [flight]   ON [aircraft].[AircraftID] = [flight].[AircraftID]
        GROUP BY [aircraft].[AircraftID]
    )
    SELECT    [aircraft].*
             ,ISNULL([flightCount].[FlightsCount], 0) AS [FlightsCount]
    FROM      @Codes                   AS [code]
    JOIN      [BaseStation].[Aircraft] AS [aircraft]    ON [code].[ModeS] = [aircraft].[ModeS]
    LEFT JOIN [FlightCounts]           AS [flightCount] ON [aircraft].[AircraftID] = [flightCount].[AircraftID];
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/DBHistory_GetAll.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/DBHistory_GetAll.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'DBHistory_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[DBHistory_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[DBHistory_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[DBHistory];
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/DBInfo_GetAll.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/DBInfo_GetAll.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'DBInfo_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[DBInfo_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[DBInfo_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[DBInfo];
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/Flights_Delete.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Flights_Delete.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_Delete]
    @FlightID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Flights]
    WHERE  [FlightID] = @FlightID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Flights_GetByID.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Flights_GetByID.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_GetByID')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_GetByID] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_GetByID]
    @FlightID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Flights]
    WHERE  [FlightID] = @FlightID;
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/Flights_Insert.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Flights_Insert.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_Insert]
    @SessionID           BIGINT
   ,@AircraftID          BIGINT
   ,@StartTime           DATETIME
   ,@EndTime             DATETIME = NULL
   ,@Callsign            NVARCHAR(20) = NULL
   ,@NumPosMsgRec        INT = NULL
   ,@NumADSBMsgRec       INT = NULL
   ,@NumModeSMsgRec      INT = NULL
   ,@NumIDMsgRec         INT = NULL
   ,@NumSurPosMsgRec     INT = NULL
   ,@NumAirPosMsgRec     INT = NULL
   ,@NumAirVelMsgRec     INT = NULL
   ,@NumSurAltMsgRec     INT = NULL
   ,@NumSurIDMsgRec      INT = NULL
   ,@NumAirToAirMsgRec   INT = NULL
   ,@NumAirCallRepMsgRec INT = NULL
   ,@FirstIsOnGround     BIT = 0
   ,@LastIsOnGround      BIT = 0
   ,@FirstLat            REAL = NULL
   ,@LastLat             REAL = NULL
   ,@FirstLon            REAL = NULL
   ,@LastLon             REAL = NULL
   ,@FirstGroundSpeed    REAL = NULL
   ,@LastGroundSpeed     REAL = NULL
   ,@FirstAltitude       INT = NULL
   ,@LastAltitude        INT = NULL
   ,@FirstVerticalRate   INT = NULL
   ,@LastVerticalRate    INT = NULL
   ,@FirstTrack          REAL = NULL
   ,@LastTrack           REAL = NULL
   ,@FirstSquawk         INT = NULL
   ,@LastSquawk          INT = NULL
   ,@HadAlert            BIT = 0
   ,@HadEmergency        BIT = 0
   ,@HadSPI              BIT = 0
   ,@UserNotes           NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[Flights] (
         [SessionID]
        ,[AircraftID]
        ,[StartTime]
        ,[EndTime]
        ,[Callsign]
        ,[NumPosMsgRec]
        ,[NumADSBMsgRec]
        ,[NumModeSMsgRec]
        ,[NumIDMsgRec]
        ,[NumSurPosMsgRec]
        ,[NumAirPosMsgRec]
        ,[NumAirVelMsgRec]
        ,[NumSurAltMsgRec]
        ,[NumSurIDMsgRec]
        ,[NumAirToAirMsgRec]
        ,[NumAirCallRepMsgRec]
        ,[FirstIsOnGround]
        ,[LastIsOnGround]
        ,[FirstLat]
        ,[LastLat]
        ,[FirstLon]
        ,[LastLon]
        ,[FirstGroundSpeed]
        ,[LastGroundSpeed]
        ,[FirstAltitude]
        ,[LastAltitude]
        ,[FirstVerticalRate]
        ,[LastVerticalRate]
        ,[FirstTrack]
        ,[LastTrack]
        ,[FirstSquawk]
        ,[LastSquawk]
        ,[HadAlert]
        ,[HadEmergency]
        ,[HadSPI]
        ,[UserNotes]
    ) VALUES (
         @SessionID
        ,@AircraftID
        ,@StartTime
        ,@EndTime
        ,@Callsign
        ,@NumPosMsgRec
        ,@NumADSBMsgRec
        ,@NumModeSMsgRec
        ,@NumIDMsgRec
        ,@NumSurPosMsgRec
        ,@NumAirPosMsgRec
        ,@NumAirVelMsgRec
        ,@NumSurAltMsgRec
        ,@NumSurIDMsgRec
        ,@NumAirToAirMsgRec
        ,@NumAirCallRepMsgRec
        ,@FirstIsOnGround
        ,@LastIsOnGround
        ,@FirstLat
        ,@LastLat
        ,@FirstLon
        ,@LastLon
        ,@FirstGroundSpeed
        ,@LastGroundSpeed
        ,@FirstAltitude
        ,@LastAltitude
        ,@FirstVerticalRate
        ,@LastVerticalRate
        ,@FirstTrack
        ,@LastTrack
        ,@FirstSquawk
        ,@LastSquawk
        ,@HadAlert
        ,@HadEmergency
        ,@HadSPI
        ,@UserNotes
    );

    SELECT SCOPE_IDENTITY() AS [FlightID];
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Flights_Update.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Flights_Update.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_Update]
    @FlightID            BIGINT
   ,@SessionID           BIGINT
   ,@AircraftID          BIGINT
   ,@StartTime           DATETIME = NULL
   ,@EndTime             DATETIME
   ,@Callsign            NVARCHAR(20)
   ,@NumPosMsgRec        INT
   ,@NumADSBMsgRec       INT
   ,@NumModeSMsgRec      INT
   ,@NumIDMsgRec         INT
   ,@NumSurPosMsgRec     INT
   ,@NumAirPosMsgRec     INT
   ,@NumAirVelMsgRec     INT
   ,@NumSurAltMsgRec     INT
   ,@NumSurIDMsgRec      INT
   ,@NumAirToAirMsgRec   INT
   ,@NumAirCallRepMsgRec INT
   ,@FirstIsOnGround     BIT
   ,@LastIsOnGround      BIT
   ,@FirstLat            REAL
   ,@LastLat             REAL
   ,@FirstLon            REAL
   ,@LastLon             REAL
   ,@FirstGroundSpeed    REAL
   ,@LastGroundSpeed     REAL
   ,@FirstAltitude       INT
   ,@LastAltitude        INT
   ,@FirstVerticalRate   INT
   ,@LastVerticalRate    INT
   ,@FirstTrack          REAL
   ,@LastTrack           REAL
   ,@FirstSquawk         INT
   ,@LastSquawk          INT
   ,@HadAlert            BIT
   ,@HadEmergency        BIT
   ,@HadSPI              BIT
   ,@UserNotes           NVARCHAR(300)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Flights]
    SET    [SessionID]              = @SessionID
          ,[AircraftID]             = @AircraftID
          ,[StartTime]              = ISNULL(@StartTime, [StartTime])
          ,[EndTime]                = @EndTime
          ,[Callsign]               = @Callsign
          ,[NumPosMsgRec]           = @NumPosMsgRec
          ,[NumADSBMsgRec]          = @NumADSBMsgRec
          ,[NumModeSMsgRec]         = @NumModeSMsgRec
          ,[NumIDMsgRec]            = @NumIDMsgRec
          ,[NumSurPosMsgRec]        = @NumSurPosMsgRec
          ,[NumAirPosMsgRec]        = @NumAirPosMsgRec
          ,[NumAirVelMsgRec]        = @NumAirVelMsgRec
          ,[NumSurAltMsgRec]        = @NumSurAltMsgRec
          ,[NumSurIDMsgRec]         = @NumSurIDMsgRec
          ,[NumAirToAirMsgRec]      = @NumAirToAirMsgRec
          ,[NumAirCallRepMsgRec]    = @NumAirCallRepMsgRec
          ,[FirstIsOnGround]        = @FirstIsOnGround
          ,[LastIsOnGround]         = @LastIsOnGround
          ,[FirstLat]               = @FirstLat
          ,[LastLat]                = @LastLat
          ,[FirstLon]               = @FirstLon
          ,[LastLon]                = @LastLon
          ,[FirstGroundSpeed]       = @FirstGroundSpeed
          ,[LastGroundSpeed]        = @LastGroundSpeed
          ,[FirstAltitude]          = @FirstAltitude
          ,[LastAltitude]           = @LastAltitude
          ,[FirstVerticalRate]      = @FirstVerticalRate
          ,[LastVerticalRate]       = @LastVerticalRate
          ,[FirstTrack]             = @FirstTrack
          ,[LastTrack]              = @LastTrack
          ,[FirstSquawk]            = @FirstSquawk
          ,[LastSquawk]             = @LastSquawk
          ,[HadAlert]               = @HadAlert
          ,[HadEmergency]           = @HadEmergency
          ,[HadSPI]                 = @HadSPI
          ,[UserNotes]              = @UserNotes
    WHERE [FlightID] = @FlightID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Locations_Delete.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Locations_Delete.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_Delete]
    @LocationID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Locations]
    WHERE  [LocationID] = @LocationID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Locations_GetAll.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Locations_GetAll.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Locations];
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/Locations_Insert.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Locations_Insert.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_Insert]
    @LocationName NVARCHAR(80)
   ,@Latitude     REAL
   ,@Longitude    REAL
   ,@Altitude     REAL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[Locations] (
         [LocationName]
        ,[Latitude]
        ,[Longitude]
        ,[Altitude]
    ) VALUES (
         @LocationName
        ,@Latitude
        ,@Longitude
        ,@Altitude
    );

    SELECT SCOPE_IDENTITY() AS [LocationID];
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Locations_Update.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Locations_Update.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_Update]
    @LocationID   BIGINT
   ,@LocationName NVARCHAR(80)
   ,@Latitude     REAL
   ,@Longitude    REAL
   ,@Altitude     REAL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Locations]
    SET    [LocationName]    = @LocationName
          ,[Latitude]        = @Latitude
          ,[Longitude]       = @Longitude
          ,[Altitude]        = @Altitude
    WHERE [LocationID] = @LocationID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Sessions_Delete.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Sessions_Delete.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Delete]
    @SessionID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Sessions]
    WHERE  [SessionID] = @SessionID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Sessions_GetAll.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Sessions_GetAll.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Sessions];
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/Sessions_Insert.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Sessions_Insert.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Insert]
    @LocationID BIGINT
   ,@StartTime  DATETIME
   ,@EndTime    DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[Sessions] (
         [LocationID]
        ,[StartTime]
        ,[EndTime]
    ) VALUES (
         @LocationID
        ,@StartTime
        ,@EndTime
    );

    SELECT SCOPE_IDENTITY() AS [SessionID];
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/Sessions_Update.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/Sessions_Update.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Update]
    @SessionID  BIGINT
   ,@LocationID BIGINT
   ,@StartTime  DATETIME = NULL
   ,@EndTime    DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Sessions]
    SET    [LocationID]    = @LocationID
          ,[StartTime]     = ISNULL(@StartTime, [StartTime])
          ,[EndTime]       = @EndTime
    WHERE [SessionID] = @SessionID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/SystemEvents_Delete.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/SystemEvents_Delete.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'SystemEvents_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[SystemEvents_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[SystemEvents_Delete]
    @SystemEventsID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[SystemEvents]
    WHERE  [SystemEventsID] = @SystemEventsID;
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/SystemEvents_GetAll.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/SystemEvents_GetAll.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'SystemEvents_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[SystemEvents_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[SystemEvents_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[SystemEvents];
END;
GO





-------------------------------------------------------------------------------
-- BaseStation/Procs/SystemEvents_Insert.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/SystemEvents_Insert.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'SystemEvents_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[SystemEvents_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[SystemEvents_Insert]
    @TimeStamp DATETIME
   ,@App       NVARCHAR(15)
   ,@Msg       NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[SystemEvents] (
         [TimeStamp]
        ,[App]
        ,[Msg]
    ) VALUES (
         @TimeStamp
        ,@App
        ,@Msg
    );

    SELECT SCOPE_IDENTITY() AS [SystemEventsID];
END;
GO




-------------------------------------------------------------------------------
-- BaseStation/Procs/SystemEvents_Update.sql
-------------------------------------------------------------------------------
PRINT 'BaseStation/Procs/SystemEvents_Update.sql';
GO

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'SystemEvents_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[SystemEvents_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[SystemEvents_Update]
    @SystemEventsID BIGINT
   ,@TimeStamp      DATETIME
   ,@App            NVARCHAR(15)
   ,@Msg            NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[SystemEvents]
    SET    [TimeStamp]         = @TimeStamp
          ,[App]               = @App
          ,[Msg]               = @Msg
    WHERE [SystemEventsID] = @SystemEventsID;
END;
GO



