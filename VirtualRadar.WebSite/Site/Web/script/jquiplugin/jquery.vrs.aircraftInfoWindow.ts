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
 * @fileoverview A jQueryUI plugin that can manage the detail panel for an aircraft.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftInfoWindowClass = VRS.globalOptions.aircraftInfoWindowClass || 'vrsAircraftInfoWindow';       // The class for the info window panel.
    VRS.globalOptions.aircraftInfoWindowEnabled = VRS.globalOptions.aircraftInfoWindowEnabled !== undefined ? VRS.globalOptions.aircraftInfoWindowEnabled : true;   // True if the info window is enabled by default
    VRS.globalOptions.aircraftInfoWindowItems = VRS.globalOptions.aircraftInfoWindowItems || [          // The array of items that are rendered into the info window.
        VRS.RenderProperty.Icao,
        VRS.RenderProperty.Registration,
        VRS.RenderProperty.ModelIcao,
        VRS.RenderProperty.Operator,
        VRS.RenderProperty.Model,
        VRS.RenderProperty.Callsign,
        VRS.RenderProperty.RouteShort,
        VRS.RenderProperty.Speed,
        VRS.RenderProperty.Altitude
    ];
    VRS.globalOptions.aircraftInfoWindowShowUnits = VRS.globalOptions.aircraftInfoWindowShowUnits !== undefined ? VRS.globalOptions.aircraftInfoWindowShowUnits : true; // True if units should be shown in the info window.
    VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns = VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns : true;  // True if uncertain callsigns are to be flagged as such.
    VRS.globalOptions.aircraftInfoWindowDistinguishOnGround = VRS.globalOptions.aircraftInfoWindowDistinguishOnGround !== undefined ? VRS.globalOptions.aircraftInfoWindowDistinguishOnGround : true;           // True if aircraft on the ground should show an altitude of GND.
    VRS.globalOptions.aircraftInfoWindowAllowConfiguration = VRS.globalOptions.aircraftInfoWindowAllowConfiguration !== undefined ? VRS.globalOptions.aircraftInfoWindowAllowConfiguration : true;              // True if the user can configure the infowindow settings.
    VRS.globalOptions.aircraftInfoWindowEnablePanning = VRS.globalOptions.aircraftInfoWindowEnablePanning !== undefined ? VRS.globalOptions.aircraftInfoWindowEnablePanning : true;                             // True if the map should pan to the info window when it opens.

    /**
     * The options that the AircraftInfoWindowPlugin honours.
     */
    export interface AircraftInfoWindowPlugin_Options
    {
        /**
         * The name to use when storing settings.
         */
        name?: string;

        /**
         * The aircraft list that the plugin will listen to.
         */
        aircraftList?: AircraftList;

        /**
         * The object that holds onto the markers for the map we'll be using.
         */
        aircraftPlotter?: AircraftPlotter;

        /**
         * The unit display preferences to use when showing aircraft detail.
         */
        unitDisplayPreferences: UnitDisplayPreferences;

        /**
         * True if the info window is to be shown, false otherwise.
         */
        enabled?: boolean;

        /**
         * True if the info window should override the options with those saved by the user.
         */
        useStateOnOpen?: boolean;

        /**
         * The items to display in the info window.
         */
        items?: RenderPropertyEnum[];

        /**
         * True if units should be shown.
         */
        showUnits?: boolean;

        /**
         * True if uncertain callsigns are to be highlighted.
         */
        flagUncertainCallsigns?: boolean;

        /**
         * True if aircraft on the ground should show an altitude of GND.
         */
        distinguishOnGround?: boolean;

        /**
         * True if the map should pan to the info window when it opens.
         */
        enablePanning?: boolean;
    }

    /**
     * The state held by the AircraftInfoWindowPlugin.
     */
    class AircraftInfoWindowPlugin_State
    {
        /**
         * The jQuery element for the container that the items are rendered into.
         */
        containerElement: JQuery = null;

        /**
         * The jQuery element for the items container.
         */
        itemsContainerElement: JQuery = null;

        /**
         * An associative array of jQuery elements that hold values against the render properties that will be rendered into them.
         */
        itemElements: { [index: string]: JQuery } = {};

        /**
         * The map info window that gets us displayed on the map.
         */
        mapInfoWindow: IMapInfoWindow = null;

        /**
         * The aircraft whose details we are displaying.
         */
        aircraft: Aircraft = null;

        /**
         * The aircraft that the info window is anchored to.
         */
        anchoredAircraft: Aircraft = null;

        /**
         * True if updates have been suspended.
         */
        suspended = false;

        /**
         * True if the user closed an open InfoWindow. If they closed a window then we don't open any new ones unless
         * they specifically request it by clicking a map marker - that will clear this flag and we then resume showing
         * info windows for all selections.
         */
        closedByUser = false;

        /**
         * The object that manages the link to the aircraft detail page.
         */
        jumpToAircraftDetailLinkRenderer: JumpToAircraftDetailPageRenderHandler = null;

        /**
         * The jQuery element that holds the links.
         */
        linksElement: JQuery = null;

        /**
         * The direct reference to the aircraft links manager.
         */
        linksPlugin: AircraftLinksPlugin = null;

        /**
         * The hook result from an aircraft list updated event.
         */
        aircraftListUpdatedHookResult: IEventHandle = null;

        /**
         * The hook result from a info window closed by the user event.
         */
        infoWindowClosedByUserHookResult: IEventHandleJQueryUI = null;

        /**
         * The hook result from a marker clicked event.
         */
        markerClickedHookResult: IEventHandleJQueryUI = null;

        /**
         * The hook result from a selected aircraft changed event.
         */
        selectedAircraftChangedHook: IEventHandle = null;

        /**
         * The hook result from the unit display preferences unit changed event.
         */
        unitChangedHookResult: IEventHandle = null;

        /**
         * The hook result for the change of language event.
         */
        localeChangedHookResult: IEventHandle = null;
    }

    /**
     * The settings that an AircraftInfoWindowPlugin can persist between sessions.
     */
    export interface AircraftInfoWindowPlugin_SaveState
    {
        enabled:   boolean;
        items:     RenderPropertyEnum[];
        showUnits: boolean;
    }

    /*
     * jQueryUIHelper
     */
    export var jQueryUIHelper: JQueryUIHelper  = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getAircraftInfoWindowPlugin = function(jQueryElement: JQuery) : AircraftInfoWindowPlugin
    {
        return <AircraftInfoWindowPlugin>jQueryElement.data('vrsVrsAircraftInfoWindow');
    }
    VRS.jQueryUIHelper.getAircraftInfoWindowOptions = function(overrides?: AircraftInfoWindowPlugin_Options) : AircraftInfoWindowPlugin_Options
    {
        return $.extend({
            name:                   'default',
            aircraftList:           null,
            aircraftPlotter:        null,
            unitDisplayPreferences: null,
            enabled:                VRS.globalOptions.aircraftInfoWindowEnabled,
            useStateOnOpen:         true,
            items:                  VRS.globalOptions.aircraftInfoWindowItems,
            showUnits:              VRS.globalOptions.aircraftInfoWindowShowUnits,
            flagUncertainCallsigns: VRS.globalOptions.aircraftInfoWindowFlagUncertainCallsigns,
            distinguishOnGround:    VRS.globalOptions.aircraftInfoWindowDistinguishOnGround,
            enablePanning:          VRS.globalOptions.aircraftInfoWindowEnablePanning
        }, overrides);
    }

    /**
     * A widget that can show details from the selected aircraft in a map's info window.
     */
    export class AircraftInfoWindowPlugin extends JQueryUICustomWidget implements ISelfPersist<AircraftInfoWindowPlugin_SaveState>
    {
        options: AircraftInfoWindowPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getAircraftInfoWindowOptions();
        }

        private _getState() : AircraftInfoWindowPlugin_State
        {
            var result = this.element.data('aircraftInfoWindowState');
            if(result === undefined) {
                result = new AircraftInfoWindowPlugin_State();
                this.element.data('aircraftInfoWindowState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;
            var map = options.aircraftPlotter.getMap();

            if(options.useStateOnOpen) {
                this.loadAndApplyState();
            }

            this.element.addClass(VRS.globalOptions.aircraftInfoWindowClass);

            state.containerElement = $('<div/>')
                .appendTo(this.element);
            state.itemsContainerElement = $('<ul/>')
                .appendTo(state.containerElement);

            this._buildItems(state);

            state.jumpToAircraftDetailLinkRenderer = new VRS.JumpToAircraftDetailPageRenderHandler();
            state.linksElement = $('<div/>')
                .addClass('links')
                .vrsAircraftLinks(VRS.jQueryUIHelper.getAircraftLinksOptions({ linkSites: [ state.jumpToAircraftDetailLinkRenderer ] }))
                .appendTo(state.containerElement);
            state.linksPlugin = VRS.jQueryUIHelper.getAircraftLinksPlugin(state.linksElement);

            state.mapInfoWindow = map.addInfoWindow(map.getUnusedInfoWindowId(), {
                content: this.element[0],
                disableAutoPan: !options.enablePanning
            });
            state.infoWindowClosedByUserHookResult = map.hookInfoWindowClosedByUser(this._infoWindowClosedByUser, this);

            state.aircraftListUpdatedHookResult = options.aircraftList.hookUpdated(this._aircraftListUpdated, this);
            state.selectedAircraftChangedHook = options.aircraftList.hookSelectedAircraftChanged(this._selectedAircraftChanged, this);
            state.markerClickedHookResult = map.hookMarkerClicked(this._markerClicked, this);
            state.unitChangedHookResult = options.unitDisplayPreferences.hookUnitChanged(this._displayUnitChanged, this);
            state.localeChangedHookResult = VRS.globalisation.hookLocaleChanged(this._localeChanged, this);

            this.showForAircraft(options.aircraftList.getSelectedAircraft());
        }

        _destroy()
        {
            var state = this._getState();
            var options = this.options;

            // Unhook all of the events
            if(state.aircraftListUpdatedHookResult) options.aircraftList.unhook(state.aircraftListUpdatedHookResult);
            state.aircraftListUpdatedHookResult = null;

            if(state.selectedAircraftChangedHook) options.aircraftList.unhook(state.selectedAircraftChangedHook);
            state.selectedAircraftChangedHook = null;

            if(state.infoWindowClosedByUserHookResult) options.aircraftPlotter.getMap().unhook(state.infoWindowClosedByUserHookResult);
            state.infoWindowClosedByUserHookResult = null;

            if(state.markerClickedHookResult) options.aircraftPlotter.getMap().unhook(state.markerClickedHookResult);
            state.markerClickedHookResult = null;

            if(state.unitChangedHookResult) options.unitDisplayPreferences.unhook(state.unitChangedHookResult);
            state.unitChangedHookResult = null;

            if(state.localeChangedHookResult) VRS.globalisation.unhook(state.localeChangedHookResult);
            state.localeChangedHookResult = null;

            // Destroy the links
            if(state.linksElement) {
                state.linksPlugin.destroy();
                state.linksElement.remove();
            }
            state.linksElement = null;
            state.jumpToAircraftDetailLinkRenderer = null;

            // Destroy the items
            this._destroyItems(state);
            if(state.itemsContainerElement) state.itemsContainerElement.remove();
            state.itemsContainerElement = null;

            // Destroy the container
            if(state.containerElement) state.containerElement.remove();
            state.containerElement = null;

            // Remove the class
            this.element.removeClass(VRS.globalOptions.aircraftInfoWindowClass);

            // Remove the info window
            if(state.mapInfoWindow) {
                options.aircraftPlotter.getMap().destroyInfoWindow(state.mapInfoWindow);
            }
            state.mapInfoWindow = null;

            // Null out anything that's left over
            state.aircraft = null;
        }

        /**
         * Creates the items list.
         */
        private _buildItems(state: AircraftInfoWindowPlugin_State)
        {
            this._destroyItems(state);

            var options = this.options;
            var length = options.items.length;
            for(var i = 0;i < length;++i) {
                var renderProperty = options.items[i];
                var handler = VRS.renderPropertyHandlers[renderProperty];
                if(!handler) throw 'Cannot find the render property handler for ' + renderProperty;

                var listItem = $('<li/>')
                    .appendTo(state.itemsContainerElement);
                var label = $('<label/>')
                    .text(handler.suppressLabelCallback(VRS.RenderSurface.InfoWindow) ? '' : VRS.globalisation.getText(handler.labelKey) + ':')
                    .appendTo(listItem);
                state.itemElements[renderProperty] = $('<p/>')
                    .addClass('value')
                    .appendTo(listItem);
            }
        }

        /**
         * Destroys the items list.
         * @param {VRS.AircraftInfoWindowState} state
         * @private
         */
        private _destroyItems(state: AircraftInfoWindowPlugin_State)
        {
            state.itemsContainerElement.empty();
            state.itemElements = {};
        }

        /**
         * Saves the current state to persistent storage.
         */
        saveState()
        {
            VRS.configStorage.save(this._persistenceKey(), this._createSettings());
        }

        /**
         * Returns the previously saved state or, if none has been saved, the current state.
         */
        loadState() : AircraftInfoWindowPlugin_SaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            var result = $.extend(this._createSettings(), savedSettings);
            result.items = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.items, [ VRS.RenderSurface.InfoWindow ]);

            return result;
        }

        /**
         * Applies the previously saved state to this object.
         */
        applyState(settings: AircraftInfoWindowPlugin_SaveState)
        {
            var options = this.options;
            options.enabled = settings.enabled;
            options.items = settings.items.slice();
            options.showUnits = settings.showUnits;

            var state = this._getState();
            if(state.containerElement) {
                this._buildItems(state);
                this.refreshDisplay();
            }
        }

        /**
         * Loads and applies the previously saved state.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key to use when saving and loading state.
         */
        private _persistenceKey() : string
        {
            return 'vrsAircraftInfoWindow-' + this.options.name;
        }

        /**
         * Returns the object that holds the current state.
         */
        private _createSettings() : AircraftInfoWindowPlugin_SaveState
        {
            var options = this.options;
            return {
                enabled:            options.enabled,
                items:              options.items,
                showUnits:          options.showUnits
            };
        }

        /**
         * Creates the option pane for configuring the standard options.
         */
        createOptionPane(displayOrder: number) : OptionPane
        {
            var result = new VRS.OptionPane({
                name: 'infoWindow',
                titleKey: 'PaneInfoWindow',
                displayOrder: displayOrder
            });
            var options = this.options;

            var saveAndApplyState = () => {
                this.saveState();
                var settings = this._createSettings();
                this.applyState(settings);
            };

            // Enabled field - users can always turn this off even if they can't configure anything else
            result.addField(new VRS.OptionFieldCheckBox({
                name:           'enable',
                labelKey:       'EnableInfoWindow',
                getValue:       () => options.enabled,
                setValue:       (value) => options.enabled = value,
                saveState:      saveAndApplyState
            }));

            if(VRS.globalOptions.aircraftInfoWindowAllowConfiguration) {
                result.addField(new VRS.OptionFieldCheckBox({
                    name:           'showUnits',
                    labelKey:       'ShowUnits',
                    getValue:       () => options.showUnits,
                    setValue:       (value) => options.showUnits = value,
                    saveState:      saveAndApplyState
                }));

                VRS.renderPropertyHelper.addRenderPropertiesListOptionsToPane({
                    pane:       result,
                    surface:    VRS.RenderSurface.InfoWindow,
                    fieldLabel: 'Columns',
                    getList:    () => options.items,
                    setList:    (value) => options.items = value,
                    saveState:  saveAndApplyState
                });
            }

            return result;
        }

        /**
         * Suspends or resumes updates.
         */
        suspend(onOff: boolean)
        {
            var state = this._getState();
            if(state.suspended !== onOff) {
                state.suspended = onOff;
            }
        }

        /**
         * Displays information for an aircraft.
         */
        showForAircraft(aircraft: Aircraft)
        {
            var state = this._getState();
            state.aircraft = aircraft;
            this._displayDetails(state, true);
        }

        /**
         * Refreshes the display.
         */
        refreshDisplay()
        {
            var state = this._getState();
            this._displayDetails(state, true);
        }

        /**
         * Displays information for the current aircraft.
         */
        private _displayDetails(state: AircraftInfoWindowPlugin_State, forceRefresh?: boolean)
        {
            var options = this.options;
            if(state.suspended || state.closedByUser) return;

            if(forceRefresh === undefined) forceRefresh = false;
            var aircraft = state.aircraft;
            var map = options.aircraftPlotter.getMap();
            var mapMarker = options.aircraftPlotter.getAircraftMarker(aircraft);
            var mapInfoWindow = state.mapInfoWindow;

            if(state.anchoredAircraft !== aircraft) forceRefresh = true;

            var length = options.items.length;
            if(options.enabled) {
                for(var i = 0;i < length;++i) {
                    var renderProperty = options.items[i];
                    var handler = VRS.renderPropertyHandlers[renderProperty];
                    if(!handler) throw 'Cannot find the handler for ' + renderProperty;

                    var renderElement = state.itemElements[renderProperty];
                    if(renderElement) {
                        if(!aircraft) renderElement.text('');
                        else if(forceRefresh || handler.hasChangedCallback(aircraft)) {
                            handler.renderToJQuery(renderElement, VRS.RenderSurface.InfoWindow, aircraft, options);
                        }
                    }
                }

                state.linksPlugin.renderForAircraft(state.aircraft, false);
            }

            if(!mapMarker || !options.enabled) {
                if(mapInfoWindow.isOpen) map.closeInfoWindow(mapInfoWindow);
                state.anchoredAircraft = null;
            } else {
                if(forceRefresh) {
                    if(mapInfoWindow.isOpen) map.closeInfoWindow(mapInfoWindow);
                    map.openInfoWindow(mapInfoWindow, mapMarker);
                    state.anchoredAircraft = aircraft;
                }
            }
        }

        /**
         * Called when the selected aircraft changes.
         */
        private _selectedAircraftChanged()
        {
            var selectedAircraft = this.options.aircraftList.getSelectedAircraft();
            this.showForAircraft(selectedAircraft);
        }

        /**
         * Called when the user clicks a map marker.
         */
        private _markerClicked(event: Event, data: IMapMarkerEventArgs)
        {
            var state = this._getState();
            var options = this.options;

            if(state.mapInfoWindow) {
                var aircraft = options.aircraftPlotter.getAircraftForMarkerId(<number>data.id);
                if(aircraft) {
                    state.closedByUser = false;
                    if(!state.mapInfoWindow.isOpen || state.aircraft != aircraft) this.showForAircraft(aircraft);
                }
            }
        }

        /**
         * Called when the user closes the info window manually.
         */
        private _infoWindowClosedByUser(event: Event, data: IMapInfoWindowEventArgs)
        {
            var state = this._getState();
            state.closedByUser = true;
        }

        /**
         * Called when the aircraft list has been updated.
         */
        private _aircraftListUpdated()
        {
            var state = this._getState();
            var options = this.options;

            if(state.aircraft) {
                if(!options.aircraftList.findAircraftById(state.aircraft.id)) this.showForAircraft(null);
                else this._displayDetails(state, false);
            }
        }

        /**
         * Called when the unit display preferences have been changed.
         */
        private _displayUnitChanged()
        {
            var state = this._getState();
            if(state.aircraft) this.refreshDisplay();
        }

        /**
         * Called when the language has been changed.
         */
        private _localeChanged()
        {
            var state = this._getState();
            if(state.aircraft) this.refreshDisplay();
        }
    }

    $.widget('vrs.vrsAircraftInfoWindow', new AircraftInfoWindowPlugin());
}

declare interface JQuery
{
    vrsAircraftInfoWindow();
    vrsAircraftInfoWindow(options: VRS.AircraftInfoWindowPlugin_Options);
    vrsAircraftInfoWindow(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
