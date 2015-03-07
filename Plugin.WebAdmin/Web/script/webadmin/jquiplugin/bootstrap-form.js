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
 * @fileoverview A jQuery UI plugin that displays bootstrap form elements.
 */
(function(VRS, $, undefined)
{
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    //region bootstrapForm
    $.widget('vrs.bootstrapForm', {
        _create: function()
        {
            this.element
                .addClass('panel-group')
                .attr('role', 'tablist')
                .attr('aria-multiselectable', 'true');

            $('[data-vrs-plugin="form-page"]', this.element).bootstrapFormPage();
            $('[data-vrs-plugin="field-text"]', this.element).bootstrapFieldText();
            $('[data-vrs-plugin="field-checkbox"]', this.element).bootstrapFieldCheckbox();
        }
    });
    //endregion

    //region bootstrapFormPage
    $.widget('vrs.bootstrapFormPage', {
        /**
         * @typedef {{
         * heading:         jQuery,
         * bodyOuter:       jQuery,
         * body:            jQuery
         * }} VRS_BOOTSTRAP_FORM_PAGE_STATE
         * @private
         */
        /**
         * @returns {VRS_BOOTSTRAP_FORM_PAGE_STATE}
         * @private
         */
        _getState: function()
        {
            var key = 'vrs-bootstrap-form-page-state';
            var result = this.element.data(key);
            if(result === undefined) {
                result = {
                    heading:    undefined,
                    bodyOuter:  undefined,
                    body:       undefined
                };
                this.element.data(key, result);
            }

            return result;
        },

        _create: function()
        {
            var state = this._getState();
            var parent = this.element.closest('.panel-group');
            var parentId = parent.attr('id');
            var title = this.element.attr('data-vrs-title');
            var id = this.element.attr('id');
            var collapsed = $('[data-vrs-plugin="form-page"]').first().attr('id') !== id;

            this.element
                .addClass('panel panel-default');

            var content = this.element.children();
            content.detach();

            state.bodyOuter = $('<div />')
                .uniqueId()
                .addClass('panel-collapse collapse')
                .addClass(collapsed ? '' : 'in')
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
                        .addClass(collapsed ? 'collapsed' : '')
                        .attr('data-toggle', 'collapse')
                        .attr('data-parent', '#' + parentId)
                        .attr('href', '#' + bodyOuterId)
                        .attr('aria-expanded', collapsed ? 'false' : 'true')
                        .attr('aria-controls', bodyOuterId) // no hash
                        .text(title)
                )
            );
            var headingId = state.heading.attr('id');
            state.bodyOuter.attr('aria-labelledby', headingId);

            this.element.append(state.heading);
            this.element.append(state.bodyOuter);
            state.body.append(content);
        }
    });
    //endregion

    //region bootstrapFieldText
    $.widget('vrs.bootstrapFieldText', {
        /**
         * @typedef {{
         * label:           jQuery,
         * input:           jQuery,
         * validationGlyph: jQuery,
         * validationText:  jQuery
         * }} VRS_BOOTSTRAP_FIELD_TEXT_STATE
         * @private
         */
        /**
         * @returns {VRS_BOOTSTRAP_FIELD_TEXT_STATE}
         * @private
         */
        _getState: function()
        {
            var key = 'vrs-bootstrap-field-text-state';
            var result = this.element.data(key);
            if(result === undefined) {
                result = {
                    label:              undefined,
                    input:              undefined,
                    validationGlyph:    undefined,
                    validationText:     undefined
                };
                this.element.data(key, result);
            }

            return result;
        },

        _create: function()
        {
            var state = this._getState();
            var title = this.element.attr('data-vrs-title');
            var dataField = this.element.attr('data-vrs-bind');

            this.element.addClass('form-group vrs-input-text');
            state.label = $('<label />')
                .text(title);
            state.input = $('<input />')
                .uniqueId()
                .attr('type', 'text')
                .attr('data-bind', 'value: ' + dataField)
                .addClass('form-control');
            state.label.attr('for', '#' + state.input.attr('id'));
            state.validationGlyph = $('<span />')
                .attr('aria-hidden', 'true')
                .addClass('glyphicon form-control-feedback')
                .hide();
            state.validationText = $('<span />')
                .uniqueId()
                .addClass('validation-message')
                .hide();
            state.input.attr('aria-describedby', state.validationText.attr('id'));

            this.element
                .append(state.label)
                .append(state.input)
                .append(state.validationGlyph)
                .append(state.validationText);
        }
    });
    //endregion

    //region bootstrapFieldCheckbox
    $.widget('vrs.bootstrapFieldCheckbox', {
        /**
         * @typedef {{
         * label:           jQuery,
         * input:           jQuery,
         * }} VRS_BOOTSTRAP_FIELD_CHECKBOX_STATE
         * @private
         */
        /**
         * @returns {VRS_BOOTSTRAP_FIELD_CHECKBOX_STATE}
         * @private
         */
        _getState: function()
        {
            var key = 'vrs-bootstrap-field-checkbox-state';
            var result = this.element.data(key);
            if(result === undefined) {
                result = {
                    label:              undefined,
                    input:              undefined
                };
                this.element.data(key, result);
            }

            return result;
        },

        _create: function()
        {
            var state = this._getState();
            var title = this.element.attr('data-vrs-title');
            var dataField = this.element.attr('data-vrs-bind');

            this.element.addClass('checkbox');
            state.label = $('<label />')
                .text(title);
            state.input = $('<input />')
                .uniqueId()
                .attr('type', 'checkbox')
                .attr('data-bind', 'checked: ' + dataField);

            this.element.append(state.label);
            state.label.prepend(state.input);
        }
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
