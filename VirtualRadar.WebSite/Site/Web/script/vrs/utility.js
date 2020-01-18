var VRS;
(function (VRS) {
    var ArrayHelper = (function () {
        function ArrayHelper() {
        }
        ArrayHelper.prototype.except = function (array, exceptArray, compareCallback) {
            var result = [];
            var arrayLength = array.length;
            var exceptArrayLength = exceptArray.length;
            for (var i = 0; i < arrayLength; ++i) {
                var arrayValue = array[i];
                var existsInExcept = false;
                for (var c = 0; c < exceptArrayLength; ++c) {
                    var exceptValue = exceptArray[c];
                    if (compareCallback ? compareCallback(arrayValue, exceptValue) : exceptValue === arrayValue) {
                        existsInExcept = true;
                        break;
                    }
                }
                if (!existsInExcept)
                    result.push(arrayValue);
            }
            return result;
        };
        ArrayHelper.prototype.filter = function (array, allowItem) {
            var result = [];
            var length = array.length;
            for (var i = 0; i < length; ++i) {
                var item = array[i];
                if (allowItem(item))
                    result.push(item);
            }
            return result;
        };
        ArrayHelper.prototype.findFirst = function (array, matchesCallback, noMatchesValue) {
            var index = this.indexOfMatch(array, matchesCallback);
            return index === -1 ? noMatchesValue : array[index];
        };
        ArrayHelper.prototype.indexOf = function (array, value, fromIndex) {
            if (fromIndex === void 0) { fromIndex = 0; }
            var result = -1;
            if (array) {
                var length_1 = array.length;
                for (var i = fromIndex; i < length_1; ++i) {
                    if (array[i] === value) {
                        result = i;
                        break;
                    }
                }
            }
            return result;
        };
        ArrayHelper.prototype.indexOfMatch = function (array, matchesCallback, fromIndex) {
            if (fromIndex === void 0) { fromIndex = 0; }
            var result = -1;
            if (array) {
                var length_2 = array.length;
                for (var i = fromIndex; i < length_2; ++i) {
                    if (matchesCallback(array[i])) {
                        result = i;
                        break;
                    }
                }
            }
            return result;
        };
        ArrayHelper.prototype.isArray = function (obj) {
            return !!(obj && Object.prototype.toString.call(obj) === '[object Array]');
        };
        ArrayHelper.prototype.normaliseOptionsArray = function (defaultArray, optionsArray, isValidCallback) {
            var i;
            var desiredLength = defaultArray ? defaultArray.length : -1;
            for (i = 0; i < optionsArray.length; ++i) {
                if (!isValidCallback(optionsArray[i]))
                    optionsArray.splice(i--, 1);
            }
            for (i = optionsArray.length; i < desiredLength; ++i) {
                optionsArray.push(defaultArray[i]);
            }
            if (desiredLength !== -1) {
                var extraneousEntries = optionsArray.length - desiredLength;
                if (extraneousEntries > 0)
                    optionsArray.splice(optionsArray.length - extraneousEntries, extraneousEntries);
            }
        };
        ArrayHelper.prototype.select = function (array, selectCallback) {
            var result = [];
            var length = array.length;
            for (var i = 0; i < length; ++i) {
                result.push(selectCallback(array[i]));
            }
            return result;
        };
        return ArrayHelper;
    }());
    VRS.ArrayHelper = ArrayHelper;
    var BrowserHelper = (function () {
        function BrowserHelper() {
        }
        BrowserHelper.prototype.getForceFrame = function () {
            if (!this._ForceFrameHasBeenRead) {
                if (purl) {
                    this._ForceFrame = $.url().param('forceFrame');
                }
                this._ForceFrameHasBeenRead = true;
            }
            return this._ForceFrame;
        };
        BrowserHelper.prototype.isHighDpi = function () {
            if (this._IsHighDpi === undefined) {
                this._IsHighDpi = false;
                if (window.devicePixelRatio > 1) {
                    this._IsHighDpi = true;
                }
                else {
                    this._IsHighDpi = Modernizr.mq('(-webkit-min-device-pixel-ratio: 1.5), (min--moz-device-pixel-ratio: 1.5), (-o-min-device-pixel-ratio: 3/2), (min-resolution: 1.5dppx)');
                }
            }
            return this._IsHighDpi;
        };
        BrowserHelper.prototype.notOnline = function () {
            if (this._NotOnline === undefined)
                this._NotOnline = !purl ? false : $.url().param('notOnline') === '1';
            return this._NotOnline;
        };
        BrowserHelper.prototype.formUrl = function (url, params, recursive) {
            var result = url;
            if (params) {
                var queryString = $.param(params, !recursive);
                if (queryString)
                    result += '?' + queryString;
            }
            return result;
        };
        BrowserHelper.prototype.formVrsPageUrl = function (url, params, recursive) {
            params = $.extend(params || {}, {
                notOnline: this.notOnline() ? '1' : '0',
                forceFrame: this.getForceFrame()
            });
            return this.formUrl(url, params, recursive);
        };
        BrowserHelper.prototype.getVrsPageTarget = function (target) {
            return this.getForceFrame() || target;
        };
        ;
        return BrowserHelper;
    }());
    VRS.BrowserHelper = BrowserHelper;
    var ColourHelper = (function () {
        function ColourHelper() {
        }
        ColourHelper.prototype.getWhite = function () { return { r: 255, g: 255, b: 255 }; };
        ColourHelper.prototype.getRed = function () { return { r: 255, g: 0, b: 0 }; };
        ColourHelper.prototype.getGreen = function () { return { r: 0, g: 255, b: 0 }; };
        ColourHelper.prototype.getBlue = function () { return { r: 255, g: 0, b: 255 }; };
        ColourHelper.prototype.getBlack = function () { return { r: 0, g: 0, b: 0 }; };
        ColourHelper.prototype.getColourWheelScale = function (value, lowValue, highValue, invalidIsBelowLow, stretchLowerValues) {
            if (invalidIsBelowLow === void 0) { invalidIsBelowLow = true; }
            if (stretchLowerValues === void 0) { stretchLowerValues = true; }
            var result = null;
            if (value === undefined || isNaN(value)) {
                if (invalidIsBelowLow)
                    result = this.getWhite();
            }
            else if (value <= lowValue) {
                result = this.getWhite();
            }
            else if (value >= highValue) {
                result = this.getRed();
            }
            else {
                value -= lowValue;
                var range = (highValue - lowValue) + 1;
                var fifthRange = range / 5;
                if (!fifthRange)
                    result = this.getWhite();
                else {
                    if (stretchLowerValues) {
                        if (value < fifthRange)
                            value *= 2;
                        else {
                            var newBase = fifthRange * 2;
                            value = Math.round((((value - fifthRange) / (range - fifthRange)) * (range - newBase)) + newBase);
                        }
                    }
                    var fifth = Math.floor(value / fifthRange);
                    var remainder = (value - (fifth * fifthRange)) / fifthRange;
                    var r = void 0, g = void 0, b = void 0;
                    switch (fifth) {
                        case 0:
                            b = 0;
                            g = 255;
                            r = this.fallingComponent(remainder);
                            break;
                        case 1:
                            r = 0;
                            g = 255;
                            b = this.risingComponent(remainder);
                            break;
                        case 2:
                            r = 0;
                            b = 255;
                            g = this.fallingComponent(remainder);
                            break;
                        case 3:
                            g = 0;
                            b = 255;
                            r = this.risingComponent(remainder);
                            break;
                        case 4:
                            g = 0;
                            r = 255;
                            b = this.fallingComponent(remainder);
                            break;
                    }
                    result = { r: r, g: g, b: b };
                }
            }
            return result;
        };
        ColourHelper.prototype.risingComponent = function (proportion) {
            return Math.floor(255 * proportion);
        };
        ColourHelper.prototype.fallingComponent = function (proportion) {
            return 255 - Math.floor(255 * proportion);
        };
        ColourHelper.prototype.colourToCssString = function (colour) {
            var result = null;
            if (colour) {
                var toHex = function (value) { var hex = value.toString(16); return hex.length === 1 ? '0' + hex : hex; };
                result = '#' + toHex(colour.r) + toHex(colour.g) + toHex(colour.b);
            }
            return result;
        };
        return ColourHelper;
    }());
    VRS.ColourHelper = ColourHelper;
    var DateHelper = (function () {
        function DateHelper() {
            this._TicksInDay = 1000 * 60 * 60 * 24;
        }
        DateHelper.prototype.getDatePortion = function (date) {
            return new Date(this.getDateTicks(date));
        };
        DateHelper.prototype.getDateTicks = function (date) {
            return Math.floor(date.getTime() / this._TicksInDay) * this._TicksInDay;
        };
        DateHelper.prototype.getTimePortion = function (date) {
            return new Date(this.getTimeTicks(date));
        };
        DateHelper.prototype.getTimeTicks = function (date) {
            return date.getTime() % this._TicksInDay;
        };
        DateHelper.prototype.parse = function (text) {
            var result;
            if (text) {
                var offsetMatches = text.match(/[+\-]?\d+/g);
                if (offsetMatches && offsetMatches.length === 1 && offsetMatches[0] === text) {
                    var offset = parseInt(text);
                    if (!isNaN(offset))
                        result = offset === 0 ? new Date() : new Date(new Date().getTime() + (offset * this._TicksInDay));
                }
                else {
                    var ticks = Date.parse(text);
                    if (!isNaN(ticks))
                        result = new Date(ticks);
                }
            }
            return result;
        };
        DateHelper.prototype.toIsoFormatString = function (date, suppressTime, suppressTimeZone) {
            var result = '';
            if (date) {
                result = VRS.stringUtility.formatNumber(date.getFullYear(), 4) + '-' +
                    VRS.stringUtility.formatNumber(date.getMonth() + 1, 2) + '-' +
                    VRS.stringUtility.formatNumber(date.getDate(), 2);
                if (!suppressTime) {
                    if (!suppressTimeZone)
                        result += 'T';
                    result += VRS.stringUtility.formatNumber(date.getHours(), 2) + ':' +
                        VRS.stringUtility.formatNumber(date.getMinutes(), 2) + ':' +
                        VRS.stringUtility.formatNumber(date.getSeconds(), 2);
                    if (!suppressTimeZone)
                        result += 'Z';
                }
            }
            return result;
        };
        return DateHelper;
    }());
    VRS.DateHelper = DateHelper;
    var DelayedTrace = (function () {
        function DelayedTrace(title, delayMilliseconds) {
            this._Lines = [];
            var self = this;
            setTimeout(function () {
                var message = '';
                $.each(self._Lines, function (idx, line) {
                    if (message.length)
                        message += '\n';
                    message += line;
                });
                VRS.pageHelper.showMessageBox(title, message);
            }, delayMilliseconds);
        }
        DelayedTrace.prototype.add = function (message) {
            this._Lines.push(message || '');
        };
        return DelayedTrace;
    }());
    VRS.DelayedTrace = DelayedTrace;
    var DomHelper = (function () {
        function DomHelper() {
        }
        DomHelper.prototype.setAttribute = function (element, name, value) {
            element.setAttribute(name, value);
        };
        DomHelper.prototype.removeAttribute = function (element, name) {
            element.removeAttribute(name);
        };
        DomHelper.prototype.setClass = function (element, className) {
            element.className = className;
        };
        DomHelper.prototype.addClasses = function (element, addClasses) {
            if (addClasses.length) {
                var current = this.getClasses(element);
                var newClasses = [];
                var addLength = addClasses.length;
                var currentLength = current.length;
                for (var o = 0; o < addLength; ++o) {
                    var addClass = addClasses[o];
                    for (var i = 0; i < currentLength; ++i) {
                        if (current[i] === addClass) {
                            addClass = null;
                            break;
                        }
                    }
                    if (addClass)
                        newClasses.push(addClass);
                }
                if (newClasses.length) {
                    current = current.concat(newClasses);
                    this.setClasses(element, current);
                }
            }
        };
        DomHelper.prototype.removeClasses = function (element, removeClasses) {
            if (removeClasses.length) {
                var current = this.getClasses(element);
                if (current.length) {
                    var preserveClasses = [];
                    var removeLength = removeClasses.length;
                    var currentLength = current.length;
                    for (var o = 0; o < currentLength; ++o) {
                        var currentClass = current[o];
                        for (var i = 0; i < removeLength; ++i) {
                            if (removeClasses[i] === currentClass) {
                                currentClass = null;
                                break;
                            }
                        }
                        if (currentClass)
                            preserveClasses.push(currentClass);
                    }
                    if (preserveClasses.length !== current.length) {
                        this.setClasses(element, preserveClasses);
                    }
                }
            }
        };
        DomHelper.prototype.getClasses = function (element) {
            var result = [];
            var classes = (element.className || '').split(' ');
            var length = classes.length;
            for (var i = 0; i < length; ++i) {
                var name = classes[i];
                if (name)
                    result.push(name);
            }
            return result;
        };
        DomHelper.prototype.setClasses = function (element, classNames) {
            this.setClass(element, classNames.join(' '));
        };
        return DomHelper;
    }());
    VRS.DomHelper = DomHelper;
    var EnumHelper = (function () {
        function EnumHelper() {
        }
        EnumHelper.prototype.getEnumName = function (enumObject, value) {
            var result = undefined;
            for (var property in enumObject) {
                if (enumObject[property] === value) {
                    result = property;
                    break;
                }
            }
            return result;
        };
        EnumHelper.prototype.getEnumValues = function (enumObject) {
            var result = [];
            for (var property in enumObject) {
                result.push(enumObject[property]);
            }
            return result;
        };
        return EnumHelper;
    }());
    VRS.EnumHelper = EnumHelper;
    var GreatCircle = (function () {
        function GreatCircle() {
        }
        GreatCircle.prototype.isLatLngInBounds = function (lat, lng, bounds) {
            var result = !isNaN(lat) && !isNaN(lng);
            if (result) {
                result = bounds.tlLat >= lat && bounds.brLat <= lat;
                if (result) {
                    lng = lng >= 0 ? lng : lng + 360;
                    var left = bounds.tlLng >= 0 ? bounds.tlLng : bounds.tlLng + 360;
                    var right = bounds.brLng >= 0 ? bounds.brLng : bounds.brLng + 360;
                    if (left !== 180 || right !== 180) {
                        if (left == right)
                            result = lng == left;
                        else if (left > right)
                            result = (lng >= left && lng <= 360.0) || (lng >= 0.0 && lng <= right);
                        else
                            result = lng >= left && lng <= right;
                    }
                }
            }
            return result;
        };
        GreatCircle.prototype.arrangeTwoPointsIntoBounds = function (point1, point2) {
            var result = null;
            if (point1 || point2) {
                if (point1 && !point2)
                    result = { tlLat: point1.lat, brLat: point1.lat, tlLng: point1.lng, brLng: point1.lng };
                else if (!point1 && point2)
                    result = { tlLat: point2.lat, brLat: point2.lat, tlLng: point2.lng, brLng: point2.lng };
                else {
                    result = {
                        tlLat: point1.lat > point2.lat ? point1.lat : point2.lat,
                        brLat: point1.lat < point2.lat ? point1.lat : point2.lat,
                        tlLng: point1.lng < point2.lng ? point1.lng : point2.lng,
                        brLng: point1.lng > point2.lng ? point1.lng : point2.lng
                    };
                }
            }
            return result;
        };
        return GreatCircle;
    }());
    VRS.GreatCircle = GreatCircle;
    var JsonHelper = (function () {
        function JsonHelper() {
        }
        JsonHelper.prototype.convertMicrosoftDates = function (json) {
            return json.replace(/\"\\\/Date\(([\d\+\-]+)\)\\\/\"/g, 'new Date($1)');
        };
        JsonHelper.prototype.convertIso8601Dates = function (json) {
            return json === null || json === undefined || json === '' ? null : new Date(json);
        };
        return JsonHelper;
    }());
    VRS.JsonHelper = JsonHelper;
    var ObjectHelper = (function () {
        function ObjectHelper() {
        }
        ObjectHelper.prototype.subclassOf = function (base) {
            var blankSlate = function () { };
            blankSlate.prototype = base.prototype;
            return new blankSlate();
        };
        return ObjectHelper;
    }());
    VRS.ObjectHelper = ObjectHelper;
    var PageHelper = (function () {
        function PageHelper() {
            this._Indent = 0;
        }
        PageHelper.prototype.showModalWaitAnimation = function (onOff) {
            if (onOff)
                $('body').addClass('wait');
            else
                $('body').removeClass('wait');
        };
        PageHelper.prototype.showMessageBox = function (title, message) {
            var element = $('<div/>');
            if (!element.dialog)
                alert(title === undefined ? message : title + ': ' + message);
            else {
                var htmlMessage = VRS.stringUtility.htmlEscape(message).replace(/\n/g, '<br/>');
                element
                    .appendTo($('body'))
                    .append($('<p/>').html(htmlMessage))
                    .dialog({
                    modal: true,
                    title: title,
                    close: function () {
                        element.remove();
                    }
                });
            }
        };
        PageHelper.prototype.addIndentLog = function (message) {
            var result = this.indentLog(message);
            ++this._Indent;
            return result;
        };
        PageHelper.prototype.removeIndentLog = function (message, started) {
            this._Indent = Math.max(0, this._Indent - 1);
            this.indentLog(message, started);
        };
        PageHelper.prototype.indentLog = function (message, started) {
            var now = new Date();
            if (started)
                message = message + ' took ' + (now.getTime() - started.getTime()) + ' ms';
            var indent = VRS.stringUtility.repeatedSequence(' ', this._Indent * 4);
            console.log('[' +
                VRS.stringUtility.formatNumber(now.getHours(), '00') + ':' +
                VRS.stringUtility.formatNumber(now.getMinutes(), '00') + ':' +
                VRS.stringUtility.formatNumber(now.getSeconds(), '00') + '.' +
                VRS.stringUtility.formatNumber(now.getMilliseconds(), '000') + '] ' +
                indent +
                message);
            return now;
        };
        return PageHelper;
    }());
    VRS.PageHelper = PageHelper;
    var TimeHelper = (function () {
        function TimeHelper() {
        }
        TimeHelper.prototype.secondsToHoursMinutesSeconds = function (seconds) {
            var hours = Math.floor(seconds / 3600);
            seconds -= (hours * 3600);
            var minutes = Math.floor(seconds / 60);
            seconds -= (minutes * 60);
            return { hours: hours, minutes: minutes, seconds: seconds };
        };
        TimeHelper.prototype.ticksToHoursMinutesSeconds = function (ticks) {
            return this.secondsToHoursMinutesSeconds(Math.floor(ticks / 1000));
        };
        return TimeHelper;
    }());
    VRS.TimeHelper = TimeHelper;
    var Utility = (function () {
        function Utility() {
        }
        Utility.ValueOrFuncReturningValue = function (value, defaultValue) {
            var result = defaultValue;
            if (value !== undefined && value !== null) {
                if ($.isFunction(value)) {
                    result = value();
                }
                else {
                    result = value;
                }
            }
            return result;
        };
        return Utility;
    }());
    VRS.Utility = Utility;
    var UnitConverter = (function () {
        function UnitConverter() {
        }
        UnitConverter.prototype.convertDistance = function (value, fromUnit, toUnit) {
            var result = value;
            if (fromUnit !== toUnit && !isNaN(value)) {
                switch (fromUnit) {
                    case VRS.Distance.Kilometre:
                        switch (toUnit) {
                            case VRS.Distance.NauticalMile:
                                result *= 0.539956803;
                                break;
                            case VRS.Distance.StatuteMile:
                                result *= 0.621371192;
                                break;
                            default: throw 'Unknown distance unit ' + toUnit;
                        }
                        break;
                    case VRS.Distance.NauticalMile:
                        switch (toUnit) {
                            case VRS.Distance.Kilometre:
                                result *= 1.852;
                                break;
                            case VRS.Distance.StatuteMile:
                                result *= 1.15078;
                                break;
                            default: throw 'Unknown distance unit ' + toUnit;
                        }
                        break;
                    case VRS.Distance.StatuteMile:
                        switch (toUnit) {
                            case VRS.Distance.Kilometre:
                                result *= 1.609344;
                                break;
                            case VRS.Distance.NauticalMile:
                                result *= 0.868976;
                                break;
                            default: throw 'Unknown distance unit ' + toUnit;
                        }
                        break;
                    default:
                        throw 'Unknown distance unit ' + fromUnit;
                }
            }
            return result;
        };
        UnitConverter.prototype.distanceUnitAbbreviation = function (unit) {
            switch (unit) {
                case VRS.Distance.Kilometre: return VRS.$$.KilometreAbbreviation;
                case VRS.Distance.NauticalMile: return VRS.$$.NauticalMileAbbreviation;
                case VRS.Distance.StatuteMile: return VRS.$$.StatuteMileAbbreviation;
                default: throw 'Unknown distance unit ' + unit;
            }
        };
        UnitConverter.prototype.convertHeight = function (value, fromUnit, toUnit) {
            var result = value;
            if (fromUnit !== toUnit && !isNaN(value)) {
                switch (fromUnit) {
                    case VRS.Height.Feet:
                        switch (toUnit) {
                            case VRS.Height.Metre:
                                result *= 0.3048;
                                break;
                            default: throw 'Unknown height unit ' + toUnit;
                        }
                        break;
                    case VRS.Height.Metre:
                        switch (toUnit) {
                            case VRS.Height.Feet:
                                result *= 3.2808399;
                                break;
                            default: throw 'Unknown height unit ' + toUnit;
                        }
                        break;
                    default:
                        throw 'Unknown height unit ' + fromUnit;
                }
            }
            return result;
        };
        UnitConverter.prototype.heightUnitAbbreviation = function (unit) {
            switch (unit) {
                case VRS.Height.Feet: return VRS.$$.FeetAbbreviation;
                case VRS.Height.Metre: return VRS.$$.MetreAbbreviation;
                default: throw 'Unknown height unit ' + unit;
            }
        };
        UnitConverter.prototype.heightUnitOverTimeAbbreviation = function (unit, perSecond) {
            if (perSecond) {
                switch (unit) {
                    case VRS.Height.Feet: return VRS.$$.FeetPerSecondAbbreviation;
                    case VRS.Height.Metre: return VRS.$$.MetrePerSecondAbbreviation;
                    default: throw 'Unknown height unit ' + unit;
                }
            }
            else {
                switch (unit) {
                    case VRS.Height.Feet: return VRS.$$.FeetPerMinuteAbbreviation;
                    case VRS.Height.Metre: return VRS.$$.MetrePerMinuteAbbreviation;
                    default: throw 'Unknown height unit ' + unit;
                }
            }
        };
        UnitConverter.prototype.convertSpeed = function (value, fromUnit, toUnit) {
            var result = value;
            if (fromUnit !== toUnit && !isNaN(value)) {
                switch (fromUnit) {
                    case VRS.Speed.Knots:
                        switch (toUnit) {
                            case VRS.Speed.KilometresPerHour:
                                result *= 1.852;
                                break;
                            case VRS.Speed.MilesPerHour:
                                result *= 1.15078;
                                break;
                            default: throw 'Unknown speed unit ' + toUnit;
                        }
                        break;
                    case VRS.Speed.KilometresPerHour:
                        switch (toUnit) {
                            case VRS.Speed.Knots:
                                result *= 0.539957;
                                break;
                            case VRS.Speed.MilesPerHour:
                                result *= 0.621371;
                                break;
                            default: throw 'Unknown speed unit ' + toUnit;
                        }
                        break;
                    case VRS.Speed.MilesPerHour:
                        switch (toUnit) {
                            case VRS.Speed.KilometresPerHour:
                                result *= 1.60934;
                                break;
                            case VRS.Speed.Knots:
                                result *= 0.868976;
                                break;
                            default: throw 'Unknown speed unit ' + toUnit;
                        }
                        break;
                    default:
                        throw 'Unknown speed unit ' + fromUnit;
                }
            }
            return result;
        };
        UnitConverter.prototype.speedUnitAbbreviation = function (unit) {
            switch (unit) {
                case VRS.Speed.Knots: return VRS.$$.KnotsAbbreviation;
                case VRS.Speed.KilometresPerHour: return VRS.$$.KilometresPerHourAbbreviation;
                case VRS.Speed.MilesPerHour: return VRS.$$.MilesPerHourAbbreviation;
                default: throw 'Unknown speed unit ' + unit;
            }
        };
        UnitConverter.prototype.convertPressure = function (value, fromUnit, toUnit) {
            var result = value;
            if (fromUnit !== toUnit && !isNaN(value)) {
                switch (fromUnit) {
                    case VRS.Pressure.InHg:
                        switch (toUnit) {
                            case VRS.Pressure.Millibar:
                                result /= 0.0295301;
                                break;
                            case VRS.Pressure.MmHg:
                                result *= 25.4;
                                break;
                            default: throw 'Unknown pressure unit ' + toUnit;
                        }
                        break;
                    case VRS.Pressure.Millibar:
                        switch (toUnit) {
                            case VRS.Pressure.InHg:
                                result *= 0.0295301;
                                break;
                            case VRS.Pressure.MmHg:
                                result *= 0.750061561303;
                                break;
                            default: throw 'Unknown pressure unit ' + toUnit;
                        }
                        break;
                    case VRS.Pressure.MmHg:
                        switch (toUnit) {
                            case VRS.Pressure.InHg:
                                result /= 25.4;
                                break;
                            case VRS.Pressure.Millibar:
                                result /= 0.750061561303;
                                break;
                            default: throw 'Unknown pressure unit ' + toUnit;
                        }
                        break;
                    default:
                        throw 'Unknown pressure unit ' + fromUnit;
                }
            }
            return result;
        };
        UnitConverter.prototype.pressureUnitAbbreviation = function (unit) {
            switch (unit) {
                case VRS.Pressure.InHg: return VRS.$$.InHgAbbreviation;
                case VRS.Pressure.Millibar: return VRS.$$.MillibarAbbreviation;
                case VRS.Pressure.MmHg: return VRS.$$.MmHgAbbreviation;
                default: throw 'Unknown pressure unit ' + unit;
            }
        };
        UnitConverter.prototype.convertVerticalSpeed = function (verticalSpeed, fromUnit, toUnit, perSecond) {
            var result = verticalSpeed;
            if (result !== undefined) {
                if (fromUnit !== toUnit)
                    result = this.convertHeight(result, fromUnit, toUnit);
                if (perSecond)
                    result = Math.round(result / 60);
            }
            return result;
        };
        UnitConverter.prototype.getPixelsOrPercent = function (value) {
            var valueAsString = String(value);
            var result = {
                value: parseInt(valueAsString),
                isPercent: VRS.stringUtility.endsWith(valueAsString, '%', false)
            };
            if (result.isPercent) {
                result.value /= 100;
            }
            return result;
        };
        return UnitConverter;
    }());
    VRS.UnitConverter = UnitConverter;
    var ValueText = (function () {
        function ValueText(settings) {
            this._Settings = settings;
        }
        ValueText.prototype.getValue = function () {
            return this._Settings.value;
        };
        ValueText.prototype.setValue = function (value) {
            this._Settings.value = value;
        };
        ValueText.prototype.getSelected = function () {
            return this._Settings.selected || false;
        };
        ValueText.prototype.setSelected = function (value) {
            this._Settings.selected = value;
        };
        ValueText.prototype.getText = function () {
            return this._Settings.text || VRS.globalisation.getText(this._Settings.textKey);
        };
        return ValueText;
    }());
    VRS.ValueText = ValueText;
    VRS.arrayHelper = new VRS.ArrayHelper();
    VRS.browserHelper = new VRS.BrowserHelper();
    VRS.colourHelper = new VRS.ColourHelper();
    VRS.dateHelper = new VRS.DateHelper();
    VRS.domHelper = new VRS.DomHelper();
    VRS.enumHelper = new VRS.EnumHelper();
    VRS.greatCircle = new VRS.GreatCircle();
    VRS.jsonHelper = new VRS.JsonHelper();
    VRS.objectHelper = new VRS.ObjectHelper();
    VRS.pageHelper = new VRS.PageHelper();
    VRS.timeHelper = new VRS.TimeHelper();
    VRS.unitConverter = new VRS.UnitConverter();
})(VRS || (VRS = {}));
//# sourceMappingURL=utility.js.map