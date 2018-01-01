# VirtualRadar.Interface.Owin
This namespace holds the interfaces, enumerations and utility
classes used by Virtual Radar Server in its implementation of
an OWIN-compliant web app (see http://owin.org/).

## Dependencies
The following NuGet packages are required (see packages.config
in the root of VirtualRadar.Interface for version numbers):

* Owin
* Microsoft.Owin

## Pipeline
Normally a Microsoft OWIN application uses a *Startup* class
to configure the application. Virtual Radar Server doesn't do
this because it would make life tricky for plugins - middleware
priority is determined by the order in which it is added to
the web app, it would be difficult to interleave middleware from
plugins in with the standard VRS middleware if Startup classes
were used.

Instead your plugin should have a class that implements
[IPipeline](https://github.com/vradarserver/vrs/blob/master/VirtualRadar.Interface/Owin/IPipeline.cs).
The class will need to be registered with
[IPipelineConfiguration](https://github.com/vradarserver/vrs/blob/master/VirtualRadar.Interface/Owin/IPipelineConfiguration.cs).
The class exposes a single method called **Register** that
takes an instance of
[IWebAppConfiguration](https://github.com/vradarserver/vrs/blob/master/VirtualRadar.Interface/Owin/IWebAppConfiguration.cs).
It calls the *IWebAppConfiguration* instance to add its middleware
to the application's pipeline.

At runtime every time a pipeline needs to be initialised (which
will happen at least once, for the main web site, but can also
happen when pipelines are set up to handle requests that are not
sent through the network adapter) a new instance of your pipeline
class is created and called for every pipeline. Each instance is
only ever called once.

If you want to take a look at the application's core pipeline then
that can be found in the **WebSite** library here:
[WebSitePipeline](https://github.com/vradarserver/vrs/blob/master/VirtualRadar.WebSite/WebSitePipeline.cs).

### Middleware Priorities
The
[IWebAppConfiguration](https://github.com/vradarserver/vrs/blob/master/VirtualRadar.Interface/Owin/IWebAppConfiguration.cs)
configuration class insists on priorities being supplied for every
bit of middleware added to the application. Priorities are integers
with lower values called before higher values. If two middleware have
the same priority then the order in which they are called is undocumented.

The priorities used by the standard middleware can be found in
[StandardPipelinePriority](https://github.com/vradarserver/vrs/blob/master/VirtualRadar.Interface/Owin/StandardPipelinePriority.cs).


### Building a standard pipeline
If a plugin wants to request content from the web server without sending
it through the network adapter then it can create a new instance of
[ILoopbackHost](https://github.com/vradarserver/vrs/blob/master/VirtualRadar.Interface/Owin/ILoopbackHost.cs),
call **ConfigureStandardPipeline** on it and then make the appropriate
calls to fetch content.
