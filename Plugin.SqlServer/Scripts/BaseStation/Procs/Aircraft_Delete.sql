IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Delete]
    @AircraftID INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Aircraft]
    WHERE  [AircraftID] = @AircraftID;
END;
GO
