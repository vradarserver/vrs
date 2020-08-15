INSERT INTO [SpeciesSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[EnumValue]
   ,[SpeciesName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@EnumValue
   ,@SpeciesName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [SpeciesSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
