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

namespace VRS
{
    /**
     * The options for the OptionFieldRadioButtonPlugin
     */
    export interface OptionFieldRadioButtonPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldRadioButton;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldRadioButtonPlugin = function(jQueryElement: JQuery) : OptionFieldRadioButtonPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldRadioButton');
    }
    VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions = function(overrides?: OptionFieldRadioButtonPlugin_Options) : OptionFieldRadioButtonPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * Lets the user chose from a set of mutually exclusive values.
     */
    export class OptionFieldRadioButtonPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldRadioButtonPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions();
        }

        _create()
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

                if(value.getValue() === currentValue) {
                    input.prop('checked', true);
                }
                first = false;
            });
        }

        _destroy()
        {
            $.each(this.element.children('input'), function(idx, input) {
                $(input).off();
            });
            this.element.empty();
        }
    }

    $.widget('vrs.vrsOptionFieldRadioButton', new OptionFieldRadioButtonPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.radioButton,
            function(settings: OptionFieldRadioButtonPlugin_Options) {
                return $('<div/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldRadioButton(VRS.jQueryUIHelper.getOptionFieldRadioButtonOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldRadioButton();
    vrsOptionFieldRadioButton(options: VRS.OptionFieldRadioButtonPlugin_Options);
    vrsOptionFieldRadioButton(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
