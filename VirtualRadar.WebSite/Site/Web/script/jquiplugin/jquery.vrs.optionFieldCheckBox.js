var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionFieldCheckBoxPlugin_State = (function () {
        function OptionFieldCheckBoxPlugin_State() {
            this.suppressFieldSet = false;
        }
        return OptionFieldCheckBoxPlugin_State;
    })();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldCheckBoxPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionFieldCheckBox');
    };
    VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions = function (overrides) {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    };
    var OptionFieldCheckBoxPlugin = (function (_super) {
        __extends(OptionFieldCheckBoxPlugin, _super);
        function OptionFieldCheckBoxPlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions();
        }
        OptionFieldCheckBoxPlugin.prototype._getState = function () {
            var result = this.element.data('optionFieldCheckBoxState');
            if (result === undefined) {
                result = new OptionFieldCheckBoxPlugin_State();
                this.element.data('optionFieldCheckBoxState', result);
            }
            return result;
        };
        OptionFieldCheckBoxPlugin.prototype._create = function () {
            var field = this.options.field;
            var state = this._getState();
            var optionPageParent = this.options.optionPageParent;
            var checkbox = this.element
                .uniqueId()
                .attr('type', 'checkbox')
                .prop('checked', field.getValue());
            if (field.getLabelKey()) {
                $('<label/>')
                    .attr('for', checkbox.attr('id'))
                    .text(field.getLabelText())
                    .appendTo(this.element.parent().first());
            }
            checkbox.change(function () {
                if (VRS.timeoutManager)
                    VRS.timeoutManager.resetTimer();
                if (!state.suppressFieldSet) {
                    field.setValue(checkbox.prop('checked'));
                    field.saveState();
                    optionPageParent.raiseFieldChanged();
                }
            });
            field.hookEvents(function () {
                var suppressFieldSet = state.suppressFieldSet;
                state.suppressFieldSet = true;
                checkbox.prop('checked', field.getValue());
                state.suppressFieldSet = suppressFieldSet;
            }, this);
        };
        OptionFieldCheckBoxPlugin.prototype._destroy = function () {
            var field = this.options.field;
            field.unhookEvents();
        };
        return OptionFieldCheckBoxPlugin;
    })(JQueryUICustomWidget);
    VRS.OptionFieldCheckBoxPlugin = OptionFieldCheckBoxPlugin;
    $.widget('vrs.vrsOptionFieldCheckBox', new OptionFieldCheckBoxPlugin());
    if (VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(VRS.optionControlTypes.checkBox, function (settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldCheckBox(VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions(settings));
        });
    }
})(VRS || (VRS = {}));
