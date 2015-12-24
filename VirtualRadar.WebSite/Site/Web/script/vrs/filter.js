var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.filterLabelWidth = VRS.globalOptions.filterLabelWidth !== undefined ? VRS.globalOptions.filterLabelWidth : VRS.LabelWidth.Short;
    var ValueCondition = (function () {
        function ValueCondition(condition, reverseCondition) {
            this._Condition = condition;
            this._ReverseCondition = reverseCondition;
        }
        ValueCondition.prototype.getCondition = function () {
            return this._Condition;
        };
        ValueCondition.prototype.setCondition = function (value) {
            this._Condition = value;
        };
        ValueCondition.prototype.getReverseCondition = function () {
            return this._ReverseCondition;
        };
        ValueCondition.prototype.setReverseCondition = function (value) {
            this._ReverseCondition = value;
        };
        return ValueCondition;
    })();
    VRS.ValueCondition = ValueCondition;
    var OneValueCondition = (function (_super) {
        __extends(OneValueCondition, _super);
        function OneValueCondition(condition, reverseCondition, value) {
            _super.call(this, condition, reverseCondition);
            this._Value = value;
        }
        OneValueCondition.prototype.getValue = function () {
            return this._Value;
        };
        OneValueCondition.prototype.setValue = function (value) {
            this._Value = value;
        };
        OneValueCondition.prototype.equals = function (obj) {
            return this.getCondition() === obj.getCondition() &&
                this.getReverseCondition() === obj.getReverseCondition() &&
                this.getValue() === obj.getValue();
        };
        OneValueCondition.prototype.clone = function () {
            return new OneValueCondition(this.getCondition(), this.getReverseCondition(), this.getValue());
        };
        OneValueCondition.prototype.toSerialisableObject = function () {
            return {
                condition: this._Condition,
                reversed: this._ReverseCondition,
                value: this._Value
            };
        };
        OneValueCondition.prototype.applySerialisedObject = function (settings) {
            this.setCondition(settings.condition);
            this.setReverseCondition(settings.reversed);
            this.setValue(settings.value);
        };
        return OneValueCondition;
    })(ValueCondition);
    VRS.OneValueCondition = OneValueCondition;
    var TwoValueCondition = (function (_super) {
        __extends(TwoValueCondition, _super);
        function TwoValueCondition(condition, reverseCondition, value1, value2) {
            _super.call(this, condition, reverseCondition);
            this._Value1 = value1;
            this._Value2 = value2;
        }
        TwoValueCondition.prototype.getValue1 = function () {
            return this._Value1;
        };
        TwoValueCondition.prototype.setValue1 = function (value) {
            if (value !== this._Value1) {
                this._Value1 = value;
                this.orderValues();
            }
        };
        TwoValueCondition.prototype.getValue2 = function () {
            return this._Value2;
        };
        TwoValueCondition.prototype.setValue2 = function (value) {
            if (value !== this._Value2) {
                this._Value2 = value;
                this.orderValues();
            }
        };
        TwoValueCondition.prototype.getLowValue = function () {
            return this._Value1IsLow ? this._Value1 : this._Value2;
        };
        TwoValueCondition.prototype.getHighValue = function () {
            return this._Value1IsLow ? this._Value2 : this._Value1;
        };
        TwoValueCondition.prototype.orderValues = function () {
            if (this._Value1 === undefined || this._Value2 === undefined)
                this._Value1IsLow = true;
            else
                this._Value1IsLow = this._Value1 <= this._Value2;
        };
        TwoValueCondition.prototype.equals = function (obj) {
            return this.getCondition() === obj.getCondition() &&
                this.getReverseCondition() === obj.getReverseCondition() &&
                this.getValue1() === obj.getValue1() &&
                this.getValue2() === obj.getValue2();
        };
        TwoValueCondition.prototype.clone = function () {
            return new VRS.TwoValueCondition(this.getCondition(), this.getReverseCondition(), this.getValue1(), this.getValue2());
        };
        TwoValueCondition.prototype.toSerialisableObject = function () {
            return {
                condition: this._Condition,
                reversed: this._ReverseCondition,
                value1: this._Value1,
                value2: this._Value2
            };
        };
        TwoValueCondition.prototype.applySerialisedObject = function (settings) {
            this.setCondition(settings.condition);
            this.setReverseCondition(settings.reversed);
            this.setValue1(settings.value1);
            this.setValue2(settings.value2);
        };
        return TwoValueCondition;
    })(ValueCondition);
    VRS.TwoValueCondition = TwoValueCondition;
    var FilterPropertyTypeHandler = (function () {
        function FilterPropertyTypeHandler(settings) {
            if (!settings)
                throw 'You must supply a settings object';
            if (settings.useSingleValueEquals)
                settings.valuePassesCallback = this.singleValueEquals;
            if (settings.useValueBetweenRange)
                settings.valuePassesCallback = this.valueBetweenRange;
            if (!settings.type || !VRS.enumHelper.getEnumName(VRS.FilterPropertyType, settings.type))
                throw 'You must supply a property type';
            if (!settings.createValueCondition)
                throw 'You must supply a createValueCondition';
            if (!settings.valuePassesCallback)
                throw 'You must supply an valuePassesCallback';
            if (!settings.createOptionFieldsCallback)
                throw 'You must supply a createOptionFieldsCallback';
            this.type = settings.type;
            this.getConditions = settings.getConditions;
            this.createValueCondition = settings.createValueCondition;
            this.createOptionFieldsCallback = settings.createOptionFieldsCallback;
            this.valuePassesCallback = settings.valuePassesCallback;
            this.parseString = settings.parseString;
            this.passEmptyValues = settings.passEmptyValues || function () { return false; };
            this.toQueryString = settings.toQueryString;
        }
        FilterPropertyTypeHandler.prototype.singleValueEquals = function (value, valueCondition) {
            var filterValue = valueCondition.getValue();
            var result = filterValue === undefined;
            if (!result) {
                switch (valueCondition.getCondition()) {
                    case VRS.FilterCondition.Equals:
                        result = filterValue === value;
                        break;
                    default: throw 'Invalid condition ' + valueCondition.getCondition() + ' for a ' + this.type + ' filter type';
                }
                if (valueCondition.getReverseCondition())
                    result = !result;
            }
            return result;
        };
        FilterPropertyTypeHandler.prototype.valueBetweenRange = function (value, valueCondition) {
            var filterLowValue = valueCondition.getLowValue();
            var filterHighValue = valueCondition.getHighValue();
            var result = filterLowValue === undefined && filterHighValue === undefined;
            if (!result) {
                switch (valueCondition.getCondition()) {
                    case VRS.FilterCondition.Between:
                        if (filterLowValue === undefined)
                            result = value <= filterHighValue;
                        else if (filterHighValue === undefined)
                            result = value >= filterLowValue;
                        else
                            result = value >= filterLowValue && value <= filterHighValue;
                        break;
                    default:
                        throw 'Invalid condition ' + valueCondition.getCondition() + ' for range property types';
                }
                if (valueCondition.getReverseCondition())
                    result = !result;
            }
            return result;
        };
        FilterPropertyTypeHandler.prototype.getConditionComboBoxValues = function () {
            var result = [];
            var self = this;
            $.each(this.getConditions(), function (idx, condition) {
                result.push(new VRS.ValueText({
                    value: self.encodeConditionAndReverseCondition(condition.condition, condition.reverseCondition),
                    textKey: condition.labelKey
                }));
            });
            return result;
        };
        FilterPropertyTypeHandler.prototype.encodeConditionAndReverseCondition = function (condition, reverseCondition) {
            return condition + (reverseCondition ? '_reversed' : '');
        };
        FilterPropertyTypeHandler.prototype.decodeConditionAndReverseCondition = function (encodedConditionAndReverse) {
            var markerPosn = encodedConditionAndReverse.indexOf('_reversed');
            var condition = markerPosn === -1 ? encodedConditionAndReverse : encodedConditionAndReverse.substr(0, markerPosn);
            var reversed = markerPosn !== -1;
            return {
                condition: condition,
                reverseCondition: reversed
            };
        };
        FilterPropertyTypeHandler.prototype.encodeCondition = function (valueCondition) {
            return this.encodeConditionAndReverseCondition(valueCondition.getCondition(), valueCondition.getReverseCondition());
        };
        FilterPropertyTypeHandler.prototype.applyEncodedCondition = function (valueCondition, encodedCondition) {
            var decodedCondition = this.decodeConditionAndReverseCondition(encodedCondition);
            valueCondition.setCondition(decodedCondition.condition);
            valueCondition.setReverseCondition(decodedCondition.reverseCondition);
        };
        return FilterPropertyTypeHandler;
    })();
    VRS.FilterPropertyTypeHandler = FilterPropertyTypeHandler;
    VRS.filterPropertyTypeHandlers = VRS.filterPropertyTypeHandlers || {};
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.DateRange] = new VRS.FilterPropertyTypeHandler({
        type: VRS.FilterPropertyType.DateRange,
        createValueCondition: function () { return new VRS.TwoValueCondition(VRS.FilterCondition.Between); },
        getConditions: function () {
            return [
                { condition: VRS.FilterCondition.Between, reverseCondition: false, labelKey: 'Between' },
                { condition: VRS.FilterCondition.Between, reverseCondition: true, labelKey: 'NotBetween' }
            ];
        },
        useValueBetweenRange: true,
        createOptionFieldsCallback: function (labelKey, twoValueCondition, handler, saveState) {
            var self = this;
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name: 'label',
                    labelKey: labelKey,
                    keepWithNext: true,
                    labelWidth: VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name: 'condition',
                    getValue: function () { return self.encodeCondition(twoValueCondition); },
                    setValue: function (value) { self.applyEncodedCondition(twoValueCondition, value); },
                    saveState: saveState,
                    values: conditionValues,
                    keepWithNext: true
                }),
                new VRS.OptionFieldDate({
                    name: 'value1',
                    getValue: function () { return twoValueCondition.getValue1(); },
                    setValue: function (value) { twoValueCondition.setValue1(value); },
                    inputWidth: handler.inputWidth,
                    saveState: saveState,
                    keepWithNext: true
                }),
                new VRS.OptionFieldLabel({
                    name: 'valueSeparator',
                    labelKey: 'SeparateTwoValues',
                    keepWithNext: true
                }),
                new VRS.OptionFieldDate({
                    name: 'value2',
                    getValue: function () { return twoValueCondition.getValue2(); },
                    setValue: function (value) { twoValueCondition.setValue2(value); },
                    inputWidth: handler.inputWidth,
                    saveState: saveState
                })
            ];
        },
        parseString: function (text) {
            return VRS.dateHelper.parse(text);
        },
        toQueryString: function (value) {
            return value ? VRS.dateHelper.toIsoFormatString(value, true, true) : null;
        }
    });
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.EnumMatch] = new VRS.FilterPropertyTypeHandler({
        type: VRS.FilterPropertyType.EnumMatch,
        createValueCondition: function () { return new VRS.OneValueCondition(VRS.FilterCondition.Equals); },
        getConditions: function () {
            return [
                { condition: VRS.FilterCondition.Equals, reverseCondition: false, labelKey: 'Equal' },
                { condition: VRS.FilterCondition.Equals, reverseCondition: true, labelKey: 'NotEquals' }
            ];
        },
        useSingleValueEquals: true,
        createOptionFieldsCallback: function (labelKey, oneValueCondition, handler, saveState) {
            var self = this;
            var comboBoxValues = handler.getEnumValues();
            if (!comboBoxValues)
                throw 'Property type handlers for enum types must supply a getEnumValues callback';
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name: 'label',
                    labelKey: labelKey,
                    keepWithNext: true,
                    labelWidth: VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name: 'condition',
                    getValue: function () { return self.encodeCondition(oneValueCondition); },
                    setValue: function (value) { self.applyEncodedCondition(oneValueCondition, value); },
                    saveState: saveState,
                    values: conditionValues,
                    keepWithNext: true
                }),
                new VRS.OptionFieldComboBox({
                    name: 'value',
                    getValue: function () { return oneValueCondition.getValue(); },
                    setValue: function (value) { oneValueCondition.setValue(value); },
                    saveState: saveState,
                    values: comboBoxValues
                })
            ];
        },
        parseString: function (text) {
            return text !== undefined && text !== null ? text : undefined;
        },
        toQueryString: function (value) {
            return value;
        }
    });
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.NumberRange] = new VRS.FilterPropertyTypeHandler({
        type: VRS.FilterPropertyType.NumberRange,
        createValueCondition: function () { return new VRS.TwoValueCondition(VRS.FilterCondition.Between); },
        getConditions: function () {
            return [
                { condition: VRS.FilterCondition.Between, reverseCondition: false, labelKey: 'Between' },
                { condition: VRS.FilterCondition.Between, reverseCondition: true, labelKey: 'NotBetween' }
            ];
        },
        useValueBetweenRange: true,
        createOptionFieldsCallback: function (labelKey, twoValueCondition, handler, saveState) {
            var self = this;
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name: 'label',
                    labelKey: labelKey,
                    keepWithNext: true,
                    labelWidth: VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name: 'condition',
                    getValue: function () { return self.encodeCondition(twoValueCondition); },
                    setValue: function (value) { self.applyEncodedCondition(twoValueCondition, value); },
                    saveState: saveState,
                    values: conditionValues,
                    keepWithNext: true
                }),
                new VRS.OptionFieldNumeric({
                    name: 'value1',
                    getValue: function () { return twoValueCondition.getValue1(); },
                    setValue: function (value) { twoValueCondition.setValue1(value); },
                    inputWidth: handler.inputWidth,
                    saveState: saveState,
                    min: handler.minimumValue,
                    max: handler.maximumValue,
                    decimals: handler.decimalPlaces,
                    keepWithNext: true
                }),
                new VRS.OptionFieldLabel({
                    name: 'valueSeparator',
                    labelKey: 'SeparateTwoValues',
                    keepWithNext: true
                }),
                new VRS.OptionFieldNumeric({
                    name: 'value2',
                    getValue: function () { return twoValueCondition.getValue2(); },
                    setValue: function (value) { twoValueCondition.setValue2(value); },
                    inputWidth: handler.inputWidth,
                    saveState: saveState,
                    min: handler.minimumValue,
                    max: handler.maximumValue,
                    decimals: handler.decimalPlaces
                })
            ];
        },
        parseString: function (text) {
            var result;
            try {
                if (text !== null && text !== undefined) {
                    result = parseFloat(text);
                    if (isNaN(result))
                        result = undefined;
                }
            }
            catch (ex) {
                result = undefined;
            }
            return result;
        },
        toQueryString: function (value) {
            return value || value === 0 ? value.toString() : null;
        }
    });
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.OnOff] = new VRS.FilterPropertyTypeHandler({
        type: VRS.FilterPropertyType.OnOff,
        createValueCondition: function () { return new VRS.OneValueCondition(VRS.FilterCondition.Equals, false, true); },
        getConditions: function () {
            return [
                { condition: VRS.FilterCondition.Equals, reverseCondition: false, labelKey: 'Equal' }
            ];
        },
        useSingleValueEquals: true,
        createOptionFieldsCallback: function (labelKey, oneValueCondition, handler, saveState) {
            return [
                new VRS.OptionFieldCheckBox({
                    name: 'onOff',
                    labelKey: labelKey,
                    getValue: function () { return oneValueCondition.getValue(); },
                    setValue: function (value) { oneValueCondition.setValue(value); },
                    saveState: saveState
                })
            ];
        },
        parseString: function (text) {
            var result;
            if (text) {
                switch (text.toUpperCase()) {
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
        toQueryString: function (value) {
            return value !== undefined && value !== null ? value ? '1' : '0' : null;
        }
    });
    var filterTextMatchSettings = {
        type: VRS.FilterPropertyType.TextMatch,
        createValueCondition: function () { return new VRS.OneValueCondition(VRS.FilterCondition.Contains); },
        getConditions: function () {
            return [
                { condition: VRS.FilterCondition.Contains, reverseCondition: false, labelKey: 'Contains' },
                { condition: VRS.FilterCondition.Contains, reverseCondition: true, labelKey: 'NotContains' },
                { condition: VRS.FilterCondition.Equals, reverseCondition: false, labelKey: 'Equal' },
                { condition: VRS.FilterCondition.Equals, reverseCondition: true, labelKey: 'NotEquals' },
                { condition: VRS.FilterCondition.Starts, reverseCondition: false, labelKey: 'StartsWith' },
                { condition: VRS.FilterCondition.Starts, reverseCondition: true, labelKey: 'NotStartsWith' },
                { condition: VRS.FilterCondition.Ends, reverseCondition: false, labelKey: 'EndsWith' },
                { condition: VRS.FilterCondition.Ends, reverseCondition: true, labelKey: 'NotEndsWith' }
            ];
        },
        valuePassesCallback: function (value, oneValueCondition, options) {
            var conditionValue = oneValueCondition.getValue();
            var result = conditionValue === undefined;
            if (!result) {
                var ignoreCase = options && options.caseInsensitive;
                switch (oneValueCondition.getCondition()) {
                    case VRS.FilterCondition.Contains:
                        result = VRS.stringUtility.contains(value, conditionValue, ignoreCase);
                        break;
                    case VRS.FilterCondition.Equals:
                        result = VRS.stringUtility.equals(value, conditionValue, ignoreCase);
                        break;
                    case VRS.FilterCondition.Starts:
                        result = VRS.stringUtility.startsWith(value, conditionValue, ignoreCase);
                        break;
                    case VRS.FilterCondition.Ends:
                        result = VRS.stringUtility.endsWith(value, conditionValue, ignoreCase);
                        break;
                    default: throw 'Invalid condition ' + oneValueCondition.getCondition() + ' for text match filters';
                }
                if (oneValueCondition.getReverseCondition())
                    result = !result;
            }
            return result;
        },
        createOptionFieldsCallback: function (labelKey, oneValueCondition, handler, saveState) {
            var self = this;
            var conditionValues = self.getConditionComboBoxValues();
            return [
                new VRS.OptionFieldLabel({
                    name: 'label',
                    labelKey: labelKey,
                    keepWithNext: true,
                    labelWidth: VRS.globalOptions.filterLabelWidth
                }),
                new VRS.OptionFieldComboBox({
                    name: 'condition',
                    getValue: function () { return self.encodeCondition(oneValueCondition); },
                    setValue: function (value) { self.applyEncodedCondition(oneValueCondition, value); },
                    saveState: saveState,
                    values: conditionValues,
                    keepWithNext: true
                }),
                new VRS.OptionFieldTextBox({
                    name: 'value',
                    getValue: function () { return oneValueCondition.getValue(); },
                    setValue: function (value) { oneValueCondition.setValue(value); },
                    inputWidth: handler.inputWidth,
                    saveState: saveState,
                    upperCase: handler.isUpperCase,
                    lowerCase: handler.isLowerCase
                })
            ];
        },
        parseString: function (text) { return text !== null ? text : undefined; },
        toQueryString: function (value) { return value; },
        passEmptyValues: function (valueCondition) {
            return valueCondition.getCondition() === VRS.FilterCondition.Equals;
        }
    };
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.TextMatch] = new VRS.FilterPropertyTypeHandler(filterTextMatchSettings);
    VRS.filterPropertyTypeHandlers[VRS.FilterPropertyType.TextListMatch] = new VRS.FilterPropertyTypeHandler({
        type: VRS.FilterPropertyType.TextListMatch,
        valuePassesCallback: function (value, oneValueCondition, options) {
            var conditionValue = oneValueCondition.getValue();
            var result = conditionValue === undefined;
            if (!result) {
                var ignoreCase = options && options.caseInsensitive;
                var condition = oneValueCondition.getCondition();
                var length = value.length;
                for (var i = 0; !result && i < length; ++i) {
                    var item = value[i];
                    switch (condition) {
                        case VRS.FilterCondition.Contains:
                            result = VRS.stringUtility.contains(item, conditionValue, ignoreCase);
                            break;
                        case VRS.FilterCondition.Equals:
                            result = VRS.stringUtility.equals(item, conditionValue, ignoreCase);
                            break;
                        case VRS.FilterCondition.Starts:
                            result = VRS.stringUtility.startsWith(item, conditionValue, ignoreCase);
                            break;
                        case VRS.FilterCondition.Ends:
                            result = VRS.stringUtility.endsWith(item, conditionValue, ignoreCase);
                            break;
                        default: throw 'Invalid condition ' + condition + ' for text match filters';
                    }
                }
                if (oneValueCondition.getReverseCondition())
                    result = !result;
            }
            return result;
        },
        createValueCondition: filterTextMatchSettings.createValueCondition,
        getConditions: filterTextMatchSettings.getConditions,
        createOptionFieldsCallback: filterTextMatchSettings.createOptionFieldsCallback,
        parseString: filterTextMatchSettings.parseString,
        toQueryString: filterTextMatchSettings.toQueryString,
        passEmptyValues: filterTextMatchSettings.passEmptyValues
    });
    var FilterPropertyHandler = (function () {
        function FilterPropertyHandler(settings) {
            if (!settings)
                throw 'You must supply a settings object';
            if (!settings.property || !VRS.enumHelper.getEnumName(settings.propertyEnumObject, settings.property))
                throw 'You must supply a valid property';
            if (!settings.type || !VRS.enumHelper.getEnumName(VRS.FilterPropertyType, settings.type))
                throw 'You must supply a property type';
            if (!settings.labelKey)
                throw 'You must supply a labelKey';
            this.property = settings.property;
            this.type = settings.type;
            this.labelKey = settings.labelKey;
            this.getValueCallback = settings.getValueCallback;
            this.getEnumValues = settings.getEnumValues;
            this.isUpperCase = settings.isUpperCase;
            this.isLowerCase = settings.isLowerCase;
            this.minimumValue = settings.minimumValue;
            this.maximumValue = settings.maximumValue;
            this.decimalPlaces = settings.decimalPlaces;
            this.inputWidth = settings.inputWidth === undefined ? VRS.InputWidth.Auto : settings.inputWidth;
            this.serverFilterName = settings.serverFilterName;
            this.isServerFilter = settings.isServerFilter || function () { return !!settings.serverFilterName; };
            this.normaliseValue = settings.normaliseValue || function (value) { return value; };
            this.defaultCondition = settings.defaultCondition;
        }
        FilterPropertyHandler.prototype.getFilterPropertyTypeHandler = function () {
            return VRS.filterPropertyTypeHandlers[this.type];
        };
        return FilterPropertyHandler;
    })();
    VRS.FilterPropertyHandler = FilterPropertyHandler;
    var Filter = (function () {
        function Filter(settings) {
            if (!settings)
                throw 'The derivee must supply a subclassSettings object';
            if (!settings.property || !VRS.enumHelper.getEnumName(settings.propertyEnumObject, settings.property))
                throw 'You must supply a valid property';
            if (!settings.valueCondition)
                throw 'You must supply a filter';
            this._Settings = settings;
        }
        Filter.prototype.getProperty = function () {
            return this._Settings.property;
        };
        Filter.prototype.setProperty = function (value) {
            if (!value || !VRS.enumHelper.getEnumName(this._Settings.propertyEnumObject, value)) {
                throw 'Cannot set property of "' + value + '", it is not a valid property';
            }
            this._Settings.property = value;
        };
        Filter.prototype.getValueCondition = function () {
            return this._Settings.valueCondition;
        };
        Filter.prototype.setValueCondition = function (value) {
            if (!value)
                throw 'You cannot set the value/condition to null or undefined';
            this._Settings.valueCondition = value;
        };
        Filter.prototype.getPropertyHandler = function () {
            return this._Settings.filterPropertyHandlers[this._Settings.property];
        };
        Filter.prototype.equals = function (obj) {
            return this.getProperty() === obj.getProperty() && this.getValueCondition().equals(obj.getValueCondition());
        };
        Filter.prototype.clone = function () {
            return this._Settings.cloneCallback(this._Settings.property, this._Settings.valueCondition.clone());
        };
        Filter.prototype.toSerialisableObject = function () {
            return {
                property: this._Settings.property,
                valueCondition: this._Settings.valueCondition.toSerialisableObject()
            };
        };
        Filter.prototype.applySerialisedObject = function (settings) {
            this.setProperty(settings.property);
            this._Settings.valueCondition.applySerialisedObject(settings.valueCondition);
        };
        Filter.prototype.createOptionPane = function (saveState) {
            var propertyHandler = this.getPropertyHandler();
            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            var labelKey = propertyHandler.labelKey;
            return new VRS.OptionPane({
                name: 'filter',
                fields: typeHandler.createOptionFieldsCallback(labelKey, this.getValueCondition(), propertyHandler, saveState)
            });
        };
        Filter.prototype.addToQueryParameters = function (query, unitDisplayPreferences) {
            var valueCondition = this.getValueCondition();
            var propertyHandler = this._Settings.filterPropertyHandlers[this.getProperty()];
            if (propertyHandler && propertyHandler.isServerFilter(valueCondition)) {
                var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
                var filterName = propertyHandler.serverFilterName;
                var reverse = valueCondition.getReverseCondition() ? 'N' : '';
                var passEmptyValue = typeHandler.passEmptyValues(valueCondition);
                var addQueryString = function (suffix, value) {
                    if (passEmptyValue || value !== undefined) {
                        value = propertyHandler.normaliseValue(value, unitDisplayPreferences);
                        if (propertyHandler.decimalPlaces === 0)
                            value = Math.round(value);
                        var textValue = typeHandler.toQueryString(value);
                        if (passEmptyValue || textValue)
                            query[filterName + suffix + reverse] = textValue || '';
                    }
                };
                switch (valueCondition.getCondition()) {
                    case VRS.FilterCondition.Between:
                        addQueryString('L', valueCondition.getLowValue());
                        addQueryString('U', valueCondition.getHighValue());
                        break;
                    case VRS.FilterCondition.Contains:
                        addQueryString('C', valueCondition.getValue());
                        break;
                    case VRS.FilterCondition.Ends:
                        addQueryString('E', valueCondition.getValue());
                        break;
                    case VRS.FilterCondition.Equals:
                        addQueryString('Q', valueCondition.getValue());
                        break;
                    case VRS.FilterCondition.Starts:
                        addQueryString('S', valueCondition.getValue());
                        break;
                    default:
                        throw 'Unknown condition ' + valueCondition.getCondition();
                }
            }
        };
        return Filter;
    })();
    VRS.Filter = Filter;
    var FilterHelper = (function () {
        function FilterHelper(settings) {
            if (!settings)
                throw 'You must supply the subclass settings';
            this._Settings = settings;
        }
        FilterHelper.prototype.createFilter = function (property) {
            var propertyHandler = this._Settings.filterPropertyHandlers[property];
            if (!propertyHandler)
                throw 'There is no property handler for ' + property + ' properties';
            var typeHandler = propertyHandler.getFilterPropertyTypeHandler();
            if (!typeHandler)
                throw 'There is no type handler of ' + propertyHandler.type + ' for ' + property + ' properties';
            var valueCondition = typeHandler.createValueCondition();
            if (propertyHandler.defaultCondition)
                valueCondition.setCondition(propertyHandler.defaultCondition);
            return this._Settings.createFilterCallback(propertyHandler, valueCondition);
        };
        FilterHelper.prototype.serialiseFiltersList = function (filters) {
            var result = [];
            $.each(filters, function (idx, filter) { result.push(filter.toSerialisableObject()); });
            return result;
        };
        FilterHelper.prototype.buildValidFiltersList = function (serialisedFilters, maximumFilters) {
            if (maximumFilters === void 0) { maximumFilters = -1; }
            maximumFilters = maximumFilters === undefined ? -1 : maximumFilters;
            var validFilters = [];
            var self = this;
            $.each(serialisedFilters, function (idx, serialisedFilter) {
                if (maximumFilters === -1 || validFilters.length <= maximumFilters) {
                    if (VRS.enumHelper.getEnumName(self._Settings.propertyEnumObject, serialisedFilter.property) && serialisedFilter.valueCondition) {
                        validFilters.push(serialisedFilter);
                    }
                }
            });
            return validFilters;
        };
        FilterHelper.prototype.addConfigureFiltersListToPane = function (settings) {
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
            var isAlreadyInUse = settings.isAlreadyInUse || function () { return false; };
            var length = filters.length;
            var panes = [];
            for (var i = 0; i < length; ++i) {
                var filter = filters[i];
                panes.push(filter.createOptionPane(saveState));
            }
            var paneListField = new VRS.OptionFieldPaneList({
                name: fieldName,
                labelKey: 'Filters',
                saveState: saveState,
                maxPanes: maxFilters,
                panes: panes
            });
            if (allowableProperties && allowableProperties.length) {
                var addPropertyComboBoxValues = [];
                length = allowableProperties.length;
                for (i = 0; i < length; ++i) {
                    var allowableProperty = allowableProperties[i];
                    var handler = this._Settings.filterPropertyHandlers[allowableProperty];
                    if (!handler)
                        throw 'No handler defined for the "' + allowableProperty + '" property';
                    addPropertyComboBoxValues.push(new VRS.ValueText({ value: allowableProperty, textKey: handler.labelKey }));
                }
                var newProperty = defaultProperty || allowableProperties[0];
                var addFieldButton = new VRS.OptionFieldButton({
                    name: 'addComparer',
                    labelKey: addFilterButtonLabel,
                    saveState: function () { addFilter(newProperty, paneListField); },
                    icon: 'plusthick'
                });
                var addPane = new VRS.OptionPane({
                    name: 'vrsAddNewEntryPane',
                    fields: [
                        new VRS.OptionFieldComboBox({
                            name: 'newComparerCombo',
                            getValue: function () { return newProperty; },
                            setValue: function (value) { newProperty = value; },
                            saveState: saveState,
                            keepWithNext: true,
                            values: addPropertyComboBoxValues,
                            changed: function (newValue) {
                                if (onlyUniqueFilters)
                                    addFieldButton.setEnabled(!isAlreadyInUse(newValue));
                            }
                        }),
                        addFieldButton
                    ]
                });
                paneListField.setRefreshAddControls(function (disabled, addParentJQ) {
                    if (!disabled && onlyUniqueFilters && isAlreadyInUse(newProperty))
                        disabled = true;
                    addFieldButton.setEnabled(!disabled);
                });
                paneListField.setAddPane(addPane);
            }
            pane.addField(paneListField);
            return paneListField;
        };
        FilterHelper.prototype.isFilterInUse = function (filters, property) {
            return this.getIndexForFilterProperty(filters, property) !== -1;
        };
        FilterHelper.prototype.getFilterForFilterProperty = function (filters, property) {
            var index = this.getIndexForFilterProperty(filters, property);
            return index === -1 ? null : filters[index];
        };
        FilterHelper.prototype.getIndexForFilterProperty = function (filters, property) {
            var result = -1;
            var length = filters.length;
            for (var i = 0; i < length; ++i) {
                if (filters[i].getProperty() === property) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        FilterHelper.prototype.addToQueryParameters = function (filters, query, unitDisplayPreferences) {
            if (this._Settings.addToQueryParameters) {
                this._Settings.addToQueryParameters(filters, query);
            }
            if (filters && filters.length) {
                var length = filters.length;
                for (var i = 0; i < length; ++i) {
                    var filter = filters[i];
                    filter.addToQueryParameters(query, unitDisplayPreferences);
                }
            }
        };
        return FilterHelper;
    })();
    VRS.FilterHelper = FilterHelper;
})(VRS || (VRS = {}));
//# sourceMappingURL=filter.js.map