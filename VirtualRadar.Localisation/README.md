# VirtualRadar.Localisation
The localised strings for the program along with helper classes to make localisation a little easier.

**Localiser**: Helper methods to localise forms and controls, mostly by looking for strings delimited
by double-colons, assuming that those strings are resource key names and substituting in the translation
of that key name. Can work with any set of resource strings.

**Localise**: A static instance of *Localiser* that only works with the resource strings in *VirtualRadar.Localisation*.

**LocalisedStringsMap**: Manages the lookup of localised strings read from a resource file.
