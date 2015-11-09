INSERT INTO [DBHistory] (
    [TimeStamp]
   ,[Description]
) VALUES (
    @timeStamp
   ,@description
);
SELECT [DBHistoryID] FROM [DBHistory] WHERE _ROWID_ = last_insert_rowid();
