/*
    This script can be run against a bog-standard StandingData.sqb
    to create a set of data for the StandingDataManager unit tests
*/

DELETE FROM [AircraftTypeModel]
WHERE  [AircraftTypeId] NOT IN (
    SELECT [AircraftTypeId]
    FROM   [AircraftType]
    WHERE  [Icao] IN ('A30B', 'BCS3', 'D11', '-GND', '-TWR')
);

/* We need to whittle the A30B models down to a single instance, the A-300B2 */
DELETE FROM [AircraftTypeModel]
WHERE  [AircraftTypeId] = (SELECT [AircraftTypeId] FROM [AircraftType] WHERE [Icao] = 'A30B')
AND    [ModelId] IN (SELECT [ModelId] FROM [Model] WHERE [Name] <> 'A-300B2');

/* Ditto, but this time the D11 needs to be whittled down to two examples, Cruiser and F-11 */
DELETE FROM [AircraftTypeModel]
WHERE  [AircraftTypeId] = (SELECT [AircraftTypeId] FROM [AircraftType] WHERE [Icao] = 'D11')
AND    [ModelId] IN (SELECT [ModelId] FROM [Model] WHERE [Name] NOT IN ('Cruiser', 'F-11'));

DELETE FROM [AircraftType]
WHERE  [Icao] NOT IN ('A30B', 'BCS3', 'D11', '-GND', '-TWR');

DELETE FROM [Model]
WHERE [ModelId] NOT IN (
    SELECT [ModelId]
    FROM   [AircraftTypeModel]
);

DELETE FROM [Manufacturer]
WHERE  [ManufacturerId] NOT IN (
    SELECT [ManufacturerId]
    FROM   [Model]
);

DELETE FROM [RouteStop]
WHERE  [RouteId] NOT IN (
    SELECT [RouteId]
    FROM   [Route]
    WHERE  [Callsign] IN ('DLH400', 'DLH8208', 'DLH8222')
);

DELETE FROM [Route]
WHERE  [Callsign] NOT IN ('DLH400', 'DLH8208', 'DLH8222');

DELETE FROM [Operator]
WHERE  [Icao] NOT IN ('DLH');

DELETE FROM [Airport]
WHERE  [AirportId] NOT IN (
    SELECT [FromAirportId]
    FROM   [Route]
    UNION ALL
    SELECT [ToAirportId]
    FROM   [Route]
    UNION ALL
    SELECT [AirportId]
    FROM   [RouteStop]
);

VACUUM;
