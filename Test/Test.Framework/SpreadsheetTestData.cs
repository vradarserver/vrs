// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using ClosedXML.Excel;

namespace Test.Framework
{
    public class SpreadsheetTestData
    {
        public string WorksheetName { get; }

        public bool HasHeadingRow { get; }

        private Dictionary<string, string> _HeadingColumnLetters = new(StringComparer.InvariantCultureIgnoreCase);
        public IReadOnlyDictionary<string, string> HeadingColumnLetters => _HeadingColumnLetters;

        public IReadOnlyList<SpreadsheetTestDataRow> Rows { get; private set; }

        public SpreadsheetTestData(byte[] spreadsheetData, string worksheetName, bool hasHeadingRow = true)
        {
            WorksheetName = worksheetName;
            HasHeadingRow = hasHeadingRow;
            LoadDocumentFromByteArray(spreadsheetData);
        }

        private void LoadDocumentFromByteArray(byte[] spreadsheetData)
        {
            using(var documentStream = new MemoryStream(spreadsheetData, writable: false)) {
                using(var workbook = new XLWorkbook(documentStream)) {
                    var sheet = workbook
                        .Worksheets
                        .FirstOrDefault(s => String.Equals(s.Name, WorksheetName));
                    if(sheet == null) {
                        throw new InvalidOperationException($"Cannot find a worksheet called {WorksheetName}");
                    }

                    Rows = sheet
                        .Rows()
                        .OrderBy(r => r.RowNumber())
                        .Select(r => new SpreadsheetTestDataRow(this, r))
                        .ToArray()
                        ?? Array.Empty<SpreadsheetTestDataRow>();
                }
            }

            ReadHeadings();
        }

        private void ReadHeadings()
        {
            _HeadingColumnLetters.Clear();
            if(HasHeadingRow) {
                var row = Rows.FirstOrDefault();
                if(row != null) {
                    foreach(var cell in row.Cells) {
                        var heading = cell.Value.Trim();
                        if(heading != "" && !_HeadingColumnLetters.ContainsKey(heading)) {
                            _HeadingColumnLetters.Add(heading, cell.ColumnLetter);
                        }
                    }
                }
            }
        }

        public void TestEveryRow(object testClass, Action<SpreadsheetTestDataRow> testMethod)
        {
            var inlineDataTest = new InlineDataTest(testClass);
            var rows = Rows.Skip(HasHeadingRow ? 1 : 0);

            Assert.IsTrue(rows.Any(), "Spreadsheet contains no rows, that doesn't seem right");

            inlineDataTest.TestAndAssert<SpreadsheetTestDataRow>(rows,testMethod);
        }
    }
}
