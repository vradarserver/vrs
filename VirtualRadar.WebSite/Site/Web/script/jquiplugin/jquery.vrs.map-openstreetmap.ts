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

    export class LeafletUtilities
    {
        fromLeafletLatLng(latLng: L.LatLng) : ILatLng
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
            if(state.map) state.map.setView(VRS.leafletUtilities.toLeafletLatLng(latLng), state.map.getZoom());
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
                zoom:                   mapOptions.zoom,
                center:                 VRS.leafletUtilities.toLeafletLatLng(mapOptions.center),
                scrollWheelZoom:        mapOptions.scrollwheel,
                dragging:               mapOptions.draggable,
                zoomControl:            mapOptions.scaleControl
            };

            var state = this._getState();
            state.map = L.map(state.mapContainer[0], leafletOptions);

            L.tileLayer(VRS.serverConfig.get().OpenStreetMapTileServerUrl).addTo(state.map);

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
            return null;
        }

        getMarker(idOrMarker: string | number | IMapMarker): IMapMarker
        {
            return null;
        }

        destroyMarker(idOrMarker: string | number | IMapMarker)
        {
            ;
        }

        centerOnMarker(idOrMarker: string | number | IMapMarker)
        {
            ;
        }

        createMapMarkerClusterer(settings?: IMapMarkerClustererSettings): IMapMarkerClusterer
        {
            return null;
        }

        addPolyline(id: string | number, userOptions: IMapPolylineSettings): IMapPolyline
        {
            return null;
        }

        getPolyline(idOrPolyline: string | number | IMapPolyline): IMapPolyline
        {
            return null;
        }

        destroyPolyline(idOrPolyline: string | number | IMapPolyline)
        {
            ;
        }

        trimPolyline(idOrPolyline: string | number | IMapPolyline, countPoints: number, fromStart: boolean): IMapTrimPolylineResult
        {
            return null;
        }

        removePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number)
        {
            ;
        }

        appendToPolyline(idOrPolyline: string | number | IMapPolyline, path: ILatLng[], toStart: boolean)
        {
            ;
        }

        replacePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number, point: ILatLng)
        {
            ;
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
            return null;
        }

        getCircle(idOrCircle: string | number | IMapCircle): IMapCircle
        {
            return null;
        }

        destroyCircle(idOrCircle: string | number | IMapCircle)
        {
            ;
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
            ;
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
