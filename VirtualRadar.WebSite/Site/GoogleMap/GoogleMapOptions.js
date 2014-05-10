function GoogleMapOptions()
{
    var that = this;

    // These are all saved to and loaded from cookies
    this.trace = TraceType.JustSelected;
    this.mapLatitude = _ServerConfig.initialLatitude;
    this.mapLongitude = _ServerConfig.initialLongitude;
    this.mapType = _ServerConfig.initialMapType;
    this.mapZoom = _ServerConfig.initialZoom;
    this.deselectWhenNotTracked = false;
    this.autoSelectEnabled = false;
    this.refreshSeconds = _ServerConfig.initialRefreshSeconds;
    this.pinTextLines = [ PinText.Callsign, PinText.Registration, PinText.None ];
    this.callOutSelected = false;
    this.onlyCallOutAutoSelected = false;
    this.showAltitudeStalk = true;
    this.movingMapChecked = false;
    this.showMovingMap = true;
    this.showShortTrail = false;
    this.distanceUnit = _ServerConfig.initialDistanceUnit;
    this.heightUnit = _ServerConfig.initialHeightUnit;
    this.speedUnit = _ServerConfig.initialSpeedUnit;
    this.autoSelect = new AutoSelect();
    this.verticalSpeedPerSecond = false;
    this.simplePinsWhenZoomedOut = true;
    this.flightLevelTransitionAltitude = -2000;
    this.flightLevelTransitionAltitudeUnit = _ServerConfig.initialHeightUnit;
    this.flightLevelHeightUnit = _ServerConfig.initialHeightUnit;

    this.filteringEnabled = false;
    this.filterAircraftWithNoPosition = false;
    this.filterAircraftNotOnMap = false;
    this.filterAltitudeLower = "";
    this.filterAltitudeUpper = "";
    this.filterDistanceLower = "";
    this.filterDistanceUpper = "";
    this.filterSquawkLower = "";
    this.filterSquawkUpper = "";
    this.filterCallsign = "";
    this.filterOperator = "";
    this.filterRegistration = "";
    this.filterType = "";
    this.filterWtc = "";
    this.filterSpecies = "";
    this.filterEngType = "";
    this.filterCountry = "";
    this.filterIsMilitary = false;
    this.filterIsInteresting = false;

    // Override any defaults for the iPhone and/or iPad
    if(isIphone() || isIpad()) {
        this.showAltitudeStalk = false;
        this.pinTextLines = [ PinText.Registration, PinText.None, PinText.None ];
    }

    this.canRunReports = function() { return _ServerConfig.isLocalAddress || _ServerConfig.internetClientCanRunReports; };
    this.canShowPinText = function() { return _ServerConfig.isLocalAddress || _ServerConfig.internetClientCanShowPinText; };
    this.canPlayAudio = function() { return _ServerConfig.isLocalAddress || _ServerConfig.internetClientCanPlayAudio; };

    // These are temporary and are not saved to or loaded from cookies. Events are
    // not generally raised by code that sets these values.
    this.tempServerTime = new Date();
    this.tempShowSilhouettes = false;
    this.tempShowFlags = false;
    this.tempShowPictures = false;
    this.tempFlagWidth = 85;
    this.tempFlagHeight = 20;
    this.tempRefreshTrails = true;
    this.tempServerShortTrailLengthSeconds = 30;
    this.tempCurrentLocationKnown = false;
    this.tempAvailableAircraft = 0;
    this.tempFlightLevelTransitionAltitudeFeet = 0;

    this.getVsiUnit = function()
    {
        return that.heightUnit;
    }

    this.showTrace = function(isSelected)
    {
        switch(that.trace) {
            case TraceType.All: return true;
            case TraceType.JustSelected: return isSelected;
            case TraceType.None: return false;
        }
    }

    this.recalculateTempValues = function()
    {
        that.tempFlightLevelTransitionAltitudeFeet = that.flightLevelTransitionAltitude;
        switch(that.flightLevelTransitionAltitudeUnit) {
            case HeightUnit.Feet:   break;
            case HeightUnit.Metres: that.tempFlightLevelTransitionAltitudeFeet = that.flightLevelTransitionAltitude * 3.2808399; break;
        }
    };
}

