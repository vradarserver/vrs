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

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
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
    //endregion

    //region AircraftListPluginState
    /**
     * The state object for the aircraft list plugin.
     * @constructor
     */
    VRS.AircraftListPluginState = function()
    {
        /**
         * A jQuery element for the container for the list.
         * @type {jQuery=}
         */
        this.container = undefined;

        /**
         * A jQuery element for the table that the list is rendered through.
         * @type {jQuery=}
         */
        this.table = undefined;

        /**
         * A jQuery element for the thead of the table.
         * @type {jQuery=}
         */
        this.tableHeader = undefined;

        /**
         * A jQuery element for the tbody of the table.
         * @type {jQuery=}
         */
        this.tableBody = undefined;

        /**
         * An array of VRS.RenderPropertyHandlers for each column in the table, in display order.
         * @type {Array.<VRS.RenderPropertyHandler>}
         */
        this.columns = [];

        /**
         * An array of jQuery elements for each cell in the table header.
         * @type {Array.<jQuery>}
         */
        this.headerCells = [];

        /**
         * The data associated with each row in the table and the cells for each row. They are not attached to the DOM via jQuery as it was a bit slow.
         * @type {Array.<VRS_AIRCRAFTLIST_ROWDATA>}
         */
        this.rowData = [];

        /**
         * A jQuery element holding the 'tracking x aircraft' text.
         * @type {jQuery=}
         */
        this.trackingAircraftElement = undefined;

        /**
         * A count of the number of aircraft tracked by the server.
         * @type {number}
         */
        this.trackingAircraftCount = -1;

        /**
         * A count of the number of aircraft received in the last update. May be less than the number tracked.
         * @type {number}
         */
        this.availableAircraftCount = -1;

        /**
         * A reference to the object that can display a pause link.
         * @type {VRS.PauseLinkRenderHandler}
         */
        this.pauseLinkRenderer = null;

        /**
         * A reference to the object that can display a link to hide aircraft not on the map.
         * @type {VRS.HideAircraftNotOnMapLinkRenderHandler}
         */
        this.hideAircraftNotOnMapLinkRenderer = null;

        /**
         * The jQuery element that the links are rendered into
         * @type {jQuery}
         */
        this.linksElement = null;

        /**
         * A direct references to the plugin that is rendering the links for us.
         * @type {VRS.vrsAircraftLinks}
         */
        this.linksPlugin = null;

        /**
         * A jQuery element holding the 'Powered by Virtual Radar Server' text.
         * @type {jQuery=}
         */
        this.poweredByElement = undefined;

        /**
         * The hook result for the VRS.AircraftList.updated event.
         * @type {object=}
         */
        this.aircraftListUpdatedHook = undefined;

        /**
         * The hook result for the VRS.AircraftList.selectedAircraftChanged event.
         * @type {object=}
         */
        this.selectedAircraftChangedHook = undefined;

        /**
         * The hook result for the VRS.Localise.localeChanged event.
         * @type {object=}
         */
        this.localeChangedHook = undefined;

        /**
         * The hook result for the VRS.UnitDisplayPreferences.unitChanged event.
         * @type {object=}
         */
        this.unitChangedHook = undefined;

        /**
         * The hook result for the VRS.AircraftListSorter.sortFieldsChanged event.
         * @type {object=}
         */
        this.sortFieldsChangedHook = undefined;

        /**
         * True if the aircraft list is not to respond to updates.
         * @type {boolean}
         */
        this.suspended = false;
    };

    /**
     * An enumeration of the different states an aircraft's row can be in.
     * @enum {number}
     */
    VRS.AircraftListPluginState.RowState = {
        Odd:                0x0001,
        Even:               0x0002,
        Selected:           0x0004,
        Emergency:          0x0008,
        FirstRow:           0x0010
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsAircraftList plugin attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsAircraftList}
     */
    VRS.jQueryUIHelper.getAircraftListPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsAircraftList'); };

    /**
     * Returns the default options for the aircraft list with optional overrides.
     * @param {VRS_OPTIONS_AIRCRAFTLIST=} overrides
     * @returns {VRS_OPTIONS_AIRCRAFTLIST}
     */
    VRS.jQueryUIHelper.getAircraftListOptions = function(overrides)
    {
        return $.extend({
            name:                   'default',                                          // The name to use when saving and loading state.
            aircraftList:           undefined,                                          // The VRS.AircraftList whose content is to be displayed by the control.
            aircraftListFetcher:    undefined,                                          // Optional - if missing then the pause link is not rendered.
            unitDisplayPreferences: undefined,                                          // The VRS.UnitDisplayPreferences that dictate how values are to be displayed.
            sorter:                 null,                                               // The VRS.AircraftListSorter that will sort the list for us.
            showSorterOptions:      VRS.globalOptions.listPluginShowSorterOptions,      // True if the sorter's options pane should be shown in amongst the plugin's options pane
            columns:                VRS.globalOptions.listPluginDefaultColumns.slice(), // An array of columns to show to the user.
            useSavedState:          true,                                               // True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
            useSorterSavedState:    true,                                               // True if the plugin is responsible for loading the sorter's state.
            showUnits:              VRS.globalOptions.listPluginDefaultShowUnits,       // True if heights, distances and speeds should indicate their units.
            distinguishOnGround:    VRS.globalOptions.listPluginDistinguishOnGround,    // True if altitudes should distinguish between altitudes of zero and aircraft on the ground.
            flagUncertainCallsigns: VRS.globalOptions.listPluginFlagUncertainCallsigns, // True if callsigns that may not be correct should be flagged up.
            showPause:              VRS.globalOptions.listPluginShowPause,              // True if the pause link should be shown.
            showHideAircraftNotOnMap: VRS.globalOptions.listPluginShowHideAircraftNotOnMap, // True if the link to hide aircraft not on the map should be shown.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsAircraftList
    /**
     * A jQuery UI widget that displays the aircraft list.
     * @namespace VRS.vrsAircraftList
     */
    $.widget('vrs.vrsAircraftList', {
        //region -- options
        options: VRS.jQueryUIHelper.getAircraftListOptions(),
        //endregion

        //region -- _getState, _create etc.
        /**
         * Fetches the state object for the plugin or creates it if it doesn't already exist.
         * @returns {VRS.AircraftListPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('aircraftListPluginState');
            if(result === undefined) {
                result = new VRS.AircraftListPluginState();
                this.element.data('aircraftListPluginState', result);
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
        },

        /**
         * Called when the plugin is destroyed - releases all resources held by the plugin.
         * @private
         */
        _destroy: function()
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
            var tbodyElement = tbody[0];
            var tbodyRows = tbodyElement.rows;
            var countRows = tbodyRows.length;
            var dataArray = state.rowData;
            for(var hideRowNum = 0;hideRowNum < countRows;++hideRowNum) {
                this._showRow(state, tbodyRows[hideRowNum], dataArray[hideRowNum], false);
            }

            this.empty();
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
         * Returns the previously saved state or the current state if there is no saved state.
         * @returns {VRS_STATE_AIRCRAFTLISTPLUGIN}
         */
        loadState: function()
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);

            if(!result.columns) result.columns = VRS.globalOptions.listPluginDefaultColumns.slice();
            result.columns = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.columns, [ VRS.RenderSurface.List ] );

            return result;
        },

        /**
         * Applies the previously saved state to the object.
         * @param {VRS_STATE_AIRCRAFTLISTPLUGIN} settings
         */
        applyState: function(settings)
        {
            this.options.columns = settings.columns;
            this.options.showUnits = settings.showUnits;
        },

        /**
         * Loads and applies the previously saved state to the object.
         */
        loadAndApplyState: function()
        {
            this.applyState(this.loadState());
        },

        /**
         * Returns the key to save the state against.
         * @returns {string}
         * @private
         */
        _persistenceKey: function()
        {
            return 'vrsAircraftListPlugin-' + this.options.name;
        },

        /**
         * Returns the current state.
         * @returns {VRS_STATE_AIRCRAFTLISTPLUGIN}
         * @private
         */
        _createSettings: function()
        {
            return {
                columns: this.options.columns || [],
                showUnits: this.options.showUnits
            };
        },
        //endregion

        //region -- createOptionPane
        /**
         * Creates the option panes for configuring the object.
         * @param {number} displayOrder
         * @returns {Array.<VRS.OptionPane>}
         */
        createOptionPane: function(displayOrder)
        {
            var result = [];
            var self = this;

            if(this.options.sorter && this.options.showSorterOptions) {
                var sorterOptions = this.options.sorter.createOptionPane(displayOrder);
                if(VRS.arrayHelper.isArray(sorterOptions)) $.each(sorterOptions, function(idx, pane) { result.push(pane); });
                else result.push(sorterOptions);
            }

            var settingsPane = new VRS.OptionPane({
                name:           'vrsAircraftListPlugin_' + this.options.name + '_Settings',
                titleKey:       'PaneListSettings',
                displayOrder:   displayOrder + 10,
                fields:         [
                    new VRS.OptionFieldCheckBox({
                        name:           'showUnits',
                        labelKey:       'ShowUnits',
                        getValue:       function() { return self.options.showUnits; },
                        setValue:       function(value) { self.options.showUnits = value; self._buildTable(self._getState()); },
                        saveState:      function() { self.saveState(); }
                    })
                ]
            });

            if(VRS.globalOptions.listPluginUserCanConfigureColumns) {
                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane:       settingsPane,
                    fieldLabel: 'Columns',
                    surface:    VRS.RenderSurface.List,
                    getList:    function() { return self.options.columns; },
                    setList:    function(cols) {
                        self.options.columns = cols;
                        self._buildTable(self._getState());
                    },
                    saveState:  function() { self.saveState(); }
                });
            }

            result.push(settingsPane);

            return result;
        },
        //endregion

        //region -- suspend
        /**
         * Suspends updates when the aircraft list is updated.
         * @param {boolean} onOff
         */
        suspend: function(onOff)
        {
            var state = this._getState();
            if(state.suspended !== onOff) {
                state.suspended = onOff;
                if(!state.suspended) {
                    this._refreshDisplay(state, true);
                }
            }
        },
        //endregion

        //region -- _buildTable, _buildHeader, _refreshDisplay, _buildRows
        /**
         * Builds up the table, refreshing content if required.
         * @param {VRS.AircraftListPluginState} state
         * @private
         */
        _buildTable: function(state)
        {
            this._buildHeader(state);
            this._refreshDisplay(state, true);
        },

        /**
         * Builds up the header row for the table.
         * @param {VRS.AircraftListPluginState} state
         * @private
         */
        _buildHeader: function(state)
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

                var cell = $('<th/>')
                    .text(VRS.globalisation.getText(handler.headingKey))
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
        },

        /**
         * Updates the display to reflect changes in the aircraft list, sort order etc.
         * @param {VRS.AircraftListPluginState} state
         * @param {bool} refreshAll
         * @param {VRS.DisplayUnitDependency} [displayUnitChanged]
         * @private
         */
        _refreshDisplay: function(state, refreshAll, displayUnitChanged)
        {
            if(refreshAll) this._updatePoweredByCredit(state);
            this._updateTrackedAircraftCount(state, refreshAll);
            this._buildRows(state, refreshAll, displayUnitChanged);
        },

        /**
         * Refreshes the display of the 'Powered by Virtual Radar Server' credits.
         * @param {VRS.AircraftListPluginState} state
         * @private
         */
        _updatePoweredByCredit: function(state)
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
        },

        /**
         * Refreshes the display of tracked aircraft.
         * @param {VRS.AircraftListPluginState} state
         * @param {bool} refreshAll
         * @private
         */
        _updateTrackedAircraftCount: function(state, refreshAll)
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
        },

        /**
         * Fills in the table with the content of the aircraft list.<br/>
         * <br/>
         * I did originally implement this with jQuery but it was painfully slow on large data feeds, so instead it
         * fiddles with the DOM directly. To also improve performance the array handling methods are not plugin
         * methods (each call on which involves widget code) - instead they're inline functions. It does make the
         * code a bit ugly to read, sorry about that.<br/>
         * Note that property renderers can, in principle, be jQueryUI widgets. However the use of jQueryUI widgets
         * for the list surface should be strongly discouraged as it will kill performance on large aircraft lists.
         * @param {VRS.AircraftListPluginState}     state
         * @param {bool}                            refreshAll
         * @param {VRS.DisplayUnitDependency}      [displayUnitChanged]
         * @private
         */
        _buildRows: function(state, refreshAll, displayUnitChanged)
        {
            var self = this;
            var aircraftList = this.options.aircraftList.toList();
            var options = this.options;
            if(options.sorter) options.sorter.sortAircraftArray(aircraftList);
            var columns = this.options.columns;
            var columnCount = columns.length;
            var tbody = state.tableBody;
            var dataArray = state.rowData;

            /** @type {HTMLTableSectionElement} */
            var tbodyElement = state.tableBody[0];
            var tbodyRows = tbodyElement.rows;
            if(tbodyRows.length !== dataArray.length) throw 'Assertion failed - state.rowData and tbody.rows are no longer in sync';

            if(refreshAll) this._ensureBodyRowsHaveCorrectNumberOfColumns(tbodyRows, dataArray, columnCount);

            var createRow = function(/** number */ id) {
                var row = tbodyElement.insertRow(-1);
                var data = { row: { aircraftId: id, visible: true }, cells: [] };
                $(row).click(function(e) { self._rowClicked(e, this); });
                dataArray.push(data);
                for(var i = 0;i < columnCount;++i) {
                    /** @type {HTMLElement} */ var cell = row.insertCell(-1);
                    data.cells.push({});

                    if(i + 1 == columnCount) VRS.domHelper.setClass(cell, 'lastColumn');
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

                /** @type {HTMLTableRowElement} */          var row = tbodyRows[rowNum];
                /** @type {VRS_AIRCRAFTLIST_ROWDATA} */     var data = dataArray[rowNum];
                /** @type {VRS_AIRCRAFTLIST_ROWDATA_ROW} */ var rowDataRow = data.row;

                if(rowDataRow.aircraftId !== aircraft.id) {
                    rowDataRow.aircraftId = aircraft.id;
                    refreshCellProperties = refreshRow = true;
                }
                if(rowDataRow.visible !== undefined && !rowDataRow.visible) refreshRow = true;

                // Fill in the content for each cell
                var cells = row.cells;
                for(var colNum = 0;colNum < columnCount;++colNum) {
                    /** @type {VRS.RenderPropertyHandler} */     var handler = state.columns[colNum];
                    /** @type {HTMLTableCellElement} */          var cell = cells[colNum];
                    /** @type {VRS_AIRCRAFTLIST_ROWDATA_CELL} */ var cellData = data.cells[colNum];

                    var contentChanged = refreshRow;
                    if(!contentChanged) {
                        if(displayUnitChanged) contentChanged = handler.usesDisplayUnit(displayUnitChanged);
                        else                   contentChanged = handler.hasChangedCallback(aircraft);
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

                this._setRowState(row, rowDataRow, rowNum, aircraft === selectedAircraft, aircraft.isEmergency.val);
                this._showRow(state, row, data, true);
            }

            // Any rows left by this point are for aircraft that are no longer on radar, we need to hide these for reuse later
            var countRows = tbodyRows.length;
            for(var hideRowNum = countAircraft;hideRowNum < countRows;++hideRowNum) {
                this._showRow(state, tbodyRows[hideRowNum], dataArray[hideRowNum], false);
            }
        },
        //endregion

        //region -- _ensureBodyRowsHaveCorrectNumberOfColumns, _setCellAlignment, _setCellWidth, _showRow, _setRowClasses
        /**
         * Adds or removes cells on each row to match the column count passed across.
         *
         * Called when the list is being forced to refresh, which may be happening as a result of adding or removing columns.
         * @param {HTMLCollection}              tbodyRows
         * @param {VRS_AIRCRAFTLIST_ROWDATA[]}  dataArray
         * @param {number}                      columnCount
         * @private
         */
        _ensureBodyRowsHaveCorrectNumberOfColumns: function(tbodyRows, dataArray, columnCount)
        {
            var i;
            var length = tbodyRows.length;
            if(length > 0 && dataArray[0].cells.length !== columnCount) {
                var addCells = columnCount - dataArray[0].cells.length;
                for(var r = 0;r < length;++r) {
                    var data = dataArray[r];
                    /** @type {HTMLTableRowElement} */ var row = tbodyRows[r];

                    if(addCells > 0) {
                        for(i = 0;i < addCells;++i) {
                            data.cells.push({});
                            row.insertCell(-1);
                        }
                    } else {
                        // Remove widgets from the cells being removed. In principle this should never be called, the
                        // use of widgets in the list should be strongly discouraged!
                        var cellsLength = data.cells.length;
                        for(i = data.cells.length + addCells;i < cellsLength;++i) {
                            var cellData = data.cells[i];
                            if(cellData.widgetProperty) this._destroyWidgetRenderer(row.cells[i], cellData);
                        }

                        data.cells.splice(cellsLength + addCells, -addCells);
                        for(i = 0;i > addCells;--i) {
                            row.deleteCell(-1);
                        }
                    }
                }
            }
        },

        /**
         * Sets the alignment on a cell. Works with both jQuery objects and HTMLTableCellElements.
         * @param {HTMLTableCellElement|jQuery} cell
         * @param {VRS_AIRCRAFTLIST_ROWDATA_CELL} cellData
         * @param {VRS.Alignment} alignment
         * @private
         */
        _setCellAlignment: function(cell, cellData, alignment)
        {
            if(!cellData.alignment || cellData.alignment !== alignment) {
                var isDOM = cell instanceof HTMLTableCellElement;
                var addClass =      function(classNames) { isDOM ? VRS.domHelper.addClasses(cell, [classNames]) : cell.addClass(classNames); };
                var removeClass =   function(classNames) { isDOM ? VRS.domHelper.removeClasses(cell, [classNames]) : cell.removeClass(classNames); };

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
        },

        /**
         * Sets the fixed width on a cell. Works with both jQuery objects and HTMLTableCellElements.
         * @param {HTMLTableCellElement|jQuery} cell
         * @param {VRS_AIRCRAFTLIST_ROWDATA_CELL} cellData
         * @param {string} fixedWidth
         * @private
         */
        _setCellWidth: function(cell, cellData, fixedWidth)
        {
            if(!cellData.fixedWidth || cellData.fixedWidth !== fixedWidth) {
                var isDOM = cell instanceof HTMLTableCellElement;

                if(cellData.fixedWidth) {
                    isDOM ? VRS.domHelper.removeAttribute(cell, 'width') : cell.removeAttr('width');
                    isDOM ? VRS.domHelper.removeAttribute(cell, 'max-width') : cell.removeAttr('max-width');
                    isDOM ? VRS.domHelper.removeClasses(cell, ['fixedWidth']) : cell.removeClass('fixedWidth');
                }
                if(fixedWidth) {
                    isDOM ? VRS.domHelper.setAttribute(cell, 'width', fixedWidth) : cell.attr('width', fixedWidth);
                    isDOM ? VRS.domHelper.setAttribute(cell, 'max-width', fixedWidth) : cell.attr('max-width', fixedWidth);
                    isDOM ? VRS.domHelper.addClasses(cell, ['fixedWidth']) : cell.addClass('fixedWidth');
                }
                cellData.fixedWidth = fixedWidth;
            }
        },

        /**
         * Shows or hides a row. Works with both jQuery objects and HTMLTableRowElements.
         * @param {VRS.AircraftListPluginState} state
         * @param {HTMLTableRowElement|jQuery}  row
         * @param {VRS_AIRCRAFTLIST_ROWDATA}    rowData
         * @param {bool}                        visible
         * @private
         */
        _showRow: function(state, row, rowData, visible)
        {
            if(rowData.row.visible === undefined || rowData.row.visible !== visible) {
                var isDOM = row instanceof HTMLTableRowElement;
                if(!visible) isDOM ? VRS.domHelper.addClasses(row, ['hidden']) : row.addClass('hidden');
                else         isDOM ? VRS.domHelper.removeClasses(row, ['hidden']) : row.removeClass('hidden');

                if(!visible) {
                    var cells = isDOM ? row.cells : row[0].cells;
                    var countCells = cells.length;
                    for(var i = 0;i < countCells;++i) {
                        var cell = cells[i];
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
        },

        /**
         * Sets the state of a row and applies or removes classes as necessary to get the display matching the state.
         * @param {HTMLTableRowElement|jQuery} row
         * @param {VRS_AIRCRAFTLIST_ROWDATA_ROW} rowData
         * @param {number} rowNumber
         * @param {bool} isSelectedAircraft
         * @param {bool} isEmergency
         * @private
         */
        _setRowState: function(row, rowData, rowNumber, isSelectedAircraft, isEmergency)
        {
            var rowState = isSelectedAircraft ?
                VRS.AircraftListPluginState.RowState.Selected
                : rowNumber % 2 === 0 ?
                    VRS.AircraftListPluginState.RowState.Odd
                    : VRS.AircraftListPluginState.RowState.Even;

            if(isEmergency) rowState |= VRS.AircraftListPluginState.RowState.Emergency;
            if(rowNumber === 0) rowState |= VRS.AircraftListPluginState.RowState.FirstRow;

            var getRowStateClass = function(value) {
                var result = [];

                if(value & VRS.AircraftListPluginState.RowState.FirstRow) result.push('firstRow');
                if(value & VRS.AircraftListPluginState.RowState.Selected) {
                    if(value & VRS.AircraftListPluginState.RowState.Emergency) result.push('vrsSelectedEmergency');
                    else                                                       result.push('vrsSelected');
                } else {
                    if(value & VRS.AircraftListPluginState.RowState.Emergency) result.push('vrsEmergency');
                    else if(value & VRS.AircraftListPluginState.RowState.Even) result.push('vrsEven');
                    else                                                       result.push('vrsOdd');
                }

                return result;
            };

            if(!rowData.rowState || rowData.rowState !== rowState) {
                var isDOM = row instanceof HTMLTableRowElement;
                if(rowData.rowState) {
                    var removeClasses = getRowStateClass(rowData.rowState);
                    isDOM ? VRS.domHelper.removeClasses(row, removeClasses) : row.removeClass(removeClasses.join(' '));
                }

                var addClasses = getRowStateClass(rowState);
                isDOM ? VRS.domHelper.addClasses(row, addClasses) : row.addClass(addClasses.join(' '));
                rowData.rowState = rowState;
            }
        },
        //endregion

        //region -- _createWidgetRenderer, _destroyWidgetRenderer
        /**
         * Creates a widget in the cell for properties that are rendered by a jQuery widget. Their use in lists should
         * be carefully considered, introducing jQuery into the list REALLY slows it down.
         * @param {HTMLElement}                     cell
         * @param {VRS_AIRCRAFTLIST_ROWDATA_CELL}   cellData
         * @param {VRS.RenderPropertyHandler}       handler
         * @private
         */
        _createWidgetRenderer: function(cell, cellData, handler)
        {
            if(handler.isWidgetProperty() && cellData.widgetProperty !== handler.property) {
                var cellJQ = $(cell);
                if(cellData.widgetProperty) handler.destroyWidgetInJQuery(cellJQ, VRS.RenderSurface.List);
                handler.createWidgetInJQuery(cellJQ, VRS.RenderSurface.List);
                cellData.widgetProperty = handler.property;
            }
        },

        /**
         * Destroys the widget attached to a cell.
         * @param {HTMLElement}                     cell
         * @param {VRS_AIRCRAFTLIST_ROWDATA_CELL}   cellData
         * @param {VRS.RenderPropertyHandler}      [handler]
         * @private
         */
        _destroyWidgetRenderer: function(cell, cellData, handler)
        {
            if(cellData.widgetProperty) {
                if(!handler || handler.property !== cellData.widgetProperty) {
                    handler = VRS.renderPropertyHandlers[cellData.widgetProperty];
                }
                handler.destroyWidgetInDom(cell, VRS.RenderSurface.List);
                cellData.widgetProperty = undefined;
            }
        },
        //endregion

        //region -- _getRowIndexForAircraftId, _getRowIndexForRowElement
        /**
         * Returns the index of the row holding the aircraft passed across or -1 if no such row exists.
         * @param {VRS.AircraftListPluginState} state
         * @param {number} aircraftId
         * @returns {number}
         * @private
         */
        _getRowIndexForAircraftId: function(state, aircraftId)
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
        },

        /**
         * Returns the index of the row holding the row element passed across or -1 if no such row exists.
         * @param {VRS.AircraftListPluginState} state
         * @param {HTMLTableRowElement} rowElement
         * @returns {number}
         * @private
         */
        _getRowIndexForRowElement: function(state, rowElement)
        {
            var result = -1;

            var tbodyRows = state.tableBody[0].rows;
            var rowCount = tbodyRows.length;
            for(var i = 0;i < rowCount;++i) {
                if(tbodyRows[i] === rowElement) {
                    result = i;
                    break;
                }
            }

            return result;
        },
        //endregion

        //region -- Events consumed
        /**
         * The event handler for the VRS.AircraftList.updated event.
         //* @param {VRS.AircraftCollection} newAircraft
         //* @param {VRS.AircraftCollection} offRadar
         * @private
         */
        _aircraftListUpdated: function()
        {
            var state = this._getState();
            if(!state.suspended) this._refreshDisplay(state, false);
        },

        /**
         * Called when the locale is changed.
         * @private
         */
        _localeChanged: function()
        {
            var state = this._getState();
            this._buildHeader(state);
            if(!state.suspended) this._refreshDisplay(state, true);
        },

        /**
         * Called when the user changes a display unit.
         * @param {VRS.DisplayUnitDependency} displayUnitChanged
         * @private
         */
        _displayUnitChanged: function(displayUnitChanged)
        {
            var state = this._getState();
            if(!state.suspended) this._refreshDisplay(state, false, displayUnitChanged);
        },

        /**
         * Called when the user clicks a row.
         * @param {object} event
         * @param {HTMLTableRowElement} target
         * @private
         */
        _rowClicked: function(event, target)
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
        },

        /**
         * Called when the selected aircraft changes.
         * @param {VRS.Aircraft} oldSelectedAircraft
         * @private
         */
        _selectedAircraftChanged: function(oldSelectedAircraft)
        {
            var state = this._getState();
            var options = this.options;
            var tbodyRows = state.tableBody[0].rows;

            var self = this;
            var refreshRowStateForAircraft = function(aircraft, isSelected) {
                var rowNum = self._getRowIndexForAircraftId(state, aircraft.id);
                if(rowNum !== -1 && rowNum < tbodyRows.length) {
                    self._setRowState(tbodyRows[rowNum], state.rowData[rowNum].row, rowNum, isSelected, aircraft.isEmergency.val);
                }
            };

            var selectedAircraft = options.aircraftList.getSelectedAircraft();
            if(oldSelectedAircraft) refreshRowStateForAircraft(oldSelectedAircraft, false);
            if(selectedAircraft) refreshRowStateForAircraft(selectedAircraft, true);
        },

        /**
         * Called when the user changes the sort order for the list.
         * @private
         */
        _sortFieldsChanged: function()
        {
            var state = this._getState();
            if(!state.suspended) this._refreshDisplay(state, false);
        },

        /**
         * Called when the user clicks a header on a sortable column.
         * @param {Event} event
         * @private
         */
        _sortableHeaderClicked: function(event)
        {
            var state = this._getState();
            var options = this.options;

            var target = $(event.target);
            var cellIndex = -1;
            $.each(state.headerCells, function(/** number */idx, /** jQuery */ cell) {
                if(cell.is(target)) {
                    cellIndex = idx;
                }
                return cellIndex === -1;
            });

            /** @type {VRS.RenderPropertyHandler} */ var handler = cellIndex === -1 ? null : state.columns[cellIndex];

            if(options.sorter && handler.sortableField !== VRS.AircraftListSortableField.None) {
                var existingSortField = options.sorter.getSingleSortField();
                var sortField = existingSortField != null && existingSortField.field == handler.sortableField ? existingSortField : { field: handler.sortableField, ascending: false };
                sortField.ascending = !sortField.ascending;
                options.sorter.setSingleSortField(sortField);
                options.sorter.saveState();
            }

            event.stopPropagation();
            return false;
        },
        //endregion

        __nop: null
    });
    //endregion
})(window.VRS = window.VRS || {}, jQuery);
