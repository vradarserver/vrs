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
 * @fileoverview Collects together all (well, most) of the enumerations in VRS.
 */

namespace VRS
{
    /*
     * GlobalOptions that don't have a natural home elsewhere
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftPictureSizeDesktopDetail = VRS.globalOptions.aircraftPictureSizeDesktopDetail || { width: 350 };      // The dimensions for desktop detail pictures.
    VRS.globalOptions.aircraftPictureSizeInfoWindow = VRS.globalOptions.aircraftPictureSizeInfoWindow || { width: 85, height: 40 }; // The dimensions for pictures in the info window.
    VRS.globalOptions.aircraftPictureSizeMobileDetail = VRS.globalOptions.aircraftPictureSizeMobileDetail || { width: 680 };            // The dimensions for iPad detail pictures.
    VRS.globalOptions.aircraftPictureSizeList = VRS.globalOptions.aircraftPictureSizeList || { width: 60, height: 40 };             // The dimensions for pictures in the list, set to null to have them sized to whatever the server returns.

    export type AircraftFilterPropertyEnum = string;
    /**
     * An enumeration of the different properties that can be compared against a filter by an AircraftFilter.
     * If 3rd party code adds to this list then they should not use 3 letter codes, they are reserved for VRS use.
     */
    export var AircraftFilterProperty = {
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
        OperatorCode:   'opc',
        Registration:   'reg',
        Species:        'spc',
        Squawk:         'sqk',
        UserInterested: 'int',
        UserTag:        'tag',
        Wtc:            'wtc'
    };

    export type AircraftListSortableFieldEnum = string;
    /**
     * An enumeration of the different fields that the aircraft list can be sorted on.
     * If 3rd party code adds to this list then they should not use 3 letter codes, they are reserved for VRS use.
     */
    export var AircraftListSortableField = {
        None:               '---',
        Altitude:           'alt',
        AltitudeBarometric: 'alb',
        AltitudeGeometric:  'alg',
        AltitudeType:       'aty',
        AirPressure:        'apr',
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
        Manufacturer:       'man',
        Mlat:               'mlt',
        Model:              'mod',
        ModelIcao:          'typ',
        Operator:           'opr',
        OperatorIcao:       'opi',
        PositionAgeSeconds: 'pas',
        Receiver:           'rec',
        Registration:       'reg',
        Serial:             'ser',
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
        VerticalSpeedType:  'vty',
        YearBuilt:          'yrb'
    };

    export type AircraftListSourceEnum = number;
    /**
     * An enumeration of the different sources of aircraft list data.
     */
    export var AircraftListSource = {
        Unknown:            0,
        BaseStation:        1,
        FakeAircraftList:   2,
        FlightSimulatorX:   3
    };

    export type AircraftPictureServerSize = string;
    /**
     * An enumeration of the different picture sizes understood by the server.
     */
    export var AircraftPictureServerSize = {
        DesktopDetailPanel: 'detail',           // Resize the aircraft picture for the desktop aircraft detail panel
        IPhoneDetail:       'iPhoneDetail',     // Resize the aircraft picture for detail panels on small screen mobile devices
        IPadDetail:         'iPadDetail',       // Resize the aircraft picture for detail panels on large screen mobile devices
        List:               'list',             // Resize the aircraft picture for aircraft lists
        Original:           'Full'              // Do not resize the aircraft picture
    };

    export type AlignmentEnum = string;
    /**
     * An enumeration of the different horizontal alignments.
     */
    export var Alignment = {
        Left:           'l',
        Centre:         'c',
        Right:          'r'
    };

    export type AltitudeTypeEnum = number;
    /**
     * An enumeration of the different altitude types.
     */
    export var AltitudeType = {
        Barometric:     0,
        Geometric:      1
    };

    export type DisplayUnitDependencyEnum = string;
    /**
     * An enumeration of the different kinds of display units that an element may be using.
     */
    export var DisplayUnitDependency = {
        Height:                 'a',
        Speed:                  'b',
        Distance:               'c',
        VsiSeconds:             'd',
        FLTransitionAltitude:   'e',
        FLTransitionHeightUnit: 'f',
        FLHeightUnit:           'g',
        Angle:                  'h',
        Pressure:               'i'
    };

