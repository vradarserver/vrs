/**
 * @license Copyright © 2018 onwards, Andrew Whewell
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
 * @fileoverview A jQuery UI plugin that wraps OpenStreetMap.
 */
namespace VRS
{
    /*
     * Backwards compatible global options. These are not used, they are here so that custom scripts that
     * rely on them won't crash.
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapGoogleMapHttpUrl = VRS.globalOptions.mapGoogleMapHttpUrl || '';
    VRS.globalOptions.mapGoogleMapHttpsUrl = VRS.globalOptions.mapGoogleMapHttpsUrl || '';
    VRS.globalOptions.mapGoogleMapTimeout = VRS.globalOptions.mapGoogleMapTimeout || 30000;
    VRS.globalOptions.mapGoogleMapUseHttps = VRS.globalOptions.mapGoogleMapUseHttps !== undefined ? VRS.globalOptions.mapGoogleMapUseHttps : true;
    VRS.globalOptions.mapShowStreetView = VRS.globalOptions.mapShowStreetView !== undefined ? VRS.globalOptions.mapShowStreetView : false;
    VRS.globalOptions.mapShowHighContrastStyle = VRS.globalOptions.mapShowHighContrastStyle !== undefined ? VRS.globalOptions.mapShowHighContrastStyle : true;
    VRS.globalOptions.mapHighContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle !== undefined ? VRS.globalOptions.mapHighContrastMapStyle : [];

    /*
     * These options are shared with Google Maps. They should probably be moved out of both plugin modules to somewhere common.
     */
    VRS.globalOptions.mapScrollWheelActive = VRS.globalOptions.mapScrollWheelActive !== undefined ? VRS.globalOptions.mapScrollWheelActive : true;
    VRS.globalOptions.mapDraggable = VRS.globalOptions.mapDraggable !== undefined ? VRS.globalOptions.mapDraggable : true;
    VRS.globalOptions.mapShowPointsOfInterest = VRS.globalOptions.mapShowPointsOfInterest !== undefined ? VRS.globalOptions.mapShowPointsOfInterest : false;
    VRS.globalOptions.mapShowScaleControl = VRS.globalOptions.mapShowScaleControl !== undefined ? VRS.globalOptions.mapShowScaleControl : true;

    export type LeafletControlPosition = 'topleft' | 'topright' | 'bottomleft' | 'bottomright';

    export class LeafletUtilities
    {
        fromLeafletLatLng(latLng: L.LatLng): ILatLng
        {
            return latLng;
        }

        toLeafletLatLng(latLng: ILatLng): L.LatLng
        {
            if(latLng instanceof L.LatLng) {
                return latLng;
            } else if(latLng) {
                return new L.LatLng(latLng.lat, latLng.lng);
            }

            return null;
        }

        fromLeafletLatLngArray(latLngArray: L.LatLng[]): ILatLng[]
        {
            return latLngArray;
        }

        toLeafletLatLngArray(latLngArray: ILatLng[]): L.LatLng[]
        {
            latLngArray = latLngArray || [];

            var result = [];
            var len = latLngArray.length;
            for(var i = 0;i < len;++i) {
                result.push(this.toLeafletLatLng(latLngArray[i]));
            }

            return result;
        }

        fromLeafletLatLngBounds(bounds: L.LatLngBounds): IBounds
        {
            if(!bounds) {
                return null;
            }
            return {
                tlLat: bounds.getNorth(),
                tlLng: bounds.getWest(),
                brLat: bounds.getSouth(),
                brLng: bounds.getEast()
            };
        }

        toLeaftletLatLngBounds(bounds: IBounds): L.LatLngBounds
        {
            if(!bounds) {
                return null;
            }
            return new L.LatLngBounds([ bounds.brLat, bounds.tlLng ], [ bounds.tlLat, bounds.brLng ]);
        }

        fromLeafletIcon(icon: L.Icon|L.DivIcon): IMapIcon
        {
            // For now I'm just supporting Icon objects
            if(icon === null || icon === undefined) {
                return null;
            }

            return new MapIcon(
                icon.options.iconUrl,
                VRS.leafletUtilities.fromLeafletSize(icon.options.iconSize),
                VRS.leafletUtilities.fromLeafletPoint(icon.options.iconAnchor),
                null,
                null
            );
        }

