# State History

This is a database that records program state at different points in time. The intervals between the
points in time can vary, typically you would have long gaps between state snapshots recorded many months
ago and short gaps between recent snapshots.

The state snapshots record state for:

* Client connections
* The aircraft lists

## Client Connections

The state database records the /24 IP address of the machine that connects, the user name that they connect as,
the moment of the initial connection and the moment of the most recent connection.

The database records are only updated periodically so the most recent connection might not be up-to-date.

The intention is to replace the existing client connection log with this log.

### Configuration

Configuration options exist to control:

* Whether client connections are recorded at all (default is to record connections)
* How many hours client connections are stored for (default is 28 days - 28 * 24).

## Aircraft Lists

The state database periodically takes a snapshot of every aircraft list that is not flagged as "merge only"
and records the state of the lists on the database.

Recording an aircraft list will involve different types of database records:

* Standing data - copies of SDM items that rarely change over time: e.g. aircraft lookups, models,
  airports, routes etc.
* State data - values transmitted by the aircraft during flight.

### Standing Data

Each record type is stored in its own table. Each record type contains a history of all changes to
the standing data, one record per version of the record. To support this we need to follow a few
rules, some of which are drawn from git:

1. Each record has an immutable unique database-assigned ID. This is the primary key. It is an 8-byte int.
2. Each record has a CreatedUTC column that holds the date and time the record was created.
3. Most record types will have the concept of a unique real-world ID - for example, an aircraft
   record might have the Mode-S six digit hex code. These are not unique within the database,
   if you want the most recent version of the record then you need to take the top record by
   descending order of CreatedUTC.
4. To simplify version management each record will have a 20 byte SHA-1 hash. The idea is borrowed
   from git - the hash is formed from a text representation of the record and is used to figure out
   whether a particular version already exists or whether it needs to be created. Hashes will likely
   be unique across the database but will definitely be unique within a record type. The database ID
   and the CreatedUTC fields will not be considered when calculating the hash. The field needs to
   have a unique index.

The intention is to have a single core library that can deal with the creation or loading of database IDs
for any set of SDM records presented to it. It will assume that all records have this core set of fields.

### Flight Recording

The easiest way to record a flight is to periodically write all known values for the flight for every
snapshot. This will use a lot of disk.

A more disk efficient approach would be to write one record that holds the initial values and then subsequent
records only store changes from the previous record.

Different database and file management systems store nulls in different ways. The ones we care about at the
moment, SQLite and SQL Server, both store nulls reasonably efficiently:

| Engine | NULL storage requirements |
| ---    | ---                       |
| SQLite | One byte per NULL field |
| SQL Server | None (sets the field's bit in the null bitmap) |

#### Snapshot

There is one snapshot record per aircraft list. It is the parent for SnapshotAircraft records.

| Name       | Type     | Meaning |
| ---        | ---      | --- |
| SnapshotID | bigint   | Unique ID of the record |
| CreatedUTC | datetime | Moment when the snapshot was taken |
| Sequence   | bigint   | Indicates display order in ascending order |
| ReceiverID | bigint   | ID of the SDM record for the receiver being recorded |

```Sequence``` is assigned via the database, either by the stored procedure that saves the record or (if ```SnapshotID``` is
guaranteed by the database to always be in ascending order of creation) it can be a calculated field on ```SnapshotID```. No
meaning should be assigned to it other than it can be used to sort snapshots into the correct display order.

#### SnapshotAircraft

There is one aircraft record per aircraft in an aircraft list.

| Name                | Type       | Nullable | Meaning |
| ---                 | ---        | ---      | --- |
| SnapshotAircraftID  | bigint     | N        | Unique ID of the record |
| SnapshotID          | bigint     | N        | Parent snapshot |
| Icao                | int        | N        | Unique ID of the aircraft |
| IsPreserved         | bit        | N        | True if the aircraft snapshot (and parent snapshot) are to be excluded from auto-cleans |
| AircraftID          | bigint     | Y        | Unique ID of the new SDM record for the aircraft |
| ReceiverID          | bigint     | Y        | Unique ID of the SDM record of receiver than is now providing aircraft messages |
| PositionReceiverID  | bigint     | Y        | Unique ID of the SDM record of receiver that is now providing position messages |
| Callsign            | varchar(8) | Y        | New callsign |
| IsCallsignSuspect   | bit        | Y        | Change in whether callsign is from a reliable source |
| Longitude           | real       | Y        | New longitude |
| Latitude            | real       | Y        | New latitude |
| IsMlat              | bit        | Y        | Change in position MLAT source indicator |
| IsTisb              | bit        | Y        | Change in TISB message source indicator |
| AltitudeFeet        | int        | Y        | New altitude |
| AltitudeTypeID      | bigint     | Y        | Unique ID of the new altitude type SDM record |
| TargetAltitudeFeet  | int        | Y        | New altitude target on the autopilot |
| AirPressureInHg     | real       | Y        | New local air pressure at ground level in inches of mercury |
| GroundSpeedKnots    | int        | Y        | New ground speed in knots |
| SpeedTypeID         | bigint     | Y        | Unique ID of the new ground speed type SDM record |
| TrackDegrees        | int        | Y        | New ground track angle in degrees clockwise from north |
| TargetTrackDegrees  | int        | Y        | New target track on the autopilot |
| TrackIsHeading      | bit        | Y        | New setting for flag indicating that the track values indicate aircraft heading not ground track |
| VerticalRateFeetMin | int        | Y        | New vertical speed in feet per minute |
| VerticalRateTypeID  | bigint     | Y        | Unique ID of the new vertical rate speed type SDM record |
| Squawk              | smallint   | Y        | New squawk expressed as if it were base-10 (e.g. squawk of 0021 is saved as the number 21 instead of more technically correct 17) |
| IdentActive         | bit        | Y        | New ident flag value |
| SignalLevel         | int        | Y        | New signal level for receivers that pass it through |
| RouteID             | bigint     | Y        | Unique ID of the SDM record for the new route being flown by the aircraft |
| IsPositioningFlight | bit        | Y        | New flag indicating that the aircraft is flying a positioning flight |
| IsCharterFlight     | bit        | Y        | New flag indicating that the aircraft is flying a charter flight |
| TransponderTypeID   | bigint     | Y        | Unique ID of the SDM record for the aircraft's transponder type (as inferred from messages received so far) |

There is a possible update anomaly between ```Icao``` and the aircraft referenced by ```AircraftID```.
However, we need an aircraft ID that cannot change mid-flight and that is not true of the SDM aircraft,
an aircraft lookup could change the SDM details and trigger creation of a new SDM record for it. I
think we just need to put up with the possible anomaly.

The aircraft ```ReceiverID``` will be different to the parent snapshot's ```ReceiverID``` when the parent
snapshot is a merged feed. The parent records the merged feed ID, each aircraft records the receiver that
is picking up the aircraft. The aircraft receivers can change over the duration of the flight.