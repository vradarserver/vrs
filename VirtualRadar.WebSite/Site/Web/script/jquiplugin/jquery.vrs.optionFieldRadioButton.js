var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var VRS;
(function (VRS) {
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldRadioButtonPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldRadioButton');
    };
    VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldRadioButtonPlugin = (function (_super) {
        __extends(OptionFieldRadioButtonPlugin, _super);
        function OptionFieldRadioButtonPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions();
            return _this;
        }
        OptionFieldRadioButtonPlugin.prototype._create = function () {
            var options = this.options;
            var field = options.field;
            var values = field.getValues();
            var container = this.element
                .uniqueId()
                .addClass('vrsOptionRadioButton');
            if (field.getLabelKey()) {
                $('<span/>')
                    .addClass('asLabel')
                    .text(VRS.globalisation.getText(field.getLabelKey()) + ':')
                    .appendTo(container);
            }
            var currentValue = field.getValue();
            var name = field.getName() + container.attr('id');
            var first = true;
            $.each(values, function (idx, value) {
                var wrapper = $('<div/>')
                    .appendTo(container);
                var input = $('<input/>')
                    .uniqueId()
                    .attr('type', 'radio')
                    .attr('value', value.getValue())
                    .attr('name', name)
                    .click(function () {
                    if (VRS.timeoutManager)
                        VRS.timeoutManager.resetTimer();
                    field.setValue(value.getValue());
                    field.saveState();
                    options.optionPageParent.raiseFieldChanged();
                })
                    .appendTo(wrapper);
                var label = $('<label/>')
                    .attr('for', input.attr('id'))
                    .text(value.getText())
                    .appendTo(wrapper);
                if (value.getValue() === currentValue) {
                    input.prop('checked', true);
                }
                first = false;
            });
        };
        OptionFieldRadioButtonPlugin.prototype._destroy = function () {
            $.each(this.element.children('input'), function (idx, input) {
                $(input).off();
            });
            this.element.empty();
        };
        return OptionFieldRadioButtonPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldRadioButtonPlugin = OptionFieldRadioButtonPlugin;
    $.widget('vrs.vrsOptionFieldRadioButton', new OptionFieldRadioButtonPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.radioButton, function (settings) {
            return $('<div/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldRadioButton(VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldRadioButton.js.map