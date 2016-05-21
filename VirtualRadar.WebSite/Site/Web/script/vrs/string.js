var VRS;
(function (VRS) {
    var StringUtility = (function () {
        function StringUtility() {
        }
        StringUtility.prototype.contains = function (text, hasText, ignoreCase) {
            if (ignoreCase === void 0) { ignoreCase = false; }
            var result = !!(text && hasText);
            if (result) {
                if (!ignoreCase)
                    result = text.indexOf(hasText) !== -1;
                else
                    result = text.toUpperCase().indexOf(hasText.toUpperCase()) !== -1;
            }
            return result;
        };
        StringUtility.prototype.equals = function (lhs, rhs, ignoreCase) {
            if (ignoreCase === void 0) { ignoreCase = false; }
            var result = false;
            if (!lhs)
                result = !rhs;
            else
                result = rhs && (!ignoreCase ? lhs === rhs : lhs.toUpperCase() === rhs.toUpperCase());
            return result;
        };
        StringUtility.prototype.filter = function (text, allowCharacter) {
            return this.filterReplace(text, function (ch) {
                return allowCharacter(ch) ? ch : null;
            });
        };
        StringUtility.prototype.filterReplace = function (text, replaceCharacter) {
            var result = text;
            if (result) {
                result = '';
                var length_1 = text.length;
                var start_1 = 0;
                var useChunk = function (end) {
                    result += text.substring(start_1, end);
                    start_1 = end + 1;
                };
                for (var i = 0; i < length_1; ++i) {
                    var ch = text[i];
                    var replaceWith = replaceCharacter(ch);
                    if (ch !== replaceWith) {
                        useChunk(i);
                        if (replaceWith)
                            result += replaceWith;
                    }
                }
                useChunk(length_1);
            }
            return result;
        };
        StringUtility.prototype.htmlEscape = function (text) {
            return this.filterReplace(text, function (ch) {
                switch (ch) {
                    case '&': return '&amp;';
                    case '<': return '&lt;';
                    case '>': return '&gt;';
                    default: return ch;
                }
            });
        };
        StringUtility.prototype.format = function (text) {
            var args = [];
            for (var _i = 1; _i < arguments.length; _i++) {
                args[_i - 1] = arguments[_i];
            }
            return this.toFormattedString(false, arguments);
        };
        StringUtility.prototype.toFormattedString = function (useLocale, args) {
            var result = '';
            var format = args[0];
            for (var i = 0;;) {
                var open = format.indexOf('{', i);
                var close = format.indexOf('}', i);
                if ((open < 0) && (close < 0)) {
                    result += format.slice(i);
                    break;
                }
                if ((close > 0) && ((close < open) || (open < 0))) {
                    if (format.charAt(close + 1) !== '}')
                        throw new Error('format stringFormatBraceMismatch');
                    result += format.slice(i, close + 1);
                    i = close + 2;
                    continue;
                }
                result += format.slice(i, open);
                i = open + 1;
                if (format.charAt(i) === '{') {
                    result += '{';
                    i++;
                    continue;
                }
                if (close < 0)
                    throw new Error('format stringFormatBraceMismatch');
                var brace = format.substring(i, close);
                var colonIndex = brace.indexOf(':');
                var argNumber = parseInt((colonIndex < 0) ? brace : brace.substring(0, colonIndex), 10) + 1;
                if (isNaN(argNumber))
                    throw new Error('format stringFormatInvalid');
                var argFormat = (colonIndex < 0) ? '' : brace.substring(colonIndex + 1);
                var arg = args[argNumber];
                if (typeof (arg) === "undefined" || arg === null) {
                    arg = '';
                }
                if (arg.toFormattedString)
                    result += arg.toFormattedString(argFormat);
                else if (useLocale && arg.localeFormat)
                    result += arg.localeFormat(argFormat);
                else if (arg.format)
                    result += arg.format(argFormat);
                else if (arg.toFixed)
                    result += this.formatNumber(arg, argFormat);
                else
                    result += arg.toString();
                i = close + 1;
            }
            return result;
        };
        StringUtility.prototype.formatNumber = function (value, format) {
            if (isNaN(value))
                value = 0;
            var result;
            if (value >= 0) {
                if (format === undefined || format === null)
                    return value.toString();
                if (typeof format === 'number') {
                    result = value.toString();
                    if (result.length < format) {
                        while (result.length < format)
                            result = '0' + result;
                    }
                    return result;
                }
            }
            var culture = Globalize.culture();
            var leadingZeros = 0;
            var decimalPlaces = -1;
            var showSeparators = false;
            var decimalPosn = 0;
            if (format) {
                if (typeof format === 'number')
                    format = this.repeatedSequence('0', format);
                if (format[0] === 'n' || format[0] === 'N') {
                    decimalPlaces = Number(format.substr(1));
                    showSeparators = true;
                }
                else {
                    decimalPosn = format.indexOf('.');
                    var integerFormat = decimalPosn === -1 ? format : format.substr(0, decimalPosn);
                    var decimalFormat = decimalPosn === -1 ? null : format.substr(decimalPosn + 1);
                    if (integerFormat == '0,000') {
                        showSeparators = true;
                        decimalPlaces = 0;
                    }
                    else {
                        if (this.indexNotOf(integerFormat, '0') !== -1)
                            throw 'Invalid format ' + format;
                        leadingZeros = integerFormat.length;
                    }
                    if (!decimalFormat)
                        decimalPlaces = 0;
                    else {
                        if (this.indexNotOf(decimalFormat, '0') !== -1)
                            throw 'Invalid format ' + format;
                        decimalPlaces = decimalFormat.length;
                    }
                }
            }
            var isNegative = value < 0;
            var numberText = decimalPlaces === -1 ? Math.abs(value).toString() : Math.abs(value).toFixed(decimalPlaces);
            decimalPosn = numberText.indexOf('.');
            var integerText = decimalPosn === -1 ? numberText : numberText.substr(0, decimalPosn);
            var decimalText = decimalPosn === -1 || decimalPlaces === 0 ? '' : numberText.substr(decimalPosn + 1);
            if (isNegative && integerText === '0' && this.indexNotOf(decimalText, '0') === -1)
                isNegative = false;
            if (leadingZeros) {
                var zerosRequired = leadingZeros - integerText.length;
                if (zerosRequired > 0)
                    integerText = this.repeatedSequence('0', zerosRequired) + integerText;
            }
            if (showSeparators) {
                var groupSizes = culture.numberFormat.groupSizes;
                var groupSizeIndex = 0;
                var groupSize = groupSizes[groupSizeIndex];
                var stringIndex = integerText.length - 1;
                var separator = culture.numberFormat[','];
                var withSeparators = '';
                while (stringIndex >= 0) {
                    if (groupSize === 0 || groupSize > stringIndex) {
                        withSeparators = integerText.slice(0, stringIndex + 1) + (withSeparators.length ? (separator + withSeparators) : '');
                        break;
                    }
                    withSeparators = integerText.slice(stringIndex - groupSize + 1, stringIndex + 1) + (withSeparators.length ? (separator + withSeparators) : '');
                    stringIndex -= groupSize;
                    if (groupSizeIndex < groupSizes.length)
                        groupSize = groupSizes[groupSizeIndex++];
                }
                integerText = withSeparators;
            }
            result = integerText;
            if (decimalText) {
                result += culture.numberFormat['.'];
                result += decimalText;
            }
            if (isNegative) {
                var pattern = culture.numberFormat.pattern[0];
                var indexOfN = pattern.indexOf('n');
                if (indexOfN === -1)
                    throw 'Invalid negative pattern ' + pattern + ' for culture ' + culture.name;
                result = pattern.substr(0, indexOfN) + result + pattern.substr(indexOfN + 1);
            }
            return result || '';
        };
        StringUtility.prototype.indexNotOf = function (text, character) {
            if (!character || character.length !== 1)
                throw 'A single character must be supplied';
            var result = -1;
            if (text) {
                var length = text.length;
                for (var i = 0; i < length; ++i) {
                    if (text[i] !== character) {
                        result = i;
                        break;
                    }
                }
            }
            return result;
        };
        StringUtility.prototype.isUpperCase = function (text) {
            return text && text.toUpperCase() == text;
        };
        StringUtility.prototype.repeatedSequence = function (sequence, count) {
            var result = '';
            for (var i = 0; i < count; ++i) {
                result += sequence;
            }
            return result;
        };
        StringUtility.prototype.padLeft = function (text, ch, length) {
            return this.doPad(text, ch, length, false);
        };
        StringUtility.prototype.padRight = function (text, ch, length) {
            return this.doPad(text, ch, length, true);
        };
        StringUtility.prototype.doPad = function (text, ch, length, toRight) {
            if (text === null || text === undefined)
                text = '';
            var requiredCount = Math.ceil((length - text.length) / ch.length);
            return requiredCount <= 0 ? text :
                toRight ? text + this.repeatedSequence(ch, requiredCount)
                    : this.repeatedSequence(ch, requiredCount) + text;
        };
        StringUtility.prototype.startsWith = function (text, withText, ignoreCase) {
            if (ignoreCase === void 0) { ignoreCase = false; }
            return this.startsOrEndsWith(text, withText, ignoreCase, true);
        };
        StringUtility.prototype.endsWith = function (text, withText, ignoreCase) {
            if (ignoreCase === void 0) { ignoreCase = false; }
            return this.startsOrEndsWith(text, withText, ignoreCase, false);
        };
        ;
        StringUtility.prototype.startsOrEndsWith = function (text, withText, ignoreCase, fromStart) {
            var length = withText ? withText.length : 0;
            var result = !!(text && length && text.length >= length);
            if (result) {
                var chunk = fromStart ? text.substr(0, length) : text.slice(-length);
                if (ignoreCase) {
                    chunk = chunk.toUpperCase();
                    withText = withText.toUpperCase();
                }
                result = chunk === withText;
            }
            return result;
        };
        return StringUtility;
    }());
    VRS.StringUtility = StringUtility;
    VRS.stringUtility = new VRS.StringUtility();
})(VRS || (VRS = {}));
//# sourceMappingURL=string.js.map