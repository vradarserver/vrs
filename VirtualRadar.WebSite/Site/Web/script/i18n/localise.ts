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

namespace VRS
{
    /**
     * The settings that can be passed when creating an instance of CultureInfo.
     */
    export interface CultureInfo_Settings
    {
        /**
         * The ISO-2 code for the language.
         */
        language:               string;

        /**
         * The culture name to use if locale is not appropriate.
         */
        forceCultureName?:      string;

        /**
         * The URL of the flag image for the culture.
         */
        flagImage?:             string;

        /**
         * The country flag name for the locale.
         */
        countryFlag?:           string;

        /**
         * The dimensions of the flag image.
         */
        flagSize?:              ISize;

        /**
         * The English name of the culture.
         */
        englishName:            string;

        /**
         * The native language name of the culture.
         */
        nativeName?:            string;

        /**
         * True if this is a top-level language description.
         */
        topLevel?:              boolean;

        /**
         * The language to use when grouping localisations together.
         */
        groupLanguage?:         string;
    }

    /**
     * Describes some basic information about a culture.
     */
    export class CultureInfo
    {
        private _Locale: string;
        private _CultureName: string;
        private _Language: string;
        private _FlagImage: string;
        private _FlagSize: ISize;
        private _EnglishName: string;
        private _NativeName: string;
        private _TopLevel: boolean;
        private _GroupLanguage: string;

        constructor(locale: string, settings: CultureInfo_Settings)
        {
            this._Locale = locale;
            this._CultureName = settings.forceCultureName || locale;
            this._Language = settings.language;
            this._FlagImage = settings.flagImage || ('images/regions/' + (settings.countryFlag ? settings.countryFlag : settings.language) + '.bmp');
            this._FlagSize = settings.flagSize || { width: 20, height: 16 };
            this._EnglishName = settings.englishName;
            this._NativeName = settings.nativeName || this._EnglishName;
            this._TopLevel = settings.topLevel !== undefined ? settings.topLevel : false;
            this._GroupLanguage = settings.groupLanguage || settings.language;
        }

        get locale()
        {
            return this._Locale;
        }

        get cultureName()
        {
            return this._CultureName;
        }

        get language()
        {
            return this._Language;
        }

        get flagImage()
        {
            return this._FlagImage;
        }

        get flagSize()
        {
            return this._FlagSize;
        }

        get englishName()
        {
            return this._EnglishName;
        }

        get nativeName()
        {
            return this._NativeName;
        }

        get topLevel()
        {
            return this._TopLevel;
        }

        get groupLanguage()
        {
            return this._GroupLanguage;
        }

        getFlagImageHtml() : string
        {
            var result = '';
            if(this._FlagImage && this._FlagSize) {
                result = '<img src="' + this._FlagImage + '" width="' + this._FlagSize.width + 'px" height="' + this._FlagSize.height + 'px" alt="' + this._NativeName + '" />';
            }

            return result;
        }
    }

    /**
     * Describes the stored settings for the Localise object.
     */
    export interface Localise_SaveState
    {
        locale?: string;
    }

