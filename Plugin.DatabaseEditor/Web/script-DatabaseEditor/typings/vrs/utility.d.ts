declare namespace VRS
{
    export interface ArrayHelper
    {
        except(array: any[], exceptArray: any[], compareCallback: (lhs:any, rhs:any) => boolean) : any[];

        filter(array: any[], allowItem: (any) => boolean) : any[];

        findFirst(array: any[], matchesCallback: (any) => boolean, noMatchesValue: any) : any;

        indexOf(array: any[], value: any, fromIndex?: number) : number;

        indexOfMatch(array: any[], matchesCallback: (any) => boolean, fromIndex?: number) : number;

        isArray(obj: any) : boolean;

        normaliseOptionsArray(defaultArray: any[], optionsArray: any[], isValidCallback: (any) => boolean);

        select(array: any[], selectCallback: (any) => any) : any[];
    }


    export interface BrowserHelper
    {
        getForceFrame() : string;

        isProbablyIPad() : boolean;

        isProbablyIPhone() : boolean;

        isProbablyAndroid() : boolean;

        isProbablyAndroidPhone() : boolean;

        isProbablyAndroidTablet() : boolean;

        isProbablyWindowsPhone() : boolean;

        isProbablyTablet() : boolean;

        isProbablyPhone() : boolean;

        isHighDpi() : boolean;

        notOnline() : boolean;

        formUrl(url: string, params?: any, recursive?: boolean) : string;

        formVrsPageUrl(url: string, params?: any, recursive?: boolean) : string;

        getVrsPageTarget(target: string) : string;
    }

    export interface VRS_COLOUR
    {
        r: number;
        g: number;
        b: number;
    }

    export interface ColourHelper
    {
        getWhite() : VRS_COLOUR;

        getRed() : VRS_COLOUR;

        getGreen() : VRS_COLOUR;

        getBlue() : VRS_COLOUR;

        getBlack() : VRS_COLOUR;

        getColourWheelScale(value: number, lowValue: number, highValue: number, invalidIsBelowLow: boolean, stretchLowerValues: boolean) : VRS_COLOUR;

        colourToCssString(colour: VRS_COLOUR) : string;
    }

    export interface DateHelper
    {
        getDatePortion(date: Date) : Date;

        getDateTicks(date: Date) : number;

        getTimePortion(date: Date) : Date;

        getTimeTicks(date: Date) : number;

        parse(text: string) : Date;

        toIsoFormatString(date: Date, suppressTime: boolean, suppressTimeZone: boolean) : string;
    }

    export interface DelayedTrace
    {
        constructor(title: string, delayMilliseconds: number);

        add(message: string);
    }

    export interface DomHelper
    {
        setAttribute(element: Element, name: string, value: string);

        removeAttribute(element: Element, name: string);

        setClass(element: Element, className: string);

        addClasses(element: Element, addClasses: string[]);

        removeClasses(element: Element, removeClasses: string[]);

        getClasses(element: Element) : string[];

        setClasses(element: Element, classNames: string[]);
    }

    export interface EnumHelper
    {
        getEnumName(enumObject: any, value: any) : string;

        getEnumValues(enumObject: any) : any[];
    }

    export interface VRS_BOUNDS
    {
        tlLat: number;
        tlLng: number;
        brLat: number;
        brLng: number;
    }

    export interface VRS_LAT_LNG
    {
        lat: number;
        lng: number;
    }

    export interface GreatCircle
    {
        isLatLngInBounds(lat: number, lng: number, bounds: VRS_BOUNDS) : boolean;

        arrangeTwoPointsIntoBounds(point1: VRS_LAT_LNG, point2: VRS_LAT_LNG) : VRS_BOUNDS;
    }

    export interface JsonHelper
    {
        convertMicrosoftDates(json: string) : string;
    }

    export interface ObjectHelper
    {
        subclassOf(base: any) : any;
    }

    export interface PageHelper
    {
        showModalWaitAnimation(onOff: boolean);

        showMessageBox(title: string, message: string);

        addIndentLog(message: string) : string;

        removeIndentLog(message: string, started?: Date);

        indentLog(message: string, started?: Date);
    }

    export interface VRS_TIME
    {
        hours: number;
        minutes: number;
        seconds: number;
    }

    export interface TimeHelper
    {
        secondsToHoursMinutesSeconds(seconds: number) : VRS_TIME;

        ticksToHoursMinutesSeconds(ticks: number) : VRS_TIME;
    }

    export interface VRS_VALUE_PERCENT
    {
        value: number;
        isPercent: boolean;
    }

    export interface UnitConverter
    {
        convertDistance(value: number, fromUnit: string, toUnit: string) : number;

        distanceUnitAbbreviation(unit: string) : string;

        convertHeight(value: number, fromUnit: string, toUnit: string) : number;

        heightUnitAbbreviation(unit: string) : string;

        heightUnitOverTimeAbbreviation(unit:string, perSecond: boolean) : string;

        convertSpeed(value: number, fromUnit: string, toUnit: string) : number;

        speedUnitAbbreviation(unit: string) : string;

        convertVerticalSpeed(verticalSpeed: number, fromUnit: string, toUnit: string, perSecond: boolean) : number;

        getPixelsOrPercent(value: string) : VRS_VALUE_PERCENT;
        getPixelsOrPercent(value: number) : VRS_VALUE_PERCENT;
    }

    export interface VRS_VALUE
    {
        value: any;
        text?: string;
        textKey?: string;
        selected?: boolean;
    }

    export interface ValueText
    {
        constructor(settings: VRS_VALUE);

        setValue(value: any);

        setSelected(value: boolean);

        getText() : string;
    }

    export var arrayHelper : VRS.ArrayHelper;
    export var browserHelper : VRS.BrowserHelper;
    export var colourHelper : VRS.ColourHelper;
    export var dateHelper : VRS.DateHelper;
    export var domHelper : VRS.DomHelper;
    export var enumHelper : VRS.EnumHelper;
    export var greatCircle : VRS.GreatCircle;
    export var jsonHelper : VRS.JsonHelper;
    export var objectHelper : VRS.ObjectHelper;
    export var pageHelper : VRS.PageHelper;
    export var timeHelper : VRS.TimeHelper;
    export var unitConverter : VRS.UnitConverter;
} 