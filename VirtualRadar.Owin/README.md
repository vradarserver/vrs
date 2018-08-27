# VirtualRadar.Owin
Implementations of interfaces in **VirtualRadar.Interface.Owin**

**Configuration**: Implementations that deal with the configuration of the middleware. Most of the interfaces
implemented here are singletons.

**Middleware**: Implementations of OWIN middleware. Most of these interfaces are not singletons.

**StreamManipulator**: Implementations of classes that modify responses for text files before they are sent
back to the browser (e.g. substituting references to the correct map plugin into HTML headers etc.).