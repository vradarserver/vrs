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
    var BootstrapReport = (function (_super) {
        __extends(BootstrapReport, _super);
        function BootstrapReport(settings) {
            var _this = this;
            settings = $.extend({
                dispatcherName: 'VRS.BootstrapReport',
                suppressTitleUpdate: false,
                settingsPosition: VRS.MapPosition.TopLeft,
                settingsMenuAlignment: VRS.Alignment.Left
            }, settings);
            _this = _super.call(this, settings) || this;
            return _this;
        }
        BootstrapReport.prototype.initialise = function (userPageSettings) {
            var _this = this;
            var pageSettings = $.extend({
                showSettingsButton: true,
                showOptionsSetting: true,
                showLanguageSetting: true,
                showLayoutSetting: true,
                showLayersMenu: true,
                settingsMenu: null,
                splittersJQ: null,
                pagesJQ: null,
                mapJQ: null
            }, userPageSettings);
            this.doStartInitialise(pageSettings, function () {
                VRS.currentLocation.setShowCurrentLocationOnMap(false);
                if (!_this._Settings.suppressTitleUpdate) {
                    document.title = VRS.$$.VirtualRadar;
                }
                pageSettings.report = new VRS.Report({
                    name: pageSettings.reportName,
                    autoSaveState: true,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                });
                pageSettings.report.loadAndApplyState();
                pageSettings.report.getCriteria().loadAndApplyState();
                pageSettings.report.populateFromQueryString();
                _this.raiseReportCreated(pageSettings);
                if (pageSettings.mapJQ) {
                    _this.createMapSettingsControl(pageSettings);
                }
                pageSettings.plotterOptions = new VRS.AircraftPlotterOptions({
                    trailDisplay: VRS.TrailDisplay.AllAircraft,
                    trailType: VRS.TrailType.Full
                });
                if (!pageSettings.mapJQ) {
                    _this.mapCreated(pageSettings);
                }
                else {
                    var mapOptions = {
                        plotterOptions: pageSettings.plotterOptions,
                        report: pageSettings.report,
                        unitDisplayPreferences: pageSettings.unitDisplayPreferences,
                        loadedCallback: function () {
                            pageSettings.mapPlugin = pageSettings.mapJQ ? VRS.jQueryUIHelper.getReportMapPlugin(pageSettings.mapJQ) : null;
                            if (pageSettings.mapPlugin && !pageSettings.mapPlugin.isOpen()) {
                                pageSettings.mapPlugin = null;
                            }
                            _this.mapCreated(pageSettings);
                        }
                    };
                    if (pageSettings.menuJQ && pageSettings.showSettingsButton) {
                        mapOptions.mapControls = [
                            { control: pageSettings.menuJQ, position: _this._Settings.settingsPosition }
                        ];
                    }
                    pageSettings.mapJQ.vrsReportMap(VRS.jQueryUIHelper.getReportMapOptions(mapOptions));
                }
            });
        };
        BootstrapReport.prototype.mapCreated = function (pageSettings) {
            if (VRS.mapLayerManager) {
                VRS.mapLayerManager.registerMap(pageSettings.mapPlugin.getMapWrapper());
            }
            if (pageSettings.detailJQ) {
                pageSettings.detailJQ.vrsReportDetail(VRS.jQueryUIHelper.getReportDetailOptions({
                    report: pageSettings.report,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences,
                    plotterOptions: pageSettings.plotterOptions
                }));
                pageSettings.detailPlugin = VRS.jQueryUIHelper.getReportDetailPlugin(pageSettings.detailJQ);
            }
            if (pageSettings.listJQ) {
                pageSettings.listJQ.vrsReportList(VRS.jQueryUIHelper.getReportListOptions({
                    report: pageSettings.report,
                    unitDisplayPreferences: pageSettings.unitDisplayPreferences
                }));
                pageSettings.listPlugin = VRS.jQueryUIHelper.getReportListPlugin(pageSettings.listJQ);
            }
            if (pageSettings.splittersJQ) {
                this.initialisePageLayouts(pageSettings);
            }
            if (pageSettings.pagesJQ) {
                this.initialisePageManager(pageSettings);
            }
            this.buildSettingsMenu(pageSettings, pageSettings.settingsMenu.getTopLevelMenuItems());
            this.doEndInitialise(pageSettings);
            pageSettings.report.fetchPage();
        };
        BootstrapReport.prototype.buildSettingsMenu = function (pageSettings, menuItems) {
            if (pageSettings.showOptionsSetting) {
                menuItems.push(this.createCriteriaMenuEntry(pageSettings));
            }
            if (pageSettings.showLanguageSetting) {
                menuItems.push(this.createLocaleMenuEntry(pageSettings));
            }
            if (pageSettings.showLayoutSetting && pageSettings.layoutMenuItem) {
                menuItems.push(null);
                menuItems.push(pageSettings.layoutMenuItem);
            }
            var mapWrapper = pageSettings.mapPlugin.getMapWrapper();
            var mapLayersMenu = this.createLayersMenuEntry(pageSettings, mapWrapper, false);
            if (mapLayersMenu) {
                menuItems.push(null);
                menuItems.push(mapLayersMenu);
            }
            menuItems.push(null);
        };
        BootstrapReport.prototype.createCriteriaMenuEntry = function (pageSettings) {
            var _this = this;
            var menuEntry = new VRS.MenuItem({
                name: 'options',
                labelKey: 'Criteria',
                vrsIcon: 'equalizer',
                clickCallback: function () {
                    if (_this._Settings.showOptionsInPage) {
                        VRS.pageManager.show(VRS.MobilePageName.Options);
                    }
                    else {
                        _this.buildCriteriaOptionPanelPages(pageSettings);
                        $('<div/>')
                            .appendTo($('body'))
                            .vrsOptionDialog(VRS.jQueryUIHelper.getOptionDialogOptions({
                            pages: pageSettings.pages,
                            autoRemove: true
                        }));
                    }
                }
            });
            pageSettings.report.hookFailedNoCriteria(function () {
                menuEntry.clickCallback(menuEntry);
            }, this);
            return menuEntry;
        };
        BootstrapReport.prototype.initialisePageLayouts = function (pageSettings) {
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
            this.endLayoutInitialisation(pageSettings);
            this.createLayoutMenuEntry(pageSettings, ['Layout2', 'Layout4']);
        };
        BootstrapReport.prototype.initialisePageManager = function (pageSettings) {
            var _this = this;
            VRS.pageManager.initialise(pageSettings.pagesJQ);
            if (pageSettings.listJQ) {
                pageSettings.listJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                    element: pageSettings.listJQ,
                    previousPageName: VRS.MobilePageName.AircraftDetail,
                    previousPageLabelKey: 'FlightDetailShort',
                    titleLabelKey: 'TitleFlightsList',
                    nextPageName: VRS.MobilePageName.AircraftDetail,
                    nextPageLabelKey: 'FlightDetailShort',
                    headerMenu: pageSettings.settingsMenu
                }));
                VRS.pageManager.addPage(new VRS.Page({
                    name: VRS.MobilePageName.AircraftList,
                    element: pageSettings.listJQ
                }));
            }
            if (pageSettings.detailJQ) {
                pageSettings.detailJQ = $('<div/>')
                    .appendTo($('body'))
                    .vrsPagePanel(VRS.jQueryUIHelper.getPagePanelOptions({
                    element: pageSettings.detailJQ,
                    previousPageName: VRS.MobilePageName.AircraftList,
                    previousPageLabelKey: 'FlightsListShort',
                    titleLabelKey: 'TitleFlightDetail',
                    nextPageName: VRS.MobilePageName.AircraftList,
                    nextPageLabelKey: 'FlightsListShort',
                    headerMenu: pageSettings.settingsMenu
                }));
                VRS.pageManager.addPage(new VRS.Page({
                    name: VRS.MobilePageName.AircraftDetail,
                    element: pageSettings.detailJQ,
                    visibleCallback: function (visible) {
                        if (!visible) {
                            pageSettings.detailPlugin.suspend(true);
                        }
                    },
                    afterVisibleCallback: function () {
                        pageSettings.detailPlugin.suspend(false);
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
                    previousPageName: VRS.MobilePageName.AircraftList,
                    previousPageLabelKey: 'FlightsListShort',
                    titleLabelKey: 'Criteria',
                    nextPageName: VRS.MobilePageName.AircraftDetail,
                    nextPageLabelKey: 'FlightDetailShort'
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
                        }
                        else {
                            _this.buildCriteriaOptionPanelPages(pageSettings);
                            optionsContainer.vrsOptionForm(VRS.jQueryUIHelper.getOptionFormOptions({
                                pages: pageSettings.pages,
                                showInAccordion: true
                            }));
                        }
                    }
                }));
            }
            pageSettings.report.hookRowsFetched(function () { VRS.pageManager.show(VRS.MobilePageName.AircraftList); }, this);
            this.raisePageManagerInitialised(pageSettings);
        };
        BootstrapReport.prototype.buildCriteriaOptionPanelPages = function (pageSettings) {
            pageSettings.pages = [];
            pageSettings.pages.push(new VRS.OptionPage({
                name: 'criteriaPage',
                titleKey: 'Criteria',
                displayOrder: 100,
                panes: [pageSettings.report.createOptionPane(100)]
            }));
            if (pageSettings.detailPlugin) {
                pageSettings.pages.push(new VRS.OptionPage({
                    name: 'detailPanelPage',
                    titleKey: 'DetailPanel',
                    displayOrder: 200,
                    panes: [pageSettings.detailPlugin.createOptionPane(100)]
                }));
            }
            if (pageSettings.listPlugin) {
                pageSettings.pages.push(new VRS.OptionPage({
                    name: 'listPage',
                    titleKey: 'PageList',
                    displayOrder: 300,
                    panes: [pageSettings.listPlugin.createOptionPane(100)]
                }));
            }
            this.raiseOptionsPagesInitialised(pageSettings);
        };
        return BootstrapReport;
    }(VRS.Bootstrap));
    VRS.BootstrapReport = BootstrapReport;
})(VRS || (VRS = {}));
//# sourceMappingURL=bootstrapReport.js.map