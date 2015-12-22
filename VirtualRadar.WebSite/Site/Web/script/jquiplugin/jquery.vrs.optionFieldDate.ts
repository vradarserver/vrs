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
 * @fileoverview A jQuery UI plugin that wraps date controls in the configuration screens.
 */

namespace VRS
{
    /**
     * The options used by an OptionFieldDatePlugin.
     */
    export interface OptionFieldDatePlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldDate;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldDatePlugin = function(jQueryElement: JQuery) : OptionFieldDatePlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldDate');
    }
    VRS.jQueryUIHelper.getOptionFieldDateOptions = function(overrides?: OptionFieldDatePlugin_Options) : OptionFieldDatePlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that supports editing of OptionFieldDate objects.
     */
    export class OptionFieldDatePlugin extends JQueryUICustomWidget
    {
        options: OptionFieldDatePlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldDateOptions();
        }

        _create()
        {
            var options = this.options;
            var field = options.field;

            var input = this.element;
            var dateTimePickerOptions: JQueryUI.DatepickerOptions = {
                defaultDate:    field.getDefaultDate(),
                minDate:        field.getMinDate(),
                maxDate:        field.getMaxDate(),
                onSelect:       function() { input.change(); }
            };
            $.extend(dateTimePickerOptions, VRS.globalisation.getDatePickerOptions());

            input.change(function() {
                if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

                var val = input.datepicker('getDate');
                if(val !== null) {
                    var minDate = field.getMinDate();
                    var maxDate = field.getMaxDate();
                    if(minDate && minDate.getTime() !== 0 && val < minDate) val = minDate;
                    if(maxDate && maxDate.getTime() !== 0 && val > maxDate) val = maxDate;

                    field.setValue(val);
                    field.saveState();
                    options.optionPageParent.raiseFieldChanged();
                }
            });

            var label = null;
            if(field.getLabelKey()) {
                label = $('<label/>')
                    .text(field.getLabelText() + ':')
                    .insertBefore(this.element);
            }

            this.element
                .uniqueId()
                .datepicker(dateTimePickerOptions);
            field.applyInputClass(this.element);

            this.element.val($.datepicker.formatDate(dateTimePickerOptions.dateFormat, field.getValue(), dateTimePickerOptions));

            if(label) {
                label.attr('for', this.element.attr('id'));
            }
        }

        private _destroy()
        {
            var label = $('label[for="' + this.element.attr('id') + '"');
            label.remove();

            this.element.datepicker('destroy');
            this.element.off();
        }
    }

    $.widget('vrs.vrsOptionFieldDate', new OptionFieldDatePlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.date,
            function(settings: OptionFieldDatePlugin_Options) {
                return $('<input/>')
                    .attr('type', 'text')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldDate(VRS.jQueryUIHelper.getOptionFieldDateOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldDate();
    vrsOptionFieldDate(options: VRS.OptionFieldDatePlugin_Options);
    vrsOptionFieldDate(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
