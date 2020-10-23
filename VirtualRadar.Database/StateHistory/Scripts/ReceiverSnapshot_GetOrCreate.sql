INSERT INTO [ReceiverSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[Key]
   ,[ReceiverID]
   ,[ReceiverName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@Key
   ,@ReceiverID
   ,@ReceiverName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [ReceiverSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
