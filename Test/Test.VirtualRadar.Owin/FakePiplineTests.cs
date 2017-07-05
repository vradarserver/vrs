using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Owin;

namespace Test.VirtualRadar.Owin
{
    [TestClass]
    public class FakePiplineTests
    {
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IFakePipeline _FakePipeline;
        private MockOwinEnvironment _Environment;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _FakePipeline = Factory.Singleton.Resolve<IFakePipeline>();
            _Environment = new MockOwinEnvironment();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void FakePipeline_ConfigureStandardPipeline_Adds_Standard_Middleware()
        {
            throw new NotImplementedException();
        }
    }
}
