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
 * @fileoverview A jQuery UI plugin that wraps label controls in the configuration screens.
 */

namespace VRS
{
    /**
     * The options used by an OptionFieldLabelPlugin
     */
    export interface OptionFieldLabelPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldLabel
    }

    /**
     * The state object held by OptionFieldLabelPlugin.
     */
    class OptionFieldLabelPlugin_State
    {
        /**
         * The hook result from the refresh field content event.
         */
        refreshFieldContentHookResult: IEventHandle = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper  = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldLabelPlugin = function(jQueryElement: JQuery) : OptionFieldLabelPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldLabel');
    }
    VRS.jQueryUIHelper.getOptionFieldLabelOptions = function(overrides?: OptionFieldLabelPlugin_Options) : OptionFieldLabelPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that shows the user the content of an OptionFieldLabel.
     */
    export class OptionFieldLabelPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldLabelPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldLabelOptions();
        }

        private _getState() : OptionFieldLabelPlugin_State
        {
            var result = this.element.data('optionFieldLabelState');
            if(result === undefined) {
                result = new OptionFieldLabelPlugin_State();
                this.element.data('optionFieldLabelState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var field = this.options.field;

            this.element
                .text(field.getLabelText());

            switch(field.getLabelWidth()) {
                case VRS.LabelWidth.Auto:   break;
                case VRS.LabelWidth.Short:  this.element.addClass('short'); break;
                case VRS.LabelWidth.Long:   this.element.addClass('long'); break;
                default:                    throw 'Unknown label width ' + field.getLabelWidth();
            }

            state.refreshFieldContentHookResult = field.hookRefreshFieldContent(function() {
                this.element.text(field.getLabelText());
            }, this);
        }

        _destroy()
        {
            var state = this._getState();
            var field = this.options.field;
            if(state.refreshFieldContentHookResult) {
                field.unhook(state.refreshFieldContentHookResult);
            }
            state.refreshFieldContentHookResult = null;
        }
    }

    $.widget('vrs.vrsOptionFieldLabel', new OptionFieldLabelPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.label,
            function(settings: OptionFieldLabelPlugin_Options) {
                return $('<span/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldLabel(VRS.jQueryUIHelper.getOptionFieldLabelOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldLabel();
    vrsOptionFieldLabel(options: VRS.OptionFieldLabelPlugin_Options);
    vrsOptionFieldLabel(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