function GoogleMapOptionsStorage()
{
    var that = this;
    var mCookieName = _MapMode !== MapMode.flightSim ? 'googleMapOptions' : 'flightSimOptions';

    function eraseOldCookies()
    {
        eraseCookie("gmOptTraceType");
        eraseCookie("gmOptMapLatitude");
        eraseCookie("gmOptMapLongitude");
        eraseCookie("gmOptMapType");
        eraseCookie("gmOptMapZoom");
        eraseCookie("gmOptAutoDeselect");
        eraseCookie("gmOptAutoSelectClosest");
        eraseCookie("gmOptRefreshSeconds");
        eraseCookie("gmOptDisanceInKm");
        eraseCookie("gmOptShowOutlines");
        eraseCookie("gmOptPinTextLines");
        eraseCookie("gmOptcallOutSelected");
        eraseCookie("gmOptcallOutSelectedVol");
    };

    function loadOldCookieValues(options, valuePair)
    {
        var result = true;
        switch (valuePair.name) {
            case "gmOptTraceType":          options.trace = Number(valuePair.value); break;
            case "gmOptMapLatitude":        if (valuePair.value !== null) options.mapLatitude = Number(valuePair.value); break;
            case "gmOptMapLongitude":       if (valuePair.value !== null) options.mapLongitude = Number(valuePair.value); break;
            case "gmOptMapType":            if (valuePair.value !== null) options.mapType = valuePair.value; break;
            case "gmOptMapZoom":            if (valuePair.value !== null) options.mapZoom = Number(valuePair.value); break;
            case "gmOptAutoDeselect":       options.deselectWhenNotTracked = valuePair.value !== "false"; break;
            case "gmOptAutoSelectClosest":  options.autoSelectEnabled = valuePair.value !== "false"; break;
            case "gmOptRefreshSeconds":     if (valuePair.value !== null) options.refreshSeconds = Number(valuePair.value); break;
            case "gmOptPinTextLines":       options.pinTextLines = readNumberArrayFromString(valuePair.value); break;
            case "gmOptcallOutSelected":    options.callOutSelected = valuePair.value !== "false"; break;
            case "gmOptcallOutSelectedVol": if (valuePair.value !== null) options.callOutSelectedVolume = Number(valuePair.value); break;
            default:                        result = false; break;
        }

        return result;
    };

    this.save = function (options) {
        eraseOldCookies();

        var nameValues = new nameValueCollection();
        nameValues.pushValue('01', options.trace);
        nameValues.pushValue('02', options.mapLatitude);
        nameValues.pushValue('03', options.mapLongitude);
        nameValues.pushValue('04', options.mapType);
        nameValues.pushValue('05', options.mapZoom);
        nameValues.pushValue('06', options.deselectWhenNotTracked);
        nameValues.pushValue('07', options.autoSelectEnabled);
        nameValues.pushValue('08', options.refreshSeconds);
        // '09' was distanceInKm, now superceded by unit options
        nameValues.pushValue('10', writeNumberArrayToString(options.pinTextLines));
        nameValues.pushValue('11', options.callOutSelected);
        nameValues.pushValue('12', options.onlyCallOutAutoSelected);
        nameValues.pushValue('13', options.showAltitudeStalk);
        nameValues.pushValue('14', options.movingMapChecked);
        nameValues.pushValue('15', options.autoSelect.toString());
        nameValues.pushValue('16', options.showMovingMap);
        nameValues.pushValue('17', options.showShortTrail);
        nameValues.pushValue('18', options.distanceUnit);
        nameValues.pushValue('19', options.heightUnit);
        nameValues.pushValue('20', options.speedUnit);
        nameValues.pushValue('21', options.verticalSpeedPerSecond);
        nameValues.pushValue('22', options.simplePinsWhenZoomedOut);
        nameValues.pushValue('23', options.filterAircraftWithNoPosition);
        nameValues.pushValue('24', options.filterAltitudeLower);
        nameValues.pushValue('25', options.filterAltitudeUpper);
        nameValues.pushValue('26', options.filteringEnabled);
        nameValues.pushValue('27', options.filterDistanceLower);
        nameValues.pushValue('28', options.filterDistanceUpper);
        nameValues.pushValue('29', options.filterSquawkLower);
        nameValues.pushValue('30', options.filterSquawkUpper);
        nameValues.pushValue('31', options.filterCallsign);
        nameValues.pushValue('32', options.filterOperator);
        nameValues.pushValue('33', options.filterRegistration);
        nameValues.pushValue('34', options.filterAircraftNotOnMap);
        nameValues.pushValue('35', options.filterType);
        nameValues.pushValue('36', options.filterWtc);
        nameValues.pushValue('37', options.filterSpecies);
        nameValues.pushValue('38', options.filterEngType);
        nameValues.pushValue('39', options.filterCountry);
        nameValues.pushValue('40', options.filterIsMilitary);
        nameValues.pushValue('41', options.filterIsInteresting);
        nameValues.pushValue('42', options.flightLevelTransitionAltitude);
        nameValues.pushValue('43', options.flightLevelTransitionAltitudeUnit);
        nameValues.pushValue('44', options.flightLevelHeightUnit);
        
        writeCookie(mCookieName, nameValues.toString());
    };

    this.load = function () {
        var result = new GoogleMapOptions();

        var cookieValues = readCookieValues();
        var length = cookieValues.length;
        for (var i = 0; i < length; ++i) {
            var valuePair = cookieValues[i];
            if(valuePair.name !== mCookieName) loadOldCookieValues(result, valuePair);
            else {
                var nameValues = new nameValueCollection()
                nameValues.fromString(valuePair.value);
                for(var c = 0;c < nameValues.getLength();++c) {
                    switch(nameValues.getNameAt(c)) {
                        case '01': result.trace = nameValues.getValueAsNumberAt(c); break;
                        case '02': result.mapLatitude = nameValues.getValueAsNumberAt(c); break;
                        case '03': result.mapLongitude = nameValues.getValueAsNumberAt(c); break;
                        case '04': result.mapType = nameValues.getValueAt(c); break;
                        case '05': result.mapZoom = nameValues.getValueAsNumberAt(c); break;
                        case '06': result.deselectWhenNotTracked = nameValues.getValueAsBoolAt(c); break;
                        case '07': result.autoSelectEnabled = nameValues.getValueAsBoolAt(c); break;
                        case '08': result.refreshSeconds = nameValues.getValueAsNumberAt(c); break;
                        case '09': break; // was distanceInKm, now superceded by unit options
                        case '10': result.pinTextLines = readNumberArrayFromString(nameValues.getValueAt(c)); break;
                        case '11': result.callOutSelected = nameValues.getValueAsBoolAt(c); break;
                        case '12': result.onlyCallOutAutoSelected = nameValues.getValueAsBoolAt(c); break;
                        case '13': result.showAltitudeStalk = nameValues.getValueAsBoolAt(c); break;
                        case '14': result.movingMapChecked = nameValues.getValueAsBoolAt(c); break;
                        case '15': result.autoSelect.fromString(nameValues.getValueAt(c)); break;
                        case '16': result.showMovingMap = nameValues.getValueAsBoolAt(c); break;
                        case '17': result.showShortTrail = nameValues.getValueAsBoolAt(c); break;
                        case '18': result.distanceUnit = nameValues.getValueAt(c); break;
                        case '19': result.heightUnit = nameValues.getValueAt(c); break;
                        case '20': result.speedUnit = nameValues.getValueAt(c); break;
                        case '21': result.verticalSpeedPerSecond = nameValues.getValueAsBoolAt(c); break;
                        case '22': result.simplePinsWhenZoomedOut = nameValues.getValueAsBoolAt(c); break;
                        case '23': result.filterAircraftWithNoPosition = nameValues.getValueAsBoolAt(c); break;
                        case '24': result.filterAltitudeLower = nameValues.getValueAt(c); break;
                        case '25': result.filterAltitudeUpper = nameValues.getValueAt(c); break;
                        case '26': result.filteringEnabled = nameValues.getValueAsBoolAt(c); break;
                        case '27': result.filterDistanceLower = nameValues.getValueAt(c); break;
                        case '28': result.filterDistanceUpper = nameValues.getValueAt(c); break;
                        case '29': result.filterSquawkLower = nameValues.getValueAt(c); break;
                        case '30': result.filterSquawkUpper = nameValues.getValueAt(c); break;
                        case '31': result.filterCallsign = nameValues.getValueAt(c); break;
                        case '32': result.filterOperator = nameValues.getValueAt(c); break;
                        case '33': result.filterRegistration = nameValues.getValueAt(c); break;
                        case '34': result.filterAircraftNotOnMap = nameValues.getValueAsBoolAt(c); break;
                        case '35': result.filterType = nameValues.getValueAt(c); break;
                        case '36': result.filterWtc = nameValues.getValueAt(c); break;
                        case '37': result.filterSpecies = nameValues.getValueAt(c); break;
                        case '38': result.filterEngType = nameValues.getValueAt(c); break;
                        case '39': result.filterCountry = nameValues.getValueAt(c); break;
                        case '40': result.filterIsMilitary = nameValues.getValueAsBoolAt(c); break;
                        case '41': result.filterIsInteresting = nameValues.getValueAsBoolAt(c); break;
                        case '42': result.flightLevelTransitionAltitude = nameValues.getValueAt(c); break;
                        case '43': result.flightLevelTransitionAltitudeUnit = nameValues.getValueAt(c); break;
                        case '44': result.flightLevelHeightUnit = nameValues.getValueAt(c); break;
                    }
                }
                i = length;
            }
        }

        if (result.refreshSeconds < _ServerConfig.minimumRefreshSeconds) result.refreshSeconds = _ServerConfig.minimumRefreshSeconds;
        while(result.pinTextLines.length > 3) result.pinTextLines.pop();
        while(result.pinTextLines.length < 3) result.pinTextLines.push(PinText.None);

        if(result.callOutSelectedVolume < 0) result.callOutSelectedVolume = 0;
        if(result.callOutSelectedVolume > 100) result.callOutSelectedVolume = 100;

        if(result.showMovingMap === false) result.movingMapChecked = false;

        result.recalculateTempValues();
        
        return result;
    };
}

