var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldLinkLabelPlugin_State = (function () {
        function OptionFieldLinkLabelPlugin_State() {
            this.refreshFieldContentHookResult = null;
        }
        return OptionFieldLinkLabelPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldLinkLabelPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldLinkLabel');
    };
    VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldLinkLabelPlugin = (function (_super) {
        __extends(OptionFieldLinkLabelPlugin, _super);
        function OptionFieldLinkLabelPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions();
            return _this;
        }
        OptionFieldLinkLabelPlugin.prototype._getState = function () {
            var result = this.element.data('optionFieldLinkLabelState');
            if (result === undefined) {
                result = new OptionFieldLinkLabelPlugin_State();
                this.element.data('optionFieldLinkLabelState', result);
            }
            return result;
        };
        OptionFieldLinkLabelPlugin.prototype._create = function () {
            var state = this._getState();
            var field = this.options.field;
            this._setProperties();
            state.refreshFieldContentHookResult = field.hookRefreshFieldContent(this._setProperties, this);
        };
        OptionFieldLinkLabelPlugin.prototype._setProperties = function () {
            var state = this._getState();
            var field = this.options.field;
            this.element
                .text(field.getLabelText())
                .attr('href', field.getHref())
                .attr('target', field.getTarget());
            this.element.removeClass('short long');
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
        };
        OptionFieldLinkLabelPlugin.prototype._destroy = function () {
            var state = this._getState();
            var field = this.options.field;
            if (state.refreshFieldContentHookResult) {
                field.unhook(state.refreshFieldContentHookResult);
            }
            state.refreshFieldContentHookResult = null;
        };
        return OptionFieldLinkLabelPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldLinkLabelPlugin = OptionFieldLinkLabelPlugin;
    $.widget('vrs.vrsOptionFieldLinkLabel', new OptionFieldLinkLabelPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.linkLabel, function (settings) {
            return $('<a/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldLinkLabel(VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldLinkLabel.js.map