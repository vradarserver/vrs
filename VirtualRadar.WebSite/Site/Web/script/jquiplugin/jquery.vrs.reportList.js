var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportListClass = VRS.globalOptions.reportListClass || 'vrsAircraftList flights';
    VRS.globalOptions.reportListSingleAircraftColumns = VRS.globalOptions.reportListSingleAircraftColumns || [
        VRS.ReportFlightProperty.RowNumber,
        VRS.ReportFlightProperty.StartTime,
        VRS.ReportFlightProperty.Duration,
        VRS.ReportFlightProperty.Callsign,
        VRS.ReportFlightProperty.RouteShort,
        VRS.ReportFlightProperty.Altitude,
        VRS.ReportFlightProperty.Speed,
        VRS.ReportFlightProperty.Squawk
    ];
    VRS.globalOptions.reportListManyAircraftColumns = VRS.globalOptions.reportListManyAircraftColumns || [
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
    VRS.globalOptions.reportListDefaultShowUnits = VRS.globalOptions.reportListDefaultShowUnits !== undefined ? VRS.globalOptions.reportListDefaultShowUnits : true;
    VRS.globalOptions.reportListDistinguishOnGround = VRS.globalOptions.reportListDistinguishOnGround !== undefined ? VRS.globalOptions.reportListDistinguishOnGround : true;
    VRS.globalOptions.reportListShowPagerTop = VRS.globalOptions.reportListShowPagerTop !== undefined ? VRS.globalOptions.reportListShowPagerTop : true;
    VRS.globalOptions.reportListShowPagerBottom = VRS.globalOptions.reportListShowPagerBottom !== undefined ? VRS.globalOptions.reportListShowPagerBottom : true;
    VRS.globalOptions.reportListUserCanConfigureColumns = VRS.globalOptions.reportListUserCanConfigureColumns !== undefined ? VRS.globalOptions.reportListUserCanConfigureColumns : true;
    VRS.globalOptions.reportListGroupBySortColumn = VRS.globalOptions.reportListGroupBySortColumn !== undefined ? VRS.globalOptions.reportListGroupBySortColumn : true;
    VRS.globalOptions.reportListGroupResetAlternateRows = VRS.globalOptions.reportListGroupResetAlternateRows !== undefined ? VRS.globalOptions.reportListGroupResetAlternateRows : false;
    var ReportListPlugin_State = (function () {
        function ReportListPlugin_State() {
            this.tableElement = null;
            this.messageElement = null;
            this.flightRows = {};
            this.selectedRowElement = null;
            this.pagerTopElement = null;
            this.pagerTopPlugin = null;
            this.pagerBottomElement = null;
            this.pagerBottomPlugin = null;
            this.rowsFetchedHookResult = null;
            this.selectedFlightChangedHookResult = null;
            this.localeChangedHookResult = null;
        }
        return ReportListPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getReportListPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsReportList');
    };
    VRS.jQueryUIHelper.getReportListOptions = function (overrides) {
        return $.extend({
            name: 'default',
            singleAircraftColumns: VRS.globalOptions.reportListSingleAircraftColumns.slice(),
            manyAircraftColumns: VRS.globalOptions.reportListManyAircraftColumns.slice(),
            useSavedState: true,
            showUnits: VRS.globalOptions.reportListDefaultShowUnits,
            distinguishOnGround: VRS.globalOptions.reportListDistinguishOnGround,
            showPagerTop: VRS.globalOptions.reportListShowPagerTop,
            showPagerBottom: VRS.globalOptions.reportListShowPagerBottom,
            groupBySortColumn: VRS.globalOptions.reportListGroupBySortColumn,
            groupResetAlternateRows: VRS.globalOptions.reportListGroupResetAlternateRows,
            justShowStartTime: false,
            alwaysShowEndDate: false
        }, overrides);
    };
    var ReportListPlugin = (function (_super) {
        __extends(ReportListPlugin, _super);
        function ReportListPlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getReportListOptions();
        }
        ReportListPlugin.prototype._getState = function () {
            var result = this.element.data('reportListPluginState');
            if (result === undefined) {
                result = new ReportListPlugin_State();
                this.element.data('reportListPluginState', result);
            }
            return result;
        };
        ReportListPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            this.element.addClass(VRS.globalOptions.reportListClass);
            if (options.useSavedState) {
                this.loadAndApplyState();
            }
            VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.rowsFetchedHookResult = options.report.hookRowsFetched(this._rowsFetched, this);
            state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
        };
        ReportListPlugin.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            if (state.rowsFetchedHookResult && options.report) {
                options.report.unhook(state.rowsFetchedHookResult);
            }
            state.rowsFetchedHookResult = null;
            if (state.selectedFlightChangedHookResult && options.report) {
                options.report.unhook(state.selectedFlightChangedHookResult);
            }
            state.selectedFlightChangedHookResult = null;
            if (state.localeChangedHookResult && VRS.globalisation) {
                VRS.globalisation.unhook(state.localeChangedHookResult);
            }
            state.localeChangedHookResult = null;
            this._destroyTable(state);
            this.element.removeClass(VRS.globalOptions.reportListClass);
        };
        ReportListPlugin.prototype.saveState = function () {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        };
        ReportListPlugin.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.singleAircraftColumns = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.singleAircraftColumns, [VRS.ReportSurface.List]);
            result.manyAircraftColumns = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.manyAircraftColumns, [VRS.ReportSurface.List]);
            return result;
        };
        ReportListPlugin.prototype.applyState = function (settings) {
            this.options.singleAircraftColumns = settings.singleAircraftColumns;
            this.options.manyAircraftColumns = settings.manyAircraftColumns;
            this.options.showUnits = settings.showUnits;
        };
        ReportListPlugin.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        ReportListPlugin.prototype._persistenceKey = function () {
            return 'vrsReportList-' + this.options.report.getName() + '-' + this.options.name;
        };
        ReportListPlugin.prototype._createSettings = function () {
            return {
                singleAircraftColumns: this.options.singleAircraftColumns,
                manyAircraftColumns: this.options.manyAircraftColumns,
                showUnits: this.options.showUnits
            };
        };
        ReportListPlugin.prototype.createOptionPane = function (displayOrder) {
            var _this = this;
            var result = [];
            result.push(new VRS.OptionPane({
                name: 'commonListSettingsPane',
                titleKey: 'PaneListSettings',
                displayOrder: displayOrder,
                fields: [
                    new VRS.OptionFieldCheckBox({
                        name: 'showUnits',
                        labelKey: 'ShowUnits',
                        getValue: function () { return _this.options.showUnits; },
                        setValue: function (value) {
                            _this.options.showUnits = value;
                            _this.refreshDisplay();
                        },
                        saveState: function () { return _this.saveState(); }
                    })
                ]
            }));
            if (VRS.globalOptions.reportListUserCanConfigureColumns) {
                var singleAircraftPane = new VRS.OptionPane({
                    name: 'singleAircraftPane',
                    titleKey: 'PaneSingleAircraft',
                    displayOrder: displayOrder + 10
                });
                result.push(singleAircraftPane);
                VRS.reportPropertyHandlerHelper.addReportPropertyListOptionsToPane({
                    pane: singleAircraftPane,
                    fieldLabel: 'Columns',
                    surface: VRS.ReportSurface.List,
                    getList: function () { return _this.options.singleAircraftColumns; },
                    setList: function (cols) {
                        _this.options.singleAircraftColumns = cols;
                        _this.refreshDisplay();
                    },
                    saveState: function () { return _this.saveState(); }
                });
                var manyAircraftPane = new VRS.OptionPane({
                    name: 'manyAircraftPane',
                    titleKey: 'PaneManyAircraft',
                    displayOrder: displayOrder + 20
                });
                result.push(manyAircraftPane);
                VRS.reportPropertyHandlerHelper.addReportPropertyListOptionsToPane({
                    pane: manyAircraftPane,
                    fieldLabel: 'Columns',
                    surface: VRS.ReportSurface.List,
                    getList: function () { return _this.options.manyAircraftColumns; },
                    setList: function (cols) {
                        _this.options.manyAircraftColumns = cols;
                        _this.refreshDisplay();
                    },
                    saveState: function () { return _this.saveState(); }
                });
            }
            return result;
        };
        ReportListPlugin.prototype.refreshDisplay = function () {
            this._buildTable(this._getState());
        };
        ReportListPlugin.prototype._buildTable = function (state) {
            var options = this.options;
            this._destroyTable(state);
            if (options.report.getFlights().length === 0) {
                state.messageElement = $('<div/>')
                    .addClass('emptyReport')
                    .text(VRS.$$.ReportEmpty)
                    .appendTo(this.element);
            }
            else {
                var self = this;
                var createPager = function (enabled, isTop) {
                    var element = null, plugin = null;
                    if (enabled && VRS.jQueryUIHelper.getReportPagerPlugin) {
                        element = $('<div/>')
                            .addClass(isTop ? 'top' : 'bottom')
                            .vrsReportPager(VRS.jQueryUIHelper.getReportPagerOptions({
                            report: options.report
                        }));
                        plugin = VRS.jQueryUIHelper.getReportPagerPlugin(element);
                    }
                    if (isTop) {
                        state.pagerTopElement = element;
                        state.pagerTopPlugin = plugin;
                    }
                    else {
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
                columns = VRS.arrayHelper.filter(columns, function (item) { return VRS.reportPropertyHandlers[item] instanceof VRS.ReportPropertyHandler; });
                this._buildHeader(state, columns, topPager);
                this._buildBody(state, columns, options.report.getFlights(), bottomPager);
            }
        };
        ReportListPlugin.prototype._addPagerToTable = function (pager, section, cellType, colspan) {
            if (pager) {
                var pagerRow = $('<tr/>')
                    .addClass('pagerRow')
                    .appendTo(section);
                $(cellType)
                    .attr('colspan', colspan)
                    .appendTo(pagerRow)
                    .append(pager);
            }
        };
        ReportListPlugin.prototype._buildHeader = function (state, columns, pager) {
            var thead = $('<thead/>')
                .appendTo(state.tableElement);
            this._addPagerToTable(pager, thead, '<th/>', columns.length);
            var tr = $('<tr/>')
                .appendTo(thead);
            var length = columns.length;
            var lastCell;
            for (var i = 0; i < length; ++i) {
                var property = columns[i];
                var handler = VRS.reportPropertyHandlers[property];
                var cell = lastCell = $('<th/>')
                    .text(VRS.globalisation.getText(handler.headingKey))
                    .appendTo(tr);
                this._setCellAlignment(cell, handler.headingAlignment);
                if (handler.fixedWidth)
                    this._setFixedWidth(cell, handler.fixedWidth());
            }
            if (lastCell) {
                lastCell.addClass('lastColumn');
            }
        };
        ReportListPlugin.prototype._buildBody = function (state, columns, flights, pager) {
            var options = this.options;
            var self = this;
            var tbody = $('<tbody/>')
                .appendTo(state.tableElement);
            var groupColumnHandler = VRS.reportPropertyHandlerHelper.findPropertyHandlerForSortColumn(options.report.getGroupSortColumn());
            var previousGroupValue = null;
            var justShowStartTime = options.justShowStartTime;
            if (groupColumnHandler && groupColumnHandler.property === VRS.ReportFlightProperty.StartTime) {
                options.justShowStartTime = true;
            }
            var countColumns = columns.length;
            var countRows = flights.length;
            var isOdd = true;
            for (var rowNum = 0; rowNum < countRows; ++rowNum) {
                var flight = flights[rowNum];
                var aircraft = flight.aircraft;
                var firstRow = rowNum === 0;
                if (options.groupBySortColumn && groupColumnHandler) {
                    var groupValue = groupColumnHandler.groupValue(groupColumnHandler.isAircraftProperty ? aircraft : flight);
                    if (!groupValue) {
                        groupValue = VRS.$$.None;
                    }
                    if (rowNum === 0 || groupValue !== previousGroupValue) {
                        if (options.groupResetAlternateRows) {
                            isOdd = true;
                        }
                        var groupRow = $('<tr/>')
                            .addClass('group')
                            .appendTo(tbody);
                        if (firstRow) {
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
                    .on('click', function (e) { self._rowClicked(e, this); });
                if (firstRow) {
                    tr.addClass('firstRow');
                }
                state.flightRows[flight.row] = tr;
                var lastCell;
                for (var colNum = 0; colNum < countColumns; ++colNum) {
                    var property = columns[colNum];
                    var handler = VRS.reportPropertyHandlers[property];
                    var cell = lastCell = $('<td/>')
                        .appendTo(tr);
                    this._setCellAlignment(cell, handler.headingAlignment);
                    if (handler.fixedWidth) {
                        this._setFixedWidth(cell, handler.fixedWidth(VRS.ReportSurface.List));
                    }
                    handler.renderIntoJQueryElement(cell, handler.isAircraftProperty ? aircraft : flight, options, VRS.ReportSurface.List);
                }
                if (lastCell) {
                    lastCell.addClass('lastColumn');
                }
                isOdd = !isOdd;
            }
            this._addPagerToTable(pager, tbody, '<td/>', columns.length);
            options.justShowStartTime = justShowStartTime;
            this._markSelectedRow();
        };
        ReportListPlugin.prototype._destroyTable = function (state) {
            if (state.messageElement) {
                state.messageElement.remove();
            }
            state.messageElement = null;
            for (var rowId in state.flightRows) {
                var row = state.flightRows[rowId];
                if (row instanceof jQuery) {
                    row.off();
                }
            }
            state.flightRows = {};
            if (state.tableElement) {
                state.tableElement.remove();
                state.tableElement = null;
            }
            state.selectedRowElement = null;
            if (state.pagerTopPlugin) {
                state.pagerTopPlugin.destroy();
            }
            if (state.pagerTopElement) {
                state.pagerTopElement.remove();
            }
            state.pagerTopElement = state.pagerTopPlugin = null;
            if (state.pagerBottomPlugin) {
                state.pagerBottomPlugin.destroy();
            }
            if (state.pagerBottomElement) {
                state.pagerBottomElement.remove();
            }
            state.pagerBottomElement = state.pagerBottomPlugin = null;
        };
        ReportListPlugin.prototype._setCellAlignment = function (cell, alignment) {
            if (cell && alignment) {
                switch (alignment) {
                    case VRS.Alignment.Left: break;
                    case VRS.Alignment.Centre:
                        cell.addClass('vrsCentre');
                        break;
                    case VRS.Alignment.Right:
                        cell.addClass('vrsRight');
                        break;
                }
            }
        };
        ReportListPlugin.prototype._setFixedWidth = function (cell, fixedWidth) {
            if (fixedWidth) {
                cell.attr('width', fixedWidth);
                cell.addClass('fixedWidth');
            }
        };
        ReportListPlugin.prototype._getFlightForTableRow = function (row) {
            var result = null;
            var state = this._getState();
            var options = this.options;
            var flights = options.report.getFlights();
            if (flights.length) {
                if (!(row instanceof jQuery)) {
                    row = $(row);
                }
                for (var rowId in state.flightRows) {
                    var numericRowId = Number(rowId);
                    var flightRow = state.flightRows[rowId];
                    if (flightRow instanceof jQuery && flightRow.is(row)) {
                        var length = flights.length;
                        for (var i = 0; i < length; ++i) {
                            var flight = flights[i];
                            if (flight.row == numericRowId) {
                                result = flight;
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            return result;
        };
        ReportListPlugin.prototype._markSelectedRow = function () {
            var state = this._getState();
            if (state.selectedRowElement) {
                state.selectedRowElement.removeClass('vrsSelected');
            }
            var selectedFlight = this.options.report.getSelectedFlight();
            if (!selectedFlight) {
                state.selectedRowElement = null;
            }
            else {
                var selectedFlightRowId = selectedFlight ? selectedFlight.row : null;
                var selectedRow = state.flightRows[selectedFlightRowId];
                if (selectedRow)
                    selectedRow.addClass('vrsSelected');
                state.selectedRowElement = selectedRow;
            }
        };
        ReportListPlugin.prototype._localeChanged = function () {
            this.refreshDisplay();
        };
        ReportListPlugin.prototype._rowClicked = function (event, target) {
            var flight = this._getFlightForTableRow(target);
            this.options.report.setSelectedFlight(flight);
        };
        ReportListPlugin.prototype._rowsFetched = function () {
            var state = this._getState();
            this._buildTable(state);
        };
        ReportListPlugin.prototype._selectedFlightChanged = function () {
            this._markSelectedRow();
        };
        return ReportListPlugin;
    }(JQueryUICustomWidget));
    VRS.ReportListPlugin = ReportListPlugin;
    $.widget('vrs.vrsReportList', new ReportListPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.reportList.js.map