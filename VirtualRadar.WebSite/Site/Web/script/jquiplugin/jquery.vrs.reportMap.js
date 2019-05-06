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
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportMapClass = VRS.globalOptions.reportMapClass || 'reportMap';
    VRS.globalOptions.reportMapScrollToAircraft = VRS.globalOptions.reportMapScrollToAircraft !== undefined ? VRS.globalOptions.reportMapScrollToAircraft : true;
    VRS.globalOptions.reportMapShowPath = VRS.globalOptions.reportMapShowPath !== undefined ? VRS.globalOptions.reportMapShowPath : true;
    VRS.globalOptions.reportMapStartSelected = VRS.globalOptions.reportMapStartSelected !== undefined ? VRS.globalOptions.reportMapStartSelected : false;
    var ReportMapPlugin_State = (function () {
        function ReportMapPlugin_State() {
            this.mapContainer = null;
            this.mapPlugin = null;
            this.aircraftPlotter = null;
            this.aircraftList = new VRS.AircraftCollection();
            this.nextAircraftId = 1;
            this.firstPositionAircraftId = 0;
            this.lastPositionAircraftId = 0;
            this.selectedFlight = null;
            this.selectedAircraft = null;
            this.selectedFlightChangedHookResult = null;
            this.localeChangedHookResult = null;
        }
        return ReportMapPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getReportMapPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsReportMap');
    };
    VRS.jQueryUIHelper.getReportMapOptions = function (overrides) {
        return $.extend({
            name: 'default',
            report: null,
            plotterOptions: null,
            elementClasses: '',
            unitDisplayPreferences: undefined,
            mapOptionOverrides: {},
            mapControls: [],
            scrollToAircraft: VRS.globalOptions.reportMapScrollToAircraft,
            showPath: VRS.globalOptions.reportMapShowPath,
            startSelected: VRS.globalOptions.reportMapStartSelected,
            loadedCallback: $.noop
        }, overrides);
    };
    var ReportMapPlugin = (function (_super) {
        __extends(ReportMapPlugin, _super);
        function ReportMapPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getReportMapOptions();
            return _this;
        }
        ReportMapPlugin.prototype._getState = function () {
            var result = this.element.data('reportMapState');
            if (result === undefined) {
                result = new ReportMapPlugin_State();
                this.element.data('reportMapState', result);
            }
            return result;
        };
        ReportMapPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            this.element.addClass(VRS.globalOptions.reportMapClass);
            if (options.elementClasses) {
                this.element.addClass(options.elementClasses);
            }
            this._createMap(state);
        };
        ReportMapPlugin.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            if (state.selectedFlightChangedHookResult && options.report) {
                options.report.unhook(state.selectedFlightChangedHookResult);
            }
            state.selectedFlightChangedHookResult = null;
            if (state.localeChangedHookResult && VRS.globalisation) {
                VRS.globalisation.unhook(state.localeChangedHookResult);
            }
            state.localeChangedHookResult = null;
            if (state.aircraftPlotter) {
                state.aircraftPlotter.dispose();
            }
            state.aircraftPlotter = null;
            if (state.mapPlugin) {
                state.mapPlugin.destroy();
            }
            if (state.mapContainer) {
                state.mapContainer.remove();
            }
            state.mapPlugin = state.mapContainer = null;
            this.element.removeClass(VRS.globalOptions.reportMapClass);
        };
        ReportMapPlugin.prototype.isOpen = function () {
            var state = this._getState();
            return state.mapPlugin && state.mapPlugin.isOpen();
        };
        ReportMapPlugin.prototype.getMapWrapper = function () {
            var state = this._getState();
            return state.mapPlugin;
        };
        ReportMapPlugin.prototype._createMap = function (state) {
            var options = this.options;
            if (options.report) {
                state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
            }
            var mapOptions = {
                name: 'report-' + options.name,
                scrollwheel: true,
                draggable: !VRS.globalOptions.isMobile,
                useServerDefaults: true,
                loadMarkerWithLabel: true,
                loadMarkerCluster: false,
                autoSaveState: true,
                useStateOnOpen: true,
                mapControls: options.mapControls,
                controlStyle: VRS.MapControlStyle.DropdownMenu
            };
            $.extend(mapOptions, options.mapOptionOverrides);
            mapOptions.afterOpen = $.proxy(this._mapCreated, this);
            state.mapContainer = $('<div/>')
                .appendTo(this.element);
            state.mapContainer.vrsMap(VRS.jQueryUIHelper.getMapOptions(mapOptions));
        };
        ReportMapPlugin.prototype._mapCreated = function () {
            var _this = this;
            var state = this._getState();
            var options = this.options;
            if (state.mapContainer) {
                state.mapPlugin = VRS.jQueryUIHelper.getMapPlugin(state.mapContainer);
                if (!state.mapPlugin.isOpen()) {
                    if (options.mapControls) {
                        $.each(options.mapControls, function (idx, control) {
                            state.mapContainer.children().first().prepend(control.control);
                        });
                    }
                }
                else {
                    state.aircraftPlotter = new VRS.AircraftPlotter({
                        plotterOptions: options.plotterOptions,
                        map: state.mapContainer,
                        unitDisplayPreferences: options.unitDisplayPreferences,
                        getAircraft: function () { return _this._getAircraft(); },
                        getSelectedAircraft: function () { return _this._getSelectedAircraft(); },
                        getCustomPinTexts: function (aircraft) {
                            return [aircraft.id === state.firstPositionAircraftId ? VRS.$$.Start : VRS.$$.End];
                        }
                    });
                }
                VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            }
            if (options.loadedCallback) {
                options.loadedCallback();
            }
        };
        ReportMapPlugin.prototype._buildFakeVrsAircraft = function (state) {
            var options = this.options;
            state.aircraftList = new VRS.AircraftCollection();
            state.selectedAircraft = null;
            var flight = state.selectedFlight;
            if (flight && ((flight.fLat && flight.fLng) || (flight.lLat && flight.lLng))) {
                var first = null, last = null;
                if (flight.fLat && flight.fLng) {
                    first = VRS.Report.convertFlightToVrsAircraft(flight, true);
                    first.id = state.firstPositionAircraftId = state.nextAircraftId++;
                    state.aircraftList[first.id] = first;
                }
                if (flight.lLat && flight.lLng) {
                    last = VRS.Report.convertFlightToVrsAircraft(flight, false);
                    last.id = state.lastPositionAircraftId = state.nextAircraftId++;
                    state.aircraftList[last.id] = last;
                }
                state.selectedAircraft = options.startSelected ? first : last;
                if (options.showPath && first && last) {
                    last.fullTrail.arr.push(new VRS.FullTrailValue(first.latitude.val, first.longitude.val, first.heading.val, first.altitude.val, first.speed.val));
                    last.fullTrail.arr.push(new VRS.FullTrailValue(last.latitude.val, last.longitude.val, last.heading.val, last.altitude.val, last.speed.val));
                }
            }
        };
        ReportMapPlugin.prototype._getAircraft = function () {
            var state = this._getState();
            return state.aircraftList;
        };
        ReportMapPlugin.prototype._getSelectedAircraft = function () {
            var state = this._getState();
            return state.selectedAircraft;
        };
        ReportMapPlugin.prototype._localeChanged = function () {
            var state = this._getState();
            if (state.aircraftPlotter) {
                state.aircraftPlotter.plot(true, true);
            }
        };
        ReportMapPlugin.prototype._selectedFlightChanged = function () {
            this.showFlight(this.options.report.getSelectedFlight());
        };
        ReportMapPlugin.prototype.showFlight = function (flight) {
            var state = this._getState();
            state.selectedFlight = flight;
            var options = this.options;
            this._buildFakeVrsAircraft(state);
            var applyWhenReady = function () {
                var map = state.aircraftPlotter ? state.aircraftPlotter.getMap() : null;
                if (!map || !map.isReady()) {
                    setTimeout(applyWhenReady, 100);
                }
                else {
                    var fromAircraft = state.aircraftList.findAircraftById(state.firstPositionAircraftId);
                    var toAircraft = fromAircraft ? state.aircraftList.findAircraftById(state.lastPositionAircraftId) : null;
                    if (options.scrollToAircraft && (fromAircraft || toAircraft)) {
                        if (fromAircraft.hasPosition() && toAircraft.hasPosition() && (fromAircraft.latitude.val !== toAircraft.latitude.val || fromAircraft.longitude.val !== toAircraft.longitude.val)) {
                            var bounds = VRS.greatCircle.arrangeTwoPointsIntoBounds(!fromAircraft ? null : { lat: fromAircraft.latitude.val, lng: fromAircraft.longitude.val }, !toAircraft ? null : { lat: toAircraft.latitude.val, lng: toAircraft.longitude.val });
                            if (bounds) {
                                map.fitBounds(bounds);
                            }
                        }
                        else if (fromAircraft.hasPosition()) {
                            map.panTo(fromAircraft.getPosition());
                        }
                        else if (toAircraft.hasPosition()) {
                            map.panTo(toAircraft.getPosition());
                        }
                    }
                    state.aircraftPlotter.plot(true, true);
                }
            };
            applyWhenReady();
        };
        return ReportMapPlugin;
    }(JQueryUICustomWidget));
    VRS.ReportMapPlugin = ReportMapPlugin;
    $.widget('vrs.vrsReportMap', new ReportMapPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.reportMap.js.map