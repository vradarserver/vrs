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
 * @fileoverview Part of the form processor, handles field GUIs.
 */
(function(VRS, $, undefined)
{
    VRS.WebAdmin = VRS.WebAdmin || {};
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    //region FormFieldString
    VRS.WebAdmin.FormStringFieldState = function()
    {
        /**
         * @type {jQuery}
         */
        this.input = undefined;

        /**
         * @type {jQuery}
         */
        this.label = undefined;

        /**
         * @type {jQuery}
         */
        this.validationGlyph = undefined;

        /**
         * @type {jQuery}
         */
        this.validationText = undefined;
    };

    /**
     * @param {jQuery} jQueryElement
     * @returns {VRS.formFieldString}
     */
    VRS.jQueryUIHelper.getWebAdminFormFieldStringPlugin = function(jQueryElement) { return jQueryElement.data('vrsFormFieldString'); };

    /**
     * @namespace VRS.formFieldString
     */
    $.widget('vrs.formFieldString', {
        options: {
            /** @type {VRS.WebAdmin.Field} */   fieldSpec: null
        },

        /**
         * @returns {VRS.WebAdmin.FormStringFieldState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('vrs-webadmin-form-field-string-state');
            if(result === undefined) {
                result = new VRS.WebAdmin.FormStringFieldState();
                this.element.data('vrs-webadmin-form-field-string-state', result);
            }

            return result;
        },

        _create: function()
        {
            var options = this.options;
            var state = this._getState();
            var spec = options.fieldSpec;

            this.element.addClass('form-group vrs-input-text');
            state.label = $('<label />')
                .text(spec.getLabel());
            state.input = $('<input />')
                .uniqueId()
                .attr('type', 'text')
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
        },

        getValue: function()
        {
            var state = this._getState();
            return state.input.val();
        },

        setValue: function(value)
        {
            var state = this._getState();
            state.input.val(value);
        },

        showValidationResult: function(validationResult)
        {
            var state = this._getState();
            var message = validationResult ? validationResult.Message : '';
            var isWarning = validationResult ? validationResult.IsWarning : '';

            if(!message) {
                this.element.removeClass('has-feedback has-warning has-error');
                state.validationGlyph.hide();
                state.validationText.hide();
                state.input.attr('aria-invalid', '');
            } else {
                this.element.addClass('has-feedback');
                if(isWarning) this.element.addClass('has-warning').removeClass('has-error');
                else          this.element.addClass('has-error').removeClass('has-warning');
                state.validationGlyph.show();
                state.validationText.show();
                if(isWarning) state.validationGlyph.addClass('glyphicon-warning-sign').removeClass('glyphicon-remove');
                else          state.validationGlyph.addClass('glyphicon-remove').removeClass('glyphicon-warning-sign');
                if(isWarning) state.validationText.addClass('is-warning').removeClass('is-error');
                else          state.validationText.addClass('is-error').removeClass('is-warning');
                state.validationText.text(message);
                state.input.attr('aria-invalid', 'true');
            }
        }
    });
    //endregion

    //region FormFieldCheckbox
    VRS.WebAdmin.FormCheckboxFieldState = function()
    {
        /**
         * @type {jQuery}
         */
        this.input = undefined;

        /**
         * @type {jQuery}
         */
        this.label = undefined;
    };

    /**
     * @param {jQuery} jQueryElement
     * @returns {VRS.formFieldCheckbox}
     */
    VRS.jQueryUIHelper.getWebAdminFormFieldCheckboxPlugin = function(jQueryElement) { return jQueryElement.data('vrsFormFieldCheckbox'); };

    /**
     * @namespace VRS.formFieldString
     */
    $.widget('vrs.formFieldCheckbox', {
        options: {
            /** @type {VRS.WebAdmin.Field} */   fieldSpec: null
        },

        /**
         * @returns {VRS.WebAdmin.FormCheckboxFieldState}
         * @private
         */
        _getState: function()
        {
            var result = this.element.data('vrs-webadmin-form-field-checkbox-state');
            if(result === undefined) {
                result = new VRS.WebAdmin.FormCheckboxFieldState();
                this.element.data('vrs-webadmin-form-field-checkbox-state', result);
            }

            return result;
        },

        _create: function()
        {
            var options = this.options;
            var state = this._getState();
            var spec = options.fieldSpec;

            this.element.addClass('checkbox');
            state.label = $('<label />')
                .text(spec.getLabel());
            state.input = $('<input />')
                .uniqueId()
                .attr('type', 'checkbox');

            this.element.append(state.label);
            state.label.prepend(state.input);
        },

        getValue: function()
        {
            var state = this._getState();
            return state.input.prop('checked');
        },

        setValue: function(value)
        {
            var state = this._getState();
            state.input.prop('checked', !!value);
        },

        showValidationResult: function(validationResult)
        {
        }
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
