INSERT INTO [Sessions] (
    [LocationID]
   ,[StartTime]
   ,[EndTime]
) VALUES (
    @locationID
   ,@startTime
   ,@endTime
);
SELECT [SessionID] FROM [Sessions] WHERE _ROWID_ = last_insert_rowid();
