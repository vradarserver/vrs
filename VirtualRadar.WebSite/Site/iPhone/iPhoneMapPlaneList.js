var iPhoneMapPlaneListObjects = [];

function iPhoneMapPlaneList(events, options, iPhoneMapPages, iPhoneAircraftDetail, aircraftCollection)
{
    var that = this;
    var mGlobalIndex = iPhoneMapPlaneListObjects.length;
    iPhoneMapPlaneListObjects.push(that);
    var mEvents = events;
    var mIPhoneMapPages = iPhoneMapPages;
    var mIPhoneAircraftDetail = iPhoneAircraftDetail;
    var mOptions = options;
    var mAircraftCollection = aircraftCollection;
    var mListElement;
    var mPauseElement;
    var mPauseBlock;
    var mCountBlock;
    var mElements = [];
    
    this.initialise = function()
    {
        var containerElement = document.getElementById('plane_list');

        var toolbar = createElement('div', 'toolbar', containerElement, 'plane_list_toolbar');
        createElement('h1', null, toolbar).innerHTML = 'Aircraft List';
        createLink('#', 'button back', toolbar, null, 'Map').onclick = mapButtonClicked;
        createLink('#', 'button', toolbar, null, 'Settings').onclick = optionsButtonClicked;

        createListSkeleton(containerElement);

        mEvents.addListener(EventId.iPhonePageSwitched, pageSwitchedHandler);
        mEvents.addListener(EventId.acListRefreshed, acListRefreshedHandler);
        mEvents.addListener(EventId.pauseChanged, pauseChangedHandler);
    };

    function pageSwitchedHandler()
    {
        refreshList();
    };

    function acListRefreshedHandler()
    {
        refreshList();
    };

    function pauseChangedHandler(sender, args)
    {
        mPauseBlock.innerHTML = args ? 'Resume' : 'Pause';
    };

    function mapButtonClicked() { mIPhoneMapPages.select(PageId.map, PageAnimation.none); };
    function optionsButtonClicked() { mIPhoneMapPages.select(PageId.options, PageAnimation.none); };

    function selectAircraft(aircraft)
    {
        if(aircraft !== null) {
            mIPhoneAircraftDetail.setAircraft(aircraft);
            mIPhoneMapPages.select(PageId.aircraftDetail, PageAnimation.none);
        }
    };

    function togglePauseClicked()
    {
        mEvents.raise(EventId.togglePauseClicked, null, null);
    };

    function createListSkeleton(parent)
    {
        var container = createElement('div', null, parent, 'plane_list_list');

        mListElement = createElement('ul', 'edgetoedge', container);

        mPauseElement = createElement('li', 'acListLine acListPauseLine', mListElement);
        mPauseElement.onclick = function() { togglePauseClicked(); };
        mPauseBlock = createElement('div', 'acListPause acListPadLeft', mPauseElement);
        mPauseBlock.innerHTML = 'Pause';
        mCountBlock = createElement('div', 'acListCount acListCell acListRight', mPauseElement);
        mCountBlock.innerHTML = '0';

        createElement('p', 'acListCredit', container).innerHTML = 'Powered by Virtual Radar Server';
    };

    function createListItemSkeleton()
    {
        var listItem = createElement('li', 'arrow', mListElement);
        listItem._VirtualRadarServerAircraft = null;
        listItem.onclick = function() { selectAircraft(this._VirtualRadarServerAircraft); };

        var flagLine = createElement('div', 'acListLine', listItem);
        var flagValue = createElement('div', 'acListImg', flagLine);
        var opValue = createElement('div', 'acListCell acListHeading acListPadLeft', flagLine);

        var acLine = createElement('div', 'acListLine', listItem);
        var silhouetteValue = createElement('div', 'acListImg', acLine);
        var regValue = createElement('div', 'acListCell acListHeading acListPadLeft', acLine);
        var callValue = createElement('div', 'acListCell acListHeading acListRight', acLine);

        mElements.push({ line: listItem, flag: flagValue, op: opValue, silhouette: silhouetteValue, reg: regValue, call: callValue, selected: false });
    };

    function refreshList()
    {
        if(mIPhoneMapPages.getSelectedPageId() === PageId.aircraftList) {
            var aircraftList = mAircraftCollection.getAllAircraft();
            var acLength = aircraftList.length;

            var countText = acLength.toString();
            var availableAircraft = mOptions.tempAvailableAircraft;
            if(acLength !== availableAircraft) countText += ' / ' + availableAircraft;
            countText += ' aircraft';
            mCountBlock.innerHTML = countText;

            for(var i = 0;i < acLength;++i) {
                while(mElements.length <= i) createListItemSkeleton();

                var aircraft = aircraftList[i];
                var elements = mElements[i];
                var forceRefresh = elements.line._VirtualRadarServerAircraft !== aircraft;
                elements.line._VirtualRadarServerAircraft = aircraft;

                if(forceRefresh || elements.selected !== aircraft.Selected) {
                    setClass(elements.line, aircraft.Selected ? 'arrow acListSelected' : 'arrow');
                    elements.selected = aircraft.Selected;
                }

                if(forceRefresh || aircraft.OpIcaoChanged) elements.flag.innerHTML = aircraft.formatOperatorFlag();
                if(forceRefresh || aircraft.OpChanged) elements.op.innerHTML = aircraft.Op;
                if(forceRefresh || aircraft.TypeChanged) elements.silhouette.innerHTML = aircraft.formatSilhouette();
                if(forceRefresh || aircraft.RegChanged) elements.reg.innerHTML = aircraft.Reg;
                if(forceRefresh || aircraft.CallChanged) elements.call.innerHTML = aircraft.Call;

                elements.line.style.display = 'block';
            }

            var elementsLength = mElements.length;
            for(var i = acLength;i < elementsLength;++i) {
                var elements = mElements[i];
                elements.line.style.display = 'none';
                elements.line._VirtualRadarServerAircraft = null;
            }
        }
    };
}