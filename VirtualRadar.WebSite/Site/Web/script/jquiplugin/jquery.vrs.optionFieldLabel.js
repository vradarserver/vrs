var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldLabelPlugin_State = (function () {
        function OptionFieldLabelPlugin_State() {
            this.refreshFieldContentHookResult = null;
        }
        return OptionFieldLabelPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldLabelPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldLabel');
    };
    VRS.jQueryUIHelper.getOptionFieldLabelOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldLabelPlugin = (function (_super) {
        __extends(OptionFieldLabelPlugin, _super);
        function OptionFieldLabelPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldLabelOptions();
            return _this;
        }
        OptionFieldLabelPlugin.prototype._getState = function () {
            var result = this.element.data('optionFieldLabelState');
            if (result === undefined) {
                result = new OptionFieldLabelPlugin_State();
                this.element.data('optionFieldLabelState', result);
            }
            return result;
        };
        OptionFieldLabelPlugin.prototype._create = function () {
            var state = this._getState();
            var field = this.options.field;
            this.element
                .text(field.getLabelText());
            switch (field.getLabelWidth()) {
                case VRS.LabelWidth.Auto: break;
                case VRS.LabelWidth.Short:
                    this.element.addClass('short');
                    break;
                case VRS.LabelWidth.Long:
                    this.element.addClass('long');
                    break;
                default: throw 'Unknown label width ' + field.getLabelWidth();
            }
            state.refreshFieldContentHookResult = field.hookRefreshFieldContent(function () {
                this.element.text(field.getLabelText());
            }, this);
        };
        OptionFieldLabelPlugin.prototype._destroy = function () {
            var state = this._getState();
            var field = this.options.field;
            if (state.refreshFieldContentHookResult) {
                field.unhook(state.refreshFieldContentHookResult);
            }
            state.refreshFieldContentHookResult = null;
        };
        return OptionFieldLabelPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldLabelPlugin = OptionFieldLabelPlugin;
    $.widget('vrs.vrsOptionFieldLabel', new OptionFieldLabelPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.label, function (settings) {
            return $('<span/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldLabel(VRS.jQueryUIHelper.getOptionFieldLabelOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldLabel.js.map