IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_GetByID')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_GetByID] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_GetByID]
    @FlightID INTEGER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM   [BaseStation].[Flights]
    WHERE  [FlightID] = @FlightID;
END;
GO

