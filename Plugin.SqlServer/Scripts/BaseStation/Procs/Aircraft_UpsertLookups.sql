IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_UpsertLookups')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_UpsertLookups] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_UpsertLookups]
    @Lookups [BaseStation].[BaseStationAircraftUpsertLookup] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @action AS TABLE (
        [AircraftID] INTEGER PRIMARY KEY
       ,[Action]     VARCHAR(7)             -- 'Created' or 'Updated'
    );

    INSERT INTO [BaseStation].[Aircraft] (
        [ModeS]
       ,[FirstCreated]
       ,[LastModified]
       ,[Registration]
       ,[Country]
       ,[ModeSCountry]
       ,[Manufacturer]
       ,[Type]
       ,[ICAOTypeCode]
       ,[RegisteredOwners]
       ,[OperatorFlagCode]
       ,[SerialNo]
       ,[YearBuilt]
    )
    OUTPUT    INSERTED.[AircraftID]
             ,'Created'
    INTO      @action
    SELECT    [lookup].[ModeS]
             ,[lookup].[LastModified]
             ,[lookup].[LastModified]
             ,[lookup].[Registration]
             ,[lookup].[Country]
             ,[lookup].[ModeSCountry]
             ,[lookup].[Manufacturer]
             ,[lookup].[Type]
             ,[lookup].[ICAOTypeCode]
             ,[lookup].[RegisteredOwners]
             ,[lookup].[OperatorFlagCode]
             ,[lookup].[SerialNo]
             ,[lookup].[YearBuilt]
    FROM      @Lookups                 AS [lookup]
    LEFT JOIN [BaseStation].[Aircraft] AS [aircraft] ON [lookup].[ModeS] = [aircraft].[ModeS]
    WHERE     [aircraft].[AircraftID] IS NULL;

    UPDATE [aircraft]
    SET    [LastModified] =     [lookup].[LastModified]
          ,[Registration] =     [lookup].[Registration]
          ,[Country] =          [lookup].[Country]
          ,[ModeSCountry] =     [lookup].[ModeSCountry]
          ,[Manufacturer] =     [lookup].[Manufacturer]
          ,[Type] =             [lookup].[Type]
          ,[ICAOTypeCode] =     [lookup].[ICAOTypeCode]
          ,[RegisteredOwners] = [lookup].[RegisteredOwners]
          ,[OperatorFlagCode] = [lookup].[OperatorFlagCode]
          ,[SerialNo] =         [lookup].[SerialNo]
          ,[YearBuilt] =        [lookup].[YearBuilt]
          ,[UserString1] =      CASE WHEN [aircraft].[UserString1] = 'Missing' THEN NULL ELSE [aircraft].[UserString1] END
    OUTPUT INSERTED.[AircraftID]
          ,'Updated'
    INTO   @action
    FROM   @Lookups                 AS [lookup]
    JOIN   [BaseStation].[Aircraft] AS [aircraft] ON [lookup].[ModeS] = [aircraft].[ModeS]
    WHERE  [aircraft].[LastModified] <> [lookup].[LastModified]
    OR     ISNULL([aircraft].[Registration], '') <>     ISNULL([lookup].[Registration], '')
    OR     ISNULL([aircraft].[Country], '') <>          ISNULL([lookup].[Country], '')
    OR     ISNULL([aircraft].[ModeSCountry], '') <>     ISNULL([lookup].[ModeSCountry], '')
    OR     ISNULL([aircraft].[Manufacturer], '') <>     ISNULL([lookup].[Manufacturer], '')
    OR     ISNULL([aircraft].[Type], '') <>             ISNULL([lookup].[Type], '')
    OR     ISNULL([aircraft].[ICAOTypeCode], '') <>     ISNULL([lookup].[ICAOTypeCode], '')
    OR     ISNULL([aircraft].[RegisteredOwners], '') <> ISNULL([lookup].[RegisteredOwners], '')
    OR     ISNULL([aircraft].[OperatorFlagCode], '') <> ISNULL([lookup].[OperatorFlagCode], '')
    OR     ISNULL([aircraft].[SerialNo], '') <>         ISNULL([lookup].[SerialNo], '')
    OR     ISNULL([aircraft].[YearBuilt], '') <>        ISNULL([lookup].[YearBuilt], '')
    OR     [aircraft].[UserString1] = 'Missing';

    SELECT [aircraft].*
    FROM   [BaseStation].[Aircraft] AS [aircraft]
    JOIN   @Lookups                 AS [lookup]   ON [aircraft].[ModeS] = [lookup].[ModeS];

    SELECT *
    FROM   @action;
END;
GO
