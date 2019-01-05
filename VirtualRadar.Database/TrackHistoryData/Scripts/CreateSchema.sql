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
    [ReceiverID]    INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Name]          NVARCHAR(255) COLLATE NOCASE
   ,[CreatedUtc]    DATETIME NOT NULL
   ,[UpdatedUtc]    DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Receiver_Name] ON [Receiver] ([Name]);


--
-- Country
--
CREATE TABLE IF NOT EXISTS [Country]
(
    [CountryID]     INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Name]          NVARCHAR(255) COLLATE NOCASE
   ,[CreatedUtc]    DATETIME NOT NULL
   ,[UpdatedUtc]    DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Country_Name] ON [Country] ([Name]);


--
-- Manufacturer
--
CREATE TABLE IF NOT EXISTS [Manufacturer]
(
    [ManufacturerID]    INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Name]              NVARCHAR(255) COLLATE NOCASE
   ,[CreatedUtc]        DATETIME NOT NULL
   ,[UpdatedUtc]        DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Manufacturer_Name] ON [Manufacturer] ([Name]);


--
-- Model
--
CREATE TABLE IF NOT EXISTS [Model]
(
    [ModelID]       INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Name]          NVARCHAR(255) COLLATE NOCASE
   ,[CreatedUtc]    DATETIME NOT NULL
   ,[UpdatedUtc]    DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Model_Name] ON [Model] ([Name]);


--
-- EnginePlacement
--
CREATE TABLE IF NOT EXISTS [EnginePlacement]
(
    [EnginePlacementID] INTEGER PRIMARY KEY
   ,[Code]              VARCHAR(10) NOT NULL
   ,[Description]       VARCHAR(20) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_EnginePlacement_Code] ON [EnginePlacement] ([Code]);

INSERT OR IGNORE INTO [EnginePlacement] ([EnginePlacementID], [Code], [Description]) VALUES
 (1, 'AM', 'Aft Mounted')
,(2, 'WB', 'Wing Buried')
,(3, 'FB', 'Fuselage Buried')
,(4, 'NM', 'Nose Mounted')
,(5, 'WM', 'Wing Mounted')
;


--
-- EngineType
--
CREATE TABLE IF NOT EXISTS [EngineType]
(
    [EngineTypeID]  INTEGER PRIMARY KEY
   ,[Code]          VARCHAR(10) NOT NULL
   ,[Description]   VARCHAR(20) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_EngineType_Code] ON [EngineType] ([Code]);

INSERT OR IGNORE INTO [EngineType] ([EngineTypeID], [Code], [Description]) VALUES
 (1, 'P', 'Piston')
,(2, 'T', 'Turboprop')
,(3, 'J', 'Jet')
,(4, 'E', 'Electric')
,(5, 'R', 'Rocket')
;


--
-- Species
--
CREATE TABLE IF NOT EXISTS [Species]
(
    [SpeciesID]     INTEGER PRIMARY KEY
   ,[Code]          VARCHAR(10) NOT NULL
   ,[IsFake]        BIT NOT NULL
   ,[Description]   VARCHAR(20) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Species_Code] ON [Species] ([Code]);

INSERT OR IGNORE INTO [Species] ([SpeciesID], [Code], [IsFake], [Description]) VALUES
 (1, 'L',    0, 'Landplane')
,(2, 'S',    0, 'Seaplane')
,(3, 'A',    0, 'Amphibian')
,(4, 'H',    0, 'Helicopter')
,(5, 'G',    0, 'Gyrocopter')
,(6, 'T',    0, 'Tiltwing')
,(7, '-GND', 1, 'Ground Vehicle')
,(8, '-TWR', 1, 'Radio Mast')
;


--
-- WakeTurbulenceCategory
--
CREATE TABLE IF NOT EXISTS [WakeTurbulenceCategory]
(
    [WakeTurbulenceCategoryID]  INTEGER PRIMARY KEY
   ,[Code]                      VARCHAR(10) NOT NULL
   ,[Description]               VARCHAR(20) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_WakeTurbulenceCategory_Code] ON [WakeTurbulenceCategory] ([Code]);

INSERT OR IGNORE INTO [WakeTurbulenceCategory] ([WakeTurbulenceCategoryID], [Code], [Description]) VALUES
 (1, 'L', 'Light')
,(2, 'M', 'Medium')
,(3, 'H', 'Heavy')
;


