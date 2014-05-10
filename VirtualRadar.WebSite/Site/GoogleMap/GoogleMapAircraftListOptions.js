function GoogleMapAircraftListOptions(events, aircraftList, aircraftListColumns)
{
    var that = this;
    var mEvents = events;
    var mOptionsUI;
    var mAircraftList = aircraftList;
    var mAircraftListColumns = aircraftListColumns;

    var mSortField1 = _MapMode !== MapMode.iPhone ? 'timeSeen' : 'operator';
    var mSortDirection1 = _MapMode !== MapMode.iPhone ? 'desc' : 'asc';
    var mSortField2 = _MapMode !== MapMode.iPhone ? '' : 'reg';
    var mSortDirection2 = 'asc';
    this.getSortField1 = function() { return mSortField1; };
    this.getSortDirection1 = function() { return mSortDirection1; };
    this.getSortField2 = function () { return mSortField2; };
    this.getSortDirection2 = function () { return mSortDirection2; };

    this.setSortField1 = function(value) { mSortField1 = value; };
    this.setSortField2 = function(value) { mSortField2 = value; };
    this.setSortDirection1 = function(value) { mSortDirection1 = value; };
    this.setSortDirection2 = function(value) { mSortDirection2 = value; };

    this.save = function() { saveSortSettings(); };

    mEvents.addListener(EventId.optionsUICreateTabPage, createTabPage);

    function eraseOldCookies()
    {
        eraseCookie('gmListSortBy1');
        eraseCookie('gmListSortOrder1');
        eraseCookie('gmListSortBy2');
        eraseCookie('gmListSortOrder2');
    };

    function saveSortSettings()
    {
        eraseOldCookies();
        var nameValues = new nameValueCollection();
        nameValues.pushValue('sf1', mSortField1);
        nameValues.pushValue('sd1', mSortDirection1);
        nameValues.pushValue('sf2', mSortField2);
        nameValues.pushValue('sd2', mSortDirection2);
        writeCookie('googleMapListOptions', nameValues.toString());
    };

    this.loadSortSettings = function()
    {
        var cookies = readCookieValues();
        for(var c = 0;c < cookies.length;c++) {
            var valuePair = cookies[c];
            switch(valuePair.name) {
                // Old cookies, no longer used but we need to be able to read them
                case 'gmListSortBy1':     mSortField1 = valuePair.value; break;
                case 'gmListSortOrder1':  mSortDirection1 = valuePair.value; break;
                case 'gmListSortBy2':     mSortField2 = valuePair.value; break;
                case 'gmListSortOrder2':  mSortDirection2 = valuePair.value; break;

                case 'googleMapListOptions':
                    var nameValues = new nameValueCollection()
                    nameValues.fromString(valuePair.value);
                    for(var i = 0;i < nameValues.getLength();++i) {
                        var value = nameValues.getValueAt(i);
                        switch(nameValues.getNameAt(i)) {
                            case 'sf1': mSortField1 = value; break;
                            case 'sd1': mSortDirection1 = value; break;
                            case 'sf2': mSortField2 = value; break;
                            case 'sd2': mSortDirection2 = value; break;
                        }
                    }
                    c = cookies.length;
                    break;
            }
        }
    };

    function createTabPage(optionsUI, args)
    {
        mOptionsUI = optionsUI;

        mOptionsUI.createTabPage('list', 4, 'List', createHtml(optionsUI, 'list'), copyOptionsToForm, copyFormToOptions, save);
    };

    function createHtml(optionsUI, name)
    {
        var html = '';

        html += optionsUI.htmlHeading('Sort aircraft list');
        html += optionsUI.htmlColumns([ 
              [ optionsUI.htmlLabel(name, 'listSortBy1', 'Sort by:'), createSortSelect(optionsUI, name, 'listSortBy1'), createSortDirection(optionsUI, name, 'listSortOrder1') ],
              [ optionsUI.htmlLabel(name, 'listSortBy2', 'and then:'), createSortSelect(optionsUI, name, 'listSortBy2'), createSortDirection(optionsUI, name, 'listSortOrder2') ]
            ]);

        html += mAircraftListColumns.getTabPageHtml(optionsUI, name);

        return html;
    };

    function createSortSelect(optionsUI, name, selectName)
    {
        return optionsUI.htmlSelect(name, selectName, [
            { value: '', text: 'None' },
            { value: 'model', text: 'Aircraft Model' },
            { value: 'type', text: 'Aircraft Type' },
            { value: 'alt', text: 'Altitude' },
            { value: 'call', text: 'Callsign' },
            { value: 'cou', text: 'Country' },
            { value: 'dist', text: 'Distance' },
            { value: 'eng', text: 'Engines' },
            { value: 'from', text: 'From' },
            { value: 'icao', text: 'ICAO' },
            { value: 'operator', text: 'Operator Name' },
            { value: 'operatorIcao', text: 'Operator Code' },
            { value: 'reg', text: 'Registration' },
            { value: 'fcnt', text: 'Seen Count' },
            { value: 'spc', text: 'Species' },
            { value: 'spd', text: 'Speed' },
            { value: 'sqk', text: 'Squawk' },
            { value: 'timeSeen', text: 'Time First Seen' },
            { value: 'to', text: 'To' },
            { value: 'vsi', text: 'Vertical Speed' },
            { value: 'wtc', text: 'Wake Turbulence Category' }
            ], 1);
    }

    function createSortDirection(optionsUI, name, selectName)
    {
        return optionsUI.htmlSelect(name, selectName, [
            { value: 'desc', text: 'Descending' },
            { value: 'asc', text: 'Ascending' }
            ], 1);
    }

    function copyOptionsToForm(form, optionsUI, tabPage)
    {
        form.listSortBy1.value = mSortField1;
        form.listSortOrder1.value = mSortDirection1;
        form.listSortBy2.value = mSortField2;
        form.listSortOrder2.value = mSortDirection2;

        mAircraftListColumns.copyOptionsToForm(form, optionsUI, tabPage);
    }

    function copyFormToOptions(form, optionsUI, tabPage)
    {
        mSortField1 = form.listSortBy1.value;
        mSortDirection1 = form.listSortOrder1.value;
        mSortField2 = form.listSortBy2.value;
        mSortDirection2 = form.listSortOrder2.value;

        mAircraftListColumns.copyFormToOptions(form);
    }

    function save(optionsUI, tabPage)
    {
        saveSortSettings();
        mAircraftListColumns.saveCustomColumns(optionsUI, tabPage);
    }
}