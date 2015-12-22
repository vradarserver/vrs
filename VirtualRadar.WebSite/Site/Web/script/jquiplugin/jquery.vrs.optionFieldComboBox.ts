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
 * @fileoverview A jQuery UI plugin that wraps combo box controls in the configuration screens.
 */

namespace VRS
{
    /**
     * The options supported by the OptionFieldComboBoxPlugin plugin.
     */
    export interface OptionFieldComboBoxPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldComboBox;
    }

    /**
     * The state held by an OptionFieldComboBoxPlugin.
     */
    class OptionFieldComboBoxPlugin_State
    {
        labelElement: JQuery = null;
        refreshFieldVisibilityHookResult: IEventHandle = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldComboBoxPlugin = function(jQueryElement: JQuery) : OptionFieldComboBoxPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldComboBox');
    }
    VRS.jQueryUIHelper.getOptionFieldComboBoxOptions = function(overrides?: OptionFieldComboBoxPlugin_Options) : OptionFieldComboBoxPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A jQuery UI widget that can be used to edit a configuration field via a combo box control.
     */
    export class OptionFieldComboBoxPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldComboBoxPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldComboBoxOptions();
        }

        private _getState() : OptionFieldComboBoxPlugin_State
        {
            var result = this.element.data('optionFieldComboBoxState');
            if(result === undefined) {
                result = new OptionFieldComboBoxPlugin_State();
                this.element.data('optionFieldComboBoxState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;
            var field = options.field;

            var comboBox = this.element
                .uniqueId();
            field.applyInputClass(comboBox);

            if(field.getLabelKey()) {
                state.labelElement = $('<label/>')
                    .text(field.getLabelText() + ':')
                    .attr('for', comboBox.attr('id'))
                    .insertBefore(this.element);
            }

            var selectedValue = field.getValue();
            var dropDownListValues = field.getValues();
            var selectedValueExists = false;
            var firstValue = undefined;
            for(var i = 0;i < dropDownListValues.length;++i) {
                var valueText = dropDownListValues[i];
                var value = valueText.getValue();

                if(i === 0) firstValue = value;
                if(value == selectedValue) {
                    selectedValueExists = true;  // Allow type conversion in comparison here, filters were being saved as strings but the enums are numbers...
                }

                var option =
                    $('<option/>')
                        .attr('value', value)
                        .text(valueText.getText())
                        .appendTo(comboBox);
            }
            if(!selectedValueExists) {
                selectedValue = firstValue;
                field.setValue(selectedValue);
            }
            comboBox.val(selectedValue);
            field.callChangedCallback(selectedValue);

            this._setVisibility(state, true);
            state.refreshFieldVisibilityHookResult = field.hookRefreshFieldVisibility(this._fieldRefreshVisibility, this);

            comboBox.change(function() {
                if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();

                var newValue = comboBox.val();
                field.setValue(newValue);
                field.saveState();
                field.callChangedCallback(newValue);
                options.optionPageParent.raiseFieldChanged();
            });
        }

        _destroy()
        {
            var state = this._getState();
            var options = this.options;
            var field = options.field;

            if(state.refreshFieldVisibilityHookResult) {
                field.unhook(state.refreshFieldVisibilityHookResult);
                state.refreshFieldVisibilityHookResult = null;
            }

            if(state.labelElement) {
                state.labelElement.remove();
                state.labelElement = null;
            }

            this.element.off();
        }

        /**
         * Sets the visibility of the element.
         */
        private _setVisibility(state: OptionFieldComboBoxPlugin_State, assumeVisible?: boolean)
        {
            if(!state) state = this._getState();
            var options = this.options;
            var field = options.field;
            var visible = field.getVisible();

            if(assumeVisible || this.element.is(':visible')) {
                if(!visible) {
                    this.element.hide();
                    if(state.labelElement) {
                        state.labelElement.hide();
                    }
                }
            } else {
                if(visible) {
                    this.element.show();
                    if(state.labelElement) {
                        state.labelElement.show();
                    }
                }
            }
        }

        /**
         * Called when the field needs to refresh its visibility.
         */
        private _fieldRefreshVisibility()
        {
            this._setVisibility(null);
        }
    }

    $.widget('vrs.vrsOptionFieldComboBox', new OptionFieldComboBoxPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.comboBox,
            function(settings: OptionFieldComboBoxPlugin_Options) {
                return $('<select/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldComboBox(VRS.jQueryUIHelper.getOptionFieldComboBoxOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldComboBox();
    vrsOptionFieldComboBox(options: VRS.OptionFieldComboBoxPlugin_Options);
    vrsOptionFieldComboBox(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