        toLeafletIcon(icon: string|IMapIcon): L.Icon
        {
            if(typeof icon === 'string') {
                return null;
            }

            return L.icon({
                iconUrl: icon.url,
                iconSize: VRS.leafletUtilities.toLeafletSize(icon.size),
                iconAnchor: VRS.leafletUtilities.toLeafletPoint(icon.anchor)
            });
        }

        fromLeafletContent(content: L.Content): string
        {
            if(content === null || content === undefined) {
                return null;
            } else {
                if(typeof content === "string") {
                    return content;
                }
                return (<HTMLElement>content).innerText;
            }
        }

        fromLeafletSize(size: L.PointExpression): ISize
        {
            if(size === null || size === undefined) {
                return null;
            }
            if(size instanceof L.Point) {
                return {
                    width: size.x,
                    height: size.y
                };
            }
            return {
                width:  (<L.PointTuple>size)[0],
                height: (<L.PointTuple>size)[1]
            };
        }

        toLeafletSize(size: ISize): L.Point
        {
            if(size === null || size === undefined) {
                return null;
            }
            return L.point(size.width, size.height);
        }

        fromLeafletPoint(point: L.PointExpression): IPoint
        {
            if(point === null || point === undefined) {
                return null;
            }
            if(point instanceof L.Point) {
                return point;
            }
            return {
                x:  (<L.PointTuple>point)[0],
                y: (<L.PointTuple>point)[1]
            };
        }

        toLeafletPoint(point: IPoint): L.Point
        {
            if(point === null || point === undefined) {
                return null;
            }
            if(point instanceof L.Point) {
                return point;
            }
            return L.point(point.x, point.y);
        }

        fromLeafletMapPosition(mapPosition: LeafletControlPosition): VRS.MapPositionEnum
        {
            switch(mapPosition || '') {
                case 'topleft':     return VRS.MapPosition.TopLeft;
                case 'bottomleft':  return VRS.MapPosition.BottomLeft;
                case 'bottomright': return VRS.MapPosition.BottomRight;
                default:            return VRS.MapPosition.TopRight;
            }
        }

        toLeafletMapPosition(mapPosition: VRS.MapPositionEnum): LeafletControlPosition
        {
            switch(mapPosition || VRS.MapPosition.TopRight) {
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
        }
    }
    export var leafletUtilities = new VRS.LeafletUtilities();

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
            // OpenStreetMap load options - THESE ONLY HAVE ANY EFFECT ON THE FIRST MAP LOADED ON A PAGE
            key:                null,                                   // Unused
            version:            '',                                     // Unused
            sensor:             false,                                  // Unused
            libraries:          [],                                     // Unused
            loadMarkerWithLabel:false,                                  // Unused
            loadMarkerCluster:  false,                                  // Unused

            // Map options
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
     * An abstracted wrapper around an object that represents a leaflet marker.
     */
    class MapMarker implements IMapMarker
    {
        id: string|number;
        marker: L.Marker;
        map: L.Map;
        mapIcon: IMapIcon;
        zIndex: number;
        isMarkerWithLabel: boolean;
        tag: any;
        visible: boolean;

        /**
         * Creates a new object.
         * @param {string|number}       id                  The identifier of the marker.
         * @param {L.Map}               nativeMap           The map that this marker will be (or is) attached to.
         * @param {L.Marker}            nativeMarker        The native map marker handle to wrap.
         * @param {L.MarkerOptions}     markerOptions       The options used when creating the marker.
         * @param {boolean}             isMarkerWithLabel   See notes against field of same name.
         * @param {boolean}             isVisible           True if the marker is already attached to a map.
         * @param {*}                   tag                 An object to carry around with the marker. No meaning is attached to the tag.
        */
        constructor(id: string|number, map: L.Map, nativeMarker: L.Marker, markerOptions: L.MarkerOptions, isMarkerWithLabel: boolean, isVisible: boolean, tag: any)
        {
            this.id = id;
            this.map = map;
            this.marker = nativeMarker;
            this.mapIcon = VRS.leafletUtilities.fromLeafletIcon(markerOptions.icon);
            this.zIndex = markerOptions.zIndexOffset;
            this.isMarkerWithLabel = isMarkerWithLabel;
            this.tag = tag;
            this.visible = isVisible;
        }

