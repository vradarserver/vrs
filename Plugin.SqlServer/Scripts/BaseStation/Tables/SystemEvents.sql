IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'SystemEvents')
BEGIN
    CREATE TABLE [BaseStation].[SystemEvents]
    (
        [SystemEventsID]    BIGINT IDENTITY
       ,[TimeStamp]         DATETIME NOT NULL
       ,[App]               VARCHAR(15) NOT NULL
       ,[Msg]               VARCHAR(100) NOT NULL

       ,CONSTRAINT [PK_SystemEvents] PRIMARY KEY ([SystemEventsID])
    );

    CREATE INDEX [IX_SystemEvents_App]          ON [BaseStation].[SystemEvents] ([App]);
    CREATE INDEX [IX_SystemEvents_TimeStamp]    ON [BaseStation].[SystemEvents] ([TimeStamp]);
END;
GO
