// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using ClosedXML.Excel;

namespace Test.Framework
{
    /// <summary>
    /// Represents a row in a test data spreadsheet.
    /// </summary>
    public class SpreadsheetTestDataRow
    {
        public SpreadsheetTestData Spreadsheet { get; }

        public int RowNumber { get; }

        public IReadOnlyList<SpreadsheetTestDataCell> Cells { get; }

        public SpreadsheetTestDataCell this[string columnHeaderName] => CellForColumnHeader(columnHeaderName);

        internal SpreadsheetTestDataRow(SpreadsheetTestData spreadsheet, IXLRow row)
        {
            Spreadsheet = spreadsheet;
            RowNumber = row.RowNumber();

            Cells = row.Cells()
                .Select(cell => new SpreadsheetTestDataCell(cell))
                .OrderBy(cell => cell.ColumnNumber)
                .ToArray();
        }

        public override string ToString() => $"{RowNumber}: {System.String.Join(", ", Cells.OrderBy(r => r.ColumnNumber).Select(cell => cell.ToString()))}";

        public SpreadsheetTestDataCell CellForColumnHeader(string headerName, bool returnEmptyIfMissing = true)
        {
            SpreadsheetTestDataCell result = null;

            if(Spreadsheet.HeadingColumnLetters.TryGetValue(headerName, out var columnLetter)) {
                result = CellForColumnLetter(columnLetter, returnEmptyIfMissing);
            }
            if(result == null && returnEmptyIfMissing) {
                result = SpreadsheetTestDataCell.Empty;
            }

            return result;
        }

        public SpreadsheetTestDataCell CellForColumnLetter(string columnLetter, bool returnEmptyIfMissing = true)
        {
            var result = Cells.FirstOrDefault(cell => cell.ColumnLetter == columnLetter);
            if(result == null && returnEmptyIfMissing) {
                result = SpreadsheetTestDataCell.Empty;
            }
            return result;
        }

        public SpreadsheetTestDataCell CellForColumnNumber(int columnNumber, bool returnEmptyIfMissing = true)
        {
            var result = Cells.FirstOrDefault(cell => cell.ColumnNumber == columnNumber);
            if(result == null && returnEmptyIfMissing) {
                result = SpreadsheetTestDataCell.Empty;
            }
            return result;
        }

        public SpreadsheetTestDataCell CellForColumnOrdinal(int ordinal, bool returnEmptyIfMissing = true) => CellForColumnNumber(ordinal + 1, returnEmptyIfMissing);

        public bool Bool(string headerName, bool emptyValue = default) => CellForColumnHeader(headerName).Bool(emptyValue);

        public bool Bool(int ordinal, bool emptyValue = default) => CellForColumnOrdinal(ordinal).Bool(emptyValue);

        public byte Byte(string headerName, byte emptyValue = default) => CellForColumnHeader(headerName).Byte(emptyValue);

        public byte Byte(int ordinal, byte emptyValue = default) => CellForColumnOrdinal(ordinal).Byte(emptyValue);

        public char Char(string headerName, char emptyValue = default) => CellForColumnHeader(headerName).Char(emptyValue);

        public char Char(int ordinal, char emptyValue = default) => CellForColumnOrdinal(ordinal).Char(emptyValue);

        public byte[] Bytes(string headerName) => CellForColumnHeader(headerName).Bytes();

        public byte[] Bytes(int ordinal) => CellForColumnOrdinal(ordinal).Bytes();

        public DateTime DateTime(string headerName, DateTime emptyValue = default) => CellForColumnHeader(headerName).DateTime(emptyValue);

        public DateTime DateTime(int ordinal, DateTime emptyValue = default) => CellForColumnOrdinal(ordinal).DateTime(emptyValue);

        public decimal Decimal(string headerName, decimal emptyValue = default) => CellForColumnHeader(headerName).Decimal(emptyValue);

        public decimal Decimal(int ordinal, decimal emptyValue = default) => CellForColumnOrdinal(ordinal).Decimal(emptyValue);

        public double Double(string headerName, double emptyValue = default) => CellForColumnHeader(headerName).Double(emptyValue);

        public double Double(int ordinal, double emptyValue = default) => CellForColumnOrdinal(ordinal).Double(emptyValue);

        public T Enum<T>(string headerName, T emptyValue = default) where T:struct => CellForColumnHeader(headerName).Enum<T>(default);

