--
-- Aircraft
--
CREATE TABLE IF NOT EXISTS [Aircraft]
(
    [AircraftID]        INTEGER PRIMARY KEY
   ,[FirstCreated]      DATETIME NOT NULL
   ,[LastModified]      DATETIME NOT NULL
   ,[ModeS]             VARCHAR(6) NOT NULL UNIQUE
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
   ,[Interested]        BOOLEAN NOT NULL DEFAULT 0
   ,[UserTag]           VARCHAR(5)
   ,[InfoURL]           VARCHAR(150)
   ,[PictureURL1]       VARCHAR(150)
   ,[PictureURL2]       VARCHAR(150)
   ,[PictureURL3]       VARCHAR(150)
   ,[UserBool1]         BOOLEAN NOT NULL DEFAULT 0
   ,[UserBool2]         BOOLEAN NOT NULL DEFAULT 0
   ,[UserBool3]         BOOLEAN NOT NULL DEFAULT 0
   ,[UserBool4]         BOOLEAN NOT NULL DEFAULT 0
   ,[UserBool5]         BOOLEAN NOT NULL DEFAULT 0
   ,[UserString1]       VARCHAR(20)
   ,[UserString2]       VARCHAR(20)
   ,[UserString3]       VARCHAR(20)
   ,[UserString4]       VARCHAR(20)
   ,[UserString5]       VARCHAR(20)
   ,[UserInt1]          INTEGER DEFAULT 0
   ,[UserInt2]          INTEGER DEFAULT 0
   ,[UserInt3]          INTEGER DEFAULT 0
   ,[UserInt4]          INTEGER DEFAULT 0
   ,[UserInt5]          INTEGER DEFAULT 0
   ,[OperatorFlagCode]  VARCHAR(20)
);
CREATE INDEX IF NOT EXISTS [AircraftAircraftClass]      ON [Aircraft]([AircraftClass]);
CREATE INDEX IF NOT EXISTS [AircraftCountry]            ON [Aircraft]([Country]);
CREATE INDEX IF NOT EXISTS [AircraftGenericName]        ON [Aircraft]([GenericName]);
CREATE INDEX IF NOT EXISTS [AircraftICAOTypeCode]       ON [Aircraft]([ICAOTypeCode]);
CREATE INDEX IF NOT EXISTS [AircraftInterested]         ON [Aircraft]([Interested]);
CREATE INDEX IF NOT EXISTS [AircraftManufacturer]       ON [Aircraft]([Manufacturer]);
CREATE INDEX IF NOT EXISTS [AircraftModeS]              ON [Aircraft]([ModeS]);
CREATE INDEX IF NOT EXISTS [AircraftModeSCountry]       ON [Aircraft]([ModeSCountry]);
CREATE INDEX IF NOT EXISTS [AircraftPopularName]        ON [Aircraft]([PopularName]);
CREATE INDEX IF NOT EXISTS [AircraftRegisteredOwners]   ON [Aircraft]([RegisteredOwners]);
CREATE INDEX IF NOT EXISTS [AircraftRegistration]       ON [Aircraft]([Registration]);
CREATE INDEX IF NOT EXISTS [AircraftSerialNo]           ON [Aircraft]([SerialNo]);
CREATE INDEX IF NOT EXISTS [AircraftType]               ON [Aircraft]([Type]);
CREATE INDEX IF NOT EXISTS [AircraftUserTag]            ON [Aircraft]([UserTag]);
CREATE INDEX IF NOT EXISTS [AircraftYearBuilt]          ON [Aircraft]([YearBuilt]);

CREATE TRIGGER IF NOT EXISTS [AircraftIDdeltrig] BEFORE DELETE ON [Aircraft]
FOR EACH ROW BEGIN
    DELETE FROM [Flights] WHERE [AircraftID] = OLD.[AircraftID];
END;

--
-- DBHistory
--
CREATE TABLE IF NOT EXISTS [DBHistory]
(
    [DBHistoryID]   INTEGER PRIMARY KEY
   ,[TimeStamp]     DATETIME NOT NULL
   ,[Description]   VARCHAR(100) NOT NULL
);


