IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = 'BaseStation' AND ROUTINE_NAME = 'Aircraft_UpdateModeSCountry')
BEGIN
    EXECUTE sys.sp_executesql N'CREATE PROCEDURE [BaseStation].[Aircraft_UpdateModeSCountry] AS BEGIN SET NOCOUNT ON; END;';
END;
GO

ALTER PROCEDURE [BaseStation].[Aircraft_UpdateModeSCountry]
    @AircraftID   INTEGER
   ,@LastModified DATETIME
   ,@ModeSCountry NVARCHAR(80)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BaseStation].[Aircraft]
    SET    [LastModified] = @LastModified
          ,[ModeSCountry] = @ModeSCountry
    WHERE [AircraftID] = @AircraftID;
END;
GO
