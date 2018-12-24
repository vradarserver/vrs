SELECT   *
FROM     [TrackHistory]
WHERE    [Icao] = @icao
AND      [CreatedUtc] BETWEEN IFNULL(@startTimeInclusive, '1990-01-01') AND IFNULL(@endTimeInclusive, '9999-12-31')
ORDER BY [CreatedUtc];
