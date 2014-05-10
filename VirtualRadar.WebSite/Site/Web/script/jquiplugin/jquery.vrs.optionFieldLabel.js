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

(function(VRS, $, /** object= */ undefined)
{
    //region OptionFieldLabelState
    VRS.OptionFieldLabelState = function()
    {
        /**
         * The hook result from the refresh field content event.
         * @type {Object}
         */
        this.refreshFieldContentHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldLabel attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldLabel}
     */
    VRS.jQueryUIHelper.getOptionFieldLabelPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldLabel'); };

    /**
     * Returns the default options for an option field label widget, with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDLABEL=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDLABEL}
     */
    VRS.jQueryUIHelper.getOptionFieldLabelOptions = function(overrides)
    {
        return $.extend({
            field: undefined,
            optionPageParent: null,

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionFieldLabel
    /**
     * @namespace VRS.vrsOptionFieldLabel
     */
    $.widget('vrs.vrsOptionFieldLabel', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDLABEL} */
        options: VRS.jQueryUIHelper.getOptionFieldLabelOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object for the widget, creating it if it's not already there.
         * @returns {VRS.OptionFieldLabelState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('optionFieldLabelState');
            if(result === undefined) {
                result = new VRS.OptionFieldLabelState();
                this.element.data('optionFieldLabelState', result);
            }

            return result;
        },

        /**
         * Creates the UI for the widget.
         * @private
         */
        _create: function()
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
        },

        /**
         * Releases all of the resources allocated to the widget.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();
            var field = this.options.field;
            if(state.refreshFieldContentHookResult) field.unhook(state.refreshFieldContentHookResult);
            state.refreshFieldContentHookResult = null;
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.label,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<span/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldLabel(VRS.jQueryUIHelper.getOptionFieldLabelOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));