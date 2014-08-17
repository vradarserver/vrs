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
 * @fileoverview A jQuery UI plugin that wraps maps.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    /** @type {VRS_GLOBAL_OPTIONS} */
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapGoogleMapHttpUrl = VRS.globalOptions.mapGoogleMapHttpUrl || 'http://maps.google.com/maps/api/js';            // The HTTP URL for Google Maps
    VRS.globalOptions.mapGoogleMapHttpsUrl = VRS.globalOptions.mapGoogleMapHttpsUrl || 'https://maps.google.com/maps/api/js';         // The HTTPS URL for Google Maps
    VRS.globalOptions.mapGoogleMapTimeout = VRS.globalOptions.mapGoogleMapTimeout || 5000;                                            // The number of milliseconds to wait before giving up and assuming that the maps aren't going to load.
    VRS.globalOptions.mapGoogleMapUseHttps = VRS.globalOptions.mapGoogleMapUseHttps !== undefined ? VRS.globalOptions.mapGoogleMapUseHttps : true;  // True to load the HTTPS version, false to load the HTTP. Note that Chrome on iOS fails if it's not HTTPS!
    VRS.globalOptions.mapShowStreetView = VRS.globalOptions.mapShowStreetView !== undefined ? VRS.globalOptions.mapShowStreetView : false;              // True if the StreetView control is to be shown on Google Maps.
    VRS.globalOptions.mapScrollWheelActive = VRS.globalOptions.mapScrollWheelActive !== undefined ? VRS.globalOptions.mapScrollWheelActive : true;      // True if the scroll wheel zooms the map.
    VRS.globalOptions.mapDraggable = VRS.globalOptions.mapDraggable !== undefined ? VRS.globalOptions.mapDraggable : true;                              // True if the user can move the map.
    VRS.globalOptions.mapShowPointsOfInterest = VRS.globalOptions.mapShowPointsOfInterest !== undefined ? VRS.globalOptions.mapShowPointsOfInterest : false;    // True if points of interest are to be shown on Google Maps.
    VRS.globalOptions.mapShowScaleControl = VRS.globalOptions.mapShowScaleControl !== undefined ? VRS.globalOptions.mapShowScaleControl : true;         // True if the map should display a scale on it.
    VRS.globalOptions.mapShowHighContrastStyle = VRS.globalOptions.mapShowHighContrastStyle !== undefined ? VRS.globalOptions.mapShowHighContrastStyle : true;  // True if the high-contrast map style is to be shown.
    VRS.globalOptions.mapHighContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle !== undefined ? VRS.globalOptions.mapHighContrastMapStyle : [ // The Google map styles to use for the high contrast map.
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
    //endregion

    //region GoogleMapUtilities
    /**
     * An object that can convert to and from VRS map objects and Google map object.
     * @constructor
     */
    VRS.GoogleMapUtilities = function()
    {
        //region -- Fields
        /**
         * The ID *and* the label of the high contrast map type. We cannot refer to the VRS.$$ identifier directly when
         * translating map types because we need to use the one that Google knows about, not the one for the currently
         * selected language. The first time the map type gets translated this gets filled in with VRS.$$.HighContrastMap.
         * @type {string}
         * @private
         */
        var _HighContrastMapTypeName = null;
        //endregion

        //region -- LatLng conversion
        /**
         * Converts from a Google latLng object to a VRS latLng object.
         * @param {google.maps.LatLng} latLng A Google latLng object.
         * @returns {VRS_LAT_LNG}
         */
        this.fromGoogleLatLng = function(latLng) { return latLng ? { lat: latLng.lat(), lng: latLng.lng() } : latLng; };

        /**
         * Converts from a VRS latLng to a Google latLng.
         * @param {VRS_LAT_LNG} latLng
         * @returns {google.maps.LatLng}
         */
        this.toGoogleLatLng = function(latLng)   { return latLng ? new google.maps.LatLng(latLng.lat, latLng.lng) : latLng; };
        //endregion

        //region -- Point conversion
        /**
         * Converts from a Google point to a VRS point.
         * @param {google.maps.Point} point
         * @returns {VRS_POINT}
         */
        this.fromGooglePoint = function(point)   { return point ? { x: point.x, y: point.y } : point; };

        /**
         * Converts from a VRS point to a Google point
         * @param {VRS_POINT} point
         * @returns {google.maps.Point}
         */
        this.toGooglePoint = function(point)     { return point ? new google.maps.Point(point.x, point.y) : point; };
        //endregion

        //region -- Size conversion
        /**
         * Converts from a Google Size to a VRS size.
         * @param {google.maps.Size} size
         * @returns {VRS_SIZE}
         */
        this.fromGoogleSize = function(size)     { return size ? { width: size.width, height: size.height } : size; };

        /**
         * Converts from a VRS size to a Google size.
         * @param {VRS_SIZE} size
         * @returns {google.maps.Size}
         */
        this.toGoogleSize = function(size)       { return size ? new google.maps.Size(size.width, size.height) : size; };
        //endregion

        //region -- LatLngBounds conversion
        /**
         * Converts from a Google latLngBounds to a VRS bounds.
         * @param {google.maps.LatLngBounds} latLngBounds
         * @returns {VRS_BOUNDS}
         */
        this.fromGoogleLatLngBounds = function(latLngBounds)
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
        };

        //noinspection JSUnusedGlobalSymbols
        /**
         * Converts from a VRS bounds to a Google LatLngBounds.
         * @param {VRS_BOUNDS} bounds
         * @returns {google.maps.LatLngBounds}
         */
        this.toGoogleLatLngBounds = function(bounds)
        {
            return bounds ? new google.maps.LatLngBounds(
                new google.maps.LatLng(bounds.brLat, bounds.tlLng),
                new google.maps.LatLng(bounds.tlLat, bounds.brLng)
            ) : bounds;
        };
        //endregion

        //region -- MapControlStyle conversion
        /**
         * Converts from a Google map control style to a VRS one.
         * @param {google.maps.MapTypeControlStyle} mapControlStyle
         * @returns {VRS.MapControlStyle}
         */
        this.fromGoogleMapControlStyle = function(mapControlStyle)
        {
            if(!mapControlStyle) return null;
            switch(mapControlStyle) {
                case google.maps.MapTypeControlStyle.DEFAULT:           return VRS.MapControlStyle.Default;
                case google.maps.MapTypeControlStyle.DROPDOWN_MENU:     return VRS.MapControlStyle.DropdownMenu;
                case google.maps.MapTypeControlStyle.HORIZONTAL_BAR:    return VRS.MapControlStyle.HorizontalBar;
                default:                                                throw 'Not implemented';
            }
        };

        /**
         * Converts from a VRS map type control style to a Google one.
         * @param {VRS.MapControlStyle} mapControlStyle
         * @returns {google.maps.MapTypeControlStyle}
         */
        this.toGoogleMapControlStyle = function(mapControlStyle)
        {
            if(!mapControlStyle) return null;
            switch(mapControlStyle) {
                case VRS.MapControlStyle.Default:           return google.maps.MapTypeControlStyle.DEFAULT;
                case VRS.MapControlStyle.DropdownMenu:      return google.maps.MapTypeControlStyle.DROPDOWN_MENU;
                case VRS.MapControlStyle.HorizontalBar:     return google.maps.MapTypeControlStyle.HORIZONTAL_BAR;
                default:                                    throw 'Not implemented';
            }
        };
        //endregion

        //region -- MapType conversion
        /**
         * Converts from a Google map type to a VRS map type.
         * @param {google.maps.MapTypeId|string} mapType
         * @returns {VRS.MapType} A VRS.MapType string.
         */
        this.fromGoogleMapType = function(mapType)
        {
            if(!mapType) return null;
            if(!_HighContrastMapTypeName) _HighContrastMapTypeName = VRS.$$.HighContrastMap;
            switch(mapType) {
                case google.maps.MapTypeId.HYBRID:      return VRS.MapType.Hybrid;
                case google.maps.MapTypeId.ROADMAP:     return VRS.MapType.RoadMap;
                case google.maps.MapTypeId.SATELLITE:   return VRS.MapType.Satellite;
                case google.maps.MapTypeId.TERRAIN:     return VRS.MapType.Terrain;
                case _HighContrastMapTypeName:          return VRS.MapType.HighContrast;
                default:                                throw 'Not implemented';
            }
        };

        /**
         * Converts from a VRS map type to a Google map type.
         * @param {VRS.MapType} mapType
         * @param {bool} [suppressException]    If supplied and true then null is returned when a bad map type is passed across rather than having an exception thrown.
         * @returns {google.maps.MapTypeId|string}
         */
        this.toGoogleMapType = function(mapType, suppressException)
        {
            if(!mapType) return null;
            if(!_HighContrastMapTypeName) _HighContrastMapTypeName = VRS.$$.HighContrastMap;
            switch(mapType) {
                case VRS.MapType.Hybrid:        return google.maps.MapTypeId.HYBRID;
                case VRS.MapType.RoadMap:       return google.maps.MapTypeId.ROADMAP;
                case VRS.MapType.Satellite:     return google.maps.MapTypeId.SATELLITE;
                case VRS.MapType.Terrain:       return google.maps.MapTypeId.TERRAIN;
                case VRS.MapType.HighContrast:  return _HighContrastMapTypeName;
                default:
                    if(suppressException) return null;
                    throw 'Not implemented';
            }
        };
        //endregion

        //region -- Icon conversion
        /**
         * Converts from a Google icon to a VRS map icon.
         * @param {VRS_GOOGLE_ICON} icon
         * @returns {VRS.MapIcon}
         */
        this.fromGoogleIcon = function(icon)
        {
            if(!icon) return null;
            else return new VRS.MapIcon(
                icon.url,
                this.fromGoogleSize(icon.size),
                this.fromGooglePoint(icon.anchor),
                this.fromGooglePoint(icon.origin),
                this.fromGoogleSize(icon.scaledSize)
            );
        };

        /**
         * Converts from a VRS map icon to a Google icon.
         * @param {VRS.MapIcon} icon
         * @returns {VRS_GOOGLE_ICON}
         */
        this.toGoogleIcon = function(icon)
        {
            if(!icon) return null;
            var result = /** @type {VRS_GOOGLE_ICON } */ { };
            if(icon.anchor)     result.anchor = this.toGooglePoint(icon.anchor);
            if(icon.origin)     result.origin = this.toGooglePoint(icon.origin);
            if(icon.scaledSize) result.scaledSize = this.toGoogleSize(icon.scaledSize);
            if(icon.size)       result.size = this.toGoogleSize(icon.size);
            if(icon.url)        result.url = icon.url;

            return result;
        };
        //endregion

        //region -- LatLngMVCArray conversion
        //noinspection JSUnusedGlobalSymbols
        /**
         * Converts from a Google LatLngMVCArray to an VRS array of latLng objects.
         * @param {google.maps.MVCArray.<google.maps.LatLng>} latLngMVCArray
         * @returns {VRS_LAT_LNG[]}
         */
        this.fromGoogleLatLngMVCArray = function(latLngMVCArray)
        {
            if(!latLngMVCArray) return null;
            var result = [];
            var length = latLngMVCArray.getLength();
            for(var i = 0;i < length;++i) {
                result.push(this.fromGoogleLatLng(latLngMVCArray.getAt(i)));
            }
            return result;
        };

        /**
         * Converts from a VRS array of latLng objects to a Google LatLngMVCArray.
         * @param {VRS_LAT_LNG[]} latLngArray
         * @returns {google.maps.MVCArray.<google.maps.LatLng>}
         */
        this.toGoogleLatLngMVCArray = function(latLngArray)
        {
            if(!latLngArray) return null;
            var googleLatLngArray = [];
            var length = latLngArray.length;
            for(var i = 0;i < length;++i) {
                googleLatLngArray.push(this.toGoogleLatLng(latLngArray[i]));
            }
            return new google.maps.MVCArray(googleLatLngArray);
        };

        /**
         * Converts from a Google MVCArray of Google LatLngMVCArrays to an array of
         * an array of VRS latLng objects.
         * @param {google.maps.MVCArray.<google.maps.MVCArray.<google.maps.latLng>>} latLngMVCArrayArray
         * @returns {VRS_LAT_LNG[][]}
         */
        this.fromGoogleLatLngMVCArrayArray = function(latLngMVCArrayArray)
        {
            if(!latLngMVCArrayArray) return null;
            var result = [];
            var length = latLngMVCArrayArray.getLength();
            for(var i = 0;i < length;++i) {
                result.push(this.fromGoogleLatLngMVCArray(latLngMVCArrayArray[i]));
            }
            return result;
        };

        /**
         * Converts from an array of an array of VRS_LAT_LNG objects to a Google MVCArray
         * of a Google MVCArray of Google latLng objects.
         * @param {VRS_LAT_LNG[][]} latLngArrayArray
         * @returns {google.maps.MVCArray.<google.maps.MVCArray.<google.maps.latLng>>}
         */
        this.toGoogleLatLngMVCArrayArray = function(latLngArrayArray)
        {
            if(!latLngArrayArray) return null;
            var result = [];
            var length = latLngArrayArray.length;
            for(var i = 0;i < length;++i) {
                result.push(this.toGoogleLatLngMVCArray(latLngArrayArray[i]));
            }
            return new google.maps.MVCArray(result);
        };
        //endregion

        //region -- MapPosition conversion
        //noinspection JSUnusedGlobalSymbols
        /**
         * Converts from a Google control position to a VRS.MapPosition.
         * @param {google.maps.ControlPosition} controlPosition
         * @returns {VRS.MapPosition}
         */
        this.fromGoogleControlPosition = function(controlPosition)
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
        };

        /**
         * Converts from a VRS.MapPosition to a Google control position.
         * @param {VRS.MapPosition} mapPosition
         * @returns {google.maps.ControlPosition}
         */
        this.toGoogleControlPosition = function(mapPosition)
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
        };
        //endregion
    };

    VRS.googleMapUtilities = new VRS.GoogleMapUtilities();
    //endregion

    //region MapIcon
    /**
     * Describes an icon drawn onto the map.
     * @param {string}      url             The URL of the image.
     * @param {VRS_SIZE}    size            The size of the image.
     * @param {VRS_POINT}   anchor          The anchor point for the image.
     * @param {VRS_POINT}   origin          The origin point for the image.
     * @param {VRS_SIZE}   [scaledSize]     The scaled size for the image.
     * @param {VRS_POINT}  [labelAnchor]    The anchor for the label. This is not applied to the Google icon, it's just here so that it's carried with the icon.
     * @constructor
     */
    VRS.MapIcon = function(url, size, anchor, origin, scaledSize, labelAnchor)
    {
        this.anchor = anchor;
        this.origin = origin;
        this.scaledSize = scaledSize;
        this.size = size;
        this.url = url;
        this.labelAnchor = labelAnchor;
    };
    //endregion

    //region MapMarker
    /**
     * An abstracted wrapper around an object that represents a map's native marker.
     * @param {string|number}   id                  The identifier of the marker
     * @param {*}               nativeMarker        The native map marker handle to wrap.
     * @param {bool}            isMarkerWithLabel   Indicates that the native marker is a Google Maps MarkerWithLabel.
     * @param {*}               tag                 An object to carry around with the marker. No meaning is attached to the tag.
     * @constructor
     */
    VRS.MapMarker = function(id, nativeMarker, isMarkerWithLabel, tag)
    {
        /**
         * The identifier of the marker. Left as a field to speed things up a bit.
         * @type {string|number}
         */
        this.id = id;

        /**
         * The native marker object. Leave this alone.
         * @type {*}
         */
        this.marker = nativeMarker;

        /**
         * True if the native marker is a Google Maps MarkerWithLabel.
         * @type {boolean}
         */
        this.isMarkerWithLabel = isMarkerWithLabel;

        /**
         * The object that the marker has been tagged with. Not used by the plugin.
         * @type {*}
         */
        this.tag = tag;

        /**
         * An array of objects describing the events that have been hooked on the marker. Leave these alone.
         * @type {Array}
         */
        this.nativeListeners = [];

        /**
         * Returns true if the marker can be dragged.
         * @returns {boolean}
         */
        this.getDraggable = function()          { return this.marker.getDraggable(); };
        /**
         * Sets a value indicating whether the marker can be dragged.
         * @param {bool} draggable
         */
        this.setDraggable = function(draggable) { this.marker.setDraggable(draggable); };

        /**
         * Returns the icon for the marker.
         * @returns {VRS.MapIcon}
         */
        this.getIcon = function()               { return VRS.googleMapUtilities.fromGoogleIcon(/** @type {VRS_GOOGLE_ICON} */this.marker.getIcon()); };
        /**
         * Sets the icon for the marker.
         * @param {VRS.MapIcon} icon
         */
        this.setIcon = function(icon)           { this.marker.setIcon(VRS.googleMapUtilities.toGoogleIcon(icon)); };

        /**
         * Gets the coordinates of the marker.
         * @returns {{lat: number, lng: number}}
         */
        this.getPosition = function()           { return VRS.googleMapUtilities.fromGoogleLatLng(this.marker.getPosition()); };
        /**
         * Sets the coordinates for the marker.
         * @param {{lat: number, lng: number}} position
         */
        this.setPosition = function(position)   { this.marker.setPosition(VRS.googleMapUtilities.toGoogleLatLng(position)); };

        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the tooltip for the marker.
         * @returns {string}
         */
        this.getTooltip = function()            { return this.marker.getTitle(); };
        /**
         * Sets the tooltip for the marker.
         * @param {string} tooltip
         */
        this.setTooltip = function(tooltip)     { this.marker.setTitle(tooltip); };

        /**
         * Gets a value indicating that the marker is visible.
         * @returns {boolean}
         */
        this.getVisible = function()            { return this.marker.getVisible(); };
        /**
         * Sets a value indicating whether the marker is visible.
         * @param {boolean} visible
         */
        this.setVisible = function(visible)     { this.marker.setVisible(visible); };

        /**
         * Gets the z-index of the marker.
         * @returns {number}
         */
        this.getZIndex = function()             { return this.marker.getZIndex(); };
        /**
         * Sets the z-index of the marker.
         * @param {number} zIndex
         */
        this.setZIndex = function(zIndex)       { this.marker.setZIndex(zIndex); };

        /**
         * Returns true if the marker was created with useMarkerWithLabel and the label is visible.
         * @returns {bool}
         */
        this.getLabelVisible = function()
        {
            return this.isMarkerWithLabel ? this.marker.get('labelVisible') : false;
        };

        /**
         * Sets the visibility of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * @param {bool} visible
         */
        this.setLabelVisible = function(visible)
        {
            if(this.isMarkerWithLabel) this.marker.set('labelVisible', visible);
        };

        /**
         * Sets the content of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * @param {*} content
         */
        this.setLabelContent = function(content)
        {
            if(this.isMarkerWithLabel) this.marker.set('labelContent', content);
        };

        /**
         * Sets the anchor for a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * @param {VRS_POINT} anchor
         */
        this.setLabelAnchor = function(anchor)
        {
            if(this.isMarkerWithLabel) this.marker.set('labelAnchor', VRS.googleMapUtilities.toGooglePoint(anchor));
        }
    };
    //endregion

    //region MapPolygon
    /**
     * Describes a polygon on a map.
     * @param {number}              id
     * @param {google.maps.Polygon} nativePolygon
     * @param {*}                   tag
     * @param {VRS_OPTIONS_POLYGON} options
     * @constructor
     */
    VRS.MapPolygon = function(id, nativePolygon, tag, options)
    {
        this.id = id;
        this.polygon = nativePolygon;
        this.tag = tag;

        /**
         * Gets a value indicating that the polygon is draggable.
         * @returns {boolean}
         */
        this.getDraggable = function()          { return this.polygon.getDraggable(); };
        /**
         * Sets a value indicating whether the polygon is draggable.
         * @param {bool} draggable
         */
        this.setDraggable = function(draggable) { this.polygon.setDraggable(draggable); };

        /**
         * Gets a value indicating whether the polygon can be changed by the user.
         * @returns {boolean}
         */
        this.getEditable = function()           { return this.polygon.getEditable(); };
        /**
         * Sets a value indicating whether the user can change the polygon.
         * @param {bool} editable
         */
        this.setEditable = function(editable)   { this.polygon.setEditable(editable); };

        /**
         * Gets a value indicating that the polygon is visible.
         * @returns {boolean}
         */
        this.getVisible = function()            { return this.polygon.getVisible(); };
        /**
         * Sets a value indicating whether the polygon is visible.
         * @param {bool} visible
         */
        this.setVisible = function(visible)     { this.polygon.setVisible(visible); };

        /**
         * Gets the first path.
         * @returns {VRS_LAT_LNG[]}
         */
        this.getFirstPath = function()          { return VRS.googleMapUtilities.fromGoogleLatLngMVCArray(this.polygon.getPath()); };
        /**
         * Sets the first path.
         * @param {VRS_LAT_LNG[]} path
         */
        this.setFirstPath = function(path)      { this.polygon.setPath(VRS.googleMapUtilities.toGoogleLatLngMVCArray(path)); };

        /**
         * Gets an array of every path array.
         * @returns {VRS_LAT_LNG[][]}
         */
        this.getPaths = function()              { return VRS.googleMapUtilities.fromGoogleLatLngMVCArrayArray(this.polygon.getPaths()); };
        /**
         * Sets an array of every path array.
         * @param {VRS_LAT_LNG[][]} paths
         */
        this.setPaths = function(paths)         { this.polygon.setPaths(VRS.googleMapUtilities.toGoogleLatLngMVCArrayArray(paths)); };

        var _Clickable = options.clickable;
        /**
         * Gets a value indicating whether the polygon handles mouse events.
         * @returns {boolean}
         */
        this.getClickable = function()          { return _Clickable; }
        /**
         * Sets a value that indicates whether the polygon handles mouse events.
         * @param {boolean} value
         */
        this.setClickable = function(value)
        {
            if(value !== _Clickable) {
                _Clickable = value;
                this.polygon.setOptions({ clickable: value });
            }
        };

        var _FillColour = options.fillColour;
        /**
         * Gets the CSS colour of the fill area.
         * @returns {string=}
         */
        this.getFillColour = function()       { return _FillColour; };
        /**
         * Sets the CSS colour of the fill area.
         * @param {string=} value
         */
        this.setFillColour = function(value) {
            if(value !== _FillColour) {
                _FillColour = value;
                this.polygon.setOptions({ fillColor: value });
            }
        };

        var _FillOpacity = options.fillOpacity;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the opacity of the fill area.
         * @returns {number=}
         */
        this.getFillOpacity = function()       { return _FillOpacity; };
        //noinspection JSUnusedGlobalSymbols
        /**
         * Sets the opacity of the fill area (between 0 and 1).
         * @param {number=} value
         */
        this.setFillOpacity = function(value) {
            if(value !== _FillOpacity) {
                _FillOpacity = value;
                this.polygon.setOptions({ fillOpacity: value });
            }
        };

        var _StrokeColour = options.strokeColour;
        /**
         * Gets the CSS colour of the stroke line.
         * @returns {string=}
         */
        this.getStrokeColour = function()       { return _StrokeColour; };
        /**
         * Sets the CSS colour of the stroke line.
         * @param {string=} value
         */
        this.setStrokeColour = function(value) {
            if(value !== _StrokeColour) {
                _StrokeColour = value;
                this.polygon.setOptions({ strokeColor: value });
            }
        };

        var _StrokeOpacity = options.strokeOpacity;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the opacity of the stroke line.
         * @returns {number=}
         */
        this.getStrokeOpacity = function()       { return _StrokeOpacity; };
        //noinspection JSUnusedGlobalSymbols
        /**
         * Sets the opacity of the stroke line (between 0 and 1).
         * @param {number=} value
         */
        this.setStrokeOpacity = function(value) {
            if(value !== _StrokeOpacity) {
                _StrokeOpacity = value;
                this.polygon.setOptions({ strokeOpacity: value });
            }
        };

        var _StrokeWeight = options.strokeWeight;
        /**
         * Gets the weight of the stroke line in pixels.
         * @returns {number=}
         */
        this.getStrokeWeight = function()       { return _StrokeWeight; };
        /**
         * Sets the weight of the stroke line in pixels.
         * @param {number=} value
         */
        this.setStrokeWeight = function(value) {
            if(value !== _StrokeWeight) {
                _StrokeWeight = value;
                this.polygon.setOptions({ strokeWeight: value });
            }
        };

        var _ZIndex = options.zIndex;
        /**
         * Gets the z-index of the polygon.
         * @returns {number=}
         */
        this.getZIndex = function() { return _ZIndex; };
        /**
         * Sets the weight of the stroke line in pixels.
         * @param {number=} value
         */
        this.setZIndex = function(value) {
            if(value !== _ZIndex) {
                _ZIndex = value;
                this.polygon.setOptions({ zIndex: value });
            }
        };
    };
    //endregion

    //region MapPolyline
    /**
     * An object that wraps a map's native polyline object.
     * @param {string|number}           id                  The unique identifier of the polyline object.
     * @param {*}                       nativePolyline      The native object that is being wrapped.
     * @param {*}                       tag                 An object attached to the polyline.
     * @param {VRS_OPTIONS_POLYLINE}    options             The CSS colour, opacity (0-1) and weight (pixel width) of the line.
     * @constructor
     */
    VRS.MapPolyline = function(id, nativePolyline, tag, options)
    {
        this.id = id;
        this.polyline = nativePolyline;
        this.tag = tag;

        /**
         * Gets a value indicating that the line is draggable.
         * @returns {boolean}
         */
        this.getDraggable = function()          { return this.polyline.getDraggable(); };
        /**
         * Sets a value indicating whether the line is draggable.
         * @param {bool} draggable
         */
        this.setDraggable = function(draggable) { this.polyline.setDraggable(draggable); };

        /**
         * Gets a value indicating whether the line can be changed by the user.
         * @returns {boolean}
         */
        this.getEditable = function()           { return this.polyline.getEditable(); };
        /**
         * Sets a value indicating whether the user can change the line.
         * @param {bool} editable
         */
        this.setEditable = function(editable)   { this.polyline.setEditable(editable); };

        /**
         * Gets a value indicating whether the line is visible.
         * @returns {boolean}
         */
        this.getVisible = function()            { return this.polyline.getVisible(); };
        /**
         * Sets a value indicating whether the line is visible.
         * @param {bool} visible
         */
        this.setVisible = function(visible)     { this.polyline.setVisible(visible); };

        var _StrokeColour = options.strokeColour;
        /**
         * Gets the CSS colour of the line.
         * @returns {string=}
         */
        this.getStrokeColour = function()       { return _StrokeColour; };
        /**
         * Sets the CSS colour of the line.
         * @param {string=} value
         */
        this.setStrokeColour = function(value) {
            if(value !== _StrokeColour) {
                _StrokeColour = value;
                this.polyline.setOptions({ strokeColor: value });
            }
        };

        var _StrokeOpacity = options.strokeOpacity;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Gets the opacity of the line.
         * @returns {number=}
         */
        this.getStrokeOpacity = function()       { return _StrokeOpacity; };
        //noinspection JSUnusedGlobalSymbols
        /**
         * Sets the opacity of the line (between 0 and 1).
         * @param {number=} value
         */
        this.setStrokeOpacity = function(value) {
            if(value !== _StrokeOpacity) {
                _StrokeOpacity = value;
                this.polyline.setOptions({ strokeOpacity: value });
            }
        };

        var _StrokeWeight = options.strokeWeight;
        /**
         * Gets the weight of the line in pixels.
         * @returns {number=}
         */
        this.getStrokeWeight = function()       { return _StrokeWeight; };
        /**
         * Sets the weight of the line in pixels.
         * @param {number=} value
         */
        this.setStrokeWeight = function(value) {
            if(value !== _StrokeWeight) {
                _StrokeWeight = value;
                this.polyline.setOptions({ strokeWeight: value });
            }
        };

        /**
         * Returns the path for the polyline.
         * @returns {Array.<VRS_LAT_LNG>}
         */
        this.getPath = function() {
            var result = [];
            var nativePath = this.polyline.getPath();
            var length = nativePath.getLength();
            for(var i = 0;i < length;++i) {
                result.push(VRS.googleMapUtilities.fromGoogleLatLng(nativePath.getAt(i)));
            }
            return result;
        };

        /**
         * Returns the first point on the path or null if the path is empty.
         * @returns {VRS_LAT_LNG}
         */
        this.getFirstLatLng = function() {
            var result = null;
            var nativePath = this.polyline.getPath();
            if(nativePath.getLength()) result = VRS.googleMapUtilities.fromGoogleLatLng(nativePath.getAt(0));

            return result;
        };

        /**
         * Returns the last point on the path or null if the path is empty.
         * @returns {VRS_LAT_LNG}
         */
        this.getLastLatLng = function() {
            var result = null;
            var nativePath = this.polyline.getPath();
            var length = nativePath.getLength();
            if(length) result = VRS.googleMapUtilities.fromGoogleLatLng(nativePath.getAt(length - 1));

            return result;
        };
    };
    //endregion

    //region MapCircle
    /**
     * An object that wraps a map's native circle object.
     * @param {string|number}           id                  The unique identifier of the circle object.
     * @param {*}                       nativeCircle        The native object that is being wrapped.
     * @param {*}                       tag                 An object attached to the circle.
     * @param {VRS_OPTIONS_CIRCLE}      options             The options used when the circle was created.
     * @constructor
     */
    VRS.MapCircle = function(id, nativeCircle, tag, options)
    {
        this.id = id;
        this.circle = nativeCircle;
        this.tag = tag;

        this.getBounds = function() { return VRS.googleMapUtilities.fromGoogleLatLngBounds(this.circle.getBounds()); };
        this.setBounds = function(/** VRS_BOUNDS */ value) { this.circle.setBounds(VRS.googleMapUtilities.toGoogleLatLngBounds(value)); };

        this.getCenter = function() { return VRS.googleMapUtilities.fromGoogleLatLng(this.circle.getCenter()); };
        this.setCenter = function(/** VRS_LAT_LNG */ value) { this.circle.setCenter(VRS.googleMapUtilities.toGoogleLatLng(value)); };

        this.getDraggable = function() { return this.circle.getDraggable(); };
        this.setDraggable = function(/** boolean */ value) { this.circle.setDraggable(value); };

        this.getEditable = function() { return this.circle.getEditable(); };
        this.setEditable = function(/** boolean */ value) { this.circle.setEditable(value); };

        this.getRadius = function() { return this.circle.getRadius(); };
        this.setRadius = function(/** Number */ value) { this.circle.setRadius(value); };

        this.getVisible = function() { return this.circle.getVisible(); };
        this.setVisible = function(/** boolean */ value) { this.circle.setVisible(value); };

        var _FillColour = options.fillColor;
        this.getFillColor = function() { return _FillColour; };
        this.setFillColor = function(/** string */ value) {
            if(_FillColour !== value) {
                _FillColour = value;
                this.circle.setOptions({ fillColor: value });
            }
        };

        var _FillOpacity = options.fillOpacity;
        this.getFillOpacity = function() { return _FillOpacity; };
        this.setFillOpacity = function(/** Number */ value) {
            if(_FillOpacity !== value) {
                _FillOpacity = value;
                this.circle.setOptions({ fillOpacity: value });
            }
        };

        var _StrokeColour = options.strokeColor;
        this.getStrokeColor = function() { return _StrokeColour; };
        this.setStrokeColor = function(/** string */ value) {
            if(_StrokeColour !== value) {
                _StrokeColour = value;
                this.circle.setOptions({ strokeColor: value });
            }
        };

        var _StrokeOpacity = options.strokeOpacity;
        this.getStrokeOpacity = function() { return _StrokeOpacity; };
        this.setStrokeOpacity = function(/** Number */ value) {
            if(_StrokeOpacity !== value) {
                _StrokeOpacity = value;
                this.circle.setOptions({ strokeOpacity: value });
            }
        };

        var _StrokeWeight = options.strokeWeight;
        this.getStrokeWeight = function() { return _StrokeWeight; };
        this.setStrokeWeight = function(/** Number */ value) {
            if(_StrokeWeight !== value) {
                _StrokeWeight = value;
                this.circle.setOptions({ strokeWeight: value });
            }
        };

        var _ZIndex = options.zIndex;
        this.getZIndex = function() { return _ZIndex; };
        this.setZIndex = function(/** Number */ value) {
            if(_ZIndex !== value) {
                _ZIndex = value;
                this.circle.setOptions({ zIndex: value });
            }
        };
    };
    //endregion

    //region MapInfoWindow
    /**
     * A wrapper around a map's native info window.
     * @param {String|Number}           id                  The unique identifier of the info window
     * @param {Object}                  nativeInfoWindow    The map's native info window object that this wraps.
     * @param {Object}                  tag                 An abstract object that is associated with the info window.
     * @param {VRS_OPTIONS_INFOWINDOW}  options             The options used to create the info window.
     * @constructor
     */
    VRS.MapInfoWindow = function(id, nativeInfoWindow, tag, options)
    {
        this.id = id;
        this.infoWindow = nativeInfoWindow;
        this.tag = tag;
        this.isOpen = false;

        /**
         * An array of objects describing the events that have been hooked on the info window. Leave these alone.
         * @type {Array}
         */
        this.nativeListeners = [];

        this.getContent = function() { return this.infoWindow.getContent(); };
        this.setContent = function(/** HTMLElement */ value) { this.infoWindow.setContent(value); };

        var _DisableAutoPan = options.disableAutoPan;
        this.getDisableAutoPan = function() { return _DisableAutoPan; };
        this.setDisableAutoPan = function(/** boolean */ value) {
            if(_DisableAutoPan !== value) {
                _DisableAutoPan = value;
                this.infoWindow.setOptions({ disableAutoPan: value });
            }
        };

        var _MaxWidth = options.maxWidth;
        this.getMaxWidth = function() { return _MaxWidth; };
        this.setMaxWidth = function(/** Number */ value) {
            if(_MaxWidth !== value) {
                _MaxWidth = value;
                this.infoWindow.setOptions({ maxWidth: value });
            }
        };

        var _PixelOffset = options.pixelOffset;
        this.getPixelOffset = function() { return _PixelOffset; };
        this.setPixelOffset = function(/** Number */ value) {
            if(_PixelOffset !== value) {
                _PixelOffset = value;
                this.infoWindow.setOptions({ pixelOffset: value });
            }
        };

        this.getPosition = function() { return VRS.googleMapUtilities.fromGoogleLatLng(this.infoWindow.getPosition()); };
        this.setPosition = function(/** VRS_LAT_LNG */ value) { this.infoWindow.setPosition(VRS.googleMapUtilities.toGoogleLatLng(value)); };

        this.getZIndex = function() { return this.infoWindow.getZIndex(); };
        this.setZIndex = function(/** Number */ value) { this.infoWindow.setZIndex(value); };
    };
    //endregion

    //region MapPluginState
    /**
     * The state held for every map plugin object.
     * @constructor
     */
    VRS.MapPluginState = function()
    {
        /**
         * The map that the plugin wraps.
         * @type {google.maps.Map}
         */
        this.map = undefined;

        /**
         * The map's container.
         * @type {jQuery}
         */
        this.mapContainer = undefined;

        /**
         * An associative array of marker IDs to markers.
         * @type {Object.<number, VRS.MapMarker>}
         */
        this.markers = {};

        /**
         * An associative array of polyline IDs to polylines.
         * @type {Object.<number, VRS.MapPolyline>}
         */
        this.polylines = {};

        /**
         * An associative array of polygon IDs to polygons.
         * @type {Object.<number, VRS.MapPolygon>}
         */
        this.polygons = {};

        /**
         * An associative array of circle IDs to circles.
         * @type {Object.<number, VRS.MapCircle>}
         */
        this.circles = {};

        /**
         * An associative array of info window IDs to info windows.
         * @type {Object.<number|string, VRS.MapInfoWindow>}
         */
        this.infoWindows = {};

        /**
         * An array of Google Maps listener objects that we can use to unhook ourselves from the map when the plugin
         * is destroyed.
         * @type {Array}
         */
        this.nativeHooks = [];
    };
    //endregion

    //region jQueryHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsMap plugin object attached to the element.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsMap}
     */
    VRS.jQueryUIHelper.getMapPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsMap'); };

    /**
     * Returns the options for a vrsMap widget with optional overrides.
     * @param {VRS_OPTIONS_MAP=} overrides
     * @returns {VRS_OPTIONS_MAP}
     */
    VRS.jQueryUIHelper.getMapOptions = function(overrides)
    {
        return $.extend({
            // Google Map load options - THESE ONLY HAVE ANY EFFECT ON THE FIRST MAP LOADED ON A PAGE
            key:                null,                                   // If supplied then the Google Maps script is loaded with this API key. API keys are optional for v3 of Google Maps.
            version:            '3.17',                                 // The version of Google Maps to load.
            sensor:             false,                                  // True if the location-aware stuff is to be turned on.
            libraries:          [],                                     // The optional libraries to load.
            loadMarkerWithLabel:false,                                  // Loads the marker-with-labels library after loading Google Maps. Has no effect with other map providers.

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
    };
    //endregion

    //region vrsMap
    //noinspection JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols,JSUnusedGlobalSymbols
    /**
     * A jQuery UI plugin that wraps a map.
     * @namespace VRS.vrsMap
     */
    // This implementation wraps the Google Map service. So far it's the only implementation. It's important
    // that the API allows for a reasonably smooth switch to other map providers, just in case Google decide
    // one day to follow through on their threat to plaster adverts all over the maps. However there is a LOT
    // of API to wrap, so some shortcuts have been taken - anonymous objects used for points, coordinates etc.,
    // the API follows Google's API etc. etc.
    $.widget('vrs.vrsMap', {
        //region -- options
        /** @type {VRS_OPTIONS_MAP} */
        options: VRS.jQueryUIHelper.getMapOptions(),
        //endregion

        //region -- _getState, _create etc.
        /**
         * Returns the state object attached to the widget, creating it if one does not already exist.
         * @returns {VRS.MapPluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('mapPluginState');
            if(result === undefined) {
                result = new VRS.MapPluginState();
                this.element.data('mapPluginState', result);
            }

            return result;
        },

        /**
         * Creates the widget.
         * @private
         */
        _create: function()
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

                if(self.options.afterCreate) self.options.afterCreate(this);
                if(self.options.openOnCreate) self.open();

                if(VRS.refreshManager) VRS.refreshManager.registerTarget(self.element, self._targetResized, self);
            }, function(jqXHR, textStatus, errorThrown) {
                var state = self._getState();
                state.mapContainer = $('<div />')
                    .addClass('vrsMap notOnline')
                    .appendTo(self.element);
                $('<p/>')
                    .text(VRS.$$.GoogleMapsCouldNotBeLoaded + ': ' + textStatus)
                    .appendTo(state.mapContainer);

                if(self.options.afterCreate) self.options.afterCreate(this);
                if(self.options.openOnCreate && self.options.afterOpen) self.options.afterOpen(self);
            });
        },

        /**
         * Cleans up the widget on destruction.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();

            if(VRS.refreshManager) VRS.refreshManager.unregisterTarget(this.element);

            $.each(state.nativeHooks, function(idx, hookResult) {
                google.maps.event.removeListener(hookResult);
            });
            state.nativeHooks = [];

            if(state.mapContainer) state.mapContainer.remove();
        },

        /**
         * Ensures that Google Maps has been loaded. Note that only the first call to this on a page will actually do anything.
         * @param {function()}                      successCallback
         * @param {function(jqXHR, string, string)} failureCallback
         * @private
         */
        _loadGoogleMapsScript: function(successCallback, failureCallback)
        {
            var url = VRS.globalOptions.mapGoogleMapUseHttps ? VRS.globalOptions.mapGoogleMapHttpsUrl : VRS.globalOptions.mapGoogleMapHttpUrl;
            var params = {
                v:      this.options.version,
                sensor: this.options.sensor
            };
            if(this.options.key)                  params.key = this.options.key;
            if(this.options.libraries.length > 0) params.libraries = this.options.libraries.join(',');

            if(VRS.browserHelper && VRS.browserHelper.notOnline()) {
                failureCallback(null, VRS.$$.WorkingInOfflineMode, VRS.$$.WorkingInOfflineMode);
            } else {
                var callback = successCallback;
                if(this.options.loadMarkerWithLabel) {
                    callback = function() {
                        VRS.scriptManager.loadScript({
                            key:        'markerWithLabel',
                            url:        'script/markerWithLabel.js',
                            async:      true,
                            queue:      true,
                            success:    successCallback
                        });
                    }
                }

                if(window.google && google.maps) callback();
                else {
                    VRS.scriptManager.loadScript({
                        key:        VRS.scriptKey.GoogleMaps,
                        url:        url,
                        params:     params,
                        async:      true,
                        queue:      true,
                        success:    callback,
                        error:      failureCallback || null,
                        timeout:    VRS.globalOptions.mapGoogleMapTimeout
                    });
                }
            }
        },
        //endregion

        //region -- Properties
        /**
         * Gets a value indicating that the map was successfully opened.
         * @returns {boolean}
         */
        isOpen: function() { return !!this._getState().map; },

        /**
         * Gets a value indicating that the map has initialised and is ready for use.
         * @returns {boolean}
         */
        isReady: function() { var state = this._getState(); return !!state.map && !!state.map.getBounds(); },

        /**
         * Gets the rectangle of coordinates that the map is displaying.
         * @returns {VRS_BOUNDS}
         */
        getBounds: function() { return this._getBounds(this._getState()); },
        /**
         * Worker method for getBounds.
         * @param {VRS.MapPluginState} state
         * @returns {VRS_BOUNDS}
         * @private
         */
        _getBounds: function(state) { return state.map ? VRS.googleMapUtilities.fromGoogleLatLngBounds(state.map.getBounds()) : { tlLat: 0, tlLng: 0, brLat: 0, brLng: 0}; },

        /**
         * Gets the coordinate at the centre of the map.
         * @returns {VRS_LAT_LNG}
         */
        getCenter: function() { return this._getCenter(this._getState()); },
        /**
         * Worker method for getCenter.
         * @param {VRS.MapPluginState} state
         * @returns {VRS_LAT_LNG}
         * @private
         */
        _getCenter: function(state) { return state.map ? VRS.googleMapUtilities.fromGoogleLatLng(state.map.getCenter()) : this.options.center; },

        /**
         * Moves the map so that the coordinate passed across is the centre of the map.
         * @param {VRS_LAT_LNG} latLng
         */
        setCenter: function(latLng) { this._setCenter(this._getState(), latLng); },
        /**
         * Worker method for setCenter.
         * @param {VRS.MapPluginState} state
         * @param {VRS_LAT_LNG} latLng
         * @private
         */
        _setCenter: function(state, latLng)
        {
            if(state.map) state.map.setCenter(VRS.googleMapUtilities.toGoogleLatLng(latLng));
            else          this.options.center = latLng;
        },

        /**
         * Returns true if the map is draggable, false if it is not.
         * @returns {boolean}
         */
        getDraggable: function() { return this.options.draggable; },

        /**
         * Returns the currently selected map type.
         * @returns {VRS.MapType}
         */
        getMapType: function() { return this._getMapType(this._getState()); },
        /**
         * Worker method for getMapType.
         * @param {VRS.MapPluginState} state
         * @returns {VRS.MapType}
         * @private
         */
        _getMapType: function(state) { return state.map ? VRS.googleMapUtilities.fromGoogleMapType(state.map.getMapTypeId()) : this.options.mapTypeId; },

        /**
         * Sets the currently selected map type.
         * @param {VRS.MapType} mapType
         */
        setMapType: function(mapType) { this._setMapType(this._getState(), mapType); },
        /**
         * Worker method for setMapType.
         * @param {VRS.MapPluginState} state
         * @param {VRS.MapType} mapType
         * @private
         */
        _setMapType: function(state, mapType)
        {
            if(!state.map) this.options.mapTypeId = mapType;
            else {
                var currentMapType = this.getMapType();
                if(currentMapType !== mapType) state.map.setMapTypeId(VRS.googleMapUtilities.toGoogleMapType(mapType));
            }
        },

        /**
         * Gets a value indicating whether the scroll wheel zooms the map.
         * @returns {boolean}
         */
        getScrollWheel: function() { return this.options.scrollwheel; },

        /**
         * Gets a value indicating whether Google StreetView is enabled for the map.
         * @returns {boolean}
         */
        getStreetView: function() { return this.options.streetViewControl; },

        /**
         * Gets the current zoom level.
         * @returns {number}
         */
        getZoom: function() { return this._getZoom(this._getState()); },
        /**
         * Worker method for getZoom.
         * @param {VRS.MapPluginState} state
         * @returns {number}
         * @private
         */
        _getZoom: function(state) { return state.map ? state.map.getZoom() : this.options.zoom; },

        /**
         * Sets the current zoom level.
         * @param {number} zoom
         */
        setZoom: function(zoom) { this._setZoom(this._getState(), zoom); },
        /**
         * Worker method for setZoom.
         * @param {VRS.MapPluginState} state
         * @param {number} zoom
         * @private
         */
        _setZoom: function(state, zoom)
        {
            if(state.map) state.map.setZoom(zoom);
            else          this.options.zoom = zoom;
        },
        //endregion

        //region -- Events exposed
        // Map events
        /**
         * Raised when the map's boundaries change.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookBoundsChanged: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'boundsChanged', callback, forceThis); },
        _raiseBoundsChanged: function() { this._trigger('boundsChanged'); },

        /**
         * Raised when the map is moved.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookCenterChanged: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'centerChanged', callback, forceThis); },
        _raiseCenterChanged: function() { this._trigger('centerChanged'); },

        /**
         * Raised when the map is clicked.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookClicked: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'clicked', callback, forceThis); },
        _raiseClicked: function(mouseEvent) { this._trigger('clicked', null, { mouseEvent: mouseEvent }); },

        /**
         * Raised when the map is double-clicked.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookDoubleClicked: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'doubleClicked', callback, forceThis); },
        _raiseDoubleClicked: function(mouseEvent) { this._trigger('doubleClicked', null, { mouseEvent: mouseEvent }); },

        /**
         * Raised after the user has stopped moving, zooming or otherwise changing the map's properties.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookIdle: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'idle', callback, forceThis); },
        _raiseIdle: function() { this._trigger('idle'); },

        /**
         * Raised after the user changes the map type.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookMapTypeChanged: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'mapTypeChanged', callback, forceThis); },
        _raiseMapTypeChanged: function() { this._trigger('mapTypeChanged'); },

        /**
         * Raised when the user right-clcks the map.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookRightClicked: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'mouseEvent', callback, forceThis); },
        _raiseRightClicked: function(mouseEvent) { this._trigger('mouseEvent', null, { mouseEvent: mouseEvent }); },

        /**
         * Raised after the map's graphics have been displayed or changed.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookTilesLoaded: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'tilesLoaded', callback, forceThis); },
        _raiseTilesLoaded: function() { this._trigger('tilesLoaded'); },

        /**
         * Raised after the map has been zoomed.
         * @param {function} callback
         * @param {object=} forceThis
         * @returns {object}
         */
        hookZoomChanged: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'zoomChanged', callback, forceThis); },
        _raiseZoomChanged: function() { this._trigger('zoomChanged'); },

        // Marker events
        /**
         * Raised after a marker has been clicked.
         * @param {function(number)} callback Passed the ID of the marker that was clicked.
         * @param {object=} forceThis
         * @returns {object}
         */
        hookMarkerClicked: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'markerClicked', callback, forceThis); },
        _raiseMarkerClicked: function(id) { this._trigger('markerClicked', null, { id: id }); },

        /**
         * Raised after a marker has been dragged to a new location.
         * @param {function(number)} callback Passed the ID of the marker that was dragged.
         * @param {object=} forceThis
         * @returns {object}
         */
        hookMarkerDragged: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'markerDragged', callback, forceThis); },
        _raiseMarkerDragged: function(id) { this._trigger('markerDragged', null, { id: id }); },

        /**
         * Raised after the user closes an InfoWindow.
         * @param {function(number|string)}     callback    Passed the ID of the info window that's been closed.
         * @param {Object}                     [forceThis]  The object to use as 'this' when calling the callback.
         * @returns {Object}                                The hook result.
         */
        hookInfoWindowClosedByUser: function(callback, forceThis) { return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'infoWindowClosedByUser', callback, forceThis); },
        _raiseInfoWindowClosedByUser: function(id) { this._trigger('infoWindowClosedByUser', null, { id: id }); },

        /**
         * Unhooks any method hooked on this object.
         * @param {object} hookResult
         */
        unhook: function(hookResult)
        {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        },

        /**
         * Called when the map becomes idle.
         * @private
         */
        _onIdle: function()
        {
            if(this.options.autoSaveState) this.saveState();
            this._raiseIdle();
        },

        /**
         * Called after the map type has changed.
         * @private
         */
        _onMapTypeChanged: function()
        {
            if(this.options.autoSaveState) this.saveState();
            this._raiseMapTypeChanged();
        },
        //endregion

        //region -- Basic map operations: open, refreshMap, panTo, fitBounds
        /**
         * Opens the map. If you configure the options so that the map is not opened when the widget is created then the
         * Google Maps javascript might still be loading when you call this, in which case the call will fail. The rule
         * is that you either auto-open the map using the options or you use a script tag and wait until the document
         * is ready before you call this method.
         * @param {object}                      userOptions
         * @param {number}                     [userOptions.zoom]                   The zoom level to open with.
         * @param {{lat: number, lng: number}} [userOptions.center]                 The location to centre the map on.
         * @param {VRS.MapType}                [userOptions.mapTypeId]              The map type to use.
         * @param {bool}                       [userOptions.streetViewControl]      True to show the Google Street View control.
         * @param {bool}                       [userOptions.draggable]              True to allow the map to be dragged by the user.
         * @param {bool}                       [userOptions.showHighContrast]       True to show the custom high-contrast map style.
         * @param {bool}                       [userOptions.scaleControl]           True if the map is to show the scale control.
         * @param {VRS.MapControlStyle}        [userOptions.controlStyle]           The optional map type control style.
         * @param {VRS.MapPosition}            [userOptions.controlPosition]        The position of the map control.
         * @param {VRS_MAP_CONTROL[]}          [userOptions.mapControls]            Controls to add to the map after it has been opened.
         */
        open: function(userOptions)
        {
            var self = this;
            var mapOptions = $.extend({}, userOptions, {
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

            var googleMapOptions = {
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
            if(mapOptions.controlPosition) googleMapOptions.mapTypeControlOptions.position = VRS.googleMapUtilities.toGoogleControlPosition(mapOptions.controlPosition);

            if(!mapOptions.pointsOfInterest) {
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
            if(mapOptions.showHighContrast && VRS.globalOptions.mapHighContrastMapStyle && VRS.globalOptions.mapHighContrastMapStyle.length) {
                var googleMapTypeIds = [];
                $.each(VRS.MapType, function(idx, /** VRS.MapType */ mapType) {
                    var googleMapType = VRS.googleMapUtilities.toGoogleMapType(mapType);
                    if(googleMapType) googleMapTypeIds.push(googleMapType);
                });
                googleMapOptions.mapTypeControlOptions.mapTypeIds = googleMapTypeIds;
                var highContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle;
                highContrastMap = new google.maps.StyledMapType(highContrastMapStyle, { name: highContrastMapName });
            }

            var state = this._getState();
            state.map = new google.maps.Map(state.mapContainer[0], googleMapOptions);

            if(highContrastMap) {
                state.map.mapTypes.set(highContrastMapName, /** @type {google.maps.MapType} */ highContrastMap);
            } else if(mapOptions.mapTypeId === VRS.MapType.HighContrast) {
                mapOptions.mapTypeId = VRS.MapType.RoadMap;
            }
            state.map.setMapTypeId(VRS.googleMapUtilities.toGoogleMapType(mapOptions.mapTypeId));

            if(mapOptions.mapControls && mapOptions.mapControls.length) {
                $.each(mapOptions.mapControls, function(/** Number */ idx, /** VRS_MAP_CONTROL */ mapControl) {
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
        },

        /**
         * Hooks the events we care about on the Google map.
         * @param {VRS.MapPluginState} state
         * @private
         */
        _hookEvents: function(state)
        {
            var self = this;
            var map = state.map;
            var hooks = state.nativeHooks;

            hooks.push(google.maps.event.addListener(map, 'bounds_changed',    function() { self._raiseBoundsChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'center_changed',    function() { self._raiseCenterChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'click',             function(mouseEvent) { self._userNotIdle(); self._raiseClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'dblclick',          function(mouseEvent) { self._userNotIdle(); self._raiseDoubleClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'idle',              function() { self._onIdle(); }));
            hooks.push(google.maps.event.addListener(map, 'maptypeid_changed', function() { self._userNotIdle(); self._onMapTypeChanged(); }));
            hooks.push(google.maps.event.addListener(map, 'rightclick',        function(mouseEvent) { self._userNotIdle(); self._raiseRightClicked(mouseEvent); }));
            hooks.push(google.maps.event.addListener(map, 'tilesloaded',       function() { self._raiseTilesLoaded(); }));
            hooks.push(google.maps.event.addListener(map, 'zoom_changed',      function() { self._userNotIdle(); self._raiseZoomChanged(); }));
        },

        /**
         * Records the fact that the user did something.
         * @private
         */
        _userNotIdle: function()
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
        },

        /**
         * Refreshes the map, typically after it has been resized.
         */
        refreshMap: function()
        {
            var state = this._getState();
            if(state.map) google.maps.event.trigger(state.map, 'resize');
        },

        /**
         * Moves the map to a new map centre.
         * @param {VRS_LAT_LNG} mapCenter
         * @param {VRS.MapPluginState} [state]
         */
        panTo: function(mapCenter, state)
        {
            if(!state) state = this._getState();
            if(state.map) state.map.panTo(VRS.googleMapUtilities.toGoogleLatLng(mapCenter));
            else          this.options.center = mapCenter;
        },

        /**
         * Moves the map so that the bounds specified are shown. This does nothing if the map has not been opened.
         * @param {VRS_BOUNDS} bounds
         */
        fitBounds: function(bounds)
        {
            var state = this._getState();
            if(state.map) {
                state.map.fitBounds(VRS.googleMapUtilities.toGoogleLatLngBounds(bounds));
            }
        },
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the map.
         */
        saveState: function()
        {
            var settings = this._createSettings();
            VRS.configStorage.save(this._persistenceKey(), settings);
        },

        /**
         * Returns a previously saved state or returns the current state if no state was previously saved.
         * @returns {VRS_STATE_MAP_PLUGIN}
         */
        loadState: function()
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            return $.extend(this._createSettings(), savedSettings);
        },

        /**
         * Applies a previously saved state.
         * @param {VRS_STATE_MAP_PLUGIN} config
         */
        applyState: function(config)
        {
            config = config || {};
            var state = this._getState();

            if(config.center)                       this._setCenter(state, config.center);
            if(config.zoom || config.zoom === 0)    this._setZoom(state, config.zoom);
            if(config.mapTypeId)                    this._setMapType(state, config.mapTypeId);
        },

        /**
         * Loads and applies a previously saved state.
         */
        loadAndApplyState: function()
        {
            this.applyState(this.loadState());
        },

        /**
         * Returns the key against which the state will be saved.
         * @returns {string}
         * @private
         */
        _persistenceKey: function()
        {
            return 'vrsMapState-' + (this.options.name || 'default');
        },

        /**
         * Returns the current state of the object.
         * @returns {VRS_STATE_MAP_PLUGIN}
         * @private
         */
        _createSettings: function()
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
        },
        //endregion

        //region -- Map marker operations: addMarker, getMarker, destroyMarker, centreOnMarker
        /**
         * Adds a marker to the map. Behaviour is undefined if the map has not been opened.
         * @param {string|number}   id                                  The unique ID of the marker to add. Destroys any existing marker with this ID.
         * @param {object}          userOptions                         The settings for the new marker.
         * @param {bool}           [userOptions.clickable]              True if the marker raises the clicked event when clicked. Default true.
         * @param {bool}           [userOptions.draggable]              True if the marker can be moved by the user. Default false.
         * @param {bool}           [userOptions.flat]                   True if the marker has no shadow or 3D effect. Default false.
         * @param {bool}           [userOptions.optimized]              True if the marker image can be merged with other marker images. Be wary of using this with moving or changing markers. Default false.
         * @param {bool}           [userOptions.raiseOnDrag]            True if the marker should appear to hover above the map when the user drags it. Default true.
         * @param {bool}           [userOptions.visible]                True if the marker should start visible. Default true.
         * @param {bool}           [userOptions.animateAdd]             True if the marker's addition to the map should be animated. Default false.
         * @param {VRS_LAT_LNG}    [userOptions.position]               The location of the marker. Defaults to map centre.
         * @param {VRS.MapIcon}    [userOptions.icon]                   The image to use for the marker. Defaults to native default marker image.
         * @param {string}         [userOptions.tooltip]                The tooltip text to show for the marker. Defaults to empty string.
         * @param {number}         [userOptions.zIndex]                 The z-index for the marker. Defaults to native default z-index.
         * @param {*}              [userOptions.tag]                    An object to record against the marker.
         * @param {bool}           [userOptions.useMarkerWithLabel]     True if the Google marker is to be created as a MarkerWithLabel.
         * @param {bool}           [userOptions.mwlLabelInBackground]   Sets the labelInBackground flag for MarkerWithLabel markers.
         * @param {string}         [userOptions.mwlLabelClass]          Sets the class to apply to labels for MarkerWithLabel markers.
         * @returns {VRS.MapMarker=}
         */
        addMarker: function(id, userOptions)
        {
            var self = this;
            /** @type {VRS.MapMarker} */ var result;

            var state = this._getState();
            if(state.map) {
                var googleOptions = {
                    map:            state.map,
                    clickable:      userOptions.clickable !== undefined ? userOptions.clickable : true,
                    draggable:      userOptions.draggable !== undefined ? userOptions.draggable : false,
                    flat:           userOptions.flat !== undefined ? userOptions.flat : false,
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
                var marker;
                if(!userOptions.useMarkerWithLabel) marker = new google.maps.Marker(googleOptions);
                else                                marker = new MarkerWithLabel(googleOptions);
                result = new VRS.MapMarker(id, marker, !!userOptions.useMarkerWithLabel, userOptions.tag);
                state.markers[id] = result;

                result.nativeListeners.push(google.maps.event.addListener(marker, 'click', function() { self._raiseMarkerClicked.call(self, id); }));
                result.nativeListeners.push(google.maps.event.addListener(marker, 'dragend', function() { self._raiseMarkerDragged.call(self, id); }));
            }

            return result;
        },

        /**
         * Gets a VRS.MapMarker by its ID or, if passed a marker, returns the same marker.
         * @param {string|number|VRS.MapMarker} idOrMarker
         * @returns {VRS.MapMarker}
         */
        getMarker: function(idOrMarker)
        {
            if(idOrMarker instanceof VRS.MapMarker) return idOrMarker;
            var state = this._getState();
            return state.markers[idOrMarker];
        },

        /**
         * Destroys the map marker passed across.
         * @param {string|number|VRS.MapMarker} idOrMarker
         */
        destroyMarker: function(idOrMarker)
        {
            var state = this._getState();
            var marker = this.getMarker(idOrMarker);
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
        },

        /**
         * Centres the map on the marker passed across.
         * @param {string|number|VRS.MapMarker} idOrMarker
         */
        centerOnMarker: function(idOrMarker)
        {
            var state = this._getState();
            var marker = this.getMarker(idOrMarker);
            if(marker) state.map.setCenter(marker.marker.getPosition());
        },
        //endregion

        //region -- Polyline operations: addPolyline, getPolyline, destroyPolyline, trimPolyline, appendToPolyline
        /**
         * Adds a line to the map. The behaviour is undefined if the map has not already been opened.
         * @param {string|number}               id                          The unique ID of the line to add. If the ID is already in use then the existing line is replaced.
         * @param {object}                      userOptions                 The settings for the new line.
         * @param {bool}                       [userOptions.clickable]      True if the line can be clicked. Defaults to false.
         * @param {bool}                       [userOptions.draggable]      True if the line can be dragged by the user. Defaults to false.
         * @param {bool}                       [userOptions.editable]       True if the line can be edited by the user. Defaults to false.
         * @param {bool}                       [userOptions.geodesic]       True if the line is geodesic and follows the curve of the earth. Defaults to false.
         * @param {bool}                       [userOptions.visible]        True if the line is immediately visible. Defaults to true.
         * @param {{lat:number, lng:number}[]} [userOptions.path]           The points along the path of the line. Defaults to empty path.
         * @param {string}                     [userOptions.strokeColour]   The CSS stroke colour of the line. Defaults to #000000.
         * @param {number}                     [userOptions.strokeOpacity]  The opacity of the line (0 = transparent, 1 = opaque).
         * @param {number}                     [userOptions.strokeWeight]   The pixel width of the line.
         * @param {number}                     [userOptions.zIndex]         The vertical order of the line.
         * @param {*}                          [userOptions.tag]            An object to attach to the VRS.MapPolyline]
         * @returns {VRS.MapPolyline=}
         */
        addPolyline: function(id, userOptions)
        {
            /** @type {VRS.MapPolyline} */ var result;

            var state = this._getState();
            if(state.map) {
                var googleOptions = {
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
                result = new VRS.MapPolyline(id, polyline, userOptions.tag, {
                    strokeColour:   userOptions.strokeColour,
                    strokeOpacity:  userOptions.strokeOpacity,
                    strokeWeight:   userOptions.strokeWeight
                });
                state.polylines[id] = result;
            }

            return result;
        },

        /**
         * Returns the VRS.MapPolyline associated with the ID.
         * @param {string|number|VRS.MapPolyline} idOrPolyline
         * @returns {VRS.MapPolyline}
         */
        getPolyline: function(idOrPolyline)
        {
            if(idOrPolyline instanceof VRS.MapPolyline) return idOrPolyline;
            var state = this._getState();
            return state.polylines[idOrPolyline];
        },

        /**
         * Destroys the line passed across.
         * @param {string|number|VRS.MapPolyline} idOrPolyline
         */
        destroyPolyline: function(idOrPolyline)
        {
            var state = this._getState();
            var polyline = this.getPolyline(idOrPolyline);
            if(polyline) {
                polyline.polyline.setMap(null);
                polyline.polyline = null;
                polyline.tag = null;
                delete state.polylines[polyline.id];
                polyline.id = null;
            }
        },

        /**
         * Removes a number of points from the start or end of the line.
         * @param {string|number|VRS.MapPolyline}   idOrPolyline    The line to trim.
         * @param {number}                          countPoints     The number of points to remove.
         * @param {bool}                            fromStart       True to remove the points from the start of the line, false to remove them from the end.
         * @returns {{ emptied: boolean, countRemoved: number}}     Object indicating whether all points were removed (emptied = true) and the number of points removed.
         */
        trimPolyline: function(idOrPolyline, countPoints, fromStart)
        {
            var emptied = false;
            var countRemoved = 0;

            if(countPoints > 0) {
                var polyline = this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getPath();
                var length = points.getLength();
                if(length < countPoints) countPoints = length;
                if(countPoints > 0) {
                    countRemoved = countPoints;
                    emptied = countPoints === length;
                    if(emptied) points.clear();
                    else {
                        var end = length - 1;
                        for(;countPoints > 0;--countPoints) {
                            points.removeAt(fromStart ? 0 : end--);
                        }
                    }
                }
            }

            return { emptied: emptied, countRemoved: countRemoved };
        },

        /**
         * Remove a single point from the line's path.
         * @param {string|number|VRS.MapPolyline}   idOrPolyline    The line to change.
         * @param {number}                          index           The index of the point to remove.
         */
        removePolylinePointAt: function(idOrPolyline, index)
        {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getPath();
            points.removeAt(index);
        },

        /**
         * Appends points to a line's path.
         * @param {string|number|VRS.MapPolyline}   idOrPolyline    The line to change.
         * @param {{lat:number, lng:number}[]}      path            The points to add to the path.
         * @param {bool}                            toStart         True to add the points to the start of the path, false to add them to the end.
         */
        appendToPolyline: function(idOrPolyline, path, toStart)
        {
            var length = !path ? 0 : path.length;
            if(length > 0) {
                var polyline = this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getPath();
                var insertAt = toStart ? 0 : -1;
                for(var i = 0;i < length;++i) {
                    var googlePoint = VRS.googleMapUtilities.toGoogleLatLng(path[i]);
                    if(toStart) points.insertAt(insertAt++, googlePoint);
                    else points.push(googlePoint);
                }
            }
        },

        /**
         * Replaces an existing point along a line's path.
         * @param {string|number|VRS.MapPolyline}   idOrPolyline    The line to change.
         * @param {number}                          index           The index of the point to change. -1 changes the last point in the list.
         * @param {{lat:number, lng:number}}        point           The new point to use.
         */
        replacePolylinePointAt: function(idOrPolyline, index, point)
        {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getPath();
            var length = points.getLength();
            if(index === -1) index = length - 1;
            if(index >= 0 && index < length) points.setAt(index, VRS.googleMapUtilities.toGoogleLatLng(point));
        },
        //endregion

        //region -- Polygon operations: addPolygon, getPolygon, destroyPolygon
        /**
         * Adds a polygon to the map. The behaviour is undefined if the map has not already been opened.
         * @param {string|number}           id              The unique ID of the shape to add. If the ID is already in use then the existing shape is replaced.
         * @param {VRS_OPTIONS_POLYGON}     userOptions     The settings for the new shape.
         * @returns {VRS.MapPolygon=}
         */
        addPolygon: function(id, userOptions)
        {
            /** @type {VRS.MapPolygon} */ var result;

            var state = this._getState();
            if(state.map) {
                var googleOptions = {
                    map:            state.map,
                    clickable:      userOptions.clickable !== undefined ? userOptions.clickable : false,
                    draggable:      userOptions.draggable !== undefined ? userOptions.draggable : false,
                    editable:       userOptions.editable !== undefined ? userOptions.editable : false,
                    geodesic:       userOptions.geodesic !== undefined ? userOptions.geodesic : false,
                    fillColor:      userOptions.fillColour,
                    fillOpacity:    userOptions.fillOpacity,
                    paths:          VRS.googleMapUtilities.toGoogleLatLngMVCArrayArray(userOptions.paths) || undefined,
                    strokeColor:    userOptions.strokeColour || '#000000',
                    strokeWeight:   userOptions.strokeWeight,
                    strokeOpacity:  userOptions.strokeOpacity,
                    visible:        userOptions.visible !== undefined ? userOptions.visible : true,
                    zIndex:         userOptions.zIndex
                };

                this.destroyPolygon(id);
                var polygon = new google.maps.Polygon(googleOptions);
                result = new VRS.MapPolygon(id, polygon, userOptions.tag, userOptions);
                state.polygons[id] = result;
            }

            return result;
        },

        /**
         * Returns the VRS.MapPolygon associated with the ID.
         * @param {string|number|VRS.MapPolygon} idOrPolygon
         * @returns {VRS.MapPolygon}
         */
        getPolygon: function(idOrPolygon)
        {
            if(idOrPolygon instanceof VRS.MapPolygon) return idOrPolygon;
            var state = this._getState();
            return state.polygons[idOrPolygon];
        },

        /**
         * Destroys the polygon passed across.
         * @param {string|number|VRS.MapPolygon} idOrPolygon
         */
        destroyPolygon: function(idOrPolygon)
        {
            var state = this._getState();
            var polygon = this.getPolygon(idOrPolygon);
            if(polygon) {
                polygon.polygon.setMap(null);
                polygon.polygon = null;
                polygon.tag = null;
                delete state.polygons[polygon.id];
                polygon.id = null;
            }
        },
        //endregion

        //region -- Circle operations: addCircle, getCircle, destroyCircle
        /**
         * Adds a circle to the map.
         * @param {string|Number}   id                          The ID number of the circle to add.
         * @param {Object}          userOptions                 The options to use when creating the circle.
         * @param {VRS_LAT_LNG}     userOptions.center          The centre of the circle.
         * @param {boolean}        [userOptions.clickable]      True if the circle can be clicked. Defaults to false.
         * @param {boolean}        [userOptions.draggable]      True if the circle can be dragged. Defaults to false.
         * @param {boolean}        [userOptions.editable]       True if the circle can be edited. Defaults to false.
         * @param {string}         [userOptions.fillColor]      The CSS fill colour of the circle. Defaults to black.
         * @param {Number}         [userOptions.fillOpacity]    The opacity of the fill. Defaults to 0 (transparent).
         * @param {Number}          userOptions.radius          The radius of the circle in metres.
         * @param {string}         [userOptions.strokeColor]    The CSS stroke colour of the circle. Defaults to black.
         * @param {Number}         [userOptions.strokeOpacity]  The opacity of the stroke. Defaults to 1 (opaque).
         * @param {Number}         [userOptions.strokeWeight]   The stroke weight in pixels. Defaults to 1.
         * @param {boolean}        [userOptions.visible]        True if the circle is visible, false if it is not. Defaults to true.
         * @param {boolean}        [userOptions.zIndex]         The z-index of the circle. Defaults to undefined.
         * @param {object}         [userOptions.tag]            An object that the application can associate with the circle. Defaults to null.
         * @returns {VRS.MapCircle}
         */
        addCircle: function(id, userOptions)
        {
            /** @type {VRS.MapCircle} */
            var result = null;

            var state = this._getState();
            if(state.map) {
                var googleOptions = $.extend({
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
                result = new VRS.MapCircle(id, circle, userOptions.tag, userOptions || {});
                state.circles[id] = result;
            }

            return result;
        },

        /**
         * Returns the VRS.MapCircle associated with the ID.
         * @param {string|number|VRS.MapCircle} idOrCircle
         * @returns {VRS.MapCircle}
         */
        getCircle: function(idOrCircle)
        {
            if(idOrCircle instanceof VRS.MapCircle) return idOrCircle;
            var state = this._getState();
            return state.circles[idOrCircle];
        },

        /**
         * Destroys the circle passed across.
         * @param {string|number|VRS.MapCircle} idOrCircle
         */
        destroyCircle: function(idOrCircle)
        {
            var state = this._getState();
            var circle = this.getCircle(idOrCircle);
            if(circle) {
                circle.circle.setMap(null);
                circle.circle = null;
                circle.tag = null;
                delete state.circles[circle.id];
                circle.id = null;
            }
        },
        //endregion

        //region -- InfoWindow operations: addInfoWindow, getInfoWindow, destroyInfoWindow, openInfoWindow, closeInfoWindow
        /**
         * Returns an Info Window ID that is guaranteed to not be in current use.
         * @returns {string|number}
         */
        getUnusedInfoWindowId: function()
        {
            var result;

            var state = this._getState();
            for(var i = 1;i > 0;++i) {
                result = 'autoID' + i;
                if(!state.infoWindows[result]) break;
            }

            return result;
        },

        /**
         * Creates a new info window for the map.
         * @param {string|number}           id              The unique identifier for the info window.
         * @param {VRS_OPTIONS_INFOWINDOW}  userOptions     The options to use when creating the info window.
         * @returns {VRS.MapInfoWindow}
         */
        addInfoWindow: function(id, userOptions)
        {
            /** @type {VRS.MapInfoWindow} */
            var result = null;

            var state = this._getState();
            if(state.map) {
                var googleOptions = $.extend({ }, userOptions);
                if(googleOptions.position) googleOptions.position = VRS.googleMapUtilities.toGoogleLatLng(googleOptions.position);

                this.destroyInfoWindow(id);
                var infoWindow = new google.maps.InfoWindow(googleOptions);
                result = new VRS.MapInfoWindow(id, infoWindow, userOptions.tag, userOptions || {});
                state.infoWindows[id] = result;

                var self = this;
                result.nativeListeners.push(google.maps.event.addListener(infoWindow, 'closeclick', function() {
                    result.isOpen = false;
                    self._raiseInfoWindowClosedByUser(id);
                }));
            }

            return result;
        },

        /**
         * If passed the ID of an info window then the associated info window is returned. If passed an info window
         * then it is just returned.
         * @param {string|number|VRS.MapInfoWindow} idOrInfoWindow
         * @returns {VRS.MapInfoWindow}
         */
        getInfoWindow: function(idOrInfoWindow)
        {
            if(idOrInfoWindow instanceof VRS.MapInfoWindow) return idOrInfoWindow;
            var state = this._getState();
            return state.infoWindows[idOrInfoWindow];
        },

        /**
         * Destroys the info window passed across. Note that Google do not supply a method to dispose of an info window.
         * @param {string|number|VRS.MapInfoWindow} idOrInfoWindow
         */
        destroyInfoWindow: function(idOrInfoWindow)
        {
            var state = this._getState();
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
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
        },

        /**
         * Opens the info window at the position specified on the window, optionally with an anchor to specify the
         * location of the tip of the info window. Does nothing if it's already open.
         * @param {string|number|VRS.MapInfoWindow}     idOrInfoWindow  The info window or its ID.
         * @param {VRS.MapMarker}                      [mapMarker]      The optional map marker to use as an anchor.
         */
        openInfoWindow: function(idOrInfoWindow, mapMarker)
        {
            var state = this._getState();
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            if(infoWindow && state.map && !infoWindow.isOpen) {
                infoWindow.infoWindow.open(state.map, mapMarker ? mapMarker.marker : undefined);
                infoWindow.isOpen = true;
            }
        },

        /**
         * Closes an info window if it's open. Does nothing if it's already closed.
         * @param {string|number|VRS.MapInfoWindow} idOrInfoWindow
         */
        closeInfoWindow: function(idOrInfoWindow)
        {
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            if(infoWindow && infoWindow.isOpen) {
                infoWindow.infoWindow.close();
                infoWindow.isOpen = false;
            }
        },
        //endregion

        //region -- addControl
        /**
         * Adds a control to the map.
         * @param {jQuery|HTMLElement}  element         The jQuery or DOM element to add to the map.
         * @param {VRS.MapPosition}     mapPosition     The location where the control should be added.
         */
        addControl: function(element, mapPosition)
        {
            /// <summary>Adds arbitrary DOM or jQuery elements to the map.</summary>
            var state = this._getState();
            if(state.map) {
                var controlsArray = state.map.controls[VRS.googleMapUtilities.toGoogleControlPosition(mapPosition)];
                if(!(element instanceof jQuery)) controlsArray.push(element);
                else $.each(element, function() { controlsArray.push(this); });
            }
        },
        //endregion

        //region -- Events subscribed
        /**
         * Called when the refresh manager indicates that one of our parents has resized, or done something that we need
         * to refresh for.
         * @private
         */
        _targetResized: function()
        {
            var state = this._getState();

            var center = this._getCenter(state);
            this.refreshMap();
            this._setCenter(state, center);
        },
        //endregion

        __nop: null
    });
    //endregion
})(window.VRS = window.VRS || {}, jQuery);