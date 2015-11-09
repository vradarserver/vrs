INSERT INTO [SystemEvents] (
    [TimeStamp]
   ,[App]
   ,[Msg]
) VALUES (
    @timeStamp
   ,@app
   ,@msg
);
SELECT [SystemEventsID] FROM [SystemEvents] WHERE _ROWID_ = last_insert_rowid();
