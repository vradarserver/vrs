IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'DBHistory')
BEGIN
    CREATE TABLE [BaseStation].[DBHistory]
    (
        [DBHistoryID]   BIGINT IDENTITY
       ,[TimeStamp]     DATETIME NOT NULL
       ,[Description]   VARCHAR(100) NOT NULL

       ,CONSTRAINT [PK_DBHistory] PRIMARY KEY ([DBHistoryID])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[DBHistory])
BEGIN
    INSERT INTO [BaseStation].[DBHistory] (
        [TimeStamp]
       ,[Description]
    ) VALUES (
        GETUTCDATE()
       ,'Schema created by ' + SUSER_NAME()
    );
END;
GO
