var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldColourPlugin_State = (function () {
        function OptionFieldColourPlugin_State() {
            this.colourPicker = null;
        }
        return OptionFieldColourPlugin_State;
    }());
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldColourPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldColour');
    };
    VRS.jQueryUIHelper.getOptionFieldColourOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldColourPlugin = (function (_super) {
        __extends(OptionFieldColourPlugin, _super);
        function OptionFieldColourPlugin() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getOptionFieldColourOptions();
            return _this;
        }
        OptionFieldColourPlugin.prototype._getState = function () {
            var result = this.element.data('optionColourWidgetState');
            if (result === undefined) {
                result = new OptionFieldColourPlugin_State();
                this.element.data('optionColourWidgetState', result);
            }
            return result;
        };
        OptionFieldColourPlugin.prototype._create = function () {
            var _this = this;
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            this.element
                .uniqueId()
                .change(function () {
                if (VRS.timeoutManager) {
                    VRS.timeoutManager.resetTimer();
                }
                var value = _this.element.val();
                field.setValue(value);
                field.saveState();
                options.optionPageParent.raiseFieldChanged();
            });
            field.applyInputClass(this.element);
            this.element.val(field.getValue());
            state.colourPicker = this.element.colourPicker({
                speed: 0,
                title: null,
                colours: this._generateColours()
            });
            if (field.getLabelKey()) {
                $('<label/>')
                    .text(field.getLabelText() + ':')
                    .attr('for', this.element.attr('id'))
                    .insertBefore(this.element);
            }
        };
        OptionFieldColourPlugin.prototype._destroy = function () {
            this.element.off();
        };
        OptionFieldColourPlugin.prototype._generateColours = function () {
            var result = [
                '000000', '202020', '404040', '606060', '808080', 'a0a0a0', 'c0c0c0', 'e0e0e0', 'ffffff'
            ];
            var stepDown = function (doStep, value, step) { return !doStep ? value : Math.max(0, Math.floor(value - step)); };
            var stepUp = function (doStep, value, step) { return !doStep ? value : Math.min(255, Math.ceil(value + step)); };
            var toHex = function (value) { var hex = value.toString(16); return hex.length === 1 ? '0' + hex : hex; };
            var addColour = function (red, green, blue) { result.push(toHex(red) + toHex(green) + toHex(blue)); };
            var addColourWheel = function (iRed, iGreen, iBlue, subRed, subGreen, subBlue, addRed, addGreen, addBlue) {
                var i;
                var steps = 9;
                var step = 256 / 9;
                for (i = 0; i < steps; ++i) {
                    iRed = stepDown(subRed, iRed, step);
                    iGreen = stepDown(subGreen, iGreen, step);
                    iBlue = stepDown(subBlue, iBlue, step);
                    addColour(iRed, iGreen, iBlue);
                }
                for (i = 0; i < steps; ++i) {
                    iRed = stepUp(addRed, iRed, step);
                    iGreen = stepUp(addGreen, iGreen, step);
                    iBlue = stepUp(addBlue, iBlue, step);
                    addColour(iRed, iGreen, iBlue);
                }
            };
            addColourWheel(0, 255, 255, false, false, true, true, false, false);
            addColourWheel(255, 255, 0, false, true, false, false, false, true);
            addColourWheel(255, 0, 255, true, false, false, false, true, false);
            return result;
        };
        return OptionFieldColourPlugin;
    }(JQueryUICustomWidget));
    VRS.OptionFieldColourPlugin = OptionFieldColourPlugin;
    $.widget('vrs.vrsOptionFieldColour', new OptionFieldColourPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.colour, function (settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldColour(VRS.jQueryUIHelper.getOptionFieldColourOptions(settings));
        });
    }
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionFieldColour.js.map