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
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview String utility methods.
 */

(function(VRS, $, /** object= */ undefined)
{
    /**
     * Methods for working with strings.
     *
     * The format() code was copied from http://stackoverflow.com/questions/2534803/string-format-in-javascript
     * which in turn was extracted from MicrosoftAjax.js by Sky Sanders on 28th March 2010.
     * @constructor
     */
    VRS.StringUtility = function()
    {
        //region -- Fields
        var that = this;
        //endregion

        //region -- contains
        /**
         * Returns true if the text contains the hasText string.
         * @param {string} text The text to search within.
         * @param {string} hasText The text to search for.
         * @param {bool} [ignoreCase] True if the search is to ignore case.
         * @returns {bool}
         */
        this.contains = function(text, hasText, ignoreCase)
        {
            var result = !!(text && hasText);
            if(result) {
                if(!ignoreCase) result = text.indexOf(hasText) !== -1;
                else            result = text.toUpperCase().indexOf(hasText.toUpperCase()) !== -1;
            }
            return result;
        };
        //endregion

        //region -- equals
        /**
         * Returns true if the lhs is equals to the rhs.
         * @param {string} lhs The first string to compare.
         * @param {string} rhs The second string to compare.
         * @param {bool} [ignoreCase] True if the comparison should ignore case.
         * @returns {bool}
         */
        this.equals = function(lhs, rhs, ignoreCase)
        {
            var result = false;
            if(!lhs) result = !rhs;
            else result = rhs && (!ignoreCase ? lhs === rhs : lhs.toUpperCase() === rhs.toUpperCase());
            return result;
        };
        //endregion

        //region -- filter
        /**
         * Calls the allowCharacter callback for each character in the text and returns the string formed from those
         * characters that the callback returns true for.
         * @param {string} text
         * @param {function(string):bool} allowCharacter
         * @returns {string}
         */
        this.filter = function(text, allowCharacter)
        {
            return that.filterReplace(text, function(ch) {
                return allowCharacter(ch) ? ch : null;
            })
        };
        //endregion

        //region -- filterReplace
        /**
         * Calls the replaceCharacter callback for each character in the text and returns the string formed from the
         * strings that the callback returns.
         * @param {string} text
         * @param {function(string):string} replaceCharacter
         * @returns {string}
         */
        this.filterReplace = function(text, replaceCharacter)
        {
            var result = text;
            if(result) {
                result = '';
                var length = text.length;
                var start = 0;
                var useChunk = function(end) {
                    result += text.substring(start, end);
                    start = end + 1;
                };
                for(var i = 0;i < length;++i) {
                    var ch = text[i];
                    var replaceWith = replaceCharacter(ch);
                    if(ch !== replaceWith) {
                        useChunk(i);
                        if(replaceWith) result += replaceWith;
                    }
                }
                useChunk(length);
            }

            return result;
        };
        //endregion

        //region -- htmlEscape
        /**
         * Returns the text with chevrons and ampersands escaped out for display in HTML.
         *
         * The aim of this function is to make the text HTML-safe in one pass - it is not to convert
         * every possible HTML character to escape codes.
         * @param {string} text
         * @returns {string}
         */
        this.htmlEscape = function(text)
        {
            return that.filterReplace(text, function(ch) {
                switch(ch) {
                    case '&':   return '&amp;';
                    case '<':   return '&lt;';
                    case '>':   return '&gt;';
                    default:    return ch;
                }
            });
        };
        //endregion

        //region -- format
        /**
         * Formats a .NET style format string and arguments.
         * @param {string} text The formatting string.
         * @param {...} args The arguments to the formatting string.
         * @returns {string} The formatted string.
         */
        this.format = function(text, args)
        {
            return toFormattedString(false, arguments);
        };

        /**
         * Replaces .NET style substitution markers with the arguments passed across.
         * @param {bool} useLocale
         * @param {...} args
         * @returns {string}
         */
        function toFormattedString(useLocale, args)
        {
            var result = '';
            /** @type {string} */ var format = args[0];

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
                    //noinspection JSValidateTypes
                    arg = '';
                }

                // If it has a toFormattedString method, call it.  Otherwise, call toString()
                if (arg.toFormattedString) result += arg.toFormattedString(argFormat);
                else if (useLocale && arg.localeFormat) result += arg.localeFormat(argFormat);
                else if (arg.format) result += arg.format(argFormat);
                else if (arg.toFixed) result += that.formatNumber(/** @type {number} */arg, argFormat);
                else result += arg.toString();

                i = close + 1;
            }

            return result;
        }
        //endregion

        //region -- formatNumber
        /**
         * Formats a number with an optional .NET-style format parameter.
         *
         * This does not try to ape every .NET number format, just those that VRS needs.
         * @param {number|undefined}    value   The value to format.
         * @param {string|number}      [format] The .NET-style format to apply to the number or, if it's a number, the minimum length of the string required (padded with leading 0).
         * @returns {string} The formatted number.
         */
        this.formatNumber = function(value, format)
        {
            if(isNaN(value)) value = 0;
            var result;

            // Test for the most common case where only simple (or no) formatting is required.
            if(value >= 0) {
                if(format === undefined || format === null) return value.toString();
                if(typeof format === 'number') {
                    result = value.toString();
                    if(result.length < format)
                    while(result.length < format) result = '0' + result;        // This is ugly but very quick.
                    return result;
                }
            }

            var culture = Globalize.culture();
            var leadingZeros = 0;
            var decimalPlaces = -1;         // -1 indicates that we just display the natural decimals from a toString() call
            var showSeparators = false;

            var decimalPosn = 0;
            if(format) {
                if(typeof format === 'number') format = that.repeatedSequence('0', format);
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
                        if(that.indexNotOf(integerFormat, '0') !== -1) throw 'Invalid format ' + format;
                        leadingZeros = integerFormat.length;
                    }

                    if(!decimalFormat) decimalPlaces = 0;
                    else {
                        if(that.indexNotOf(decimalFormat, '0') !== -1) throw 'Invalid format ' + format;
                        decimalPlaces = decimalFormat.length;
                    }
                }
            }

            var isNegative = value < 0;
            var numberText = decimalPlaces === -1 ? Math.abs(value).toString() : Math.abs(value).toFixed(decimalPlaces);
            decimalPosn = numberText.indexOf('.');
            var integerText = decimalPosn === -1 ? numberText : numberText.substr(0, decimalPosn);
            var decimalText = decimalPosn === -1 || decimalPlaces === 0 ? '' : numberText.substr(decimalPosn + 1);

            if(isNegative && integerText === '0' && that.indexNotOf(decimalText, '0') === -1) isNegative = false;

            if(leadingZeros) {
                var zerosRequired = leadingZeros - integerText.length;
                if(zerosRequired > 0) integerText = that.repeatedSequence('0', zerosRequired) + integerText;
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
        };
        //endregion

        //region -- indexNotOf
        /**
         * Returns the index of the first character that is not the character passed across.
         * @param {string} text
         * @param {string} character
         * @returns {number}
         */
        this.indexNotOf = function(text, character)
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
        };
        //endregion

        //region -- isUpperCase
        /**
         * Returns true if the text is in upper-case.
         * @param {string} text
         * @returns {boolean}
         */
        this.isUpperCase = function(text)
        {
            return text && text.toUpperCase() == text;
        };
        //endregion

        //region -- repeatedSequence
        /**
         * Returns a string consisting of the sequence repeated count times.
         * @param {string} sequence The text to repeat.
         * @param {number} count The number of times to repeat it.
         * @returns {string}
         */
        this.repeatedSequence = function(sequence, count)
        {
            // As ugly as this looks it's a lot quicker than other methods like new Array(count+1).join(sequence)
            var result = '';
            for(var i = 0;i < count;++i) {
                result += sequence;
            }

            return result;
        };
        //endregion

        //region -- padLeft, padRight
        /**
         * Returns the text padded with a string for length characters.
         * @param {String}  text        The text to pad.
         * @param {String}  ch          The sequence to keep adding to the left of the string until it is at least length characters long.
         * @param {Number}  length      The minimum length of the result.
         */
        this.padLeft = function(text, ch, length)
        {
            return doPad(text, ch, length, false);
        };

        /**
         * Returns the text padded with a string for length characters.
         * @param {String}  text        The text to pad.
         * @param {String}  ch          The sequence to keep adding to the right of the string until it is at least length characters long.
         * @param {Number}  length      The minimum length of the result.
         */
        this.padRight = function(text, ch, length)
        {
            return doPad(text, ch, length, true);
        };

        /**
         * Does the work for padLeft and padRight.
         * @param {string}  text
         * @param {string}  ch
         * @param {number}  length
         * @param {boolean} toRight
         * @returns {string}
         */
        function doPad(text, ch, length, toRight)
        {
            if(text === null || text === undefined) text = '';
            var requiredCount = Math.ceil((length - text.length) / ch.length);

            return requiredCount <= 0 ? text :
                   toRight ? text + that.repeatedSequence(ch, requiredCount)
                           : that.repeatedSequence(ch, requiredCount) + text;
        }
        //endregion

        //region -- startsWith, endsWith
        /**
         * Returns true if the text begins with the withText string.
         * @param {string} text The text to search within.
         * @param {string} withText The text to search for.
         * @param {bool} [ignoreCase] True if the search is to ignore case.
         * @returns {bool}
         */
        this.startsWith = function(text, withText, ignoreCase)
        {
            return startsOrEndsWith(text, withText, ignoreCase, true);
        };

        /**
         * Returns true if the text ends with the withText string.
         * @param {string} text The text to search within.
         * @param {string} withText The text to search for.
         * @param {bool} [ignoreCase] True if the search is to ignore case.
         * @returns {bool}
         */
        this.endsWith = function(text, withText, ignoreCase)
        {
            return startsOrEndsWith(text, withText, ignoreCase, false);
        };

        /**
         * Returns true if the text starts or ends with the withText string.
         * @param {string} text
         * @param {string} withText
         * @param {bool} ignoreCase
         * @param {bool} fromStart
         * @returns {bool}
         */
        function startsOrEndsWith(text, withText, ignoreCase, fromStart)
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
        //endregion
    };

    //region Pre-builts
    VRS.stringUtility = new VRS.StringUtility();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));