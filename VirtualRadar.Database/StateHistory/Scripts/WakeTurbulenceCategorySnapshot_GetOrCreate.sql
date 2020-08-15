INSERT INTO [WakeTurbulenceCategorySnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[EnumValue]
   ,[WakeTurbulenceCategoryName]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@EnumValue
   ,@WakeTurbulenceCategoryName
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [WakeTurbulenceCategorySnapshot]
WHERE  [Fingerprint] = @Fingerprint;
