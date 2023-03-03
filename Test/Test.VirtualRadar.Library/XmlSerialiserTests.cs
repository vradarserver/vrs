// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class XmlSerialiserTests
    {
        #region Comparable base class
        #pragma warning disable 0659 // overrode Equals without overriding GetHashCode
        public class Comparable
        {
            public override bool Equals(object obj)
            {
                return DoEqualityComparison(this, obj);
            }

            private bool DoEqualityComparison(object lhs, object rhs)
            {
                var result = Object.ReferenceEquals(lhs, rhs);
                if(!result) {
                    result = true;

                    var type = lhs.GetType();
                    if(type.IsValueType || type == typeof(string)) {
                        result = Object.Equals(lhs, rhs);
                    } else if(typeof(IList).IsAssignableFrom(type)) {
                        var lhsList = lhs as IList;
                        var rhsList = rhs as IList;
                        result = lhsList.Count == rhsList.Count;
                        for(var i = 0;result && i < lhsList.Count;++i) {
                            var lhsElement = lhsList[i];
                            var rhsElement = rhsList[i];
                            result = DoEqualityComparison(lhsElement, rhsElement);
                        }
                    } else {
                        foreach(var property in lhs.GetType().GetProperties()) {
                            var hasIgnoreAttribute = property.GetCustomAttributes(typeof(XmlIgnoreAttribute), inherit: false).Length > 0;
                            if(!hasIgnoreAttribute) {
                                var lhsValue = property.GetValue(lhs, null);
                                var rhsValue = property.GetValue(rhs, null);
                                result = DoEqualityComparison(lhsValue, rhsValue);
                                if(!result) break;
                            }
                        }
                    }
                }

                return result;
            }
        }
        #endregion

        #region Test serialise classes
        public class IntClass : Comparable
        {
            public int TheInt { get; set; }
        }

        public class DateClass : Comparable
        {
            public DateTime TheDate { get; set; }
        }

        public class StringClass : Comparable
        {
            public string TheString { get; set; }
        }

        public class BoolClass : Comparable
        {
            public bool TheBool { get; set; }
        }

        public class DoubleClass : Comparable
        {
            public double TheDouble { get; set; }
        }

        public class ByteListClass : Comparable
        {
            private List<byte> _ByteList = new List<byte>();
            public List<byte> ByteList { get { return _ByteList; } }
        }

        public class EnumClass : Comparable
        {
            public FileAccess FileAccess { get; set; }
        }

        public class SimpleParentClass : Comparable
        {
            public SimpleChildClass ChildClass { get; set; }
        }

        public class SimpleChildClass : Comparable
        {
            public string ChildValue { get; set; }
        }

        public class IgnoreAttributeClass : Comparable
        {
            public int NotIgnored { get; set; }

            [XmlIgnore]
            public virtual int Ignored { get; set; }
        }

        public class InheritedIgnoreClass : IgnoreAttributeClass
        {
            public override int Ignored
            {
                get { return base.Ignored; }
                set { base.Ignored = value; }
            }
        }

        public class ListOfListsClass : Comparable
        {
            private List<List<int>> _Lists = new List<List<int>>();
            public List<List<int>> Lists
            {
                get { return _Lists; }
            }
        }

        public class ListOfClassesClass : Comparable
        {
            private List<IntClass> _List = new List<IntClass>();
            public List<IntClass> List
            {
                get { return _List; }
            }
        }

        public class ValueTypes : Comparable
        {
            public byte AByte { get; set; }
            public char AChar { get; set; }
            public Int16 AnInt16 { get; set; }
            public Int32 AnInt32 { get; set; }
            public Int64 AnInt64 { get; set; }
            public Single ASingle { get; set; }
            public Double ADouble { get; set; }
            public DateTime ADateTime { get; set; }
            //public TimeSpan ATimeSpan { get; set; }   XmlSerializer emits an empty value for these
            //public IntPtr AnIntPtr { get; set; }      Not supported by XmlSerializer so we don't test it
            public Guid AGuid { get; set; }
            public UInt16 AUInt16 { get; set; }
            public UInt32 AUint32 { get; set; }
            public UInt64 AUInt64 { get; set; }
            public sbyte AnSByte { get; set; }
            public Decimal ADecimal { get; set; }
        }

        public class ListsOfValueTypes : Comparable
        {
            private List<byte> _Bytes = new List<byte>();
            public List<byte> Bytes { get { return _Bytes; } }

            private List<char> _Chars = new List<char>();
            public List<char> Chars { get { return _Chars; } }

            private List<Int16> _Int16s = new List<Int16>();
            public List<Int16> Int16s { get { return _Int16s; } }

            private List<Int32> _Int32s = new List<Int32>();
            public List<Int32> Int32s { get { return _Int32s; } }

            private List<Int64> _Int64s = new List<Int64>();
            public List<Int64> Int64s { get { return _Int64s; } }

            private List<Single> _Singles = new List<Single>();
            public List<Single> Singles { get { return _Singles; } }

            private List<Double> _Doubles = new List<Double>();
            public List<Double> Doubles { get { return _Doubles; } }

            private List<DateTime> _DateTimes = new List<DateTime>();
            public List<DateTime> DateTimes { get { return _DateTimes; } }

            private List<Guid> _Guids = new List<Guid>();
            public List<Guid> Guids { get { return _Guids; } }

            private List<UInt16> _UInt16s = new List<UInt16>();
            public List<UInt16> UInt16s { get { return _UInt16s; } }

            private List<UInt32> _UInt32s = new List<UInt32>();
            public List<UInt32> UInt32s { get { return _UInt32s; } }

            private List<UInt64> _UInt64s = new List<UInt64>();
            public List<UInt64> UInt64s { get { return _UInt64s; } }

            private List<SByte> _SBytes = new List<SByte>();
            public List<SByte> SBytes { get { return _SBytes; } }

            private List<Decimal> _Decimals = new List<Decimal>();
            public List<Decimal> Decimals { get { return _Decimals; } }
        }

        public class ListOfStrings : Comparable
        {
            private List<string> _Strings = new List<string>();
            public List<string> Strings { get { return _Strings; } }
        }

        public class IntPtrClass : Comparable
        {
            public IntPtr TheIntPtr { get; set; }
        }

        public class TimeSpanClass : Comparable
        {
            public TimeSpan TheTimeSpan { get; set; }
        }
        #endregion

        #region Fields, TestContext, TestInitialise
        private IXmlSerialiser _Serialiser;
        private List<MemoryStream> _Streams;
        private Dictionary<TextWriter, StringBuilder> _TextWriters;
        private static string[] _Cultures = new string[] { "en-GB", "de-DE", "ru-RU", "fr-FR", "zh-CN" };

        [TestInitialize]
        public void TestInitialise()
        {
#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            _Serialiser = new global::VirtualRadar.Library.XmlSerialiser();
#pragma warning restore CS0618 // Type or member is obsolete
            _Streams = new List<MemoryStream>();
            _TextWriters = new Dictionary<TextWriter,StringBuilder>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach(var stream in _Streams) {
                stream.Dispose();
            }
            foreach(var textWriter in _TextWriters.Keys) {
                textWriter.Dispose();
            }
        }
        #endregion

        #region Helper methods
        private Stream CreateStream(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return new MemoryStream(bytes);
        }

        private TextReader CreateTextReader(string text)
        {
            return new StringReader(text);
        }

        private Stream CreateStream()
        {
            var result = new MemoryStream();
            _Streams.Add(result);

            return result;
        }

        private TextWriter CreateTextWriter()
        {
            var builder = new StringBuilder();
            var result = new StringWriter(builder);
            _TextWriters.Add(result, builder);

            return result;
        }

        private string GetStreamText(MemoryStream stream = null)
        {
            if(stream == null && _Streams.Count == 1) stream = _Streams[0];
            var bytes = stream.ToArray();
            return Encoding.UTF8.GetString(bytes);
        }

        private string GetTextWriterText(TextWriter textWriter = null)
        {
            if(textWriter == null && _TextWriters.Count == 1) textWriter = _TextWriters.First().Key;
            var builder = _TextWriters[textWriter];
            return builder.ToString();
        }

        private string GetXmlSerializerOutput(object obj)
        {
            var xmlSerialiser = new XmlSerializer(obj.GetType());
            using(var stream = new MemoryStream()) {
                xmlSerialiser.Serialize(stream, obj);
                var bytes = stream.ToArray();
                var result = Encoding.UTF8.GetString(bytes);

                // Serialising to a memory stream doesn't add the encoding marker. However IXmlSerialiser is expected
                // to always explicitly declare a UTF-8 encoding.
                result = result.Replace(@"<?xml version=""1.0""?>", @"<?xml version=""1.0"" encoding=""utf-8""?>");

                return result;
            }
        }

        private T DeserialiseWithXmlSerializer<T>(string text)
        {
            var xmlSerialiser = new XmlSerializer(typeof(T));
            using(var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(text))) {
                return (T)xmlSerialiser.Deserialize(memoryStream);
            }
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void XmlSerialiser_Ctor_Initialises_To_Known_Values_And_Properties_Work()
        {
            Assert.AreEqual(true, _Serialiser.XmlSerializerCompatible);
            Assert.AreEqual(false, _Serialiser.UseDefaultEnumValueIfUnknown);
        }
        #endregion

        #region Serialise Stream
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlSerialiser_Serialise_Stream_Throws_If_Passed_Null_Object()
        {
            _Serialiser.Serialise(null, CreateStream());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlSerialiser_Serialise_Stream_Throws_If_Passed_Null_Stream()
        {
            _Serialiser.Serialise(new IntClass(), (Stream)null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void XmlSerialiser_Serialise_Stream_Throws_If_Serialising_IntPtr()
        {
            _Serialiser.Serialise(new IntPtrClass(), CreateStream());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void XmlSerialiser_Serialise_Stream_Throws_If_Serialising_TimeSpan()
        {
            _Serialiser.Serialise(new TimeSpanClass(), CreateStream());
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_IntClass()
        {
            Produces_Expected_Text_For_IntClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_DateClass()
        {
            Produces_Expected_Text_For_DateClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_StringClass()
        {
            Produces_Expected_Text_For_StringClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_Null_String()
        {
            Produces_Expected_Text_For_Null_String(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_BoolClass()
        {
            Produces_Expected_Text_For_BoolClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_DoubleClass()
        {
            Produces_Expected_Text_For_DoubleClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_ByteList_Class()
        {
            Produces_Expected_Text_For_ByteList_Class(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_EnumClass()
        {
            Produces_Expected_Text_For_EnumClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_SimpleParentClass()
        {
            Produces_Expected_Text_For_SimpleParentClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_SimpleParentClass_Null_Child()
        {
            Produces_Expected_Text_For_SimpleParentClass_Null_Child(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_List_Of_Lists()
        {
            Produces_Expected_Text_For_List_Of_Lists(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_IgnoreAttributeClass()
        {
            Produces_Expected_Text_For_IgnoreAttributeClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_InheritedIgnoreClass()
        {
            Produces_Expected_Text_For_InheritedIgnoreClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_ValueTypes()
        {
            Produces_Expected_Text_For_ValueTypes(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_IntPtrClass()
        {
            Produces_Expected_Text_For_IntPtrClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_TimeSpanClass()
        {
            Produces_Expected_Text_For_TimeSpanClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_ListOfClassesClass()
        {
            Produces_Expected_Text_For_ListOfClassesClass(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_ListOfClassesClass_With_Null_Element()
        {
            Produces_Expected_Text_For_ListOfClassesClass_With_Null_Element(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_ListsOfValueTypes()
        {
            Produces_Expected_Text_For_ListsOfValueTypes(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_Stream_Produces_Expected_Text_For_ListOfStrings()
        {
            Produces_Expected_Text_For_ListOfStrings(r => {
                _Serialiser.Serialise(r, CreateStream());
                return GetStreamText();
            });
        }
        #endregion

        #region Serialise TextWriter
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlSerialiser_Serialise_TextWriter_Throws_If_Passed_Null_Object()
        {
            _Serialiser.Serialise(null, CreateTextWriter());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlSerialiser_Serialise_TextWriter_Throws_If_Passed_Null_TextWriter()
        {
            _Serialiser.Serialise(new IntClass(), (TextWriter)null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void XmlSerialiser_Serialise_TextWriter_Throws_If_Serialising_IntPtr()
        {
            _Serialiser.Serialise(new IntPtrClass(), CreateTextWriter());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void XmlSerialiser_Serialise_TextWriter_Throws_If_Serialising_TimeSpan()
        {
            _Serialiser.Serialise(new TimeSpanClass(), CreateTextWriter());
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_IntClass()
        {
            Produces_Expected_Text_For_IntClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_DateClass()
        {
            Produces_Expected_Text_For_DateClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_StringClass()
        {
            Produces_Expected_Text_For_StringClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_Null_String()
        {
            Produces_Expected_Text_For_Null_String(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_BoolClass()
        {
            Produces_Expected_Text_For_BoolClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_DoubleClass()
        {
            Produces_Expected_Text_For_DoubleClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_ByteList_Class()
        {
            Produces_Expected_Text_For_ByteList_Class(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_EnumClass()
        {
            Produces_Expected_Text_For_EnumClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_SimpleParentClass()
        {
            Produces_Expected_Text_For_SimpleParentClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_SimpleParentClass_Null_Child()
        {
            Produces_Expected_Text_For_SimpleParentClass_Null_Child(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_IgnoreAttributeClass()
        {
            Produces_Expected_Text_For_IgnoreAttributeClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_InheritedIgnoreClass()
        {
            Produces_Expected_Text_For_InheritedIgnoreClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_ValueTypes()
        {
            Produces_Expected_Text_For_ValueTypes(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_IntPtrClass()
        {
            Produces_Expected_Text_For_IntPtrClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_TimeSpanClass()
        {
            Produces_Expected_Text_For_TimeSpanClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_ListOfClassesClass()
        {
            Produces_Expected_Text_For_ListOfClassesClass(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_ListOfClassesClass_With_Null_Element()
        {
            Produces_Expected_Text_For_ListOfClassesClass_With_Null_Element(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_ListsOfValueTypes()
        {
            Produces_Expected_Text_For_ListsOfValueTypes(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_ListOfStrings()
        {
            Produces_Expected_Text_For_ListOfStrings(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }

        [TestMethod]
        public void XmlSerialiser_Serialise_TextWriter_Produces_Expected_Text_For_ListOfStrings_With_Null_Element()
        {
            Produces_Expected_Text_For_ListOfStrings_With_Null_Element(r => {
                _Serialiser.Serialise(r, CreateTextWriter());
                return GetTextWriterText();
            });
        }
        #endregion

        #region Common Serialise tests
        private void Produces_Expected_Text_For_IntClass(Func<object, string> run)
        {
            var obj = new IntClass() { TheInt = 101 };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_DateClass(Func<object, string> run)
        {
            foreach(var cultureName in _Cultures) {
                TestCleanup();
                TestInitialise();

                using(var cultureSwitcher = new CultureSwitcher(cultureName)) {
                    var obj = new DateClass() { TheDate = new DateTime(2015, 3, 23) };
                    var expected = GetXmlSerializerOutput(obj);
                    var actual = run(obj);
                    Assert.AreEqual(expected, actual, cultureName);
                }
            }
        }

        private void Produces_Expected_Text_For_StringClass(Func<object, string> run)
        {
            var obj = new StringClass() { TheString = "No Reptiles" };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_Null_String(Func<object, string> run)
        {
            var obj = new StringClass() { TheString = null };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_BoolClass(Func<object, string> run)
        {
            var obj = new BoolClass() { TheBool = true };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_DoubleClass(Func<object, string> run)
        {
            foreach(var cultureName in _Cultures) {
                TestCleanup();
                TestInitialise();

                using(var cultureSwitcher = new CultureSwitcher(cultureName)) {
                    var obj = new DoubleClass() { TheDouble = 1.234 };
                    var expected = GetXmlSerializerOutput(obj);
                    var actual = run(obj);
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        private void Produces_Expected_Text_For_ByteList_Class(Func<object, string> run)
        {
            var obj = new ByteListClass() { ByteList = { 1, 2 } };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_EnumClass(Func<object, string> run)
        {
            var obj = new EnumClass() { FileAccess = FileAccess.ReadWrite };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_SimpleParentClass(Func<object, string> run)
        {
            var obj = new SimpleParentClass() { ChildClass = new SimpleChildClass() { ChildValue = "To The Blade", } };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_SimpleParentClass_Null_Child(Func<object, string> run)
        {
            var obj = new SimpleParentClass() { ChildClass = null };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        public void Produces_Expected_Text_For_List_Of_Lists(Func<object, string> run)
        {
            var obj = new ListOfListsClass() { Lists = { new List<int>() { 1, 2 }, new List<int>() { 3, 4 } } };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_IgnoreAttributeClass(Func<object, string> run)
        {
            var obj = new IgnoreAttributeClass() { Ignored = 1, NotIgnored = 2 };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_InheritedIgnoreClass(Func<object, string> run)
        {
            var obj = new InheritedIgnoreClass() { Ignored = 1, NotIgnored = 2 };
            //var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);

            // The XmlSerializer and my implementation return the same XML but the nodes are in a different order
            // I'll just double-check to make sure the expected nodes are in there somewhere
            Assert.IsTrue(actual.Contains(@"<Ignored>1</Ignored>"));
            Assert.IsTrue(actual.Contains(@"<NotIgnored>2</NotIgnored>"));
        }

        private void Produces_Expected_Text_For_ValueTypes(Func<object, string> run)
        {
            var obj = new ValueTypes() {
                AByte = 1,
                AChar = '2',
                ADateTime = new DateTime(2013, 4, 25),
                ADouble = 6.7,
                AGuid = Guid.NewGuid(),
                AnInt16 = 8,
                AnInt32 = 9,
                AnInt64 = 10,
                ASingle = 12.13F,
                AUInt16 = 14,
                AUint32 = 15,
                AUInt64 = 16,
                AnSByte = -17,
                ADecimal = 18,
            };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_IntPtrClass(Func<object, string> run)
        {
            _Serialiser.XmlSerializerCompatible = false;
            var obj = new IntPtrClass() { TheIntPtr = new IntPtr(123456) };
            var expected = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                           @"<IntPtrClass xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">" +
                           @"  <TheIntPtr>123456</TheIntPtr>" +
                           @"</IntPtrClass>";
            var actual = run(obj).Replace("\r", "").Replace("\n", "");
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_TimeSpanClass(Func<object, string> run)
        {
            _Serialiser.XmlSerializerCompatible = false;
            var obj = new TimeSpanClass() { TheTimeSpan = new TimeSpan(15, 14, 13, 12) };
            var expected = @"<?xml version=""1.0"" encoding=""utf-8""?>" +
                           @"<TimeSpanClass xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">" +
                           @"  <TheTimeSpan>15.14:13:12</TheTimeSpan>" +
                           @"</TimeSpanClass>";
            var actual = run(obj).Replace("\r", "").Replace("\n", "");
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_ListOfClassesClass(Func<object, string> run)
        {
            var obj = new ListOfClassesClass() { List = { new IntClass() { TheInt = 2 } } };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_ListOfClassesClass_With_Null_Element(Func<object, string> run)
        {
            var obj = new ListOfClassesClass() { List = { null } };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_ListsOfValueTypes(Func<object, string> run)
        {
            var obj = new ListsOfValueTypes() {
                Bytes = { 1, 2, },
                Chars = { 'a', 'b', },
                DateTimes = { DateTime.Today, DateTime.Today.AddDays(-1), },
                Doubles = { 1.2, 3.4, },
                Guids = { Guid.NewGuid(), Guid.NewGuid(), },
                Int16s = { 3, 4, },
                Int32s = { 5, 6, },
                Int64s = { 7, 8, },
                Singles = { 9.10f, 11.12f },
                UInt16s = { 13, 14 },
                UInt32s = { 15, 16 },
                UInt64s = { 17, 18 },
                SBytes = { -19, 20 },
                Decimals = { -21, 22 },
            };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_ListOfStrings(Func<object, string> run)
        {
            var obj = new ListOfStrings() { Strings = { "Warm Healer", "Regret" }, };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_ListOfStrings_With_Null_Element(Func<object, string> run)
        {
            var obj = new ListOfStrings() { Strings = { null }, };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Expected_Text_For_WebServerSettings_With_NonNull_Hash(Func<object, string> run)
        {
            var obj = new WebServerSettings() { BasicAuthenticationPasswordHash = new Hash() { Version = 1 } };
            var expected = GetXmlSerializerOutput(obj);
            var actual = run(obj);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Deserialise Stream
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlSerialiser_Deserialise_Stream_Throws_If_Passed_Null_Stream()
        {
            _Serialiser.Deserialise<IntClass>((Stream)null);
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_IntClass()
        {
            Produces_Correct_Object_For_IntClass(r => {
                return _Serialiser.Deserialise<IntClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_DateClass()
        {
            Produces_Correct_Object_For_DateClass(r => {
                return _Serialiser.Deserialise<DateClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_StringClass()
        {
            Produces_Correct_Object_For_StringClass(r => {
                return _Serialiser.Deserialise<StringClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_StringClass_With_Null_String()
        {
            Produces_Correct_Object_For_StringClass_With_Null_String(r => {
                return _Serialiser.Deserialise<StringClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_BoolClass()
        {
            Produces_Correct_Object_For_BoolClass(r => {
                return _Serialiser.Deserialise<BoolClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_DoubleClass()
        {
            Produces_Correct_Object_For_DoubleClass(r => {
                return _Serialiser.Deserialise<DoubleClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_ByteListClass()
        {
            Produces_Correct_Object_For_ByteListClass(r => {
                return _Serialiser.Deserialise<ByteListClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_EnumClass()
        {
            Produces_Correct_Object_For_EnumClass(r => {
                return _Serialiser.Deserialise<EnumClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_SimpleParentClass()
        {
            Produces_Correct_Object_For_SimpleParentClass(r => {
                return _Serialiser.Deserialise<SimpleParentClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_SimpleParentClass_With_Null_Child()
        {
            Produces_Correct_Object_For_SimpleParentClass_With_Null_Child(r => {
                return _Serialiser.Deserialise<SimpleParentClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Expected_Text_For_List_Of_Lists()
        {
            Produces_Correct_Object_For_ListOfListsClass(r => {
                return _Serialiser.Deserialise<ListOfListsClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_ValueTypes()
        {
            Produces_Correct_Object_For_ValueTypes(r => {
                return _Serialiser.Deserialise<ValueTypes>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_IntPtrClass()
        {
            Produces_Correct_Object_For_IntPtrClass(r => {
                return _Serialiser.Deserialise<IntPtrClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_TimeSpanClass()
        {
            Produces_Correct_Object_For_TimeSpanClass(r => {
                return _Serialiser.Deserialise<TimeSpanClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_ListOfClassesClass()
        {
            Produces_Correct_Object_For_ListOfClassesClass(r => {
                return _Serialiser.Deserialise<ListOfClassesClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_ListOfClassesClass_With_Null_Element()
        {
            Produces_Correct_Object_For_ListOfClassesClass_With_Null_Element(r => {
                return _Serialiser.Deserialise<ListOfClassesClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_IgnoreAttributeClass()
        {
            Produces_Correct_Object_For_IgnoreAttributeClass(r => {
                return _Serialiser.Deserialise<IgnoreAttributeClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_ListsOfValueTypes()
        {
            Produces_Correct_Object_For_ListsOfValueTypes(r => {
                return _Serialiser.Deserialise<ListsOfValueTypes>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_ListOfStrings()
        {
            Produces_Correct_Object_For_ListOfStrings(r => {
                return _Serialiser.Deserialise<ListOfStrings>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Produces_Correct_Object_For_ListOfStrings_With_Null_Element()
        {
            Produces_Correct_Object_For_ListOfStrings_With_Null_Element(r => {
                return _Serialiser.Deserialise<ListOfStrings>(CreateStream(r));
            });
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void XmlSerialiser_Deserialise_Stream_Throws_Exception_By_Default_If_Enum_Value_Unknown()
        {
            Throws_Exception_By_Default_If_Enum_Value_Unknown(r => {
                return _Serialiser.Deserialise<EnumClass>(CreateStream(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_Stream_Can_Use_Default_Enum_Value_For_Unknown_Values_If_Required()
        {
            Can_Use_Default_Enum_Value_For_Unknown_Values_If_Required(r => {
                return _Serialiser.Deserialise<EnumClass>(CreateStream(r));
            });
        }
        #endregion

        #region Deserialise TextReader
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void XmlSerialiser_Deserialise_TextReader_Throws_If_Passed_Null_TextReader()
        {
            _Serialiser.Deserialise<IntClass>((TextReader)null);
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_IntClass()
        {
            Produces_Correct_Object_For_IntClass(r => {
                return _Serialiser.Deserialise<IntClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_DateClass()
        {
            Produces_Correct_Object_For_DateClass(r => {
                return _Serialiser.Deserialise<DateClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_StringClass()
        {
            Produces_Correct_Object_For_StringClass(r => {
                return _Serialiser.Deserialise<StringClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_StringClass_With_Null_String()
        {
            Produces_Correct_Object_For_StringClass_With_Null_String(r => {
                return _Serialiser.Deserialise<StringClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_BoolClass()
        {
            Produces_Correct_Object_For_BoolClass(r => {
                return _Serialiser.Deserialise<BoolClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_DoubleClass()
        {
            Produces_Correct_Object_For_DoubleClass(r => {
                return _Serialiser.Deserialise<DoubleClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_ByteListClass()
        {
            Produces_Correct_Object_For_ByteListClass(r => {
                return _Serialiser.Deserialise<ByteListClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_EnumClass()
        {
            Produces_Correct_Object_For_EnumClass(r => {
                return _Serialiser.Deserialise<EnumClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_SimpleParentClass()
        {
            Produces_Correct_Object_For_SimpleParentClass(r => {
                return _Serialiser.Deserialise<SimpleParentClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_SimpleParentClass_With_Null_Child()
        {
            Produces_Correct_Object_For_SimpleParentClass_With_Null_Child(r => {
                return _Serialiser.Deserialise<SimpleParentClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Expected_Text_For_List_Of_Lists()
        {
            Produces_Correct_Object_For_ListOfListsClass(r => {
                return _Serialiser.Deserialise<ListOfListsClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_ValueTypes()
        {
            Produces_Correct_Object_For_ValueTypes(r => {
                return _Serialiser.Deserialise<ValueTypes>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_IntPtrClass()
        {
            Produces_Correct_Object_For_IntPtrClass(r => {
                return _Serialiser.Deserialise<IntPtrClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_TimeSpanClass()
        {
            Produces_Correct_Object_For_TimeSpanClass(r => {
                return _Serialiser.Deserialise<TimeSpanClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_ListOfClassesClass()
        {
            Produces_Correct_Object_For_ListOfClassesClass(r => {
                return _Serialiser.Deserialise<ListOfClassesClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_ListOfClassesClass_With_Null_Element()
        {
            Produces_Correct_Object_For_ListOfClassesClass_With_Null_Element(r => {
                return _Serialiser.Deserialise<ListOfClassesClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_IgnoreAttributeClass()
        {
            Produces_Correct_Object_For_IgnoreAttributeClass(r => {
                return _Serialiser.Deserialise<IgnoreAttributeClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_ListsOfValueTypes()
        {
            Produces_Correct_Object_For_ListsOfValueTypes(r => {
                return _Serialiser.Deserialise<ListsOfValueTypes>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_ListOfStrings()
        {
            Produces_Correct_Object_For_ListOfStrings(r => {
                return _Serialiser.Deserialise<ListOfStrings>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Produces_Correct_Object_For_ListOfStrings_With_Null_Element()
        {
            Produces_Correct_Object_For_ListOfStrings_With_Null_Element(r => {
                return _Serialiser.Deserialise<ListOfStrings>(CreateTextReader(r));
            });
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentException))]
        public void XmlSerialiser_Deserialise_TextReader_Throws_Exception_By_Default_If_Enum_Value_Unknown()
        {
            Throws_Exception_By_Default_If_Enum_Value_Unknown(r => {
                return _Serialiser.Deserialise<EnumClass>(CreateTextReader(r));
            });
        }

        [TestMethod]
        public void XmlSerialiser_Deserialise_TextReader_Can_Use_Default_Enum_Value_For_Unknown_Values_If_Required()
        {
            Can_Use_Default_Enum_Value_For_Unknown_Values_If_Required(r => {
                return _Serialiser.Deserialise<EnumClass>(CreateTextReader(r));
            });
        }
        #endregion

        #region Common Deserialise tests
        private void Produces_Correct_Object_For_IntClass(Func<string, IntClass> run)
        {
            var original = new IntClass() { TheInt = 102, };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<IntClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_DateClass(Func<string, DateClass> run)
        {
            var original = new DateClass() { TheDate = new DateTime(2015, 1, 20) };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<DateClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_StringClass(Func<string, StringClass> run)
        {
            var original = new StringClass() { TheString = "Zero Pharoh" };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<StringClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_StringClass_With_Null_String(Func<string, StringClass> run)
        {
            var original = new StringClass() { TheString = null };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<StringClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_BoolClass(Func<string, BoolClass> run)
        {
            var original = new BoolClass() { TheBool = true, };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<BoolClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_DoubleClass(Func<string, DoubleClass> run)
        {
            foreach(var cultureName in _Cultures) {
                TestCleanup();
                TestInitialise();

                using(var switcher = new CultureSwitcher(cultureName)) {
                    var original = new DoubleClass() { TheDouble = 1.234 };
                    var text = GetXmlSerializerOutput(original);
                    var expected = DeserialiseWithXmlSerializer<DoubleClass>(text);
                    var actual = run(text);
                    Assert.AreEqual(expected, actual, cultureName);
                }
            }
        }

        private void Produces_Correct_Object_For_ByteListClass(Func<string, ByteListClass> run)
        {
            var original = new ByteListClass() { ByteList = { 1, 2 } };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ByteListClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_EnumClass(Func<string, EnumClass> run)
        {
            var original = new EnumClass() { FileAccess = FileAccess.ReadWrite };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<EnumClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_SimpleParentClass(Func<string, SimpleParentClass> run)
        {
            var original = new SimpleParentClass() { ChildClass = new SimpleChildClass() { ChildValue = "Blast Doors" } };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<SimpleParentClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_SimpleParentClass_With_Null_Child(Func<string, SimpleParentClass> run)
        {
            var original = new SimpleParentClass() { ChildClass = null };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<SimpleParentClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_ListOfListsClass(Func<string, ListOfListsClass> run)
        {
            var original = new ListOfListsClass() { Lists = { new List<int>() { 1, 2 }, new List<int>() { 3, 4 } } };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ListOfListsClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_ValueTypes(Func<string, ValueTypes> run)
        {
            var original = new ValueTypes() {
                AByte = byte.MaxValue,
                AChar = char.MaxValue,
                ADateTime = DateTime.MaxValue,
                ADouble = Double.MaxValue,
                AGuid = Guid.NewGuid(),
                AnInt16 = Int16.MinValue,
                AnInt32 = Int32.MaxValue,
                AnInt64 = Int64.MinValue,
                ASingle = Single.MaxValue,
                AUInt16 = UInt16.MaxValue,
                AUint32 = UInt32.MaxValue,
                AUInt64 = UInt64.MaxValue,
                AnSByte = SByte.MinValue,
                ADecimal = Decimal.MinValue,
            };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ValueTypes>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_IntPtrClass(Func<string, IntPtrClass> run)
        {
            var original = new IntPtrClass() { TheIntPtr = new IntPtr(10) };

            // XmlSerializer can't cope with these so we're testing a round-trip instead
            _Serialiser.XmlSerializerCompatible = false;
            _Serialiser.Serialise(original, CreateStream());
            var text = GetStreamText();

            TestCleanup();
            TestInitialise();

            var expected = original;
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_TimeSpanClass(Func<string, TimeSpanClass> run)
        {
            foreach(var cultureName in _Cultures) {
                TestCleanup();
                TestInitialise();

                using(var switcher = new CultureSwitcher(cultureName)) {
                    var original = new TimeSpanClass() { TheTimeSpan = new TimeSpan(44, 23, 59, 58) };

                    // XmlSerializer can't cope with these so we're testing a round-trip instead
                    _Serialiser.XmlSerializerCompatible = false;
                    _Serialiser.Serialise(original, CreateStream());
                    var text = GetStreamText();

                    TestCleanup();
                    TestInitialise();

                    var expected = original;
                    var actual = run(text);
                    Assert.AreEqual(expected, actual, cultureName);
                }
            }
        }

        private void Produces_Correct_Object_For_ListOfClassesClass(Func<string, ListOfClassesClass> run)
        {
            var original = new ListOfClassesClass() { List = { new IntClass() { TheInt = 7 } } };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ListOfClassesClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_ListOfClassesClass_With_Null_Element(Func<string, ListOfClassesClass> run)
        {
            var original = new ListOfClassesClass() { List = { null } };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ListOfClassesClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_IgnoreAttributeClass(Func<string, IgnoreAttributeClass> run)
        {
            var original = new IgnoreAttributeClass() { NotIgnored = 2 };
            var text = GetXmlSerializerOutput(original);
            text = text.Replace(@"<NotIgnored>2</NotIgnored>", @"<Ignored>1</Ignored><NotIgnored>2</NotIgnored>");

            var expected = DeserialiseWithXmlSerializer<IgnoreAttributeClass>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_ListsOfValueTypes(Func<string, ListsOfValueTypes> run)
        {
            var original = new ListsOfValueTypes() {
                Bytes = { 1, 2, },
                Chars = { 'a', 'b', },
                DateTimes = { DateTime.Today, DateTime.Today.AddDays(-1), },
                Doubles = { 1.2, 3.4, },
                Guids = { Guid.NewGuid(), Guid.NewGuid(), },
                Int16s = { 3, 4, },
                Int32s = { 5, 6, },
                Int64s = { 7, 8, },
                Singles = { 9.10f, 11.12f },
                UInt16s = { 13, 14 },
                UInt32s = { 15, 16 },
                UInt64s = { 17, 18 },
                SBytes = { -19, 20 },
                Decimals = { -21, 22 },
            };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ListsOfValueTypes>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_ListOfStrings(Func<string, ListOfStrings> run)
        {
            var original = new ListOfStrings() { Strings = { "Blast Doors", "Fortune 500" }, };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ListOfStrings>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Produces_Correct_Object_For_ListOfStrings_With_Null_Element(Func<string, ListOfStrings> run)
        {
            var original = new ListOfStrings() { Strings = { null }, };
            var text = GetXmlSerializerOutput(original);
            var expected = DeserialiseWithXmlSerializer<ListOfStrings>(text);
            var actual = run(text);
            Assert.AreEqual(expected, actual);
        }

        private void Throws_Exception_By_Default_If_Enum_Value_Unknown(Func<string, EnumClass> run)
        {
            var text = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <EnumClass xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                  <FileAccess>NotAFileAccessValue</FileAccess>
                </EnumClass>
            ";
            run(text);
        }

        private void Can_Use_Default_Enum_Value_For_Unknown_Values_If_Required(Func<string, EnumClass> run)
        {
            var text = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <EnumClass xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                  <FileAccess>NotAFileAccessValue</FileAccess>
                </EnumClass>
            ";
            _Serialiser.UseDefaultEnumValueIfUnknown = true;
            var enumClass = run(text);

            Assert.AreEqual((FileAccess)0, enumClass.FileAccess);
        }
        #endregion
    }
}
