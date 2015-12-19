var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftAutoSelectEnabled = VRS.globalOptions.aircraftAutoSelectEnabled !== undefined ? VRS.globalOptions.aircraftAutoSelectEnabled : true;
    VRS.globalOptions.aircraftAutoSelectClosest = VRS.globalOptions.aircraftAutoSelectClosest !== undefined ? VRS.globalOptions.aircraftAutoSelectClosest : true;
    VRS.globalOptions.aircraftAutoSelectOffRadarAction = VRS.globalOptions.aircraftAutoSelectOffRadarAction || VRS.OffRadarAction.EnableAutoSelect;
    VRS.globalOptions.aircraftAutoSelectFilters = VRS.globalOptions.aircraftAutoSelectFilters || [];
    VRS.globalOptions.aircraftAutoSelectFiltersLimit = VRS.globalOptions.aircraftAutoSelectFiltersLimit !== undefined ? VRS.globalOptions.aircraftAutoSelectFiltersLimit : 2;
    var AircraftAutoSelect = (function () {
        function AircraftAutoSelect(aircraftList, name) {
            var _this = this;
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.AircraftAutoSelect'
            });
            this._Events = {
                enabledChanged: 'enabledChanged'
            };
            this.dispose = function () {
                if (_this._AircraftListUpdatedHook)
                    _this._AircraftList.unhook(_this._AircraftListUpdatedHook);
                if (_this._SelectedAircraftChangedHook)
                    _this._AircraftList.unhook(_this._SelectedAircraftChangedHook);
                _this._AircraftListUpdatedHook = _this._SelectedAircraftChangedHook = null;
            };
            this.getName = function () {
                return _this._Name;
            };
            this.getEnabled = function () {
                return _this._Enabled;
            };
            this.setEnabled = function (value) {
                if (_this._Enabled !== value) {
                    _this._Enabled = value;
                    _this._Dispatcher.raise(_this._Events.enabledChanged);
                    if (_this._Enabled && _this._AircraftList) {
                        var closestAircraft = _this.closestAircraft(_this._AircraftList);
                        if (closestAircraft && closestAircraft !== _this._AircraftList.getSelectedAircraft()) {
                            _this._AircraftList.setSelectedAircraft(closestAircraft, false);
                        }
                    }
                }
            };
            this.getSelectClosest = function () {
                return _this._SelectClosest;
            };
            this.setSelectClosest = function (value) {
                _this._SelectClosest = value;
            };
            this.getOffRadarAction = function () {
                return _this._OffRadarAction;
            };
            this.setOffRadarAction = function (value) {
                _this._OffRadarAction = value;
            };
            this.getFilter = function (index) {
                return _this._Filters[index].clone();
            };
            this.setFilter = function (index, aircraftFilter) {
                if (_this._Filters.length < VRS.globalOptions.aircraftListFiltersLimit) {
                    var existing = index < _this._Filters.length ? _this._Filters[index] : null;
                    if (existing && !existing.equals(aircraftFilter))
                        _this._Filters[index] = aircraftFilter;
                }
            };
            this.hookEnabledChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.enabledChanged, callback, forceThis);
            };
            this.unhook = function (hookResult) {
                _this._Dispatcher.unhook(hookResult);
            };
            this.saveState = function () {
                VRS.configStorage.save(_this.persistenceKey(), _this.createSettings());
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.load(_this.persistenceKey(), {});
                var result = $.extend(_this.createSettings(), savedSettings);
                result.filters = VRS.aircraftFilterHelper.buildValidFiltersList(result.filters, VRS.globalOptions.aircraftAutoSelectFiltersLimit);
                return result;
            };
            this.applyState = function (settings) {
                _this.setEnabled(settings.enabled);
                _this.setSelectClosest(settings.selectClosest);
                _this.setOffRadarAction(settings.offRadarAction);
                _this._Filters = [];
                var self = _this;
                $.each(settings.filters, function (idx, serialisedFilter) {
                    var aircraftFilter = VRS.aircraftFilterHelper.createFilter(serialisedFilter.property);
                    aircraftFilter.applySerialisedObject(serialisedFilter);
                    self._Filters.push(aircraftFilter);
                });
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.persistenceKey = function () {
                return 'vrsAircraftAutoSelect-' + _this._Name;
            };
            this.createSettings = function () {
                var filters = VRS.aircraftFilterHelper.serialiseFiltersList(_this._Filters);
                return {
                    enabled: _this._Enabled,
                    selectClosest: _this._SelectClosest,
                    offRadarAction: _this._OffRadarAction,
                    filters: filters
                };
            };
            this.createOptionPane = function (displayOrder) {
                var result = [];
                var pane = new VRS.OptionPane({
                    name: 'vrsAircraftAutoSelectPane1',
                    titleKey: 'PaneAutoSelect',
                    displayOrder: displayOrder
                });
                result.push(pane);
                pane.addField(new VRS.OptionFieldCheckBox({
                    name: 'enable',
                    labelKey: 'AutoSelectAircraft',
                    getValue: _this.getEnabled,
                    setValue: _this.setEnabled,
                    saveState: _this.saveState,
                    hookChanged: _this.hookEnabledChanged,
                    unhookChanged: _this.unhook
                }));
                pane.addField(new VRS.OptionFieldRadioButton({
                    name: 'closest',
                    labelKey: 'Select',
                    getValue: _this.getSelectClosest,
                    setValue: _this.setSelectClosest,
                    saveState: _this.saveState,
                    values: [
                        new VRS.ValueText({ value: true, textKey: 'ClosestToCurrentLocation' }),
                        new VRS.ValueText({ value: false, textKey: 'FurthestFromCurrentLocation' })
                    ]
                }));
                if (VRS.globalOptions.aircraftListFiltersLimit !== 0) {
                    var panesTypeSettings = VRS.aircraftFilterHelper.addConfigureFiltersListToPane({
                        pane: pane,
                        filters: _this._Filters,
                        saveState: $.proxy(_this.saveState, _this),
                        maxFilters: VRS.globalOptions.aircraftAutoSelectFiltersLimit,
                        allowableProperties: [
                            VRS.AircraftFilterProperty.Altitude,
                            VRS.AircraftFilterProperty.Distance
                        ],
                        addFilter: function (newComparer, paneField) {
                            var aircraftFilter = _this.addFilter(newComparer);
                            paneField.addPane(aircraftFilter.createOptionPane(function () { return _this.saveState; }));
                            _this.saveState();
                        },
                        addFilterButtonLabel: 'AddCondition'
                    });
                    panesTypeSettings.hookPaneRemoved(_this.filterPaneRemoved, _this);
                }
                pane = new VRS.OptionPane({
                    name: 'vrsAircraftAutoSelectPane2',
                    displayOrder: displayOrder + 1
                });
                result.push(pane);
                pane.addField(new VRS.OptionFieldLabel({
                    name: 'offRadarActionLabel',
                    labelKey: 'OffRadarAction'
                }));
                pane.addField(new VRS.OptionFieldRadioButton({
                    name: 'offRadarAction',
                    getValue: _this.getOffRadarAction,
                    setValue: _this.setOffRadarAction,
                    saveState: _this.saveState,
                    values: [
                        new VRS.ValueText({ value: VRS.OffRadarAction.WaitForReturn, textKey: 'OffRadarActionWait' }),
                        new VRS.ValueText({ value: VRS.OffRadarAction.EnableAutoSelect, textKey: 'OffRadarActionEnableAutoSelect' }),
                        new VRS.ValueText({ value: VRS.OffRadarAction.Nothing, textKey: 'OffRadarActionNothing' })
                    ]
                }));
                return result;
            };
            this.addFilter = function (filterOrFilterProperty) {
                var filter = filterOrFilterProperty;
                if (!(filter instanceof VRS.AircraftFilter)) {
                    filter = VRS.aircraftFilterHelper.createFilter(filterOrFilterProperty);
                }
                _this._Filters.push(filter);
                return filter;
            };
            this.filterPaneRemoved = function (pane, index) {
                _this.removeFilterAt(index);
                _this.saveState();
            };
            this.removeFilterAt = function (index) {
                _this._Filters.splice(index, 1);
            };
            this.closestAircraft = function (aircraftList) {
                var result = null;
                if (_this.getEnabled()) {
                    var useClosest = _this.getSelectClosest();
                    var useFilters = _this._Filters.length > 0;
                    var selectedDistance = useClosest ? Number.MAX_VALUE : Number.MIN_VALUE;
                    var self = _this;
                    aircraftList.foreachAircraft(function (aircraft) {
                        if (aircraft.distanceFromHereKm.val !== undefined) {
                            var isCandidate = useClosest ? aircraft.distanceFromHereKm.val < selectedDistance : aircraft.distanceFromHereKm.val > selectedDistance;
                            if (isCandidate) {
                                if (useFilters && !VRS.aircraftFilterHelper.aircraftPasses(aircraft, self._Filters, {}))
                                    isCandidate = false;
                                if (isCandidate) {
                                    result = aircraft;
                                    selectedDistance = aircraft.distanceFromHereKm.val;
                                }
                            }
                        }
                    });
                }
                return result;
            };
            this.aircraftListUpdated = function (newAircraft, offRadar) {
                var self = _this;
                var selectedAircraft = _this._AircraftList.getSelectedAircraft();
                var autoSelectAircraft = _this.closestAircraft(_this._AircraftList);
                var useAutoSelectedAircraft = function () {
                    if (autoSelectAircraft !== selectedAircraft) {
                        self._AircraftList.setSelectedAircraft(autoSelectAircraft, false);
                        selectedAircraft = autoSelectAircraft;
                    }
                };
                if (autoSelectAircraft) {
                    useAutoSelectedAircraft();
                }
                else if (selectedAircraft) {
                    var isOffRadar = offRadar.findAircraftById(selectedAircraft.id);
                    if (isOffRadar) {
                        switch (_this.getOffRadarAction()) {
                            case VRS.OffRadarAction.Nothing:
                                break;
                            case VRS.OffRadarAction.EnableAutoSelect:
                                if (!_this.getEnabled()) {
                                    _this.setEnabled(true);
                                    autoSelectAircraft = _this.closestAircraft(_this._AircraftList);
                                    if (autoSelectAircraft)
                                        useAutoSelectedAircraft();
                                }
                                break;
                            case VRS.OffRadarAction.WaitForReturn:
                                _this._AircraftList.setSelectedAircraft(undefined, false);
                                break;
                        }
                    }
                }
                if (!selectedAircraft && _this._LastAircraftSelected) {
                    var reselectAircraft = newAircraft.findAircraftById(_this._LastAircraftSelected.id);
                    if (reselectAircraft) {
                        _this._AircraftList.setSelectedAircraft(reselectAircraft, false);
                    }
                }
            };
            this.selectedAircraftChanged = function () {
                var selectedAircraft = _this._AircraftList.getSelectedAircraft();
                if (selectedAircraft)
                    _this._LastAircraftSelected = selectedAircraft;
                if (_this._AircraftList.getWasAircraftSelectedByUser()) {
                    _this.setEnabled(false);
                }
            };
            this._AircraftList = aircraftList;
            this._Name = name || 'default';
            this._Enabled = VRS.globalOptions.aircraftAutoSelectEnabled;
            this._SelectClosest = VRS.globalOptions.aircraftAutoSelectClosest;
            this._OffRadarAction = VRS.globalOptions.aircraftAutoSelectOffRadarAction;
            this._Filters = VRS.globalOptions.aircraftAutoSelectFilters;
            this._AircraftListUpdatedHook = aircraftList.hookUpdated(this.aircraftListUpdated, this);
            this._SelectedAircraftChangedHook = aircraftList.hookSelectedAircraftChanged(this.selectedAircraftChanged, this);
        }
        return AircraftAutoSelect;
    })();
    VRS.AircraftAutoSelect = AircraftAutoSelect;
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraftAutoSelect.js.map