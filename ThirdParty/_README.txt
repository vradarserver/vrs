This folder contains the source for third-party libraries and utilities that I couldn't
directly use because either they do not target .NET 3.5 or I needed to make changes to
them.


TypeLite.Net35
==============
Home page: http://type.litesolutions.net/
Backported from .NET 4 to .NET 3.5. Added an AlternateGenerator to optionally produce
Knockout observable versions of the generated interfaces. I seem to remember adding
something else as well, but it was a few months ago now and I've forgotten.

This isn't shipped with VRS, it's a development-time tool used to generate TypeScript
interfaces for VRS models.


