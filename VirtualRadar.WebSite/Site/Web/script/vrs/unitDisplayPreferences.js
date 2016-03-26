var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.isFlightSim = VRS.isFlightSim || false;
    VRS.globalOptions.unitDisplayHeight = VRS.globalOptions.unitDisplayHeight || VRS.Height.Feet;
    VRS.globalOptions.unitDisplaySpeed = VRS.globalOptions.unitDisplaySpeed || VRS.Speed.Knots;
    VRS.globalOptions.unitDisplayDistance = VRS.globalOptions.unitDisplayDistance || VRS.Distance.Kilometre;
    VRS.globalOptions.unitDisplayPressure = VRS.globalOptions.unitDisplayPressure || VRS.Pressure.InHg;
    VRS.globalOptions.unitDisplayVsiPerSecond = VRS.globalOptions.unitDisplayVsiPerSecond !== undefined ? VRS.globalOptions.unitDisplayVsiPerSecond : VRS.isFlightSim;
    VRS.globalOptions.unitDisplayFLTransitionAltitude = VRS.globalOptions.unitDisplayFLTransitionAltitude || 18000;
    VRS.globalOptions.unitDisplayFLTransitionHeightUnit = VRS.globalOptions.unitDisplayFLTransitionHeightUnit || VRS.Height.Feet;
    VRS.globalOptions.unitDisplayFLHeightUnit = VRS.globalOptions.unitDisplayFLHeightUnit || VRS.Height.Feet;
    VRS.globalOptions.unitDisplayAllowConfiguration = VRS.globalOptions.unitDisplayAllowConfiguration !== undefined ? VRS.globalOptions.unitDisplayAllowConfiguration : true;
    VRS.globalOptions.unitDisplayAltitudeType = VRS.globalOptions.unitDisplayAltitudeType !== undefined ? VRS.globalOptions.unitDisplayAltitudeType : false;
    VRS.globalOptions.unitDisplayVerticalSpeedType = VRS.globalOptions.unitDisplayVerticalSpeedType !== undefined ? VRS.globalOptions.unitDisplayVerticalSpeedType : false;
    VRS.globalOptions.unitDisplaySpeedType = VRS.globalOptions.unitDisplaySpeedType !== undefined ? VRS.globalOptions.unitDisplaySpeedType : true;
    VRS.globalOptions.unitDisplayTrackType = VRS.globalOptions.unitDisplayTrackType !== undefined ? VRS.globalOptions.unitDisplayTrackType : false;
    VRS.globalOptions.unitDisplayUsePressureAltitude = VRS.globalOptions.unitDisplayUsePressureAltitude !== undefined ? VRS.globalOptions.unitDisplayUsePressureAltitude : true;
    var UnitDisplayPreferences = (function () {
        function UnitDisplayPreferences(name) {
            var _this = this;
            this._Dispatcher = new VRS.EventHandler({
                name: 'VRS.UnitDisplayPreferences'
            });
            this._Events = {
                unitChanged: 'unitChanged',
                distanceUnitChanged: 'distanceUnitChanged',
                heightUnitChanged: 'heightUnitChanged',
                speedUnitChanged: 'speedUnitChanged',
                pressureUnitChanged: 'pressureUnitChanged',
                showVsiInSecondsChanged: 'showVsiSecondsChanged',
                flAltitudeChanged: 'flAltitudeChanged',
                flTransUnitChanged: 'flTransUnitChanged',
                flHeightUnitChanged: 'flHeightUnitChanged',
                showAltitudeTypeChanged: 'showAltTypeChanged',
                showVerticalSpeedTypeChanged: 'showVsiTypeChanged',
                showSpeedTypeChanged: 'showSpeedTypeChanged',
                showTrackTypeChanged: 'showTrackTypeChanged',
                usePressureAltitudeChanged: 'usePressureAltitudeChanged'
            };
            this.getName = function () {
                return _this._Name;
            };
            this.getDistanceUnit = function () {
                return _this._DistanceUnit;
            };
            this.setDistanceUnit = function (value) {
                if (_this._DistanceUnit !== value) {
                    _this._DistanceUnit = value;
                    _this._Dispatcher.raise(_this._Events.distanceUnitChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Distance]);
                }
            };
            this.getHeightUnit = function () {
                return _this._HeightUnit;
            };
            this.setHeightUnit = function (value) {
                if (_this._HeightUnit !== value) {
                    _this._HeightUnit = value;
                    _this._Dispatcher.raise(_this._Events.heightUnitChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Height]);
                }
            };
            this.getSpeedUnit = function () {
                return _this._SpeedUnit;
            };
            this.setSpeedUnit = function (value) {
                if (_this._SpeedUnit !== value) {
                    _this._SpeedUnit = value;
                    _this._Dispatcher.raise(_this._Events.speedUnitChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Speed]);
                }
            };
            this.getPressureUnit = function () {
                return _this._PressureUnit;
            };
            this.setPressureUnit = function (value) {
                if (_this._PressureUnit !== value) {
                    _this._PressureUnit = value;
                    _this._Dispatcher.raise(_this._Events.pressureUnitChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Pressure]);
                }
            };
            this.getShowVerticalSpeedPerSecond = function () {
                return _this._ShowVerticalSpeedPerSecond;
            };
            this.setShowVerticalSpeedPerSecond = function (value) {
                if (_this._ShowVerticalSpeedPerSecond !== value) {
                    _this._ShowVerticalSpeedPerSecond = value;
                    _this._Dispatcher.raise(_this._Events.showVsiInSecondsChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.VsiSeconds]);
                }
            };
            this.getShowAltitudeType = function () {
                return _this._ShowAltitudeType;
            };
            this.setShowAltitudeType = function (value) {
                if (_this._ShowAltitudeType !== value) {
                    _this._ShowAltitudeType = value;
                    _this._Dispatcher.raise(_this._Events.showAltitudeTypeChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Height]);
                }
            };
            this.getShowVerticalSpeedType = function () {
                return _this._ShowVerticalSpeedType;
            };
            this.setShowVerticalSpeedType = function (value) {
                if (_this._ShowVerticalSpeedType !== value) {
                    _this._ShowVerticalSpeedType = value;
                    _this._Dispatcher.raise(_this._Events.showVerticalSpeedTypeChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Height]);
                }
            };
            this.getShowSpeedType = function () {
                return _this._ShowSpeedType;
            };
            this.setShowSpeedType = function (value) {
                if (_this._ShowSpeedType !== value) {
                    _this._ShowSpeedType = value;
                    _this._Dispatcher.raise(_this._Events.showSpeedTypeChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Speed]);
                }
            };
            this.getShowTrackType = function () {
                return _this._ShowTrackType;
            };
            this.setShowTrackType = function (value) {
                if (_this._ShowTrackType !== value) {
                    _this._ShowTrackType = value;
                    _this._Dispatcher.raise(_this._Events.showTrackTypeChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Angle]);
                }
            };
            this.getUsePressureAltitude = function () {
                return _this._UsePressureAltitude;
            };
            this.setUsePressureAltitude = function (value) {
                if (_this._UsePressureAltitude !== value) {
                    _this._UsePressureAltitude = value;
                    _this._Dispatcher.raise(_this._Events.usePressureAltitudeChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.Height]);
                }
            };
            this.getFlightLevelTransitionAltitude = function () {
                return _this._FlightLevelTransitionAltitude;
            };
            this.setFlightLevelTransitionAltitude = function (value) {
                if (_this._FlightLevelTransitionAltitude !== value) {
                    _this._FlightLevelTransitionAltitude = value;
                    _this._Dispatcher.raise(_this._Events.flAltitudeChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.FLTransitionAltitude]);
                }
            };
            this.getFlightLevelTransitionHeightUnit = function () {
                return _this._FlightLevelTransitionHeightUnit;
            };
            this.setFlightLevelTransitionHeightUnit = function (value) {
                if (_this._FlightLevelTransitionHeightUnit !== value) {
                    _this._FlightLevelTransitionHeightUnit = value;
                    _this._Dispatcher.raise(_this._Events.flTransUnitChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.FLTransitionHeightUnit]);
                }
            };
            this.getFlightLevelHeightUnit = function () {
                return _this._FlightLevelHeightUnit;
            };
            this.setFlightLevelHeightUnit = function (value) {
                if (_this._FlightLevelHeightUnit !== value) {
                    _this._FlightLevelHeightUnit = value;
                    _this._Dispatcher.raise(_this._Events.flHeightUnitChanged);
                    _this._Dispatcher.raise(_this._Events.unitChanged, [VRS.DisplayUnitDependency.FLHeightUnit]);
                }
            };
            this.hookDistanceUnitChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.distanceUnitChanged, callback, forceThis);
            };
            this.hookHeightUnitChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.heightUnitChanged, callback, forceThis);
            };
            this.hookSpeedUnitChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.speedUnitChanged, callback, forceThis);
            };
            this.hookPressureUnitChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.pressureUnitChanged, callback, forceThis);
            };
            this.hookShowVerticalSpeedPerSecondChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.showVsiInSecondsChanged, callback, forceThis);
            };
            this.hookShowAltitudeTypeChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.showAltitudeTypeChanged, callback, forceThis);
            };
            this.hookShowVerticalSpeedTypeChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.showVerticalSpeedTypeChanged, callback, forceThis);
            };
            this.hookShowSpeedTypeChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.showSpeedTypeChanged, callback, forceThis);
            };
            this.hookShowTrackTypeChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.showTrackTypeChanged, callback, forceThis);
            };
            this.hookFlightLevelTransitionAltitudeChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.flAltitudeChanged, callback, forceThis);
            };
            this.hookFlightLevelTransitionHeightUnitChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.flTransUnitChanged, callback, forceThis);
            };
            this.hookFlightLevelHeightUnitChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.flHeightUnitChanged, callback, forceThis);
            };
            this.hookUsePressureAltitudeChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.usePressureAltitudeChanged, callback, forceThis);
            };
            this.hookUnitChanged = function (callback, forceThis) {
                return _this._Dispatcher.hook(_this._Events.unitChanged, callback, forceThis);
            };
            this.unhook = function (hookResult) {
                _this._Dispatcher.unhook(hookResult);
            };
            this.createOptionPane = function (displayOrder) {
                var self = _this;
                var pane = new VRS.OptionPane({
                    name: 'vrsUnitDisplayPreferences_' + _this._Name,
                    titleKey: 'PaneUnits',
                    displayOrder: displayOrder
                });
                var distanceUnitValues = UnitDisplayPreferences.getDistanceUnitValues();
                var altitudeUnitValues = UnitDisplayPreferences.getAltitudeUnitValues();
                var speedUnitValues = UnitDisplayPreferences.getSpeedUnitValues();
                var pressureUnitValues = UnitDisplayPreferences.getPressureUnitValues();
                if (VRS.globalOptions.unitDisplayAllowConfiguration) {
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'showVsiInSeconds',
                        labelKey: 'ShowVsiInSeconds',
                        getValue: _this.getShowVerticalSpeedPerSecond,
                        setValue: _this.setShowVerticalSpeedPerSecond,
                        saveState: _this.saveState
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'showAltitudeType',
                        labelKey: 'ShowAltitudeType',
                        getValue: _this.getShowAltitudeType,
                        setValue: _this.setShowAltitudeType,
                        saveState: _this.saveState
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'showVerticalSpeedType',
                        labelKey: 'ShowVerticalSpeedType',
                        getValue: _this.getShowVerticalSpeedType,
                        setValue: _this.setShowVerticalSpeedType,
                        saveState: _this.saveState
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'showSpeedType',
                        labelKey: 'ShowSpeedType',
                        getValue: _this.getShowSpeedType,
                        setValue: _this.setShowSpeedType,
                        saveState: _this.saveState
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'showTrackType',
                        labelKey: 'ShowTrackType',
                        getValue: _this.getShowTrackType,
                        setValue: _this.setShowTrackType,
                        saveState: _this.saveState
                    }));
                    pane.addField(new VRS.OptionFieldCheckBox({
                        name: 'usePressureAltitude',
                        labelKey: 'UsePressureAltitude',
                        getValue: _this.getUsePressureAltitude,
                        setValue: _this.setUsePressureAltitude,
                        saveState: _this.saveState
                    }));
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'distanceUnit',
                        labelKey: 'Distances',
                        getValue: _this.getDistanceUnit,
                        setValue: _this.setDistanceUnit,
                        saveState: _this.saveState,
                        values: distanceUnitValues
                    }));
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'heightUnit',
                        labelKey: 'Heights',
                        getValue: _this.getHeightUnit,
                        setValue: _this.setHeightUnit,
                        saveState: _this.saveState,
                        values: altitudeUnitValues
                    }));
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'speedUnit',
                        labelKey: 'Speeds',
                        getValue: _this.getSpeedUnit,
                        setValue: _this.setSpeedUnit,
                        saveState: _this.saveState,
                        values: speedUnitValues
                    }));
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'pressureUnit',
                        labelKey: 'Pressures',
                        getValue: _this.getPressureUnit,
                        setValue: _this.setPressureUnit,
                        saveState: _this.saveState,
                        values: pressureUnitValues
                    }));
                    pane.addField(new VRS.OptionFieldNumeric({
                        name: 'flTransAltitude',
                        labelKey: 'FlightLevelTransitionAltitude',
                        getValue: _this.getFlightLevelTransitionAltitude,
                        setValue: _this.setFlightLevelTransitionAltitude,
                        saveState: _this.saveState,
                        inputWidth: VRS.InputWidth.SixChar,
                        min: -10000,
                        max: 99900,
                        decimals: 0,
                        step: 100,
                        keepWithNext: true
                    }));
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'flTransUnit',
                        getValue: _this.getFlightLevelTransitionHeightUnit,
                        setValue: _this.setFlightLevelTransitionHeightUnit,
                        saveState: _this.saveState,
                        values: altitudeUnitValues
                    }));
                    pane.addField(new VRS.OptionFieldComboBox({
                        name: 'flHeightUnit',
                        labelKey: 'FlightLevelHeightUnit',
                        getValue: _this.getFlightLevelHeightUnit,
                        setValue: _this.setFlightLevelHeightUnit,
                        saveState: _this.saveState,
                        values: altitudeUnitValues
                    }));
                }
                return pane;
            };
            this.saveState = function () {
                VRS.configStorage.saveWithoutPrefix(_this.persistenceKey(), _this.createSettings());
            };
            this.loadState = function () {
                var savedSettings = VRS.configStorage.loadWithoutPrefix(_this.persistenceKey(), {});
                return $.extend(_this.createSettings(), savedSettings);
            };
            this.applyState = function (settings) {
                _this.setDistanceUnit(settings.distanceUnit);
                _this.setHeightUnit(settings.heightUnit);
                _this.setSpeedUnit(settings.speedUnit);
                _this.setPressureUnit(settings.pressureUnit);
                _this.setShowVerticalSpeedPerSecond(settings.vsiPerSecond);
                _this.setFlightLevelTransitionAltitude(settings.flTransitionAlt);
                _this.setFlightLevelTransitionHeightUnit(settings.flTransitionUnit);
                _this.setFlightLevelHeightUnit(settings.flHeightUnit);
                _this.setShowAltitudeType(settings.showAltType);
                _this.setShowVerticalSpeedType(settings.showVsiType);
                _this.setShowSpeedType(settings.showSpeedType);
                _this.setShowTrackType(settings.showTrackType);
                _this.setUsePressureAltitude(settings.usePressureAltitude);
            };
            this.loadAndApplyState = function () {
                _this.applyState(_this.loadState());
            };
            this.persistenceKey = function () {
                return 'unitDisplayPreferences-' + _this.getName();
            };
            this.createSettings = function () {
                return {
                    distanceUnit: _this.getDistanceUnit(),
                    heightUnit: _this.getHeightUnit(),
                    speedUnit: _this.getSpeedUnit(),
                    pressureUnit: _this.getPressureUnit(),
                    vsiPerSecond: _this.getShowVerticalSpeedPerSecond(),
                    flTransitionAlt: _this.getFlightLevelTransitionAltitude(),
                    flTransitionUnit: _this.getFlightLevelTransitionHeightUnit(),
                    flHeightUnit: _this.getFlightLevelHeightUnit(),
                    showAltType: _this.getShowAltitudeType(),
                    showVsiType: _this.getShowVerticalSpeedType(),
                    showSpeedType: _this.getShowSpeedType(),
                    showTrackType: _this.getShowTrackType(),
                    usePressureAltitude: _this.getUsePressureAltitude()
                };
            };
            this._Name = name || 'vrsUnitDisplayPreferences';
            this._DistanceUnit = VRS.globalOptions.unitDisplayDistance;
            this._HeightUnit = VRS.globalOptions.unitDisplayHeight;
            this._SpeedUnit = VRS.globalOptions.unitDisplaySpeed;
            this._PressureUnit = VRS.globalOptions.unitDisplayPressure;
            this._ShowVerticalSpeedPerSecond = VRS.globalOptions.unitDisplayVsiPerSecond;
            this._ShowAltitudeType = VRS.globalOptions.unitDisplayAltitudeType;
            this._ShowVerticalSpeedType = VRS.globalOptions.unitDisplayVerticalSpeedType;
            this._ShowSpeedType = VRS.globalOptions.unitDisplaySpeedType;
            this._ShowTrackType = VRS.globalOptions.unitDisplayTrackType;
            this._UsePressureAltitude = VRS.globalOptions.unitDisplayUsePressureAltitude;
            this._FlightLevelTransitionAltitude = VRS.globalOptions.unitDisplayFLTransitionAltitude;
            this._FlightLevelTransitionHeightUnit = VRS.globalOptions.unitDisplayFLTransitionHeightUnit;
            this._FlightLevelHeightUnit = VRS.globalOptions.unitDisplayFLHeightUnit;
            if (VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if (config) {
                    this.setDistanceUnit(config.InitialDistanceUnit);
                    this.setHeightUnit(config.InitialHeightUnit);
                    this.setSpeedUnit(config.InitialSpeedUnit);
                }
            }
        }
        UnitDisplayPreferences.getAltitudeUnitValues = function () {
            return [
                new VRS.ValueText({ value: VRS.Height.Feet, textKey: 'Feet' }),
                new VRS.ValueText({ value: VRS.Height.Metre, textKey: 'Metres' })
            ];
        };
        UnitDisplayPreferences.getDistanceUnitValues = function () {
            return [
                new VRS.ValueText({ value: VRS.Distance.Kilometre, textKey: 'Kilometres' }),
                new VRS.ValueText({ value: VRS.Distance.NauticalMile, textKey: 'NauticalMiles' }),
                new VRS.ValueText({ value: VRS.Distance.StatuteMile, textKey: 'StatuteMiles' })
            ];
        };
        UnitDisplayPreferences.getSpeedUnitValues = function () {
            return [
                new VRS.ValueText({ value: VRS.Speed.KilometresPerHour, textKey: 'KilometresPerHour' }),
                new VRS.ValueText({ value: VRS.Speed.Knots, textKey: 'Knots' }),
                new VRS.ValueText({ value: VRS.Speed.MilesPerHour, textKey: 'MilesPerHour' })
            ];
        };
        UnitDisplayPreferences.getPressureUnitValues = function () {
            return [
                new VRS.ValueText({ value: VRS.Pressure.InHg, textKey: 'InHgDescription' }),
                new VRS.ValueText({ value: VRS.Pressure.Millibar, textKey: 'MillibarDescription' })
            ];
        };
        return UnitDisplayPreferences;
    })();
    VRS.UnitDisplayPreferences = UnitDisplayPreferences;
})(VRS || (VRS = {}));
//# sourceMappingURL=unitDisplayPreferences.js.map