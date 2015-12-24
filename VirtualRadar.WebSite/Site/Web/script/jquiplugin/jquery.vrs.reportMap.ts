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
 * @fileoverview A jQueryUI plugin that displays the location of an aircraft for the reports.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportMapClass = VRS.globalOptions.reportMapClass || 'reportMap';                                                                             // The class to use for the map widget container.
    VRS.globalOptions.reportMapScrollToAircraft = VRS.globalOptions.reportMapScrollToAircraft !== undefined ? VRS.globalOptions.reportMapScrollToAircraft : true;   // True if the map should automatically scroll to show the selected aircraft's path.
    VRS.globalOptions.reportMapShowPath = VRS.globalOptions.reportMapShowPath !== undefined ? VRS.globalOptions.reportMapShowPath : true;                           // True if a line should be drawn between the start and end points of the aircraft's path.
    VRS.globalOptions.reportMapStartSelected = VRS.globalOptions.reportMapStartSelected !== undefined ? VRS.globalOptions.reportMapStartSelected : false;           // True if the start point should be displayed in the selected colours, false if the end point should show in selected colours.

    /**
     * Report map plugin options
     */
    export interface ReportMapPlugin_Options
    {
        /**
         * The name to save state under.
         */
        name?: string;

        /**
         * The report whose flights we're going to display. If no report is supplied then aircraft must be manually rendered, if it's supplied then it renders them automatically.
         */
        report?: Report;

        /**
         * The mandatory plotter options to use when plotting aircraft.
         */
        plotterOptions: AircraftPlotterOptions;

        /**
         * Additional classes to add to the element.
         */
        elementClasses?: string;

        /**
         * The unit display preferences to use when displaying the marker.
         */
        unitDisplayPreferences: UnitDisplayPreferences;

        /**
         * Overrides to apply to the map.
         */
        mapOptionOverrides?: IMapOptions;

        /**
         * A list of controls to add to the map.
         */
        mapControls?: IMapControl[];

        /**
         * True if the map should automatically scroll to show the aircraft's path.
         */
        scrollToAircraft?: boolean;

        /**
         * True if a line should be drawn between the start and end points of the aircraft's path.
         */
        showPath?: boolean;

        /**
         * True if the start point should be shown in selected colours, false if the end point is shown in selected colours.
         */
        startSelected?: boolean;

        /**
         * Called once the map has been loaded.
         */
        loadedCallback?: () => void;
    }

    /**
     * The state carried by the ReportMapPlugin plugin.
     */
    class ReportMapPlugin_State
    {
        /**
         * The element that contains the map.
         */
        mapContainer: JQuery = null;

        /**
         * The direct reference to the map plugin.
         */
        mapPlugin: IMap = null;

        /**
         * The plotter that this object uses to draw aircraft onto the map.
         */
        aircraftPlotter: AircraftPlotter = null;

        /**
         * The list of aircraft that we plot onto the map. It's just the same aircraft twice - once for the marker on
         * the start position and once for the marker on the end position.
         */
        aircraftList: AircraftCollection = new VRS.AircraftCollection();

        /**
         * The next aircraft ID to assign to a fake VRS.Aircraft object.
         */
        nextAircraftId: number = 1;

        /**
         * The ID in the aircraft list of the fake VRS.Aircraft for the first marker to show for the aircraft.
         */
        firstPositionAircraftId: number = 0;

        /**
         * The ID in the aircraft list of the fake VRS.Aircraft for the last marker to show for the aircraft.
         */
        lastPositionAircraftId: number = 0;

        /**
         * The flight to display to the user, if any.
         */
        selectedFlight: IReportFlight = null;

        /**
         * The entry in the aircraft list that pertains to the "selected" aircraft, i.e. the one we show in selected
         * colours. The end marker is always shown in selected colours.
         */
        selectedAircraft: Aircraft = null;

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
    VRS.jQueryUIHelper.getReportMapPlugin = function(jQueryElement: JQuery) : ReportMapPlugin
    {
        return jQueryElement.data('vrsVrsReportMap');
    }
    VRS.jQueryUIHelper.getReportMapOptions = function(overrides?: ReportMapPlugin_Options) : ReportMapPlugin_Options
    {
        return $.extend(<ReportMapPlugin_Options> {
            name:                       'default',
            report:                     null,
            plotterOptions:             null,
            elementClasses:             '',
            unitDisplayPreferences:     undefined,
            mapOptionOverrides:         {},
            mapControls:                [],
            scrollToAircraft:           VRS.globalOptions.reportMapScrollToAircraft,
            showPath:                   VRS.globalOptions.reportMapShowPath,
            startSelected:              VRS.globalOptions.reportMapStartSelected,
            loadedCallback:             $.noop
        }, overrides);
    }

    /**
     * A jQuery widget that can display a single aircraft's location on a map for a report.
     */
    export class ReportMapPlugin extends JQueryUICustomWidget
    {
        options: ReportMapPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getReportMapOptions();
        }

        private _getState() : ReportMapPlugin_State
        {
            var result = this.element.data('reportMapState');
            if(result === undefined) {
                result = new ReportMapPlugin_State();
                this.element.data('reportMapState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.reportMapClass);
            if(options.elementClasses) {
                this.element.addClass(options.elementClasses);
            }
            this._createMap(state);
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

            if(state.aircraftPlotter) {
                state.aircraftPlotter.dispose();
            }
            state.aircraftPlotter = null;

            if(state.mapPlugin) {
                state.mapPlugin.destroy();
            }
            if(state.mapContainer) {
                state.mapContainer.remove();
            }
            state.mapPlugin = state.mapContainer = null;

            this.element.removeClass(VRS.globalOptions.reportMapClass);
        }

        isOpen() : boolean
        {
            var state = this._getState();
            return state.mapPlugin && state.mapPlugin.isOpen();
        }

        /**
         * Creates the map container and populates it with a map.
         */
        private _createMap(state: ReportMapPlugin_State)
        {
            var options = this.options;

            if(options.report) {
                state.selectedFlightChangedHookResult = options.report.hookSelectedFlightCHanged(this._selectedFlightChanged, this);
            }

            var mapOptions: IMapOptions = {
                name:                   'report-' + options.name,
                scrollwheel:            true,
                draggable:              !VRS.globalOptions.isMobile,
                useServerDefaults:      true,
                loadMarkerWithLabel:    true,
                autoSaveState:          true,
                useStateOnOpen:         true,
                mapControls:            options.mapControls,
                controlStyle:           VRS.MapControlStyle.DropdownMenu
            };
            $.extend(mapOptions, options.mapOptionOverrides);
            mapOptions.afterOpen = $.proxy(this._mapCreated, this);

            state.mapContainer = $('<div/>')
                .appendTo(this.element);
            state.mapContainer.vrsMap(VRS.jQueryUIHelper.getMapOptions(mapOptions));
        }

        private _mapCreated()
        {
            var state = this._getState();
            var options = this.options;

            // It is possible for this to get called on an object that has been destroyed and resurrected. We can detect
            // when this happens - the container will no longer exist.
            if(state.mapContainer) {
                state.mapPlugin = VRS.jQueryUIHelper.getMapPlugin(state.mapContainer);

                if(!state.mapPlugin.isOpen()) {
                    if(options.mapControls) {
                        $.each(options.mapControls, function(idx, control) {
                            state.mapContainer.children().first().prepend(control.control);
                        });
                    }
                } else {
                    state.aircraftPlotter = new VRS.AircraftPlotter({
                        plotterOptions:         options.plotterOptions,
                        map:                    state.mapContainer,
                        unitDisplayPreferences: options.unitDisplayPreferences,
                        getAircraft:            () => this._getAircraft(),
                        getSelectedAircraft:    () => this._getSelectedAircraft(),
                        getCustomPinTexts:      function(aircraft: Aircraft) {
                            return [ aircraft.id === state.firstPositionAircraftId ? VRS.$$.Start : VRS.$$.End ];
                        }
                    });
                }

                VRS.globalisation.hookLocaleChanged(this._localeChanged, this);
            }

            if(options.loadedCallback) {
                options.loadedCallback();
            }
        }

        /**
         * Constructs a list of aircraft for the selected flight. It is this list that the plotter will end up showing
         * as markers on the map. The list can only have two entries at most - both for the same aircraft, the first
         * shows the start position and the second shows its final position. To distinguish between the two "aircraft"
         * they are given different IDs - ID 1 is the start position of the aircraft, ID 2 is the end position.
         */
        private _buildFakeVrsAircraft(state: ReportMapPlugin_State)
        {
            var options = this.options;

            state.aircraftList = new VRS.AircraftCollection();
            state.selectedAircraft = null;

            var flight = state.selectedFlight;

            if(flight && ((flight.fLat && flight.fLng) || (flight.lLat && flight.lLng))) {
                var first = null, last = null;
                if(flight.fLat && flight.fLng) {
                    first = VRS.Report.convertFlightToVrsAircraft(flight, true);
                    first.id = state.firstPositionAircraftId = state.nextAircraftId++;
                    state.aircraftList[first.id] = first;
                }
                if(flight.lLat && flight.lLng) {
                    last = VRS.Report.convertFlightToVrsAircraft(flight, false);
                    last.id = state.lastPositionAircraftId = state.nextAircraftId++;
                    state.aircraftList[last.id] = last;
                }

                state.selectedAircraft = options.startSelected ? first : last;

                if(options.showPath && first && last) {
                    last.fullTrail.arr.push(new VRS.FullTrailValue(first.latitude.val, first.longitude.val, first.heading.val, first.altitude.val, first.speed.val));
                    last.fullTrail.arr.push(new VRS.FullTrailValue(last.latitude.val, last.longitude.val, last.heading.val, last.altitude.val, last.speed.val));
                }
            }
        }

        /**
         * Returns a collection of aircraft to plot on the map. See _buildFakeVrsAircraft.
         */
        private _getAircraft() : AircraftCollection
        {
            var state = this._getState();
            return state.aircraftList;
        }

        /**
         * Returns the aircraft to paint in selected colours. See _buildFakeVrsAircraft.
         */
        private _getSelectedAircraft() : Aircraft
        {
            var state = this._getState();
            return state.selectedAircraft;
        }

        /**
         * Called when the user chooses another language.
         */
        private _localeChanged()
        {
            var state = this._getState();
            if(state.aircraftPlotter) {
                state.aircraftPlotter.plot(true, true);
            }
        }

        /**
         * Called when the report indicates that the selected flight has been changed.
         */
        private _selectedFlightChanged()
        {
            this.showFlight(this.options.report.getSelectedFlight());
        }

        /**
         * Shows the flight's details on the map.
         */
        showFlight(flight: IReportFlight)
        {
            var state = this._getState();
            state.selectedFlight = flight;

            var options = this.options;
            this._buildFakeVrsAircraft(state);

            var applyWhenReady = function() {
                var map = state.aircraftPlotter ? state.aircraftPlotter.getMap() : null;
                if(!map || !map.isReady()) {
                    setTimeout(applyWhenReady, 100);
                } else {
                    var fromAircraft = state.aircraftList.findAircraftById(state.firstPositionAircraftId);
                    var toAircraft = fromAircraft ? state.aircraftList.findAircraftById(state.lastPositionAircraftId) : null;

                    if(options.scrollToAircraft && (fromAircraft || toAircraft)) {
                        if(fromAircraft.hasPosition() && toAircraft.hasPosition() && (fromAircraft.latitude.val !== toAircraft.latitude.val || fromAircraft.longitude.val !== toAircraft.longitude.val)) {
                            var bounds = VRS.greatCircle.arrangeTwoPointsIntoBounds(
                                !fromAircraft ? null : { lat: fromAircraft.latitude.val, lng: fromAircraft.longitude.val },
                                !toAircraft ? null : { lat: toAircraft.latitude.val, lng: toAircraft.longitude.val }
                            );
                            if(bounds) {
                                map.fitBounds(bounds);
                            }
                        } else if(fromAircraft.hasPosition()) {
                            map.panTo(fromAircraft.getPosition());
                        } else if(toAircraft.hasPosition()) {
                            map.panTo(toAircraft.getPosition());
                        }
                    }

                    state.aircraftPlotter.plot(true, true);
                }
            };

            applyWhenReady();
        }
    }

    $.widget('vrs.vrsReportMap', new ReportMapPlugin());
}

declare interface JQuery
{
    vrsReportMap();
    vrsReportMap(options: VRS.ReportMapPlugin_Options);
    vrsReportMap(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
