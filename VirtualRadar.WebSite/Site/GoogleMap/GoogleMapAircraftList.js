var GoogleMapAircraftListObjects = [];

function googleMapAircraftListLinkClicked(idx, id)  { GoogleMapAircraftListObjects[idx].listLineClicked(id); };
function googleMapAircraftListPauseClicked(idx)     { GoogleMapAircraftListObjects[idx].togglePauseClicked(); };

function GoogleMapAircraftList(events, aircraftCollection, options, sidebar)
{
    var that = this;
    var mGlobalIndex = GoogleMapAircraftListObjects.length;
    GoogleMapAircraftListObjects.push(that);

    var mEvents = events;
    var mAircraftCollection = aircraftCollection;
    var mOptions = options;
    var mSidebar = sidebar;
    var mListElement = document.getElementById('list_table');
    var mListColumns = new GoogleMapAircraftListColumns(mOptions, mEvents);
    this.getListColumns = function() { return mListColumns; } ;
    var mColumns = mListColumns.getColumns();
    var mRowAircraftIds = [];
    var mDebugFixedColumnWidths = false;

    this.setDebugCountPoints = function(value) { mListColumns.setShowDebugCountPoints(value); }
    this.setDebugFixedColumnWidths = function(value) { mDebugFixedColumnWidths = value; }

    addToUI();

    mEvents.addListener(EventId.acListRefreshed, aircraftListRefreshedHandler);
    mEvents.addListener(EventId.listChooseColumnsChanged, columnLayoutChangedHandler);
    mEvents.addListener(EventId.optionsChanged, optionsChangedHandler);
    mEvents.addListener(EventId.acDeselected, aircraftSelectionChangedHandler);
    mEvents.addListener(EventId.acSelected, aircraftSelectionChangedHandler);
    mEvents.addListener(EventId.pauseChanged, pauseChangedHandler);

    function columnLayoutChangedHandler(sender, args) { that.refreshCurrentList(false); };
    function optionsChangedHandler(sender, args) { that.refreshCurrentList(true); };
    function aircraftListRefreshedHandler(sender, args) { fillAircraftList(args); }
    function aircraftSelectionChangedHandler(sender, args)
    {
        var aircraft = args.aircraft;
        if(aircraft !== null) {
            var length = mRowAircraftIds.length;
            for(var rowNumber = 0;rowNumber < length;++rowNumber) {
                var id = mRowAircraftIds[rowNumber];
                if(id === aircraft.Id) {
                    var row = mListElement.tBodies[0].rows[rowNumber];
                    setRowClassAndId(row, aircraft, rowNumber);
                    break;
                }
            }
        }
    }

    this.listLineClicked = function(id)
    {
        var aircraft = mAircraftCollection.findAircraft(id);
        if(aircraft !== null) mAircraftCollection.selectAircraft(aircraft, true);
    };

    this.togglePauseClicked = function()
    {
        mEvents.raise(EventId.togglePauseClicked, null, null);
    };

    function addToUI()
    {
        var trackingText = 'Tracking <span id="list_aircraftCount">0</span> aircraft<span id="list_availableAircraftCount"></span>';
        if(mOptions.canRunReports()) {
            trackingText = formatDateReportLink(trackingText, 'today');
        }

        var html = '<div class="listTracking">' + trackingText + '</div>';
        html += '<div class="listPause"><a href="#" onClick="googleMapAircraftListPauseClicked(' + mGlobalIndex + ')"><span id="list_pauseState">Pause</span></div>';

        document.getElementById("list_heading").innerHTML = html;

        createListHeader();
    };

    function formatDateReportLink(text, date)
    {
        var result = text === null ? '' : text;
        if(_MapMode !== MapMode.flightSim) {
            if(mOptions.canRunReports()) {
                var dateText = date === 'today' ? date : date.getFullYear().toString() + '-' + (date.getMonth() + 1).toString() + '-' + date.getDate().toString();
                var forceFrame = getPageParameterValue(null, 'forceFrame');
                forceFrame = forceFrame ? '&forceFrame=' + encodeURIComponent(forceFrame) : '';
                result = result === null ? '' : '<a href="DateReport.htm?date=' + dateText + forceFrame + '"' + target('dateReport') + '>' + text + '</a>';
            }
        }

        return result;
    }

    function pauseChangedHandler(sender, args)
    {
        var paused = args;
        document.getElementById('list_pauseState').innerHTML = paused ? 'Resume' : 'Pause';
    };

    this.refreshCurrentList = function(forceFullRefresh)
    {
        if(forceFullRefresh) mRowAircraftIds = [];
        fillAircraftList(mAircraftCollection.getAllAircraft());
    };

    function fillAircraftList(aircraftList)
    {
        if(mListColumns.getCanShowSilhouettes() !== mOptions.tempShowSilhouettes || mListColumns.getCanShowFlags() !== mOptions.tempShowFlags || mListColumns.getCanShowPictures() !== mOptions.tempShowPictures) {
            mListColumns.setCanShowSilhouettes(mOptions.tempShowSilhouettes);
            mListColumns.setCanShowFlags(mOptions.tempShowFlags);
            mListColumns.setCanShowPictures(mOptions.tempShowPictures);
        }
        if(mListColumns.columnsHaveChanged()) {
            mColumns = mListColumns.getColumns();
            useNewColumnLayout();
            mRowAircraftIds = [];
        }

        document.getElementById('list_aircraftCount').innerHTML = aircraftList.length;

        var availableAircraftCount = mOptions.tempAvailableAircraft === aircraftList.length ? '' : '&nbsp;(out of ' + mOptions.tempAvailableAircraft + ')';
        document.getElementById('list_availableAircraftCount').innerHTML = availableAircraftCount;

        // We want to limit the number of new pictures we add to the list within each update
        // to try to avoid hammering the server
        var newPicturesCount = 0;
        var newPicturesLimit = Math.min(8, (mOptions.refreshSeconds + 1) * 2);

        var listBody = mListElement.tBodies[0];
        var rowCount = listBody.rows.length;

        while(mRowAircraftIds.length > aircraftList.length) mRowAircraftIds.pop();
        while(mRowAircraftIds.length < aircraftList.length) mRowAircraftIds.push(null);

        var alLength = aircraftList.length;
        var colLength = mColumns.length;
        for(var i = 0;i < alLength;++i) {
            var aircraft = aircraftList[i];

            var rowAircraftId = mRowAircraftIds.length > i ? mRowAircraftIds[i] : null;
            mRowAircraftIds[i] = aircraft.Id;
            var refreshRow = rowAircraftId === null || rowAircraftId !== aircraft.Id;

            if(rowCount <= i) {
                createListRow(listBody);
                ++rowCount;
            }
            var row = listBody.rows[i];

            setRowClassAndId(row, aircraft, i);

            var selectOnClick = 'onclick="googleMapAircraftListLinkClicked(' + mGlobalIndex + ',' + aircraft.Id + ')"';
            var modelDescription = joinCodeAndDescription(aircraft.Type, aircraft.Mdl, aircraft.formatIcao8643Details());
            var operatorDescription = joinCodeAndDescription(aircraft.OpIcao, aircraft.Op);

            var cellContent = null;
            var toolTip = null;
            for(var c = 0;c < colLength;c++) {
                cellContent = null;
                toolTip = null;

                var cell = row.cells[c];
                switch(mColumns[c]) {
                    case ListColumn.RowHeader:      if(refreshRow) cellContent = '<img src="Images/RowHeader.png" width="10px" height="20px" alt="" title="" ' + selectOnClick + ' />'; break;
                    case ListColumn.Silhouette:
                        if(refreshRow || aircraft.TypeChanged) {
                            cellContent = aircraft.formatSilhouette(selectOnClick);
                            toolTip = modelDescription;
                        }
                        break;
                    case ListColumn.OperatorFlag:
                        if(refreshRow || (aircraft.OpIcaoChanged || aircraft.OpChanged)) {
                            cellContent = aircraft.formatOperatorFlag(selectOnClick);
                            toolTip = operatorDescription;
                        }
                        break;
                    case ListColumn.Picture:
                        if(refreshRow || aircraft.HasPicChanged || aircraft.listForcedBlankPicture) {
                            // This will limit the number of fresh <img> tags that we add to the page within each refresh of the list
                            var forceBlank = aircraft.HasPic && !aircraft.listHasShownPicture ? ++newPicturesCount > newPicturesLimit : false;
                            if(forceBlank) aircraft.listForcedBlankPicture = true;
                            else if(aircraft.listForcedBlankPicture) delete aircraft.listForcedBlankPicture;
                            if(aircraft.HasPic && !forceBlank) aircraft.listHasShownPicture = true;

                            cellContent = aircraft.formatPicture(selectOnClick, null, false, 'list', 60, 40, forceBlank);
                        }
                        break;
                    case ListColumn.Registration:   if(refreshRow || aircraft.RegChanged) cellContent = aircraft.formatRegReportLink(); break;
                    case ListColumn.Icao:           if(refreshRow || aircraft.IcaoChanged) cellContent = aircraft.formatIcaoReportLink(); break;
                    case ListColumn.Callsign:       if(refreshRow || aircraft.CallChanged || aircraft.CallSusChanged) { cellContent = aircraft.formatCallsignReportLink() + (aircraft.CallSus ? '*' : ''); toolTip = aircraft.formatRouteTooltip() + (aircraft.CallSus ? '. Callsign may not be correct.' : ''); } break;
                    case ListColumn.CivOrMil:       if(refreshRow || aircraft.MilChanged) cellContent = aircraft.Mil ? 'Yes' : aircraft.Mil !== undefined ? 'No' : ''; break;
                    case ListColumn.Country:        if(refreshRow || aircraft.CouChanged) cellContent = aircraft.Cou; break;
                    case ListColumn.CountMessages:  if(refreshRow || aircraft.CMsgsChanged) cellContent = aircraft.formatCountMessages(); break;
                    case ListColumn.Type:           if(refreshRow || aircraft.TypeChanged) { cellContent = aircraft.Type; toolTip = modelDescription; } break;
                    case ListColumn.Altitude:       if(refreshRow || aircraft.ConvertedAltChanged) cellContent = aircraft.ConvertedAlt === null ? '' : aircraft.ConvertedAlt; break;
                    case ListColumn.FlightLevel:    if(refreshRow || aircraft.FlightLevelChanged) cellContent = aircraft.FlightLevel === null ? '' : aircraft.formatFlightLevel(); break;
                    case ListColumn.Squawk:         if(refreshRow || aircraft.SqkChanged) cellContent = aircraft.Sqk; break;
                    case ListColumn.Speed:          if(refreshRow || aircraft.ConvertedSpdChanged || aircraft.SpeedTypeChanged) { cellContent = (aircraft.ConvertedSpd === null ? '' : aircraft.ConvertedSpd) + (aircraft.SpeedType === 0 ? '' : '*'); toolTip = aircraft.SpeedType === 0 ? '' : aircraft.formatSpeedType(true); } break;
                    case ListColumn.Latitude:       if(refreshRow || aircraft.LatChanged) cellContent = aircraft.Lat === null ? '' : aircraft.Lat; break;
                    case ListColumn.Longitude:      if(refreshRow || aircraft.LongChanged) cellContent = aircraft.Long === null ? '' : aircraft.Long; break;
                    case ListColumn.Heading:        if(refreshRow || aircraft.TrakChanged) cellContent = aircraft.Trak === null ? '' : aircraft.Trak; break;
                    case ListColumn.Bearing:        if(refreshRow || aircraft.BrngChanged) cellContent = aircraft.Brng === null ? '' : aircraft.Brng; break;
                    case ListColumn.Model:          if(refreshRow || aircraft.MdlChanged) cellContent = aircraft.Mdl; break;
                    case ListColumn.Operator:       if(refreshRow || aircraft.OpChanged) cellContent = aircraft.Op; break;
                    case ListColumn.VerticalSpeed:  if(refreshRow || aircraft.ConvertedVsiChanged) cellContent = aircraft.ConvertedVsi === null ? '' : aircraft.ConvertedVsi; break;
                    case ListColumn.Distance:       if(refreshRow || aircraft.ConvertedDstChanged) cellContent = aircraft.ConvertedDst === null ? '' : aircraft.ConvertedDst; break;
                    case ListColumn.Engines:        if(refreshRow || aircraft.EnginesChanged || aircraft.EngTypeChanged) cellContent = capitaliseSentence(aircraft.formatEngines()); break;
                    case ListColumn.Species:        if(refreshRow || aircraft.SpeciesChanged) cellContent = capitaliseSentence(aircraft.formatSpecies(true)); break;
                    case ListColumn.Wtc:            if(refreshRow || aircraft.WTCChanged) cellContent = capitaliseSentence(aircraft.formatWakeTurbulenceCategory(true)); break;
                    case ListColumn.TimeTracked:    cellContent = aircraft.formatTrackedSeconds(); break;
                    case ListColumn.FlightsCount:   if(refreshRow || aircraft.FlightsCountChanged) cellContent = aircraft.FlightsCount === null ? '' : aircraft.FlightsCount; break;
                    case ListColumn.UserTag:        if(refreshRow || aircraft.TagChanged) cellContent = aircraft.Tag === null ? '' : aircraft.Tag; break;
                    case ListColumn.Route:          if(refreshRow || aircraft.FromChanged || aircraft.ToChanged || aircraft.StopsChanged) { cellContent = aircraft.formatRouteListText(); toolTip = aircraft.formatRouteTooltip(); } break;
                    case ListColumn.DebugCountPoints:
                        var debugCountArgs = { aircraft: aircraft, result: 0 };
                        mEvents.raise(EventId.debugGetCountPoints, null, debugCountArgs);
                        cellContent = debugCountArgs.result;
                        break;
                }
                if(cellContent !== null) cell.innerHTML = cellContent;
                if(toolTip !== null) cell.setAttribute('title', toolTip);
            }
        }

        for(i = aircraftList.length;i < rowCount;++i) {
            row = listBody.rows[i];
            setClass(row, 'hidden');
            row.setAttribute('id', '');

            if(i < mRowAircraftIds.length) mRowAircraftIds[i] = null;
        }
    };

    function setRowClassAndId(row, aircraft, rowIndex)
    {
        var classText = (rowIndex % 2) == 1 ? 'even' : '';
        var idText = 'aid_' + aircraft.Id;
        if(aircraft.Help) classText = 'emergency';
        else if(aircraft.Selected) classText = 'selected';
        if(row.getAttribute('id') != idText) row.setAttribute('id', idText);
        if(row.getAttribute('class') != classText) setClass(row, classText);
    }

    function joinCodeAndDescription(code, description1, description2)
    {
        var description = description1 ? description1 : '';
        if(description2 && description2.length > 0) description += (description.length == 0 ? '' : ', ') + description2;

        var result = code === null ? '' : code;
        if(description.length > 0) result += ': ' + description;

        return result;
    };

    function createListHeader()
    {
        var row = document.createElement('tr');
        mListElement.tHead.appendChild(row);
        addHeadingCells(row);
    };

    function createListRow(listBody)
    {
        var row = document.createElement('tr');
        row.setAttribute('id', '');
        listBody.appendChild(row);
        addDataCells(row);
    };

    function useNewColumnLayout()
    {
        var listHead = mListElement.tHead;
        var headRow = listHead.rows[0];
        removeCellsFromRow(headRow);

        var tableWidth = '100%';
        var sidebarWidth = mSidebar === undefined ? 0 : mSidebar.getInitialWidth();
        if(mListColumns.getUseCustomColumns()) {
            var idealTableWidth = mListColumns.getCustomColumnSetWidth();
            if(idealTableWidth > sidebarWidth || mDebugFixedColumnWidths) {
                tableWidth = idealTableWidth.toString() + 'px';
                sidebarWidth = idealTableWidth;
            }
        }
        var stylesheetIndex = findStylesheet('.googleMapStylesheet');
        var tableCss = getCSSRule(stylesheetIndex, '#list_table');
        tableCss.style.width = tableWidth;
        if(mSidebar !== undefined) mSidebar.setWidth(sidebarWidth);

        addHeadingCells(headRow);

        var listBody = mListElement.tBodies[0];
        for(var rowNum = 0;rowNum < listBody.rows.length;++rowNum) {
            var row = listBody.rows[rowNum];
            removeCellsFromRow(row);
            addDataCells(row);
        }
    };

    function removeCellsFromRow(row)
    {
        while(row.cells.length > 0) {
            row.deleteCell(-1);
        }
    };

    function addHeadingCells(headingRow)
    {
        for(var i = 0;i < mColumns.length;++i) {
            var headerText = null;
            var cell = document.createElement('th');
            var align = null;
            switch(mColumns[i]) {
                case ListColumn.RowHeader:      setClass(cell, 'cellRowHeader'); break;
                case ListColumn.Altitude:       headerText = 'Alt.'; align = 'right'; break;
                case ListColumn.Callsign:       headerText = 'Callsign'; align = 'left'; break;
                case ListColumn.CivOrMil:       headerText = 'Military'; align = 'left'; break;
                case ListColumn.Country:        headerText = 'Country'; align = 'left'; break;
                case ListColumn.CountMessages:  headerText = 'Msgs'; align = 'right'; break;
                case ListColumn.FlightLevel:    headerText = 'Alt.'; align = 'right'; break;
                case ListColumn.Icao:           headerText = 'ICAO'; cell.align = 'left'; break;
                case ListColumn.OperatorFlag:   setClass(cell, 'cellOperatorFlag'); headerText = 'Flag'; break;
                case ListColumn.Registration:   setClass(cell, 'cellRegistration'); headerText = 'Reg.'; align = 'left'; break;
                case ListColumn.Silhouette:     setClass(cell, 'cellSilhouette'); headerText = 'Silhouette'; break;
                case ListColumn.Picture:        setClass(cell, 'cellPicture'); headerText = 'Picture'; break;
                case ListColumn.Speed:          headerText = 'Speed'; align = 'right'; break;
                case ListColumn.Squawk:         headerText = 'Squawk'; align = 'right'; break;
                case ListColumn.Type:           headerText = 'Type'; align = 'left'; break;
                case ListColumn.Latitude:       headerText = 'Lat.'; align = 'right'; break;
                case ListColumn.Longitude:      headerText = 'Lng.'; align = 'right'; break;
                case ListColumn.Heading:        headerText = 'Hdng.'; align = 'right'; break;
                case ListColumn.Bearing:        headerText = 'Brng.'; align = 'right'; break;
                case ListColumn.Model:          headerText = 'Model'; break;
                case ListColumn.Operator:       headerText = 'Operator'; break;
                case ListColumn.VerticalSpeed:  headerText = 'VSpd.'; align = 'right'; break;
                case ListColumn.Distance:       headerText = 'Dist.'; align = 'right'; break;
                case ListColumn.Engines:        headerText = 'Engines'; align = 'left'; break;
                case ListColumn.Species:        headerText = 'Species'; align = 'left'; break;
                case ListColumn.Wtc:            headerText = 'WTC'; align = 'left'; break;
                case ListColumn.TimeTracked:    headerText = 'Tracked'; align = 'right'; break;
                case ListColumn.FlightsCount:   headerText = 'Seen'; align = 'right'; break;
                case ListColumn.UserTag:        headerText = 'Tag'; align = 'left'; break;
                case ListColumn.Route:          headerText = 'Route'; align = 'left'; break;
                case ListColumn.DebugCountPoints: headerText = '# Points'; align = 'right'; break;
            }
            setCellWidth(cell, mColumns[i]);
            if(headerText !== null) cell.appendChild(document.createTextNode(headerText));
            if(align !== null) cell.align = align;
            headingRow.appendChild(cell);
        }
    };

    function addDataCells(dataRow)
    {
        for(var i = 0;i < mColumns.length;++i) {
            var cell = document.createElement('td');
            var align = null;
            switch(mColumns[i]) {
                case ListColumn.RowHeader:      setClass(cell, 'cellRowHeader'); break;
                case ListColumn.Altitude:       align = 'right'; break;
                case ListColumn.CountMessages:  align = 'right'; break;
                case ListColumn.FlightLevel:    align = 'right'; break;
                case ListColumn.Speed:          align = 'right'; break;
                case ListColumn.Squawk:         align = 'right'; break;
                case ListColumn.OperatorFlag:   setClass(cell, 'cellOperatorFlag'); break;
                case ListColumn.Silhouette:     setClass(cell, 'cellSilhouette'); break;
                case ListColumn.Registration:   setClass(cell, 'cellRegistration'); break;
                case ListColumn.Picture:        setClass(cell, 'cellPicture'); break;
                case ListColumn.Latitude:       align = 'right'; break;
                case ListColumn.Longitude:      align = 'right'; break;
                case ListColumn.Heading:        align = 'right'; break;
                case ListColumn.Bearing:        align = 'right'; break;
                case ListColumn.VerticalSpeed:  align = 'right'; break;
                case ListColumn.Distance:       align = 'right'; break;
                case ListColumn.TimeTracked:    align = 'right'; break;
                case ListColumn.FlightsCount:   align = 'right'; break;
                case ListColumn.DebugCountPoints: align = 'right'; break;
            }
            setCellWidth(cell, mColumns[i]);
            if(align !== null) cell.align = align;
            dataRow.appendChild(cell);
        }
    };

    function setCellWidth(cell, listColumn)
    {
        var useFixedWidth = mListColumns.hasMandatoryWidth(listColumn);
        if(mDebugFixedColumnWidths && mListColumns.getUseCustomColumns()) useFixedWidth = true;
        if(useFixedWidth) cell.width = mListColumns.getColumnWidth(listColumn);
    };
}