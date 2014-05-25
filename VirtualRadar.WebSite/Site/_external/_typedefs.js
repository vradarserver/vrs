// This is not source. It's a bunch of jsdoc typedefs for anonymous objects. The site does not use this file.

//region VRS_AIRCRAFTLIST_ROWDATA, VRS_AIRCRAFTLIST_ROWDATA_ROW, VRS_AIRCRAFTLIST_ROWDATA_CELL
/**
* @typedef {{
* aircraftId:   number=,
* rowState:     VRS.AircraftListPluginState.RowState=,
* visible:      bool=
* }} VRS_AIRCRAFTLIST_ROWDATA_ROW
*/
VRS_AIRCRAFTLIST_ROWDATA_ROW;

/**
* @typedef {{
* alignment:        VRS.Alignment=,
* fixedWidth:       string=,
* hasTitle:         bool=,
* value:            ?,
* widgetProperty:   VRS.RenderProperty=
* }} VRS_AIRCRAFTLIST_ROWDATA_CELL
*/
VRS_AIRCRAFTLIST_ROWDATA_CELL;

/**
* @typedef {{
* row:          VRS_AIRCRAFTLIST_ROWDATA_ROW,
* cells:        Array.<VRS_AIRCRAFTLIST_ROWDATA_CELL>
* }} VRS_AIRCRAFTLIST_ROWDATA
*/
VRS_AIRCRAFTLIST_ROWDATA;
//endregion

//region VRS_ANY_FILTERPROPERTY
/**
 * @typedef {(VRS.AircraftFilterProperty|VRS.ReportFilterProperty)}
 */
VRS_ANY_FILTERPROPERTY;
//endregion

//region VRS_ANY_VALUECONDITION
/**
 * @typedef {VRS.OneValueCondition|VRS.TwoValueCondition} VRS_ANY_VALUECONDITION
 */
VRS_ANY_VALUECONDITION;
//endregion

//region VRS_AUDIO_AUTOPLAY
/**
 * @typedef {{
 * src:     String
 * }} VRS_AUDIO_AUTOPLAY
 */
VRS_AUDIO_AUTOPLAY;
//endregion

//region VRS_BOUNDS
/**
 * @typedef {{
 * tlLat: number,
 * tlLng: number,
 * brLat: number,
 * brLng: number
 * }} VRS_BOUNDS
 */
VRS_BOUNDS;
//endregion

//region VRS_COLOUR
/**
 * @typedef {{
 * r:   Number,
 * g:   Number,
 * b:   Number,
 * a:   Number=
 * }} VRS_COLOUR
 */
VRS_COLOUR;
//endregion

//region VRS_CONDITION
/**
 * @typedef {{
 * condition:           VRS.FilterCondition,
 * reverseCondition:    bool,
 * labelKey:            string=
 * }} VRS_CONDITION
 */
VRS_CONDITION;
//endregion

//region VRS_ENABLE_FLAG_SETTINGS
/**
 * @typedef {{
 * getValue:            function():boolean,
 * setValue:            function(boolean),
 * saveState:           function()
 * }} VRS_ENABLE_FLAG_SETTINGS
 */
VRS_ENABLE_FLAG_SETTINGS;
//endregion

//region VRS_EVENT_HANDLE
/**
* @typedef {{
* callback: function,
* forceThis: object
* }} VRS_EVENT_HANDLE
*/
VRS_EVENT_HANDLE;
//endregion

//region VRS_FORMAT_ARG
/**
* @typedef {{
* toFormattedString:    function(string):string=,
* localeFormat:         function(string):string=,
* format:               function(string):string=
* }} VRS_FORMAT_ARG
*/
VRS_FORMAT_ARG;
//endregion

//region VRS_GLOBAL_OPTIONS
/**
 * @typedef {?}
 */
VRS_GLOBAL_OPTIONS;
//endregion

//region VRS_GOOGLE_ICON
/**
 * @typedef {{
 * anchor:      google.maps.Point,
 * origin:      google.maps.Point,
 * scaledSize:  google.maps.Size,
 * size:        google.maps.Size,
 * url:         string,
 * labelAnchor: google.maps.Point=
 * }} VRS_GOOGLE_ICON
 */
VRS_GOOGLE_ICON;
//endregion

//region VRS_JSON_AIRCRAFT
/**
* @typedef {{
* Id:           Number,
* TSecs:        Number,
* Rcvr:         Number,
* Icao:         String,
* Bad:          boolean,
* Reg:          String,
* Alt:          Number,
* AltT:         Number,
* Call:         String,
* CallSus:      boolean,
* Lat:          Number,
* Long:         Number,
* PosTime:      Number,
* Spd:          Number,
* SpdTyp:       Number,
* Vsi:          Number,
* VsiT:         Number,
* Trak:         Number,
* TrkH:         boolean,
* Mdl:          String,
* Type:         String,
* From:         String,
* To:           String,
* Op:           String,
* OpIcao:       String,
* Sqk:          String,
* Help:         boolean,
* Dst:          Number,
* Brng:         Number,
* WTC:          Number,
* Engines:      String,
* EngType:      Number,
* Species:      Number,
* Mil:          boolean,
* Cou:          String,
* HasPic:       boolean,
* PicX:         Number,
* PicY:         Number,
* FlightsCount: Number,
* CMsgs:        Number,
* Gnd:          boolean,
* Tag:          String,
* Interested:   boolean,
* Stops:        String[],
* TT:           String,
* Cos:          Number[],
* Cot:          Number[],
* ResetTrail:   boolean,
* HasSig:       boolean=,
* Sig:          Number=
* }} VRS_JSON_AIRCRAFT
*/
VRS_JSON_AIRCRAFT;
//endregion

