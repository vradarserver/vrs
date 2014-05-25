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
 * @fileoverview Common formatting of aircraft values.
 */

(function(VRS, $, /** Object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftOperatorFlagSize = VRS.globalOptions.aircraftOperatorFlagSize || { width: 85, height: 20 };       // The dimensions of operator flags
    VRS.globalOptions.aircraftSilhouetteSize = VRS.globalOptions.aircraftSilhouetteSize || { width: 85, height: 20 };           // The dimensions of silhouette flags
    VRS.globalOptions.aircraftBearingCompassSize = VRS.globalOptions.aircraftBearingCompassSize || { width: 16, height: 16 };   // The dimensions of the bearing compass image.
    VRS.globalOptions.aircraftFlagUncertainCallsigns = VRS.globalOptions.aircraftFlagUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftFlagUncertainCallsigns : true;    // True if callsigns that we're not 100% sure about are to be shown with an asterisk against them.
    VRS.globalOptions.aircraftAllowRegistrationFlagOverride = VRS.globalOptions.aircraftAllowRegistrationFlagOverride !== undefined ? VRS.globalOptions.aircraftAllowRegistrationFlagOverride : false;  // True if the user wants to support searching for operator and silhouette flags by registration. Note that some registrations are not distinct from operator and ICAO8643 codes.
    //endregion

    VRS.Format = function()
    {
        var that = this;

        /**
         * Formats thumbnails retrieved from www.airport-data.com. Returns an HTML string.
         * @param {VRS_JSON_AIRPORTDATA_THUMBNAILS} airportDataThumbnails       The thumbnails to render.
         * @param {boolean}                         showLinkToSite              True if the thumbnails should be wrapped in a link to www.airport-data.com's page for the aircraft.
         */
        this.airportDataThumbnails = function(airportDataThumbnails, showLinkToSite)
        {
            var result = '';

            if(airportDataThumbnails && airportDataThumbnails.data && airportDataThumbnails.data.length) {
                result += '<div class="thumbnails">';
                var length = airportDataThumbnails.data.length;
                for(var i = 0;i < length;++i) {
                    var thumbnail = airportDataThumbnails.data[i];
                    if(thumbnail && thumbnail.image && thumbnail.link) {
                        var copyrightNotice = thumbnail.photographer ? 'Copyright &copy; ' + thumbnail.photographer : 'Copyright holder unknown';
                        if(showLinkToSite) result += '<a href="' + thumbnail.link + '" target="airport-data">';
                        result += '<img src="' + thumbnail.image + '" alt="' + copyrightNotice + '" title="' + copyrightNotice + '">';
                        if(showLinkToSite) result += '</a>';
                    }
                }
                result += '</div>';
            }

            return result;
        };

        /**
         * Formats the aircraft class as a string.
         * @param {string} aircraftClass
         * @returns {string}
         */
        this.aircraftClass = function(aircraftClass)
        {
            return aircraftClass || '';
        };

        /**
         * Formats the altitude as a string.
         * @param {Number}              altitude            The altitude to format.
         * @param {VRS.AltitudeType}    altitudeType        The type of altitude.
         * @param {boolean}             isOnGround          True if the aircraft is on the ground.
         * @param {VRS.Height}          heightUnit          The VRS.Height unit to use.
         * @param {bool}                distinguishOnGround True if aircraft on the ground are to be shown as 'GND' instead of the altitude.
         * @param {bool}                showUnits           True if units are to be shown.
         * @param {bool}                showType            True if the type is to be shown to the user.
         * @returns {string}
         */
        this.altitude = function(altitude, altitudeType, isOnGround, heightUnit, distinguishOnGround, showUnits, showType)
        {
            /** @type {*} */
            var result = undefined;
            if(distinguishOnGround && isOnGround) result = VRS.$$.GroundAbbreviation;
            else {
                result = VRS.unitConverter.convertHeight(altitude, VRS.Height.Feet, heightUnit);
                if(result || result === 0) result = VRS.stringUtility.formatNumber(result, '0');
                if(showUnits && result) result = VRS.stringUtility.format(VRS.unitConverter.heightUnitAbbreviation(heightUnit), result);
                if(showType && result && altitudeType === VRS.AltitudeType.Geometric) result += ' ' + VRS.$$.GeometricAltitudeIndicator;
            }

            return result ? result.toString() : '';
        };

        /**
         * Formats a pair of altitudes as a string.
         * @param {Number}      firstAltitude
         * @param {boolean}     firstIsOnGround
         * @param {Number}      lastAltitude
         * @param {boolean}     lastIsOnGround
         * @param {VRS.Height}  heightUnit
         * @param {boolean}     distinguishOnGround
         * @param {boolean}     showUnits
         */
        this.altitudeFromTo = function(firstAltitude, firstIsOnGround, lastAltitude, lastIsOnGround, heightUnit, distinguishOnGround, showUnits)
        {
            var first = that.altitude(firstAltitude, VRS.AltitudeType.Barometric, firstIsOnGround, heightUnit, distinguishOnGround, showUnits, false);
            var last  = that.altitude(lastAltitude, VRS.AltitudeType.Barometric, lastIsOnGround, heightUnit, distinguishOnGround, showUnits, false);
            return formatFromTo(first, last, VRS.$$.FromToAltitude);
        };

        /**
         * Formats an altitude type as a string.
         * @param {VRS.AltitudeType} altitudeType
         * @returns {string}
         */
        this.altitudeType = function(altitudeType)
        {
            var result = '';

            if(altitudeType !== undefined) {
                switch(altitudeType) {
                    case VRS.AltitudeType.Barometric:   result = VRS.$$.Barometric; break;
                    case VRS.AltitudeType.Geometric:    result = VRS.$$.Geometric; break;
                    default:                            result = VRS.$$.Unknown; break;
                }
            }

            return result;
        };

        /**
         * Formats the average signal level as a string.
         * @param {number}      avgSignalLevel
         * @returns {string}
         */
        this.averageSignalLevel = function(avgSignalLevel)
        {
            return avgSignalLevel || avgSignalLevel === 0 ? avgSignalLevel.toString() : '';
        };

        /**
         * Formats the bearing from here as a string.
         * @param {number}      bearingFromHere     The bearing from here in degrees
         * @param {bool}        showUnits           True if units are to be shown.
         * @returns {string}
         */
        this.bearingFromHere = function(bearingFromHere, showUnits)
        {
            var result = bearingFromHere;
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, '0.0');
            if(showUnits && result) result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);

            return result ? result.toString() : '';
        };

        /**
         * Formats the bearing from here as an HTML IMG tag.
         * @param {number} bearingFromHere
         * @returns {string}
         */
        this.bearingFromHereImage = function(bearingFromHere)
        {
            var result = '';
            if(bearingFromHere === 0 || bearingFromHere) {
                var size = VRS.globalOptions.aircraftBearingCompassSize;
                result = '<img src="images/Rotate-' + bearingFromHere;
                if(VRS.browserHelper.isHighDpi()) result += '/HiDpi';
                result += '/Compass.png"' +
                    ' width="' + size.width + 'px"' +
                    ' height="' + size.height + 'px"' +
                    ' />';
            }

            return result;
        };

        /**
         * Formats the callsign as a string.
         * @param {string=}     callsign        The callsign to format.
         * @param {boolean=}    callsignSuspect True if the callsign is uncertain.
         * @param {bool}        showUncertainty True if uncertain callsigns are to be flagged.
         * @returns {string}
         */
        this.callsign = function(callsign, callsignSuspect, showUncertainty)
        {
            var result = callsign;
            if(result && result.length > 0 && callsignSuspect) {
                if(showUncertainty && VRS.globalOptions.aircraftFlagUncertainCallsigns) result += '*';
            }

            return result ? result : '';
        };

        /**
         * Formats the certificate of airworthiness category for display.
         * @param {string} cofACategory
         * @returns {string}
         */
        this.certOfACategory = function(cofACategory)
        {
            return cofACategory || '';
        };

        /**
         * Formats the certificate of airworthiness expiry for display.
         * @param {string} cofAExpiry
         * @returns {string}
         */
        this.certOfAExpiry = function(cofAExpiry)
        {
            return cofAExpiry || '';
        };

        /**
         * Formats the count of flights as a string.
         * @param {number}  countFlights    The count of flights to format.
         * @param {string=} format          The number format to use when formatting the count of flights.
         * @returns {string=}
         */
        this.countFlights = function(countFlights, format)
        {
            /** @type {*} */
            var result = countFlights;
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, format || '0');

            return result ? result : '';
        };

        /**
         * Formats the count of messages as a string.
         * @param {number}  countMessages   The count of messages to format.
         * @param {string=} format          The .NET format to use when formatting the count.
         * @returns {string}
         */
        this.countMessages = function(countMessages, format)
        {
            /** @type {*} */
            var result = countMessages;
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, format || 'N0');

            return result ? result : '';
        };

        /**
         * Formats the country as a string.
         * @param {string} country
         * @returns {string}
         */
        this.country = function(country)
        {
            return country ? VRS.$$.translateCountry(country) : '';
        };

        /**
         * Formats the current registration date for display.
         * @param {string} currentRegDate
         * @returns {string}
         */
        this.currentRegistrationDate = function(currentRegDate)
        {
            return currentRegDate || '';
        };

        /**
         * Formats the deregistered date for display.
         * @param {string} deregDate
         * @returns {string}
         */
        this.deregisteredDate = function(deregDate)
        {
            return deregDate || '';
        };

        /**
         * Formats the distance from here as a string.
         * @param {Number}          distanceFromHere    The distance from here in km.
         * @param {VRS.Distance}    distanceUnit        The VRS.Distance unit to use in formatting.
         * @param {bool}            showUnits           True if units are to be shown.
         * @returns {string}
         */
        this.distanceFromHere = function(distanceFromHere, distanceUnit, showUnits)
        {
            /** @type {*} */
            var result = VRS.unitConverter.convertDistance(distanceFromHere, VRS.Distance.Kilometre, distanceUnit);
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, '0.00');
            if(showUnits && result) result = VRS.stringUtility.format(VRS.unitConverter.distanceUnitAbbreviation(distanceUnit), result);

            return result ? result : '';
        };

        /**
         * Formats the duration of a flight, expressed as a number of ticks, into a string.
         * @param {Number}  elapsedTicks
         * @param {boolean} showZeroHours
         * @returns {string}
         */
        this.duration = function(elapsedTicks, showZeroHours)
        {
            var hms = VRS.timeHelper.ticksToHoursMinutesSeconds(elapsedTicks);
            return VRS.$$.formatHoursMinutesSeconds(hms.hours, hms.minutes, hms.seconds, showZeroHours);
        };

        /**
         * Formats the end date for display.
         * @param {Date}    startDate           The start date.
         * @param {Date}    endDate             The end date.
         * @param {boolean} showFullDate        True if the full date format should be used, false if the short date format should be used.
         * @param {boolean} alwaysShowEndDate   True if the date component of the end date is always to be shown. It is suppressed if the start and end dates fall on the same day.
         * @returns {string}
         */
        this.endDateTime = function(startDate, endDate, showFullDate, alwaysShowEndDate)
        {
            return formatStartEndDate(startDate, endDate, showFullDate, false, false, true, alwaysShowEndDate);
        };

        /**
         * Formats the count of engines and engine type as a string.
         * @param {string}          countEngines    The count of engines
         * @param {VRS.EngineType}  engineType      The type of engines
         * @returns {string}
         */
        this.engines = function(countEngines, engineType)
        {
            var result = '';
            if((countEngines || countEngines === 0) && engineType) {
                result = VRS.$$.formatEngines(countEngines, engineType);
            }

            return result;
        };

        /**
         * Returns the date of the first registration.
         * @param {string} firstRegDate
         * @returns {string}
         */
        this.firstRegistrationDate = function(firstRegDate)
        {
            return firstRegDate || '';
        };

        /**
         * Formats the flight level as a string.
         * @param {number}              altitude                The altitude in feet.
         * @param {VRS.AltitudeType}    altitudeType            The type of altitude transmitted by the aircraft.
         * @param {boolean}             isOnGround              True if the aircraft is on the ground.
         * @param {number}              transitionAltitude      The altitude above which flight levels are reported and below which altitudes are reported.
         * @param {VRS.Height}          transitionAltitudeUnit  The VRS.Height unit that the transition altitude is in.
         * @param {VRS.Height}          flightLevelAltitudeUnit The VRS.Height unit to report flight levels with.
         * @param {VRS.Height}          altitudeUnit            The VRS.Height unit to report altitudes with.
         * @param {bool=}               distinguishOnGround     True if aircraft on the ground are to be reported as GND.
         * @param {bool=}               showUnits               True if units are to be shown.
         * @param {bool=}               showType                True if the altitude type is to be shown.
         * @returns {string}
         */
        this.flightLevel = function(altitude, altitudeType, isOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, showType)
        {
            /** @type {*} */
            var result = altitude;
            if(result || result === 0) {
                if(distinguishOnGround && isOnGround) result = VRS.$$.GroundAbbreviation;
                else {
                    var transitionAltitudeFeet = VRS.unitConverter.convertHeight(transitionAltitude, transitionAltitudeUnit, VRS.Height.Feet);
                    if(result < transitionAltitudeFeet) {
                        result = that.altitude(altitude, altitudeType, isOnGround, altitudeUnit, distinguishOnGround, showUnits, showType);
                    } else {
                        result = VRS.unitConverter.convertHeight(result, VRS.Height.Feet, flightLevelAltitudeUnit);
                        result = VRS.stringUtility.format(VRS.$$.FlightLevelAbbreviation, Math.max(0, Math.round(result / 100)));
                        if(showType && result && altitudeType === VRS.AltitudeType.Geometric) result += ' ' + VRS.$$.GeometricAltitudeIndicator;
                    }
                }
            }

            return result ? result : '';
        };

        /**
         * Formats a range of flight levels as a string.
         * @param {number}      firstAltitude           The initial altitude in feet.
         * @param {boolean}     firstIsOnGround         True if the aircraft started on the ground.
         * @param {number}      lastAltitude            The final altitude in feet.
         * @param {boolean}     lastIsOnGround          True if the aircraft ended on the ground.
         * @param {number}      transitionAltitude      The altitude above which flight levels are reported and below which altitudes are reported.
         * @param {VRS.Height}  transitionAltitudeUnit  The VRS.Height unit that the transition altitude is in.
         * @param {VRS.Height}  flightLevelAltitudeUnit The VRS.Height unit to report flight levels with.
         * @param {VRS.Height}  altitudeUnit            The VRS.Height unit to report altitudes with.
         * @param {bool=}       distinguishOnGround     True if aircraft on the ground are to be reported as GND.
         * @param {bool=}       showUnits               True if units are to be shown.
         * @returns {string}
         */
        this.flightLevelFromTo = function(firstAltitude, firstIsOnGround, lastAltitude, lastIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits)
        {
            var first = that.flightLevel(firstAltitude, VRS.AltitudeType.Barometric, firstIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, false);
            var last = that.flightLevel(lastAltitude, VRS.AltitudeType.Barometric, lastIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, false);
            return formatFromTo(first, last, VRS.$$.FromToFlightLevel);
        };

        /**
         * Formats the generic name.
         * @param {string} genericName
         * @returns {string}
         */
        this.genericName = function(genericName)
        {
            return genericName || '';
        };

        /**
         * Formats the alert flag for display.
         * @param {boolean} hadAlert
         * @returns {string}
         */
        this.hadAlert = function(hadAlert)
        {
            return hadAlert === undefined || hadAlert === null ? '' : hadAlert ? VRS.$$.Yes : VRS.$$.No;
        };

        /**
         * Formats the had emergency flag for display.
         * @param {boolean} hadEmergency
         * @returns {string}
         */
        this.hadEmergency = function(hadEmergency)
        {
            return hadEmergency === undefined || hadEmergency === null ? '' : hadEmergency ? VRS.$$.Yes : VRS.$$.No;
        };

        /**
         * Formats the had SPI flag for display.
         * @param {boolean} hadSPI
         * @returns {string}
         */
        this.hadSPI = function(hadSPI)
        {
            return hadSPI === undefined || hadSPI === null ? '' : hadSPI ? VRS.$$.Yes : VRS.$$.No;
        };

        /**
         * Formats the aircraft's heading as a string.
         * @param {number}  heading     The heading to format
         * @param {bool}    showUnit    True if the units are to be shown.
         * @returns {string}
         */
        this.heading = function(heading, showUnit)
        {
            /** @type {*} */
            var result = heading;
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, '0.0');
            if(showUnit && result) result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);

            return result ? result : '';
        };

        /**
         * Formats the aircraft's ICAO as a string.
         * @param {string} icao
         * @returns {string}
         */
        this.icao = function(icao)
        {
            return icao ? icao : '';
        };

        /**
         * Formats the on-ground flag for display.
         * @param {boolean} isOnGround
         * @returns {string}
         */
        this.isOnGround = function(isOnGround)
        {
            return isOnGround === undefined || isOnGround === null ? '' : isOnGround ? VRS.$$.Yes : VRS.$$.No;
        };

        /**
         * Formats the aircraft's military status as a string.
         * @param {boolean} isMilitary
         * @returns {string}
         */
        this.isMilitary = function(isMilitary)
        {
            var result = '';
            if(isMilitary !== undefined) {
                result = isMilitary ? VRS.$$.Military : VRS.$$.Civil;
            }

            return result;
        };

        /**
         * Formats the aircraft's latitude as a string.
         * @param {number}  latitude    The latitude to format.
         * @param {bool}    showUnit    True if units are to be shown.
         * @returns {string}
         */
        this.latitude = function(latitude, showUnit)
        {
            return formatLatitudeLongitude(latitude, showUnit);
        };

        /**
         * Formats the aircraft's longitude as a string.
         * @param {number}  longitude   The longitude to format.
         * @param {bool}    showUnit    True if units are to be shown.
         * @returns {string}
         */
        this.longitude = function(longitude, showUnit)
        {
            return formatLatitudeLongitude(longitude, showUnit);
        };

        /**
         * Formats latitudes and longitudes.
         * @param {Number}  value
         * @param {boolean} showUnit
         * @returns {string}
         */
        function formatLatitudeLongitude(value, showUnit)
        {
            /** @type {*} */
            var result = value;
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, '0.00000');
            if(showUnit && result) result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);

            return result ? result : '';
        }

        /**
         * Formats the manufacturer for display.
         * @param {string} manufacturer
         * @returns {string}
         */
        this.manufacturer = function(manufacturer)
        {
            return manufacturer || '';
        };

        /**
         * Formats the maximum takeoff weight for display.
         * @param {string} mtow
         * @returns {string}
         */
        this.maxTakeoffWeight = function(mtow)
        {
            return mtow || '';
        };

        /**
         * Formats the aircraft's model as a string.
         * @param {string} model   The aircraft model to format.
         * @returns {string}
         */
        this.model = function(model)
        {
            return model || '';
        };

        /**
         * Formats the aircraft ICAO code for its model as a string.
         * @param {string} modelIcao   The aircraft model ICAO to format.
         * @returns {string}
         */
        this.modelIcao = function(modelIcao)
        {
            return modelIcao || '';
        };

        /**
         * Formats the aircraft's ICAO code for its model as an HTML IMG tag for a silhouette image.
         * @param {string} modelIcao    The aircraft model ICAO to format.
         * @param {string} icao         The ICAO for the aircraft.
         * @param {string} registration The registration for the aircraft.
         * @returns {string}
         */
        this.modelIcaoImageHtml = function(modelIcao, icao, registration)
        {
            var codeToUse = buildLogoCodeToUse(modelIcao, icao, registration);
            var size = VRS.globalOptions.aircraftSilhouetteSize;
            var result = '<img src="images/File-' + encodeURIComponent(codeToUse);
            if(VRS.browserHelper.isHighDpi()) result += '/HiDpi';
            result +=  '/Type.png"' +
                ' width="' + size.width.toString() + 'px"' +
                ' height="' + size.height.toString() + 'px"' +
                ' />';

            return result;
        };

        /**
         * Formats the aircraft's ICAO code for its model as a description of engines, wake turbulence and species.
         * @param {string}                      modelIcao
         * @param {string}                      model
         * @param {string}                      countEngines
         * @param {VRS.EngineType}              engineType
         * @param {VRS.Species}                 species
         * @param {VRS.WakeTurbulenceCategory}  wtc
         * @returns {string}
         */
        this.modelIcaoNameAndDetail = function(modelIcao, model, countEngines, engineType, species, wtc)
        {
            var result = modelIcao;
            if(result && result.length > 0) {
                if(model) result += ': ' + model;
                var appendDetail = function(detail) { if(detail && detail.length > 0) result += ' | ' + detail; };
                appendDetail(that.engines(countEngines, engineType));
                appendDetail(that.wakeTurbulenceCat(wtc, true, true));
                appendDetail(that.species(species, true));
            }

            return result;
        };

        /**
         * Formats the Mode-S country as a string.
         * @param {string} modeSCountry
         * @returns {string}
         */
        this.modeSCountry = function(modeSCountry)
        {
            return modeSCountry ? VRS.$$.translateCountry(modeSCountry) : '';
        };

        /**
         * Formats the notes for display.
         * @param {string} notes
         * @returns {string}
         */
        this.notes = function(notes)
        {
            return notes || '';
        };

        /**
         * Formats the aircraft's operator as a string.
         * @param {string} operator    The operator to format.
         * @returns {string}
         */
        this.operator = function(operator)
        {
            return operator ? operator : '';
        };

        /**
         * Formats the aircraft's operator's ICAO code as a string.
         * @param {string} operatorIcao The operator ICAO code to format.
         * @returns {string}
         */
        this.operatorIcao = function(operatorIcao)
        {
            return operatorIcao ? operatorIcao : '';
        };

        /**
         * Formats the aircraft's operator ICAO code and name as a string.
         * @param {string} operator
         * @param {string} operatorIcao
         * @returns {string}
         */
        this.operatorIcaoAndName = function(operator, operatorIcao)
        {
            var result = operatorIcao;
            if(operator) {
                if(!result) result = '';
                else result += ': ';
                result += operator;
            }

            return result;
        };

        /**
         * Formats the aircraft's operator ICAO as an HTML IMG tag to the operator flag image.
         * @param {string} operator
         * @param {string} operatorIcao
         * @param {string} icao
         * @param {string} registration
         * @returns {string}
         */
        this.operatorIcaoImageHtml = function(operator, operatorIcao, icao, registration)
        {
            var codeToUse = buildLogoCodeToUse(operatorIcao, icao, registration);
            var size = VRS.globalOptions.aircraftOperatorFlagSize;
            var result = '<img src="images/File-' + encodeURIComponent(codeToUse);
            if(VRS.browserHelper.isHighDpi()) result += '/HiDpi';
            result += '/OpFlag.png"' +
                ' width="' + size.width.toString() + 'px"' +
                ' height="' + size.height.toString() + 'px"' +
                ' />';

            return result;
        };

        /**
         * Joins together a logo code and the ICAO for operator flag and silhouette logos.
         * @param {string}  logoCode
         * @param {string}  icao
         * @param {string}  registration
         * @returns {string}
         */
        function buildLogoCodeToUse(logoCode, icao, registration)
        {
            var result = '';

            var addCode = function(code) {
                if(code && code.length) {
                    if(result.length) result += '|';
                    result += code;
                }
            };

            addCode(icao);
            if(VRS.globalOptions.aircraftAllowRegistrationFlagOverride) addCode(registration);
            addCode(logoCode);

            return result;
        }

        /**
         * Formats the ownership status for display.
         * @param {string} ownershipStatus
         * @returns {string}
         */
        this.ownershipStatus = function(ownershipStatus)
        {
            return ownershipStatus || '';
        };

        //noinspection JSUnusedLocalSymbols
        /**
         * Returns an HTML IMG tag to a picture of the aircraft.
         * @param {string}                          registration                The registration of the aircraft.
         * @param {string}                          icao                        The ICAO of the aircraft.
         * @param {Number}                          picWidth                    The width of the aircraft picture held by the server.
         * @param {Number}                          picHeight                   The height of the aircraft picture held by the server.
         * @param {VRS_SIZE}                        requestSize                 The size of image required. If height is not supplied then the code calculates it, maintaining aspect ratio. If width and height are supplied then the image gets centred when it is resized. If parameter is null then the original size is used.
         * @param {boolean=}                        allowResizeUp               True if the image should be scaled up if it is too small. Defaults to true.
         * @param {boolean=}                        linkToOriginal              True if the IMG tag should be wrapped in an A tag pointing back to the full-sized picture.
         * @param {VRS_SIZE=}                       blankSize                   The full dimensions of the blank image to request if the aircraft has no picture.
         * @returns {string}
         */
        this.pictureHtml = function(registration, icao, picWidth, picHeight, requestSize, allowResizeUp, linkToOriginal, blankSize)
        {
            var result = '';

            if(!VRS.serverConfig || VRS.serverConfig.picturesEnabled()) {
                if(allowResizeUp === undefined) allowResizeUp = true;
                var hasPicture = icao && picWidth && picHeight;
                var useOriginal = !requestSize;
                var keepAspectRatio = !!(!useOriginal && requestSize.width && requestSize.height);
                var isHighDpi = VRS.browserHelper.isHighDpi();

                var filePortion = 'images/File-' + encodeURIComponent(registration ? registration : '') + ' ' + encodeURIComponent(icao ? icao : '');
                var sizes = calculatedPictureSizes(isHighDpi, picWidth, picHeight, requestSize, blankSize, allowResizeUp);
                if(hasPicture) {
                    result = '<img src="' + filePortion + '/Size-' + (keepAspectRatio ? VRS.AircraftPictureServerSize.List           // Forces a resize that ignores aspect ratio and keeps image centred during resize
                                                                   :  useOriginal ?     VRS.AircraftPictureServerSize.Original       // Required if we want to avoid a resize on the server
                                                                                  :     VRS.AircraftPictureServerSize.IPadDetail);   // Can be any setting except desktop detail, which won't resize upwards. We've already dealt with protecting against a resize up.
                    if(sizes.requestSize) result += '/Wdth-' + sizes.requestSize.width + '/Hght-' + sizes.requestSize.height;
                    result += '/Picture.png"';
                } else if(blankSize) {
                    result = '<img src="images/Wdth-' + blankSize.width + '/Hght-' + blankSize.height + '/Blank.png" ';
                }
                if(result) {
                    if(sizes.tagSize) result += 'width="' + sizes.tagSize.width + 'px" height="' + sizes.tagSize.height + 'px" ';
                    result += ' />';

                    if(linkToOriginal && hasPicture) result = '<a href="' + filePortion + '/Size-Full/Picture.png" target="picture">' + result + '</a>';
                }
            }

            return result;
        };

        /**
         * Calculates the size of image to request from the server and the size of image to use in the tag.
         * @param {boolean}     isHighDpi
         * @param {Number}      picWidth
         * @param {Number}      picHeight
         * @param {VRS_SIZE}    requestSize
         * @param {VRS_SIZE}    blankSize
         * @param {boolean}     allowResizeUp
         * @returns {{tagSize: VRS_SIZE, requestSize: VRS_SIZE= }}
         */
        function calculatedPictureSizes(isHighDpi, picWidth, picHeight, requestSize, blankSize, allowResizeUp)
        {
            var result = {
                tagSize: blankSize
            };

            if(picWidth !== -1 && picHeight !== -1 && requestSize) {
                var width = requestSize.width;
                var height = requestSize.height;

                if(!allowResizeUp && width > picWidth) width = picWidth;

                if(!height) height = Math.floor(((width / (picWidth / picHeight)) + 0.5));
                result.tagSize = { width: width, height: height };
                if(isHighDpi) {
                    width *= 2;
                    height *= 2;
                }
                result.requestSize = { width: width, height: height };
            }

            return result;
        }

        /**
         * Formats the popular name for display.
         * @param {string} popularName
         * @returns {string}
         */
        this.popularName = function(popularName)
        {
            return popularName || '';
        };

        /**
         * Formats the previous ID for display.
         * @param {string} previousId
         * @returns {string}
         */
        this.previousId = function(previousId)
        {
            return previousId || '';
        };

        /**
         * Returns the formatted name of the receiver that last picked up a message for this aircraft.
         * @param {number}                  receiverId          The receiver identifier.
         * @param {VRS.AircraftListFetcher} aircraftListFetcher The fetcher that holds the list of receivers.
         * @returns {string}
         */
        this.receiver = function(receiverId, aircraftListFetcher)
        {
            var result = '';
            if(aircraftListFetcher) {
                var feed = aircraftListFetcher.getFeed(receiverId);
                if(feed) result = feed.name;
            }

            return result;
        };

        /**
         * Returns the registration formatted as a string.
         * @param {string}      registration        The registration to format.
         * @param {boolean=}    onlyAlphaNumeric    True if non-alphanumeric characters in the registration are to be stripped out.
         * @returns {string=}
         */
        this.registration = function(registration, onlyAlphaNumeric)
        {
            var result = registration ? registration : '';
            if(!!onlyAlphaNumeric) {
                result = VRS.stringUtility.filter(result, function(ch) {
                    return (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z');
                });
            }

            return result;
        };

        /**
         * Given a route consisting of an airport code and name this returns the airport code.
         * @param {string} route
         * @returns {string}
         */
        this.routeAirportCode = function(route)
        {
            var result = route;
            if(result && result.length) {
                var separator = result.indexOf(' ');
                if(separator !== -1) result = result.substr(0, separator);
            }

            return result;
        };

        /**
         * Returns the full route, including all stopovers and airport names, formatted as a string.
         * @param {string}          callsign    The callsign for the aircraft.
         * @param {string}          from        The origin airport details.
         * @param {string}          to          The destination airport details.
         * @param {string[]}        via         The stopover airports.
         * @returns {string}
         */
        this.routeFull = function(callsign, from, to, via)
        {
            var result = '';
            if(callsign) {
                if(!from || !to) result = VRS.$$.RouteNotKnown;
                else result = VRS.$$.formatRoute(from, to, via);
            }

            return result;
        };

        /**
         * Returns the full route, including all stopovers and airport names, formatted as a string.
         * @param {string}                  callsign
         * @param {VRS_JSON_REPORT_ROUTE}   route
         * @returns {string}
         */
        this.reportRouteFull = function(callsign, route)
        {
            var sroute = extractReportRouteStrings(route);
            return that.routeMultiLine(callsign, sroute.from, sroute.to, sroute.via);
        };

        /**
         * Returns HTML for the full route spread over multiple lines (separated by BR tags).
         * @param {string}          callsign                    The callsign for the aircraft.
         * @param {string}          from                        The origin airport details.
         * @param {string}          to                          The destination airport details.
         * @param {string[]}        via                         The stopover airports.
         * @returns {string}
         */
        this.routeMultiLine = function(callsign, from, to, via)
        {
            var result = '';
            if(!callsign) result = VRS.$$.AircraftNotTransmittingCallsign;
            else {
                if(!from || !to) result = VRS.$$.RouteNotKnown;
                else {
                    result += VRS.stringUtility.htmlEscape(from);
                    var length = via.length;
                    if(length) {
                        for(var i = 0;i < length;++i) {
                            result += '<br />';
                            result += VRS.stringUtility.htmlEscape(via[i]);
                        }
                    }
                    result += '<br />';
                    result += VRS.stringUtility.htmlEscape(to);
                }
            }

            return result;
        };

        /**
         * Returns HTML for the full route spread over multiple lines (separated by BR tags).
         * @param {string}                  callsign
         * @param {VRS_JSON_REPORT_ROUTE}   route
         * @returns {string}
         */
        this.reportRouteMultiLine = function(callsign, route)
        {
            var sroute = extractReportRouteStrings(route);
            return that.routeMultiLine(callsign, sroute.from, sroute.to, sroute.via);
        };

        /**
         * Returns HTML for the short route (where only airport codes are shown and stopovers can be reduced to an asterisk).
         * @param {string}          callsign                    The callsign for the aircraft.
         * @param {string}          from                        The origin airport details.
         * @param {string}          to                          The destination airport details.
         * @param {string[]}        via                         The stopover airports.
         * @param {boolean=}        abbreviateStopovers         True if stopovers are to be shown as a single *. Defaults to true.
         * @param {boolean=}        showRouteNotKnown           True if an unknown route is to be shown as 'Route not known'. Defaults to false.
         * @returns {string}
         */
        this.routeShort = function(callsign, from, to, via, abbreviateStopovers, showRouteNotKnown)
        {
            if(abbreviateStopovers === undefined) abbreviateStopovers = true;

            var result = '';
            if(callsign) {
                var length = via.length;
                var showCircularRoute = from && to && from === to && abbreviateStopovers && length;

                if(from) result += that.routeAirportCode(from);
                if(length) {
                    if(abbreviateStopovers) {
                        if(!showCircularRoute || length > 1) result += '-*';
                        if(showCircularRoute) result += '-' + that.routeAirportCode(via[length - 1]);
                    } else {
                        for(var i = 0;i < length;++i) {
                            result += '-' + that.routeAirportCode(via[i]);
                        }
                    }
                }
                if(to) {
                    if(!showCircularRoute) result += '-' + that.routeAirportCode(to);
                }
                if(showCircularRoute) result += ' ∞';

                if(!result) result = showRouteNotKnown ? VRS.$$.RouteNotKnown : '';
            }

            return result;
        };

        /**
         * Returns HTML for the short route (where only airport codes are shown and stopovers can be reduced to an asterisk).
         * @param {string}                  callsign
         * @param {VRS_JSON_REPORT_ROUTE}   route
         * @param {boolean}                 abbreviateStopovers
         * @param {boolean}                 showRouteNotKnown
         * @returns {string}
         */
        this.reportRouteShort = function(callsign, route, abbreviateStopovers, showRouteNotKnown)
        {
            var sroute = extractReportRouteStrings(route);
            return that.routeShort(callsign, sroute.from, sroute.to, sroute.via, abbreviateStopovers, showRouteNotKnown);
        };

        /**
         * Formats the aircraft's serial number.
         * @param {string} serial
         * @returns {string}
         */
        this.serial = function(serial)
        {
            return serial || '';
        };

        /**
         * Formats the start date/time for display.
         * @param {Date}        startDate
         * @param {boolean}     showFullDate
         * @param {boolean}     justShowTime
         * @returns {string}
         */
        this.startDateTime = function(startDate, showFullDate, justShowTime)
        {
            return formatStartEndDate(startDate, undefined, showFullDate, true, justShowTime, false, false);
        };

        /**
         * Formats the aircraft's current status (in service, mothballed etc.)
         * @param {string} status
         * @returns {string}
         */
        this.status = function(status)
        {
            return status || '';
        };

        /**
         * Formats the seconds tracks as a string.
         * @param {number} secondsTracked   The number of seconds tracked to format.
         * @returns {string}
         */
        this.secondsTracked = function(secondsTracked)
        {
            var hms = VRS.timeHelper.secondsToHoursMinutesSeconds(secondsTracked);
            return VRS.$$.formatHoursMinutesSeconds(hms.hours, hms.minutes, hms.seconds);
        };

        /**
         * Formats the signal level as a string.
         * @param {number} signalLevel  The signal level to format.
         * @returns {string}
         */
        this.signalLevel = function(signalLevel)
        {
            return signalLevel === undefined ? '' : signalLevel.toString();
        };

        /**
         * Formats the aircraft species as a string.
         * @param {VRS.Species} species     The species to format.
         * @param {bool}        ignoreNone  True if an empty string is to be returned when the species is unknown instead of some warning text.
         * @returns {string}
         */
        this.species = function(species, ignoreNone)
        {
            if(!species && species !== 0) return '';
            switch(species) {
                case VRS.Species.None:          return ignoreNone ? '' : VRS.$$.None;
                case VRS.Species.Amphibian:     return VRS.$$.Amphibian;
                case VRS.Species.Gyrocopter:    return VRS.$$.Gyrocopter;
                case VRS.Species.Helicopter:    return VRS.$$.Helicopter;
                case VRS.Species.LandPlane:     return VRS.$$.LandPlane;
                case VRS.Species.SeaPlane:      return VRS.$$.SeaPlane;
                case VRS.Species.Tiltwing:      return VRS.$$.Tiltwing;
                case VRS.Species.GroundVehicle: return VRS.$$.GroundVehicle;
                case VRS.Species.Tower:         return VRS.$$.RadioMast;
                default:                        throw 'Unknown species type ' + species;
            }
        };

        /**
         * Returns the speed formatted as a string.
         * @param {number}      speed       The speed in knots to format.
         * @param {VRS.Speed}   speedUnit   The VRS.Speed unit to use when formatting the speed.
         * @param {bool}        showUnit    True if the units are to be shown.
         * @returns {string}
         */
        this.speed = function(speed, speedUnit, showUnit)
        {
            /** @type {*} */
            var result = VRS.unitConverter.convertSpeed(speed, VRS.Speed.Knots, speedUnit);
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, '0.0');
            if(showUnit && result) result = VRS.stringUtility.format(VRS.unitConverter.speedUnitAbbreviation(speedUnit), result);

            return result ? result : '';
        };

        /**
         * Returns the first and last speeds formatted as a string.
         * @param {Number}      fromSpeed
         * @param {Number}      toSpeed
         * @param {VRS.Speed}   speedUnit
         * @param {boolean}     showUnits
         * @returns {string}
         */
        this.speedFromTo = function(fromSpeed, toSpeed, speedUnit, showUnits)
        {
            var first = that.speed(fromSpeed, speedUnit, showUnits);
            var last  = that.speed(toSpeed, speedUnit, showUnits);
            return formatFromTo(first, last, VRS.$$.FromToSpeed);
        };

        /**
         * Returns the squawk formatted as a string.
         * @param {string|number} squawk    The squawk to format.
         * @returns {string}
         */
        this.squawk = function(squawk)
        {
            if(squawk === null || squawk === undefined) return '';
            if(typeof squawk === 'string') return squawk;
            return VRS.stringUtility.formatNumber(squawk, 4);
        };

        /**
         * Returns the pair of squawks formatted as a string.
         * @param {Number|String} fromSquawk
         * @param {Number|String} toSquawk
         */
        this.squawkFromTo = function(fromSquawk, toSquawk)
        {
            var first = that.squawk(fromSquawk);
            var last = that.squawk(toSquawk);
            return formatFromTo(first, last, VRS.$$.FromToSquawk);
        };

        /**
         * Returns an HTML string that displays two text values stacked one on top of the other.
         * @param {string}      topValue            The top value to show.
         * @param {string}      bottomValue         The bottom value to show.
         * @param {string}     [tag]                The tag to wrap each value with - defaults to 'p'.
         * @returns {string}
         */
        this.stackedValues = function(topValue, bottomValue, tag)
        {
            var startTag = !tag ? '<p>' : '<' + tag + '>';
            var endTag = !tag ? '</p>' : '</' + tag + '>';
            return startTag + (VRS.stringUtility.htmlEscape(topValue) || '&nbsp;') + endTag +
                   startTag + (VRS.stringUtility.htmlEscape(bottomValue) || '&nbsp;') + endTag;
        };

        /**
         * Formats the total hours for display.
         * @param {string} totalHours
         * @returns {string}
         */
        this.totalHours = function(totalHours)
        {
            return totalHours || '';
        };

        /**
         * Returns the interested flag formatted as a string.
         * @param {boolean} userInterested
         * @returns {string}
         */
        this.userInterested = function(userInterested)
        {
            return userInterested === undefined || userInterested === null ? '' : userInterested ? VRS.$$.Yes : VRS.$$.No;
        };

        /**
         * Returns the user tag formatted as a string.
         * @param {string} userTag
         * @returns {string}
         */
        this.userTag = function(userTag)
        {
            return userTag || '';
        };

        /**
         * Returns the vertical speed formatted as a string.
         * @param {number}      verticalSpeed   The VSI to format.
         * @param {VRS.Height}  heightUnit      The VRS.Height unit to use when formatting.
         * @param {bool}        perSecond       True if the VSI is /second, false if it's /minute.
         * @param {bool}        showUnit        True if units are to be shown.
         * @returns {string}
         */
        this.verticalSpeed = function(verticalSpeed, heightUnit, perSecond, showUnit)
        {
            /** @type {*} */
            var result = VRS.unitConverter.convertVerticalSpeed(verticalSpeed, VRS.Height.Feet, heightUnit, perSecond);
            if(result || result === 0) result = VRS.stringUtility.formatNumber(result, '0');
            if(showUnit && result)     result = VRS.stringUtility.format(VRS.unitConverter.heightUnitOverTimeAbbreviation(heightUnit, perSecond), result);

            return result ? result : '';
        };

        /**
         * Returns the wake turbulence category formatted as a string.
         * @param {VRS.WakeTurbulenceCategory}  wtc                     The wake turbulence category to format.
         * @param {bool}                        ignoreNone              True if an empty string is to be returned instead of warning text when the WTC is unknown.
         * @param {bool}                        expandedDescription     True if the rough weight limits are to be shown.
         * @returns {string}
         */
        this.wakeTurbulenceCat = function(wtc, ignoreNone, expandedDescription)
        {
            var result = '';
            if(wtc || wtc === 0) {
                result = VRS.$$.formatWakeTurbulenceCategory(wtc, ignoreNone, expandedDescription);
            }

            return result;
        };

        /**
         * Formats the year built for display.
         * @param {string} yearBuilt
         * @returns {string}
         */
        this.yearBuilt = function(yearBuilt)
        {
            return yearBuilt || '';
        };

        //region extractReportRouteStrings, formatReportAirport
        /**
         * Returns an object that contains the route information from a report in strings.
         * @param {VRS_JSON_REPORT_ROUTE} route
         * @returns {{
         *      from: string,
         *      to:   string,
         *      via:  string[]
         * }}
         */
        function extractReportRouteStrings(route)
        {
            var via = [];
            if(route && route.via) {
                var length = route.via.length;
                for(var i = 0;i < length;++i) {
                    via.push(formatReportAirport(route.via[i]));
                }
            }

            return {
                from: route ? formatReportAirport(route.from) : null,
                to:   route ? formatReportAirport(route.to) : null,
                via:  via
            };
        }

        /**
         * Joins the code and name from an airport into a single string.
         * @param {VRS_JSON_REPORT_AIRPORT} airport
         * @returns {string}
         */
        function formatReportAirport(airport)
        {
            return airport ? airport.fullName : '';
        }
        //endregion

        //region formatStartEndDate
        /**
         * Formats the start and/or end date for display. If the end date is being shown and it has the same date as the
         * start date then, by default, only its time is shown. The start date includes both date and time but its date
         * portion can be suppressed.
         * @param {Date}       [startDate]                  The start date to display, if any.
         * @param {Date}       [endDate]                    The end date to display, if any.
         * @param {boolean}     showFullDates               True if the localised full date format should be used instead of the localised short date format.
         * @param {boolean}     showStartDate               True if the start should be shown.
         * @param {boolean}     onlyShowStartTime           True if only the time portion of the start date should be shown.
         * @param {boolean}     showEndDate                 True if the end date should be shown.
         * @param {boolean}     alwaysShowEndDate           True if the end date should be shown even if it's the same date as the start date. If false then only the end time is shown.
         */
        function formatStartEndDate(startDate, endDate, showFullDates, showStartDate, onlyShowStartTime, showEndDate, alwaysShowEndDate)
        {
            var result = '';

            var startDateText = '';
            if(startDate && showStartDate) {
                if(onlyShowStartTime) {
                    startDateText = Globalize.format(startDate, 'T');
                } else {
                    if(showFullDates) startDateText = Globalize.format(startDate, 'F');
                    else {
                        startDateText = VRS.stringUtility.format(
                            VRS.$$.DateTimeShort,
                            Globalize.format(startDate, 'd'),
                            Globalize.format(startDate, 'T')
                        );
                    }
                }
            }

            var endDateText = '';
            if(endDate && showEndDate) {
                var suppressDate = !alwaysShowEndDate && startDate && (VRS.dateHelper.getDatePortion(startDate) === VRS.dateHelper.getDatePortion(endDate));
                if(suppressDate) {
                    endDateText = Globalize.format(endDate, 'T');
                } else {
                    if(showFullDates) endDateText = Globalize.format(endDate, 'F');
                    else {
                        endDateText = VRS.stringUtility.format(
                            VRS.$$.DateTimeShort,
                            Globalize.format(endDate, 'd'),
                            Globalize.format(endDate, 'T')
                        );
                    }
                }
            }

            return formatFromTo(startDateText, endDateText, VRS.$$.FromToDate);
        }
        //endregion

        //region formatFromTo
        /**
         * Returns the first and last strings joined together with a format. If either string is empty then the other string
         * is returned without any extra formatting. If both strings are empty then an empty string is returned.
         * @param {string}  first           The first string to join.
         * @param {string}  last            The last string to join.
         * @param {string}  fromToFormat    The format to use if both first and last are not empty (e.g. '{0} to {1}').
         * @returns {string}
         */
        function formatFromTo(first, last, fromToFormat)
        {
            if(first === last)  last = null;
            if(first && last)   return VRS.stringUtility.format(fromToFormat, first, last);
            else if(first)      return first;
            else                return last || '';
        }
        //endregion
    };

    VRS.format = new VRS.Format();
}(window.VRS = window.VRS || {}, jQuery));