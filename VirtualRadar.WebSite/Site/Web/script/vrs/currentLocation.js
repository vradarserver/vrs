var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.currentLocationFixed = VRS.globalOptions.currentLocationFixed || undefined;
    VRS.globalOptions.currentLocationConfigurable = VRS.globalOptions.currentLocationConfigurable !== undefined ? VRS.globalOptions.currentLocationConfigurable : true;
    VRS.globalOptions.currentLocationIconUrl = VRS.globalOptions.currentLocationIconUrl || null;
    VRS.globalOptions.currentLocationUseGeoLocation = VRS.globalOptions.currentLocationUseGeoLocation !== undefined ? VRS.globalOptions.currentLocationUseGeoLocation : true;
    VRS.globalOptions.currentLocationUseBrowserLocation = VRS.globalOptions.currentLocationUseBrowserLocation !== undefined ? VRS.globalOptions.currentLocationUseBrowserLocation : VRS.globalOptions.isMobile;
    VRS.globalOptions.currentLocationShowOnMap = VRS.globalOptions.currentLocationShowOnMap !== undefined ? VRS.globalOptions.currentLocationShowOnMap : true;
    VRS.globalOptions.currentLocationImageUrl = VRS.globalOptions.currentLocationImageUrl || 'images/location.png';
    VRS.globalOptions.currentLocationImageSize = VRS.globalOptions.currentLocationImageSize || { width: 10, height: 10 };
    VRS.globalOptions.currentLocationUseMapCentreForFirstVisit = VRS.globalOptions.currentLocationUseMapCentreForFirstVisit != undefined ? VRS.globalOptions.currentLocationUseMapCentreForFirstVisit : true;
    var CurrentLocation = (function () {
        function CurrentLocation(settings) {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.CurrentLocation'
            });
            this._Events = {
                currentLocationChanged: 'currentLocationChanged'
            };
            this._SetCurrentLocationMarker = null;
            this._CurrentLocationMarker = null;
            this._PlottedCurrentLocation = null;
            this._MapForDisplay = undefined;
            this._MapForApproximateLocationPlugin = null;
            this._MapForApproximateLocationCentreChangedHookResult = null;
            this._MapMarkerDraggedHookResult = null;
            this._CurrentLocation = VRS.globalOptions.currentLocationFixed;
            this._LastBrowserLocation = null;
            this._GeoLocationHandlersInstalled = false;
            this._UseBrowserLocation = VRS.globalOptions.currentLocationUseBrowserLocation;
            this._UserSuppliedCurrentLocation = null;
            this._MapCentreLocation = null;
            this._MapForApproximateLocation = null;
            this._ShowCurrentLocationOnMap = VRS.globalOptions.currentLocationShowOnMap;
            settings = $.extend({
                name: 'default',
                mapForApproximateLocation: null
            }, settings);
            this._Name = settings.name;
            this._GeoLocationAvailable = 'geolocation' in navigator;
            this.setMapForApproximateLocation(settings.mapForApproximateLocation);
        }
        CurrentLocation.prototype.getName = function () {
            return this._Name;
        };
        CurrentLocation.prototype.getCurrentLocation = function () {
            return this._CurrentLocation;
        };
        CurrentLocation.prototype.setCurrentLocation = function (value) {
            if (value && this._CurrentLocation !== value) {
                this._CurrentLocation = value;
                this.showCurrentLocationOnMap();
                this._Dispatcher.raise(this._Events.currentLocationChanged);
            }
        };
        CurrentLocation.prototype.getGeoLocationAvailable = function () {
            return VRS.globalOptions.currentLocationUseGeoLocation && this._GeoLocationAvailable;
        };
        CurrentLocation.prototype.getLastBrowserLocation = function () {
            return this._LastBrowserLocation;
        };
        CurrentLocation.prototype.getUseBrowserLocation = function () {
            return this._UseBrowserLocation;
        };
        CurrentLocation.prototype.setUseBrowserLocation = function (value) {
            this._UseBrowserLocation = value;
            if (!this._UseBrowserLocation || !this.getGeoLocationAvailable()) {
                if (this.getUserHasAssignedCurrentLocation())
                    this.setCurrentLocation(this._UserSuppliedCurrentLocation);
                else
                    this.setCurrentLocation(this._MapCentreLocation);
            }
            else {
                if (!this._GeoLocationHandlersInstalled) {
                    this._GeoLocationHandlersInstalled = true;
                    var usePosition = function (position) {
                        this._LastBrowserLocation = { lat: position.coords.latitude, lng: position.coords.longitude };
                        if (this.getBrowserIsSupplyingLocation())
                            this.setCurrentLocation(this._LastBrowserLocation);
                    };
                    var self = this;
                    navigator.geolocation.getCurrentPosition(function (position) {
                        usePosition(position);
                        navigator.geolocation.watchPosition(usePosition, function (error) {
                            self._LastBrowserLocation = null;
                        });
                    });
                }
            }
        };
        CurrentLocation.prototype.getBrowserIsSupplyingLocation = function () {
            return !!(this._UseBrowserLocation && this._LastBrowserLocation);
        };
        CurrentLocation.prototype.getUserSuppliedCurrentLocation = function () {
            return this._UserSuppliedCurrentLocation;
        };
        CurrentLocation.prototype.setUserSuppliedCurrentLocation = function (value) {
            if (value && value !== this._UserSuppliedCurrentLocation) {
                this._UserSuppliedCurrentLocation = value;
                if (!this.getBrowserIsSupplyingLocation() && value)
                    this.setCurrentLocation(value);
            }
        };
        CurrentLocation.prototype.getUserHasAssignedCurrentLocation = function () {
            return this._UserSuppliedCurrentLocation !== null;
        };
        CurrentLocation.prototype.getMapCentreLocation = function () {
            return this._MapCentreLocation;
        };
        CurrentLocation.prototype.setMapCentreLocation = function (value) {
            if (value && value !== this._MapCentreLocation) {
                this._MapCentreLocation = value;
                if (this.getMapIsSupplyingLocation()) {
                    this.setCurrentLocation(value);
                }
            }
        };
        CurrentLocation.prototype.getMapIsSupplyingLocation = function () {
            return !this.getUserHasAssignedCurrentLocation() && !this.getBrowserIsSupplyingLocation();
        };
        CurrentLocation.prototype.getMapForApproximateLocation = function () {
            return this._MapForApproximateLocation;
        };
        CurrentLocation.prototype.setMapForApproximateLocation = function (value) {
            if (this._MapForApproximateLocationCentreChangedHookResult) {
                this._MapForApproximateLocationPlugin.unhook(this._MapForApproximateLocationCentreChangedHookResult);
                this._MapForApproximateLocationCentreChangedHookResult = null;
            }
            this._MapForApproximateLocation = value;
            if (this._MapForApproximateLocation != null) {
                this._MapForApproximateLocationPlugin = VRS.jQueryUIHelper.getMapPlugin(this._MapForApproximateLocation);
                this._MapForApproximateLocationCentreChangedHookResult = this._MapForApproximateLocationPlugin.hookCenterChanged(this.mapForApproximateLocationCentreChanged, this);
                if (VRS.globalOptions.currentLocationUseMapCentreForFirstVisit && !this._UserSuppliedCurrentLocation) {
                    var centre = this._MapForApproximateLocationPlugin.getCenter();
                    this.setUserSuppliedCurrentLocation(centre);
                    var settings = this.loadState();
                    if (settings.userSuppliedLocation.lat === centre.lat && settings.userSuppliedLocation.lng === centre.lng) {
                        this.applyState(settings);
                        this.saveState();
                    }
                }
            }
            this.showCurrentLocationOnMap();
            this.determineLocationFromMap();
        };
        CurrentLocation.prototype.getIsSetCurrentLocationMarkerDisplayed = function () {
            return !!(this._SetCurrentLocationMarker);
        };
        CurrentLocation.prototype.setIsSetCurrentLocationMarkerDisplayed = function (value) {
            if (value !== this.getIsSetCurrentLocationMarkerDisplayed()) {
                this.showOrHideSetCurrentLocationMarker(value);
            }
        };
        CurrentLocation.prototype.getShowCurrentLocationOnMap = function () {
            return this._ShowCurrentLocationOnMap;
        };
        CurrentLocation.prototype.setShowCurrentLocationOnMap = function (value) {
            if (this._ShowCurrentLocationOnMap !== value) {
                this._ShowCurrentLocationOnMap = value;
                this.showCurrentLocationOnMap();
            }
        };
        CurrentLocation.prototype.hookCurrentLocationChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.currentLocationChanged, callback, forceThis);
        };
        CurrentLocation.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        CurrentLocation.prototype.dispose = function () {
            this.destroyCurrentLocationMarker();
            this.showOrHideSetCurrentLocationMarker(false);
            if (this._MapForApproximateLocationCentreChangedHookResult) {
                this._MapForApproximateLocationPlugin.unhook(this._MapForApproximateLocationCentreChangedHookResult);
                this._MapForApproximateLocationCentreChangedHookResult = null;
            }
            this._MapForApproximateLocation = null;
        };
        CurrentLocation.prototype.saveState = function () {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        };
        CurrentLocation.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        };
        CurrentLocation.prototype.applyState = function (settings) {
            this.setUserSuppliedCurrentLocation(settings.userSuppliedLocation);
            this.setUseBrowserLocation(settings.useBrowserLocation);
            this.setShowCurrentLocationOnMap(settings.showCurrentLocation);
        };
        CurrentLocation.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        CurrentLocation.prototype.persistenceKey = function () {
            return 'vrsCurrentLocation-' + this.getName();
        };
        CurrentLocation.prototype.createSettings = function () {
            return {
                userSuppliedLocation: this.getUserSuppliedCurrentLocation(),
                useBrowserLocation: this.getUseBrowserLocation(),
                showCurrentLocation: this.getShowCurrentLocationOnMap()
            };
        };
        CurrentLocation.prototype.createOptionPane = function (displayOrder, mapForLocationDisplay) {
            var self = this;
            var pane = new VRS.OptionPane({
                name: 'vrsCurrentLocation' + this.getName(),
                titleKey: 'PaneCurrentLocation',
                displayOrder: displayOrder
            });
            if (!mapForLocationDisplay)
                mapForLocationDisplay = this.getMapForApproximateLocation();
            if (mapForLocationDisplay && VRS.globalOptions.currentLocationConfigurable) {
                this._MapForDisplay = mapForLocationDisplay;
                pane.addField(new VRS.OptionFieldLabel({
                    name: 'instructions',
                    labelKey: 'CurrentLocationInstruction'
                }));
                pane.addField(new VRS.OptionFieldCheckBox({
                    name: 'setCurrentLocation',
                    labelKey: 'SetCurrentLocation',
                    getValue: this.getIsSetCurrentLocationMarkerDisplayed,
                    setValue: this.setIsSetCurrentLocationMarkerDisplayed
                }));
            }
            if (VRS.globalOptions.currentLocationUseGeoLocation && this.getGeoLocationAvailable()) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name: 'useBrowserLocation',
                    labelKey: 'UseBrowserLocation',
                    getValue: this.getUseBrowserLocation,
                    setValue: this.setUseBrowserLocation,
                    saveState: this.saveState
                }));
            }
            pane.addField(new VRS.OptionFieldCheckBox({
                name: 'showCurrentLocation',
                labelKey: 'ShowCurrentLocation',
                getValue: this.getShowCurrentLocationOnMap,
                setValue: this.setShowCurrentLocationOnMap,
                saveState: this.saveState,
                keepWithNext: true
            }));
            pane.addField(new VRS.OptionFieldLabel({
                name: 'currentLocationValue',
                labelKey: function () {
                    var location = self.getCurrentLocation();
                    var lat = location && location.lat !== undefined ? VRS.stringUtility.formatNumber(location.lat, 'N5') : '';
                    var lng = location && location.lng !== undefined ? VRS.stringUtility.formatNumber(location.lng, 'N5') : '';
                    return lat && lng ? ' (' + lat + ' / ' + lng + ')' : '';
                }
            }));
            return pane;
        };
        CurrentLocation.prototype.determineLocationFromMap = function () {
            if (this._MapForApproximateLocationPlugin) {
                var centre = this._MapForApproximateLocationPlugin.getCenter();
                if (centre)
                    this.setMapCentreLocation(centre);
            }
        };
        CurrentLocation.prototype.showOrHideSetCurrentLocationMarker = function (showMarker) {
            if (this._MapForDisplay) {
                var plugin = VRS.jQueryUIHelper.getMapPlugin(this._MapForDisplay);
                if (!showMarker) {
                    if (this._MapMarkerDraggedHookResult)
                        plugin.unhook(this._MapMarkerDraggedHookResult);
                    if (this._SetCurrentLocationMarker)
                        plugin.destroyMarker(this._SetCurrentLocationMarker);
                    this._MapMarkerDraggedHookResult = null;
                    this._SetCurrentLocationMarker = null;
                }
                else {
                    var markerOptions = {
                        animateAdd: true,
                        clickable: false,
                        draggable: true,
                        flat: true,
                        optimized: false,
                        raiseOnDrag: true,
                        visible: true,
                        zIndex: 200
                    };
                    var currentLocation = this.getUserSuppliedCurrentLocation() || this.getCurrentLocation();
                    if (currentLocation)
                        markerOptions.position = currentLocation;
                    if (VRS.globalOptions.currentLocationIconUrl)
                        markerOptions.icon = VRS.globalOptions.currentLocationIconUrl;
                    this._SetCurrentLocationMarker = plugin.addMarker('setCurrentLocation', markerOptions);
                    this._MapMarkerDraggedHookResult = plugin.hookMarkerDragged(this.setCurrentLocationMarkerDragged);
                    if (currentLocation)
                        plugin.panTo(currentLocation);
                }
            }
        };
        CurrentLocation.prototype.setCurrentLocationMarkerDragged = function (event, data) {
            if (this._SetCurrentLocationMarker && data.id === this._SetCurrentLocationMarker.id) {
                var plugin = VRS.jQueryUIHelper.getMapPlugin(this._MapForDisplay);
                this.setUserSuppliedCurrentLocation(this._SetCurrentLocationMarker.getPosition());
                this.saveState();
            }
        };
        CurrentLocation.prototype.mapForApproximateLocationCentreChanged = function () {
            this.determineLocationFromMap();
        };
        CurrentLocation.prototype.showCurrentLocationOnMap = function () {
            if (this._MapForApproximateLocation) {
                var plugin = this._MapForApproximateLocationPlugin;
                var currentLocation = this.getCurrentLocation();
                var showCurrentLocation = this.getShowCurrentLocationOnMap();
                if (!currentLocation || !showCurrentLocation) {
                    this.destroyCurrentLocationMarker();
                }
                else {
                    if (this._CurrentLocationMarker) {
                        if (this._PlottedCurrentLocation.lat !== currentLocation.lat || this._PlottedCurrentLocation.lng !== currentLocation.lng) {
                            this._PlottedCurrentLocation = currentLocation;
                            this._CurrentLocationMarker.setPosition(this._PlottedCurrentLocation);
                        }
                    }
                    else {
                        this._PlottedCurrentLocation = currentLocation;
                        this._CurrentLocationMarker = plugin.addMarker('staticCurrentLocationMarker', {
                            clickable: false,
                            draggable: false,
                            flat: true,
                            optimized: false,
                            visible: true,
                            position: this._PlottedCurrentLocation,
                            icon: new VRS.MapIcon(VRS.globalOptions.currentLocationImageUrl, VRS.globalOptions.currentLocationImageSize, null, null, VRS.globalOptions.currentLocationImageSize, null),
                            zIndex: 0
                        });
                    }
                }
            }
        };
        CurrentLocation.prototype.destroyCurrentLocationMarker = function () {
            if (this._CurrentLocationMarker) {
                var plugin = this._MapForApproximateLocationPlugin;
                plugin.destroyMarker(this._CurrentLocationMarker);
                this._CurrentLocationMarker = null;
                this._PlottedCurrentLocation = null;
            }
        };
        return CurrentLocation;
    })();
    VRS.CurrentLocation = CurrentLocation;
    VRS.currentLocation = new VRS.CurrentLocation();
})(VRS || (VRS = {}));
//# sourceMappingURL=currentLocation.js.map