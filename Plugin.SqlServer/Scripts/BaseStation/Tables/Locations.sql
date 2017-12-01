IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Locations')
BEGIN
    CREATE TABLE [BaseStation].[Locations]
    (
        [LocationID]    BIGINT IDENTITY
       ,[LocationName]  NVARCHAR(80) NOT NULL
       ,[Latitude]      REAL NOT NULL
       ,[Longitude]     REAL NOT NULL
       ,[Altitude]      REAL NOT NULL

       ,CONSTRAINT [PK_Locations] PRIMARY KEY ([LocationID])
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM [BaseStation].[Locations])
BEGIN
    INSERT INTO [BaseStation].[Locations] (
        [LocationName]
       ,[Latitude]
       ,[Longitude]
       ,[Altitude]
    ) VALUES (
        'Home'
       ,51.4
       ,-0.6
       ,25.0
    );
END;
GO
