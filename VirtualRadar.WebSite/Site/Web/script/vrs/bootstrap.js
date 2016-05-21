var VRS;
(function (VRS) {
    var Bootstrap = (function () {
        function Bootstrap(settings) {
            this._Events = {
                aircraftDetailPanelInitialised: 'aircraftDetailPanelInitialised',
                aircraftListPanelInitialised: 'aircraftListPanelInitialised',
                configStorageInitialised: 'configStorageInitialised',
                createdSettingsMenu: 'createdSettingsMenu',
                initialised: 'initialised',
                initialising: 'initialising',
                layoutsInitialised: 'layoutsInitialised',
                localeInitialised: 'localeInitialised',
                mapInitialised: 'mapInitialised',
                mapInitialising: 'mapInitialising',
                mapLoaded: 'mapLoaded',
                mapSettingsInitialised: 'mapSettingsInitialised',
                optionsPagesInitialised: 'optionsPagesInitialised',
                pageManagerInitialised: 'pageManagerInitialised',
                reportCreated: 'reportCreated'
            };
            if (!settings)
                throw 'Settings must be supplied';
            if (!settings.configPrefix)
                throw 'A configuration prefix must be supplied';
            if (!settings.dispatcherName)
                throw 'A dispatcher name must be supplied';
            this._Settings = settings;
            this._Dispatcher = new VRS.EventHandler({
                name: settings.dispatcherName
            });
        }
        Object.defineProperty(Bootstrap.prototype, "pageSettings", {
            get: function () {
                return this._PageSettings;
            },
            set: function (value) {
                this._PageSettings = value;
            },
            enumerable: true,
            configurable: true
        });
        Bootstrap.prototype.hookAircraftDetailPanelInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.aircraftDetailPanelInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseAircraftDetailPanelInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.aircraftDetailPanelInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookAircraftListPanelInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.aircraftListPanelInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseAircraftListPanelInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.aircraftListPanelInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookConfigStorageInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.configStorageInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseConfigStorageInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.configStorageInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookCreatedSettingsMenu = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.createdSettingsMenu, callback, forceThis);
        };
        Bootstrap.prototype.raiseCreatedSettingsMenu = function (pageSettings) {
            this._Dispatcher.raise(this._Events.createdSettingsMenu, [pageSettings, this]);
        };
        Bootstrap.prototype.hookInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.initialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.initialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookInitialising = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.initialising, callback, forceThis);
        };
        Bootstrap.prototype.raiseInitialising = function (pageSettings) {
            this._Dispatcher.raise(this._Events.initialising, [pageSettings, this]);
        };
        Bootstrap.prototype.hookLayoutsInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.layoutsInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseLayoutsInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.layoutsInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookLocaleInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.localeInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseLocaleInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.localeInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookMapInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.mapInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseMapInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.mapInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookMapInitialising = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.mapInitialising, callback, forceThis);
        };
        Bootstrap.prototype.raiseMapInitialising = function (pageSettings) {
            this._Dispatcher.raise(this._Events.mapInitialising, [pageSettings, this]);
        };
        Bootstrap.prototype.hookMapLoaded = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.mapLoaded, callback, forceThis);
        };
        Bootstrap.prototype.raiseMapLoaded = function (pageSettings) {
            this._Dispatcher.raise(this._Events.mapLoaded, [pageSettings, this]);
        };
        Bootstrap.prototype.hookMapSettingsInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.mapSettingsInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseMapSettingsInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.mapSettingsInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookOptionsPagesInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.optionsPagesInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raiseOptionsPagesInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.optionsPagesInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookPageManagerInitialised = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.pageManagerInitialised, callback, forceThis);
        };
        Bootstrap.prototype.raisePageManagerInitialised = function (pageSettings) {
            this._Dispatcher.raise(this._Events.pageManagerInitialised, [pageSettings, this]);
        };
        Bootstrap.prototype.hookReportCreated = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.reportCreated, callback, forceThis);
        };
        Bootstrap.prototype.raiseReportCreated = function (pageSettings) {
            this._Dispatcher.raise(this._Events.reportCreated, [pageSettings, this]);
        };
        Bootstrap.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        Bootstrap.prototype.addMapLibrary = function (pageSettings, library) {
            if (pageSettings) {
                if (!pageSettings.mapSettings)
                    pageSettings.mapSettings = {};
                if (!pageSettings.mapSettings.libraries)
                    pageSettings.mapSettings.libraries = [];
                if (VRS.arrayHelper.indexOf(pageSettings.mapSettings.libraries, library) === -1) {
                    pageSettings.mapSettings.libraries.push(library);
                }
            }
        };
        Bootstrap.prototype.doStartInitialise = function (pageSettings, successCallback) {
            var _this = this;
            VRS.bootstrap = this;
            this.pageSettings = pageSettings;
            this.raiseInitialising(pageSettings);
            VRS.configStorage.warnIfMissing();
            VRS.configStorage.setPrefix(this._Settings.configPrefix);
            VRS.configStorage.cleanupOldStorage();
            this.raiseConfigStorageInitialised(pageSettings);
            VRS.globalisation.loadAndApplyState(function () {
                _this.raiseLocaleInitialised(pageSettings);
                if (VRS.timeoutManager) {
                    VRS.timeoutManager.initialise();
                }
                pageSettings.unitDisplayPreferences = new VRS.UnitDisplayPreferences();
                pageSettings.unitDisplayPreferences.loadAndApplyState();
                if (!pageSettings.settingsMenu)
                    pageSettings.settingsMenu = new VRS.Menu();
                _this.raiseCreatedSettingsMenu(pageSettings);
                if (successCallback) {
                    successCallback();
                }
            });
        };
        Bootstrap.prototype.doEndInitialise = function (pageSettings) {
            this.raiseInitialised(pageSettings);
        };
        Bootstrap.prototype.createMapSettingsControl = function (pageSettings) {
            if (pageSettings.showSettingsButton && VRS.jQueryUIHelper.getMenuPlugin) {
                pageSettings.mapButton = $('<div/>')
                    .addClass('mapButton')
                    .append($('<span/>').addClass('vrsIcon vrsIcon-cog'))
                    .append($('<span/>').text(VRS.$$.Menu));
                VRS.globalisation.hookLocaleChanged(function () {
                    $('span:not(.vrsIcon)', pageSettings.mapButton).text(VRS.$$.Menu);
                });
                pageSettings.menuJQ = $('<div/>')
                    .vrsMenu(VRS.jQueryUIHelper.getMenuOptions({
                    menu: pageSettings.settingsMenu,
                    showButtonTrigger: false,
                    triggerElement: pageSettings.mapButton,
                    alignment: this._Settings.settingsMenuAlignment
                }));
                pageSettings.menuPlugin = VRS.jQueryUIHelper.getMenuPlugin(pageSettings.mapJQ);
            }
            return pageSettings.mapButton;
        };
        Bootstrap.prototype.createHelpMenuEntry = function (relativeUrl) {
            return new VRS.MenuItem({
                name: 'pageHelp',
                labelKey: 'Help',
                vrsIcon: 'question',
                clickCallback: function () { window.open('http://www.virtualradarserver.co.uk/Documentation/' + relativeUrl, 'help'); }
            });
        };
        Bootstrap.prototype.endLayoutInitialisation = function (pageSettings) {
            this.raiseLayoutsInitialised(pageSettings);
            VRS.layoutManager.setSplitterParent(pageSettings.splittersJQ);
            VRS.layoutManager.loadAndApplyState();
            if (!VRS.layoutManager.getCurrentLayout()) {
                VRS.layoutManager.applyLayout(VRS.layoutManager.getLayouts()[0].name);
            }
        };
        Bootstrap.prototype.createLayoutMenuEntry = function (pageSettings, separatorIds) {
            if (!separatorIds)
                separatorIds = [];
            var result = null;
            if (VRS.layoutManager && pageSettings.showLayoutSetting && pageSettings.settingsMenu) {
                result = new VRS.MenuItem({
                    name: 'layout',
                    labelKey: 'Layout',
                    vrsIcon: 'screen'
                });
                pageSettings.layoutMenuItem = result;
                $.each(VRS.layoutManager.getLayouts(), function (idx, label) {
                    var addSeparator = $.inArray(label.labelKey, separatorIds) !== -1;
                    if (addSeparator) {
                        result.subItems.push(null);
                    }
                    result.subItems.push(new VRS.MenuItem({
                        name: 'layout-' + label.name,
                        labelKey: label.labelKey,
                        disabled: function () { return VRS.layoutManager.getCurrentLayout() === label.name; },
                        checked: function () { return VRS.layoutManager.getCurrentLayout() === label.name; },
                        clickCallback: function () {
                            VRS.layoutManager.applyLayout(label.name);
                            VRS.layoutManager.saveState();
                        }
                    }));
                });
            }
            return result;
        };
        Bootstrap.prototype.createLocaleMenuEntry = function (pageSettings) {
            pageSettings.localeMenuItem = new VRS.MenuItem({
                name: 'localeSelect',
                labelKey: function () { return VRS.globalisation.getCultureInfo().nativeName; },
                labelImageUrl: function () { return VRS.globalisation.getCultureInfo().flagImage; },
                labelImageClasses: 'flagImage'
            });
            $.each(VRS.globalisation.getCultureInfosGroupedByLanguage(true), function (idx, languageCultures) {
                var languageCultureInfo = VRS.arrayHelper.findFirst(languageCultures, function (r) { return r.topLevel; });
                if (languageCultureInfo) {
                    var languageSubMenuItem = new VRS.MenuItem({
                        name: 'locale-' + languageCultureInfo.locale,
                        labelKey: function () { return languageCultureInfo.nativeName; },
                        labelImageUrl: function () { return languageCultureInfo.flagImage; },
                        labelImageClasses: 'flagImage'
                    });
                    pageSettings.localeMenuItem.subItems.push(languageSubMenuItem);
                    $.each(languageCultures, function (idx, cultureInfo) {
                        languageSubMenuItem.subItems.push(new VRS.MenuItem({
                            name: languageSubMenuItem.name + '-' + cultureInfo.locale,
                            labelKey: function () { return cultureInfo.nativeName; },
                            labelImageUrl: function () { return cultureInfo.flagImage; },
                            labelImageClasses: 'flagImage',
                            disabled: function () { return cultureInfo.locale === VRS.globalisation.getLocale(); },
                            clickCallback: function () {
                                VRS.globalisation.setLocaleInBackground(cultureInfo.locale, true, function () {
                                    VRS.globalisation.saveState();
                                });
                            }
                        }));
                    });
                }
            });
            return pageSettings.localeMenuItem;
        };
        return Bootstrap;
    })();
    VRS.Bootstrap = Bootstrap;
    VRS.bootstrap = null;
})(VRS || (VRS = {}));
