--
-- DatabaseVersion
--
CREATE TABLE IF NOT EXISTS [DatabaseVersion]
(
    [DatabaseVersionID] INTEGER PRIMARY KEY    -- There is only one of these and its ID is 1
   ,[Version]           INTEGER NOT NULL
);
INSERT OR IGNORE INTO [DatabaseVersion] ([DatabaseVersionID], [Version]) VALUES (1, 1);


--
-- Receiver
--
CREATE TABLE IF NOT EXISTS [Receiver]
(
    [ReceiverID]        INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Name]              NVARCHAR(255) COLLATE NOCASE
   ,[CreatedUtc]        DATETIME NOT NULL
   ,[UpdatedUtc]        DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Receiver_Name] ON [Receiver] ([Name]);


--
-- Aircraft
--
CREATE TABLE IF NOT EXISTS [Aircraft]
(
    [AircraftID]            INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Icao]                  VARCHAR(6) NOT NULL COLLATE NOCASE
   ,[Registration]          VARCHAR(20) NULL COLLATE NOCASE
   ,[Serial]                NVARCHAR(200) NULL COLLATE NOCASE
   ,[YearBuilt]             INTEGER NULL
   ,[IsInteresting]         BIT NOT NULL
   ,[Notes]                 NVARCHAR(2000) NULL
   ,[LastLookupUtc]         DATETIME NULL
   ,[IsMissingFromLookup]   BIT NOT NULL
   ,[SuppressAutoUpdates]   BIT NOT NULL
   ,[CreatedUtc]            DATETIME NOT NULL
   ,[UpdatedUtc]            DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Aircraft_Icao]         ON [Aircraft] ([Icao]);
CREATE INDEX        IF NOT EXISTS [IX_Aircraft_Registration] ON [Aircraft] ([Registration]);


--
-- TrackHistory
--
CREATE TABLE IF NOT EXISTS [TrackHistory]
(
    [TrackHistoryID]    INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Icao]              VARCHAR(6) NOT NULL COLLATE NOCASE
   ,[IsPreserved]       BIT NOT NULL
   ,[CreatedUtc]        DATETIME NOT NULL
   ,[UpdatedUtc]        DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_TrackHistory_Icao]       ON [TrackHistory] ([Icao]);
CREATE INDEX IF NOT EXISTS [IX_TrackHistory_CreatedUtc] ON [TrackHistory] ([CreatedUtc]);
CREATE INDEX IF NOT EXISTS [IX_TrackHistory_UpdatedUtc] ON [TrackHistory] ([UpdatedUtc]);


--
-- AltitudeType
--
CREATE TABLE IF NOT EXISTS [AltitudeType]
(
    [AltitudeTypeID]    INTEGER PRIMARY KEY
   ,[Description]       VARCHAR(80) NOT NULL
);
INSERT OR IGNORE INTO [AltitudeType] ([AltitudeTypeID], [Description]) VALUES (0, 'Barometric');
INSERT OR IGNORE INTO [AltitudeType] ([AltitudeTypeID], [Description]) VALUES (1, 'Geometric');


--
-- SpeedType
--
CREATE TABLE IF NOT EXISTS [SpeedType]
(
    [SpeedTypeID]   INTEGER PRIMARY KEY
   ,[Description]   VARCHAR(80) NOT NULL
);
INSERT OR IGNORE INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (0, 'Ground Speed');
INSERT OR IGNORE INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (1, 'Ground Speed Reversing');
INSERT OR IGNORE INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (2, 'Indicated Air Speed');
INSERT OR IGNORE INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (3, 'True Air Speed');


--
-- TrackHistoryState
--
-- Ideally the enum columns (AltitudeType etc.) should have an ID suffix to match their foreign reference column.
-- However, to keep life simple with Dapper I'm going to keep a 1:1 relationship between column names here and
-- property names in the source DTO. Dapper can be given field maps but there's no (as of time of writing) attribute
-- support for column mapping and I cannot be arsed to make sure that Dapper gets initialised with a field map just
-- to keep the ID suffix on religous grounds.
CREATE TABLE IF NOT EXISTS [TrackHistoryState]
(
    [TrackHistoryStateID]   INTEGER PRIMARY KEY AUTOINCREMENT
   ,[TrackHistoryID]        INTEGER NOT NULL CONSTRAINT [FK_TrackHistoryState_TrackHistory] REFERENCES [TrackHistory] ([TrackHistoryID])
   ,[TimestampUtc]          DATETIME NOT NULL
   ,[SequenceNumber]        INTEGER NOT NULL
   ,[ReceiverID]            INTEGER NULL CONSTRAINT [FK_TrackHistoryState_Receiver] REFERENCES [Receiver] ([ReceiverID]) ON DELETE SET NULL
   ,[SignalLevel]           INTEGER NULL
   ,[Callsign]              VARCHAR(8) NULL
   ,[IsCallsignSuspect]     BIT NULL
   ,[Latitude]              REAL NULL
   ,[Longitude]             REAL NULL
   ,[IsMlat]                BIT NULL
   ,[IsTisb]                BIT NULL
   ,[AltitudeFeet]          INTEGER NULL
   ,[AltitudeType]          INTEGER NULL CONSTRAINT [FK_TrackHistoryState_AltitudeType] REFERENCES [AltitudeType] ([AltitudeTypeID])
   ,[TargetAltitudeFeet]    INTEGER NULL
   ,[AirPressureInHg]       REAL NULL
   ,[GroundSpeedKnots]      REAL NULL
   ,[SpeedType]             INTEGER NULL CONSTRAINT [FK_TrackHistoryState_SpeedType] REFERENCES [SpeedType] ([SpeedTypeID])
   ,[TrackDegrees]          REAL NULL
   ,[TargetTrack]           REAL NULL
   ,[TrackIsHeading]        BIT NULL
   ,[VerticalRateFeetMin]   INTEGER NULL
   ,[VerticalRateType]      INTEGER NULL CONSTRAINT [FK_TrackHistoryState_VerticalRateType] REFERENCES [AltitudeType] ([AltitudeTypeID])
   ,[SquawkOctal]           INTEGER NULL
   ,[IdentActive]           BIT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_TrackHistoryState_TrackHistory] ON [TrackHistoryState] ([TrackHistoryID], [SequenceNumber]);
