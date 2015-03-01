/**
 * @license Copyright © 2015 onwards, Andrew Whewell
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
 * @fileoverview A jQuery UI plugin that displays content from a JSON object and lets users modify it.
 */
(function(VRS, $, undefined)
{
    VRS.WebAdmin = VRS.WebAdmin || {};
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    //region FormProcessorState
    VRS.WebAdmin.FormProcessorState = function()
    {
        /**
         * @type {jQuery}
         */
        this.container = undefined;

        /**
         * @type {jQuery[]}
         */
        this.submitButtons = [];

        /**
         * @type {string}
         */
        this.containerId = undefined;

        /**
         * @type {VRS_WEBADMIN_FORM_PAGE_INSTANCE[]}
         */
        this.pageInstances = [];
    };
    //endregion

    /**
     * @param {jQuery} jQueryElement
     * @returns {VRS.formProcessor}
     */
    VRS.jQueryUIHelper.getWebAdminFormProcessorPlugin = function(jQueryElement) { return jQueryElement.data('vrsFormProcessor'); };

    /**
     * @namespace VRS.formProcessor
     */
    $.widget('vrs.formProcessor', {
        options: {
            /** @type {VRS.WebAdmin.Form} */    formSpec: null,
            /** @type {string} */               saveButtonLabel: null
        },

        _getState: function()
        {
            var result = this.element.data('vrs-webadmin-form-processor-state');
            if(result === undefined) {
                result = new VRS.WebAdmin.FormProcessorState();
                this.element.data('vrs-webadmin-form-processor-state', result);
            }

            return result;
        },

        _create: function()
        {
            var options = this.options;
            var state = this._getState();

            var topSubmitButton = this._createSubmitButton('save-top');
            var bottomSubmitButton = this._createSubmitButton('save-bottom');
            state.submitButtons.push(topSubmitButton);
            state.submitButtons.push(bottomSubmitButton);

            this.element.append(topSubmitButton);

            state.container =
                $('<div />')
                    .uniqueId()
                    .addClass('panel-group')
                    .attr('role', 'tablist')
                    .attr('aria-multiselectable', 'true');
            state.containerId = state.container.attr('id');
            this.element.append(state.container);

            $.each(options.formSpec.getPages(), function(/** Number */ idx, /** VRS.WebAdmin.FormPage */ pageSpec) {
                var pageElement = $('<div />');
                pageElement.formPage({
                    pageSpec:   pageSpec,
                    collapsed:  idx !== 0,
                    parentId:   state.containerId
                });
                state.container.append(pageElement);
                state.pageInstances.push({
                    page: pageElement,
                    plugin: VRS.jQueryUIHelper.getWebAdminFormPagePlugin(pageElement)
                });
            });

            this.element.append(bottomSubmitButton);
        },

        _createSubmitButton: function(additionalClasses)
        {
            var result = $('<button />')
                .text(this.options.saveButtonLabel)
                .addClass('btn btn-default btn-lg')
                .attr('type', 'button')
                .prepend($('<span />')
                    .addClass('glyphicon glyphicon-save')
                    .attr('aria-hidden', 'true')
                );
            if(additionalClasses) result.addClass(additionalClasses);

            var that = this;
            result.on('click', function(event) {
                that._trigger('saveclicked', event);
            });

            return result;
        },

        /**
         * Returns an associative array of every field in the form indexed by property name.
         * @returns {Object.<string, VRS_WEBADMIN_FORM_FIELD_INSTANCE>}
         */
        getAllFieldInstances: function()
        {
            var result = {};
            var state = this._getState();

            $.each(state.pageInstances, function(/** Number */ idx, /** VRS_WEBADMIN_FORM_PAGE_INSTANCE */ pageInstance) {
                var fieldInstances = pageInstance.plugin.getAllFieldInstances();

                $.each(fieldInstances, function(/** Number */ innerIdx, /** VRS_WEBADMIN_FORM_FIELD_INSTANCE */ fieldInstance) {
                    result[fieldInstance.propertyName] = fieldInstance;
                });
            });

            return result;
        }
    });
}(window.VRS = window.VRS || {}, jQuery));
