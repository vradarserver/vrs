// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Globalization;
using System.Text;
using ClosedXML.Excel;

namespace Test.Framework
{
    /// <summary>
    /// Represents a cell in a test data spreadsheet.
    /// </summary>
    public class SpreadsheetTestDataCell
    {
        public static readonly SpreadsheetTestDataCell Empty = new();

        public (string ColumnLetter, int RowNumber) CellReference { get; private set; }

        public string ColumnLetter => CellReference.ColumnLetter;

        public int ColumnNumber
        {
            get {
                var result = 0;
                var multiplier = 1;
                for(var idx = ColumnLetter.Length - 1;idx >= 0;--idx) {
                    result += ColumnLetter[idx] * multiplier;
                    multiplier *= 26;
                }
                return result;
            }
        }

        public int RowNumber => CellReference.RowNumber;

        public string Value { get; }

        internal SpreadsheetTestDataCell()
        {
            CellReference = new("", 0);
            Value = "";
        }

        internal SpreadsheetTestDataCell(IXLCell cell)
        {
            CellReference = new(cell.Address.ColumnLetter, cell.Address.ColumnNumber);
            Value = cell.Value.ToString() ?? "";
        }

        public override string ToString() => $"{CellReference.ColumnLetter}{CellReference.RowNumber}=\"{Value}\"";

        /// <summary>
        /// Converts <see cref="Value"/> to a value of the type passed across.
        /// </summary>
        /// <param name="rawValue"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object ConvertValue(Type type) => ConvertString(Value, type);

        /// <summary>
        /// Converts <see cref="Value"/> to a value of the type indicated by the
        /// type parameter. If <see cref="Value"/> is null or empty then <paramref name="emptyValue"/>
        /// is returned instead.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="emptyValue"></param>
        /// <returns></returns>
        public T ConvertValue<T>(T emptyValue) => Value == ""
            ? emptyValue
            : (T)ConvertValue(typeof(T));

        /// <summary>
        /// Converts a nullable struct value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? ConvertNullable<T>()
            where T: struct
        {
            return Value == ""
                ? null
                : (T)ConvertString(Value, typeof(T));
        }

        public static object ConvertString(string text, Type type)
        {
            object result = null;

            if(type != typeof(DateTime)) {
                var parsed = false;
                if(text.StartsWith("0x")) {
                    parsed = true;
                    if(type == typeof(ushort))      result = Convert.ToUInt16(text[2..], 16);
                    else if(type == typeof(uint))   result = Convert.ToUInt32(text[2..], 16);
                    else if(type == typeof(ulong))  result = Convert.ToUInt64(text[2..], 16);
                    else                            parsed = false;
                }
                if(!parsed) {
                    result = Convert.ChangeType(text, type, new CultureInfo("en-GB"));
                }
            } else {
                var dateParts = text.Split(new char[] {'/', ' ', ':', '.'}, StringSplitOptions.RemoveEmptyEntries);
                var parsed = new List<int>();
                var kind = DateTimeKind.Unspecified;
                foreach(var datePart in dateParts) {
                    switch(datePart.ToUpperInvariant()) {
                        case "L":   kind = DateTimeKind.Local; break;
                        case "Z":
                        case "U":   kind = DateTimeKind.Utc; break;
                        default:    parsed.Add(int.Parse(datePart)); break;
                    }
                }
                switch(parsed.Count) {
                    case 3:     result = new DateTime(parsed[2], parsed[1], parsed[0]); break;
                    case 6:     result = new DateTime(parsed[2], parsed[1], parsed[0], parsed[3], parsed[4], parsed[5]); break;
                    case 7:     result = new DateTime(parsed[2], parsed[1], parsed[0], parsed[3], parsed[4], parsed[5], parsed[6]); break;
                    default:    throw new InvalidOperationException($"Cannot parse date string {text}");
                }
                if(kind != DateTimeKind.Unspecified) {
                    result = System.DateTime.SpecifyKind((DateTime)result, kind);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns <see cref="Value"/> parsed into an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="separator"></param>
        /// <returns></returns>
        public T[] Array<T>(string separator = ",")
        {
            var result = new List<T>();
            foreach(var chunk in Value.Split(separator)) {
                result.Add((T)ConvertString(chunk.Trim(), typeof(T)));
            }

            return result.ToArray();
        }

        public bool Bool() => ConvertValue<bool>(false);

        public bool Bool(bool emptyValue) => ConvertValue<bool>(emptyValue);

        public char Char() => ConvertValue<char>('\0');

        public char Char(char emptyValue) => ConvertValue<char>(emptyValue);

        public byte Byte() => ConvertValue<byte>(0);

        public byte Byte(byte emptyValue) => ConvertValue<byte>(emptyValue);

        public byte[] Bytes()
        {
            var result = System.Array.Empty<byte>();

            if(Value != "") {
                result = Value
                    .Split(' ', ',', ';', '/', '\t', '.')
                    .Select(r => Convert.ToByte(r, 16))
                    .ToArray();
            }

            return result;
        }

        public DateTime DateTime() => ConvertValue<DateTime>(default);

        public DateTime DateTime(DateTime emptyValue) => ConvertValue<DateTime>(emptyValue);

        public decimal Decimal() => ConvertValue<decimal>(0);

        public decimal Decimal(decimal emptyValue) => ConvertValue<decimal>(emptyValue);

        public double Double() => ConvertValue<double>(0);

        public double Double(double emptyValue) => ConvertValue<double>(emptyValue);

        public T Enum<T>() where T:struct => Enum<T>(default);

        public T Enum<T>(T emptyValue) where T:struct => Value.Trim() == "" ? emptyValue : System.Enum.Parse<T>(Value);

        public string EString() => Value == "\"\"" ? "" : String();

        public float Float() => ConvertValue<float>(0);

        public float Float(float emptyValue) => ConvertValue<float>(emptyValue);

        public int Int() => ConvertValue<int>(0);

        public int Int(int emptyValue) => ConvertValue<int>(emptyValue);

        public long Long() => ConvertValue<long>(0);

        public long Long(long emptyValue) => ConvertValue<long>(emptyValue);

        public bool? NBool() => ConvertNullable<bool>();

        public byte? NByte() => ConvertNullable<byte>();

        public char? NChar() => ConvertNullable<char>();

        public DateTime? NDateTime() => ConvertNullable<DateTime>();

        public decimal? NDecimal() => ConvertNullable<decimal>();

        public double? NDouble() => ConvertNullable<double>();

        public float? NFloat() => ConvertNullable<float>();

        public int? NInt() => ConvertNullable<int>();

        public long? NLong() => ConvertNullable<long>();

        public short? NShort() => ConvertNullable<short>();

        public uint? NUInt() => ConvertNullable<uint>();

        public ulong? NULong() => ConvertNullable<ulong>();

        public ushort? NUShort() => ConvertNullable<ushort>();

        public short Short() => ConvertValue<short>(0);

        public short Short(short emptyValue) => ConvertValue<short>(emptyValue);

        public string String() => ConvertValue<string>(null);

        public string String(string emptyValue) => ConvertValue<string>(emptyValue);

        public uint UInt() => ConvertValue<uint>(0);

        public uint UInt(uint emptyValue) => ConvertValue<uint>(emptyValue);

        public ulong ULong() => ConvertValue<ulong>(0);

        public ulong ULong(ulong emptyValue) => ConvertValue<ulong>(emptyValue);

        public ushort UShort() => ConvertValue<ushort>(0);

        public ushort UShort(ushort emptyValue) => ConvertValue<ushort>(emptyValue);
    }
}
