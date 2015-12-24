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
 * @fileoverview Exposes the current location of the browser.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.currentLocationFixed = VRS.globalOptions.currentLocationFixed || undefined;           // Set to an object of { lat: 1.234, lng: 5.678 }; to force the default current location (when the user has not assigned a location) to a fixed point rather than the server-configured initial location.
    VRS.globalOptions.currentLocationConfigurable = VRS.globalOptions.currentLocationConfigurable !== undefined ? VRS.globalOptions.currentLocationConfigurable : true; // True if the user is allowed to set their current location.
    VRS.globalOptions.currentLocationIconUrl = VRS.globalOptions.currentLocationIconUrl || null;            // The icon to display on the map for the set current location marker.
    VRS.globalOptions.currentLocationUseGeoLocation = VRS.globalOptions.currentLocationUseGeoLocation !== undefined ? VRS.globalOptions.currentLocationUseGeoLocation : true; // True if the option to use the browser's current location should be shown.
    VRS.globalOptions.currentLocationUseBrowserLocation = VRS.globalOptions.currentLocationUseBrowserLocation !== undefined ? VRS.globalOptions.currentLocationUseBrowserLocation : VRS.globalOptions.isMobile;  // True if the browser location should be used as the current location. This overrides the map centre / user-supplied location options.
    VRS.globalOptions.currentLocationShowOnMap = VRS.globalOptions.currentLocationShowOnMap !== undefined ? VRS.globalOptions.currentLocationShowOnMap : true;      // True if the current location should be shown on the map
    VRS.globalOptions.currentLocationImageUrl = VRS.globalOptions.currentLocationImageUrl || 'images/location.png';   // The URL of the current location marker.
    VRS.globalOptions.currentLocationImageSize = VRS.globalOptions.currentLocationImageSize || { width: 10, height: 10 }; // The size of the current location marker.
    VRS.globalOptions.currentLocationUseMapCentreForFirstVisit = VRS.globalOptions.currentLocationUseMapCentreForFirstVisit != undefined ? VRS.globalOptions.currentLocationUseMapCentreForFirstVisit : true;   // If true then on the first visit the user-supplied current location is set to the map centre. If false then the user must always choose a current location (i.e. the same behaviour as version 1 of the site).

    /**
     * The settings to use when creating a new CurrentLocation object.
     */
    export interface CurrentLocation_Settings
    {
        name?:  string;                     // The name to use when saving and loading state.
        mapForApproximateLocation?: JQuery; // The jQuery element containing the map to take the approximate location from.
    }

    /**
     * The settings that records a CurrentLocation object's state.
     */
    interface CurrentLocation_SaveState
    {
        userSuppliedLocation?:  ILatLng;
        useBrowserLocation?:    boolean;
        showCurrentLocation?:   boolean;
    }

    /**
     * An object that keeps track of the user's current location.
     */
    export class CurrentLocation implements ISelfPersist<CurrentLocation_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.CurrentLocation'
        });
        private _Events = {
            currentLocationChanged: 'currentLocationChanged'
        };

        private _SetCurrentLocationMarker: IMapMarker = null;                                   // The map marker used to manually set the user's current location.
        private _CurrentLocationMarker: IMapMarker = null;                                      // The map marker used to display the current location.
        private _PlottedCurrentLocation: ILatLng = null;                                        // The position at which the current location marker has been plotted.
        private _MapForDisplay: JQuery = undefined;                                             // The map to display the location pin on. This might not be the same as the map for approximate location.
        private _MapForApproximateLocationPlugin: IMap = null;                                  // A direct reference to the map plugin for the approximate location map.
        private _MapForApproximateLocationCentreChangedHookResult: IEventHandleJQueryUI = null; // The hook result from the "map for approximate location"'s centre changed event.
        private _MapMarkerDraggedHookResult: IEventHandleJQueryUI = null;                       // The hook result from the map marker dragged event.
        private _Name: string;
        private _CurrentLocation: ILatLng = VRS.globalOptions.currentLocationFixed;
        private _GeoLocationAvailable: boolean;
        private _LastBrowserLocation: ILatLng = null;
        private _GeoLocationHandlersInstalled = false;
        private _UseBrowserLocation: boolean = VRS.globalOptions.currentLocationUseBrowserLocation;
        private _UserSuppliedCurrentLocation: ILatLng = null;
        private _MapCentreLocation: ILatLng = null;
        private _MapForApproximateLocation: JQuery = null;
        private _ShowCurrentLocationOnMap: boolean = VRS.globalOptions.currentLocationShowOnMap;

        constructor(settings?: CurrentLocation_Settings)
        {
            settings = $.extend({
                name:                       'default',
                mapForApproximateLocation:  null
            }, settings);

            this._Name = settings.name;
            this._GeoLocationAvailable = 'geolocation' in navigator;
            this.setMapForApproximateLocation(settings.mapForApproximateLocation);
        }

        /**
         * Gets the name of the object.
         */
        getName = () : string =>
        {
            return this._Name;
        }

        /**
         * Gets the current location.
         */
        getCurrentLocation = () : ILatLng =>
        {
            return this._CurrentLocation;
        }
        setCurrentLocation = (value: ILatLng) =>
        {
            if(value && this._CurrentLocation !== value) {
                this._CurrentLocation = value;
                this.showCurrentLocationOnMap();
                this._Dispatcher.raise(this._Events.currentLocationChanged);
            }
        }

        /**
         * Gets a value indicating whether we're allowed to use the browser's current location.
         */
        getGeoLocationAvailable = () : boolean =>
        {
            return VRS.globalOptions.currentLocationUseGeoLocation && this._GeoLocationAvailable;
        }

        /**
         * Gets the last reported browser location. This can be null if the browser has never reported a position.
         */
        getLastBrowserLocation = () : ILatLng =>
        {
            return this._LastBrowserLocation;
        }

        /**
         * Gets a value indicating that the user wants the current location to be supplied by the browser. This overrides
         * all other options.
         */
        getUseBrowserLocation = () : boolean =>
        {
            return this._UseBrowserLocation;
        }
        setUseBrowserLocation = (value: boolean) =>
        {
            this._UseBrowserLocation = value;
            if(!this._UseBrowserLocation || !this.getGeoLocationAvailable()) {
                if(this.getUserHasAssignedCurrentLocation()) this.setCurrentLocation(this._UserSuppliedCurrentLocation);
                else                                         this.setCurrentLocation(this._MapCentreLocation);
            } else {
                if(this._GeoLocationHandlersInstalled) {
                    if(this.getBrowserIsSupplyingLocation()) {
                        this.setCurrentLocation(this._LastBrowserLocation);
                    }
                } else {
                    this._GeoLocationHandlersInstalled = true;
                    navigator.geolocation.getCurrentPosition((position: Position) => {
                        this.useBrowserPosition(position);
                        navigator.geolocation.watchPosition(
                            this.useBrowserPosition,
                            () => {
                                this._LastBrowserLocation = null
                            }
                        );
                    });
                }
            }
        }
        private useBrowserPosition = (position: Position) =>
        {
            this._LastBrowserLocation = { lat: position.coords.latitude, lng: position.coords.longitude };
            if(this.getBrowserIsSupplyingLocation()) {
                this.setCurrentLocation(this._LastBrowserLocation);
            }
        }

        /**
         * Gets a value indicating that the browser is supplying the current location.
         */
        getBrowserIsSupplyingLocation = () : boolean =>
        {
            return !!(this._UseBrowserLocation && this._LastBrowserLocation);
        }

        /**
         * Gets the current location supplied by the user.
         */
        getUserSuppliedCurrentLocation = () : ILatLng =>
        {
            return this._UserSuppliedCurrentLocation;
        }
        setUserSuppliedCurrentLocation = (value: ILatLng) =>
        {
            if(value && value !== this._UserSuppliedCurrentLocation) {
                this._UserSuppliedCurrentLocation = value;
                if(!this.getBrowserIsSupplyingLocation() && value) this.setCurrentLocation(value);
            }
        }

        /**
         * Gets a value indicating that the user has supplied a current location.
         */
        getUserHasAssignedCurrentLocation = () : boolean =>
        {
            return this._UserSuppliedCurrentLocation !== null;
        }

        /**
         * Gets the centre location from the map that is currently supplying approximate locations.
         */
        getMapCentreLocation = () : ILatLng =>
        {
            return this._MapCentreLocation;
        }
        setMapCentreLocation = (value: ILatLng) =>
        {
            if(value && value !== this._MapCentreLocation) {
                this._MapCentreLocation = value;
                if(this.getMapIsSupplyingLocation()) {
                    this.setCurrentLocation(value);
                }
            }
        }

        /**
         * Returns true if the current location is the map centre.
         */
        getMapIsSupplyingLocation = () : boolean =>
        {
            return !this.getUserHasAssignedCurrentLocation() && !this.getBrowserIsSupplyingLocation();
        }

        /**
         * Gets the JQuery element that holds the map that is being used for approximate locations.
         */
        getMapForApproximateLocation = () : JQuery =>
        {
            return this._MapForApproximateLocation;
        }
        setMapForApproximateLocation = (value: JQuery) =>
        {
            if(this._MapForApproximateLocationCentreChangedHookResult) {
                this._MapForApproximateLocationPlugin.unhook(this._MapForApproximateLocationCentreChangedHookResult);
                this._MapForApproximateLocationCentreChangedHookResult = null;
            }

            this._MapForApproximateLocation = value;
            if(this._MapForApproximateLocation != null) {
                this._MapForApproximateLocationPlugin = VRS.jQueryUIHelper.getMapPlugin(this._MapForApproximateLocation);
                this._MapForApproximateLocationCentreChangedHookResult = this._MapForApproximateLocationPlugin.hookCenterChanged(this.mapForApproximateLocationCentreChanged, this);

                if(VRS.globalOptions.currentLocationUseMapCentreForFirstVisit && !this._UserSuppliedCurrentLocation) {
                    var centre = this._MapForApproximateLocationPlugin.getCenter();
                    this.setUserSuppliedCurrentLocation(centre);

                    // Load the settings. If there were no settings then it'll return the current centre, which is the
                    // map centre. We can then safely save the settings with the map centre and from then on it'll use
                    // them, regardless of where the user drags the map. If the user sets their own location then this
                    // will also stop us from overwriting it.
                    var settings = this.loadState();
                    if(settings.userSuppliedLocation.lat === centre.lat && settings.userSuppliedLocation.lng === centre.lng) {
                        this.applyState(settings);
                        this.saveState();
                    }
                }
            }

            this.showCurrentLocationOnMap();
            this.determineLocationFromMap();
        }

        /**
         * Gets a value indicating that the current location marker is currently being displayed on the map.
         */
        getIsSetCurrentLocationMarkerDisplayed = () : boolean =>
        {
            return !!(this._SetCurrentLocationMarker);
        }
        setIsSetCurrentLocationMarkerDisplayed = (value: boolean) =>
        {
            if(value !== this.getIsSetCurrentLocationMarkerDisplayed()) {
                this.showOrHideSetCurrentLocationMarker(value);
            }
        }

        getShowCurrentLocationOnMap = () : boolean =>
        {
            return this._ShowCurrentLocationOnMap;
        }
        setShowCurrentLocationOnMap = (value: boolean) =>
        {
            if(this._ShowCurrentLocationOnMap !== value) {
                this._ShowCurrentLocationOnMap = value;
                this.showCurrentLocationOnMap();
            }
        }

        /**
         * Hooks an event that is raised when the current location is changed.
         */
        hookCurrentLocationChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.currentLocationChanged, callback, forceThis);
        }

        /**
         * Unhooks an event from the object.
         */
        unhook = (hookResult: IEventHandle) =>
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Releases the resources held by the object.
         */
        dispose = () =>
        {
            this.destroyCurrentLocationMarker();
            this.showOrHideSetCurrentLocationMarker(false);

            if(this._MapForApproximateLocationCentreChangedHookResult) {
                this._MapForApproximateLocationPlugin.unhook(this._MapForApproximateLocationCentreChangedHookResult);
                this._MapForApproximateLocationCentreChangedHookResult = null;
            }
            this._MapForApproximateLocation = null;
        }

        /**
         * Saves the current state of the object.
         */
        saveState = () =>
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Returns the previously saved state or the current state if no state has been saved.
         */
        loadState = () : CurrentLocation_SaveState =>
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        }

        /**
         * Applies the previously saved state to the object.
         */
        applyState = (settings: CurrentLocation_SaveState) =>
        {
            this.setUserSuppliedCurrentLocation(settings.userSuppliedLocation);
            this.setUseBrowserLocation(settings.useBrowserLocation);
            this.setShowCurrentLocationOnMap(settings.showCurrentLocation);
        }

        /**
         * Loads and applies the previously saved state to the object.
         */
        loadAndApplyState = () =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key to save the state against.
         */
        private persistenceKey() : string
        {
            return 'vrsCurrentLocation-' + this.getName();
        }

        /**
         * Returns the current state of the object.
         */
        private createSettings() : CurrentLocation_SaveState
        {
            return {
                userSuppliedLocation:   this.getUserSuppliedCurrentLocation(),
                useBrowserLocation:     this.getUseBrowserLocation(),
                showCurrentLocation:    this.getShowCurrentLocationOnMap()
            };
        }

        /**
         * Returns the option pane that allows the object to be configured.
         */
        createOptionPane = (displayOrder: number, mapForLocationDisplay: JQuery) : OptionPane =>
        {
            var pane = new VRS.OptionPane({
                name:           'vrsCurrentLocation' + this.getName(),
                titleKey:       'PaneCurrentLocation',
                displayOrder:   displayOrder
            });

            if(!mapForLocationDisplay) mapForLocationDisplay = this.getMapForApproximateLocation();
            if(mapForLocationDisplay && VRS.globalOptions.currentLocationConfigurable) {
                this._MapForDisplay = mapForLocationDisplay;

                pane.addField(new VRS.OptionFieldLabel({
                    name:           'instructions',
                    labelKey:       'CurrentLocationInstruction'
                }));
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'setCurrentLocation',
                    labelKey:       'SetCurrentLocation',
                    getValue:       this.getIsSetCurrentLocationMarkerDisplayed,
                    setValue:       this.setIsSetCurrentLocationMarkerDisplayed
                }));
            }
            if(VRS.globalOptions.currentLocationUseGeoLocation && this.getGeoLocationAvailable()) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'useBrowserLocation',
                    labelKey:       'UseBrowserLocation',
                    getValue:       this.getUseBrowserLocation,
                    setValue:       this.setUseBrowserLocation,
                    saveState:      this.saveState
                }));
            }

            pane.addField(new VRS.OptionFieldCheckBox({
                name:           'showCurrentLocation',
                labelKey:       'ShowCurrentLocation',
                getValue:       this.getShowCurrentLocationOnMap,
                setValue:       this.setShowCurrentLocationOnMap,
                saveState:      this.saveState,
                keepWithNext:   true
            }));

            pane.addField(new VRS.OptionFieldLabel({
                name:           'currentLocationValue',
                labelKey:       () => {
                    var location = this.getCurrentLocation();
                    var lat = location && location.lat !== undefined ? VRS.stringUtility.formatNumber(location.lat, 'N5') : '';
                    var lng = location && location.lng !== undefined ? VRS.stringUtility.formatNumber(location.lng, 'N5') : '';
                    return lat && lng ? ' (' + lat + ' / ' + lng + ')' : '';
                }
            }));

            return pane;
        }

        /**
         * Sets the current location to the map centre.
         */
        private determineLocationFromMap = () =>
        {
            if(this._MapForApproximateLocationPlugin) {
                var centre = this._MapForApproximateLocationPlugin.getCenter();
                if(centre) this.setMapCentreLocation(centre);
            }
        }

        /**
         * Hides or shows the current location based on the setting of isMarkerDisplayed.
         */
        private showOrHideSetCurrentLocationMarker = (showMarker: boolean) =>
        {
            if(this._MapForDisplay) {
                var plugin: IMap = VRS.jQueryUIHelper.getMapPlugin(this._MapForDisplay);

                if(!showMarker) {
                    if(this._MapMarkerDraggedHookResult) plugin.unhook(this._MapMarkerDraggedHookResult);
                    if(this._SetCurrentLocationMarker)   plugin.destroyMarker(this._SetCurrentLocationMarker);
                    this._MapMarkerDraggedHookResult = null;
                    this._SetCurrentLocationMarker = null;
                } else {
                    var markerOptions: IMapMarkerSettings = {
                        animateAdd:     true,
                        clickable:      false,
                        draggable:      true,
                        flat:           true,
                        optimized:      false,
                        raiseOnDrag:    true,
                        visible:        true,
                        zIndex:         200
                    };
                    var currentLocation = this.getUserSuppliedCurrentLocation() || this.getCurrentLocation();
                    if(currentLocation) {
                        markerOptions.position = currentLocation;
                    }
                    if(VRS.globalOptions.currentLocationIconUrl) {
                        markerOptions.icon = VRS.globalOptions.currentLocationIconUrl;
                    }

                    this._SetCurrentLocationMarker = plugin.addMarker('setCurrentLocation', markerOptions);
                    this._MapMarkerDraggedHookResult = plugin.hookMarkerDragged(this.setCurrentLocationMarkerDragged);

                    if(currentLocation) plugin.panTo(currentLocation);
                }
            }
        }

        /**
         * Called when the user has finished dragging the marker representing the current location on the map.
         */
        private setCurrentLocationMarkerDragged = (event: Event, data: IMapMarkerEventArgs) =>
        {
            if(this._SetCurrentLocationMarker && data.id === this._SetCurrentLocationMarker.id) {
                var plugin: IMap = VRS.jQueryUIHelper.getMapPlugin(this._MapForDisplay);
                this.setUserSuppliedCurrentLocation(this._SetCurrentLocationMarker.getPosition());
                this.saveState();
            }
        }

        /**
         * Called when the user drags the approximate location map around.
         */
        private mapForApproximateLocationCentreChanged = () =>
        {
            this.determineLocationFromMap();
        }

        /**
         * Shows, hides or moves the current location marker on the map.
         */
        private showCurrentLocationOnMap = () =>
        {
            if(this._MapForApproximateLocation) {
                var plugin = this._MapForApproximateLocationPlugin;
                var currentLocation = this.getCurrentLocation();
                var showCurrentLocation = this.getShowCurrentLocationOnMap();

                if(!currentLocation || !showCurrentLocation) {
                    this.destroyCurrentLocationMarker();
                } else {
                    if(this._CurrentLocationMarker) {
                        if(this._PlottedCurrentLocation.lat !== currentLocation.lat || this._PlottedCurrentLocation.lng !== currentLocation.lng) {
                            this._PlottedCurrentLocation = currentLocation;
                            this._CurrentLocationMarker.setPosition(this._PlottedCurrentLocation);
                        }
                    } else {
                        this._PlottedCurrentLocation = currentLocation;
                        this._CurrentLocationMarker = plugin.addMarker('staticCurrentLocationMarker', {
                            clickable: false,
                            draggable: false,
                            flat: true,
                            optimized: false,
                            visible: true,
                            position: this._PlottedCurrentLocation,
                            icon: new VRS.MapIcon(
                                VRS.globalOptions.currentLocationImageUrl,
                                VRS.globalOptions.currentLocationImageSize,
                                null,
                                null,
                                VRS.globalOptions.currentLocationImageSize,
                                null
                            ),
                            zIndex: 0
                        });
                    }
                }
            }
        }

        /**
         * Destroys the current location marker.
         */
        private destroyCurrentLocationMarker = () =>
        {
            if(this._CurrentLocationMarker) {
                var plugin = this._MapForApproximateLocationPlugin;
                plugin.destroyMarker(this._CurrentLocationMarker);
                this._CurrentLocationMarker = null;
                this._PlottedCurrentLocation = null;
            }
        }
    }

    /*
     * Pre-builts
     */
    export var currentLocation = new VRS.CurrentLocation();
}
 