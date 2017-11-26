IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'DBInfo')
BEGIN
    CREATE TABLE [BaseStation].[DBInfo]
    (
        [OriginalVersion]   SMALLINT NOT NULL
       ,[CurrentVersion]    SMALLINT NOT NULL

       ,CONSTRAINT [PK_DBInfo] PRIMARY KEY ([OriginalVersion], [CurrentVersion])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[DBInfo])
BEGIN
    INSERT INTO [BaseStation].[DBInfo] (
        [OriginalVersion]
       ,[CurrentVersion]
    ) VALUES (
        2
       ,2
    );
END;
GO