--
-- DBInfo
--
CREATE TABLE IF NOT EXISTS [DBInfo]
(
    [OriginalVersion]   SMALLINT NOT NULL
   ,[CurrentVersion]    SMALLINT NOT NULL
);


--
-- Locations
--
CREATE TABLE IF NOT EXISTS [Locations]
(
    [LocationID]    INTEGER PRIMARY KEY
   ,[LocationName]  VARCHAR(20) NOT NULL
   ,[Latitude]      REAL NOT NULL
   ,[Longitude]     REAL NOT NULL
   ,[Altitude]      REAL NOT NULL
);
CREATE INDEX IF NOT EXISTS [LocationsLocationName] ON [Locations] ([LocationName]);


--
-- Sessions
--
CREATE TABLE IF NOT EXISTS [Sessions]
(
    [SessionID]     INTEGER PRIMARY KEY
   ,[LocationID]    INTEGER NOT NULL
   ,[StartTime]     DATETIME NOT NULL
   ,[EndTime]       DATETIME
   ,CONSTRAINT      [LocationIDfk] FOREIGN KEY ([LocationID]) REFERENCES [Locations]
);
CREATE INDEX IF NOT EXISTS [SessionsEndTime]    ON [Sessions]([EndTime]);
CREATE INDEX IF NOT EXISTS [SessionsLocationID] ON [Sessions]([LocationID]);
CREATE INDEX IF NOT EXISTS [SessionsStartTime]  ON [Sessions]([StartTime]);

CREATE TRIGGER IF NOT EXISTS [SessionIDdeltrig] BEFORE DELETE ON [Sessions]
FOR EACH ROW BEGIN
    DELETE FROM [Flights] WHERE [SessionID] = OLD.[SessionID];
END;


--
-- Flights
--
CREATE TABLE IF NOT EXISTS [Flights]
(
    [FlightID]              INTEGER PRIMARY KEY
   ,[SessionID]             INTEGER NOT NULL
   ,[AircraftID]            INTEGER NOT NULL
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
   ,[FirstIsOnGround]       BOOLEAN NOT NULL DEFAULT 0
   ,[LastIsOnGround]        BOOLEAN NOT NULL DEFAULT 0
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
   ,[HadAlert]              BOOLEAN NOT NULL DEFAULT 0
   ,[HadEmergency]          BOOLEAN NOT NULL DEFAULT 0
   ,[HadSPI]                BOOLEAN NOT NULL DEFAULT 0
   ,[UserNotes]             VARCHAR(300)
   ,CONSTRAINT [SessionIDfk]    FOREIGN KEY ([SessionID])   REFERENCES [Sessions]
   ,CONSTRAINT [AircraftIDfk]   FOREIGN KEY ([AircraftID])  REFERENCES [Aircraft]
);
CREATE INDEX IF NOT EXISTS [FlightsAircraftID]  ON [Flights]([AircraftID]);
CREATE INDEX IF NOT EXISTS [FlightsCallsign]    ON [Flights]([Callsign]);
CREATE INDEX IF NOT EXISTS [FlightsEndTime]     ON [Flights]([EndTime]);
CREATE INDEX IF NOT EXISTS [FlightsSessionID]   ON [Flights]([SessionID]);
CREATE INDEX IF NOT EXISTS [FlightsStartTime]   ON [Flights]([StartTime]);


--
-- SystemEvents
--
CREATE TABLE IF NOT EXISTS [SystemEvents]
(
    [SystemEventsID]    INTEGER PRIMARY KEY
   ,[TimeStamp]         DATETIME NOT NULL
   ,[App]               VARCHAR(15) NOT NULL
   ,[Msg]               VARCHAR(100) NOT NULL
);
CREATE INDEX [SystemEventsApp]          ON [SystemEvents]([App]);
CREATE INDEX [SystemEventsTimeStamp]    ON [SystemEvents]([TimeStamp]);
