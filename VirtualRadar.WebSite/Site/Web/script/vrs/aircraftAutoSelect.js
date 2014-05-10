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

(function(VRS, $, undefined)
{
    //region Global settings
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftAutoSelectEnabled = VRS.globalOptions.aircraftAutoSelectEnabled !== undefined ? VRS.globalOptions.aircraftAutoSelectEnabled : true;   // True if auto-select is enabled by default.
    VRS.globalOptions.aircraftAutoSelectClosest = VRS.globalOptions.aircraftAutoSelectClosest !== undefined ? VRS.globalOptions.aircraftAutoSelectClosest : true;   // True if the closest matching aircraft should be auto-selected, false if the furthest matching aircraft should be auto-selected.
    VRS.globalOptions.aircraftAutoSelectOffRadarAction = VRS.globalOptions.aircraftAutoSelectOffRadarAction || VRS.OffRadarAction.EnableAutoSelect;                 // What to do when an aircraft goes out of range of the radar.
    VRS.globalOptions.aircraftAutoSelectFilters = VRS.globalOptions.aircraftAutoSelectFilters || [];                                                                // The list of filters that auto-select starts off with.
    VRS.globalOptions.aircraftAutoSelectFiltersLimit = VRS.globalOptions.aircraftAutoSelectFiltersLimit !== undefined ? VRS.globalOptions.aircraftAutoSelectFiltersLimit : 2; // The maximum number of auto-select filters that we're going to allow.
    //endregion

    //region AircraftAutoSelect
    /**
     * A class that can auto-select aircraft on each refresh.
     * @param {VRS.AircraftList} aircraftList
     * @param {string=} name
     * @constructor
     */
    VRS.AircraftAutoSelect = function(aircraftList, name)
    {
        //region -- Fields
        var that = this;
        var _AircraftList = aircraftList;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftAutoSelect'
        });
        var _Events = {
            enabledChanged: 'enabledChanged'
        };

        /**
         * @type {VRS.Aircraft}
         * The last aircraft selected by the object.
         */
        var _LastAircraftSelected;
        //endregion

        //region -- Events subscribed, dispose
        var _AircraftListUpdatedHook = _AircraftList.hookUpdated(aircraftListUpdated, this);
        var _SelectedAircraftChangedHook = _AircraftList.hookSelectedAircraftChanged(selectedAircraftChanged, this);

        /**
         * Releases all of the resources held by the object.
         */
        this.dispose = function()
        {
            if(_AircraftListUpdatedHook) _AircraftList.unhook(_AircraftListUpdatedHook);
            if(_SelectedAircraftChangedHook) _AircraftList.unhook(_SelectedAircraftChangedHook);
            _AircraftListUpdatedHook = _SelectedAircraftChangedHook = null;
        };
        //endregion

        //region -- Properties
        var _Name = name || 'default';
        /**
         * Gets the name of the object. This is used when storing and loading settings.
         * @returns {string}
         */
        this.getName = function() { return _Name; };

        /**
         * Determines whether the object is actively setting selected aircraft.
         * @type {bool}
         * @private
         */
        var _Enabled = VRS.globalOptions.aircraftAutoSelectEnabled;
        /**
         * Gets a value indicating whether the object is actively setting the selected aircraft.
         * @returns {bool}
         */
        this.getEnabled = function() { return _Enabled; };
        /**
         * Sets a value indicating whether the object is to auto-select aircraft or not.
         * @param {bool} value
         */
        this.setEnabled = function(value) {
            if(_Enabled !== value) {
                _Enabled = value;
                _Dispatcher.raise(_Events.enabledChanged);

                if(_Enabled && _AircraftList) {
                    var closestAircraft = that.closestAircraft(_AircraftList);
                    if(closestAircraft && closestAircraft !== _AircraftList.getSelectedAircraft()) {
                        _AircraftList.setSelectedAircraft(closestAircraft, false);
                    }
                }
            }
        };

        /**
         * True if the closest aircraft is to be selected, false otherwise.
         * @type {bool}
         * @private
         */
        var _SelectClosest = VRS.globalOptions.aircraftAutoSelectClosest;
        /**
         * Gets a value indicating that the closest aircraft is to be selected.
         * @returns {boolean}
         */
        this.getSelectClosest = function() { return _SelectClosest; };
        /**
         * Sets a value indicating that the closest aircraft is to be selected.
         * @param value
         */
        this.setSelectClosest = function(value) { _SelectClosest = value; };

        /** @type {VRS.OffRadarAction} @private */
        var _OffRadarAction = VRS.globalOptions.aircraftAutoSelectOffRadarAction;
        /** Gets a value that describes what to do when an aircraft goes off the radar.
         * @type {VRS.OffRadarAction}
         */
        this.getOffRadarAction = function() { return _OffRadarAction; };
        this.setOffRadarAction = function(/** VRS.OffRadarAction */ value)  { _OffRadarAction = value; };

        /**
         * An array of the different aircraft filters to use when determining which aircraft to auto-select.
         * @type {VRS.AircraftFilter[]}
         * @private
         */
        var _Filters = VRS.globalOptions.aircraftAutoSelectFilters;
        //noinspection JSUnusedGlobalSymbols
        /**
         * Returns a clone of the filter at the specified index.
         * @param {number} index
         * @returns {VRS.AircraftFilter}
         */
        this.getFilter = function(index) { return /** @type {VRS.AircraftFilter} */ _Filters[index].clone(); };
        //noinspection JSUnusedGlobalSymbols
        /**
         * Sets the property filter at the specified index.
         * @param {number}              index
         * @param {VRS.AircraftFilter}  aircraftFilter
         */
        this.setFilter = function(index, aircraftFilter) {
            if(_Filters.length < VRS.globalOptions.aircraftListFiltersLimit) {
                var existing = index < _Filters.length ? _Filters[index] : null;
                if(existing && !existing.equals(aircraftFilter)) _Filters[index] = aircraftFilter;
            }
        };
        //endregion

        //region -- Events exposed
        /**
         * Raised after the Enabled property value has changed.
         * @param {function} callback
         * @param {object} forceThis
         * @returns {object}
         */
        this.hookEnabledChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.enabledChanged, callback, forceThis); };

        /**
         * Unhooks an event hooked on this object.
         * @param {object} hookResult
         * @returns {*}
         */
        this.unhook = function(hookResult) { return _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- State persistence - saveState, loadState, applyState, loadAndApplyState
        /**
         * Saves the current state of the object.
         */
        this.saveState = function()
        {
            VRS.configStorage.save(persistenceKey(), createSettings());
        };

        /**
         * Returns the saved state of the object.
         * @returns {VRS_STATE_AIRCRAFTAUTOSELECT}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            var result = $.extend(createSettings(), savedSettings);
            result.filters = VRS.aircraftFilterHelper.buildValidFiltersList(result.filters, VRS.globalOptions.aircraftAutoSelectFiltersLimit);

            return result;
        };

        /**
         * Applies a saved state to the object.
         * @param {VRS_STATE_AIRCRAFTAUTOSELECT} settings
         */
        this.applyState = function(settings)
        {
            that.setEnabled(settings.enabled);
            that.setSelectClosest(settings.selectClosest);
            that.setOffRadarAction(settings.offRadarAction);

            _Filters = [];
            $.each(settings.filters, function(idx, serialisedFilter) {
                var aircraftFilter = VRS.aircraftFilterHelper.createFilter(serialisedFilter.property);
                aircraftFilter.applySerialisedObject(serialisedFilter);
                _Filters.push(aircraftFilter);
            });
        };

        /**
         * Loads and applies the saved state to the object.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key under which the object's settings are saved.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'vrsAircraftAutoSelect-' + _Name;
        }

        /**
         * Creates the state object.
         * @returns {VRS_STATE_AIRCRAFTAUTOSELECT}
         */
        function createSettings()
        {
            var filters = VRS.aircraftFilterHelper.serialiseFiltersList(_Filters);
            return {
                enabled: _Enabled,
                selectClosest: _SelectClosest,
                offRadarAction: _OffRadarAction,
                filters: filters
            };
        }
        //endregion

        //region -- createOptionPane
        /**
         * Creates the option pane that allows configuration of the object.
         * @param {number} displayOrder
         * @returns {VRS.OptionPane[]}
         */
        this.createOptionPane = function(displayOrder)
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
                getValue:       that.getEnabled,
                setValue:       that.setEnabled,
                saveState:      that.saveState,
                hookChanged:    that.hookEnabledChanged,
                unhook:         that.unhook
            }));

            pane.addField(new VRS.OptionFieldRadioButton({
                name:           'closest',
                labelKey:       'Select',
                getValue:       that.getSelectClosest,
                setValue:       that.setSelectClosest,
                saveState:      that.saveState,
                values: [
                    new VRS.ValueText({ value: true, textKey: 'ClosestToCurrentLocation' }),
                    new VRS.ValueText({ value: false, textKey: 'FurthestFromCurrentLocation' })
                ]
            }));

            if(VRS.aircraftListFiltersLimit !== 0) {
                var panesTypeSettings = VRS.aircraftFilterHelper.addConfigureFiltersListToPane({
                    pane:       pane,
                    filters:    _Filters,
                    saveState:  function() { that.saveState(); },
                    maxFilters: VRS.globalOptions.aircraftAutoSelectFiltersLimit,
                    allowableProperties: [
                        VRS.AircraftFilterProperty.Altitude,
                        VRS.AircraftFilterProperty.Distance
                    ],
                    addFilter: function(newComparer, paneField) {
                        var aircraftFilter = that.addFilter(newComparer);
                        paneField.addPane(aircraftFilter.createOptionPane(that.saveState));
                        that.saveState();
                    },
                    addFilterButtonLabel: 'AddCondition'
                });

                panesTypeSettings.hookPaneRemoved(filterPaneRemoved, this);
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
                getValue:       that.getOffRadarAction,
                setValue:       that.setOffRadarAction,
                saveState:      that.saveState,
                values: [
                    new VRS.ValueText({ value: VRS.OffRadarAction.WaitForReturn, textKey: 'OffRadarActionWait' }),
                    new VRS.ValueText({ value: VRS.OffRadarAction.EnableAutoSelect, textKey: 'OffRadarActionEnableAutoSelect' }),
                    new VRS.ValueText({ value: VRS.OffRadarAction.Nothing, textKey: 'OffRadarActionNothing' })
                ]
            }));

            return result;
        };
        //endregion

        //region --addFilter, comparerPaneRemoved, removeFilterAt
        /**
         * Adds a new filter to the auto-select settings.
         * @param {VRS.AircraftFilter|VRS.AircraftFilterProperty} filterOrFilterProperty
         * @returns {VRS.AircraftFilter}
         */
        this.addFilter = function(filterOrFilterProperty)
        {
            var filter = filterOrFilterProperty;
            if(!(filter instanceof VRS.AircraftFilter)) {
                filter = VRS.aircraftFilterHelper.createFilter(filterOrFilterProperty);
            }
            _Filters.push(filter);

            return filter;
        };

        //noinspection JSUnusedLocalSymbols
        /**
         * Removes an existing filter from the auto-select settings.
         * @param {VRS.OptionPane} [pane] Unused.
         * @param {number} index
         */
        function filterPaneRemoved(pane, index)
        {
            that.removeFilterAt(index);
            that.saveState();
        }

        /**
         * Removes an existing filter from the auto-select settings.
         * @param {number} index
         */
        this.removeFilterAt = function(index)
        {
            _Filters.splice(index, 1);
        };
        //endregion

        //region -- closestAircraft, aircraftListUpdated, selectedAircraftChanged
        /**
         * Returns the closest aircraft from the aircraft list passed across.
         * @param {VRS.AircraftList} aircraftList
         * @returns {VRS.Aircraft}
         */
        this.closestAircraft = function(aircraftList)
        {
            var result = null;

            if(this.getEnabled()) {
                var useClosest = this.getSelectClosest();
                var useFilters = _Filters.length > 0;
                var selectedDistance = useClosest ? Number.MAX_VALUE : Number.MIN_VALUE;
                aircraftList.foreachAircraft(function(aircraft) {
                    if(aircraft.distanceFromHereKm.val !== undefined) {
                        var isCandidate = useClosest ? aircraft.distanceFromHereKm.val < selectedDistance : aircraft.distanceFromHereKm.val > selectedDistance;
                        if(isCandidate) {
                            if(useFilters && !VRS.aircraftFilterHelper.aircraftPasses(aircraft, _Filters, {})) isCandidate = false;
                            if(isCandidate) {
                                result = aircraft;
                                selectedDistance = aircraft.distanceFromHereKm.val;
                            }
                        }
                    }
                });
            }

            return result;
        };

        //noinspection JSUnusedLocalSymbols
        /**
         * Called when the aircraft list is updated.
         * @param {VRS.AircraftCollection} newAircraft
         * @param {VRS.AircraftCollection} offRadar
         */
        function aircraftListUpdated(newAircraft, offRadar)
        {
            var selectedAircraft = _AircraftList.getSelectedAircraft();
            var autoSelectAircraft = that.closestAircraft(_AircraftList);

            var useAutoSelectedAircraft = function() {
                if(autoSelectAircraft !== selectedAircraft) {
                    _AircraftList.setSelectedAircraft(autoSelectAircraft, false);
                    selectedAircraft = autoSelectAircraft;
                }
            };

            if(autoSelectAircraft) {
                useAutoSelectedAircraft();
            } else if(selectedAircraft) {
                var isOffRadar = offRadar.findAircraftById(selectedAircraft.id);
                if(isOffRadar) {
                    switch(that.getOffRadarAction()) {
                        case VRS.OffRadarAction.Nothing:
                            break;
                        case VRS.OffRadarAction.EnableAutoSelect:
                            if(!that.getEnabled()) {
                                that.setEnabled(true);
                                autoSelectAircraft = that.closestAircraft(_AircraftList);
                                if(autoSelectAircraft) useAutoSelectedAircraft();
                            }
                            break;
                        case VRS.OffRadarAction.WaitForReturn:
                            _AircraftList.setSelectedAircraft(undefined, false);
                            break;
                    }
                }
            }

            if(!selectedAircraft && _LastAircraftSelected) {
                var reselectAircraft = newAircraft.findAircraftById(_LastAircraftSelected.id);
                if(reselectAircraft) _AircraftList.setSelectedAircraft(reselectAircraft, false);
            }
        }

        /**
         * Called when the aircraft list changes the selected aircraft.
         //* @param {VRS.Aircraft} oldSelectedAircraft
         */
        function selectedAircraftChanged()
        {
            var selectedAircraft = _AircraftList.getSelectedAircraft();
            if(selectedAircraft) _LastAircraftSelected = selectedAircraft;

            if(_AircraftList.getWasAircraftSelectedByUser()) that.setEnabled(false);
        }
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));