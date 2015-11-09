SELECT [Aircraft].*
      ,(SELECT COUNT(*) FROM [Flights] WHERE [Flights].[AircraftID] = [Aircraft].[AircraftID]) AS [FlightsCount]
  FROM [Aircraft]
 WHERE [Aircraft].[ModeS] IN @icaos;
