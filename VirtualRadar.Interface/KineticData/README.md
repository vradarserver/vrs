# VirtualRadar.Interface.KineticData

This namespace holds classes associated with the schema for Kinetic Avionic's `BaseStation.sqb`
SQLite local database file. The file contains local aircraft details and a rudimentary flight history
that records the start and end positions of each flight tracked. It is the de facto standard for local
aircraft detail lookups.

## Breaking changes from older versions

In the .NET Framework versions of VRS these classes were in the `VirtualRadar.Interface.BaseStation`
namespace, they all had the prefix `BaseStation` instead of `Kinetic` and a couple of classes had the
word `Upsert` in their name.

In older versions of VRS the repository was implemented using ADO.NET and it exposed a set of insert,
update and delete calls. It also exposed a transaction mechanism.

In the .NET Core world the repository is implemented using Entity Framework, and consequently its
behaviour has changed. In Entity Framework records are retrieved from, added to and deleted from a
`DbSet`. Changes to the set are saved back to the database when `SaveChanges` is called on the context
that owns the set.

Consequently all of the old `Insert` calls are now `Add` calls, and they just attach a new record to a
set. Unlike the old `Insert` call the new record is not immediately saved to the database and the identifier
for the new record is not established by the time the call returns.

The `Delete` calls are now `Remove` calls, and they remove the record from a set. Again, the record is not
actually removed until the changes are saved.

The `Update` calls have been taken out altogether. Any changes to an object retrieved from the database
will be tracked and saved when `SaveChanges` is called.

Because the identifiers are not established by the Add calls you must now make relationships between
records by attaching references to objects instead of setting identifier properties up.

The names of the Insert and Delete calls were changed to force people to revisit their code if they
were writing to the database. Plugins that were just reading `BaseStation.sqb` should not need to change
anything.

## Writes

The repository starts in read-only mode, it will either ignore or throw exceptions (depending on the
call) when calls are made that would change the underlying database. The program has to set
`WriteSupportEnabled` before they make any changes to the database.

The `SaveChanges` call writes all changes back to the database. Calling this when the repository is in
read-only mode will throw an exception.

If you retrieve objects from the database while it is in read-only mode and then change them then the
changes will be lost if you subsequently switch to read-write mode and call `SaveChanges`. This is because
implementations of the repository will generally discard the original context and create a new one when
the read-write mode is changed. If your plugin needs to write to the database then set read-write mode
early on and leave it set.

## Nested SaveChanges calls

There is no nesting of `SaveChanges`.
