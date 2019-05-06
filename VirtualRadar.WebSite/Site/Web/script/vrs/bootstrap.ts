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
 * @fileoverview The base bootstrap class.
 */

namespace VRS
{
    /**
     * The base bootstrap settings.
     */
    export interface Bootstrap_Settings
    {
        /**
         * The prefix to use for all configuration settings.
         */
        configPrefix: string;

        /**
         * The name for the events dispatcher.
         */
        dispatcherName?: string;

        /**
         * How to align the menu items.
         */
        settingsMenuAlignment?: AlignmentEnum;
    }

    /**
     * The base page settings.
     */
    export interface PageSettings_Base
    {
        /**
         * The menu item that Bootstrap creates to allow the user to select which splitter layout they want to use.
         */
        layoutMenuItem?: MenuItem;

        /**
         * The menu item that Bootstrap creates to allow the user to select a language.
         */
        localeMenuItem?: MenuItem;

        /**
         * The settings menu button that the Bootstrap code adds to the map.
         */
        mapButton?: JQuery;

        /**
         * The main map element.
         */
        mapJQ?: JQuery;

        /**
         * The settings to use when creating the map.
         */
        mapSettings?: IMapOptions;

        /**
         * The menu plugin created by the bootstrap, associated with menuJQ.
         */
        menuPlugin?: MenuPlugin;

        /**
         * The menu control that the bootstrap creates.
         */
        menuJQ?: JQuery;

        /**
         * The menu to show to the user.
         */
        settingsMenu?: Menu;

        /**
         * True if there should be a menu entry that lets the user choose between the splitter layouts.
         */
        showLayoutSetting?: boolean;

        /**
         * True if the settings button should be shown on the map.
         */
        showSettingsButton?: boolean;

        /**
         * True if the layers menu entry should be shown to the user.
         */
        showLayersMenu?: boolean;

        /**
         * The element that is the parent for all of the page's splitters.
         */
        splittersJQ?: JQuery;

        /**
         * The units to use when displaying altitudes etc.
         */
        unitDisplayPreferences?: UnitDisplayPreferences;
    }

    /**
     * The base for all bootstrap objects.
     */
    export class Bootstrap
    {
        private _Dispatcher: EventHandler;
        private _Events = {
            aircraftDetailPanelInitialised: 'aircraftDetailPanelInitialised',
            aircraftListPanelInitialised:   'aircraftListPanelInitialised',
            configStorageInitialised:       'configStorageInitialised',
            createdSettingsMenu:            'createdSettingsMenu',
            initialised:                    'initialised',
            initialising:                   'initialising',
            layoutsInitialised:             'layoutsInitialised',
            localeInitialised:              'localeInitialised',
            mapInitialised:                 'mapInitialised',
            mapInitialising:                'mapInitialising',
            mapLoaded:                      'mapLoaded',
            mapSettingsInitialised:         'mapSettingsInitialised',
            optionsPagesInitialised:        'optionsPagesInitialised',
            pageManagerInitialised:         'pageManagerInitialised',
            reportCreated:                  'reportCreated'
        };
        protected _Settings: Bootstrap_Settings;
        protected _PageSettings: PageSettings_Base;

        constructor(settings: Bootstrap_Settings)
        {
            if(!settings) throw 'Settings must be supplied';
            if(!settings.configPrefix) throw 'A configuration prefix must be supplied';
            if(!settings.dispatcherName) throw 'A dispatcher name must be supplied';

            this._Settings = settings;
            this._Dispatcher = new VRS.EventHandler({
                name: settings.dispatcherName
            });
        }

        get pageSettings() : PageSettings_Base
        {
            return this._PageSettings;
        }

        set pageSettings(value: PageSettings_Base)
        {
            this._PageSettings = value;
        }

        hookAircraftDetailPanelInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.aircraftDetailPanelInitialised, callback, forceThis);
        }
        protected raiseAircraftDetailPanelInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.aircraftDetailPanelInitialised, [ pageSettings, this ]);
        }

        hookAircraftListPanelInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.aircraftListPanelInitialised, callback, forceThis);
        }
        protected raiseAircraftListPanelInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.aircraftListPanelInitialised, [ pageSettings, this ]);
        }

        hookConfigStorageInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.configStorageInitialised, callback, forceThis);
        }
        protected raiseConfigStorageInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.configStorageInitialised, [ pageSettings, this ]);
        }

        hookCreatedSettingsMenu(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.createdSettingsMenu, callback, forceThis);
        }
        protected raiseCreatedSettingsMenu(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.createdSettingsMenu, [ pageSettings, this ]);
        }

        hookInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.initialised, callback, forceThis);
        }
        protected raiseInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.initialised, [ pageSettings, this ]);
        }

        hookInitialising(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.initialising, callback, forceThis);
        }
        protected raiseInitialising(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.initialising, [ pageSettings, this ]);
        }

        hookLayoutsInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.layoutsInitialised, callback, forceThis);
        }
        protected raiseLayoutsInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.layoutsInitialised, [ pageSettings, this ]);
        }

        hookLocaleInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.localeInitialised,  callback, forceThis);
        }
        protected raiseLocaleInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.localeInitialised, [ pageSettings, this ]);
        }

        hookMapInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.mapInitialised, callback, forceThis);
        }
        protected raiseMapInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.mapInitialised, [ pageSettings, this ]);
        }

        hookMapInitialising(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.mapInitialising, callback, forceThis);
        }
        protected raiseMapInitialising(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.mapInitialising, [ pageSettings, this ]);
        }

        hookMapLoaded(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.mapLoaded, callback, forceThis);
        }
        protected raiseMapLoaded(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.mapLoaded, [ pageSettings, this ]);
        }

        hookMapSettingsInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.mapSettingsInitialised, callback, forceThis);
        }
        protected raiseMapSettingsInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.mapSettingsInitialised, [ pageSettings, this ]);
        }

        hookOptionsPagesInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.optionsPagesInitialised, callback, forceThis);
        }
        protected raiseOptionsPagesInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.optionsPagesInitialised, [ pageSettings, this ]);
        }

        hookPageManagerInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.pageManagerInitialised, callback, forceThis);
        }
        protected raisePageManagerInitialised(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.pageManagerInitialised, [ pageSettings, this ]);
        }

        hookReportCreated(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.reportCreated, callback, forceThis);
        }
        protected raiseReportCreated(pageSettings: PageSettings_Base)
        {
            this._Dispatcher.raise(this._Events.reportCreated, [ pageSettings, this ]);
        }

        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Call this from an Initialising event handler. Adds the library to pageSettings.mapSettings.libraries.
         */
        addMapLibrary(pageSettings: PageSettings_Base, library: string)
        {
            if(pageSettings) {
                if(!pageSettings.mapSettings)           pageSettings.mapSettings = {};
                if(!pageSettings.mapSettings.libraries) pageSettings.mapSettings.libraries = [];

                if(VRS.arrayHelper.indexOf(pageSettings.mapSettings.libraries, library) === -1) {
                    pageSettings.mapSettings.libraries.push(library);
                }
            }
        }

        /**
         * All implementations of initialise should call this before doing any work.
         */
        protected doStartInitialise(pageSettings: PageSettings_Base, successCallback: () => void)
        {
            // Make the bootstrap and page settings accessible to the browser's console to aid in debugging / diagnostics
            VRS.bootstrap = this;
            this.pageSettings = pageSettings;

            // Tell the world that we're going to start initialising the page
            this.raiseInitialising(pageSettings);

            // Configure the storage
            VRS.configStorage.warnIfMissing();
            VRS.configStorage.setPrefix(this._Settings.configPrefix);
            VRS.configStorage.cleanupOldStorage();
            this.raiseConfigStorageInitialised(pageSettings);

            // Load the appropriate language
            VRS.globalisation.loadAndApplyState(() => {
                this.raiseLocaleInitialised(pageSettings);

                // If a timeout manager is present then initialise it
                if(VRS.timeoutManager) {
                    VRS.timeoutManager.initialise();
                }

                // Initialise the unit display preferences
                pageSettings.unitDisplayPreferences = new VRS.UnitDisplayPreferences();
                pageSettings.unitDisplayPreferences.loadAndApplyState();

                // Create the settings menu - the page decides where this goes, but all pages have one somewhere
                if(!pageSettings.settingsMenu) pageSettings.settingsMenu = new Menu();
                this.raiseCreatedSettingsMenu(pageSettings);

                if(successCallback) {
                    successCallback();
                }
            });
        }

        /**
         * All implementations of initialise should call this after they have finished initialising the page.
         */
        protected doEndInitialise(pageSettings: PageSettings_Base)
        {
            this.raiseInitialised(pageSettings);
        }

        /**
         * Creates a map settings button control. Does not add it to any map.
         */
        protected createMapSettingsControl(pageSettings: PageSettings_Base) : JQuery
        {
            if(pageSettings.showSettingsButton && VRS.jQueryUIHelper.getMenuPlugin) {
                // Create the settings button control that the page should add to a map somewhere
                pageSettings.mapButton = $('<div/>')
                    .addClass('mapButton')
                    .append($('<span/>').addClass('vrsIcon vrsIcon-cog'))
                    .append($('<span/>').text(VRS.$$.Menu));
                VRS.globalisation.hookLocaleChanged(function() {
                    $('span:not(.vrsIcon)', pageSettings.mapButton).text(VRS.$$.Menu);
                });

                // Create the menu control that the settings button will trigger
                pageSettings.menuJQ = $('<div/>')
                    .vrsMenu(VRS.jQueryUIHelper.getMenuOptions({
                        menu:                   pageSettings.settingsMenu,
                        showButtonTrigger:      false,
                        triggerElement:         pageSettings.mapButton,
                        alignment:              this._Settings.settingsMenuAlignment
                    }));
                pageSettings.menuPlugin = VRS.jQueryUIHelper.getMenuPlugin(pageSettings.mapJQ);
            }

            return pageSettings.mapButton;
        }

        protected createLayersMenuEntry(pageSettings: PageSettings_Base, isLivePage: boolean) : MenuItem
        {
            var result: VRS.MenuItem = null;

            if(pageSettings.showLayersMenu && VRS.serverConfig && VRS.mapLayerManager) {
                var mapPlugin = VRS.jQueryUIHelper.getMapPlugin(pageSettings.mapJQ);
                var layers = VRS.mapLayerManager.getMapLayerSettings();
                if(mapPlugin && layers.length > 0 || mapPlugin.getCanSetMapBrightness()) {
                    result = new VRS.MenuItem({
                        name: 'layers',
                        labelKey: 'MapLayers'
                    });

                    if(mapPlugin.getCanSetMapBrightness()) {
                        result.subItems.push(new VRS.MenuItem({
                            name:               'map-brightness',
                            labelKey:           'MapBrightness',
                            showSlider:         true,
                            sliderMinimum:      10,
                            sliderMaximum:      150,
                            sliderStep:         10,
                            sliderInitialValue: () => mapPlugin.getMapBrightness(),
                            sliderDefaultValue: () => mapPlugin.getDefaultMapBrightness(),
                            sliderCallback:     (value: number) => mapPlugin.setMapBrightness(value),
                            noAutoClose:        true
                        }));

                        if(layers.length > 0) {
                            result.subItems.push(null);
                        }
                    }

                    $.each(layers, (idx: number, layer: MapLayerSetting) => {
                        result.subItems.push(new VRS.MenuItem({
                            name:               'layer-' + layer.Name,
                            labelKey:           () => layer.Name,
                            checked:            () => layer.IsVisible,
                            clickCallback:      () => layer.toggleVisible(),
                            showSlider:         true,
                            sliderMinimum:      10,
                            sliderMaximum:      100,
                            sliderStep:         10,
                            sliderInitialValue: () => layer.getMapOpacity(),
                            sliderDefaultValue: () => layer.TileServerSettings.DefaultOpacity,
                            sliderCallback:     (value: number) => layer.setMapOpacity(value),
                            noAutoClose:        true
                        }));
                    });
                }
            }

            return result;
        }

        /**
         * Creates a menu entry for the online help URL for the page. The help URL is relative to the main VRS website's documentation folder.
         */
        protected createHelpMenuEntry(relativeUrl: string) : MenuItem
        {
            return new VRS.MenuItem({
                name: 'pageHelp',
                labelKey: 'Help',
                vrsIcon: 'question',
                clickCallback: function() { window.open('http://www.virtualradarserver.co.uk/Documentation/' + relativeUrl, 'help'); }
            });
        }

        /**
         * Performs common initialisation after the layout manager has had all of the layouts added to it.
         */
        protected endLayoutInitialisation(pageSettings: PageSettings_Base)
        {
            // Give other people a chance to add their own layouts.
            this.raiseLayoutsInitialised(pageSettings);

            // Set the parent element for all splitters
            VRS.layoutManager.setSplitterParent(pageSettings.splittersJQ);

            // Load the layout that they last used, or the first registered if no explicit layout is known
            VRS.layoutManager.loadAndApplyState();
            if(!VRS.layoutManager.getCurrentLayout()) {
                VRS.layoutManager.applyLayout(VRS.layoutManager.getLayouts()[0].name);
            }
        }

        /**
         * Creates a settings menu entry for the layouts.
         */
        protected createLayoutMenuEntry(pageSettings: PageSettings_Base, separatorIds?: string[]) : MenuItem
        {
            if(!separatorIds) separatorIds = [];
            var result = null;

            if(VRS.layoutManager && pageSettings.showLayoutSetting && pageSettings.settingsMenu) {
                result = new VRS.MenuItem({
                    name:       'layout',
                    labelKey:   'Layout',
                    vrsIcon:    'screen'
                });
                pageSettings.layoutMenuItem = result;

                $.each(VRS.layoutManager.getLayouts(), function(idx, label) {
                    var addSeparator = $.inArray(label.labelKey, separatorIds) !== -1;
                    if(addSeparator) {
                        result.subItems.push(null);
                    }

                    result.subItems.push(new VRS.MenuItem({
                        name:           'layout-' + label.name,
                        labelKey:       label.labelKey,
                        disabled:       function() { return VRS.layoutManager.getCurrentLayout() === label.name; },
                        checked:        function() { return VRS.layoutManager.getCurrentLayout() === label.name; },
                        clickCallback:  function() {
                            VRS.layoutManager.applyLayout(label.name);
                            VRS.layoutManager.saveState();
                        }
                    }));
                });
            }

            return result;
        }

        /**
         * Creates and returns a menu entry for locale selection.
         * @param {Object}          pageSettings                    The page settings object.
         * @param {VRS.MenuItem}    pageSettings.localeMenuItem     Populated with the locale selection menu item.
         * @returns {VRS.MenuItem}
         */
        protected createLocaleMenuEntry(pageSettings: PageSettings_Base) : MenuItem
        {
            pageSettings.localeMenuItem = new VRS.MenuItem({
                name:               'localeSelect',
                labelKey:           function() { return VRS.globalisation.getCultureInfo().nativeName; },
                labelImageUrl:      function() { return VRS.globalisation.getCultureInfo().flagImage; },
                labelImageClasses:  'flagImage'
            });

            $.each(VRS.globalisation.getCultureInfosGroupedByLanguage(true), function(idx, languageCultures) {
                var languageCultureInfo = VRS.arrayHelper.findFirst(languageCultures, function(r) { return r.topLevel; });
                if(languageCultureInfo) {
                    var languageSubMenuItem = new VRS.MenuItem({
                        name:               'locale-' + languageCultureInfo.locale,
                        labelKey:           function() { return languageCultureInfo.nativeName; },
                        labelImageUrl:      function() { return languageCultureInfo.flagImage; },
                        labelImageClasses:  'flagImage'
                    });
                    pageSettings.localeMenuItem.subItems.push(languageSubMenuItem);

                    $.each(languageCultures, function(idx, cultureInfo) {
                        languageSubMenuItem.subItems.push(new VRS.MenuItem({
                            name:               languageSubMenuItem.name + '-' + cultureInfo.locale,
                            labelKey:           function() { return cultureInfo.nativeName; },
                            labelImageUrl:      function() { return cultureInfo.flagImage; },
                            labelImageClasses:  'flagImage',
                            disabled:           function() { return cultureInfo.locale === VRS.globalisation.getLocale(); },
                            clickCallback:      function() {
                                VRS.globalisation.setLocaleInBackground(cultureInfo.locale, true, function() {
                                    VRS.globalisation.saveState();
                                }); }
                        }));
                    })
                }
            });

            return pageSettings.localeMenuItem;
        }
    }

    export var bootstrap = null;
}
