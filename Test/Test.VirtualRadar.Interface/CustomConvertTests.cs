using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class CustomConvertTests
    {
        #region Icao24
        [TestMethod]
        public void CustomConvert_Icao24_Null_String_Is_Invalid_Icao()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24(null));
        }

        [TestMethod]
        public void CustomConvert_Icao24_Empty_String_Is_Invalid_Icao()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24(""));
        }

        [TestMethod]
        public void CustomConvert_Icao24_Whitespace_String_Is_Invalid_Icao()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24(" "));
        }

        [TestMethod]
        public void CustomConvert_Icao24_Six_Digit_Hex_Codes_Are_Valid()
        {
            Assert.AreEqual(0, CustomConvert.Icao24("000000"));
            Assert.AreEqual(1193046, CustomConvert.Icao24("123456"));
            Assert.AreEqual(11259375, CustomConvert.Icao24("ABCDEF"));
            Assert.AreEqual(16777215, CustomConvert.Icao24("FFFFFF"));
        }

        [TestMethod]
        public void CustomConvert_Icao24_Seven_Digit_Hex_Codes_Are_Invalid()
        {
            Assert.AreEqual(-1, CustomConvert.Icao24("1000000"));
        }

        [TestMethod]
        public void CustomConvert_Icao24_Short_Hex_Codes_Are_Valid()
        {
            Assert.AreEqual(10, CustomConvert.Icao24("A"));
            Assert.AreEqual(170, CustomConvert.Icao24("AA"));
            Assert.AreEqual(2730, CustomConvert.Icao24("AAA"));
            Assert.AreEqual(43690, CustomConvert.Icao24("AAAA"));
            Assert.AreEqual(699050, CustomConvert.Icao24("AAAAA"));
        }

        [TestMethod]
        public void CustomConvert_Icao24_Is_Case_Insensitive()
        {
            Assert.AreEqual(1223476, CustomConvert.Icao24("12AB34"));
            Assert.AreEqual(1223476, CustomConvert.Icao24("12ab34"));
            Assert.AreEqual(1223476, CustomConvert.Icao24("12Ab34"));
            Assert.AreEqual(1223476, CustomConvert.Icao24("12aB34"));
        }

        [TestMethod]
        public void CustomConvert_Icao24_Non_Hex_Digits_Are_Invalid()
        {
            for(var ch = 32;ch < 256;++ch) {
                if((ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F')) {
                    continue;
                }
                var text = new String((char)ch, 6);
                Assert.AreEqual(-1, CustomConvert.Icao24(text), $"ICAO24 was {text}");
            }
        }

        [TestMethod]
        public void CustomConvert_Icao24_All_Valid_Hex_Digits_Are_Valid()
        {
            foreach(var ch in new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'A', 'b', 'B', 'c', 'C', 'd', 'D', 'e', 'E', 'f', 'F' }) {
                var text = new String(ch, 1);
                Assert.AreNotEqual(-1, CustomConvert.Icao24(text), $"Failed on {ch}");
            }
        }
        #endregion
    }
}
