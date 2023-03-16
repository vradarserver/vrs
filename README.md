# Virtual Radar Server .NET Core Edition

## Standard .NET Core stuff that is not being used

### System.Text.Json

`System.Text.Json` is good as far as it goes, but it is a bit Mickey Mouse compared to Newtonsoft.
The lack of support for `System.Runtime.Serialization` properties was the final nail in the coffin.
It's obviously only for new development, they are not interested in supporting ports of existing
code.

I'm sticking with Newtonsoft. It's just easier, no nasty surprises.

## Changes from V2

### Use of double-precision floating point numbers

There are a few properties and variables in the code that have been changed from single-precision
to double-precision floating point numbers. This is due to differences between how .NET Core
handles floating point numbers and how .NET Framework handled them.
