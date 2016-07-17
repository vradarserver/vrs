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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftOperatorFlagSize = VRS.globalOptions.aircraftOperatorFlagSize || { width: 85, height: 20 };       // The dimensions of operator flags
    VRS.globalOptions.aircraftSilhouetteSize = VRS.globalOptions.aircraftSilhouetteSize || { width: 85, height: 20 };           // The dimensions of silhouette flags
    VRS.globalOptions.aircraftBearingCompassSize = VRS.globalOptions.aircraftBearingCompassSize || { width: 16, height: 16 };   // The dimensions of the bearing compass image.
    VRS.globalOptions.aircraftTransponderTypeSize = VRS.globalOptions.aircraftTransponderTypeSize || { width: 20, height: 20 }; // The dimensions of the transponder type normal-size images.
    VRS.globalOptions.aircraftFlagUncertainCallsigns = VRS.globalOptions.aircraftFlagUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftFlagUncertainCallsigns : true;    // True if callsigns that we're not 100% sure about are to be shown with an asterisk against them.
    VRS.globalOptions.aircraftAllowRegistrationFlagOverride = VRS.globalOptions.aircraftAllowRegistrationFlagOverride !== undefined ? VRS.globalOptions.aircraftAllowRegistrationFlagOverride : false;  // True if the user wants to support searching for operator and silhouette flags by registration. Note that some registrations are not distinct from operator and ICAO8643 codes.

    interface ICalculatedPictureSize
    {
        tagSize:        ISize;
        requestSize?:   ISize;
    }

    interface IRoute
    {
        from:   string;
        to:     string;
        via:    string[];
    }

    export class Format
    {
        /**
         * Formats thumbnails retrieved from www.airport-data.com. Returns an HTML string.
         */
        airportDataThumbnails(airportDataThumbnails: IAirportDataThumbnails, showLinkToSite: boolean) : string
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
        }

        /**
         * Formats the aircraft class as a string.
         */
        aircraftClass(aircraftClass: string) : string
        {
            return aircraftClass || '';
        }

        /**
         * Formats the altitude as a string.
         */
        altitude(altitude: number, altitudeType: AltitudeTypeEnum, isOnGround: boolean, heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean) : string
        {
            var result = '';
            if(distinguishOnGround && isOnGround) result = VRS.$$.GroundAbbreviation;
            else {
                altitude = VRS.unitConverter.convertHeight(altitude, VRS.Height.Feet, heightUnit);
                var hasAltitude = altitude || altitude === 0;
                if(hasAltitude) {
                    result = VRS.stringUtility.formatNumber(altitude, '0');
                    if(showUnits) result = VRS.stringUtility.format(VRS.unitConverter.heightUnitAbbreviation(heightUnit), result);
                    if(showType && altitudeType === VRS.AltitudeType.Geometric) result += ' ' + VRS.$$.GeometricAltitudeIndicator;
                }
            }

            return result;
        }

        /**
         * Formats a pair of altitudes as a string.
         */
        altitudeFromTo(firstAltitude: number, firstIsOnGround: boolean, lastAltitude: number, lastIsOnGround: boolean, heightUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean) : string
        {
            var first = this.altitude(firstAltitude, VRS.AltitudeType.Barometric, firstIsOnGround, heightUnit, distinguishOnGround, showUnits, false);
            var last  = this.altitude(lastAltitude, VRS.AltitudeType.Barometric, lastIsOnGround, heightUnit, distinguishOnGround, showUnits, false);
            return this.formatFromTo(first, last, VRS.$$.FromToAltitude);
        }

        /**
         * Formats an altitude type as a string.
         */
        altitudeType(altitudeType: AltitudeTypeEnum) : string
        {
            return this.formatAltitudeType(altitudeType);
        }

        /**
         * A worker function that translates an altitude type into a string.
         */
        private formatAltitudeType(altitudeType: AltitudeTypeEnum) : string
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
        }

        /**
         * Formats the average signal level as a string.
         */
        averageSignalLevel(avgSignalLevel: number) : string
        {
            return avgSignalLevel || avgSignalLevel === 0 ? avgSignalLevel.toString() : '';
        }

        /**
         * Formats the bearing from here as a string.
         */
        bearingFromHere(bearingFromHere: number, showUnits: boolean) : string
        {
            var result = '';
            if(bearingFromHere || bearingFromHere === 0) {
                result = VRS.stringUtility.formatNumber(bearingFromHere, '0.0');
                if(showUnits) result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);
            }

            return result;
        }

        /**
         * Formats the bearing from here as an HTML IMG tag.
         */
        bearingFromHereImage(bearingFromHere: number) : string
        {
            var result = '';
            if(bearingFromHere || bearingFromHere === 0) {
                var size = VRS.globalOptions.aircraftBearingCompassSize;
                result = '<img src="images/Rotate-' + bearingFromHere;
                if(VRS.browserHelper.isHighDpi()) result += '/HiDpi';
                result += '/Compass.png"' +
                    ' width="' + size.width + 'px"' +
                    ' height="' + size.height + 'px"' +
                    ' />';
            }

            return result;
        }

        /**
         * Formats the callsign as a string.
         */
        callsign(callsign: string, callsignSuspect: boolean, showUncertainty: boolean) : string
        {
            var result = callsign || '';
            if(result.length > 0 && callsignSuspect) {
                if(showUncertainty && VRS.globalOptions.aircraftFlagUncertainCallsigns) result += '*';
            }

            return result;
        }

        /**
         * Formats the certificate of airworthiness category for display.
         */
        certOfACategory(cofACategory: string) : string
        {
            return cofACategory || '';
        }

        /**
         * Formats the certificate of airworthiness expiry for display.
         */
        certOfAExpiry(cofAExpiry: string) : string
        {
            return cofAExpiry || '';
        }

        /**
         * Formats the count of flights as a string.
         */
        countFlights(countFlights: number, format?: string) : string
        {
            var result = '';
            if(countFlights || countFlights === 0) {
                result = VRS.stringUtility.formatNumber(countFlights, format || 'N0');
            }

            return result;
        }

        /**
         * Formats the count of messages as a string.
         */
        countMessages(countMessages: number, format?: string) : string
        {
            var result = '';
            if(countMessages || countMessages === 0) {
                result = VRS.stringUtility.formatNumber(countMessages, format || 'N0');
            }

            return result;
        }

        /**
         * Formats the country as a string.
         */
        country(country: string) : string
        {
            return country ? VRS.$$.translateCountry(country) : '';
        }

        /**
         * Formats the current registration date for display.
         */
        currentRegistrationDate(currentRegDate: string) : string
        {
            return currentRegDate || '';
        }

        /**
         * Formats the deregistered date for display.
         */
        deregisteredDate(deregDate: string) : string
        {
            return deregDate || '';
        }

        /**
         * Formats the distance from here as a string.
         */
        distanceFromHere(distanceFromHere: number, distanceUnit: DistanceEnum, showUnits: boolean) : string
        {
            var result = '';
            distanceFromHere = VRS.unitConverter.convertDistance(distanceFromHere, VRS.Distance.Kilometre, distanceUnit);
            if(distanceFromHere || distanceFromHere === 0) {
                result = VRS.stringUtility.formatNumber(distanceFromHere, '0.00');
                if(showUnits) result = VRS.stringUtility.format(VRS.unitConverter.distanceUnitAbbreviation(distanceUnit), result);
            }

            return result;
        }

        /**
         * Formats the duration of a flight, expressed as a number of ticks, into a string.
         */
        duration(elapsedTicks: number, showZeroHours: boolean) : string
        {
            var hms = VRS.timeHelper.ticksToHoursMinutesSeconds(elapsedTicks);
            return VRS.$$.formatHoursMinutesSeconds(hms.hours, hms.minutes, hms.seconds, showZeroHours);
        }

        /**
         * Formats the end date for display.
         * @param {boolean} showFullDate        True if the full date format should be used, false if the short date format should be used.
         * @param {boolean} alwaysShowEndDate   True if the date component of the end date is always to be shown. It is suppressed if the start and end dates fall on the same day.
         */
        endDateTime(startDate: Date, endDate: Date, showFullDate: boolean, alwaysShowEndDate: boolean) : string
        {
            return this.formatStartEndDate(startDate, endDate, showFullDate, false, false, true, alwaysShowEndDate);
        }

        /**
         * Formats the count of engines and engine type as a string.
         */
        engines(countEngines: string, engineType: EngineTypeEnum) : string
        {
            var result = '';
            if(countEngines && engineType) {
                result = VRS.$$.formatEngines(countEngines, engineType);
            }

            return result;
        }

        /**
         * Returns the date of the first registration.
         */
        firstRegistrationDate(firstRegDate: string) : string
        {
            return firstRegDate || '';
        }

        /**
         * Formats the flight level as a string.
         */
        flightLevel(pressureAltitude: number, geometricAltitude: number, altitudeType: AltitudeTypeEnum, isOnGround: boolean, transitionAltitude: number, transitionAltitudeUnit: HeightEnum, flightLevelAltitudeUnit: HeightEnum, altitudeUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean, showType: boolean) : string
        {
            var result = '';
            if(pressureAltitude || pressureAltitude === 0) {
                if(distinguishOnGround && isOnGround) result = VRS.$$.GroundAbbreviation;
                else {
                    var transitionAltitudeFeet = VRS.unitConverter.convertHeight(transitionAltitude, transitionAltitudeUnit, VRS.Height.Feet);
                    if(geometricAltitude < transitionAltitudeFeet) {
                        result = this.altitude(geometricAltitude, VRS.AltitudeType.Geometric, isOnGround, altitudeUnit, distinguishOnGround, showUnits, showType);
                    } else {
                        pressureAltitude = VRS.unitConverter.convertHeight(pressureAltitude, VRS.Height.Feet, flightLevelAltitudeUnit);
                        result = VRS.stringUtility.format(VRS.$$.FlightLevelAbbreviation, Math.max(0, Math.round(pressureAltitude / 100)));
                    }
                }
            }

            return result;
        }

        /**
         * Formats a range of flight levels as a string.
         * @returns {string}
         */
        flightLevelFromTo(firstPressureAltitude: number, firstIsOnGround: boolean, lastPressureAltitude: number, lastIsOnGround: boolean, transitionAltitude: number, transitionAltitudeUnit: HeightEnum, flightLevelAltitudeUnit: HeightEnum, altitudeUnit: HeightEnum, distinguishOnGround: boolean, showUnits: boolean) : string
        {
            var first = this.flightLevel(firstPressureAltitude, firstPressureAltitude, VRS.AltitudeType.Barometric, firstIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, false);
            var last = this.flightLevel(lastPressureAltitude, lastPressureAltitude, VRS.AltitudeType.Barometric, lastIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, false);
            return this.formatFromTo(first, last, VRS.$$.FromToFlightLevel);
        }

        /**
         * Formats the generic name.
         */
        genericName(genericName: string) : string
        {
            return genericName || '';
        }

        /**
         * Formats the alert flag for display.
         */
        hadAlert(hadAlert: boolean) : string
        {
            return hadAlert === undefined || hadAlert === null ? '' : hadAlert ? VRS.$$.Yes : VRS.$$.No;
        }

        /**
         * Formats the had emergency flag for display.
         */
        hadEmergency(hadEmergency: boolean) : string
        {
            return hadEmergency === undefined || hadEmergency === null ? '' : hadEmergency ? VRS.$$.Yes : VRS.$$.No;
        }

        /**
         * Formats the had SPI flag for display.
         */
        hadSPI(hadSPI: boolean) : string
        {
            return hadSPI === undefined || hadSPI === null ? '' : hadSPI ? VRS.$$.Yes : VRS.$$.No;
        }

        /**
         * Formats the aircraft's heading as a string.
         */
        heading(heading: number, headingIsTrue: boolean, showUnit: boolean, showType: boolean) : string
        {
            var result = ''
            if(heading || heading === 0) {
                result = VRS.stringUtility.formatNumber(heading, '0.0');
                if(showUnit) result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);
                if(showType && headingIsTrue) result += ' ' + VRS.$$.TrueHeadingShort;
            }

            return result;
        }

        /**
         * Formats the heading type as a string.
         */
        headingType(headingIsTrue: boolean) : string
        {
            return headingIsTrue ? VRS.$$.TrueHeading : VRS.$$.GroundTrack;
        }

        /**
         * Formats the aircraft's ICAO as a string.
         */
        icao(icao: string) : string
        {
            return icao ? icao : '';
        }

        /**
         * Formats the is-MLAT flag for display.
         */
        isMlat(isMlat: boolean) : string
        {
            return isMlat === undefined || isMlat === null ? '' : isMlat ? VRS.$$.Yes : VRS.$$.No;
        }

        /**
         * Formats the is Tis-B flag for display.
         */
        isTisb(isTisb: boolean) : string
        {
            return isTisb === undefined || isTisb === null ? '' : isTisb ? VRS.$$.Yes : VRS.$$.No;
        }

        /**
         * Formats the on-ground flag for display.
         */
        isOnGround(isOnGround: boolean) : string
        {
            return isOnGround === undefined || isOnGround === null ? '' : isOnGround ? VRS.$$.Yes : VRS.$$.No;
        }

        /**
         * Formats the aircraft's military status as a string.
         */
        isMilitary(isMilitary: boolean) : string
        {
            return isMilitary === undefined || isMilitary === null ? '' : isMilitary ? VRS.$$.Military : VRS.$$.Civil;
        }

        /**
         * Formats the aircraft's latitude as a string.
         */
        latitude(latitude: number, showUnit: boolean) : string
        {
            return this.formatLatitudeLongitude(latitude, showUnit);
        }

        /**
         * Formats the aircraft's longitude as a string.
         */
        longitude(longitude: number, showUnit: boolean) : string
        {
            return this.formatLatitudeLongitude(longitude, showUnit);
        }

        /**
         * Formats latitudes and longitudes.
         */
        private formatLatitudeLongitude(value: number, showUnit: boolean) : string
        {
            var result = '';
            if(value || value === 0) {
                result = VRS.stringUtility.formatNumber(value, '0.00000');
                if(showUnit) result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);
            }

            return result;
        }

        /**
         * Formats the manufacturer for display.
         */
        manufacturer(manufacturer: string) : string
        {
            return manufacturer || '';
        }

        /**
         * Formats the maximum takeoff weight for display.
         */
        maxTakeoffWeight(mtow: string) : string
        {
            return mtow || '';
        }

        /**
         * Formats the aircraft's model as a string.
         */
        model(model: string) : string
        {
            return model || '';
        }

        /**
         * Formats the aircraft ICAO code for its model as a string.
         */
        modelIcao(modelIcao: string) : string
        {
            return modelIcao || '';
        }

        /**
         * Formats the aircraft's ICAO code for its model as an HTML IMG tag for a silhouette image.
         */
        modelIcaoImageHtml(modelIcao: string, icao: string, registration: string) : string
        {
            var codeToUse = this.buildLogoCodeToUse(modelIcao, icao, registration);
            var size = VRS.globalOptions.aircraftSilhouetteSize;
            var result = '<img src="images/File-' + encodeURIComponent(codeToUse);
            if(VRS.browserHelper.isHighDpi()) result += '/HiDpi';
            result +=  '/Type.png"' +
                ' width="' + size.width.toString() + 'px"' +
                ' height="' + size.height.toString() + 'px"' +
                ' />';

            return result;
        }

        /**
         * Formats the aircraft's ICAO code for its model as a description of engines, wake turbulence and species.
         */
        modelIcaoNameAndDetail(modelIcao: string, model: string, countEngines: string, engineType: EngineTypeEnum, species: number, wtc: number) : string
        {
            var result = modelIcao;
            if(result && result.length > 0) {
                if(model) result += ': ' + model;
                var appendDetail = function(detail) { if(detail && detail.length > 0) result += ' | ' + detail; };
                appendDetail(this.engines(countEngines, engineType));
                appendDetail(this.wakeTurbulenceCat(wtc, true, true));
                appendDetail(this.species(species, true));
            }

            return result;
        }

        /**
         * Formats the Mode-S country as a string.
         */
        modeSCountry(modeSCountry: string) : string
        {
            return modeSCountry ? VRS.$$.translateCountry(modeSCountry) : '';
        }

        /**
         * Formats the notes for display.
         */
        notes(notes: string) : string
        {
            return notes || '';
        }

        /**
         * Formats the aircraft's operator as a string.
         */
        operator(operator: string) : string
        {
            return operator ? operator : '';
        }

        /**
         * Formats the aircraft's operator's ICAO code as a string.
         */
        operatorIcao(operatorIcao: string) : string
        {
            return operatorIcao ? operatorIcao : '';
        }

        /**
         * Formats the aircraft's operator ICAO code and name as a string.
         */
        operatorIcaoAndName(operator: string, operatorIcao: string) : string
        {
            var result = operatorIcao;
            if(operator) {
                if(!result) result = '';
                else result += ': ';
                result += operator;
            }

            return result;
        }

        /**
         * Formats the aircraft's operator ICAO as an HTML IMG tag to the operator flag image.
         */
        operatorIcaoImageHtml(operator: string, operatorIcao: string, icao: string, registration: string) : string
        {
            var codeToUse = this.buildLogoCodeToUse(operatorIcao, icao, registration);
            var size = VRS.globalOptions.aircraftOperatorFlagSize;
            var result = '<img src="images/File-' + encodeURIComponent(codeToUse);
            if(VRS.browserHelper.isHighDpi()) result += '/HiDpi';
            result += '/OpFlag.png"' +
                ' width="' + size.width.toString() + 'px"' +
                ' height="' + size.height.toString() + 'px"' +
                ' />';

            return result;
        }

        /**
         * Joins together a logo code and the ICAO for operator flag and silhouette logos.
         */
        private buildLogoCodeToUse(logoCode: string, icao: string, registration: string) : string
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
         */
        ownershipStatus(ownershipStatus: string) : string
        {
            return ownershipStatus || '';
        }

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
        pictureHtml(registration: string, icao: string, picWidth: number, picHeight: number, requestSize: ISizePartial, allowResizeUp?: boolean, linkToOriginal?: boolean, blankSize?: ISize) : string
        {
            var result = '';

            if(!VRS.serverConfig || VRS.serverConfig.picturesEnabled()) {
                if(allowResizeUp === undefined) allowResizeUp = true;
                var hasPicture = icao && picWidth && picHeight;
                var useOriginal = !requestSize;
                var keepAspectRatio = !!(!useOriginal && requestSize.width && requestSize.height);
                var isHighDpi = VRS.browserHelper.isHighDpi();

                var filePortion = 'images/File-' + encodeURIComponent(registration ? registration : '') + ' ' + encodeURIComponent(icao ? icao : '');
                var sizes = this.calculatedPictureSizes(isHighDpi, picWidth, picHeight, requestSize, blankSize, allowResizeUp);
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
        }

        /**
         * Calculates the size of image to request from the server and the size of image to use in the tag.
         */
        private calculatedPictureSizes(isHighDpi: boolean, picWidth: number, picHeight: number, requestSize: ISizePartial, blankSize: ISize, allowResizeUp: boolean) : ICalculatedPictureSize
        {
            var result: ICalculatedPictureSize = {
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
         */
        popularName(popularName: string) : string
        {
            return popularName || '';
        }

        /**
         * Formats the pressure for display.
         */
        pressure(value: number, unit: PressureEnum, showUnit: boolean) : string
        {
            var result = '';
            value = VRS.unitConverter.convertPressure(value, VRS.Pressure.InHg, unit);
            if(value || value === 0) {
                switch(unit) {
                    case VRS.Pressure.InHg:         result = VRS.stringUtility.formatNumber(value, '0.00'); break;
                    case VRS.Pressure.Millibar:     result = VRS.stringUtility.formatNumber(value, '0'); break;
                    case VRS.Pressure.MmHg:         result = VRS.stringUtility.formatNumber(value, '0.00'); break;
                    default:                        throw 'Unknown pressure unit ' + unit;
                }
                if(showUnit && result) {
                    result = VRS.stringUtility.format(VRS.unitConverter.pressureUnitAbbreviation(unit), result);
                }
            }

            return result;
        }

        /**
         * Formats the previous ID for display.
         */
        previousId(previousId: string) : string
        {
            return previousId || '';
        }

        /**
         * Returns the formatted name of the receiver that last picked up a message for this aircraft.
         */
        receiver(receiverId: number, aircraftListFetcher: AircraftListFetcher) : string
        {
            var result = '';
            if(aircraftListFetcher) {
                var feed = aircraftListFetcher.getFeed(receiverId);
                if(feed) result = feed.name;
            }

            return result;
        }

        /**
         * Returns the registration formatted as a string.
         */
        registration(registration: string, onlyAlphaNumeric?: boolean) : string
        {
            var result = registration ? registration : '';
            if(!!onlyAlphaNumeric) {
                result = VRS.stringUtility.filter(result, function(ch) {
                    return (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z');
                });
            }

            return result;
        }

        /**
         * Given a route consisting of an airport code and name this returns the airport code.
         */
        routeAirportCode(route: string) : string
        {
            var result = route;
            if(result && result.length) {
                var separator = result.indexOf(' ');
                if(separator !== -1) result = result.substr(0, separator);
            }

            return result;
        }

        /**
         * Returns the full route, including all stopovers and airport names, formatted as a string.
         */
        routeFull(callsign: string, from: string, to: string, via: string[])
        {
            var result = '';
            if(callsign) {
                if(!from || !to) result = VRS.$$.RouteNotKnown;
                else result = VRS.$$.formatRoute(from, to, via);
            }

            return result;
        }

        /**
         * Returns the full route, including all stopovers and airport names, formatted as a string.
         */
        reportRouteFull(callsign: string, route: IReportRoute) : string
        {
            var sroute = this.extractReportRouteStrings(route);
            return this.routeMultiLine(callsign, sroute.from, sroute.to, sroute.via);
        }

        /**
         * Returns HTML for the full route spread over multiple lines (separated by BR tags).
         */
        routeMultiLine(callsign: string, from: string, to: string, via: string[]) : string
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
        }

        /**
         * Returns HTML for the full route spread over multiple lines (separated by BR tags).
         */
        reportRouteMultiLine(callsign: string, route: IReportRoute) : string
        {
            var sroute = this.extractReportRouteStrings(route);
            return this.routeMultiLine(callsign, sroute.from, sroute.to, sroute.via);
        }

        /**
         * Returns HTML for the short route (where only airport codes are shown and stopovers can be reduced to an asterisk).
         */
        routeShort(callsign: string, from: string, to: string, via: string[], abbreviateStopovers: boolean, showRouteNotKnown: boolean)
        {
            if(abbreviateStopovers === undefined) abbreviateStopovers = true;

            var result = '';
            if(callsign) {
                var length = via.length;
                var showCircularRoute = from && to && from === to && abbreviateStopovers && length;

                if(from) result += this.routeAirportCode(from);
                if(length) {
                    if(abbreviateStopovers) {
                        if(!showCircularRoute || length > 1) result += '-*';
                        if(showCircularRoute) result += '-' + this.routeAirportCode(via[length - 1]);
                    } else {
                        for(var i = 0;i < length;++i) {
                            result += '-' + this.routeAirportCode(via[i]);
                        }
                    }
                }
                if(to) {
                    if(!showCircularRoute) result += '-' + this.routeAirportCode(to);
                }
                if(showCircularRoute) result += ' ∞';

                if(!result) result = showRouteNotKnown ? VRS.$$.RouteNotKnown : '';
            }

            return result;
        }

        /**
         * Returns HTML for the short route (where only airport codes are shown and stopovers can be reduced to an asterisk).
         */
        reportRouteShort(callsign: string, route: IReportRoute, abbreviateStopovers: boolean, showRouteNotKnown: boolean) : string
        {
            var sroute = this.extractReportRouteStrings(route);
            return this.routeShort(callsign, sroute.from, sroute.to, sroute.via, abbreviateStopovers, showRouteNotKnown);
        }

        /**
         * Formats the aircraft's serial number.
         */
        serial(serial: string) : string
        {
            return serial || '';
        }

        /**
         * Formats the start date/time for display.
         */
        startDateTime(startDate: Date, showFullDate: boolean, justShowTime: boolean) : string
        {
            return this.formatStartEndDate(startDate, undefined, showFullDate, true, justShowTime, false, false);
        }

        /**
         * Formats the aircraft's current status (in service, mothballed etc.)
         */
        status(status: string) : string
        {
            return status || '';
        }

        /**
         * Formats the seconds tracks as a string.
         */
        secondsTracked(secondsTracked: number) : string
        {
            var hms = VRS.timeHelper.secondsToHoursMinutesSeconds(secondsTracked);
            return VRS.$$.formatHoursMinutesSeconds(hms.hours, hms.minutes, hms.seconds);
        }

        /**
         * Formats the signal level as a string.
         */
        signalLevel(signalLevel: number) : string
        {
            return signalLevel === undefined ? '' : signalLevel.toString();
        }

        /**
         * Formats the aircraft species as a string.
         */
        species(species: SpeciesEnum, ignoreNone?: boolean) : string
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
        }

        /**
         * Returns the speed formatted as a string.
         */
        speed(speed: number, speedType: SpeedTypeEnum, speedUnit: SpeedEnum, showUnit: boolean, showType: boolean) : string
        {
            var result = '';
            speed = VRS.unitConverter.convertSpeed(speed, VRS.Speed.Knots, speedUnit);
            if(speed || speed === 0) {
                result = VRS.stringUtility.formatNumber(speed, '0.0');
                if(showUnit && result) result = VRS.stringUtility.format(VRS.unitConverter.speedUnitAbbreviation(speedUnit), result);
                if(showType && result && speedType !== VRS.SpeedType.Ground) {
                    switch(speedType) {
                        case VRS.SpeedType.GroundReversing:     result += ' ' + VRS.$$.ReversingShort; break;
                        case VRS.SpeedType.IndicatedAirSpeed:   result += ' ' + VRS.$$.IndicatedAirSpeedShort; break;
                        case VRS.SpeedType.TrueAirSpeed:        result += ' ' + VRS.$$.TrueAirSpeedShort; break;
                    }
                }
            }

            return result;
        }

        /**
         * Returns the first and last speeds formatted as a string.
         */
        speedFromTo(fromSpeed: number, toSpeed: number, speedUnit: SpeedEnum, showUnits: boolean) : string
        {
            var first = this.speed(fromSpeed, VRS.SpeedType.Ground, speedUnit, showUnits, false);
            var last  = this.speed(toSpeed, VRS.SpeedType.Ground, speedUnit, showUnits, false);
            return this.formatFromTo(first, last, VRS.$$.FromToSpeed);
        }

        /**
         * Returns the speed type formatted as a string.
         */
        speedType(speedType: SpeedTypeEnum) : string
        {
            var result = '';

            if(speedType !== undefined) {
                switch(speedType) {
                    case VRS.SpeedType.Ground:              result = VRS.$$.Ground; break;
                    case VRS.SpeedType.GroundReversing:     result = VRS.$$.Reversing; break;
                    case VRS.SpeedType.IndicatedAirSpeed:   result = VRS.$$.IndicatedAirSpeed; break;
                    case VRS.SpeedType.TrueAirSpeed:        result = VRS.$$.TrueAirSpeed; break;
                    default:                                result = VRS.$$.Unknown;
                }
            }

            return result;
        }

        /**
         * Returns the squawk formatted as a string.
         */
        squawk(squawk: string) : string
        squawk(squawk: number) : string
        squawk(squawk) : string
        {
            if(squawk === null || squawk === undefined) return '';
            if(typeof squawk === 'string') return squawk;
            return VRS.stringUtility.formatNumber(squawk, 4);
        }

        /**
         * Returns a description of the squawk code.
         */
        squawkDescription(squawk: string) : string
        squawkDescription(squawk: number) : string
        squawkDescription(squawk) : string
        {
            var result = '';
            squawk = this.squawk(squawk);
            if(squawk) {
                switch(squawk) {
                    case '7000':    result = VRS.$$.Squawk7000; break;
                    case '7500':    result = VRS.$$.Squawk7500; break;
                    case '7600':    result = VRS.$$.Squawk7600; break;
                    case '7700':    result = VRS.$$.Squawk7700; break;
                }
            }

            return result;
        }

        /**
         * Returns the pair of squawks formatted as a string.
         */
        squawkFromTo(fromSquawk: string, toSquawk: string) : string
        squawkFromTo(fromSquawk: number, toSquawk: number) : string
        squawkFromTo(fromSquawk, toSquawk) : string
        {
            var first = this.squawk(fromSquawk);
            var last = this.squawk(toSquawk);
            return this.formatFromTo(first, last, VRS.$$.FromToSquawk);
        }

        /**
         * Returns an HTML string that displays two text values stacked one on top of the other.
         */
        stackedValues = function(topValue: string, bottomValue: string, tag: string = 'p')
        {
            var startTag = '<' + tag + '>';
            var endTag = '</' + tag + '>';
            return startTag + (VRS.stringUtility.htmlEscape(topValue) || '&nbsp;') + endTag +
                   startTag + (VRS.stringUtility.htmlEscape(bottomValue) || '&nbsp;') + endTag;
        }

        /**
         * Formats the total hours for display.
         */
        totalHours(totalHours: string) : string
        {
            return totalHours || '';
        }

        /**
         * Formats a transponder type into a string.
         */
        transponderType(transponderType: TransponderTypeEnum) : string
        {
            var result = '';

            if(transponderType !== undefined && transponderType !== null) {
                switch(transponderType) {
                    case VRS.TransponderType.Unknown:   result = VRS.$$.Unknown; break;
                    case VRS.TransponderType.ModeS:     result = VRS.$$.ModeS; break;
                    case VRS.TransponderType.Adsb:      result = VRS.$$.ADSB; break;
                    case VRS.TransponderType.Adsb0:     result = VRS.$$.ADSB0; break;
                    case VRS.TransponderType.Adsb1:     result = VRS.$$.ADSB1; break;
                    case VRS.TransponderType.Adsb2:     result = VRS.$$.ADSB2; break;
                }
            }

            return result;
        }

        /**
         * Formats a transponder type into an IMG tag for a corresponding image for the type.
         */
        transponderTypeImageHtml(transponderType: TransponderTypeEnum) : string
        {
            var result = '';

            if(transponderType) {
                var name = '';
                switch(transponderType) {
                    case VRS.TransponderType.ModeS:     name = 'Mode-S'; break;
                    case VRS.TransponderType.Adsb:      name = 'ADSB'; break;
                    case VRS.TransponderType.Adsb0:     name = 'ADSB-0'; break;
                    case VRS.TransponderType.Adsb1:     name = 'ADSB-1'; break;
                    case VRS.TransponderType.Adsb2:     name = 'ADSB-2'; break;
                }

                if(name) {
                    var size = VRS.globalOptions.aircraftTransponderTypeSize;
                    if(VRS.browserHelper.isHighDpi()) name += '@2x';
                    name += '.png';
                    result = '<img src="images/' + name + '" width="' + size.width + 'px" height="' + size.height + '">';
                }
            }

            return result;
        }

        /**
         * Returns the interested flag formatted as a string.
         */
        userInterested(userInterested: boolean) : string
        {
            return userInterested === undefined || userInterested === null ? '' : userInterested ? VRS.$$.Yes : VRS.$$.No;
        }

        /**
         * Returns the user tag formatted as a string.
         */
        userTag(userTag: string) : string
        {
            return userTag || '';
        }

        /**
         * Returns the vertical speed formatted as a string.
         */
        verticalSpeed(verticalSpeed: number, verticalSpeedType: AltitudeTypeEnum, heightUnit: HeightEnum, perSecond: boolean, showUnit?: boolean, showType?: boolean) : string
        {
            var result = '';

            verticalSpeed = VRS.unitConverter.convertVerticalSpeed(verticalSpeed, VRS.Height.Feet, heightUnit, perSecond);
            if(verticalSpeed || verticalSpeed === 0) {
                result = VRS.stringUtility.formatNumber(verticalSpeed, '0');
                if(showUnit) result = VRS.stringUtility.format(VRS.unitConverter.heightUnitOverTimeAbbreviation(heightUnit, perSecond), result);
                if(showType&& verticalSpeedType === VRS.AltitudeType.Geometric) result += ' ' + VRS.$$.GeometricAltitudeIndicator;
            }

            return result;
        }

        /**
         * Returns the vertical speed type formatted as a string.
         */
        verticalSpeedType(verticalSpeedType: AltitudeTypeEnum) : string
        {
            return this.formatAltitudeType(verticalSpeedType);
        }

        /**
         * Returns the wake turbulence category formatted as a string.
         */
        wakeTurbulenceCat(wtc: WakeTurbulenceCategoryEnum, ignoreNone: boolean, expandedDescription: boolean) : string
        {
            var result = '';
            if(wtc || wtc === 0) {
                result = VRS.$$.formatWakeTurbulenceCategory(wtc, ignoreNone, expandedDescription);
            }

            return result;
        }

        /**
         * Formats the year built for display.
         */
        yearBuilt(yearBuilt: string) : string
        {
            return yearBuilt || '';
        }

        /**
         * Returns an object that contains the route information from a report in strings.
         */
        private extractReportRouteStrings(route: IReportRoute) : IRoute
        {
            var via = [];
            if(route && route.via) {
                var length = route.via.length;
                for(var i = 0;i < length;++i) {
                    via.push(this.formatReportAirport(route.via[i]));
                }
            }

            return {
                from: route ? this.formatReportAirport(route.from) : null,
                to:   route ? this.formatReportAirport(route.to) : null,
                via:  via
            };
        }

        /**
         * Joins the code and name from an airport into a single string.
         */
        private formatReportAirport(airport: IReportAirport) : string
        {
            return airport ? airport.fullName : '';
        }

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
        private formatStartEndDate(startDate: Date, endDate: Date, showFullDates: boolean, showStartDate: boolean, onlyShowStartTime: boolean, showEndDate: boolean, alwaysShowEndDate: boolean) : string
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

            return this.formatFromTo(startDateText, endDateText, VRS.$$.FromToDate);
        }

        /**
         * Returns the first and last strings joined together with a format. If either string is empty then the other string
         * is returned without any extra formatting. If both strings are empty then an empty string is returned.
         * @param {string}  fromToFormat    The format to use if both first and last are not empty (e.g. '{0} to {1}').
         */
        private formatFromTo(first: string, last: string, fromToFormat: string) : string
        {
            if(first === last)  last = null;
            if(first && last)   return VRS.stringUtility.format(fromToFormat, first, last);
            else if(first)      return first;
            else                return last || '';
        }
    }

    /*
     * Pre-builts
     */
    export var format = new VRS.Format();
}
