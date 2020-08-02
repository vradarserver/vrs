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
    VRS.globalOptions.listPluginDistinguishOnGround = VRS.globalOptions.listPluginDistinguishOnGround !== undefined ? VRS.globalOptions.listPluginDistinguishOnGround : true;
    VRS.globalOptions.listPluginFlagUncertainCallsigns = VRS.globalOptions.listPluginFlagUncertainCallsigns !== undefined ? VRS.globalOptions.listPluginFlagUncertainCallsigns : true;
    VRS.globalOptions.listPluginUserCanConfigureColumns = VRS.globalOptions.listPluginUserCanConfigureColumns !== undefined ? VRS.globalOptions.listPluginUserCanConfigureColumns : true;
    if (VRS.globalOptions.isMobile) {
        VRS.globalOptions.listPluginDefaultColumns = VRS.globalOptions.listPluginDefaultColumns || [
            VRS.RenderProperty.SilhouetteAndOpFlag,
            VRS.RenderProperty.RegistrationAndIcao,
            VRS.RenderProperty.CallsignAndShortRoute,
            VRS.RenderProperty.AltitudeAndVerticalSpeed,
            VRS.RenderProperty.Speed
        ];
        VRS.globalOptions.listPluginDefaultShowUnits = VRS.globalOptions.listPluginDefaultShowUnits !== undefined ? VRS.globalOptions.listPluginDefaultShowUnits : false;
    }
    else if (VRS.globalOptions.isFlightSim) {
        VRS.globalOptions.listPluginDefaultColumns = VRS.globalOptions.listPluginDefaultColumns || [
            VRS.RenderProperty.Registration,
            VRS.RenderProperty.Callsign,
            VRS.RenderProperty.ModelIcao,
            VRS.RenderProperty.AltitudeBarometric,
            VRS.RenderProperty.VerticalSpeed,
            VRS.RenderProperty.Squawk,
            VRS.RenderProperty.Speed
        ];
        VRS.globalOptions.listPluginDefaultShowUnits = VRS.globalOptions.listPluginDefaultShowUnits !== undefined ? VRS.globalOptions.listPluginDefaultShowUnits : true;
    }
    else {
        VRS.globalOptions.listPluginDefaultColumns = VRS.globalOptions.listPluginDefaultColumns || [
            VRS.RenderProperty.Silhouette,
            VRS.RenderProperty.OperatorFlag,
            VRS.RenderProperty.Registration,
            VRS.RenderProperty.Icao,
            VRS.RenderProperty.Callsign,
            VRS.RenderProperty.RouteShort,
            VRS.RenderProperty.Altitude,
            VRS.RenderProperty.Speed
        ];
        VRS.globalOptions.listPluginDefaultShowUnits = VRS.globalOptions.listPluginDefaultShowUnits !== undefined ? VRS.globalOptions.listPluginDefaultShowUnits : true;
    }
    VRS.globalOptions.listPluginShowSorterOptions = VRS.globalOptions.listPluginShowSorterOptions !== undefined ? VRS.globalOptions.listPluginShowSorterOptions : true;
    VRS.globalOptions.listPluginShowPause = VRS.globalOptions.listPluginShowPause !== undefined ? VRS.globalOptions.listPluginShowPause : true;
    VRS.globalOptions.listPluginShowHideAircraftNotOnMap = VRS.globalOptions.listPluginShowHideAircraftNotOnMap !== undefined ? VRS.globalOptions.listPluginShowHideAircraftNotOnMap : true;
    var RowState = {
        Odd: 0x0001,
        Even: 0x0002,
        Selected: 0x0004,
        Emergency: 0x0008,
        FirstRow: 0x0010,
        Interested: 0x0020
    };
    var AircraftListPlugin_State = (function () {
        function AircraftListPlugin_State() {
            this.container = undefined;
            this.table = undefined;
            this.tableHeader = undefined;
            this.tableBody = undefined;
            this.columns = [];
            this.headerCells = [];
            this.rowData = [];
            this.trackingAircraftElement = undefined;
            this.trackingAircraftCount = -1;
            this.availableAircraftCount = -1;
            this.pauseLinkRenderer = null;
            this.hideAircraftNotOnMapLinkRenderer = null;
            this.linksElement = null;
            this.linksPlugin = null;
            this.poweredByElement = undefined;
            this.aircraftListUpdatedHook = undefined;
            this.selectedAircraftChangedHook = undefined;
            this.localeChangedHook = undefined;
            this.unitChangedHook = undefined;
            this.sortFieldsChangedHook = undefined;
            this.suspended = false;
        }
        return AircraftListPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftListPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsAircraftList');
    };
    VRS.jQueryUIHelper.getAircraftListOptions = function (overrides) {
        return $.extend({
            name: 'default',
            sorter: null,
            showSorterOptions: VRS.globalOptions.listPluginShowSorterOptions,
            columns: VRS.globalOptions.listPluginDefaultColumns.slice(),
            useSavedState: true,
            useSorterSavedState: true,
            showUnits: VRS.globalOptions.listPluginDefaultShowUnits,
            distinguishOnGround: VRS.globalOptions.listPluginDistinguishOnGround,
            flagUncertainCallsigns: VRS.globalOptions.listPluginFlagUncertainCallsigns,
            showPause: VRS.globalOptions.listPluginShowPause,
            showHideAircraftNotOnMap: VRS.globalOptions.listPluginShowHideAircraftNotOnMap
        }, overrides);
    };
    var AircraftListPlugin = (function (_super) {
        __extends(AircraftListPlugin, _super);
        function AircraftListPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getAircraftListOptions();
            return _this;
        }
        AircraftListPlugin.prototype._getState = function () {
            var result = this.element.data('aircraftListPluginState');
            if (result === undefined) {
                result = new AircraftListPlugin_State();
                this.element.data('aircraftListPluginState', result);
            }
            return result;
        };
        AircraftListPlugin.prototype._create = function () {
            var options = this.options;
            if (!options.aircraftList)
                throw 'An aircraft list must be supplied';
            if (!options.unitDisplayPreferences)
                throw 'A unit display preferences object must be supplied';
            if (options.useSavedState)
                this.loadAndApplyState();
            if (options.useSorterSavedState)
                this.options.sorter.loadAndApplyState();
            var state = this._getState();
            state.container = $('<div/>')
                .addClass('vrsAircraftList')
                .appendTo(this.element);
            state.trackingAircraftElement = $('<p/>')
                .addClass('count')
                .appendTo(state.container);
            if ((options.showPause || options.showHideAircraftNotOnMap) && options.aircraftListFetcher) {
                var linkSites = [];
                if (options.showPause) {
                    state.pauseLinkRenderer = new VRS.PauseLinkRenderHandler(options.aircraftListFetcher);
                    linkSites.push(state.pauseLinkRenderer);
                }
                if (options.showHideAircraftNotOnMap) {
                    state.hideAircraftNotOnMapLinkRenderer = new VRS.HideAircraftNotOnMapLinkRenderHandler(options.aircraftListFetcher);
                    linkSites.push(state.hideAircraftNotOnMapLinkRenderer);
                }
                state.linksElement = $('<div/>')
                    .addClass('links')
                    .vrsAircraftLinks(VRS.jQueryUIHelper.getAircraftLinksOptions({ linkSites: linkSites }))
                    .appendTo(state.container);
                state.linksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(state.linksElement);
                if (state.pauseLinkRenderer)
                    state.pauseLinkRenderer.addLinksRendererPlugin(state.linksPlugin);
                if (state.hideAircraftNotOnMapLinkRenderer)
                    state.hideAircraftNotOnMapLinkRenderer.addLinksRendererPlugin(state.linksPlugin);
                state.linksPlugin.renderForAircraft(null, true);
            }
            state.table = $('<table/>')
                .addClass('aircraftList live')
                .appendTo(state.container);
            state.tableHeader = $('<thead/>')
                .appendTo(state.table);
            state.tableBody = $('<tbody/>')
                .appendTo(state.table);
            state.poweredByElement = $('<div/>')
                .addClass('poweredBy')
                .appendTo(state.container);
            this._buildTable(state);
            state.aircraftListUpdatedHook = options.aircraftList.hookUpdated(this._aircraftListUpdated, this);
            state.selectedAircraftChangedHook = options.aircraftList.hookSelectedAircraftChanged(this._selectedAircraftChanged, this);
            state.localeChangedHook = VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.unitChangedHook = options.unitDisplayPreferences.hookUnitChanged(this._displayUnitChanged, this);
            state.sortFieldsChangedHook = options.sorter ? options.sorter.hookSortFieldsChanged(this._sortFieldsChanged, this) : null;
        };
        AircraftListPlugin.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            if (state.linksElement) {
                if (state.linksPlugin)
                    state.linksPlugin.destroy();
                state.linksElement.remove();
                if (state.pauseLinkRenderer)
                    state.pauseLinkRenderer.dispose();
                if (state.hideAircraftNotOnMapLinkRenderer)
                    state.hideAircraftNotOnMapLinkRenderer.dispose();
                state.linksElement = state.linksPlugin = state.pauseLinkRenderer = state.hideAircraftNotOnMapLinkRenderer = null;
            }
            if (state.container) {
                this.element.empty();
                state.container = state.table = state.tableHeader = state.tableBody = undefined;
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
            if (state.sortFieldsChangedHook) {
                if (options && options.sorter)
                    options.sorter.unhook(state.sortFieldsChangedHook);
                state.sortFieldsChangedHook = undefined;
            }
            $.each(state.headerCells, function (idx, cell) {
                cell.off();
            });
            var tbody = state.tableBody;
            var tbodyElement = tbody[0];
            var tbodyRows = tbodyElement.rows;
            var countRows = tbodyRows.length;
            var dataArray = state.rowData;
            for (var hideRowNum = 0; hideRowNum < countRows; ++hideRowNum) {
                this._showRow(state, tbodyRows[hideRowNum], dataArray[hideRowNum], false);
            }
            this.element.empty();
        };
        AircraftListPlugin.prototype.saveState = function () {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        };
        AircraftListPlugin.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            if (!result.columns)
                result.columns = VRS.globalOptions.listPluginDefaultColumns.slice();
            result.columns = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.columns, [VRS.RenderSurface.List]);
            return result;
        };
        AircraftListPlugin.prototype.applyState = function (settings) {
            this.options.columns = settings.columns;
            this.options.showUnits = settings.showUnits;
        };
        AircraftListPlugin.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        AircraftListPlugin.prototype._persistenceKey = function () {
            return 'vrsAircraftListPlugin-' + this.options.name;
        };
        AircraftListPlugin.prototype._createSettings = function () {
            return {
                columns: this.options.columns || [],
                showUnits: this.options.showUnits
            };
        };
        AircraftListPlugin.prototype.createOptionPane = function (displayOrder) {
            var _this = this;
            var result = [];
            if (this.options.sorter && this.options.showSorterOptions) {
                var sorterOptions = this.options.sorter.createOptionPane(displayOrder);
                if (VRS.arrayHelper.isArray(sorterOptions)) {
                    $.each(sorterOptions, function (idx, pane) { result.push(pane); });
                }
                else {
                    result.push(sorterOptions);
                }
            }
            var settingsPane = new VRS.OptionPane({
                name: 'vrsAircraftListPlugin_' + this.options.name + '_Settings',
                titleKey: 'PaneListSettings',
                displayOrder: displayOrder + 10,
                fields: [
                    new VRS.OptionFieldCheckBox({
                        name: 'showUnits',
                        labelKey: 'ShowUnits',
                        getValue: function () { return _this.options.showUnits; },
                        setValue: function (value) {
                            _this.options.showUnits = value;
                            _this._buildTable(_this._getState());
                        },
                        saveState: function () { return _this.saveState(); }
                    })
                ]
            });
            if (VRS.globalOptions.listPluginUserCanConfigureColumns) {
                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane: settingsPane,
                    fieldLabel: 'Columns',
                    surface: VRS.RenderSurface.List,
                    getList: function () { return _this.options.columns; },
                    setList: function (value) {
                        _this.options.columns = value;
                        _this._buildTable(_this._getState());
                    },
                    saveState: function () { return _this.saveState(); }
                });
            }
            result.push(settingsPane);
            return result;
        };
        AircraftListPlugin.prototype.suspend = function (onOff) {
            var state = this._getState();
            if (state.suspended !== onOff) {
                state.suspended = onOff;
                if (!state.suspended) {
                    this._refreshDisplay(state, true);
                }
            }
        };
        AircraftListPlugin.prototype.prependElement = function (elementJQ) {
            var state = this._getState();
            state.container.prepend(elementJQ);
        };
        AircraftListPlugin.prototype._buildTable = function (state) {
            this._buildHeader(state);
            this._refreshDisplay(state, true);
        };
        AircraftListPlugin.prototype._buildHeader = function (state) {
            $.each(state.headerCells, function (idx, cell) {
                cell.off();
            });
            var thead = state.tableHeader;
            thead.empty();
            var trow = $('<tr/>')
                .appendTo(thead);
            var columns = this.options.columns;
            var countColumns = columns.length;
            state.columns = [];
            state.headerCells = [];
            for (var i = 0; i < countColumns; ++i) {
                var columnId = columns[i];
                var handler = VRS.renderPropertyHandlers[columnId];
                if (!handler)
                    throw 'No handler has been registered for property ID ' + columnId;
                var headingText = handler.suppressLabelCallback(VRS.RenderSurface.List) ? '' : VRS.globalisation.getText(handler.headingKey);
                var cell = $('<th/>')
                    .text(headingText)
                    .appendTo(trow);
                if (i + 1 == countColumns)
                    cell.addClass('lastColumn');
                state.columns.push(handler);
                state.headerCells.push(cell);
                if (handler.sortableField !== VRS.AircraftListSortableField.None) {
                    cell.addClass('sortHeader');
                    cell.on('click', $.proxy(this._sortableHeaderClicked, this));
                }
                var data = {};
                this._setCellAlignment(cell, data, handler.headingAlignment);
                this._setCellWidth(cell, data, handler.fixedWidth(VRS.RenderSurface.List));
            }
        };
        AircraftListPlugin.prototype._refreshDisplay = function (state, refreshAll, displayUnitChanged) {
            if (refreshAll)
                this._updatePoweredByCredit(state);
            this._updateTrackedAircraftCount(state, refreshAll);
            this._buildRows(state, refreshAll, displayUnitChanged);
        };
        AircraftListPlugin.prototype._updatePoweredByCredit = function (state) {
            var version = VRS.serverConfig.get().VrsVersion;
            state.poweredByElement
                .empty()
                .append($('<a/>')
                .attr('href', 'http://www.virtualradarserver.co.uk/')
                .attr('target', 'external')
                .attr('title', VRS.stringUtility.format(VRS.$$.VrsVersion, version))
                .text(VRS.$$.PoweredByVRS));
        };
        AircraftListPlugin.prototype._updateTrackedAircraftCount = function (state, refreshAll) {
            var trackedAircraft = this.options.aircraftList.getCountTrackedAircraft();
            var availableAircraft = this.options.aircraftList.getCountAvailableAircraft();
            if (refreshAll || state.trackingAircraftCount !== trackedAircraft || state.availableAircraftCount !== availableAircraft) {
                var text = '';
                if (availableAircraft == 1) {
                    text = trackedAircraft == availableAircraft ? VRS.$$.TrackingOneAircraft
                        : VRS.stringUtility.format(VRS.$$.TrackingOneAircraftOutOf, trackedAircraft);
                }
                else {
                    text = trackedAircraft == availableAircraft ? VRS.stringUtility.format(VRS.$$.TrackingCountAircraft, availableAircraft)
                        : VRS.stringUtility.format(VRS.$$.TrackingCountAircraftOutOf, availableAircraft, trackedAircraft);
                }
                state.trackingAircraftElement.text(text);
                state.trackingAircraftCount = trackedAircraft;
                state.availableAircraftCount = availableAircraft;
            }
        };
        AircraftListPlugin.prototype._buildRows = function (state, refreshAll, displayUnitChanged) {
            var _this = this;
            var aircraftList = this.options.aircraftList.toList();
            var options = this.options;
            if (options.sorter)
                options.sorter.sortAircraftArray(aircraftList, options.unitDisplayPreferences);
            var columns = this.options.columns;
            var columnCount = columns.length;
            var tbody = state.tableBody;
            var dataArray = state.rowData;
            var tbodyElement = state.tableBody[0];
            var tbodyRows = tbodyElement.rows;
            if (tbodyRows.length !== dataArray.length)
                throw 'Assertion failed - state.rowData and tbody.rows are no longer in sync';
            if (refreshAll) {
                this._ensureBodyRowsHaveCorrectNumberOfColumns(tbodyRows, dataArray, columnCount);
            }
            var createRow = function (id) {
                var row = tbodyElement.insertRow(-1);
                var data = { row: { aircraftId: id, visible: true, rowState: undefined }, cells: [] };
                var self = _this;
                $(row).click(function (e) { self._rowClicked(e, this); });
                dataArray.push(data);
                for (var i = 0; i < columnCount; ++i) {
                    var cell = row.insertCell(-1);
                    data.cells.push({});
                    if (i + 1 == columnCount) {
                        VRS.domHelper.setClass(cell, 'lastColumn');
                    }
                }
            };
            var countAircraft = aircraftList.length;
            var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
            for (var rowNum = 0; rowNum < countAircraft; ++rowNum) {
                var aircraft = aircraftList[rowNum];
                var refreshRow = refreshAll;
                var refreshCellProperties = refreshAll;
                if (rowNum >= dataArray.length) {
                    createRow(aircraft.id);
                    refreshCellProperties = refreshRow = true;
                }
                var row = tbodyRows[rowNum];
                var data = dataArray[rowNum];
                var rowDataRow = data.row;
                if (rowDataRow.aircraftId !== aircraft.id) {
                    rowDataRow.aircraftId = aircraft.id;
                    refreshCellProperties = refreshRow = true;
                }
                if (rowDataRow.visible !== undefined && !rowDataRow.visible)
                    refreshRow = true;
                var cells = row.cells;
                for (var colNum = 0; colNum < columnCount; ++colNum) {
                    var handler = state.columns[colNum];
                    var cell = cells[colNum];
                    var cellData = data.cells[colNum];
                    var contentChanged = refreshRow;
                    if (!contentChanged) {
                        if (displayUnitChanged) {
                            contentChanged = handler.usesDisplayUnit(displayUnitChanged);
                        }
                        else {
                            contentChanged = handler.hasChangedCallback(aircraft);
                        }
                    }
                    var tooltipChanged = refreshRow;
                    if (!tooltipChanged)
                        tooltipChanged = handler.tooltipChangedCallback(aircraft);
                    if (refreshCellProperties) {
                        this._createWidgetRenderer(cell, cellData, handler);
                        this._setCellAlignment(cell, cellData, handler.contentAlignment);
                        this._setCellWidth(cell, cellData, handler.fixedWidth(VRS.RenderSurface.List));
                    }
                    if (contentChanged) {
                        cellData.value = handler.renderToDom(cell, VRS.RenderSurface.List, aircraft, options, true, cellData.value);
                    }
                    if (tooltipChanged) {
                        cellData.hasTitle = handler.renderTooltipToDom(cell, VRS.RenderSurface.List, aircraft, options);
                    }
                }
                this._setRowState(row, rowDataRow, rowNum, aircraft === selectedAircraft, aircraft.isEmergency.val, aircraft.userInterested.val);
                this._showRow(state, row, data, true);
            }
            var countRows = tbodyRows.length;
            for (var hideRowNum = countAircraft; hideRowNum < countRows; ++hideRowNum) {
                this._showRow(state, tbodyRows[hideRowNum], dataArray[hideRowNum], false);
            }
        };
        AircraftListPlugin.prototype._ensureBodyRowsHaveCorrectNumberOfColumns = function (tbodyRows, dataArray, columnCount) {
            var length = tbodyRows.length;
            if (length > 0 && dataArray[0].cells.length !== columnCount) {
                var addCells = columnCount - dataArray[0].cells.length;
                for (var r = 0; r < length; ++r) {
                    var data = dataArray[r];
                    var row = tbodyRows[r];
                    if (addCells > 0) {
                        for (var i = 0; i < addCells; ++i) {
                            data.cells.push({});
                            row.insertCell(-1);
                        }
                    }
                    else {
                        var cellsLength = data.cells.length;
                        for (var i = data.cells.length + addCells; i < cellsLength; ++i) {
                            var cellData = data.cells[i];
                            if (cellData.widgetProperty) {
                                this._destroyWidgetRenderer((row.cells[i]), cellData);
                            }
                        }
                        data.cells.splice(cellsLength + addCells, -addCells);
                        for (var i = 0; i > addCells; --i) {
                            row.deleteCell(-1);
                        }
                    }
                }
            }
        };
        AircraftListPlugin.prototype._setCellAlignment = function (cell, cellData, alignment) {
            if (!cellData.alignment || cellData.alignment !== alignment) {
                var isDOM = cell instanceof HTMLTableCellElement;
                var addClass = function (classNames) { isDOM ? VRS.domHelper.addClasses(cell, [classNames]) : cell.addClass(classNames); };
                var removeClass = function (classNames) { isDOM ? VRS.domHelper.removeClasses(cell, [classNames]) : cell.removeClass(classNames); };
                if (cellData.alignment) {
                    switch (cellData.alignment) {
                        case VRS.Alignment.Left: break;
                        case VRS.Alignment.Centre:
                            removeClass('vrsCentre');
                            break;
                        case VRS.Alignment.Right:
                            removeClass('vrsRight');
                            break;
                        default: throw 'Unknown alignment ' + cellData.alignment;
                    }
                }
                switch (alignment) {
                    case VRS.Alignment.Left: break;
                    case VRS.Alignment.Centre:
                        addClass('vrsCentre');
                        break;
                    case VRS.Alignment.Right:
                        addClass('vrsRight');
                        break;
                    default: throw 'Unknown alignment ' + alignment;
                }
                cellData.alignment = alignment;
            }
        };
        AircraftListPlugin.prototype._setCellWidth = function (cell, cellData, fixedWidth) {
            if (!cellData.fixedWidth || cellData.fixedWidth !== fixedWidth) {
                var isDOM = cell instanceof HTMLTableCellElement;
                if (cellData.fixedWidth) {
                    isDOM ? VRS.domHelper.removeAttribute(cell, 'width') : cell.removeAttr('width');
                    isDOM ? VRS.domHelper.removeAttribute(cell, 'max-width') : cell.removeAttr('max-width');
                    isDOM ? VRS.domHelper.removeClasses(cell, ['fixedWidth']) : cell.removeClass('fixedWidth');
                }
                if (fixedWidth) {
                    isDOM ? VRS.domHelper.setAttribute(cell, 'width', fixedWidth) : cell.attr('width', fixedWidth);
                    isDOM ? VRS.domHelper.setAttribute(cell, 'max-width', fixedWidth) : cell.attr('max-width', fixedWidth);
                    isDOM ? VRS.domHelper.addClasses(cell, ['fixedWidth']) : cell.addClass('fixedWidth');
                }
                cellData.fixedWidth = fixedWidth;
            }
        };
        AircraftListPlugin.prototype._showRow = function (state, row, rowData, visible) {
            if (rowData.row.visible === undefined || rowData.row.visible !== visible) {
                var isDOM = row instanceof HTMLTableRowElement;
                if (!visible)
                    isDOM ? VRS.domHelper.addClasses(row, ['hidden']) : row.addClass('hidden');
                else
                    isDOM ? VRS.domHelper.removeClasses(row, ['hidden']) : row.removeClass('hidden');
                if (!visible) {
                    var cells = isDOM ? row.cells : row[0].cells;
                    var countCells = cells.length;
                    for (var i = 0; i < countCells; ++i) {
                        var cell = cells[i];
                        var cellData = rowData.cells[i];
                        if (cellData.widgetProperty) {
                            var handler = state.columns[i];
                            this._destroyWidgetRenderer(cell, cellData, handler);
                        }
                        cell.textContent = '';
                        cellData.value = '';
                    }
                    rowData.row.aircraftId = -1;
                }
                rowData.row.visible = visible;
            }
        };
        AircraftListPlugin.prototype._setRowState = function (row, rowData, rowNumber, isSelectedAircraft, isEmergency, isInterested) {
            var rowState = isSelectedAircraft ? RowState.Selected
                : rowNumber % 2 === 0 ? RowState.Odd
                    : RowState.Even;
            if (isEmergency)
                rowState |= RowState.Emergency;
            if (isInterested)
                rowState |= RowState.Interested;
            if (rowNumber === 0)
                rowState |= RowState.FirstRow;
            var getRowStateClass = function (value) {
                var result = [];
                if (value & RowState.FirstRow)
                    result.push('firstRow');
                if (value & RowState.Interested)
                    result.push('interested');
                if (value & RowState.Selected) {
                    if (value & RowState.Emergency)
                        result.push('vrsSelectedEmergency');
                    else
                        result.push('vrsSelected');
                }
                else {
                    if (value & RowState.Emergency)
                        result.push('vrsEmergency');
                    else if (value & RowState.Even)
                        result.push('vrsEven');
                    else
                        result.push('vrsOdd');
                }
                return result;
            };
            if (!rowData.rowState || rowData.rowState !== rowState) {
                var isDOM = row instanceof HTMLTableRowElement;
                if (rowData.rowState) {
                    var removeClasses = getRowStateClass(rowData.rowState);
                    isDOM ? VRS.domHelper.removeClasses(row, removeClasses) : row.removeClass(removeClasses.join(' '));
                }
                var addClasses = getRowStateClass(rowState);
                isDOM ? VRS.domHelper.addClasses(row, addClasses) : row.addClass(addClasses.join(' '));
                rowData.rowState = rowState;
            }
        };
        AircraftListPlugin.prototype._createWidgetRenderer = function (cell, cellData, handler) {
            if (handler.isWidgetProperty() && cellData.widgetProperty !== handler.property) {
                var cellJQ = $(cell);
                if (cellData.widgetProperty)
                    handler.destroyWidgetInJQuery(cellJQ, VRS.RenderSurface.List);
                handler.createWidgetInJQuery(cellJQ, VRS.RenderSurface.List);
                cellData.widgetProperty = handler.property;
            }
        };
        AircraftListPlugin.prototype._destroyWidgetRenderer = function (cell, cellData, handler) {
            if (cellData.widgetProperty) {
                if (!handler || handler.property !== cellData.widgetProperty) {
                    handler = VRS.renderPropertyHandlers[cellData.widgetProperty];
                }
                handler.destroyWidgetInDom(cell, VRS.RenderSurface.List);
                cellData.widgetProperty = undefined;
            }
        };
        AircraftListPlugin.prototype._getRowIndexForAircraftId = function (state, aircraftId) {
            var result = -1;
            var length = state.rowData.length;
            for (var i = 0; i < length; ++i) {
                if (state.rowData[i].row.aircraftId === aircraftId) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        AircraftListPlugin.prototype._getRowIndexForRowElement = function (state, rowElement) {
            var result = -1;
            var tbodyRows = state.tableBody[0].rows;
            var rowCount = tbodyRows.length;
            for (var i = 0; i < rowCount; ++i) {
                if (tbodyRows[i] === rowElement) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        AircraftListPlugin.prototype._aircraftListUpdated = function () {
            var state = this._getState();
            if (!state.suspended) {
                this._refreshDisplay(state, false);
            }
        };
        AircraftListPlugin.prototype._localeChanged = function () {
            var state = this._getState();
            this._buildHeader(state);
            if (!state.suspended) {
                this._refreshDisplay(state, true);
            }
        };
        AircraftListPlugin.prototype._displayUnitChanged = function (displayUnitChanged) {
            var state = this._getState();
            if (!state.suspended) {
                this._refreshDisplay(state, false, displayUnitChanged);
            }
        };
        AircraftListPlugin.prototype._rowClicked = function (event, target) {
            if (target) {
                var state = this._getState();
                var clickedRowIndex = this._getRowIndexForRowElement(state, target);
                if (clickedRowIndex !== -1 && clickedRowIndex < state.rowData.length) {
                    var rowData = state.rowData[clickedRowIndex];
                    var aircraftId = rowData.row.aircraftId;
                    var aircraft = this.options.aircraftList.findAircraftById(aircraftId);
                    if (aircraft)
                        this.options.aircraftList.setSelectedAircraft(aircraft, true);
                    VRS.globalDispatch.raise(VRS.globalEvent.displayUpdated);
                }
            }
        };
        AircraftListPlugin.prototype._selectedAircraftChanged = function (oldSelectedAircraft) {
            var _this = this;
            var state = this._getState();
            var options = this.options;
            var tbodyRows = state.tableBody[0].rows;
            var refreshRowStateForAircraft = function (aircraft, isSelected) {
                var rowNum = _this._getRowIndexForAircraftId(state, aircraft.id);
                if (rowNum !== -1 && rowNum < tbodyRows.length) {
                    _this._setRowState(tbodyRows[rowNum], state.rowData[rowNum].row, rowNum, isSelected, aircraft.isEmergency.val, aircraft.userInterested.val);
                }
            };
            var selectedAircraft = options.aircraftList.getSelectedAircraft();
            if (oldSelectedAircraft) {
                refreshRowStateForAircraft(oldSelectedAircraft, false);
            }
            if (selectedAircraft) {
                refreshRowStateForAircraft(selectedAircraft, true);
            }
        };
        AircraftListPlugin.prototype._sortFieldsChanged = function () {
            var state = this._getState();
            if (!state.suspended) {
                this._refreshDisplay(state, false);
            }
        };
        AircraftListPlugin.prototype._sortableHeaderClicked = function (event) {
            var state = this._getState();
            var options = this.options;
            var target = $(event.target);
            var cellIndex = -1;
            $.each(state.headerCells, function (idx, cell) {
                if (cell.is(target)) {
                    cellIndex = idx;
                }
                return cellIndex === -1;
            });
            var handler = cellIndex === -1 ? null : state.columns[cellIndex];
            if (options.sorter && handler.sortableField !== VRS.AircraftListSortableField.None) {
                var existingSortField = options.sorter.getSingleSortField();
                var sortField = existingSortField != null && existingSortField.field == handler.sortableField ? existingSortField : { field: handler.sortableField, ascending: false };
                sortField.ascending = !sortField.ascending;
                options.sorter.setSingleSortField(sortField);
                options.sorter.saveState();
            }
            event.stopPropagation();
            return false;
        };
        return AircraftListPlugin;
    }(JQueryUICustomWidget));
    VRS.AircraftListPlugin = AircraftListPlugin;
    $.widget('vrs.vrsAircraftList', new AircraftListPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.aircraftList.js.map