# Virtual Radar Server

You will need Visual Studio 2015 (Update 2 or 3) to build the program. The free
Community version of Visual Studio will work just fine. You can download the installer
for it from here:

* **Visual Studio Installer**: https://www.visualstudio.com/en-us/downloads/download-visual-studio-vs.aspx

You can also use Visual Studio 2013 (or VS2015 pre-Update 2) but you'll need to install
TypeScript 1.8:

* **TypeScript 1.8 for Visual Studio 2013**: https://www.microsoft.com/en-us/download/details.aspx?id=48739
* **TypeScript 1.8 for pre-Update 2 Visual Studio 2015**: https://www.microsoft.com/en-us/download/details.aspx?id=48593

#
## Quirks

### The web site returns 404 errors on every JavaScript file
The short answer to this is to rebuild the **VirtualRadar.WebSite** project and then rebuild
**VirtualRadar**.

The reason why this happens is because there is a pre-build step to VirtualRadar.WebSite that
checksums all of the web content in the **Web** folder. Part of that content is the JavaScript
that has been compiled from the TypeScript. The checksums are built into the resources for
VirtualRadar.WebSite. At run-time the program compares the compile-time checksums against the
checksums for the same files loaded at run-time - if they differ then it won't serve those
files.

Unfortunately TypeScript 1.8 can produce slightly different JavaScript when it's compiling
the same TypeScript file twice. The checksumming operation is actually checksumming the JavaScript
that was compiled the *last* time you built the WebSite project (or what you cloned from the
repository) but the JavaScript that was built *this* time is what'll get checksummed at run-time.

I'll be changing the build process to address this problem and when I'll do I'll remove this note.
In the meantime if you hit the problem just rebuild and it should go away.
