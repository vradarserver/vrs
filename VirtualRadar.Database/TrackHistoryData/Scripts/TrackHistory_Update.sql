UPDATE [TrackHistory]
SET    [Icao] =         @Icao
      ,[IsPreserved] =  @IsPreserved
      ,[UpdatedUtc] =   @UpdatedUtc
WHERE  [TrackHistoryID] = @TrackHistoryID;

SELECT [CreatedUtc]
FROM   [TrackHistory]
WHERE  [TrackHistoryID] = @TrackHistoryID;
