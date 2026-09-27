INSERT INTO [Sessions] (
    [LocationID]
   ,[StartTime]
   ,[EndTime]
) VALUES (
    @LocationID
   ,@StartTime
   ,@EndTime
);
SELECT [SessionID] FROM [Sessions] WHERE _ROWID_ = last_insert_rowid();
