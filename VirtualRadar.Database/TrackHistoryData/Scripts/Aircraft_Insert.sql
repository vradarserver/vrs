INSERT INTO [Aircraft] (
    [Icao]
   ,[IcaoCountryID]
   ,[AircraftTypeID]
   ,[OperatorID]
   ,[Registration]
   ,[Serial]
   ,[YearBuilt]
   ,[IsInteresting]
   ,[IsMilitary]
   ,[Notes]
   ,[LastLookupUtc]
   ,[IsMissingFromLookup]
   ,[SuppressAutoUpdates]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @Icao
   ,@IcaoCountryID
   ,@AircraftTypeID
   ,@OperatorID
   ,@Registration
   ,@Serial
   ,@YearBuilt
   ,@IsInteresting
   ,@IsMilitary
   ,@Notes
   ,@LastLookupUtc
   ,@IsMissingFromLookup
   ,@SuppressAutoUpdates
   ,@CreatedUtc
   ,@UpdatedUtc
);
SELECT [AircraftID] FROM [Aircraft] WHERE _ROWID_ = last_insert_rowid();
