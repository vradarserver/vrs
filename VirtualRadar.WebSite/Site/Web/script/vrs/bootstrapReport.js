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
 * @fileoverview The common bootstrap code for report pages.
 */

(function(VRS, $, /** object= */ undefined)
{
    /**
     * The object that deals with creating all of the objects necessary for report pages and wiring them all together.
     * @constructor
     * @augments VRS.Bootstrap
     * @param {Object}              objSettings
     * @param {string}              objSettings.configPrefix            MANDATORY - the configuration prefix to use.
     * @param {boolean=}            objSettings.suppressTitleUpdate     True if the page title should be left alone, false if it can be modified.
     * @param {VRS.MapPosition}    [objSettings.settingsPosition]       Where to draw the settings button. Defaults to top-right.
     * @param {VRS.Alignment}      [objSettings.settingsMenuAlignment]  The alignment of the settings menu. Defaults to VRS.Alignment.Left.
     */
    VRS.BootstrapReport = function(objSettings)
    {
        var that = this;

        objSettings = $.extend({
            dispatcherName:         'VRS.BootstrapReport',
            suppressTitleUpdate:    false,
            settingsPosition:       VRS.MapPosition.TopLeft,
            settingsMenuAlignment:  VRS.Alignment.Left
        }, objSettings);
        VRS.Bootstrap.call(this, objSettings);
        var base = this.getBase();

        //region initialise
        /**
         * Builds the page.
         * @param {Object}              userPageSettings
         * @param {string}              userPageSettings.reportName                 The name of the report.
         * @param {boolean}            [userPageSettings.showSettingsButton]        True if the user is shown the settings button, false if they are not. Defaults to true.
         * @param {boolean}            [userPageSettings.showLanguageSetting]       True if the user is shown the option to configure the language.
         * @param {boolean}            [userPageSettings.showOptionsSetting]        True if the user is shown the option to configure the criteria and settings.
         * @param {bool}               [userPageSettings.showLayoutSetting]         True if the user is to be shown the change layout setting.
         * @param {VRS.Menu}           [userPageSettings.settingsMenu]              The menu that is shown on the settings menu. Created by the bootstrapper if missing. Always raises "createdSettingsMenu" regardless of whether it is supplied.
         * @param {jQuery=}             userPageSettings.splittersJQ                (Desktop) The jQuery element which will act as the parent for splitters. If not supplied then splitters are not used.
         * @param {jQuery=}             userPageSettings.pagesJQ                    (Mobile)  The jQuery element while will act as the parent for pages. If not supplied then the page manager is not used.
         * @param {jQuery=}             userPageSettings.mapJQ                      The element to display a map in.
         * @param {jQuery=}             userPageSettings.listJQ                     The element that the list is displayed in.
         * @param {jQuery=}             userPageSettings.detailJQ                   The element that the detail panel is displayed in.
         */
        this.initialise = function(userPageSettings)
        {
            var pageSettings = $.extend({
                showSettingsButton:         true,
                showOptionsSetting:         true,
                showLanguageSetting:        true,
                showLayoutSetting:          true,
                settingsMenu:               null,
                splittersJQ:                null,
                pagesJQ:                    null,
                mapJQ:                      null
            }, userPageSettings);

            var self = this;
            base.doStartInitialise(pageSettings, function() {
                // Make sure that the current location isn't shown on any map
                VRS.currentLocation.setShowCurrentLocationOnMap(false);

                // Set up the page title
                if(!objSettings.suppressTitleUpdate) document.title = VRS.$$.VirtualRadar;

                // Create the report object
                pageSettings.report = new VRS.Report({
                    name: pageSettings.reportName,
                    autoSaveState: true,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.report.loadAndApplyState();
                pageSettings.report.getCriteria().loadAndApplyState();
                pageSettings.report.populateFromQueryString();
                base.raiseReportCreated(pageSettings);

                // Create the settings button
                if(pageSettings.mapJQ) base.createMapSettingsControl(pageSettings);

                // Create the plotter options - these are used on the various maps
                pageSettings.plotterOptions = new VRS.AircraftPlotterOptions({
                    trailDisplay:           VRS.TrailDisplay.AllAircraft,
                    trailType:              VRS.TrailType.Full
                });

                // Create the report map
                if(pageSettings.mapJQ) {
                    /** @type {VRS_OPTIONS_REPORTMAP} */
                    var mapOptions = {
                        plotterOptions:         pageSettings.plotterOptions,
                        report:                 pageSettings.report,
                        unitDisplayPreferences: pageSettings.unitDisplayPreferences
                    };
                    if(pageSettings.menuJQ && pageSettings.showSettingsButton) mapOptions.mapControls = [
                        { control: pageSettings.menuJQ, position: objSettings.settingsPosition }
                    ];
                    pageSettings.mapJQ.vrsReportMap(VRS.jQueryUIHelper.getReportMapOptions(mapOptions));
                    pageSettings.mapPlugin = pageSettings.mapJQ ? VRS.jQueryUIHelper.getReportMapPlugin(pageSettings.mapJQ) : null;
                    if(pageSettings.mapPlugin && !pageSettings.mapPlugin.isOpen()) pageSettings.mapPlugin = null;
                }

                // Create the report detail panel
                if(pageSettings.detailJQ) {
                    pageSettings.detailJQ.vrsReportDetail(VRS.jQueryUIHelper.getReportDetailOptions({
                        report:                 pageSettings.report,
                        unitDisplayPreferences: pageSettings.unitDisplayPreferences,
                        plotterOptions:         pageSettings.plotterOptions
                    }));
                    pageSettings.detailPlugin = VRS.jQueryUIHelper.getReportDetailPlugin(pageSettings.detailJQ);
                }

                // Create the report list panel
                if(pageSettings.listJQ) {
                    pageSettings.listJQ.vrsReportList(VRS.jQueryUIHelper.getReportListOptions({
                        report:                 pageSettings.report,
                        unitDisplayPreferences: pageSettings.unitDisplayPreferences
                    }));
                    pageSettings.listPlugin = VRS.jQueryUIHelper.getReportListPlugin(pageSettings.listJQ);
                }

                // Configure the layouts
                if(pageSettings.splittersJQ) self.initialisePageLayouts(pageSettings);
                if(pageSettings.pagesJQ) self.initialisePageManager(pageSettings);

                // Configure the settings menu
                self.buildSettingsMenu(pageSettings, pageSettings.settingsMenu.getTopLevelMenuItems());

                // Lift-off :)
                base.doEndInitialise(pageSettings);
                pageSettings.report.fetchPage();
            });
        };
        //endregion

        //region Settings menu - buildSettingsMenu, createCriteriaMenuEntry
        /**
         * Constructs the settings menu.
         * @param pageSettings
         * @param menuItems
         */
        this.buildSettingsMenu = function(pageSettings, menuItems)
        {
            if(pageSettings.showOptionsSetting)                                 menuItems.push(this.createCriteriaMenuEntry(pageSettings));
            if(pageSettings.showLanguageSetting)                                menuItems.push(base.createLocaleMenuEntry(pageSettings));
            menuItems.push(null);
            if(pageSettings.showLayoutSetting && pageSettings.layoutMenuItem)   menuItems.push(pageSettings.layoutMenuItem);
        };

        /**
         * Creates the 'show criteria' menu entry. This actually shows all of the options as well - best to keep them in
         * the same place.
         * @param pageSettings
         */
        this.createCriteriaMenuEntry = function(pageSettings)
        {
            var menuEntry = new VRS.MenuItem({
                name: 'options',
                labelKey: 'Criteria',
                vrsIcon:  'equalizer',
                clickCallback: function() {
                    if(objSettings.showOptionsInPage) VRS.pageManager.show(VRS.MobilePageName.Options);
                    else {
                        that.buildCriteriaOptionPanelPages(pageSettings);
                        $('<div/>')
                            .appendTo($('body'))
                            .vrsOptionDialog(VRS.jQueryUIHelper.getOptionDialogOptions({
                                pages: pageSettings.pages,
                                autoRemove: true
                        }));
                    }
                }
            });

            pageSettings.report.hookFailedNoCriteria(function() {
                menuEntry.clickCallback();
            }, this);

            return menuEntry;
        };
        //endregion

        //region Splitters - initialisePageLayouts
        /**
         * Adds all of the page layouts to the layout manager.
         * @param pageSettings
         */
        this.initialisePageLayouts = function(pageSettings)
        {
            // Classic layout - map top-left, detail top-right, list below both
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A00',
                labelKey: 'Layout1',
                layout: [
                    [
                        pageSettings.mapJQ,
                        { name: 'S2', vertical: true, savePane: 2, collapsePane: 1 },
                        pageSettings.detailJQ
                    ],
                    { name: 'S1', vertical: false, savePane: 2, collapsePane: 1 },
                    pageSettings.listJQ
                ]
            }));

            // Tall detail to the right, map above list to the left
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A01',
                labelKey: 'Layout2',
                layout: [
                    [
                        pageSettings.mapJQ,
                        { name: 'S2', vertical: false, savePane: 2, collapsePane: 1 },
                        pageSettings.listJQ
                    ],
                    { name: 'S1', vertical: true, savePane: 2, collapsePane: 2 },
                    pageSettings.detailJQ
                ]
            }));

            // Tall detail to the right, list above map to the left
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A02',
                labelKey: 'Layout3',
                layout: [
                    [
                        pageSettings.listJQ,
                        { name: 'S2', vertical: false, savePane: 1, collapsePane: 2 },
                        pageSettings.mapJQ
                    ],
                    { name: 'S1', vertical: true, savePane: 2, collapsePane: 2 },
                    pageSettings.detailJQ
                ]
            }));

            // Tall list to the left, map above detail to the right
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A03',
                labelKey: 'Layout4',
                layout: [
                    pageSettings.listJQ,
                    { name: 'S1', vertical: true, savePane: 1, collapsePane: 2 },
                    [
                        pageSettings.mapJQ,
                        { name: 'S2', vertical: false, savePane: 2, collapsePane: 1 },
                        pageSettings.detailJQ
                    ]
                ]
            }));

            // Tall list to the left, detail above map to the right
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A04',
                labelKey: 'Layout5',
                layout: [
                    pageSettings.listJQ,
                    { name: 'S1', vertical: true, savePane: 1, collapsePane: 2 },
                    [
                        pageSettings.detailJQ,
                        { name: 'S2', vertical: false, savePane: 1, collapsePane: 2 },
                        pageSettings.mapJQ
                    ]
                ]
            }));

            base.endLayoutInitialisation(pageSettings);
            base.createLayoutMenuEntry(pageSettings, [ 'Layout2', 'Layout4' ]);
        };
        //endregion

        //region Pages - initialisePageManager
        /**
         * Adds pages to the page manager. Only used by the mobile site.
         * @param pageSettings
         */
        this.initialisePageManager = function(pageSettings)
        {
            VRS.pageManager.initialise(pageSettings.pagesJQ);

            if(pageSettings.listJQ) {
                pageSettings.listJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                        element:                pageSettings.listJQ,
                        previousPageName:       VRS.MobilePageName.AircraftDetail,
                        previousPageLabelKey:   'FlightDetailShort',
                        titleLabelKey:          'TitleFlightsList',
                        nextPageName:           VRS.MobilePageName.AircraftDetail,
                        nextPageLabelKey:       'FlightDetailShort',
                        headerMenu:             pageSettings.settingsMenu
                    }));
                VRS.pageManager.addPage(new VRS.Page({
                    name:                   VRS.MobilePageName.AircraftList,
                    element:                pageSettings.listJQ
                }));
            }

            if(pageSettings.detailJQ) {
                pageSettings.detailJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                        element:                pageSettings.detailJQ,
                        previousPageName:       VRS.MobilePageName.AircraftList,
                        previousPageLabelKey:   'FlightsListShort',
                        titleLabelKey:          'TitleFlightDetail',
                        nextPageName:           VRS.MobilePageName.AircraftList,
                        nextPageLabelKey:       'FlightsListShort',
                        headerMenu:             pageSettings.settingsMenu
                    }));
                VRS.pageManager.addPage(new VRS.Page({
                    name:                   VRS.MobilePageName.AircraftDetail,
                    element:                pageSettings.detailJQ,
                    visibleCallback:        function(visible) {
                        if(!visible) pageSettings.detailPlugin.suspend(true);
                    },
                    afterVisibleCallback:   function() {
                        pageSettings.detailPlugin.suspend(false);
                    }
                }));
            }

            if(objSettings.showOptionsInPage) {
                var optionsContainer = $('<div/>');
                var pausedStateWhenMadeVisible = false;

                pageSettings.optionsPagePanelJQ = $('<div/>')
                    .appendTo('body')
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                        element:                optionsContainer,
                        previousPageName:       VRS.MobilePageName.AircraftList,
                        previousPageLabelKey:   'FlightsListShort',
                        titleLabelKey:          'Criteria',
                        nextPageName:           VRS.MobilePageName.AircraftDetail,
                        nextPageLabelKey:       'FlightDetailShort'
                    }));
                VRS.pageManager.addPage(new VRS.Page({
                    name:               VRS.MobilePageName.Options,
                    element:            pageSettings.optionsPagePanelJQ,
                    visibleCallback:    function(isVisible) {
                        if(!isVisible) {
                            var plugin = VRS.jQueryUIHelper.getOptionFormPlugin(optionsContainer);
                            if(plugin) plugin.destroy();
                            if(pageSettings.aircraftListFetcher) pageSettings.aircraftListFetcher.setPaused(pausedStateWhenMadeVisible);
                        } else {
                            if(pageSettings.aircraftListFetcher) {
                                pausedStateWhenMadeVisible = pageSettings.aircraftListFetcher.getPaused();
                                if(!pausedStateWhenMadeVisible) pageSettings.aircraftListFetcher.setPaused(true);
                            }

                            that.buildCriteriaOptionPanelPages(pageSettings);
                            optionsContainer.vrsOptionForm(VRS.jQueryUIHelper.getOptionFormOptions({
                                pages: pageSettings.pages,
                                showInAccordion: true
                            }));
                        }
                    }
                }));
            }

            // Hook the report fetched event and force a switch to the list page whenever the report is updated
            pageSettings.report.hookRowsFetched(function() { VRS.pageManager.show(VRS.MobilePageName.AircraftList); }, this);

            base.raisePageManagerInitialised(pageSettings);
        };
        //endregion

        //region buildCriteriaOptionPanelPages
        /**
         * Builds the criteria / options pages.
         * @param pageSettings
         */
        this.buildCriteriaOptionPanelPages = function(pageSettings)
        {
            pageSettings.pages = [];

            // Criteria page
            pageSettings.pages.push(new VRS.OptionPage({
                name:           'criteriaPage',
                titleKey:       'Criteria',
                displayOrder:   100,
                panes:          [ pageSettings.report.createOptionPane(100) ]
            }));

            // Detail panel options page
            if(pageSettings.detailPlugin) {
                pageSettings.pages.push(new VRS.OptionPage({
                    name:           'detailPanelPage',
                    titleKey:       'DetailPanel',
                    displayOrder:   200,
                    panes:          [ pageSettings.detailPlugin.createOptionPane(100) ]
                }));
            }

            // List options page
            if(pageSettings.listPlugin) {
                pageSettings.pages.push(new VRS.OptionPage({
                    name:           'listPage',
                    titleKey:       'PageList',
                    displayOrder:   300,
                    panes:          [ pageSettings.listPlugin.createOptionPane(100) ]
                }));
            }

            base.raiseOptionsPagesInitialised(pageSettings);
        };
        //endregion
    };
    VRS.BootstrapReport.prototype = VRS.objectHelper.subclassOf(VRS.Bootstrap);
}(window.VRS = window.VRS || {}, jQuery));