    export type DistanceEnum = string;
    /**
     * An enumeration of the different units that distances can be displayed in.
     */
    export var Distance = {
        Kilometre:      'km',
        StatuteMile:    'sm',
        NauticalMile:   'nm'
    };

    export type EngineTypeEnum = number;
    /**
     * An enumeration of the different engine types sent by the server.
     */
    export var EngineType = {
        None:           0,
        Piston:         1,
        Turbo:          2,
        Jet:            3,
        Electric:       4,
        Rocket:         5
    };

    export type EnginePlacementEnum = number;
    /**
     * An enumeration of the different engine placements sent by the server.
     */
    export var EnginePlacement = {
        Unknown:        0,
        AftMounted:     1,
        WingBuried:     2,
        FuselageBuried: 3,
        NoseMounted:    4,
        WingMounted:    5
    };

    export type FilterConditionEnum = string;
    /**
     * An enumeration of the different filter conditions.
     */
    export var FilterCondition = {
        Equals:         'equ',
        Contains:       'con',
        Between:        'btw',
        Starts:         'srt',
        Ends:           'end'
    };

    export type FilterPropertyTypeEnum = string;
    /**
     * An enumeration of the different types of properties that filters can deal with.
     */
    export var FilterPropertyType = {
        OnOff:          'a',
        TextMatch:      'b',
        NumberRange:    'c',
        EnumMatch:      'd',
        DateRange:      'e',
        TextListMatch:  'f'         // As per TextMatch but the value is a list of strings and the condition is true if any string matches
    };

    export type HeightEnum = string;
    /**
     * An enumeration of the different units that altitudes can be displayed in.
     */
    export var Height = {
        Metre:  'm',
        Feet:   'f'
    };

    export type InputWidthEnum = string;
    /**
     * An enumeration of the different input widths that the CSS supports.
     */
    export var InputWidth = {
        Auto:       '',             // No input width is enforced
        OneChar:    'oneChar',      // Enough width for at least a single character
        ThreeChar:  'threeChar',    // Enough width for at least three characters
        SixChar:    'sixChar',      // Enough width for at least six characters
        EightChar:  'eightChar',    // Enough width for at least eight characters
        NineChar:   'nineChar',     // Enough width for at least nine characters
        Long:       'long'          // A large input field
    };

    export type LabelWidthEnum = number;
    //region LabelWidth
    /**
     * An enumeration of the different label widths that the CSS supports.
     */
    export var LabelWidth = {
        Auto:           0,          // No label width is enforced
        Short:          1,          // Suitable for short labels
        Long:           2           // Suitable for long labels
    };

    export type LinkSiteEnum = string;
    /**
     * An enumeration of different sites that can have links formed from an aircraft's details.
     */
    export var LinkSite = {
        None:                       'none',
        AirframesDotOrg:            'airframes.org',        // Defunct, requires account, no longer used
        AirlinersDotNet:            'airliners.net',
        AirportDataDotCom:          'airport-data.com',     // Defunct, AVG and Malwarebytes are blocking links to the site
        StandingDataMaintenance:    'sdm',                  // SDM Routes - names left alone for backwards compatability
        SDMAircraft:                'sdm-aircraft',         // SDM Aircraft lookup details
        JetPhotosDotCom:            'jetphotos.com',
    };

    export type MapControlStyleEnum = string;
    /**
     * An enumeration of the different control styles on a map.
     */
    export var MapControlStyle = {
        Default:                    'a',
        DropdownMenu:               'b',
        HorizontalBar:              'c'
    };

