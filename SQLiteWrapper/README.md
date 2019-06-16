# SQLiteWrapper
The wrapper around SQLite ADO.Net libraries.

This became much less relevant with the move to dotnet core, so much so that
I think I can probably get rid of it. However, in the interests of only
changing one thing at a time I'll keep it for now.

Below is the raison d'etre for the library.

## History

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

This is the .NET x86 version of the SQLite wrapper. It is shipped with 32-bit Windows builds of VRS.