INSERT INTO [OperatorSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[Icao]
   ,[OperatorName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@Icao
   ,@OperatorName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [OperatorSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
