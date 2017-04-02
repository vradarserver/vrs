var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var AircraftFilterPropertyHandler = (function (_super) {
        __extends(AircraftFilterPropertyHandler, _super);
        function AircraftFilterPropertyHandler(settings) {
            var _this = _super.call(this, $.extend({
                propertyEnumObject: VRS.AircraftFilterProperty
            }, settings)) || this;
            if (!settings.getValueCallback)
                throw 'You must supply a getValueCallback';
            _this.property = settings.property;
            _this.getValueCallback = settings.getValueCallback;
            return _this;
        }
        return AircraftFilterPropertyHandler;
    }(VRS.FilterPropertyHandler));
    VRS.AircraftFilterPropertyHandler = AircraftFilterPropertyHandler;
    VRS.aircraftFilterPropertyHandlers = VRS.aircraftFilterPropertyHandlers || {};
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Airport] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Airport,
        type: VRS.FilterPropertyType.TextListMatch,
        labelKey: 'Airport',
        isUpperCase: true,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.getAirportCodes(); },
        serverFilterName: 'fAir'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Altitude] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Altitude,
        type: VRS.FilterPropertyType.NumberRange,
        labelKey: 'Altitude',
        minimumValue: -2000,
        maximumValue: 100000,
        decimalPlaces: 0,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.altitude.val; },
        serverFilterName: 'fAlt',
        normaliseValue: function (value, unitDisplayPreferences) { return VRS.unitConverter.convertHeight(value, unitDisplayPreferences.getHeightUnit(), VRS.Height.Feet); }
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Callsign] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Callsign,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Callsign',
        isUpperCase: true,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.callsign.val; },
        serverFilterName: 'fCall'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Country] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Country,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Country',
        inputWidth: VRS.InputWidth.Long,
        getValueCallback: function (aircraft) { return aircraft.country.val; },
        serverFilterName: 'fCou'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Distance] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Distance,
        type: VRS.FilterPropertyType.NumberRange,
        labelKey: 'Distance',
        minimumValue: 0,
        maximumValue: 30000,
        decimalPlaces: 2,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.distanceFromHereKm.val; },
        serverFilterName: 'fDst',
        normaliseValue: function (value, unitDisplayPreferences) { return VRS.unitConverter.convertDistance(value, unitDisplayPreferences.getDistanceUnit(), VRS.Distance.Kilometre); }
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.EngineType] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.EngineType,
        type: VRS.FilterPropertyType.EnumMatch,
        labelKey: 'EngineType',
        getValueCallback: function (aircraft) { return aircraft.engineType.val; },
        getEnumValues: function () {
            return [
                new VRS.ValueText({ value: VRS.EngineType.None, textKey: 'None' }),
                new VRS.ValueText({ value: VRS.EngineType.Piston, textKey: 'Piston' }),
                new VRS.ValueText({ value: VRS.EngineType.Turbo, textKey: 'Turbo' }),
                new VRS.ValueText({ value: VRS.EngineType.Electric, textKey: 'Electric' }),
                new VRS.ValueText({ value: VRS.EngineType.Jet, textKey: 'Jet' })
            ];
        },
        serverFilterName: 'fEgt'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.HideNoPosition] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.HideNoPosition,
        type: VRS.FilterPropertyType.OnOff,
        labelKey: 'HideNoPosition',
        getValueCallback: function (aircraft) { return aircraft.hasPosition(); },
        serverFilterName: 'fNoPos'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Icao] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Icao,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Icao',
        isUpperCase: true,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.icao.val; },
        serverFilterName: 'fIco'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.IsMilitary] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.IsMilitary,
        type: VRS.FilterPropertyType.OnOff,
        labelKey: 'IsMilitary',
        getValueCallback: function (aircraft) { return aircraft.isMilitary.val; },
        serverFilterName: 'fMil'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.ModelIcao] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.ModelIcao,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'ModelIcao',
        isUpperCase: true,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.modelIcao.val; },
        serverFilterName: 'fTyp'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Operator] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Operator,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Operator',
        inputWidth: VRS.InputWidth.Long,
        getValueCallback: function (aircraft) { return aircraft.operator.val; },
        serverFilterName: 'fOp'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.OperatorCode] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.OperatorCode,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'OperatorCode',
        inputWidth: VRS.InputWidth.ThreeChar,
        getValueCallback: function (aircraft) { return aircraft.operatorIcao.val; },
        serverFilterName: 'fOpIcao'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Registration] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Registration,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'Registration',
        isUpperCase: true,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.registration.val; },
        serverFilterName: 'fReg'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Species] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Species,
        type: VRS.FilterPropertyType.EnumMatch,
        labelKey: 'Species',
        getValueCallback: function (aircraft) { return aircraft.species.val; },
        getEnumValues: function () {
            return [
                new VRS.ValueText({ value: VRS.Species.None, textKey: 'None' }),
                new VRS.ValueText({ value: VRS.Species.LandPlane, textKey: 'LandPlane' }),
                new VRS.ValueText({ value: VRS.Species.SeaPlane, textKey: 'SeaPlane' }),
                new VRS.ValueText({ value: VRS.Species.Amphibian, textKey: 'Amphibian' }),
                new VRS.ValueText({ value: VRS.Species.Helicopter, textKey: 'Helicopter' }),
                new VRS.ValueText({ value: VRS.Species.Gyrocopter, textKey: 'Gyrocopter' }),
                new VRS.ValueText({ value: VRS.Species.Tiltwing, textKey: 'Tiltwing' }),
                new VRS.ValueText({ value: VRS.Species.GroundVehicle, textKey: 'GroundVehicle' }),
                new VRS.ValueText({ value: VRS.Species.Tower, textKey: 'RadioMast' })
            ];
        },
        serverFilterName: 'fSpc'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Squawk] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Squawk,
        type: VRS.FilterPropertyType.NumberRange,
        labelKey: 'Squawk',
        minimumValue: 0,
        maximumValue: 7777,
        decimalPlaces: 0,
        inputWidth: VRS.InputWidth.SixChar,
        getValueCallback: function (aircraft) { return aircraft.squawk.val ? Number(aircraft.squawk.val) : 0; },
        serverFilterName: 'fSqk'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.UserTag] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.UserTag,
        type: VRS.FilterPropertyType.TextMatch,
        labelKey: 'UserTag',
        getValueCallback: function (aircraft) { return aircraft.userTag.val; },
        serverFilterName: 'fUt'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.UserInterested] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.UserInterested,
        type: VRS.FilterPropertyType.OnOff,
        labelKey: 'Interesting',
        getValueCallback: function (aircraft) { return aircraft.userInterested.val; },
        serverFilterName: 'fInt'
    });
    VRS.aircraftFilterPropertyHandlers[VRS.AircraftFilterProperty.Wtc] = new VRS.AircraftFilterPropertyHandler({
        property: VRS.AircraftFilterProperty.Wtc,
        type: VRS.FilterPropertyType.EnumMatch,
        labelKey: 'WakeTurbulenceCategory',
        getValueCallback: function (aircraft) { return aircraft.wakeTurbulenceCat.val; },
        getEnumValues: function () {
            return [
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.None, textKey: 'None' }),
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Light, textKey: 'WtcLight' }),
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Medium, textKey: 'WtcMedium' }),
                new VRS.ValueText({ value: VRS.WakeTurbulenceCategory.Heavy, textKey: 'WtcHeavy' })
            ];
        },
        serverFilterName: 'fWtc'
    });
    var AircraftFilter = (function (_super) {
        __extends(AircraftFilter, _super);
        function AircraftFilter(property, valueCondition) {
            return _super.call(this, {
                property: property,
                valueCondition: valueCondition,
                propertyEnumObject: VRS.AircraftFilterProperty,
                cloneCallback: function (property, filter) { return new VRS.AircraftFilter(property, filter); },
                filterPropertyHandlers: VRS.aircraftFilterPropertyHandlers
            }) || this;
        }
        AircraftFilter.prototype.passes = function (aircraft, options) {
            var result = false;
            var handler = VRS.aircraftFilterPropertyHandlers[this.getProperty()];
            if (handler) {
                var value = handler.getValueCallback(aircraft, options);
                if (value !== undefined) {
                    var typeHandler = VRS.filterPropertyTypeHandlers[handler.type];
                    if (typeHandler) {
                        result = typeHandler.valuePassesCallback(value, this.getValueCondition(), options);
                    }
                }
            }
            return result;
        };
        return AircraftFilter;
    }(VRS.Filter));
    VRS.AircraftFilter = AircraftFilter;
    var AircraftFilterHelper = (function (_super) {
        __extends(AircraftFilterHelper, _super);
        function AircraftFilterHelper() {
            return _super.call(this, {
                propertyEnumObject: VRS.AircraftFilterProperty,
                filterPropertyHandlers: VRS.aircraftFilterPropertyHandlers,
                createFilterCallback: function (propertyHandler, valueCondition) {
                    return new VRS.AircraftFilter(propertyHandler.property, valueCondition);
                }
            }) || this;
        }
        AircraftFilterHelper.prototype.aircraftPasses = function (aircraft, aircraftFilters, options) {
            var result = true;
            options = options || {};
            var length = aircraftFilters.length;
            for (var i = 0; i < length; ++i) {
                var aircraftFilter = aircraftFilters[i];
                result = aircraftFilter.passes(aircraft, options);
                if (!result)
                    break;
            }
            return result;
        };
        return AircraftFilterHelper;
    }(VRS.FilterHelper));
    VRS.AircraftFilterHelper = AircraftFilterHelper;
    VRS.aircraftFilterHelper = new VRS.AircraftFilterHelper();
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraftFilter.js.map