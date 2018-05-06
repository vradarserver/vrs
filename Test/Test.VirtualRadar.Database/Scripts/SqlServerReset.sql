USE [master];

-- Create the test database if it does not already exist
IF NOT EXISTS (SELECT 1 FROM [sys].[databases] WHERE [name] = 'VRSTest')
BEGIN
    CREATE DATABASE [VRSTest];
END;
GO


USE [VRSTest];

-- Drop all tables and views
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Flights')
BEGIN
    DROP TABLE [BaseStation].[Flights];
END;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Aircraft')
BEGIN
    DROP TABLE [BaseStation].[Aircraft];
END;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Sessions')
BEGIN
    DROP TABLE [BaseStation].[Sessions];
END;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Locations')
BEGIN
    DROP TABLE [BaseStation].[Locations];
END;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'SystemEvents')
BEGIN
    DROP TABLE [BaseStation].[SystemEvents];
END;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'DBHistory')
BEGIN
    DROP TABLE [BaseStation].[DBHistory];
END;
GO
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'DBInfo')
BEGIN
    DROP TABLE [BaseStation].[DBInfo];
END;
GO

-- Drop all stored procedures
DECLARE @schema AS NVARCHAR(128);
DECLARE @procedure AS NVARCHAR(128);
DECLARE @sql AS NVARCHAR(MAX);
DECLARE [StoredProcedureCursor] CURSOR LOCAL FAST_FORWARD READ_ONLY FOR
SELECT ROUTINE_SCHEMA
      ,ROUTINE_NAME
FROM   INFORMATION_SCHEMA.ROUTINES
WHERE  ROUTINE_TYPE = 'PROCEDURE';

DECLARE @storedProcedureCursorStatus AS INTEGER = 0;
OPEN [StoredProcedureCursor];
WHILE @storedProcedureCursorStatus = 0
BEGIN
    FETCH NEXT FROM [StoredProcedureCursor] 
    INTO  @schema
         ,@procedure;
    SET @storedProcedureCursorStatus = @@FETCH_STATUS;
    IF @storedProcedureCursorStatus = 0
    BEGIN
        SET @sql = 'DROP PROCEDURE [' + @schema + '].[' + @procedure + ']';
        EXEC [sys].[sp_executesql] @sql;
    END;
END;
CLOSE [StoredProcedureCursor];
DEALLOCATE [StoredProcedureCursor];
GO

-- Drop all UDTTs
IF EXISTS (SELECT * FROM [sys].[table_types] AS [tt] JOIN [sys].[schemas] AS [s] ON [tt].[schema_id] = [s].[schema_id] WHERE [s].[name] = 'VRS' AND [tt].[name] = 'Icao24')
BEGIN
    DROP TYPE [VRS].[Icao24];
END;
GO
IF EXISTS (SELECT * FROM [sys].[table_types] AS [tt] JOIN [sys].[schemas] AS [s] ON [tt].[schema_id] = [s].[schema_id] WHERE [s].[name] = 'BaseStation' AND [tt].[name] = 'BaseStationAircraftUpsert')
BEGIN
    DROP TYPE [BaseStation].[BaseStationAircraftUpsert];
END;
GO
IF EXISTS (SELECT * FROM [sys].[table_types] AS [tt] JOIN [sys].[schemas] AS [s] ON [tt].[schema_id] = [s].[schema_id] WHERE [s].[name] = 'BaseStation' AND [tt].[name] = 'BaseStationAircraftUpsertLookup')
BEGIN
    DROP TYPE [BaseStation].[BaseStationAircraftUpsertLookup];
END;
GO
IF EXISTS (SELECT * FROM [sys].[table_types] AS [tt] JOIN [sys].[schemas] AS [s] ON [tt].[schema_id] = [s].[schema_id] WHERE [s].[name] = 'BaseStation' AND [tt].[name] = 'BaseStationFlightUpsert')
BEGIN
    DROP TYPE [BaseStation].[BaseStationFlightUpsert];
END;
GO
