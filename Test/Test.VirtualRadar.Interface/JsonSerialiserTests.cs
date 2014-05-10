// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using System.Runtime.Serialization;
using InterfaceFactory;
using System.IO;
using Test.Framework;
using System.Threading;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class JsonSerialiserTests
    {
        #region Private classes - ValueTypes, StringType
        [DataContract]
        class ValueTypes
        {
            [DataMember]
            public bool BoolValue { get; set; }

            public bool UnusedBool { get; set; }

            [DataMember]
            public bool? NullableBool { get; set; }

            [DataMember]
            public int Int { get; set; }

            [DataMember]
            public int? NullableInt { get; set; }

            [DataMember]
            public long Long { get; set; }

            [DataMember]
            public long? NullableLong { get; set; }

            [DataMember]
            public float Float { get; set; }

            [DataMember]
            public float? NullableFloat { get; set; }

            [DataMember]
            public double Double { get; set; }

            [DataMember]
            public double? NullableDouble { get; set; }

            [DataMember]
            public DateTime DateTime { get; set; }

            [DataMember]
            public DateTime? NullableDateTime { get; set; }
        }

        [DataContract]
        class StringType
        {
            [DataMember]
            public string Text { get; set; }
        }

        [DataContract]
        class ValueListContainer
        {
            [DataMember]
            public List<int> List { get; set; }
        }

        [DataContract]
        class StringListContainer
        {
            [DataMember]
            public List<string> List { get; set; }
        }

        [DataContract]
        class Attributes
        {
            [DataMember(Name="Renamed")]
            public int OriginalName { get; set; }

            [DataMember(EmitDefaultValue=false, Name="NEDS")]
            public string NoEmitDefaultString { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public int NoEmitDefaultInt { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public bool NoEmitDefaultBool { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public long NoEmitDefaultLong { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public DateTime NoEmitDefaultDateTime { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public bool? NoEmitDefaultNullable { get; set; }
        }

        [DataContract]
        class SmallClass
        {
            [DataMember]
            public int Value { get; set; }
        }

        [DataContract]
        class SmallClassContainer
        {
            [DataMember]
            public int ContainerValue { get; set; }

            [DataMember]
            public List<SmallClass> SmallClasses { get; set; }
        }
        
        [DataContract]
        class NestedClassContainer
        {
            [DataMember]
            public SmallClass SmallClass { get; set; }
        }

        [DataContract]
        class NestedTwiceContainer
        {
            [DataMember]
            public SmallClass SmallClass1 { get; set; }

            [DataMember]
            public SmallClass SmallClass2 { get; set; }
        }
        #endregion

        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private JsonSerialiser _JsonSerialiser;
        private MemoryStream _Stream;

        [TestInitialize]
        public void TestInitialise()
        {
            _JsonSerialiser = new JsonSerialiser();
            _Stream = new MemoryStream();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if(_Stream != null) {
                _Stream.Dispose();
                _Stream = null;
            }
        }

        private string GetJson()
        {
            return _Stream.Length == 0 ? "" : Encoding.UTF8.GetString(_Stream.ToArray());
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JsonSerialiser_Initialise_Throws_If_Passed_Null()
        {
            _JsonSerialiser.Initialise(null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void JsonSerialiser_Initialise_Throws_If_Passed_Type_Not_Marked_With_DataContract()
        {
            _JsonSerialiser.Initialise(typeof(JsonSerialiserTests));
        }
        #endregion

        #region WriteObject
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void JsonSerialiser_WriteObject_Throws_If_Called_Before_Initialise()
        {
            _JsonSerialiser.WriteObject(_Stream, this);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void JsonSerialiser_WriteObject_Throws_If_Passed_Null_Stream()
        {
            _JsonSerialiser.Initialise(typeof(ValueTypes));
            _JsonSerialiser.WriteObject(null, new ValueTypes());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void JsonSerialiser_WriteObject_Throws_If_Type_Passed_Is_Not_One_Passed_To_Initialise()
        {
            _JsonSerialiser.Initialise(typeof(ValueTypes));
            _JsonSerialiser.WriteObject(_Stream, this);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "JsonValueTypes$")]
        public void JsonSerialiser_WriteObject_Writes_ValueTypes_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var obj = new ValueTypes() {
                BoolValue = worksheet.Bool("BoolValue"),
                UnusedBool = worksheet.Bool("UnusedBool"),
                NullableBool = worksheet.NBool("NullableBool"),
                Int = worksheet.Int("Int"),
                NullableInt = worksheet.NInt("NullableInt"),
                Long = worksheet.Long("Long"),
                NullableLong = worksheet.NLong("NullableLong"),
                Float = worksheet.Float("Float"),
                NullableFloat = worksheet.NFloat("NullableFloat"),
                Double = worksheet.Double("Double"),
                NullableDouble = worksheet.NDouble("NullableDouble"),
                DateTime = worksheet.DateTime("DateTime"),
                NullableDateTime = worksheet.NDateTime("NullableDateTime"),
            };

            _JsonSerialiser.Initialise(typeof(ValueTypes));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(worksheet.EString("Json"), GetJson());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "JsonValueTypes$")]
        public void JsonSerialiser_WriteObject_Writes_ValueTypes_Correctly_For_Non_UK_Cultures()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            foreach(var culture in new string[] { "en-US", "de-DE", "fr-FR", "ru-RU" }) {
                using(var cultureSwitcher = new CultureSwitcher(culture)) {
                    TestCleanup();
                    TestInitialise();

                    var obj = new ValueTypes() {
                        BoolValue = worksheet.Bool("BoolValue"),
                        UnusedBool = worksheet.Bool("UnusedBool"),
                        NullableBool = worksheet.NBool("NullableBool"),
                        Int = worksheet.Int("Int"),
                        NullableInt = worksheet.NInt("NullableInt"),
                        Long = worksheet.Long("Long"),
                        NullableLong = worksheet.NLong("NullableLong"),
                        Float = worksheet.Float("Float"),
                        NullableFloat = worksheet.NFloat("NullableFloat"),
                        Double = worksheet.Double("Double"),
                        NullableDouble = worksheet.NDouble("NullableDouble"),
                        DateTime = worksheet.DateTime("DateTime"),
                        NullableDateTime = worksheet.NDateTime("NullableDateTime"),
                    };

                    _JsonSerialiser.Initialise(typeof(ValueTypes));
                    _JsonSerialiser.WriteObject(_Stream, obj);

                    var message = String.Format("when culture is {0}", culture);
                    Assert.AreEqual(worksheet.EString("Json"), GetJson(), message);
                }
            }
        }

        [TestMethod]
        public void JsonSerialiser_WriteObject_Does_Not_Leave_Current_Culture_Set_To_Invariant()
        {
            using(var cultureSwitcher = new CultureSwitcher("de-DE")) {
                _JsonSerialiser.Initialise(typeof(SmallClass));
                _JsonSerialiser.WriteObject(_Stream, new SmallClass());

                Assert.AreEqual("de-DE", Thread.CurrentThread.CurrentCulture.Name);
                Assert.AreEqual("de-DE", Thread.CurrentThread.CurrentUICulture.Name);
            }
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "JsonStrings$")]
        public void JsonSerialiser_WriteObject_Writes_Strings_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var obj = new StringType() {
                Text = worksheet.EString("Text"),
            };
            if(obj.Text != null) {
                obj.Text = obj.Text
                              .Replace(@"\b", "\b")
                              .Replace(@"\f", "\f")
                              .Replace(@"\n", "\n")
                              .Replace(@"\r", "\r")
                              .Replace(@"\t", "\t");
            }

            _JsonSerialiser.Initialise(typeof(StringType));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(worksheet.EString("Json"), GetJson());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "JsonValueLists$")]
        public void JsonSerialiser_WriteObject_Writes_Lists_Of_ValueTypes_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var container = new ValueListContainer();
            if(worksheet.EString("Value1") != null) {
                var list = new List<int>();
                for(int column = 1;column <= 3;++column) {
                    var name = String.Format("Value{0}", column);
                    if(String.IsNullOrEmpty(worksheet.EString(name))) continue;
                    list.Add(worksheet.Int(name));
                }

                container.List = list;
            }

            _JsonSerialiser.Initialise(typeof(ValueListContainer));
            _JsonSerialiser.WriteObject(_Stream, container);

            Assert.AreEqual(worksheet.EString("Json"), GetJson());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "JsonAttributes$")]
        public void JsonSerialiser_WriteObject_Honours_Important_DataMember_Attributes()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var obj = new Attributes() {
                OriginalName = worksheet.Int("OriginalName"),
                NoEmitDefaultString = worksheet.EString("NoEmitDefaultString"),
                NoEmitDefaultInt = worksheet.Int("NoEmitDefaultInt"),
                NoEmitDefaultBool = worksheet.Bool("NoEmitDefaultBool"),
                NoEmitDefaultLong = worksheet.Long("NoEmitDefaultLong"),
                NoEmitDefaultDateTime = worksheet.DateTime("NoEmitDefaultDateTime"),
                NoEmitDefaultNullable = worksheet.NBool("NoEmitDefaultNullable"),
            };

            _JsonSerialiser.Initialise(typeof(Attributes));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(worksheet.EString("Json"), GetJson());
        }

        [TestMethod]
        public void JsonSerialiser_WriteObject_Writes_Lists_Of_Strings_Correctly()
        {
            var obj = new StringListContainer() {
                List = new List<string>() {
                    "A", null, "", "C"
                }
            };

            _JsonSerialiser.Initialise(typeof(StringListContainer));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(@"{""List"":[""A"",null,"""",""C""]}", GetJson());
        }

        [TestMethod]
        public void JsonSerialiser_WriteObject_Writes_Lists_Of_Classes_Correctly()
        {
            var obj = new SmallClassContainer() {
                ContainerValue = 42,
                SmallClasses = new List<SmallClass>() {
                    new SmallClass() { Value = 7 },
                    new SmallClass() { Value = 4 },
                },
            };

            _JsonSerialiser.Initialise(typeof(SmallClassContainer));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(@"{""ContainerValue"":42,""SmallClasses"":[{""Value"":7},{""Value"":4}]}", GetJson());
        }

        [TestMethod]
        public void JsonSerialiser_WriteObject_Writes_Nested_Classes_Correctly()
        {
            var obj = new NestedClassContainer() {
                SmallClass = new SmallClass() {
                    Value = 42,
                }
            };

            _JsonSerialiser.Initialise(typeof(NestedClassContainer));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(@"{""SmallClass"":{""Value"":42}}", GetJson());
        }

        [TestMethod]
        public void JsonSerialiser_WriteObject_Writes_Null_Nested_Classes_Correctly()
        {
            var obj = new NestedClassContainer() {
                SmallClass = null
            };

            _JsonSerialiser.Initialise(typeof(NestedClassContainer));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(@"{""SmallClass"":null}", GetJson());
        }

        [TestMethod]
        public void JsonSerialiser_WriteObject_Writes_Double_Nested_Classes_Correctly()
        {
            var obj = new NestedTwiceContainer() {
                SmallClass1 = new SmallClass() { Value = 12 },
                SmallClass2 = new SmallClass() { Value = 24 },
            };

            _JsonSerialiser.Initialise(typeof(NestedTwiceContainer));
            _JsonSerialiser.WriteObject(_Stream, obj);

            Assert.AreEqual(@"{""SmallClass1"":{""Value"":12},""SmallClass2"":{""Value"":24}}", GetJson());
        }
        #endregion
    }
}
