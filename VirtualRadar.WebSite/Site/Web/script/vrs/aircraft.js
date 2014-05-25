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

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.suppressTrails = VRS.globalOptions.suppressTrails || false;       // If true then position history is not stored, significantly reducing memory footprint for each aircraft but prevents trails from being shown for the aircraft
    VRS.globalOptions.aircraftHideUncertainCallsigns = VRS.globalOptions.aircraftHideUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftHideUncertainCallsigns : false;   // True if callsigns that we're not 100% sure about are to be hidden from view.
    VRS.globalOptions.aircraftMaxAvgSignalLevelHistory = VRS.globalOptions.aircraftMaxAvgSignalLevelHistory !== undefined ? VRS.globalOptions.aircraftMaxAvgSignalLevelHistory : 6; // The number of signal levels to average out to determine average signal level. Don't make this more than about 10.
    //endregion

    //region Value, StringValue, BoolValue, NumberValue
    /**
     * An object that carries an abstract value and a flag indicating whether it has changed or not.
     * @param {*} value
     * @constructor
     */
    VRS.Value = function(value)
    {
        this.val = value;
        this.chg = false;

        /**
         * Forces a value onto the object and always sets the chg flag. This is used more by reports when creating fake
         * aircraft to ensure that all values set up for a fake aircraft will trigger a render in the objects that use
         * them.
         * @param {*} value
         */
        this.setValue = function(value)
        {
            this.val = value;
            this.chg = true;
        };
    };

    /**
     * A string value held by an aircraft and whether it changed in the last update.
     * @param {string} [value]
     * @constructor
     * @augments VRS.Value
     */
    VRS.StringValue = function(value)
    {
        VRS.Value.call(this, value);
    };
    VRS.StringValue.prototype = VRS.objectHelper.subclassOf(VRS.Value);

    /**
     * A boolean value held by an aircraft and whether it changed in the last update.
     * @param {bool} [value]
     * @constructor
     * @augments VRS.Value
     */
    VRS.BoolValue = function(value)
    {
        VRS.Value.call(this, value);
    };
    VRS.BoolValue.prototype = VRS.objectHelper.subclassOf(VRS.Value);

    /**
     * A number value held by an aircraft and whether it changed in the last update.
     * @param {number} [value]
     * @constructor
     * @augments VRS.Value
     */
    VRS.NumberValue = function(value)
    {
        VRS.Value.call(this, value);
    };
    VRS.NumberValue.prototype = VRS.objectHelper.subclassOf(VRS.Value);
    //endregion

    //region ArrayValue, RouteValue, ShortTrailValue, FullTrailValue
    /**
     * Describes an array of values held by an aircraft and whether it changed in the last update.
     * @param {Array} [array] The initial value to report.
     * @constructor
     */
    VRS.ArrayValue = function(array)
    {
        var that = this;

        /**
         * The array of values held by the aircraft.
         * @type {Array}
         */
        this.arr = array || [];

        /**
         * True if the array's content changed in the last update.
         * @type {boolean}
         */
        this.chg = false;

        /**
         * The index of the values added by the last update. Only meaningful if 'chg' is true.
         * @type {number}
         */
        this.chgIdx = -1;

        /**
         * The number of array elements trimmed from the start of the array in the last update.
         * @type {number}
         */
        this.trimStartCount = 0;

        /**
         * Sets the array and configures the other fields to indicate that the entire array is new. Used when setting
         * up fake aircraft for reports etc.
         * @param {Array} value
         */
        this.setValue = function(value)
        {
            if(value && value.length) {
                that.value = value;
                that.chg = true;
                that.chgIdx = 0;
                that.trimStartCount = 0;
            }
        };

        /**
         * Sets the fields to indicate that nothing changed in the last update.
         */
        this.setNoChange = function()
        {
            that.chg = false;
            that.chgIdx = -1;
            that.trimStartCount = 0;
        };

        /**
         * Sets the fields to indicate that the array was reset back to an empty state in the last update.
         */
        this.resetArray = function()
        {
            if(that.arr.length > 0) {
                that.trimStartCount = that.arr.length;
                that.arr = [];
                that.chg = true;
                that.chgIdx = -1;
            }
        };

        /**
         * Trims elements from the start of the array and sets the fields to indicate that this has happened.
         * @param {number} trimCount
         */
        this.trimStart = function(trimCount)
        {
            if(trimCount > 0) {
                if(trimCount > that.arr.length) trimCount = that.arr.length;
                that.arr.splice(0, trimCount);
                that.trimStartCount = trimCount;
                that.chg = true;
            }
        };
    };

    /**
     * Describes a route held by an aircraft and whether it changed in the last update.
     * @param {string} [value] An airport code followed by the description of the airport.
     * @constructor
     * @augments VRS.Value
     */
    VRS.RouteValue = function(value)
    {
        var _AirportCodeDerivedFromVal;
        var _AirportCode;

        VRS.Value.call(this, value);

        /**
         * Returns the airport code from the route value.
         * @returns {string}
         */
        this.getAirportCode = function()
        {
            if(_AirportCodeDerivedFromVal !== this.val) {
                _AirportCodeDerivedFromVal = this.val;
                _AirportCode = VRS.format.routeAirportCode(_AirportCodeDerivedFromVal);
            }
            return _AirportCode;
        };
    };
    VRS.RouteValue.prototype = VRS.objectHelper.subclassOf(VRS.Value);

    /**
     * Describes a set of thumbnails that have been fetched for the aircraft from www.airport-data.com.
     * @param {VRS_JSON_AIRPORTDATA_THUMBNAILS} value
     * @constructor
     */
    VRS.AirportDataThumbnailValue = function(value)
    {
        var _LastResetChgValue = false;

        VRS.Value.call(this, value);

        /**
         * Called on every refresh. Thumbnails are set asynchronously, they are set when some code calls a fetch method
         * for them on VRS.Aircraft. Once the response comes back from the server chg is set to true. We want that chg
         * value to remain true for the next refresh of the aircraft list and then for all future refreshes it should
         * be false unless the ICAO changes. This method is called on every refresh - if the chg value has changed from
         * the previous refresh then it is left alone, otherwise it is set to false.
         */
        this.resetChg = function()
        {
            if(this.chg && _LastResetChgValue) this.chg = false;
            _LastResetChgValue = this.chg;
        };
    };
    VRS.AirportDataThumbnailValue.prototype = VRS.objectHelper.subclassOf(VRS.Value);

    /**
     * Describes a coordinate held by an aircraft in its short trail. These have no Chg field, the array holding these
     * values records the index of any new entries.
     * @param {number} latitude     The latitude of the aircraft.
     * @param {number} longitude    The longitude of the aircraft.
     * @param {number} posnTick     The time at which the aircraft was observed at the coordinate.
     * @param {number} [altitude]   The altitude at this coordinate.
     * @param {number} [speed]      The speed at this coordinate.
     * @constructor
     */
    VRS.ShortTrailValue = function(latitude, longitude, posnTick, altitude, speed)
    {
        /**
         * The latitude of the aircraft.
         * @type {number}
         */
        this.lat = latitude;

        /**
         * The longitude of the aircraft.
         * @type {number}
         */
        this.lng = longitude;

        /**
         * The time at which the aircraft was observed at the coordinate, in ticks.
         * @type {number}
         */
        this.posnTick = posnTick;

        /**
         * The altitude, if any, that the aircraft had reached at this point.
         * @type {number=}
         */
        this.altitude = altitude;

        /**
         * The speed, if any, that the aircraft had reached at this point.
         * @type {number=}
         */
        this.speed = speed;
    };

    /**
     * Describes a coordinate held by an aircraft in its full trail.
     * @param {number}  latitude    The latitude of the aircraft.
     * @param {number}  longitude   The longitude of the aircraft.
     * @param {number}  heading     The compass heading from 0 to 360 that the aircraft was heading in at this coordinate.
     * @param {number} [altitude]   The altitude at this coordinate.
     * @param {number} [speed]      The speed at this coordinate.
     * @constructor
     */
    VRS.FullTrailValue = function(latitude, longitude, heading, altitude, speed)
    {
        /**
         * The latitude of the aircraft.
         * @type {number}
         */
        this.lat = latitude;

        /**
         * The longitude of the aircraft.
         * @type {number}
         */
        this.lng = longitude;

        /**
         * The heading that the aircraft was pointing in.
         * @type {number}
         */
        this.heading = heading;

        /**
         * The altitude, if any, that the aircraft had reached at this point.
         * @type {number=}
         */
        this.altitude = altitude;

        /**
         * The speed, if any, that the aircraft had reached at this point.
         * @type {number=}
         */
        this.speed = speed;

        /**
         * True when this is the last element in the array. It is set to true if the aircraft changed position but its
         * heading was the same as it was in the previous update, in which case the last element in the trail is set to
         * the new position and the polyline describing the trail should just be extended to this new location.
         * @type {boolean}
         */
        this.chg = false;
    };
    //endregion

    //region Aircraft
    /**
     * Describes an aircraft tracked by the server.
     * @constructor
     */
    VRS.Aircraft = function()
    {
        //region -- Private fields
        var that = this;

        /**
         * The aircraft list fetcher that last supplied details for the aircraft. Usually there's just one of these for
         * the lifetime of the site.
         * @type {VRS.AircraftListFetcher}
         * @private
         */
        var _AircraftListFetcher = null;

        /**
         * A fixed length array of the signal levels for the aircraft, used to determine averageSignalLevel.
         * @type {Array}
         */
        var signalLevelHistory = [];
        //endregion

        //region -- Public fields
        /**
         * The unique identifier of the aircraft, derived from the ICAO.
         * @type {number}
         */
        this.id = 0;

        /**
         * The number of seconds that the aircraft has been tracked for.
         * @type {number}
         */
        this.secondsTracked = 0;

        /**
         * The number of times the aircraft details have been updated.
         * @type {number}
         */
        this.updateCounter = 0;

        /**
         * The identity of the receiver.
         * @type {VRS.NumberValue}
         */
        this.receiverId =               new VRS.NumberValue();
        this.icao =                     new VRS.StringValue();
        this.icaoInvalid =              new VRS.BoolValue();
        this.registration =             new VRS.StringValue();
        this.altitude =                 new VRS.NumberValue();
        this.altitudeType =             new VRS.NumberValue();
        this.callsign =                 new VRS.StringValue();
        this.callsignSuspect =          new VRS.BoolValue();
        this.latitude =                 new VRS.NumberValue();
        this.longitude =                new VRS.NumberValue();
        this.positionTime =             new VRS.NumberValue();
        this.speed =                    new VRS.NumberValue();
        this.speedType =                new VRS.NumberValue();
        this.verticalSpeed =            new VRS.NumberValue();
        this.verticalSpeedType =        new VRS.NumberValue();
        this.heading =                  new VRS.NumberValue();            // The track across the ground that the aircraft is following, unless headingIsTrue is true in which case it's the aircraft's true heading (i.e. the direction the nose is pointing in)
        this.headingIsTrue =            new VRS.BoolValue();              // True if heading is the aircraft's true heading, false if it's the ground track.
        this.model =                    new VRS.StringValue();
        this.modelIcao =                new VRS.StringValue();
        this.from =                     new VRS.RouteValue();
        this.to =                       new VRS.RouteValue();
        this.via =                      new VRS.ArrayValue();
        this.operator =                 new VRS.StringValue();
        this.operatorIcao =             new VRS.StringValue();
        this.squawk =                   new VRS.StringValue();
        this.isEmergency =              new VRS.BoolValue();
        this.distanceFromHereKm =       new VRS.NumberValue();
        this.bearingFromHere =          new VRS.NumberValue();            // The bearing from the browser's location to the aircraft, assuming that the browser is pointing due north
        this.wakeTurbulenceCat =        new VRS.StringValue();
        this.countEngines =             new VRS.StringValue();
        this.engineType =               new VRS.NumberValue();
        this.species =                  new VRS.StringValue();
        this.isMilitary =               new VRS.BoolValue();
        this.country =                  new VRS.StringValue();
        this.hasPicture =               new VRS.BoolValue();
        this.pictureWidth =             new VRS.NumberValue();
        this.pictureHeight =            new VRS.NumberValue();
        this.countFlights =             new VRS.NumberValue();
        this.countMessages =            new VRS.NumberValue();
        this.isOnGround =               new VRS.BoolValue();
        this.userTag =                  new VRS.StringValue();
        this.userInterested =           new VRS.BoolValue();
        this.signalLevel =              new VRS.NumberValue();
        this.averageSignalLevel =       new VRS.NumberValue();
        this.airportDataThumbnails =    new VRS.AirportDataThumbnailValue();

        this.shortTrail =               new VRS.ArrayValue();
        this.fullTrail =                new VRS.ArrayValue();
        //endregion

        //region -- applyJson
        /**
         * Applies details about an individual aircraft's current state to the aircraft object.
         * In general most of the fields are optional. If they are missing then the value has not
         * changed. We need to track values that changed so that we only update the UI for those
         * and not for every value on every refresh. At best that can cause flicker, at worst it
         * can hammer the browser.
         * @param {VRS_JSON_AIRCRAFT}       aircraftJson            The single JSON record for an aircraft.
         * @param {VRS.AircraftListFetcher} aircraftListFetcher     The fetcher that obtained the JSON.
         * @param {object}                  settings                The settings required to correctly decode the aircraft's JSON.
         * @param {number}                  settings.shortTrailTickThreshold    The earliest allowable tick in a short trail list.
         * Coordinates for ticks before this value must be removed on update. If -1 then no short trails should be recorded.
         * @param {boolean}                 settings.picturesEnabled            True if the user can see local pictures.
         */
        this.applyJson = function(aircraftJson, aircraftListFetcher, settings)
        {
            this.id = aircraftJson.Id;
            this.secondsTracked = aircraftJson.TSecs;
            _AircraftListFetcher = aircraftListFetcher;
            ++this.updateCounter;

            setValue(this.receiverId,           aircraftJson.Rcvr);
            setValue(this.icao,                 aircraftJson.Icao);
            setValue(this.icaoInvalid,          aircraftJson.Bad);
            setValue(this.registration,         aircraftJson.Reg);
            setValue(this.altitude,             aircraftJson.Alt);
            setValue(this.altitudeType,         aircraftJson.AltT);
            setValue(this.callsign,             aircraftJson.Call);
            setValue(this.callsignSuspect,      aircraftJson.CallSus);
            setValue(this.latitude,             aircraftJson.Lat);
            setValue(this.longitude,            aircraftJson.Long);
            setValue(this.positionTime,         aircraftJson.PosTime);
            setValue(this.speed,                aircraftJson.Spd);
            setValue(this.speedType,            aircraftJson.SpdTyp);
            setValue(this.verticalSpeed,        aircraftJson.Vsi);
            setValue(this.verticalSpeedType,    aircraftJson.VsiT);
            setValue(this.heading,              aircraftJson.Trak);
            setValue(this.headingIsTrue,        aircraftJson.TrkH);
            setValue(this.model,                aircraftJson.Mdl);
            setValue(this.modelIcao,            aircraftJson.Type);
            setValue(this.from,                 aircraftJson.From);
            setValue(this.to,                   aircraftJson.To);
            setValue(this.operator,             aircraftJson.Op);
            setValue(this.operatorIcao,         aircraftJson.OpIcao);
            setValue(this.squawk,               aircraftJson.Sqk);
            setValue(this.isEmergency,          aircraftJson.Help);
            setValue(this.distanceFromHereKm,   aircraftJson.Dst, true);
            setValue(this.bearingFromHere,      aircraftJson.Brng, true);
            setValue(this.wakeTurbulenceCat,    aircraftJson.WTC);
            setValue(this.countEngines,         aircraftJson.Engines);
            setValue(this.engineType,           aircraftJson.EngType);
            setValue(this.species,              aircraftJson.Species);
            setValue(this.isMilitary,           aircraftJson.Mil);
            setValue(this.country,              aircraftJson.Cou);
            setValue(this.hasPicture,           aircraftJson.HasPic && settings.picturesEnabled);
            setValue(this.pictureWidth,         aircraftJson.PicX);
            setValue(this.pictureHeight,        aircraftJson.PicY);
            setValue(this.countFlights,         aircraftJson.FlightsCount);
            setValue(this.countMessages,        aircraftJson.CMsgs);
            setValue(this.isOnGround,           aircraftJson.Gnd);
            setValue(this.userTag,              aircraftJson.Tag);
            setValue(this.userInterested,       aircraftJson.Interested);
            setRouteArray(this.via,             aircraftJson.Stops);

            if(aircraftJson.HasSig !== undefined) {
                setValue(this.signalLevel, aircraftJson.Sig);
            }
            recordSignalLevelHistory(this.signalLevel.val);

            if(!VRS.globalOptions.suppressTrails) {
                setShortTrailArray(this.shortTrail, aircraftJson.Cos, aircraftJson.ResetTrail, settings.shortTrailTickThreshold, aircraftJson.TT);
                setFullTrailArray(this.fullTrail, aircraftJson.Cot, aircraftJson.ResetTrail, aircraftJson.TT)
            }

            if(VRS.globalOptions.aircraftHideUncertainCallsigns && this.callsignSuspect.val) {
                this.callsign.val = undefined;
                this.callsign.chg = false;
            }

            this.airportDataThumbnails.resetChg();
        };

        /**
         * Assigns content to a value object.
         * @param {VRS.StringValue|VRS.BoolValue|VRS.NumberValue}   field
         * @param {*}                                               jsonValue
         * @param {bool=}                                           alwaysSent
         */
        function setValue(field, jsonValue, alwaysSent)
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
         * @param {VRS.ArrayValue}  field
         * @param {*}               jsonArray
         */
        function setRouteArray(field, jsonArray)
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
         * @param {VRS.ArrayValue}  field                       The ArrayValue field that holds the short trails.
         * @param {Array}           jsonArray                   The array of new coordinates to add to the trail. It is
         * a simple single dimension array to be parsed in groups of 3 - the 1st value is latitude, the 2nd is longitude
         * and the 3rd is the server time (expressed as a tick) when the coordinate was recorded.
         * @param {bool}            resetTrail                  True if the trail is to be erased before doing any work.
         * @param {number}          shortTrailTickThreshold     The server time, expressed as a tick, of the oldest
         * allowable short trail coordinate. Coordinates before this point must be removed from the trail.
         * @param {string}          trailType                   'a' if the trail has altitudes on it, 's' if it has speeds
         */
        function setShortTrailArray(field, jsonArray, resetTrail, shortTrailTickThreshold, trailType)
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
         * @param {VRS.ArrayValue}  field       The ArrayValue field that holds the full trail points.
         * @param {Number[]}        jsonArray   A single-dimensional array holding new points to add to the trail. The
         * array is to be read in groups of 3, where the 1st element is latitude, 2nd is longitude and 3rd is heading
         * that the aircraft was pointing in when the coordinate was recorded.
         * @param {bool}            resetTrail  True if the array should be reset before adding any points to it.
         * @param {string}          trailType   'a' if the trail has altitudes on it, 's' if it has speeds
         */
        function setFullTrailArray(field, jsonArray, resetTrail, trailType)
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
         * @param {Number=} signalLevel
         */
        function recordSignalLevelHistory(signalLevel)
        {
            /** @type {Number} */
            var averageSignalLevel = 0;

            if(VRS.globalOptions.aircraftMaxAvgSignalLevelHistory) {
                if(signalLevel === null) {
                    if(signalLevelHistory.length !== 0) signalLevelHistory = [];
                } else {
                    var i;
                    averageSignalLevel = signalLevel;
                    var length = signalLevelHistory.length;
                    if(length < VRS.globalOptions.aircraftMaxAvgSignalLevelHistory) {
                        for(i = 0;i < length;++i) {
                            averageSignalLevel += signalLevelHistory[i];
                        }
                        signalLevelHistory.push(signalLevel);
                        ++length;
                    } else {
                        var shiftLength = length - 1;
                        for(i = 0;i < shiftLength;++i) {
                            var useValue = signalLevelHistory[i + 1];
                            signalLevelHistory[i] = useValue;
                            averageSignalLevel += useValue;
                        }
                        signalLevelHistory[shiftLength] = signalLevel;
                    }
                    averageSignalLevel = Math.floor(averageSignalLevel / length);
                }
            }

            setValue(that.averageSignalLevel, averageSignalLevel, true);
        }
        //endregion

        //region -- hasPosition, getPosition, positionWithinBounds
        /**
         * Returns true if the aircraft has a position associated with it.
         * @returns {bool}
         */
        this.hasPosition = function()
        {
            return this.positionTime.val > 0;
        };

        /**
         * Returns the position of the aircraft as an object or null if the aircraft has no position.
         * @returns {VRS_LAT_LNG}
         */
        this.getPosition = function()
        {
            return this.hasPosition() ? { lat: this.latitude.val, lng: this.longitude.val } : null;
        };

        /**
         * Returns true if the aircraft is within the bounding box described by the parameters.
         * @param {VRS_BOUNDS} bounds The boundary expressed as top-left / bottom-right.
         * @returns {boolean}
         */
        this.positionWithinBounds = function(bounds)
        {
            /// <summary></summary>
            /// <param name="bounds"></param>
            /// <returns type="Boolean">True if the aircraft is within the boundaries, false otherwise.</returns>

            var result = this.hasPosition();
            if(result) result = VRS.greatCircle.isLatLngInBounds(this.latitude.val, this.longitude.val, bounds);

            return result;
        };
        //endregion

        //region -- hasRoute, hasRouteChanged, getViaAirports
        /**
         * Returns true if the aircraft has a route, false if it does not.
         * @returns {boolean}
         */
        this.hasRoute = function()
        {
            return !!this.from.val && !!this.to.val;
        };

        /**
         * Returns true if the aircraft's route has changed.
         * @returns {boolean}
         */
        this.hasRouteChanged = function()
        {
            return this.from.chg || this.to.chg || this.via.chg;
        };

        /**
         * Returns a copy of the airports from the via routes array.
         * @returns {Array.<string>}
         */
        this.getViaAirports = function()
        {
            var result = [];

            var length = this.via.arr.length;
            for(var i = 0;i < length;++i) {
                result.push(this.via.arr[i].val);
            }

            return result;
        };

        /**
         * Returns an array of all of the airport codes in the route.
         * @param {boolean} [distinctOnly]  True if only distinct codes are to be returned.
         */
        this.getAirportCodes = function(distinctOnly)
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
        };
        //endregion

        //region -- isAircraftSpecies
        /**
         * Returns true if the aircraft is some kind of aircraft, false if it is a ground vehicle or radio mast.
         * @returns {boolean}
         */
        this.isAircraftSpecies = function()
        {
            return this.species.val !== VRS.Species.GroundVehicle && this.species.val !== VRS.Species.Tower;
        };
        //endregion

        //region -- convert***
        /**
         * Returns the speed converted from knots (as sent by the server) to the unit passed across.
         * @param {VRS.Speed} toUnit The unit to convert to.
         * @returns {number=}
         */
        this.convertSpeed = function(toUnit)
        {
            /// <summary></summary>
            var result = this.speed.val;
            if(result !== undefined && toUnit !== VRS.Speed.Knots) {
                result = VRS.unitConverter.convertSpeed(result, VRS.Speed.Knots, toUnit);
            }
            return result;
        };

        /**
         * Returns the altitude converted from feet (as sent by the server) to the unit passed across.
         * @param {VRS.Height} toUnit The unit to convert to.
         * @returns {number=}
         */
        this.convertAltitude = function(toUnit)
        {
            var result = this.altitude.val;
            if(result !== undefined && toUnit !== VRS.Height.Feet) {
                result = VRS.unitConverter.convertHeight(result, VRS.Height.Feet, toUnit);
            }
            return result;
        };

        /**
         * Returns the distance from here converted from kilometres (as sent by the server) to the unit passed across.
         * @param {VRS.Distance} toUnit The VRS.Distance unit to convert to.
         * @returns {number=}
         */
        this.convertDistanceFromHere = function(toUnit)
        {
            var result = this.distanceFromHereKm.val;
            if(result !== undefined && toUnit !== VRS.Distance.Kilometre) {
                result = VRS.unitConverter.convertDistance(result, VRS.Distance.Kilometre, toUnit);
            }
            return result;
        };

        /**
         * Returns the vertical speed converted from feet per minute to the unit passed across.
         * @param {VRS.Height}  toUnit      The VRS.Height unit to convert to.
         * @param {bool}        perSecond   True if the unit is to be expressed per second instead of per minute.
         * @returns {number=}
         */
        this.convertVerticalSpeed = function(toUnit, perSecond)
        {
            return VRS.unitConverter.convertVerticalSpeed(this.verticalSpeed.val, VRS.Height.Feet, toUnit, perSecond);
        };
        //endregion

        //region -- fetchAirportDataThumbnails
        /**
         * Fetches thumbnails for the aircraft from www.airport-data.com. Once the fetch has been satisfied the chg
         * value for airportDataThumbnail will be set for the next refresh of aircraft data, and then cleared on the
         * subsequent refresh.
         * @param {number=}     numThumbnails       The number of thumbnails to fetch, defaults to 1.
         */
        this.fetchAirportDataThumbnails = function(numThumbnails)
        {
            if(!numThumbnails) numThumbnails = 1;
            if(this.icao.val) {
                var fetcher = new VRS.AirportDataApi();
                fetcher.getThumbnails(this.icao.val, this.registration.val, numThumbnails, function(icao, thumbnails) {
                    if(icao === that.icao.val) that.airportDataThumbnails.setValue(thumbnails);
                });
            }
        };
        //endregion

        //region -- format***
        this.formatAirportDataThumbnails = function(showLinkToSite)
        {
            if(showLinkToSite === undefined) showLinkToSite = true;
            return VRS.format.airportDataThumbnails(this.airportDataThumbnails.val, showLinkToSite);
        };

        /**
         * Formats the altitude as a string.
         * @param {VRS.Height}  heightUnit          The VRS.Height unit to use.
         * @param {bool}        distinguishOnGround True if aircraft on the ground are to be shown as 'GND' instead of the altitude.
         * @param {bool}        showUnits           True if units are to be shown.
         * @param {bool}        showType            True if the type of altitude is to be shown.
         * @returns {string}
         */
        this.formatAltitude = function(heightUnit, distinguishOnGround, showUnits, showType)
        {
            return VRS.format.altitude(this.altitude.val, this.altitudeType.val, this.isOnGround.val, heightUnit, distinguishOnGround, showUnits, showType);
        };

        /**
         * Formats the altitude type as a string.
         * @returns {string}
         */
        this.formatAltitudeType = function()
        {
            return VRS.format.altitudeType(this.altitudeType.val);
        };

        /**
         * Formats the average signal level as a string.
         * @returns {string}
         */
        this.formatAverageSignalLevel = function()
        {
            return VRS.format.averageSignalLevel(this.averageSignalLevel.val);
        };

        /**
         * Formats the bearing from here as a string.
         * @param {bool} showUnits True if units are to be shown.
         * @returns {string}
         */
        this.formatBearingFromHere = function(showUnits)
        {
            return VRS.format.bearingFromHere(this.bearingFromHere.val, showUnits);
        };

        /**
         * Formats the bearing from here as an HTML IMG tag.
         * @returns {string}
         */
        this.formatBearingFromHereImage = function()
        {
            return VRS.format.bearingFromHereImage(this.bearingFromHere.val);
        };

        /**
         * Formats the callsign as a string.
         * @param {bool} showUncertainty True if uncertain callsigns are to be flagged.
         * @returns {string}
         */
        this.formatCallsign = function(showUncertainty)
        {
            return VRS.format.callsign(this.callsign.val, this.callsignSuspect.val, showUncertainty);
        };

        /**
         * Formats the count of flights as a string.
         * @param {string} [format] The number format to use when formatting the count of flights.
         * @returns {string}
         */
        this.formatCountFlights = function(format)
        {
            return VRS.format.countFlights(this.countFlights.val, format);
        };

        /**
         * Formats the count of messages as a string.
         * @param {string} [format] The .NET format to use when formatting the count.
         * @returns {string}
         */
        this.formatCountMessages = function(format)
        {
            return VRS.format.countMessages(this.countMessages.val, format);
        };

        /**
         * Formats the country as a string.
         * @returns {string}
         */
        this.formatCountry = function()
        {
            return VRS.format.country(this.country.val);
        };

        /**
         * Formats the distance from here as a string.
         * @param {VRS.Distance} distanceUnit The VRS.Distance unit to use in formatting.
         * @param {bool} showUnits True if units are to be shown.
         * @returns {string}
         */
        this.formatDistanceFromHere = function(distanceUnit, showUnits)
        {
            return VRS.format.distanceFromHere(this.distanceFromHereKm.val, distanceUnit, showUnits);
        };

        /**
         * Formats the count of engines and engine type as a string.
         * @returns {string}
         */
        this.formatEngines = function()
        {
            return VRS.format.engines(this.countEngines.val, this.engineType.val);
        };

        /**
         * Formats the flight level as a string.
         * @param {number}      transitionAltitude      The altitude above which flight levels are reported and below which altitudes are reported.
         * @param {VRS.Height}  transitionAltitudeUnit  The VRS.Height unit that the transition altitude is in.
         * @param {VRS.Height}  flightLevelAltitudeUnit The VRS.Height unit to report flight levels with.
         * @param {VRS.Height}  altitudeUnit            The VRS.Height unit to report altitudes with.
         * @param {bool}        distinguishOnGround     True if aircraft on the ground are to be reported as GND.
         * @param {bool}        showUnits               True if units are to be shown.
         * @param {bool}        showType                True if altitude type is to be shown.
         * @returns {string}
         */
        this.formatFlightLevel = function(transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, showType)
        {
            return VRS.format.flightLevel(this.altitude.val, this.altitudeType.val, this.isOnGround.val, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, showType);
        };

        /**
         * Formats the aircraft's heading as a string.
         * @param {bool} showUnit True if the units are to be shown.
         * @param {bool} showType True if the type of heading is to be shown.
         * @returns {string}
         */
        this.formatHeading = function(showUnit, showType)
        {
            return VRS.format.heading(this.heading.val, this.headingIsTrue.val, showUnit, showType);
        };

        /**
         * Formats the aircraft's heading type as a string.
         * @returns {string}
         */
        this.formatHeadingType = function()
        {
            return VRS.format.headingType(this.headingIsTrue.val);
        };

        /**
         * Formats the aircraft's ICAO as a string.
         * @returns {string}
         */
        this.formatIcao = function()
        {
            return VRS.format.icao(this.icao.val);
        };

        /**
         * Formats the aircraft's military status as a string.
         * @returns {string}
         */
        this.formatIsMilitary = function()
        {
            return VRS.format.isMilitary(this.isMilitary.val);
        };

        /**
         * Formats the aircraft's latitude as a string.
         * @param {bool} showUnit True if units are to be shown.
         * @returns {string}
         */
        this.formatLatitude = function(showUnit)
        {
            return VRS.format.latitude(this.latitude.val, showUnit);
        };

        /**
         * Formats the aircraft's longitude as a string.
         * @param {bool} showUnit True if units are to be shown.
         * @returns {string}
         */
        this.formatLongitude = function(showUnit)
        {
            return VRS.format.longitude(this.longitude.val, showUnit);
        };

        /**
         * Formats the aircraft's model as a string.
         * @returns {string}
         */
        this.formatModel = function()
        {
            return VRS.format.model(this.model.val);
        };

        /**
         * Formats the aircraft ICAO code for its model as a string.
         * @returns {string}
         */
        this.formatModelIcao = function()
        {
            return VRS.format.modelIcao(this.modelIcao.val);
        };

        /**
         * Formats the aircraft's ICAO code for its model as an HTML IMG tag for a silhouette image.
         * @returns {string}
         */
        this.formatModelIcaoImageHtml = function()
        {
            return VRS.format.modelIcaoImageHtml(this.modelIcao.val, this.icao.val, this.registration.val);
        };

        /**
         * Formats the aircraft's ICAO code for its model as a description of engines, wake turbulence and species.
         * @returns {string}
         */
        this.formatModelIcaoNameAndDetail = function()
        {
            return VRS.format.modelIcaoNameAndDetail(this.modelIcao.val, this.model.val, this.countEngines.val, this.engineType.val, this.species.val, this.wakeTurbulenceCat.val);
        };

        /**
         * Formats the aircraft's operator as a string.
         * @returns {string}
         */
        this.formatOperator = function()
        {
            return VRS.format.operator(this.operator.val);
        };

        /**
         * Formats the aircraft's operator's ICAO code as a string.
         * @returns {string}
         */
        this.formatOperatorIcao = function()
        {
            return VRS.format.operatorIcao(this.operatorIcao.val);
        };

        /**
         * Formats the aircraft's operator ICAO code and name as a string.
         * @returns {string}
         */
        this.formatOperatorIcaoAndName = function()
        {
            return VRS.format.operatorIcaoAndName(this.operator.val, this.operatorIcao.val);
        };

        /**
         * Formats the aircraft's operator ICAO as an HTML IMG tag to the operator flag image.
         * @returns {string}
         */
        this.formatOperatorIcaoImageHtml = function()
        {
            return VRS.format.operatorIcaoImageHtml(this.operator.val, this.operatorIcao.val, this.icao.val, this.registration.val);
        };

        //noinspection JSUnusedLocalSymbols
        /**
         * Returns an HTML IMG tag to a picture of the aircraft.
         * @param {VRS_SIZE}    requestSize         The size of image to request. Height is optional. Pass null to request the original image size.
         * @param {bool}       [allowResizeUp]      True to always use the request size, false if resizes cannot be larger than the original picture size.
         * @param {bool}       [linkToOriginal]     True if the IMG tag should be wrapped in an A tag pointing back to the full-sized picture.
         * @param {VRS_SIZE}   [blankSize]          The size of the blank image to use if the aircraft has no picture.
         * @returns {string}
         */
        this.formatPictureHtml = function(requestSize, allowResizeUp, linkToOriginal, blankSize)
        {
            return VRS.format.pictureHtml(this.registration.val, this.icao.val, this.pictureWidth.val, this.pictureHeight.val, requestSize, allowResizeUp, linkToOriginal, blankSize);
        };

        /**
         * Returns the formatted name of the receiver that last picked up a message for this aircraft.
         * @returns {string}
         */
        this.formatReceiver = function()
        {
            return VRS.format.receiver(this.receiverId.val, _AircraftListFetcher);
        };

        /**
         * Returns the registration formatted as a string.
         * @param {bool} [onlyAlphaNumeric] True if non-alphanumeric characters in the registration are to be stripped out.
         * @returns {string=}
         */
        this.formatRegistration = function(onlyAlphaNumeric)
        {
            return VRS.format.registration(this.registration.val, onlyAlphaNumeric);
        };

        /**
         * Returns the full route, including all stopovers and airport names, formatted as a string.
         * @returns {string}
         */
        this.formatRouteFull = function()
        {
            return VRS.format.routeFull(this.callsign.val, this.from.val, this.to.val, this.getViaAirports());
        };

        /**
         * Returns HTML for the full route spread over multiple lines (separated by BR tags).
         * @returns {string}
         */
        this.formatRouteMultiLine = function()
        {
            return VRS.format.routeMultiLine(this.callsign.val, this.from.val, this.to.val, this.getViaAirports());
        };

        /**
         * Returns HTML for the short route (where only airport codes are shown and stopovers can be reduced to an asterisk).
         * @param {bool} [abbreviateStopovers]      True if stopovers are to be shown as a single *. Defaults to true.
         * @param {bool} [showRouteNotKnown]        True if an unknown route is to be shown as 'Route not known'. Defaults to false.
         * @returns {string}
         */
        this.formatRouteShort = function(abbreviateStopovers, showRouteNotKnown)
        {
            return VRS.format.routeShort(this.callsign.val, this.from.val, this.to.val, this.getViaAirports(), abbreviateStopovers, showRouteNotKnown);
        };

        /**
         * Formats the seconds tracks as a string.
         * @returns {string}
         */
        this.formatSecondsTracked = function()
        {
            return VRS.format.secondsTracked(this.secondsTracked);
        };

        /**
         * Formats the signal level as a string.
         * @returns {string}
         */
        this.formatSignalLevel = function()
        {
            return VRS.format.signalLevel(this.signalLevel.val);
        };

        /**
         * Formats the aircraft species as a string.
         * @param {bool} ignoreNone True if an empty string is to be returned when the species is unknown instead of some warning text.
         * @returns {string}
         */
        this.formatSpecies = function(ignoreNone)
        {
            return VRS.format.species(this.species.val);
        };

        /**
         * Returns the speed formatted as a string.
         * @param {VRS.Speed}   speedUnit   The VRS.Speed unit to use when formatting the speed.
         * @param {bool}        showUnit    True if the units are to be shown.
         * @param {bool}        showType    True if the type of speed is to be shown.
         * @returns {string}
         */
        this.formatSpeed = function(speedUnit, showUnit, showType)
        {
            return VRS.format.speed(this.speed.val, this.speedType.val, speedUnit, showUnit, showType);
        };

        /**
         * Returns the speed type formatted as a string.
         * @returns {string}
         */
        this.formatSpeedType = function()
        {
            return VRS.format.speedType(this.speedType.val);
        };

        /**
         * Returns the squawk formatted as a string.
         * @returns {string}
         */
        this.formatSquawk = function()
        {
            return VRS.format.squawk(this.squawk.val);
        };

        /**
         * Returns the interested flag formatted as a string.
         */
        this.formatUserInterested = function()
        {
            return VRS.format.userInterested(this.userInterested.val);
        };

        /**
         * Returns the user tag formatted as a string.
         * @returns {string}
         */
        this.formatUserTag = function()
        {
            return VRS.format.userTag(this.userTag.val);
        };

        /**
         * Returns the vertical speed formatted as a string.
         * @param {VRS.Height}  heightUnit  The VRS.Height unit to use when formatting.
         * @param {bool}        perSecond   True if the VSI is /second, false if it's /minute.
         * @param {bool}        showUnit    True if units are to be shown.
         * @param {bool}        showType    True if the vertical speed type is to be shown.
         * @returns {string}
         */
        this.formatVerticalSpeed = function(heightUnit, perSecond, showUnit, showType)
        {
            return VRS.format.verticalSpeed(this.verticalSpeed.val, this.verticalSpeedType.val, heightUnit, perSecond, showUnit, showType);
        };

        /**
         * Returns the vertical speed type formatted as a string.
         * @returns {string}
         */
        this.formatVerticalSpeedType = function()
        {
            return VRS.format.verticalSpeedType(this.verticalSpeedType.val);
        };

        /**
         * Returns the wake turbulence category formatted as a string.
         * @param {bool} ignoreNone             True if an empty string is to be returned instead of warning text when the WTC is unknown.
         * @param {bool} expandedDescription    True if the rough weight limits are to be shown.
         * @returns {string}
         */
        this.formatWakeTurbulenceCat = function(ignoreNone, expandedDescription)
        {
            return VRS.format.wakeTurbulenceCat(this.wakeTurbulenceCat.val, ignoreNone, expandedDescription);
        };
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));