# BaseStation
The intention here is to reproduce Kinetic's BaseStation SQLite
database schema here but in T-SQL and in a schema called "BaseStation".

This means that all of the weirdness of Kinetic's schema is
reproduced here but with the following exceptions:

1. BaseStation uses case-insensitive collation. I did not want to impose
any collation sequence on the user so the schema just sticks with the
database default. VRS should work with either case-sensitive or -insensitive
collation.

2. SQLite does not have strict field sizes in the same way that SQL Server
does. Sizes have been suggested by the SQLite schema but some of them are
too small (e.g. around 20 characters for manufacturer name). I have buffed
some of the sizes to fit in with limits used in the VRS SDM site.

3. Following on from the point above, SQLite row IDs are technically 64-bit
integers. However, VRS assumes that integer IDs are 32-bit. I have made them
32-bit in the database.
