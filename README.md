# Virtual Radar Server .NET Core Edition

## Changes from V2

### Use of double-precision floating point numbers

There are a few properties and variables in the code that have been changed from single-precision
to double-precision floating point numbers. This is due to differences between how .NET Core
handles floating point numbers and how .NET Framework handled them.
