INSERT INTO [Receiver] (
    [Name]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Name
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [ReceiverID] FROM [Receiver] WHERE _ROWID_ = last_insert_rowid();
