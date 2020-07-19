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

### Recording Deltas

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


### Usage

VRS will need to be able to quickly retrieve state histories for the following situations:

1. Time shifted playback of all aircraft seen on a receiver. The user interface will offer up UI to start playback
   of saved history at a given date and time. The UI then displays the recorded state one list at a time.
   
   The ```CreatedUTC``` field on ```AircraftList``` supports the retrieval of aircraft lists for playback.
   
2. Time shifted playback of a single flight. Not sure if this will be in the first release but it would be possible
   via the ```Flight``` table and the ```CreatedUTC``` field on ```AircraftList```.
   
3. Display the actual flight track on BaseStation.sqb reports. See section re. tying session history flights
   to BaseStation.sqb files later in this document.
   
4. Use state history as a source for the standard reports. Reports that look to the old Flights table for criteria
   should map onto the ```FlightView``` view fairly easily, but those that searched ```Aircraft``` might get
   tricky. A flight can be comprised of any number of versions of a single aircraft. However, it should be possible.
   
5. Use state history for sundry aircraft list values that normally come from BaseStation.sqb - in particular the number
   of flight records previously seen for an aircraft. Given this would use the aircraft's ICAO that should not be
   a problem, ```FlightView``` exposes the ICAO.
   
6. Add a new report that can only run from state history that shows the changes in state over the course of a flight,
   for example the changes in altitude and when they occur, changes in speed, full track etc. This would be showing
   data for a single ```Flight```.
   
### Common Schema

#### VRSSession

VRS creates one record every time it starts up. It can be used to detect whether records are associated with a live
VRS session or a historic session. This could be of interest to utilities that are trying to clean up records left
behind by a previous session that might have stopped abruptly. It can also be used to detect and work around issues
with historical states that might be missing certain features added in a later schema.

SDM records do not need to refer to a VRS session. If new fields are added in a later schema then the existing records
will have SHA1 fingerprints that do not include the new fields, so they will not be valid and new records will be
created as required. This is by design, it preserves SDM records as they were for a given state record.

| Name              | Type     | Meaning |
| ---               | ---      | --- |
| VRSSessionID      | bigint   | Primary key |
| CreatedUTC        | datetime | Moment when the record was created |
| DatabaseVersionID | bigint   | ID of the schema that was in force when the session was running |


#### DatabaseVersion

Records the version of the schema in force at the time a session started. New records are written every time the schema
is updated. The IDs correspond to an enum in code.

| Name              | Type     | Meaning |
| ---               | ---      | --- |
| DatabaseVersionID | bigint   | Primary key - corresponds to a code enum |
| CreatedUTC        | datetime | Moment when the record was created / schema updated |


### Aircraft List Schema

#### AircraftList

There is one aircraft list record per aircraft list capture. The intention here is to record values that are common
to all ```AircraftState``` records in the list.

| Name                   | Type     | Meaning |
| ---                    | ---      | --- |
| AircraftListID         | bigint   | Primary key |
| VRSSessionID           | bigint   | ID of the session that the aircraft list was created in |
| CreatedUTC             | datetime | Moment when the record was first created |
| UpdatedUTC             | datetime | Moment when the record was last updated |
| ReceiverID             | bigint   | ID of the SDM record for the receiver being recorded |

Indexes on primary key plus:

* Duplicate on ```CreatedUTC```.

VRS will ensure that the ```CreatedUTC``` for a new record is **always** later than the CreatedUTC of the last
record written for any receiver ID with the same receiver GUID. The system will try to use the current time but
if a system clock correction has moved the clock to a point earlier than the latest saved CreatedUTC then the new
CreatedUTC will be one millisecond later than the previous CreatedUTC.

#### AircraftListView

View joining ```AircraftList``` to ```Receiver```. Contains all of the fields from ```AircraftListView``` plus
the receiver's GUID.

#### KeyAircraftList

There is one key aircraft list record per fully populated aircraft list.

A fully populated list is an ```AircraftList``` record where every ```AircraftState``` child contains the full state of the aircraft
list at that moment in time (i.e. its ```IsDelta``` field is 0 / false).

The aim with key lists is similar to key frames in video. If we only record one initial state followed by thousands of deltas
for an aircraft then if you want to know the state of the aircraft one hour into its flight you will need to read and process
up to an hours' worth of state records. Key states are periodically recorded in place of a set of deltas to reduce the number
of records that need to be read to calculate the end state from deltas.

Key records are also used to implement time-shifting UI. When the user asks to start playback at a point in time the system
goes back to the first key record before that point in time and then reads forward to the last record before the moment that
playback is to start from.

