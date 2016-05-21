var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListDefaultFiltersEnabled = VRS.globalOptions.aircraftListDefaultFiltersEnabled !== undefined ? VRS.globalOptions.aircraftListDefaultFiltersEnabled : false;
    VRS.globalOptions.aircraftListFilters = VRS.globalOptions.aircraftListFilters || [];
    VRS.globalOptions.aircraftListFiltersLimit = VRS.globalOptions.aircraftListFiltersLimit !== undefined ? VRS.globalOptions.aircraftListFiltersLimit : 12;
    var AircraftListFilter = (function () {
        function AircraftListFilter(settings) {
            var _this = this;
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.AircraftListFilter'
            });
            this._Events = {
                filterChanged: 'filterChanged',
                enabledChanged: 'enabledChanged'
            };
            this._Filters = VRS.globalOptions.aircraftListFilters;
            this.getName = function () {
                return _this._Settings.name;
            };
            this.getEnabled = function () {
                return _this._Enabled;
            };
            this.setEnabled = function (value) {
                if (_this._Enabled !== value) {
                    _this._Enabled = value;
                    _this._Dispatcher.raise(_this._Events.enabledChanged);
                }
            };
            this.hookFilterChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.filterChanged, callback, forceThis);
            };
            this.hookEnabledChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.enabledChanged, callback, forceThis);
            };
            this.unhook = function (hookResult) {
                _this._Dispatcher.unhook(hookResult);
            };
            this.dispose = function () {
                if (_this._AircraftList_FetchingListHookResult) {
                    _this._Settings.aircraftList.unhook(_this._AircraftList_FetchingListHookResult);
                    _this._AircraftList_FetchingListHookResult = null;
                }
                _this._Settings.aircraftList = null;
            };
            this.saveState = function () {
                VRS.configStorage.save(_this.persistenceKey(), _this.createSettings());
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.load(_this.persistenceKey(), {});
                var result = $.extend(_this.createSettings(), savedSettings);
                result.filters = VRS.aircraftFilterHelper.buildValidFiltersList(result.filters, VRS.globalOptions.aircraftListFiltersLimit);
                return result;
            };
            this.applyState = function (settings) {
                _this.setEnabled(settings.enabled);
                _this._Filters = [];
                $.each(settings.filters, function (idx, serialisedFilter) {
                    var aircraftFilter = VRS.aircraftFilterHelper.createFilter(serialisedFilter.property);
                    aircraftFilter.applySerialisedObject(serialisedFilter);
                    _this._Filters.push(aircraftFilter);
                });
                _this._Dispatcher.raise(_this._Events.filterChanged);
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.createOptionPane = function (displayOrder) {
                var pane = new VRS.OptionPane({
                    name: 'vrsAircraftListFilterPane',
                    titleKey: 'Filters',
                    displayOrder: displayOrder
                });
                if (VRS.globalOptions.aircraftListFiltersLimit !== 0) {
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'enable',
                        labelKey: 'EnableFilters',
                        getValue: _this.getEnabled,
                        setValue: _this.setEnabled,
                        saveState: _this.saveState
                    }));
                    var panesTypeSettings = VRS.aircraftFilterHelper.addConfigureFiltersListToPane({
                        pane: pane,
                        filters: _this._Filters,
                        saveState: function () { return _this.saveState(); },
                        maxFilters: VRS.globalOptions.aircraftListFiltersLimit,
                        allowableProperties: VRS.enumHelper.getEnumValues(VRS.AircraftFilterProperty),
                        addFilter: function (newFilter, paneField) {
                            var filter = _this.addFilter(newFilter);
                            if (filter) {
                                paneField.addPane(filter.createOptionPane(_this.saveState));
                                _this.saveState();
                            }
                        },
                        addFilterButtonLabel: 'AddFilter',
                        onlyUniqueFilters: true,
                        isAlreadyInUse: function (aircraftProperty) {
                            return VRS.aircraftFilterHelper.isFilterInUse(_this._Filters, aircraftProperty);
                        }
                    });
                    panesTypeSettings.hookPaneRemoved(_this.filterPaneRemoved, _this);
                }
                return pane;
            };
            this.addFilter = function (filterOrPropertyId) {
                var filter = filterOrPropertyId;
                if (!(filter instanceof VRS.AircraftFilter)) {
                    filter = VRS.aircraftFilterHelper.createFilter(filterOrPropertyId);
                }
                if (VRS.aircraftFilterHelper.isFilterInUse(_this._Filters, filter.getProperty()))
                    filter = null;
                else {
                    _this._Filters.push(filter);
                    _this._Dispatcher.raise(_this._Events.filterChanged);
                }
                return filter;
            };
            this.filterPaneRemoved = function (pane, index) {
                _this.removeFilterAt(index);
                _this.saveState();
            };
            this.removeFilterAt = function (index) {
                _this._Filters.splice(index, 1);
                _this._Dispatcher.raise(_this._Events.filterChanged);
            };
            this.getFilterIndexForProperty = function (filterProperty) {
                return VRS.aircraftFilterHelper.getIndexForFilterProperty(_this._Filters, filterProperty);
            };
            this.getFilterForProperty = function (filterProperty) {
                return VRS.aircraftFilterHelper.getFilterForFilterProperty(_this._Filters, filterProperty);
            };
            this.removeFilterForProperty = function (filterProperty) {
                var index = VRS.aircraftFilterHelper.getIndexForFilterProperty(_this._Filters, filterProperty);
                if (index !== -1) {
                    _this.removeFilterAt(index);
                }
            };
            this.addFilterForOneConditionProperty = function (filterProperty, condition, reverseCondition, value) {
                _this.removeFilterForProperty(filterProperty);
                var filter = VRS.aircraftFilterHelper.createFilter(filterProperty);
                var condObj = filter.getValueCondition();
                condObj.setCondition(condition);
                condObj.setReverseCondition(reverseCondition);
                condObj.setValue(value);
                return _this.addFilter(filter);
            };
            this.addFilterForTwoConditionProperty = function (filterProperty, condition, reverseCondition, value1, value2) {
                _this.removeFilterForProperty(filterProperty);
                var filter = VRS.aircraftFilterHelper.createFilter(filterProperty);
                var condObj = filter.getValueCondition();
                condObj.setCondition(condition);
                condObj.setReverseCondition(reverseCondition);
                condObj.setValue1(value1);
                condObj.setValue2(value2);
                return _this.addFilter(filter);
            };
            this.hasFilterForOneConditionProperty = function (filterProperty, condition, reverseCondition, value) {
                var result = false;
                var filter = _this.getFilterForProperty(filterProperty);
                if (filter) {
                    var condObj = filter.getValueCondition();
                    var isReversed = condObj.getReverseCondition();
                    if (isReversed === undefined)
                        isReversed = false;
                    result = condObj.getCondition() == condition &&
                        isReversed == reverseCondition &&
                        condObj.getValue() == value;
                }
                return result;
            };
            this.hasFilterForTwoConditionProperty = function (filterProperty, condition, reverseCondition, value1, value2) {
                var result = false;
                var filter = _this.getFilterForProperty(filterProperty);
                if (filter) {
                    var condObj = filter.getValueCondition();
                    var isReversed = condObj.getReverseCondition();
                    if (isReversed === undefined)
                        isReversed = false;
                    result = condObj.getCondition() == condition &&
                        isReversed == reverseCondition &&
                        condObj.getValue1() == value1 &&
                        condObj.getValue2() == value2;
                }
                return result;
            };
            this.filterAircraft = function (aircraft) {
                var result = !_this._Enabled;
                if (!result) {
                    result = VRS.aircraftFilterHelper.aircraftPasses(aircraft, _this._Filters);
                }
                return result;
            };
            this.aircraftList_FetchingList = function (xhrParams) {
                if (_this._Enabled) {
                    VRS.aircraftFilterHelper.addToQueryParameters(_this._Filters, xhrParams, _this._Settings.unitDisplayPreferences);
                }
            };
            settings = $.extend(settings, {
                name: 'default'
            });
            this._Settings = settings;
            this._AircraftList_FetchingListHookResult = settings.aircraftList.hookFetchingList(this.aircraftList_FetchingList, this);
        }
        AircraftListFilter.prototype.persistenceKey = function () {
            return 'vrsAircraftFilter-' + this._Settings.name;
        };
        AircraftListFilter.prototype.createSettings = function () {
            var filters = VRS.aircraftFilterHelper.serialiseFiltersList(this._Filters);
            return {
                filters: filters,
                enabled: this._Enabled
            };
        };
        return AircraftListFilter;
    })();
    VRS.AircraftListFilter = AircraftListFilter;
})(VRS || (VRS = {}));
