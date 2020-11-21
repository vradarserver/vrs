var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftPictureSizeDesktopDetail = VRS.globalOptions.aircraftPictureSizeDesktopDetail || { width: 350 };
    VRS.globalOptions.aircraftPictureSizeInfoWindow = VRS.globalOptions.aircraftPictureSizeInfoWindow || { width: 85, height: 40 };
    VRS.globalOptions.aircraftPictureSizeIPadDetail = VRS.globalOptions.aircraftPictureSizeIPadDetail || { width: 680 };
    VRS.globalOptions.aircraftPictureSizeIPhoneDetail = VRS.globalOptions.aircraftPictureSizeIPhoneDetail || { width: 260 };
    VRS.globalOptions.aircraftPictureSizeList = VRS.globalOptions.aircraftPictureSizeList || { width: 60, height: 40 };
    VRS.AircraftFilterProperty = {
        Airport: 'air',
        Altitude: 'alt',
        Callsign: 'csn',
        Country: 'cou',
        Distance: 'dis',
        EngineType: 'egt',
        HideNoPosition: 'hnp',
        Icao: 'ico',
        IsMilitary: 'mil',
        ModelIcao: 'typ',
        Operator: 'opr',
        OperatorCode: 'opc',
        Registration: 'reg',
        Species: 'spc',
        Squawk: 'sqk',
        UserInterested: 'int',
        UserTag: 'tag',
        Wtc: 'wtc'
    };
    VRS.AircraftListSortableField = {
        None: '---',
        Altitude: 'alt',
        AltitudeBarometric: 'alb',
        AltitudeGeometric: 'alg',
        AltitudeType: 'aty',
        AirPressure: 'apr',
        AverageSignalLevel: 'avs',
        Bearing: 'bng',
        Callsign: 'csn',
        CivOrMil: 'mil',
        CountMessages: 'mct',
        Country: 'cou',
        Distance: 'dis',
        FlightsCount: 'fct',
        Heading: 'hdg',
        HeadingType: 'hty',
        Icao: 'ico',
        Latitude: 'lat',
        Longitude: 'lng',
        Manufacturer: 'man',
        Mlat: 'mlt',
        Model: 'mod',
        ModelIcao: 'typ',
        Operator: 'opr',
        OperatorIcao: 'opi',
        Receiver: 'rec',
        Registration: 'reg',
        Serial: 'ser',
        SignalLevel: 'sig',
        Speed: 'spd',
        SpeedType: 'sty',
        Squawk: 'sqk',
        TargetAltitude: 'tal',
        TargetHeading: 'thd',
        TimeTracked: 'tim',
        TransponderType: 'trt',
        UserTag: 'tag',
        VerticalSpeed: 'vsi',
        VerticalSpeedType: 'vty',
        YearBuilt: 'yrb'
    };
    VRS.AircraftListSource = {
        Unknown: 0,
        BaseStation: 1,
        FakeAircraftList: 2,
        FlightSimulatorX: 3
    };
    VRS.AircraftPictureServerSize = {
        DesktopDetailPanel: 'detail',
        IPhoneDetail: 'iPhoneDetail',
        IPadDetail: 'iPadDetail',
        List: 'list',
        Original: 'Full'
    };
    VRS.Alignment = {
        Left: 'l',
        Centre: 'c',
        Right: 'r'
    };
    VRS.AltitudeType = {
        Barometric: 0,
        Geometric: 1
    };
    VRS.DisplayUnitDependency = {
        Height: 'a',
        Speed: 'b',
        Distance: 'c',
        VsiSeconds: 'd',
        FLTransitionAltitude: 'e',
        FLTransitionHeightUnit: 'f',
        FLHeightUnit: 'g',
        Angle: 'h',
        Pressure: 'i'
    };
    VRS.Distance = {
        Kilometre: 'km',
        StatuteMile: 'sm',
        NauticalMile: 'nm'
    };
    VRS.EngineType = {
        None: 0,
        Piston: 1,
        Turbo: 2,
        Jet: 3,
        Electric: 4,
        Rocket: 5
    };
    VRS.EnginePlacement = {
        Unknown: 0,
        AftMounted: 1,
        WingBuried: 2,
        FuselageBuried: 3,
        NoseMounted: 4,
        WingMounted: 5
    };
    VRS.FilterCondition = {
        Equals: 'equ',
        Contains: 'con',
        Between: 'btw',
        Starts: 'srt',
        Ends: 'end'
    };
    VRS.FilterPropertyType = {
        OnOff: 'a',
        TextMatch: 'b',
        NumberRange: 'c',
        EnumMatch: 'd',
        DateRange: 'e',
        TextListMatch: 'f'
    };
    VRS.Height = {
        Metre: 'm',
        Feet: 'f'
    };
    VRS.InputWidth = {
        Auto: '',
        OneChar: 'oneChar',
        ThreeChar: 'threeChar',
        SixChar: 'sixChar',
        EightChar: 'eightChar',
        NineChar: 'nineChar',
        Long: 'long'
    };
    VRS.LabelWidth = {
        Auto: 0,
        Short: 1,
        Long: 2
    };
    VRS.LinkSite = {
        None: 'none',
        AirframesDotOrg: 'airframes.org',
        AirlinersDotNet: 'airliners.net',
        AirportDataDotCom: 'airport-data.com',
        StandingDataMaintenance: 'sdm',
        JetPhotosDotCom: 'jetphotos.com',
    };
    VRS.MapControlStyle = {
        Default: 'a',
        DropdownMenu: 'b',
        HorizontalBar: 'c'
    };
    VRS.MapPosition = {
        BottomCentre: 'bc',
        BottomLeft: 'bl',
        BottomRight: 'br',
        LeftBottom: 'lb',
        LeftCentre: 'lc',
        LeftTop: 'lt',
        RightBottom: 'rb',
        RightCentre: 'rc',
        RightTop: 'rt',
        TopCentre: 'tc',
        TopLeft: 'tl',
        TopRight: 'tr'
    };
    VRS.MapType = {
        Hybrid: 'h',
        RoadMap: 'm',
        Satellite: 's',
        Terrain: 't',
        HighContrast: 'o'
    };
    VRS.MobilePageName = {
        Map: 'map',
        AircraftDetail: 'aircraftDetail',
        AircraftList: 'aircraftList',
        Options: 'options'
    };
    VRS.OffRadarAction = {
        Nothing: '---',
        WaitForReturn: 'wfr',
        EnableAutoSelect: 'eas'
    };
    VRS.Pressure = {
        InHg: '0',
        Millibar: '1',
        MmHg: '2'
    };
    VRS.RenderProperty = {
        None: '---',
        AirportDataThumbnails: 'adt',
        AirPressure: 'apr',
        Altitude: 'alt',
        AltitudeBarometric: 'alb',
        AltitudeGeometric: 'alg',
        AltitudeAndVerticalSpeed: 'alv',
        AltitudeType: 'aty',
        AverageSignalLevel: 'avs',
        Bearing: 'bng',
        Callsign: 'csn',
        CallsignAndShortRoute: 'csr',
        CivOrMil: 'mil',
        CountMessages: 'mct',
        Country: 'cou',
        Distance: 'dis',
        Engines: 'eng',
        FlightLevel: 'flv',
        FlightLevelAndVerticalSpeed: 'fav',
        FlightsCount: 'fct',
        Heading: 'hdg',
        HeadingType: 'hty',
        Icao: 'ico',
        Interesting: 'int',
        Latitude: 'lat',
        Longitude: 'lng',
        Manufacturer: 'man',
        Mlat: 'mlt',
        Model: 'mod',
        ModelIcao: 'typ',
        Operator: 'opr',
        OperatorFlag: 'opf',
        OperatorIcao: 'opi',
        Picture: 'pct',
        PictureOrThumbnails: 'pic',
        PositionOnMap: 'pom',
        Receiver: 'rec',
        Registration: 'reg',
        RegistrationAndIcao: 'rai',
        RouteFull: 'rtf',
        RouteShort: 'rts',
        Serial: 'ser',
        SignalLevel: 'sig',
        Silhouette: 'sil',
        SilhouetteAndOpFlag: 'sop',
        Species: 'spc',
        Speed: 'spd',
        SpeedType: 'sty',
        Squawk: 'sqk',
        TargetAltitude: 'tal',
        TargetHeading: 'thd',
        TimeTracked: 'tim',
        Tisb: 'tsb',
        TransponderType: 'trt',
        TransponderTypeFlag: 'trf',
        UserNotes: 'not',
        UserTag: 'tag',
        VerticalSpeed: 'vsi',
        VerticalSpeedType: 'vty',
        Wtc: 'wtc',
        YearBuilt: 'yrb'
    };
    VRS.RenderSurface = {
        List: 0x00000001,
        DetailHead: 0x00000002,
        DetailBody: 0x00000004,
        Marker: 0x00000008,
        InfoWindow: 0x00000010
    };
    VRS.ReportAircraftProperty = {
        AircraftClass: 'acc',
        CofACategory: 'coc',
        CofAExpiry: 'coe',
        Country: 'cod',
        CurrentRegDate: 'crd',
        DeRegDate: 'der',
        Engines: 'eng',
        FirstRegDate: 'frd',
        GenericName: 'gen',
        Icao: 'ico',
        Interesting: 'int',
        Manufacturer: 'man',
        Military: 'mil',
        Model: 'mdl',
        ModelIcao: 'mdi',
        ModeSCountry: 'msc',
        MTOW: 'mto',
        Notes: 'not',
        Operator: 'opr',
        OperatorFlag: 'opf',
        OperatorIcao: 'ops',
        OwnershipStatus: 'ows',
        Picture: 'pic',
        PopularName: 'pop',
        PreviousId: 'prv',
        Registration: 'reg',
        SerialNumber: 'ser',
        Silhouette: 'sil',
        Species: 'spc',
        Status: 'sta',
        TotalHours: 'thr',
        UserTag: 'tag',
        WakeTurbulenceCategory: 'wtc',
        YearBuilt: 'yrb'
    };
    VRS.ReportFilterProperty = {
        Callsign: 'cal',
        Country: 'cou',
        Date: 'dat',
        FirstAltitude: 'fal',
        HadEmergency: 'emg',
        Icao: 'ico',
        IsMilitary: 'mil',
        LastAltitude: 'lal',
        ModelIcao: 'typ',
        Operator: 'opr',
        Species: 'spc',
        Registration: 'reg',
        WakeTurbulenceCategory: 'wtc'
    };
    VRS.ReportFlightProperty = {
        Altitude: 'alti',
        Callsign: 'call',
        CountAdsb: 'cads',
        CountModeS: 'cmds',
        CountPositions: 'cpos',
        Duration: 'drtn',
        EndTime: 'etim',
        FirstAltitude: 'falt',
        FirstFlightLevel: 'flvl',
        FirstHeading: 'ftrk',
        FirstLatitude: 'flat',
        FirstLongitude: 'flng',
        FirstOnGround: 'fgnd',
        FirstSpeed: 'fspd',
        FirstSquawk: 'fsqk',
        FirstVerticalSpeed: 'fvsi',
        FlightLevel: 'flev',
        HadAlert: 'halt',
        HadEmergency: 'hemg',
        HadSPI: 'hspi',
        LastAltitude: 'lalt',
        LastFlightLevel: 'llvl',
        LastHeading: 'ltrk',
        LastLatitude: 'llat',
        LastLongitude: 'llng',
        LastOnGround: 'lgnd',
        LastSpeed: 'lspd',
        LastSquawk: 'lsqk',
        LastVerticalSpeed: 'lvsi',
        PositionsOnMap: 'posn',
        RouteShort: 'rsht',
        RouteFull: 'rful',
        RowNumber: 'rown',
        Speed: 'sped',
        Squawk: 'sqwk',
        StartTime: 'stim'
    };
    VRS.ReportSortColumn = {
        None: '',
        Callsign: 'callsign',
        Country: 'country',
        Date: 'date',
        FirstAltitude: 'firstaltitude',
        Icao: 'icao',
        LastAltitude: 'lastaltitude',
        Model: 'model',
        ModelIcao: 'type',
        Operator: 'operator',
        Registration: 'reg'
    };
    VRS.ReportSurface = {
        List: 0x00000001,
        DetailHead: 0x00000002,
        DetailBody: 0x00000004
    };
    VRS.SortSpecial = {
        Neither: 0,
        First: 1,
        Last: 2
    };
    VRS.Species = {
        None: 0,
        LandPlane: 1,
        SeaPlane: 2,
        Amphibian: 3,
        Helicopter: 4,
        Gyrocopter: 5,
        Tiltwing: 6,
        GroundVehicle: 7,
        Tower: 8
    };
    VRS.Speed = {
        Knots: 'kt',
        MilesPerHour: 'ml',
        KilometresPerHour: 'km'
    };
    VRS.SpeedType = {
        Ground: 0,
        GroundReversing: 1,
        IndicatedAirSpeed: 2,
        TrueAirSpeed: 3
    };
    VRS.TrailDisplay = {
        None: 'a',
        SelectedOnly: 'b',
        AllAircraft: 'c'
    };
    VRS.TrailType = {
        Short: 'a',
        Full: 'b',
        ShortAltitude: 'c',
        FullAltitude: 'd',
        ShortSpeed: 'e',
        FullSpeed: 'f'
    };
    VRS.TransponderType = {
        Unknown: 0,
        ModeS: 1,
        Adsb: 2,
        Adsb0: 3,
        Adsb1: 4,
        Adsb2: 5
    };
    VRS.WakeTurbulenceCategory = {
        None: 0,
        Light: 1,
        Medium: 2,
        Heavy: 3
    };
})(VRS || (VRS = {}));
//# sourceMappingURL=enums.js.map