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
    VRS.jQueryUIHelper.getOptionFieldTextBoxPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldTextBox');
    };
    VRS.jQueryUIHelper.getOptionFieldTextBoxOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldTextBoxPlugin = (function (_super) {
        __extends(OptionFieldTextBoxPlugin, _super);
        function OptionFieldTextBoxPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldTextBoxOptions();
            return _this;
        }
        OptionFieldTextBoxPlugin.prototype._create = function () {
            var options = this.options;
            var field = options.field;
            var isUpperCase = field.getUpperCase();
            var isLowerCase = !isUpperCase && field.getLowerCase();
            var value = field.getValue();
            var input = this.element
                .uniqueId()
                .change(function () {
                if (VRS.timeoutManager)
                    VRS.timeoutManager.resetTimer();
                var value = input.val();
                if (value && isUpperCase)
                    value = value.toUpperCase();
                if (value && isLowerCase)
                    value = value.toLowerCase();
                field.setValue(value);
                field.saveState();
                options.optionPageParent.raiseFieldChanged();
            });
            field.applyInputClass(input);
            if (isUpperCase) {
                input.addClass('upperCase');
            }
            else if (isLowerCase) {
                input.addClass('lowerCase');
            }
            if (field.getMaxLength()) {
                input.attr('maxlength', field.getMaxLength());
            }
            input.val(value);
            if (field.getLabelKey()) {
                $('<label/>')
                    .text(field.getLabelText() + ':')
                    .attr('for', input.attr('id'))
                    .insertBefore(this.element);
            }
        };
        OptionFieldTextBoxPlugin.prototype._destroy = function () {
            this.element.off();
        };
        return OptionFieldTextBoxPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldTextBoxPlugin = OptionFieldTextBoxPlugin;
    $.widget('vrs.vrsOptionFieldTextBox', new OptionFieldTextBoxPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.textBox, function (settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldTextBox(VRS.jQueryUIHelper.getOptionFieldTextBoxOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldTextBox.js.map