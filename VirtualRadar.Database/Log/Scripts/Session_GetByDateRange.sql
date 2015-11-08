SELECT *
  FROM [Session]
 WHERE ([StartTime] >= @startDate AND [StartTime] <= @endDate)
    OR ([EndTime]   >= @startDate AND [EndTime]   <= @endDate)
    OR ([StartTime] <  @startDate AND [EndTime]   >  @endDate)
;
