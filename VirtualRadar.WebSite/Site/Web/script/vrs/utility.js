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
 */
/**
 * @fileoverview General utility methods.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region ArrayHelper
    /**
     * Helper methods for dealing with arrays.
     * @constructor
     */
    VRS.ArrayHelper = function()
    {
        var that = this;

        //region -- except
        /**
         * Returns a new array containing all of the elements in array that are not also in exceptArray. This could get
         * very expensive as it involves a loop within a loop.
         * @param {Array}                               array
         * @param {Array}                               exceptArray
         * @param {function(object, object):boolean}   [compareCallback]
         * @returns {Array}
         */
        this.except = function(array, exceptArray, compareCallback)
        {
            var result = [];

            var arrayLength = array.length;
            var exceptArrayLength = exceptArray.length;
            for(var i = 0;i < arrayLength;++i) {
                var arrayValue = array[i];
                var existsInExcept = false;
                for(var c = 0;c < exceptArrayLength;++c) {
                    var exceptValue = exceptArray[c];
                    if(compareCallback ? compareCallback(arrayValue, exceptValue) : exceptValue === arrayValue) {
                        existsInExcept = true;
                        break;
                    }
                }
                if(!existsInExcept) result.push(arrayValue);
            }

            return result;
        };
        //endregion

        //region -- filter
        /**
         * Calls the allowItem callback with each item in the array. Returns the array formed from those items for which
         * the callback returned true.
         * @param {Array} array
         * @param {function(*):bool} allowItem
         * @returns {Array}
         */
        this.filter = function(array, allowItem)
        {
            var result = [];

            var length = array.length;
            for(var i = 0;i < length;++i) {
                var item = array[i];
                if(allowItem(item)) result.push(item);
            }

            return result;
        };
        //endregion

        //region -- findFirst
        /**
         * Returns the first item for which the matchesCallback returns true or the noMatchesValue if nothing matches.
         * @param {Array}               array               The array to test.
         * @param {function(*):boolean} matchesCallback     Passed each element in the array, returns true if the element matches.
         * @param {*=}                  noMatchesValue      The value to return if there are no matches. Defaults to undefined.
         * @returns {*}
         */
        this.findFirst = function(array, matchesCallback, noMatchesValue)
        {
            var index = that.indexOfMatch(array, matchesCallback);
            return index === -1 ? noMatchesValue : array[index];
        };
        //endregion

        //region -- indexOf, indexOfMatch
        /**
         * Returns the index of the first element that is the same as the value passed across or -1 if the value is not present in the array.
         * @param {Array}               array
         * @param {*}                   value
         * @param {number}              fromIndex
         * @returns {number}
         */
        this.indexOf = function(array, value, fromIndex)
        {
            var result = -1;
            if(array) {
                if(fromIndex === undefined) fromIndex = 0;
                var length = array.length;
                for(var i = fromIndex;i < length;++i) {
                    if(array[i] === value) {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        };

        /**
         * Returns the index of the first element for which matchesCallback returns true or -1 if no element matches.
         * @param {Array}               array
         * @param {function(*):bool}    matchesCallback
         * @param {number}             [fromIndex]
         * @returns {number}
         */
        this.indexOfMatch = function(array, matchesCallback, fromIndex)
        {
            var result = -1;

            if(array) {
                if(fromIndex === undefined) fromIndex = 0;
                var length = array.length;
                for(var i = fromIndex;i < length;++i) {
                    if(matchesCallback(array[i])) {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        };
        //endregion

        //region -- isArray
        /**
         * Returns true if the object passed across is an array.
         * @param {*} obj
         * @returns {boolean}
         */
        this.isArray = function(obj)
        {
            return !!(obj && Object.prototype.toString.call(obj) === '[object Array]');
        };
        //endregion

        //region -- normaliseOptionsArray
        /**
         * Modifies an array, usually loaded from persistent storage, so that it is optionally the same length as the
         * 'default' array and its entries are all known to be valid.
         * @param {Array}              [defaultArray]       The array whose contents will be used to pad out missing entries in optionsArray. If not supplied then no padding or trimming is performed.
         * @param {Array}               optionsArray        The array that is being normalised.
         * @param {function(*):bool}    isValidCallback     A method that will be called for each entry in optionsArray to ensure that the entry is valid.
         */
        this.normaliseOptionsArray = function(defaultArray, optionsArray, isValidCallback)
        {
            var i;
            var desiredLength = defaultArray ? defaultArray.length : -1;

            for(i = 0;i < optionsArray.length;++i) {
                if(!isValidCallback(optionsArray[i])) optionsArray.splice(i--, 1);
            }

            for(i = optionsArray.length;i < desiredLength;++i) {
                optionsArray.push(defaultArray[i]);
            }

            if(desiredLength !== -1) {
                var extraneousEntries = optionsArray.length - desiredLength;
                if(extraneousEntries > 0) optionsArray.splice(optionsArray.length - extraneousEntries, extraneousEntries);
            }
        };
        //endregion

        //region -- select
        /**
         * Calls the selectCallback for each item in the array and returns the array formed from the values returned by
         * the callback.
         * @param {Array} array
         * @param {function(*):*} selectCallback
         * @returns {Array}
         */
        this.select = function(array, selectCallback)
        {
            var result = [];

            var length = array.length;
            for(var i = 0;i < length;++i) {
                result.push(selectCallback(array[i]));
            }

            return result;
        };
        //endregion
    };
    //endregion

    //region BrowserHelper
    /**
     * A helper object that can tell us some things about the browser.
     * @constructor
     */
    VRS.BrowserHelper = function()
    {
        var that = this;

        //region -- Properties
        var _ForceFrame;
        var _ForceFrameHasBeenRead;
        this.getForceFrame = function()
        {
            if(!_ForceFrameHasBeenRead) {
                if(purl) {
                    _ForceFrame = $.url().param('forceFrame');
                }
                _ForceFrameHasBeenRead = true;
            }
            return _ForceFrame;
        };

        /** @type {boolean=} */
        var _IsProbablyIPad;
        /**
         * Returns true if the browser is probably running on an iPad. Actual iPads should be automatically detected but
         * you can force it for any page by passing 'isIpad=1' on the query string (case insensitive, any non-zero value will do).
         * @returns {boolean}
         */
        this.isProbablyIPad = function()
        {
            if(_IsProbablyIPad === undefined) {
                if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isIpad')) _IsProbablyIPad = true;
                }
                if(_IsProbablyIPad === undefined) _IsProbablyIPad = navigator.userAgent.indexOf('iPad') !== -1;
            }
            return _IsProbablyIPad;
        };

        /** @type {boolean=} */
        var _IsProbablyIPhone;
        /**
         * Returns true if the browser is probably running on an iPhone. Actual iPhones should be automatically detected
         * but you can force it by passing 'isIphone=1' on the query string (case insensitive, any non-zero value will work).
         * @returns {boolean}
         */
        this.isProbablyIPhone = function()
        {
            if(_IsProbablyIPhone === undefined) {
                if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isIphone')) _IsProbablyIPhone = true;
                }
                if(_IsProbablyIPhone === undefined) _IsProbablyIPhone = navigator.userAgent.indexOf('iPhone') !== -1;
            }
            return _IsProbablyIPhone;
        };

        /** @type {boolean=} */
        var _IsProbablyAndroid;
        /**
         * Returns true if the browser is probably running on an Android device.
         * @returns {boolean=}
         */
        this.isProbablyAndroid = function()
        {
            if(_IsProbablyAndroid === undefined) _IsProbablyAndroid = navigator.userAgent.indexOf('; Android ') !== -1;
            return _IsProbablyAndroid;
        };

        /** @type {boolean=} */
        var _IsProbablyAndroidPhone;
        /**
         * Returns true if the browser is probably running on an Android phone.
         * @returns {boolean=}
         */
        this.isProbablyAndroidPhone = function()
        {
            if(_IsProbablyAndroidPhone === undefined) _IsProbablyAndroidPhone = that.isProbablyAndroid() && navigator.userAgent.indexOf(' Mobile') !== -1;
            return _IsProbablyAndroidPhone;
        };

        /** @type {boolean=} */
        var _IsProbablyAndroidTablet;
        /**
         * Returns true if the browser is probably running on an Android tablet.
         * @returns {boolean=}
         */
        this.isProbablyAndroidTablet = function()
        {
            if(_IsProbablyAndroidTablet === undefined) _IsProbablyAndroidTablet = that.isProbablyAndroid() && !that.isProbablyAndroidPhone();
            return _IsProbablyAndroidTablet;
        };

        /** @type {boolean=} */
        var _IsProbablyWindowsPhone;
        /**
         * Returns true if the browser is probably running on a Windows phone.
         * @returns {boolean=}
         */
        this.isProbablyWindowsPhone = function()
        {
            if(_IsProbablyWindowsPhone === undefined) _IsProbablyWindowsPhone = navigator.userAgent.indexOf('; Windows Phone ') !== -1;
            return _IsProbablyWindowsPhone;
        };

        /*
         I'd like some code here to guess if it's a Windows tablet or not, but the UA doesn't say. If a user visits the
         mobile site with a Windows tablet they'll need to use the query string.
         */

        /** @type {boolean=} */
        var _IsProbablyTablet;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Returns true if the browser is probably running on a tablet. iPads and Android tablets are automatically
         * detected, for other tablets you need to pass 'isTablet=1' on the query string (case insensitive, any non-zero
         * value will do).
         * @returns {boolean=}
         */
        this.isProbablyTablet = function()
        {
            if(_IsProbablyTablet === undefined) {
                if(that.isProbablyIPad() || that.isProbablyAndroidTablet()) _IsProbablyTablet = true;
                else if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isTablet')) _IsProbablyTablet = true;
                } else _IsProbablyTablet = false;
            }
            return _IsProbablyTablet;
        };

        /** @type {boolean=} */
        var _IsProbablyPhone;
        /**
         * Returns true if the browser is probably running on a phone. iPhones, Android and Windows phones are automatically
         * detected, for other phones you need to pass 'isPhone=1' on the query string (case insensitive, any non-zero
         * value will do).
         * @returns {boolean=}
         */
        this.isProbablyPhone = function()
        {
            if(_IsProbablyPhone === undefined) {
                if(that.isProbablyIPhone() || that.isProbablyAndroidPhone() || that.isProbablyWindowsPhone()) _IsProbablyPhone = true;
                else if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isPhone')) _IsProbablyPhone = true;
                } else _IsProbablyPhone = false;
            }
            return _IsProbablyPhone;
        };

        var _IsHighDpi;
        /**
         * Returns true if the browser is probably running on a high DPI display.
         * @returns {*}
         */
        this.isHighDpi = function()
        {
            if(_IsHighDpi === undefined) {
                _IsHighDpi = false;
                if(window.devicePixelRatio > 1) _IsHighDpi = true;
                else {
                    var query = '(-webkit-min-device-pixel-ratio: 1.5), (min--moz-device-pixel-ratio: 1.5), (-o-min-device-pixel-ratio: 3/2), (min-resolution: 1.5dppx)';
                    if(window.matchMedia && window.matchMedia(query).matches) _IsHighDpi = true;
                }
            }

            return _IsHighDpi;
        };

        var _NotOnline;
        this.notOnline = function()
        {
            if(_NotOnline === undefined) _NotOnline = !purl ? false : $.url().param('notOnline') === '1';
            return _NotOnline;
        }
        //endregion

        //region -- formUrl, formVrsPageUrl
        /**
         * Returns a URL with a parameters object appended as a query string.
         * @param {string}   url        The URL to base the result on.
         * @param {object}  [params]    An optional parameters object - see jQuery's param function for more details.
         * @param {boolean} [recursive] Whether to perform a recursive copy when forming the query string.
         */
        this.formUrl = function(url, params, recursive)
        {
            var result = url;
            if(params) {
                var queryString = $.param(params, !recursive);
                if(queryString) result += '?' + queryString;
            }

            return result;
        };

        /**
         * Returns a URL to another page in VRS with a parameters object appended as a query string.
         * @param {string}   url        The URL to base the result on.
         * @param {object}  [params]    An optional parameters object - see jQuery's param function for more details.
         * @param {boolean} [recursive] Whether to perform a recursive copy when forming the query string.
         */
        this.formVrsPageUrl = function(url, params, recursive)
        {
            params = $.extend({
                notOnline: that.notOnline() ? '1' : '0',
                forceFrame: that.getForceFrame()
            }, params);
            return that.formUrl(url, params, recursive);
        };
        //endregion

        //region getVrsPageTarget
        /**
         * Gets the target for a link to a VRS page. This is normally the string passed in, but if the site is running
         * within an iframe then targets out of the iframe will point directly at the server so this, in conjunction
         * with the forceFrame query string parameter, keep everything in the same iframe.
         * @param target        The target to use if one isn't being forced.
         * @returns {string}    The target that the link should use.
         */
        this.getVrsPageTarget = function(target)
        {
            return that.getForceFrame() || target;
        };
        //endregion
    };
    //endregion

    //region ColourHelper
    /**
     * Methods to help with handling colours.
     * @constructor
     */
    VRS.ColourHelper = function()
    {
        var that = this;

        /** @returns {VRS_COLOUR} */ this.getWhite = function() { return { r: 255, g: 255, b: 255 }; };
        /** @returns {VRS_COLOUR} */ this.getRed   = function() { return { r: 255, g: 0,   b: 0   }; };
        /** @returns {VRS_COLOUR} */ this.getGreen = function() { return { r: 0,   g: 255, b: 0   }; };
        /** @returns {VRS_COLOUR} */ this.getBlue  = function() { return { r: 255, g: 0,   b: 255 }; };
        /** @returns {VRS_COLOUR} */ this.getBlack = function() { return { r: 0,   g: 0,   b: 0   }; };

        /**
         * Given a numeric value, a low value and a high value this converts the value to a colour on a colour wheel.
         *
         * The colour wheel represents all possible HSV values where S and V are 100% and only the hue changes. The hue
         * starts at 100% green and red, red falls to 0%, blue rises to 100%, green falls to 0%, red rises to 100%,
         * blue falls to 100%. The last phase, where green rises to 100% (bringing us to the start of the wheel) is not
         * performed as it would lead to confusion between the start and the end of the wheel - the intended use of this
         * method is for coloured trails where we want no confusion between low and high values.
         * @param {Number=} value               The value to convert.
         * @param {Number}  lowValue            The low value. Anything below this is white.
         * @param {Number}  highValue           The high value. Anything above this is red.
         * @param {boolean} invalidIsBelowLow   True if invalid values are treated as though they are below low - i.e., they return white. False if invalid values return a null colour. Defaults to true.
         * @param {boolean} stretchLowerValues  True if the first 5th of the range is stretched so that the change in colour over these values is more striking.
         * @returns {VRS_COLOUR}
         */
        this.getColourWheelScale = function(value, lowValue, highValue, invalidIsBelowLow, stretchLowerValues)
        {
            if(invalidIsBelowLow === undefined) invalidIsBelowLow = true;
            if(stretchLowerValues === undefined) stretchLowerValues = true;
            var result = null;

            if(value === undefined || isNaN(value)) {
                if(invalidIsBelowLow) result = that.getWhite();
            } else if(value <= lowValue) result = that.getWhite();
            else if(value >= highValue) result = that.getRed();
            else {
                value -= lowValue;
                var range = (highValue - lowValue) + 1;
                var fifthRange = range / 5;
                if(!fifthRange) result = that.getWhite();
                else {
                    if(stretchLowerValues) {
                        if(value < fifthRange) value *= 2;
                        else {
                            var newBase = fifthRange * 2;
                            value = Math.round((((value - fifthRange) / (range - fifthRange)) * (range - newBase)) + newBase);
                        }
                    }
                    var fifth = Math.floor(value / fifthRange);
                    var remainder = (value - (fifth * fifthRange)) / fifthRange;

                    var r, g, b;
                    switch(fifth) {
                        case 0:     b = 0; g = 255; r = fallingComponent(remainder); break;
                        case 1:     r = 0; g = 255; b = risingComponent(remainder); break;
                        case 2:     r = 0; b = 255; g = fallingComponent(remainder); break;
                        case 3:     g = 0; b = 255; r = risingComponent(remainder); break;
                        case 4:     g = 0; r = 255; b = fallingComponent(remainder); break;
                    }

                    result = { r: r, g: g, b: b };
                }
            }

            return result;
        };

        function risingComponent(proportion)
        {
            return Math.floor(255 * proportion);
        }

        function fallingComponent(proportion)
        {
            return 255 - Math.floor(255 * proportion);
        }

        /**
         * Takes a colour object and returns a CSS colour string.
         * @param {VRS_COLOUR} colour
         * @returns {string}
         */
        this.colourToCssString = function(colour)
        {
            var result = null;
            if(colour) {
                var toHex = function(value) { var hex = value.toString(16); return hex.length === 1 ? '0' + hex : hex; };
                result = '#' + toHex(colour.r) + toHex(colour.g) + toHex(colour.b);
            }

            return result;
        };
    };
    //endregion

    //region DateHelper
    /**
     * A collection of methods that help when dealing with dates.
     * @constructor
     */
    VRS.DateHelper = function()
    {
        //region -- Fields
        var that = this;
        var _TicksInDay = 1000 * 60 * 60 * 24;
        //endregion

        //region -- getDatePortion, getDateTicks, getTimePortion, getTimeTicks
        /**
         * Returns the date portion of the date/time passed across.
         * @param {Date} date
         * @returns {Date}
         */
        this.getDatePortion = function(date)
        {
            return new Date(that.getDateTicks(date));
        };

        /**
         * Returns the number of ticks that represent the date portion of the date/time passed across.
         * @param {Date} date
         * @returns {Number}
         */
        this.getDateTicks = function(date)
        {
            return Math.floor(date.getTime() / _TicksInDay) * _TicksInDay;
        };

        /**
         * Returns a new date consisting only of the time portion of the date/time passed across.
         * @param {Date} date
         * @returns {Date}
         */
        this.getTimePortion = function(date)
        {
            return new Date(that.getTimeTicks(date));
        };

        /**
         * Returns the number of ticks in the time portion of the date/time passed across.
         * @param {Date} date
         * @returns {number}
         */
        this.getTimeTicks = function(date)
        {
            return date.getTime() % _TicksInDay;
        };
        //endregion

        //region -- parse
        /**
         * Parses text into a date. The text can either be in ISO format (yyyy-mm-dd) or it can be a number of days
         * offset from today (e.g. +0 = today, -1 = yesterday, +1 = tomorrow). Returns undefined if the text cannot be
         * parsed into a date.
         * @param {string} text
         * @returns {Date=}
         */
        this.parse = function(text)
        {
            /** @type {Date=} */
            var result;

            if(text) {
                var offsetMatches = text.match(/[+\-]?\d+/g);
                if(offsetMatches && offsetMatches.length === 1 && offsetMatches[0] === text) {
                    var offset = parseInt(text);
                    if(!isNaN(offset)) result = offset === 0 ? new Date() : new Date(new Date().getTime() + (offset * _TicksInDay));
                } else {
                    var ticks = Date.parse(text);
                    if(!isNaN(ticks)) result = new Date(ticks);
                }
            }

            return result;
        };
        //endregion

        //region -- toIsoFormatString
        /**
         * Formats the date passed across as a string in ISO format.
         * @param {Date}        date                The date to format.
         * @param {boolean=}    suppressTime        If true then only the date portion of the date is shown.
         * @param {boolean=}    suppressTimeZone    If true then the time zone is not shown. Ignored if suppressTime is true.
         * @returns {string}
         */
        this.toIsoFormatString = function(date, suppressTime, suppressTimeZone)
        {
            var result = '';

            if(date) {
                result = VRS.stringUtility.formatNumber(date.getFullYear(), 4) + '-' +
                         VRS.stringUtility.formatNumber(date.getMonth() + 1, 2) + '-' +
                         VRS.stringUtility.formatNumber(date.getDate(), 2);
                if(!suppressTime) {
                    if(!suppressTimeZone) result += 'T';
                    result += VRS.stringUtility.formatNumber(date.getHours(), 2) + ':' +
                              VRS.stringUtility.formatNumber(date.getMinutes(), 2) + ':' +
                              VRS.stringUtility.formatNumber(date.getSeconds(), 2);
                    if(!suppressTimeZone) result += 'Z';
                }
            }

            return result;
        };
        //endregion
    };
    //endregion

    //region DelayedTrace
    /**
     * An object that can help with displaying traces on mobile browsers that don't have debug facilities.
     * @param {string} title                    The title of the message box to show after the timer has expired.
     * @param {Number} delayMilliseconds        The number of milliseconds to wait before the trace is displayed.
     * @constructor
     */
    VRS.DelayedTrace = function(title, delayMilliseconds)
    {
        /**
         * The trace lines that will be shown after the timer expires.
         * @type {Array}
         * @private
         */
        var _Lines = [];

        /**
         * Adds a line to the trace message.
         * @param {string} message
         */
        this.add = function(message)
        {
            _Lines.push(message || '');
        };

        setTimeout(function() {
            var message = '';
            $.each(_Lines, function(/** number */ idx, /** string */ line) {
                if(message.length) message += '\n';
                message += line;
            });
            VRS.pageHelper.showMessageBox(title, message);
        }, delayMilliseconds);
    };
    //endregion

    //region DomHelper
    /**
     * An object that can help when dealing directly with DOM elements.
     * @constructor
     */
    VRS.DomHelper = function()
    {
        var that = this;

        //region -- setAttribute, removeAttribute
        /**
         * Sets a value for the attribute passed across.
         * @param {Element} element
         * @param {string} name
         * @param {string} value
         */
        this.setAttribute = function(element, name, value)
        {
            element.setAttribute(name, value);
        };

        /**
         * Removes an attribute from the element. The attribute does not have to already be present on the element.
         * @param {Element} element
         * @param {string} name
         */
        this.removeAttribute = function(element, name)
        {
            element.removeAttribute(name);
        };
        //endregion

        //region -- setClass, addClasses, removeClasses, getClasses, setClasses
        /**
         * Sets the class on the element passed across, obliterating any previous class set on the element.
         * @param {HTMLElement} element
         * @param {string} className
         */
        this.setClass = function(element, className)
        {
            element.className = className;
        };

        /**
         * Adds one or more class names to the element passed across. If a class is already on the element then it
         * is ignored.
         * @param {HTMLElement} element
         * @param {string[]} addClasses
         */
        this.addClasses = function(element, addClasses)
        {
            if(addClasses.length) {
                var current = that.getClasses(element);
                var newClasses = [];
                var addLength = addClasses.length;
                var currentLength = current.length;
                for(var o = 0;o < addLength;++o) {
                    var addClass = addClasses[o];
                    for(var i = 0;i < currentLength;++i) {
                        if(current[i] === addClass) {
                            addClass = null;
                            break;
                        }
                    }
                    if(addClass) newClasses.push(addClass);
                }

                if(newClasses.length) {
                    current = current.concat(newClasses);
                    that.setClasses(element, current);
                }
            }
        };

        /**
         * Removes one or more class names from the element passed across. If a class is not already on the element then
         * it is ignored.
         * @param {HTMLElement} element
         * @param {string[]} removeClasses
         */
        this.removeClasses = function(element, removeClasses)
        {
            if(removeClasses.length) {
                var current = that.getClasses(element);
                if(current.length) {
                    var preserveClasses = [];
                    var removeLength = removeClasses.length;
                    var currentLength = current.length;
                    for(var o = 0;o < currentLength;++o) {
                        var currentClass = current[o];
                        for(var i = 0;i < removeLength;++i) {
                            if(removeClasses[i] === currentClass) {
                                currentClass = null;
                                break;
                            }
                        }
                        if(currentClass) preserveClasses.push(currentClass);
                    }

                    if(preserveClasses.length !== current.length) {
                        that.setClasses(element, preserveClasses);
                    }
                }
            }
        };

        /**
         * Returns an array of classes associated with the element.
         * @param {HTMLElement} element
         * @returns {Array.<String>}
         */
        this.getClasses = function(element)
        {
            var result = [];
            var classes = (element.className || '').split(' ');
            var length = classes.length;
            for(var i = 0;i < length;++i) {
                var name = classes[i];
                if(name) result.push(name);
            }

            return result;
        };

        /**
         * Sets the classes associated with the array. Does not check for duplicate classes.
         * @param {HTMLElement} element
         * @param {Array.<String>} classNames
         */
        this.setClasses = function(element, classNames)
        {
            that.setClass(element, classNames.join(' '));
        };
        //endregion
    };
    //endregion

    //region EnumHelper
    /**
     * Helper methods for dealing with objects that only contain fields with constant values.
     * @constructor
     */
    VRS.EnumHelper = function()
    {
        /**
         * Returns the property name of an enum value within its parent object.
         * @param {Object} enumObject The object that contains all of the enums as properties.
         * @param {String|Number} value The enum value that needs to be looked up.
         * @returns {String|undefined} The name of the property that matches the enum value or undefined if no such property exists.
         */
        this.getEnumName = function(enumObject, value)
        {
            var result = undefined;

            for(var property in enumObject) {
                //noinspection JSUnfilteredForInLoop
                if(enumObject[property] === value) {
                    result = property;
                    break;
                }
            }

            return result;
        };

        /**
         * Returns all of the enum values as a list.
         * @param {Object} enumObject
         * @returns {Array}
         */
        this.getEnumValues = function(enumObject)
        {
            var result = [];

            for(var property in enumObject) {
                //noinspection JSUnfilteredForInLoop
                result.push(enumObject[property]);
            }

            return result;
        }
    };
    //endregion

    //region GreatCircle
    /**
     * A collection of Great Circle math functions.
     * @constructor
     */
    VRS.GreatCircle = function()
    {
        var that = this;

        /**
         * Returns true if the lat and lng are within the bounds described by tlLat/tlLng and brLat/brLng.
         * @param {Number} lat The latitude to test.
         * @param {Number} lng The longitude to test.
         * @param {VRS_BOUNDS} bounds The latitudes and longitudes of the bounding box.
         * @returns {boolean} True if the coordinate is within the boundaries, false otherwise.
         */
        this.isLatLngInBounds = function(lat, lng, bounds)
        {
            var result = !isNaN(lat) && !isNaN(lng);
            if(result) {
                result = bounds.tlLat >= lat && bounds.brLat <= lat;
                if(result) {
                    if(bounds.tlLng >= 0 && bounds.brLng < 0) {
                        result = lng >= 0 ? bounds.tlLng <= lng : bounds.brLng >= lng;
                    } else {
                        result = bounds.tlLng <= lng && bounds.brLng >= lng;
                    }
                }
            }

            return result;
        };

        /**
         * Takes two lat/lngs and returns a bounds object that encompases them both. If either point is missing then
         * a bounds encompassing just the remaining point is returned. If neither point is supplied then null is returned.
         * @param {VRS_LAT_LNG} point1
         * @param {VRS_LAT_LNG} point2
         * @returns {VRS_BOUNDS}
         */
        this.arrangeTwoPointsIntoBounds = function(point1, point2)
        {
            var result = null;

            if(point1 || point2) {
                if(point1 && !point2)       result = { tlLat: point1.lat, brLat: point1.lat, tlLng: point1.lng, brLng: point1.lng };
                else if(!point1 && point2)  result = { tlLat: point2.lat, brLat: point2.lat, tlLng: point2.lng, brLng: point2.lng };
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
    };
    //endregion

    //region JsonHelper
    /**
     * A collection of functions that help when dealing with JSON.
     * @constructor
     */
    VRS.JsonHelper = function()
    {
        /**
         * Converts the Microsoft format date constructors in JSON into actual date constructors. Note that the resulting
         * JSON is no longer valid JSON - but without it the dates will be evaluated as strings.
         * @param {string} json
         * @returns {string}
         */
        this.convertMicrosoftDates = function(json)
        {
            return json.replace(/\"\\\/Date\(([\d\+\-]+)\)\\\/\"/g, 'new Date($1)');
        };
    };
    //endregion

    //region ObjectHelper
    /**
     * A utility class that holds methods that help when working with objects.
     * @constructor
     */
    VRS.ObjectHelper = function()
    {
        /**
         * Creates a prototype object containing the members of a base class. The constructor of the prototype object
         * must remember to call the base's constructor in its constructor.
         * @param {object} base
         * @returns {object}
         */
        this.subclassOf = function(base)
        {
            _subclassOf.prototype = base.prototype;
            return new _subclassOf();
        };
        function _subclassOf() {}
    };
    //endregion

    //region PageHelper
    /**
     * Common full-page HTML operations.
     * @constructor
     */
    VRS.PageHelper = function()
    {
        var that = this;

        /**
         * Shows or hides a full-page wait animation that prevents the user from interacting with the page.
         * @param {boolean} onOff
         */
        this.showModalWaitAnimation = function(onOff)
        {
            if(onOff) $('body').addClass('wait');
            else      $('body').removeClass('wait');
        };

        /**
         * Displays a simple message-box to the user. Attempts to use jQueryUI dialog but if that isn't present then
         * it falls back onto the browser's alert method. Note that this may or may not block, depending upon whether
         * it had to fall back to alert (the method blocks) or whether it could use jQueryUI (the method does not block).
         * If the dialog is used then it shows a modal message box so the UI can't be accessed while it's running.
         * @param {string} title
         * @param {string} message
         */
        this.showMessageBox = function(title, message)
        {
            var element = $('<div/>');
            if(!element.dialog) alert(title === undefined ? message : title + ': ' + message);
            else {
                var htmlMessage = VRS.stringUtility.htmlEscape(message).replace(/\n/g, '<br/>');
                element
                    .appendTo($('body'))
                    .append($('<p/>').html(htmlMessage))
                    .dialog({
                        modal: true,
                        title: title,
                        close: function() {
                            element.remove();
                        }
                    });
            }
        };

        //region -- addIndentLog, removeIndentLog, indentLog
        var _Indent = 0;
        /**
         * Adds an opening indented log entry.
         * @param message
         * @returns {Date}
         */
        this.addIndentLog = function(message)
        {
            var result = that.indentLog(message);
            ++_Indent;
            return result;
        };

        /**
         * Adds a closing indented log entry.
         * @param message
         * @param [started]
         */
        this.removeIndentLog = function(message, started)
        {
            _Indent = Math.max(0, _Indent - 1);
            that.indentLog(message, started);
        };

        /**
         * Adds a message at the current indent level.
         * @param message
         * @param [started]
         * @returns {Date}
         */
        this.indentLog = function(message, started)
        {
            var now = new Date();
            if(started) message = message + ' took ' + (now - started) + ' ticks';
            var indent = VRS.stringUtility.repeatedSequence(' ', _Indent * 4);
            console.log(
                '[' +
                    VRS.stringUtility.formatNumber(now.getHours(), '00') + ':' +
                    VRS.stringUtility.formatNumber(now.getMinutes(), '00') + ':' +
                    VRS.stringUtility.formatNumber(now.getSeconds(), '00') + '.' +
                    VRS.stringUtility.formatNumber(now.getMilliseconds(), '000') + '] ' +
                    indent +
                    message
            );
            return now;
        };
        //endregion
    };
    //endregion

    //region TimeHelper
    /**
     * Helper methods for dealing with time spans.
     * @constructor
     */
    VRS.TimeHelper = function()
    {
        var that = this;

        /**
         * Returns an object reporting the number of hours, minutes and seconds that a number of seconds describes.
         * @param {Number} seconds
         * @returns {VRS_TIME} The number of hours, minutes and seconds.
         */
        this.secondsToHoursMinutesSeconds = function(seconds)
        {
            var hours = Math.floor(seconds / 3600);
            seconds -= (hours * 3600);
            var minutes = Math.floor(seconds / 60);
            seconds -= (minutes * 60);

            return { hours: hours, minutes: minutes, seconds: seconds };
        };

        /**
         * Returns an object reporting the number of hours, minutes and seconds that a number of ticks (milliseconds) describes.
         * @param {Number} ticks
         * @returns {VRS_TIME}
         */
        this.ticksToHoursMinutesSeconds = function(ticks)
        {
            return that.secondsToHoursMinutesSeconds(Math.floor(ticks / 1000));
        };
    };
    //endregion

    //region UnitConverter
    /**
     * A collection of functions that convert values from one unit to another.
     * @constructor
     */
    VRS.UnitConverter = function()
    {
        var that = this;

        //region -- convertDistance, distanceUnitAbbreviation
        /**
         * Converts distances from one unit to another.
         * @param {number} value The distance to convert.
         * @param {string} fromUnit A VRS.Distance unit to convert from.
         * @param {string} toUnit A VRS.Distance unit to convert to.
         * @returns {number} The converted value.
         */
        this.convertDistance = function(value, fromUnit, toUnit)
        {
            var result = value;

            if(fromUnit !== toUnit && !isNaN(value)) {
                switch(fromUnit) {
                    case VRS.Distance.Kilometre:
                        switch(toUnit) {
                            case VRS.Distance.NauticalMile: result *= 0.539956803; break;
                            case VRS.Distance.StatuteMile:  result *= 0.621371192; break;
                            default:                        throw 'Unknown distance unit ' + toUnit;
                        }
                        break;
                    case VRS.Distance.NauticalMile:
                        switch(toUnit) {
                            case VRS.Distance.Kilometre:    result *= 1.852; break;
                            case VRS.Distance.StatuteMile:  result *= 1.15078; break;
                            default:                        throw 'Unknown distance unit ' + toUnit;
                        }
                        break;
                    case VRS.Distance.StatuteMile:
                        switch(toUnit) {
                            case VRS.Distance.Kilometre:    result *= 1.609344; break;
                            case VRS.Distance.NauticalMile: result *= 0.868976; break;
                            default:                        throw 'Unknown distance unit ' + toUnit;
                        }
                        break;
                    default:
                        throw 'Unknown distance unit ' + fromUnit;
                }
            }

            return result;
        };

        /**
         * Returns the translated abbreviation for a VRS.Distance unit.
         * @param {string} unit The VRS.Distance unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        this.distanceUnitAbbreviation = function(unit)
        {
            switch(unit) {
                case VRS.Distance.Kilometre:    return VRS.$$.KilometreAbbreviation;
                case VRS.Distance.NauticalMile: return VRS.$$.NauticalMileAbbreviation;
                case VRS.Distance.StatuteMile:  return VRS.$$.StatuteMileAbbreviation;
                default:                        throw 'Unknown distance unit ' + unit;
            }
        };
        //endregion

        //region -- convertHeight
        /**
         * Converts heights from one unit to another.
         * @param {number} value The height to convert.
         * @param {string} fromUnit A VRS.Height unit to convert from.
         * @param {string} toUnit A VRS.Height unit to convert to.
         * @returns {number} The converted value.
         */
        this.convertHeight = function(value, fromUnit, toUnit)
        {
            var result = value;

            if(fromUnit !== toUnit && !isNaN(value)) {
                switch(fromUnit) {
                    case VRS.Height.Feet:
                        switch(toUnit) {
                            case VRS.Height.Metre:  result *= 0.3048; break;
                            default:                throw 'Unknown height unit ' + toUnit;
                        }
                        break;
                    case VRS.Height.Metre:
                        switch(toUnit) {
                            case VRS.Height.Feet:   result *= 3.2808399; break;
                            default:                throw 'Unknown height unit ' + toUnit;
                        }
                        break;
                    default:
                        throw 'Unknown height unit ' + fromUnit;
                }
            }

            return result;
        };

        /**
         * Returns the translated abbreviation for a VRS.Height unit.
         * @param {string} unit The VRS.Height unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        this.heightUnitAbbreviation = function(unit)
        {
            switch(unit) {
                case VRS.Height.Feet:           return VRS.$$.FeetAbbreviation;
                case VRS.Height.Metre:          return VRS.$$.MetreAbbreviation;
                default:                        throw 'Unknown height unit ' + unit;
            }
        };

        /**
         * Returns the translated abbreviation for a VRS.Height unit over time.
         * @param {string} unit The VRS.Height unit to get an abbreviation for.
         * @param {boolean} perSecond True if it is height over seconds, false if it is height over minutes.
         * @returns {string} The translated abbreviation.
         */
        this.heightUnitOverTimeAbbreviation = function(unit, perSecond)
        {
            if(perSecond) {
                switch(unit) {
                    case VRS.Height.Feet:       return VRS.$$.FeetPerSecondAbbreviation;
                    case VRS.Height.Metre:      return VRS.$$.MetrePerSecondAbbreviation;
                    default:                    throw 'Unknown height unit ' + unit;
                }
            } else {
                switch(unit) {
                    case VRS.Height.Feet:       return VRS.$$.FeetPerMinuteAbbreviation;
                    case VRS.Height.Metre:      return VRS.$$.MetrePerMinuteAbbreviation;
                    default:                    throw 'Unknown height unit ' + unit;
                }
            }
        };
        //endregion

        //region -- convertSpeed
        /**
         * Converts speeds from one unit to another.
         * @param {number} value The speed to convert.
         * @param {string} fromUnit A VRS.Speed unit to convert from.
         * @param {string} toUnit A VRS.Speed unit to convert to.
         * @returns {number} The converted value.
         */
        this.convertSpeed = function(value, fromUnit, toUnit)
        {
            var result = value;

            if(fromUnit !== toUnit && !isNaN(value)) {
                switch(fromUnit) {
                    case VRS.Speed.Knots:
                        switch(toUnit) {
                            case VRS.Speed.KilometresPerHour:   result *= 1.852; break;
                            case VRS.Speed.MilesPerHour:        result *= 1.15078; break;
                            default:                            throw 'Unknown speed unit ' + toUnit;
                        }
                        break;
                    case VRS.Speed.KilometresPerHour:
                        switch(toUnit) {
                            case VRS.Speed.Knots:               result *= 0.539957; break;
                            case VRS.Speed.MilesPerHour:        result *= 0.621371; break;
                            default:                            throw 'Unknown speed unit ' + toUnit;
                        }
                        break;
                    case VRS.Speed.MilesPerHour:
                        switch(toUnit) {
                            case VRS.Speed.KilometresPerHour:   result *= 1.60934; break;
                            case VRS.Speed.Knots:               result *= 0.868976; break;
                            default:                            throw 'Unknown speed unit ' + toUnit;
                        }
                        break;
                    default:
                        throw 'Unknown speed unit ' + fromUnit;
                }
            }

            return result;
        };

        /**
         * Returns the translated abbreviation for a VRS.Speed unit.
         * @param {string} unit The VRS.Speed unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        this.speedUnitAbbreviation = function(unit)
        {
            switch(unit) {
                case VRS.Speed.Knots:               return VRS.$$.KnotsAbbreviation;
                case VRS.Speed.KilometresPerHour:   return VRS.$$.KilometresPerHourAbbreviation;
                case VRS.Speed.MilesPerHour:        return VRS.$$.MilesPerHourAbbreviation;
                default:                            throw 'Unknown speed unit ' + unit;
            }
        };
        //endregion

        //region convertVerticalSpeed
        /**
         * Converts a vertical speed from one unit to another.
         * @param {number}      verticalSpeed   The vertical speed in x units per minute to convert.
         * @param {VRS.Height}  fromUnit        The units that the vertical speed is expressed in.
         * @param {VRS.Height}  toUnit          The units to convert to.
         * @param {boolean}     perSecond       True if the vertical speed should be converted to y units per second.
         * @returns {*}
         */
        this.convertVerticalSpeed = function(verticalSpeed, fromUnit, toUnit, perSecond)
        {
            var result = verticalSpeed;
            if(result !== undefined) {
                if(fromUnit !== toUnit) result = that.convertHeight(result, fromUnit, toUnit);
                if(perSecond) result = Math.round(result / 60);
            }

            return result;
        };
        //endregion

        //region --getPixelsOrPercent
        /**
         * Accepts an integer number of pixels or a string ending with '%' and returns an
         * object describing whether the value is pixels or percent, and a number indicating what that value is.
         * Percents are divided by 100 before being returned.
         * @param {String|Number} value Either the integer percentage or a string ending with '%'.
         * @returns {VRS_VALUE_PERCENT}
         */
        this.getPixelsOrPercent = function(value)
        {
            var valueAsString = String(value);
            var result = {
                value: parseInt(valueAsString),
                isPercent: VRS.stringUtility.endsWith(valueAsString, '%', false)
            };
            if(result.isPercent) result.value /= 100;

            return result;
        };
        //endregion
    };
    //endregion

    //region ValueText
    /**
     * Associates a value with a text description.
     * @param {Object}                          settings            The settings object.
     * @param {*}                               settings.value      The value associated with the text.
     * @param {string}                         [settings.text]      The text associated with the value.
     * @param {string|(function():string)}     [settings.textKey]   Either an index into VRS.$$ or a function that returns translated text. Mandatory if text is unused.
     * @param {bool}                           [settings.selected]  True if the value is to be marked as the selected value in a list of values.
     * @constructor
     */
    VRS.ValueText = function(settings)
    {
        var that = this;

        /** @returns {*} */ this.getValue = function() { return settings.value; };
        this.setValue = function(/** * */ value) { settings.value = value; };

        /** @return {bool} */ this.getSelected = function() { return settings.selected || false; };
        this.setSelected = function(/** bool */ value) { settings.selected = value; };

        /** @returns {string} */ this.getText = function() { return settings.text || VRS.globalisation.getText(settings.textKey); };
    };
    //endregion

    //region Pre-builts
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
     //endregion
}(window.VRS = window.VRS || {}, jQuery));