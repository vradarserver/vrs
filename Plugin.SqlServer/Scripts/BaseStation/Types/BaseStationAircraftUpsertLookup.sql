IF NOT EXISTS (
    SELECT 1
    FROM   [sys].[table_types] AS [tt]
    JOIN   [sys].[schemas]     AS [s]  ON [tt].[schema_id] = [s].[schema_id]
    WHERE  [s].[name] =  'BaseStation'
    AND    [tt].[name] = 'BaseStationAircraftUpsertLookup'
)
BEGIN
    CREATE TYPE [BaseStation].[BaseStationAircraftUpsertLookup] AS TABLE
    (
        [ModeS]             VARCHAR(6) NOT NULL PRIMARY KEY
       ,[LastModified]      DATETIME NOT NULL
       ,[Registration]      NVARCHAR(20)
       ,[Country]           NVARCHAR(80)
       ,[ModeSCountry]      NVARCHAR(80)
       ,[Manufacturer]      NVARCHAR(80)
       ,[Type]              NVARCHAR(80)
       ,[ICAOTypeCode]      NVARCHAR(10)
       ,[RegisteredOwners]  NVARCHAR(100)
       ,[OperatorFlagCode]  NVARCHAR(20)
       ,[SerialNo]          NVARCHAR(30)
       ,[YearBuilt]         NVARCHAR(4)
    );
END;
GO