        /**
         * Returns true if the marker can be dragged.
         */
        getDraggable() : boolean
        {
            return this.marker.dragging.enabled();
        }

        /**
         * Sets a value indicating whether the marker can be dragged.
         */
        setDraggable(draggable: boolean)
        {
            if(draggable) {
                this.marker.dragging.enable();
            } else {
                this.marker.dragging.disable();
            }
        }

        /**
         * Returns the icon for the marker.
         */
        getIcon() : IMapIcon
        {
            return this.mapIcon;
        }

        /**
         * Sets the icon for the marker.
         */
        setIcon(icon: IMapIcon)
        {
            this.marker.setIcon(VRS.leafletUtilities.toLeafletIcon(icon));
            this.mapIcon = icon;
        }

        /**
         * Gets the coordinates of the marker.
         */
        getPosition() : ILatLng
        {
            return VRS.leafletUtilities.fromLeafletLatLng(this.marker.getLatLng());
        }

        /**
         * Sets the coordinates for the marker.
         */
        setPosition(position: ILatLng)
        {
            this.marker.setLatLng(VRS.leafletUtilities.toLeafletLatLng(position));
        }

        /**
         * Gets the tooltip for the marker.
         */
        getTooltip() : string
        {
            var tooltip = this.marker.getTooltip();
            return tooltip ? VRS.leafletUtilities.fromLeafletContent(tooltip.getContent()) : null;
        }

        /**
         * Sets the tooltip for the marker.
         */
        setTooltip(tooltip: string)
        {
            this.marker.setTooltipContent(tooltip);
        }

        /**
         * Gets a value indicating that the marker is visible.
         */
        getVisible() : boolean
        {
            return this.visible;
        }

        /**
         * Sets a value indicating whether the marker is visible.
         */
        setVisible(visible: boolean)
        {
            if(visible !== this.getVisible()) {
                if(visible) {
                    this.marker.addTo(this.map);
                } else {
                    this.marker.removeFrom(this.map);
                }
                this.visible = visible;
            }
        }

        /**
         * Gets the z-index of the marker.
         */
        getZIndex() : number
        {
            return this.zIndex;
        }

        /**
         * Sets the z-index of the marker.
         */
        setZIndex(zIndex: number)
        {
            this.marker.setZIndexOffset(zIndex);
            this.zIndex = zIndex;
        }

        /**
         * Returns true if the marker was created with useMarkerWithLabel and the label is visible.
         * Note that this is not a part of the marker interface.
         */
        getLabelVisible() : boolean
        {
            return false;
        }

        /**
         * Sets the visibility of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelVisible(visible: boolean)
        {
            ;
        }

        /**
         * Sets the label content. Only works on markers that have been created with useMarkerWithLabel.
         */
        getLabelContent()
        {
            return '';
        }

        /**
         * Sets the content of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelContent(content: string)
        {
            ;
        }

        /**
         * Gets the label anchor. Only works on markers that have been created with useMarkerWithLabel.
         */
        getLabelAnchor()
        {
            return null;
        }

        /**
         * Sets the anchor for a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelAnchor(anchor: IPoint)
        {
            ;
        }
    }

    /**
     * An object that wraps a map's native polyline object.
     */
    class MapPolyline implements IMapPolyline
    {
        id:         string | number;
        map:        L.Map;
        polyline:   L.Polyline;
        tag:        any;
        visible:    boolean;

        constructor(id: string | number, map: L.Map, nativePolyline: L.Polyline, tag: any, options: IMapPolylineSettings)
        {
            this.id = id;
            this.map = map;
            this.polyline = nativePolyline;
            this.tag = tag;
            this.visible = options.visible;

            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
        }

        getDraggable() : boolean
        {
            return false;
        }

        setDraggable(draggable: boolean)
        {
            ;
        }

        getEditable() : boolean
        {
            return false;
        }

        setEditable(editable: boolean)
        {
            ;
        }

        getVisible() : boolean
        {
            return this.visible;
        }

        setVisible(visible: boolean)
        {
            if(this.visible !== visible) {
                if(visible) {
                    this.polyline.addTo(this.map);
                } else {
                    this.polyline.removeFrom(this.map);
                }
                this.visible = visible;
            }
        }

