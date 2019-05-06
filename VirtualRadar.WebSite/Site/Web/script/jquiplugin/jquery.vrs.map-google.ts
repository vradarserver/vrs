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
 * @fileoverview A jQuery UI plugin that wraps Google Maps.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapGoogleMapHttpUrl = VRS.globalOptions.mapGoogleMapHttpUrl || 'http://maps.google.com/maps/api/js';            // The HTTP URL for Google Maps
    VRS.globalOptions.mapGoogleMapHttpsUrl = VRS.globalOptions.mapGoogleMapHttpsUrl || 'https://maps.google.com/maps/api/js';         // The HTTPS URL for Google Maps
    VRS.globalOptions.mapGoogleMapTimeout = VRS.globalOptions.mapGoogleMapTimeout || 30000;                                           // The number of milliseconds to wait before giving up and assuming that the maps aren't going to load.
    VRS.globalOptions.mapGoogleMapUseHttps = VRS.globalOptions.mapGoogleMapUseHttps !== undefined ? VRS.globalOptions.mapGoogleMapUseHttps : true;  // True to load the HTTPS version, false to load the HTTP. Note that Chrome on iOS fails if it's not HTTPS!
    VRS.globalOptions.mapShowStreetView = VRS.globalOptions.mapShowStreetView !== undefined ? VRS.globalOptions.mapShowStreetView : false;              // True if the StreetView control is to be shown on Google Maps.
    VRS.globalOptions.mapScrollWheelActive = VRS.globalOptions.mapScrollWheelActive !== undefined ? VRS.globalOptions.mapScrollWheelActive : true;      // True if the scroll wheel zooms the map.
    VRS.globalOptions.mapDraggable = VRS.globalOptions.mapDraggable !== undefined ? VRS.globalOptions.mapDraggable : true;                              // True if the user can move the map.
    VRS.globalOptions.mapShowPointsOfInterest = VRS.globalOptions.mapShowPointsOfInterest !== undefined ? VRS.globalOptions.mapShowPointsOfInterest : false;    // True if points of interest are to be shown on Google Maps.
    VRS.globalOptions.mapShowScaleControl = VRS.globalOptions.mapShowScaleControl !== undefined ? VRS.globalOptions.mapShowScaleControl : true;         // True if the map should display a scale on it.
    VRS.globalOptions.mapShowHighContrastStyle = VRS.globalOptions.mapShowHighContrastStyle !== undefined ? VRS.globalOptions.mapShowHighContrastStyle : true;  // True if the high-contrast map style is to be shown.
    VRS.globalOptions.mapHighContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle !== undefined ? VRS.globalOptions.mapHighContrastMapStyle : <google.maps.MapTypeStyle[]>[ // The Google map styles to use for the high contrast map.
        {
            featureType: 'poi',
            stylers: [
                { visibility: 'off' }
            ]
        },{
            featureType: 'landscape',
            stylers: [
                { saturation: -70 },
                { lightness: -30 },
                { gamma: 0.80 }
            ]
        },{
            featureType: 'road',
            stylers: [
                { visibility: 'simplified' },
                { weight: 0.4 },
                { saturation: -70 },
                { lightness: -30 },
                { gamma: 0.80 }
            ]
        },{
            featureType: 'road',
            elementType: 'labels',
            stylers: [
                { visibility: 'simplified' },
                { saturation: -46 },
                { gamma: 1.82 }
            ]
        },{
            featureType: 'administrative',
            stylers: [
                { weight: 1 }
            ]
        },{
            featureType: 'administrative',
            elementType: 'labels',
            stylers: [
                { saturation: -100 },
                { weight: 0.1 },
                { lightness: -60 },
                { gamma: 2.0 }
            ]
        },{
            featureType: 'water',
            stylers: [
                { saturation: -72 },
                { lightness: -25 }
            ]
        },{
            featureType: 'administrative.locality',
            stylers: [
                { weight: 0.1 }
            ]
        },{
            featureType: 'administrative.province',
            stylers: [
                { lightness: -43 }
            ]
        },{
            "featureType": "transit.line",
            "stylers": [
                { "visibility": "off" }
            ]
        },{
            "featureType": "transit.station.bus",
            "stylers": [
                { "visibility": "off" }
            ]
        },{
            "featureType": "transit.station.rail",
            "stylers": [
                { "visibility": "off" }
            ]
        }
    ];


    /**
     * An object that can convert to and from VRS map objects and Google map object.
     */
    export class GoogleMapUtilities
    {
        /**
         * The ID *and* the label of the high contrast map type. We cannot refer to the VRS.$$ identifier directly when
         * translating map types because we need to use the one that Google knows about, not the one for the currently
         * selected language. The first time the map type gets translated this gets filled in with VRS.$$.HighContrastMap.
         */
        private _HighContrastMapTypeName: string = null;

        /**
         * Converts from a Google latLng object to a VRS latLng object.
         */
        fromGoogleLatLng(latLng: google.maps.LatLng) : ILatLng
        {
            return latLng ? { lat: latLng.lat(), lng: latLng.lng() } : undefined;
        }

        /**
         * Converts from a VRS latLng to a Google latLng.
         */
        toGoogleLatLng(latLng: ILatLng) : google.maps.LatLng
        {
            return latLng ? new google.maps.LatLng(latLng.lat, latLng.lng) : undefined;
        }

        /**
         * Converts from a Google point to a VRS point.
         */
        fromGooglePoint(point: google.maps.Point) : IPoint
        {
            return point ? { x: point.x, y: point.y } : undefined;
        }

        /**
         * Converts from a VRS point to a Google point
         */
        toGooglePoint(point: IPoint) : google.maps.Point
        {
            return point ? new google.maps.Point(point.x, point.y) : undefined;
        }

        /**
         * Converts from a Google Size to a VRS size.
         */
        fromGoogleSize(size: google.maps.Size) : ISize
        {
            return size ? { width: size.width, height: size.height } : undefined;
        }

        /**
         * Converts from a VRS size to a Google size.
         */
        toGoogleSize(size: ISize) : google.maps.Size
        {
            return size ? new google.maps.Size(size.width, size.height) : undefined;
        }

        /**
         * Converts from a Google latLngBounds to a VRS bounds.
         */
        fromGoogleLatLngBounds(latLngBounds: google.maps.LatLngBounds) : IBounds
        {
            if(!latLngBounds) return null;
            var northEast = latLngBounds.getNorthEast();
            var southWest = latLngBounds.getSouthWest();
            return {
                tlLat: northEast.lat(),
                tlLng: southWest.lng(),
                brLat: southWest.lat(),
                brLng: northEast.lng()
            };
        }

        /**
         * Converts from a VRS bounds to a Google LatLngBounds.
         */
        toGoogleLatLngBounds(bounds: IBounds) : google.maps.LatLngBounds
        {
            return bounds ? new google.maps.LatLngBounds(
                new google.maps.LatLng(bounds.brLat, bounds.tlLng),
                new google.maps.LatLng(bounds.tlLat, bounds.brLng)
            ) : null;
        }

        /**
         * Converts from a Google map control style to a VRS one.
         */
        fromGoogleMapControlStyle(mapControlStyle: google.maps.MapTypeControlStyle) : VRS.MapControlStyleEnum
        {
            if(!mapControlStyle) return null;
            switch(mapControlStyle) {
                case google.maps.MapTypeControlStyle.DEFAULT:           return VRS.MapControlStyle.Default;
                case google.maps.MapTypeControlStyle.DROPDOWN_MENU:     return VRS.MapControlStyle.DropdownMenu;
                case google.maps.MapTypeControlStyle.HORIZONTAL_BAR:    return VRS.MapControlStyle.HorizontalBar;
                default:                                                throw 'Not implemented';
            }
        }

        /**
         * Converts from a VRS map type control style to a Google one.
         */
        toGoogleMapControlStyle(mapControlStyle: VRS.MapControlStyleEnum) : google.maps.MapTypeControlStyle
        {
            if(!mapControlStyle) return null;
            switch(mapControlStyle) {
                case VRS.MapControlStyle.Default:           return google.maps.MapTypeControlStyle.DEFAULT;
                case VRS.MapControlStyle.DropdownMenu:      return google.maps.MapTypeControlStyle.DROPDOWN_MENU;
                case VRS.MapControlStyle.HorizontalBar:     return google.maps.MapTypeControlStyle.HORIZONTAL_BAR;
                default:                                    throw 'Not implemented';
            }
        }

        /**
         * Converts from a Google map type to a VRS map type.
         */
        fromGoogleMapType(mapType: google.maps.MapTypeId | string) : VRS.MapTypeEnum
        {
            if(!mapType) return null;
            if(!this._HighContrastMapTypeName) this._HighContrastMapTypeName = VRS.$$.HighContrastMap;
            switch(mapType) {
                case google.maps.MapTypeId.HYBRID:      return VRS.MapType.Hybrid;
                case google.maps.MapTypeId.ROADMAP:     return VRS.MapType.RoadMap;
                case google.maps.MapTypeId.SATELLITE:   return VRS.MapType.Satellite;
                case google.maps.MapTypeId.TERRAIN:     return VRS.MapType.Terrain;
                case this._HighContrastMapTypeName:     return VRS.MapType.HighContrast;
                default:                                throw 'Not implemented';
            }
        }

        /**
         * Converts from a VRS map type to a Google map type.
         */
        toGoogleMapType(mapType: VRS.MapTypeEnum, suppressException: boolean = false) : google.maps.MapTypeId | string
        {
            if(!mapType) return null;
            if(!this._HighContrastMapTypeName) this._HighContrastMapTypeName = VRS.$$.HighContrastMap;
            switch(mapType) {
                case VRS.MapType.Hybrid:        return google.maps.MapTypeId.HYBRID;
                case VRS.MapType.RoadMap:       return google.maps.MapTypeId.ROADMAP;
                case VRS.MapType.Satellite:     return google.maps.MapTypeId.SATELLITE;
                case VRS.MapType.Terrain:       return google.maps.MapTypeId.TERRAIN;
                case VRS.MapType.HighContrast:  return this._HighContrastMapTypeName;
                default:
                    if(suppressException) return null;
                    throw 'Not implemented';
            }
        }

        /**
         * Converts from a Google icon to a VRS map icon.
         */
        fromGoogleIcon(icon: google.maps.Icon) : IMapIcon
        {
            if(!icon) return null;
            else return new MapIcon(
                icon.url,
                this.fromGoogleSize(icon.size),
                this.fromGooglePoint(icon.anchor),
                this.fromGooglePoint(icon.origin),
                this.fromGoogleSize(icon.scaledSize)
            );
        }

        /**
         * Converts from a VRS map icon to a Google icon.
         */
        toGoogleIcon(icon: IMapIcon | string) : google.maps.Icon
        {
            if(!icon) return null;
            if(!(<IMapIcon>icon).url) return <any>icon;

            var result: google.maps.Icon = {};
            if((<IMapIcon>icon).anchor)     result.anchor = this.toGooglePoint((<IMapIcon>icon).anchor);
            if((<IMapIcon>icon).origin)     result.origin = this.toGooglePoint((<IMapIcon>icon).origin);
            if((<IMapIcon>icon).scaledSize) result.scaledSize = this.toGoogleSize((<IMapIcon>icon).scaledSize);
            if((<IMapIcon>icon).size)       result.size = this.toGoogleSize((<IMapIcon>icon).size);
            if((<IMapIcon>icon).url)        result.url = (<IMapIcon>icon).url;

            return result;
        }

        /**
         * Converts from a Google LatLngMVCArray to an VRS array of latLng objects.
         */
        fromGoogleLatLngMVCArray(latLngMVCArray: google.maps.MVCArray) : ILatLng[]
        {
            if(!latLngMVCArray) return null;
            var result: ILatLng[] = [];
            var length = latLngMVCArray.getLength();
            for(var i = 0;i < length;++i) {
                result.push(this.fromGoogleLatLng(latLngMVCArray.getAt(i)));
            }

            return result;
        }

        /**
         * Converts from a VRS array of latLng objects to a Google LatLngMVCArray.
         */
        toGoogleLatLngMVCArray(latLngArray: ILatLng[]) : google.maps.MVCArray
        {
            if(!latLngArray) return null;
            var googleLatLngArray: google.maps.LatLng[] = [];
            var length = latLngArray.length;
            for(var i = 0;i < length;++i) {
                googleLatLngArray.push(this.toGoogleLatLng(latLngArray[i]));
            }

            return new google.maps.MVCArray(googleLatLngArray);
        }

        /**
         * Converts from a Google MVCArray of Google LatLngMVCArrays to an array of
         * an array of VRS latLng objects.
         */
        fromGoogleLatLngMVCArrayArray(latLngMVCArrayArray: google.maps.MVCArray) : ILatLng[][]
        {
            if(!latLngMVCArrayArray) return null;
            var result: ILatLng[][] = [];
            var length = latLngMVCArrayArray.getLength();
            for(var i = 0;i < length;++i) {
                result.push(this.fromGoogleLatLngMVCArray(latLngMVCArrayArray[i]));
            }

            return result;
        }

        /**
         * Converts from an array of an array of VRS_LAT_LNG objects to a Google MVCArray
         * of a Google MVCArray of Google latLng objects.
         */
        toGoogleLatLngMVCArrayArray(latLngArrayArray: ILatLng[][]) : google.maps.MVCArray
        {
            if(!latLngArrayArray) return null;
            var result: google.maps.MVCArray[] = [];
            var length = latLngArrayArray.length;
            for(var i = 0;i < length;++i) {
                result.push(this.toGoogleLatLngMVCArray(latLngArrayArray[i]));
            }

            return new google.maps.MVCArray(result);
        }

        /**
         * Converts from a Google control position to a VRS.MapPosition.
         */
        fromGoogleControlPosition(controlPosition: google.maps.ControlPosition) : MapPositionEnum
        {
            switch(controlPosition) {
                case google.maps.ControlPosition.BOTTOM_CENTER: return VRS.MapPosition.BottomCentre;
                case google.maps.ControlPosition.BOTTOM_LEFT:   return VRS.MapPosition.BottomLeft;
                case google.maps.ControlPosition.BOTTOM_RIGHT:  return VRS.MapPosition.BottomRight;
                case google.maps.ControlPosition.LEFT_BOTTOM:   return VRS.MapPosition.LeftBottom;
                case google.maps.ControlPosition.LEFT_CENTER:   return VRS.MapPosition.LeftCentre;
                case google.maps.ControlPosition.LEFT_TOP:      return VRS.MapPosition.LeftTop;
                case google.maps.ControlPosition.RIGHT_BOTTOM:  return VRS.MapPosition.RightBottom;
                case google.maps.ControlPosition.RIGHT_CENTER:  return VRS.MapPosition.RightCentre;
                case google.maps.ControlPosition.RIGHT_TOP:     return VRS.MapPosition.RightTop;
                case google.maps.ControlPosition.TOP_CENTER:    return VRS.MapPosition.TopCentre;
                case google.maps.ControlPosition.TOP_LEFT:      return VRS.MapPosition.TopLeft;
                case google.maps.ControlPosition.TOP_RIGHT:     return VRS.MapPosition.TopRight;
                default:                                        throw 'Unknown control position ' + controlPosition;
            }
        }

        /**
         * Converts from a VRS.MapPosition to a Google control position.
         */
        toGoogleControlPosition(mapPosition: MapPositionEnum) : google.maps.ControlPosition
        {
            switch(mapPosition) {
                case VRS.MapPosition.BottomCentre:  return google.maps.ControlPosition.BOTTOM_CENTER;
                case VRS.MapPosition.BottomLeft:    return google.maps.ControlPosition.BOTTOM_LEFT;
                case VRS.MapPosition.BottomRight:   return google.maps.ControlPosition.BOTTOM_RIGHT;
                case VRS.MapPosition.LeftBottom:    return google.maps.ControlPosition.LEFT_BOTTOM;
                case VRS.MapPosition.LeftCentre:    return google.maps.ControlPosition.LEFT_CENTER;
                case VRS.MapPosition.LeftTop:       return google.maps.ControlPosition.LEFT_TOP;
                case VRS.MapPosition.RightBottom:   return google.maps.ControlPosition.RIGHT_BOTTOM;
                case VRS.MapPosition.RightCentre:   return google.maps.ControlPosition.RIGHT_CENTER;
                case VRS.MapPosition.RightTop:      return google.maps.ControlPosition.RIGHT_TOP;
                case VRS.MapPosition.TopCentre:     return google.maps.ControlPosition.TOP_CENTER;
                case VRS.MapPosition.TopLeft:       return google.maps.ControlPosition.TOP_LEFT;
                case VRS.MapPosition.TopRight:      return google.maps.ControlPosition.TOP_RIGHT;
                default:                            throw 'Unknown map position ' + mapPosition;
            }
        }
    }

    export var googleMapUtilities = new VRS.GoogleMapUtilities();

    /**
     * An abstracted wrapper around an object that represents a map's native marker.
     */
    class MapMarker implements IMapMarker
    {
        /**
         * The identifier of the marker. Left as a field to speed things up a bit.
         */
        id: string|number;

        /**
         * The native marker object. Leave this alone.
         */
        marker: google.maps.Marker;

        /**
         * True if the native marker is a Google Maps MarkerWithLabel.
         */
        isMarkerWithLabel: boolean;

        /**
         * The object that the marker has been tagged with. Not used by the plugin.
         */
        tag: any;

        /**
         * An array of objects describing the events that have been hooked on the marker.
         */
        nativeListeners: google.maps.MapsEventListener[] = [];

        /**
         * Creates a new object.
         * @param {string|number}       id                  The identifier of the marker
         * @param {google.maps.Marker}  nativeMarker        The native map marker handle to wrap.
         * @param {boolean}             isMarkerWithLabel   Indicates that the native marker is a Google Maps MarkerWithLabel.
         * @param {*}                   tag                 An object to carry around with the marker. No meaning is attached to the tag.
        */
        constructor(id: string|number, nativeMarker: google.maps.Marker, isMarkerWithLabel: boolean, tag: any)
        {
            this.id = id;
            this.marker = nativeMarker;
            this.isMarkerWithLabel = isMarkerWithLabel;
            this.tag = tag;
        }

        /**
         * Returns true if the marker can be dragged.
         */
        getDraggable() : boolean
        {
            return this.marker.getDraggable();
        }

        /**
         * Sets a value indicating whether the marker can be dragged.
         */
        setDraggable(draggable: boolean)
        {
            this.marker.setDraggable(draggable);
        }

        /**
         * Returns the icon for the marker.
         */
        getIcon() : IMapIcon
        {
            return VRS.googleMapUtilities.fromGoogleIcon(this.marker.getIcon());
        }

        /**
         * Sets the icon for the marker.
         */
        setIcon(icon: IMapIcon)
        {
            this.marker.setIcon(VRS.googleMapUtilities.toGoogleIcon(icon));
        }

        /**
         * Gets the coordinates of the marker.
         */
        getPosition() : ILatLng
        {
            return VRS.googleMapUtilities.fromGoogleLatLng(this.marker.getPosition());
        }

        /**
         * Sets the coordinates for the marker.
         */
        setPosition(position: ILatLng)
        {
            this.marker.setPosition(VRS.googleMapUtilities.toGoogleLatLng(position));
        }

        /**
         * Gets the tooltip for the marker.
         */
        getTooltip() : string
        {
            return this.marker.getTitle();
        }

        /**
         * Sets the tooltip for the marker.
         */
        setTooltip(tooltip: string)
        {
            this.marker.setTitle(tooltip);
        }

        /**
         * Gets a value indicating that the marker is visible.
         */
        getVisible() : boolean
        {
            return this.marker.getVisible();
        }

        /**
         * Sets a value indicating whether the marker is visible.
         */
        setVisible(visible: boolean)
        {
            this.marker.setVisible(visible);
        }

        /**
         * Gets the z-index of the marker.
         */
        getZIndex() : number
        {
            return this.marker.getZIndex();
        }

        /**
         * Sets the z-index of the marker.
         */
        setZIndex(zIndex: number)
        {
            this.marker.setZIndex(zIndex);
        }

        /**
         * Returns true if the marker was created with useMarkerWithLabel and the label is visible.
         * Note that this is not a part of the marker interface.
         */
        getLabelVisible() : boolean
        {
            return this.isMarkerWithLabel ? this.marker.get('labelVisible') : false;
        }

        /**
         * Sets the visibility of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelVisible(visible: boolean)
        {
            if(this.isMarkerWithLabel) this.marker.set('labelVisible', visible);
        }

        /**
         * Sets the label content. Only works on markers that have been created with useMarkerWithLabel.
         */
        getLabelContent()
        {
            return this.isMarkerWithLabel ? this.marker.get('labelContent') : null;
        }

        /**
         * Sets the content of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelContent(content: string)
        {
            if(this.isMarkerWithLabel) this.marker.set('labelContent', content);
        }

        /**
         * Gets the label anchor. Only works on markers that have been created with useMarkerWithLabel.
         */
        getLabelAnchor()
        {
            return this.isMarkerWithLabel ? VRS.googleMapUtilities.fromGooglePoint(this.marker.get('labelAnchor')) : null;
        }

        /**
         * Sets the anchor for a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelAnchor(anchor: IPoint)
        {
            if(this.isMarkerWithLabel) this.marker.set('labelAnchor', VRS.googleMapUtilities.toGooglePoint(anchor));
        }
    }

    /**
     * An object that can cluster map markers together.
     */
    class MapMarkerClusterer implements IMapMarkerClusterer
    {
        constructor(private map: MapPlugin, private nativeMarkerClusterer: MarkerClusterer)
        {
        }

        getNative()
        {
            return this.nativeMarkerClusterer;
        }

        getNativeType()
        {
            return 'GoogleMaps';
        }

        getMaxZoom()
        {
            return this.nativeMarkerClusterer.getMaxZoom();
        }

        setMaxZoom(maxZoom: number)
        {
            this.nativeMarkerClusterer.setMaxZoom(maxZoom);
        }

        addMarker(marker: IMapMarker, noRepaint?: boolean)
        {
            this.nativeMarkerClusterer.addMarker((<MapMarker>marker).marker, noRepaint);
        }

        addMarkers(markers: IMapMarker[], noRepaint?: boolean)
        {
            this.nativeMarkerClusterer.addMarkers(this.castArrayOfMarkers(markers), noRepaint);
        }

        removeMarker(marker: IMapMarker, noRepaint?: boolean)
        {
            this.nativeMarkerClusterer.removeMarker((<MapMarker>marker).marker, noRepaint, true);
        }

        removeMarkers(markers: IMapMarker[], noRepaint?: boolean)
        {
            this.nativeMarkerClusterer.removeMarkers(this.castArrayOfMarkers(markers), noRepaint, true);
        }

        repaint()
        {
            this.nativeMarkerClusterer.repaint();
        }

        private castArrayOfMarkers(markers: IMapMarker[]) : google.maps.Marker[]
        {
            var result: google.maps.Marker[] = [];

            var length = markers ? markers.length : 0;
            for(var i = 0;i < length;++i) {
                result.push((<MapMarker>markers[i]).marker);
            }

            return result;
        }
    }

    /**
     * Describes a polygon on a map.
     */
    class MapPolygon implements IMapPolygon
    {
        /**
         * The VRS ID for the polygon.
         */
        id: string | number;

        /**
         * The native polygon.
         */
        polygon: google.maps.Polygon;

        /**
         * The application's tag attached to the polygon.
         */
        tag: any;

        constructor(id: string | number, nativePolygon: google.maps.Polygon, tag: any, options: IMapPolygonSettings)
        {
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

        /**
         * Gets a value indicating that the polygon is draggable.
         */
        getDraggable() : boolean
        {
            return this.polygon.getDraggable();
        }

        /**
         * Sets a value indicating whether the polygon is draggable.
         */
        setDraggable(draggable: boolean)
        {
            this.polygon.setDraggable(draggable);
        }

        /**
         * Gets a value indicating whether the polygon can be changed by the user.
         */
        getEditable() : boolean
        {
            return this.polygon.getEditable();
        }

        /**
         * Sets a value indicating whether the user can change the polygon.
         */
        setEditable(editable: boolean)
        {
            this.polygon.setEditable(editable);
        }

        /**
         * Gets a value indicating that the polygon is visible.
         */
        getVisible() : boolean
        {
            return this.polygon.getVisible();
        }

        /**
         * Sets a value indicating whether the polygon is visible.
         */
        setVisible(visible: boolean)
        {
            this.polygon.setVisible(visible);
        }

        /**
         * Gets the first path.
         */
        getFirstPath() : ILatLng[]
        {
            return VRS.googleMapUtilities.fromGoogleLatLngMVCArray(this.polygon.getPath());
        }

        /**
         * Sets the first path.
         */
        setFirstPath(path: ILatLng[])
        {
            this.polygon.setPath(VRS.googleMapUtilities.toGoogleLatLngMVCArray(path));
        }

        /**
         * Gets an array of every path array.
         */
        getPaths() : ILatLng[][]
        {
            return VRS.googleMapUtilities.fromGoogleLatLngMVCArrayArray(this.polygon.getPaths());
        }

        /**
         * Sets an array of every path array.
         */
        setPaths(paths: ILatLng[][])
        {
            this.polygon.setPaths(VRS.googleMapUtilities.toGoogleLatLngMVCArrayArray(paths));
        }

        private _Clickable: boolean;
        /**
         * Gets a value indicating whether the polygon handles mouse events.
         */
        getClickable() : boolean
        {
            return this._Clickable;
        }
        /**
         * Sets a value that indicates whether the polygon handles mouse events.
         */
        setClickable(value: boolean)
        {
            if(value !== this._Clickable) {
                this._Clickable = value;
                this.polygon.setOptions({ clickable: value });
            }
        }

        private _FillColour: string;
        /**
         * Gets the CSS colour of the fill area.
         */
        getFillColour() : string
        {
            return this._FillColour;
        }
        /**
         * Sets the CSS colour of the fill area.
         */
        setFillColour(value: string)
        {
            if(value !== this._FillColour) {
                this._FillColour = value;
                this.polygon.setOptions({ fillColor: value });
            }
        }

        private _FillOpacity: number;
        /**
         * Gets the opacity of the fill area.
         */
        getFillOpacity() : number
        {
            return this._FillOpacity;
        }
        /**
         * Sets the opacity of the fill area (between 0 and 1).
         */
        setFillOpacity(value: number)
        {
            if(value !== this._FillOpacity) {
                this._FillOpacity = value;
                this.polygon.setOptions({ fillOpacity: value });
            }
        }

        private _StrokeColour: string;
        /**
         * Gets the CSS colour of the stroke line.
         */
        getStrokeColour() : string
        {
            return this._StrokeColour;
        }
        /**
         * Sets the CSS colour of the stroke line.
         */
        setStrokeColour(value: string)
        {
            if(value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polygon.setOptions({ strokeColor: value });
            }
        }

        private _StrokeOpacity: number;
        /**
         * Gets the opacity of the stroke line.
         */
        getStrokeOpacity() : number
        {
            return this._StrokeOpacity;
        }
        /**
         * Sets the opacity of the stroke line (between 0 and 1).
         */
        setStrokeOpacity(value: number)
        {
            if(value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polygon.setOptions({ strokeOpacity: value });
            }
        }

        private _StrokeWeight: number;
        /**
         * Gets the weight of the stroke line in pixels.
         */
        getStrokeWeight() : number
        {
            return this._StrokeWeight;
        }
        /**
         * Sets the weight of the stroke line in pixels.
         */
        setStrokeWeight(value: number)
        {
            if(value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polygon.setOptions({ strokeWeight: value });
            }
        }

        private _ZIndex: number;
        /**
         * Gets the z-index of the polygon.
         */
        getZIndex() : number
        {
            return this._ZIndex;
        }
        /**
         * Sets the z-index of the polygon.
         */
        setZIndex(value: number)
        {
            if(value !== this._ZIndex) {
                this._ZIndex = value;
                this.polygon.setOptions({ zIndex: value });
            }
        }
    }

    /**
     * An object that wraps a map's native polyline object.
     */
    class MapPolyline implements IMapPolyline
    {
        id:         string | number;
        polyline:   google.maps.Polyline;
        tag:        any;

        constructor(id: string | number, nativePolyline: google.maps.Polyline, tag: any, options: IMapPolylineSettings)
        {
            this.id = id;
            this.polyline = nativePolyline;
            this.tag = tag;

            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
        }

        /**
         * Gets a value indicating that the line is draggable.
         */
        getDraggable() : boolean
        {
            return this.polyline.getDraggable();
        }
        /**
         * Sets a value indicating whether the line is draggable.
         */
        setDraggable(draggable: boolean)
        {
            this.polyline.setDraggable(draggable);
        }

        /**
         * Gets a value indicating whether the line can be changed by the user.
         */
        getEditable() : boolean
        {
            return this.polyline.getEditable();
        }
        /**
         * Sets a value indicating whether the user can change the line.
         */
        setEditable(editable: boolean)
        {
            this.polyline.setEditable(editable);
        }

        /**
         * Gets a value indicating whether the line is visible.
         */
        getVisible() : boolean
        {
            return this.polyline.getVisible();
        }
        /**
         * Sets a value indicating whether the line is visible.
         */
        setVisible(visible: boolean)
        {
            this.polyline.setVisible(visible);
        }

        private _StrokeColour: string;
        /**
         * Gets the CSS colour of the line.
         */
        getStrokeColour() : string
        {
            return this._StrokeColour;
        }
        /**
         * Sets the CSS colour of the line.
         */
        setStrokeColour(value: string)
        {
            if(value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polyline.setOptions({ strokeColor: value });
            }
        }

        private _StrokeOpacity: number;
        /**
         * Gets the opacity of the line.
         */
        getStrokeOpacity() : number
        {
            return this._StrokeOpacity;
        }
        /**
         * Sets the opacity of the line (between 0 and 1).
         */
        setStrokeOpacity(value: number)
        {
            if(value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polyline.setOptions({ strokeOpacity: value });
            }
        }

        private _StrokeWeight: number;
        /**
         * Gets the weight of the line in pixels.
         */
        getStrokeWeight() : number
        {
            return this._StrokeWeight;
        }
        /**
         * Sets the weight of the line in pixels.
         */
        setStrokeWeight(value: number)
        {
            if(value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polyline.setOptions({ strokeWeight: value });
            }
        }

        /**
         * Gets the path for the polyline.
         */
        getPath() : ILatLng[]
        {
            var result = VRS.googleMapUtilities.fromGoogleLatLngMVCArray(this.polyline.getPath());
            return result || [];
        }
        /**
         * Sets the path for the polyline.
         */
        setPath(path: ILatLng[])
        {
            var nativePath = VRS.googleMapUtilities.toGoogleLatLngMVCArray(path);
            this.polyline.setPath(nativePath);
        }

        /**
         * Returns the first point on the path or null if the path is empty.
         */
        getFirstLatLng() : ILatLng
        {
            var result = null;
            var nativePath = this.polyline.getPath();
            if(nativePath.getLength()) result = VRS.googleMapUtilities.fromGoogleLatLng(nativePath.getAt(0));

            return result;
        }

        /**
         * Returns the last point on the path or null if the path is empty.
         */
        getLastLatLng() : ILatLng
        {
            var result = null;
            var nativePath = this.polyline.getPath();
            var length = nativePath.getLength();
            if(length) result = VRS.googleMapUtilities.fromGoogleLatLng(nativePath.getAt(length - 1));

            return result;
        }
    }

    /**
     * An object that wraps a map's native circle object.
     * @constructor
     */
    class MapCircle implements IMapCircle
    {
        id:     string | number;
        circle: google.maps.Circle;
        tag:    any;

        /**
         * Creates a new object.
         * @param {string|number}           id                  The unique identifier of the circle object.
         * @param {google.maps.Circle}      nativeCircle        The native object that is being wrapped.
         * @param {*}                       tag                 An object attached to the circle.
         * @param {IMapCircleSettings}      options             The options used when the circle was created.
        */
        constructor(id: string | number, nativeCircle: google.maps.Circle, tag: any, options: IMapCircleSettings)
        {
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

        getBounds() : IBounds
        {
            return VRS.googleMapUtilities.fromGoogleLatLngBounds(this.circle.getBounds());
        }

        getCenter() : ILatLng
        {
            return VRS.googleMapUtilities.fromGoogleLatLng(this.circle.getCenter());
        }
        setCenter(value: ILatLng)
        {
            this.circle.setCenter(VRS.googleMapUtilities.toGoogleLatLng(value));
        }

        getDraggable() : boolean
        {
            return this.circle.getDraggable();
        }
        setDraggable(value: boolean)
        {
            this.circle.setDraggable(value);
        }

        getEditable() : boolean
        {
            return this.circle.getEditable();
        }
        setEditable(value: boolean)
        {
            this.circle.setEditable(value);
        }

        getRadius() : number
        {
            return this.circle.getRadius();
        }
        setRadius(value: number)
        {
            this.circle.setRadius(value);
        }

        getVisible() : boolean
        {
            return this.circle.getVisible();
        }
        setVisible(value: boolean)
        {
            this.circle.setVisible(value);
        }

        private _FillColour: string;
        getFillColor() : string
        {
            return this._FillColour;
        }
        setFillColor(value: string)
        {
            if(this._FillColour !== value) {
                this._FillColour = value;
                this.circle.setOptions({ fillColor: value });
            }
        }

        private _FillOpacity: number;
        getFillOpacity() : number
        {
            return this._FillOpacity;
        }
        setFillOpacity(value: number)
        {
            if(this._FillOpacity !== value) {
                this._FillOpacity = value;
                this.circle.setOptions({ fillOpacity: value });
            }
        }

        private _StrokeColour: string;
        getStrokeColor() : string
        {
            return this._StrokeColour;
        }
        setStrokeColor(value: string)
        {
            if(this._StrokeColour !== value) {
                this._StrokeColour = value;
                this.circle.setOptions({ strokeColor: value });
            }
        }

        private _StrokeOpacity: number;
        getStrokeOpacity() : number
        {
            return this._StrokeOpacity;
        }
        setStrokeOpacity(value: number)
        {
            if(this._StrokeOpacity !== value) {
                this._StrokeOpacity = value;
                this.circle.setOptions({ strokeOpacity: value });
            }
        }

        private _StrokeWeight: number;
        getStrokeWeight() : number
        {
            return this._StrokeWeight;
        }
        setStrokeWeight(value: number)
        {
            if(this._StrokeWeight !== value) {
                this._StrokeWeight = value;
                this.circle.setOptions({ strokeWeight: value });
            }
        }

        private _ZIndex: number;
        getZIndex() : number
        {
            return this._ZIndex;
        }
        setZIndex(value: number)
        {
            if(this._ZIndex !== value) {
                this._ZIndex = value;
                this.circle.setOptions({ zIndex: value });
            }
        }
    }

    /**
     * A wrapper around a map's native info window.
     */
    class MapInfoWindow implements IMapInfoWindow
    {
        id:         string | number;
        infoWindow: google.maps.InfoWindow;
        tag:        any;
        isOpen:     boolean;

        /**
         * Creates a new object.
         * @param {string|number}           id                  The unique identifier of the info window
         * @param {google.maps.InfoWindow}  nativeInfoWindow    The map's native info window object that this wraps.
         * @param {*}                       tag                 An abstract object that is associated with the info window.
         * @param {IMapInfoWindowSettings}  options             The options used to create the info window.
        */
        constructor(id: string | number, nativeInfoWindow: google.maps.InfoWindow, tag: any, options: IMapInfoWindowSettings)
        {
            this.id = id;
            this.infoWindow = nativeInfoWindow;
            this.tag = tag;
            this.isOpen = false;

            this._DisableAutoPan = options.disableAutoPan;
            this._MaxWidth = options.maxWidth;
            this._PixelOffset = options.pixelOffset;
        }

        /**
         * An array of objects describing the events that have been hooked on the info window.
         */
        nativeListeners: google.maps.MapsEventListener[] = [];

        getContent() : Element
        {
            return <Element>this.infoWindow.getContent();
        }
        setContent(value: Element)
        {
            this.infoWindow.setContent(value);
        }

        private _DisableAutoPan: boolean;
        getDisableAutoPan() : boolean
        {
            return this._DisableAutoPan;
        }
        setDisableAutoPan(value: boolean)
        {
            if(this._DisableAutoPan !== value) {
                this._DisableAutoPan = value;
                this.infoWindow.setOptions({ disableAutoPan: value });
            }
        }

        private _MaxWidth: number;
        getMaxWidth() : number
        {
            return this._MaxWidth;
        }
        setMaxWidth(value: number)
        {
            if(this._MaxWidth !== value) {
                this._MaxWidth = value;
                this.infoWindow.setOptions({ maxWidth: value });
            }
        }

        private _PixelOffset: ISize;
        getPixelOffset() : ISize
        {
            return this._PixelOffset;
        }
        setPixelOffset(value: ISize)
        {
            if(this._PixelOffset !== value) {
                this._PixelOffset = value;
                this.infoWindow.setOptions({ pixelOffset: VRS.googleMapUtilities.toGoogleSize(value) });
            }
        }

        getPosition() : ILatLng
        {
            return VRS.googleMapUtilities.fromGoogleLatLng(this.infoWindow.getPosition());
        }
        setPosition(value: ILatLng)
        {
            this.infoWindow.setPosition(VRS.googleMapUtilities.toGoogleLatLng(value));
        }

        getZIndex() : number
        {
            return this.infoWindow.getZIndex();
        }
        setZIndex(value: number)
        {
            this.infoWindow.setZIndex(value);
        }
    }

    /**
     * The state held for every map plugin object.
     */
    class MapPluginState
    {
        /**
         * The map that the plugin wraps.
         */
        map: google.maps.Map = undefined;

        /**
         * The map's container.
         */
        mapContainer: JQuery = undefined;

        /**
         * An associative array of marker IDs to markers.
         */
        markers: { [markerId: string]: MapMarker } = {};

        /**
         * An associative array of polyline IDs to polylines.
         */
        polylines: { [polylineId: string]: MapPolyline } = {};

        /**
         * An associative array of polygon IDs to polygons.
         */
        polygons: { [polygonId: string]: MapPolygon } = {};

        /**
         * An associative array of circle IDs to circles.
         */
        circles: { [circleId: string]: MapCircle } = {};

        /**
         * An associative array of info window IDs to info windows.
         */
        infoWindows: { [infoWindowId: string]: MapInfoWindow } = {};

        /**
         * An array of Google Maps listener objects that we can use to unhook ourselves from the map when the plugin
         * is destroyed.
         */
        nativeHooks: google.maps.MapsEventListener[] = [];
    }

    /*
     * jQueryUIHelper
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    jQueryUIHelper.getMapPlugin = (jQueryElement: JQuery) : IMap =>
    {
        return <IMap>jQueryElement.data('vrsVrsMap');
    }
    jQueryUIHelper.getMapOptions = (overrides: IMapOptions) : IMapOptions =>
    {
        return $.extend({
            // Google Map load options - THESE ONLY HAVE ANY EFFECT ON THE FIRST MAP LOADED ON A PAGE
            key:                null,                                   // If supplied then the Google Maps script is loaded with this API key. API keys are no longer optional for public servers but remain optional for LAN and local loopback servers.
            version:            '3.36',                                 // The version of Google Maps to load.
            sensor:             false,                                  // True if the location-aware stuff is to be turned on.
            libraries:          [],                                     // The optional libraries to load.
            loadMarkerWithLabel:false,                                  // Loads the marker-with-labels library after loading Google Maps.
            loadMarkerCluster:  false,                                  // Loads the marker cluster library after loading Google Maps.

            // Google map open options
            openOnCreate:       true,                                   // Open the map when the widget is created, if false then the code that creates the map has to call open() itself.
            waitUntilReady:     true,                                   // If true then the widget does not call afterOpen until after the map has completely loaded. If this is false then calling getBounds (and perhaps other calls) may fail until the map has loaded.
            zoom:               12,                                     // The zoom level to open with
            center:             { lat: 51.5, lng: -0.125 },             // The location to centre the map on
            showMapTypeControl: true,                                   // True to show the map type control, false to hide it.
            mapTypeId:          VRS.MapType.Hybrid,                     // The map type to start with
            streetViewControl:  VRS.globalOptions.mapShowStreetView,    // Whether to show Street View or not
            scrollwheel:        VRS.globalOptions.mapScrollWheelActive, // Whether the scrollwheel zooms the map
            scaleControl:       VRS.globalOptions.mapShowScaleControl,  // Whether to show the scale control or not.
            draggable:          VRS.globalOptions.mapDraggable,         // Whether the map is draggable
            controlStyle:       VRS.MapControlStyle.Default,            // The style of map control to display
            controlPosition:    undefined,                              // Where the map control should be placed
            pointsOfInterest:   VRS.globalOptions.mapShowPointsOfInterest, // Whether to show Google Maps' points of interest
            showHighContrast:   VRS.globalOptions.mapShowHighContrastStyle, // Whether to show the custom high-contrast map style or not.

            // Custom map open options
            mapControls:        [],                                     // Controls to add to the map after it has been opened.

            // Callbacks
            afterCreate:        null,                                   // Called after the map has been created but before it has been opened
            afterOpen:          null,                                   // Called after the map has been opened

            // Persistence options
            name:               'default',                              // The name to use to distinguish the state settings from those of other maps
            useStateOnOpen:     false,                                  // Load and apply state when opening the map
            autoSaveState:      false,                                  // Automatically save state whenever any state variable is changed by the user
            useServerDefaults:  false,                                  // Always use the server-supplied configuration settings rather than those in options.

            __nop:      null
        }, overrides);
    }

    /**
     * A jQuery UI plugin that wraps the Google Maps map.
     */
    class MapPlugin extends JQueryUICustomWidget implements IMap
    {
        options: IMapOptions;
        private _EventPluginName = 'vrsMap';

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getMapOptions();
        }

        private _getState() : MapPluginState
        {
            var result = this.element.data('mapPluginState');
            if(result === undefined) {
                result = new MapPluginState();
                this.element.data('mapPluginState', result);
            }

            return result;
        }

        _create()
        {
            var self = this;

            if(this.options.useServerDefaults && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    this.options.center = { lat: config.InitialLatitude, lng: config.InitialLongitude };
                    this.options.mapTypeId = config.InitialMapType;
                    this.options.zoom = config.InitialZoom;
                }
            }

            this._loadGoogleMapsScript(function() {
                var state = self._getState();
                state.mapContainer = $('<div />')
                    .addClass('vrsMap')
                    .appendTo(self.element);

                if(self.options.afterCreate) {
                    self.options.afterCreate(this);
                }
                if(self.options.openOnCreate) {
                    self.open();
                }

                if(VRS.refreshManager) VRS.refreshManager.registerTarget(self.element, self._targetResized, self);
            }, function(jqXHR, textStatus, errorThrown) {
                var state = self._getState();
                state.mapContainer = $('<div />')
                    .addClass('vrsMap notOnline')
                    .appendTo(self.element);
                $('<p/>')
                    .text(VRS.$$.GoogleMapsCouldNotBeLoaded + ': ' + textStatus)
                    .appendTo(state.mapContainer);

                if(self.options.afterCreate) {
                    self.options.afterCreate(this);
                }
                if(self.options.openOnCreate && self.options.afterOpen) {
                    self.options.afterOpen(self);
                }
            });
        }

        _destroy()
        {
            var state = this._getState();

            if(VRS.refreshManager) VRS.refreshManager.unregisterTarget(this.element);

            $.each(state.nativeHooks, function(idx, hookResult) {
                google.maps.event.removeListener(hookResult);
            });
            state.nativeHooks = [];

            if(state.mapContainer) state.mapContainer.remove();
        }


        /*
         * MAP INITIALISATION
         */


        /**
         * Loads Google Maps. Note that only the first call to this on a page will actually do anything.
         */
        _loadGoogleMapsScript(successCallback: () => void, failureCallback: (jqXHR: JQueryXHR, status: string, error: string) => void)
        {
            var url = VRS.globalOptions.mapGoogleMapUseHttps ? VRS.globalOptions.mapGoogleMapHttpsUrl : VRS.globalOptions.mapGoogleMapHttpUrl;
            var params = <any>{
                // Note that Google Maps no longer requires the sensor flag and will report a warning if it is used
                v: this.options.version
            };
            var googleMapsApiKey = this.options.key;
            if(!googleMapsApiKey && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config && config.GoogleMapsApiKey) {
                    googleMapsApiKey = config.GoogleMapsApiKey;
                }
            }
            if(googleMapsApiKey) {
                params.key = googleMapsApiKey;
            }

            if(this.options.libraries.length > 0) {
                params.libraries = this.options.libraries.join(',');
            }

            if(VRS.browserHelper && VRS.browserHelper.notOnline()) {
                failureCallback(null, VRS.$$.WorkingInOfflineMode, VRS.$$.WorkingInOfflineMode);
            } else {
                var callback = successCallback;
                if(this.options.loadMarkerWithLabel) {
                    var chainCallbackMarkerWithLabel = callback;
                    callback = function() {
                        VRS.scriptManager.loadScript({
                            key:        'markerWithLabel',
                            url:        'script/markerWithLabel.js',
                            queue:      true,
                            success:    chainCallbackMarkerWithLabel
                        });
                    };
                }
                if(this.options.loadMarkerCluster) {
                    var chainCallbackMarkerCluster = callback;
                    callback = function() {
                        VRS.scriptManager.loadScript({
                            key:        'markerCluster',
                            url:        'script/markercluster.js',
                            queue:      true,
                            success:    chainCallbackMarkerCluster
                        });
                    };
                }

                if(window['google'] && window['google']['maps']) {
                    callback();
                } else {
                    VRS.scriptManager.loadScript({
                        key:        VRS.scriptKey.GoogleMaps,
                        url:        url,
                        params:     params,
                        queue:      true,
                        success:    callback,
                        error:      failureCallback || null,
                        timeout:    VRS.globalOptions.mapGoogleMapTimeout
                    });
                }
            }
        }


        /*
         * PROPERTIES
         */


        /**
         * Gets the native map object.
         */
        getNative(): any
        {
            return this._getState().map;
        }

        /**
         * Returns a string indicating what kind of map object is returned by getNative().
         */
        getNativeType() : string
        {
            return 'GoogleMaps';
        }

        /**
         * Gets a value indicating that the map was successfully opened.
         */
        isOpen() : boolean
        {
            return !!this._getState().map;
        }

        /**
         * Gets a value indicating that the map has initialised and is ready for use.
         */
        isReady() : boolean
        {
            var state = this._getState();
            return !!state.map && !!state.map.getBounds();
        }

        /**
         * Gets the rectangle of coordinates that the map is displaying.
         */
        getBounds() : IBounds
        {
            return this._getBounds(this._getState());
        }
        private _getBounds(state: MapPluginState)
        {
            return state.map ? VRS.googleMapUtilities.fromGoogleLatLngBounds(state.map.getBounds()) : { tlLat: 0, tlLng: 0, brLat: 0, brLng: 0};
        }

        /**
         * Gets the coordinate at the centre of the map.
         */
        getCenter() : ILatLng
        {
            return this._getCenter(this._getState());
        }
        private _getCenter(state: MapPluginState)
        {
            return state.map ? VRS.googleMapUtilities.fromGoogleLatLng(state.map.getCenter()) : this.options.center;
        }
        /**
         * Moves the map so that the coordinate passed across is the centre of the map.
         */
        setCenter(latLng: ILatLng)
        {
            this._setCenter(this._getState(), latLng);
        }
        private _setCenter(state: MapPluginState, latLng: ILatLng)
        {
            if(state.map) state.map.setCenter(VRS.googleMapUtilities.toGoogleLatLng(latLng));
            else          this.options.center = latLng;
        }

        /**
         * Returns true if the map is draggable, false if it is not.
         */
        getDraggable() : boolean
        {
            return this.options.draggable;
        }

        /**
         * Returns the currently selected map type.
         */
        getMapType() : MapTypeEnum
        {
            return this._getMapType(this._getState());
        }
        private _getMapType(state: MapPluginState) : MapTypeEnum
        {
            return state.map ? VRS.googleMapUtilities.fromGoogleMapType(state.map.getMapTypeId()) : this.options.mapTypeId;
        }
        /**
         * Sets the currently selected map type.
         */
        setMapType(mapType: MapTypeEnum)
        {
            this._setMapType(this._getState(), mapType);
        }
        private _setMapType(state: MapPluginState, mapType: MapTypeEnum)
        {
            if(!state.map) {
                this.options.mapTypeId = mapType;
            } else {
                var currentMapType = this.getMapType();
                if(currentMapType !== mapType) state.map.setMapTypeId(VRS.googleMapUtilities.toGoogleMapType(mapType));
            }
        }

        /**
         * Gets a value indicating whether the scroll wheel zooms the map.
         */
        getScrollWheel() : boolean
        {
            return this.options.scrollwheel;
        }

        /**
         * Gets a value indicating whether Google StreetView is enabled for the map.
         */
        getStreetView() : boolean
        {
            return this.options.streetViewControl;
        }

        /**
         * Gets the current zoom level.
         */
        getZoom() : number
        {
            return this._getZoom(this._getState());
        }
        private _getZoom(state: MapPluginState) : number
        {
            return state.map ? state.map.getZoom() : this.options.zoom;
        }
        /**
         * Sets the current zoom level.
         */
        setZoom(zoom: number)
        {
            this._setZoom(this._getState(), zoom);
        }
        private _setZoom(state: MapPluginState, zoom: number)
        {
            if(state.map) state.map.setZoom(zoom);
            else          this.options.zoom = zoom;
        }


        /*
         * MAP EVENTS EXPOSED
         */


        /**
         * Unhooks any event hooked on this plugin.
         */
        unhook(hookResult: IEventHandleJQueryUI)
        {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        }

        /**
         * Raised when the map's boundaries change.
         */
        hookBoundsChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'boundsChanged', callback, forceThis);
        }
        private _raiseBoundsChanged()
        {
            this._trigger('boundsChanged');
        }

        /**
         * Raised when the map is moved.
         */
        hookCenterChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'centerChanged', callback, forceThis);
        }
        private _raiseCenterChanged()
        {
            this._trigger('centerChanged');
        }

        /**
         * Raised when the map is clicked.
         */
        hookClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'clicked', callback, forceThis);
        }
        private _raiseClicked(mouseEvent: Event)
        {
            this._trigger('clicked', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        /**
         * Raised when the map is double-clicked.
         */
        hookDoubleClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'doubleClicked', callback, forceThis);
        }
        private _raiseDoubleClicked(mouseEvent: Event)
        {
            this._trigger('doubleClicked', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        /**
         * Raised after the user has stopped moving, zooming or otherwise changing the map's properties.
         */
        hookIdle(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'idle', callback, forceThis);
        }
        private _raiseIdle()
        {
            this._trigger('idle');
        }

        /**
         * Raised after the user changes the map type.
         */
        hookMapTypeChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mapTypeChanged', callback, forceThis);
        }
        private _raiseMapTypeChanged()
        {
            this._trigger('mapTypeChanged');
        }

        /**
         * Raised when the user right-clcks the map.
         */
        hookRightClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mouseEvent', callback, forceThis);
        }
        private _raiseRightClicked(mouseEvent: Event)
        {
            this._trigger('mouseEvent', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        /**
         * Raised after the map's images have been displayed or changed.
         */
        hookTilesLoaded(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'tilesLoaded', callback, forceThis);
        }
        private _raiseTilesLoaded()
        {
            this._trigger('tilesLoaded');
        }

        /**
         * Raised after the map has been zoomed.
         */
        hookZoomChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'zoomChanged', callback, forceThis);
        }
        private _raiseZoomChanged()
        {
            this._trigger('zoomChanged');
        }


        /*
         * CHILD OBJECT EVENTS EXPOSED
         */


        /**
         * Raised when a map marker is clicked.
         */
        hookMarkerClicked(callback: (event?: Event, data?: IMapMarkerEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerClicked', callback, forceThis);
        }
        private _raiseMarkerClicked(id: string | number)
        {
            this._trigger('markerClicked', null, <IMapMarkerEventArgs>{ id: id });
        }

        /**
         * Raised when a map marker is dragged.
         */
        hookMarkerDragged(callback: (event?: Event, data?: IMapMarkerEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerDragged', callback, forceThis);
        }
        private _raiseMarkerDragged(id: string | number)
        {
            this._trigger('markerDragged', null, <IMapMarkerEventArgs>{ id: id });
        }

        /**
         * Raised after the user closes an InfoWindow.
         */
        hookInfoWindowClosedByUser(callback: (event?: Event, data?: IMapInfoWindowEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'infoWindowClosedByUser', callback, forceThis);
        }
        private _raiseInfoWindowClosedByUser(id: string | number)
        {
            this._trigger('infoWindowClosedByUser', null, <IMapInfoWindowEventArgs>{ id: id });
        }


        //
        // MAP EVENT HANDLERS
        //



        /**
         * Called when the map becomes idle.
         */
        private _onIdle()
        {
            if(this.options.autoSaveState) this.saveState();
            this._raiseIdle();
        }

        /**
         * Called after the map type has changed.
         */
        private _onMapTypeChanged()
        {
            if(this.options.autoSaveState) this.saveState();
            this._raiseMapTypeChanged();
        }


        //
        // BASIC MAP OPERATIONS
        //


        /**
         * Opens the map. If you configure the options so that the map is not opened when the widget is created then the
         * Google Maps javascript might still be loading when you call this, in which case the call will fail. The rule
         * is that you either auto-open the map using the options or you use a script tag and wait until the document
         * is ready before you call this method.
         */
        open(userOptions?: IMapOpenOptions)
        {
            var self = this;
            var mapOptions: IMapOptions = $.extend(<IMapOptions>{}, userOptions, {
                zoom:               this.options.zoom,
                center:             this.options.center,
                mapTypeControl:     this.options.showMapTypeControl,
                mapTypeId:          this.options.mapTypeId,
                streetViewControl:  this.options.streetViewControl,
                scrollwheel:        this.options.scrollwheel,
                scaleControl:       this.options.scaleControl,
                draggable:          this.options.draggable,
                showHighContrast:   this.options.showHighContrast,
                controlStyle:       this.options.controlStyle,
                controlPosition:    this.options.controlPosition,
                mapControls:        this.options.mapControls
            });

            if(this.options.useStateOnOpen) {
                var settings = this.loadState();
                mapOptions.zoom = settings.zoom;
                mapOptions.center = settings.center;
                mapOptions.mapTypeId = settings.mapTypeId;
            }

            var googleMapOptions: google.maps.MapOptions = {
                zoom:                   mapOptions.zoom,
                center:                 VRS.googleMapUtilities.toGoogleLatLng(mapOptions.center),
                streetViewControl:      mapOptions.streetViewControl,
                scrollwheel:            mapOptions.scrollwheel,
                draggable:              mapOptions.draggable,
                scaleControl:           mapOptions.scaleControl,
                mapTypeControlOptions:  {
                    style: VRS.googleMapUtilities.toGoogleMapControlStyle(mapOptions.controlStyle)
                }
            };
            if(mapOptions.controlPosition) {
                googleMapOptions.mapTypeControlOptions.position = VRS.googleMapUtilities.toGoogleControlPosition(mapOptions.controlPosition);
            }
            if(!mapOptions.pointsOfInterest) {
                googleMapOptions.styles = [
                    {
                        featureType: 'poi',
                        elementType: 'labels',
                        stylers: [{ visibility: 'off' }]
                    }
                ];
            }

            var highContrastMap: google.maps.MapType;
            var highContrastMapName = <string>VRS.googleMapUtilities.toGoogleMapType(VRS.MapType.HighContrast);
            if(mapOptions.showHighContrast && VRS.globalOptions.mapHighContrastMapStyle && VRS.globalOptions.mapHighContrastMapStyle.length) {
                var googleMapTypeIds = [];
                $.each(VRS.MapType, function(idx, mapType: MapTypeEnum) {
                    var googleMapType = VRS.googleMapUtilities.toGoogleMapType(mapType);
                    if(googleMapType) googleMapTypeIds.push(googleMapType);
                });
                googleMapOptions.mapTypeControlOptions.mapTypeIds = googleMapTypeIds;
                var highContrastMapStyle: google.maps.MapTypeStyle[] = VRS.globalOptions.mapHighContrastMapStyle;
                highContrastMap = new google.maps.StyledMapType(highContrastMapStyle, { name: highContrastMapName });
            }

            var state = this._getState();
            state.map = new google.maps.Map(state.mapContainer[0], googleMapOptions);

            if(highContrastMap) {
                state.map.mapTypes.set(highContrastMapName, highContrastMap);
            } else if(mapOptions.mapTypeId === VRS.MapType.HighContrast) {
                mapOptions.mapTypeId = VRS.MapType.RoadMap;
            }
            state.map.setMapTypeId(VRS.googleMapUtilities.toGoogleMapType(mapOptions.mapTypeId));

            if(mapOptions.mapControls && mapOptions.mapControls.length) {
                $.each(mapOptions.mapControls, function(idx, mapControl) {
                    self.addControl(mapControl.control, mapControl.position);
                });
            }

            this._hookEvents(state);

            var waitUntilReady = function() {
                if(self.options.waitUntilReady && !self.isReady()) {
                    setTimeout(waitUntilReady, 100);
                } else {
                    if(self.options.afterOpen) self.options.afterOpen(self);
                }
            };
            waitUntilReady();
        }

        /**
         * Hooks the events we care about on the Google map.
         */
        private _hookEvents(state: MapPluginState)
        {
            var self = this;
            var map = state.map;
            var hooks = state.nativeHooks;

            hooks.push(google.maps.event.addListener(map, 'bounds_changed',    function() { self._raiseBoundsChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'center_changed',    function() { self._raiseCenterChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'click',             function(mouseEvent: Event) { self._userNotIdle(); self._raiseClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'dblclick',          function(mouseEvent: Event) { self._userNotIdle(); self._raiseDoubleClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'idle',              function() { self._onIdle(); }));
            hooks.push(google.maps.event.addListener(map, 'maptypeid_changed', function() { self._userNotIdle(); self._onMapTypeChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'rightclick',        function(mouseEvent: Event) { self._userNotIdle(); self._raiseRightClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'tilesloaded',       function() { self._raiseTilesLoaded(); }));
            hooks.push(google.maps.event.addListener(map, 'zoom_changed',      function() { self._userNotIdle(); self._raiseZoomChanged(); }));
        }

        /**
         * Records the fact that the user did something.
         */
        private _userNotIdle()
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
        }

        /**
         * Refreshes the map, typically after it has been resized.
         */
        refreshMap()
        {
            var state = this._getState();
            if(state.map) google.maps.event.trigger(state.map, 'resize');
        }

        /**
         * Moves the map to a new map centre.
         */
        panTo(mapCenter: ILatLng)
        {
            this._panTo(mapCenter, this._getState());
        }
        private _panTo(mapCenter: ILatLng, state: MapPluginState)
        {
            if(state.map) state.map.panTo(VRS.googleMapUtilities.toGoogleLatLng(mapCenter));
            else          this.options.center = mapCenter;
        }

        /**
         * Moves the map so that the bounds specified are shown. This does nothing if the map has not been opened.
         */
        fitBounds(bounds: IBounds)
        {
            var state = this._getState();
            if(state.map) {
                state.map.fitBounds(VRS.googleMapUtilities.toGoogleLatLngBounds(bounds));
            }
        }


        //
        // STATE PERSISTENCE
        //


        /**
         * Saves the current state of the map.
         */
        saveState()
        {
            var settings = this._createSettings();
            VRS.configStorage.save(this._persistenceKey(), settings);
        }

        /**
         * Returns a previously saved state or returns the current state if no state was previously saved.
         */
        loadState() : IMapSaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            return $.extend(this._createSettings(), savedSettings);
        }

        /**
         * Applies a previously saved state.
         */
        applyState(config: IMapSaveState)
        {
            config = config || <IMapSaveState>{};
            var state = this._getState();

            if(config.center)                       this._setCenter(state, config.center);
            if(config.zoom || config.zoom === 0)    this._setZoom(state, config.zoom);
            if(config.mapTypeId)                    this._setMapType(state, config.mapTypeId);
        };

        /**
         * Loads and applies a previously saved state.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key against which the state will be saved.
         */
        private _persistenceKey() : string
        {
            return 'vrsMapState-' + (this.options.name || 'default');
        }

        /**
         * Returns the current state of the object.
         */
        private _createSettings() : IMapSaveState
        {
            var state = this._getState();
            var zoom = this._getZoom(state);
            var mapTypeId = this._getMapType(state);
            var center = this._getCenter(state);

            return {
                zoom:       zoom,
                mapTypeId:  mapTypeId,
                center:     center
            };
        }


        //
        // MAP MARKER METHODS
        //



        /**
         * Adds a marker to the map. Behaviour is undefined if the map has not been opened.
         */
        addMarker(id: string | number, userOptions: IMapMarkerSettings) : IMapMarker
        {
            var self = this;
            var result: MapMarker;

            var state = this._getState();
            if(state.map) {
                var googleOptions: MarkerWithLabelOptions = {
                    map:            state.map,
                    position:       undefined,
                    clickable:      userOptions.clickable !== undefined ? userOptions.clickable : true,
                    draggable:      userOptions.draggable !== undefined ? userOptions.draggable : false,
                    //flat:           userOptions.flat !== undefined ? userOptions.flat : false,                 //  Google Maps no longer supports shadows on map markers
                    optimized:      userOptions.optimized !== undefined ? userOptions.optimized : false,
                    raiseOnDrag:    userOptions.raiseOnDrag !== undefined ? userOptions.raiseOnDrag : true,
                    visible:        userOptions.visible !== undefined ? userOptions.visible : true
                };

                if(userOptions.animateAdd) googleOptions.animation = google.maps.Animation.DROP;
                if(userOptions.position)   googleOptions.position = VRS.googleMapUtilities.toGoogleLatLng(userOptions.position);
                else                       googleOptions.position = state.map.getCenter();
                if(userOptions.icon)       googleOptions.icon = VRS.googleMapUtilities.toGoogleIcon(userOptions.icon);
                if(userOptions.tooltip)    googleOptions.title = userOptions.tooltip;
                if(userOptions.zIndex || userOptions.zIndex === 0) googleOptions.zIndex = userOptions.zIndex;

                if(userOptions.useMarkerWithLabel) {
                    if(userOptions.mwlLabelInBackground !== undefined) googleOptions.labelInBackground = userOptions.mwlLabelInBackground;
                    if(userOptions.mwlLabelClass)                      googleOptions.labelClass = userOptions.mwlLabelClass;
                }

                this.destroyMarker(id);
                var marker: google.maps.Marker;
                if(!userOptions.useMarkerWithLabel) marker = new google.maps.Marker(googleOptions);
                else                                marker = new MarkerWithLabel(googleOptions);
                result = new MapMarker(id, marker, !!userOptions.useMarkerWithLabel, userOptions.tag);
                state.markers[id] = result;

                result.nativeListeners.push(google.maps.event.addListener(marker, 'click', function() { self._raiseMarkerClicked.call(self, id); }));
                result.nativeListeners.push(google.maps.event.addListener(marker, 'dragend', function() { self._raiseMarkerDragged.call(self, id); }));
            }

            return result;
        }

        /**
         * Gets a VRS.MapMarker by its ID or, if passed a marker, returns the same marker.
         */
        getMarker(idOrMarker: string | number | IMapMarker) : IMapMarker
        {
            if(idOrMarker instanceof MapMarker) return idOrMarker;
            var state = this._getState();
            return state.markers[<string | number>idOrMarker];
        }

        /**
         * Destroys the map marker passed across.
         */
        destroyMarker(idOrMarker: string | number | IMapMarker)
        {
            var state = this._getState();
            var marker = <MapMarker>this.getMarker(idOrMarker);
            if(marker) {
                $.each(marker.nativeListeners, function(idx, listener) {
                    google.maps.event.removeListener(listener);
                });
                marker.nativeListeners = [];
                marker.marker.setMap(null);
                marker.marker = null;
                marker.tag = null;
                delete state.markers[marker.id];
                marker.id = null;
            }
        }

        /**
         * Centres the map on the marker passed across.
         */
        centerOnMarker(idOrMarker: string | number | IMapMarker)
        {
            var state = this._getState();
            var marker = <MapMarker>this.getMarker(idOrMarker);
            if(marker) {
                state.map.setCenter(marker.marker.getPosition());
            }
        }


        //
        // MAP CLUSTERER METHODS
        //


        createMapMarkerClusterer(settings?: IMapMarkerClustererSettings) : IMapMarkerClusterer
        {
            var result: MapMarkerClusterer = null;

            if(typeof(MarkerClusterer) == 'function') {
                var state = this._getState();
                if(state.map) {
                    settings = $.extend({}, settings);
                    var clusterer = new MarkerClusterer(state.map, [], settings);
                    result = new MapMarkerClusterer(this, clusterer);
                }
            }

            return result;
        }


        //
        // POLYLINE METHODS
        //


        /**
         * Adds a line to the map. The behaviour is undefined if the map has not already been opened.
         */
        addPolyline(id: string | number, userOptions: IMapPolylineSettings) : IMapPolyline
        {
            var result: MapPolyline;

            var state = this._getState();
            if(state.map) {
                var googleOptions: google.maps.PolylineOptions = {
                    map:            state.map,
                    clickable:      userOptions.clickable !== undefined ? userOptions.clickable : false,
                    draggable:      userOptions.draggable !== undefined ? userOptions.draggable : false,
                    editable:       userOptions.editable !== undefined ? userOptions.editable : false,
                    geodesic:       userOptions.geodesic !== undefined ? userOptions.geodesic : false,
                    strokeColor:    userOptions.strokeColour || '#000000',
                    visible:        userOptions.visible !== undefined ? userOptions.visible : true
                };

                if(userOptions.path) googleOptions.path = VRS.googleMapUtilities.toGoogleLatLngMVCArray(userOptions.path);
                if(userOptions.strokeOpacity || userOptions.strokeOpacity === 0) googleOptions.strokeOpacity = userOptions.strokeOpacity;
                if(userOptions.strokeWeight || userOptions.strokeWeight === 0) googleOptions.strokeWeight = userOptions.strokeWeight;
                if(userOptions.zIndex || userOptions.zIndex === 0) googleOptions.zIndex = userOptions.zIndex;

                this.destroyPolyline(id);
                var polyline = new google.maps.Polyline(googleOptions);
                result = new MapPolyline(id, polyline, userOptions.tag, {
                    strokeColour:   userOptions.strokeColour,
                    strokeOpacity:  userOptions.strokeOpacity,
                    strokeWeight:   userOptions.strokeWeight
                });
                state.polylines[id] = result;
            }

            return result;
        }

        /**
         * Returns the VRS.MapPolyline associated with the ID.
         */
        getPolyline(idOrPolyline: string | number | IMapPolyline) : IMapPolyline
        {
            if(idOrPolyline instanceof MapPolyline) return idOrPolyline;
            var state = this._getState();
            return state.polylines[<string | number>idOrPolyline];
        }

        /**
         * Destroys the line passed across.
         */
        destroyPolyline(idOrPolyline: string | number | IMapPolyline)
        {
            var state = this._getState();
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            if(polyline) {
                polyline.polyline.setMap(null);
                polyline.polyline = null;
                polyline.tag = null;
                delete state.polylines[polyline.id];
                polyline.id = null;
            }
        }

        /**
         * Removes a number of points from the start or end of the line.
         */
        trimPolyline(idOrPolyline: string | number | IMapPolyline, countPoints: number, fromStart: boolean) : IMapTrimPolylineResult
        {
            var emptied = false;
            var countRemoved = 0;

            if(countPoints > 0) {
                var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getPath();
                var length = points.getLength();
                if(length < countPoints) countPoints = length;
                if(countPoints > 0) {
                    countRemoved = countPoints;
                    emptied = countPoints === length;
                    if(emptied) {
                        points.clear();
                    } else {
                        var end = length - 1;
                        for(;countPoints > 0;--countPoints) {
                            points.removeAt(fromStart ? 0 : end--);
                        }
                    }
                }
            }

            return { emptied: emptied, countRemoved: countRemoved };
        };

        /**
         * Remove a single point from the line's path.
         */
        removePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number)
        {
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getPath();
            points.removeAt(index);
        }

        /**
         * Appends points to a line's path.
         * @param {string|number|VRS.MapPolyline}   idOrPolyline    The line to change.
         * @param {{lat:number, lng:number}[]}      path            The points to add to the path.
         * @param {bool}                            toStart         True to add the points to the start of the path, false to add them to the end.
         */
        appendToPolyline(idOrPolyline: string | number | IMapPolyline, path: ILatLng[], toStart: boolean)
        {
            var length = !path ? 0 : path.length;
            if(length > 0) {
                var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getPath();
                var insertAt = toStart ? 0 : -1;
                for(var i = 0;i < length;++i) {
                    var googlePoint = VRS.googleMapUtilities.toGoogleLatLng(path[i]);
                    if(toStart) points.insertAt(insertAt++, googlePoint);
                    else points.push(googlePoint);
                }
            }
        }

        /**
         * Replaces an existing point along a line's path.
         */
        replacePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number, point: ILatLng)
        {
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getPath();
            var length = points.getLength();
            if(index === -1) index = length - 1;
            if(index >= 0 && index < length) points.setAt(index, VRS.googleMapUtilities.toGoogleLatLng(point));
        }


        //
        // POLYGON METHODS
        //


        /**
         * Adds a polygon to the map. The behaviour is undefined if the map has not already been opened.
         */
        addPolygon(id: string | number, userOptions: IMapPolygonSettings) : IMapPolygon
        {
            var result: MapPolygon;

            var state = this._getState();
            if(state.map) {
                var googleOptions: google.maps.PolygonOptions = {
                    map:            state.map,
                    clickable:      userOptions.clickable !== undefined ? userOptions.clickable : false,
                    draggable:      userOptions.draggable !== undefined ? userOptions.draggable : false,
                    editable:       userOptions.editable !== undefined ? userOptions.editable : false,
                    geodesic:       userOptions.geodesic !== undefined ? userOptions.geodesic : false,
                    fillColor:      userOptions.fillColour,
                    fillOpacity:    userOptions.fillOpacity,
                    paths:          <any[]>(<any>(VRS.googleMapUtilities.toGoogleLatLngMVCArrayArray(userOptions.paths) || undefined)),
                    strokeColor:    userOptions.strokeColour || '#000000',
                    strokeWeight:   userOptions.strokeWeight,
                    strokeOpacity:  userOptions.strokeOpacity,
                    visible:        userOptions.visible !== undefined ? userOptions.visible : true,
                    zIndex:         userOptions.zIndex
                };

                this.destroyPolygon(id);
                var polygon = new google.maps.Polygon(googleOptions);
                result = new MapPolygon(id, polygon, userOptions.tag, userOptions);
                state.polygons[id] = result;
            }

            return result;
        }

        /**
         * Returns the VRS.MapPolygon associated with the ID.
         */
        getPolygon(idOrPolygon: string | number | IMapPolygon) : IMapPolygon
        {
            if(idOrPolygon instanceof MapPolygon) return idOrPolygon;
            var state = this._getState();
            return state.polygons[<string | number>idOrPolygon];
        }

        /**
         * Destroys the polygon passed across.
         */
        destroyPolygon(idOrPolygon: string | number | IMapPolygon)
        {
            var state = this._getState();
            var polygon = <MapPolygon>this.getPolygon(idOrPolygon);
            if(polygon) {
                polygon.polygon.setMap(null);
                polygon.polygon = null;
                polygon.tag = null;
                delete state.polygons[polygon.id];
                polygon.id = null;
            }
        }


        //
        // CIRCLE METHODS
        //


        /**
         * Adds a circle to the map.
         */
        addCircle(id: string | number, userOptions: IMapCircleSettings) : IMapCircle
        {
            var result: MapCircle = null;

            var state = this._getState();
            if(state.map) {
                var googleOptions: google.maps.CircleOptions = $.extend({
                    clickable:      false,
                    draggable:      false,
                    editable:       false,
                    fillColor:      '#000',
                    fillOpacity:    0,
                    strokeColor:    '#000',
                    strokeOpacity:  1,
                    strokeWeight:   1,
                    visible:        true
                }, userOptions);
                googleOptions.center = VRS.googleMapUtilities.toGoogleLatLng(userOptions.center);
                googleOptions.map = state.map;

                this.destroyCircle(id);
                var circle = new google.maps.Circle(googleOptions);
                result = new MapCircle(id, circle, userOptions.tag, userOptions || {});
                state.circles[id] = result;
            }

            return result;
        }

        /**
         * Returns the VRS.MapCircle associated with the ID.
         */
        getCircle(idOrCircle: string | number | IMapCircle) : IMapCircle
        {
            if(idOrCircle instanceof MapCircle) return idOrCircle;
            var state = this._getState();
            return state.circles[<string | number>idOrCircle];
        }

        /**
         * Destroys the circle passed across.
         */
        destroyCircle(idOrCircle: string | number | IMapCircle)
        {
            var state = this._getState();
            var circle = <MapCircle>this.getCircle(idOrCircle);
            if(circle) {
                circle.circle.setMap(null);
                circle.circle = null;
                circle.tag = null;
                delete state.circles[circle.id];
                circle.id = null;
            }
        }


        //
        // INFOWINDOW METHODS
        //


        /**
         * Returns an Info Window ID that is guaranteed to not be in current use.
         */
        getUnusedInfoWindowId() : string
        {
            var result;

            var state = this._getState();
            for(var i = 1;i > 0;++i) {
                result = 'autoID' + i;
                if(!state.infoWindows[result]) break;
            }

            return result;
        }

        /**
         * Creates a new info window for the map.
         */
        addInfoWindow(id: string | number, userOptions: IMapInfoWindowSettings) : IMapInfoWindow
        {
            var result: MapInfoWindow = null;

            var state = this._getState();
            if(state.map) {
                var googleOptions: google.maps.InfoWindowOptions = $.extend({
                }, userOptions);
                if(userOptions.position) googleOptions.position = VRS.googleMapUtilities.toGoogleLatLng(userOptions.position);

                this.destroyInfoWindow(id);
                var infoWindow = new google.maps.InfoWindow(googleOptions);
                result = new MapInfoWindow(id, infoWindow, userOptions.tag, userOptions || {});
                state.infoWindows[id] = result;

                var self = this;
                result.nativeListeners.push(google.maps.event.addListener(infoWindow, 'closeclick', function() {
                    result.isOpen = false;
                    self._raiseInfoWindowClosedByUser(id);
                }));
            }

            return result;
        }

        /**
         * If passed the ID of an info window then the associated info window is returned. If passed an info window
         * then it is just returned.
         */
        getInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow) : IMapInfoWindow
        {
            if(idOrInfoWindow instanceof MapInfoWindow) return idOrInfoWindow;
            var state = this._getState();
            return state.infoWindows[<string | number>idOrInfoWindow];
        }

        /**
         * Destroys the info window passed across. Note that Google do not supply a method to dispose of an info window.
         */
        destroyInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow)
        {
            var state = this._getState();
            var infoWindow = <MapInfoWindow>this.getInfoWindow(idOrInfoWindow);
            if(infoWindow) {
                $.each(infoWindow.nativeListeners, function(idx, listener) {
                    google.maps.event.removeListener(listener);
                });
                this.closeInfoWindow(infoWindow);
                infoWindow.infoWindow.setContent('');
                infoWindow.tag = null;
                infoWindow.infoWindow = null;
                delete state.infoWindows[infoWindow.id];
                infoWindow.id = null;
            }
        }

        /**
         * Opens the info window at the position specified on the window, optionally with an anchor to specify the
         * location of the tip of the info window. Does nothing if it's already open.
         */
        openInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow, mapMarker?: IMapMarker)
        {
            var state = this._getState();
            var infoWindow = <MapInfoWindow>this.getInfoWindow(idOrInfoWindow);
            if(infoWindow && state.map && !infoWindow.isOpen) {
                infoWindow.infoWindow.open(state.map, mapMarker ? (<MapMarker>mapMarker).marker : undefined);
                infoWindow.isOpen = true;
            }
        }

        /**
         * Closes an info window if it's open. Does nothing if it's already closed.
         */
        closeInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow)
        {
            var infoWindow = <MapInfoWindow>this.getInfoWindow(idOrInfoWindow);
            if(infoWindow && infoWindow.isOpen) {
                infoWindow.infoWindow.close();
                infoWindow.isOpen = false;
            }
        }


        //
        // MAP CONTROL METHODS
        //


        /**
         * Adds a control to the map.
         */
        addControl(element: JQuery | HTMLElement, mapPosition: MapPositionEnum)
        {
            var state = this._getState();
            if(state.map) {
                var controlsArray = state.map.controls[VRS.googleMapUtilities.toGoogleControlPosition(mapPosition)];
                if(!(element instanceof jQuery)) controlsArray.push(element);
                else $.each(element, function() { controlsArray.push(this); });
            }
        }


        //
        // MAP LAYER METHODS
        //
        // The Google Maps version of the plugin does not support layers.

        addLayer(layerTileSettings: ITileServerSettings, opacity: number)
        {
        }

        destroyLayer(layerName: string)
        {
        }

        hasLayer(layerName: string) : boolean
        {
            return false;
        }

        getLayerOpacity(layerName: string) : number
        {
            return undefined;
        }

        setLayerOpacity(layerName: string, opacity: number)
        {
        }

        
        //
        // VRS EVENTS SUBSCRIBED
        //


        /**
         * Called when the refresh manager indicates that one of our parents has resized, or done something that we need
         * to refresh for.
         */
        private _targetResized()
        {
            var state = this._getState();

            var center = this._getCenter(state);
            this.refreshMap();
            this._setCenter(state, center);
        }
    }

    $.widget('vrs.vrsMap', new MapPlugin());
}

declare interface JQuery
{
    vrsMap();
    vrsMap(options: VRS.IMapOptions);
    vrsMap(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
