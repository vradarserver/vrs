INSERT INTO [Airport] (
    [Icao]
   ,[Iata]
   ,[Name]
   ,[Latitude]
   ,[Longitude]
   ,[CountryID]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Icao
   ,@Iata
   ,@Name
   ,@Latitude
   ,@Longitude
   ,@CountryID
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [AirportID] FROM [Airport] WHERE _ROWID_ = last_insert_rowid();
