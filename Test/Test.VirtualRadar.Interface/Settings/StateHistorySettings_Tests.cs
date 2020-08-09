using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class StateHistorySettings_Tests
    {
        private StateHistorySettings _Settings;

        [TestInitialize]
        public void TestInitialise()
        {
            _Settings = new StateHistorySettings();
        }

        [TestMethod]
        public void Ctor_Initialises_To_Known_State()
        {
            CheckProperties(_Settings);
        }

        public static void CheckProperties(StateHistorySettings settings)
        {
            TestUtilities.TestProperty(settings, nameof(settings.WritesEnabled),        true);
            TestUtilities.TestProperty(settings, nameof(settings.NonStandardFolder),    "", "Foo");
        }
    }
}
