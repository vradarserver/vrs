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
 * @fileoverview A jQuery UI plugin that wraps numeric controls in the configuration screens.
 */

(function(VRS, $, /** option= */undefined)
{
    //region OptionFieldNumericState
    /**
     * The state object held against numeric configuration fields.
     * @constructor
     */
    VRS.OptionFieldNumericState = function()
    {
        /**
         * The optional slider element.
         * @type {jQuery}
         * */
        this.sliderElement = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldNumeric attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldNumeric}
     */
    VRS.jQueryUIHelper.getOptionFieldNumericPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldNumeric'); };

    /**
     * Returns the default options for an option field numeric widget, with optional defaults.
     * @param {VRS_OPTIONS_OPTIONFIELDNUMERIC=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDNUMERIC}
     */
    VRS.jQueryUIHelper.getOptionFieldNumericOptions = function(overrides)
    {
        return $.extend({
            field: undefined,
            optionPageParent: null,

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionFieldNumeric
    /**
     * @namespace VRS.vrsOptionFieldNumeric
     */
    $.widget('vrs.vrsOptionFieldNumeric', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDNUMERIC} */
        options: VRS.jQueryUIHelper.getOptionFieldNumericOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state attached to the plugin, creating it if it's not already there.
         * @returns {VRS.OptionFieldCheckBoxState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('optionFieldNumericState');
            if(result === undefined) {
                result = new VRS.OptionFieldNumericState();
                this.element.data('optionFieldNumericState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
        {
            var that = this;
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var value = field.getValue();

            var input = this.element;
            var onChange = function() {
                if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

                var val = input.spinner('value');
                if(val !== null) {
                    var min = field.getMin();
                    var max = field.getMax();
                    if(min !== undefined && val < min) val = min;
                    if(max !== undefined && val > max) val = max;

                    field.setValue(val);
                    field.saveState();

                    if(state.sliderElement) {
                        state.sliderElement.slider('value', val);
                    }

                    options.optionPageParent.raiseFieldChanged();
                }
            };

            var spinnerOptions = {
                change: onChange,
                stop: onChange
            };
            if(field.getMin() !== undefined) {
                spinnerOptions.min = field.getMin();
                if(value < spinnerOptions.min) value = spinnerOptions.min;
            }
            if(field.getMax() !== undefined) {
                spinnerOptions.max = field.getMax();
                if(value > spinnerOptions.max) value = spinnerOptions.max;
            }
            if(field.getDecimals() !== undefined) spinnerOptions.numberFormat = 'n' + field.getDecimals().toString();
            spinnerOptions.step = field.getStep();

            var label = null;
            if(field.getLabelKey()) {
                label = $('<label/>')
                    .text(field.getLabelText() + ':')
                    .insertBefore(this.element);
            }

            if(field.showSlider()) {
                state.sliderElement = $('<div/>')
                    .uniqueId()
                    .addClass('vrsSlider')
                    .slider({
                        range: "min",
                        value: value,
                        min: field.getMin(),
                        max: field.getMax(),
                        step: field.getSliderStep(),
                        slide: function(event, ui) {
                            that.element.spinner('value', ui.value);
                        }
                    })
                    .insertAfter(this.element);
            }

            this.element
                .uniqueId()
                .val(value)
                .spinner(spinnerOptions);
            field.applyInputClass(this.element);

            if(label) label.attr('for', this.element.attr('id'));
        },

        /**
         * Releases resources allocated to the plugin.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();

            if(state.sliderElement) {
                state.sliderElement.slider('destroy');
                state.sliderElement.off();
                state.sliderElement = null;
            }

            var label = $('label[for="' + this.element.attr('id') + '"]');
            label.remove();

            this.element.spinner('destroy');
            this.element.off();
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.numeric,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldNumeric(VRS.jQueryUIHelper.getOptionFieldNumericOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));