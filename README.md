# Virtual Radar Server

You will need Visual Studio 2017 to build the program. The free Community version of Visual Studio will work just
fine. You can download the installer for it from here:

* **Visual Studio Installer**: https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx

## TypeScript

When you install VS2017 (or upgrade it) be sure to select TypeScript 2.3 on the Individual Components tab.

If you get TypeScript errors when compiling then go into the properties for VirtualRadar.WebSite and open the TypeScript tab.
If it says that the version is "2.3 (unavailable)" then quit Visual Studio, run the installer, click More | Modify, go to
Individual Components and install TypeScript 2.3.

## Earlier versions of Visual Studio
Virtual Radar Server targets .NET 4.6.1 and TypeScript 2.3. It also makes use of language features from C# 6.

You can probably use versions of Visual Studio 2015 as long as you install the .NET 4.6.1 targeting pack and TypeScript 2.3,
but I haven't tried it.
