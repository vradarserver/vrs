UPDATE [Model]
SET    [Name] =       @Name
      ,[UpdatedUtc] = @UpdatedUtc
WHERE  [ModelID] = @ModelID;

SELECT [CreatedUtc]
FROM   [Model]
WHERE  [ModelID] = @ModelID;
