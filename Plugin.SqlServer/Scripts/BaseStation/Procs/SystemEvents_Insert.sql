IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'SystemEvents_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[SystemEvents_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[SystemEvents_Insert]
    @TimeStamp DATETIME2
   ,@App       NVARCHAR(15)
   ,@Msg       NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[SystemEvents] (
         [TimeStamp]
        ,[App]
        ,[Msg]
    ) VALUES (
         @TimeStamp
        ,@App
        ,@Msg
    );

    SELECT SCOPE_IDENTITY() AS [SystemEventsID];
END;
GO
