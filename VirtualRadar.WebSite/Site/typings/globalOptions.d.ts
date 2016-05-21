declare namespace VRS
{
    export interface GlobalOptions
    {
        isFlightSim?: boolean;
        isMobile?: boolean;

        // aircraft
        suppressTrails?: boolean;
        aircraftHideUncertainCallsigns?: boolean;
        aircraftMaxAvgSignalLevelHistory?: number;

        // aircraftAutoSelect
        aircraftAutoSelectEnabled?: boolean;
        aircraftAutoSelectClosest?: boolean;
        aircraftAutoSelectOffRadarAction?: OffRadarActionEnum;
        aircraftAutoSelectFilters?: AircraftFilter[];
        aircraftAutoSelectFiltersLimit?: number;

        // aircraftListFetcher
        aircraftListUrl?: string;
        aircraftListFlightSimUrl?: string;
        aircraftListDataType?: string;
        aircraftListTimeout?: number;
        aircraftListRetryInterval?: number;
        aircraftListFixedRefreshInterval?: number;
        aircraftListRequestFeedId?: number;
        aircraftListUserCanChangeFeeds?: boolean;
        aircraftListHideAircraftNotOnMap?: boolean;

        // aircraftListFilter
        aircraftListDefaultFiltersEnabled?: boolean;
        aircraftListFilters?: AircraftFilter[];
        aircraftListFiltersLimit?: number;

        // aircraftListSorter
        aircraftListDefaultSortOrder?: AircraftListSorter_SortField[];
        aircraftListShowEmergencySquawks?: SortSpecialEnum;
        aircraftListShowInteresting?: SortSpecialEnum;

        // aircraftPlotter
        aircraftMarkerPinTextWidth?: number;
        aircraftMarkerPinTextLineHeight?: number;
        aircraftMarkerRotate?: boolean;
        aircraftMarkerRotationGranularity?: number;
        aircraftMarkerAllowAltitudeStalk?: boolean;
        aircraftMarkerShowAltitudeStalk?: boolean;
        aircraftMarkerAllowPinText?: boolean;
        aircraftMarkerDefaultPinTexts?: RenderPropertyEnum[];
        aircraftMarkerPinTextLines?: number;
        aircraftMarkerMaximumPinTextLines?: number;
        aircraftMarkerHideEmptyPinTextLines?: boolean;
        aircraftMarkerSuppressAltitudeStalkWhenZoomed?: boolean;
        aircraftMarkerSuppressAltitudeStalkZoomLevel?: number;
        aircraftMarkerTrailColourNormal?: string;
        aircraftMarkerTrailColourSelected?: string;
        aircraftMarkerTrailWidthNormal?: number;
        aircraftMarkerTrailWidthSelected?: number;
        aircraftMarkerTrailDisplay?: TrailDisplayEnum;
        aircraftMarkerTrailType?: TrailTypeEnum;
        aircraftMarkerShowTooltip?: boolean;
        aircraftMarkerMovingMapOn?: boolean;
        aircraftMarkerSuppressTextOnImages?: boolean;
        aircraftMarkerAllowRangeCircles?: boolean;
        aircraftMarkerShowRangeCircles?: boolean;
        aircraftMarkerRangeCircleInterval?: number;
        aircraftMarkerRangeCircleDistanceUnit?: DistanceEnum;
        aircraftMarkerRangeCircleCount?: number;
        aircraftMarkerRangeCircleOddColour?: string;
        aircraftMarkerRangeCircleEvenColour?: string;
        aircraftMarkerRangeCircleOddWeight?: number;
        aircraftMarkerRangeCircleEvenWeight?: number;
        aircraftMarkerRangeCircleMaxCircles?: number;
        aircraftMarkerRangeCircleMaxInterval?: number;
        aircraftMarkerRangeCircleMaxWeight?: number;
        aircraftMarkerAltitudeTrailLow?: number;
        aircraftMarkerAltitudeTrailHigh?: number;
        aircraftMarkerSpeedTrailLow?: number;
        aircraftMarkerSpeedTrailHigh?: number;
        aircraftMarkerAlwaysPlotSelected?: boolean;
        aircraftMarkerHideNonAircraftZoomLevel?: number;
        aircraftMarkerShowNonAircraftTrails?: boolean;
        aircraftMarkerOnlyUsePre22Icons?: boolean;
        aircraftMarkerClustererEnabled?: boolean;
        aircraftMarkerClustererMaxZoom?: number;
        aircraftMarkerClustererMinimumClusterSize?: number;
        aircraftMarkerClustererUserCanConfigure?: boolean;
        aircraftMarkers?: AircraftMarker[];

        // aircraftRenderer
        aircraftRendererEnableDebugProperties?: boolean;

        // airportDataApi
        airportDataApiThumbnailsUrl?: string;
        airportDataApiTimeout?: number;

        // audio
        audioEnabled?: boolean;
        audioAnnounceSelected?: boolean;
        audioAnnounceOnlyAutoSelected?: boolean;
        audioDefaultVolume?: number;
        audioTimeout?: number;

        // configStorage
        configSuppressEraseOldSiteConfig?: boolean;

        // currentLocation
        currentLocationFixed?: ILatLng;
        currentLocationConfigurable?: boolean;
        currentLocationIconUrl?: string;
        currentLocationUseGeoLocation?: boolean;
        currentLocationUseBrowserLocation?: boolean;
        currentLocationShowOnMap?: boolean;
        currentLocationImageUrl?: string;
        currentLocationImageSize?: ISize;
        currentLocationUseMapCentreForFirstVisit?: boolean;

        // enums
        aircraftPictureSizeDesktopDetail?: ISizePartial;
        aircraftPictureSizeInfoWindow?: ISize;
        aircraftPictureSizeIPadDetail?: ISizePartial;
        aircraftPictureSizeIPhoneDetail?: ISizePartial;
        aircraftPictureSizeList?: ISize;

        // filter
        filterLabelWidth?: LabelWidthEnum;

        // format
        aircraftOperatorFlagSize?: ISize;
        aircraftSilhouetteSize?: ISize;
        aircraftBearingCompassSize?: ISize;
        aircraftTransponderTypeSize?: ISize;
        aircraftFlagUncertainCallsigns?: boolean;
        aircraftAllowRegistrationFlagOverride?: boolean;

        // jquery.vrs.aircraftDetail
        detailPanelDefaultShowUnits?: boolean;
        detailPanelDefaultItems?: RenderPropertyEnum[];
        detailPanelUserCanConfigureItems?: boolean;
        detailPanelShowSeparateRouteLink?: boolean;
        detailPanelShowAircraftLinks?: boolean;
        detailPanelShowEnableAutoSelect?: boolean;
        detailPanelShowCentreOnAircraft?: boolean;
        detailPanelFlagUncertainCallsigns?: boolean;
        detailPanelDistinguishOnGround?: boolean;
        detailPanelAirportDataThumbnails?: number;
        detailPanelUseShortLabels?: boolean;

        // jquery.vrs.aircraftInfoWindow
        aircraftInfoWindowClass?: string;
        aircraftInfoWindowEnabled?: boolean;
        aircraftInfoWindowItems?: RenderPropertyEnum[];
        aircraftInfoWindowShowUnits?: boolean;
        aircraftInfoWindowFlagUncertainCallsigns?: boolean;
        aircraftInfoWindowDistinguishOnGround?: boolean;
        aircraftInfoWindowAllowConfiguration?: boolean;
        aircraftInfoWindowEnablePanning?: boolean;

        // jquery.vrs.aircraftList
        listPluginDistinguishOnGround?: boolean;
        listPluginFlagUncertainCallsigns?: boolean;
        listPluginUserCanConfigureColumns?: boolean;
        listPluginDefaultColumns?: RenderPropertyEnum[];
        listPluginDefaultShowUnits?: boolean;
        listPluginShowSorterOptions?: boolean;
        listPluginShowPause?: boolean;
        listPluginShowHideAircraftNotOnMap?: boolean;

        // jquery.vrs.aircraftPositionMap
        aircraftPositionMapClass?: string;

        // jquery.vrs.map
        mapGoogleMapHttpUrl?: string;
        mapGoogleMapHttpsUrl?: string;
        mapGoogleMapTimeout?: number;
        mapGoogleMapUseHttps?: boolean;
        mapShowStreetView?: boolean;
        mapScrollWheelActive?: boolean;
        mapDraggable?: boolean;
        mapShowPointsOfInterest?: boolean;
        mapShowScaleControl?: boolean;
        mapShowHighContrastStyle?: boolean;
        mapHighContrastMapStyle?: google.maps.MapTypeStyle[];

        // jquery.vrs.mapNextPageButton
        mapNextPageButtonClass?: string;
        mapNextPageButtonImage?: string;
        mapNextPageButtonSize?: ISize;
        mapNextPageButtonPausedImage?: string;
        mapNextPageButtonPausedSize?: ISize;
        mapNextPageButtonFilteredImage?: string;
        mapNextPageButtonFilteredSize?: ISize;

        // jquery.vrs.menu
        menuClass?: string;

        // jquery.vrs.optionDialog
        optionsDialogWidth?: number | string;
        optionsDialogHeight?: number | string;
        optionsDialogModal?: boolean;
        optionsDialogPosition?: any;

        // jquery.vrs.pagePanel
        pagePanelClass?: string;

        // jquery.vrs.reportDetail
        reportDetailClass?: string;
        reportDetailColumns?: (ReportAircraftPropertyEnum | ReportFlightPropertyEnum)[];
        reportDetailAddMapToDefaultColumns?: boolean;
        reportDetailDefaultShowUnits?: boolean;
        reportDetailDistinguishOnGround?: boolean;
        reportDetailUserCanConfigureColumns?: boolean;
        reportDetailDefaultShowEmptyValues?: boolean;
        reportDetailShowAircraftLinks?: boolean;
        reportDetailShowSeparateRouteLink?: boolean;

        // jquery.vrs.reportList
        reportListClass?: string;
        reportListSingleAircraftColumns?: (ReportFlightPropertyEnum | ReportAircraftPropertyEnum)[];
        reportListManyAircraftColumns?: (ReportFlightPropertyEnum | ReportAircraftPropertyEnum)[];
        reportListDefaultShowUnits?: boolean;
        reportListDistinguishOnGround?: boolean;
        reportListShowPagerTop?: boolean;
        reportListShowPagerBottom?: boolean;
        reportListUserCanConfigureColumns?: boolean;
        reportListGroupBySortColumn?: boolean;
        reportListGroupResetAlternateRows?: boolean;

        // jquery.vrs.reportMap
        reportMapClass?: string;
        reportMapScrollToAircraft?: boolean;
        reportMapShowPath?: boolean;
        reportMapStartSelected?: boolean;

        // jquery.vrs.reportPager
        reportPagerClass?: string;
        reportPagerSpinnerPageSize?: number;
        reportPagerAllowPageSizeChange?: boolean;
        reportPagerAllowShowAllRows?: boolean;

        // jquery.vrs.splitter
        splitterBorderWidth?: number;

        // linksRenderer
        linkSeparator?: string;
        linkClass?: string;

        // polarPlotter
        polarPlotEnabled?: boolean;
        polarPlotFetchUrl?: string;
        polarPlotFetchTimeout?: number;
        polarPlotAutoRefreshSeconds?: number;
        polarPlotAltitudeConfigs?: PolarPlot_AltitudeRangeConfig[];
        polarPlotUserConfigurable?: boolean;
        polarPlotStrokeWeight?: number;
        polarPlotStrokeColour?: string;
        polarPlotStrokeOpacity?: number;
        polarPlotFillOpacity?: number;
        polarPlotStrokeColourCallback?: (feedId: number, lowAlt: number, highAlt: number) => string;
        polarPlotFillColourCallback?: (feedId: number, lowAlt: number, highAlt: number) => string;
        polarPlotDisplayOnStartup?: PolarPlot_FeedSliceAbstract[];

        // report
        reportDefaultStepSize?: number;
        reportDefaultPageSize?: number;
        reportUrl?: string;
        reportDefaultSortColumns?: Report_SortColumn[];
        reportShowPermanentLinkToReport?: boolean;
        reportUseRelativeDatesInLink?: boolean;

        // reportFilter
        reportMaximumCriteria?: number;
        reportFindAllPermutationsOfCallsign?: boolean;

        // scriptManager
        scriptManagerTimeout?: number;

        // serverConfiguration
        serverConfigUrl?: string;
        serverConfigDataType?: string;
        serverConfigTimeout?: number;
        serverConfigRetryInterval?: number;
        serverConfigOverwrite?: boolean;
        serverConfigResetBeforeImport?: boolean;
        serverConfigIgnoreSplitters?: boolean;
        serverConfigIgnoreLanguage?: boolean;
        serverConfigIgnoreRequestFeedId?: boolean;

        // unitDisplayPreferences
        unitDisplayHeight?: HeightEnum;
        unitDisplaySpeed?: SpeedEnum;
        unitDisplayDistance?: DistanceEnum;
        unitDisplayPressure?: PressureEnum;
        unitDisplayVsiPerSecond?: boolean;
        unitDisplayFLTransitionAltitude?: number;
        unitDisplayFLTransitionHeightUnit?: HeightEnum;
        unitDisplayFLHeightUnit?: HeightEnum;
        unitDisplayAllowConfiguration?: boolean;
        unitDisplayAltitudeType?: boolean;
        unitDisplayVerticalSpeedType?: boolean;
        unitDisplaySpeedType?: boolean;
        unitDisplayTrackType?: boolean;
        unitDisplayUsePressureAltitude?: boolean;
    }
}