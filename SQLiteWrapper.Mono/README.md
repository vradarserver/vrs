# SQLiteWrapper.DotNet
The wrapper around SQLite ADO.Net libraries.

There are two SQLite libraries that VRS has to deal with - the official SQLite ADO.Net library and Mono's
SQLite ADO.Net library. They are very similar but unfortunately they use different names for various
objects:

| SQLite Class | Mono Class | Wrapper Interface |
|--------------|------------|---------------|
| SQLiteConnection | SqliteConnection | ISQLiteConnectionProvider |
| SQLiteConnectionStringBuilder | SqliteConnectionStringBuilder | ISQLiteConnectionStringBuilder |
| SQLiteException | SqliteException | ISQLiteException |

VRS uses interfaces instead of direct references to SQLite objects. There are two wrapper libraries that
implement these interfaces, a .NET version and a Mono version.

This is the Mono version of the SQLite wrapper. It is shipped with Mono builds of VRS.