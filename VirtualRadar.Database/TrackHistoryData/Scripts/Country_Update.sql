UPDATE [Country]
SET    [Name] =       @Name
      ,[UpdatedUtc] = @UpdatedUtc
WHERE  [CountryID] = @CountryID;

SELECT [CreatedUtc]
FROM   [Country]
WHERE  [CountryID] = @CountryID;
