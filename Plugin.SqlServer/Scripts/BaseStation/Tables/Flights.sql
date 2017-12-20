IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'BaseStation' AND TABLE_NAME = 'Flights')
BEGIN
    CREATE TABLE [BaseStation].[Flights]
    (
        [FlightID]              INTEGER IDENTITY
       ,[SessionID]             INTEGER NOT NULL CONSTRAINT [FK_Flights_Session]  FOREIGN KEY REFERENCES [BaseStation].[Sessions] ([SessionID]) ON DELETE CASCADE
       ,[AircraftID]            INTEGER NOT NULL CONSTRAINT [FK_Flights_Aircraft] FOREIGN KEY REFERENCES [BaseStation].[Aircraft] ([AircraftID]) ON DELETE CASCADE
       ,[StartTime]             DATETIME2 NOT NULL
       ,[EndTime]               DATETIME2
       ,[Callsign]              NVARCHAR(20)
       ,[NumPosMsgRec]          INTEGER
       ,[NumADSBMsgRec]         INTEGER
       ,[NumModeSMsgRec]        INTEGER
       ,[NumIDMsgRec]           INTEGER
       ,[NumSurPosMsgRec]       INTEGER
       ,[NumAirPosMsgRec]       INTEGER
       ,[NumAirVelMsgRec]       INTEGER
       ,[NumSurAltMsgRec]       INTEGER
       ,[NumSurIDMsgRec]        INTEGER
       ,[NumAirToAirMsgRec]     INTEGER
       ,[NumAirCallRepMsgRec]   INTEGER
       ,[FirstIsOnGround]       BIT NOT NULL CONSTRAINT [DF_Flights_FirstIsOnGround] DEFAULT 0
       ,[LastIsOnGround]        BIT NOT NULL CONSTRAINT [DF_Flights_LastIsOnGround] DEFAULT 0
       ,[FirstLat]              REAL
       ,[LastLat]               REAL
       ,[FirstLon]              REAL
       ,[LastLon]               REAL
       ,[FirstGroundSpeed]      REAL
       ,[LastGroundSpeed]       REAL
       ,[FirstAltitude]         INTEGER
       ,[LastAltitude]          INTEGER
       ,[FirstVerticalRate]     INTEGER
       ,[LastVerticalRate]      INTEGER
       ,[FirstTrack]            REAL
       ,[LastTrack]             REAL
       ,[FirstSquawk]           INTEGER
       ,[LastSquawk]            INTEGER
       ,[HadAlert]              BIT NOT NULL CONSTRAINT [DF_Flights_HadAlert] DEFAULT 0
       ,[HadEmergency]          BIT NOT NULL CONSTRAINT [DF_Flights_HadEmergency] DEFAULT 0
       ,[HadSPI]                BIT NOT NULL CONSTRAINT [DF_Flights_HadSPI] DEFAULT 0
       ,[UserNotes]             NVARCHAR(300)

       ,CONSTRAINT [PK_Flights] PRIMARY KEY ([FlightID])
    );

    CREATE INDEX [IX_Flights_AircraftID]    ON [BaseStation].[Flights] ([AircraftID]);
    CREATE INDEX [IX_Flights_Callsign]      ON [BaseStation].[Flights] ([Callsign]);
    CREATE INDEX [IX_Flights_EndTime]       ON [BaseStation].[Flights] ([EndTime]);
    CREATE INDEX [IX_Flights_SessionID]     ON [BaseStation].[Flights] ([SessionID]);
    CREATE INDEX [IX_Flights_StartTime]     ON [BaseStation].[Flights] ([StartTime]);
END;
GO
