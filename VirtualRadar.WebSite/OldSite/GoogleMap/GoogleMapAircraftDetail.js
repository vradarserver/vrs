var GoogleMapAircraftDetailObjects = [];

function googleMapAircraftDetailSelectClosest(idx)          { GoogleMapAircraftDetailObjects[idx].selectClosestClicked(); }
function googleMapAircraftDetailStopSelecting(idx)          { GoogleMapAircraftDetailObjects[idx].stopSelectingClicked(); }
function googleMapAircraftDetailToggleAutoSelect(idx)       { GoogleMapAircraftDetailObjects[idx].toggleAutoSelectClicked(); }
function googleMapAircraftDetailToggleReverseGeocode(idx)   { GoogleMapAircraftDetailObjects[idx].toggleReverseGeocodeClicked(); }

function GoogleMapAircraftDetail(mapMode, events, options, aircraftCollection, showReverseGeocode, showCurrentLocation, currentLocationKnown)
{
    var that = this;
    var mGlobalIndex = GoogleMapAircraftDetailObjects.length;
    GoogleMapAircraftDetailObjects.push(that);

    var mLastReverseGeocodeResponse = "Position lookup disabled";
    var mCurrentAircraft = null;
    var mMapMode = mapMode;
    var mEvents = events;
    var mOptions = options;
    var mAircraftCollection = aircraftCollection;
    var mShowReverseGeocode = showReverseGeocode;
    var mShowDebugCountPoints = false;
    var mShowCurrentLocation = showCurrentLocation;
    var mCurrentLocationKnown = currentLocationKnown;

    this.setDebugCountPoints = function(value) { mShowDebugCountPoints = value; };

    var canvasElement = document.getElementById("detail_canvas");
    canvasElement.style.display = "block";

    var contentElement = document.createElement("div");
    contentElement.setAttribute("id", "detail_content");
    contentElement.style.display = "none";
    canvasElement.appendChild(contentElement);

    createContentSkeleton();
    var registrationElement = document.getElementById('detailRegistration');
    var icaoElement = document.getElementById('detailIcao');
    var opFlagElement = document.getElementById('detailOpFlag');
    var operatorElement = document.getElementById('detailOperator');
    var callsignElement = document.getElementById('detailCallsign');
    var modelElement = document.getElementById('detailModel');
    var typeElement = document.getElementById('detailType');
    var countryElement = document.getElementById('detailICAOCountry');
    var civOrMilElement = document.getElementById('detailCivOrMil');
    var routeElement = document.getElementById('detailRoute');
    var bearingElement = document.getElementById('detailBearing');
    var positionElement = document.getElementById('detailPosition');
    var linksElement = document.getElementById('detailLinks');
    var pictureElement = document.getElementById('detailPicture');

    var visibilityElement = document.createElement("div");
    visibilityElement.setAttribute("id", "detail_select");
    var visibilityElementHtml = "";
    if(mMapMode !== MapMode.flightSim) {
        visibilityElementHtml += 
            "<a href='#' id='detail_toggleAutoSelect' onClick='googleMapAircraftDetailToggleAutoSelect(" + mGlobalIndex + ")'></a>" +
            "&nbsp;:: <a href='#' onClick='googleMapAircraftDetailSelectClosest(" + mGlobalIndex + ")'>Select closest</a>" +
            "<span id='detail_hideSelectionLink'> :: <a href='#' onClick='googleMapAircraftDetailStopSelecting(" + mGlobalIndex + ")'>Hide selection</a></span>" +
            " :: ";
    }
    visibilityElementHtml += '<a id="detail_pageHelp" href="http://www.virtualradarserver.co.uk/OnlineHelp/DesktopGoogleMap.aspx" target="help">Page help</a>';
    visibilityElement.innerHTML = visibilityElementHtml;
    canvasElement.appendChild(visibilityElement);

    showDetail(false);
    setToggleAutoSelectText(mOptions.autoSelectEnabled);

    mEvents.addListener(EventId.reverseGeocodeResponseChanged, reverseGeocodeResponseHandler);
    mEvents.addListener(EventId.autoSelectChanged, autoSelectChangedHandler);
    mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);
    mEvents.addListener(EventId.acListRefreshed, refreshContentHandler);
    mEvents.addListener(EventId.acSelected, aircraftSelectedHandler);
    mEvents.addListener(EventId.currentLocationChanged, currentLocationChangedHandler);

    function autoSelectChangedHandler(sender, args) { setToggleAutoSelectText(args); };
    function optionsChangedHandler(sender, args) { refresh(); };
    function refreshContentHandler(sender, args) { refresh(); };
    function aircraftSelectedHandler(sender, args) { showAircraft(args.aircraft); };
    function currentLocationChangedHandler(sender, args) { mCurrentLocationKnown = true; };
    function reverseGeocodeResponseHandler(sender, args)
    {
        mLastReverseGeocodeResponse = args;
        var span = document.getElementById("rgeocode_result");
        if(span !== null) span.innerHTML = mLastReverseGeocodeResponse;
    };

    this.selectClosestClicked = function()      { mEvents.raise(EventId.selectClosestClicked, null, null); };
    this.stopSelectingClicked = function()      { mAircraftCollection.selectAircraft(null, true); };
    this.toggleAutoSelectClicked = function()   { mEvents.raise(EventId.toggleAutoSelectClicked, null, null); };
    this.toggleReverseGeocodeClicked = function()
    {
        mEvents.raise(EventId.resetTimeOut, null, null);
        mEvents.raise(EventId.toggleReverseGeocode, null, null);
    };

    function createContentSkeleton()
    {
        var html = '';
        html += '<table class="detailHeadingTable"><tr>' +
                '<td id="detailRegistration" class="detailHeading detailRegistration"></td>' +
                '<td id="detailIcao" class="detailHeading detailIcao"></td>' +
                '<td id="detailBearing" class="detailBearing"></td>' +
                '<td id="detailOpFlag" class="detailOpFlag"></td>' +
                '</tr></table>';

        html += '<table class="detailSummaryTable"><tr>' +
                '<td id="detailOperator" class="detailHeading detailOperator"></td>' +
                '<td id="detailCallsign" class="detailHeading detailCallsign"></td>' +
                '</tr><tr>' +
                '<td id="detailICAOCountry" class="detailHeading detailICAOCountry"></td>' +
                '<td id="detailCivOrMil" class="detailHeading detailCivOrMil"></td>' +
                '</tr><tr>' +
                '<td id="detailModel" class="detailHeading detailModel"></td>' +
                '<td id="detailType" class="detailHeading detailType"></td>' +
                '</tr></table>';

        html += '<div id="detailRoute"></div>';

        html += '<div id="detailPosition" class="detailPositionLine"></div>';

        if(mMapMode !== MapMode.flightSim) html += '<div id="detailPicture"></div>';

        if(mMapMode !== MapMode.flightSim) html += '<div id="detailLinks" class="detailLinkList"></div>';

        contentElement.innerHTML = html;
    }

    function refresh()
    {
        showAircraft(mCurrentAircraft);
    };

    function showDetail(show)
    {
        var hideSelectionElement = document.getElementById("detail_hideSelectionLink");
        if(hideSelectionElement !== null) {
            if(show) {
                contentElement.style.display = "block";
                hideSelectionElement.style.display = "inline";
            } else {
                contentElement.style.display = "none";
                hideSelectionElement.style.display = "none";
            }
        }
    };

    function setToggleAutoSelectText(autoSelectSetting)
    {
        var linkElement = document.getElementById("detail_toggleAutoSelect");
        if(linkElement !== null) {
            linkElement.innerHTML = autoSelectSetting ? "Disable auto-select" : "Enable auto-select";
        }
    };

    function showAircraft(aircraft)
    {
        var html;

        var currentGeocode = null;
        var geocodeElement = document.getElementById('rgeocode_result');
        if(geocodeElement != null) currentGeocode = geocodeElement.innerHTML;

        var refreshContent = mCurrentAircraft !== aircraft;
        mCurrentAircraft = aircraft;

        if(aircraft === null) showDetail(false);
        else {
            mEvents.raise(EventId.aircraftDetailRefresh, this, aircraft);

            if(refreshContent || aircraft.RegChanged) registrationElement.innerHTML = aircraft.formatRegReportLink();
            if(refreshContent || aircraft.IcaoChanged) icaoElement.innerHTML = aircraft.formatIcaoReportLink();
            if(refreshContent || aircraft.OpIcaoChanged) {
                opFlagElement.innerHTML = '<img src="Images/File-' + encodeURIComponent(aircraft.OpIcao == '' ? 'Blank' : aircraft.OpIcao) + '/OpFlag.png" width="' + mOptions.tempFlagWidth + 'px" height="' + mOptions.tempFlagHeight + 'px">';
            }
            if(refreshContent || aircraft.OpChanged) operatorElement.innerHTML = aircraft.Op;
            if(refreshContent || aircraft.CallChanged || aircraft.CallSusChanged) {
                callsignElement.innerHTML = aircraft.formatCallsignReportLink() + (aircraft.CallSus ? "*" : "");
                callsignElement.setAttribute("title", aircraft.CallSus ? "Callsign may not be correct" : "");
            }
            if(refreshContent || aircraft.MdlChanged) modelElement.innerHTML = aircraft.Mdl;
            if(refreshContent || aircraft.TypeChanged) typeElement.innerHTML = aircraft.Type;
            if(refreshContent || aircraft.CouChanged) countryElement.innerHTML = aircraft.Cou;
            if(refreshContent || aircraft.MilChanged) civOrMilElement.innerHTML = aircraft.Mil === undefined ? '' : aircraft.Mil ? 'Military' : '';

            if(refreshContent || (aircraft.FromChanged || aircraft.ToChanged || aircraft.StopsChanged ||
                                  aircraft.CallChanged ||
                                  currentGeocode !== mLastReverseGeocodeResponse ||
                                  aircraft.WTCChanged || aircraft.EnginesChanged || aircraft.EngTypeChanged || aircraft.SpeciesChanged ||
                                  aircraft.FlightsCountChanged)) {
                html = '<table class="detailRouteTable">';

                html += '<tr><td class="detailRouteHead">Type:</td><td class="detailRouteDetail">' + capitaliseSentence(aircraft.formatIcao8643Details()) + '</td></tr>';

                if(aircraft.FlightsCount !== null && aircraft.FlightsCount !== 0) {
                    var flightsCountText = aircraft.FlightsCount + ' time' + (aircraft.FlightsCount === 1 ? '' : 's');
                    html += '<tr><td class="detailRouteHead">Seen:</td><td class="detailRouteDetail">' + flightsCountText + '</td></tr>';
                }
                html += '<tr><td class="detailRouteHead">Time Tracked:</td><td class="detailRouteDetail"><span id="detailTimeTracked"></span></td></tr>';

                var showRouteSdmLink = true;
                if(aircraft.From === '' && aircraft.To === '') {
                    showRouteSdmLink = false;
                    html += '<tr><td class="detailRouteHead">Route:</td><td class="detailRouteDetail">';
                    if(aircraft.Call === '') html += 'Aircraft is not transmitting its callsign';
                    else html += aircraft.formatLinkToSubmitRoute('Unknown');
                    html += '</td></tr>';
                } else {
                    html += '<tr><td class="detailRouteHead">From:</td><td class="detailRouteDetail">' + aircraft.From + '</td></tr>';
                    if(aircraft.Stops.length > 0) {
                        html += '<tr><td class="detailRouteHead">Via:</td><td class="detailRouteDetail">';
                        for(var i = 0;i < aircraft.Stops.length;i++) {
                            if(i > 0) html += '<br />';
                            html += aircraft.Stops[i];
                        }
                        html += '</td></tr>';
                    }
                    html += '<tr><td class="detailRouteHead">To:</td><td class="detailRouteDetail">' + aircraft.To + '</td></tr>';
                }
                if(mShowReverseGeocode) {
                    html += '<tr><td class="detailRouteHead">Above:</td><td class="detailRouteDetail">';
                    if(aircraft.getHasPos()) html += '<span id="rgeocode_result">' + mLastReverseGeocodeResponse + '</span>';
                    else html += 'Aircraft is not transmitting its position';
                    html += '</td></tr>';
                    html += '<td class="detailRouteHead"></td><td class="detailRouteDetail"><a href="#" onClick="googleMapAircraftDetailToggleReverseGeocode(' + mGlobalIndex + ')">Toggle position lookup</a></td></tr>';
                }
                if(showRouteSdmLink) html += '<tr><td class="detailRouteHead"></td><td class="detailRouteDetail">' + aircraft.formatLinkToSubmitRoute() + '</td></tr>';

                html += '</table>';

                routeElement.innerHTML = html;
            }

            if(refreshContent || aircraft.BrngChanged) bearingElement.innerHTML = aircraft.formatBearingCompass(mCurrentLocationKnown);
            document.getElementById('detailTimeTracked').innerHTML = aircraft.formatTrackedSeconds();

            if(refreshContent || (aircraft.ConvertedAltChanged ||
                                  aircraft.ConvertedVsiChanged ||
                                  aircraft.ConvertedSpdChanged ||
                                  aircraft.SpeedTypeChanged ||
                                  aircraft.TrakChanged ||
                                  aircraft.LatChanged ||
                                  aircraft.LongChanged ||
                                  aircraft.ConvertedDstChanged ||
                                  aircraft.BrngChanged ||
                                  aircraft.SqkChanged ||
                                  aircraft.HelpChanged ||
                                  aircraft.OnGroundChanged)) {
                var positionLine = "";
                if(aircraft.ConvertedAlt !== null || aircraft.OnGround) {
                    var vsi = aircraft.ConvertedVsi;
                    var noVsi = vsi === null || vsi === 0;
                    var alt = aircraft.ConvertedAlt;
                    if(aircraft.OnGround) positionLine += "On the ground";
                    else {
                        if(noVsi) positionLine += "Flying at ";
                        else {
                            if(vsi > 0) positionLine += "Climbing ";
                            else        positionLine += "Descending ";
                            positionLine += alt <= 0 ? "from " : "through ";
                        }
                        positionLine += aircraft.formatConvertedAlt();
                        if(!noVsi) positionLine += " at " + aircraft.formatAbsConvertedVsi();
                    }
                }
                if(aircraft.ConvertedSpd !== null) {
                    var speedType = aircraft.formatSpeedType(positionLine === "");
                    if(positionLine !== "") positionLine += ", ";
                    positionLine += speedType + " " + aircraft.formatConvertedSpd();
                }
                if(aircraft.Trak !== null) {
                    if(positionLine !== "") positionLine += ", h";
                    else positionLine += "H";
                    positionLine += "eading " + getTrackDescription(aircraft.Trak) + " " + aircraft.Trak + "&deg;";
                }
                if(mShowCurrentLocation && aircraft.getHasPos() && aircraft.ConvertedDst !== null) {
                    if(positionLine !== "") positionLine += ", ";
                    positionLine += aircraft.formatConvertedDst();
                    if(aircraft.Brng !== null) positionLine += " " + getTrackDescription(aircraft.Brng);
                    positionLine += (aircraft.Brng !== null ? " of " : " from ");
                    positionLine += (!mCurrentLocationKnown ? "map centre" : "here");
                    if(aircraft.Brng !== null) positionLine += " at " + aircraft.Brng + "&deg;";
                }
                if(aircraft.Sqk !== "") {
                    if(positionLine !== "") positionLine += ". ";
                    positionLine += "<span class='detailSquawkLine'>Squawking " + aircraft.Sqk;
                    if(aircraft.Help) {
                        positionLine += " <b>";
                        if(aircraft.Sqk == "7500") positionLine += "(unlawful interference)";
                        else if(aircraft.Sqk == "7600") positionLine += "(lost communications)";
                        else if(aircraft.Sqk == "7700") positionLine += "(general emergency)";
                        positionLine += "</b>";
                    }
                    positionLine += "</span>";
                }
                if(positionLine.length > 0) positionLine += '.';

                if(mShowDebugCountPoints) {
                    var debugArgs = { aircraft: aircraft, result: '' };
                    mEvents.raise(EventId.debugGetPointText, null, debugArgs);
                    positionLine += '<br/>' + debugArgs.result;
                }

                positionElement.innerHTML = positionLine;
            }

            if(mMapMode !== MapMode.flightSim) {
                if(refreshContent || aircraft.HasPicChanged) {
                    html = '';
                    if(mOptions.tempShowPictures && aircraft.HasPic && aircraft.Reg) {
                        html = aircraft.formatPicture(null, 'detailPictureImg', true, 'detail');
                    }
                    pictureElement.innerHTML = html;
                    pictureElement.style.display = html == '' ? 'none' : 'block';
                }

                if(refreshContent || aircraft.RegChanged) {
                    html = '';
                    if(aircraft.Reg != null && aircraft.Reg != "") {
                        var afr = aircraft.getAFR();
                        html += '<a href="http://www.airport-data.com/aircraft/' + encodeURI(aircraft.Reg.replace('+', ' ', 'g')) + '.html" target="_airportData">www.airport-data.com</a>';
                        html += ' :: <a href="http://www.airliners.net/search/photo.search?regsearch=' + aircraft.Reg + '" target="_airlinersnet">www.airliners.net</a>';
                        html += ' :: <a href="https://www.planelogger.com/aircraft/AdsbView/' + aircraft.Reg + '" target="_planelogger">www.planelogger.com</a>';
                        if(afr.length > 0) html += ' :: <a href="http://www.airframes.org/reg/' + afr + '" target="_airframesorg">www.airframes.org</a>';
                    }
                    linksElement.innerHTML = html;
                }
            }

            showDetail(true);
        }
    };
}
