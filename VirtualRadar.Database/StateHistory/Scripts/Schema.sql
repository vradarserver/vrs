--
-- Database Version (v1)
--
CREATE TABLE IF NOT EXISTS [DatabaseVersion]
(
    [DatabaseVersionID] INTEGER NOT NULL PRIMARY KEY
   ,[CreatedUtc]        DATETIME NOT NULL
);


--
-- VrsSession (v1)
--
CREATE TABLE IF NOT EXISTS [VrsSession]
(
    [VrsSessionID]      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[DatabaseVersionID] BIGINT NOT NULL
   ,[CreatedUtc]        DATETIME NOT NULL
);


--
-- CountrySnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [CountrySnapshot]
(
    [CountrySnapshotID]     INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]            DATETIME NOT NULL
   ,[Fingerprint]           VARBINARY(20) NOT NULL
   ,[CountryName]           NVARCHAR(80) NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_CountrySnapshot_Fingerprint] ON [CountrySnapshot] ([Fingerprint]);
CREATE        INDEX IF NOT EXISTS [IX_CountrySnapshot_CountryName] ON [CountrySnapshot] ([CountryName]);


--
-- Receiver (v1)
--
CREATE TABLE IF NOT EXISTS [ReceiverSnapshot]
(
    [ReceiverSnapshotID]    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]            DATETIME NOT NULL
   ,[Fingerprint]           VARBINARY(20) NOT NULL
   ,[Key]                   UNIQUEIDENTIFIER NOT NULL
   ,[ReceiverID]            INTEGER NOT NULL
   ,[ReceiverName]          NVARCHAR(255) NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_ReceiverSnapshot_Fingerprint] ON [ReceiverSnapshot] ([Fingerprint]);


--
-- EnginePlacementSnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [EnginePlacementSnapshot]
(
    [EnginePlacementSnapshotID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]                DATETIME NOT NULL
   ,[Fingerprint]               VARBINARY(20) NOT NULL
   ,[EnumValue]                 INTEGER NOT NULL
   ,[EnginePlacementName]       VARCHAR(80) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_EnginePlacementSnapshot_Fingerprint] ON [EnginePlacementSnapshot] ([Fingerprint]);


--
-- EngineTypeSnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [EngineTypeSnapshot]
(
    [EngineTypeSnapshotID]  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]            DATETIME NOT NULL
   ,[Fingerprint]           VARBINARY(20) NOT NULL
   ,[EnumValue]             INTEGER NOT NULL
   ,[EngineTypeName]        VARCHAR(80) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_EngineTypeSnapshot_Fingerprint] ON [EngineTypeSnapshot] ([Fingerprint]);


--
-- ManufacturerSnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [ManufacturerSnapshot]
(
    [ManufacturerSnapshotID]    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]                DATETIME NOT NULL
   ,[Fingerprint]               VARBINARY(20) NOT NULL
   ,[ManufacturerName]          NVARCHAR(80) NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_ManufacturerSnapshot_Fingerprint]      ON [ManufacturerSnapshot] ([Fingerprint]);
CREATE        INDEX IF NOT EXISTS [IX_ManufacturerSnapshot_ManufacturerName] ON [ManufacturerSnapshot] ([ManufacturerName]);


--
-- OperatorSnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [OperatorSnapshot]
(
    [OperatorSnapshotID]    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]            DATETIME NOT NULL
   ,[Fingerprint]           VARBINARY(20) NOT NULL
   ,[Icao]                  NVARCHAR(80) NULL
   ,[OperatorName]          NVARCHAR(100) NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_OperatorSnapshot_Fingerprint]  ON [OperatorSnapshot] ([Fingerprint]);
CREATE        INDEX IF NOT EXISTS [IX_OperatorSnapshot_Icao]         ON [OperatorSnapshot] ([Icao]);
CREATE        INDEX IF NOT EXISTS [IX_OperatorSnapshot_OperatorName] ON [OperatorSnapshot] ([OperatorName]);


--
-- SpeciesSnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [SpeciesSnapshot]
(
    [SpeciesSnapshotID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]        DATETIME NOT NULL
   ,[Fingerprint]       VARBINARY(20) NOT NULL
   ,[EnumValue]         INTEGER NOT NULL
   ,[SpeciesName]       VARCHAR(80) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_SpeciesSnapshot_Fingerprint] ON [SpeciesSnapshot] ([Fingerprint]);


--
-- WakeTurbulenceCategorySnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [WakeTurbulenceCategorySnapshot]
(
    [WakeTurbulenceCategorySnapshotID]  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]                        DATETIME NOT NULL
   ,[Fingerprint]                       VARBINARY(20) NOT NULL
   ,[EnumValue]                         INTEGER NOT NULL
   ,[WakeTurbulenceCategoryName]        VARCHAR(80) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_WakeTurbulenceCategorySnapshot_Fingerprint] ON [WakeTurbulenceCategorySnapshot] ([Fingerprint]);


