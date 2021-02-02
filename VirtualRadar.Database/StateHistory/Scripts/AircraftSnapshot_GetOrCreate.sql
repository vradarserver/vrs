INSERT INTO [AircraftSnapshot] (
    [CreatedUtc]
   ,[Fingerprint]
   ,[Icao]
   ,[Registration]
   ,[ModelSnapshotID]
   ,[ConstructionNumber]
   ,[YearBuilt]
   ,[OperatorSnapshotID]
   ,[CountrySnapshotID]
   ,[IsMilitary]
   ,[IsInteresting]
   ,[UserNotes]
   ,[UserTag]
) VALUES (
    @CreatedUtc
   ,@Fingerprint
   ,@Icao
   ,@Registration
   ,@ModelSnapshotID
   ,@ConstructionNumber
   ,@YearBuilt
   ,@OperatorSnapshotID
   ,@CountrySnapshotID
   ,@IsMilitary
   ,@IsInteresting
   ,@UserNotes
   ,@UserTag
)
ON CONFLICT ([Fingerprint]) DO NOTHING;

SELECT *
FROM   [AircraftSnapshot]
WHERE  [Fingerprint] = @Fingerprint;
