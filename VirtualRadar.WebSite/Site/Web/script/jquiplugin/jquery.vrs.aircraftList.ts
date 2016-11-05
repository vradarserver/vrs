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
 * @fileoverview A jQueryUI plugin that can manage the aircraft list panel.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.listPluginDistinguishOnGround = VRS.globalOptions.listPluginDistinguishOnGround !== undefined ? VRS.globalOptions.listPluginDistinguishOnGround : true;               // True if altitudes should, by default, distinguish between a value of 0 and aircraft that are on the ground.
    VRS.globalOptions.listPluginFlagUncertainCallsigns = VRS.globalOptions.listPluginFlagUncertainCallsigns !== undefined ? VRS.globalOptions.listPluginFlagUncertainCallsigns : true;      // True if uncertain callsigns should, by default, be flagged in the aircraft list.
    VRS.globalOptions.listPluginUserCanConfigureColumns = VRS.globalOptions.listPluginUserCanConfigureColumns !== undefined ? VRS.globalOptions.listPluginUserCanConfigureColumns : true;   // True if the user can configure the aircraft list columns.

    if(VRS.globalOptions.isMobile) {
        VRS.globalOptions.listPluginDefaultColumns = VRS.globalOptions.listPluginDefaultColumns || [            // The default set of columns to show to the user.
            VRS.RenderProperty.SilhouetteAndOpFlag,
            VRS.RenderProperty.RegistrationAndIcao,
            VRS.RenderProperty.CallsignAndShortRoute,
            VRS.RenderProperty.AltitudeAndVerticalSpeed,
            VRS.RenderProperty.Speed
        ];
        VRS.globalOptions.listPluginDefaultShowUnits = VRS.globalOptions.listPluginDefaultShowUnits !== undefined ? VRS.globalOptions.listPluginDefaultShowUnits : false;                   // True if units should be shown by default.
    } else if(VRS.globalOptions.isFlightSim) {
        VRS.globalOptions.listPluginDefaultColumns = VRS.globalOptions.listPluginDefaultColumns || [            // The default set of columns to show to the user.
            VRS.RenderProperty.Registration,
            VRS.RenderProperty.Callsign,
            VRS.RenderProperty.ModelIcao,
            VRS.RenderProperty.Altitude,
            VRS.RenderProperty.VerticalSpeed,
            VRS.RenderProperty.Squawk,
            VRS.RenderProperty.Speed
        ];
        VRS.globalOptions.listPluginDefaultShowUnits = VRS.globalOptions.listPluginDefaultShowUnits !== undefined ? VRS.globalOptions.listPluginDefaultShowUnits : true;                    // True if units should be shown by default.
    } else {
        VRS.globalOptions.listPluginDefaultColumns = VRS.globalOptions.listPluginDefaultColumns || [            // The default set of columns to show to the user.
            VRS.RenderProperty.Silhouette,
            VRS.RenderProperty.OperatorFlag,
            VRS.RenderProperty.Registration,
            VRS.RenderProperty.Icao,
            VRS.RenderProperty.Callsign,
            VRS.RenderProperty.RouteShort,
            VRS.RenderProperty.Altitude,
            VRS.RenderProperty.Speed
        ];
        VRS.globalOptions.listPluginDefaultShowUnits = VRS.globalOptions.listPluginDefaultShowUnits !== undefined ? VRS.globalOptions.listPluginDefaultShowUnits : true;                    // True if units should be shown by default.
    }
    VRS.globalOptions.listPluginShowSorterOptions = VRS.globalOptions.listPluginShowSorterOptions !== undefined ? VRS.globalOptions.listPluginShowSorterOptions : true;                     // True if sorter options should be shown on the list panel.
    VRS.globalOptions.listPluginShowPause = VRS.globalOptions.listPluginShowPause !== undefined ? VRS.globalOptions.listPluginShowPause : true;                                             // True if a pause link should be shown on the list, false if it should not.
    VRS.globalOptions.listPluginShowHideAircraftNotOnMap = VRS.globalOptions.listPluginShowHideAircraftNotOnMap !== undefined ? VRS.globalOptions.listPluginShowHideAircraftNotOnMap : true; // True if the link to hide aircraft not on map should be shown.

    type RowStateBitFlags = number;
    /**
     * An enumeration of the different states an aircraft's row can be in.
     */
    var RowState = {
        Odd:                0x0001,
        Even:               0x0002,
        Selected:           0x0004,
        Emergency:          0x0008,
        FirstRow:           0x0010,
        Interested:         0x0020
    }

    /**
     * Carries information about a row in the list.
     */
    interface RowData_Row
    {
        aircraftId:   number;
        rowState?:    RowStateBitFlags;
        visible:      boolean;
    }

    /**
     * Carries information about a cell in a row.
     */
    interface RowData_Cell
    {
        alignment?:       AlignmentEnum;
        fixedWidth?:      string;
        hasTitle?:        boolean;
        value?:           any;
        widgetProperty?:  RenderPropertyEnum;
    }

    /**
     * Carries information about the row and the row's cells.
     */
    interface RowData
    {
        row:    RowData_Row;
        cells:  RowData_Cell[];
    }

    /**
     * The values that AircraftListPlugin persists between sessions.
     */
    export interface AircraftListPlugin_SaveState
    {
        columns: RenderPropertyEnum[];
        showUnits: boolean;
    }

    /**
     * The state object for the aircraft list plugin.
     */
    class AircraftListPlugin_State
    {
        /**
         * A jQuery element for the container for the list.
         */
        container: JQuery = undefined;

        /**
         * A jQuery element for the table that the list is rendered through.
         */
        table: JQuery = undefined;

        /**
         * A jQuery element for the thead of the table.
         */
        tableHeader: JQuery = undefined;

        /**
         * A jQuery element for the tbody of the table.
         */
        tableBody: JQuery = undefined;

        /**
         * An array of VRS.RenderPropertyHandlers for each column in the table, in display order.
         */
        columns: RenderPropertyHandler[] = [];

        /**
         * An array of jQuery elements for each cell in the table header.
         */
        headerCells: JQuery[] = [];

        /**
         * The data associated with each row in the table and the cells for each row. They are not attached to the DOM via jQuery as it was a bit slow.
         */
        rowData: RowData[] = [];

        /**
         * A jQuery element holding the 'tracking x aircraft' text.
         */
        trackingAircraftElement: JQuery = undefined;

        /**
         * A count of the number of aircraft tracked by the server.
         */
        trackingAircraftCount: number = -1;

        /**
         * A count of the number of aircraft received in the last update. May be less than the number tracked.
         */
        availableAircraftCount: number = -1;

        /**
         * A reference to the object that can display a pause link.
         */
        pauseLinkRenderer: PauseLinkRenderHandler = null;

        /**
         * A reference to the object that can display a link to hide aircraft not on the map.
         */
        hideAircraftNotOnMapLinkRenderer: HideAircraftNotOnMapLinkRenderHandler = null;

        /**
         * The jQuery element that the links are rendered into
         */
        linksElement: JQuery = null;

        /**
         * A direct references to the plugin that is rendering the links for us.
         */
        linksPlugin: AircraftLinksPlugin = null;

        /**
         * A jQuery element holding the 'Powered by Virtual Radar Server' text.
         */
        poweredByElement: JQuery = undefined;

        /**
         * The hook result for the VRS.AircraftList.updated event.
         */
        aircraftListUpdatedHook: IEventHandle = undefined;

        /**
         * The hook result for the VRS.AircraftList.selectedAircraftChanged event.
         */
        selectedAircraftChangedHook: IEventHandle = undefined;

        /**
         * The hook result for the VRS.Localise.localeChanged event.
         */
        localeChangedHook: IEventHandle = undefined;

        /**
         * The hook result for the VRS.UnitDisplayPreferences.unitChanged event.
         */
        unitChangedHook: IEventHandle = undefined;

        /**
         * The hook result for the VRS.AircraftListSorter.sortFieldsChanged event.
         */
        sortFieldsChangedHook: IEventHandle = undefined;

        /**
         * True if the aircraft list is not to respond to updates.
         */
        suspended = false;
    }

    /*
     * jQueryUIHelper
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftListPlugin = function(jQueryElement: JQuery) : AircraftListPlugin
    {
        return <AircraftListPlugin>jQueryElement.data('vrsVrsAircraftList');
    }
    VRS.jQueryUIHelper.getAircraftListOptions = function(overrides?: AircraftListPlugin_Options) : AircraftListPlugin_Options
    {
        return $.extend({
            name:                   'default',
            sorter:                 null,
            showSorterOptions:      VRS.globalOptions.listPluginShowSorterOptions,
            columns:                VRS.globalOptions.listPluginDefaultColumns.slice(),
            useSavedState:          true,
            useSorterSavedState:    true,
            showUnits:              VRS.globalOptions.listPluginDefaultShowUnits,
            distinguishOnGround:    VRS.globalOptions.listPluginDistinguishOnGround,
            flagUncertainCallsigns: VRS.globalOptions.listPluginFlagUncertainCallsigns,
            showPause:              VRS.globalOptions.listPluginShowPause,
            showHideAircraftNotOnMap: VRS.globalOptions.listPluginShowHideAircraftNotOnMap
        }, overrides);
    }

    /**
     * The options that AircraftListPlugin supports.
     */
    export interface AircraftListPlugin_Options
    {
        /**
         * The name to use when saving and loading state.
         */
        name?: string;

        /**
         * The VRS.AircraftList whose content is to be displayed by the control.
         */
        aircraftList: AircraftList;

        /**
         * Optional - if missing then the pause link is not rendered.
         */
        aircraftListFetcher?: AircraftListFetcher;

        /**
         * The VRS.UnitDisplayPreferences that dictate how values are to be displayed.
         */
        unitDisplayPreferences: UnitDisplayPreferences;

        /**
         * The VRS.AircraftListSorter that will sort the list for us.
         */
        sorter?: AircraftListSorter;

        /**
         * True if the sorter's options pane should be shown in amongst the plugin's options pane
         */
        showSorterOptions?: boolean;

        /**
         * An array of columns to show to the user.
         */
        columns?: RenderPropertyEnum[];

        /**
         * True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
         */
        useSavedState?: boolean;

        /**
         * True if the plugin is responsible for loading the sorter's state.
         */
        useSorterSavedState?: boolean;

        /**
         * True if heights, distances and speeds should indicate their units.
         */
        showUnits?: boolean;

        /**
         * True if altitudes should distinguish between altitudes of zero and aircraft on the ground.
         */
        distinguishOnGround?: boolean;

        /**
         * True if callsigns that may not be correct should be flagged up.
         */
        flagUncertainCallsigns?: boolean;

        /**
         * True if the pause link should be shown.
         */
        showPause?: boolean;

        /**
         * True if the link to hide aircraft not on the map should be shown.
         */
        showHideAircraftNotOnMap?: boolean;
    }

    /**
     * A jQuery UI widget that displays the aircraft list.
     */
    export class AircraftListPlugin extends JQueryUICustomWidget implements ISelfPersist<AircraftListPlugin_SaveState>
    {
        options: AircraftListPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getAircraftListOptions();
        }

        private _getState() : AircraftListPlugin_State
        {
            var result = this.element.data('aircraftListPluginState');
            if(result === undefined) {
                result = new AircraftListPlugin_State();
                this.element.data('aircraftListPluginState', result);
            }

            return result;
        }

        _create()
        {
            var options = this.options;
            if(!options.aircraftList) throw 'An aircraft list must be supplied';
            if(!options.unitDisplayPreferences) throw 'A unit display preferences object must be supplied';

            if(options.useSavedState) this.loadAndApplyState();
            if(options.useSorterSavedState) this.options.sorter.loadAndApplyState();

            var state = this._getState();
            state.container = $('<div/>')
                .addClass('vrsAircraftList')
                .appendTo(this.element);
            state.trackingAircraftElement = $('<p/>')
                .addClass('count')
                .appendTo(state.container);
            if((options.showPause || options.showHideAircraftNotOnMap)  && options.aircraftListFetcher) {
                var linkSites = [];

                if(options.showPause) {
                    state.pauseLinkRenderer = new VRS.PauseLinkRenderHandler(options.aircraftListFetcher);
                    linkSites.push(state.pauseLinkRenderer);
                }
                if(options.showHideAircraftNotOnMap) {
                    state.hideAircraftNotOnMapLinkRenderer = new VRS.HideAircraftNotOnMapLinkRenderHandler(options.aircraftListFetcher);
                    linkSites.push(state.hideAircraftNotOnMapLinkRenderer);
                }

                state.linksElement = $('<div/>')
                    .addClass('links')
                    .vrsAircraftLinks(VRS.jQueryUIHelper.getAircraftLinksOptions({ linkSites: linkSites }))
                    .appendTo(state.container);
                state.linksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(state.linksElement);

                if(state.pauseLinkRenderer)                 state.pauseLinkRenderer.addLinksRendererPlugin(state.linksPlugin);
                if(state.hideAircraftNotOnMapLinkRenderer)  state.hideAircraftNotOnMapLinkRenderer.addLinksRendererPlugin(state.linksPlugin);

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
        }

        _destroy()
        {
            var state = this._getState();
            var options = this.options;
            if(state.linksElement) {
                if(state.linksPlugin) state.linksPlugin.destroy();
                state.linksElement.remove();
                if(state.pauseLinkRenderer) state.pauseLinkRenderer.dispose();
                if(state.hideAircraftNotOnMapLinkRenderer) state.hideAircraftNotOnMapLinkRenderer.dispose();

                state.linksElement = state.linksPlugin = state.pauseLinkRenderer = state.hideAircraftNotOnMapLinkRenderer = null;
            }
            if(state.container) {
                this.element.empty();
                state.container = state.table = state.tableHeader = state.tableBody = undefined;
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
            if(state.sortFieldsChangedHook) {
                if(options && options.sorter) options.sorter.unhook(state.sortFieldsChangedHook);
                state.sortFieldsChangedHook = undefined;
            }

            // Hiding all of the rows in the table will destroy any jQueryUI widgets that anyone was mad enough to use
            $.each(state.headerCells, function(idx, cell) {
                cell.off();
            });
            var tbody = state.tableBody;
            var tbodyElement = <HTMLTableSectionElement> tbody[0];
            var tbodyRows = tbodyElement.rows;
            var countRows = tbodyRows.length;
            var dataArray = state.rowData;
            for(var hideRowNum = 0;hideRowNum < countRows;++hideRowNum) {
                this._showRow(state, <HTMLTableRowElement> tbodyRows[hideRowNum], dataArray[hideRowNum], false);
            }

            this.element.empty();
        }

        /**
         * Saves the current state.
         */
        saveState()
        {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        }

        /**
         * Returns the previously saved state or the current state if there is no saved state.
         */
        loadState() : AircraftListPlugin_SaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);

            if(!result.columns) result.columns = VRS.globalOptions.listPluginDefaultColumns.slice();
            result.columns = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.columns, [ VRS.RenderSurface.List ] );

            return result;
        }

        /**
         * Applies the previously saved state to the object.
         */
        applyState(settings: AircraftListPlugin_SaveState)
        {
            this.options.columns = settings.columns;
            this.options.showUnits = settings.showUnits;
        }

        /**
         * Loads and applies the previously saved state to the object.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key to save the state against.
         */
        private _persistenceKey() : string
        {
            return 'vrsAircraftListPlugin-' + this.options.name;
        }

        /**
         * Returns the current state.
         */
        private _createSettings() : AircraftListPlugin_SaveState
        {
            return {
                columns: this.options.columns || [],
                showUnits: this.options.showUnits
            };
        }

        /**
         * Creates the option panes for configuring the object.
         */
        createOptionPane(displayOrder: number) : OptionPane[]
        {
            var result: OptionPane[] = [];

            if(this.options.sorter && this.options.showSorterOptions) {
                var sorterOptions = this.options.sorter.createOptionPane(displayOrder);
                if(VRS.arrayHelper.isArray(sorterOptions)) {
                    $.each(sorterOptions, function(idx, pane) { result.push(pane); });
                }
                else {
                    result.push(sorterOptions);
                }
            }

            var settingsPane = new VRS.OptionPane({
                name:           'vrsAircraftListPlugin_' + this.options.name + '_Settings',
                titleKey:       'PaneListSettings',
                displayOrder:   displayOrder + 10,
                fields:         [
                    new VRS.OptionFieldCheckBox({
                        name:           'showUnits',
                        labelKey:       'ShowUnits',
                        getValue:       () => this.options.showUnits,
                        setValue:       (value) => {
                            this.options.showUnits = value;
                            this._buildTable(this._getState());
                        },
                        saveState:      () => this.saveState()
                    })
                ]
            });

            if(VRS.globalOptions.listPluginUserCanConfigureColumns) {
                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane:       settingsPane,
                    fieldLabel: 'Columns',
                    surface:    VRS.RenderSurface.List,
                    getList:    () => this.options.columns,
                    setList:    (value) => {
                        this.options.columns = value;
                        this._buildTable(this._getState());
                    },
                    saveState:  () => this.saveState()
                });
            }

            result.push(settingsPane);

            return result;
        }

        /**
         * Suspends updates when the aircraft list is updated.
         */
        suspend(onOff: boolean)
        {
            var state = this._getState();
            if(state.suspended !== onOff) {
                state.suspended = onOff;
                if(!state.suspended) {
                    this._refreshDisplay(state, true);
                }
            }
        }

        /**
         * Prepends a jQuery element to the aircraft list.
         */
        prependElement(elementJQ: JQuery)
        {
            var state = this._getState();
            state.container.prepend(elementJQ);
        }

        /**
         * Builds up the table, refreshing content if required.
         */
        private _buildTable(state: AircraftListPlugin_State)
        {
            this._buildHeader(state);
            this._refreshDisplay(state, true);
        }

        /**
         * Builds up the header row for the table.
         */
        private _buildHeader(state: AircraftListPlugin_State)
        {
            $.each(state.headerCells, function(idx, cell) {
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

            for(var i = 0;i < countColumns;++i) {
                var columnId = columns[i];
                var handler = VRS.renderPropertyHandlers[columnId];
                if(!handler) throw 'No handler has been registered for property ID ' + columnId;

                var headingText = handler.suppressLabelCallback(VRS.RenderSurface.List) ? '' : VRS.globalisation.getText(handler.headingKey);

                var cell = $('<th/>')
                    .text(headingText)
                    .appendTo(trow);
                if(i + 1 == countColumns) cell.addClass('lastColumn');

                state.columns.push(handler);
                state.headerCells.push(cell);

                if(handler.sortableField !== VRS.AircraftListSortableField.None) {
                    cell.addClass('sortHeader');
                    cell.on('click', $.proxy(this._sortableHeaderClicked, this));
                }

                var data = {};
                this._setCellAlignment(cell, data, handler.headingAlignment);
                this._setCellWidth(cell, data, handler.fixedWidth(VRS.RenderSurface.List));
            }
        }

        /**
         * Updates the display to reflect changes in the aircraft list, sort order etc.
         */
        private _refreshDisplay(state: AircraftListPlugin_State, refreshAll: boolean, displayUnitChanged?: DisplayUnitDependencyEnum)
        {
            if(refreshAll) this._updatePoweredByCredit(state);
            this._updateTrackedAircraftCount(state, refreshAll);
            this._buildRows(state, refreshAll, displayUnitChanged);
        }

        /**
         * Refreshes the display of the 'Powered by Virtual Radar Server' credits.
         */
        private _updatePoweredByCredit(state: AircraftListPlugin_State)
        {
            var version = VRS.serverConfig.get().VrsVersion;

            state.poweredByElement
                .empty()
                .append($('<a/>')
                    .attr('href', 'http://www.virtualradarserver.co.uk/')
                    .attr('target', 'external')
                    .attr('title', VRS.stringUtility.format(VRS.$$.VrsVersion, version))
                    .text(VRS.$$.PoweredByVRS)
                );
        }

        /**
         * Refreshes the display of tracked aircraft.
         */
        private _updateTrackedAircraftCount(state: AircraftListPlugin_State, refreshAll: boolean)
        {
            var trackedAircraft = this.options.aircraftList.getCountTrackedAircraft();
            var availableAircraft = this.options.aircraftList.getCountAvailableAircraft();

            if(refreshAll || state.trackingAircraftCount !== trackedAircraft || state.availableAircraftCount !== availableAircraft) {
                var text = '';
                if(availableAircraft == 1) {
                    text = trackedAircraft == availableAircraft ? VRS.$$.TrackingOneAircraft
                                                                : VRS.stringUtility.format(VRS.$$.TrackingOneAircraftOutOf, trackedAircraft);
                } else {
                    text = trackedAircraft == availableAircraft ? VRS.stringUtility.format(VRS.$$.TrackingCountAircraft, availableAircraft)
                                                                : VRS.stringUtility.format(VRS.$$.TrackingCountAircraftOutOf, availableAircraft, trackedAircraft);
                }
                state.trackingAircraftElement.text(text);
                state.trackingAircraftCount = trackedAircraft;
                state.availableAircraftCount = availableAircraft;
            }
        }

        /**
         * Fills in the table with the content of the aircraft list.
         * 
         * I did originally implement this with jQuery but it was painfully slow on large data feeds, so instead it
         * fiddles with the DOM directly. To also improve performance the array handling methods are not plugin
         * methods (each call on which involves widget code) - instead they're inline functions. It does make the
         * code a bit ugly to read, sorry about that.
         *
         * Note that property renderers can, in principle, be jQueryUI widgets. However the use of jQueryUI widgets
         * for the list surface should be discouraged as it will kill performance on large aircraft lists.
         */
        private _buildRows(state: AircraftListPlugin_State, refreshAll: boolean, displayUnitChanged?: DisplayUnitDependencyEnum)
        {
            var aircraftList = this.options.aircraftList.toList();
            var options = this.options;
            if(options.sorter) options.sorter.sortAircraftArray(aircraftList, options.unitDisplayPreferences);
            var columns = this.options.columns;
            var columnCount = columns.length;
            var tbody = state.tableBody;
            var dataArray = state.rowData;

            var tbodyElement = <HTMLTableSectionElement>state.tableBody[0];
            var tbodyRows = tbodyElement.rows;
            if(tbodyRows.length !== dataArray.length) throw 'Assertion failed - state.rowData and tbody.rows are no longer in sync';

            if(refreshAll) {
                this._ensureBodyRowsHaveCorrectNumberOfColumns(tbodyRows, dataArray, columnCount);
            }

            var createRow = (id: number) => {
                var row = <HTMLTableRowElement>tbodyElement.insertRow(-1);
                var data = { row: { aircraftId: id, visible: true, rowState: undefined }, cells: [] };
                var self = this;
                $(row).click(function(e) { self._rowClicked(e, this); });
                dataArray.push(data);
                for(var i = 0;i < columnCount;++i) {
                    var cell = <HTMLTableCellElement>row.insertCell(-1);
                    data.cells.push({});

                    if(i + 1 == columnCount) {
                        VRS.domHelper.setClass(cell, 'lastColumn');
                    }
                }
            };

            // Fill in rows for each aircraft in display order
            var countAircraft = aircraftList.length;
            var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
            for(var rowNum = 0;rowNum < countAircraft;++rowNum) {
                var aircraft = aircraftList[rowNum];
                var refreshRow = refreshAll;                // true if the row content should be filled even if it looks like it hasn't changed
                var refreshCellProperties = refreshAll;     // true if the cell's alignment etc. should be set

                // Ensure that there is a row in the correct location for this aircraft
                if(rowNum >= dataArray.length) {
                    createRow(aircraft.id);
                    refreshCellProperties = refreshRow = true;
                }

                var row = <HTMLTableRowElement>tbodyRows[rowNum];
                var data = dataArray[rowNum];
                var rowDataRow = data.row;

                if(rowDataRow.aircraftId !== aircraft.id) {
                    rowDataRow.aircraftId = aircraft.id;
                    refreshCellProperties = refreshRow = true;
                }
                if(rowDataRow.visible !== undefined && !rowDataRow.visible) refreshRow = true;

                // Fill in the content for each cell
                var cells = row.cells;
                for(var colNum = 0;colNum < columnCount;++colNum) {
                    var handler = state.columns[colNum];
                    var cell = <HTMLTableCellElement>cells[colNum];
                    var cellData = data.cells[colNum];

                    var contentChanged = refreshRow;
                    if(!contentChanged) {
                        if(displayUnitChanged) {
                            contentChanged = handler.usesDisplayUnit(displayUnitChanged);
                        } else {
                            contentChanged = handler.hasChangedCallback(aircraft);
                        }
                    }

                    var tooltipChanged = refreshRow;
                    if(!tooltipChanged) tooltipChanged = handler.tooltipChangedCallback(aircraft);

                    if(refreshCellProperties) {
                        this._createWidgetRenderer(cell, cellData, handler);
                        this._setCellAlignment(cell, cellData, handler.contentAlignment);
                        this._setCellWidth(cell, cellData, handler.fixedWidth(VRS.RenderSurface.List));
                    }
                    if(contentChanged) {
                        cellData.value = handler.renderToDom(cell, VRS.RenderSurface.List, aircraft, options, true, cellData.value);
                    }
                    if(tooltipChanged) {
                        cellData.hasTitle = handler.renderTooltipToDom(cell, VRS.RenderSurface.List, aircraft, options);
                    }
                }

                this._setRowState(row, rowDataRow, rowNum, aircraft === selectedAircraft, aircraft.isEmergency.val, aircraft.userInterested.val);
                this._showRow(state, row, data, true);
            }

            // Any rows left by this point are for aircraft that are no longer on radar, we need to hide these for reuse later
            var countRows = tbodyRows.length;
            for(var hideRowNum = countAircraft;hideRowNum < countRows;++hideRowNum) {
                this._showRow(state, <HTMLTableRowElement>tbodyRows[hideRowNum], dataArray[hideRowNum], false);
            }
        }

        /**
         * Adds or removes cells on each row to match the column count passed across.
         */
        private _ensureBodyRowsHaveCorrectNumberOfColumns(tbodyRows: HTMLCollection, dataArray: RowData[], columnCount: number)
        {
            var length = tbodyRows.length;
            if(length > 0 && dataArray[0].cells.length !== columnCount) {
                var addCells = columnCount - dataArray[0].cells.length;
                for(var r = 0;r < length;++r) {
                    var data = dataArray[r];
                    var row = <HTMLTableRowElement>tbodyRows[r];

                    if(addCells > 0) {
                        for(let i = 0;i < addCells;++i) {
                            data.cells.push({});
                            row.insertCell(-1);
                        }
                    } else {
                        // Remove widgets from the cells being removed. In principle this should never be called, the
                        // use of widgets in the list should be discouraged!
                        var cellsLength = data.cells.length;
                        for(let i = data.cells.length + addCells;i < cellsLength;++i) {
                            var cellData = data.cells[i];
                            if(cellData.widgetProperty) {
                                this._destroyWidgetRenderer(<HTMLTableCellElement>(row.cells[i]), cellData);
                            }
                        }

                        data.cells.splice(cellsLength + addCells, -addCells);
                        for(let i = 0;i > addCells;--i) {
                            row.deleteCell(-1);
                        }
                    }
                }
            }
        }

        /**
         * Sets the alignment on a cell. Works with both jQuery objects and HTMLTableCellElements.
         */
        private _setCellAlignment(cell: HTMLTableCellElement | JQuery, cellData: RowData_Cell, alignment: AlignmentEnum)
        {
            if(!cellData.alignment || cellData.alignment !== alignment) {
                var isDOM = cell instanceof HTMLTableCellElement;
                var addClass =      function(classNames) { isDOM ? VRS.domHelper.addClasses(<HTMLTableCellElement>cell, [classNames]) : (<JQuery>cell).addClass(classNames); };
                var removeClass =   function(classNames) { isDOM ? VRS.domHelper.removeClasses(<HTMLTableCellElement>cell, [classNames]) : (<JQuery>cell).removeClass(classNames); };

                if(cellData.alignment) {
                    switch(cellData.alignment) {
                        case VRS.Alignment.Left:    break;
                        case VRS.Alignment.Centre:  removeClass('vrsCentre'); break;
                        case VRS.Alignment.Right:   removeClass('vrsRight'); break;
                        default:                    throw 'Unknown alignment ' + cellData.alignment;
                    }
                }

                switch(alignment) {
                    case VRS.Alignment.Left:    break;
                    case VRS.Alignment.Centre:  addClass('vrsCentre'); break;
                    case VRS.Alignment.Right:   addClass('vrsRight'); break;
                    default:                    throw 'Unknown alignment ' + alignment;
                }

                cellData.alignment = alignment;
            }
        }

        /**
         * Sets the fixed width on a cell. Works with both jQuery objects and HTMLTableCellElements.
         */
        private _setCellWidth(cell: HTMLTableCellElement | JQuery, cellData: RowData_Cell, fixedWidth: string)
        {
            if(!cellData.fixedWidth || cellData.fixedWidth !== fixedWidth) {
                var isDOM = cell instanceof HTMLTableCellElement;

                if(cellData.fixedWidth) {
                    isDOM ? VRS.domHelper.removeAttribute(<HTMLTableCellElement>cell, 'width') : (<JQuery>cell).removeAttr('width');
                    isDOM ? VRS.domHelper.removeAttribute(<HTMLTableCellElement>cell, 'max-width') : (<JQuery>cell).removeAttr('max-width');
                    isDOM ? VRS.domHelper.removeClasses(<HTMLTableCellElement>cell, ['fixedWidth']) : (<JQuery>cell).removeClass('fixedWidth');
                }
                if(fixedWidth) {
                    isDOM ? VRS.domHelper.setAttribute(<HTMLTableCellElement>cell, 'width', fixedWidth) : (<JQuery>cell).attr('width', fixedWidth);
                    isDOM ? VRS.domHelper.setAttribute(<HTMLTableCellElement>cell, 'max-width', fixedWidth) : (<JQuery>cell).attr('max-width', fixedWidth);
                    isDOM ? VRS.domHelper.addClasses(<HTMLTableCellElement>cell, ['fixedWidth']) : (<JQuery>cell).addClass('fixedWidth');
                }
                cellData.fixedWidth = fixedWidth;
            }
        }

        /**
         * Shows or hides a row. Works with both jQuery objects and HTMLTableRowElements.
         */
        private _showRow(state: AircraftListPlugin_State, row: HTMLTableRowElement | JQuery, rowData: RowData, visible: boolean)
        {
            if(rowData.row.visible === undefined || rowData.row.visible !== visible) {
                var isDOM = row instanceof HTMLTableRowElement;
                if(!visible) isDOM ? VRS.domHelper.addClasses(<HTMLTableRowElement>row, ['hidden']) : (<JQuery>row).addClass('hidden');
                else         isDOM ? VRS.domHelper.removeClasses(<HTMLTableRowElement>row, ['hidden']) : (<JQuery>row).removeClass('hidden');

                if(!visible) {
                    var cells: HTMLCollection = isDOM ? (<HTMLTableRowElement>row).cells : row[0].cells;
                    var countCells = cells.length;
                    for(var i = 0;i < countCells;++i) {
                        var cell = <HTMLTableCellElement>cells[i];
                        var cellData = rowData.cells[i];

                        if(cellData.widgetProperty) {
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
        }

        /**
         * Sets the state of a row and applies or removes classes as necessary to get the display matching the state.
         */
        private _setRowState(row: HTMLTableRowElement | JQuery, rowData: RowData_Row, rowNumber: number, isSelectedAircraft: boolean, isEmergency: boolean, isInterested: boolean)
        {
            var rowState = isSelectedAircraft ? RowState.Selected
                : rowNumber % 2 === 0 ? RowState.Odd
                : RowState.Even;

            if(isEmergency)     rowState |= RowState.Emergency;
            if(isInterested)    rowState |= RowState.Interested;
            if(rowNumber === 0) rowState |= RowState.FirstRow;

            var getRowStateClass = function(value) {
                var result = [];

                if(value & RowState.FirstRow)       result.push('firstRow');
                if(value & RowState.Interested)     result.push('interested');
                if(value & RowState.Selected) {
                    if(value & RowState.Emergency)  result.push('vrsSelectedEmergency');
                    else                            result.push('vrsSelected');
                } else {
                    if(value & RowState.Emergency)  result.push('vrsEmergency');
                    else if(value & RowState.Even)  result.push('vrsEven');
                    else                            result.push('vrsOdd');
                }

                return result;
            };

            if(!rowData.rowState || rowData.rowState !== rowState) {
                var isDOM = row instanceof HTMLTableRowElement;
                if(rowData.rowState) {
                    var removeClasses = getRowStateClass(rowData.rowState);
                    isDOM ? VRS.domHelper.removeClasses(<HTMLTableRowElement>row, removeClasses) : (<JQuery>row).removeClass(removeClasses.join(' '));
                }

                var addClasses = getRowStateClass(rowState);
                isDOM ? VRS.domHelper.addClasses(<HTMLTableRowElement>row, addClasses) : (<JQuery>row).addClass(addClasses.join(' '));
                rowData.rowState = rowState;
            }
        }

        /**
         * Creates a widget in the cell for properties that are rendered by a jQuery widget. Their use in lists should
         * be carefully considered, introducing jQuery into the list REALLY slows it down.
         */
        private _createWidgetRenderer(cell: HTMLElement, cellData: RowData_Cell, handler: RenderPropertyHandler)
        {
            if(handler.isWidgetProperty() && cellData.widgetProperty !== handler.property) {
                var cellJQ = $(cell);
                if(cellData.widgetProperty) handler.destroyWidgetInJQuery(cellJQ, VRS.RenderSurface.List);
                handler.createWidgetInJQuery(cellJQ, VRS.RenderSurface.List);
                cellData.widgetProperty = handler.property;
            }
        }

        /**
         * Destroys the widget attached to a cell.
         */
        private _destroyWidgetRenderer(cell: HTMLElement, cellData: RowData_Cell, handler?: RenderPropertyHandler)
        {
            if(cellData.widgetProperty) {
                if(!handler || handler.property !== cellData.widgetProperty) {
                    handler = VRS.renderPropertyHandlers[cellData.widgetProperty];
                }
                handler.destroyWidgetInDom(cell, VRS.RenderSurface.List);
                cellData.widgetProperty = undefined;
            }
        }

        /**
         * Returns the index of the row holding the aircraft passed across or -1 if no such row exists.
         */
        private _getRowIndexForAircraftId(state: AircraftListPlugin_State, aircraftId: number) : number
        {
            var result = -1;

            var length = state.rowData.length;
            for(var i = 0;i < length;++i) {
                if(state.rowData[i].row.aircraftId === aircraftId) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * Returns the index of the row holding the row element passed across or -1 if no such row exists.
         */
        private _getRowIndexForRowElement(state: AircraftListPlugin_State, rowElement: HTMLTableRowElement) : number
        {
            var result = -1;

            var tbodyRows = (<HTMLTableSectionElement>state.tableBody[0]).rows;
            var rowCount = tbodyRows.length;
            for(var i = 0;i < rowCount;++i) {
                if(tbodyRows[i] === rowElement) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /**
         * The event handler for the VRS.AircraftList.updated event.
         */
        private _aircraftListUpdated()
        {
            var state = this._getState();
            if(!state.suspended) {
                this._refreshDisplay(state, false);
            }
        }

        /**
         * Called when the locale is changed.
         */
        private _localeChanged()
        {
            var state = this._getState();
            this._buildHeader(state);
            if(!state.suspended) {
                this._refreshDisplay(state, true);
            }
        }

        /**
         * Called when the user changes a display unit.
         */
        private _displayUnitChanged(displayUnitChanged: DisplayUnitDependencyEnum)
        {
            var state = this._getState();
            if(!state.suspended) {
                this._refreshDisplay(state, false, displayUnitChanged);
            }
        }

        /**
         * Called when the user clicks a row.
         */
        private _rowClicked(event: Event, target: HTMLTableRowElement)
        {
            if(target) {
                var state = this._getState();
                var clickedRowIndex = this._getRowIndexForRowElement(state, target);

                if(clickedRowIndex !== -1 && clickedRowIndex < state.rowData.length) {
                    var rowData = state.rowData[clickedRowIndex];
                    var aircraftId = rowData.row.aircraftId;
                    var aircraft = this.options.aircraftList.findAircraftById(aircraftId);
                    if(aircraft) this.options.aircraftList.setSelectedAircraft(aircraft, true);

                    VRS.globalDispatch.raise(VRS.globalEvent.displayUpdated);
                }
            }
        }

        /**
         * Called when the selected aircraft changes.
         */
        private _selectedAircraftChanged(oldSelectedAircraft: Aircraft)
        {
            var state = this._getState();
            var options = this.options;
            var tbodyRows = (<HTMLTableSectionElement>state.tableBody[0]).rows;

            var refreshRowStateForAircraft = (aircraft, isSelected) => {
                var rowNum = this._getRowIndexForAircraftId(state, aircraft.id);
                if(rowNum !== -1 && rowNum < tbodyRows.length) {
                    this._setRowState(<HTMLTableRowElement>tbodyRows[rowNum], state.rowData[rowNum].row, rowNum, isSelected, aircraft.isEmergency.val, aircraft.userInterested.val);
                }
            };

            var selectedAircraft = options.aircraftList.getSelectedAircraft();
            if(oldSelectedAircraft) {
                refreshRowStateForAircraft(oldSelectedAircraft, false);
            }
            if(selectedAircraft) {
                refreshRowStateForAircraft(selectedAircraft, true);
            }
        }

        /**
         * Called when the user changes the sort order for the list.
         */
        private _sortFieldsChanged()
        {
            var state = this._getState();
            if(!state.suspended) {
                this._refreshDisplay(state, false);
            }
        }

        /**
         * Called when the user clicks a header on a sortable column.
         */
        private _sortableHeaderClicked(event: Event) : boolean
        {
            var state = this._getState();
            var options = this.options;

            var target = $(event.target);
            var cellIndex = -1;
            $.each(state.headerCells, function(idx, cell) {
                if(cell.is(target)) {
                    cellIndex = idx;
                }
                return cellIndex === -1;
            });

            var handler = cellIndex === -1 ? null : state.columns[cellIndex];

            if(options.sorter && handler.sortableField !== VRS.AircraftListSortableField.None) {
                var existingSortField = options.sorter.getSingleSortField();
                var sortField = existingSortField != null && existingSortField.field == handler.sortableField ? existingSortField : { field: handler.sortableField, ascending: false };
                sortField.ascending = !sortField.ascending;
                options.sorter.setSingleSortField(sortField);
                options.sorter.saveState();
            }

            event.stopPropagation();
            return false;
        }
    }

    $.widget('vrs.vrsAircraftList', new AircraftListPlugin());
}

declare interface JQuery
{
    vrsAircraftList();
    vrsAircraftList(options: VRS.AircraftListPlugin_Options);
    vrsAircraftList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
