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

namespace VRS
{
    /*
     * Global options
     */
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListDefaultSortOrder = VRS.globalOptions.aircraftListDefaultSortOrder || [ // The default set of columns to sort by. The length of this array also determines the number of fields to allow the user to sort by.
        { field: VRS.AircraftListSortableField.TimeTracked, ascending: true },
        { field: VRS.AircraftListSortableField.None,        ascending: true },
        { field: VRS.AircraftListSortableField.None,        ascending: true }
    ];
    VRS.globalOptions.aircraftListShowEmergencySquawks = VRS.globalOptions.aircraftListShowEmergencySquawks !== undefined ? VRS.globalOptions.aircraftListShowEmergencySquawks : VRS.SortSpecial.First; // Whether to force aircraft reporting an emergency squawk to the start or end of the list
    VRS.globalOptions.aircraftListShowInteresting = VRS.globalOptions.aircraftListShowInteresting !== undefined ? VRS.globalOptions.aircraftListShowInteresting : VRS.SortSpecial.First;                // Whether to force aircraft flagged as interesting to the start or end of the list

    /**
     * The settings to pass when creating a new instance of AircraftListSortHandler.
     */
    export interface AircraftListSortHandler_Settings
    {
        /**
         * The VRS.AircraftListSortableField field that this handler is dealing with.
         */
        field?: AircraftListSortableFieldEnum;

        /**
         * The VRS.$$ entry to use for UI labels.
         */
        labelKey?: string;

        /**
         * A function that returns a numeric value from an aircraft. If supplied then the sort order is based on the returned value.
         */
        getNumberCallback?: (aircraft: Aircraft) => number;

        /**
         * A function that returns a string value from an aircraft. If supplied then the sort order is based on the returned string.
         */
        getStringCallback?: (aircraft: Aircraft) => string;

        /**
         * A function that takes two aircraft and returns an integer describing their relative sort order.
         */
        compareCallback?: (lhs: Aircraft, rhs: Aircraft) => number;
    }

    /**
     * A class that can determine the relative sort order of two aircraft based on the content of a single property on the aircraft.
     */
    export class AircraftListSortHandler
    {
        private _Settings: AircraftListSortHandler_Settings;

        constructor(settings: AircraftListSortHandler_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(!settings.field && !settings.compareCallback) throw 'You must supply a field field';
            if(!settings.labelKey && !settings.compareCallback) throw 'You must supply a labelKey field';
            if(!settings.compareCallback && !settings.getNumberCallback && !settings.getStringCallback) throw 'You must supply a compareCallback, getNumberCallback or getStringCallback';

            if(!settings.compareCallback) {
                settings.compareCallback = settings.getNumberCallback ? this.compareNumericValues : this.compareStringValues;
            }

            this._Settings = settings;
        }

        get Field()
        {
            return this._Settings.field;
        }

        get LabelKey()
        {
            return this._Settings.labelKey;
        }

        get GetNumberCallback()
        {
            return this._Settings.getNumberCallback;
        }

        get GetStringCallback()
        {
            return this._Settings.getStringCallback;
        }

        get CompareCallback()
        {
            return this._Settings.compareCallback;
        }

        /**
         * Returns the relative sort order of two aircraft based on a numeric value held by each.
         */
        private compareNumericValues(lhs: Aircraft, rhs: Aircraft) : number
        {
            var lhsValue = this.GetNumberCallback(lhs);
            var rhsValue = this.GetNumberCallback(rhs);
            if(!lhsValue && lhsValue !== 0) return rhsValue === undefined ? 0 : -1;
            if(!rhsValue && rhsValue !== 0) return 1;
            return lhsValue - rhsValue;
        }

        /**
         * Returns the relative sort order of two aircraft based on a string value held by each.
         */
        private compareStringValues(lhs: Aircraft, rhs: Aircraft) : number
        {
            return (this.GetStringCallback(lhs) || '').localeCompare(this.GetStringCallback(rhs) || '');
        }
    }

