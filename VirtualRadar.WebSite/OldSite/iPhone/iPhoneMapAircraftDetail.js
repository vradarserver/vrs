function iPhoneMapAircraftDetail(events, options, iPhoneMapPages)
{
    var that = this;
    var mEvents = events;
    var mOptions = options;
    var mIPhoneMapPages = iPhoneMapPages;
    var mElements = {};
    var mAircraft = null;
    var mPreviousAircraft = null;
    var mMap;
    var mMarker;
    var mTrail;
    var mLocationKnown = false;
    this.getLocationKnown = function() { return mLocationKnown; };
    this.setLocationKnown = function(value) { mLocationKnown = value; };

    this.getAircraft = function() { return mAircraft; };
    this.setAircraft = function(value) { mAircraft = value; initialiseMap(); };

    this.initialise = function()
    {
        var containerElement = document.getElementById('aircraft_detail');

        var toolbar = createElement('div', 'toolbar', containerElement, 'aircraft_detail_toolbar');
        createElement('h1', null, toolbar).innerHTML = 'Aircraft Info';
        createLink('#', 'button back', toolbar, null, 'List').onclick = listButtonClicked;
        createLink('#', 'button', toolbar, null, 'Map').onclick = mapButtonClicked;

        createDetailSkeleton(containerElement);

        mMap = new google.maps.Map(document.getElementById('aircraft_detail_map'), {
            zoom: mOptions.mapZoom,
            center: new google.maps.LatLng(mOptions.mapLatitude, mOptions.mapLongitude),
            mapTypeId: mOptions.mapType,
            streetViewControl: false,
            draggable: false,
            mapTypeControlOptions: { style: google.maps.MapTypeControlStyle.DROPDOWN_MENU, position: google.maps.ControlPosition.TOP_LEFT },
            scaleControl: true
        });

        mMarker = new google.maps.Marker({
            position: new google.maps.LatLng(0, 0),
            map: mMap,
            clickable: false,
            flat: true,
            visible: true
        });

        mTrail = new google.maps.Polyline({map: mMap});

        mEvents.addListener(EventId.iPhonePageSwitched, pageSwitchHandler);
        mEvents.addListener(EventId.acListRefreshed, acListRefreshedHandler);
    };

    function listButtonClicked()
    {
        mIPhoneMapPages.select(PageId.aircraftList, PageAnimation.none);
    };

    function mapButtonClicked()
    {
        mIPhoneMapPages.select(PageId.map, PageAnimation.none);
    };

    function createDetailSkeleton(parent)
    {
        var container = createElement('div', null, parent, 'aircraft_detail_body');

        var opGroup = createGroup(container, 'groupOp', 'Operator');
        var opLine = createElement('li', null, opGroup);
        createItem(opLine, 'opFlag', 'acDetailOpFlag');
        createItem(opLine, 'opIcao', 'acDetailRight acDetailSmall');
        createItem(opLine, 'op', 'acDetailLine acDetailNoWrap acDetailHeading');

        var aircraftGroup = createGroup(container, 'groupAircraft', 'Aircraft');
        createLine(aircraftGroup, 'icao', 'ICAO', false);
        createLine(aircraftGroup, 'reg', 'Registration', false);
        createLine(aircraftGroup, 'flightsCount', 'Seen', false);
        createLine(aircraftGroup, 'cou', 'Country', false);
        createLine(aircraftGroup, 'type', 'Type', false);
        createLine(aircraftGroup, 'mdl', 'Model', false);
        createLine(aircraftGroup, 'engines', 'Engines', false);
        createLine(aircraftGroup, 'wtc', 'Weight', false);
        createLine(aircraftGroup, 'species', 'Species', false);
        var picLine = createElement('li', null, aircraftGroup);
        mElements['pictureLine'] = picLine;
        createItem(picLine, 'pictureContent', 'acDetailPicture');

        var callGroup = createGroup(container, 'groupCallsign', 'Callsign, Squawk and Route');
        createLine(callGroup, 'call', 'Callsign', false);
        createLine(callGroup, 'squawk', 'Squawk', false);
        createLine(callGroup, 'from', 'From', true);
        createLine(callGroup, 'via', 'Via', true);
        createLine(callGroup, 'to', 'To', true);

        var positionGroup = createGroup(container, 'groupPosition', 'Position');
        createLine(positionGroup, 'alt', 'Altitude', false);
        createLine(positionGroup, 'vsi', 'V. Speed', false);
        createLine(positionGroup, 'spd', 'Speed', false);
        createLine(positionGroup, 'trak', 'Heading', false);
        createLine(positionGroup, 'dst', 'Distance', false);
        createLine(positionGroup, 'brng', 'Bearing', false);

        var movingMapGroup = createElement('div', null, container);
        mElements['map'] = movingMapGroup;
        createElement('div', null, movingMapGroup, 'aircraft_detail_map');

        createElement('p', null, container).innerHTML = '&nbsp;';
    };

    function createGroup(parent, name, labelText)
    {
        var outer = createElement('div', null, parent);
        mElements[name] = outer;
        outer.style.display = 'none';
        if(labelText !== undefined && labelText !== null) createElement('h4', null, outer).innerHTML = labelText;
        return createElement('ul', null, outer);
    };

    function createItem(parent, name, classContent)
    {
        var item = createElement('div', classContent, parent);
        mElements[name] = item;
        return item;
    };

    function fillItem(name, isVisible, content)
    {
        mElements[name].innerHTML = content;
        return isVisible ? 1 : 0;
    };

    function createLine(parent, namePrefix, labelText, allowWrap)
    {
        var line = createElement('li', null, parent);
        mElements[namePrefix + 'Line'] = line;

        var outer = createElement('div', 'acDetailLine' + (allowWrap ? '' : ' acDetailNoWrap' ), line);
        var label = createElement('div', 'acDetailLabel', outer);
        if(labelText !== null) label.innerHTML = labelText;
        createItem(outer, namePrefix + 'Value', 'acDetailContent');
    };

    function fillLine(namePrefix, isVisible, content)
    {
        mElements[namePrefix + 'Line'].style.display = isVisible ? 'block' : 'none';
        return fillItem(namePrefix + 'Value', isVisible, content);
    };

    function setGroupVisibility(name, forceRefresh, contentCount)
    {
        if(forceRefresh) mElements[name].style.display = contentCount > 0 ? 'block' : 'none';
        else if(contentCount > 0) mElements[name].style.display = 'block';
    };

    function acListRefreshedHandler(sender, args)
    {
        if(mAircraft !== null) {
            if(!mAircraft.OnRadar) reacquireAircraft(args);
            if(mAircraft.OnRadar) refreshDetail();
        }
    };

    function pageSwitchHandler(sender, args)
    {
        // I noticed on the iPhone 4 that if the map was showing in this div when the list page
        // was made visible then the text on the list page would be blurry... and similarly if you
        // make this page visible and the map isn't on show then it becomes blurry. Weird, no? :)
        if(mIPhoneMapPages.getSelectedPageId() === PageId.aircraftDetail) google.maps.event.trigger(mMap, 'resize');
        refreshDetail();
    };

    function reacquireAircraft(aircraftList)
    {
        var length = aircraftList.length;
        for(var i = 0;i < length;++i) {
            if(aircraftList[i].Id === mAircraft.Id) {
                mAircraft = aircraftList[i];
                mPreviousAircraft = null;
                break;
            }
        }
    };

    function refreshDetail()
    {
        if(mAircraft !== null && mIPhoneMapPages.getSelectedPageId() === PageId.aircraftDetail) {
            var forceRefresh = mAircraft !== mPreviousAircraft;
            mPreviousAircraft = mAircraft;

            fillOpGroup(forceRefresh);
            fillAircraftGroup(forceRefresh);
            fillCallsignGroup(forceRefresh);
            fillPositionGroup(forceRefresh);

            var mapElement = mElements['map'];
            if(!mAircraft.getHasPos()) {
                mapElement.style.display = 'none';
            } else {
                if(mapElement.style.display === 'none') {
                    mapElement.style.display = 'block';
                    google.maps.event.trigger(mMap, 'resize');
                }

                if(forceRefresh || mAircraft.PosTimeChanged) {
                    if(mMarker.getVisible() === false) mMarker.setVisible(true);
                    mAircraft.centreOnMap(mMap);
                    mAircraft.Marker.cloneToPin(mMarker);
                    mAircraft.Marker.cloneToLine(mTrail)

                    switch(mOptions.trace) {
                        case TraceType.None: if(mTrail.getMap() !== null) mTrail.setMap(null); break;
                        default:             if(mTrail.getMap() === null) mTrail.setMap(mMap); break;
                    }
                }
            }
        }
    };

    function fillOpGroup(forceRefresh)
    {
        var contentCount = 0;
        if(forceRefresh || mAircraft.OpChanged) contentCount += fillItem('op', mAircraft.Op !== '', mAircraft.Op);
        if(forceRefresh || mAircraft.OpIcaoChanged) {
            contentCount += fillItem('opIcao', mAircraft.OpIcao !== '', mAircraft.OpIcao);
            contentCount += fillItem('opFlag', mAircraft.OpIcao !== '', mAircraft.formatOperatorFlag());
        }

        setGroupVisibility('groupOp', forceRefresh, contentCount);
    };

    function fillAircraftGroup(forceRefresh)
    {
        var contentCount = 0;
        if(forceRefresh || mAircraft.IcaoChanged) contentCount += fillLine('icao', mAircraft.Icao !== '', mAircraft.Icao);
        if(forceRefresh || mAircraft.RegChanged) contentCount += fillLine('reg', mAircraft.Reg !== '', mAircraft.Reg);
        if(forceRefresh || mAircraft.TypeChanged) contentCount += fillLine('type', mAircraft.Type !== '', mAircraft.Type + mAircraft.formatSilhouette(undefined, 'acDetailRight'));
        if(forceRefresh || mAircraft.MdlChanged) contentCount += fillLine('mdl', mAircraft.Mdl !== '', mAircraft.Mdl);
        if(forceRefresh || mAircraft.CouChanged || mAircraft.MilChanged) contentCount += fillLine('cou', true, mAircraft.formatCountry());
        if(forceRefresh || mAircraft.EnginesChanged || mAircraft.EngTypeChanged) contentCount += fillLine('engines', mAircraft.Engines || mAircraft.EngType, capitaliseSentence(mAircraft.formatEngines()));
        if(forceRefresh || mAircraft.WTCChanged) contentCount += fillLine('wtc', mAircraft.WTC, capitaliseSentence(mAircraft.formatWakeTurbulenceCategory(false, true)));
        if(forceRefresh || mAircraft.SpeciesChanged) contentCount += fillLine('species', mAircraft.Species, capitaliseSentence(mAircraft.formatSpecies(false)));
        if(forceRefresh || mAircraft.FlightsCountChanged) contentCount += fillLine('flightsCount', mAircraft.FlightsCount !== null, (mAircraft.FlightsCount === 0 ? 'No flights in database' : mAircraft.FlightsCount.toString() + ' time' + (mAircraft.FlightsCount === 1 ? '' : 's')));
        if(forceRefresh || mAircraft.HasPicChanged) {
            if(mAircraft.HasPic) {
                mElements['pictureContent'].innerHTML = mAircraft.formatPicture(null, null, false, window.innerWidth < 750 ? 'iPhoneDetail' : 'iPadDetail');
                ++contentCount;
            }
            mElements['pictureLine'].style.display = mAircraft.HasPic ? 'block' : 'none';
        }

        setGroupVisibility('groupAircraft', forceRefresh, contentCount);
    };

    function fillCallsignGroup(forceRefresh)
    {
        var contentCount = 0;
        if(forceRefresh || mAircraft.CallChanged) contentCount += fillLine('call', true, mAircraft.Call === '' ? 'Not transmitted' : mAircraft.Call);
        if(forceRefresh || mAircraft.SqkChanged) contentCount += fillLine('squawk', mAircraft.Sqk !== '', mAircraft.Sqk);
        if(forceRefresh || mAircraft.FromChanged) contentCount += fillLine('from', mAircraft.From !== '', mAircraft.From);
        if(forceRefresh || mAircraft.StopsChanged) contentCount += fillLine('via', mAircraft.Stops.length > 0, formatStops(mAircraft.Stops));
        if(forceRefresh || mAircraft.ToChanged) contentCount += fillLine('to', mAircraft.To !== '', mAircraft.To);

        setGroupVisibility('groupCallsign', forceRefresh, contentCount);
    };

    function fillPositionGroup(forceRefresh)
    {
        var contentCount = 0;
        if(forceRefresh || mAircraft.AltChanged) contentCount += fillLine('alt', mAircraft.Alt !== null, mAircraft.formatConvertedAlt());
        if(forceRefresh || mAircraft.VsiChanged) contentCount += fillLine('vsi', mAircraft.Vsi !== null, mAircraft.formatVerticalSpeed());
        if(forceRefresh || mAircraft.SpdChanged) contentCount += fillLine('spd', mAircraft.Spd !== null, mAircraft.formatConvertedSpd());
        if(forceRefresh || mAircraft.TrakChanged) contentCount += fillLine('trak', mAircraft.Trak !== null, mAircraft.formatHeading());
        if(forceRefresh || mAircraft.DstChanged) contentCount += fillLine('dst', mAircraft.Dst !== null, mAircraft.formatDistance(mLocationKnown));
        if(forceRefresh || mAircraft.BrngChanged) contentCount += fillLine('brng', mAircraft.Brng !== null, mAircraft.formatBearingCompass(mLocationKnown) + ' ' + mAircraft.formatBearing(mLocationKnown));

        setGroupVisibility('groupPosition', forceRefresh, contentCount);
    };

    function formatStops(stops)
    {
        var result = '';
        for(var i = 0;i < stops.length;++i) {
            result += stops[i];
            if(i + 1 !== stops.length) result += '<br />';
        }

        return result;
    };

    function initialiseMap()
    {
        mMap.setZoom(mOptions.mapZoom);
        mMap.setMapTypeId(mOptions.mapType);
        mMarker.setVisible(false);
    };
}