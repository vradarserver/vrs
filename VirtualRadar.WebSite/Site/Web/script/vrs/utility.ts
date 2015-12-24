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

/// <reference path="../../../_external/purl.d.ts" />
/// <reference path="../../../_external/purl-jquery.d.ts" />


namespace VRS
{
    /**
     * Helper methods for dealing with arrays.
     */
    export class ArrayHelper
    {
        /**
         * Returns a new array containing all of the elements in array that are not also in exceptArray. This could get
         * very expensive as it involves a loop within a loop.
         */
        except<T>(array: T[], exceptArray: T[], compareCallback?: (lhs: T, rhs: T) => boolean) : T[]
        {
            let result = <T[]>[];

            let arrayLength = array.length;
            let exceptArrayLength = exceptArray.length;
            for(let i = 0;i < arrayLength;++i) {
                let arrayValue = array[i];
                let existsInExcept = false;
                for(let c = 0;c < exceptArrayLength;++c) {
                    let exceptValue = exceptArray[c];
                    if(compareCallback ? compareCallback(arrayValue, exceptValue) : exceptValue === arrayValue) {
                        existsInExcept = true;
                        break;
                    }
                }
                if(!existsInExcept) result.push(arrayValue);
            }

            return result;
        }

        /**
         * Calls the allowItem callback with each item in the array. Returns the array formed from those items for which
         * the callback returned true.
         */
        filter<T>(array: T[], allowItem: (item: T) => boolean) : T[]
        {
            let result = <T[]>[];

            let length = array.length;
            for(let i = 0;i < length;++i) {
                let item = array[i];
                if(allowItem(item)) result.push(item);
            }

            return result;
        }

        /**
         * Returns the first item for which the matchesCallback returns true or the noMatchesValue if nothing matches.
         */
        findFirst<T>(array: T[], matchesCallback: (item: T) => boolean, noMatchesValue?: T) : T
        {
            let index = this.indexOfMatch(array, matchesCallback);
            return index === -1 ? noMatchesValue : array[index];
        }

