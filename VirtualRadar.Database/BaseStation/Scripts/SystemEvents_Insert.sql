INSERT INTO [SystemEvents] (
    [TimeStamp]
   ,[App]
   ,[Msg]
) VALUES (
    @TimeStamp
   ,@App
   ,@Msg
);
SELECT [SystemEventsID] FROM [SystemEvents] WHERE _ROWID_ = last_insert_rowid();
