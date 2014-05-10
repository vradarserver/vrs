var PinText = {
    None: 0,
    Registration: 1,
    Callsign: 2,
    Type: 3,
    ICAO: 4,
    Squawk: 5,
    OperatorCode: 6,
    Altitude: 7,
    FlightLevel: 8,
    Speed: 9,
    Route: 10
};

function GoogleMapMarker(options, map, pinClickHandler)
{
    var that = this;
    var mOptions = options;
    var mMap = map;
    var mMarkerOrigin = new google.maps.Point(0, 0);
    var mMarkerSizeAndAnchors = [];
    var mPinClickHandler;
    var mUseMarkerWithLabel = _ServerConfig.isMono;
    var mLabelText = '';
    
    var mMarker;
    var markerOptions = {
        position: new google.maps.LatLng(0, 0),
        map: mMap,
        clickable: true,
        flat: true,
        visible: false,
        zIndex: 100,
        optimized: false
    };
    
    if(!mUseMarkerWithLabel) mMarker = new google.maps.Marker(markerOptions);
    else {
        markerOptions.labelInBackground = true;
        markerOptions.labelClass = 'markerLabel';
        mMarker = new MarkerWithLabel(markerOptions);
    }
    if(mMarker._VirtualRadarServerMarkerReference !== undefined) throw "This version of Google Maps already uses the name _VirtualRadarServerMarkerReference";
    mMarker._VirtualRadarServerMarkerReference = that;
    var mClickListener = pinClickHandler !== undefined ? google.maps.event.addListener(mMarker, "click", pinClickHandler) : null;
    this.getAnchor = function() { return mMarker; };

    var mNormalTraceColour = "#000040";
    var mSelectedTraceColour = "#202080";
    var mLine = new google.maps.Polyline({strokeColor: mNormalTraceColour, strokeOpacity: 1.0, strokeWeight: 2});
    var mLastFullTrailTrack = null;
    var mSecondLastFullTrailTrack = null;
    var mShortTrailTimes = [];
    var mShowingShortTrail = false;

    var mImageUrl = null;
    var mAircraft = null;
    var mTooltip = null;

    this.getAircraft = function() { return mAircraft; };
    this.getIsInUse = function() { return mAircraft !== null; };

    this.show = function() { if(mAircraft !== null) { showPin(); if(mOptions.showTrace(mAircraft.Selected)) showTrace(); else hideTrace(); } };
    this.hide = function() { hidePin(); hideTrace(); };

    this.initialiseForAircraft = function(aircraft)
    {
        mAircraft = aircraft;
        setPosition(aircraft.Lat, aircraft.Long);
        setTooltip();
        mImageUrl = null;

        that.refreshPinImage();
        setTraceAppearance();
        that.show();
    };

    this.cloneToPin = function(otherPin)
    {
        otherPin.setPosition(mMarker.getPosition());
        otherPin.setIcon(mMarker.getIcon());
    };

    this.cloneToLine = function(otherLine)
    {
        setAnyTraceAppearance(otherLine);

        var path = mLine.getPath();
        var pathLength = path.getLength();
        var otherPath = otherLine.getPath();
        var otherPathLength = otherPath.getLength();

        while(otherPathLength > pathLength) {
            otherPath.pop();
            --otherPathLength;
        }

        for(var i = 0;i < pathLength;++i) {
            var point = path.getAt(i);
            var otherPoint = point;
            if(i < otherPathLength) otherPath.setAt(i, otherPoint);
            else {
                otherPath.push(otherPoint);
                ++otherPathLength;
            }
        }

        otherLine.setPath(otherPath);
    };

    this.makeDormant = function()
    {
        that.hide();
        mAircraft = null;
    };

    this.refresh = function()
    {
        if(mAircraft.LatChanged || mAircraft.LongChanged) setPosition(mAircraft.Lat, mAircraft.Long);
        setTooltip();
        that.refreshPinImage();
    };

    function showPin() { mMarker.setVisible(true);  };
    function hidePin() { mMarker.setVisible(false); };

    function setPosition(latitude, longitude)
    {
        mMarker.setPosition(new google.maps.LatLng(latitude, longitude));
    };

    function setTooltip(tooltipText)
    {
        var newTooltip = buildAircraftTooltipText();
        if(mTooltip !== newTooltip) {
            mTooltip = newTooltip;
            mMarker.setTitle(newTooltip);
        }
    };

    function buildAircraftTooltipText()
    {
        var result = '';
        if(_MapMode !== MapMode.iPhone) {
            result = mAircraft.Icao;
            if(mAircraft.Type && mAircraft.Type.length) result += ', ' + mAircraft.Type;
            if(mAircraft.Reg && mAircraft.Reg.length) result += ', ' + mAircraft.Reg;
            if(mAircraft.Call && mAircraft.Call.length) result += ', ' + mAircraft.Call;
            if(mAircraft.Op && mAircraft.Op.length) result += ', operated by ' + mAircraft.Op;
            if(mAircraft.Call && mAircraft.Call.length) result += '. ' + mAircraft.formatRouteTooltip();
            result += '.';
        }

        return result;
    };

    this.refreshPinImage = function()
    {
        if(mAircraft !== null) {
            var track = mAircraft.Trak;
            if(!isNaN(track)) track = Math.round(track / 5) * 5;
            if(isNaN(track)) track = 0;

            var useSimpleImages = mOptions.simplePinsWhenZoomedOut && mOptions.mapZoom < 7;
            var showAltitudeStalk = mOptions.showAltitudeStalk && !useSimpleImages;
            var showPinText = mOptions.canShowPinText() && !useSimpleImages;

            var altitudeUnits = mAircraft.Alt;
            if(altitudeUnits === 0 || altitudeUnits < 0) altitudeUnits = 0;
            if(altitudeUnits > 35000) altitudeUnits = 35000;
            altitudeUnits = Math.round(altitudeUnits / 2500);

            var altitudeStalkHeight = showAltitudeStalk ? 2 : 0;

            var altitude = altitudeUnits * 5;
            if(!showAltitudeStalk) altitude = 0;

            var imageHeight = 35 + altitude + altitudeStalkHeight;
            var imageWidth = 35;

            var isSelected = mAircraft.Selected;
            var imageUrl = 'Images/Rotate-' + track;
            var labelText = !mUseMarkerWithLabel ? null : '';

            if(showPinText) {
                var pinTextLines = 0;
                for(var i = 0;i < mOptions.pinTextLines.length;++i) {
                    var pinText = mOptions.pinTextLines[i];
                    if(pinText == PinText.None) continue;
                    ++pinTextLines;

                    var content = '';
                    switch(pinText) {
                        case PinText.Registration:  content = mAircraft.Reg; break;
                        case PinText.Callsign:      content = mAircraft.Call; break;
                        case PinText.Type:          content = mAircraft.Type; break;
                        case PinText.ICAO:          content = mAircraft.Icao; break;
                        case PinText.Squawk:        content = mAircraft.Sqk; break;
                        case PinText.OperatorCode:  content = mAircraft.OpIcao; break;
                        case PinText.Altitude:      content = mAircraft.ConvertedAlt === null || mAircraft.ConvertedAlt === undefined ? null : mAircraft.ConvertedAlt.toString(); break;
                        case PinText.FlightLevel:   content = mAircraft.formatFlightLevel(); break;
                        case PinText.Speed:         content = mAircraft.ConvertedSpd === null || mAircraft.ConvertedSpd === undefined ? null : mAircraft.ConvertedSpd.toString(); break;
                        case PinText.Route:         content = mAircraft.formatRoutePinText(); break;
                    }
                    if(!mUseMarkerWithLabel) imageUrl += '/PL' + (i + 1) + '-' + encodeURIComponent(content ? content : "");
                    else if(content && content.length)  {
                        if(labelText.length !== 0) labelText += '<br />';
                        labelText += '<span class="markerLabelText">&nbsp;' + normaliseHtml(content) + '&nbsp;</span>';
                    }
                }
                if(pinTextLines > 0 && !mUseMarkerWithLabel) {
                    // The shadow text effect works best if the height and width are exactly divisible by 4
                    imageUrl += "/Wdth-68";
                    imageWidth = 68;
                    if(showAltitudeStalk) {
                        altitude = (altitudeUnits * 4) + (pinTextLines * 12) + 3; // 3 is just to get overall height % 4 == 0
                        imageHeight = 35 + altitude + altitudeStalkHeight;
                    } else {
                        imageHeight = 36 + (pinTextLines * 12);
                    }
                }
            }

            var centrePin = Math.floor(imageWidth / 2);
            if(showAltitudeStalk) imageUrl += "/Alt-" + altitude;
            imageUrl += "/Hght-" + imageHeight;
            imageUrl += "/CenX-" + centrePin;

            imageUrl += '/' + (isSelected ? 'AirplaneSelected' : 'Airplane') + '.png';

            if(mImageUrl !== imageUrl || (mUseMarkerWithLabel && mLabelText !== labelText)) {
                mImageUrl = imageUrl;
                mLabelText = labelText;
                mMarker.setIcon(createMarkerIcon(imageUrl, centrePin, !showAltitudeStalk ? 18 : imageHeight - 3, imageWidth, imageHeight, labelText));
                if(mUseMarkerWithLabel) {
                    if(!labelText || labelText === '') mMarker.set('labelVisible', false);
                    else {
                        if(!mMarker.get('labelVisible')) mMarker.set('labelVisible', true);
                        mMarker.set('labelContent', labelText);
                    }
                }
            }
        }
    };

    function createMarkerIcon(imageUrl, centrePinX, centrePinY, imageWidth, imageHeight, labelText)
    {
        var markerSize = null;
        var markerAnchor = null;
        var labelAnchor = null;

        var length = mMarkerSizeAndAnchors.length;
        var sizeAndAnchor = null;
        for(var i = 0;i < length;++i) {
            sizeAndAnchor = mMarkerSizeAndAnchors[i];
            if(sizeAndAnchor.size.width === imageWidth && sizeAndAnchor.size.height === imageHeight &&
               sizeAndAnchor.anchor.x === centrePinX && sizeAndAnchor.anchor.y === centrePinY) {
                markerSize = sizeAndAnchor.size;
                markerAnchor = sizeAndAnchor.anchor;
                labelAnchor = sizeAndAnchor.labelAnchor;
                break;
            }
        }

        if(markerSize === null) {
            markerSize = new google.maps.Size(imageWidth, imageHeight);
            markerAnchor = new google.maps.Point(centrePinX, centrePinY);
            labelAnchor = !mUseMarkerWithLabel ? null : new google.maps.Point(imageWidth - centrePinX, centrePinY - imageHeight);
            mMarkerSizeAndAnchors.push({size: markerSize, anchor: markerAnchor, labelAnchor: labelAnchor});
        }

        if(mUseMarkerWithLabel) mMarker.set('labelAnchor', labelAnchor);
        
        return new google.maps.MarkerImage(imageUrl, markerSize, mMarkerOrigin, markerAnchor);
    };

    function showTrace() { mLine.setMap(mMap); };
    function hideTrace() { mLine.setMap(null); };

    // Sets the path to be a full trace path sent from the server
    this.setFullTrailPoints = function(resetTrail, tracePoints)
    {
        var path = mLine.getPath();
        if(resetTrail) initialiseForFullTrail(path);

        var length = tracePoints.length;
        for(var i = 0;i < length;i += 3) {
            addFullTrailPoint(path, tracePoints[i], tracePoints[i+1], tracePoints[i+2]);
        }
    };

    function initialiseForFullTrail(path)
    {
        var pathCount = path.getLength();
        while(pathCount-- > 0) path.pop();
        mLastFullTrailTrack = null;
        mSecondLastFullTrailTrack = null;
        mShowingShortTrail = false;
    };

    function addFullTrailPoint(path, lat, lng, track)
    {
        track = track === null ? null : Math.round(track);

        var latLng = new google.maps.LatLng(lat, lng);
        if(path.length > 1 && mLastFullTrailTrack !== null && mSecondLastFullTrailTrack !== null && mLastFullTrailTrack === mSecondLastFullTrailTrack && mLastFullTrailTrack === track) {
            path.setAt(path.length - 1, latLng);
        } else {
            path.push(latLng);
            mSecondLastFullTrailTrack = mLastFullTrailTrack;
            mLastFullTrailTrack = track;
        }
    };

    // Sets tha path to be a short trace path sent from the server
    this.setShortTrailPoints = function(resetTrail, tracePoints, shortTrailThresholdTicks)
    {
        var path = mLine.getPath();
        if(resetTrail) initialiseForShortTrail(path);

        var length = path.length;
        if(length > 0 && length == mShortTrailTimes.length) {
            var removeCount = 0;
            for(var c = 0;c < length;c++) {
                if(mShortTrailTimes[c] >= shortTrailThresholdTicks) break;
                ++removeCount;
            }
            if(removeCount > 0) {
                mShortTrailTimes.splice(0, removeCount);
                for(var i = 0;i < removeCount;++i) {
                    path.removeAt(0);
                }
            }
        }

        length = tracePoints.length;
        for(var i = 0;i < length;i += 3) {
            var ticks = tracePoints[i+2];
            if(ticks >= shortTrailThresholdTicks) addShortTrailPoint(path, tracePoints[i], tracePoints[i+1], ticks);
        }
    }

    function initialiseForShortTrail(path)
    {
        var pathCount = path.getLength();
        while(pathCount-- > 0) path.pop();
        mShortTrailTimes = [];
        mShowingShortTrail = true;
    };

    function addShortTrailPoint(path, lat, lng, utcMilliseconds)
    {
        var latLng = new google.maps.LatLng(lat, lng);
        path.push(latLng);
        mShortTrailTimes.push(utcMilliseconds);
    };

    this.eraseTrace = function()
    {
        hideTrace();
        mShortTrailTimes = [];
        mLastFullTrailTrack = null;
        mSecondLastFullTrailTrack = null;
        var path = mLine.getPath();
        var pathCount = path.getLength();
        while(pathCount-- > 0) path.pop();
    };

    function setTraceAppearance() { setAnyTraceAppearance(mLine); };
    function setAnyTraceAppearance(line)
    {
        var isSelected = mAircraft.Selected;
        line.setOptions({
            strokeColor: isSelected ? mSelectedTraceColour : mNormalTraceColour,
            strokeWeight: isSelected ? 3 : 2});
    };

    this.applyOptions = function()
    {
        that.refreshPinImage();
        setTraceAppearance();
        switch(mOptions.trace) {
            case TraceType.None:            hideTrace(); break;
            case TraceType.All:             showTrace(); break;
            case TraceType.JustSelected:    if(mAircraft.Selected) showTrace(); else hideTrace(); break;
            default:                        alert("Unknown TraceType option"); break;
        }
    };

    this.getDebugCountPoints = function()
    {
        var result = mLine.getPath().getLength().toString();
        if(mShortTrailTimes) result += ' (' + mShortTrailTimes.length + ')';
        return result;
    };

    this.getDebugPointText = function()
    {
        var result = '';
        if(!mShowingShortTrail) result = 'Showing full trail';
        else {
            result += 'Path length: ' + mLine.getPath().length + ', times length: ' + mShortTrailTimes.length + '<br/>';
            result += 'Server time: ' + mOptions.tempServerTime.getTime() + '<br/>';
            for(var i = 0;i < mShortTrailTimes.length;++i) {
                result += i.toString() + ': ' +
                          mShortTrailTimes[i] +
                          ' diff: ' + (mOptions.tempServerTime.getTime() - mShortTrailTimes[i]) + ' ms' +
                          '<br/>';
            }
        }

        return result;
    }
}
