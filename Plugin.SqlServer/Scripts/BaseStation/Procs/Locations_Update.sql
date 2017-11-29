IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_Update')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_Update] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_Update]
    @LocationID   BIGINT
   ,@LocationName NVARCHAR(20)
   ,@Latitude     REAL
   ,@Longitude    REAL
   ,@Altitude     REAL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Locations]
    SET    [LocationName]    = @LocationName
          ,[Latitude]        = @Latitude
          ,[Longitude]       = @Longitude
          ,[Altitude]        = @Altitude
    WHERE [LocationID] = @LocationID;
END;
GO