        private _StrokeColour: string;
        getStrokeColour() : string
        {
            return this._StrokeColour;
        }
        setStrokeColour(value: string)
        {
            if(value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polyline.setStyle({
                    color: value
                });
            }
        }

        private _StrokeOpacity: number;
        getStrokeOpacity() : number
        {
            return this._StrokeOpacity;
        }
        setStrokeOpacity(value: number)
        {
            if(value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polyline.setStyle({
                    opacity: value
                });
            }
        }

        private _StrokeWeight: number;
        getStrokeWeight() : number
        {
            return this._StrokeWeight;
        }
        setStrokeWeight(value: number)
        {
            if(value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polyline.setStyle({
                    weight: value
                });
            }
        }

        getPath() : ILatLng[]
        {
            return VRS.leafletUtilities.fromLeafletLatLngArray(<L.LatLng[]>(this.polyline.getLatLngs()));
        }

        setPath(path: ILatLng[])
        {
            this.polyline.setLatLngs(VRS.leafletUtilities.toLeafletLatLngArray(path));
        }

        getFirstLatLng() : ILatLng
        {
            var result = null;
            var nativePath = <L.LatLng[]>this.polyline.getLatLngs();
            if(nativePath.length) result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[0]);

            return result;
        }

        getLastLatLng() : ILatLng
        {
            var result = null;
            var nativePath = <L.LatLng[]>this.polyline.getLatLngs();
            if(nativePath.length) result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[nativePath.length - 1]);

            return result;
        }
    }

    /**
     * An object that wraps a map's native circle object.
     * @constructor
     */
    class MapCircle implements IMapCircle
    {
        id:         string | number;
        circle:     L.Circle;
        map:        L.Map;
        tag:        any;
        visible:    boolean;

        /**
         * Creates a new object.
         * @param {string|number}           id                  The unique identifier of the circle object.
         * @param {L.Map}                   map                 The map to add the circle to or remove the circle from.
         * @param {L.Circle}                nativeCircle        The native object that is being wrapped.
         * @param {*}                       tag                 An object attached to the circle.
         * @param {IMapCircleSettings}      options             The options used when the circle was created.
        */
        constructor(id: string | number, map: L.Map, nativeCircle: L.Circle, tag: any, options: IMapCircleSettings)
        {
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

        getBounds() : IBounds
        {
            return VRS.leafletUtilities.fromLeafletLatLngBounds(this.circle.getBounds());
        }

        getCenter() : ILatLng
        {
            return VRS.leafletUtilities.fromLeafletLatLng(this.circle.getLatLng());
        }
        setCenter(value: ILatLng)
        {
            this.circle.setLatLng(VRS.leafletUtilities.toLeafletLatLng(value));
        }

        getDraggable() : boolean
        {
            return false;
        }
        setDraggable(value: boolean)
        {
            ;
        }

        getEditable() : boolean
        {
            return false;
        }
        setEditable(value: boolean)
        {
            ;
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
            return this.visible;
        }
        setVisible(visible: boolean)
        {
            if(this.visible !== visible) {
                if(visible) {
                    this.circle.addTo(this.map);
                } else {
                    this.circle.removeFrom(this.map);
                }
                this.visible = visible;
            }
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
                this.circle.setStyle({
                    fillColor: value
                });
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
                this.circle.setStyle({
                    fillOpacity: value
                });
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
                this.circle.setStyle({
                    color: value
                });
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
                this.circle.setStyle({
                    opacity: value
                });
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
                this.circle.setStyle({
                    weight: value
                });
            }
        }

        private _ZIndex: number;
        getZIndex() : number
        {
            return this._ZIndex;
        }
        setZIndex(value: number)
        {
            this._ZIndex = value;
        }
    }

    class MapControl extends L.Control
    {
        constructor(readonly element: JQuery | HTMLElement, options: L.ControlOptions)
        {
            super(options);
        }

        onAdd(map: L.Map): HTMLElement
        {
            var result = $('<div class="leaflet-control"></div>');
            result.append(this.element);

            return result[0];
        }
    }

    /**
     * The state held for every map plugin object.
     */
    class MapPluginState
    {
        /**
         * The leaflet map.
         */
        map: L.Map = undefined;

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
         * An associative array of circle IDs to circles.
         */
        circles: { [circleId: string]: MapCircle } = {};
    }

