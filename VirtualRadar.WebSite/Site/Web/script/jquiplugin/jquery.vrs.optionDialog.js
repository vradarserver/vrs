var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.optionsDialogWidth = VRS.globalOptions.optionsDialogWidth !== undefined ? VRS.globalOptions.optionsDialogWidth : 600;
    VRS.globalOptions.optionsDialogHeight = VRS.globalOptions.optionsDialogHeight !== undefined ? VRS.globalOptions.optionsDialogHeight : 'auto';
    VRS.globalOptions.optionsDialogModal = VRS.globalOptions.optionsDialogModal !== undefined ? VRS.globalOptions.optionsDialogModal : true;
    VRS.globalOptions.optionsDialogPosition = VRS.globalOptions.optionsDialogPosition || { my: 'center top', at: 'center top', of: window };
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionDialogPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsOptionDialog');
    };
    VRS.jQueryUIHelper.getOptionDialogOptions = function (overrides) {
        return $.extend({
            pages: [],
            autoRemove: false
        }, overrides);
    };
    var OptionDialog = (function (_super) {
        __extends(OptionDialog, _super);
        function OptionDialog() {
            _super.call(this);
            this.options = VRS.jQueryUIHelper.getOptionDialogOptions();
        }
        OptionDialog.prototype._create = function () {
            var _this = this;
            var options = this.options;
            var optionsFormJQ = $('<div/>')
                .appendTo(this.element)
                .vrsOptionForm(VRS.jQueryUIHelper.getOptionFormOptions({
                pages: options.pages
            }));
            var optionsFormPlugin = VRS.jQueryUIHelper.getOptionFormPlugin(optionsFormJQ);
            this.element.dialog({
                title: VRS.$$.Options,
                width: VRS.globalOptions.optionsDialogWidth,
                height: VRS.globalOptions.optionsDialogHeight,
                modal: VRS.globalOptions.optionsDialogModal,
                position: VRS.globalOptions.optionsDialogPosition,
                autoOpen: true,
                close: function () {
                    if (VRS.timeoutManager) {
                        VRS.timeoutManager.resetTimer();
                    }
                    if (options.autoRemove) {
                        optionsFormPlugin.destroy();
                        _this.element.remove();
                    }
                }
            });
        };
        return OptionDialog;
    }(JQueryUICustomWidget));
    VRS.OptionDialog = OptionDialog;
    $.widget('vrs.vrsOptionDialog', new OptionDialog());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.optionDialog.js.map