        /**
         * Returns the index of the first element that is the same as the value passed across or -1 if the value is not present in the array.
         */
        indexOf<T>(array: T[], value: T, fromIndex: number = 0) : number
        {
            let result = -1;
            if(array) {
                let length = array.length;
                for(let i = fromIndex;i < length;++i) {
                    if(array[i] === value) {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }

        /**
         * Returns the index of the first element for which matchesCallback returns true or -1 if no element matches.
         */
        indexOfMatch<T>(array: T[], matchesCallback: (item: T) => boolean, fromIndex: number = 0) : number
        {
            let result = -1;

            if(array) {
                let length = array.length;
                for(let i = fromIndex;i < length;++i) {
                    if(matchesCallback(array[i])) {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }

        /**
         * Returns true if the object passed across is an array.
         */
        isArray(obj: any) : boolean
        {
            return !!(obj && Object.prototype.toString.call(obj) === '[object Array]');
        }

        /**
         * Modifies an array, usually loaded from persistent storage, so that it is optionally the same length as the
         * 'default' array and its entries are all known to be valid.
         * @param {Array}              [defaultArray]       The array whose contents will be used to pad out missing entries in optionsArray. If not supplied then no padding or trimming is performed.
         * @param {Array}               optionsArray        The array that is being normalised.
         * @param {function(*):bool}    isValidCallback     A method that will be called for each entry in optionsArray to ensure that the entry is valid.
         */
        normaliseOptionsArray<T>(defaultArray: T[], optionsArray: T[], isValidCallback: (item: T) => boolean)
        {
            let i;
            let desiredLength = defaultArray ? defaultArray.length : -1;

            for(i = 0;i < optionsArray.length;++i) {
                if(!isValidCallback(optionsArray[i])) optionsArray.splice(i--, 1);
            }

            for(i = optionsArray.length;i < desiredLength;++i) {
                optionsArray.push(defaultArray[i]);
            }

            if(desiredLength !== -1) {
                let extraneousEntries = optionsArray.length - desiredLength;
                if(extraneousEntries > 0) optionsArray.splice(optionsArray.length - extraneousEntries, extraneousEntries);
            }
        }

        /**
         * Calls the selectCallback for each item in the array and returns the array formed from the values returned by
         * the callback.
         */
        select<TArray, TResult>(array: TArray[], selectCallback: (item: TArray) => TResult) : TResult[]
        {
            let result = <TResult[]>[];

            let length = array.length;
            for(let i = 0;i < length;++i) {
                result.push(selectCallback(array[i]));
            }

            return result;
        }
    }

    /**
     * A helper object that can tell us some things about the browser.
     */
    export class BrowserHelper
    {
        private _ForceFrame: string;
        private _ForceFrameHasBeenRead: boolean;
        /**
         * Returns the iframe that VRS has been told that it's been loaded into via the forceFrame query string parameter.
         */
        getForceFrame() : string
        {
            if(!this._ForceFrameHasBeenRead) {
                if(purl) {
                    this._ForceFrame = $.url().param('forceFrame');
                }
                this._ForceFrameHasBeenRead = true;
            }

            return this._ForceFrame;
        }

        private _IsProbablyIPad: boolean;
        /**
         * Returns true if the browser is probably running on an iPad. Actual iPads should be automatically detected but
         * you can force it for any page by passing 'isIpad=1' on the query string (case insensitive, any non-zero value will do).
         */
        isProbablyIPad(): boolean
        {
            if(this._IsProbablyIPad === undefined) {
                if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isIpad')) this._IsProbablyIPad = true;
                }
                if(this._IsProbablyIPad === undefined) this._IsProbablyIPad = navigator.userAgent.indexOf('iPad') !== -1;
            }

            return this._IsProbablyIPad;
        }

        private _IsProbablyIPhone: boolean;
        /**
         * Returns true if the browser is probably running on an iPhone. Actual iPhones should be automatically detected
         * but you can force it by passing 'isIphone=1' on the query string (case insensitive, any non-zero value will work).
         */
        isProbablyIPhone() : boolean
        {
            if(this._IsProbablyIPhone === undefined) {
                if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isIphone')) this._IsProbablyIPhone = true;
                }
                if(this._IsProbablyIPhone === undefined) this._IsProbablyIPhone = navigator.userAgent.indexOf('iPhone') !== -1;
            }

            return this._IsProbablyIPhone;
        }

        private _IsProbablyAndroid: boolean;
        /**
         * Returns true if the browser is probably running on an Android device.
         */
        isProbablyAndroid() : boolean
        {
            if(this._IsProbablyAndroid === undefined) this._IsProbablyAndroid = navigator.userAgent.indexOf('; Android ') !== -1;
            return this._IsProbablyAndroid;
        };

        private _IsProbablyAndroidPhone: boolean;
        /**
         * Returns true if the browser is probably running on an Android phone.
         */
        isProbablyAndroidPhone() : boolean
        {
            if(this._IsProbablyAndroidPhone === undefined) this._IsProbablyAndroidPhone = this.isProbablyAndroid() && navigator.userAgent.indexOf(' Mobile') !== -1;
            return this._IsProbablyAndroidPhone;
        }

        private _IsProbablyAndroidTablet: boolean;
        /**
         * Returns true if the browser is probably running on an Android tablet.
         */
        isProbablyAndroidTablet() : boolean
        {
            if(this._IsProbablyAndroidTablet === undefined) this._IsProbablyAndroidTablet = this.isProbablyAndroid() && !this.isProbablyAndroidPhone();
            return this._IsProbablyAndroidTablet;
        }

        private _IsProbablyWindowsPhone: boolean;
        /**
         * Returns true if the browser is probably running on a Windows phone.
         */
        isProbablyWindowsPhone() : boolean
        {
            if(this._IsProbablyWindowsPhone === undefined) this._IsProbablyWindowsPhone = navigator.userAgent.indexOf('; Windows Phone ') !== -1;
            return this._IsProbablyWindowsPhone;
        };


        private _IsProbablyTablet: boolean;
        /**
         * Returns true if the browser is probably running on a tablet. iPads and Android tablets are automatically
         * detected, for other tablets you need to pass 'isTablet=1' on the query string (case insensitive, any non-zero
         * value will do).
         */
        isProbablyTablet() : boolean
        {
            if(this._IsProbablyTablet === undefined) {
                if(this.isProbablyIPad() || this.isProbablyAndroidTablet()) this._IsProbablyTablet = true;
                else if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isTablet')) this._IsProbablyTablet = true;
                } else this._IsProbablyTablet = false;
            }
            return this._IsProbablyTablet;
        }

        private _IsProbablyPhone: boolean;
        /**
         * Returns true if the browser is probably running on a phone. iPhones, Android and Windows phones are automatically
         * detected, for other phones you need to pass 'isPhone=1' on the query string (case insensitive, any non-zero
         * value will do).
         */
        isProbablyPhone() : boolean
        {
            if(this._IsProbablyPhone === undefined) {
                if(this.isProbablyIPhone() || this.isProbablyAndroidPhone() || this.isProbablyWindowsPhone()) this._IsProbablyPhone = true;
                else if(purl) {
                    var pageUrl = $.url();
                    if(pageUrl.param('isPhone')) this._IsProbablyPhone = true;
                } else this._IsProbablyPhone = false;
            }
            return this._IsProbablyPhone;
        }

        private _IsHighDpi: boolean;
        /**
         * Returns true if the browser is probably running on a high DPI display.
         */
        isHighDpi() : boolean
        {
            if(this._IsHighDpi === undefined) {
                this._IsHighDpi = false;
                if(window.devicePixelRatio > 1) this._IsHighDpi = true;
                else {
                    var query = '(-webkit-min-device-pixel-ratio: 1.5), (min--moz-device-pixel-ratio: 1.5), (-o-min-device-pixel-ratio: 3/2), (min-resolution: 1.5dppx)';
                    if(window.matchMedia && window.matchMedia(query).matches) this._IsHighDpi = true;
                }
            }

            return this._IsHighDpi;
        }

        private _NotOnline: boolean;
        /**
         * Returns true if the notOnline query string has been set.
         */
        notOnline() : boolean
        {
            if(this._NotOnline === undefined) this._NotOnline = !purl ? false : $.url().param('notOnline') === '1';
            return this._NotOnline;
        }

        /**
         * Returns a URL with a parameters object appended as a query string.
         */
        formUrl(url: string, params: Object, recursive: boolean) : string
        {
            var result = url;
            if(params) {
                var queryString = $.param(params, !recursive);
                if(queryString) result += '?' + queryString;
            }

            return result;
        }

        /**
         * Returns a URL to another page in VRS with a parameters object appended as a query string.
         */
        formVrsPageUrl(url: string, params?: Object, recursive?: boolean) : string
        {
            // We need to ensure that anything in params comes first so that the caller can control which parameter
            // is first. Internet Explorer has a hard time if some parameters have a & before them (e.g. &reg= gets
            // turned into <registered symbol>= - so for those we need to get them to the start of the query string
            // so they start with a ? instead of a &.
            params = $.extend(params || {}, {
                notOnline: this.notOnline() ? '1' : '0',
                forceFrame: this.getForceFrame()
            });

            return this.formUrl(url, params, recursive);
        }

        /**
         * Gets the target for a link to a VRS page. This is normally the string passed in, but if the site is running
         * within an iframe then targets out of the iframe will point directly at the server so this, in conjunction
         * with the forceFrame query string parameter, keep everything in the same iframe.
         */
        getVrsPageTarget(target: string) : string
        {
            return this.getForceFrame() || target;
        };
    }

    /**
     * Methods to help with handling colours.
     */
    export class ColourHelper
    {
        getWhite() : IColour    { return { r: 255, g: 255, b: 255 }; }
        getRed() : IColour      { return { r: 255, g: 0,   b: 0   }; }
        getGreen() : IColour    { return { r: 0,   g: 255, b: 0   }; }
        getBlue() : IColour     { return { r: 255, g: 0,   b: 255 }; }
        getBlack() : IColour    { return { r: 0,   g: 0,   b: 0   }; }

        /**
         * Given a numeric value, a low value and a high value this converts the value to a colour on a colour wheel.
         *
         * The colour wheel represents all possible HSV values where S and V are 100% and only the hue changes. The hue
         * starts at 100% green and red, red falls to 0%, blue rises to 100%, green falls to 0%, red rises to 100%,
         * blue falls to 100%. The last phase, where green rises to 100% (bringing us to the start of the wheel) is not
         * performed as it would lead to confusion between the start and the end of the wheel - the intended use of this
         * method is for coloured trails where we want no confusion between low and high values.
         */
        getColourWheelScale(value: number, lowValue: number, highValue: number, invalidIsBelowLow: boolean = true, stretchLowerValues: boolean = true) : IColour
        {
            let result = <IColour>null;

            if(value === undefined || isNaN(value)) {
                if(invalidIsBelowLow) result = this.getWhite();
            } else if(value <= lowValue) {
                result = this.getWhite();
            } else if(value >= highValue) {
                result = this.getRed();
            } else {
                value -= lowValue;
                let range = (highValue - lowValue) + 1;
                let fifthRange = range / 5;
                if(!fifthRange) result = this.getWhite();
                else {
                    if(stretchLowerValues) {
                        if(value < fifthRange) value *= 2;
                        else {
                            let newBase = fifthRange * 2;
                            value = Math.round((((value - fifthRange) / (range - fifthRange)) * (range - newBase)) + newBase);
                        }
                    }
                    let fifth = Math.floor(value / fifthRange);
                    let remainder = (value - (fifth * fifthRange)) / fifthRange;

                    let r, g, b;
                    switch(fifth) {
                        case 0:     b = 0; g = 255; r = this.fallingComponent(remainder); break;
                        case 1:     r = 0; g = 255; b = this.risingComponent(remainder); break;
                        case 2:     r = 0; b = 255; g = this.fallingComponent(remainder); break;
                        case 3:     g = 0; b = 255; r = this.risingComponent(remainder); break;
                        case 4:     g = 0; r = 255; b = this.fallingComponent(remainder); break;
                    }

                    result = { r: r, g: g, b: b };
                }
            }

            return result;
        }

        private risingComponent(proportion: number) : number
        {
            return Math.floor(255 * proportion);
        }

        private fallingComponent(proportion: number) : number
        {
            return 255 - Math.floor(255 * proportion);
        }

        /**
         * Takes a colour object and returns a CSS colour string.
         */
        colourToCssString(colour: IColour) : string
        {
            var result = <string>null;
            if(colour) {
                var toHex = function(value) { var hex = value.toString(16); return hex.length === 1 ? '0' + hex : hex; };
                result = '#' + toHex(colour.r) + toHex(colour.g) + toHex(colour.b);
            }

            return result;
        }
    }

    /**
     * A collection of methods that help when dealing with dates.
     */
    export class DateHelper
    {
        private _TicksInDay: number = 1000 * 60 * 60 * 24;

        /**
         * Returns the date portion of the date/time passed across.
         */
        getDatePortion(date: Date) : Date
        {
            return new Date(this.getDateTicks(date));
        }

        /**
         * Returns the number of ticks that represent the date portion of the date/time passed across.
         */
        getDateTicks(date: Date) : number
        {
            return Math.floor(date.getTime() / this._TicksInDay) * this._TicksInDay;
        }

        /**
         * Returns a new date consisting only of the time portion of the date/time passed across.
         */
        getTimePortion(date: Date) : Date
        {
            return new Date(this.getTimeTicks(date));
        }

        /**
         * Returns the number of ticks in the time portion of the date/time passed across.
         */
        getTimeTicks(date: Date) : number
        {
            return date.getTime() % this._TicksInDay;
        }

        /**
         * Parses text into a date. The text can either be in ISO format (yyyy-mm-dd) or it can be a number of days
         * offset from today (e.g. +0 = today, -1 = yesterday, +1 = tomorrow). Returns undefined if the text cannot be
         * parsed into a date.
         */
        parse(text: string) : Date
        {
            var result: Date;

            if(text) {
                var offsetMatches = text.match(/[+\-]?\d+/g);
                if(offsetMatches && offsetMatches.length === 1 && offsetMatches[0] === text) {
                    var offset = parseInt(text);
                    if(!isNaN(offset)) result = offset === 0 ? new Date() : new Date(new Date().getTime() + (offset * this._TicksInDay));
                } else {
                    var ticks = Date.parse(text);
                    if(!isNaN(ticks)) result = new Date(ticks);
                }
            }

            return result;
        }

        /**
         * Formats the date passed across as a string in ISO format.
         */
        toIsoFormatString(date: Date, suppressTime: boolean, suppressTimeZone: boolean) : string
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
        }
    }

    /**
     * An object that can help with displaying traces on mobile browsers that don't have debug facilities.
     */
    export class DelayedTrace
    {
        private _Lines: string[] = [];

        constructor(title: string, delayMilliseconds: number)
        {
            var self = this;
            setTimeout(function() {
                var message = '';
                $.each(self._Lines, function(idx, line) {
                    if(message.length) message += '\n';
                    message += line;
                });
                VRS.pageHelper.showMessageBox(title, message);
            }, delayMilliseconds);
        }

        /**
         * Adds a line to the trace message.
         */
        add(message: string)
        {
            this._Lines.push(message || '');
        }
    }

    /**
     * An object that can help when dealing directly with DOM elements.
     */
    export class DomHelper
    {
        /**
         * Sets a value for the attribute passed across.
         */
        setAttribute(element: HTMLElement, name: string, value: string)
        {
            element.setAttribute(name, value);
        }

        /**
         * Removes an attribute from the element. The attribute does not have to already be present on the element.
         */
        removeAttribute(element: HTMLElement, name: string)
        {
            element.removeAttribute(name);
        }

        /**
         * Sets the class on the element passed across, obliterating any previous class set on the element.
         */
        setClass(element: HTMLElement, className: string)
        {
            element.className = className;
        }

        /**
         * Adds one or more class names to the element passed across. If a class is already on the element then it
         * is ignored.
         */
        addClasses(element: HTMLElement, addClasses: string[])
        {
            if(addClasses.length) {
                var current = this.getClasses(element);
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
                    this.setClasses(element, current);
                }
            }
        }

        /**
         * Removes one or more class names from the element passed across. If a class is not already on the element then
         * it is ignored.
         */
        removeClasses(element: HTMLElement, removeClasses: string[])
        {
            if(removeClasses.length) {
                var current = this.getClasses(element);
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
                        this.setClasses(element, preserveClasses);
                    }
                }
            }
        }

