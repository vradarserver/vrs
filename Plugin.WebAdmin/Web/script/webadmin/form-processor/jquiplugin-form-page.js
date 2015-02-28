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
 * @fileoverview Part of the form processor, handles pages within a form.
 */
(function(VRS, $, undefined)
{
    VRS.WebAdmin = VRS.WebAdmin || {};
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    //region FormPageState
    VRS.WebAdmin.FormPageState = function()
    {
        /**
         * @type {jQuery}
         */
        this.heading = undefined;

        /**
         * @type {jQuery}
         */
        this.bodyOuter = undefined;

        /**
         * @type {jQuery}
         */
        this.body = undefined;

        /**
         * @type {VRS_WEBADMIN_FORM_FIELD_INSTANCE[]}
         */
        this.fieldInstances = [];
    };
    //endregion

    /**
     * @param {jQuery} jQueryElement
     * @returns {VRS.formPage}
     */
    VRS.jQueryUIHelper.getWebAdminFormPagePlugin = function(jQueryElement) { return jQueryElement.data('vrsFormPage'); };

    /**
     * @namespace VRS.formPage
     */
    $.widget('vrs.formPage', {
        options: {
            /** @type {VRS.WebAdmin.FormPage} */    pageSpec: null,
            /** @type {Boolean} */                  collapsed: true,
            /** @type {String} */                   parentId: null
        },

        _getState: function()
        {
            var result = this.element.data('vrs-webadmin-form-page-state');
            if(result === undefined) {
                result = new VRS.WebAdmin.FormPageState();
                this.element.data('vrs-webadmin-form-page-state', result);
            }

            return result;
        },

        _create: function()
        {
            var options = this.options;
            var state = this._getState();
            var spec = options.pageSpec;

            this.element
                .uniqueId()
                .addClass('panel panel-default');
            state.id = this.element.attr('id');

            state.bodyOuter = $('<div />')
                .uniqueId()
                .addClass('panel-collapse collapse')
                .addClass(options.collapsed ? '' : 'in')
                .attr('role', 'tabpanel')
            var bodyOuterId = state.bodyOuter.attr('id');
            state.body = $('<div />')
                .addClass('panel-body')
                .appendTo(state.bodyOuter);
            state.heading = $('<div />')
                .uniqueId()
                .addClass('panel-heading')
                .attr('role', 'tab')
                .append($('<h4 />')
                    .addClass('panel-title')
                    .append($('<a />')
                        .addClass(options.collapsed ? 'collapsed' : '')
                        .attr('data-toggle', 'collapse')
                        .attr('data-parent', '#' + options.parentId)
                        .attr('href', '#' + bodyOuterId)
                        .attr('aria-expanded', options.collapsed ? 'false' : 'true')
                        .attr('aria-controls', bodyOuterId) // no hash
                        .text(spec.getTitle())
                )
            );
            var headingId = state.heading.attr('id');
            state.bodyOuter.attr('aria-labelledby', headingId);

            this.element.append(state.heading);
            this.element.append(state.bodyOuter);

            $.each(spec.getFields(), function(/** Number */ idx, /** VRS.WebAdmin.FormField */ fieldSpec) {
                var fieldElement = fieldSpec.createFieldContainer();
                var propertyName = fieldSpec.getPropertyName();
                var fieldPlugin = null;
                switch(fieldSpec.getType()) {
                    case 'string':
                        fieldElement.formFieldString({ fieldSpec: fieldSpec });
                        fieldPlugin = VRS.jQueryUIHelper.getWebAdminFormFieldStringPlugin(fieldElement);
                        break;
                    default:        throw 'Unknown field type ' + fieldSpec.getType();
                }

                state.body.append(fieldElement);
                state.fieldInstances.push({
                    field:          fieldElement,
                    plugin:         fieldPlugin,
                    propertyName:   propertyName
                });
            });
        },

        /**
         * Returns an array containing every field on the page.
         * @returns {VRS_WEBADMIN_FORM_FIELD_INSTANCE[]}
         */
        getAllFieldInstances: function()
        {
            var state = this._getState();
            return state.fieldInstances.splice(0);
        }
    });
}(window.VRS = window.VRS || {}, jQuery));