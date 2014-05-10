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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Settings;
using System.IO;
using Test.Framework;
using InterfaceFactory;
using Moq;
using VirtualRadar.Interface;
using System.IO.Ports;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class BaseStationSettingsTests
    {
        private IClassFactory _OriginalFactory;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private BaseStationSettings _Implementation;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _Implementation = new BaseStationSettings();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }

        [TestMethod]
        public void BaseStationSettings_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CheckProperties(_Implementation);
        }

        public static void CheckProperties(BaseStationSettings settings)
        {
            settings.DatabaseFileName = settings.DatabaseFileName.ToUpperInvariant();
            settings.OperatorFlagsFolder = settings.OperatorFlagsFolder.ToUpperInvariant();

            TestUtilities.TestProperty(settings, r => r.Address, "127.0.0.1", "Ab");
            TestUtilities.TestProperty(settings, r => r.AutoReconnectAtStartup, false);
            TestUtilities.TestProperty(settings, r => r.Port, 30003, 65535);
            TestUtilities.TestProperty(settings, r => r.ComPort, null, "ABC");
            TestUtilities.TestProperty(settings, r => r.BaudRate, 115200, 2400);
            TestUtilities.TestProperty(settings, r => r.DataBits, 8, 7);
            TestUtilities.TestProperty(settings, r => r.StopBits, StopBits.One, StopBits.None);
            TestUtilities.TestProperty(settings, r => r.Parity, Parity.None, Parity.Odd);
            TestUtilities.TestProperty(settings, r => r.Handshake, Handshake.None, Handshake.XOnXOff);
            TestUtilities.TestProperty(settings, r => r.StartupText, "#43-02\\r", "anything");
            TestUtilities.TestProperty(settings, r => r.ShutdownText, "#43-00\\r", "anything");
            TestUtilities.TestProperty(settings, r => r.ConnectionType, ConnectionType.TCP, ConnectionType.COM);
            TestUtilities.TestProperty(settings, r => r.DataSource, DataSource.Port30003, DataSource.Sbs3);
            TestUtilities.TestProperty(settings, r => r.DatabaseFileName, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Kinetic\BaseStation\BaseStation.sqb").ToUpperInvariant(), "Zz", true);
            TestUtilities.TestProperty(settings, r => r.OperatorFlagsFolder, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Kinetic\BaseStation\OperatorFlags").ToUpperInvariant(), "Zz", true);
            TestUtilities.TestProperty(settings, r => r.SilhouettesFolder, null, "Ab");
            TestUtilities.TestProperty(settings, r => r.OutlinesFolder, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), @"Kinetic\BaseStation\Outlines"), "Ab", true);
            TestUtilities.TestProperty(settings, r => r.PicturesFolder, null, "Bb");
            TestUtilities.TestProperty(settings, r => r.DisplayTimeoutSeconds, 30, 300);
            TestUtilities.TestProperty(settings, r => r.IgnoreBadMessages, true);
            TestUtilities.TestProperty(settings, r => r.TrackingTimeoutSeconds, 600, 3600);
            TestUtilities.TestProperty(settings, r => r.MinimiseToSystemTray, false);
            TestUtilities.TestProperty(settings, r => r.SearchPictureSubFolders, false);
        }

        [TestMethod]
        public void BaseStationSettings_Constructor_Initialises_To_Known_State_Under_Mono()
        {
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(true);

            _Implementation = new BaseStationSettings();

            TestUtilities.TestProperty(_Implementation, r => r.Address, "127.0.0.1", "Ab");
            TestUtilities.TestProperty(_Implementation, r => r.ConnectionType, ConnectionType.TCP, ConnectionType.COM);
            TestUtilities.TestProperty(_Implementation, r => r.Port, 30003, 65535);
            TestUtilities.TestProperty(_Implementation, r => r.DataSource, DataSource.Port30003, DataSource.Sbs3);
            TestUtilities.TestProperty(_Implementation, r => r.DatabaseFileName, null, "Zz", true);
            TestUtilities.TestProperty(_Implementation, r => r.OperatorFlagsFolder, null, "Zz", true);
            TestUtilities.TestProperty(_Implementation, r => r.SilhouettesFolder, null, "Ab");
            TestUtilities.TestProperty(_Implementation, r => r.OutlinesFolder, null, "Ab", true);
            TestUtilities.TestProperty(_Implementation, r => r.PicturesFolder, null, "Bb");
            TestUtilities.TestProperty(_Implementation, r => r.DisplayTimeoutSeconds, 30, 300);
            TestUtilities.TestProperty(_Implementation, r => r.TrackingTimeoutSeconds, 600, 3600);
            TestUtilities.TestProperty(_Implementation, r => r.IgnoreBadMessages, true);
        }
    }
}
