var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftPositionMapClass = VRS.globalOptions.aircraftPositionMapClass || 'aircraftPosnMap';
    var AircraftPositionMapPlugin_State = (function () {
        function AircraftPositionMapPlugin_State() {
            this.mapContainer = null;
            this.mapPlugin = null;
            this.aircraftCollection = new VRS.AircraftCollection();
            this.selectedAircraft = null;
            this.aircraftPlotter = null;
            this.mirrorMapPlugin = null;
            this.firstRender = true;
            this.mapTypeChangedHookResult = null;
            this.mirrorMapTypeChangedHookResult = null;
        }
        return AircraftPositionMapPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftPositionMapPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsAircraftPositonMap');
    };
    VRS.jQueryUIHelper.getAircraftPositionMapOptions = function (overrides) {
        return $.extend({
            plotterOptions: null,
            mirrorMapJQ: null,
            stateName: null,
            mapOptionOverrides: {},
            unitDisplayPreferences: undefined,
            autoHideNoPosition: true,
            reflectMapTypeBackToMirror: true
        }, overrides);
    };
    var AircraftPositionMapPlugin = (function (_super) {
        __extends(AircraftPositionMapPlugin, _super);
        function AircraftPositionMapPlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getAircraftPositionMapOptions();
        }
        AircraftPositionMapPlugin.prototype._getState = function () {
            var result = this.element.data('aircraftPositionMapState');
            if (result === undefined) {
                result = new AircraftPositionMapPlugin_State();
                this.element.data('aircraftPositionMapState', result);
            }
            return result;
        };
        AircraftPositionMapPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            this.element.addClass(VRS.globalOptions.aircraftPositionMapClass);
            if (options.mirrorMapJQ) {
                state.mirrorMapPlugin = VRS.jQueryUIHelper.getMapPlugin(options.mirrorMapJQ);
            }
            this._createMap(state);
        };
        AircraftPositionMapPlugin.prototype._createMap = function (state) {
            var options = this.options;
            var mapOptions = {};
            if (state.mirrorMapPlugin && state.mirrorMapPlugin.isOpen()) {
                mapOptions.zoom = state.mirrorMapPlugin.getZoom();
                mapOptions.center = state.mirrorMapPlugin.getCenter();
                mapOptions.mapTypeId = state.mirrorMapPlugin.getMapType();
                mapOptions.streetViewControl = state.mirrorMapPlugin.getStreetView();
                mapOptions.scrollwheel = state.mirrorMapPlugin.getScrollWheel();
                mapOptions.draggable = state.mirrorMapPlugin.getDraggable();
                mapOptions.controlStyle = VRS.MapControlStyle.DropdownMenu;
                mapOptions.useServerDefaults = false;
            }
            $.extend(mapOptions, options.mapOptionOverrides);
            if (!options.stateName) {
                mapOptions.autoSaveState = false;
            }
            else {
                mapOptions.autoSaveState = true;
                mapOptions.name = options.stateName;
                mapOptions.useStateOnOpen = true;
            }
            mapOptions.afterOpen = $.proxy(this._mapCreated, this);
            state.mapContainer = $('<div/>')
                .appendTo(this.element);
            state.mapContainer.vrsMap(VRS.jQueryUIHelper.getMapOptions(mapOptions));
        };
        AircraftPositionMapPlugin.prototype._mapCreated = function () {
            var state = this._getState();
            var options = this.options;
            if (state.mapContainer) {
                state.mapPlugin = VRS.jQueryUIHelper.getMapPlugin(state.mapContainer);
                if (state.mapPlugin && state.mapPlugin.isOpen()) {
                    state.aircraftPlotter = new VRS.AircraftPlotter({
                        plotterOptions: options.plotterOptions,
                        map: state.mapContainer,
                        unitDisplayPreferences: options.unitDisplayPreferences,
                        getAircraft: $.proxy(this._getAircraft, this),
                        getSelectedAircraft: $.proxy(this._getSelectedAircraft, this)
                    });
                }
                state.mapTypeChangedHookResult = state.mapPlugin.hookMapTypeChanged(this._mapTypeChanged, this);
                if (state.mirrorMapPlugin) {
                    state.mirrorMapTypeChangedHookResult = state.mirrorMapPlugin.hookMapTypeChanged(this._mirrorMapTypeChanged, this);
                }
            }
        };
        AircraftPositionMapPlugin.prototype._destroy = function () {
            var state = this._getState();
            if (state.mapTypeChangedHookResult)
                state.mapPlugin.unhook(state.mapTypeChangedHookResult);
            state.mapTypeChangedHookResult = null;
            if (state.mirrorMapTypeChangedHookResult)
                state.mirrorMapPlugin.unhook(state.mirrorMapTypeChangedHookResult);
            state.mirrorMapTypeChangedHookResult = null;
            state.mirrorMapPlugin = null;
            if (state.mapPlugin) {
                state.mapPlugin.destroy();
                state.mapPlugin = null;
            }
            state.mapContainer = null;
            if (state.aircraftPlotter)
                state.aircraftPlotter.dispose();
            state.aircraftPlotter = null;
            state.aircraftCollection = null;
            this.element.removeClass(VRS.globalOptions.aircraftPositionMapClass);
            this.element.empty();
        };
        AircraftPositionMapPlugin.prototype.renderAircraft = function (aircraft, showAsSelected) {
            var state = this._getState();
            var options = this.options;
            if (state.aircraftPlotter) {
                if (aircraft && !aircraft.hasPosition())
                    aircraft = null;
                var existingAircraft = state.aircraftCollection.toList();
                if (!aircraft) {
                    if (existingAircraft.length > 0)
                        state.aircraftCollection = new VRS.AircraftCollection();
                }
                else {
                    if (existingAircraft.length !== 1 || existingAircraft[aircraft.id] !== aircraft) {
                        state.aircraftCollection = new VRS.AircraftCollection();
                        state.aircraftCollection[aircraft.id] = aircraft;
                    }
                }
                state.selectedAircraft = showAsSelected ? aircraft : null;
                if (!aircraft) {
                    if (options.autoHideNoPosition) {
                        $(this.element, ':visible').hide();
                    }
                    else {
                        state.aircraftPlotter.plot();
                    }
                }
                else {
                    var refreshMap = state.firstRender || (options.autoHideNoPosition && this.element.is(':hidden'));
                    if (refreshMap) {
                        this.element.show();
                        state.mapPlugin.refreshMap();
                    }
                    state.mapPlugin.panTo(aircraft.getPosition());
                    state.aircraftPlotter.plot();
                    state.firstRender = false;
                }
            }
        };
        AircraftPositionMapPlugin.prototype.suspend = function (onOff) {
            var state = this._getState();
            if (state.aircraftPlotter) {
                state.aircraftPlotter.suspend(onOff);
            }
        };
        AircraftPositionMapPlugin.prototype._mapTypeChanged = function () {
            var state = this._getState();
            var options = this.options;
            if (state.mirrorMapPlugin && state.mapPlugin && options.reflectMapTypeBackToMirror) {
                state.mirrorMapPlugin.setMapType(state.mapPlugin.getMapType());
            }
        };
        AircraftPositionMapPlugin.prototype._mirrorMapTypeChanged = function () {
            var state = this._getState();
            if (state.mirrorMapPlugin && state.mapPlugin) {
                state.mapPlugin.setMapType(state.mirrorMapPlugin.getMapType());
            }
        };
        AircraftPositionMapPlugin.prototype._getAircraft = function () {
            var state = this._getState();
            return state.aircraftCollection;
        };
        AircraftPositionMapPlugin.prototype._getSelectedAircraft = function () {
            var state = this._getState();
            return state.selectedAircraft;
        };
        return AircraftPositionMapPlugin;
    }(JQueryUICustomWidget));
    VRS.AircraftPositionMapPlugin = AircraftPositionMapPlugin;
    $.widget('vrs.vrsAircraftPositonMap', new AircraftPositionMapPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.aircraftPositionMap.js.map