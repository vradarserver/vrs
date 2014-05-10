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
 * @fileoverview A jQueryUI plugin that can display a list of flight/aircraft data for a report.
 */

(function(VRS, $, /** Object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};

    VRS.globalOptions.reportDetailClass = VRS.globalOptions.reportDetailClass || 'vrsAircraftDetail aircraft';      // The CSS class to attach to the report detail panel.
    VRS.globalOptions.reportDetailColumns = VRS.globalOptions.reportDetailColumns || [                              // The default columns to show in the detail panel.
        VRS.ReportAircraftProperty.Country,
        VRS.ReportAircraftProperty.Engines,
        VRS.ReportAircraftProperty.WakeTurbulenceCategory,
        VRS.ReportAircraftProperty.Species,
        VRS.ReportAircraftProperty.SerialNumber,
        VRS.ReportAircraftProperty.OperatorIcao,
        VRS.ReportAircraftProperty.AircraftClass,
        VRS.ReportAircraftProperty.CofACategory,
        VRS.ReportAircraftProperty.CofAExpiry,
        VRS.ReportAircraftProperty.CurrentRegDate,
        VRS.ReportAircraftProperty.DeRegDate,
        VRS.ReportAircraftProperty.FirstRegDate,
        VRS.ReportAircraftProperty.GenericName,
        VRS.ReportAircraftProperty.Interesting,
        VRS.ReportAircraftProperty.Manufacturer,
        VRS.ReportAircraftProperty.MTOW,
        VRS.ReportAircraftProperty.Notes,
        VRS.ReportAircraftProperty.OwnershipStatus,
        VRS.ReportAircraftProperty.PopularName,
        VRS.ReportAircraftProperty.PreviousId,
        VRS.ReportAircraftProperty.Status,
        VRS.ReportAircraftProperty.TotalHours,
        VRS.ReportAircraftProperty.YearBuilt,
        VRS.ReportFlightProperty.RouteFull,
        VRS.ReportAircraftProperty.Picture,
        VRS.ReportFlightProperty.StartTime,
        VRS.ReportFlightProperty.EndTime,
        VRS.ReportFlightProperty.Duration,
        VRS.ReportFlightProperty.Altitude,
        VRS.ReportFlightProperty.FlightLevel,
        VRS.ReportFlightProperty.Speed,
        VRS.ReportFlightProperty.Squawk,
        VRS.ReportFlightProperty.HadEmergency,
        VRS.ReportFlightProperty.HadAlert,
        VRS.ReportFlightProperty.HadSPI,
        VRS.ReportFlightProperty.Heading,
        VRS.ReportFlightProperty.CountModeS,
        VRS.ReportFlightProperty.CountAdsb,
        VRS.ReportFlightProperty.CountPositions
    ];
    VRS.globalOptions.reportDetailAddMapToDefaultColumns = VRS.globalOptions.reportDetailAddMapToDefaultColumns !== undefined ? VRS.globalOptions.reportDetailAddMapToDefaultColumns : VRS.globalOptions.isMobile;  // True if the map should be added to the default columns
    if(VRS.globalOptions.reportDetailAddMapToDefaultColumns) {
        if(VRS.arrayHelper.indexOf(VRS.globalOptions.reportDetailColumns, VRS.ReportFlightProperty.PositionsOnMap) === -1) {
            VRS.globalOptions.reportDetailColumns.push(VRS.ReportFlightProperty.PositionsOnMap);
        }
    }
    VRS.globalOptions.reportDetailDefaultShowUnits = VRS.globalOptions.reportDetailDefaultShowUnits !== undefined ? VRS.globalOptions.reportDetailDefaultShowUnits : true;          // True if the detail panel should show units by default.
    VRS.globalOptions.reportDetailDistinguishOnGround = VRS.globalOptions.reportDetailDistinguishOnGround !== undefined ? VRS.globalOptions.reportDetailDistinguishOnGround : true; // True if the detail panel should show GND for aircraft that are on the ground.
    VRS.globalOptions.reportDetailUserCanConfigureColumns = VRS.globalOptions.reportDetailUserCanConfigureColumns !== undefined ? VRS.globalOptions.reportDetailUserCanConfigureColumns : true; // True if the user is allowed to configure which values are shown in the detail panel.
    VRS.globalOptions.reportDetailDefaultShowEmptyValues = VRS.globalOptions.reportDetailDefaultShowEmptyValues !== undefined ? VRS.globalOptions.reportDetailDefaultShowEmptyValues : false;   // True if empty values are to be shown.
    //endregion

    //region ReportDetailPluginState
    /**
     * Holds the state associated with a report detail panel.
     * @constructor
     */
    VRS.ReportDetailPluginState = function()
    {
        /**
         * True if the control has been suspended because it is no longer in view, false if it's active.
         * @type {boolean}
         */
        this.suspended = false;

        /**
         * The element that all of the content is rendered into.
         * @type {jQuery}
         */
        this.containerElement = null;

        /**
         * The element that the header is rendered into.
         * @type {jQuery}
         */
        this.headerElement = null;

        /**
         * The element that the body is rendered into.
         * @type {jQuery}
         */
        this.bodyElement = null;

        /**
         * The flight whose details are currently on display.
         * @type {VRS_JSON_REPORT_FLIGHT}
         */
        this.displayedFlight = null;

        /**
         * An associative array of VRS_REPORT_PROPERTY properties against the jQuery element created for their display
         * in the body.
         * @type {Object.<VRS_REPORT_PROPERTY, jQuery>}
         */
        this.bodyPropertyElements = {};

        /**
         * The hook result for the selected flight changed event.
         * @type {Object}
         */
        this.selectedFlightChangedHookResult = null;

        /**
         * The hook result for the locale changed event.
         * @type {Object}
         */
        this.localeChangedHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsReportDetail plugin attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsReportDetail}
     */
    VRS.jQueryUIHelper.getReportDetailPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsReportDetail'); };

    /**
     * Returns the default options for a report detail panel widget, with optional overrides.
     * @param {VRS_OPTIONS_REPORTDETAIL=} overrides
     * @returns {VRS_OPTIONS_REPORTDETAIL}
     */
    VRS.jQueryUIHelper.getReportDetailOptions = function(overrides)
    {
        return $.extend({
            name:                   'default',                                              // The name to use when saving and loading state.
            report:                 undefined,                                              // The report whose selected flight is going to be displayed in the panel.
            unitDisplayPreferences: undefined,                                              // The VRS.UnitDisplayPreferences that dictate how values are to be displayed.
            plotterOptions:         undefined,                                              // The plotter options to use with map widgets.
            columns:                VRS.globalOptions.reportDetailColumns.slice(),          // The columns to display.
            useSavedState:          true,                                                   // True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
            showUnits:              VRS.globalOptions.reportDetailDefaultShowUnits,         // True if heights, distances and speeds should indicate their units.
            showEmptyValues:        VRS.globalOptions.reportDetailDefaultShowEmptyValues,   // True if empty values are to be shown.
            distinguishOnGround:    VRS.globalOptions.reportDetailDistinguishOnGround,      // True if altitudes should distinguish between altitudes of zero and aircraft on the ground.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsReportDetailPanel
    /**
     * A jQuery UI widget that can display the detail for a single flight.
     * @namespace VRS.vrsReportDetailPanel
     */
    $.widget('vrs.vrsReportDetail', {
        //region -- options
        /** @type {VRS_OPTIONS_REPORTDETAIL} */
        options: VRS.jQueryUIHelper.getReportDetailOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Fetches the state object for the plugin or creates it if it doesn't already exist.
         * @returns {VRS.ReportDetailPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('reportDetailPanelState');
            if(result === undefined) {
                result = new VRS.ReportDetailPluginState();
                this.element.data('reportDetailPanelState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the plugin.
         * @private
         */
        _create: function()
        {
            var state = this._getState();
            var options = this.options;

            if(options.useSavedState) this.loadAndApplyState();
            this._displayFlightDetails(state, options.report.getSelectedFlight());

            VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
        },

        /**
         * Releases the resources allocated to the plugin and reverses the UI changes wrought by _create.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            var options = this.options;

            if(state.selectedFlightChangedHookResult && options.report) options.report.unhook(state.selectedFlightChangedHookResult);
            state.selectedFlightChangedHookResult = null;

            if(state.localeChangedHookResult && VRS.globalisation) VRS.globalisation.unhook(state.localeChangedHookResult);
            state.localeChangedHookResult = null;

            state.displayedFlight = null;

            this._destroyDisplay(state);
        },
        //endregion

        //region -- saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        saveState: function()
        {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        },

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         * @returns {VRS_STATE_REPORTDETAILPANEL}
         */
        loadState: function()
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.columns = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.columns, [ VRS.ReportSurface.DetailBody ]);

            return result;
        },

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_REPORTDETAILPANEL} settings
         */
        applyState: function(settings)
        {
            this.options.columns = settings.columns;
            this.options.showUnits = settings.showUnits;
            this.options.showEmptyValues = settings.showEmptyValues;
        },

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        loadAndApplyState: function()
        {
            this.applyState(this.loadState());
        },

        /**
         * Returns the key under which the state will be saved.
         * @returns {string}
         */
        _persistenceKey: function()
        {
            return 'vrsReportDetailPanel-' + this.options.report.getName() + '-' + this.options.name;
        },

        /**
         * Creates the saved state object.
         * @returns {VRS_STATE_REPORTDETAILPANEL}
         */
        _createSettings: function()
        {
            return {
                columns:            this.options.columns,
                showUnits:          this.options.showUnits,
                showEmptyValues:    this.options.showEmptyValues
            };
        },
        //endregion

        //region -- createOptionPane
        /**
         * Returns the option pane that can be used to configure the widget via the UI.
         * @param {number} displayOrder     The relative display order of the pane.
         * @returns {VRS.OptionPane}
         */
        createOptionPane: function(displayOrder)
        {
            var self = this;

            var result = new VRS.OptionPane({
                name:           'vrsReportDetailPane-' + this.options.name + '_Settings',
                titleKey:       'DetailPanel',
                displayOrder:   displayOrder,
                fields:         [
                    new VRS.OptionFieldCheckBox({
                        name:           'showUnits',
                        labelKey:       'ShowUnits',
                        getValue:       function() { return self.options.showUnits; },
                        setValue:       function(value) { self.options.showUnits = value; self.refreshDisplay(); },
                        saveState:      function() { self.saveState(); }
                    }),
                    new VRS.OptionFieldCheckBox({
                        name:           'showEmptyValues',
                        labelKey:       'ShowEmptyValues',
                        getValue:       function() { return self.options.showEmptyValues; },
                        setValue:       function(value) { self.options.showEmptyValues = value; self.refreshDisplay(); },
                        saveState:      function() { self.saveState(); }
                    })
                ]
            });

            if(VRS.globalOptions.reportDetailUserCanConfigureColumns) {
                VRS.reportPropertyHandlerHelper.addReportPropertyListOptionsToPane({
                    pane:       result,
                    fieldLabel: 'Columns',
                    surface:    VRS.ReportSurface.DetailBody,
                    getList:    function() { return self.options.columns; },
                    setList:    function(cols) { self.options.columns = cols; self.refreshDisplay(); },
                    saveState:  function() { self.saveState(); }
                });
            }

            return result;
        },
        //endregion

        //region -- suspend
        /**
         * Suspends or actives the control.
         * @param {boolean}     onOff   True if the control is to be suspended, false if it is to be activated.
         */
        suspend: function(onOff)
        {
            onOff = !!onOff;

            var state = this._getState();
            if(state.suspended !== onOff) {
                state.suspended = onOff;
                if(!state.suspended) this.refreshDisplay();
            }
        },
        //endregion

        //region -- _displayFlightDetails, refreshDisplay, _destroyDisplay
        /**
         * Displays the details for the flight passed across.
         * @param {VRS.ReportDetailPluginState} state
         * @param {VRS_JSON_REPORT_FLIGHT}      flight
         * @private
         */
        _displayFlightDetails: function(state, flight)
        {
            state.displayedFlight = flight;
            if(!state.suspended) {
                this._destroyDisplay(state);

                if(flight) {
                    state.containerElement = $('<div/>')
                        .addClass(VRS.globalOptions.reportDetailClass)
                        .appendTo(this.element);
                    this._createHeader(state, flight);
                    this._createBody(state, flight);
                }
            }
        },

        /**
         * Redraws the displayed flight.
         * @private
         */
        refreshDisplay: function()
        {
            var state = this._getState();
            this._displayFlightDetails(state, state.displayedFlight);
        },

        /**
         * Destroys the UI associated with the display.
         * @param {VRS.ReportDetailPluginState} state
         * @private
         */
        _destroyDisplay: function(state)
        {
            for(var propertyName in state.bodyPropertyElements) {
                var handler = VRS.reportPropertyHandlers[propertyName];
                var element = state.bodyPropertyElements[propertyName];
                if(handler && element) handler.destroyWidgetInJQueryElement(element, VRS.ReportSurface.DetailBody);
            }
            state.bodyPropertyElements = {};

            if(state.bodyElement) state.bodyElement.remove();
            state.bodyElement = null;

            if(state.headerElement) state.headerElement.remove();
            state.headerElement = null;

            if(state.containerElement) state.containerElement.remove();
            state.containerElement = null;
        },

        /**
         * Creates and populates the header showing the important details about the flight. This is not configurable.
         * @param {VRS.ReportDetailPluginState} state
         * @param {VRS_JSON_REPORT_FLIGHT}      flight
         * @private
         */
        _createHeader: function(state, flight)
        {
            state.headerElement = $('<div/>')
                .addClass('header')
                .appendTo(state.containerElement);

            var table = $('<table/>')
                .appendTo(state.headerElement);

            var row1 = $('<tr/>').appendTo(table);
            this._addHeaderCell(state, row1, 1, flight, VRS.ReportAircraftProperty.Registration, 'reg');
            this._addHeaderCell(state, row1, 1, flight, VRS.ReportAircraftProperty.Icao, 'icao');
            this._addHeaderCell(state, row1, 1, flight, VRS.ReportAircraftProperty.OperatorFlag, 'flag');

            var row2 = $('<tr/>').appendTo(table);
            this._addHeaderCell(state, row2, 2, flight, VRS.ReportAircraftProperty.Operator, 'op');
            this._addHeaderCell(state, row2, 1, flight, VRS.ReportFlightProperty.Callsign, 'callsign');

            var row3 = $('<tr/>').appendTo(table);
            this._addHeaderCell(state, row3, 2, flight, VRS.ReportAircraftProperty.ModeSCountry, 'country');
            this._addHeaderCell(state, row3, 1, flight, VRS.ReportAircraftProperty.Military, 'military');

            var row4 = $('<tr/>').appendTo(table);
            this._addHeaderCell(state, row4, 2, flight, VRS.ReportAircraftProperty.Model, 'model');
            this._addHeaderCell(state, row4, 1, flight, VRS.ReportAircraftProperty.ModelIcao, 'modelType');
        },

        /**
         * Adds a cell to a row in the header table.
         * @param {VRS.ReportDetailPluginState} state
         * @param {jQuery}                      row
         * @param {number}                      colspan
         * @param {VRS_JSON_REPORT_FLIGHT}      flight
         * @param {VRS_REPORT_PROPERTY}         property
         * @param {string}                      classes
         * @private
         */
        _addHeaderCell: function(state, row, colspan, flight, property, classes)
        {
            var cell = $('<td/>')
                .addClass(classes)
                .appendTo(row);
            if(colspan > 1) cell.attr('colspan', colspan);

            var handler = VRS.reportPropertyHandlers[property];
            if(!handler) throw 'Cannot find the handler for the ' + property + ' property';
            handler.renderIntoJQueryElement(cell, handler.isAircraftProperty ? flight.aircraft : flight, this.options, VRS.ReportSurface.DetailHead);
        },

        /**
         * Creates and populates the body of the detail panel.
         * @param {VRS.ReportDetailPluginState} state
         * @param {VRS_JSON_REPORT_FLIGHT}      flight
         * @private
         */
        _createBody: function(state, flight)
        {
            var options = this.options;
            var columns = options.columns;

            state.bodyElement = $('<div/>')
                .addClass('body')
                .appendTo(state.containerElement);

            var list = $('<ul/>')
                .appendTo(state.bodyElement);

            var length = columns.length;
            for(var i = 0;i < length;++i) {
                var property = columns[i];
                var handler = VRS.reportPropertyHandlers[property];
                if(handler && (options.showEmptyValues || handler.hasValue(flight))) {
                    var suppressLabel = handler.suppressLabelCallback(VRS.ReportSurface.DetailBody);
                    var listItem = $('<li/>')
                        .appendTo(list);
                    if(!suppressLabel) {
                        $('<div/>')
                            .addClass('label')
                            .append($('<span/>')
                                .text(VRS.globalisation.getText(handler.labelKey) + ':')
                            )
                            .appendTo(listItem);
                    }
                    var contentContainer = $('<div/>')
                        .addClass('content')
                        .appendTo(listItem);
                    var content = $('<span/>')
                        .appendTo(contentContainer);
                    if(suppressLabel) {
                        listItem.addClass('wide');
                        contentContainer.addClass('wide');
                    }
                    if(handler.isMultiLine) {
                        listItem.addClass('multiline');
                        contentContainer.addClass('multiline');
                    }

                    state.bodyPropertyElements[property] = content;
                    var json = handler.isAircraftProperty ? flight.aircraft : flight;
                    handler.createWidgetInJQueryElement(content, options, VRS.ReportSurface.DetailBody);
                    handler.renderIntoJQueryElement(content, json, options, VRS.ReportSurface.DetailBody);
                }
            }
        },
        //endregion

        //region -- Events subscribed
        /**
         * Called when the user chooses another language.
         * @private
         */
        _localeChanged: function()
        {
            this.refreshDisplay();
        },

        /**
         * Called when the selected flight changes.
         * @private
         */
        _selectedFlightChanged: function()
        {
            var selectedFlight = this.options.report.getSelectedFlight();
            this._displayFlightDetails(this._getState(), selectedFlight);
        },
        //endregion

        __nop: null
    });
    //endregion
})(window.VRS = window.VRS || {}, jQuery);