//region VRS_JSON_AIRCRAFTLIST
/**
* @typedef {{
* totalAc:          number,
* src:              VRS.AircraftListSource,
* showSil:          bool,
* showFlg:          bool,
* showPic:          bool,
* flgW:             number,
* flgH:             number,
* lastDv:           number,
* shtTrlSec:        number,
* stm:              number,
* acList:           VRS_JSON_AIRCRAFT[],
* feeds:            VRS_RECEIVER[],
* srcFeed:          number,
* configChanged:    bool
* }} VRS_JSON_AIRCRAFTLIST
*/
VRS_JSON_AIRCRAFTLIST;
//endregion

//region VRS_JSON_AIRPORTDATA_THUMBNAIL
/**
 * @typedef {{
 * image:           string,
 * link:            string,
 * photographer:    string
 * }} VRS_JSON_AIRPORTDATA_THUMBNAIL
 */
VRS_JSON_AIRPORTDATA_THUMBNAIL;
//endregion

//region VRS_JSON_AIRPORTDATA_THUMBNAILS
/**
 * @typedef {{
 * status:          number,
 * error:           string=,
 * data:            VRS_JSON_AIRPORTDATA_THUMBNAIL[]=
 * }} VRS_JSON_AIRPORTDATA_THUMBNAILS
 */
VRS_JSON_AIRPORTDATA_THUMBNAILS;
//endregion

//region VRS_JSON_REPORT
/**
 * @typedef {{
 * countRows:               number,
 * groupBy:                 VRS.ReportSortColumn,
 * processingTime:          string,
 * errorText:               string=,
 * silhouettesAvailable:    boolean,
 * operatorFlagsAvailable:  boolean,
 * fromDate:                string,
 * toDate:                  string,
 * aircraftList:            Array.<VRS_JSON_REPORT_AIRCRAFT>,
 * airports:                Array.<VRS_JSON_REPORT_AIRPORT>,
 * routes:                  Array.<VRS_JSON_REPORT_ROUTE>,
 * flights:                 Array.<VRS_JSON_REPORT_FLIGHT>
 * }} VRS_JSON_REPORT
 */
VRS_JSON_REPORT;
//endregion

//region VRS_JSON_REPORT_AIRCRAFT
/**
 * @typedef {{
 * isUnknown:       bool=,
 * acID:            Number,
 * reg:             string=,
 * icao:            string=,
 * acClass:         string=,
 * country:         string=,
 * modeSCountry:    string=,
 * wtc:             VRS.WakeTurbulenceCategory=,
 * engines:         string=,
 * engType:         VRS.EngineType=,
 * species:         VRS.Species=,
 * genericName:     string=,
 * icaoType:        string=,
 * manufacturer:    string=,
 * opFlag:          string=,
 * ownerStatus:     string=,
 * popularName:     string=,
 * previousId:      string=,
 * owner:           string=,
 * serial:          string=,
 * status:          string=,
 * typ:             string=,
 * deregDate:       string=,
 * cofACategory:    string=,
 * cofAExpiry:      string=,
 * curRegDate:      string=,
 * firstRegDate:    string=,
 * infoUrl:         string=,
 * interested:      boolean=,
 * military:        boolean=,
 * mtow:            string=,
 * pictureUrl1:     string=,
 * pictureUrl2:     string=,
 * pictureUrl3:     string=,
 * totalHours:      string=,
 * notes:           string=,
 * yearBuilt:       string=,
 * hasPic:          boolean=,
 * picX:            number=,
 * picY:            number=
 * }} VRS_JSON_REPORT_AIRCRAFT
 */
VRS_JSON_REPORT_AIRCRAFT;
//endregion

//region VRS_JSON_REPORT_AIRPORT
/**
 * Note that the fullName property is not a part of the original JSON. It is added by the code that receives the JSON.
 * It is the code and name joined together.
 *
 * @typedef {{
 * code:        string,
 * name:        string,
 * fullName:    string
 * }} VRS_JSON_REPORT_AIRPORT
 */
VRS_JSON_REPORT_AIRPORT;
//endregion

//region VRS_JSON_REPORT_FLIGHT
/**
 * Note that route and aircraft are not a part of the original JSON. They are added by the code that receives the JSON.
 *
 * @typedef {{
 * row:             Number,
 * acIdx:           Number,
 * call:            string=,
 * rtIdx:           Number=,
 * start:           Date=,
 * end:             Date=,
 * fAlt:            Number=,
 * fSpd:            Number=,
 * fOnGnd:          boolean=,
 * fLat:            Number=,
 * fLng:            Number=,
 * fSqk:            Number=,
 * fTrk:            Number=,
 * fVsi:            Number=,
 * hAlrt:           boolean=,
 * hEmg:            boolean=,
 * hSpi:            boolean=,
 * lAlt:            Number=,
 * lSpd:            Number=,
 * lOnGnd:          boolean=,
 * lLat:            Number=,
 * lLng:            Number=,
 * lSqk:            Number=,
 * lTrk:            Number=,
 * lVsi:            Number=,
 * cADSB:           Number=,
 * cMDS:            Number=,
 * cPOS:            Number=,
 * route:           VRS_JSON_REPORT_ROUTE,
 * aircraft:        VRS_JSON_REPORT_AIRCRAFT
 * }} VRS_JSON_REPORT_FLIGHT
 */
VRS_JSON_REPORT_FLIGHT;
//endregion

//region VRS_JSON_REPORT_ROUTE
/**
 * Note that the from, via and to properties are not a part of the original JSON. They are added by the receiving JavaScript.
 * @typedef {{
 * fIdx:            Number,
 * sIdx:            Array.<Number>,
 * tIdx:            Number,
 * from:            VRS_JSON_REPORT_AIRPORT,
 * via:             Array.<VRS_JSON_REPORT_AIRPORT>,
 * to:              VRS_JSON_REPORT_AIRPORT
 * }} VRS_JSON_REPORT_ROUTE
 */