The reason for keeping a separate table of key aircraft lists is to make searches for key lists as efficient as possible.

| Name           | Type     | Meaning |
| ---            | ---      | --- |
| AircraftListID | bigint   | Primary key, ID of the fully-populated aircraft list |


#### KeyAircraftListView

All of the fields from ```AircraftListView``` joined against ```KeyAircraftList``` to filter out delta aircraft lists.

The view itself does not join to AircraftListView, it just builds on the definition for that view.


#### AircraftState

There is one aircraft state record per aircraft in an aircraft list.

To keep life simple VRS does not record aircraft lists as soon as the snapshot is taken. Rather it builds up a set of snapshot records
and holds them all in memory until a key list needs to be saved. It then saves all of the delta aircraft lists and the key list in one
operation.

This means that the latest aircraft list - and therefore the latest ```AircraftState``` - is *always* a complete state and
never a delta. This is an important point to bear in mind when considering the ```Flight``` table later on.


| Name                | Type       | Nullable | Meaning |
| ---                 | ---        | ---      | --- |
| AircraftStateID     | bigint     | N        | Primary key |
| AircraftListID      | bigint     | N        | ID of parent aircraft list |
| FlightID            | bigint     | N [1]    | ID of parent flight |
| AircraftID          | bigint     | N        | Unique ID of the  SDM record for the aircraft |
| IsDelta             | bit        | N        | 1 if this is a set of deltas against a previous record, 0 if this is the complete state of the aircraft |
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

* Duplicate on ```AircraftListID```
* Duplicate on ```AircraftID```
* Duplicate on ```RouteID```

[1] There is a chicken and egg situation with ```FlightID```. The flight has the IDs of the first and last
```AircraftState```s in the flight. The ```AircraftState``` has a link to the flight. They both can't
be non-nullable. So the first aircraft state is created without a flight, then its ID is used to create
the flight record, then it is updated to set the parent flight ID. All this is done in one transaction
so that the temporarily null FlightID parent is not visible to the rest of the world.

See ```Flight``` re. the update anomaly caused by it containing references to ```AircraftState```.

Note that any given aircraft (as identified by its Mode-S ICAO ID) can be represented by more than one
```Aircraft``` record for a single flight. It will get a new aircraft record any time there is a change to
one of the aircraft's details, e.g. when an online aircraft detail lookup completes, or values are read
from the database. This means that you cannot just search against ```AircraftID``` if you want to find
all records for a given aircraft. You need to join against ```Aircraft``` and search against the
```Aircraft```'s ICAO. Use ```AircraftStateView``` to isolate your searches from changes to ```AircraftID```
over the lifetime of a flight.

The aircraft ```ReceiverID``` will be different to the parent aircraft list's ```ReceiverID``` when the
parent is a merged feed. The parent records the merged feed ID, each aircraft records the receiver that
is picking up the aircraft. The aircraft receivers can change over the duration of the flight.

The receiver ID and various type IDs are not indexed because their cardinality would be too low. It is
anticipated that these record types will be excluded from auto-cleans so the RDBMS will not need to
search for existing instances before deletion.


#### AircraftStateView

An inner join between ```AircraftState``` and ```Aircraft``` exposing every field from ```AircraftState```
and ```Icao``` from ```Aircraft```.


#### AircraftStateless

Keeps a record of aircraft whose state records have been deleted from a given aircraft list. These aircraft
must not be allowed to time out during playback of aircraft lists, they can be considered to be completely
empty deltas from the last known state.

```AircraftStateless``` records are not considered when searching for empty aircraft lists to delete. This
means that when you have an ```AircraftState``` record that is an empty delta - i.e. there are no changes -
you must still write an ```AircraftState``` record. If you were to substitute it with one of these stateless
records instead then the entire aircraft list could be prematurely removed in the next trim.

| Name                | Type       | Meaning |
| ---                 | ---        | --- |
| AircraftListID      | bigint     | ID of parent aircraft list |
| AircraftID          | bigint     | ID of the SDM record for the aircraft |
| FlightID            | bigint     | ID of the flight record for the aircraft |

The primary key is a composite of ```AircraftListID``` and ```AircraftID```, in that order.

There is a duplicates index on ```FlightID```.


#### AircraftStatelessView

An inner join between ```AircraftStateless``` and ```Aircraft``` that adds the ```Icao``` from ```Aircraft```.


#### Flight

Records the start and end states for a flight. A flight comprises a sequence of messages for an aircraft, starting
with the first received within a session to the last received before the aircraft times out.

