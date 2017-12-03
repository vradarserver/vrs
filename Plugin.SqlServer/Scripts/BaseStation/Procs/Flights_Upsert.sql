IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_Upsert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_Upsert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_Upsert]
    @BulkFlights AS [BaseStation].[BaseStationFlightUpsert] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @action AS TABLE (
        [FlightID]   INTEGER PRIMARY KEY
       ,[Action]     VARCHAR(7)             -- 'Created' or 'Updated'
    );

    INSERT INTO [BaseStation].[Flights] (
         [SessionID]
        ,[AircraftID]
        ,[StartTime]
        ,[EndTime]
        ,[Callsign]
        ,[NumPosMsgRec]
        ,[NumADSBMsgRec]
        ,[NumModeSMsgRec]
        ,[NumIDMsgRec]
        ,[NumSurPosMsgRec]
        ,[NumAirPosMsgRec]
        ,[NumAirVelMsgRec]
        ,[NumSurAltMsgRec]
        ,[NumSurIDMsgRec]
        ,[NumAirToAirMsgRec]
        ,[NumAirCallRepMsgRec]
        ,[FirstIsOnGround]
        ,[LastIsOnGround]
        ,[FirstLat]
        ,[LastLat]
        ,[FirstLon]
        ,[LastLon]
        ,[FirstGroundSpeed]
        ,[LastGroundSpeed]
        ,[FirstAltitude]
        ,[LastAltitude]
        ,[FirstVerticalRate]
        ,[LastVerticalRate]
        ,[FirstTrack]
        ,[LastTrack]
        ,[FirstSquawk]
        ,[LastSquawk]
        ,[HadAlert]
        ,[HadEmergency]
        ,[HadSPI]
        ,[UserNotes]
    )
    OUTPUT    INSERTED.[FlightID]
             ,'Created'
    INTO      @action
    SELECT    [bulk].[SessionID]
             ,[bulk].[AircraftID]
             ,[bulk].[StartTime]
             ,[bulk].[EndTime]
             ,[bulk].[Callsign]
             ,[bulk].[NumPosMsgRec]
             ,[bulk].[NumADSBMsgRec]
             ,[bulk].[NumModeSMsgRec]
             ,[bulk].[NumIDMsgRec]
             ,[bulk].[NumSurPosMsgRec]
             ,[bulk].[NumAirPosMsgRec]
             ,[bulk].[NumAirVelMsgRec]
             ,[bulk].[NumSurAltMsgRec]
             ,[bulk].[NumSurIDMsgRec]
             ,[bulk].[NumAirToAirMsgRec]
             ,[bulk].[NumAirCallRepMsgRec]
             ,[bulk].[FirstIsOnGround]
             ,[bulk].[LastIsOnGround]
             ,[bulk].[FirstLat]
             ,[bulk].[LastLat]
             ,[bulk].[FirstLon]
             ,[bulk].[LastLon]
             ,[bulk].[FirstGroundSpeed]
             ,[bulk].[LastGroundSpeed]
             ,[bulk].[FirstAltitude]
             ,[bulk].[LastAltitude]
             ,[bulk].[FirstVerticalRate]
             ,[bulk].[LastVerticalRate]
             ,[bulk].[FirstTrack]
             ,[bulk].[LastTrack]
             ,[bulk].[FirstSquawk]
             ,[bulk].[LastSquawk]
             ,[bulk].[HadAlert]
             ,[bulk].[HadEmergency]
             ,[bulk].[HadSPI]
             ,[bulk].[UserNotes]
    FROM      @BulkFlights            AS [bulk]
    LEFT JOIN [BaseStation].[Flights] AS [flight] ON [flight].[AircraftID] = [bulk].[AircraftID] AND [flight].[StartTime] = [bulk].[StartTime]
    WHERE     [flight].[FlightID] IS NULL;

    UPDATE [flight]
    SET    [SessionID]              = [bulk].[SessionID]
          ,[EndTime]                = [bulk].[EndTime]
          ,[Callsign]               = [bulk].[Callsign]
          ,[NumPosMsgRec]           = [bulk].[NumPosMsgRec]
          ,[NumADSBMsgRec]          = [bulk].[NumADSBMsgRec]
          ,[NumModeSMsgRec]         = [bulk].[NumModeSMsgRec]
          ,[NumIDMsgRec]            = [bulk].[NumIDMsgRec]
          ,[NumSurPosMsgRec]        = [bulk].[NumSurPosMsgRec]
          ,[NumAirPosMsgRec]        = [bulk].[NumAirPosMsgRec]
          ,[NumAirVelMsgRec]        = [bulk].[NumAirVelMsgRec]
          ,[NumSurAltMsgRec]        = [bulk].[NumSurAltMsgRec]
          ,[NumSurIDMsgRec]         = [bulk].[NumSurIDMsgRec]
          ,[NumAirToAirMsgRec]      = [bulk].[NumAirToAirMsgRec]
          ,[NumAirCallRepMsgRec]    = [bulk].[NumAirCallRepMsgRec]
          ,[FirstIsOnGround]        = [bulk].[FirstIsOnGround]
          ,[LastIsOnGround]         = [bulk].[LastIsOnGround]
          ,[FirstLat]               = [bulk].[FirstLat]
          ,[LastLat]                = [bulk].[LastLat]
          ,[FirstLon]               = [bulk].[FirstLon]
          ,[LastLon]                = [bulk].[LastLon]
          ,[FirstGroundSpeed]       = [bulk].[FirstGroundSpeed]
          ,[LastGroundSpeed]        = [bulk].[LastGroundSpeed]
          ,[FirstAltitude]          = [bulk].[FirstAltitude]
          ,[LastAltitude]           = [bulk].[LastAltitude]
          ,[FirstVerticalRate]      = [bulk].[FirstVerticalRate]
          ,[LastVerticalRate]       = [bulk].[LastVerticalRate]
          ,[FirstTrack]             = [bulk].[FirstTrack]
          ,[LastTrack]              = [bulk].[LastTrack]
          ,[FirstSquawk]            = [bulk].[FirstSquawk]
          ,[LastSquawk]             = [bulk].[LastSquawk]
          ,[HadAlert]               = [bulk].[HadAlert]
          ,[HadEmergency]           = [bulk].[HadEmergency]
          ,[HadSPI]                 = [bulk].[HadSPI]
          ,[UserNotes]              = [bulk].[UserNotes]
    OUTPUT INSERTED.[FlightID]
          ,'Updated'
    INTO   @action
    FROM   @BulkFlights            AS [bulk]
    JOIN   [BaseStation].[Flights] AS [flight] ON [flight].[AircraftID] = [bulk].[AircraftID] AND [flight].[StartTime] = [bulk].[StartTime]
    WHERE  [flight].[SessionID] <>          [bulk].[SessionID]
    OR     [flight].[FirstIsOnGround] <>    [bulk].[FirstIsOnGround]
    OR     [flight].[LastIsOnGround] <>     [bulk].[LastIsOnGround]
    OR     [flight].[HadAlert] <>           [bulk].[HadAlert]
    OR     [flight].[HadEmergency] <>       [bulk].[HadEmergency]
    OR     [flight].[HadSPI] <>             [bulk].[HadSPI]
    OR     ISNULL(NULLIF([flight].[EndTime],                [bulk].[EndTime]),              NULLIF([bulk].[EndTime],                [flight].[EndTime])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[Callsign],               [bulk].[Callsign]),             NULLIF([bulk].[Callsign],               [flight].[Callsign])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumPosMsgRec],           [bulk].[NumPosMsgRec]),         NULLIF([bulk].[NumPosMsgRec],           [flight].[NumPosMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumADSBMsgRec],          [bulk].[NumADSBMsgRec]),        NULLIF([bulk].[NumADSBMsgRec],          [flight].[NumADSBMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumModeSMsgRec],         [bulk].[NumModeSMsgRec]),       NULLIF([bulk].[NumModeSMsgRec],         [flight].[NumModeSMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumIDMsgRec],            [bulk].[NumIDMsgRec]),          NULLIF([bulk].[NumIDMsgRec],            [flight].[NumIDMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumSurPosMsgRec],        [bulk].[NumSurPosMsgRec]),      NULLIF([bulk].[NumSurPosMsgRec],        [flight].[NumSurPosMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumAirPosMsgRec],        [bulk].[NumAirPosMsgRec]),      NULLIF([bulk].[NumAirPosMsgRec],        [flight].[NumAirPosMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumAirVelMsgRec],        [bulk].[NumAirVelMsgRec]),      NULLIF([bulk].[NumAirVelMsgRec],        [flight].[NumAirVelMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumSurAltMsgRec],        [bulk].[NumSurAltMsgRec]),      NULLIF([bulk].[NumSurAltMsgRec],        [flight].[NumSurAltMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumSurIDMsgRec],         [bulk].[NumSurIDMsgRec]),       NULLIF([bulk].[NumSurIDMsgRec],         [flight].[NumSurIDMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumAirToAirMsgRec],      [bulk].[NumAirToAirMsgRec]),    NULLIF([bulk].[NumAirToAirMsgRec],      [flight].[NumAirToAirMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[NumAirCallRepMsgRec],    [bulk].[NumAirCallRepMsgRec]),  NULLIF([bulk].[NumAirCallRepMsgRec],    [flight].[NumAirCallRepMsgRec])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[FirstLat],               [bulk].[FirstLat]),             NULLIF([bulk].[FirstLat],               [flight].[FirstLat])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[LastLat],                [bulk].[LastLat]),              NULLIF([bulk].[LastLat],                [flight].[LastLat])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[FirstLon],               [bulk].[FirstLon]),             NULLIF([bulk].[FirstLon],               [flight].[FirstLon])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[LastLon],                [bulk].[LastLon]),              NULLIF([bulk].[LastLon],                [flight].[LastLon])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[FirstGroundSpeed],       [bulk].[FirstGroundSpeed]),     NULLIF([bulk].[FirstGroundSpeed],       [flight].[FirstGroundSpeed])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[LastGroundSpeed],        [bulk].[LastGroundSpeed]),      NULLIF([bulk].[LastGroundSpeed],        [flight].[LastGroundSpeed])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[FirstAltitude],          [bulk].[FirstAltitude]),        NULLIF([bulk].[FirstAltitude],          [flight].[FirstAltitude])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[LastAltitude],           [bulk].[LastAltitude]),         NULLIF([bulk].[LastAltitude],           [flight].[LastAltitude])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[FirstVerticalRate],      [bulk].[FirstVerticalRate]),    NULLIF([bulk].[FirstVerticalRate],      [flight].[FirstVerticalRate])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[LastVerticalRate],       [bulk].[LastVerticalRate]),     NULLIF([bulk].[LastVerticalRate],       [flight].[LastVerticalRate])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[FirstTrack],             [bulk].[FirstTrack]),           NULLIF([bulk].[FirstTrack],             [flight].[FirstTrack])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[LastTrack],              [bulk].[LastTrack]),            NULLIF([bulk].[LastTrack],              [flight].[LastTrack])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[FirstSquawk],            [bulk].[FirstSquawk]),          NULLIF([bulk].[FirstSquawk],            [flight].[FirstSquawk])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[LastSquawk],             [bulk].[LastSquawk]),           NULLIF([bulk].[LastSquawk],             [flight].[LastSquawk])) IS NOT NULL
    OR     ISNULL(NULLIF([flight].[UserNotes],              [bulk].[UserNotes]),            NULLIF([bulk].[UserNotes],              [flight].[UserNotes])) IS NOT NULL;

    SELECT [flight].*
    FROM   @BulkFlights            AS [bulk]
    JOIN   [BaseStation].[Flights] AS [flight] ON [flight].[AircraftID] = [bulk].[AircraftID] AND [flight].[StartTime] = [bulk].[StartTime];

    SELECT *
    FROM   @action;
END;
GO