    /**
     * A jQuery plugin that wraps the OpenStreetMap map.
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
            if(this.options.useServerDefaults && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    this.options.center = { lat: config.InitialLatitude, lng: config.InitialLongitude };
                    this.options.mapTypeId = config.InitialMapType;
                    this.options.zoom = config.InitialZoom;
                }
            }

            var state = this._getState();
            state.mapContainer = $('<div />')
                .addClass('vrsMap')
                .appendTo(this.element);

            if(this.options.afterCreate) {
                this.options.afterCreate(this);
            }
            if(this.options.openOnCreate) {
                this.open();
            }
        }

        _destroy()
        {
            var state = this._getState();

            if(VRS.refreshManager) VRS.refreshManager.unregisterTarget(this.element);

            if(state.mapContainer) state.mapContainer.remove();
        }

        getNative(): any
        {
            return this._getState().map;
        }

        getNativeType() : string
        {
            return 'OpenStreetMap';
        }

        isOpen() : boolean
        {
            return !!this._getState().map;
        }

        isReady() : boolean
        {
            var state = this._getState();
            return !!state.map;
        }

        getBounds() : IBounds
        {
            return this._getBounds(this._getState());
        }
        private _getBounds(state: MapPluginState)
        {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLngBounds(state.map.getBounds()) : { tlLat: 0, tlLng: 0, brLat: 0, brLng: 0};
        }

        getCenter() : ILatLng
        {
            return this._getCenter(this._getState());
        }
        private _getCenter(state: MapPluginState)
        {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLng(state.map.getCenter()) : this.options.center;
        }

        setCenter(latLng: ILatLng)
        {
            this._setCenter(this._getState(), latLng);
        }
        private _setCenter(state: MapPluginState, latLng: ILatLng)
        {
            if(state.map) state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(latLng));
            else          this.options.center = latLng;
        }

        getDraggable() : boolean
        {
            return this.options.draggable;
        }

        getMapType() : MapTypeEnum
        {
            return this._getMapType(this._getState());
        }
        private _getMapType(state: MapPluginState) : MapTypeEnum
        {
            return this.options.mapTypeId;
        }

        setMapType(mapType: MapTypeEnum)
        {
            this._setMapType(this._getState(), mapType);
        }
        private _setMapType(state: MapPluginState, mapType: MapTypeEnum)
        {
            this.options.mapTypeId = mapType;
        }

        getScrollWheel() : boolean
        {
            return this.options.scrollwheel;
        }

        getStreetView() : boolean
        {
            return this.options.streetViewControl;
        }

        getZoom() : number
        {
            return this._getZoom(this._getState());
        }
        private _getZoom(state: MapPluginState) : number
        {
            return state.map ? state.map.getZoom() : this.options.zoom;
        }

        setZoom(zoom: number)
        {
            this._setZoom(this._getState(), zoom);
        }
        private _setZoom(state: MapPluginState, zoom: number)
        {
            if(state.map) state.map.setZoom(zoom);
            else          this.options.zoom = zoom;
        }

        unhook(hookResult: IEventHandleJQueryUI)
        {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        }

        hookBoundsChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'boundsChanged', callback, forceThis);
        }
        private _raiseBoundsChanged()
        {
            this._trigger('boundsChanged');
        }

        hookCenterChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'centerChanged', callback, forceThis);
        }
        private _raiseCenterChanged()
        {
            this._trigger('centerChanged');
        }

        hookClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'clicked', callback, forceThis);
        }
        private _raiseClicked(mouseEvent: Event)
        {
            this._trigger('clicked', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        hookDoubleClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'doubleClicked', callback, forceThis);
        }
        private _raiseDoubleClicked(mouseEvent: Event)
        {
            this._trigger('doubleClicked', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        hookIdle(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'idle', callback, forceThis);
        }
        private _raiseIdle()
        {
            this._trigger('idle');
        }

        hookMapTypeChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mapTypeChanged', callback, forceThis);
        }
        private _raiseMapTypeChanged()
        {
            this._trigger('mapTypeChanged');
        }

        hookRightClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mouseEvent', callback, forceThis);
        }
        private _raiseRightClicked(mouseEvent: Event)
        {
            this._trigger('mouseEvent', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        hookTilesLoaded(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'tilesLoaded', callback, forceThis);
        }
        private _raiseTilesLoaded()
        {
            this._trigger('tilesLoaded');
        }

        hookZoomChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'zoomChanged', callback, forceThis);
        }
        private _raiseZoomChanged()
        {
            this._trigger('zoomChanged');
        }

        hookMarkerClicked(callback: (event?: Event, data?: IMapMarkerEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerClicked', callback, forceThis);
        }
        private _raiseMarkerClicked(id: string | number)
        {
            this._trigger('markerClicked', null, <IMapMarkerEventArgs>{ id: id });
        }

        hookMarkerDragged(callback: (event?: Event, data?: IMapMarkerEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerDragged', callback, forceThis);
        }
        private _raiseMarkerDragged(id: string | number)
        {
            this._trigger('markerDragged', null, <IMapMarkerEventArgs>{ id: id });
        }

        hookInfoWindowClosedByUser(callback: (event?: Event, data?: IMapInfoWindowEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'infoWindowClosedByUser', callback, forceThis);
        }
        private _raiseInfoWindowClosedByUser(id: string | number)
        {
            this._trigger('infoWindowClosedByUser', null, <IMapInfoWindowEventArgs>{ id: id });
        }

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

        open(userOptions?: IMapOpenOptions)
        {
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

            var leafletOptions: L.MapOptions = {
                attributionControl:     true,
                zoom:                   mapOptions.zoom,
                center:                 VRS.leafletUtilities.toLeafletLatLng(mapOptions.center),
                scrollWheelZoom:        mapOptions.scrollwheel,
                dragging:               mapOptions.draggable,
                zoomControl:            mapOptions.scaleControl
            };

            var state = this._getState();
            state.map = L.map(state.mapContainer[0], leafletOptions);

            L.tileLayer(VRS.serverConfig.get().OpenStreetMapTileServerUrl, {
                attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors, <a href="https://creativecommons.org/licenses/by-sa/2.0/">CC-BY-SA</a></a>',
                className: 'vrs-leaflet-tile-layer'
            }).addTo(state.map);

            var waitUntilReady = () => {
                if(this.options.waitUntilReady && !this.isReady()) {
                    setTimeout(waitUntilReady, 100);
                } else {
                    if(this.options.afterOpen) this.options.afterOpen(this);
                }
            };
            waitUntilReady();
        }

        private _userNotIdle()
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
        }

        refreshMap()
        {
            var state = this._getState();
            if(state.map) {
                state.map.invalidateSize();
            }
        }

        panTo(mapCenter: ILatLng)
        {
            this._panTo(mapCenter, this._getState());
        }
        private _panTo(mapCenter: ILatLng, state: MapPluginState)
        {
            if(state.map) state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(mapCenter));
            else          this.options.center = mapCenter;
        }

        fitBounds(bounds: IBounds)
        {
            var state = this._getState();
            if(state.map) {
                state.map.fitBounds(VRS.leafletUtilities.toLeaftletLatLngBounds(bounds));
            }
        }

        saveState()
        {
            var settings = this._createSettings();
            VRS.configStorage.save(this._persistenceKey(), settings);
        }

        loadState() : IMapSaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            return $.extend(this._createSettings(), savedSettings);
        }

        applyState(config: IMapSaveState)
        {
            config = config || <IMapSaveState>{};
            var state = this._getState();

            if(config.center)                       this._setCenter(state, config.center);
            if(config.zoom || config.zoom === 0)    this._setZoom(state, config.zoom);
            if(config.mapTypeId)                    this._setMapType(state, config.mapTypeId);
        };

        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        private _persistenceKey() : string
        {
            return 'vrsOpenStreetMapState-' + (this.options.name || 'default');
        }

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

        addMarker(id: string | number, userOptions: IMapMarkerSettings): IMapMarker
        {
            var result: MapMarker;

            var state = this._getState();
            if(state.map) {
                if(userOptions.zIndex === null || userOptions.zIndex === undefined) {
                    userOptions.zIndex = 0;
                }
                var leafletOptions: L.MarkerOptions = {
                    interactive:    userOptions.clickable !== undefined ? userOptions.clickable : true,
                    draggable:      userOptions.draggable !== undefined ? userOptions.draggable : false,
                    zIndexOffset:   userOptions.zIndex,
                };
                if(userOptions.icon) {
                    leafletOptions.icon = VRS.leafletUtilities.toLeafletIcon(userOptions.icon);
                }
                if(userOptions.tooltip) {
                    leafletOptions.title = userOptions.tooltip;
                }

                var position = userOptions.position ? VRS.leafletUtilities.toLeafletLatLng(userOptions.position) : state.map.getCenter();

                this.destroyMarker(id);
                var nativeMarker = L.marker(position, leafletOptions);
                if(userOptions.visible) {
                    nativeMarker.addTo(state.map);
                }
                result = new MapMarker(id, state.map, nativeMarker, leafletOptions, !!userOptions.useMarkerWithLabel, userOptions.visible, userOptions.tag);
                state.markers[id] = result;
            }

            return result;
        }

        getMarker(idOrMarker: string | number | IMapMarker): IMapMarker
        {
            if(idOrMarker instanceof MapMarker) return idOrMarker;
            var state = this._getState();
            return state.markers[<string | number>idOrMarker];
        }

        destroyMarker(idOrMarker: string | number | IMapMarker)
        {
            var state = this._getState();
            var marker = <MapMarker>this.getMarker(idOrMarker);
            if(marker) {
                marker.setVisible(false);
                marker.marker = null;
                marker.map = null;
                marker.tag = null;
                delete state.markers[marker.id];
                marker.id = null;
            }
        }

        centerOnMarker(idOrMarker: string | number | IMapMarker)
        {
            var state = this._getState();
            var marker = <MapMarker>this.getMarker(idOrMarker);
            if(marker) {
                this.setCenter(marker.getPosition());
            }
        }

        createMapMarkerClusterer(settings?: IMapMarkerClustererSettings): IMapMarkerClusterer
        {
            return null;
        }

        addPolyline(id: string | number, userOptions: IMapPolylineSettings): IMapPolyline
        {
            var result: MapPolyline;

            var state = this._getState();
            if(state.map) {
                var options = $.extend(<IMapPolylineSettings>{}, userOptions, {
                    visible: true
                });
                var leafletOptions: L.PolylineOptions = {
                    color:      options.strokeColour || '#000000'
                };
                if(options.strokeOpacity || leafletOptions.opacity === 0) leafletOptions.opacity = options.strokeOpacity;
                if(options.strokeWeight || leafletOptions.weight === 0) leafletOptions.weight = options.strokeWeight;

                var path: L.LatLng[] = [];
                if(options.path) path = VRS.leafletUtilities.toLeafletLatLngArray(options.path);

                this.destroyPolyline(id);
                var polyline = L.polyline(path, leafletOptions);
                if(options.visible) {
                    polyline.addTo(state.map);
                }

                result = new MapPolyline(id, state.map, polyline, options.tag, {
                    strokeColour:   options.strokeColour,
                    strokeOpacity:  options.strokeOpacity,
                    strokeWeight:   options.strokeWeight
                });
                state.polylines[id] = result;
            }

            return result;
        }

        getPolyline(idOrPolyline: string | number | IMapPolyline): IMapPolyline
        {
            if(idOrPolyline instanceof MapPolyline) return idOrPolyline;
            var state = this._getState();
            return state.polylines[<string | number>idOrPolyline];
        }

        destroyPolyline(idOrPolyline: string | number | IMapPolyline)
        {
            var state = this._getState();
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            if(polyline) {
                polyline.setVisible(false);
                polyline.polyline = null;
                polyline.map = null;
                polyline.tag = null;
                delete state.polylines[polyline.id];
                polyline.id = null;
            }
        }

        trimPolyline(idOrPolyline: string | number | IMapPolyline, countPoints: number, fromStart: boolean): IMapTrimPolylineResult
        {
            var emptied = false;
            var countRemoved = 0;

            if(countPoints > 0) {
                var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
                var points = <L.LatLng[]>polyline.polyline.getLatLngs();
                var length = points.length;
                if(length < countPoints) countPoints = length;
                if(countPoints > 0) {
                    countRemoved = countPoints;
                    emptied = countPoints === length;

                    if(emptied) {
                        points = [];
                    } else {
                        if(fromStart) {
                            points.splice(0, countPoints);
                        } else {
                            points.splice(length - countPoints, countPoints);
                        }
                    }
                    polyline.polyline.setLatLngs(points);
                }
            }

            return { emptied: emptied, countRemoved: countRemoved };
        }

        removePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number)
        {
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            var points = <L.LatLng[]>polyline.polyline.getLatLngs();
            points.splice(index, 1);
            polyline.polyline.setLatLngs(points);
        }

        appendToPolyline(idOrPolyline: string | number | IMapPolyline, path: ILatLng[], toStart: boolean)
        {
            var length = !path ? 0 : path.length;
            if(length > 0) {
                var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
                var points = <L.LatLng[]>polyline.polyline.getLatLngs();
                var insertAt = toStart ? 0 : -1;
                for(var i = 0;i < length;++i) {
                    var leafletPoint = VRS.leafletUtilities.toLeafletLatLng(path[i]);
                    if(toStart) {
                        points.splice(insertAt++, 0, leafletPoint);
                    } else {
                        points.push(leafletPoint);
                    }
                }
                polyline.polyline.setLatLngs(points);
            }
        }

        replacePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number, point: ILatLng)
        {
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            var points = <L.LatLng[]>polyline.polyline.getLatLngs();
            var length = points.length;
            if(index === -1) index = length - 1;
            if(index >= 0 && index < length) {
                points.splice(index, 1, VRS.leafletUtilities.toLeafletLatLng(point));
                polyline.polyline.setLatLngs(points);
            }
        }

        addPolygon(id: string | number, userOptions: IMapPolygonSettings): IMapPolygon
        {
            return null;
        }

        getPolygon(idOrPolygon: string | number | IMapPolygon): IMapPolygon
        {
            return null;
        }

        destroyPolygon(idOrPolygon: string | number | IMapPolygon)
        {
            ;
        }

        addCircle(id: string | number, userOptions: IMapCircleSettings): IMapCircle
        {
            var result: MapCircle = null;

            var state = this._getState();
            if(state.map) {
                var options = $.extend(<IMapPolylineSettings>{}, userOptions, {
                    visible: true
                });
                var leafletOptions: L.CircleMarkerOptions = {
                    fillColor:      '#000',
                    fillOpacity:    0,
                    color:          '#000',
                    opacity:        1,
                    weight:         1,
                    radius:         options.radius || 0
                };
                var centre = VRS.leafletUtilities.toLeafletLatLng(options.center);

                this.destroyCircle(id);
                var circle = L.circle(centre, leafletOptions);
                if(options.visible) {
                    circle.addTo(state.map);
                }
                result = new MapCircle(id, state.map, circle, options.tag, options);
                state.circles[id] = result;
            }

            return result;
        }

        getCircle(idOrCircle: string | number | IMapCircle): IMapCircle
        {
            if(idOrCircle instanceof MapCircle) return idOrCircle;
            var state = this._getState();
            return state.circles[<string | number>idOrCircle];
        }

        destroyCircle(idOrCircle: string | number | IMapCircle)
        {
            var state = this._getState();
            var circle = <MapCircle>this.getCircle(idOrCircle);
            if(circle) {
                circle.setVisible(false);
                circle.circle = null;
                circle.map = null;
                circle.tag = null;
                delete state.circles[circle.id];
                circle.id = null;
            }
        }

        getUnusedInfoWindowId(): string
        {
            return null;
        }

        addInfoWindow(id: string | number, userOptions: IMapInfoWindowSettings): IMapInfoWindow
        {
            return null;
        }

        getInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow): IMapInfoWindow
        {
            return null;
        }

        destroyInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow)
        {
            ;
        }

        openInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow, mapMarker?: IMapMarker)
        {
            ;
        }

        closeInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow)
        {
            ;
        }

        addControl(element: JQuery | HTMLElement, mapPosition: MapPositionEnum)
        {
            var state = this._getState();
            if(state.map) {
                var controlOptions: L.ControlOptions = {
                    position: VRS.leafletUtilities.toLeafletMapPosition(mapPosition)
                };
                var control = new MapControl(element, controlOptions);
                control.addTo(state.map);
            }
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