| Name                    | Type     | Meaning |
| ---                     | ---      | --- |
| FlightID                | bigint   | Primary key |
| UpdatedUTC              | datetime | Date of last update |
| Preserve                | bit      | 1 if the flight should be excluded from auto-clean |
| IntervalSeconds         | integer  | The nominal number of seconds between the flight's snapshots |
| InitialAircraftStateID  | bigint   | ID of the first state record for the aircraft |
| FinalAircraftStateID    | bigint   | ID of the last state record for the aircraft |

When an aircraft is first detected in an aircraft list a new ```Flight``` record is created for it. All state IDs will
point at the same ```AircraftState``` record.

Every time VRS adds more ```AircraftState``` records it will update the ```FinalAircraftStateID``` field.

It is expected that VRS will save ```AircraftState``` records in bursts, and that each burst of records will finish
with a key frame, i.e. a state record where ```IsDelta``` will be 0. If this expectation goes ahead then both state IDs
will always point at complete records.

The ```AircraftState``` contains a reference back to the flight. This means that it is possible for the ```InitialAircraftStateID```
to point to an aircraft state that is not the first for this flight (in ascending order of parent aircraft list) and
likewise ```FinalAircraftStateID``` might not point to the last record.

I have considered putting a sequence number onto ```AircraftState``` which starts at 1 and increments on each state
written for the flight. The ```FlightView``` (see later) could report ```InitialAircraftStateID``` as the ID of the
record with the lowest flight sequence and ```FinalAircraftStateID``` as the record with the highest flight sequence.

This would work but it is slower than direct access to the ID that we've spec'd here. The reason why we have the first
and last state IDs is so that we can implement a view that broadly reproduces the ```Flight``` record from BaseStation.sqb,
it can be used to search for flights matching criteria. The ```FlightView``` needs to be fast to read, as fast as possible.

So - in the trade off between speed and data purity we are going with speed, just for the sake of reporting.

If it is necessary then a database check utility can easily tell whether the initial and final state IDs here are actually
pointing at the first and last records for the flight.

```Preserve``` overrides all other auto-clean considerations. Preserved flights are never trimmed or deleted.

```IntervalSeconds``` starts at the configured interval between snapshots in the flight. If the interval is reconfigured
by the user during a flight recording then the largest value is recorded - e.g. if the interval between snapshots is
10 seconds when a flight starts and then the user changes it to 5 seconds mid-flight then intervals between snapshots for
the flight will be a mixture of 5 and 10 seconds apart, but the flight will declare that they are 10 seconds apart.

If the flight only contains the start and end states for a flight then ```IntervalSeconds``` will be 2^31 (more than 2 billion).


#### FlightView

Left outer joins ```Flight``` records to two ```AircraftState``` records, one for the initial state and one for the final state.
In both cases the join is where the ```IsDelta``` flag is 0 / false.


#### (SDM) Aircraft

There is one aircraft record per version of an aircraft's details.

| Name               | Type           | Meaning |
| ---                | ---            | --- |
| AircraftID         | bigint         | Primary key |
| CreatedUTC         | datetime       | Date record was created |
| SHA1Fingerprint    | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
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

* Duplicate on ```Icao```
* Duplicate on ```Registration```
* Duplicate on ```ModelID```
* Duplicate on ```OperatorID```
* Duplicate on ```CountryID```


#### (SDM) Model

There is one model record per version of an aircraft's model details.

| Name                     | Type           | Meaning |
| ---                      | ---            | --- |
| ModelID                  | bigint         | Primary key |
| CreatedUTC               | datetime       | Date record was created |
| SHA1Fingerprint          | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| Icao                     | nvarchar(80)   | ICAO8643 type - see notes elsewhere regarding use of oversized fields |
| ManufacturerID           | bigint         | ID of the SDM manufacturer record |
| ModelName                | nvarchar(80)   | Model name |
| WakeTurbulenceCategoryID | bigint         | ID of the SDM wake turbulence category record |
| EngineTypeID             | bigint         | ID of the SDM engine type record |
| EnginePlacementID        | bigint         | ID of the SDM engine placement record |
| NumberOfEngines          | varchar(2)     | Number of engines expressed as a string (ICAO use a letter code for some engine counts) |
| SpeciesID                | bigint         | ID of the SDM species record |

Indexes on primary key plus:

* Duplicate on ```Icao```
* Duplicate on ```ManufacturerID```
* Duplicate on ```ModelName```


#### (SDM) Manufacturer

There is one manufacturer record per version of an aircraft's manufacturer details.

