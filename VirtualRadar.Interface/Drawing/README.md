# VirtualRadar.Interface.Drawing
As a part of the move from .NET Framework to DotNet Core 3.0 the program has to
move away from using System.Drawing because there is no cross platform equivalent
under DotNet Core.

As of time of writing the intention is to switch over to SixLabors' ImageSharp
library. However, just in case that doesn't pan out, or I need to switch to a
different library in the future, I'm going to put all of the image handling
stuff into its own bunch of classes and have abstractions for the objects that
image handlers typically need.

This namespace contains all of the image library wrappers and abstractions.
