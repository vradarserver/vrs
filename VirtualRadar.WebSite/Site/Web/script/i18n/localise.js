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
 * @fileoverview Declares everything about the locale handling in VRS.
 */
var VRS;
(function (VRS) {
    /**
     * Describes some basic information about a culture.
     */
    var CultureInfo = (function () {
        function CultureInfo(locale, settings) {
            this.locale = locale;
            this.cultureName = settings.forceCultureName || locale;
            this.language = settings.language;
            this.flagImage = settings.flagImage || ('images/regions/' + (settings.countryFlag ? settings.countryFlag : settings.language) + '.bmp');
            this.flagSize = settings.flagSize || { width: 20, height: 16 };
            this.englishName = settings.englishName;
            this.nativeName = settings.nativeName || this.englishName;
            this.topLevel = settings.topLevel !== undefined ? settings.topLevel : false;
            this.groupLanguage = settings.groupLanguage || settings.language;
        }
        CultureInfo.prototype.getFlagImageHtml = function () {
            var result = '';
            if (this.flagImage && this.flagSize) {
                result = '<img src="' + this.flagImage + '" width="' + this.flagSize.width + 'px" height="' + this.flagSize.height + 'px" alt="' + this.nativeName + '" />';
            }
            return result;
        };
        return CultureInfo;
    })();
    VRS.CultureInfo = CultureInfo;
    /**
     * The class that handles the selection and loading of locales for VRS.
     */
    var Localise = (function () {
        function Localise() {
            /**
             * An associative array of region names (e.g. en-GB) and an object describing the settings associated with that region.
             */
            this._CultureInfos = {};
            /**
             * An associative array of loaded Globalize files indexed by region name (e.g. en-GB).
             * @type {Object.<string, bool>}
             * @private
             */
            this._LoadedGlobalizations = {};
            /**
             * The event dispatcher.
             */
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.Localise'
            });
            /**
             * A collection of event names.
             */
            this._Events = {
                localeChanged: 'localeChanged'
            };
            /**
             * The currently selected locale.
             */
            this._Locale = '';
        }
        /**
         * Gets the currently selected locale. Locales are full .NET region codes, e.g. 'en-GB' for British English.
         */
        Localise.prototype.getLocale = function () {
            return this._Locale;
        };
        /**
         * Sets the locale by region code.
         * @param {string}      value               The code of the language to load.
         * @param {function}    successCallback     Function called if the language is loaded successfully, or is already loaded.
         */
        Localise.prototype.setLocale = function (value, successCallback) {
            if (value === this._Locale) {
                if (successCallback)
                    successCallback();
            }
            else {
                this._Locale = value;
                var cultureInfo = this._CultureInfos[this._Locale];
                if (cultureInfo) {
                    var self = this;
                    // English is the base language, if other language files don't supply a string then the English version should be used instead.
                    this.loadLanguage('en', function () {
                        self.loadLanguage(cultureInfo.language, function () {
                            self.loadCulture(cultureInfo.cultureName, function () {
                                Globalize.culture(cultureInfo.cultureName);
                                self._Dispatcher.raise(self._Events.localeChanged);
                                if (successCallback)
                                    successCallback();
                            });
                        });
                    });
                }
            }
        };
        /**
         * Hooks the event raised after the locale has changed.
         */
        Localise.prototype.hookLocaleChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.localeChanged, callback, forceThis);
        };
        /**
         * Unhooks any event hooked on this object.
         */
        Localise.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        /**
         * Saves the current state of the object.
         */
        Localise.prototype.saveState = function () {
            var settings = this.createSettings();
            VRS.configStorage.saveWithoutPrefix('Localise', settings);
        };
        /**
         * Loads the saved state of the object.
         */
        Localise.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.loadWithoutPrefix('Localise', {});
            var result = $.extend(this.createSettings(), savedSettings);
            if (!result.locale || !this._CultureInfos[result.locale])
                result.locale = this.guessBrowserLocale();
            return result;
        };
        /**
         * Applies a saved state to the object.
         */
        Localise.prototype.applyState = function (config, successCallback) {
            config = config || {};
            this.setLocale(config.locale || 'en', successCallback);
        };
        /**
         * Loads and then applies the saved state to the object.
         */
        Localise.prototype.loadAndApplyState = function (successCallback) {
            this.applyState(this.loadState(), successCallback);
        };
        /**
         * Creates the saved state object.
         */
        Localise.prototype.createSettings = function () {
            return {
                locale: this._Locale
            };
        };
        /**
         * Loads a language script (i.e. translations of text) from the server.
         */
        Localise.prototype.loadLanguage = function (language, successCallback) {
            var self = this;
            if (language === this._LoadedLanguage) {
                if (successCallback)
                    successCallback();
            }
            else {
                var url = 'script/i18n/strings.' + language.toLowerCase() + '.js';
                VRS.scriptManager.loadScript({ url: url, success: function () {
                        self._LoadedLanguage = language;
                        if (successCallback)
                            successCallback();
                    } });
            }
        };
        /**
         * Loads a culture script (i.e. number and date formatting) from the server.
         */
        Localise.prototype.loadCulture = function (cultureName, successCallback) {
            var self = this;
            if (this._LoadedGlobalizations[cultureName]) {
                if (successCallback)
                    successCallback();
            }
            else {
                var url = 'script/i18n/globalize/globalize.culture.' + cultureName + '.js';
                VRS.scriptManager.loadScript({ url: url, success: function () {
                        self._LoadedGlobalizations[cultureName] = true;
                        if (successCallback)
                            successCallback();
                    } });
            }
        };
        /**
         * Returns a best-guess at the current region code for the browser or en-GB if either no code could be reliably figured out or
         * if we don't have any information about the region.
         */
        Localise.prototype.guessBrowserLocale = function () {
            var result = navigator.userLanguage || navigator.systemLanguage || navigator.browserLanguage || navigator.language;
            if (!result)
                result = 'en-GB';
            if (!this._CultureInfos[result]) {
                // If we know the base language (e.g. the 'en' in 'en-??') then use it
                var hyphenPos = result.indexOf('-');
                var language = hyphenPos === -1 ? null : result.substr(0, hyphenPos);
                if (language && this._CultureInfos[language])
                    result = language;
                else
                    result = 'en-GB';
            }
            return result;
        };
        /**
         * Adds culture information to the object.
         */
        Localise.prototype.addCultureInfo = function (cultureName, settings) {
            if (!this._CultureInfos[cultureName])
                this._CultureInfos[cultureName] = new VRS.CultureInfo(cultureName, settings);
        };
        /**
         * Returns the currently selected culture's information or the information about any known culture.
         */
        Localise.prototype.getCultureInfo = function (cultureName) {
            return this._CultureInfos[cultureName || this._Locale];
        };
        /**
         * Removes the information about a culture.
         */
        Localise.prototype.removeCultureInfo = function (cultureName) {
            if (this._CultureInfos[cultureName])
                delete this._CultureInfos[cultureName];
        };
        /**
         * Returns an array of every known culture.
         */
        Localise.prototype.getCultureInfos = function () {
            var result = [];
            for (var locale in this._CultureInfos) {
                var cultureInfo = this._CultureInfos[locale];
                if (cultureInfo instanceof VRS.CultureInfo)
                    result.push(cultureInfo);
            }
            return result;
        };
        /**
         * Returns an array of arrays of cultures, grouped by language.
         */
        Localise.prototype.getCultureInfosGroupedByLanguage = function (sortByNativeName) {
            var result = [];
            $.each(this.getCultureInfos(), function (idx, cultureInfo) {
                var language = cultureInfo.groupLanguage;
                var innerArray = VRS.arrayHelper.findFirst(result, function (r) { return r[0].groupLanguage === language; }, null);
                if (!innerArray) {
                    innerArray = [];
                    result.push(innerArray);
                }
                innerArray.push(cultureInfo);
            });
            if (sortByNativeName) {
                $.each(result, function (idx, cultureArray) {
                    cultureArray.sort(function (lhs, rhs) {
                        return lhs.nativeName.localeCompare(rhs.nativeName);
                    });
                });
                result.sort(function (lhs, rhs) {
                    return lhs[0].nativeName.localeCompare(rhs[0].nativeName);
                });
            }
            return result;
        };
        ;
        /**
         * Returns the raw culture object for the current locale. Throws an exception if fails.
         */
        Localise.prototype.getRawGlobalizeData = function () {
            var result = Globalize.findClosestCulture();
            if (!result)
                throw 'Could not find the current Globalize culture';
            return result;
        };
        Localise.prototype.getText = function (keyOrFormatFunction) {
            if (keyOrFormatFunction instanceof Function)
                return keyOrFormatFunction();
            return VRS.$$[keyOrFormatFunction];
        };
        /**
         * Configures the date picker with the current locale's date formatting styles.
         */
        Localise.prototype.localiseDatePicker = function (datePickerJQ) {
            var options = this.getDatePickerOptions();
            datePickerJQ.datepicker('option', options);
        };
        /**
         * Returns an object containing jQueryUI datepicker options that match the current locale. The jQuery UI
         * datepicker does have its own localisation support but (a) it involves loading more JS modules for it, (b) it
         * duplicates some information in Globalization's culture files and (c) it is not guaranteed to be consistent
         * with the date formats in Globalization. I don't want to have a mixture of different localisation mechanisms
         * if I can help it.
         */
        Localise.prototype.getDatePickerOptions = function () {
            var culture = this.getRawGlobalizeData();
            var calendar = culture.calendars.standard;
            var months = VRS.$$.DateUseGenetiveMonths && calendar.monthsGenitive ? calendar.monthsGenitive : calendar.months;
            var shortYear = calendar.shortYearCutoff;
            if (Object.prototype.toString.call(shortYear) !== '[object String]')
                shortYear %= 100;
            var monthYearPattern = calendar.patterns['Y'] || 'MMMM yyyy';
            var showMonthAfterYear = monthYearPattern[0] === 'y';
            return {
                closeText: VRS.$$.DateClose,
                currentText: VRS.$$.DateCurrent,
                dateFormat: this.dotNetDateFormatToJQueryDateFormat(calendar.patterns['d']),
                dayNames: calendar.days.names,
                dayNamesMin: calendar.days.namesShort,
                dayNamesShort: calendar.days.namesAbbr,
                firstDay: calendar.firstDay,
                isRTL: culture.isRTL,
                shortYearCutoff: shortYear,
                showMonthAfterYear: showMonthAfterYear,
                monthNames: months.names,
                monthNamesShort: months.namesAbbr,
                nextText: VRS.$$.DateNext,
                prevText: VRS.$$.DatePrevious,
                weekHeader: VRS.$$.DateWeekAbbr,
                yearSuffix: VRS.$$.DateYearSuffix
            };
        };
        /**
         * Takes a format in .NET format and returns the equivalent date format in JQuery UI's date picker format.
         */
        Localise.prototype.dotNetDateFormatToJQueryDateFormat = function (dateFormat) {
            // We're a little hampered here by not having look behind in regex, hence the ugly switching to known text.
            // As these are date formats coming out of Globalize files it's very unlikely that these marker strings
            // will appear in the Globalization formats.
            var fullMonthMarker = 'FMONTH';
            var shortMonthMarker = 'SMONTH';
            var fullYearMarker = 'FYEAR';
            return dateFormat
                .replace('dddd', 'DD')
                .replace('ddd', 'D')
                .replace('MMMM', fullMonthMarker)
                .replace('MMM', shortMonthMarker)
                .replace('MM', 'mm')
                .replace('M', 'm')
                .replace(fullMonthMarker, 'MM')
                .replace(shortMonthMarker, 'M')
                .replace('yyyy', fullYearMarker)
                .replace('yy', 'y')
                .replace(fullYearMarker, 'yy');
        };
        /**
         * Sets the locale in the background. The loading of globalization files from the server takes a while, if it's
         * performed on a UI action like a button click it can feel unresponsive. This method wraps that in a function
         * that is called from a timer on a short delay and optionally adds a modal wait animation so that the UI can
         * be made to feel more responsive.
         */
        Localise.prototype.setLocaleInBackground = function (locale, showModalWait, localeLoadedCallback) {
            if (showModalWait === void 0) { showModalWait = true; }
            if (showModalWait)
                VRS.pageHelper.showModalWaitAnimation(true);
            this.setLocale(locale, function () {
                if (showModalWait)
                    VRS.pageHelper.showModalWaitAnimation(false);
                if (localeLoadedCallback)
                    localeLoadedCallback();
            });
        };
        return Localise;
    })();
    VRS.Localise = Localise;
    /**
     * The singleton instance of VRS.Localise.
     */
    VRS.globalisation = new VRS.Localise();
    // English
    VRS.globalisation.addCultureInfo('en', { language: 'en', englishName: 'English', forceCultureName: 'en-GB', topLevel: true }); // Globalize uses American settings for the default 'en' language. This is a British program :P
    VRS.globalisation.addCultureInfo('en-029', { language: 'en', englishName: 'English (Caribbean)' });
    VRS.globalisation.addCultureInfo('en-AU', { language: 'en', countryFlag: 'au', englishName: 'English (Australia)' });
    VRS.globalisation.addCultureInfo('en-BZ', { language: 'en', countryFlag: 'bz', englishName: 'English (Belize)' });
    VRS.globalisation.addCultureInfo('en-CA', { language: 'en', countryFlag: 'ca', englishName: 'English (Canada)' });
    VRS.globalisation.addCultureInfo('en-GB', { language: 'en', englishName: 'English (United Kingdom)' });
    VRS.globalisation.addCultureInfo('en-IE', { language: 'en', countryFlag: 'ie', englishName: 'English (Ireland)' });
    VRS.globalisation.addCultureInfo('en-IN', { language: 'en', countryFlag: 'in', englishName: 'English (India)' });
    VRS.globalisation.addCultureInfo('en-JM', { language: 'en', countryFlag: 'jm', englishName: 'English (Jamaica)' });
    VRS.globalisation.addCultureInfo('en-MY', { language: 'en', countryFlag: 'my', englishName: 'English (Malaysia)' });
    VRS.globalisation.addCultureInfo('en-NZ', { language: 'en', countryFlag: 'nz', englishName: 'English (New Zealand)' });
    VRS.globalisation.addCultureInfo('en-SG', { language: 'en', countryFlag: 'sg', englishName: 'English (Singapore)' });
    VRS.globalisation.addCultureInfo('en-TT', { language: 'en', countryFlag: 'tt', englishName: 'English (Trinidad and Tobago)', nativeName: 'English (Trinidad y Tobago)' });
    VRS.globalisation.addCultureInfo('en-US', { language: 'en', countryFlag: 'us', englishName: 'English (United States)' });
    VRS.globalisation.addCultureInfo('en-ZA', { language: 'en', countryFlag: 'za', englishName: 'English (South Africa)' });
    // German
    VRS.globalisation.addCultureInfo('de', { language: 'de', englishName: 'German', nativeName: 'Deutsch', topLevel: true });
    VRS.globalisation.addCultureInfo('de-DE', { language: 'de', countryFlag: 'de', englishName: 'German (Germany)', nativeName: 'Deutsch (Deutschland)' });
    // French
    VRS.globalisation.addCultureInfo('fr', { language: 'fr', englishName: 'French', nativeName: 'Français', topLevel: true });
    VRS.globalisation.addCultureInfo('fr-BE', { language: 'fr', countryFlag: 'be', englishName: 'French (Belgium)', nativeName: 'Français (Belgique)' });
    VRS.globalisation.addCultureInfo('fr-CA', { language: 'fr', countryFlag: 'ca', englishName: 'French (Canada)', nativeName: 'Français (Canada)' });
    VRS.globalisation.addCultureInfo('fr-CH', { language: 'fr', countryFlag: 'ch', englishName: 'French (Switzerland)', nativeName: 'Français (Suisse)' });
    VRS.globalisation.addCultureInfo('fr-FR', { language: 'fr', englishName: 'French (France)', nativeName: 'Français (France)' });
    VRS.globalisation.addCultureInfo('fr-LU', { language: 'fr', countryFlag: 'lu', englishName: 'French (Luxembourg)', nativeName: 'Français (Luxembourg)' });
    VRS.globalisation.addCultureInfo('fr-MC', { language: 'fr', countryFlag: 'mc', englishName: 'French (Monaco)', nativeName: 'Français (Principauté de Monaco)' });
    // Russian
    VRS.globalisation.addCultureInfo('ru', { language: 'ru', englishName: 'Russian', nativeName: 'Русский', topLevel: true });
    VRS.globalisation.addCultureInfo('ru-RU', { language: 'ru', englishName: 'Russian (Russia)', nativeName: 'Русский (Россия)' });
    // Chinese
    VRS.globalisation.addCultureInfo('zh', { language: 'zh', englishName: 'Chinese', nativeName: '中文', topLevel: true });
    VRS.globalisation.addCultureInfo('zh-CN', { language: 'zh', englishName: 'Chinese (China)', nativeName: '中文 (中国)' });
    // Portuguese
    VRS.globalisation.addCultureInfo('pt-BR', { language: 'pt-BR', groupLanguage: 'pt', englishName: 'Portuguese (Brazil)', nativeName: 'Português (Brasil)', countryFlag: 'br', topLevel: true });
})(VRS || (VRS = {}));
//# sourceMappingURL=localise.js.map