VRS_JSON_REPORT_ROUTE;
//endregion

//region VRS_JSON_REPORT_TOPLEVEL
/**
 * @typedef {VRS_JSON_REPORT_AIRCRAFT|VRS_JSON_REPORT_FLIGHT} VRS_JSON_REPORT_TOPLEVEL
 */
VRS_JSON_REPORT_TOPLEVEL;
//endregion

//region VRS_LAT_LNG
/**
 * @typedef {{
 * lat: number,
 * lng: number
 * }} VRS_LAT_LNG
 */
VRS_LAT_LNG;
//endregion

//region VRS_LAYOUT_ARRAY, VRS_LAYOUT_SPLITTER_OPTIONS
/**
 * @typedef {
 * Array.<jQuery|VRS_LAYOUT_SPLITTER_OPTIONS|Array.<jQuery|VRS_LAYOUT_SPLITTER_OPTIONS>>
 * } VRS_LAYOUT_ARRAY
 */
VRS_LAYOUT_ARRAY;
/**
 * @typedef {{
 * name:            string,
 * vertical:        bool,
 * savePane:        number,
 * collapsePane:    number=,
 * maxPane:         number=,
 * max:             (number|string)=,
 * startSizePane:   number=,
 * startSize:       (number|string)=
 * }} VRS_LAYOUT_SPLITTER_OPTIONS
 */
VRS_LAYOUT_SPLITTER_OPTIONS;
//endregion

//region VRS_LAYOUT_LABEL
/**
 * @typedef {{
 * name:            string,
 * labelKey:        string
 * }} VRS_LAYOUT_LABEL
 */
VRS_LAYOUT_LABEL;
//endregion

//region VRS_LINK_ELEMENT
/**
 * @typedef {{
 * linkSite:    VRS.LinkSite,
 * aircraft:    VRS.Aircraft,
 * element:     jQuery
 * }} VRS_LINK_ELEMENT
 */
VRS_LINK_ELEMENT;
//endregion

//region VRS_LOADSCRIPT_OPTIONS
/**
 * @typedef {{
 * key:         string=,
 * url:         string,
 * params:      object=,
 * async:       boolean=,
 * queue:       boolean=,
 * success:     function()=,
 * error:       function(jQuery.jqXHR, string, string)=
 * }} VRS_LOADSCRIPT_OPTIONS
 */
VRS_LOADSCRIPT_OPTIONS;
//endregion

//region VRS_MAP_CONTROL
/**
 * @typedef {{
 * control:     jQuery,
 * position:    VRS.MapPosition
 * }} VRS_MAP_CONTROL
 */
VRS_MAP_CONTROL;
//endregion

//region VRS_MENU_ELEMENTS
/**
 * @typedef {{
 * listItem:    jQuery,
 * image:       jQuery=,
 * link:        jQuery,
 * text:        jQuery
 * }} VRS_MENU_ELEMENTS
 */
VRS_MENU_ELEMENTS;
//endregion

//region VRS_OPTIONS_AIRCRAFTDETAIL
/**
 * @typedef {{
 * name:                    string,
 * aircraftList:            VRS.AircraftList=,
 * unitDisplayPreferences:  VRS.UnitDisplayPreferences=,
 * aircraftAutoSelect:      VRS.AircraftAutoSelect=,
 * mapPlugin:               VRS.vrsMap=,
 * useSavedState:           bool,
 * showUnits:               bool,
 * items:                   VRS.RenderProperty[],
 * showSeparateRouteLink:   bool,
 * flagUncertainCallsigns:  bool,
 * distinguishOnGround:     bool,
 * mirrorMapJQ:             jQuery,
 * plotterOptions:          VRS.AircraftPlotterOptions
 * }} VRS_OPTIONS_AIRCRAFTDETAIL
 */
VRS_OPTIONS_AIRCRAFTDETAIL;
//endregion

//region VRS_OPTIONS_AIRCRAFTFILTER
/**
* @typedef {{
* caseInsensitive: bool
* }} VRS_OPTIONS_AIRCRAFTFILTER
*/
VRS_OPTIONS_AIRCRAFTFILTER;
//endregion

//region VRS_OPTIONS_AIRCRAFTINFOWINDOW
/**
 * @typedef {{
 * name:                    String,
 * aircraftList:            VRS.AircraftList,
 * aircraftPlotter:         VRS.AircraftPlotter,
 * unitDisplayPreferences:  VRS.UnitDisplayPreferences,
 * enabled:                 boolean,
 * useStateOnOpen:          boolean,
 * items:                   VRS.RenderProperty[],
 * showUnits:               boolean,
 * flagUncertainCallsigns:  boolean,
 * distinguishOnGround:     boolean,
 * enablePanning:           boolean
 * }} VRS_OPTIONS_AIRCRAFTINFOWINDOW
 */
VRS_OPTIONS_AIRCRAFTINFOWINDOW;
//endregion

//region VRS_OPTIONS_AIRCRAFTLINKS
/**
 * @typedef {{
 * linkSites:   Array.<VRS.LinkSite|VRS.LinkRenderHandler>
 * }} VRS_OPTIONS_AIRCRAFTLINKS
 */
VRS_OPTIONS_AIRCRAFTLINKS;
//endregion

//region VRS_OPTIONS_AIRCRAFTLIST
/**
 * @typedef {{
 * name:                        string,
 * aircraftList:                VRS.AircraftList,
 * aircraftListFetcher:         VRS.AircraftListFetcher,
 * unitDisplayPreferences:      VRS.UnitDisplayPreferences,
 * sorter:                      VRS.AircraftListSorter,
 * showSorterOptions:           bool,
 * columns:                     VRS.RenderProperty[],
 * useSavedState:               bool,
 * useSorterSavedState:         bool,
 * showUnits:                   bool,
 * distinguishOnGround:         bool,
 * flagUncertainCallsigns:      bool,
 * showPause:                   bool,
 * showHideAircraftNotOnMap:    bool
 * }} VRS_OPTIONS_AIRCRAFTLIST
 */
