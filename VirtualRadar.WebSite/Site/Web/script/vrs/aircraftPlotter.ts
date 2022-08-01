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
 * @fileoverview Code that can handle the plotting of aircraft onto a map.
 */

namespace VRS
{
    /**
     * The object to use when creating a new instance of AircraftMarker.
     */
    export interface AircraftMarker_Settings
    {
        // The raster image URL for the aircraft
        folder?: string;
        normalFileName?: string;
        selectedFileName?: string;

        // The embedded SVG for the aircraft
        embeddedSvg?:           EmbeddedSvg;
        svgFillColourCallback?: (aircraft: Aircraft, isSelected: boolean) => string;

        // Settings common to all aircraft markers
        size?: ISize;
        isAircraft?: boolean;
        canRotate?: boolean;
        isPre22Icon?: boolean;
        matches?: (aircraft: Aircraft) => boolean;
    }

    /**
     * Describes the properties of an aircraft marker.
     */
    export class AircraftMarker
    {
        private _Settings: AircraftMarker_Settings;

        constructor(settings: AircraftMarker_Settings)
        {
            this._Settings = $.extend({
                folder:                 'images/web-markers',
                normalFileName :        null,
                selectedFileName:       settings.normalFileName || null,
                embeddedSvg:            null,
                svgFillColourCallback:  (aircraft, isSelected) => isSelected ? VRS.globalOptions.svgAircraftMarkerSelectedFill : VRS.globalOptions.svgAircraftMarkerNormalFill,
                size:                   { width: 35, height: 35 },
                isAircraft:             true,
                canRotate:              true,
                isPre22Icon:            false
            }, settings);
        }

        getFolder()
        {
            return this._Settings.folder;
        }
        setFolder(value: string)
        {
            this._Settings.folder = value;
        }

        getNormalFileName()
        {
            return this._Settings.normalFileName;
        }
        setNormalFileName(value: string)
        {
            this._Settings.normalFileName = value;
        }

        getSelectedFileName()
        {
            return this._Settings.selectedFileName;
        }
        setSelectedFileName(value: string)
        {
            this._Settings.selectedFileName = value;
        }

        getSize()
        {
            return this._Settings.size;
        }
        setSize(value: ISize)
        {
            this._Settings.size = value;
        }

        getIsAircraft()
        {
            return this._Settings.isAircraft;
        }
        setIsAircraft(value: boolean)
        {
            this._Settings.isAircraft = value;
        }

        getCanRotate()
        {
            return this._Settings.canRotate;
        }
        setCanRotate(value: boolean)
        {
            this._Settings.canRotate = value;
        }

        getIsPre22Icon()
        {
            return this._Settings.isPre22Icon;
        }
        setIsPre22Icon(value: boolean)
        {
            this._Settings.isPre22Icon = value;
        }

        getMatches()
        {
            return this._Settings.matches;
        }
        setMatches(value: (aircraft: Aircraft) => boolean)
        {
            this._Settings.matches = value;
        }

        getEmbeddedSvg()
        {
            return this._Settings.embeddedSvg;
        }
        setEmbeddedSvg(value: EmbeddedSvg)
        {
            this._Settings.embeddedSvg = value;
        }

        getSvgFillColourCallback()
        {
            return this._Settings.svgFillColourCallback;
        }
        setSvgFillColourCallback(value: (Aircraft, boolean) => string)
        {
            this._Settings.svgFillColourCallback = value;
        }

        /**
         * Returns true if the marker can be used to represent the aircraft passed across.
         */
        matchesAircraft(aircraft: Aircraft) : boolean
        {
            return this._Settings.matches ? this._Settings.matches(aircraft) : false;
        }

        /**
         * Returns true if the aircraft marker should be rendered browser-side using SVG.
         */
        useEmbeddedSvg()
        {
            return SvgGenerator.useSvgGraphics() && this._Settings.embeddedSvg;
        }

        /**
         * Returns the fill colour for embedded SVGs.
         * @param aircraft
         * @param isSelected
         */
        getSvgFillColour(aircraft: Aircraft, isSelected: boolean) : string
        {
            return this._Settings.svgFillColourCallback(aircraft, isSelected);
        }
    }

    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftMarkerPinTextWidth = VRS.globalOptions.aircraftMarkerPinTextWidth || 68;                  // The width for markers that are showing pin text.
    VRS.globalOptions.aircraftMarkerPinTextLineHeight = VRS.globalOptions.aircraftMarkerPinTextLineHeight || 12;        // The height added to each marker for each line of pin text.
    VRS.globalOptions.aircraftMarkerRotate = VRS.globalOptions.aircraftMarkerRotate !== undefined ? VRS.globalOptions.aircraftMarkerRotate : true;     // True to rotate aircraft markers to follow the aircraft heading, false otherwise.
    VRS.globalOptions.aircraftMarkerRotationGranularity = VRS.globalOptions.aircraftMarkerRotationGranularity || 5;     // The smallest number of degrees that an aircraft's heading will be rotated when displaying its marker.
    VRS.globalOptions.aircraftMarkerAllowAltitudeStalk = VRS.globalOptions.aircraftMarkerAllowAltitudeStalk !== undefined ? VRS.globalOptions.aircraftMarkerAllowAltitudeStalk : true;  // True if altitude stalks can be shown, false if they are permanently suppressed.
    VRS.globalOptions.aircraftMarkerShowAltitudeStalk = VRS.globalOptions.aircraftMarkerShowAltitudeStalk !== undefined ? VRS.globalOptions.aircraftMarkerShowAltitudeStalk : true;     // True if altitude stalks are to be shown, false if they are to be suppressed.
    VRS.globalOptions.aircraftMarkerAllowPinText = VRS.globalOptions.aircraftMarkerAllowPinText !== undefined ? VRS.globalOptions.aircraftMarkerAllowPinText : true;  // True to allow the user to display pin text on the markers. This can be overridden by server options.
    VRS.globalOptions.aircraftMarkerDefaultPinTexts = VRS.globalOptions.aircraftMarkerDefaultPinTexts ||                // An array of VRS.RenderProperty entries to use for browsers that have never accessed the site before.
        [
            VRS.RenderProperty.Registration,
            VRS.RenderProperty.Callsign,
            VRS.RenderProperty.Altitude
        ];
    VRS.globalOptions.aircraftMarkerPinTextLines = VRS.globalOptions.aircraftMarkerPinTextLines !== undefined ? VRS.globalOptions.aircraftMarkerPinTextLines : 3;   // The number of lines of pin text to use.
    VRS.globalOptions.aircraftMarkerMaximumPinTextLines = VRS.globalOptions.aircraftMarkerMaximumPinTextLines !== undefined ? VRS.globalOptions.aircraftMarkerMaximumPinTextLines : 6;  // The maximum number of pin text lines to allow.
    VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines = VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines !== undefined ? VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines : false;    // True to hide blank pintext lines.
    VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed = VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed !== undefined ? VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed : true;   // True to suppress the altitude stalk when zoomed out, false to always show it.
    VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkZoomLevel = VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkZoomLevel || 7;   // The map zoom level at which altitude stalks will be suppressed.
    VRS.globalOptions.aircraftMarkerTrailColourNormal = VRS.globalOptions.aircraftMarkerTrailColourNormal || '#000040';     // The colour of trails for aircraft that are not selected.
    VRS.globalOptions.aircraftMarkerTrailColourSelected = VRS.globalOptions.aircraftMarkerTrailColourSelected || '#202080'; // The colour of trails for aircraft that are selected.
    VRS.globalOptions.aircraftMarkerTrailWidthNormal = VRS.globalOptions.aircraftMarkerTrailWidthNormal || 2;           // The width in pixels of trails for aircraft that are not selected.
    VRS.globalOptions.aircraftMarkerTrailWidthSelected = VRS.globalOptions.aircraftMarkerTrailWidthSelected || 3;       // The width in pixels of trails for aircraft that are selected.
    VRS.globalOptions.aircraftMarkerTrailDisplay = VRS.globalOptions.aircraftMarkerTrailDisplay || VRS.TrailDisplay.SelectedOnly;   // The default setting for whether to show trails on all aircraft, the selected aircraft only or no aircraft. Note that trails are still sent by the server even when no aircraft is selected.
    VRS.globalOptions.aircraftMarkerTrailType = VRS.globalOptions.aircraftMarkerTrailType || VRS.TrailType.Full;                    // The type of trail to show by default.
    VRS.globalOptions.aircraftMarkerShowTooltip = VRS.globalOptions.aircraftMarkerShowTooltip !== undefined ? VRS.globalOptions.aircraftMarkerShowTooltip : true;   // True to show tooltips on aircraft markers, false otherwise.
    VRS.globalOptions.aircraftMarkerMovingMapOn = VRS.globalOptions.aircraftMarkerMovingMapOn !== undefined ? VRS.globalOptions.aircraftMarkerMovingMapOn : false;    // True if the moving map is switched on by default, false if it is not.
    VRS.globalOptions.aircraftMarkerSuppressTextOnImages = VRS.globalOptions.aircraftMarkerSuppressTextOnImages !== undefined ? VRS.globalOptions.aircraftMarkerSuppressTextOnImages : undefined;   // Forces the use of labels to draw pin text instead of adding text to the graphics. Can reduce server load but doesn't look as nice as text on images.
    VRS.globalOptions.aircraftMarkerAllowRangeCircles = VRS.globalOptions.aircraftMarkerAllowRangeCircles !== undefined ? VRS.globalOptions.aircraftMarkerAllowRangeCircles : true; // True if range circles are to be allowed, false if they are to be suppressed.
    VRS.globalOptions.aircraftMarkerShowRangeCircles = VRS.globalOptions.aircraftMarkerShowRangeCircles !== undefined ? VRS.globalOptions.aircraftMarkerShowRangeCircles : false;   // True if range circles are to be shown by default, false if they are not.
    VRS.globalOptions.aircraftMarkerRangeCircleInterval = VRS.globalOptions.aircraftMarkerRangeCircleInterval || 20;    // The number of distance units between each successive range circle.
    VRS.globalOptions.aircraftMarkerRangeCircleDistanceUnit = VRS.globalOptions.aircraftMarkerRangeCircleDistanceUnit || VRS.Distance.StatuteMile;  // The distance units for the range circle intervals.
    VRS.globalOptions.aircraftMarkerRangeCircleCount = VRS.globalOptions.aircraftMarkerRangeCircleCount || 6;           // The number of range circles to display around the current location.
    VRS.globalOptions.aircraftMarkerRangeCircleOddColour = VRS.globalOptions.aircraftMarkerRangeCircleOddColour || '#333333';   // The CSS colour for the odd range circles.
    VRS.globalOptions.aircraftMarkerRangeCircleEvenColour = VRS.globalOptions.aircraftMarkerRangeCircleEvenColour || '#111111'; // The CSS colour for the even range circles.
    VRS.globalOptions.aircraftMarkerRangeCircleOddWeight =  VRS.globalOptions.aircraftMarkerRangeCircleOddWeight || 1;          // The width in pixels for the odd range circles.
    VRS.globalOptions.aircraftMarkerRangeCircleEvenWeight = VRS.globalOptions.aircraftMarkerRangeCircleEvenWeight || 2;         // The width in pixels for the even range circles.
    VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles = VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles || 9;         // The maximum number of circles that the user request.
    VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval = VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval || 100;     // The maximum interval that the user can request for a range circle.
    VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight || 4;           // The maximum weight that the user can specify for a range circle.
    VRS.globalOptions.aircraftMarkerAltitudeTrailLow = VRS.globalOptions.aircraftMarkerAltitudeTrailLow !== undefined ? VRS.globalOptions.aircraftMarkerAltitudeTrailLow : 300; // The low range to use when colouring altitude trails, in feet.
    VRS.globalOptions.aircraftMarkerAltitudeTrailHigh = VRS.globalOptions.aircraftMarkerAltitudeTrailHigh || 45000;             // The high range to use when colouring altitude trails, in feet.
    VRS.globalOptions.aircraftMarkerSpeedTrailLow = VRS.globalOptions.aircraftMarkerSpeedTrailLow !== undefined ? VRS.globalOptions.aircraftMarkerSpeedTrailLow : 10;   // The low range to use when colouring speed trails, in knots.
    VRS.globalOptions.aircraftMarkerSpeedTrailHigh = VRS.globalOptions.aircraftMarkerSpeedTrailHigh || 660;                     // The high range to use when colouring speed trails, in knots.
    VRS.globalOptions.aircraftMarkerAlwaysPlotSelected = VRS.globalOptions.aircraftMarkerAlwaysPlotSelected !== undefined ? VRS.globalOptions.aircraftMarkerAlwaysPlotSelected : true;  // Always plot the selected aircraft, even if it is not on the map. This preserves the selected aircraft's trail.
    VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel = VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel != undefined ? VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel : 13;   // Ground vehicles and towers are only shown when the zoom level is this value or above. Set to 0 to always show them, 99999 to never show them.
    VRS.globalOptions.aircraftMarkerShowNonAircraftTrails = VRS.globalOptions.aircraftMarkerShowNonAircraftTrails !== undefined ? VRS.globalOptions.aircraftMarkerShowNonAircraftTrails : false; // True if trails are to be shown for ground vehicles and towers, false if they are not.
    VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons = VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons !== undefined ? VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons : false; // True if we should only show the old style (pre-version 2.2) aircraft markers.
    VRS.globalOptions.aircraftMarkerClustererEnabled = VRS.globalOptions.aircraftMarkerClustererEnabled !== false;                      // True if the marker clusterer is to be used.
    VRS.globalOptions.aircraftMarkerClustererMaxZoom = VRS.globalOptions.aircraftMarkerClustererMaxZoom || 5;                           // The maximum zoom level at which to cluster map markers or null if there is no maximum.
    VRS.globalOptions.aircraftMarkerClustererMinimumClusterSize = VRS.globalOptions.aircraftMarkerClustererMinimumClusterSize || 1;     // The minimum number of adjacent markers in a map marker cluster.
    VRS.globalOptions.aircraftMarkerClustererUserCanConfigure = VRS.globalOptions.aircraftMarkerClustererUserCanConfigure !== false;    // True if the user can configure the map marker clusterer.

