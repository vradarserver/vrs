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

(function(VRS, $, undefined)
{
    //region AircraftMarker
    /**
     * Describes the properties of an aircraft marker.
     * @param {VRS_AIRCRAFTMARKER_SETTINGS} settings
     * @constructor
     */
    VRS.AircraftMarker = function(settings)
    {
        /** @type {string} */
        var _NormalFileName = settings.normalFileName || null;
        this.getNormalFileName = function()                     { return _NormalFileName; };
        this.setNormalFileName = function(/**string*/ value)    { _NormalFileName = value; };

        /** @type {string} */
        var _SelectedFileName = settings.selectedFileName || settings.normalFileName || null;
        this.getSelectedFileName = function()                   { return _SelectedFileName; };
        this.setSelectedFileName = function(/**string*/ value)  { _SelectedFileName = value; };

        /** @type {VRS_SIZE} */
        var _Size = settings.size || { width: 35, height: 35};
        this.getSize = function()                               { return _Size; };
        this.setSize = function(/**VRS_SIZE*/ value)            { _Size = value; };

        /** @type {bool} */
        var _IsAircraft = settings.isAircraft || true;
        this.getIsAircraft = function()                         { return _IsAircraft; };
        this.setIsAircraft = function(/**bool*/ value)          { _IsAircraft = value; };

        /** @type {bool} */
        var _CanRotate = settings.canRotate || true;
        this.getCanRotate = function()                          { return _CanRotate; };
        this.setCanRotate = function(/**bool*/ value)           { _CanRotate = value; };

        /** @type {function(VRS.Aircraft):bool} */
        var _Matches = settings.matches;
        this.getMatches = function()                                          { return _Matches; };
        this.setMatches = function(/** function(VRS.Aircraft):bool */ value)  { _Matches = value; };

        /**
         * Returns true if the marker can be used to represent the aircraft passed across.
         * @param {VRS.Aircraft} aircraft
         * @returns {boolean}
         */
        this.matchesAircraft = function(aircraft)
        {
            return _Matches ? _Matches(aircraft) : false;
        };
    };
    //endregion

    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
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

    // The order in which these appear in the list is important. Earlier items take precedence over later items.
    VRS.globalOptions.aircraftMarkers = VRS.globalOptions.aircraftMarkers || [
        new VRS.AircraftMarker({
            normalFileName: 'GroundVehicle.png',
            selectedFileName: 'GroundVehicle.png',
            size: { width: 26, height: 24},
            isAircraft: false,
            matches: function(/** VRS.Aircraft */ aircraft) { return aircraft.species.val === VRS.Species.GroundVehicle; }
        }),
        new VRS.AircraftMarker ({
            normalFileName: 'Tower.png',
            selectedFileName: 'Tower.png',
            size: { width: 20, height: 20 },
            isAircraft: false,
            canRotate: false,
            matches: function(/** VRS.Aircraft */ aircraft) { return aircraft.species.val === VRS.Species.Tower; }
        }),
        new VRS.AircraftMarker ({
            normalFileName: 'WtcHeavy.png',
            selectedFileName: 'WtcHeavySelected.png',
            size: { width: 50, height: 50 },
            matches: function(/** VRS.Aircraft */ aircraft) { return aircraft.wakeTurbulenceCat.val === VRS.WakeTurbulenceCategory.Heavy; }
        }),
        new VRS.AircraftMarker({
            normalFileName: 'Airplane.png',
            selectedFileName: 'AirplaneSelected.png',
            size: { width: 35, height: 35 },
            matches: function(/** VRS.Aircraft */ aircraft) { return true; }
        })
    ];
    //endregion

    //region PlottedDetail
    /**
     * Describes an aircraft that we are plotting on the map.
     * @param {VRS.Aircraft} aircraft
     * @constructor
     */
    VRS.PlottedDetail = function(aircraft)
    {
        /**
         * The unique ID of the aircraft.
         * @type {number}
         */
        this.id = aircraft.id;

        /**
         * The aircraft being plotted.
         * @type {VRS.Aircraft}
         */
        this.aircraft = aircraft;

        /**
         * The map marker for the aircraft.
         * @type {VRS.MapMarker=}
         */
        this.mapMarker = undefined;

        /**
         * The image displayed at the map marker for the aircraft.
         * @type {VRS.MapIcon=}
         */
        this.mapIcon = undefined;

        /**
         * The array of lines that describe the trail behind the aircraft.
         * @type {Array.<VRS.MapPolyline>}
         */
        this.mapPolylines = [];

        /**
         * A counter that is incremented every time a polyline is created, and is used to ensure that each polyline has
         * a unique ID.
         * @type {number}
         */
        this.nextPolylineId = 0;

        /**
         * The degree of rotation of the aircraft's image.
         * @type {number=}
         */
        this.iconRotation = undefined;

        /**
         * The height of the altitude stalk in pixels.
         * @type {number=}
         */
        this.iconAltitudeStalkHeight = undefined;

        /**
         * An array of strings drawn onto the map marker.
         * @type {string[]}
         */
        this.pinTexts = [];

        /**
         * The URL of the map marker image.
         * @type {string=}
         */
        this.iconUrl = undefined;

        /**
         * The aircraft's updateCounter as-at the last time the trail was redrawn for the aircraft.
         * @type {number=}
         */
        this.polylinePathUpdateCounter = -1;

        /**
         * A VRS.TrailType value indicating what kind of trail, if any, is currently drawn for the aircraft.
         * @type {VRS.TrailType=}
         */
        this.polylineTrailType = undefined;
    };
    //endregion

    //region AircraftPlotterOptions
    /**
     * Collects together the options for an aircraft plotter.
     * @param {Object}                  settings                                    The settings to apply to the aircraft plotter.
     * @param {string}                 [settings.name]                              The name to use when saving and loading state.
     * @param {boolean}                [settings.showAltitudeStalk]                 True to show altitude stalks, false otherwise.
     * @param {boolean}                [settings.suppressAltitudeStalkWhenZoomed]   True to suppress the display of altitude stalks when zoomed, false otherwise.
     * @param {boolean}                [settings.showPinText]                       True to show text on the markers, false otherwise.
     * @param {VRS.RenderProperty[]}   [settings.pinTexts]                          The pin texts to show on the aircraft markers.
     * @param {number}                 [settings.pinTextLines]                      The number of pin text lines to show.
     * @param {boolean}                [settings.hideEmptyPinTextLines]             True to suppress blank lines in pin text.
     * @param {VRS.TrailDisplay}       [settings.trailDisplay]                      Determines which aircraft are given trails.
     * @param {VRS.TrailType}          [settings.trailType]                         Determines what kind of trail is shown on aircraft.
     * @param {boolean}                [settings.showRangeCircles]                  True if range circles are to be shown on the map.
     * @param {Number}                 [settings.rangeCircleInterval]               The distance between each range circle.
     * @param {VRS.Distance}           [settings.rangeCircleDistanceUnit]           The unit that rangeCircleInterval is in.
     * @param {Number}                 [settings.rangeCircleCount]                  The number of range circles to show.
     * @param {string}                 [settings.rangeCircleOddColour]              The CSS colour for odd range circles.
     * @param {Number}                 [settings.rangeCircleOddWeight]              The pixel width of odd range circles.
     * @param {string}                 [settings.rangeCircleEvenColour]             The CSS colour for even range circles.
     * @param {Number}                 [settings.rangeCircleEvenWeight]             The pixel width of even range circles.
     * @constructor
     */
    VRS.AircraftPlotterOptions = function(settings)
    {
        //region -- Initialise settings
        settings = $.extend({
            name:                               'default',
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
            rangeCircleEvenWeight:              VRS.globalOptions.aircraftMarkerRangeCircleEvenWeight
        }, settings);
        //endregion

        //region -- Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftPlotterOptions'
        });
        var _Events = {
            propertyChanged:            'propertyChanged',
            rangeCirclePropertyChanged: 'rangeCirclePropertyChanged'
        };

        /**
         * True if attempts to raise events are to be suppressed.
         * @type {boolean}
         * @private
         */
        var _SuppressEvents = false;
        //endregion

        //region -- Properties
        /** @type {string} */
        this.getName = function() { return settings.name; };

        /** @type {boolean} */
        var _ShowAltitudeStalk = settings.showAltitudeStalk;
        this.getShowAltitudeStalk = function() { return _ShowAltitudeStalk; };
        this.setShowAltitudeStalk = function(/**bool*/ value) {
            if(_ShowAltitudeStalk !== value) {
                _ShowAltitudeStalk = value;
                raisePropertyChanged();
            }
        };

        var _SuppressAltitudeStalkWhenZoomedOut = settings.suppressAltitudeStalkWhenZoomed;
        this.getSuppressAltitudeStalkWhenZoomedOut = function() { return _SuppressAltitudeStalkWhenZoomedOut; };
        this.setSuppressAltitudeStalkWhenZoomedOut = function(/**bool*/value) {
            if(_SuppressAltitudeStalkWhenZoomedOut !== value) {
                _SuppressAltitudeStalkWhenZoomedOut = value;
                raisePropertyChanged();
            }
        };

        /** @type {boolean} */
        var _ShowPinText = settings.showPinText;
        this.getShowPinText = function() { return _ShowPinText; };
        this.setShowPinText = function(/**bool*/ value) {
            if(_ShowPinText !== value) {
                _ShowPinText = value;
                raisePropertyChanged();
            }
        };

        /**
         * An array of VRS.RenderProperty values representing values to draw onto the marker.
         * @type {VRS.RenderProperty[]}
         * @private
         */
        var _PinTexts = [];
        this.getPinTexts = function() { return _PinTexts; };
        this.getPinText = function(/** Number */ index) { return index >= _PinTexts.length ? VRS.RenderProperty.None : _PinTexts[index]; };
        /**
         * Sets the pin text at the index specified.
         * @param {number}             index The index of the pin text to change.
         * @param {VRS.RenderProperty} value The VRS.RenderProperty property describing the pin text to draw onto the marker. If it is not suitable for rendering onto a marker then it is translated into None.
         */
        this.setPinText = function(index, value) {
            if(index <= VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                if(!VRS.renderPropertyHandlers[value] || !VRS.renderPropertyHandlers[value].isSurfaceSupported(VRS.RenderSurface.Marker)) value = VRS.RenderProperty.None;
                if(that.getPinText[index] !== value) {
                    if(index < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                        while(_PinTexts.length <= index) {
                            _PinTexts.push(_PinTexts.length < settings.pinTexts.length ? settings.pinTexts[_PinTexts.length] : VRS.RenderProperty.None);
                        }
                        _PinTexts[index] = value;
                        raisePropertyChanged();
                    }
                }
            }
        };
        $.each(settings.pinTexts, function(/** Number */idx, /** VRS.RenderProperty */ renderProperty) {
            that.setPinText(idx, renderProperty);
        });
        for(var noPinTextIdx = settings.pinTexts ? settings.pinTexts.length : 0;noPinTextIdx < VRS.globalOptions.aircraftMarkerMaximumPinTextLines;++noPinTextIdx) {
            that.setPinText(noPinTextIdx, VRS.RenderProperty.None);
        }

        var _PinTextLines = settings.pinTextLines;
        this.getPinTextLines = function() { return _PinTextLines; };
        this.setPinTextLines = function(/** number */ value) {
            if(value !== _PinTextLines) {
                _PinTextLines = value;
                raisePropertyChanged();
            }
        };

        var _HideEmptyPinTextLines = settings.hideEmptyPinTextLines;
        this.getHideEmptyPinTextLines = function() { return _HideEmptyPinTextLines; };
        this.setHideEmptyPinTextLines = function(/** boolean */ value) {
            if(value !== _HideEmptyPinTextLines) {
                _HideEmptyPinTextLines = value;
                raisePropertyChanged();
            }
        };

        /** @type {VRS.TrailDisplay} */
        var _TrailDisplay = settings.trailDisplay;
        this.getTrailDisplay = function() { return _TrailDisplay; };
        /**
         * The VRS.TrailDisplay value describing which aircraft should have trails shown for them.
         * @param {VRS.TrailDisplay} value
         */
        this.setTrailDisplay = function(value) {
            if(value !== _TrailDisplay) {
                _TrailDisplay = value;
                raisePropertyChanged();
            }
        };

        /** @type {VRS.TrailType} */
        var _TrailType = settings.trailType;
        this.getTrailType = function() { return _TrailType; };
        /**
         * The VRS.TrailType value describing the kind of trail to show for the aircraft.
         * @param {VRS.TrailType} value
         */
        this.setTrailType = function(value) {
            if(value !== _TrailType) {
                _TrailType = value;
                // Changes to trail type do not raise property changed as there's nothing we can do about them. We need new trails from the server.
            }
        };

        /** @type {boolean} @private */
        var _ShowRangeCircles = settings.showRangeCircles;
        this.getShowRangeCircles = function() { return _ShowRangeCircles; };
        this.setShowRangeCircles = function(/** boolean */value) {
            if(value !== _ShowRangeCircles) {
                _ShowRangeCircles = value;
                raiseRangeCirclePropertyChanged();
            }
        };

        /** @type {number} @private */
        var _RangeCircleInterval = settings.rangeCircleInterval;
        this.getRangeCircleInterval = function() { return _RangeCircleInterval; };
        this.setRangeCircleInterval = function(/** number */ value) {
            if(value !== _RangeCircleInterval) {
                _RangeCircleInterval = value;
                raiseRangeCirclePropertyChanged();
            }
        };

        /** @type {VRS.Distance} @private */
        var _RangeCircleDistanceUnit = settings.rangeCircleDistanceUnit;
        this.getRangeCircleDistanceUnit = function() { return _RangeCircleDistanceUnit; };
        this.setRangeCircleDistanceUnit = function(/** VRS.Distance */ value) {
            if(value !== _RangeCircleDistanceUnit) {
                _RangeCircleDistanceUnit = value;
                raiseRangeCirclePropertyChanged();
            }
        };

        /** @type {number} @private */
        var _RangeCircleCount = settings.rangeCircleCount;
        this.getRangeCircleCount = function() { return _RangeCircleCount; };
        this.setRangeCircleCount = function(/** number */ value) {
            if(value !== _RangeCircleCount) {
                _RangeCircleCount = value;
                raiseRangeCirclePropertyChanged();
            }
        };

        /** @type {string} @private */
        var _RangeCircleOddColour = settings.rangeCircleOddColour;
        this.getRangeCircleOddColour = function() { return _RangeCircleOddColour; };
        this.setRangeCircleOddColour = function(/** string */value) {
            if(value !== _RangeCircleOddColour) {
                _RangeCircleOddColour = value;
                raiseRangeCirclePropertyChanged();
            }
        };

        /** @type {number} @private */
        var _RangeCircleOddWeight = settings.rangeCircleOddWeight;
        this.getRangeCircleOddWeight = function() { return _RangeCircleOddWeight; };
        this.setRangeCircleOddWeight = function(/** number */ value) {
            if(value !== _RangeCircleOddWeight) {
                _RangeCircleOddWeight = value;
                raiseRangeCirclePropertyChanged();
            }
        };

        /** @type {string} @private */
        var _RangeCircleEvenColour = settings.rangeCircleEvenColour;
        this.getRangeCircleEvenColour = function() { return _RangeCircleEvenColour; };
        this.setRangeCircleEvenColour = function(/** string */value) {
            if(value !== _RangeCircleEvenColour) {
                _RangeCircleEvenColour = value;
                raiseRangeCirclePropertyChanged();
            }
        };

        /** @type {number} @private */
        var _RangeCircleEvenWeight = settings.rangeCircleEvenWeight;
        this.getRangeCircleEvenWeight = function() { return _RangeCircleEvenWeight; };
        this.setRangeCircleEvenWeight = function(/** number */ value) {
            if(value !== _RangeCircleEvenWeight) {
                _RangeCircleEvenWeight = value;
                raiseRangeCirclePropertyChanged();
            }
        };
        //endregion

        //region -- Events exposed
        this.hookPropertyChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.propertyChanged, callback, forceThis); };
        function raisePropertyChanged() { if(!_SuppressEvents) _Dispatcher.raise(_Events.propertyChanged); }

        this.hookRangeCirclePropertyChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.rangeCirclePropertyChanged, callback, forceThis); };
        function raiseRangeCirclePropertyChanged() { if(!_SuppressEvents) _Dispatcher.raise(_Events.rangeCirclePropertyChanged); }

        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state to persistent storage.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Returns the previously saved state or, if none has been saved, the current state.
         * @returns {VRS_STATE_AIRCRAFTLISTPLOTTER}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            var result = $.extend(createSettings(), savedSettings);

            if(result.showAltitudeStalk && !VRS.globalOptions.aircraftMarkerAllowAltitudeStalk) result.showAltitudeStalk = false;
            if(result.showPinText && !VRS.globalOptions.aircraftMarkerAllowPinText)             result.showPinText = false;

            if(result.rangeCircleCount > VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles)     result.rangeCircleCount = VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles;
            if(result.rangeCircleInterval > VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval) result.rangeCircleInterval = VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval;
            if(result.rangeCircleEvenWeight > VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight) result.rangeCircleEvenWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight;
            if(result.rangeCircleOddWeight > VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight)  result.rangeCircleOddWeight = VRS.globalOptions.aircraftMarkerRangeCircleMaxWeight;

            result.pinTexts = VRS.renderPropertyHelper.buildValidRenderPropertiesList(result.pinTexts, [ VRS.RenderSurface.Marker ], VRS.globalOptions.aircraftMarkerMaximumPinTextLines);
            while(result.pinTexts.length < settings.pinTexts.length && result.pinTexts.length < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                result.pinTexts.push(settings.pinTexts[result.pinTexts.length]);
            }
            while(result.pinTexts.length < VRS.globalOptions.aircraftMarkerMaximumPinTextLines) {
                result.pinTexts.push(VRS.RenderProperty.None);
            }

            return result;
        };

        /**
         * Applies the previously saved state to this object.
         * @param {VRS_STATE_AIRCRAFTLISTPLOTTER} settings
         */
        this.applyState = function(settings)
        {
            var suppressEvents = _SuppressEvents;
            _SuppressEvents = true;

            that.setSuppressAltitudeStalkWhenZoomedOut(settings.suppressAltitudeStalkWhenZoomedOut);
            that.setShowAltitudeStalk(settings.showAltitudeStalk);
            that.setShowPinText(settings.showPinText);
            for(var i = 0;i < settings.pinTexts.length;++i) {
                that.setPinText(i, settings.pinTexts[i]);
            }
            that.setPinTextLines(settings.pinTextLines);
            that.setHideEmptyPinTextLines(settings.hideEmptyPinTextLines);
            that.setTrailType(settings.trailType);
            that.setTrailDisplay(settings.trailDisplay);

            that.setShowRangeCircles(settings.showRangeCircles);
            that.setRangeCircleCount(settings.rangeCircleCount);
            that.setRangeCircleInterval(settings.rangeCircleInterval);
            that.setRangeCircleDistanceUnit(settings.rangeCircleDistanceUnit);
            that.setRangeCircleOddColour(settings.rangeCircleOddColour);
            that.setRangeCircleOddWeight(settings.rangeCircleOddWeight);
            that.setRangeCircleEvenColour(settings.rangeCircleEvenColour);
            that.setRangeCircleEvenWeight(settings.rangeCircleEvenWeight);

            _SuppressEvents = suppressEvents;

            raisePropertyChanged();
            raiseRangeCirclePropertyChanged();
        };

        /**
         * Loads and applies the previously saved state.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key to use when saving and loading state.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsAircraftPlotterOptions-' + that.getName();
        }

        /**
         * Returns the object that holds the current state.
         * @returns {VRS_STATE_AIRCRAFTLISTPLOTTER}
         */
        function createSettings()
        {
            return {
                showAltitudeStalk:                  that.getShowAltitudeStalk(),
                suppressAltitudeStalkWhenZoomedOut: that.getSuppressAltitudeStalkWhenZoomedOut(),
                showPinText:                        that.getShowPinText(),
                pinTexts:                           that.getPinTexts(),
                pinTextLines:                       that.getPinTextLines(),
                hideEmptyPinTextLines:              that.getHideEmptyPinTextLines(),
                trailDisplay:                       that.getTrailDisplay(),
                trailType:                          that.getTrailType(),
                showRangeCircles:                   that.getShowRangeCircles(),
                rangeCircleInterval:                that.getRangeCircleInterval(),
                rangeCircleDistanceUnit:            that.getRangeCircleDistanceUnit(),
                rangeCircleCount:                   that.getRangeCircleCount(),
                rangeCircleOddColour:               that.getRangeCircleOddColour(),
                rangeCircleOddWeight:               that.getRangeCircleOddWeight(),
                rangeCircleEvenColour:              that.getRangeCircleEvenColour(),
                rangeCircleEvenWeight:              that.getRangeCircleEvenWeight()
            };
        }
        //endregion

        //region -- createOptionPane, createOptionPaneForRangeCircles
        /**
         * Returns the configuration UI option panes for the object (except for range circles).
         * @param {number} displayOrder
         * @returns {VRS.OptionPane[]}
         */
        this.createOptionPane = function(displayOrder)
        {
            var result = [];

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
                    getValue:       that.getShowAltitudeStalk,
                    setValue:       that.setShowAltitudeStalk,
                    saveState:      that.saveState
                }));
                displayPane.addField(new VRS.OptionFieldCheckBox({
                    name:           'suppressAltitudeStalkWhenZoomedOut',
                    labelKey:       'SuppressAltitudeStalkWhenZoomedOut',
                    getValue:       that.getSuppressAltitudeStalkWhenZoomedOut,
                    setValue:       that.setSuppressAltitudeStalkWhenZoomedOut,
                    saveState:      that.saveState
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
                    getValue:           that.getPinTextLines,
                    setValue:           that.setPinTextLines,
                    min:                0,
                    max:                pinTextLines,
                    inputWidth:         VRS.InputWidth.OneChar,
                    saveState:          function() {
                        that.saveState();
                        for(var contentIdx = 0;contentIdx < pinTextLines;++contentIdx) {
                            var contentField = displayPane.getFieldByName(buildContentFieldName(contentIdx));
                            if(contentField) contentField.raiseRefreshFieldVisibility();
                        }
                    }
                }));

                var addPinTextContentField = function(idx) {
                    displayPane.addField(new VRS.OptionFieldComboBox({
                        name:           buildContentFieldName(idx),
                        labelKey:       function() { return VRS.stringUtility.format(VRS.$$.PinTextNumber, idx + 1); },
                        getValue:       function() { return that.getPinText(idx); },
                        setValue:       function(value) { that.setPinText(idx, value); },
                        saveState:      that.saveState,
                        visible:        function() { return idx < that.getPinTextLines(); },
                        values:         values
                    }));
                };
                for(var lineIdx = 0;lineIdx < pinTextLines;++lineIdx) {
                    addPinTextContentField(lineIdx);
                }

                displayPane.addField(new VRS.OptionFieldCheckBox({
                    name:               'hideEmptyPinTextLines',
                    labelKey:           'HideEmptyPinTextLines',
                    getValue:           that.getHideEmptyPinTextLines,
                    setValue:           that.setHideEmptyPinTextLines,
                    saveState:          that.saveState
                }));
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
                            getValue:       that.getTrailDisplay,
                            setValue:       that.setTrailDisplay,
                            saveState:      that.saveState,
                            values: [
                                new VRS.ValueText({ value: VRS.TrailDisplay.None,         textKey: 'DoNotShow' }),
                                new VRS.ValueText({ value: VRS.TrailDisplay.SelectedOnly, textKey: 'ShowForSelectedOnly' }),
                                new VRS.ValueText({ value: VRS.TrailDisplay.AllAircraft,  textKey: 'ShowForAllAircraft' })
                            ]
                        }),
                        new VRS.OptionFieldRadioButton({
                            name:           'trailType',
                            getValue:       function() {
                                switch(that.getTrailType()) {
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.ShortAltitude:
                                        return VRS.TrailType.FullAltitude;
                                    case VRS.TrailType.FullSpeed:
                                    case VRS.TrailType.ShortSpeed:
                                        return VRS.TrailType.FullSpeed;
                                }
                                return VRS.TrailType.Full;
                            },
                            setValue:       function(value) {
                                switch(that.getTrailType()) {
                                    case VRS.TrailType.Full:
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.FullSpeed:
                                        that.setTrailType(value);
                                        break;
                                    default:
                                        switch(value) {
                                            case VRS.TrailType.Full:            that.setTrailType(VRS.TrailType.Short); break;
                                            case VRS.TrailType.FullAltitude:    that.setTrailType(VRS.TrailType.ShortAltitude); break;
                                            case VRS.TrailType.FullSpeed:       that.setTrailType(VRS.TrailType.ShortSpeed); break;
                                        }
                                }
                            },
                            saveState:      that.saveState,
                            values: [
                                new VRS.ValueText({ value: VRS.TrailType.Full,            textKey: 'JustPositions' }),
                                new VRS.ValueText({ value: VRS.TrailType.FullAltitude,    textKey: 'PositionAndAltitude' }),
                                new VRS.ValueText({ value: VRS.TrailType.FullSpeed,       textKey: 'PositionAndSpeed' })
                            ]
                        }),
                        new VRS.OptionFieldCheckBox({
                            name:           'showShortTrails',
                            labelKey:       'ShowShortTrails',
                            getValue:       function() {
                                switch(that.getTrailType()) {
                                    case VRS.TrailType.Full:
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.FullSpeed:
                                        return false;
                                    default:
                                        return true;
                                }
                            },
                            setValue:       function(value) {
                                switch(that.getTrailType()) {
                                    case VRS.TrailType.Full:
                                    case VRS.TrailType.Short:
                                        that.setTrailType(value ? VRS.TrailType.Short : VRS.TrailType.Full);
                                        break;
                                    case VRS.TrailType.FullAltitude:
                                    case VRS.TrailType.ShortAltitude:
                                        that.setTrailType(value ? VRS.TrailType.ShortAltitude : VRS.TrailType.FullAltitude);
                                        break;
                                    case VRS.TrailType.FullSpeed:
                                    case VRS.TrailType.ShortSpeed:
                                        that.setTrailType(value ? VRS.TrailType.ShortSpeed : VRS.TrailType.FullSpeed);
                                        break;
                                }
                            },
                            saveState:      that.saveState
                        })
                    ]
                });
                result.push(trailsPane);
            }

            return result;
        };

        /**
         * Returns the configuration UI option pane for range circle settings.
         * @param {number} displayOrder
         * @returns VRS.OptionPane
         */
        this.createOptionPaneForRangeCircles = function(displayOrder)
        {
            return new VRS.OptionPane({
                name:           'rangeCircle',
                titleKey:       'PaneRangeCircles',
                displayOrder:   displayOrder,
                fields: [
                    new VRS.OptionFieldCheckBox({
                        name:           'show',
                        labelKey:       'ShowRangeCircles',
                        getValue:       that.getShowRangeCircles,
                        setValue:       that.setShowRangeCircles,
                        saveState:      that.saveState
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'count',
                        labelKey:       'Quantity',
                        getValue:       that.getRangeCircleCount,
                        setValue:       that.setRangeCircleCount,
                        saveState:      that.saveState,
                        inputWidth:     VRS.InputWidth.ThreeChar,
                        min:            1,
                        max:            VRS.globalOptions.aircraftMarkerRangeCircleMaxCircles
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'interval',
                        labelKey:       'Distance',
                        getValue:       that.getRangeCircleInterval,
                        setValue:       that.setRangeCircleInterval,
                        saveState:      that.saveState,
                        inputWidth:     VRS.InputWidth.ThreeChar,
                        min:            5,
                        max:            VRS.globalOptions.aircraftMarkerRangeCircleMaxInterval,
                        step:           5,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldComboBox({
                        name:           'distanceUnit',
                        getValue:       that.getRangeCircleDistanceUnit,
                        setValue:       that.setRangeCircleDistanceUnit,
                        saveState:      that.saveState,
                        values:         VRS.UnitDisplayPreferences.getDistanceUnitValues()
                    }),

                    new VRS.OptionFieldColour({
                        name:           'oddColour',
                        labelKey:       'RangeCircleOddColour',
                        getValue:       that.getRangeCircleOddColour,
                        setValue:       that.setRangeCircleOddColour,
                        saveState:      that.saveState,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'oddWidth',
                        getValue:       that.getRangeCircleOddWeight,
                        setValue:       that.setRangeCircleOddWeight,
                        saveState:      that.saveState,
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
                        getValue:       that.getRangeCircleEvenColour,
                        setValue:       that.setRangeCircleEvenColour,
                        saveState:      that.saveState,
                        keepWithNext:   true
                    }),

                    new VRS.OptionFieldNumeric({
                        name:           'evenWidth',
                        getValue:       that.getRangeCircleEvenWeight,
                        setValue:       that.setRangeCircleEvenWeight,
                        saveState:      that.saveState,
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
        };
        //endregion
    };
    //endregion

    //region AircraftPlotter
    /**
     * The object that can plot aircraft onto a map.
     * @param {Object}                      settings                                    The object carrying the settings for the plotter.
     * @param {VRS.AircraftPlotterOptions}  settings.plotterOptions                     The mandatory object that handles the plotter options for us.
     * @param {VRS.AircraftList}           [settings.aircraftList]                      The aircraft list whose aircraft are to be plotted onto the map. If passed then aircraft are automatically plotted when the list is updated.
     * @param {jQuery}                      settings.map                                The jQuery element to which the map VRS jQuery UI plugin has been applied
     * @param {VRS.UnitDisplayPreferences} [settings.unitDisplayPreferences]            The object describing the unit display preferences of the user.
     * @param {string=}                     settings.name                               The name to use when saving the object's state.
     * @param {function():VRS.AircraftCollection} [settings.getAircraft]                A function that returns the collection of aircraft to plot. Defaults to the current aircraft on the aircraft list or an empty collection if no list is supplied.
     * @param {function():VRS.Aircraft}    [settings.getSelectedAircraft]               A function that returns the selected aircraft. Defaults to the selected aircraft from the aircraft list or null if no list is supplied.
     * @param {VRS.AircraftMarker[]}       [settings.aircraftMarkers]                   An array of objects that describe all of the different types of aircraft marker.
     * @param {Number}                     [settings.pinTextMarkerWidth]                The width in pixels for markers that are showing pin text. Has no effect if labels are used to display pin text.
     * @param {Number}                     [settings.pinTextLineHeight]                 The pixels added to the height of the marker for each line of pin text. Has no effect if labels are used to display pin text. The actual number of pixels added may be slightly larger to prevent jaggies in the rendered text.
     * @param {boolean}                    [settings.allowRotation]                     True if rotation of the normal and selected aircraft images is enabled, false if it is not.
     * @param {Number}                     [settings.rotationGranularity]               The smallest number of degrees of rotation that an aircraft has to turn through before its marker is refreshed to display it.
     * @param {Number}                     [settings.suppressAltitudeStalkAboveZoom]    The map zoom level at which altitude stalks are suppressed.
     * @param {string}                     [settings.normalTrailColour]                 The CSS colour of the trail when the aircraft is not selected.
     * @param {string}                     [settings.selectedTrailColour]               The CSS colour of the trail when the aircraft is selected.
     * @param {Number}                     [settings.normalTrailWidth]                  The pixel width of trails for aircraft that are not selected.
     * @param {Number}                     [settings.selectedTrailWidth]                The pixel width of trails for aircraft that are selected.
     * @param {boolean}                    [settings.showTooltips]                      True if tooltips are to be shown on markers, false if they are not.
     * @param {boolean}                    [settings.suppressTextOnImages]              True if text is never to be drawn onto markers by the server, false if it is. Setting this to true draws pin text in labels instead of having the server add it to the marker image. Ignored if the server is running Mono (in which case it's always set to true).
     * @param {function(VRS.Aircraft):Array.<string>} [settings.getCustomPinTexts]      An optional array that returns a custom array of pin text strings for an aircraft rather than using the plotter options to derive pin texts.
     * @param {boolean}                    [settings.allowRangeCircles]                 True if range circles are allowed, false if they must always be suppressed.
     * @constructor
     */
    VRS.AircraftPlotter = function(settings)
    {
        //region -- Default settings
        var _Settings = $.extend({
            aircraftMarkers:                VRS.globalOptions.aircraftMarkers,
            pinTextMarkerWidth:             VRS.globalOptions.aircraftMarkerPinTextWidth,
            pinTextLineHeight:              VRS.globalOptions.aircraftMarkerPinTextLineHeight,
            allowRotation:                  VRS.globalOptions.aircraftMarkerRotate,
            rotationGranularity:            VRS.globalOptions.aircraftMarkerRotationGranularity,
            suppressAltitudeStalkAboveZoom: VRS.globalOptions.aircraftMarkerSuppressAltitudeStalkZoomLevel,
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
        //endregion

        //region -- Fields
        var that = this;
        var _Map = VRS.jQueryUIHelper.getMapPlugin(_Settings.map);
        var _UnitDisplayPreferences = _Settings.unitDisplayPreferences || new VRS.UnitDisplayPreferences();
        var _SuppressTextOnImages = _Settings.suppressTextOnImages;
        var _Suspended = false;

        configureSuppressTextOnImages();

        /**
         * An associative array of all of the currently plotted aircraft indexed by aircraft ID.
         * @type {Object.<number, VRS.PlottedDetail>}
         * @private
         */
        var _PlottedDetail = {};

        /**
         * A VRS.TrailType string describing the type of trails drawn in the last refresh of the display.
         * @type {VRS.TrailType=}
         * @private
         */
        var _PreviousTrailTypeRequested = undefined;

        /**
         * A method that returns the selected aircraft.
         * @type {function(): VRS.Aircraft}
         * @private
         */
        var _GetSelectedAircraft = _Settings.getSelectedAircraft || function() {
            return _Settings.aircraftList ? _Settings.aircraftList.getSelectedAircraft() : null;
        };

        /**
         * A method that returns the list of aircraft to plot.
         * @type {function(): VRS.AircraftCollection}
         * @private
         */
        var _GetAircraft = _Settings.getAircraft || function() {
            return _Settings.aircraftList ? _Settings.aircraftList.getAircraft() : new VRS.AircraftCollection();
        };

        /**
         * The point on which all of the range circles are centered.
         * @type {VRS_LAT_LNG}
         * @private
         */
        var _RangeCircleCentre = null;

        /**
         * An array of the map circles that represent the range circles on the map.
         * @type {Array.<VRS.MapCircle>}
         * @private
         */
        var _RangeCircleCircles = [];
        //endregion

        //region -- Properties
        var _Name = _Settings.name || 'default';
        this.getName = function() { return _Name; };

        /**
         * Gets the map that the plotter is drawing onto.
         * @returns {VRS.vrsMap}
         */
        this.getMap = function() { return _Map; };

        /** @type {boolean} */
        var _MovingMap = VRS.globalOptions.aircraftMarkerMovingMapOn;
        this.getMovingMap = function() { return _MovingMap; };
        this.setMovingMap = function(/**bool*/value) {
            if(value !== _MovingMap) {
                _MovingMap = value;
                if(value) moveMapToSelectedAircraft();
            }
        };
        //endregion

        //region -- Events subscribed
        var _PlotterOptionsPropertyChangedHook = _Settings.plotterOptions.hookPropertyChanged(optionsPropertyChanged, this);
        var _PlotterOptionsRangeCirclePropertyChangedHook = _Settings.plotterOptions.hookRangeCirclePropertyChanged(optionsRangePropertyChanged, this);
        var _AircraftListUpdatedHook = _Settings.aircraftList ? _Settings.aircraftList.hookUpdated(refreshMarkersOnListUpdate, this) : null;
        var _AircraftListFetchingListHook = _Settings.aircraftList ? _Settings.aircraftList.hookFetchingList(fetchingList, this) : null;
        var _SelectedAircraftChangedHook = _Settings.aircraftList ? _Settings.aircraftList.hookSelectedAircraftChanged(refreshSelectedAircraft, this) : null;
        var _FlightLevelHeightUnitChangedHook = _UnitDisplayPreferences.hookFlightLevelHeightUnitChanged(function() { refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel); }, this);
        var _FlightLevelTransitionAltitudeChangedHook = _UnitDisplayPreferences.hookFlightLevelTransitionAltitudeChanged(function() { refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel); }, this);
        var _FlightLevelTransitionHeightUnitChangedHook = _UnitDisplayPreferences.hookFlightLevelTransitionHeightUnitChanged(function() { refreshMarkersIfUsingPinText(VRS.RenderProperty.FlightLevel); }, this);
        var _HeightUnitChangedHook = _UnitDisplayPreferences.hookHeightUnitChanged(function() { refreshMarkersIfUsingPinText(VRS.RenderProperty.Altitude); }, this);
        var _SpeedUnitChangedHook = _UnitDisplayPreferences.hookSpeedUnitChanged(function() { refreshMarkersIfUsingPinText(VRS.RenderProperty.Speed); }, this);
        var _ShowVsiInSecondsHook = _UnitDisplayPreferences.hookShowVerticalSpeedPerSecondChanged(function() { refreshMarkersIfUsingPinText(VRS.RenderProperty.VerticalSpeed); }, this);
        var _MapIdleHook = _Map.hookIdle(function() { refreshMarkers(null, null); });
        var _MapMarkerClickedHook = _Map.hookMarkerClicked(function(event, data) { selectAircraftById(data.id); });
        var _CurrentLocationChangedHook = VRS.currentLocation ? VRS.currentLocation.hookCurrentLocationChanged(currentLocationChanged, this) : null;
        var _ConfigurationChangedHook = VRS.globalDispatch.hook(VRS.globalEvent.serverConfigChanged, configurationChanged, this);
        //endregion

        //region -- dispose
        /**
         * Releases all resources allocated or hooked by the object.
         */
        this.dispose = function()
        {
            destroyRangeCircles();

            if(_PlotterOptionsPropertyChangedHook)              _Settings.plotterOptions.unhook(_PlotterOptionsPropertyChangedHook);
            if(_PlotterOptionsRangeCirclePropertyChangedHook)   _Settings.plotterOptions.unhook(_PlotterOptionsRangeCirclePropertyChangedHook);
            if(_AircraftListUpdatedHook)                        _Settings.aircraftList.unhook(_AircraftListUpdatedHook);
            if(_AircraftListFetchingListHook)                   _Settings.aircraftList.unhook(_AircraftListFetchingListHook);
            if(_SelectedAircraftChangedHook)                    _Settings.aircraftList.unhook(_SelectedAircraftChangedHook);
            if(_FlightLevelHeightUnitChangedHook)               _UnitDisplayPreferences.unhook(_FlightLevelHeightUnitChangedHook);
            if(_FlightLevelTransitionAltitudeChangedHook)       _UnitDisplayPreferences.unhook(_FlightLevelTransitionAltitudeChangedHook);
            if(_FlightLevelTransitionHeightUnitChangedHook)     _UnitDisplayPreferences.unhook(_FlightLevelTransitionHeightUnitChangedHook);
            if(_HeightUnitChangedHook)                          _UnitDisplayPreferences.unhook(_HeightUnitChangedHook);
            if(_SpeedUnitChangedHook)                           _UnitDisplayPreferences.unhook(_SpeedUnitChangedHook);
            if(_ShowVsiInSecondsHook)                           _UnitDisplayPreferences.unhook(_ShowVsiInSecondsHook);
            if(_MapIdleHook)                                    _Map.unhook(_MapIdleHook);
            if(_MapMarkerClickedHook)                           _Map.unhook(_MapMarkerClickedHook);
            if(_CurrentLocationChangedHook)                     VRS.currentLocation.unhook(_CurrentLocationChangedHook);
            if(_ConfigurationChangedHook)                       VRS.globalDispatch.unhook(_ConfigurationChangedHook);
        };
        //endregion

        //region -- configureSuppressTextOnImages
        /***
         * Configures the _SuppressTextOnImages field from global options and the server configuration.
         */
        function configureSuppressTextOnImages()
        {
            var originalValue = _SuppressTextOnImages;

            _SuppressTextOnImages = _Settings.suppressTextOnImages;
            if(VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    if(_SuppressTextOnImages === undefined) {
                        _SuppressTextOnImages = config.UseMarkerLabels;
                    }
                }
            }
            if(_SuppressTextOnImages === undefined) _SuppressTextOnImages = false;

            return originalValue != _SuppressTextOnImages;
        }
        //endregion

        //region -- suspend
        /**
         * Suspends or resumes the plotter. While the plotter is suspended no updates are made to the map. Once the plotter
         * is resumed the map is refreshed and future updates are plotted.
         * @param {boolean} onOff
         */
        this.suspend = function(onOff)
        {
            onOff = !!onOff;

            if(_Suspended !== onOff) {
                _Suspended = onOff;
                if(!_Suspended) {
                    refreshMarkers(null, null, true);
                    that.refreshRangeCircles(true);
                }
            }
        };
        //endregion

        //region -- plot, refreshMarkers, refreshAircraftMarker, removeOldMarkers etc.
        /**
         * Refreshes the markers for the aircraft.
         * @param {boolean} [refreshAllMarkers]     True if the markers are to be redrawn even if they might not need it.
         * @param {boolean} [ignoreBounds]          True if all markers are to be plotted even if they're not within bounds.
         */
        this.plot = function(refreshAllMarkers, ignoreBounds)
        {
            refreshMarkers(null, null, !!refreshAllMarkers, !!ignoreBounds);
        };

        /**
         * Returns an array of plotted aircraft identifiers.
         **/
        this.getPlottedAircraftIds = function()
        {
            var result = [];
            for(var aircraftId in _PlottedDetail) {
                //noinspection JSUnfilteredForInLoop
                result.push(aircraftId);
            }

            return result;
        };

        //noinspection JSUnusedLocalSymbols
        /**
         * Refreshes the markers for all of the aircraft in the aircraft list. Note that this is an internal method, it
         * is not the handler for the aircraft list updated event.
         * @param {VRS.AircraftCollection} newAircraft
         * @param {VRS.AircraftCollection} oldAircraft
         * @param {bool=}                  alwaysRefreshIcon
         * @param {bool=}                  ignoreBounds
         */
        function refreshMarkers(newAircraft, oldAircraft, alwaysRefreshIcon, ignoreBounds)
        {
            /** @type {VRS.AircraftCollection} */
            var unusedAircraft = null;
            if(oldAircraft) removeOldMarkers(oldAircraft);
            else {
                unusedAircraft = new VRS.AircraftCollection();
                for(var aircraftId in _PlottedDetail) {
                    //noinspection JSUnfilteredForInLoop
                    unusedAircraft[aircraftId] = _PlottedDetail[aircraftId].aircraft;
                }
            }

            var bounds = _Map.getBounds();
            if(bounds || ignoreBounds) {
                var mapZoomLevel = _Map.getZoom();
                var selectedAircraft = _GetSelectedAircraft();
                _GetAircraft().foreachAircraft(function(aircraft) {
                    refreshAircraftMarker(aircraft, alwaysRefreshIcon, ignoreBounds, bounds, mapZoomLevel, selectedAircraft && selectedAircraft === aircraft);
                    if(unusedAircraft && unusedAircraft[aircraft.id]) unusedAircraft[aircraft.id] = undefined;
                });
                moveMapToSelectedAircraft(selectedAircraft);
            }

            if(unusedAircraft) removeOldMarkers(unusedAircraft);
        }

        /**
         * Refreshes an aircraft's marker on the map.
         * @param {VRS.Aircraft}                                                aircraft
         * @param {bool}                                                        forceRefresh
         * @param {bool}                                                        ignoreBounds
         * @param {{tlLat:number, tlLng:number, brLat:number, brLng:number}}    bounds
         * @param {number}                                                      mapZoomLevel
         * @param {bool}                                                        isSelectedAircraft
         */
        function refreshAircraftMarker(aircraft, forceRefresh, ignoreBounds, bounds, mapZoomLevel, isSelectedAircraft)
        {
            if((ignoreBounds || bounds) && aircraft.hasPosition()) {
                var position = aircraft.getPosition();
                var isInBounds = ignoreBounds || aircraft.positionWithinBounds(bounds);
                var plotAircraft = isInBounds || (isSelectedAircraft && VRS.globalOptions.aircraftMarkerAlwaysPlotSelected);
                if(plotAircraft && !aircraft.isAircraftSpecies() && mapZoomLevel < _Settings.hideNonAircraftZoomLevel) plotAircraft = false;

                var details = _PlottedDetail[aircraft.id];
                if(details && details.aircraft !== aircraft) {
                    // This can happen if the aircraft goes off the radar and then returns - it will have a different object
                    removeDetails(details);
                    details = undefined;
                }

                if(details) {
                    if(!plotAircraft) removeDetails(details);
                    else {
                        var marker = details.mapMarker;
                        marker.setPosition(position);
                        if(forceRefresh || haveIconDetailsChanged(details, mapZoomLevel)) {
                            var icon = createIcon(details, mapZoomLevel, isSelectedAircraft);
                            if(icon) {
                                marker.setIcon(icon);
                                var zIndex = isSelectedAircraft ? 101 : 100;
                                if(zIndex !== marker.getZIndex()) marker.setZIndex(zIndex);
                                if(marker.isMarkerWithLabel) {
                                    if(icon.labelAnchor && (!details.mapIcon || (details.mapIcon.labelAnchor.x !== icon.x || details.mapIcon.labelAnchor.y !== icon.y))) {
                                        marker.setLabelAnchor(icon.labelAnchor);
                                    }
                                }
                                details.mapIcon = icon;
                            }
                        }
                        if(forceRefresh || haveTooltipDetailsChanged(details)) {
                            marker.setTooltip(getTooltip(details));
                        }
                        if(forceRefresh || haveLabelDetailsChanged(details)) {
                            createLabel(details);
                        }
                        updateTrail(details, isSelectedAircraft, forceRefresh);
                    }
                } else if(plotAircraft) {
                    details = new VRS.PlottedDetail(aircraft);
                    details.mapIcon = createIcon(details, mapZoomLevel, isSelectedAircraft);
                    var markerOptions = {
                        clickable: true,
                        draggable: false,
                        flat: true,
                        icon: details.mapIcon,
                        visible: true,
                        position: position,
                        tooltip: getTooltip(details),
                        zIndex: isSelectedAircraft ? 101 : 100
                    };
                    if(_SuppressTextOnImages) {
                        markerOptions.useMarkerWithLabel = true;
                        markerOptions.mwlLabelInBackground = true;
                        markerOptions.mwlLabelClass = 'markerLabel';
                    }
                    details.mapMarker = _Map.addMarker(aircraft.id, markerOptions);
                    createLabel(details);
                    updateTrail(details, isSelectedAircraft, forceRefresh);
                    _PlottedDetail[aircraft.id] = details;
                }
            }
        }

        /**
         * Removes all of the markers for the aircraft collection passed across.
         * @param {VRS.AircraftCollection} oldAircraft
         */
        function removeOldMarkers(oldAircraft)
        {
            oldAircraft.foreachAircraft(function(aircraft) {
                var details = _PlottedDetail[aircraft.id];
                if(details) removeDetails(details);
            });
        }

        /**
         * Removes every marker from the map.
         */
        function removeAllMarkers()
        {
            var allPlottedAircraftIds = that.getPlottedAircraftIds();
            var length = allPlottedAircraftIds.length;
            for(var i = 0;i < length;++i) {
                var aircraftId = allPlottedAircraftIds[i];
                var details = _PlottedDetail[aircraftId];
                removeDetails(details);
            }
        }

        /**
         * Returns true if the aircraft is being plotted on the map.
         * @param {VRS.Aircraft} aircraft
         * @returns {bool}
         */
        function isAircraftBeingPlotted(aircraft)
        {
            return !!(aircraft && _PlottedDetail[aircraft.id]);
        }

        /**
         * Destroys the PlottedDetail passed across and removes it from the internal _PlottedDetail associative array.
         * @param {VRS.PlottedDetail} details
         */
        function removeDetails(details)
        {
            if(details.mapMarker) _Map.destroyMarker(details.mapMarker);
            removeTrail(details);

            details.mapIcon = null;
            details.aircraft = null;
            details.mapMarker = null;
            details.mapPolylines = [];
            delete _PlottedDetail[details.id];
            details.id = null;
            details.pinTexts = null;
            details.iconUrl = null;
        }
        //endregion

        //region -- haveIconDetailsChanged, createIcon
        /**
         * Returns true if the details for the marker's image have changed.
         * @param {VRS.PlottedDetail} details
         * @param {number} mapZoomLevel
         * @returns {boolean}
         */
        function haveIconDetailsChanged(details, mapZoomLevel)
        {
            var result = false;
            var aircraft = details.aircraft;

            if(!result) {
                if(!allowIconRotation()) {
                    if(details.iconRotation !== undefined) result = true;
                } else {
                    result = details.iconRotation === undefined || details.iconRotation !== getIconHeading(aircraft);
                }
            }

            if(!result) {
                if(!allowIconAltitudeStalk(mapZoomLevel)) {
                    if(details.iconAltitudeStalkHeight !== undefined) result = true;
                } else {
                    result = details.iconAltitudeStalkHeight == undefined || details.iconAltitudeStalkHeight !== getIconAltitudeStalkHeight(aircraft);
                }
            }

            if(!result && !_SuppressTextOnImages && (!details.mapMarker || !details.mapMarker.isMarkerWithLabel)) {
                if(!allowPinTexts()) {
                    if(details.pinTexts.length !== 0) result = true;
                } else {
                    result = havePinTextDependenciesChanged(aircraft);
                }
            }

            return result;
        }

        /**
         * Creates a VRS.MapIcon for the marker for the aircraft detail passed across.
         * @param {VRS.PlottedDetail}   details
         * @param {number}              mapZoomLevel
         * @param {bool}                isSelectedAircraft
         * @returns {VRS.MapIcon}       The new image for the marker passed across.
         */
        function createIcon(details, mapZoomLevel, isSelectedAircraft)
        {
            var aircraft = details.aircraft;
            var marker = getAircraftMarker(aircraft);

            var size = marker.getSize();
            size = { width: size.width, height: size.height };
            var anchorY = Math.floor(size.height / 2);
            var suppressPinText = _SuppressTextOnImages || (details.mapMarker && details.mapMarker.isMarkerWithLabel);

            if(!allowIconRotation() || !marker.getCanRotate()) details.iconRotation = undefined;
            else                                               details.iconRotation = getIconHeading(aircraft);

            var blankPixelsAtBottom = 0;
            if(!suppressPinText) {
                if(!allowPinTexts()) {
                    if(details.pinTexts.length > 0) details.pinTexts = [];
                } else {
                    details.pinTexts = getPinTexts(aircraft);
                    size.height += (_Settings.pinTextLineHeight * details.pinTexts.length);
                    size.width = _Settings.pinTextMarkerWidth;

                    // The text scaling works best if the icon height and width are multiples of 4. We already ensure that
                    // the width is a multiple of 4 so we just need to add pixels to adjust the height.
                    blankPixelsAtBottom = size.height % 4;
                    size.height += blankPixelsAtBottom;
                }
            }
            var pinTextLines = suppressPinText ? 0 : details.pinTexts.length;

            var hasAltitudeStalk = false;
            if(!marker.getIsAircraft() || !allowIconAltitudeStalk(mapZoomLevel)) details.iconAltitudeStalkHeight = undefined;
            else {
                hasAltitudeStalk = true;
                details.iconAltitudeStalkHeight = getIconAltitudeStalkHeight(aircraft);
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
            if(VRS.browserHelper.isHighDpi()) {
                multiplier = 2;
                requestSize = { width: size.width * multiplier, height: size.height * multiplier };
            }

            var url = 'images/top/web-markers';
            url += '/Wdth-' + requestSize.width;
            url += '/Hght-' + requestSize.height;
            if(VRS.browserHelper.isHighDpi()) url += '/hiDpi';
            if(details.iconRotation || details.iconRotation === 0) url += '/Rotate-' + details.iconRotation;
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

            return !urlChanged ? null : new VRS.MapIcon(url, size, { x: centreX, y: anchorY }, { x: 0, y: 0 }, size, labelAnchor);
        }

        /**
         * Returns the aircraft marker to use for the aircraft passed across.
         * @param {VRS.Aircraft} aircraft
         * @returns {VRS.AircraftMarker|null}
         */
        function getAircraftMarker(aircraft)
        {
            var result = null;

            if(aircraft) {
                var markers = _Settings.aircraftMarkers;
                var length = markers.length;
                for(var i = 0;i < length;++i) {
                    result = markers[i];
                    if(result.matchesAircraft(aircraft)) break;
                }
            }

            return result;
        }

        /**
         * Returns true if the marker images are allowed to rotate.
         * @returns {boolean}
         */
        function allowIconRotation()
        {
            return _Settings.allowRotation;
        }

        /**
         * Gets the degree of rotation for the image for an aircraft's marker.
         * @param {VRS.Aircraft} aircraft
         * @returns {number}
         */
        function getIconHeading(aircraft)
        {
            var rotationGranularity = _Settings.rotationGranularity;
            var heading = aircraft.heading.val;
            if(isNaN(heading)) heading = 0;
            else               heading = Math.round(heading / rotationGranularity) * rotationGranularity;

            return heading;
        }

        /**
         * Returns true if altitude stalks can be shown at the map zoom level passed across.
         * @param {number} mapZoomLevel
         * @returns {boolean}
         */
        function allowIconAltitudeStalk(mapZoomLevel)
        {
            return VRS.globalOptions.aircraftMarkerAllowAltitudeStalk &&
                   _Settings.plotterOptions.getShowAltitudeStalk() &&
                   (!_Settings.plotterOptions.getSuppressAltitudeStalkWhenZoomedOut() || mapZoomLevel >= _Settings.suppressAltitudeStalkAboveZoom);
        }

        /**
         * Gets the height in pixels for the aircraft passed across.
         * @param {VRS.Aircraft} aircraft
         * @returns {number}
         */
        function getIconAltitudeStalkHeight(aircraft)
        {
            var result = aircraft.altitude.val;
            if(isNaN(result)) result = 0;
            else {
                result = Math.max(0, Math.min(result, 35000));
                result = Math.round(result / 2500) * 5;
            }

            return result;
        }

        /**
         * Returns true if the pin texts can be shown.
         * @returns {boolean}
         */
        function allowPinTexts()
        {
            var result = _Settings.plotterOptions.getShowPinText();
            if(result && VRS.serverConfig) result = VRS.serverConfig.pinTextEnabled();

            return result;
        }

        /**
         * Returns true if any pin text may have changed for an aircraft.
         * @param {VRS.Aircraft} aircraft
         * @returns {boolean}
         */
        function havePinTextDependenciesChanged(aircraft)
        {
            var result = false;

            var pinTexts = _Settings.plotterOptions.getPinTexts();
            var length = pinTexts.length;
            for(var i = 0;i < length;++i) {
                var handler = VRS.renderPropertyHandlers[pinTexts[i]];
                if(handler && handler.hasChangedCallback(aircraft)) result = true;
                if(result) break;
            }

            return result;
        }

        /**
         * Gets an array of pin text values (i.e. the text from the aircraft, not VRS.RenderProperty enums) for an aircraft.
         * @param {VRS.Aircraft} aircraft
         * @returns {string[]}
         */
        function getPinTexts(aircraft)
        {
            var result = [];
            var suppressBlankLines = _Settings.plotterOptions.getHideEmptyPinTextLines();

            if(_Settings.getCustomPinTexts) {
                result = _Settings.getCustomPinTexts(aircraft) || [];
            } else {
                var options = {
                    unitDisplayPreferences: _UnitDisplayPreferences,
                    distinguishOnGround:    true
                };

                var length = Math.min(_Settings.plotterOptions.getPinTextLines(), VRS.globalOptions.aircraftMarkerMaximumPinTextLines);
                for(var i = 0;i < length;++i) {
                    var renderProperty = _Settings.plotterOptions.getPinText(i);
                    if(renderProperty === VRS.RenderProperty.None) continue;
                    var handler = VRS.renderPropertyHandlers[renderProperty];
                    var text = handler ? handler.contentCallback(aircraft, options, VRS.RenderSurface.Marker) || '' : '';
                    if(!suppressBlankLines || text) result.push(text);
                }
            }

            return result;
        }
        //endregion

        //region -- haveLabelDetailsChanged, createLabel
        /**
         * Returns true if the details for a label on the marker have changed.
         * @param {VRS.PlottedDetail} details
         * @returns {boolean}
         */
        function haveLabelDetailsChanged(details)
        {
            var result = false;

            if(_SuppressTextOnImages || (details.mapMarker && details.mapMarker.isMarkerWithLabel)) {
                if(!allowPinTexts()) {
                    if(details.pinTexts.length !== 0) result = true;
                } else {
                    result = havePinTextDependenciesChanged(details.aircraft);
                }
            }

            return result;
        }

        /**
         * Sets the label content for the marker passed across.
         * @param {VRS.PlottedDetail} details
         */
        function createLabel(details)
        {
            if(_SuppressTextOnImages && details.mapMarker && details.mapMarker.isMarkerWithLabel) {
                if(!allowPinTexts()) {
                    if(details.pinTexts.length > 0) details.pinTexts = [];
                } else {
                    details.pinTexts = getPinTexts(details.aircraft);
                }

                var labelText = '';
                var length = details.pinTexts.length;
                for(var i = 0;i < length;++i) {
                    if(labelText.length) labelText += '<br/>';
                    labelText += '<span>&nbsp;' + VRS.stringUtility.htmlEscape(details.pinTexts[i]) + '&nbsp;</span>'
                }

                var marker = details.mapMarker;
                if(labelText.length === 0 || !details.mapIcon) {
                    marker.setLabelVisible(false);
                } else {
                    if(!marker.getLabelVisible()) marker.setLabelVisible(true);
                    marker.setLabelAnchor(details.mapIcon.labelAnchor);
                    marker.setLabelContent(labelText);
                }
            }
        }
        //endregion

        //region -- updateTrail, getMonochromeTrailColour, getShortTrailPath, createTrail, removeTrail, synchroniseAircraftAndMapPolylinePaths
        /**
         * Updates the aircraft trail for the aircraft passed aross.
         * @param {VRS.PlottedDetail}   details
         * @param {bool}                isAircraftSelected  True if the details represent those of the selected aircraft.
         * @param {bool}                forceRefresh
         */
        function updateTrail(details, isAircraftSelected, forceRefresh)
        {
            if(VRS.globalOptions.suppressTrails) return;

            var aircraft = details.aircraft;
            var showTrails = false;
            switch(_Settings.plotterOptions.getTrailDisplay()) {
                case VRS.TrailDisplay.None:         break;
                case VRS.TrailDisplay.AllAircraft:  showTrails = true; break;
                case VRS.TrailDisplay.SelectedOnly: showTrails = isAircraftSelected; break;
            }
            if(showTrails && !aircraft.isAircraftSpecies() && !VRS.globalOptions.aircraftMarkerShowNonAircraftTrails) showTrails = false;

            if(forceRefresh) removeTrail(details);

            if(!showTrails) {
                if(details.mapPolylines.length) removeTrail(details);
            } else {
                var trailType = _Settings.plotterOptions.getTrailType();
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

                if(details.mapPolylines.length && details.polylineTrailType !== trailType) removeTrail(details);
                if(trail.trimStartCount) trimShortTrailPoints(details, trail);

                var polylines = details.mapPolylines;
                var lastLine = polylines.length ? polylines[polylines.length - 1] : null;

                if(!lastLine) createTrail(details, trail, trailType, isAircraftSelected, isMonochrome);
                else {
                    var width = getTrailWidth(isAircraftSelected);
                    if(lastLine.getStrokeWeight() !== width) {
                        var length = polylines.length;
                        for(var i = 0;i < length;++i) {
                            polylines[i].setStrokeWeight(width);
                        }
                    }

                    if(isMonochrome) {
                        var colour = getMonochromeTrailColour(isAircraftSelected);
                        if(lastLine.getStrokeColour() !== colour) lastLine.setStrokeColour(colour);
                    }

                    if(details.polylinePathUpdateCounter !== aircraft.updateCounter && trail.chg) {
                        synchroniseAircraftAndMapPolylinePaths(details, trailType, trail, isAircraftSelected, isMonochrome, isFullTrail);
                    }
                }

                details.polylineTrailType = trailType;
                details.polylinePathUpdateCounter = aircraft.updateCounter;
            }
        }

        /**
         * Gets the colour to paint the trail in assuming the use of a single colour for the entire trail.
         * @param {boolean}             isAircraftSelected
         * @returns {string}
         */
        function getMonochromeTrailColour(isAircraftSelected)
        {
            return isAircraftSelected ? _Settings.selectedTrailColour : _Settings.normalTrailColour;
        }

        /**
         * Gets the colour for a coordinate in a coloured trail.
         * @param {VRS.FullTrailValue|VRS.ShortTrailValue}  coordinate
         * @param {VRS.TrailType}                           trailType
         * @returns {string}
         */
        function getCoordinateTrailColour(coordinate, trailType)
        {
            var result = null;
            switch(trailType) {
                case VRS.TrailType.FullAltitude:
                case VRS.TrailType.ShortAltitude:
                    result = VRS.colourHelper.colourToCssString(VRS.colourHelper.getColourWheelScale(coordinate.altitude, VRS.globalOptions.aircraftMarkerAltitudeTrailLow, VRS.globalOptions.aircraftMarkerAltitudeTrailHigh, true, true));
                    break;
                case VRS.TrailType.FullSpeed:
                case VRS.TrailType.ShortSpeed:
                    result = VRS.colourHelper.colourToCssString(VRS.colourHelper.getColourWheelScale(coordinate.speed, VRS.globalOptions.aircraftMarkerSpeedTrailLow, VRS.globalOptions.aircraftMarkerSpeedTrailHigh, true, true));
                    break;
                default:
                    throw 'The trail type ' + trailType + ' does not infer multiple colours on a single trail';
            }

            return result;
        }

        /**
         * Gets the width of a trail.
         * @param isAircraftSelected
         * @returns {number}
         */
        function getTrailWidth(isAircraftSelected)
        {
            return isAircraftSelected ? _Settings.selectedTrailWidth : _Settings.normalTrailWidth;
        }

        /**
         * Gets a portion of the polyline path for an aircraft's trail.
         * @param {VRS.ArrayValue}      trail               The full or short trail from an aircraft.
         * @param {number}             [start]              The index to start building the polyline from, if undefined then it starts from 0.
         * @param {number}             [count]              The number of coordinates to return, if undefined then it copies from start to the end of the trail.
         * @param {VRS.Aircraft}       [aircraft]           The aircraft whose trail is being extracted.
         * @param {VRS.TrailType}      [trailType]          The type of trail.
         * @param {boolean}            [isAircraftSelected] True if the aircraft is selected.
         * @param {boolean}            [isMonochrome]       True if the trail is entirely one colour.
         * @returns {{lat:number, lng:number, colour: string}[]}
         */
        function getTrailPath(trail, start, count, aircraft, trailType, isAircraftSelected, isMonochrome)
        {
            var result = [];
            var length = trail.arr.length;
            if(start === undefined) start = 0;
            if(start > length) throw 'Cannot get the trail from index ' + start + ', there are only ' + length + ' coordinates';
            if(count === undefined) count = length - start;
            if(start + count > length) throw 'Cannot get ' + count + ' points from index ' + start + ', there are only ' + length + ' coordinates';

            var colour = aircraft && isMonochrome ? getMonochromeTrailColour(isAircraftSelected) : null;

            var end = start + count;
            for(var i = start;i < end;++i) {
                var coord = trail.arr[i];
                if(aircraft && !isMonochrome) colour = getCoordinateTrailColour(coord, trailType);
                result.push({ lat: coord.lat, lng: coord.lng, colour: colour });
            }

            return result;
        }

        /**
         * Creates a new trail for an aircraft.
         * @param {VRS.PlottedDetail}   details             The details for the aircraft that the trail is being created for.
         * @param {VRS.ArrayValue}      trail               The full or short trail from an aircraft.
         * @param {VRS.TrailType}       trailType           The type of trail to display.
         * @param {bool}                isAircraftSelected  True if the aircraft is selected.
         * @param {bool}                isMonochrome        True if the path is only shown in a single colour.
         */
        function createTrail(details, trail, trailType, isAircraftSelected, isMonochrome)
        {
            if(details.mapPolylines.length) throw 'Cannot create a trail for aircraft ID ' + details.id + ', one already exists';
            var aircraft = details.aircraft;
            var path = getTrailPath(trail, undefined, undefined, aircraft, trailType, isAircraftSelected, isMonochrome);

            if(path.length) {
                var weight = getTrailWidth(isAircraftSelected);
                if(isMonochrome) {
                    details.mapPolylines.push(_Map.addPolyline(aircraft.id, {
                        clickable:      false,
                        draggable:      false,
                        editable:       false,
                        geodesic:       true,
                        strokeColour:   path[0].colour,
                        strokeWeight:   weight,
                        strokeOpacity:  1,
                        path:           path
                    }));
                } else {
                    addMultiColouredPolylines(details, path, weight, null);
                }
            }
        }

        /**
         * Adds multiple polylines to the map to represent a part of the trail for an aircraft.
         * @param {VRS.PlottedDetail}                               details     The details for the aircraft being plotted.
         * @param {Array.<{lat:number, lng:number, colour:string}>} path        The paths (with colours) to add to the trail.
         * @param {number}                                          weight      The stroke weight of the trail.
         * @param {(VRS.ShortTrailValue|VRS.FullTrailValue)=}       fromCoord   An optional coordinate to start the new paths from. Its colour is ignored.
         */
        function addMultiColouredPolylines(details, path, weight, fromCoord)
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

                if(nextSegment && nextSegment.colour === segment.colour) continue;
                if(nextSegment) segments.push(nextSegment);

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
                            if(firstPoint && Math.abs(firstPoint.lat - firstSegment.lat) < 0.0000001 && Math.abs(firstPoint.lng - firstSegment.lng) < 0.0000001) {
                                _Map.destroyPolyline(lastLine);
                                details.mapPolylines.splice(-1, 1);
                            }
                        }
                    }

                    var id = aircraft.id.toString() + '$' + details.nextPolylineId++;
                    details.mapPolylines.push(_Map.addPolyline(id, {
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
         * Removes the trail for the aircraft passed across.
         * @param {VRS.PlottedDetail} details
         */
        function removeTrail(details)
        {
            var length = details.mapPolylines.length;
            if(length) {
                for(var i = 0;i < length;++i) {
                    _Map.destroyPolyline(details.mapPolylines[i]);
                }
                details.mapPolylines = [];
                details.polylinePathUpdateCounter = undefined;
                details.polylineTrailType = undefined;
            }
        }

        /**
         * Updates an existing map polyline to reflect changes to an aircraft' trail.
         * @param {VRS.PlottedDetail}   details             The aircraft.
         * @param {string}              trailType           The VRS.TrailType type of trail to show.
         * @param {VRS.ArrayValue}      trail               The full or short trail to apply.
         * @param {boolean}             isAircraftSelected  True if the aircraft is selected.
         * @param {boolean}             isMonochrome        True if the trail is a single colour, false if it is not.
         * @param {boolean}             isFullTrail         True if the trail is the full trail and trailType indicates that it is a variation on the full trail theme.
         */
        function synchroniseAircraftAndMapPolylinePaths(details, trailType, trail, isAircraftSelected, isMonochrome, isFullTrail)
        {
            var polylines = details.mapPolylines;
            var polylinesLength = polylines.length;
            var trailLength = trail.arr.length;

            if(isFullTrail && trail.chg && trail.chgIdx === -1 && trailLength > 0 && trail.arr[trailLength - 1].chg) {
                // The last coordinate of the polyline has been moved - we need to update the last point on the trail
                var changedTrail = trail.arr[trailLength - 1];
                _Map.replacePolylinePointAt(polylines[polylinesLength - 1], -1, { lat: changedTrail.lat, lng: changedTrail.lng });
            } else {
                if(trail.chgIdx !== -1 && trail.chgIdx < trail.arr.length) {
                    var path = getTrailPath(trail, trail.chgIdx, undefined, details.aircraft, trailType, isAircraftSelected, isMonochrome);
                    if(isMonochrome) _Map.appendToPolyline(polylines[polylinesLength - 1], path, false);
                    else {
                        var weight = getTrailWidth(isAircraftSelected);
                        var fromCoord = trail.chgIdx > 0 ? trail.arr[trail.chgIdx - 1] : null;
                        addMultiColouredPolylines(details, path, weight, fromCoord);
                    }
                }
            }
        }

        /**
         * Removes points from the oldest part of the trail.
         * @param {VRS.PlottedDetail}   details
         * @param {VRS.ArrayValue}      trail
         */
        function trimShortTrailPoints(details, trail)
        {
            var countRemove = trail.trimStartCount;
            var polylines = details.mapPolylines;
            var countLines = polylines.length;
            while(countRemove > 0 && countLines) {
                var oldestLine = polylines[0];
                var removeState = _Map.trimPolyline(oldestLine, countRemove, true);
                countRemove -= removeState.countRemoved;
                if(removeState.emptied || !removeState.countRemoved) {     // Remove the line if it was emptied or if nothing was removed and nothing emptied (i.e. it was an empty line)
                    polylines.splice(0, 1);
                    --countLines;
                    _Map.destroyPolyline(oldestLine);
                }
            }
        }
        //endregion

        //region -- haveTooltipDetailsChanged, getTooltip
        /**
         * Returns true if the tooltip details have changed for the aircraft passed across.
         * @param {VRS.PlottedDetail} details
         * @returns {boolean}
         */
        function haveTooltipDetailsChanged(details)
        {
            var result = false;
            if(_Settings.showTooltips) {
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
         * @param {VRS.PlottedDetail} details
         * @returns {string}
         */
        function getTooltip(details)
        {
            var result = '';

            if(_Settings.showTooltips) {
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
        //endregion

        //region -- selectAircraftById, moveMapToSelectedAircraft, moveMapToAircraft
        /**
         * Selects the aircraft with the unique ID passed across. This only works with the aircraft list - if you are
         * using a custom list then you need to supply a getSelectedAircraft method.
         * @param {number} id
         */
        function selectAircraftById(id)
        {
            if(_Settings.aircraftList) {
                var details = _PlottedDetail[id];
                if(details) settings.aircraftList.setSelectedAircraft(details.aircraft, true);
            }
        }

        /**
         * Moves the map centre to the selected aircraft or does nothing if either the option to move the map is disabled
         * or there is no selected aircraft or the selected aircraft does not have a position.
         * @param {VRS.Aircraft} [selectedAircraft] The selected aircraft to centre on.
         */
        function moveMapToSelectedAircraft(selectedAircraft)
        {
            if(_MovingMap) {
                if(!selectedAircraft) selectedAircraft = _GetSelectedAircraft();
                that.moveMapToAircraft(selectedAircraft);
            }
        }

        /**
         * Moves the map to centre on the aircraft passed across. If the aircraft is not supplied, or it has no position,
         * then this does nothing.
         * @param {VRS.Aircraft} aircraft
         */
        this.moveMapToAircraft = function(aircraft)
        {
            if(aircraft && aircraft.hasPosition()) {
                _Map.setCenter(aircraft.getPosition());
            }
        };
        //endregion

        //region -- refreshRangeCircles, destroyRangeCircles
        /**
         * Draws (or removes, or moves) the range circles on the map so that they are centered on the current location.
         * @param {boolean} [forceRefresh]
         */
        this.refreshRangeCircles = function(forceRefresh)
        {
            var i;
            var currentLocation = VRS.currentLocation ? VRS.currentLocation.getCurrentLocation() : null;

            if(!currentLocation || !_Settings.allowRangeCircles || !_Settings.plotterOptions.getShowRangeCircles()) {
                destroyRangeCircles();
            } else {
                if(forceRefresh || !_RangeCircleCentre || _RangeCircleCentre.lat !== currentLocation.lat || _RangeCircleCentre.lng !== currentLocation.lng) {
                    var plotterOptions = _Settings.plotterOptions;

                    var baseCircleId = -1000;
                    var intervalMetres = VRS.unitConverter.convertDistance(plotterOptions.getRangeCircleInterval(), plotterOptions.getRangeCircleDistanceUnit(), VRS.Distance.Kilometre) * 1000;
                    var countCircles = plotterOptions.getRangeCircleCount();
                    for(i = 0;i < countCircles;++i) {
                        var isOdd = i % 2 === 0;

                        var circle = _RangeCircleCircles.length > i ? _RangeCircleCircles[i] : null;
                        var radius = intervalMetres * (i + 1);
                        var colour = isOdd ? plotterOptions.getRangeCircleOddColour() : plotterOptions.getRangeCircleEvenColour();
                        var weight = isOdd ? plotterOptions.getRangeCircleOddWeight() : plotterOptions.getRangeCircleEvenWeight();

                        if(circle) {
                            circle.setCenter(currentLocation);
                            circle.setRadius(radius);
                            circle.setStrokeColor(colour);
                            circle.setStrokeWeight(weight);
                        } else {
                            _RangeCircleCircles.push(_Map.addCircle(baseCircleId - i, {
                                center:         currentLocation,
                                radius:         radius,
                                strokeColor:    colour,
                                strokeWeight:   weight
                            }));
                        }
                    }

                    if(countCircles < _RangeCircleCircles.length) {
                        for(i = countCircles;i < _RangeCircleCircles.length;++i) {
                            _Map.destroyCircle(_RangeCircleCircles[i]);
                        }
                        _RangeCircleCircles.splice(countCircles, _RangeCircleCircles.length - countCircles);
                    }

                    _RangeCircleCentre = currentLocation;
                }
            }
        };

        /**
         * Destroys the range circles and releases any resources allocated to them.
         */
        function destroyRangeCircles()
        {
            if(_RangeCircleCircles.length) {
                $.each(_RangeCircleCircles, function(/** Number */ idx, /** VRS.MapCircle */ circle) {
                    _Map.destroyCircle(circle);
                });
                _RangeCircleCircles = [];
                _RangeCircleCentre = null;
            }
        }
        //endregion

        //region -- getAircraftMarker, getAircraftForMarkerId, diagnosticsGetPlottedDetail
        /**
         * Returns the VRS.MapMarker for the aircraft passed across or null / undefined if no such marker exists.
         * @param {VRS.Aircraft} aircraft
         */
        this.getAircraftMarker = function(aircraft)
        {
            var result = null;
            if(aircraft) {
                var detail = _PlottedDetail[aircraft.id];
                if(detail) result = detail.mapMarker;
            }

            return result;
        };

        /**
         * Returns the VRS.Aircraft for the map marker passed across or null / undefined if the marker either
         * does not belong to the plotter or isn't associated with an aircraft.
         * @param {Number|String} mapMarkerId
         */
        this.getAircraftForMarkerId = function(mapMarkerId)
        {
            var result = null;

            var details = _PlottedDetail[mapMarkerId];
            if(details) result = details.aircraft;

            return result;
        };

        /**
         * A diagnostics method that returns the plotted detail for the aircraft.
         * @param {VRS.Aircraft}        aircraft    The aircraft to return the plotted detail for.
         * @returns {VRS.PlottedDetail}
         */
        this.diagnosticsGetPlottedDetail = function(aircraft)
        {
            return _PlottedDetail[aircraft.id];
        };
        //endregion

        //region -- Events subscribed
        /**
         * Called whenever an aircraft plotter options property is changed.
         */
        function optionsPropertyChanged()
        {
            if(!_Suspended) {
                refreshMarkers(null, null, true);
            }
        }

        /**
         * Called whenever a range circles property changes.
         */
        function optionsRangePropertyChanged()
        {
            if(!_Suspended) {
                that.refreshRangeCircles(true);
            }
        }

        /**
         * Called when the XHR call to refresh the aircraft list is being built.
         * @param {*} xhrParams   The query string sent to the server with the refresh request.
         //* @param {*} xhrHeaders  The HTML headers sent to the server with the refresh request.
         */
        function fetchingList(xhrParams)
        {
            if(!VRS.globalOptions.suppressTrails) {
                var trailType = _Settings.plotterOptions.getTrailType();
                switch(trailType) {
                    case VRS.TrailType.Full:            xhrParams.trFmt = 'f'; break;
                    case VRS.TrailType.Short:           xhrParams.trFmt = 's'; break;
                    case VRS.TrailType.FullAltitude:    xhrParams.trFmt = 'fa'; break;
                    case VRS.TrailType.ShortAltitude:   xhrParams.trFmt = 'sa'; break;
                    case VRS.TrailType.FullSpeed:       xhrParams.trFmt = 'fs'; break;
                    case VRS.TrailType.ShortSpeed:      xhrParams.trFmt = 'ss'; break;
                }

                if(_PreviousTrailTypeRequested && _PreviousTrailTypeRequested !== trailType) xhrParams.refreshTrails = '1';
                _PreviousTrailTypeRequested = trailType;
            }
        }

        /**
         * Called when the aircraft list is updated.
         * @param {VRS.AircraftCollection} newAircraft
         * @param {VRS.AircraftCollection} oldAircraft
         */
        function refreshMarkersOnListUpdate(newAircraft, oldAircraft)
        {
            if(!_Suspended) refreshMarkers(newAircraft, oldAircraft);
        }

        /**
         * Called when the selected aircraft changes.
         * @param {VRS.Aircraft} oldSelectedAircraft
         */
        function refreshSelectedAircraft(oldSelectedAircraft)
        {
            if(!_Suspended) {
                var bounds = _Map.getBounds();
                var mapZoomLevel = _Map.getZoom();

                if(isAircraftBeingPlotted(oldSelectedAircraft)) refreshAircraftMarker(oldSelectedAircraft, true, false, bounds, mapZoomLevel, false);
                var selectedAircraft = _GetSelectedAircraft();
                if(isAircraftBeingPlotted(selectedAircraft)) refreshAircraftMarker(selectedAircraft, true, false, bounds, mapZoomLevel, true);
            }
        }

        /**
         * Called when a display unit has changed. Refreshes the aircraft markers but only if the VRS.RenderProperty passed
         * across is in use.
         * @param {VRS.RenderProperty} renderProperty A VRS.RenderProperty value.
         */
        function refreshMarkersIfUsingPinText(renderProperty)
        {
            if(!_Suspended) {
                if($.inArray(renderProperty, _Settings.plotterOptions.getPinTexts()) !== -1) refreshMarkers(null, null, true);
            }
        }

        /**
         * Called when the current location has changed.
         */
        function currentLocationChanged()
        {
            if(!_Suspended) that.refreshRangeCircles();
        }

        /**
         * Called when the configuration has changed.
         */
        function configurationChanged()
        {
            var destroyAndRepaintMarkers = configureSuppressTextOnImages();

            if(!_Suspended) {
                if(destroyAndRepaintMarkers) {
                    removeAllMarkers();
                    refreshMarkers(null, null, true);
                }
            }
        }
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
