IF NOT EXISTS (
    SELECT 1
    FROM   [sys].[table_types] AS [tt]
    JOIN   [sys].[schemas]     AS [s]  ON [tt].[schema_id] = [s].[schema_id]
    WHERE  [s].[name] =  'VRS'
    AND    [tt].[name] = 'Icao24'
)
BEGIN
    CREATE TYPE [VRS].[Icao24] AS TABLE
    (
        [ModeS] VARCHAR(6) NOT NULL PRIMARY KEY
    );
END;
GO
