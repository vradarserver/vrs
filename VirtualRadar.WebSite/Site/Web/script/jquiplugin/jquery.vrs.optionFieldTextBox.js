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
 * @fileoverview A jQuery UI plugin that lets users edit a string value.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldTextBox attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldTextBox}
     */
    VRS.jQueryUIHelper.getOptionFieldTextBoxPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldTextBox'); };

    /**
     * Returns the default options for an option field textbox widget with default overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDTEXTBOX=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDTEXTBOX}
     */
    VRS.jQueryUIHelper.getOptionFieldTextBoxOptions = function(overrides)
    {
        return $.extend({
            field: undefined,
            optionPageParent: null,

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionFieldTextBox
    /**
     * @namespace VRS.vrsOptionFieldTextBox
     */
    $.widget('vrs.vrsOptionFieldTextBox', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDTEXTBOX} */
        options: VRS.jQueryUIHelper.getOptionFieldTextBoxOptions(),
        //endregion

        //region -- _create
        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
        {
            var options = this.options;
            var field = options.field;
            var isUpperCase = field.getUpperCase();
            var isLowerCase = !isUpperCase && field.getLowerCase();
            var value = field.getValue();

            var input =
                this.element
                    .uniqueId()
                    .change(function() {
                        if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
                        var value = input.val();
                        if(value && isUpperCase) value = value.toUpperCase();
                        if(value && isLowerCase) value = value.toLowerCase();
                        field.setValue(value);
                        field.saveState();
                        options.optionPageParent.raiseFieldChanged();
                    });
            field.applyInputClass(input);
            if(isUpperCase) input.addClass('upperCase');
            else if(isLowerCase) input.addClass('lowerCase');
            if(field.getMaxLength()) input.attr('maxlength', field.getMaxLength());
            input.val(value);

            if(field.getLabelKey()) {
                $('<label/>')
                    .text(field.getLabelText() + ':')
                    .attr('for', input.attr('id'))
                    .insertBefore(this.element);
            }
        },

        /**
         * Releases the resources allocated to the widget.
         * @private
         */
        _destroy: function()
        {
            this.element.off();
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.textBox,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldTextBox(VRS.jQueryUIHelper.getOptionFieldTextBoxOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));