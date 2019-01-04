INSERT INTO [Manufacturer] (
    [Name]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Name
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [ManufacturerID] FROM [Manufacturer] WHERE _ROWID_ = last_insert_rowid();