| Name             | Type           | Meaning |
| ---              | ---            | --- |
| ManufacturerID   | bigint         | Primary key |
| CreatedUTC       | datetime       | Date record was created |
| SHA1Fingerprint  | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| ManufacturerName | nvarchar(80)   | The name of the manufacturer |

* Duplicate on ManufacturerName.


#### (SDM) Wake Turbulence Category

There is one WTC record per version of the ```WakeTurbulenceCategory``` enum. These are never auto-cleaned.

| Name                       | Type           | Meaning |
| ---                        | ---            | --- |
| WakeTurbulenceCategoryID   | bigint         | Primary key |
| CreatedUTC                 | datetime       | Date record was created |
| SHA1Fingerprint            | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC```  |
| EnumValue                  | int            | The enumeration value |
| WakeTurbulenceCategoryName | varchar(80)    | The enumeration name |

It is anticipated that there will be too few records in the table to warrant an index on the name.


#### (SDM) Engine Type

There is one engine type record per version of the ```EngineType``` enum. These are never auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| EngineTypeID    | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC```  |
| EnumValue       | int            | The enumeration value |
| EngineTypeName  | varchar(80)    | The enumeration name |

It is anticipated that there will be too few records in the table to warrant an index on the name.


#### (SDM) Engine Placement

There is one engine placement record per version of the ```EnginePlacement``` enum. These are never auto-cleaned.

| Name                | Type           | Meaning |
| ---                 | ---            | --- |
| EnginePlacementID   | bigint         | Primary key |
| CreatedUTC          | datetime       | Date record was created |
| SHA1Fingerprint     | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| EnumValue           | int            | The enumeration value |
| EnginePlacementName | varchar(80)    | The enumeration name |

It is anticipated that there will be too few records in the table to warrant an index on the name.


#### (SDM) Species

There is one species record per version of the ```Species``` enum. These are never auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| SpeciesID       | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| EnumValue       | int            | The enumeration value |
| SpeciesName     | varchar(80)    | The enumeration name |

It is anticipated that there will be too few records in the table to warrant an index on the name.


#### (SDM) Operator

There is one operator record per version of an operator.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| OperatorID      | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| Icao            | nvarchar(80)   | The three character operator ICAO code, see notes regarding use of oversized strings |
| OperatorName    | nvarchar(100)  | The operator name |

* Duplicate index on Icao.
* Duplicate index on OperatorName.


#### (SDM) Country

There is one country record per version of a country. These are not auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| CountryID       | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| CountryName     | nvarchar(80)   | The country's name |

* Duplicate index on CountryName.


#### (SDM) Receiver

There is one record per version of each receiver being recorded. These are not auto-cleaned.

| Name            | Type             | Meaning |
| ---             | ---              | --- |
| ReceiverID      | bigint           | Primary key |
| CreatedUTC      | datetime         | Date record was created |
| SHA1Fingerprint | binary(20)       | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| Guid            | uniqueidentifier | The receiver's GUID. |
| ReceiverID      | int              | The receiver's ID at the time of recording |
| ReceiverName    | nvarchar(255)    | The receiver's name |

It is anticipated that there will be too few records in the table to warrant an index on the name.


#### (SDM) Altitude Type

There is one altitude type record per version of the ```AltitudeType``` enum. These are never auto-cleaned.

| Name             | Type           | Meaning |
| ---              | ---            | --- |
| AltitudeTypeID   | bigint         | Primary key |
| CreatedUTC       | datetime       | Date record was created |
| SHA1Fingerprint  | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| EnumValue        | int            | The enumeration value |
| AltitudeTypeName | varchar(80)    | The enumeration name |

It is anticipated that there will be too few records in the table to warrant an index on the name.


#### (SDM) Speed Type

There is one speed type record per version of the ```SpeedType``` enum. These are never auto-cleaned.

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| SpeedTypeID     | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| EnumValue       | int            | The enumeration value |
| SpeedTypeName   | varchar(80)    | The enumeration name |

It is anticipated that there will be too few records in the table to warrant an index on the name.


#### (SDM) Route

There is one record per version of a route

| Name            | Type           | Meaning |
| ---             | ---            | --- |
| RouteID         | bigint         | Primary key |
| CreatedUTC      | datetime       | Date record was created |
| SHA1Fingerprint | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| FromAirportID   | bigint         | ID of the airport that the route starts from |
| ToAirportID     | bigint         | ID of the airport that the route finishes on |

Indexes on primary key plus:

* Duplicate on ```FromAirportID```.
* Duplicate on ```ToAirportID```.



#### (SDM) Route Stopover

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
| SHA1Fingerprint     | binary(20)     | SHA1 hash of all fields except primary key and ```CreatedUTC``` |
| EnumValue           | int            | The enumeration value |
| TransponderTypeName | varchar(80)    | The enumeration name |

