INSERT INTO [EnginePlacementSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[EnumValue]
   ,[EnginePlacementName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@EnumValue
   ,@EnginePlacementName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [EnginePlacementSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