VRS_OPTIONS_AIRCRAFTLIST
//endregion

//region VRS_OPTIONS_AIRCRAFTPOSITIONMAP
/**
 * @typedef {{
 * plotterOptions:              VRS.AircraftPlotterOptions,
 * mirrorMapJQ:                 jQuery,
 * stateName:                   string,
 * mapOptionOverrides:          Object,
 * unitDisplayPreferences:      VRS.UnitDisplayPreferences,
 * autoHideNoPosition:          boolean,
 * reflectMapTypeBackToMirror:  jQuery
 * }} VRS_OPTIONS_AIRCRAFTPOSITIONMAP
 */
VRS_OPTIONS_AIRCRAFTPOSITIONMAP;
//endregion

//region VRS_OPTIONS_AIRCRAFTRENDER
/**
* @typedef {{
* distinguishOnGround:              bool,
* flagUncertainCallsigns:           bool,
* showUnits:                        bool,
* suppressRouteCorrectionLinks:     bool,
* unitDisplayPreferences:           VRS.UnitDisplayPreferences,
* airportDataThumbnails:            number
* }} VRS_OPTIONS_AIRCRAFTRENDER
*/
VRS_OPTIONS_AIRCRAFTRENDER;
//endregion

//region VRS_OPTIONS_CIRCLE
/**
 * -- colours are CSS colours.
 * -- radius is in metres.
 * -- opacity is a value from 0 (transparent) to 1 (fully opaque).
 * -- weights are in pixels.
 * @typedef {{
 * center:          VRS_LAT_LNG,
 * clickable:       boolean,
 * draggable:       boolean,
 * editable:        boolean,
 * fillColor:       string,
 * fillOpacity:     Number,
 * radius:          Number,
 * strokeColor:     string,
 * strokeOpacity:   Number,
 * strokeWeight:    Number,
 * visible:         boolean,
 * zIndex:          boolean
 * }} VRS_OPTIONS_CIRCLE
 */
VRS_OPTIONS_CIRCLE;
//endregion

//region VRS_OPTION_FIELD_SETTINGS
/**
 * @typedef {{
 * field:               VRS.OptionField,
 * fieldParentJQ:       jQuery,
 * optionPageParent:    VRS.OptionsPageParent
 * }} VRS_OPTION_FIELD_SETTINGS
 */
VRS_OPTION_FIELD_SETTINGS;
//endregion

//region VRS_OPTIONS_INFOWINDOW
/**
 * @typedef {{
 * content:         HTMLElement,
 * disableAutoPan:  boolean,
 * maxWidth:        Number,
 * pixelOffset:     Number,
 * position:        VRS_LAT_LNG,
 * zIndex:          Number
 * }} VRS_OPTIONS_INFOWINDOW
 */
VRS_OPTIONS_INFOWINDOW;
//endregion

//region VRS_OPTIONS_MAP
/**
 * @typedef {{
 * key:                 string,
 * version:             string,
 * sensor:              bool,
 * libraries:           string[],
 * loadMarkerWithLabel  bool,
 * openOnCreate:        bool,
 * waitUntilReady:      bool,
 * zoom:                number,
 * center:              VRS_LAT_LNG,
 * showMapTypeControl:  bool,
 * mapTypeId:           VRS.MapType,
 * streetViewControl:   bool,
 * scrollwheel:         bool,
 * scaleControl:        bool,
 * draggable:           bool,
 * controlStyle:        bool,
 * controlPosition:     VRS.MapPosition,
 * pointsOfInterest:    bool,
 * showHighContrast:    bool,
 * mapControls:         VRS_MAP_CONTROL[],
 * afterCreate:         function(VRS.vrsMap),
 * afterOpen:           function(VRS.vrsMap),
 * name:                string,
 * useStateOnOpen:      bool,
 * autoSaveState:       bool,
 * useServerDefaults:   bool
 * }} VRS_OPTIONS_MAP
 */
VRS_OPTIONS_MAP;
//endregion

//region VRS_OPTIONS_MAPNEXTPAGEBUTTON
/**
 * @typedef {{
 * nextPageName:        string,
 * aircraftListFilter:  VRS.AircraftListFilter,
 * aircraftListFetcher: VRS.AircraftListFetcher
 * }} VRS_OPTIONS_MAPNEXTPAGEBUTTON
 */
VRS_OPTIONS_MAPNEXTPAGEBUTTON
//endregion

//region VRS_OPTIONS_MENU
/**
 * @typedef {{
 * menu:                    VRS.Menu,
 * showButtonTrigger:       boolean,
 * triggerElement:          jQuery,
 * menuContainerClasses:    string,
 * offsetX:                 number,
 * offsetY:                 number,
 * alignment:               VRS.Alignment,
 * cssMenuWidth:            number,
 * zIndex:                  number
 * }} VRS_OPTIONS_MENU
 */
VRS_OPTIONS_MENU;
//endregion

//region VRS_OPTIONS_OPTIONDIALOG
/**
 * @typedef {{
 * pages:       VRS.OptionPage[],
 * autoRemove:  bool
 * }} VRS_OPTIONS_OPTIONDIALOG
 */
VRS_OPTIONS_OPTIONDIALOG;
//endregion

//region VRS_OPTIONS_OPTIONFIELDBUTTON
/**
 * @typedef {{
 * field:               VRS.OptionFieldButton,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDBUTTON
 */
VRS_OPTIONS_OPTIONFIELDBUTTON;
//endregion

//region VRS_OPTIONS_OPTIONFIELDCHECKBOX
/**
 * @typedef {{
 * field:               VRS.OptionFieldCheckBox,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDCHECKBOX
 */
