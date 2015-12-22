var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldComboBoxPlugin_State = (function () {
        function OptionFieldComboBoxPlugin_State() {
            this.labelElement = null;
            this.refreshFieldVisibilityHookResult = null;
        }
        return OptionFieldComboBoxPlugin_State;
    })();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldComboBoxPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldComboBox');
    };
    VRS.jQueryUIHelper.getOptionFieldComboBoxOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldComboBoxPlugin = (function (_super) {
        __extends(OptionFieldComboBoxPlugin, _super);
        function OptionFieldComboBoxPlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getOptionFieldComboBoxOptions();
        }
        OptionFieldComboBoxPlugin.prototype._getState = function () {
            var result = this.element.data('optionFieldComboBoxState');
            if (result === undefined) {
                result = new OptionFieldComboBoxPlugin_State();
                this.element.data('optionFieldComboBoxState', result);
            }
            return result;
        };
        OptionFieldComboBoxPlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var comboBox = this.element
                .uniqueId();
            field.applyInputClass(comboBox);
            if (field.getLabelKey()) {
                state.labelElement = $('<label/>')
                    .text(field.getLabelText() + ':')
                    .attr('for', comboBox.attr('id'))
                    .insertBefore(this.element);
            }
            var selectedValue = field.getValue();
            var dropDownListValues = field.getValues();
            var selectedValueExists = false;
            var firstValue = undefined;
            for (var i = 0; i < dropDownListValues.length; ++i) {
                var valueText = dropDownListValues[i];
                var value = valueText.getValue();
                if (i === 0)
                    firstValue = value;
                if (value == selectedValue) {
                    selectedValueExists = true;
                }
                var option = $('<option/>')
                    .attr('value', value)
                    .text(valueText.getText())
                    .appendTo(comboBox);
            }
            if (!selectedValueExists) {
                selectedValue = firstValue;
                field.setValue(selectedValue);
            }
            comboBox.val(selectedValue);
            field.callChangedCallback(selectedValue);
            this._setVisibility(state, true);
            state.refreshFieldVisibilityHookResult = field.hookRefreshFieldVisibility(this._fieldRefreshVisibility, this);
            comboBox.change(function () {
                if (VRS.timeoutManager)
                    VRS.timeoutManager.resetTimer();
                var newValue = comboBox.val();
                field.setValue(newValue);
                field.saveState();
                field.callChangedCallback(newValue);
                options.optionPageParent.raiseFieldChanged();
            });
        };
        OptionFieldComboBoxPlugin.prototype._destroy = function () {
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            if (state.refreshFieldVisibilityHookResult) {
                field.unhook(state.refreshFieldVisibilityHookResult);
                state.refreshFieldVisibilityHookResult = null;
            }
            if (state.labelElement) {
                state.labelElement.remove();
                state.labelElement = null;
            }
            this.element.off();
        };
        OptionFieldComboBoxPlugin.prototype._setVisibility = function (state, assumeVisible) {
            if (!state)
                state = this._getState();
            var options = this.options;
            var field = options.field;
            var visible = field.getVisible();
            if (assumeVisible || this.element.is(':visible')) {
                if (!visible) {
                    this.element.hide();
                    if (state.labelElement) {
                        state.labelElement.hide();
                    }
                }
            }
            else {
                if (visible) {
                    this.element.show();
                    if (state.labelElement) {
                        state.labelElement.show();
                    }
                }
            }
        };
        OptionFieldComboBoxPlugin.prototype._fieldRefreshVisibility = function () {
            this._setVisibility(null);
        };
        return OptionFieldComboBoxPlugin;
    })(JQueryUICustomWidget);
    VRS.OptionFieldComboBoxPlugin = OptionFieldComboBoxPlugin;
    $.widget('vrs.vrsOptionFieldComboBox', new OptionFieldComboBoxPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.comboBox, function (settings) {
            return $('<select/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldComboBox(VRS.jQueryUIHelper.getOptionFieldComboBoxOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldComboBox.js.map