IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_Upsert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_Upsert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_Upsert]
    @BulkAircraft [BaseStation].[BaseStationAircraftUpsert] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @action AS TABLE (
        [AircraftID] INTEGER PRIMARY KEY
       ,[Action]     VARCHAR(7)             -- 'Created' or 'Updated'
    );

    INSERT INTO [BaseStation].[Aircraft] (
         [FirstCreated]
        ,[LastModified]
        ,[ModeS]
        ,[ModeSCountry]
        ,[Country]
        ,[Registration]
        ,[CurrentRegDate]
        ,[PreviousID]
        ,[FirstRegDate]
        ,[Status]
        ,[DeRegDate]
        ,[Manufacturer]
        ,[ICAOTypeCode]
        ,[Type]
        ,[SerialNo]
        ,[PopularName]
        ,[GenericName]
        ,[AircraftClass]
        ,[Engines]
        ,[OwnershipStatus]
        ,[RegisteredOwners]
        ,[MTOW]
        ,[TotalHours]
        ,[YearBuilt]
        ,[CofACategory]
        ,[CofAExpiry]
        ,[UserNotes]
        ,[Interested]
        ,[UserTag]
        ,[InfoURL]
        ,[PictureURL1]
        ,[PictureURL2]
        ,[PictureURL3]
        ,[UserBool1]
        ,[UserBool2]
        ,[UserBool3]
        ,[UserBool4]
        ,[UserBool5]
        ,[UserString1]
        ,[UserString2]
        ,[UserString3]
        ,[UserString4]
        ,[UserString5]
        ,[UserInt1]
        ,[UserInt2]
        ,[UserInt3]
        ,[UserInt4]
        ,[UserInt5]
        ,[OperatorFlagCode]
    )
    OUTPUT    INSERTED.[AircraftID]
             ,'Created'
    INTO      @action
    SELECT    [bulk].[FirstCreated]
             ,[bulk].[LastModified]
             ,[bulk].[ModeS]
             ,[bulk].[ModeSCountry]
             ,[bulk].[Country]
             ,[bulk].[Registration]
             ,[bulk].[CurrentRegDate]
             ,[bulk].[PreviousID]
             ,[bulk].[FirstRegDate]
             ,[bulk].[Status]
             ,[bulk].[DeRegDate]
             ,[bulk].[Manufacturer]
             ,[bulk].[ICAOTypeCode]
             ,[bulk].[Type]
             ,[bulk].[SerialNo]
             ,[bulk].[PopularName]
             ,[bulk].[GenericName]
             ,[bulk].[AircraftClass]
             ,[bulk].[Engines]
             ,[bulk].[OwnershipStatus]
             ,[bulk].[RegisteredOwners]
             ,[bulk].[MTOW]
             ,[bulk].[TotalHours]
             ,[bulk].[YearBuilt]
             ,[bulk].[CofACategory]
             ,[bulk].[CofAExpiry]
             ,[bulk].[UserNotes]
             ,[bulk].[Interested]
             ,[bulk].[UserTag]
             ,[bulk].[InfoURL]
             ,[bulk].[PictureURL1]
             ,[bulk].[PictureURL2]
             ,[bulk].[PictureURL3]
             ,[bulk].[UserBool1]
             ,[bulk].[UserBool2]
             ,[bulk].[UserBool3]
             ,[bulk].[UserBool4]
             ,[bulk].[UserBool5]
             ,[bulk].[UserString1]
             ,[bulk].[UserString2]
             ,[bulk].[UserString3]
             ,[bulk].[UserString4]
             ,[bulk].[UserString5]
             ,[bulk].[UserInt1]
             ,[bulk].[UserInt2]
             ,[bulk].[UserInt3]
             ,[bulk].[UserInt4]
             ,[bulk].[UserInt5]
             ,[bulk].[OperatorFlagCode]
    FROM      @BulkAircraft            AS [bulk]
    LEFT JOIN [BaseStation].[Aircraft] AS [aircraft] ON [bulk].[ModeS] = [aircraft].[ModeS]
    WHERE     [aircraft].[AircraftID] IS NULL;

    UPDATE [aircraft]
    SET    [FirstCreated]        = [bulk].[FirstCreated]
          ,[LastModified]        = [bulk].[LastModified]
          ,[ModeS]               = [bulk].[ModeS]
          ,[ModeSCountry]        = [bulk].[ModeSCountry]
          ,[Country]             = [bulk].[Country]
          ,[Registration]        = [bulk].[Registration]
          ,[CurrentRegDate]      = [bulk].[CurrentRegDate]
          ,[PreviousID]          = [bulk].[PreviousID]
          ,[FirstRegDate]        = [bulk].[FirstRegDate]
          ,[Status]              = [bulk].[Status]
          ,[DeRegDate]           = [bulk].[DeRegDate]
          ,[Manufacturer]        = [bulk].[Manufacturer]
          ,[ICAOTypeCode]        = [bulk].[ICAOTypeCode]
          ,[Type]                = [bulk].[Type]
          ,[SerialNo]            = [bulk].[SerialNo]
          ,[PopularName]         = [bulk].[PopularName]
          ,[GenericName]         = [bulk].[GenericName]
          ,[AircraftClass]       = [bulk].[AircraftClass]
          ,[Engines]             = [bulk].[Engines]
          ,[OwnershipStatus]     = [bulk].[OwnershipStatus]
          ,[RegisteredOwners]    = [bulk].[RegisteredOwners]
          ,[MTOW]                = [bulk].[MTOW]
          ,[TotalHours]          = [bulk].[TotalHours]
          ,[YearBuilt]           = [bulk].[YearBuilt]
          ,[CofACategory]        = [bulk].[CofACategory]
          ,[CofAExpiry]          = [bulk].[CofAExpiry]
          ,[UserNotes]           = [bulk].[UserNotes]
          ,[Interested]          = [bulk].[Interested]
          ,[UserTag]             = [bulk].[UserTag]
          ,[InfoURL]             = [bulk].[InfoURL]
          ,[PictureURL1]         = [bulk].[PictureURL1]
          ,[PictureURL2]         = [bulk].[PictureURL2]
          ,[PictureURL3]         = [bulk].[PictureURL3]
          ,[UserBool1]           = [bulk].[UserBool1]
          ,[UserBool2]           = [bulk].[UserBool2]
          ,[UserBool3]           = [bulk].[UserBool3]
          ,[UserBool4]           = [bulk].[UserBool4]
          ,[UserBool5]           = [bulk].[UserBool5]
          ,[UserString1]         = [bulk].[UserString1]
          ,[UserString2]         = [bulk].[UserString2]
          ,[UserString3]         = [bulk].[UserString3]
          ,[UserString4]         = [bulk].[UserString4]
          ,[UserString5]         = [bulk].[UserString5]
          ,[UserInt1]            = [bulk].[UserInt1]
          ,[UserInt2]            = [bulk].[UserInt2]
          ,[UserInt3]            = [bulk].[UserInt3]
          ,[UserInt4]            = [bulk].[UserInt4]
          ,[UserInt5]            = [bulk].[UserInt5]
          ,[OperatorFlagCode]    = [bulk].[OperatorFlagCode]
    OUTPUT INSERTED.[AircraftID]
          ,'Updated'
    INTO   @action
    FROM   @BulkAircraft            AS [bulk]
    JOIN   [BaseStation].[Aircraft] AS [aircraft] ON [bulk].[ModeS] = [aircraft].[ModeS]
    WHERE  [aircraft].[FirstCreated] <> [bulk].[FirstCreated]
    OR     [aircraft].[LastModified] <> [bulk].[LastModified]
    OR     [aircraft].[Interested] <>   [bulk].[Interested]
    OR     [aircraft].[UserBool1] <>    [bulk].[UserBool1]
    OR     [aircraft].[UserBool2] <>    [bulk].[UserBool2]
    OR     [aircraft].[UserBool3] <>    [bulk].[UserBool3]
    OR     [aircraft].[UserBool4] <>    [bulk].[UserBool4]
    OR     [aircraft].[UserBool5] <>    [bulk].[UserBool5]
    OR     ISNULL(NULLIF([aircraft].[ModeSCountry],     [bulk].[ModeSCountry]),     NULLIF([bulk].[ModeSCountry],       [aircraft].[ModeSCountry])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Country],          [bulk].[Country]),          NULLIF([bulk].[Country],            [aircraft].[Country])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Registration],     [bulk].[Registration]),     NULLIF([bulk].[Registration],       [aircraft].[Registration])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[CurrentRegDate],   [bulk].[CurrentRegDate]),   NULLIF([bulk].[CurrentRegDate],     [aircraft].[CurrentRegDate])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[PreviousID],       [bulk].[PreviousID]),       NULLIF([bulk].[PreviousID],         [aircraft].[PreviousID])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[FirstRegDate],     [bulk].[FirstRegDate]),     NULLIF([bulk].[FirstRegDate],       [aircraft].[FirstRegDate])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Status],           [bulk].[Status]),           NULLIF([bulk].[Status],             [aircraft].[Status])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[DeRegDate],        [bulk].[DeRegDate]),        NULLIF([bulk].[DeRegDate],          [aircraft].[DeRegDate])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Manufacturer],     [bulk].[Manufacturer]),     NULLIF([bulk].[Manufacturer],       [aircraft].[Manufacturer])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[ICAOTypeCode],     [bulk].[ICAOTypeCode]),     NULLIF([bulk].[ICAOTypeCode],       [aircraft].[ICAOTypeCode])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Type],             [bulk].[Type]),             NULLIF([bulk].[Type],               [aircraft].[Type])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[SerialNo],         [bulk].[SerialNo]),         NULLIF([bulk].[SerialNo],           [aircraft].[SerialNo])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[PopularName],      [bulk].[PopularName]),      NULLIF([bulk].[PopularName],        [aircraft].[PopularName])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[GenericName],      [bulk].[GenericName]),      NULLIF([bulk].[GenericName],        [aircraft].[GenericName])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[AircraftClass],    [bulk].[AircraftClass]),    NULLIF([bulk].[AircraftClass],      [aircraft].[AircraftClass])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[Engines],          [bulk].[Engines]),          NULLIF([bulk].[Engines],            [aircraft].[Engines])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[OwnershipStatus],  [bulk].[OwnershipStatus]),  NULLIF([bulk].[OwnershipStatus],    [aircraft].[OwnershipStatus])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[RegisteredOwners], [bulk].[RegisteredOwners]), NULLIF([bulk].[RegisteredOwners],   [aircraft].[RegisteredOwners])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[MTOW],             [bulk].[MTOW]),             NULLIF([bulk].[MTOW],               [aircraft].[MTOW])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[TotalHours],       [bulk].[TotalHours]),       NULLIF([bulk].[TotalHours],         [aircraft].[TotalHours])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[YearBuilt],        [bulk].[YearBuilt]),        NULLIF([bulk].[YearBuilt],          [aircraft].[YearBuilt])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[CofACategory],     [bulk].[CofACategory]),     NULLIF([bulk].[CofACategory],       [aircraft].[CofACategory])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[CofAExpiry],       [bulk].[CofAExpiry]),       NULLIF([bulk].[CofAExpiry],         [aircraft].[CofAExpiry])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserNotes],        [bulk].[UserNotes]),        NULLIF([bulk].[UserNotes],          [aircraft].[UserNotes])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserTag],          [bulk].[UserTag]),          NULLIF([bulk].[UserTag],            [aircraft].[UserTag])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[InfoURL],          [bulk].[InfoURL]),          NULLIF([bulk].[InfoURL],            [aircraft].[InfoURL])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[PictureURL1],      [bulk].[PictureURL1]),      NULLIF([bulk].[PictureURL1],        [aircraft].[PictureURL1])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[PictureURL2],      [bulk].[PictureURL2]),      NULLIF([bulk].[PictureURL2],        [aircraft].[PictureURL2])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[PictureURL3],      [bulk].[PictureURL3]),      NULLIF([bulk].[PictureURL3],        [aircraft].[PictureURL3])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserString1],      [bulk].[UserString1]),      NULLIF([bulk].[UserString1],        [aircraft].[UserString1])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserString2],      [bulk].[UserString2]),      NULLIF([bulk].[UserString2],        [aircraft].[UserString2])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserString3],      [bulk].[UserString3]),      NULLIF([bulk].[UserString3],        [aircraft].[UserString3])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserString4],      [bulk].[UserString4]),      NULLIF([bulk].[UserString4],        [aircraft].[UserString4])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserString5],      [bulk].[UserString5]),      NULLIF([bulk].[UserString5],        [aircraft].[UserString5])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserInt1],         [bulk].[UserInt1]),         NULLIF([bulk].[UserInt1],           [aircraft].[UserInt1])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserInt2],         [bulk].[UserInt2]),         NULLIF([bulk].[UserInt2],           [aircraft].[UserInt2])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserInt3],         [bulk].[UserInt3]),         NULLIF([bulk].[UserInt3],           [aircraft].[UserInt3])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserInt4],         [bulk].[UserInt4]),         NULLIF([bulk].[UserInt4],           [aircraft].[UserInt4])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[UserInt5],         [bulk].[UserInt5]),         NULLIF([bulk].[UserInt5],           [aircraft].[UserInt5])) IS NOT NULL
    OR     ISNULL(NULLIF([aircraft].[OperatorFlagCode], [bulk].[OperatorFlagCode]), NULLIF([bulk].[OperatorFlagCode],   [aircraft].[OperatorFlagCode])) IS NOT NULL;

    SELECT [aircraft].*
    FROM   [BaseStation].[Aircraft] AS [aircraft]
    JOIN   @BulkAircraft            AS [bulk]     ON [aircraft].[ModeS] = [bulk].[ModeS];

    SELECT *
    FROM   @action;
END;
GO
