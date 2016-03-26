This folder contains the source for third-party libraries and utilities that I couldn't
directly use because either they do not target .NET 3.5 or I needed to make changes to
them.

KdTreeLib
=========
Home page: https://github.com/codeandcats/KdTree
Backported from .NET 4 to .NET 3.5, otherwise unchanged.

This is shipped with VRS, VirtualRadar.Library uses it to store the air pressures reported
from weather stations. It's used to search for the weather station closest to an aircraft's
position.


TypeLite.Net35
==============
Home page: http://type.litesolutions.net/
Backported from .NET 4 to .NET 3.5. Added an AlternateGenerator to optionally produce
Knockoutout observable versions of the generated interfaces. I seem to remember adding
something else as well, but it was a few months ago now and I've forgotten. I would
probably have marked my changes with an AGW comment somewhere :)

This isn't shipped with VRS, it's a development-time tool used to generate TypeScript
interfaces for VRS models.


