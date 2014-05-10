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
//noinspection JSUnusedLocalSymbols
/**
 * @fileoverview A jQuery UI widget that displays a group of option fields within a panel.
 */

(function(VRS, $, /** object= */ undefined)
{
    //region OptionPanePluginState
    /**
     * Holds the state for the option pane plugin.
     * @constructor
     */
    VRS.OptionPanePluginState = function()
    {
        /**
         * An array of arbitrary jQueryUI elements for each field rendered into the pane.
         * @type {Array.<jQuery>}
         */
        this.fieldElements = [];
    };
    //endregion

    //region jQueryUIHelper
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    /**
     * Returns the VRS.vrsOptionPane plugin attached to the jQuery element passed across.
     * @param {jQuery} jQueryElement
     * @returns {VRS.vrsOptionPane}
     */
    VRS.jQueryUIHelper.getOptionPanePlugin = function(jQueryElement) { return jQueryElement.data('vrsVrsOptionPane'); };

    /**
     * Returns the default options for the option pane widget, with optional overrides.
     * @param {VRS_OPTIONS_OPTIONPANE=} overrides
     * @returns {VRS_OPTIONS_OPTIONPANE}
     */
    VRS.jQueryUIHelper.getOptionPaneOptions = function(overrides)
    {
        return $.extend({
            optionPane: null,           // The VRS.OptionPane object that describes the pane to create
            optionPageParent: null,     // The VRS.OptionPageParent object that exposes events across the entire option form.
            isInStack: false,           // True if the panes are being displayed in a stack

            __nop: null
        }, overrides);
    }
    //endregion

    //region vrsOptionPane
    /**
     * @namespace VRS.vrsOptionPane
     */
    $.widget('vrs.vrsOptionPane', {
        //region -- options
        /** @type {VRS_OPTIONS_OPTIONPANE} */
        options: VRS.jQueryUIHelper.getOptionPaneOptions(),
        //endregion

        //region -- _getState, _create, _destroy
        /**
         * Returns the state associated with the plugin, creating it if it doesn't already exist.
         * @returns {VRS.OptionPanePluginState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('vrsOptionPaneState');
            if(result === undefined) {
                result = new VRS.OptionPanePluginState();
                this.element.data('vrsOptionPaneState', result);
            }

            return result;
        },

        /**
         * Creates the widget's UI.
         * @private
         */
        _create: function()
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

                var fieldContainer = null;
                pane.foreachField(function(field) {
                    var keepTogether = field.getKeepWithNext();
                    var firstKeepTogether = keepTogether && !fieldContainer;
                    var nextKeepTogether = fieldContainer !== null;

                    if(!fieldContainer) fieldContainer = $('<li/>').appendTo(fieldList);
                    if(firstKeepTogether) fieldContainer.addClass('multiField');

                    /** @type {jQuery} */ var fieldParent = fieldContainer;
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
        },

        /**
         * Releases all of the resources held by the object.
         * @private
         */
        _destroy: function()
        {
            var state = this._getState();

            // As-at the time of writing there isn't an easy way to call the destroy method on an arbitrary jQuery UI
            // widget (or at least, not one that I know of!) - this just looks for objects attached via data() and if
            // they have a destroy method on them it gets called.
            $.each(state.fieldElements, function(/** Number */ fieldIdx, /** jQuery */ fieldElement) {
                $.each(fieldElement.data(), function(/** Number */ dataIdx, /** Object */ data) {
                    if($.isFunction(data.destroy)) data.destroy();
                });
            });

            this.options.optionPane.dispose(this.options);
            this.element.empty();
        },
        //endregion

        __nop: null
    });
})(window.VRS = window.VRS || {}, jQuery);
