# BaseStationImport Utility

This utility copies records out of one BaseStation database and
into another.

## Build Folder and Debugging
It links to several VRS libraries and makes use of VRS plugins.
Visual Studio will copy the libraries into the utility's build
folder but the plugins will not be available to the utility
from there. There is a post-build step that copies the utility
into the **VirtualRadar** build folder - if you need to debug
the utility then you need to run the utility from there, unless
the bit you're debugging does not involve a plugin.

## Version Number
The utility's version number needs to be kept in step with
**VirtualRadar.exe**'s version number, otherwise it will not be
able to load the plugins.

## Plugin IDs
The *Main* function loads all of VRS' plugins and then picks
out the database ones for initialisation. It identifies the
database plugins by a case sensitive examination of the *Id*
property. If the plugin IDs are changed then this test will
need to be updated.