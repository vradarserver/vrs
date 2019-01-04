UPDATE [AircraftType]
SET    [Icao] =                     @Icao
      ,[ManufacturerID] =           @ManufacturerID
      ,[ModelID] =                  @ModelID
      ,[EngineTypeID] =             @EngineTypeID
      ,[EnginePlacementID] =        @EnginePlacementID
      ,[WakeTurbulenceCategoryID] = @WakeTurbulenceCategoryID
      ,[EngineCount] =              @EngineCount
      ,[UpdatedUtc] =               @UpdatedUtc
WHERE  [AircraftTypeID] = @AircraftTypeID;

SELECT [CreatedUtc]
FROM   [AircraftType]
WHERE  [AircraftTypeID] = @AircraftTypeID;