It is anticipated that there will be too few records in the table to warrant an index on the name.

### Auto-cleaning Aircraft Lists

The state history for aircraft lists can easily consume a lot of disk space very quickly.

SQLite files have an upper limit of ~140 TB.

SQL Server has an upper limit of ~524 TB but Express has an upper limit of 10 GB.

The intention is to support the gradual trimming back and eventual removal of snapshot records for aircraft lists.

#### Trimming and Deleting Flights

Configuration options will let users control the interval between recorded snapshots for a flight. These intervals
are only allowed to get longer over time. The default schedule might look like this:

| Threshold          | Interval |
| ---                | ---      |
| Up to 2 weeks ago  | 10 seconds, key every 2 minutes |
| Up to 1 month ago  | 1 minute |
| Up to 6 months ago | 5 minutes |
| Up to 1 year ago   | 10 minutes |
| Up to 3 years ago  | Just start and end |
| Over 3 years ago   | Delete |

The ```Preserved``` field on ```Flight``` indicates that the flight cannot be trimmed or deleted. Flights with this
flag set are ignored.

##### Trimming Flights

The ```IntervalSeconds``` field on ```Flight``` declares the maximum period between each snapshot for the flight.
When trimming flights the system fetches the ```CreatedUTC``` for the flight's initial aircraft state's parent
list and from that it calculates the minimum allowable interval between the flight's snapshots from configuration.

Flights that already at or above the minimum interval are ignored, they do not need to be trimmed.

When the flight needs to be trimmed the system loads all ```AircraftState```s for the flight. 

The first state record is always preserved. The system takes the created time for this record (as stored on its parent aircraft list) and adds the interval. This gives us a target CreatedUTC.

It finds the nearest aircraft list to that CreatedUTC. The aircraft list can be before or after the target, it
just needs to be closest. The aircraft state hanging from the closest aircraft list is the **Preserved State**.

If there is no qualifying aircraft list then the Preserved State will be the last ```AircraftState``` for the
flight.

It then finds the last key aircraft list on or before the Preserved State's aircraft list. This is the **Preserved
Key State**. It is possible for the Preserved State and Preserved Key State to be the same state record. It is
also possible that there is no Preserved Key State, in which case it is null.

If Preserved State and the first state are the same then there is nothing to trim, we stop here. There was only
one state record for the aircraft.

If Preserved Key State is null then all states after the first state up to (but not including) the Preserved State
are deleted from ```AircraftState```.

Otherwise all states after the first state and before the Preserved Key State are deleted from ```AircraftState```,
along with all states after Preserved Key State and up to (but not including) the Preserved State.

When states are deleted from ```AircraftState``` their corresponding ```AircraftID```s are added to ```AircraftStateless```,
one record per list that an aircraft state was removed from. All deletions from ```AircraftState``` and
```AircraftStateless``` should be done in a transaction so that you cannot have the same aircraft appear simultaneously
in both the State and Stateless tables for a list.

If Preversed Key State is null, and states were removed between the first state and the Preserved State, then
Preserved State is updated so that it is now a delta from the first state record.

Likewise if Preserved Key State is not null, and is not the same as Preserved State, and deltas were removed between the
Preserved Key State and the Preserved State, then the Preserved State is updated so that it is now a delta from the
Preserved Key State.

If no state records had to be removed then there is no need to update the Preserved State record.

The code then repeats this process with the Preserved State taking the place of the initial state for each iteration
through the loop.

Finally the ```Flight``` record can be updated with the new ```IntervalSeconds``` value.


##### Deleting Flights

When a flight is deleted the following records need to be removed:

* Flight
* All AircraftState records for the flight.
* All AircraftStateless records for the flight.


#### Trimming Aircraft Lists

Any ```AircraftList``` records that have no child ```AircraftState``` records can be deleted. The order of operations should
be:

* Delete parent KeyAircraftList
* Delete all child AircraftStateless
* Delete the AircraftList

All of this needs doing within a transaction.


#### Trimming SDM Records

Periodically defunct SDM records can be removed from the system. SDM records for enum values or configured records that
are unlikely to be created in large numbers (e.g. receivers) do not need to be trimmed. However, aircraft versions for
flights that no longer exist, routes for flights that no longer exist etc. - they can all be deleted.

Deleting SDM records is a little tricky because VRS will be caching SDM record IDs. Care would need to be taken to ensure
that VRS does not write a record referring to an SDM record at the same time that the SDM record is deleted.


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
