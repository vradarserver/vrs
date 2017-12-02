IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_Insert]
    @SessionID           INTEGER
   ,@AircraftID          INTEGER
   ,@StartTime           DATETIME
   ,@EndTime             DATETIME = NULL
   ,@Callsign            NVARCHAR(20) = NULL
   ,@NumPosMsgRec        INT = NULL
   ,@NumADSBMsgRec       INT = NULL
   ,@NumModeSMsgRec      INT = NULL
   ,@NumIDMsgRec         INT = NULL
   ,@NumSurPosMsgRec     INT = NULL
   ,@NumAirPosMsgRec     INT = NULL
   ,@NumAirVelMsgRec     INT = NULL
   ,@NumSurAltMsgRec     INT = NULL
   ,@NumSurIDMsgRec      INT = NULL
   ,@NumAirToAirMsgRec   INT = NULL
   ,@NumAirCallRepMsgRec INT = NULL
   ,@FirstIsOnGround     BIT = 0
   ,@LastIsOnGround      BIT = 0
   ,@FirstLat            REAL = NULL
   ,@LastLat             REAL = NULL
   ,@FirstLon            REAL = NULL
   ,@LastLon             REAL = NULL
   ,@FirstGroundSpeed    REAL = NULL
   ,@LastGroundSpeed     REAL = NULL
   ,@FirstAltitude       INT = NULL
   ,@LastAltitude        INT = NULL
   ,@FirstVerticalRate   INT = NULL
   ,@LastVerticalRate    INT = NULL
   ,@FirstTrack          REAL = NULL
   ,@LastTrack           REAL = NULL
   ,@FirstSquawk         INT = NULL
   ,@LastSquawk          INT = NULL
   ,@HadAlert            BIT = 0
   ,@HadEmergency        BIT = 0
   ,@HadSPI              BIT = 0
   ,@UserNotes           NVARCHAR(300) = NULL
AS
BEGIN
    SET NOCOUNT ON;

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
    ) VALUES (
         @SessionID
        ,@AircraftID
        ,@StartTime
        ,@EndTime
        ,@Callsign
        ,@NumPosMsgRec
        ,@NumADSBMsgRec
        ,@NumModeSMsgRec
        ,@NumIDMsgRec
        ,@NumSurPosMsgRec
        ,@NumAirPosMsgRec
        ,@NumAirVelMsgRec
        ,@NumSurAltMsgRec
        ,@NumSurIDMsgRec
        ,@NumAirToAirMsgRec
        ,@NumAirCallRepMsgRec
        ,@FirstIsOnGround
        ,@LastIsOnGround
        ,@FirstLat
        ,@LastLat
        ,@FirstLon
        ,@LastLon
        ,@FirstGroundSpeed
        ,@LastGroundSpeed
        ,@FirstAltitude
        ,@LastAltitude
        ,@FirstVerticalRate
        ,@LastVerticalRate
        ,@FirstTrack
        ,@LastTrack
        ,@FirstSquawk
        ,@LastSquawk
        ,@HadAlert
        ,@HadEmergency
        ,@HadSPI
        ,@UserNotes
    );

    SELECT SCOPE_IDENTITY() AS [FlightID];
END;
GO
