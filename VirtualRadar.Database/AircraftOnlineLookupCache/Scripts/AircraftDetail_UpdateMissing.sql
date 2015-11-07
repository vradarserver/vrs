UPDATE [AircraftDetail]
   SET [Icao]          = @icao
      ,[UpdatedUtc]    = @updatedUtc
 WHERE [AircraftDetailID] = @aircraftDetailId;
