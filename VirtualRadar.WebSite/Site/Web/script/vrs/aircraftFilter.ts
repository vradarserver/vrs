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
 * @fileoverview Code that deals with condition-based filtering against lists of aircraft.
 */

namespace VRS
{
    /**
     * The settings to use when creating a new instance of an AircraftFilterPropertyHandler.
     */
    export interface AircraftFilterPropertyHandler_Settings extends FilterPropertyHandler_Settings
    {
        /**
         * The VRS.AircraftFilterProperty that this object handles.
         */
        property: AircraftFilterPropertyEnum;

        /**
         * The callback that returns the value for this property from the aircraft.
         */
        getValueCallback: (aircraft: Aircraft, options?: Filter_Options) => any;
    }

    /**
     * An object that describes a property on an aircraft, the ranges that the filters can be set to, its description and
     * how to fetch the property value from an aircraft object.
     */
    export class AircraftFilterPropertyHandler extends FilterPropertyHandler
    {
        // Keeping these as public fields for backwards compatability
        property:           AircraftFilterPropertyEnum;
        getValueCallback:   (aircraft: Aircraft, options?: Filter_Options) => any;

        constructor(settings: AircraftFilterPropertyHandler_Settings)
        {
            super($.extend({
                propertyEnumObject:     VRS.AircraftFilterProperty
            }, settings));

            if(!settings.getValueCallback) throw 'You must supply a getValueCallback';

            this.property = settings.property;
            this.getValueCallback = settings.getValueCallback;
        }
    }

