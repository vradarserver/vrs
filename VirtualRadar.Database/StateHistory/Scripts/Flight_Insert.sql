INSERT INTO [Flight] (
    [Preserve]
   ,[IntervalSeconds]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Preserve
   ,@IntervalSeconds
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT last_insert_rowid();
