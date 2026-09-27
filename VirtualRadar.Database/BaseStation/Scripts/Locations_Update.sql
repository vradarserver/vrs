UPDATE [Locations]
   SET [LocationName]   = @LocationName
      ,[Latitude]       = @Latitude
      ,[Longitude]      = @Longitude
      ,[Altitude]       = @Altitude
WHERE [LocationID] = @LocationID;
