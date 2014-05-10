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
 * @fileoverview A jQueryUI widget that handles the data entry for colour fields.
 */

(function(VRS, $, /** Object= */ undefined)
{
    //region OptionFieldColourState
    /**
     * The state for the option field colour widget.
     * @constructor
     */
    VRS.OptionFieldColourState = function()
    {
        /**
         * The colour picker.
         * @type {colourPicker}
         */
        this.colourPicker = null;
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the vrsOptionFieldColour widget attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionFieldColour}
     */
    VRS.jQueryUIHelper.getOptionFieldColourPlugin = function(jQueryElement)
    {
        return jQueryElement.data('vrsVrsOptionFieldColour');
    };

    /**
     * Returns the default options for an option field colour control with optional overrides.
     * @param {VRS_OPTIONS_OPTIONFIELDCOLOUR=} overrides
     * @returns {VRS_OPTIONS_OPTIONFIELDCOLOUR}
     */
    VRS.jQueryUIHelper.getOptionFieldColourOptions = function(overrides)
    {
        return $.extend({
            field: undefined,
            optionPageParent: null,

            __nop: null
        }, overrides);
    };
    //endregion

    //region VRS.vrsOptionFieldColour
    /**
     *
     * @namespace VRS.vrsOptionFieldColour
     */
    $.widget('vrs.vrsOptionFieldColour', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONFIELDCOLOUR} */
        options: VRS.jQueryUIHelper.getOptionFieldColourOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state object for the widget, creating it if it's not already there.
         * @returns {VRS.OptionFieldColourState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('optionColourWidgetState');
            if(result === undefined) {
                result = new VRS.OptionFieldColourState();
                this.element.data('optionColourWidgetState', result);
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
            var options = this.options;
            var field = options.field;

            var self = this;
            this.element
                .uniqueId()
                .change(function() {
                    if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
                    var value = self.element.val();
                    field.setValue(value);
                    field.saveState();
                    options.optionPageParent.raiseFieldChanged();
                });
            field.applyInputClass(this.element);

            this.element.val(field.getValue());

            state.colourPicker = this.element.colourPicker({
                speed: 0,
                title: null,
                colours: this._generateColours()
            });

            if(field.getLabelKey()) {
                $('<label/>')
                    .text(field.getLabelText() + ':')
                    .attr('for', this.element.attr('id'))
                    .insertBefore(this.element);
            }
        },

        /**
         * Destroys the UI created by the plugin and releases all resources held.
         * @private
         */
        _destroy: function()
        {
            this.element.off();
        },
        //endregion

        //region -- _generateColours
        /**
         * Returns an array of CSS colours
         * @returns {Array.<string>}
         * @private
         */
        _generateColours: function()
        {
            var result = [
                '000000', '202020', '404040', '606060', '808080', 'a0a0a0', 'c0c0c0', 'e0e0e0', 'ffffff'
            ];

            var stepDown = function(doStep, value, step) { return !doStep ? value : Math.max(0, Math.floor(value - step)); };
            var stepUp = function(doStep, value, step) { return !doStep ? value : Math.min(255, Math.ceil(value + step)); };
            var toHex = function(value) { var hex = value.toString(16); return hex.length === 1 ? '0' + hex : hex; };
            var addColour = function(red, green, blue) { result.push(toHex(red) + toHex(green) + toHex(blue)); };
            var addColourWheel = function(iRed, iGreen, iBlue, subRed, subGreen, subBlue, addRed, addGreen, addBlue) {
                var i;
                var steps = 9;
                var step = 256 / 9;
                for(i = 0;i < steps;++i) {
                    iRed =   stepDown(subRed, iRed, step);
                    iGreen = stepDown(subGreen, iGreen, step);
                    iBlue =  stepDown(subBlue, iBlue, step);
                    addColour(iRed, iGreen, iBlue);
                }

                for(i = 0;i < steps;++i) {
                    iRed =   stepUp(addRed, iRed, step);
                    iGreen = stepUp(addGreen, iGreen, step);
                    iBlue =  stepUp(addBlue, iBlue, step);
                    addColour(iRed, iGreen, iBlue);
                }
            };

            addColourWheel(0, 255, 255, false, false, true, true, false, false);
            addColourWheel(255, 255, 0, false, true, false, false, false, true);
            addColourWheel(255, 0, 255, true, false, false, false, true, false);

            return result;
        },
        //endregion

        __nop: null
    });
    //endregion

    //region Register control type
    if(VRS.optionControlTypeBroker) VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
        VRS.optionControlTypes.colour,
        function(/** VRS_OPTION_FIELD_SETTINGS */ settings) {
            return $('<input/>')
                .appendTo(settings.fieldParentJQ)
                .vrsOptionFieldColour(VRS.jQueryUIHelper.getOptionFieldColourOptions(settings));
        }
    );
    //endregion
})(window.VRS = window.VRS || {}, jQuery);
