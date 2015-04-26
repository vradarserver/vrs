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
/**
 * @fileoverview Code that can manage filtering an aircraft list.
 */

/// <reference path="../jquery.js" />
(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListDefaultFiltersEnabled = VRS.globalOptions.aircraftListDefaultFiltersEnabled !== undefined ? VRS.globalOptions.aircraftListDefaultFiltersEnabled : false; // True if filters are enabled by default.
    VRS.globalOptions.aircraftListFilters = VRS.globalOptions.aircraftListFilters || [];            // The list of filters that new browsers start with.
    VRS.globalOptions.aircraftListFiltersLimit = VRS.globalOptions.aircraftListFiltersLimit !== undefined ? VRS.globalOptions.aircraftListFiltersLimit : 12;    // The maximum number of filters that can be applied by the user. Set to zero to disable filters.
    //endregion

    //region AircraftListFilter
    /**
     * A class that can be used to filter an aircraft list.
     * @param {Object}                      settings
     * @param {string=}                    [settings.name]                      The name to use when storing the object's state.
     * @param {VRS.AircraftList}            settings.aircraftList               The aircraft list that we will be filtering.
     * @param {VRS.UnitDisplayPreferences}  settings.unitDisplayPreferences     The unit display preferences to use when filtering the aircraft list.
     * @constructor
     */
    VRS.AircraftListFilter = function(settings)
    {
        settings = $.extend(settings, {
            name: 'default'
        });

        //region -- Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftListFilter'
        });
        var _Events = {
            filterChanged:      'filterChanged',
            enabledChanged:     'enabledChanged'
        };

        /**
         * The filters that the list will be passed through.
         * @type {VRS.AircraftFilter[]}
         * @private
         */
        var _Filters = VRS.globalOptions.aircraftListFilters;

        /**
         * The hook result for the the aircraft list fetching list event.
         * @type {object}
         * @private
         */
        var _AircraftList_FetchingListHookResult = settings.aircraftList.hookFetchingList(aircraftList_FetchingList, this);
        //endregion

        //region -- Properties
        var _Name = settings.name;
        this.getName = function() { return _Name; };

        /**
         * True if the filters are enabled, otherwise false.
         * @type {boolean}
         */
        var _Enabled;
        /**
         * Gets a value indicating whether the filters are enabled.
         * @returns {boolean}
         */
        this.getEnabled = function() { return _Enabled; };
        /**
         * Sets a value indicating whether the filters are enabled.
         * @param {boolean} value
         */
        this.setEnabled = function(value) {
            if(_Enabled !== value) {
                _Enabled = value;
                _Dispatcher.raise(_Events.enabledChanged);
            }
        };
        //endregion

        //region -- Events exposed
        //noinspection JSUnusedGlobalSymbols
        this.hookFilterChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.filterChanged, callback, forceThis); };
        this.hookEnabledChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.enabledChanged, callback, forceThis); };
        this.unhook = function(hookResult) { return _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- dispose
        /**
         * Releases any resources held by the object.
         */
        this.dispose = function()
        {
            if(_AircraftList_FetchingListHookResult) {
                settings.aircraftList.unhook(_AircraftList_FetchingListHookResult);
                _AircraftList_FetchingListHookResult = null;
            }
            settings.aircraftList = null;
        };
        //endregion

        //region -- saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Returns the previously saved state or the current state if the state has never been saved.
         * @returns {VRS_STATE_AIRCRAFTLISTFILTER}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            var result = $.extend(createSettings(), savedSettings);
            result.filters = VRS.aircraftFilterHelper.buildValidFiltersList(result.filters, VRS.globalOptions.aircraftListFiltersLimit);

            return result;
        };

        /**
         * Applies the previously saved state to this object.
         * @param {VRS_STATE_AIRCRAFTLISTFILTER} settings
         */
        this.applyState = function(settings)
        {
            that.setEnabled(settings.enabled);
            _Filters = [];
            $.each(settings.filters, function(idx, serialisedFilter) {
                var aircraftFilter = VRS.aircraftFilterHelper.createFilter(serialisedFilter.property);
                aircraftFilter.applySerialisedObject(serialisedFilter);
                _Filters.push(aircraftFilter);
            });

            _Dispatcher.raise(_Events.filterChanged);
        };

        /**
         * Loads and applies the saved state.
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
            return 'vrsAircraftFilter-' + _Name;
        }

        /**
         * Creates the object that carries the saved state to disk.
         * @returns {VRS_STATE_AIRCRAFTLISTFILTER}
         */
        function createSettings()
        {
            var filters = VRS.aircraftFilterHelper.serialiseFiltersList(_Filters);
            return {
                filters: filters,
                enabled: _Enabled
            };
        }
        //endregion

        //region -- createOptionPane
        /**
         * Creates the option pane used to take configuration settings for this object.
         * @param {number} displayOrder The relative display order for the pane amongst other panes.
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder)
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAircraftListFilterPane',
                titleKey:       'Filters',
                displayOrder:   displayOrder
            });

            if(VRS.aircraftListFiltersLimit !== 0) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'enable',
                    labelKey:       'EnableFilters',
                    getValue:       that.getEnabled,
                    setValue:       that.setEnabled,
                    saveState:      that.saveState
                }));

                var panesTypeSettings = VRS.aircraftFilterHelper.addConfigureFiltersListToPane({
                    pane: pane,
                    filters: _Filters,
                    saveState: function() { that.saveState(); },
                    maxFilters: VRS.globalOptions.aircraftListFiltersLimit,
                    allowableProperties: VRS.enumHelper.getEnumValues(VRS.AircraftFilterProperty),
                    addFilter: function(newFilter, paneField) {
                        var filter = that.addFilter(newFilter);
                        if(filter) {
                            paneField.addPane(filter.createOptionPane(that.saveState));
                            that.saveState();
                        }
                    },
                    addFilterButtonLabel: 'AddFilter',
                    onlyUniqueFilters: true,
                    isAlreadyInUse: $.proxy(function(/** VRS.AircraftFilterProperty */ aircraftProperty) {
                        return VRS.aircraftFilterHelper.isFilterInUse(_Filters, aircraftProperty);
                    }, this)
                });

                panesTypeSettings.hookPaneRemoved(filterPaneRemoved, this);
            }

            return pane;
        };
        //endregion

        //region -- addFilter, filterPaneRemoved, removeFilterAt
        /**
         * Adds a new filter to the object.
         * @param {VRS.AircraftFilter|VRS.AircraftFilterProperty} filterOrPropertyId Either a filter or a VRS.AircraftFilterProperty property name.
         * @returns {VRS.AircraftFilter} Either the filter passed in or the filter built from the property name.
         */
        this.addFilter = function(filterOrPropertyId)
        {
            var filter = filterOrPropertyId;
            if(!(filter instanceof VRS.AircraftFilter)) {
                filter = VRS.aircraftFilterHelper.createFilter(filterOrPropertyId);
            }
            if(VRS.aircraftFilterHelper.isFilterInUse(_Filters, filter.getProperty())) filter = null;
            else {
                _Filters.push(filter);
                _Dispatcher.raise(_Events.filterChanged);
            }

            return filter;
        };

        //noinspection JSUnusedLocalSymbols
        /**
         * Removes the filter at the index passed across.
         * @param {VRS.OptionPane=} pane Unused.
         * @param {number} index
         */
        function filterPaneRemoved(pane, index)
        {
            that.removeFilterAt(index);
            that.saveState();
        }

        /**
         * Removes the filter at the index passed across.
         * @param {number} index
         */
        this.removeFilterAt = function(index)
        {
            _Filters.splice(index, 1);
            _Dispatcher.raise(_Events.filterChanged);
        };
        //endregion

        //region Programmatical control over filters - getFilterForProperty, addFilterForProperty, removeFilterForProperty
        /**
         * Returns the index of the filter for the filter property passed across or -1 if no such filter exists.
         * @param {VRS.AircraftFilterProperty} filterProperty A VRS.AircraftFilterProperty property name.
         * @returns {number}
         */
        this.getFilterIndexForProperty = function(filterProperty)
        {
            return VRS.aircraftFilterHelper.getIndexForFilterProperty(_Filters, filterProperty);
        };

        /**
         * Returns the filter that exists for the filter property passed across or null if no such filter exists.
         * @param {VRS.AircraftFilterProperty} filterProperty A VRS.AircraftFilterProperty property name.
         * @returns {VRS.AircraftFilter}
         */
        this.getFilterForProperty = function(filterProperty)
        {
            return VRS.aircraftFilterHelper.getFilterForFilterProperty(_Filters, filterProperty);
        };

        /**
         * Removes the filter for the property passed across, if it exists.
         * @param {VRS.AircraftFilterProperty} filterProperty A VRS.AircraftFilterProperty property name.
         */
        this.removeFilterForProperty = function(filterProperty)
        {
            var index = VRS.aircraftFilterHelper.getIndexForFilterProperty(_Filters, filterProperty);
            if(index !== -1) {
                that.removeFilterAt(index);
            }
        };

        /**
         * Adds a filter for a single-condition property. If the property is already present then it is
         * removed first. Note that you must ensure that the condition and value are appropriate.
         * @param {VRS.AircraftFilterProperty}  filterProperty
         * @param {VRS.FilterCondition}         condition
         * @param {boolean}                     reverseCondition
         * @param {*}                           value
         * @returns {VRS.AircraftFilter}
         */
        this.addFilterForOneConditionProperty = function(filterProperty, condition, reverseCondition, value)
        {
            that.removeFilterForProperty(filterProperty);

            var filter = VRS.aircraftFilterHelper.createFilter(filterProperty);
            var condObj = filter.getValueCondition();
            condObj.setCondition(condition);
            condObj.setReverseCondition(reverseCondition);
            condObj.setValue(value);

            return that.addFilter(filter);
        };

        /**
         * Adds a filter for a two-condition property. If the property is already present then it is
         * removed first. Note that you must ensure that the condition and value are appropriate.
         * @param {VRS.AircraftFilterProperty}  filterProperty
         * @param {VRS.FilterCondition}         condition
         * @param {boolean}                     reverseCondition
         * @param {*}                           value1
         * @param {*}                           value2
         * @returns {VRS.AircraftFilter}
         */
        this.addFilterForTwoConditionProperty = function(filterProperty, condition, reverseCondition, value1, value2)
        {
            that.removeFilterForProperty(filterProperty);

            var filter = VRS.aircraftFilterHelper.createFilter(filterProperty);
            var condObj = filter.getValueCondition();
            condObj.setCondition(condition);
            condObj.setReverseCondition(reverseCondition);
            condObj.setValue1(value1);
            condObj.setValue2(value2);

            return that.addFilter(filter);
        };

        /**
         * Returns true if there is a filter for the filter property passed across and the filter's condition
         * and value match the parameters passed across. Note that you must ensure that the property uses a OneValue
         * condition.
         * @param {VRS.AircraftFilterProperty}  filterProperty
         * @param {VRS.FilterCondition}         condition
         * @param {boolean}                     reverseCondition
         * @param {*}                           value
         * @returns {boolean}
         */
        this.hasFilterForOneConditionProperty = function(filterProperty, condition, reverseCondition, value)
        {
            var result = false;
            var filter = that.getFilterForProperty(filterProperty);

            if(filter) {
                var condObj = filter.getValueCondition();
                var isReversed = condObj.getReverseCondition();
                if(isReversed === undefined) isReversed = false;

                result = condObj.getCondition() == condition &&
                         isReversed == reverseCondition &&
                         condObj.getValue() == value;
            }

            return result;
        };

        /**
         * Returns true if there is a filter for the filter property passed across and the filter's condition
         * and values match the parameters passed across. Note that you must ensure that the property uses a OneValue
         * condition.
         * @param {VRS.AircraftFilterProperty}  filterProperty
         * @param {VRS.FilterCondition}         condition
         * @param {boolean}                     reverseCondition
         * @param {*}                           value1
         * @param {*}                           value2
         * @returns {boolean}
         */
        this.hasFilterForTwoConditionProperty = function(filterProperty, condition, reverseCondition, value1, value2)
        {
            var result = false;
            var filter = that.getFilterForProperty(filterProperty);

            if(filter) {
                var condObj = filter.getValueCondition();
                var isReversed = condObj.getReverseCondition();
                if(isReversed === undefined) isReversed = false;

                result = condObj.getCondition() == condition &&
                         isReversed == reverseCondition &&
                         condObj.getValue1() == value1 &&
                         condObj.getValue2() == value2;
            }

            return result;
        };
        //endregion

        //region -- filterAircraft
        //noinspection JSUnusedGlobalSymbols
        /**
         * Applies the filter to a single aircraft. Note that the website does not use this - it passes all of the filters
         * to the server and it only returns aircraft that match them (the intention being that we want to reduce browser
         * workload, not increase it). This method is probably only of use to 3rd parties that want to filter lists.
         * @param {VRS.Aircraft} aircraft
         * @returns {boolean} True if the aircraft passes, false if it does not.
         */
        this.filterAircraft = function(aircraft)
        {
            var result = !_Enabled;
            if(!result) result = VRS.aircraftFilterHelper.aircraftPasses(aircraft, _Filters);

            return result;
        };
        //endregion

        //region -- Events subscribed
        /**
         * Called when the aircraft list is being updated.
         * @param {Object} xhrParams
         //* @param {Object} xhrHeaders
         */
        function aircraftList_FetchingList(xhrParams)
        {
            if(_Enabled) VRS.aircraftFilterHelper.addToQueryParameters(_Filters, xhrParams, settings.unitDisplayPreferences);
        }
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));