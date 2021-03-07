# VirtualRadar.Interface.Drawing
This originally came about during work to build version 3 under
DotNet Core. DNC has no cross platform System.Drawing so VRS needs
to arrange its code in such a way that all graphics operations can
be switched between different libraries depending on the platform.
Under Windows we want to continue using System.Drawing but under
Linux we'd want to use a cross-platform alternative.

As of time of writing there is no DotNet Core support for version
3. It was ported ahead of time because Mono 6's Image.Clone
implementation was causing issues and I needed somewhere where I
could put the workaround that all of VRS would call when it wanted
to clone an image. I already had that in the DNC drawing wrapper,
so I figured I'd reuse it.