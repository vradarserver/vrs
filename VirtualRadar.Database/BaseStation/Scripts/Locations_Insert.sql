INSERT INTO [Locations] (
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
SELECT [LocationID] FROM [Locations] WHERE _ROWID_ = last_insert_rowid();
