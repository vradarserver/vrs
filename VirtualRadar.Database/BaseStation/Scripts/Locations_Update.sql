UPDATE [Locations]
   SET [LocationName]   = @locationName
      ,[Latitude]       = @latitude
      ,[Longitude]      = @longitude
      ,[Altitude]       = @altitude
WHERE [LocationID] = @locationID;
