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
 * @fileoverview The common bootstrap code for pages that show a map, a list of aircraft and aircraft details.
 */

(function(VRS, $, /** object= */ undefined)
{
    /**
     * The object that deals with creating all of the objects necessary for the map pages and wiring them all together.
     * @constructor
     * @augments VRS.Bootstrap
     * @param {Object}              objSettings
     * @param {string}              objSettings.configPrefix            MANDATORY - the configuration prefix to use.
     * @param {Object}             [objSettings.mapSettings]            Optional settings to send to vrs.vrsMap.open.
     * @param {bool}               [objSettings.suppressTitleUpdate]    True if we don't want the page title to show the count of aircraft.
     * @param {VRS.MapPosition}    [objSettings.settingsPosition]       Where to draw the settings button. Defaults to top-right.
     * @param {VRS.Alignment}      [objSettings.settingsMenuAlignment]  The alignment of the settings menu. Defaults to VRS.Alignment.Right.
     * @param {bool}               [objSettings.showOptionsInPage]      True if options are to be shown in a separate page, false if they're to be shown in a dialog. Default false.
     * @param {string}             [objSettings.reportUrl]              The base URL for reports.
     */
    VRS.BootstrapMap = function(objSettings)
    {
        var that = this;

        objSettings = $.extend({
            dispatcherName:         'VRS.BootstrapMap',
            suppressTitleUpdate:    false,
            settingsPosition:       VRS.MapPosition.TopRight,
            settingsMenuAlignment:  VRS.Alignment.Right,
            showOptionsInPage:      false
        }, objSettings);
        VRS.Bootstrap.call(this, objSettings);
        var base = this.getBase();

        //region initialise, beginLoadMap
        /**
         * Builds the page.
         * @param {object}                      pageSettings
         * @param {jQuery}                     [pageSettings.splittersJQ]               (Desktop) The jQuery element which will act as the parent for splitters. If not supplied then splitters are not used.
         * @param {jQuery}                     [pageSettings.pagesJQ]                   (Mobile)  The jQuery element while will act as the parent for pages. If not supplied then the page manager is not used.
         * @param {boolean}                    [pageSettings.showSettingsButton]        True if the user is shown the settings button, false if they are not. Defaults to true.
         * @param {VRS.Menu}                   [pageSettings.settingsMenu]              The menu that is shown on the settings menu. Created by the bootstrapper if missing. Always raises "createdSettingsMenu" regardless of whether it is supplied.
         * @param {jQuery}                     [pageSettings.mapJQ]                     The jQuery element where the map will be drawn. Defaults to null.
         * @param {VRS.vrsMap}                 [pageSettings.mapPlugin]                 A direct reference to the map plugin attached to the map jQuery object. Filled in by the bootstrapper, will be null if there is no map.
         * @param {Object}                     [pageSettings.mapSettings]               A settings object for the VRS.vrsMap map that will be used to plot aircraft.
         * @param {jQuery}                     [pageSettings.aircraftDetailJQ]          The jQuery element where the aircraft default panel will be drawn. Defaults to null.
         * @param {jQuery}                     [pageSettings.aircraftListJQ]            The jQuery element where the aircraft list will be drawn. Defaults to null.
         * @param {bool}                       [pageSettings.showOptionsSetting]        True if the user should be shown the settings map button. Defaults to true.
         * @param {bool}                       [pageSettings.showLanguageSetting]       True if the user should be allowed to switch languages. Defaults to true.
         * @param {bool}                       [pageSettings.showReceiversShortcut]     True if the user can change receivers from the menu. Defaults to true.
         * @param {bool}                       [pageSettings.showMovingMapSetting]      True if the user should be shown the moving map button. Defaults to true.
         * @param {bool}                       [pageSettings.showPauseSetting]          True if the user should be shown the pause button. Defaults to true.
         * @param {bool}                       [pageSettings.showAudioSetting]          True if the user should be shown the audio settings. Defaults to true.
         * @param {bool}                       [pageSettings.showRangeCircleSetting]    True if the user should be shown the option to toggle range circles. Defaults to true.
         * @param {bool}                       [pageSettings.showLayoutSetting]         True if the user is to be shown the change layout setting.
         * @param {bool}                       [pageSettings.showReportLinks]           True if the user should be shown the links to standard reports. Defaults to true.
         * @param {bool}                       [pageSettings.showAutoSelectToggle]      True if the user can toggle auto-select from the menu. Defaults to true.
         * @param {bool}                       [pageSettings.showGotoCurrentLocation]   True if the user is to be shown the option to jump to the current location.
         * @param {bool}                       [pageSettings.showGotoSelectedAircraft]  True if the user is to be shown the option to jump to the selected aircraft's location.
         * @param {jQuery}                     [pageSettings.infoWindowJQ]              Populated with the element used as the info window shown against selected aircraft.
         * @param {VRS.vrsAircraftInfoWindow}  [pageSettings.infoWindowPlugin]          Populated with a direct reference to the infoWindow plugin.
         * @param {VRS.AircraftListSorter}     [pageSettings.aircraftListSorter]        Populated with the aircraft list sorter that the site will use.
         * @param {VRS.PolarPlotter}           [pageSettings.polarPlotter]              Populated with the object that can manage polar plots for us.
         */
        this.initialise = function(pageSettings)
        {
            pageSettings = $.extend({
                splittersJQ: null,
                pagesJQ: null,
                mapJQ: null,
                mapSettings: {},
                showSettingsButton: true,
                settingsMenu: null,
                aircraftDetailJQ: null,
                aircraftListJQ: null,
                showOptionsSetting: true,
                showLanguageSetting: true,
                showLayoutSetting: true,
                showReceiversShortcut: true,
                showMovingMapSetting: true,
                showPauseSetting: true,
                showAudioSetting: true,
                showRangeCircleSetting: true,
                showReportLinks: true,
                showAutoSelectToggle: true,
                showGotoCurrentLocation: true,
                showGotoSelectedAircraft: true,
                showPolarPlotterSetting: true
            }, pageSettings);

            // Common startup stuff
            base.doStartInitialise(pageSettings);

            // Page title
            if(!objSettings.suppressTitleUpdate) document.title = VRS.$$.VirtualRadar;

            // Load the map. If the user has disabled the map then jump straight to the "map loaded" callback.
            if(!pageSettings.mapJQ) {
                that.mapLoaded(pageSettings);
            } else {
                pageSettings.mapSettings = $.extend(pageSettings.mapSettings, {
                    useStateOnOpen: true,
                    autoSaveState: true,
                    useServerDefaults: true,
                    loadMarkerWithLabel: true,
                    controlStyle: VRS.MapControlStyle.DropdownMenu,
                    afterOpen: $.proxy(function() {
                        base.raiseMapInitialising(pageSettings);
                        this.mapLoaded(pageSettings);
                        base.raiseMapInitialised(pageSettings);
                    }, this)
                }, objSettings.mapSettings || {});
                base.raiseMapSettingsInitialised(pageSettings);

                pageSettings.mapJQ.vrsMap(VRS.jQueryUIHelper.getMapOptions(pageSettings.mapSettings));
            }
        };
        //endregion

        //region mapLoaded
        /**
         * Called once the map has finished loading.
         * @param pageSettings
         */
        this.mapLoaded = function(pageSettings)
        {
            // Keep a reference to the map
            pageSettings.mapPlugin = pageSettings.mapJQ ? VRS.jQueryUIHelper.getMapPlugin(pageSettings.mapJQ) : null;
            if(pageSettings.mapPlugin && !pageSettings.mapPlugin.isOpen()) pageSettings.mapPlugin = null;
            base.raiseMapLoaded(pageSettings);

            // Set up the current location
            if(VRS.currentLocation) {
                if(pageSettings.mapJQ) VRS.currentLocation.setMapForApproximateLocation(pageSettings.mapJQ);
                VRS.currentLocation.loadAndApplyState();
            }

            // Add the settings button
            var settingsButton = base.createMapSettingsControl(pageSettings);
            if(pageSettings.mapPlugin) {
                if(settingsButton) pageSettings.mapPlugin.addControl(pageSettings.menuJQ, objSettings.settingsPosition);
            } else if(pageSettings.mapJQ) {
                pageSettings.mapJQ.children().first().prepend(settingsButton);
            }

            // Create the aircraft list
            pageSettings.aircraftList = new VRS.AircraftList();

            // Create the object that fetches the aircraft list.
            pageSettings.aircraftListFetcher = new VRS.AircraftListFetcher({
                aircraftList:       pageSettings.aircraftList,
                currentLocation:    VRS.currentLocation,
                mapJQ:              pageSettings.mapJQ,
                fetchFsxList:       VRS.globalOptions.isFlightSim
            });
            pageSettings.aircraftListFetcher.loadAndApplyState();

            // Create the polar plotter
            if(VRS.globalOptions.polarPlotEnabled && pageSettings.mapPlugin && pageSettings.aircraftListFetcher) {
                pageSettings.polarPlotter = new VRS.PolarPlotter({
                    map:                    pageSettings.mapPlugin,
                    aircraftListFetcher:    pageSettings.aircraftListFetcher,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.polarPlotter.startAutoRefresh();
            }

            // Create the timeout message box
            if(VRS.jQueryUIHelper.getTimeoutMessageBox) {
                pageSettings.timeoutMessageBox = $('<div/>')
                    .vrsTimeoutMessageBox({
                        aircraftListFetcher: pageSettings.aircraftListFetcher
                    })
                    .appendTo('body');
            }

            // Create the object that manages the page title changes
            if(!objSettings.suppressTitleUpdate) {
                pageSettings.titleUpdater = new VRS.TitleUpdater();
                pageSettings.titleUpdater.showAircraftListCount(pageSettings.aircraftList);
            }

            // Create the object that automatically selects aircraft
            if(VRS.AircraftAutoSelect) {
                pageSettings.aircraftAutoSelect = new VRS.AircraftAutoSelect(pageSettings.aircraftList);
                pageSettings.aircraftAutoSelect.loadAndApplyState();
            }

            // Create the object that can filter the aircraft list
            if(VRS.AircraftListFilter) {
                pageSettings.aircraftListFilter = new VRS.AircraftListFilter({
                    aircraftList: pageSettings.aircraftList,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.aircraftListFilter.loadAndApplyState();
            }

            // Create the object that plots aircraft on the map
            if(pageSettings.mapPlugin && VRS.AircraftPlotter) {
                pageSettings.aircraftPlotterOptions = new VRS.AircraftPlotterOptions(null);
                pageSettings.aircraftPlotterOptions.loadAndApplyState();

                pageSettings.aircraftPlotter = new VRS.AircraftPlotter({
                    plotterOptions:         pageSettings.aircraftPlotterOptions,
                    aircraftList:           pageSettings.aircraftList,
                    map:                    pageSettings.mapJQ,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.aircraftPlotter.refreshRangeCircles();
            }

            // Create the info window that gets shown against the selected aircraft.
            if(VRS.jQueryUIHelper.getAircraftInfoWindowPlugin && pageSettings.aircraftPlotterOptions) {
                pageSettings.infoWindowJQ = $('<div/>')
                    .vrsAircraftInfoWindow(VRS.jQueryUIHelper.getAircraftInfoWindowOptions({
                        aircraftList:           pageSettings.aircraftList,
                        aircraftPlotter:        pageSettings.aircraftPlotter,
                        unitDisplayPreferences: pageSettings.unitDisplayPreferences
                    }));
                pageSettings.infoWindowPlugin = VRS.jQueryUIHelper.getAircraftInfoWindowPlugin(pageSettings.infoWindowJQ);
            }

            // Create the aircraft list sorter. Originally this was a singleton but this made it rather tricky to set
            // the sort order by just setting VRS.globalOptions, so it's been moved into a per-instance object in
            // pageSettings.
            if(VRS.AircraftListSorter) {
                pageSettings.aircraftListSorter = new VRS.AircraftListSorter();
                pageSettings.aircraftListSorter.loadAndApplyState();
            }

            // Create the aircraft detail panel
            if(pageSettings.aircraftDetailJQ) {
                this.initialiseAircraftDetailPanel(pageSettings);
                base.raiseAircraftDetailPanelInitialised(pageSettings);
            }

            // Create the aircraft list panel
            if(pageSettings.aircraftListJQ) {
                this.initialiseAircraftListPanel(pageSettings);
                base.raiseAircraftListPanelInitialised(pageSettings);
            }

            // Create the object that calls out selected aircraft details
            if(VRS.Audio) {
                pageSettings.audio = new VRS.Audio();
                pageSettings.audio.loadAndApplyState();
                pageSettings.audio.annouceSelectedAircraftOnList(pageSettings.aircraftList);
            }

            // Build up the splitter layouts (desktop) or pages (mobile)
            if(pageSettings.pagesJQ) this.initialisePageManager(pageSettings);
            if(pageSettings.splittersJQ) {
                if(VRS.globalOptions.isFlightSim) this.initialiseFsxLayout(pageSettings);
                else                              this.initialisePageLayouts(pageSettings);
            }

            // Build up the menu options for the settings button
            if(pageSettings.settingsMenu) {
                // Originally I had planned to use fixed menu items, but it's more flexible if we rebuild the menu every
                // time it's needed
                pageSettings.settingsMenu.hookBeforeAddingFixedMenuItems(function(/** VRS.Menu */ unused, /** VRS.MenuItem[] */ menuItems) {
                    this.buildSettingsMenu(pageSettings, menuItems);
                }, this);
            }

            // All done
            base.doEndInitialise(pageSettings);

            // Start the first fetch and trigger the display of any initial polar plots
            pageSettings.aircraftListFetcher.setPaused(false);
            if(pageSettings.polarPlotter) pageSettings.polarPlotter.loadAndApplyState();
        };
        //endregion

        //region showOptionsSetting, showMovingMapSetting, showPauseSetting, showAudioSetting, showReportLinks
        /**
         * Populates the settings menu.
         * @param {*}               pageSettings
         * @param {VRS.MenuItem[]}  menuItems
         */
        this.buildSettingsMenu = function(pageSettings, menuItems)
        {
            if(pageSettings.showOptionsSetting)     menuItems.push(this.createOptionsMenuEntry(pageSettings));
            if(pageSettings.showLanguageSetting)    menuItems.push(base.createLocaleMenuEntry(pageSettings));
            if(pageSettings.showReceiversShortcut && VRS.globalOptions.aircraftListUserCanChangeFeeds && pageSettings.aircraftListFetcher.getFeeds().length > 1) {
                menuItems.push(this.createReceiversMenuEntry(pageSettings));
            }
            if(pageSettings.showPolarPlotterSetting && pageSettings.polarPlotter && pageSettings.polarPlotter.getPolarPlotterFeeds().length) {
                menuItems.push(this.createPolarPlotterMenuEntry(pageSettings));
            }
            menuItems.push(this.createShortcutsMenuEntry(pageSettings));
// Didn't have enough time to update the documentation for the website help... will come back to this in a future update.            menuItems.push(base.createHelpMenuEntry('WebSite/DesktopPage.aspx'));
            menuItems.push(null);


            if(pageSettings.showAudioSetting && pageSettings.audio) {
                menuItems.push(this.createAudioMenuEntry(pageSettings));
                pageSettings.settingsMenu.getTopLevelMenuItems().push(null);
            }

            if(pageSettings.showLayoutSetting && pageSettings.layoutMenuItem) {
                menuItems.push(pageSettings.layoutMenuItem);
                pageSettings.settingsMenu.getTopLevelMenuItems().push(null);
            }

            if(pageSettings.showReportLinks && (!VRS.serverConfig || VRS.serverConfig.reportsEnabled())) {
                menuItems.push(this.createReportsMenuEntry(pageSettings));
            }
        };

        /**
         * Creates a menu entry for the options screen.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createOptionsMenuEntry = function(pageSettings)
        {
            return new VRS.MenuItem({
                name: 'options',
                labelKey: 'Options',
                vrsIcon: 'equalizer',
                clickCallback: $.proxy(function() {
                    if(objSettings.showOptionsInPage) VRS.pageManager.show(VRS.MobilePageName.Options);
                    else {
                        this.buildOptionPanelPages(pageSettings);
                        $('<div/>')
                            .appendTo($('body'))
                            .vrsOptionDialog(VRS.jQueryUIHelper.getOptionDialogOptions({
                                pages: pageSettings.pages,
                                autoRemove: true
                            }));
                    }
                }, this)
            });
        };

        /**
         * Creates a menu entry for the shortcuts.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createShortcutsMenuEntry = function(pageSettings)
        {
            var menuEntry = new VRS.MenuItem({
                name: 'shortcuts',
                labelKey: 'Shortcuts'
            });
            var menuItems = menuEntry.subItems;

            if(pageSettings.mapPlugin) {
                if(pageSettings.showMovingMapSetting)                                                           menuItems.push(this.createMovingMapMenuEntry(pageSettings));
                if(pageSettings.showRangeCircleSetting && VRS.globalOptions.aircraftMarkerAllowRangeCircles)    menuItems.push(this.createRangeCirclesMenuEntry(pageSettings));
            }
            if(pageSettings.showPauseSetting && pageSettings.aircraftListFetcher)                               menuItems.push(this.createPauseMenuEntry(pageSettings));
            if(pageSettings.showGotoCurrentLocation && pageSettings.mapPlugin && VRS.currentLocation)           menuItems.push(this.createGotoCurrentLocationMenuEntry(pageSettings));
            menuItems.push(null);
            if(pageSettings.showGotoSelectedAircraft && pageSettings.mapPlugin)                                 menuItems.push(this.createGotoSelectedAircraftMenuEntry(pageSettings));
            if(pageSettings.showAutoSelectToggle)                                                               menuItems.push(this.createAutoSelectMenuEntry(pageSettings));

            return menuEntry;
        };

        /**
         * Creates a menu entry that lets the user change receiver.
         * @param {Object} pageSettings
         * @returns {VRS.MenuItem} menuEntry
         */
        this.createReceiversMenuEntry = function(pageSettings)
        {
            var result = new VRS.MenuItem({
                name: 'receivers',
                labelKey: 'Receiver'
            });

            var feeds = pageSettings.aircraftListFetcher.getSortedFeeds(true);
            var currentFeed = pageSettings.aircraftListFetcher.getActualFeedId();
            $.each(feeds, function(/** number */ idx, /** VRS_RECEIVER */ feed) {
                result.subItems.push(new VRS.MenuItem({
                    name: 'receiver-' + idx,
                    labelKey: function() { return feed.name; },
                    disabled: function() { return feed.id === currentFeed; },
                    checked: function() { return feed.id === currentFeed; },
                    clickCallback: function() {
                        pageSettings.aircraftListFetcher.setRequestFeedId(feed.id);
                        pageSettings.aircraftListFetcher.saveState();
                    }
                }));
            });

            return result;
        };

        /**
         * Creates a menu entry for displaying the polar plotter.
         * @param {Object}  pageSettings
         * @returns {VRS.MenuItem} menuEntry
         */
        this.createPolarPlotterMenuEntry = function(pageSettings)
        {
            var result = new VRS.MenuItem({
                name: 'polarPlotter',
                labelKey: 'ReceiverRange'
            });

            var feeds = pageSettings.polarPlotter.getSortedPolarPlotterFeeds();
            var countFeeds = feeds.length;
            $.each(feeds, function(/** number */ idx, /** VRS_RECEIVER */ feed) {
                var subMenu = null;
                if(countFeeds === 1) subMenu = result;
                else {
                    subMenu = new VRS.MenuItem({
                        name: 'polarPlotter-' + idx + '-' + feed.id,
                        labelKey: function() { return feed.name; }
                    });
                    result.subItems.push(subMenu);
                }

                $.each(pageSettings.polarPlotter.getAltitudeRangeConfigs(), function(/** number */ altIdx, /** VRS_POLAR_PLOT_CONFIG */ altitudeRange) {
                    if(altIdx === 1) subMenu.subItems.push(null);
                    subMenu.subItems.push(new VRS.MenuItem({
                        name: subMenu.name + '-' + altIdx,
                        labelKey: function() { return pageSettings.polarPlotter.getSliceRangeDescription(altitudeRange.low, altitudeRange.high); },
                        checked: function() {
                            return pageSettings.polarPlotter.isOnDisplay(feed.id, altitudeRange.low, altitudeRange.high);
                        },
                        clickCallback: function() {
                            pageSettings.polarPlotter.fetchAndToggleByIdentifiers([{
                                feedId: feed.id,
                                lowAlt: altitudeRange.low,
                                highAlt: altitudeRange.high
                            }]);
                        }
                    }));
                });

                subMenu.subItems.push(null);
                subMenu.subItems.push(new VRS.MenuItem({
                    name: subMenu.name + '-removeAll',
                    labelKey: 'RemoveAll',
                    clickCallback: function() { pageSettings.polarPlotter.removeAllSlicesForFeed(feed.id); }
                }));
            });

            if(countFeeds > 1) {
                result.subItems.push(null);
                result.subItems.push(new VRS.MenuItem({
                    name: 'polarPlotter-masterRemoveAll',
                    labelKey: 'RemoveAll',
                    clickCallback: function() { pageSettings.polarPlotter.removeAllSlicesForAllFeeds(); }
                }));
            }

            return result;
        };

        /**
         * Creates a menu entry to toggle the moving map.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createMovingMapMenuEntry = function(pageSettings)
        {
            return new VRS.MenuItem({
                name: 'movingMap',
                labelKey: 'MovingMap',
                checked: function() { return pageSettings.aircraftPlotter.getMovingMap(); },
                clickCallback: function() {
                    pageSettings.aircraftPlotter.setMovingMap(!pageSettings.aircraftPlotter.getMovingMap());
                }
            });
        };

        /**
         * Creates a menu entry to toggle range circles.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createRangeCirclesMenuEntry = function(pageSettings)
        {
            return new VRS.MenuItem({
                name: 'rangeCircles',
                labelKey: 'RangeCircles',
                checked: function() { return pageSettings.aircraftPlotterOptions.getShowRangeCircles(); },
                clickCallback: function() {
                    pageSettings.aircraftPlotterOptions.setShowRangeCircles(!pageSettings.aircraftPlotterOptions.getShowRangeCircles());
                    pageSettings.aircraftPlotterOptions.saveState();
                }
            });
        };

        /**
         * Creates a menu entry to toggle fetching of the aircraft list.
         * @param pageSettings
         */
        this.createPauseMenuEntry = function(pageSettings)
        {
            return new VRS.MenuItem({
                name: 'pause',
                labelKey: function() {
                    return pageSettings.aircraftListFetcher.getPaused() ? VRS.$$.Resume : VRS.$$.Pause;
                },
                vrsIcon: function() {
                    return pageSettings.aircraftListFetcher.getPaused() ? 'play' : 'pause';
                },
                clickCallback: function() {
                    pageSettings.aircraftListFetcher.setPaused(!pageSettings.aircraftListFetcher.getPaused());
                }
            });
        };

        /**
         * Creates a menu entry that moves the map to the current location.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createGotoCurrentLocationMenuEntry = function(pageSettings)
        {
            return new VRS.MenuItem({
                name: 'gotoCurrentLocation',
                labelKey: 'GotoCurrentLocation',
                disabled: function() { return VRS.currentLocation.getMapIsSupplyingLocation(); },
                clickCallback: function() { pageSettings.mapPlugin.panTo(VRS.currentLocation.getCurrentLocation()); }
            });
        };

        /**
         * Creates a menu entry that moves the map to the currently selected aircraft.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createGotoSelectedAircraftMenuEntry = function(pageSettings)
        {
            var selectedAircraft = pageSettings.aircraftList.getSelectedAircraft();
            return new VRS.MenuItem({
                name: 'gotoSelectedAircraft',
                labelKey: 'GotoSelectedAircraft',
                disabled: function() { return !selectedAircraft; },
                clickCallback: function() {
                    if(selectedAircraft) pageSettings.mapPlugin.panTo(VRS.currentLocation.getCurrentLocation());
                }
            });
        };

        /**
         * Creates a menu entry to toggle the auto-select setting.
         * @param pageSettings
         */
        this.createAutoSelectMenuEntry = function(pageSettings)
        {
            return new VRS.MenuItem({
                name: 'autoSelectToggle',
                labelKey: function () {
                    return pageSettings.aircraftAutoSelect.getEnabled() ? VRS.$$.DisableAutoSelect : VRS.$$.EnableAutoSelect;
                },
                clickCallback: function() {
                    pageSettings.aircraftAutoSelect.setEnabled(!pageSettings.aircraftAutoSelect.getEnabled());
                }
            });
        };

        /**
         * Creates a set of menu entries to control audio.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createAudioMenuEntry = function(pageSettings)
        {
            var audioMenuItem = new VRS.MenuItem({
                name:       'audio',
                labelKey:   'PaneAudio',
                vrsIcon:    'volume-high',
                suppress:   function() { return !pageSettings.audio.canPlayAudio(true); }
            });

            audioMenuItem.subItems.push(new VRS.MenuItem({
                name:           'audio-mute',
                labelKey:       function() { return pageSettings.audio.getMuted() ? VRS.$$.MuteOff : VRS.$$.MuteOn; },
                vrsIcon:        function() { return pageSettings.audio.getMuted() ? 'volume-medium' : 'volume-mute'; },
                clickCallback:  function() { pageSettings.audio.setMuted(!pageSettings.audio.getMuted()); }
            }));
            $.each([25, 50, 75, 100], function(/** Number */ idx, /** Number */ vol) {
                audioMenuItem.subItems.push(new VRS.MenuItem({
                    name:           'audio-vol' + vol,
                    labelKey:       'Volume' + vol,
                    disabled:       function() { return pageSettings.audio.getVolume() == vol/100; },
                    clickCallback:  function() { pageSettings.audio.setVolume(vol/100); pageSettings.audio.saveState(); },
                    vrsIcon:        function() {
                        switch(vol) {
                            case 25:    return 'volume-mute2';
                            case 50:    return 'volume-low';
                            case 75:    return 'volume-medium';
                            default:    return 'volume-high';
                        }
                    }
                }))
            });

            return audioMenuItem;
        };

        /**
         * Creates a set of menu entries for the reports.
         * @param pageSettings
         * @returns {VRS.MenuItem}
         */
        this.createReportsMenuEntry = function(pageSettings)
        {
            var selectedAircraft;

            var reportMenuItem = new VRS.MenuItem({
                name:           'report',
                labelKey:       'Reports',
                vrsIcon:        'print',
                noAutoClose:    true,
                clickCallback:  function() { selectedAircraft = pageSettings.aircraftList ? pageSettings.aircraftList.getSelectedAircraft() : null; }
            });

            reportMenuItem.subItems.push(new VRS.MenuItem({
                name:           'report-freeform',
                labelKey:       'ReportFreeForm',
                clickCallback:  function() {
                    window.open(VRS.browserHelper.formVrsPageUrl(objSettings.reportUrl),
                                VRS.browserHelper.getVrsPageTarget('vrsReport')
                    );
                }
            }));

            reportMenuItem.subItems.push(null);
            reportMenuItem.subItems.push(new VRS.MenuItem({
                name:           'report-today',
                labelKey:       'ReportTodaysFlights',
                clickCallback:  function() { window.open(VRS.browserHelper.formVrsPageUrl(objSettings.reportUrl, {
                    'date-L':   0,
                    'date-U':   0,
                    'sort1':    VRS.ReportSortColumn.Date,
                    'sortAsc1': 1,
                    'sort2':    'none'
                }), VRS.browserHelper.getVrsPageTarget('vrsReportToday'))}
            }));
            reportMenuItem.subItems.push(new VRS.MenuItem({
                name:           'report-yesterday',
                labelKey:       'ReportYesterdaysFlights',
                clickCallback:  function() { window.open(VRS.browserHelper.formVrsPageUrl(objSettings.reportUrl, {
                    'date-L':   -1,
                    'date-U':   -1,
                    'sort1':    VRS.ReportSortColumn.Date,
                    'sortAsc1': 1,
                    'sort2':    'none'
                }), VRS.browserHelper.getVrsPageTarget('vrsReportYesterday'))}
            }));

            if(pageSettings.aircraftList) {
                reportMenuItem.subItems.push(null);
                reportMenuItem.subItems.push(new VRS.MenuItem({
                    name:           'report-registration',
                    labelKey:       function() { return selectedAircraft && selectedAircraft.registration.val ? VRS.stringUtility.format(VRS.$$.ReportRegistrationValid, selectedAircraft.formatRegistration()) : VRS.$$.ReportRegistrationInvalid; },
                    disabled:       function() { return !selectedAircraft || !selectedAircraft.registration.val; },
                    clickCallback:  function() { window.open(VRS.browserHelper.formVrsPageUrl(objSettings.reportUrl, {
                        'reg-Q':    selectedAircraft.registration.val,  // this needs to be the first parameter
                        'sort1':    VRS.ReportSortColumn.Date,
                        'sortAsc1': 0,
                        'sort2':    'none'
                    }), VRS.browserHelper.getVrsPageTarget('vrsReportRegistration'))}
                }));
                reportMenuItem.subItems.push(new VRS.MenuItem({
                    name:           'report-icao',
                    labelKey:       function() { return selectedAircraft && selectedAircraft.icao.val ? VRS.stringUtility.format(VRS.$$.ReportIcaoValid, selectedAircraft.formatIcao()) : VRS.$$.ReportIcaoInvalid; },
                    disabled:       function() { return !selectedAircraft || !selectedAircraft.icao.val; },
                    clickCallback:  function() { window.open(VRS.browserHelper.formVrsPageUrl(objSettings.reportUrl, {
                        'icao-Q':   selectedAircraft.icao.val,
                        'sort1':    VRS.ReportSortColumn.Date,
                        'sortAsc1': 0,
                        'sort2':    'none'
                    }), VRS.browserHelper.getVrsPageTarget('vrsReportIcao'))}
                }));
                reportMenuItem.subItems.push(new VRS.MenuItem({
                    name:           'report-callsign',
                    labelKey:       function() { return selectedAircraft && selectedAircraft.callsign.val ? VRS.stringUtility.format(VRS.$$.ReportCallsignValid, selectedAircraft.formatCallsign(false)) : VRS.$$.ReportCallsignInvalid; },
                    disabled:       function() { return !selectedAircraft || !selectedAircraft.callsign.val; },
                    clickCallback:  function() { window.open(VRS.browserHelper.formVrsPageUrl(objSettings.reportUrl, {
                        'call-Q':   selectedAircraft.callsign.val,
                        'sort1':    VRS.ReportSortColumn.Date,
                        'sortAsc1': 0,
                        'sort2':    VRS.ReportSortColumn.Callsign,
                        'sortAsc2': 1,
                        'callPerms':'1'
                    }), VRS.browserHelper.getVrsPageTarget('vrsReportCallsign'))}
                }));
            }

            return reportMenuItem;
        };
        //endregion

        //region initialiseAircraftDetailPanel, initialiseAircraftListPanel
        /**
         * Constructs the aircraft detail panel and attaches it to the appropriate element.
         * @param pageSettings
         */
        this.initialiseAircraftDetailPanel = function(pageSettings)
        {
            pageSettings.aircraftDetailJQ.vrsAircraftDetail(VRS.jQueryUIHelper.getAircraftDetailOptions({
                aircraftList:           pageSettings.aircraftList,
                unitDisplayPreferences: pageSettings.unitDisplayPreferences,
                aircraftAutoSelect:     pageSettings.aircraftAutoSelect,
                mapPlugin:              pageSettings.mapPlugin,
                useSavedState:          true,
                mirrorMapJQ:            pageSettings.mapPlugin ? pageSettings.mapJQ : null,
                plotterOptions:         pageSettings.aircraftPlotterOptions
            }));
        };

        /**
         * Constructs the aircraft list panel and attaches it to the appropriate element.
         * @param pageSettings
         */
        this.initialiseAircraftListPanel = function(pageSettings)
        {
            /** @type {VRS_OPTIONS_AIRCRAFTLIST} */
            var options = {
                aircraftList:           pageSettings.aircraftList,
                aircraftListFetcher:    pageSettings.aircraftListFetcher,
                unitDisplayPreferences: pageSettings.unitDisplayPreferences,
                useSavedState:          true,
                sorter:                 pageSettings.aircraftListSorter,
                useSorterSavedState:    !!(VRS.aircraftListSorter)
            };
            if(VRS.globalOptions.isFlightSim) {
                options.showHideAircraftNotOnMap = false;
            }
            pageSettings.aircraftListJQ.vrsAircraftList(VRS.jQueryUIHelper.getAircraftListOptions(options));
        };
        //endregion

        //region buildOptionPanelPages
        /**
         * Constructs the options configuration pages.
         * @param pageSettings
         */
        this.buildOptionPanelPages = function(pageSettings)
        {
            pageSettings.pages = [];

            var generalPage = new VRS.OptionPage({
                name:           'vrsGeneralPage',
                titleKey:       'PageGeneral',
                displayOrder:   100
            });
            pageSettings.pages.push(generalPage);
            generalPage.addPane(pageSettings.aircraftListFetcher.createOptionPane(100));
            if(VRS.currentLocation && pageSettings.mapPlugin) generalPage.addPane(VRS.currentLocation.createOptionPane(200, pageSettings.mapJQ));
            generalPage.addPane(pageSettings.unitDisplayPreferences.createOptionPane(400));
            if(pageSettings.audio) generalPage.addPane(pageSettings.audio.createOptionPane(500));

            var mapPage = new VRS.OptionPage({
                name: 'vrsMapPage',
                titleKey: 'PageMapShort',
                displayOrder: 200
            });
            if(pageSettings.aircraftAutoSelect) {
                mapPage.addPane(pageSettings.aircraftAutoSelect.createOptionPane(100));
            }
            if(pageSettings.aircraftPlotterOptions) {
                if(VRS.currentLocation && VRS.globalOptions.aircraftMarkerAllowRangeCircles) mapPage.addPane(pageSettings.aircraftPlotterOptions.createOptionPaneForRangeCircles(200));
            }
            if(pageSettings.polarPlotter) {
                mapPage.addPane(pageSettings.polarPlotter.createOptionPane(300));
            }
            pageSettings.pages.push(mapPage);

            var aircraftDetail = pageSettings.aircraftDetailJQ ? VRS.jQueryUIHelper.getAircraftDetailPlugin(pageSettings.aircraftDetailJQ) : null;
            if(pageSettings.aircraftPlotterOptions || aircraftDetail) {
                var aircraftPage = new VRS.OptionPage({
                    name:           'vrsAircraftPage',
                    titleKey:       'PageAircraft',
                    displayOrder:   300
                });
                pageSettings.pages.push(aircraftPage);
                if(pageSettings.aircraftPlotterOptions) aircraftPage.addPane(pageSettings.aircraftPlotterOptions.createOptionPane(100));
                if(aircraftDetail)                      aircraftPage.addPane(aircraftDetail.createOptionPane(200));
                if(pageSettings.infoWindowPlugin)       aircraftPage.addPane(pageSettings.infoWindowPlugin.createOptionPane(300));
            }

            var aircraftList = pageSettings.aircraftListJQ ? VRS.jQueryUIHelper.getAircraftListPlugin(pageSettings.aircraftListJQ) : null;
            if(aircraftList) {
                pageSettings.pages.push(
                    new VRS.OptionPage({
                        name:           'vrsAircraftListPage',
                        titleKey:       'PageList',
                        displayOrder:   400,
                        panes:          [
                            aircraftList.createOptionPane(100)
                        ]
                    })
                );
            }

            if(pageSettings.aircraftListFilter) {
                pageSettings.pages.push(
                    new VRS.OptionPage({
                        name:           'vrsAircraftFilterPage',
                        titleKey:       'Filters',
                        displayOrder:   500,
                        panes:          [
                            pageSettings.aircraftListFilter.createOptionPane(100)
                        ]
                    })
                );
            }

            base.raiseOptionsPagesInitialised(pageSettings);
        };
        //endregion

        //region initialisePageLayouts
        /**
         * Adds the layout for the Flight Simulator X page.
         * @param pageSettings
         */
        this.initialiseFsxLayout = function(pageSettings)
        {
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A00',
                labelKey: 'Layout1',
                layout: [
                    pageSettings.mapJQ,
                    { name: 'S1', vertical: true, savePane: 2, collapsePane: 2, maxPane: 2, max: '80%', startSizePane: 2, startSize: 475 },
                    pageSettings.aircraftListJQ
                ]
            }));

            base.endLayoutInitialisation(pageSettings);
        };

        /**
         * Adds the page layouts to the layout manager.
         * @param pageSettings
         */
        this.initialisePageLayouts = function(pageSettings)
        {
            // Classic layout (map on left, detail above list on right)
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A00',
                labelKey: 'Layout1',
                layout: [
                    pageSettings.mapJQ,
                    { name: 'S1', vertical: true, savePane: 2, collapsePane: 2, maxPane: 2, max: '80%', startSizePane: 2, startSize: 550 },
                    [
                        pageSettings.aircraftDetailJQ,
                        { name: 'S2', vertical: false, fixedPane: 1, savePane: 1, collapsePane: 1 },
                        pageSettings.aircraftListJQ
                    ]
                ]
            }));

            // Tall detail layout (map above list on left, detail on right)
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A01',
                labelKey: 'Layout2',
                layout: [
                    [
                        pageSettings.mapJQ,
                        { name: 'S1', vertical: false, savePane: 2, startSizePane: 1, startSize: '50%', collapsePane: 2 },
                        pageSettings.aircraftListJQ
                    ],
                    { name: 'S2', vertical: true, savePane: 2, startSizePane: 2, startSize: 410, collapsePane: 2 },
                    pageSettings.aircraftDetailJQ
                ]
            }));

            // Tall detail layout (list above map on left, detail on right)
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A02',
                labelKey: 'Layout3',
                layout: [
                    [
                        pageSettings.aircraftListJQ,
                        { name: 'S1', vertical: false, savePane: 1, startSizePane: 2, startSize: '50%', collapsePane: 1 },
                        pageSettings.mapJQ
                    ],
                    { name: 'S2', vertical: true, savePane: 2, startSizePane: 2, startSize: 410, collapsePane: 2 },
                    pageSettings.aircraftDetailJQ
                ]
            }));

            // Tall list layout (map above detail on left, list on right)
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A03',
                labelKey: 'Layout4',
                layout: [
                    [
                        pageSettings.mapJQ,
                        { name: 'S1', vertical: false, fixedPane: 2, savePane: 2, collapsePane: 2, startSizePane: 1, startSize: '50%' },
                        pageSettings.aircraftDetailJQ
                    ],
                    { name: 'S2', vertical: true, fixedPane: 2, savePane: 2, startSizePane: 2, startSize: 550, collapsePane: 2 },
                    pageSettings.aircraftListJQ
                ]
            }));

            // Tall list layout (detail above map on left, list on right)
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A04',
                labelKey: 'Layout5',
                layout: [
                    [
                        pageSettings.aircraftDetailJQ,
                        { name: 'S1', vertical: false, fixedPane: 1, savePane: 1, collapsePane: 1, startSizePane: 2, startSize: '50%' },
                        pageSettings.mapJQ
                    ],
                    { name: 'S2', vertical: true, fixedPane: 2, savePane: 2, startSizePane: 2, startSize: 550, collapsePane: 2 },
                    pageSettings.aircraftListJQ
                ]
            }));

            var mapButtonParent = null;
            var mapButtonContainer = $('<div />').addClass('mapButtonContainer');
            var aircraftListPlugin = VRS.jQueryUIHelper.getAircraftListPlugin(pageSettings.aircraftListJQ);
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A05',
                labelKey: 'Layout6',
                layout: [
                    pageSettings.aircraftListJQ,
                    { name: 'S1', vertical: true, collapsePane: 2, savePane: 1, startSize: 550, fixedPane: 1, startSizePane: 1 },
                    pageSettings.aircraftDetailJQ
                ],
                onFocus: function() {
                    if(pageSettings.aircraftPlotter) pageSettings.aircraftPlotter.suspend(true);
                    if(pageSettings.mapJQ) {
                        pageSettings.mapJQ.hide();
                        if(pageSettings.mapButton) {
                            mapButtonParent = pageSettings.mapButton.parent();
                            pageSettings.mapButton.detach();
                            mapButtonContainer.append(pageSettings.mapButton);
                            aircraftListPlugin.prependElement(mapButtonContainer);
                        }
                    }
                },
                onBlur: function() {
                    if(pageSettings.mapJQ) {
                        if(pageSettings.mapButton) {
                            mapButtonContainer.detach();
                            pageSettings.mapButton.detach();
                            mapButtonParent.append(pageSettings.mapButton);
                        }
                        pageSettings.mapJQ.show();
                    }
                    if(pageSettings.aircraftPlotter) pageSettings.aircraftPlotter.suspend(false);
                }
            }));

            base.endLayoutInitialisation(pageSettings);
            base.createLayoutMenuEntry(pageSettings, [ 'Layout2', 'Layout4', 'Layout6' ]);
        };
        //endregion

        //region initialisePageManager
        /**
         * Adds pages to the page manager. Only used by the mobile site.
         * @param pageSettings
         */
        this.initialisePageManager = function(pageSettings)
        {
            VRS.pageManager.initialise(pageSettings.pagesJQ);

            if(pageSettings.mapJQ) {
                VRS.pageManager.addPage(new VRS.Page({
                    name:                   VRS.MobilePageName.Map,
                    element:                pageSettings.mapJQ,
                    visibleCallback:        function(isVisible) {
                        if(pageSettings.aircraftPlotter) pageSettings.aircraftPlotter.suspend(!isVisible);
                        if(pageSettings.infoWindowPlugin) pageSettings.infoWindowPlugin.suspend(!isVisible);
                    },
                    afterVisibleCallback:   function() {
                        if(pageSettings.mapPlugin) pageSettings.mapPlugin.refreshMap();
                        if(pageSettings.infoWindowPlugin) pageSettings.infoWindowPlugin.refreshDisplay();
                    }
                }));

                pageSettings.mapNextPageButton = $('<div/>')
                    .vrsMapNextPageButton(VRS.jQueryUIHelper.getMapNextPageButtonOptions({
                        nextPageName:           VRS.MobilePageName.AircraftList,
                        aircraftListFetcher:    pageSettings.aircraftListFetcher,
                        aircraftListFilter:     pageSettings.aircraftListFilter
                    }));
                if(pageSettings.mapPlugin) pageSettings.mapPlugin.addControl(pageSettings.mapNextPageButton, VRS.MapPosition.TopRight);
                else if(pageSettings.mapJQ) pageSettings.mapJQ.children().first().append(pageSettings.mapNextPageButton);
            }

            if(pageSettings.aircraftDetailJQ) {
                pageSettings.aircraftDetailPagePanelJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                        element:                pageSettings.aircraftDetailJQ,
                        previousPageName:       VRS.MobilePageName.AircraftList,
                        previousPageLabelKey:   'PageListShort',
                        titleLabelKey:          'TitleAircraftDetail',
                        nextPageName:           VRS.MobilePageName.Map,
                        nextPageLabelKey:       'PageMapShort',
                        headerMenu:             pageSettings.settingsMenu,
                        showFooterGap:          true
                    }));
                VRS.pageManager.addPage(new VRS.Page({
                    name:               VRS.MobilePageName.AircraftDetail,
                    element:            pageSettings.aircraftDetailPagePanelJQ,
                    visibleCallback:    function(isVisible) {
                        var aircraftDetailPlugin = VRS.jQueryUIHelper.getAircraftDetailPlugin(pageSettings.aircraftDetailJQ);
                        aircraftDetailPlugin.suspend(!isVisible);
                    }
                }));
            }

            if(pageSettings.aircraftListJQ) {
                pageSettings.aircraftListPagePanelJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                        element:                pageSettings.aircraftListJQ,
                        previousPageName:       VRS.MobilePageName.Map,
                        previousPageLabelKey:   'PageMapShort',
                        titleLabelKey:          'TitleAircraftList',
                        nextPageName:           VRS.MobilePageName.AircraftDetail,
                        nextPageLabelKey:       'AircraftDetailShort',
                        headerMenu:             pageSettings.settingsMenu
                    }));
                VRS.pageManager.addPage(new VRS.Page({
                    name:               VRS.MobilePageName.AircraftList,
                    element:            pageSettings.aircraftListPagePanelJQ,
                    visibleCallback:    function(isVisible) {
                        var aircraftListPlugin = VRS.jQueryUIHelper.getAircraftListPlugin(pageSettings.aircraftListJQ);
                        aircraftListPlugin.suspend(!isVisible);
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
                        previousPageName:       VRS.MobilePageName.Map,
                        previousPageLabelKey:   'PageMapShort',
                        titleLabelKey:          'Options',
                        nextPageName:           VRS.MobilePageName.AircraftList,
                        nextPageLabelKey:       'PageListShort'
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

                            that.buildOptionPanelPages(pageSettings);
                            optionsContainer.vrsOptionForm(VRS.jQueryUIHelper.getOptionFormOptions({
                                pages: pageSettings.pages,
                                showInAccordion: true
                            }));
                        }
                    }
                }));
            }

            base.raisePageManagerInitialised(pageSettings);
        };
        //endregion
    };
    VRS.BootstrapMap.prototype = VRS.objectHelper.subclassOf(VRS.Bootstrap);
}(window.VRS = window.VRS || {}, jQuery));