VRS_OPTIONS_OPTIONFIELDCHECKBOX;
//endregion

//region VRS_OPTIONS_OPTIONFIELDCOLOUR
/**
 * @typedef {{
 * field:               VRS.OptionFieldColour,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDCOLOUR
 */
VRS_OPTIONS_OPTIONFIELDCOLOUR;
//endregion

//region VRS_OPTIONS_OPTIONFIELDCOMBOBOX
/**
 * @typedef {{
 * field:               VRS.OptionFieldComboBox,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDCOMBOBOX
 */
VRS_OPTIONS_OPTIONFIELDCOMBOBOX;
//endregion

//region VRS_OPTIONS_OPTIONFIELDDATE
/**
 * @typedef {{
 * field:               VRS.OptionFieldDate,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDDATE
 */
VRS_OPTIONS_OPTIONFIELDDATE;
//endregion

//region VRS_OPTIONS_OPTIONFIELDLABEL
/**
 * @typedef {{
 * field:               VRS.OptionFieldLabel,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDLABEL
 */
VRS_OPTIONS_OPTIONFIELDLABEL;
//endregion

//region VRS_OPTIONS_OPTIONFIELDLINKLABEL
/**
 * @typedef {{
 * field:               VRS.OptionFieldLinkLabel,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDLINKLABEL
 */
VRS_OPTIONS_OPTIONFIELDLINKLABEL;
//endregion

//region VRS_OPTIONS_OPTIONFIELDNUMERIC
/**
 * @typedef {{
 * field:               VRS.OptionFieldNumeric,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDNUMERIC
 */
VRS_OPTIONS_OPTIONFIELDNUMERIC;
//endregion

//region VRS_OPTIONS_OPTIONFIELDORDEREDSUBSET
/**
 * @typedef {{
 * field:               VRS.OptionFieldOrderedSubset,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDORDEREDSUBSET
 */
VRS_OPTIONS_OPTIONFIELDORDEREDSUBSET;
//endregion

//region VRS_OPTIONS_OPTIONFIELDPANELIST
/**
 * @typedef {{
 * field:               VRS.OptionFieldPaneList,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDPANELIST
 */
VRS_OPTIONS_OPTIONFIELDPANELIST;
//endregion

//region VRS_OPTIONS_OPTIONFIELDRADIOBUTTON
/**
 * @typedef {{
 * field:               VRS.OptionFieldRadioButton,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDRADIOBUTTON
 */
VRS_OPTIONS_OPTIONFIELDRADIOBUTTON;
//endregion

//region VRS_OPTIONS_OPTIONFIELDTEXTBOX
/**
 * @typedef {{
 * field:               VRS.OptionFieldTextBox,
 * optionPageParent:    VRS.OptionPageParent
 * }} VRS_OPTIONS_OPTIONFIELDTEXTBOX
 */
VRS_OPTIONS_OPTIONFIELDTEXTBOX;
//endregion

//region VRS_OPTIONS_OPTIONFORM
/**
 * @typedef {{
 * pages:           VRS.OptionPage[],
 * showInAccordion: boolean
 * }} VRS_OPTIONS_OPTIONFORM
 */
VRS_OPTIONS_OPTIONFORM
//endregion

//region VRS_OPTIONS_OPTIONPANE
/**
 * @typedef {{
 * optionPane:          VRS.OptionPane,
 * optionPageParent:    VRS.OptionPageParent,
 * isInStack:           bool
 * }} VRS_OPTIONS_OPTIONPANE
 */
VRS_OPTIONS_OPTIONPANE;
//endregion

//region VRS_OPTIONS_PAGEPANEL
/**
 * @typedef {{
 * element:                 jQuery,
 * previousPageName:        string,
 * previousPageLabelKey:    string|function():string,
 * nextPageName:            string,
 * nextPageLabelKey:        string|function():string,
 * titleLabelKey:           string|function():string,
 * headerMenu:              VRS.Menu,
 * showFooterGap:           boolean
 * }} VRS_OPTIONS_PAGEPANEL
 */
VRS_OPTIONS_PAGEPANEL;
//endregion

//region VRS_OPTIONS_POLYGON
/**
 * @typedef {{
 * clickable:               boolean=,
 * draggable:               boolean=,
 * editable:                boolean=,
 * fillColour:              string=,
 * fillOpacity:             number=,
 * geodesic:                boolean=,
 * paths:                   VRS_LAT_LNG[][],
 * strokeColour:            string=,
 * strokeOpacity:           number=,
 * strokeWeight:            number=,
 * visible:                 boolean=,
 * zIndex:                  number=,
 * tag:                     *
  * }} VRS_OPTIONS_POLYGON
 */
VRS_OPTIONS_POLYGON;
//endregion

//region VRS_OPTIONS_POLYLINE
/**
 * -- strokeColour     is a CSS string (e.g. #000000)
 * -- strokeOpacity    is a value from 0.0 to 1.0
 * -- strokeWeight     is in pixels
 * @typedef {{
 * strokeColour:    string=,
 * strokeOpacity:   number=,
 * strokeWeight:    number=
 * }} VRS_OPTIONS_POLYLINE
 */
VRS_OPTIONS_POLYLINE;
//endregion

//region VRS_OPTIONS_REPORTDETAIL
/**
 * @typedef {{
 * name:                    string,
 * report:                  VRS.Report,
 * unitDisplayPreferences:  VRS.UnitDisplayPreferences,
 * plotterOptions:          VRS.AircraftPlotterOptions,
 * columns:                 VRS_REPORT_PROPERTY[],
 * useSavedState:           boolean,
 * showUnits:               boolean,
 * showEmptyValues:         boolean,
 * distinguishOnGround:     boolean
 * }} VRS_OPTIONS_REPORTDETAIL
 */
VRS_OPTIONS_REPORTDETAIL;
//endregion