    // The order in which these appear in the list is important. Earlier items take precedence over later items.
    VRS.globalOptions.aircraftMarkers = VRS.globalOptions.aircraftMarkers || [
        new VRS.AircraftMarker({
            normalFileName:     'GroundVehicle.png',
            selectedFileName:   'GroundVehicle.png',
            embeddedSvg:        EmbeddedSvgs.Marker_GroundVehicle,
            size:               { width: 26, height: 24},
            isAircraft:         false,
            isPre22Icon:        true,
            matches:            function(aircraft) { return aircraft.species.val === VRS.Species.GroundVehicle; }
        }),
        new VRS.AircraftMarker ({
            normalFileName:     'Tower.png',
            selectedFileName:   'Tower.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Tower,
            size:               { width: 20, height: 20 },
            isAircraft:         false,
            isPre22Icon:        true,
            canRotate:          false,
            matches:            function(aircraft) { return aircraft.species.val === VRS.Species.Tower; }
        }),
        new VRS.AircraftMarker ({
            normalFileName:     'Helicopter.png',
            selectedFileName:   'Helicopter-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Helicopter,
            size:               { width: 32, height: 32 },
            matches:            function(aircraft) { return aircraft.species.val === VRS.Species.Helicopter; }
        }),
        new VRS.AircraftMarker ({
            normalFileName:     'Balloon.png',
            selectedFileName:   'Balloon-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Balloon,
            size:               { width: 20, height: 25 },
            canRotate:          false,
            matches:            function(aircraft) { return aircraft.modelIcao.val === 'BALL'; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'Type-GLID.png',
            selectedFileName:   'Type-GLID-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_TypeGLID,
            size:               { width: 60, height: 60},
            matches:            function(aircraft) { return aircraft.modelIcao.val === 'GLID'; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'Type-A380.png',
            selectedFileName:   'Type-A380-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_TypeA380,
            size:               { width: 60, height: 60},
            matches:            function(aircraft) { return aircraft.modelIcao.val === 'A388'; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'Type-A340.png',
            selectedFileName:   'Type-A340-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_TypeA340,
            size:               { width: 60, height: 60},
            matches:            function(aircraft) { return aircraft.modelIcao.val && (aircraft.modelIcao.val === 'E6' || (aircraft.modelIcao.val.length === 4 && aircraft.modelIcao.val.substring(0, 3) === 'A34')); }
        }),
        new VRS.AircraftMarker ({
            normalFileName:     'WTC-Light-1-Prop.png',
            selectedFileName:   'WTC-Light-1-Prop-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Light1Prop,
            size:               { width: 32, height: 32 },
            matches:            function(aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Light && aircraft.engineType.val !== VRS.EngineType.Jet && aircraft.countEngines.val === '1'; }
        }),
        new VRS.AircraftMarker ({
            normalFileName:     'WTC-Light-2-Prop.png',
            selectedFileName:   'WTC-Light-2-Prop-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Light2Prop,
            size:               { width: 36, height: 36 },
            matches:            function(aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Light && aircraft.engineType.val !== VRS.EngineType.Jet; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'Type-GLFx.png',
            selectedFileName:   'Type-GLFx-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_TypeGLFx,
            size:               { width: 40, height: 40},
            matches:            function(aircraft) {
                return aircraft.engineType.val === VRS.EngineType.Jet &&
                (
                    aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Light || 
                    (aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Medium && aircraft.enginePlacement.val === VRS.EnginePlacement.AftMounted)
                );
            }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'WTC-Medium-4-Jet.png',
            selectedFileName:   'WTC-Medium-4-Jet-Selected.png',
            embeddedSvg:        VRS.EmbeddedSvgs.Marker_Medium4Jet,
            size:               { width: 40, height: 40 },
            matches:            function(aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Medium && aircraft.countEngines.val === '4' && aircraft.engineType.val === VRS.EngineType.Jet; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'WTC-Medium-2-Jet.png',
            selectedFileName:   'WTC-Medium-2-Jet-Selected.png',
            embeddedSvg:        VRS.EmbeddedSvgs.Marker_Medium2Jet,
            size:               { width: 40, height: 40 },
            matches:            function(aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Medium && aircraft.countEngines.val !== '4' && aircraft.engineType.val === VRS.EngineType.Jet; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'WTC-Medium-2-Turbo.png',
            selectedFileName:   'WTC-Medium-2-Turbo-Selected.png',
            embeddedSvg:        VRS.EmbeddedSvgs.Marker_Medium2TurboProp,
            size:               { width: 40, height: 40 },
            matches:            function(aircraft) { return (<any>aircraft.countEngines.val < 4 || isNaN(<any>aircraft.countEngines.val)) && aircraft.engineType.val === VRS.EngineType.Turbo; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     '4-TurboProp.png',
            selectedFileName:   '4-TurboPropSelected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_4TurboProp,
            size:               { width: 40, height: 40 },
            matches:            function(aircraft) { return aircraft.engineType.val === VRS.EngineType.Turbo; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'WTC-Heavy-2-Jet.png',
            selectedFileName:   'WTC-Heavy-2-Jet-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Heavy2Jet,
            size:               { width: 57, height: 57 },
            matches:            function(aircraft) { return (<any>aircraft.countEngines.val < 4 || isNaN(<any>aircraft.countEngines.val)) && aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Heavy; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'WTC-Heavy-4-Jet.png',
            selectedFileName:   'WTC-Heavy-4-Jet-Selected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Heavy4Jet,
            size:               { width: 60, height: 60 },
            matches:            function(aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Heavy; }
        }),
        new VRS.AircraftMarker({
            normalFileName:     'Airplane.png',
            selectedFileName:   'AirplaneSelected.png',
            embeddedSvg:        EmbeddedSvgs.Marker_Generic,
            size:               { width: 35, height: 35 },
            isPre22Icon:        true,
            matches:            function() { return true; }
        })
    ];

    /**
     * Describes an aircraft that we are plotting on the map.
     */
    class PlottedDetail
    {
        private _Id: number;

        constructor(public aircraft: Aircraft)
        {
            this._Id = aircraft.id;
        }

        /**
         * The unique ID of the aircraft.
         */
        get id()
        {
            return this._Id;
        }

        resetId()
        {
            this._Id = null;
        }

        /**
         * The map marker for the aircraft.
         */
        mapMarker: IMapMarker;

        /**
         * The image displayed at the map marker for the aircraft.
         */
        mapIcon: IMapIcon;

        /**
         * The array of lines that describe the trail behind the aircraft.
         */
        mapPolylines: IMapPolyline[] = [];

        /**
         * A counter that is incremented every time a polyline is created, and is used to ensure that each polyline has
         * a unique ID.
         */
        nextPolylineId: number = 0;

        /**
         * The degree of rotation of the aircraft's image.
         */
        iconRotation: number;

        /**
         * The height of the altitude stalk in pixels.
         */
        iconAltitudeStalkHeight: number;

        /**
         * An array of strings drawn onto the map marker.
         */
        pinTexts: string[] = [];

        /**
         * The URL of the map marker image.
         */
        iconUrl: string;

        /**
         * The aircraft's updateCounter as-at the last time the trail was redrawn for the aircraft.
         */
        polylinePathUpdateCounter: number = -1;

        /**
         * A VRS.TrailType value indicating what kind of trail, if any, is currently drawn for the aircraft.
         */
        polylineTrailType: TrailTypeEnum;

        /**
         * True if the marker is an embedded SVG, false if it is a bitmap.
         */
        isSvg: boolean = false;
    }

    /**
     * The settings to use when creating new instances of AircraftPlotterOptions.
     */
    export interface AircraftPlotterOptions_Settings
    {
        /**
         * The name to use when saving and loading state.
         */
        name?: string;

        /**
         * The map that will be used as a source of zoom level for configuring the map clusterer.
         */
        map?: VRS.IMap;

        /**
         * True to show altitude stalks, false otherwise.
         */
        showAltitudeStalk?: boolean;

        /**
         * True to suppress the display of altitude stalks when zoomed, false otherwise.
         */
        suppressAltitudeStalkWhenZoomed?: boolean;

        /**
         * True to show text on the markers, false otherwise.
         */
        showPinText?: boolean;

        /**
         * The pin texts to show on the aircraft markers.
         */
        pinTexts?: RenderPropertyEnum[];

        /**
         * The number of pin text lines to show.
         */
        pinTextLines?: number;

        /**
         * True to suppress blank lines in pin text.
         */
        hideEmptyPinTextLines?: boolean;

        /**
         * Determines which aircraft are given trails.
         */
        trailDisplay?: TrailDisplayEnum;

        /**
         * Determines what kind of trail is shown on aircraft.
         */
        trailType?: TrailTypeEnum;

        /**
         * True if range circles are to be shown on the map.
         */
        showRangeCircles?: boolean;

        /**
         * The distance between each range circle.
         */
        rangeCircleInterval?: number;

        /**
         * The unit that rangeCircleInterval is in.
         */
        rangeCircleDistanceUnit?: DistanceEnum;

        /**
         * The number of range circles to show.
         */
        rangeCircleCount?: number;

        /**
         * The CSS colour for odd range circles.
         */
        rangeCircleOddColour?: string;

        /**
         * The pixel width of odd range circles.
         */
        rangeCircleOddWeight?: number;

        /**
         * The CSS colour for even range circles.
         */
        rangeCircleEvenColour?: string;

        /**
         * The pixel width of even range circles.
         */
        rangeCircleEvenWeight?: number;

        /**
         * True to only show the old-school aircraft markers.
         */
        onlyUsePre22Icons?: boolean;

        /**
         * The zoom level at which the clusterer will start clustering.
         */
        aircraftMarkerClustererMaxZoom?: number;
    }

    /**
     * The object that AircraftPlotterOptions state is recorded in.
     */
    export interface AircraftPlotterOptions_SaveState
    {
        showAltitudeStalk:                  boolean;
        suppressAltitudeStalkWhenZoomedOut: boolean;
        showPinText:                        boolean;
        pinTexts:                           RenderPropertyEnum[];
        pinTextLines:                       number;
        hideEmptyPinTextLines:              boolean;
        trailDisplay:                       TrailDisplayEnum;
        trailType:                          TrailTypeEnum;
        showRangeCircles:                   boolean;
        rangeCircleInterval:                number;
        rangeCircleDistanceUnit:            DistanceEnum;
        rangeCircleCount:                   number;
        rangeCircleOddColour:               string;
        rangeCircleOddWeight:               number;
        rangeCircleEvenColour:              string;
        rangeCircleEvenWeight:              number;
        onlyUsePre22Icons:                  boolean;
        aircraftMarkerClustererMaxZoom:     number;
    }

    /**
     * Collects together the options for an aircraft plotter.
     */
    export class AircraftPlotterOptions implements ISelfPersist<AircraftPlotterOptions_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftPlotterOptions'
        });
        private _Events = {
            propertyChanged:            'propertyChanged',
            rangeCirclePropertyChanged: 'rangeCirclePropertyChanged'
        };
        private _Settings: AircraftPlotterOptions_Settings;

        private _SuppressEvents = false;                        // True if attempts to raise events are to be suppressed.
        private _PinTexts: RenderPropertyEnum[] = [];           // An array of VRS.RenderProperty values representing values to draw onto the marker.

        constructor(settings: AircraftPlotterOptions_Settings)
        {
            this._Settings = $.extend({
                name:                               'default',
                map:                                null,
                showAltitudeStalk:                  VRS.globalOptions.aircraftMarkerAllowAltitudeStalk && VRS.globalOptions.aircraftMarkerShowAltitudeStalk,
                suppressAltitudeStalkWhenZoomed:    VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkWhenZoomed,
                showPinText:                        VRS.globalOptions.aircraftMarkerAllowPinText,
                pinTexts:                           VRS.globalOptions.aircraftMarkerDefaultPinTexts,
                pinTextLines:                       VRS.globalOptions.aircraftMarkerPinTextLines,
                hideEmptyPinTextLines:              VRS.globalOptions.aircraftMarkerHideEmptyPinTextLines,
                trailDisplay:                       VRS.globalOptions.aircraftMarkerTrailDisplay,
                trailType:                          VRS.globalOptions.aircraftMarkerTrailType,
                showRangeCircles:                   VRS.globalOptions.aircraftMarkerShowRangeCircles,
                rangeCircleInterval:                VRS.globalOptions.aircraftMarkerRangeCircleInterval,
                rangeCircleDistanceUnit:            VRS.globalOptions.aircraftMarkerRangeCircleDistanceUnit,
                rangeCircleCount:                   VRS.globalOptions.aircraftMarkerRangeCircleCount,
                rangeCircleOddColour:               VRS.globalOptions.aircraftMarkerRangeCircleOddColour,
                rangeCircleOddWeight:               VRS.globalOptions.aircraftMarkerRangeCircleOddWeight,
                rangeCircleEvenColour:              VRS.globalOptions.aircraftMarkerRangeCircleEvenColour,
                rangeCircleEvenWeight:              VRS.globalOptions.aircraftMarkerRangeCircleEvenWeight,
                onlyUsePre22Icons:                  VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons,
                aircraftMarkerClustererMaxZoom:     VRS.globalOptions.aircraftMarkerClustererMaxZoom
            }, settings);

            $.each(this._Settings.pinTexts, (idx, renderProperty) => {
                this.setPinText(idx, renderProperty);
            });
            for(var noPinTextIdx = this._Settings.pinTexts ? this._Settings.pinTexts.length : 0;noPinTextIdx < VRS.globalOptions.aircraftMarkerMaximumPinTextLines;++noPinTextIdx) {
                this.setPinText(noPinTextIdx, VRS.RenderProperty.None);
            }
        }

        getName = () : string =>
        {
            return this._Settings.name;
        }

        getMap = () : IMap =>
        {
            return this._Settings.map;
        }
        setMap = (map: IMap) =>
        {
            this._Settings.map = map;
        }

        getShowAltitudeStalk = () : boolean =>
        {
            return this._Settings.showAltitudeStalk;
        }
        setShowAltitudeStalk = (value: boolean) =>
        {
            if(this._Settings.showAltitudeStalk !== value) {
                this._Settings.showAltitudeStalk = value;
                this.raisePropertyChanged();
            }
        }

        getSuppressAltitudeStalkWhenZoomedOut = () : boolean =>
        {
            return this._Settings.suppressAltitudeStalkWhenZoomed;
        }
        setSuppressAltitudeStalkWhenZoomedOut = (value: boolean) =>
        {
            if(this._Settings.suppressAltitudeStalkWhenZoomed !== value) {
                this._Settings.suppressAltitudeStalkWhenZoomed = value;
                this.raisePropertyChanged();
            }
        }

        getShowPinText = () : boolean =>
        {
            return this._Settings.showPinText;
        }
        setShowPinText = (value: boolean) =>
        {
            if(this._Settings.showPinText !== value) {
                this._Settings.showPinText = value;
                this.raisePropertyChanged();
            }
        }

        getPinTexts = () : RenderPropertyEnum[] =>
        {
            return this._PinTexts;
        }
        getPinText = (index: number) =>
        {
            return index >= this._PinTexts.length ? VRS.RenderProperty.None : this._PinTexts[index];
        }
        setPinText = (index: number, value: RenderPropertyEnum) =>
        {
            if(index <= VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                if(!VRS.renderPropertyHandlers[value] || !VRS.renderPropertyHandlers[value].isSurfaceSupported(VRS.RenderSurface.Marker)) {
                    value = VRS.RenderProperty.None;
                }
                if(this.getPinText[index] !== value) {
                    if(index < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                        while(this._PinTexts.length <= index) {
                            this._PinTexts.push(this._PinTexts.length < this._Settings.pinTexts.length ? this._Settings.pinTexts[this._PinTexts.length] : VRS.RenderProperty.None);
                        }
                        this._PinTexts[index] = value;
                        this.raisePropertyChanged();
                    }
                }
            }
        }

        getPinTextLines = () : number =>
        {
            return this._Settings.pinTextLines;
        }
        setPinTextLines = (value: number) =>
        {
            if(value !== this._Settings.pinTextLines) {
                this._Settings.pinTextLines = value;
                this.raisePropertyChanged();
            }
        }

        getHideEmptyPinTextLines = () : boolean =>
        {
            return this._Settings.hideEmptyPinTextLines;
        }
        setHideEmptyPinTextLines = (value: boolean) =>
        {
            if(value !== this._Settings.hideEmptyPinTextLines) {
                this._Settings.hideEmptyPinTextLines = value;
                this.raisePropertyChanged();
            }
        }

        getTrailDisplay = () : TrailDisplayEnum =>
        {
            return this._Settings.trailDisplay;
        }
        setTrailDisplay = (value: TrailDisplayEnum) =>
        {
            if(value !== this._Settings.trailDisplay) {
                this._Settings.trailDisplay = value;
                this.raisePropertyChanged();
            }
        }

        getTrailType = () : TrailTypeEnum =>
        {
            return this._Settings.trailType;
        }
        setTrailType = (value: TrailTypeEnum) =>
        {
            if(value !== this._Settings.trailType) {
                this._Settings.trailType = value;
                // Changes to trail type do not raise property changed as there's nothing we can do about them. We need new trails from the server.
            }
        }

        getShowRangeCircles = () : boolean =>
        {
            return this._Settings.showRangeCircles;
        }
        setShowRangeCircles = (value: boolean) =>
        {
            if(value !== this._Settings.showRangeCircles) {
                this._Settings.showRangeCircles = value;
                this.raiseRangeCirclePropertyChanged();
            }
        }

        getRangeCircleInterval = () : number =>
        {
            return this._Settings.rangeCircleInterval;
        }
        setRangeCircleInterval = (value: number) =>
        {
            if(value !== this._Settings.rangeCircleInterval) {
                this._Settings.rangeCircleInterval = value;
                this.raiseRangeCirclePropertyChanged();
            }
        }

        getRangeCircleDistanceUnit = () : DistanceEnum =>
        {
            return this._Settings.rangeCircleDistanceUnit;
        }
        setRangeCircleDistanceUnit = (value: DistanceEnum) =>
        {
            if(value !== this._Settings.rangeCircleDistanceUnit) {
                this._Settings.rangeCircleDistanceUnit = value;
                this.raiseRangeCirclePropertyChanged();
            }
        };

        getRangeCircleCount = () : number =>
        {
            return this._Settings.rangeCircleCount;
        }
        setRangeCircleCount = (value: number) =>
        {
            if(value !== this._Settings.rangeCircleCount) {
                this._Settings.rangeCircleCount = value;
                this.raiseRangeCirclePropertyChanged();
            }
        }

        getRangeCircleOddColour = () : string =>
        {
            return this._Settings.rangeCircleOddColour;
        }
        setRangeCircleOddColour = (value: string) =>
        {
            if(value !== this._Settings.rangeCircleOddColour) {
                this._Settings.rangeCircleOddColour = value;
                this.raiseRangeCirclePropertyChanged();
            }
        }

        getRangeCircleOddWeight = () : number =>
        {
            return this._Settings.rangeCircleOddWeight;
        }
        setRangeCircleOddWeight = (value: number) =>
        {
            if(value !== this._Settings.rangeCircleOddWeight) {
                this._Settings.rangeCircleOddWeight = value;
                this.raiseRangeCirclePropertyChanged();
            }
        }

        getRangeCircleEvenColour = () : string =>
        {
            return this._Settings.rangeCircleEvenColour;
        }
        setRangeCircleEvenColour = (value: string) =>
        {
            if(value !== this._Settings.rangeCircleEvenColour) {
                this._Settings.rangeCircleEvenColour = value;
                this.raiseRangeCirclePropertyChanged();
            }
        }

        getRangeCircleEvenWeight = () : number =>
        {
            return this._Settings.rangeCircleEvenWeight;
        }
        setRangeCircleEvenWeight = (value: number) =>
        {
            if(value !== this._Settings.rangeCircleEvenWeight) {
                this._Settings.rangeCircleEvenWeight = value;
                this.raiseRangeCirclePropertyChanged();
            }
        }

        getOnlyUsePre22Icons = () : boolean =>
        {
            return this._Settings.onlyUsePre22Icons;
        }
        setOnlyUsePre22Icons = (value: boolean) =>
        {
            if(this._Settings.onlyUsePre22Icons !== value) {
                this._Settings.onlyUsePre22Icons = value;
                this.raisePropertyChanged();
            }
        }

        getAircraftMarkerClustererMaxZoom = () : number =>
        {
            return this._Settings.aircraftMarkerClustererMaxZoom;
        }
        setAircraftMarkerClustererMaxZoom = (value: number) =>
        {
            if(this._Settings.aircraftMarkerClustererMaxZoom !== value) {
                this._Settings.aircraftMarkerClustererMaxZoom = value;
                this.raisePropertyChanged();
            }
        }
        getCanSetAircraftMarkerClustererMaxZoomFromMap = () : boolean =>
        {
            return !!this._Settings.map;
        }
        setAircraftMarkerClusterMaxZoomFromMap()
        {
            if(this.getCanSetAircraftMarkerClustererMaxZoomFromMap()) {
                this.setAircraftMarkerClustererMaxZoom(this._Settings.map.getZoom());
            }
        }

        hookPropertyChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.propertyChanged, callback, forceThis);
        }
        private raisePropertyChanged()
        {
            if(!this._SuppressEvents) {
                this._Dispatcher.raise(this._Events.propertyChanged);
            }
        }

        hookRangeCirclePropertyChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.rangeCirclePropertyChanged, callback, forceThis);
        }
        private raiseRangeCirclePropertyChanged()
        {
            if(!this._SuppressEvents) {
                this._Dispatcher.raise(this._Events.rangeCirclePropertyChanged);
            }
        }

        unhook = (hookResult: IEventHandle) =>
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Saves the current state to persistent storage.
         */
        saveState = () =>
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Returns the previously saved state or, if none has been saved, the current state.
         */
        loadState = () : AircraftPlotterOptions_SaveState =>
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);

            if(result.showAltitudeStalk && !VRS.globalOptions.aircraftMarkerAllowAltitudeStalk) result.showAltitudeStalk = false;
            if(result.showPinText && !VRS.globalOptions.aircraftMarkerAllowPinText)             result.showPinText = false;
            if(!result.onlyUsePre22Icons && VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons)  result.onlyUsePre22Icons = true;

            if(result.rangeCircleCount > VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles)     result.rangeCircleCount = VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles;
            if(result.rangeCircleInterval > VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval) result.rangeCircleInterval = VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval;
            if(result.rangeCircleEvenWeight > VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight) result.rangeCircleEvenWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight;
            if(result.rangeCircleOddWeight > VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight)  result.rangeCircleOddWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight;

            result.pinTexts = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.pinTexts, [ VRS.RenderSurface.Marker ], VRS.globalOptions.aircraftMarkerMaximumPinTextLines);
            while(result.pinTexts.length < this._Settings.pinTexts.length && result.pinTexts.length < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                result.pinTexts.push(this._Settings.pinTexts[result.pinTexts.length]);
            }
            while(result.pinTexts.length < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                result.pinTexts.push(VRS.RenderProperty.None);
            }

            return result;
        }

