# BaseStation
The intention here is to have a pretty much perfect reproduction
of Kinetic's BaseStation SQLite database schema here but in T-SQL
and in a schema called "BaseStation".

This means that all of the weirdness of Kinetic's schema is
reproduced here. I could have tried to accomplish this with a nice
underlying schema and then views over the top that reproduces the
ugly bits, but that would only make life more complicated than it
already is. I'm just going to reproduce Kinetic's schema.

However, I have made an exception for collation. Strictly speaking
BaseStation.sqb uses case-insensitive collation sequences. The code
assumes the use of case-insensitive collation sequences. But the idea
with the plugin is that the user can embed the schemas that it uses
within a larger database, and if we enforced a particular collation
sequence then it would make life difficult for anyone doing that.
The schema tries to be agnostic and just use whatever the defaults are.