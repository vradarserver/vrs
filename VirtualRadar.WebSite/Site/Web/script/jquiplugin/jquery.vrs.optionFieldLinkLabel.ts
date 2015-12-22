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
     * The options for an OptionFieldLinkLabelPlugin.
     */
    export interface OptionFieldLinkLabelPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldLinkLabel;
    }

    /**
     * The object that carries state for OptionFieldLinkLabelPlugin.
     */
    class OptionFieldLinkLabelPlugin_State
    {
        /**
         * The hook result for the refresh field content event.
         */
        refreshFieldContentHookResult: IEventHandle = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldLinkLabelPlugin = function(jQueryElement: JQuery) : OptionFieldLinkLabelPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldLinkLabel');
    }
    VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions = function(overrides?: OptionFieldLinkLabelPlugin_Options) : OptionFieldLinkLabelPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that shows the users a link as represented by an OptionFieldLinkLabel.
     */
    export class OptionFieldLinkLabelPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldLinkLabelPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions();
        }

        private _getState() : OptionFieldLinkLabelPlugin_State
        {
            var result = this.element.data('optionFieldLinkLabelState');
            if(result === undefined) {
                result = new OptionFieldLinkLabelPlugin_State();
                this.element.data('optionFieldLinkLabelState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var field = this.options.field;

            this._setProperties();

            state.refreshFieldContentHookResult = field.hookRefreshFieldContent(this._setProperties, this);
        }

        private _setProperties()
        {
            var state = this._getState();
            var field = this.options.field;

            this.element
                .text(field.getLabelText())
                .attr('href', field.getHref())
                .attr('target', field.getTarget());

            this.element.removeClass('short long');
            switch(field.getLabelWidth()) {
                case VRS.LabelWidth.Auto:   break;
                case VRS.LabelWidth.Short:  this.element.addClass('short'); break;
                case VRS.LabelWidth.Long:   this.element.addClass('long'); break;
                default:                    throw 'Unknown label width ' + field.getLabelWidth();
            }
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

    $.widget('vrs.vrsOptionFieldLinkLabel', new OptionFieldLinkLabelPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.linkLabel,
            function(settings: OptionFieldLinkLabelPlugin_Options) {
                return $('<a/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldLinkLabel(VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldLinkLabel();
    vrsOptionFieldLinkLabel(options: VRS.OptionFieldLinkLabelPlugin_Options);
    vrsOptionFieldLinkLabel(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
