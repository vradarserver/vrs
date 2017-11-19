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
    VRS.jQueryUIHelper.getOptionFieldDatePlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldDate');
    };
    VRS.jQueryUIHelper.getOptionFieldDateOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldDatePlugin = (function (_super) {
        __extends(OptionFieldDatePlugin, _super);
        function OptionFieldDatePlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldDateOptions();
            return _this;
        }
        OptionFieldDatePlugin.prototype._create = function () {
            var options = this.options;
            var field = options.field;
            var input = this.element;
            var dateTimePickerOptions = {
                defaultDate: field.getDefaultDate(),
                minDate: field.getMinDate(),
                maxDate: field.getMaxDate(),
                onSelect: function () { input.change(); }
            };
            $.extend(dateTimePickerOptions, VRS.globalisation.getDatePickerOptions());
            input.change(function () {
                if (VRS.timeoutManager)
                    VRS.timeoutManager.resetTimer();
                var val = input.datepicker('getDate');
                if (val !== null) {
                    var minDate = field.getMinDate();
                    var maxDate = field.getMaxDate();
                    if (minDate && minDate.getTime() !== 0 && val < minDate)
                        val = minDate;
                    if (maxDate && maxDate.getTime() !== 0 && val > maxDate)
                        val = maxDate;
                    field.setValue(val);
                    field.saveState();
                    options.optionPageParent.raiseFieldChanged();
                }
            });
            var label = null;
            if (field.getLabelKey()) {
                label = $('<label/>')
                    .text(field.getLabelText() + ':')
                    .insertBefore(this.element);
            }
            this.element
                .uniqueId()
                .datepicker(dateTimePickerOptions);
            field.applyInputClass(this.element);
            this.element.val($.datepicker.formatDate(dateTimePickerOptions.dateFormat, field.getValue(), dateTimePickerOptions));
            if (label) {
                label.attr('for', this.element.attr('id'));
            }
        };
        OptionFieldDatePlugin.prototype._destroy = function () {
            var label = $('label[for="' + this.element.attr('id') + '"');
            label.remove();
            this.element.datepicker('destroy');
            this.element.off();
        };
        return OptionFieldDatePlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldDatePlugin = OptionFieldDatePlugin;
    $.widget('vrs.vrsOptionFieldDate', new OptionFieldDatePlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.date, function (settings) {
            return $('<input/>')
                .attr('type', 'text')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldDate(VRS.jQueryUIHelper.getOptionFieldDateOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldDate.js.map