INSERT INTO [TrackHistory] (
    [Icao]
   ,[IsPreserved]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Icao
   ,@IsPreserved
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [TrackHistoryID] FROM [TrackHistory] WHERE _ROWID_ = last_insert_rowid();
