UPDATE [Aircraft]
SET    [Icao] =                 @Icao
      ,[IcaoCountryID] =        @IcaoCountryID
      ,[AircraftTypeID] =       @AircraftTypeID
      ,[OperatorID] =           @OperatorID
      ,[Registration] =         @Registration
      ,[Serial] =               @Serial
      ,[YearBuilt] =            @YearBuilt
      ,[IsInteresting] =        @IsInteresting
      ,[IsMilitary] =           @IsMilitary
      ,[Notes] =                @Notes
      ,[LastLookupUtc] =        @LastLookupUtc
      ,[IsMissingFromLookup] =  @IsMissingFromLookup
      ,[SuppressAutoUpdates] =  @SuppressAutoUpdates
      ,[UpdatedUtc] =           @UpdatedUtc
WHERE  [AircraftID] = @AircraftID;

SELECT [CreatedUtc]
FROM   [Aircraft]
WHERE  [AircraftID] = @AircraftID;