//region VRS_OPTIONS_REPORTLIST
/**
 * @typedef {{
 * name:                    string,
 * report:                  VRS.Report,
 * unitDisplayPreferences:  VRS.UnitDisplayPreferences,
 * singleAircraftColumns:   VRS_REPORT_PROPERTY[],
 * manyAircraftColumns:     VRS_REPORT_PROPERTY[],
 * useSavedState:           boolean,
 * showUnits:               boolean,
 * distinguishOnGround:     boolean,
 * showPagerTop:            boolean,
 * showPagerBottom:         boolean,
 * groupBySortColumn:       boolean,
 * groupResetAlternateRows: boolean,
 * justShowStartTime:       boolean,
 * alwaysShowEndDate:       boolean
 * }} VRS_OPTIONS_REPORTLIST
 */
VRS_OPTIONS_REPORTLIST;
//endregion

//region VRS_OPTIONS_REPORTMAP
/**
 * @typedef {{
 * name:                    string,
 * report:                  VRS.Report,
 * plotterOptions:          VRS.AircraftPlotterOptions,
 * elementClasses:          string,
 * unitDisplayPreferences:  VRS.UnitDisplayPreferences,
 * mapOptionOverrides:      Object,
 * mapControls:             VRS_MAP_CONTROL[],
 * scrollToAircraft:        boolean,
 * showPath:                boolean,
 * startSelected:           boolean
 * }} VRS_OPTIONS_REPORTMAP
 */
VRS_OPTIONS_REPORTMAP;
//endregion

//region VRS_OPTIONS_REPORTPAGER
/**
 * @typedef {{
 * name:                    string,
 * report:                  VRS.Report,
 * allowPageSizeChange:     boolean,
 * allowShowAllRows:        boolean
 * }} VRS_OPTIONS_REPORTPAGER
 */
VRS_OPTIONS_REPORTPAGER;
//endregion

//region VRS_OPTIONS_SELECTDIALOG
/**
 * @typedef {{
 * items:       VRS.ValueText[]|function():VRS.ValueText[],
 * value:       *,
 * autoOpen:    bool,
 * onSelect:    function(*),
 * titleKey:    string,
 * minWidth:    number,
 * minHeight:   number,
 * onClose:     function(),
 * modal:       bool,
 * lines:       number
 * }} VRS_OPTIONS_SELECTDIALOG
 */
VRS_OPTIONS_SELECTDIALOG;
//endregion

//region VRS_POINT
/**
 * @typedef {{
 * x: number,
 * y: number
 * }} VRS_POINT
 */
VRS_POINT;
//endregion

//region VRS_POLAR_PLOT
/**
 * @typedef {{
 * lat:         number,
 * lng:         number
 * }} VRS_POLAR_PLOT
 */
VRS_POLAR_PLOT;
//endregion

//region VRS_POLAR_PLOT_CONFIG
/**
 * @typedef {{
 * low:         number,
 * high:        number,
 * colour:      string,
 * zIndex:      number
 * }} VRS_POLAR_PLOT_CONFIG
 */
VRS_POLAR_PLOT_CONFIG;
//endregion

//region VRS_POLAR_PLOT_SLICE_ABSTRACT
/**
 * @typedef {{
 * feedName:    string,
 * low:         number,
 * high:        number
 * }} VRS_POLAR_PLOT_SLICE_ABSTRACT
 */
VRS_POLAR_PLOT_SLICE_ABSTRACT;
//endregion

//region VRS_POLAR_PLOT_DISPLAY
/**
 * @typedef {{
 * feedId:  number,
 * slice:   VRS_POLAR_PLOTS_SLICE
 * }} VRS_POLAR_PLOT_DISPLAY
 */
VRS_POLAR_PLOT_DISPLAY;
//endregion

//region VRS_POLAR_PLOT_ID
/**
 * @typedef {{
 * feedId:      number,
 * lowAlt:      number,
 * highAlt:     number,
 * colour:      string=
 * }} VRS_POLAR_PLOT_ID
 */
VRS_POLAR_PLOT_ID;
//endregion

//region VRS_POLAR_PLOTS
/**
 * @typedef {{
 * feedId:      number,
 * slices:      VRS_POLAR_PLOTS_SLICE[]
 * }} VRS_POLAR_PLOTS
 */
VRS_POLAR_PLOTS;
//endregion

//region VRS_POLAR_PLOTS_SLICE
/**
 * @typedef {{
 * lowAlt:      number,
 * highAlt:     number,
 * plots:       VRS_POLAR_PLOT[]
 * }} VRS_POLAR_PLOTS_SLICE
 */
VRS_POLAR_PLOTS_SLICE;
//endregion

//region VRS_RECEIVER
/**
* @typedef {{
* id:           number,
* name:         string,
* polarPlot:    boolean
* }} VRS_RECEIVER
*/
VRS_RECEIVER;
//endregion

//region VRS_REPORT_PAGE_METRICS
/**
 * @typedef {{
 * isPaged:         boolean,
 * pageSize:        number,
 * currentPage:     number,
 * totalPages:      number,
 * onFirstPage:     boolean,
 * onLastPage:      boolean,
 * pageFirstRow:    number,
 * pageLastRow:     number
 * }} VRS_REPORT_PAGE_METRICS
 */
VRS_REPORT_PAGE_METRICS;
//endregion

//region VRS_REPORT_PROPERTY
/**
 * @typedef {VRS.ReportAircraftProperty|VRS.ReportFlightProperty} VRS_REPORT_PROPERTY
 */
VRS_REPORT_PROPERTY;
//endregion

//region VRS_SERIALISED_FILTER
/**
* @typedef {{
* property:         *,
* valueCondition:   (VRS_SERIALISED_ONEVALUECONDITION|VRS_SERIALISED_TWOVALUECONDITION)
* }} VRS_SERIALISED_FILTER
*/
VRS_SERIALISED_FILTER;
//endregion

