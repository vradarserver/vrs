INSERT INTO [VrsSession] (
    [DatabaseVersionID]
   ,[CreatedUtc]
) VALUES (
    @DatabaseVersionID
   ,@CreatedUtc
);
SELECT last_insert_rowid();
