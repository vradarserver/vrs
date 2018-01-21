var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftOperatorFlagSize = VRS.globalOptions.aircraftOperatorFlagSize || { width: 85, height: 20 };
    VRS.globalOptions.aircraftSilhouetteSize = VRS.globalOptions.aircraftSilhouetteSize || { width: 85, height: 20 };
    VRS.globalOptions.aircraftBearingCompassSize = VRS.globalOptions.aircraftBearingCompassSize || { width: 16, height: 16 };
    VRS.globalOptions.aircraftTransponderTypeSize = VRS.globalOptions.aircraftTransponderTypeSize || { width: 20, height: 20 };
    VRS.globalOptions.aircraftFlagUncertainCallsigns = VRS.globalOptions.aircraftFlagUncertainCallsigns !== undefined ? VRS.globalOptions.aircraftFlagUncertainCallsigns : true;
    VRS.globalOptions.aircraftAllowRegistrationFlagOverride = VRS.globalOptions.aircraftAllowRegistrationFlagOverride !== undefined ? VRS.globalOptions.aircraftAllowRegistrationFlagOverride : false;
    var Format = (function () {
        function Format() {
            this.stackedValues = function (topValue, bottomValue, tag) {
                if (tag === void 0) { tag = 'p'; }
                var startTag = '<' + tag + '>';
                var endTag = '</' + tag + '>';
                return startTag + (VRS.stringUtility.htmlEscape(topValue) || '&nbsp;') + endTag +
                    startTag + (VRS.stringUtility.htmlEscape(bottomValue) || '&nbsp;') + endTag;
            };
        }
        Format.prototype.airportDataThumbnails = function (airportDataThumbnails, showLinkToSite) {
            var result = '';
            if (airportDataThumbnails && airportDataThumbnails.data && airportDataThumbnails.data.length) {
                result += '<div class="thumbnails">';
                var length = airportDataThumbnails.data.length;
                for (var i = 0; i < length; ++i) {
                    var thumbnail = airportDataThumbnails.data[i];
                    if (thumbnail && thumbnail.image && thumbnail.link) {
                        var copyrightNotice = thumbnail.photographer ? 'Copyright &copy; ' + thumbnail.photographer : 'Copyright holder unknown';
                        if (showLinkToSite)
                            result += '<a href="' + thumbnail.link + '" target="airport-data">';
                        result += '<img src="' + thumbnail.image + '" alt="' + copyrightNotice + '" title="' + copyrightNotice + '">';
                        if (showLinkToSite)
                            result += '</a>';
                    }
                }
                result += '</div>';
            }
            return result;
        };
        Format.prototype.aircraftClass = function (aircraftClass) {
            return aircraftClass || '';
        };
        Format.prototype.altitude = function (altitude, altitudeType, isOnGround, heightUnit, distinguishOnGround, showUnits, showType) {
            var result = '';
            if (distinguishOnGround && isOnGround)
                result = VRS.$$.GroundAbbreviation;
            else {
                altitude = VRS.unitConverter.convertHeight(altitude, VRS.Height.Feet, heightUnit);
                var hasAltitude = altitude || altitude === 0;
                if (hasAltitude) {
                    result = VRS.stringUtility.formatNumber(altitude, '0');
                    if (showUnits)
                        result = VRS.stringUtility.format(VRS.unitConverter.heightUnitAbbreviation(heightUnit), result);
                    if (showType && altitudeType === VRS.AltitudeType.Geometric)
                        result += ' ' + VRS.$$.GeometricAltitudeIndicator;
                }
            }
            return result;
        };
        Format.prototype.altitudeFromTo = function (firstAltitude, firstIsOnGround, lastAltitude, lastIsOnGround, heightUnit, distinguishOnGround, showUnits) {
            var first = this.altitude(firstAltitude, VRS.AltitudeType.Barometric, firstIsOnGround, heightUnit, distinguishOnGround, showUnits, false);
            var last = this.altitude(lastAltitude, VRS.AltitudeType.Barometric, lastIsOnGround, heightUnit, distinguishOnGround, showUnits, false);
            return this.formatFromTo(first, last, VRS.$$.FromToAltitude);
        };
        Format.prototype.altitudeType = function (altitudeType) {
            return this.formatAltitudeType(altitudeType);
        };
        Format.prototype.formatAltitudeType = function (altitudeType) {
            var result = '';
            if (altitudeType !== undefined) {
                switch (altitudeType) {
                    case VRS.AltitudeType.Barometric:
                        result = VRS.$$.Barometric;
                        break;
                    case VRS.AltitudeType.Geometric:
                        result = VRS.$$.Geometric;
                        break;
                    default:
                        result = VRS.$$.Unknown;
                        break;
                }
            }
            return result;
        };
        Format.prototype.averageSignalLevel = function (avgSignalLevel) {
            return avgSignalLevel || avgSignalLevel === 0 ? avgSignalLevel.toString() : '';
        };
        Format.prototype.bearingFromHere = function (bearingFromHere, showUnits) {
            var result = '';
            if (bearingFromHere || bearingFromHere === 0) {
                result = VRS.stringUtility.formatNumber(bearingFromHere, '0.0');
                if (showUnits)
                    result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);
            }
            return result;
        };
        Format.prototype.bearingFromHereImage = function (bearingFromHere) {
            var result = '';
            if (bearingFromHere || bearingFromHere === 0) {
                var size = VRS.globalOptions.aircraftBearingCompassSize;
                result = '<img src="images/Rotate-' + bearingFromHere;
                if (VRS.browserHelper.isHighDpi())
                    result += '/HiDpi';
                result += '/Compass.png"' +
                    ' width="' + size.width + 'px"' +
                    ' height="' + size.height + 'px"' +
                    ' />';
            }
            return result;
        };
        Format.prototype.callsign = function (callsign, callsignSuspect, showUncertainty) {
            var result = callsign || '';
            if (result.length > 0 && callsignSuspect) {
                if (showUncertainty && VRS.globalOptions.aircraftFlagUncertainCallsigns)
                    result += '*';
            }
            return result;
        };
        Format.prototype.certOfACategory = function (cofACategory) {
            return cofACategory || '';
        };
        Format.prototype.certOfAExpiry = function (cofAExpiry) {
            return cofAExpiry || '';
        };
        Format.prototype.countFlights = function (countFlights, format) {
            var result = '';
            if (countFlights || countFlights === 0) {
                result = VRS.stringUtility.formatNumber(countFlights, format || 'N0');
            }
            return result;
        };
        Format.prototype.countMessages = function (countMessages, format) {
            var result = '';
            if (countMessages || countMessages === 0) {
                result = VRS.stringUtility.formatNumber(countMessages, format || 'N0');
            }
            return result;
        };
        Format.prototype.country = function (country) {
            return country ? VRS.$$.translateCountry(country) : '';
        };
        Format.prototype.currentRegistrationDate = function (currentRegDate) {
            return currentRegDate || '';
        };
        Format.prototype.deregisteredDate = function (deregDate) {
            return deregDate || '';
        };
        Format.prototype.distanceFromHere = function (distanceFromHere, distanceUnit, showUnits) {
            var result = '';
            distanceFromHere = VRS.unitConverter.convertDistance(distanceFromHere, VRS.Distance.Kilometre, distanceUnit);
            if (distanceFromHere || distanceFromHere === 0) {
                result = VRS.stringUtility.formatNumber(distanceFromHere, '0.00');
                if (showUnits)
                    result = VRS.stringUtility.format(VRS.unitConverter.distanceUnitAbbreviation(distanceUnit), result);
            }
            return result;
        };
        Format.prototype.duration = function (elapsedTicks, showZeroHours) {
            var hms = VRS.timeHelper.ticksToHoursMinutesSeconds(elapsedTicks);
            return VRS.$$.formatHoursMinutesSeconds(hms.hours, hms.minutes, hms.seconds, showZeroHours);
        };
        Format.prototype.endDateTime = function (startDate, endDate, showFullDate, alwaysShowEndDate) {
            return this.formatStartEndDate(startDate, endDate, showFullDate, false, false, true, alwaysShowEndDate);
        };
        Format.prototype.engines = function (countEngines, engineType) {
            var result = '';
            if (countEngines && engineType) {
                result = VRS.$$.formatEngines(countEngines, engineType);
            }
            return result;
        };
        Format.prototype.firstRegistrationDate = function (firstRegDate) {
            return firstRegDate || '';
        };
        Format.prototype.flightLevel = function (pressureAltitude, geometricAltitude, altitudeType, isOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, showType) {
            var result = '';
            if (pressureAltitude || pressureAltitude === 0) {
                if (distinguishOnGround && isOnGround)
                    result = VRS.$$.GroundAbbreviation;
                else {
                    var transitionAltitudeFeet = VRS.unitConverter.convertHeight(transitionAltitude, transitionAltitudeUnit, VRS.Height.Feet);
                    if (geometricAltitude < transitionAltitudeFeet) {
                        result = this.altitude(geometricAltitude, VRS.AltitudeType.Geometric, isOnGround, altitudeUnit, distinguishOnGround, showUnits, showType);
                    }
                    else {
                        pressureAltitude = VRS.unitConverter.convertHeight(pressureAltitude, VRS.Height.Feet, flightLevelAltitudeUnit);
                        result = VRS.stringUtility.format(VRS.$$.FlightLevelAbbreviation, Math.max(0, Math.round(pressureAltitude / 100)));
                    }
                }
            }
            return result;
        };
        Format.prototype.flightLevelFromTo = function (firstPressureAltitude, firstIsOnGround, lastPressureAltitude, lastIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits) {
            var first = this.flightLevel(firstPressureAltitude, firstPressureAltitude, VRS.AltitudeType.Barometric, firstIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, false);
            var last = this.flightLevel(lastPressureAltitude, lastPressureAltitude, VRS.AltitudeType.Barometric, lastIsOnGround, transitionAltitude, transitionAltitudeUnit, flightLevelAltitudeUnit, altitudeUnit, distinguishOnGround, showUnits, false);
            return this.formatFromTo(first, last, VRS.$$.FromToFlightLevel);
        };
        Format.prototype.genericName = function (genericName) {
            return genericName || '';
        };
        Format.prototype.hadAlert = function (hadAlert) {
            return hadAlert === undefined || hadAlert === null ? '' : hadAlert ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.hadEmergency = function (hadEmergency) {
            return hadEmergency === undefined || hadEmergency === null ? '' : hadEmergency ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.hadSPI = function (hadSPI) {
            return hadSPI === undefined || hadSPI === null ? '' : hadSPI ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.heading = function (heading, headingIsTrue, showUnit, showType) {
            var result = '';
            if (heading || heading === 0) {
                result = VRS.stringUtility.formatNumber(heading, '0.0');
                if (showUnit)
                    result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);
                if (showType && headingIsTrue)
                    result += ' ' + VRS.$$.TrueHeadingShort;
            }
            return result;
        };
        Format.prototype.headingType = function (headingIsTrue) {
            return headingIsTrue ? VRS.$$.TrueHeading : VRS.$$.GroundTrack;
        };
        Format.prototype.icao = function (icao) {
            return icao ? icao : '';
        };
        Format.prototype.ident = function (identActive) {
            return identActive ? VRS.$$.IDENT : '';
        };
        Format.prototype.identActive = function (identActive) {
            return identActive === undefined || identActive === null ? '' : identActive ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.isMlat = function (isMlat) {
            return isMlat === undefined || isMlat === null ? '' : isMlat ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.isTisb = function (isTisb) {
            return isTisb === undefined || isTisb === null ? '' : isTisb ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.isOnGround = function (isOnGround) {
            return isOnGround === undefined || isOnGround === null ? '' : isOnGround ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.isMilitary = function (isMilitary) {
            return isMilitary === undefined || isMilitary === null ? '' : isMilitary ? VRS.$$.Military : VRS.$$.Civil;
        };
        Format.prototype.latitude = function (latitude, showUnit) {
            return this.formatLatitudeLongitude(latitude, showUnit);
        };
        Format.prototype.longitude = function (longitude, showUnit) {
            return this.formatLatitudeLongitude(longitude, showUnit);
        };
        Format.prototype.formatLatitudeLongitude = function (value, showUnit) {
            var result = '';
            if (value || value === 0) {
                result = VRS.stringUtility.formatNumber(value, '0.00000');
                if (showUnit)
                    result = VRS.stringUtility.format(VRS.$$.DegreesAbbreviation, result);
            }
            return result;
        };
        Format.prototype.manufacturer = function (manufacturer) {
            return manufacturer || '';
        };
        Format.prototype.maxTakeoffWeight = function (mtow) {
            return mtow || '';
        };
        Format.prototype.model = function (model) {
            return model || '';
        };
        Format.prototype.modelIcao = function (modelIcao) {
            return modelIcao || '';
        };
        Format.prototype.modelIcaoImageHtml = function (modelIcao, icao, registration) {
            var codeToUse = this.buildLogoCodeToUse(modelIcao, icao, registration);
            var size = VRS.globalOptions.aircraftSilhouetteSize;
            var result = '<img src="images/File-' + encodeURIComponent(codeToUse);
            if (VRS.browserHelper.isHighDpi())
                result += '/HiDpi';
            result += '/Type.png"' +
                ' width="' + size.width.toString() + 'px"' +
                ' height="' + size.height.toString() + 'px"' +
                ' />';
            return result;
        };
        Format.prototype.modelIcaoNameAndDetail = function (modelIcao, model, countEngines, engineType, species, wtc) {
            var result = modelIcao;
            if (result && result.length > 0) {
                if (model)
                    result += ': ' + model;
                var appendDetail = function (detail) { if (detail && detail.length > 0)
                    result += ' | ' + detail; };
                appendDetail(this.engines(countEngines, engineType));
                appendDetail(this.wakeTurbulenceCat(wtc, true, true));
                appendDetail(this.species(species, true));
            }
            return result;
        };
        Format.prototype.modeSCountry = function (modeSCountry) {
            return modeSCountry ? VRS.$$.translateCountry(modeSCountry) : '';
        };
        Format.prototype.notes = function (notes) {
            return notes || '';
        };
        Format.prototype.operator = function (operator) {
            return operator ? operator : '';
        };
        Format.prototype.operatorIcao = function (operatorIcao) {
            return operatorIcao ? operatorIcao : '';
        };
        Format.prototype.operatorIcaoAndName = function (operator, operatorIcao) {
            var result = operatorIcao;
            if (operator) {
                if (!result)
                    result = '';
                else
                    result += ': ';
                result += operator;
            }
            return result;
        };
        Format.prototype.operatorIcaoImageHtml = function (operator, operatorIcao, icao, registration) {
            var codeToUse = this.buildLogoCodeToUse(operatorIcao, icao, registration);
            var size = VRS.globalOptions.aircraftOperatorFlagSize;
            var result = '<img src="images/File-' + encodeURIComponent(codeToUse);
            if (VRS.browserHelper.isHighDpi())
                result += '/HiDpi';
            result += '/OpFlag.png"' +
                ' width="' + size.width.toString() + 'px"' +
                ' height="' + size.height.toString() + 'px"' +
                ' />';
            return result;
        };
        Format.prototype.buildLogoCodeToUse = function (logoCode, icao, registration) {
            var result = '';
            var addCode = function (code) {
                if (code && code.length) {
                    if (result.length)
                        result += '|';
                    result += code;
                }
            };
            addCode(icao);
            if (VRS.globalOptions.aircraftAllowRegistrationFlagOverride)
                addCode(registration);
            addCode(logoCode);
            return result;
        };
        Format.prototype.ownershipStatus = function (ownershipStatus) {
            return ownershipStatus || '';
        };
        Format.prototype.pictureHtml = function (registration, icao, picWidth, picHeight, requestSize, allowResizeUp, linkToOriginal, blankSize) {
            var result = '';
            if (!VRS.serverConfig || VRS.serverConfig.picturesEnabled()) {
                if (allowResizeUp === undefined)
                    allowResizeUp = true;
                var hasPicture = icao && picWidth && picHeight;
                var useOriginal = !requestSize;
                var keepAspectRatio = !!(!useOriginal && requestSize.width && requestSize.height);
                var isHighDpi = VRS.browserHelper.isHighDpi();
                var filePortion = 'images/File-' + encodeURIComponent(registration ? registration : '') + ' ' + encodeURIComponent(icao ? icao : '');
                var sizes = this.calculatedPictureSizes(isHighDpi, picWidth, picHeight, requestSize, blankSize, allowResizeUp);
                if (hasPicture) {
                    result = '<img src="' + filePortion + '/Size-' + (keepAspectRatio ? VRS.AircraftPictureServerSize.List
                        : useOriginal ? VRS.AircraftPictureServerSize.Original
                            : VRS.AircraftPictureServerSize.IPadDetail);
                    if (sizes.requestSize)
                        result += '/Wdth-' + sizes.requestSize.width + '/Hght-' + sizes.requestSize.height;
                    result += '/Picture.png"';
                }
                else if (blankSize) {
                    result = '<img src="images/Wdth-' + blankSize.width + '/Hght-' + blankSize.height + '/Blank.png" ';
                }
                if (result) {
                    if (sizes.tagSize)
                        result += 'width="' + sizes.tagSize.width + 'px" height="' + sizes.tagSize.height + 'px" ';
                    result += ' />';
                    if (linkToOriginal && hasPicture)
                        result = '<a href="' + filePortion + '/Size-Full/Picture.png" target="picture">' + result + '</a>';
                }
            }
            return result;
        };
        Format.prototype.calculatedPictureSizes = function (isHighDpi, picWidth, picHeight, requestSize, blankSize, allowResizeUp) {
            var result = {
                tagSize: blankSize
            };
            if (picWidth !== -1 && picHeight !== -1 && requestSize) {
                var width = requestSize.width;
                var height = requestSize.height;
                if (!allowResizeUp && width > picWidth)
                    width = picWidth;
                if (!height)
                    height = Math.floor(((width / (picWidth / picHeight)) + 0.5));
                result.tagSize = { width: width, height: height };
                if (isHighDpi) {
                    width *= 2;
                    height *= 2;
                }
                result.requestSize = { width: width, height: height };
            }
            return result;
        };
        Format.prototype.popularName = function (popularName) {
            return popularName || '';
        };
        Format.prototype.positionAgeSeconds = function (seconds) {
            var result = '';
            if (!isNaN(seconds)) {
                var hms = VRS.timeHelper.secondsToHoursMinutesSeconds(seconds);
                result = VRS.$$.formatHoursMinutesSeconds(hms.hours, hms.minutes, hms.seconds);
            }
            return result;
        };
        Format.prototype.pressure = function (value, unit, showUnit) {
            var result = '';
            value = VRS.unitConverter.convertPressure(value, VRS.Pressure.InHg, unit);
            if (value || value === 0) {
                switch (unit) {
                    case VRS.Pressure.InHg:
                        result = VRS.stringUtility.formatNumber(value, '0.00');
                        break;
                    case VRS.Pressure.Millibar:
                        result = VRS.stringUtility.formatNumber(value, '0');
                        break;
                    case VRS.Pressure.MmHg:
                        result = VRS.stringUtility.formatNumber(value, '0.00');
                        break;
                    default: throw 'Unknown pressure unit ' + unit;
                }
                if (showUnit && result) {
                    result = VRS.stringUtility.format(VRS.unitConverter.pressureUnitAbbreviation(unit), result);
                }
            }
            return result;
        };
        Format.prototype.previousId = function (previousId) {
            return previousId || '';
        };
        Format.prototype.receiver = function (receiverId, aircraftListFetcher) {
            var result = '';
            if (aircraftListFetcher) {
                var feed = aircraftListFetcher.getFeed(receiverId);
                if (feed)
                    result = feed.name;
            }
            return result;
        };
        Format.prototype.registration = function (registration, onlyAlphaNumeric) {
            var result = registration ? registration : '';
            if (!!onlyAlphaNumeric) {
                result = VRS.stringUtility.filter(result, function (ch) {
                    return (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'z');
                });
            }
            return result;
        };
        Format.prototype.routeAirportCode = function (route) {
            var result = route;
            if (result && result.length) {
                var separator = result.indexOf(' ');
                if (separator !== -1)
                    result = result.substr(0, separator);
            }
            return result;
        };
        Format.prototype.routeFull = function (callsign, from, to, via) {
            var result = '';
            if (callsign) {
                if (!from || !to)
                    result = VRS.$$.RouteNotKnown;
                else
                    result = VRS.$$.formatRoute(from, to, via);
            }
            return result;
        };
        Format.prototype.reportRouteFull = function (callsign, route) {
            var sroute = this.extractReportRouteStrings(route);
            return this.routeMultiLine(callsign, sroute.from, sroute.to, sroute.via);
        };
        Format.prototype.routeMultiLine = function (callsign, from, to, via) {
            var result = '';
            if (!callsign)
                result = VRS.$$.AircraftNotTransmittingCallsign;
            else {
                if (!from || !to)
                    result = VRS.$$.RouteNotKnown;
                else {
                    result += VRS.stringUtility.htmlEscape(from);
                    var length = via.length;
                    if (length) {
                        for (var i = 0; i < length; ++i) {
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
        Format.prototype.reportRouteMultiLine = function (callsign, route) {
            var sroute = this.extractReportRouteStrings(route);
            return this.routeMultiLine(callsign, sroute.from, sroute.to, sroute.via);
        };
        Format.prototype.routeShort = function (callsign, from, to, via, abbreviateStopovers, showRouteNotKnown) {
            if (abbreviateStopovers === undefined)
                abbreviateStopovers = true;
            var result = '';
            if (callsign) {
                var length = via.length;
                var showCircularRoute = from && to && from === to && abbreviateStopovers && length;
                if (from)
                    result += this.routeAirportCode(from);
                if (length) {
                    if (abbreviateStopovers) {
                        if (!showCircularRoute || length > 1)
                            result += '-*';
                        if (showCircularRoute)
                            result += '-' + this.routeAirportCode(via[length - 1]);
                    }
                    else {
                        for (var i = 0; i < length; ++i) {
                            result += '-' + this.routeAirportCode(via[i]);
                        }
                    }
                }
                if (to) {
                    if (!showCircularRoute)
                        result += '-' + this.routeAirportCode(to);
                }
                if (showCircularRoute)
                    result += ' ∞';
                if (!result)
                    result = showRouteNotKnown ? VRS.$$.RouteNotKnown : '';
            }
            return result;
        };
        Format.prototype.reportRouteShort = function (callsign, route, abbreviateStopovers, showRouteNotKnown) {
            var sroute = this.extractReportRouteStrings(route);
            return this.routeShort(callsign, sroute.from, sroute.to, sroute.via, abbreviateStopovers, showRouteNotKnown);
        };
        Format.prototype.serial = function (serial) {
            return serial || '';
        };
        Format.prototype.startDateTime = function (startDate, showFullDate, justShowTime) {
            return this.formatStartEndDate(startDate, undefined, showFullDate, true, justShowTime, false, false);
        };
        Format.prototype.status = function (status) {
            return status || '';
        };
        Format.prototype.secondsTracked = function (secondsTracked) {
            var hms = VRS.timeHelper.secondsToHoursMinutesSeconds(secondsTracked);
            return VRS.$$.formatHoursMinutesSeconds(hms.hours, hms.minutes, hms.seconds);
        };
        Format.prototype.signalLevel = function (signalLevel) {
            return signalLevel === undefined ? '' : signalLevel.toString();
        };
        Format.prototype.species = function (species, ignoreNone) {
            if (!species && species !== 0)
                return '';
            switch (species) {
                case VRS.Species.None: return ignoreNone ? '' : VRS.$$.None;
                case VRS.Species.Amphibian: return VRS.$$.Amphibian;
                case VRS.Species.Gyrocopter: return VRS.$$.Gyrocopter;
                case VRS.Species.Helicopter: return VRS.$$.Helicopter;
                case VRS.Species.LandPlane: return VRS.$$.LandPlane;
                case VRS.Species.SeaPlane: return VRS.$$.SeaPlane;
                case VRS.Species.Tiltwing: return VRS.$$.Tiltwing;
                case VRS.Species.GroundVehicle: return VRS.$$.GroundVehicle;
                case VRS.Species.Tower: return VRS.$$.RadioMast;
                default: throw 'Unknown species type ' + species;
            }
        };
        Format.prototype.speed = function (speed, speedType, speedUnit, showUnit, showType) {
            var result = '';
            speed = VRS.unitConverter.convertSpeed(speed, VRS.Speed.Knots, speedUnit);
            if (speed || speed === 0) {
                result = VRS.stringUtility.formatNumber(speed, '0.0');
                if (showUnit && result)
                    result = VRS.stringUtility.format(VRS.unitConverter.speedUnitAbbreviation(speedUnit), result);
                if (showType && result && speedType !== VRS.SpeedType.Ground) {
                    switch (speedType) {
                        case VRS.SpeedType.GroundReversing:
                            result += ' ' + VRS.$$.ReversingShort;
                            break;
                        case VRS.SpeedType.IndicatedAirSpeed:
                            result += ' ' + VRS.$$.IndicatedAirSpeedShort;
                            break;
                        case VRS.SpeedType.TrueAirSpeed:
                            result += ' ' + VRS.$$.TrueAirSpeedShort;
                            break;
                    }
                }
            }
            return result;
        };
        Format.prototype.speedFromTo = function (fromSpeed, toSpeed, speedUnit, showUnits) {
            var first = this.speed(fromSpeed, VRS.SpeedType.Ground, speedUnit, showUnits, false);
            var last = this.speed(toSpeed, VRS.SpeedType.Ground, speedUnit, showUnits, false);
            return this.formatFromTo(first, last, VRS.$$.FromToSpeed);
        };
        Format.prototype.speedType = function (speedType) {
            var result = '';
            if (speedType !== undefined) {
                switch (speedType) {
                    case VRS.SpeedType.Ground:
                        result = VRS.$$.Ground;
                        break;
                    case VRS.SpeedType.GroundReversing:
                        result = VRS.$$.Reversing;
                        break;
                    case VRS.SpeedType.IndicatedAirSpeed:
                        result = VRS.$$.IndicatedAirSpeed;
                        break;
                    case VRS.SpeedType.TrueAirSpeed:
                        result = VRS.$$.TrueAirSpeed;
                        break;
                    default: result = VRS.$$.Unknown;
                }
            }
            return result;
        };
        Format.prototype.squawk = function (squawk) {
            if (squawk === null || squawk === undefined)
                return '';
            if (typeof squawk === 'string')
                return squawk;
            return VRS.stringUtility.formatNumber(squawk, 4);
        };
        Format.prototype.squawkDescription = function (squawk) {
            var result = '';
            squawk = this.squawk(squawk);
            if (squawk) {
                switch (squawk) {
                    case '7000':
                        result = VRS.$$.Squawk7000;
                        break;
                    case '7500':
                        result = VRS.$$.Squawk7500;
                        break;
                    case '7600':
                        result = VRS.$$.Squawk7600;
                        break;
                    case '7700':
                        result = VRS.$$.Squawk7700;
                        break;
                }
            }
            return result;
        };
        Format.prototype.squawkFromTo = function (fromSquawk, toSquawk) {
            var first = this.squawk(fromSquawk);
            var last = this.squawk(toSquawk);
            return this.formatFromTo(first, last, VRS.$$.FromToSquawk);
        };
        Format.prototype.totalHours = function (totalHours) {
            return totalHours || '';
        };
        Format.prototype.transponderType = function (transponderType) {
            var result = '';
            if (transponderType !== undefined && transponderType !== null) {
                switch (transponderType) {
                    case VRS.TransponderType.Unknown:
                        result = VRS.$$.Unknown;
                        break;
                    case VRS.TransponderType.ModeS:
                        result = VRS.$$.ModeS;
                        break;
                    case VRS.TransponderType.Adsb:
                        result = VRS.$$.ADSB;
                        break;
                    case VRS.TransponderType.Adsb0:
                        result = VRS.$$.ADSB0;
                        break;
                    case VRS.TransponderType.Adsb1:
                        result = VRS.$$.ADSB1;
                        break;
                    case VRS.TransponderType.Adsb2:
                        result = VRS.$$.ADSB2;
                        break;
                }
            }
            return result;
        };
        Format.prototype.transponderTypeImageHtml = function (transponderType) {
            var result = '';
            if (transponderType) {
                var name = '';
                switch (transponderType) {
                    case VRS.TransponderType.ModeS:
                        name = 'Mode-S';
                        break;
                    case VRS.TransponderType.Adsb:
                        name = 'ADSB';
                        break;
                    case VRS.TransponderType.Adsb0:
                        name = 'ADSB-0';
                        break;
                    case VRS.TransponderType.Adsb1:
                        name = 'ADSB-1';
                        break;
                    case VRS.TransponderType.Adsb2:
                        name = 'ADSB-2';
                        break;
                }
                if (name) {
                    var size = VRS.globalOptions.aircraftTransponderTypeSize;
                    if (VRS.browserHelper.isHighDpi())
                        name += '@2x';
                    name += '.png';
                    result = '<img src="images/' + name + '" width="' + size.width + 'px" height="' + size.height + '">';
                }
            }
            return result;
        };
        Format.prototype.userInterested = function (userInterested) {
            return userInterested === undefined || userInterested === null ? '' : userInterested ? VRS.$$.Yes : VRS.$$.No;
        };
        Format.prototype.userTag = function (userTag) {
            return userTag || '';
        };
        Format.prototype.verticalSpeed = function (verticalSpeed, isOnGround, verticalSpeedType, heightUnit, perSecond, showUnit, showType) {
            var result = '';
            if (verticalSpeed && !!isOnGround) {
                verticalSpeed = 0;
            }
            verticalSpeed = VRS.unitConverter.convertVerticalSpeed(verticalSpeed, VRS.Height.Feet, heightUnit, perSecond);
            if (verticalSpeed || verticalSpeed === 0) {
                result = VRS.stringUtility.formatNumber(verticalSpeed, '0');
                if (showUnit)
                    result = VRS.stringUtility.format(VRS.unitConverter.heightUnitOverTimeAbbreviation(heightUnit, perSecond), result);
                if (showType && verticalSpeedType === VRS.AltitudeType.Geometric)
                    result += ' ' + VRS.$$.GeometricAltitudeIndicator;
            }
            return result;
        };
        Format.prototype.verticalSpeedType = function (verticalSpeedType) {
            return this.formatAltitudeType(verticalSpeedType);
        };
        Format.prototype.wakeTurbulenceCat = function (wtc, ignoreNone, expandedDescription) {
            var result = '';
            if (wtc || wtc === 0) {
                result = VRS.$$.formatWakeTurbulenceCategory(wtc, ignoreNone, expandedDescription);
            }
            return result;
        };
        Format.prototype.yearBuilt = function (yearBuilt) {
            return yearBuilt || '';
        };
        Format.prototype.extractReportRouteStrings = function (route) {
            var via = [];
            if (route && route.via) {
                var length = route.via.length;
                for (var i = 0; i < length; ++i) {
                    via.push(this.formatReportAirport(route.via[i]));
                }
            }
            return {
                from: route ? this.formatReportAirport(route.from) : null,
                to: route ? this.formatReportAirport(route.to) : null,
                via: via
            };
        };
        Format.prototype.formatReportAirport = function (airport) {
            return airport ? airport.fullName : '';
        };
        Format.prototype.formatStartEndDate = function (startDate, endDate, showFullDates, showStartDate, onlyShowStartTime, showEndDate, alwaysShowEndDate) {
            var result = '';
            var startDateText = '';
            if (startDate && showStartDate) {
                if (onlyShowStartTime) {
                    startDateText = Globalize.format(startDate, 'T');
                }
                else {
                    if (showFullDates)
                        startDateText = Globalize.format(startDate, 'F');
                    else {
                        startDateText = VRS.stringUtility.format(VRS.$$.DateTimeShort, Globalize.format(startDate, 'd'), Globalize.format(startDate, 'T'));
                    }
                }
            }
            var endDateText = '';
            if (endDate && showEndDate) {
                var suppressDate = !alwaysShowEndDate && startDate && (VRS.dateHelper.getDatePortion(startDate) === VRS.dateHelper.getDatePortion(endDate));
                if (suppressDate) {
                    endDateText = Globalize.format(endDate, 'T');
                }
                else {
                    if (showFullDates)
                        endDateText = Globalize.format(endDate, 'F');
                    else {
                        endDateText = VRS.stringUtility.format(VRS.$$.DateTimeShort, Globalize.format(endDate, 'd'), Globalize.format(endDate, 'T'));
                    }
                }
            }
            return this.formatFromTo(startDateText, endDateText, VRS.$$.FromToDate);
        };
        Format.prototype.formatFromTo = function (first, last, fromToFormat) {
            if (first === last)
                last = null;
            if (first && last)
                return VRS.stringUtility.format(fromToFormat, first, last);
            else if (first)
                return first;
            else
                return last || '';
        };
        return Format;
    }());
    VRS.Format = Format;
    VRS.format = new VRS.Format();
})(VRS || (VRS = {}));
//# sourceMappingURL=format.js.map