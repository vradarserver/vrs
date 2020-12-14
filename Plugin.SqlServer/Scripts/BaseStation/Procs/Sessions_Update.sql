IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Update]
    @SessionID  INT
   ,@LocationID INT
   ,@StartTime  DATETIME2 = NULL
   ,@EndTime    DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Sessions]
    SET    [LocationID]    = CASE WHEN @LocationID = 0 THEN NULL ELSE @LocationID END
          ,[StartTime]     = ISNULL(@StartTime, [StartTime])
          ,[EndTime]       = @EndTime
    WHERE [SessionID] = @SessionID;
END;
GO
