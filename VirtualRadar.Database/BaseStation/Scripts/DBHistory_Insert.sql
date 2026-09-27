INSERT INTO [DBHistory] (
    [TimeStamp]
   ,[Description]
) VALUES (
    @TimeStamp
   ,@Description
);
SELECT [DBHistoryID] FROM [DBHistory] WHERE _ROWID_ = last_insert_rowid();
