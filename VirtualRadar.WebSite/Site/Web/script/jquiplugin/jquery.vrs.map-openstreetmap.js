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
        LeafletUtilities.prototype.fromLeafletLatLngArray = function (latLngArray) {
            return latLngArray;
        };
        LeafletUtilities.prototype.toLeafletLatLngArray = function (latLngArray) {
            latLngArray = latLngArray || [];
            var result = [];
            var len = latLngArray.length;
            for (var i = 0; i < len; ++i) {
                result.push(this.toLeafletLatLng(latLngArray[i]));
            }
            return result;
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
        LeafletUtilities.prototype.fromLeafletIcon = function (icon) {
            if (icon === null || icon === undefined) {
                return null;
            }
            return new VRS.MapIcon(icon.options.iconUrl, VRS.leafletUtilities.fromLeafletSize(icon.options.iconSize), VRS.leafletUtilities.fromLeafletPoint(icon.options.iconAnchor), null, null);
        };
        LeafletUtilities.prototype.toLeafletIcon = function (icon) {
            if (typeof icon === 'string') {
                return null;
            }
            return L.icon({
                iconUrl: icon.url,
                iconSize: VRS.leafletUtilities.toLeafletSize(icon.size),
                iconAnchor: VRS.leafletUtilities.toLeafletPoint(icon.anchor)
            });
        };
        LeafletUtilities.prototype.fromLeafletContent = function (content) {
            if (content === null || content === undefined) {
                return null;
            }
            else {
                if (typeof content === "string") {
                    return content;
                }
                return content.innerText;
            }
        };
        LeafletUtilities.prototype.fromLeafletSize = function (size) {
            if (size === null || size === undefined) {
                return null;
            }
            if (size instanceof L.Point) {
                return {
                    width: size.x,
                    height: size.y
                };
            }
            return {
                width: size[0],
                height: size[1]
            };
        };
        LeafletUtilities.prototype.toLeafletSize = function (size) {
            if (size === null || size === undefined) {
                return null;
            }
            return L.point(size.width, size.height);
        };
        LeafletUtilities.prototype.fromLeafletPoint = function (point) {
            if (point === null || point === undefined) {
                return null;
            }
            if (point instanceof L.Point) {
                return point;
            }
            return {
                x: point[0],
                y: point[1]
            };
        };
        LeafletUtilities.prototype.toLeafletPoint = function (point) {
            if (point === null || point === undefined) {
                return null;
            }
            if (point instanceof L.Point) {
                return point;
            }
            return L.point(point.x, point.y);
        };
        LeafletUtilities.prototype.fromLeafletMapPosition = function (mapPosition) {
            switch (mapPosition || '') {
                case 'topleft': return VRS.MapPosition.TopLeft;
                case 'bottomleft': return VRS.MapPosition.BottomLeft;
                case 'bottomright': return VRS.MapPosition.BottomRight;
                default: return VRS.MapPosition.TopRight;
            }
        };
        LeafletUtilities.prototype.toLeafletMapPosition = function (mapPosition) {
            switch (mapPosition || VRS.MapPosition.TopRight) {
                case VRS.MapPosition.BottomCentre:
                case VRS.MapPosition.BottomLeft:
                case VRS.MapPosition.LeftBottom:
                case VRS.MapPosition.LeftCentre:
                    return 'bottomleft';
                case VRS.MapPosition.BottomRight:
                case VRS.MapPosition.RightBottom:
                case VRS.MapPosition.RightCentre:
                    return 'bottomright';
                case VRS.MapPosition.LeftTop:
                case VRS.MapPosition.TopCentre:
                case VRS.MapPosition.TopLeft:
                    return 'topleft';
                default:
                    return 'topright';
            }
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
    var MapMarker = (function () {
        function MapMarker(id, map, nativeMarker, markerOptions, isMarkerWithLabel, isVisible, tag) {
            this.id = id;
            this.map = map;
            this.marker = nativeMarker;
            this.mapIcon = VRS.leafletUtilities.fromLeafletIcon(markerOptions.icon);
            this.zIndex = markerOptions.zIndexOffset;
            this.isMarkerWithLabel = isMarkerWithLabel;
            this.tag = tag;
            this.visible = isVisible;
        }
        MapMarker.prototype.getDraggable = function () {
            return this.marker.dragging.enabled();
        };
        MapMarker.prototype.setDraggable = function (draggable) {
            if (draggable) {
                this.marker.dragging.enable();
            }
            else {
                this.marker.dragging.disable();
            }
        };
        MapMarker.prototype.getIcon = function () {
            return this.mapIcon;
        };
        MapMarker.prototype.setIcon = function (icon) {
            this.marker.setIcon(VRS.leafletUtilities.toLeafletIcon(icon));
            this.mapIcon = icon;
        };
        MapMarker.prototype.getPosition = function () {
            return VRS.leafletUtilities.fromLeafletLatLng(this.marker.getLatLng());
        };
        MapMarker.prototype.setPosition = function (position) {
            this.marker.setLatLng(VRS.leafletUtilities.toLeafletLatLng(position));
        };
        MapMarker.prototype.getTooltip = function () {
            var tooltip = this.marker.getTooltip();
            return tooltip ? VRS.leafletUtilities.fromLeafletContent(tooltip.getContent()) : null;
        };
        MapMarker.prototype.setTooltip = function (tooltip) {
            this.marker.setTooltipContent(tooltip);
        };
        MapMarker.prototype.getVisible = function () {
            return this.visible;
        };
        MapMarker.prototype.setVisible = function (visible) {
            if (visible !== this.getVisible()) {
                if (visible) {
                    this.marker.addTo(this.map);
                }
                else {
                    this.marker.removeFrom(this.map);
                }
                this.visible = visible;
            }
        };
        MapMarker.prototype.getZIndex = function () {
            return this.zIndex;
        };
        MapMarker.prototype.setZIndex = function (zIndex) {
            this.marker.setZIndexOffset(zIndex);
            this.zIndex = zIndex;
        };
        MapMarker.prototype.getLabelVisible = function () {
            return false;
        };
        MapMarker.prototype.setLabelVisible = function (visible) {
            ;
        };
        MapMarker.prototype.getLabelContent = function () {
            return '';
        };
        MapMarker.prototype.setLabelContent = function (content) {
            ;
        };
        MapMarker.prototype.getLabelAnchor = function () {
            return null;
        };
        MapMarker.prototype.setLabelAnchor = function (anchor) {
            ;
        };
        return MapMarker;
    }());
    var MapPolyline = (function () {
        function MapPolyline(id, map, nativePolyline, tag, options) {
            this.id = id;
            this.map = map;
            this.polyline = nativePolyline;
            this.tag = tag;
            this.visible = options.visible;
            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
        }
        MapPolyline.prototype.getDraggable = function () {
            return false;
        };
        MapPolyline.prototype.setDraggable = function (draggable) {
            ;
        };
        MapPolyline.prototype.getEditable = function () {
            return false;
        };
        MapPolyline.prototype.setEditable = function (editable) {
            ;
        };
        MapPolyline.prototype.getVisible = function () {
            return this.visible;
        };
        MapPolyline.prototype.setVisible = function (visible) {
            if (this.visible !== visible) {
                if (visible) {
                    this.polyline.addTo(this.map);
                }
                else {
                    this.polyline.removeFrom(this.map);
                }
                this.visible = visible;
            }
        };
        MapPolyline.prototype.getStrokeColour = function () {
            return this._StrokeColour;
        };
        MapPolyline.prototype.setStrokeColour = function (value) {
            if (value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polyline.setStyle({
                    color: value
                });
            }
        };
        MapPolyline.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapPolyline.prototype.setStrokeOpacity = function (value) {
            if (value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polyline.setStyle({
                    opacity: value
                });
            }
        };
        MapPolyline.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapPolyline.prototype.setStrokeWeight = function (value) {
            if (value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polyline.setStyle({
                    weight: value
                });
            }
        };
        MapPolyline.prototype.getPath = function () {
            return VRS.leafletUtilities.fromLeafletLatLngArray((this.polyline.getLatLngs()));
        };
        MapPolyline.prototype.setPath = function (path) {
            this.polyline.setLatLngs(VRS.leafletUtilities.toLeafletLatLngArray(path));
        };
        MapPolyline.prototype.getFirstLatLng = function () {
            var result = null;
            var nativePath = this.polyline.getLatLngs();
            if (nativePath.length)
                result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[0]);
            return result;
        };
        MapPolyline.prototype.getLastLatLng = function () {
            var result = null;
            var nativePath = this.polyline.getLatLngs();
            if (nativePath.length)
                result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[nativePath.length - 1]);
            return result;
        };
        return MapPolyline;
    }());
    var MapCircle = (function () {
        function MapCircle(id, map, nativeCircle, tag, options) {
            this.id = id;
            this.circle = nativeCircle;
            this.map = map;
            this.tag = tag;
            this.visible = options.visible;
            this._FillOpacity = options.fillOpacity;
            this._FillColour = options.fillColor;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeColour = options.strokeColor;
            this._StrokeWeight = options.strokeWeight;
            this._ZIndex = options.zIndex;
        }
        MapCircle.prototype.getBounds = function () {
            return VRS.leafletUtilities.fromLeafletLatLngBounds(this.circle.getBounds());
        };
        MapCircle.prototype.getCenter = function () {
            return VRS.leafletUtilities.fromLeafletLatLng(this.circle.getLatLng());
        };
        MapCircle.prototype.setCenter = function (value) {
            this.circle.setLatLng(VRS.leafletUtilities.toLeafletLatLng(value));
        };
        MapCircle.prototype.getDraggable = function () {
            return false;
        };
        MapCircle.prototype.setDraggable = function (value) {
            ;
        };
        MapCircle.prototype.getEditable = function () {
            return false;
        };
        MapCircle.prototype.setEditable = function (value) {
            ;
        };
        MapCircle.prototype.getRadius = function () {
            return this.circle.getRadius();
        };
        MapCircle.prototype.setRadius = function (value) {
            this.circle.setRadius(value);
        };
        MapCircle.prototype.getVisible = function () {
            return this.visible;
        };
        MapCircle.prototype.setVisible = function (visible) {
            if (this.visible !== visible) {
                if (visible) {
                    this.circle.addTo(this.map);
                }
                else {
                    this.circle.removeFrom(this.map);
                }
                this.visible = visible;
            }
        };
        MapCircle.prototype.getFillColor = function () {
            return this._FillColour;
        };
        MapCircle.prototype.setFillColor = function (value) {
            if (this._FillColour !== value) {
                this._FillColour = value;
                this.circle.setStyle({
                    fillColor: value
                });
            }
        };
        MapCircle.prototype.getFillOpacity = function () {
            return this._FillOpacity;
        };
        MapCircle.prototype.setFillOpacity = function (value) {
            if (this._FillOpacity !== value) {
                this._FillOpacity = value;
                this.circle.setStyle({
                    fillOpacity: value
                });
            }
        };
        MapCircle.prototype.getStrokeColor = function () {
            return this._StrokeColour;
        };
        MapCircle.prototype.setStrokeColor = function (value) {
            if (this._StrokeColour !== value) {
                this._StrokeColour = value;
                this.circle.setStyle({
                    color: value
                });
            }
        };
        MapCircle.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapCircle.prototype.setStrokeOpacity = function (value) {
            if (this._StrokeOpacity !== value) {
                this._StrokeOpacity = value;
                this.circle.setStyle({
                    opacity: value
                });
            }
        };
        MapCircle.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapCircle.prototype.setStrokeWeight = function (value) {
            if (this._StrokeWeight !== value) {
                this._StrokeWeight = value;
                this.circle.setStyle({
                    weight: value
                });
            }
        };
        MapCircle.prototype.getZIndex = function () {
            return this._ZIndex;
        };
        MapCircle.prototype.setZIndex = function (value) {
            this._ZIndex = value;
        };
        return MapCircle;
    }());
    var MapControl = (function (_super) {
        __extends(MapControl, _super);
        function MapControl(element, options) {
            var _this = _super.call(this, options) || this;
            _this.element = element;
            return _this;
        }
        MapControl.prototype.onAdd = function (map) {
            var result = $('<div class="leaflet-control"></div>');
            result.append(this.element);
            return result[0];
        };
        return MapControl;
    }(L.Control));
    var MapPluginState = (function () {
        function MapPluginState() {
            this.map = undefined;
            this.mapContainer = undefined;
            this.markers = {};
            this.polylines = {};
            this.circles = {};
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
                state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(latLng));
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
                attributionControl: true,
                zoom: mapOptions.zoom,
                center: VRS.leafletUtilities.toLeafletLatLng(mapOptions.center),
                scrollWheelZoom: mapOptions.scrollwheel,
                dragging: mapOptions.draggable,
                zoomControl: mapOptions.scaleControl
            };
            var state = this._getState();
            state.map = L.map(state.mapContainer[0], leafletOptions);
            L.tileLayer(VRS.serverConfig.get().OpenStreetMapTileServerUrl, {
                attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a></a>'
            }).addTo(state.map);
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
            var result;
            var state = this._getState();
            if (state.map) {
                if (userOptions.zIndex === null || userOptions.zIndex === undefined) {
                    userOptions.zIndex = 0;
                }
                var leafletOptions = {
                    interactive: userOptions.clickable !== undefined ? userOptions.clickable : true,
                    draggable: userOptions.draggable !== undefined ? userOptions.draggable : false,
                    zIndexOffset: userOptions.zIndex,
                };
                if (userOptions.icon) {
                    leafletOptions.icon = VRS.leafletUtilities.toLeafletIcon(userOptions.icon);
                }
                if (userOptions.tooltip) {
                    leafletOptions.title = userOptions.tooltip;
                }
                var position = userOptions.position ? VRS.leafletUtilities.toLeafletLatLng(userOptions.position) : state.map.getCenter();
                this.destroyMarker(id);
                var nativeMarker = L.marker(position, leafletOptions);
                if (userOptions.visible) {
                    nativeMarker.addTo(state.map);
                }
                result = new MapMarker(id, state.map, nativeMarker, leafletOptions, !!userOptions.useMarkerWithLabel, userOptions.visible, userOptions.tag);
                state.markers[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getMarker = function (idOrMarker) {
            if (idOrMarker instanceof MapMarker)
                return idOrMarker;
            var state = this._getState();
            return state.markers[idOrMarker];
        };
        MapPlugin.prototype.destroyMarker = function (idOrMarker) {
            var state = this._getState();
            var marker = this.getMarker(idOrMarker);
            if (marker) {
                marker.setVisible(false);
                marker.marker = null;
                marker.map = null;
                marker.tag = null;
                delete state.markers[marker.id];
                marker.id = null;
            }
        };
        MapPlugin.prototype.centerOnMarker = function (idOrMarker) {
            var state = this._getState();
            var marker = this.getMarker(idOrMarker);
            if (marker) {
                this.setCenter(marker.getPosition());
            }
        };
        MapPlugin.prototype.createMapMarkerClusterer = function (settings) {
            return null;
        };
        MapPlugin.prototype.addPolyline = function (id, userOptions) {
            var result;
            var state = this._getState();
            if (state.map) {
                var options = $.extend({}, userOptions, {
                    visible: true
                });
                var leafletOptions = {
                    color: options.strokeColour || '#000000'
                };
                if (options.strokeOpacity || leafletOptions.opacity === 0)
                    leafletOptions.opacity = options.strokeOpacity;
                if (options.strokeWeight || leafletOptions.weight === 0)
                    leafletOptions.weight = options.strokeWeight;
                var path = [];
                if (options.path)
                    path = VRS.leafletUtilities.toLeafletLatLngArray(options.path);
                this.destroyPolyline(id);
                var polyline = L.polyline(path, leafletOptions);
                if (options.visible) {
                    polyline.addTo(state.map);
                }
                result = new MapPolyline(id, state.map, polyline, options.tag, {
                    strokeColour: options.strokeColour,
                    strokeOpacity: options.strokeOpacity,
                    strokeWeight: options.strokeWeight
                });
                state.polylines[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getPolyline = function (idOrPolyline) {
            if (idOrPolyline instanceof MapPolyline)
                return idOrPolyline;
            var state = this._getState();
            return state.polylines[idOrPolyline];
        };
        MapPlugin.prototype.destroyPolyline = function (idOrPolyline) {
            var state = this._getState();
            var polyline = this.getPolyline(idOrPolyline);
            if (polyline) {
                polyline.setVisible(false);
                polyline.polyline = null;
                polyline.map = null;
                polyline.tag = null;
                delete state.polylines[polyline.id];
                polyline.id = null;
            }
        };
        MapPlugin.prototype.trimPolyline = function (idOrPolyline, countPoints, fromStart) {
            var emptied = false;
            var countRemoved = 0;
            if (countPoints > 0) {
                var polyline = this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getLatLngs();
                var length = points.length;
                if (length < countPoints)
                    countPoints = length;
                if (countPoints > 0) {
                    countRemoved = countPoints;
                    emptied = countPoints === length;
                    if (emptied) {
                        points = [];
                    }
                    else {
                        if (fromStart) {
                            points.splice(0, countPoints);
                        }
                        else {
                            points.splice(length - countPoints, countPoints);
                        }
                    }
                    polyline.polyline.setLatLngs(points);
                }
            }
            return { emptied: emptied, countRemoved: countRemoved };
        };
        MapPlugin.prototype.removePolylinePointAt = function (idOrPolyline, index) {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getLatLngs();
            points.splice(index, 1);
            polyline.polyline.setLatLngs(points);
        };
        MapPlugin.prototype.appendToPolyline = function (idOrPolyline, path, toStart) {
            var length = !path ? 0 : path.length;
            if (length > 0) {
                var polyline = this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getLatLngs();
                var insertAt = toStart ? 0 : -1;
                for (var i = 0; i < length; ++i) {
                    var leafletPoint = VRS.leafletUtilities.toLeafletLatLng(path[i]);
                    if (toStart) {
                        points.splice(insertAt++, 0, leafletPoint);
                    }
                    else {
                        points.push(leafletPoint);
                    }
                }
                polyline.polyline.setLatLngs(points);
            }
        };
        MapPlugin.prototype.replacePolylinePointAt = function (idOrPolyline, index, point) {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getLatLngs();
            var length = points.length;
            if (index === -1)
                index = length - 1;
            if (index >= 0 && index < length) {
                points.splice(index, 1, VRS.leafletUtilities.toLeafletLatLng(point));
                polyline.polyline.setLatLngs(points);
            }
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
            var result = null;
            var state = this._getState();
            if (state.map) {
                var options = $.extend({}, userOptions, {
                    visible: true
                });
                var leafletOptions = {
                    fillColor: '#000',
                    fillOpacity: 0,
                    color: '#000',
                    opacity: 1,
                    weight: 1,
                    radius: options.radius || 0
                };
                var centre = VRS.leafletUtilities.toLeafletLatLng(options.center);
                this.destroyCircle(id);
                var circle = L.circle(centre, leafletOptions);
                if (options.visible) {
                    circle.addTo(state.map);
                }
                result = new MapCircle(id, state.map, circle, options.tag, options);
                state.circles[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getCircle = function (idOrCircle) {
            if (idOrCircle instanceof MapCircle)
                return idOrCircle;
            var state = this._getState();
            return state.circles[idOrCircle];
        };
        MapPlugin.prototype.destroyCircle = function (idOrCircle) {
            var state = this._getState();
            var circle = this.getCircle(idOrCircle);
            if (circle) {
                circle.setVisible(false);
                circle.circle = null;
                circle.map = null;
                circle.tag = null;
                delete state.circles[circle.id];
                circle.id = null;
            }
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
            var state = this._getState();
            if (state.map) {
                var controlOptions = {
                    position: VRS.leafletUtilities.toLeafletMapPosition(mapPosition)
                };
                var control = new MapControl(element, controlOptions);
                control.addTo(state.map);
            }
        };
        return MapPlugin;
    }(JQueryUICustomWidget));
    $.widget('vrs.vrsMap', new MapPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.map-openstreetmap.js.map