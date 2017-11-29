IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_GetByCodes')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_GetByCodes] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_GetByCodes]
    @Codes AS [VRS].[Icao24] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT    [aircraft].*
    FROM      @Codes                   AS [code]
    JOIN      [BaseStation].[Aircraft] AS [aircraft] ON [code].[ModeS] = [aircraft].[ModeS];
END;
GO
