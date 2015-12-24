function ReportMap(cookieSuffix)
{
    var that = this;
    var mCookieSuffix = cookieSuffix;
    var mMapLatitude = _ServerConfig.initialLatitude;
    var mMapLongitude = _ServerConfig.initialLongitude;
    var mMapType = _ServerConfig.initialMapType;
    var mMapZoom = _ServerConfig.initialZoom;
    var mMap = null;
    var mStartPin = null;
    var mEndPin = null;
    var mLine = new google.maps.Polyline({strokeColor: '#000040', strokeOpacity: 1.0, strokeWeight: 2});

    _Events.addListener(EventId.displayFlightPath, displayFlightPathHandler);

    loadCookies();

    function loadCookies()
    {
        var cookies = readCookieValues();
        for(var c = 0;c < cookies.length;c++) {
            var valuePair = cookies[c];
            switch(valuePair.name) {
                // These four are obsolete
                case 'repMapLatitude' + mCookieSuffix:  mMapLatitude = Number(valuePair.value); break;
                case 'repMapLongitude' + mCookieSuffix: mMapLongitude = Number(valuePair.value); break;
                case 'repMapType' + mCookieSuffix:      mMapType = valuePair.value; break;
                case 'repMapZoom' + mCookieSuffix:      mMapZoom = Number(valuePair.value); break;

                // These are the real options
                case 'reportMapOptions':
                    var nameValues = new nameValueCollection();
                    nameValues.fromString(valuePair.value);
                    mMapLatitude = nameValues.getValueAsNumber('lat' + mCookieSuffix, mMapLatitude);
                    mMapLongitude = nameValues.getValueAsNumber('lng' + mCookieSuffix, mMapLongitude);
                    mMapType = nameValues.getValueAsString('typ' + mCookieSuffix, mMapType);
                    mMapZoom = nameValues.getValueAsNumber('zoo' + mCookieSuffix, mMapZoom);

                    c = cookies.length;
                    break;
            }
        }
    };

    function eraseOldCookies()
    {
        eraseCookie('repMapLatitude' + mCookieSuffix);
        eraseCookie('repMapLongitude' + mCookieSuffix);
        eraseCookie('repMapType' + mCookieSuffix);
        eraseCookie('repMapZoom' + mCookieSuffix);
    };

    function saveCookies()
    {
        eraseOldCookies();

        var cookieValues = readCookieValues();

        var nameValues = new nameValueCollection();
        nameValues.fromString(extractCookieValue(cookieValues, 'reportMapOptions'));

        nameValues.setValue('lat' + mCookieSuffix, mMapLatitude);
        nameValues.setValue('lng' + mCookieSuffix, mMapLongitude);
        nameValues.setValue('typ' + mCookieSuffix, mMapType);
        nameValues.setValue('zoo' + mCookieSuffix, mMapZoom);

        writeCookie('reportMapOptions', nameValues.toString());
    };

    this.addUI = function()
    {
        mMap = new google.maps.Map(document.getElementById('map_canvas'), {
            zoom: mMapZoom,
            center: new google.maps.LatLng(mMapLatitude, mMapLongitude),
            mapTypeId: mMapType,
            streetViewControl: false
        });
        google.maps.event.addListener(mMap, "dragend", mapSettingsChangedHandler);
        google.maps.event.addListener(mMap, "maptypeid_changed", mapSettingsChangedHandler);
        google.maps.event.addListener(mMap, "zoom_changed", mapSettingsChangedHandler);
    };

    function mapSettingsChangedHandler()
    {
        var centre = mMap.getCenter();
        mMapLatitude = centre.lat();
        mMapLongitude = centre.lng();
        mMapType = mMap.getMapTypeId();
        mMapZoom = mMap.getZoom();
        saveCookies();
    };

    function displayFlightPathHandler(sender, args)
    {
        that.displayPath(args.flight);
    };

    this.displayPath = function(flight)
    {
        createPins();

        clearPath();

        if(flight.fLat === undefined || flight.fLng === undefined || flight.lLat === undefined || flight.lLng === undefined) {
            mStartPin.setVisible(false);
            mEndPin.setVisible(false);
        } else {
            mStartPin.setPosition(new google.maps.LatLng(flight.fLat, flight.fLng));
            mEndPin.setPosition(new google.maps.LatLng(flight.lLat, flight.lLng));

            mStartPin.setIcon(createIcon(flight.fAlt, flight.fTrk, false));
            mEndPin.setIcon(createIcon(flight.lAlt, flight.lTrk, true));

            mStartPin.setTitle(createTitle(flight.start, flight.fAlt, flight.fTrk, flight.fVsi, flight.fSpd, false));
            mEndPin.setTitle(createTitle(flight.end, flight.lAlt, flight.lTrk, flight.lVsi, flight.lSpd, true));

            mStartPin.setVisible(true);
            mEndPin.setVisible(true);

            setPath(flight);
        }
    };

    function clearPath()
    {
        mLine.setMap(null);
        var path = mLine.getPath();
        var pathCount = path.getLength();
        while(pathCount-- > 0) path.pop();
    };

    function setPath(flight)
    {
        var path = mLine.getPath();
        path.push(new google.maps.LatLng(flight.fLat, flight.fLng));
        path.push(new google.maps.LatLng(flight.lLat, flight.lLng));
        mLine.setMap(mMap);
    };

    function createPins()
    {
        if(mStartPin == null) mStartPin = createPin("Start");
        if(mEndPin == null) mEndPin = createPin("End");
    };

    function createPin()
    {
        return new google.maps.Marker({
            position: new google.maps.LatLng(0, 0),
            map: mMap,
            visible: false,
            clickable: false
        });
    };

    function createIcon(altitude, track, isEnd)
    {
        if(!isNaN(track)) track = Math.round(track / 10) * 10;
        if(isNaN(track)) track = 0;

        if(isNaN(altitude)) altitude = 0;
        if(altitude > 35000) altitude = 35000;
        altitude = Math.round(altitude / 2500) * 5;

        var imageHeight = 35 + altitude + 2;
        var imageUrl = 'Images/Rotate-' + track + '/Alt-' + altitude + '/Hght-' + imageHeight + '/CenX-17/Airplane';
        if(isEnd) imageUrl += 'Selected';
        imageUrl += '.png';

        return new google.maps.MarkerImage(imageUrl, new google.maps.Size(35, imageHeight), new google.maps.Point(0, 0), new google.maps.Point(17, imageHeight - 2));
    };

    function createTitle(date, altitude, track, vsi, speed, isEnd)
    {
        var result = isEnd ? "End" : "Start";

        if(date !== null) result += ' :: ' + intToString(date.getHours(), 2) + ':' + intToString(date.getMinutes(), 2);
        if(altitude !== 0) result += ' :: A=' + altitude.toString();
        if(vsi !== 0) result += ' :: V=' + vsi.toString();
        if(speed !== 0) result += ' :: S=' + speed.toString();
        if(track !== undefined && track !== null) result += ' :: T=' + track.toString();

        return result;
    };
}