        /**
         * Applies the previously saved state to this object.
         */
        applyState = (settings: AircraftPlotterOptions_SaveState) =>
        {
            var suppressEvents = this._SuppressEvents;
            this._SuppressEvents = true;

            this.setSuppressAltitudeStalkWhenZoomedOut(settings.suppressAltitudeStalkWhenZoomedOut);
            this.setShowAltitudeStalk(settings.showAltitudeStalk);
            this.setShowPinText(settings.showPinText);
            for(var i = 0;i < settings.pinTexts.length;++i) {
                this.setPinText(i, settings.pinTexts[i]);
            }
            this.setPinTextLines(settings.pinTextLines);
            this.setHideEmptyPinTextLines(settings.hideEmptyPinTextLines);
            this.setTrailType(settings.trailType);
            this.setTrailDisplay(settings.trailDisplay);

            this.setShowRangeCircles(settings.showRangeCircles);
            this.setRangeCircleCount(settings.rangeCircleCount);
            this.setRangeCircleInterval(settings.rangeCircleInterval);
            this.setRangeCircleDistanceUnit(settings.rangeCircleDistanceUnit);
            this.setRangeCircleOddColour(settings.rangeCircleOddColour);
            this.setRangeCircleOddWeight(settings.rangeCircleOddWeight);
            this.setRangeCircleEvenColour(settings.rangeCircleEvenColour);
            this.setRangeCircleEvenWeight(settings.rangeCircleEvenWeight);
            this.setOnlyUsePre22Icons(settings.onlyUsePre22Icons);
            this.setAircraftMarkerClustererMaxZoom(settings.aircraftMarkerClustererMaxZoom);

            this._SuppressEvents = suppressEvents;

            this.raisePropertyChanged();
            this.raiseRangeCirclePropertyChanged();
        }

        /**
         * Loads and applies the previously saved state.
         */
        loadAndApplyState = () =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key to use when saving and loading state.
         */
        private persistenceKey() : string
        {
            return 'vrsAircraftPlotterOptions-' + this.getName();
        }

        /**
         * Returns the object that holds the current state.
         */
        private createSettings() : AircraftPlotterOptions_SaveState
        {
            return {
                showAltitudeStalk:                  this.getShowAltitudeStalk(),
                suppressAltitudeStalkWhenZoomedOut: this.getSuppressAltitudeStalkWhenZoomedOut(),
                showPinText:                        this.getShowPinText(),
                pinTexts:                           this.getPinTexts(),
                pinTextLines:                       this.getPinTextLines(),
                hideEmptyPinTextLines:              this.getHideEmptyPinTextLines(),
                trailDisplay:                       this.getTrailDisplay(),
                trailType:                          this.getTrailType(),
                showRangeCircles:                   this.getShowRangeCircles(),
                rangeCircleInterval:                this.getRangeCircleInterval(),
                rangeCircleDistanceUnit:            this.getRangeCircleDistanceUnit(),
                rangeCircleCount:                   this.getRangeCircleCount(),
                rangeCircleOddColour:               this.getRangeCircleOddColour(),
                rangeCircleOddWeight:               this.getRangeCircleOddWeight(),
                rangeCircleEvenColour:              this.getRangeCircleEvenColour(),
                rangeCircleEvenWeight:              this.getRangeCircleEvenWeight(),
                onlyUsePre22Icons:                  this.getOnlyUsePre22Icons(),
                aircraftMarkerClustererMaxZoom:     this.getAircraftMarkerClustererMaxZoom(),
            };
        }

