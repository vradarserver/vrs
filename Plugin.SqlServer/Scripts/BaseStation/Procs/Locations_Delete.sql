IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_Delete]
    @LocationID INTEGER
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Locations]
    WHERE  [LocationID] = @LocationID;
END;
GO
