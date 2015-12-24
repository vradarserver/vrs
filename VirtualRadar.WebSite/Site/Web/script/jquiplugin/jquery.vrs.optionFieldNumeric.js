var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldNumericPlugin_State = (function () {
        function OptionFieldNumericPlugin_State() {
            this.sliderElement = null;
        }
        return OptionFieldNumericPlugin_State;
    })();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldNumericPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldNumeric');
    };
    VRS.jQueryUIHelper.getOptionFieldNumericOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldNumericPlugin = (function (_super) {
        __extends(OptionFieldNumericPlugin, _super);
        function OptionFieldNumericPlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getOptionFieldNumericOptions();
        }
        OptionFieldNumericPlugin.prototype._getState = function () {
            var result = this.element.data('optionFieldNumericState');
            if (result === undefined) {
                result = new OptionFieldNumericPlugin_State();
                this.element.data('optionFieldNumericState', result);
            }
            return result;
        };
        OptionFieldNumericPlugin.prototype._create = function () {
            var _this = this;
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var value = field.getValue();
            var input = this.element;
            var onChange = function () {
                if (VRS.timeoutManager) {
                    VRS.timeoutManager.resetTimer();
                }
                var val = input.spinner('value');
                if (!val === null && !field.getAllowNullValue()) {
                    val = 0;
                }
                var min = field.getMin();
                var max = field.getMax();
                if (min !== undefined && (val || 0) < min)
                    val = min;
                if (max !== undefined && (val || 0) > max)
                    val = max;
                field.setValue(val);
                field.saveState();
                if (state.sliderElement) {
                    state.sliderElement.slider('value', val);
                }
                options.optionPageParent.raiseFieldChanged();
            };
            var spinnerOptions = {
                change: onChange,
                stop: onChange
            };
            if (field.getMin() !== undefined) {
                spinnerOptions.min = field.getMin();
                if (value < spinnerOptions.min)
                    value = spinnerOptions.min;
            }
            if (field.getMax() !== undefined) {
                spinnerOptions.max = field.getMax();
                if (value > spinnerOptions.max)
                    value = spinnerOptions.max;
            }
            if (field.getDecimals() !== undefined) {
                spinnerOptions.numberFormat = 'n' + field.getDecimals().toString();
            }
            spinnerOptions.step = field.getStep();
            var label = null;
            if (field.getLabelKey()) {
                label = $('<label/>')
                    .text(field.getLabelText() + ':')
                    .insertBefore(this.element);
            }
            if (field.showSlider()) {
                state.sliderElement = $('<div/>')
                    .uniqueId()
                    .addClass('vrsSlider')
                    .slider({
                    range: "min",
                    value: value,
                    min: field.getMin(),
                    max: field.getMax(),
                    step: field.getSliderStep(),
                    slide: function (event, ui) {
                        _this.element.spinner('value', ui.value);
                    }
                })
                    .insertAfter(this.element);
            }
            this.element
                .uniqueId()
                .val(value)
                .spinner(spinnerOptions);
            field.applyInputClass(this.element);
            if (label) {
                label.attr('for', this.element.attr('id'));
            }
        };
        OptionFieldNumericPlugin.prototype._destroy = function () {
            var state = this._getState();
            if (state.sliderElement) {
                state.sliderElement.slider('destroy');
                state.sliderElement.off();
                state.sliderElement = null;
            }
            var label = $('label[for="' + this.element.attr('id') + '"]');
            label.remove();
            this.element.spinner('destroy');
            this.element.off();
        };
        return OptionFieldNumericPlugin;
    })(JQueryUICustomWidget);
    VRS.OptionFieldNumericPlugin = OptionFieldNumericPlugin;
    $.widget('vrs.vrsOptionFieldNumeric', new OptionFieldNumericPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.numeric, function (settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldNumeric(VRS.jQueryUIHelper.getOptionFieldNumericOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldNumeric.js.map