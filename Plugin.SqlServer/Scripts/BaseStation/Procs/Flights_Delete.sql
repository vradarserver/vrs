IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Flights_Delete')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Flights_Delete] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Flights_Delete]
    @FlightID BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [BaseStation].[Flights]
    WHERE  [FlightID] = @FlightID;
END;
GO