    /**
     * A collection of VRS.AircraftListSortHandler objects indexed by VRS.AircraftListSortableField values.
     */
    export var aircraftListSortHandlers: { [index: string/* AircraftListSortableFieldEnum */]:AircraftListSortHandler } = VRS.aircraftListSortHandlers || {};

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
        getNumberCallback:  function(aircraft) { return aircraft.bearingFromHere.val; }
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

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.HeadingType] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.HeadingType,
        labelKey:           'HeadingType',
        getStringCallback:  function(aircraft) { return aircraft.formatHeadingType(); }
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

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Manufacturer] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Manufacturer,
        labelKey:           'Manufacturer',
        getStringCallback:  function(aircraft) { return aircraft.manufacturer.val; }
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
        compareCallback:    function(aircraft) : number { throw 'If this gets called then it\'s a bug'; }
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

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Serial] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Serial,
        labelKey:           'SerialNumber',
        getStringCallback:  function(aircraft) { return aircraft.serial.val; }
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

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.SpeedType] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.SpeedType,
        labelKey:           'SpeedType',
        getStringCallback:  function(aircraft) { return aircraft.formatSpeedType(); }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Squawk] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.Squawk,
        labelKey:           'Squawk',
        getStringCallback:  function(aircraft) { return aircraft.squawk.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TargetAltitude] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.TargetAltitude,
        labelKey:           'TargetAltitude',
        getNumberCallback:  function(aircraft) { return aircraft.targetAltitude.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TargetHeading] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.TargetHeading,
        labelKey:           'TargetHeading',
        getNumberCallback:  function(aircraft) { return aircraft.targetHeading.val; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TimeTracked] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.TimeTracked,
        labelKey:           'TimeTracked',
        getNumberCallback:  function(aircraft) { return aircraft.secondsTracked; }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TransponderType] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.TransponderType,
        labelKey:           'TransponderType',
        getNumberCallback:  function(aircraft) { return aircraft.transponderType.val; }
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

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.VerticalSpeedType] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.VerticalSpeedType,
        labelKey:           'VerticalSpeedType',
        getStringCallback:  function(aircraft) { return aircraft.formatVerticalSpeedType(); }
    });

    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.YearBuilt] = new VRS.AircraftListSortHandler({
        field:              VRS.AircraftListSortableField.YearBuilt,
        labelKey:           'YearBuilt',
        getStringCallback:  function(aircraft) { return aircraft.yearBuilt.val; }
    });

    type AircraftListSpecialHandlerIndexEnum = number;
    /**
     * An enumeration of the indexes used for the list of special handlers.
     */
    export var AircraftListSpecialHandlerIndex = {
        Emergency:     0,
        Interesting:   1
    };

    /**
     * A collection of special handlers that deal with forcing emergency / interesting etc. aircraft to the
     * start or the end of the list, as determined by the options.
     */
    export var aircraftListSpecialHandlers: AircraftListSortHandler[] = [
        // Make sure that these are inserted in the same order as the VRS.AircraftListSpecialHandlerIndex enum

        // 0: Emergency First
        new VRS.AircraftListSortHandler({
            compareCallback: function(lhs: Aircraft, rhs: Aircraft) {
                return !!lhs.isEmergency.val === !!rhs.isEmergency.val ? 0 : !!lhs.isEmergency.val ? -1 : 1;
            }
        }),

        // 1: Interesting First
        new VRS.AircraftListSortHandler({
            compareCallback: function(lhs: Aircraft, rhs: Aircraft) {
                return !!lhs.userInterested.val === !!rhs.userInterested.val ? 0 : !!lhs.userInterested.val ? -1 : 1;
            }
        })
    ];

    /**
     * Describes a sortable field.
     */
    export interface AircraftListSorter_SortField
    {
        field:      AircraftListSortableFieldEnum;
        ascending:  boolean;
    }

    /**
     * The state object saved when AircraftListSorter persists itself.
     */
    export interface AircraftListSorter_SaveState
    {
        sortFields:             AircraftListSorter_SortField[];
        showEmergencySquawks:   SortSpecialEnum;
        showInteresting:        SortSpecialEnum;
    }

    /**
     * A class that can sort an aircraft list in a configurable order.
     */
    export class AircraftListSorter implements ISelfPersist<AircraftListSorter_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.AircraftListSorter'
        });
        private _Events = {
            sortFieldsChanged: 'sortFieldsChanged'
        };

        private _Name: string;
        private _SortFields: AircraftListSorter_SortField[];        // The VRS.AircraftListSortableField fields that the user is sorting by.
        private _SortFieldsLength: number;                          // Gets the number of sort fields available. This never changes over the lifetime of the site.
        private _ShowEmergencySquawksSortSpecial: SortSpecialEnum;  // The special sort order for emergency aircraft.
        private _ShowInterestingSortSpecial: SortSpecialEnum;       // The special sort order for interesting aircraft.

        constructor(name?: string)
        {
            this._Name = name || 'default';
            this._SortFields = VRS.globalOptions.aircraftListDefaultSortOrder.slice();
            this._SortFieldsLength = this._SortFields.length;
            this._ShowEmergencySquawksSortSpecial = VRS.globalOptions.aircraftListShowEmergencySquawks;
            this._ShowInterestingSortSpecial = VRS.globalOptions.aircraftListShowInteresting;
        }

        getName = () : string =>
        {
            return this._Name;
        }

        getSortFieldsCount = () : number =>
        {
            return this._SortFieldsLength;
        }

        /**
         * Gets the sortable field description at the index passed across.
         */
        getSortField = (index: number) : AircraftListSorter_SortField =>
        {
            var existing = this._SortFields[index];
            return { field: existing.field, ascending: existing.ascending };
        }
        setSortField = (index: number, sortField: AircraftListSorter_SortField) =>
        {
            var existing = this._SortFields[index];
            if(existing.field !== sortField.field || existing.ascending !== sortField.ascending) {
                this._SortFields[index] = sortField;
                this._Dispatcher.raise(this._Events.sortFieldsChanged);
            }
        }

        /**
         * If there is only one sort field in use by the sorter then this method returns it. If there are no sort fields
         * in use, or it is sorting on more than one field, then it returns null.
         */
        getSingleSortField = () : AircraftListSorter_SortField =>
        {
            var result: AircraftListSorter_SortField = null;

            var length = this._SortFields.length;
            for(var i = 0;i < length;++i) {
                var sortField = this._SortFields[i];
                if(sortField.field !== VRS.AircraftListSortableField.None) {
                    if(result !== null) {
                        result = null;
                        break;
                    }
                    result = this.getSortField(i);
                }
            }

            return result;
        }
        setSingleSortField = (sortField: AircraftListSorter_SortField) =>
        {
            var raiseEvent = false;

            var empty = { field: VRS.AircraftListSortableField.None, ascending: false };
            var length = this._SortFields.length;
            for(var i = 0;i < length;++i) {
                var existing = this._SortFields[i];
                var replaceWith = i == 0 ? sortField : empty;
                if(!raiseEvent) {
                    raiseEvent = existing.field !== replaceWith.field || existing.ascending !== replaceWith.ascending;
                }
                existing.field = replaceWith.field;
                existing.ascending = replaceWith.ascending;
            }

            if(raiseEvent) {
                this._Dispatcher.raise(this._Events.sortFieldsChanged);
            }
        }

        /**
         * Returns a value indicating how emergency squawks should be treated in the sort.
         */
        getShowEmergencySquawksSortSpecial = () : SortSpecialEnum =>
        {
            return this._ShowEmergencySquawksSortSpecial;
        }
        setShowEmergencySquawksSortSpecial = (value: SortSpecialEnum) =>
        {
            this._ShowEmergencySquawksSortSpecial = value;
            this._Dispatcher.raise(this._Events.sortFieldsChanged);
        }

        /**
         * Returns a value indicating how interesting aircraft should be treated in the sort.
         */
        getShowInterestingSortSpecial = () : SortSpecialEnum =>
        {
            return this._ShowInterestingSortSpecial;
        }
        setShowInterestingSortSpecial = (value: SortSpecialEnum) =>
        {
            this._ShowInterestingSortSpecial = value;
            this._Dispatcher.raise(this._Events.sortFieldsChanged);
        }

        hookSortFieldsChanged = (callback: () => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.sortFieldsChanged, callback, forceThis);
        }

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
         * Returns the previously saved state or the current state if the state has never been saved.
         */
        loadState = () : AircraftListSorter_SaveState =>
        {
            var savedSettings = VRS.configStorage.load(this.persistenceKey(), {});
            var result = $.extend(this.createSettings(), savedSettings);

            VRS.arrayHelper.normaliseOptionsArray(VRS.globalOptions.aircraftListDefaultSortOrder, result.sortFields, (entry: AircraftListSorter_SortField) =>
            {
                return entry.ascending !== undefined && (VRS.aircraftListSortHandlers[entry.field] ? true : false);
            });

            return result;
        }

        /**
         * Applies the previously saved state to this object.
         */
        applyState = (settings: AircraftListSorter_SaveState) =>
        {
            this._SortFields = [];
            $.each(settings.sortFields, (idx, sortField) => {
                this._SortFields.push(sortField);
            });
            this._ShowEmergencySquawksSortSpecial = settings.showEmergencySquawks;
            this._ShowInterestingSortSpecial = settings.showInteresting;

            this._Dispatcher.raise(this._Events.sortFieldsChanged);
        }

        /**
         * Loads and then applies the previously saved state.
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
            return 'vrsAircraftSorter-' + this._Name;
        }

        /**
         * Creates the object that describes the persistent state.
         */
        private createSettings() : AircraftListSorter_SaveState
        {
            return {
                sortFields:             this._SortFields,
                showEmergencySquawks:   this._ShowEmergencySquawksSortSpecial,
                showInteresting:        this._ShowInterestingSortSpecial
            };
        }

        /**
         * Returns the description of the configuration pane for this object.
         */
        createOptionPane = (displayOrder: number) : OptionPane =>
        {
            var pane = new VRS.OptionPane({
                name:           'vrsAircraftSorter_' + this._Name,
                titleKey:       'PaneSortAircraftList',
                displayOrder:   displayOrder
            });

            var handlers = [];
            var handler;
            for(var fieldId in VRS.aircraftListSortHandlers) {
                handler = VRS.aircraftListSortHandlers[fieldId];
                if(handler instanceof VRS.AircraftListSortHandler) {
                    handlers.push(handler);
                }
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
                values.push(new VRS.ValueText({
                    value: handler.Field,
                    textKey: handler.LabelKey
                }));
            }

            $.each(this._SortFields, (idx: number) =>
            {
                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'sortBy' + idx.toString(),
                    labelKey:       idx === 0 ? 'SortBy' : 'ThenBy',
                    getValue:       () => this.getSortField(idx).field,
                    setValue:       (value: AircraftListSortableFieldEnum) => {
                                        var setEntry = this.getSortField(idx);
                                        setEntry.field = value;
                                        this.setSortField(idx, setEntry);
                                    },
                    saveState:      this.saveState,
                    values:         values,
                    keepWithNext:   true
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'ascending' + idx.toString(),
                    labelKey:       'Ascending',
                    getValue:       () => this.getSortField(idx).ascending,
                    setValue:       (value: boolean) => {
                                        var setEntry = this.getSortField(idx);
                                        setEntry.ascending = value;
                                        this.setSortField(idx, setEntry);
                                    },
                    saveState:      this.saveState
                }));
            });

            pane.addField(new VRS.OptionFieldRadioButton({
                name:           'showEmergency',
                getValue:       this.getShowEmergencySquawksSortSpecial,
                setValue:       this.setShowEmergencySquawksSortSpecial,
                saveState:      this.saveState,
                labelKey:       'ShowEmergencySquawks',
                values:         this.buildSortSpecialValueTexts()
            }));
            pane.addField(new VRS.OptionFieldRadioButton({
                name:           'showInteresting',
                getValue:       this.getShowInterestingSortSpecial,
                setValue:       this.setShowInterestingSortSpecial,
                saveState:      this.saveState,
                labelKey:       'ShowInterestingAircraft',
                values:         this.buildSortSpecialValueTexts()
            }));

            return pane;
        }

        private buildSortSpecialValueTexts() : ValueText[]
        {
            return [
                new VRS.ValueText({ value: VRS.SortSpecial.First,   textKey: 'First' }),
                new VRS.ValueText({ value: VRS.SortSpecial.Last,    textKey: 'Last' }),
                new VRS.ValueText({ value: VRS.SortSpecial.Neither, textKey: 'Neither' })
            ];
        }

        /**
         * Changes the order of the elements in the array passed across so that the aircraft follow the configured sort order.
         */
        sortAircraftArray = (array: Aircraft[]) =>
        {
            var i = 0;
            var handlers = [];

            switch(this._ShowEmergencySquawksSortSpecial) {
                case VRS.SortSpecial.First:     handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Emergency], ascending: true }); break;
                case VRS.SortSpecial.Last:      handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Emergency], ascending: false }); break;
            }
            switch(this._ShowInterestingSortSpecial) {
                case VRS.SortSpecial.First:     handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Interesting], ascending: true }); break;
                case VRS.SortSpecial.Last:      handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Interesting], ascending: false }); break;
            }

            for(i = 0;i < this._SortFieldsLength;++i) {
                var sortField = this._SortFields[i];
                if(sortField.field !== VRS.AircraftListSortableField.None) {
                    handlers.push({
                        handler: VRS.aircraftListSortHandlers[sortField.field],
                        ascending: sortField.ascending
                    });
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
        }
    }
} 