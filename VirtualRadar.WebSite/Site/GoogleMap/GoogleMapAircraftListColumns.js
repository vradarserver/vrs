var ListColumn = {
    OperatorFlag: 'A',
    Registration: 'B',
    Icao: 'C',
    Callsign: 'D',
    Type: 'E',
    Altitude: 'F',
    Squawk: 'G',
    Speed: 'H',
    Silhouette: 'I',
    Latitude: 'J',
    Longitude: 'K',
    Heading: 'L',
    Model: 'M',
    Operator: 'N',
    VerticalSpeed: 'O',
    Distance: 'P',
    Bearing: 'Q',
    FlightLevel: 'R',
    Wtc: 'S',
    Engines: 'T',
    Country: 'U',
    CivOrMil: 'V',
    Species: 'W',
    Picture: 'X',
    TimeTracked: 'Y',
    FlightsCount: 'Z',
    RowHeader: '0',
    DebugCountPoints: '1',
    CountMessages: '2',
    UserTag: '3',
    Route: '4'
};

var _SingletonAircraftListColumns;
function listChooseAddColumnClicked()       { _SingletonAircraftListColumns.addColumnClicked(); }
function listResetToDefaultClicked()        { _SingletonAircraftListColumns.resetToDefaultClicked(); }
function listChooseRemoveColumnClicked(idx) { _SingletonAircraftListColumns.removeColumnClicked(idx); }
function listChooseColumnUpClicked(idx)     { _SingletonAircraftListColumns.columnUpClicked(idx); }
function listChooseColumnDownClicked(idx)   { _SingletonAircraftListColumns.columnDownClicked(idx); }

