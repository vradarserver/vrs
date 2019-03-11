# VirtualRadar.Resources
Where the embedded resources are stored. In old versions of VRS the web site was stored
in resources but for the most part that's gone away, now all that remains are some of the
old images.

In version 3 there was a move away from .NET Framework and on to DotNet Core. At the time
of writing the DNC compiler will only compile string RESX files. It will not correctly
compile binaries (including images).

By the time version 3 came along this library only had images left in its RESX files. These
had to be turned into embedded resources and then the API modified so that they returned
the new image wrapper classes instead of System.Drawing images (which are supported in
DNC 3 but are not cross platform, so we can't use them).