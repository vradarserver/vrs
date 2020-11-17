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
    VRS.globalOptions.mapGoogleMapHttpUrl = VRS.globalOptions.mapGoogleMapHttpUrl || 'http://maps.google.com/maps/api/js';
    VRS.globalOptions.mapGoogleMapHttpsUrl = VRS.globalOptions.mapGoogleMapHttpsUrl || 'https://maps.google.com/maps/api/js';
    VRS.globalOptions.mapGoogleMapTimeout = VRS.globalOptions.mapGoogleMapTimeout || 30000;
    VRS.globalOptions.mapGoogleMapUseHttps = VRS.globalOptions.mapGoogleMapUseHttps !== undefined ? VRS.globalOptions.mapGoogleMapUseHttps : true;
    VRS.globalOptions.mapShowStreetView = VRS.globalOptions.mapShowStreetView !== undefined ? VRS.globalOptions.mapShowStreetView : false;
    VRS.globalOptions.mapScrollWheelActive = VRS.globalOptions.mapScrollWheelActive !== undefined ? VRS.globalOptions.mapScrollWheelActive : true;
    VRS.globalOptions.mapDraggable = VRS.globalOptions.mapDraggable !== undefined ? VRS.globalOptions.mapDraggable : true;
    VRS.globalOptions.mapShowPointsOfInterest = VRS.globalOptions.mapShowPointsOfInterest !== undefined ? VRS.globalOptions.mapShowPointsOfInterest : false;
    VRS.globalOptions.mapShowScaleControl = VRS.globalOptions.mapShowScaleControl !== undefined ? VRS.globalOptions.mapShowScaleControl : true;
    VRS.globalOptions.mapShowHighContrastStyle = VRS.globalOptions.mapShowHighContrastStyle !== undefined ? VRS.globalOptions.mapShowHighContrastStyle : true;
    VRS.globalOptions.mapHighContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle !== undefined ? VRS.globalOptions.mapHighContrastMapStyle : [
        {
            featureType: 'poi',
            stylers: [
                { visibility: 'off' }
            ]
        }, {
            featureType: 'landscape',
            stylers: [
                { saturation: -70 },
                { lightness: -30 },
                { gamma: 0.80 }
            ]
        }, {
            featureType: 'road',
            stylers: [
                { visibility: 'simplified' },
                { weight: 0.4 },
                { saturation: -70 },
                { lightness: -30 },
                { gamma: 0.80 }
            ]
        }, {
            featureType: 'road',
            elementType: 'labels',
            stylers: [
                { visibility: 'simplified' },
                { saturation: -46 },
                { gamma: 1.82 }
            ]
        }, {
            featureType: 'administrative',
            stylers: [
                { weight: 1 }
            ]
        }, {
            featureType: 'administrative',
            elementType: 'labels',
            stylers: [
                { saturation: -100 },
                { weight: 0.1 },
                { lightness: -60 },
                { gamma: 2.0 }
            ]
        }, {
            featureType: 'water',
            stylers: [
                { saturation: -72 },
                { lightness: -25 }
            ]
        }, {
            featureType: 'administrative.locality',
            stylers: [
                { weight: 0.1 }
            ]
        }, {
            featureType: 'administrative.province',
            stylers: [
                { lightness: -43 }
            ]
        }, {
            "featureType": "transit.line",
            "stylers": [
                { "visibility": "off" }
            ]
        }, {
            "featureType": "transit.station.bus",
            "stylers": [
                { "visibility": "off" }
            ]
        }, {
            "featureType": "transit.station.rail",
            "stylers": [
                { "visibility": "off" }
            ]
        }
    ];
    var GoogleMapUtilities = (function () {
        function GoogleMapUtilities() {
            this._HighContrastMapTypeName = null;
        }
        GoogleMapUtilities.prototype.fromGoogleLatLng = function (latLng) {
            return latLng ? { lat: latLng.lat(), lng: latLng.lng() } : undefined;
        };
        GoogleMapUtilities.prototype.toGoogleLatLng = function (latLng) {
            return latLng ? new google.maps.LatLng(latLng.lat, latLng.lng) : undefined;
        };
        GoogleMapUtilities.prototype.fromGooglePoint = function (point) {
            return point ? { x: point.x, y: point.y } : undefined;
        };
        GoogleMapUtilities.prototype.toGooglePoint = function (point) {
            return point ? new google.maps.Point(point.x, point.y) : undefined;
        };
        GoogleMapUtilities.prototype.fromGoogleSize = function (size) {
            return size ? { width: size.width, height: size.height } : undefined;
        };
        GoogleMapUtilities.prototype.toGoogleSize = function (size) {
            return size ? new google.maps.Size(size.width, size.height) : undefined;
        };
        GoogleMapUtilities.prototype.fromGoogleLatLngBounds = function (latLngBounds) {
            if (!latLngBounds)
                return null;
            var northEast = latLngBounds.getNorthEast();
            var southWest = latLngBounds.getSouthWest();
            return {
                tlLat: northEast.lat(),
                tlLng: southWest.lng(),
                brLat: southWest.lat(),
                brLng: northEast.lng()
            };
        };
        GoogleMapUtilities.prototype.toGoogleLatLngBounds = function (bounds) {
            return bounds ? new google.maps.LatLngBounds(new google.maps.LatLng(bounds.brLat, bounds.tlLng), new google.maps.LatLng(bounds.tlLat, bounds.brLng)) : null;
        };
        GoogleMapUtilities.prototype.fromGoogleMapControlStyle = function (mapControlStyle) {
            if (!mapControlStyle)
                return null;
            switch (mapControlStyle) {
                case google.maps.MapTypeControlStyle.DEFAULT: return VRS.MapControlStyle.Default;
                case google.maps.MapTypeControlStyle.DROPDOWN_MENU: return VRS.MapControlStyle.DropdownMenu;
                case google.maps.MapTypeControlStyle.HORIZONTAL_BAR: return VRS.MapControlStyle.HorizontalBar;
                default: throw 'Not implemented';
            }
        };
        GoogleMapUtilities.prototype.toGoogleMapControlStyle = function (mapControlStyle) {
            if (!mapControlStyle)
                return null;
            switch (mapControlStyle) {
                case VRS.MapControlStyle.Default: return google.maps.MapTypeControlStyle.DEFAULT;
                case VRS.MapControlStyle.DropdownMenu: return google.maps.MapTypeControlStyle.DROPDOWN_MENU;
                case VRS.MapControlStyle.HorizontalBar: return google.maps.MapTypeControlStyle.HORIZONTAL_BAR;
                default: throw 'Not implemented';
            }
        };
        GoogleMapUtilities.prototype.fromGoogleMapType = function (mapType) {
            if (!mapType)
                return null;
            if (!this._HighContrastMapTypeName)
                this._HighContrastMapTypeName = VRS.$$.HighContrastMap;
            switch (mapType) {
                case google.maps.MapTypeId.HYBRID: return VRS.MapType.Hybrid;
                case google.maps.MapTypeId.ROADMAP: return VRS.MapType.RoadMap;
                case google.maps.MapTypeId.SATELLITE: return VRS.MapType.Satellite;
                case google.maps.MapTypeId.TERRAIN: return VRS.MapType.Terrain;
                case this._HighContrastMapTypeName: return VRS.MapType.HighContrast;
                default: throw 'Not implemented';
            }
        };
        GoogleMapUtilities.prototype.toGoogleMapType = function (mapType, suppressException) {
            if (suppressException === void 0) { suppressException = false; }
            if (!mapType)
                return null;
            if (!this._HighContrastMapTypeName)
                this._HighContrastMapTypeName = VRS.$$.HighContrastMap;
            switch (mapType) {
                case VRS.MapType.Hybrid: return google.maps.MapTypeId.HYBRID;
                case VRS.MapType.RoadMap: return google.maps.MapTypeId.ROADMAP;
                case VRS.MapType.Satellite: return google.maps.MapTypeId.SATELLITE;
                case VRS.MapType.Terrain: return google.maps.MapTypeId.TERRAIN;
                case VRS.MapType.HighContrast: return this._HighContrastMapTypeName;
                default:
                    if (suppressException)
                        return null;
                    throw 'Not implemented';
            }
        };
        GoogleMapUtilities.prototype.fromGoogleIcon = function (icon) {
            if (!icon)
                return null;
            else
                return new VRS.MapIcon(icon.url, this.fromGoogleSize(icon.size), this.fromGooglePoint(icon.anchor), this.fromGooglePoint(icon.origin), this.fromGoogleSize(icon.scaledSize));
        };
        GoogleMapUtilities.prototype.toGoogleIcon = function (icon) {
            if (!icon)
                return null;
            if (!icon.url)
                return icon;
            var result = {};
            if (icon.anchor)
                result.anchor = this.toGooglePoint(icon.anchor);
            if (icon.origin)
                result.origin = this.toGooglePoint(icon.origin);
            if (icon.scaledSize)
                result.scaledSize = this.toGoogleSize(icon.scaledSize);
            if (icon.size)
                result.size = this.toGoogleSize(icon.size);
            if (icon.url)
                result.url = icon.url;
            return result;
        };
        GoogleMapUtilities.prototype.fromGoogleLatLngMVCArray = function (latLngMVCArray) {
            if (!latLngMVCArray)
                return null;
            var result = [];
            var length = latLngMVCArray.getLength();
            for (var i = 0; i < length; ++i) {
                result.push(this.fromGoogleLatLng(latLngMVCArray.getAt(i)));
            }
            return result;
        };
        GoogleMapUtilities.prototype.toGoogleLatLngMVCArray = function (latLngArray) {
            if (!latLngArray)
                return null;
            var googleLatLngArray = [];
            var length = latLngArray.length;
            for (var i = 0; i < length; ++i) {
                googleLatLngArray.push(this.toGoogleLatLng(latLngArray[i]));
            }
            return new google.maps.MVCArray(googleLatLngArray);
        };
        GoogleMapUtilities.prototype.fromGoogleLatLngMVCArrayArray = function (latLngMVCArrayArray) {
            if (!latLngMVCArrayArray)
                return null;
            var result = [];
            var length = latLngMVCArrayArray.getLength();
            for (var i = 0; i < length; ++i) {
                result.push(this.fromGoogleLatLngMVCArray(latLngMVCArrayArray[i]));
            }
            return result;
        };
        GoogleMapUtilities.prototype.toGoogleLatLngMVCArrayArray = function (latLngArrayArray) {
            if (!latLngArrayArray)
                return null;
            var result = [];
            var length = latLngArrayArray.length;
            for (var i = 0; i < length; ++i) {
                result.push(this.toGoogleLatLngMVCArray(latLngArrayArray[i]));
            }
            return new google.maps.MVCArray(result);
        };
        GoogleMapUtilities.prototype.fromGoogleControlPosition = function (controlPosition) {
            switch (controlPosition) {
                case google.maps.ControlPosition.BOTTOM_CENTER: return VRS.MapPosition.BottomCentre;
                case google.maps.ControlPosition.BOTTOM_LEFT: return VRS.MapPosition.BottomLeft;
                case google.maps.ControlPosition.BOTTOM_RIGHT: return VRS.MapPosition.BottomRight;
                case google.maps.ControlPosition.LEFT_BOTTOM: return VRS.MapPosition.LeftBottom;
                case google.maps.ControlPosition.LEFT_CENTER: return VRS.MapPosition.LeftCentre;
                case google.maps.ControlPosition.LEFT_TOP: return VRS.MapPosition.LeftTop;
                case google.maps.ControlPosition.RIGHT_BOTTOM: return VRS.MapPosition.RightBottom;
                case google.maps.ControlPosition.RIGHT_CENTER: return VRS.MapPosition.RightCentre;
                case google.maps.ControlPosition.RIGHT_TOP: return VRS.MapPosition.RightTop;
                case google.maps.ControlPosition.TOP_CENTER: return VRS.MapPosition.TopCentre;
                case google.maps.ControlPosition.TOP_LEFT: return VRS.MapPosition.TopLeft;
                case google.maps.ControlPosition.TOP_RIGHT: return VRS.MapPosition.TopRight;
                default: throw 'Unknown control position ' + controlPosition;
            }
        };
        GoogleMapUtilities.prototype.toGoogleControlPosition = function (mapPosition) {
            switch (mapPosition) {
                case VRS.MapPosition.BottomCentre: return google.maps.ControlPosition.BOTTOM_CENTER;
                case VRS.MapPosition.BottomLeft: return google.maps.ControlPosition.BOTTOM_LEFT;
                case VRS.MapPosition.BottomRight: return google.maps.ControlPosition.BOTTOM_RIGHT;
                case VRS.MapPosition.LeftBottom: return google.maps.ControlPosition.LEFT_BOTTOM;
                case VRS.MapPosition.LeftCentre: return google.maps.ControlPosition.LEFT_CENTER;
                case VRS.MapPosition.LeftTop: return google.maps.ControlPosition.LEFT_TOP;
                case VRS.MapPosition.RightBottom: return google.maps.ControlPosition.RIGHT_BOTTOM;
                case VRS.MapPosition.RightCentre: return google.maps.ControlPosition.RIGHT_CENTER;
                case VRS.MapPosition.RightTop: return google.maps.ControlPosition.RIGHT_TOP;
                case VRS.MapPosition.TopCentre: return google.maps.ControlPosition.TOP_CENTER;
                case VRS.MapPosition.TopLeft: return google.maps.ControlPosition.TOP_LEFT;
                case VRS.MapPosition.TopRight: return google.maps.ControlPosition.TOP_RIGHT;
                default: throw 'Unknown map position ' + mapPosition;
            }
        };
        return GoogleMapUtilities;
    }());
    VRS.GoogleMapUtilities = GoogleMapUtilities;
    VRS.googleMapUtilities = new VRS.GoogleMapUtilities();
    var MapMarker = (function () {
        function MapMarker(id, nativeMarker, isMarkerWithLabel, tag) {
            this.nativeListeners = [];
            this.id = id;
            this.marker = nativeMarker;
            this.isMarkerWithLabel = isMarkerWithLabel;
            this.tag = tag;
        }
        MapMarker.prototype.getDraggable = function () {
            return this.marker.getDraggable();
        };
        MapMarker.prototype.setDraggable = function (draggable) {
            this.marker.setDraggable(draggable);
        };
        MapMarker.prototype.getIcon = function () {
            return VRS.googleMapUtilities.fromGoogleIcon(this.marker.getIcon());
        };
        MapMarker.prototype.setIcon = function (icon) {
            this.marker.setIcon(VRS.googleMapUtilities.toGoogleIcon(icon));
        };
        MapMarker.prototype.getPosition = function () {
            return VRS.googleMapUtilities.fromGoogleLatLng(this.marker.getPosition());
        };
        MapMarker.prototype.setPosition = function (position) {
            this.marker.setPosition(VRS.googleMapUtilities.toGoogleLatLng(position));
        };
        MapMarker.prototype.getTooltip = function () {
            return this.marker.getTitle();
        };
        MapMarker.prototype.setTooltip = function (tooltip) {
            this.marker.setTitle(tooltip);
        };
        MapMarker.prototype.getVisible = function () {
            return this.marker.getVisible();
        };
        MapMarker.prototype.setVisible = function (visible) {
            this.marker.setVisible(visible);
        };
        MapMarker.prototype.getZIndex = function () {
            return this.marker.getZIndex();
        };
        MapMarker.prototype.setZIndex = function (zIndex) {
            this.marker.setZIndex(zIndex);
        };
        MapMarker.prototype.getLabelVisible = function () {
            return this.isMarkerWithLabel ? this.marker.get('labelVisible') : false;
        };
        MapMarker.prototype.setLabelVisible = function (visible) {
            if (this.isMarkerWithLabel)
                this.marker.set('labelVisible', visible);
        };
        MapMarker.prototype.getLabelContent = function () {
            return this.isMarkerWithLabel ? this.marker.get('labelContent') : null;
        };
        MapMarker.prototype.setLabelContent = function (content) {
            if (this.isMarkerWithLabel)
                this.marker.set('labelContent', content);
        };
        MapMarker.prototype.getLabelAnchor = function () {
            return this.isMarkerWithLabel ? VRS.googleMapUtilities.fromGooglePoint(this.marker.get('labelAnchor')) : null;
        };
        MapMarker.prototype.setLabelAnchor = function (anchor) {
            if (this.isMarkerWithLabel)
                this.marker.set('labelAnchor', VRS.googleMapUtilities.toGooglePoint(anchor));
        };
        return MapMarker;
    }());
    var MapMarkerClusterer = (function () {
        function MapMarkerClusterer(map, nativeMarkerClusterer) {
            this.map = map;
            this.nativeMarkerClusterer = nativeMarkerClusterer;
        }
        MapMarkerClusterer.prototype.getNative = function () {
            return this.nativeMarkerClusterer;
        };
        MapMarkerClusterer.prototype.getNativeType = function () {
            return 'GoogleMaps';
        };
        MapMarkerClusterer.prototype.getMaxZoom = function () {
            return this.nativeMarkerClusterer.getMaxZoom();
        };
        MapMarkerClusterer.prototype.setMaxZoom = function (maxZoom) {
            this.nativeMarkerClusterer.setMaxZoom(maxZoom);
        };
        MapMarkerClusterer.prototype.addMarker = function (marker, noRepaint) {
            this.nativeMarkerClusterer.addMarker(marker.marker, noRepaint);
        };
        MapMarkerClusterer.prototype.addMarkers = function (markers, noRepaint) {
            this.nativeMarkerClusterer.addMarkers(this.castArrayOfMarkers(markers), noRepaint);
        };
        MapMarkerClusterer.prototype.removeMarker = function (marker, noRepaint) {
            this.nativeMarkerClusterer.removeMarker(marker.marker, noRepaint, true);
        };
        MapMarkerClusterer.prototype.removeMarkers = function (markers, noRepaint) {
            this.nativeMarkerClusterer.removeMarkers(this.castArrayOfMarkers(markers), noRepaint, true);
        };
        MapMarkerClusterer.prototype.repaint = function () {
            this.nativeMarkerClusterer.repaint();
        };
        MapMarkerClusterer.prototype.castArrayOfMarkers = function (markers) {
            var result = [];
            var length = markers ? markers.length : 0;
            for (var i = 0; i < length; ++i) {
                result.push(markers[i].marker);
            }
            return result;
        };
        return MapMarkerClusterer;
    }());
    var MapPolygon = (function () {
        function MapPolygon(id, nativePolygon, tag, options) {
            this.id = id;
            this.polygon = nativePolygon;
            this.tag = tag;
            this._Clickable = options.clickable;
            this._FillColour = options.fillColour;
            this._FillOpacity = options.fillOpacity;
            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
            this._ZIndex = options.zIndex;
        }
        MapPolygon.prototype.getDraggable = function () {
            return this.polygon.getDraggable();
        };
        MapPolygon.prototype.setDraggable = function (draggable) {
            this.polygon.setDraggable(draggable);
        };
        MapPolygon.prototype.getEditable = function () {
            return this.polygon.getEditable();
        };
        MapPolygon.prototype.setEditable = function (editable) {
            this.polygon.setEditable(editable);
        };
        MapPolygon.prototype.getVisible = function () {
            return this.polygon.getVisible();
        };
        MapPolygon.prototype.setVisible = function (visible) {
            this.polygon.setVisible(visible);
        };
        MapPolygon.prototype.getFirstPath = function () {
            return VRS.googleMapUtilities.fromGoogleLatLngMVCArray(this.polygon.getPath());
        };
        MapPolygon.prototype.setFirstPath = function (path) {
            this.polygon.setPath(VRS.googleMapUtilities.toGoogleLatLngMVCArray(path));
        };
        MapPolygon.prototype.getPaths = function () {
            return VRS.googleMapUtilities.fromGoogleLatLngMVCArrayArray(this.polygon.getPaths());
        };
        MapPolygon.prototype.setPaths = function (paths) {
            this.polygon.setPaths(VRS.googleMapUtilities.toGoogleLatLngMVCArrayArray(paths));
        };
        MapPolygon.prototype.getClickable = function () {
            return this._Clickable;
        };
        MapPolygon.prototype.setClickable = function (value) {
            if (value !== this._Clickable) {
                this._Clickable = value;
                this.polygon.setOptions({ clickable: value });
            }
        };
        MapPolygon.prototype.getFillColour = function () {
            return this._FillColour;
        };
        MapPolygon.prototype.setFillColour = function (value) {
            if (value !== this._FillColour) {
                this._FillColour = value;
                this.polygon.setOptions({ fillColor: value });
            }
        };
        MapPolygon.prototype.getFillOpacity = function () {
            return this._FillOpacity;
        };
        MapPolygon.prototype.setFillOpacity = function (value) {
            if (value !== this._FillOpacity) {
                this._FillOpacity = value;
                this.polygon.setOptions({ fillOpacity: value });
            }
        };
        MapPolygon.prototype.getStrokeColour = function () {
            return this._StrokeColour;
        };
        MapPolygon.prototype.setStrokeColour = function (value) {
            if (value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polygon.setOptions({ strokeColor: value });
            }
        };
        MapPolygon.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapPolygon.prototype.setStrokeOpacity = function (value) {
            if (value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polygon.setOptions({ strokeOpacity: value });
            }
        };
        MapPolygon.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapPolygon.prototype.setStrokeWeight = function (value) {
            if (value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polygon.setOptions({ strokeWeight: value });
            }
        };
        MapPolygon.prototype.getZIndex = function () {
            return this._ZIndex;
        };
        MapPolygon.prototype.setZIndex = function (value) {
            if (value !== this._ZIndex) {
                this._ZIndex = value;
                this.polygon.setOptions({ zIndex: value });
            }
        };
        return MapPolygon;
    }());
    var MapPolyline = (function () {
        function MapPolyline(id, nativePolyline, tag, options) {
            this.id = id;
            this.polyline = nativePolyline;
            this.tag = tag;
            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
        }
        MapPolyline.prototype.getDraggable = function () {
            return this.polyline.getDraggable();
        };
        MapPolyline.prototype.setDraggable = function (draggable) {
            this.polyline.setDraggable(draggable);
        };
        MapPolyline.prototype.getEditable = function () {
            return this.polyline.getEditable();
        };
        MapPolyline.prototype.setEditable = function (editable) {
            this.polyline.setEditable(editable);
        };
        MapPolyline.prototype.getVisible = function () {
            return this.polyline.getVisible();
        };
        MapPolyline.prototype.setVisible = function (visible) {
            this.polyline.setVisible(visible);
        };
        MapPolyline.prototype.getStrokeColour = function () {
            return this._StrokeColour;
        };
        MapPolyline.prototype.setStrokeColour = function (value) {
            if (value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polyline.setOptions({ strokeColor: value });
            }
        };
        MapPolyline.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapPolyline.prototype.setStrokeOpacity = function (value) {
            if (value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polyline.setOptions({ strokeOpacity: value });
            }
        };
        MapPolyline.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapPolyline.prototype.setStrokeWeight = function (value) {
            if (value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polyline.setOptions({ strokeWeight: value });
            }
        };
        MapPolyline.prototype.getPath = function () {
            var result = VRS.googleMapUtilities.fromGoogleLatLngMVCArray(this.polyline.getPath());
            return result || [];
        };
        MapPolyline.prototype.setPath = function (path) {
            var nativePath = VRS.googleMapUtilities.toGoogleLatLngMVCArray(path);
            this.polyline.setPath(nativePath);
        };
        MapPolyline.prototype.getFirstLatLng = function () {
            var result = null;
            var nativePath = this.polyline.getPath();
            if (nativePath.getLength())
                result = VRS.googleMapUtilities.fromGoogleLatLng(nativePath.getAt(0));
            return result;
        };
        MapPolyline.prototype.getLastLatLng = function () {
            var result = null;
            var nativePath = this.polyline.getPath();
            var length = nativePath.getLength();
            if (length)
                result = VRS.googleMapUtilities.fromGoogleLatLng(nativePath.getAt(length - 1));
            return result;
        };
        return MapPolyline;
    }());
    var MapCircle = (function () {
        function MapCircle(id, nativeCircle, tag, options) {
            this.id = id;
            this.circle = nativeCircle;
            this.tag = tag;
            this._FillOpacity = options.fillOpacity;
            this._FillColour = options.fillColor;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeColour = options.strokeColor;
            this._StrokeWeight = options.strokeWeight;
            this._ZIndex = options.zIndex;
        }
        MapCircle.prototype.getBounds = function () {
            return VRS.googleMapUtilities.fromGoogleLatLngBounds(this.circle.getBounds());
        };
        MapCircle.prototype.getCenter = function () {
            return VRS.googleMapUtilities.fromGoogleLatLng(this.circle.getCenter());
        };
        MapCircle.prototype.setCenter = function (value) {
            this.circle.setCenter(VRS.googleMapUtilities.toGoogleLatLng(value));
        };
        MapCircle.prototype.getDraggable = function () {
            return this.circle.getDraggable();
        };
        MapCircle.prototype.setDraggable = function (value) {
            this.circle.setDraggable(value);
        };
        MapCircle.prototype.getEditable = function () {
            return this.circle.getEditable();
        };
        MapCircle.prototype.setEditable = function (value) {
            this.circle.setEditable(value);
        };
        MapCircle.prototype.getRadius = function () {
            return this.circle.getRadius();
        };
        MapCircle.prototype.setRadius = function (value) {
            this.circle.setRadius(value);
        };
        MapCircle.prototype.getVisible = function () {
            return this.circle.getVisible();
        };
        MapCircle.prototype.setVisible = function (value) {
            this.circle.setVisible(value);
        };
        MapCircle.prototype.getFillColor = function () {
            return this._FillColour;
        };
        MapCircle.prototype.setFillColor = function (value) {
            if (this._FillColour !== value) {
                this._FillColour = value;
                this.circle.setOptions({ fillColor: value });
            }
        };
        MapCircle.prototype.getFillOpacity = function () {
            return this._FillOpacity;
        };
        MapCircle.prototype.setFillOpacity = function (value) {
            if (this._FillOpacity !== value) {
                this._FillOpacity = value;
                this.circle.setOptions({ fillOpacity: value });
            }
        };
        MapCircle.prototype.getStrokeColor = function () {
            return this._StrokeColour;
        };
        MapCircle.prototype.setStrokeColor = function (value) {
            if (this._StrokeColour !== value) {
                this._StrokeColour = value;
                this.circle.setOptions({ strokeColor: value });
            }
        };
        MapCircle.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapCircle.prototype.setStrokeOpacity = function (value) {
            if (this._StrokeOpacity !== value) {
                this._StrokeOpacity = value;
                this.circle.setOptions({ strokeOpacity: value });
            }
        };
        MapCircle.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapCircle.prototype.setStrokeWeight = function (value) {
            if (this._StrokeWeight !== value) {
                this._StrokeWeight = value;
                this.circle.setOptions({ strokeWeight: value });
            }
        };
        MapCircle.prototype.getZIndex = function () {
            return this._ZIndex;
        };
        MapCircle.prototype.setZIndex = function (value) {
            if (this._ZIndex !== value) {
                this._ZIndex = value;
                this.circle.setOptions({ zIndex: value });
            }
        };
        return MapCircle;
    }());
    var MapInfoWindow = (function () {
        function MapInfoWindow(id, nativeInfoWindow, tag, options) {
            this.nativeListeners = [];
            this.id = id;
            this.infoWindow = nativeInfoWindow;
            this.tag = tag;
            this.isOpen = false;
            this._DisableAutoPan = options.disableAutoPan;
            this._MaxWidth = options.maxWidth;
            this._PixelOffset = options.pixelOffset;
        }
        MapInfoWindow.prototype.getContent = function () {
            return this.infoWindow.getContent();
        };
        MapInfoWindow.prototype.setContent = function (value) {
            this.infoWindow.setContent(value);
        };
        MapInfoWindow.prototype.getDisableAutoPan = function () {
            return this._DisableAutoPan;
        };
        MapInfoWindow.prototype.setDisableAutoPan = function (value) {
            if (this._DisableAutoPan !== value) {
                this._DisableAutoPan = value;
                this.infoWindow.setOptions({ disableAutoPan: value });
            }
        };
        MapInfoWindow.prototype.getMaxWidth = function () {
            return this._MaxWidth;
        };
        MapInfoWindow.prototype.setMaxWidth = function (value) {
            if (this._MaxWidth !== value) {
                this._MaxWidth = value;
                this.infoWindow.setOptions({ maxWidth: value });
            }
        };
        MapInfoWindow.prototype.getPixelOffset = function () {
            return this._PixelOffset;
        };
        MapInfoWindow.prototype.setPixelOffset = function (value) {
            if (this._PixelOffset !== value) {
                this._PixelOffset = value;
                this.infoWindow.setOptions({ pixelOffset: VRS.googleMapUtilities.toGoogleSize(value) });
            }
        };
        MapInfoWindow.prototype.getPosition = function () {
            return VRS.googleMapUtilities.fromGoogleLatLng(this.infoWindow.getPosition());
        };
        MapInfoWindow.prototype.setPosition = function (value) {
            this.infoWindow.setPosition(VRS.googleMapUtilities.toGoogleLatLng(value));
        };
        MapInfoWindow.prototype.getZIndex = function () {
            return this.infoWindow.getZIndex();
        };
        MapInfoWindow.prototype.setZIndex = function (value) {
            this.infoWindow.setZIndex(value);
        };
        return MapInfoWindow;
    }());
    var MapPluginState = (function () {
        function MapPluginState() {
            this.map = undefined;
            this.mapContainer = undefined;
            this.markers = {};
            this.polylines = {};
            this.polygons = {};
            this.circles = {};
            this.infoWindows = {};
            this.nativeHooks = [];
        }
        return MapPluginState;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getMapPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsMap');
    };
    VRS.jQueryUIHelper.getMapOptions = function (overrides) {
        return $.extend({
            key: null,
            version: '3.42',
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
            var self = this;
            if (this.options.useServerDefaults && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if (config) {
                    this.options.center = { lat: config.InitialLatitude, lng: config.InitialLongitude };
                    this.options.mapTypeId = config.InitialMapType;
                    this.options.zoom = config.InitialZoom;
                }
            }
            this._loadGoogleMapsScript(function () {
                var state = self._getState();
                state.mapContainer = $('<div />')
                    .addClass('vrsMap')
                    .appendTo(self.element);
                if (self.options.afterCreate) {
                    self.options.afterCreate(this);
                }
                if (self.options.openOnCreate) {
                    self.open();
                }
                if (VRS.refreshManager)
                    VRS.refreshManager.registerTarget(self.element, self._targetResized, self);
            }, function (jqXHR, textStatus, errorThrown) {
                var state = self._getState();
                state.mapContainer = $('<div />')
                    .addClass('vrsMap notOnline')
                    .appendTo(self.element);
                $('<p/>')
                    .text(VRS.$$.GoogleMapsCouldNotBeLoaded + ': ' + textStatus)
                    .appendTo(state.mapContainer);
                if (self.options.afterCreate) {
                    self.options.afterCreate(this);
                }
                if (self.options.openOnCreate && self.options.afterOpen) {
                    self.options.afterOpen(self);
                }
            });
        };
        MapPlugin.prototype._destroy = function () {
            var state = this._getState();
            if (VRS.refreshManager)
                VRS.refreshManager.unregisterTarget(this.element);
            $.each(state.nativeHooks, function (idx, hookResult) {
                google.maps.event.removeListener(hookResult);
            });
            state.nativeHooks = [];
            if (state.mapContainer)
                state.mapContainer.remove();
        };
        MapPlugin.prototype._loadGoogleMapsScript = function (successCallback, failureCallback) {
            var url = VRS.globalOptions.mapGoogleMapUseHttps ? VRS.globalOptions.mapGoogleMapHttpsUrl : VRS.globalOptions.mapGoogleMapHttpUrl;
            var params = {
                v: this.options.version
            };
            var googleMapsApiKey = this.options.key;
            if (!googleMapsApiKey && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if (config && config.GoogleMapsApiKey) {
                    googleMapsApiKey = config.GoogleMapsApiKey;
                }
            }
            if (googleMapsApiKey) {
                params.key = googleMapsApiKey;
            }
            if (this.options.libraries.length > 0) {
                params.libraries = this.options.libraries.join(',');
            }
            if (VRS.browserHelper && VRS.browserHelper.notOnline()) {
                failureCallback(null, VRS.$$.WorkingInOfflineMode, VRS.$$.WorkingInOfflineMode);
            }
            else {
                var callback = successCallback;
                if (this.options.loadMarkerWithLabel) {
                    var chainCallbackMarkerWithLabel = callback;
                    callback = function () {
                        VRS.scriptManager.loadScript({
                            key: 'markerWithLabel',
                            url: 'script/markerWithLabel.js',
                            queue: true,
                            success: chainCallbackMarkerWithLabel
                        });
                    };
                }
                if (this.options.loadMarkerCluster) {
                    var chainCallbackMarkerCluster = callback;
                    callback = function () {
                        VRS.scriptManager.loadScript({
                            key: 'markerCluster',
                            url: 'script/markercluster.js',
                            queue: true,
                            success: chainCallbackMarkerCluster
                        });
                    };
                }
                if (window['google'] && window['google']['maps']) {
                    callback();
                }
                else {
                    VRS.scriptManager.loadScript({
                        key: VRS.scriptKey.GoogleMaps,
                        url: url,
                        params: params,
                        queue: true,
                        success: callback,
                        error: failureCallback || null,
                        timeout: VRS.globalOptions.mapGoogleMapTimeout
                    });
                }
            }
        };
        MapPlugin.prototype.getNative = function () {
            return this._getState().map;
        };
        MapPlugin.prototype.getNativeType = function () {
            return 'GoogleMaps';
        };
        MapPlugin.prototype.isOpen = function () {
            return !!this._getState().map;
        };
        MapPlugin.prototype.isReady = function () {
            var state = this._getState();
            return !!state.map && !!state.map.getBounds();
        };
        MapPlugin.prototype.getBounds = function () {
            return this._getBounds(this._getState());
        };
        MapPlugin.prototype._getBounds = function (state) {
            return state.map ? VRS.googleMapUtilities.fromGoogleLatLngBounds(state.map.getBounds()) : { tlLat: 0, tlLng: 0, brLat: 0, brLng: 0 };
        };
        MapPlugin.prototype.getCenter = function () {
            return this._getCenter(this._getState());
        };
        MapPlugin.prototype._getCenter = function (state) {
            return state.map ? VRS.googleMapUtilities.fromGoogleLatLng(state.map.getCenter()) : this.options.center;
        };
        MapPlugin.prototype.setCenter = function (latLng) {
            this._setCenter(this._getState(), latLng);
        };
        MapPlugin.prototype._setCenter = function (state, latLng) {
            if (state.map)
                state.map.setCenter(VRS.googleMapUtilities.toGoogleLatLng(latLng));
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
            return state.map ? VRS.googleMapUtilities.fromGoogleMapType(state.map.getMapTypeId()) : this.options.mapTypeId;
        };
        MapPlugin.prototype.setMapType = function (mapType) {
            this._setMapType(this._getState(), mapType);
        };
        MapPlugin.prototype._setMapType = function (state, mapType) {
            if (!state.map) {
                this.options.mapTypeId = mapType;
            }
            else {
                var currentMapType = this.getMapType();
                if (currentMapType !== mapType)
                    state.map.setMapTypeId(VRS.googleMapUtilities.toGoogleMapType(mapType));
            }
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
        MapPlugin.prototype.hookBrightnessChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'brightnessChanged', callback, forceThis);
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
            var self = this;
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
            var googleMapOptions = {
                zoom: mapOptions.zoom,
                center: VRS.googleMapUtilities.toGoogleLatLng(mapOptions.center),
                streetViewControl: mapOptions.streetViewControl,
                scrollwheel: mapOptions.scrollwheel,
                draggable: mapOptions.draggable,
                scaleControl: mapOptions.scaleControl,
                mapTypeControlOptions: {
                    style: VRS.googleMapUtilities.toGoogleMapControlStyle(mapOptions.controlStyle)
                }
            };
            if (mapOptions.controlPosition) {
                googleMapOptions.mapTypeControlOptions.position = VRS.googleMapUtilities.toGoogleControlPosition(mapOptions.controlPosition);
            }
            if (!mapOptions.pointsOfInterest) {
                googleMapOptions.styles = [
                    {
                        featureType: 'poi',
                        elementType: 'labels',
                        stylers: [{ visibility: 'off' }]
                    }
                ];
            }
            var highContrastMap;
            var highContrastMapName = VRS.googleMapUtilities.toGoogleMapType(VRS.MapType.HighContrast);
            if (mapOptions.showHighContrast && VRS.globalOptions.mapHighContrastMapStyle && VRS.globalOptions.mapHighContrastMapStyle.length) {
                var googleMapTypeIds = [];
                $.each(VRS.MapType, function (idx, mapType) {
                    var googleMapType = VRS.googleMapUtilities.toGoogleMapType(mapType);
                    if (googleMapType)
                        googleMapTypeIds.push(googleMapType);
                });
                googleMapOptions.mapTypeControlOptions.mapTypeIds = googleMapTypeIds;
                var highContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle;
                highContrastMap = new google.maps.StyledMapType(highContrastMapStyle, { name: highContrastMapName });
            }
            var state = this._getState();
            state.map = new google.maps.Map(state.mapContainer[0], googleMapOptions);
            if (highContrastMap) {
                state.map.mapTypes.set(highContrastMapName, highContrastMap);
            }
            else if (mapOptions.mapTypeId === VRS.MapType.HighContrast) {
                mapOptions.mapTypeId = VRS.MapType.RoadMap;
            }
            state.map.setMapTypeId(VRS.googleMapUtilities.toGoogleMapType(mapOptions.mapTypeId));
            if (mapOptions.mapControls && mapOptions.mapControls.length) {
                $.each(mapOptions.mapControls, function (idx, mapControl) {
                    self.addControl(mapControl.control, mapControl.position);
                });
            }
            this._hookEvents(state);
            var waitUntilReady = function () {
                if (self.options.waitUntilReady && !self.isReady()) {
                    setTimeout(waitUntilReady, 100);
                }
                else {
                    if (self.options.afterOpen)
                        self.options.afterOpen(self);
                }
            };
            waitUntilReady();
        };
        MapPlugin.prototype._hookEvents = function (state) {
            var self = this;
            var map = state.map;
            var hooks = state.nativeHooks;
            hooks.push(google.maps.event.addListener(map, 'bounds_changed', function () { self._raiseBoundsChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'center_changed', function () { self._raiseCenterChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'click', function (mouseEvent) { self._userNotIdle(); self._raiseClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'dblclick', function (mouseEvent) { self._userNotIdle(); self._raiseDoubleClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'idle', function () { self._onIdle(); }));
            hooks.push(google.maps.event.addListener(map, 'maptypeid_changed', function () { self._userNotIdle(); self._onMapTypeChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'rightclick', function (mouseEvent) { self._userNotIdle(); self._raiseRightClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'tilesloaded', function () { self._raiseTilesLoaded(); }));
            hooks.push(google.maps.event.addListener(map, 'zoom_changed', function () { self._userNotIdle(); self._raiseZoomChanged(); }));
        };
        MapPlugin.prototype._userNotIdle = function () {
            if (VRS.timeoutManager)
                VRS.timeoutManager.resetTimer();
        };
        MapPlugin.prototype.refreshMap = function () {
            var state = this._getState();
            if (state.map)
                google.maps.event.trigger(state.map, 'resize');
        };
        MapPlugin.prototype.panTo = function (mapCenter) {
            this._panTo(mapCenter, this._getState());
        };
        MapPlugin.prototype._panTo = function (mapCenter, state) {
            if (state.map)
                state.map.panTo(VRS.googleMapUtilities.toGoogleLatLng(mapCenter));
            else
                this.options.center = mapCenter;
        };
        MapPlugin.prototype.fitBounds = function (bounds) {
            var state = this._getState();
            if (state.map) {
                state.map.fitBounds(VRS.googleMapUtilities.toGoogleLatLngBounds(bounds));
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
            return 'vrsMapState-' + (this.options.name || 'default');
        };
        MapPlugin.prototype._createSettings = function () {
            var state = this._getState();
            var zoom = this._getZoom(state);
            var mapTypeId = this._getMapType(state);
            var center = this._getCenter(state);
            return {
                zoom: zoom,
                mapTypeId: mapTypeId,
                center: center,
                brightnessMapName: 'google',
                brightness: 100
            };
        };
        MapPlugin.prototype.addMarker = function (id, userOptions) {
            var self = this;
            var result;
            var state = this._getState();
            if (state.map) {
                var googleOptions = {
                    map: state.map,
                    position: undefined,
                    clickable: userOptions.clickable !== undefined ? userOptions.clickable : true,
                    draggable: userOptions.draggable !== undefined ? userOptions.draggable : false,
                    optimized: userOptions.optimized !== undefined ? userOptions.optimized : false,
                    raiseOnDrag: userOptions.raiseOnDrag !== undefined ? userOptions.raiseOnDrag : true,
                    visible: userOptions.visible !== undefined ? userOptions.visible : true
                };
                if (userOptions.animateAdd)
                    googleOptions.animation = google.maps.Animation.DROP;
                if (userOptions.position)
                    googleOptions.position = VRS.googleMapUtilities.toGoogleLatLng(userOptions.position);
                else
                    googleOptions.position = state.map.getCenter();
                if (userOptions.icon)
                    googleOptions.icon = VRS.googleMapUtilities.toGoogleIcon(userOptions.icon);
                if (userOptions.tooltip)
                    googleOptions.title = userOptions.tooltip;
                if (userOptions.zIndex || userOptions.zIndex === 0)
                    googleOptions.zIndex = userOptions.zIndex;
                if (userOptions.useMarkerWithLabel) {
                    if (userOptions.mwlLabelInBackground !== undefined)
                        googleOptions.labelInBackground = userOptions.mwlLabelInBackground;
                    if (userOptions.mwlLabelClass)
                        googleOptions.labelClass = userOptions.mwlLabelClass;
                }
                this.destroyMarker(id);
                var marker;
                if (!userOptions.useMarkerWithLabel)
                    marker = new google.maps.Marker(googleOptions);
                else
                    marker = new MarkerWithLabel(googleOptions);
                result = new MapMarker(id, marker, !!userOptions.useMarkerWithLabel, userOptions.tag);
                state.markers[id] = result;
                result.nativeListeners.push(google.maps.event.addListener(marker, 'click', function () { self._raiseMarkerClicked.call(self, id); }));
                result.nativeListeners.push(google.maps.event.addListener(marker, 'dragend', function () { self._raiseMarkerDragged.call(self, id); }));
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
                $.each(marker.nativeListeners, function (idx, listener) {
                    google.maps.event.removeListener(listener);
                });
                marker.nativeListeners = [];
                marker.marker.setMap(null);
                marker.marker = null;
                marker.tag = null;
                delete state.markers[marker.id];
                marker.id = null;
            }
        };
        MapPlugin.prototype.centerOnMarker = function (idOrMarker) {
            var state = this._getState();
            var marker = this.getMarker(idOrMarker);
            if (marker) {
                state.map.setCenter(marker.marker.getPosition());
            }
        };
        MapPlugin.prototype.createMapMarkerClusterer = function (settings) {
            var result = null;
            if (typeof (MarkerClusterer) == 'function') {
                var state = this._getState();
                if (state.map) {
                    settings = $.extend({}, settings);
                    var clusterer = new MarkerClusterer(state.map, [], settings);
                    result = new MapMarkerClusterer(this, clusterer);
                }
            }
            return result;
        };
        MapPlugin.prototype.addPolyline = function (id, userOptions) {
            var result;
            var state = this._getState();
            if (state.map) {
                var googleOptions = {
                    map: state.map,
                    clickable: userOptions.clickable !== undefined ? userOptions.clickable : false,
                    draggable: userOptions.draggable !== undefined ? userOptions.draggable : false,
                    editable: userOptions.editable !== undefined ? userOptions.editable : false,
                    geodesic: userOptions.geodesic !== undefined ? userOptions.geodesic : false,
                    strokeColor: userOptions.strokeColour || '#000000',
                    visible: userOptions.visible !== undefined ? userOptions.visible : true
                };
                if (userOptions.path)
                    googleOptions.path = VRS.googleMapUtilities.toGoogleLatLngMVCArray(userOptions.path);
                if (userOptions.strokeOpacity || userOptions.strokeOpacity === 0)
                    googleOptions.strokeOpacity = userOptions.strokeOpacity;
                if (userOptions.strokeWeight || userOptions.strokeWeight === 0)
                    googleOptions.strokeWeight = userOptions.strokeWeight;
                if (userOptions.zIndex || userOptions.zIndex === 0)
                    googleOptions.zIndex = userOptions.zIndex;
                this.destroyPolyline(id);
                var polyline = new google.maps.Polyline(googleOptions);
                result = new MapPolyline(id, polyline, userOptions.tag, {
                    strokeColour: userOptions.strokeColour,
                    strokeOpacity: userOptions.strokeOpacity,
                    strokeWeight: userOptions.strokeWeight
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
                polyline.polyline.setMap(null);
                polyline.polyline = null;
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
                var points = polyline.polyline.getPath();
                var length = points.getLength();
                if (length < countPoints)
                    countPoints = length;
                if (countPoints > 0) {
                    countRemoved = countPoints;
                    emptied = countPoints === length;
                    if (emptied) {
                        points.clear();
                    }
                    else {
                        var end = length - 1;
                        for (; countPoints > 0; --countPoints) {
                            points.removeAt(fromStart ? 0 : end--);
                        }
                    }
                }
            }
            return { emptied: emptied, countRemoved: countRemoved };
        };
        ;
        MapPlugin.prototype.removePolylinePointAt = function (idOrPolyline, index) {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getPath();
            points.removeAt(index);
        };
        MapPlugin.prototype.appendToPolyline = function (idOrPolyline, path, toStart) {
            var length = !path ? 0 : path.length;
            if (length > 0) {
                var polyline = this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getPath();
                var insertAt = toStart ? 0 : -1;
                for (var i = 0; i < length; ++i) {
                    var googlePoint = VRS.googleMapUtilities.toGoogleLatLng(path[i]);
                    if (toStart)
                        points.insertAt(insertAt++, googlePoint);
                    else
                        points.push(googlePoint);
                }
            }
        };
        MapPlugin.prototype.replacePolylinePointAt = function (idOrPolyline, index, point) {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getPath();
            var length = points.getLength();
            if (index === -1)
                index = length - 1;
            if (index >= 0 && index < length)
                points.setAt(index, VRS.googleMapUtilities.toGoogleLatLng(point));
        };
        MapPlugin.prototype.addPolygon = function (id, userOptions) {
            var result;
            var state = this._getState();
            if (state.map) {
                var googleOptions = {
                    map: state.map,
                    clickable: userOptions.clickable !== undefined ? userOptions.clickable : false,
                    draggable: userOptions.draggable !== undefined ? userOptions.draggable : false,
                    editable: userOptions.editable !== undefined ? userOptions.editable : false,
                    geodesic: userOptions.geodesic !== undefined ? userOptions.geodesic : false,
                    fillColor: userOptions.fillColour,
                    fillOpacity: userOptions.fillOpacity,
                    paths: (VRS.googleMapUtilities.toGoogleLatLngMVCArrayArray(userOptions.paths) || undefined),
                    strokeColor: userOptions.strokeColour || '#000000',
                    strokeWeight: userOptions.strokeWeight,
                    strokeOpacity: userOptions.strokeOpacity,
                    visible: userOptions.visible !== undefined ? userOptions.visible : true,
                    zIndex: userOptions.zIndex
                };
                this.destroyPolygon(id);
                var polygon = new google.maps.Polygon(googleOptions);
                result = new MapPolygon(id, polygon, userOptions.tag, userOptions);
                state.polygons[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getPolygon = function (idOrPolygon) {
            if (idOrPolygon instanceof MapPolygon)
                return idOrPolygon;
            var state = this._getState();
            return state.polygons[idOrPolygon];
        };
        MapPlugin.prototype.destroyPolygon = function (idOrPolygon) {
            var state = this._getState();
            var polygon = this.getPolygon(idOrPolygon);
            if (polygon) {
                polygon.polygon.setMap(null);
                polygon.polygon = null;
                polygon.tag = null;
                delete state.polygons[polygon.id];
                polygon.id = null;
            }
        };
        MapPlugin.prototype.addCircle = function (id, userOptions) {
            var result = null;
            var state = this._getState();
            if (state.map) {
                var googleOptions = $.extend({
                    clickable: false,
                    draggable: false,
                    editable: false,
                    fillColor: '#000',
                    fillOpacity: 0,
                    strokeColor: '#000',
                    strokeOpacity: 1,
                    strokeWeight: 1,
                    visible: true
                }, userOptions);
                googleOptions.center = VRS.googleMapUtilities.toGoogleLatLng(userOptions.center);
                googleOptions.map = state.map;
                this.destroyCircle(id);
                var circle = new google.maps.Circle(googleOptions);
                result = new MapCircle(id, circle, userOptions.tag, userOptions || {});
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
                circle.circle.setMap(null);
                circle.circle = null;
                circle.tag = null;
                delete state.circles[circle.id];
                circle.id = null;
            }
        };
        MapPlugin.prototype.getUnusedInfoWindowId = function () {
            var result;
            var state = this._getState();
            for (var i = 1; i > 0; ++i) {
                result = 'autoID' + i;
                if (!state.infoWindows[result])
                    break;
            }
            return result;
        };
        MapPlugin.prototype.addInfoWindow = function (id, userOptions) {
            var result = null;
            var state = this._getState();
            if (state.map) {
                var googleOptions = $.extend({}, userOptions);
                if (userOptions.position)
                    googleOptions.position = VRS.googleMapUtilities.toGoogleLatLng(userOptions.position);
                this.destroyInfoWindow(id);
                var infoWindow = new google.maps.InfoWindow(googleOptions);
                result = new MapInfoWindow(id, infoWindow, userOptions.tag, userOptions || {});
                state.infoWindows[id] = result;
                var self = this;
                result.nativeListeners.push(google.maps.event.addListener(infoWindow, 'closeclick', function () {
                    result.isOpen = false;
                    self._raiseInfoWindowClosedByUser(id);
                }));
            }
            return result;
        };
        MapPlugin.prototype.getInfoWindow = function (idOrInfoWindow) {
            if (idOrInfoWindow instanceof MapInfoWindow)
                return idOrInfoWindow;
            var state = this._getState();
            return state.infoWindows[idOrInfoWindow];
        };
        MapPlugin.prototype.destroyInfoWindow = function (idOrInfoWindow) {
            var state = this._getState();
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            if (infoWindow) {
                $.each(infoWindow.nativeListeners, function (idx, listener) {
                    google.maps.event.removeListener(listener);
                });
                this.closeInfoWindow(infoWindow);
                infoWindow.infoWindow.setContent('');
                infoWindow.tag = null;
                infoWindow.infoWindow = null;
                delete state.infoWindows[infoWindow.id];
                infoWindow.id = null;
            }
        };
        MapPlugin.prototype.openInfoWindow = function (idOrInfoWindow, mapMarker) {
            var state = this._getState();
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            if (infoWindow && state.map && !infoWindow.isOpen) {
                infoWindow.infoWindow.open(state.map, mapMarker ? mapMarker.marker : undefined);
                infoWindow.isOpen = true;
            }
        };
        MapPlugin.prototype.closeInfoWindow = function (idOrInfoWindow) {
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            if (infoWindow && infoWindow.isOpen) {
                infoWindow.infoWindow.close();
                infoWindow.isOpen = false;
            }
        };
        MapPlugin.prototype.addControl = function (element, mapPosition) {
            var state = this._getState();
            if (state.map) {
                var controlsArray = state.map.controls[VRS.googleMapUtilities.toGoogleControlPosition(mapPosition)];
                if (!(element instanceof jQuery))
                    controlsArray.push(element);
                else
                    $.each(element, function () { controlsArray.push(this); });
            }
        };
        MapPlugin.prototype.addLayer = function (layerTileSettings, opacity) {
        };
        MapPlugin.prototype.destroyLayer = function (layerName) {
        };
        MapPlugin.prototype.hasLayer = function (layerName) {
            return false;
        };
        MapPlugin.prototype.getLayerOpacity = function (layerName) {
            return undefined;
        };
        MapPlugin.prototype.setLayerOpacity = function (layerName, opacity) {
        };
        MapPlugin.prototype.getCanSetMapBrightness = function () {
            return false;
        };
        MapPlugin.prototype.getDefaultMapBrightness = function () {
            return 100;
        };
        MapPlugin.prototype.getMapBrightness = function () {
            return 100;
        };
        MapPlugin.prototype.setMapBrightness = function (value) {
        };
        MapPlugin.prototype._targetResized = function () {
            var state = this._getState();
            var center = this._getCenter(state);
            this.refreshMap();
            this._setCenter(state, center);
        };
        return MapPlugin;
    }(JQueryUICustomWidget));
    $.widget('vrs.vrsMap', new MapPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.map-google.js.map