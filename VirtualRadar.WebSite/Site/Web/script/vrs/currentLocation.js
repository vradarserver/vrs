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
            var _this = this;
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
            this.getName = function () {
                return _this._Name;
            };
            this.getCurrentLocation = function () {
                return _this._CurrentLocation;
            };
            this.setCurrentLocation = function (value) {
                if (value && _this._CurrentLocation !== value) {
                    _this._CurrentLocation = value;
                    _this.showCurrentLocationOnMap();
                    _this._Dispatcher.raise(_this._Events.currentLocationChanged);
                }
            };
            this.getGeoLocationAvailable = function () {
                return VRS.globalOptions.currentLocationUseGeoLocation && _this._GeoLocationAvailable;
            };
            this.getLastBrowserLocation = function () {
                return _this._LastBrowserLocation;
            };
            this.getUseBrowserLocation = function () {
                return _this._UseBrowserLocation;
            };
            this.setUseBrowserLocation = function (value) {
                _this._UseBrowserLocation = value;
                if (!_this._UseBrowserLocation || !_this.getGeoLocationAvailable()) {
                    if (_this.getUserHasAssignedCurrentLocation())
                        _this.setCurrentLocation(_this._UserSuppliedCurrentLocation);
                    else
                        _this.setCurrentLocation(_this._MapCentreLocation);
                }
                else {
                    if (_this._GeoLocationHandlersInstalled) {
                        if (_this.getBrowserIsSupplyingLocation()) {
                            _this.setCurrentLocation(_this._LastBrowserLocation);
                        }
                    }
                    else {
                        _this._GeoLocationHandlersInstalled = true;
                        navigator.geolocation.getCurrentPosition(function (position) {
                            _this.useBrowserPosition(position);
                            navigator.geolocation.watchPosition(_this.useBrowserPosition, function () {
                                _this._LastBrowserLocation = null;
                            });
                        });
                    }
                }
            };
            this.useBrowserPosition = function (position) {
                _this._LastBrowserLocation = { lat: position.coords.latitude, lng: position.coords.longitude };
                if (_this.getBrowserIsSupplyingLocation()) {
                    _this.setCurrentLocation(_this._LastBrowserLocation);
                }
            };
            this.getBrowserIsSupplyingLocation = function () {
                return !!(_this._UseBrowserLocation && _this._LastBrowserLocation);
            };
            this.getUserSuppliedCurrentLocation = function () {
                return _this._UserSuppliedCurrentLocation;
            };
            this.setUserSuppliedCurrentLocation = function (value) {
                if (value && value !== _this._UserSuppliedCurrentLocation) {
                    _this._UserSuppliedCurrentLocation = value;
                    if (!_this.getBrowserIsSupplyingLocation() && value)
                        _this.setCurrentLocation(value);
                }
            };
            this.getUserHasAssignedCurrentLocation = function () {
                return _this._UserSuppliedCurrentLocation !== null;
            };
            this.getMapCentreLocation = function () {
                return _this._MapCentreLocation;
            };
            this.setMapCentreLocation = function (value) {
                if (value && value !== _this._MapCentreLocation) {
                    _this._MapCentreLocation = value;
                    if (_this.getMapIsSupplyingLocation()) {
                        _this.setCurrentLocation(value);
                    }
                }
            };
            this.getMapIsSupplyingLocation = function () {
                return !_this.getUserHasAssignedCurrentLocation() && !_this.getBrowserIsSupplyingLocation();
            };
            this.getMapForApproximateLocation = function () {
                return _this._MapForApproximateLocation;
            };
            this.setMapForApproximateLocation = function (value) {
                if (_this._MapForApproximateLocationCentreChangedHookResult) {
                    _this._MapForApproximateLocationPlugin.unhook(_this._MapForApproximateLocationCentreChangedHookResult);
                    _this._MapForApproximateLocationCentreChangedHookResult = null;
                }
                _this._MapForApproximateLocation = value;
                if (_this._MapForApproximateLocation != null) {
                    _this._MapForApproximateLocationPlugin = VRS.jQueryUIHelper.getMapPlugin(_this._MapForApproximateLocation);
                    _this._MapForApproximateLocationCentreChangedHookResult = _this._MapForApproximateLocationPlugin.hookCenterChanged(_this.mapForApproximateLocationCentreChanged, _this);
                    if (VRS.globalOptions.currentLocationUseMapCentreForFirstVisit && !_this._UserSuppliedCurrentLocation) {
                        var centre = _this._MapForApproximateLocationPlugin.getCenter();
                        _this.setUserSuppliedCurrentLocation(centre);
                        var settings = _this.loadState();
                        if (settings.userSuppliedLocation.lat === centre.lat && settings.userSuppliedLocation.lng === centre.lng) {
                            _this.applyState(settings);
                            _this.saveState();
                        }
                    }
                }
                _this.showCurrentLocationOnMap();
                _this.determineLocationFromMap();
            };
            this.getIsSetCurrentLocationMarkerDisplayed = function () {
                return !!(_this._SetCurrentLocationMarker);
            };
            this.setIsSetCurrentLocationMarkerDisplayed = function (value) {
                if (value !== _this.getIsSetCurrentLocationMarkerDisplayed()) {
                    _this.showOrHideSetCurrentLocationMarker(value);
                }
            };
            this.getShowCurrentLocationOnMap = function () {
                return _this._ShowCurrentLocationOnMap;
            };
            this.setShowCurrentLocationOnMap = function (value) {
                if (_this._ShowCurrentLocationOnMap !== value) {
                    _this._ShowCurrentLocationOnMap = value;
                    _this.showCurrentLocationOnMap();
                }
            };
            this.hookCurrentLocationChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.currentLocationChanged, callback, forceThis);
            };
            this.unhook = function (hookResult) {
                _this._Dispatcher.unhook(hookResult);
            };
            this.dispose = function () {
                _this.destroyCurrentLocationMarker();
                _this.showOrHideSetCurrentLocationMarker(false);
                if (_this._MapForApproximateLocationCentreChangedHookResult) {
                    _this._MapForApproximateLocationPlugin.unhook(_this._MapForApproximateLocationCentreChangedHookResult);
                    _this._MapForApproximateLocationCentreChangedHookResult = null;
                }
                _this._MapForApproximateLocation = null;
            };
            this.saveState = function () {
                VRS.configStorage.save(_this.persistenceKey(), _this.createSettings());
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.load(_this.persistenceKey(), {});
                return $.extend(_this.createSettings(), savedSettings);
            };
            this.applyState = function (settings) {
                _this.setUserSuppliedCurrentLocation(settings.userSuppliedLocation);
                _this.setUseBrowserLocation(settings.useBrowserLocation);
                _this.setShowCurrentLocationOnMap(settings.showCurrentLocation);
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.createOptionPane = function (displayOrder, mapForLocationDisplay) {
                var pane = new VRS.OptionPane({
                    name: 'vrsCurrentLocation' + _this.getName(),
                    titleKey: 'PaneCurrentLocation',
                    displayOrder: displayOrder
                });
                if (!mapForLocationDisplay)
                    mapForLocationDisplay = _this.getMapForApproximateLocation();
                if (mapForLocationDisplay && VRS.globalOptions.currentLocationConfigurable) {
                    _this._MapForDisplay = mapForLocationDisplay;
                    pane.addField(new VRS.OptionFieldLabel({
                        name: 'instructions',
                        labelKey: 'CurrentLocationInstruction'
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'setCurrentLocation',
                        labelKey: 'SetCurrentLocation',
                        getValue: _this.getIsSetCurrentLocationMarkerDisplayed,
                        setValue: _this.setIsSetCurrentLocationMarkerDisplayed
                    }));
                }
                if (VRS.globalOptions.currentLocationUseGeoLocation && _this.getGeoLocationAvailable()) {
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'useBrowserLocation',
                        labelKey: 'UseBrowserLocation',
                        getValue: _this.getUseBrowserLocation,
                        setValue: _this.setUseBrowserLocation,
                        saveState: _this.saveState
                    }));
                }
                pane.addField(new VRS.OptionFieldCheckBox({
                    name: 'showCurrentLocation',
                    labelKey: 'ShowCurrentLocation',
                    getValue: _this.getShowCurrentLocationOnMap,
                    setValue: _this.setShowCurrentLocationOnMap,
                    saveState: _this.saveState,
                    keepWithNext: true
                }));
                pane.addField(new VRS.OptionFieldLabel({
                    name: 'currentLocationValue',
                    labelKey: function () {
                        var location = _this.getCurrentLocation();
                        var lat = location && location.lat !== undefined ? VRS.stringUtility.formatNumber(location.lat, 'N5') : '';
                        var lng = location && location.lng !== undefined ? VRS.stringUtility.formatNumber(location.lng, 'N5') : '';
                        return lat && lng ? ' (' + lat + ' / ' + lng + ')' : '';
                    }
                }));
                return pane;
            };
            this.determineLocationFromMap = function () {
                if (_this._MapForApproximateLocationPlugin) {
                    var centre = _this._MapForApproximateLocationPlugin.getCenter();
                    if (centre)
                        _this.setMapCentreLocation(centre);
                }
            };
            this.showOrHideSetCurrentLocationMarker = function (showMarker) {
                if (_this._MapForDisplay) {
                    var plugin = VRS.jQueryUIHelper.getMapPlugin(_this._MapForDisplay);
                    if (!showMarker) {
                        if (_this._MapMarkerDraggedHookResult)
                            plugin.unhook(_this._MapMarkerDraggedHookResult);
                        if (_this._SetCurrentLocationMarker)
                            plugin.destroyMarker(_this._SetCurrentLocationMarker);
                        _this._MapMarkerDraggedHookResult = null;
                        _this._SetCurrentLocationMarker = null;
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
                        var currentLocation = _this.getUserSuppliedCurrentLocation() || _this.getCurrentLocation();
                        if (currentLocation) {
                            markerOptions.position = currentLocation;
                        }
                        if (VRS.globalOptions.currentLocationIconUrl) {
                            markerOptions.icon = VRS.globalOptions.currentLocationIconUrl;
                        }
                        _this._SetCurrentLocationMarker = plugin.addMarker('setCurrentLocation', markerOptions);
                        _this._MapMarkerDraggedHookResult = plugin.hookMarkerDragged(_this.setCurrentLocationMarkerDragged);
                        if (currentLocation)
                            plugin.panTo(currentLocation);
                    }
                }
            };
            this.setCurrentLocationMarkerDragged = function (event, data) {
                if (_this._SetCurrentLocationMarker && data.id === _this._SetCurrentLocationMarker.id) {
                    var plugin = VRS.jQueryUIHelper.getMapPlugin(_this._MapForDisplay);
                    _this.setUserSuppliedCurrentLocation(_this._SetCurrentLocationMarker.getPosition());
                    _this.saveState();
                }
            };
            this.mapForApproximateLocationCentreChanged = function () {
                _this.determineLocationFromMap();
            };
            this.showCurrentLocationOnMap = function () {
                if (_this._MapForApproximateLocation) {
                    var plugin = _this._MapForApproximateLocationPlugin;
                    var currentLocation = _this.getCurrentLocation();
                    var showCurrentLocation = _this.getShowCurrentLocationOnMap();
                    if (!currentLocation || !showCurrentLocation) {
                        _this.destroyCurrentLocationMarker();
                    }
                    else {
                        if (_this._CurrentLocationMarker) {
                            if (_this._PlottedCurrentLocation.lat !== currentLocation.lat || _this._PlottedCurrentLocation.lng !== currentLocation.lng) {
                                _this._PlottedCurrentLocation = currentLocation;
                                _this._CurrentLocationMarker.setPosition(_this._PlottedCurrentLocation);
                            }
                        }
                        else {
                            _this._PlottedCurrentLocation = currentLocation;
                            _this._CurrentLocationMarker = plugin.addMarker('staticCurrentLocationMarker', {
                                clickable: false,
                                draggable: false,
                                flat: true,
                                optimized: false,
                                visible: true,
                                position: _this._PlottedCurrentLocation,
                                icon: new VRS.MapIcon(VRS.globalOptions.currentLocationImageUrl, VRS.globalOptions.currentLocationImageSize, null, null, VRS.globalOptions.currentLocationImageSize, null),
                                zIndex: 0
                            });
                        }
                    }
                }
            };
            this.destroyCurrentLocationMarker = function () {
                if (_this._CurrentLocationMarker) {
                    var plugin = _this._MapForApproximateLocationPlugin;
                    plugin.destroyMarker(_this._CurrentLocationMarker);
                    _this._CurrentLocationMarker = null;
                    _this._PlottedCurrentLocation = null;
                }
            };
            settings = $.extend({
                name: 'default',
                mapForApproximateLocation: null
            }, settings);
            this._Name = settings.name;
            this._GeoLocationAvailable = 'geolocation' in navigator;
            this.setMapForApproximateLocation(settings.mapForApproximateLocation);
        }
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
        return CurrentLocation;
    })();
    VRS.CurrentLocation = CurrentLocation;
    VRS.currentLocation = new VRS.CurrentLocation();
})(VRS || (VRS = {}));
