UPDATE [Manufacturer]
SET    [Name] =       @Name
      ,[UpdatedUtc] = @UpdatedUtc
WHERE  [ManufacturerID] = @ManufacturerID;

SELECT [CreatedUtc]
FROM   [Manufacturer]
WHERE  [ManufacturerID] = @ManufacturerID;
