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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListDefaultFiltersEnabled = VRS.globalOptions.aircraftListDefaultFiltersEnabled !== undefined ? VRS.globalOptions.aircraftListDefaultFiltersEnabled : false; // True if filters are enabled by default.
    VRS.globalOptions.aircraftListFilters = VRS.globalOptions.aircraftListFilters || [];            // The list of filters that new browsers start with.
    VRS.globalOptions.aircraftListFiltersLimit = VRS.globalOptions.aircraftListFiltersLimit !== undefined ? VRS.globalOptions.aircraftListFiltersLimit : 12;    // The maximum number of filters that can be applied by the user. Set to zero to disable filters.

    /**
     * The settings to pass when creating a new instance of AircraftListFilter.
     */
    export interface AircraftListFilter_Settings
    {
        /**
         * The name to use when storing the object's state.
         */
        name?: string;

        /**
         * The aircraft list that we will be filtering.
         */
        aircraftList: AircraftList;

        /**
         * The unit display preferences to use when filtering the aircraft list.
         */
        unitDisplayPreferences: UnitDisplayPreferences;
    }

    /**
     * The state saved by instances of AircraftListFilter.
     */
    export interface AircraftListFilter_SaveState
    {
        filters:    ISerialisedFilter[];
        enabled:    boolean;
    }

    /**
     * A class that can be used to filter the aircraft list that is fetched from the server.
     */
    export class AircraftListFilter implements ISelfPersist<AircraftListFilter_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftListFilter'
        });
        private _Events = {
            filterChanged:      'filterChanged',
            enabledChanged:     'enabledChanged'
        };
        private _Settings: AircraftListFilter_Settings;

        private _Filters: AircraftFilter[] = VRS.globalOptions.aircraftListFilters;     // The filters that the list will be passed through
        private _AircraftList_FetchingListHookResult: IEventHandle;                     // The hook result for the the aircraft list fetching list event
        private _Enabled: boolean;

        constructor(settings: AircraftListFilter_Settings)
        {
            settings = $.extend(settings, {
                name: 'default'
            });
            this._Settings = settings;

            this._AircraftList_FetchingListHookResult =  settings.aircraftList.hookFetchingList(this.aircraftList_FetchingList, this);
        }

        getName = () : string =>
        {
            return this._Settings.name;
        }

        /**
         * Gets a value indicating whether the filters are enabled.
         */
        getEnabled = () : boolean =>
        {
            return this._Enabled;
        }
        setEnabled = (value: boolean) =>
        {
            if(this._Enabled !== value) {
                this._Enabled = value;
                this._Dispatcher.raise(this._Events.enabledChanged);
            }
        }

        hookFilterChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.filterChanged, callback, forceThis);
        }

        hookEnabledChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.enabledChanged, callback, forceThis);
        }

        unhook = (hookResult: IEventHandle) =>
        {
            this._Dispatcher.unhook(hookResult)
        }

        /**
         * Releases any resources held by the object.
         */
        dispose = () =>
        {
            if(this._AircraftList_FetchingListHookResult) {
                this._Settings.aircraftList.unhook(this._AircraftList_FetchingListHookResult);
                this._AircraftList_FetchingListHookResult = null;
            }
            this._Settings.aircraftList = null;
        }

        /**
         * Saves the current state.
         */
        saveState = () =>
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Returns the previously saved state or the current state if the state has never been saved.
         */
        loadState = () : AircraftListFilter_SaveState =>
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);
            result.filters = VRS.aircraftFilterHelper.buildValidFiltersList(result.filters, VRS.globalOptions.aircraftListFiltersLimit);

            return result;
        }

        /**
         * Applies the previously saved state to this object.
         */
        applyState = (settings: AircraftListFilter_SaveState) =>
        {
            this.setEnabled(settings.enabled);
            this._Filters = [];
            $.each(settings.filters, (idx, serialisedFilter) => {
                var aircraftFilter = <AircraftFilter> VRS.aircraftFilterHelper.createFilter(serialisedFilter.property);
                aircraftFilter.applySerialisedObject(serialisedFilter);
                this._Filters.push(aircraftFilter);
            });

            this._Dispatcher.raise(this._Events.filterChanged);
        }

        /**
         * Loads and applies the saved state.
         */
        loadAndApplyState = () =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the state will be saved.
         */
        private persistenceKey() : string
        {
            return 'vrsAircraftFilter-' + this._Settings.name;
        }

        /**
         * Creates the object that carries the saved state to disk.
         */
        private createSettings() : AircraftListFilter_SaveState
        {
            var filters = VRS.aircraftFilterHelper.serialiseFiltersList(this._Filters);
            return {
                filters: filters,
                enabled: this._Enabled
            };
        }

        /**
         * Creates the option pane used to take configuration settings for this object.
         */
        createOptionPane = (displayOrder: number) : OptionPane =>
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAircraftListFilterPane',
                titleKey:       'Filters',
                displayOrder:   displayOrder
            });

            if(VRS.globalOptions.aircraftListFiltersLimit !== 0) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'enable',
                    labelKey:       'EnableFilters',
                    getValue:       this.getEnabled,
                    setValue:       this.setEnabled,
                    saveState:      this.saveState
                }));

                var panesTypeSettings = VRS.aircraftFilterHelper.addConfigureFiltersListToPane({
                    pane: pane,
                    filters:                this._Filters,
                    saveState:              () => this.saveState(),
                    maxFilters:             VRS.globalOptions.aircraftListFiltersLimit,
                    allowableProperties:    VRS.enumHelper.getEnumValues(VRS.AircraftFilterProperty),
                    addFilter: (newFilter: AircraftFilterPropertyEnum, paneField: OptionFieldPaneList) => {
                        var filter = this.addFilter(newFilter);
                        if(filter) {
                            paneField.addPane(filter.createOptionPane(this.saveState));
                            this.saveState();
                        }
                    },
                    addFilterButtonLabel: 'AddFilter',
                    onlyUniqueFilters: true,
                    isAlreadyInUse: (aircraftProperty: AircraftFilterPropertyEnum) : boolean => {
                        return VRS.aircraftFilterHelper.isFilterInUse(this._Filters, aircraftProperty);
                    }
                });

                panesTypeSettings.hookPaneRemoved(this.filterPaneRemoved, this);
            }

            return pane;
        }

        /**
         * Adds a new filter to the object.
         */
        addFilter = (filterOrPropertyId: AircraftFilterPropertyEnum | AircraftFilter) : AircraftFilter =>
        {
            var filter = filterOrPropertyId;
            if(!(filter instanceof VRS.AircraftFilter)) {
                filter = <AircraftFilter>VRS.aircraftFilterHelper.createFilter(<AircraftFilterPropertyEnum>filterOrPropertyId);
            }
            if(VRS.aircraftFilterHelper.isFilterInUse(this._Filters, (<AircraftFilter>filter).getProperty())) filter = null;
            else {
                this._Filters.push(<AircraftFilter>filter);
                this._Dispatcher.raise(this._Events.filterChanged);
            }

            return <AircraftFilter>filter;
        }

        /**
         * Removes the filter at the index passed across.
         */
        private filterPaneRemoved = (pane: OptionPane, index?: number) =>
        {
            this.removeFilterAt(index);
            this.saveState();
        }

        /**
         * Removes the filter at the index passed across.
         */
        removeFilterAt = (index: number) =>
        {
            this._Filters.splice(index, 1);
            this._Dispatcher.raise(this._Events.filterChanged);
        }

        /**
         * Returns the index of the filter for the filter property passed across or -1 if no such filter exists.
         */
        getFilterIndexForProperty = (filterProperty: AircraftFilterPropertyEnum) : number =>
        {
            return VRS.aircraftFilterHelper.getIndexForFilterProperty(this._Filters, filterProperty);
        }

        /**
         * Returns the filter that exists for the filter property passed across or null if no such filter exists.
         */
        getFilterForProperty = (filterProperty: AircraftFilterPropertyEnum) : AircraftFilter =>
        {
            return <AircraftFilter>VRS.aircraftFilterHelper.getFilterForFilterProperty(this._Filters, filterProperty);
        }

        /**
         * Removes the filter for the property passed across, if it exists.
         */
        removeFilterForProperty = (filterProperty: AircraftFilterPropertyEnum) =>
        {
            var index = VRS.aircraftFilterHelper.getIndexForFilterProperty(this._Filters, filterProperty);
            if(index !== -1) {
                this.removeFilterAt(index);
            }
        }

        /**
         * Adds a filter for a single-condition property. If the property is already present then it is
         * removed first. Note that you must ensure that the condition and value are appropriate.
         */
        addFilterForOneConditionProperty = (filterProperty: AircraftFilterPropertyEnum, condition: FilterConditionEnum, reverseCondition: boolean, value: any) : AircraftFilter =>
        {
            this.removeFilterForProperty(filterProperty);

            var filter = <AircraftFilter>VRS.aircraftFilterHelper.createFilter(filterProperty);
            var condObj = <OneValueCondition>filter.getValueCondition();
            condObj.setCondition(condition);
            condObj.setReverseCondition(reverseCondition);
            condObj.setValue(value);

            return this.addFilter(filter);
        }

        /**
         * Adds a filter for a two-condition property. If the property is already present then it is
         * removed first. Note that you must ensure that the condition and value are appropriate.
         */
        addFilterForTwoConditionProperty = (filterProperty: AircraftFilterPropertyEnum, condition: FilterConditionEnum, reverseCondition: boolean, value1: any, value2: any) : AircraftFilter =>
        {
            this.removeFilterForProperty(filterProperty);

            var filter = <AircraftFilter>VRS.aircraftFilterHelper.createFilter(filterProperty);
            var condObj = <TwoValueCondition>filter.getValueCondition();
            condObj.setCondition(condition);
            condObj.setReverseCondition(reverseCondition);
            condObj.setValue1(value1);
            condObj.setValue2(value2);

            return this.addFilter(filter);
        }

        /**
         * Returns true if there is a filter for the filter property passed across and the filter's condition
         * and value match the parameters passed across. Note that you must ensure that the property uses a OneValue
         * condition.
         */
        hasFilterForOneConditionProperty = (filterProperty: AircraftFilterPropertyEnum, condition: FilterConditionEnum, reverseCondition: boolean, value: any) : boolean =>
        {
            var result = false;
            var filter = this.getFilterForProperty(filterProperty);

            if(filter) {
                var condObj = <OneValueCondition>filter.getValueCondition();
                var isReversed = condObj.getReverseCondition();
                if(isReversed === undefined) isReversed = false;

                result = condObj.getCondition() == condition &&
                         isReversed == reverseCondition &&
                         condObj.getValue() == value;
            }

            return result;
        }

        /**
         * Returns true if there is a filter for the filter property passed across and the filter's condition
         * and values match the parameters passed across. Note that you must ensure that the property uses a OneValue
         * condition.
         */
        hasFilterForTwoConditionProperty = (filterProperty: AircraftFilterPropertyEnum, condition: FilterConditionEnum, reverseCondition: boolean, value1: any, value2: any) : boolean =>
        {
            var result = false;
            var filter = this.getFilterForProperty(filterProperty);

            if(filter) {
                var condObj = <TwoValueCondition>filter.getValueCondition();
                var isReversed = condObj.getReverseCondition();
                if(isReversed === undefined) isReversed = false;

                result = condObj.getCondition() == condition &&
                         isReversed == reverseCondition &&
                         condObj.getValue1() == value1 &&
                         condObj.getValue2() == value2;
            }

            return result;
        }

        /**
         * Applies the filter to a single aircraft. Note that the website does not use this - it passes all of the filters
         * to the server and it only returns aircraft that match them (the intention being that we want to reduce browser
         * workload, not increase it). This method is probably only of use to 3rd parties that want to filter lists.
         */
        filterAircraft = (aircraft: Aircraft) : boolean =>
        {
            var result = !this._Enabled;
            if(!result) {
                result = VRS.aircraftFilterHelper.aircraftPasses(aircraft, this._Filters);
            }

            return result;
        }

        /**
         * Called when the aircraft list is being updated.
         */
        private aircraftList_FetchingList = (xhrParams: Object) =>
        {
            if(this._Enabled) {
                VRS.aircraftFilterHelper.addToQueryParameters(this._Filters, xhrParams, this._Settings.unitDisplayPreferences);
            }
        }
    }
}