var GoogleMapOptionsTabPagesObjects = [];
function googleMapOptionsResetMapClicked(idx)   { GoogleMapOptionsTabPagesObjects[idx].resetMapClicked(); };

function GoogleMapOptionsTabPages(events, options, optionsStorage, currentLocationUI, audio, movingMapButton)
{
    var that = this;
    var mGlobalIndex = GoogleMapOptionsTabPagesObjects.length;
    GoogleMapOptionsTabPagesObjects.push(that);

    var mEvents = events;
    var mOptions = options;
    var mOptionsStorage = optionsStorage;
    var mOptionsUI = null;
    var mCurrentLocationUI = currentLocationUI;
    var mAudio = audio;
    var mMovingMapButton = movingMapButton;

    mEvents.addListener(EventId.optionsUICreateTabPage, createTabPage);
    mEvents.addListener(EventId.toggleAutoSelectClicked, toggleAutoSelectClickedHandler);
    mEvents.addListener(EventId.setAutoSelect, setAutoSelectHandler);

    this.resetMapClicked = function() { mEvents.raise(EventId.optionsResetMapClicked, null, null); };

    function createTabPage(optionsUI, args)
    {
        mOptionsUI = optionsUI;
        optionsUI.createTabPage('general', 1, 'General', generalHtml('general', optionsUI), copyOptionsToForm, copyFormToOptions, save);
        optionsUI.createTabPage('select', 2, 'Selecting', selectingHtml('select', optionsUI), copyOptionsToForm, copyFormToOptions, save);
        optionsUI.createTabPage('aircraft', 3, 'Aircraft', aircraftHtml('aircraft', optionsUI), copyOptionsToForm, copyFormToOptions, save);
        if(_MapMode !== MapMode.flightSim) optionsUI.createTabPage('filter', 5, 'Filters', filterHtml('filter', optionsUI), copyOptionsToForm, copyFormToOptions, save);
    };

    function generalHtml(name, optionsUI)
    {
        var html = '';

        var movingMapButtonExists = mMovingMapButton !== undefined;
        html += optionsUI.htmlHeading('Miscellaneous');
        html += optionsUI.htmlLabel(name, 'refreshSeconds', 'Refresh every ') +
                optionsUI.htmlTextBox(name, 'refreshSeconds', 4) + ' seconds' +
                optionsUI.htmlEol();
        html += optionsUI.htmlHidden('div', !movingMapButtonExists) +
                optionsUI.htmlCheckBox(name, 'showMovingMap', 'Show moving map button') + optionsUI.htmlEol() +
                '</div>';

        html += optionsUI.htmlHeading('Units');
        html += optionsUI.htmlColumns([
                    [ 'Distances', optionsUI.htmlSelect(name, 'distanceUnit', [
                        { value: DistanceUnit.Miles, text: 'Miles (statute)' },
                        { value: DistanceUnit.Kilometres, text: 'Kilometres' },
                        { value: DistanceUnit.NauticalMiles, text: 'Miles (nautical)' }
                        ], 1) ],
                    [ 'Heights', optionsUI.htmlSelect(name, 'heightUnit', [
                        { value: HeightUnit.Feet, text: 'Feet' },
                        { value: HeightUnit.Metres, text: 'Metres' }
                        ], 1) ],
                    [ 'Speeds', optionsUI.htmlSelect(name, 'speedUnit', [
                        { value: SpeedUnit.Knots, text: 'Knots' },
                        { value: SpeedUnit.MilesPerHour, text: 'Miles/Hour' },
                        { value: SpeedUnit.KilometresPerHour, text: 'Kilometres/Hour' }
                        ], 1) ]
                ]);
        html += optionsUI.htmlCheckBox(name, 'verticalSpeedPerSecond', 'Show vertical speed per second') + optionsUI.htmlEol();
        html += optionsUI.htmlColumns([
                    [ 'FL transition altitude', optionsUI.htmlTextBox(name, 'flightLevelTransitionAltitude', 6) + ' ' +
                      optionsUI.htmlSelect(name, 'flightLevelTransitionAltitudeUnit', [
                        { value: HeightUnit.Feet, text: 'Feet' },
                        { value: HeightUnit.Metres, text: 'Metres' }
                        ], 1)
                    ],
                    [ 'FL height unit', optionsUI.htmlSelect(name, 'flightLevelHeightUnit', [
                        { value: HeightUnit.Feet, text: 'Feet' },
                        { value: HeightUnit.Metres, text: 'Metres' }
                        ], 1) ]
                ]);

        if(mCurrentLocationUI !== undefined) html += mCurrentLocationUI.tabPageHtml(optionsUI, name);

        var audioHidden = mAudio === undefined || mAudio.getForbidden() || _MapMode === MapMode.flightSim;
        var audioSupported = mAudio !== undefined && mAudio.getSupported();
        var audioAutoplaySupported = mAudio !== undefined && mAudio.getAutoplaySupported();
        html += optionsUI.htmlHidden('div', audioHidden);
        html += optionsUI.htmlHeading('Audio');
        if(!audioSupported) html += 'This browser does not support HTML5 WAV audio';
        else if(!audioAutoplaySupported) html += 'This browser will not autoplay audio';
        html += optionsUI.htmlHidden('div', !audioSupported || !audioAutoplaySupported);
        html += optionsUI.htmlCheckBox(name, 'callOutSelected', 'Announce details of selected aircraft') + optionsUI.htmlEol();
        html += optionsUI.htmlCheckBox(name, 'onlyCallOutAutoSelected', 'Only announce details of auto-selected aircraft') + optionsUI.htmlEol();
        html += '</div></div>';

        return html;
    }

    function selectingHtml(name, optionsUI)
    {
        var html = '';

        html += optionsUI.htmlHeading('Selecting aircraft');
        html += optionsUI.htmlCheckBox(name, 'deselectWhenNotTracked', 'Hide selected aircraft details when they go off radar') + optionsUI.htmlEol();

        html += optionsUI.htmlHidden('div', _MapMode === MapMode.flightSim);
        html += optionsUI.htmlHeading('Auto-selection');
        html += optionsUI.htmlCheckBox(name, 'autoSelectEnabled', 'Auto-select aircraft') + optionsUI.htmlEol();
        html += 'Select ';
        html += optionsUI.htmlRadioButton(name, 'autoSelectClosest', 'C', 'closest to') + ' ';
        html += optionsUI.htmlRadioButton(name, 'autoSelectClosest', 'F', 'furthest from');
        html += ' current location' + optionsUI.htmlEol();
        var autoSelectRows = [];
        for(var i = 1;i <= 2;++i) {
            autoSelectRows.push( [
                i === 1 ? 'where ' : 'and ',
                optionsUI.htmlSelect(name, 'autoSelectField' + i, [
                        { value: AutoSelectField.nothing, text: 'Nothing' },
                        { value: AutoSelectField.altitude, text: 'Altitude' },
                        { value: AutoSelectField.distance, text: 'Distance' }
                    ], 1) + ' ',
                optionsUI.htmlSelect(name, 'autoSelectCondition' + i, [
                        { value: AutoSelectCondition.lessThan, text: '<' },
                        { value: AutoSelectCondition.lessThanOrEqual, text: '<=' },
                        { value: AutoSelectCondition.equal, text: '=' },
                        { value: AutoSelectCondition.greaterThan, text: '>' },
                        { value: AutoSelectCondition.greaterThanOrEqual, text: '>=' }
                    ], 1) + ' ',
                optionsUI.htmlTextBox(name, 'autoSelectValue' + i, 5)
            ] );
        }
        html += optionsUI.htmlColumns(autoSelectRows);
        html += '</div>';

        return html;
    }

    function aircraftHtml(name, optionsUI)
    {
        var html = '';

        html += optionsUI.htmlHeading('Aircraft display');
        html += optionsUI.htmlCheckBox(name, 'simplePinsWhenZoomedOut', 'Show simple images when zoomed out') + optionsUI.htmlEol();
        html += optionsUI.htmlCheckBox(name, 'showAltitudeStalk', 'Show altitude stalks') + optionsUI.htmlEol();
        if(mOptions.canShowPinText()) {
            for(var lineNumber = 1;lineNumber <= 3;++lineNumber) {
                html += 'Aircraft label line ' + lineNumber + ': ' +
                        optionsUI.htmlSelect(name, 'pinTextLine' + lineNumber, [
                            { value: PinText.None, text: 'None' },
                            { value: PinText.Registration, text:'Registration' },
                            { value: PinText.Callsign, text: 'Callsign' },
                            { value: PinText.Type, text: 'Type' },
                            { value: PinText.ICAO, text: 'ICAO' },
                            { value: PinText.Squawk, text: 'Squawk' },
                            { value: PinText.OperatorCode, text: 'Operator Code' },
                            { value: PinText.Altitude, text: 'Altitude' },
                            { value: PinText.FlightLevel, text: 'Flight Level' },
                            { value: PinText.Speed, text: 'Speed' },
                            { value: PinText.Route, text: 'Route' }
                        ], 1) + optionsUI.htmlEol();
            }
        }
        html += "<a href='#' onClick='googleMapOptionsResetMapClicked(" + mGlobalIndex + ")'>Reset map to default position, zoom and type</a><br/>";

        html += optionsUI.htmlHeading('Aircraft trails');
        html += optionsUI.htmlCheckBox(name, 'showShortTrail', 'Show short trails') + optionsUI.htmlEol();
        html += optionsUI.htmlRadioButton(name, 'traceType', TraceType.None, 'Do not show') + optionsUI.htmlEol();
        html += optionsUI.htmlRadioButton(name, 'traceType', TraceType.JustSelected, 'Show just for the selected aircraft') + optionsUI.htmlEol();
        html += optionsUI.htmlRadioButton(name, 'traceType', TraceType.All, 'Show for all aircraft') + optionsUI.htmlEol();

        return html;
    }

    function filterHtml(name, optionsUI)
    {
        var html = '';

        html += optionsUI.htmlHeading('Filters');
        html += optionsUI.htmlCheckBox(name, 'filterAircraftWithNoPosition', 'Hide aircraft with no position data') + optionsUI.htmlEol();
        html += optionsUI.htmlCheckBox(name, 'filterAircraftNotOnMap', 'Hide aircraft not on map') + optionsUI.htmlEol();

        html += optionsUI.htmlCheckBox(name, 'filterFilteringEnabled', 'Use filters') + optionsUI.htmlEol();

        html += optionsUI.htmlColumns([
                    [  optionsUI.htmlLabel(name, 'filterCallsign', 'Callsign contains'),
                       optionsUI.htmlTextBox(name, 'filterCallsign', 18, true)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterOperator', 'Operator contains'),
                       optionsUI.htmlTextBox(name, 'filterOperator', 18, true)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterRegistration', 'Registration contains'),
                       optionsUI.htmlTextBox(name, 'filterRegistration', 12, true)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterType', 'Type starts with'),
                       optionsUI.htmlTextBox(name, 'filterType', 12, true)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterCountry', 'Country contains'),
                       optionsUI.htmlTextBox(name, 'filterCountry', 12, true)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterAltitudeLower', 'Altitude between'),
                       optionsUI.htmlTextBox(name, 'filterAltitudeLower', 6) +
                       optionsUI.htmlLabel(name, 'filterAltitudeUpper', ' and ') +
                       optionsUI.htmlTextBox(name, 'filterAltitudeUpper', 6)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterDistanceLower', 'Distance between'),
                       optionsUI.htmlTextBox(name, 'filterDistanceLower', 6) +
                       optionsUI.htmlLabel(name, 'filterDistanceUpper', ' and ') +
                       optionsUI.htmlTextBox(name, 'filterDistanceUpper', 6)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterSquawkLower', 'Squawk between'),
                       optionsUI.htmlTextBox(name, 'filterSquawkLower', 6) +
                       optionsUI.htmlLabel(name, 'filterSquawkUpper', ' and ') +
                       optionsUI.htmlTextBox(name, 'filterSquawkUpper', 6)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterWtc', 'Wake turbulence category is'),
                       optionsUI.htmlSelect(name, 'filterWtc', [
                            { value: '', text: '' },
                            { value: '0', text: 'None' },
                            { value: '1', text: 'Light' },
                            { value: '2', text: 'Medium' },
                            { value: '3', text: 'Heavy' }
                       ], 1)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterSpecies', 'Species is'),
                       optionsUI.htmlSelect(name, 'filterSpecies', [
                            { value: '', text: '' },
                            { value: '0', text: 'None' },
                            { value: '1', text: 'Landplane' },
                            { value: '2', text: 'Seaplane' },
                            { value: '3', text: 'Amphibian' },
                            { value: '4', text: 'Helicopter' },
                            { value: '5', text: 'Gyrocopter' },
                            { value: '6', text: 'Tilt-wing' }
                       ], 1)
                    ],
                    [  optionsUI.htmlLabel(name, 'filterEngType', 'Engine type is'),
                       optionsUI.htmlSelect(name, 'filterEngType', [
                            { value: '', text: '' },
                            { value: '0', text: 'None' },
                            { value: '1', text: 'Piston' },
                            { value: '2', text: 'Turboprop / Turboshaft' },
                            { value: '3', text: 'Jet' },
                            { value: '4', text: 'Electric' }
                       ], 1)
                    ],
                    [  '',
                       optionsUI.htmlCheckBox(name, 'filterIsMilitary', 'Only show military')
                    ],
                    [  '',
                       optionsUI.htmlCheckBox(name, 'filterIsInteresting', 'Only show interesting aircraft')
                    ]
                ]);

        return html;
    }

    function copyOptionsToForm(form)
    {
        if(mCurrentLocationUI !== undefined) mCurrentLocationUI.copyOptionsToForm(form);

        setRadioValue(form.traceType, mOptions.trace.toString());
        form.deselectWhenNotTracked.checked = mOptions.deselectWhenNotTracked;
        form.refreshSeconds.value = mOptions.refreshSeconds.toString();
        form.callOutSelected.checked = mOptions.callOutSelected;
        form.onlyCallOutAutoSelected.checked = mOptions.onlyCallOutAutoSelected;
        form.showAltitudeStalk.checked = mOptions.showAltitudeStalk;
        form.simplePinsWhenZoomedOut.checked = mOptions.simplePinsWhenZoomedOut;
        form.showMovingMap.checked = mOptions.showMovingMap;
        form.showShortTrail.checked = mOptions.showShortTrail;
        form.distanceUnit.value = mOptions.distanceUnit;
        form.heightUnit.value = mOptions.heightUnit;
        form.speedUnit.value = mOptions.speedUnit;
        form.verticalSpeedPerSecond.checked = mOptions.verticalSpeedPerSecond;
        form.flightLevelTransitionAltitude.value = mOptions.flightLevelTransitionAltitude;
        form.flightLevelTransitionAltitudeUnit.value = mOptions.flightLevelTransitionAltitudeUnit;
        form.flightLevelHeightUnit.value = mOptions.flightLevelHeightUnit;

        if(_MapMode !== MapMode.flightSim) {
            form.filterAircraftWithNoPosition.checked = mOptions.filterAircraftWithNoPosition;
            form.filterFilteringEnabled.checked = mOptions.filteringEnabled;
            form.filterAircraftNotOnMap.checked = mOptions.filterAircraftNotOnMap;
            form.filterAltitudeLower.value = mOptions.filterAltitudeLower;
            form.filterAltitudeUpper.value = mOptions.filterAltitudeUpper;
            form.filterDistanceLower.value = mOptions.filterDistanceLower;
            form.filterDistanceUpper.value = mOptions.filterDistanceUpper;
            form.filterSquawkLower.value = mOptions.filterSquawkLower;
            form.filterSquawkUpper.value = mOptions.filterSquawkUpper;
            form.filterCallsign.value = mOptions.filterCallsign;
            form.filterOperator.value = mOptions.filterOperator;
            form.filterRegistration.value = mOptions.filterRegistration;
            form.filterType.value = mOptions.filterType;
            form.filterWtc.value = mOptions.filterWtc;
            form.filterSpecies.value = mOptions.filterSpecies;
            form.filterEngType.value = mOptions.filterEngType;
            form.filterCountry.value = mOptions.filterCountry;
            form.filterIsMilitary.checked = mOptions.filterIsMilitary;
            form.filterIsInteresting.checked = mOptions.filterIsInteresting;
        }

        for(var i = 0;i < mOptions.pinTextLines.length;++i) {
            var element = getFormInputElementByName(form, 'pinTextLine' + (i + 1));
            if(element !== null) element.value = mOptions.pinTextLines[i];
        }

        form.autoSelectEnabled.checked = mOptions.autoSelectEnabled;
        setRadioValue(form.autoSelectClosest, mOptions.autoSelect.useClosest ? 'C' : 'F');
        for(i = 0;i < 2;++i) {
            var asField = getFormInputElementByName(form, 'autoSelectField' + (i + 1));
            var asCondition = getFormInputElementByName(form, 'autoSelectCondition' + (i + 1));
            var asValue = getFormInputElementByName(form, 'autoSelectValue' + (i + 1));
            if(mOptions.autoSelect.expressions.length <= i) {
                asField.value = AutoSelectField.nothing;
                asCondition.value = AutoSelectCondition.equal;
                asValue.value = '';
            } else {
                var expression = mOptions.autoSelect.expressions[i];
                asField.value = expression.getField();
                asCondition.value = expression.getCondition();
                asValue.value = expression.getValue();
            }
        }
    }

    function copyFormToOptions(form)
    {
        if(mCurrentLocationUI !== undefined) mCurrentLocationUI.copyFormToOptions(form);
        if(mOptions.autoSelectEnabled !== form.autoSelectEnabled.checked) mEvents.raise(EventId.autoSelectChanged, this, form.autoSelectEnabled.checked);

        mOptions.trace = Number(getRadioValue(form.traceType));
        mOptions.deselectWhenNotTracked = form.deselectWhenNotTracked.checked;
        mOptions.refreshSeconds = Number(form.refreshSeconds.value);
        if(isNaN(mOptions.refreshSeconds) || mOptions.refreshSeconds < _ServerConfig.minimumRefreshSeconds) mOptions.refreshSeconds = _ServerConfig.minimumRefreshSeconds;
        if(mOptions.refreshSeconds > 3600) mOptions.refreshSeconds = 3600;
        mOptions.callOutSelected = form.callOutSelected.checked;
        mOptions.onlyCallOutAutoSelected = form.onlyCallOutAutoSelected.checked;
        mOptions.showAltitudeStalk = form.showAltitudeStalk.checked;
        mOptions.simplePinsWhenZoomedOut = form.simplePinsWhenZoomedOut.checked;
        mOptions.showMovingMap = form.showMovingMap.checked;
        if(mOptions.showShortTrail !== form.showShortTrail.checked) mOptions.tempRefreshTrails = true;
        mOptions.showShortTrail = form.showShortTrail.checked;
        mOptions.distanceUnit = form.distanceUnit.value;
        mOptions.heightUnit = form.heightUnit.value;
        mOptions.speedUnit = form.speedUnit.value;
        mOptions.verticalSpeedPerSecond = form.verticalSpeedPerSecond.checked;
        mOptions.flightLevelTransitionAltitude = form.flightLevelTransitionAltitude.value;
        mOptions.flightLevelTransitionAltitudeUnit = form.flightLevelTransitionAltitudeUnit.value;
        mOptions.flightLevelHeightUnit = form.flightLevelHeightUnit.value;

        if(_MapMode !== MapMode.flightSim) {
            mOptions.filterAircraftWithNoPosition = form.filterAircraftWithNoPosition.checked;
            mOptions.filteringEnabled = form.filterFilteringEnabled.checked;
            mOptions.filterAircraftNotOnMap = form.filterAircraftNotOnMap.checked;
            mOptions.filterAltitudeLower = form.filterAltitudeLower.value;
            mOptions.filterAltitudeUpper = form.filterAltitudeUpper.value;
            mOptions.filterDistanceLower = form.filterDistanceLower.value;
            mOptions.filterDistanceUpper = form.filterDistanceUpper.value;
            mOptions.filterSquawkLower = form.filterSquawkLower.value;
            mOptions.filterSquawkUpper = form.filterSquawkUpper.value;
            mOptions.filterCallsign = form.filterCallsign.value;
            mOptions.filterOperator = form.filterOperator.value;
            mOptions.filterRegistration = form.filterRegistration.value;
            mOptions.filterType = form.filterType.value;
            mOptions.filterWtc = form.filterWtc.value;
            mOptions.filterSpecies = form.filterSpecies.value;
            mOptions.filterEngType = form.filterEngType.value;
            mOptions.filterCountry = form.filterCountry.value;
            mOptions.filterIsMilitary = form.filterIsMilitary.checked;
            mOptions.filterIsInteresting = form.filterIsInteresting.checked;
        }

        for(var i = 0;i < mOptions.pinTextLines.length;++i) {
            var element = getFormInputElementByName(form, 'pinTextLine' + (i + 1));
            if(element !== null) mOptions.pinTextLines[i] = Number(element.value);
        }

        mOptions.autoSelectEnabled = form.autoSelectEnabled.checked;
        mOptions.autoSelect.useClosest = getRadioValue(form.autoSelectClosest) == 'C';
        mOptions.autoSelect.expressions = [];
        for(i = 0;i < 2;++i) {
            var asField = getFormInputElementByName(form, 'autoSelectField' + (i + 1));
            if(asField.value !== AutoSelectField.nothing) {
                var asCondition = getFormInputElementByName(form, 'autoSelectCondition' + (i + 1));
                var asValue = getFormInputElementByName(form, 'autoSelectValue' + (i + 1));
                var expression = new AutoSelectExpression();
                expression.setField(asField.value);
                expression.setCondition(asCondition.value);
                expression.setValue(asValue.value);
                mOptions.autoSelect.expressions.push(expression);
            }
        }
        
        mOptions.recalculateTempValues();
    }

    function save()
    {
        mOptionsStorage.save(mOptions);
    }

    function toggleAutoSelectClickedHandler(sender, args)
    {
        var form = mOptionsUI.getForm();
        form.autoSelectEnabled.checked = !form.autoSelectEnabled.checked;
        if(form.autoSelectEnabled.checked) mEvents.raise(EventId.selectClosestClicked, null, null);
        mOptionsUI.changedHandler(null);
    };

    function setAutoSelectHandler(sender, args)
    {
        if(mOptionsUI !== undefined && mOptionsUI !== null && _MapMode !== MapMode.iPhone) {
            var form = mOptionsUI.getForm();
            if(args != form.autoSelectEnabled.checked) {
                form.autoSelectEnabled.checked = args;
                mOptionsUI.changedHandler(null);
            }
        }
    };
}