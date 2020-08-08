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
