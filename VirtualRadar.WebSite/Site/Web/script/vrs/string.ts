/**
 * @license Copyright © 2013 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 * Portions Copyright (c) 2009, CodePlex Foundation
 * All rights reserved.
 *
 * CodePlex license:
 * Redistribution and use in source and binary forms, with or without modification, are permitted
 * provided that the following conditions are met:
 *
 * *   Redistributions of source code must retain the above copyright notice, this list of conditions
 * and the following disclaimer.
 *
 * *   Redistributions in binary form must reproduce the above copyright notice, this list of conditions
 * and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *
 * *   Neither the name of CodePlex Foundation nor the names of its contributors may be used to endorse or
 * promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS AS IS AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
 * OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN
 * IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview String utility methods.
 */

namespace VRS
{
    /**
     * Methods for working with strings.
     *
     * The format() code was copied from http://stackoverflow.com/questions/2534803/string-format-in-javascript
     * which in turn was extracted from MicrosoftAjax.js by Sky Sanders on 28th March 2010.
     */
    export class StringUtility
    {
        /**
         * Returns true if the text contains the hasText string.
         */
        contains(text: string, hasText: string, ignoreCase: boolean = false) : boolean
        {
            var result = !!(text && hasText);
            if(result) {
                if(!ignoreCase) result = text.indexOf(hasText) !== -1;
                else            result = text.toUpperCase().indexOf(hasText.toUpperCase()) !== -1;
            }
            return result;
        }

        /**
         * Returns true if the lhs is equals to the rhs.
         */
        equals(lhs: string, rhs: string, ignoreCase: boolean = false) : boolean
        {
            var result = false;
            if(!lhs) result = !rhs;
            else result = rhs && (!ignoreCase ? lhs === rhs : lhs.toUpperCase() === rhs.toUpperCase());
            return result;
        }

        /**
         * Calls the allowCharacter callback for each character in the text and returns the string formed from those
         * characters that the callback returns true for.
         */
        filter(text: string, allowCharacter: (ch: string) => boolean) : string
        {
            return this.filterReplace(text, function(ch) {
                return allowCharacter(ch) ? ch : null;
            });
        }

        /**
         * Calls the replaceCharacter callback for each character in the text and returns the string formed from the
         * strings that the callback returns.
         */
        filterReplace(text: string, replaceCharacter: (ch: string) => string) : string
        {
            let result = text;
            if(result) {
                result = '';
                let length = text.length;
                let start = 0;
                let useChunk = function(end) {
                    result += text.substring(start, end);
                    start = end + 1;
                };
                for(let i = 0;i < length;++i) {
                    let ch = text[i];
                    let replaceWith = replaceCharacter(ch);
                    if(ch !== replaceWith) {
                        useChunk(i);
                        if(replaceWith) result += replaceWith;
                    }
                }
                useChunk(length);
            }

            return result;
        }

        /**
         * Returns the text with chevrons and ampersands escaped out for display in HTML.
         *
         * The aim of this function is to make the text HTML-safe in one pass - it is not to convert
         * every possible HTML character to escape codes.
         */
        htmlEscape(text: string, newlineToLineBreak: boolean = false) : string
        {
            return this.filterReplace(text, function(ch) {
                switch(ch) {
                    case '&':   return '&amp;';
                    case '<':   return '&lt;';
                    case '>':   return '&gt;';
                    case '\n':  return newlineToLineBreak ? '<br />' : ch;
                    default:    return ch;
                }
            });
        }

        /**
         * Formats a .NET style format string and arguments.
         * @param {string} text The formatting string.
         * @param {...} args The arguments to the formatting string.
         * @returns {string} The formatted string.
         */
        format(text: string, ...args: any[]) : string
        {
            return this.toFormattedString(false, arguments);
        }

