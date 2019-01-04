INSERT INTO [Model] (
    [Name]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Name
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [ModelID] FROM [Model] WHERE _ROWID_ = last_insert_rowid();
