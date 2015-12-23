var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportDetailClass = VRS.globalOptions.reportDetailClass || 'vrsAircraftDetail aircraft';
    VRS.globalOptions.reportDetailColumns = VRS.globalOptions.reportDetailColumns || [
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
        VRS.ReportFlightProperty.CountModeS,
        VRS.ReportFlightProperty.CountAdsb,
        VRS.ReportFlightProperty.CountPositions
    ];
    VRS.globalOptions.reportDetailAddMapToDefaultColumns = VRS.globalOptions.reportDetailAddMapToDefaultColumns !== undefined ? VRS.globalOptions.reportDetailAddMapToDefaultColumns : VRS.globalOptions.isMobile;
    if (VRS.globalOptions.reportDetailAddMapToDefaultColumns) {
        if (VRS.arrayHelper.indexOf(VRS.globalOptions.reportDetailColumns, VRS.ReportFlightProperty.PositionsOnMap) === -1) {
            VRS.globalOptions.reportDetailColumns.push(VRS.ReportFlightProperty.PositionsOnMap);
        }
    }
    VRS.globalOptions.reportDetailDefaultShowUnits = VRS.globalOptions.reportDetailDefaultShowUnits !== undefined ? VRS.globalOptions.reportDetailDefaultShowUnits : true;
    VRS.globalOptions.reportDetailDistinguishOnGround = VRS.globalOptions.reportDetailDistinguishOnGround !== undefined ? VRS.globalOptions.reportDetailDistinguishOnGround : true;
    VRS.globalOptions.reportDetailUserCanConfigureColumns = VRS.globalOptions.reportDetailUserCanConfigureColumns !== undefined ? VRS.globalOptions.reportDetailUserCanConfigureColumns : true;
    VRS.globalOptions.reportDetailDefaultShowEmptyValues = VRS.globalOptions.reportDetailDefaultShowEmptyValues !== undefined ? VRS.globalOptions.reportDetailDefaultShowEmptyValues : false;
    var ReportDetailPlugin_State = (function () {
        function ReportDetailPlugin_State() {
            this.suspended = false;
            this.containerElement = null;
            this.headerElement = null;
            this.bodyElement = null;
            this.bodyPropertyElements = {};
            this.selectedFlightChangedHookResult = null;
            this.localeChangedHookResult = null;
        }
        return ReportDetailPlugin_State;
    })();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getReportDetailPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsReportDetail');
    };
    VRS.jQueryUIHelper.getReportDetailOptions = function (overrides) {
        return $.extend({
            name: 'default',
            columns: VRS.globalOptions.reportDetailColumns.slice(),
            useSavedState: true,
            showUnits: VRS.globalOptions.reportDetailDefaultShowUnits,
            showEmptyValues: VRS.globalOptions.reportDetailDefaultShowEmptyValues,
            distinguishOnGround: VRS.globalOptions.reportDetailDistinguishOnGround
        }, overrides);
    };
    var ReportDetailPlugin = (function (_super) {
        __extends(ReportDetailPlugin, _super);
        function ReportDetailPlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getReportDetailOptions();
        }
        ReportDetailPlugin.prototype._getState = function () {
            var result = this.element.data('reportDetailPanelState');
            if (result === undefined) {
                result = new ReportDetailPlugin_State();
                this.element.data('reportDetailPanelState', result);
            }
            return result;
        };
        ReportDetailPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            if (options.useSavedState) {
                this.loadAndApplyState();
            }
            this._displayFlightDetails(state, options.report.getSelectedFlight());
            VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
        };
        ReportDetailPlugin.prototype._destroy = function () {
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
            state.displayedFlight = null;
            this._destroyDisplay(state);
        };
        ReportDetailPlugin.prototype.saveState = function () {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        };
        ReportDetailPlugin.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.columns = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.columns, [VRS.ReportSurface.DetailBody]);
            return result;
        };
        ReportDetailPlugin.prototype.applyState = function (settings) {
            this.options.columns = settings.columns;
            this.options.showUnits = settings.showUnits;
            this.options.showEmptyValues = settings.showEmptyValues;
        };
        ReportDetailPlugin.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        ReportDetailPlugin.prototype._persistenceKey = function () {
            return 'vrsReportDetailPanel-' + this.options.report.getName() + '-' + this.options.name;
        };
        ReportDetailPlugin.prototype._createSettings = function () {
            return {
                columns: this.options.columns,
                showUnits: this.options.showUnits,
                showEmptyValues: this.options.showEmptyValues
            };
        };
        ReportDetailPlugin.prototype.createOptionPane = function (displayOrder) {
            var _this = this;
            var result = new VRS.OptionPane({
                name: 'vrsReportDetailPane-' + this.options.name + '_Settings',
                titleKey: 'DetailPanel',
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
                    }),
                    new VRS.OptionFieldCheckBox({
                        name: 'showEmptyValues',
                        labelKey: 'ShowEmptyValues',
                        getValue: function () { return _this.options.showEmptyValues; },
                        setValue: function (value) {
                            _this.options.showEmptyValues = value;
                            _this.refreshDisplay();
                        },
                        saveState: function () { return _this.saveState(); }
                    })
                ]
            });
            if (VRS.globalOptions.reportDetailUserCanConfigureColumns) {
                VRS.reportPropertyHandlerHelper.addReportPropertyListOptionsToPane({
                    pane: result,
                    fieldLabel: 'Columns',
                    surface: VRS.ReportSurface.DetailBody,
                    getList: function () { return _this.options.columns; },
                    setList: function (cols) {
                        _this.options.columns = cols;
                        _this.refreshDisplay();
                    },
                    saveState: function () { return _this.saveState(); }
                });
            }
            return result;
        };
        ReportDetailPlugin.prototype.suspend = function (onOff) {
            onOff = !!onOff;
            var state = this._getState();
            if (state.suspended !== onOff) {
                state.suspended = onOff;
                if (!state.suspended)
                    this.refreshDisplay();
            }
        };
        ReportDetailPlugin.prototype._displayFlightDetails = function (state, flight) {
            state.displayedFlight = flight;
            if (!state.suspended) {
                this._destroyDisplay(state);
                if (flight) {
                    state.containerElement = $('<div/>')
                        .addClass(VRS.globalOptions.reportDetailClass)
                        .appendTo(this.element);
                    this._createHeader(state, flight);
                    this._createBody(state, flight);
                }
            }
        };
        ReportDetailPlugin.prototype.refreshDisplay = function () {
            var state = this._getState();
            this._displayFlightDetails(state, state.displayedFlight);
        };
        ReportDetailPlugin.prototype._destroyDisplay = function (state) {
            for (var propertyName in state.bodyPropertyElements) {
                var handler = VRS.reportPropertyHandlers[propertyName];
                var element = state.bodyPropertyElements[propertyName];
                if (handler && element) {
                    handler.destroyWidgetInJQueryElement(element, VRS.ReportSurface.DetailBody);
                }
            }
            state.bodyPropertyElements = {};
            if (state.bodyElement) {
                state.bodyElement.remove();
            }
            state.bodyElement = null;
            if (state.headerElement) {
                state.headerElement.remove();
            }
            state.headerElement = null;
            if (state.containerElement) {
                state.containerElement.remove();
            }
            state.containerElement = null;
        };
        ReportDetailPlugin.prototype._createHeader = function (state, flight) {
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
        };
        ReportDetailPlugin.prototype._addHeaderCell = function (state, row, colspan, flight, property, classes) {
            var cell = $('<td/>')
                .addClass(classes)
                .appendTo(row);
            if (colspan > 1)
                cell.attr('colspan', colspan);
            var handler = VRS.reportPropertyHandlers[property];
            if (!handler)
                throw 'Cannot find the handler for the ' + property + ' property';
            handler.renderIntoJQueryElement(cell, handler.isAircraftProperty ? flight.aircraft : flight, this.options, VRS.ReportSurface.DetailHead);
        };
        ReportDetailPlugin.prototype._createBody = function (state, flight) {
            var options = this.options;
            var columns = options.columns;
            state.bodyElement = $('<div/>')
                .addClass('body')
                .appendTo(state.containerElement);
            var list = $('<ul/>')
                .appendTo(state.bodyElement);
            var length = columns.length;
            for (var i = 0; i < length; ++i) {
                var property = columns[i];
                var handler = VRS.reportPropertyHandlers[property];
                if (handler && (options.showEmptyValues || handler.hasValue(flight))) {
                    var suppressLabel = handler.suppressLabelCallback(VRS.ReportSurface.DetailBody);
                    var listItem = $('<li/>')
                        .appendTo(list);
                    if (suppressLabel) {
                        $('<div/>')
                            .addClass('noLabel')
                            .appendTo(listItem);
                    }
                    else {
                        $('<div/>')
                            .addClass('label')
                            .append($('<span/>')
                            .text(VRS.globalisation.getText(handler.labelKey) + ':'))
                            .appendTo(listItem);
                    }
                    var contentContainer = $('<div/>')
                        .addClass('content')
                        .appendTo(listItem);
                    var content = $('<span/>')
                        .appendTo(contentContainer);
                    if (suppressLabel) {
                        listItem.addClass('wide');
                        contentContainer.addClass('wide');
                    }
                    if (handler.isMultiLine) {
                        listItem.addClass('multiline');
                        contentContainer.addClass('multiline');
                    }
                    state.bodyPropertyElements[property] = content;
                    var json = handler.isAircraftProperty ? flight.aircraft : flight;
                    handler.createWidgetInJQueryElement(content, VRS.ReportSurface.DetailBody, options);
                    handler.renderIntoJQueryElement(content, json, options, VRS.ReportSurface.DetailBody);
                }
            }
        };
        ReportDetailPlugin.prototype._localeChanged = function () {
            this.refreshDisplay();
        };
        ReportDetailPlugin.prototype._selectedFlightChanged = function () {
            var selectedFlight = this.options.report.getSelectedFlight();
            this._displayFlightDetails(this._getState(), selectedFlight);
        };
        return ReportDetailPlugin;
    })(JQueryUICustomWidget);
    VRS.ReportDetailPlugin = ReportDetailPlugin;
    $.widget('vrs.vrsReportDetail', new ReportDetailPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.reportDetail.js.map