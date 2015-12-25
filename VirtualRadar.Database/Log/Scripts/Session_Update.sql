UPDATE [Session]
   SET [ClientId] =         @clientId
      ,[UserName] =         IFNULL(@userName, [UserName])
      ,[StartTime] =        @startTime
      ,[EndTime] =          @endTime
      ,[CountRequests] =    @countRequests
      ,[OtherBytesSent] =   @otherBytesSent
      ,[HtmlBytesSent] =    @htmlBytesSent
      ,[JsonBytesSent] =    @jsonBytesSent
      ,[ImageBytesSent] =   @imageBytesSent
      ,[AudioBytesSent] =   @audioBytesSent
WHERE [Id] = @id;