    export type MapPositionEnum = string;
    /**
     * The location at which controls can be added to the map.
     */
    export var MapPosition = {
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

    export type MapTypeEnum = string;
    /**
     * The different map types known to the map plugin. Third parties adding to this list should not use single-character
     * codes, they are reserved for use by VRS.
     */
    export var MapType = {
        Hybrid:         'h',
        RoadMap:        'm',
        Satellite:      's',
        Terrain:        't',
        HighContrast:   'o'         // <-- note that this is referenced BY VALUE in VRS.globalOptions.mapGoogleMapStyles
    };

    export type MobilePageNameEnum = string;
    /**
     * The names for the pages on the mobile site. These need to be unique.
     */
    export var MobilePageName = {
        Map:            'map',
        AircraftDetail: 'aircraftDetail',
        AircraftList:   'aircraftList',
        Options:        'options'
    };

    export type OffRadarActionEnum = string;
    /**
     * An enumeration of different actions that can be taken when an aircraft goes off-radar.
     */
    export var OffRadarAction = {
        Nothing:            '---',
        WaitForReturn:      'wfr',
        EnableAutoSelect:   'eas'
    };

    export type PressureEnum = string;
    /**
     * An enumeration of the different pressure units that the site can deal with.
     */
    export var Pressure = {
        InHg:               '0',
        Millibar:           '1',
        MmHg:               '2'
    };

    export type RenderPropertyEnum = string;
    /**
     * An enumeration of the different properties that can be rendered for an aircraft.
     * If 3rd party code adds to this list then they should not use 3 letter codes. Those are reserved for VRS use.
     */
    export var RenderProperty = {
        None:                           '---',
        AirportDataThumbnails:          'adt',
        AirPressure:                    'apr',
        Altitude:                       'alt',
        AltitudeBarometric:             'alb',
        AltitudeGeometric:              'alg',
//        AltitudeAndSpeedGraph:          'grs',
        AltitudeAndVerticalSpeed:       'alv',
//        AltitudeGraph:                  'gra',
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
        IdentActive:                    'ida',
        Interesting:                    'int',
        Latitude:                       'lat',
        Longitude:                      'lng',
        Manufacturer:                   'man',
        Mlat:                           'mlt',
        Model:                          'mod',
        ModelIcao:                      'typ',
        Operator:                       'opr',
        OperatorFlag:                   'opf',
        OperatorIcao:                   'opi',
        Picture:                        'pct',      // Pre-2.0.2 this was pic
        PictureOrThumbnails:            'pic',      // Took over pic from 2.0.2 onwards
        PositionAgeSeconds:             'pas',
        PositionOnMap:                  'pom',
        Receiver:                       'rec',
        Registration:                   'reg',
        RegistrationAndIcao:            'rai',
        RouteFull:                      'rtf',
        RouteShort:                     'rts',
        Serial:                         'ser',
        SignalLevel:                    'sig',
        Silhouette:                     'sil',
        SilhouetteAndOpFlag:            'sop',
        Species:                        'spc',
        Speed:                          'spd',
        SpeedType:                      'sty',
//        SpeedGraph:                     'spg',
        Squawk:                         'sqk',
        SquawkAndIdent:           'sqi',
        TargetAltitude:                 'tal',
        TargetHeading:                  'thd',
        TimeTracked:                    'tim',
        Tisb:                           'tsb',
        TransponderType:                'trt',
        TransponderTypeFlag:            'trf',
        UserNotes:                      'not',
        UserTag:                        'tag',
        VerticalSpeed:                  'vsi',
        VerticalSpeedType:              'vty',
        Wtc:                            'wtc',
        YearBuilt:                      'yrb'
    };

    export type RenderSurfaceBitFlags = number;
    /**
     * A set of bitflags indicating the different areas that a RenderProperty can be rendered on.
     */
    export var RenderSurface = {
        List:           0x00000001,         // The property is being rendered into the aircraft list
        DetailHead:     0x00000002,         // The property is being rendered into the header portion of the aircraft detail panel
        DetailBody:     0x00000004,         // The property is being rendered into the body portion of the aircraft detail panel
        Marker:         0x00000008,         // The property is being rendered as pin text onto a map marker
        InfoWindow:     0x00000010          // The property is being rendered into the mobile map info window
    };

    export type ReportAircraftPropertyEnum = string;
    /**
     * An enumeration of all of the properties that can be shown for an aircraft in a report. These must be unique both
     * within this enum and within ReportFlightProperty - to make this easier all of these values are 3 characters
     * whereas ReportFlightProperty enums are 4 characters.
     */
    export var ReportAircraftProperty = {
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
        UserTag:                'tag',
        WakeTurbulenceCategory: 'wtc',
        YearBuilt:              'yrb'
    };

    export type ReportFilterPropertyEnum = string;
    /**
     * An enumeration of the different criteria in a report.
     */
    export var ReportFilterProperty = {
        Callsign:               'cal',
        Country:                'cou',
        Date:                   'dat',
        FirstAltitude:          'fal',
        HadEmergency:           'emg',
        Icao:                   'ico',
        IsMilitary:             'mil',
        LastAltitude:           'lal',
        ModelIcao:              'typ',
        Operator:               'opr',
        Species:                'spc',
        Registration:           'reg',
        WakeTurbulenceCategory: 'wtc'
    };

    export type ReportFlightPropertyEnum = string;
    /**
     * An enumeration of the different columns that can be shown for a flight. Each must be unique both within this enum
     * and within VRS.ReportAircraftProperty - to make this easier all of these values are 4 characters
     * whereas ReportAircraftProperty enums are 3 characters.
     */
    export var ReportFlightProperty = {
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

    export type ReportAircraftOrFlightPropertyEnum = (ReportAircraftPropertyEnum | ReportFlightPropertyEnum);

    export type ReportSortColumnEnum = string;
    /**
     * An enumeration of the columns that reports can be sorted on.
     */
    export var ReportSortColumn = {
        None:                   '',
        Callsign:               'callsign',
        Country:                'country',
        Date:                   'date',
        FirstAltitude:          'firstaltitude',
        Icao:                   'icao',
        LastAltitude:           'lastaltitude',
        Model:                  'model',
        ModelIcao:              'type',
        Operator:               'operator',
        Registration:           'reg'
    };

    export type ReportSurfaceBitFlags = number;
    /**
     * A set of bitflags indicating the different areas that a ReportProperty can be rendered on.
     */
    export var ReportSurface = {
        List:           0x00000001,         // The property is being rendered into the report list
        DetailHead:     0x00000002,         // The property is being rendered into the header portion of the detail panel
        DetailBody:     0x00000004          // The property is being rendered into the body portion of the detail panel
    };

    export type SortSpecialEnum = number;
    /**
     * An enumeration of the different special positions an element can appear at within a sorted list.
     */
    export var SortSpecial = {
        Neither:        0,
        First:          1,
        Last:           2
    };

    export type SpeciesEnum = number;
    /**
     * An enumeration of the different species types sent by the server.
     */
    export var Species = {
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

    export type SpeedEnum = string;
    /**
     * An enumeration of the different units that speeds can be displayed in.
     */
    export var Speed = {
        Knots:              'kt',
        MilesPerHour:       'ml',
        KilometresPerHour:  'km'
    };

    export type SpeedTypeEnum = number;
    /**
     * An enumeration of the different types of speed that can be transmitted by the aircraft.
     */
    export var SpeedType = {
        Ground:             0,
        GroundReversing:    1,
        IndicatedAirSpeed:  2,
        TrueAirSpeed:       3
    };

    export type TrailDisplayEnum = string
    /**
     * An enumeration of the different kinds of trail display settings.
     */
    export var TrailDisplay = {
        None:           'a',
        SelectedOnly:   'b',
        AllAircraft:    'c'
    };

    export type TrailTypeEnum = string;
    /**
     * An enumeration of the different kinds of trail that can be displayed.
     */
    export var TrailType = {
        Short:          'a',            // Short trail, monochrome
        Full:           'b',            // Full trail, monochrome
        ShortAltitude:  'c',            // Short trail, colour indicates altitude
        FullAltitude:   'd',            // Full trail, colour indicate altitude
        ShortSpeed:     'e',            // Short trail, colour indicates speed
        FullSpeed:      'f'             // Full trail, colour indicates speed
    };

    export type TransponderTypeEnum = number;
    /**
     * An enumeration of the different types of transponder carried by aircraft.
     */
    export var TransponderType = {
        Unknown:        0,              // Transponder type is unknown
        ModeS:          1,              // Mode-S transponder with no ADS-B
        Adsb:           2,              // Mode-S transponder with ADS-B, cannot be certain about ADS-B version
        Adsb0:          3,              // Mode-S transponder with ADS-B, certain that it is version 0
        Adsb1:          4,              // Mode-S transponder with ADS-B, certain that it is version 1
        Adsb2:          5               // Mode-S transponder with ADS-B, certain that it is version 2
    };

    export type WakeTurbulenceCategoryEnum = number;
    /**
     * An enumeration of the different kind of wake turbulence categories (roughly equivalent to size / weight) sent by the server.
     */
    export var WakeTurbulenceCategory = {
        None:           0,
        Light:          1,
        Medium:         2,
        Heavy:          3
    };
}
