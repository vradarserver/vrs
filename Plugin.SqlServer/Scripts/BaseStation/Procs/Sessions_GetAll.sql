IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [SessionID]
          ,ISNULL([LocationID], 0) AS [LocationID]
          ,[StartTime]
          ,[EndTime]
    FROM   [BaseStation].[Sessions];
END;
GO

