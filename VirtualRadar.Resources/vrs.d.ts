declare namespace VRS {
    interface CultureInfo_Settings {
        language: string;
        forceCultureName?: string;
        flagImage?: string;
        countryFlag?: string;
        flagSize?: ISize;
        englishName: string;
        nativeName?: string;
        topLevel?: boolean;
        groupLanguage?: string;
    }
    class CultureInfo {
        private _Locale;
        private _CultureName;
        private _Language;
        private _FlagImage;
        private _FlagSize;
        private _EnglishName;
        private _NativeName;
        private _TopLevel;
        private _GroupLanguage;
        constructor(locale: string, settings: CultureInfo_Settings);
        locale: string;
        cultureName: string;
        language: string;
        flagImage: string;
        flagSize: ISize;
        englishName: string;
        nativeName: string;
        topLevel: boolean;
        groupLanguage: string;
        getFlagImageHtml(): string;
    }
    interface Localise_SaveState {
        locale?: string;
    }
    class Localise {
        private _Dispatcher;
        private _Events;
        private _LoadedLanguage;
        private _CultureInfos;
        private _LoadedGlobalizations;
        private _Locale;
        getLocale(): string;
        setLocale(value: string, successCallback: () => void): void;
        hookLocaleChanged(callback: () => void, forceThis?: Object): IEventHandle;
        unhook(hookResult: IEventHandle): void;
        saveState(): void;
        loadState(): Localise_SaveState;
        applyState(config: Localise_SaveState, successCallback: () => void): void;
        loadAndApplyState(successCallback: () => void): void;
        private createSettings();
        private loadLanguage(language, successCallback);
        private loadCulture(cultureName, successCallback);
        private guessBrowserLocale();
        addCultureInfo(cultureName: string, settings: CultureInfo_Settings): void;
        getCultureInfo(cultureName?: string): CultureInfo;
        removeCultureInfo(cultureName: string): void;
        getCultureInfos(): CultureInfo[];
        getCultureInfosGroupedByLanguage(sortByNativeName: boolean): CultureInfo[][];
        private getRawGlobalizeData();
        getText(keyOrFormatFunction: string | VoidFuncReturning<string>): any;
        localiseDatePicker(datePickerJQ: JQuery): void;
        getDatePickerOptions(): JQueryUI.DatepickerOptions;
        private dotNetDateFormatToJQueryDateFormat(dateFormat);
        setLocaleInBackground(locale: string, showModalWait?: boolean, localeLoadedCallback?: () => void): void;
    }
    var globalisation: Localise;
}
interface JQueryUICustomWidget_Options {
    disabled?: boolean;
    hide?: boolean;
    show?: boolean;
}
declare class JQueryUICustomWidget {
    protected defaultElement: string;
    protected document: JQuery;
    protected element: JQuery;
    protected namespace: string;
    protected uuid: string;
    protected version: string;
    protected widgetEventPrefix: string;
    protected widgetFullName: string;
    protected widgetName: string;
    protected window: JQuery;
    constructor();
    protected _delay(callback: Function, milliseconds?: number): number;
    protected _focusable(element: JQuery): JQuery;
    protected _getCreateEventData(): Object;
    protected _getCreateOptions(): Object;
    protected _hide(element: JQuery, option: Object, callback?: () => void): JQuery;
    protected _hoverable(element: JQuery): JQuery;
    protected _off(element: JQuery, eventName: string): JQuery;
    protected _on(...args: any[]): JQuery;
    protected _setOptions<T>(options: T): JQuery;
    protected _show(element: JQuery, option: Object, callback?: () => void): JQuery;
    protected _super(...args: any[]): JQuery;
    protected _superApply(args: any[]): JQuery;
    protected _trigger(triggerType: string, event?: Event, data?: Object): boolean;
    destroy(): JQuery;
    disable(): JQuery;
    enable(): JQuery;
    instance(): Object;
    option(...args: any[]): Object;
    widget(): JQuery;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftDetailPlugin_Options {
        name?: string;
        aircraftList?: AircraftList;
        unitDisplayPreferences: UnitDisplayPreferences;
        aircraftAutoSelect?: AircraftAutoSelect;
        mapPlugin?: IMap;
        useSavedState?: boolean;
        showUnits?: boolean;
        items?: RenderPropertyEnum[];
        showSeparateRouteLink?: boolean;
        flagUncertainCallsigns?: boolean;
        distinguishOnGround?: boolean;
        mirrorMapJQ?: JQuery;
        plotterOptions?: AircraftPlotterOptions;
        airportDataThumbnails?: number;
    }
    interface AircraftDetailPlugin_SaveState {
        showUnits: boolean;
        items: RenderPropertyEnum[];
    }
    var jQueryUIHelper: JQueryUIHelper;
    class AircraftDetailPlugin extends JQueryUICustomWidget implements ISelfPersist<AircraftDetailPlugin_SaveState> {
        options: AircraftDetailPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _destroyProperties(surface, properties);
        suspend(onOff: boolean): void;
        private _suspendWidgets(state, onOff);
        private _suspendWidgetProperties(state, onOff, properties, surface);
        saveState(): void;
        loadState(): AircraftDetailPlugin_SaveState;
        applyState(settings: AircraftDetailPlugin_SaveState): void;
        loadAndApplyState(): void;
        private _persistenceKey();
        private _createSettings();
        createOptionPane(displayOrder: number): OptionPane[];
        private _buildContent(state);
        private _buildHeader(state);
        private _buildBody(state);
        private _buildLinks(state);
        private _renderContent(state, aircraft, refreshAll, displayUnit?);
        private _renderProperties(state, aircraft, refreshAll, properties, surface, displayUnit?);
        private _renderLinks(state, aircraft, refreshAll);
        private _reRenderAircraft(state);
        private _aircraftListUpdated();
        private _displayUnitChanged(displayUnit);
        private _selectedAircraftChanged();
        private _localeChanged();
    }
}
interface JQuery {
    vrsAircraftDetail(): any;
    vrsAircraftDetail(options: VRS.AircraftDetailPlugin_Options): any;
    vrsAircraftDetail(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftInfoWindowPlugin_Options {
        name?: string;
        aircraftList?: AircraftList;
        aircraftPlotter?: AircraftPlotter;
        unitDisplayPreferences: UnitDisplayPreferences;
        enabled?: boolean;
        useStateOnOpen?: boolean;
        items?: RenderPropertyEnum[];
        showUnits?: boolean;
        flagUncertainCallsigns?: boolean;
        distinguishOnGround?: boolean;
        enablePanning?: boolean;
    }
    interface AircraftInfoWindowPlugin_SaveState {
        enabled: boolean;
        items: RenderPropertyEnum[];
        showUnits: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class AircraftInfoWindowPlugin extends JQueryUICustomWidget implements ISelfPersist<AircraftInfoWindowPlugin_SaveState> {
        options: AircraftInfoWindowPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _buildItems(state);
        private _destroyItems(state);
        saveState(): void;
        loadState(): AircraftInfoWindowPlugin_SaveState;
        applyState(settings: AircraftInfoWindowPlugin_SaveState): void;
        loadAndApplyState(): void;
        private _persistenceKey();
        private _createSettings();
        createOptionPane(displayOrder: number): OptionPane;
        suspend(onOff: boolean): void;
        showForAircraft(aircraft: Aircraft): void;
        refreshDisplay(): void;
        private _displayDetails(state, forceRefresh?);
        private _selectedAircraftChanged();
        private _markerClicked(event, data);
        private _infoWindowClosedByUser(event, data);
        private _aircraftListUpdated();
        private _displayUnitChanged();
        private _localeChanged();
    }
}
interface JQuery {
    vrsAircraftInfoWindow(): any;
    vrsAircraftInfoWindow(options: VRS.AircraftInfoWindowPlugin_Options): any;
    vrsAircraftInfoWindow(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    type LinkSiteEnumOrRenderHandler = LinkSiteEnum | LinkRenderHandler;
    interface AircraftLinksPlugin_Options {
        linkSites: LinkSiteEnumOrRenderHandler[];
    }
    var jQueryUIHelper: JQueryUIHelper;
    class AircraftLinksPlugin extends JQueryUICustomWidget {
        options: AircraftLinksPlugin_Options;
        constructor();
        _getState(): any;
        _create(): void;
        _destroy(): void;
        renderForAircraft(aircraft: Aircraft, forceRefresh: boolean): void;
        reRender(forceRefresh: boolean): void;
        private doReRender(forceRefresh, state);
        private _removeLinkElements(state);
    }
}
interface JQuery {
    vrsAircraftLinks(): any;
    vrsAircraftLinks(options: VRS.AircraftLinksPlugin_Options): any;
    vrsAircraftLinks(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftListPlugin_SaveState {
        columns: RenderPropertyEnum[];
        showUnits: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    interface AircraftListPlugin_Options {
        name?: string;
        aircraftList: AircraftList;
        aircraftListFetcher?: AircraftListFetcher;
        unitDisplayPreferences: UnitDisplayPreferences;
        sorter?: AircraftListSorter;
        showSorterOptions?: boolean;
        columns?: RenderPropertyEnum[];
        useSavedState?: boolean;
        useSorterSavedState?: boolean;
        showUnits?: boolean;
        distinguishOnGround?: boolean;
        flagUncertainCallsigns?: boolean;
        showPause?: boolean;
        showHideAircraftNotOnMap?: boolean;
    }
    class AircraftListPlugin extends JQueryUICustomWidget implements ISelfPersist<AircraftListPlugin_SaveState> {
        options: AircraftListPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        saveState(): void;
        loadState(): AircraftListPlugin_SaveState;
        applyState(settings: AircraftListPlugin_SaveState): void;
        loadAndApplyState(): void;
        private _persistenceKey();
        private _createSettings();
        createOptionPane(displayOrder: number): OptionPane[];
        suspend(onOff: boolean): void;
        prependElement(elementJQ: JQuery): void;
        private _buildTable(state);
        private _buildHeader(state);
        private _refreshDisplay(state, refreshAll, displayUnitChanged?);
        private _updatePoweredByCredit(state);
        private _updateTrackedAircraftCount(state, refreshAll);
        private _buildRows(state, refreshAll, displayUnitChanged?);
        private _ensureBodyRowsHaveCorrectNumberOfColumns(tbodyRows, dataArray, columnCount);
        private _setCellAlignment(cell, cellData, alignment);
        private _setCellWidth(cell, cellData, fixedWidth);
        private _showRow(state, row, rowData, visible);
        private _setRowState(row, rowData, rowNumber, isSelectedAircraft, isEmergency, isInterested);
        private _createWidgetRenderer(cell, cellData, handler);
        private _destroyWidgetRenderer(cell, cellData, handler?);
        private _getRowIndexForAircraftId(state, aircraftId);
        private _getRowIndexForRowElement(state, rowElement);
        private _aircraftListUpdated();
        private _localeChanged();
        private _displayUnitChanged(displayUnitChanged);
        private _rowClicked(event, target);
        private _selectedAircraftChanged(oldSelectedAircraft);
        private _sortFieldsChanged();
        private _sortableHeaderClicked(event);
    }
}
interface JQuery {
    vrsAircraftList(): any;
    vrsAircraftList(options: VRS.AircraftListPlugin_Options): any;
    vrsAircraftList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    var jQueryUIHelper: JQueryUIHelper;
    interface AircraftPositionMapPlugin_Options {
        plotterOptions: AircraftPlotterOptions;
        mirrorMapJQ: JQuery;
        stateName?: string;
        mapOptionOverrides?: IMapOptions;
        unitDisplayPreferences?: UnitDisplayPreferences;
        autoHideNoPosition: boolean;
        reflectMapTypeBackToMirror?: boolean;
    }
    class AircraftPositionMapPlugin extends JQueryUICustomWidget {
        options: AircraftPositionMapPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        private _createMap(state);
        _mapCreated(): void;
        _destroy(): void;
        renderAircraft(aircraft: Aircraft, showAsSelected: boolean): void;
        suspend(onOff: boolean): void;
        private _mapTypeChanged();
        private _mirrorMapTypeChanged();
        private _getAircraft();
        _getSelectedAircraft(): Aircraft;
    }
}
interface JQuery {
    vrsAircraftPositonMap(): any;
    vrsAircraftPositonMap(options: VRS.AircraftPositionMapPlugin_Options): any;
    vrsAircraftPositonMap(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    class GoogleMapUtilities {
        private _HighContrastMapTypeName;
        fromGoogleLatLng(latLng: google.maps.LatLng): ILatLng;
        toGoogleLatLng(latLng: ILatLng): google.maps.LatLng;
        fromGooglePoint(point: google.maps.Point): IPoint;
        toGooglePoint(point: IPoint): google.maps.Point;
        fromGoogleSize(size: google.maps.Size): ISize;
        toGoogleSize(size: ISize): google.maps.Size;
        fromGoogleLatLngBounds(latLngBounds: google.maps.LatLngBounds): IBounds;
        toGoogleLatLngBounds(bounds: IBounds): google.maps.LatLngBounds;
        fromGoogleMapControlStyle(mapControlStyle: google.maps.MapTypeControlStyle): VRS.MapControlStyleEnum;
        toGoogleMapControlStyle(mapControlStyle: VRS.MapControlStyleEnum): google.maps.MapTypeControlStyle;
        fromGoogleMapType(mapType: google.maps.MapTypeId | string): VRS.MapTypeEnum;
        toGoogleMapType(mapType: VRS.MapTypeEnum, suppressException?: boolean): google.maps.MapTypeId | string;
        fromGoogleIcon(icon: google.maps.Icon): IMapIcon;
        toGoogleIcon(icon: IMapIcon | string): google.maps.Icon;
        fromGoogleLatLngMVCArray(latLngMVCArray: google.maps.MVCArray): ILatLng[];
        toGoogleLatLngMVCArray(latLngArray: ILatLng[]): google.maps.MVCArray;
        fromGoogleLatLngMVCArrayArray(latLngMVCArrayArray: google.maps.MVCArray): ILatLng[][];
        toGoogleLatLngMVCArrayArray(latLngArrayArray: ILatLng[][]): google.maps.MVCArray;
        fromGoogleControlPosition(controlPosition: google.maps.ControlPosition): MapPositionEnum;
        toGoogleControlPosition(mapPosition: MapPositionEnum): google.maps.ControlPosition;
    }
    var googleMapUtilities: GoogleMapUtilities;
    class MapIcon implements IMapIcon {
        anchor: IPoint;
        origin: IPoint;
        scaledSize: ISize;
        size: ISize;
        url: string;
        labelAnchor: IPoint;
        constructor(url: string, size: ISize, anchor: IPoint, origin: IPoint, scaledSize?: ISize, labelAnchor?: IPoint);
    }
    var jQueryUIHelper: JQueryUIHelper;
}
interface JQuery {
    vrsMap(): any;
    vrsMap(options: VRS.IMapOptions): any;
    vrsMap(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface MapNextPageButton_Options {
        nextPageName: string;
        aircraftListFilter?: AircraftListFilter;
        aircraftListFetcher?: AircraftListFetcher;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class MapNextPageButton extends JQueryUICustomWidget {
        options: MapNextPageButton_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _showImage();
        private _buttonClicked(event);
        private _pausedChanged();
        private _filterEnabledChanged();
    }
}
interface JQuery {
    vrsMapNextPageButton(): any;
    vrsMapNextPageButton(options: VRS.MapNextPageButton_Options): any;
    vrsMapNextPageButton(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    var jQueryUIHelper: JQueryUIHelper;
    interface MenuPlugin_Options extends JQueryUICustomWidget_Options {
        menu: Menu;
        showButtonTrigger?: boolean;
        triggerElement?: JQuery;
        menuContainerClasses?: string;
        offsetX?: number;
        offsetY?: number;
        alignment?: string;
        cssMenuWidth?: number;
        zIndex?: number;
    }
    class MenuPlugin extends JQueryUICustomWidget {
        options: MenuPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _createMenu(state);
        private _destroyMenu(state);
        private _determineTopLeft(state);
        private _createMenuItemElements(state, parentJQ, menuItems);
        private _buildMenuItemImageElement(state, menuItem);
        private _buildMenuItemTextElement(state, menuItem);
        private _refreshMenuItem(state, menuItem);
        private _refreshChildItems(state, menuItem);
        getIsOpen(): boolean;
        private doGetIsOpen(state?);
        toggleMenu(): void;
        private doToggleMenu(state?);
        openMenu(): void;
        private doOpenMenu(state?);
        closeMenu(): void;
        private doCloseMenu(state?);
        private _triggerPressed(event);
        private _menuItemClicked(event, menuItem);
        private _clickCatcherClicked(event);
        private _windowResized(event);
    }
}
interface JQuery {
    vrsMenu(): any;
    vrsMenu(options: VRS.MenuPlugin_Options): any;
    vrsMenu(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface OptionDialog_Options {
        pages: OptionPage[];
        autoRemove?: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionDialog extends JQueryUICustomWidget {
        options: OptionDialog_Options;
        constructor();
        _create(): void;
    }
}
interface JQuery {
    vrsOptionDialog(): any;
    vrsOptionDialog(options: VRS.OptionDialog_Options): any;
    vrsOptionDialog(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldButtonPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldButton;
    }
    class OptionFieldButtonPlugin_State {
        refreshFieldContentHookResult: IEventHandle;
        refreshFieldStateHookResult: IEventHandle;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldButtonPlugin extends JQueryUICustomWidget {
        options: OptionFieldButtonPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionFieldButton(): any;
    vrsOptionFieldButton(options: VRS.OptionFieldButtonPlugin_Options): any;
    vrsOptionFieldButton(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldCheckBoxPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldCheckBox;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldCheckBoxPlugin extends JQueryUICustomWidget {
        options: OptionFieldCheckBoxPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionFieldCheckBox(): any;
    vrsOptionFieldCheckBox(options: VRS.OptionFieldCheckBoxPlugin_Options): any;
    vrsOptionFieldCheckBox(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldColourPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldColour;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldColourPlugin extends JQueryUICustomWidget {
        options: OptionFieldColourPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _generateColours();
    }
}
interface JQuery {
    vrsOptionFieldColour(): any;
    vrsOptionFieldColour(options: VRS.OptionFieldColourPlugin_Options): any;
    vrsOptionFieldColour(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldComboBoxPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldComboBox;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldComboBoxPlugin extends JQueryUICustomWidget {
        options: OptionFieldComboBoxPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _setVisibility(state, assumeVisible?);
        private _fieldRefreshVisibility();
    }
}
interface JQuery {
    vrsOptionFieldComboBox(): any;
    vrsOptionFieldComboBox(options: VRS.OptionFieldComboBoxPlugin_Options): any;
    vrsOptionFieldComboBox(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldDatePlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldDate;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldDatePlugin extends JQueryUICustomWidget {
        options: OptionFieldDatePlugin_Options;
        constructor();
        _create(): void;
        private _destroy();
    }
}
interface JQuery {
    vrsOptionFieldDate(): any;
    vrsOptionFieldDate(options: VRS.OptionFieldDatePlugin_Options): any;
    vrsOptionFieldDate(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldLabelPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldLabel;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldLabelPlugin extends JQueryUICustomWidget {
        options: OptionFieldLabelPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionFieldLabel(): any;
    vrsOptionFieldLabel(options: VRS.OptionFieldLabelPlugin_Options): any;
    vrsOptionFieldLabel(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldLinkLabelPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldLinkLabel;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldLinkLabelPlugin extends JQueryUICustomWidget {
        options: OptionFieldLinkLabelPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        private _setProperties();
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionFieldLinkLabel(): any;
    vrsOptionFieldLinkLabel(options: VRS.OptionFieldLinkLabelPlugin_Options): any;
    vrsOptionFieldLinkLabel(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldNumeric_Options extends OptionControl_BaseOptions {
        field: OptionFieldNumeric;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldNumericPlugin extends JQueryUICustomWidget {
        options: OptionFieldNumeric_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionFieldNumeric(): any;
    vrsOptionFieldNumeric(options: VRS.OptionFieldNumeric_Options): any;
    vrsOptionFieldNumeric(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldOrderedSubsetPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldOrderedSubset;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldOrderedSubsetPlugin extends JQueryUICustomWidget {
        options: OptionFieldOrderedSubsetPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _buildToolstripContent(state, values, subsetValues, autoSelect?);
        private _showEnabledDisabled(state);
        private _removeToolstripContent(state);
        private _buildSortableList(state, parent, values);
        private _addValueToSortableList(state, value);
        private _destroySortableListValue(valueElement);
        private _getSubsetValueObjects(values, subset);
        private _getSelectedValues(state);
        private _sortableSortStopped(event, ui);
        private _sortIncrementClicked(event);
        private _addValueButtonClicked();
        private _lockButtonClicked();
        private _removeIconClicked(event);
    }
}
interface JQuery {
    vrsOptionFieldOrderedSubset(): any;
    vrsOptionFieldOrderedSubset(options: VRS.OptionFieldOrderedSubsetPlugin_Options): any;
    vrsOptionFieldOrderedSubset(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldPaneListPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldPaneList;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldPaneListPlugin extends JQueryUICustomWidget {
        options: OptionFieldPaneListPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _addPane(pane, state);
        private _addOptionsPane(pane, parent);
        private _refreshControlStates();
        private _paneAdded(pane);
        _paneRemoved(pane: OptionPane, index: number): void;
        private _maxPanesChanged();
    }
}
interface JQuery {
    vrsOptionFieldPaneList(): any;
    vrsOptionFieldPaneList(options: VRS.OptionFieldPaneListPlugin_Options): any;
    vrsOptionFieldPaneList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldRadioButtonPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldRadioButton;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldRadioButtonPlugin extends JQueryUICustomWidget {
        options: OptionFieldRadioButtonPlugin_Options;
        constructor();
        _create(): void;
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionFieldRadioButton(): any;
    vrsOptionFieldRadioButton(options: VRS.OptionFieldRadioButtonPlugin_Options): any;
    vrsOptionFieldRadioButton(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionFieldTextBoxPlugin_Options extends OptionControl_BaseOptions {
        field: OptionFieldTextBox;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionFieldTextBoxPlugin extends JQueryUICustomWidget {
        options: OptionFieldTextBoxPlugin_Options;
        constructor();
        _create(): void;
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionFieldTextBox(): any;
    vrsOptionFieldTextBox(options: VRS.OptionFieldTextBoxPlugin_Options): any;
    vrsOptionFieldTextBox(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionForm_Options {
        pages: OptionPage[];
        showInAccordion?: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionForm extends JQueryUICustomWidget {
        options: OptionForm_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _buildValidPages();
        private _addPage(page, pagesContainer, selectPageContainer);
    }
}
interface JQuery {
    vrsOptionForm(): any;
    vrsOptionForm(options: VRS.OptionForm_Options): any;
    vrsOptionForm(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface OptionPanePlugin_Options {
        optionPane: OptionPane;
        optionPageParent: OptionPageParent;
        isInStack?: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class OptionPanePlugin extends JQueryUICustomWidget {
        options: OptionPanePlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
    }
}
interface JQuery {
    vrsOptionPane(): any;
    vrsOptionPane(options: VRS.OptionPanePlugin_Options): any;
    vrsOptionPane(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface PagePanel_Options {
        element: JQuery;
        previousPageName?: string;
        previousPageLabelKey?: string;
        nextPageName?: string;
        nextPageLabelKey?: string;
        titleLabelKey?: string;
        headerMenu?: Menu;
        showFooterGap?: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class PagePanel extends JQueryUICustomWidget {
        options: PagePanel_Options;
        constructor();
        private _getState();
        _create(): void;
        private _destroy();
        private _updateHeaderText();
        private _doPageClicked(event, pageName);
        _setOption(key: string, value: any): void;
        private _previousPageClicked(event);
        private _nextPageClicked(event);
        private _localeChanged();
    }
}
interface JQuery {
    vrsPagePanel(): any;
    vrsPagePanel(options: VRS.PagePanel_Options): any;
    vrsPagePanel(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface ReportDetailPlugin_Options extends ReportRender_Options {
        name?: string;
        report: Report;
        unitDisplayPreferences: UnitDisplayPreferences;
        plotterOptions?: AircraftPlotterOptions;
        columns?: ReportAircraftOrFlightPropertyEnum[];
        useSavedState?: boolean;
        showUnits?: boolean;
        showEmptyValues?: boolean;
        distinguishOnGround?: boolean;
    }
    interface ReportDetailPlugin_SaveState {
        columns: ReportAircraftOrFlightPropertyEnum[];
        showUnits: boolean;
        showEmptyValues: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class ReportDetailPlugin extends JQueryUICustomWidget implements ISelfPersist<ReportDetailPlugin_SaveState> {
        options: ReportDetailPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        saveState(): void;
        loadState(): ReportDetailPlugin_SaveState;
        applyState(settings: ReportDetailPlugin_SaveState): void;
        loadAndApplyState(): void;
        private _persistenceKey();
        private _createSettings();
        createOptionPane(displayOrder: number): OptionPane;
        suspend(onOff: boolean): void;
        private _displayFlightDetails(state, flight);
        refreshDisplay(): void;
        private _destroyDisplay(state);
        private _createHeader(state, flight);
        private _addHeaderCell(state, row, colspan, flight, property, classes);
        private _createBody(state, flight);
        private _createLinks(state, flight);
        private _localeChanged();
        private _selectedFlightChanged();
    }
}
interface JQuery {
    vrsReportDetail(): any;
    vrsReportDetail(options: VRS.ReportDetailPlugin_Options): any;
    vrsReportDetail(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface ReportListPlugin_Options extends ReportRender_Options {
        name?: string;
        report: Report;
        unitDisplayPreferences: UnitDisplayPreferences;
        singleAircraftColumns?: ReportAircraftPropertyEnum[];
        manyAircraftColumns?: ReportAircraftOrFlightPropertyEnum[];
        useSavedState?: boolean;
        showUnits?: boolean;
        distinguishOnGround?: boolean;
        showPagerTop?: boolean;
        showPagerBottom?: boolean;
        groupBySortColumn?: boolean;
        groupResetAlternateRows?: boolean;
        justShowStartTime?: boolean;
        alwaysShowEndDate?: boolean;
    }
    interface ReportListPlugin_SaveState {
        singleAircraftColumns: ReportAircraftPropertyEnum[];
        manyAircraftColumns: ReportAircraftOrFlightPropertyEnum[];
        showUnits: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class ReportListPlugin extends JQueryUICustomWidget implements ISelfPersist<ReportListPlugin_SaveState> {
        options: ReportListPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        private _destroy();
        saveState(): void;
        loadState(): ReportListPlugin_SaveState;
        applyState(settings: ReportListPlugin_SaveState): void;
        loadAndApplyState(): void;
        private _persistenceKey();
        private _createSettings();
        createOptionPane(displayOrder: number): OptionPane[];
        refreshDisplay(): void;
        private _buildTable(state);
        private _addPagerToTable(pager, section, cellType, colspan);
        private _buildHeader(state, columns, pager);
        private _buildBody(state, columns, flights, pager);
        private _destroyTable(state);
        private _setCellAlignment(cell, alignment);
        private _setFixedWidth(cell, fixedWidth);
        private _getFlightForTableRow(row);
        private _markSelectedRow();
        private _localeChanged();
        private _rowClicked(event, target);
        private _rowsFetched();
        private _selectedFlightChanged();
    }
}
interface JQuery {
    vrsReportList(): any;
    vrsReportList(options: VRS.ReportListPlugin_Options): any;
    vrsReportList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface ReportMapPlugin_Options {
        name?: string;
        report?: Report;
        plotterOptions: AircraftPlotterOptions;
        elementClasses?: string;
        unitDisplayPreferences: UnitDisplayPreferences;
        mapOptionOverrides?: IMapOptions;
        mapControls?: IMapControl[];
        scrollToAircraft?: boolean;
        showPath?: boolean;
        startSelected?: boolean;
        loadedCallback?: () => void;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class ReportMapPlugin extends JQueryUICustomWidget {
        options: ReportMapPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        isOpen(): boolean;
        private _createMap(state);
        private _mapCreated();
        private _buildFakeVrsAircraft(state);
        private _getAircraft();
        private _getSelectedAircraft();
        private _localeChanged();
        private _selectedFlightChanged();
        showFlight(flight: IReportFlight): void;
    }
}
interface JQuery {
    vrsReportMap(): any;
    vrsReportMap(options: VRS.ReportMapPlugin_Options): any;
    vrsReportMap(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface ReportPagerPlugin_Options {
        name?: string;
        report: Report;
        allowPageSizeChange?: boolean;
        allowShowAllRows?: boolean;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class ReportPagerPlugin extends JQueryUICustomWidget {
        options: ReportPagerPlugin_Options;
        constructor();
        private _getState();
        _create(): void;
        _destroy(): void;
        private _createPageSelectionPanel(state, container);
        private _destroyPageSelectionPanel(state);
        private _createPageSizePanel(state, container);
        private _destroyPageSizePanel(state);
        private _setPageSelectionPanelState(state);
        private _getPageMetrics();
        private _rowsFetched();
        private _firstPageClicked();
        private _prevPageClicked();
        private _nextPageClicked();
        private _lastPageClicked();
        private _fetchPageClicked();
        private _pageSizeChanged();
        private _pageSizeChangedByUs();
    }
}
interface JQuery {
    vrsReportPager(): any;
    vrsReportPager(options: VRS.ReportPagerPlugin_Options): any;
    vrsReportPager(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface SelectDialog_Options {
        items: ValueText[] | (() => ValueText[]);
        value?: any;
        autoOpen?: boolean;
        onSelect?: (selectedValue: ValueText) => void;
        titleKey?: string;
        minWidth?: number;
        minHeight?: number;
        onClose?: () => void;
        modal?: boolean;
        lines?: number;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class SelectDialog extends JQueryUICustomWidget {
        options: SelectDialog_Options;
        constructor();
        _create(): void;
    }
}
interface JQuery {
    vrsSelectDialog(): any;
    vrsSelectDialog(options: VRS.SelectDialog_Options): any;
    vrsSelectDialog(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface Splitter_Detail {
        splitter: Splitter;
        barMovedHookResult: IEventHandleJQueryUI;
        pane1Length?: number;
        pane2Length?: number;
    }
    interface SplitterGroupPersistence_SaveState {
        lengths: SplitterGroupPersistence_SplitterSaveState[];
    }
    interface SplitterGroupPersistence_SplitterSaveState {
        name: string;
        pane: number;
        vertical: boolean;
        length: number;
    }
    class SplitterGroupPersistence implements ISelfPersist<SplitterGroupPersistence_SaveState> {
        private _Name;
        private _SplitterDetails;
        private _AutoSaveEnabled;
        constructor(name: string);
        getAutoSaveEnabled(): boolean;
        setAutoSaveEnabled(value: boolean): void;
        dispose(): void;
        saveState(): void;
        loadState(): SplitterGroupPersistence_SaveState;
        getSplitterSavedState(splitterName: string): any;
        applyState(settings: SplitterGroupPersistence_SaveState): void;
        loadAndApplyState(): void;
        private persistenceKey();
        private createSettings();
        registerSplitter(splitterElement: JQuery): Splitter_Detail;
        private onBarMoved(event, data);
    }
    var jQueryUIHelper: JQueryUIHelper;
    interface SplitterPane_Options {
        isVertical?: boolean;
        minPixels?: number;
        max?: PercentValue | ((splitterWidth: number) => number);
        startSize?: PercentValue | ((splitterWidth: number) => number);
    }
    class SplitterPane extends JQueryUICustomWidget {
        options: SplitterPane_Options;
        private _getState();
        _create(): void;
        _destroy(): void;
        getContainer(): JQuery;
        getContent(): JQuery;
        getContentIsSplitter(): boolean;
        getMinPixels(): number;
        getMaxPixels(availableLengthWithoutBar: number): number;
        getStartSize(availableLengthWithoutBar: number): number;
        _getPixelsFromMaxOrStartSize(maxOrStartSize: PercentValue | ((size: number) => number), availableLengthWithoutBar: number): number;
    }
    interface Splitter_Options {
        name: string;
        vertical?: boolean;
        savePane?: number;
        collapsePane?: number;
        minPixels?: number[];
        maxPane?: number;
        max?: number | string | PercentValue | ((availableLength: number) => number);
        startSizePane?: number;
        startSize?: number | string | PercentValue | ((availableLength: number) => number);
        splitterGroupPersistence?: SplitterGroupPersistence;
        isTopLevelSplitter?: boolean;
        leftTopParent?: JQuery;
        rightBottomParent?: JQuery;
    }
    interface Splitter_BarMovedEventArgs {
        splitterElement: JQuery;
        pane1Length: number;
        pane2Length: number;
        barLength: number;
    }
    class Splitter extends JQueryUICustomWidget {
        options: Splitter_Options;
        private _getState();
        _create(): void;
        _destroy(): void;
        private convertMaxOrStartSize(paneNumber, maxOrStartSize, description);
        getName(): string;
        getIsVertical(): boolean;
        getSavePane(): number;
        hookBarMoved(callback: (event: Event, data: Splitter_BarMovedEventArgs) => void, forceThis?: Object): IEventHandleJQueryUI;
        private _raiseBarMoved(state);
        unhook(hookResult: IEventHandleJQueryUI): void;
        private _determineAvailableLength(state);
        private _determineAvailableLengthWithoutBar(state, availableLength);
        private _sizePanesToSplitter(state, availableLength);
        private _applyMinMax(state, availableLength);
        applySavedLength(savedState: SplitterGroupPersistence_SplitterSaveState): void;
        private _setPositions(state, availableLength);
        private _moveSplitterToFitPaneLength(state, fitPaneIndex, availableLength?);
        private _moveSplitterToKeepPaneProportions(state, availableLength);
        private adjustToNewSize(state);
        private _syncCollapseButtonState(state);
        private _barMouseDown(event);
        private _touchStart(event);
        private _startMove(event, testForLeftButton, pageX, pageY, hookMoveAndUp);
        private _documentMouseMove(event);
        private _touchMove(event);
        private _continueMove(event, pageX, pageY);
        private _documentMouseUp(event);
        private _touchEnd(event);
        private _stopMove(event, unhookMoveAndUp);
        private _windowResized();
        private _collapseClicked(event);
    }
}
interface JQuery {
    vrsSplitterPane(): any;
    vrsSplitterPane(options: VRS.SplitterPane_Options): any;
    vrsSplitterPane(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
    vrsSplitter(): any;
    vrsSplitter(options: VRS.Splitter_Options): any;
    vrsSplitter(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    class StoredSettingsList extends JQueryUICustomWidget {
        options: {};
        private _getState();
        _create(): void;
        private _addCheckBox(parentElement, labelText, initialCheckedState);
        private _buildKeysTable(state);
        private _showKeyContent(keyName, content);
        private _refreshDisplay();
        private _removeKey(keyName);
        private _removeAllKeys();
        private _exportSettings();
        private _showImportControls();
        private _importSettings(options);
        private _keyClicked(keyName);
    }
}
interface JQuery {
    vrsStoredSettingsList(): any;
    vrsStoredSettingsList(options: {}): any;
    vrsStoredSettingsList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface TimeoutMessageBoxPlugin_Options extends JQueryUICustomWidget_Options {
        aircraftListFetcher: any;
    }
    var jQueryUIHelper: JQueryUIHelper;
    class TimeoutMessageBoxPlugin extends JQueryUICustomWidget {
        options: TimeoutMessageBoxPlugin_Options;
        private _getState();
        _create(): void;
        _destroy(): void;
        private _siteTimedOut();
    }
}
interface JQuery {
    vrsTimeoutMessageBox(): any;
    vrsTimeoutMessageBox(options: VRS.TimeoutMessageBoxPlugin_Options): any;
    vrsTimeoutMessageBox(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any): any;
}
declare namespace VRS {
    interface JQueryUIHelper {
        getAircraftDetailPlugin?: (element: JQuery) => AircraftDetailPlugin;
        getAircraftDetailOptions?: (overrides?: AircraftDetailPlugin_Options) => AircraftDetailPlugin_Options;
        getAircraftInfoWindowPlugin?: (element: JQuery) => AircraftInfoWindowPlugin;
        getAircraftInfoWindowOptions?: (overrides?: AircraftInfoWindowPlugin_Options) => AircraftInfoWindowPlugin_Options;
        getAircraftLinksPlugin?: (element: JQuery) => AircraftLinksPlugin;
        getAircraftLinksOptions?: (overrides?: AircraftLinksPlugin_Options) => AircraftLinksPlugin_Options;
        getAircraftListPlugin?: (element: JQuery) => AircraftListPlugin;
        getAircraftListOptions?: (overrides?: AircraftListPlugin_Options) => AircraftListPlugin_Options;
        getAircraftPositionMapPlugin?: (element: JQuery) => AircraftPositionMapPlugin;
        getAircraftPositionMapOptions?: (overrides?: AircraftPositionMapPlugin_Options) => AircraftPositionMapPlugin_Options;
        getMapNextPageButtonPlugin?: (element: JQuery) => MapNextPageButton;
        getMapNextPageButtonOptions?: (overrides?: MapNextPageButton_Options) => MapNextPageButton_Options;
        getMapPlugin?: (element: JQuery) => IMap;
        getMapOptions?: (overrides?: IMapOptions) => IMapOptions;
        getMenuPlugin?: (jQueryElement: JQuery) => MenuPlugin;
        getMenuOptions?: (overrides?: MenuPlugin_Options) => MenuPlugin_Options;
        getOptionDialogPlugin?: (element: JQuery) => OptionDialog;
        getOptionDialogOptions?: (overrides?: OptionDialog_Options) => OptionDialog_Options;
        getOptionFieldButtonPlugin?: (element: JQuery) => OptionFieldButtonPlugin;
        getOptionFieldButtonOptions?: (overrides?: OptionFieldButtonPlugin_Options) => OptionFieldButtonPlugin_Options;
        getOptionFieldCheckBoxPlugin?: (element: JQuery) => OptionFieldCheckBoxPlugin;
        getOptionFieldCheckBoxOptions?: (overrides?: OptionFieldCheckBoxPlugin_Options) => OptionFieldCheckBoxPlugin_Options;
        getOptionFieldColourPlugin?: (element: JQuery) => OptionFieldColourPlugin;
        getOptionFieldColourOptions?: (overrides?: OptionFieldColourPlugin_Options) => OptionFieldColourPlugin_Options;
        getOptionFieldComboBoxPlugin?: (element: JQuery) => OptionFieldComboBoxPlugin;
        getOptionFieldComboBoxOptions?: (overrides?: OptionFieldComboBoxPlugin_Options) => OptionFieldComboBoxPlugin_Options;
        getOptionFieldDatePlugin?: (jQueryElement: JQuery) => OptionFieldDatePlugin;
        getOptionFieldDateOptions?: (overrides?: OptionFieldDatePlugin_Options) => OptionFieldDatePlugin_Options;
        getOptionFieldLabelPlugin?: (element: JQuery) => OptionFieldLabelPlugin;
        getOptionFieldLabelOptions?: (overrides?: OptionFieldLabelPlugin_Options) => OptionFieldLabelPlugin_Options;
        getOptionFieldLinkLabelPlugin?: (element: JQuery) => OptionFieldLinkLabelPlugin;
        getOptionFieldLinkLabelOptions?: (overrides?: OptionFieldLinkLabelPlugin_Options) => OptionFieldLinkLabelPlugin_Options;
        getOptionFieldNumericPlugin?: (element: JQuery) => OptionFieldNumericPlugin;
        getOptionFieldNumericOptions?: (overrides?: OptionFieldNumeric_Options) => OptionFieldNumeric_Options;
        getOptionFieldOrderedSubsetPlugin?: (element: JQuery) => OptionFieldOrderedSubsetPlugin;
        getOptionFieldOrderedSubsetOptions?: (overrides?: OptionFieldOrderedSubsetPlugin_Options) => OptionFieldOrderedSubsetPlugin_Options;
        getOptionFieldPaneListPlugin?: (element: JQuery) => OptionFieldPaneListPlugin;
        getOptionFieldPaneListOptions?: (overrides?: OptionFieldPaneListPlugin_Options) => OptionFieldPaneListPlugin_Options;
        getOptionFieldRadioButtonPlugin?: (element: JQuery) => OptionFieldRadioButtonPlugin;
        getOptionFieldRadioButtonOptions?: (overrides?: OptionFieldRadioButtonPlugin_Options) => OptionFieldRadioButtonPlugin_Options;
        getOptionFieldTextBoxPlugin?: (element: JQuery) => OptionFieldTextBoxPlugin;
        getOptionFieldTextBoxOptions?: (overrides?: OptionFieldTextBoxPlugin_Options) => OptionFieldTextBoxPlugin_Options;
        getOptionFormPlugin?: (element: JQuery) => OptionForm;
        getOptionFormOptions?: (overrides?: OptionForm_Options) => OptionForm_Options;
        getOptionPanePlugin?: (element: JQuery) => OptionPanePlugin;
        getOptionPaneOptions?: (overrides?: OptionPanePlugin_Options) => OptionPanePlugin_Options;
        getPagePanelPlugin?: (element: JQuery) => PagePanel;
        getPagePanelOptions?: (overrides?: PagePanel_Options) => PagePanel_Options;
        getReportDetailPlugin?: (element: JQuery) => ReportDetailPlugin;
        getReportDetailOptions?: (overrides?: ReportDetailPlugin_Options) => ReportDetailPlugin_Options;
        getReportListPlugin?: (element: JQuery) => ReportListPlugin;
        getReportListOptions?: (overrides?: ReportListPlugin_Options) => ReportListPlugin_Options;
        getReportMapPlugin?: (element: JQuery) => ReportMapPlugin;
        getReportMapOptions?: (overrides?: ReportMapPlugin_Options) => ReportMapPlugin_Options;
        getReportPagerPlugin?: (element: JQuery) => ReportPagerPlugin;
        getReportPagerOptions?: (overrides?: ReportPagerPlugin_Options) => ReportPagerPlugin_Options;
        getSelectDialogPlugin?: (element: JQuery) => SelectDialog;
        getSelectDialogOptions?: (overrides?: SelectDialog_Options) => SelectDialog_Options;
        getSplitterPlugin?: (element: JQuery) => Splitter;
        getSplitterPanePlugin?: (element: JQuery) => SplitterPane;
        getTimeoutMessageBox?: (element: JQuery) => TimeoutMessageBoxPlugin;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    class Value<T> {
        val: T;
        chg: boolean;
        constructor(value?: T);
        setValue(value: T): void;
    }
    class StringValue extends Value<string> {
        constructor(value?: string);
    }
    class BoolValue extends Value<boolean> {
        constructor(value?: boolean);
    }
    class NumberValue extends Value<number> {
        constructor(value?: number);
    }
    class ArrayValue<T> {
        arr: T[];
        chg: boolean;
        chgIdx: number;
        trimStartCount: number;
        constructor(initialArray?: T[]);
        setValue(value: T[]): void;
        setNoChange(): void;
        resetArray(): void;
        trimStart(trimCount: number): void;
    }
    class RouteValue extends Value<string> {
        private _AirportCodeDerivedFromVal;
        private _AirportCode;
        constructor(value?: string);
        getAirportCode(): string;
    }
    class AirportDataThumbnailValue extends Value<IAirportDataThumbnails> {
        private _LastResetChgValue;
        constructor(value?: IAirportDataThumbnails);
        resetChg(): void;
    }
    class ShortTrailValue {
        lat: number;
        lng: number;
        posnTick: number;
        altitude: number;
        speed: number;
        constructor(latitude: number, longitude: number, posnTick: number, altitude: number, speed: number);
    }
    class FullTrailValue {
        lat: number;
        lng: number;
        heading: number;
        altitude: number;
        speed: number;
        chg: boolean;
        constructor(latitude: number, longitude: number, heading: number, altitude?: number, speed?: number);
    }
    type TrailArray = ArrayValue<ShortTrailValue> | ArrayValue<FullTrailValue>;
    interface Aircraft_ApplyJsonSettings {
        shortTrailTickThreshold: number;
        picturesEnabled: boolean;
    }
    class Aircraft {
        private _AircraftListFetcher;
        private signalLevelHistory;
        id: number;
        secondsTracked: number;
        updateCounter: number;
        receiverId: NumberValue;
        icao: StringValue;
        icaoInvalid: BoolValue;
        registration: StringValue;
        altitude: NumberValue;
        altitudeType: Value<AltitudeTypeEnum>;
        targetAltitude: NumberValue;
        callsign: StringValue;
        callsignSuspect: BoolValue;
        latitude: NumberValue;
        longitude: NumberValue;
        isMlat: BoolValue;
        positionTime: NumberValue;
        positionStale: BoolValue;
        speed: NumberValue;
        speedType: Value<SpeedTypeEnum>;
        verticalSpeed: NumberValue;
        verticalSpeedType: Value<AltitudeTypeEnum>;
        heading: NumberValue;
        headingIsTrue: BoolValue;
        targetHeading: NumberValue;
        manufacturer: StringValue;
        serial: StringValue;
        yearBuilt: StringValue;
        model: StringValue;
        modelIcao: StringValue;
        from: RouteValue;
        to: RouteValue;
        via: ArrayValue<RouteValue>;
        operator: StringValue;
        operatorIcao: StringValue;
        squawk: StringValue;
        isEmergency: BoolValue;
        distanceFromHereKm: NumberValue;
        bearingFromHere: NumberValue;
        wakeTurbulenceCat: Value<WakeTurbulenceCategoryEnum>;
        countEngines: StringValue;
        engineType: Value<EngineTypeEnum>;
        enginePlacement: NumberValue;
        species: Value<SpeciesEnum>;
        isMilitary: BoolValue;
        isTisb: BoolValue;
        country: StringValue;
        hasPicture: BoolValue;
        pictureWidth: NumberValue;
        pictureHeight: NumberValue;
        countFlights: NumberValue;
        countMessages: NumberValue;
        isOnGround: BoolValue;
        userTag: StringValue;
        userInterested: BoolValue;
        signalLevel: NumberValue;
        averageSignalLevel: NumberValue;
        airportDataThumbnails: AirportDataThumbnailValue;
        transponderType: Value<TransponderTypeEnum>;
        shortTrail: ArrayValue<ShortTrailValue>;
        fullTrail: ArrayValue<FullTrailValue>;
        applyJson(aircraftJson: IAircraftListAircraft, aircraftListFetcher: AircraftListFetcher, settings: Aircraft_ApplyJsonSettings): void;
        private setValue<T>(field, jsonValue, alwaysSent?);
        private setRouteArray<T>(field, jsonArray);
        private setShortTrailArray(field, jsonArray, resetTrail, shortTrailTickThreshold, trailType);
        private setFullTrailArray(field, jsonArray, resetTrail, trailType);
        private recordSignalLevelHistory(signalLevel);
        hasPosition(): boolean;
        getPosition(): ILatLng;
        positionWithinBounds(bounds: IBounds): boolean;
        hasRoute(): boolean;
        hasRouteChanged(): boolean;
        getViaAirports(): string[];
        getAirportCodes(distinctOnly?: boolean): string[];
        isAircraftSpecies(): boolean;
        convertSpeed(toUnit: SpeedEnum): number;
        convertAltitude(toUnit: HeightEnum): number;
        convertDistanceFromHere(toUnit: DistanceEnum): number;
        convertVerticalSpeed(toUnit: HeightEnum, perSecond: boolean): number;
        fetchAirportDataThumbnails(numThumbnails?: number): void;
        formatAirportDataThumbnails(showLinkToSite?: boolean): string;
        formatAltitude(heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean): string;
        formatAltitudeType(): string;
        formatAverageSignalLevel(): string;
        formatBearingFromHere(showUnits: boolean): string;
        formatBearingFromHereImage(): string;
        formatCallsign(showUncertainty: boolean): string;
        formatCountFlights(format?: string): string;
        formatCountMessages(format?: string): string;
        formatCountry(): string;
        formatDistanceFromHere(distanceUnit: DistanceEnum, showUnits: boolean): string;
        formatEngines(): string;
        formatFlightLevel(transitionAltitude: number, transitionAltitudeUnit: HeightEnum, flightLevelAltitudeUnit: HeightEnum, altitudeUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean): string;
        formatHeading(showUnit: boolean, showType: boolean): string;
        formatHeadingType(): string;
        formatIcao(): string;
        formatIsMilitary(): string;
        formatIsMlat(): string;
        formatIsTisb(): string;
        formatLatitude(showUnit: boolean): string;
        formatLongitude(showUnit: boolean): string;
        formatManufacturer(): string;
        formatModel(): string;
        formatModelIcao(): string;
        formatModelIcaoImageHtml(): string;
        formatModelIcaoNameAndDetail(): string;
        formatOperator(): string;
        formatOperatorIcao(): string;
        formatOperatorIcaoAndName(): string;
        formatOperatorIcaoImageHtml(): string;
        formatPictureHtml(requestSize: ISizePartial, allowResizeUp?: boolean, linkToOriginal?: boolean, blankSize?: ISize): string;
        formatReceiver(): string;
        formatRegistration(onlyAlphaNumeric?: boolean): string;
        formatRouteFull(): string;
        formatRouteMultiLine(): string;
        formatRouteShort(abbreviateStopovers?: boolean, showRouteNotKnown?: boolean): string;
        formatSecondsTracked(): string;
        formatSerial: () => string;
        formatSignalLevel(): string;
        formatSpecies(ignoreNone: boolean): string;
        formatSpeed(speedUnit: SpeedEnum, showUnit: boolean, showType: boolean): string;
        formatSpeedType(): string;
        formatSquawk(): string;
        formatSquawkDescription(): string;
        formatTargetAltitude(heightUnit: HeightEnum, showUnits: boolean, showType: boolean): string;
        formatTargetHeading(showUnits: boolean, showType: boolean): string;
        formatTransponderType(): string;
        formatTransponderTypeImageHtml(): string;
        formatUserInterested(): string;
        formatUserTag(): string;
        formatVerticalSpeed(heightUnit: HeightEnum, perSecond: boolean, showUnit: boolean, showType: boolean): string;
        formatVerticalSpeedType(): string;
        formatWakeTurbulenceCat(ignoreNone: boolean, expandedDescription: boolean): string;
        formatYearBuilt(): string;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftAutoSelect_SaveState {
        enabled: boolean;
        selectClosest: boolean;
        offRadarAction: OffRadarActionEnum;
        filters: ISerialisedFilter[];
    }
    class AircraftAutoSelect implements ISelfPersist<AircraftAutoSelect_SaveState> {
        private _Dispatcher;
        private _Events;
        private _AircraftList;
        private _LastAircraftSelected;
        private _Name;
        private _Enabled;
        private _SelectClosest;
        private _OffRadarAction;
        private _Filters;
        private _AircraftListUpdatedHook;
        private _SelectedAircraftChangedHook;
        constructor(aircraftList: AircraftList, name?: string);
        dispose: () => void;
        getName: () => string;
        getEnabled: () => boolean;
        setEnabled: (value: boolean) => void;
        getSelectClosest: () => boolean;
        setSelectClosest: (value: boolean) => void;
        getOffRadarAction: () => string;
        setOffRadarAction: (value: string) => void;
        getFilter: (index: number) => AircraftFilter;
        setFilter: (index: number, aircraftFilter: AircraftFilter) => void;
        hookEnabledChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        unhook: (hookResult: IEventHandle) => void;
        saveState: () => void;
        loadState: () => AircraftAutoSelect_SaveState;
        applyState: (settings: AircraftAutoSelect_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey;
        private createSettings;
        createOptionPane: (displayOrder: number) => OptionPane[];
        addFilter: (filterOrFilterProperty: AircraftFilter | string) => AircraftFilter;
        private filterPaneRemoved;
        private removeFilterAt;
        closestAircraft: (aircraftList: AircraftList) => Aircraft;
        private aircraftListUpdated;
        private selectedAircraftChanged;
    }
}
declare namespace VRS {
    interface AircraftFilterPropertyHandler_Settings extends FilterPropertyHandler_Settings {
        property: AircraftFilterPropertyEnum;
        getValueCallback: (aircraft: Aircraft, options?: Filter_Options) => any;
    }
    class AircraftFilterPropertyHandler extends FilterPropertyHandler {
        property: AircraftFilterPropertyEnum;
        getValueCallback: (aircraft: Aircraft, options?: Filter_Options) => any;
        constructor(settings: AircraftFilterPropertyHandler_Settings);
    }
    var aircraftFilterPropertyHandlers: {
        [index: string]: AircraftFilterPropertyHandler;
    };
    class AircraftFilter extends Filter {
        constructor(property: AircraftFilterPropertyEnum, valueCondition: ValueCondition);
        passes(aircraft: Aircraft, options: Filter_Options): boolean;
    }
    class AircraftFilterHelper extends FilterHelper {
        constructor();
        aircraftPasses(aircraft: Aircraft, aircraftFilters: AircraftFilter[], options?: Filter_Options): boolean;
    }
    var aircraftFilterHelper: AircraftFilterHelper;
}
declare namespace VRS {
    class AircraftCollection {
        foreachAircraft(callback: (aircraft: Aircraft) => void): AircraftCollection;
        findAircraftById(id: number): Aircraft;
        toList(filterCallback?: (aircraft: Aircraft) => boolean): Aircraft[];
    }
    class AircraftList {
        private _Dispatcher;
        private _Events;
        private _Aircraft;
        getAircraft(): AircraftCollection;
        private _CountTrackedAircraft;
        getCountTrackedAircraft(): number;
        private _CountAvailableAircraft;
        getCountAvailableAircraft(): number;
        private _AircraftListSource;
        getAircraftListSource(): AircraftListSourceEnum;
        private _ServerHasSilhouettes;
        getServerHasSilhouettes(): boolean;
        private _ServerHasOperatorFlags;
        getServerHasOperatorFlags(): boolean;
        private _ServerHasPictures;
        getServerHasPictures(): boolean;
        private _FlagWidth;
        getFlagWidth(): number;
        private _FlagHeight;
        getFlagHeight(): number;
        private _DataVersion;
        getDataVersion(): number;
        private _ShortTrailSeconds;
        getShortTrailSeconds(): number;
        private _ServerTicks;
        getServerTicks(): number;
        private _WasAircraftSelectedByUser;
        getWasAircraftSelectedByUser(): boolean;
        private _SelectedAircraft;
        getSelectedAircraft(): Aircraft;
        setSelectedAircraft(value: Aircraft, wasSelectedByUser: boolean): void;
        hookUpdated(callback: (newAircraft: AircraftCollection, offRadar: AircraftCollection) => void, forceThis?: Object): IEventHandle;
        hookSelectedAircraftChanged(callback: (wasSelected: Aircraft) => void, forceThis?: Object): IEventHandle;
        hookSelectedReselected(callback: () => void, forceThis?: Object): IEventHandle;
        hookFetchingList(callback: (parameters: Object, headers: Object, postBody: Object) => void, forceThis?: Object): IEventHandle;
        raiseFetchingList(xhrParams: Object, xhrHeaders: Object, xhrPostBody: Object): void;
        unhook(hookResult: IEventHandle): void;
        foreachAircraft(callback: (aircraft: Aircraft) => void): AircraftCollection;
        toList(filterCallback?: (aircraft: Aircraft) => boolean): Aircraft[];
        findAircraftById: (id: number) => Aircraft;
        getAllAircraftIdsString(): string;
        getAllAircraftIcaosString(): string;
        applyJson(aircraftListJson: IAircraftList, aircraftListFetcher: AircraftListFetcher): void;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftListFetcher_Settings {
        aircraftList: AircraftList;
        name?: string;
        currentLocation?: CurrentLocation;
        mapJQ?: JQuery;
        fetchFsxList?: boolean;
    }
    interface AircraftListFetcher_SaveState {
        interval: number;
        requestFeedId: number;
        hideAircraftNotOnMap: boolean;
    }
    class AircraftListFetcher implements ISelfPersist<AircraftListFetcher_SaveState> {
        private _Dispatcher;
        private _Events;
        private _Settings;
        private _LastRequestFeedId;
        private _TimeoutHandle;
        private _ServerConfigChangedHook;
        private _SiteTimedOutHook;
        private _MinimumRefreshInterval;
        private _ServerConfigDefaultRefreshInterval;
        private _Feeds;
        private _RequestFeedId;
        private _IntervalMilliseconds;
        private _Paused;
        private _HideAircraftNotOnMap;
        private _MapPlugin;
        private _MapJQ;
        private _ActualFeedId;
        constructor(settings: AircraftListFetcher_Settings);
        getName: () => string;
        getFeeds: () => IReceiver[];
        getSortedFeeds: (includeDefaultFeed?: boolean) => IReceiver[];
        getFeed: (id: number) => IReceiver;
        getRequestFeedId: () => number;
        setRequestFeedId: (value?: number) => void;
        getActualFeedId: () => number;
        getActualFeed: () => IReceiver;
        getInterval: () => number;
        setInterval: (value: number) => void;
        getPaused: () => boolean;
        setPaused: (value: boolean) => void;
        getHideAircraftNotOnMap: () => boolean;
        setHideAircraftNotOnMap: (value: boolean) => void;
        getMapJQ: () => JQuery;
        setMapJQ: (value: JQuery) => void;
        hookPausedChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        hookHideAircraftNotOnMapChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        unhook: (hookResult: IEventHandle) => void;
        dispose: () => void;
        saveState: () => void;
        loadState: () => AircraftListFetcher_SaveState;
        applyState: (settings: AircraftListFetcher_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey;
        private createSettings;
        createOptionPane: (displayOrder: number) => OptionPane;
        private getIntervalSeconds;
        private setIntervalSeconds;
        private applyServerConfiguration;
        private fetch;
        private fetchSuccess;
        private fetchError;
        private serverConfigChanged;
        private siteTimedOut;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftListFilter_Settings {
        name?: string;
        aircraftList: AircraftList;
        unitDisplayPreferences: UnitDisplayPreferences;
    }
    interface AircraftListFilter_SaveState {
        filters: ISerialisedFilter[];
        enabled: boolean;
    }
    class AircraftListFilter implements ISelfPersist<AircraftListFilter_SaveState> {
        private _Dispatcher;
        private _Events;
        private _Settings;
        private _Filters;
        private _AircraftList_FetchingListHookResult;
        private _Enabled;
        constructor(settings: AircraftListFilter_Settings);
        getName: () => string;
        getEnabled: () => boolean;
        setEnabled: (value: boolean) => void;
        hookFilterChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        hookEnabledChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        unhook: (hookResult: IEventHandle) => void;
        dispose: () => void;
        saveState: () => void;
        loadState: () => AircraftListFilter_SaveState;
        applyState: (settings: AircraftListFilter_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey();
        private createSettings();
        createOptionPane: (displayOrder: number) => OptionPane;
        addFilter: (filterOrPropertyId: string | AircraftFilter) => AircraftFilter;
        private filterPaneRemoved;
        removeFilterAt: (index: number) => void;
        getFilterIndexForProperty: (filterProperty: string) => number;
        getFilterForProperty: (filterProperty: string) => AircraftFilter;
        removeFilterForProperty: (filterProperty: string) => void;
        addFilterForOneConditionProperty: (filterProperty: string, condition: string, reverseCondition: boolean, value: any) => AircraftFilter;
        addFilterForTwoConditionProperty: (filterProperty: string, condition: string, reverseCondition: boolean, value1: any, value2: any) => AircraftFilter;
        hasFilterForOneConditionProperty: (filterProperty: string, condition: string, reverseCondition: boolean, value: any) => boolean;
        hasFilterForTwoConditionProperty: (filterProperty: string, condition: string, reverseCondition: boolean, value1: any, value2: any) => boolean;
        filterAircraft: (aircraft: Aircraft) => boolean;
        private aircraftList_FetchingList;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftListSortHandler_Settings {
        field?: AircraftListSortableFieldEnum;
        labelKey?: string;
        getNumberCallback?: (aircraft: Aircraft) => number;
        getStringCallback?: (aircraft: Aircraft) => string;
        compareCallback?: (lhs: Aircraft, rhs: Aircraft) => number;
    }
    class AircraftListSortHandler {
        Field: AircraftListSortableFieldEnum;
        LabelKey: string;
        GetNumberCallback: (aircraft: Aircraft) => number;
        GetStringCallback: (aircraft: Aircraft) => string;
        CompareCallback: (lhs: Aircraft, rhs: Aircraft) => number;
        constructor(settings: AircraftListSortHandler_Settings);
        private compareNumericValues(lhs, rhs);
        private compareStringValues(lhs, rhs);
    }
    var aircraftListSortHandlers: {
        [index: string]: AircraftListSortHandler;
    };
    var AircraftListSpecialHandlerIndex: {
        Emergency: number;
        Interesting: number;
    };
    var aircraftListSpecialHandlers: AircraftListSortHandler[];
    interface AircraftListSorter_SortField {
        field: AircraftListSortableFieldEnum;
        ascending: boolean;
    }
    interface AircraftListSorter_SaveState {
        sortFields: AircraftListSorter_SortField[];
        showEmergencySquawks: SortSpecialEnum;
        showInteresting: SortSpecialEnum;
    }
    class AircraftListSorter implements ISelfPersist<AircraftListSorter_SaveState> {
        private _Dispatcher;
        private _Events;
        private _Name;
        private _SortFields;
        private _SortFieldsLength;
        private _ShowEmergencySquawksSortSpecial;
        private _ShowInterestingSortSpecial;
        constructor(name?: string);
        getName: () => string;
        getSortFieldsCount: () => number;
        getSortField: (index: number) => AircraftListSorter_SortField;
        setSortField: (index: number, sortField: AircraftListSorter_SortField) => void;
        getSingleSortField: () => AircraftListSorter_SortField;
        setSingleSortField: (sortField: AircraftListSorter_SortField) => void;
        getShowEmergencySquawksSortSpecial: () => number;
        setShowEmergencySquawksSortSpecial: (value: number) => void;
        getShowInterestingSortSpecial: () => number;
        setShowInterestingSortSpecial: (value: number) => void;
        hookSortFieldsChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        unhook: (hookResult: IEventHandle) => void;
        saveState: () => void;
        loadState: () => AircraftListSorter_SaveState;
        applyState: (settings: AircraftListSorter_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey();
        private createSettings();
        createOptionPane: (displayOrder: number) => OptionPane;
        private buildSortSpecialValueTexts();
        sortAircraftArray: (array: Aircraft[]) => void;
    }
}
declare namespace VRS {
    interface AircraftMarker_Settings {
        folder?: string;
        normalFileName?: string;
        selectedFileName?: string;
        size?: ISize;
        isAircraft?: boolean;
        canRotate?: boolean;
        isPre22Icon?: boolean;
        matches?: (aircraft: Aircraft) => boolean;
    }
    class AircraftMarker {
        private _Settings;
        constructor(settings: AircraftMarker_Settings);
        getFolder(): string;
        setFolder(value: string): void;
        getNormalFileName(): string;
        setNormalFileName(value: string): void;
        getSelectedFileName(): string;
        setSelectedFileName(value: string): void;
        getSize(): ISize;
        setSize(value: ISize): void;
        getIsAircraft(): boolean;
        setIsAircraft(value: boolean): void;
        getCanRotate(): boolean;
        setCanRotate(value: boolean): void;
        getIsPre22Icon(): boolean;
        setIsPre22Icon(value: boolean): void;
        getMatches(): (aircraft: Aircraft) => boolean;
        setMatches(value: (aircraft: Aircraft) => boolean): void;
        matchesAircraft(aircraft: Aircraft): boolean;
    }
    var globalOptions: GlobalOptions;
    interface AircraftPlotterOptions_Settings {
        name?: string;
        showAltitudeStalk?: boolean;
        suppressAltitudeStalkWhenZoomed?: boolean;
        showPinText?: boolean;
        pinTexts?: RenderPropertyEnum[];
        pinTextLines?: number;
        hideEmptyPinTextLines?: boolean;
        trailDisplay?: TrailDisplayEnum;
        trailType?: TrailTypeEnum;
        showRangeCircles?: boolean;
        rangeCircleInterval?: number;
        rangeCircleDistanceUnit?: DistanceEnum;
        rangeCircleCount?: number;
        rangeCircleOddColour?: string;
        rangeCircleOddWeight?: number;
        rangeCircleEvenColour?: string;
        rangeCircleEvenWeight?: number;
        onlyUsePre22Icons?: boolean;
    }
    interface AircraftPlotterOptions_SaveState {
        showAltitudeStalk: boolean;
        suppressAltitudeStalkWhenZoomedOut: boolean;
        showPinText: boolean;
        pinTexts: RenderPropertyEnum[];
        pinTextLines: number;
        hideEmptyPinTextLines: boolean;
        trailDisplay: TrailDisplayEnum;
        trailType: TrailTypeEnum;
        showRangeCircles: boolean;
        rangeCircleInterval: number;
        rangeCircleDistanceUnit: DistanceEnum;
        rangeCircleCount: number;
        rangeCircleOddColour: string;
        rangeCircleOddWeight: number;
        rangeCircleEvenColour: string;
        rangeCircleEvenWeight: number;
        onlyUsePre22Icons: boolean;
    }
    class AircraftPlotterOptions implements ISelfPersist<AircraftPlotterOptions_SaveState> {
        private _Dispatcher;
        private _Events;
        private _Settings;
        private _SuppressEvents;
        private _PinTexts;
        constructor(settings: AircraftPlotterOptions_Settings);
        getName: () => string;
        getShowAltitudeStalk: () => boolean;
        setShowAltitudeStalk: (value: boolean) => void;
        getSuppressAltitudeStalkWhenZoomedOut: () => boolean;
        setSuppressAltitudeStalkWhenZoomedOut: (value: boolean) => void;
        getShowPinText: () => boolean;
        setShowPinText: (value: boolean) => void;
        getPinTexts: () => string[];
        getPinText: (index: number) => string;
        setPinText: (index: number, value: string) => void;
        getPinTextLines: () => number;
        setPinTextLines: (value: number) => void;
        getHideEmptyPinTextLines: () => boolean;
        setHideEmptyPinTextLines: (value: boolean) => void;
        getTrailDisplay: () => string;
        setTrailDisplay: (value: string) => void;
        getTrailType: () => string;
        setTrailType: (value: string) => void;
        getShowRangeCircles: () => boolean;
        setShowRangeCircles: (value: boolean) => void;
        getRangeCircleInterval: () => number;
        setRangeCircleInterval: (value: number) => void;
        getRangeCircleDistanceUnit: () => string;
        setRangeCircleDistanceUnit: (value: string) => void;
        getRangeCircleCount: () => number;
        setRangeCircleCount: (value: number) => void;
        getRangeCircleOddColour: () => string;
        setRangeCircleOddColour: (value: string) => void;
        getRangeCircleOddWeight: () => number;
        setRangeCircleOddWeight: (value: number) => void;
        getRangeCircleEvenColour: () => string;
        setRangeCircleEvenColour: (value: string) => void;
        getRangeCircleEvenWeight: () => number;
        setRangeCircleEvenWeight: (value: number) => void;
        getOnlyUsePre22Icons: () => boolean;
        setOnlyUsePre22Icons: (value: boolean) => void;
        hookPropertyChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        private raisePropertyChanged();
        hookRangeCirclePropertyChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        private raiseRangeCirclePropertyChanged();
        unhook: (hookResult: IEventHandle) => void;
        saveState: () => void;
        loadState: () => AircraftPlotterOptions_SaveState;
        applyState: (settings: AircraftPlotterOptions_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey();
        private createSettings();
        createOptionPane: (displayOrder: number) => OptionPane[];
        createOptionPaneForRangeCircles: (displayOrder: number) => OptionPane;
    }
    interface AircraftPlotter_Settings {
        plotterOptions: AircraftPlotterOptions;
        aircraftList?: AircraftList;
        map: JQuery;
        unitDisplayPreferences?: UnitDisplayPreferences;
        name?: string;
        getAircraft?: () => AircraftCollection;
        getSelectedAircraft?: () => Aircraft;
        aircraftMarkers?: AircraftMarker[];
        pinTextMarkerWidth?: number;
        pinTextLineHeight?: number;
        allowRotation?: boolean;
        rotationGranularity?: number;
        suppressAltitudeStalkAboveZoom?: number;
        normalTrailColour?: string;
        selectedTrailColour?: string;
        normalTrailWidth?: number;
        selectedTrailWidth?: number;
        showTooltips?: boolean;
        suppressTextOnImages?: boolean;
        getCustomPinTexts?: (aircraft: Aircraft) => string[];
        allowRangeCircles?: boolean;
        hideNonAircraftZoomLevel?: number;
    }
    class AircraftPlotter {
        private _Settings;
        private _Map;
        private _UnitDisplayPreferences;
        private _SuppressTextOnImages;
        private _Suspended;
        private _PlottedDetail;
        private _PreviousTrailTypeRequested;
        private _GetSelectedAircraft;
        private _GetAircraft;
        private _RangeCircleCentre;
        private _RangeCircleCircles;
        private _MovingMap;
        private _PlotterOptionsPropertyChangedHook;
        private _PlotterOptionsRangeCirclePropertyChangedHook;
        private _AircraftListUpdatedHook;
        private _AircraftListFetchingListHook;
        private _SelectedAircraftChangedHook;
        private _FlightLevelHeightUnitChangedHook;
        private _FlightLevelTransitionAltitudeChangedHook;
        private _FlightLevelTransitionHeightUnitChangedHook;
        private _HeightUnitChangedHook;
        private _SpeedUnitChangedHook;
        private _ShowVsiInSecondsHook;
        private _MapIdleHook;
        private _MapMarkerClickedHook;
        private _CurrentLocationChangedHook;
        private _ConfigurationChangedHook;
        constructor(settings: AircraftPlotter_Settings);
        getName(): string;
        getMap(): IMap;
        getMovingMap(): boolean;
        setMovingMap(value: boolean): void;
        dispose(): void;
        private configureSuppressTextOnImages();
        suspend(onOff: boolean): void;
        plot(refreshAllMarkers?: boolean, ignoreBounds?: boolean): void;
        getPlottedAircraftIds(): number[];
        private refreshMarkers(newAircraft?, oldAircraft?, alwaysRefreshIcon?, ignoreBounds?);
        private refreshAircraftMarker(aircraft, forceRefresh, ignoreBounds, bounds, mapZoomLevel, isSelectedAircraft);
        private removeOldMarkers(oldAircraft);
        private removeAllMarkers();
        private isAircraftBeingPlotted(aircraft);
        private removeDetails(details);
        private haveIconDetailsChanged(details, mapZoomLevel);
        private createIcon(details, mapZoomLevel, isSelectedAircraft);
        private getAircraftMarkerDetails(aircraft);
        private allowIconRotation();
        private getIconHeading(aircraft);
        private allowIconAltitudeStalk(mapZoomLevel);
        private getIconAltitudeStalkHeight(aircraft);
        private allowPinTexts();
        private havePinTextDependenciesChanged(aircraft);
        private getPinTexts(aircraft);
        private haveLabelDetailsChanged(details);
        private createLabel(details);
        private updateTrail(details, isAircraftSelected, forceRefresh);
        private getMonochromeTrailColour(isAircraftSelected);
        private getCoordinateTrailColour(coordinate, trailType);
        private getTrailWidth(isAircraftSelected);
        private getTrailPath(trail, start, count, aircraft, trailType, isAircraftSelected, isMonochrome);
        private createTrail(details, trail, trailType, isAircraftSelected, isMonochrome);
        private addMultiColouredPolylines(details, path, weight, fromCoord);
        private removeTrail(details);
        private synchroniseAircraftAndMapPolylinePaths(details, trailType, trail, isAircraftSelected, isMonochrome, isFullTrail);
        private trimShortTrailPoints(details, trail);
        private haveTooltipDetailsChanged(details);
        private getTooltip(details);
        private selectAircraftById(id);
        private moveMapToSelectedAircraft(selectedAircraft?);
        moveMapToAircraft(aircraft: Aircraft): void;
        refreshRangeCircles(forceRefresh?: boolean): void;
        private destroyRangeCircles();
        getAircraftMarker(aircraft: Aircraft): IMapMarker;
        getAircraftForMarkerId(mapMarkerId: number): any;
        diagnosticsGetPlottedDetail(aircraft: Aircraft): Object;
        private optionsPropertyChanged();
        private optionsRangePropertyChanged();
        private fetchingList(xhrParams);
        private refreshMarkersOnListUpdate(newAircraft, oldAircraft);
        private refreshSelectedAircraft(oldSelectedAircraft);
        private refreshMarkersIfUsingPinText(renderProperty);
        private currentLocationChanged();
        private configurationChanged();
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface AircraftRenderOptions {
        unitDisplayPreferences: UnitDisplayPreferences;
        distinguishOnGround?: boolean;
        flagUncertainCallsigns?: boolean;
        showUnits?: boolean;
        suppressRouteCorrectionLinks?: boolean;
        airportDataThumbnails?: number;
        plotterOptions?: AircraftPlotterOptions;
        mirrorMapJQ?: JQuery;
        aircraftList?: AircraftList;
    }
    interface RenderPropertyHandler_Settings {
        property: RenderPropertyEnum;
        surfaces: RenderSurfaceBitFlags;
        headingKey?: string;
        labelKey?: string;
        optionsLabelKey?: string;
        headingAlignment?: AlignmentEnum;
        contentAlignment?: AlignmentEnum;
        alignment?: AlignmentEnum;
        fixedWidth?: (surface: RenderSurfaceBitFlags) => string;
        hasChangedCallback?: (aircraft?: Aircraft) => boolean;
        contentCallback?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        renderCallback?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        useHtmlRendering?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => boolean;
        usesDisplayUnit?: (displayUnitDependency: DisplayUnitDependencyEnum) => boolean;
        tooltipChangedCallback?: (aircraft?: Aircraft) => boolean;
        tooltipCallback?: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        suppressLabelCallback?: (surface: RenderSurfaceBitFlags) => boolean;
        isMultiLine?: boolean;
        sortableField?: AircraftListSortableFieldEnum;
        createWidget?: (element?: JQuery, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;
        renderWidget?: (element?: JQuery, aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;
        destroyWidget?: (element?: JQuery, surface?: RenderSurfaceBitFlags) => void;
        suspendWidget?: (element: JQuery, surface: RenderSurfaceBitFlags, suspend: boolean) => void;
    }
    class RenderPropertyHandler {
        private _SuspendWidget;
        property: RenderPropertyEnum;
        surfaces: RenderSurfaceBitFlags;
        headingKey: string;
        labelKey: string;
        optionsLabelKey: string;
        headingAlignment: AlignmentEnum;
        contentAlignment: AlignmentEnum;
        fixedWidth: (surface: RenderSurfaceBitFlags) => string;
        hasChangedCallback: (aircraft?: Aircraft) => boolean;
        contentCallback: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        renderCallback: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        useHtmlRendering: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => boolean;
        usesDisplayUnit: (displayUnitDependency: DisplayUnitDependencyEnum) => boolean;
        tooltipChangedCallback: (aircraft?: Aircraft) => boolean;
        tooltipCallback: (aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => string;
        suppressLabelCallback: (surface: RenderSurfaceBitFlags) => boolean;
        isMultiLine: boolean;
        sortableField: AircraftListSortableFieldEnum;
        createWidget: (element?: JQuery, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;
        renderWidget: (element?: JQuery, aircraft?: Aircraft, options?: AircraftRenderOptions, surface?: RenderSurfaceBitFlags) => void;
        destroyWidget: (element?: JQuery, surface?: RenderSurfaceBitFlags) => void;
        constructor(settings: RenderPropertyHandler_Settings);
        isSurfaceSupported(surface: RenderSurfaceBitFlags): boolean;
        isWidgetProperty(): boolean;
        suspendWidget(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, onOff: boolean): void;
        createWidgetInDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags, options: AircraftRenderOptions): void;
        createWidgetInJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, options?: AircraftRenderOptions): void;
        destroyWidgetInDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags): void;
        destroyWidgetInJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags): void;
        renderToDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions, compareContent?: boolean, existingContent?: string): string;
        renderToJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions): void;
        renderTooltipToDom(domElement: HTMLElement, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions): boolean;
        renderTooltipToJQuery(jQueryElement: JQuery, surface: RenderSurfaceBitFlags, aircraft: Aircraft, options: AircraftRenderOptions): boolean;
    }
    var renderPropertyHandlers: {
        [index: string]: RenderPropertyHandler;
    };
    interface RenderPropertyHelper_ListOptionsToPane {
        pane: OptionPane;
        surface: RenderSurfaceBitFlags;
        fieldLabel: string;
        getList: () => RenderPropertyEnum[];
        setList: (properties: RenderPropertyEnum[]) => void;
        saveState: () => void;
    }
    class RenderPropertyHelper {
        buildValidRenderPropertiesList(renderPropertiesList: RenderPropertyEnum[], surfaces?: RenderSurfaceBitFlags[], maximumProperties?: number): RenderPropertyEnum[];
        getHandlersForSurface(surface: RenderSurfaceBitFlags): RenderPropertyHandler[];
        sortHandlers(handlers: RenderPropertyHandler[], putNoneFirst: boolean): void;
        addRenderPropertiesListOptionsToPane(settings: RenderPropertyHelper_ListOptionsToPane): OptionFieldOrderedSubset;
        createValueTextListForHandlers(handlers: RenderPropertyHandler[]): ValueText[];
    }
    var renderPropertyHelper: RenderPropertyHelper;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    class AirportDataApi {
        getThumbnails(icao: string, registration: string, countThumbnails: number, callback: (icao: string, data: IAirportDataThumbnails) => void): void;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface Audio_AutoPlay {
        src: string;
    }
    interface AudioWrapper_SaveState {
        announceSelected: boolean;
        announceOnlyAutoSelected: boolean;
        volume: number;
    }
    class AudioWrapper implements ISelfPersist<AudioWrapper_SaveState> {
        private _PausePhrase;
        private _BrowserSupportsAudio;
        private _AutoplayQueue;
        private _Playing;
        private _PlayingTimeout;
        private _AnnounceSelectedAircraftList;
        private _SelectedAircraftChangedHookResult;
        private _ListUpdatedHookResult;
        private _AnnouncedAircraft;
        private _AnnouncedAircraftIsUserSelected;
        private _PreviouslyAnnouncedAircraftIds;
        private _Name;
        private _AnnounceSelected;
        private _AnnounceOnlyAutoSelected;
        private _Volume;
        private _Muted;
        constructor(name?: string);
        getName: () => string;
        getAnnounceSelected: () => boolean;
        setAnnounceSelected: (value: boolean) => void;
        getAnnounceOnlyAutoSelected: () => boolean;
        setAnnounceOnlyAutoSelected: (value: boolean) => void;
        getVolume: () => number;
        setVolume: (value: number) => void;
        getMuted: () => boolean;
        setMuted: (value: boolean) => void;
        dispose: () => void;
        saveState: () => void;
        loadState: () => AudioWrapper_SaveState;
        applyState: (settings: AudioWrapper_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey();
        private createSettings();
        createOptionPane: (displayOrder: number) => OptionPane;
        isSupported: () => boolean;
        isAutoplaySupported: () => boolean;
        canPlayAudio: (mustAllowAutoPlay?: boolean) => boolean;
        private playNextEntry;
        private playSource;
        private stopPlaying;
        private stopAudioTimeout;
        private announceAircraft;
        private isPreviouslyAnnouncedAutoSelected;
        private recordPreviouslyAnnouncedAutoSelected;
        say: (text: string) => void;
        formatAcronymForSpeech: (acronym: string) => string;
        formatUpperCaseWordsForSpeech: (text: string) => string;
        formatPunctuationForSpeech: (text: string) => string;
        annouceSelectedAircraftOnList: (aircraftList: AircraftList) => void;
        private audioEnded;
        private audioTimedOut;
        private selectedAircraftChanged;
        private listUpdated;
    }
}
declare namespace VRS {
    interface Bootstrap_Settings {
        configPrefix: string;
        dispatcherName?: string;
        settingsMenuAlignment?: AlignmentEnum;
    }
    interface PageSettings_Base {
        layoutMenuItem?: MenuItem;
        localeMenuItem?: MenuItem;
        mapButton?: JQuery;
        mapJQ?: JQuery;
        mapSettings?: IMapOptions;
        menuPlugin?: MenuPlugin;
        menuJQ?: JQuery;
        settingsMenu?: Menu;
        showLayoutSetting?: boolean;
        showSettingsButton?: boolean;
        splittersJQ?: JQuery;
        unitDisplayPreferences?: UnitDisplayPreferences;
    }
    class Bootstrap {
        private _Dispatcher;
        private _Events;
        protected _Settings: Bootstrap_Settings;
        protected _PageSettings: PageSettings_Base;
        constructor(settings: Bootstrap_Settings);
        pageSettings: PageSettings_Base;
        hookAircraftDetailPanelInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseAircraftDetailPanelInitialised(pageSettings: PageSettings_Base): void;
        hookAircraftListPanelInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseAircraftListPanelInitialised(pageSettings: PageSettings_Base): void;
        hookConfigStorageInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseConfigStorageInitialised(pageSettings: PageSettings_Base): void;
        hookCreatedSettingsMenu(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseCreatedSettingsMenu(pageSettings: PageSettings_Base): void;
        hookInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseInitialised(pageSettings: PageSettings_Base): void;
        hookInitialising(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseInitialising(pageSettings: PageSettings_Base): void;
        hookLayoutsInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseLayoutsInitialised(pageSettings: PageSettings_Base): void;
        hookLocaleInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseLocaleInitialised(pageSettings: PageSettings_Base): void;
        hookMapInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseMapInitialised(pageSettings: PageSettings_Base): void;
        hookMapInitialising(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseMapInitialising(pageSettings: PageSettings_Base): void;
        hookMapLoaded(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseMapLoaded(pageSettings: PageSettings_Base): void;
        hookMapSettingsInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseMapSettingsInitialised(pageSettings: PageSettings_Base): void;
        hookOptionsPagesInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseOptionsPagesInitialised(pageSettings: PageSettings_Base): void;
        hookPageManagerInitialised(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raisePageManagerInitialised(pageSettings: PageSettings_Base): void;
        hookReportCreated(callback: (pageSettings?: PageSettings_Base, bootstrap?: Bootstrap) => void, forceThis?: Object): IEventHandle;
        protected raiseReportCreated(pageSettings: PageSettings_Base): void;
        unhook(hookResult: IEventHandle): void;
        addMapLibrary(pageSettings: PageSettings_Base, library: string): void;
        protected doStartInitialise(pageSettings: PageSettings_Base, successCallback: () => void): void;
        protected doEndInitialise(pageSettings: PageSettings_Base): void;
        protected createMapSettingsControl(pageSettings: PageSettings_Base): JQuery;
        protected createHelpMenuEntry(relativeUrl: string): MenuItem;
        protected endLayoutInitialisation(pageSettings: PageSettings_Base): void;
        protected createLayoutMenuEntry(pageSettings: PageSettings_Base, separatorIds?: string[]): MenuItem;
        protected createLocaleMenuEntry(pageSettings: PageSettings_Base): MenuItem;
    }
    var bootstrap: any;
}
declare namespace VRS {
    interface BootstrapMap_Settings extends Bootstrap_Settings {
        mapSettings?: IMapOpenOptions;
        suppressTitleUpdate?: boolean;
        settingsPosition?: MapPositionEnum;
        showOptionsInPage?: boolean;
        reportUrl?: string;
    }
    interface PageSettings_Map extends PageSettings_Base {
        aircraftAutoSelect?: AircraftAutoSelect;
        aircraftDetailJQ?: JQuery;
        aircraftDetailPagePanelJQ?: JQuery;
        aircraftList?: AircraftList;
        aircraftListFetcher?: AircraftListFetcher;
        aircraftListFilter?: AircraftListFilter;
        aircraftListJQ?: JQuery;
        aircraftListPagePanelJQ?: JQuery;
        aircraftListSorter?: AircraftListSorter;
        aircraftPlotter?: AircraftPlotter;
        aircraftPlotterOptions?: AircraftPlotterOptions;
        audio?: AudioWrapper;
        infoWindowJQ?: JQuery;
        infoWindowPlugin?: AircraftInfoWindowPlugin;
        mapNextPageButton?: JQuery;
        mapPlugin?: IMap;
        optionsPagePanelJQ?: JQuery;
        pages?: OptionPage[];
        pagesJQ?: JQuery;
        polarPlotter: PolarPlotter;
        showAudioSetting?: boolean;
        showAutoSelectToggle?: boolean;
        showGotoCurrentLocation?: boolean;
        showGotoSelectedAircraft?: boolean;
        showLanguageSetting?: boolean;
        showMovingMapSetting?: boolean;
        showOptionsSetting?: boolean;
        showPauseSetting?: boolean;
        showRangeCircleSetting?: boolean;
        showReceiversShortcut?: boolean;
        showReportLinks?: boolean;
        timeoutMessageBox?: JQuery;
        titleUpdater?: TitleUpdater;
    }
    class BootstrapMap extends Bootstrap {
        protected _Settings: BootstrapMap_Settings;
        constructor(settings: BootstrapMap_Settings);
        initialise(pageSettings: PageSettings_Map): void;
        private mapLoaded(pageSettings);
        private buildSettingsMenu(pageSettings, menuItems);
        private createOptionsMenuEntry(pageSettings);
        private createShortcutsMenuEntry(pageSettings);
        private createReceiversMenuEntry(pageSettings);
        private createPolarPlotterMenuEntry(pageSettings);
        private createMovingMapMenuEntry(pageSettings);
        private createRangeCirclesMenuEntry(pageSettings);
        private createPauseMenuEntry(pageSettings);
        private createGotoCurrentLocationMenuEntry(pageSettings);
        private createGotoSelectedAircraftMenuEntry(pageSettings);
        private createAutoSelectMenuEntry(pageSettings);
        private createAudioMenuEntry(pageSettings);
        private createReportsMenuEntry(pageSettings);
        private initialiseAircraftDetailPanel(pageSettings);
        private initialiseAircraftListPanel(pageSettings);
        private buildOptionPanelPages(pageSettings);
        private initialiseFsxLayout(pageSettings);
        private initialisePageLayouts(pageSettings);
        private initialisePageManager(pageSettings);
    }
}
declare namespace VRS {
    class BootstrapMapDesktop extends BootstrapMap {
        constructor();
    }
}
declare namespace VRS {
    class BootstrapMapFsx extends BootstrapMap {
        constructor();
    }
}
declare namespace VRS {
    class BootstrapMapMobile extends BootstrapMap {
        constructor();
    }
}
declare namespace VRS {
    interface BootstrapReport_Settings extends Bootstrap_Settings {
        suppressTitleUpdate?: boolean;
        settingsPosition?: MapPositionEnum;
        showOptionsInPage?: boolean;
    }
    interface PageSettings_Report extends PageSettings_Base {
        detailJQ?: JQuery;
        detailPlugin?: ReportDetailPlugin;
        listJQ?: JQuery;
        listPlugin?: ReportListPlugin;
        mapPlugin?: ReportMapPlugin;
        optionsPagePanelJQ?: JQuery;
        pages?: OptionPage[];
        pagesJQ?: JQuery;
        plotterOptions?: AircraftPlotterOptions;
        report?: Report;
        reportName: string;
        showLanguageSetting?: boolean;
        showOptionsSetting?: boolean;
    }
    class BootstrapReport extends Bootstrap {
        protected _Settings: BootstrapReport_Settings;
        constructor(settings: BootstrapReport_Settings);
        initialise(userPageSettings: PageSettings_Report): void;
        private mapCreated(pageSettings);
        private buildSettingsMenu(pageSettings, menuItems);
        private createCriteriaMenuEntry(pageSettings);
        private initialisePageLayouts(pageSettings);
        private initialisePageManager(pageSettings);
        private buildCriteriaOptionPanelPages(pageSettings);
    }
}
declare namespace VRS {
    class BootstrapReportDesktop extends BootstrapReport {
        constructor();
    }
}
declare namespace VRS {
    class BootstrapReportMobile extends BootstrapReport {
        constructor();
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface ConfigStorage_ImportOptions {
        overwrite?: boolean;
        resetBeforeImport?: boolean;
        ignoreLanguage?: boolean;
        ignoreSplitters?: boolean;
        ignoreCurrentLocation?: boolean;
        ignoreAutoSelect?: boolean;
        ignoreRequestFeedId?: boolean;
    }
    class ConfigStorage {
        private _VRSKeyPrefix;
        private _Prefix;
        getPrefix(): string;
        setPrefix(prefix: string): void;
        getStorageSize(): number;
        getStorageEngine(): string;
        getHasSettings(): boolean;
        warnIfMissing(): void;
        load(key: string, defaultValue?: any): any;
        loadWithoutPrefix(key: string, defaultValue?: any): any;
        private doLoad(key, defaultValue?, ignorePrefix?);
        save(key: string, value: any): void;
        saveWithoutPrefix(key: string, value: any): void;
        private doSave(key, value, ignorePrefix);
        erase(key: string, ignorePrefix: boolean): void;
        cleanupOldStorage(): void;
        private eraseCookie(name);
        normaliseKey(key: string, ignorePrefix: boolean): string;
        getAllVirtualRadarKeys(stripVrsPrefix?: boolean): string[];
        getContentWithoutPrefix(key: string): any;
        removeContentWithoutPrefix(key: string): void;
        removeAllContent(): void;
        exportSettings(): string;
        importSettings(exportString: string, options: ConfigStorage_ImportOptions): void;
        private isValidImportKey(keyName, options);
        private adjustImportValues(keyName, value, options);
    }
    var configStorage: ConfigStorage;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface CurrentLocation_Settings {
        name?: string;
        mapForApproximateLocation?: JQuery;
    }
    interface CurrentLocation_SaveState {
        userSuppliedLocation?: ILatLng;
        useBrowserLocation?: boolean;
        showCurrentLocation?: boolean;
    }
    class CurrentLocation implements ISelfPersist<CurrentLocation_SaveState> {
        private _Dispatcher;
        private _Events;
        private _SetCurrentLocationMarker;
        private _CurrentLocationMarker;
        private _PlottedCurrentLocation;
        private _MapForDisplay;
        private _MapForApproximateLocationPlugin;
        private _MapForApproximateLocationCentreChangedHookResult;
        private _MapMarkerDraggedHookResult;
        private _Name;
        private _CurrentLocation;
        private _GeoLocationAvailable;
        private _LastBrowserLocation;
        private _GeoLocationHandlersInstalled;
        private _UseBrowserLocation;
        private _UserSuppliedCurrentLocation;
        private _MapCentreLocation;
        private _MapForApproximateLocation;
        private _ShowCurrentLocationOnMap;
        constructor(settings?: CurrentLocation_Settings);
        getName: () => string;
        getCurrentLocation: () => ILatLng;
        setCurrentLocation: (value: ILatLng) => void;
        getGeoLocationAvailable: () => boolean;
        getLastBrowserLocation: () => ILatLng;
        getUseBrowserLocation: () => boolean;
        setUseBrowserLocation: (value: boolean) => void;
        private useBrowserPosition;
        getBrowserIsSupplyingLocation: () => boolean;
        getUserSuppliedCurrentLocation: () => ILatLng;
        setUserSuppliedCurrentLocation: (value: ILatLng) => void;
        getUserHasAssignedCurrentLocation: () => boolean;
        getMapCentreLocation: () => ILatLng;
        setMapCentreLocation: (value: ILatLng) => void;
        getMapIsSupplyingLocation: () => boolean;
        getMapForApproximateLocation: () => JQuery;
        setMapForApproximateLocation: (value: JQuery) => void;
        getIsSetCurrentLocationMarkerDisplayed: () => boolean;
        setIsSetCurrentLocationMarkerDisplayed: (value: boolean) => void;
        getShowCurrentLocationOnMap: () => boolean;
        setShowCurrentLocationOnMap: (value: boolean) => void;
        hookCurrentLocationChanged: (callback: () => void, forceThis?: Object) => IEventHandle;
        unhook: (hookResult: IEventHandle) => void;
        dispose: () => void;
        saveState: () => void;
        loadState: () => CurrentLocation_SaveState;
        applyState: (settings: CurrentLocation_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey();
        private createSettings();
        createOptionPane: (displayOrder: number, mapForLocationDisplay: JQuery) => OptionPane;
        private determineLocationFromMap;
        private showOrHideSetCurrentLocationMarker;
        private setCurrentLocationMarkerDragged;
        private mapForApproximateLocationCentreChanged;
        private showCurrentLocationOnMap;
        private destroyCurrentLocationMarker;
    }
    var currentLocation: CurrentLocation;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    type AircraftFilterPropertyEnum = string;
    var AircraftFilterProperty: {
        Airport: string;
        Altitude: string;
        Callsign: string;
        Country: string;
        Distance: string;
        EngineType: string;
        HideNoPosition: string;
        Icao: string;
        IsMilitary: string;
        ModelIcao: string;
        Operator: string;
        OperatorCode: string;
        Registration: string;
        Species: string;
        Squawk: string;
        UserInterested: string;
        Wtc: string;
    };
    type AircraftListSortableFieldEnum = string;
    var AircraftListSortableField: {
        None: string;
        Altitude: string;
        AltitudeType: string;
        AverageSignalLevel: string;
        Bearing: string;
        Callsign: string;
        CivOrMil: string;
        CountMessages: string;
        Country: string;
        Distance: string;
        FlightsCount: string;
        Heading: string;
        HeadingType: string;
        Icao: string;
        Latitude: string;
        Longitude: string;
        Manufacturer: string;
        Model: string;
        ModelIcao: string;
        Operator: string;
        OperatorIcao: string;
        Receiver: string;
        Registration: string;
        Serial: string;
        SignalLevel: string;
        Speed: string;
        SpeedType: string;
        Squawk: string;
        TargetAltitude: string;
        TargetHeading: string;
        TimeTracked: string;
        TransponderType: string;
        UserTag: string;
        VerticalSpeed: string;
        VerticalSpeedType: string;
        YearBuilt: string;
    };
    type AircraftListSourceEnum = number;
    var AircraftListSource: {
        Unknown: number;
        BaseStation: number;
        FakeAircraftList: number;
        FlightSimulatorX: number;
    };
    type AircraftPictureServerSize = string;
    var AircraftPictureServerSize: {
        DesktopDetailPanel: string;
        IPhoneDetail: string;
        IPadDetail: string;
        List: string;
        Original: string;
    };
    type AlignmentEnum = string;
    var Alignment: {
        Left: string;
        Centre: string;
        Right: string;
    };
    type AltitudeTypeEnum = number;
    var AltitudeType: {
        Barometric: number;
        Geometric: number;
    };
    type DisplayUnitDependencyEnum = string;
    var DisplayUnitDependency: {
        Height: string;
        Speed: string;
        Distance: string;
        VsiSeconds: string;
        FLTransitionAltitude: string;
        FLTransitionHeightUnit: string;
        FLHeightUnit: string;
        Angle: string;
    };
    type DistanceEnum = string;
    var Distance: {
        Kilometre: string;
        StatuteMile: string;
        NauticalMile: string;
    };
    type EngineTypeEnum = number;
    var EngineType: {
        None: number;
        Piston: number;
        Turbo: number;
        Jet: number;
        Electric: number;
    };
    type EnginePlacementEnum = number;
    var EnginePlacement: {
        Unknown: number;
        AftMounted: number;
        WingBuried: number;
        FuselageBuried: number;
        NoseMounted: number;
        WingMounted: number;
    };
    type FilterConditionEnum = string;
    var FilterCondition: {
        Equals: string;
        Contains: string;
        Between: string;
        Starts: string;
        Ends: string;
    };
    type FilterPropertyTypeEnum = string;
    var FilterPropertyType: {
        OnOff: string;
        TextMatch: string;
        NumberRange: string;
        EnumMatch: string;
        DateRange: string;
        TextListMatch: string;
    };
    type HeightEnum = string;
    var Height: {
        Metre: string;
        Feet: string;
    };
    type InputWidthEnum = string;
    var InputWidth: {
        Auto: string;
        OneChar: string;
        ThreeChar: string;
        SixChar: string;
        EightChar: string;
        NineChar: string;
        Long: string;
    };
    type LabelWidthEnum = number;
    var LabelWidth: {
        Auto: number;
        Short: number;
        Long: number;
    };
    type LinkSiteEnum = string;
    var LinkSite: {
        None: string;
        AirframesDotOrg: string;
        AirlinersDotNet: string;
        AirportDataDotCom: string;
        StandingDataMaintenance: string;
    };
    type MapControlStyleEnum = string;
    var MapControlStyle: {
        Default: string;
        DropdownMenu: string;
        HorizontalBar: string;
    };
    type MapPositionEnum = string;
    var MapPosition: {
        BottomCentre: string;
        BottomLeft: string;
        BottomRight: string;
        LeftBottom: string;
        LeftCentre: string;
        LeftTop: string;
        RightBottom: string;
        RightCentre: string;
        RightTop: string;
        TopCentre: string;
        TopLeft: string;
        TopRight: string;
    };
    type MapTypeEnum = string;
    var MapType: {
        Hybrid: string;
        RoadMap: string;
        Satellite: string;
        Terrain: string;
        HighContrast: string;
    };
    type MobilePageNameEnum = string;
    var MobilePageName: {
        Map: string;
        AircraftDetail: string;
        AircraftList: string;
        Options: string;
    };
    type OffRadarActionEnum = string;
    var OffRadarAction: {
        Nothing: string;
        WaitForReturn: string;
        EnableAutoSelect: string;
    };
    type RenderPropertyEnum = string;
    var RenderProperty: {
        None: string;
        AirportDataThumbnails: string;
        Altitude: string;
        AltitudeAndVerticalSpeed: string;
        AltitudeType: string;
        AverageSignalLevel: string;
        Bearing: string;
        Callsign: string;
        CallsignAndShortRoute: string;
        CivOrMil: string;
        CountMessages: string;
        Country: string;
        Distance: string;
        Engines: string;
        FlightLevel: string;
        FlightLevelAndVerticalSpeed: string;
        FlightsCount: string;
        Heading: string;
        HeadingType: string;
        Icao: string;
        Interesting: string;
        Latitude: string;
        Longitude: string;
        Manufacturer: string;
        Mlat: string;
        Model: string;
        ModelIcao: string;
        Operator: string;
        OperatorFlag: string;
        OperatorIcao: string;
        Picture: string;
        PictureOrThumbnails: string;
        PositionOnMap: string;
        Receiver: string;
        Registration: string;
        RegistrationAndIcao: string;
        RouteFull: string;
        RouteShort: string;
        Serial: string;
        SignalLevel: string;
        Silhouette: string;
        SilhouetteAndOpFlag: string;
        Species: string;
        Speed: string;
        SpeedType: string;
        Squawk: string;
        TargetAltitude: string;
        TargetHeading: string;
        TimeTracked: string;
        Tisb: string;
        TransponderType: string;
        TransponderTypeFlag: string;
        UserTag: string;
        VerticalSpeed: string;
        VerticalSpeedType: string;
        Wtc: string;
        YearBuilt: string;
    };
    type RenderSurfaceBitFlags = number;
    var RenderSurface: {
        List: number;
        DetailHead: number;
        DetailBody: number;
        Marker: number;
        InfoWindow: number;
    };
    type ReportAircraftPropertyEnum = string;
    var ReportAircraftProperty: {
        AircraftClass: string;
        CofACategory: string;
        CofAExpiry: string;
        Country: string;
        CurrentRegDate: string;
        DeRegDate: string;
        Engines: string;
        FirstRegDate: string;
        GenericName: string;
        Icao: string;
        Interesting: string;
        Manufacturer: string;
        Military: string;
        Model: string;
        ModelIcao: string;
        ModeSCountry: string;
        MTOW: string;
        Notes: string;
        Operator: string;
        OperatorFlag: string;
        OperatorIcao: string;
        OwnershipStatus: string;
        Picture: string;
        PopularName: string;
        PreviousId: string;
        Registration: string;
        SerialNumber: string;
        Silhouette: string;
        Species: string;
        Status: string;
        TotalHours: string;
        WakeTurbulenceCategory: string;
        YearBuilt: string;
    };
    type ReportFilterPropertyEnum = string;
    var ReportFilterProperty: {
        Callsign: string;
        Country: string;
        Date: string;
        FirstAltitude: string;
        HadEmergency: string;
        Icao: string;
        IsMilitary: string;
        LastAltitude: string;
        ModelIcao: string;
        Operator: string;
        Species: string;
        Registration: string;
        WakeTurbulenceCategory: string;
    };
    type ReportFlightPropertyEnum = string;
    var ReportFlightProperty: {
        Altitude: string;
        Callsign: string;
        CountAdsb: string;
        CountModeS: string;
        CountPositions: string;
        Duration: string;
        EndTime: string;
        FirstAltitude: string;
        FirstFlightLevel: string;
        FirstHeading: string;
        FirstLatitude: string;
        FirstLongitude: string;
        FirstOnGround: string;
        FirstSpeed: string;
        FirstSquawk: string;
        FirstVerticalSpeed: string;
        FlightLevel: string;
        HadAlert: string;
        HadEmergency: string;
        HadSPI: string;
        LastAltitude: string;
        LastFlightLevel: string;
        LastHeading: string;
        LastLatitude: string;
        LastLongitude: string;
        LastOnGround: string;
        LastSpeed: string;
        LastSquawk: string;
        LastVerticalSpeed: string;
        PositionsOnMap: string;
        RouteShort: string;
        RouteFull: string;
        RowNumber: string;
        Speed: string;
        Squawk: string;
        StartTime: string;
    };
    type ReportAircraftOrFlightPropertyEnum = (ReportAircraftPropertyEnum | ReportFlightPropertyEnum);
    type ReportSortColumnEnum = string;
    var ReportSortColumn: {
        None: string;
        Callsign: string;
        Country: string;
        Date: string;
        FirstAltitude: string;
        Icao: string;
        LastAltitude: string;
        Model: string;
        ModelIcao: string;
        Operator: string;
        Registration: string;
    };
    type ReportSurfaceBitFlags = number;
    var ReportSurface: {
        List: number;
        DetailHead: number;
        DetailBody: number;
    };
    type SortSpecialEnum = number;
    var SortSpecial: {
        Neither: number;
        First: number;
        Last: number;
    };
    type SpeciesEnum = number;
    var Species: {
        None: number;
        LandPlane: number;
        SeaPlane: number;
        Amphibian: number;
        Helicopter: number;
        Gyrocopter: number;
        Tiltwing: number;
        GroundVehicle: number;
        Tower: number;
    };
    type SpeedEnum = string;
    var Speed: {
        Knots: string;
        MilesPerHour: string;
        KilometresPerHour: string;
    };
    type SpeedTypeEnum = number;
    var SpeedType: {
        Ground: number;
        GroundReversing: number;
        IndicatedAirSpeed: number;
        TrueAirSpeed: number;
    };
    type TrailDisplayEnum = string;
    var TrailDisplay: {
        None: string;
        SelectedOnly: string;
        AllAircraft: string;
    };
    type TrailTypeEnum = string;
    var TrailType: {
        Short: string;
        Full: string;
        ShortAltitude: string;
        FullAltitude: string;
        ShortSpeed: string;
        FullSpeed: string;
    };
    type TransponderTypeEnum = number;
    var TransponderType: {
        Unknown: number;
        ModeS: number;
        Adsb: number;
        Adsb0: number;
        Adsb1: number;
        Adsb2: number;
    };
    type WakeTurbulenceCategoryEnum = number;
    var WakeTurbulenceCategory: {
        None: number;
        Light: number;
        Medium: number;
        Heavy: number;
    };
}
declare namespace VRS {
    var globalEvent: any;
    interface IEventHandle {
        eventName: string;
        callback: Function;
    }
    interface IEventHandleJQueryUI {
        eventName: string;
    }
    interface IEventHandler {
        callback: Function;
        forceThis: Object;
    }
    interface EventHandler_Settings {
        name?: string;
        logLevel?: number;
    }
    class EventHandler {
        private _Settings;
        constructor(settings?: EventHandler_Settings);
        private _Events;
        hook(eventName: string, callback: Function, forceThis?: Object): IEventHandle;
        unhook(hookResult: IEventHandle): void;
        raise(eventName: string, args?: any[]): void;
        hookJQueryUIPluginEvent(pluginElement: JQuery, pluginName: string, eventName: string, callback: Function, forceThis?: any): IEventHandleJQueryUI;
        unhookJQueryUIPluginEvent(pluginElement: JQuery, hookResult: IEventHandleJQueryUI): void;
    }
    var globalDispatch: EventHandler;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface Filter_Options {
        caseInsensitive?: boolean;
    }
    interface IValueCondition {
        getCondition(): FilterConditionEnum;
        setCondition(filter: FilterConditionEnum): any;
        getReverseCondition(): boolean;
        setReverseCondition(reverseCondition: boolean): any;
        equals(other: IValueCondition): boolean;
        clone(): IValueCondition;
        toSerialisableObject(): ISerialisedCondition;
        applySerialisedObject(obj: ISerialisedCondition): any;
    }
    abstract class ValueCondition implements IValueCondition {
        protected _Condition: FilterConditionEnum;
        protected _ReverseCondition: boolean;
        constructor(condition: FilterConditionEnum, reverseCondition?: boolean);
        getCondition(): FilterConditionEnum;
        setCondition(value: FilterConditionEnum): void;
        getReverseCondition(): boolean;
        setReverseCondition(value: boolean): void;
        abstract equals(other: ValueCondition): boolean;
        abstract clone(): ValueCondition;
        abstract toSerialisableObject(): ISerialisedCondition;
        abstract applySerialisedObject(obj: ISerialisedCondition): any;
    }
    class OneValueCondition extends ValueCondition {
        private _Value;
        constructor(condition: FilterConditionEnum, reverseCondition?: boolean, value?: any);
        getValue(): any;
        setValue(value: any): void;
        equals(obj: OneValueCondition): boolean;
        clone(): OneValueCondition;
        toSerialisableObject(): ISerialisedOneValueCondition;
        applySerialisedObject(settings: ISerialisedOneValueCondition): void;
    }
    class TwoValueCondition extends ValueCondition {
        private _Value1;
        private _Value2;
        private _Value1IsLow;
        constructor(condition: FilterConditionEnum, reverseCondition?: boolean, value1?: any, value2?: any);
        getValue1(): any;
        setValue1(value: any): void;
        getValue2(): any;
        setValue2(value: any): void;
        getLowValue(): any;
        getHighValue(): any;
        private orderValues();
        equals(obj: TwoValueCondition): boolean;
        clone(): TwoValueCondition;
        toSerialisableObject(): ISerialisedTwoValueCondition;
        applySerialisedObject(settings: ISerialisedTwoValueCondition): void;
    }
    interface Condition {
        condition: FilterConditionEnum;
        reverseCondition: boolean;
        labelKey: string;
    }
    interface FilterPropertyTypeHandler_Settings {
        type: FilterPropertyTypeEnum;
        getConditions?: () => Condition[];
        createValueCondition: () => ValueCondition;
        valuePassesCallback?: (value: any, valueCondition: ValueCondition, options?: Filter_Options) => boolean;
        useSingleValueEquals?: boolean;
        useValueBetweenRange?: boolean;
        createOptionFieldsCallback: (labelKey: string, valueCondition: ValueCondition, handler: FilterPropertyHandler, saveState: () => void) => OptionField[];
        parseString: (str: string) => any;
        toQueryString: (value: any) => string;
        passEmptyValues?: (valueCondition: ValueCondition) => boolean;
    }
    class FilterPropertyTypeHandler {
        type: FilterPropertyTypeEnum;
        getConditions: () => Condition[];
        createValueCondition: () => ValueCondition;
        createOptionFieldsCallback: (labelKey: string, valueCondition: ValueCondition, handler: FilterPropertyHandler, saveState: () => void) => OptionField[];
        valuePassesCallback: (value: any, valueCondition: ValueCondition, options?: Filter_Options) => boolean;
        parseString: (str: string) => any;
        passEmptyValues: (valueCondition: ValueCondition) => boolean;
        toQueryString: (value: any) => string;
        constructor(settings: FilterPropertyTypeHandler_Settings);
        private singleValueEquals(value, valueCondition);
        private valueBetweenRange(value, valueCondition);
        getConditionComboBoxValues(): ValueText[];
        encodeConditionAndReverseCondition(condition: FilterConditionEnum, reverseCondition: boolean): string;
        decodeConditionAndReverseCondition(encodedConditionAndReverse: string): Condition;
        encodeCondition(valueCondition: ValueCondition): string;
        applyEncodedCondition(valueCondition: ValueCondition, encodedCondition: string): void;
    }
    var filterPropertyTypeHandlers: {
        [index: string]: FilterPropertyTypeHandler;
    };
    interface FilterPropertyHandler_Settings {
        property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum;
        propertyEnumObject?: Object;
        type: FilterPropertyTypeEnum;
        labelKey: string;
        getValueCallback?: (parameter: any) => any;
        getEnumValues?: () => ValueText[];
        isUpperCase?: boolean;
        isLowerCase?: boolean;
        minimumValue?: number;
        maximumValue?: number;
        decimalPlaces?: number;
        inputWidth?: InputWidthEnum;
        isServerFilter?: (valueCondition: ValueCondition) => boolean;
        serverFilterName?: string;
        normaliseValue?: (value: any, unitDisplayPrefs: UnitDisplayPreferences) => any;
        defaultCondition?: FilterConditionEnum;
    }
    class FilterPropertyHandler {
        property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum;
        type: FilterPropertyTypeEnum;
        labelKey: string;
        getValueCallback: (parameter: any) => any;
        getEnumValues: () => ValueText[];
        isUpperCase: boolean;
        isLowerCase: boolean;
        minimumValue: number;
        maximumValue: number;
        decimalPlaces: number;
        inputWidth: InputWidthEnum;
        serverFilterName: string;
        isServerFilter: (valueCondition: ValueCondition) => boolean;
        normaliseValue: (value: any, unitDisplayPrefs: UnitDisplayPreferences) => any;
        defaultCondition: FilterConditionEnum;
        constructor(settings: FilterPropertyHandler_Settings);
        getFilterPropertyTypeHandler(): FilterPropertyTypeHandler;
    }
    interface Filter_Settings {
        property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum;
        valueCondition: ValueCondition;
        propertyEnumObject: Object;
        filterPropertyHandlers: {
            [index: string]: FilterPropertyHandler;
        };
        cloneCallback: (obj: AircraftFilterPropertyEnum | ReportFilterPropertyEnum, valueCondition: ValueCondition) => Filter;
    }
    class Filter {
        protected _Settings: Filter_Settings;
        constructor(settings: Filter_Settings);
        getProperty(): AircraftFilterPropertyEnum | ReportFilterPropertyEnum;
        setProperty(value: AircraftFilterPropertyEnum | ReportFilterPropertyEnum): void;
        getValueCondition(): ValueCondition;
        setValueCondition(value: ValueCondition): void;
        getPropertyHandler(): FilterPropertyHandler;
        equals(obj: Filter): boolean;
        clone(): Filter;
        toSerialisableObject(): ISerialisedFilter;
        applySerialisedObject(settings: ISerialisedFilter): void;
        createOptionPane(saveState: () => any): OptionPane;
        addToQueryParameters(query: Object, unitDisplayPreferences: UnitDisplayPreferences): void;
    }
    interface FilterHelper_AddConfigureSettings {
        pane: OptionPane;
        filters: Filter[];
        fieldName?: string;
        saveState: () => any;
        maxFilters?: number;
        allowableProperties?: AircraftFilterPropertyEnum[] | ReportFilterPropertyEnum[];
        defaultProperty?: AircraftFilterPropertyEnum | ReportFilterPropertyEnum;
        addFilter?: (property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum, paneListField: OptionFieldPaneList) => void;
        addFilterButtonLabel?: string;
        onlyUniqueFilters?: boolean;
        isAlreadyInUse?: (property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum) => boolean;
    }
    interface FilterHelper_Settings {
        propertyEnumObject: Object;
        filterPropertyHandlers: {
            [index: string]: FilterPropertyHandler;
        };
        createFilterCallback: (filterPropertyHandler: FilterPropertyHandler, valueCondition: ValueCondition) => Filter;
        addToQueryParameters?: (filters: Filter[], query: Object) => void;
    }
    class FilterHelper {
        private _Settings;
        constructor(settings: FilterHelper_Settings);
        createFilter(property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum): Filter;
        serialiseFiltersList(filters: Filter[]): ISerialisedFilter[];
        buildValidFiltersList(serialisedFilters: ISerialisedFilter[], maximumFilters?: number): ISerialisedFilter[];
        addConfigureFiltersListToPane(settings: FilterHelper_AddConfigureSettings): OptionFieldPaneList;
        isFilterInUse(filters: Filter[], property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum): boolean;
        getFilterForFilterProperty(filters: Filter[], property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum): Filter;
        getIndexForFilterProperty(filters: Filter[], property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum): number;
        addToQueryParameters(filters: Filter[], query: Object, unitDisplayPreferences: UnitDisplayPreferences): void;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    class Format {
        airportDataThumbnails(airportDataThumbnails: IAirportDataThumbnails, showLinkToSite: boolean): string;
        aircraftClass(aircraftClass: string): string;
        altitude(altitude: number, altitudeType: AltitudeTypeEnum, isOnGround: boolean, heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean): string;
        altitudeFromTo(firstAltitude: number, firstIsOnGround: boolean, lastAltitude: number, lastIsOnGround: boolean, heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean): string;
        altitudeType(altitudeType: AltitudeTypeEnum): string;
        private formatAltitudeType(altitudeType);
        averageSignalLevel(avgSignalLevel: number): string;
        bearingFromHere(bearingFromHere: number, showUnits: boolean): string;
        bearingFromHereImage(bearingFromHere: number): string;
        callsign(callsign: string, callsignSuspect: boolean, showUncertainty: boolean): string;
        certOfACategory(cofACategory: string): string;
        certOfAExpiry(cofAExpiry: string): string;
        countFlights(countFlights: number, format?: string): string;
        countMessages(countMessages: number, format?: string): string;
        country(country: string): string;
        currentRegistrationDate(currentRegDate: string): string;
        deregisteredDate(deregDate: string): string;
        distanceFromHere(distanceFromHere: number, distanceUnit: DistanceEnum, showUnits: boolean): string;
        duration(elapsedTicks: number, showZeroHours: boolean): string;
        endDateTime(startDate: Date, endDate: Date, showFullDate: boolean, alwaysShowEndDate: boolean): string;
        engines(countEngines: string, engineType: EngineTypeEnum): string;
        firstRegistrationDate(firstRegDate: string): string;
        flightLevel(altitude: number, altitudeType: AltitudeTypeEnum, isOnGround: boolean, transitionAltitude: number, transitionAltitudeUnit: HeightEnum, flightLevelAltitudeUnit: HeightEnum, altitudeUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean): string;
        flightLevelFromTo(firstAltitude: number, firstIsOnGround: boolean, lastAltitude: number, lastIsOnGround: boolean, transitionAltitude: number, transitionAltitudeUnit: HeightEnum, flightLevelAltitudeUnit: HeightEnum, altitudeUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean): string;
        genericName(genericName: string): string;
        hadAlert(hadAlert: boolean): string;
        hadEmergency(hadEmergency: boolean): string;
        hadSPI(hadSPI: boolean): string;
        heading(heading: number, headingIsTrue: boolean, showUnit: boolean, showType: boolean): string;
        headingType(headingIsTrue: boolean): string;
        icao(icao: string): string;
        isMlat(isMlat: boolean): string;
        isTisb(isTisb: boolean): string;
        isOnGround(isOnGround: boolean): string;
        isMilitary(isMilitary: boolean): string;
        latitude(latitude: number, showUnit: boolean): string;
        longitude(longitude: number, showUnit: boolean): string;
        private formatLatitudeLongitude(value, showUnit);
        manufacturer(manufacturer: string): string;
        maxTakeoffWeight(mtow: string): string;
        model(model: string): string;
        modelIcao(modelIcao: string): string;
        modelIcaoImageHtml(modelIcao: string, icao: string, registration: string): string;
        modelIcaoNameAndDetail(modelIcao: string, model: string, countEngines: string, engineType: EngineTypeEnum, species: number, wtc: number): string;
        modeSCountry(modeSCountry: string): string;
        notes(notes: string): string;
        operator(operator: string): string;
        operatorIcao(operatorIcao: string): string;
        operatorIcaoAndName(operator: string, operatorIcao: string): string;
        operatorIcaoImageHtml(operator: string, operatorIcao: string, icao: string, registration: string): string;
        private buildLogoCodeToUse(logoCode, icao, registration);
        ownershipStatus(ownershipStatus: string): string;
        pictureHtml(registration: string, icao: string, picWidth: number, picHeight: number, requestSize: ISizePartial, allowResizeUp?: boolean, linkToOriginal?: boolean, blankSize?: ISize): string;
        private calculatedPictureSizes(isHighDpi, picWidth, picHeight, requestSize, blankSize, allowResizeUp);
        popularName(popularName: string): string;
        previousId(previousId: string): string;
        receiver(receiverId: number, aircraftListFetcher: AircraftListFetcher): string;
        registration(registration: string, onlyAlphaNumeric?: boolean): string;
        routeAirportCode(route: string): string;
        routeFull(callsign: string, from: string, to: string, via: string[]): string;
        reportRouteFull(callsign: string, route: IReportRoute): string;
        routeMultiLine(callsign: string, from: string, to: string, via: string[]): string;
        reportRouteMultiLine(callsign: string, route: IReportRoute): string;
        routeShort(callsign: string, from: string, to: string, via: string[], abbreviateStopovers: boolean, showRouteNotKnown: boolean): string;
        reportRouteShort(callsign: string, route: IReportRoute, abbreviateStopovers: boolean, showRouteNotKnown: boolean): string;
        serial(serial: string): string;
        startDateTime(startDate: Date, showFullDate: boolean, justShowTime: boolean): string;
        status(status: string): string;
        secondsTracked(secondsTracked: number): string;
        signalLevel(signalLevel: number): string;
        species(species: SpeciesEnum, ignoreNone?: boolean): string;
        speed(speed: number, speedType: SpeedTypeEnum, speedUnit: SpeedEnum, showUnit: boolean, showType: boolean): string;
        speedFromTo(fromSpeed: number, toSpeed: number, speedUnit: SpeedEnum, showUnits: boolean): string;
        speedType(speedType: SpeedTypeEnum): string;
        squawk(squawk: string): string;
        squawk(squawk: number): string;
        squawkDescription(squawk: string): string;
        squawkDescription(squawk: number): string;
        squawkFromTo(fromSquawk: string, toSquawk: string): string;
        squawkFromTo(fromSquawk: number, toSquawk: number): string;
        stackedValues: (topValue: string, bottomValue: string, tag?: string) => string;
        totalHours(totalHours: string): string;
        transponderType(transponderType: TransponderTypeEnum): string;
        transponderTypeImageHtml(transponderType: TransponderTypeEnum): string;
        userInterested(userInterested: boolean): string;
        userTag(userTag: string): string;
        verticalSpeed(verticalSpeed: number, verticalSpeedType: AltitudeTypeEnum, heightUnit: HeightEnum, perSecond: boolean, showUnit?: boolean, showType?: boolean): string;
        verticalSpeedType(verticalSpeedType: AltitudeTypeEnum): string;
        wakeTurbulenceCat(wtc: WakeTurbulenceCategoryEnum, ignoreNone: boolean, expandedDescription: boolean): string;
        yearBuilt(yearBuilt: string): string;
        private extractReportRouteStrings(route);
        private formatReportAirport(airport);
        private formatStartEndDate(startDate, endDate, showFullDates, showStartDate, onlyShowStartTime, showEndDate, alwaysShowEndDate);
        private formatFromTo(first, last, fromToFormat);
    }
    var format: Format;
}
declare namespace VRS {
    interface Layout_Options {
        name: string;
        vertical: boolean;
        savePane: number;
        collapsePane?: number;
        maxPane?: number;
        max?: number | string;
        startSizePane?: number;
        startSize?: number | string;
        fixedPane?: number;
    }
    type Layout_Array = [JQuery | any[], Layout_Options, JQuery | any[]];
    interface Layout_Settings {
        name: string;
        labelKey: string;
        layout: Layout_Array;
        onFocus?: () => void;
        onBlur?: () => void;
    }
    class Layout {
        name: string;
        labelKey: string;
        layout: Layout_Array;
        onFocus: () => void;
        onBlur: () => void;
        constructor(settings: Layout_Settings);
    }
    interface LayoutNameAndLabel {
        name: string;
        labelKey: string;
    }
    interface LayoutManager_SaveState {
        currentLayout: string;
    }
    class LayoutManager implements ISelfPersist<LayoutManager_SaveState> {
        private _Layouts;
        private _CurrentLayout;
        private _Name;
        private _SplitterParent;
        constructor(name?: string);
        getName(): string;
        getSplitterParent(): JQuery;
        setSplitterParent(value: JQuery): void;
        getCurrentLayout(): string;
        applyLayout(layoutOrName: string | Layout, splitterParent?: JQuery): void;
        private doApplyLayout(layout, splitterParent, splitterGroupPersistence, isTopLevelSplitter);
        private undoLayout();
        registerLayout(layout: Layout): void;
        removeLayoutByName(name: string): void;
        getLayouts(): LayoutNameAndLabel[];
        private findLayout(name);
        private findLayoutIndex(name);
        saveState: () => void;
        loadState(): LayoutManager_SaveState;
        applyState(settings: LayoutManager_SaveState): void;
        loadAndApplyState(): void;
        private persistenceKey();
        private createSettings();
    }
    var layoutManager: LayoutManager;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface LinkRenderHandler_Settings {
        linkSite: LinkSiteEnum;
        displayOrder: number;
        canLinkAircraft: (aircraft: Aircraft) => boolean;
        hasChanged: (aircraft: Aircraft) => boolean;
        title: string | AircraftFuncReturningString;
        buildUrl: (aircraft: Aircraft) => string;
        target?: string | AircraftFuncReturningString;
        onClick?: (event: Event) => void;
    }
    class LinkRenderHandler {
        linkSite: LinkSiteEnum;
        displayOrder: number;
        canLinkAircraft: (aircraft: Aircraft) => boolean;
        hasChanged: (aircraft: Aircraft) => boolean;
        title: string | AircraftFuncReturningString;
        buildUrl: (aircraft: Aircraft) => string;
        target: string | AircraftFuncReturningString;
        onClick: (event: Event) => void;
        constructor(settings: LinkRenderHandler_Settings);
        getTitle(aircraft: Aircraft): string;
        getTarget(aircraft: Aircraft): string;
    }
    var linkRenderHandlers: LinkRenderHandler[];
    abstract class LinkRenderHandler_AutoRefreshPluginBase extends LinkRenderHandler {
        protected _LinksRendererPlugin: AircraftLinksPlugin[];
        constructor(settings: LinkRenderHandler_Settings);
        protected disposeBase(): void;
        addLinksRendererPlugin(value: AircraftLinksPlugin): void;
        protected refreshAircraftLinksPlugin(): void;
    }
    class AutoSelectLinkRenderHelper extends LinkRenderHandler_AutoRefreshPluginBase {
        private _AutoSelectEnabledChangedHook;
        private _AircraftAutoSelect;
        constructor(aircraftAutoSelect: AircraftAutoSelect);
        dispose(): void;
        private autoSelectEnabledChanged();
    }
    class CentreOnSelectedAircraftLinkRenderHandler extends LinkRenderHandler {
        constructor(aircraftList: AircraftList, mapPlugin: IMap);
    }
    class HideAircraftNotOnMapLinkRenderHandler extends LinkRenderHandler_AutoRefreshPluginBase {
        private _HideAircraftNotOnMapHook;
        private _AircraftListFetcher;
        constructor(aircraftListFetcher: AircraftListFetcher);
        dispose(): void;
        private hideAircraftChanged();
    }
    class JumpToAircraftDetailPageRenderHandler extends LinkRenderHandler {
        constructor();
    }
    class PauseLinkRenderHandler extends LinkRenderHandler_AutoRefreshPluginBase {
        private _PausedChangedHook;
        private _AircraftListFetcher;
        constructor(aircraftListFetcher: AircraftListFetcher);
        dispose(): void;
        private pausedChanged();
    }
    class LinksRenderer {
        getDefaultAircraftLinkSites(): LinkSiteEnum[];
        findLinkHandler(linkSite: LinkSiteEnum | LinkRenderHandler): LinkRenderHandler;
        private getAircraftLinkHandlers();
    }
    var linksRenderer: LinksRenderer;
}
declare namespace VRS {
    interface MenuItem_Settings {
        name: string;
        labelKey?: string | VoidFuncReturning<string>;
        jqIcon?: string | VoidFuncReturning<string>;
        vrsIcon?: string | VoidFuncReturning<string>;
        checked?: boolean | VoidFuncReturning<boolean>;
        labelImageUrl?: string | VoidFuncReturning<string>;
        labelImageClasses?: string | VoidFuncReturning<string>;
        clickCallback?: (menuItem?: MenuItem) => void;
        initialise?: () => void;
        disabled?: boolean | VoidFuncReturning<boolean>;
        subItems?: MenuItem[];
        tag?: any;
        noAutoClose?: boolean;
        suppress?: () => boolean;
    }
    class MenuItem {
        private _Initialise;
        private _Disabled;
        private _LabelKey;
        private _JqIcon;
        private _VrsIcon;
        private _Checked;
        private _LabelImageUrl;
        private _LabelImageClasses;
        name: string;
        clickCallback: (menuItem?: MenuItem) => void;
        subItems: MenuItem[];
        subItemsNormalised: MenuItem[];
        noAutoClose: boolean;
        tag: any;
        suppress: () => boolean;
        constructor(settings: MenuItem_Settings);
        initialise(): void;
        isDisabled(): boolean;
        getLabelText(): any;
        getJQueryIcon(): string;
        getVrsIcon(): string;
        getLabelImageUrl(): string;
        getLabelImageClasses(): string;
        getLabelImageElement(): JQuery;
    }
    interface Menu_Settings {
        items: MenuItem[];
    }
    class Menu {
        private _Dispatcher;
        private _Events;
        private _Settings;
        private _TopLevelMenuItems;
        getTopLevelMenuItems(): MenuItem[];
        hookBeforeAddingFixedMenuItems(callback: (menu: Menu, menuItems: MenuItem[]) => void, forceThis?: Object): IEventHandle;
        hookAfterAddingFixedMenuItems(callback: (menu: Menu, menuItems: MenuItem[]) => void, forceThis?: Object): IEventHandle;
        unhook(hookResult: IEventHandle): void;
        constructor(settings?: Menu_Settings);
        buildMenuItems(): MenuItem[];
        private normaliseMenu(rawItems);
        findMenuItemForName(name: string, menuItems?: MenuItem[], useNormalisedSubItems?: boolean): MenuItem;
    }
}
declare namespace VRS {
    interface OptionField_Settings {
        name: string;
        dispatcherName?: string;
        controlType?: OptionControlTypesEnum;
        labelKey?: string | VoidFuncReturning<string>;
        getValue?: () => any;
        setValue?: (value: any) => void;
        saveState?: () => void;
        keepWithNext?: boolean;
        hookChanged?: (callback: Function, forceThis?: Object) => any;
        unhookChanged?: (eventHandle: IEventHandle | IEventHandleJQueryUI) => void;
        inputWidth?: InputWidthEnum;
        visible?: boolean | VoidFuncReturning<boolean>;
    }
    class OptionField {
        protected _Dispatcher: EventHandler;
        private _Events;
        protected _Settings: OptionField_Settings;
        constructor(settings: OptionField_Settings);
        private _ChangedHookResult;
        getName(): string;
        getControlType(): OptionControlTypesEnum;
        getKeepWithNext(): boolean;
        setKeepWithNext(value: boolean): void;
        getLabelKey(): string | VoidFuncReturning<string>;
        getLabelText(): string;
        getInputWidth(): InputWidthEnum;
        getVisible(): boolean;
        hookRefreshFieldContent(callback: () => void, forceThis?: Object): IEventHandle;
        raiseRefreshFieldContent(): void;
        hookRefreshFieldState(callback: () => void, forceThis?: Object): IEventHandle;
        raiseRefreshFieldState(): void;
        hookRefreshFieldVisibility(callback: () => void, forceThis?: Object): IEventHandle;
        raiseRefreshFieldVisibility(): void;
        unhook(hookResult: IEventHandle): void;
        getValue(): any;
        setValue(value: any): void;
        saveState(): void;
        getInputClass(): string;
        applyInputClass(jqElement: JQuery): void;
        hookEvents(callback: () => void, forceThis?: Object): void;
        unhookEvents(): void;
    }
    interface OptionFieldButton_Settings extends OptionField_Settings {
        icon?: string;
        primaryIcon?: string;
        secondaryIcon?: string;
        showText?: boolean;
    }
    class OptionFieldButton extends OptionField {
        protected _Settings: OptionFieldButton_Settings;
        private _Enabled;
        constructor(settings: OptionFieldButton_Settings);
        getPrimaryIcon(): string;
        getSecondaryIcon(): string;
        getShowText(): boolean;
        getEnabled(): boolean;
        setEnabled(value: boolean): void;
    }
    interface OptionFieldCheckBox_Settings extends OptionField_Settings {
    }
    class OptionFieldCheckBox extends OptionField {
        constructor(settings: OptionFieldCheckBox_Settings);
    }
    interface OptionFieldColour_Settings extends OptionField_Settings {
    }
    class OptionFieldColour extends OptionField {
        constructor(settings: OptionFieldColour_Settings);
    }
    interface OptionFieldComboBox_Settings extends OptionField_Settings {
        values?: ValueText[];
        changed?: (any) => void;
    }
    class OptionFieldComboBox extends OptionField {
        protected _Settings: OptionFieldComboBox_Settings;
        constructor(settings: OptionFieldComboBox_Settings);
        getValues(): ValueText[];
        callChangedCallback(selectedValue: any): void;
    }
    interface OptionFieldDate_Settings extends OptionField_Settings {
        defaultDate?: Date;
        minDate?: Date;
        maxDate?: Date;
    }
    class OptionFieldDate extends OptionField {
        protected _Settings: OptionFieldDate_Settings;
        constructor(settings: OptionFieldDate_Settings);
        getDefaultDate(): Date;
        getMinDate(): Date;
        getMaxDate(): Date;
    }
    interface OptionFieldLabel_Settings extends OptionField_Settings {
        labelWidth?: LabelWidthEnum;
    }
    class OptionFieldLabel extends OptionField {
        protected _Settings: OptionFieldLabel_Settings;
        constructor(settings: OptionFieldLabel_Settings);
        getLabelWidth(): LabelWidthEnum;
    }
    interface OptionFieldLinkLabel_Settings extends OptionField_Settings {
        getHref?: () => string;
        getTarget?: () => string;
    }
    class OptionFieldLinkLabel extends OptionFieldLabel {
        protected _Settings: OptionFieldLinkLabel_Settings;
        constructor(settings: OptionFieldLinkLabel_Settings);
        getHref(): string;
        getTarget(): string;
    }
    interface OptionFieldNumeric_Settings extends OptionField_Settings {
        min?: number;
        max?: number;
        decimals?: number;
        step?: number;
        showSlider?: boolean;
        sliderStep?: number;
        allowNullValue?: boolean;
    }
    class OptionFieldNumeric extends OptionField {
        protected _Settings: OptionFieldNumeric_Settings;
        constructor(settings: OptionFieldNumeric_Settings);
        getMin(): number;
        getMax(): number;
        getDecimals(): number;
        getStep(): number;
        showSlider(): boolean;
        getSliderStep(): number;
        getAllowNullValue(): boolean;
    }
    interface OptionFieldOrderedSubset_Settings extends OptionField_Settings {
        values?: ValueText[];
        keepValuesSorted?: boolean;
    }
    class OptionFieldOrderedSubset extends OptionField {
        protected _Settings: OptionFieldOrderedSubset_Settings;
        constructor(settings: OptionFieldOrderedSubset_Settings);
        getValues(): ValueText[];
        getKeepValuesSorted(): boolean;
    }
    interface OptionFieldPaneList_Settings extends OptionField_Settings {
        panes?: OptionPane[];
        maxPanes?: number;
        addPane?: OptionPane;
        suppressRemoveButton?: boolean;
        refreshAddControls?: (disabled: boolean, addPanel: JQuery) => void;
    }
    class OptionFieldPaneList extends OptionField {
        protected _Settings: OptionFieldPaneList_Settings;
        private _PaneListEvents;
        constructor(settings: OptionFieldPaneList_Settings);
        getMaxPanes(): number;
        setMaxPanes(value: number): void;
        getPanes(): OptionPane[];
        getAddPane(): OptionPane;
        setAddPane(value: OptionPane): void;
        getSuppressRemoveButton(): boolean;
        setSuppressRemoveButton(value: boolean): void;
        getRefreshAddControls(): (disabled: boolean, addParentJQ: JQuery) => void;
        setRefreshAddControls(value: (disabled: boolean, addParentJQ: JQuery) => void): void;
        hookPaneAdded(callback: (newPane: OptionPane, index: number) => void, forceThis?: Object): IEventHandle;
        private onPaneAdded(pane, index);
        hookPaneRemoved(callback: (removedPane: OptionPane, index: number) => void, forceThis?: Object): IEventHandle;
        private onPaneRemoved(pane, index);
        hookMaxPanesChanged(callback: () => void, forceThis?: Object): IEventHandle;
        private onMaxPanesChanged();
        addPane(pane: OptionPane, index?: number): void;
        removePane(pane: OptionPane): void;
        trimExcessPanes(): void;
        private findPaneIndex(pane);
        removePaneAt(index: number): void;
    }
    interface OptionFieldRadioButton_Settings extends OptionField_Settings {
        values?: ValueText[];
    }
    class OptionFieldRadioButton extends OptionField {
        protected _Settings: OptionFieldRadioButton_Settings;
        constructor(settings: OptionFieldRadioButton_Settings);
        getValues(): ValueText[];
    }
    interface OptionFieldTextBox_Settings extends OptionField_Settings {
        upperCase?: boolean;
        lowerCase?: boolean;
        maxLength?: number;
    }
    class OptionFieldTextBox extends OptionField {
        protected _Settings: OptionFieldTextBox_Settings;
        constructor(settings: OptionFieldTextBox_Settings);
        getUpperCase(): boolean;
        getLowerCase(): boolean;
        getMaxLength(): number;
    }
    interface OptionPane_Settings {
        name: string;
        titleKey?: string | VoidFuncReturning<string>;
        displayOrder?: number;
        fields?: OptionField[];
        dispose?: (objects: OptionPane_DisposeObjects) => void;
        pageParentCreated?: (parent: OptionPageParent) => void;
    }
    interface OptionPane_DisposeObjects {
        optionPane: OptionPane;
        optionPageParent: OptionPageParent;
    }
    class OptionPane {
        private _Settings;
        private _OptionFields;
        private _Generation;
        constructor(settings: OptionPane_Settings);
        getName(): string;
        getTitleKey(): string | VoidFuncReturning<string>;
        getTitleText(): string;
        setTitleKey(value: string | VoidFuncReturning<string>): void;
        getDisplayOrder(): number;
        setDisplayOrder(value: number): void;
        getFieldCount(): number;
        getField(idx: number): OptionField;
        getFieldByName(optionFieldName: string): OptionField;
        dispose(options: OptionPane_DisposeObjects): void;
        pageParentCreated(optionPageParent: OptionPageParent): void;
        addField(optionField: OptionField): void;
        removeFieldByName(optionFieldName: string): void;
        foreachField(callback: (field: OptionField) => void): void;
        private findIndexByName(optionFieldName);
    }
    interface OptionPage_Settings {
        name: string;
        titleKey?: string | VoidFuncReturning<string>;
        displayOrder?: number;
        panes?: OptionPane[] | OptionPane[][];
    }
    class OptionPage {
        private _Settings;
        private _OptionPanes;
        private _Generation;
        private _SortGeneration;
        constructor(settings: OptionPage_Settings);
        getName(): string;
        setName(value: string): void;
        getTitleKey(): string | VoidFuncReturning<string>;
        setTitleKey(value: string | VoidFuncReturning<string>): void;
        getDisplayOrder(): number;
        setDisplayOrder(value: number): void;
        addPane(optionPane: OptionPane | OptionPane[]): void;
        removePaneByName(optionPaneName: string): void;
        foreachPane(callback: (pane: OptionPane) => void): void;
        private findIndexByName(optionPaneName);
        private sortPanes();
    }
    class OptionPageParent {
        private _Dispatcher;
        private _Events;
        hookFieldChanged(callback: () => void, forceThis?: Object): IEventHandle;
        raiseFieldChanged(): void;
        unhook(hookResult: IEventHandle): void;
    }
    interface OptionControl_BaseOptions {
        field: OptionField;
        fieldParentJQ: JQuery;
        optionPageParent: OptionPageParent;
    }
    class OptionControlTypeBroker {
        private _ControlTypes;
        addControlTypeHandler(controlType: OptionControlTypesEnum, creatorCallback: (options: OptionControl_BaseOptions) => JQuery): void;
        addControlTypeHandlerIfNotRegistered(controlType: OptionControlTypesEnum, creatorCallback: (options: OptionControl_BaseOptions) => JQuery): void;
        removeControlTypeHandler(controlType: OptionControlTypesEnum): void;
        controlTypeHasHandler(controlType: OptionControlTypesEnum): boolean;
        createControlTypeHandler(options: OptionControl_BaseOptions): JQuery;
    }
    var optionControlTypeBroker: OptionControlTypeBroker;
    type OptionControlTypesEnum = string;
    var optionControlTypes: any;
}
declare namespace VRS {
    interface Page_Settings {
        name: string;
        element: JQuery;
        visibleCallback?: (willBecomeVisible: boolean) => void;
        afterVisibleCallback?: () => void;
    }
    class Page {
        name: string;
        element: JQuery;
        visibleCallback: (willBecomeVisible: boolean) => void;
        afterVisibleCallback: () => void;
        container: JQuery;
        isVisible: boolean;
        originalParent: JQuery;
        originalCss: Object;
        constructor(settings: Page_Settings);
        dispose(): void;
    }
    class PageManager {
        private _Dispatcher;
        private _Events;
        private _Element;
        private _ElementOriginalCss;
        private _Pages;
        private _VisiblePage;
        dispose(): void;
        hookHidingPage(callback: (page: Page) => void, forceThis?: Object): IEventHandle;
        hookHiddenPage(callback: (page: Page) => void, forceThis?: Object): IEventHandle;
        hookShowingPage(callback: (page: Page) => void, forceThis?: Object): IEventHandle;
        hookShownPage(callback: (page: Page) => void, forceThis?: Object): IEventHandle;
        private raiseEvent(event, page);
        unhook(hookResult: IEventHandle): void;
        initialise(element: JQuery): void;
        addPage(page: Page): void;
        removePage(pageOrName: string | Page): void;
        show(pageOrName: string | Page): void;
        private hidePage(page);
        private showPage(page);
        private findPage(nameOrPage);
        private findPageIndex(nameOrPage);
        private windowResized();
    }
    var pageManager: PageManager;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface PolarPlotter_Settings {
        name?: string;
        map: IMap;
        aircraftListFetcher: AircraftListFetcher;
        autoSaveState?: boolean;
        unitDisplayPreferences: UnitDisplayPreferences;
    }
    interface AltitudeRange {
        lowAlt: number;
        highAlt: number;
    }
    interface PolarPlot_Slice {
        lowAlt: number;
        highAlt: number;
        plots: ILatLng[];
    }
    interface PolarPlot_FeedSliceAbstract {
        feedName: string;
        low: number;
        high: number;
    }
    interface PolarPlot_AllFeedSlices {
        feedId: number;
        slices: PolarPlot_Slice[];
    }
    interface PolarPlot_AltitudeRangeConfig {
        low: number;
        high: number;
        colour: string;
        zIndex: number;
    }
    interface PolarPlotter_SaveState {
        altitudeRangeConfigs: PolarPlot_AltitudeRangeConfig[];
        plotsOnDisplay: PolarPlot_FeedSliceAbstract[];
        strokeOpacity: number;
        fillOpacity: number;
    }
    interface PolarPlot_Id {
        feedId: number;
        lowAlt: number;
        highAlt: number;
        colour?: string;
    }
    class PolarPlotter implements ISelfPersist<PolarPlotter_SaveState> {
        private _Settings;
        private _AutoRefreshTimerId;
        private _PlotsOnDisplay;
        private _PolarPlot;
        private _AltitudeRangeConfigs;
        private _StrokeOpacity;
        private _FillOpacity;
        constructor(settings: PolarPlotter_Settings);
        private getPlotsOnDisplayIndex;
        private addToPlotsOnDisplay;
        private removeFromPlotsOnDisplay;
        private getPlotsOnDisplayForFeed;
        getName: () => string;
        getPolarPlot: () => PolarPlot_AllFeedSlices;
        getPolarPlotterFeeds: () => IReceiver[];
        getSortedPolarPlotterFeeds: () => IReceiver[];
        getPlotsOnDisplay: () => PolarPlot_FeedSliceAbstract[];
        getAltitudeRangeConfigs: () => PolarPlot_AltitudeRangeConfig[];
        setAltitudeRangeConfigs: (value: PolarPlot_AltitudeRangeConfig[]) => void;
        getStrokeOpacity: () => number;
        setStrokeOpacity: (value: number) => void;
        getFillOpacity: () => number;
        setFillOpacity: (value: number) => void;
        saveState: () => void;
        loadState: () => PolarPlotter_SaveState;
        applyState: (settings: PolarPlotter_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey();
        private createSettings();
        createOptionPane: (displayOrder: number) => OptionPane;
        fetchPolarPlot: (feedId: number, callback?: () => void) => void;
        findSliceForAltitudeRange: (plots: PolarPlot_AllFeedSlices, lowAltitude: number, highAltitude: number) => PolarPlot_Slice;
        isSliceForAltitudeRange: (slice: PolarPlot_Slice, lowAltitude: number, highAltitude: number) => boolean;
        getNormalisedSliceRange: (slice: PolarPlot_Slice, lowOpenEnd?: number, highOpenEnd?: number) => AltitudeRange;
        getNormalisedRange: (lowAltitude?: number, highAltitude?: number, lowOpenEnd?: number, highOpenEnd?: number) => AltitudeRange;
        getSliceRangeDescription: (lowAltitude: number, highAltitude: number) => string;
        isAllAltitudes: (lowAltitude: number, highAltitude: number) => boolean;
        getAltitudeRangeConfigRecord: (lowAltitude: number, highAltitude: number) => PolarPlot_AltitudeRangeConfig;
        getSliceRangeColour: (lowAltitude: number, highAltitude: number) => string;
        getSliceRangeZIndex: (lowAltitude: number, highAltitude: number) => number;
        displayPolarPlotSlice: (feedId: number, slice: PolarPlot_Slice, colour?: string) => void;
        isOnDisplay: (feedId: number, lowAltitude: number, highAltitude: number) => boolean;
        removePolarPlotSlice: (feedId: number, slice: PolarPlot_Slice) => void;
        removeAllSlicesForFeed: (feedId: number) => void;
        removeAllSlicesForAllFeeds: () => void;
        togglePolarPlotSlice: (feedId: number, slice: PolarPlot_Slice, colour?: string) => boolean;
        fetchAndToggleByIdentifiers: (plotIdentifiers: PolarPlot_Id[]) => boolean;
        fetchAndDisplayByIdentifiers: (plotIdentifiers: PolarPlot_Id[]) => void;
        removeByIdentifiers: (plotIdentifiers: PolarPlot_Id[]) => PolarPlot_Id[];
        private getPolygonId;
        private getPolygonColour;
        private getDistinctFeedIds;
        refetchAllDisplayed: () => void;
        refreshAllDisplayed: () => void;
        startAutoRefresh: () => void;
    }
}
declare namespace VRS {
    interface RefreshTarget_Settings {
        targetJQ: JQuery;
        onRefresh: () => void;
        onRefreshThis?: Object;
    }
    class RefreshTarget {
        targetJQ: JQuery;
        onRefresh: () => void;
        onRefreshThis: Object;
        constructor(settings: RefreshTarget_Settings);
        dispose(): void;
        callOnRefresh: () => void;
    }
    interface RefreshOwner_Settings {
        ownerJQ: JQuery;
        targets?: RefreshTarget[];
    }
    class RefreshOwner {
        private _Targets;
        ownerJQ: JQuery;
        constructor(settings: RefreshOwner_Settings);
        dispose(): void;
        refreshTargets(): void;
        getTarget(elementJQ: JQuery): RefreshTarget;
        getTargetIndex(elementJQ: JQuery): number;
        addTarget(target: RefreshTarget): void;
        removeTarget(target: RefreshTarget): void;
        removeAllTargets(): void;
    }
    class RefreshManager {
        private _Targets;
        private _Owners;
        registerTarget(elementJQ: JQuery, onRefresh: () => void, onRefreshThis?: Object): void;
        unregisterTarget(elementJQ: JQuery): void;
        registerOwner(elementJQ: JQuery): void;
        unregisterOwner(elementJQ: JQuery): void;
        rebuildRelationships(): void;
        refreshTargets(ownerJQ: JQuery): void;
        private getTargetIndex(elementJQ);
        private getOwnerIndex(elementJQ);
        private buildOwners(elementJQ);
    }
    var refreshManager: RefreshManager;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface Report_Settings {
        name?: string;
        autoSaveState?: boolean;
        showFetchUI?: boolean;
        unitDisplayPreferences: UnitDisplayPreferences;
    }
    interface Report_SortColumn {
        field: ReportSortColumnEnum;
        ascending: boolean;
    }
    interface Report_SaveState {
        pageSize: number;
    }
    class Report implements ISelfPersist<Report_SaveState> {
        private _Dispatcher;
        private _Events;
        private _Settings;
        private _LastFetchResult;
        private _Criteria;
        private _SortColumns;
        private _SelectedFlight;
        private _PageSize;
        constructor(settings: Report_Settings);
        getName(): string;
        getCriteria(): ReportCriteria;
        getSortColumnsLength(): number;
        getSortColumn(index: number): Report_SortColumn;
        setSortColumn(index: number, value: Report_SortColumn): void;
        getGroupSortColumn(): ReportSortColumnEnum;
        getFlights(): IReportFlight[];
        getLastError(): string;
        getSelectedFlight(): IReportFlight;
        setSelectedFlight(value: IReportFlight): void;
        getPageSize(): number;
        setPageSize(value: number): void;
        getCountRowsAvailable(): number;
        isReportPaged(): boolean;
        hasData(): boolean;
        hookFailedNoCriteria(callback: (report: Report) => void, forceThis?: Object): IEventHandle;
        hookBuildingRequest(callback: (report: Report, parameters: any, headers: any) => void, forceThis?: boolean): IEventHandle;
        hookFetchFailed(callback: (report: Report, statusCode: number, textStatus: string, error: string) => void, forceThis?: Object): IEventHandle;
        hookPageSizeChanged(callback: (report: Report) => void, forceThis?: Object): IEventHandle;
        hookRowsFetched(callback: (report: Report) => void, forceThis?: Object): IEventHandle;
        hookSelectedFlightCHanged(callback: (report: Report) => void, forceThis?: Object): IEventHandle;
        unhook(hookResult: IEventHandle): void;
        saveState(): void;
        loadState(): Report_SaveState;
        applyState(settings: Report_SaveState): void;
        loadAndApplyState(): void;
        private persistenceKey();
        private createSettings();
        createOptionPane(displayOrder: number): OptionPane[];
        private getSortColumnValues();
        populateFromQueryString(): void;
        _formPermanentLinkUrl(useRelativeDates: boolean): string;
        fetchPage(pageNumber?: number): void;
        private pageFetched(rawData);
        private fixupRoutesAndAirports();
        private fetchFailed(jqXHR, textStatus, errorThrown);
        static convertFlightToVrsAircraft(flight: IReportFlight, useFirstValues: boolean): Aircraft;
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface ReportFilterPropertyHandler_Settings extends FilterPropertyHandler_Settings {
        property: ReportFilterPropertyEnum;
    }
    class ReportFilterPropertyHandler extends FilterPropertyHandler {
        constructor(settings: ReportFilterPropertyHandler_Settings);
    }
    var reportFilterPropertyHandlers: {
        [index: string]: ReportFilterPropertyHandler;
    };
    class ReportFilter extends Filter {
        constructor(reportFilterProperty: ReportFilterPropertyEnum, valueCondition: ValueCondition);
    }
    class ReportFilterHelper extends FilterHelper {
        constructor();
    }
    var reportFilterHelper: ReportFilterHelper;
    interface ReportCriteria_Settings {
        name: string;
        unitDisplayPreferences?: UnitDisplayPreferences;
    }
    interface ReportCriteria_SaveState {
        findAllPermutationsOfCallsign: boolean;
    }
    class ReportCriteria implements ISelfPersist<ReportCriteria_SaveState> {
        private _Dispatcher;
        private _Events;
        private _Settings;
        private _Filters;
        private _FindAllPermutationsOfCallsign;
        constructor(settings: ReportCriteria_Settings);
        getName(): string;
        getFindAllPermutationsOfCallsign(): boolean;
        setFindAllPermutationsOfCallsign(value: boolean): void;
        hasCriteria(): boolean;
        isForSingleAircraft(): boolean;
        hookCriteriaChanged(callback: () => void, forceThis?: Object): IEventHandle;
        unhook(hookResult: IEventHandle): void;
        saveState(): void;
        loadState(): ReportCriteria_SaveState;
        applyState(settings: ReportCriteria_SaveState): void;
        loadAndApplyState(): void;
        private persistenceKey();
        private createSettings();
        createOptionPane(displayOrder: number): OptionPane;
        addFilter(filterOrPropertyId: ReportFilter | ReportFilterPropertyEnum): ReportFilter;
        private filterPaneRemoved(pane, index);
        removeFilterAt(index: number): void;
        populateFromQueryString(): void;
        createQueryString(useRelativeDates: boolean): Object;
        private extractFilterFromQueryString(pageUrl, propertyHandler, typeHandler, condition, reverseCondition);
        private doExtractFilterFromQueryString(filter, pageUrl, propertyHandler, typeHandler, condition, reverseCondition, nameSuffix);
        private doAddQueryStringFromFilter(queryStringParams, filter, value, nameSuffix, valueIsString?);
        private getFilterName(propertyHandler, nameSuffix, reverseCondition);
        addToQueryParameters(params: any): void;
    }
}
declare namespace VRS {
    interface ReportRender_Options {
        unitDisplayPreferences: UnitDisplayPreferences;
        distinguishOnGround?: boolean;
        showUnits?: boolean;
        alwaysShowEndDate?: boolean;
        plotterOptions?: AircraftPlotterOptions;
        justShowStartTime?: boolean;
    }
    interface ReportPropertyHandler_Settings {
        property: ReportAircraftOrFlightPropertyEnum;
        surfaces?: ReportSurfaceBitFlags;
        labelKey?: string;
        headingKey?: string;
        optionsLabelKey?: string;
        headingAlignment?: AlignmentEnum;
        contentAlignment?: AlignmentEnum;
        alignment?: AlignmentEnum;
        fixedWidth?: (surface?: ReportSurfaceBitFlags) => string;
        hasValue: (aircraftOrFlight: IReportAircraft | IReportFlight) => boolean;
        contentCallback?: (aircraftOrFlight: IReportAircraft | IReportFlight, options?: ReportRender_Options, surface?: ReportSurfaceBitFlags) => string;
        renderCallback?: (aircraftOrFlight: IReportAircraft | IReportFlight, options?: ReportRender_Options, surface?: ReportSurfaceBitFlags) => string;
        tooltipCallback?: (aircraftOrFlight: IReportAircraft | IReportFlight, options?: ReportRender_Options) => string;
        isMultiLine?: boolean;
        suppressLabelCallback?: (surface: ReportSurfaceBitFlags) => boolean;
        sortColumn?: ReportSortColumnEnum;
        groupValue?: (aircraftOrFlight: IReportAircraft | IReportFlight) => string | number;
        createWidget?: (element: JQuery, surface: ReportSurfaceBitFlags, options: ReportRender_Options) => void;
        renderWidget?: (element: JQuery, aircraftOrFlight: IReportAircraft | IReportFlight, options: ReportRender_Options, surface: ReportSurfaceBitFlags) => void;
        destroyWidget?: (element: JQuery, surface: ReportSurfaceBitFlags) => void;
    }
    class ReportPropertyHandler {
        private _HasValue;
        private _CreateWidget;
        private _DestroyWidget;
        private _RenderWidget;
        private _ContentCallback;
        private _RenderCallback;
        private _TooltipCallback;
        property: ReportAircraftOrFlightPropertyEnum;
        surfaces: ReportSurfaceBitFlags;
        labelKey: string;
        headingKey: string;
        optionsLabelKey: string;
        headingAlignment: AlignmentEnum;
        contentAlignment: AlignmentEnum;
        isMultiLine: boolean;
        fixedWidth: (surface?: ReportSurfaceBitFlags) => string;
        suppressLabelCallback: (surface: ReportSurfaceBitFlags) => boolean;
        sortColumn: ReportSortColumnEnum;
        groupValue: (aircraftOrFlight: IReportAircraft | IReportFlight) => string | number;
        isAircraftProperty: boolean;
        isFlightsProperty: boolean;
        constructor(settings: ReportPropertyHandler_Settings);
        isSurfaceSupported(surface: ReportSurfaceBitFlags): boolean;
        hasValue(flightJson: IReportFlight): boolean;
        createWidgetInJQueryElement(jQueryElement: JQuery, surface: ReportSurfaceBitFlags, options: ReportRender_Options): void;
        destroyWidgetInJQueryElement(jQueryElement: JQuery, surface: ReportSurfaceBitFlags): void;
        renderIntoJQueryElement(jqElement: JQuery, json: IReportAircraft | IReportFlight, options: ReportRender_Options, surface: ReportSurfaceBitFlags): void;
        addTooltip(jqElement: JQuery, json: IReportAircraft | IReportFlight, options: ReportRender_Options): void;
    }
    var reportPropertyHandlers: {
        [index: string]: ReportPropertyHandler;
    };
    interface ReportPropertyHandlerHelper_AddPropertyToPaneSettings {
        pane: OptionPane;
        surface: ReportSurfaceBitFlags;
        fieldLabel: string;
        getList: () => ReportAircraftOrFlightPropertyEnum[];
        setList: (list: ReportAircraftOrFlightPropertyEnum[]) => void;
        saveState: () => void;
    }
    class ReportPropertyHandlerHelper {
        buildValidReportPropertiesList(reportPropertyList: ReportAircraftOrFlightPropertyEnum[], surfaces: ReportSurfaceBitFlags[], maximumProperties?: number): (ReportAircraftPropertyEnum | ReportFlightPropertyEnum)[];
        addReportPropertyListOptionsToPane(settings: ReportPropertyHandlerHelper_AddPropertyToPaneSettings): OptionFieldOrderedSubset;
        findPropertyHandlerForSortColumn(sortColumn: ReportSortColumnEnum): ReportPropertyHandler;
    }
    var reportPropertyHandlerHelper: ReportPropertyHandlerHelper;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    var scriptKey: {
        GoogleMaps: string;
    };
    interface LoadScript_Options {
        key?: string;
        url?: string;
        params?: any;
        queue?: boolean;
        success?: () => void;
        error?: (xhr?: JQueryXHR, status?: string, error?: string) => void;
        timeout?: number;
    }
    class ScriptManager {
        private _LoadedScripts;
        private _Queue;
        loadScript(options: LoadScript_Options): void;
        private doProcessQueue();
        private doLoadScript(options, onCompletion?);
    }
    var scriptManager: ScriptManager;
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    interface IServerConfig {
        VrsVersion: string;
        IsMono: boolean;
        UseMarkerLabels: boolean;
        Receivers: IServerConfigReceiver[];
        IsLocalAddress: boolean;
        IsAudioEnabled: boolean;
        MinimumRefreshSeconds: number;
        RefreshSeconds: number;
        InitialSettings: string;
        InitialLatitude: number;
        InitialLongitude: number;
        InitialMapType: string;
        InitialZoom: number;
        InitialDistanceUnit: string;
        InitialHeightUnit: string;
        InitialSpeedUnit: string;
        InternetClientCanRunReports: boolean;
        InternetClientCanShowPinText: boolean;
        InternetClientTimeoutMinutes: number;
        InternetClientsCanPlayAudio: boolean;
        InternetClientsCanSubmitRoutes: boolean;
        InternetClientsCanSeeAircraftPictures: boolean;
        InternetClientsCanSeePolarPlots: boolean;
    }
    interface IServerConfigReceiver {
        UniqueId: number;
        Name: string;
    }
    class ServerConfiguration {
        private _ServerConfig;
        get(): IServerConfig;
        audioEnabled(): boolean;
        picturesEnabled(): boolean;
        pinTextEnabled(): boolean;
        reportsEnabled(): boolean;
        routeSubmissionEnabled(): boolean;
        polarPlotsEnabled(): boolean;
        fetch(successCallback: () => void): void;
        private fetchFailed(successCallback);
    }
    var serverConfig: ServerConfiguration;
}
declare namespace VRS {
    class StringUtility {
        contains(text: string, hasText: string, ignoreCase?: boolean): boolean;
        equals(lhs: string, rhs: string, ignoreCase?: boolean): boolean;
        filter(text: string, allowCharacter: (ch: string) => boolean): string;
        filterReplace(text: string, replaceCharacter: (ch: string) => string): string;
        htmlEscape(text: string): string;
        format(text: string, ...args: any[]): string;
        private toFormattedString(useLocale, args);
        formatNumber(value: number, format?: number): string;
        formatNumber(value: number, format?: string): string;
        indexNotOf(text: string, character: string): number;
        isUpperCase(text: string): boolean;
        repeatedSequence(sequence: string, count: number): string;
        padLeft(text: string, ch: string, length: number): string;
        padRight(text: string, ch: string, length: number): string;
        private doPad(text, ch, length, toRight);
        startsWith(text: string, withText: string, ignoreCase?: boolean): boolean;
        endsWith(text: string, withText: string, ignoreCase?: boolean): boolean;
        private startsOrEndsWith(text, withText, ignoreCase, fromStart);
    }
    var stringUtility: StringUtility;
}
declare namespace VRS {
    class TimeoutManager {
        private _Dispatcher;
        private _Events;
        private _TimerInterval;
        private _Timer;
        private _Enabled;
        private _Threshold;
        private _ServerConfigurationChangedHookResult;
        private _LastActivity;
        private _Expired;
        getExpired(): boolean;
        hookSiteTimedOut(callback: Function, forceThis: Object): IEventHandle;
        unhook(hookResult: IEventHandle): void;
        initialise(): void;
        dispose(): void;
        resetTimer(): void;
        restartTimedOutSite(): void;
        private loadConfiguration();
        private serverConfigChanged();
        private timeoutExpired();
    }
    var timeoutManager: TimeoutManager;
}
declare namespace VRS {
    class TitleUpdater {
        private _LocaleChangedHookResult;
        private _AircraftList;
        private _AircraftListUpdatedHookResult;
        private _AircraftListPreviousCount;
        constructor();
        dispose(): void;
        showAircraftListCount(aircraftList: AircraftList): void;
        private refreshAircraftCount(forceRefresh);
        private aircraftListUpdated();
        private localeChanged();
    }
}
declare namespace VRS {
    var globalOptions: GlobalOptions;
    var isFlightSim: any;
    interface UnitDisplayPreferences_SaveState {
        distanceUnit: DistanceEnum;
        heightUnit: HeightEnum;
        speedUnit: SpeedEnum;
        vsiPerSecond: boolean;
        flTransitionAlt: number;
        flTransitionUnit: HeightEnum;
        flHeightUnit: HeightEnum;
        showAltType: boolean;
        showVsiType: boolean;
        showSpeedType: boolean;
        showTrackType: boolean;
    }
    class UnitDisplayPreferences implements ISelfPersist<UnitDisplayPreferences_SaveState> {
        private _Dispatcher;
        private _Events;
        private _Name;
        private _DistanceUnit;
        private _HeightUnit;
        private _SpeedUnit;
        private _ShowVerticalSpeedPerSecond;
        private _ShowAltitudeType;
        private _ShowVerticalSpeedType;
        private _ShowSpeedType;
        private _ShowTrackType;
        private _FlightLevelTransitionAltitude;
        private _FlightLevelTransitionHeightUnit;
        private _FlightLevelHeightUnit;
        constructor(name?: string);
        getName: () => string;
        getDistanceUnit: () => string;
        setDistanceUnit: (value: string) => void;
        getHeightUnit: () => string;
        setHeightUnit: (value: string) => void;
        getSpeedUnit: () => string;
        setSpeedUnit: (value: string) => void;
        getShowVerticalSpeedPerSecond: () => boolean;
        setShowVerticalSpeedPerSecond: (value: boolean) => void;
        getShowAltitudeType: () => boolean;
        setShowAltitudeType: (value: boolean) => void;
        getShowVerticalSpeedType: () => boolean;
        setShowVerticalSpeedType: (value: boolean) => void;
        getShowSpeedType: () => boolean;
        setShowSpeedType: (value: boolean) => void;
        getShowTrackType: () => boolean;
        setShowTrackType: (value: boolean) => void;
        getFlightLevelTransitionAltitude: () => number;
        setFlightLevelTransitionAltitude: (value: number) => void;
        getFlightLevelTransitionHeightUnit: () => string;
        setFlightLevelTransitionHeightUnit: (value: string) => void;
        getFlightLevelHeightUnit: () => string;
        setFlightLevelHeightUnit: (value: string) => void;
        static getAltitudeUnitValues(): ValueText[];
        static getDistanceUnitValues(): ValueText[];
        static getSpeedUnitValues(): ValueText[];
        hookDistanceUnitChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookHeightUnitChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookSpeedUnitChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookShowVerticalSpeedPerSecondChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookShowAltitudeTypeChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookShowVerticalSpeedTypeChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookShowSpeedTypeChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookShowTrackTypeChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookFlightLevelTransitionAltitudeChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookFlightLevelTransitionHeightUnitChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookFlightLevelHeightUnitChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        hookUnitChanged: (callback: (dependency?: string) => void, forceThis?: Object) => IEventHandle;
        unhook: (hookResult: IEventHandle) => void;
        createOptionPane: (displayOrder: number) => OptionPane;
        saveState: () => void;
        loadState: () => UnitDisplayPreferences_SaveState;
        applyState: (settings: UnitDisplayPreferences_SaveState) => void;
        loadAndApplyState: () => void;
        private persistenceKey;
        private createSettings;
    }
}
declare namespace VRS {
    class ArrayHelper {
        except<T>(array: T[], exceptArray: T[], compareCallback?: (lhs: T, rhs: T) => boolean): T[];
        filter<T>(array: T[], allowItem: (item: T) => boolean): T[];
        findFirst<T>(array: T[], matchesCallback: (item: T) => boolean, noMatchesValue?: T): T;
        indexOf<T>(array: T[], value: T, fromIndex?: number): number;
        indexOfMatch<T>(array: T[], matchesCallback: (item: T) => boolean, fromIndex?: number): number;
        isArray(obj: any): boolean;
        normaliseOptionsArray<T>(defaultArray: T[], optionsArray: T[], isValidCallback: (item: T) => boolean): void;
        select<TArray, TResult>(array: TArray[], selectCallback: (item: TArray) => TResult): TResult[];
    }
    class BrowserHelper {
        private _ForceFrame;
        private _ForceFrameHasBeenRead;
        private _IsProbablyIPad;
        private _IsProbablyIPhone;
        private _IsProbablyAndroid;
        private _IsProbablyAndroidPhone;
        private _IsProbablyAndroidTablet;
        private _IsProbablyWindowsPhone;
        private _IsProbablyTablet;
        private _IsProbablyPhone;
        private _IsHighDpi;
        private _NotOnline;
        getForceFrame(): string;
        isProbablyIPad(): boolean;
        isProbablyIPhone(): boolean;
        isProbablyAndroid(): boolean;
        isProbablyAndroidPhone(): boolean;
        isProbablyAndroidTablet(): boolean;
        isProbablyWindowsPhone(): boolean;
        isProbablyTablet(): boolean;
        isProbablyPhone(): boolean;
        isHighDpi(): boolean;
        notOnline(): boolean;
        formUrl(url: string, params: Object, recursive: boolean): string;
        formVrsPageUrl(url: string, params?: Object, recursive?: boolean): string;
        getVrsPageTarget(target: string): string;
    }
    interface Colour {
        r: number;
        g: number;
        b: number;
        a?: number;
    }
    class ColourHelper {
        getWhite(): Colour;
        getRed(): Colour;
        getGreen(): Colour;
        getBlue(): Colour;
        getBlack(): Colour;
        getColourWheelScale(value: number, lowValue: number, highValue: number, invalidIsBelowLow?: boolean, stretchLowerValues?: boolean): Colour;
        private risingComponent(proportion);
        private fallingComponent(proportion);
        colourToCssString(colour: Colour): string;
    }
    class DateHelper {
        private _TicksInDay;
        getDatePortion(date: Date): Date;
        getDateTicks(date: Date): number;
        getTimePortion(date: Date): Date;
        getTimeTicks(date: Date): number;
        parse(text: string): Date;
        toIsoFormatString(date: Date, suppressTime: boolean, suppressTimeZone: boolean): string;
    }
    class DelayedTrace {
        private _Lines;
        constructor(title: string, delayMilliseconds: number);
        add(message: string): void;
    }
    class DomHelper {
        setAttribute(element: HTMLElement, name: string, value: string): void;
        removeAttribute(element: HTMLElement, name: string): void;
        setClass(element: HTMLElement, className: string): void;
        addClasses(element: HTMLElement, addClasses: string[]): void;
        removeClasses(element: HTMLElement, removeClasses: string[]): void;
        getClasses(element: HTMLElement): string[];
        setClasses(element: HTMLElement, classNames: string[]): void;
    }
    class EnumHelper {
        getEnumName(enumObject: Object, value: any): string;
        getEnumValues(enumObject: Object): any[];
    }
    class GreatCircle {
        isLatLngInBounds(lat: number, lng: number, bounds: IBounds): boolean;
        arrangeTwoPointsIntoBounds(point1: ILatLng, point2: ILatLng): IBounds;
    }
    class JsonHelper {
        convertMicrosoftDates(json: string): string;
    }
    class ObjectHelper {
        subclassOf(base: Function): Function;
    }
    class PageHelper {
        private _Indent;
        showModalWaitAnimation(onOff: boolean): void;
        showMessageBox(title: string, message: string): void;
        addIndentLog(message: string): Date;
        removeIndentLog(message: string, started: Date): void;
        indentLog(message: string, started?: Date): Date;
    }
    class TimeHelper {
        secondsToHoursMinutesSeconds(seconds: number): ITime;
        ticksToHoursMinutesSeconds(ticks: number): ITime;
    }
    class Utility {
        static ValueOrFuncReturningValue<T>(value: T | VoidFuncReturning<T>, defaultValue?: T): T;
    }
    interface PercentValue {
        value: number;
        isPercent: boolean;
    }
    class UnitConverter {
        convertDistance(value: number, fromUnit: DistanceEnum, toUnit: DistanceEnum): number;
        distanceUnitAbbreviation(unit: DistanceEnum): string;
        convertHeight(value: number, fromUnit: HeightEnum, toUnit: HeightEnum): number;
        heightUnitAbbreviation(unit: HeightEnum): string;
        heightUnitOverTimeAbbreviation(unit: HeightEnum, perSecond: boolean): string;
        convertSpeed(value: number, fromUnit: SpeedEnum, toUnit: SpeedEnum): number;
        speedUnitAbbreviation(unit: SpeedEnum): string;
        convertVerticalSpeed(verticalSpeed: number, fromUnit: HeightEnum, toUnit: HeightEnum, perSecond: boolean): number;
        getPixelsOrPercent(value: string | number): PercentValue;
    }
    interface ValueText_Settings {
        value: any;
        text?: string;
        textKey?: string;
        selected?: boolean;
    }
    class ValueText {
        private _Settings;
        constructor(settings: ValueText_Settings);
        getValue(): any;
        setValue(value: any): void;
        getSelected(): boolean;
        setSelected(value: boolean): void;
        getText(): string;
    }
    var arrayHelper: ArrayHelper;
    var browserHelper: BrowserHelper;
    var colourHelper: ColourHelper;
    var dateHelper: DateHelper;
    var domHelper: DomHelper;
    var enumHelper: EnumHelper;
    var greatCircle: GreatCircle;
    var jsonHelper: JsonHelper;
    var objectHelper: ObjectHelper;
    var pageHelper: PageHelper;
    var timeHelper: TimeHelper;
    var unitConverter: UnitConverter;
}
