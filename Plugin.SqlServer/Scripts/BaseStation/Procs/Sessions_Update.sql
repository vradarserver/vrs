IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Update]
    @SessionID  BIGINT
   ,@LocationID BIGINT
   ,@StartTime  DATETIME = NULL
   ,@EndTime    DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Sessions]
    SET    [LocationID]    = @LocationID
          ,[StartTime]     = ISNULL(@StartTime, [StartTime])
          ,[EndTime]       = @EndTime
    WHERE [SessionID] = @SessionID;
END;
GO
