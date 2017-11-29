IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_MarkManyMissing')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_MarkManyMissing] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_MarkManyMissing]
    @Codes    AS [VRS].[Icao24] READONLY
   ,@LocalNow AS DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @missingMarker AS NVARCHAR(20) = 'Missing';

    -- Update existing aircraft but only if they have no details
    UPDATE [aircraft]
    SET    [LastModified] = @LocalNow
          ,[UserString1] = @missingMarker
    FROM   [BaseStation].[Aircraft] AS [aircraft]
    JOIN   @Codes                   AS [code]     ON [aircraft].[ModeS] = [code].[ModeS]
    WHERE  ISNULL([aircraft].[Registration], '') = ''
    AND    ISNULL([aircraft].[Manufacturer], '') = ''
    AND    ISNULL([aircraft].[Type], '') = ''
    AND    ISNULL([aircraft].[RegisteredOwners], '') = '';

    -- Insert stubs for aircraft that don't exist
    INSERT INTO [BaseStation].[Aircraft] (
        [ModeS]
       ,[FirstCreated]
       ,[LastModified]
       ,[UserString1]
    )
    SELECT    [code].[ModeS]
             ,@LocalNow
             ,@LocalNow
             ,@missingMarker
    FROM      @Codes                   AS [code]
    LEFT JOIN [BaseStation].[Aircraft] AS [aircraft] ON [code].[ModeS] = [aircraft].[ModeS]
    WHERE     [aircraft].[AircraftID] IS NULL;
END;
GO
