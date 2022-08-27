// Copyright © 2022 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that can parse CSV files into string arrays.
    /// </summary>
    public class CsvParser
    {
        class State
        {
            public StringBuilder CurrentChunk = new StringBuilder();
            public bool IsInString;
            public bool IsEscaped;

            public void Reset()
            {
                CurrentChunk.Clear();
                IsInString = false;
                IsEscaped = false;
            }
        }

        public char ChunkSeparator { get; }

        public char StringDelimiter { get; }

        public char EscapeCharacter { get; }

        public bool OnlyAllowEscapesInStrings { get; }

        public bool EscapeWithDoubleStringDelimiter { get; }

        public bool PreserveEscapeIfNotEscapingString { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="chunkSepartor"></param>
        /// <param name="stringDelimiter"></param>
        /// <param name="escapeCharacter"></param>
        /// <param name="onlyAllowEscapesInStrings"></param>
        /// <param name="escapeWithDoubleStringDelimiter"></param>
        public CsvParser(
            char chunkSepartor =                        ',',
            char stringDelimiter =                      '"',
            char escapeCharacter =                      '\\',
            bool onlyAllowEscapesInStrings =            false,
            bool escapeWithDoubleStringDelimiter =      true,
            bool preserveEscapeIfNotEscapingString =    true
        )
        {
            ChunkSeparator =                    chunkSepartor;
            StringDelimiter =                   stringDelimiter;
            EscapeCharacter =                   escapeCharacter;
            OnlyAllowEscapesInStrings =         onlyAllowEscapesInStrings;
            EscapeWithDoubleStringDelimiter =   escapeWithDoubleStringDelimiter;
            PreserveEscapeIfNotEscapingString = preserveEscapeIfNotEscapingString;
        }

        /// <summary>
        /// Parses a line in a CSV file into chunks. Ignores commas within quoted strings.
        /// Cannot deal with newlines in chunks.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public IList<string> ParseLineToChunks(string line)
        {
            var result = new List<string>();

            if(!String.IsNullOrEmpty(line)) {
                var state = new State();
                for(var charIdx = 0;charIdx < line.Length;++charIdx) {
                    var ch = line[charIdx];
                    if(state.IsEscaped) {
                        if(ch != StringDelimiter && PreserveEscapeIfNotEscapingString) {
                            state.CurrentChunk.Append(line[charIdx - 1]);
                        }
                        state.CurrentChunk.Append(ch);
                        state.IsEscaped = false;
                    } else {
                        if(ch == StringDelimiter) {
                            if(IsStringEscape(line, charIdx, state)) {
                                state.IsEscaped = true;
                            } else {
                                state.IsInString = !state.IsInString;
                            }
                        } else if(ch == EscapeCharacter && ch != '\0' && (!OnlyAllowEscapesInStrings || state.IsInString)) {
                            state.IsEscaped = true;
                        } else if(!state.IsInString && ch == ChunkSeparator) {
                            result.Add(state.CurrentChunk.ToString());
                            state.Reset();
                        } else {
                            state.CurrentChunk.Append(ch);
                        }
                    }
                }
                result.Add(state.CurrentChunk.ToString());
            }

            return result;
        }

        /// <summary>
        /// Assumes that the current character is a string delimiter, tests to see
        /// whether the following character is also a string delimiter and whether
        /// it counts as an escape character.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="charIdx"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool IsStringEscape(string line, int charIdx, State state)
        {
            return EscapeWithDoubleStringDelimiter
                && charIdx + 1 < line.Length
                && state.IsInString
                && line[charIdx + 1] == StringDelimiter;
        }

        public IDictionary<string, int> ExtractOrdinals(IList<string> chunks, bool caseInsensitive = false, bool expectDuplicateNames = false)
        {
            var result = new Dictionary<string, int>(caseInsensitive
                ? StringComparer.InvariantCultureIgnoreCase
                : StringComparer.InvariantCulture
            );

            for(var chunkIdx = 0;chunkIdx < chunks.Count;++chunkIdx) {
                var name = chunks[chunkIdx];
                if(!String.IsNullOrEmpty(name)) {
                    var nameExists = result.ContainsKey(name);
                    if(nameExists) {
                        if(!expectDuplicateNames) {
                            throw new InvalidOperationException(String.Format("There are two columns named {0}", name));
                        }
                        var newName = name;
                        for(var dupNum = 2;dupNum < 1000 && nameExists;++dupNum) {
                            newName = String.Format("{0}({1})", name, dupNum);
                            nameExists = result.ContainsKey(newName);
                        }
                        if(nameExists) {
                            throw new InvalidOperationException(String.Format("There are too many columns named {0}, could not generate a unique name for it", name));
                        }
                        name = newName;
                    }

                    result.Add(name, chunkIdx);
                }
            }

            return result;
        }
    }
}
