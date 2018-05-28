var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.suppressTrails = VRS.globalOptions.suppressTrails || false;
    VRS.globalOptions.aircraftHideUncertainCallsigns = VRS.globalOptions.aircraftHideUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftHideUncertainCallsigns : false;
    VRS.globalOptions.aircraftMaxAvgSignalLevelHistory = VRS.globalOptions.aircraftMaxAvgSignalLevelHistory !== undefined ? VRS.globalOptions.aircraftMaxAvgSignalLevelHistory : 6;
    var Value = (function () {
        function Value(value) {
            this.chg = false;
            this.val = value;
        }
        Value.prototype.setValue = function (value) {
            this.val = value;
            this.chg = true;
        };
        return Value;
    }());
    VRS.Value = Value;
    var StringValue = (function (_super) {
        __extends(StringValue, _super);
        function StringValue(value) {
            return _super.call(this, value) || this;
        }
        return StringValue;
    }(Value));
    VRS.StringValue = StringValue;
    var BoolValue = (function (_super) {
        __extends(BoolValue, _super);
        function BoolValue(value) {
            return _super.call(this, value) || this;
        }
        return BoolValue;
    }(Value));
    VRS.BoolValue = BoolValue;
    var NumberValue = (function (_super) {
        __extends(NumberValue, _super);
        function NumberValue(value) {
            return _super.call(this, value) || this;
        }
        return NumberValue;
    }(Value));
    VRS.NumberValue = NumberValue;
    var ArrayValue = (function () {
        function ArrayValue(initialArray) {
            this.arr = [];
            this.chg = false;
            this.chgIdx = -1;
            this.trimStartCount = 0;
            this.arr = initialArray || [];
        }
        ArrayValue.prototype.setValue = function (value) {
            if (value && value.length) {
                this.arr = value;
                this.chg = true;
                this.chgIdx = 0;
                this.trimStartCount = 0;
            }
        };
        ArrayValue.prototype.setNoChange = function () {
            this.chg = false;
            this.chgIdx = -1;
            this.trimStartCount = 0;
        };
        ArrayValue.prototype.resetArray = function () {
            if (this.arr.length > 0) {
                this.trimStartCount = this.arr.length;
                this.arr = [];
                this.chg = true;
                this.chgIdx = -1;
            }
        };
        ArrayValue.prototype.trimStart = function (trimCount) {
            if (trimCount > 0) {
                if (trimCount > this.arr.length)
                    trimCount = this.arr.length;
                this.arr.splice(0, trimCount);
                this.trimStartCount = trimCount;
                this.chg = true;
            }
        };
        return ArrayValue;
    }());
    VRS.ArrayValue = ArrayValue;
    var RouteValue = (function (_super) {
        __extends(RouteValue, _super);
        function RouteValue(value) {
            return _super.call(this, value) || this;
        }
        RouteValue.prototype.getAirportCode = function () {
            if (this._AirportCodeDerivedFromVal !== this.val) {
                this._AirportCodeDerivedFromVal = this.val;
                this._AirportCode = VRS.format.routeAirportCode(this._AirportCodeDerivedFromVal);
            }
            return this._AirportCode;
        };
        return RouteValue;
    }(Value));
    VRS.RouteValue = RouteValue;
    var AirportDataThumbnailValue = (function (_super) {
        __extends(AirportDataThumbnailValue, _super);
        function AirportDataThumbnailValue(value) {
            var _this = _super.call(this, value) || this;
            _this._LastResetChgValue = false;
            return _this;
        }
        AirportDataThumbnailValue.prototype.resetChg = function () {
            if (this.chg && this._LastResetChgValue)
                this.chg = false;
            this._LastResetChgValue = this.chg;
        };
        return AirportDataThumbnailValue;
    }(Value));
    VRS.AirportDataThumbnailValue = AirportDataThumbnailValue;
    var ShortTrailValue = (function () {
        function ShortTrailValue(latitude, longitude, posnTick, altitude, speed) {
            this.lat = latitude;
            this.lng = longitude;
            this.posnTick = posnTick;
            this.altitude = altitude;
            this.speed = speed;
        }
        return ShortTrailValue;
    }());
    VRS.ShortTrailValue = ShortTrailValue;
    var FullTrailValue = (function () {
        function FullTrailValue(latitude, longitude, heading, altitude, speed) {
            this.lat = latitude;
            this.lng = longitude;
            this.heading = heading;
            this.altitude = altitude;
            this.speed = speed;
            this.chg = false;
        }
        return FullTrailValue;
    }());
    VRS.FullTrailValue = FullTrailValue;
    var Aircraft = (function () {
        function Aircraft() {
            this._AircraftListFetcher = null;
            this.signalLevelHistory = [];
            this.id = 0;
            this.secondsTracked = 0;
            this.updateCounter = 0;
            this.receiverId = new NumberValue();
            this.icao = new StringValue();
            this.icaoInvalid = new BoolValue();
            this.registration = new StringValue();
            this.altitude = new NumberValue();
            this.geometricAltitude = new NumberValue();
            this.airPressureInHg = new NumberValue();
            this.altitudeType = new Value();
            this.targetAltitude = new NumberValue();
            this.callsign = new StringValue();
            this.callsignSuspect = new BoolValue();
            this.latitude = new NumberValue();
            this.longitude = new NumberValue();
            this.isMlat = new BoolValue();
            this.positionTime = new NumberValue();
            this.positionStale = new BoolValue();
            this.speed = new NumberValue();
            this.speedType = new Value();
            this.verticalSpeed = new NumberValue();
            this.verticalSpeedType = new Value();
            this.heading = new NumberValue();
            this.headingIsTrue = new BoolValue();
            this.targetHeading = new NumberValue();
            this.manufacturer = new StringValue();
            this.serial = new StringValue();
            this.yearBuilt = new StringValue();
            this.model = new StringValue();
            this.modelIcao = new StringValue();
            this.from = new RouteValue();
            this.to = new RouteValue();
            this.via = new ArrayValue();
            this.operator = new StringValue();
            this.operatorIcao = new StringValue();
            this.squawk = new StringValue();
            this.isEmergency = new BoolValue();
            this.distanceFromHereKm = new NumberValue();
            this.bearingFromHere = new NumberValue();
            this.wakeTurbulenceCat = new Value();
            this.countEngines = new StringValue();
            this.engineType = new Value();
            this.enginePlacement = new NumberValue();
            this.species = new Value();
            this.isMilitary = new BoolValue();
            this.isTisb = new BoolValue();
            this.country = new StringValue();
            this.hasPicture = new BoolValue();
            this.pictureWidth = new NumberValue();
            this.pictureHeight = new NumberValue();
            this.countFlights = new NumberValue();
            this.countMessages = new NumberValue();
            this.isOnGround = new BoolValue();
            this.userTag = new StringValue();
            this.userInterested = new BoolValue();
            this.signalLevel = new NumberValue();
            this.averageSignalLevel = new NumberValue();
            this.airportDataThumbnails = new AirportDataThumbnailValue();
            this.transponderType = new Value();
            this.shortTrail = new ArrayValue();
            this.fullTrail = new ArrayValue();
            this.formatSerial = function () {
                return VRS.format.serial(this.serial.val);
            };
        }
        Aircraft.prototype.applyJson = function (aircraftJson, aircraftListFetcher, settings) {
            this.id = aircraftJson.Id;
            this.secondsTracked = aircraftJson.TSecs;
            this._AircraftListFetcher = aircraftListFetcher;
            ++this.updateCounter;
            this.setValue(this.receiverId, aircraftJson.Rcvr);
            this.setValue(this.icao, aircraftJson.Icao);
            this.setValue(this.icaoInvalid, aircraftJson.Bad);
            this.setValue(this.registration, aircraftJson.Reg);
            this.setValue(this.altitude, aircraftJson.Alt);
            this.setValue(this.geometricAltitude, aircraftJson.GAlt);
            this.setValue(this.airPressureInHg, aircraftJson.InHg);
            this.setValue(this.altitudeType, aircraftJson.AltT);
            this.setValue(this.targetAltitude, aircraftJson.TAlt);
            this.setValue(this.callsign, aircraftJson.Call);
            this.setValue(this.callsignSuspect, aircraftJson.CallSus);
            this.setValue(this.latitude, aircraftJson.Lat);
            this.setValue(this.longitude, aircraftJson.Long);
            this.setValue(this.isMlat, aircraftJson.Mlat);
            this.setValue(this.positionTime, aircraftJson.PosTime);
            this.setValue(this.positionStale, !!aircraftJson.PosStale, true);
            this.setValue(this.speed, aircraftJson.Spd);
            this.setValue(this.speedType, aircraftJson.SpdTyp);
            this.setValue(this.verticalSpeed, aircraftJson.Vsi);
            this.setValue(this.verticalSpeedType, aircraftJson.VsiT);
            this.setValue(this.heading, aircraftJson.Trak);
            this.setValue(this.headingIsTrue, aircraftJson.TrkH);
            this.setValue(this.targetHeading, aircraftJson.TTrk);
            this.setValue(this.manufacturer, aircraftJson.Man);
            this.setValue(this.serial, aircraftJson.CNum);
            this.setValue(this.yearBuilt, aircraftJson.Year);
            this.setValue(this.model, aircraftJson.Mdl);
            this.setValue(this.modelIcao, aircraftJson.Type);
            this.setValue(this.from, aircraftJson.From);
            this.setValue(this.to, aircraftJson.To);
            this.setValue(this.operator, aircraftJson.Op);
            this.setValue(this.operatorIcao, aircraftJson.OpIcao);
            this.setValue(this.squawk, aircraftJson.Sqk);
            this.setValue(this.isEmergency, aircraftJson.Help);
            this.setValue(this.distanceFromHereKm, aircraftJson.Dst, true);
            this.setValue(this.bearingFromHere, aircraftJson.Brng, true);
            this.setValue(this.wakeTurbulenceCat, aircraftJson.WTC);
            this.setValue(this.countEngines, aircraftJson.Engines);
            this.setValue(this.engineType, aircraftJson.EngType);
            this.setValue(this.enginePlacement, aircraftJson.EngMount);
            this.setValue(this.species, aircraftJson.Species);
            this.setValue(this.isMilitary, aircraftJson.Mil);
            this.setValue(this.isTisb, aircraftJson.Tisb);
            this.setValue(this.country, aircraftJson.Cou);
            this.setValue(this.hasPicture, aircraftJson.HasPic && settings.picturesEnabled);
            this.setValue(this.pictureWidth, aircraftJson.PicX);
            this.setValue(this.pictureHeight, aircraftJson.PicY);
            this.setValue(this.countFlights, aircraftJson.FlightsCount);
            this.setValue(this.countMessages, aircraftJson.CMsgs);
            this.setValue(this.isOnGround, aircraftJson.Gnd);
            this.setValue(this.userTag, aircraftJson.Tag);
            this.setValue(this.userInterested, aircraftJson.Interested);
            this.setValue(this.transponderType, aircraftJson.Trt);
            this.setRouteArray(this.via, aircraftJson.Stops);
            if (aircraftJson.HasSig !== undefined) {
                this.setValue(this.signalLevel, aircraftJson.Sig);
            }
            this.recordSignalLevelHistory(this.signalLevel.val);
            if (!VRS.globalOptions.suppressTrails) {
                this.setShortTrailArray(this.shortTrail, aircraftJson.Cos, aircraftJson.ResetTrail, settings.shortTrailTickThreshold, aircraftJson.TT);
                this.setFullTrailArray(this.fullTrail, aircraftJson.Cot, aircraftJson.ResetTrail, aircraftJson.TT);
            }
            if (VRS.globalOptions.aircraftHideUncertainCallsigns && this.callsignSuspect.val) {
                this.callsign.val = undefined;
                this.callsign.chg = false;
            }
            this.airportDataThumbnails.resetChg();
        };
        Aircraft.prototype.setValue = function (field, jsonValue, alwaysSent) {
            if (jsonValue === undefined)
                field.chg = false;
            else if (!alwaysSent) {
                field.val = jsonValue;
                field.chg = true;
            }
            else {
                field.chg = field.val !== jsonValue;
                if (field.chg)
                    field.val = jsonValue;
            }
        };
        Aircraft.prototype.setRouteArray = function (field, jsonArray) {
            field.setNoChange();
            if (jsonArray) {
                field.arr = [];
                field.chg = true;
                field.chgIdx = 0;
                $.each(jsonArray, function (idx, val) {
                    field.arr.push(new VRS.RouteValue(val));
                });
            }
        };
        Aircraft.prototype.setShortTrailArray = function (field, jsonArray, resetTrail, shortTrailTickThreshold, trailType) {
            field.setNoChange();
            var length = field.arr.length;
            var i = 0;
            if (length > 0) {
                if (resetTrail)
                    field.resetArray();
                else {
                    if (shortTrailTickThreshold === -1)
                        field.resetArray();
                    else {
                        var indexFirstValidCoordinate = -1;
                        for (i = 0; i < length; ++i) {
                            if (field.arr[i].posnTick >= shortTrailTickThreshold) {
                                indexFirstValidCoordinate = i;
                                break;
                            }
                        }
                        if (indexFirstValidCoordinate !== 0) {
                            if (indexFirstValidCoordinate === -1)
                                field.resetArray();
                            else
                                field.trimStart(indexFirstValidCoordinate);
                        }
                    }
                }
                length = field.arr.length;
            }
            var addLength = jsonArray ? jsonArray.length : 0;
            if (addLength > 0) {
                field.chg = true;
                field.chgIdx = length;
                for (i = 0; i < addLength;) {
                    var lat = jsonArray[i++];
                    var lng = jsonArray[i++];
                    var tik = jsonArray[i++];
                    var alt = trailType === 'a' ? jsonArray[i++] : undefined;
                    var spd = trailType === 's' ? jsonArray[i++] : undefined;
                    field.arr.push(new VRS.ShortTrailValue(lat, lng, tik, alt, spd));
                }
            }
        };
        Aircraft.prototype.setFullTrailArray = function (field, jsonArray, resetTrail, trailType) {
            field.setNoChange();
            var length = field.arr.length;
            var lastTrail = length ? field.arr[length - 1] : null;
            var lastButOne = length > 1 ? field.arr[length - 2] : null;
            if (lastTrail)
                lastTrail.chg = false;
            if (resetTrail) {
                field.resetArray();
                length = field.arr.length;
            }
            var addLength = jsonArray ? jsonArray.length : 0;
            if (addLength > 0) {
                field.chg = true;
                var i;
                field.chgIdx = length;
                for (i = 0; i < addLength;) {
                    var lat = jsonArray[i++];
                    var lng = jsonArray[i++];
                    var hdg = jsonArray[i++];
                    var alt = trailType === 'a' ? jsonArray[i++] : undefined;
                    var spd = trailType === 's' ? jsonArray[i++] : undefined;
                    if (hdg && lastTrail && lastButOne &&
                        lastTrail.heading === hdg && lastButOne.heading === hdg &&
                        lastTrail.altitude === alt && lastButOne.altitude === alt &&
                        lastTrail.speed === spd && lastButOne.speed === spd) {
                        if (lastTrail.lat !== lat || lastTrail.lng !== lng) {
                            lastTrail.lat = lat;
                            lastTrail.lng = lng;
                            lastTrail.chg = true;
                        }
                    }
                    else {
                        lastButOne = lastTrail;
                        lastTrail = new VRS.FullTrailValue(lat, lng, hdg, alt, spd);
                        field.arr.push(lastTrail);
                        ++length;
                    }
                }
                if (field.chgIdx === length) {
                    field.chgIdx = -1;
                }
                else {
                    var oldLastIndex = field.chgIdx - 1;
                    for (i = Math.max(0, field.chgIdx - 1); i < length; ++i) {
                        if (i === oldLastIndex && field.arr[i].chg)
                            --field.chgIdx;
                        field.arr[i].chg = false;
                    }
                }
            }
        };
        Aircraft.prototype.recordSignalLevelHistory = function (signalLevel) {
            var averageSignalLevel = 0;
            if (VRS.globalOptions.aircraftMaxAvgSignalLevelHistory) {
                if (signalLevel === null) {
                    if (this.signalLevelHistory.length !== 0)
                        this.signalLevelHistory = [];
                }
                else {
                    var i;
                    averageSignalLevel = signalLevel;
                    var length = this.signalLevelHistory.length;
                    if (length < VRS.globalOptions.aircraftMaxAvgSignalLevelHistory) {
                        for (i = 0; i < length; ++i) {
                            averageSignalLevel += this.signalLevelHistory[i];
                        }
                        this.signalLevelHistory.push(signalLevel);
                        ++length;
                    }
                    else {
                        var shiftLength = length - 1;
                        for (i = 0; i < shiftLength; ++i) {
                            var useValue = this.signalLevelHistory[i + 1];
                            this.signalLevelHistory[i] = useValue;
                            averageSignalLevel += useValue;
                        }
                        this.signalLevelHistory[shiftLength] = signalLevel;
                    }
                    averageSignalLevel = Math.floor(averageSignalLevel / length);
                }
            }
            this.setValue(this.averageSignalLevel, averageSignalLevel, true);
        };
        Aircraft.prototype.hasPosition = function () {
            return this.positionTime.val > 0;
        };
        Aircraft.prototype.getPosition = function () {
            return this.hasPosition() ? { lat: this.latitude.val, lng: this.longitude.val } : null;
        };
        Aircraft.prototype.positionWithinBounds = function (bounds) {
            var result = this.hasPosition();
            if (result)
                result = VRS.greatCircle.isLatLngInBounds(this.latitude.val, this.longitude.val, bounds);
            return result;
        };
        Aircraft.prototype.hasRoute = function () {
            return !!this.from.val && !!this.to.val;
        };
        Aircraft.prototype.hasRouteChanged = function () {
            return this.from.chg || this.to.chg || this.via.chg;
        };
        Aircraft.prototype.getViaAirports = function () {
            var result = [];
            var length = this.via.arr.length;
            for (var i = 0; i < length; ++i) {
                result.push(this.via.arr[i].val);
            }
            return result;
        };
        Aircraft.prototype.getAirportCodes = function (distinctOnly) {
            distinctOnly = !!distinctOnly;
            var result = [];
            var addAirportCode = function (code) {
                if (code && (!distinctOnly || VRS.arrayHelper.indexOf(result, code) === -1)) {
                    result.push(code);
                }
            };
            if (this.from.val)
                addAirportCode(this.from.getAirportCode());
            var length = this.via.arr.length;
            for (var i = 0; i < length; ++i) {
                addAirportCode(this.via.arr[i].getAirportCode());
            }
            if (this.to.val)
                addAirportCode(this.to.getAirportCode());
            return result;
        };
        Aircraft.prototype.getMixedAltitude = function (usePressureAltitude) {
            return usePressureAltitude ? this.altitude.val : this.geometricAltitude.val;
        };
        Aircraft.prototype.hasMixedAltitudeChanged = function (usePressureAltitude) {
            return usePressureAltitude ? this.altitude.chg : this.geometricAltitude.chg;
        };
        Aircraft.prototype.isAircraftSpecies = function () {
            return this.species.val !== VRS.Species.GroundVehicle && this.species.val !== VRS.Species.Tower;
        };
        Aircraft.prototype.convertSpeed = function (toUnit) {
            var result = this.speed.val;
            if (result !== undefined && toUnit !== VRS.Speed.Knots) {
                result = VRS.unitConverter.convertSpeed(result, VRS.Speed.Knots, toUnit);
            }
            return result;
        };
        Aircraft.prototype.convertAltitude = function (toUnit) {
            return this.convertMixedAltitude(true, toUnit);
        };
        Aircraft.prototype.convertGeometricAltitude = function (toUnit) {
            return this.convertMixedAltitude(false, toUnit);
        };
        Aircraft.prototype.convertMixedAltitude = function (usePressureAltitude, toUnit) {
            var result = usePressureAltitude ? this.altitude.val : this.geometricAltitude.val;
            if (result !== undefined && toUnit !== VRS.Height.Feet) {
                result = VRS.unitConverter.convertHeight(result, VRS.Height.Feet, toUnit);
            }
            return result;
        };
        Aircraft.prototype.convertAirPressure = function (toUnit) {
            var result = this.airPressureInHg.val;
            if (result !== undefined && toUnit !== VRS.Pressure.InHg) {
                result = VRS.unitConverter.convertPressure(result, VRS.Pressure.InHg, toUnit);
            }
            return result;
        };
        Aircraft.prototype.convertDistanceFromHere = function (toUnit) {
            var result = this.distanceFromHereKm.val;
            if (result !== undefined && toUnit !== VRS.Distance.Kilometre) {
                result = VRS.unitConverter.convertDistance(result, VRS.Distance.Kilometre, toUnit);
            }
            return result;
        };
        Aircraft.prototype.convertVerticalSpeed = function (toUnit, perSecond) {
            return VRS.unitConverter.convertVerticalSpeed(this.verticalSpeed.val, VRS.Height.Feet, toUnit, perSecond);
        };
        Aircraft.prototype.fetchAirportDataThumbnails = function (numThumbnails) {
            if (numThumbnails === void 0) { numThumbnails = 1; }
            if (this.icao.val) {
                var self = this;
                var fetcher = new VRS.AirportDataApi();
                fetcher.getThumbnails(this.icao.val, this.registration.val, numThumbnails, function (icao, thumbnails) {
                    if (icao === self.icao.val)
                        self.airportDataThumbnails.setValue(thumbnails);
                });
            }
        };
        Aircraft.prototype.formatAirportDataThumbnails = function (showLinkToSite) {
            if (showLinkToSite === undefined)
                showLinkToSite = true;
            return VRS.format.airportDataThumbnails(this.airportDataThumbnails.val, showLinkToSite);
        };
        Aircraft.prototype.formatAltitude = function (heightUnit, distinguishOnGround, showUnits, showType) {
            return VRS.format.altitude(this.altitude.val, VRS.AltitudeType.Barometric, this.isOnGround.val, heightUnit, distinguishOnGround, showUnits, showType);
        };
        Aircraft.prototype.formatGeometricAltitude = function (heightUnit, distinguishOnGround, showUnits, showType) {
            return VRS.format.altitude(this.geometricAltitude.val, VRS.AltitudeType.Geometric, this.isOnGround.val, heightUnit, distinguishOnGround, showUnits, showType);
        };
        Aircraft.prototype.formatMixedAltitude = function (usePressureAltitude, heightUnit, distinguishOnGround, showUnits, showType) {
            var value = usePressureAltitude ? this.altitude.val : this.geometricAltitude.val;
            var valueType = usePressureAltitude ? VRS.AltitudeType.Barometric : VRS.AltitudeType.Geometric;
            return VRS.format.altitude(value, valueType, this.isOnGround.val, heightUnit, distinguishOnGround, showUnits, showType);
        };
        Aircraft.prototype.formatAltitudeType = function () {
            return VRS.format.altitudeType(this.altitudeType.val);
        };
        Aircraft.prototype.formatAirPressureInHg = function (pressureUnit, showUnits) {
            return VRS.format.pressure(this.airPressureInHg.val, pressureUnit, showUnits);
        };
        Aircraft.prototype.formatAverageSignalLevel = function () {
            return VRS.format.averageSignalLevel(this.averageSignalLevel.val);
        };
        Aircraft.prototype.formatBearingFromHere = function (showUnits) {
            return VRS.format.bearingFromHere(this.bearingFromHere.val, showUnits);
        };
        Aircraft.prototype.formatBearingFromHereImage = function () {
            return VRS.format.bearingFromHereImage(this.bearingFromHere.val);
        };
        Aircraft.prototype.formatCallsign = function (showUncertainty) {
            return VRS.format.callsign(this.callsign.val, this.callsignSuspect.val, showUncertainty);
        };
        Aircraft.prototype.formatCountFlights = function (format) {
            return VRS.format.countFlights(this.countFlights.val, format);
        };
        Aircraft.prototype.formatCountMessages = function (format) {
            return VRS.format.countMessages(this.countMessages.val, format);
        };
        Aircraft.prototype.formatCountry = function () {
            return VRS.format.country(this.country.val);
        };
        Aircraft.prototype.formatDistanceFromHere = function (distanceUnit, showUnits) {
            return VRS.format.distanceFromHere(this.distanceFromHereKm.val, distanceUnit, showUnits);
        };
        Aircraft.prototype.formatEngines = function () {
            return VRS.format.engines(this.countEngines.val, this.engineType.val);
        };
        Aircraft.prototype.formatFlightLevel = function (transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, showType) {
            return VRS.format.flightLevel(this.altitude.val, this.geometricAltitude.val, this.altitudeType.val, this.isOnGround.val, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, showType);
        };
        Aircraft.prototype.formatHeading = function (showUnit, showType) {
            return VRS.format.heading(this.heading.val, this.headingIsTrue.val, showUnit, showType);
        };
        Aircraft.prototype.formatHeadingType = function () {
            return VRS.format.headingType(this.headingIsTrue.val);
        };
        Aircraft.prototype.formatIcao = function () {
            return VRS.format.icao(this.icao.val);
        };
        Aircraft.prototype.formatIsMilitary = function () {
            return VRS.format.isMilitary(this.isMilitary.val);
        };
        Aircraft.prototype.formatIsMlat = function () {
            return VRS.format.isMlat(this.isMlat.val);
        };
        Aircraft.prototype.formatIsTisb = function () {
            return VRS.format.isTisb(this.isTisb.val);
        };
        Aircraft.prototype.formatLatitude = function (showUnit) {
            return VRS.format.latitude(this.latitude.val, showUnit);
        };
        Aircraft.prototype.formatLongitude = function (showUnit) {
            return VRS.format.longitude(this.longitude.val, showUnit);
        };
        Aircraft.prototype.formatManufacturer = function () {
            return VRS.format.manufacturer(this.manufacturer.val);
        };
        Aircraft.prototype.formatModel = function () {
            return VRS.format.model(this.model.val);
        };
        Aircraft.prototype.formatModelIcao = function () {
            return VRS.format.modelIcao(this.modelIcao.val);
        };
        Aircraft.prototype.formatModelIcaoImageHtml = function () {
            return VRS.format.modelIcaoImageHtml(this.modelIcao.val, this.icao.val, this.registration.val);
        };
        Aircraft.prototype.formatModelIcaoNameAndDetail = function () {
            return VRS.format.modelIcaoNameAndDetail(this.modelIcao.val, this.model.val, this.countEngines.val, this.engineType.val, this.species.val, this.wakeTurbulenceCat.val);
        };
        Aircraft.prototype.formatOperator = function () {
            return VRS.format.operator(this.operator.val);
        };
        Aircraft.prototype.formatOperatorIcao = function () {
            return VRS.format.operatorIcao(this.operatorIcao.val);
        };
        Aircraft.prototype.formatOperatorIcaoAndName = function () {
            return VRS.format.operatorIcaoAndName(this.operator.val, this.operatorIcao.val);
        };
        Aircraft.prototype.formatOperatorIcaoImageHtml = function () {
            return VRS.format.operatorIcaoImageHtml(this.operator.val, this.operatorIcao.val, this.icao.val, this.registration.val);
        };
        Aircraft.prototype.formatPictureHtml = function (requestSize, allowResizeUp, linkToOriginal, blankSize) {
            return VRS.format.pictureHtml(this.registration.val, this.icao.val, this.pictureWidth.val, this.pictureHeight.val, requestSize, allowResizeUp, linkToOriginal, blankSize);
        };
        Aircraft.prototype.formatReceiver = function () {
            return VRS.format.receiver(this.receiverId.val, this._AircraftListFetcher);
        };
        Aircraft.prototype.formatRegistration = function (onlyAlphaNumeric) {
            return VRS.format.registration(this.registration.val, onlyAlphaNumeric);
        };
        Aircraft.prototype.formatRouteFull = function () {
            return VRS.format.routeFull(this.callsign.val, this.from.val, this.to.val, this.getViaAirports());
        };
        Aircraft.prototype.formatRouteMultiLine = function () {
            return VRS.format.routeMultiLine(this.callsign.val, this.from.val, this.to.val, this.getViaAirports());
        };
        Aircraft.prototype.formatRouteShort = function (abbreviateStopovers, showRouteNotKnown) {
            return VRS.format.routeShort(this.callsign.val, this.from.val, this.to.val, this.getViaAirports(), abbreviateStopovers, showRouteNotKnown);
        };
        Aircraft.prototype.formatSecondsTracked = function () {
            return VRS.format.secondsTracked(this.secondsTracked);
        };
        Aircraft.prototype.formatSignalLevel = function () {
            return VRS.format.signalLevel(this.signalLevel.val);
        };
        Aircraft.prototype.formatSpecies = function (ignoreNone) {
            return VRS.format.species(this.species.val, ignoreNone);
        };
        Aircraft.prototype.formatSpeed = function (speedUnit, showUnit, showType) {
            return VRS.format.speed(this.speed.val, this.speedType.val, speedUnit, showUnit, showType);
        };
        Aircraft.prototype.formatSpeedType = function () {
            return VRS.format.speedType(this.speedType.val);
        };
        Aircraft.prototype.formatSquawk = function () {
            return VRS.format.squawk(this.squawk.val);
        };
        Aircraft.prototype.formatSquawkDescription = function () {
            return VRS.format.squawkDescription(this.squawk.val);
        };
        Aircraft.prototype.formatTargetAltitude = function (heightUnit, showUnits, showType) {
            return VRS.format.altitude(this.targetAltitude.val, VRS.AltitudeType.Barometric, false, heightUnit, false, showUnits, showType);
        };
        Aircraft.prototype.formatTargetHeading = function (showUnits, showType) {
            return VRS.format.heading(this.targetHeading.val, false, showUnits, showType);
        };
        Aircraft.prototype.formatTransponderType = function () {
            return VRS.format.transponderType(this.transponderType.val);
        };
        Aircraft.prototype.formatTransponderTypeImageHtml = function () {
            return VRS.format.transponderTypeImageHtml(this.transponderType.val);
        };
        Aircraft.prototype.formatUserInterested = function () {
            return VRS.format.userInterested(this.userInterested.val);
        };
        Aircraft.prototype.formatUserTag = function () {
            return VRS.format.userTag(this.userTag.val);
        };
        Aircraft.prototype.formatVerticalSpeed = function (heightUnit, perSecond, showUnit, showType) {
            return VRS.format.verticalSpeed(this.verticalSpeed.val, this.isOnGround.val, this.verticalSpeedType.val, heightUnit, perSecond, showUnit, showType);
        };
        Aircraft.prototype.formatVerticalSpeedType = function () {
            return VRS.format.verticalSpeedType(this.verticalSpeedType.val);
        };
        Aircraft.prototype.formatWakeTurbulenceCat = function (ignoreNone, expandedDescription) {
            return VRS.format.wakeTurbulenceCat(this.wakeTurbulenceCat.val, ignoreNone, expandedDescription);
        };
        Aircraft.prototype.formatYearBuilt = function () {
            return VRS.format.yearBuilt(this.yearBuilt.val);
        };
        return Aircraft;
    }());
    VRS.Aircraft = Aircraft;
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraft.js.map