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
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
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
    VRS.globalOptions.reportDetailShowAircraftLinks = VRS.globalOptions.reportDetailShowAircraftLinks !== undefined ? VRS.globalOptions.reportDetailShowAircraftLinks : true;       // True if links should be shown for the aircraft in the aircraft detail panel
    VRS.globalOptions.reportDetailShowSeparateRouteLink = VRS.globalOptions.reportDetailShowSeparateRouteLink !== undefined ? VRS.globalOptions.reportDetailShowSeparateRouteLink : true;  // Show a separate link to add or correct routes
    VRS.globalOptions.reportDetailShowSDMAircraftLink = VRS.globalOptions.reportDetailShowSDMAircraftLink !== undefined ? VRS.globalOptions.reportDetailShowSDMAircraftLink : true;  // Show a link to add or update aircraft lookup details.

    /**
     * The options that can be passed when creating a new ReportDetailPlugin
     */
    export interface ReportDetailPlugin_Options extends ReportRender_Options
    {
        /**
         * The name to use when saving and loading state.
         */
        name?: string;

        /**
         * The report whose selected flight is going to be displayed in the panel.
         */
        report: Report;

        /**
         * The VRS.UnitDisplayPreferences that dictate how values are to be displayed.
         */
        unitDisplayPreferences: UnitDisplayPreferences;

        /**
         * The plotter options to use with map widgets.
         */
        plotterOptions?: AircraftPlotterOptions;

        /**
         * The columns to display.
         */
        columns?: ReportAircraftOrFlightPropertyEnum[];

        /**
         * True if the state last saved by the user against name should be loaded and applied immediately when creating the control.
         */
        useSavedState?: boolean;

        /**
         * True if heights, distances and speeds should indicate their units.
         */
        showUnits?: boolean;

        /**
         * True if empty values are to be shown.
         */
        showEmptyValues?: boolean;

        /**
         * True if altitudes should distinguish between altitudes of zero and aircraft on the ground.
         */
        distinguishOnGround?: boolean;
    }

    /**
     * The settings saved between sessions by the ReportDetailPlugin.
     */
    export interface ReportDetailPlugin_SaveState
    {
        columns:            ReportAircraftOrFlightPropertyEnum[];
        showUnits:          boolean;
        showEmptyValues:    boolean;
    }

    /**
     * Holds the state associated with a report detail panel.
     */
    class ReportDetailPlugin_State
    {
        /**
         * True if the control has been suspended because it is no longer in view, false if it's active.
         */
        suspended = false;

        /**
         * The element that all of the content is rendered into.
         */
        containerElement: JQuery = null;

        /**
         * The element that the header is rendered into.
         */
        headerElement: JQuery = null;

        /**
         * The element that the body is rendered into.
         */
        bodyElement: JQuery = null;

        /**
         * A jQuery element for the links.
         */
        linksContainer: JQuery = undefined;

        /**
         * The flight whose details are currently on display.
         */
        displayedFlight: IReportFlight;

        /**
         * An associative array of VRS_REPORT_PROPERTY properties against the jQuery element created for their display
         * in the body.
         * @type {Object.<VRS_REPORT_PROPERTY, jQuery>}
         */
        bodyPropertyElements: { [index: string /* ReportAircraftPropertyEnum */]: JQuery } = {};

        /**
         * A direct reference to the plugin for standard aircraft links.
         */
        aircraftLinksPlugin: AircraftLinksPlugin = null;

        /**
         * A direct reference to the plugin for links to the routes submission site.
         */
        routeLinksPlugin: AircraftLinksPlugin = null;

        /**
         * The hook result for the selected flight changed event.
         */
        selectedFlightChangedHookResult: IEventHandle = null;

        /**
         * The hook result for the locale changed event.
         */
        localeChangedHookResult: IEventHandle = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getReportDetailPlugin = function(jQueryElement: JQuery) : ReportDetailPlugin
    {
        return jQueryElement.data('vrsVrsReportDetail');
    }
    VRS.jQueryUIHelper.getReportDetailOptions = function(overrides?: ReportDetailPlugin_Options) : ReportDetailPlugin_Options
    {
        return $.extend({
            name:                   'default',
            columns:                VRS.globalOptions.reportDetailColumns.slice(),
            useSavedState:          true,
            showUnits:              VRS.globalOptions.reportDetailDefaultShowUnits,
            showEmptyValues:        VRS.globalOptions.reportDetailDefaultShowEmptyValues,
            distinguishOnGround:    VRS.globalOptions.reportDetailDistinguishOnGround
        }, overrides);
    }

    /**
     * A jQuery UI widget that can display the detail for a single flight.
     */
    export class ReportDetailPlugin extends JQueryUICustomWidget implements ISelfPersist<ReportDetailPlugin_SaveState>
    {
        options: ReportDetailPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getReportDetailOptions();
        }

        private _getState() : ReportDetailPlugin_State
        {
            var result = this.element.data('reportDetailPanelState');
            if(result === undefined) {
                result = new ReportDetailPlugin_State();
                this.element.data('reportDetailPanelState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            if(options.useSavedState) {
                this.loadAndApplyState();
            }
            this._displayFlightDetails(state, options.report.getSelectedFlight());

            VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
        }

        _destroy()
        {
            var state = this._getState();
            var options = this.options;

            if(state.selectedFlightChangedHookResult && options.report) {
                options.report.unhook(state.selectedFlightChangedHookResult);
            }
            state.selectedFlightChangedHookResult = null;

            if(state.localeChangedHookResult && VRS.globalisation) {
                VRS.globalisation.unhook(state.localeChangedHookResult);
            }
            state.localeChangedHookResult = null;

            if(state.aircraftLinksPlugin) {
                state.aircraftLinksPlugin.destroy();
            }
            state.linksContainer.empty();

            state.displayedFlight = null;

            this._destroyDisplay(state);
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
        loadState() : ReportDetailPlugin_SaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.columns = VRS.reportPropertyHandlerHelper.buildValidReportPropertiesList(result.columns, [ VRS.ReportSurface.DetailBody ]);

            return result;
        }

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_REPORTDETAILPANEL} settings
         */
        applyState(settings: ReportDetailPlugin_SaveState)
        {
            this.options.columns = settings.columns;
            this.options.showUnits = settings.showUnits;
            this.options.showEmptyValues = settings.showEmptyValues;
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
            return 'vrsReportDetailPanel-' + this.options.report.getName() + '-' + this.options.name;
        }

        /**
         * Creates the saved state object.
         */
        private _createSettings() : ReportDetailPlugin_SaveState
        {
            return {
                columns:            this.options.columns,
                showUnits:          this.options.showUnits,
                showEmptyValues:    this.options.showEmptyValues
            };
        }

        /**
         * Returns the option pane that can be used to configure the widget via the UI.
         */
        createOptionPane(displayOrder: number) : OptionPane
        {
            var result = new VRS.OptionPane({
                name:           'vrsReportDetailPane-' + this.options.name + '_Settings',
                titleKey:       'DetailPanel',
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
                    }),
                    new VRS.OptionFieldCheckBox({
                        name:           'showEmptyValues',
                        labelKey:       'ShowEmptyValues',
                        getValue:       () => this.options.showEmptyValues,
                        setValue:       (value) => {
                            this.options.showEmptyValues = value;
                            this.refreshDisplay();
                        },
                        saveState:      () => this.saveState()
                    })
                ]
            });

            if(VRS.globalOptions.reportDetailUserCanConfigureColumns) {
                VRS.reportPropertyHandlerHelper.addReportPropertyListOptionsToPane({
                    pane:       result,
                    fieldLabel: 'Columns',
                    surface:    VRS.ReportSurface.DetailBody,
                    getList:    () => this.options.columns,
                    setList:    (cols) => {
                        this.options.columns = cols;
                        this.refreshDisplay();
                    },
                    saveState:  () => this.saveState()
                });
            }

            return result;
        }

        /**
         * Suspends or actives the control.
         */
        suspend(onOff: boolean)
        {
            onOff = !!onOff;

            var state = this._getState();
            if(state.suspended !== onOff) {
                state.suspended = onOff;
                if(!state.suspended) this.refreshDisplay();
            }
        }

        /**
         * Displays the details for the flight passed across.
         */
        private _displayFlightDetails(state: ReportDetailPlugin_State, flight: IReportFlight)
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
                    this._createLinks(state, flight);
                }
            }
        }

        /**
         * Redraws the displayed flight.
         */
        refreshDisplay()
        {
            var state = this._getState();
            this._displayFlightDetails(state, state.displayedFlight);
        }

        /**
         * Destroys the UI associated with the display.
         */
        private _destroyDisplay(state: ReportDetailPlugin_State)
        {
            for(var propertyName in state.bodyPropertyElements) {
                var handler = VRS.reportPropertyHandlers[propertyName];
                var element = state.bodyPropertyElements[propertyName];
                if(handler && element) {
                    handler.destroyWidgetInJQueryElement(element, VRS.ReportSurface.DetailBody);
                }
            }
            state.bodyPropertyElements = {};

            if(state.bodyElement) {
                state.bodyElement.remove();
            }
            state.bodyElement = null;

            if(state.headerElement) {
                state.headerElement.remove();
            }
            state.headerElement = null;

            if(state.aircraftLinksPlugin) {
                state.aircraftLinksPlugin.destroy();
            }
            if(state.routeLinksPlugin) {
                state.routeLinksPlugin.destroy();
            }
            if(state.linksContainer) {
                state.linksContainer.empty();
                state.linksContainer.remove();
            }
            state.linksContainer = null;

            if(state.containerElement) {
                state.containerElement.remove();
            }
            state.containerElement = null;
        }

        /**
         * Creates and populates the header showing the important details about the flight. This is not configurable.
         */
        private _createHeader(state: ReportDetailPlugin_State, flight: IReportFlight)
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
        }

        /**
         * Adds a cell to a row in the header table.
         */
        private _addHeaderCell(state: ReportDetailPlugin_State, row: JQuery, colspan: number, flight: IReportFlight, property: ReportAircraftOrFlightPropertyEnum, classes: string)
        {
            var cell = $('<td/>')
                .addClass(classes)
                .appendTo(row);
            if(colspan > 1) cell.attr('colspan', colspan);

            var handler = VRS.reportPropertyHandlers[property];
            if(!handler) throw 'Cannot find the handler for the ' + property + ' property';
            handler.renderIntoJQueryElement(cell, handler.isAircraftProperty ? flight.aircraft : flight, this.options, VRS.ReportSurface.DetailHead);
        }

        /**
         * Creates and populates the body of the detail panel.
         */
        private _createBody(state: ReportDetailPlugin_State, flight: IReportFlight)
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
                    if(suppressLabel) {
                        // We want an empty label here so that the print media CSS can reserve space for it
                        $('<div/>')
                            .addClass('noLabel')
                            .appendTo(listItem);
                    } else {
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
                    handler.createWidgetInJQueryElement(content, VRS.ReportSurface.DetailBody, options);
                    handler.renderIntoJQueryElement(content, json, options, VRS.ReportSurface.DetailBody);
                }
            }
        }

        /**
         * Creates the links panel for the flight.
         */
        private _createLinks(state: ReportDetailPlugin_State, flight: IReportFlight)
        {
            if(VRS.globalOptions.reportDetailShowAircraftLinks) {
                state.linksContainer =
                    $('<div/>')
                        .addClass('links')
                        .appendTo(state.containerElement);

                var aircraftLinksElement = $('<div/>')
                    .appendTo(state.linksContainer)
                    .vrsAircraftLinks();
                state.aircraftLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(aircraftLinksElement);

                var aircraft = Report.convertFlightToVrsAircraft(flight, true);
                state.aircraftLinksPlugin.renderForAircraft(aircraft, true);

                var routeLinks: (LinkSiteEnum | LinkRenderHandler)[] = [];
                if(VRS.globalOptions.reportDetailShowSeparateRouteLink) {
                    routeLinks.push(VRS.LinkSite.StandingDataMaintenance);
                }
                if(VRS.globalOptions.reportDetailShowSDMAircraftLink) {
                    routeLinks.push(VRS.LinkSite.SDMAircraft);
                }

                if(routeLinks.length > 0) {
                    var routeLinksElement = $('<div/>')
                        .appendTo(state.linksContainer)
                        .vrsAircraftLinks({
                            linkSites: routeLinks
                        });
                    state.routeLinksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(routeLinksElement);
                    state.routeLinksPlugin.renderForAircraft(aircraft, true);
                }

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
         * Called when the selected flight changes.
         */
        private _selectedFlightChanged()
        {
            var selectedFlight = this.options.report.getSelectedFlight();
            this._displayFlightDetails(this._getState(), selectedFlight);
        }
    }

    $.widget('vrs.vrsReportDetail', new ReportDetailPlugin());
}

declare interface JQuery
{
    vrsReportDetail();
    vrsReportDetail(options: VRS.ReportDetailPlugin_Options);
    vrsReportDetail(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