        /**
         * Returns an array of classes associated with the element.
         */
        getClasses(element: HTMLElement) : string[]
        {
            var result: string[] = [];
            var classes = (element.className || '').split(' ');
            var length = classes.length;
            for(var i = 0;i < length;++i) {
                var name = classes[i];
                if(name) result.push(name);
            }

            return result;
        }

        /**
         * Sets the classes associated with the array. Does not check for duplicate classes.
         */
        setClasses(element: HTMLElement, classNames: string[])
        {
            this.setClass(element, classNames.join(' '));
        }
    }

    /**
     * Helper methods for dealing with objects that only contain fields with constant values.
     */
    export class EnumHelper
    {
        /**
         * Returns the property name of an enum value within its parent object.
         */
        getEnumName(enumObject: Object, value: any) : string
        {
            var result: string = undefined;

            for(var property in enumObject) {
                if(enumObject[property] === value) {
                    result = property;
                    break;
                }
            }

            return result;
        }

        /**
         * Returns all of the enum values as a list.
         */
        getEnumValues(enumObject: Object) : any[]
        {
            var result = [];

            for(var property in enumObject) {
                result.push(enumObject[property]);
            }

            return result;
        }
    }

    /**
     * A collection of Great Circle math functions.
     */
    export class GreatCircle
    {
        /**
         * Returns true if the lat and lng are within the bounds described by tlLat/tlLng and brLat/brLng.
         */
        isLatLngInBounds(lat: number, lng: number, bounds: IBounds) : boolean
        {
            // This is basically a port of VirtualRadar.WebSite.AircraftListJsonBuilder.IsWithinBounds()
            // See comments there for an explanation of why this is doing what it does.

            var result = !isNaN(lat) && !isNaN(lng);
            if(result) {
                result = bounds.tlLat >= lat && bounds.brLat <= lat;
                if(result) {
                    lng = lng >= 0 ? lng : lng + 360;
                    var left = bounds.tlLng >= 0 ? bounds.tlLng : bounds.tlLng + 360;
                    var right = bounds.brLng >= 0 ? bounds.brLng : bounds.brLng + 360;
                    if(left !== 180 || right !== 180 ) {
                        if(left == right)     result = lng == left;
                        else if(left > right) result = (lng >= left && lng <= 360.0) || (lng >= 0.0 && lng <= right);
                        else                  result = lng >= left && lng <= right;
                    }
                }
            }

            return result;
        }

        /**
         * Takes two lat/lngs and returns a bounds object that encompases them both. If either point is missing then
         * a bounds encompassing just the remaining point is returned. If neither point is supplied then null is returned.
         */
        arrangeTwoPointsIntoBounds(point1: ILatLng, point2: ILatLng) : IBounds
        {
            var result: IBounds = null;

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
        }
    }

    /**
     * A collection of functions that help when dealing with JSON.
     */
    export class JsonHelper
    {
        /**
         * Converts the Microsoft format date constructors in JSON into actual date constructors. Note that the resulting
         * JSON is no longer valid JSON - but without it the dates will be evaluated as strings.
         */
        convertMicrosoftDates(json: string) : string
        {
            return json.replace(/\"\\\/Date\(([\d\+\-]+)\)\\\/\"/g, 'new Date($1)');
        }
    }

    /**
     * A utility class that holds methods that help when working with objects.
     */
    export class ObjectHelper
    {
        /**
         * Creates a prototype object containing the members of a base class. The constructor of the prototype object
         * must remember to call the base's constructor in its constructor.
         */
        subclassOf(base: Function) : Function
        {
            var blankSlate = () => {};
            blankSlate.prototype = base.prototype;
            return new blankSlate();
        }
    }

    /**
     * Common full-page HTML operations.
     */
    export class PageHelper
    {
        /**
         * Shows or hides a full-page wait animation that prevents the user from interacting with the page.
         */
        showModalWaitAnimation(onOff: boolean)
        {
            if(onOff) $('body').addClass('wait');
            else      $('body').removeClass('wait');
        }

        /**
         * Displays a simple message-box to the user. Attempts to use jQueryUI dialog but if that isn't present then
         * it falls back onto the browser's alert method. Note that this may or may not block, depending upon whether
         * it had to fall back to alert (the method blocks) or whether it could use jQueryUI (the method does not block).
         * If the dialog is used then it shows a modal message box so the UI can't be accessed while it's running.
         */
        showMessageBox(title: string, message: string)
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
        }

        private _Indent = 0;
        /**
         * Adds an opening indented log entry.
         */
        addIndentLog(message: string) : Date
        {
            var result = this.indentLog(message);
            ++this._Indent;
            return result;
        }

        /**
         * Adds a closing indented log entry.
         */
        removeIndentLog(message: string, started: Date)
        {
            this._Indent = Math.max(0, this._Indent - 1);
            this.indentLog(message, started);
        }

        /**
         * Adds a message at the current indent level.
         */
        indentLog(message: string, started?: Date) : Date
        {
            var now = new Date();
            if(started) message = message + ' took ' + (now.getTime() - started.getTime()) + ' ms';
            var indent = VRS.stringUtility.repeatedSequence(' ', this._Indent * 4);
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
        }
    }

    /**
     * Helper methods for dealing with time spans.
     */
    export class TimeHelper
    {
        /**
         * Returns an object reporting the number of hours, minutes and seconds that a number of seconds describes.
         */
        secondsToHoursMinutesSeconds(seconds: number) : ITime
        {
            var hours = Math.floor(seconds / 3600);
            seconds -= (hours * 3600);
            var minutes = Math.floor(seconds / 60);
            seconds -= (minutes * 60);

            return { hours: hours, minutes: minutes, seconds: seconds };
        }

        /**
         * Returns an object reporting the number of hours, minutes and seconds that a number of ticks (milliseconds) describes.
         */
        ticksToHoursMinutesSeconds(ticks: number) : ITime
        {
            return this.secondsToHoursMinutesSeconds(Math.floor(ticks / 1000));
        }
    }

    /**
     * A dumping ground for miscellaneous utility methods.
     */
    export class Utility
    {
        /**
         * Given a value that is either a function that returns the value, or the value itself, either
         * returns the value or returns the result of calling the function. If the value is undefined
         * or null then the default value is returned (which, by default, is undefined).
         */
        public static ValueOrFuncReturningValue<T>(value: T | VoidFuncReturning<T>, defaultValue?: T) : T
        {
            var result = defaultValue;
            if(value !== undefined && value !== null) {
                if($.isFunction(value)) {
                    result = (<()=>T>value)();
                } else {
                    result = <T>value;
                }
            }

            return result;
        }
    }

    /**
     * A collection of functions that convert values from one unit to another.
     */
    export class UnitConverter
    {
        /**
         * Converts distances from one unit to another.
         * @param {number} value The distance to convert.
         * @param {string} fromUnit A VRS.Distance unit to convert from.
         * @param {string} toUnit A VRS.Distance unit to convert to.
         * @returns {number} The converted value.
         */
        convertDistance(value: number, fromUnit: string, toUnit: string) : number
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
        }

        /**
         * Returns the translated abbreviation for a VRS.Distance unit.
         * @param {string} unit The VRS.Distance unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        distanceUnitAbbreviation(unit: string) : string
        {
            switch(unit) {
                case VRS.Distance.Kilometre:    return VRS.$$.KilometreAbbreviation;
                case VRS.Distance.NauticalMile: return VRS.$$.NauticalMileAbbreviation;
                case VRS.Distance.StatuteMile:  return VRS.$$.StatuteMileAbbreviation;
                default:                        throw 'Unknown distance unit ' + unit;
            }
        }

        /**
         * Converts heights from one unit to another.
         * @param {number} value The height to convert.
         * @param {string} fromUnit A VRS.Height unit to convert from.
         * @param {string} toUnit A VRS.Height unit to convert to.
         * @returns {number} The converted value.
         */
        convertHeight(value: number, fromUnit: string, toUnit: string) : number
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
        }

        /**
         * Returns the translated abbreviation for a VRS.Height unit.
         * @param {string} unit The VRS.Height unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        heightUnitAbbreviation(unit: string) : string
        {
            switch(unit) {
                case VRS.Height.Feet:           return VRS.$$.FeetAbbreviation;
                case VRS.Height.Metre:          return VRS.$$.MetreAbbreviation;
                default:                        throw 'Unknown height unit ' + unit;
            }
        }

        /**
         * Returns the translated abbreviation for a VRS.Height unit over time.
         * @param {string} unit The VRS.Height unit to get an abbreviation for.
         * @param {boolean} perSecond True if it is height over seconds, false if it is height over minutes.
         * @returns {string} The translated abbreviation.
         */
        heightUnitOverTimeAbbreviation(unit: string, perSecond: boolean) : string
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
        }

        /**
         * Converts speeds from one unit to another.
         * @param {number} value The speed to convert.
         * @param {string} fromUnit A VRS.Speed unit to convert from.
         * @param {string} toUnit A VRS.Speed unit to convert to.
         * @returns {number} The converted value.
         */
        convertSpeed(value: number, fromUnit: string, toUnit: string) : number
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
        }

        /**
         * Returns the translated abbreviation for a VRS.Speed unit.
         * @param {string} unit The VRS.Speed unit to get an abbreviation for.
         * @returns {string} The translated abbreviation.
         */
        speedUnitAbbreviation(unit: string) : string
        {
            switch(unit) {
                case VRS.Speed.Knots:               return VRS.$$.KnotsAbbreviation;
                case VRS.Speed.KilometresPerHour:   return VRS.$$.KilometresPerHourAbbreviation;
                case VRS.Speed.MilesPerHour:        return VRS.$$.MilesPerHourAbbreviation;
                default:                            throw 'Unknown speed unit ' + unit;
            }
        }

        /**
         * Converts a vertical speed from one unit to another.
         * @param {number}      verticalSpeed   The vertical speed in x units per minute to convert.
         * @param {VRS.Height}  fromUnit        The units that the vertical speed is expressed in.
         * @param {VRS.Height}  toUnit          The units to convert to.
         * @param {boolean}     perSecond       True if the vertical speed should be converted to y units per second.
         */
        convertVerticalSpeed(verticalSpeed: number, fromUnit: string, toUnit: string, perSecond: boolean) : number
        {
            var result = verticalSpeed;
            if(result !== undefined) {
                if(fromUnit !== toUnit) result = this.convertHeight(result, fromUnit, toUnit);
                if(perSecond) result = Math.round(result / 60);
            }

            return result;
        }

        /**
         * Accepts an integer number of pixels or a string ending with '%' and returns an
         * object describing whether the value is pixels or percent, and a number indicating what that value is.
         * Percents are divided by 100 before being returned.
         * @param {String|Number} value Either the integer percentage or a string ending with '%'.
         */
        getPixelsOrPercent(value: string | number) : IPercentValue
        {
            var valueAsString = String(value);
            var result = {
                value: parseInt(valueAsString),
                isPercent: VRS.stringUtility.endsWith(valueAsString, '%', false)
            };
            if(result.isPercent) result.value /= 100;

            return result;
        }
    }

    /**
     * The settings that can be passed when creating a new instance of ValueText.
     */
    export interface ValueText_Settings
    {
        value:      any;
        text?:      string;
        textKey?:   string;
        selected?:  boolean;
    }

    /**
     * Associates a value with a text description.
     */
    export class ValueText
    {
        private _Settings: ValueText_Settings;

        constructor(settings: ValueText_Settings)
        {
            this._Settings = settings;
        }

        getValue() : any
        {
            return this._Settings.value;
        }

        setValue(value: any)
        {
            this._Settings.value = value;
        }

        getSelected() : boolean
        {
            return this._Settings.selected || false;
        }

        setSelected(value: boolean)
        {
            this._Settings.selected = value;
        }

        getText() : string
        {
            return this._Settings.text || VRS.globalisation.getText(this._Settings.textKey);
        }
    }

    // Pre-builts. The non-TypeScript JavaScript (including custom content scripts) expect to be able to access the utility
    // classes as instances rather than statics.
    export var arrayHelper = new VRS.ArrayHelper();
    export var browserHelper = new VRS.BrowserHelper();
    export var colourHelper = new VRS.ColourHelper();
    export var dateHelper = new VRS.DateHelper();
    export var domHelper = new VRS.DomHelper();
    export var enumHelper = new VRS.EnumHelper();
    export var greatCircle = new VRS.GreatCircle();
    export var jsonHelper = new VRS.JsonHelper();
    export var objectHelper = new VRS.ObjectHelper();
    export var pageHelper = new VRS.PageHelper();
    export var timeHelper = new VRS.TimeHelper();
    export var unitConverter = new VRS.UnitConverter();
}