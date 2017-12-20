IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'DBHistory')
BEGIN
    CREATE TABLE [BaseStation].[DBHistory]
    (
        [DBHistoryID]   INTEGER IDENTITY
       ,[TimeStamp]     DATETIME2 NOT NULL
       ,[Description]   NVARCHAR(100) NOT NULL

       ,CONSTRAINT [PK_DBHistory] PRIMARY KEY ([DBHistoryID])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[DBHistory] WHERE [Description] LIKE 'Schema created by %')
BEGIN
    INSERT INTO [BaseStation].[DBHistory] (
        [TimeStamp]
       ,[Description]
    ) VALUES (
        GETDATE()
       ,'Schema created by ' + SUSER_NAME()
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[DBHistory] WHERE [Description] = 'Database autocreated by Virtual Radar Server')
BEGIN
    -- This one is required by BaseStationDBHistory.IsCreationOfDatabaseByVirtualRadarServer
    INSERT INTO [BaseStation].[DBHistory] (
        [TimeStamp]
       ,[Description]
    ) VALUES (
        GETDATE()
       ,'Database autocreated by Virtual Radar Server'
    );
END;
GO
