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
 * @fileoverview Describes an aircraft that is being tracked.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.suppressTrails = VRS.globalOptions.suppressTrails || false;       // If true then position history is not stored, significantly reducing memory footprint for each aircraft but prevents trails from being shown for the aircraft
    VRS.globalOptions.aircraftHideUncertainCallsigns = VRS.globalOptions.aircraftHideUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftHideUncertainCallsigns : false;   // True if callsigns that we're not 100% sure about are to be hidden from view.
    VRS.globalOptions.aircraftMaxAvgSignalLevelHistory = VRS.globalOptions.aircraftMaxAvgSignalLevelHistory !== undefined ? VRS.globalOptions.aircraftMaxAvgSignalLevelHistory : 6; // The number of signal levels to average out to determine average signal level. Don't make this more than about 10.

    /**
     * An object that carries an abstract value and a flag indicating whether it has changed or not.
     */
    export class Value<T>
    {
        val: T;
        chg: boolean = false;

        constructor(value?: T)
        {
            this.val = value;
        }

        /**
         * Forces a value onto the object and always sets the chg flag. This is used more by reports when creating fake
         * aircraft to ensure that all values set up for a fake aircraft will trigger a render in the objects that use
         * them.
         */
        setValue(value: T)
        {
            this.val = value;
            this.chg = true;
        }
    }

    /**
     * A string value held by an aircraft and whether it changed in the last update.
     */
    export class StringValue extends Value<string>
    {
        constructor(value?: string)
        {
            super(value);
        }
    }

    /**
     * A boolean value held by an aircraft and whether it changed in the last update.
     */
    export class BoolValue extends Value<boolean>
    {
        constructor(value?: boolean)
        {
            super(value);
        }
    }

    /**
     * A number value held by an aircraft and whether it changed in the last update.
     */
    export class NumberValue extends Value<number>
    {
        constructor(value?: number)
        {
            super(value);
        }
    }

    /**
     * Describes an array of values held by an aircraft and whether it changed in the last update.
     */
    export class ArrayValue<T>
    {
        /**
         * The array of values held by the aircraft.
         */
        arr: T[] = [];

        /**
         * True if the array changed in the last update refresh.
         */
        chg: boolean = false;

        /**
         * The index of the values added by the last update. Only meaningful if chg is true.
         */
        chgIdx: number = -1;

        /**
         * The number of array elements trimmed from the start of the array in the last update.
         */
        trimStartCount: number = 0;

        constructor(initialArray?: T[])
        {
            this.arr = initialArray || [];
        }

        /**
         * Sets the array and configures the other fields to indicate that the entire array is new. Used when setting
         * up fake aircraft for reports etc.
         */
        setValue(value: T[])
        {
            if(value && value.length) {
                this.arr = value;
                this.chg = true;
                this.chgIdx = 0;
                this.trimStartCount = 0;
            }
        }

        /**
         * Sets the fields to indicate that nothing changed in the last update.
         */
        setNoChange()
        {
            this.chg = false;
            this.chgIdx = -1;
            this.trimStartCount = 0;
        }

        /**
         * Sets the fields to indicate that the array was reset back to an empty state in the last update.
         */
        resetArray()
        {
            if(this.arr.length > 0) {
                this.trimStartCount = this.arr.length;
                this.arr = [];
                this.chg = true;
                this.chgIdx = -1;
            }
        }

        /**
         * Trims elements from the start of the array and sets the fields to indicate that this has happened.
         */
        trimStart(trimCount: number)
        {
            if(trimCount > 0) {
                if(trimCount > this.arr.length) trimCount = this.arr.length;
                this.arr.splice(0, trimCount);
                this.trimStartCount = trimCount;
                this.chg = true;
            }
        }
    }

    /**
     * Describes a route held by an aircraft and whether it changed in the last update.
     * @param {string} [value] An airport code followed by the description of the airport.
     * @constructor
     * @augments VRS.Value
     */
    export class RouteValue extends Value<string>
    {
        private _AirportCodeDerivedFromVal: string;
        private _AirportCode: string;

        constructor(value?: string)
        {
            super(value);
        }

        /**
         * Returns the airport code from the route value.
         */
        getAirportCode() : string
        {
            if(this._AirportCodeDerivedFromVal !== this.val) {
                this._AirportCodeDerivedFromVal = this.val;
                this._AirportCode = VRS.format.routeAirportCode(this._AirportCodeDerivedFromVal);
            }
            return this._AirportCode;
        }
    }

    /**
     * Describes a set of thumbnails that have been fetched for the aircraft from www.airport-data.com.
     */
    export class AirportDataThumbnailValue extends Value<IAirportDataThumbnails>
    {
        private _LastResetChgValue: boolean = false;

        constructor(value?: IAirportDataThumbnails)
        {
            super(value);
        }

        /**
         * Called on every refresh. Thumbnails are set asynchronously, they are set when some code calls a fetch method
         * for them on VRS.Aircraft. Once the response comes back from the server chg is set to true. We want that chg
         * value to remain true for the next refresh of the aircraft list and then for all future refreshes it should
         * be false unless the ICAO changes. This method is called on every refresh - if the chg value has changed from
         * the previous refresh then it is left alone, otherwise it is set to false.
         */
        resetChg()
        {
            if(this.chg && this._LastResetChgValue) this.chg = false;
            this._LastResetChgValue = this.chg;
        }
    }

    /**
     * Describes a coordinate held by an aircraft in its short trail. These have no Chg field, the array holding these
     * values records the index of any new entries.
     */
    export class ShortTrailValue
    {
        /**
         * The latitude of the aircraft.
         */
        lat: number;

        /**
         * The longitude of the aircraft.
         */
        lng: number;

        /**
         * The time at which the aircraft was observed at the coordinate, in ticks.
         */
        posnTick: number;

        /**
         * The altitude, if any, that the aircraft had reached at this point.
         */
        altitude: number;

        /**
         * The speed, if any, that the aircraft had reached at this point.
         */
        speed: number;

        constructor(latitude: number, longitude: number, posnTick: number, altitude: number, speed: number)
        {
            this.lat = latitude;
            this.lng = longitude;
            this.posnTick = posnTick;
            this.altitude = altitude;
            this.speed = speed;
        }
    }

    /**
     * Describes a coordinate held by an aircraft in its full trail.
     */
    export class FullTrailValue
    {
        /**
         * The latitude of the aircraft.
         */
        lat: number;

        /**
         * The longitude of the aircraft.
         */
        lng: number;

        /**
         * The heading that the aircraft was pointing in.
         */
        heading: number;

        /**
         * The altitude, if any, that the aircraft had reached at this point.
         */
        altitude: number;

        /**
         * The speed, if any, that the aircraft had reached at this point.
         */
        speed: number;

        /**
         * True when this is the last element in the array. It is set to true if the aircraft changed position but its
         * heading was the same as it was in the previous update, in which case the last element in the trail is set to
         * the new position and the polyline describing the trail should just be extended to this new location.
         */
        chg: boolean;

        constructor(latitude: number, longitude: number, heading: number, altitude?: number, speed?: number)
        {
            this.lat = latitude;
            this.lng = longitude;
            this.heading = heading;
            this.altitude = altitude;
            this.speed = speed;
            this.chg = false;
        }
    }

    /**
     * A type alias for the different kinds of trail arrays.
     */
    export type TrailArray = ArrayValue<ShortTrailValue> | ArrayValue<FullTrailValue>;

    /**
     * The settings that control how Aircraft.ApplyJson applies updates.
     */
    export interface Aircraft_ApplyJsonSettings
    {
        /**
            * The earliest allowable tick in a short trail list. Coordinates for ticks before this value must be
            * removed on update. If -1 then no short trails should be recorded.
            */
        shortTrailTickThreshold:    number;

        /**
            * True if the user can see local pictures.
            */
        picturesEnabled: boolean;
    }

    /**
     * Describes an aircraft tracked by the server.
     */
    export class Aircraft
    {
        /**
         * The aircraft list fetcher that last supplied details for the aircraft. Usually there's just one of these for
         * the lifetime of the site.
         * @type {VRS.AircraftListFetcher}
         * @private
         */
        private _AircraftListFetcher: AircraftListFetcher = null;

        /**
         * A fixed length array of the signal levels for the aircraft, used to determine averageSignalLevel.
         */
        private signalLevelHistory: number[] = [];

        /**
         * The unique identifier of the aircraft, derived from the ICAO.
         */
        id: number = 0;

        /**
         * The number of seconds that the aircraft has been tracked for.
         */
        secondsTracked: number = 0;

        /**
         * The number of times the aircraft details have been updated.
         */
        updateCounter: number = 0;

        // The value fields
        receiverId:             NumberValue =                       new NumberValue();
        icao:                   StringValue =                       new StringValue();
        icaoInvalid:            BoolValue =                         new BoolValue();
        registration:           StringValue =                       new StringValue();
        altitude:               NumberValue =                       new NumberValue();                  // Pressure altitude
        geometricAltitude:      NumberValue =                       new NumberValue();                  // Geometric altitude
        airPressureInHg:        NumberValue =                       new NumberValue();                  // Air pressure in inches of mercury either transmitted by the aircraft or, failing that, from the closest weather station
        altitudeType:           Value<AltitudeTypeEnum> =           new Value<AltitudeTypeEnum>();      // The altitude field that was transmitted by the aircraft
        targetAltitude:         NumberValue =                       new NumberValue();
        callsign:               StringValue =                       new StringValue();
        callsignSuspect:        BoolValue =                         new BoolValue();
        latitude:               NumberValue =                       new NumberValue();
        longitude:              NumberValue =                       new NumberValue();
        isMlat:                 BoolValue =                         new BoolValue();
        positionTime:           NumberValue =                       new NumberValue();
        positionStale:          BoolValue =                         new BoolValue();
        speed:                  NumberValue =                       new NumberValue();
        speedType:              Value<SpeedTypeEnum> =              new Value<SpeedTypeEnum>();
        verticalSpeed:          NumberValue =                       new NumberValue();
        verticalSpeedType:      Value<AltitudeTypeEnum> =           new Value<AltitudeTypeEnum>();
        heading:                NumberValue =                       new NumberValue();          // The track across the ground that the aircraft is following, unless headingIsTrue is true in which case it's the aircraft's true heading (i.e. the direction the nose is pointing in)
        headingIsTrue:          BoolValue =                         new BoolValue();            // True if heading is the aircraft's true heading, false if it's the ground track.
        targetHeading:          NumberValue =                       new NumberValue();
        manufacturer:           StringValue =                       new StringValue();
        serial:                 StringValue =                       new StringValue();
        yearBuilt:              StringValue =                       new StringValue();
        model:                  StringValue =                       new StringValue();
        modelIcao:              StringValue =                       new StringValue();
        from:                   RouteValue =                        new RouteValue();
        to:                     RouteValue =                        new RouteValue();
        via:                    ArrayValue<RouteValue> =            new ArrayValue<RouteValue>();
        operator:               StringValue =                       new StringValue();
        operatorIcao:           StringValue =                       new StringValue();
        squawk:                 StringValue =                       new StringValue();
        isEmergency:            BoolValue =                         new BoolValue();
        distanceFromHereKm:     NumberValue =                       new NumberValue();
        bearingFromHere:        NumberValue =                       new NumberValue();          // The bearing from the browser's location to the aircraft, assuming that the browser is pointing due north
        wakeTurbulenceCat:      Value<WakeTurbulenceCategoryEnum> = new Value<WakeTurbulenceCategoryEnum>();
        countEngines:           StringValue =                       new StringValue();
        engineType:             Value<EngineTypeEnum> =             new Value<EngineTypeEnum>();
        enginePlacement:        NumberValue =                       new NumberValue();
        species:                Value<SpeciesEnum> =                new Value<SpeciesEnum>();
        isMilitary:             BoolValue =                         new BoolValue();
        isTisb:                 BoolValue =                         new BoolValue();
        country:                StringValue =                       new StringValue();
        hasPicture:             BoolValue =                         new BoolValue();
        pictureWidth:           NumberValue =                       new NumberValue();
        pictureHeight:          NumberValue =                       new NumberValue();
        countFlights:           NumberValue =                       new NumberValue();
        countMessages:          NumberValue =                       new NumberValue();
        isOnGround:             BoolValue =                         new BoolValue();
        userNotes:              StringValue =                       new StringValue();
        userTag:                StringValue =                       new StringValue();
        userInterested:         BoolValue =                         new BoolValue();
        signalLevel:            NumberValue =                       new NumberValue();
        averageSignalLevel:     NumberValue =                       new NumberValue();
        airportDataThumbnails:  AirportDataThumbnailValue =         new AirportDataThumbnailValue();
        transponderType:        Value<TransponderTypeEnum> =        new Value<TransponderTypeEnum>();
        shortTrail:             ArrayValue<ShortTrailValue> =       new ArrayValue<ShortTrailValue>();
        fullTrail:              ArrayValue<FullTrailValue> =        new ArrayValue<FullTrailValue>();


        /**
         * Applies details about an individual aircraft's current state to the aircraft object.
         * In general most of the fields are optional. If they are missing then the value has not
         * changed. We need to track values that changed so that we only update the UI for those
         * and not for every value on every refresh. At best that can cause flicker, at worst it
         * can hammer the browser.
         */
        applyJson(aircraftJson: IAircraftListAircraft, aircraftListFetcher: AircraftListFetcher, settings: Aircraft_ApplyJsonSettings)
        {
            this.id = aircraftJson.Id;
            this.secondsTracked = aircraftJson.TSecs;
            this._AircraftListFetcher = aircraftListFetcher;
            ++this.updateCounter;

            this.setValue(this.receiverId,           aircraftJson.Rcvr);
            this.setValue(this.icao,                 aircraftJson.Icao);
            this.setValue(this.icaoInvalid,          aircraftJson.Bad);
            this.setValue(this.registration,         aircraftJson.Reg);
            this.setValue(this.altitude,             aircraftJson.Alt);
            this.setValue(this.geometricAltitude,    aircraftJson.GAlt);
            this.setValue(this.airPressureInHg,      aircraftJson.InHg);
            this.setValue(this.altitudeType,         aircraftJson.AltT);
            this.setValue(this.targetAltitude,       aircraftJson.TAlt);
            this.setValue(this.callsign,             aircraftJson.Call);
            this.setValue(this.callsignSuspect,      aircraftJson.CallSus);
            this.setValue(this.latitude,             aircraftJson.Lat);
            this.setValue(this.longitude,            aircraftJson.Long);
            this.setValue(this.isMlat,               aircraftJson.Mlat);
            this.setValue(this.positionTime,         aircraftJson.PosTime);
            this.setValue(this.positionStale,        !!aircraftJson.PosStale, true);
            this.setValue(this.speed,                aircraftJson.Spd);
            this.setValue(this.speedType,            aircraftJson.SpdTyp);
            this.setValue(this.verticalSpeed,        aircraftJson.Vsi);
            this.setValue(this.verticalSpeedType,    aircraftJson.VsiT);
            this.setValue(this.heading,              aircraftJson.Trak);
            this.setValue(this.headingIsTrue,        aircraftJson.TrkH);
            this.setValue(this.targetHeading,        aircraftJson.TTrk);
            this.setValue(this.manufacturer,         aircraftJson.Man);
            this.setValue(this.serial,               aircraftJson.CNum);
            this.setValue(this.yearBuilt,            aircraftJson.Year);
            this.setValue(this.model,                aircraftJson.Mdl);
            this.setValue(this.modelIcao,            aircraftJson.Type);
            this.setValue(this.from,                 aircraftJson.From);
            this.setValue(this.to,                   aircraftJson.To);
            this.setValue(this.operator,             aircraftJson.Op);
            this.setValue(this.operatorIcao,         aircraftJson.OpIcao);
            this.setValue(this.squawk,               aircraftJson.Sqk);
            this.setValue(this.isEmergency,          aircraftJson.Help);
            this.setValue(this.distanceFromHereKm,   aircraftJson.Dst, true);
            this.setValue(this.bearingFromHere,      aircraftJson.Brng, true);
            this.setValue(this.wakeTurbulenceCat,    aircraftJson.WTC);
            this.setValue(this.countEngines,         aircraftJson.Engines);
            this.setValue(this.engineType,           aircraftJson.EngType);
            this.setValue(this.enginePlacement,      aircraftJson.EngMount);
            this.setValue(this.species,              aircraftJson.Species);
            this.setValue(this.isMilitary,           aircraftJson.Mil);
            this.setValue(this.isTisb,               aircraftJson.Tisb);
            this.setValue(this.country,              aircraftJson.Cou);
            this.setValue(this.hasPicture,           aircraftJson.HasPic && settings.picturesEnabled);
            this.setValue(this.pictureWidth,         aircraftJson.PicX);
            this.setValue(this.pictureHeight,        aircraftJson.PicY);
            this.setValue(this.countFlights,         aircraftJson.FlightsCount);
            this.setValue(this.countMessages,        aircraftJson.CMsgs);
            this.setValue(this.isOnGround,           aircraftJson.Gnd);
            this.setValue(this.userTag,              aircraftJson.Tag);
            this.setValue(this.userNotes,            aircraftJson.Notes);
            this.setValue(this.userInterested,       aircraftJson.Interested);
            this.setValue(this.transponderType,      aircraftJson.Trt);
            this.setRouteArray(this.via,             aircraftJson.Stops);

            if(aircraftJson.HasSig !== undefined) {
                this.setValue(this.signalLevel, aircraftJson.Sig);
            }
            this.recordSignalLevelHistory(this.signalLevel.val);

            if(!VRS.globalOptions.suppressTrails) {
                this.setShortTrailArray(this.shortTrail, aircraftJson.Cos, aircraftJson.ResetTrail, settings.shortTrailTickThreshold, aircraftJson.TT);
                this.setFullTrailArray(this.fullTrail, aircraftJson.Cot, aircraftJson.ResetTrail, aircraftJson.TT)
            }

            if(VRS.globalOptions.aircraftHideUncertainCallsigns && this.callsignSuspect.val) {
                this.callsign.val = undefined;
                this.callsign.chg = false;
            }

            this.airportDataThumbnails.resetChg();
        }

        /**
         * Assigns content to a value object and sets / clears the chg flag.
         */
        private setValue<T>(field: Value<T>, jsonValue: T, alwaysSent?: boolean)
        {
            if(jsonValue === undefined) field.chg = false;
            else if(!alwaysSent) {
                field.val = jsonValue;
                field.chg = true;
            } else {
                field.chg = field.val !== jsonValue;
                if(field.chg) field.val = jsonValue;
            }
        }

        /**
         * Assigns content to an array object.
         */
        private setRouteArray<T>(field: ArrayValue<RouteValue>, jsonArray: string[])
        {
            field.setNoChange();
            if(jsonArray) {
                field.arr = [];
                field.chg = true;
                field.chgIdx = 0;
                $.each(jsonArray, function(idx, val) {
                    field.arr.push(new VRS.RouteValue(val));
                });
            }
        }

        /**
         * Updates the short trail array.
         */
        private setShortTrailArray(field: ArrayValue<ShortTrailValue>, jsonArray: number[], resetTrail: boolean, shortTrailTickThreshold: number, trailType: string)
        {
            field.setNoChange();
            var length = field.arr.length;
            var i = 0;

            // Clean up expired coordinates first - this must happen even if no new points have been supplied
            if(length > 0) {
                if(resetTrail) field.resetArray();
                else {
                    if(shortTrailTickThreshold === -1) field.resetArray();
                    else {
                        var indexFirstValidCoordinate = -1;
                        for(i = 0;i < length;++i) {
                            if(field.arr[i].posnTick >= shortTrailTickThreshold) {
                                indexFirstValidCoordinate = i;
                                break;
                            }
                        }
                        if(indexFirstValidCoordinate !== 0) {
                            if(indexFirstValidCoordinate === -1) field.resetArray();
                            else field.trimStart(indexFirstValidCoordinate);
                        }
                    }
                }

                length = field.arr.length;
            }

            // Add the new coordinates to the end of the trail
            var addLength = jsonArray ? jsonArray.length : 0;
            if(addLength > 0) {
                field.chg = true;
                field.chgIdx = length;
                for(i = 0;i < addLength;) {
                    var lat = jsonArray[i++];
                    var lng = jsonArray[i++];
                    var tik = jsonArray[i++];
                    var alt = trailType === 'a' ? jsonArray[i++] : undefined;
                    var spd = trailType === 's' ? jsonArray[i++] : undefined;
                    field.arr.push(new VRS.ShortTrailValue(lat, lng, tik, alt, spd));
                }
            }
        }

        /**
         * Manages updates to the full trail for the aircraft.
         */
        private setFullTrailArray(field: ArrayValue<FullTrailValue>, jsonArray: number[], resetTrail: boolean, trailType: string)
        {
            field.setNoChange();
            var length = field.arr.length;
            var lastTrail = length ? field.arr[length - 1] : null;
            var lastButOne = length > 1 ? field.arr[length - 2] : null;
            if(lastTrail) lastTrail.chg = false;

            if(resetTrail) {
                field.resetArray();
                length = field.arr.length;
            }

            var addLength = jsonArray ? jsonArray.length : 0;
            if(addLength > 0) {
                field.chg = true;

                var i;
                field.chgIdx = length;
                for(i = 0;i < addLength;) {
                    var lat = jsonArray[i++];
                    var lng = jsonArray[i++];
                    var hdg = jsonArray[i++];
                    var alt = trailType === 'a' ? jsonArray[i++] : undefined;
                    var spd = trailType === 's' ? jsonArray[i++] : undefined;

                    if(hdg && lastTrail && lastButOne &&
                       lastTrail.heading === hdg && lastButOne.heading === hdg &&
                       lastTrail.altitude === alt && lastButOne.altitude === alt &&
                       lastTrail.speed === spd && lastButOne.speed === spd) {
                        if(lastTrail.lat !== lat || lastTrail.lng !== lng) {
                            lastTrail.lat = lat;
                            lastTrail.lng = lng;
                            lastTrail.chg = true;
                        }
                    } else {
                        lastButOne = lastTrail;
                        lastTrail = new VRS.FullTrailValue(lat, lng, hdg, alt, spd);
                        field.arr.push(lastTrail);
                        ++length;
                    }
                }

                // Were they all just updates to the last trail entry?
                if(field.chgIdx === length) {
                    field.chgIdx = -1;
                } else {
                    // Nope, we had to add at least one new point. We need to reset the chg flag and, if we had changed
                    // what had been the latest trail point, then we need to indicate that it's a new point so that it
                    // all joins up correctly.
                    var oldLastIndex = field.chgIdx - 1;
                    for(i = Math.max(0, field.chgIdx - 1);i < length;++i) {
                        if(i === oldLastIndex && field.arr[i].chg) --field.chgIdx;
                        field.arr[i].chg = false;
                    }
                }
            }
        }

        /**
         * Records the signal level history.
         */
        private recordSignalLevelHistory(signalLevel: number)
        {
            /** @type {Number} */
            var averageSignalLevel = 0;

            if(VRS.globalOptions.aircraftMaxAvgSignalLevelHistory) {
                if(signalLevel === null) {
                    if(this.signalLevelHistory.length !== 0) this.signalLevelHistory = [];
                } else {
                    var i;
                    averageSignalLevel = signalLevel;
                    var length = this.signalLevelHistory.length;
                    if(length < VRS.globalOptions.aircraftMaxAvgSignalLevelHistory) {
                        for(i = 0;i < length;++i) {
                            averageSignalLevel += this.signalLevelHistory[i];
                        }
                        this.signalLevelHistory.push(signalLevel);
                        ++length;
                    } else {
                        var shiftLength = length - 1;
                        for(i = 0;i < shiftLength;++i) {
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
        }

        /**
         * Returns true if the aircraft has a position associated with it.
         */
        hasPosition() : boolean
        {
            return this.positionTime.val > 0;
        }

        /**
         * Returns the position of the aircraft as an object or null if the aircraft has no position.
         */
        getPosition() : ILatLng
        {
            return this.hasPosition() ? { lat: this.latitude.val, lng: this.longitude.val } : null;
        }

        /**
         * Returns true if the aircraft is within the bounding box described by the parameters.
         */
        positionWithinBounds(bounds: IBounds) : boolean
        {
            var result = this.hasPosition();
            if(result) result = VRS.greatCircle.isLatLngInBounds(this.latitude.val, this.longitude.val, bounds);

            return result;
        }

        /**
         * Returns true if the aircraft has a route, false if it does not.
         */
        hasRoute() : boolean
        {
            return !!this.from.val && !!this.to.val;
        }

        /**
         * Returns true if the aircraft's route has changed.
         */
        hasRouteChanged() : boolean
        {
            return this.from.chg || this.to.chg || this.via.chg;
        }

        /**
         * Returns a copy of the airports from the via routes array.
         */
        getViaAirports() : string[]
        {
            var result: string[] = [];

            var length = this.via.arr.length;
            for(var i = 0;i < length;++i) {
                result.push(this.via.arr[i].val);
            }

            return result;
        }

        /**
         * Returns an array of all of the airport codes in the route.
         */
        getAirportCodes(distinctOnly?: boolean) : string[]
        {
            distinctOnly = !!distinctOnly;
            var result = [];

            var addAirportCode = function(code) {
                if(code && (!distinctOnly || VRS.arrayHelper.indexOf(result, code) === -1)) {
                    result.push(code);
                }
            };

            if(this.from.val) addAirportCode(this.from.getAirportCode());
            var length = this.via.arr.length;
            for(var i = 0;i < length;++i) {
                addAirportCode(this.via.arr[i].getAirportCode());
            }
            if(this.to.val) addAirportCode(this.to.getAirportCode());

            return result;
        }

        /**
         * Returns either the pressure or geometric altitude, depending on the "UsePressureAltitude"
         * switch in unit display preferences.
         */
        getMixedAltitude(usePressureAltitude: boolean) : number
        {
            return usePressureAltitude ? this.altitude.val : this.geometricAltitude.val;
        }

        /**
         * Returns the value changed flag for either the pressure or geometric altitude, depending
         * on the "UsePressureAltitude" switch in unit display preferences.
         * @param usePressureAltitude
         */
        hasMixedAltitudeChanged(usePressureAltitude: boolean) : boolean
        {
            return usePressureAltitude ? this.altitude.chg : this.geometricAltitude.chg;
        }

        /**
         * Returns true if the aircraft is some kind of aircraft, false if it is a ground vehicle or radio mast.
         */
        isAircraftSpecies() : boolean
        {
            return this.species.val !== VRS.Species.GroundVehicle && this.species.val !== VRS.Species.Tower;
        }

        /**
         * Returns the speed converted from knots (as sent by the server) to the unit passed across.
         */
        convertSpeed(toUnit: SpeedEnum) : number
        {
            var result = this.speed.val;
            if(result !== undefined && toUnit !== VRS.Speed.Knots) {
                result = VRS.unitConverter.convertSpeed(result, VRS.Speed.Knots, toUnit);
            }
            return result;
        }

        /**
         * Returns the pressure altitude converted from feet (as sent by the server) to the unit passed across.
         */
        convertAltitude(toUnit: HeightEnum) : number
        {
            return this.convertMixedAltitude(true, toUnit);
        }

        /**
         * Returns the geometric altitude converted from feet to the unit passed across.
         */
        convertGeometricAltitude(toUnit: HeightEnum) : number
        {
            return this.convertMixedAltitude(false, toUnit);
        }

        /**
         * Returns either the pressure or geometric altitude converted from feet to the unit passed across.
         */
        convertMixedAltitude(usePressureAltitude: boolean, toUnit: HeightEnum) : number
        {
            var result = usePressureAltitude ? this.altitude.val : this.geometricAltitude.val;
            if(result !== undefined && toUnit !== VRS.Height.Feet) {
                result = VRS.unitConverter.convertHeight(result, VRS.Height.Feet, toUnit);
            }
            return result;
        }

        /**
         * Returns the air pressure converted from inches of mercury to the unit passed across.
         */
        convertAirPressure(toUnit: PressureEnum) : number
        {
            var result = this.airPressureInHg.val;
            if(result !== undefined && toUnit !== VRS.Pressure.InHg) {
                result = VRS.unitConverter.convertPressure(result, VRS.Pressure.InHg, toUnit);
            }
            return result;
        }

        /**
         * Returns the distance from here converted from kilometres (as sent by the server) to the unit passed across.
         */
        convertDistanceFromHere(toUnit: DistanceEnum) : number
        {
            var result = this.distanceFromHereKm.val;
            if(result !== undefined && toUnit !== VRS.Distance.Kilometre) {
                result = VRS.unitConverter.convertDistance(result, VRS.Distance.Kilometre, toUnit);
            }
            return result;
        }

        /**
         * Returns the vertical speed converted from feet per minute to the unit passed across.
         */
        convertVerticalSpeed(toUnit: HeightEnum, perSecond: boolean) : number
        {
            return VRS.unitConverter.convertVerticalSpeed(this.verticalSpeed.val, VRS.Height.Feet, toUnit, perSecond);
        }

        /**
         * Returns true if the SDM site allows lookup details for the aircraft.
         */
        canSubmitAircraftLookup() : boolean
        {
            return !this.icaoInvalid.val;
        }

        /**
         * Fetches thumbnails for the aircraft from www.airport-data.com. Once the fetch has been satisfied the chg
         * value for airportDataThumbnail will be set for the next refresh of aircraft data, and then cleared on the
         * subsequent refresh.
         */
        fetchAirportDataThumbnails(numThumbnails: number = 1)
        {
            if(this.icao.val) {
                var self = this;
                var fetcher = new AirportDataApi();
                fetcher.getThumbnails(this.icao.val, this.registration.val, numThumbnails, function(icao, thumbnails) {
                    if(icao === self.icao.val) self.airportDataThumbnails.setValue(thumbnails);
                });
            }
        }

        /**
         * Formats the airport-data.com thumbnails as an IMG tag HTML.
         */
        formatAirportDataThumbnails(showLinkToSite?: boolean) : string
        {
            if(showLinkToSite === undefined) showLinkToSite = true;
            return VRS.format.airportDataThumbnails(this.airportDataThumbnails.val, showLinkToSite);
        }

        /**
         * Formats the pressure altitude as a string.
         */
        formatAltitude(heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean) : string
        {
            return VRS.format.altitude(this.altitude.val, VRS.AltitudeType.Barometric, this.isOnGround.val, heightUnit, distinguishOnGround, showUnits, showType);
        }

        /**
         * Formats the geometric altitude as a string.
         */
        formatGeometricAltitude(heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean) : string
        {
            return VRS.format.altitude(this.geometricAltitude.val, VRS.AltitudeType.Geometric, this.isOnGround.val, heightUnit, distinguishOnGround, showUnits, showType);
        }

        /**
         * Formats either the pressure or geometric altitude as a string.
         */
        formatMixedAltitude(usePressureAltitude: boolean, heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean) : string
        {
            var value = usePressureAltitude ? this.altitude.val : this.geometricAltitude.val;
            var valueType = usePressureAltitude ? VRS.AltitudeType.Barometric : VRS.AltitudeType.Geometric;
            return VRS.format.altitude(value, valueType, this.isOnGround.val, heightUnit, distinguishOnGround, showUnits, showType);
        }

        /**
         * Formats the altitude type as a string.
         */
        formatAltitudeType() : string
        {
            return VRS.format.altitudeType(this.altitudeType.val);
        }

        /**
         * Formats the air pressure as a string.
         */
        formatAirPressureInHg(pressureUnit: PressureEnum, showUnits: boolean) : string
        {
            return VRS.format.pressure(this.airPressureInHg.val, pressureUnit, showUnits);
        }

        /**
         * Formats the average signal level as a string.
         */
        formatAverageSignalLevel() : string
        {
            return VRS.format.averageSignalLevel(this.averageSignalLevel.val);
        }

        /**
         * Formats the bearing from here as a string.
         */
        formatBearingFromHere(showUnits: boolean) : string
        {
            return VRS.format.bearingFromHere(this.bearingFromHere.val, showUnits);
        }

        /**
         * Formats the bearing from here as an HTML IMG tag.
         */
        formatBearingFromHereImage() : string
        {
            return VRS.format.bearingFromHereImage(this.bearingFromHere.val);
        }

        /**
         * Formats the callsign as a string.
         */
        formatCallsign(showUncertainty: boolean) : string
        {
            return VRS.format.callsign(this.callsign.val, this.callsignSuspect.val, showUncertainty);
        }

        /**
         * Formats the count of flights as a string.
         */
        formatCountFlights(format?: string) : string
        {
            return VRS.format.countFlights(this.countFlights.val, format);
        }

        /**
         * Formats the count of messages as a string.
         */
        formatCountMessages(format?: string) : string
        {
            return VRS.format.countMessages(this.countMessages.val, format);
        }

        /**
         * Formats the country as a string.
         */
        formatCountry() : string
        {
            return VRS.format.country(this.country.val);
        }

        /**
         * Formats the distance from here as a string.
         */
        formatDistanceFromHere(distanceUnit: DistanceEnum, showUnits: boolean) : string
        {
            return VRS.format.distanceFromHere(this.distanceFromHereKm.val, distanceUnit, showUnits);
        }

        /**
         * Formats the count of engines and engine type as a string.
         */
        formatEngines() : string
        {
            return VRS.format.engines(this.countEngines.val, this.engineType.val);
        }

        /**
         * Formats the flight level as a string.
         */
        formatFlightLevel(transitionAltitude: number, transitionAltitudeUnit: HeightEnum, flightLevelAltitudeUnit: HeightEnum, altitudeUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean) : string
        {
            return VRS.format.flightLevel(this.altitude.val, this.geometricAltitude.val, this.altitudeType.val, this.isOnGround.val, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, showType);
        }

        /**
         * Formats the aircraft's heading as a string.
         */
        formatHeading(showUnit: boolean, showType: boolean) : string
        {
            return VRS.format.heading(this.heading.val, this.headingIsTrue.val, showUnit, showType);
        }

        /**
         * Formats the aircraft's heading type as a string.
         */
        formatHeadingType() : string
        {
            return VRS.format.headingType(this.headingIsTrue.val);
        }

        /**
         * Formats the aircraft's ICAO as a string.
         */
        formatIcao() : string
        {
            return VRS.format.icao(this.icao.val);
        }

        /**
         * Formats the aircraft's military status as a string.
         */
        formatIsMilitary() : string
        {
            return VRS.format.isMilitary(this.isMilitary.val);
        }

        /**
         * Formats the aircraft's is MLAT value as a string.
         */
        formatIsMlat() : string
        {
            return VRS.format.isMlat(this.isMlat.val);
        }

        /**
         * Formats the aircraft's is TIS-B value as a string.
         */
        formatIsTisb() : string
        {
            return VRS.format.isTisb(this.isTisb.val);
        }

        /**
         * Formats the aircraft's latitude as a string.
         */
        formatLatitude(showUnit: boolean) : string
        {
            return VRS.format.latitude(this.latitude.val, showUnit);
        }

        /**
         * Formats the aircraft's longitude as a string.
         */
        formatLongitude(showUnit: boolean) : string
        {
            return VRS.format.longitude(this.longitude.val, showUnit);
        }

        /**
         * Format's the aircraft's manufacturer as a string.
         */
        formatManufacturer() : string
        {
            return VRS.format.manufacturer(this.manufacturer.val);
        }

        /**
         * Formats the aircraft's model as a string.
         */
        formatModel() : string
        {
            return VRS.format.model(this.model.val);
        }

        /**
         * Formats the aircraft ICAO code for its model as a string.
         */
        formatModelIcao() : string
        {
            return VRS.format.modelIcao(this.modelIcao.val);
        }

        /**
         * True if the modelIcao is filled.
         */
        hasModelIcao() : boolean
        {
            return !!this.modelIcao.val;
        }

        /**
         * Formats the aircraft's ICAO code for its model as an HTML IMG tag for a silhouette image.
         */
        formatModelIcaoImageHtml() : string
        {
            return VRS.format.modelIcaoImageHtml(this.modelIcao.val, this.icao.val, this.registration.val);
        }

        /**
         * Formats the aircraft's ICAO code for its model as a description of engines, wake turbulence and species.
         */
        formatModelIcaoNameAndDetail() : string
        {
            return VRS.format.modelIcaoNameAndDetail(this.modelIcao.val, this.model.val, this.countEngines.val, this.engineType.val, this.species.val, this.wakeTurbulenceCat.val);
        }

        /**
         * Formats the aircraft's operator as a string.
         */
        formatOperator() : string
        {
            return VRS.format.operator(this.operator.val);
        }

        /**
         * Formats the aircraft's operator's ICAO code as a string.
         */
        formatOperatorIcao() : string
        {
            return VRS.format.operatorIcao(this.operatorIcao.val);
        }

        /**
         * Formats the aircraft's operator ICAO code and name as a string.
         */
        formatOperatorIcaoAndName() : string
        {
            return VRS.format.operatorIcaoAndName(this.operator.val, this.operatorIcao.val);
        }

        /**
         * Formats the aircraft's operator ICAO as an HTML IMG tag to the operator flag image.
         */
        formatOperatorIcaoImageHtml() : string
        {
            return VRS.format.operatorIcaoImageHtml(this.operator.val, this.operatorIcao.val, this.icao.val, this.registration.val);
        }

        /**
         * Returns an HTML IMG tag to a picture of the aircraft.
         */
        formatPictureHtml(requestSize: ISizePartial, allowResizeUp?: boolean, linkToOriginal?: boolean, blankSize?: ISize) : string
        {
            return VRS.format.pictureHtml(this.registration.val, this.icao.val, this.pictureWidth.val, this.pictureHeight.val, requestSize, allowResizeUp, linkToOriginal, blankSize);
        }

        /**
         * Returns the formatted name of the receiver that last picked up a message for this aircraft.
         */
        formatReceiver() : string
        {
            return VRS.format.receiver(this.receiverId.val, this._AircraftListFetcher);
        }

        /**
         * Returns the registration formatted as a string.
         */
        formatRegistration(onlyAlphaNumeric?: boolean) : string
        {
            return VRS.format.registration(this.registration.val, onlyAlphaNumeric);
        }

        /**
         * Returns the full route, including all stopovers and airport names, formatted as a string.
         */
        formatRouteFull() : string
        {
            return VRS.format.routeFull(this.callsign.val, this.from.val, this.to.val, this.getViaAirports());
        }

        /**
         * Returns HTML for the full route spread over multiple lines (separated by BR tags).
         */
        formatRouteMultiLine() : string
        {
            return VRS.format.routeMultiLine(this.callsign.val, this.from.val, this.to.val, this.getViaAirports());
        }

        /**
         * Returns HTML for the short route (where only airport codes are shown and stopovers can be reduced to an asterisk).
         */
        formatRouteShort(abbreviateStopovers?: boolean, showRouteNotKnown?: boolean) : string
        {
            return VRS.format.routeShort(this.callsign.val, this.from.val, this.to.val, this.getViaAirports(), abbreviateStopovers, showRouteNotKnown);
        }

        /**
         * Formats the seconds tracks as a string.
         */
        formatSecondsTracked() : string
        {
            return VRS.format.secondsTracked(this.secondsTracked);
        }

        /**
         * Formats the serial number.
         */
        formatSerial = function() : string
        {
            return VRS.format.serial(this.serial.val);
        }

        /**
         * Formats the signal level as a string.
         */
        formatSignalLevel() : string
        {
            return VRS.format.signalLevel(this.signalLevel.val);
        }

        /**
         * Formats the aircraft species as a string.
         */
        formatSpecies(ignoreNone: boolean) : string
        {
            return VRS.format.species(this.species.val, ignoreNone);
        }

        /**
         * Returns the speed formatted as a string.
         */
        formatSpeed(speedUnit: SpeedEnum, showUnit: boolean, showType: boolean)
        {
            return VRS.format.speed(this.speed.val, this.speedType.val, speedUnit, showUnit, showType);
        }

        /**
         * Returns the speed type formatted as a string.
         */
        formatSpeedType() : string
        {
            return VRS.format.speedType(this.speedType.val);
        }

        /**
         * Returns the squawk formatted as a string.
         */
        formatSquawk() : string
        {
            return VRS.format.squawk(this.squawk.val);
        }

        /**
         * Returns a description of the aircraft's squawk.
         */
        formatSquawkDescription() : string
        {
            return VRS.format.squawkDescription(this.squawk.val);
        }

        /**
         * Formats the target altitude as a string.
         */
        formatTargetAltitude(heightUnit: HeightEnum, showUnits: boolean, showType: boolean) : string
        {
            return VRS.format.altitude(this.targetAltitude.val, VRS.AltitudeType.Barometric, false, heightUnit, false, showUnits, showType);
        }

        /**
         * Formats the target heading as a string.
         */
        formatTargetHeading(showUnits: boolean, showType: boolean)
        {
            return VRS.format.heading(this.targetHeading.val, false, showUnits, showType);
        }

        /**
         * Returns the transponder type formatted as a string.
         */
        formatTransponderType() : string
        {
            return VRS.format.transponderType(this.transponderType.val);
        }

        /**
         * Returns the transponder type formatted as an IMG HTML element.
         */
        formatTransponderTypeImageHtml() : string
        {
            return VRS.format.transponderTypeImageHtml(this.transponderType.val);
        }

        /**
         * Returns the interested flag formatted as a string.
         */
        formatUserInterested() : string
        {
            return VRS.format.userInterested(this.userInterested.val);
        }

        /**
         * Returns the user notes formatted as a string.
         */
        formatUserNotes() : string
        {
            return VRS.format.userNotes(this.userNotes.val);
        }

        /**
         * Returns the user notes formatted as HTML with line breaks.
         */
        formatUserNotesMultiline() : string
        {
            return VRS.format.userNotesMultiline(this.userNotes.val);
        }

        /**
         * Returns the user tag formatted as a string.
         */
        formatUserTag() : string
        {
            return VRS.format.userTag(this.userTag.val);
        }

        /**
         * Returns the vertical speed formatted as a string.
         */
        formatVerticalSpeed(heightUnit: HeightEnum, perSecond: boolean, showUnit: boolean, showType: boolean) : string
        {
            return VRS.format.verticalSpeed(this.verticalSpeed.val, this.isOnGround.val, this.verticalSpeedType.val, heightUnit, perSecond, showUnit, showType);
        }

        /**
         * Returns the vertical speed type formatted as a string.
         */
        formatVerticalSpeedType() : string
        {
            return VRS.format.verticalSpeedType(this.verticalSpeedType.val);
        }

        /**
         * Returns the wake turbulence category formatted as a string.
         */
        formatWakeTurbulenceCat(ignoreNone: boolean, expandedDescription: boolean) : string
        {
            return VRS.format.wakeTurbulenceCat(this.wakeTurbulenceCat.val, ignoreNone, expandedDescription);
        }

        /**
         * Returns the year built as a string.
         */
        formatYearBuilt() : string
        {
            return VRS.format.yearBuilt(this.yearBuilt.val);
        }
    }
}
