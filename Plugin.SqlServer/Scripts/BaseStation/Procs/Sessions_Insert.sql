IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Insert]
    @LocationID INT
   ,@StartTime  DATETIME
   ,@EndTime    DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[Sessions] (
         [LocationID]
        ,[StartTime]
        ,[EndTime]
    ) VALUES (
         @LocationID
        ,@StartTime
        ,@EndTime
    );

    SELECT SCOPE_IDENTITY() AS [SessionID];
END;
GO
