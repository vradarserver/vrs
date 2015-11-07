--
-- DatabaseVersion
--
CREATE TABLE IF NOT EXISTS [DatabaseVersion]
(
    [Version]   INTEGER NOT NULL
);


--
-- AircraftDetail
--
CREATE TABLE IF NOT EXISTS [AircraftDetail]
(
    [AircraftDetailID]   INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Icao]               VARCHAR(6) NOT NULL COLLATE NOCASE
   ,[Registration]       VARCHAR(20) NULL COLLATE NOCASE
   ,[Country]            NVARCHAR(200) NULL
   ,[Manufacturer]       NVARCHAR(200) NULL
   ,[Model]              NVARCHAR(200) NULL
   ,[ModelIcao]          VARCHAR(10) NULL COLLATE NOCASE
   ,[Operator]           NVARCHAR(200) NULL
   ,[OperatorIcao]       VARCHAR(3) NULL COLLATE NOCASE
   ,[Serial]             VARCHAR(80) NULL
   ,[YearBuilt]          INTEGER NULL
   ,[CreatedUtc]         DATETIME NOT NULL
   ,[UpdatedUtc]         DATETIME NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS [IX_Aircraft_Icao] ON [AircraftDetail] ([Icao]);
