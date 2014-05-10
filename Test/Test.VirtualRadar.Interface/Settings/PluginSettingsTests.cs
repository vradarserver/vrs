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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using Moq;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class PluginSettingsTests
    {
        private PluginSettings _PluginSettings;
        private IPlugin _Plugin;

        [TestInitialize]
        public void TestInitialise()
        {
            _PluginSettings = new PluginSettings();
            _Plugin = new Mock<IPlugin>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            Mock.Get(_Plugin).Setup(p => p.Id).Returns("x");
        }

        [TestMethod]
        public void PluginSettings_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.AreEqual(0, _PluginSettings.Values.Count);
        }

        [TestMethod]
        public void PluginSettings_Read_Write_String_Works()
        {
            _PluginSettings.Write(_Plugin, "a", "X");
            _PluginSettings.Write(_Plugin, "b", "y");
            _PluginSettings.Write(_Plugin, "c", (int?)null);

            Assert.AreEqual(3, _PluginSettings.Values.Count);
            Assert.AreEqual("X", _PluginSettings.Values["x.a"]);
            Assert.AreEqual("y", _PluginSettings.Values["x.b"]);
            Assert.AreEqual(null, _PluginSettings.Values["x.c"]);

            Assert.AreEqual(null, _PluginSettings.ReadString(_Plugin, "z"));

            Assert.AreEqual("X", _PluginSettings.ReadString(_Plugin, "a"));
            Assert.AreEqual("y", _PluginSettings.ReadString(_Plugin, "b"));
            Assert.AreEqual(null, _PluginSettings.ReadString(_Plugin, "c"));
        }

        [TestMethod]
        public void PluginSettings_Read_Write_Bool_Works()
        {
            _PluginSettings.Write(_Plugin, "a", true);
            _PluginSettings.Write(_Plugin, "b", false);
            _PluginSettings.Write(_Plugin, "c", (bool?)null);

            Assert.AreEqual(3, _PluginSettings.Values.Count);
            Assert.IsNotNull(_PluginSettings.Values["x.a"]);
            Assert.IsNotNull(_PluginSettings.Values["x.b"]);
            Assert.IsNull(_PluginSettings.Values["x.c"]);

            Assert.AreEqual(null, _PluginSettings.ReadBool(_Plugin, "z"));
            Assert.AreEqual(false, _PluginSettings.ReadBool(_Plugin, "z", false));
            Assert.AreEqual(true, _PluginSettings.ReadBool(_Plugin, "z", true));

            Assert.AreEqual(true, _PluginSettings.ReadBool(_Plugin, "a"));
            Assert.AreEqual(false, _PluginSettings.ReadBool(_Plugin, "b"));
            Assert.AreEqual(null, _PluginSettings.ReadBool(_Plugin, "c"));

            Assert.AreEqual(true, _PluginSettings.ReadBool(_Plugin, "a", false));
            Assert.AreEqual(false, _PluginSettings.ReadBool(_Plugin, "b", true));
            Assert.AreEqual(true, _PluginSettings.ReadBool(_Plugin, "c", true));
            Assert.AreEqual(false, _PluginSettings.ReadBool(_Plugin, "c", false));
        }

        [TestMethod]
        public void PluginSettings_Read_Write_Int_Works()
        {
            _PluginSettings.Write(_Plugin, "a", 1);
            _PluginSettings.Write(_Plugin, "b", 2);
            _PluginSettings.Write(_Plugin, "c", (int?)null);

            Assert.AreEqual(3, _PluginSettings.Values.Count);
            Assert.AreEqual("1", _PluginSettings.Values["x.a"]);
            Assert.AreEqual("2", _PluginSettings.Values["x.b"]);
            Assert.AreEqual(null, _PluginSettings.Values["x.c"]);

            Assert.AreEqual(null, _PluginSettings.ReadInt(_Plugin, "z"));
            Assert.AreEqual(1, _PluginSettings.ReadInt(_Plugin, "z", 1));
            Assert.AreEqual(2, _PluginSettings.ReadInt(_Plugin, "z", 2));

            Assert.AreEqual(1, _PluginSettings.ReadInt(_Plugin, "a"));
            Assert.AreEqual(2, _PluginSettings.ReadInt(_Plugin, "b"));
            Assert.AreEqual(null, _PluginSettings.ReadInt(_Plugin, "c"));

            Assert.AreEqual(1, _PluginSettings.ReadInt(_Plugin, "a", 99));
            Assert.AreEqual(2, _PluginSettings.ReadInt(_Plugin, "b", 99));
            Assert.AreEqual(1, _PluginSettings.ReadInt(_Plugin, "c", 1));
            Assert.AreEqual(2, _PluginSettings.ReadInt(_Plugin, "c", 2));
        }

        [TestMethod]
        public void PluginSettings_Read_Write_Long_Works()
        {
            _PluginSettings.Write(_Plugin, "a", 1L);
            _PluginSettings.Write(_Plugin, "b", 2L);
            _PluginSettings.Write(_Plugin, "c", (int?)null);

            Assert.AreEqual(3, _PluginSettings.Values.Count);
            Assert.AreEqual("1", _PluginSettings.Values["x.a"]);
            Assert.AreEqual("2", _PluginSettings.Values["x.b"]);
            Assert.AreEqual(null, _PluginSettings.Values["x.c"]);

            Assert.AreEqual(null, _PluginSettings.ReadLong(_Plugin, "z"));
            Assert.AreEqual(1L, _PluginSettings.ReadLong(_Plugin, "z", 1L));
            Assert.AreEqual(2L, _PluginSettings.ReadLong(_Plugin, "z", 2L));

            Assert.AreEqual(1L, _PluginSettings.ReadLong(_Plugin, "a"));
            Assert.AreEqual(2L, _PluginSettings.ReadLong(_Plugin, "b"));
            Assert.AreEqual(null, _PluginSettings.ReadLong(_Plugin, "c"));

            Assert.AreEqual(1L, _PluginSettings.ReadLong(_Plugin, "a", 99L));
            Assert.AreEqual(2L, _PluginSettings.ReadLong(_Plugin, "b", 99L));
            Assert.AreEqual(1L, _PluginSettings.ReadLong(_Plugin, "c", 1L));
            Assert.AreEqual(2L, _PluginSettings.ReadLong(_Plugin, "c", 2L));
        }

        [TestMethod]
        public void PluginSettings_Read_Write_Double_Works()
        {
            _PluginSettings.Write(_Plugin, "a", 1.2);
            _PluginSettings.Write(_Plugin, "b", 2.3);
            _PluginSettings.Write(_Plugin, "c", (int?)null);

            Assert.AreEqual(3, _PluginSettings.Values.Count);
            Assert.AreEqual("1.2", _PluginSettings.Values["x.a"]);
            Assert.AreEqual("2.3", _PluginSettings.Values["x.b"]);
            Assert.AreEqual(null, _PluginSettings.Values["x.c"]);

            Assert.AreEqual(null, _PluginSettings.ReadDouble(_Plugin, "z"));
            Assert.AreEqual(1.2, _PluginSettings.ReadDouble(_Plugin, "z", 1.2));
            Assert.AreEqual(2.3, _PluginSettings.ReadDouble(_Plugin, "z", 2.3));

            Assert.AreEqual(1.2, _PluginSettings.ReadDouble(_Plugin, "a"));
            Assert.AreEqual(2.3, _PluginSettings.ReadDouble(_Plugin, "b"));
            Assert.AreEqual(null, _PluginSettings.ReadDouble(_Plugin, "c"));

            Assert.AreEqual(1.2, _PluginSettings.ReadDouble(_Plugin, "a", 99.0));
            Assert.AreEqual(2.3, _PluginSettings.ReadDouble(_Plugin, "b", 99.0));
            Assert.AreEqual(1.2, _PluginSettings.ReadDouble(_Plugin, "c", 1.2));
            Assert.AreEqual(2.3, _PluginSettings.ReadDouble(_Plugin, "c", 2.3));
        }

        [TestMethod]
        public void PluginSettings_Read_Write_DateTime_Works()
        {
            var dateTime1 = new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);

            _PluginSettings.Write(_Plugin, "a", dateTime1);

            Assert.AreEqual(1, _PluginSettings.Values.Count);
            Assert.AreEqual("2001-02-03 04:05:06Z", _PluginSettings.Values["x.a"]);

            Assert.AreEqual(null, _PluginSettings.ReadDateTime(_Plugin, "z"));
            Assert.AreEqual(dateTime1, _PluginSettings.ReadDateTime(_Plugin, "z", dateTime1));

            Assert.AreEqual(dateTime1, _PluginSettings.ReadDateTime(_Plugin, "a"));

            Assert.AreEqual(dateTime1, _PluginSettings.ReadDateTime(_Plugin, "a", DateTime.Now));
        }

        [TestMethod]
        public void PluginSettings_Read_Write_Works_In_Different_Regions()
        {
            using(var switcher = new CultureSwitcher("de-DE")) {
                _PluginSettings.Write(_Plugin, "int", 1234567890);
                Assert.AreEqual("1234567890", _PluginSettings.Values["x.int"]);
                Assert.AreEqual(1234567890, _PluginSettings.ReadInt(_Plugin, "int"));

                _PluginSettings.Write(_Plugin, "long", 1234567890987654321L);
                Assert.AreEqual("1234567890987654321", _PluginSettings.Values["x.long"]);
                Assert.AreEqual(1234567890987654321L, _PluginSettings.ReadLong(_Plugin, "long"));

                _PluginSettings.Write(_Plugin, "double", 1.2345);
                Assert.AreEqual("1.2345", _PluginSettings.Values["x.double"]);
                Assert.AreEqual(1.2345, _PluginSettings.ReadDouble(_Plugin, "double"));

                var dateTime = new DateTime(2010, 07, 31, 23, 22, 21, DateTimeKind.Local);
                _PluginSettings.Write(_Plugin, "dateTime", dateTime);
                Assert.AreEqual("2010-07-31 22:22:21Z", _PluginSettings.Values["x.dateTime"]);  // This assumes that the test is running under a timezone where 31/07/2010 is subject to daylight saving time
                var readBackDate = _PluginSettings.ReadDateTime(_Plugin, "dateTime");
                Assert.AreEqual(DateTimeKind.Local, readBackDate.Value.Kind);
                Assert.AreEqual(dateTime, readBackDate);
            }
        }
    }
}
