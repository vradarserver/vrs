var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var VRS;
(function (VRS) {
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getSelectDialogPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsSelectDialog');
    };
    VRS.jQueryUIHelper.getSelectDialogOptions = function (overrides) {
        return $.extend({
            items: null,
            value: null,
            autoOpen: true,
            onSelect: $.noop,
            titleKey: null,
            onClose: $.noop,
            modal: true,
            lines: 20
        }, overrides);
    };
    var SelectDialog = (function (_super) {
        __extends(SelectDialog, _super);
        function SelectDialog() {
            var _this = _super.call(this) || this;
            _this.options = VRS.jQueryUIHelper.getSelectDialogOptions();
            return _this;
        }
        SelectDialog.prototype._create = function () {
            var options = this.options;
            if (!options.items)
                throw 'You must supply a list of items or a method that returns a list of items';
            var container = $('<div/>')
                .addClass('vrsSelectDialog')
                .appendTo(this.element), select = $('<select/>')
                .attr('size', options.lines)
                .appendTo(container);
            var items = options.items;
            if (!(items instanceof Array)) {
                items = items();
            }
            var length = items.length;
            for (var i = 0; i < length; ++i) {
                var item = items[i];
                var option = $('<option/>')
                    .val(item.getValue())
                    .text(item.getText())
                    .appendTo(select);
                if (item.getSelected())
                    option.prop('selected', item.getSelected());
            }
            if (options.value) {
                var value = options.value;
                if (value instanceof Function)
                    value = value();
                select.val(value);
            }
            select.change(function () {
                options.onSelect(select.val());
            });
            var title = options.titleKey ? VRS.globalisation.getText(options.titleKey) : null;
            var dialogSettings = {
                autoOpen: options.autoOpen,
                title: title,
                minWidth: options.minWidth,
                minHeight: options.minHeight,
                close: options.onClose,
                modal: options.modal
            };
            this.element.dialog(dialogSettings);
        };
        return SelectDialog;
    }(JQueryUICustomWidget));
    VRS.SelectDialog = SelectDialog;
    $.widget('vrs.vrsSelectDialog', new SelectDialog());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.selectDialog.js.map