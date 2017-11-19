var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var VRS;
(function (VRS) {
    var BootstrapMap = (function (_super) {
        __extends(BootstrapMap, _super);
        function BootstrapMap(settings) {
            var _this = this;
            settings = $.extend({
                dispatcherName: 'VRS.BootstrapMap',
                suppressTitleUpdate: false,
                settingsPosition: VRS.MapPosition.TopLeft,
                settingsMenuAlignment: VRS.Alignment.Left,
                showOptionsInPage: false
            }, settings);
            _this = _super.call(this, settings) || this;
            return _this;
        }
        BootstrapMap.prototype.initialise = function (pageSettings) {
            var _this = this;
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
            this.doStartInitialise(pageSettings, function () {
                if (!_this._Settings.suppressTitleUpdate) {
                    document.title = VRS.$$.VirtualRadar;
                }
                if (!pageSettings.mapJQ) {
                    _this.mapLoaded(pageSettings);
                }
                else {
                    pageSettings.mapSettings = $.extend(pageSettings.mapSettings, {
                        useStateOnOpen: true,
                        autoSaveState: true,
                        useServerDefaults: true,
                        loadMarkerWithLabel: true,
                        loadMarkerCluster: true,
                        controlStyle: VRS.MapControlStyle.DropdownMenu,
                        afterOpen: function () {
                            _this.raiseMapInitialising(pageSettings);
                            _this.mapLoaded(pageSettings);
                            _this.raiseMapInitialised(pageSettings);
                        }
                    }, _this._Settings.mapSettings || {});
                    _this.raiseMapSettingsInitialised(pageSettings);
                    pageSettings.mapJQ.vrsMap(VRS.jQueryUIHelper.getMapOptions(pageSettings.mapSettings));
                }
            });
        };
        BootstrapMap.prototype.mapLoaded = function (pageSettings) {
            pageSettings.mapPlugin = pageSettings.mapJQ ? VRS.jQueryUIHelper.getMapPlugin(pageSettings.mapJQ) : null;
            if (pageSettings.mapPlugin && !pageSettings.mapPlugin.isOpen()) {
                pageSettings.mapPlugin = null;
            }
            this.raiseMapLoaded(pageSettings);
            if (VRS.currentLocation) {
                if (pageSettings.mapJQ) {
                    VRS.currentLocation.setMapForApproximateLocation(pageSettings.mapJQ);
                }
                VRS.currentLocation.loadAndApplyState();
            }
            var settingsButton = this.createMapSettingsControl(pageSettings);
            if (pageSettings.mapPlugin) {
                if (settingsButton) {
                    pageSettings.mapPlugin.addControl(pageSettings.menuJQ, this._Settings.settingsPosition);
                }
            }
            else if (pageSettings.mapJQ) {
                pageSettings.mapJQ.children().first().prepend(settingsButton);
            }
            pageSettings.aircraftList = new VRS.AircraftList();
            pageSettings.aircraftListFetcher = new VRS.AircraftListFetcher({
                aircraftList: pageSettings.aircraftList,
                currentLocation: VRS.currentLocation,
                mapJQ: pageSettings.mapJQ,
                fetchFsxList: VRS.globalOptions.isFlightSim
            });
            pageSettings.aircraftListFetcher.loadAndApplyState();
            if (VRS.globalOptions.polarPlotEnabled && pageSettings.mapPlugin && pageSettings.aircraftListFetcher) {
                pageSettings.polarPlotter = new VRS.PolarPlotter({
                    map: pageSettings.mapPlugin,
                    aircraftListFetcher: pageSettings.aircraftListFetcher,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.polarPlotter.startAutoRefresh();
            }
            if (VRS.jQueryUIHelper.getTimeoutMessageBox) {
                pageSettings.timeoutMessageBox = $('<div/>')
                    .vrsTimeoutMessageBox({
                    aircraftListFetcher: pageSettings.aircraftListFetcher
                })
                    .appendTo('body');
            }
            if (!this._Settings.suppressTitleUpdate) {
                pageSettings.titleUpdater = new VRS.TitleUpdater();
                pageSettings.titleUpdater.showAircraftListCount(pageSettings.aircraftList);
            }
            if (VRS.AircraftAutoSelect) {
                pageSettings.aircraftAutoSelect = new VRS.AircraftAutoSelect(pageSettings.aircraftList);
                pageSettings.aircraftAutoSelect.loadAndApplyState();
                if (purl) {
                    var preselectIcao = $.url().param('icao');
                    if (preselectIcao !== null && preselectIcao !== undefined && preselectIcao.length === 6) {
                        pageSettings.aircraftAutoSelect.setSelectAircraftByIcao(preselectIcao.toUpperCase());
                        pageSettings.aircraftAutoSelect.setAutoClearSelectAircraftByIcao(true);
                    }
                }
            }
            if (VRS.AircraftListFilter) {
                pageSettings.aircraftListFilter = new VRS.AircraftListFilter({
                    aircraftList: pageSettings.aircraftList,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.aircraftListFilter.loadAndApplyState();
            }
            if (pageSettings.mapPlugin && VRS.AircraftPlotter) {
                pageSettings.aircraftPlotterOptions = new VRS.AircraftPlotterOptions({
                    map: pageSettings.mapPlugin
                });
                pageSettings.aircraftPlotterOptions.loadAndApplyState();
                pageSettings.aircraftPlotter = new VRS.AircraftPlotter({
                    plotterOptions: pageSettings.aircraftPlotterOptions,
                    aircraftList: pageSettings.aircraftList,
                    map: pageSettings.mapJQ,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.aircraftPlotter.refreshRangeCircles();
                if (purl) {
                    var initialMovingMapStatus = $.url().param('movingMap');
                    if (initialMovingMapStatus !== null && initialMovingMapStatus !== undefined) {
                        pageSettings.aircraftPlotter.setMovingMap(initialMovingMapStatus !== '0');
                    }
                }
            }
            if (VRS.jQueryUIHelper.getAircraftInfoWindowPlugin && pageSettings.aircraftPlotterOptions) {
                pageSettings.infoWindowJQ = $('<div/>')
                    .vrsAircraftInfoWindow(VRS.jQueryUIHelper.getAircraftInfoWindowOptions({
                    aircraftList: pageSettings.aircraftList,
                    aircraftPlotter: pageSettings.aircraftPlotter,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                }));
                pageSettings.infoWindowPlugin = VRS.jQueryUIHelper.getAircraftInfoWindowPlugin(pageSettings.infoWindowJQ);
            }
            if (VRS.AircraftListSorter) {
                pageSettings.aircraftListSorter = new VRS.AircraftListSorter();
                pageSettings.aircraftListSorter.loadAndApplyState();
            }
            if (pageSettings.aircraftDetailJQ) {
                this.initialiseAircraftDetailPanel(pageSettings);
                this.raiseAircraftDetailPanelInitialised(pageSettings);
            }
            if (pageSettings.aircraftListJQ) {
                this.initialiseAircraftListPanel(pageSettings);
                this.raiseAircraftListPanelInitialised(pageSettings);
            }
            if (VRS.AudioWrapper) {
                pageSettings.audio = new VRS.AudioWrapper();
                pageSettings.audio.loadAndApplyState();
                pageSettings.audio.annouceSelectedAircraftOnList(pageSettings.aircraftList);
            }
            if (pageSettings.pagesJQ)
                this.initialisePageManager(pageSettings);
            if (pageSettings.splittersJQ) {
                if (VRS.globalOptions.isFlightSim)
                    this.initialiseFsxLayout(pageSettings);
                else
                    this.initialisePageLayouts(pageSettings);
            }
            if (pageSettings.settingsMenu) {
                pageSettings.settingsMenu.hookBeforeAddingFixedMenuItems(function (unused, menuItems) {
                    this.buildSettingsMenu(pageSettings, menuItems);
                }, this);
            }
            this.doEndInitialise(pageSettings);
            pageSettings.aircraftListFetcher.setPaused(false);
            if (pageSettings.polarPlotter) {
                pageSettings.polarPlotter.loadAndApplyState();
            }
        };
        BootstrapMap.prototype.buildSettingsMenu = function (pageSettings, menuItems) {
            if (pageSettings.showOptionsSetting) {
                menuItems.push(this.createOptionsMenuEntry(pageSettings));
            }
            if (pageSettings.showLanguageSetting) {
                menuItems.push(this.createLocaleMenuEntry(pageSettings));
            }
            if (pageSettings.showReceiversShortcut && VRS.globalOptions.aircraftListUserCanChangeFeeds && pageSettings.aircraftListFetcher.getFeeds().length > 1) {
                menuItems.push(this.createReceiversMenuEntry(pageSettings));
            }
            if (pageSettings.showPolarPlotterSetting && pageSettings.polarPlotter && pageSettings.polarPlotter.getPolarPlotterFeeds().length) {
                menuItems.push(this.createPolarPlotterMenuEntry(pageSettings));
            }
            menuItems.push(this.createShortcutsMenuEntry(pageSettings));
            menuItems.push(null);
            if (pageSettings.showAudioSetting && pageSettings.audio) {
                menuItems.push(this.createAudioMenuEntry(pageSettings));
                pageSettings.settingsMenu.getTopLevelMenuItems().push(null);
            }
            if (pageSettings.showLayoutSetting && pageSettings.layoutMenuItem) {
                menuItems.push(pageSettings.layoutMenuItem);
                pageSettings.settingsMenu.getTopLevelMenuItems().push(null);
            }
            if (pageSettings.showReportLinks && (!VRS.serverConfig || VRS.serverConfig.reportsEnabled())) {
                menuItems.push(this.createReportsMenuEntry(pageSettings));
            }
        };
        BootstrapMap.prototype.createOptionsMenuEntry = function (pageSettings) {
            var _this = this;
            return new VRS.MenuItem({
                name: 'options',
                labelKey: 'Options',
                vrsIcon: 'equalizer',
                clickCallback: function () {
                    if (_this._Settings.showOptionsInPage) {
                        VRS.pageManager.show(VRS.MobilePageName.Options);
                    }
                    else {
                        _this.buildOptionPanelPages(pageSettings);
                        $('<div/>')
                            .appendTo($('body'))
                            .vrsOptionDialog(VRS.jQueryUIHelper.getOptionDialogOptions({
                            pages: pageSettings.pages,
                            autoRemove: true
                        }));
                    }
                }
            });
        };
        BootstrapMap.prototype.createShortcutsMenuEntry = function (pageSettings) {
            var menuEntry = new VRS.MenuItem({
                name: 'shortcuts',
                labelKey: 'Shortcuts'
            });
            var menuItems = menuEntry.subItems;
            if (pageSettings.mapPlugin) {
                if (pageSettings.showMovingMapSetting)
                    menuItems.push(this.createMovingMapMenuEntry(pageSettings));
                if (pageSettings.showRangeCircleSetting && VRS.globalOptions.aircraftMarkerAllowRangeCircles)
                    menuItems.push(this.createRangeCirclesMenuEntry(pageSettings));
            }
            if (pageSettings.showPauseSetting && pageSettings.aircraftListFetcher)
                menuItems.push(this.createPauseMenuEntry(pageSettings));
            if (pageSettings.showGotoCurrentLocation && pageSettings.mapPlugin && VRS.currentLocation)
                menuItems.push(this.createGotoCurrentLocationMenuEntry(pageSettings));
            menuItems.push(null);
            if (pageSettings.showGotoSelectedAircraft && pageSettings.mapPlugin)
                menuItems.push(this.createGotoSelectedAircraftMenuEntry(pageSettings));
            if (pageSettings.showAutoSelectToggle)
                menuItems.push(this.createAutoSelectMenuEntry(pageSettings));
            return menuEntry;
        };
        BootstrapMap.prototype.createReceiversMenuEntry = function (pageSettings) {
            var result = new VRS.MenuItem({
                name: 'receivers',
                labelKey: 'Receiver'
            });
            var feeds = pageSettings.aircraftListFetcher.getSortedFeeds(true);
            var currentFeed = pageSettings.aircraftListFetcher.getActualFeedId();
            $.each(feeds, function (idx, feed) {
                result.subItems.push(new VRS.MenuItem({
                    name: 'receiver-' + idx,
                    labelKey: function () { return feed.name; },
                    disabled: function () { return feed.id === currentFeed; },
                    checked: function () { return feed.id === currentFeed; },
                    clickCallback: function () {
                        pageSettings.aircraftListFetcher.setRequestFeedId(feed.id);
                        pageSettings.aircraftListFetcher.saveState();
                    }
                }));
            });
            return result;
        };
        BootstrapMap.prototype.createPolarPlotterMenuEntry = function (pageSettings) {
            var result = new VRS.MenuItem({
                name: 'polarPlotter',
                labelKey: 'ReceiverRange'
            });
            var feeds = pageSettings.polarPlotter.getSortedPolarPlotterFeeds();
            var countFeeds = feeds.length;
            $.each(feeds, function (idx, feed) {
                var subMenu = null;
                if (countFeeds === 1) {
                    subMenu = result;
                }
                else {
                    subMenu = new VRS.MenuItem({
                        name: 'polarPlotter-' + idx + '-' + feed.id,
                        labelKey: function () { return feed.name; }
                    });
                    result.subItems.push(subMenu);
                }
                var onDisplay = [];
                $.each(pageSettings.polarPlotter.getAltitudeRangeConfigs(), function (altIdx, altitudeRange) {
                    if (altIdx === 1) {
                        subMenu.subItems.push(null);
                    }
                    var isChecked = pageSettings.polarPlotter.isOnDisplay(feed.id, altitudeRange.low, altitudeRange.high);
                    subMenu.subItems.push(new VRS.MenuItem({
                        name: subMenu.name + '-' + altIdx,
                        labelKey: function () {
                            return pageSettings.polarPlotter.getSliceRangeDescription(altitudeRange.low, altitudeRange.high);
                        },
                        checked: function () {
                            return isChecked;
                        },
                        clickCallback: function () {
                            isChecked = pageSettings.polarPlotter.fetchAndToggleByIdentifiers([{
                                    feedId: feed.id,
                                    lowAlt: altitudeRange.low,
                                    highAlt: altitudeRange.high
                                }]);
                        },
                        noAutoClose: true
                    }));
                });
            });
            if (countFeeds > 1) {
                result.subItems.push(null);
                result.subItems.push(new VRS.MenuItem({
                    name: 'polarPlotter-masterRemoveAll',
                    labelKey: 'RemoveAll',
                    clickCallback: function () {
                        pageSettings.polarPlotter.removeAllSlicesForAllFeeds();
                    }
                }));
            }
            return result;
        };
        BootstrapMap.prototype.createMovingMapMenuEntry = function (pageSettings) {
            return new VRS.MenuItem({
                name: 'movingMap',
                labelKey: 'MovingMap',
                checked: function () {
                    return pageSettings.aircraftPlotter.getMovingMap();
                },
                clickCallback: function () {
                    pageSettings.aircraftPlotter.setMovingMap(!pageSettings.aircraftPlotter.getMovingMap());
                },
                noAutoClose: true
            });
        };
        BootstrapMap.prototype.createRangeCirclesMenuEntry = function (pageSettings) {
            return new VRS.MenuItem({
                name: 'rangeCircles',
                labelKey: 'RangeCircles',
                checked: function () {
                    return pageSettings.aircraftPlotterOptions.getShowRangeCircles();
                },
                clickCallback: function () {
                    pageSettings.aircraftPlotterOptions.setShowRangeCircles(!pageSettings.aircraftPlotterOptions.getShowRangeCircles());
                    pageSettings.aircraftPlotterOptions.saveState();
                },
                noAutoClose: true
            });
        };
        BootstrapMap.prototype.createPauseMenuEntry = function (pageSettings) {
            return new VRS.MenuItem({
                name: 'pause',
                labelKey: function () {
                    return pageSettings.aircraftListFetcher.getPaused() ? VRS.$$.Resume : VRS.$$.Pause;
                },
                vrsIcon: function () {
                    return pageSettings.aircraftListFetcher.getPaused() ? 'play' : 'pause';
                },
                clickCallback: function () {
                    pageSettings.aircraftListFetcher.setPaused(!pageSettings.aircraftListFetcher.getPaused());
                },
                noAutoClose: true
            });
        };
        BootstrapMap.prototype.createGotoCurrentLocationMenuEntry = function (pageSettings) {
            return new VRS.MenuItem({
                name: 'gotoCurrentLocation',
                labelKey: 'GotoCurrentLocation',
                disabled: function () { return VRS.currentLocation.getMapIsSupplyingLocation(); },
                clickCallback: function () { pageSettings.mapPlugin.panTo(VRS.currentLocation.getCurrentLocation()); }
            });
        };
        BootstrapMap.prototype.createGotoSelectedAircraftMenuEntry = function (pageSettings) {
            var selectedAircraft = pageSettings.aircraftList.getSelectedAircraft();
            return new VRS.MenuItem({
                name: 'gotoSelectedAircraft',
                labelKey: 'GotoSelectedAircraft',
                disabled: function () { return !selectedAircraft; },
                clickCallback: function () {
                    if (selectedAircraft) {
                        pageSettings.mapPlugin.panTo(selectedAircraft.getPosition());
                    }
                }
            });
        };
        BootstrapMap.prototype.createAutoSelectMenuEntry = function (pageSettings) {
            return new VRS.MenuItem({
                name: 'autoSelectToggle',
                labelKey: function () {
                    return pageSettings.aircraftAutoSelect.getEnabled() ? VRS.$$.DisableAutoSelect : VRS.$$.EnableAutoSelect;
                },
                clickCallback: function () {
                    pageSettings.aircraftAutoSelect.setEnabled(!pageSettings.aircraftAutoSelect.getEnabled());
                }
            });
        };
        BootstrapMap.prototype.createAudioMenuEntry = function (pageSettings) {
            var audioMenuItem = new VRS.MenuItem({
                name: 'audio',
                labelKey: 'PaneAudio',
                vrsIcon: 'volume-high',
                suppress: function () { return !pageSettings.audio.canPlayAudio(true); }
            });
            audioMenuItem.subItems.push(new VRS.MenuItem({
                name: 'audio-mute',
                labelKey: function () { return pageSettings.audio.getMuted() ? VRS.$$.MuteOff : VRS.$$.MuteOn; },
                vrsIcon: function () { return pageSettings.audio.getMuted() ? 'volume-medium' : 'volume-mute'; },
                clickCallback: function () { pageSettings.audio.setMuted(!pageSettings.audio.getMuted()); }
            }));
            $.each([25, 50, 75, 100], function (idx, vol) {
                audioMenuItem.subItems.push(new VRS.MenuItem({
                    name: 'audio-vol' + vol,
                    labelKey: 'Volume' + vol,
                    disabled: function () { return pageSettings.audio.getVolume() == vol / 100; },
                    clickCallback: function () { pageSettings.audio.setVolume(vol / 100); pageSettings.audio.saveState(); },
                    vrsIcon: function () {
                        switch (vol) {
                            case 25: return 'volume-mute2';
                            case 50: return 'volume-low';
                            case 75: return 'volume-medium';
                            default: return 'volume-high';
                        }
                    }
                }));
            });
            return audioMenuItem;
        };
        BootstrapMap.prototype.createReportsMenuEntry = function (pageSettings) {
            var _this = this;
            var selectedAircraft;
            var reportMenuItem = new VRS.MenuItem({
                name: 'report',
                labelKey: 'Reports',
                vrsIcon: 'print',
                noAutoClose: true,
                clickCallback: function () {
                    selectedAircraft = pageSettings.aircraftList ? pageSettings.aircraftList.getSelectedAircraft() : null;
                }
            });
            reportMenuItem.subItems.push(new VRS.MenuItem({
                name: 'report-freeform',
                labelKey: 'ReportFreeForm',
                clickCallback: function () {
                    window.open(VRS.browserHelper.formVrsPageUrl(_this._Settings.reportUrl), VRS.browserHelper.getVrsPageTarget('vrsReport'));
                }
            }));
            reportMenuItem.subItems.push(null);
            reportMenuItem.subItems.push(new VRS.MenuItem({
                name: 'report-today',
                labelKey: 'ReportTodaysFlights',
                clickCallback: function () {
                    window.open(VRS.browserHelper.formVrsPageUrl(_this._Settings.reportUrl, {
                        'date-L': 0,
                        'date-U': 0,
                        'sort1': VRS.ReportSortColumn.Date,
                        'sortAsc1': 1,
                        'sort2': 'none'
                    }), VRS.browserHelper.getVrsPageTarget('vrsReportToday'));
                }
            }));
            reportMenuItem.subItems.push(new VRS.MenuItem({
                name: 'report-yesterday',
                labelKey: 'ReportYesterdaysFlights',
                clickCallback: function () {
                    window.open(VRS.browserHelper.formVrsPageUrl(_this._Settings.reportUrl, {
                        'date-L': -1,
                        'date-U': -1,
                        'sort1': VRS.ReportSortColumn.Date,
                        'sortAsc1': 1,
                        'sort2': 'none'
                    }), VRS.browserHelper.getVrsPageTarget('vrsReportYesterday'));
                }
            }));
            if (pageSettings.aircraftList) {
                reportMenuItem.subItems.push(null);
                reportMenuItem.subItems.push(new VRS.MenuItem({
                    name: 'report-registration',
                    labelKey: function () { return selectedAircraft && selectedAircraft.registration.val ? VRS.stringUtility.format(VRS.$$.ReportRegistrationValid, selectedAircraft.formatRegistration()) : VRS.$$.ReportRegistrationInvalid; },
                    disabled: function () { return !selectedAircraft || !selectedAircraft.registration.val; },
                    clickCallback: function () {
                        window.open(VRS.browserHelper.formVrsPageUrl(_this._Settings.reportUrl, {
                            'reg-Q': selectedAircraft.registration.val,
                            'sort1': VRS.ReportSortColumn.Date,
                            'sortAsc1': 0,
                            'sort2': 'none'
                        }), VRS.browserHelper.getVrsPageTarget('vrsReportRegistration'));
                    }
                }));
                reportMenuItem.subItems.push(new VRS.MenuItem({
                    name: 'report-icao',
                    labelKey: function () { return selectedAircraft && selectedAircraft.icao.val ? VRS.stringUtility.format(VRS.$$.ReportIcaoValid, selectedAircraft.formatIcao()) : VRS.$$.ReportIcaoInvalid; },
                    disabled: function () { return !selectedAircraft || !selectedAircraft.icao.val; },
                    clickCallback: function () {
                        window.open(VRS.browserHelper.formVrsPageUrl(_this._Settings.reportUrl, {
                            'icao-Q': selectedAircraft.icao.val,
                            'sort1': VRS.ReportSortColumn.Date,
                            'sortAsc1': 0,
                            'sort2': 'none'
                        }), VRS.browserHelper.getVrsPageTarget('vrsReportIcao'));
                    }
                }));
                reportMenuItem.subItems.push(new VRS.MenuItem({
                    name: 'report-callsign',
                    labelKey: function () { return selectedAircraft && selectedAircraft.callsign.val ? VRS.stringUtility.format(VRS.$$.ReportCallsignValid, selectedAircraft.formatCallsign(false)) : VRS.$$.ReportCallsignInvalid; },
                    disabled: function () { return !selectedAircraft || !selectedAircraft.callsign.val; },
                    clickCallback: function () {
                        window.open(VRS.browserHelper.formVrsPageUrl(_this._Settings.reportUrl, {
                            'call-Q': selectedAircraft.callsign.val,
                            'sort1': VRS.ReportSortColumn.Date,
                            'sortAsc1': 0,
                            'sort2': VRS.ReportSortColumn.Callsign,
                            'sortAsc2': 1,
                            'callPerms': '1'
                        }), VRS.browserHelper.getVrsPageTarget('vrsReportCallsign'));
                    }
                }));
            }
            return reportMenuItem;
        };
        BootstrapMap.prototype.initialiseAircraftDetailPanel = function (pageSettings) {
            pageSettings.aircraftDetailJQ.vrsAircraftDetail(VRS.jQueryUIHelper.getAircraftDetailOptions({
                aircraftList: pageSettings.aircraftList,
                unitDisplayPreferences: pageSettings.unitDisplayPreferences,
                aircraftAutoSelect: pageSettings.aircraftAutoSelect,
                mapPlugin: pageSettings.mapPlugin,
                useSavedState: true,
                mirrorMapJQ: pageSettings.mapPlugin ? pageSettings.mapJQ : null,
                plotterOptions: pageSettings.aircraftPlotterOptions
            }));
        };
        BootstrapMap.prototype.initialiseAircraftListPanel = function (pageSettings) {
            var options = {
                aircraftList: pageSettings.aircraftList,
                aircraftListFetcher: pageSettings.aircraftListFetcher,
                unitDisplayPreferences: pageSettings.unitDisplayPreferences,
                useSavedState: true,
                sorter: pageSettings.aircraftListSorter,
                useSorterSavedState: !!(VRS.aircraftListSorter)
            };
            if (VRS.globalOptions.isFlightSim) {
                options.showHideAircraftNotOnMap = false;
            }
            pageSettings.aircraftListJQ.vrsAircraftList(VRS.jQueryUIHelper.getAircraftListOptions(options));
        };
        BootstrapMap.prototype.buildOptionPanelPages = function (pageSettings) {
            pageSettings.pages = [];
            var generalPage = new VRS.OptionPage({
                name: 'vrsGeneralPage',
                titleKey: 'PageGeneral',
                displayOrder: 100
            });
            pageSettings.pages.push(generalPage);
            generalPage.addPane(pageSettings.aircraftListFetcher.createOptionPane(100));
            if (VRS.currentLocation && pageSettings.mapPlugin) {
                generalPage.addPane(VRS.currentLocation.createOptionPane(200, pageSettings.mapJQ));
            }
            generalPage.addPane(pageSettings.unitDisplayPreferences.createOptionPane(400));
            if (pageSettings.audio) {
                generalPage.addPane(pageSettings.audio.createOptionPane(500));
            }
            var mapPage = new VRS.OptionPage({
                name: 'vrsMapPage',
                titleKey: 'PageMapShort',
                displayOrder: 200
            });
            if (pageSettings.aircraftAutoSelect) {
                mapPage.addPane(pageSettings.aircraftAutoSelect.createOptionPane(100));
            }
            if (pageSettings.aircraftPlotterOptions) {
                if (VRS.currentLocation && VRS.globalOptions.aircraftMarkerAllowRangeCircles) {
                    mapPage.addPane(pageSettings.aircraftPlotterOptions.createOptionPaneForRangeCircles(200));
                }
            }
            if (pageSettings.polarPlotter) {
                mapPage.addPane(pageSettings.polarPlotter.createOptionPane(300));
            }
            pageSettings.pages.push(mapPage);
            var aircraftDetail = pageSettings.aircraftDetailJQ ? VRS.jQueryUIHelper.getAircraftDetailPlugin(pageSettings.aircraftDetailJQ) : null;
            if (pageSettings.aircraftPlotterOptions || aircraftDetail) {
                var aircraftPage = new VRS.OptionPage({
                    name: 'vrsAircraftPage',
                    titleKey: 'PageAircraft',
                    displayOrder: 300
                });
                pageSettings.pages.push(aircraftPage);
                if (pageSettings.aircraftPlotterOptions)
                    aircraftPage.addPane(pageSettings.aircraftPlotterOptions.createOptionPane(100));
                if (aircraftDetail)
                    aircraftPage.addPane(aircraftDetail.createOptionPane(200));
                if (pageSettings.infoWindowPlugin)
                    aircraftPage.addPane(pageSettings.infoWindowPlugin.createOptionPane(300));
            }
            var aircraftList = pageSettings.aircraftListJQ ? VRS.jQueryUIHelper.getAircraftListPlugin(pageSettings.aircraftListJQ) : null;
            if (aircraftList) {
                pageSettings.pages.push(new VRS.OptionPage({
                    name: 'vrsAircraftListPage',
                    titleKey: 'PageList',
                    displayOrder: 400,
                    panes: [
                        aircraftList.createOptionPane(100)
                    ]
                }));
            }
            if (pageSettings.aircraftListFilter) {
                pageSettings.pages.push(new VRS.OptionPage({
                    name: 'vrsAircraftFilterPage',
                    titleKey: 'Filters',
                    displayOrder: 500,
                    panes: [
                        pageSettings.aircraftListFilter.createOptionPane(100)
                    ]
                }));
            }
            this.raiseOptionsPagesInitialised(pageSettings);
        };
        BootstrapMap.prototype.initialiseFsxLayout = function (pageSettings) {
            VRS.layoutManager.registerLayout(new VRS.Layout({
                name: 'vrsLayout-A00',
                labelKey: 'Layout1',
                layout: [
                    pageSettings.mapJQ,
                    { name: 'S1', vertical: true, savePane: 2, collapsePane: 2, maxPane: 2, max: '80%', startSizePane: 2, startSize: 475 },
                    pageSettings.aircraftListJQ
                ]
            }));
            this.endLayoutInitialisation(pageSettings);
        };
        BootstrapMap.prototype.initialisePageLayouts = function (pageSettings) {
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
                onFocus: function () {
                    if (pageSettings.aircraftPlotter)
                        pageSettings.aircraftPlotter.suspend(true);
                    if (pageSettings.mapJQ) {
                        pageSettings.mapJQ.hide();
                        if (pageSettings.mapButton) {
                            mapButtonParent = pageSettings.mapButton.parent();
                            pageSettings.mapButton.detach();
                            mapButtonContainer.append(pageSettings.mapButton);
                            aircraftListPlugin.prependElement(mapButtonContainer);
                        }
                    }
                },
                onBlur: function () {
                    if (pageSettings.mapJQ) {
                        if (pageSettings.mapButton) {
                            mapButtonContainer.detach();
                            pageSettings.mapButton.detach();
                            mapButtonParent.append(pageSettings.mapButton);
                        }
                        pageSettings.mapJQ.show();
                    }
                    if (pageSettings.aircraftPlotter)
                        pageSettings.aircraftPlotter.suspend(false);
                }
            }));
            this.endLayoutInitialisation(pageSettings);
            this.createLayoutMenuEntry(pageSettings, ['Layout2', 'Layout4', 'Layout6']);
        };
        BootstrapMap.prototype.initialisePageManager = function (pageSettings) {
            var _this = this;
            VRS.pageManager.initialise(pageSettings.pagesJQ);
            if (pageSettings.mapJQ) {
                VRS.pageManager.addPage(new VRS.Page({
                    name: VRS.MobilePageName.Map,
                    element: pageSettings.mapJQ,
                    visibleCallback: function (isVisible) {
                        if (pageSettings.aircraftPlotter) {
                            pageSettings.aircraftPlotter.suspend(!isVisible);
                        }
                        if (pageSettings.infoWindowPlugin) {
                            pageSettings.infoWindowPlugin.suspend(!isVisible);
                        }
                    },
                    afterVisibleCallback: function () {
                        if (pageSettings.mapPlugin) {
                            pageSettings.mapPlugin.refreshMap();
                        }
                        if (pageSettings.infoWindowPlugin) {
                            pageSettings.infoWindowPlugin.refreshDisplay();
                        }
                    }
                }));
                pageSettings.mapNextPageButton = $('<div/>')
                    .vrsMapNextPageButton(VRS.jQueryUIHelper.getMapNextPageButtonOptions({
                    nextPageName: VRS.MobilePageName.AircraftList,
                    aircraftListFetcher: pageSettings.aircraftListFetcher,
                    aircraftListFilter: pageSettings.aircraftListFilter
                }));
                if (pageSettings.mapPlugin) {
                    pageSettings.mapPlugin.addControl(pageSettings.mapNextPageButton, VRS.MapPosition.TopRight);
                }
                else if (pageSettings.mapJQ) {
                    pageSettings.mapJQ.children().first().append(pageSettings.mapNextPageButton);
                }
            }
            if (pageSettings.aircraftDetailJQ) {
                pageSettings.aircraftDetailPagePanelJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                    element: pageSettings.aircraftDetailJQ,
                    previousPageName: VRS.MobilePageName.AircraftList,
                    previousPageLabelKey: 'PageListShort',
                    titleLabelKey: 'TitleAircraftDetail',
                    nextPageName: VRS.MobilePageName.Map,
                    nextPageLabelKey: 'PageMapShort',
                    headerMenu: pageSettings.settingsMenu,
                    showFooterGap: true
                }));
                VRS.pageManager.addPage(new VRS.Page({
                    name: VRS.MobilePageName.AircraftDetail,
                    element: pageSettings.aircraftDetailPagePanelJQ,
                    visibleCallback: function (isVisible) {
                        var aircraftDetailPlugin = VRS.jQueryUIHelper.getAircraftDetailPlugin(pageSettings.aircraftDetailJQ);
                        aircraftDetailPlugin.suspend(!isVisible);
                    }
                }));
            }
            if (pageSettings.aircraftListJQ) {
                pageSettings.aircraftListPagePanelJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                    element: pageSettings.aircraftListJQ,
                    previousPageName: VRS.MobilePageName.Map,
                    previousPageLabelKey: 'PageMapShort',
                    titleLabelKey: 'TitleAircraftList',
                    nextPageName: VRS.MobilePageName.AircraftDetail,
                    nextPageLabelKey: 'AircraftDetailShort',
                    headerMenu: pageSettings.settingsMenu
                }));
                VRS.pageManager.addPage(new VRS.Page({
                    name: VRS.MobilePageName.AircraftList,
                    element: pageSettings.aircraftListPagePanelJQ,
                    visibleCallback: function (isVisible) {
                        var aircraftListPlugin = VRS.jQueryUIHelper.getAircraftListPlugin(pageSettings.aircraftListJQ);
                        aircraftListPlugin.suspend(!isVisible);
                    }
                }));
            }
            if (this._Settings.showOptionsInPage) {
                var optionsContainer = $('<div/>');
                var pausedStateWhenMadeVisible = false;
                pageSettings.optionsPagePanelJQ = $('<div/>')
                    .appendTo('body')
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                    element: optionsContainer,
                    previousPageName: VRS.MobilePageName.Map,
                    previousPageLabelKey: 'PageMapShort',
                    titleLabelKey: 'Options',
                    nextPageName: VRS.MobilePageName.AircraftList,
                    nextPageLabelKey: 'PageListShort'
                }));
                VRS.pageManager.addPage(new VRS.Page({
                    name: VRS.MobilePageName.Options,
                    element: pageSettings.optionsPagePanelJQ,
                    visibleCallback: function (isVisible) {
                        if (!isVisible) {
                            var plugin = VRS.jQueryUIHelper.getOptionFormPlugin(optionsContainer);
                            if (plugin) {
                                plugin.destroy();
                            }
                            if (pageSettings.aircraftListFetcher) {
                                pageSettings.aircraftListFetcher.setPaused(pausedStateWhenMadeVisible);
                            }
                        }
                        else {
                            if (pageSettings.aircraftListFetcher) {
                                pausedStateWhenMadeVisible = pageSettings.aircraftListFetcher.getPaused();
                                if (!pausedStateWhenMadeVisible) {
                                    pageSettings.aircraftListFetcher.setPaused(true);
                                }
                            }
                            _this.buildOptionPanelPages(pageSettings);
                            optionsContainer.vrsOptionForm(VRS.jQueryUIHelper.getOptionFormOptions({
                                pages: pageSettings.pages,
                                showInAccordion: true
                            }));
                        }
                    }
                }));
            }
            this.raisePageManagerInitialised(pageSettings);
        };
        return BootstrapMap;
    }(VRS.Bootstrap));
    VRS.BootstrapMap = BootstrapMap;
})(VRS || (VRS = {}));
//# sourceMappingURL=bootstrapMap.js.map