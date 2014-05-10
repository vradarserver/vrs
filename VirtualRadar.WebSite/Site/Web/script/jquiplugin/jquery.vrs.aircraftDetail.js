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
 * @fileoverview A jQueryUI plugin that can manage the detail panel for an aircraft.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    /** @type {VRS_GLOBAL_OPTIONS} */
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.detailPanelDefaultShowUnits = VRS.globalOptions.detailPanelDefaultShowUnits !== undefined ? VRS.globalOptions.detailPanelDefaultShowUnits : true;     // True if the aircraft details panel should default to showing distance / speed / height units.
    if(VRS.globalOptions.isMobile) {
        VRS.globalOptions.detailPanelDefaultItems = VRS.globalOptions.detailPanelDefaultItems || [              // The default set of items to display on the detail panel.
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
    } else {
        VRS.globalOptions.detailPanelDefaultItems = VRS.globalOptions.detailPanelDefaultItems || [              // The default set of items to display on the detail panel.
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
    VRS.globalOptions.detailPanelUserCanConfigureItems = VRS.globalOptions.detailPanelUserCanConfigureItems !== undefined ? VRS.globalOptions.detailPanelUserCanConfigureItems : true;    // True if the user can change the items shown for an aircraft in the details panel.
    VRS.globalOptions.detailPanelShowSeparateRouteLink = VRS.globalOptions.detailPanelShowSeparateRouteLink !== undefined ? VRS.globalOptions.detailPanelShowSeparateRouteLink : true;  // Show a separate link to add or correct routes if the detail panel is showing routes.
    VRS.globalOptions.detailPanelShowAircraftLinks = VRS.globalOptions.detailPanelShowAircraftLinks !== undefined ? VRS.globalOptions.detailPanelShowAircraftLinks : true;              // True to show the links for an aircraft, false to suppress them.
    VRS.globalOptions.detailPanelShowEnableAutoSelect = VRS.globalOptions.detailPanelShowEnableAutoSelect !== undefined ? VRS.globalOptions.detailPanelShowEnableAutoSelect : true;     // True to show a link to enable and disable auto-select, false to suppress the link.
    VRS.globalOptions.detailPanelShowCentreOnAircraft = VRS.globalOptions.detailPanelShowCentreOnAircraft !== undefined ? VRS.globalOptions.detailPanelShowCentreOnAircraft : true;     // True to show a link to centre the map on the selected aircraft.
    VRS.globalOptions.detailPanelFlagUncertainCallsigns = VRS.globalOptions.detailPanelFlagUncertainCallsigns !== undefined ? VRS.globalOptions.detailPanelFlagUncertainCallsigns : true; // True if uncertain callsigns are to be flagged up on the detail panel.
    VRS.globalOptions.detailPanelDistinguishOnGround = VRS.globalOptions.detailPanelDistinguishOnGround !== undefined ? VRS.globalOptions.detailPanelDistinguishOnGround : true;        // True if altitudes should show 'GND' when the aircraft is on the ground.
    VRS.globalOptions.detailPanelAirportDataThumbnails = VRS.globalOptions.detailPanelAirportDataThumbnails || 2;                                                                       // The number of thumbnails to fetch from www.airport-data.com.
    //endregion

    //region AircraftDetailPluginState
    /**
     * Carries the state for the VRS.AircraftDetail plugin.
     * @constructor
     */
    VRS.AircraftDetailPluginState = function()
    {
        /**
         * Indicates whether the detail panel responds to updates or not.
         * @type {boolean}
         */
        this.suspended = false;

        /**
         * A jQuery element for the container for the panel when an aircraft is selected.
         * @type {jQuery=} */
        this.container = undefined;

        /**
         * A jQuery element for the container for the panel when no aircraft is selected.
         * @type {jQuery=}
         */
        this.noAircraftContainer = undefined;

        /**
         * A jQuery element for the header's container.
         * @type {jQuery=} */
        this.headerContainer = undefined;

        /**
         * A jQuery element for the body's container.
         * @type {jQuery=} */
        this.bodyContainer = undefined;

        /**
         * A jQuery element for the links.
         * @type {jQuery=}
         */
        this.linksContainer = undefined;

        /**
         * A direct reference to the plugin for standard aircraft links.
         * @type {VRS.vrsAircraftLinks}
         */
        this.aircraftLinksPlugin = null;

        /**
         * A direct reference to the plugin for links to the routes submission site.
         * @type {VRS.vrsAircraftLinks}
         */
        this.routeLinksPlugin = null;

        /**
         * A direct reference to the plugin for links that are shown when no aircraft is selected.
         * @type {VRS.vrsAircraftLinks}
         */
        this.noAircraftLinksPlugin = null;

        /**
         * An instance of a link render helper that can manage a link to enable and disable auto-select.
         * @type {VRS.AutoSelectLinkRenderHelper}
         */
        this.autoSelectLinkRenderHelper = null;

        /**
         * An instance of a link render helper that can centre the map on the selected aircraft.
         * @type {VRS.CentreOnSelectedAircraftLinkRenderHandler}
         */
        this.centreOnSelectedAircraftLinkRenderHandler = null;

        /**
         * The hook result for the VRS.AircraftList.updated event.
         * @type {object=} */
        this.aircraftListUpdatedHook = undefined;

        /**
         * The hook result for the VRS.AircraftList.selectedAircraftChanged event.
         * @type {object=} */
        this.selectedAircraftChangedHook = undefined;

        /**
         * The hook result for the VRS.Localise.localeChanged event.
         * @type {object=} */
        this.localeChangedHook = undefined;

        /**
         * The hook result for the VRS.UnitDisplayPreferences.unitChanged event.
         * @type {object=} */
        this.unitChangedHook = undefined;

        /**
         * An array of VRS.AircraftDetailProperty objects that have been rendered into the detail panel's header.
         * @type {Array.<VRS.AircraftDetailProperty>} */
        this.headerProperties = [];

        /**
         * An array of VRS.AircraftDetailProperty objects that have been rendered into the detail panel's body.
         * @type {Array.<VRS.AircraftDetailProperty>}
         */
        this.bodyProperties = [];

        /**
         * The aircraft last rendered.
         * @type {VRS.Aircraft}
         */
        this.aircraftLastRendered = undefined;
    };
    //endregion

    //region AircraftDetailProperty
    /**
     * Describes a property that has been rendered into the aircraft detail panel.
     * @param {VRS.RenderProperty}  property        The property that has been rendered.
     * @param {bool}                isHeader        True if it's in the header, false if it's in the body.
     * @param {jQuery}              element         The element that it's been rendered into.
     * @param {*}                   options         The options to pass to the property handlers that create jQueryUI widgets.
     * @constructor
     */
    VRS.AircraftDetailProperty = function(property, isHeader, element, options)
    {
        if(!property) throw 'You must supply a RenderProperty';
        var handler = VRS.renderPropertyHandlers[property];
        if(!handler) throw 'The render property ' + property + ' has no handler declared for it';
        if(!handler.isSurfaceSupported(isHeader ? VRS.RenderSurface.DetailHead : VRS.RenderSurface.DetailBody)) throw 'The render property ' + property + ' does not support rendering on detail ' + (isHeader ? 'headers' : 'bodies');
        if(!element) throw 'You must supply a jQuery element';

        this.handler = handler;
        this.element = element;
        this.contentElement = isHeader ? element : element.children().first();

        handler.createWidgetInJQuery(this.contentElement, isHeader ? VRS.RenderSurface.DetailHead : VRS.RenderSurface.DetailBody, options);
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    /**
     * Returns the aircraft detail plugin attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsAircraftDetail}
     */
    VRS.jQueryUIHelper.getAircraftDetailPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsAircraftDetail'); };

    /**
     * Builds the default options for the vrsAircraftDetail plugin. These should be preferred over the normal jQuery UI
     * mechanism for setting the widget's options because these can be overridden by changes to VRS.globalOptions.
     * @param {VRS_OPTIONS_AIRCRAFTDETAIL=} overrides
     * @returns {VRS_OPTIONS_AIRCRAFTDETAIL}
     */
    VRS.jQueryUIHelper.getAircraftDetailOptions = function(overrides)
    {
        return $.extend({
            name: 'default',                                                                // The name to use when saving and loading state.
            aircraftList: undefined,                                                        // The VRS.AircraftList to display aircraft details from.
            unitDisplayPreferences: undefined,                                              // The VRS.UnitDisplayPreferences that dictates how values are to be displayed.
            aircraftAutoSelect: undefined,                                                  // The VRS.AircraftAutoSelect that, if supplied, can be configured via controls in the detail panel.
            mapPlugin: undefined,                                                           // The map to centre when the 'Centre selected aircraft on map' link is clicked. If it isn't supplied then the option isn't shown.
            useSavedState: true,                                                            // True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
            showUnits: VRS.globalOptions.detailPanelDefaultShowUnits,                       // True if heights, distances and speeds should indicate their units.
            items: VRS.globalOptions.detailPanelDefaultItems.slice(),                       // The items to show to the user aside from those that are always shown in the detail header.
            showSeparateRouteLink: VRS.globalOptions.detailPanelShowSeparateRouteLink,      // True if a separate link to add or correct links is to be shown if the user is displaying routes. Always hidden if there are no routes on display.
            flagUncertainCallsigns: VRS.globalOptions.detailPanelFlagUncertainCallsigns,    // True if uncertain callsigns are to show be flagged up as such on display.
            distinguishOnGround: VRS.globalOptions.detailPanelDistinguishOnGround,          // True if aircraft on the ground should be shown with altitudes of 'GND'.
            mirrorMapJQ: undefined,                                                         // The map to pass to property renderers that display mirrored maps.
            plotterOptions: undefined,                                                      // The plotter options to pass to property renderers that employ aircraft plotters.
            airportDataThumbnails:  VRS.globalOptions.detailPanelAirportDataThumbnails,     // The number of thumbnails to fetch from www.airport-data.com.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsAircraftDetail
    /**
     * A jQuery widget that can display detail about a selected aircraft in a list.
     * @namespace VRS.vrsAircraftDetail
     */
    $.widget('vrs.vrsAircraftDetail', {
        //region -- options
        /** @type {VRS_OPTIONS_AIRCRAFTDETAIL} */
        options: VRS.jQueryUIHelper.getAircraftDetailOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object for the plugin, creating it if it's not already there.
         * @returns {VRS.AircraftDetailPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('aircraftDetailPluginState');
            if(result === undefined) {
                result = new VRS.AircraftDetailPluginState();
                this.element.data('aircraftDetailPluginState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the plugin.
         * @private
         */
        _create: function()
        {
            var options = this.options;
            if(!options.aircraftList) throw 'An aircraft list must be supplied';
            if(!options.unitDisplayPreferences) throw 'A unit display preferences object must be supplied';

            if(options.useSavedState) this.loadAndApplyState();

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
        },

        /**
         * Called when the plugin is destroyed, removes any resources held by the plugin.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            var options = this.options;

            state.headerProperties = this._destroyProperties(VRS.RenderSurface.DetailHead, state.headerProperties);
            state.bodyProperties =   this._destroyProperties(VRS.RenderSurface.DetailBody, state.bodyProperties);

            if(state.container) {
                this.element.empty();
                state.container = undefined;
            }
            if(state.aircraftLinksPlugin) {
                state.aircraftLinksPlugin.destroy();
                state.aircraftLinksPlugin = null;
            }
            if(state.routeLinksPlugin) {
                state.routeLinksPlugin.destroy();
                state.routeLinksPlugin = null;
            }
            if(state.autoSelectLinkRenderHelper) {
                state.autoSelectLinkRenderHelper.dispose();
                state.autoSelectLinkRenderHelper = null;
            }
            if(state.aircraftListUpdatedHook) {
                if(options && options.aircraftList) options.aircraftList.unhook(state.aircraftListUpdatedHook);
                state.aircraftListUpdatedHook = undefined;
            }
            if(state.selectedAircraftChangedHook) {
                if(options && options.aircraftList) options.aircraftList.unhook(state.selectedAircraftChangedHook);
                state.selectedAircraftChangedHook = undefined;
            }
            if(state.localeChangedHook) {
                if(VRS.globalisation) VRS.globalisation.unhook(state.localeChangedHook);
                state.localeChangedHook = undefined;
            }
            if(state.unitChangedHook) {
                if(options && options.unitDisplayPreferences) options.unitDisplayPreferences.unhook(state.unitChangedHook);
                state.unitChangedHook = undefined;
            }
        },

        /**
         * Destroys the elements and releases any resources held by a collection of properties.
         * @param {VRS.RenderSurface}                   surface
         * @param {Array.<VRS.AircraftDetailProperty>}  properties
         * @returns {Array.<VRS.AircraftDetailProperty>}
         * @private
         */
        _destroyProperties: function(surface, properties)
        {
            var length = properties.length;
            for(var i = 0;i < length;++i) {
                var property = properties[i];
                if(property.element) {
                    property.handler.destroyWidgetInJQuery(property.contentElement, surface);
                    property.element.remove();
                }
            }

            return [];
        },
        //endregion

        //region -- suspend
        /**
         * Suspends or resumes updates.
         * @param {boolean} onOff
         */
        suspend: function(onOff)
        {
            onOff = !!onOff;
            var state = this._getState();
            if(state.suspended !== onOff) {
                state.suspended = onOff;
                this._suspendWidgets(state, onOff);

                if(!state.suspended) {
                    var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                    this._renderContent(state, selectedAircraft, true);
                }
            }
        },

        /**
         * Suspends or resumes widgets that have been used to render items.
         * @param {VRS.AircraftDetailPluginState}   state
         * @param {boolean}                         onOff
         * @private
         */
        _suspendWidgets: function(state, onOff)
        {
            this._suspendWidgetProperties(state, onOff, state.headerProperties, VRS.RenderSurface.DetailHead);
            this._suspendWidgetProperties(state, onOff, state.bodyProperties, VRS.RenderSurface.DetailBody);
        },

        /**
         * Suspends or resumes widgets used to render an array of properties.
         * @param {VRS.AircraftDetailPluginState}       state
         * @param {boolean}                             onOff
         * @param {Array.<VRS.AircraftDetailProperty>}  properties
         * @param {VRS.RenderSurface}                   surface
         * @private
         */
        _suspendWidgetProperties: function(state, onOff, properties, surface)
        {
            var length = properties.length;
            for(var i = 0;i < length;++i) {
                var property = properties[i];
                property.handler.suspendWidget(property.contentElement, surface, onOff);
            }
        },
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state.
         */
        saveState: function()
        {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        },

        /**
         * Returns the saved state or, if there is no saved state, the current state.
         * @returns {VRS_STATE_AIRCRAFTDETAIL}
         */
        loadState: function()
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.items = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.items, [ VRS.RenderSurface.DetailBody ]);

            return result;
        },

        /**
         * Applies the saved state to the object.
         * @param {VRS_STATE_AIRCRAFTDETAIL} settings
         */
        applyState: function(settings)
        {
            this.options.showUnits = settings.showUnits;
            this.options.items = settings.items;
        },

        /**
         * Loads and then applies the saved state.
         */
        loadAndApplyState: function()
        {
            this.applyState(this.loadState());
        },

        /**
         * Returns the key against which the state is saved.
         * @returns {string}
         * @private
         */
        _persistenceKey: function()
        {
            return 'vrsAircraftDetailPlugin-' + this.options.name;
        },

        /**
         * Returns the current state.
         * @returns {VRS_STATE_AIRCRAFTDETAIL}
         * @private
         */
        _createSettings: function()
        {
            return {
                showUnits: this.options.showUnits,
                items: this.options.items
            };
        },
        //endregion

        //region -- createOptionPane
        /**
         * Returns the option panes that can be used to configure the panel.
         * @param {number} displayOrder
         * @returns {Array.<VRS.OptionPane>}
         */
        createOptionPane: function(displayOrder)
        {
            var result = [];
            var self = this;
            var state = this._getState();
            var options = this.options;

            var settingsPane = new VRS.OptionPane({
                name:           'vrsAircraftDetailPlugin_' + options.name + '_Settings',
                titleKey:       'PaneDetailSettings',
                displayOrder:   displayOrder,
                fields:         [
                    new VRS.OptionFieldCheckBox({
                        name:           'showUnits',
                        labelKey:       'ShowUnits',
                        getValue:       function() { return options.showUnits; },
                        setValue:       function(value) {
                            options.showUnits = value;
                            self._buildContent(self._getState());
                            self._reRenderAircraft(self._getState())
                        },
                        saveState:      function() { self.saveState(); }
                    })
                ]
            });
            result.push(settingsPane);

            if(VRS.globalOptions.detailPanelUserCanConfigureItems) {
                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane:       settingsPane,
                    fieldLabel: 'DetailItems',
                    surface:    VRS.RenderSurface.DetailBody,
                    getList:    function() { return options.items; },
                    setList:    function(items) {
                        options.items = items;
                        self._buildBody(state);
                        self._reRenderAircraft(state);
                    },
                    saveState:  function() { self.saveState(); }
                });
            }

            return result;
        },
        //endregion

        //region -- _buildContent, _buildHeader, _buildBody, _buildLinks
        /**
         * Fills both the header and body with content.
         * @param {VRS.AircraftDetailPluginState} state
         * @private
         */
        _buildContent: function(state)
        {
            this._buildHeader(state);
            this._buildBody(state);
            this._buildLinks(state);
        },

        /**
         * Empties the header and then prepares it with elements to render into.
         * @param {VRS.AircraftDetailPluginState} state
         * @private
         */
        _buildHeader: function(state)
        {
            var options = this.options;

            state.headerProperties = this._destroyProperties(VRS.RenderSurface.DetailHead, state.headerProperties);
            state.headerContainer.empty();

            var table = $('<table/>')
                .appendTo(state.headerContainer),

                row1 = $('<tr/>')
                    .appendTo(table),
                regElement = $('<td/>')
                    .addClass('reg')
                    .appendTo(row1),
                icaoElement = $('<td/>')
                    .addClass('icao')
                    .appendTo(row1),
                bearingElement = $('<td/>')
                    .addClass('bearing')
                    .appendTo(row1),
                opFlagElement = $('<td/>')
                    .addClass('flag')
                    .appendTo(row1),

                row2 = $('<tr/>')
                    .appendTo(table),
                operatorElement = $('<td/>')
                    .addClass('op')
                    .attr('colspan', '3')
                    .appendTo(row2),
                callsignElement = $('<td/>')
                    .addClass('callsign')
                    .appendTo(row2),

                row3 = $('<tr/>')
                    .appendTo(table),
                countryElement = $('<td/>')
                    .addClass('country')
                    .attr('colspan', '3')
                    .appendTo(row3),
                militaryElement = $('<td/>')
                    .addClass('military')
                    .appendTo(row3),

                row4 = $('<tr/>')
                    .appendTo(table),
                modelElement = $('<td/>')
                    .addClass('model')
                    .attr('colspan', '3')
                    .appendTo(row4),
                modelTypeElement = $('<td/>')
                    .addClass('modelType')
                    .appendTo(row4);

            state.headerProperties = [
                new VRS.AircraftDetailProperty(VRS.RenderProperty.Registration, true, regElement,       options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.Icao,         true, icaoElement,      options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.Bearing,      true, bearingElement,   options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.OperatorFlag, true, opFlagElement,    options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.Operator,     true, operatorElement,  options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.Callsign,     true, callsignElement,  options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.Country,      true, countryElement,   options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.CivOrMil,     true, militaryElement,  options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.Model,        true, modelElement,     options),
                new VRS.AircraftDetailProperty(VRS.RenderProperty.ModelIcao,    true, modelTypeElement, options)
            ];
        },

        /**
         * Empties the body and then prepares it with elements to render into.
         * @param {VRS.AircraftDetailPluginState} state
         * @private
         */
        _buildBody: function(state)
        {
            state.bodyProperties = this._destroyProperties(VRS.RenderSurface.DetailBody, state.bodyProperties);
            state.bodyContainer.empty();
            var options = this.options;

            var list =
                $('<ul/>')
                    .appendTo(state.bodyContainer);

            state.bodyProperties = [];
            var countProperties = options.items.length;
            for(var i = 0;i < countProperties;++i) {
                var property = options.items[i];
                var handler = VRS.renderPropertyHandlers[property];
                var suppressLabel = handler.suppressLabelCallback(VRS.RenderSurface.DetailBody);

                var listItem =
                    $('<li/>')
                        .appendTo(list);
                if(!suppressLabel) {
                    $('<div/>')
                        .addClass('label')
                        .append($('<span/>')
                            .text(VRS.globalisation.getText(handler.labelKey) + ':')
                        )
                        .appendTo(listItem);
                }
                var content =
                    $('<div/>')
                        .addClass('content')
                        .append($('<span/>'))
                        .appendTo(listItem);
                if(suppressLabel) {
                    listItem.addClass('wide');
                    content.addClass('wide');
                }
                if(handler.isMultiLine) {
                    listItem.addClass('multiline');
                    content.addClass('multiline');
                }
                state.bodyProperties.push(new VRS.AircraftDetailProperty(property, false, content, options));
            }
        },

        /**
         * Adds the UI for the aircraft links.
         * @param {VRS.AircraftDetailPluginState} state
         * @private
         */
        _buildLinks: function(state)
        {
            var options = this.options;

            if(state.aircraftLinksPlugin) state.aircraftLinksPlugin.destroy();
            if(state.noAircraftLinksPlugin) state.noAircraftLinksPlugin.destroy();
            state.linksContainer.empty();
            state.noAircraftContainer.empty();

            var aircraftLinksElement = $('<div/>')
                .appendTo(state.linksContainer)
                .vrsAircraftLinks();
            state.aircraftLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(aircraftLinksElement);

            /** @type {Array.<VRS.LinkSite|VRS.LinkRenderHandler>} */
            var routeLinks = [];

            if(options.mapPlugin && options.aircraftList && VRS.globalOptions.detailPanelShowCentreOnAircraft) {
                state.centreOnSelectedAircraftLinkRenderHandler = new VRS.CentreOnSelectedAircraftLinkRenderHandler(options.aircraftList, options.mapPlugin);
                routeLinks.push(state.centreOnSelectedAircraftLinkRenderHandler);
            }

            if(options.aircraftAutoSelect && VRS.globalOptions.detailPanelShowEnableAutoSelect) {
                state.autoSelectLinkRenderHelper = new VRS.AutoSelectLinkRenderHelper(options.aircraftAutoSelect);
                routeLinks.push(state.autoSelectLinkRenderHelper);
            }
            if(VRS.globalOptions.detailPanelShowSeparateRouteLink) routeLinks.push(VRS.LinkSite.StandingDataMaintenance);

            if(routeLinks.length > 0) {
                var routeLinksElement = $('<div/>')
                    .appendTo(state.linksContainer)
                    .vrsAircraftLinks({
                        linkSites: routeLinks
                    });
                state.routeLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(routeLinksElement);

                if(state.autoSelectLinkRenderHelper) state.autoSelectLinkRenderHelper.addLinksRendererPlugin(state.routeLinksPlugin);
            }

            if(options.aircraftAutoSelect && VRS.globalOptions.detailPanelShowEnableAutoSelect) {
                var noAircraftLinksElement = $('<div/>')
                    .appendTo(state.noAircraftContainer)
                    .vrsAircraftLinks({
                        linkSites: [ state.autoSelectLinkRenderHelper ]
                    });
                state.noAircraftLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(noAircraftLinksElement);
                state.autoSelectLinkRenderHelper.addLinksRendererPlugin(state.noAircraftLinksPlugin);
            }
        },
        //endregion

        //region -- _renderContent, _renderProperties, _renderLinks, _reRenderAircraft
        /**
         * Updates the header and body with any values on the aircraft that have changed.
         * @param {VRS.AircraftDetailPluginState}   state           The state object.
         * @param {VRS.Aircraft}                    aircraft        The aircraft being rendered. Can be null / undefined.
         * @param {bool}                            refreshAll      True if every value is to be rendered, false if only the updated values are rendered.
         * @param {VRS.DisplayUnitDependency}      [displayUnit]    If supplied then only properties that depend on this display unit are refreshed.
         * @private
         */
        _renderContent: function(state, aircraft, refreshAll, displayUnit)
        {
            state.aircraftLastRendered = aircraft;
            if(!aircraft) {
                $(state.container, ':visible').hide();
                $(state.noAircraftContainer, ':hidden').show();
                this._renderLinks(state, null, refreshAll);
            } else {
                $(state.container, ':hidden').show();
                $(state.noAircraftContainer, ':visible').hide();

                this._renderProperties(state, aircraft, refreshAll, state.headerProperties, VRS.RenderSurface.DetailHead, displayUnit);
                this._renderProperties(state, aircraft, refreshAll, state.bodyProperties, VRS.RenderSurface.DetailBody, displayUnit);
                this._renderLinks(state, aircraft, refreshAll);
            }
        },

        /**
         * Renders the values into a set of existing elements on a surface.
         * @param {VRS.AircraftDetailPluginState}       state           The state object.
         * @param {VRS.Aircraft}                        aircraft        The aircraft.
         * @param {bool}                                refreshAll      True if all fields are to be rendered, false if only the updated ones should be done.
         * @param {Array.<VRS.AircraftDetailProperty>}  properties      The list of properties to render.
         * @param {VRS.RenderSurface}                   surface         The surface being rendered.
         * @param {VRS.DisplayUnitDependency}          [displayUnit]    If supplied then only properties that depend on this display unit are refreshed.
         * @private
         */
        _renderProperties: function(state, aircraft, refreshAll, properties, surface, displayUnit)
        {
            var options = this.options;
            var countProperties = properties.length;

            var refreshedContent = false;
            for(var i = 0;i < countProperties;++i) {
                var property = properties[i];
                var handler = property.handler;
                var element = property.contentElement;

                var forceRefresh = refreshAll;
                if(!forceRefresh) forceRefresh = displayUnit && handler.usesDisplayUnit(displayUnit);

                if(forceRefresh || handler.hasChangedCallback(aircraft)) {
                    handler.renderToJQuery(element, surface, aircraft, options);
                    refreshedContent = true;
                }
                if(forceRefresh || handler.tooltipChangedCallback(aircraft)) {
                    handler.renderTooltipToJQuery(element, surface, aircraft, options);
                }
            }
        },

        /**
         * Renders the links for the aircraft.
         * @param {VRS.AircraftDetailPluginState}       state           The state object.
         * @param {VRS.Aircraft}                        aircraft        The aircraft.
         * @param {bool}                                refreshAll      True if the display should be refreshed regardless of whether it has changed.
         * @private
         */
        _renderLinks: function(state, aircraft, refreshAll)
        {
            if(aircraft === null) {
                if(state.noAircraftLinksPlugin) {
                    state.noAircraftLinksPlugin.renderForAircraft(null, refreshAll);
                }
            } else {
                if(VRS.globalOptions.detailPanelShowAircraftLinks) {
                    state.aircraftLinksPlugin.renderForAircraft(aircraft, refreshAll);
                }
                if(state.routeLinksPlugin) {
                    state.routeLinksPlugin.renderForAircraft(aircraft, refreshAll);
                }
            }
        },

        /**
         * Forces the drawing of every item in both the header and body.
         * @param {VRS.AircraftDetailPluginState} state
         * @private
         */
        _reRenderAircraft: function(state)
        {
            this._renderContent(state, state.aircraftLastRendered, true);
        },
        //endregion

        //region -- _aircraftListUpdated, _selectedAircraftChanged
        /**
         * Called when the aircraft list has been updated.
         //* @param {VRS.AircraftCollection} newAircraft
         //* @param {VRS.AircraftCollection} offRadar
         * @private
         */
        _aircraftListUpdated: function()
        {
            var state = this._getState();
            if(!state.suspended) {
                var options = this.options;
                var selectedAircraft = options.aircraftList.getSelectedAircraft();
                if(selectedAircraft) this._renderContent(state, selectedAircraft, false);
            }
        },

        /**
         * Called when the user changes the display units.
         * @param {VRS.DisplayUnitDependency} displayUnit
         * @private
         */
        _displayUnitChanged: function(displayUnit)
        {
            var state = this._getState();
            if(!state.suspended) {
                var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                if(selectedAircraft) this._renderContent(state, selectedAircraft, false, displayUnit);
            }
        },

        /**
         * Called when the selected aircraft has changed.
         //* @param {VRS.Aircraft} oldSelectedAircraft
         * @private
         */
        _selectedAircraftChanged: function()
        {
            var state = this._getState();
            if(!state.suspended) {
                var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
                this._renderContent(state, selectedAircraft, true);
            }
        },

        /**
         * Called when the language has changed.
         * @private
         */
        _localeChanged: function()
        {
            var state = this._getState();
            this._buildContent(state);
            if(!state.suspended) this._reRenderAircraft(state);
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));