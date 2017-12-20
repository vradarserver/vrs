IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Aircraft')
BEGIN
    CREATE TABLE [BaseStation].[Aircraft]
    (
        [AircraftID]        INTEGER IDENTITY
       ,[FirstCreated]      DATETIME2 NOT NULL
       ,[LastModified]      DATETIME2 NOT NULL
       ,[ModeS]             NVARCHAR(6) NOT NULL
       ,[ModeSCountry]      NVARCHAR(80)
       ,[Country]           NVARCHAR(80)
       ,[Registration]      NVARCHAR(20)
       ,[CurrentRegDate]    NVARCHAR(20)
       ,[PreviousID]        NVARCHAR(20)
       ,[FirstRegDate]      NVARCHAR(20)
       ,[Status]            NVARCHAR(10)
       ,[DeRegDate]         NVARCHAR(10)
       ,[Manufacturer]      NVARCHAR(80)
       ,[ICAOTypeCode]      NVARCHAR(10)
       ,[Type]              NVARCHAR(80)
       ,[SerialNo]          NVARCHAR(30)
       ,[PopularName]       NVARCHAR(80)
       ,[GenericName]       NVARCHAR(80)
       ,[AircraftClass]     NVARCHAR(80)
       ,[Engines]           NVARCHAR(40)
       ,[OwnershipStatus]   NVARCHAR(20)
       ,[RegisteredOwners]  NVARCHAR(100)
       ,[MTOW]              NVARCHAR(20)
       ,[TotalHours]        NVARCHAR(20)
       ,[YearBuilt]         NVARCHAR(4)
       ,[CofACategory]      NVARCHAR(30)
       ,[CofAExpiry]        NVARCHAR(20)
       ,[UserNotes]         NVARCHAR(300)
       ,[Interested]        BIT NOT NULL CONSTRAINT [DF_Aircraft_Interested] DEFAULT 0
       ,[UserTag]           NVARCHAR(80)
       ,[InfoURL]           NVARCHAR(150)
       ,[PictureURL1]       NVARCHAR(150)
       ,[PictureURL2]       NVARCHAR(150)
       ,[PictureURL3]       NVARCHAR(150)
       ,[UserBool1]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool1] DEFAULT 0
       ,[UserBool2]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool2] DEFAULT 0
       ,[UserBool3]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool3] DEFAULT 0
       ,[UserBool4]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool4] DEFAULT 0
       ,[UserBool5]         BIT NOT NULL CONSTRAINT [DF_Aircraft_UserBool5] DEFAULT 0
       ,[UserString1]       NVARCHAR(40)
       ,[UserString2]       NVARCHAR(40)
       ,[UserString3]       NVARCHAR(40)
       ,[UserString4]       NVARCHAR(40)
       ,[UserString5]       NVARCHAR(40)
       ,[UserInt1]          BIGINT CONSTRAINT [DF_Aircraft_UserInt1] DEFAULT 0
       ,[UserInt2]          BIGINT CONSTRAINT [DF_Aircraft_UserInt2] DEFAULT 0
       ,[UserInt3]          BIGINT CONSTRAINT [DF_Aircraft_UserInt3] DEFAULT 0
       ,[UserInt4]          BIGINT CONSTRAINT [DF_Aircraft_UserInt4] DEFAULT 0
       ,[UserInt5]          BIGINT CONSTRAINT [DF_Aircraft_UserInt5] DEFAULT 0
       ,[OperatorFlagCode]  NVARCHAR(20)

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
