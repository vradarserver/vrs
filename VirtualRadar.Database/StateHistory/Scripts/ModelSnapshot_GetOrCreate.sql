INSERT INTO [ModelSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[Icao]
   ,[ManufacturerSnapshotID]
   ,[ModelName]
   ,[WakeTurbulenceCategorySnapshotID]
   ,[EngineTypeSnapshotID]
   ,[EnginePlacementSnapshotID]
   ,[NumberOfEngines]
   ,[SpeciesSnapshotID]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@Icao
   ,@ManufacturerSnapshotID
   ,@ModelName
   ,@WakeTurbulenceCategorySnapshotID
   ,@EngineTypeSnapshotID
   ,@EnginePlacementSnapshotID
   ,@NumberOfEngines
   ,@SpeciesSnapshotID
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [ModelSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
