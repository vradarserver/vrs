using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class AircraftListJsonBuilderArgsTests
    {
        [TestMethod]
        public void AircraftListJsonBuilderArgs_Constructor_Initialses_Properties_To_Known_Values()
        {
            var args = new AircraftListJsonBuilderArgs();

            Assert.AreEqual(0, args.PreviousAircraft.Count);
            Assert.AreEqual(0, args.SortBy.Count);

            TestUtilities.TestProperty(args, r => r.AircraftList, null, TestUtilities.CreateMockInstance<IAircraftList>().Object);
            TestUtilities.TestProperty(args, r => r.AlwaysShowIcao, false);
            TestUtilities.TestProperty(args, r => r.BrowserLatitude, null, 1.1);
            TestUtilities.TestProperty(args, r => r.BrowserLongitude, null, 2.2);
            TestUtilities.TestProperty(args, r => r.FeedsNotRequired, false);
            TestUtilities.TestProperty(args, r => r.Filter, null, new AircraftListJsonBuilderFilter());
            TestUtilities.TestProperty(args, r => r.IgnoreUnchanged, false);
            TestUtilities.TestProperty(args, r => r.IsFlightSimulatorList, false);
            TestUtilities.TestProperty(args, r => r.IsInternetClient, false);
            TestUtilities.TestProperty(args, r => r.OnlyIncludeMessageFields, false);
            TestUtilities.TestProperty(args, r => r.PreviousDataVersion, -1L, 1L);
            TestUtilities.TestProperty(args, r => r.ResendTrails, false);
            TestUtilities.TestProperty(args, r => r.SelectedAircraftId, -1, 1);
            TestUtilities.TestProperty(args, r => r.SourceFeedId, -1, 2);
            TestUtilities.TestProperty(args, r => r.TrailType, TrailType.None, TrailType.Full);
        }
    }
}
