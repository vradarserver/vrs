INSERT INTO [CountrySnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[CountryName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@CountryName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [CountrySnapshot]
WHERE  [Fingerprint] = @Fingerprint;
