# VirtualRadar.Database
This holds implementations of database-related interfaces.

There's been a few different approaches to handling SQLite databases over the lifetime of the program. Modern
code, or code that has had to be changed recently, uses *Dapper*. Older code that hasn't been touched in a
while uses a low level wrapper around ADO.Net calls that mostly lives in the **Sql** class.

**Top-level namespace**: Holds classes that either don't logically fit elsewhere or the other database
namespaces rely upon. Most of the classes are part of the legacy **Sql** ADO.Net wrapper code that one or
two of the older bits of code still use.

**AircraftOnlineLookupCache**: If the user does not have the database writer plugin installed, or they elect
not to store online aircraft lookup results in *BaseStation.sqb*, then the lookup results are stored in a
standalone SQLite database file. This namespace contains the classes for handling this cache of lookup results.

Uses *Dapper*.

**BaseStation**: Classes for reading (and optionally writing) Kinetic's de-facto standard *BaseStation.sqb*
database file.

Uses *Dapper*.

**Log**: Despite the name this is not the implementation of **ILog** but rather **ILogDatabase**. ILogDatabase
records connections to the web site by clients.

Uses the old **Sql** wrapper code.

**StandingData**: VRS does not ship with a full set of standing data. It is instead downloaded when the
program first starts up and then once a day (but only if the version on the main web site is different to
the version last downloaded). It is an SQLite file containing details of routes, model ICAO types, countries
etc.

These are the classes for downloading and reading the standing data file. Some standing data can be overridden
with local text files, code for that is also in this namespace.

Uses *Dapper*.

**Users**: The users database is a SQLite database stored in the configuration folder. It holds the users
that are shown and edited in the options screen.

Uses the old **Sql** wrapper code.
