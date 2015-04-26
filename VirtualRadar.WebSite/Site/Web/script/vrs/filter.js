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
 * @fileoverview Code that is common to all condition-based filtering of lists.
 */

(function(VRS, $, undefined)
{
    //region Global options
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.filterLabelWidth = VRS.globalOptions.filterLabelWidth !== undefined ? VRS.globalOptions.filterLabelWidth : VRS.LabelWidth.Short;   // The default width for labels in the filter options UI.
    //endregion

    //region OneValueCondition, TwoValueCondition
    //region OneValueCondition
    /**
     * Describes a condition with single parameter that can be tested against a value.
     * @param {VRS.FilterCondition} condition           A VRS.FilterCondition that describes the condition being tested.
     * @param {bool=}               reverseCondition    True if the condition is to be reversed.
     * @param {*=} value The value to test against.
     * @constructor
     */
    VRS.OneValueCondition = function(condition, reverseCondition, value)
    {
        //region -- Properties
        /** @type {VRS.FilterCondition} */
        var _Condition = condition;
        this.getCondition = function() { return _Condition; };
        this.setCondition = function(/** VRS.FilterCondition */ value) { _Condition = value; };

        /** @type {bool} */
        var _ReverseCondition = reverseCondition;
        this.getReverseCondition = function() { return _ReverseCondition; };
        this.setReverseCondition = function(/** bool */ value) { _ReverseCondition = value; };

        /** @type {?} */
        var _Value = value;
        this.getValue = function() { return _Value; };
        this.setValue = function(value) { _Value = value; };
        //endregion

        //region -- equals, clone
        /**
         * Returns true of the object passed in has the same properties as this one.
         * @param {VRS.OneValueCondition} obj
         * @returns {boolean}
         */
        this.equals = function(obj)
        {
            return this.getCondition() === obj.getCondition() &&
                this.getReverseCondition() === obj.getReverseCondition() &&
                this.getValue() === obj.getValue();
        };

        /**
         * Returns a copy of this object.
         * @returns {VRS.OneValueCondition}
         */
        this.clone = function()
        {
            return new VRS.OneValueCondition(this.getCondition(), this.getReverseCondition(), this.getValue());
        };
        //endregion

        //region -- toSerialisableObject, applySerialisedObject
        /**
         * Creates a serialised copy of this object.
         * @returns {VRS_SERIALISED_ONEVALUECONDITION}
         */
        this.toSerialisableObject = function()
        {
            return {
                condition: _Condition,
                reversed: _ReverseCondition,
                value: _Value
            };
        };

        /**
         * Applies the serialised copy of an object to this object.
         * @param {VRS_SERIALISED_ONEVALUECONDITION} settings
         */
        this.applySerialisedObject = function(settings)
        {
            this.setCondition(settings.condition);
            this.setReverseCondition(settings.reversed);
            this.setValue(settings.value);
        };
        //endregion
    };
    //endregion

    //region TwoValueCondition
    /**
     * Describes a condition that takes two parameters to be tested against a value.
     * @param {VRS.FilterCondition} condition           A VRS.FilterCondition that describes the condition being tested.
     * @param {bool=}               reverseCondition    True if the condition is to be reversed.
     * @param {*=}                  value1              The first value.
     * @param {*=}                  value2              The second value.
     * @constructor
     */
    VRS.TwoValueCondition = function(condition, reverseCondition, value1, value2)
    {
        //region -- Properties
        /** @type {VRS.FilterCondition} */
        var _Condition = condition;
        this.getCondition = function() { return _Condition; };
        this.setCondition = function(/** VRS.FilterCondition */ value) { _Condition = value; };

        /** @type {bool} */
        var _ReverseCondition = reverseCondition;
        this.getReverseCondition = function() { return _ReverseCondition; };
        this.setReverseCondition = function(/** bool */ value) { _ReverseCondition = value; };

        /** @type {?} */
        var _Value1 = value1;
        this.getValue1 = function() { return _Value1; };
        this.setValue1 = function(value) {
            if(value !== _Value1) {
                _Value1 = value;
                orderValues();
            }
        };

        /** @type {?} */
        var _Value2 = value2;
        this.getValue2 = function() { return _Value2; };
        this.setValue2 = function(value) {
            if(value !== _Value2) {
                _Value2 = value;
                orderValues();
            }
        };

        var _Value1IsLow;
        /**
         * Returns the lower of the two values.
         * @returns {?}
         */
        this.getLowValue  = function() { return _Value1IsLow ? _Value1 : _Value2; };
        /**
         * Gets the higher of the two values.
         * @returns {?}
         */
        this.getHighValue = function() { return _Value1IsLow ? _Value2 : _Value1; };
        //endregion

        //region -- orderValues, equals, clone
        /**
         * Records which of the two values is lower.
         */
        function orderValues()
        {
            if(_Value1 === undefined || _Value2 === undefined) _Value1IsLow = true;
            else _Value1IsLow = _Value1 <= _Value2;
        }

        /**
         * Returns true if the object passed in has the same properties as this object.
         * @param {VRS.TwoValueCondition} obj
         * @returns {boolean}
         */
        this.equals = function(obj)
        {
            return this.getCondition() === obj.getCondition() &&
                this.getReverseCondition() === obj.getReverseCondition() &&
                this.getValue1() === obj.getValue1() &&
                this.getValue2() === obj.getValue2();
        };

        /**
         * Returns a copy of this object.
         * @returns {VRS.TwoValueCondition}
         */
        this.clone = function()
        {
            return new VRS.TwoValueCondition(this.getCondition(), this.getReverseCondition(), this.getValue1(), this.getValue2());
        };
        //endregion

        //region -- toSerialisableObject, applySerialisedObject
        /**
         * Returns a serialisable object that describes this object.
         * @returns {VRS_SERIALISED_TWOVALUECONDITION}
         */
        this.toSerialisableObject = function()
        {
            return {
                condition: _Condition,
                reversed: _ReverseCondition,
                value1: _Value1,
                value2: _Value2
            };
        };

        /**
         * Applies a serialised group of settings to this object.
         * @param {VRS_SERIALISED_TWOVALUECONDITION} settings
         */
        this.applySerialisedObject = function(settings)
        {
            this.setCondition(settings.condition);
            this.setReverseCondition(settings.reversed);
            this.setValue1(settings.value1);
            this.setValue2(settings.value2);
        };
        //endregion
    };
    //endregion
    //endregion

    //region FilterPropertyTypeHandler
    /**
     * An object that matches a filter property type (boolean, numeric etc.) with the value filter required to test the
     * a property of this type and details of the UI required to collect those filter's parameters from the user.
     * @param {Object}                                               settings
     * @param {VRS.FilterPropertyType}                               settings.type                       The VRS.FilterPropertyType that this object handles.
     * @param {function():VRS_CONDITION[]}                          [settings.getConditions]             Returns an array of condition enum values and their text keys.
     * @param {function():VRS_ANY_VALUECONDITION}                    settings.createValueCondition       Returns a new condition and values object.
     * @param {function(*, VRS_ANY_VALUECONDITION):bool}            [settings.valuePassesCallback]       Mandatory if useSingleValueEquals is false, takes a value and a value/condition object and returns true if the value passes the filter.
     * @param {bool}                                                [settings.useSingleValueEquals]      True if the aircraft property is just compared as equal to the filter. Pre-fills 'valuePassesCallback'.
     * @param {bool}                                                [settings.useValueBetweenRange]      True if the aircraft property is to lie between a range of values. Pre-fills 'valuePassesCallback'.
     * @param {function(string, VRS_ANY_VALUECONDITION,
     * VRS.FilterPropertyHandler, function()):VRS.OptionField[]}     settings.createOptionFieldsCallback Takes a label key, a value/condition object, a filter property handler and a saveState callback and returns an array of option fields.
     * @param {function(string):*}                                   settings.parseString                Parses a string into the correct type. Returns undefined or null if the string is unparseable.
     * @param {function(*):string}                                   settings.toQueryString              Formats a value into a query string value. Returns null or undefined if the value is empty / missing.
     * @param {function(VRS_ANY_VALUECONDITION):bool}               [settings.passEmptyValues]           Returns true if empty values are to be passed to the server. Defaults to function that returns false.
     * @constructor
     */
    VRS.FilterPropertyTypeHandler = function(settings)
    {
        if(!settings) throw 'You must supply a settings object';
        if(settings.useSingleValueEquals) settings.valuePassesCallback = singleValueEquals;
        if(settings.useValueBetweenRange) settings.valuePassesCallback = valueBetweenRange;

        if(!settings.type || !VRS.enumHelper.getEnumName(VRS.FilterPropertyType, settings.type)) throw 'You must supply a property type';
        if(!settings.createValueCondition) throw 'You must supply a createValueCondition';
        if(!settings.valuePassesCallback) throw 'You must supply an valuePassesCallback';
        if(!settings.createOptionFieldsCallback) throw 'You must supply a createOptionFieldsCallback';

        var that = this;
        this.type = settings.type;
        this.getConditions = settings.getConditions;
        this.createValueCondition = settings.createValueCondition;
        this.createOptionFieldsCallback = settings.createOptionFieldsCallback;
        this.valuePassesCallback = settings.valuePassesCallback;
        this.parseString = settings.parseString;
        this.passEmptyValues = settings.passEmptyValues || function() { return false; };
        this.toQueryString = settings.toQueryString;

        //region -- singleValueEquals, valueBetweenRange
        /**
         * Returns true if a value is equal to the value held by the parameters.
         * @param {*}                       value
         * @param {VRS.OneValueCondition}   valueCondition
         * @returns {boolean}
         */
        function singleValueEquals(value, valueCondition)
        {
            var filterValue = valueCondition.getValue();
            var result = filterValue === undefined;
            if(!result) {
                switch(valueCondition.getCondition()) {
                    case VRS.FilterCondition.Equals:  result = filterValue === value; break;
                    default:                          throw 'Invalid condition ' + valueCondition.getCondition() + ' for a ' + that.type + ' filter type';
                }
                if(valueCondition.getReverseCondition()) result = !result;
            }
            return result;
        }

        /**
         * Returns true if the value is between the values held by the parameters.
         * @param {*}                       value
         * @param {VRS.TwoValueCondition}   valueCondition
         * @returns {boolean}
         */
        function valueBetweenRange(value, valueCondition)
        {
            var filterLowValue = valueCondition.getLowValue();
            var filterHighValue = valueCondition.getHighValue();
            var result = filterLowValue === undefined && filterHighValue === undefined;
            if(!result) {
                switch(valueCondition.getCondition()) {
                    case VRS.FilterCondition.Between:
                        if(filterLowValue === undefined)        result = value <= filterHighValue;
                        else if(filterHighValue === undefined)  result = value >= filterLowValue;
                        else                                    result = value >= filterLowValue && value <= filterHighValue;
                        break;
                    default:
                        throw 'Invalid condition ' + valueCondition.getCondition() + ' for range property types';
                }
                if(valueCondition.getReverseCondition()) result = !result;
            }

            return result;
        }
        //endregion

        //region -- getConditionComboBoxValues, encodeConditionAndReverseCondition, decodeConditionAndReverseCondition
        /**
         * Returns an array of objects describing all of the filter conditions ready for use in a combo box drop-down.
         * @returns {VRS.ValueText[]}
         */
        this.getConditionComboBoxValues = function()
        {
            var result = [];
            $.each(this.getConditions(), function(idx, condition) {
                result.push(new VRS.ValueText({
                    value: that.encodeConditionAndReverseCondition(condition.condition, condition.reverseCondition),
                    textKey: condition.labelKey
                }));
            });
            return result;
        };

        /**
         * Returns the condition and, if reverseCondition is true, a suffix to distinguish it as the reverse condition.
         * @param {VRS.FilterCondition} condition
         * @param {bool} reverseCondition
         * @returns {string}
         */
        this.encodeConditionAndReverseCondition = function(condition, reverseCondition)
        {
            return condition + (reverseCondition ? '_reversed' : '');
        };

        /**
         * Takes a string encoded by encodeConditionAndReverseCondition and returns the original condition and reverseCondition flag.
         * @param {string} encodedConditionAndReverse
         * @returns {VRS_CONDITION}
         */
        this.decodeConditionAndReverseCondition = function(encodedConditionAndReverse)
        {
            var markerPosn = encodedConditionAndReverse.indexOf('_reversed');
            var condition = /** @type {VRS.FilterCondition} */
                markerPosn === -1 ? encodedConditionAndReverse : encodedConditionAndReverse.substr(0, markerPosn);
            var reversed = markerPosn !== -1;
            return {
                condition: condition,
                reverseCondition: reversed
            };
        };
        //endregion

        //region -- encodeCondition, applyEncodedCondition
        /**
         * Takes a value/condition and returns the condition and reverse condition values encoded as a single string.
         * @param {VRS_ANY_VALUECONDITION} valueCondition
         * @returns {string}
         */
        this.encodeCondition = function(valueCondition)
        {
            return that.encodeConditionAndReverseCondition(valueCondition.getCondition(), valueCondition.getReverseCondition());
        };

        /**
         * Takes a string that holds an encoded condition and the reverse condition flag and applies them to a value/condition object.
         * @param {VRS_ANY_VALUECONDITION}  valueCondition
         * @param {string}                  encodedCondition
         */
        this.applyEncodedCondition = function(valueCondition, encodedCondition)
        {
            var decodedCondition = that.decodeConditionAndReverseCondition(encodedCondition);
            valueCondition.setCondition(decodedCondition.condition);
            valueCondition.setReverseCondition(decodedCondition.reverseCondition);
        };
        //endregion
    };
    //endregion

    //region VRS.filterPropertyTypeHandlers - pre-built filter property type handlers
    /**
     * This is the list of pre-built (and potentially 3rd party) handlers for filter property types that describe how to
     * ask for the filter parameters used to test a property of a given type.
     * @type {Object.<VRS.FilterPropertyType, VRS.FilterPropertyTypeHandler>}
     */
    VRS.filterPropertyTypeHandlers = VRS.filterPropertyTypeHandlers || {};

    //region -- DateRange
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.DateRange] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.DateRange,
        createValueCondition:       function() { return new VRS.TwoValueCondition(VRS.FilterCondition.Between); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Between, reverseCondition: false, labelKey: 'Between' },
            { condition: VRS.FilterCondition.Between, reverseCondition: true,  labelKey: 'NotBetween' }
        ]},
        useValueBetweenRange:       true,
        createOptionFieldsCallback: function(/** string */ labelKey, /** VRS.TwoValueCondition */ twoValueCondition, /** VRS.FilterPropertyHandler */ handler, /** function() */ saveState) {
            var self = this;
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name:           'label',
                    labelKey:       labelKey,
                    keepWithNext:   true,
                    labelWidth:     VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name:           'condition',
                    getValue:       function() { return self.encodeCondition(twoValueCondition); },
                    setValue:       function(value) { self.applyEncodedCondition(twoValueCondition, value); },
                    saveState:      saveState,
                    values:         conditionValues,
                    keepWithNext:   true
                }),
                new VRS.OptionFieldDate({
                    name:           'value1',
                    getValue:       function() { return twoValueCondition.getValue1(); },
                    setValue:       function(value) { twoValueCondition.setValue1(value); },
                    inputWidth:     handler.inputWidth,
                    saveState:      saveState,
                    keepWithNext:   true
                }),
                new VRS.OptionFieldLabel({
                    name:           'valueSeparator',
                    labelKey:       'SeparateTwoValues',
                    keepWithNext:   true
                }),
                new VRS.OptionFieldDate({
                    name:           'value2',
                    getValue:       function() { return twoValueCondition.getValue2(); },
                    setValue:       function(value) { twoValueCondition.setValue2(value); },
                    inputWidth:     handler.inputWidth,
                    saveState:      saveState
                })
            ];
        },
        parseString: function(text) {
            return VRS.dateHelper.parse(text);
        },
        toQueryString: function(value) {
            return value ? VRS.dateHelper.toIsoFormatString(value, true, true) : null;
        }
    });
    //endregion

    //region -- EnumMatch
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.EnumMatch] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.EnumMatch,
        createValueCondition:       function() { return new VRS.OneValueCondition(VRS.FilterCondition.Equals); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Equals, reverseCondition: false, labelKey: 'Equals' },
            { condition: VRS.FilterCondition.Equals, reverseCondition: true,  labelKey: 'NotEquals' }
        ]},
        useSingleValueEquals:       true,
        createOptionFieldsCallback: function(/** string */ labelKey, /** VRS.OneValueCondition */ oneValueCondition, /** VRS.FilterPropertyHandler */ handler, /** function() */ saveState) {
            var self = this;
            var comboBoxValues = handler.getEnumValues();
            if(!comboBoxValues) throw 'Property type handlers for enum types must supply a getEnumValues callback';
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name:           'label',
                    labelKey:       labelKey,
                    keepWithNext:   true,
                    labelWidth:     VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name:           'condition',
                    getValue:       function() { return self.encodeCondition(oneValueCondition); },
                    setValue:       function(value) { self.applyEncodedCondition(oneValueCondition, value); },
                    saveState:      saveState,
                    values:         conditionValues,
                    keepWithNext:   true
                }),
                new VRS.OptionFieldComboBox({
                    name:           'value',
                    getValue:       function() { return oneValueCondition.getValue(); },
                    setValue:       function(value) { oneValueCondition.setValue(value); },
                    saveState:      saveState,
                    values:         comboBoxValues
                })
            ];
        },
        parseString: function(text) { return text !== undefined && text !== null ? text : undefined; },
        toQueryString: function(value) { return value; }
    });
    //endregion

    //region -- NumberRange
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.NumberRange] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.NumberRange,
        createValueCondition:       function() { return new VRS.TwoValueCondition(VRS.FilterCondition.Between); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Between, reverseCondition: false, labelKey: 'Between' },
            { condition: VRS.FilterCondition.Between, reverseCondition: true,  labelKey: 'NotBetween' }
        ]},
        useValueBetweenRange:       true,
        createOptionFieldsCallback: function(/** string */ labelKey, /** VRS.TwoValueCondition */ twoValueCondition, /** VRS.FilterPropertyHandler */ handler, /** function() */ saveState) {
            var self = this;
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name:           'label',
                    labelKey:       labelKey,
                    keepWithNext:   true,
                    labelWidth:     VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name:           'condition',
                    getValue:       function() { return self.encodeCondition(twoValueCondition); },
                    setValue:       function(value) { self.applyEncodedCondition(twoValueCondition, value); },
                    saveState:      saveState,
                    values:         conditionValues,
                    keepWithNext:   true
                }),
                new VRS.OptionFieldNumeric({
                    name:           'value1',
                    getValue:       function() { return twoValueCondition.getValue1(); },
                    setValue:       function(value) { twoValueCondition.setValue1(value); },
                    inputWidth:     handler.inputWidth,
                    saveState:      saveState,
                    min:            handler.minimumValue,
                    max:            handler.maximumValue,
                    decimals:       handler.decimalPlaces,
                    keepWithNext:   true
                }),
                new VRS.OptionFieldLabel({
                    name:           'valueSeparator',
                    labelKey:       'SeparateTwoValues',
                    keepWithNext:   true
                }),
                new VRS.OptionFieldNumeric({
                    name:           'value2',
                    getValue:       function() { return twoValueCondition.getValue2(); },
                    setValue:       function(value) { twoValueCondition.setValue2(value); },
                    inputWidth:     handler.inputWidth,
                    saveState:      saveState,
                    min:            handler.minimumValue,
                    max:            handler.maximumValue,
                    decimals:       handler.decimalPlaces
                })
            ];
        },
        parseString: function(text) {
            var result;
            try {
                if(text !== null && text !== undefined) {
                    result = parseFloat(text);
                    if(isNaN(result)) result = undefined;
                }
            } catch(ex) {
                result = undefined;
            }
            return result;
        },
        toQueryString: function(value) { return value || value === 0 ? value.toString() : null; }
    });
    //endregion

    //region -- OnOff
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.OnOff] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.OnOff,
        createValueCondition:       function() { return new VRS.OneValueCondition(VRS.FilterCondition.Equals, false, true); },        // Preset the option to true.
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Equals, reverseCondition: false, labelKey: 'Equals' }
        ]},
        useSingleValueEquals:       true,
        createOptionFieldsCallback: function(/** string */ labelKey, /** VRS.OneValueCondition */ oneValueCondition, /** VRS.FilterPropertyHandler */ handler, /** function() */ saveState) { return [
            new VRS.OptionFieldCheckBox({
                name:           'onOff',
                labelKey:       labelKey,
                getValue:       function() { return oneValueCondition.getValue(); },
                setValue:       function(value) { oneValueCondition.setValue(value); },
                saveState:      saveState
            })
        ];},
        parseString: function(text) {
            var result;
            if(text) {
                switch(text.toUpperCase()) {
                    case 'TRUE':
                    case 'YES':
                    case 'ON':
                    case '1':
                        result = true;
                        break;
                    case 'FALSE':
                    case 'NO':
                    case 'OFF':
                    case '0':
                        result = false;
                        break;
                }
            }
            return result;
        },
        toQueryString: function(value) {
            return value !== undefined && value !== null ? value ? 1 : '0' : null;
        }
    });
    //endregion

    //region -- TextMatch
    VRS.filterTextMatchSettings = {
        type:                       VRS.FilterPropertyType.TextMatch,
        createValueCondition:       function() { return new VRS.OneValueCondition(VRS.FilterCondition.Contains); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Contains, reverseCondition: false, labelKey: 'Contains' },
            { condition: VRS.FilterCondition.Contains, reverseCondition: true,  labelKey: 'NotContains' },
            { condition: VRS.FilterCondition.Equals,   reverseCondition: false, labelKey: 'Equals' },
            { condition: VRS.FilterCondition.Equals,   reverseCondition: true,  labelKey: 'NotEquals' },
            { condition: VRS.FilterCondition.Starts,   reverseCondition: false, labelKey: 'StartsWith' },
            { condition: VRS.FilterCondition.Starts,   reverseCondition: true,  labelKey: 'NotStartsWith' },
            { condition: VRS.FilterCondition.Ends,     reverseCondition: false, labelKey: 'EndsWith' },
            { condition: VRS.FilterCondition.Ends,     reverseCondition: true,  labelKey: 'NotEndsWith' }
        ]},
        valuePassesCallback:        function(value, /** VRS.OneValueCondition */ oneValueCondition, /** VRS_OPTIONS_AIRCRAFTFILTER */ options) {
            var conditionValue = oneValueCondition.getValue();
            var result = conditionValue === undefined;
            if(!result) {
                var ignoreCase = options && options.caseInsensitive;
                switch(oneValueCondition.getCondition()) {
                    case VRS.FilterCondition.Contains:  result = VRS.stringUtility.contains(value, conditionValue, ignoreCase); break;
                    case VRS.FilterCondition.Equals:    result = VRS.stringUtility.equals(value, conditionValue, ignoreCase); break;
                    case VRS.FilterCondition.Starts:    result = VRS.stringUtility.startsWith(value, conditionValue, ignoreCase); break;
                    case VRS.FilterCondition.Ends:      result = VRS.stringUtility.endsWith(value, conditionValue, ignoreCase); break;
                    default:                            throw 'Invalid condition ' + oneValueCondition.getCondition() + ' for text match filters';
                }
                if(oneValueCondition.getReverseCondition()) result = !result;
            }
            return result;
        },
        createOptionFieldsCallback: function(/** string */ labelKey, /** VRS.OneValueCondition */ oneValueCondition, /** VRS.FilterPropertyHandler */ handler, /** function() */ saveState) {
            var self = this;
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name:           'label',
                    labelKey:       labelKey,
                    keepWithNext:   true,
                    labelWidth:     VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name:           'condition',
                    getValue:       function() { return self.encodeCondition(oneValueCondition); },
                    setValue:       function(value) { self.applyEncodedCondition(oneValueCondition, value); },
                    saveState:      saveState,
                    values:         conditionValues,
                    keepWithNext:   true
                }),
                new VRS.OptionFieldTextBox({
                    name:           'value',
                    getValue:       function() { return oneValueCondition.getValue(); },
                    setValue:       function(value) { oneValueCondition.setValue(value); },
                    inputWidth:     handler.inputWidth,
                    saveState:      saveState,
                    upperCase:      handler.isUpperCase,
                    lowerCase:      handler.isLowerCase
                })
            ];
        },
        parseString: function(text) { return text !== null ? text : undefined; },
        toQueryString: function(value) { return value; },
        passEmptyValues: function(valueCondition) {
            // We want to pass empty strings if they're searching for null/empty strings
            return valueCondition.getCondition() === VRS.FilterCondition.Equals;
        }
    };
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.TextMatch] = new VRS.FilterPropertyTypeHandler(VRS.filterTextMatchSettings);
    //endregion

    //region -- TextListMatch
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.TextListMatch] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.TextListMatch,
        valuePassesCallback:        function(/** String[] */ value, /** VRS.OneValueCondition */ oneValueCondition, /** VRS_OPTIONS_AIRCRAFTFILTER */ options) {
            var conditionValue = /** @type {String} */ oneValueCondition.getValue();
            var result = conditionValue === undefined;
            if(!result) {
                var ignoreCase = options && options.caseInsensitive;
                var condition = oneValueCondition.getCondition();
                var length = value.length;
                for(var i = 0;!result && i < length;++i) {
                    var item = value[i];
                    switch(condition) {
                        case VRS.FilterCondition.Contains:  result = VRS.stringUtility.contains(item, conditionValue, ignoreCase); break;
                        case VRS.FilterCondition.Equals:    result = VRS.stringUtility.equals(item, conditionValue, ignoreCase); break;
                        case VRS.FilterCondition.Starts:    result = VRS.stringUtility.startsWith(item, conditionValue, ignoreCase); break;
                        case VRS.FilterCondition.Ends:      result = VRS.stringUtility.endsWith(item, conditionValue, ignoreCase); break;
                        default:                            throw 'Invalid condition ' + condition + ' for text match filters';
                    }
                }
                if(oneValueCondition.getReverseCondition()) result = !result;
            }
            return result;
        },
        createValueCondition:       VRS.filterTextMatchSettings.createValueCondition,
        getConditions:              VRS.filterTextMatchSettings.getConditions,
        createOptionFieldsCallback: VRS.filterTextMatchSettings.createOptionFieldsCallback,
        parseString:                VRS.filterTextMatchSettings.parseString,
        toQueryString:              VRS.filterTextMatchSettings.toQueryString,
        passEmptyValues:            VRS.filterTextMatchSettings.passEmptyValues
    });
    //endregion
    //endregion

    //region FilterPropertyHandler
    /**
     * The base for objects that bring together a property and its type, and describe the ranges that the filters can be
     * set to, its on-screen description and so on.
     * @param {Object}                                      subclassSettings                    The object that a derivee must provide that tells us how to behave.
     * @param {VRS_ANY_FILTERPROPERTY}                      subclassSettings.property           The property that this object handles.
     * @param {Object}                                      subclassSettings.propertyEnumObject The property is expected to be an enum value - this is the enum object that holds all possible values of the property.
     * @param {VRS.FilterPropertyType}                      subclassSettings.type               The VRS.FilterPropertyType of the filter used by this object.
     * @param {string}                                      subclassSettings.labelKey           The key of the translated text for the filter's label.
     * @param {function():VRS.ValueText[]}                 [subclassSettings.getEnumValues]     The callback (mandatory for enum types) that returns the list of all possible enum values and their descriptions.
     * @param {bool}                                       [subclassSettings.isUpperCase]       True if string values are to be converted to upper-case on data-entry.
     * @param {bool}                                       [subclassSettings.isLowerCase]       True if string values are to be converted to lower-case on data-entry.
     * @param {number}                                     [subclassSettings.minimumValue]      The minimum value for numeric range fields.
     * @param {number}                                     [subclassSettings.maximumValue]      The maximum value for numeric range fields.
     * @param {number}                                     [subclassSettings.decimalPlaces]     The number of decimals for numeric range fields.
     * @param {VRS.InputWidth}                             [subclassSettings.inputWidth]        The optional width to use on input fields.
     * @param {function(VRS_ANY_VALUECONDITION):bool}      [subclassSettings.isServerFilter]    Returns true if the filter is supported server-side, false if it is not. Default returns true if serverFilterName is present, false if it is not.
     * @param {string}                                     [subclassSettings.serverFilterName]  The name to use when representing the filter in a query to the server.
     * @param {function(*, VRS.UnitDisplayPreferences):*}  [subclassSettings.normaliseValue]    An optional method that takes a value and a unit display preferences and converts it from the user's preferred unit to the unit expected by the server. Default just returns the value unchanged.
     * @param {VRS.FilterCondition}                        [subclassSettings.defaultCondition]  The initial condition to offer when the filter is added to the user interface.
     * @constructor
     */
    VRS.FilterPropertyHandler = function(subclassSettings)
    {
        var that = this;

        if(!subclassSettings) throw 'You must supply a settings object';
        if(!subclassSettings.property || !VRS.enumHelper.getEnumName(subclassSettings.propertyEnumObject, subclassSettings.property)) throw 'You must supply a valid property';
        if(!subclassSettings.type || !VRS.enumHelper.getEnumName(VRS.FilterPropertyType, subclassSettings.type)) throw 'You must supply a property type';
        if(!subclassSettings.labelKey) throw 'You must supply a labelKey';

        //noinspection JSUnusedGlobalSymbols
        this.property = subclassSettings.property;
        this.type = subclassSettings.type;
        this.labelKey = subclassSettings.labelKey;
        this.getValueCallback = subclassSettings.getValueCallback;
        this.getEnumValues = subclassSettings.getEnumValues;
        this.isUpperCase = subclassSettings.isUpperCase;
        this.isLowerCase = subclassSettings.isLowerCase;
        this.minimumValue = subclassSettings.minimumValue;
        this.maximumValue = subclassSettings.maximumValue;
        this.decimalPlaces = subclassSettings.decimalPlaces;
        this.inputWidth = subclassSettings.inputWidth === undefined ? VRS.InputWidth.Auto : subclassSettings.inputWidth;
        this.serverFilterName = subclassSettings.serverFilterName;
        this.isServerFilter = subclassSettings.isServerFilter || function() { return !!subclassSettings.serverFilterName; };
        this.normaliseValue = subclassSettings.normaliseValue || function(value) { return value; };
        this.defaultCondition = subclassSettings.defaultCondition;

        /**
         * Returns the handler for the property's type.
         * @returns {VRS.FilterPropertyTypeHandler}
         */
        this.getFilterPropertyTypeHandler = function()
        {
            return VRS.filterPropertyTypeHandlers[that.type];
        };
    };
    //endregion

    //region Filter
    /**
     * The base for more specialised filters. A filter joins together a property identifier and an object that carries a
     * condition and values to use with the condition in the filter test. The classes derived from this one specialise
     * the property but they all share the concept of joining a property and a value/condition together.
     * @param {Object}                                                              subclassSettings                        A collection of settings that the subclass must supply. Influences the behaviour of the base.
     * @param {VRS_ANY_FILTERPROPERTY}                                              subclassSettings.property               The property that is to be associated with the filter.
     * @param {VRS_ANY_VALUECONDITION}                                              subclassSettings.valueCondition         The object that describes the condition and values to compare against a property.
     * @param {Object}                                                              subclassSettings.propertyEnumObject     The property is expected to belong to an enum - this is the enum object that holds all possible values of property.
     * @param {Object.<VRS_ANY_FILTERPROPERTY, VRS.FilterPropertyHandler>}          subclassSettings.filterPropertyHandlers An associative array of property values and their associated FilterPropertyHandler-based objects.
     * @param {function(VRS_ANY_FILTERPROPERTY,VRS_ANY_VALUECONDITION):VRS.Filter}  subclassSettings.cloneCallback          A method that can create a clone of the filter derivee.
     * @constructor
     */
    VRS.Filter = function(subclassSettings)
    {
        var that = this;

        //region -- Sanity checks
        if(!subclassSettings) throw 'The derivee must supply a subclassSettings object';
        if(!subclassSettings.property || !VRS.enumHelper.getEnumName(subclassSettings.propertyEnumObject, subclassSettings.property)) throw 'You must supply a valid property';
        if(!subclassSettings.valueCondition) throw 'You must supply a filter';
        //endregion

        //region -- Properties
        /** @type {VRS_ANY_FILTERPROPERTY} */
        var _Property = subclassSettings.property;
        this.getProperty = function() { return _Property; };
        this.setProperty = function(/** VRS_ANY_FILTERPROPERTY */ value) {
            if(!value || !VRS.enumHelper.getEnumName(subclassSettings.propertyEnumObject, value)) throw 'Cannot set property of "' + value + '", it is not a valid property';
            _Property = value;
        };

        /** @type {VRS_ANY_VALUECONDITION} */
        var _ValueCondition = subclassSettings.valueCondition;
        this.getValueCondition = function() { return _ValueCondition; };
        //noinspection JSUnusedGlobalSymbols
        this.setValueCondition = function(/** VRS_ANY_VALUECONDITION */ value) {
            if(!value) throw 'You cannot set the value/condition to null or undefined';
            _ValueCondition = value;
        };

        /**
         * Returns the filter's property handler.
         * @returns {FilterPropertyHandler}
         */
        this.getPropertyHandler = function()
        {
            return subclassSettings.filterPropertyHandlers[_Property];
        };
        //endregion

        //region -- equals, clone
        /**
         * Returns true if this object has the same property values as another object.
         * @param {*} obj
         * @returns {boolean}
         */
        this.equals = function(obj)
        {
            return this.getProperty() === obj.getProperty() && this.getValueCondition().equals(obj.getValueCondition());
        };

        /**
         * Returns a copy of this object.
         * @returns {VRS.Filter}
         */
        this.clone = function()
        {
            return subclassSettings.cloneCallback(_Property, _ValueCondition.clone());
        };
        //endregion

        //region -- toSerialisableObject, applySerialisedObject
        /**
         * Creates a serialisable copy of the filter.
         * @returns {VRS_SERIALISED_FILTER}
         */
        this.toSerialisableObject = function()
        {
            return {
                property:       _Property,
                valueCondition: _ValueCondition.toSerialisableObject()
            };
        };

        /**
         * Applies serialised settings to this object.
         * @param {VRS_SERIALISED_FILTER} settings
         */
        this.applySerialisedObject = function(settings)
        {
            that.setProperty(settings.property);
            _ValueCondition.applySerialisedObject(settings.valueCondition);
        };
        //endregion

        //region -- createOptionPane
        /**
         * Creates an object pane for the condition and parameters to the condition.
         * @param {function()} saveState The method to call when the condition has been changed by the user.
         * @returns {VRS.OptionPane}
         */
        this.createOptionPane = function(saveState)
        {
            var propertyHandler = that.getPropertyHandler();
            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            var labelKey = propertyHandler.labelKey;

            return new VRS.OptionPane({
                name:   'filter',
                fields: typeHandler.createOptionFieldsCallback(labelKey, _ValueCondition, propertyHandler, saveState)
            });
        };
        //endregion

        /**
         * Adds parameters to a query string parameters object to represent the filter property, condition and values.
         * @param {object}                      query                   The query object whose properties will be expanded into a query string.
         * @param {VRS.UnitDisplayPreferences}  unitDisplayPreferences  The unit display preferences used to normalise all filter values.
         */
        this.addToQueryParameters = function(query, unitDisplayPreferences)
        {
            /** @type {VRS.FilterPropertyHandler} */
            var propertyHandler = subclassSettings.filterPropertyHandlers[_Property];
            if(propertyHandler && propertyHandler.isServerFilter(_ValueCondition)) {
                var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
                var filterName = propertyHandler.serverFilterName;
                var reverse = _ValueCondition.getReverseCondition() ? 'N' : '';
                var passEmptyValue = typeHandler.passEmptyValues(_ValueCondition);

                var addQueryString = function(suffix, value) {
                    if(passEmptyValue || value !== undefined) {
                        value = propertyHandler.normaliseValue(value, unitDisplayPreferences);
                        if(propertyHandler.decimalPlaces === 0) value = Math.round(value);
                        var textValue = typeHandler.toQueryString(value);
                        if(passEmptyValue || textValue) query[filterName + suffix + reverse] = textValue || '';
                    }
                };

                switch(_ValueCondition.getCondition()) {
                    case VRS.FilterCondition.Between:
                        addQueryString('L', _ValueCondition.getLowValue());
                        addQueryString('U', _ValueCondition.getHighValue());
                        break;
                    case VRS.FilterCondition.Contains:
                        addQueryString('C', _ValueCondition.getValue());
                        break;
                    case VRS.FilterCondition.Ends:
                        addQueryString('E', _ValueCondition.getValue());
                        break;
                    case VRS.FilterCondition.Equals:
                        addQueryString('Q', _ValueCondition.getValue());
                        break;
                    case VRS.FilterCondition.Starts:
                        addQueryString('S', _ValueCondition.getValue());
                        break;
                    default:
                        throw 'Unknown condition ' + _ValueCondition.getCondition();
                }
            }
        };
    };
    //endregion

    //region FilterHelper
    /**
     * The base for helper objects that can deal with common routine tasks when working with filters.
     * @param {Object}                                                              subclassSettings
     * @param {Object}                                                              subclassSettings.propertyEnumObject         The enum object from which all filter properties are obtained.
     * @param {Object.<*, VRS.FilterPropertyHandler>}                               subclassSettings.filterPropertyHandlers     An associative array of property enum values against the handler for the property.
     * @param {function(VRS.FilterPropertyHandler, VRS_ANY_CONDITION):VRS.Filter}   subclassSettings.createFilterCallback       A method that creates a filter of the correct type.
     * @constructor
     */
    VRS.FilterHelper = function(subclassSettings)
    {
        if(!subclassSettings) throw 'You must supply the subclass settings';
        var that = this;

        //region -- createFilter
        /**
         * Creates a new Filter for the property passed across.
         * @param {*} property A property value.
         * @returns {VRS.Filter}
         */
        this.createFilter = function(property)
        {
            var propertyHandler = subclassSettings.filterPropertyHandlers[property];
            if(!propertyHandler) throw 'There is no property handler for ' + property + ' properties';

            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            if(!typeHandler) throw 'There is no type handler of ' + propertyHandler.type + ' for ' + property + ' properties';

            var valueCondition = typeHandler.createValueCondition();
            if(propertyHandler.defaultCondition) valueCondition.setCondition(propertyHandler.defaultCondition);

            return subclassSettings.createFilterCallback(propertyHandler, valueCondition);
        };
        //endregion

        //region -- serialiseFiltersList, buildValidFiltersList
        /**
         * Converts a list of filters to an array of serialised objects that can be saved in an object's state.
         * @param {VRS.Filter[]} filters
         * @returns {VRS_SERIALISED_FILTER[]}
         */
        this.serialiseFiltersList = function(filters)
        {
            var result = [];
            $.each(filters, function(idx, filter) { result.push(filter.toSerialisableObject()); });

            return result;
        };

        /**
         * Removes invalid serialised filters from a list of serialised filters, presumably from a previous session's state.
         * @param {VRS_SERIALISED_FILTER[]}  serialisedFilters
         * @param {Number}                          [maximumFilters]      The maximum number of filters - pass null or -1 to indicate there is no maximum.
         * @returns {VRS_SERIALISED_FILTER[]}
         */
        this.buildValidFiltersList = function(serialisedFilters, maximumFilters)
        {
            maximumFilters = maximumFilters === undefined ? -1 : maximumFilters;
            var validFilters = [];
            $.each(serialisedFilters, function(idx, serialisedFilter) {
                if(maximumFilters === -1 || validFilters.length <= maximumFilters) {
                    if(VRS.enumHelper.getEnumName(subclassSettings.propertyEnumObject, serialisedFilter.property) && serialisedFilter.valueCondition) validFilters.push(serialisedFilter);
                }
            });

            return validFilters;
        };
        //endregion

        //region -- addConfigureFiltersListToPane
        /**
         * Adds a paneList option field (an option field that shows a list of option panes) to a pane where the content
         * of the field is a set of panes, each pane representing a single filter and its parameters. The field allows
         * the user to add and remove filters.
         * @param {object}                          settings
         * @param {VRS.OptionPane}                  settings.pane                       The pane that the field holding the filters will be added to.
         * @param {VRS.Filter[]}                    settings.filters                    The array of filters to pre-fill the field with. These are the ones already set up by the user.
         * @param {string}                         [settings.fieldName]                 The name to assign to the field, defaults to 'filters'.
         * @param {function()}                      settings.saveState                  The callback that will save the state when the filters list is changed.
         * @param {number}                         [settings.maxFilters]                The maximum number of filters that the user can have.
         * @param {Array}                          [settings.allowableProperties]       An array of properties that the user can choose from. If not supplied then the user cannot add new filters.
         * @param {*}                              [settings.defaultProperty]           The property to show when adding a new filter. If not supplied then the first allowableProperties value is used.
         * @param {function(VRS.Filter, VRS.OptionFieldPaneList)} [settings.addFilter]  Mandatory if settings.allowableProperties is provided - called when the user creates a new filter, expected to add the filter to a list of user-created filters.
         * @param {string}                         [settings.addFilterButtonLabel]      The labelKey into VRS.$$ for the 'add a new filter' button.
         * @param {bool=}                           settings.onlyUniqueFilters          True if the user must be prohibited from adding filters for the same property twice. Defaults to false.
         * @param {function(*):bool=}               settings.isAlreadyInUse             A method that takes a property and returns true if it is already in use. Must be supplied if onlyUniqueFilters is true.
         * @returns {VRS.OptionFieldPaneList}
         */
        this.addConfigureFiltersListToPane = function(settings)
        {
            var pane = settings.pane;
            var filters = settings.filters;
            var fieldName = settings.fieldName || 'filters';
            var saveState = settings.saveState;
            var maxFilters = settings.maxFilters;
            var allowableProperties = settings.allowableProperties;
            var defaultProperty = settings.defaultProperty;
            var addFilter = settings.addFilter;
            var addFilterButtonLabel = settings.addFilterButtonLabel;
            var onlyUniqueFilters = settings.onlyUniqueFilters === undefined ? true : settings.onlyUniqueFilters;
            var isAlreadyInUse = settings.isAlreadyInUse || function() { return false; };

            var length = filters.length;
            var panes = [];
            for(var i = 0;i < length;++i) {
                var filter = filters[i];
                panes.push(filter.createOptionPane(saveState));
            }

            var paneListField = new VRS.OptionFieldPaneList({
                name:           fieldName,
                labelKey:       'Filters',
                saveState:      saveState,
                maxPanes:       maxFilters,
                panes:          panes
            });

            if(allowableProperties && allowableProperties.length) {
                var addPropertyComboBoxValues = [];
                length = allowableProperties.length;
                for(i = 0;i < length;++i) {
                    var allowableProperty = allowableProperties[i];
                    var handler = subclassSettings.filterPropertyHandlers[allowableProperty];
                    if(!handler) throw 'No handler defined for the "' + allowableProperty + '" property';
                    addPropertyComboBoxValues.push(new VRS.ValueText({ value: allowableProperty, textKey: handler.labelKey }));
                }

                var newComparer = defaultProperty || allowableProperties[0];
                var addFieldButton = new VRS.OptionFieldButton({
                        name:           'addComparer',
                        labelKey:       addFilterButtonLabel,
                        saveState:      function() { addFilter(newComparer, paneListField); },
                        icon:           'plusthick'
                    });

                var addPane = new VRS.OptionPane({
                    name:           'vrsAddNewEntryPane',
                    fields:         [
                        new VRS.OptionFieldComboBox({
                            name:           'newComparerCombo',
                            getValue:       function() { return newComparer; },
                            setValue:       function(value) { newComparer = value; },
                            saveState:      saveState,
                            keepWithNext:   true,
                            values:         addPropertyComboBoxValues,
                            changed:        function(newValue) {
                                if(onlyUniqueFilters) addFieldButton.setEnabled(!isAlreadyInUse(newValue));
                            }
                        }),
                        addFieldButton
                    ]
                });

                paneListField.setRefreshAddControls($.proxy(function(/** bool */ disabled, /** jQuery */ addParentJQ) {
                    if(!disabled && onlyUniqueFilters && isAlreadyInUse(newComparer)) disabled = true;
                    addFieldButton.setEnabled(!disabled);
                }, this));

                paneListField.setAddPane(addPane);
            }

            pane.addField(paneListField);

            return paneListField;
        };
        //endregion

        //region -- isFilterInUse, getIndexForFilterProperty, getFilterForFilterProperty
        /**
         * Returns true if any of the filters in the array passed across have the property passed across, otherwise returns false.
         * @param {Array.<VRS.Filter>} filters
         * @param {*} property
         * @returns {boolean}
         */
        this.isFilterInUse = function(filters, property)
        {
            return that.getIndexForFilterProperty(filters, property) !== -1;
        };

        /**
         * Returns the filter with the filter property passed across or null if no such filter exists.
         * @param {Array.<VRS.Filter>} filters
         * @param {*} property
         * @returns {VRS.Filter}
         */
        this.getFilterForFilterProperty = function(filters, property)
        {
            var index = that.getIndexForFilterProperty(filters, property);
            return index === -1 ? null : filters[index];
        };

        /**
         * Returns the index of the filter with the filter property passed across or -1 if no such filter exists.
         * @param {Array.<VRS.Filter>} filters
         * @param {*} property
         * @returns {number}
         */
        this.getIndexForFilterProperty = function(filters, property)
        {
            var result = -1;

            var length = filters.length;
            for(var i = 0;i < length;++i) {
                if(filters[i].getProperty() === property) {
                    result = i;
                    break;
                }
            }

            return result;
        };
        //endregion

        //region -- addToQueryParameters
        /**
         * Adds filter parameters to an object that collects together query string parameters for server fetch.
         * @param {VRS.Filter[]}                filters                 A list of filters to add to the query parameters.
         * @param {Object}                      query                   An object whose properties are going to be translated into a query string.
         * @param {VRS.UnitDisplayPreferences}  unitDisplayPreferences  The object that describes the units to use when filtering.
         */
        this.addToQueryParameters = function(filters, query, unitDisplayPreferences)
        {
            if(subclassSettings.addToQueryParameters) subclassSettings.addToQueryParameters(filters, query);
            if(filters && filters.length) {
                var length = filters.length;
                for(var i = 0;i < length;++i) {
                    var filter = filters[i];
                    filter.addToQueryParameters(query, unitDisplayPreferences);
                }
            }
        };
        //endregion
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