    /**
     * The class that handles the selection and loading of locales for VRS.
     */
    export class Localise
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.Localise'
        });
        private _Events = {
            localeChanged: 'localeChanged'
        };

        private _LoadedLanguage: string;                                        // The currently loaded language file substring (e.g. strings.en.js would be 'en').
        private _CultureInfos: { [cultureName: string]: CultureInfo } = {};     // An associative array of region names (e.g. en-GB) and an object describing the settings associated with that region.
        private _LoadedGlobalizations: { [cultureName: string]: boolean } = {}; // An associative array of loaded Globalize files indexed by region name (e.g. en-GB).
        private _Locale = '';                                                   // The currently selected locale.

        /**
         * Gets the currently selected locale. Locales are full region codes, e.g. 'en-GB' for British English.
         */
        getLocale() : string
        {
            return this._Locale;
        }
        setLocale(value: string, successCallback: () => void)
        {
            if(value === this._Locale) {
                if(successCallback) {
                    successCallback();
                }
            } else {
                this._Locale = value;
                var cultureInfo = this._CultureInfos[this._Locale];
                if(cultureInfo) {
                    // English is the base language, if other language files don't supply a string then the English version should be used instead.
                    this.loadLanguage('en', () => {
                        this.loadLanguage(cultureInfo.language, () => {
                            this.loadCulture(cultureInfo.cultureName, () => {
                                Globalize.culture(cultureInfo.cultureName);
                                this._Dispatcher.raise(this._Events.localeChanged);

                                if(successCallback) {
                                    successCallback();
                                }
                            });
                        });
                    });
                }
            }
        }

        /**
         * Hooks the event raised after the locale has changed.
         */
        hookLocaleChanged(callback: () => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.localeChanged, callback, forceThis);
        }

        /**
         * Unhooks any event hooked on this object.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Saves the current state of the object.
         */
        saveState()
        {
            var settings = this.createSettings();
            VRS.configStorage.saveWithoutPrefix('Localise', settings);
        }

        /**
         * Loads the saved state of the object.
         */
        loadState() : Localise_SaveState
        {
            var savedSettings = VRS.configStorage.loadWithoutPrefix('Localise', {});
            var result = $.extend(this.createSettings(), savedSettings);
            if(!result.locale || !this._CultureInfos[result.locale]) result.locale = this.guessBrowserLocale();

            return result;
        }

        /**
         * Applies a saved state to the object.
         */
        applyState(config: Localise_SaveState, successCallback: () => void)
        {
            config = config || {};
            this.setLocale(config.locale || 'en', successCallback);
        }

        /**
         * Loads and then applies the saved state to the object.
         */
        loadAndApplyState(successCallback: () => void)
        {
            this.applyState(this.loadState(), successCallback);
        }

        /**
         * Creates the saved state object.
         */
        private createSettings() : Localise_SaveState
        {
            return {
                locale: this._Locale
            };
        }

        /**
         * Loads a language script (i.e. translations of text) from the server.
         */
        private loadLanguage(language: string, successCallback: () => void)
        {
            if(language === this._LoadedLanguage) {
                if(successCallback) {
                    successCallback();
                }
            } else {
                var url = 'script/i18n/strings.' + language.toLowerCase() + '.js';
                VRS.scriptManager.loadScript({ url: url, success: () => {
                    this._LoadedLanguage = language;
                    if(successCallback) {
                        successCallback();
                    }
                }});
            }
        }

        /**
         * Loads a culture script (i.e. number and date formatting) from the server.
         */
        private loadCulture(cultureName: string, successCallback: () => void)
        {
            if(this._LoadedGlobalizations[cultureName]) {
                if(successCallback) {
                    successCallback();
                }
            } else {
                var url = 'script/i18n/globalize/globalize.culture.' + cultureName + '.js';
                VRS.scriptManager.loadScript({ url: url, success: () => {
                    this._LoadedGlobalizations[cultureName] = true;
                    if(successCallback) {
                        successCallback();
                    }
                }});
            }
        }

        /**
         * Returns a best-guess at the current region code for the browser or en-GB if either no code could be reliably figured out or
         * if we don't have any information about the region.
         */
        private guessBrowserLocale() : string
        {
            var result = navigator.userLanguage || navigator.systemLanguage || navigator.browserLanguage || navigator.language;
            if(!result) result = 'en-GB';
            if(!this._CultureInfos[result]) {
                // If we know the base language (e.g. the 'en' in 'en-??') then use it
                var hyphenPos = result.indexOf('-');
                var language = hyphenPos === -1 ? null : result.substr(0, hyphenPos);
                if(language && this._CultureInfos[language]) {
                    result = language;
                } else {
                    result = 'en-GB';
                }
            }

            return result;
        }

        /**
         * Adds culture information to the object.
         */
        addCultureInfo(cultureName: string, settings: CultureInfo_Settings)
        {
            if(!this._CultureInfos[cultureName]) {
                this._CultureInfos[cultureName] = new VRS.CultureInfo(cultureName, settings);
            }
        }

        /**
         * Returns the currently selected culture's information or the information about any known culture.
         */
        getCultureInfo(cultureName?: string) : CultureInfo
        {
            return this._CultureInfos[cultureName || this._Locale];
        }

        /**
         * Removes the information about a culture.
         */
        removeCultureInfo(cultureName: string)
        {
            if(this._CultureInfos[cultureName]) {
                delete this._CultureInfos[cultureName];
            }
        }

        /**
         * Returns an array of every known culture.
         */
        getCultureInfos() : CultureInfo[]
        {
            var result: CultureInfo[] = [];

            for(var locale in this._CultureInfos) {
                var cultureInfo = this._CultureInfos[locale];
                if(cultureInfo instanceof VRS.CultureInfo) {
                    result.push(cultureInfo);
                }
            }

            return result;
        }

        /**
         * Returns an array of arrays of cultures, grouped by language.
         */
        getCultureInfosGroupedByLanguage(sortByNativeName: boolean) : CultureInfo[][]
        {
            var result: CultureInfo[][] = [];

            $.each(this.getCultureInfos(), function(idx, cultureInfo) {
                var language = cultureInfo.groupLanguage;
                var innerArray = VRS.arrayHelper.findFirst(result, function(r) { return r[0].groupLanguage === language; }, null);
                if(!innerArray) {
                    innerArray = [];
                    result.push(innerArray);
                }
                innerArray.push(cultureInfo);
            });

            if(sortByNativeName) {
                $.each(result, function(idx, cultureArray) {
                    cultureArray.sort(function(lhs, rhs) {
                        return lhs.nativeName.localeCompare(rhs.nativeName);
                    });
                });
                result.sort(function(lhs, rhs) {
                    return lhs[0].nativeName.localeCompare(rhs[0].nativeName);
                });
            }

            return result;
        }

        /**
         * Returns the raw culture object for the current locale. Throws an exception if fails.
         */
        private getRawGlobalizeData() : GlobalizeCulture
        {
            var result = Globalize.findClosestCulture();
            if(!result) throw 'Could not find the current Globalize culture';

            return result;
        }

        /**
         * Either returns the translated text associated with the text key passed across or, if the parameter is a function, calls that to obtain the translated text.
         */
        getText(keyOrFormatFunction: string | VoidFuncReturning<string>)
        {
            if(keyOrFormatFunction instanceof Function) {
                return keyOrFormatFunction();
            }
            return VRS.$$[<string>keyOrFormatFunction];
        }

        /**
         * Configures the date picker with the current locale's date formatting styles.
         */
        localiseDatePicker(datePickerJQ: JQuery)
        {
            var options = this.getDatePickerOptions();
            datePickerJQ.datepicker('option', options);
        }

        /**
         * Returns an object containing jQueryUI datepicker options that match the current locale. The jQuery UI
         * datepicker does have its own localisation support but (a) it involves loading more JS modules for it, (b) it
         * duplicates some information in Globalization's culture files and (c) it is not guaranteed to be consistent
         * with the date formats in Globalization. I don't want to have a mixture of different localisation mechanisms
         * if I can help it.
         */
        getDatePickerOptions() : JQueryUI.DatepickerOptions
        {
            var culture = this.getRawGlobalizeData();
            var calendar = culture.calendars.standard;
            var months = VRS.$$.DateUseGenetiveMonths && calendar.monthsGenitive ? calendar.monthsGenitive : calendar.months;

            var shortYear = <number>calendar.shortYearCutoff;
            if(Object.prototype.toString.call(shortYear) !== '[object String]') shortYear %= 100;

            var monthYearPattern = calendar.patterns['Y'] || 'MMMM yyyy';
            var showMonthAfterYear = monthYearPattern[0] === 'y';

            return {
                closeText:          VRS.$$.DateClose,
                currentText:        VRS.$$.DateCurrent,
                dateFormat:         this.dotNetDateFormatToJQueryDateFormat(calendar.patterns['d']),
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
        }

        /**
         * Takes a format in .NET format and returns the equivalent date format in JQuery UI's date picker format.
         */
        private dotNetDateFormatToJQueryDateFormat(dateFormat: string) : string
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

        /**
         * Sets the locale in the background. The loading of globalization files from the server takes a while, if it's
         * performed on a UI action like a button click it can feel unresponsive. This method wraps that in a function
         * that is called from a timer on a short delay and optionally adds a modal wait animation so that the UI can
         * be made to feel more responsive.
         */
        setLocaleInBackground(locale: string, showModalWait: boolean = true, localeLoadedCallback?: () => void)
        {
            if(showModalWait) VRS.pageHelper.showModalWaitAnimation(true);
            this.setLocale(locale, function() {
                if(showModalWait) VRS.pageHelper.showModalWaitAnimation(false);
                if(localeLoadedCallback) localeLoadedCallback();
            });
        }
    }

    /*
     * Pre-builts
     */
    export var globalisation = new VRS.Localise();

    // English
    VRS.globalisation.addCultureInfo('en',      { language: 'en',                    englishName: 'English', forceCultureName: 'en-GB', topLevel: true });  // Globalize uses American settings for the default 'en' language. This is a British program :P
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

    // German
    VRS.globalisation.addCultureInfo('de',    { language: 'de',                    englishName: 'German',           nativeName: 'Deutsch', topLevel: true });
    VRS.globalisation.addCultureInfo('de-DE', { language: 'de', countryFlag: 'de', englishName: 'German (Germany)', nativeName: 'Deutsch (Deutschland)' });

    // French
    VRS.globalisation.addCultureInfo('fr',    { language: 'fr',                    englishName: 'French',                  nativeName: 'Français', topLevel: true });
    VRS.globalisation.addCultureInfo('fr-BE', { language: 'fr', countryFlag: 'be', englishName: 'French (Belgium)',        nativeName: 'Français (Belgique)' });
    VRS.globalisation.addCultureInfo('fr-CA', { language: 'fr', countryFlag: 'ca', englishName: 'French (Canada)',         nativeName: 'Français (Canada)' });
    VRS.globalisation.addCultureInfo('fr-CH', { language: 'fr', countryFlag: 'ch', englishName: 'French (Switzerland)',    nativeName: 'Français (Suisse)' });
    VRS.globalisation.addCultureInfo('fr-FR', { language: 'fr',                    englishName: 'French (France)',         nativeName: 'Français (France)' });
    VRS.globalisation.addCultureInfo('fr-LU', { language: 'fr', countryFlag: 'lu', englishName: 'French (Luxembourg)',     nativeName: 'Français (Luxembourg)' });
    VRS.globalisation.addCultureInfo('fr-MC', { language: 'fr', countryFlag: 'mc', englishName: 'French (Monaco)',         nativeName: 'Français (Principauté de Monaco)' });

    // Russian
    VRS.globalisation.addCultureInfo('ru',    { language: 'ru', englishName: 'Russian',                 nativeName: 'Русский', topLevel: true });
    VRS.globalisation.addCultureInfo('ru-RU', { language: 'ru', englishName: 'Russian (Russia)',        nativeName: 'Русский (Россия)' });

    // Chinese
    VRS.globalisation.addCultureInfo('zh',    { language: 'zh', englishName: 'Chinese',                 nativeName: '中文', topLevel: true });
    VRS.globalisation.addCultureInfo('zh-CN', { language: 'zh', englishName: 'Chinese (China)',         nativeName: '中文 (中国)' });

    // Portuguese
    VRS.globalisation.addCultureInfo('pt-BR',   { language: 'pt-BR', groupLanguage: 'pt', englishName: 'Portuguese (Brazil)', nativeName: 'Português (Brasil)',  countryFlag: 'br', topLevel: true });
}
