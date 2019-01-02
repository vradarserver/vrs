UPDATE [TrackHistory]
SET    [AircraftID] =   @AircraftID
      ,[IsPreserved] =  @IsPreserved
      ,[UpdatedUtc] =   @UpdatedUtc
WHERE  [TrackHistoryID] = @TrackHistoryID;

SELECT [CreatedUtc]
FROM   [TrackHistory]
WHERE  [TrackHistoryID] = @TrackHistoryID;
