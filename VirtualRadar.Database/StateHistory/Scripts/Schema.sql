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
-- Database Version (v1)
--
CREATE TABLE IF NOT EXISTS [DatabaseVersion]
(
    [DatabaseVersionID] INTEGER NOT NULL PRIMARY KEY
   ,[CreatedUtc]        DATETIME NOT NULL
);


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
-- VrsSession (v1)
--
CREATE TABLE IF NOT EXISTS [VrsSession]
(
    [VrsSessionID]      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
   ,[DatabaseVersionID] BIGINT NOT NULL
   ,[CreatedUtc]        DATETIME NOT NULL
);
