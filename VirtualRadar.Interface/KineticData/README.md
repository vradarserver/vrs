# VirtualRadar.Interface.KineticData

This namespace holds classes associated with the schema for Kinetic
Avionic's `BaseStation.sqb` SQLite local database file. The file
contains local aircraft details and a rudimentary flight history that
records the start and end positions of each flight tracked. It is the
de facto standard for local aircraft detail lookups.

In the .NET Framework versions of VRS these classes were in the
`VirtualRadar.Interface.BaseStation` namespace, they all had the prefix
`BaseStation` instead of `Kinetic` and a couple of classes had the word
`Upsert` in their name.