//region VRS_SERIALISED_ONEVALUECONDITION
/**
* @typedef {{
* condition:    VRS.FilterCondition,
* reversed:     bool,
* value:        *
* }} VRS_SERIALISED_ONEVALUECONDITION
*/
VRS_SERIALISED_ONEVALUECONDITION;
//endregion

//region VRS_SERIALISED_TWOVALUECONDITION
/**
* @typedef {{
* condition:    VRS.FilterCondition,
* reversed:     bool,
* value1:       *,
* value2:       *
* }} VRS_SERIALISED_TWOVALUECONDITION
*/
VRS_SERIALISED_TWOVALUECONDITION;
//endregion

//region VRS_SERVER_CONFIG
/**
 * @typedef {{
 * UniqueId: number,
 * Name:     string
 * }} VRS_SERVER_CONFIG_RECEIVER
 */
VRS_SERVER_CONFIG_RECEIVER;

/**
 * @typedef {{
 * VrsVersion:                              string,
 * IsMono:                                  bool,
 * Receivers:                               Array.<VRS_SERVER_CONFIG_RECEIVER>,
 * IsLocalAddress:                          bool,
 * IsAudioEnabled:                          bool,
 * MinimumRefreshSeconds:                   number,
 * RefreshSeconds:                          number,
 * InitialLatitude:                         number,
 * InitialLongitude:                        number,
 * InitialMapType:                          VRS.MapType,
 * InitialZoom:                             number,
 * InitialDistanceUnit:                     VRS.Distance,
 * InitialHeightUnit:                       VRS.Height,
 * InitialSpeedUnit:                        VRS.Speed,
 * InternetClientCanRunReports:             bool,
 * InternetClientCanShowPinText:            bool,
 * InternetClientTimeoutMinutes:            number,
 * InternetClientsCanPlayAudio:             bool,
 * InternetClientsCanSubmitRoutes:          bool,
 * InternetClientsCanSeeAircraftPictures:   bool,
 * InternetClientsCanSeePolarPlots:         bool
 * }} VRS_SERVER_CONFIG
 */
VRS_SERVER_CONFIG;
//endregion

//region VRS_SETTINGS_POLAR_PLOTTER
/**
 * @typedef {{
 * name:                    string,
 * map:                     VRS.vrsMap,
 * aircraftListFetcher:     VRS.AircraftListFetcher,
 * autoSaveState:           boolean
 * }} VRS_SETTINGS_POLAR_PLOTTER
 */
VRS_SETTINGS_POLAR_PLOTTER;
//endregion

//region VRS_SIZE
/**
 * @typedef {{
* width:    number,
* height:   number
* }} VRS_SIZE
 */
VRS_SIZE;
//endregion

//region VRS_SORTFIELD
/**
* @typedef {{
* field: VRS.AircraftListSortableField,
* ascending: bool
* }} VRS_SORTFIELD
*/
VRS_SORTFIELD;
//endregion

//region VRS_SORTREPORT
/**
 * @typedef {{
 * field:       VRS.ReportSortColumn,
 * ascending:   bool
 * }} VRS_SORTREPORT
 */
VRS_SORTREPORT;
//endregion

//region VRS_SPLITTER_BAR_MOVED_ARGS
/**
 * @typedef {{
 * splitterElement:     VRS.vrsSplitter,
 * pane1Length:         number,
 * pane2Length:         number,
 * barLength:           number
 * }} VRS_SPLITTER_BAR_MOVED_ARGS
 */
VRS_SPLITTER_BAR_MOVED_ARGS;
//endregion

//region VRS_SPLITTER_PERSIST_DETAIL
/**
 * @typedef {{
 * splitter:            VRS.vrsSplitter,
 * barMovedHookResult:  object
 * }} VRS_SPLITTER_PERSIST_DETAIL
 */
VRS_SPLITTER_PERSIST_DETAIL;
//endregion

//region VRS_STATE_AIRCRAFTAUTOSELECT
/**
* @typedef {{
*  enabled:             bool,
*  selectClosest:       bool,
*  offRadarAction:      VRS.OffRadarAction,
*  filters:             Array.<VRS_SERIALISED_FILTER>
* }} VRS_STATE_AIRCRAFTAUTOSELECT
*/
VRS_STATE_AIRCRAFTAUTOSELECT;
//endregion

//region VRS_STATE_AIRCRAFTDETAIL
/**
 * @typedef {{
* showUnits: bool,
* items: Array.<VRS.RenderProperty>
* }} VRS_STATE_AIRCRAFTDETAIL
 */
VRS_STATE_AIRCRAFTDETAIL;
//endregion

//region VRS_STATE_AIRCRAFTINFOWINDOW
/**
 * @typedef {{
 * enabled:             boolean,
 * items:               VRS.RenderProperty[],
 * showUnits:           boolean
 * }} VRS_STATE_AIRCRAFTINFOWINDOW
 */
VRS_STATE_AIRCRAFTINFOWINDOW;
//endregion

//region VRS_STATE_AIRCRAFTLISTFETCHER
/**
* @typedef {{
* interval: number,
* requestFeedId: number
* }} VRS_STATE_AIRCRAFTLISTFETCHER
*/
VRS_STATE_AIRCRAFTLISTFETCHER;
//endregion

//region VRS_STATE_AIRCRAFTLISTFILTER
/**
* @typedef {{
* filters:              Array.<VRS_SERIALISED_FILTER>,
* enabled:              boolean
* }} VRS_STATE_AIRCRAFTLISTFILTER
*/
VRS_STATE_AIRCRAFTLISTFILTER;
//endregion

//region VRS_STATE_AIRCRAFTLISTSORTER
/**
* @typedef {{
* sortFields: VRS_SORTFIELD[]
* }} VRS_STATE_AIRCRAFTLISTSORTER
*/
//endregion

