var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionField = (function () {
        function OptionField(settings) {
            this._Events = {
                refreshFieldContent: 'refreshFieldContent',
                refreshFieldState: 'refreshFieldState',
                refreshFieldVisibility: 'refreshFieldVisibility'
            };
            this._ChangedHookResult = null;
            if (!settings)
                throw 'You must supply settings';
            if (!settings.name)
                throw 'You must supply a name for the field';
            if (!settings.controlType)
                throw 'You must supply the field\'s control type';
            if (!VRS.optionControlTypeBroker.controlTypeHasHandler(settings.controlType))
                throw 'There is no control type handler for ' + settings.controlType;
            settings = $.extend({
                name: null,
                dispatcherName: 'VRS.OptionField',
                controlType: null,
                labelKey: '',
                getValue: $.noop,
                setValue: $.noop,
                saveState: $.noop,
                keepWithNext: false,
                hookChanged: null,
                unhookChanged: null,
                inputWidth: VRS.InputWidth.Auto,
                visible: true
            }, settings);
            this._Settings = settings;
            this._Dispatcher = new VRS.EventHandler({ name: settings.dispatcherName });
        }
        OptionField.prototype.getName = function () {
            return this._Settings.name;
        };
        OptionField.prototype.getControlType = function () {
            return this._Settings.controlType;
        };
        OptionField.prototype.getKeepWithNext = function () {
            return this._Settings.keepWithNext;
        };
        OptionField.prototype.setKeepWithNext = function (value) {
            this._Settings.keepWithNext = value;
        };
        OptionField.prototype.getLabelKey = function () {
            return this._Settings.labelKey;
        };
        OptionField.prototype.getLabelText = function () {
            return VRS.globalisation.getText(this._Settings.labelKey);
        };
        OptionField.prototype.getInputWidth = function () {
            return this._Settings.inputWidth;
        };
        OptionField.prototype.getVisible = function () {
            return VRS.Utility.ValueOrFuncReturningValue(this._Settings.visible, true);
        };
        OptionField.prototype.hookRefreshFieldContent = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.refreshFieldContent, callback, forceThis);
        };
        OptionField.prototype.raiseRefreshFieldContent = function () {
            this._Dispatcher.raise(this._Events.refreshFieldContent);
        };
        OptionField.prototype.hookRefreshFieldState = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.refreshFieldState, callback, forceThis);
        };
        OptionField.prototype.raiseRefreshFieldState = function () {
            this._Dispatcher.raise(this._Events.refreshFieldState);
        };
        OptionField.prototype.hookRefreshFieldVisibility = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.refreshFieldVisibility, callback, forceThis);
        };
        OptionField.prototype.raiseRefreshFieldVisibility = function () {
            this._Dispatcher.raise(this._Events.refreshFieldVisibility);
        };
        OptionField.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        OptionField.prototype.getValue = function () {
            return this._Settings.getValue();
        };
        OptionField.prototype.setValue = function (value) {
            this._Settings.setValue(value);
        };
        OptionField.prototype.saveState = function () {
            this._Settings.saveState();
        };
        OptionField.prototype.getInputClass = function () {
            return this.getInputWidth();
        };
        OptionField.prototype.applyInputClass = function (jqElement) {
            var inputClass = this.getInputClass();
            if (inputClass && jqElement) {
                jqElement.addClass(inputClass);
            }
        };
        OptionField.prototype.hookEvents = function (callback, forceThis) {
            if (this._Settings.hookChanged) {
                this._ChangedHookResult = this._Settings.hookChanged(callback, forceThis);
            }
        };
        OptionField.prototype.unhookEvents = function () {
            if (this._Settings.unhookChanged && this._ChangedHookResult) {
                this._Settings.unhookChanged(this._ChangedHookResult);
                this._ChangedHookResult = null;
            }
        };
        return OptionField;
    })();
    VRS.OptionField = OptionField;
    var OptionFieldButton = (function (_super) {
        __extends(OptionFieldButton, _super);
        function OptionFieldButton(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldButton',
                controlType: VRS.optionControlTypes.button,
                icon: null,
                primaryIcon: null,
                secondaryIcon: null,
                showText: true
            }, settings);
            this._Enabled = true;
            _super.call(this, settings);
        }
        OptionFieldButton.prototype.getPrimaryIcon = function () {
            return this._Settings.primaryIcon || this._Settings.icon;
        };
        OptionFieldButton.prototype.getSecondaryIcon = function () {
            return this._Settings.secondaryIcon;
        };
        OptionFieldButton.prototype.getShowText = function () {
            return this._Settings.showText;
        };
        OptionFieldButton.prototype.getEnabled = function () {
            return this._Enabled;
        };
        OptionFieldButton.prototype.setEnabled = function (value) {
            if (value !== this._Enabled) {
                this._Enabled = value;
                this.raiseRefreshFieldState();
            }
        };
        return OptionFieldButton;
    })(OptionField);
    VRS.OptionFieldButton = OptionFieldButton;
    var OptionFieldCheckBox = (function (_super) {
        __extends(OptionFieldCheckBox, _super);
        function OptionFieldCheckBox(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldCheckBox',
                controlType: VRS.optionControlTypes.checkBox
            }, settings);
            _super.call(this, settings);
        }
        return OptionFieldCheckBox;
    })(OptionField);
    VRS.OptionFieldCheckBox = OptionFieldCheckBox;
    var OptionFieldColour = (function (_super) {
        __extends(OptionFieldColour, _super);
        function OptionFieldColour(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldColour',
                controlType: VRS.optionControlTypes.colour,
                inputWidth: VRS.InputWidth.NineChar
            }, settings);
            _super.call(this, settings);
        }
        return OptionFieldColour;
    })(OptionField);
    VRS.OptionFieldColour = OptionFieldColour;
    var OptionFieldComboBox = (function (_super) {
        __extends(OptionFieldComboBox, _super);
        function OptionFieldComboBox(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldComboBox',
                controlType: VRS.optionControlTypes.comboBox,
                values: [],
                changed: $.noop
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldComboBox.prototype.getValues = function () {
            return this._Settings.values;
        };
        OptionFieldComboBox.prototype.callChangedCallback = function (selectedValue) {
            this._Settings.changed(selectedValue);
        };
        return OptionFieldComboBox;
    })(OptionField);
    VRS.OptionFieldComboBox = OptionFieldComboBox;
    var OptionFieldDate = (function (_super) {
        __extends(OptionFieldDate, _super);
        function OptionFieldDate(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldDate',
                controlType: VRS.optionControlTypes.date,
                defaultDate: null,
                minDate: null,
                maxDate: null
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldDate.prototype.getDefaultDate = function () {
            return this._Settings.defaultDate;
        };
        OptionFieldDate.prototype.getMinDate = function () {
            return this._Settings.minDate;
        };
        OptionFieldDate.prototype.getMaxDate = function () {
            return this._Settings.maxDate;
        };
        return OptionFieldDate;
    })(OptionField);
    VRS.OptionFieldDate = OptionFieldDate;
    var OptionFieldLabel = (function (_super) {
        __extends(OptionFieldLabel, _super);
        function OptionFieldLabel(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldLabel',
                controlType: VRS.optionControlTypes.label,
                labelWidth: VRS.LabelWidth.Auto
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldLabel.prototype.getLabelWidth = function () {
            return this._Settings.labelWidth;
        };
        return OptionFieldLabel;
    })(OptionField);
    VRS.OptionFieldLabel = OptionFieldLabel;
    var OptionFieldLinkLabel = (function (_super) {
        __extends(OptionFieldLinkLabel, _super);
        function OptionFieldLinkLabel(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldLinkLabel',
                controlType: VRS.optionControlTypes.linkLabel,
                getHref: $.noop,
                getTarget: $.noop
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldLinkLabel.prototype.getHref = function () {
            return this._Settings.getHref() || '#';
        };
        OptionFieldLinkLabel.prototype.getTarget = function () {
            return this._Settings.getTarget() || null;
        };
        return OptionFieldLinkLabel;
    })(OptionFieldLabel);
    VRS.OptionFieldLinkLabel = OptionFieldLinkLabel;
    var OptionFieldNumeric = (function (_super) {
        __extends(OptionFieldNumeric, _super);
        function OptionFieldNumeric(settings) {
            settings = $.extend({
                name: null,
                dispatcherName: 'VRS.OptionFieldNumeric',
                controlType: VRS.optionControlTypes.numeric,
                min: undefined,
                max: undefined,
                decimals: undefined,
                step: 1,
                showSlider: false,
                sliderStep: undefined,
                allowNullValue: false
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldNumeric.prototype.getMin = function () {
            return this._Settings.min;
        };
        OptionFieldNumeric.prototype.getMax = function () {
            return this._Settings.max;
        };
        OptionFieldNumeric.prototype.getDecimals = function () {
            return this._Settings.decimals;
        };
        OptionFieldNumeric.prototype.getStep = function () {
            return this._Settings.step;
        };
        OptionFieldNumeric.prototype.showSlider = function () {
            return this._Settings.showSlider;
        };
        OptionFieldNumeric.prototype.getSliderStep = function () {
            return this._Settings.sliderStep === undefined ? this._Settings.step : this._Settings.sliderStep;
        };
        OptionFieldNumeric.prototype.getAllowNullValue = function () {
            return this._Settings.allowNullValue;
        };
        return OptionFieldNumeric;
    })(OptionField);
    VRS.OptionFieldNumeric = OptionFieldNumeric;
    var OptionFieldOrderedSubset = (function (_super) {
        __extends(OptionFieldOrderedSubset, _super);
        function OptionFieldOrderedSubset(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldOrderedSubset',
                controlType: VRS.optionControlTypes.orderedSubset,
                values: [],
                keepValuesSorted: false
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldOrderedSubset.prototype.getValues = function () {
            return this._Settings.values;
        };
        OptionFieldOrderedSubset.prototype.getKeepValuesSorted = function () {
            return this._Settings.keepValuesSorted;
        };
        return OptionFieldOrderedSubset;
    })(OptionField);
    VRS.OptionFieldOrderedSubset = OptionFieldOrderedSubset;
    var OptionFieldPaneList = (function (_super) {
        __extends(OptionFieldPaneList, _super);
        function OptionFieldPaneList(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldPaneList',
                controlType: VRS.optionControlTypes.paneList,
                panes: [],
                maxPanes: -1,
                addPane: null,
                suppressRemoveButton: false,
                refreshAddControls: function (disabled, addParentJQ) {
                    $(':input', addParentJQ).prop('disabled', disabled);
                    $(':button', addParentJQ).button('option', 'disabled', disabled);
                }
            }, settings);
            this._PaneListEvents = {
                paneAdded: 'paneAdded',
                paneRemoved: 'paneRemoved',
                maxPanesChanged: 'maxPanesChanged'
            };
            _super.call(this, settings);
        }
        OptionFieldPaneList.prototype.getMaxPanes = function () {
            return this._Settings.maxPanes;
        };
        OptionFieldPaneList.prototype.setMaxPanes = function (value) {
            if (value !== this._Settings.maxPanes) {
                this._Settings.maxPanes = value;
                this.trimExcessPanes();
                this.onMaxPanesChanged();
            }
        };
        OptionFieldPaneList.prototype.getPanes = function () {
            return this._Settings.panes;
        };
        OptionFieldPaneList.prototype.getAddPane = function () {
            return this._Settings.addPane;
        };
        OptionFieldPaneList.prototype.setAddPane = function (value) {
            this._Settings.addPane = value;
        };
        OptionFieldPaneList.prototype.getSuppressRemoveButton = function () {
            return this._Settings.suppressRemoveButton;
        };
        OptionFieldPaneList.prototype.setSuppressRemoveButton = function (value) {
            this._Settings.suppressRemoveButton = value;
        };
        OptionFieldPaneList.prototype.getRefreshAddControls = function () {
            return this._Settings.refreshAddControls;
        };
        OptionFieldPaneList.prototype.setRefreshAddControls = function (value) {
            this._Settings.refreshAddControls = value;
        };
        OptionFieldPaneList.prototype.hookPaneAdded = function (callback, forceThis) {
            return this._Dispatcher.hook(this._PaneListEvents.paneAdded, callback, forceThis);
        };
        OptionFieldPaneList.prototype.onPaneAdded = function (pane, index) {
            this._Dispatcher.raise(this._PaneListEvents.paneAdded, [pane, index]);
        };
        OptionFieldPaneList.prototype.hookPaneRemoved = function (callback, forceThis) {
            return this._Dispatcher.hook(this._PaneListEvents.paneRemoved, callback, forceThis);
        };
        OptionFieldPaneList.prototype.onPaneRemoved = function (pane, index) {
            this._Dispatcher.raise(this._PaneListEvents.paneRemoved, [pane, index]);
        };
        OptionFieldPaneList.prototype.hookMaxPanesChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._PaneListEvents.maxPanesChanged, callback, forceThis);
        };
        OptionFieldPaneList.prototype.onMaxPanesChanged = function () {
            this._Dispatcher.raise(this._PaneListEvents.maxPanesChanged);
        };
        OptionFieldPaneList.prototype.addPane = function (pane, index) {
            if (index !== undefined) {
                this._Settings.panes.splice(index, 0, pane);
                this.onPaneAdded(pane, index);
            }
            else {
                this._Settings.panes.push(pane);
                this.onPaneAdded(pane, this._Settings.panes.length - 1);
            }
        };
        OptionFieldPaneList.prototype.removePane = function (pane) {
            var index = this.findPaneIndex(pane);
            if (index === -1)
                throw 'Cannot find the pane to remove';
            this.removePaneAt(index);
        };
        OptionFieldPaneList.prototype.trimExcessPanes = function () {
            if (this._Settings.maxPanes !== -1) {
                while (this._Settings.maxPanes > this._Settings.panes.length) {
                    this.removePane(this._Settings.panes[this._Settings.panes.length - 1]);
                }
            }
        };
        OptionFieldPaneList.prototype.findPaneIndex = function (pane) {
            var result = -1;
            var length = this._Settings.panes.length;
            for (var i = 0; i < length; ++i) {
                if (this._Settings.panes[i] === pane) {
                    result = i;
                    break;
                }
            }
            return result;
        };
        OptionFieldPaneList.prototype.removePaneAt = function (index) {
            var pane = this._Settings.panes[index];
            this._Settings.panes.splice(index, 1);
            pane.dispose(null);
            this.onPaneRemoved(pane, index);
        };
        return OptionFieldPaneList;
    })(OptionField);
    VRS.OptionFieldPaneList = OptionFieldPaneList;
    var OptionFieldRadioButton = (function (_super) {
        __extends(OptionFieldRadioButton, _super);
        function OptionFieldRadioButton(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldRadioButton',
                controlType: VRS.optionControlTypes.radioButton,
                values: []
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldRadioButton.prototype.getValues = function () {
            return this._Settings.values;
        };
        return OptionFieldRadioButton;
    })(OptionField);
    VRS.OptionFieldRadioButton = OptionFieldRadioButton;
    var OptionFieldTextBox = (function (_super) {
        __extends(OptionFieldTextBox, _super);
        function OptionFieldTextBox(settings) {
            settings = $.extend({
                dispatcherName: 'VRS.OptionFieldTextBox',
                controlType: VRS.optionControlTypes.textBox,
                upperCase: false,
                lowerCase: false,
                maxLength: undefined
            }, settings);
            _super.call(this, settings);
        }
        OptionFieldTextBox.prototype.getUpperCase = function () {
            return this._Settings.upperCase;
        };
        OptionFieldTextBox.prototype.getLowerCase = function () {
            return this._Settings.lowerCase;
        };
        OptionFieldTextBox.prototype.getMaxLength = function () {
            return this._Settings.maxLength;
        };
        return OptionFieldTextBox;
    })(OptionField);
    VRS.OptionFieldTextBox = OptionFieldTextBox;
    var OptionPane = (function () {
        function OptionPane(settings) {
            this._OptionFields = [];
            this._Generation = 0;
            if (!settings)
                throw 'You must supply settings';
            if (!settings.name)
                throw 'You must supply a name for the pane';
            settings = $.extend({
                name: null,
                titleKey: null,
                displayOrder: 0,
                fields: [],
                dispose: $.noop,
                pageParentCreated: $.noop
            }, settings);
            this._Settings = settings;
            if (settings.fields) {
                for (var i = 0; i < settings.fields.length; ++i) {
                    this.addField(settings.fields[i]);
                }
            }
        }
        OptionPane.prototype.getName = function () {
            return this._Settings.name;
        };
        OptionPane.prototype.getTitleKey = function () {
            return this._Settings.titleKey;
        };
        OptionPane.prototype.getTitleText = function () {
            return VRS.globalisation.getText(this._Settings.titleKey);
        };
        OptionPane.prototype.setTitleKey = function (value) {
            this._Settings.titleKey = value;
        };
        OptionPane.prototype.getDisplayOrder = function () {
            return this._Settings.displayOrder;
        };
        OptionPane.prototype.setDisplayOrder = function (value) {
            this._Settings.displayOrder = value;
        };
        OptionPane.prototype.getFieldCount = function () {
            return this._OptionFields.length;
        };
        OptionPane.prototype.getField = function (idx) {
            return this._OptionFields[idx];
        };
        OptionPane.prototype.getFieldByName = function (optionFieldName) {
            var index = this.findIndexByName(optionFieldName);
            return index === -1 ? null : this.getField(index);
        };
        OptionPane.prototype.dispose = function (options) {
            this._Settings.dispose(options);
        };
        OptionPane.prototype.pageParentCreated = function (optionPageParent) {
            this._Settings.pageParentCreated(optionPageParent);
        };
        OptionPane.prototype.addField = function (optionField) {
            var existingIndex = this.findIndexByName(optionField.getName());
            if (existingIndex !== -1)
                throw 'There is already a field in this pane called ' + optionField.getName();
            this._OptionFields.push(optionField);
            ++this._Generation;
        };
        OptionPane.prototype.removeFieldByName = function (optionFieldName) {
            var index = this.findIndexByName(optionFieldName);
            if (index === -1)
                throw 'Cannot remove option field ' + optionFieldName + ', it does not exist.';
            this._OptionFields.splice(index, 1);
            ++this._Generation;
        };
        OptionPane.prototype.foreachField = function (callback) {
            var length = this._OptionFields.length;
            var generation = this._Generation;
            for (var i = 0; i < length; ++i) {
                callback(this._OptionFields[i]);
                if (this._Generation !== generation) {
                    throw 'Cannot continue to iterate through the fields after the collection has been modified';
                }
            }
        };
        OptionPane.prototype.findIndexByName = function (optionFieldName) {
            var result = -1;
            $.each(this._OptionFields, function (idx, val) {
                var breakLoop = val.getName() === optionFieldName;
                if (breakLoop)
                    result = idx;
                return !breakLoop;
            });
            return result;
        };
        return OptionPane;
    })();
    VRS.OptionPane = OptionPane;
    var OptionPage = (function () {
        function OptionPage(settings) {
            this._OptionPanes = [];
            this._Generation = 0;
            this._SortGeneration = -1;
            if (!settings)
                throw 'You must supply settings';
            if (!settings.name)
                throw 'You must give the page a name';
            this._Settings = settings;
            if (settings.panes) {
                for (var i = 0; i < settings.panes.length; ++i) {
                    this.addPane(settings.panes[i]);
                }
            }
        }
        OptionPage.prototype.getName = function () {
            return this._Settings.name;
        };
        OptionPage.prototype.setName = function (value) {
            this._Settings.name = value;
        };
        OptionPage.prototype.getTitleKey = function () {
            return this._Settings.titleKey;
        };
        OptionPage.prototype.setTitleKey = function (value) {
            this._Settings.titleKey = value;
        };
        OptionPage.prototype.getDisplayOrder = function () {
            return this._Settings.displayOrder;
        };
        OptionPage.prototype.setDisplayOrder = function (value) {
            if (!isNaN(value)) {
                this._Settings.displayOrder = value;
            }
        };
        OptionPage.prototype.addPane = function (optionPane) {
            if (!(optionPane instanceof VRS.OptionPane)) {
                var length = optionPane.length;
                for (var i = 0; i < length; ++i) {
                    this.addPane(optionPane[i]);
                }
            }
            else {
                var index = this.findIndexByName(optionPane.getName());
                if (index !== -1)
                    throw 'There is already a pane on this page called ' + optionPane.getName();
                this._OptionPanes.push(optionPane);
                ++this._Generation;
            }
        };
        OptionPage.prototype.removePaneByName = function (optionPaneName) {
            var index = this.findIndexByName(optionPaneName);
            if (index === -1)
                throw 'There is no pane called ' + optionPaneName;
            this._OptionPanes.splice(index, 1);
            ++this._Generation;
        };
        OptionPage.prototype.foreachPane = function (callback) {
            this.sortPanes();
            var generation = this._Generation;
            var length = this._OptionPanes.length;
            for (var i = 0; i < length; ++i) {
                callback(this._OptionPanes[i]);
                if (this._Generation != generation)
                    throw 'Cannot continue to iterate through the panes, they have been changed';
            }
        };
        OptionPage.prototype.findIndexByName = function (optionPaneName) {
            var result = -1;
            $.each(this._OptionPanes, function (idx, val) {
                var foundMatch = val.getName() === optionPaneName;
                if (foundMatch)
                    result = idx;
                return !foundMatch;
            });
            return result;
        };
        OptionPage.prototype.sortPanes = function () {
            if (this._SortGeneration !== this._Generation) {
                this._OptionPanes.sort(function (lhs, rhs) {
                    return lhs.getDisplayOrder() - rhs.getDisplayOrder();
                });
                this._SortGeneration = this._Generation;
            }
        };
        return OptionPage;
    })();
    VRS.OptionPage = OptionPage;
    var OptionPageParent = (function () {
        function OptionPageParent() {
            this._Dispatcher = new VRS.EventHandler({ name: 'VRS.OptionPageParent' });
            this._Events = {
                fieldChanged: 'fieldChanged'
            };
        }
        OptionPageParent.prototype.hookFieldChanged = function (callback, forceThis) {
            return this._Dispatcher.hook(this._Events.fieldChanged, callback, forceThis);
        };
        OptionPageParent.prototype.raiseFieldChanged = function () {
            this._Dispatcher.raise(this._Events.fieldChanged);
        };
        OptionPageParent.prototype.unhook = function (hookResult) {
            this._Dispatcher.unhook(hookResult);
        };
        return OptionPageParent;
    })();
    VRS.OptionPageParent = OptionPageParent;
    var OptionControlTypeBroker = (function () {
        function OptionControlTypeBroker() {
            this._ControlTypes = {};
        }
        OptionControlTypeBroker.prototype.addControlTypeHandler = function (controlType, creatorCallback) {
            if (this._ControlTypes[controlType])
                throw 'There is already a handler registered for ' + controlType + ' control types';
            this._ControlTypes[controlType] = creatorCallback;
        };
        OptionControlTypeBroker.prototype.addControlTypeHandlerIfNotRegistered = function (controlType, creatorCallback) {
            if (!this.controlTypeHasHandler(controlType)) {
                this.addControlTypeHandler(controlType, creatorCallback);
            }
        };
        OptionControlTypeBroker.prototype.removeControlTypeHandler = function (controlType) {
            if (!this._ControlTypes[controlType])
                throw 'There is no handler registered for ' + controlType + ' control types';
            delete this._ControlTypes[controlType];
        };
        OptionControlTypeBroker.prototype.controlTypeHasHandler = function (controlType) {
            return !!this._ControlTypes[controlType];
        };
        OptionControlTypeBroker.prototype.createControlTypeHandler = function (options) {
            var controlType = options.field.getControlType();
            var creator = this._ControlTypes[controlType];
            if (!creator)
                throw 'There is no handler registered for ' + controlType + ' control types';
            return creator(options);
        };
        return OptionControlTypeBroker;
    })();
    VRS.OptionControlTypeBroker = OptionControlTypeBroker;
    VRS.optionControlTypeBroker = new VRS.OptionControlTypeBroker();
    VRS.optionControlTypes = VRS.optionControlTypes || {};
    VRS.optionControlTypes.button = 'vrsButton';
    VRS.optionControlTypes.checkBox = 'vrsCheckBox';
    VRS.optionControlTypes.colour = 'vrsColour';
    VRS.optionControlTypes.comboBox = 'vrsComboBox';
    VRS.optionControlTypes.date = 'vrsDate';
    VRS.optionControlTypes.label = 'vrsLabel';
    VRS.optionControlTypes.linkLabel = 'vrsLinkLabel';
    VRS.optionControlTypes.numeric = 'vrsNumeric';
    VRS.optionControlTypes.orderedSubset = 'vrsOrderedSubset';
    VRS.optionControlTypes.paneList = 'vrsPaneList';
    VRS.optionControlTypes.radioButton = 'vrsRadioButton';
    VRS.optionControlTypes.textBox = 'vrsTextBox';
})(VRS || (VRS = {}));
//# sourceMappingURL=optionsUI.js.map