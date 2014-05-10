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

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions.currentLocationFixed = VRS.globalOptions.currentLocationFixed || undefined;           // Set to an object of { lat: 1.234, lng: 5.678 }; to force the default current location (when the user has not assigned a location) to a fixed point rather than the server-configured initial location.
    VRS.globalOptions.currentLocationConfigurable = VRS.globalOptions.currentLocationConfigurable !== undefined ? VRS.globalOptions.currentLocationConfigurable : true; // True if the user is allowed to set their current location.
    VRS.globalOptions.currentLocationIconUrl = VRS.globalOptions.currentLocationIconUrl || null;            // The icon to display on the map for the set current location marker.
    VRS.globalOptions.currentLocationUseGeoLocation = VRS.globalOptions.currentLocationUseGeoLocation !== undefined ? VRS.globalOptions.currentLocationUseGeoLocation : true; // True if the option to use the browser's current location should be shown.
    VRS.globalOptions.currentLocationUseBrowserLocation = VRS.globalOptions.currentLocationUseBrowserLocation !== undefined ? VRS.globalOptions.currentLocationUseBrowserLocation : VRS.globalOptions.isMobile;  // True if the browser location should be used as the current location. This overrides the map centre / user-supplied location options.
    VRS.globalOptions.currentLocationShowOnMap = VRS.globalOptions.currentLocationShowOnMap !== undefined ? VRS.globalOptions.currentLocationShowOnMap : true;      // True if the current location should be shown on the map
    VRS.globalOptions.currentLocationImageUrl = VRS.globalOptions.currentLocationImageUrl || 'images/location.png';   // The URL of the current location marker.
    VRS.globalOptions.currentLocationImageSize = VRS.globalOptions.currentLocationImageSize || { width: 10, height: 10 }; // The size of the current location marker.
    VRS.globalOptions.currentLocationUseMapCentreForFirstVisit = VRS.globalOptions.currentLocationUseMapCentreForFirstVisit != undefined ? VRS.globalOptions.currentLocationUseMapCentreForFirstVisit : true;   // If true then on the first visit the user-supplied current location is set to the map centre. If false then the user must always choose a current location (i.e. the same behaviour as version 1 of the site).
    //endregion

    //region CurrentLocation
    /**
     * An object that can manage the browser's current location.
     * @param {Object}       settings                               This is optional, it's just that WebStorm refuses to acknowledge the other properties if I mark it as such.
     * @param {string}      [settings.name]                         The name to use when saving and loading state.
     * @param {VRS.vrsMap}  [settings.mapForApproximateLocation]    The map to use to obtain the approximate location.
     * @constructor
     */
    VRS.CurrentLocation = function(settings)
    {
        // Initialise settings
        settings = $.extend({
            name:                       'default',
            mapForApproximateLocation:  null
        }, settings);

        //region -- Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.CurrentLocation'
        });
        var _Events = {
            currentLocationChanged: 'currentLocationChanged'
        };

        /**
         * The map marker used to manually set the user's current location.
         * @type {VRS.MapMarker}
         * @private
         */
        var _SetCurrentLocationMarker = null;

        /**
         * The map marker used to display the current location.
         * @type {VRS.MapMarker}
         * @private
         */
        var _CurrentLocationMarker = null;

        /**
         * The position at which the current location marker has been plotted.
         * @type {VRS_LAT_LNG}
         * @private
         */
        var _PlottedCurrentLocation = null;

        /**
         * The map to display the location pin on. This might not be the same as the map for approximate location.
         * @type {jQuery=}
         * @private */
        var _MapForDisplay = undefined;

        /**
         * A direct reference to the map plugin for the approximate location map.
         * @type {VRS.vrsMap}
         * @private
         */
        var _MapForApproximateLocationPlugin = null;

        /**
         * The hook result from the "map for approximate location"'s centre changed event.
         * @type {Object}
         * @private
         */
        var _MapForApproximateLocationCentreChangedHookResult = null;

        /**
         * The hook result from the map marker dragged event.
         * @type {Object}
         * @private
         */
        var _MapMarkerDraggedHookResult = null;
        //endregion

        //region -- Properties
        /** @type {string} @private */
        var _Name = settings.name;
        this.getName = function() { return _Name; };

        /** @type {{lat:number, lng:number}=} @private */
        var _CurrentLocation = VRS.globalOptions.currentLocationFixed;
        this.getCurrentLocation = function() { return _CurrentLocation; };
        /**
         * Sets the current location.
         * @param {{lat:number, lng:number}=} value
         */
        this.setCurrentLocation = function(value) {
            if(value && _CurrentLocation !== value) {
                _CurrentLocation = value;
                showCurrentLocationOnMap();
                _Dispatcher.raise(_Events.currentLocationChanged);
            }
        };

        var _GeoLocationAvailable = 'geolocation' in navigator;
        /**
         * Gets a value indicating whether we're allowed to use the browser's current location.
         * @returns {boolean}
         */
        this.getGeoLocationAvailable = function() {
            return VRS.globalOptions.currentLocationUseGeoLocation && _GeoLocationAvailable;
        };

        /** @type {VRS_LAT_LNG} @private */
        var _LastBrowserLocation = null;
        /**
         * Gets the last reported browser location. This can be null if the browser has never reported a position.
         * @returns {VRS_LAT_LNG}
         */
        this.getLastBrowserLocation = function() { return _LastBrowserLocation; };

        var _GeoLocationHandlersInstalled = false;
        var _UseBrowserLocation = VRS.globalOptions.currentLocationUseBrowserLocation;
        /**
         * Gets a value indicating that the user wants the current location to be supplied by the browser. This overrides
         * all other options.
         * @returns {boolean}
         */
        this.getUseBrowserLocation = function() { return _UseBrowserLocation; };
        this.setUseBrowserLocation = function(/** boolean */ value) {
            _UseBrowserLocation = value;
            if(!_UseBrowserLocation || !that.getGeoLocationAvailable()) {
                if(that.getUserHasAssignedCurrentLocation()) that.setCurrentLocation(_UserSuppliedCurrentLocation);
                else                                         that.setCurrentLocation(_MapCentreLocation);
            } else {
                if(!_GeoLocationHandlersInstalled) {
                    _GeoLocationHandlersInstalled = true;

                    var usePosition = function(position) {
                        _LastBrowserLocation = { lat: position.coords.latitude, lng: position.coords.longitude };
                        if(that.getBrowserIsSupplyingLocation()) that.setCurrentLocation(_LastBrowserLocation);
                    };

                    navigator.geolocation.getCurrentPosition(function(position) {
                        usePosition(position);
                        navigator.geolocation.watchPosition(
                            usePosition,
                            function(/** PositionError */ error) {
                                _LastBrowserLocation = null;
                            }
                        );
                    });
                }
            }
        };

        /**
         * Gets a value indicating that the browser is supplying the current location.
         * @returns {boolean}
         */
        this.getBrowserIsSupplyingLocation = function() {
            return !!(_UseBrowserLocation && _LastBrowserLocation);
        };

        /** @type {VRS_LAT_LNG} @private */
        var _UserSuppliedCurrentLocation = null;
        /**
         * Gets the current location supplied by the user.
         * @returns {VRS_LAT_LNG}
         */
        this.getUserSuppliedCurrentLocation = function() { return _UserSuppliedCurrentLocation; };
        this.setUserSuppliedCurrentLocation = function(/** VRS_LAT_LNG */ value) {
            if(value && value !== _UserSuppliedCurrentLocation) {
                _UserSuppliedCurrentLocation = value;
                if(!that.getBrowserIsSupplyingLocation() && value) that.setCurrentLocation(value);
            }
        };

        /**
         * Gets a value indicating that the user has supplied a current location.
         * @returns {boolean}
         */
        this.getUserHasAssignedCurrentLocation = function() { return _UserSuppliedCurrentLocation !== null; };

        /** @type {VRS_LAT_LNG} @private */
        var _MapCentreLocation = null;

        this.getMapCentreLocation = function() { return _MapCentreLocation; };
        this.setMapCentreLocation = function(/** VRS_LAT_LNG */ value) {
            if(value && value !== _MapCentreLocation) {
                _MapCentreLocation = value;
                if(that.getMapIsSupplyingLocation()) that.setCurrentLocation(value);
            }
        };

        /**
         * Returns true if the current location is the map centre.
         * @returns {boolean}
         */
        this.getMapIsSupplyingLocation = function() { return !that.getUserHasAssignedCurrentLocation() && !that.getBrowserIsSupplyingLocation(); };

        /** @type {jQuery} @private */
        var _MapForApproximateLocation = null;
        this.getMapForApproximateLocation = function() { return _MapForApproximateLocation; };
        this.setMapForApproximateLocation = function(/** jQuery */ value) {
            if(_MapForApproximateLocationCentreChangedHookResult) {
                _MapForApproximateLocationPlugin.unhook(_MapForApproximateLocationCentreChangedHookResult);
                _MapForApproximateLocationCentreChangedHookResult = null;
            }
            _MapForApproximateLocation = value;
            if(_MapForApproximateLocation != null) {
                _MapForApproximateLocationPlugin = VRS.jQueryUIHelper.getMapPlugin(_MapForApproximateLocation);
                _MapForApproximateLocationCentreChangedHookResult = _MapForApproximateLocationPlugin.hookCenterChanged(mapForApproximateLocationCentreChanged, this);

                if(VRS.globalOptions.currentLocationUseMapCentreForFirstVisit && !_UserSuppliedCurrentLocation) {
                    var centre = _MapForApproximateLocationPlugin.getCenter();
                    that.setUserSuppliedCurrentLocation(centre);

                    // Load the settings. If there were no settings then it'll return the current centre, which is the
                    // map centre. We can then safely save the settings with the map centre and from then on it'll use
                    // them, regardless of where the user drags the map. If the user sets their own location then this
                    // will also stop us from overwriting it.
                    var settings = that.loadState();
                    if(settings.userSuppliedLocation.lat === centre.lat && settings.userSuppliedLocation.lng === centre.lng) {
                        that.applyState(settings);
                        that.saveState();
                    }
                }
            }

            showCurrentLocationOnMap();
            determineLocationFromMap();
        };

        this.getIsSetCurrentLocationMarkerDisplayed = function() { return !!(_SetCurrentLocationMarker); };
        /**
         * Sets a value indicating that the current location marker is currently being displayed on the map.
         * @param {boolean} value
         */
        this.setIsSetCurrentLocationMarkerDisplayed = function(value) {
            if(value !== that.getIsSetCurrentLocationMarkerDisplayed()) showOrHideSetCurrentLocationMarker(value);
        };

        /** @type {boolean} @private */
        var _ShowCurrentLocationOnMap = VRS.globalOptions.currentLocationShowOnMap;
        this.getShowCurrentLocationOnMap = function() { return _ShowCurrentLocationOnMap; };
        this.setShowCurrentLocationOnMap = function(/** boolean */ value) {
            if(_ShowCurrentLocationOnMap !== value) {
                _ShowCurrentLocationOnMap = value;
                showCurrentLocationOnMap();
            }
        };
        //endregion

        //region -- Construction
        this.setMapForApproximateLocation(settings.mapForApproximateLocation);
        //endregion

        //region -- Events exposed
        //noinspection JSUnusedGlobalSymbols
        this.hookCurrentLocationChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.currentLocationChanged, callback, forceThis); };

        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- dispose
        /**
         * Releases the resources held by the object.
         */
        this.dispose = function()
        {
            destroyCurrentLocationMarker();
            showOrHideSetCurrentLocationMarker(false);

            if(_MapForApproximateLocationCentreChangedHookResult) {
                _MapForApproximateLocationPlugin.unhook(_MapForApproximateLocationCentreChangedHookResult);
                _MapForApproximateLocationCentreChangedHookResult = null;
            }
            _MapForApproximateLocation = null;
        };
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Returns the previously saved state or the current state if no state has been saved.
         * @returns {VRS_STATE_CURRENTLOCATION}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            return $.extend(createSettings(), savedSettings);
        };

        /**
         * Applies the previously saved state to the object.
         * @param {VRS_STATE_CURRENTLOCATION} settings
         */
        this.applyState = function(settings)
        {
            that.setUserSuppliedCurrentLocation(settings.userSuppliedLocation);
            that.setUseBrowserLocation(settings.useBrowserLocation);
            that.setShowCurrentLocationOnMap(settings.showCurrentLocation);
        };

        /**
         * Loads and applies the previously saved state to the object.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key to save the state against.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsCurrentLocation-' + that.getName();
        }

        /**
         * Returns the current state of the object.
         * @returns {VRS_STATE_CURRENTLOCATION}
         */
        function createSettings()
        {
            return {
                userSuppliedLocation:   that.getUserSuppliedCurrentLocation(),
                useBrowserLocation:     that.getUseBrowserLocation(),
                showCurrentLocation:    that.getShowCurrentLocationOnMap()
            };
        }
        //endregion

        //region -- createOptionPane
        /**
         * Returns the option pane that allows the object to be configured.
         * @param {number} displayOrder
         * @param {jQuery} mapForLocationDisplay
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder, mapForLocationDisplay)
        {
            var pane = new VRS.OptionPane({
                name:           'vrsCurrentLocation' + that.getName(),
                titleKey:       'PaneCurrentLocation',
                displayOrder:   displayOrder
            });

            if(!mapForLocationDisplay) mapForLocationDisplay = that.getMapForApproximateLocation();
            if(mapForLocationDisplay && VRS.globalOptions.currentLocationConfigurable) {
                _MapForDisplay = mapForLocationDisplay;

                pane.addField(new VRS.OptionFieldLabel({
                    name:           'instructions',
                    labelKey:       'CurrentLocationInstruction'
                }));
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'setCurrentLocation',
                    labelKey:       'SetCurrentLocation',
                    getValue:       that.getIsSetCurrentLocationMarkerDisplayed,
                    setValue:       that.setIsSetCurrentLocationMarkerDisplayed
                }));
            }
            if(VRS.globalOptions.currentLocationUseGeoLocation && that.getGeoLocationAvailable()) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'useBrowserLocation',
                    labelKey:       'UseBrowserLocation',
                    getValue:       that.getUseBrowserLocation,
                    setValue:       that.setUseBrowserLocation,
                    saveState:      that.saveState
                }));
            }

            pane.addField(new VRS.OptionFieldCheckBox({
                name:           'showCurrentLocation',
                labelKey:       'ShowCurrentLocation',
                getValue:       that.getShowCurrentLocationOnMap,
                setValue:       that.setShowCurrentLocationOnMap,
                saveState:      that.saveState,
                keepWithNext:   true
            }));

            pane.addField(new VRS.OptionFieldLabel({
                name:           'currentLocationValue',
                labelKey:       function() {
                    var location = that.getCurrentLocation();
                    var lat = location && location.lat !== undefined ? VRS.stringUtility.formatNumber(location.lat, 'N5') : '';
                    var lng = location && location.lng !== undefined ? VRS.stringUtility.formatNumber(location.lng, 'N5') : '';
                    return lat && lng ? ' (' + lat + ' / ' + lng + ')' : '';
                }
            }));

            return pane;
        };
        //endregion

        //region -- determineLocationFromMap
        /**
         * Sets the current location to the map centre.
         */
        function determineLocationFromMap()
        {
            if(_MapForApproximateLocationPlugin) {
                var centre = _MapForApproximateLocationPlugin.getCenter();
                if(centre) that.setMapCentreLocation(centre);
            }
        }
        //endregion

        //region -- showOrHideSetCurrentLocationMarker, setCurrentLocationMarkerDragged
        /**
         * Hides or shows the current location based on the setting of isMarkerDisplayed.
         * @params {boolean} showMarker
         */
        function showOrHideSetCurrentLocationMarker(showMarker)
        {
            if(_MapForDisplay) {
                var plugin = VRS.jQueryUIHelper.getMapPlugin(_MapForDisplay);

                if(!showMarker) {
                    if(_MapMarkerDraggedHookResult) plugin.unhook(_MapMarkerDraggedHookResult);
                    if(_SetCurrentLocationMarker)   plugin.destroyMarker(_SetCurrentLocationMarker);
                    _MapMarkerDraggedHookResult = null;
                    _SetCurrentLocationMarker = null;
                } else {
                    var markerOptions = {
                        animateAdd:     true,
                        clickable:      false,
                        draggable:      true,
                        flat:           true,
                        optimized:      false,
                        raiseOnDrag:    true,
                        visible:        true,
                        zIndex:         200
                    };
                    var currentLocation = that.getUserSuppliedCurrentLocation() || that.getCurrentLocation();
                    if(currentLocation) markerOptions.position = currentLocation;
                    if(VRS.globalOptions.currentLocationIconUrl) markerOptions.icon = VRS.globalOptions.currentLocationIconUrl;

                    _SetCurrentLocationMarker = plugin.addMarker('setCurrentLocation', markerOptions);
                    _MapMarkerDraggedHookResult = plugin.hookMarkerDragged(setCurrentLocationMarkerDragged);

                    if(currentLocation) plugin.panTo(currentLocation);
                }
            }
        }

        /**
         * Called when the user has finished dragging the marker representing the current location on the map.
         * @param {Event}                   event
         * @param {{ id: string|number }}   data
         */
        function setCurrentLocationMarkerDragged(event, data)
        {
            if(_SetCurrentLocationMarker && data.id === _SetCurrentLocationMarker.id) {
                var plugin = VRS.jQueryUIHelper.getMapPlugin(_MapForDisplay);
                that.setUserSuppliedCurrentLocation(_SetCurrentLocationMarker.getPosition());
                that.saveState();
            }
        }

        /**
         * Called when the user drags the approximate location map around.
         */
        function mapForApproximateLocationCentreChanged()
        {
            determineLocationFromMap();
        }
        //endregion

        //region -- showCurrentLocationOnMap
        /**
         * Shows, hides or moves the current location marker on the map.
         */
        function showCurrentLocationOnMap()
        {
            if(_MapForApproximateLocation) {
                var plugin = _MapForApproximateLocationPlugin;
                var currentLocation = that.getCurrentLocation();
                var showCurrentLocation = that.getShowCurrentLocationOnMap();

                if(!currentLocation || !showCurrentLocation) destroyCurrentLocationMarker();
                else {
                    if(_CurrentLocationMarker) {
                        if(_PlottedCurrentLocation.lat !== currentLocation.lat || _PlottedCurrentLocation.lng !== currentLocation.lng) {
                            _PlottedCurrentLocation = currentLocation;
                            _CurrentLocationMarker.setPosition(_PlottedCurrentLocation);
                        }
                    } else {
                        _PlottedCurrentLocation = currentLocation;
                        _CurrentLocationMarker = plugin.addMarker('staticCurrentLocationMarker', {
                            clickable: false,
                            draggable: false,
                            flat: true,
                            optimized: false,
                            visible: true,
                            position: _PlottedCurrentLocation,
                            icon: new VRS.MapIcon(
                                VRS.globalOptions.currentLocationImageUrl,
                                VRS.globalOptions.currentLocationImageSize,
                                null,
                                null,
                                VRS.globalOptions.currentLocationImageSize
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
        function destroyCurrentLocationMarker()
        {
            if(_CurrentLocationMarker) {
                var plugin = _MapForApproximateLocationPlugin;
                plugin.destroyMarker(_CurrentLocationMarker);
                _CurrentLocationMarker = null;
                _PlottedCurrentLocation = null;
            }
        }
        //endregion
    };
    //endregion

    //region Pre-builts
    /**
     * The singleton instance of the VRS.CurrentLocation object.
     * @type {VRS.CurrentLocation}
     */
    VRS.currentLocation = new VRS.CurrentLocation();
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
