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
 * @fileoverview A jQuery UI plugin that displays the options UI in a dialog box.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region Global settings
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.optionsDialogWidth = VRS.globalOptions.optionsDialogWidth !== undefined ? VRS.globalOptions.optionsDialogWidth : 600;         // The initial width of the options dialog
    VRS.globalOptions.optionsDialogHeight = VRS.globalOptions.optionsDialogHeight !== undefined ? VRS.globalOptions.optionsDialogHeight : 'auto';   // The initial height of the options dialog
    VRS.globalOptions.optionsDialogModal = VRS.globalOptions.optionsDialogModal !== undefined ? VRS.globalOptions.optionsDialogModal : true;        // True if the options dialog is modal, false otherwise
    VRS.globalOptions.optionsDialogPosition = VRS.globalOptions.optionsDialogPosition || { my: 'center top', at: 'center top', of: window };                 // The initial position of the options dialog.
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionDialog attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionDialog}
     */
    VRS.jQueryUIHelper.getOptionDialogPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionDialog'); };

    /**
     * Returns the default options for the option dialog widget, with optional overrides.
     * @param {VRS_OPTIONS_OPTIONDIALOG=} overrides
     * @returns {VRS_OPTIONS_OPTIONDIALOG}
     */
    VRS.jQueryUIHelper.getOptionDialogOptions = function(overrides)
    {
        return $.extend({
            pages:  [],                 // An array of VRS.OptionPage objects that describe the option pages to display.
            autoRemove: false,          // Automatically remove the dialog once it has been closed.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionDialog jQueryUI widget
    /**
     * @namespace VRS.vrsOptionDialog
     */
    $.widget('vrs.vrsOptionDialog', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONDIALOG} */
        options: VRS.jQueryUIHelper.getOptionDialogOptions(),
        //endregion

        //region -- _create, _destroy
        /**
         * Creates the UI for the option dialog.
         * @private
         */
        _create: function()
        {
            var options = this.options;

            var optionsFormJQ = $('<div/>')
                .appendTo(this.element)
                .vrsOptionForm(VRS.jQueryUIHelper.getOptionFormOptions({
                    pages: options.pages
                }));
            var optionsFormPlugin = VRS.jQueryUIHelper.getOptionFormPlugin(optionsFormJQ);

            var self = this;
            this.element.dialog({
                title: VRS.$$.Options,
                width: VRS.globalOptions.optionsDialogWidth,
                height: VRS.globalOptions.optionsDialogHeight,
                modal: VRS.globalOptions.optionsDialogModal,
                position: VRS.globalOptions.optionsDialogPosition,
                autoOpen: true,
                close: function() {
                    if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
                    if(options.autoRemove) {
                        optionsFormPlugin.destroy();
                        self.element.remove();
                    }
                }
            });
        },
        //endregion

        __nop: null
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
