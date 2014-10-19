This namespace holds classes that raise events when their content changes.
The reason why I am not using the .NET classes that already do the things
these classes do is because not all of them have been fully implemented in
Mono, particularly older versions, and I need something that works across
the board.

They are not intended to support all data-binding situations. They're just
complete enough to support the kind of data binding that I need for the
new (as of 2.0.3) options screen.