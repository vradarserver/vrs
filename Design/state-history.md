# State History

This is a database that records program state at different points in time. The intervals between the
points in time can vary, typically you would have long gaps between state snapshots recorded many months
ago and short gaps between recent snapshots.

The state snapshots record state for:

* The aircraft lists

All strings use case insensitive collation.

## Pre-requisites

At the moment receiver IDs are integers and are only guaranteed to be unique across a run of VRS. State history
will be recording snapshots against receivers and those recordings can run across sessions. We need receiver IDs
that are globally unique.

To this end receivers are going to be allocated GUIDs. They are allocated behind the scenes and are stored in
the configuration file. The GUIDs will be used to identify receivers in state history.

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
rules:

1. Each record has an immutable unique database-assigned ID. This is the primary key. It is an 8-byte int.
2. Each record has a CreatedUTC column that holds the date and time the record was created.
3. Most record types will have the concept of a unique real-world ID - for example, an aircraft
   record might have the Mode-S six digit hex code. These are not unique within the database,
   if you want the most recent version of the record then you need to take the top record by
   descending order of CreatedUTC.
4. To simplify version management each record will have a 20 byte SHA-1 hash. The idea is nicked
   from git - the hash is formed from a binary representation of the record and is used to figure out
   whether a particular version already exists or whether it needs to be created. Hashes will likely
   be unique across the database but will definitely be unique within a record type. The database ID
   and timestamp(s) will not be considered when calculating the hash. The field needs to
   have a unique index.

The intention is to have a single core library that can deal with the creation or loading of database IDs
for any set of SDM records presented to it. It will assume that all records have this core set of fields.

### String Field Lengths

Some string field lengths will be excessive when compared to the maximums in corresponding ICAO specifications. This
is because the snapshots could be recording data coming from informal sources such as privately curated
BaseStation.sqb files, and those sources do not always stick to field length rules.

The SQL Server plugin originally observed strict rules for field lengths and had to relax them considerably to
accommodate some of the things that people were putting into BaseStation.sqbs. We will have to do the same.

### Flight Recording

The easiest way to record a flight is to periodically write all known values for the flight for every
snapshot.

Many values will either not change between snapshots (e.g. callsign) or will mostly stay within a range of
values (e.g. altitude is usually within a small band of altitudes).

A more efficient approach would be to write one record that holds the initial values and then subsequent
records only record changes from previous records.

Different database and file management systems store nulls in different ways. The ones we care about at the
moment, SQLite and SQL Server, both store nulls reasonably efficiently:

| Engine     | NULL storage requirements |
| ---        | ---  |
| SQLite     | One byte per NULL field |
| SQL Server | None (sets the field's bit in the null bitmap) |

The drawback to this approach is that you may need to read many records if you want to work out what the full
state of the aircraft was at any given time. You cannot look at a record full of changes and know what the current
values for the unchanged fields are without reading backwards through the database until you find the last change
to those fields.

### KeySnapshot

There is one key snapshot record per full snapshot.

A key snapshot is a ```Snapshot``` record where every ```SnapshotAircraft``` child records the full state of the aircraft
list at that moment in time.

The aim with key snapshots is similar to key frames in video. If we only record one initial state and then thousands of deltas
for an aircraft then if you want to know the state of the aircraft one hour into its flight you will need to read and process
up to an hours' worth of snapshots. Key snapshots are periodically recorded in place of a set of deltas to reduce the number
of snapshots that need to be read before you get to the first record that deltas were possibly calculated from.

Key snapshots are also used to implement time-shifting UI. When the user asks to start playback at a point in time the system
goes back to the first key snapshot before that point in time and then reads forward to the last snapshot before the moment in
time that playback is to start from.

| Name          | Type     | Meaning |
| ---           | ---      | --- |
| KeySnapshotID | bigint   | Primary key |
| SnapshotID    | bigint   | ID of the snapshot that contains a full set of aircraft data instead of deltas |

Indexes on primary key plus:

* Unique on ```SnapshotID```.


#### Snapshot

There is one snapshot record per aircraft list capture.

| Name       | Type     | Meaning |
| ---        | ---      | --- |
| SnapshotID | bigint   | Primary key |
| CreatedUTC | datetime | Moment when the snapshot was taken |
| Sequence   | bigint   | Indicates display order in ascending order |
| ReceiverID | bigint   | ID of the SDM record for the receiver being recorded |

Indexes on primary key plus:

* Duplicate on ```CreatedUTC```
* Unique on composite of SnapshotID and Sequence (if they are separate) and duplicate on Sequence (if appropriate).

```Sequence``` is assigned via the database, either by the stored procedure that saves the record or (if ```SnapshotID``` is
guaranteed by the database to always be in ascending order of creation) it can be a calculated field on ```SnapshotID```. No
meaning should be assigned to it other than it can be used to sort snapshots into the correct display order.

In principle ```CreatedUTC``` could be used to sort snapshots into display order. However, the program is expected to run for
extended periods of time and the clock will be periodically adjusted while the program is running. The scheme for only recording
deltas from a previous snapshot only works if the order of snapshots is guaranteed.


#### KeySnapshotView

View used to make accessing key snapshots easier.

| Table       | Field         | Alias |
| ---         | ---           | ---   |
| KeySnapshot | KeySnapshotID | |
| Snapshot    | SnapshotID    | |
| Snapshot    | CreatedUTC    | |
| Snapshot    | Sequence      | |
| Receiver    | ReceiverID    | |
| Receiver    | ReceiverName  | |
| Receiver    | Guid          | ReceiverGuid |


#### AircraftState

There is one aircraft state record per aircraft in an aircraft list.

| Name                | Type       | Nullable | Meaning |
| ---                 | ---        | ---      | --- |
| AircraftStateID     | bigint     | N        | Primary key |
| AircraftID          | bigint     | N        | Unique ID of the  SDM record for the aircraft |
| IsKeyFrame          | bit        | N        | 0 if this is a set of deltas against a previous record, 1 if this is the complete state of the aircraft |
| ReceiverID          | bigint     | Y        | Unique ID of the new SDM record of the receiver than is now providing aircraft messages |
| PositionReceiverID  | bigint     | Y        | Unique ID of the new SDM record of the receiver that is now providing position messages |
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
| VerticalRateTypeID  | bigint     | Y        | Unique ID of the new altitude type SDM record for the vertical rate |
| Squawk              | smallint   | Y        | New squawk expressed as if it were base-10 (e.g. squawk of 0021 is saved as the number 21 instead of more technically correct 17) |
| IdentActive         | bit        | Y        | New ident flag value |
| SignalLevel         | int        | Y        | New signal level for receivers that pass it through |
| RouteID             | bigint     | Y        | Unique ID of the SDM record for the new route being flown by the aircraft |
| IsPositioningFlight | bit        | Y        | New flag indicating that the aircraft is flying a positioning flight |
| IsCharterFlight     | bit        | Y        | New flag indicating that the aircraft is flying a charter flight |
| TransponderTypeID   | bigint     | Y        | Unique ID of the SDM record for the aircraft's transponder type (as inferred from messages received so far) |

Indexes on primary key plus:

* Duplicate on ```AircraftID```
* Duplicate on ```RouteID```

Note that ```AircraftID``` is not nullable and is repeated on every snapshot for a given aircraft, even if
the aircraft record does not change. We need some kind of ID for the aircraft on the table and it would be
nice to avoid update anomalies between this and ```Aircraft```.

Note that any given aircraft (as identified by its Mode-S ICAO ID) can be represented by more than one
```Aircraft``` record for a single flight. It will get a new aircraft record any time there is a change to
one of the aircraft's details, e.g. when an online aircraft detail lookup completes, or values are read
from the database.

```IsKeyFrame``` is a similar idea to ```KeySnapshot```, it indicates whether this is a delta or a complete
representation of the entire set of values for the aircraft. ```KeySnapshot``` snapshots will always create
aircraft snapshots with ```IsKeyFrame``` set, however there are circumstances where key frames will be
created for aircraft that are not in key frames. Typically this will be after an aircraft is removed from
an aircraft list when it times out - a state history will be recorded for its final state and this history
might not be in a key snapshot.

The aircraft ```ReceiverID``` will be different to the parent snapshot's ```ReceiverID``` when the parent
snapshot is a merged feed. The parent records the merged feed ID, each aircraft records the receiver that
is picking up the aircraft. The aircraft receivers can change over the duration of the flight.

The receiver ID and various type IDs are not indexed because their cardinality would be too low. It is
anticipated that these record types will be excluded from auto-cleans.

#### SnapshotAircraft

Links aircraft states to a snapshot.

| Name            | Type   | Meaning |
| ---             | ---    | --- |
| AircraftStateID | bigint | ID of an aircraft in the snapshot's aircraft list |
| SnapshotID      | bigint | ID of the snapshot that is linking to the aircraft state |

Primary key is ```AircraftStateID```.

Duplicates index on ```SnapshotID```.


#### SnapshotAircraftStateView

Joins ```SnapshotAircraft``` and ```AircraftState```.

This is the ```SnapshotID``` from ```SnapshotAircraft``` and every field from ```AircraftState```.


#### Flight

Records the start and end states for a flight. A flight comprises a sequence of messages for an aircraft, starting
with the first received within a session to the last received before the aircraft times out.

| Name                    | Type   | Meaning |
| ---                     | ---    | --- |
| FlightID                | bigint | Primary key |
| InitialAircraftStateID  | bigint | ID of the first state record for the aircraft |
| FinalKeyAircraftStateID | bigint | ID of the last state record that holds a full set of state data for the aircraft |
| FinalAircraftStateID    | bigint | ID of the last state record for the aircraft |

When an aircraft is first detected in a snapshot a new ```Flight``` record is created for it. The initial and both final
states will point to the same ```AircraftSnapshot```.

While an aircraft continues to appear in an aircraft list its ```Flight``` record will be updated to set the ```FinalAircraftStateID```
to the latest state record.

Whenever a key frame is saved the ```Flight``` is updated to record the ID of the key frame.

Nothing special happens when the aircraft times out and is removed from the aircraft list.

This means that to obtain the final state for an aircraft you will have to read the state from ```FinalKeyAircraftStateID```
and then, if ```FinalAircraftStateID``` is different, apply all states.


#### (SDM) Aircraft

There is one aircraft record per version of an aircraft's details.

| Name               | Type           | Meaning |
| ---                | ---            | --- |
| AircraftID         | bigint         | Primary key |
| CreatedUTC         | datetime       | Date record was created |
| Preserve           | bit            | True if the record should be excluded from automatic deletion of old records |
| SHA1Fingerprint    | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| Icao               | char(6)        | Mode-S identitifer expressed as a hex string |
| Registration       | nvarchar(80)   | Aircraft registration |
| ModelID            | bigint         | Unique ID of the SDM model record |
| ConstructionNumber | nvarchar(80)   | Construction number |
| YearBuilt          | nvarchar(80)   | Year built expressed as a string - this is for compatability with BaseStation.sqb sources |
| OperatorID         | bigint         | Unique ID of the SDM operator record |
| CountryID          | bigint         | Unique ID of the SDM country record |
| IsMilitary         | bit            | 1 if the aircraft has a military ICAO code assignment |
| IsInteresting      | bit            | Whether the aircraft is currently flagged as interesting |
| UserNotes          | nvarchar(300)  | Notes user has recorded against the aircraft |
| UserTag            | nvarchar(300)  | Tag user has recorded against the aircraft |

Indexes on primary key plus:

* Duplicate on ```ModelID```
* Duplicate on ```OperatorID```
* Duplicate on ```CountryID```


#### (SDM) Model

There is one model record per version of an aircraft's model details.

| Name                     | Type           | Meaning |
| ---                      | ---            | --- |
| ModelID                  | bigint         | Primary key |
| CreatedUTC               | datetime       | Date record was created |
| Preserve                 | bit            | True if the record should be excluded from automatic deletion of old records |
| SHA1Fingerprint          | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| Icao                     | nvarchar(80)   | ICAO8643 type - see notes elsewhere regarding use of oversized fields |
| ManufacturerID           | bigint         | ID of the SDM manufacturer record |
| ModelName                | nvarchar(80)   | Model name |
| WakeTurbulenceCategoryID | bigint         | ID of the SDM wake turbulence category record |
| EngineTypeID             | bigint         | ID of the SDM engine type record |
| EnginePlacementID        | bigint         | ID of the SDM engine placement record |
| NumberOfEngines          | varchar(2)     | Number of engines expressed as a string (ICAO use a letter code for some engine counts) |
| SpeciesID                | bigint         | ID of the SDM species record |

Indexes on primary key plus:

* Duplicate on ```ManufacturerID```


#### (SDM) Manufacturer

There is one manufacturer record per version of an aircraft's manufacturer details.

| Name             | Type           | Meaning |
| ---              | ---            | --- |
| ManufacturerID   | bigint         | Primary key |
| CreatedUTC       | datetime       | Date record was created |
| Preserve         | bit            | True if the record should be excluded from automatic deletion of old records |
| SHA1Fingerprint  | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| ManufacturerName | nvarchar(80)   | The name of the manufacturer |


#### (SDM) Wake Turbulence Category

There is one WTC record per version of the ```WakeTurbulenceCategory``` enum. These are never auto-cleaned.

| Name                       | Type           | Meaning |
| ---                        | ---            | --- |
| WakeTurbulenceCategoryID   | bigint         | Primary key |
| CreatedUTC                 | datetime       | Date record was created |
| SHA1Fingerprint            | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| EnumValue                  | int            | The enumeration value |
| WakeTurbulenceCategoryName | varchar(80)    | The enumeration name |


#### (SDM) Engine Type

There is one engine type record per version of the ```EngineType``` enum. These are never auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| EngineTypeID    | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| EnumValue       | int            | The enumeration value |
| EngineTypeName  | varchar(80)    | The enumeration name |


#### (SDM) Engine Placement

There is one engine placement record per version of the ```EnginePlacement``` enum. These are never auto-cleaned.

| Name                | Type           | Meaning |
| ---                 | ---            | --- |
| EnginePlacementID   | bigint         | Primary key |
| CreatedUTC          | datetime       | Date record was created |
| SHA1Fingerprint     | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| EnumValue           | int            | The enumeration value |
| EnginePlacementName | varchar(80)    | The enumeration name |


#### (SDM) Species

There is one species record per version of the ```Species``` enum. These are never auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| SpeciesID       | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| EnumValue       | int            | The enumeration value |
| SpeciesName     | varchar(80)    | The enumeration name |


#### (SDM) Operator

There is one operator record per version of an operator.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| OperatorID      | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| Preserve        | bit            | True if the record should be excluded from automatic deletion of old records |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| Icao            | nvarchar(80)   | The three character operator ICAO code, see notes regarding use of oversized strings |
| OperatorName    | nvarchar(100)  | The operator name |


#### (SDM) Country

There is one country record per version of a country. These are not auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| CountryID       | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| CountryName     | nvarchar(80)   | The country's name |


### (SDM) Receiver

There is one record per version of each receiver being recorded. These are not auto-cleaned.

| Name            | Type             | Meaning |
| ---             | ---              | --- |
| ReceiverID      | bigint           | Primary key |
| CreatedUTC      | datetime         | Date record was created |
| SHA1Fingerprint | binary(20)       | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| Guid            | uniqueidentifier | The receiver's GUID. |
| ReceiverID      | int              | The receiver's ID at the time of recording |
| ReceiverName    | nvarchar(255)    | The receiver's name |


#### (SDM) Altitude Type

There is one altitude type record per version of the ```AltitudeType``` enum. These are never auto-cleaned.

| Name             | Type           | Meaning |
| ---              | ---            | --- |
| AltitudeTypeID   | bigint         | Primary key |
| CreatedUTC       | datetime       | Date record was created |
| SHA1Fingerprint  | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| EnumValue        | int            | The enumeration value |
| AltitudeTypeName | varchar(80)    | The enumeration name |


#### (SDM) Speed Type

There is one speed type record per version of the ```SpeedType``` enum. These are never auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| SpeedTypeID     | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| EnumValue       | int            | The enumeration value |
| SpeedTypeName   | varchar(80)    | The enumeration name |


### (SDM) Route

There is one record per version of a route

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| RouteID         | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| Preserve        | bit            | True if the record should be excluded from automatic deletion of old records |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| FromAirportID   | bigint         | ID of the airport that the route starts from |
| ToAirportID     | bigint         | ID of the airport that the route finishes on |

Indexes on primary key plus:

* Duplicate on ```FromAirportID```.
* Duplicate on ```ToAirportID```.



### (SDM) Route Stopover

There is one record per stopover in a route. Stopovers appear between the ```FromAirportID``` and ```ToAirportID``` of the
parent ```Route``` record. They are always associated with a version of a route, the stopovers themselves are not versioned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| RouteStopoverID | bigint         | Primary key |
| RouteID         | bigint         | ID of parent record
| AirportID       | bigint         | ID of the airport that the route stops at |
| Sequence        | int            | Used to sort stopovers into ascending order |

Indexes on primary key plus:

* Duplicate on ```RouteID```.
* Duplicate on ```AirportID```.


#### (SDM) Transponder Type

There is one transponder type record per version of the ```TransponderType``` enum. These are never auto-cleaned.

| Name                | Type           | Meaning |
| ---                 | ---            | --- |
| TransponderTypeID   | bigint         | Primary key |
| CreatedUTC          | datetime       | Date record was created |
| SHA1Fingerprint     | binary(20)     | SHA1 hash of all fields except primary key, ```CreatedUTC``` and ```Preserve``` |
| EnumValue           | int            | The enumeration value |
| TransponderTypeName | varchar(80)    | The enumeration name |

### Searching

The intended uses for the state history of aircraft lists are:

1. Support time shifting displays of all aircraft movements.
2. Support playback of a single aircraft's flight from when it was picked up to when it went out of range.
3. Support historic reports in the style of the BaseStation.sqb reports.

The ```CreatedUTC``` field on ```Snapshot``` lends itself to time shifting displays of aircraft movements.

The ```Flight``` table exists to support playback of a single flight for an aircraft.

Historic BaseStation reports are more difficult because their criteria includes fields that would be punishing
to index here. Indexing anything in ```AircraftState``` needs a lot of forethought and we can't assume that the
database engine / library supports features to improve cardinality like nullable indexes.

One possibility is to add a set of child records to a flight that record searchable values for each flight,
as per BaseStation flight records - minimum altitude, maximum altitude, initial and final callsign and so on.
This will lead to update anomalies between the aircraft state records and the criteria records and there have
been issues in the past where the initial and final values omit transient values that were of interest to the
user (e.g. whether the aircraft passes through a band of altitudes).

It could be that historic reports continue to be the purview of BaseStation.sqb and there is no support for
general queries against state history.

TBD.

### Tying State History and BaseStation.sqb Reports Together

The intention is to add time shifting controls to the main display (which can be configured through the
server) so you can either view current activity or jump back to some point in time and playback from
there.

The intention is also to add a control that lets you search for a known flight and jump to the start of
that flight, and then play back from there.

Another intention, perhaps not for the first release, is to add a report page that brings together all
of the recorded information for a single flight.

Those are fairly straight-forward.

A more complicated requirement is that it would be nice to be able to show the following for a standard
BaseStation.sqb report:

1. Display the actual flight path rather than a straight line between the start and stop points.
2. Show a link that brings up the desktop or mobile window in playback mode starting at the start time
   of the report flight.
3. If / when a page is added to report on a single flight then have a link to that from the report.
   
Both of these will involve tying together the BaseStation.sqb flight record and the corresponding state
```Flight``` record.

There is no guarantee that the BaseStation.sqb that the report is using is being recorded by VRS, it
could be getting recorded by BaseStation.

BaseStation uses local time. The intention is to use UTC with state history.

However, we have to make some assumptions about time. I think we can assume that whatever is recording
BaseStation.sqb is in the same time zone as the VRS instance that is trying to tie flights together.

An aircraft with a given ICAO can only be flying in one place at a time. So if we have a BaseStation
flight then we should be able to convert the local start and end times of the flight to UTC and find
all state ```Flight``` records that have some overlap with the BaseStation flight.
