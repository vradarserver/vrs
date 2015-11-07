UPDATE [AircraftDetail]
   SET [Icao]          = @icao
      ,[Registration]  = @registration
      ,[Country]       = @country
      ,[Manufacturer]  = @manufacturer
      ,[Model]         = @model
      ,[ModelIcao]     = @modelIcao
      ,[Operator]      = @operator
      ,[OperatorIcao]  = @operatorIcao
      ,[Serial]        = @serial
      ,[YearBuilt]     = @yearBuilt
      ,[UpdatedUtc]    = @updatedUtc
 WHERE [AircraftDetailID] = @aircraftDetailId;