        /**
         * Replaces .NET style substitution markers with the arguments passed across.
         * This was copied from http://stackoverflow.com/questions/2534803/string-format-in-javascript
         * which in turn was extracted from MicrosoftAjax.js.
         */
        private toFormattedString(useLocale: boolean, args: IArguments) : string
        {
            var result = '';
            var format = <string>args[0];

            for(var i = 0;;) {
                // Find the next opening or closing brace
                var open = format.indexOf('{', i);
                var close = format.indexOf('}', i);
                if((open < 0) && (close < 0)) {
                    // Not found: copy the end of the string and break
                    result += format.slice(i);
                    break;
                }
                if((close > 0) && ((close < open) || (open < 0))) {
                    if(format.charAt(close + 1) !== '}') throw new Error('format stringFormatBraceMismatch');

                    result += format.slice(i, close + 1);
                    i = close + 2;
                    continue;
                }

                // Copy the string before the brace
                result += format.slice(i, open);
                i = open + 1;

                // Check for double braces (which display as one and are not arguments)
                if (format.charAt(i) === '{') {
                    result += '{';
                    i++;
                    continue;
                }

                if (close < 0) throw new Error('format stringFormatBraceMismatch');

                // Find the closing brace

                // Get the string between the braces, and split it around the ':' (if any)
                var brace = format.substring(i, close);
                var colonIndex = brace.indexOf(':');
                var argNumber = parseInt((colonIndex < 0) ? brace : brace.substring(0, colonIndex), 10) + 1;

                if (isNaN(argNumber)) throw new Error('format stringFormatInvalid');

                var argFormat = (colonIndex < 0) ? '' : brace.substring(colonIndex + 1);

                /** @type {VRS_FORMAT_ARG} */
                var arg = args[argNumber];
                if (typeof (arg) === "undefined" || arg === null) {
                    arg = '';
                }

                // If it has a toFormattedString method, call it.  Otherwise, call toString()
                if (arg.toFormattedString)              result += arg.toFormattedString(argFormat);
                else if (useLocale && arg.localeFormat) result += arg.localeFormat(argFormat);
                else if (arg.format)                    result += arg.format(argFormat);
                else if (arg.toFixed)                   result += this.formatNumber(<number>arg, argFormat);
                else                                    result += arg.toString();

                i = close + 1;
            }

            return result;
        }

        /**
         * Formats a number with an optional .NET-style format parameter.
         *
         * This does not try to ape every .NET number format, just those that VRS needs.
         */
        formatNumber(value: number, format?: number) : string;
        formatNumber(value: number, format?: string) : string;
        formatNumber(value: number, format?) : string
        {
            if(isNaN(value)) value = 0;
            var result: string;

            // Test for the most common case where only simple (or no) formatting is required.
            if(value >= 0) {
                if(format === undefined || format === null) return value.toString();
                if(typeof format === 'number') {
                    result = value.toString();
                    if(result.length < format) {
                        while(result.length < format) result = '0' + result;    // This is ugly but very quick.
                    }
                    return result;
                }
            }

            var culture = Globalize.culture();
            var leadingZeros = 0;
            var decimalPlaces = -1;         // -1 indicates that we just display the natural decimals from a toString() call
            var showSeparators = false;

            var decimalPosn = 0;
            if(format) {
                if(typeof format === 'number') format = this.repeatedSequence('0', format);
                if(format[0] === 'n' || format[0] === 'N') {
                    decimalPlaces = Number(format.substr(1));
                    showSeparators = true;
                } else {
                    decimalPosn = format.indexOf('.');
                    var integerFormat = decimalPosn === -1 ? format : format.substr(0, decimalPosn);
                    var decimalFormat = decimalPosn === -1 ? null : format.substr(decimalPosn + 1);

                    if(integerFormat == '0,000') {
                        showSeparators = true;
                        decimalPlaces = 0;
                    } else {
                        if(this.indexNotOf(integerFormat, '0') !== -1) throw 'Invalid format ' + format;
                        leadingZeros = integerFormat.length;
                    }

                    if(!decimalFormat) decimalPlaces = 0;
                    else {
                        if(this.indexNotOf(decimalFormat, '0') !== -1) throw 'Invalid format ' + format;
                        decimalPlaces = decimalFormat.length;
                    }
                }
            }

            var isNegative = value < 0;
            var numberText = decimalPlaces === -1 ? Math.abs(value).toString() : Math.abs(value).toFixed(decimalPlaces);
            decimalPosn = numberText.indexOf('.');
            var integerText = decimalPosn === -1 ? numberText : numberText.substr(0, decimalPosn);
            var decimalText = decimalPosn === -1 || decimalPlaces === 0 ? '' : numberText.substr(decimalPosn + 1);

            if(isNegative && integerText === '0' && this.indexNotOf(decimalText, '0') === -1) isNegative = false;

            if(leadingZeros) {
                var zerosRequired = leadingZeros - integerText.length;
                if(zerosRequired > 0) integerText = this.repeatedSequence('0', zerosRequired) + integerText;
            }

            if(showSeparators) {
                var groupSizes = culture.numberFormat.groupSizes;
                var groupSizeIndex = 0;
                var groupSize = groupSizes[groupSizeIndex];
                var stringIndex = integerText.length - 1;
                var separator = culture.numberFormat[','];
                var withSeparators = '';

                while(stringIndex >= 0) {
                    if(groupSize === 0 || groupSize > stringIndex) {
                        withSeparators = integerText.slice(0, stringIndex + 1) + (withSeparators.length ? (separator + withSeparators) : '');
                        break;
                    }
                    withSeparators = integerText.slice(stringIndex - groupSize + 1, stringIndex + 1) + (withSeparators.length ? (separator + withSeparators) : '');
                    stringIndex -= groupSize;
                    if(groupSizeIndex < groupSizes.length) groupSize = groupSizes[groupSizeIndex++];
                }
                integerText = withSeparators;
            }

            result = integerText;
            if(decimalText) {
                result += culture.numberFormat['.'];
                result += decimalText;
            }

            if(isNegative) {
                var pattern = culture.numberFormat.pattern[0];
                var indexOfN = pattern.indexOf('n');
                if(indexOfN === -1) throw 'Invalid negative pattern ' + pattern + ' for culture ' + culture.name;
                result = pattern.substr(0, indexOfN) + result + pattern.substr(indexOfN + 1);
            }

            return result || '';
        }

