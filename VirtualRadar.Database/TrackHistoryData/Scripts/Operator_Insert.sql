INSERT INTO [Operator] (
    [Icao]
   ,[Name]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Icao
   ,@Name
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [OperatorID] FROM [Operator] WHERE _ROWID_ = last_insert_rowid();
