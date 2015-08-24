using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Adsb;

namespace Test.VirtualRadar.Interface.Adsb
{
    [TestClass]
    public class CoarseTisbAirbornePositionTests
    {
        [TestMethod]
        public void CoarseTisbAirbornePosition_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var message = new CoarseTisbAirbornePosition();

            TestUtilities.TestProperty(message, r => r.BarometricAltitude, null, 123);
            TestUtilities.TestProperty(message, r => r.CompactPosition, null, new CompactPositionReportingCoordinate());
            TestUtilities.TestProperty(message, r => r.GroundSpeed, null, 1.2);
            TestUtilities.TestProperty(message, r => r.GroundTrack, null, 1.2);
            TestUtilities.TestProperty(message, r => r.ServiceVolumeID, (byte)0, (byte)12);
            TestUtilities.TestProperty(message, r => r.SurveillanceStatus, SurveillanceStatus.NoInformation, SurveillanceStatus.PermanentAlert);
        }
    }
}
