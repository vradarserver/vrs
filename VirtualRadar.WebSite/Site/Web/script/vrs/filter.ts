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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.filterLabelWidth = VRS.globalOptions.filterLabelWidth !== undefined ? VRS.globalOptions.filterLabelWidth : VRS.LabelWidth.Short;   // The default width for labels in the filter options UI.

    /**
     * Options that can be applied when matching values in a filter.
     */
    export interface Filter_Options
    {
        caseInsensitive?: boolean;
    }

    export interface IValueCondition
    {
        getCondition() : FilterConditionEnum;
        setCondition(filter: FilterConditionEnum);

        getReverseCondition() : boolean;
        setReverseCondition(reverseCondition: boolean);

        equals(other: IValueCondition) : boolean;
        clone() : IValueCondition;
        toSerialisableObject() : ISerialisedCondition;
        applySerialisedObject(obj: ISerialisedCondition);
    }

    /**
     * The base for all value conditions.
     */
    export abstract class ValueCondition implements IValueCondition
    {
        protected _Condition: FilterConditionEnum;
        protected _ReverseCondition: boolean;

        constructor(condition: FilterConditionEnum, reverseCondition?: boolean)
        {
            this._Condition = condition;
            this._ReverseCondition = reverseCondition;
        }

        getCondition() : FilterConditionEnum
        {
            return this._Condition;
        }
        setCondition(value: FilterConditionEnum)
        {
            this._Condition = value;
        }

        getReverseCondition() : boolean
        {
            return this._ReverseCondition;
        }
        setReverseCondition(value: boolean)
        {
            this._ReverseCondition = value;
        }

        abstract equals(other: ValueCondition) : boolean;
        abstract clone() : ValueCondition;
        abstract toSerialisableObject() : ISerialisedCondition;
        abstract applySerialisedObject(obj: ISerialisedCondition);
    }

    /**
     * Describes a condition with single parameter that can be tested against a value.
     */
    export class OneValueCondition extends ValueCondition
    {
        private _Value: any;

        constructor(condition: FilterConditionEnum, reverseCondition?: boolean, value?: any)
        {
            super(condition, reverseCondition);
            this._Value = value;
        }

        getValue() : any
        {
            return this._Value;
        }
        setValue(value: any)
        {
            this._Value = value;
        }

        equals(obj: OneValueCondition) : boolean
        {
            return this.getCondition() === obj.getCondition() &&
                this.getReverseCondition() === obj.getReverseCondition() &&
                this.getValue() === obj.getValue();
        }

        clone() : OneValueCondition
        {
            return new OneValueCondition(this.getCondition(), this.getReverseCondition(), this.getValue());
        }

        toSerialisableObject() : ISerialisedOneValueCondition
        {
            return {
                condition: this._Condition,
                reversed: this._ReverseCondition,
                value: this._Value
            };
        }

        applySerialisedObject(settings: ISerialisedOneValueCondition)
        {
            this.setCondition(settings.condition);
            this.setReverseCondition(settings.reversed);
            this.setValue(settings.value);
        }
    }

    /**
     * Describes a condition that takes two parameters to be tested against a value.
     */
    export class TwoValueCondition extends ValueCondition
    {
        private _Value1: any;
        private _Value2: any;
        private _Value1IsLow: boolean;

        constructor(condition: FilterConditionEnum, reverseCondition?: boolean, value1?: any, value2?: any)
        {
            super(condition, reverseCondition);
            this._Value1 = value1;
            this._Value2 = value2;
        }

        getValue1() : any
        {
            return this._Value1;
        }
        setValue1(value: any)
        {
            if(value !== this._Value1) {
                this._Value1 = value;
                this.orderValues();
            }
        }

        getValue2() : any
        {
            return this._Value2;
        }
        setValue2(value: any)
        {
            if(value !== this._Value2) {
                this._Value2 = value;
                this.orderValues();
            }
        }

        getLowValue() : any
        {
            return this._Value1IsLow ? this._Value1 : this._Value2;
        }

        getHighValue() : any
        {
            return this._Value1IsLow ? this._Value2 : this._Value1;
        }

        private orderValues()
        {
            if(this._Value1 === undefined || this._Value2 === undefined) this._Value1IsLow = true;
            else this._Value1IsLow = this._Value1 <= this._Value2;
        }

        equals(obj: TwoValueCondition) : boolean
        {
            return this.getCondition() === obj.getCondition() &&
                this.getReverseCondition() === obj.getReverseCondition() &&
                this.getValue1() === obj.getValue1() &&
                this.getValue2() === obj.getValue2();
        }

        clone() : TwoValueCondition
        {
            return new VRS.TwoValueCondition(this.getCondition(), this.getReverseCondition(), this.getValue1(), this.getValue2());
        }

        toSerialisableObject() : ISerialisedTwoValueCondition
        {
            return {
                condition: this._Condition,
                reversed: this._ReverseCondition,
                value1: this._Value1,
                value2: this._Value2
            }
        }

        applySerialisedObject(settings: ISerialisedTwoValueCondition)
        {
            this.setCondition(settings.condition);
            this.setReverseCondition(settings.reversed);
            this.setValue1(settings.value1);
            this.setValue2(settings.value2);
        }
    }

    /**
     * Describes a condition without a value.
     */
    export interface Condition
    {
        condition:          FilterConditionEnum;
        reverseCondition:   boolean;
        labelKey:           string;
    }

    /**
     * The settings to use when creating a new FilterPropertyTypeHandler.
     */
    export interface FilterPropertyTypeHandler_Settings
    {
        /**
         * The VRS.FilterPropertyType that this object handles.
         */
        type: FilterPropertyTypeEnum;

        /**
         * Returns an array of condition enum values and their text keys.
         */
        getConditions?: () => Condition[];

        /**
         * Returns a new condition and values object.
         */
        createValueCondition: () => ValueCondition;

        /**
         * Mandatory if useSingleValueEquals is false, takes a value and a value/condition object and returns true if the value passes the filter.
         */
        valuePassesCallback?: (value: any, valueCondition: ValueCondition, options?: Filter_Options) => boolean;

        /**
         * True if the aircraft property is just compared as equal to the filter. Pre-fills 'valuePassesCallback'.
         */
        useSingleValueEquals?: boolean;

        /**
         * True if the aircraft property is to lie between a range of values. Pre-fills 'valuePassesCallback'.
         */
        useValueBetweenRange?: boolean;

        /**
         * Takes a label key, a value/condition object, a filter property handler and a saveState callback and returns an array of option fields.
         */
        createOptionFieldsCallback: (labelKey: string, valueCondition: ValueCondition, handler: FilterPropertyHandler, saveState: () => void) => OptionField[];

        /**
         * Parses a string into the correct type. Returns undefined or null if the string is unparseable.
         */
        parseString: (str: string) => any;

        /**
         * Formats a value into a query string value. Returns null or undefined if the value is empty / missing.
         */
        toQueryString: (value: any) => string;

        /**
         * Returns true if empty values are to be passed to the server. Defaults to function that returns false.
         */
        passEmptyValues?: (valueCondition: ValueCondition) => boolean;
    }

    /**
     * An object that matches a filter property type (boolean, numeric etc.) with the value filter required to test the
     * a property of this type and details of the UI required to collect those filter's parameters from the user.
     */
    export class FilterPropertyTypeHandler
    {
        private _Settings: FilterPropertyTypeHandler_Settings;

        constructor(settings: FilterPropertyTypeHandler_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(settings.useSingleValueEquals) settings.valuePassesCallback = this.singleValueEquals;
            if(settings.useValueBetweenRange) settings.valuePassesCallback = this.valueBetweenRange;

            if(!settings.type || !VRS.enumHelper.getEnumName(VRS.FilterPropertyType, settings.type)) throw 'You must supply a property type';
            if(!settings.createValueCondition) throw 'You must supply a createValueCondition';
            if(!settings.valuePassesCallback) throw 'You must supply an valuePassesCallback';
            if(!settings.createOptionFieldsCallback) throw 'You must supply a createOptionFieldsCallback';

            this._Settings = $.extend({
                passEmptyValues: function() { return false; }
            }, settings);
        }

        get type()
        {
            return this._Settings.type;
        }

        get getConditions()
        {
            return this._Settings.getConditions;
        }

        get createValueCondition()
        {
            return this._Settings.createValueCondition;
        }

        get createOptionFieldsCallback()
        {
            return this._Settings.createOptionFieldsCallback;
        }

        get valuePassesCallback()
        {
            return this._Settings.valuePassesCallback;
        }

        get parseString()
        {
            return this._Settings.parseString;
        }

        get passEmptyValues()
        {
            return this._Settings.passEmptyValues;
        }

        get toQueryString()
        {
            return this._Settings.toQueryString;
        }

        /**
         * Returns true if a value is equal to the value held by the parameters.
         */
        private singleValueEquals(value: any, valueCondition: OneValueCondition) : boolean
        {
            var filterValue = valueCondition.getValue();
            var result = filterValue === undefined;
            if(!result) {
                switch(valueCondition.getCondition()) {
                    case VRS.FilterCondition.Equals:  result = filterValue === value; break;
                    default:                          throw 'Invalid condition ' + valueCondition.getCondition() + ' for a ' + this._Settings.type + ' filter type';
                }
                if(valueCondition.getReverseCondition()) result = !result;
            }
            return result;
        }

        /**
         * Returns true if the value is between the values held by the parameters.
         */
        private valueBetweenRange(value: any, valueCondition: TwoValueCondition) : boolean
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

        /**
         * Returns an array of objects describing all of the filter conditions ready for use in a combo box drop-down.
         */
        getConditionComboBoxValues() : ValueText[]
        {
            var result = [];
            var self = this;
            $.each(this._Settings.getConditions(), function(idx, condition) {
                result.push(new VRS.ValueText({
                    value: self.encodeConditionAndReverseCondition(condition.condition, condition.reverseCondition),
                    textKey: condition.labelKey
                }));
            });
            return result;
        }

        /**
         * Returns the condition and, if reverseCondition is true, a suffix to distinguish it as the reverse condition.
         */
        encodeConditionAndReverseCondition(condition: FilterConditionEnum, reverseCondition: boolean) : string
        {
            return condition + (reverseCondition ? '_reversed' : '');
        }

        /**
         * Takes a string encoded by encodeConditionAndReverseCondition and returns the original condition and reverseCondition flag.
         * @param {string} encodedConditionAndReverse
         * @returns {VRS_CONDITION}
         */
        decodeConditionAndReverseCondition(encodedConditionAndReverse: string) : Condition
        {
            var markerPosn = encodedConditionAndReverse.indexOf('_reversed');
            var condition: FilterConditionEnum =
                markerPosn === -1 ? encodedConditionAndReverse : encodedConditionAndReverse.substr(0, markerPosn);
            var reversed = markerPosn !== -1;
            return <Condition>{
                condition: condition,
                reverseCondition: reversed
            };
        }

        /**
         * Takes a value/condition and returns the condition and reverse condition values encoded as a single string.
         */
        encodeCondition(valueCondition: ValueCondition) : string
        {
            return this.encodeConditionAndReverseCondition(valueCondition.getCondition(), valueCondition.getReverseCondition());
        }

        /**
         * Takes a string that holds an encoded condition and the reverse condition flag and applies them to a value/condition object.
         */
        applyEncodedCondition(valueCondition: ValueCondition, encodedCondition: string)
        {
            var decodedCondition = this.decodeConditionAndReverseCondition(encodedCondition);
            valueCondition.setCondition(decodedCondition.condition);
            valueCondition.setReverseCondition(decodedCondition.reverseCondition);
        }
    }

    /**
     * This is the list of pre-built (and potentially 3rd party) handlers for filter property types that describe how to
     * ask for the filter parameters used to test a property of a given type.
     * @type {Object.<VRS.FilterPropertyType, VRS.FilterPropertyTypeHandler>}
     */
    export var filterPropertyTypeHandlers: { [index: string]: FilterPropertyTypeHandler } = VRS.filterPropertyTypeHandlers || {};

    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.DateRange] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.DateRange,
        createValueCondition:       function() { return new VRS.TwoValueCondition(VRS.FilterCondition.Between); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Between, reverseCondition: false, labelKey: 'Between' },
            { condition: VRS.FilterCondition.Between, reverseCondition: true,  labelKey: 'NotBetween' }
        ]},
        useValueBetweenRange:       true,
        createOptionFieldsCallback: function(labelKey: string, twoValueCondition: TwoValueCondition, handler: FilterPropertyHandler, saveState: () => void) {
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

    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.EnumMatch] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.EnumMatch,
        createValueCondition:       function() { return new VRS.OneValueCondition(VRS.FilterCondition.Equals); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Equals, reverseCondition: false, labelKey: 'Equal' },
            { condition: VRS.FilterCondition.Equals, reverseCondition: true,  labelKey: 'NotEquals' }
        ]},
        useSingleValueEquals:       true,
        createOptionFieldsCallback: function(labelKey: string, oneValueCondition: OneValueCondition, handler: FilterPropertyHandler, saveState: () => void) {
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
        parseString: function(text) {
            return text !== undefined && text !== null ? text : undefined;
        },
        toQueryString: function(value) {
            return value;
        }
    });

    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.NumberRange] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.NumberRange,
        createValueCondition:       function() { return new VRS.TwoValueCondition(VRS.FilterCondition.Between); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Between, reverseCondition: false, labelKey: 'Between' },
            { condition: VRS.FilterCondition.Between, reverseCondition: true,  labelKey: 'NotBetween' }
        ]},
        useValueBetweenRange:       true,
        createOptionFieldsCallback: function(labelKey: string, twoValueCondition: TwoValueCondition, handler: FilterPropertyHandler, saveState: () => void) {
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
        toQueryString: function(value) {
            return value || value === 0 ? value.toString() : null;
        }
    });

    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.OnOff] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.OnOff,
        createValueCondition:       function() { return new VRS.OneValueCondition(VRS.FilterCondition.Equals, false, true); },        // Preset the option to true.
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Equals, reverseCondition: false, labelKey: 'Equal' }
        ]},
        useSingleValueEquals:       true,
        createOptionFieldsCallback: function(labelKey: string, oneValueCondition: OneValueCondition, handler: FilterPropertyHandler, saveState: () => void) { return [
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
            return value !== undefined && value !== null ? value ? '1' : '0' : null;
        }
    });

    var filterTextMatchSettings: FilterPropertyTypeHandler_Settings = {
        type:                       VRS.FilterPropertyType.TextMatch,
        createValueCondition:       function() { return new VRS.OneValueCondition(VRS.FilterCondition.Contains); },
        getConditions:              function() { return [
            { condition: VRS.FilterCondition.Contains, reverseCondition: false, labelKey: 'Contains' },
            { condition: VRS.FilterCondition.Contains, reverseCondition: true,  labelKey: 'NotContains' },
            { condition: VRS.FilterCondition.Equals,   reverseCondition: false, labelKey: 'Equal' },
            { condition: VRS.FilterCondition.Equals,   reverseCondition: true,  labelKey: 'NotEquals' },
            { condition: VRS.FilterCondition.Starts,   reverseCondition: false, labelKey: 'StartsWith' },
            { condition: VRS.FilterCondition.Starts,   reverseCondition: true,  labelKey: 'NotStartsWith' },
            { condition: VRS.FilterCondition.Ends,     reverseCondition: false, labelKey: 'EndsWith' },
            { condition: VRS.FilterCondition.Ends,     reverseCondition: true,  labelKey: 'NotEndsWith' }
        ]},
        valuePassesCallback:        function(value: any, oneValueCondition: OneValueCondition, options: Filter_Options) {
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
        createOptionFieldsCallback: function(labelKey: string, oneValueCondition: OneValueCondition, handler: FilterPropertyHandler, saveState: () => void) {
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
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.TextMatch] = new VRS.FilterPropertyTypeHandler(filterTextMatchSettings);

    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.TextListMatch] = new VRS.FilterPropertyTypeHandler({
        type:                       VRS.FilterPropertyType.TextListMatch,
        valuePassesCallback:        function(value: string[], oneValueCondition: OneValueCondition, options: Filter_Options) {
            var conditionValue: string = oneValueCondition.getValue();
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
        createValueCondition:       filterTextMatchSettings.createValueCondition,
        getConditions:              filterTextMatchSettings.getConditions,
        createOptionFieldsCallback: filterTextMatchSettings.createOptionFieldsCallback,
        parseString:                filterTextMatchSettings.parseString,
        toQueryString:              filterTextMatchSettings.toQueryString,
        passEmptyValues:            filterTextMatchSettings.passEmptyValues
    });

    /**
     * The settings to pass when creating a new instance of a FilterPropertyHandler.
     */
    export interface FilterPropertyHandler_Settings
    {
        /**
         * The property that this object handles.
         */
        property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum;

        /**
         * The property is expected to be an enum value - this is the enum object that holds all possible values of the property.
         */
        propertyEnumObject?: Object;

        /**
         * The VRS.FilterPropertyType of the filter used by this object.
         */
        type: FilterPropertyTypeEnum;

        /**
         * The key of the translated text for the filter's label.
         */
        labelKey: string;

        /**
         * A method that returns the value being filtered.
         */
        getValueCallback: (parameter: any) => any;

        /**
         * The callback (mandatory for enum types) that returns the list of all possible enum values and their descriptions.
         */
        getEnumValues?: () => ValueText[];

        /**
         * True if string values are to be converted to upper-case on data-entry.
         */
        isUpperCase?: boolean;

        /**
         * True if string values are to be converted to lower-case on data-entry.
         */
        isLowerCase?: boolean;

        /**
         * The minimum value for numeric range fields.
         */
        minimumValue?: number;

        /**
         * The maximum value for numeric range fields.
         */
        maximumValue?: number;

        /**
         * The number of decimals for numeric range fields.
         */
        decimalPlaces?: number;

        /**
         * The optional width to use on input fields.
         */
        inputWidth?: InputWidthEnum;

        /**
         * Returns true if the filter is supported server-side, false if it is not. Default returns true if serverFilterName is present, false if it is not.
         */
        isServerFilter?: (valueCondition: ValueCondition) => boolean;

        /**
         * The name to use when representing the filter in a query to the server.
         */
        serverFilterName?: string;

        /**
         * A method that takes a value and a unit display preferences and converts it from the user's preferred unit to the unit expected by the server.
         * The default just returns the value unchanged.
         */
        normaliseValue?: (value: any, unitDisplayPrefs: UnitDisplayPreferences) => any;

        /**
         * The initial condition to offer when the filter is added to the user interface.
         */
        defaultCondition?: FilterConditionEnum;
    }

    /**
     * The base for objects that bring together a property and its type, and describe the ranges that the filters can be
     * set to, its on-screen description and so on.
     */
    export class FilterPropertyHandler
    {
        protected _Settings: FilterPropertyHandler_Settings;

        constructor(settings: FilterPropertyHandler_Settings)
        {
            if(!settings) throw 'You must supply a settings object';
            if(!settings.property || !VRS.enumHelper.getEnumName(settings.propertyEnumObject, settings.property)) throw 'You must supply a valid property';
            if(!settings.type || !VRS.enumHelper.getEnumName(VRS.FilterPropertyType, settings.type)) throw 'You must supply a property type';
            if(!settings.labelKey) throw 'You must supply a labelKey';

            var self = this;
            this._Settings = $.extend({
                inputWidth: VRS.InputWidth.Auto,
                isServerFilter: function() { return !!self._Settings.serverFilterName; },
                normaliseValue: function(value: any) { return value; }
            }, settings);
        }

        get property()
        {
            return this._Settings.property;
        }

        get type()
        {
            return this._Settings.type;
        }

        get labelKey()
        {
            return this._Settings.labelKey;
        }

        get getValueCallback()
        {
            return this._Settings.getValueCallback;
        }

        get getEnumValues()
        {
            return this._Settings.getEnumValues;
        }

        get isUpperCase()
        {
            return this._Settings.isUpperCase;
        }

        get isLowerCase()
        {
            return this._Settings.isLowerCase;
        }

        get minimumValue()
        {
            return this._Settings.minimumValue;
        }

        get maximumValue()
        {
            return this._Settings.maximumValue;
        }

        get decimalPlaces()
        {
            return this._Settings.decimalPlaces;
        }

        get inputWidth()
        {
            return this._Settings.inputWidth;
        }

        get serverFilterName()
        {
            return this._Settings.serverFilterName;
        }

        get isServerFilter()
        {
            return this._Settings.isServerFilter;
        }

        get normaliseValue()
        {
            return this._Settings.normaliseValue;
        }

        get defaultCondition()
        {
            return this._Settings.defaultCondition;
        }

        /**
         * Returns the handler for the property's type.
         * @returns {VRS.FilterPropertyTypeHandler}
         */
        getFilterPropertyTypeHandler() : FilterPropertyTypeHandler
        {
            return filterPropertyTypeHandlers[this._Settings.type];
        }
    }

    /**
     * Describes the settings to pass when creating a new instance of Filter.
     */
    export interface Filter_Settings
    {
        /**
         * The property that is to be associated with the filter.
         */
        property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum;

        /**
         * The object that describes the condition and values to compare against a property.
         */
        valueCondition: ValueCondition;

        /**
         * The property is expected to belong to an enum - this is the enum object that holds all possible values of property.
         */
        propertyEnumObject: Object;

        /**
         * An associative array of property values and their associated FilterPropertyHandler-based objects. The index is either
         * an AircraftFilterPropertyEnum or a ReportFilterPropertyEnum.
         */
        filterPropertyHandlers: { [index: string]: FilterPropertyHandler };

        /**
         * A method that can create a clone of the filter derivee.
         */
        cloneCallback: (obj: AircraftFilterPropertyEnum | ReportFilterPropertyEnum, valueCondition: ValueCondition) => Filter;
    }

    /**
     * The base for more specialised filters. A filter joins together a property identifier and an object that carries a
     * condition and values to use with the condition in the filter test. The classes derived from this one specialise
     * the property but they all share the concept of joining a property and a value/condition together.
     */
    export class Filter
    {
        protected _Settings: Filter_Settings;

        constructor(settings: Filter_Settings)
        {
            if(!settings) throw 'The derivee must supply a subclassSettings object';
            if(!settings.property || !VRS.enumHelper.getEnumName(settings.propertyEnumObject, settings.property)) throw 'You must supply a valid property';
            if(!settings.valueCondition) throw 'You must supply a filter';

            this._Settings = settings;
        }

        getProperty() : AircraftFilterPropertyEnum | ReportFilterPropertyEnum
        {
            return this._Settings.property;
        }
        setProperty(value: AircraftFilterPropertyEnum | ReportFilterPropertyEnum)
        {
            if(!value || !VRS.enumHelper.getEnumName(this._Settings.propertyEnumObject, value)) {
                throw 'Cannot set property of "' + value + '", it is not a valid property';
            }
            this._Settings.property = value;
        }

        getValueCondition() : ValueCondition
        {
            return this._Settings.valueCondition;
        }
        setValueCondition(value: ValueCondition)
        {
            if(!value) throw 'You cannot set the value/condition to null or undefined';
            this._Settings.valueCondition = value;
        }

        getPropertyHandler() : FilterPropertyHandler
        {
            return this._Settings.filterPropertyHandlers[this._Settings.property];
        }

        /**
         * Returns true if this object has the same property values as another object.
         */
        equals(obj: Filter) : boolean
        {
            return this.getProperty() === obj.getProperty() && this.getValueCondition().equals(obj.getValueCondition());
        }

        /**
         * Returns a copy of this object.
         */
        clone() : Filter
        {
            return this._Settings.cloneCallback(this._Settings.property, this._Settings.valueCondition.clone());
        }

        /**
         * Creates a serialisable copy of the filter.
         */
        toSerialisableObject() : ISerialisedFilter
        {
            return {
                property:       this._Settings.property,
                valueCondition: this._Settings.valueCondition.toSerialisableObject()
            };
        }

        /**
         * Applies serialised settings to this object.
         */
        applySerialisedObject(settings: ISerialisedFilter)
        {
            this.setProperty(settings.property);
            this._Settings.valueCondition.applySerialisedObject(settings.valueCondition);
        }

        /**
         * Creates an object pane for the condition and parameters to the condition.
         */
        createOptionPane(saveState: () => any) : OptionPane
        {
            var propertyHandler = this.getPropertyHandler();
            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            var labelKey = propertyHandler.labelKey;

            return new VRS.OptionPane({
                name:   'filter',
                fields: typeHandler.createOptionFieldsCallback(labelKey, this.getValueCondition(), propertyHandler, saveState)
            });
        }

        /**
         * Adds parameters to a query string parameters object to represent the filter property, condition and values.
         */
        addToQueryParameters(query: Object, unitDisplayPreferences: UnitDisplayPreferences)
        {
            var valueCondition = this.getValueCondition();
            var propertyHandler = this._Settings.filterPropertyHandlers[this.getProperty()];
            if(propertyHandler && propertyHandler.isServerFilter(valueCondition)) {
                var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
                var filterName = propertyHandler.serverFilterName;
                var reverse = valueCondition.getReverseCondition() ? 'N' : '';
                var passEmptyValue = typeHandler.passEmptyValues(valueCondition);

                var addQueryString = function(suffix, value) {
                    if(passEmptyValue || value !== undefined) {
                        value = propertyHandler.normaliseValue(value, unitDisplayPreferences);
                        if(propertyHandler.decimalPlaces === 0) value = Math.round(value);
                        var textValue = typeHandler.toQueryString(value);
                        if(passEmptyValue || textValue) query[filterName + suffix + reverse] = textValue || '';
                    }
                };

                switch(valueCondition.getCondition()) {
                    case VRS.FilterCondition.Between:
                        addQueryString('L', (<TwoValueCondition>valueCondition).getLowValue());
                        addQueryString('U', (<TwoValueCondition>valueCondition).getHighValue());
                        break;
                    case VRS.FilterCondition.Contains:
                        addQueryString('C', (<OneValueCondition>valueCondition).getValue());
                        break;
                    case VRS.FilterCondition.Ends:
                        addQueryString('E', (<OneValueCondition>valueCondition).getValue());
                        break;
                    case VRS.FilterCondition.Equals:
                        addQueryString('Q', (<OneValueCondition>valueCondition).getValue());
                        break;
                    case VRS.FilterCondition.Starts:
                        addQueryString('S', (<OneValueCondition>valueCondition).getValue());
                        break;
                    default:
                        throw 'Unknown condition ' + valueCondition.getCondition();
                }
            }
        }
    }

    /**
     * The settings that are passed as parameters to FilterHelper.addConfigureFiltersListToPane
     */
    export interface FilterHelper_AddConfigureSettings
    {
        /**
         * The pane that the field holding the filters will be added to.
         */
        pane: OptionPane;

        /**
         * The array of filters to pre-fill the field with. These are the ones already set up by the user.
         */
        filters: Filter[];

        /**
         * The name to assign to the field, defaults to 'filters'.
         */
        fieldName?: string;

        /**
         * The callback that will save the state when the filters list is changed.
         */
        saveState: () => any;

        /**
         * The maximum number of filters that the user can have.
         */
        maxFilters?: number;

        /**
         * An array of properties that the user can choose from. If not supplied then the user cannot add new filters.
         */
        allowableProperties?: AircraftFilterPropertyEnum[] | ReportFilterPropertyEnum[];

        /**
         * The property to show when adding a new filter. If not supplied then the first allowableProperties value is used.
         */
        defaultProperty?: AircraftFilterPropertyEnum | ReportFilterPropertyEnum;

        /**
         * Mandatory if settings.allowableProperties is provided - called when the user creates a new filter, expected to
         * add the filter to a list of user-created filters.
         */
        addFilter?: (property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum, paneListField: OptionFieldPaneList) => void;

        /**
         * The labelKey into VRS.$$ for the 'add a new filter' button.
         */
        addFilterButtonLabel?: string;

        /**
         * True if the user must be prohibited from adding filters for the same property twice. Defaults to false.
         */
        onlyUniqueFilters?: boolean;

        /**
         * A method that takes a property and returns true if it is already in use. Must be supplied if onlyUniqueFilters is true.
         */
        isAlreadyInUse?: (property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum) => boolean;
    }

    /**
     * The settings to use when creating a new instance of FilterHelper.
     */
    export interface FilterHelper_Settings
    {
        /**
         * The enum object from which all filter properties are obtained.
         */
        propertyEnumObject: Object;

        /**
         * An associative array of property enum values against the handler for the property. The index is either an
         * AircraftFilterPropertyEnum or a ReportFilterPropertyEnum.
         */
        filterPropertyHandlers: { [index: string]: FilterPropertyHandler };

        /**
         * A method that creates a filter of the correct type.
         */
        createFilterCallback: (filterPropertyHandler: FilterPropertyHandler, valueCondition: ValueCondition) => Filter;

        /**
         * A method that adds query string parameters for a fetch from the server to a query string object.
         */
        addToQueryParameters?: (filters: Filter[], query: Object) => void;
    }

    /**
     * The base for helper objects that can deal with common routine tasks when working with filters.
     */
    export class FilterHelper
    {
        private _Settings: FilterHelper_Settings;

        constructor(settings: FilterHelper_Settings)
        {
            if(!settings) throw 'You must supply the subclass settings';
            this._Settings = settings;
        }

        /**
         * Creates a new Filter for the property passed across.
         */
        createFilter(property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum) : Filter
        {
            var propertyHandler = this._Settings.filterPropertyHandlers[property];
            if(!propertyHandler) throw 'There is no property handler for ' + property + ' properties';

            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            if(!typeHandler) throw 'There is no type handler of ' + propertyHandler.type + ' for ' + property + ' properties';

            var valueCondition = typeHandler.createValueCondition();
            if(propertyHandler.defaultCondition) valueCondition.setCondition(propertyHandler.defaultCondition);

            return this._Settings.createFilterCallback(propertyHandler, valueCondition);
        }

        /**
         * Converts a list of filters to an array of serialised objects that can be saved in an object's state.
         */
        serialiseFiltersList(filters: Filter[]) : ISerialisedFilter[]
        {
            var result = [];
            $.each(filters, function(idx, filter) { result.push(filter.toSerialisableObject()); });

            return result;
        }

        /**
         * Removes invalid serialised filters from a list of serialised filters, presumably from a previous session's state.
         */
        buildValidFiltersList(serialisedFilters: ISerialisedFilter[], maximumFilters: number = -1) : ISerialisedFilter[]
        {
            maximumFilters = maximumFilters === undefined ? -1 : maximumFilters;
            var validFilters = [];
            var self = this;
            $.each(serialisedFilters, function(idx, serialisedFilter) {
                if(maximumFilters === -1 || validFilters.length <= maximumFilters) {
                    if(VRS.enumHelper.getEnumName(self._Settings.propertyEnumObject, serialisedFilter.property) && serialisedFilter.valueCondition) {
                        validFilters.push(serialisedFilter);
                    }
                }
            });

            return validFilters;
        }

        /**
         * Adds a paneList option field (an option field that shows a list of option panes) to a pane where the content
         * of the field is a set of panes, each pane representing a single filter and its parameters. The field allows
         * the user to add and remove filters.
         */
        addConfigureFiltersListToPane(settings: FilterHelper_AddConfigureSettings) : OptionFieldPaneList
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
                    var handler = this._Settings.filterPropertyHandlers[allowableProperty];
                    if(!handler) throw 'No handler defined for the "' + allowableProperty + '" property';
                    addPropertyComboBoxValues.push(new VRS.ValueText({ value: allowableProperty, textKey: handler.labelKey }));
                }

                var newProperty = defaultProperty || allowableProperties[0];
                var addFieldButton = new VRS.OptionFieldButton({
                        name:           'addComparer',
                        labelKey:       addFilterButtonLabel,
                        saveState:      function() { addFilter(newProperty, paneListField); },
                        icon:           'plusthick'
                    });

                var addPane = new VRS.OptionPane({
                    name:           'vrsAddNewEntryPane',
                    fields:         [
                        new VRS.OptionFieldComboBox({
                            name:           'newComparerCombo',
                            getValue:       function() { return newProperty; },
                            setValue:       function(value) { newProperty = value; },
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

                paneListField.setRefreshAddControls((disabled: boolean, addParentJQ: JQuery) => {
                    if(!disabled && onlyUniqueFilters && isAlreadyInUse(newProperty)) disabled = true;
                    addFieldButton.setEnabled(!disabled);
                });

                paneListField.setAddPane(addPane);
            }

            pane.addField(paneListField);

            return paneListField;
        }

        /**
         * Returns true if any of the filters in the array passed across have the property passed across, otherwise returns false.
         */
        isFilterInUse(filters: Filter[], property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum) : boolean
        {
            return this.getIndexForFilterProperty(filters, property) !== -1;
        }

        /**
         * Returns the filter with the filter property passed across or null if no such filter exists.
         */
        getFilterForFilterProperty(filters: Filter[], property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum) : Filter
        {
            var index = this.getIndexForFilterProperty(filters, property);
            return index === -1 ? null : filters[index];
        }

        /**
         * Returns the index of the filter with the filter property passed across or -1 if no such filter exists.
         */
        getIndexForFilterProperty(filters: Filter[], property: AircraftFilterPropertyEnum | ReportFilterPropertyEnum) : number
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
        }

        /**
         * Adds filter parameters to an object that collects together query string parameters for server fetch.
         */
        addToQueryParameters(filters: Filter[], query: Object, unitDisplayPreferences: UnitDisplayPreferences)
        {
            if(this._Settings.addToQueryParameters) {
                this._Settings.addToQueryParameters(filters, query);
            }
            if(filters && filters.length) {
                var length = filters.length;
                for(var i = 0;i < length;++i) {
                    var filter = filters[i];
                    filter.addToQueryParameters(query, unitDisplayPreferences);
                }
            }
        }
    }
}
 