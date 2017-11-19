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
    VRS.globalOptions.aircraftInfoWindowClass = VRS.globalOptions.aircraftInfoWindowClass || 'vrsAircraftInfoWindow';
    VRS.globalOptions.aircraftInfoWindowEnabled = VRS.globalOptions.aircraftInfoWindowEnabled !== undefined ? VRS.globalOptions.aircraftInfoWindowEnabled : true;
    VRS.globalOptions.aircraftInfoWindowItems = VRS.globalOptions.aircraftInfoWindowItems || [
        VRS.RenderProperty.Icao,
        VRS.RenderProperty.Registration,
        VRS.RenderProperty.ModelIcao,
        VRS.RenderProperty.Operator,
        VRS.RenderProperty.Model,
        VRS.RenderProperty.Callsign,
        VRS.RenderProperty.RouteShort,
        VRS.RenderProperty.Speed,
        VRS.RenderProperty.Altitude
    ];
    VRS.globalOptions.aircraftInfoWindowShowUnits = VRS.globalOptions.aircraftInfoWindowShowUnits !== undefined ? VRS.globalOptions.aircraftInfoWindowShowUnits : true;
    VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns = VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns : true;
    VRS.globalOptions.aircraftInfoWindowDistinguishOnGround = VRS.globalOptions.aircraftInfoWindowDistinguishOnGround !== undefined ? VRS.globalOptions.aircraftInfoWindowDistinguishOnGround : true;
    VRS.globalOptions.aircraftInfoWindowAllowConfiguration = VRS.globalOptions.aircraftInfoWindowAllowConfiguration !== undefined ? VRS.globalOptions.aircraftInfoWindowAllowConfiguration : true;
    VRS.globalOptions.aircraftInfoWindowEnablePanning = VRS.globalOptions.aircraftInfoWindowEnablePanning !== undefined ? VRS.globalOptions.aircraftInfoWindowEnablePanning : true;
    var AircraftInfoWindowPlugin_State = (function () {
        function AircraftInfoWindowPlugin_State() {
            this.containerElement = null;
            this.itemsContainerElement = null;
            this.itemElements = {};
            this.mapInfoWindow = null;
            this.aircraft = null;
            this.anchoredAircraft = null;
            this.suspended = false;
            this.closedByUser = false;
            this.jumpToAircraftDetailLinkRenderer = null;
            this.linksElement = null;
            this.linksPlugin = null;
            this.aircraftListUpdatedHookResult = null;
            this.infoWindowClosedByUserHookResult = null;
            this.markerClickedHookResult = null;
            this.selectedAircraftChangedHook = null;
            this.unitChangedHookResult = null;
            this.localeChangedHookResult = null;
        }
        return AircraftInfoWindowPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftInfoWindowPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsAircraftInfoWindow');
    };
    VRS.jQueryUIHelper.getAircraftInfoWindowOptions = function (overrides) {
        return $.extend({
            name: 'default',
            aircraftList: null,
            aircraftPlotter: null,
            unitDisplayPreferences: null,
            enabled: VRS.globalOptions.aircraftInfoWindowEnabled,
            useStateOnOpen: true,
            items: VRS.globalOptions.aircraftInfoWindowItems,
            showUnits: VRS.globalOptions.aircraftInfoWindowShowUnits,
            flagUncertainCallsigns: VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns,
            distinguishOnGround: VRS.globalOptions.aircraftInfoWindowDistinguishOnGround,
            enablePanning: VRS.globalOptions.aircraftInfoWindowEnablePanning
        }, overrides);
    };
    var AircraftInfoWindowPlugin = (function (_super) {
        __extends(AircraftInfoWindowPlugin, _super);
        function AircraftInfoWindowPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getAircraftInfoWindowOptions();
            return _this;
        }
        AircraftInfoWindowPlugin.prototype._getState = function () {
            var result = this.element.data('aircraftInfoWindowState');
            if (result === undefined) {
                result = new AircraftInfoWindowPlugin_State();
                this.element.data('aircraftInfoWindowState', result);
            }
            return result;
        };
        AircraftInfoWindowPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            var map = options.aircraftPlotter.getMap();
            if (options.useStateOnOpen) {
                this.loadAndApplyState();
            }
            this.element.addClass(VRS.globalOptions.aircraftInfoWindowClass);
            state.containerElement = $('<div/>')
                .appendTo(this.element);
            state.itemsContainerElement = $('<ul/>')
                .appendTo(state.containerElement);
            this._buildItems(state);
            state.jumpToAircraftDetailLinkRenderer = new VRS.JumpToAircraftDetailPageRenderHandler();
            state.linksElement = $('<div/>')
                .addClass('links')
                .vrsAircraftLinks(VRS.jQueryUIHelper.getAircraftLinksOptions({ linkSites: [state.jumpToAircraftDetailLinkRenderer] }))
                .appendTo(state.containerElement);
            state.linksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(state.linksElement);
            state.mapInfoWindow = map.addInfoWindow(map.getUnusedInfoWindowId(), {
                content: this.element[0],
                disableAutoPan: !options.enablePanning
            });
            state.infoWindowClosedByUserHookResult = map.hookInfoWindowClosedByUser(this._infoWindowClosedByUser, this);
            state.aircraftListUpdatedHookResult = options.aircraftList.hookUpdated(this._aircraftListUpdated, this);
            state.selectedAircraftChangedHook = options.aircraftList.hookSelectedAircraftChanged(this._selectedAircraftChanged, this);
            state.markerClickedHookResult = map.hookMarkerClicked(this._markerClicked, this);
            state.unitChangedHookResult = options.unitDisplayPreferences.hookUnitChanged(this._displayUnitChanged, this);
            state.localeChangedHookResult = VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            this.showForAircraft(options.aircraftList.getSelectedAircraft());
        };
        AircraftInfoWindowPlugin.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            if (state.aircraftListUpdatedHookResult)
                options.aircraftList.unhook(state.aircraftListUpdatedHookResult);
            state.aircraftListUpdatedHookResult = null;
            if (state.selectedAircraftChangedHook)
                options.aircraftList.unhook(state.selectedAircraftChangedHook);
            state.selectedAircraftChangedHook = null;
            if (state.infoWindowClosedByUserHookResult)
                options.aircraftPlotter.getMap().unhook(state.infoWindowClosedByUserHookResult);
            state.infoWindowClosedByUserHookResult = null;
            if (state.markerClickedHookResult)
                options.aircraftPlotter.getMap().unhook(state.markerClickedHookResult);
            state.markerClickedHookResult = null;
            if (state.unitChangedHookResult)
                options.unitDisplayPreferences.unhook(state.unitChangedHookResult);
            state.unitChangedHookResult = null;
            if (state.localeChangedHookResult)
                VRS.globalisation.unhook(state.localeChangedHookResult);
            state.localeChangedHookResult = null;
            if (state.linksElement) {
                state.linksPlugin.destroy();
                state.linksElement.remove();
            }
            state.linksElement = null;
            state.jumpToAircraftDetailLinkRenderer = null;
            this._destroyItems(state);
            if (state.itemsContainerElement)
                state.itemsContainerElement.remove();
            state.itemsContainerElement = null;
            if (state.containerElement)
                state.containerElement.remove();
            state.containerElement = null;
            this.element.removeClass(VRS.globalOptions.aircraftInfoWindowClass);
            if (state.mapInfoWindow) {
                options.aircraftPlotter.getMap().destroyInfoWindow(state.mapInfoWindow);
            }
            state.mapInfoWindow = null;
            state.aircraft = null;
        };
        AircraftInfoWindowPlugin.prototype._buildItems = function (state) {
            this._destroyItems(state);
            var options = this.options;
            var length = options.items.length;
            for (var i = 0; i < length; ++i) {
                var renderProperty = options.items[i];
                var handler = VRS.renderPropertyHandlers[renderProperty];
                if (!handler)
                    throw 'Cannot find the render property handler for ' + renderProperty;
                var listItem = $('<li/>')
                    .appendTo(state.itemsContainerElement);
                var label = $('<label/>')
                    .text(handler.suppressLabelCallback(VRS.RenderSurface.InfoWindow) ? '' : VRS.globalisation.getText(handler.labelKey) + ':')
                    .appendTo(listItem);
                state.itemElements[renderProperty] = $('<p/>')
                    .addClass('value')
                    .appendTo(listItem);
            }
        };
        AircraftInfoWindowPlugin.prototype._destroyItems = function (state) {
            state.itemsContainerElement.empty();
            state.itemElements = {};
        };
        AircraftInfoWindowPlugin.prototype.saveState = function () {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        };
        AircraftInfoWindowPlugin.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.items = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.items, [VRS.RenderSurface.InfoWindow]);
            return result;
        };
        AircraftInfoWindowPlugin.prototype.applyState = function (settings) {
            var options = this.options;
            options.enabled = settings.enabled;
            options.items = settings.items.slice();
            options.showUnits = settings.showUnits;
            var state = this._getState();
            if (state.containerElement) {
                this._buildItems(state);
                this.refreshDisplay();
            }
        };
        AircraftInfoWindowPlugin.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        AircraftInfoWindowPlugin.prototype._persistenceKey = function () {
            return 'vrsAircraftInfoWindow-' + this.options.name;
        };
        AircraftInfoWindowPlugin.prototype._createSettings = function () {
            var options = this.options;
            return {
                enabled: options.enabled,
                items: options.items,
                showUnits: options.showUnits
            };
        };
        AircraftInfoWindowPlugin.prototype.createOptionPane = function (displayOrder) {
            var _this = this;
            var result = new VRS.OptionPane({
                name: 'infoWindow',
                titleKey: 'PaneInfoWindow',
                displayOrder: displayOrder
            });
            var options = this.options;
            var saveAndApplyState = function () {
                _this.saveState();
                var settings = _this._createSettings();
                _this.applyState(settings);
            };
            result.addField(new VRS.OptionFieldCheckBox({
                name: 'enable',
                labelKey: 'EnableInfoWindow',
                getValue: function () { return options.enabled; },
                setValue: function (value) { return options.enabled = value; },
                saveState: saveAndApplyState
            }));
            if (VRS.globalOptions.aircraftInfoWindowAllowConfiguration) {
                result.addField(new VRS.OptionFieldCheckBox({
                    name: 'showUnits',
                    labelKey: 'ShowUnits',
                    getValue: function () { return options.showUnits; },
                    setValue: function (value) { return options.showUnits = value; },
                    saveState: saveAndApplyState
                }));
                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane: result,
                    surface: VRS.RenderSurface.InfoWindow,
                    fieldLabel: 'Columns',
                    getList: function () { return options.items; },
                    setList: function (value) { return options.items = value; },
                    saveState: saveAndApplyState
                });
            }
            return result;
        };
        AircraftInfoWindowPlugin.prototype.suspend = function (onOff) {
            var state = this._getState();
            if (state.suspended !== onOff) {
                state.suspended = onOff;
            }
        };
        AircraftInfoWindowPlugin.prototype.showForAircraft = function (aircraft) {
            var state = this._getState();
            state.aircraft = aircraft;
            this._displayDetails(state, true);
        };
        AircraftInfoWindowPlugin.prototype.refreshDisplay = function () {
            var state = this._getState();
            this._displayDetails(state, true);
        };
        AircraftInfoWindowPlugin.prototype._displayDetails = function (state, forceRefresh) {
            var options = this.options;
            if (state.suspended || state.closedByUser)
                return;
            if (forceRefresh === undefined)
                forceRefresh = false;
            var aircraft = state.aircraft;
            var map = options.aircraftPlotter.getMap();
            var mapMarker = options.aircraftPlotter.getMapMarkerForAircraft(aircraft);
            var mapInfoWindow = state.mapInfoWindow;
            if (state.anchoredAircraft !== aircraft)
                forceRefresh = true;
            var length = options.items.length;
            if (options.enabled) {
                for (var i = 0; i < length; ++i) {
                    var renderProperty = options.items[i];
                    var handler = VRS.renderPropertyHandlers[renderProperty];
                    if (!handler)
                        throw 'Cannot find the handler for ' + renderProperty;
                    var renderElement = state.itemElements[renderProperty];
                    if (renderElement) {
                        if (!aircraft)
                            renderElement.text('');
                        else if (forceRefresh || handler.hasChangedCallback(aircraft)) {
                            handler.renderToJQuery(renderElement, VRS.RenderSurface.InfoWindow, aircraft, options);
                        }
                    }
                }
                state.linksPlugin.renderForAircraft(state.aircraft, false);
            }
            if (!mapMarker || !options.enabled) {
                if (mapInfoWindow.isOpen)
                    map.closeInfoWindow(mapInfoWindow);
                state.anchoredAircraft = null;
            }
            else {
                if (forceRefresh) {
                    if (mapInfoWindow.isOpen)
                        map.closeInfoWindow(mapInfoWindow);
                    map.openInfoWindow(mapInfoWindow, mapMarker);
                    state.anchoredAircraft = aircraft;
                }
            }
        };
        AircraftInfoWindowPlugin.prototype._selectedAircraftChanged = function () {
            var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
            this.showForAircraft(selectedAircraft);
        };
        AircraftInfoWindowPlugin.prototype._markerClicked = function (event, data) {
            var state = this._getState();
            var options = this.options;
            if (state.mapInfoWindow) {
                var aircraft = options.aircraftPlotter.getAircraftForMarkerId(data.id);
                if (aircraft) {
                    state.closedByUser = false;
                    if (!state.mapInfoWindow.isOpen || state.aircraft != aircraft)
                        this.showForAircraft(aircraft);
                }
            }
        };
        AircraftInfoWindowPlugin.prototype._infoWindowClosedByUser = function (event, data) {
            var state = this._getState();
            state.closedByUser = true;
        };
        AircraftInfoWindowPlugin.prototype._aircraftListUpdated = function () {
            var state = this._getState();
            var options = this.options;
            if (state.aircraft) {
                if (!options.aircraftList.findAircraftById(state.aircraft.id))
                    this.showForAircraft(null);
                else
                    this._displayDetails(state, false);
            }
        };
        AircraftInfoWindowPlugin.prototype._displayUnitChanged = function () {
            var state = this._getState();
            if (state.aircraft)
                this.refreshDisplay();
        };
        AircraftInfoWindowPlugin.prototype._localeChanged = function () {
            var state = this._getState();
            if (state.aircraft)
                this.refreshDisplay();
        };
        return AircraftInfoWindowPlugin;
    }(JQueryUICustomWidget));
    VRS.AircraftInfoWindowPlugin = AircraftInfoWindowPlugin;
    $.widget('vrs.vrsAircraftInfoWindow', new AircraftInfoWindowPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.aircraftInfoWindow.js.map