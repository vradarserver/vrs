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
    var OptionFieldButtonPlugin_State = (function () {
        function OptionFieldButtonPlugin_State() {
            this.refreshFieldContentHookResult = null;
            this.refreshFieldStateHookResult = null;
        }
        return OptionFieldButtonPlugin_State;
    }());
    VRS.OptionFieldButtonPlugin_State = OptionFieldButtonPlugin_State;
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldButtonPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldButton');
    };
    VRS.jQueryUIHelper.getOptionFieldButtonOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldButtonPlugin = (function (_super) {
        __extends(OptionFieldButtonPlugin, _super);
        function OptionFieldButtonPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldButtonOptions();
            return _this;
        }
        OptionFieldButtonPlugin.prototype._getState = function () {
            var result = this.element.data('optionFieldButtonState');
            if (result === undefined) {
                result = new OptionFieldButtonPlugin_State();
                this.element.data('optionFieldButtonState', result);
            }
            return result;
        };
        OptionFieldButtonPlugin.prototype._create = function () {
            var state = this._getState();
            var field = this.options.field;
            var optionPageParent = this.options.optionPageParent;
            var getTextOption = function () { return field.getShowText(); };
            var getIconsOption = function () {
                var result = undefined;
                if (field.getPrimaryIcon()) {
                    if (!field.getSecondaryIcon())
                        result = { primary: 'ui-icon-' + field.getPrimaryIcon() };
                    else
                        result = { primary: 'ui-icon-' + field.getPrimaryIcon(), secondary: 'ui-icon-' + field.getSecondaryIcon() };
                }
                return result;
            };
            var jqSettings = {
                text: getTextOption(),
                icons: getIconsOption()
            };
            state.refreshFieldContentHookResult = field.hookRefreshFieldContent(function () {
                this.element.button('option', 'text', getTextOption());
                var icons = getIconsOption();
                if (icons)
                    this.element.button('option', 'icons', icons);
                this.element.text(field.getLabelText());
            }, this);
            state.refreshFieldStateHookResult = field.hookRefreshFieldState($.proxy(function () {
                if (field.getEnabled())
                    this.element.button('enable');
                else
                    this.element.button('disable');
            }, this), this);
            jqSettings.disabled = !field.getEnabled();
            this.element
                .text(field.getLabelText())
                .button(jqSettings)
                .click(function () {
                if (VRS.timeoutManager) {
                    VRS.timeoutManager.resetTimer();
                }
                field.saveState();
                optionPageParent.raiseFieldChanged();
            });
            field.applyInputClass(this.element);
        };
        OptionFieldButtonPlugin.prototype._destroy = function () {
            var state = this._getState();
            var field = this.options.field;
            if (state.refreshFieldContentHookResult)
                field.unhook(state.refreshFieldContentHookResult);
            if (state.refreshFieldStateHookResult)
                field.unhook(state.refreshFieldStateHookResult);
            state.refreshFieldContentHookResult = null;
            state.refreshFieldStateHookResult = null;
            this.element.button('destroy');
            this.element.off();
        };
        return OptionFieldButtonPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldButtonPlugin = OptionFieldButtonPlugin;
    $.widget('vrs.vrsOptionFieldButton', new OptionFieldButtonPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.button, function (settings) {
            return $('<button/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldButton(VRS.jQueryUIHelper.getOptionFieldButtonOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldButton.js.map