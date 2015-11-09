INSERT INTO [Locations] (
    [LocationName]
   ,[Latitude]
   ,[Longitude]
   ,[Altitude]
) VALUES (
    @locationName
   ,@latitude
   ,@longitude
   ,@altitude
);
SELECT [LocationID] FROM [Locations] WHERE _ROWID_ = last_insert_rowid();
