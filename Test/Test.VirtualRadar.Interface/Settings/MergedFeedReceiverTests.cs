using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class MergedFeedReceiverTests
    {
        [TestMethod]
        public void MergedFeedReceiver_Ctor_Initialises_To_Known_State_And_Properties_Work()
        {
            CheckProperties(new MergedFeedReceiver());
        }

        public static void CheckProperties(MergedFeedReceiver settings)
        {
            TestUtilities.TestProperty(settings, r => r.IsMlatFeed, false);
            TestUtilities.TestProperty(settings, r => r.UniqueId, 0, 120);
        }
    }
}
