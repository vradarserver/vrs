declare namespace VRS
{
    /**
     * Describes a rectangle on the surface of the earth.
     */
    export interface IBounds
    {
        tlLat: number;
        tlLng: number;
        brLat: number;
        brLng: number;
    }

    /**
     * Describes a colour with an optional alpha.
     */
    export interface IColour
    {
        r:   number;
        g:   number;
        b:   number;
        a?:  number;
    }

    /**
     * Describes a latitude and longitude.
     */
    export interface ILatLng
    {
        lat: number;
        lng: number;
    }

    /**
     * Describes a value that may or may not be a percentage.
     */
    export interface IPercentValue
    {
        value:      number;
        isPercent:  boolean;
    }

    /**
     * Describes a size.
     */
    export interface ISize
    {
        width:    number;
        height:   number;
    }

    /**
     * Describes a time without reference to a date.
     */
    export interface ITime
    {
        hours:      number;
        minutes:    number;
        seconds:    number;
    }
} 