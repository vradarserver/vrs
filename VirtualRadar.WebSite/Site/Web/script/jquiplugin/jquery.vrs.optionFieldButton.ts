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
 * @fileoverview A jQuery UI plugin that wraps button controls in the configuration screens.
 */

namespace VRS
{
    /**
     * The options supported by the OptionFieldButtonPlugin.
     */
    export interface OptionFieldButtonPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldButton;
    }

    /**
     * The state for option plugin buttons.
     */
    export class OptionFieldButtonPlugin_State
    {
        /**
         * The hook result from the field's refresh content event.
         */
        refreshFieldContentHookResult: IEventHandle = null;

        /**
         * The hook result from the field's refresh state event.
         */
        refreshFieldStateHookResult: IEventHandle = null;
    }

    /*
     * jQUeryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldButtonPlugin = function(jQueryElement: JQuery) : OptionFieldButtonPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldButton');
    }
    VRS.jQueryUIHelper.getOptionFieldButtonOptions = function(overrides?: OptionFieldButtonPlugin_Options) : OptionFieldButtonPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that can present an OptionField as a button.
     */
    export class OptionFieldButtonPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldButtonPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldButtonOptions();
        }

        private _getState() : OptionFieldButtonPlugin_State
        {
            var result = this.element.data('optionFieldButtonState');
            if(result === undefined) {
                result = new OptionFieldButtonPlugin_State();
                this.element.data('optionFieldButtonState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var field = this.options.field;
            var optionPageParent = this.options.optionPageParent;

            var getTextOption = function() { return field.getShowText(); };
            var getIconsOption = function() {
                var result = undefined;
                if(field.getPrimaryIcon()) {
                    if(!field.getSecondaryIcon()) result = { primary: 'ui-icon-' + field.getPrimaryIcon() };
                    else                          result = { primary: 'ui-icon-' + field.getPrimaryIcon(), secondary: 'ui-icon-' + field.getSecondaryIcon() };
                }
                return result;
            };

            var jqSettings: JQueryUI.ButtonOptions = {
                text: getTextOption(),
                icons: getIconsOption()
            };

            state.refreshFieldContentHookResult = field.hookRefreshFieldContent(function() {
                this.element.button('option', 'text', getTextOption());
                var icons = getIconsOption();
                if(icons) this.element.button('option', 'icons', icons);
                this.element.text(field.getLabelText());
            }, this);

            state.refreshFieldStateHookResult = field.hookRefreshFieldState($.proxy(function() {
                if(field.getEnabled())  this.element.button('enable');
                else                    this.element.button('disable');
            }, this), this);

            jqSettings.disabled = !field.getEnabled();
            this.element
                .text(field.getLabelText())
                .button(jqSettings)
                .click(function() {
                    if(VRS.timeoutManager) {
                        VRS.timeoutManager.resetTimer();
                    }
                    field.saveState();
                    optionPageParent.raiseFieldChanged();
                });
            field.applyInputClass(this.element);
        }

        _destroy()
        {
            var state = this._getState();
            var field = this.options.field;

            if(state.refreshFieldContentHookResult) field.unhook(state.refreshFieldContentHookResult);
            if(state.refreshFieldStateHookResult)   field.unhook(state.refreshFieldStateHookResult);
            state.refreshFieldContentHookResult = null;
            state.refreshFieldStateHookResult = null;

            this.element.button('destroy');
            this.element.off();
        }
    }

    $.widget('vrs.vrsOptionFieldButton', new OptionFieldButtonPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.button,
            function(settings: OptionFieldButtonPlugin_Options) {
                return $('<button/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldButton(VRS.jQueryUIHelper.getOptionFieldButtonOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldButton();
    vrsOptionFieldButton(options: VRS.OptionFieldButtonPlugin_Options);
    vrsOptionFieldButton(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
