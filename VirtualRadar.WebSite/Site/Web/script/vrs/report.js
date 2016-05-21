var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportDefaultStepSize = VRS.globalOptions.reportDefaultStepSize || 25;
    VRS.globalOptions.reportDefaultPageSize = VRS.globalOptions.reportDefaultPageSize || 50;
    VRS.globalOptions.reportUrl = VRS.globalOptions.reportUrl || 'ReportRows.json';
    VRS.globalOptions.reportDefaultSortColumns = VRS.globalOptions.reportDefaultSortColumns || [
        { field: VRS.ReportSortColumn.Date, ascending: true },
        { field: VRS.ReportSortColumn.None, ascending: false }
    ];
    VRS.globalOptions.reportShowPermanentLinkToReport = VRS.globalOptions.reportShowPermanentLinkToReport !== undefined ? VRS.globalOptions.reportShowPermanentLinkToReport : true;
    VRS.globalOptions.reportUseRelativeDatesInLink = VRS.globalOptions.reportUseRelativeDatesInLink !== undefined ? VRS.globalOptions.reportUseRelativeDatesInLink : true;
    var Report = (function () {
        function Report(settings) {
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.Report'
            });
            this._Events = {
                buildingRequest: 'buildingRequest',
                pageSizeChanged: 'pageSizeChanged',
                rowsFetched: 'rowsFetched',
                failedNoCriteria: 'failedNoCriteria',
                fetchFailed: 'fetchFailed',
                selectedFlightChanged: 'selectedFlightChanged'
            };
            this._LastFetchResult = null;
            this._SortColumns = VRS.globalOptions.reportDefaultSortColumns.slice();
            this._SelectedFlight = null;
            this._PageSize = VRS.globalOptions.reportDefaultPageSize;
            this._Settings = $.extend({
                name: 'default',
                autoSaveState: false,
                showFetchUI: true
            }, settings);
            this._Criteria = new VRS.ReportCriteria({
                name: this._Settings.name,
                unitDisplayPreferences: this._Settings.unitDisplayPreferences
            });
        }
        Report.prototype.getName = function () {
            return this._Settings.name;
        };
        Report.prototype.getCriteria = function () {
            return this._Criteria;
        };
        Report.prototype.getSortColumnsLength = function () {
            return this._SortColumns.length;
        };
        Report.prototype.getSortColumn = function (index) {
            var existing = this._SortColumns[index];
            return {
                field: existing.field,
                ascending: existing.ascending
            };
        };
        Report.prototype.setSortColumn = function (index, value) {
            var existing = this._SortColumns[index];
            if (existing.field !== value.field || existing.ascending !== value.ascending) {
                this._SortColumns[index] = value;
            }
        };
        Report.prototype.getGroupSortColumn = function () {
            return this._LastFetchResult && !this._LastFetchResult.errorText
                ? this._LastFetchResult.groupBy
                : VRS.ReportSortColumn.None;
        };
        Report.prototype.getFlights = function () {
            return this._LastFetchResult && !this._LastFetchResult.errorText && this._LastFetchResult.flights
                ? this._LastFetchResult.flights
                : [];
        };
        Report.prototype.getLastError = function () {
            return this._LastFetchResult && this._LastFetchResult.errorText ? this._LastFetchResult.errorText : null;
        };
        Report.prototype.getSelectedFlight = function () {
            return this._SelectedFlight;
        };
        Report.prototype.setSelectedFlight = function (value) {
            if (value !== this._SelectedFlight) {
                this._SelectedFlight = value;
                this._Dispatcher.raise(this._Events.selectedFlightChanged, [this]);
            }
        };
        Report.prototype.getPageSize = function () {
            return this._PageSize;
        };
        Report.prototype.setPageSize = function (value) {
            if (this._PageSize !== value) {
                this._PageSize = value;
                if (this._Settings.autoSaveState && value > 0) {
                    this.saveState();
                }
                this._Dispatcher.raise(this._Events.pageSizeChanged, [this]);
            }
        };
        Report.prototype.getCountRowsAvailable = function () {
            return this._LastFetchResult && !this._LastFetchResult.errorText ? this._LastFetchResult.countRows : 0;
        };
        Report.prototype.isReportPaged = function () {
            return this._PageSize > 0;
        };
        Report.prototype.hasData = function () {
            return !!(this._LastFetchResult && this._LastFetchResult.flights.length);
        };
        Report.prototype.hookFailedNoCriteria = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.failedNoCriteria, callback, forceThis);
        };
        Report.prototype.hookBuildingRequest = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.buildingRequest, callback, forceThis);
        };
        Report.prototype.hookFetchFailed = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.fetchFailed, callback, forceThis);
        };
        Report.prototype.hookPageSizeChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.pageSizeChanged, callback, forceThis);
        };
        Report.prototype.hookRowsFetched = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.rowsFetched, callback, forceThis);
        };
        Report.prototype.hookSelectedFlightCHanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.selectedFlightChanged, callback, forceThis);
        };
        Report.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        Report.prototype.saveState = function () {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        };
        Report.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        };
        Report.prototype.applyState = function (settings) {
            this.setPageSize(settings.pageSize);
        };
        Report.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        Report.prototype.persistenceKey = function () {
            return 'vrsReport-' + this.getName();
        };
        Report.prototype.createSettings = function () {
            return {
                pageSize: this.getPageSize()
            };
        };
        Report.prototype.createOptionPane = function (displayOrder) {
            var _this = this;
            var result = [];
            result.push(this._Criteria.createOptionPane(displayOrder));
            var runReportFieldButton = new VRS.OptionFieldButton({
                name: 'runButton',
                labelKey: 'RunReport',
                saveState: function () { _this.fetchPage(); },
                primaryIcon: 'refresh'
            });
            runReportFieldButton.setEnabled(this.getCriteria().hasCriteria());
            var criteriaChangedHookResult = this.getCriteria().hookCriteriaChanged(function () {
                runReportFieldButton.setEnabled(this.getCriteria().hasCriteria());
            }, this);
            var sortPane = new VRS.OptionPane({
                name: 'report_' + this._Settings.name,
                titleKey: 'PaneSortReport',
                displayOrder: displayOrder + 10,
                dispose: function () {
                    if (criteriaChangedHookResult) {
                        _this.getCriteria().unhook(criteriaChangedHookResult);
                    }
                }
            });
            result.push(sortPane);
            var sortColumnValues = this.getSortColumnValues();
            sortColumnValues.sort(function (lhs, rhs) {
                if (lhs.getValue() === VRS.ReportSortColumn.None)
                    return rhs.getValue() === VRS.ReportSortColumn.None ? 0 : -1;
                if (rhs.getValue() === VRS.ReportSortColumn.None)
                    return 1;
                var lhsText = lhs.getText() || '';
                var rhsText = rhs.getText() || '';
                return lhsText.localeCompare(rhsText);
            });
            $.each(this._SortColumns, function (idx) {
                sortPane.addField(new VRS.OptionFieldComboBox({
                    name: 'sortBy' + idx.toString(),
                    labelKey: idx === 0 ? 'SortBy' : 'ThenBy',
                    getValue: function () { return _this.getSortColumn(idx).field; },
                    setValue: function (value) {
                        var setEntry = _this.getSortColumn(idx);
                        setEntry.field = value;
                        _this.setSortColumn(idx, setEntry);
                    },
                    saveState: $.noop,
                    values: sortColumnValues,
                    keepWithNext: true
                }));
                sortPane.addField(new VRS.OptionFieldCheckBox({
                    name: 'ascending' + idx.toString(),
                    labelKey: 'Ascending',
                    getValue: function () { return _this.getSortColumn(idx).ascending; },
                    setValue: function (value) {
                        var setEntry = _this.getSortColumn(idx);
                        setEntry.ascending = value;
                        _this.setSortColumn(idx, setEntry);
                    },
                    saveState: $.noop
                }));
            });
            var runButtonPane = sortPane;
            if (VRS.globalOptions.reportShowPermanentLinkToReport) {
                var useRelativeDates = VRS.globalOptions.reportUseRelativeDatesInLink;
                var linkUrl = '';
                var hookResults = [];
                var linkLabel = new VRS.OptionFieldLinkLabel({
                    name: 'linkLabel',
                    labelKey: function () { return linkUrl; },
                    getHref: function () { return linkUrl; },
                    getTarget: function () { return 'vrsReport'; }
                });
                var refreshLink = function () {
                    var newUrl = _this._formPermanentLinkUrl(useRelativeDates);
                    if (newUrl !== linkUrl) {
                        linkUrl = newUrl;
                        linkLabel.raiseRefreshFieldContent();
                    }
                };
                var linkPane = new VRS.OptionPane({
                    name: 'permanentLink',
                    titleKey: 'PanePermanentLink',
                    displayOrder: displayOrder + 20,
                    fields: [
                        new VRS.OptionFieldCheckBox({
                            name: 'relativeDates',
                            labelKey: 'UseRelativeDates',
                            getValue: function () { return useRelativeDates; },
                            setValue: function (value) { return useRelativeDates = value; },
                            saveState: $.noop
                        }),
                        linkLabel
                    ],
                    pageParentCreated: function (optionPageParent) {
                        var hookResult = optionPageParent.hookFieldChanged(refreshLink, self);
                        hookResults.push({
                            hookResult: hookResult,
                            optionPageParent: optionPageParent
                        });
                        refreshLink.call(_this);
                    },
                    dispose: function (options) {
                        var hookResultIndex = VRS.arrayHelper.indexOfMatch(hookResults, function (item) {
                            return item.optionPageParent === options.optionPageParent;
                        });
                        if (hookResultIndex !== -1) {
                            var hookResult = hookResults[hookResultIndex];
                            hookResults.splice(hookResultIndex, 1);
                            hookResult.optionPageParent.unhook(hookResult.hookResult);
                        }
                    }
                });
                result.push(linkPane);
                runButtonPane = linkPane;
            }
            result.push(new VRS.OptionPane({
                name: 'runButtonPane',
                displayOrder: displayOrder + 30,
                fields: [runReportFieldButton]
            }));
            return result;
        };
        Report.prototype.getSortColumnValues = function () {
            return [
                new VRS.ValueText({ value: VRS.ReportSortColumn.None, textKey: 'None' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Callsign, textKey: 'Callsign' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Date, textKey: 'Date' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.FirstAltitude, textKey: 'FirstAltitude' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Icao, textKey: 'Icao' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.LastAltitude, textKey: 'LastAltitude' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Model, textKey: 'Model' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.ModelIcao, textKey: 'ModelIcao' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Operator, textKey: 'Operator' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Registration, textKey: 'Registration' })
            ];
        };
        Report.prototype.populateFromQueryString = function () {
            var pageUrl = $.url();
            var length = this._SortColumns.length;
            for (var i = 0; i < length; ++i) {
                var sortFieldName = 'sort' + (i + 1);
                var sortAscendingName = 'sortAsc' + (i + 1);
                var sortField = pageUrl.param(sortFieldName);
                var sortAscending = pageUrl.param(sortAscendingName) || '1';
                if (sortField === 'none')
                    sortField = VRS.ReportSortColumn.None;
                if (sortField && VRS.enumHelper.getEnumName(VRS.ReportSortColumn, sortField)) {
                    this.setSortColumn(i, { field: sortField, ascending: sortAscending !== '0' });
                }
            }
            this.getCriteria().populateFromQueryString();
        };
        Report.prototype._formPermanentLinkUrl = function (useRelativeDates) {
            var result = '';
            var queryStringParams = this.getCriteria().createQueryString(useRelativeDates);
            var queryString = $.param(queryStringParams);
            if (queryString) {
                if (this.getCriteria().getFindAllPermutationsOfCallsign()) {
                    queryStringParams['callPerms'] = '1';
                }
                var length = this._SortColumns.length;
                for (var i = 0; i < length; ++i) {
                    var sortField = this._SortColumns[i];
                    if (sortField.field === VRS.ReportSortColumn.None) {
                        continue;
                    }
                    var sortFieldName = 'sort' + (i + 1);
                    var sortAscendingName = 'sortAsc' + (i + 1);
                    queryStringParams[sortFieldName] = sortField.field;
                    if (!sortField.ascending) {
                        queryStringParams[sortAscendingName] = '0';
                    }
                }
                queryString = $.param(queryStringParams);
                var pageUrl = $.url();
                result = pageUrl.attr('base') + pageUrl.attr('path') + '?' + queryString;
            }
            return result;
        };
        Report.prototype.fetchPage = function (pageNumber) {
            if (pageNumber === void 0) { pageNumber = 0; }
            if (!this.getCriteria().hasCriteria()) {
                this._Dispatcher.raise(this._Events.failedNoCriteria, [this]);
            }
            else {
                var firstRow = this._PageSize > 0 ? pageNumber * this._PageSize : -1;
                var lastRow = this._PageSize > 0 ? ((pageNumber + 1) * this._PageSize) - 1 : -1;
                var parameters = {
                    rep: 'date',
                    fromRow: firstRow,
                    toRow: lastRow
                };
                this.getCriteria().addToQueryParameters(parameters);
                var length = this._SortColumns.length;
                for (var i = 0; i < length; ++i) {
                    var sortColumn = this._SortColumns[i];
                    if (sortColumn.field !== VRS.ReportSortColumn.None) {
                        var propertyName = 'sort' + (i + 1);
                        parameters[propertyName] = sortColumn.field;
                        parameters[propertyName + 'dir'] = sortColumn.ascending ? 'asc' : 'desc';
                    }
                }
                if (this._Settings.showFetchUI) {
                    VRS.pageHelper.showModalWaitAnimation(true);
                }
                $.ajax({
                    url: VRS.globalOptions.reportUrl,
                    dataType: 'text',
                    data: parameters,
                    success: $.proxy(this.pageFetched, this),
                    error: $.proxy(this.fetchFailed, this),
                    cache: false
                });
            }
        };
        Report.prototype.pageFetched = function (rawData) {
            if (this._Settings.showFetchUI) {
                VRS.pageHelper.showModalWaitAnimation(false);
            }
            var json = VRS.jsonHelper.convertMicrosoftDates(rawData);
            this._LastFetchResult = eval('(' + json + ')');
            this.fixupRoutesAndAirports();
            this.setSelectedFlight(null);
            this._Dispatcher.raise(this._Events.rowsFetched, [this]);
            if (this._Settings.showFetchUI && this._LastFetchResult.errorText) {
                VRS.pageHelper.showMessageBox(VRS.$$.ServerReportExceptionTitle, VRS.stringUtility.format(VRS.$$.ServerReportExceptionBody, this._LastFetchResult.errorText));
            }
            else {
                var flights = this.getFlights();
                if (flights.length) {
                    this.setSelectedFlight(flights[0]);
                }
            }
        };
        Report.prototype.fixupRoutesAndAirports = function () {
            if (this._LastFetchResult.flights && this._LastFetchResult.aircraftList && this._LastFetchResult.routes) {
                var emptyRoute = {};
                var length_1 = this._LastFetchResult.flights.length;
                for (var i = 0; i < length_1; ++i) {
                    var flight = this._LastFetchResult.flights[i];
                    flight.route = flight.rtIdx > -1 ? this._LastFetchResult.routes[flight.rtIdx] : emptyRoute;
                    flight.aircraft = this._LastFetchResult.aircraftList[flight.acIdx];
                }
            }
            if (this._LastFetchResult.routes && this._LastFetchResult.airports) {
                var emptyAirport = {};
                var length_2 = this._LastFetchResult.routes.length;
                for (var i = 0; i < length_2; ++i) {
                    var route = this._LastFetchResult.routes[i];
                    route.from = route.fIdx > -1 ? this._LastFetchResult.airports[route.fIdx] : emptyAirport;
                    route.to = route.tIdx > -1 ? this._LastFetchResult.airports[route.tIdx] : emptyAirport;
                    route.via = [];
                    var sLength = route.sIdx.length;
                    for (var s = 0; s < sLength; ++s) {
                        var sidx = route.sIdx[s];
                        if (sidx > -1) {
                            route.via.push(this._LastFetchResult.airports[sidx]);
                        }
                    }
                }
            }
            if (this._LastFetchResult.airports) {
                var length_3 = this._LastFetchResult.airports.length;
                for (var i = 0; i < length_3; ++i) {
                    var airport = this._LastFetchResult.airports[i];
                    var fullName = '';
                    if (airport.code && airport.name)
                        fullName = airport.code + ' ' + airport.name;
                    else if (airport.code)
                        fullName = airport.code;
                    else
                        fullName = airport.name || '';
                    airport.fullName = fullName;
                }
            }
        };
        Report.prototype.fetchFailed = function (jqXHR, textStatus, errorThrown) {
            if (this._Settings.showFetchUI) {
                VRS.pageHelper.showModalWaitAnimation(false);
            }
            this._Dispatcher.raise(this._Events.fetchFailed, [
                this,
                jqXHR && $.isFunction(jqXHR.statusCode) ? jqXHR.statusCode() : undefined,
                textStatus,
                errorThrown
            ]);
            if (this._Settings.showFetchUI) {
                if (textStatus === 'timeout') {
                    VRS.pageHelper.showMessageBox(VRS.$$.ServerFetchFailedTitle, VRS.$$.ServerFetchTimedOut);
                }
                else {
                    VRS.pageHelper.showMessageBox(VRS.$$.ServerFetchFailedTitle, VRS.stringUtility.format(VRS.$$.ServerFetchFailedBody, errorThrown, textStatus));
                }
            }
        };
        Report.convertFlightToVrsAircraft = function (flight, useFirstValues) {
            var result = null;
            if (flight && flight.aircraft) {
                result = new VRS.Aircraft();
                result.altitude.setValue(useFirstValues ? flight.fAlt : flight.lAlt);
                result.callsign.setValue(flight.call);
                result.countEngines.setValue(flight.aircraft.engines);
                result.country.setValue(flight.aircraft.country);
                result.engineType.setValue(flight.aircraft.engType);
                result.enginePlacement.setValue(flight.aircraft.engMount);
                result.hasPicture.setValue(flight.aircraft.hasPic);
                result.heading.setValue(useFirstValues ? flight.fTrk : flight.lTrk);
                result.icao.setValue(flight.aircraft.icao);
                result.id = flight.row;
                result.isEmergency.setValue(flight.hEmg);
                result.isMilitary.setValue(flight.aircraft.military);
                result.isOnGround.setValue(useFirstValues ? flight.fOnGnd : flight.lOnGnd);
                result.latitude.setValue(useFirstValues ? flight.fLat : flight.lLat);
                result.longitude.setValue(useFirstValues ? flight.fLng : flight.lLng);
                result.model.setValue(flight.aircraft.typ);
                result.modelIcao.setValue(flight.aircraft.icaoType);
                result.operator.setValue(flight.aircraft.owner);
                result.operatorIcao.setValue(flight.aircraft.opFlag);
                result.positionTime.setValue(result.latitude.val || result.longitude.val ? 1 : 0);
                result.registration.setValue(flight.aircraft.reg);
                result.species.setValue(flight.aircraft.species);
                result.speed.setValue(useFirstValues ? flight.fSpd : flight.lSpd);
                result.squawk.setValue(useFirstValues ? flight.fSqk ? flight.fSqk.toString() : undefined : flight.lSqk ? flight.lSqk.toString() : undefined);
                result.userInterested.setValue(flight.aircraft.interested);
                result.verticalSpeed.setValue(useFirstValues ? flight.fVsi : flight.lVsi);
                result.wakeTurbulenceCat.setValue(flight.aircraft.wtc);
                result.from.setValue(flight.route.from ? flight.route.from.fullName : null);
                result.to.setValue(flight.route.to ? flight.route.to.fullName : null);
                if (flight.route.via) {
                    var via = [];
                    $.each(flight.route.via, function (idx, viaAirport) {
                        via.push(viaAirport.fullName);
                    });
                    result.via.setValue(via);
                }
            }
            return result;
        };
        ;
        return Report;
    })();
    VRS.Report = Report;
})(VRS || (VRS = {}));
