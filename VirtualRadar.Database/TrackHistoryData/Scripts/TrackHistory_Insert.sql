INSERT INTO [TrackHistory] (
    [AircraftID]
   ,[IsPreserved]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @AircraftID
   ,@IsPreserved
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [TrackHistoryID] FROM [TrackHistory] WHERE _ROWID_ = last_insert_rowid();
