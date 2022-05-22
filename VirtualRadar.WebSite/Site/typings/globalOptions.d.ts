declare namespace VRS
{
    export interface GlobalOptions
    {
        isFlightSim?: boolean;                          /* Undocumented */
        isMobile?: boolean;                             /* Undocumented */
        isReport?: boolean;                             /* Undocumented */


        // Aircraft details
        /* aircraft */
        suppressTrails?: boolean;                       // If true then trails are never shown for any aircraft.
        aircraftHideUncertainCallsigns?: boolean;       // If true then uncertain callsigns are never shown.
        aircraftMaxAvgSignalLevelHistory?: number;      // The number of signal level values that the average signal level is calculated over.

        /* airportDataApi */
        airportDataApiThumbnailsUrl?: string;                       // The default URL for airportdata.com thumbnails JSON fetches.
        airportDataApiTimeout?: number;                             // The number of milliseconds to wait before timing out a fetch of airportdata.com JSON.

        /* jquery.vrs.aircraftDetail */
        detailPanelDefaultShowUnits?: boolean;                      // True if the aircraft details panel should default to showing distance / speed / height units.
        detailPanelDefaultItems?: RenderPropertyEnum[];             // An array of VRS.RenderProperty values that are shown by default in the aircraft detail panel.
        detailPanelUserCanConfigureItems?: boolean;                 // True if the user can change the items shown for an aircraft in the details panel.
        detailPanelShowSeparateRouteLink?: boolean;                 // Show a separate link to add or correct routes if the detail panel is showing routes.
        detailPanelShowSDMAircraftLink?: boolean;                   // Show a link to add or update aircraft lookup details on the SDM site.
        detailPanelShowAircraftLinks?: boolean;                     // True to show the links for an aircraft, false to suppress them.
        detailPanelShowEnableAutoSelect?: boolean;                  // True to show a link to enable and disable auto-select, false to suppress the link.
        detailPanelShowCentreOnAircraft?: boolean;                  // True to show a link to centre the map on the selected aircraft.
        detailPanelFlagUncertainCallsigns?: boolean;                // True if uncertain callsigns are to be flagged up on the detail panel.
        detailPanelDistinguishOnGround?: boolean;                   // True if altitudes should show 'GND' when the aircraft is on the ground.
        detailPanelAirportDataThumbnails?: number;                  // The number of airportdata.com thumbnails to show.
        detailPanelUseShortLabels?: boolean;                        // True if the short list heading labels should be used in the aircraft detail panel.

        /* jquery.vrs.aircraftInfoWindow */
        aircraftInfoWindowClass?: string;                           // The CSS class to assign to info windows.
        aircraftInfoWindowEnabled?: boolean;                        // True if the info window is enabled by default.
        aircraftInfoWindowItems?: RenderPropertyEnum[];             // An array of VRS.RenderProperty values that are shown by default in the info window.
        aircraftInfoWindowShowUnits?: boolean;                      // True if units should be shown in the info window.
        aircraftInfoWindowFlagUncertainCallsigns?: boolean;         // True if uncertain callsigns are to be flagged as such.
        aircraftInfoWindowDistinguishOnGround?: boolean;            // True if aircraft on the ground should show an altitude of GND.
        aircraftInfoWindowAllowConfiguration?: boolean;             // True if the user can configure the infowindow settings.
        aircraftInfoWindowEnablePanning?: boolean;                  // True if the map should pan to the info window when it opens.

        /* aircraftAutoSelect */
        aircraftAutoSelectEnabled?: boolean;                    // True if auto-select is enabled by default.
        aircraftAutoSelectClosest?: boolean;                    // True if auto-select closest is enabled by default.
        aircraftAutoSelectOffRadarAction?: OffRadarActionEnum;  // The VRS.OffRadarAction enum value describing the default auto-select behaviour when an aircraft goes off radar.
        aircraftAutoSelectFilters?: AircraftFilter[];           // The initial array of VRS.AircraftFilter objects that will be used by auto-select.
        aircraftAutoSelectFiltersLimit?: number;                // The maximum number of auto-select filters that the user can enter.

        /* enums */
        aircraftPictureSizeDesktopDetail?: ISizePartial;            // The width or height in pixels of aircraft pictures on the desktop page.
        aircraftPictureSizeInfoWindow?: ISize;                      // The width and height in pixels of the mobile page's info window.
        aircraftPictureSizeMobileDetail?: ISizePartial;             // The width or height in pixels of aircraft pictures when viewed on mobile devices.
        aircraftPictureSizeList?: ISize;                            // The width and height in pixels of aircraft picture thumbnails in the aircraft list.

        /* linksRenderer */
        linkSeparator?: string;                                     // The string to display between links in groups of external links.
        linkClass?: string;                                         // The CSS class to assign to links.



        // Aircraft lists
        /* aircraftListFetcher */
        aircraftListUrl?: string;                       // The URL that the aircraft list JSON is fetched from.
        aircraftListFlightSimUrl?: string;              // The URL that the aircraft list JSON is fetched from when running in Flight Simulator X mode.
        aircraftListDataType?: string;                  // The format that the aircraft list is returned in.
        aircraftListTimeout?: number;                   // The number of milliseconds before an aircraft list fetch will time out.
        aircraftListRetryInterval?: number;             // The number of milliseconds to wait before fetching an aircraft list after a failure.
        aircraftListFixedRefreshInterval?: number;      // The number of milliseconds between refreshes, -1 if the user can configure this themselves.
        aircraftListRequestFeedId?: number;             // The ID of the feed to request. If undefined then the default feed configured on the server is fetched.
        aircraftListUserCanChangeFeeds?: boolean;       // True if the user can change feeds, false if they cannot.
        aircraftListHideAircraftNotOnMap?: boolean;     // True if aircraft that are not on display are hidden from the list, false if they are not.

        /* aircraftListFilter */
        filterLabelWidth?: LabelWidthEnum;              /* Undocumented */
        aircraftListDefaultFiltersEnabled?: boolean;    // True if filters are enabled by default.
        aircraftListFilters?: AircraftFilter[];         // The initial array of VRS.AircraftFilter objects that the list is filtered by.
        aircraftListFiltersLimit?: number;              // The maximum number of list filters that the user can enter.

        /* aircraftListSorter */
        aircraftListDefaultSortOrder?: AircraftListSorter_SortField[];  // (See Note 1) The default sort order of the aircraft list.
        aircraftListShowEmergencySquawks?: SortSpecialEnum;             // Indicates the precedence given to aircraft transmitting emergency squawks in the aircraft list.
        aircraftListShowInteresting?: SortSpecialEnum;                  // Indicates the precedence given to aircraft flagged as interesting in the aircraft list.

        /* jquery.vrs.aircraftList */
        listPluginDistinguishOnGround?: boolean;                    // True if altitudes should distinguish between a value of 0 and aircraft that are on the ground.
        listPluginFlagUncertainCallsigns?: boolean;                 // True if uncertain callsigns should be flagged in the aircraft list.
        listPluginUserCanConfigureColumns?: boolean;                // True if the user can configure the aircraft list columns.
        listPluginDefaultColumns?: RenderPropertyEnum[];            // An array of VRS.RenderProperty values that are shown by default in the aircraft list.
        listPluginDefaultShowUnits?: boolean;                       // True if units should be shown by default.
        listPluginShowSorterOptions?: boolean;                      // True if sorter options should be shown on the list configuration panel.
        listPluginShowPause?: boolean;                              // True if a pause link should be shown on the list, false if it should not.
        listPluginShowHideAircraftNotOnMap?: boolean;               // True if the link to hide aircraft not on map should be shown.



        // Map options
        /* aircraftPlotter */
        aircraftMarkerPinTextWidth?: number;                        // The pixel width for markers with pin text drawn on them.
        aircraftMarkerPinTextLineHeight?: number;                   // The pixel height for each line of pin text on a marker.
        aircraftMarkerRotate?: boolean;                             // True if markers are allowed to rotate to indicate the direction of travel, assuming that the natural state points due north.
        aircraftMarkerRotationGranularity?: number;                 // The smallest number of degrees an aircraft has to rotate through before its marker is rotated.
        aircraftMarkerAllowAltitudeStalk?: boolean;                 // True if altitude stalks can be shown, false if they are permanently suppressed.
        aircraftMarkerShowAltitudeStalk?: boolean;                  // True if altitude stalks are shown by default, false if they are not.
        aircraftMarkerAllowPinText?: boolean;                       // True to allow the user to display pin text on the markers. This can be overridden by server options.
        aircraftMarkerDefaultPinTexts?: RenderPropertyEnum[];       // (See Note 2) The initial array of VRS.RenderProperty values to show on the markers.
        aircraftMarkerPinTextLines?: number;                        // The number of lines of pin text to show on markers.
        aircraftMarkerMaximumPinTextLines?: number;                 // The maximum number of lines of pin text that a user can show on markers.
        aircraftMarkerHideEmptyPinTextLines?: boolean;              // True if empty pin text lines are to be hidden.
        aircraftMarkerSuppressAltitudeStalkWhenZoomed?: boolean;    // True to suppress the altitude stalk when zoomed out, false to always show it.
        aircraftMarkerSuppressAltitudeStalkZoomLevel?: number;      // The map zoom level at which altitude stalks will be suppressed.
        aircraftMarkerTrailColourNormal?: string;                   // The CSS colour of trails for aircraft that are not selected.
        aircraftMarkerTrailColourSelected?: string;                 // The CSS colour of trails for aircraft that are selected.
        aircraftMarkerTrailWidthNormal?: number;                    // The width in pixels of trails for aircraft that are not selected.
        aircraftMarkerTrailWidthSelected?: number;                  // The width in pixels of trails for aircraft that are selected.
        aircraftMarkerTrailDisplay?: TrailDisplayEnum;              // The VRS.TrailDisplay value that indicates which aircraft to display trails for.
        aircraftMarkerTrailType?: TrailTypeEnum;                    // The VRS.TrailType value that indicates the type of trail to display.
        aircraftMarkerShowTooltip?: boolean;                        // True to show tooltips on aircraft markers, false otherwise.
        aircraftMarkerMovingMapOn?: boolean;                        // True if the moving map is switched on by default, false if it is not.
        aircraftMarkerSuppressTextOnImages?: boolean;               // True if pin texts are rendered in JavaScript instead of at the server. Usually automatically set to true when server is running under Mono.
        aircraftMarkerAllowRangeCircles?: boolean;                  // True if range circles are to be allowed, false if they are to be suppressed.
        aircraftMarkerShowRangeCircles?: boolean;                   // True if range circles are to be shown by default, false if they are not.
        aircraftMarkerRangeCircleInterval?: number;                 // The number of distance units between each successive range circle.
        aircraftMarkerRangeCircleDistanceUnit?: DistanceEnum;       // The VRS.Distance value indicating the units for aircraftMarkerRangeCircleInterval
        aircraftMarkerRangeCircleCount?: number;                    // The number of range circles to display around the current location.
        aircraftMarkerRangeCircleOddColour?: string;                // The CSS colour for the odd range circles.
        aircraftMarkerRangeCircleEvenColour?: string;               // The CSS colour for the even range circles.
        aircraftMarkerRangeCircleOddWeight?: number;                // The width in pixels for the odd range circles.
        aircraftMarkerRangeCircleEvenWeight?: number;               // The width in pixels for the even range circles.
        aircraftMarkerRangeCircleMaxCircles?: number;               // The maximum number of circles that the user can show.
        aircraftMarkerRangeCircleMaxInterval?: number;              // The maximum interval that the user can request for a range circle.
        aircraftMarkerRangeCircleMaxWeight?: number;                // The maximum weight that the user can specify for a range circle.
        aircraftMarkerAltitudeTrailLow?: number;                    // The low range to use when colouring altitude trails, in feet. Altitudes below this are coloured white.
        aircraftMarkerAltitudeTrailHigh?: number;                   // The high range to use when colouring altitude trails, in feet. Altitudes above this are coloured red.
        aircraftMarkerSpeedTrailLow?: number;                       // The low range to use when colouring speed trails, in knots. Speeds below this are coloured white.
        aircraftMarkerSpeedTrailHigh?: number;                      // The high range to use when colouring speed trails, in knots. Speeds above this are coloured red.
        aircraftMarkerAlwaysPlotSelected?: boolean;                 // True to always plot the selected aircraft, even if it is not on the map. This preserves the selected aircraft's trail.
        aircraftMarkerHideNonAircraftZoomLevel?: number;            // The zoom level at which non-aircraft traffic is hidden. Lower numbers are further away from the ground.
        aircraftMarkerShowNonAircraftTrails?: boolean;              // True if non-aircraft traffic should have trails drawn for them.
        aircraftMarkerOnlyUsePre22Icons?: boolean;                  // True if only the old pre-version 2.2 aircraft icon should be shown.
        aircraftMarkerClustererEnabled?: boolean;                   // True if clusters of aircraft should be presented as a single icon.
        aircraftMarkerClustererMaxZoom?: number;                    // The zoom level at which clusters of aircraft should be presented as a single icon. Smaller numbers are further away from the ground.
        aircraftMarkerClustererMinimumClusterSize?: number;         // The number of aircraft that have to be grouped together before they become merged into a single cluster icon.
        aircraftMarkerClustererUserCanConfigure?: boolean;          // True if the user can configure the parameters for aircraft marker clustering.
        aircraftMarkers?: AircraftMarker[];                         // (See Note 7) An array of the aircraft icons that can represent different types of aircraft on the map.

        /* aircraftRenderer */
        aircraftRendererEnableDebugProperties?: boolean;            // True if debug properties can be shown in aircraft lists, on markers etc.

        /* jquery.vrs.aircraftPositionMap */
        aircraftPositionMapClass?: string;                          // The CSS class to assign to the aircraft position map panel.

        /* jquery.vrs.map */
        mapGoogleMapHttpUrl?: string;                               // The HTTP URL to load Google Map JavaScript from.
        mapGoogleMapHttpsUrl?: string;                              // The HTTPS URL to load Google Map JavaScript from.
        mapGoogleMapTimeout?: number;                               // The number of milliseconds that the site will wait for before it times out the fetch of Google Maps JavaScript.
        mapGoogleMapUseHttps?: boolean;                             // True if the Google Maps JavaScript should be fetched over HTTPS.
        mapLeafletNoWrap?: boolean;                                 // True if Leaflet maps should have wrapping turned off.
        mapShowStreetView?: boolean;                                // True if the StreetView icon should be shown on the map.
        mapScrollWheelActive?: boolean;                             // True if the scroll wheel zooms the map. Overridden in some panels.
        mapDraggable?: boolean;                                     // True if the user can move the map. Overridden in some panels.
        mapShowPointsOfInterest?: boolean;                          // True if maps should show points of interest by default.
        mapShowScaleControl?: boolean;                              // True if maps should show the scale control by default.
        mapShowHighContrastStyle?: boolean;                         // True if the custom high contrast map style should be offered to the user.
        mapHighContrastMapStyle?: google.maps.MapTypeStyle[];       // The Google map styles to use for the high contrast map. See Google's map style wizard.

        /* jquery.vrs.mapNextPageButton */
        mapNextPageButtonClass?: string;                            // The CSS class to use on the next page button shown on the mobile map.
        mapNextPageButtonImage?: string;                            // The image to use for the normal next page button on the mobile map.
        mapNextPageButtonSize?: ISize;                              // The size of the normal next page button.
        mapNextPageButtonPausedImage?: string;                      // The image to use for paused next page on the mobile map.
        mapNextPageButtonPausedSize?: ISize;                        // The size of the paused next page button.
        mapNextPageButtonFilteredImage?: string;                    // The image to use for the "filtering is in effect" next page button.
        mapNextPageButtonFilteredSize?: ISize;                      // The size of the filtered next page button.

        /* jquery.vrs.menu */
        menuClass?: string;                                         // The CSS class to use on the menu.

        /* jquery.vrs.optionDialog */
        optionsDialogWidth?: number | string;                       // The width of the options dialog.
        optionsDialogHeight?: number | string;                      // The height of the options dialog.
        optionsDialogModal?: boolean;                               // True if the options dialog is modal.
        optionsDialogPosition?: any;                                // The default position of the options dialog.

        /* jquery.vrs.splitter */
        splitterBorderWidth?: number;                               // The width in pixels of the splitter bar.

        /* polarPlotter */
        polarPlotEnabled?: boolean;                                                                     // True if the user is allowed to see receiver range plots, false if they are to be suppressed.
        polarPlotFetchUrl?: string;                                                                     // The URL to fetch plotter JSON data from.
        polarPlotFetchTimeout?: number;                                                                 // The timeout in milliseconds when fetching polar plots from the server.
        polarPlotAutoRefreshSeconds?: number;                                                           // The number of seconds between the automatic refresh of all polar plots on display. Set to 0 to disable automatic refreshes.
        polarPlotAltitudeConfigs?: PolarPlot_AltitudeRangeConfig[];                                     // (See Note 6) An array of objects that describe the colours and Z-index for each plot. The object is { low: number, high: number, colour: string, zIndex: number } where low and high describe the altitude range (-1 for an open end) and colour is a CSS string.
        polarPlotUserConfigurable?: boolean;                                                            // True if the user can configure the receiver range plot colours and opacity.
        polarPlotStrokeWeight?: number;                                                                 // The pixel width of the lines to draw around the edge of a polar plot.
        polarPlotStrokeColour?: string;                                                                 // The CSS colour to use for the outline of the plot. If this is an empty string then the plot is outlined in the fill colour.
        polarPlotStrokeOpacity?: number;                                                                // The transparency of the polar plot's outline. 0.0 is transparent, 1.0 is opaque.
        polarPlotFillOpacity?: number;                                                                  // The transparency of the polar plot fill. 0.0 is transparent, 1.0 is opaque.
        polarPlotStrokeColourCallback?: (feedId: number, lowAlt: number, highAlt: number) => string;    // A function that is passed the feed ID, low altitude and high altitude and returns the CSS colour for the stroke.
        polarPlotFillColourCallback?: (feedId: number, lowAlt: number, highAlt: number) => string;      // A function that is passed the feed ID, low altitude and high altitude and returns the CSS colour for the fill.
        polarPlotDisplayOnStartup?: PolarPlot_FeedSliceAbstract[];                                      // An array of objects that describe the plots to show when the site starts. The object is { feedName: string, low: number, high: number } where low and high describe the altitude range (-1 for an open end) and feedName is the case insensitive name of a feed with a polar plotter attached.



        // Miscellaneous
        /* audio */
        audioEnabled?: boolean;                                     // True if the audio features are enabled (can still be disabled on server). False if they are to be suppressed.
        audioAnnounceSelected?: boolean;                            // True if details of selected aircraft should be announced.
        audioAnnounceOnlyAutoSelected?: boolean;                    // True if only auto-selected aircraft should have their details announced.
        audioDefaultVolume?: number;                                // The default volume for the audio control. Range is 0.0 to 1.0.
        audioTimeout?: number;                                      // The number of milliseconds that must elapse before audio is timed-out.

        /* configStorage */
        configSuppressEraseOldSiteConfig?: boolean;                 // True if the old site's configuration is not to be erased by the new site. If you set this to false it could lead the new site to be slighty less efficient when sending requests to the server.

        /* currentLocation */
        currentLocationFixed?: ILatLng;                             // Set to an object of { lat: 1.234, lng: 5.678 }; to force the default current location (when the user has not assigned a location) to a fixed point rather than the server-configured initial location.
        currentLocationConfigurable?: boolean;                      // True if the user is allowed to set their current location.
        currentLocationIconUrl?: string;                            // The URL of the marker to display on the map for the set current location marker. Set to null for the default Google Map marker.
        currentLocationUseGeoLocation?: boolean;                    // True if the option to use the browser's current location should be shown.
        currentLocationUseBrowserLocation?: boolean;                // (See Note 3) True if the browser location should be used as the current location. This overrides the map centre / user-supplied location options.
        currentLocationShowOnMap?: boolean;                         // True if the current location should be shown on the map.
        currentLocationImageUrl?: string;                           // The URL of the current location marker.
        currentLocationImageSize?: ISize;                           // An object of { width: x, height: y } indicating the size of the current location marker in pixels.
        currentLocationUseMapCentreForFirstVisit?: boolean;         // If true then on the first visit the user-supplied current location is set to the map centre. If false then the user must always choose a current location (i.e. the same behaviour as version 1 of the site).

        /* format */
        aircraftOperatorFlagSize?: ISize;                           // An object of { width: x, height: y } indicating the size of aircraft operator flag images.
        aircraftSilhouetteSize?: ISize;                             // An object of { width: x, height: y } indicating the size of aircraft silhouette images.
        aircraftBearingCompassSize?: ISize;                         // An object of { width: x, height: y } indicating the size of the compass bearing image.
        aircraftTransponderTypeSize?: ISize;                        // An object of { width: x, height: y } indicating the size of the transponder type images.
        aircraftFlagUncertainCallsigns?: boolean;                   // True if callsigns that we're not 100% sure about are to be shown with an asterisk against them on. This overrides all other 'FlagUncertainCallsign' options.
        aircraftAllowRegistrationFlagOverride?: boolean;            // True if you want to search for operator flags and silhouettes that match an aircraft's registration. Note that registrations can match the operator code or silhouette for other aircraft.

        /* scriptManager */
        scriptManagerTimeout?: number;                              // The timeout in milliseconds when dynamically loading scripts (e.g. Google Maps and the language files).

        /* svgGenerator */
        svgAircraftMarkerTextStyle?: Object;                        // An object of SVG attributes to apply to the pin text on aircraft markers (e.g. { 'font-family': 'Roboto', 'font-size': '8pt' }).
        svgAircraftMarkerTextShadowFilterXml?: string;              // The filter that is applied to text to add a shadow to it. Set this to an empty string to remove the shadow entirely.
        svgAircraftMarkerAltitudeLineStroke?: string;               // The CSS colour for the altitude line.
        svgAircraftMarkerAltitudeLineWidth?: number;                // The width of the altitude line in pixels.
        svgAircraftMarkerNormalFill?: string;                       // The CSS colour for unselected aircraft.
        svgAircraftMarkerSelectedFill?: string;                     // The CSS colour for selected aircraft.

        /* unitDisplayPreferences */
        unitDisplayHeight?: HeightEnum;                             // The default VRS.Height unit for altitudes.
        unitDisplaySpeed?: SpeedEnum;                               // The default VRS.Speed unit for speeds.
        unitDisplayDistance?: DistanceEnum;                         // The default VRS.Distance unit for distances.
        unitDisplayPressure?: PressureEnum;                         // The default VRS.Pressure unit for air pressure.
        unitDisplayVsiPerSecond?: boolean;                          // (See Note 4) True if vertical speeds are to be shown per second rather than per minute.
        unitDisplayFLTransitionAltitude?: number;                   // The default flight level transition altitude.
        unitDisplayFLTransitionHeightUnit?: HeightEnum;             // The VRS.Height unit for unitDisplayFLTransitionAltitude.
        unitDisplayFLHeightUnit?: HeightEnum;                       // The VRS.Height unit that flight levels are displayed in.
        unitDisplayAllowConfiguration?: boolean;                    // True if the user can configure the unit display options.
        unitDisplayAltitudeType?: boolean;                          // True if the altitude type should be shown.
        unitDisplayVerticalSpeedType?: boolean;                     // True if the vertical speed type should be shown.
        unitDisplaySpeedType?: boolean;                             // True if the speed type should be shown.
        unitDisplayTrackType?: boolean;                             // True if the heading type should be shown.
        unitDisplayUsePressureAltitude?: boolean;                   // True to show pressure altitudes or false to show corrected altitudes by default.

        /* serverConfiguration */
        serverConfigUrl?: string;                                   // The URL to fetch the server configuration from.
        serverConfigDataType?: string;                              // The format that the server configuration will use.
        serverConfigTimeout?: number;                               // The number of milliseconds to wait before timing out a fetch of server configuration. 
        serverConfigRetryInterval?: number;                         // The number of milliseconds to wait before retrying a fetch of server configuration.
        serverConfigOverwrite?: boolean;                            // True to overwrite the existing configuration with the configuration stored on the server.
        serverConfigResetBeforeImport?: boolean;                    // True to erase the existing configuration before importing the configuration stored on the server.
        serverConfigIgnoreSplitters?: boolean;                      // True to ignore the splitter settings when importing the configuration stored on the server.
        serverConfigIgnoreLanguage?: boolean;                       // True to ignore the language settings when importing the configuration stored on the server.
        serverConfigIgnoreRequestFeedId?: boolean;                  // True to ignore the feed ID to fetch when importing the configuration stored on the server.




        // Reports
        /* jquery.vrs.pagePanel */
        pagePanelClass?: string;                                    // The CSS class to assign to page panels in the options dialog.

        /* jquery.vrs.reportDetail */
        reportDetailClass?: string;                                                         // The CSS class to assign to the report detail panel.
        reportDetailColumns?: (ReportAircraftPropertyEnum | ReportFlightPropertyEnum)[];    // An array of VRS.ReportAircraftProperty and VRS.ReportFlightProperty values that are shown in the aircraft detail panel by default.
        reportDetailAddMapToDefaultColumns?: boolean;                                       // (See Note 3) True if the map should be added to the default columns.
        reportDetailDefaultShowUnits?: boolean;                                             // True if the detail panel should show units by default.
        reportDetailDistinguishOnGround?: boolean;                                          // True if the detail panel should show GND for aircraft that are on the ground.
        reportDetailUserCanConfigureColumns?: boolean;                                      // True if the user is allowed to configure which values are shown in the detail panel.
        reportDetailDefaultShowEmptyValues?: boolean;                                       // True if empty values are to be shown.
        reportDetailShowAircraftLinks?: boolean;                                            // True if external site links are to be shown.
        reportDetailShowSeparateRouteLink?: boolean;                                        // True if the user should be shown a link to correct routes.
        reportDetailShowSDMAircraftLink?: boolean;                                          // True if the user should be shown a link to add or update aircraft lookup details on the SDM site.

        /* jquery.vrs.reportList */
        reportListClass?: string;                                                                       // The CSS class to assign to the report list panel.
        reportListSingleAircraftColumns?: (ReportFlightPropertyEnum | ReportAircraftPropertyEnum)[];    // An array of VRS.ReportFlightProperty and VRS.ReportAircraftProperty values to show for reports on a single aircraft.
        reportListManyAircraftColumns?: (ReportFlightPropertyEnum | ReportAircraftPropertyEnum)[];      // An array of VRS.ReportFlightProperty and VRS.ReportAircraftProperty values to show for reports with criteria that could cover many aircraft.
        reportListDefaultShowUnits?: boolean;                                                           // True if the default should be to show units.
        reportListDistinguishOnGround?: boolean;                                                        // True if aircraft on ground should be shown as an altitude of GND.
        reportListShowPagerTop?: boolean;                                                               // True if the report list is to show a pager above the list.
        reportListShowPagerBottom?: boolean;                                                            // True if the report list is to show a pager below the list.
        reportListUserCanConfigureColumns?: boolean;                                                    // True if the user is allowed to configure the columns in the report list.
        reportListGroupBySortColumn?: boolean;                                                          // True if the report list should show group rows when the value of the first sort column changes.
        reportListGroupResetAlternateRows?: boolean;                                                    // True if the report list should reset the alternate row shading at the start of each group.

        /* jquery.vrs.reportMap */
        reportMapClass?: string;                                    // The CSS class to assign to the report map panel.
        reportMapScrollToAircraft?: boolean;                        // True if the map should automatically scroll to show the selected aircraft's path.
        reportMapShowPath?: boolean;                                // True if a line should be drawn between the start and end points of the aircraft's path.
        reportMapStartSelected?: boolean;                           // True if the start point should be displayed in the selected colours, false if the end point should show in selected colours.

        /* jquery.vrs.reportPager */
        reportPagerClass?: string;                                  // The CSS class to assign to the report pager panel.
        reportPagerSpinnerPageSize?: number;                        // The number of pages to skip when paging up and down through the page number spinner.
        reportPagerAllowPageSizeChange?: boolean;                   // True if the user can change the report page size, false if they cannot.
        reportPagerAllowShowAllRows?: boolean;                      // True if the user is allowed to show all rows simultaneously, false if they're not.

        /* report */
        reportDefaultStepSize?: number;                             // The default step to show on the page size controls.
        reportDefaultPageSize?: number;                             // The default page size to show. Use -1 if you want to default to showing all rows.
        reportUrl?: string;                                         // The URL to fetch report data from.
        reportDefaultSortColumns?: Report_SortColumn[];             // (See Note 5) The default sort order for reports. Note that the server will not accept any more than two sort columns.
        reportShowPermanentLinkToReport?: boolean;                  // True to show the permanent link to a report and its criteria.
        reportUseRelativeDatesInLink?: boolean;                     // True to use relative dates in the permanent link.

        /* reportFilter */
        reportMaximumCriteria?: number;                             // The maximum number of criteria that can be passed to a report.
        reportFindAllPermutationsOfCallsign?: boolean;              // True if all permutations of a callsign should be found.
    }
}
