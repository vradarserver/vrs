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
-- AircraftList (v1)
--
CREATE TABLE IF NOT EXISTS [AircraftList]
(
    [AircraftListID]        BIGINT NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[VrsSessionID]          BIGINT NOT NULL CONSTRAINT [FK_AircraftList_VrsSession] REFERENCES [VrsSession] ([VrsSessionID])
   ,[IsKeyList]             BIT NOT NULL
   ,[ReceiverSnapshotID]    BIGINT NOT NULL CONSTRAINT [FK_AircraftList_ReceiverSnapshot] REFERENCES [ReceiverSnapshot] ([ReceiverSnapshotID])
   ,[CreatedUtc]            DATETIME NOT NULL
   ,[UpdatedUtc]            DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_AircraftList_ReceiverCreated]          ON [AircraftList] ([ReceiverSnapshotID], [Created]);
CREATE INDEX IF NOT EXISTS [IX_AircraftList_ReceiverdCreatedKeyLists] ON [AircraftList] ([ReceiverSnapshotID], [Created]) WHERE [IsKeyList] = 1;
