/**
 * @license Copyright © 2013 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview A jQuery UI widget that displays a select list within a dialog.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsSelectDialog widget attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsSelectDialog}
     */
    VRS.jQueryUIHelper.getSelectDialogPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsSelectDialog'); };

    /**
     * Returns the default options for a select dialog widget, with optional overrides.
     * @param {VRS_OPTIONS_SELECTDIALOG=} overrides
     * @returns {VRS_OPTIONS_SELECTDIALOG}
     */
    VRS.jQueryUIHelper.getSelectDialogOptions = function(overrides)
    {
        return $.extend({
            items:      null,       // Either an array or a method that gets called to return a list of items to display.
            value:      null,       // The current value (can also be specified by setting 'selected: true' against the current item).
            autoOpen:   true,       // True if the dialog should be automatically opened on creation, false if it should not.
            onSelect:   $.noop,     // Called when the user selects a value.
            titleKey:   null,       // The key of the title for the dialog box.
            minWidth:   undefined,  // The minimum width of the dialog box.
            minHeight:  undefined,  // The minimum height of the dialog box.
            onClose:    $.noop,     // Called when the dialog box is closed.
            modal:      true,       // True if the dialog box is modal, false otherwise.
            lines:      20,         // The number of lines of options to show to the user.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsSelectDialog
    /**
     * @namespace VRS.vrsSelectDialog
     */
    $.widget('vrs.vrsSelectDialog', {
        //region -- options
        /** @type {VRS_OPTIONS_SELECTDIALOG} */
        options: VRS.jQueryUIHelper.getSelectDialogOptions(),
        //endregion

        //region -- _create
        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
        {
            var options = this.options;
            if(!options.items) throw 'You must supply a list of items or a method that returns a list of items';

            var container =
                $('<div/>')
                    .addClass('vrsSelectDialog')
                    .appendTo(this.element),
            select =
                $('<select/>')
                    .attr('size', options.lines)
                    .appendTo(container);

            var items = options.items;
            if(!(items instanceof Array)) items = items();
            var length = items.length;
            for(var i = 0;i < length;++i) {
                var item = items[i];
                var option =
                    $('<option/>')
                        .val(item.getValue())
                        .text(item.getText())
                        .appendTo(select);
                if(item.selected) option.prop('selected', item.selected);
            }
            if(options.value) {
                var value = options.value;
                if(value instanceof Function) value = value();
                select.val(value);
            }

            select.change(function() {
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
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
