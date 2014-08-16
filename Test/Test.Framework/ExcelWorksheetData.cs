// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Data.Odbc;
using System.Data;

namespace Test.Framework
{
    /// <summary>
    /// A class to help when dealing with spreadsheet data-driven tests.
    /// </summary>
    /// <remarks><para>
    /// Spreadsheet data-driven tests have the advantage over database data-driven tests in that the source data is very easy to setup.
    /// You can use Excel or OpenOffice (saving the spreadsheet as a .XLS file) to create the source data in a worksheet and then tell
    /// VSTS to repeatedly run the test for each row in the sheet.
    /// </para><para>
    /// There are a few caveats when working with spreadsheets. The first is that <b>EVERY CELL MUST BE FORMATTED FOR TEXT</b>. If you
    /// fail to do this then the ADO.NET data adapter that is reading the spreadsheet will infer the type from the content of the first
    /// data row for the cell. If you have a column that contains the heading &quot;Text&quot; on row 1, the data value 42 on row 2 and
    /// the data value Hello on row 3 then you will get an exception when row 3 is read because the number on row 2 has fooled the data
    /// adapter into thinking ALL of the cells are numbers. So rule one, absolutely definitely never break it, when you start a new
    /// worksheet press Ctrl+A to select all cells and then set the format to TEXT.
    /// </para><para>
    /// Once you're past that it gets a lot easier. In your test class you need to declare the public read/write property <b>TestContext</b> of
    /// type <see cref="TestContext"/>. This is a magic property that is filled by VSTS at the start of the test and it will carry the data from
    /// the spreadsheet to your test.
    /// </para><para>
    /// To pull data from a worksheet in a test you add this attribute to the test method, replacing SPREADSHEET.XLS with the name of your
    /// spreadsheet and WORKSHEET$ with the name of the worksheet to read, suffixed with a dollar sign:
    /// </para><para>
    /// [DataSource("Data Source='SPREADSHEET.XLS';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
    /// "WORKSHEET$")]
    /// </para><para>
    /// You then create an instance of ExcelWorksheetData at the start of the test and use the read methods to pull data in, usually by column name.
    /// The read methods are all named after the type that they cast the value to, read methods for nullable value types all start with a capital
    /// N. There are special methods for reading strings where you need to be able to return either null or an empty string, methods to parse the
    /// names of enum values into the correct type and methods to parse a cell's text into a byte array.
    /// </para><para>
    /// Finally you need to get the spreadsheet copied into the test deployment folder by VSTS so that the test can use it. The easiest way to do this
    /// is to create a folder on a test project that contains all of your spreadsheets (Virtual Radar Server has a folder called TestFiles under the
    /// Test.Framework project for this) and then edit the test configuration file (in VSTS either double-click LocalTestRun.testrunconfig in Solution
    /// Items or use the Test | Edit Test Settings menu entry) and add the sub-folder with the spreadsheet(s) in to the Deployment section.
    /// </para></remarks>
    /// <example><para>
    /// This example tests the standard <see cref="ASCIIEncoding"/> object's GetString method. Assume that we have a spreadsheet called Tests.xls that
    /// contains a worksheet called TestData with the following rows and columns (all formatted as TEXT):
    /// </para>
    /// <list type="table">
    ///     <listheader>
    ///         <description>Bytes</description>
    ///         <description>Text</description>
    ///     </listheader>
    ///     <item>
    ///         <description>40</description>
    ///         <description>@</description>
    ///     </item>
    ///     <item>
    ///         <description>41</description>
    ///         <description>A</description>
    ///     </item>
    ///     <item>
    ///         <description>61 62 63</description>
    ///         <description>abc</description>
    ///     </item>
    /// </list>
    /// <para>
    /// The full code for the test class might be:
    /// </para>
    /// <code>
    /// [TestClass]
    /// public class ExampleTest
    /// {
    ///     public TestContext TestContext { get; set; }
    /// 
    ///     [TestMethod]
    ///     [DataSource("Data Source='Tests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
    ///     "TestData$")]
    ///     public void ASCIIEncoding_GetString_Converts_Byte_Arrays_To_Strings()
    ///     {
    ///         var worksheet = new ExcelWorksheetData(TestContext);
    ///
    ///         // You could do this in one line, as below, but to make it clearer we will spell it out step-by-step:
    ///         // Assert.AreEqual(worksheet.String("Text"), Encoding.ASCII.GetString(worksheet.ParseBytes("Bytes")));
    ///
    ///         byte[] input = worksheet.ParseBytes("Bytes");    // Read string in cell "Bytes" and parse into a byte array
    ///         string expected = worksheet.String("Text");      // Read content of "Text" cell as a string
    ///         string actual = Encoding.ASCII.GetString(input);
    ///
    ///         Assert.AreEqual(expected, actual);
    ///     }
    /// }
    /// </code>
    /// <para>
    /// When you run the test you will get 4 executions (one per row plus one for the overall test). In this case all 4 will pass. If one failed then you would get
    /// 3/4 passed, and so on. Double-clicking on the row for the test in TestResults will show a list of results for each row. Add two to the row number to translate
    /// from the row number in TestResults to the row number in the spreadsheet (e.g. row number 0 in TestResults corresponds to row 2 in the spreadsheet).
    /// </para><para>
    /// If the intent of a test is not clear from the input and output values listed in the spreadsheet row then you may want to consider adding a 'Comments' column
    /// to the spreadsheet and describe in there what it is that you're trying to assert.
    /// </para>
    /// </example>
    public class ExcelWorksheetData
    {
        #region Fields
        private TestContext _TestContext;
        #endregion

