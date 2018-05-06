IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_Update]
    @FlightID            INT
   ,@SessionID           INT
   ,@AircraftID          INT
   ,@StartTime           DATETIME2 = NULL
   ,@EndTime             DATETIME2
   ,@Callsign            NVARCHAR(80)
   ,@NumPosMsgRec        INT
   ,@NumADSBMsgRec       INT
   ,@NumModeSMsgRec      INT
   ,@NumIDMsgRec         INT
   ,@NumSurPosMsgRec     INT
   ,@NumAirPosMsgRec     INT
   ,@NumAirVelMsgRec     INT
   ,@NumSurAltMsgRec     INT
   ,@NumSurIDMsgRec      INT
   ,@NumAirToAirMsgRec   INT
   ,@NumAirCallRepMsgRec INT
   ,@FirstIsOnGround     BIT
   ,@LastIsOnGround      BIT
   ,@FirstLat            REAL
   ,@LastLat             REAL
   ,@FirstLon            REAL
   ,@LastLon             REAL
   ,@FirstGroundSpeed    REAL
   ,@LastGroundSpeed     REAL
   ,@FirstAltitude       INT
   ,@LastAltitude        INT
   ,@FirstVerticalRate   INT
   ,@LastVerticalRate    INT
   ,@FirstTrack          REAL
   ,@LastTrack           REAL
   ,@FirstSquawk         INT
   ,@LastSquawk          INT
   ,@HadAlert            BIT
   ,@HadEmergency        BIT
   ,@HadSPI              BIT
   ,@UserNotes           NVARCHAR(3000)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Flights]
    SET    [SessionID]              = @SessionID
          ,[AircraftID]             = @AircraftID
          ,[StartTime]              = ISNULL(@StartTime, [StartTime])
          ,[EndTime]                = @EndTime
          ,[Callsign]               = @Callsign
          ,[NumPosMsgRec]           = @NumPosMsgRec
          ,[NumADSBMsgRec]          = @NumADSBMsgRec
          ,[NumModeSMsgRec]         = @NumModeSMsgRec
          ,[NumIDMsgRec]            = @NumIDMsgRec
          ,[NumSurPosMsgRec]        = @NumSurPosMsgRec
          ,[NumAirPosMsgRec]        = @NumAirPosMsgRec
          ,[NumAirVelMsgRec]        = @NumAirVelMsgRec
          ,[NumSurAltMsgRec]        = @NumSurAltMsgRec
          ,[NumSurIDMsgRec]         = @NumSurIDMsgRec
          ,[NumAirToAirMsgRec]      = @NumAirToAirMsgRec
          ,[NumAirCallRepMsgRec]    = @NumAirCallRepMsgRec
          ,[FirstIsOnGround]        = @FirstIsOnGround
          ,[LastIsOnGround]         = @LastIsOnGround
          ,[FirstLat]               = @FirstLat
          ,[LastLat]                = @LastLat
          ,[FirstLon]               = @FirstLon
          ,[LastLon]                = @LastLon
          ,[FirstGroundSpeed]       = @FirstGroundSpeed
          ,[LastGroundSpeed]        = @LastGroundSpeed
          ,[FirstAltitude]          = @FirstAltitude
          ,[LastAltitude]           = @LastAltitude
          ,[FirstVerticalRate]      = @FirstVerticalRate
          ,[LastVerticalRate]       = @LastVerticalRate
          ,[FirstTrack]             = @FirstTrack
          ,[LastTrack]              = @LastTrack
          ,[FirstSquawk]            = @FirstSquawk
          ,[LastSquawk]             = @LastSquawk
          ,[HadAlert]               = @HadAlert
          ,[HadEmergency]           = @HadEmergency
          ,[HadSPI]                 = @HadSPI
          ,[UserNotes]              = @UserNotes
    WHERE [FlightID] = @FlightID;
END;
GO
