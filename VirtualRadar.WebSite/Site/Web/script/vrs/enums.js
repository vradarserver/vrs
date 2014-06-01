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
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview Collects together all (well, most) of the enumerations in VRS.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region GlobalOptions that don't have a natural home elsewhere
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftPictureSizeDesktopDetail = VRS.globalOptions.aircraftPictureSizeDesktopDetail || { width: 350 };      // The dimensions for desktop detail pictures.
    VRS.globalOptions.aircraftPictureSizeInfoWindow = VRS.globalOptions.aircraftPictureSizeInfoWindow || { width: 85, height: 40 }; // The dimensions for pictures in the info window.
    VRS.globalOptions.aircraftPictureSizeIPadDetail = VRS.globalOptions.aircraftPictureSizeIPadDetail || { width: 680 };            // The dimensions for iPad detail pictures.
    VRS.globalOptions.aircraftPictureSizeIPhoneDetail = VRS.globalOptions.aircraftPictureSizeIPhoneDetail || { width: 260 };        // The dimensions for iPhone detail pictures.
    VRS.globalOptions.aircraftPictureSizeList = VRS.globalOptions.aircraftPictureSizeList || { width: 60, height: 40 };             // The dimensions for pictures in the list, set to null to have them sized to whatever the server returns.
    //endregion

    //region AircraftFilterProperty
    /**
     * An enumeration of the different properties that can be compared against a filter by an AircraftFilter.
     * If 3rd party code adds to this list then they should not use 3 letter codes, they are reserved for VRS use.
     * @enum {string}
     * @readonly
     */
    VRS.AircraftFilterProperty = {
        Airport:        'air',
        Altitude:       'alt',
        Callsign:       'csn',
        Country:        'cou',
        Distance:       'dis',
        EngineType:     'egt',
        HideNoPosition: 'hnp',
        Icao:           'ico',
        IsMilitary:     'mil',
        ModelIcao:      'typ',
        Operator:       'opr',
        Registration:   'reg',
        Species:        'spc',
        Squawk:         'sqk',
        UserInterested: 'int',
        Wtc:            'wtc'
    };
    //endregion

    //region AircraftListSortableField
    /**
     * An enumeration of the different fields that the aircraft list can be sorted on.
     * If 3rd party code adds to this list then they should not use 3 letter codes, they are reserved for VRS use.
     * @enum {string}
     * @readonly
     */
    VRS.AircraftListSortableField = {
        None:               '---',
        Altitude:           'alt',
        AltitudeType:       'aty',
        AverageSignalLevel: 'avs',
        Bearing:            'bng',
        Callsign:           'csn',
        CivOrMil:           'mil',
        CountMessages:      'mct',
        Country:            'cou',
        Distance:           'dis',
        FlightsCount:       'fct',
        Heading:            'hdg',
        HeadingType:        'hty',
        Icao:               'ico',
        Latitude:           'lat',
        Longitude:          'lng',
        Model:              'mod',
        ModelIcao:          'typ',
        Operator:           'opr',
        OperatorIcao:       'opi',
        Receiver:           'rec',
        Registration:       'reg',
        SignalLevel:        'sig',
        Speed:              'spd',
        SpeedType:          'sty',
        Squawk:             'sqk',
        TargetAltitude:     'tal',
        TargetHeading:      'thd',
        TimeTracked:        'tim',
        TransponderType:    'trt',
        UserTag:            'tag',
        VerticalSpeed:      'vsi',
        VerticalSpeedType:  'vty'
    };
    //endregion

    //region AircraftListSource
    /**
     * An enumeration of the different sources of aircraft list data.
     * @enum {number}
     * @readonly
     */
    VRS.AircraftListSource = {
        Unknown:            0,
        BaseStation:        1,
        FakeAircraftList:   2,
        FlightSimulatorX:   3
    };
    //endregion

    //region AircraftPictureServerSize
    /**
     * An enumeration of the different picture sizes understood by the server.
     * @enum {string}
     * @readonly
     */
    VRS.AircraftPictureServerSize = {
        DesktopDetailPanel: 'detail',           // Resize the aircraft picture for the desktop aircraft detail panel
        IPhoneDetail:       'iPhoneDetail',     // Resize the aircraft picture for detail panels on small screen mobile devices
        IPadDetail:         'iPadDetail',       // Resize the aircraft picture for detail panels on large screen mobile devices
        List:               'list',             // Resize the aircraft picture for aircraft lists
        Original:           'Full'              // Do not resize the aircraft picture
    };
    //endregion

    //region Alignment
    /**
     * An enumeration of the different horizontal alignments.
     * @enum {string}
     * @readonly
     */
    VRS.Alignment = {
        Left:           'l',
        Centre:         'c',
        Right:          'r'
    };
    //endregion

    //region AltitudeType
    /**
     * An enumeration of the different altitude types.
     * @enum {number}
     * @readonly
     */
    VRS.AltitudeType = {
        Barometric:     0,
        Geometric:      1
    };
    //endregion

    //region DisplayUnit
    /**
     * An enumeration of the different kinds of display units that an element may be using.
     * @enum {string}
     * @readonly
     */
    VRS.DisplayUnitDependency = {
        Height:                 'a',
        Speed:                  'b',
        Distance:               'c',
        VsiSeconds:             'd',
        FLTransitionAltitude:   'e',
        FLTransitionHeightUnit: 'f',
        FLHeightUnit:           'g',
        Angle:                  'h'
    };
    //endregion

    //region Distance
    /**
     * An enumeration of the different units that distances can be displayed in.
     * @enum {string}
     * @readonly
     */
    VRS.Distance = {
        Kilometre:      'km',
        StatuteMile:    'sm',
        NauticalMile:   'nm'
    };
    //endregion

    //region EngineType
    /**
     * An enumeration of the different engine types sent by the server.
     * @enum {number}
     * @readonly
     */
    VRS.EngineType = {
        None:           0,
        Piston:         1,
        Turbo:          2,
        Jet:            3,
        Electric:       4
    };
    //endregion

    //region FilterCondition
    /**
     * An enumeration of the different filter conditions.
     * @enum {string}
     * @readonly
     */
    VRS.FilterCondition = {
        Equals:         'equ',
        Contains:       'con',
        Between:        'btw',
        Starts:         'srt',
        Ends:           'end'
    };
    //endregion

    //region FilterPropertyType
    /**
     * An enumeration of the different types of properties that filters can deal with.
     * @enum {string}
     * @readonly
     */
    VRS.FilterPropertyType = {
        OnOff:          'a',
        TextMatch:      'b',
        NumberRange:    'c',
        EnumMatch:      'd',
        DateRange:      'e',
        TextListMatch:  'f'         // As per TextMatch but the value is a list of strings and the condition is true if any string matches
    };
    //endregion

    //region Height
    /**
     * An enumeration of the different units that altitudes can be displayed in.
     * @enum {string}
     * @readonly
     */
    VRS.Height = {
        Metre:  'm',
        Feet:   'f'
    };
    //endregion

    //region InputWidth
    /**
     * An enumeration of the different input widths that the CSS supports.
     * @enum {string}
     * @readonly
     */
    VRS.InputWidth = {
        Auto:       '',             // No input width is enforced
        OneChar:    'oneChar',      // Enough width for at least a single character
        ThreeChar:  'threeChar',    // Enough width for at least three characters
        SixChar:    'sixChar',      // Enough width for at least six characters
        EightChar:  'eightChar',    // Enough width for at least eight characters
        NineChar:   'nineChar',     // Enough width for at least nine characters
        Long:       'long'          // A large input field
    };
    //endregion

    //region LabelWidth
    /**
     * An enumeration of the different label widths that the CSS supports.
     * @enum {number}
     * @readonly
     */
    VRS.LabelWidth = {
        Auto:           0,          // No label width is enforced
        Short:          1,          // Suitable for short labels
        Long:           2           // Suitable for long labels
    };
    //endregion

    //region LinkSite
    /**
     * An enumeration of different sites that can have links formed from an aircraft's details.
     * @enum {string}
     */
    VRS.LinkSite = {
        None:                       'none',
        AirframesDotOrg:            'airframes.org',
        AirlinersDotNet:            'airliners.net',
        AirportDataDotCom:          'airport-data.com',
        StandingDataMaintenance:    'sdm'
    };
    //endregion

    //region MapControlStyle
    /**
     * An enumeration of the different control styles on a map.
     * @enum {string}
     */
    VRS.MapControlStyle = {
        Default:                    'a',
        DropdownMenu:               'b',
        HorizontalBar:              'c'
    };
    //endregion

    //region MapPosition
    /**
     * The location at which controls can be added to the map.
     * @enum {string}
     * @readonly
     */
    VRS.MapPosition = {
        BottomCentre:   'bc',
        BottomLeft:     'bl',
        BottomRight:    'br',
        LeftBottom:     'lb',
        LeftCentre:     'lc',
        LeftTop:        'lt',
        RightBottom:    'rb',
        RightCentre:    'rc',
        RightTop:       'rt',
        TopCentre:      'tc',
        TopLeft:        'tl',
        TopRight:       'tr'
    };
    //endregion

    //region MapType
    /**
     * The different map types known to the map plugin. Third parties adding to this list should not use single-character
     * codes, they are reserved for use by VRS.
     * @enum {string}
     * @readonly
     */
    VRS.MapType = {
        Hybrid:         'h',
        RoadMap:        'm',
        Satellite:      's',
        Terrain:        't',
        HighContrast:   'o'         // <-- note that this is referenced BY VALUE in VRS.globalOptions.mapGoogleMapStyles
    };
    //endregion

    //region MobilePageName
    /**
     * The names for the pages on the mobile site. These need to be unique.
     * @enum {string}
     */
    VRS.MobilePageName = {
        Map:            'map',
        AircraftDetail: 'aircraftDetail',
        AircraftList:   'aircraftList',
        Options:        'options'
    };
    //endregion

    //region OffRadarAction
    /**
     * An enumeration of different actions that can be taken when an aircraft goes off-radar.
     * @enum {string}
     */
    VRS.OffRadarAction = {
        Nothing:            '---',
        WaitForReturn:      'wfr',
        EnableAutoSelect:   'eas'
    };
    //endregion

    //region RenderProperty
    /**
     * An enumeration of the different properties that can be rendered for an aircraft.
     * If 3rd party code adds to this list then they should not use 3 letter codes. Those are reserved for VRS use.
     * @enum {string}
     * @readonly
     */
    VRS.RenderProperty = {
        None:                           '---',
        AirportDataThumbnails:          'adt',
        Altitude:                       'alt',
//        AltitudeAndSpeedGraph:          'als',
        AltitudeAndVerticalSpeed:       'alv',
//        AltitudeGraph:                  'alg',
        AltitudeType:                   'aty',
        AverageSignalLevel:             'avs',
        Bearing:                        'bng',
        Callsign:                       'csn',
        CallsignAndShortRoute:          'csr',
        CivOrMil:                       'mil',
        CountMessages:                  'mct',
        Country:                        'cou',
        Distance:                       'dis',
        Engines:                        'eng',
        FlightLevel:                    'flv',
        FlightLevelAndVerticalSpeed:    'fav',
        FlightsCount:                   'fct',
        Heading:                        'hdg',
        HeadingType:                    'hty',
        Icao:                           'ico',
        Interesting:                    'int',
        Latitude:                       'lat',
        Longitude:                      'lng',
        Model:                          'mod',
        ModelIcao:                      'typ',
        Operator:                       'opr',
        OperatorFlag:                   'opf',
        OperatorIcao:                   'opi',
        Picture:                        'pct',      // Pre-2.0.2 this was pic
        PictureOrThumbnails:            'pic',      // Took over pic from 2.0.2 onwards
        PositionOnMap:                  'pom',
        Receiver:                       'rec',
        Registration:                   'reg',
        RegistrationAndIcao:            'rai',
        RouteFull:                      'rtf',
        RouteShort:                     'rts',
        SignalLevel:                    'sig',
        Silhouette:                     'sil',
        SilhouetteAndOpFlag:            'sop',
        Species:                        'spc',
        Speed:                          'spd',
        SpeedType:                      'sty',
//        SpeedGraph:                     'spg',
        Squawk:                         'sqk',
        TargetAltitude:                 'tal',
        TargetHeading:                  'thd',
        TimeTracked:                    'tim',
        TransponderType:                'trt',
        TransponderTypeFlag:            'trf',
        UserTag:                        'tag',
        VerticalSpeed:                  'vsi',
        VerticalSpeedType:              'vty',
        Wtc:                            'wtc'
    };
    //endregion

    //region RenderSurface
    /**
     * A set of bitflags indicating the different areas that a RenderProperty can be rendered on.
     * @enum {number}
     * @readonly
     */
    VRS.RenderSurface = {
        List:           0x00000001,         // The property is being rendered into the aircraft list
        DetailHead:     0x00000002,         // The property is being rendered into the header portion of the aircraft detail panel
        DetailBody:     0x00000004,         // The property is being rendered into the body portion of the aircraft detail panel
        Marker:         0x00000008,         // The property is being rendered as pin text onto a map marker
        InfoWindow:     0x00000010          // The property is being rendered into the mobile map info window
    };
    //endregion

    //region ReportAircraftProperty
    /**
     * An enumeration of all of the properties that can be shown for an aircraft in a report. These must be unique both
     * within this enum and within ReportFlightProperty - to make this easier all of these values are 3 characters
     * whereas ReportFlightProperty enums are 4 characters.
     * @enum {string}
     */
    VRS.ReportAircraftProperty = {
        AircraftClass:          'acc',
        CofACategory:           'coc',
        CofAExpiry:             'coe',
        Country:                'cod',
        CurrentRegDate:         'crd',
        DeRegDate:              'der',
        Engines:                'eng',
        FirstRegDate:           'frd',
        GenericName:            'gen',
        Icao:                   'ico',
        Interesting:            'int',
        Manufacturer:           'man',
        Military:               'mil',
        Model:                  'mdl',
        ModelIcao:              'mdi',
        ModeSCountry:           'msc',
        MTOW:                   'mto',
        Notes:                  'not',
        Operator:               'opr',
        OperatorFlag:           'opf',
        OperatorIcao:           'ops',
        OwnershipStatus:        'ows',
        Picture:                'pic',
        PopularName:            'pop',
        PreviousId:             'prv',
        Registration:           'reg',
        SerialNumber:           'ser',
        Silhouette:             'sil',
        Species:                'spc',
        Status:                 'sta',
        TotalHours:             'thr',
        WakeTurbulenceCategory: 'wtc',
        YearBuilt:              'yrb'
    };
    //endregion

    //region ReportFilterPropertyHandler
    /**
     * An enumeration of the different criteria in a report.
     * @enum {string}
     */
    VRS.ReportFilterProperty = {
        Callsign:               'cal',
        Country:                'cou',
        Date:                   'dat',
        HadEmergency:           'emg',
        Icao:                   'ico',
        IsMilitary:             'mil',
        ModelIcao:              'typ',
        Operator:               'opr',
        Species:                'spc',
        Registration:           'reg',
        WakeTurbulenceCategory: 'wtc'
    };
    //endregion

    //region ReportFlightProperty
    /**
     * An enumeration of the different columns that can be shown for a flight. Each must be unique both within this enum
     * and within VRS.ReportAircraftProperty - to make this easier all of these values are 4 characters
     * whereas ReportAircraftProperty enums are 3 characters.
     * @enum {string}
     */
    VRS.ReportFlightProperty = {
        Altitude:               'alti',
        Callsign:               'call',
        CountAdsb:              'cads',
        CountModeS:             'cmds',
        CountPositions:         'cpos',
        Duration:               'drtn',
        EndTime:                'etim',
        FirstAltitude:          'falt',
        FirstFlightLevel:       'flvl',
        FirstHeading:           'ftrk',
        FirstLatitude:          'flat',
        FirstLongitude:         'flng',
        FirstOnGround:          'fgnd',
        FirstSpeed:             'fspd',
        FirstSquawk:            'fsqk',
        FirstVerticalSpeed:     'fvsi',
        FlightLevel:            'flev',
        HadAlert:               'halt',
        HadEmergency:           'hemg',
        HadSPI:                 'hspi',
        LastAltitude:           'lalt',
        LastFlightLevel:        'llvl',
        LastHeading:            'ltrk',
        LastLatitude:           'llat',
        LastLongitude:          'llng',
        LastOnGround:           'lgnd',
        LastSpeed:              'lspd',
        LastSquawk:             'lsqk',
        LastVerticalSpeed:      'lvsi',
        PositionsOnMap:         'posn',
        RouteShort:             'rsht',
        RouteFull:              'rful',
        RowNumber:              'rown',
        Speed:                  'sped',
        Squawk:                 'sqwk',
        StartTime:              'stim'
    };
    //endregion

    //region ReportSortColumn
    /**
     * An enumeration of the columns that reports can be sorted on.
     * @enum {string}
     */
    VRS.ReportSortColumn = {
        None:                   '',
        Callsign:               'callsign',
        Country:                'country',
        Date:                   'date',
        Icao:                   'icao',
        Model:                  'model',
        ModelIcao:              'type',
        Operator:               'operator',
        Registration:           'reg'
    };
    //endregion

    //region ReportSurface
    /**
     * A set of bitflags indicating the different areas that a ReportProperty can be rendered on.
     * @enum {number}
     * @readonly
     */
    VRS.ReportSurface = {
        List:           0x00000001,         // The property is being rendered into the report list
        DetailHead:     0x00000002,         // The property is being rendered into the header portion of the detail panel
        DetailBody:     0x00000004          // The property is being rendered into the body portion of the detail panel
    };
    //endregion

    //region Species
    /**
     * An enumeration of the different species types sent by the server.
     * @enum {number}
     * @readonly
     */
    VRS.Species = {
        None:           0,
        LandPlane:      1,
        SeaPlane:       2,
        Amphibian:      3,
        Helicopter:     4,
        Gyrocopter:     5,
        Tiltwing:       6,
        GroundVehicle:  7,
        Tower:          8
    };
    //endregion

    //region Speed
    /**
     * An enumeration of the different units that speeds can be displayed in.
     * @enum {string}
     * @readonly
     */
    VRS.Speed = {
        Knots:              'kt',
        MilesPerHour:       'ml',
        KilometresPerHour:  'km'
    };
    //endregion

    //region SpeedType
    /**
     * An enumeration of the different types of speed that can be transmitted by the aircraft.
     * @enum {number}
     * @readonly
     */
    VRS.SpeedType = {
        Ground:             0,
        GroundReversing:    1,
        IndicatedAirSpeed:  2,
        TrueAirSpeed:       3
    };
    //endregion

    //region TrailDisplay
    /**
     * An enumeration of the different kinds of trail display settings.
     * @enum {string}
     * @readonly
     */
    VRS.TrailDisplay = {
        None:           'a',
        SelectedOnly:   'b',
        AllAircraft:    'c'
    };
    //endregion

    //region TrailType
    /**
     * An enumeration of the different kinds of trail that can be displayed.
     * @enum {string}
     * @readonly
     */
    VRS.TrailType = {
        Short:          'a',            // Short trail, monochrome
        Full:           'b',            // Full trail, monochrome
        ShortAltitude:  'c',            // Short trail, colour indicates altitude
        FullAltitude:   'd',            // Full trail, colour indicate altitude
        ShortSpeed:     'e',            // Short trail, colour indicates speed
        FullSpeed:      'f'             // Full trail, colour indicates speed
    };
    //endregion

    //region TransponderType
    VRS.TransponderType = {
        Unknown:        0,              // Transponder type is unknown
        ModeS:          1,              // Mode-S transponder with no ADS-B
        Adsb:           2,              // Mode-S transponder with ADS-B, cannot be certain about ADS-B version
        Adsb0:          3,              // Mode-S transponder with ADS-B, certain that it is version 0
        Adsb1:          4,              // Mode-S transponder with ADS-B, certain that it is version 1
        Adsb2:          5               // Mode-S transponder with ADS-B, certain that it is version 2
    };
    //endregion

    //region WakeTurbulenceCategory
    /**
     * An enumeration of the different kind of wake turbulence categories (roughly equivalent to size / weight) sent by the server.
     * @enum {number}
     * @readonly
     */
    VRS.WakeTurbulenceCategory = {
        None:           0,
        Light:          1,
        Medium:         2,
        Heavy:          3
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
