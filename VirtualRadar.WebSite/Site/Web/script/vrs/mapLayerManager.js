var VRS;
(function (VRS) {
    var MapLayerSetting = (function () {
        function MapLayerSetting(map, tileServerSettings) {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.MapLayerSetting'
            });
            this._Events = {
                visibilityChanged: 'visibilityChanged',
                opacityOverrideChanged: 'opacityOverrideChanged'
            };
            this._Map = map;
            this._TileServerSettings = tileServerSettings;
        }
        Object.defineProperty(MapLayerSetting.prototype, "IsVisible", {
            get: function () { return this._Map && this._Map.hasLayer(this.Name); },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MapLayerSetting.prototype, "DisplayOrder", {
            get: function () { return this.IsVisible ? this._DisplayOrder : -1; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MapLayerSetting.prototype, "TileServerSettings", {
            get: function () { return this._TileServerSettings; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MapLayerSetting.prototype, "Name", {
            get: function () { return this._TileServerSettings.Name; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MapLayerSetting.prototype, "Opacity", {
            get: function () { return this._Opacity === undefined ? this._TileServerSettings.DefaultOpacity : this._Opacity; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MapLayerSetting.prototype, "OpacityDefault", {
            get: function () { return this._TileServerSettings.DefaultOpacity; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MapLayerSetting.prototype, "OpacityOverride", {
            get: function () { return this._Opacity; },
            set: function (value) {
                var newOpacity = isNaN(value) || value === null || value === undefined || value === this._TileServerSettings.DefaultOpacity
                    ? undefined
                    : Math.max(0, Math.min(100, value));
                if (newOpacity !== this._Opacity) {
                    this._Opacity = newOpacity;
                    this.raiseOpacityOverrideChanged();
                }
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(MapLayerSetting.prototype, "IsSuppressed", {
            get: function () { return this._IsSuppressed; },
            set: function (value) {
                if (this._IsSuppressed != !!value) {
                    this._IsSuppressed = !!value;
                    if (this.IsSuppressed && this.IsVisible) {
                        this.hide();
                    }
                }
            },
            enumerable: true,
            configurable: true
        });
        MapLayerSetting.prototype.hookVisibilityChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.visibilityChanged, callback, forceThis);
        };
        MapLayerSetting.prototype.raiseVisibilityChanged = function () {
            this._Dispatcher.raise(this._Events.visibilityChanged, [this]);
        };
        MapLayerSetting.prototype.hookOpacityOverrideChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.opacityOverrideChanged, callback, forceThis);
        };
        MapLayerSetting.prototype.raiseOpacityOverrideChanged = function () {
            this._Dispatcher.raise(this._Events.opacityOverrideChanged, [this]);
        };
        MapLayerSetting.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        MapLayerSetting.prototype.show = function () {
            if (!this.IsVisible && this._Map && !this.IsSuppressed) {
                this._DisplayOrder = new Date().getTime();
                this._Map.addLayer(this.TileServerSettings, this.OpacityOverride);
                this.raiseVisibilityChanged();
            }
        };
        MapLayerSetting.prototype.hide = function () {
            if (this.IsVisible && this._Map) {
                this._Map.destroyLayer(this.Name);
                this.raiseVisibilityChanged();
            }
        };
        MapLayerSetting.prototype.toggleVisible = function () {
            if (this.IsVisible) {
                this.hide();
            }
            else {
                this.show();
            }
        };
        MapLayerSetting.prototype.getMapOpacity = function () {
            return this.IsVisible ? this._Map.getLayerOpacity(this.Name) : this.Opacity;
        };
        MapLayerSetting.prototype.setMapOpacity = function (value) {
            this.OpacityOverride = value;
            if (this.IsVisible) {
                this._Map.setLayerOpacity(this.Name, value);
            }
        };
        return MapLayerSetting;
    }());
    VRS.MapLayerSetting = MapLayerSetting;
    var HookHandles = (function () {
        function HookHandles() {
        }
        return HookHandles;
    }());
    var MapLayerManager = (function () {
        function MapLayerManager() {
            this._MapLayerSettings = [];
            this._HookHandles = {};
            this._PersistenceKey = 'vrsMapLayerManager';
            this._ApplyingState = false;
            this._CustomMapLayerSettings = [];
            this._SuppressedMapLayers = [];
        }
        MapLayerManager.prototype.registerMap = function (map) {
            this._Map = map;
            this.buildMapLayerSettings();
            this.loadAndApplyState();
            this._ConfigurationChangedHook = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.configurationChanged, this);
        };
        MapLayerManager.prototype.registerCustomMapLayerSetting = function (layerTileServerSettings) {
            if (layerTileServerSettings && layerTileServerSettings.IsLayer && layerTileServerSettings.Name) {
                if (!VRS.arrayHelper.findFirst(this._MapLayerSettings, function (r) { return r.Name === layerTileServerSettings.Name; }) &&
                    !VRS.arrayHelper.findFirst(this._CustomMapLayerSettings, function (r) { return r.Name === layerTileServerSettings.Name; })) {
                    this._CustomMapLayerSettings.push(layerTileServerSettings);
                    this.buildMapLayerSettings();
                    this.loadAndApplyState();
                }
            }
        };
        MapLayerManager.prototype.suppressStandardLayer = function (layerName) {
            if (layerName) {
                if (VRS.arrayHelper.indexOf(this._SuppressedMapLayers, layerName) === -1) {
                    this._SuppressedMapLayers.push(layerName);
                    var layerTileServerSettings = VRS.arrayHelper.findFirst(this._MapLayerSettings, function (r) { return r.Name == layerName; });
                    if (layerTileServerSettings) {
                        layerTileServerSettings.IsSuppressed = true;
                    }
                }
            }
        };
        MapLayerManager.prototype.getMapLayerSettings = function () {
            var result = [];
            $.each(this._MapLayerSettings, function (idx, mapLayerSetting) {
                result.push(mapLayerSetting);
            });
            return result;
        };
        MapLayerManager.prototype.saveState = function () {
            if (!this._ApplyingState && this._Map) {
                VRS.configStorage.save(this._PersistenceKey, this.createSettings());
            }
        };
        MapLayerManager.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._PersistenceKey, {});
            return $.extend(this.createSettings(), savedSettings);
        };
        MapLayerManager.prototype.applyState = function (settings) {
            var _this = this;
            if (!this._ApplyingState && this._Map) {
                try {
                    this._ApplyingState = true;
                    $.each(this._MapLayerSettings, function (idx, mapLayerSetting) {
                        var opacityOverride = settings.opacityOverrides[mapLayerSetting.Name];
                        if (opacityOverride !== null && opacityOverride !== undefined && !isNaN(opacityOverride) && mapLayerSetting.OpacityOverride !== opacityOverride) {
                            mapLayerSetting.OpacityOverride = opacityOverride;
                        }
                    });
                    $.each(this._SuppressedMapLayers, function (idx, suppressedMapLayerName) {
                        var mapLayer = VRS.arrayHelper.findFirst(_this._MapLayerSettings, function (r) { return r.Name == suppressedMapLayerName; });
                        if (mapLayer && !mapLayer.IsSuppressed) {
                            mapLayer.IsSuppressed = true;
                        }
                    });
                    $.each(settings.visibleLayouts, function (idx, visibleLayoutName) {
                        var mapLayerSetting = VRS.arrayHelper.findFirst(_this._MapLayerSettings, function (r) { return r.Name === visibleLayoutName; });
                        if (mapLayerSetting && !mapLayerSetting.IsVisible) {
                            mapLayerSetting.show();
                        }
                    });
                }
                finally {
                    this._ApplyingState = false;
                }
            }
        };
        MapLayerManager.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        MapLayerManager.prototype.createSettings = function () {
            var result = {
                visibleLayouts: [],
                opacityOverrides: {}
            };
            this._MapLayerSettings.sort(function (lhs, rhs) { return lhs.DisplayOrder - rhs.DisplayOrder; });
            $.each(this._MapLayerSettings, function (idx, mapLayerSetting) {
                if (mapLayerSetting.IsVisible) {
                    result.visibleLayouts.push(mapLayerSetting.Name);
                }
                if (mapLayerSetting.OpacityOverride !== undefined) {
                    result.opacityOverrides[mapLayerSetting.Name] = mapLayerSetting.OpacityOverride;
                }
            });
            return result;
        };
        MapLayerManager.prototype.buildMapLayerSettings = function () {
            var _this = this;
            if (this._Map) {
                var newMapLayerSettings = [];
                var tileServerSettings = VRS.serverConfig.get().TileServerLayers;
                $.each(this._CustomMapLayerSettings, function (idx, customTileServerSettings) {
                    tileServerSettings.push(customTileServerSettings);
                });
                var len = tileServerSettings.length;
                for (var i = 0; i < len; ++i) {
                    var tileServerSetting = tileServerSettings[i];
                    var mapLayerSetting = VRS.arrayHelper.findFirst(this._MapLayerSettings, function (r) { return r.Name == tileServerSetting.Name; });
                    if (!mapLayerSetting) {
                        mapLayerSetting = new MapLayerSetting(this._Map, tileServerSetting);
                        var hookHandles = {
                            opacityOverrideChangedEventHandle: mapLayerSetting.hookOpacityOverrideChanged(this.mapLayer_opacityChanged, this),
                            visibilityChangedEventHandle: mapLayerSetting.hookVisibilityChanged(this.mapLayer_visibilityChanged, this)
                        };
                        this._HookHandles[mapLayerSetting.Name] = hookHandles;
                    }
                    newMapLayerSettings.push(mapLayerSetting);
                }
                $.each(VRS.arrayHelper.except(this._MapLayerSettings, newMapLayerSettings, function (lhs, rhs) { return lhs.Name === rhs.Name; }), function (idx, retiredMapLayer) {
                    var hookHandles = _this._HookHandles[retiredMapLayer.Name];
                    if (hookHandles) {
                        retiredMapLayer.unhook(hookHandles.opacityOverrideChangedEventHandle);
                        retiredMapLayer.unhook(hookHandles.visibilityChangedEventHandle);
                        delete _this._HookHandles[retiredMapLayer.Name];
                    }
                    retiredMapLayer.hide();
                });
                this._MapLayerSettings = newMapLayerSettings;
            }
        };
        MapLayerManager.prototype.configurationChanged = function () {
            this.buildMapLayerSettings();
            this.loadAndApplyState();
        };
        MapLayerManager.prototype.mapLayer_opacityChanged = function (mapLayer) {
            this.saveState();
        };
        MapLayerManager.prototype.mapLayer_visibilityChanged = function (mapLayer) {
            this.saveState();
        };
        return MapLayerManager;
    }());
    VRS.MapLayerManager = MapLayerManager;
    VRS.mapLayerManager = new MapLayerManager();
})(VRS || (VRS = {}));
//# sourceMappingURL=mapLayerManager.js.map