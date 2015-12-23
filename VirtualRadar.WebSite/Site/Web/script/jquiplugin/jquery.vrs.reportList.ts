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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
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

    /**
     * The options for the ReportListPlugin
     */
    export interface ReportListPlugin_Options extends ReportRender_Options
    {
        /**
         * The name to use when saving and loading state.
         */
        name?: string;

        /**
         * The report whose content is going to be displayed by the list.
         */
        report: Report;

        /**
         * The VRS.UnitDisplayPreferences that dictate how values are to be displayed.
         */
        unitDisplayPreferences: UnitDisplayPreferences

        /**
         * The columns to display when the report criteria only allows for a single aircraft and only 0 or 1 aircraft were returned.
         */
        singleAircraftColumns?: ReportAircraftPropertyEnum[];

        /**
         * The columns to display when the report criteria allows for more than one aircraft to be returned.
         */
        manyAircraftColumns?: ReportAircraftOrFlightPropertyEnum[];

        /**
         * True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
         */
        useSavedState?: boolean;

        /**
         * True if heights, distances and speeds should indicate their units.
         */
        showUnits?: boolean;

        /**
         * True if altitudes should distinguish between altitudes of zero and aircraft on the ground.
         */
        distinguishOnGround?: boolean;

        /**
         * True if the pager is to be shown above the list, false if it is not.
         */
        showPagerTop?: boolean;

        /**
         * True if the pager is to be shown below the list, false if it is not.
         */
        showPagerBottom?: boolean;

        /**
         * True if the report should break the report into groups on the first sort column.
         */
        groupBySortColumn?: boolean;

        /**
         * True if the report should reset the alternate row shading at the start of each group.
         */
        groupResetAlternateRows?: boolean;

        /**
         * FOR INTERNAL USE ONLY. True if only the start time is to be shown on start dates.
         */
        justShowStartTime?: boolean;

        /**
         * FOR INTERNAL USE ONLY. True if the end date should always show the date portion, even if it is the same as the start date.
         */
        alwaysShowEndDate?: boolean;
    }

    /**
     * The state for ReportListPlugin.
     */
    class ReportListPlugin_State
    {
        /**
         * The element holding the table.
         */
        tableElement: JQuery = null;

        /**
         * The element that is shown when a message is to be displayed instead of the table - e.g. when the report is empty.
         */
        messageElement: JQuery = null;

        /**
         * An associative array of flight row numbers and the table row element for that flight's row.
         */
        flightRows: { [index: number]: JQuery } = {};

        /**
         * The element for the row that is currently marked as selected.
         */
        selectedRowElement: JQuery = null;

        /**
         * The element for the pager placed before the list.
         */
        pagerTopElement: JQuery = null;

        /**
         * The direct reference to the pager placed after the list, if any.
         */
        pagerTopPlugin: ReportPagerPlugin = null;

        /**
         * The element for the pager placed after the list.
         */
        pagerBottomElement: JQuery = null;

        /**
         * The direct reference to the pager placed after the list, if any.
         */
        pagerBottomPlugin: ReportPagerPlugin = null;

        /**
         * The hook result for the rows fetched event.
         */
        rowsFetchedHookResult: IEventHandle = null;

        /**
         * The hook result for the selected flight changed event.
         */
        selectedFlightChangedHookResult: IEventHandle = null;

        /**
         * The hook result for the locale changed event.
         */
        localeChangedHookResult: IEventHandle = null;
    }

    /**
     * The settings saved by the ReportListPlugin between sessions.
     */
    export interface ReportListPlugin_SaveState
    {
        singleAircraftColumns: ReportAircraftPropertyEnum[];
        manyAircraftColumns:   ReportAircraftOrFlightPropertyEnum[];
        showUnits:            boolean;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getReportListPlugin = function(jQueryElement: JQuery) : ReportListPlugin
    {
        return jQueryElement.data('vrsVrsReportList');
    }
    VRS.jQueryUIHelper.getReportListOptions = function(overrides?: ReportListPlugin_Options) : ReportListPlugin_Options
    {
        return $.extend({
            name:                       'default',
            singleAircraftColumns:      VRS.globalOptions.reportListSingleAircraftColumns.slice(),
            manyAircraftColumns:        VRS.globalOptions.reportListManyAircraftColumns.slice(),
            useSavedState:              true,
            showUnits:                  VRS.globalOptions.reportListDefaultShowUnits,
            distinguishOnGround:        VRS.globalOptions.reportListDistinguishOnGround,
            showPagerTop:               VRS.globalOptions.reportListShowPagerTop,
            showPagerBottom:            VRS.globalOptions.reportListShowPagerBottom,
            groupBySortColumn:          VRS.globalOptions.reportListGroupBySortColumn,
            groupResetAlternateRows:    VRS.globalOptions.reportListGroupResetAlternateRows,

            // These are intended for internal use only.
            justShowStartTime:          false,
            alwaysShowEndDate:          false
        }, overrides);
    }

    /**
     * A jQuery UI widget that can display the rows in a report.
     */
    export class ReportListPlugin extends JQueryUICustomWidget implements ISelfPersist<ReportListPlugin_SaveState>
    {
        options: ReportListPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getReportListOptions();
        }

        private _getState() : ReportListPlugin_State
        {
            var result = this.element.data('reportListPluginState');
            if(result === undefined) {
                result = new ReportListPlugin_State();
                this.element.data('reportListPluginState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.reportListClass);

            if(options.useSavedState) {
                this.loadAndApplyState();
            }

            VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.rowsFetchedHookResult = options.report.hookRowsFetched(this._rowsFetched, this);
            state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
        }

        private _destroy()
        {
            var state = this._getState();
            var options = this.options;

            if(state.rowsFetchedHookResult && options.report) {
                options.report.unhook(state.rowsFetchedHookResult);
            }
            state.rowsFetchedHookResult = null;

            if(state.selectedFlightChangedHookResult && options.report) {
                options.report.unhook(state.selectedFlightChangedHookResult);
            }
            state.selectedFlightChangedHookResult = null;

            if(state.localeChangedHookResult && VRS.globalisation) {
                VRS.globalisation.unhook(state.localeChangedHookResult);
            }
            state.localeChangedHookResult = null;

            this._destroyTable(state);

            this.element.removeClass(VRS.globalOptions.reportListClass);
        }

        /**
         * Saves the current state of the object.
         */
        saveState()
        {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        }

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         */
        loadState() : ReportListPlugin_SaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);

            result.singleAircraftColumns = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.singleAircraftColumns, [ VRS.ReportSurface.List ]);
            result.manyAircraftColumns   = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.manyAircraftColumns, [ VRS.ReportSurface.List ]);

            return result;
        }

        /**
         * Applies a previously saved state to the object.
         */
        applyState(settings: ReportListPlugin_SaveState)
        {
            this.options.singleAircraftColumns = settings.singleAircraftColumns;
            this.options.manyAircraftColumns = settings.manyAircraftColumns;
            this.options.showUnits = settings.showUnits;
        }

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the state will be saved.
         */
        private _persistenceKey() : string
        {
            return 'vrsReportList-' + this.options.report.getName() + '-' + this.options.name;
        }

        /**
         * Creates the saved state object.
         */
        private _createSettings() : ReportListPlugin_SaveState
        {
            return {
                singleAircraftColumns:  this.options.singleAircraftColumns,
                manyAircraftColumns:    this.options.manyAircraftColumns,
                showUnits:              this.options.showUnits
            };
        }

        /**
         * Returns the option pane that can be used to configure the widget via the UI.
         */
        createOptionPane(displayOrder: number) : OptionPane[]
        {
            var result: OptionPane[] = [];

            result.push(new VRS.OptionPane({
                name:           'commonListSettingsPane',
                titleKey:       'PaneListSettings',
                displayOrder:   displayOrder,
                fields:         [
                    new VRS.OptionFieldCheckBox({
                        name:           'showUnits',
                        labelKey:       'ShowUnits',
                        getValue:       () => this.options.showUnits,
                        setValue:       (value) => {
                            this.options.showUnits = value;
                            this.refreshDisplay();
                        },
                        saveState:      () => this.saveState()
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
                    getList:    () => this.options.singleAircraftColumns,
                    setList:    (cols) => {
                        this.options.singleAircraftColumns = cols;
                        this.refreshDisplay();
                    },
                    saveState:  () => this.saveState()
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
                    getList:    () => this.options.manyAircraftColumns,
                    setList:    (cols) => {
                        this.options.manyAircraftColumns = cols;
                        this.refreshDisplay();
                    },
                    saveState:  () => this.saveState()
                });
            }

            return result;
        }

        /**
         * Rebuilds the display.
         */
        refreshDisplay()
        {
            this._buildTable(this._getState());
        }

        /**
         * Erases the table if it currently exists and then rebuilds it in its entirety. Unlike the aircraft list speed
         * is not an issue here - the table is not being continuously updated. However, be aware that if the user asks
         * for all rows then it could be an extremely large table, so we don't want to be too tardy about it.
         */
        private _buildTable(state: ReportListPlugin_State)
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
        }

        /**
         * Adds the pager passed across to a table.
         */
        private _addPagerToTable(pager: JQuery, section: JQuery, cellType: string, colspan: number)
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
        }

        /**
         * Builds the table header.
         */
        private _buildHeader(state: ReportListPlugin_State, columns: ReportAircraftOrFlightPropertyEnum[], pager: JQuery)
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
            if(lastCell) {
                lastCell.addClass('lastColumn');
            }
        }

        /**
         * Builds the table body.
         */
        private _buildBody(state: ReportListPlugin_State, columns: ReportAircraftOrFlightPropertyEnum[], flights: IReportFlight[], pager: JQuery)
        {
            var options = this.options;
            var self = this;

            var tbody = $('<tbody/>')
                .appendTo(state.tableElement);

            var groupColumnHandler = VRS.reportPropertyHandlerHelper.findPropertyHandlerForSortColumn(options.report.getGroupSortColumn());
            var previousGroupValue = null;
            var justShowStartTime = options.justShowStartTime;
            if(groupColumnHandler && groupColumnHandler.property === VRS.ReportFlightProperty.StartTime) {
                options.justShowStartTime = true;
            }

            var countColumns = columns.length;
            var countRows = flights.length;
            var isOdd = true;
            for(var rowNum = 0;rowNum < countRows;++rowNum) {
                var flight = flights[rowNum];
                var aircraft = flight.aircraft;
                var firstRow = rowNum === 0;

                if(options.groupBySortColumn && groupColumnHandler) {
                    var groupValue = groupColumnHandler.groupValue(groupColumnHandler.isAircraftProperty ? aircraft : flight);
                    if(!groupValue) {
                        groupValue = VRS.$$.None;
                    }
                    if(rowNum === 0 || groupValue !== previousGroupValue) {
                        if(options.groupResetAlternateRows) {
                            isOdd = true;
                        }
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
                if(firstRow) {
                    tr.addClass('firstRow');
                }

                state.flightRows[flight.row] = tr;

                var lastCell;
                for(var colNum = 0;colNum < countColumns;++colNum) {
                    var property = columns[colNum];
                    var handler = VRS.reportPropertyHandlers[property];
                    var cell = lastCell = $('<td/>')
                        .appendTo(tr);
                    this._setCellAlignment(cell, handler.headingAlignment);
                    if(handler.fixedWidth) {
                        this._setFixedWidth(cell, handler.fixedWidth(VRS.ReportSurface.List));
                    }
                    handler.renderIntoJQueryElement(cell, handler.isAircraftProperty ? aircraft : flight, options, VRS.ReportSurface.List);
                }
                if(lastCell) {
                    lastCell.addClass('lastColumn');
                }

                isOdd = !isOdd;
            }

            this._addPagerToTable(pager, tbody, '<td/>', columns.length);

            options.justShowStartTime = justShowStartTime;
            this._markSelectedRow();
        }

        /**
         * Erases the table if it currently exists, releasing all resources allocated to it.
         */
        private _destroyTable(state: ReportListPlugin_State)
        {
            if(state.messageElement) {
                state.messageElement.remove();
            }
            state.messageElement = null;

            for(var rowId in state.flightRows) {
                var row = state.flightRows[rowId];
                if(row instanceof jQuery) {
                    row.off();
                }
            }
            state.flightRows = {};

            if(state.tableElement) {
                state.tableElement.remove();
                state.tableElement = null;
            }
            state.selectedRowElement = null;

            if(state.pagerTopPlugin) {
                state.pagerTopPlugin.destroy();
            }
            if(state.pagerTopElement) {
                state.pagerTopElement.remove();
            }
            state.pagerTopElement = state.pagerTopPlugin = null;

            if(state.pagerBottomPlugin) {
                state.pagerBottomPlugin.destroy();
            }
            if(state.pagerBottomElement) {
                state.pagerBottomElement.remove();
            }
            state.pagerBottomElement = state.pagerBottomPlugin = null;
        }

        /**
         * Sets the alignment on a cell.
         */
        private _setCellAlignment(cell: JQuery, alignment: AlignmentEnum)
        {
            if(cell && alignment) {
                switch(alignment) {
                    case VRS.Alignment.Left:    break;
                    case VRS.Alignment.Centre:  cell.addClass('vrsCentre'); break;
                    case VRS.Alignment.Right:   cell.addClass('vrsRight'); break;
                }
            }
        }

        /**
         * Sets the fixed width on a cell.
         */
        private _setFixedWidth(cell: JQuery, fixedWidth: string)
        {
            if(fixedWidth) {
                cell.attr('width', fixedWidth);
                cell.addClass('fixedWidth');
            }
        }

        /**
         * Returns the flight corresponding to the row clicked.
         */
        private _getFlightForTableRow(row: JQuery | HTMLTableRowElement) : IReportFlight
        {
            var result: IReportFlight = null;
            var state = this._getState();
            var options = this.options;
            var flights = options.report.getFlights();

            if(flights.length) {
                if(!(row instanceof jQuery)) {
                    row = $(row);
                }
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
        }

        /**
         * Ensures that the selected row, and only the selected row, has a class on it to indicate that it is selected.
         */
        private _markSelectedRow()
        {
            var state = this._getState();

            if(state.selectedRowElement) {
                state.selectedRowElement.removeClass('vrsSelected');
            }

            var selectedFlight = this.options.report.getSelectedFlight();
            if(!selectedFlight) {
                state.selectedRowElement = null;
            } else {
                var selectedFlightRowId = selectedFlight ? selectedFlight.row : null;
                var selectedRow = state.flightRows[selectedFlightRowId];
                if(selectedRow) selectedRow.addClass('vrsSelected');
                state.selectedRowElement = selectedRow;
            }
        }

        /**
         * Called when the user chooses another language.
         */
        private _localeChanged()
        {
            this.refreshDisplay();
        }

        /**
         * Called when the user clicks a row in the body of the list.
         */
        private _rowClicked(event: Event, target: HTMLTableRowElement)
        {
            var flight = this._getFlightForTableRow(target);
            this.options.report.setSelectedFlight(flight);
        }

        /**
         * Called when the report has successfully fetched some data to display.
         */
        private _rowsFetched()
        {
            var state = this._getState();
            this._buildTable(state);
        }

        /**
         * Called when something changes the selected flight on the report.
         */
        private _selectedFlightChanged()
        {
            this._markSelectedRow();
        }
    }

    $.widget('vrs.vrsReportList', new ReportListPlugin());
}

declare interface JQuery
{
    vrsReportList();
    vrsReportList(options: VRS.ReportListPlugin_Options);
    vrsReportList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