--
-- AircraftType
--
CREATE TABLE IF NOT EXISTS [AircraftType]
(
    [AircraftTypeID]           INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Icao]                     VARCHAR(10) NOT NULL
   ,[ManufacturerID]           INTEGER NULL CONSTRAINT [FK_AircraftType_Manufacturer]           REFERENCES [Manufacturer]           ([ManufacturerID])           ON DELETE SET NULL
   ,[ModelID]                  INTEGER NULL CONSTRAINT [FK_AircraftType_Model]                  REFERENCES [Model]                  ([ModelID])                  ON DELETE SET NULL
   ,[EngineTypeID]             INTEGER NULL CONSTRAINT [FK_AircraftType_EngineType]             REFERENCES [EngineType]             ([EngineTypeID])             ON DELETE SET NULL
   ,[EnginePlacementID]        INTEGER NULL CONSTRAINT [FK_AircraftType_EnginePlacement]        REFERENCES [EnginePlacement]        ([EnginePlacementID])        ON DELETE SET NULL
   ,[WakeTurbulenceCategoryID] INTEGER NULL CONSTRAINT [FK_AircraftType_WakeTurbulenceCategory] REFERENCES [WakeTurbulenceCategory] ([WakeTurbulenceCategoryID]) ON DELETE SET NULL
   ,[EngineCount]              VARCHAR(1) NULL
   ,[CreatedUtc]               DATETIME NOT NULL
   ,[UpdatedUtc]               DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_AircraftType_Icao] ON [AircraftType] ([Icao]);


--
-- Operator
--
CREATE TABLE IF NOT EXISTS [Operator]
(
    [OperatorID]    INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Icao]          VARCHAR(10) NOT NULL COLLATE NOCASE
   ,[Name]          NVARCHAR(255) NULL COLLATE NOCASE
   ,[CreatedUtc]    DATETIME NOT NULL
   ,[UpdatedUtc]    DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Operator_Icao_Name] ON [Operator] ([Icao], [Name]);


--
-- Aircraft
--
CREATE TABLE IF NOT EXISTS [Aircraft]
(
    [AircraftID]            INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Icao]                  VARCHAR(6) NOT NULL COLLATE NOCASE
   ,[IcaoCountryID]         INTEGER NULL CONSTRAINT [FK_Aircraft_IcaoCountry]  REFERENCES [Country]      ([CountryID])      ON DELETE SET NULL
   ,[AircraftTypeID]        INTEGER NULL CONSTRAINT [FK_Aircraft_AircraftType] REFERENCES [AircraftType] ([AircraftTypeID]) ON DELETE SET NULL
   ,[OperatorID]            INTEGER NULL CONSTRAINT [FK_Aircraft_Operator]     REFERENCES [Operator]     ([OperatorID])     ON DELETE SET NULL
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
CREATE INDEX        IF NOT EXISTS [IX_Aircraft_Registration] ON [Aircraft] ([Registration]) WHERE [Registration] IS NOT NULL;


--
-- TrackHistory
--
CREATE TABLE IF NOT EXISTS [TrackHistory]
(
    [TrackHistoryID]    INTEGER PRIMARY KEY AUTOINCREMENT
   ,[AircraftID]        INTEGER NOT NULL CONSTRAINT [FK_TrackHistory_Aircraft] REFERENCES [Aircraft] ([AircraftID]) ON DELETE CASCADE
   ,[IsPreserved]       BIT NOT NULL
   ,[CreatedUtc]        DATETIME NOT NULL
   ,[UpdatedUtc]        DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_TrackHistory_Aircraft]   ON [TrackHistory] ([AircraftID]);
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
INSERT OR IGNORE INTO [AltitudeType] ([AltitudeTypeID], [Description]) VALUES
 (0, 'Barometric')
,(1, 'Geometric')
;


--
-- SpeedType
--
CREATE TABLE IF NOT EXISTS [SpeedType]
(
    [SpeedTypeID]   INTEGER PRIMARY KEY
   ,[Description]   VARCHAR(80) NOT NULL
);
INSERT OR IGNORE INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES
 (0, 'Ground Speed')
,(1, 'Ground Speed Reversing')
,(2, 'Indicated Air Speed')
,(3, 'True Air Speed')
;


--
-- TrackHistoryState
--
CREATE TABLE IF NOT EXISTS [TrackHistoryState]
(
    [TrackHistoryStateID]   INTEGER PRIMARY KEY AUTOINCREMENT
   ,[TrackHistoryID]        INTEGER NOT NULL CONSTRAINT [FK_TrackHistoryState_TrackHistory] REFERENCES [TrackHistory] ([TrackHistoryID]) ON DELETE CASCADE
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
   ,[AltitudeTypeID]        INTEGER NULL CONSTRAINT [FK_TrackHistoryState_AltitudeType] REFERENCES [AltitudeType] ([AltitudeTypeID])
   ,[TargetAltitudeFeet]    INTEGER NULL
   ,[AirPressureInHg]       REAL NULL
   ,[GroundSpeedKnots]      REAL NULL
   ,[SpeedTypeID]           INTEGER NULL CONSTRAINT [FK_TrackHistoryState_SpeedType] REFERENCES [SpeedType] ([SpeedTypeID])
   ,[TrackDegrees]          REAL NULL
   ,[TargetTrack]           REAL NULL
   ,[TrackIsHeading]        BIT NULL
   ,[VerticalRateFeetMin]   INTEGER NULL
   ,[VerticalRateTypeID]    INTEGER NULL CONSTRAINT [FK_TrackHistoryState_VerticalRateType] REFERENCES [AltitudeType] ([AltitudeTypeID])
   ,[SquawkOctal]           INTEGER NULL
   ,[IdentActive]           BIT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_TrackHistoryState_TrackHistory] ON [TrackHistoryState] ([TrackHistoryID], [SequenceNumber]);
