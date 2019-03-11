# VirtualRadar.Localisation
The localised strings for the program along with helper classes to make localisation a little easier.

**Localiser**: Helper methods to localise forms and controls, mostly by looking for strings delimited
by double-colons, assuming that those strings are resource key names and substituting in the
translation
of that key name. Can work with any set of resource strings.

**Localise**: A static instance of *Localiser* that only works with the resource strings in
*VirtualRadar.Localisation*.

**LocalisedStringsMap**: Manages the lookup of localised strings read from a resource file.

## Version 3
Version 3 of VRS saw the move away from .NET Framework to DotNet Core 3. The DNC compiler supports
building string RESX files and they behave in the same way as before, the only changes needed to
this library for DNC 3 are to remove the WinForms references (DNC3 only supports WinForms under
Windows, we need a GUI that works on all platforms).

DNC 3 does not support embedding binaries from RESX files, only strings. However, this library did
not have any binaries in it. For details on what had to be done for binaries see
**VirtualRadar.Resources**.