        /**
         * Returns the index of the first character that is not the character passed across.
         */
        indexNotOf(text: string, character: string) : number
        {
            if(!character || character.length !== 1) throw 'A single character must be supplied';
            var result = -1;
            if(text) {
                var length = text.length;
                for(var i = 0;i < length;++i) {
                    if(text[i] !== character) {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }

        /**
         * Returns true if the text is in upper-case.
         */
        isUpperCase(text: string) : boolean
        {
            return text && text.toUpperCase() == text;
        }

        /**
         * Returns a string consisting of the sequence repeated count times.
         */
        repeatedSequence(sequence: string, count: number) : string
        {
            // As ugly as this looks it's a lot quicker than other methods like new Array(count+1).join(sequence)
            var result = '';
            for(var i = 0;i < count;++i) {
                result += sequence;
            }

            return result;
        }

        /**
         * Returns the text padded with a string for length characters.
         */
        padLeft(text: string, ch: string, length: number) : string
        {
            return this.doPad(text, ch, length, false);
        }

        /**
         * Returns the text padded with a string for length characters.
         */
        padRight(text: string, ch: string, length: number) : string
        {
            return this.doPad(text, ch, length, true);
        }

        /**
         * Does the work for padLeft and padRight.
         */
        private doPad(text: string, ch: string, length: number, toRight: boolean) : string
        {
            if(text === null || text === undefined) text = '';
            var requiredCount = Math.ceil((length - text.length) / ch.length);

            return requiredCount <= 0 ? text :
                   toRight ? text + this.repeatedSequence(ch, requiredCount)
                           : this.repeatedSequence(ch, requiredCount) + text;
        }

        /**
         * Returns true if the text begins with the withText string.
         */
        startsWith(text: string, withText: string, ignoreCase: boolean = false) : boolean
        {
            return this.startsOrEndsWith(text, withText, ignoreCase, true);
        }

        /**
         * Returns true if the text ends with the withText string.
         */
        endsWith(text: string, withText: string, ignoreCase: boolean = false) : boolean
        {
            return this.startsOrEndsWith(text, withText, ignoreCase, false);
        };

        /**
         * Returns true if the text starts or ends with the withText string.
         */
        private startsOrEndsWith(text: string, withText: string, ignoreCase: boolean, fromStart: boolean) : boolean
        {
            var length = withText ? withText.length : 0;
            var result = !!(text && length && text.length >= length);
            if(result) {
                var chunk = fromStart ? text.substr(0, length) : text.slice(-length);
                if(ignoreCase) {
                    chunk = chunk.toUpperCase();
                    withText = withText.toUpperCase();
                }
                result = chunk === withText;
            }
            return result;
        }

        /**
         * Encodes the string in Base64 without incurring the 'failed to be encoded' error that Chrome throws when you
         * pass it characters that need more than one byte to encode. Based on snippet from MDN here:
         * https://developer.mozilla.org/en-US/docs/Web/API/WindowBase64/Base64_encoding_and_decoding#Solution_.232_.E2.80.93_rewriting_atob()_and_btoa()_using_TypedArrays_and_UTF-8
         * @param text
         */
        safeBtoa(text: string)
        {
            return btoa(encodeURIComponent(text).replace(/%([0-9A-F]{2})/g, function(match: any, p1: string) {
                return String.fromCharCode(Number('0x' + p1));
            }));
        }
    }

    /**
     * Pre-built instances of StringUtility. The code historically expects to be able to use these,
     * can't replace them with static methods.
     */
    export var stringUtility = new VRS.StringUtility();
}
