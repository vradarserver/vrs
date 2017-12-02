IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Sessions_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Sessions_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Sessions_Delete]
    @SessionID INTEGER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Sessions]
    WHERE  [SessionID] = @SessionID;
END;
GO
