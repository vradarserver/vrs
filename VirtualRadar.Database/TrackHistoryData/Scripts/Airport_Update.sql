UPDATE [Airport]
SET    [Icao] =         @Icao
      ,[Iata] =         @Iata
      ,[Name] =         @Name
      ,[Latitude] =     @Latitude
      ,[Longitude] =    @Longitude
      ,[CountryID] =    @CountryID
      ,[UpdatedUtc] =   @UpdatedUtc
WHERE  [AirportID] = @AirportID;

SELECT [CreatedUtc]
FROM   [Airport]
WHERE  [AirportID] = @AirportID;
