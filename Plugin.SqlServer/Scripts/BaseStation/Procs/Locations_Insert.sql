IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Locations_Insert')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Locations_Insert] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Locations_Insert]
    @LocationName NVARCHAR(80)
   ,@Latitude     REAL
   ,@Longitude    REAL
   ,@Altitude     REAL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [BaseStation].[Locations] (
         [LocationName]
        ,[Latitude]
        ,[Longitude]
        ,[Altitude]
    ) VALUES (
         @LocationName
        ,@Latitude
        ,@Longitude
        ,@Altitude
    );

    SELECT SCOPE_IDENTITY() AS [LocationID];
END;
GO
