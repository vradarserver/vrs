IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'AircraftAndFlightsCount_GetByCodes')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[AircraftAndFlightsCount_GetByCodes] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[AircraftAndFlightsCount_GetByCodes]
    @Codes AS [VRS].[Icao24] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    WITH [FlightCounts] AS (
        SELECT   [aircraft].[AircraftID]
                ,COUNT(*) AS [FlightsCount]
        FROM     @Codes                   AS [code]
        JOIN     [BaseStation].[Aircraft] AS [aircraft] ON [code].[ModeS] = [aircraft].[ModeS]
        JOIN     [BaseStation].[Flights]  AS [flight]   ON [aircraft].[AircraftID] = [flight].[AircraftID]
        GROUP BY [aircraft].[AircraftID]
    )
    SELECT    [aircraft].*
             ,ISNULL([flightCount].[FlightsCount], 0) AS [FlightsCount]
    FROM      @Codes                   AS [code]
    JOIN      [BaseStation].[Aircraft] AS [aircraft]    ON [code].[ModeS] = [aircraft].[ModeS]
    LEFT JOIN [FlightCounts]           AS [flightCount] ON [aircraft].[AircraftID] = [flightCount].[AircraftID];
END;
GO
