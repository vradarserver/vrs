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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test.Framework
{
    /// <summary>
    /// Helps to look up the value associated with a field within a spreadsheet.
    /// </summary>
    /// <remarks>
    /// For some data-driven tests it can be useful to have a number of columns in the spreadsheet named 'Field1', 'Field2' ... 'Field<em>n</em>'
    /// which contain the name of a field and an associated set of columns 'Value1', 'Value2' ... 'Value<em>n</em>' which contain the expected
    /// value associated with the field. This class wraps up the code necessary to find an associated value for a given field name.
    /// </remarks>
    public class SpreadsheetFieldValue
    {
        /// <summary>
        /// Gets or sets the worksheet that field names and values will be extracted from.
        /// </summary>
        public ExcelWorksheetData Worksheet { get; set; }

        /// <summary>
        /// Gets or sets the prefix for field name columns - defaults to 'Field'.
        /// </summary>
        /// <remarks>
        /// The class searches across all columns whose name starts with this prefix for field names.
        /// </remarks>
        public string FieldPrefix { get; set; }

        /// <summary>
        /// Gets or sets the prefix for value columns - defaults to 'Value'.
        /// </summary>
        /// <remarks>
        /// The class searches across all columns whose name starts with this prefix for values associated with a field.
        /// </remarks>
        public string ValuePrefix { get; set; }

        /// <summary>
        /// Gets or sets the number of columns to search across for field names and associated values.
        /// </summary>
        /// <remarks>
        /// The code searches for field names in columns whose name starts with <see cref="FieldPrefix"/> followed by a digit.
        /// The digit starts at 1 and increments up to the MaxFields value, inclusive. So if MaxFields is 3, <see cref="FieldPrefix"/> is 'Field'
        /// and <see cref="ValuePrefix"/> is 'Value' then the columns Field1, Field2 and Field3 are searched for field names and the
        /// associated values are expected to be found in one of Value1, Value2 or Value3.
        /// </remarks>
        public int MaxFields { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="maxFields"></param>
        /// <param name="fieldPrefix"></param>
        /// <param name="valuePrefix"></param>
        public SpreadsheetFieldValue(ExcelWorksheetData worksheet, int maxFields, string fieldPrefix = "Field", string valuePrefix = "Value")
        {
            if(maxFields < 1) throw new ArgumentOutOfRangeException("maxFields");
            Worksheet = worksheet;
            MaxFields = maxFields;
            FieldPrefix = fieldPrefix;
            ValuePrefix = valuePrefix;
        }

        /// <summary>
        /// Returns the name of the column that holds the value for a field name.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        private string FindValueColumn(string fieldName)
        {
            string result = null;

            for(int fieldNumber = 1;fieldNumber <= MaxFields;++fieldNumber) {
                string nameColumn = String.Format("{0}{1}", FieldPrefix, fieldNumber);
                if(Worksheet.String(nameColumn) == fieldName) {
                    result = String.Format("{0}{1}", ValuePrefix, fieldNumber);
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public bool? GetNBool(string fieldName)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? (bool?)null : Worksheet.NBool(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="isHex">True if the cell contains a hex value rather than decimal.</param>
        /// <returns></returns>
        public byte? GetNByte(string fieldName, bool isHex = false)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? (byte?)null : isHex ? Convert.ToByte(Worksheet.String(valueColumn), 16) : Worksheet.NByte(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="isHex">True if the cell contains a hex value rather than decimal.</param>
        /// <returns></returns>
        public short? GetNShort(string fieldName, bool isHex = false)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? (short?)null : isHex ? Convert.ToInt16(Worksheet.String(valueColumn), 16) : Worksheet.NShort(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="isHex">True if the cell contains a hex value rather than decimal.</param>
        /// <returns></returns>
        public int? GetNInt(string fieldName, bool isHex = false)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? (int?)null : isHex ? Convert.ToInt32(Worksheet.String(valueColumn), 16) : Worksheet.NInt(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="isHex">True if the cell contains a hex value rather than decimal.</param>
        /// <returns></returns>
        public long? GetNLong(string fieldName, bool isHex = false)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? (long?)null : isHex ? Convert.ToInt64(Worksheet.String(valueColumn), 16) : Worksheet.NLong(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public float? GetNFloat(string fieldName)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? (float?)null : Worksheet.NFloat(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public double? GetNDouble(string fieldName)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? (double?)null : Worksheet.NDouble(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetString(string fieldName)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? null : Worksheet.String(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string GetEString(string fieldName)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? null : Worksheet.EString(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public byte[] GetBytes(string fieldName)
        {
            var valueColumn = FindValueColumn(fieldName);
            return valueColumn == null ? null : Worksheet.Bytes(valueColumn);
        }

        /// <summary>
        /// Returns the value for the field passed across or null if the field could not be found.
        /// </summary>
        /// <typeparam name="T">The type of the enumeration to cast the result to. Passing a non-enum type here will cause an exception to be thrown when the value cell has non-null content.</typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public Nullable<T> GetEnum<T>(string fieldName)
            where T: struct
        {
            Nullable<T> value = default(Nullable<T>);

            var valueColumn = FindValueColumn(fieldName);
            if(valueColumn != null) {
                var text = Worksheet.String(valueColumn);
                if(text != null) value = (T)Enum.Parse(typeof(T), text);
            }

            return value;
        }
    }
}