        public T Enum<T>(int ordinal, T emptyValue = default) where T:struct => CellForColumnOrdinal(ordinal).Enum<T>(default);

        public string EString(string headerName) => CellForColumnHeader(headerName).EString();

        public string EString(int ordinal) => CellForColumnOrdinal(ordinal).EString();

        public float Float(string headerName, float emptyValue = default) => CellForColumnHeader(headerName).Float(emptyValue);

        public float Float(int ordinal, float emptyValue = default) => CellForColumnOrdinal(ordinal).Float(emptyValue);

        public int Int(string headerName, int emptyValue = default) => CellForColumnHeader(headerName).Int(emptyValue);

        public int Int(int ordinal, int emptyValue = default) => CellForColumnOrdinal(ordinal).Int(emptyValue);

        public long Long(string headerName, long emptyValue = default) => CellForColumnHeader(headerName).Long(emptyValue);

        public long Long(int ordinal, long emptyValue = default) => CellForColumnOrdinal(ordinal).Long(emptyValue);

        public bool? NBool(string headerName) => CellForColumnHeader(headerName).NBool();

        public bool? NBool(int ordinal) => CellForColumnOrdinal(ordinal).NBool();

        public byte? NByte(string headerName) => CellForColumnHeader(headerName).NByte();

        public byte? NByte(int ordinal) => CellForColumnOrdinal(ordinal).NByte();

        public char? NChar(string headerName) => CellForColumnHeader(headerName).NChar();

        public char? NChar(int ordinal) => CellForColumnOrdinal(ordinal).NChar();

        public DateTime? NDateTime(string headerName) => CellForColumnHeader(headerName).NDateTime();

        public DateTime? NDateTime(int ordinal) => CellForColumnOrdinal(ordinal).NDateTime();

        public decimal? NDecimal(string headerName) => CellForColumnHeader(headerName).NDecimal();

        public decimal? NDecimal(int ordinal) => CellForColumnOrdinal(ordinal).NDecimal();

        public double? NDouble(string headerName) => CellForColumnHeader(headerName).NDouble();

        public double? NDouble(int ordinal) => CellForColumnOrdinal(ordinal).NDouble();

        public float? NFloat(string headerName) => CellForColumnHeader(headerName).NFloat();

        public float? NFloat(int ordinal) => CellForColumnOrdinal(ordinal).NFloat();

        public int? NInt(string headerName) => CellForColumnHeader(headerName).NInt();

        public int? NInt(int ordinal) => CellForColumnOrdinal(ordinal).NInt();

        public long? NLong(string headerName) => CellForColumnHeader(headerName).NLong();

        public long? NLong(int ordinal) => CellForColumnOrdinal(ordinal).NLong();

        public short? NShort(string headerName) => CellForColumnHeader(headerName).NShort();

        public short? NShort(int ordinal) => CellForColumnOrdinal(ordinal).NShort();

        public uint? NUInt(string headerName) => CellForColumnHeader(headerName).NUInt();

        public uint? NUInt(int ordinal) => CellForColumnOrdinal(ordinal).NUInt();

        public ulong? NULong(string headerName) => CellForColumnHeader(headerName).NULong();

        public ulong? NULong(int ordinal) => CellForColumnOrdinal(ordinal).NULong();

        public ushort? NUShort(string headerName) => CellForColumnHeader(headerName).NUShort();

        public ushort? NUShort(int ordinal) => CellForColumnOrdinal(ordinal).NUShort();

        public short Short(string headerName) => CellForColumnHeader(headerName).Short();

        public short Short(int ordinal) => CellForColumnOrdinal(ordinal).Short();

        public string String(string headerName) => CellForColumnHeader(headerName).String();

        public string String(int ordinal) => CellForColumnOrdinal(ordinal).String();

        public uint? UInt(string headerName) => CellForColumnHeader(headerName).UInt();

        public uint? UInt(int ordinal) => CellForColumnOrdinal(ordinal).UInt();

        public ulong? ULong(string headerName) => CellForColumnHeader(headerName).ULong();

        public ulong? ULong(int ordinal) => CellForColumnOrdinal(ordinal).ULong();

        public ushort? UShort(string headerName) => CellForColumnHeader(headerName).UShort();

        public ushort? UShort(int ordinal) => CellForColumnOrdinal(ordinal).UShort();
    }
}
