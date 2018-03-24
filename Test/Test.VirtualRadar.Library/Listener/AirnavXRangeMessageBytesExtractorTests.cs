using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class AirnavXRangeMessageBytesExtractorTests
    {
        public TestContext TestContext { get; set; }

        private IAirnavXRangeMessageBytesExtractor _Extractor;
        private CommonMessageBytesExtractorTests _CommonTests;

        [TestInitialize]
        public void TestInitialise()
        {
            _Extractor = Factory.Resolve<IAirnavXRangeMessageBytesExtractor>();
            _CommonTests = new CommonMessageBytesExtractorTests(TestContext, _Extractor, ExtractedBytesFormat.AirnavXRange);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AirnavXRangeMessageBytes$")]
        public void AirnavXRangeMessageBytesExtractor_ExtractMessageBytes_Extracts_Messages_From_Bytes()
        {
            _CommonTests.Do_ExtractMessageBytes_Extracts_Messages_From_Bytes(true, 3, 2);
        }
    }
}
