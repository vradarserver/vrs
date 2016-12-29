var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.aircraftListDefaultSortOrder = VRS.globalOptions.aircraftListDefaultSortOrder || [
        { field: VRS.AircraftListSortableField.TimeTracked, ascending: true },
        { field: VRS.AircraftListSortableField.None, ascending: true },
        { field: VRS.AircraftListSortableField.None, ascending: true }
    ];
    VRS.globalOptions.aircraftListShowEmergencySquawks = VRS.globalOptions.aircraftListShowEmergencySquawks !== undefined ? VRS.globalOptions.aircraftListShowEmergencySquawks : VRS.SortSpecial.First;
    VRS.globalOptions.aircraftListShowInteresting = VRS.globalOptions.aircraftListShowInteresting !== undefined ? VRS.globalOptions.aircraftListShowInteresting : VRS.SortSpecial.First;
    var AircraftListSortHandler = (function () {
        function AircraftListSortHandler(settings) {
            if (!settings)
                throw 'You must supply a settings object';
            if (!settings.field && !settings.compareCallback)
                throw 'You must supply a field field';
            if (!settings.labelKey && !settings.compareCallback)
                throw 'You must supply a labelKey field';
            if (!settings.compareCallback && !settings.getNumberCallback && !settings.getStringCallback)
                throw 'You must supply a compareCallback, getNumberCallback or getStringCallback';
            if (!settings.compareCallback) {
                settings.compareCallback = settings.getNumberCallback ? this.compareNumericValues : this.compareStringValues;
            }
            this.Field = settings.field;
            this.LabelKey = settings.labelKey;
            this.GetNumberCallback = settings.getNumberCallback;
            this.GetStringCallback = settings.getStringCallback;
            this.CompareCallback = settings.compareCallback;
        }
        AircraftListSortHandler.prototype.compareNumericValues = function (lhs, rhs, unitDisplayPreferences) {
            var lhsValue = this.GetNumberCallback(lhs, unitDisplayPreferences);
            var rhsValue = this.GetNumberCallback(rhs, unitDisplayPreferences);
            if (!lhsValue && lhsValue !== 0)
                return -1;
            if (!rhsValue && rhsValue !== 0)
                return 1;
            return lhsValue - rhsValue;
        };
        AircraftListSortHandler.prototype.compareStringValues = function (lhs, rhs, unitDisplayPreferences) {
            var lhsString = this.GetStringCallback(lhs, unitDisplayPreferences);
            var rhsString = this.GetStringCallback(rhs, unitDisplayPreferences);
            if (lhsString === undefined || lhsString === null)
                return -1;
            if (rhsString === undefined || rhsString === null)
                return 1;
            return lhsString.localeCompare(rhsString);
        };
        return AircraftListSortHandler;
    }());
    VRS.AircraftListSortHandler = AircraftListSortHandler;
    VRS.aircraftListSortHandlers = VRS.aircraftListSortHandlers || {};
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Altitude] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Altitude,
        labelKey: 'Altitude',
        getNumberCallback: function (aircraft, unitDisplayPreferences) { return aircraft.getMixedAltitude(unitDisplayPreferences.getUsePressureAltitude()); }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.AltitudeBarometric] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.AltitudeBarometric,
        labelKey: 'PressureAltitude',
        getNumberCallback: function (aircraft, unitDisplayPreferences) { return aircraft.altitude.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.AltitudeGeometric] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.AltitudeGeometric,
        labelKey: 'GeometricAltitude',
        getNumberCallback: function (aircraft, unitDisplayPreferences) { return aircraft.geometricAltitude.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.AltitudeType] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.AltitudeType,
        labelKey: 'AltitudeType',
        getStringCallback: function (aircraft) { return aircraft.formatAltitudeType(); }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.AirPressure] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.AirPressure,
        labelKey: 'AirPressure',
        getNumberCallback: function (aircraft) { return aircraft.airPressureInHg.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.AverageSignalLevel] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.AverageSignalLevel,
        labelKey: 'AverageSignalLevel',
        getNumberCallback: function (aircraft) { return aircraft.averageSignalLevel.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Bearing] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Bearing,
        labelKey: 'Bearing',
        getNumberCallback: function (aircraft) { return aircraft.bearingFromHere.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Callsign] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Callsign,
        labelKey: 'Callsign',
        getStringCallback: function (aircraft) { return aircraft.callsign.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.CivOrMil] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.CivOrMil,
        labelKey: 'CivilOrMilitary',
        getNumberCallback: function (aircraft) { return aircraft.isMilitary.val ? 1 : 0; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.CountMessages] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.CountMessages,
        labelKey: 'MessageCount',
        getNumberCallback: function (aircraft) { return aircraft.countMessages.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Country] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Country,
        labelKey: 'Country',
        getStringCallback: function (aircraft) { return aircraft.country.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Distance] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Distance,
        labelKey: 'Distance',
        getNumberCallback: function (aircraft) { return aircraft.distanceFromHereKm.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.FlightsCount] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.FlightsCount,
        labelKey: 'FlightsCount',
        getNumberCallback: function (aircraft) { return aircraft.countFlights.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Heading] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Heading,
        labelKey: 'Heading',
        getNumberCallback: function (aircraft) { return aircraft.heading.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.HeadingType] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.HeadingType,
        labelKey: 'HeadingType',
        getStringCallback: function (aircraft) { return aircraft.formatHeadingType(); }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Icao] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Icao,
        labelKey: 'Icao',
        getStringCallback: function (aircraft) { return aircraft.icao.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Latitude] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Latitude,
        labelKey: 'Latitude',
        getNumberCallback: function (aircraft) { return aircraft.latitude.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Longitude] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Longitude,
        labelKey: 'Longitude',
        getNumberCallback: function (aircraft) { return aircraft.longitude.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Manufacturer] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Manufacturer,
        labelKey: 'Manufacturer',
        getStringCallback: function (aircraft) { return aircraft.manufacturer.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Mlat] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Mlat,
        labelKey: 'Mlat',
        getNumberCallback: function (aircraft) { return aircraft.isMlat.val === undefined ? 0 : aircraft.isMlat ? 1 : 2; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Model] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Model,
        labelKey: 'Model',
        getStringCallback: function (aircraft) { return aircraft.model.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.ModelIcao] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.ModelIcao,
        labelKey: 'ModelIcao',
        getStringCallback: function (aircraft) { return aircraft.modelIcao.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.None] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.None,
        labelKey: 'None',
        compareCallback: function (aircraft) { throw 'If this gets called then it\'s a bug'; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Operator] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Operator,
        labelKey: 'Operator',
        getStringCallback: function (aircraft) { return aircraft.operator.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.OperatorIcao] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.OperatorIcao,
        labelKey: 'OperatorCode',
        getStringCallback: function (aircraft) { return aircraft.operatorIcao.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.PositionAgeSeconds] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.PositionAgeSeconds,
        labelKey: 'PositionAge',
        getNumberCallback: function (aircraft) { return aircraft.positionAgeSeconds.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Receiver] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Receiver,
        labelKey: 'Receiver',
        getNumberCallback: function (aircraft) { return aircraft.receiverId.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Registration] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Registration,
        labelKey: 'Registration',
        getStringCallback: function (aircraft) { return aircraft.registration.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Serial] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Serial,
        labelKey: 'SerialNumber',
        getStringCallback: function (aircraft) { return aircraft.serial.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.SignalLevel] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.SignalLevel,
        labelKey: 'SignalLevel',
        getNumberCallback: function (aircraft) { return aircraft.signalLevel.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Speed] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Speed,
        labelKey: 'Speed',
        getNumberCallback: function (aircraft) { return aircraft.speed.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.SpeedType] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.SpeedType,
        labelKey: 'SpeedType',
        getStringCallback: function (aircraft) { return aircraft.formatSpeedType(); }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.Squawk] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.Squawk,
        labelKey: 'Squawk',
        getStringCallback: function (aircraft) { return aircraft.squawk.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TargetAltitude] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.TargetAltitude,
        labelKey: 'TargetAltitude',
        getNumberCallback: function (aircraft) { return aircraft.targetAltitude.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TargetHeading] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.TargetHeading,
        labelKey: 'TargetHeading',
        getNumberCallback: function (aircraft) { return aircraft.targetHeading.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TimeTracked] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.TimeTracked,
        labelKey: 'TimeTracked',
        getNumberCallback: function (aircraft) { return aircraft.secondsTracked; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.TransponderType] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.TransponderType,
        labelKey: 'TransponderType',
        getNumberCallback: function (aircraft) { return aircraft.transponderType.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.UserTag] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.UserTag,
        labelKey: 'UserTag',
        getStringCallback: function (aircraft) { return aircraft.userTag.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.VerticalSpeed] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.VerticalSpeed,
        labelKey: 'VerticalSpeed',
        getNumberCallback: function (aircraft) { return aircraft.verticalSpeed.val; }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.VerticalSpeedType] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.VerticalSpeedType,
        labelKey: 'VerticalSpeedType',
        getStringCallback: function (aircraft) { return aircraft.formatVerticalSpeedType(); }
    });
    VRS.aircraftListSortHandlers[VRS.AircraftListSortableField.YearBuilt] = new VRS.AircraftListSortHandler({
        field: VRS.AircraftListSortableField.YearBuilt,
        labelKey: 'YearBuilt',
        getStringCallback: function (aircraft) { return aircraft.yearBuilt.val; }
    });
    VRS.AircraftListSpecialHandlerIndex = {
        Emergency: 0,
        Interesting: 1
    };
    VRS.aircraftListSpecialHandlers = [
        new VRS.AircraftListSortHandler({
            compareCallback: function (lhs, rhs) {
                return !!lhs.isEmergency.val === !!rhs.isEmergency.val ? 0 : !!lhs.isEmergency.val ? -1 : 1;
            }
        }),
        new VRS.AircraftListSortHandler({
            compareCallback: function (lhs, rhs) {
                return !!lhs.userInterested.val === !!rhs.userInterested.val ? 0 : !!lhs.userInterested.val ? -1 : 1;
            }
        })
    ];
    var AircraftListSorter = (function () {
        function AircraftListSorter(name) {
            var _this = this;
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.AircraftListSorter'
            });
            this._Events = {
                sortFieldsChanged: 'sortFieldsChanged'
            };
            this.getName = function () {
                return _this._Name;
            };
            this.getSortFieldsCount = function () {
                return _this._SortFieldsLength;
            };
            this.getSortField = function (index) {
                var existing = _this._SortFields[index];
                return { field: existing.field, ascending: existing.ascending };
            };
            this.setSortField = function (index, sortField) {
                var existing = _this._SortFields[index];
                if (existing.field !== sortField.field || existing.ascending !== sortField.ascending) {
                    _this._SortFields[index] = sortField;
                    _this._Dispatcher.raise(_this._Events.sortFieldsChanged);
                }
            };
            this.getSingleSortField = function () {
                var result = null;
                var length = _this._SortFields.length;
                for (var i = 0; i < length; ++i) {
                    var sortField = _this._SortFields[i];
                    if (sortField.field !== VRS.AircraftListSortableField.None) {
                        if (result !== null) {
                            result = null;
                            break;
                        }
                        result = _this.getSortField(i);
                    }
                }
                return result;
            };
            this.setSingleSortField = function (sortField) {
                var raiseEvent = false;
                var empty = { field: VRS.AircraftListSortableField.None, ascending: false };
                var length = _this._SortFields.length;
                for (var i = 0; i < length; ++i) {
                    var existing = _this._SortFields[i];
                    var replaceWith = i == 0 ? sortField : empty;
                    if (!raiseEvent) {
                        raiseEvent = existing.field !== replaceWith.field || existing.ascending !== replaceWith.ascending;
                    }
                    existing.field = replaceWith.field;
                    existing.ascending = replaceWith.ascending;
                }
                if (raiseEvent) {
                    _this._Dispatcher.raise(_this._Events.sortFieldsChanged);
                }
            };
            this.getShowEmergencySquawksSortSpecial = function () {
                return _this._ShowEmergencySquawksSortSpecial;
            };
            this.setShowEmergencySquawksSortSpecial = function (value) {
                _this._ShowEmergencySquawksSortSpecial = value;
                _this._Dispatcher.raise(_this._Events.sortFieldsChanged);
            };
            this.getShowInterestingSortSpecial = function () {
                return _this._ShowInterestingSortSpecial;
            };
            this.setShowInterestingSortSpecial = function (value) {
                _this._ShowInterestingSortSpecial = value;
                _this._Dispatcher.raise(_this._Events.sortFieldsChanged);
            };
            this.hookSortFieldsChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.sortFieldsChanged, callback, forceThis);
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
                VRS.arrayHelper.normaliseOptionsArray(VRS.globalOptions.aircraftListDefaultSortOrder, result.sortFields, function (entry) {
                    return entry.ascending !== undefined && (VRS.aircraftListSortHandlers[entry.field] ? true : false);
                });
                return result;
            };
            this.applyState = function (settings) {
                _this._SortFields = [];
                $.each(settings.sortFields, function (idx, sortField) {
                    _this._SortFields.push(sortField);
                });
                _this._ShowEmergencySquawksSortSpecial = settings.showEmergencySquawks;
                _this._ShowInterestingSortSpecial = settings.showInteresting;
                _this._Dispatcher.raise(_this._Events.sortFieldsChanged);
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.createOptionPane = function (displayOrder) {
                var pane = new VRS.OptionPane({
                    name: 'vrsAircraftSorter_' + _this._Name,
                    titleKey: 'PaneSortAircraftList',
                    displayOrder: displayOrder
                });
                var handlers = [];
                var handler;
                for (var fieldId in VRS.aircraftListSortHandlers) {
                    handler = VRS.aircraftListSortHandlers[fieldId];
                    if (handler instanceof VRS.AircraftListSortHandler) {
                        handlers.push(handler);
                    }
                }
                handlers.sort(function (lhs, rhs) {
                    if (lhs.Field === VRS.AircraftListSortableField.None)
                        return rhs.Field === VRS.AircraftListSortableField.None ? 0 : -1;
                    if (rhs.Field === VRS.AircraftListSortableField.None)
                        return 1;
                    var lhsText = VRS.globalisation.getText(lhs.LabelKey) || '';
                    var rhsText = VRS.globalisation.getText(rhs.LabelKey) || '';
                    return lhsText.localeCompare(rhsText);
                });
                var values = [];
                var countHandlers = handlers.length;
                for (var i = 0; i < countHandlers; ++i) {
                    handler = handlers[i];
                    values.push(new VRS.ValueText({
                        value: handler.Field,
                        textKey: handler.LabelKey
                    }));
                }
                $.each(_this._SortFields, function (idx) {
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'sortBy' + idx.toString(),
                        labelKey: idx === 0 ? 'SortBy' : 'ThenBy',
                        getValue: function () { return _this.getSortField(idx).field; },
                        setValue: function (value) {
                            var setEntry = _this.getSortField(idx);
                            setEntry.field = value;
                            _this.setSortField(idx, setEntry);
                        },
                        saveState: _this.saveState,
                        values: values,
                        keepWithNext: true
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'ascending' + idx.toString(),
                        labelKey: 'Ascending',
                        getValue: function () { return _this.getSortField(idx).ascending; },
                        setValue: function (value) {
                            var setEntry = _this.getSortField(idx);
                            setEntry.ascending = value;
                            _this.setSortField(idx, setEntry);
                        },
                        saveState: _this.saveState
                    }));
                });
                pane.addField(new VRS.OptionFieldRadioButton({
                    name: 'showEmergency',
                    getValue: _this.getShowEmergencySquawksSortSpecial,
                    setValue: _this.setShowEmergencySquawksSortSpecial,
                    saveState: _this.saveState,
                    labelKey: 'ShowEmergencySquawks',
                    values: _this.buildSortSpecialValueTexts()
                }));
                pane.addField(new VRS.OptionFieldRadioButton({
                    name: 'showInteresting',
                    getValue: _this.getShowInterestingSortSpecial,
                    setValue: _this.setShowInterestingSortSpecial,
                    saveState: _this.saveState,
                    labelKey: 'ShowInterestingAircraft',
                    values: _this.buildSortSpecialValueTexts()
                }));
                return pane;
            };
            this.sortAircraftArray = function (array, unitDisplayPreferences) {
                var i = 0;
                var handlers = [];
                switch (_this._ShowEmergencySquawksSortSpecial) {
                    case VRS.SortSpecial.First:
                        handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Emergency], ascending: true });
                        break;
                    case VRS.SortSpecial.Last:
                        handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Emergency], ascending: false });
                        break;
                }
                switch (_this._ShowInterestingSortSpecial) {
                    case VRS.SortSpecial.First:
                        handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Interesting], ascending: true });
                        break;
                    case VRS.SortSpecial.Last:
                        handlers.push({ handler: VRS.aircraftListSpecialHandlers[VRS.AircraftListSpecialHandlerIndex.Interesting], ascending: false });
                        break;
                }
                for (i = 0; i < _this._SortFieldsLength; ++i) {
                    var sortField = _this._SortFields[i];
                    if (sortField.field !== VRS.AircraftListSortableField.None) {
                        handlers.push({
                            handler: VRS.aircraftListSortHandlers[sortField.field],
                            ascending: sortField.ascending
                        });
                    }
                }
                var length = handlers.length;
                if (length > 0) {
                    array.sort(function (lhs, rhs) {
                        for (i = 0; i < length; ++i) {
                            var handler = handlers[i];
                            var result = handler.handler.CompareCallback(lhs, rhs, unitDisplayPreferences);
                            if (result != 0) {
                                if (!handler.ascending)
                                    result = -result;
                                break;
                            }
                        }
                        return result;
                    });
                }
            };
            this._Name = name || 'default';
            this._SortFields = VRS.globalOptions.aircraftListDefaultSortOrder.slice();
            this._SortFieldsLength = this._SortFields.length;
            this._ShowEmergencySquawksSortSpecial = VRS.globalOptions.aircraftListShowEmergencySquawks;
            this._ShowInterestingSortSpecial = VRS.globalOptions.aircraftListShowInteresting;
        }
        AircraftListSorter.prototype.persistenceKey = function () {
            return 'vrsAircraftSorter-' + this._Name;
        };
        AircraftListSorter.prototype.createSettings = function () {
            return {
                sortFields: this._SortFields,
                showEmergencySquawks: this._ShowEmergencySquawksSortSpecial,
                showInteresting: this._ShowInterestingSortSpecial
            };
        };
        AircraftListSorter.prototype.buildSortSpecialValueTexts = function () {
            return [
                new VRS.ValueText({ value: VRS.SortSpecial.First, textKey: 'First' }),
                new VRS.ValueText({ value: VRS.SortSpecial.Last, textKey: 'Last' }),
                new VRS.ValueText({ value: VRS.SortSpecial.Neither, textKey: 'Neither' })
            ];
        };
        return AircraftListSorter;
    }());
    VRS.AircraftListSorter = AircraftListSorter;
})(VRS || (VRS = {}));
//# sourceMappingURL=aircraftListSorter.js.map