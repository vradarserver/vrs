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
    VRS.globalOptions.detailPanelDefaultShowUnits = VRS.globalOptions.detailPanelDefaultShowUnits !== undefined ? VRS.globalOptions.detailPanelDefaultShowUnits : true;
    if (VRS.globalOptions.isMobile) {
        VRS.globalOptions.detailPanelDefaultItems = VRS.globalOptions.detailPanelDefaultItems || [
            VRS.RenderProperty.Altitude,
            VRS.RenderProperty.VerticalSpeed,
            VRS.RenderProperty.Speed,
            VRS.RenderProperty.Heading,
            VRS.RenderProperty.Distance,
            VRS.RenderProperty.Squawk,
            VRS.RenderProperty.Engines,
            VRS.RenderProperty.Species,
            VRS.RenderProperty.Wtc,
            VRS.RenderProperty.RouteFull,
            VRS.RenderProperty.PictureOrThumbnails,
            VRS.RenderProperty.PositionOnMap
        ];
    }
    else {
        VRS.globalOptions.detailPanelDefaultItems = VRS.globalOptions.detailPanelDefaultItems || [
            VRS.RenderProperty.Altitude,
            VRS.RenderProperty.VerticalSpeed,
            VRS.RenderProperty.Speed,
            VRS.RenderProperty.Heading,
            VRS.RenderProperty.Distance,
            VRS.RenderProperty.Squawk,
            VRS.RenderProperty.Engines,
            VRS.RenderProperty.Species,
            VRS.RenderProperty.Wtc,
            VRS.RenderProperty.RouteFull,
            VRS.RenderProperty.PictureOrThumbnails
        ];
    }
    VRS.globalOptions.detailPanelUserCanConfigureItems = VRS.globalOptions.detailPanelUserCanConfigureItems !== undefined ? VRS.globalOptions.detailPanelUserCanConfigureItems : true;
    VRS.globalOptions.detailPanelShowSeparateRouteLink = VRS.globalOptions.detailPanelShowSeparateRouteLink !== undefined ? VRS.globalOptions.detailPanelShowSeparateRouteLink : true;
    VRS.globalOptions.detailPanelShowAircraftLinks = VRS.globalOptions.detailPanelShowAircraftLinks !== undefined ? VRS.globalOptions.detailPanelShowAircraftLinks : true;
    VRS.globalOptions.detailPanelShowEnableAutoSelect = VRS.globalOptions.detailPanelShowEnableAutoSelect !== undefined ? VRS.globalOptions.detailPanelShowEnableAutoSelect : true;
    VRS.globalOptions.detailPanelShowCentreOnAircraft = VRS.globalOptions.detailPanelShowCentreOnAircraft !== undefined ? VRS.globalOptions.detailPanelShowCentreOnAircraft : true;
    VRS.globalOptions.detailPanelFlagUncertainCallsigns = VRS.globalOptions.detailPanelFlagUncertainCallsigns !== undefined ? VRS.globalOptions.detailPanelFlagUncertainCallsigns : true;
    VRS.globalOptions.detailPanelDistinguishOnGround = VRS.globalOptions.detailPanelDistinguishOnGround !== undefined ? VRS.globalOptions.detailPanelDistinguishOnGround : true;
    VRS.globalOptions.detailPanelAirportDataThumbnails = VRS.globalOptions.detailPanelAirportDataThumbnails || 2;
    VRS.globalOptions.detailPanelUseShortLabels = VRS.globalOptions.detailPanelUseShortLabels !== undefined ? VRS.globalOptions.detailPanelUseShortLabels : false;
    var AircraftDetailPlugin_State = (function () {
        function AircraftDetailPlugin_State() {
            this.suspended = false;
            this.container = undefined;
            this.noAircraftContainer = undefined;
            this.headerContainer = undefined;
            this.bodyContainer = undefined;
            this.linksContainer = undefined;
            this.aircraftLinksPlugin = null;
            this.routeLinksPlugin = null;
            this.noAircraftLinksPlugin = null;
            this.autoSelectLinkRenderHelper = null;
            this.centreOnSelectedAircraftLinkRenderHandler = null;
            this.aircraftListUpdatedHook = undefined;
            this.selectedAircraftChangedHook = undefined;
            this.localeChangedHook = undefined;
            this.unitChangedHook = undefined;
            this.headerProperties = [];
            this.bodyProperties = [];
            this.aircraftLastRendered = undefined;
        }
        return AircraftDetailPlugin_State;
    }());
    var AircraftDetailProperty = (function () {
        function AircraftDetailProperty(property, isHeader, element, options) {
            this.element = element;
            if (!property)
                throw 'You must supply a RenderProperty';
            var handler = VRS.renderPropertyHandlers[property];
            if (!handler)
                throw 'The render property ' + property + ' has no handler declared for it';
            if (!handler.isSurfaceSupported(isHeader ? VRS.RenderSurface.DetailHead : VRS.RenderSurface.DetailBody))
                throw 'The render property ' + property + ' does not support rendering on detail ' + (isHeader ? 'headers' : 'bodies');
            if (!element)
                throw 'You must supply a jQuery element';
            this.handler = handler;
            this.contentElement = isHeader ? element : element.children().first();
            handler.createWidgetInJQuery(this.contentElement, isHeader ? VRS.RenderSurface.DetailHead : VRS.RenderSurface.DetailBody, options);
        }
        return AircraftDetailProperty;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftDetailPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsAircraftDetail');
    };
    VRS.jQueryUIHelper.getAircraftDetailOptions = function (overrides) {
        return $.extend({
            name: 'default',
            useSavedState: true,
            showUnits: VRS.globalOptions.detailPanelDefaultShowUnits,
            items: VRS.globalOptions.detailPanelDefaultItems.slice(),
            showSeparateRouteLink: VRS.globalOptions.detailPanelShowSeparateRouteLink,
            flagUncertainCallsigns: VRS.globalOptions.detailPanelFlagUncertainCallsigns,
            distinguishOnGround: VRS.globalOptions.detailPanelDistinguishOnGround,
            airportDataThumbnails: VRS.globalOptions.detailPanelAirportDataThumbnails,
            useShortLabels: VRS.globalOptions.detailPanelUseShortLabels,
            unitDisplayPreferences: undefined
        }, overrides);
    };
    var AircraftDetailPlugin = (function (_super) {
        __extends(AircraftDetailPlugin, _super);
        function AircraftDetailPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getAircraftDetailOptions();
            return _this;
        }
        AircraftDetailPlugin.prototype._getState = function () {
            var result = this.element.data('aircraftDetailPluginState');
            if (result === undefined) {
                result = new AircraftDetailPlugin_State();
                this.element.data('aircraftDetailPluginState', result);
            }
            return result;
        };
        AircraftDetailPlugin.prototype._create = function () {
            var options = this.options;
            if (!options.aircraftList)
                throw 'An aircraft list must be supplied';
            if (!options.unitDisplayPreferences)
                throw 'A unit display preferences object must be supplied';
            if (options.useSavedState) {
                this.loadAndApplyState();
            }
            var state = this._getState();
            state.container =
                $('<div/>')
                    .addClass('vrsAircraftDetail aircraft')
                    .appendTo(this.element);
            state.headerContainer =
                $('<div/>')
                    .addClass('header')
                    .appendTo(state.container);
            state.bodyContainer =
                $('<div/>')
                    .addClass('body')
                    .appendTo(state.container);
            state.linksContainer =
                $('<div/>')
                    .addClass('links')
                    .appendTo(state.container);
            state.noAircraftContainer =
                $('<div/>')
                    .addClass('vrsAircraftDetail noAircraft')
                    .appendTo(this.element);
            this._buildContent(state);
            state.aircraftListUpdatedHook = options.aircraftList.hookUpdated(this._aircraftListUpdated, this);
            state.selectedAircraftChangedHook = options.aircraftList.hookSelectedAircraftChanged(this._selectedAircraftChanged, this);
            state.localeChangedHook = VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.unitChangedHook = options.unitDisplayPreferences.hookUnitChanged(this._displayUnitChanged, this);
            this._renderContent(state, options.aircraftList.getSelectedAircraft(), true);
        };
        AircraftDetailPlugin.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            state.headerProperties = this._destroyProperties(VRS.RenderSurface.DetailHead, state.headerProperties);
            state.bodyProperties = this._destroyProperties(VRS.RenderSurface.DetailBody, state.bodyProperties);
            if (state.container) {
                this.element.empty();
                state.container = undefined;
            }
            if (state.aircraftLinksPlugin) {
                state.aircraftLinksPlugin.destroy();
                state.aircraftLinksPlugin = null;
            }
            if (state.routeLinksPlugin) {
                state.routeLinksPlugin.destroy();
                state.routeLinksPlugin = null;
            }
            if (state.autoSelectLinkRenderHelper) {
                state.autoSelectLinkRenderHelper.dispose();
                state.autoSelectLinkRenderHelper = null;
            }
            if (state.aircraftListUpdatedHook) {
                if (options && options.aircraftList)
                    options.aircraftList.unhook(state.aircraftListUpdatedHook);
                state.aircraftListUpdatedHook = undefined;
            }
            if (state.selectedAircraftChangedHook) {
                if (options && options.aircraftList)
                    options.aircraftList.unhook(state.selectedAircraftChangedHook);
                state.selectedAircraftChangedHook = undefined;
            }
            if (state.localeChangedHook) {
                if (VRS.globalisation)
                    VRS.globalisation.unhook(state.localeChangedHook);
                state.localeChangedHook = undefined;
            }
            if (state.unitChangedHook) {
                if (options && options.unitDisplayPreferences)
                    options.unitDisplayPreferences.unhook(state.unitChangedHook);
                state.unitChangedHook = undefined;
            }
        };
        AircraftDetailPlugin.prototype._destroyProperties = function (surface, properties) {
            var length = properties.length;
            for (var i = 0; i < length; ++i) {
                var property = properties[i];
                if (property.element) {
                    property.handler.destroyWidgetInJQuery(property.contentElement, surface);
                    property.element.remove();
                }
            }
            return [];
        };
        AircraftDetailPlugin.prototype.suspend = function (onOff) {
            onOff = !!onOff;
            var state = this._getState();
            if (state.suspended !== onOff) {
                state.suspended = onOff;
                this._suspendWidgets(state, onOff);
                if (!state.suspended) {
                    var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                    this._renderContent(state, selectedAircraft, true);
                }
            }
        };
        AircraftDetailPlugin.prototype._suspendWidgets = function (state, onOff) {
            this._suspendWidgetProperties(state, onOff, state.headerProperties, VRS.RenderSurface.DetailHead);
            this._suspendWidgetProperties(state, onOff, state.bodyProperties, VRS.RenderSurface.DetailBody);
        };
        AircraftDetailPlugin.prototype._suspendWidgetProperties = function (state, onOff, properties, surface) {
            var length = properties.length;
            for (var i = 0; i < length; ++i) {
                var property = properties[i];
                property.handler.suspendWidget(property.contentElement, surface, onOff);
            }
        };
        AircraftDetailPlugin.prototype.saveState = function () {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        };
        AircraftDetailPlugin.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.items = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.items, [VRS.RenderSurface.DetailBody]);
            return result;
        };
        AircraftDetailPlugin.prototype.applyState = function (settings) {
            this.options.showUnits = settings.showUnits;
            this.options.items = settings.items;
            this.options.useShortLabels = !!settings.useShortLabels;
        };
        AircraftDetailPlugin.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        AircraftDetailPlugin.prototype._persistenceKey = function () {
            return 'vrsAircraftDetailPlugin-' + this.options.name;
        };
        AircraftDetailPlugin.prototype._createSettings = function () {
            return {
                showUnits: this.options.showUnits,
                items: this.options.items,
                useShortLabels: this.options.useShortLabels
            };
        };
        AircraftDetailPlugin.prototype.createOptionPane = function (displayOrder) {
            var _this = this;
            var result = [];
            var state = this._getState();
            var options = this.options;
            var settingsPane = new VRS.OptionPane({
                name: 'vrsAircraftDetailPlugin_' + options.name + '_Settings',
                titleKey: 'PaneDetailSettings',
                displayOrder: displayOrder,
                fields: [
                    new VRS.OptionFieldCheckBox({
                        name: 'showUnits',
                        labelKey: 'ShowUnits',
                        getValue: function () { return options.showUnits; },
                        setValue: function (value) {
                            options.showUnits = value;
                            _this._buildContent(_this._getState());
                            _this._reRenderAircraft(_this._getState());
                        },
                        saveState: function () { return _this.saveState(); }
                    }),
                    new VRS.OptionFieldCheckBox({
                        name: 'useShortLabels',
                        labelKey: 'UseShortLabels',
                        getValue: function () { return options.useShortLabels; },
                        setValue: function (value) {
                            options.useShortLabels = value;
                            _this._buildContent(_this._getState());
                            _this._reRenderAircraft(_this._getState());
                        },
                        saveState: function () { return _this.saveState(); }
                    })
                ]
            });
            result.push(settingsPane);
            if (VRS.globalOptions.detailPanelUserCanConfigureItems) {
                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane: settingsPane,
                    fieldLabel: 'DetailItems',
                    surface: VRS.RenderSurface.DetailBody,
                    getList: function () { return options.items; },
                    setList: function (items) {
                        options.items = items;
                        _this._buildBody(state);
                        _this._reRenderAircraft(state);
                    },
                    saveState: function () { return _this.saveState(); }
                });
            }
            return result;
        };
        AircraftDetailPlugin.prototype._buildContent = function (state) {
            this._buildHeader(state);
            this._buildBody(state);
            this._buildLinks(state);
        };
        AircraftDetailPlugin.prototype._buildHeader = function (state) {
            var options = this.options;
            state.headerProperties = this._destroyProperties(VRS.RenderSurface.DetailHead, state.headerProperties);
            state.headerContainer.empty();
            var table = $('<table/>')
                .appendTo(state.headerContainer), row1 = $('<tr/>')
                .appendTo(table), regElement = $('<td/>')
                .addClass('reg')
                .appendTo(row1), icaoElement = $('<td/>')
                .addClass('icao')
                .appendTo(row1), bearingElement = $('<td/>')
                .addClass('bearing')
                .appendTo(row1), opFlagElement = $('<td/>')
                .addClass('flag')
                .appendTo(row1), row2 = $('<tr/>')
                .appendTo(table), operatorElement = $('<td/>')
                .addClass('op')
                .attr('colspan', '3')
                .appendTo(row2), callsignElement = $('<td/>')
                .addClass('callsign')
                .appendTo(row2), row3 = $('<tr/>')
                .appendTo(table), countryElement = $('<td/>')
                .addClass('country')
                .attr('colspan', '3')
                .appendTo(row3), militaryElement = $('<td/>')
                .addClass('military')
                .appendTo(row3), row4 = $('<tr/>')
                .appendTo(table), modelElement = $('<td/>')
                .addClass('model')
                .attr('colspan', '3')
                .appendTo(row4), modelTypeElement = $('<td/>')
                .addClass('modelType')
                .appendTo(row4);
            state.headerProperties = [
                new AircraftDetailProperty(VRS.RenderProperty.Registration, true, regElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.Icao, true, icaoElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.Bearing, true, bearingElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.OperatorFlag, true, opFlagElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.Operator, true, operatorElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.Callsign, true, callsignElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.Country, true, countryElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.CivOrMil, true, militaryElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.Model, true, modelElement, options),
                new AircraftDetailProperty(VRS.RenderProperty.ModelIcao, true, modelTypeElement, options)
            ];
        };
        AircraftDetailPlugin.prototype._buildBody = function (state) {
            state.bodyProperties = this._destroyProperties(VRS.RenderSurface.DetailBody, state.bodyProperties);
            state.bodyContainer.empty();
            var options = this.options;
            var list = $('<ul/>')
                .appendTo(state.bodyContainer);
            state.bodyProperties = [];
            var countProperties = options.items.length;
            for (var i = 0; i < countProperties; ++i) {
                var property = options.items[i];
                var handler = VRS.renderPropertyHandlers[property];
                var suppressLabel = handler.suppressLabelCallback(VRS.RenderSurface.DetailBody);
                var listItem = $('<li/>')
                    .appendTo(list);
                if (!suppressLabel) {
                    $('<div/>')
                        .addClass('label')
                        .append($('<span/>')
                        .text(VRS.globalisation.getText(options.useShortLabels ? handler.headingKey : handler.labelKey) + ':'))
                        .appendTo(listItem);
                }
                var content = $('<div/>')
                    .addClass('content')
                    .append($('<span/>'))
                    .appendTo(listItem);
                if (suppressLabel) {
                    listItem.addClass('wide');
                    content.addClass('wide');
                }
                if (handler.isMultiLine) {
                    listItem.addClass('multiline');
                    content.addClass('multiline');
                }
                state.bodyProperties.push(new AircraftDetailProperty(property, false, content, options));
            }
        };
        AircraftDetailPlugin.prototype._buildLinks = function (state) {
            var options = this.options;
            if (state.aircraftLinksPlugin) {
                state.aircraftLinksPlugin.destroy();
            }
            if (state.noAircraftLinksPlugin) {
                state.noAircraftLinksPlugin.destroy();
            }
            state.linksContainer.empty();
            state.noAircraftContainer.empty();
            var aircraftLinksElement = $('<div/>')
                .appendTo(state.linksContainer)
                .vrsAircraftLinks();
            state.aircraftLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(aircraftLinksElement);
            var routeLinks = [];
            if (options.mapPlugin && options.aircraftList && VRS.globalOptions.detailPanelShowCentreOnAircraft) {
                state.centreOnSelectedAircraftLinkRenderHandler = new VRS.CentreOnSelectedAircraftLinkRenderHandler(options.aircraftList, options.mapPlugin);
                routeLinks.push(state.centreOnSelectedAircraftLinkRenderHandler);
            }
            if (options.aircraftAutoSelect && VRS.globalOptions.detailPanelShowEnableAutoSelect) {
                state.autoSelectLinkRenderHelper = new VRS.AutoSelectLinkRenderHelper(options.aircraftAutoSelect);
                routeLinks.push(state.autoSelectLinkRenderHelper);
            }
            if (VRS.globalOptions.detailPanelShowSeparateRouteLink) {
                routeLinks.push(VRS.LinkSite.StandingDataMaintenance);
            }
            if (routeLinks.length > 0) {
                var routeLinksElement = $('<div/>')
                    .appendTo(state.linksContainer)
                    .vrsAircraftLinks({
                    linkSites: routeLinks
                });
                state.routeLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(routeLinksElement);
                if (state.autoSelectLinkRenderHelper)
                    state.autoSelectLinkRenderHelper.addLinksRendererPlugin(state.routeLinksPlugin);
            }
            if (options.aircraftAutoSelect && VRS.globalOptions.detailPanelShowEnableAutoSelect) {
                var noAircraftLinksElement = $('<div/>')
                    .appendTo(state.noAircraftContainer)
                    .vrsAircraftLinks({
                    linkSites: [state.autoSelectLinkRenderHelper]
                });
                state.noAircraftLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(noAircraftLinksElement);
                state.autoSelectLinkRenderHelper.addLinksRendererPlugin(state.noAircraftLinksPlugin);
            }
        };
        AircraftDetailPlugin.prototype._renderContent = function (state, aircraft, refreshAll, displayUnit) {
            state.aircraftLastRendered = aircraft;
            if (!aircraft) {
                $(state.container, ':visible').hide();
                $(state.noAircraftContainer, ':hidden').show();
                this._renderLinks(state, null, refreshAll);
            }
            else {
                $(state.container, ':hidden').show();
                $(state.noAircraftContainer, ':visible').hide();
                this._renderProperties(state, aircraft, refreshAll, state.headerProperties, VRS.RenderSurface.DetailHead, displayUnit);
                this._renderProperties(state, aircraft, refreshAll, state.bodyProperties, VRS.RenderSurface.DetailBody, displayUnit);
                this._renderLinks(state, aircraft, refreshAll);
            }
        };
        AircraftDetailPlugin.prototype._renderProperties = function (state, aircraft, refreshAll, properties, surface, displayUnit) {
            var options = this.options;
            var countProperties = properties.length;
            var refreshedContent = false;
            for (var i = 0; i < countProperties; ++i) {
                var property = properties[i];
                var handler = property.handler;
                var element = property.contentElement;
                var forceRefresh = refreshAll;
                if (!forceRefresh)
                    forceRefresh = displayUnit && handler.usesDisplayUnit(displayUnit);
                if (forceRefresh || handler.hasChangedCallback(aircraft)) {
                    handler.renderToJQuery(element, surface, aircraft, options);
                    refreshedContent = true;
                }
                if (forceRefresh || handler.tooltipChangedCallback(aircraft)) {
                    handler.renderTooltipToJQuery(element, surface, aircraft, options);
                }
            }
        };
        AircraftDetailPlugin.prototype._renderLinks = function (state, aircraft, refreshAll) {
            if (aircraft === null) {
                if (state.noAircraftLinksPlugin) {
                    state.noAircraftLinksPlugin.renderForAircraft(null, refreshAll);
                }
            }
            else {
                if (VRS.globalOptions.detailPanelShowAircraftLinks) {
                    state.aircraftLinksPlugin.renderForAircraft(aircraft, refreshAll);
                }
                if (state.routeLinksPlugin) {
                    state.routeLinksPlugin.renderForAircraft(aircraft, refreshAll);
                }
            }
        };
        AircraftDetailPlugin.prototype._reRenderAircraft = function (state) {
            this._renderContent(state, state.aircraftLastRendered, true);
        };
        AircraftDetailPlugin.prototype._aircraftListUpdated = function () {
            var state = this._getState();
            if (!state.suspended) {
                var options = this.options;
                var selectedAircraft = options.aircraftList.getSelectedAircraft();
                if (selectedAircraft)
                    this._renderContent(state, selectedAircraft, false);
            }
        };
        AircraftDetailPlugin.prototype._displayUnitChanged = function (displayUnit) {
            var state = this._getState();
            if (!state.suspended) {
                var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                if (selectedAircraft)
                    this._renderContent(state, selectedAircraft, false, displayUnit);
            }
        };
        AircraftDetailPlugin.prototype._selectedAircraftChanged = function () {
            var state = this._getState();
            if (!state.suspended) {
                var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                this._renderContent(state, selectedAircraft, true);
            }
        };
        AircraftDetailPlugin.prototype._localeChanged = function () {
            var state = this._getState();
            this._buildContent(state);
            if (!state.suspended)
                this._reRenderAircraft(state);
        };
        return AircraftDetailPlugin;
    }(JQueryUICustomWidget));
    VRS.AircraftDetailPlugin = AircraftDetailPlugin;
    $.widget('vrs.vrsAircraftDetail', new AircraftDetailPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.aircraftDetail.js.map