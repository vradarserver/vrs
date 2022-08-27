// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class CsvParserTests
    {
        [TestMethod]
        public void ParseLineToChunks_Can_Parse_Simple_CSV()
        {
            var parser = new CsvParser();

            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { Line = (string)null,      Expected = new string[0] },
                new { Line = "",                Expected = new string[0] },
                new { Line = "A",               Expected = new string[] { "A", } },
                new { Line = "A,B",             Expected = new string[] { "A", "B", } },
                new { Line = "\"A,B\"",         Expected = new string[] { "A,B", } },
                new { Line = "\"A,\"\"B\"\"\"", Expected = new string[] { "A,\"B\"", } },
            },
            (row) => {
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }

        [TestMethod]
        public void ParseLineToChunks_Can_Use_Alternate_Chunk_Separators()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { ChunkSeparator = ',', Line = "A;B", Expected = new string[] { "A;B", } },
                new { ChunkSeparator = ';', Line = "A;B", Expected = new string[] { "A", "B", } },
            },
            (row) => {
                var parser = new CsvParser(
                    chunkSepartor:              row.ChunkSeparator,
                    stringDelimiter:            '"',
                    escapeCharacter:            '\\',
                    onlyAllowEscapesInStrings:  false
                );
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }

        [TestMethod]
        public void ParseLineToChunks_Can_Use_Alternate_String_Delimiter()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { StringDelimiter = '"',  Line = "'A,B'", Expected = new string[] { "'A", "B'", } },
                new { StringDelimiter = '\'', Line = "'A,B'", Expected = new string[] { "A,B", } },
            },
            (row) => {
                var parser = new CsvParser(
                    stringDelimiter: row.StringDelimiter
                );;
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }

        [TestMethod]
        public void ParseLineToChunks_Can_Use_Alternate_Escape_Characters()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { EscapeCharacter = '\\', Line = "A\\,B;,C", Expected = new string[] { "A,B;", "C" } },
                new { EscapeCharacter = ';',  Line = "A\\,B;,C", Expected = new string[] { "A\\", "B,C" } },
            },
            (row) => {
                var parser = new CsvParser(
                    escapeCharacter:                    row.EscapeCharacter,
                    preserveEscapeIfNotEscapingString:  false
                );
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }

        [TestMethod]
        public void ParseLineToChunks_Can_Suppress_Concept_Of_Escape_Characters()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { EscapeCharacter = '\\', Line = "A\\,B\0,C", Expected = new string[] { "A,B\0", "C" } },
                new { EscapeCharacter = '\0', Line = "A\\,B\0,C", Expected = new string[] { "A\\", "B\0", "C" } },
            },
            (row) => {
                var parser = new CsvParser(
                    escapeCharacter:                    row.EscapeCharacter,
                    preserveEscapeIfNotEscapingString:  false
                );
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }

        [TestMethod]
        public void ParseLineToChunks_Can_Control_Whether_Escape_Characters_Only_Work_In_Strings()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { OnlyAllowEscapesInStrings = false, Line = "A\\,B,\"C\\\"D\"", Expected = new string[] { "A,B", "C\"D" } },
                new { OnlyAllowEscapesInStrings = true,  Line = "A\\,B,\"C\\\"D\"", Expected = new string[] { "A\\", "B", "C\"D" } },
            },
            (row) => {
                var parser = new CsvParser(
                    onlyAllowEscapesInStrings:          row.OnlyAllowEscapesInStrings,
                    preserveEscapeIfNotEscapingString:  false
                );
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }

        [TestMethod]
        public void ParseLineToChunks_Can_Escape_Quotes_In_Strings_By_Doubling_Them_Up()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { EscapeWithDoubleStringDelimiter = false, Line = "\"A\"\"B\"", Expected = new string[] { "AB", } },
                new { EscapeWithDoubleStringDelimiter = true,  Line = "\"A\"\"B\"", Expected = new string[] { "A\"B", } },
            },
            (row) => {
                var parser = new CsvParser(
                    escapeWithDoubleStringDelimiter: row.EscapeWithDoubleStringDelimiter
                );
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }

        [TestMethod]
        public void ParseLineToChunks_Can_Preserve_Escapes_When_They_Do_Not_Escape_Strings()
        {
            new InlineDataTest(this)
            .TestAndAssert(new dynamic[] {
                new { PreserveEscapeIfNotEscapingString = false, Line = "A\\r\\nB,\"C\\\"D\"", Expected = new string[] { "ArnB", "C\"D", } },
                new { PreserveEscapeIfNotEscapingString = true,  Line = "A\\r\\nB,\"C\\\"D\"", Expected = new string[] { "A\\r\\nB", "C\"D", } },
            },
            (row) => {
                var parser = new CsvParser(
                    preserveEscapeIfNotEscapingString: row.PreserveEscapeIfNotEscapingString
                );
                var expected = (IEnumerable<string>)row.Expected;
                var actual = (IList<string>)parser.ParseLineToChunks(row.Line);
                Assert.IsTrue(expected.SequenceEqual(actual));
            });
        }
    }
}
