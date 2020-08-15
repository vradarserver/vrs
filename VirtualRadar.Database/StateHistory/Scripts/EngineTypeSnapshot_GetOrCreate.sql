INSERT INTO [EngineTypeSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[EnumValue]
   ,[EngineTypeName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@EnumValue
   ,@EngineTypeName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [EngineTypeSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
