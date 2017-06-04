// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Owin.Configuration
{
    [TestClass]
    public class ImageServerConfigurationTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;
        private MockFileSystemProvider _FileSystemProvider;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private global::VirtualRadar.Interface.Settings.Configuration _Settings;
        private IImageServerConfiguration _Configuration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _FileSystemProvider = new MockFileSystemProvider();
            Factory.Singleton.RegisterInstance<IFileSystemProvider>(_FileSystemProvider);

            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Settings = new global::VirtualRadar.Interface.Settings.Configuration();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Settings);

            _Configuration = Factory.Singleton.Resolve<IImageServerConfiguration>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void ImageServerConfiguration_Singleton_Is_Not_Null()
        {
            Assert.IsNotNull(_Configuration.Singleton);
        }

        [TestMethod]
        public void ImageServerConfiguration_Singleton_Is_Same_Instance_Across_All_Instances()
        {
            var other = Factory.Singleton.Resolve<IImageServerConfiguration>();
            Assert.AreSame(_Configuration.Singleton, other.Singleton);
        }

        [TestMethod]
        public void ImageServerConfiguration_OperatorFolder_Returns_Settings_Value()
        {
            var folder = @"C:\VRS\Folder";
            _FileSystemProvider.AddFolder(folder);
            _Settings.BaseStationSettings.OperatorFlagsFolder = folder;

            Assert.AreEqual(folder, _Configuration.OperatorFolder);
        }

        [TestMethod]
        public void ImageServerConfiguration_OperatorFolder_Returns_Null_If_Configured_Folder_Does_Not_Exist()
        {
            var folder = @"C:\VRS\Folder";
            _FileSystemProvider.AddFolder(@"C:\VRS");
            _Settings.BaseStationSettings.OperatorFlagsFolder = folder;

            Assert.IsNull(_Configuration.OperatorFolder);
        }

        [TestMethod]
        public void ImageServerConfiguration_OperatorFolder_Only_Checks_Folder_Once()
        {
            _Settings.BaseStationSettings.OperatorFlagsFolder = @"C:\Folder";
            Assert.AreEqual(_Configuration.OperatorFolder, _Configuration.OperatorFolder);

            Assert.AreEqual(1, _FileSystemProvider.DirectoryExists_CallCount);
        }

        [TestMethod]
        public void ImageServerConfiguration_OperatorFolder_Picks_Up_Changes_In_Configuration()
        {
            _FileSystemProvider.AddFolder(@"C:\Folder1");
            _FileSystemProvider.AddFolder(@"C:\Folder2");

            _Settings.BaseStationSettings.OperatorFlagsFolder = @"C:\Folder1";
            Assert.AreEqual(@"C:\Folder1", _Configuration.OperatorFolder);
            Assert.AreEqual(1, _FileSystemProvider.DirectoryExists_CallCount);

            _Settings.BaseStationSettings.OperatorFlagsFolder = @"C:\Folder2";
            Assert.AreEqual(@"C:\Folder2", _Configuration.OperatorFolder);
            Assert.AreEqual(2, _FileSystemProvider.DirectoryExists_CallCount);
        }

        [TestMethod]
        public void ImageServerConfiguration_SilhouettesFolder_Returns_Settings_Value()
        {
            var folder = @"C:\VRS\Folder";
            _FileSystemProvider.AddFolder(folder);
            _Settings.BaseStationSettings.SilhouettesFolder = folder;

            Assert.AreEqual(folder, _Configuration.SilhouettesFolder);
        }

        [TestMethod]
        public void ImageServerConfiguration_SilhouettesFolder_Returns_Null_If_Configured_Folder_Does_Not_Exist()
        {
            var folder = @"C:\VRS\Folder";
            _FileSystemProvider.AddFolder(@"C:\VRS");
            _Settings.BaseStationSettings.SilhouettesFolder = folder;

            Assert.IsNull(_Configuration.SilhouettesFolder);
        }

        [TestMethod]
        public void ImageServerConfiguration_SilhouettesFolder_Only_Checks_Folder_Once()
        {
            _Settings.BaseStationSettings.SilhouettesFolder = @"C:\Folder";
            Assert.AreEqual(_Configuration.SilhouettesFolder, _Configuration.SilhouettesFolder);

            Assert.AreEqual(1, _FileSystemProvider.DirectoryExists_CallCount);
        }

        [TestMethod]
        public void ImageServerConfiguration_SilhouettesFolder_Picks_Up_Changes_In_Configuration()
        {
            _FileSystemProvider.AddFolder(@"C:\Folder1");
            _FileSystemProvider.AddFolder(@"C:\Folder2");

            _Settings.BaseStationSettings.SilhouettesFolder = @"C:\Folder1";
            Assert.AreEqual(@"C:\Folder1", _Configuration.SilhouettesFolder);
            Assert.AreEqual(1, _FileSystemProvider.DirectoryExists_CallCount);

            _Settings.BaseStationSettings.SilhouettesFolder = @"C:\Folder2";
            Assert.AreEqual(@"C:\Folder2", _Configuration.SilhouettesFolder);
            Assert.AreEqual(2, _FileSystemProvider.DirectoryExists_CallCount);
        }

        [TestMethod]
        public void ImageServerConfiguration_AircraftPictureManager_Is_Not_Null()
        {
            Assert.IsNotNull(_Configuration.AircraftPictureManager);
        }

        [TestMethod]
        public void ImageServerConfiguration_AircraftPictureCache_Is_Not_Null()
        {
            Assert.IsNotNull(_Configuration.AircraftPictureCache);
        }
    }
}
