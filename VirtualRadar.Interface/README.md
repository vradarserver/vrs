# VirtualRadar.Interface
Where all of the interfaces, helper classes, exception classes, enums etc. live. Note that there are no
implementations of interfaces here, those are spread out throughout the program (mostly in
**VirtualRadar.Library**). However there is some code here, mostly in helper classes.

**Top-level namespace**: Interfaces, enums and classes that don't logically fit elsewhere.

**Adsb**: Interfaces, enums and classes used to decode ADSB messages.

**BaseStation**: Interfaces, enums and classes used to decode Kinetic's port 30003 (aka BaseStation)
messages.

**Database**: Interfaces, enums and classes for reading and writing the BaseStation.sqb database and
the session log database.

**FlightSimulatorX**: Interfaces, enums and classes used to interact with Microsoft's Flight Simulator X
flight sim.

**Listener**: Interfaces, enums and classes used to extract messages from various different formats.
Also includes interfaces for polar plots.

**ModeS**: Interfaces, enums and classes used to decode Mode-S transponder messages (but not the ADSB
payloads of those messages, see **Adsb**).

**Network**: Interfaces, enums and classes used to handle network connections, serial / USB connections and
rebroadcast servers.

**Owin**: Interfaces, enums and classes used to describe and configure OWIN middleware.

**PortableBinding**: Classes that raise events when their content changes. Written because Mono (at least
at the time) did not implement similar classes that VRS relied on.

**Presenter**: Interfaces, enums and classes for objects that hold the logic for user interface forms.

**Settings**: Interfaces, enums and classes for storing and loading program options.

**SQLite**: Interfaces, enums and classes for a wrapper around SQLite libraries. See the *SQLiteWrapper.DotNet*
library documentation for more details.

**StandingData**: Interfaces, enums and classes for reading the standing data SQLite database of routes,
model types and countries.

**View**: Interfaces, enums and classes that describe the program's user interface forms.

**WebServer**: Interfaces, enums and classes for the web server that offers up the VRS web site. Version 3
of VRS no longer has its own hand-rolled web server, instead it uses OWIN and Microsoft's Katana server.
However the interfaces for the old web server still exist, there is a shim layer that exposes the Katana
server via the interfaces.

**WebSite**: Interfaces, enums and classes for the web site that is served by the web server.