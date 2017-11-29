IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_GetAll')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_GetAll] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_GetAll]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Locations];
END;
GO

