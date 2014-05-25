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
 * @fileoverview Code that can handle the sorting of an aircraft list.
 */

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};

    VRS.globalOptions.aircraftListDefaultSortOrder = VRS.aircraftListDefaultSortOrder || [ // The default set of columns to sort by. The length of this array also determines the number of fields to allow the user to sort by.
        { field: VRS.AircraftListSortableField.TimeTracked, ascending: true },
        { field: VRS.AircraftListSortableField.None,        ascending: true },
        { field: VRS.AircraftListSortableField.None,        ascending: true }
    ];
    //endregion

    //region AircraftListSortHandler
    /**
     * The class that can determine the relative sort order of two aircraft based on the content of a single property on the aircraft.
     * @param {Object}                                       settings
     * @param {VRS.AircraftListSortableField}                settings.field              The VRS.AircraftListSortableField field that this handler is dealing with.
     * @param {string}                                       settings.labelKey           The VRS.$$ entry to use for UI labels.
     * @param {function(VRS.Aircraft):number}               [settings.getNumberCallback] A function that returns a numeric value from an aircraft. If supplied then the sort order is based on the returned value.
     * @param {function(VRS.Aircraft):string}               [settings.getStringCallback] A function that returns a string value from an aircraft. If supplied then the sort order is based on the returned string.
     * @param {function(VRS.Aircraft, VRS.Aircraft):number} [settings.compareCallback]   A function that takes two aircraft and returns an integer describing their relative sort order.
     * @constructor
     */
    VRS.AircraftListSortHandler = function(settings)
    {
        if(!settings) throw 'You must supply a settings object';
        if(!settings.field) throw 'You must supply a field field';
        if(!settings.labelKey) throw 'You must supply a labelKey field';
        if(!settings.compareCallback && !settings.getNumberCallback && !settings.getStringCallback) throw 'You must supply a compareCallback, getNumberCallback or getStringCallback';

        var that = this;
        this.Field = settings.field;
        this.LabelKey = settings.labelKey;
        this.GetNumberCallback = settings.getNumberCallback;
        this.GetStringCallback = settings.getStringCallback;
        this.CompareCallback = settings.compareCallback;

        if(!this.CompareCallback) this.CompareCallback = this.GetNumberCallback ? compareNumericValues : compareStringValues;

        /**
         * Returns the relative sort order of two aircraft based on a numeric value held by each.
         * @param {VRS.Aircraft} lhs
         * @param {VRS.Aircraft} rhs
         * @returns {number}
         */
        function compareNumericValues(lhs, rhs)
        {
            var lhsValue = that.GetNumberCallback(lhs);
            var rhsValue = that.GetNumberCallback(rhs);
            if(!lhsValue && lhsValue !== 0) return rhsValue === undefined ? 0 : -1;
            if(!rhsValue && rhsValue !== 0) return 1;
            return lhsValue - rhsValue;
        }

        /**
         * Returns the relative sort order of two aircraft based on a string value held by each.
         * @param {VRS.Aircraft} lhs
         * @param {VRS.Aircraft} rhs
         * @returns {number}
         */
        function compareStringValues(lhs, rhs)
        {
            // WebStorm 7 seems to think that localeCompare returns a bool. It does not, it returns a number.
            //noinspection JSValidateTypes
            return (that.GetStringCallback(lhs) || '').localeCompare(that.GetStringCallback(rhs) || '');
        }
    };
    //endregion

    //region VRS.aircraftListSortHandlers - Pre-built VRS.AircraftListSortHandler objects
    /**
     * A collection of VRS.AircraftListSortHandler objects indexed by VRS.AircraftListSortableField values.
     * @type {Object.<VRS.AircraftListSortableField, VRS.AircraftListSortHandler>}
     */
    VRS.aircraftListSortHandlers = VRS.aircraftListSortHandlers || {};

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Altitude] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Altitude,
        labelKey:           'Altitude',
        getNumberCallback:  function(aircraft) { return aircraft.altitude.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.AltitudeType] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.AltitudeType,
        labelKey:           'AltitudeType',
        getStringCallback:  function(aircraft) { return aircraft.formatAltitudeType(); }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.AverageSignalLevel] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.AverageSignalLevel,
        labelKey:           'AverageSignalLevel',
        getNumberCallback:  function(aircraft) { return aircraft.averageSignalLevel.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Bearing] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Bearing,
        labelKey:           'Bearing',
        getNumberCallback:  function(aircraft) { return aircraft.Bearing.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Callsign] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Callsign,
        labelKey:           'Callsign',
        getStringCallback:  function(aircraft) { return aircraft.callsign.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.CivOrMil] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.CivOrMil,
        labelKey:           'CivilOrMilitary',
        getNumberCallback:  function(aircraft) { return aircraft.isMilitary.val ? 1 : 0; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.CountMessages] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.CountMessages,
        labelKey:           'MessageCount',
        getNumberCallback:  function(aircraft) { return aircraft.countMessages.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Country] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Country,
        labelKey:           'Country',
        getStringCallback:  function(aircraft) { return aircraft.country.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Distance] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Distance,
        labelKey:           'Distance',
        getNumberCallback:  function(aircraft) { return aircraft.distanceFromHereKm.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.FlightsCount] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.FlightsCount,
        labelKey:           'FlightsCount',
        getNumberCallback:  function(aircraft) { return aircraft.countFlights.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Heading] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Heading,
        labelKey:           'Heading',
        getNumberCallback:  function(aircraft) { return aircraft.heading.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Icao] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Icao,
        labelKey:           'Icao',
        getStringCallback:  function(aircraft) { return aircraft.icao.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Latitude] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Latitude,
        labelKey:           'Latitude',
        getNumberCallback:  function(aircraft) { return aircraft.latitude.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Longitude] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Longitude,
        labelKey:           'Longitude',
        getNumberCallback:  function(aircraft) { return aircraft.longitude.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Model] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Model,
        labelKey:           'Model',
        getStringCallback:  function(aircraft) { return aircraft.model.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.ModelIcao] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.ModelIcao,
        labelKey:           'ModelIcao',
        getStringCallback:  function(aircraft) { return aircraft.modelIcao.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.None] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.None,
        labelKey:           'None',
        compareCallback:    function() { throw 'If this gets called then it\'s a bug'; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Operator] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Operator,
        labelKey:           'Operator',
        getStringCallback:  function(aircraft) { return aircraft.operator.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.OperatorIcao] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.OperatorIcao,
        labelKey:           'OperatorCode',
        getStringCallback:  function(aircraft) { return aircraft.operatorIcao.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Receiver] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Receiver,
        labelKey:           'Receiver',
        getNumberCallback:  function(aircraft) { return aircraft.receiverId.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Registration] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Registration,
        labelKey:           'Registration',
        getStringCallback:  function(aircraft) { return aircraft.registration.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.SignalLevel] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.SignalLevel,
        labelKey:           'SignalLevel',
        getNumberCallback:  function(aircraft) { return aircraft.signalLevel.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Speed] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Speed,
        labelKey:           'Speed',
        getNumberCallback:  function(aircraft) { return aircraft.speed.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Squawk] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Squawk,
        labelKey:           'Squawk',
        getStringCallback:  function(aircraft) { return aircraft.squawk.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TimeTracked] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.TimeTracked,
        labelKey:           'TimeTracked',
        getNumberCallback:  function(aircraft) { return aircraft.secondsTracked; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.UserTag] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.UserTag,
        labelKey:           'UserTag',
        getStringCallback:  function(aircraft) { return aircraft.userTag.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.VerticalSpeed] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.VerticalSpeed,
        labelKey:           'VerticalSpeed',
        getNumberCallback:  function(aircraft) { return aircraft.verticalSpeed.val; }
    });
    //endregion

    //region AircraftListSorter
    /**
     * A class that can sort an aircraft list in a configurable order.
     * @param {string=} name The name to use when saving the object's state.
     * @constructor
     */
    VRS.AircraftListSorter = function(name)
    {
        //region -- Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftListSorter'
        });
        var _Events = {
            sortFieldsChanged: 'sortFieldsChanged'
        };
        //endregion

        //region -- Properties
        var _Name = name || 'default';
        this.getName = function() { return _Name; };

        /**
         * The VRS.AircraftListSortableField fields that the user is sorting by.
         * @type {VRS_SORTFIELD[]}
         * @private
         */
        var _SortFields = VRS.globalOptions.aircraftListDefaultSortOrder.slice();
        /** @type {Number} @private */
        var _SortFieldsLength = _SortFields.length;
        /**
         * Gets the number of sort fields available. This never changes over the lifetime of the site.
         * @returns {Number}
         */
        this.getSortFieldsCount = function() { return _SortFieldsLength; };
        /**
         * Gets the sortable field description at the index passed across.
         * @param {number} index
         * @returns {VRS_SORTFIELD}
         */
        this.getSortField = function(index) {
            var existing = _SortFields[index];
            return { field: existing.field, ascending: existing.ascending };
        };
        /**
         * Sets the sortable field description for the index passed across.
         * @param {number} index
         * @param {VRS_SORTFIELD} sortField
         */
        this.setSortField = function(index, sortField) {
            var existing = _SortFields[index];
            if(existing.field !== sortField.field || existing.ascending !== sortField.ascending) {
                _SortFields[index] = sortField;
                _Dispatcher.raise(_Events.sortFieldsChanged);
            }
        };

        /**
         * If there is only one sort field in use by the sorter then this method returns it. If there are no sort fields
         * in use, or it is sorting on more than one field, then it returns null.
         * @returns {VRS_SORTFIELD}
         */
        this.getSingleSortField = function()
        {
            /** @type {VRS_SORTFIELD} */ var result = null;

            var length = _SortFields.length;
            for(var i = 0;i < length;++i) {
                var sortField = _SortFields[i];
                if(sortField.field !== VRS.AircraftListSortableField.None) {
                    if(result !== null) {
                        result = null;
                        break;
                    }
                    result = that.getSortField(i);
                }
            }

            return result;
        };

        /**
         * Configures the sorter to only use a single sort field.
         * @param {VRS_SORTFIELD} sortField
         */
        this.setSingleSortField = function(sortField)
        {
            var raiseEvent = false;

            var empty = { field: VRS.AircraftListSortableField.None, ascending: false };
            var length = _SortFields.length;
            for(var i = 0;i < length;++i) {
                var existing = _SortFields[i];
                var replaceWith = i == 0 ? sortField : empty;
                if(!raiseEvent) raiseEvent = existing.field !== replaceWith.field || existing.ascending !== replaceWith.ascending;
                existing.field = replaceWith.field;
                existing.ascending = replaceWith.ascending;
            }

            if(raiseEvent) _Dispatcher.raise(_Events.sortFieldsChanged);
        };
        //endregion

        //region -- Events
        this.hookSortFieldsChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.sortFieldsChanged, callback, forceThis); };

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
         * Returns the previously saved state or the current state if the state has never been saved.
         * @returns {VRS_STATE_AIRCRAFTLISTSORTER}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.load(persistenceKey(), {});
            var result = $.extend(createSettings(), savedSettings);

            VRS.arrayHelper.normaliseOptionsArray(VRS.globalOptions.aircraftListDefaultSortOrder, result.sortFields, function(entry) {
                return entry.ascending !== undefined && (VRS.aircraftListSortHandlers[entry.field] ? true : false);
            });

            return result;
        };

        /**
         * Applies the previously saved state to this object.
         * @param {VRS_STATE_AIRCRAFTLISTSORTER} settings
         */
        this.applyState = function(settings)
        {
            _SortFields = [];
            $.each(settings.sortFields, function(idx, sortField) { _SortFields.push(sortField); });
            _Dispatcher.raise(_Events.sortFieldsChanged);
        };

        /**
         * Loads and then applies the previously saved state.
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
            return 'vrsAircraftSorter-' + _Name;
        }

        /**
         * Creates the object that describes the persistent state.
         * @returns {VRS_STATE_AIRCRAFTLISTSORTER}
         */
        function createSettings()
        {
            return {
                sortFields: _SortFields
            };
        }
        //endregion

        //region -- createOptionPane
        /**
         * Returns the description of the configuration pane for this object.
         * @param {number} displayOrder
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder)
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAircraftSorter_' + _Name,
                titleKey:       'PaneSortAircraftList',
                displayOrder:   displayOrder
            });

            var handlers = [];
            var handler;
            for(var fieldId in VRS.aircraftListSortHandlers) {
                //noinspection JSUnfilteredForInLoop
                handler = VRS.aircraftListSortHandlers[fieldId];
                if(handler instanceof VRS.AircraftListSortHandler) handlers.push(handler);
            }
            handlers.sort(function(lhs, rhs) {
                if(lhs.Field === VRS.AircraftListSortableField.None) return rhs.Field === VRS.AircraftListSortableField.None ? 0 : -1;
                if(rhs.Field === VRS.AircraftListSortableField.None) return 1;
                var lhsText = VRS.globalisation.getText(lhs.LabelKey) || '';
                var rhsText = VRS.globalisation.getText(rhs.LabelKey) || '';
                return lhsText.localeCompare(rhsText);
            });

            var values = [];
            var countHandlers = handlers.length;
            for(var i = 0;i < countHandlers;++i) {
                handler = handlers[i];
                values.push(new VRS.ValueText({ value: handler.Field, textKey: handler.LabelKey }));
            }

            var self = this;
            $.each(_SortFields, function(idx) {
                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'sortBy' + idx.toString(),
                    labelKey:       idx === 0 ? 'SortBy' : 'ThenBy',
                    getValue:       function() { return self.getSortField(idx).field; },
                    setValue:       function(value) { var setEntry = self.getSortField(idx); setEntry.field = value; self.setSortField(idx, setEntry); },
                    saveState:      self.saveState,
                    values:         values,
                    keepWithNext:   true
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'ascending' + idx.toString(),
                    labelKey:       'Ascending',
                    getValue:       function() { return self.getSortField(idx).ascending; },
                    setValue:       function(value) { var setEntry = self.getSortField(idx); setEntry.ascending = value; self.setSortField(idx, setEntry); },
                    saveState:      self.saveState
                }));
            });

            return pane;
        };
        //endregion

        //region -- sortAircraftArray
        /**
         * Changes the order of the elements in the array passed across so that the aircraft follow the configured sort order.
         * @param {VRS.Aircraft[]} array
         */
        this.sortAircraftArray = function(array)
        {
            var i = 0;
            var handlers = [];
            for(i = 0;i < _SortFieldsLength;++i) {
                var sortField = _SortFields[i];
                if(sortField.field !== VRS.AircraftListSortableField.None) {
                    handlers.push({ handler: VRS.aircraftListSortHandlers[sortField.field], ascending: sortField.ascending });
                }
            }

            var length = handlers.length;
            if(length > 0) {
                array.sort(function(lhs, rhs) {
                    for(i = 0;i < length;++i) {
                        var handler = handlers[i];
                        var result = handler.handler.CompareCallback(lhs, rhs);
                        if(result != 0) {
                            if(!handler.ascending) result = -result;
                            break;
                        }
                    }
                    return result;
                });
            }
        };
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));