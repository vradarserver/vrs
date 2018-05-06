IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_MarkManyMissing')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_MarkManyMissing] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_MarkManyMissing]
    @Codes    AS [VRS].[Icao24] READONLY
   ,@LocalNow AS DATETIME2
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @missingMarker AS NVARCHAR(20) = 'Missing';

    -- Insert stubs for aircraft that don't exist
    -- All aircraft inserted here will later be updated, which is redundant... but at the same
    -- time if we did the update first we could miss out an aircraft that was inserted between
    -- the update and the later insert. This should guarantee that no aircraft will be missed.
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

    -- Update just the last modified time if the aircraft has details
    UPDATE [aircraft]
    SET    [LastModified] = @LocalNow
    FROM   [BaseStation].[Aircraft] AS [aircraft]
    JOIN   @Codes                   AS [code]     ON [aircraft].[ModeS] = [code].[ModeS]
    WHERE  ISNULL([aircraft].[Registration], '') <> ''
    OR     ISNULL([aircraft].[Manufacturer], '') <> ''
    OR     ISNULL([aircraft].[Type], '') <> ''
    OR     ISNULL([aircraft].[RegisteredOwners], '') <> '';
END;
GO
