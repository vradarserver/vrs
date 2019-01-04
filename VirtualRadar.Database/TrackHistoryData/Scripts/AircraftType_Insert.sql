INSERT INTO [AircraftType] (
    [Icao]
   ,[ManufacturerID]
   ,[ModelID]
   ,[EngineTypeID]
   ,[EnginePlacementID]
   ,[WakeTurbulenceCategoryID]
   ,[EngineCount]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Icao
   ,@ManufacturerID
   ,@ModelID
   ,@EngineTypeID
   ,@EnginePlacementID
   ,@WakeTurbulenceCategoryID
   ,@EngineCount
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [AircraftTypeID] FROM [AircraftType] WHERE _ROWID_ = last_insert_rowid();
