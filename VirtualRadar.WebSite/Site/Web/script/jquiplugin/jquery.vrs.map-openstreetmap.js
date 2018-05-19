var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapGoogleMapHttpUrl = VRS.globalOptions.mapGoogleMapHttpUrl || '';
    VRS.globalOptions.mapGoogleMapHttpsUrl = VRS.globalOptions.mapGoogleMapHttpsUrl || '';
    VRS.globalOptions.mapGoogleMapTimeout = VRS.globalOptions.mapGoogleMapTimeout || 30000;
    VRS.globalOptions.mapGoogleMapUseHttps = VRS.globalOptions.mapGoogleMapUseHttps !== undefined ? VRS.globalOptions.mapGoogleMapUseHttps : true;
    VRS.globalOptions.mapShowStreetView = VRS.globalOptions.mapShowStreetView !== undefined ? VRS.globalOptions.mapShowStreetView : false;
    VRS.globalOptions.mapShowHighContrastStyle = VRS.globalOptions.mapShowHighContrastStyle !== undefined ? VRS.globalOptions.mapShowHighContrastStyle : true;
    VRS.globalOptions.mapHighContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle !== undefined ? VRS.globalOptions.mapHighContrastMapStyle : [];
    VRS.globalOptions.mapScrollWheelActive = VRS.globalOptions.mapScrollWheelActive !== undefined ? VRS.globalOptions.mapScrollWheelActive : true;
    VRS.globalOptions.mapDraggable = VRS.globalOptions.mapDraggable !== undefined ? VRS.globalOptions.mapDraggable : true;
    VRS.globalOptions.mapShowPointsOfInterest = VRS.globalOptions.mapShowPointsOfInterest !== undefined ? VRS.globalOptions.mapShowPointsOfInterest : false;
    VRS.globalOptions.mapShowScaleControl = VRS.globalOptions.mapShowScaleControl !== undefined ? VRS.globalOptions.mapShowScaleControl : true;
    var LeafletUtilities = (function () {
        function LeafletUtilities() {
        }
        LeafletUtilities.prototype.fromLeafletLatLng = function (latLng) {
            return latLng;
        };
        LeafletUtilities.prototype.toLeafletLatLng = function (latLng) {
            if (latLng instanceof L.LatLng) {
                return latLng;
            }
            else if (latLng) {
                return new L.LatLng(latLng.lat, latLng.lng);
            }
            return null;
        };
        LeafletUtilities.prototype.fromLeafletLatLngBounds = function (bounds) {
            if (!bounds) {
                return null;
            }
            return {
                tlLat: bounds.getNorth(),
                tlLng: bounds.getWest(),
                brLat: bounds.getSouth(),
                brLng: bounds.getEast()
            };
        };
        LeafletUtilities.prototype.toLeaftletLatLngBounds = function (bounds) {
            if (!bounds) {
                return null;
            }
            return new L.LatLngBounds([bounds.brLat, bounds.tlLng], [bounds.tlLat, bounds.brLng]);
        };
        return LeafletUtilities;
    }());
    VRS.LeafletUtilities = LeafletUtilities;
    VRS.leafletUtilities = new VRS.LeafletUtilities();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getMapPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsMap');
    };
    VRS.jQueryUIHelper.getMapOptions = function (overrides) {
        return $.extend({
            key: null,
            version: '',
            sensor: false,
            libraries: [],
            loadMarkerWithLabel: false,
            loadMarkerCluster: false,
            openOnCreate: true,
            waitUntilReady: true,
            zoom: 12,
            center: { lat: 51.5, lng: -0.125 },
            showMapTypeControl: true,
            mapTypeId: VRS.MapType.Hybrid,
            streetViewControl: VRS.globalOptions.mapShowStreetView,
            scrollwheel: VRS.globalOptions.mapScrollWheelActive,
            scaleControl: VRS.globalOptions.mapShowScaleControl,
            draggable: VRS.globalOptions.mapDraggable,
            controlStyle: VRS.MapControlStyle.Default,
            controlPosition: undefined,
            pointsOfInterest: VRS.globalOptions.mapShowPointsOfInterest,
            showHighContrast: VRS.globalOptions.mapShowHighContrastStyle,
            mapControls: [],
            afterCreate: null,
            afterOpen: null,
            name: 'default',
            useStateOnOpen: false,
            autoSaveState: false,
            useServerDefaults: false,
            __nop: null
        }, overrides);
    };
    var MapPluginState = (function () {
        function MapPluginState() {
            this.map = undefined;
            this.mapContainer = undefined;
        }
        return MapPluginState;
    }());
    var MapPlugin = (function (_super) {
        __extends(MapPlugin, _super);
        function MapPlugin() {
            var _this = _super.call(this) || this;
            _this._EventPluginName = 'vrsMap';
            _this.options = VRS.jQueryUIHelper.getMapOptions();
            return _this;
        }
        MapPlugin.prototype._getState = function () {
            var result = this.element.data('mapPluginState');
            if (result === undefined) {
                result = new MapPluginState();
                this.element.data('mapPluginState', result);
            }
            return result;
        };
        MapPlugin.prototype._create = function () {
            if (this.options.useServerDefaults && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if (config) {
                    this.options.center = { lat: config.InitialLatitude, lng: config.InitialLongitude };
                    this.options.mapTypeId = config.InitialMapType;
                    this.options.zoom = config.InitialZoom;
                }
            }
            var state = this._getState();
            state.mapContainer = $('<div />')
                .addClass('vrsMap')
                .appendTo(this.element);
            if (this.options.afterCreate) {
                this.options.afterCreate(this);
            }
            if (this.options.openOnCreate) {
                this.open();
            }
        };
        MapPlugin.prototype._destroy = function () {
            var state = this._getState();
            if (VRS.refreshManager)
                VRS.refreshManager.unregisterTarget(this.element);
            if (state.mapContainer)
                state.mapContainer.remove();
        };
        MapPlugin.prototype.getNative = function () {
            return this._getState().map;
        };
        MapPlugin.prototype.getNativeType = function () {
            return 'OpenStreetMap';
        };
        MapPlugin.prototype.isOpen = function () {
            return !!this._getState().map;
        };
        MapPlugin.prototype.isReady = function () {
            var state = this._getState();
            return !!state.map;
        };
        MapPlugin.prototype.getBounds = function () {
            return this._getBounds(this._getState());
        };
        MapPlugin.prototype._getBounds = function (state) {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLngBounds(state.map.getBounds()) : { tlLat: 0, tlLng: 0, brLat: 0, brLng: 0 };
        };
        MapPlugin.prototype.getCenter = function () {
            return this._getCenter(this._getState());
        };
        MapPlugin.prototype._getCenter = function (state) {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLng(state.map.getCenter()) : this.options.center;
        };
        MapPlugin.prototype.setCenter = function (latLng) {
            this._setCenter(this._getState(), latLng);
        };
        MapPlugin.prototype._setCenter = function (state, latLng) {
            if (state.map)
                state.map.setView(VRS.leafletUtilities.toLeafletLatLng(latLng), state.map.getZoom());
            else
                this.options.center = latLng;
        };
        MapPlugin.prototype.getDraggable = function () {
            return this.options.draggable;
        };
        MapPlugin.prototype.getMapType = function () {
            return this._getMapType(this._getState());
        };
        MapPlugin.prototype._getMapType = function (state) {
            return this.options.mapTypeId;
        };
        MapPlugin.prototype.setMapType = function (mapType) {
            this._setMapType(this._getState(), mapType);
        };
        MapPlugin.prototype._setMapType = function (state, mapType) {
            this.options.mapTypeId = mapType;
        };
        MapPlugin.prototype.getScrollWheel = function () {
            return this.options.scrollwheel;
        };
        MapPlugin.prototype.getStreetView = function () {
            return this.options.streetViewControl;
        };
        MapPlugin.prototype.getZoom = function () {
            return this._getZoom(this._getState());
        };
        MapPlugin.prototype._getZoom = function (state) {
            return state.map ? state.map.getZoom() : this.options.zoom;
        };
        MapPlugin.prototype.setZoom = function (zoom) {
            this._setZoom(this._getState(), zoom);
        };
        MapPlugin.prototype._setZoom = function (state, zoom) {
            if (state.map)
                state.map.setZoom(zoom);
            else
                this.options.zoom = zoom;
        };
        MapPlugin.prototype.unhook = function (hookResult) {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        };
        MapPlugin.prototype.hookBoundsChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'boundsChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseBoundsChanged = function () {
            this._trigger('boundsChanged');
        };
        MapPlugin.prototype.hookCenterChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'centerChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseCenterChanged = function () {
            this._trigger('centerChanged');
        };
        MapPlugin.prototype.hookClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'clicked', callback, forceThis);
        };
        MapPlugin.prototype._raiseClicked = function (mouseEvent) {
            this._trigger('clicked', null, { mouseEvent: mouseEvent });
        };
        MapPlugin.prototype.hookDoubleClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'doubleClicked', callback, forceThis);
        };
        MapPlugin.prototype._raiseDoubleClicked = function (mouseEvent) {
            this._trigger('doubleClicked', null, { mouseEvent: mouseEvent });
        };
        MapPlugin.prototype.hookIdle = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'idle', callback, forceThis);
        };
        MapPlugin.prototype._raiseIdle = function () {
            this._trigger('idle');
        };
        MapPlugin.prototype.hookMapTypeChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mapTypeChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseMapTypeChanged = function () {
            this._trigger('mapTypeChanged');
        };
        MapPlugin.prototype.hookRightClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mouseEvent', callback, forceThis);
        };
        MapPlugin.prototype._raiseRightClicked = function (mouseEvent) {
            this._trigger('mouseEvent', null, { mouseEvent: mouseEvent });
        };
        MapPlugin.prototype.hookTilesLoaded = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'tilesLoaded', callback, forceThis);
        };
        MapPlugin.prototype._raiseTilesLoaded = function () {
            this._trigger('tilesLoaded');
        };
        MapPlugin.prototype.hookZoomChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'zoomChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseZoomChanged = function () {
            this._trigger('zoomChanged');
        };
        MapPlugin.prototype.hookMarkerClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerClicked', callback, forceThis);
        };
        MapPlugin.prototype._raiseMarkerClicked = function (id) {
            this._trigger('markerClicked', null, { id: id });
        };
        MapPlugin.prototype.hookMarkerDragged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerDragged', callback, forceThis);
        };
        MapPlugin.prototype._raiseMarkerDragged = function (id) {
            this._trigger('markerDragged', null, { id: id });
        };
        MapPlugin.prototype.hookInfoWindowClosedByUser = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'infoWindowClosedByUser', callback, forceThis);
        };
        MapPlugin.prototype._raiseInfoWindowClosedByUser = function (id) {
            this._trigger('infoWindowClosedByUser', null, { id: id });
        };
        MapPlugin.prototype._onIdle = function () {
            if (this.options.autoSaveState)
                this.saveState();
            this._raiseIdle();
        };
        MapPlugin.prototype._onMapTypeChanged = function () {
            if (this.options.autoSaveState)
                this.saveState();
            this._raiseMapTypeChanged();
        };
        MapPlugin.prototype.open = function (userOptions) {
            var _this = this;
            var mapOptions = $.extend({}, userOptions, {
                zoom: this.options.zoom,
                center: this.options.center,
                mapTypeControl: this.options.showMapTypeControl,
                mapTypeId: this.options.mapTypeId,
                streetViewControl: this.options.streetViewControl,
                scrollwheel: this.options.scrollwheel,
                scaleControl: this.options.scaleControl,
                draggable: this.options.draggable,
                showHighContrast: this.options.showHighContrast,
                controlStyle: this.options.controlStyle,
                controlPosition: this.options.controlPosition,
                mapControls: this.options.mapControls
            });
            if (this.options.useStateOnOpen) {
                var settings = this.loadState();
                mapOptions.zoom = settings.zoom;
                mapOptions.center = settings.center;
                mapOptions.mapTypeId = settings.mapTypeId;
            }
            var leafletOptions = {
                zoom: mapOptions.zoom,
                center: VRS.leafletUtilities.toLeafletLatLng(mapOptions.center),
                scrollWheelZoom: mapOptions.scrollwheel,
                dragging: mapOptions.draggable,
                zoomControl: mapOptions.scaleControl
            };
            var state = this._getState();
            state.map = L.map(state.mapContainer[0], leafletOptions);
            L.tileLayer(VRS.serverConfig.get().OpenStreetMapTileServerUrl).addTo(state.map);
            var waitUntilReady = function () {
                if (_this.options.waitUntilReady && !_this.isReady()) {
                    setTimeout(waitUntilReady, 100);
                }
                else {
                    if (_this.options.afterOpen)
                        _this.options.afterOpen(_this);
                }
            };
            waitUntilReady();
        };
        MapPlugin.prototype._userNotIdle = function () {
            if (VRS.timeoutManager)
                VRS.timeoutManager.resetTimer();
        };
        MapPlugin.prototype.refreshMap = function () {
            var state = this._getState();
            if (state.map) {
                state.map.invalidateSize();
            }
        };
        MapPlugin.prototype.panTo = function (mapCenter) {
            this._panTo(mapCenter, this._getState());
        };
        MapPlugin.prototype._panTo = function (mapCenter, state) {
            if (state.map)
                state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(mapCenter));
            else
                this.options.center = mapCenter;
        };
        MapPlugin.prototype.fitBounds = function (bounds) {
            var state = this._getState();
            if (state.map) {
                state.map.fitBounds(VRS.leafletUtilities.toLeaftletLatLngBounds(bounds));
            }
        };
        MapPlugin.prototype.saveState = function () {
            var settings = this._createSettings();
            VRS.configStorage.save(this._persistenceKey(), settings);
        };
        MapPlugin.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            return $.extend(this._createSettings(), savedSettings);
        };
        MapPlugin.prototype.applyState = function (config) {
            config = config || {};
            var state = this._getState();
            if (config.center)
                this._setCenter(state, config.center);
            if (config.zoom || config.zoom === 0)
                this._setZoom(state, config.zoom);
            if (config.mapTypeId)
                this._setMapType(state, config.mapTypeId);
        };
        ;
        MapPlugin.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        MapPlugin.prototype._persistenceKey = function () {
            return 'vrsOpenStreetMapState-' + (this.options.name || 'default');
        };
        MapPlugin.prototype._createSettings = function () {
            var state = this._getState();
            var zoom = this._getZoom(state);
            var mapTypeId = this._getMapType(state);
            var center = this._getCenter(state);
            return {
                zoom: zoom,
                mapTypeId: mapTypeId,
                center: center
            };
        };
        MapPlugin.prototype.addMarker = function (id, userOptions) {
            return null;
        };
        MapPlugin.prototype.getMarker = function (idOrMarker) {
            return null;
        };
        MapPlugin.prototype.destroyMarker = function (idOrMarker) {
            ;
        };
        MapPlugin.prototype.centerOnMarker = function (idOrMarker) {
            ;
        };
        MapPlugin.prototype.createMapMarkerClusterer = function (settings) {
            return null;
        };
        MapPlugin.prototype.addPolyline = function (id, userOptions) {
            return null;
        };
        MapPlugin.prototype.getPolyline = function (idOrPolyline) {
            return null;
        };
        MapPlugin.prototype.destroyPolyline = function (idOrPolyline) {
            ;
        };
        MapPlugin.prototype.trimPolyline = function (idOrPolyline, countPoints, fromStart) {
            return null;
        };
        MapPlugin.prototype.removePolylinePointAt = function (idOrPolyline, index) {
            ;
        };
        MapPlugin.prototype.appendToPolyline = function (idOrPolyline, path, toStart) {
            ;
        };
        MapPlugin.prototype.replacePolylinePointAt = function (idOrPolyline, index, point) {
            ;
        };
        MapPlugin.prototype.addPolygon = function (id, userOptions) {
            return null;
        };
        MapPlugin.prototype.getPolygon = function (idOrPolygon) {
            return null;
        };
        MapPlugin.prototype.destroyPolygon = function (idOrPolygon) {
            ;
        };
        MapPlugin.prototype.addCircle = function (id, userOptions) {
            return null;
        };
        MapPlugin.prototype.getCircle = function (idOrCircle) {
            return null;
        };
        MapPlugin.prototype.destroyCircle = function (idOrCircle) {
            ;
        };
        MapPlugin.prototype.getUnusedInfoWindowId = function () {
            return null;
        };
        MapPlugin.prototype.addInfoWindow = function (id, userOptions) {
            return null;
        };
        MapPlugin.prototype.getInfoWindow = function (idOrInfoWindow) {
            return null;
        };
        MapPlugin.prototype.destroyInfoWindow = function (idOrInfoWindow) {
            ;
        };
        MapPlugin.prototype.openInfoWindow = function (idOrInfoWindow, mapMarker) {
            ;
        };
        MapPlugin.prototype.closeInfoWindow = function (idOrInfoWindow) {
            ;
        };
        MapPlugin.prototype.addControl = function (element, mapPosition) {
            ;
        };
        return MapPlugin;
    }(JQueryUICustomWidget));
    $.widget('vrs.vrsMap', new MapPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.map-openstreetmap.js.map