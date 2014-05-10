// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Describes the interface for a class that plugin DLLs must implement in order to be loaded by Virtual Radar Server.
    /// </summary>
    /// <remarks><para>
    /// Plugin libraries are loaded by Virtual Radar Server during startup. The program looks for DLLs whose name matches
    /// VirtualRadar.Plugin.*.dll in subfolders below the Virtual Radar Server &quot;Plugins&quot; folder. It loads each
    /// DLL into the process and then searches for a public class that implements this interface. If it can find one then
    /// it instantiates the object implementing the interface and makes calls upon it at various points in the life of the
    /// application to give the plugin the opportunity to change the way the program behaves.
    /// </para></remarks>
    public interface IPlugin
    {
        /// <summary>
        /// Gets the unique identifier of the plugin.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This identifier should be different to that of all other plugins and should remain constant regardless
        /// of the version of the plugin or the region that the application is running under. VRS plugins use the
        /// name of the plugin DLL as the identifier (e.g. 'VirtualRadar.Plugin.Foobar' for virtualradar.plugin.foobar.dll)
        /// but you can use a GUID if you like.
        /// </para><para>
        /// The ID is used by <see cref="Settings.PluginSettings"/> to uniquely identify the settings for a plugin so if you
        /// change it across releases of your plugin then you may have problems reading old settings. It is recommended that
        /// once you set the identifier for a plugin you never change it.
        /// </para>
        /// </remarks>
        string Id { get; }

        /// <summary>
        /// Gets or sets the folder that the plugin has been installed into.
        /// </summary>
        /// <remarks>
        /// The plugin should just provide the property - VRS will fill this value in when it loads the plugin. It will not
        /// be filled in until after the constructor has finished running so you cannot refer to it from the constructor.
        /// </remarks>
        string PluginFolder { get; set; }

        /// <summary>
        /// Gets the name of the plugin. This is only shown to the user, it is otherwise unused by Virtual Radar Server.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version number of the plugin. This is only shown to the user, it is otherwise unused by Virtual Radar Server.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets a short string describing the state of the plugin (e.g. Disabled, Enabled, Writing to XYZ etc.).
        /// </summary>
        /// <remarks>
        /// You should raise <see cref="StatusChanged"/> when you change the value of this property.
        /// </remarks>
        string Status { get; }

        /// <summary>
        /// Gets a longer string giving more information on <see cref="Status"/>. If no description is appropriate,
        /// i.e. <see cref="Status"/> already says everything you want to say, then return an empty string or null.
        /// </summary>
        /// <remarks>
        /// You should raise <see cref="StatusChanged"/> when you change the value of this property.
        /// </remarks>
        string StatusDescription { get; }

        /// <summary>
        /// Gets a value indicating that the plugin has user-configurable settings.
        /// </summary>
        /// <remarks>
        /// If you return false here then the user will not be shown an Options button against your plugin in Virtual Radar
        /// Server's list of plugins and <see cref="ShowWinFormsOptionsUI"/> will never be called.
        /// </remarks>
        bool HasOptions { get; }

        /// <summary>
        /// Raised by the plugin whenever Status or StatusChanged has been updated by the plugin.
        /// </summary>
        /// <remarks>VRS does not make any assumptions about which thread this is raised on, you can raise it on the GUI
        /// thread or a background thread.</remarks>
        event EventHandler StatusChanged;

        /// <summary>
        /// Called by VRS during the early stages of program startup to give the plugin a chance to register
        /// its own implementations of the program interfaces. This is called before the UI has been built up, do not
        /// make GUI calls from here.
        /// </summary>
        /// <param name="classFactory"></param>
        /// <remarks><para>
        /// VRS works by declaring interfaces for all of the important classes in VirtualRadar.Interfaces and
        /// then registering implementations of those interfaces with a class factory. Whenever the program wants
        /// to obtain an implementation of an interface it calls the class factory.
        /// </para><para>
        /// This method is called after VRS has registered all of its implementations for the interfaces but before
        /// any of them have been instantiated. If you want to replace one or more of the standard implementations of
        /// the interfaces with your own version then this is the time to do so.
        /// </para><para>
        /// Plugins are loaded in a random order. It is possible that another plugin could override your implementation
        /// of a VRS class with its own. Also please note that any exception thrown by this method will result in
        /// your plugin being disabled and the class factory reset back to how it was before your plugin made any
        /// changes.
        /// </para></remarks>
        void RegisterImplementations(IClassFactory classFactory);

        /// <summary>
        /// Called by VRS towards the end of the splash screen, after the web server etc. have been built up but before the
        /// main window is displayed. This is called on a background thread. Do not make any calls that must be made on the
        /// GUI thread from here.
        /// </summary>
        /// <param name="parameters"></param>
        /// <remarks>
        /// The plugin should use this method to read its configuration, hook any events that it is interested in and generally
        /// start doing some useful work. Any exceptions thrown by this method will be displayed to the user and recorded in the
        /// log but will not cause the program to stop, nor will they cause the plugin to be unloaded.
        /// </remarks>
        void Startup(PluginStartupParameters parameters);

        /// <summary>
        /// Called by VRS on the GUI thread after <see cref="Startup"/> has been called.
        /// </summary>
        /// <remarks><para>
        /// Some plugins may need to perform some startup work on the GUI thread. This method is guaranteed to be called on the
        /// GUI thread by the main presenter.
        /// </para><para>
        /// The method is called after the main view has been constructed but before it is shown to the user. At the point where
        /// this is called no UI will be visible to the user.
        /// </para></remarks>
        void GuiThreadStartup();

        /// <summary>
        /// Called by VRS when the program is closing down, before the web server etc. have been destroyed. This may be called
        /// on a background thread, do not make GUI calls from within here.
        /// </summary>
        /// <remarks>
        /// The plugin should use this method to release any resources that it has held onto for the duration of the program, close
        /// any open files, shut down background threads etc.
        /// </remarks>
        void Shutdown();

        /// <summary>
        /// Called by VRS when the user wants to change the options for the application. This will not be called if <see cref="HasOptions"/>
        /// is false. You must present the options using WinForms. The function will be called on the GUI thread.
        /// </summary>
        /// <remarks>
        /// See <see cref="Settings.IPluginSettingsStorage"/> for the object to use to load and save your plugin's settings.
        /// </remarks>
        void ShowWinFormsOptionsUI();
    }
}
