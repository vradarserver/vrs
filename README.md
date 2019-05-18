# Virtual Radar Server .NET Core 3 Branch
You will need Visual Studio 2019 to build the program. The free Community version of Visual Studio will work just
fine. You can download the installer for it from here:

* **Visual Studio Installer**: https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx

You will need to have installed the latest .NET Core 3.0 preview from here:

https://dotnet.microsoft.com/download/dotnet-core/3.0

By default Visual Studio will not use preview versions of .NET core. You can fix this
by going into **Tools | Options | Projects and Solutions | .NET Core** and ticking the
````Use previews of the .NET Core SDK```` option.


## Earlier versions of Visual Studio / Visual Studio Code
.NET Core 3.0 projects cannot be built with versions of Visual Studio earlier than 2019.

The intention is to support builds with Visual Studio Code but while this is a work-in-progress
it is only being built with VS2019.


## TypeScript

The projects reference the TypeScript 2.3.3 NuGet package so they should build correctly. The downside is that you'll see a lot of erroneous TypeScript errors coming out
of Intellisense. This is because Intellisense is using a later version of TypeScript and TypeScript is not good at being backwards
compatible.

You can safely ignore the Intellisense errors, the build will be fine.


## Overview
* **InterfaceFactory**: Exposes a static class called *Factory* that can create new instances of classes that implement interfaces.
Used everywhere.
* **VirtualRadar.Interface**: Where all of the interfaces, enums, attributes and simple helper classes live. Everything has a
reference to this.
* **VirtualRadar.Library**: Where most of the implementations of the interfaces live.
* **VirtualRadar**: The executable assembly.
* **VirtualRadar.Database**: Implementations of database interfaces.
* **VirtualRadar.Headless**: Implementations of GUI elements that direct output to the console rather than a Winforms window. The main
executable will use these implementations if it's started with the -nogui switch.
* **VirtualRadar.Interop**: There isn't much interop in VRS but what little there is lives here.
* **VirtualRadar.Localisation**: Where all of the translations for the main program live except for plugin translations (each plugin
maintains its own RESX files) and the web site translations (which have ended up in .WebSite). Also has classes for manipulating string
resources.
* **VirtualRadar.Owin**: Where all of the OWIN middleware lives as well as classes for building the OWIN pipeline in a controlled manner.
* **VirtualRadar.Resources**: Where all of the embedded images, files etc. live.
* **VirtualRadar.WebServer**: When VRS had its own web server built around HttpListener it lived here. That web server has been replaced
by Microsoft's Katana web server for OWIN and it's just peripheral web classes that remain.
* **VirtualRadar.WebServer.HttpListener**: Implementations of the web server interface that use Microsoft's HttpListener-based host for
OWIN middleware.
* **VirtualRadar.WebSite**: The web server and web site content are kept separate. This is where the web site content lives.
* **VirtualRadar.WinForms**: Where all of the dialog boxes and windows used by the program live.
* **VirtualRadar.Service**: The executable that can install / uninstall VRS as a service, and is also the entry point for the service.
* **Plugin.***: Optional plugins.

