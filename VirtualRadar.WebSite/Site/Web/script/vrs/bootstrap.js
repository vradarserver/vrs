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
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview The base bootstrap class.
 */

(function(VRS, $, /** object= */ undefined)
{
    /**
     * The base for all bootstrap objects.
     * @param {Object}  objSettings                 The base settings for the bootstrap object.
     * @param {string}  objSettings.configPrefix    The prefix to store configuration settings under.
     * @param {string}  objSettings.dispatcherName  The name for the events dispatcher.
     * @constructor
     */
    VRS.Bootstrap = function(objSettings)
    {
        var that = this;

        if(!objSettings) throw 'An objSettings must be supplied';
        if(!objSettings.configPrefix) throw 'A configuration prefix must be supplied';
        if(!objSettings.dispatcherName) throw 'A dispatcher name must be supplied';

        //region Events exposed
        var _Dispatcher = new VRS.EventHandler({
            name: objSettings.dispatcherName
        });
        var _Events = {
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

        this.hookAircraftDetailPanelInitialised = function(callback, forceThis)     { return _Dispatcher.hook(_Events.aircraftDetailPanelInitialised,  callback, forceThis); };
        this.hookAircraftListPanelInitialised = function(callback, forceThis)       { return _Dispatcher.hook(_Events.aircraftListPanelInitialised,  callback, forceThis); };
        this.hookConfigStorageInitialised = function(callback, forceThis)           { return _Dispatcher.hook(_Events.configStorageInitialised, callback, forceThis); };
        this.hookCreatedSettingsMenu = function(callback, forceThis)                { return _Dispatcher.hook(_Events.createdSettingsMenu, callback, forceThis); };
        this.hookInitialised = function(callback, forceThis)                        { return _Dispatcher.hook(_Events.initialised, callback, forceThis); };
        this.hookInitialising = function(callback, forceThis)                       { return _Dispatcher.hook(_Events.initialising, callback, forceThis); };
        this.hookLayoutsInitialised = function(callback, forceThis)                 { return _Dispatcher.hook(_Events.layoutsInitialised, callback, forceThis); };
        this.hookLocaleInitialised = function(callback, forceThis)                  { return _Dispatcher.hook(_Events.localeInitialised,  callback, forceThis); };
        this.hookMapInitialised = function(callback, forceThis)                     { return _Dispatcher.hook(_Events.mapInitialised, callback, forceThis); };
        this.hookMapInitialising = function(callback, forceThis)                    { return _Dispatcher.hook(_Events.mapInitialising, callback, forceThis); };
        this.hookMapLoaded = function(callback, forceThis)                          { return _Dispatcher.hook(_Events.mapLoaded, callback, forceThis); };
        this.hookMapSettingsInitialised = function(callback, forceThis)             { return _Dispatcher.hook(_Events.mapSettingsInitialised, callback, forceThis); };
        this.hookOptionsPagesInitialised = function(callback, forceThis)            { return _Dispatcher.hook(_Events.optionsPagesInitialised, callback, forceThis); };
        this.hookPageManagerInitialised = function(callback, forceThis)             { return _Dispatcher.hook(_Events.pageManagerInitialised, callback, forceThis); };
        this.hookReportCreated = function(callback, forceThis)                      { return _Dispatcher.hook(_Events.reportCreated, callback, forceThis); };

        this.unhook = function(hookResult)                                          { _Dispatcher.unhook(hookResult); };
        //endregion

        //region Script helpers - addMapLibrary
        /**
         * Call this from an Initialising event handler. Adds the library to pageSettings.mapSettings.libraries.
         * @param {*}       pageSettings
         * @param {string}  library
         */
        this.addMapLibrary = function(pageSettings, library)
        {
            if(pageSettings) {
                if(!pageSettings.mapSettings)           pageSettings.mapSettings = {};
                if(!pageSettings.mapSettings.libraries) pageSettings.mapSettings.libraries = [];

                if(VRS.arrayHelper.indexOf(pageSettings.mapSettings.libraries, library) === -1) {
                    pageSettings.mapSettings.libraries.push(library);
                }
            }
        };
        //endregion

        //region getBase
        /**
         * Gets the "protected" base methods. For internal use only.
         * @returns {{
         *      dispatcher:                             VRS.EventHandler,
         *      events:                                 Object.<String>,
         *
         *      doStartInitialise:                      function(Object),
         *      doEndInitialise:                        function(Object),
         *
         *      createMapSettingsControl:               function(Object):jQuery,
         *      createHelpMenuEntry:                    function(string):VRS.MenuItem,
         *      createLayoutMenuEntry:                  function(Object, Array):VRS.MenuItem,
         *      createLocaleMenuEntry:                  function(Object):VRS.MenuItem,
         *      endLayoutInitialisation:                function(Object),
         *
         *      raiseAircraftDetailPanelInitialised:    function(Object),
         *      raiseAircraftListPanelInitialised:      function(Object),
         *      raiseConfigStorageInitialised:          function(Object),
         *      raiseCreatedSettingsMenu:               function(Object),
         *      raiseInitialised:                       function(Object),
         *      raiseInitialising:                      function(Object),
         *      raiseLayoutsInitialised:                function(Object),
         *      raiseLocaleInitialised:                 function(Object),
         *      raiseMapInitialised:                    function(Object),
         *      raiseMapInitialising:                   function(Object),
         *      raiseMapLoaded:                         function(Object),
         *      raiseMapSettingsInitialised:            function(Object),
         *      raiseOptionsPagesInitialised:           function(Object),
         *      raisePageManagerInitialised:            function(Object),
         *      raiseReportCreated:                     function(Object)
         * }}
         */
        this.getBase = function()
        {
            return {
                //region -- dispatcher, events
                dispatcher: _Dispatcher,
                events:     _Events,
                //endregion

                //region -- raise**** event raiser
                raiseAircraftDetailPanelInitialised: function(pageSettings)         { _Dispatcher.raise(_Events.aircraftDetailPanelInitialised, [ pageSettings, this ]); },
                raiseAircraftListPanelInitialised: function(pageSettings)           { _Dispatcher.raise(_Events.aircraftListPanelInitialised, [ pageSettings, this ]); },
                raiseConfigStorageInitialised: function(pageSettings)               { _Dispatcher.raise(_Events.configStorageInitialised, [ pageSettings, this ]); },
                raiseCreatedSettingsMenu: function(pageSettings)                    { _Dispatcher.raise(_Events.createdSettingsMenu, [ pageSettings, this ]); },
                raiseInitialised: function(pageSettings)                            { _Dispatcher.raise(_Events.initialised, [ pageSettings, this ]); },
                raiseInitialising: function(pageSettings)                           { _Dispatcher.raise(_Events.initialising, [ pageSettings, this ]); },
                raiseLayoutsInitialised: function(pageSettings)                     { _Dispatcher.raise(_Events.layoutsInitialised, [ pageSettings, this ]); },
                raiseLocaleInitialised: function(pageSettings)                      { _Dispatcher.raise(_Events.localeInitialised, [ pageSettings, this ]); },
                raiseMapInitialised: function(pageSettings)                         { _Dispatcher.raise(_Events.mapInitialised, [ pageSettings, this ]); },
                raiseMapInitialising: function(pageSettings)                        { _Dispatcher.raise(_Events.mapInitialising, [ pageSettings, this ]); },
                raiseMapLoaded: function(pageSettings)                              { _Dispatcher.raise(_Events.mapLoaded, [ pageSettings, this ]); },
                raiseMapSettingsInitialised: function(pageSettings)                 { _Dispatcher.raise(_Events.mapSettingsInitialised, [ pageSettings, this ]); },
                raiseOptionsPagesInitialised: function(pageSettings)                { _Dispatcher.raise(_Events.optionsPagesInitialised, [ pageSettings, this ]); },
                raisePageManagerInitialised: function(pageSettings)                 { _Dispatcher.raise(_Events.pageManagerInitialised, [ pageSettings, this ]); },
                raiseReportCreated: function(pageSettings)                          { _Dispatcher.raise(_Events.reportCreated, [ pageSettings, this ]); },
                //endregion

                //region -- doStartInitialise, doEndInitialise
                /**
                 * All implementations of initialise should call this before doing any work.
                 * @param {Object}  pageSettings
                 */
                doStartInitialise: function(pageSettings) {
                    // Make the bootstrap and page settings accessible to the browser's console to aid in debugging / diagnostics
                    VRS.bootstrap = that;
                    VRS.bootstrap.pageSettings = pageSettings;

                    // Tell the world that we're going to start initialising the page
                    this.raiseInitialising(pageSettings);

                    // Configure the storage
                    VRS.configStorage.warnIfMissing();
                    VRS.configStorage.setPrefix(objSettings.configPrefix);
                    VRS.configStorage.cleanupOldStorage();
                    this.raiseConfigStorageInitialised(pageSettings);

                    // Load the appropriate language
                    VRS.globalisation.loadAndApplyState();
                    this.raiseLocaleInitialised(pageSettings);

                    // If a timeout manager is present then initialise it
                    if(VRS.timeoutManager) VRS.timeoutManager.initialise();

                    // Initialise the unit display preferences
                    pageSettings.unitDisplayPreferences = new VRS.UnitDisplayPreferences();
                    pageSettings.unitDisplayPreferences.loadAndApplyState();

                    // Create the settings menu - the page decides where this goes, but all pages have one somewhere
                    if(!pageSettings.settingsMenu) pageSettings.settingsMenu = new VRS.Menu();
                    this.raiseCreatedSettingsMenu(pageSettings);
                },

                /**
                 * All implementations of initialise should call this after they have finished initialising the page.
                 * @param {Object}  pageSettings
                 */
                doEndInitialise: function(pageSettings) {
                    // Tell the world that we've finished initialising the page.
                    this.raiseInitialised(pageSettings);
                },
                //endregion

                //region -- Page helpers - createMapSettingsControl, createHelpMenuEntry
                /**
                 * Creates a map settings button control. Does not add it to any map.
                 * @param {Object}          pageSettings                        The page settings.
                 * @param {boolean}        [pageSettings.showSettingsButton]    True if the button is to be added.
                 * @param {jQuery}         [pageSettings.mapButton]             Populated with the jQuery element for the settings button control.
                 * @param {jQuery}         [pageSettings.menuJQ]                Populated with the jQuery element for the menu that is triggered by the settings button.
                 * @param {VRS.vrsMenu}    [pageSettings.menuPlugin]            Populated with a direct reference to the menu in menuJQ.
                 * @returns {jQuery=}                                           The mapButton settings button to add to a map.
                 */
                createMapSettingsControl: function(pageSettings) {
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
                                alignment:              VRS.Alignment.Right
                            }));
                        pageSettings.menuPlugin = VRS.jQueryUIHelper.getMenuPlugin(pageSettings.mapJQ);
                    }

                    return pageSettings.mapButton;
                },

                /**
                 * Creates a menu entry for the online help URL for the page.
                 * @param {String} relativeUrl  The help URL relative to the main VRS website's documentation folder.
                 * @returns {VRS.MenuItem}
                 */
                createHelpMenuEntry: function(relativeUrl)
                {
                    return new VRS.MenuItem({
                        name: 'pageHelp',
                        labelKey: 'Help',
                        vrsIcon: 'question',
                        clickCallback: function() { window.open('http://www.virtualradarserver.co.uk/Documentation/' + relativeUrl, 'help'); }
                    });
                },
                //endregion

                //region -- endLayoutInitialisation, createLayoutMenuEntry
                /**
                 * Performs common initialisation after the layout manager has had all of the layouts added to it.
                 * @param {Object}  pageSettings                    The object carrying all of the page settings.
                 * @param {jQuery}  pageSettings.splittersJQ        The jQuery element that will be the parent for all splitters.
                 */
                endLayoutInitialisation: function(pageSettings)
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
                },

                /**
                 * Creates a settings menu entry for the layouts.
                 * @param {Object}           pageSettings                       The page settings.
                 * @param {boolean}         [pageSettings.showLayoutSetting]    True if the layouts should be added to the settings menu.
                 * @param {VRS.Menu}        [pageSettings.settingsMenu]         The menu to add the layouts to.
                 * @param {VRS.MenuItem}    [pageSettings.layoutMenuItem]       Populated with the menu item for the layouts.
                 * @param {string[]}        [separatorIds]                      An optional array of layout names - separators are added before the menu entries for these layouts.
                 * @returns {VRS.MenuItem=}                                     The top-level menu item for the layout menu.
                 */
                createLayoutMenuEntry: function(pageSettings, separatorIds)
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

                        $.each(VRS.layoutManager.getLayouts(), function(/** Number */ idx, /** VRS_LAYOUT_LABEL */ label) {
                            var addSeparator = $.inArray(label.labelKey, separatorIds) !== -1;
                            if(addSeparator) result.subItems.push(null);

                            result.subItems.push(new VRS.MenuItem({
                                name:           'layout-' + label.name,
                                labelKey:       label.labelKey,
                                disabled:       function() { return VRS.layoutManager.getCurrentLayout() === label.name; },
                                checked:        function() { return VRS.layoutManager.getCurrentLayout() === label.name; },
                                clickCallback:  function() { VRS.layoutManager.applyLayout(label.name); VRS.layoutManager.saveState(); }
                            }));
                        });
                    }

                    return result;
                },
                //endregion

                //region -- createLocaleMenuEntry
                /**
                 * Creates and returns a menu entry for locale selection.
                 * @param {Object}          pageSettings                    The page settings object.
                 * @param {VRS.MenuItem}    pageSettings.localeMenuItem     Populated with the locale selection menu item.
                 * @returns {VRS.MenuItem}
                 */
                createLocaleMenuEntry: function(pageSettings)
                {
                    pageSettings.localeMenuItem = new VRS.MenuItem({
                        name:               'localeSelect',
                        labelKey:           function() { return VRS.globalisation.getCultureInfo().nativeName; },
                        labelImageUrl:      function() { return VRS.globalisation.getCultureInfo().flagImage; },
                        labelImageClasses:  'flagImage'
                    });

                    $.each(VRS.globalisation.getCultureInfosGroupedByLanguage(true), function(/** Number */ idx, /** Array.<VRS.CultureInfo> */ languageCultures) {
                        /** @type {VRS.CultureInfo} */
                        var languageCultureInfo = VRS.arrayHelper.findFirst(languageCultures, function(r) { return r.locale.indexOf('-') === -1; });
                        if(languageCultureInfo) {
                            var languageSubMenuItem = new VRS.MenuItem({
                                name:               'locale-' + languageCultureInfo.locale,
                                labelKey:           function() { return languageCultureInfo.nativeName; },
                                labelImageUrl:      function() { return languageCultureInfo.flagImage; },
                                labelImageClasses:  'flagImage'
                            });
                            pageSettings.localeMenuItem.subItems.push(languageSubMenuItem);

                            $.each(languageCultures, function(/** Number */idx, /** VRS.CultureInfo */ cultureInfo) {
                                languageSubMenuItem.subItems.push(new VRS.MenuItem({
                                    name:               languageSubMenuItem.name + '-' + cultureInfo.locale,
                                    labelKey:           function() { return cultureInfo.nativeName; },
                                    labelImageUrl:      function() { return cultureInfo.flagImage; },
                                    labelImageClasses:  'flagImage',
                                    disabled:           function() { return cultureInfo.locale === VRS.globalisation.getLocale(); },
                                    clickCallback:      function() { VRS.globalisation.setLocaleInBackground(cultureInfo.locale, true, function() { VRS.globalisation.saveState(); }); }
                                }));
                            })
                        }
                    });

                    return pageSettings.localeMenuItem;
                },
                //endregion

                __nop: null
            }
        };
        //endregion
    };
}(window.VRS = window.VRS || {}, jQuery));
