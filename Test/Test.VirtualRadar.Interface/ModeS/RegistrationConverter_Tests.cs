using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.ModeS;

namespace Test.VirtualRadar.Interface.ModeS
{
    [TestClass]
    public class RegistrationConverter_Tests
    {
        [TestMethod]
        public void RegistrationToModeS_Can_Convert_US_Registrations()
        {
            new InlineDataTest(this).TestAndAssert(new dynamic[] {
                new { Registration = "N1",    Expected = "A00001", },
                new { Registration = "N1A",   Expected = "A00002", },
                new { Registration = "N1AA",  Expected = "A00003", },
                new { Registration = "N1AZ",  Expected = "A0001A", },
                new { Registration = "N1B",   Expected = "A0001B", },
                new { Registration = "N1BA",  Expected = "A0001C", },
                new { Registration = "N1BZ",  Expected = "A00033", },
                new { Registration = "N1C",   Expected = "A00034", },
                new { Registration = "N1HZ",  Expected = "A000C9", },
                new { Registration = "N1J",   Expected = "A000CA", },
                new { Registration = "N1NZ",  Expected = "A00146", },
                new { Registration = "N1P",   Expected = "A00147", },
                new { Registration = "N1ZZ",  Expected = "A00259", },
                new { Registration = "N10",   Expected = "A0025A", },
                new { Registration = "N10A",  Expected = "A0025B", },
//                new { Registration = "N10AA", Expected = "A0025C", },
            },
            row => {
                var actual = RegistrationConverter.RegistrationToModeS(row.Registration);
                Assert.AreEqual(row.Expected, actual);
            });
        }
    }
}
