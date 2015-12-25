INSERT INTO [Session] (
    [ClientId]
   ,[UserName]
   ,[StartTime]
   ,[EndTime]
   ,[CountRequests]
   ,[OtherBytesSent]
   ,[HtmlBytesSent]
   ,[JsonBytesSent]
   ,[ImageBytesSent]
   ,[AudioBytesSent]
) VALUES (
    @clientId
   ,@userName
   ,@startTime
   ,@endTime
   ,@countRequests
   ,@otherBytesSent
   ,@htmlBytesSent
   ,@jsonBytesSent
   ,@imageBytesSent
   ,@audioBytesSent
);
SELECT [Id] FROM [Session] WHERE _ROWID_ = last_insert_rowid();
