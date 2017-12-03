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
    OR     ISNULL(NULLIF([aircraft].[Registration],     [lookup].[Registration]),       NULLIF([lookup].[Registration],     [aircraft].[Registration])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Country],          [lookup].[Country]),            NULLIF([lookup].[Country],          [aircraft].[Country])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[ModeSCountry],     [lookup].[ModeSCountry]),       NULLIF([lookup].[ModeSCountry],     [aircraft].[ModeSCountry])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Manufacturer],     [lookup].[Manufacturer]),       NULLIF([lookup].[Manufacturer],     [aircraft].[Manufacturer])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Type],             [lookup].[Type]),               NULLIF([lookup].[Type],             [aircraft].[Type])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[ICAOTypeCode],     [lookup].[ICAOTypeCode]),       NULLIF([lookup].[ICAOTypeCode],     [aircraft].[ICAOTypeCode])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[RegisteredOwners], [lookup].[RegisteredOwners]),   NULLIF([lookup].[RegisteredOwners], [aircraft].[RegisteredOwners])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[OperatorFlagCode], [lookup].[OperatorFlagCode]),   NULLIF([lookup].[OperatorFlagCode], [aircraft].[OperatorFlagCode])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[SerialNo],         [lookup].[SerialNo]),           NULLIF([lookup].[SerialNo],         [aircraft].[SerialNo])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[YearBuilt],        [lookup].[YearBuilt]),          NULLIF([lookup].[YearBuilt],        [aircraft].[YearBuilt])) IS NOT NULL
    OR     [aircraft].[UserString1] = 'Missing';

    SELECT [aircraft].*
    FROM   [BaseStation].[Aircraft] AS [aircraft]
    JOIN   @Lookups                 AS [lookup]   ON [aircraft].[ModeS] = [lookup].[ModeS];

    SELECT *
    FROM   @action;
END;
GO
