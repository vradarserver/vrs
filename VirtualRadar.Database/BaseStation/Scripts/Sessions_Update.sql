UPDATE [Sessions]
   SET [LocationID] = @LocationID
      ,[StartTime]  = @StartTime
      ,[EndTime]    = @EndTime
WHERE [SessionID] = @SessionID;
