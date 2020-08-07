# 3rd party dependencies

## SQLite

There are three sets of SQLite libraries. The mono one is just for linking
and can be ignored, the program will use whatever has shipped with Mono.

The Windows ones come from SQLite's site:

http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki

The files come from two different downloads. Of the myriad of choices you want these:

`$/Dependencies/System.Data.SQLite.dll`
This comes from the **sqlite-netFx46-static-binary-bundle-Win32-yyyy-ver** download. It is
the 32-bit version of the library. The download section title is
**Precompiled Statically-Linked Binaries for 32-bit Windows (.NET Framework 4.6)**.

`$/Dependencies/AnyCPUBuilds`
The `System.Data.SQLite.dll` and the two copies of `SQLite.Interop.dll` in the x64 and x86
subfolders come from the **System.Data.SQLite.Core.*ver*.nupkg** download. It is the core
NuGet package. Unblock, add a .zip extension and fish the files out from the following locations:

`System.Data.SQLite.dll`: $/lib/net46
`x64/SQLite.Interop.dll`: $/build/net46/x64
`x86/SQLite.Interop.dll`: $/build/net46/x86

The download section title is **Official NuGet Packages**, Core is the largest download (~15Mb as of
time of writing).