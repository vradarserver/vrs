UPDATE [Receiver]
SET    [Name] =       @Name
      ,[UpdatedUtc] = @UpdatedUtc
WHERE  [ReceiverID] = @ReceiverID;

SELECT [CreatedUtc]
FROM   [Receiver]
WHERE  [ReceiverID] = @ReceiverID;
