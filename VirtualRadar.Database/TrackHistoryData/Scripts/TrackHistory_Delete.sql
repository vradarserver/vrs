DECLARE @countTrackHistories AS INTEGER;
DECLARE @countTrackHistoryStates AS INTEGER;
DECLARE @historyUtc AS DATETIME;

DELETE FROM [TrackHistoryState]
WHERE  [TrackHistoryID] = @trackHistoryID;
SET @countTrackHistoryStates = CHANGES();

SELECT @historyUtc = [CreatedUtc]
FROM   [TrackHistory]
WHERE  [TrackHistoryID] = @trackHistoryID;

DELETE FROM [TrackHistory]
WHERE  [TrackHistoryID] = @trackHistoryID;
SET @countTrackHistories = CHANGES();

SELECT @countTrackHistories     AS [CountTrackHistories]
      ,@countTrackHistoryStates AS [CountTrackHistoryStates]
      ,@historyUtc              AS [EarliestHistoryUtc]
      ,@historyUtc              AS [LatestHistoryUtc];
