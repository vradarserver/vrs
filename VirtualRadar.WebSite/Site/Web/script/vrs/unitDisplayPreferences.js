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
 * @fileoverview Code to record and configure the units of measurement shown for various heights, distances etc.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global options
    VRS.globalOptions.unitDisplayHeight = VRS.globalOptions.unitDisplayHeight || VRS.Height.Feet;                                   // The default unit for altitudes.
    VRS.globalOptions.unitDisplaySpeed = VRS.globalOptions.unitDisplaySpeed || VRS.Speed.Knots;                                     // The default unit for speeds.
    VRS.globalOptions.unitDisplayDistance = VRS.globalOptions.unitDisplayDistance || VRS.Distance.Kilometre;                        // The default unit for distances.
    VRS.globalOptions.unitDisplayVsiPerSecond = VRS.globalOptions.unitDisplayVsiPerSecond !== undefined ? VRS.globalOptions.unitDisplayVsiPerSecond : VRS.isFlightSim;          // True if vertical speeds are to be shown per second rather than per minute.
    VRS.globalOptions.unitDisplayFLTransitionAltitude = VRS.globalOptions.unitDisplayFLTransitionAltitude || 18000;                 // The default flight level transition altitude.
    VRS.globalOptions.unitDisplayFLTransitionHeightUnit = VRS.globalOptions.unitDisplayFLTransitionHeightUnit || VRS.Height.Feet;   // The units that FLTransitionAltitude is in.
    VRS.globalOptions.unitDisplayFLHeightUnit = VRS.globalOptions.unitDisplayFLHeightUnit || VRS.Height.Feet;                       // The units that flight levels are displayed in.
    VRS.globalOptions.unitDisplayAllowConfiguration = VRS.globalOptions.unitDisplayAllowConfiguration !== undefined ? VRS.globalOptions.unitDisplayAllowConfiguration : true;   // True if users can configure the display units, false if they cannot.
    VRS.globalOptions.unitDisplayAltitudeType = VRS.globalOptions.unitDisplayAltitudeType !== undefined ? VRS.globalOptions.unitDisplayAltitudeType : false;                // True if altitude types are to be shown by default.
    VRS.globalOptions.unitDisplayVerticalSpeedType = VRS.globalOptions.unitDisplayVerticalSpeedType !== undefined ? VRS.globalOptions.unitDisplayVerticalSpeedType : false; // True if vertical speed types are to be shown by default.
    //endregion

    //region UnitDisplayPreferences
    /**
     * A class that brings together the various units of measurement that the user can configure.
     * @param {string} [name] The name to use when storing the state of the object.
     * @constructor
     */
    VRS.UnitDisplayPreferences = function(name)
    {
        //region --Fields
        var that = this;
        var _Dispatcher = new VRS.EventHandler({
            name: 'VRS.UnitDisplayPreferences'
        });

        /** @enum {string} @private */
        var _Events = {
            unitChanged:                    'unitChanged',
            distanceUnitChanged:            'distanceUnitChanged',
            heightUnitChanged:              'heightUnitChanged',
            speedUnitChanged:               'speedUnitChanged',
            showVsiInSecondsChanged:        'showVsiSecondsChanged',
            flAltitudeChanged:              'flAltitudeChanged',
            flTransUnitChanged:             'flTransUnitChanged',
            flHeightUnitChanged:            'flHeightUnitChanged',
            showAltitudeTypeChanged:        'showAltTypeChanged',
            showVerticalSpeedTypeChanged:   'showVsiTypeChanged'
        };
        //endregion

        //region --Properties
        /** @type {string} @private */
        var _Name = name || 'vrsUnitDisplayPreferences';
        this.getName = function() { return _Name; };

        /** @type {*|VRS.Distance} @private */
        var _DistanceUnit = VRS.globalOptions.unitDisplayDistance;
        this.getDistanceUnit = function() { return _DistanceUnit; };
        this.setDistanceUnit = function(/** VRS.Distance */ value) {
            if(_DistanceUnit !== value) {
                _DistanceUnit = value;
                _Dispatcher.raise(_Events.distanceUnitChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.Distance ]);
            }
        };

        /** @type {VRS.Height} @private */
        var _HeightUnit = VRS.globalOptions.unitDisplayHeight;
        this.getHeightUnit = function() { return _HeightUnit; };
        this.setHeightUnit = function(/** VRS.Height */ value) {
            if(_HeightUnit !== value) {
                _HeightUnit = value;
                _Dispatcher.raise(_Events.heightUnitChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.Height ]);
            }
        };

        /** @type {VRS.Speed} @private */
        var _SpeedUnit = VRS.globalOptions.unitDisplaySpeed;
        this.getSpeedUnit = function() { return _SpeedUnit; };
        this.setSpeedUnit = function(/** VRS.Speed */ value) {
            if(_SpeedUnit !== value) {
                _SpeedUnit = value;
                _Dispatcher.raise(_Events.speedUnitChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.Speed ]);
            }
        };

        /** @type {boolean} @private */
        var _ShowVerticalSpeedPerSecond = VRS.globalOptions.unitDisplayVsiPerSecond;
        this.getShowVerticalSpeedPerSecond = function() { return _ShowVerticalSpeedPerSecond; };
        this.setShowVerticalSpeedPerSecond = function(/** boolean */ value) {
            if(_ShowVerticalSpeedPerSecond !== value) {
                _ShowVerticalSpeedPerSecond = value;
                _Dispatcher.raise(_Events.showVsiInSecondsChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.VsiSeconds ]);
            }
        };

        /** @type {boolean} @private */
        var _ShowAltitudeType = VRS.globalOptions.unitDisplayAltitudeType;
        this.getShowAltitudeType = function() { return _ShowAltitudeType; };
        this.setShowAltitudeType = function(/** boolean */ value) {
            if(_ShowAltitudeType !== value) {
                _ShowAltitudeType = value;
                _Dispatcher.raise(_Events.showAltitudeTypeChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.Height ]);
            }
        };

        /** @type {boolean} @private */
        var _ShowVerticalSpeedType = VRS.globalOptions.unitDisplayVerticalSpeedType;
        this.getShowVerticalSpeedType = function() { return _ShowVerticalSpeedType; };
        this.setShowVerticalSpeedType = function(/** boolean */ value) {
            if(_ShowVerticalSpeedType !== value) {
                _ShowVerticalSpeedType = value;
                _Dispatcher.raise(_Events.showVerticalSpeedTypeChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.Height ]);
            }
        };

        /** @type {number} @private */
        var _FlightLevelTransitionAltitude = VRS.globalOptions.unitDisplayFLTransitionAltitude;
        this.getFlightLevelTransitionAltitude = function() { return _FlightLevelTransitionAltitude; };
        this.setFlightLevelTransitionAltitude = function(/** number */ value) {
            if(_FlightLevelTransitionAltitude !== value) {
                _FlightLevelTransitionAltitude = value;
                _Dispatcher.raise(_Events.flAltitudeChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.FLTransitionAltitude ]);
            }
        };

        /** @type {VRS.Height} @private */
        var _FlightLevelTransitionHeightUnit = VRS.globalOptions.unitDisplayFLTransitionHeightUnit;
        this.getFlightLevelTransitionHeightUnit = function() { return _FlightLevelTransitionHeightUnit; };
        this.setFlightLevelTransitionHeightUnit = function(/** VRS.Height */ value) {
            if(_FlightLevelTransitionHeightUnit !== value) {
                _FlightLevelTransitionHeightUnit = value;
                _Dispatcher.raise(_Events.flTransUnitChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.FLTransitionHeightUnit ]);
            }
        };

        /** @type {VRS.Height} @private */
        var _FlightLevelHeightUnit = VRS.globalOptions.unitDisplayFLHeightUnit;
        this.getFlightLevelHeightUnit = function() { return _FlightLevelHeightUnit; };
        this.setFlightLevelHeightUnit = function(/** VRS.Height */ value) {
            if(_FlightLevelHeightUnit !== value) {
                _FlightLevelHeightUnit = value;
                _Dispatcher.raise(_Events.flHeightUnitChanged);
                _Dispatcher.raise(_Events.unitChanged, [ VRS.DisplayUnitDependency.FLHeightUnit ]);
            }
        };
        //endregion

        //region --Events exposed
        //noinspection JSUnusedGlobalSymbols
        this.hookDistanceUnitChanged =                      function(callback, forceThis) { return _Dispatcher.hook(_Events.distanceUnitChanged, callback, forceThis); };
        this.hookHeightUnitChanged =                        function(callback, forceThis) { return _Dispatcher.hook(_Events.heightUnitChanged, callback, forceThis); };
        this.hookSpeedUnitChanged =                         function(callback, forceThis) { return _Dispatcher.hook(_Events.speedUnitChanged, callback, forceThis); };
        this.hookShowVerticalSpeedPerSecondChanged =        function(callback, forceThis) { return _Dispatcher.hook(_Events.showVsiInSecondsChanged, callback, forceThis); };
        this.hookShowAltitudeTypeChanged =                  function(callback, forceThis) { return _Dispatcher.hook(_Events.showAltitudeTypeChanged, callback, forceThis); };
        this.hookShowVerticalSpeedTypeChanged =             function(callback, forceThis) { return _Dispatcher.hook(_Events.showVerticalSpeedTypeChanged, callback, forceThis); };
        this.hookFlightLevelTransitionAltitudeChanged =     function(callback, forceThis) { return _Dispatcher.hook(_Events.flAltitudeChanged, callback, forceThis); };
        this.hookFlightLevelTransitionHeightUnitChanged =   function(callback, forceThis) { return _Dispatcher.hook(_Events.flTransUnitChanged, callback, forceThis); };
        this.hookFlightLevelHeightUnitChanged =             function(callback, forceThis) { return _Dispatcher.hook(_Events.flHeightUnitChanged, callback, forceThis); };

        /**
         * Raised when any of the units have been changed.
         * @param {function(VRS.DisplayUnitDependency)} callback
         * @param {object} forceThis
         * @returns {Object}
         */
        this.hookUnitChanged = function(callback, forceThis) { return _Dispatcher.hook(_Events.unitChanged, callback, forceThis); };

        this.unhook = function(hookResult) { return _Dispatcher.unhook(hookResult); };
        //endregion

        //region -- Apply server settings
        if(VRS.serverConfig) {
            var config = VRS.serverConfig.get();
            if(config) {
                this.setDistanceUnit(config.InitialDistanceUnit);
                this.setHeightUnit(config.InitialHeightUnit);
                this.setSpeedUnit(config.InitialSpeedUnit);
            }
        }
        //endregion

        //region -- createOptionPane, getAltitudeUnitValues, getDistanceUnitValues, getSpeedUnitValues
        /**
         * Creates the option pane for the configuration of display units.
         * @param displayOrder
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(displayOrder)
        {
            var pane = new VRS.OptionPane({
                name:           'vrsUnitDisplayPreferences_' + _Name,
                titleKey:       'PaneUnits',
                displayOrder:   displayOrder
            });

            var distanceUnitValues = VRS.UnitDisplayPreferences.getDistanceUnitValues();
            var altitudeUnitValues = VRS.UnitDisplayPreferences.getAltitudeUnitValues();
            var speedUnitValues = VRS.UnitDisplayPreferences.getSpeedUnitValues();

            if(VRS.globalOptions.unitDisplayAllowConfiguration) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showVsiInSeconds',
                    labelKey:       'ShowVsiInSeconds',
                    getValue:       that.getShowVerticalSpeedPerSecond,
                    setValue:       that.setShowVerticalSpeedPerSecond,
                    saveState:      that.saveState
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showAltitudeType',
                    labelKey:       'ShowAltitudeType',
                    getValue:       that.getShowAltitudeType,
                    setValue:       that.setShowAltitudeType,
                    saveState:      that.saveState
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showVerticalSpeedType',
                    labelKey:       'ShowVerticalSpeedType',
                    getValue:       that.getShowVerticalSpeedType,
                    setValue:       that.setShowVerticalSpeedType,
                    saveState:      that.saveState
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'distanceUnit',
                    labelKey:       'Distances',
                    getValue:       that.getDistanceUnit,
                    setValue:       that.setDistanceUnit,
                    saveState:      that.saveState,
                    values:         distanceUnitValues
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'heightUnit',
                    labelKey:       'Heights',
                    getValue:       that.getHeightUnit,
                    setValue:       that.setHeightUnit,
                    saveState:      that.saveState,
                    values:         altitudeUnitValues
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'speedUnit',
                    labelKey:       'Speeds',
                    getValue:       that.getSpeedUnit,
                    setValue:       that.setSpeedUnit,
                    saveState:      that.saveState,
                    values:         speedUnitValues
                }));

                pane.addField(new VRS.OptionFieldNumeric({
                    name:           'flTransAltitude',
                    labelKey:       'FlightLevelTransitionAltitude',
                    getValue:       that.getFlightLevelTransitionAltitude,
                    setValue:       that.setFlightLevelTransitionAltitude,
                    saveState:      that.saveState,
                    inputWidth:     VRS.InputWidth.SixChar,
                    min:            -10000,
                    max:            99900,
                    decimals:       0,
                    step:           100,
                    keepWithNext:   true
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'flTransUnit',
                    getValue:       that.getFlightLevelTransitionHeightUnit,
                    setValue:       that.setFlightLevelTransitionHeightUnit,
                    saveState:      that.saveState,
                    values:         altitudeUnitValues
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'flHeightUnit',
                    labelKey:       'FlightLevelHeightUnit',
                    getValue:       that.getFlightLevelHeightUnit,
                    setValue:       that.setFlightLevelHeightUnit,
                    saveState:      that.saveState,
                    values:         altitudeUnitValues
                }));
            }

            return pane;
        };
        //endregion

        //region -- saveState, loadState, applyState, loadAndApplyState
        /**
         * Stores the current state of the object.
         */
        this.saveState = function()
        {
            VRS.configStorage.saveWithoutPrefix(persistenceKey(), createSettings());
        };

        /**
         * Returns the previously stored state of the object or the current state if no state was previously saved.
         * @returns {VRS_STATE_UNITDISPLAYPREFERENCES}
         */
        this.loadState = function()
        {
            var savedSettings = VRS.configStorage.loadWithoutPrefix(persistenceKey(), {});
            return $.extend(createSettings(), savedSettings);
        };

        /**
         * Applies a previously stored state to the object.
         * @param {VRS_STATE_UNITDISPLAYPREFERENCES} settings
         */
        this.applyState = function(settings)
        {
            that.setDistanceUnit(settings.distanceUnit);
            that.setHeightUnit(settings.heightUnit);
            that.setSpeedUnit(settings.speedUnit);
            that.setShowVerticalSpeedPerSecond(settings.vsiPerSecond);
            that.setFlightLevelTransitionAltitude(settings.flTransitionAlt);
            that.setFlightLevelTransitionHeightUnit(settings.flTransitionUnit);
            that.setFlightLevelHeightUnit(settings.flHeightUnit);
            that.setShowAltitudeType(settings.showAltType);
            that.setShowVerticalSpeedType(settings.showVsiType);
        };

        /**
         * Loads and applies a previously stored state.
         */
        this.loadAndApplyState = function()
        {
            that.applyState(that.loadState());
        };

        /**
         * Returns the key used to store the state.
         * @returns {string}
         */
        function persistenceKey()
        {
            return 'unitDisplayPreferences-' + that.getName();
        }

        /**
         * Returns the current state of the object.
         * @returns {VRS_STATE_UNITDISPLAYPREFERENCES}
         */
        function createSettings()
        {
            return {
                distanceUnit:       that.getDistanceUnit(),
                heightUnit:         that.getHeightUnit(),
                speedUnit:          that.getSpeedUnit(),
                vsiPerSecond:       that.getShowVerticalSpeedPerSecond(),
                flTransitionAlt:    that.getFlightLevelTransitionAltitude(),
                flTransitionUnit:   that.getFlightLevelTransitionHeightUnit(),
                flHeightUnit:       that.getFlightLevelHeightUnit(),
                showAltType:        that.getShowAltitudeType(),
                showVsiType:        that.getShowVerticalSpeedType()
            };
        }
        //endregion
    };
    //endregion

    //region VRS.UnitDisplayPreferences 'static' functions
    /**
     * Returns an array of every possible altitude unit value and their text keys.
     * @static
     * @returns {Array.<VRS.ValueText>}
     */
    VRS.UnitDisplayPreferences.getAltitudeUnitValues = function()
    {
        return [
            new VRS.ValueText({ value: VRS.Height.Feet,   textKey: 'Feet' }),
            new VRS.ValueText({ value: VRS.Height.Metre,  textKey: 'Metres' })
        ];
    };

    /**
     * Returns an array of every possible distance unit value and their text keys.
     * @static
     * @returns {Array.<VRS.ValueText>}
     */
    VRS.UnitDisplayPreferences.getDistanceUnitValues = function()
    {
        return [
            new VRS.ValueText({ value: VRS.Distance.Kilometre,    textKey: 'Kilometres' }),
            new VRS.ValueText({ value: VRS.Distance.NauticalMile, textKey: 'NauticalMiles' }),
            new VRS.ValueText({ value: VRS.Distance.StatuteMile,  textKey: 'StatuteMiles' })
        ];
    };

    /**
     * Returns an array of every possible speed unit value and their text keys.
     * @static
     * @returns {Array.<VRS.ValueText>}
     */
    VRS.UnitDisplayPreferences.getSpeedUnitValues = function()
    {
        return [
            new VRS.ValueText({ value: VRS.Speed.KilometresPerHour,   textKey: 'KilometresPerHour' }),
            new VRS.ValueText({ value: VRS.Speed.Knots,               textKey: 'Knots' }),
            new VRS.ValueText({ value: VRS.Speed.MilesPerHour,        textKey: 'MilesPerHour' })
        ];
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));