INSERT INTO [AircraftDetail] (
    [Icao]
   ,[Registration]
   ,[Country]
   ,[Manufacturer]
   ,[Model]
   ,[ModelIcao]
   ,[Operator]
   ,[OperatorIcao]
   ,[Serial]
   ,[YearBuilt]
   ,[CreatedUtc]
   ,[UpdatedUtc]
) VALUES (
    @icao
   ,@registration
   ,@country
   ,@manufacturer
   ,@model
   ,@modelIcao
   ,@operator
   ,@operatorIcao
   ,@serial
   ,@yearBuilt
   ,@createdUtc
   ,@updatedUtc
);
SELECT [AircraftDetailID] FROM [AircraftDetail] WHERE _ROWID_ = last_insert_rowid();
