var VRS;
(function (VRS) {
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
    var Localise = (function () {
        function Localise() {
            this._CultureInfos = {};
            this._LoadedGlobalizations = {};
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.Localise'
            });
            this._Events = {
                localeChanged: 'localeChanged'
            };
            this._Locale = '';
        }
        Localise.prototype.getLocale = function () {
            return this._Locale;
        };
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
        Localise.prototype.hookLocaleChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.localeChanged, callback, forceThis);
        };
        Localise.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        Localise.prototype.saveState = function () {
            var settings = this.createSettings();
            VRS.configStorage.saveWithoutPrefix('Localise', settings);
        };
        Localise.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.loadWithoutPrefix('Localise', {});
            var result = $.extend(this.createSettings(), savedSettings);
            if (!result.locale || !this._CultureInfos[result.locale])
                result.locale = this.guessBrowserLocale();
            return result;
        };
        Localise.prototype.applyState = function (config, successCallback) {
            config = config || {};
            this.setLocale(config.locale || 'en', successCallback);
        };
        Localise.prototype.loadAndApplyState = function (successCallback) {
            this.applyState(this.loadState(), successCallback);
        };
        Localise.prototype.createSettings = function () {
            return {
                locale: this._Locale
            };
        };
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
        Localise.prototype.guessBrowserLocale = function () {
            var result = navigator.userLanguage || navigator.systemLanguage || navigator.browserLanguage || navigator.language;
            if (!result)
                result = 'en-GB';
            if (!this._CultureInfos[result]) {
                var hyphenPos = result.indexOf('-');
                var language = hyphenPos === -1 ? null : result.substr(0, hyphenPos);
                if (language && this._CultureInfos[language])
                    result = language;
                else
                    result = 'en-GB';
            }
            return result;
        };
        Localise.prototype.addCultureInfo = function (cultureName, settings) {
            if (!this._CultureInfos[cultureName])
                this._CultureInfos[cultureName] = new VRS.CultureInfo(cultureName, settings);
        };
        Localise.prototype.getCultureInfo = function (cultureName) {
            return this._CultureInfos[cultureName || this._Locale];
        };
        Localise.prototype.removeCultureInfo = function (cultureName) {
            if (this._CultureInfos[cultureName])
                delete this._CultureInfos[cultureName];
        };
        Localise.prototype.getCultureInfos = function () {
            var result = [];
            for (var locale in this._CultureInfos) {
                var cultureInfo = this._CultureInfos[locale];
                if (cultureInfo instanceof VRS.CultureInfo)
                    result.push(cultureInfo);
            }
            return result;
        };
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
        Localise.prototype.localiseDatePicker = function (datePickerJQ) {
            var options = this.getDatePickerOptions();
            datePickerJQ.datepicker('option', options);
        };
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
        Localise.prototype.dotNetDateFormatToJQueryDateFormat = function (dateFormat) {
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
    VRS.globalisation = new VRS.Localise();
    VRS.globalisation.addCultureInfo('en', { language: 'en', englishName: 'English', forceCultureName: 'en-GB', topLevel: true });
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
    VRS.globalisation.addCultureInfo('de', { language: 'de', englishName: 'German', nativeName: 'Deutsch', topLevel: true });
    VRS.globalisation.addCultureInfo('de-DE', { language: 'de', countryFlag: 'de', englishName: 'German (Germany)', nativeName: 'Deutsch (Deutschland)' });
    VRS.globalisation.addCultureInfo('fr', { language: 'fr', englishName: 'French', nativeName: 'Français', topLevel: true });
    VRS.globalisation.addCultureInfo('fr-BE', { language: 'fr', countryFlag: 'be', englishName: 'French (Belgium)', nativeName: 'Français (Belgique)' });
    VRS.globalisation.addCultureInfo('fr-CA', { language: 'fr', countryFlag: 'ca', englishName: 'French (Canada)', nativeName: 'Français (Canada)' });
    VRS.globalisation.addCultureInfo('fr-CH', { language: 'fr', countryFlag: 'ch', englishName: 'French (Switzerland)', nativeName: 'Français (Suisse)' });
    VRS.globalisation.addCultureInfo('fr-FR', { language: 'fr', englishName: 'French (France)', nativeName: 'Français (France)' });
    VRS.globalisation.addCultureInfo('fr-LU', { language: 'fr', countryFlag: 'lu', englishName: 'French (Luxembourg)', nativeName: 'Français (Luxembourg)' });
    VRS.globalisation.addCultureInfo('fr-MC', { language: 'fr', countryFlag: 'mc', englishName: 'French (Monaco)', nativeName: 'Français (Principauté de Monaco)' });
    VRS.globalisation.addCultureInfo('ru', { language: 'ru', englishName: 'Russian', nativeName: 'Русский', topLevel: true });
    VRS.globalisation.addCultureInfo('ru-RU', { language: 'ru', englishName: 'Russian (Russia)', nativeName: 'Русский (Россия)' });
    VRS.globalisation.addCultureInfo('zh', { language: 'zh', englishName: 'Chinese', nativeName: '中文', topLevel: true });
    VRS.globalisation.addCultureInfo('zh-CN', { language: 'zh', englishName: 'Chinese (China)', nativeName: '中文 (中国)' });
    VRS.globalisation.addCultureInfo('pt-BR', { language: 'pt-BR', groupLanguage: 'pt', englishName: 'Portuguese (Brazil)', nativeName: 'Português (Brasil)', countryFlag: 'br', topLevel: true });
})(VRS || (VRS = {}));
//# sourceMappingURL=localise.js.map