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

(function(VRS, $, /** object= */ undefined)
{
    //region OptionFieldCheckBoxState
    /**
     * The state object held against check box configuration fields.
     * @constructor
     */
    VRS.OptionFieldCheckBoxState = function()
    {
        /**
         * True if the backing field should not be changed when the checkbox changes.
         * @type {boolean}
         */
        this.suppressFieldSet = false;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldCheckBox attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldCheckBox}
     */
    VRS.jQueryUIHelper.getOptionFieldCheckBoxPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldCheckBox'); };

    /**
     * Returns the default options for an option field checkbox with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDCHECKBOX=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDCHECKBOX}
     */
    VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions = function(overrides)
    {
        return $.extend({
            field: undefined,
            optionPageParent: null,

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionFieldCheckBox
    /**
     * @namespace VRS.vrsOptionFieldCheckBox
     */
    $.widget('vrs.vrsOptionFieldCheckBox', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDCHECKBOX} */
        options: VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state attached to the plugin, creating it if it's not already there.
         * @returns {VRS.OptionFieldCheckBoxState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('optionFieldCheckBoxState');
            if(result === undefined) {
                result = new VRS.OptionFieldCheckBoxState();
                this.element.data('optionFieldCheckBoxState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the plugin.
         * @private
         */
        _create: function()
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
        },

        /**
         * Destroys any resources held by the plugin.
         * @private
         */
        _destroy: function()
        {
            var field = this.options.field;
            field.unhookEvents();
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.checkBox,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldCheckBox(VRS.jQueryUIHelper.getOptionFieldCheckBoxOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));