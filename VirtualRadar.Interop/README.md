# VirtualRadar.Interop
Where the Win32 interop classes live.

**Console**: Interop for creating Windows consoles. This is not exposed through an interface. It's only
used by *VirtualRadar.exe* when in headless mode on Windows machines. Linux doesn't need it.

**TcpKeepAlive**: Interop for controling TCP keep-alive packets on sockets. Used by network connectors
to switch keep-alive packets on. Mono builds of VRS do not support keep-alive on TCP connections.

**Window**: Exposes *SendMessage*. I think this is just used by the *Flight Simulator X* stuff, FSX needs to
have messages sent to some window handle. Mono builds of VRS do not support any FSX features.