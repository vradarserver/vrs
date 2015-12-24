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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.reportDefaultStepSize = VRS.globalOptions.reportDefaultStepSize || 25;            // The default step to show on the page size controls.
    VRS.globalOptions.reportDefaultPageSize = VRS.globalOptions.reportDefaultPageSize || 50;            // The default page size to show. Use -1 if you want to default to showing all rows.
    VRS.globalOptions.reportUrl = VRS.globalOptions.reportUrl || 'ReportRows.json';                     // The URL to fetch when retrieving report rows.
    VRS.globalOptions.reportDefaultSortColumns = VRS.globalOptions.reportDefaultSortColumns || [        // The default sort order for reports. Note that the server will not accept any more than two sort columns.
        { field: VRS.ReportSortColumn.Date, ascending: true },
        { field: VRS.ReportSortColumn.None, ascending: false }
    ];
    VRS.globalOptions.reportShowPermanentLinkToReport = VRS.globalOptions.reportShowPermanentLinkToReport !== undefined ? VRS.globalOptions.reportShowPermanentLinkToReport : true; // True to show the permanent link to the report in the criteria UI, false to suppress it.
    VRS.globalOptions.reportUseRelativeDatesInLink = VRS.globalOptions.reportUseRelativeDatesInLink !== undefined ? VRS.globalOptions.reportUseRelativeDatesInLink : true;          // True to default the 'use relative dates' checkbox in the permanent link to true.

    /**
     * The settings to use when creating a new instance of Report.
     */
    export interface Report_Settings
    {
        /**
         * The name to assign to the report. Defaults to 'default'.
         */
        name?: string;

        /**
         * True if the state should be auto-saved when properties are changed. Defaults to false.
         */
        autoSaveState?: boolean;

        /**
         * True if the report should indicate visually that it is fetching rows and show error messages to the user on failure to fetch rows. Defaults to true.
         */
        showFetchUI?: boolean;

        /**
         * The unit display preferences to apply to report values.
         */
        unitDisplayPreferences: UnitDisplayPreferences;
    }

    /**
     * Describes a column in the sort order for a report.
     */
    export interface Report_SortColumn
    {
        field: ReportSortColumnEnum;

        ascending: boolean;
    }

    /**
     * The values that are persisted by Report objects between sessions.
     */
    export interface Report_SaveState
    {
        pageSize: number;
    }

    export class Report implements ISelfPersist<Report_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.Report'
        });
        private _Events = {
            buildingRequest:        'buildingRequest',
            pageSizeChanged:        'pageSizeChanged',
            rowsFetched:            'rowsFetched',
            failedNoCriteria:       'failedNoCriteria',
            fetchFailed:            'fetchFailed',
            selectedFlightChanged:  'selectedFlightChanged'
        };
        private _Settings: Report_Settings;

        private _LastFetchResult: IReport = null;       // The result of the last successful request for a page of results.
        private  _Criteria: ReportCriteria;             // The report criteria
        private _SortColumns: Report_SortColumn[] = VRS.globalOptions.reportDefaultSortColumns.slice();
        private _SelectedFlight: IReportFlight = null;
        private _PageSize: number = VRS.globalOptions.reportDefaultPageSize;

        constructor(settings: Report_Settings)
        {
            this._Settings = $.extend({
                name:           'default',
                autoSaveState:  false,
                showFetchUI:    true
            }, settings);

            this._Criteria = new VRS.ReportCriteria({
                name: this._Settings.name,
                unitDisplayPreferences: this._Settings.unitDisplayPreferences
            });
        }

        /**
         * Gets the name used to store and load settings.
         */
        getName() : string
        {
            return this._Settings.name;
        }

        /**
         * Gets the criteria for the report.
         */
        getCriteria() : ReportCriteria
        {
            return this._Criteria;
        }

        /***
         * Gets the number of sort columns available to the user.
         */
        getSortColumnsLength() : number
        {
            return this._SortColumns.length;
        }

        /**
         * Gets the sort column at the index specified.
         */
        getSortColumn(index: number) : Report_SortColumn
        {
            var existing = this._SortColumns[index];
            return {
                field: existing.field,
                ascending: existing.ascending
            };
        }
        setSortColumn(index: number, value: Report_SortColumn)
        {
            var existing = this._SortColumns[index];
            if(existing.field !== value.field || existing.ascending !== value.ascending) {
                this._SortColumns[index] = value;
            }
        }

        /**
         * Returns the column on which the last set of results were sorted.
         */
        getGroupSortColumn() : ReportSortColumnEnum
        {
            return this._LastFetchResult && !this._LastFetchResult.errorText 
                ? this._LastFetchResult.groupBy
                : VRS.ReportSortColumn.None;
        }

        /**
         * Returns the flights last fetched by the report.
         */
        getFlights() : IReportFlight[]
        {
            return this._LastFetchResult && !this._LastFetchResult.errorText && this._LastFetchResult.flights
                ? this._LastFetchResult.flights
                : [];
        }

        /**
         * Returns the error text from the last fetch or null if the last fetch did not encounter an error server-side.
         */
        getLastError() : string
        {
            return this._LastFetchResult && this._LastFetchResult.errorText ? this._LastFetchResult.errorText : null;
        }

        getSelectedFlight() : IReportFlight
        {
            return this._SelectedFlight;
        }
        setSelectedFlight(value: IReportFlight)
        {
            if(value !== this._SelectedFlight) {
                this._SelectedFlight = value;
                this._Dispatcher.raise(this._Events.selectedFlightChanged, [ this ]);
            }
        }

        getPageSize() : number
        {
            return this._PageSize;
        }
        setPageSize(value: number)
        {
            if(this._PageSize !== value) {
                this._PageSize = value;
                if(this._Settings.autoSaveState && value > 0) {
                    this.saveState();
                }
                this._Dispatcher.raise(this._Events.pageSizeChanged, [ this ]);
            }
        }

        /**
         * Returns the number of rows that the server says are in the report. Note that this can change as more data is
         * collected by the server while the user is viewing the report. This is 0 if no rows are available.
         */
        getCountRowsAvailable() : number
        {
            return this._LastFetchResult && !this._LastFetchResult.errorText ? this._LastFetchResult.countRows : 0;
        }

        /**
         * Returns true if the report is broken up into pages, false if it is not.
         */
        isReportPaged() : boolean
        {
            return this._PageSize > 0;
        }

        /**
         * Returns true if the report has data to display, false if it does not.
         */
        hasData() : boolean
        {
            return !!(this._LastFetchResult && this._LastFetchResult.flights.length);
        }

        /**
         * Hooks an event that is raised when a report run is requested but it immediately fails because no criteria
         * has been supplied.
         */
        hookFailedNoCriteria(callback: (report: Report) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.failedNoCriteria, callback, forceThis);
        }

        /**
         * Hooks an event that is raised before a request for a report page is made.
         */
        hookBuildingRequest(callback: (report: Report, parameters: any, headers: any) => void, forceThis?: boolean) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.buildingRequest, callback, forceThis);
        }

        /**
         * Hooks an event that is raised after the report has failed to fetch a new batch of rows to display to the user.
         */
        hookFetchFailed(callback: (report: Report, statusCode: number, textStatus: string, error: string) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.fetchFailed, callback, forceThis);
        }

        /**
         * Hooks an event that is raised after the page size has been changed.
         */
        hookPageSizeChanged(callback: (report: Report) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.pageSizeChanged, callback, forceThis);
        }

        /**
         * Hooks an event that is raised after the report has fetched a new batch of rows to display to the user.
         */
        hookRowsFetched(callback: (report: Report) => void, forceThis?: Object) : IEventHandle
        {
            return this._Dispatcher.hook(this._Events.rowsFetched, callback, forceThis);
        }

        /**
         * Hooks an event that is raised after the selected flight has changed.
         */
        hookSelectedFlightCHanged(callback: (report: Report) => void, forceThis?: Object)
        {
            return this._Dispatcher.hook(this._Events.selectedFlightChanged, callback, forceThis);
        }

        /**
         * Unhooks an event that was hooked on the object.
         */
        unhook(hookResult: IEventHandle)
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Saves the current state of the object.
         */
        saveState()
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Loads the previously saved state of the object or the current state if it's never been saved.
         */
        loadState() : Report_SaveState
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        }

        /**
         * Applies a previously saved state to the object.
         */
        applyState(settings: Report_SaveState)
        {
            this.setPageSize(settings.pageSize);
        }

        /**
         * Loads and then applies a previousy saved state to the object.
         */
        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the state will be saved.
         */
        private persistenceKey() : string
        {
            return 'vrsReport-' + this.getName();
        }

        /**
         * Creates the saved state object.
         */
        private createSettings() : Report_SaveState
        {
            return {
                pageSize: this.getPageSize()
            };
        }

        /**
         * Returns an option pane that can be used to control the report generation.
         */
        createOptionPane(displayOrder: number) : OptionPane[]
        {
            var result: OptionPane[] = [];

            // Criteria pane
            result.push(this._Criteria.createOptionPane(displayOrder));

            // Run button type settings - declared early as we need the reference for event handlers
            var runReportFieldButton = new VRS.OptionFieldButton({
                name:           'runButton',
                labelKey:       'RunReport',
                saveState:      () => { this.fetchPage(); },
                primaryIcon:    'refresh'
            });
            runReportFieldButton.setEnabled(this.getCriteria().hasCriteria());
            var criteriaChangedHookResult = this.getCriteria().hookCriteriaChanged(function() {
                runReportFieldButton.setEnabled(this.getCriteria().hasCriteria());
            }, this);

            // Sort pane
            var sortPane = new VRS.OptionPane({
                name:           'report_' + this._Settings.name,
                titleKey:       'PaneSortReport',
                displayOrder:   displayOrder + 10,
                dispose:        () => {
                    if(criteriaChangedHookResult) {
                        this.getCriteria().unhook(criteriaChangedHookResult);
                    }
                }
            });
            result.push(sortPane);

            var sortColumnValues = this.getSortColumnValues();
            sortColumnValues.sort(function(lhs, rhs) {
                if(lhs.getValue() === VRS.ReportSortColumn.None) return rhs.getValue() === VRS.ReportSortColumn.None ? 0 : -1;
                if(rhs.getValue() === VRS.ReportSortColumn.None) return 1;
                var lhsText = lhs.getText() || '';
                var rhsText = rhs.getText() || '';
                return lhsText.localeCompare(rhsText);
            });

            $.each(this._SortColumns, (idx) => {
                sortPane.addField(new VRS.OptionFieldComboBox({
                    name:           'sortBy' + idx.toString(),
                    labelKey:       idx === 0 ? 'SortBy' : 'ThenBy',
                    getValue:       () => this.getSortColumn(idx).field,
                    setValue:       (value) => {
                        var setEntry = this.getSortColumn(idx);
                        setEntry.field = value;
                        this.setSortColumn(idx, setEntry);
                    },
                    saveState:      $.noop,
                    values:         sortColumnValues,
                    keepWithNext:   true
                }));

                sortPane.addField(new VRS.OptionFieldCheckBox({
                    name:           'ascending' + idx.toString(),
                    labelKey:       'Ascending',
                    getValue:       () => this.getSortColumn(idx).ascending,
                    setValue:       (value) => {
                        var setEntry = this.getSortColumn(idx);
                        setEntry.ascending = value;
                        this.setSortColumn(idx, setEntry);
                    },
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

                var refreshLink = () => {
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
                            getValue:   () => useRelativeDates,
                            setValue:   (value) => useRelativeDates = value,
                            saveState:  $.noop
                        }),
                        linkLabel
                    ],
                    pageParentCreated: (optionPageParent) => {
                        var hookResult = optionPageParent.hookFieldChanged(refreshLink, self);
                        hookResults.push({
                            hookResult: hookResult,
                            optionPageParent: optionPageParent
                        });
                        refreshLink.call(this);
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
        }

        /**
         * Returns an array of VRS.ValueText objects that provide descriptions for the full list of VRS.ReportSortColumn values.
         */
        private getSortColumnValues() : ValueText[]
        {
            return [
                new VRS.ValueText({ value: VRS.ReportSortColumn.None,           textKey: 'None' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Callsign,       textKey: 'Callsign' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Date,           textKey: 'Date' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.FirstAltitude,  textKey: 'FirstAltitude' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Icao,           textKey: 'Icao' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.LastAltitude,   textKey: 'LastAltitude' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Model,          textKey: 'Model' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.ModelIcao,      textKey: 'ModelIcao' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Operator,       textKey: 'Operator' }),
                new VRS.ValueText({ value: VRS.ReportSortColumn.Registration,   textKey: 'Registration' })
            ];
        }

        /**
         * Populates the report criteria and settings from the query string used to load the report page.
         */
        populateFromQueryString()
        {
            var pageUrl = $.url();

            var length = this._SortColumns.length;
            for(var i = 0;i < length;++i) {
                var sortFieldName = 'sort' + (i + 1);
                var sortAscendingName = 'sortAsc' + (i + 1);

                var sortField = pageUrl.param(sortFieldName);
                var sortAscending = pageUrl.param(sortAscendingName) || '1';
                if(sortField === 'none') sortField = VRS.ReportSortColumn.None;

                if(sortField && VRS.enumHelper.getEnumName(VRS.ReportSortColumn, sortField)) {
                    this.setSortColumn(i, { field: sortField, ascending: sortAscending !== '0' });
                }
            }

            this.getCriteria().populateFromQueryString();
        }

        /**
         * Returns the permanent link to a report with the current criteria and sort settings or an empty string if there
         * are no criteria.
         */
        _formPermanentLinkUrl(useRelativeDates: boolean) : string
        {
            var result = '';

            var queryStringParams = this.getCriteria().createQueryString(useRelativeDates);
            var queryString = $.param(queryStringParams);
            if(queryString) {
                if(this.getCriteria().getFindAllPermutationsOfCallsign()) {
                    queryStringParams['callPerms'] = '1';
                }

                var length = this._SortColumns.length;
                for(var i = 0;i < length;++i) {
                    var sortField = this._SortColumns[i];
                    if(sortField.field === VRS.ReportSortColumn.None) {
                        continue;
                    }

                    var sortFieldName = 'sort' + (i + 1);
                    var sortAscendingName = 'sortAsc' + (i + 1);

                    queryStringParams[sortFieldName] = sortField.field;
                    if(!sortField.ascending) {
                        queryStringParams[sortAscendingName] = '0';
                    }
                }
                queryString = $.param(queryStringParams);

                var pageUrl = $.url();
                result = pageUrl.attr('base') + pageUrl.attr('path') + '?' + queryString;
            }

            return result;
        }

        /**
         * Fetches a page of results for the report.
         */
        fetchPage(pageNumber: number = 0)
        {
            if(!this.getCriteria().hasCriteria()) {
                this._Dispatcher.raise(this._Events.failedNoCriteria, [ this ]);
            } else {
                var firstRow = this._PageSize > 0 ? pageNumber * this._PageSize : -1;
                var lastRow = this._PageSize > 0 ? ((pageNumber + 1) * this._PageSize) - 1 : -1;
                var parameters = {
                    rep:        'date',     // we no longer distinguish between aircraft and date reports
                    fromRow:    firstRow,
                    toRow:      lastRow
                };
                this.getCriteria().addToQueryParameters(parameters);

                var length = this._SortColumns.length;
                for(var i = 0;i < length;++i) {
                    var sortColumn = this._SortColumns[i];
                    if(sortColumn.field !== VRS.ReportSortColumn.None) {
                        var propertyName = 'sort' + (i + 1);
                        parameters[propertyName] = sortColumn.field;
                        parameters[propertyName + 'dir'] = sortColumn.ascending ? 'asc' : 'desc';
                    }
                }

                if(this._Settings.showFetchUI) {
                    VRS.pageHelper.showModalWaitAnimation(true);
                }

                $.ajax({
                    url:        VRS.globalOptions.reportUrl,
                    dataType:   'text',     // It's always text - it contains Microsoft formatted dates, they need munging before we can use them
                    data:       parameters,
                    success:    $.proxy(this.pageFetched, this),
                    error:      $.proxy(this.fetchFailed, this),
                    cache:      false
                });
            }
        }

        /**
         * Called with the result of the fetch from the server.
         */
        private pageFetched(rawData: string)
        {
            if(this._Settings.showFetchUI) {
                VRS.pageHelper.showModalWaitAnimation(false);
            }

            var json = VRS.jsonHelper.convertMicrosoftDates(rawData);
            this._LastFetchResult = eval('(' + json + ')');

            this.fixupRoutesAndAirports();

            this.setSelectedFlight(null);
            this._Dispatcher.raise(this._Events.rowsFetched, [ this ]);

            if(this._Settings.showFetchUI && this._LastFetchResult.errorText) {
                VRS.pageHelper.showMessageBox(VRS.$$.ServerReportExceptionTitle, VRS.stringUtility.format(VRS.$$.ServerReportExceptionBody, this._LastFetchResult.errorText));
            } else {
                var flights = this.getFlights();
                if(flights.length) {
                    this.setSelectedFlight(flights[0]);
                }
            }
        }

        /**
         * Expands the flights and routes so that flights have a reference to the aircraft and route, and routes have
         * a reference to the airport.
         */
        private fixupRoutesAndAirports()
        {
            if(this._LastFetchResult.flights && this._LastFetchResult.aircraftList && this._LastFetchResult.routes) {
                var emptyRoute = {};
                let length = this._LastFetchResult.flights.length;
                for(let i = 0;i < length;++i) {
                    var flight = this._LastFetchResult.flights[i];
                    flight.route = flight.rtIdx > -1 ? this._LastFetchResult.routes[flight.rtIdx] : emptyRoute;
                    flight.aircraft = this._LastFetchResult.aircraftList[flight.acIdx];
                }
            }

            if(this._LastFetchResult.routes && this._LastFetchResult.airports) {
                var emptyAirport = {};
                let length = this._LastFetchResult.routes.length;
                for(let i = 0;i < length;++i) {
                    var route = this._LastFetchResult.routes[i];
                    route.from = route.fIdx > -1 ? this._LastFetchResult.airports[route.fIdx] : emptyAirport;
                    route.to = route.tIdx > -1 ? this._LastFetchResult.airports[route.tIdx] : emptyAirport;

                    route.via = [];
                    var sLength = route.sIdx.length;
                    for(var s = 0;s < sLength;++s) {
                        var sidx = route.sIdx[s];
                        if(sidx > -1) {
                            route.via.push(this._LastFetchResult.airports[sidx]);
                        }
                    }
                }
            }

            if(this._LastFetchResult.airports) {
                let length = this._LastFetchResult.airports.length;
                for(let i = 0;i < length;++i) {
                    var airport = this._LastFetchResult.airports[i];

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
         */
        private fetchFailed(jqXHR: JQueryXHR, textStatus: string, errorThrown: string)
        {
            if(this._Settings.showFetchUI) {
                VRS.pageHelper.showModalWaitAnimation(false);
            }

            this._Dispatcher.raise(this._Events.fetchFailed, [
                this,
                jqXHR && $.isFunction((<any>jqXHR).statusCode) ? (<any>jqXHR).statusCode() : undefined,
                textStatus,
                errorThrown
            ]);

            if(this._Settings.showFetchUI) {
                if(textStatus === 'timeout') {
                    VRS.pageHelper.showMessageBox(VRS.$$.ServerFetchFailedTitle, VRS.$$.ServerFetchTimedOut);
                } else {
                    VRS.pageHelper.showMessageBox(VRS.$$.ServerFetchFailedTitle, VRS.stringUtility.format(VRS.$$.ServerFetchFailedBody, errorThrown, textStatus));
                }
            }
        }

        /**
         * Creates a VRS.Aircraft object from a JSON flight object.
         */
        static convertFlightToVrsAircraft(flight: IReportFlight, useFirstValues: boolean) : Aircraft
        {
            var result: Aircraft = null;

            if(flight && flight.aircraft) {
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
                if(flight.route.via) {
                    var via = [];
                    $.each(flight.route.via, function(idx, viaAirport) {
                        via.push(viaAirport.fullName);
                    });
                    result.via.setValue(via);
                }
            }

            return result;
        };
    }
}
 