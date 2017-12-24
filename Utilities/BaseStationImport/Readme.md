# BaseStationImport Utility

This utility copies records out of one BaseStation database and
into another.

## Build Folder and Debugging
The program links to several VRS libraries and makes use of VRS plugins.
Visual Studio will copy the libraries into the utility's build
folder but the plugins will not be available to the utility
from there. There is a post-build step that copies the utility
into the **VirtualRadar** build folder - if you need to debug
the utility then you need to run the utility from there, unless
the bit you're debugging does not involve a plugin.

## Version Number
The utility's version number needs to be kept in step with
**VirtualRadar.exe**'s version number, otherwise it will not be
able to load the plugins.

## Plugin IDs
The *Main* function loads all of VRS' plugins and then picks
out the database ones for initialisation. It identifies the
database plugins by a case sensitive examination of the *Id*
property. If the plugin IDs are changed then this test will
need to be updated.

## Usage
````
usage: BaseStationImport <command> [options]
Commands:
  -import                  Copy BaseStation data from one database to another
  -schema                  Create or update the schema

Parameter types:
  <date>                   A date in yyyy-MM-dd ISO format
  <dbtype>                 One of SQLite | SqlServer
  <file|con>               A filename (SQLite only) or connection string
  <timeout>                A timeout in seconds, 0 = disable timeout

-import options:
  -src <dbtype> <file|con> The source database and how to connect to it
  -dst <dbtype> <file|con> The target database and how to connect to it
  -srcTimeout <timeout>    An optional timeout to apply to the source
  -dstTimeout <timeout>    An optional timeout to apply to the target
  -noSchema                Do not create or update schema on target before import
  -noAircraft              Do not import aircraft
  -noLocations             Do not import locations
  -noSessions              Do not import sessions
  -noFlights               Do not import flights
    -from <date>           Earliest flight to import from [0001-01-01]
    -to <date>             Latest flight to import to [9999-12-31]

-schema options:
  -dst <dbtype> <file|con> The database to apply the schema to
  -dstTimeout <timeout>    An optional timeout to apply to the target

Common options:
  -verbose                 Show more information in error messages
````