        /**
         * Returns the configuration UI option panes for the object (except for range circles).
         */
        createOptionPane = (displayOrder: number) : OptionPane[] =>
        {
            var result: OptionPane[] = [];

            // Display pane
            var displayPane = new VRS.OptionPane({
                name:           'vrsAircraftPlotterOptionsDisplayPane_' + this.getName(),
                titleKey:       'PaneAircraftDisplay',
                displayOrder:   displayOrder
            });
            result.push(displayPane);

            // Altitude stalk options
            if(VRS.globalOptions.aircraftMarkerAllowAltitudeStalk) {
                displayPane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showAltitudeStalk',
                    labelKey:       'ShowAltitudeStalk',
                    getValue:       this.getShowAltitudeStalk,
                    setValue:       this.setShowAltitudeStalk,
                    saveState:      this.saveState
                }));
                displayPane.addField(new VRS.OptionFieldCheckBox({
                    name:           'suppressAltitudeStalkWhenZoomedOut',
                    labelKey:       'SuppressAltitudeStalkWhenZoomedOut',
                    getValue:       this.getSuppressAltitudeStalkWhenZoomedOut,
                    setValue:       this.setSuppressAltitudeStalkWhenZoomedOut,
                    saveState:      this.saveState
                }));
            }

            // Pre-version 2.2 icon options
            if(!VRS.globalOptions.aircraftMarkerOnlyUsePre22Icons) {
                displayPane.addField(new VRS.OptionFieldCheckBox({
                    name:           'onlyUsePre22Icons',
                    labelKey:       'OnlyUsePre22Icons',
                    getValue:       this.getOnlyUsePre22Icons,
                    setValue:       this.setOnlyUsePre22Icons,
                    saveState:      this.saveState
                }));
            }

            // Pin text options
            var showPinTextOptions = VRS.globalOptions.aircraftMarkerAllowPinText;
            if(showPinTextOptions && VRS.serverConfig) showPinTextOptions = VRS.serverConfig.pinTextEnabled();
            if(showPinTextOptions) {
                var handlers = VRS.renderPropertyHelper.getHandlersForSurface(VRS.RenderSurface.Marker);
                VRS.renderPropertyHelper.sortHandlers(handlers, true);
                var values = VRS.renderPropertyHelper.createValueTextListForHandlers(handlers);
                var pinTextLines = VRS.globalOptions.aircraftMarkerMaximumPinTextLines;

                var buildContentFieldName = function(idx) {
                    return 'pinText' + idx;
                };
                displayPane.addField(new VRS.OptionFieldNumeric({
                    name:               'pinTextLines',
                    labelKey:           'PinTextLines',
                    getValue:           this.getPinTextLines,
                    setValue:           this.setPinTextLines,
                    min:                0,
                    max:                pinTextLines,
                    inputWidth:         VRS.InputWidth.OneChar,
                    saveState:          () => {
                        this.saveState();
                        for(var contentIdx = 0;contentIdx < pinTextLines;++contentIdx) {
                            var contentField = displayPane.getFieldByName(buildContentFieldName(contentIdx));
                            if(contentField) contentField.raiseRefreshFieldVisibility();
                        }
                    }
                }));

                var addPinTextContentField = (idx: number) => {
                    displayPane.addField(new VRS.OptionFieldComboBox({
                        name:           buildContentFieldName(idx),
                        labelKey:       () => VRS.stringUtility.format(VRS.$$.PinTextNumber, idx + 1),
                        getValue:       () => this.getPinText(idx),
                        setValue:       (value: RenderPropertyEnum) => this.setPinText(idx, value),
                        saveState:      this.saveState,
                        visible:        () => idx < this.getPinTextLines(),
                        values:         values
                    }));
                };
                for(var lineIdx = 0;lineIdx < pinTextLines;++lineIdx) {
                    addPinTextContentField(lineIdx);
                }

                displayPane.addField(new VRS.OptionFieldCheckBox({
                    name:               'hideEmptyPinTextLines',
                    labelKey:           'HideEmptyPinTextLines',
                    getValue:           this.getHideEmptyPinTextLines,
                    setValue:           this.setHideEmptyPinTextLines,
                    saveState:          this.saveState
                }));

                if(VRS.globalOptions.aircraftMarkerClustererUserCanConfigure && VRS.globalOptions.aircraftMarkerClustererEnabled && this.getCanSetAircraftMarkerClustererMaxZoomFromMap()) {
                    displayPane.addField(new VRS.OptionFieldButton({
                        name:           'aircraftMarkerClustererSetMaxZoom',
                        saveState:      () => {
                                            this.setAircraftMarkerClusterMaxZoomFromMap();
                                            this.saveState();
                                        },
                        keepWithNext:   true,
                        labelKey:       'SetClustererZoomLevel'
                    }));
                    displayPane.addField(new VRS.OptionFieldButton({
                        name:           'resetAircraftMarkerClustererMaxZoom',
                        saveState:      () => {
                                            this.setAircraftMarkerClustererMaxZoom(VRS.globalOptions.aircraftMarkerClustererMaxZoom);
                                            this.saveState();
                                        },
                        labelKey:       'ResetClustererZoomLevel'
                    }));
                }
            }

            // Trail options
            if(!VRS.globalOptions.suppressTrails) {
                var trailsPane = new VRS.OptionPane({
                    name:           'vrsAircraftPlotterTrailsPane' + this.getName(),
                    titleKey:       'PaneAircraftTrails',
                    displayOrder:   displayOrder + 1,
                    fields:         [
                        new VRS.OptionFieldRadioButton({
                            name:           'trailDisplay',
                            getValue:       this.getTrailDisplay,
                            setValue:       this.setTrailDisplay,
                            saveState:      this.saveState,
                            values: [
                                new VRS.ValueText({ value: VRS.TrailDisplay.None,         textKey: 'DoNotShow' }),
                                new VRS.ValueText({ value: VRS.TrailDisplay.SelectedOnly, textKey: 'ShowForSelectedOnly' }),
                                new VRS.ValueText({ value: VRS.TrailDisplay.AllAircraft,  textKey: 'ShowForAllAircraft' })
                            ]
                        }),
                        new VRS.OptionFieldRadioButton({
                            name:           'trailType',
                            getValue:       () => {
                                switch(this.getTrailType()) {
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.ShortAltitude:
                                        return VRS.TrailType.FullAltitude;
                                    case VRS.TrailType.FullSpeed:
                                    case VRS.TrailType.ShortSpeed:
                                        return VRS.TrailType.FullSpeed;
                                }
                                return VRS.TrailType.Full;
                            },
                            setValue:       (value: TrailTypeEnum) => {
                                switch(this.getTrailType()) {
                                    case VRS.TrailType.Full:
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.FullSpeed:
                                        this.setTrailType(value);
                                        break;
                                    default:
                                        switch(value) {
                                            case VRS.TrailType.Full:            this.setTrailType(VRS.TrailType.Short); break;
                                            case VRS.TrailType.FullAltitude:    this.setTrailType(VRS.TrailType.ShortAltitude); break;
                                            case VRS.TrailType.FullSpeed:       this.setTrailType(VRS.TrailType.ShortSpeed); break;
                                        }
                                }
                            },
                            saveState:      this.saveState,
                            values: [
                                new VRS.ValueText({ value: VRS.TrailType.Full,            textKey: 'JustPositions' }),
                                new VRS.ValueText({ value: VRS.TrailType.FullAltitude,    textKey: 'PositionAndAltitude' }),
                                new VRS.ValueText({ value: VRS.TrailType.FullSpeed,       textKey: 'PositionAndSpeed' })
                            ]
                        }),
                        new VRS.OptionFieldCheckBox({
                            name:           'showShortTrails',
                            labelKey:       'ShowShortTrails',
                            getValue:       () => {
                                switch(this.getTrailType()) {
                                    case VRS.TrailType.Full:
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.FullSpeed:
                                        return false;
                                    default:
                                        return true;
                                }
                            },
                            setValue:       (value: boolean) => {
                                switch(this.getTrailType()) {
                                    case VRS.TrailType.Full:
                                    case VRS.TrailType.Short:
                                        this.setTrailType(value ? VRS.TrailType.Short : VRS.TrailType.Full);
                                        break;
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.ShortAltitude:
                                        this.setTrailType(value ? VRS.TrailType.ShortAltitude : VRS.TrailType.FullAltitude);
                                        break;
                                    case VRS.TrailType.FullSpeed:
                                    case VRS.TrailType.ShortSpeed:
                                        this.setTrailType(value ? VRS.TrailType.ShortSpeed : VRS.TrailType.FullSpeed);
                                        break;
                                }
                            },
                            saveState:      this.saveState
                        })
                    ]
                });
                result.push(trailsPane);
            }

            return result;
        }

        /**
         * Returns the configuration UI option pane for range circle settings.
         */
        createOptionPaneForRangeCircles = (displayOrder: number) : OptionPane =>
        {
            return new VRS.OptionPane({
                name:           'rangeCircle',
                titleKey:       'PaneRangeCircles',
                displayOrder:   displayOrder,
                fields: [
                    new VRS.OptionFieldCheckBox({
                        name:           'show',
                        labelKey:       'ShowRangeCircles',
                        getValue:       this.getShowRangeCircles,
                        setValue:       this.setShowRangeCircles,
                        saveState:      this.saveState
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'count',
                        labelKey:       'Quantity',
                        getValue:       this.getRangeCircleCount,
                        setValue:       this.setRangeCircleCount,
                        saveState:      this.saveState,
                        inputWidth:     VRS.InputWidth.ThreeChar,
                        min:            1,
                        max:            VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'interval',
                        labelKey:       'Distance',
                        getValue:       this.getRangeCircleInterval,
                        setValue:       this.setRangeCircleInterval,
                        saveState:      this.saveState,
                        inputWidth:     VRS.InputWidth.ThreeChar,
                        min:            5,
                        max:            VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval,
                        step:           5,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldComboBox({
                        name:           'distanceUnit',
                        getValue:       this.getRangeCircleDistanceUnit,
                        setValue:       this.setRangeCircleDistanceUnit,
                        saveState:      this.saveState,
                        values:         VRS.UnitDisplayPreferences.getDistanceUnitValues()
                    }),

                    new VRS.OptionFieldColour({
                        name:           'oddColour',
                        labelKey:       'RangeCircleOddColour',
                        getValue:       this.getRangeCircleOddColour,
                        setValue:       this.setRangeCircleOddColour,
                        saveState:      this.saveState,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'oddWidth',
                        getValue:       this.getRangeCircleOddWeight,
                        setValue:       this.setRangeCircleOddWeight,
                        saveState:      this.saveState,
                        inputWidth:     VRS.InputWidth.OneChar,
                        min:            1,
                        max:            VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldLabel({
                        name:           'oddPixels',
                        labelKey:       'Pixels'
                    }),

                    new VRS.OptionFieldColour({
                        name:           'evenColour',
                        labelKey:       'RangeCircleEvenColour',
                        getValue:       this.getRangeCircleEvenColour,
                        setValue:       this.setRangeCircleEvenColour,
                        saveState:      this.saveState,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'evenWidth',
                        getValue:       this.getRangeCircleEvenWeight,
                        setValue:       this.setRangeCircleEvenWeight,
                        saveState:      this.saveState,
                        inputWidth:     VRS.InputWidth.OneChar,
                        min:            1,
                        max:            VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldLabel({
                        name:           'evenPixels',
                        labelKey:       'Pixels'
                    })
                ]
            });
        }
    }

    /**
     * The settings to use when creating an AircraftPlotter.
     */
    export interface AircraftPlotter_Settings
    {
        /**
         * The mandatory object that handles the plotter options for us.
         */
        plotterOptions: AircraftPlotterOptions;

        /**
         * The aircraft list whose aircraft are to be plotted onto the map. If passed then aircraft are automatically plotted when the list is updated.
         */
        aircraftList?: AircraftList;

        /**
         * The jQuery element to which the map VRS jQuery UI plugin has been applied
         */
        map: JQuery;

        /**
         * The object describing the unit display preferences of the user.
         */
        unitDisplayPreferences?: UnitDisplayPreferences;

        /**
         * The name to use when saving the object's state.
         */
        name?: string;

        /**
         * A function that returns the collection of aircraft to plot. Defaults to the current aircraft on the aircraft list or an empty collection if no list is supplied.
         */
        getAircraft?: () => AircraftCollection;

        /**
         * A function that returns the selected aircraft. Defaults to the selected aircraft from the aircraft list or null if no list is supplied.
         */
        getSelectedAircraft?: () => Aircraft;

        /**
         * An array of objects that describe all of the different types of aircraft marker.
         */
        aircraftMarkers?: AircraftMarker[];

        /**
         * The width in pixels for markers that are showing pin text. Has no effect if labels are used to display pin text.
         */
        pinTextMarkerWidth?: number;

        /**
         * The pixels added to the height of the marker for each line of pin text. Has no effect if labels are used to display pin text. The actual number of pixels added may be slightly larger to prevent jaggies in the rendered text.
         */
        pinTextLineHeight?: number;

        /**
         * True if rotation of the normal and selected aircraft images is enabled, false if it is not.
         */
        allowRotation?: boolean;

        /**
         * The smallest number of degrees of rotation that an aircraft has to turn through before its marker is refreshed to display it.
         */
        rotationGranularity?: number;

        /**
         * The map zoom level at which altitude stalks are suppressed.
         */
        suppressAltitudeStalkAboveZoom?: number;

        /**
         * True if marker clustering is disabled for this instance of the plotter.
         */
        suppressMarkerClustering?: boolean;

        /**
         * The CSS colour of the trail when the aircraft is not selected.
         */
        normalTrailColour?: string;

        /**
         * The CSS colour of the trail when the aircraft is selected.
         */
        selectedTrailColour?: string;

        /**
         * The pixel width of trails for aircraft that are not selected.
         */
        normalTrailWidth?: number;

        /**
         * The pixel width of trails for aircraft that are selected.
         */
        selectedTrailWidth?: number;

        /**
         * True if tooltips are to be shown on markers, false if they are not.
         */
        showTooltips?: boolean;

        /**
         * True if text is never to be drawn onto markers by the server, false if it is. Setting this to true draws pin text in labels instead of having the server add it to the marker image. Ignored if the server is running Mono (in which case it's always set to true).
         */
        suppressTextOnImages?: boolean;

        /**
         * An optional array that returns a custom array of pin text strings for an aircraft rather than using the plotter options to derive pin texts.
         */
        getCustomPinTexts?: (aircraft: Aircraft) => string[];

        /**
         * True if range circles are allowed, false if they must always be suppressed.
         */
        allowRangeCircles?: boolean;

        /**
         * The zoom level at which vehicles that are not aircraft are hidden.
         */
        hideNonAircraftZoomLevel?: number;
    }

    interface AircraftPlotter_ColourPoint extends ILatLng
    {
        colour: string;
    }

    /**
     * The object that can plot aircraft onto a map.
     */
    export class AircraftPlotter
    {
        private _Settings: AircraftPlotter_Settings;
        private _Map: IMap;
        private _UnitDisplayPreferences: UnitDisplayPreferences;
        private _SuppressTextOnImages: boolean;
        private _Suspended = false;
        private _PlottedDetail: { [index: number]: PlottedDetail } = {};    // An associative array of all of the currently plotted aircraft indexed by aircraft ID.
        private _PreviousTrailTypeRequested: TrailTypeEnum;                 // A VRS.TrailType string describing the type of trails drawn in the last refresh of the display.
        private _GetSelectedAircraft: () => Aircraft;                       // A method that returns the selected aircraft.
        private _GetAircraft: () => AircraftCollection;                     // A method that returns the list of aircraft to plot.
        private _RangeCircleCentre: ILatLng = null;
        private _RangeCircleCircles: IMapCircle[] = [];
        private _MovingMap: boolean = VRS.globalOptions.aircraftMarkerMovingMapOn;
        private _MapMarkerClusterer: IMapMarkerClusterer;
        private _SvgGenerator = new SvgGenerator();

        // Event handles
        private _PlotterOptionsPropertyChangedHook:             IEventHandle;
        private _PlotterOptionsRangeCirclePropertyChangedHook:  IEventHandle;
        private _AircraftListUpdatedHook:                       IEventHandle;
        private _AircraftListFetchingListHook:                  IEventHandle;
        private _SelectedAircraftChangedHook:                   IEventHandle;
        private _FlightLevelHeightUnitChangedHook:              IEventHandle;
        private _FlightLevelTransitionAltitudeChangedHook:      IEventHandle;
        private _FlightLevelTransitionHeightUnitChangedHook:    IEventHandle;
        private _HeightUnitChangedHook:                         IEventHandle;
        private _SpeedUnitChangedHook:                          IEventHandle;
        private _ShowVsiInSecondsHook:                          IEventHandle;
        private _MapIdleHook:                                   IEventHandleJQueryUI;
        private _MapMarkerClickedHook:                          IEventHandleJQueryUI;
        private _CurrentLocationChangedHook:                    IEventHandle;
        private _ConfigurationChangedHook:                      IEventHandle;

        constructor(settings: AircraftPlotter_Settings)
        {
            settings = $.extend({
                name:                           'default',
                aircraftMarkers:                VRS.globalOptions.aircraftMarkers,
                pinTextMarkerWidth:             VRS.globalOptions.aircraftMarkerPinTextWidth,
                pinTextLineHeight:              VRS.globalOptions.aircraftMarkerPinTextLineHeight,
                allowRotation:                  VRS.globalOptions.aircraftMarkerRotate,
                rotationGranularity:            VRS.globalOptions.aircraftMarkerRotationGranularity,
                suppressAltitudeStalkAboveZoom: VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkZoomLevel,
                suppressMarkerClustering:       false,
                normalTrailColour:              VRS.globalOptions.aircraftMarkerTrailColourNormal,
                selectedTrailColour:            VRS.globalOptions.aircraftMarkerTrailColourSelected,
                normalTrailWidth:               VRS.globalOptions.aircraftMarkerTrailWidthNormal,
                selectedTrailWidth:             VRS.globalOptions.aircraftMarkerTrailWidthSelected,
                showTooltips:                   VRS.globalOptions.aircraftMarkerShowTooltip,
                suppressTextOnImages:           VRS.globalOptions.aircraftMarkerSuppressTextOnImages,
                allowRangeCircles:              VRS.globalOptions.aircraftMarkerAllowRangeCircles,
                hideNonAircraftZoomLevel:       VRS.globalOptions.aircraftMarkerHideNonAircraftZoomLevel,
                showNonAircraftTrails:          VRS.globalOptions.aircraftMarkerShowNonAircraftTrails
            }, settings);
            this._Settings = settings;

            this._Map = VRS.jQueryUIHelper.getMapPlugin(settings.map);
            if(VRS.globalOptions.aircraftMarkerClustererEnabled && !settings.suppressMarkerClustering) {
                this._MapMarkerClusterer = this._Map.createMapMarkerClusterer({
                    maxZoom:            settings.plotterOptions.getAircraftMarkerClustererMaxZoom(),
                    minimumClusterSize: VRS.globalOptions.aircraftMarkerClustererMinimumClusterSize
                });
            }

            this._UnitDisplayPreferences = settings.unitDisplayPreferences || new VRS.UnitDisplayPreferences();
            this._SuppressTextOnImages = settings.suppressTextOnImages;

            this._GetSelectedAircraft = settings.getSelectedAircraft || function() {
                return settings.aircraftList ? settings.aircraftList.getSelectedAircraft() : null;
            };
            this._GetAircraft = settings.getAircraft || function() {
                return settings.aircraftList ? settings.aircraftList.getAircraft() : new VRS.AircraftCollection();
            };

            this.configureSuppressTextOnImages();

            this._PlotterOptionsPropertyChangedHook =               settings.plotterOptions.hookPropertyChanged(this.optionsPropertyChanged, this);
            this._PlotterOptionsRangeCirclePropertyChangedHook =    settings.plotterOptions.hookRangeCirclePropertyChanged(this.optionsRangePropertyChanged, this);
            this._AircraftListUpdatedHook =                         settings.aircraftList ? settings.aircraftList.hookUpdated(this.refreshMarkersOnListUpdate, this) : null;
            this._AircraftListFetchingListHook =                    settings.aircraftList ? settings.aircraftList.hookFetchingList(this.fetchingList, this) : null;
            this._SelectedAircraftChangedHook =                     settings.aircraftList ? settings.aircraftList.hookSelectedAircraftChanged(this.refreshSelectedAircraft, this) : null;
            this._FlightLevelHeightUnitChangedHook =                this._UnitDisplayPreferences.hookFlightLevelHeightUnitChanged(() => this.refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel));
            this._FlightLevelTransitionAltitudeChangedHook =        this._UnitDisplayPreferences.hookFlightLevelTransitionAltitudeChanged(() => this.refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel));
            this._FlightLevelTransitionHeightUnitChangedHook =      this._UnitDisplayPreferences.hookFlightLevelTransitionHeightUnitChanged(() => this.refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel));
            this._HeightUnitChangedHook =                           this._UnitDisplayPreferences.hookHeightUnitChanged(() => this.refreshMarkersIfUsingPinText(VRS.RenderProperty.Altitude));
            this._SpeedUnitChangedHook =                            this._UnitDisplayPreferences.hookSpeedUnitChanged(() => this.refreshMarkersIfUsingPinText(VRS.RenderProperty.Speed));
            this._ShowVsiInSecondsHook =                            this._UnitDisplayPreferences.hookShowVerticalSpeedPerSecondChanged(() => this.refreshMarkersIfUsingPinText(VRS.RenderProperty.VerticalSpeed));
            this._MapIdleHook =                                     this._Map.hookIdle(() => this.refreshMarkers(null, null));
            this._MapMarkerClickedHook =                            this._Map.hookMarkerClicked((event: Event, data: IMapMarkerEventArgs) => this.selectAircraftById(<number>(data.id)));
            this._CurrentLocationChangedHook =                      VRS.currentLocation ? VRS.currentLocation.hookCurrentLocationChanged(this.currentLocationChanged, this) : null;
            this._ConfigurationChangedHook =                        VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, this.configurationChanged, this);

            this.refreshRangeCircles();
        }

        getName() : string
        {
            return this._Settings.name;
        }

        /**
         * Gets the map that the plotter is drawing onto.
         */
        getMap() : IMap
        {
            return this._Map;
        }

        /**
         * Gets the object that is clustering map markers for us. Note that this can be null if clustering has been disabled.
         */
        getMapMarkerClusterer() : IMapMarkerClusterer
        {
            return this._MapMarkerClusterer;
        }

        /**
         * Gets a value indicating that trails need to be hidden past a certain zoom level.
         */
        getHideTrailsAtMaxZoom()
        {
            return !!this._MapMarkerClusterer;
        }

        /**
         * Gets the zoom level past which trails should be hidden.
         */
        getHideTrailsMaxZoom()
        {
            return !this._MapMarkerClusterer ? -1 : this._MapMarkerClusterer.getMaxZoom();
        }

        getMovingMap() : boolean
        {
            return this._MovingMap;
        }
        setMovingMap(value: boolean)
        {
            if(value !== this._MovingMap) {
                this._MovingMap = value;
                if(value) {
                    this.moveMapToSelectedAircraft();
                }
            }
        }

        /**
         * Releases all resources allocated or hooked by the object.
         */
        dispose()
        {
            this.destroyRangeCircles();

            if(this._PlotterOptionsPropertyChangedHook)              this._Settings.plotterOptions.unhook(this._PlotterOptionsPropertyChangedHook);
            if(this._PlotterOptionsRangeCirclePropertyChangedHook)   this._Settings.plotterOptions.unhook(this._PlotterOptionsRangeCirclePropertyChangedHook);
            if(this._AircraftListUpdatedHook)                        this._Settings.aircraftList.unhook(this._AircraftListUpdatedHook);
            if(this._AircraftListFetchingListHook)                   this._Settings.aircraftList.unhook(this._AircraftListFetchingListHook);
            if(this._SelectedAircraftChangedHook)                    this._Settings.aircraftList.unhook(this._SelectedAircraftChangedHook);
            if(this._FlightLevelHeightUnitChangedHook)               this._UnitDisplayPreferences.unhook(this._FlightLevelHeightUnitChangedHook);
            if(this._FlightLevelTransitionAltitudeChangedHook)       this._UnitDisplayPreferences.unhook(this._FlightLevelTransitionAltitudeChangedHook);
            if(this._FlightLevelTransitionHeightUnitChangedHook)     this._UnitDisplayPreferences.unhook(this._FlightLevelTransitionHeightUnitChangedHook);
            if(this._HeightUnitChangedHook)                          this._UnitDisplayPreferences.unhook(this._HeightUnitChangedHook);
            if(this._SpeedUnitChangedHook)                           this._UnitDisplayPreferences.unhook(this._SpeedUnitChangedHook);
            if(this._ShowVsiInSecondsHook)                           this._UnitDisplayPreferences.unhook(this._ShowVsiInSecondsHook);
            if(this._MapIdleHook)                                    this._Map.unhook(this._MapIdleHook);
            if(this._MapMarkerClickedHook)                           this._Map.unhook(this._MapMarkerClickedHook);
            if(this._CurrentLocationChangedHook)                     VRS.currentLocation.unhook(this._CurrentLocationChangedHook);
            if(this._ConfigurationChangedHook)                       VRS.globalDispatch.unhook(this._ConfigurationChangedHook);
        }

        /***
         * Configures the _SuppressTextOnImages field from global options and the server configuration.
         */
        private configureSuppressTextOnImages()
        {
            var originalValue = this._SuppressTextOnImages;

            this._SuppressTextOnImages = this._Settings.suppressTextOnImages;
            if(VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    if(this._SuppressTextOnImages === undefined) {
                        this._SuppressTextOnImages = config.UseMarkerLabels;
                    }
                }
            }
            if(this._SuppressTextOnImages === undefined) {
                this._SuppressTextOnImages = false;
            }

            return originalValue != this._SuppressTextOnImages;
        }

        /**
         * Suspends or resumes the plotter. While the plotter is suspended no updates are made to the map. Once the plotter
         * is resumed the map is refreshed and future updates are plotted.
         */
        suspend(onOff: boolean)
        {
            onOff = !!onOff;

            if(this._Suspended !== onOff) {
                this._Suspended = onOff;
                if(!this._Suspended) {
                    this.refreshMarkers(null, null, true);
                    this.refreshRangeCircles(true);
                }
            }
        }

        /**
         * Refreshes the markers for the aircraft.
         */
        plot(refreshAllMarkers?: boolean, ignoreBounds?: boolean)
        {
            this.refreshMarkers(null, null, !!refreshAllMarkers, !!ignoreBounds);
        };

        /**
         * Returns an array of plotted aircraft identifiers.
         **/
        getPlottedAircraftIds() : number[]
        {
            var result: number[] = [];
            for(var aircraftId in this._PlottedDetail) {
                result.push(Number(aircraftId));
            }

            return result;
        }

        /**
         * Refreshes the markers for all of the aircraft in the aircraft list. Note that this is an internal method, it
         * is not the handler for the aircraft list updated event.
         */
        private refreshMarkers(newAircraft?: AircraftCollection, oldAircraft?: AircraftCollection, alwaysRefreshIcon?: boolean, ignoreBounds?: boolean)
        {
            var unusedAircraft: AircraftCollection = null;
            if(oldAircraft) {
                this.removeOldMarkers(oldAircraft);
            } else {
                unusedAircraft = new VRS.AircraftCollection();
                for(var aircraftId in this._PlottedDetail) {
                    unusedAircraft[aircraftId] = this._PlottedDetail[aircraftId].aircraft;
                }
            }

            var bounds = this._Map.getBounds();
            if(bounds || ignoreBounds) {
                var mapZoomLevel = this._Map.getZoom();
                var selectedAircraft = this._GetSelectedAircraft();
                this._GetAircraft().foreachAircraft((aircraft: Aircraft) => {
                    this.refreshAircraftMarker(aircraft, alwaysRefreshIcon, ignoreBounds, bounds, mapZoomLevel, selectedAircraft && selectedAircraft === aircraft);
                    if(unusedAircraft && unusedAircraft[aircraft.id]) {
                        unusedAircraft[aircraft.id] = undefined;
                    }
                });
                this.moveMapToSelectedAircraft(selectedAircraft);
            }

            if(unusedAircraft) {
                this.removeOldMarkers(unusedAircraft);
            }

            if(this._MapMarkerClusterer) {
                this._MapMarkerClusterer.repaint();
            }
        }

        /**
         * Refreshes an aircraft's marker on the map.
         */
        private refreshAircraftMarker(aircraft: Aircraft, forceRefresh: boolean, ignoreBounds: boolean, bounds: IBounds, mapZoomLevel: number, isSelectedAircraft: boolean)
        {
            if((ignoreBounds || bounds) && aircraft.hasPosition()) {
                var isStale = aircraft.positionStale.val;
                var position = isStale ? null : aircraft.getPosition();
                var isInBounds = isStale ? false : ignoreBounds || aircraft.positionWithinBounds(bounds);
                var plotAircraft = !isStale && (isInBounds || (isSelectedAircraft && VRS.globalOptions.aircraftMarkerAlwaysPlotSelected));
                if(plotAircraft && !aircraft.isAircraftSpecies() && mapZoomLevel < this._Settings.hideNonAircraftZoomLevel) plotAircraft = false;

                var details = this._PlottedDetail[aircraft.id];
                if(details && details.aircraft !== aircraft) {
                    // This can happen if the aircraft goes off the radar and then returns - it will have a different object
                    this.removeDetails(details);
                    details = undefined;
                }

                if(details) {
                    if(!plotAircraft) {
                        this.removeDetails(details);
                    } else {
                        var marker = details.mapMarker;
                        marker.setPosition(position);
                        if(forceRefresh || this.haveIconDetailsChanged(details, mapZoomLevel)) {
                            var icon = this.createIcon(details, mapZoomLevel, isSelectedAircraft);
                            if(icon) {
                                marker.setIcon(icon);
                                var zIndex = isSelectedAircraft ? 101 : 100;
                                if(zIndex !== marker.getZIndex()) {
                                    marker.setZIndex(zIndex);
                                }
                                if(marker.isMarkerWithLabel) {
                                    if(icon.labelAnchor && (!details.mapIcon || (details.mapIcon.labelAnchor.x !== icon.labelAnchor.x || details.mapIcon.labelAnchor.y !== icon.labelAnchor.y))) {
                                        marker.setLabelAnchor(icon.labelAnchor);
                                    }
                                }
                                details.mapIcon = icon;
                            }
                        }
                        if(forceRefresh || this.haveTooltipDetailsChanged(details)) {
                            marker.setTooltip(this.getTooltip(details));
                        }
                        if(forceRefresh || this.haveLabelDetailsChanged(details)) {
                            this.createLabel(details);
                        }
                        this.updateTrail(details, isSelectedAircraft, mapZoomLevel, forceRefresh);
                    }
                } else if(plotAircraft) {
                    details = new PlottedDetail(aircraft);
                    details.mapIcon = this.createIcon(details, mapZoomLevel, isSelectedAircraft);
                    var markerOptions: IMapMarkerSettings = {
                        clickable: true,
                        draggable: false,
                        flat: true,
                        icon: details.mapIcon,
                        visible: true,
                        position: position,
                        tooltip: this.getTooltip(details),
                        zIndex: isSelectedAircraft ? 101 : 100
                    };
                    if(this._SuppressTextOnImages && !details.isSvg) {
                        markerOptions.useMarkerWithLabel = true;
                        markerOptions.mwlLabelInBackground = true;
                        markerOptions.mwlLabelClass = 'markerLabel';
                    }
                    details.mapMarker = this._Map.addMarker(aircraft.id, markerOptions);
                    this.createLabel(details);
                    this.updateTrail(details, isSelectedAircraft, mapZoomLevel, forceRefresh);
                    this._PlottedDetail[aircraft.id] = details;

                    if(this._MapMarkerClusterer) {
                        this._MapMarkerClusterer.addMarker(details.mapMarker, true);
                    }
                }
            }
        }

        /**
         * Removes all of the markers for the aircraft collection passed across.
         */
        private removeOldMarkers(oldAircraft: AircraftCollection)
        {
            oldAircraft.foreachAircraft((aircraft: Aircraft) => {
                var details = this._PlottedDetail[aircraft.id];
                if(details) {
                    this.removeDetails(details);
                }
            });
        }

        /**
         * Removes every marker from the map.
         */
        private removeAllMarkers()
        {
            var allPlottedAircraftIds = this.getPlottedAircraftIds();
            var length = allPlottedAircraftIds.length;
            for(var i = 0;i < length;++i) {
                var aircraftId = allPlottedAircraftIds[i];
                var details = this._PlottedDetail[aircraftId];
                this.removeDetails(details);
            }
        }

        /**
         * Returns true if the aircraft is being plotted on the map.
         */
        private isAircraftBeingPlotted(aircraft: Aircraft) : boolean
        {
            return !!(aircraft && this._PlottedDetail[aircraft.id]);
        }

        /**
         * Destroys the PlottedDetail passed across and removes it from the internal _PlottedDetail associative array.
         */
        private removeDetails(details: PlottedDetail)
        {
            if(details.mapMarker) {
                if(this._MapMarkerClusterer) {
                    this._MapMarkerClusterer.removeMarker(details.mapMarker, true);
                }
                this._Map.destroyMarker(details.mapMarker);
            }
            this.removeTrail(details);

            details.mapIcon = null;
            details.aircraft = null;
            details.mapMarker = null;
            details.mapPolylines = [];
            delete this._PlottedDetail[details.id];
            details.resetId();
            details.pinTexts = null;
            details.iconUrl = null;
        }

        /**
         * Returns true if the details for the marker's image have changed.
         */
        private haveIconDetailsChanged(details: PlottedDetail, mapZoomLevel: number)
        {
            var result = false;
            var aircraft = details.aircraft;

            if(!result) {
                if(this.allowIconRotation()) {
                    result = details.iconRotation === undefined || details.iconRotation !== this.getIconHeading(aircraft);
                } else {
                    if(details.iconRotation !== undefined) {
                        result = true;
                    }
                }
            }

            if(!result) {
                if(this.allowIconAltitudeStalk(mapZoomLevel)) {
                    result = details.iconAltitudeStalkHeight == undefined || details.iconAltitudeStalkHeight !== this.getIconAltitudeStalkHeight(aircraft);
                } else {
                    if(details.iconAltitudeStalkHeight !== undefined) {
                        result = true;
                    }
                }
            }

            if(!result && (!this._SuppressTextOnImages || details.isSvg) && (!details.mapMarker || !details.mapMarker.isMarkerWithLabel)) {
                if(this.allowPinTexts()) {
                    result = this.havePinTextDependenciesChanged(aircraft);
                } else {
                    if(details.pinTexts.length !== 0) {
                        result = true;
                    }
                }
            }

            return result;
        }

        /**
         * Creates a VRS.MapIcon for the marker for the aircraft detail passed across.
         */
        private createIcon(details: PlottedDetail, mapZoomLevel: number, isSelectedAircraft: boolean) : IMapIcon
        {
            var aircraft = details.aircraft;
            var marker = this.getAircraftMarkerSettings(aircraft);
            var useSvg = marker.useEmbeddedSvg();
            var isHighDpi = VRS.browserHelper.isHighDpi();

            details.isSvg = !!useSvg;

            var size = marker.getSize();
            size = { width: size.width, height: size.height };
            var anchorY = Math.floor(size.height / 2);
            var suppressPinText = details.isSvg ? false : this._SuppressTextOnImages || (details.mapMarker && details.mapMarker.isMarkerWithLabel);

            if(!this.allowIconRotation() || !marker.getCanRotate()) {
                details.iconRotation = undefined;
            } else {
                details.iconRotation = this.getIconHeading(aircraft);
            }

            var blankPixelsAtBottom = 0;
            if(!suppressPinText) {
                if(!this.allowPinTexts()) {
                    if(details.pinTexts.length > 0) {
                        details.pinTexts = [];
                    }
                } else {
                    details.pinTexts = this.getPinTexts(aircraft);
                    size.height += (this._Settings.pinTextLineHeight * details.pinTexts.length);
                    if(size.width <= this._Settings.pinTextMarkerWidth) {
                        size.width = this._Settings.pinTextMarkerWidth;
                    } else {
                        size.width += size.width % 4;
                    }

                    // The text scaling works best if the icon height and width are multiples of 4. We already ensure that
                    // the width is a multiple of 4 so we just need to add pixels to adjust the height.
                    blankPixelsAtBottom = size.height % 4;
                    size.height += blankPixelsAtBottom;
                }
            }
            var pinTextLines = suppressPinText ? 0 : details.pinTexts.length;

            var hasAltitudeStalk = false;
            if(!marker.getIsAircraft() || !this.allowIconAltitudeStalk(mapZoomLevel)) {
                details.iconAltitudeStalkHeight = undefined;
            } else {
                hasAltitudeStalk = true;
                details.iconAltitudeStalkHeight = this.getIconAltitudeStalkHeight(aircraft);
                size.height += details.iconAltitudeStalkHeight + 2;
                anchorY = size.height - blankPixelsAtBottom;
            }

            var centreX = Math.floor(size.width / 2);

            var labelAnchor = null;
            if(suppressPinText) {
                labelAnchor = {
                    x: size.width - centreX,
                    y: hasAltitudeStalk ? 3 : anchorY - size.height
                };
            }

            var requestSize = size;
            var multiplier = 1;
            if(!useSvg && isHighDpi) {
                multiplier = 2;
                requestSize = {
                    width: size.width * multiplier,
                    height: size.height * multiplier
                };
            }

            var url = marker.getFolder();
            url += '/top';
            url += '/Wdth-' + requestSize.width;
            url += '/Hght-' + requestSize.height;
            if(isHighDpi) {
                url += '/hiDpi';
            }
            if(details.iconRotation || details.iconRotation === 0) {
                url += '/Rotate-' + details.iconRotation;
            }
            if(hasAltitudeStalk) {
                url += '/Alt-' + (details.iconAltitudeStalkHeight * multiplier);
                url += '/CenX-' + (centreX * multiplier);
            }
            if(pinTextLines > 0) {
                for(var i = 0;i < pinTextLines;++i) {
                    url += '/PL' + (i + 1) + '-' + encodeURIComponent(details.pinTexts[i]);
                }
            }
            url += '/' + (isSelectedAircraft ? marker.getSelectedFileName() : marker.getNormalFileName());

            var urlChanged = details.iconUrl !== url;
            details.iconUrl = url;

            if(useSvg && urlChanged) {
                var svg = this._SvgGenerator.generateAircraftMarker(
                    marker.getEmbeddedSvg(),
                    marker.getSvgFillColour(aircraft, isSelectedAircraft),
                    requestSize.width,
                    requestSize.height,
                    details.iconRotation,
                    hasAltitudeStalk,
                    pinTextLines > 0 ? details.pinTexts : null,
                    this._Settings.pinTextLineHeight,
                    isHighDpi
                );
                var svgText = this._SvgGenerator.serialiseSvg(svg);
                url = 'data:image/svg+xml;charset=UTF-8;base64,' + VRS.stringUtility.safeBtoa(svgText);
                svg = null;
            }

            return !urlChanged ? null : new VRS.MapIcon(url, size, { x: centreX, y: anchorY }, { x: 0, y: 0 }, size, labelAnchor);
        }

        /**
         * Returns the aircraft marker to use for the aircraft passed across.
         */
        private getAircraftMarkerSettings(aircraft: Aircraft) : AircraftMarker
        {
            var result: AircraftMarker = null;

            if(aircraft) {
                var markers = this._Settings.aircraftMarkers;
                var onlyUsePre22Icons = this._Settings.plotterOptions.getOnlyUsePre22Icons();
                var length = markers.length;
                for(var i = 0;i < length;++i) {
                    result = markers[i];
                    if(onlyUsePre22Icons && !result.getIsPre22Icon()) {
                        continue;
                    }
                    if(result.matchesAircraft(aircraft)) {
                        break;
                    }
                }
            }

            return result;
        }

        /**
         * Returns true if the marker images are allowed to rotate.
         */
        private allowIconRotation() : boolean
        {
            return this._Settings.allowRotation;
        }

        /**
         * Gets the degree of rotation for the image for an aircraft's marker.
         */
        private getIconHeading(aircraft: Aircraft) : number
        {
            var rotationGranularity = this._Settings.rotationGranularity;
            var heading = aircraft.heading.val;
            if(isNaN(heading)) {
                heading = 0;
            } else {
                heading = Math.round(heading / rotationGranularity) * rotationGranularity;
            }

            return heading;
        }

        /**
         * Returns true if altitude stalks can be shown at the map zoom level passed across.
         */
        private allowIconAltitudeStalk(mapZoomLevel: number) : boolean
        {
            return VRS.globalOptions.aircraftMarkerAllowAltitudeStalk &&
                   this._Settings.plotterOptions.getShowAltitudeStalk() &&
                   (!this._Settings.plotterOptions.getSuppressAltitudeStalkWhenZoomedOut() || mapZoomLevel >= this._Settings.suppressAltitudeStalkAboveZoom);
        }

        /**
         * Gets the height in pixels for the aircraft passed across.
         */
        private getIconAltitudeStalkHeight(aircraft: Aircraft) : number
        {
            var result = aircraft.isOnGround ? 0 : aircraft.altitude.val;
            if(isNaN(result)) {
                result = 0;
            } else {
                result = Math.max(0, Math.min(result, 35000));
                result = Math.round(result / 2500) * 5;
            }

            return result;
        }

        /**
         * Returns true if the pin texts can be shown.
         */
        private allowPinTexts() : boolean
        {
            var result = this._Settings.plotterOptions.getShowPinText();
            if(result && VRS.serverConfig) {
                result = VRS.serverConfig.pinTextEnabled();
            }

            return result;
        }

        /**
         * Returns true if any pin text may have changed for an aircraft.
         */
        private havePinTextDependenciesChanged(aircraft: Aircraft) : boolean
        {
            var result = false;

            var pinTexts = this._Settings.plotterOptions.getPinTexts();
            var length = pinTexts.length;
            for(var i = 0;i < length;++i) {
                var handler = VRS.renderPropertyHandlers[pinTexts[i]];
                if(handler && handler.hasChangedCallback(aircraft)) {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /**
         * Gets an array of pin text values (i.e. the text from the aircraft, not VRS.RenderProperty enums) for an aircraft.
         */
        private getPinTexts(aircraft: Aircraft) : string[]
        {
            var result: string[] = [];
            var suppressBlankLines = this._Settings.plotterOptions.getHideEmptyPinTextLines();

            if(this._Settings.getCustomPinTexts) {
                result = this._Settings.getCustomPinTexts(aircraft) || [];
            } else {
                var options: AircraftRenderOptions = {
                    unitDisplayPreferences: this._UnitDisplayPreferences,
                    distinguishOnGround:    true
                };

                var length = Math.min(this._Settings.plotterOptions.getPinTextLines(), VRS.globalOptions.aircraftMarkerMaximumPinTextLines);
                for(var i = 0;i < length;++i) {
                    var renderProperty = this._Settings.plotterOptions.getPinText(i);
                    if(renderProperty === VRS.RenderProperty.None) continue;
                    var handler = VRS.renderPropertyHandlers[renderProperty];
                    var text = handler ? handler.contentCallback(aircraft, options, VRS.RenderSurface.Marker) || '' : '';
                    if(!suppressBlankLines || text) {
                        result.push(text);
                    }
                }
            }

            return result;
        }

        /**
         * Returns true if the details for a label on the marker have changed.
         */
        private haveLabelDetailsChanged(details: PlottedDetail) : boolean
        {
            var result = false;

            if(!details.isSvg) {
                if(this._SuppressTextOnImages || (details.mapMarker && details.mapMarker.isMarkerWithLabel)) {
                    if(this.allowPinTexts()) {
                        result = this.havePinTextDependenciesChanged(details.aircraft);
                    } else {
                        if(details.pinTexts.length !== 0) {
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        /**
         * Sets the label content for the marker passed across.
         */
        private createLabel(details: PlottedDetail)
        {
            if(!details.isSvg) {
                if(this._SuppressTextOnImages && details.mapMarker && details.mapMarker.isMarkerWithLabel) {
                    if(this.allowPinTexts()) {
                        details.pinTexts = this.getPinTexts(details.aircraft);
                    } else {
                        if(details.pinTexts.length > 0) {
                            details.pinTexts = [];
                        }
                    }

                    var labelText = '';
                    var length = details.pinTexts.length;
                    for(var i = 0;i < length;++i) {
                        if(labelText.length) {
                            labelText += '<br/>';
                        }
                        labelText += '<span>&nbsp;' + VRS.stringUtility.htmlEscape(details.pinTexts[i]) + '&nbsp;</span>'
                    }

                    var marker = details.mapMarker;
                    if(labelText.length === 0 || !details.mapIcon) {
                        marker.setLabelVisible(false);
                    } else {
                        if(!marker.getLabelVisible()) {
                            marker.setLabelVisible(true);
                        }
                        marker.setLabelAnchor(details.mapIcon.labelAnchor);
                        marker.setLabelContent(labelText);
                    }
                }
            }
        }

        /**
         * Updates the aircraft trail for the aircraft passed aross.
         */
        private updateTrail(details: PlottedDetail, isAircraftSelected: boolean, mapZoomLevel: number, forceRefresh: boolean)
        {
            if(VRS.globalOptions.suppressTrails) {
                return;
            }

            var aircraft = details.aircraft;
            var showTrails = false;
            switch(this._Settings.plotterOptions.getTrailDisplay()) {
                case VRS.TrailDisplay.None:         break;
                case VRS.TrailDisplay.AllAircraft:  showTrails = true; break;
                case VRS.TrailDisplay.SelectedOnly: showTrails = isAircraftSelected; break;
            }
            if(showTrails && !aircraft.isAircraftSpecies() && !VRS.globalOptions.aircraftMarkerShowNonAircraftTrails) {
                showTrails = false;
            }
            if(showTrails && mapZoomLevel <= this.getHideTrailsMaxZoom()) {
                showTrails = false;
            }

            if(forceRefresh) {
                this.removeTrail(details);
            }

            if(!showTrails) {
                if(details.mapPolylines.length) {
                    this.removeTrail(details);
                }
            } else {
                var trailType = this._Settings.plotterOptions.getTrailType();
                var trail;
                var isMonochrome = false;
                var isFullTrail = false;
                switch(trailType) {
                    case VRS.TrailType.Full:
                        isMonochrome = true;
                        // fall through to other full trail cases
                    case VRS.TrailType.FullAltitude:
                    case VRS.TrailType.FullSpeed:
                        isFullTrail = true;
                        trail = aircraft.fullTrail;
                        break;
                    case VRS.TrailType.Short:
                        isMonochrome = true;
                        // fall through to rest of short trail cases
                    default:
                        trail = aircraft.shortTrail;
                        break;
                }

                if(details.mapPolylines.length && details.polylineTrailType !== trailType) {
                    this.removeTrail(details);
                }
                if(trail.trimStartCount) {
                    this.trimShortTrailPoints(details, trail);
                }

                var polylines = details.mapPolylines;
                var lastLine = polylines.length ? polylines[polylines.length - 1] : null;

                if(!lastLine) {
                    this.createTrail(details, trail, trailType, isAircraftSelected, isMonochrome);
                } else {
                    var width = this.getTrailWidth(isAircraftSelected);
                    if(lastLine.getStrokeWeight() !== width) {
                        var length = polylines.length;
                        for(var i = 0;i < length;++i) {
                            polylines[i].setStrokeWeight(width);
                        }
                    }

                    if(isMonochrome) {
                        var colour = this.getMonochromeTrailColour(isAircraftSelected);
                        if(lastLine.getStrokeColour() !== colour) {
                            lastLine.setStrokeColour(colour);
                        }
                    }

                    if(details.polylinePathUpdateCounter !== aircraft.updateCounter && trail.chg) {
                        this.synchroniseAircraftAndMapPolylinePaths(details, trailType, trail, isAircraftSelected, isMonochrome, isFullTrail);
                    }
                }

                details.polylineTrailType = trailType;
                details.polylinePathUpdateCounter = aircraft.updateCounter;
            }
        }

        /**
         * Gets the colour to paint the trail in assuming the use of a single colour for the entire trail.
         */
        private getMonochromeTrailColour(isAircraftSelected: boolean) : string
        {
            return isAircraftSelected ? this._Settings.selectedTrailColour : this._Settings.normalTrailColour;
        }

        /**
         * Gets the colour for a coordinate in a coloured trail.
         */
        private getCoordinateTrailColour(coordinate: FullTrailValue | ShortTrailValue, trailType: TrailTypeEnum) : string
        {
            var result = null;
            switch(trailType) {
                case VRS.TrailType.FullAltitude:
                case VRS.TrailType.ShortAltitude:
                    result = VRS.colourHelper.colourToCssString(
                        VRS.colourHelper.getColourWheelScale(
                            coordinate.altitude,
                            VRS.globalOptions.aircraftMarkerAltitudeTrailLow,
                            VRS.globalOptions.aircraftMarkerAltitudeTrailHigh,
                            true,
                            true
                        )
                    );
                    break;
                case VRS.TrailType.FullSpeed:
                case VRS.TrailType.ShortSpeed:
                    result = VRS.colourHelper.colourToCssString(
                        VRS.colourHelper.getColourWheelScale(
                            coordinate.speed,
                            VRS.globalOptions.aircraftMarkerSpeedTrailLow,
                            VRS.globalOptions.aircraftMarkerSpeedTrailHigh,
                            true,
                            true
                        )
                    );
                    break;
                default:
                    throw 'The trail type ' + trailType + ' does not show multiple colours on a single trail';
            }

            return result;
        }

        /**
         * Gets the width of a trail.
         */
        private getTrailWidth(isAircraftSelected: boolean) : number
        {
            return isAircraftSelected ? this._Settings.selectedTrailWidth : this._Settings.normalTrailWidth;
        }

        /**
         * Gets a portion of the polyline path for an aircraft's trail.
         */
        private getTrailPath(trail: TrailArray, start: number, count: number, aircraft: Aircraft, trailType: TrailTypeEnum, isAircraftSelected: boolean, isMonochrome: boolean) : AircraftPlotter_ColourPoint[]
        {
            var result: AircraftPlotter_ColourPoint[] = [];
            var length = trail.arr.length;
            if(start === undefined) start = 0;
            if(start > length) throw 'Cannot get the trail from index ' + start + ', there are only ' + length + ' coordinates';
            if(count === undefined) count = length - start;
            if(start + count > length) throw 'Cannot get ' + count + ' points from index ' + start + ', there are only ' + length + ' coordinates';

            var colour = aircraft && isMonochrome ? this.getMonochromeTrailColour(isAircraftSelected) : null;

            var end = start + count;
            for(var i = start;i < end;++i) {
                var coord = trail.arr[i];
                if(aircraft && !isMonochrome) {
                    colour = this.getCoordinateTrailColour(coord, trailType);
                }
                result.push({ lat: coord.lat, lng: coord.lng, colour: colour });
            }

            return result;
        }

        /**
         * Creates a new trail for an aircraft.
         */
        private createTrail(details: PlottedDetail, trail: TrailArray, trailType: TrailTypeEnum, isAircraftSelected: boolean, isMonochrome: boolean)
        {
            if(details.mapPolylines.length) throw 'Cannot create a trail for aircraft ID ' + details.id + ', one already exists';
            var aircraft = details.aircraft;
            var path = this.getTrailPath(trail, undefined, undefined, aircraft, trailType, isAircraftSelected, isMonochrome);

            if(path.length) {
                var weight = this.getTrailWidth(isAircraftSelected);
                if(!isMonochrome) {
                    this.addMultiColouredPolylines(details, path, weight, null);
                } else {
                    details.mapPolylines.push(this._Map.addPolyline(aircraft.id, {
                        clickable:      false,
                        draggable:      false,
                        editable:       false,
                        geodesic:       true,
                        strokeColour:   path[0].colour,
                        strokeWeight:   weight,
                        strokeOpacity:  1,
                        path:           path
                    }));
                }
            }
        }

        /**
         * Adds multiple polylines to the map to represent a part of the trail for an aircraft.
         */
        private addMultiColouredPolylines(details: PlottedDetail, path: AircraftPlotter_ColourPoint[], weight: number, fromCoord: ShortTrailValue | FullTrailValue)
        {
            var aircraft = details.aircraft;

            var segments = [];
            if(fromCoord) {
                segments.push({
                    lat: fromCoord.lat,
                    lng: fromCoord.lng
                });
            }

            var length = path.length;
            var nextSegment = null;
            var firstAdd = true;
            for(var i = 0;i < length;++i) {
                var segment = nextSegment === null ? path[i] : nextSegment;
                nextSegment = i + 1 === length ? null : path[i + 1];
                segments.push(segment);

                if(nextSegment && nextSegment.colour === segment.colour) {
                    continue;
                }
                if(nextSegment) {
                    segments.push(nextSegment);
                }

                if(segments.length > 1) {
                    if(firstAdd) {
                        // Sometimes we can end up with the last line plotted starting at the same point as the line we
                        // want to add (particularly when the last update included a temporary point to bring a trail
                        // up to the current location of the aircraft). If this happens we want to zap that line and
                        // replace it entirely with our new one.
                        firstAdd = false;
                        var lastLine = details.mapPolylines.length ? details.mapPolylines[details.mapPolylines.length - 1] : null;
                        if(lastLine) {
                            var firstPoint = lastLine.getFirstLatLng();
                            var firstSegment = segments[0];
                            if(firstPoint && this.isSameLatLng(firstPoint, firstSegment)) {
                                this._Map.destroyPolyline(lastLine);
                                details.mapPolylines.splice(-1, 1);
                            }
                        }
                    }

                    var id = aircraft.id.toString() + '$' + details.nextPolylineId++;
                    details.mapPolylines.push(this._Map.addPolyline(id, {
                        clickable:      false,
                        draggable:      false,
                        editable:       false,
                        geodesic:       true,
                        strokeColour:   segment.colour,
                        strokeWeight:   weight,
                        strokeOpacity:  1,
                        path:           segments
                    }));
                }

                segments = [];
            }
        }

        /**
         * Returns true if two coordinates are roughly the same.
         * @param lhs
         * @param rhs
         */
        private isSameLatLng(lhs: ILatLng, rhs: ILatLng) : boolean
        {
            return Math.abs(lhs.lat - rhs.lat) < 0.0000001 && Math.abs(lhs.lng - rhs.lng) < 0.0000001;
        }

        /**
         * Removes the trail for the aircraft passed across.
         */
        private removeTrail(details: PlottedDetail)
        {
            var length = details.mapPolylines.length;
            if(length) {
                for(var i = 0;i < length;++i) {
                    this._Map.destroyPolyline(details.mapPolylines[i]);
                }
                details.mapPolylines = [];
                details.polylinePathUpdateCounter = undefined;
                details.polylineTrailType = undefined;
            }
        }

        /**
         * Updates an existing map polyline to reflect changes to an aircraft' trail.
         */
        private synchroniseAircraftAndMapPolylinePaths(details: PlottedDetail, trailType: TrailTypeEnum, trail: TrailArray, isAircraftSelected: boolean, isMonochrome: boolean, isFullTrail: boolean)
        {
            var polylines = details.mapPolylines;
            var polylinesLength = polylines.length;
            var trailLength = trail.arr.length;

            if(isFullTrail && trail.chg && trail.chgIdx === -1 && trailLength > 0 && (<FullTrailValue>trail.arr[trailLength - 1]).chg) {
                // The last coordinate of the polyline has been moved - we need to update the last point on the trail
                var changedTrail = trail.arr[trailLength - 1];
                this._Map.replacePolylinePointAt(polylines[polylinesLength - 1], -1, { lat: changedTrail.lat, lng: changedTrail.lng });
            } else {
                if(trail.chgIdx !== -1 && trail.chgIdx < trail.arr.length) {
                    var path = this.getTrailPath(trail, trail.chgIdx, undefined, details.aircraft, trailType, isAircraftSelected, isMonochrome);
                    if(isMonochrome) {
                        this._Map.appendToPolyline(polylines[polylinesLength - 1], path, false);
                    } else {
                        var weight = this.getTrailWidth(isAircraftSelected);
                        var fromCoord = trail.chgIdx > 0 ? trail.arr[trail.chgIdx - 1] : null;
                        this.addMultiColouredPolylines(details, path, weight, fromCoord);
                    }
                }
            }
        }

        /**
         * Removes points from the oldest part of the trail.
         */
        private trimShortTrailPoints(details: PlottedDetail, trail: TrailArray)
        {
            // The old code here (see history) only worked for monochrome trails. Life
            // gets harder with multi-coloured trails because they are broken into multiple
            // paths and those paths have extra points added to them to get everything to
            // join together.
            //
            // The new approach taken here simply looks for the last short trail point in
            // the polyline paths and deletes everything up until that point. If it can't
            // find the last short trail point then all of the polylines end up getting
            // deleted.
            //
            // This should work for almost all cases. Where it will fail will be for
            // aircraft that repeatedly move through the same location. If you imagine
            // locations are represented by letters then you could have a short trail and
            // an extant polyline path that look like this:
            //
            //         -8s -7s -6s -5s -4s -3s -2s -1s NOW
            // Trail:               A   B   C   B   A   X
            // Path:    C   A   B   A   B   C   B
            //
            // This code will see that the last trail position (at -4 seconds) is A and
            // then look in the path for the last occurrance of A, and delete everything
            // before it. It will trim the path at -7s instead of -5s.
            //
            // Ideally we would have position ticks against the polyline path positions,
            // in which case this would be a trivial exercise... but we don't, and adding
            // them would be painful, so this will have to do for now.

            if(trail.trimStartCount) {      // <-- we can probably replace trimStartCount with a true/false flag, we're no longer using it to count points to delete
                // The trail array has the oldest position first and the most recent position last.
                // The plotted detail can have many line segments, one for each colour in the trail,
                // with the oldest segment first. Each segment is made up of a path, the oldest
                // position is the first in each segment.

                if(trail.arr.length === 0) {
                    this.removeTrail(details);
                } else {
                    var oldestTrailPosition = trail.arr[0];
                    var matchIdx = -1;

                    while(matchIdx === -1 && details.mapPolylines.length > 0) {
                        var polyline = details.mapPolylines[0];
                        var path = polyline.getPath();
                        var pathLength = path.length;

                        for(var pathIdx = 0;pathIdx < pathLength;++pathIdx) {
                            if(this.isSameLatLng(oldestTrailPosition, path[pathIdx])) {
                                matchIdx = pathIdx;
                                break;
                            }
                        }

                        if(matchIdx > 0) {
                            this._Map.trimPolyline(polyline, matchIdx, true);
                        } else if(matchIdx === -1) {
                            this._Map.destroyPolyline(polyline);
                            details.mapPolylines.splice(0, 1);
                        }
                    }
                }
            }
        }

        /**
         * Returns true if the tooltip details have changed for the aircraft passed across.
         */
        private haveTooltipDetailsChanged(details: PlottedDetail) : boolean
        {
            var result = false;
            if(this._Settings.showTooltips) {
                var aircraft = details.aircraft;
                result = aircraft.icao.chg ||
                    aircraft.modelIcao.chg ||
                    aircraft.registration.chg ||
                    aircraft.callsign.chg ||
                    aircraft.callsignSuspect.chg ||
                    aircraft.operator.chg ||
                    aircraft.operatorIcao.chg ||
                    aircraft.from.chg ||
                    aircraft.to.chg ||
                    aircraft.via.chg;
            }

            return result;
        }

        /**
         * Gets the tooltip for the aircraft passed across.
         */
        private getTooltip(details: PlottedDetail) : string
        {
            var result = '';

            if(this._Settings.showTooltips) {
                var aircraft = details.aircraft;
                var addToResult = function(text) { if(text) { if(result) result += ' || '; result += text; } };

                addToResult(aircraft.formatIcao());
                addToResult(aircraft.formatModelIcao());
                addToResult(aircraft.formatRegistration());
                addToResult(aircraft.formatCallsign(VRS.globalOptions.aircraftFlagUncertainCallsigns));
                addToResult(aircraft.formatOperatorIcaoAndName());
                addToResult(aircraft.formatRouteFull());
            }

            return result;
        }

        /**
         * Selects the aircraft with the unique ID passed across. This only works with the aircraft list - if you are
         * using a custom list then you need to supply a getSelectedAircraft method.
         */
        private selectAircraftById(id: number)
        {
            if(this._Settings.aircraftList) {
                var details = this._PlottedDetail[id];
                if(details) {
                    this._Settings.aircraftList.setSelectedAircraft(details.aircraft, true);
                }
            }
        }

        /**
         * Moves the map centre to the selected aircraft or does nothing if either the option to move the map is disabled
         * or there is no selected aircraft or the selected aircraft does not have a position.
         */
        private moveMapToSelectedAircraft(selectedAircraft?: Aircraft)
        {
            if(this._MovingMap) {
                if(!selectedAircraft) {
                    selectedAircraft = this._GetSelectedAircraft();
                }
                this.moveMapToAircraft(selectedAircraft);
            }
        }

        /**
         * Moves the map to centre on the aircraft passed across. If the aircraft is not supplied, or it has no position,
         * then this does nothing.
         */
        moveMapToAircraft(aircraft: Aircraft)
        {
            if(aircraft && aircraft.hasPosition()) {
                this._Map.setCenter(aircraft.getPosition());
            }
        }

        /**
         * Draws (or removes, or moves) the range circles on the map so that they are centered on the current location.
         */
        refreshRangeCircles(forceRefresh?: boolean)
        {
            var currentLocation = VRS.currentLocation ? VRS.currentLocation.getCurrentLocation() : null;

            if(!currentLocation || !this._Settings.allowRangeCircles || !this._Settings.plotterOptions.getShowRangeCircles()) {
                this.destroyRangeCircles();
            } else {
                if(forceRefresh || !this._RangeCircleCentre || this._RangeCircleCentre.lat !== currentLocation.lat || this._RangeCircleCentre.lng !== currentLocation.lng) {
                    var plotterOptions = this._Settings.plotterOptions;

                    var baseCircleId = -1000;
                    var intervalMetres = VRS.unitConverter.convertDistance(plotterOptions.getRangeCircleInterval(), plotterOptions.getRangeCircleDistanceUnit(), VRS.Distance.Kilometre) * 1000;
                    var countCircles = plotterOptions.getRangeCircleCount();
                    for(let i = 0;i < countCircles;++i) {
                        var isOdd = i % 2 === 0;

                        var circle = this._RangeCircleCircles.length > i ? this._RangeCircleCircles[i] : null;
                        var radius = intervalMetres * (i + 1);
                        var colour = isOdd ? plotterOptions.getRangeCircleOddColour() : plotterOptions.getRangeCircleEvenColour();
                        var weight = isOdd ? plotterOptions.getRangeCircleOddWeight() : plotterOptions.getRangeCircleEvenWeight();

                        if(circle) {
                            circle.setCenter(currentLocation);
                            circle.setRadius(radius);
                            circle.setStrokeColor(colour);
                            circle.setStrokeWeight(weight);
                        } else {
                            this._RangeCircleCircles.push(this._Map.addCircle(baseCircleId - i, {
                                center:         currentLocation,
                                radius:         radius,
                                strokeColor:    colour,
                                strokeWeight:   weight
                            }));
                        }
                    }

                    if(countCircles < this._RangeCircleCircles.length) {
                        for(let i = countCircles;i < this._RangeCircleCircles.length;++i) {
                            this._Map.destroyCircle(this._RangeCircleCircles[i]);
                        }
                        this._RangeCircleCircles.splice(countCircles, this._RangeCircleCircles.length - countCircles);
                    }

                    this._RangeCircleCentre = currentLocation;
                }
            }
        }

        /**
         * Destroys the range circles and releases any resources allocated to them.
         */
        private destroyRangeCircles()
        {
            if(this._RangeCircleCircles.length) {
                $.each(this._RangeCircleCircles, (idx, circle) => {
                    this._Map.destroyCircle(circle);
                });
                this._RangeCircleCircles = [];
                this._RangeCircleCentre = null;
            }
        }

        /**
         * Returns the VRS.MapMarker for the aircraft passed across or null / undefined if no such marker exists.
         */
        getMapMarkerForAircraft(aircraft: Aircraft) : IMapMarker
        {
            var result = null;
            if(aircraft) {
                var detail = this._PlottedDetail[aircraft.id];
                if(detail) {
                    result = detail.mapMarker;
                }
            }

            return result;
        }

        /**
         * Returns the VRS.Aircraft for the map marker passed across or null / undefined if the marker either
         * does not belong to the plotter or isn't associated with an aircraft.
         */
        getAircraftForMarkerId(mapMarkerId: number)
        {
            var result = null;

            var details = this._PlottedDetail[mapMarkerId];
            if(details) {
                result = details.aircraft;
            }

            return result;
        }

        /**
         * A diagnostics method that returns the plotted detail for the aircraft.
         */
        diagnosticsGetPlottedDetail(aircraft: Aircraft) : Object
        {
            return this._PlottedDetail[aircraft.id];
        }

        /**
         * Called whenever an aircraft plotter options property is changed.
         */
        private optionsPropertyChanged()
        {
            if(this._MapMarkerClusterer) {
                var newMaxZoom = this._Settings.plotterOptions.getAircraftMarkerClustererMaxZoom();
                if(newMaxZoom !== this._MapMarkerClusterer.getMaxZoom()) {
                    this._MapMarkerClusterer.setMaxZoom(newMaxZoom);
                }
            }

            if(!this._Suspended) {
                this.refreshMarkers(null, null, true);
            }
        }

        /**
         * Called whenever a range circles property changes.
         */
        private optionsRangePropertyChanged()
        {
            if(!this._Suspended) {
                this.refreshRangeCircles(true);
            }
        }

        /**
         * Called when the XHR call to refresh the aircraft list is being built.
         */
        private fetchingList(xhrParams: IAircraftListRequestQueryString)
        {
            if(!VRS.globalOptions.suppressTrails) {
                var trailType = this._Settings.plotterOptions.getTrailType();
                switch(trailType) {
                    case VRS.TrailType.Full:            xhrParams.trFmt = 'f'; break;
                    case VRS.TrailType.Short:           xhrParams.trFmt = 's'; break;
                    case VRS.TrailType.FullAltitude:    xhrParams.trFmt = 'fa'; break;
                    case VRS.TrailType.ShortAltitude:   xhrParams.trFmt = 'sa'; break;
                    case VRS.TrailType.FullSpeed:       xhrParams.trFmt = 'fs'; break;
                    case VRS.TrailType.ShortSpeed:      xhrParams.trFmt = 'ss'; break;
                }

                if(this._PreviousTrailTypeRequested && this._PreviousTrailTypeRequested !== trailType) {
                    xhrParams.refreshTrails = '1';
                }
                this._PreviousTrailTypeRequested = trailType;
            }
        }

        /**
         * Called when the aircraft list is updated.
         */
        private refreshMarkersOnListUpdate(newAircraft: AircraftCollection, oldAircraft: AircraftCollection)
        {
            if(!this._Suspended) {
                this.refreshMarkers(newAircraft, oldAircraft);
            }
        }

        /**
         * Called when the selected aircraft changes.
         */
        private refreshSelectedAircraft(oldSelectedAircraft: Aircraft)
        {
            if(!this._Suspended) {
                var bounds = this._Map.getBounds();
                var mapZoomLevel = this._Map.getZoom();

                if(this.isAircraftBeingPlotted(oldSelectedAircraft)) {
                    this.refreshAircraftMarker(oldSelectedAircraft, true, false, bounds, mapZoomLevel, false);
                }

                var selectedAircraft = this._GetSelectedAircraft();

                if(this.isAircraftBeingPlotted(selectedAircraft)) {
                    this.refreshAircraftMarker(selectedAircraft, true, false, bounds, mapZoomLevel, true);
                }
            }
        }

        /**
         * Called when a display unit has changed. Refreshes the aircraft markers but only if the VRS.RenderProperty passed
         * across is in use.
         */
        private refreshMarkersIfUsingPinText(renderProperty: RenderPropertyEnum)
        {
            if(!this._Suspended) {
                if($.inArray(renderProperty, this._Settings.plotterOptions.getPinTexts()) !== -1) {
                    this.refreshMarkers(null, null, true);
                }
            }
        }

        /**
         * Called when the current location has changed.
         */
        private currentLocationChanged()
        {
            if(!this._Suspended) {
                this.refreshRangeCircles();
            }
        }

        /**
         * Called when the configuration has changed.
         */
        private configurationChanged()
        {
            this.configureSuppressTextOnImages();

            if(!this._Suspended) {
                this.removeAllMarkers();
                this.refreshMarkers(null, null, true);
            }
        }
    }
} 
