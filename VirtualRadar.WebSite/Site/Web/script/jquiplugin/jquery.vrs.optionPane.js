var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    var OptionPanePlugin_State = (function () {
        function OptionPanePlugin_State() {
            this.fieldElements = [];
        }
        return OptionPanePlugin_State;
    })();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionPanePlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionPane');
    };
    VRS.jQueryUIHelper.getOptionPaneOptions = function (overrides) {
        return $.extend({
            optionPane: null,
            optionPageParent: null,
            isInStack: false
        }, overrides);
    };
    var OptionPanePlugin = (function (_super) {
        __extends(OptionPanePlugin, _super);
        function OptionPanePlugin() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getOptionPaneOptions();
        }
        OptionPanePlugin.prototype._getState = function () {
            var result = this.element.data('vrsOptionPaneState');
            if (result === undefined) {
                result = new OptionPanePlugin_State();
                this.element.data('vrsOptionPaneState', result);
            }
            return result;
        };
        OptionPanePlugin.prototype._create = function () {
            var state = this._getState();
            var options = this.options;
            var pane = options.optionPane;
            if (!pane)
                throw 'You must supply a VRS.OptionPane object';
            if (pane.getFieldCount() > 0) {
                var paneContainer = this.element
                    .addClass('vrsOptionPane');
                if (options.isInStack)
                    paneContainer.addClass('stacked');
                var titleKey = pane.getTitleKey();
                if (titleKey) {
                    $('<h2/>')
                        .text(VRS.globalisation.getText(titleKey))
                        .appendTo(paneContainer);
                }
                var fieldList = $('<ol/>')
                    .appendTo(paneContainer);
                var fieldContainer = null;
                pane.foreachField(function (field) {
                    var keepTogether = field.getKeepWithNext();
                    var firstKeepTogether = keepTogether && !fieldContainer;
                    var nextKeepTogether = fieldContainer !== null;
                    if (!fieldContainer)
                        fieldContainer = $('<li/>').appendTo(fieldList);
                    if (firstKeepTogether)
                        fieldContainer.addClass('multiField');
                    var fieldParent = fieldContainer;
                    if (nextKeepTogether) {
                        fieldParent = $('<span/>')
                            .addClass('keepWithPrevious')
                            .appendTo(fieldContainer);
                    }
                    var fieldJQ = VRS.optionControlTypeBroker.createControlTypeHandler({
                        field: field,
                        fieldParentJQ: fieldParent,
                        optionPageParent: options.optionPageParent
                    });
                    state.fieldElements.push(fieldJQ);
                    if (!keepTogether)
                        fieldContainer = null;
                });
                pane.pageParentCreated(options.optionPageParent);
            }
        };
        OptionPanePlugin.prototype._destroy = function () {
            var state = this._getState();
            $.each(state.fieldElements, function (fieldIdx, fieldElement) {
                $.each(fieldElement.data(), function (dataIdx, data) {
                    if ($.isFunction(data.destroy)) {
                        data.destroy();
                    }
                });
            });
            this.options.optionPane.dispose(this.options);
            this.element.empty();
        };
        return OptionPanePlugin;
    })(JQueryUICustomWidget);
    VRS.OptionPanePlugin = OptionPanePlugin;
    $.widget('vrs.vrsOptionPane', new OptionPanePlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionPane.js.map