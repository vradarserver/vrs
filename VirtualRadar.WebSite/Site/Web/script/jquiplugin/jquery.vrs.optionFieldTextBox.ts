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

namespace VRS
{
    /**
     * The options for the OptionFieldTextBoxPlugin.
     */
    export interface OptionFieldTextBoxPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldTextBox;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldTextBoxPlugin = function(jQueryElement: JQuery) : OptionFieldTextBoxPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldTextBox');
    }
    VRS.jQueryUIHelper.getOptionFieldTextBoxOptions = function(overrides?: OptionFieldTextBoxPlugin_Options) : OptionFieldTextBoxPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that supports the editing of strings via a text box control.
     */
    export class OptionFieldTextBoxPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldTextBoxPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldTextBoxOptions();
        }

        _create()
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
            if(isUpperCase) {
                input.addClass('upperCase');
            } else if(isLowerCase) {
                input.addClass('lowerCase');
            }
            if(field.getMaxLength()) {
                input.attr('maxlength', field.getMaxLength());
            }
            input.val(value);

            if(field.getLabelKey()) {
                $('<label/>')
                    .text(field.getLabelText() + ':')
                    .attr('for', input.attr('id'))
                    .insertBefore(this.element);
            }
        }

        _destroy()
        {
            this.element.off();
        }
    }

    $.widget('vrs.vrsOptionFieldTextBox', new OptionFieldTextBoxPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.textBox,
            function(settings: OptionFieldTextBoxPlugin_Options) {
                return $('<input/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldTextBox(VRS.jQueryUIHelper.getOptionFieldTextBoxOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldTextBox();
    vrsOptionFieldTextBox(options: VRS.OptionFieldTextBoxPlugin_Options);
    vrsOptionFieldTextBox(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
