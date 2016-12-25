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
 * @fileoverview A class that can auto-select aircraft based on a set of criteria.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftAutoSelectEnabled = VRS.globalOptions.aircraftAutoSelectEnabled !== undefined ? VRS.globalOptions.aircraftAutoSelectEnabled : true;   // True if auto-select is enabled by default.
    VRS.globalOptions.aircraftAutoSelectClosest = VRS.globalOptions.aircraftAutoSelectClosest !== undefined ? VRS.globalOptions.aircraftAutoSelectClosest : true;   // True if the closest matching aircraft should be auto-selected, false if the furthest matching aircraft should be auto-selected.
    VRS.globalOptions.aircraftAutoSelectOffRadarAction = VRS.globalOptions.aircraftAutoSelectOffRadarAction || VRS.OffRadarAction.EnableAutoSelect;                 // What to do when an aircraft goes out of range of the radar.
    VRS.globalOptions.aircraftAutoSelectFilters = VRS.globalOptions.aircraftAutoSelectFilters || [];                                                                // The list of filters that auto-select starts off with.
    VRS.globalOptions.aircraftAutoSelectFiltersLimit = VRS.globalOptions.aircraftAutoSelectFiltersLimit !== undefined ? VRS.globalOptions.aircraftAutoSelectFiltersLimit : 2; // The maximum number of auto-select filters that we're going to allow.

    export interface AircraftAutoSelect_SaveState
    {
        enabled:             boolean;
        selectClosest:       boolean;
        offRadarAction:      OffRadarActionEnum;
        filters:             ISerialisedFilter[];
    }

    /**
     * A class that can auto-select aircraft on each refresh.
     */
    export class AircraftAutoSelect implements ISelfPersist<AircraftAutoSelect_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftAutoSelect'
        });
        private _Events = {
            enabledChanged:         'enabledChanged',
            aircraftSelectedByIcao: 'aircraftSelectedByIcao'
        };

        private _AircraftList: AircraftList;
        private _LastAircraftSelected: Aircraft;
        private _Name: string;
        private _Enabled: boolean;
        private _SelectClosest: boolean;
        private _OffRadarAction: OffRadarActionEnum;
        private _Filters: AircraftFilter[];
        private _AircraftListUpdatedHook: IEventHandle;
        private _SelectedAircraftChangedHook: IEventHandle;
        private _SelectAircraftByIcao: string;
        private _AutoClearSelectAircraftByIcao = true;

        constructor(aircraftList: AircraftList, name?: string)
        {
            this._AircraftList = aircraftList;
            this._Name = name || 'default';
            this._Enabled = VRS.globalOptions.aircraftAutoSelectEnabled;
            this._SelectClosest = VRS.globalOptions.aircraftAutoSelectClosest;
            this._OffRadarAction = VRS.globalOptions.aircraftAutoSelectOffRadarAction;
            this._Filters = VRS.globalOptions.aircraftAutoSelectFilters;

            this._AircraftListUpdatedHook = aircraftList.hookUpdated(this.aircraftListUpdated, this);
            this._SelectedAircraftChangedHook = aircraftList.hookSelectedAircraftChanged(this.selectedAircraftChanged, this);
        }

        dispose = () =>
        {
            if(this._AircraftListUpdatedHook)       this._AircraftList.unhook(this._AircraftListUpdatedHook);
            if(this._SelectedAircraftChangedHook)   this._AircraftList.unhook(this._SelectedAircraftChangedHook);
            this._AircraftListUpdatedHook = this._SelectedAircraftChangedHook = null;
        }

        /**
         * Gets the name of the object. This is used when storing and loading settings.
         */
        getName = () : string =>
        {
            return this._Name;
        }

        /**
         * Gets a value indicating whether the object is actively setting the selected aircraft.
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

                if(this._Enabled && this._AircraftList) {
                    var closestAircraft = this.closestAircraft(this._AircraftList);
                    if(closestAircraft && closestAircraft !== this._AircraftList.getSelectedAircraft()) {
                        this._AircraftList.setSelectedAircraft(closestAircraft, false);
                    }
                }
            }
        }

        /**
         * Gets a value indicating that the closest aircraft is to be selected.
         */
        getSelectClosest = () : boolean =>
        {
            return this._SelectClosest;
        }
        setSelectClosest = (value: boolean) =>
        {
            this._SelectClosest = value;
        }

        /**
         * Gets a value that describes what to do when an aircraft goes off the radar.
         */
        getOffRadarAction = () : OffRadarActionEnum =>
        {
            return this._OffRadarAction;
        }
        setOffRadarAction = (value: OffRadarActionEnum) =>
        {
            this._OffRadarAction = value;
        }

        /**
         * Returns a clone of the filter at the specified index.
         */
        getFilter = (index: number) : AircraftFilter =>
        {
            return <AircraftFilter>this._Filters[index].clone();
        }
        setFilter = (index: number, aircraftFilter: AircraftFilter) =>
        {
            if(this._Filters.length < VRS.globalOptions.aircraftListFiltersLimit) {
                var existing = index < this._Filters.length ? this._Filters[index] : null;
                if(existing && !existing.equals(aircraftFilter)) this._Filters[index] = aircraftFilter;
            }
        }

        /**
         * Gets or sets the ICAO that will be selected on the next update.
         */
        getSelectAircraftByIcao = () : string =>
        {
            return this._SelectAircraftByIcao;
        }
        setSelectAircraftByIcao = (icao: string) =>
        {
            this._SelectAircraftByIcao = icao;
        }

        /**
         * Gets or sets a value indicating that SelectAircraftByIcao will be automatically cleared after the next
         * fetch of the aircraft list.
         */
        getAutoClearSelectAircraftByIcao = () : boolean =>
        {
            return this._AutoClearSelectAircraftByIcao;
        }
        setAutoClearSelectAircraftByIcao = (value: boolean) =>
        {
            this._AutoClearSelectAircraftByIcao = value;
        }

        /**
         * Raised after the Enabled property value has changed.
         */
        hookEnabledChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.enabledChanged, callback, forceThis);
        }

        /**
         * Raised after an aircraft is selected by ICAO.
         */
        hookAircraftSelectedByIcao = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.aircraftSelectedByIcao, callback, forceThis);
        }

        /**
         * Unhooks an event hooked on this object.
         */
        unhook = (hookResult: IEventHandle) =>
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Saves the current state of the object.
         */
        saveState = () =>
        {
            VRS.configStorage.save(this.persistenceKey(), this.createSettings());
        }

        /**
         * Returns the saved state of the object.
         */
        loadState = () : AircraftAutoSelect_SaveState =>
        {
            var savedSettings: AircraftAutoSelect_SaveState = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);
            result.filters = VRS.aircraftFilterHelper.buildValidFiltersList(result.filters, VRS.globalOptions.aircraftAutoSelectFiltersLimit);

            return result;
        }

        /**
         * Applies a saved state to the object.
         */
        applyState = (settings: AircraftAutoSelect_SaveState) =>
        {
            this.setEnabled(settings.enabled);
            this.setSelectClosest(settings.selectClosest);
            this.setOffRadarAction(settings.offRadarAction);

            this._Filters = [];
            var self = this;
            $.each(settings.filters, function(idx, serialisedFilter) {
                var aircraftFilter = <AircraftFilter>VRS.aircraftFilterHelper.createFilter(serialisedFilter.property);
                aircraftFilter.applySerialisedObject(serialisedFilter);
                self._Filters.push(aircraftFilter);
            });
        }

        /**
         * Loads and applies the saved state to the object.
         */
        loadAndApplyState = () =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key under which the object's settings are saved.
         */
        private persistenceKey = () : string =>
        {
            return 'vrsAircraftAutoSelect-' + this._Name;
        }

        /**
         * Creates the state object.
         */
        private createSettings = () : AircraftAutoSelect_SaveState =>
        {
            var filters = VRS.aircraftFilterHelper.serialiseFiltersList(this._Filters);
            return {
                enabled: this._Enabled,
                selectClosest: this._SelectClosest,
                offRadarAction: this._OffRadarAction,
                filters: filters
            };
        }

        /**
         * Creates the option pane that allows configuration of the object.
         */
        createOptionPane = (displayOrder: number) : OptionPane[] =>
        {
            var result = [];

            var pane = new VRS.OptionPane({
                name:           'vrsAircraftAutoSelectPane1',
                titleKey:       'PaneAutoSelect',
                displayOrder:   displayOrder
            });
            result.push(pane);

            pane.addField(new VRS.OptionFieldCheckBox({
                name:           'enable',
                labelKey:       'AutoSelectAircraft',
                getValue:       this.getEnabled,
                setValue:       this.setEnabled,
                saveState:      this.saveState,
                hookChanged:    this.hookEnabledChanged,
                unhookChanged:  this.unhook
            }));

            pane.addField(new VRS.OptionFieldRadioButton({
                name:           'closest',
                labelKey:       'Select',
                getValue:       this.getSelectClosest,
                setValue:       this.setSelectClosest,
                saveState:      this.saveState,
                values: [
                    new VRS.ValueText({ value: true, textKey: 'ClosestToCurrentLocation' }),
                    new VRS.ValueText({ value: false, textKey: 'FurthestFromCurrentLocation' })
                ]
            }));

            if(VRS.globalOptions.aircraftListFiltersLimit !== 0) {
                var panesTypeSettings = VRS.aircraftFilterHelper.addConfigureFiltersListToPane({
                    pane:       pane,
                    filters:    this._Filters,
                    saveState:  $.proxy(this.saveState, this),
                    maxFilters: VRS.globalOptions.aircraftAutoSelectFiltersLimit,
                    allowableProperties: [
                        VRS.AircraftFilterProperty.Altitude,
                        VRS.AircraftFilterProperty.Distance
                    ],
                    addFilter: (newComparer: AircraftFilterPropertyEnum, paneField: OptionFieldPaneList) => {
                        var aircraftFilter = this.addFilter(newComparer);
                        paneField.addPane(aircraftFilter.createOptionPane(() => this.saveState));
                        this.saveState();
                    },
                    addFilterButtonLabel: 'AddCondition'
                });

                panesTypeSettings.hookPaneRemoved(this.filterPaneRemoved, this);
            }

            pane = new VRS.OptionPane({
                name:           'vrsAircraftAutoSelectPane2',
                displayOrder:   displayOrder + 1
            });
            result.push(pane);

            pane.addField(new VRS.OptionFieldLabel({
                name:           'offRadarActionLabel',
                labelKey:       'OffRadarAction'
            }));
            pane.addField(new VRS.OptionFieldRadioButton({
                name:           'offRadarAction',
                getValue:       this.getOffRadarAction,
                setValue:       this.setOffRadarAction,
                saveState:      this.saveState,
                values: [
                    new VRS.ValueText({ value: VRS.OffRadarAction.WaitForReturn, textKey: 'OffRadarActionWait' }),
                    new VRS.ValueText({ value: VRS.OffRadarAction.EnableAutoSelect, textKey: 'OffRadarActionEnableAutoSelect' }),
                    new VRS.ValueText({ value: VRS.OffRadarAction.Nothing, textKey: 'OffRadarActionNothing' })
                ]
            }));

            return result;
        }

        /**
         * Adds a new filter to the auto-select settings.
         */
        addFilter = (filterOrFilterProperty: AircraftFilter | AircraftFilterPropertyEnum) : AircraftFilter =>
        {
            var filter = filterOrFilterProperty;
            if(!(filter instanceof VRS.AircraftFilter)) {
                filter = <AircraftFilter>VRS.aircraftFilterHelper.createFilter(<string>filterOrFilterProperty);
            }
            this._Filters.push(<AircraftFilter>filter);

            return <AircraftFilter>filter;
        }

        /**
         * Removes an existing filter from the auto-select settings.
         */
        private filterPaneRemoved = (pane: OptionPane, index: number) =>
        {
            this.removeFilterAt(index);
            this.saveState();
        }

        /**
         * Removes an existing filter from the auto-select settings.
         */
        private removeFilterAt = (index: number) =>
        {
            this._Filters.splice(index, 1);
        }

        /**
         * Returns the closest aircraft from the aircraft list passed across.
         */
        closestAircraft = (aircraftList: AircraftList) : Aircraft =>
        {
            var result = null;

            if(this.getEnabled()) {
                var useClosest = this.getSelectClosest();
                var useFilters = this._Filters.length > 0;
                var selectedDistance = useClosest ? Number.MAX_VALUE : Number.MIN_VALUE;
                var self = this;

                aircraftList.foreachAircraft(function(aircraft) {
                    if(aircraft.distanceFromHereKm.val !== undefined) {
                        var isCandidate = useClosest ? aircraft.distanceFromHereKm.val < selectedDistance : aircraft.distanceFromHereKm.val > selectedDistance;
                        if(isCandidate) {
                            if(useFilters && !VRS.aircraftFilterHelper.aircraftPasses(aircraft, self._Filters, {})) isCandidate = false;
                            if(isCandidate) {
                                result = aircraft;
                                selectedDistance = aircraft.distanceFromHereKm.val;
                            }
                        }
                    }
                });
            }

            return result;
        }

        /**
         * Called when the aircraft list is updated.
         */
        private aircraftListUpdated = (newAircraft: AircraftCollection, offRadar: AircraftCollection) =>
        {
            var self = this;
            var selectedAircraft = this._AircraftList.getSelectedAircraft();
            var autoSelectAircraft = this.closestAircraft(this._AircraftList);
            var autoSelectedByIcao = false;

            if(this._SelectAircraftByIcao) {
                var aircraftByIcao = this._AircraftList.findAircraftByIcao(this._SelectAircraftByIcao);
                if(aircraftByIcao) {
                    autoSelectAircraft = aircraftByIcao;
                    autoSelectedByIcao = true;
                }

                if(this._AutoClearSelectAircraftByIcao) {
                    this._SelectAircraftByIcao = null;
                }
            }

            var useAutoSelectedAircraft = function() {
                if(autoSelectAircraft !== selectedAircraft) {
                    self._AircraftList.setSelectedAircraft(autoSelectAircraft, autoSelectedByIcao);
                    selectedAircraft = autoSelectAircraft;
                }
            };

            if(autoSelectAircraft) {
                useAutoSelectedAircraft();
            } else if(selectedAircraft) {
                var isOffRadar = offRadar.findAircraftById(selectedAircraft.id);
                if(isOffRadar) {
                    switch(this.getOffRadarAction()) {
                        case VRS.OffRadarAction.Nothing:
                            break;
                        case VRS.OffRadarAction.EnableAutoSelect:
                            if(!this.getEnabled()) {
                                this.setEnabled(true);
                                autoSelectAircraft = this.closestAircraft(this._AircraftList);
                                if(autoSelectAircraft) useAutoSelectedAircraft();
                            }
                            break;
                        case VRS.OffRadarAction.WaitForReturn:
                            this._AircraftList.setSelectedAircraft(undefined, false);
                            break;
                    }
                }
            }

            if(!selectedAircraft && this._LastAircraftSelected) {
                var reselectAircraft = newAircraft.findAircraftById(this._LastAircraftSelected.id);
                if(reselectAircraft) {
                    this._AircraftList.setSelectedAircraft(reselectAircraft, false);
                }
            }

            if(autoSelectedByIcao) {
                this._Dispatcher.raise(this._Events.aircraftSelectedByIcao);
            }
        }

        /**
         * Called when the aircraft list changes the selected aircraft.
         */
        private selectedAircraftChanged = () =>
        {
            var selectedAircraft = this._AircraftList.getSelectedAircraft();
            if(selectedAircraft) this._LastAircraftSelected = selectedAircraft;

            if(this._AircraftList.getWasAircraftSelectedByUser()) {
                this.setEnabled(false);
            }
        }
    }
} 