    /**
     * The pre-built list of VRS (and potentially 3rd party) property handlers.
     */
    export var aircraftFilterPropertyHandlers: { [index: string]: AircraftFilterPropertyHandler } = VRS.aircraftFilterPropertyHandlers || {};

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Airport] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Airport,
        type:               VRS.FilterPropertyType.TextListMatch,
        labelKey:           'Airport',
        isUpperCase:        true,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.getAirportCodes(); },
        serverFilterName:   'fAir'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Altitude] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Altitude,
        type:               VRS.FilterPropertyType.NumberRange,
        labelKey:           'Altitude',
        minimumValue:       -2000,
        maximumValue:       100000,
        decimalPlaces:      0,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.altitude.val; },
        serverFilterName:   'fAlt',
        normaliseValue:     function(value, unitDisplayPreferences) { return VRS.unitConverter.convertHeight(value, unitDisplayPreferences.getHeightUnit(), VRS.Height.Feet); }
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Callsign] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Callsign,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Callsign',
        isUpperCase:        true,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.callsign.val; },
        serverFilterName:   'fCall'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Country] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Country,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Country',
        inputWidth:         VRS.InputWidth.Long,
        getValueCallback:   function(aircraft) { return aircraft.country.val; },
        serverFilterName:   'fCou'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Distance] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Distance,
        type:               VRS.FilterPropertyType.NumberRange,
        labelKey:           'Distance',
        minimumValue:       0,
        maximumValue:       30000,
        decimalPlaces:      2,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.distanceFromHereKm.val; },
        serverFilterName:   'fDst',
        normaliseValue:     function(value, unitDisplayPreferences) { return VRS.unitConverter.convertDistance(value, unitDisplayPreferences.getDistanceUnit(), VRS.Distance.Kilometre); }
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.EngineType] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.EngineType,
        type:               VRS.FilterPropertyType.EnumMatch,
        labelKey:           'EngineType',
        getValueCallback:   function(aircraft) { return aircraft.engineType.val; },
        getEnumValues:      function() { return [
            new VRS.ValueText({ value: VRS.EngineType.None,       textKey: 'None' }),
            new VRS.ValueText({ value: VRS.EngineType.Piston,     textKey: 'Piston' }),
            new VRS.ValueText({ value: VRS.EngineType.Turbo,      textKey: 'Turbo' }),
            new VRS.ValueText({ value: VRS.EngineType.Electric,   textKey: 'Electric' }),
            new VRS.ValueText({ value: VRS.EngineType.Jet,        textKey: 'Jet' }),
            new VRS.ValueText({ value: VRS.EngineType.Rocket,     textKey: 'Rocket' })
        ];},
        serverFilterName:   'fEgt'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.HideNoPosition] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.HideNoPosition,
        type:               VRS.FilterPropertyType.OnOff,
        labelKey:           'HideNoPosition',
        getValueCallback:   function(aircraft) { return aircraft.hasPosition(); },
        serverFilterName:   'fNoPos'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Icao] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Icao,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Icao',
        isUpperCase:        true,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.icao.val; },
        serverFilterName:   'fIco'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.IsMilitary] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.IsMilitary,
        type:               VRS.FilterPropertyType.OnOff,
        labelKey:           'IsMilitary',
        getValueCallback:   function(aircraft) { return aircraft.isMilitary.val; },
        serverFilterName:   'fMil'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.ModelIcao] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.ModelIcao,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'ModelIcao',
        isUpperCase:        true,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.modelIcao.val; },
        serverFilterName:   'fTyp'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Operator] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Operator,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Operator',
        inputWidth:         VRS.InputWidth.Long,
        getValueCallback:   function(aircraft) { return aircraft.operator.val; },
        serverFilterName:   'fOp'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.OperatorCode] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.OperatorCode,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'OperatorCode',
        inputWidth:         VRS.InputWidth.ThreeChar,
        getValueCallback:   function(aircraft) { return aircraft.operatorIcao.val; },
        serverFilterName:   'fOpIcao'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Registration] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Registration,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'Registration',
        isUpperCase:        true,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.registration.val; },
        serverFilterName:   'fReg'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Species] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Species,
        type:               VRS.FilterPropertyType.EnumMatch,
        labelKey:           'Species',
        getValueCallback:   function(aircraft) { return aircraft.species.val; },
        getEnumValues:      function() { return [
            new VRS.ValueText({ value: VRS.Species.None,          textKey: 'None' }),
            new VRS.ValueText({ value: VRS.Species.LandPlane,     textKey: 'LandPlane' }),
            new VRS.ValueText({ value: VRS.Species.SeaPlane,      textKey: 'SeaPlane' }),
            new VRS.ValueText({ value: VRS.Species.Amphibian,     textKey: 'Amphibian' }),
            new VRS.ValueText({ value: VRS.Species.Helicopter,    textKey: 'Helicopter' }),
            new VRS.ValueText({ value: VRS.Species.Gyrocopter,    textKey: 'Gyrocopter' }),
            new VRS.ValueText({ value: VRS.Species.Tiltwing,      textKey: 'Tiltwing' }),
            new VRS.ValueText({ value: VRS.Species.GroundVehicle, textKey: 'GroundVehicle' }),
            new VRS.ValueText({ value: VRS.Species.Tower,         textKey: 'RadioMast' })
        ];},
        serverFilterName:   'fSpc'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Squawk] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Squawk,
        type:               VRS.FilterPropertyType.NumberRange,
        labelKey:           'Squawk',
        minimumValue:       0,
        maximumValue:       7777,
        decimalPlaces:      0,
        inputWidth:         VRS.InputWidth.SixChar,
        getValueCallback:   function(aircraft) { return aircraft.squawk.val ? Number(aircraft.squawk.val) : 0; },
        serverFilterName:   'fSqk'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.UserTag] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.UserTag,
        type:               VRS.FilterPropertyType.TextMatch,
        labelKey:           'UserTag',
        getValueCallback:   function(aircraft) { return aircraft.userTag.val; },
        serverFilterName:   'fUt'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.UserInterested] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.UserInterested,
        type:               VRS.FilterPropertyType.OnOff,
        labelKey:           'Interesting',
        getValueCallback:   function(aircraft) { return aircraft.userInterested.val; },
        serverFilterName:   'fInt'
    });

    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Wtc] = new VRS.AircraftFilterPropertyHandler({
        property:           VRS.AircraftFilterProperty.Wtc,
        type:               VRS.FilterPropertyType.EnumMatch,
        labelKey:           'WakeTurbulenceCategory',
        getValueCallback:   function(aircraft) { return aircraft.wakeTurbulenceCat.val; },
        getEnumValues:      function() { return [
            new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.None,   textKey: 'None' }),
            new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Light,  textKey: 'WtcLight' }),
            new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Medium, textKey: 'WtcMedium' }),
            new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Heavy,  textKey: 'WtcHeavy' })
        ];},
        serverFilterName:   'fWtc'
    });

    /**
     * Joins together a property identifier and a filter object to allow a condition to be tested against an aircraft's property.
     * @param {VRS.AircraftFilterProperty}  property        A VRS.AircraftFilterProperty enum value.
     * @param {VRS_ANY_VALUECONDITION}      valueCondition  The object that describes the condition and values to compare against an aircraft's property.
     * @constructor
     * @augments VRS.Filter
     */
    export class AircraftFilter extends Filter
    {
        constructor(property: AircraftFilterPropertyEnum, valueCondition: ValueCondition)
        {
            super({
                property:               property,
                valueCondition:         valueCondition,
                propertyEnumObject:     VRS.AircraftFilterProperty,
                cloneCallback:          function(property, filter) { return new VRS.AircraftFilter(property, filter); },
                filterPropertyHandlers: VRS.aircraftFilterPropertyHandlers
            });
        }

        /**
         * Returns true if the aircraft's value for the property matches the condition described by the parameters.
         */
        passes(aircraft: Aircraft, options: Filter_Options) : boolean
        {
            var result = false;
            var handler = VRS.aircraftFilterPropertyHandlers[this.getProperty()];
            if(handler) {
                var value = handler.getValueCallback(aircraft, options);
                if(value !== undefined) {
                    var typeHandler = VRS.filterPropertyTypeHandlers[handler.type];
                    if(typeHandler) {
                        result = typeHandler.valuePassesCallback(value, this.getValueCondition(), options);
                    }
                }
            }

            return result;
        }
    }

    /**
     * A helper object that can deal with common routine tasks when working with aircraft property filters.
     */
    export class AircraftFilterHelper extends FilterHelper
    {
        constructor()
        {
            super({
                propertyEnumObject:     VRS.AircraftFilterProperty,
                filterPropertyHandlers: VRS.aircraftFilterPropertyHandlers,
                createFilterCallback:   function(propertyHandler: AircraftFilterPropertyHandler, valueCondition: ValueCondition) {
                    return new VRS.AircraftFilter(propertyHandler.property, valueCondition);
                }
            });
        }

        /**
         * Takes an aircraft and a list of filters and returns true if the aircraft passes them.
         */
        aircraftPasses(aircraft: Aircraft, aircraftFilters: AircraftFilter[], options?: Filter_Options) : boolean
        {
            var result = true;
            options = options || {};

            var length = aircraftFilters.length;
            for(var i = 0;i < length;++i) {
                var aircraftFilter = aircraftFilters[i];
                result = aircraftFilter.passes(aircraft, options);
                if(!result) break;
            }

            return result;
        }
    }

    /*
     * Pre-builts
     */
    export var aircraftFilterHelper = new VRS.AircraftFilterHelper();
}
