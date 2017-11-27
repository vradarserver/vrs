PRINT 'Running schema upgrade on ' + @@SERVERNAME + ' in ' + DB_NAME() + ' AS ' + SUSER_NAME();
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
       ,[ModeSCountry]      VARCHAR(24)
       ,[Country]           VARCHAR(24)
       ,[Registration]      VARCHAR(20)
       ,[CurrentRegDate]    VARCHAR(10)
       ,[PreviousID]        VARCHAR(10)
       ,[FirstRegDate]      VARCHAR(10)
       ,[Status]            VARCHAR(10)
       ,[DeRegDate]         VARCHAR(10)
       ,[Manufacturer]      VARCHAR(60)
       ,[ICAOTypeCode]      VARCHAR(10)
       ,[Type]              VARCHAR(40)
       ,[SerialNo]          VARCHAR(30)
       ,[PopularName]       VARCHAR(20)
       ,[GenericName]       VARCHAR(20)
       ,[AircraftClass]     VARCHAR(20)
       ,[Engines]           VARCHAR(40)
       ,[OwnershipStatus]   VARCHAR(10)
       ,[RegisteredOwners]  VARCHAR(100)
       ,[MTOW]              VARCHAR(10)
       ,[TotalHours]        VARCHAR(20)
       ,[YearBuilt]         VARCHAR(4)
       ,[CofACategory]      VARCHAR(30)
       ,[CofAExpiry]        VARCHAR(10)
       ,[UserNotes]         VARCHAR(300)
       ,[Interested]        BIT NOT NULL CONSTRAINT [DF_Aircraft_Interested] DEFAULT 0
       ,[UserTag]           VARCHAR(5)
       ,[InfoURL]           VARCHAR(150)
       ,[PictureURL1]       VARCHAR(150)
       ,[PictureURL2]       VARCHAR(150)
       ,[PictureURL3]       VARCHAR(150)
       ,[UserBool1]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool1] DEFAULT 0
       ,[UserBool2]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool2] DEFAULT 0
       ,[UserBool3]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool3] DEFAULT 0
       ,[UserBool4]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool4] DEFAULT 0
       ,[UserBool5]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool5] DEFAULT 0
       ,[UserString1]       VARCHAR(20)
       ,[UserString2]       VARCHAR(20)
       ,[UserString3]       VARCHAR(20)
       ,[UserString4]       VARCHAR(20)
       ,[UserString5]       VARCHAR(20)
       ,[UserInt1]          BIGINT CONSTRAINT [DF_Aircraft_UserInt1] DEFAULT 0
       ,[UserInt2]          BIGINT CONSTRAINT [DF_Aircraft_UserInt2] DEFAULT 0
       ,[UserInt3]          BIGINT CONSTRAINT [DF_Aircraft_UserInt3] DEFAULT 0
       ,[UserInt4]          BIGINT CONSTRAINT [DF_Aircraft_UserInt4] DEFAULT 0
       ,[UserInt5]          BIGINT CONSTRAINT [DF_Aircraft_UserInt5] DEFAULT 0
       ,[OperatorFlagCode]  VARCHAR(20)

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
       ,[Description]   VARCHAR(100) NOT NULL

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
       ,[LocationName]  VARCHAR(20) NOT NULL
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
       ,[Callsign]              VARCHAR(20)
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
       ,[UserNotes]             VARCHAR(300)

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
       ,[App]               VARCHAR(15) NOT NULL
       ,[Msg]               VARCHAR(100) NOT NULL

       ,CONSTRAINT [PK_SystemEvents] PRIMARY KEY ([SystemEventsID])
    );

    CREATE INDEX [IX_SystemEvents_App]          ON [BaseStation].[SystemEvents] ([App]);
    CREATE INDEX [IX_SystemEvents_TimeStamp]    ON [BaseStation].[SystemEvents] ([TimeStamp]);
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



