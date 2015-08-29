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
 * @fileoverview A jQuery UI plugin that lets users choose a single value from a mutually exclusive set of values via a group of radio buttons.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldRadioButton attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldRadioButton}
     */
    VRS.jQueryUIHelper.getOptionFieldRadioButtonPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldRadioButton'); };

    /**
     * Returns the default options for the option field radio button widget, with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDRADIOBUTTON=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDRADIOBUTTON}
     */
    VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions = function(overrides)
    {
        return $.extend({
            field:  undefined,      // The VRS.OptionField that describes the field being edited.
            optionPageParent: null, // The object carrying events across the entire options form.

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionFieldRadioButton
    /**
     * @namespace VRS.vrsOptionFieldRadioButton
     */
    $.widget('vrs.vrsOptionFieldRadioButton', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDRADIOBUTTON} */
        options: VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions(),
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
            var values = field.getValues();

            var container = this.element
                .uniqueId()
                .addClass('vrsOptionRadioButton');

            if(field.getLabelKey()) {
                $('<span/>')
                    .addClass('asLabel')
                    .text(VRS.globalisation.getText(field.getLabelKey()) + ':')
                    .appendTo(container);
            }

            var currentValue = field.getValue();
            var name = field.getName() + container.attr('id');
            var first = true;
            $.each(values, function(idx, value) {
                var wrapper = $('<div/>')
                    .appendTo(container);
                var input = $('<input/>')
                    .uniqueId()
                    .attr('type', 'radio')
                    .attr('value', value.getValue())
                    .attr('name', name)
                    .click(function() {
                        if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
                        field.setValue(value.getValue());
                        field.saveState();
                        options.optionPageParent.raiseFieldChanged();
                    })
                    .appendTo(wrapper);
                var label = $('<label/>')
                    .attr('for', input.attr('id'))
                    .text(value.getText())
                    .appendTo(wrapper);

                if(value.getValue() === currentValue) input.prop('checked', true);
                first = false;
            });
        },

        /**
         * Releases resources allocated to the widget.
         * @private
         */
        _destroy: function()
        {
            $.each(this.element.children('input'), function(/** Number */ idx, /** Object */ input) {
                $(input).off();
            });
            this.element.empty();
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.radioButton,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<div/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldRadioButton(VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));