        #region Properties
        /// <summary>
        /// Gets a value from a worksheet by column name.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[string index]
        {
            get { return Convert.ToString(_TestContext.DataRow[index]); }
        }

        /// <summary>
        /// Gets a value from the worksheet by column number.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public string this[int ordinal]
        {
            get { return Convert.ToString(_TestContext.DataRow[ordinal]); }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="testContext"></param>
        /// <remarks>
        /// Use this to expose one row at a time in a normal data-driven test.
        /// </remarks>
        public ExcelWorksheetData(TestContext testContext)
        {
            _TestContext = testContext;
        }
        #endregion

        #region ColumnExists
        /// <summary>
        /// Returns true if there is a column with the name passed across.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool ColumnExists(string columnName)
        {
            return _TestContext.DataRow.Table.Columns.Contains(columnName);
        }
        #endregion

        #region GetColumn, GetNullableColumn
        /// <summary>
        /// Returns the content of the column cast to type or a default value if the column has no value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public T GetColumn<T>(string columnName, T emptyValue)
        {
            string rawValue = this[columnName];

            return System.String.IsNullOrEmpty(rawValue) ? emptyValue : (T)ConvertString(rawValue, typeof(T));
        }

        /// <summary>
        /// Returns the content of the column cast to type or a default value if the column has no value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public T GetColumn<T>(int ordinal, T emptyValue)
        {
            string rawValue = this[ordinal];

            return System.String.IsNullOrEmpty(rawValue) ? emptyValue : (T)ConvertString(rawValue, typeof(T));
        }

        /// <summary>
        /// Returns the content of the column cast to a nullable of the type or null if the column has no value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public Nullable<T> GetNullableColumn<T>(string columnName)
            where T: struct
        {
            string rawValue = this[columnName];

            return System.String.IsNullOrEmpty(rawValue) ? null : (Nullable<T>)ConvertString(rawValue, typeof(T));
        }

        /// <summary>
        /// Returns the content of the column cast to a nullable of the type or null if the column has no value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public Nullable<T> GetNullableColumn<T>(int ordinal)
            where T: struct
        {
            string rawValue = this[ordinal];

            return System.String.IsNullOrEmpty(rawValue) ? null : (Nullable<T>)ConvertString(rawValue, typeof(T));
        }

        /// <summary>
        /// Converts the raw value to a value of the type passed across.
        /// </summary>
        /// <param name="rawValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object ConvertString(string rawValue, Type type)
        {
            object result = null;

            if(type != typeof(DateTime)) {
                var hasHexStart = rawValue != null && rawValue.StartsWith("0x");
                if(hasHexStart && type == typeof(ushort))       result = Convert.ToUInt16(rawValue.Substring(2), 16);
                else if(hasHexStart && type == typeof(uint))    result = Convert.ToUInt32(rawValue.Substring(2), 16);
                else if(hasHexStart && type == typeof(ulong))   result = Convert.ToUInt64(rawValue.Substring(2), 16);
                else                                            result = Convert.ChangeType(rawValue, type, new CultureInfo("en-GB"));
            } else {
                string[] dateParts = rawValue.Split(new char[] {'/', ' ', ':', '.'}, StringSplitOptions.RemoveEmptyEntries);
                var parsed = new List<int>();
                var kind = DateTimeKind.Unspecified;
                for(int i = 0;i < dateParts.Length;++i) {
                    switch(dateParts[i].ToUpper()) {
                        case "L":   kind = DateTimeKind.Local; break;
                        case "Z":
                        case "U":   kind = DateTimeKind.Utc; break;
                        default:    parsed.Add(int.Parse(dateParts[i])); break;
                    }
                }
                switch(parsed.Count) {
                    case 3:     result = new DateTime(parsed[2], parsed[1], parsed[0]); break;
                    case 6:     result = new DateTime(parsed[2], parsed[1], parsed[0], parsed[3], parsed[4], parsed[5]); break;
                    case 7:     result = new DateTime(parsed[2], parsed[1], parsed[0], parsed[3], parsed[4], parsed[5], parsed[6]); break;
                    default:    throw new InvalidOperationException(string.Format("Cannot parse date string {0}", rawValue));
                }
                if(kind != DateTimeKind.Unspecified) result = System.DateTime.SpecifyKind((DateTime)result, kind);
            }

            return result;
        }
        #endregion

        #region Cell readers - Bool, Char, Int etc.
        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public T[] Array<T>(string columnName, char separator = ',')
        {
            List<T> result = new List<T>();
            var text = String(columnName) ?? "";
            foreach(var chunk in text.Split(separator)) {
                var valueText = chunk.Trim();
                result.Add((T)ConvertString(valueText, typeof(T)));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a bool.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool Bool(string columnName)                                 { return GetColumn<bool>(columnName, false); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a bool. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public bool Bool(string columnName, bool emptyValue)                { return GetColumn<bool>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a bool.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public bool Bool(int ordinal)                                       { return GetColumn<bool>(ordinal, false); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a bool. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public bool Bool(int ordinal, bool emptyValue)                      { return GetColumn<bool>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a char.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public char Char(string columnName)                                 { return GetColumn<char>(columnName, '\0'); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a char. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public char Char(string columnName, char emptyValue)                { return GetColumn<char>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a char.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public char Char(int ordinal)                                       { return GetColumn<char>(ordinal, '\0'); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a char. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public char Char(int ordinal, char emptyValue)                      { return GetColumn<char>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a byte.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public byte Byte(string columnName)                                 { return GetColumn<byte>(columnName, 0); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a byte. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public byte Byte(string columnName, byte emptyValue)                { return GetColumn<byte>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a byte.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public byte Byte(byte ordinal)                                      { return GetColumn<byte>(ordinal, 0); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a byte. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public byte Byte(byte ordinal, byte emptyValue)                     { return GetColumn<byte>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to an short.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public short Short(string columnName)                               { return GetColumn<short>(columnName, 0); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to an short. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public short Short(string columnName, short emptyValue)             { return GetColumn<short>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to an short.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public short Short(short ordinal)                                   { return GetColumn<short>(ordinal, 0); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to an short. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public short Short(short ordinal, short emptyValue)                 { return GetColumn<short>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a ushort.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public ushort UShort(string columnName)                             { return GetColumn<ushort>(columnName, 0); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a ushort. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public ushort UShort(string columnName, ushort emptyValue)          { return GetColumn<ushort>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a ushort.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public ushort UShort(int ordinal)                                   { return GetColumn<ushort>(ordinal, 0); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a ushort. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public ushort UShort(int ordinal, ushort emptyValue)                { return GetColumn<ushort>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to an int.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public int Int(string columnName)                                   { return GetColumn<int>(columnName, 0); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to an int. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public int Int(string columnName, int emptyValue)                   { return GetColumn<int>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to an int.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public int Int(int ordinal)                                         { return GetColumn<int>(ordinal, 0); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to an int. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public int Int(int ordinal, int emptyValue)                         { return GetColumn<int>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a uint.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public uint UInt(string columnName)                                 { return GetColumn<uint>(columnName, 0); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a uint. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public uint UInt(string columnName, uint emptyValue)                { return GetColumn<uint>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a uint.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public uint UInt(int ordinal)                                       { return GetColumn<uint>(ordinal, 0); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a uint. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public uint UInt(int ordinal, uint emptyValue)                      { return GetColumn<uint>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a long.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long Long(string columnName)                                 { return GetColumn<long>(columnName, 0); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a long. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public long Long(string columnName, long emptyValue)                { return GetColumn<long>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a long.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public long Long(int ordinal)                                       { return GetColumn<long>(ordinal, 0); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a long. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public long Long(int ordinal, long emptyValue)                      { return GetColumn<long>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a ulong.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public ulong ULong(string columnName)                               { return GetColumn<ulong>(columnName, 0); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a ulong. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public ulong ULong(string columnName, ulong emptyValue)             { return GetColumn<ulong>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a ulong.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public ulong ULong(int ordinal)                                     { return GetColumn<ulong>(ordinal, 0); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a ulong. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public ulong ULong(int ordinal, ulong emptyValue)                   { return GetColumn<ulong>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a decimal.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public decimal Decimal(string columnName)                           { return GetColumn<decimal>(columnName, 0M); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a decimal. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public decimal Decimal(string columnName, decimal emptyValue)       { return GetColumn<decimal>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a decimal.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public decimal Decimal(int ordinal)                                 { return GetColumn<decimal>(ordinal, 0M); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a decimal. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public decimal Decimal(int ordinal, decimal emptyValue)             { return GetColumn<decimal>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a double.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public double Double(string columnName)                             { return GetColumn<double>(columnName, 0F); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a double. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public double Double(string columnName, Double emptyValue)          { return GetColumn<double>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a double.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public double Double(int ordinal)                                   { return GetColumn<double>(ordinal, 0F); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a double. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public double Double(int ordinal, Double emptyValue)                { return GetColumn<double>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a float.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public float Float(string columnName)                               { return GetColumn<float>(columnName, 0F); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a float. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public float Float(string columnName, float emptyValue)             { return GetColumn<float>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a float.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public float Float(int ordinal)                                     { return GetColumn<float>(ordinal, 0F); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a float. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public float Float(int ordinal, float emptyValue)                   { return GetColumn<float>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a DateTime.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <remarks>
        /// The DateTime must be in UK format, i.e. 'dd/mm/yyyy' or 'dd/mm/yyyy hh:mm:ss' or 'dd/mm/yyyy hh:mm:ss.sss'.
        /// </remarks>
        public DateTime DateTime(string columnName)                         { return GetColumn<DateTime>(columnName, System.DateTime.MinValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a DateTime. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// The DateTime must be in UK format, i.e. 'dd/mm/yyyy' or 'dd/mm/yyyy hh:mm:ss' or 'dd/mm/yyyy hh:mm:ss.sss'.
        /// </remarks>
        public DateTime DateTime(string columnName, DateTime emptyValue)    { return GetColumn<DateTime>(columnName, emptyValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a DateTime.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <remarks>
        /// The DateTime must be in UK format, i.e. 'dd/mm/yyyy' or 'dd/mm/yyyy hh:mm:ss' or 'dd/mm/yyyy hh:mm:ss.sss'.
        /// </remarks>
        public DateTime DateTime(int ordinal)                               { return GetColumn<DateTime>(ordinal, System.DateTime.MinValue); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a DateTime. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// The DateTime must be in UK format, i.e. 'dd/mm/yyyy' or 'dd/mm/yyyy hh:mm:ss' or 'dd/mm/yyyy hh:mm:ss.sss'.
        /// </remarks>
        public DateTime DateTime(int ordinal, DateTime emptyValue)          { return GetColumn<DateTime>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the string content of the cell at <paramref name="columnName"/>.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <seealso cref="EString(string)"/>
        public String String(string columnName)                             { return GetColumn<String>(columnName, null); }

        /// <summary>
        /// Returns the string content of the cell at <paramref name="columnName"/>. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        /// <seealso cref="EString(string)"/>
        public String String(string columnName, String emptyValue)          { return GetColumn<String>(columnName, emptyValue); }

        /// <summary>
        /// Returns the string content of the <em>n</em>th cell.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <seealso cref="EString(int)"/>
        public String String(int ordinal)                                   { return GetColumn<String>(ordinal, null); }

        /// <summary>
        /// Returns the string content of the <em>n</em>th cell. If the cell is empty then 'emptyValue' is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        /// <see cref="EString(int)"/>
        public String String(int ordinal, String emptyValue)                { return GetColumn<String>(ordinal, emptyValue); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to an enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public T ParseEnum<T>(string columnName)                            { return (T)Enum.Parse(typeof(T), String(columnName)); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to an enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public T ParseEnum<T>(int ordinal)                                  { return (T)Enum.Parse(typeof(T), String(ordinal)); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> as a string. If the cell content is &quot;&quot; then String.Empty is returned. If the cell is empty then null is returned.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <seealso cref="String(string)"/>
        public string EString(string columnName)                            { return ParseEString(String(columnName)); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell as a string. If the cell content is &quot;&quot; then String.Empty is returned. If the cell is empty then null is returned.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <seealso cref="String(int)"/>
        public string EString(int ordinal)                                  { return ParseEString(String(ordinal)); }

        private string ParseEString(string text)
        {
            return text == @"""""" ? string.Empty : text;
        }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> as an array of bytes.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>The bytes must be formatted as a sequence of hex digits with any of these characters between each digit:</para>
        /// <list type="bullet">
        /// <item><description><em>space</em></description></item>
        /// <item><description><em>tab</em> (although this might be tricky to get into a spreadsheet cell!)</description></item>
        /// <item><description>comma</description></item>
        /// <item><description>semi-colon</description></item>
        /// <item><description>forward-slash</description></item>
        /// <item><description>dot</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// A cell whose content is null would return a null array. A cell whose content is two quotes (&quot;&quot;) returns an
        /// empty array. A cell whose content was '01 aa.BB/0c' would return a byte[] of length 4 whose contents were 0x01, 0xAA, 0xBB, 0x0C.
        /// </example>
        public byte[] Bytes(string columnName)                              { return ParseBytes(EString(columnName)); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell as an array of bytes.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>The bytes must be formatted as a sequence of hex digits with any of these characters between each digit:</para>
        /// <list type="bullet">
        /// <item><description>space</description></item>
        /// <item><description>tab (although this might be tricky to get into a spreadsheet cell!)</description></item>
        /// <item><description>comma</description></item>
        /// <item><description>semi-colon</description></item>
        /// <item><description>forward-slash</description></item>
        /// <item><description>dot</description></item>
        /// </list>
        /// </remarks>
        /// <example>
        /// A cell whose content is null would return a null array. A cell whose content is two quotes (&quot;&quot;) returns an
        /// empty array. A cell whose content was '01 aa.BB/0c' would return a byte[] of length 4 whose contents were 0x01, 0xAA, 0xBB, 0x0C.
        /// </example>
        public byte[] Bytes(int ordinal)                                    { return ParseBytes(EString(ordinal)); }

        private byte[] ParseBytes(string text)
        {
            byte[] result = null;

            if(text == "") result = new byte[]{};
            else if(text != null) {
                List<byte> bytes = new List<byte>();
                foreach(string byteText in text.Split(' ', ',', ';', '/', '\t', '.')) {
                    bytes.Add(Convert.ToByte(byteText, 16));
                }
                result = bytes.ToArray();
            }

            return result;
        }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable bool.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool? NBool(string columnName)                               { return GetNullableColumn<bool>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable bool.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public bool? NBool(int ordinal)                                     { return GetNullableColumn<bool>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable char.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public char? NChar(string columnName)                               { return GetNullableColumn<char>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable char.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public char? NChar(int ordinal)                                     { return GetNullableColumn<char>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable byte.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public byte? NByte(string columnName)                               { return GetNullableColumn<byte>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable byte.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public byte? NByte(byte ordinal)                                    { return GetNullableColumn<byte>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable short.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public short? NShort(string columnName)                             { return GetNullableColumn<short>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable short.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public short? NShort(short ordinal)                                 { return GetNullableColumn<short>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable int.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public int? NInt(string columnName)                                 { return GetNullableColumn<int>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable int.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public int? NInt(int ordinal)                                       { return GetNullableColumn<int>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable long.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public long? NLong(string columnName)                               { return GetNullableColumn<long>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable long.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public long? NLong(int ordinal)                                     { return GetNullableColumn<long>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable decimal.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public decimal? NDecimal(string columnName)                         { return GetNullableColumn<decimal>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable decimal.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public decimal? NDecimal(int ordinal)                               { return GetNullableColumn<decimal>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable double.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public double? NDouble(string columnName)                           { return GetNullableColumn<double>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable double.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public double? NDouble(int ordinal)                                 { return GetNullableColumn<double>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable float.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public float? NFloat(string columnName)                             { return GetNullableColumn<float>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable float.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public float? NFloat(int ordinal)                                   { return GetNullableColumn<float>(ordinal); }

        /// <summary>
        /// Returns the content of the cell at <paramref name="columnName"/> cast to a nullable DateTime.
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        /// <remarks>
        /// The DateTime must be in UK format, i.e. 'dd/mm/yyyy' or 'dd/mm/yyyy hh:mm:ss' or 'dd/mm/yyyy hh:mm:ss.sss'.
        /// </remarks>
        public DateTime? NDateTime(string columnName)                       { return GetNullableColumn<DateTime>(columnName); }

        /// <summary>
        /// Returns the content of the <em>n</em>th cell cast to a nullable DateTime.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <remarks>
        /// The DateTime must be in UK format, i.e. 'dd/mm/yyyy' or 'dd/mm/yyyy hh:mm:ss' or 'dd/mm/yyyy hh:mm:ss.sss'.
        /// </remarks>
        public DateTime? NDateTime(int ordinal)                             { return GetNullableColumn<DateTime>(ordinal); }
        #endregion
    }
}
