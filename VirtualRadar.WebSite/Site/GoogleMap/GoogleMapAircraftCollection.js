function GoogleMapAircraft(options)
{
    var that = this;
    var mOptions = options;

    this.Id = 0;

    this.Icao = "";
    this.IcaoChanged = true;
    this.Bad = false;
    this.BadChanged = true;
    this.Reg = "";
    this.RegChanged = true;
    this.Alt = null;
    this.AltChanged = true;
    this.Call = "";
    this.CallChanged = true;
    this.CallSus = false;
    this.CallSusChanged = true;
    this.Lat = null;
    this.LatChanged = true;
    this.Long = null;
    this.LongChanged = true;
    this.PosTime = null;
    this.PosTimeChanged = true;
    this.Spd = null;
    this.SpdChanged = true;
    this.Trak = null;
    this.TrakChanged = true;
    this.Type = "";
    this.TypeChanged = true;
    this.Mdl = "";
    this.MdlChanged = true;
    this.CNum = "";
    this.CNumChanged = true;
    this.LNum = "";
    this.LNumChanged = true;
    this.From = "";
    this.FromChanged = true;
    this.To = "";
    this.ToChanged = true;
    this.Stops = [];
    this.StopsChanged = true;
    this.Op = "";
    this.OpChanged = true;
    this.OpIcao = "";
    this.OpIcaoChanged = true;
    this.Sqk = "";
    this.SqkChanged = true;
    this.Help = false;
    this.HelpChanged = true;
    this.Vsi = null;
    this.VsiChanged = true;
    this.Dst = null;
    this.DstChanged = true;
    this.Brng = null;
    this.BrngChanged = true;
    this.PreviousBrng = null;
    this.Cot = null;
    this.Cos = null;
    this.FlightLevel = null;
    this.FlightLevelChanged = true;
    this.ConvertedDst = null;
    this.ConvertedDstChanged = true;
    this.ConvertedAlt = null;
    this.ConvertedAltChanged = true;
    this.ConvertedSpd = null;
    this.ConvertedSpdChanged = true;
    this.ConvertedVsi = null;
    this.ConvertedVsiChanged = true;
    this.WTC = null;
    this.WTCChanged = true;
    this.EngType = null;
    this.EngTypeChanged = true;
    this.Engines = null;
    this.EnginesChanged = true;
    this.Species = null;
    this.SpeciesChanged = true;
    this.Mil = false;
    this.MilChanged = false;
    this.Cou = null;
    this.CouChanged = false;
    this.HasPic = false;
    this.HasPicChanged = false;
    this.ResetTrail = false;
    this.TSecs = 0;
    this.FlightsCount = null;
    this.FlightsCountChanged = false;
    this.OnGround = null;
    this.OnGroundChanged = false;
    this.SpeedType = null;
    this.SpeedTypeChanged = false;
    this.CMsgs = null;
    this.CMsgsChanged = false;
    this.Tag = null;
    this.TagChanged = false;

    this.getHasPos = function() { return that.PosTime !== null && that.Lat !== null && that.Lat !== 0 && that.Long !== null && that.Long !== 0; };
    this.getAFR = function() { return formatAirframesOrgRegistration(that.Reg); };

    this.formatOperatorFlag = function(extraTags, imgClass)
    {
        var result = '<img src="Images/File-' + encodeURIComponent(that.OpIcao == '' ? that.Icao : that.OpIcao) + '/OpFlag.png" width="' + mOptions.tempFlagWidth + 'px" height="' + mOptions.tempFlagHeight + 'px" ';
        if(imgClass) result += 'class="' + imgClass + '" ';
        if(extraTags) result += extraTags;
        return result += ' />';
    };

    this.formatSilhouette = function(extraTags, imgClass)
    {
        var result = '<img src="Images/File-' + encodeURIComponent(that.Type == '' ? '' : that.Type) + '/Type.png" ';
        if(imgClass) result += 'class="' + imgClass + '" ';
        if(extraTags) result += extraTags;
        return result += ' />';
    };

    this.formatPicture = function(extraTags, imgClass, linkToOriginal, size, altWidth, altHeight, forceBlankPicture)
    {
        var result = '';
        if(size) {
            var validPicture = false;
            var filePortion = 'Images/File-' + encodeURIComponent(that.Reg ? that.Reg : '') + ' ' + encodeURIComponent(that.Icao);
            if(!forceBlankPicture && that.HasPic && that.Icao) {
                result = '<img src="' + filePortion + '/Size-' + size + '/Picture.png"';
                validPicture = true;
            } else if(altWidth && altHeight) {
                result = '<img src="Images/Wdth-' + altWidth + '/Hght-' + altHeight + '/Blank.png" ';
            }
            if(result !== '') {
                if(altWidth) result += 'width="' + altWidth + 'px" ';
                if(altHeight) result += 'height="' + altHeight + 'px" ';
                if(imgClass) result += 'class="' + imgClass + '" ';
                if(extraTags) result += extraTags;
                result += ' />';

                if(linkToOriginal && validPicture) {
                    result = '<a href="' + filePortion + '/Size-Full/Picture.png" target="picture">' + result + '</a>';
                }
            }
        }

        return result;
    };

    this.formatBearingCompass = function(locationKnown, extraTags, imgClass)
    {
        var result = !locationKnown || !that.Brng ? '' : '<img src="Images/Rotate-' + Math.floor(that.Brng) + '/Compass.png" width="16px" height="16px" alt="" title="' + that.Brng + '&deg;" ';
        if(result.length !== 0) {
            if(imgClass) result += 'class="' + imgClass + '" ';
            if(extraTags) result += extraTags;
            result += ' />';
        }

        return result;
    };

    this.formatConvertedDst = function()
    {
        var result = that.ConvertedDst === null ? null : that.ConvertedDst.toString();
        if(result !== null) {
            switch(mOptions.distanceUnit) {
                case DistanceUnit.Miles:            result += ' miles'; break;
                case DistanceUnit.Kilometres:       result += ' km'; break;
                case DistanceUnit.NauticalMiles:    result += ' nmi'; break;
            }
        }

        return result;
    };

    this.formatDistance = function(currentLocationKnown)
    {
        var result = null;
        if(that.Dst !== null) result = that.formatConvertedDst() + ' from ' + (currentLocationKnown ? 'here' : 'map centre');

        return result;
    }

    this.formatBearing = function(currentLocationKnown)
    {
        var result = null;
        if(that.Brng !== null) result = that.Brng + '&deg; ' + getTrackDescription(that.Brng);

        return result;
    }

    this.formatConvertedAlt = function()
    {
        var result = that.ConvertedAlt === null ? null : that.ConvertedAlt.toString();
        if(result !== null) {
            switch(mOptions.heightUnit) {
                case HeightUnit.Feet:           result += ' feet'; break;
                case HeightUnit.Metres:         result += ' metres'; break;
            }
        }

        return result;
    };

    this.formatFlightLevel = function()
    {
        return that.FlightLevel === null ? null : that.FlightLevel.toString();
    };

    this.formatConvertedSpd = function()
    {
        var result = that.ConvertedSpd === null ? null : that.ConvertedSpd.toString();
        if(result !== null) {
            switch(mOptions.speedUnit) {
                case SpeedUnit.Knots:               result += ' kts'; break;
                case SpeedUnit.MilesPerHour:        result += ' mph'; break;
                case SpeedUnit.KilometresPerHour:   result += ' km/h'; break;
            }
        }

        return result;
    };

    this.formatSpeedType = function(capitalise)
    {
        var result = '';

        if(that.SpeedType !== undefined) {
            if(capitalise === undefined) capitalise = false;

            switch(that.SpeedType) {
                case 0:     result = (capitalise ? 'G' : 'g') + 'round speed'; break;
                case 1:     result = (capitalise ? 'G' : 'g') + 'round speed (reversing)'; break;
                case 2:     result = (capitalise ? 'I' : 'i') + 'ndicated airspeed'; break;
                case 3:     result = (capitalise ? 'T' : 't') + 'rue airspeed'; break;
            }
        }

        return result;
    };

    this.formatConvertedVsi = function()        { return doFormatConvertedVsi(that.ConvertedVsi); }
    this.formatAbsConvertedVsi = function()     { return doFormatConvertedVsi(Math.abs(that.ConvertedVsi)); }
    function doFormatConvertedVsi(vsi)
    {
        var result = vsi === null ? null : vsi.toString();
        if(result !== null) {
            switch(mOptions.getVsiUnit()) {
                case HeightUnit.Feet:           result += ' feet/'; break;
                case HeightUnit.Metres:         result += ' metres/'; break;
            }
            result += mOptions.verticalSpeedPerSecond ? 'sec' : 'min';
        }

        return result;
    }

    this.formatVerticalSpeed = function()
    {
        var result = that.OnGround ? 'On ground' : that.Vsi === null ? null : '';
        if(result === '') {
            if(that.Vsi === 0) result = 'In level flight';
            else if(that.Vsi < 0) result = 'Descending ' + that.formatAbsConvertedVsi();
            else result = 'Climbing ' + that.formatAbsConvertedVsi();
        }

        return result;
    }

    this.formatHeading = function()
    {
        var result = null;
        if(that.Trak !== null) result = 'Heading ' + getTrackDescription(that.Trak) + ' ' + that.Trak + '&deg;';

        return result;
    }

    function formatReportLink(page, description, critName, critValue, targetName)
    {
        var result = !description ? '' : description;
        if(_MapMode !== MapMode.flightSim) {
            if(mOptions.canRunReports()) {
                var forceFrame = getPageParameterValue(null, 'forceFrame');
                forceFrame = forceFrame ? '&forceFrame=' + encodeURIComponent(forceFrame) : '';
                result = !description ? '' : '<a href="' + page + '?' + critName + '=' + encodeURIComponent(critValue) + forceFrame + '"' + target(targetName) + '>' + result + '</a>';
            }
        }

        return result;
    }

    this.formatRegReportLink = function()       { return formatReportLink('RegReport.htm', that.Reg, 'reg', that.Reg, 'regReport'); };
    this.formatIcaoReportLink = function()      { return formatReportLink('IcaoReport.htm', that.Icao ? that.Icao + (that.Bad ? '!' : '') : null, 'icao', that.Icao, 'icaoReport'); };
    this.formatCallsignReportLink = function()  { return formatReportLink('DateReport.htm', that.Call, 'callsign', that.Call, 'callsignReport'); };

    this.formatRouteTooltip = function()
    {
        var result = '';
        if(that.From != '') result += 'From ' + that.From;
        if(that.To != '') {
            if(result.length > 0) result += ' to ';
            else result = 'To ';
            result += that.To;
        }
        if(that.Stops && that.Stops.length) {
            if(result.length > 0) result += ' via ';
            else result = 'Via ';
            for(var i = 0;i < that.Stops.length;++i) {
                if(i > 0) result += (i + 1 == that.Stops.length) ? ' and ' : ', ';
                result += that.Stops[i];
            }
        }
        if(result.length === 0 && that.Call !== '') result = "Route not known";

        return result;
    };

    this.formatRoutePinText = function()
    {
        var result = '';
        if(that.From !== '') result += getRouteAirport(that.From);
        if(that.Stops && that.Stops.length) result += '-*';
        if(that.To !== '') {
            if(result.length) result += '-';
            result += getRouteAirport(that.To);
        }

        return result;
    };

    this.formatRouteListText = function()
    {
        return that.formatRoutePinText();
    };

    this.formatLinkToSubmitRoute = function(cannotSubmitRoutesMessage)
    {
        var result = '';
        if(!_ServerConfig.isLocalAddress && !_ServerConfig.internetClientCanSubmitRoutes) {
            result = cannotSubmitRoutesMessage === undefined ? '' : cannotSubmitRoutesMessage;
        } else {
            if(that.Call && that.Call.length > 0) {
                result = '<a href="http://www.virtualradarserver.co.uk/Redirect/SdmAddCallsigns.aspx?callsigns=' + encodeURIComponent(that.Call) + '" target="vrssdm">';
                if(that.From && that.From.length > 0) result += 'Submit route correction';
                else result += 'Submit route';
                result += '</a>';
            }
        }

        return result;
    };

    this.formatWakeTurbulenceCategory = function(ignoreNone, expandedDescription)
    {
        return formatWakeTurbulenceCategory(that.WTC, ignoreNone && ignoreNone, expandedDescription && expandedDescription);
    };

    this.formatEngines = function()
    {
        return formatEngines(that.Engines, that.EngType);
    };

    this.formatEngineType = function(ignoreNone)
    {
        return formatEngineType(that.engType, ignoreNone && ignoreNone);
    };

    this.formatIcao8643Details = function()
    {
        return formatIcao8643Details(that.Engines, that.EngType, that.Species, that.WTC);
    };

    this.formatSpecies = function(ignoreNone)
    {
        return formatSpecies(that.Species, ignoreNone && ignoreNone);
    };

    this.formatCountry = function(ignoreNone)
    {
        return formatCountry(that.Cou, that.Mil, ignoreNone && ignoreNone);
    };

    this.formatTrackedSeconds = function()
    {
        var minutes = Math.floor(that.TSecs / 60);
        var seconds = that.TSecs - (minutes * 60);

        return intToString(minutes, 2) + ':' + intToString(seconds, 2);
    };

    this.formatCountMessages = function()
    {
        return that.CMsgs ? that.CMsgs.toString() : '0';
    };

    this.centreOnMap = function(map)
    {
        if(that.getHasPos()) {
            var centre = map.getCenter();
            if(centre.lat !== that.Lat || centre.lng !== that.Long) {
                map.setCenter(new google.maps.LatLng(that.Lat, that.Long));
            }
        }
    };
}

