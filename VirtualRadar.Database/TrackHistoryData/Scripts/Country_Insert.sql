INSERT INTO [Country] (
    [Name]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Name
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [CountryID] FROM [Country] WHERE _ROWID_ = last_insert_rowid();
