UPDATE [Operator]
SET    [Icao] =       @Icao
      ,[Name] =       @Name
      ,[UpdatedUtc] = @UpdatedUtc
WHERE  [OperatorID] = @OperatorID;

SELECT [CreatedUtc]
FROM   [Operator]
WHERE  [OperatorID] = @OperatorID;
