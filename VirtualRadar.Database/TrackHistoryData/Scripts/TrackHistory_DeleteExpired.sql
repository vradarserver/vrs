PRAGMA temp_store = MEMORY;

CREATE TEMP TABLE _Result (
    [CountTrackHistories]       INTEGER
   ,[CountTrackHistoryStates]   INTEGER
   ,[EarliestHistoryUtc]        DATETIME
   ,[LatestHistoryUtc]          DATETIME
);

DELETE FROM [TrackHistoryState]
WHERE  [TrackHistoryID] IN (
    SELECT [TrackHistoryID]
    FROM   [TrackHistory]
    WHERE  [CreatedUtc] <= @threshold
    AND    [IsPreserved] = 0
);

INSERT INTO _Result (
    [CountTrackHistoryStates]
) VALUES (
    CHANGES()
);

UPDATE _Result
SET    [EarliestHistoryUtc] = (SELECT MIN([CreatedUtc]) FROM [TrackHistory] WHERE [CreatedUtc] <= @threshold AND [IsPreserved] = 0)
      ,[LatestHistoryUtc] =   (SELECT MAX([CreatedUtc]) FROM [TrackHistory] WHERE [CreatedUtc] <= @threshold AND [IsPreserved] = 0)
;

DELETE FROM [TrackHistory]
WHERE  [CreatedUtc] <= @threshold
AND    [IsPreserved] = 0;

UPDATE _Result
SET    [CountTrackHistories] = CHANGES();

SELECT *
FROM   _Result;
