# VirtualRadar.Interface.Drawing
This originally came about as a part of adding support to version 3
for DotNet Core builds. I have ported it to version 2 because Mono's
implementation of System.Drawing occasionally has problems that I
need to add workarounds for, and it's difficult to do that if there
isn't a single point (under my control) for each drawing function
that I can add code to.