function GoogleMapAircraftListColumns(options, events)
{
    var that = this;
    _SingletonAircraftListColumns = this;

    var mCanShowSilhouettes = false;
    var mCanShowFlags = false;
    var mCanShowPictures = false;
    var mCustomColumns = [];
    var mShowDebugCountPoints = false;
    var mUseCustomColumns = false;
    var mCurrentColumns = null;
    var mSuppressListChanges = false;
    var mOptions = options;
    var mEvents = events;
    var mOptionsUI;
    var mTabName;

    this.getCanShowSilhouettes = function() { return mCanShowSilhouettes; };
    this.setCanShowSilhouettes = function(value) { if(mCanShowSilhouettes != value) { mCanShowSilhouettes = value; mCurrentColumns = null; } };
    this.getCanShowFlags = function() { return mCanShowFlags; };
    this.setCanShowFlags = function(value) { if(mCanShowFlags != value) { mCanShowFlags = value; mCurrentColumns = null; } };
    this.getCanShowPictures = function() { return mCanShowPictures; };
    this.setCanShowPictures = function(value) { if(mCanShowPictures != value) { mCanShowPictures = value; mCurrentColumns = null; updatePictureSelectText(); } };
    this.getShowDebugCountPoints = function() { return mShowDebugCountPoints; }
    this.setShowDebugCountPoints = function(value) { mShowDebugCountPoints = value; mCurrentColumns = null; };
    this.getUseCustomColumns = function() { return mUseCustomColumns; }
    this.setUseCustomColumns = function(value) { if(mUseCustomColumns != value) { mUseCustomColumns = value; mCurrentColumns = null; } };

    loadCookies();

    this.columnsHaveChanged = function()
    {
        return mCurrentColumns === null;
    };

    this.getColumns = function()
    {
        var result = [];
        result = result.concat(mUseCustomColumns && mCustomColumns !== null ? mCustomColumns : that.getDefaultColumns());

        if(mShowDebugCountPoints) result.splice(0, 0, ListColumn.DebugCountPoints);

        var showRowHeader = true, picturesIndex = -1;
        for(var c = 0;c < result.length;c++) {
            switch(result[c]) {
                case ListColumn.OperatorFlag:
                case ListColumn.Silhouette:
                    showRowHeader = false;
                    break;
                case ListColumn.Picture:
                    if(showRowHeader && mCanShowPictures) showRowHeader = false;
                    picturesIndex = c;
                    break;
            }
        }
        if(!mCanShowPictures && picturesIndex != -1) result.splice(picturesIndex, 1);
        if(showRowHeader) result.splice(0, 0, ListColumn.RowHeader);

        mCurrentColumns = result;

        return result;
    };

    function loadCookies()
    {
        var cookieValues = readCookieValues();

        // Pick up the obsolete cookies first
        var useCustomColumns = extractCookieValue(cookieValues, 'gmListUseCustomColumns');
        var columns = extractCookieValue(cookieValues, 'gmListColumns');

        // Then use the real cookies
        var nameValues = new nameValueCollection()
        nameValues.fromString(extractCookieValue(cookieValues, 'googleMapCustomCols'));
        if(nameValues.getLength() > 0) {
            columns = nameValues.getValue('cols');
            useCustomColumns = nameValues.getValue('enb');
        }

        if(useCustomColumns !== null && useCustomColumns !== 'false' && useCustomColumns !== '0') mUseCustomColumns = true;
        if(columns !== null) {
            for(var c = 0;c < columns.length;c++) {
                var columnId = columns.charAt(c);
                switch(columnId) {
                    case ListColumn.Altitude:
                    case ListColumn.Bearing:
                    case ListColumn.Callsign:
                    case ListColumn.CivOrMil:
                    case ListColumn.Country:
                    case ListColumn.CountMessages:
                    case ListColumn.Distance:
                    case ListColumn.Engines:
                    case ListColumn.FlightLevel:
                    case ListColumn.Heading:
                    case ListColumn.Icao:
                    case ListColumn.Latitude:
                    case ListColumn.Longitude:
                    case ListColumn.Model:
                    case ListColumn.Operator:
                    case ListColumn.OperatorFlag:
                    case ListColumn.Picture:
                    case ListColumn.Registration:
                    case ListColumn.Silhouette:
                    case ListColumn.Species:
                    case ListColumn.Speed:
                    case ListColumn.Squawk:
                    case ListColumn.TimeTracked:
                    case ListColumn.FlightsCount:
                    case ListColumn.Type:
                    case ListColumn.UserTag:
                    case ListColumn.VerticalSpeed:
                    case ListColumn.Wtc:
                    case ListColumn.Route:
                        mCustomColumns.push(columnId);
                        break;
                }
            }
        }
    };

    function eraseOldCookies()
    {
        eraseCookie('gmListColumns');
        eraseCookie('gmListUseCustomColumns');
    }

    function saveCookies()
    {
        var columns = '';
        var ignoreColumn;
        for(var c = 0;c < mCustomColumns.length;c++) {
            switch(mCustomColumns[c]) {
                case ListColumn.RowHeader:
                case ListColumn.DebugCountPoints:
                    ignoreColumn = true;
                    break;
                default:
                    ignoreColumn = false;
                    break;
            }
            if(!ignoreColumn) columns += mCustomColumns[c];
        }

        eraseOldCookies();
        var nameValues = new nameValueCollection();
        nameValues.pushValue('cols', columns);
        nameValues.pushValue('enb', mUseCustomColumns);
        writeCookie('googleMapCustomCols', nameValues.toString());
    };

    this.getDefaultColumns = function()
    {
        var result = [];
        if(mCanShowSilhouettes) result.push(ListColumn.Silhouette);
        if(mCanShowFlags) result.push(ListColumn.OperatorFlag);
        result.push(ListColumn.Registration);
        result.push(ListColumn.Icao);
        result.push(ListColumn.Callsign);
        if(!mCanShowSilhouettes) result.push(ListColumn.Type);
        result.push(ListColumn.Altitude);
        if(!mCanShowSilhouettes || !mCanShowFlags) result.push(ListColumn.Squawk);
        result.push(ListColumn.Speed);

        return result;
    };

    this.getTabPageHtml = function(optionsUI, name)
    {
        mOptionsUI = optionsUI;
        mTabName = name;

        var html = '';
        html += optionsUI.htmlHeading('Custom columns');
        html += optionsUI.htmlCheckBox(name, 'listChooseUseCustomColumns', 'Use custom columns') + optionsUI.htmlEol();

        html += '<div id="listCustomColumns">' + customColumnsListUI(optionsUI, name) + '</div>';

        html += '<div class="listOptionsLinks">';
        html += '<div class="listChooseAdd"><a href="#" onclick="listChooseAddColumnClicked()">Add column</a></div>';
        html += '<div class="listResetToDefault"><a href="#" onclick="listResetToDefaultClicked()">Reset to defaults</a></div>';
        html += '</div>';

        return html;
    };

    function customColumnsListUI(optionsUI, name)
    {
        var html = '';

        if(mCustomColumns === null || mCustomColumns.length === 0) {
            html += '<div class="listChooseNoCols">Click "Add column" to begin modifying the list or "Reset to defaults" to prefill with default columns</div>';
        } else {
            var rows = [];
            for(var c = 0;c < mCustomColumns.length;c++) {
                rows.push(createColumnComboBox(optionsUI, name, c, c == 0, c + 1 == mCustomColumns.length));
            }
            html += optionsUI.htmlColumns(rows);
        }

        return html;
    };

    function createColumnComboBox(optionsUI, name, num, isFirst, isLast)
    {
        return [
            optionsUI.htmlSelect(name, 'listChooseCol' + num, [
                { value: ListColumn.Altitude, text: 'Altitude' },
                { value: ListColumn.Bearing, text: 'Bearing' },
                { value: ListColumn.Callsign, text: 'Callsign' },
                { value: ListColumn.Country, text: 'Country' },
                { value: ListColumn.Distance, text: 'Distance' },
                { value: ListColumn.Engines, text: 'Engines' },
                { value: ListColumn.FlightLevel, text: 'Flight Level' },
                { value: ListColumn.Heading, text: 'Heading' },
                { value: ListColumn.Icao, text: 'ICAO' },
                { value: ListColumn.Latitude, text: 'Latitude' },
                { value: ListColumn.Longitude, text: 'Longitude' },
                { value: ListColumn.CountMessages, text: 'Messages' },
                { value: ListColumn.CivOrMil, text: 'Military' },
                { value: ListColumn.Model, text: 'Model' },
                { value: ListColumn.Operator, text: 'Operator Name' },
                { value: ListColumn.OperatorFlag, text: 'Operator Flag' },
                { value: ListColumn.Picture, text: mCanShowPictures ? 'Picture' : 'Picture (Not available)' },
                { value: ListColumn.Registration, text: 'Registration' },
                { value: ListColumn.Route, text: 'Route' },
                { value: ListColumn.FlightsCount, text: 'Seen Count' },
                { value: ListColumn.Silhouette, text: 'Silhouette' },
                { value: ListColumn.Species, text: 'Species' },
                { value: ListColumn.Speed, text: 'Speed' },
                { value: ListColumn.Squawk, text: 'Squawk' },
                { value: ListColumn.TimeTracked, text: 'Time Tracked' },
                { value: ListColumn.Type, text: 'Type' },
                { value: ListColumn.UserTag, text: 'User Tag' },
                { value: ListColumn.VerticalSpeed, text: 'Vertical Speed' },
                { value: ListColumn.Wtc, text: 'Wake Turbulence Cat.' }
            ], 1),
            '<div class="listChooseOp">' + (!isFirst ? '<a href="#" onclick="listChooseColumnUpClicked(' + num + ')">Up</a>' : 'Up') + '</div>',
            '<div class="listChooseOp">' + (!isLast ? '<a href="#" onclick="listChooseColumnDownClicked(' + num + ')">Down</a>' : 'Down') + '</div>',
            '<div class="listChooseOp">' + ('<a href="#" onclick="listChooseRemoveColumnClicked(' + num + ')">Remove</a>') + '</div>'
        ];
    };

    function updatePictureSelectText()
    {
        var form = mOptionsUI.getForm();
        var index = undefined;
        var newText = mCanShowPictures ? 'Picture' : 'Picture (Not available)';

        for(var c = 0;c < 999;c++) {
            var comboBox = getCustomColumnComboBox(form, c);
            if(!comboBox) break;
            if(!index) {
                index = -1;
                for(var i = 0;i < comboBox.options.length;++i) {
                    if(comboBox.options[i].value === ListColumn.Picture) {
                        index = i;
                        break;
                    }
                }
            }

            if(index != -1) comboBox.options[index].text = newText;
        }
    };

    this.copyOptionsToForm = function(form, optionsUI, tabPage)
    {
        mSuppressListChanges = true;

        form.listChooseUseCustomColumns.checked = mUseCustomColumns;
        document.getElementById('listCustomColumns').innerHTML = customColumnsListUI(optionsUI, tabPage.name);
        for(var c = 0;c < mCustomColumns.length;c++) {
            var comboBox = getCustomColumnComboBox(form, c);
            comboBox.value = mCustomColumns[c];
        }

        mSuppressListChanges = false;
    };

    this.copyFormToOptions = function(form)
    {
        mUseCustomColumns = form.listChooseUseCustomColumns.checked;

        mCustomColumns = [];
        for(var c = 0;c < 999;c++) {
            var comboBox = getCustomColumnComboBox(form, c);
            if(comboBox === null) break;
            mCustomColumns.push(comboBox.value);
        }
    };

    function getCustomColumnComboBox(form, customColumnNumber)
    {
        return getFormInputElementByName(form, 'listChooseCol' + customColumnNumber);
    };

    this.saveCustomColumns = function()
    {
        if(!mSuppressListChanges) {
            mCurrentColumns = null;
            saveCookies();

            mEvents.raise(EventId.listChooseColumnsChanged, null, null);
        }
    };

    this.resetToDefaultClicked = function()
    {
        mCustomColumns = that.getDefaultColumns();
        mOptionsUI.copyOptionsToForm(mTabName);
        that.saveCustomColumns();
    };

    this.addColumnClicked = function()
    {
        mCustomColumns.push(ListColumn.Registration);
        mOptionsUI.copyOptionsToForm(mTabName);
        that.saveCustomColumns();
    };

    this.removeColumnClicked = function(idx)
    {
        if(mCustomColumns.length > idx) {
            mCustomColumns.splice(idx, 1);
            mOptionsUI.copyOptionsToForm(mTabName);
            that.saveCustomColumns();
        }
    };

    this.columnUpClicked = function(idx)
    {
        moveColumn(idx, true);
    };

    this.columnDownClicked = function(idx)
    {
        moveColumn(idx, false);
    };

    function moveColumn(idx, moveUp)
    {
        if(moveUp) swapArrayElements(mCustomColumns, idx, idx - 1);
        else       swapArrayElements(mCustomColumns, idx, idx + 1);
        mOptionsUI.copyOptionsToForm(mTabName);
        that.saveCustomColumns();
    };

    this.getColumnWidth = function(listColumn)
    {
        var result = null;
        switch(listColumn) {
            case ListColumn.Altitude:           result = 40; break;
            case ListColumn.Bearing:            result = 50; break;
            case ListColumn.Callsign:           result = 70; break;
            case ListColumn.CivOrMil:           result = 70; break;
            case ListColumn.Country:            result = 120; break;
            case ListColumn.CountMessages:      result = 40; break;
            case ListColumn.DebugCountPoints:   result = 30; break;
            case ListColumn.FlightLevel:        result = 40; break;
            case ListColumn.FlightsCount:       result = 40; break;
            case ListColumn.Icao:               result = 50; break;
            case ListColumn.Silhouette:
            case ListColumn.OperatorFlag:       result = mOptions.tempFlagWidth; break;
            case ListColumn.Picture:            result = 60; break;
            case ListColumn.Registration:       result = 60; break;
            case ListColumn.RowHeader:          result = 10; break;
            case ListColumn.Speed:              result = 45; break;
            case ListColumn.Squawk:             result = 50; break;
            case ListColumn.Type:               result = 50; break;
            case ListColumn.Latitude:           result = 60; break;
            case ListColumn.Longitude:          result = 60; break;
            case ListColumn.Heading:            result = 50; break;
            case ListColumn.Model:              result = 150; break;
            case ListColumn.Operator:           result = 200; break;
            case ListColumn.VerticalSpeed:      result = 50; break;
            case ListColumn.Distance:           result = 50; break;
            case ListColumn.Engines:            result = 70; break;
            case ListColumn.Species:            result = 70; break;
            case ListColumn.TimeTracked:        result = 40; break;
            case ListColumn.Wtc:                result = 50; break;
            case ListColumn.UserTag:            result = 60; break;
            case ListColumn.Route:              result = 100; break;
        }

        return result;
    };

    this.hasMandatoryWidth = function(listColumn)
    {
        var result = false;
        switch(listColumn) {
            case ListColumn.Silhouette:
            case ListColumn.OperatorFlag:
            case ListColumn.RowHeader:
            case ListColumn.Picture:
                result = true;
                break;
        }

        return result;
    };

    this.getCustomColumnSetWidth = function()
    {
        var result = 0;
        for(var i = 0;i < mCustomColumns.length;++i) {
            result += that.getColumnWidth(mCustomColumns[i]);
        }

        return result;
    };
}