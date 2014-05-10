function iPhoneMapInfoWindow(events, map, iPhoneMapPages, iPhoneAircraftDetail)
{
    var that = this;
    var mEvents = events;
    var mMap = map;
    var mInfoWindow = null;
    var mOnDisplay = false;
    var mDisplayNode;
    var mElements = {};
    var mTrackingAircraft = null;
    var mIPhoneMapPages = iPhoneMapPages;
    var mIPhoneAircraftDetail = iPhoneAircraftDetail;

    this.initialise = function()
    {
        createDisplayNode();
        mInfoWindow = new google.maps.InfoWindow({
            content: ''
        });

        mEvents.addListener(EventId.noLongerTracked, noLongerTrackedHandler);
        mEvents.addListener(EventId.acListRefreshed, aircraftRefreshedHandler);
        mEvents.addListener(EventId.iPhonePageSwitched, iPhonePageSwitchedHandler);
    };

    this.displayAircraftInformation = function(aircraft)
    {
        that.hide();
        if(aircraft.Marker !== undefined && aircraft.Marker !== null) {
            fillDisplayNode(aircraft);
            mInfoWindow.setContent(mDisplayNode);
            that.show(aircraft.Marker.getAnchor());
            mTrackingAircraft = aircraft;
        }
    };

    this.show = function(anchor)
    {
        if(!mOnDisplay) {
            mInfoWindow.open(map, anchor);
            mOnDisplay = true;
        }
    };

    this.hide = function()
    {
        if(mOnDisplay) {
            mInfoWindow.close();
            mOnDisplay = false;
        }
    };

    function aircraftRefreshedHandler(sender, args)
    {
        if(mTrackingAircraft !== null && mIPhoneMapPages.getSelectedPageId() === PageId.map) {
            updateDisplayNode(mTrackingAircraft);
        }
    };

    function iPhonePageSwitchedHandler(sender, args)
    {
        if(mTrackingAircraft !== null && mIPhoneMapPages.getSelectedPageId() !== PageId.map) {
            that.hide();
            mTrackingAircraft = null;
        }
    }

    function noLongerTrackedHandler(sender, aircraftList)
    {
        if(mTrackingAircraft !== null) {
            var length = aircraftList.length;
            for(var i = 0;i < length;++i) {
                if(mTrackingAircraft === aircraftList[i]) {
                    that.hide();
                    mTrackingAircraft = null;
                    break;
                }
            }
        }
    };

    function linkClickedHandler()
    {
        if(mTrackingAircraft !== null && mIPhoneAircraftDetail !== undefined) {
            mIPhoneAircraftDetail.setAircraft(mTrackingAircraft);
            mIPhoneMapPages.select(PageId.aircraftDetail, PageAnimation.none);
        }
    };

    function createDisplayNode()
    {
        mDisplayNode = createElement('div', 'infoWindow', null);

        var group = createElement('ul', 'infoWindowGroup', mDisplayNode);
        createItemNode(group, null, 'OpFlag');
        createItemNode(group, 'ICAO', 'Icao');
        createItemNode(group, 'Reg', 'Reg');
        createItemNode(group, null, 'RegType');
        createItemNode(group, 'Operator', 'Op');
        createItemNode(group, 'Type', 'Type');
        createItemNode(group, 'Model', 'Mdl');
        createItemNode(group, 'Callsign', 'Call');
        createItemNode(group, 'From', 'From');
        createItemNode(group, 'Via', 'Via');
        createItemNode(group, 'To', 'To');
        createItemNode(group, 'Altitude', 'Alt');
        createItemNode(group, 'Speed', 'Spd');

        var link = createElement('div', 'infoWindowLink', mDisplayNode);
        link.onclick = linkClickedHandler;
        link.innerHTML = "Aircraft information";
    };

    function createItemNode(parent, labelText, idSuffix)
    {
        var top = createElement('li', 'infoWindowItem infoWindowHidden', parent, 'infoWindowLine' + idSuffix);
        var label = createElement('div', 'infoWindowLabel', top);
        if(labelText !== null) label.innerHTML = labelText + ':';
        var value = createElement('div', 'infoWindowValue', top, 'infoWindowValue' + idSuffix);

        mElements[idSuffix + 'Line'] = top;
        mElements[idSuffix + 'Value'] = value;
    };

    function setItemNode(idSuffix, isVisible, content)
    {
        var lineElement = mElements[idSuffix + 'Line'];
        var valueElement = mElements[idSuffix + 'Value'];

        setClass(lineElement, isVisible ? 'infoWindowItem' : 'infoWindowItem infoWindowHidden');
        if(isVisible) valueElement.innerHTML = content;
    };

    function fillDisplayNode(aircraft)      { fillDisplayNodes(aircraft, true); };
    function fillDisplayNodes(aircraft, forceSet)
    {
        var roomToBreathe = window.innerHeight >= 600;

        if(forceSet || aircraft.OpIcaoChanged)  setItemNode('OpFlag', roomToBreathe && aircraft.OpIcao !== '', aircraft.formatOperatorFlag());
        if(forceSet || aircraft.IcaoChanged)    setItemNode('Icao', roomToBreathe && aircraft.Icao !== '', aircraft.Icao);
        if(forceSet || aircraft.RegChanged)     setItemNode('Reg', roomToBreathe && aircraft.Reg !== '', aircraft.Reg);
        if(forceSet || aircraft.OpChanged)      setItemNode('Op', aircraft.Op !== '', aircraft.Op);
        if(forceSet || aircraft.TypeChanged)    setItemNode('Type', roomToBreathe && aircraft.Type !== '', aircraft.Type);
        if(forceSet || aircraft.MdlChanged)     setItemNode('Mdl', roomToBreathe && aircraft.Mdl !== '', aircraft.Mdl);
        if(forceSet || aircraft.CallChanged)    setItemNode('Call', aircraft.Call !== '', aircraft.Call);
        if(forceSet || aircraft.FromChanged)    setItemNode('From', aircraft.From !== '', aircraft.From);
        if(forceSet || aircraft.ToChanged)      setItemNode('To', aircraft.To !== '', aircraft.To);
        if(forceSet || aircraft.StopsChanged)   setItemNode('Via', aircraft.Stops.length > 0, formatStops(aircraft.Stops));
        if(forceSet || aircraft.AltChanged)     setItemNode('Alt', aircraft.Alt !== null, aircraft.formatConvertedAlt());
        if(forceSet || aircraft.SpdChanged)     setItemNode('Spd', roomToBreathe && aircraft.Spd !== null, aircraft.formatConvertedSpd());

        if(forceSet || aircraft.RegChanged || aircraft.TypeChanged) setItemNode('RegType', !roomToBreathe && (aircraft.Reg !== '' || aircraft.Type !== ''), aircraft.Type + ' ' + aircraft.Reg);
    };
    function updateDisplayNode(aircraft)
    {
        var oldHeight = mDisplayNode.offsetHeight;
        fillDisplayNodes(aircraft, false);
        if(mDisplayNode.offsetHeight != oldHeight) {
            that.hide();
            that.show(aircraft.Marker.getAnchor());
        }
    };

    function formatStops(stops)
    {
        var result = '';
        for(var i = 0;i < stops.length;++i) {
            result += stops[i];
            if(i + 1 != stops.length) result += '<br />';
        }

        return result;
    };
}