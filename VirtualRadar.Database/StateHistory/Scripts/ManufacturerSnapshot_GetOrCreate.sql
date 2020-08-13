INSERT INTO [ManufacturerSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[ManufacturerName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@ManufacturerName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [ManufacturerSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
