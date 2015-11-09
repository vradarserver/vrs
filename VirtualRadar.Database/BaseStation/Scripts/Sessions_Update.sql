UPDATE [Sessions]
   SET [LocationID] = @locationID
      ,[StartTime]  = @startTime
      ,[EndTime]    = @endTime
WHERE [SessionID] = @sessionID;
