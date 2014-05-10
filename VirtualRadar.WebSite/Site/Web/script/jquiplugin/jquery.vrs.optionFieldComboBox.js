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

(function(VRS, $, /** object= */ undefined)
{
    //region OptionFieldComboBoxState
    VRS.OptionFieldComboBoxState = function()
    {
        /** @type {jQuery} */
        this.labelElement = null;

        /** @type {object} */
        this.refreshFieldVisibilityHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldComboBox attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldComboBox}
     */
    VRS.jQueryUIHelper.getOptionFieldComboBoxPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldComboBox'); };

    /**
     * Returns the default options for an option field comboBox with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDCOMBOBOX=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDCOMBOBOX}
     */
    VRS.jQueryUIHelper.getOptionFieldComboBoxOptions = function(overrides)
    {
        return $.extend({
            field: undefined,
            optionPageParent: null,

            __nop: null
        }, overrides);
    }
    //endregion

    //region vrsOptionFieldComboBox
    /**
     * A jQuery UI widget that can be used to edit a configuration field via a combo box control.
     * @namespace VRS.vrsOptionFieldComboBox
     */
    $.widget('vrs.vrsOptionFieldComboBox', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDCOMBOBOX} */
        options: VRS.jQueryUIHelper.getOptionFieldComboBoxOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object for the widget, creating it if it's not already there.
         * @returns {VRS.OptionFieldComboBoxState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('optionFieldComboBoxState');
            if(result === undefined) {
                result = new VRS.OptionFieldComboBoxState();
                this.element.data('optionFieldComboBoxState', result);
            }

            return result;
        },

        /**
         * Creates the widget.
         * @private
         */
        _create: function()
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
                if(value === selectedValue) selectedValueExists = true;

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
        },

        _destroy: function()
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
        },
        //endregion

        //region -- _setVisibility
        /**
         * Sets the visibility of the element.
         * @param {VRS.OptionFieldComboBoxState}   [state]
         * @param {boolean}                        [assumeVisible]
         * @private
         */
        _setVisibility: function(state, assumeVisible)
        {
            if(!state) state = this._getState();
            var options = this.options;
            var field = options.field;
            var visible = field.getVisible();

            if(assumeVisible || this.element.is(':visible')) {
                if(!visible) {
                    this.element.hide();
                    if(state.labelElement) state.labelElement.hide();
                }
            } else {
                if(visible) {
                    this.element.show();
                    if(state.labelElement) state.labelElement.show();
                }
            }
        },
        //endregion

        //region -- event handlers
        /**
         * Called when the field needs to refresh its visibility.
         * @private
         */
        _fieldRefreshVisibility: function()
        {
            this._setVisibility(null);
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.comboBox,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<select/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldComboBox(VRS.jQueryUIHelper.getOptionFieldComboBoxOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));