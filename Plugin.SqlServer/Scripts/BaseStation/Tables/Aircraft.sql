IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Aircraft')
BEGIN
    CREATE TABLE [BaseStation].[Aircraft]
    (
        [AircraftID]        BIGINT IDENTITY
       ,[FirstCreated]      DATETIME NOT NULL
       ,[LastModified]      DATETIME NOT NULL
       ,[ModeS]             VARCHAR(6) NOT NULL
       ,[ModeSCountry]      VARCHAR(24)
       ,[Country]           VARCHAR(24)
       ,[Registration]      VARCHAR(20)
       ,[CurrentRegDate]    VARCHAR(10)
       ,[PreviousID]        VARCHAR(10)
       ,[FirstRegDate]      VARCHAR(10)
       ,[Status]            VARCHAR(10)
       ,[DeRegDate]         VARCHAR(10)
       ,[Manufacturer]      VARCHAR(60)
       ,[ICAOTypeCode]      VARCHAR(10)
       ,[Type]              VARCHAR(40)
       ,[SerialNo]          VARCHAR(30)
       ,[PopularName]       VARCHAR(20)
       ,[GenericName]       VARCHAR(20)
       ,[AircraftClass]     VARCHAR(20)
       ,[Engines]           VARCHAR(40)
       ,[OwnershipStatus]   VARCHAR(10)
       ,[RegisteredOwners]  VARCHAR(100)
       ,[MTOW]              VARCHAR(10)
       ,[TotalHours]        VARCHAR(20)
       ,[YearBuilt]         VARCHAR(4)
       ,[CofACategory]      VARCHAR(30)
       ,[CofAExpiry]        VARCHAR(10)
       ,[UserNotes]         VARCHAR(300)
       ,[Interested]        BIT NOT NULL CONSTRAINT [DF_Aircraft_Interested] DEFAULT 0
       ,[UserTag]           VARCHAR(5)
       ,[InfoURL]           VARCHAR(150)
       ,[PictureURL1]       VARCHAR(150)
       ,[PictureURL2]       VARCHAR(150)
       ,[PictureURL3]       VARCHAR(150)
       ,[UserBool1]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool1] DEFAULT 0
       ,[UserBool2]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool2] DEFAULT 0
       ,[UserBool3]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool3] DEFAULT 0
       ,[UserBool4]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool4] DEFAULT 0
       ,[UserBool5]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool5] DEFAULT 0
       ,[UserString1]       VARCHAR(20)
       ,[UserString2]       VARCHAR(20)
       ,[UserString3]       VARCHAR(20)
       ,[UserString4]       VARCHAR(20)
       ,[UserString5]       VARCHAR(20)
       ,[UserInt1]          BIGINT CONSTRAINT [DF_Aircraft_UserInt1] DEFAULT 0
       ,[UserInt2]          BIGINT CONSTRAINT [DF_Aircraft_UserInt2] DEFAULT 0
       ,[UserInt3]          BIGINT CONSTRAINT [DF_Aircraft_UserInt3] DEFAULT 0
       ,[UserInt4]          BIGINT CONSTRAINT [DF_Aircraft_UserInt4] DEFAULT 0
       ,[UserInt5]          BIGINT CONSTRAINT [DF_Aircraft_UserInt5] DEFAULT 0
       ,[OperatorFlagCode]  VARCHAR(20)

       ,CONSTRAINT [PK_Aircraft] PRIMARY KEY ([AircraftID])
    );

    CREATE UNIQUE INDEX [IX_Aircraft_ModeS]     ON [BaseStation].[Aircraft] ([ModeS]);

    CREATE INDEX [IX_Aircraft_AircraftClass]    ON [BaseStation].[Aircraft] ([AircraftClass]);
    CREATE INDEX [IX_Aircraft_Country]          ON [BaseStation].[Aircraft] ([Country]);
    CREATE INDEX [IX_Aircraft_GenericName]      ON [BaseStation].[Aircraft] ([GenericName]);
    CREATE INDEX [IX_Aircraft_ICAOTypeCode]     ON [BaseStation].[Aircraft] ([ICAOTypeCode]);
    CREATE INDEX [IX_Aircraft_Interested]       ON [BaseStation].[Aircraft] ([Interested]);
    CREATE INDEX [IX_Aircraft_Manufacturer]     ON [BaseStation].[Aircraft] ([Manufacturer]);
    CREATE INDEX [IX_Aircraft_ModeSCountry]     ON [BaseStation].[Aircraft] ([ModeSCountry]);
    CREATE INDEX [IX_Aircraft_PopularName]      ON [BaseStation].[Aircraft] ([PopularName]);
    CREATE INDEX [IX_Aircraft_RegisteredOwners] ON [BaseStation].[Aircraft] ([RegisteredOwners]);
    CREATE INDEX [IX_Aircraft_Registration]     ON [BaseStation].[Aircraft] ([Registration]);
    CREATE INDEX [IX_Aircraft_SerialNo]         ON [BaseStation].[Aircraft] ([SerialNo]);
    CREATE INDEX [IX_Aircraft_Type]             ON [BaseStation].[Aircraft] ([Type]);
    CREATE INDEX [IX_Aircraft_UserTag]          ON [BaseStation].[Aircraft] ([UserTag]);
    CREATE INDEX [IX_Aircraft_YearBuilt]        ON [BaseStation].[Aircraft] ([YearBuilt]);
END;
GO
