using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Resources;

namespace Test.VirtualRadar.Resources
{
    [TestClass]
    public class ImagesTests
    {
        [TestMethod]
        public void Images_All_Properties_Return_Byte_Arrays()
        {
            foreach(var property in typeof(Images).GetProperties().Where(r => !r.Name.EndsWith("_IsCustom"))) {
                var bytes = property.GetValue(null, null) as byte[];
                Assert.IsNotNull(bytes,             $"{nameof(Images)}.{property.Name} returns null");
                Assert.AreNotEqual(0, bytes.Length, $"{nameof(Images)}.{property.Name} returns a zero-length array");
            }
        }
    }
}
