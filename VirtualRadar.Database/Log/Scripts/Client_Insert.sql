INSERT INTO [Client] (
    [IpAddress]
   ,[ReverseDns]
   ,[ReverseDnsDate]
) VALUES (
    @ipAddress
   ,@reverseDns
   ,@reverseDnsDate
);
SELECT [Id] FROM [Client] WHERE _ROWID_ = last_insert_rowid();
