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
 * @fileoverview A jQueryUI plugin that displays a single aircraft's location and attitude on a map.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region VRS.globalOptions
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftPositionMapClass = VRS.globalOptions.aircraftPositionMapClass || 'aircraftPosnMap';       // The class to use for the aircraft position map widget container.
    //endregion

    //region AircraftPositionMapState
    /**
     * The state carried by an aircraft position map widget.
     * @constructor
     */
    VRS.AircraftPositionMapState = function()
    {
        /**
         * The jQuery container element for the map.
         * @type {jQuery}
         */
        this.mapContainer = null;

        /**
         * The direct reference to the map in the map container. This will be null until after the map has completed
         * loading, which could be some time after _create has finished.
         * @type {VRS.vrsMap}
         */
        this.mapPlugin = null;

        /**
         * The collection of aircraft to plot on the map.
         * @type {VRS.AircraftCollection}
         */
        this.aircraftCollection = new VRS.AircraftCollection();

        /**
         * The aircraft to show in the selected state.
         * @type {VRS.Aircraft}
         */
        this.selectedAircraft = null;

        /**
         * The aircraft plotter that will plot the aircraft for us.
         * @type {VRS.AircraftPlotter}
         */
        this.aircraftPlotter = null;

        /**
         * The direct reference to the map whose settings are being mirrored.
         * @type {VRS.vrsMap}
         */
        this.mirrorMapPlugin = null;

        /**
         * True if the plugin has never rendered an aircraft before, false if it has.
         * @type {boolean}
         */
        this.firstRender = true;

        /**
         * The hook result for our map's map type changed event.
         * @type {Object}
         */
        this.mapTypeChangedHookResult = null;

        /**
         * The hook result for the mirror map's map type changed event.
         * @type {Object}
         */
        this.mirrorMapTypeChangedHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    /**
     * Returns the aircraft position map widget attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsAircraftPositonMap}
     */
    VRS.jQueryUIHelper.getAircraftPositionMapPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsAircraftPositonMap'); };

    /**
     * Returns a default set of options for the aircraft position widget, including optional overrides.
     * @param {VRS_OPTIONS_AIRCRAFTPOSITIONMAP=} overrides
     * @returns {VRS_OPTIONS_AIRCRAFTPOSITIONMAP}
     */
    VRS.jQueryUIHelper.getAircraftPositionMapOptions = function(overrides)
    {
        return $.extend({
            plotterOptions:             null,       // The mandatory plotter options to use when plotting aircraft.
            mirrorMapJQ:                null,       // The map whose settings are going to be mirrored on this map.
            stateName:                  null,       // If supplied then the map state is saved between sessions against this name. If not supplied then state is not saved.
            mapOptionOverrides:         {},         // Settings to apply to the map that override those already set on the mirror map.
            unitDisplayPreferences:     undefined,  // The unit display preferences to use when displaying the marker.
            autoHideNoPosition:         true,       // True if the element should be hidden when asked to render a position for aircraft that have no position. If false then the marker is just removed from the map.
            reflectMapTypeBackToMirror: true,       // True if changes to the map type on the plugin's map should be reflected on the mirror map.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsAircraftPositonMap
    /**
     * <p>
     * A jQuery widget that can display a single aircraft's location on a map.
     * </p><p>
     * The intention is that is displayed on aircraft detail panels and that it borrows many settings from the main map
     * display. So for instance, if the user changes the map style of the main map then this map follows suit.
     * </p><p>
     * Because this is intended for use as a property render item it is not auto-updating - you need to call renderAircraft
     * with a VRS.Aircraft in order for it to display the aircraft's position, it won't hook an aircraft list and render
     * the aircraft automatically.
     * </p>
     * @namespace VRS.vrsAircraftPositonMap
     */
    $.widget('vrs.vrsAircraftPositonMap', {
        //region -- options
        /** @type {VRS_OPTIONS_AIRCRAFTPOSITIONMAP} */
        options: VRS.jQueryUIHelper.getAircraftPositionMapOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object for the plugin, creating it if it's not already there.
         * @returns {VRS.AircraftPositionMapState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('aircraftPositionMapState');
            if(result === undefined) {
                result = new VRS.AircraftPositionMapState();
                this.element.data('aircraftPositionMapState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
        {
            var state = this._getState();
            var options = this.options;

            this.element.addClass(VRS.globalOptions.aircraftPositionMapClass);

            if(options.mirrorMapJQ) state.mirrorMapPlugin = VRS.jQueryUIHelper.getMapPlugin(options.mirrorMapJQ);

            // This method can return before the map has finished loading. Further construction of the object is completed
            // by a callback to _mapCreated, don't put any construction that relies on the map having been loaded after
            // this call.
            this._createMap(state);
        },

        /**
         * Creates the map container and populates it with a map.
         * @param {VRS.AircraftPositionMapState} state
         * @private
         */
        _createMap: function(state)
        {
            var options = this.options;

            /** @type {VRS_OPTIONS_MAP} */
            var mapOptions = {};
            if(state.mirrorMapPlugin && state.mirrorMapPlugin.isOpen()) {
                mapOptions.zoom = state.mirrorMapPlugin.getZoom();
                mapOptions.center = state.mirrorMapPlugin.getCenter();
                mapOptions.mapTypeId = state.mirrorMapPlugin.getMapType();
                mapOptions.streetViewControl = state.mirrorMapPlugin.getStreetView();
                mapOptions.scrollwheel = state.mirrorMapPlugin.getScrollWheel();
                mapOptions.draggable = state.mirrorMapPlugin.getDraggable();
                mapOptions.controlStyle = VRS.MapControlStyle.DropdownMenu;
                mapOptions.useServerDefaults = false;
            }
            $.extend(mapOptions, options.mapOptionOverrides);

            if(!options.stateName) mapOptions.autoSaveState = false;
            else {
                mapOptions.autoSaveState = true;
                mapOptions.name = options.stateName;
                mapOptions.useStateOnOpen = true;
            }

            mapOptions.afterOpen = $.proxy(this._mapCreated, this);

            state.mapContainer = $('<div/>')
                .appendTo(this.element);
            state.mapContainer.vrsMap(VRS.jQueryUIHelper.getMapOptions(mapOptions));
        },

        /**
         * Called once the map has been opened. Completes the construction of the UI.
         * @private
         */
        _mapCreated: function()
        {
            var state = this._getState();
            var options = this.options;

            // Guard against the possible call to this on a plugin that is destroyed before the map finishes loading. In
            // this case the mapContainer will have been destroyed.
            if(state.mapContainer) {
                state.mapPlugin = VRS.jQueryUIHelper.getMapPlugin(state.mapContainer);
                if(state.mapPlugin && state.mapPlugin.isOpen()) {
                    state.aircraftPlotter = new VRS.AircraftPlotter({
                        plotterOptions:         options.plotterOptions,
                        map:                    state.mapContainer,
                        unitDisplayPreferences: options.unitDisplayPreferences,
                        getAircraft:            $.proxy(this._getAircraft, this),
                        getSelectedAircraft:    $.proxy(this._getSelectedAircraft, this)
                    });
                }

                state.mapTypeChangedHookResult = state.mapPlugin.hookMapTypeChanged(this._mapTypeChanged, this);
                if(state.mirrorMapPlugin) {
                    state.mirrorMapTypeChangedHookResult = state.mirrorMapPlugin.hookMapTypeChanged(this._mirrorMapTypeChanged, this);
                }
            }
        },

        /**
         * Releases the resources held by the widget.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();

            if(state.mapTypeChangedHookResult) state.mapPlugin.unhook(state.mapTypeChangedHookResult);
            state.mapTypeChangedHookResult = null;

            if(state.mirrorMapTypeChangedHookResult) state.mirrorMapPlugin.unhook(state.mirrorMapTypeChangedHookResult);
            state.mirrorMapTypeChangedHookResult = null;
            state.mirrorMapPlugin = null;

            if(state.mapPlugin) {
                state.mapPlugin.destroy();
                state.mapPlugin = null;
            }
            state.mapContainer = null;

            if(state.aircraftPlotter) state.aircraftPlotter.dispose();
            state.aircraftPlotter = null;

            state.aircraftCollection = null;

            this.element.removeClass(VRS.globalOptions.aircraftPositionMapClass);
            this.element.empty();
        },
        //endregion

        //region -- renderAircraft, suspend
        /**
         * Renders the aircraft on the map.
         * @param {VRS.Aircraft} aircraft
         * @param {boolean}      showAsSelected
         */
        renderAircraft: function(aircraft, showAsSelected)
        {
            var state = this._getState();
            var options = this.options;

            if(state.aircraftPlotter) {
                if(aircraft && !aircraft.hasPosition()) aircraft = null;

                var existingAircraft = state.aircraftCollection.toList();
                if(!aircraft) {
                    if(existingAircraft.length > 0) state.aircraftCollection = new VRS.AircraftCollection();
                } else {
                    if(existingAircraft.length !== 1 || existingAircraft[aircraft.id] !== aircraft) {
                        state.aircraftCollection = new VRS.AircraftCollection();
                        state.aircraftCollection[aircraft.id] = aircraft;
                    }
                }

                state.selectedAircraft = showAsSelected ? aircraft : null;

                if(!aircraft) {
                    if(options.autoHideNoPosition) $(this.element, ':visible').hide();
                    else state.aircraftPlotter.plot();
                } else {
                    var refreshMap = state.firstRender || (options.autoHideNoPosition && this.element.is(':hidden'));
                    if(refreshMap) {
                        this.element.show();
                        state.mapPlugin.refreshMap();
                    }
                    state.mapPlugin.panTo(aircraft.getPosition());
                    state.aircraftPlotter.plot();

                    state.firstRender = false;
                }
            }
        },

        /**
         * Suspends or resumes updates.
         * @param {boolean} onOff
         */
        suspend: function(onOff)
        {
            var state = this._getState();
            if(state.aircraftPlotter) state.aircraftPlotter.suspend(onOff);
        },
        //endregion

        //region -- Events subscribed
        /**
         * Called when our map's map type has changed.
         * @private
         */
        _mapTypeChanged: function()
        {
            var state = this._getState();
            var options = this.options;

            if(state.mirrorMapPlugin && state.mapPlugin && options.reflectMapTypeBackToMirror) {
                state.mirrorMapPlugin.setMapType(state.mapPlugin.getMapType());
            }
        },

        /**
         * Called when the map that we're mirroring has changed map type.
         * @private
         */
        _mirrorMapTypeChanged: function()
        {
            var state = this._getState();
            if(state.mirrorMapPlugin && state.mapPlugin) {
                state.mapPlugin.setMapType(state.mirrorMapPlugin.getMapType());
            }
        },

        /**
         * Called when the plotter wants to know the list of aircraft to plot.
         * @returns {VRS.AircraftCollection}
         * @private
         */
        _getAircraft: function()
        {
            var state = this._getState();
            return state.aircraftCollection;
        },

        /**
         * Called when the plotter wants to know which aircraft has been selected.
         * @returns {VRS.Aircraft}
         * @private
         */
        _getSelectedAircraft: function()
        {
            var state = this._getState();
            return state.selectedAircraft;
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));