//region VRS_STATE_AIRCRAFTLISTPLOTTER
/**
* @typedef {{
* showAltitudeStalk:                    boolean,
* suppressAltitudeStalkWhenZoomedOut:   boolean,
* showPinText:                          boolean,
* pinTexts:                             VRS.RenderProperty[],
* pinTextLines:                         number,
* hideEmptyPinTextLines:                boolean,
* trailDisplay:                         VRS.TrailDisplay,
* trailType:                            VRS.TrailType,
* showRangeCircles:                     boolean,
* rangeCircleInterval:                  number,
* rangeCircleDistanceUnit:              VRS.Distance,
* rangeCircleCount:                     number,
* rangeCircleOddColour:                 string,
* rangeCircleOddWeight:                 number,
* rangeCircleEvenColour:                string,
* rangeCircleEvenWeight:                number,
* showCurrentLocation:                  boolean
* }} VRS_STATE_AIRCRAFTLISTPLOTTER
*/
VRS_STATE_AIRCRAFTLISTPLOTTER;
//endregion

//region VRS_STATE_AIRCRAFTLISTPLUGIN
/**
* @typedef {{
* columns:      Array.<VRS.RenderProperty>,
* showUnits:    bool
* }} VRS_STATE_AIRCRAFTLISTPLUGIN
*/
VRS_STATE_AIRCRAFTLISTPLUGIN;
//endregion

//region VRS_STATE_AUDIO
/**
 * @typedef {{
 * announceSelected:            Boolean,
 * announceOnlyAutoSelected:    Boolean,
 * volume:                      Number
 * }} VRS_STATE_AUDIO
 */
VRS_STATE_AUDIO;
//endregion

//region VRS_STATE_CURRENTLOCATION
/**
 * @typedef {{
 * userSuppliedLocation:    VRS_LAT_LNG,
 * useBrowserLocation:      boolean,
 * showCurrentLocation:     boolean
 * }} VRS_STATE_CURRENTLOCATION
 */
VRS_STATE_CURRENTLOCATION;
//endregion

//region VRS_STATE_LAYOUTMANAGER
/**
* @typedef {{
* currentLayout: string
* }} VRS_STATE_LAYOUTMANAGER
*/
VRS_STATE_LAYOUTMANAGER;
//endregion

//region VRS_STATE_MAP_PLUGIN
/**
 * @typedef {{
 * zoom:        number,
 * mapTypeId:   VRS.MapType,
 * center:      VRS_LAT_LNG
 * }} VRS_STATE_MAP_PLUGIN
 */
VRS_STATE_MAP_PLUGIN;
//endregion

//region VRS_STATE_POLARPLOTTER
/**
 * @typedef {{
 * altitudeRangeConfigs:    VRS_POLAR_PLOT_CONFIG[],
 * plotsOnDisplay:          VRS_POLAR_PLOT_SLICE_ABSTRACT[],
 * strokeOpacity:           number,
 * fillOpacity:             number
 * }} VRS_STATE_POLARPLOTTER
 */
VRS_STATE_POLARPLOTTER;
//endregion

//region VRS_STATE_REPORT
/**
 * @typedef {{
 * pageSize:    number
 * }} VRS_STATE_REPORT
 */
VRS_STATE_REPORT;
//endregion

//region VRS_STATE_REPORTCRITERIA
/**
 * @typedef {{
 * reportFindAllPermutationsOfCallsign:   boolean
 * }} VRS_STATE_REPORTCRITERIA
 */
VRS_STATE_REPORTCRITERIA;
//endregion

//region VRS_STATE_REPORTDETAILPANEL
/**
 * @typedef {{
 * columns:         Array.<VRS_REPORT_PROPERTY>,
 * showUnits:       boolean,
 * showEmptyValues: boolean
 * }} VRS_STATE_REPORTDETAILPANEL
 */
VRS_STATE_REPORTDETAILPANEL;
//endregion

//region VRS_STATE_REPORTLIST
/**
 * @typedef {{
 * singleAircraftColumns:   Array.<VRS_REPORT_PROPERTY>,
 * manyAircraftColumns:     Array.<VRS_REPORT_PROPERTY>,
 * showUnits:               boolean
 * }} VRS_STATE_REPORTLIST
 */
VRS_STATE_REPORTLIST;
//endregion

//region VRS_STATE_SPLITTER, VRS_STATE_SPLITTER_GROUP
/**
 * @typedef {{
 * name:        string,
 * pane:        number,
 * vertical:    bool,
 * length:      number
 * }} VRS_STATE_SPLITTER
 */
VRS_STATE_SPLITTER;

/**
 * @typedef {{
 * lengths:     Array.<VRS_STATE_SPLITTER>
 * }} VRS_STATE_SPLITTER_GROUP
 */
VRS_STATE_SPLITTER_GROUP;
//endregion

//region VRS_STATE_UNITDISPLAYPREFERENCES
/**
* @typedef {{
* distanceUnit:       VRS.Distance,
* heightUnit:         VRS.Height,
* speedUnit:          VRS.Speed,
* vsiPerSecond:       Boolean,
* flTransitionAlt:    Number,
* flTransitionUnit:   VRS.Height,
* flHeightUnit:       VRS.Height,
* showAltType:        Boolean
* }} VRS_STATE_UNITDISPLAYPREFERENCES
*/
VRS_STATE_UNITDISPLAYPREFERENCES;
//endregion

//region VRS_TIME
/**
 * @typedef {{
 * hours:   number,
 * minutes: number,
 * seconds: number
 * }} VRS_TIME
 */
VRS_TIME;
//endregion

//region VRS_VALUE_PERCENT
/**
 * @typedef {{
 * value:       number,
 * isPercent:   bool
 * }} VRS_VALUE_PERCENT
 */
VRS_VALUE_PERCENT;
//endregion