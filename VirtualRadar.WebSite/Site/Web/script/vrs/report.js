/**
 * @license Copyright © 2013 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview Describes a report.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportDefaultStepSize = VRS.globalOptions.reportDefaultStepSize || 25;            // The default step to show on the page size controls.
    VRS.globalOptions.reportDefaultPageSize = VRS.globalOptions.reportDefaultPageSize || 50;            // The default page size to show. Use -1 if you want to default to showing all rows.
    VRS.globalOptions.reportUrl = VRS.globalOptions.reportUrl || 'ReportRows.json';                     // The URL to fetch when retrieving report rows.
    VRS.globalOptions.reportDefaultSortColumns = VRS.globalOptions.reportDefaultSortColumns || [        // The default sort order for reports. Note that the server will not accept any more than two sort columns.
        { field: VRS.ReportSortColumn.Date, ascending: true },
        { field: VRS.ReportSortColumn.None, ascending: false }
    ];
    VRS.globalOptions.reportShowPermanentLinkToReport = VRS.globalOptions.reportShowPermanentLinkToReport !== undefined ? VRS.globalOptions.reportShowPermanentLinkToReport : true; // True to show the permanent link to the report in the criteria UI, false to suppress it.
    VRS.globalOptions.reportUseRelativeDatesInLink = VRS.globalOptions.reportUseRelativeDatesInLink !== undefined ? VRS.globalOptions.reportUseRelativeDatesInLink : true;          // True to default the 'use relative dates' checkbox in the permanent link to true.
    //endregion

    /**
     * The object that brings together the different parts of a report.
     * @param {Object}                      settings
     * @param {string}                      settings.name                       The name to assign to the report. Defaults to 'default'.
     * @param {boolean}                    [settings.autoSaveState]             True if the state should be auto-saved when properties are changed. Defaults to false.
     * @param {boolean}                    [settings.showFetchUI]               True if the report should indicate visually that it is fetching rows and show error messages to the user on failure to fetch rows. Defaults to true.
     * @param {VRS.UnitDisplayPreferences}  settings.unitDisplayPreferences     The unit display preferences to apply to report values.
     * @constructor
     */
    VRS.Report = function(settings)
    {
        //region Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.Report'
        });
        var _Events = {
            buildingRequest:        'buildingRequest',
            pageSizeChanged:        'pageSizeChanged',
            rowsFetched:            'rowsFetched',
            fetchFailed:            'fetchFailed',
            selectedFlightChanged:  'selectedFlightChanged'
        };

        var _Settings = $.extend({
            name:           'default',
            autoSaveState:  false,
            showFetchUI:    true
        }, settings);

        /**
         * The result of the last successful request for a page of results.
         * @type {VRS_JSON_REPORT}
         * @private
         */
        var _LastFetchResult = null;
        //endregion

        //region Properties
        /**
         * Gets the name used to store and load settings.
         * @returns {string}
         */
        this.getName = function() { return _Settings.name; };

        var _Criteria = new VRS.ReportCriteria({
            name: _Settings.name,
            unitDisplayPreferences: _Settings.unitDisplayPreferences
        });
        /**
         * Gets the criteria for the report.
         * @returns {VRS.ReportCriteria}
         */
        this.getCriteria = function() { return _Criteria; };

        /** @type {Array.<VRS_SORTREPORT>} @private */
        var _SortColumns = VRS.globalOptions.reportDefaultSortColumns.slice();
        /***
         * Gets the number of sort columns available to the user.
         * @returns {Number}
         */
        this.getSortColumnsLength = function() { return _SortColumns.length ; };
        /**
         * Gets the sort column at the index specified.
         * @param {Number} index
         * @returns {VRS_SORTREPORT}
         */
        this.getSortColumn = function(index) {
            var existing = _SortColumns[index];
            return { field: existing.field, ascending: existing.ascending };
        };
        this.setSortColumn = function(index, value) {
            var existing = _SortColumns[index];
            if(existing.field !== value.field || existing.ascending !== value.ascending) {
                _SortColumns[index] = value;
            }
        };

        /**
         * Returns the column on which the last set of results were sorted.
         * @returns {VRS.ReportSortColumn}
         */
        this.getGroupSortColumn = function() { return _LastFetchResult && !_LastFetchResult.errorText ? _LastFetchResult.groupBy : VRS.ReportSortColumn.None; };

        /**
         * Returns the flights last fetched by the report.
         * @returns {Array.<VRS_JSON_REPORT_FLIGHT>}
         */
        this.getFlights = function() { return _LastFetchResult && !_LastFetchResult.errorText && _LastFetchResult.flights ? _LastFetchResult.flights : []; };

        /**
         * Returns the error text from the last fetch or null if the last fetch did not encounter an error server-side.
         * @returns {string}
         */
        this.getLastError = function()
        {
            return _LastFetchResult && _LastFetchResult.errorText ? _LastFetchResult.errorText : null;
        };

        /** @type {VRS_JSON_REPORT_FLIGHT} @private */
        var _SelectedFlight = null;
        this.getSelectedFlight = function() { return _SelectedFlight; };
        this.setSelectedFlight = function(/** VRS_JSON_REPORT_FLIGHT */value) {
            if(value !== _SelectedFlight) {
                _SelectedFlight = value;
                _Dispatcher.raise(_Events.selectedFlightChanged, [ that ]);
            }
        };

        /** @type {number} @private */
        var _PageSize = VRS.globalOptions.reportDefaultPageSize;
        this.getPageSize = function() { return _PageSize; };
        this.setPageSize = function(/** number */ value) {
            if(_PageSize !== value) {
                _PageSize = value;
                if(_Settings.autoSaveState && value > 0) that.saveState();
                _Dispatcher.raise(_Events.pageSizeChanged, [ that ]);
            }
        };

        /**
         * Returns the number of rows that the server says are in the report. Note that this can change as more data is
         * collected by the server while the user is viewing the report. This is 0 if no rows are available.
         * @returns {number}
         */
        this.getCountRowsAvailable = function() { return _LastFetchResult && !_LastFetchResult.errorText ? _LastFetchResult.countRows : 0; };

        /**
         * Returns true if the report is broken up into pages, false if it is not.
         * @returns {boolean}
         */
        this.isReportPaged = function() { return _PageSize > 0; };

        /**
         * Returns true if the report has data to display, false if it does not.
         * @returns {boolean}
         */
        this.hasData = function() { return !!(_LastFetchResult && _LastFetchResult.flights.length); };
        //endregion

        //region Events exposed
        /**
         * Hooks an event that is raised before a request for a report page is made.
         * @param {function(VRS.Report, object, object)}    callback    Passed a reference to the report, the XHR parameters object and the XHR headers object.
         * @param {Object}                                  forceThis   The object to use as 'this' for the call.
         * @returns {Object}
         */
        this.hookBuildingRequest = function(callback, forceThis) { return _Dispatcher.hook(_Events.buildingRequest, callback, forceThis); };

        /**
         * Hooks an event that is raised after the report has failed to fetch a new batch of rows to display to the user.
         * @param {function(VRS.Report, number, string, string)}    callback    Passed a reference to the report, the status code of the request, the text status of the failure and the error thrown.
         * @param {Object}                                          forceThis   The object to use as 'this' for the event call.
         * @returns {Object}
         */
        this.hookFetchFailed = function(callback, forceThis) { return _Dispatcher.hook(_Events.fetchFailed, callback, forceThis); };

        /**
         * Hooks an event that is raised after the page size has been changed.
         * @param {function(VRS.Report)}    callback    Passed a reference to the report.
         * @param {Object}                  forceThis   The object to use as 'this' for the event call.
         * @returns {Object}
         */
        this.hookPageSizeChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.pageSizeChanged, callback, forceThis); };

        /**
         * Hooks an event that is raised after the report has fetched a new batch of rows to display to the user.
         * @param {function(VRS.Report)}    callback    Passed a reference to the report.
         * @param {Object}                  forceThis   The object to use as 'this' for the event call.
         * @returns {Object}
         */
        this.hookRowsFetched = function(callback, forceThis) { return _Dispatcher.hook(_Events.rowsFetched, callback, forceThis); };

        /**
         * Hooks an event that is raised after the selected flight has changed.
         * @param {function(VRS.Report)}    callback    Passed a reference to the report.
         * @param {Object}                  forceThis   The object to use as 'this' for the event callback.
         * @returns {Object}
         */
        this.hookSelectedFlightCHanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.selectedFlightChanged, callback, forceThis); };

        /**
         * Unhooks an event that was hooked on the object.
         * @param hookResult
         */
        this.unhook = function(hookResult) { _Dispatcher.unhook(hookResult); };
        //endregion

        //region saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         * @returns {VRS_STATE_REPORT}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            return $.extend(createSettings(), savedSettings);
        };

        /**
         * Applies a previously saved state to the object.
         * @param {VRS_STATE_REPORT} settings
         */
        this.applyState = function(settings)
        {
            that.setPageSize(settings.pageSize);
        };

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key under which the state will be saved.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsReport-' + that.getName();
        }

        /**
         * Creates the saved state object.
         * @returns {VRS_STATE_REPORT}
         */
        function createSettings()
        {
            return {
                pageSize:   that.getPageSize()
            };
        }
        //endregion

        //region createOptionPane
        this.createOptionPane = function(displayOrder)
        {
            var result = [];

            // Criteria pane
            result.push(_Criteria.createOptionPane(displayOrder));

            // Run button type settings - declared early as we need the reference for event handlers
            var runReportFieldButton = new VRS.OptionFieldButton({
                name:           'runButton',
                labelKey:       'RunReport',
                saveState:      function() { that.fetchPage(); },
                primaryIcon:    'refresh'
            });
            runReportFieldButton.setEnabled(that.getCriteria().hasCriteria());
            var criteriaChangedHookResult = that.getCriteria().hookCriteriaChanged(function() {
                runReportFieldButton.setEnabled(that.getCriteria().hasCriteria());
            }, this);

            // Sort pane
            var sortPane = new VRS.OptionPane({
                name:           'report_' + _Settings.name,
                titleKey:       'PaneSortReport',
                displayOrder:   displayOrder + 10,
                dispose:        function() {
                    if(criteriaChangedHookResult) that.getCriteria().unhook(criteriaChangedHookResult);
                }
            });
            result.push(sortPane);

            var sortColumnValues = getSortColumnValues();
            sortColumnValues.sort(function(lhs, rhs) {
                if(lhs.value === VRS.ReportSortColumn.None) return rhs.value === VRS.ReportSortColumn.None ? 0 : -1;
                if(rhs.value === VRS.ReportSortColumn.None) return 1;
                var lhsText = VRS.globalisation.getText(lhs.textKey) || '';
                var rhsText = VRS.globalisation.getText(rhs.textKey) || '';
                return lhsText.localeCompare(rhsText);
            });

            var self = this;
            $.each(_SortColumns, function(idx) {
                sortPane.addField(new VRS.OptionFieldComboBox({
                    name:           'sortBy' + idx.toString(),
                    labelKey:       idx === 0 ? 'SortBy' : 'ThenBy',
                    getValue:       function() { return self.getSortColumn(idx).field; },
                    setValue:       function(value) { var setEntry = self.getSortColumn(idx); setEntry.field = value; self.setSortColumn(idx, setEntry); },
                    saveState:      $.noop,
                    values:         sortColumnValues,
                    keepWithNext:   true
                }));

                sortPane.addField(new VRS.OptionFieldCheckBox({
                    name:           'ascending' + idx.toString(),
                    labelKey:       'Ascending',
                    getValue:       function() { return self.getSortColumn(idx).ascending; },
                    setValue:       function(value) { var setEntry = self.getSortColumn(idx); setEntry.ascending = value; self.setSortColumn(idx, setEntry); },
                    saveState:      $.noop
                }));
            });

            // Permanent link to report
            var runButtonPane = sortPane;
            if(VRS.globalOptions.reportShowPermanentLinkToReport) {
                var useRelativeDates = VRS.globalOptions.reportUseRelativeDatesInLink;
                var linkUrl = '';
                var hookResults = [];

                var linkLabel = new VRS.OptionFieldLinkLabel({
                    name:       'linkLabel',
                    labelKey:   function() { return linkUrl; },
                    getHref:    function() { return linkUrl; },
                    getTarget:  function() { return 'vrsReport'; }
                });

                var refreshLink = function() {
                    var newUrl = this._formPermanentLinkUrl(useRelativeDates);
                    if(newUrl !== linkUrl) {
                        linkUrl = newUrl;
                        linkLabel.raiseRefreshFieldContent();
                    }
                };

                var linkPane = new VRS.OptionPane({
                    name:           'permanentLink',
                    titleKey:       'PanePermanentLink',
                    displayOrder:   displayOrder + 20,
                    fields:         [
                        new VRS.OptionFieldCheckBox({
                            name:       'relativeDates',
                            labelKey:   'UseRelativeDates',
                            getValue:   function() { return useRelativeDates; },
                            setValue:   function(value) { useRelativeDates = value; },
                            saveState:  $.noop
                        }),
                        linkLabel
                    ],
                    pageParentCreated: function(/** VRS.OptionPageParent */ optionPageParent) {
                        var hookResult = optionPageParent.hookFieldChanged(refreshLink, self);
                        hookResults.push({ hookResult: hookResult, optionPageParent: optionPageParent });
                        refreshLink.call(self);
                    },
                    dispose: function(options) {
                        var hookResultIndex = VRS.arrayHelper.indexOfMatch(hookResults, function(item) {
                            return item.optionPageParent === options.optionPageParent;
                        });
                        if(hookResultIndex !== -1) {
                            var hookResult = hookResults[hookResultIndex];
                            hookResults.splice(hookResultIndex, 1);
                            hookResult.optionPageParent.unhook(hookResult.hookResult);
                        }
                    }
                });
                result.push(linkPane);
                runButtonPane = linkPane;
            }

            // Run button
            result.push(new VRS.OptionPane({
                name:           'runButtonPane',
                displayOrder:   displayOrder + 30,
                fields:         [ runReportFieldButton ]
            }));

            return result;
        };
        //endregion

        //region populateFromQueryString, _formPermanentLinkUrl
        /**
         * Populates the report criteria and settings from the query string used to load the report page.
         */
        this.populateFromQueryString = function()
        {
            var pageUrl = $.url();

            var length = _SortColumns.length;
            for(var i = 0;i < length;++i) {
                var sortFieldName = 'sort' + (i + 1);
                var sortAscendingName = 'sortAsc' + (i + 1);

                var sortField = pageUrl.param(sortFieldName);
                var sortAscending = pageUrl.param(sortAscendingName) || '1';
                if(sortField === 'none') sortField = VRS.ReportSortColumn.None;
                sortAscending = sortAscending !== '0';

                if(sortField && VRS.enumHelper.getEnumName(VRS.ReportSortColumn, sortField)) {
                    that.setSortColumn(i, { field: sortField, ascending: sortAscending });
                }
            }

            that.getCriteria().populateFromQueryString();
        };

        /**
         * Returns the permanent link to a report with the current criteria and sort settings or an empty string if there
         * are no criteria.
         * @param {boolean} useRelativeDates
         * @returns {string}
         * @private
         */
        this._formPermanentLinkUrl = function(useRelativeDates)
        {
            var result = '';

            var queryStringParams = that.getCriteria().createQueryString(useRelativeDates);
            var queryString = $.param(queryStringParams);
            if(queryString) {
                if(that.getCriteria().getFindAllPermutationsOfCallsign()) {
                    queryStringParams['callPerms'] = '1';
                }

                var length = _SortColumns.length;
                for(var i = 0;i < length;++i) {
                    var sortField = _SortColumns[i];
                    if(sortField.field === VRS.ReportSortColumn.None) continue;
                    var sortFieldName = 'sort' + (i + 1);
                    var sortAscendingName = 'sortAsc' + (i + 1);

                    queryStringParams[sortFieldName] = sortField.field;
                    if(!sortField.ascending) queryStringParams[sortAscendingName] = '0';
                }
                queryString = $.param(queryStringParams);

                var pageUrl = $.url();
                result = pageUrl.attr('base') + pageUrl.attr('path') + '?' + queryString;
            }

            return result;
        };
        //endregion

        //region fetchPage
        /**
         * Fetches a page of results for the report.
         * @param {Number=}     pageNumber      The 0-based page number to fetch. If not supplied then the first page is fetched.
         */
        this.fetchPage = function(pageNumber)
        {
            if(!pageNumber) pageNumber = 0;

            if(that.getCriteria().hasCriteria()) {
                var firstRow = _PageSize > 0 ? pageNumber * _PageSize : -1;
                var lastRow = _PageSize > 0 ? ((pageNumber + 1) * _PageSize) - 1 : -1;
                var parameters = {
                    rep:        'date',     // we no longer distinguish between aircraft and date reports
                    fromRow:    firstRow,
                    toRow:      lastRow
                };
                that.getCriteria().addToQueryParameters(parameters);

                var length = _SortColumns.length;
                for(var i = 0;i < length;++i) {
                    var sortColumn = _SortColumns[i];
                    if(sortColumn.field !== VRS.ReportSortColumn.None) {
                        var propertyName = 'sort' + (i + 1);
                        parameters[propertyName] = sortColumn.field;
                        parameters[propertyName + 'dir'] = sortColumn.ascending ? 'asc' : 'desc';
                    }
                }

                if(_Settings.showFetchUI) VRS.pageHelper.showModalWaitAnimation(true);

                $.ajax({
                    url:        VRS.globalOptions.reportUrl,
                    dataType:   'text',     // It's always text - it contains Microsoft formatted dates, they need munging before we can use them
                    data:       parameters,
                    success:    $.proxy(pageFetched, this),
                    error:      $.proxy(fetchFailed, this)
                });
            }
        };

        /**
         * Called with the result of the fetch from the server.
         * @param {string} rawData
         */
        function pageFetched(rawData)
        {
            if(_Settings.showFetchUI) VRS.pageHelper.showModalWaitAnimation(false);

            var json = VRS.jsonHelper.convertMicrosoftDates(rawData);
            _LastFetchResult = eval('(' + json + ')');

            fixupRoutesAndAirports();

            that.setSelectedFlight(null);
            _Dispatcher.raise(_Events.rowsFetched, [ that ]);

            if(_Settings.showFetchUI && _LastFetchResult.errorText) {
                VRS.pageHelper.showMessageBox(VRS.$$.ServerReportExceptionTitle, VRS.stringUtility.format(VRS.$$.ServerReportExceptionBody, _LastFetchResult.errorText));
            } else {
                var flights = that.getFlights();
                if(flights.length) that.setSelectedFlight(flights[0]);
            }
        }

        /**
         * Expands the flights and routes so that flights have a reference to the aircraft and route, and routes have
         * a reference to the airport.
         */
        function fixupRoutesAndAirports()
        {
            var length, i;

            if(_LastFetchResult.flights && _LastFetchResult.aircraftList && _LastFetchResult.routes) {
                var emptyRoute = {};
                length = _LastFetchResult.flights.length;
                for(i = 0;i < length;++i) {
                    var flight = _LastFetchResult.flights[i];
                    flight.route = flight.rtIdx > -1 ? _LastFetchResult.routes[flight.rtIdx] : emptyRoute;
                    flight.aircraft = _LastFetchResult.aircraftList[flight.acIdx];
                }
            }

            if(_LastFetchResult.routes && _LastFetchResult.airports) {
                var emptyAirport = {};
                length = _LastFetchResult.routes.length;
                for(i = 0;i < length;++i) {
                    var route = _LastFetchResult.routes[i];
                    route.from = route.fIdx > -1 ? _LastFetchResult.airports[route.fIdx] : emptyAirport;
                    route.to = route.tIdx > -1 ? _LastFetchResult.airports[route.tIdx] : emptyAirport;

                    route.via = [];
                    var sLength = route.sIdx.length;
                    for(var s = 0;s < sLength;++s) {
                        var sidx = route.sIdx[s];
                        if(sidx > -1) route.via.push(_LastFetchResult.airports[sidx]);
                    }
                }
            }

            if(_LastFetchResult.airports) {
                length = _LastFetchResult.airports.length;
                for(i = 0;i < length;++i) {
                    var airport = _LastFetchResult.airports[i];

                    var fullName = '';
                    if(airport.code && airport.name) fullName = airport.code + ' ' + airport.name;
                    else if(airport.code)            fullName = airport.code;
                    else                             fullName = airport.name || '';

                    airport.fullName = fullName;
                }
            }
        }

        /**
         * Called when the attempt to fetch a page has failed.
         * @param {jqXHR}   jqXHR
         * @param {string}  textStatus
         * @param {string}  errorThrown
         */
        function fetchFailed(jqXHR, textStatus, errorThrown)
        {
            if(_Settings.showFetchUI) VRS.pageHelper.showModalWaitAnimation(false);

            _Dispatcher.raise(_Events.fetchFailed, [
                that,
                jqXHR ? jqXHR.statusCode() : undefined,
                textStatus,
                errorThrown
            ]);

            if(_Settings.showFetchUI) {
                if(textStatus === 'timeout') VRS.pageHelper.showMessageBox(VRS.$$.ServerFetchFailedTitle, VRS.$$.ServerFetchTimedOut);
                else                         VRS.pageHelper.showMessageBox(VRS.$$.ServerFetchFailedTitle, VRS.stringUtility.format(VRS.$$.ServerFetchFailedBody, errorThrown, textStatus));
            }
        }
        //endregion

        //region getSortColumnValues
        /**
         * Returns an array of VRS.ValueText objects that provide descriptions for the full list of VRS.ReportSortColumn values.
         * @returns {Array.<VRS.ValueText>}
         */
        function getSortColumnValues()
        {
            return [
                new VRS.ValueText({ value: VRS.ReportSortColumn.None,         textKey: 'None' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Callsign,     textKey: 'Callsign' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Date,         textKey: 'Date' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Icao,         textKey: 'Icao' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Model,        textKey: 'Model' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.ModelIcao,    textKey: 'ModelIcao' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Operator,     textKey: 'Operator' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Registration, textKey: 'Registration' })
            ];
        }
        //endregion
    };

    //region static - convertFlightToVrsAircraft
    /**
     * Creates a VRS.Aircraft object from a JSON flight object.
     * @param {VRS_JSON_REPORT_FLIGHT}  flight
     * @param {bool}                    useFirstValues
     * @returns {VRS.Aircraft}
     * @private
     */
    VRS.Report.convertFlightToVrsAircraft = function(flight, useFirstValues)
    {
        /** @type {VRS.Aircraft} */ var result = null;

        if(flight && flight.aircraft) {
            result = new VRS.Aircraft();
            result.altitude.setValue(useFirstValues ? flight.fAlt : flight.lAlt);
            result.callsign.setValue(flight.call);
            result.countEngines.setValue(flight.aircraft.engines);
            result.country.setValue(flight.aircraft.country);
            result.engineType.setValue(flight.aircraft.engineType);
            result.hasPicture.setValue(flight.aircraft.hasPic);
            result.heading.setValue(useFirstValues ? flight.fTrk : flight.lTrk);
            result.icao.setValue(flight.aircraft.icao);
            result.id = flight.row;
            result.isEmergency.setValue(flight.hEmg);
            result.isMilitary.setValue(flight.aircraft.military);
            result.isOnGround.setValue(useFirstValues ? flight.fOnGnd : flight.lOnGnd);
            result.latitude.setValue(useFirstValues ? flight.fLat : flight.lLat);
            result.longitude.setValue(useFirstValues ? flight.fLng : flight.lLng);
            result.model.setValue(flight.aircraft.type);
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
            if(flight.route.via) {
                var via = [];
                $.each(flight.route.via, function(/** Number */idx, /** VRS_JSON_REPORT_AIRPORT */ viaAirport) {
                    via.push(viaAirport.fullName);
                });
                result.via.setValue(via);
            }
        }

        return result;
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