--
-- ModelSnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [ModelSnapshot]
(
    [ModelSnapshotID]                   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]                        DATETIME NOT NULL
   ,[Fingerprint]                       VARBINARY(20) NOT NULL
   ,[Icao]                              NVARCHAR(80) NULL
   ,[ManufacturerSnapshotID]            INTEGER NULL CONSTRAINT [FK_ModelSnapshot_ManufacturerSnapshot] REFERENCES [ManufacturerSnapshot] ([ManufacturerSnapshotID])
   ,[ModelName]                         NVARCHAR(80) NULL
   ,[WakeTurbulenceCategorySnapshotID]  INTEGER NULL CONSTRAINT [FK_ModelSnapshot_WakeTurbulenceCategorySnapshot] REFERENCES [WakeTurbulenceCategorySnapshot] ([WakeTurbulenceCategorySnapshotID])
   ,[EngineTypeSnapshotID]              INTEGER NULL CONSTRAINT [FK_ModelSnapshot_EngineTypeSnapshot] REFERENCES [EngineTypeSnapshot] ([EngineTypeSnapshotID])
   ,[EnginePlacementSnapshotID]         INTEGER NULL CONSTRAINT [FK_ModelSnapshot_EnginePlacementSnapshot] REFERENCES [EnginePlacementSnapshot] ([EnginePlacementSnapshotID])
   ,[NumberOfEngines]                   VARCHAR(2) NULL
   ,[SpeciesSnapshotID]                 INTEGER NULL CONSTRAINT [FK_ModelSnapshot_SpeciesSnapshot] REFERENCES [SpeciesSnapshot] ([SpeciesSnapshotID])
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_ModelSnapshot_Fingerprint]            ON [ModelSnapshot] ([Fingerprint]);
CREATE        INDEX IF NOT EXISTS [IX_ModelSnapshot_Icao]                   ON [ModelSnapshot] ([Icao]) WHERE [Icao] IS NOT NULL;
CREATE        INDEX IF NOT EXISTS [IX_ModelSnapshot_ManufacturerSnapshotID] ON [ModelSnapshot] ([ManufacturerSnapshotID]) WHERE [ManufacturerSnapshotID] IS NOT NULL;
CREATE        INDEX IF NOT EXISTS [IX_ModelSnapshot_ModelName]              ON [ModelSnapshot] ([ModelName]) WHERE [ModelName] IS NOT NULL;


--
-- AircraftSnapshot (v1)
--
CREATE TABLE IF NOT EXISTS [AircraftSnapshot]
(
    [AircraftSnapshotID]                INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[CreatedUtc]                        DATETIME NOT NULL
   ,[Fingerprint]                       VARBINARY(20) NOT NULL
   ,[Icao]                              VARCHAR(6) NULL
   ,[Registration]                      NVARCHAR(80) NULL
   ,[ModelSnapshotID]                   INTEGER NULL CONSTRAINT [FK_AircraftSnapshot_ModelSnapshot] REFERENCES [ModelSnapshot] ([ModelSnapshotID])
   ,[ConstructionNumber]                NVARCHAR(80) NULL
   ,[YearBuilt]                         NVARCHAR(80) NULL
   ,[OperatorSnapshotID]                INTEGER NULL CONSTRAINT [FK_AircraftSnapshot_OperatorSnapshot] REFERENCES [OperatorSnapshot] ([OperatorSnapshotID])
   ,[CountrySnapshotID]                 INTEGER NULL CONSTRAINT [FK_AircraftSnapshot_CountrySnapshot] REFERENCES [OperatorSnapshot] ([CountrySnapshotID])
   ,[IsMilitary]                        BIT NULL
   ,[IsInteresting]                     BIT NULL
   ,[UserNotes]                         NVARCHAR(300) NULL
   ,[UserTag]                           NVARCHAR(300) NULL
);
CREATE UNIQUE INDEX IF NOT EXISTS [IX_AircraftSnapshot_Fingerprint]        ON [AircraftSnapshot] ([Fingerprint]);
CREATE        INDEX IF NOT EXISTS [IX_AircraftSnapshot_Icao]               ON [AircraftSnapshot] ([Icao]) WHERE [Icao] IS NOT NULL;
CREATE        INDEX IF NOT EXISTS [IX_AircraftSnapshot_Registration]       ON [AircraftSnapshot] ([Registration]) WHERE [Registration] IS NOT NULL;
CREATE        INDEX IF NOT EXISTS [IX_AircraftSnapshot_ModelSnapshotID]    ON [AircraftSnapshot] ([ModelSnapshotID]) WHERE [ModelSnapshotID] IS NOT NULL;
CREATE        INDEX IF NOT EXISTS [IX_AircraftSnapshot_OperatorSnapshotID] ON [AircraftSnapshot] ([OperatorSnapshotID]) WHERE [OperatorSnapshotID] IS NOT NULL;
CREATE        INDEX IF NOT EXISTS [IX_AircraftSnapshot_CountrySnapshotID]  ON [AircraftSnapshot] ([CountrySnapshotID]) WHERE [CountrySnapshotID] IS NOT NULL;

--
-- AircraftList (v1)
--
CREATE TABLE IF NOT EXISTS [AircraftList]
(
    [AircraftListID]        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[VrsSessionID]          INTEGER NOT NULL CONSTRAINT [FK_AircraftList_VrsSession] REFERENCES [VrsSession] ([VrsSessionID])
   ,[IsKeyList]             BIT NOT NULL
   ,[ReceiverSnapshotID]    INTEGER NOT NULL CONSTRAINT [FK_AircraftList_ReceiverSnapshot] REFERENCES [ReceiverSnapshot] ([ReceiverSnapshotID])
   ,[CreatedUtc]            DATETIME NOT NULL
   ,[UpdatedUtc]            DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_AircraftList_ReceiverCreated]          ON [AircraftList] ([ReceiverSnapshotID], [CreatedUtc]);
CREATE INDEX IF NOT EXISTS [IX_AircraftList_ReceiverdCreatedKeyLists] ON [AircraftList] ([ReceiverSnapshotID], [CreatedUtc]) WHERE [IsKeyList] = 1;

--
-- Flight (v1)
--
CREATE TABLE IF NOT EXISTS [Flight]
(
    [FlightID]          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[Preserve]          BIT NOT NULL
   ,[IntervalSeconds]   INTEGER NOT NULL
   ,[CreatedUtc]        DATETIME NOT NULL
   ,[UpdatedUtc]        DATETIME NOT NULL
);
