# Virtual Radar Server

You will need Visual Studio 2017 or 2019 to build the program. The free Community version of Visual Studio will work just
fine. You can download the installer for it from here:

* **Visual Studio Installer**: https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx

## TypeScript

## Visual Studio 2017

When you install VS2017 (or upgrade it) be sure to select TypeScript 2.3 on the Individual Components tab.

If you get TypeScript errors when compiling then go into the properties for VirtualRadar.WebSite and open the TypeScript tab.
If it says that the version is "2.3 (unavailable)" then quit Visual Studio, run the installer, click More | Modify, go to
Individual Components and install TypeScript 2.3.

## Visual Studio 2019

The VS2019 installer does not offer TypeScript 2.3 as an optional install. However, the projects now reference the TypeScript
2.3.3 NuGet package so they should build correctly. The downside is that you'll see a lot of erroneous TypeScript errors coming out
of Intellisense. This is because Intellisense is using a later version of TypeScript and TypeScript is not good at being backwards
compatible.

You can safely ignore the errors, the build will use 2.3 and it should be fine.

## Earlier versions of Visual Studio
Virtual Radar Server targets .NET 4.6.1 and TypeScript 2.3. It also makes use of language features from C# 6.

You can probably use versions of Visual Studio 2015 as long as you install the .NET 4.6.1 targeting pack and TypeScript 2.3,
but I haven't tried it.

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

