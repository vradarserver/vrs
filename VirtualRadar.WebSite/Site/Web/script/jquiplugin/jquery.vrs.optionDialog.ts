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

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.optionsDialogWidth = VRS.globalOptions.optionsDialogWidth !== undefined ? VRS.globalOptions.optionsDialogWidth : 600;         // The initial width of the options dialog
    VRS.globalOptions.optionsDialogHeight = VRS.globalOptions.optionsDialogHeight !== undefined ? VRS.globalOptions.optionsDialogHeight : 'auto';   // The initial height of the options dialog
    VRS.globalOptions.optionsDialogModal = VRS.globalOptions.optionsDialogModal !== undefined ? VRS.globalOptions.optionsDialogModal : true;        // True if the options dialog is modal, false otherwise
    VRS.globalOptions.optionsDialogPosition = VRS.globalOptions.optionsDialogPosition || { my: 'center top', at: 'center top', of: window };                 // The initial position of the options dialog.

    /**
     * The options taken by an OptionDialog.
     */
    export interface OptionDialog_Options
    {
        /**
         * An array of VRS.OptionPage objects that describe the option pages to display.
         */
        pages: OptionPage[];

        /**
         * Automatically remove the dialog once it has been closed.
         */
        autoRemove?: boolean;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionDialogPlugin = function(jQueryElement: JQuery) : OptionDialog
    {
        return jQueryElement.data('vrsVrsOptionDialog');
    }
    VRS.jQueryUIHelper.getOptionDialogOptions = function(overrides?: OptionDialog_Options) : OptionDialog_Options
    {
        return $.extend({
            pages: [],
            autoRemove: false
        }, overrides);
    }

    /**
     * Shows a dialog box that hosts an OptionsForm.
     */
    export class OptionDialog extends JQueryUICustomWidget
    {
        options: OptionDialog_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionDialogOptions();
        }

        _create()
        {
            var options = this.options;

            var optionsFormJQ = $('<div/>')
                .appendTo(this.element)
                .vrsOptionForm(VRS.jQueryUIHelper.getOptionFormOptions({
                    pages: options.pages
                }));
            var optionsFormPlugin = VRS.jQueryUIHelper.getOptionFormPlugin(optionsFormJQ);

            this.element.dialog(<JQueryUI.DialogOptions> {
                title: VRS.$$.Options,
                width: VRS.globalOptions.optionsDialogWidth,
                height: VRS.globalOptions.optionsDialogHeight,
                modal: VRS.globalOptions.optionsDialogModal,
                position: VRS.globalOptions.optionsDialogPosition,
                autoOpen: true,
                close: () => {
                    if(VRS.timeoutManager) {
                        VRS.timeoutManager.resetTimer();
                    }
                    if(options.autoRemove) {
                        optionsFormPlugin.destroy();
                        this.element.remove();
                    }
                }
            });
        }
    }

    $.widget('vrs.vrsOptionDialog', new OptionDialog());
}

declare interface JQuery
{
    vrsOptionDialog();
    vrsOptionDialog(options: VRS.OptionDialog_Options);
    vrsOptionDialog(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
