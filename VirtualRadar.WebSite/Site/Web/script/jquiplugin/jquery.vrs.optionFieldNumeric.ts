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

namespace VRS
{
    /**
     * The options accepted by an OptionFieldNumericPlugin.
     */
    export interface OptionFieldNumeric_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldNumeric;
    }

    /**
     * The state object for OptionFieldNumericPlugin.
     */
    class OptionFieldNumericPlugin_State
    {
        /**
         * The optional slider element.
         */
        sliderElement: JQuery = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldNumericPlugin = function(jQueryElement: JQuery) : OptionFieldNumericPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldNumeric');
    }
    VRS.jQueryUIHelper.getOptionFieldNumericOptions = function(overrides?: OptionFieldNumeric_Options) : OptionFieldNumeric_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that supports data entry for OptionFieldNumeric objects.
     */
    export class OptionFieldNumericPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldNumeric_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldNumericOptions();
        }

        private _getState() : OptionFieldNumericPlugin_State
        {
            var result = this.element.data('optionFieldNumericState');
            if(result === undefined) {
                result = new OptionFieldNumericPlugin_State();
                this.element.data('optionFieldNumericState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;
            var field = options.field;
            var value = field.getValue();

            var input = this.element;
            var onChange = () => {
                if(VRS.timeoutManager) {
                    VRS.timeoutManager.resetTimer();
                }

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

            var spinnerOptions: JQueryUI.SpinnerOptions = {
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
            if(field.getDecimals() !== undefined) {
                spinnerOptions.numberFormat = 'n' + field.getDecimals().toString();
            }
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
                        slide: (event, ui) => {
                            this.element.spinner('value', ui.value);
                        }
                    })
                    .insertAfter(this.element);
            }

            this.element
                .uniqueId()
                .val(value)
                .spinner(spinnerOptions);
            field.applyInputClass(this.element);

            if(label) {
                label.attr('for', this.element.attr('id'));
            }
        }

        _destroy()
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
        }
    }

    $.widget('vrs.vrsOptionFieldNumeric', new OptionFieldNumericPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.numeric,
            function(settings: OptionFieldNumeric_Options) {
                return $('<input/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldNumeric(VRS.jQueryUIHelper.getOptionFieldNumericOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldNumeric();
    vrsOptionFieldNumeric(options: VRS.OptionFieldNumeric_Options);
    vrsOptionFieldNumeric(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
