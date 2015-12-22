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

namespace VRS
{
    /**
     * The options for an OptionFieldColourPlugin
     */
    export interface OptionFieldColourPlugin_Options extends OptionControl_BaseOptions
    {
        field: OptionFieldColour;
    }

    /**
     * The state for the option field colour widget.
     */
    class OptionFieldColourPlugin_State
    {
        /**
         * The colour picker.
         */
        colourPicker: ColourPicker = null;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionFieldColourPlugin = function(jQueryElement: JQuery) : OptionFieldColourPlugin
    {
        return jQueryElement.data('vrsVrsOptionFieldColour');
    }
    VRS.jQueryUIHelper.getOptionFieldColourOptions = function(overrides?: OptionFieldColourPlugin_Options) : OptionFieldColourPlugin_Options
    {
        return $.extend({
            optionPageParent: null,
        }, overrides);
    }

    /**
     * A widget that supports editing of OptionFieldColour fields.
     */
    export class OptionFieldColourPlugin extends JQueryUICustomWidget
    {
        options: OptionFieldColourPlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionFieldColourOptions();
        }

        private _getState() : OptionFieldColourPlugin_State
        {
            var result = this.element.data('optionColourWidgetState');
            if(result === undefined) {
                result = new OptionFieldColourPlugin_State();
                this.element.data('optionColourWidgetState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;
            var field = options.field;

            this.element
                .uniqueId()
                .change(() => {
                    if(VRS.timeoutManager) {
                        VRS.timeoutManager.resetTimer();
                    }
                    var value = this.element.val();
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
        }

        _destroy()
        {
            this.element.off();
        }

        /**
         * Returns an array of CSS colours
         */
        private _generateColours() : string[]
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
        }
    }

    $.widget('vrs.vrsOptionFieldColour', new OptionFieldColourPlugin());

    if(VRS.optionControlTypeBroker) {
        VRS.optionControlTypeBroker.addControlTypeHandlerIfNotRegistered(
            VRS.optionControlTypes.colour,
            function(settings) {
                return $('<input/>')
                    .appendTo(settings.fieldParentJQ)
                    .vrsOptionFieldColour(VRS.jQueryUIHelper.getOptionFieldColourOptions(settings));
            }
        );
    }
}

declare interface JQuery
{
    vrsOptionFieldColour();
    vrsOptionFieldColour(options: VRS.OptionFieldColourPlugin_Options);
    vrsOptionFieldColour(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
