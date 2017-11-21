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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using InterfaceFactory;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class PluginManagerTests
    {
        #region Private Classes and Interfaces - IDerivedFromPlugin, Plugin, NotPlugin
        interface IDerivedFromPlugin : IPlugin
        {
        }

        class Plugin : IPlugin
        {
            public static Dictionary<Type, int> _ConstructorCallCount = new Dictionary<Type,int>();
            public static bool _ConstructorThrowsException;
            public static string _Id;
            public static string _PluginFolder;
            public static string _Name;
            public static string _Version;
            public static bool _HasOptions;
            public static Dictionary<Type, int> _RegisterImplementationsCallCount = new Dictionary<Type,int>();
            public static IClassFactory _RegisterImplementationsClassFactory;
            public static bool _RegisterImplementationsThrowsException;
            public static void _Reset()
            {
                _Id = _Name = _Version = _PluginFolder = null;
                _HasOptions = false;
                _ConstructorCallCount.Clear();
                _RegisterImplementationsCallCount.Clear();
                _ConstructorThrowsException = false;
                _RegisterImplementationsClassFactory = null;
                _RegisterImplementationsThrowsException = false;
            }

            public string Id { get { return _Id; } }
            public string PluginFolder { get { return _PluginFolder; } set { _PluginFolder = value; } }
            public string Name { get { return _Name; } }
            public string Version { get { return _Version; } }
            public string Status { get { return null; } }
            public string StatusDescription { get { return null; } }
            public bool HasOptions { get { return _HasOptions; } }

            #pragma warning disable 0067
            public event EventHandler StatusChanged;
            #pragma warning restore 0067

            private void IncrementCallCount(Dictionary<Type, int> dictionary)
            {
                var type = GetType();
                if(!dictionary.ContainsKey(type)) dictionary[type] = 1;
                else ++dictionary[type];
            }

            public Plugin()
            {
                IncrementCallCount(_ConstructorCallCount);
                if(_ConstructorThrowsException) throw new NotImplementedException();
            }

            public void RegisterImplementations(IClassFactory classFactory)
            {
                IncrementCallCount(_RegisterImplementationsCallCount);
                _RegisterImplementationsClassFactory = classFactory;
                if(_RegisterImplementationsThrowsException) throw new NotImplementedException();
            }

            public void Startup(PluginStartupParameters parameters)
            {
            }

            public void GuiThreadStartup()
            {
            }

            public void Shutdown()
            {
            }

            public void ShowWinFormsOptionsUI()
            {
            }
        }

        class PluginA : Plugin
        {
            public PluginA() : base() { }
        }

        class PluginB : Plugin
        {
            public PluginB() : base() { }
        }

        class PluginC : Plugin
        {
            public PluginC() : base() { }
        }

        class NotPlugin
        {
            public static int _ConstructorCallCount;
            public static void _Reset()
            {
                _ConstructorCallCount = 0;
            }

            public NotPlugin()
            {
                ++_ConstructorCallCount;
            }
        }
        #endregion

        #region TestContext, Fields etc.
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IPluginManager _PluginManager;
        private Mock<IPluginManagerProvider> _Provider;
        private Mock<ILog> _Log;
        private Mock<IPluginManifestStorage> _ManifestStorage;
        private PluginManifest _Manifest;
        private Mock<IApplicationInformation> _ApplicationInformation;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _PluginManager = Factory.Singleton.ResolveNewInstance<IPluginManager>();

            _Provider = new Mock<IPluginManagerProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.ApplicationStartupPath).Returns("x");
            _Provider.Setup(p => p.DirectoryExists(It.IsAny<string>())).Returns(true);
            _Provider.Setup(p => p.DirectoryGetDirectories(It.IsAny<string>())).Returns(new string[] { "subFolder" });
            _PluginManager.Provider = _Provider.Object;

            _Manifest = new PluginManifest();
            _ManifestStorage = TestUtilities.CreateMockImplementation<IPluginManifestStorage>();
            _ManifestStorage.Setup(m => m.LoadForPlugin(It.IsAny<string>())).Returns(_Manifest);

            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();

            _Log = TestUtilities.CreateMockSingleton<ILog>();

            Plugin._Reset();
            PluginA._Reset();
            PluginB._Reset();
            PluginC._Reset();
            NotPlugin._Reset();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        private void SetupProviderForPlugins(params Type[] types)
        {
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "x" });
            _Provider.Setup(p => p.LoadTypes(It.IsAny<string>())).Returns(types);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void PluginManager_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var manager = Factory.Singleton.ResolveNewInstance<IPluginManager>();
            Assert.IsNotNull(manager.Provider);
            TestUtilities.TestProperty(manager, "Provider", manager.Provider, _Provider.Object);

            Assert.AreEqual(0, _PluginManager.LoadedPlugins.Count);
            Assert.AreEqual(0, _PluginManager.IgnoredPlugins.Count);
        }

        [TestMethod]
        public void PluginManager_LoadedPlugins_Returns_All_Plugins_Loaded_By_LoadPlugins()
        {
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "a", "b", });
            _Provider.Setup(p => p.LoadTypes("a")).Returns(new Type[] { typeof(PluginA) });
            _Provider.Setup(p => p.LoadTypes("b")).Returns(new Type[] { typeof(PluginB) });

            _PluginManager.LoadPlugins();

            Assert.AreEqual(2, _PluginManager.LoadedPlugins.Count());
            var pluginA = _PluginManager.LoadedPlugins.FirstOrDefault(p => p.GetType() == typeof(PluginA));
            var pluginB = _PluginManager.LoadedPlugins.FirstOrDefault(p => p.GetType() == typeof(PluginB));
            Assert.IsNotNull(pluginA);
            Assert.IsNotNull(pluginB);
            Assert.AreEqual("subFolder", pluginA.PluginFolder);
            Assert.AreEqual("subFolder", pluginB.PluginFolder);
        }
        #endregion

        #region LoadPlugins
        [TestMethod]
        public void PluginManager_LoadPlugins_Does_Nothing_If_Plugin_Folder_Does_Not_Exist()
        {
            _Provider = new Mock<IPluginManagerProvider>(MockBehavior.Strict);
            _Provider.Setup(p => p.ApplicationStartupPath).Returns("xyz");
            _Provider.Setup(p => p.DirectoryExists(It.IsAny<string>())).Returns(false);
            _PluginManager.Provider = _Provider.Object;

            _PluginManager.LoadPlugins();

            _Provider.Verify(p => p.DirectoryExists(It.Is<string>(s => s.Equals(@"xyz\plugins", StringComparison.OrdinalIgnoreCase))), Times.Once());
            _Provider.Verify(p => p.DirectoryExists(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Looks_In_Correct_Folder_For_Plugins()
        {
            _Provider.Setup(p => p.ApplicationStartupPath).Returns("abc");
            _Provider.Setup(p => p.DirectoryGetDirectories(@"abc\Plugins")).Returns(new string[] { "fullpath" });

            _PluginManager.LoadPlugins();

            _Provider.Verify(p => p.DirectoryGetFiles(It.Is<string>(s => s.Equals(@"fullpath", StringComparison.OrdinalIgnoreCase)), It.Is<string>(s => s.Equals("VirtualRadar.Plugin.*.dll", StringComparison.OrdinalIgnoreCase))), Times.Once());
            _Provider.Verify(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Attempts_To_Load_Each_DLL_Returned_By_DirectoryGetFiles()
        {
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "a", "b" });

            _PluginManager.LoadPlugins();

            _Provider.Verify(p => p.LoadTypes("a"), Times.Once());
            _Provider.Verify(p => p.LoadTypes("b"), Times.Once());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Ignores_Dlls_That_Cannot_Be_Loaded()
        {
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "a", "b", "c" });
            _Provider.Setup(p => p.LoadTypes("a")).Returns(new Type[] { typeof(PluginA) });
            _Provider.Setup(p => p.LoadTypes("b")).Returns(new Type[] { typeof(PluginB) }).Callback(() => { throw new NotImplementedException(); });
            _Provider.Setup(p => p.LoadTypes("c")).Returns(new Type[] { typeof(PluginC) });

            _PluginManager.LoadPlugins();

            Assert.AreEqual(1, Plugin._RegisterImplementationsCallCount[typeof(PluginA)]);
            Assert.AreEqual(false, Plugin._RegisterImplementationsCallCount.ContainsKey(typeof(PluginB)));
            Assert.AreEqual(1, Plugin._RegisterImplementationsCallCount[typeof(PluginC)]);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Ignores_Files_With_No_Manifest()
        {
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "plugin.dll" });
            _ManifestStorage.Setup(m => m.LoadForPlugin("plugin.dll")).Returns((PluginManifest)null);

            _PluginManager.LoadPlugins();

            _Provider.Verify(p => p.LoadTypes("plugin.dll"), Times.Never());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Records_Reason_For_Ignoring_File_With_No_Manifest()
        {
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { @"c:\tmp\plugin.dll" });
            _ManifestStorage.Setup(m => m.LoadForPlugin(@"c:\tmp\plugin.dll")).Returns((PluginManifest)null);

            _PluginManager.LoadPlugins();

            Assert.AreEqual(1, _PluginManager.IgnoredPlugins.Count);
            Assert.AreEqual(Strings.CouldNotFindManifest, _PluginManager.IgnoredPlugins[@"c:\tmp\plugin.dll"]);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Ignores_Files_With_Unparseable_Manifests()
        {
            var exception = new InvalidOperationException();
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "plugin.dll" });
            _ManifestStorage.Setup(m => m.LoadForPlugin("plugin.dll")).Callback(() => { throw exception; });

            _PluginManager.LoadPlugins();

            _Provider.Verify(p => p.LoadTypes("plugin.dll"), Times.Never());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Records_Reason_For_Ignoring_File_With_Unparseable_Manifest()
        {
            var exception = new InvalidOperationException();
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { @"c:\zoo\plugin.dll" });
            _ManifestStorage.Setup(m => m.LoadForPlugin(@"c:\zoo\plugin.dll")).Callback(() => { throw exception; });

            _PluginManager.LoadPlugins();

            Assert.AreEqual(1, _PluginManager.IgnoredPlugins.Count);
            Assert.AreEqual(String.Format(Strings.CouldNotParseManifest, exception.Message), _PluginManager.IgnoredPlugins[@"c:\zoo\plugin.dll"]);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "PluginManifestCheck$")]
        public void PluginManager_LoadPlugins_Checks_Manifest_Before_Loading_Plugin()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "a" });
            _Provider.Setup(p => p.LoadTypes("a")).Returns(new Type[] { typeof(PluginA) });

            _Manifest.MinimumVersion = worksheet.EString("MinimumVersion");
            _Manifest.MaximumVersion = worksheet.EString("MaximumVersion");

            _ApplicationInformation.Setup(a => a.Version).Returns(new Version(worksheet.String("VRSVersion")));

            _PluginManager.LoadPlugins();

            if(worksheet.Bool("CanLoad")) {
                Assert.AreEqual(1, _PluginManager.LoadedPlugins.Count);
                Assert.AreEqual(0, _PluginManager.IgnoredPlugins.Count);
            } else {
                Assert.AreEqual(0, _PluginManager.LoadedPlugins.Count);
                Assert.AreEqual(worksheet.String("IgnoredMessage"), _PluginManager.IgnoredPlugins["a"]);
            }
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Logs_Failure_To_Load_Dlls_In_Plugins_Folder()
        {
            var exception = new InvalidOperationException();
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "thefile" });
            _Provider.Setup(p => p.LoadTypes("thefile")).Callback(() => { throw exception; });

            _PluginManager.LoadPlugins();

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Reports_Reason_Why_Dll_That_Cannot_Be_Loaded_Was_Ignored()
        {
            var exception = new InvalidOperationException();
            _Provider.Setup(p => p.DirectoryGetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "thefile" });
            _Provider.Setup(p => p.LoadTypes("thefile")).Callback(() => { throw exception; });

            _PluginManager.LoadPlugins();

            Assert.AreEqual(0, _PluginManager.LoadedPlugins.Count);
            Assert.AreEqual(1, _PluginManager.IgnoredPlugins.Count);
            Assert.AreEqual(String.Format(Strings.PluginCannotBeLoaded, exception.Message), _PluginManager.IgnoredPlugins["thefile"]);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Instantiates_Plugins()
        {
            SetupProviderForPlugins(typeof(NotPlugin), typeof(Plugin));

            _PluginManager.LoadPlugins();

            Assert.AreEqual(1, Plugin._ConstructorCallCount[typeof(Plugin)]);
            Assert.AreEqual(0, NotPlugin._ConstructorCallCount);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Logs_Failure_To_Instantiate_Plugins()
        {
            SetupProviderForPlugins(typeof(Plugin));
            Plugin._ConstructorThrowsException = true;

            _PluginManager.LoadPlugins();

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Reports_Reason_Why_Plugin_That_Cannot_Be_Instantiated_Is_Ignored()
        {
            SetupProviderForPlugins(typeof(Plugin));
            Plugin._ConstructorThrowsException = true;

            _PluginManager.LoadPlugins();

            Assert.AreEqual(0, _PluginManager.LoadedPlugins.Count);
            Assert.AreEqual(1, _PluginManager.IgnoredPlugins.Count);
            Assert.IsTrue(_PluginManager.IgnoredPlugins["x"].StartsWith(Strings.PluginCannotBeLoaded.Substring(0, Strings.PluginCannotBeLoaded.IndexOf("{0}"))));
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Ignores_Libraries_With_Multiple_Plugin_Classes()
        {
            // If there's more than one then how can we tell which one is in charge? Better to ignore it completely.

            SetupProviderForPlugins(typeof(Plugin), typeof(PluginA));

            _PluginManager.LoadPlugins();

            Assert.AreEqual(0, Plugin._ConstructorCallCount.Count);
            Assert.AreEqual(0, _PluginManager.LoadedPlugins.Count);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Records_Reason_For_Ignoring_Libraries_With_Mulitple_Plugin_Classes()
        {
            SetupProviderForPlugins(typeof(Plugin), typeof(PluginA));

            _PluginManager.LoadPlugins();

            Assert.AreEqual(1, _PluginManager.IgnoredPlugins.Count);
            Assert.AreEqual(Strings.PluginDoesNotHaveJustOneIPlugin, _PluginManager.IgnoredPlugins["x"]);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Ignores_Interfaces_That_Inherit_From_IPlugin()
        {
            SetupProviderForPlugins(typeof(IDerivedFromPlugin), typeof(Plugin));

            _PluginManager.LoadPlugins();

            Assert.AreEqual(1, Plugin._ConstructorCallCount.Count);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Calls_RegisterImplementations_For_Each_Plugin()
        {
            SetupProviderForPlugins(typeof(Plugin));

            _PluginManager.LoadPlugins();

            Assert.AreEqual(1, Plugin._RegisterImplementationsCallCount[typeof(Plugin)]);
            Assert.AreSame(Factory.Singleton, Plugin._RegisterImplementationsClassFactory);
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Rolls_Back_RegisterImplementations_If_It_Throws_Exception()
        {
            Plugin._RegisterImplementationsThrowsException = true;
            SetupProviderForPlugins(typeof(Plugin));
            var snapshot = new Mock<IClassFactory>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _Provider.Setup(p => p.ClassFactoryTakeSnapshot()).Returns(snapshot);

            _PluginManager.LoadPlugins();

            _Provider.Verify(p => p.ClassFactoryRestoreSnapshot(snapshot), Times.Once());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Logs_Libraries_With_RegisterImplementations_That_Throws_Exception()
        {
            Plugin._RegisterImplementationsThrowsException = true;
            SetupProviderForPlugins(typeof(Plugin));

            _PluginManager.LoadPlugins();

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once());
        }

        [TestMethod]
        public void PluginManager_LoadPlugins_Does_Not_Roll_Back_RegisterImplementations_If_No_Exception_Thrown()
        {
            SetupProviderForPlugins(typeof(Plugin));
            var snapshot = new Mock<IClassFactory>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _Provider.Setup(p => p.ClassFactoryTakeSnapshot()).Returns(snapshot);

            _PluginManager.LoadPlugins();

            _Provider.Verify(p => p.ClassFactoryRestoreSnapshot(snapshot), Times.Never());
        }
        #endregion
    }
}