function GoogleMapAircraftCollection(events, options)
{
    var that = this;
    var mEvents = events;
    var mOptions = options;

    // All aircraft being tracked
    var mAircraftList = [];
    this.getAllAircraft = function() { return mAircraftList; };

    // The version of the data we got off the server
    var mDataVersion = "-1";
    this.getDataVersion = function() { return mDataVersion; };

    // The currently selected aircraft, if any, and how it was selected
    var mSelectedAircraft = null;
    var mSelectedByUser = false;
    this.getSelectedAircraft = function() { return mSelectedAircraft; };
    this.getSelectedByUser = function() { return mSelectedByUser; };

    // The aircraft that auto-select would or has selected
    var mAutoSelectAircraft = null;
    this.getAutoSelectAircraft = function() { return mAutoSelectAircraft; };

    // Event handlers
    mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);
    function optionsChangedHandler(sender, args)
    {
        var length = mAircraftList.length;
        for(var i = 0;i < length;++i) {
            CalculateConvertedFields(mAircraftList[i]);
        }
    };

    // Gets a list of all aircraft identifiers in a format suitable for transmission back to the server
    this.getAircraftIdListParameter = function()
    {
        var result = '';
        var length = mAircraftList.length;
        for(var i = 0;i < length;++i) {
            if(i !== 0) result += ',';
            result += mAircraftList[i].Id.toString();
        }
        result = encodeURIComponent(result);

        return result;
    };

    // Adds a new aircraft to the list
    this.addAircraft = function(id)
    {
        var result = new GoogleMapAircraft(mOptions);
        result.Id = id;
        mAircraftList.push(result);

        return result;
    };

    // Finds an existing aircraft in the list
    this.findAircraft = function(id)
    {
        var result = null;

        var length = mAircraftList.length;
        for(var i = 0;i < length;++i) {
            if(mAircraftList[i].Id === id) {
                result = mAircraftList[i];
                break;
            }
        }

        return result;
    };

    // Selects an aircraft
    this.selectAircraft = function(aircraft, selectedByUser)
    {
        if(mSelectedAircraft !== aircraft) {
            if(selectedByUser && mOptions.autoSelectEnabled) {
                mEvents.raise(EventId.setAutoSelect, null, false);
            }

            if(mSelectedAircraft !== null) {
                mSelectedAircraft.Selected = false;
                mEvents.raise(EventId.acDeselected, null, { aircraft: mSelectedAircraft } );
            }

            if(aircraft === null || aircraft.OnRadar) {
                mSelectedAircraft = aircraft;
                mSelectedByUser = selectedByUser;

                if(mSelectedAircraft !== null) mSelectedAircraft.Selected = true;
                mEvents.raise(EventId.acSelected, null, { aircraft: mSelectedAircraft, userSelected: mSelectedByUser } );
            }

            if(selectedByUser) mEvents.raise(EventId.resetTimeOut, null, null);
        }
    };

    // Parses the aircraft list from the server
    this.parseAircraftList = function(aircraftList)
    {
        if(aircraftList.lastDv !== undefined) {
            mDataVersion = aircraftList.lastDv;

            var length = mAircraftList.length;
            for(var i = 0;i < length;++i) {
                mAircraftList[i].OnRadar = false;
            }

            var sortedList = [];
            var autoSelectState = new AutoSelectState(mOptions.autoSelect);
            var reselectAircraft = null;

            length = aircraftList.acList.length;
            for(i = 0;i < length;++i) {
                var aircraftIn = aircraftList.acList[i];
                var ourAircraft = that.findAircraft(aircraftIn.Id);
                var isNewAircraft = ourAircraft === null;
                if(isNewAircraft) ourAircraft = that.addAircraft(aircraftIn.Id);
                sortedList.push(ourAircraft);

                ourAircraft.OnRadar = true;

                ourAircraft.IcaoChanged = aircraftIn.Icao !== undefined;
                if(ourAircraft.IcaoChanged) ourAircraft.Icao = aircraftIn.Icao;

                ourAircraft.BadChanged = aircraftIn.Bad !== undefined;
                if(ourAircraft.BadChanged) ourAircraft.Bad = aircraftIn.Bad;

                ourAircraft.RegChanged = aircraftIn.Reg !== undefined;
                if(ourAircraft.RegChanged) ourAircraft.Reg = aircraftIn.Reg;

                ourAircraft.AltChanged = aircraftIn.Alt !== undefined;
                if(ourAircraft.AltChanged) ourAircraft.Alt = aircraftIn.Alt;

                ourAircraft.CallChanged = aircraftIn.Call !== undefined;
                if(ourAircraft.CallChanged) ourAircraft.Call = aircraftIn.Call;

                ourAircraft.CallSusChanged = aircraftIn.CallSus !== undefined;
                if(ourAircraft.CallSusChanged) ourAircraft.CallSus = aircraftIn.CallSus;

                ourAircraft.LatChanged = aircraftIn.Lat !== undefined;
                if(ourAircraft.LatChanged) ourAircraft.Lat = aircraftIn.Lat;

                ourAircraft.LongChanged = aircraftIn.Long !== undefined;
                if(ourAircraft.LongChanged) ourAircraft.Long = aircraftIn.Long;

                ourAircraft.PosTimeChanged = aircraftIn.PosTime !== undefined;
                if(ourAircraft.PosTimeChanged) ourAircraft.PosTime = aircraftIn.PosTime;

                ourAircraft.SpdChanged = aircraftIn.Spd !== undefined;
                if(ourAircraft.SpdChanged) ourAircraft.Spd = aircraftIn.Spd;

                ourAircraft.TrakChanged = aircraftIn.Trak !== undefined;
                if(ourAircraft.TrakChanged) ourAircraft.Trak = aircraftIn.Trak;

                ourAircraft.TypeChanged = aircraftIn.Type !== undefined;
                if(ourAircraft.TypeChanged) ourAircraft.Type = aircraftIn.Type;

                ourAircraft.MdlChanged = aircraftIn.Mdl !== undefined;
                if(ourAircraft.MdlChanged) ourAircraft.Mdl = aircraftIn.Mdl;

                ourAircraft.CNumChanged = aircraftIn.CNum !== undefined;
                if(ourAircraft.CNumChanged) ourAircraft.CNum = aircraftIn.CNum;

                ourAircraft.LNumChanged = aircraftIn.LNum !== undefined;
                if(ourAircraft.LNumChanged) ourAircraft.LNum = aircraftIn.LNum;

                ourAircraft.FromChanged = aircraftIn.From !== undefined;
                if(ourAircraft.FromChanged) ourAircraft.From = aircraftIn.From;

                ourAircraft.ToChanged = aircraftIn.To !== undefined;
                if(ourAircraft.ToChanged) ourAircraft.To = aircraftIn.To;

                ourAircraft.StopsChanged = aircraftIn.Stops !== undefined;
                if(ourAircraft.StopsChanged) ourAircraft.Stops = aircraftIn.Stops;

                ourAircraft.OpChanged = aircraftIn.Op !== undefined;
                if(ourAircraft.OpChanged) ourAircraft.Op = aircraftIn.Op;

                ourAircraft.OpIcaoChanged = aircraftIn.OpIcao !== undefined;
                if(ourAircraft.OpIcaoChanged) ourAircraft.OpIcao = aircraftIn.OpIcao;

                ourAircraft.SqkChanged = aircraftIn.Sqk !== undefined;
                if(ourAircraft.SqkChanged) ourAircraft.Sqk = aircraftIn.Sqk;

                ourAircraft.HelpChanged = aircraftIn.Help !== undefined;
                if(ourAircraft.HelpChanged) ourAircraft.Help = aircraftIn.Help;

                ourAircraft.VsiChanged = aircraftIn.Vsi !== undefined;
                if(ourAircraft.VsiChanged) ourAircraft.Vsi = aircraftIn.Vsi;

                if(aircraftIn.Dst === undefined) ourAircraft.DstChanged = false;
                else {
                    ourAircraft.PreviousDst = ourAircraft.Dst;
                    ourAircraft.Dst = aircraftIn.Dst;
                    ourAircraft.DstChanged = ourAircraft.PreviousDst !== ourAircraft.Dst;
                }

                if(aircraftIn.Brng === undefined) ourAircraft.BrngChanged = false;
                else {
                    ourAircraft.PreviousBrng = ourAircraft.Brng;
                    ourAircraft.Brng = aircraftIn.Brng;
                    ourAircraft.BrngChanged = ourAircraft.PreviousBrng !== ourAircraft.Brng;
                }

                ourAircraft.WTCChanged = aircraftIn.WTC !== undefined;
                if(ourAircraft.WTCChanged) ourAircraft.WTC = aircraftIn.WTC;

                ourAircraft.EnginesChanged = aircraftIn.Engines !== undefined;
                if(ourAircraft.EnginesChanged) ourAircraft.Engines = aircraftIn.Engines;

                ourAircraft.EngTypeChanged = aircraftIn.EngType !== undefined;
                if(ourAircraft.EngTypeChanged) ourAircraft.EngType = aircraftIn.EngType;

                ourAircraft.SpeciesChanged = aircraftIn.Species !== undefined;
                if(ourAircraft.SpeciesChanged) ourAircraft.Species = aircraftIn.Species;

                ourAircraft.MilChanged = aircraftIn.Mil !== undefined;
                if(ourAircraft.MilChanged) ourAircraft.Mil = aircraftIn.Mil;

                ourAircraft.CouChanged = aircraftIn.Cou !== undefined;
                if(ourAircraft.CouChanged) ourAircraft.Cou = aircraftIn.Cou;

                ourAircraft.HasPicChanged = aircraftIn.HasPic !== undefined;
                if(ourAircraft.HasPicChanged) ourAircraft.HasPic = aircraftIn.HasPic;

                ourAircraft.FlightsCountChanged = aircraftIn.FlightsCount !== undefined;
                if(ourAircraft.FlightsCountChanged) ourAircraft.FlightsCount = aircraftIn.FlightsCount;

                ourAircraft.OnGroundChanged = aircraftIn.Gnd !== undefined;
                if(ourAircraft.OnGroundChanged) ourAircraft.OnGround = aircraftIn.Gnd;

                ourAircraft.SpeedTypeChanged = aircraftIn.SpdTyp !== undefined;
                if(ourAircraft.SpeedTypeChanged) ourAircraft.SpeedType = aircraftIn.SpdTyp;

                ourAircraft.CMsgsChanged = aircraftIn.CMsgs !== undefined;
                if(ourAircraft.CMsgsChanged) ourAircraft.CMsgs = aircraftIn.CMsgs;

                ourAircraft.TagChanged = aircraftIn.Tag !== undefined;
                if(ourAircraft.TagChanged) ourAircraft.Tag = aircraftIn.Tag;

                ourAircraft.ResetTrail = aircraftIn.ResetTrail;
                ourAircraft.TSecs = aircraftIn.TSecs;

                if(aircraftIn.Cot && aircraftIn.Cot.length > 0) ourAircraft.Cot = aircraftIn.Cot;
                else                                            ourAircraft.Cot = null;

                if(aircraftIn.Cos && aircraftIn.Cos.length > 0) ourAircraft.Cos = aircraftIn.Cos;
                else                                            ourAircraft.Cos = null;

                CalculateConvertedFields(ourAircraft);

                if(ourAircraft.getHasPos()) autoSelectState.evaluate(ourAircraft);

                if(mSelectedAircraft !== null && mSelectedAircraft.Id === ourAircraft.Id && !mSelectedAircraft.OnRadar) reselectAircraft = ourAircraft;

                if(isNewAircraft) mEvents.raise(EventId.newAircraft, null, ourAircraft);
            }

            var noLongerTracked = [];
            length = mAircraftList.length;
            for(i = 0;i < length;++i) {
                if(!mAircraftList[i].OnRadar) noLongerTracked.push(mAircraftList[i]);
            }

            mAircraftList = sortedList;
            mEvents.raise(EventId.acListRefreshed, null, mAircraftList);

            if(noLongerTracked.length > 0) mEvents.raise(EventId.noLongerTracked, null, noLongerTracked);

            // Select and deselect aircraft...
            if(mSelectedAircraft !== null && !mSelectedAircraft.OnRadar) {
                if(mOptions.deselectWhenNotTracked) that.selectAircraft(null, false);
            }

            mAutoSelectAircraft = autoSelectState.getSelected();
            if(mAutoSelectAircraft !== null && mAutoSelectAircraft !== mSelectedAircraft && mOptions.autoSelectEnabled) {
                that.selectAircraft(mAutoSelectAircraft, false);
            }

            if(reselectAircraft !== null) that.selectAircraft(reselectAircraft, false);

            if(_MapMode === MapMode.flightSim && mSelectedAircraft === null && mAircraftList.length > 0) {
                that.selectAircraft(mAircraftList[0], false);
            }
        }
    }

    function CalculateConvertedFields(aircraft)
    {
        var convertedDst = aircraft.Dst;
        if(convertedDst !== null) {
            switch(mOptions.distanceUnit) {
                case DistanceUnit.Miles:            convertedDst = Math.floor(convertedDst * 62.1371192) / 100; break;
                case DistanceUnit.Kilometres:       convertedDst = Math.floor(convertedDst * 100) / 100; break;
                case DistanceUnit.NauticalMiles:    convertedDst = Math.floor(convertedDst * 53.9956803) / 100; break;
            }
        }
        aircraft.ConvertedDstChanged = convertedDst !== aircraft.ConvertedDst;
        aircraft.ConvertedDst = convertedDst;

        var convertedAlt = aircraft.Alt;
        if(convertedAlt !== null) {
            switch(mOptions.heightUnit) {
                case HeightUnit.Feet:           break;
                case HeightUnit.Metres:         convertedAlt = Math.round(convertedAlt * 0.3048); break;
            }
        }
        aircraft.ConvertedAltChanged = convertedAlt !== aircraft.ConvertedAlt;
        aircraft.ConvertedAlt = convertedAlt;

        var convertedSpd = aircraft.Spd;
        if(convertedSpd !== null) {
            switch(mOptions.speedUnit) {
                case SpeedUnit.Knots:               convertedSpd = Math.round(convertedSpd); break;
                case SpeedUnit.MilesPerHour:        convertedSpd = Math.round(convertedSpd * 1.15077945); break;
                case SpeedUnit.KilometresPerHour:   convertedSpd = Math.round(convertedSpd * 1.852); break;
            }
        }
        aircraft.ConvertedSpdChanged = convertedSpd !== aircraft.ConvertedSpd;
        aircraft.ConvertedSpd = convertedSpd;

        var convertedVsi = aircraft.Vsi;
        if(convertedVsi !== null) {
            switch(mOptions.getVsiUnit()) {
                case HeightUnit.Feet:
                    if(mOptions.verticalSpeedPerSecond) convertedVsi = Math.round(convertedVsi / 60);
                    break;
                case HeightUnit.Metres:
                    if(mOptions.verticalSpeedPerSecond) convertedVsi = Math.round(convertedVsi * 0.00508);
                    else convertedVsi = Math.round(convertedVsi * 0.3048);
                    break;
            }
        }
        aircraft.ConvertedVsiChanged = convertedVsi !== aircraft.ConvertedVsi;
        aircraft.ConvertedVsi = convertedVsi;

        var flightLevel = aircraft.Alt;
        if(flightLevel !== null) {
            if(flightLevel < mOptions.tempFlightLevelTransitionAltitudeFeet) {
                flightLevel = aircraft.ConvertedAlt;
            } else {
                switch(mOptions.flightLevelHeightUnit) {
                    case HeightUnit.Feet:           break;
                    case HeightUnit.Metres:         flightLevel = Math.round(flightLevel * 0.3048); break;
                }
                flightLevel = 'FL' + Math.max(0, Math.round(flightLevel / 100));
            }
        }
        aircraft.FlightLevelChanged = flightLevel !== aircraft.FlightLevel;
        aircraft.FlightLevel = flightLevel;
    }
}