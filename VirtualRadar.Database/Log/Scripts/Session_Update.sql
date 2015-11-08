UPDATE [Session]
   SET [ClientId] =         @clientId
      ,[StartTime] =        @startTime
      ,[EndTime] =          @endTime
      ,[CountRequests] =    @countRequests
      ,[OtherBytesSent] =   @otherBytesSent
      ,[HtmlBytesSent] =    @htmlBytesSent
      ,[JsonBytesSent] =    @jsonBytesSent
      ,[ImageBytesSent] =   @imageBytesSent
      ,[AudioBytesSent] =   @audioBytesSent
WHERE [Id] = @id;
