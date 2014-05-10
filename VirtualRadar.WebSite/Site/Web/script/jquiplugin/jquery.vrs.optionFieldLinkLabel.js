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
    //region OptionFieldLinkState
    /**
     * The object that carries state for link labels.
     * @constructor
     */
    VRS.OptionFieldLinkState = function()
    {
        /**
         * The hook result for the refresh field content event.
         * @type {Object}
         */
        this.refreshFieldContentHookResult = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionFieldLinkLabel attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldLinkLabel}
     */
    VRS.jQueryUIHelper.getOptionFieldLinkLabelPlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionFieldLinkLabel'); };

    /**
     * Returns the default options for an option field link label widget, with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDLINKLABEL=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDLINKLABEL}
     */
    VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions = function(overrides)
    {
        return $.extend({
            /** @type {VRS.OptionFieldLinkLabel} */ field: undefined,
            /** @type {VRS.OptionPageParent} */     optionPageParent: null,

            __nop: null
        }, overrides);
    };
    //endregion

    //region vrsOptionFieldLinkLabel
    /**
     * A widget that can display the field content in a hyperlink.
     * @namespace VRS.vrsOptionFieldLinkLabel
     */
    $.widget('vrs.vrsOptionFieldLinkLabel', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDLINKLABEL} */
        options: VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions(),
        //endregion

        //region -- _getState, _create, destroy
        /**
         * Returns the state object for the widget, creating it if it's not already there.
         * @returns {VRS.OptionFieldLinkState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('optionFieldLinkLabelState');
            if(result === undefined) {
                result = new VRS.OptionFieldLinkState();
                this.element.data('optionFieldLinkLabelState', result);
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

            var self = this;
            var setProperties = function() {
                self.element
                    .text(field.getLabelText())
                    .attr('href', field.getHref())
                    .attr('target', field.getTarget());

                self.element.removeClass('short long');
                switch(field.getLabelWidth()) {
                    case VRS.LabelWidth.Auto:   break;
                    case VRS.LabelWidth.Short:  self.element.addClass('short'); break;
                    case VRS.LabelWidth.Long:   self.element.addClass('long'); break;
                    default:                    throw 'Unknown label width ' + field.getLabelWidth();
                }
            };

            setProperties();

            state.refreshFieldContentHookResult = field.hookRefreshFieldContent(function() {
                setProperties();
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
        VRS.optionControlTypes.linkLabel,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<a/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldLinkLabel(VRS.jQueryUIHelper.getOptionFieldLinkLabelOptions(settings));
        }
    );
    //endregion
}(window.VRS = window.VRS || {}, jQuery));