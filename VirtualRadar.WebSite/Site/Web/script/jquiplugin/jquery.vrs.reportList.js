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

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};

    VRS.globalOptions.reportListClass = VRS.globalOptions.reportListClass || 'vrsAircraftList flights';             // The CSS class to attach to the report list.
    VRS.globalOptions.reportListSingleAircraftColumns = VRS.globalOptions.reportListSingleAircraftColumns || [      // The default columns to show in the list for reports where there is only a single aircraft specified by the criteria and only one aircraft in the results.
        VRS.ReportFlightProperty.RowNumber,
        VRS.ReportFlightProperty.StartTime,
        VRS.ReportFlightProperty.Duration,
        VRS.ReportFlightProperty.Callsign,
        VRS.ReportFlightProperty.RouteShort,
        VRS.ReportFlightProperty.Altitude,
        VRS.ReportFlightProperty.Speed,
        VRS.ReportFlightProperty.Squawk
    ];
    VRS.globalOptions.reportListManyAircraftColumns = VRS.globalOptions.reportListManyAircraftColumns || [          // The default columns to show in the list for reports where the criteria could match more than one aircraft.
        VRS.ReportFlightProperty.RowNumber,
        VRS.ReportFlightProperty.StartTime,
        VRS.ReportFlightProperty.Duration,
        VRS.ReportAircraftProperty.Silhouette,
        VRS.ReportAircraftProperty.OperatorFlag,
        VRS.ReportAircraftProperty.Registration,
        VRS.ReportAircraftProperty.Icao,
        VRS.ReportFlightProperty.Callsign,
        VRS.ReportFlightProperty.RouteShort,
        VRS.ReportAircraftProperty.Operator,
        VRS.ReportAircraftProperty.ModelIcao,
        VRS.ReportAircraftProperty.Model,
        VRS.ReportAircraftProperty.Species,
        VRS.ReportAircraftProperty.ModeSCountry,
        VRS.ReportAircraftProperty.Military
    ];
    VRS.globalOptions.reportListDefaultShowUnits = VRS.globalOptions.reportListDefaultShowUnits !== undefined ? VRS.globalOptions.reportListDefaultShowUnits : true;            // True if the default should be to show units.
    VRS.globalOptions.reportListDistinguishOnGround = VRS.globalOptions.reportListDistinguishOnGround !== undefined ? VRS.globalOptions.reportListDistinguishOnGround : true;   // True if aircraft on ground should be shown as an altitude of GND.
    VRS.globalOptions.reportListShowPagerTop = VRS.globalOptions.reportListShowPagerTop !== undefined ? VRS.globalOptions.reportListShowPagerTop : true;                        // True if the report list is to show a pager above the list.
    VRS.globalOptions.reportListShowPagerBottom = VRS.globalOptions.reportListShowPagerBottom !== undefined ? VRS.globalOptions.reportListShowPagerBottom : true;               // True if the report list is to show a pager below the list.
    VRS.globalOptions.reportListUserCanConfigureColumns = VRS.globalOptions.reportListUserCanConfigureColumns !== undefined ? VRS.globalOptions.reportListUserCanConfigureColumns : true;   // True if the user is allowed to configure the columns in the report list.
    VRS.globalOptions.reportListGroupBySortColumn = VRS.globalOptions.reportListGroupBySortColumn !== undefined ? VRS.globalOptions.reportListGroupBySortColumn : true;         // True if the report list should show group rows when the value of the first sort column changes.
    VRS.globalOptions.reportListGroupResetAlternateRows = VRS.globalOptions.reportListGroupResetAlternateRows !== undefined ? VRS.globalOptions.reportListGroupResetAlternateRows : false;  // True if the report list should reset the alternate row shading at the start of each group.
    //endregion

    //region ReportListPluginState
    /**
     * Holds the state associated with a report list.
     * @constructor
     */
    VRS.ReportListPluginState = function()
    {
        /**
         * The element holding the table.
         * @type {jQuery}
         */
        this.tableElement = null;

        /**
         * The element that is shown when a message is to be displayed instead of the table - e.g. when the report is empty.
         * @type {jQuery}
         */
        this.messageElement = null;

        /**
         * An associative array of flight row numbers and the table row element for that flight's row.
         * @type {Object.<number, jQuery>}
         */
        this.flightRows = {};

        /**
         * The element for the row that is currently marked as selected.
         * @type {jQuery}
         */
        this.selectedRowElement = null;

        /**
         * The element for the pager placed before the list.
         * @type {jQuery}
         */
        this.pagerTopElement = null;

        /**
         * The direct reference to the pager placed after the list, if any.
         * @type {VRS.vrsReportPager}
         */
        this.pagerTopPlugin = null;

        /**
         * The element for the pager placed after the list.
         * @type {jQuery}
         */
        this.pagerBottomElement = null;

        /**
         * The direct reference to the pager placed after the list, if any.
         * @type {VRS.vrsReportPager}
         */
        this.pagerBottomPlugin = null;

        /**
         * The hook result for the rows fetched event.
         * @type {Object}
         */
        this.rowsFetchedHookResult = null;

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
     * Returns the VRS.vrsReportList plugin attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsReportList}
     */
    VRS.jQueryUIHelper.getReportListPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsReportList'); };

    /**
     * Returns the default options for the report list widget, with optional overrides.
     * @param {VRS_OPTIONS_REPORTLIST=} overrides
     * @returns {VRS_OPTIONS_REPORTLIST}
     */
    VRS.jQueryUIHelper.getReportListOptions = function(overrides)
    {
        return $.extend({
            name:                       'default',                                                  // The name to use when saving and loading state.
            report:                     undefined,                                                  // The report whose content is going to be displayed by the list.
            unitDisplayPreferences:     undefined,                                                  // The VRS.UnitDisplayPreferences that dictate how values are to be displayed.
            singleAircraftColumns:      VRS.globalOptions.reportListSingleAircraftColumns.slice(),  // The columns to display when the report criteria only allows for a single aircraft and only 0 or 1 aircraft were returned.
            manyAircraftColumns:        VRS.globalOptions.reportListManyAircraftColumns.slice(),    // The columns to display when the report criteria allows for more than one aircraft to be returned.
            useSavedState:              true,                                                       // True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
            showUnits:                  VRS.globalOptions.reportListDefaultShowUnits,               // True if heights, distances and speeds should indicate their units.
            distinguishOnGround:        VRS.globalOptions.reportListDistinguishOnGround,            // True if altitudes should distinguish between altitudes of zero and aircraft on the ground.
            showPagerTop:               VRS.globalOptions.reportListShowPagerTop,                   // True if the pager is to be shown above the list, false if it is not.
            showPagerBottom:            VRS.globalOptions.reportListShowPagerBottom,                // True if the pager is to be shown below the list, false if it is not.
            groupBySortColumn:          VRS.globalOptions.reportListGroupBySortColumn,              // True if the report should break the report into groups on the first sort column.
            groupResetAlternateRows:    VRS.globalOptions.reportListGroupResetAlternateRows,        // True if the report should reset the alternate row shading at the start of each group.

            // These are intended for internal use only.
            justShowStartTime:          false,                                                      // True if only the start time is to be shown on start dates. Internal use only.
            alwaysShowEndDate:          false,                                                      // True if the end date should always show the date portion, even if it is the same as the start date.

            __nop: null
        }, overrides);
    }
    //endregion

    //region vrsReportList
    /**
     * A jQuery UI widget that can display the rows in a report.
     * @namespace VRS.vrsReportList
     */
    $.widget('vrs.vrsReportList', {
        //region -- options
        /** @type {VRS_OPTIONS_REPORTLIST} */
        options: VRS.jQueryUIHelper.getReportListOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Fetches the state object for the plugin or creates it if it doesn't already exist.
         * @returns {VRS.ReportListPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('reportListPluginState');
            if(result === undefined) {
                result = new VRS.ReportListPluginState();
                this.element.data('reportListPluginState', result);
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

            this.element.addClass(VRS.globalOptions.reportListClass);

            if(options.useSavedState) this.loadAndApplyState();

            VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.rowsFetchedHookResult = options.report.hookRowsFetched(this._rowsFetched, this);
            state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
        },

        /**
         * Destroys the UI created by the plugin and releases all resources held.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();

            if(state.rowsFetchedHookResult && options.report) options.report.unhook(state.rowsFetchedHookResult);
            state.rowsFetchedHookResult = null;

            if(state.selectedFlightChangedHookResult && options.report) options.report.unhook(state.selectedFlightChangedHookResult);
            state.selectedFlightChangedHookResult = null;

            if(state.localeChangedHookResult && VRS.globalisation) VRS.globalisation.unhook(state.localeChangedHookResult);
            state.localeChangedHookResult = null;

            this._destroyTable(state);

            this.element.removeClass(VRS.globalOptions.reportListClass);
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
         * @returns {VRS_STATE_REPORTLIST}
         */
        loadState: function()
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);

            result.singleAircraftColumns = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.singleAircraftColumns, [ VRS.ReportSurface.List ]);
            result.manyAircraftColumns   = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.manyAircraftColumns, [ VRS.ReportSurface.List ]);

            return result;
        },

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_REPORTLIST} settings
         */
        applyState: function(settings)
        {
            this.options.singleAircraftColumns = settings.singleAircraftColumns;
            this.options.manyAircraftColumns = settings.manyAircraftColumns;
            this.options.showUnits = settings.showUnits;
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
            return 'vrsReportList-' + this.options.report.getName() + '-' + this.options.name;
        },

        /**
         * Creates the saved state object.
         * @returns {VRS_STATE_REPORTLIST}
         */
        _createSettings: function()
        {
            return {
                singleAircraftColumns:  this.options.singleAircraftColumns,
                manyAircraftColumns:    this.options.manyAircraftColumns,
                showUnits:              this.options.showUnits
            };
        },
        //endregion

        //region -- createOptionPane
        /**
         * Returns the option pane that can be used to configure the widget via the UI.
         * @param {number} displayOrder     The relative display order of the pane.
         * @returns {VRS.OptionPane[]}
         */
        createOptionPane: function(displayOrder)
        {
            var self = this;
            var result = [];

            result.push(new VRS.OptionPane({
                name:           'commonListSettingsPane',
                titleKey:       'PaneListSettings',
                displayOrder:   displayOrder,
                fields:         [
                    new VRS.OptionFieldCheckBox({
                        name:           'showUnits',
                        labelKey:       'ShowUnits',
                        getValue:       function() { return self.options.showUnits; },
                        setValue:       function(value) { self.options.showUnits = value; self.refreshDisplay(); },
                        saveState:      function() { self.saveState(); }
                    })
                ]
            }));

            if(VRS.globalOptions.reportListUserCanConfigureColumns) {
                var singleAircraftPane = new VRS.OptionPane({
                    name:           'singleAircraftPane',
                    titleKey:       'PaneSingleAircraft',
                    displayOrder:   displayOrder + 10
                });
                result.push(singleAircraftPane);
                VRS.reportPropertyHandlerHelper.addReportPropertyListOptionsToPane({
                    pane:       singleAircraftPane,
                    fieldLabel: 'Columns',
                    surface:    VRS.ReportSurface.List,
                    getList:    function() { return self.options.singleAircraftColumns; },
                    setList:    function(cols) { self.options.singleAircraftColumns = cols; self.refreshDisplay(); },
                    saveState:  function() { self.saveState(); }
                });

                var manyAircraftPane = new VRS.OptionPane({
                    name:           'manyAircraftPane',
                    titleKey:       'PaneManyAircraft',
                    displayOrder:   displayOrder + 20
                });
                result.push(manyAircraftPane);
                VRS.reportPropertyHandlerHelper.addReportPropertyListOptionsToPane({
                    pane:       manyAircraftPane,
                    fieldLabel: 'Columns',
                    surface:    VRS.ReportSurface.List,
                    getList:    function() { return self.options.manyAircraftColumns; },
                    setList:    function(cols) { self.options.manyAircraftColumns = cols; self.refreshDisplay(); },
                    saveState:  function() { self.saveState(); }
                });
            }

            return result;
        },
        //endregion

        //region -- refreshDisplay, _buildTable, _destroyTable
        /**
         * Rebuilds the display.
         */
        refreshDisplay: function()
        {
            this._buildTable(this._getState());
        },

        /**
         * Erases the table if it currently exists and then rebuilds it in its entirety. Unlike the aircraft list speed
         * is not an issue here - the table is not being continuously updated. However, be aware that if the user asks
         * for all rows then it could be an extremely large table, so we don't want to be too tardy about it.
         * @param {VRS.ReportListPluginState} state
         * @private
         */
        _buildTable: function(state)
        {
            var options = this.options;

            this._destroyTable(state);

            if(options.report.getFlights().length === 0) {
                state.messageElement = $('<div/>')
                    .addClass('emptyReport')
                    .text(VRS.$$.ReportEmpty)
                    .appendTo(this.element);
            } else {
                var self = this;
                var createPager = function(enabled, isTop) {
                    var element = null, plugin = null;
                    if(enabled && VRS.jQueryUIHelper.getReportPagerPlugin) {
                        element = $('<div/>')
                            .addClass(isTop ? 'top' : 'bottom')
                            .vrsReportPager(VRS.jQueryUIHelper.getReportPagerOptions({
                                report: options.report
                            }));
                        plugin = VRS.jQueryUIHelper.getReportPagerPlugin(element);
                    }
                    if(isTop) {
                        state.pagerTopElement = element;
                        state.pagerTopPlugin = plugin;
                    } else {
                        state.pagerBottomElement = element;
                        state.pagerBottomPlugin = plugin;
                    }
                    return element;
                };

                var topPager = createPager(options.showPagerTop, true);
                var bottomPager = createPager(options.showPagerBottom, false);

                state.tableElement = $('<table/>')
                    .addClass('aircraftList report')
                    .appendTo(this.element);

                var columns = options.report.getCriteria().isForSingleAircraft() ? options.singleAircraftColumns : options.manyAircraftColumns;
                columns = VRS.arrayHelper.filter(columns, function(item) { return VRS.reportPropertyHandlers[item] instanceof VRS.ReportPropertyHandler; });
                this._buildHeader(state, columns, topPager);
                this._buildBody(state, columns, options.report.getFlights(), bottomPager);
            }
        },

        /**
         * Adds the pager passed across to a table.
         * @param {jQuery}  pager       The pager to add. If no pager is supplied then nothing gets added.
         * @param {jQuery}  section     The jQuery element for the table section to add the pager to (e.g. thead etc.)
         * @param {string}  cellType    The type of cell to add - e.g. <td/>.
         * @param {number}  colspan     The number of columns the cell should span.
         * @private
         */
        _addPagerToTable: function(pager, section, cellType, colspan)
        {
            if(pager) {
                var pagerRow = $('<tr/>')
                    .addClass('pagerRow')
                    .appendTo(section);
                $(cellType)
                    .attr('colspan', colspan)
                    .appendTo(pagerRow)
                    .append(pager);
            }
        },

        /**
         * Builds the table header.
         * @param {VRS.ReportListPluginState}   state
         * @param {Array.<VRS_REPORT_PROPERTY>} columns
         * @param {jQuery}                      pager
         * @private
         */
        _buildHeader: function(state, columns, pager)
        {
            var thead = $('<thead/>')
                .appendTo(state.tableElement);

            this._addPagerToTable(pager, thead, '<th/>', columns.length);

            var tr = $('<tr/>')
                .appendTo(thead);

            var length = columns.length;
            var lastCell;
            for(var i = 0;i < length;++i) {
                var property = columns[i];
                var handler = VRS.reportPropertyHandlers[property];
                var cell = lastCell = $('<th/>')
                    .text(VRS.globalisation.getText(handler.headingKey))
                    .appendTo(tr);
                this._setCellAlignment(cell, handler.headingAlignment);
                if(handler.fixedWidth) this._setFixedWidth(cell, handler.fixedWidth());
            }
            if(lastCell) lastCell.addClass('lastColumn');
        },

        /**
         * Builds the table body.
         * @param {VRS.ReportListPluginState}       state
         * @param {Array.<VRS_REPORT_PROPERTY>}     columns
         * @param {Array.<VRS_JSON_REPORT_FLIGHT>}  flights
         * @param {jQuery}                          pager
         * @private
         */
        _buildBody: function(state, columns, flights, pager)
        {
            var options = this.options;
            var self = this;

            var tbody = $('<tbody/>')
                .appendTo(state.tableElement);

            var groupColumnHandler = VRS.reportPropertyHandlerHelper.findPropertyHandlerForSortColumn(options.report.getGroupSortColumn());
            var previousGroupValue = null;
            var justShowStartTime = options.justShowStartTime;
            if(groupColumnHandler && groupColumnHandler.property === VRS.ReportFlightProperty.StartTime) options.justShowStartTime = true;

            var countColumns = columns.length;
            var countRows = flights.length;
            var isOdd = true;
            for(var rowNum = 0;rowNum < countRows;++rowNum) {
                var flight = flights[rowNum];
                var aircraft = flight.aircraft;
                var firstRow = rowNum === 0;

                if(options.groupBySortColumn && groupColumnHandler) {
                    var groupValue = groupColumnHandler.groupValue(groupColumnHandler.isAircraftProperty ? aircraft : flight);
                    if(!groupValue) groupValue = VRS.$$.None;
                    if(rowNum === 0 || groupValue !== previousGroupValue) {
                        if(options.groupResetAlternateRows) isOdd = true;
                        var groupRow = $('<tr/>')
                            .addClass('group')
                            .appendTo(tbody);
                        if(firstRow) {
                            groupRow.addClass('firstRow');
                            firstRow = false;
                        }
                        var groupCell = $('<td/>')
                            .appendTo(groupRow)
                            .attr('colspan', countColumns)
                            .text(groupValue);
                    }
                    previousGroupValue = groupValue;
                }

                var tr = $('<tr/>')
                    .addClass(isOdd ? 'vrsOdd' : 'vrsEven')
                    .appendTo(tbody)
                    .on('click', function(e) { self._rowClicked(e, this); });
                if(firstRow) tr.addClass('firstRow');

                state.flightRows[flight.row] = tr;

                var lastCell;
                for(var colNum = 0;colNum < countColumns;++colNum) {
                    var property = columns[colNum];
                    var handler = VRS.reportPropertyHandlers[property];
                    var cell = lastCell = $('<td/>')
                        .appendTo(tr);
                    this._setCellAlignment(cell, handler.headingAlignment);
                    if(handler.fixedWidth) this._setFixedWidth(cell, handler.fixedWidth(VRS.ReportSurface.List));
                    handler.renderIntoJQueryElement(cell, handler.isAircraftProperty ? aircraft : flight, options, VRS.ReportSurface.List);
                }
                if(lastCell) lastCell.addClass('lastColumn');

                isOdd = !isOdd;
            }

            this._addPagerToTable(pager, tbody, '<td/>', columns.length);

            options.justShowStartTime = justShowStartTime;
            this._markSelectedRow();
        },

        /**
         * Erases the table if it currently exists, releasing all resources allocated to it.
         * @param {VRS.ReportListPluginState} state
         * @private
         */
        _destroyTable: function(state)
        {
            if(state.messageElement) state.messageElement.remove();
            state.messageElement = null;

            for(var rowId in state.flightRows) {
                var row = state.flightRows[rowId];
                if(row instanceof jQuery) row.off();
            }
            state.flightRows = {};

            if(state.tableElement) {
                state.tableElement.remove();
                state.tableElement = null;
            }
            state.selectedRowElement = null;

            if(state.pagerTopPlugin) state.pagerTopPlugin.destroy();
            if(state.pagerTopElement) state.pagerTopElement.remove();
            state.pagerTopElement = state.pagerTopPlugin = null;

            if(state.pagerBottomPlugin) state.pagerBottomPlugin.destroy();
            if(state.pagerBottomElement) state.pagerBottomElement.remove();
            state.pagerBottomElement = state.pagerBottomPlugin = null;
        },
        //endregion

        //region -- _setCellAlignment, _setFixedWidth, _getFlightForTableRow, _markSelectedRow
        /**
         * Sets the alignment on a cell.
         * @param {jQuery}          cell
         * @param {VRS.Alignment}   alignment
         * @private
         */
        _setCellAlignment: function(cell, alignment)
        {
            if(cell && alignment) {
                switch(alignment) {
                    case VRS.Alignment.Left:    break;
                    case VRS.Alignment.Centre:  cell.addClass('vrsCentre'); break;
                    case VRS.Alignment.Right:   cell.addClass('vrsRight'); break;
                }
            }
        },

        /**
         * Sets the fixed width on a cell.
         * @param {jQuery}  cell
         * @param {string}  fixedWidth
         * @private
         */
        _setFixedWidth: function(cell, fixedWidth)
        {
            if(fixedWidth) {
                cell.attr('width', fixedWidth);
                cell.addClass('fixedWidth');
            }
        },

        /**
         * Returns the flight corresponding to the row clicked.
         * @param {jQuery|HTMLTableRowElement} row
         * @returns {VRS_JSON_REPORT_FLIGHT}
         * @private
         */
        _getFlightForTableRow: function(row)
        {
            /** @type {VRS_JSON_REPORT_FLIGHT} */ var result = null;
            var state = this._getState();
            var options = this.options;
            var flights = options.report.getFlights();

            if(flights.length) {
                if(!(row instanceof jQuery)) row = $(row);
                for(var rowId in state.flightRows) {
                    var flightRow = state.flightRows[rowId];
                    if(flightRow instanceof jQuery && flightRow.is(row)) {
                        var length = flights.length;
                        for(var i = 0;i < length;++i) {
                            var flight = flights[i];
                            if(flight.row == rowId) {
                                result = flight;
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            return result;
        },

        /**
         * Ensures that the selected row, and only the selected row, has a class on it to indicate that it is selected.
         * @private
         */
        _markSelectedRow: function()
        {
            var state = this._getState();

            if(state.selectedRowElement) state.selectedRowElement.removeClass('vrsSelected');

            var selectedFlight = this.options.report.getSelectedFlight();
            if(!selectedFlight) state.selectedRowElement = null;
            else {
                var selectedFlightRowId = selectedFlight ? selectedFlight.row : null;
                var selectedRow = state.flightRows[selectedFlightRowId];
                if(selectedRow) selectedRow.addClass('vrsSelected');
                state.selectedRowElement = selectedRow;
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
         * Called when the user clicks a row in the body of the list.
         * @param {Event}               event
         * @param {HTMLTableRowElement} target
         * @private
         */
        _rowClicked: function(event, target)
        {
            var flight = this._getFlightForTableRow(/** @type {HTMLTableRowElement} */ target);
            this.options.report.setSelectedFlight(flight);
        },

        /**
         * Called when the report has successfully fetched some data to display.
         * @private
         */
        _rowsFetched: function()
        {
            var state = this._getState();
            this._buildTable(state);
        },

        /**
         * Called when something changes the selected flight on the report.
         * @private
         */
        _selectedFlightChanged: function()
        {
            this._markSelectedRow();
        },
        //endregion

        __nop: null
    });
    //endregion
})(window.VRS = window.VRS || {}, jQuery);
