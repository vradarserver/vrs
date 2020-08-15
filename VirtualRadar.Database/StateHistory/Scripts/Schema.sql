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
