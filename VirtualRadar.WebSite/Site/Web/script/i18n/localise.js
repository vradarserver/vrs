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

(function(VRS, $, undefined)
{
    //region CultureInfo
    VRS.CultureInfo = function(locale, settings)
    /**
     * Describes some basic information about a culture.
     * @param {string}       locale                         The name of the locale (e.g. en-GB).
     * @param {object}       settings
     * @param {string}      [settings.forceCultureName]     The culture name to use if locale is not appropriate.
     * @param {string}       settings.language              The ISO-2 code for the language.
     * @param {string}      [settings.flagImage]            The URL of the flag image for the culture.
     * @param {string}      [settings.countryFlag]          The country flag name for the locale.
     * @param {VRS_SIZE=}   [settings.flagSize]             The dimensions of the flag image.
     * @param {string}       settings.englishName           The English name of the culture.
     * @param {string}      [settings.nativeName]           The native language name of the culture.
     * @constructor
     */
    {
        var that = this;

        this.locale = locale;
        this.cultureName = settings.forceCultureName || locale;
        this.language = settings.language;
        this.flagImage = settings.flagImage || ('images/regions/' + (settings.countryFlag ? settings.countryFlag : settings.language) + '.bmp');
        this.flagSize = settings.flagSize || { width: 20, height: 16 };
        this.englishName = settings.englishName;
        this.nativeName = settings.nativeName || this.englishName;

        this.getFlagImageHtml = function()
        {
            var result = '';
            if(that.flagImage && that.flagSize) {
                result = '<img src="' + that.flagImage + '" width="' + that.flagSize.width + 'px" height="' + that.flagSize.height + 'px" alt="' + that.nativeName + '" />';
            }

            return result;
        };
    };
    //endregion

    //region Localise
    /**
     * The class that handles the selection and loading of locales for VRS.
     * @constructor
     */
    VRS.Localise = function()
    {
        //region -- Fields
        var that = this;

        /**
         * The currently loaded language file substring (e.g. strings.en.js would be 'en').
         * @type {string}
         * @private
         */
        var _LoadedLanguage;
        /**
         * An associative array of region names (e.g. en-GB) and an object describing the settings associated with that region.
         * @type {Object.<string, VRS.CultureInfo>}
         * @private
         */
        var _CultureInfos = {};
        /**
         * An associative array of loaded Globalize files indexed by region name (e.g. en-GB).
         * @type {Object.<string, bool>}
         * @private
         */
        var _LoadedGlobalizations = {};
        /**
         * The event dispatcher.
         * @type {VRS.EventHandler}
         * @private
         */
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.Localise'
        });
        /**
         * An associative array of event names.
         * @type {Object.<string>}
         * @private
         */
        var _Events = {
            localeChanged: 'localeChanged'
        };
        //endregion

        //region -- Properties
        /**
         * The currently selected locale.
         * @type {string}
         * @private
         */
        var _Locale = '';
        /**
         * Gets the currently selected locale. Locales are full .NET region codes, e.g. 'en-GB' for British English.
         * @returns {string}
         */
        this.getLocale = function() { return _Locale; };
        /**
         * Sets the locale by region code.
         * @param {string} value
         */
        this.setLocale = function(value) {
            if(value !== _Locale) {
                _Locale = value;
                var cultureInfo = _CultureInfos[_Locale];
                if(cultureInfo) {
                    loadLanguage('en');     // English is the base language, if other language files don't supply a string then the English version should be used instead.
                    loadLanguage(cultureInfo.language);
                    loadCulture(cultureInfo.cultureName);
                    Globalize.culture(cultureInfo.cultureName);
                    _Dispatcher.raise(_Events.localeChanged);
                }
            }
        };
        //endregion

        //region -- Events exposed
        /**
         * Hooks the event raised after the locale has changed.
         * @param callback
         * @param [forceThis]
         * @returns {{}}
         */
        this.hookLocaleChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.localeChanged, callback, forceThis); };

        /**
         * Unhooks any event hooked on this object.
         * @param hookResult
         * @returns {*}
         */
        this.unhook = function(hookResult) { return _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            var settings = createSettings();
            VRS.configStorage.saveWithoutPrefix('Localise', settings);
        };

        /**
         * Loads the saved state of the object.
         * @returns {{}}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.loadWithoutPrefix('Localise', {});
            var result = $.extend(createSettings(), savedSettings);
            if(!result.locale || !_CultureInfos[result.locale]) result.locale = guessBrowserLocale();

            return result;
        };

        /**
         * Applies a saved state to the object.
         * @param config
         */
        this.applyState = function(config)
        {
            config = config || {};
            this.setLocale(config.locale || 'en');
        };

        /**
         * Loads and then applies the saved state to the object.
         */
        this.loadAndApplyState = function()
        {
            this.applyState(this.loadState());
        };

        /**
         * Creates the saved state object.
         * @returns {{locale: string}}
         */
        function createSettings()
        {
            return {
                locale: _Locale
            };
        }
        //endregion

        //region -- loadLanguage, loadCulture
        /**
         * Loads a language script (i.e. translations of text) from the server.
         * @param language The ISO-2 code(e.g. 'en') for the language to load.
         */
        function loadLanguage(language)
        {
            if(language !== _LoadedLanguage) {
                var url = 'script/i18n/strings.' + language + '.js';
                VRS.scriptManager.loadScript({ url: url });
                _LoadedLanguage = language;
            }
        }

        /**
         * Loads a culture script (i.e. number and date formatting) from the server.
         * @param cultureName The name of the culture (e.g. 'en-GB') to load from the server.
         */
        function loadCulture(cultureName)
        {
            if(!_LoadedGlobalizations[cultureName]) {
                var url = 'script/i18n/globalize/globalize.culture.' + cultureName + '.js';
                VRS.scriptManager.loadScript({ url: url });
                _LoadedGlobalizations[cultureName] = true;
            }
        }
        //endregion

        //region -- guessBrowserLocale
        /**
         * Returns a best-guess at the current region code for the browser or en-GB if either no code could be reliably figured out or
         * if we don't have any information about the region.
         * @returns {string}
         */
        function guessBrowserLocale()
        {
            var result = navigator.userLanguage || navigator.systemLanguage || navigator.browserLanguage || navigator.language;
            if(!result) result = 'en-GB';
            if(!_CultureInfos[result]) {
                // If we know the base language (e.g. the 'en' in 'en-??') then use it
                var hyphenPos = result.indexOf('-');
                var language = hyphenPos === -1 ? null : result.substr(0, hyphenPos);
                if(language && _CultureInfos[language]) result = language;
                else result = 'en-GB';
            }

            return result;
        }
        //endregion

        //region -- addCultureInfo, getCultureInfo, removeCultureInfo, getCultureInfos
        /**
         * Adds culture information to the object.
         * @param {string} cultureName The region code to add (e.g. en-GB).
         * @param {{}} info Information about the culture.
         * @param {string} [info.forceCultureName]              The culture name to use if locale is not appropriate.
         * @param {string} info.language                        The ISO-2 code for the language.
         * @param {string} [info.flagImage]                     The URL of the flag image for the culture.
         * @param {{width: Number, height: Number}} [info.flagSize] The dimensions of the flag image.
         * @param {string} info.englishName                     The English name of the culture.
         * @param {string} [info.nativeName]                    The native language name of the culture.
         */
        this.addCultureInfo = function(cultureName, info)
        {
            if(!_CultureInfos[cultureName]) _CultureInfos[cultureName] = new VRS.CultureInfo(cultureName, info);
        };

        /**
         * Returns the currently selected culture's information or the information about any known culture.
         * @param {string} [cultureName]    If supplied then the culture with the matching name (e.g. en-GB) is returned.
         * @returns {VRS.CultureInfo}
         */
        this.getCultureInfo = function(cultureName)
        {
            return _CultureInfos[cultureName || _Locale];
        };

        /**
         * Removes the information about a culture.
         * @param {string} cultureName The name (e.g. en-GB) of the culture to remove.
         */
        this.removeCultureInfo = function(cultureName)
        {
            if(_CultureInfos[cultureName]) delete _CultureInfos[cultureName];
        };

        /**
         * Returns an array of every known culture.
         * @returns {VRS.CultureInfo[]}
         */
        this.getCultureInfos = function()
        {
            var result = [];

            for(var locale in _CultureInfos) {
                var cultureInfo = _CultureInfos[locale];
                if(cultureInfo instanceof VRS.CultureInfo) result.push(cultureInfo);
            }

            return result;
        };

        /**
         * Returns an array of arrays of cultures, grouped by language.
         * @param {boolean} [sortByNativeName]          True if the arrays should be sorted by native name before returning.
         * @returns {Array.<Array.<VRS.CultureInfo>>}
         */
        this.getCultureInfosGroupedByLanguage = function(sortByNativeName)
        {
            var result = [];

            $.each(that.getCultureInfos(), function(/** Number */ idx, /** VRS.CultureInfo */ cultureInfo) {
                var language = cultureInfo.language;
                var innerArray = VRS.arrayHelper.findFirst(result, function(r) { return r[0].language === language; }, null);
                if(!innerArray) {
                    innerArray = [];
                    result.push(innerArray);
                }
                innerArray.push(cultureInfo);
            });

            if(sortByNativeName) {
                $.each(result, function(/** Number */idx, /** Array.<VRS.CultureInfo> */ cultureArray) {
                    cultureArray.sort(function(/** VRS.CultureInfo */ lhs, /** VRS.CultureInfo */ rhs) {
                        return lhs.nativeName.localeCompare(rhs.nativeName);
                    });
                });
                result.sort(function(/** Array.<VRS.CultureInfo> */ lhs, /** Array.<VRS.CultureInfo> */ rhs) {
                    return lhs[0].nativeName.localeCompare(rhs[0].nativeName);
                });
            }

            return result;
        };
        //endregion

        //region -- getRawGlobalizeData
        /**
         * Returns the raw culture object for the current locale. Throws an exception if fails.
         * @returns {*}
         */
        function getRawGlobalizeData()
        {
            var result = Globalize.findClosestCulture();
            if(!result) throw 'Could not find the current Globalize culture';

            return result;
        }
        //endregion

        //region -- getText
        /**
         * Either returns the translated text associated with the text key passed across or, if the parameter is a function, calls that to obtain the translated text.
         * @param {string|function():string} keyOrFormatFunction The index into VRS.$$ or the function to call to obtain the translated text.
         * @returns {string} The translated text.
         */
        this.getText = function(keyOrFormatFunction)
        {
            if(keyOrFormatFunction instanceof Function) return keyOrFormatFunction();
            return VRS.$$[keyOrFormatFunction];
        };
        //endregion

        //region -- localiseDatePicker, getDatePickerOptions
        /**
         * Configures the date picker with the current locale's date formatting styles.
         * @param {jQuery} datePickerJQ     A jQuery element that has a date picker attached.
         */
        this.localiseDatePicker = function(datePickerJQ)
        {
            var options = that.getDatePickerOptions();
            datePickerJQ.datepicker('option', options);
        };

        /**
         * Returns an object containing jQueryUI datepicker options that match the current locale. The jQuery UI
         * datepicker does have its own localisation support but (a) it involves loading more JS modules for it, (b) it
         * duplicates some information in Globalization's culture files and (c) it is not guaranteed to be consistent
         * with the date formats in Globalization. I don't want to have a mixture of different localisation mechanisms
         * if I can help it.
         * @returns {Object}
         */
        this.getDatePickerOptions = function()
        {
            var culture = getRawGlobalizeData();
            var calendar = culture.calendars.standard;
            var months = VRS.$$.DateUseGenetiveMonths && calendar.monthsGenitive ? calendar.monthsGenitive : calendar.months;

            var shortYear = calendar.shortYearCutoff;
            if(Object.prototype.toString.call(shortYear) !== '[object String]') shortYear %= 100;

            var monthYearPattern = calendar.patterns['Y'] || 'MMMM yyyy';
            var showMonthAfterYear = monthYearPattern[0] === 'y';

            return {
                closeText:          VRS.$$.DateClose,
                currentText:        VRS.$$.DateCurrent,
                dateFormat:         dotNetDateFormatToJQueryDateFormat(calendar.patterns['d']),
                dayNames:           calendar.days.names,
                dayNamesMin:        calendar.days.namesShort,
                dayNamesShort:      calendar.days.namesAbbr,
                firstDay:           calendar.firstDay,
                isRTL:              culture.isRTL,
                shortYearCutoff:    shortYear,
                showMonthAfterYear: showMonthAfterYear,
                monthNames:         months.names,
                monthNamesShort:    months.namesAbbr,
                nextText:           VRS.$$.DateNext,
                prevText:           VRS.$$.DatePrevious,
                weekHeader:         VRS.$$.DateWeekAbbr,
                yearSuffix:         VRS.$$.DateYearSuffix
            };
        };

        /**
         * Takes a format in .NET format and returns the equivalent date format in JQuery UI's date picker format.
         * @param {string} dateFormat
         * @returns {string}
         */
        function dotNetDateFormatToJQueryDateFormat(dateFormat)
        {
            // We're a little hampered here by not having look behind in regex, hence the ugly switching to known text.
            // As these are date formats coming out of Globalize files it's very unlikely that these marker strings
            // will appear in the Globalization formats.
            var fullMonthMarker = 'FMONTH';
            var shortMonthMarker = 'SMONTH';
            var fullYearMarker = 'FYEAR';

            return dateFormat
                .replace('dddd',            'DD')
                .replace('ddd',             'D')
                .replace('MMMM',            fullMonthMarker)
                .replace('MMM',             shortMonthMarker)
                .replace('MM',              'mm')
                .replace('M',               'm')
                .replace(fullMonthMarker,   'MM')
                .replace(shortMonthMarker,  'M')
                .replace('yyyy',            fullYearMarker)
                .replace('yy',              'y')
                .replace(fullYearMarker,    'yy');
        }
        //endregion

        //region -- setLocaleInBackground
        /**
         * Sets the locale in the background. The loading of globalization files from the server takes a while, if it's
         * performed on a UI action like a button click it can feel unresponsive. This method wraps that in a function
         * that is called from a timer on a short delay and optionally adds a modal wait animation so that the UI can
         * be made to feel more responsive.
         * @param {string}          locale                  The locale to select.
         * @param {boolean}        [showModalWait]          True if the modal wait animation is to play while the locale is loaded. Defaults to true.
         * @param {function()}     [localeLoadedCallback]   An optional method that is called once the locale has been loaded and any modal wait animation removed.
         */
        this.setLocaleInBackground = function(locale, showModalWait, localeLoadedCallback)
        {
            var self = this;
            if(showModalWait === undefined) showModalWait = true;
            if(showModalWait) VRS.pageHelper.showModalWaitAnimation(true);
            setTimeout(function() {
                self.setLocale(locale);
                if(showModalWait) VRS.pageHelper.showModalWaitAnimation(false);
                if(localeLoadedCallback) localeLoadedCallback();
            }, 50);
        };
        //endregion
    };
    //endregion

    //region Pre-builts
    /**
     * The singleton instance of VRS.Localise.
     * @type {VRS.Localise}
     */
    VRS.globalisation = new VRS.Localise();

    // English
    VRS.globalisation.addCultureInfo('en',      { language: 'en',                    englishName: 'English', forceCultureName: 'en-GB' });  // Globalize uses American settings for the default 'en' language. This is a British program :P
    VRS.globalisation.addCultureInfo('en-029',  { language: 'en',                    englishName: 'English (Caribbean)' });
    VRS.globalisation.addCultureInfo('en-AU',   { language: 'en', countryFlag: 'au', englishName: 'English (Australia)' });
    VRS.globalisation.addCultureInfo('en-BZ',   { language: 'en', countryFlag: 'bz', englishName: 'English (Belize)' });
    VRS.globalisation.addCultureInfo('en-CA',   { language: 'en', countryFlag: 'ca', englishName: 'English (Canada)' });
    VRS.globalisation.addCultureInfo('en-GB',   { language: 'en',                    englishName: 'English (United Kingdom)' });
    VRS.globalisation.addCultureInfo('en-IE',   { language: 'en', countryFlag: 'ie', englishName: 'English (Ireland)' });
    VRS.globalisation.addCultureInfo('en-IN',   { language: 'en', countryFlag: 'in', englishName: 'English (India)' });
    VRS.globalisation.addCultureInfo('en-JM',   { language: 'en', countryFlag: 'jm', englishName: 'English (Jamaica)' });
    VRS.globalisation.addCultureInfo('en-MY',   { language: 'en', countryFlag: 'my', englishName: 'English (Malaysia)' });
    VRS.globalisation.addCultureInfo('en-NZ',   { language: 'en', countryFlag: 'nz', englishName: 'English (New Zealand)' });
    VRS.globalisation.addCultureInfo('en-SG',   { language: 'en', countryFlag: 'sg', englishName: 'English (Singapore)' });
    VRS.globalisation.addCultureInfo('en-TT',   { language: 'en', countryFlag: 'tt', englishName: 'English (Trinidad and Tobago)', nativeName: 'English (Trinidad y Tobago)' });
    VRS.globalisation.addCultureInfo('en-US',   { language: 'en', countryFlag: 'us', englishName: 'English (United States)' });
    VRS.globalisation.addCultureInfo('en-ZA',   { language: 'en', countryFlag: 'za', englishName: 'English (South Africa)' });

    // French
    VRS.globalisation.addCultureInfo('fr',    { language: 'fr',                    englishName: 'French',                  nativeName: 'Français' });
    VRS.globalisation.addCultureInfo('fr-BE', { language: 'fr', countryFlag: 'be', englishName: 'French (Belgium)',        nativeName: 'Français (Belgique)' });
    VRS.globalisation.addCultureInfo('fr-CA', { language: 'fr', countryFlag: 'ca', englishName: 'French (Canada)',         nativeName: 'Français (Canada)' });
    VRS.globalisation.addCultureInfo('fr-CH', { language: 'fr', countryFlag: 'ch', englishName: 'French (Switzerland)',    nativeName: 'Français (Suisse)' });
    VRS.globalisation.addCultureInfo('fr-FR', { language: 'fr',                    englishName: 'French (France)',         nativeName: 'Français (France)' });
    VRS.globalisation.addCultureInfo('fr-LU', { language: 'fr', countryFlag: 'lu', englishName: 'French (Luxembourg)',     nativeName: 'Français (Luxembourg)' });
    VRS.globalisation.addCultureInfo('fr-MC', { language: 'fr', countryFlag: 'mc', englishName: 'French (Monaco)',         nativeName: 'Français (Principauté de Monaco)' });

    // Russian
    VRS.globalisation.addCultureInfo('ru',    { language: 'ru', englishName: 'Russian',                 nativeName: 'Русский' });
    VRS.globalisation.addCultureInfo('ru-RU', { language: 'ru', englishName: 'Russian (Russia)',        nativeName: 'Русский (Россия)' });

    //endregion
}(window.VRS = window.VRS || {}, jQuery));