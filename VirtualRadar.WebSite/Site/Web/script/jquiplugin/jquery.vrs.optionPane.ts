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
 * @fileoverview A jQuery UI widget that displays a group of option fields within a panel.
 */

namespace VRS
{
    /**
     * Holds the state for the option pane plugin.
     */
    class OptionPanePlugin_State
    {
        /**
         * An array of arbitrary jQueryUI elements for each field rendered into the pane.
         */
        fieldElements: JQuery[] = [];
    }

    /**
     * The options that an OptionPanePlugin can take.
     */
    export interface OptionPanePlugin_Options
    {
        /**
         * The VRS.OptionPane object that describes the pane to create
         */
        optionPane: OptionPane;

        /**
         * The VRS.OptionPageParent object that exposes events across the entire option form.
         */
        optionPageParent: OptionPageParent;

        /**
         *  True if the panes are being displayed in a stack
         */
        isInStack?: boolean;
    }

    /*
     * jQueryUIHelper methods
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getOptionPanePlugin = function(jQueryElement: JQuery) : OptionPanePlugin
    {
        return jQueryElement.data('vrsVrsOptionPane');
    }
    VRS.jQueryUIHelper.getOptionPaneOptions = function(overrides?: OptionPanePlugin_Options) : OptionPanePlugin_Options
    {
        return $.extend({
            optionPane: null,
            optionPageParent: null,
            isInStack: false
        }, overrides);
    }

    /**
     * A widget that can present the fields within an OptionPane.
     */
    export class OptionPanePlugin extends JQueryUICustomWidget
    {
        options: OptionPanePlugin_Options;

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getOptionPaneOptions();
        }

        private _getState() : OptionPanePlugin_State
        {
            var result = this.element.data('vrsOptionPaneState');
            if(result === undefined) {
                result = new OptionPanePlugin_State();
                this.element.data('vrsOptionPaneState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();
            var options = this.options;

            var pane = options.optionPane;
            if(!pane) throw 'You must supply a VRS.OptionPane object';

            if(pane.getFieldCount() > 0) {
                var paneContainer = this.element
                        .addClass('vrsOptionPane');
                if(options.isInStack) paneContainer.addClass('stacked');

                var titleKey = pane.getTitleKey();
                if(titleKey) {
                    $('<h2/>')
                        .text(VRS.globalisation.getText(titleKey))
                        .appendTo(paneContainer);
                }

                var fieldList = $('<ol/>')
                    .appendTo(paneContainer);

                var fieldContainer: JQuery = null;
                pane.foreachField(function(field) {
                    var keepTogether = field.getKeepWithNext();
                    var firstKeepTogether = keepTogether && !fieldContainer;
                    var nextKeepTogether = fieldContainer !== null;

                    if(!fieldContainer) fieldContainer = $('<li/>').appendTo(fieldList);
                    if(firstKeepTogether) fieldContainer.addClass('multiField');

                    var fieldParent = fieldContainer;
                    if(nextKeepTogether) {
                        fieldParent = $('<span/>')
                            .addClass('keepWithPrevious')
                            .appendTo(fieldContainer);
                    }

                    var fieldJQ = VRS.optionControlTypeBroker.createControlTypeHandler({
                        field: field,
                        fieldParentJQ: fieldParent,
                        optionPageParent: options.optionPageParent
                    });
                    state.fieldElements.push(fieldJQ);

                    if(!keepTogether) fieldContainer = null;
                });

                pane.pageParentCreated(options.optionPageParent);
            }
        }

        /**
         * Releases all of the resources held by the object.
         */
        _destroy()
        {
            var state = this._getState();

            // As-at the time of writing there isn't an easy way to call the destroy method on an arbitrary jQuery UI
            // widget (or at least, not one that I know of) - this just looks for objects attached via data() and if
            // they have a destroy method on them it gets called.
            $.each(state.fieldElements, function(fieldIdx, fieldElement) {
                $.each(fieldElement.data(), function(dataIdx, data) {
                    if($.isFunction(data.destroy)) {
                        data.destroy();
                    }
                });
            });

            this.options.optionPane.dispose(this.options);
            this.element.empty();
        }
    }

    $.widget('vrs.vrsOptionPane', new OptionPanePlugin());
}

declare interface JQuery
{
    vrsOptionPane();
    vrsOptionPane(options: VRS.OptionPanePlugin_Options);
    vrsOptionPane(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
