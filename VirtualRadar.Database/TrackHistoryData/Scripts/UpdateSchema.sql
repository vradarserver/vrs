--
-- DatabaseVersion
--
CREATE TABLE IF NOT EXISTS [DatabaseVersion]
(
    [Version] INTEGER NOT NULL
);


--
-- TrackHistory
--
CREATE TABLE IF NOT EXISTS [TrackHistory]
(
    [TrackHistoryID]    INTEGER PRIMARY KEY AUTOINCREMENT
   ,[Icao]              VARCHAR(6) NOT NULL COLLATE NOCASE
   ,[IsPreserved]       BIT NOT NULL
   ,[CreatedUtc]        DATETIME NOT NULL
   ,[UpdatedUtc]        DATETIME NOT NULL
);

CREATE INDEX IF NOT EXISTS [IX_TrackHistory_Icao] ON [TrackHistory] ([Icao]);


--
-- AltitudeType
--
CREATE TABLE IF NOT EXISTS [AltitudeType]
(
    [AltitudeTypeID]    INTEGER PRIMARY KEY
   ,[Description]       VARCHAR(80) NOT NULL
);
INSERT INTO [AltitudeType] ([AltitudeTypeID], [Description]) VALUES (0, 'Barometric') WHERE NOT EXISTS (SELECT 1 FROM [AltitudeType] WHERE [AltitudeTypeID] = 0);
INSERT INTO [AltitudeType] ([AltitudeTypeID], [Description]) VALUES (1, 'Geometric')  WHERE NOT EXISTS (SELECT 1 FROM [AltitudeType] WHERE [AltitudeTypeID] = 1);


--
-- SpeedType
--
CREATE TABLE IF NOT EXISTS [SpeedType]
(
    [SpeedTypeID]   INTEGER PRIMARY KEY
   ,[Description]   VARCHAR(80) NOT NULL
);
INSERT INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (0, 'Ground Speed')           WHERE NOT EXISTS (SELECT 1 FROM [SpeedType] WHERE [SpeedTypeID] = 0);
INSERT INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (1, 'Ground Speed Reversing') WHERE NOT EXISTS (SELECT 1 FROM [SpeedType] WHERE [SpeedTypeID] = 1);
INSERT INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (2, 'Indicated Air Speed')    WHERE NOT EXISTS (SELECT 1 FROM [SpeedType] WHERE [SpeedTypeID] = 2);
INSERT INTO [SpeedType] ([SpeedTypeID], [Description]) VALUES (3, 'True Air Speed')         WHERE NOT EXISTS (SELECT 1 FROM [SpeedType] WHERE [SpeedTypeID] = 3);


--
-- TrackHistoryState
--
CREATE TABLE IF NOT EXISTS [TrackHistoryState]
(
    [TrackHistoryStateID]   INTEGER PRIMARY KEY AUTOINCREMENT
   ,[TrackHistoryID]        INTEGER NOT NULL CONSTRAINT [FK_TrackHistoryState_TrackHistory] FOREIGN KEY REFERENCES [TrackHistory] ([TrackHistoryID])
   ,[TimestampUtc]          DATETIME NOT NULL
   ,[SignalLevel]           INTEGER NULL
   ,[Callsign]              VARCHAR(8) NULL
   ,[IsCallsignSuspect]     BIT NULL
   ,[Latitude]              REAL NULL
   ,[Longitude]             REAL NULL
   ,[IsMlat]                BIT NULL
   ,[IsTisb]                BIT NULL
   ,[AltitudeFeet]          INTEGER NULL
   ,[AltitudeTypeID]        INTEGER NULL CONSTRAINT [FK_TrackHistoryState_AltitudeType] FOREIGN KEY REFERENCES [AltitudeType] ([AltitudeTypeID])
   ,[TargetAltitudeFeet]    INTEGER NULL
   ,[AirPressureInHg]       REAL NULL
   ,[GroundSpeedKnots]      REAL NULL
   ,[SpeedTypeID]           INTEGER NULL CONSTRAINT [FK_TrackHistoryState_SpeedType] FOREIGN KEY REFERENCES [SpeedType] ([SpeedTypeID])
   ,[TrackDegrees]          REAL NULL
   ,[TargetTrack]           REAL NULL
   ,[TrackIsHeading]        BIT NULL
   ,[VerticalRateFeetMin]   INTEGER NULL
   ,[VerticalRateTypeID]    INTEGER NULL CONSTRAINT [FK_TrackHistoryState_VerticalRateType] FOREIGN KEY REFERENCES [AltitudeType] ([AltitudeTypeID])
   ,[SquawkOctal]           INTEGER NULL
   ,[IdentActive]           BIT NULL
);

CREATE INDEX IF NOT EXISTS [IX_TrackHistoryState_TrackHistory] ON [TrackHistoryState] ([TrackHistoryID], [TimestampUtc]);
