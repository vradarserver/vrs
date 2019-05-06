var VRS;
(function (VRS) {
    var MapLayerSetting = (function () {
        function MapLayerSetting(tileServerSettings) {
            this._TileServerSettings = tileServerSettings;
        }
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
                this._Opacity = isNaN(value) || value === null || value === undefined || value === this._TileServerSettings.DefaultOpacity
                    ? undefined
                    : Math.max(0, Math.min(100, value));
            },
            enumerable: true,
            configurable: true
        });
        return MapLayerSetting;
    }());
    VRS.MapLayerSetting = MapLayerSetting;
    var MapLayerConfiguration = (function () {
        function MapLayerConfiguration() {
        }
        MapLayerConfiguration.prototype.getMapLayerSettings = function () {
            var result = [];
            var tileServerSettings = VRS.serverConfig.get().TileServerLayers;
            var len = tileServerSettings.length;
            for (var i = 0; i < len; ++i) {
                result.push(this.buildMapLayerSetting(tileServerSettings[i]));
            }
            return result;
        };
        MapLayerConfiguration.prototype.getMapLayerSetting = function (layerName) {
            return this.buildMapLayerSetting(VRS.arrayHelper.findFirst(VRS.serverConfig.get().TileServerLayers, function (r) { return r.Name === layerName; }));
        };
        MapLayerConfiguration.prototype.buildMapLayerSetting = function (tileServerSetting) {
            return tileServerSetting ? new MapLayerSetting(tileServerSetting) : null;
        };
        return MapLayerConfiguration;
    }());
    VRS.MapLayerConfiguration = MapLayerConfiguration;
    VRS.mapLayerConfiguration = new MapLayerConfiguration();
})(VRS || (VRS = {}));
//# sourceMappingURL=mapLayerConfiguration.js.map