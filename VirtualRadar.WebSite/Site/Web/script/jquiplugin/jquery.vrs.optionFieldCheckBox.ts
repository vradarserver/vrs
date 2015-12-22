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
 * @fileoverview A jQuery UI plugin that wraps checkbox controls in the configuration screens.
 */

namespace VRS
{
    /**
     * The options that an OptionFieldCheckBoxPlugin supports.
     */
    export interface OptionFieldCheckBoxPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldCheckBox;
    }

    /**
     * The state held by an OptionFieldCheckBoxPlugin.
     */
    class OptionFieldCheckBoxPlugin_State
    {
        /**
         * True if the backing field should not be changed when the checkbox changes.
         */
        suppressFieldSet = false;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldCheckBoxPlugin = function(jQueryElement: JQuery) : OptionFieldCheckBoxPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldCheckBox');
    }
    VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions = function(overrides?: OptionFieldCheckBoxPlugin_Options) : OptionFieldCheckBoxPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that can display a checkbox.
     */
    export class OptionFieldCheckBoxPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldCheckBoxPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions();
        }

        private _getState() : OptionFieldCheckBoxPlugin_State
        {
            var result = this.element.data('optionFieldCheckBoxState');
            if(result === undefined) {
                result = new OptionFieldCheckBoxPlugin_State();
                this.element.data('optionFieldCheckBoxState', result);
            }

            return result;
        }

        _create()
        {
            var field = this.options.field;
            var state = this._getState();
            var optionPageParent = this.options.optionPageParent;

            var checkbox =
                this.element
                    .uniqueId()
                    .attr('type', 'checkbox')
                    .prop('checked', field.getValue());
            if(field.getLabelKey()) {
                $('<label/>')
                    .attr('for', checkbox.attr('id'))
                    .text(field.getLabelText())
                    .appendTo(this.element.parent().first());
            }

            checkbox.change(function() {
                if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
                if(!state.suppressFieldSet) {
                    field.setValue(checkbox.prop('checked'));
                    field.saveState();
                    optionPageParent.raiseFieldChanged();
                }
            });

            field.hookEvents(function() {
                var suppressFieldSet = state.suppressFieldSet;
                state.suppressFieldSet = true;
                checkbox.prop('checked', field.getValue());
                state.suppressFieldSet = suppressFieldSet;
            }, this);
        }

        _destroy()
        {
            var field = this.options.field;
            field.unhookEvents();
        }
    }

    $.widget('vrs.vrsOptionFieldCheckBox', new OptionFieldCheckBoxPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.checkBox,
            function(settings) {
                return $('<input/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldCheckBox(VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldCheckBox();
    vrsOptionFieldCheckBox(options: VRS.OptionFieldCheckBoxPlugin_Options);
    vrsOptionFieldCheckBox(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
