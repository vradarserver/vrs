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
 * @fileoverview A jQuery UI plugin that displays bootstrap form elements. Assumes the use on knockout.
 */
(function(VRS, $, undefined)
{
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};

    //region BootstrapFormHelper
    VRS.BootstrapFormHelper = function()
    {
        var that = this;

        var _FieldDataNames = [];

        /**
         * For internal use - called for each field plugin to register its data name. Used when trying to fish
         * the plugin object from an abstract jQuery element.
         * @param fieldDataName
         */
        this.addFieldDataName = function(fieldDataName)
        {
            _FieldDataNames.push(fieldDataName);
        };

        /**
         * Extracts the value of the data-vrs-bind attribute on the element passed across.
         * @param {jQuery}      elementJQ
         * @param {Boolean}    [isMandatory]    Defaults to true
         * @returns {string}
         */
        this.getDataVrsBindAttr = function(elementJQ, isMandatory)
        {
            return that.getAttributeValue(elementJQ, 'data-vrs-bind', isMandatory, true);
        };

        /**
         * Extracts the value of the data-vrs-class attribute on the element passed across.
         * @param {jQuery}      elementJQ
         * @param {Boolean}    [isMandatory]    Defaults to false
         * @returns {string}
         */
        this.getDataVrsClassAttr = function(elementJQ, isMandatory)
        {
            return that.getAttributeValue(elementJQ, 'data-vrs-class', isMandatory, false);
        };

        /**
         * Reads the data-vrs-field-flags attribute of semi-colon separated flags and assigns the
         * value true to each flag in the flags object passed across.
         * @param {jQuery}  elementJQ
         * @param {object}  flags
         * @returns {object}
         */
        this.getDataVrsFlags = function(elementJQ, flags)
        {
            var concatentatedFlagsText = that.getAttributeValue(elementJQ, 'data-vrs-field-flags', false, false);
            if(concatentatedFlagsText !== undefined) {
                var flagTextArray = concatentatedFlagsText.split(';');
                $.each(flagTextArray, function(/** Number */ idx, /** String */ flagText) {
                    flags[flagText] = true;
                });
            }

            return flags;
        };

        /**
         * Extracts the value of the data-vrs-icon attribute on the element passed across.
         * @param {jQuery}      elementJQ
         * @param {Boolean}    [isMandatory]    Defaults to false
         * @returns {string}
         */
        this.getDataVrsIconAttr = function(elementJQ, isMandatory)
        {
            return that.getAttributeValue(elementJQ, 'data-vrs-icon', isMandatory, false);
        };

        /**
         * Extracts the value of the data-vrs-title attribute on the element passed across.
         * @param {jQuery}      elementJQ
         * @param {Boolean}    [isMandatory]    Defaults to false
         * @returns {string}
         */
        this.getDataVrsTitleAttr = function(elementJQ, isMandatory)
        {
            return that.getAttributeValue(elementJQ, 'data-vrs-title', isMandatory, false);
        };

        /**
         * Extracts the value of the data-vrs-suffix attribute on the element passed across.
         * @param {jQuery}      elementJQ
         * @param {Boolean}     trimParenthesis
         * @returns {string}
         */
        this.getDataVrsSuffixAttr = function(elementJQ, trimParenthesis)
        {
            var result = that.getAttributeValue(elementJQ, 'data-vrs-suffix', false, false);
            if(result !== undefined && trimParenthesis && result.length >= 2) {
                if(result[0] == '(') result = result.substring(1);
                if(result[result.length - 1] == ')') result = result.substring(0, result.length - 1);
            }

            return result;
        };

        /**
         * Does the work for the functions that extract values from common attributes.
         * @param {jQuery}      elementJQ
         * @param {string}      attribute
         * @param {Boolean}     isMandatory
         * @param {Boolean}     defaultIsMandatory
         * @returns {*|string|!jQuery}
         */
        this.getAttributeValue = function(elementJQ, attribute, isMandatory, defaultIsMandatory)
        {
            if(isMandatory === undefined) isMandatory = defaultIsMandatory;
            var result = elementJQ.attr(attribute);
            if(result === undefined && isMandatory) throw 'Missing ' + attribute + ' attribute on VRS Bootstrap form element';

            return result;
        };

        /**
         * Forms a data-bind attribute property string from an object where each property is the
         * name of the value to bind to and the value of each property is the setting. Properties
         * with a null or empty value are ignored.
         * @param {Object} dataBindings
         * @returns {string}
         */
        this.glueDataBindings = function(dataBindings)
        {
            var result = '';

            for(var propertyName in dataBindings) {
                if(dataBindings.hasOwnProperty(propertyName)) {
                    var value = dataBindings[propertyName];
                    if(value !== undefined && value !== null && String(value) !== '') {
                        if(result.length > 0) result += ', ';
                        result += propertyName + ': ' + value;
                    }
                }
            }

            return result;
        };

        /**
         * Appends '-built' to the value in data-vrs-plugin so that the element is not rebuilt in
         * subsequent refreshes.
         * @param {jQuery} elementJQ
         */
        this.markPluginAsBuilt = function(elementJQ)
        {
            var attribute = elementJQ.attr('data-vrs-plugin');
            if(attribute === undefined) throw 'The element passed to markPluginAsBuilt is not a VRS field element';
            if(attribute.indexOf('-built') === -1) {
                attribute += '-built';
                elementJQ.attr('data-vrs-plugin', attribute);
            }
        };

        /**
         * Returns the field plugin object attached to the element passed across, or undefined if one could not be found.
         * @param elementJQ
         * @returns {object|undefined}
         */
        this.getFieldPlugin = function(elementJQ)
        {
            var result = undefined;

            var length = _FieldDataNames.length;
            for(var i = 0;result === undefined && i < length;++i) {
                result = elementJQ.data(_FieldDataNames[i]);
                if(result === null) result = undefined;
            }

            return result;
        };

        /**
         * Sets the appropriate Bootstrap validation state class on a jQuery element.
         * @param {jQuery}                          elementJQ
         * @param {VRS_WEBADMIN_VALIDATIONRESULT}   validationResult
         */
        this.setBootstrapValidationState = function(elementJQ, validationResult)
        {
            if(!validationResult)               elementJQ.removeClass('has-warning has-error');
            else if(validationResult.IsWarning) elementJQ.removeClass('has-error').addClass('has-warning');
            else                                elementJQ.removeClass('has-warning').addClass('has-error');
        };

        /**
         * Sets the appropriate Bootstrap glyphicon class on a jQuery element for the validation result passed across.
         * @param {jQuery}                          elementJQ
         * @param {VRS_WEBADMIN_VALIDATIONRESULT}   validationResult
         */
        this.setBootstrapValidationGlyphIcon = function(elementJQ, validationResult)
        {
            if(!validationResult)               elementJQ.hide();
            else if(validationResult.IsWarning) elementJQ.removeClass('glyphicon-minus-sign').addClass('glyphicon-exclamation-sign');
            else                                elementJQ.removeClass('glyphicon-exclamation-sign').addClass('glyphicon-minus-sign');
        };
    };
    VRS.bootstrapFormHelper = new VRS.BootstrapFormHelper();
    //endregion

    //region bootstrapForm
    /**
     * @param {jQuery} jQueryElement
     * @returns {VRS.bootstrapModal}
     */
    VRS.jQueryUIHelper.getBootstrapFormPlugin = function(jQueryElement) { return jQueryElement.data('vrsBootstrapForm'); };

    /**
     * The main form plugin.
     * @namespace VRS.bootstrapForm
     */
    $.widget('vrs.bootstrapForm', {
        _create: function()
        {
            this.element
                .addClass('panel-group')
                .attr('role', 'tablist')
                .attr('aria-multiselectable', 'true');

            this.refresh();
        },

        refresh: function()
        {
            $('[data-vrs-plugin="form-page"]', this.element).bootstrapFormPage();
            $('[data-vrs-plugin="field-text"]', this.element).bootstrapFieldText();
            $('[data-vrs-plugin="field-numeric"]', this.element).bootstrapFieldNumeric();
            $('[data-vrs-plugin="field-checkbox"]', this.element).bootstrapFieldCheckbox();
            $('[data-vrs-plugin="field-select"]', this.element).bootstrapFieldSelect();
            $('[data-vrs-plugin="field-list"]', this.element).bootstrapFieldList();
        },

        showValidationResults: function(validationResults)
        {
            $('[data-vrs-validation-field]').each(function() {
                var element = $(this);
                var fieldPlugin = VRS.bootstrapFormHelper.getFieldPlugin(element);
                if(fieldPlugin) {
                    var fieldId = element.attr('data-vrs-validation-field');
                    var recordOwner = element.closest('[data-vrs-validation-record]');
                    var recordType = recordOwner.attr('data-vrs-validation-record');
                    var recordId = recordOwner.attr('data-vrs-validation-record-id');
                    if(recordType === undefined) recordType = null;
                    if(recordId === undefined) recordId = null;
                    var validationResult = VRS.arrayHelper.findFirst(validationResults.Results, function(/** VRS_WEBADMIN_VALIDATIONRESULT */ candidate) {
                        return candidate.FieldId === fieldId &&
                               candidate.RecordType === recordType &&
                               candidate.RecordId === recordId;
                    });
                    fieldPlugin.showValidationResult(validationResult);
                }
            });
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
            VRS.bootstrapFormHelper.markPluginAsBuilt(this.element);

            var state = this._getState();
            var parent = this.element.closest('.panel-group');
            var parentId = parent.attr('id');
            var title = VRS.bootstrapFormHelper.getDataVrsTitleAttr(this.element);
            var icon = VRS.bootstrapFormHelper.getDataVrsIconAttr(this.element);
            var id = this.element.attr('id');
            var collapsed = $('[data-vrs-plugin|="form-page"]', parent).first().attr('id') != id;

            this.element
                .addClass('panel panel-default');

            var content = this.element.children();
            content.detach();

            state.bodyOuter = $('<div />')
                .uniqueId()
                .addClass('panel-collapse collapse')
                .addClass(collapsed ? '' : 'in')
                .attr('role', 'tabpanel');
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

            if(icon) {
                var iconElement = $('<img />')
                    .attr('src', icon)
                    .attr('alt', 'Logo')
                    .attr('title', 'Logo')
                    .attr('width', 20)
                    .attr('height', 20);
                var iconParent = $('a', state.heading);
                iconParent.prepend(iconElement);
            }

            this.element.append(state.heading);
            this.element.append(state.bodyOuter);
            state.body.append(content);
        }
    });
    //endregion

    //region bootstrapFieldText
    VRS.bootstrapFormHelper.addFieldDataName('vrsBootstrapFieldText');
    $.widget('vrs.bootstrapFieldText', {
        /**
         * @typedef {{
         * label:           jQuery,
         * input:           jQuery,
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
                    validationText:     undefined
                };
                this.element.data(key, result);
            }

            return result;
        },

        _create: function()
        {
            VRS.bootstrapFormHelper.markPluginAsBuilt(this.element);

            var state = this._getState();
            var title = VRS.bootstrapFormHelper.getDataVrsTitleAttr(this.element);
            var dataField = VRS.bootstrapFormHelper.getDataVrsBindAttr(this.element);

            this.element.addClass('form-group vrs-input-text');
            state.label = $('<label />')
                .text(title);
            state.input = $('<input />')
                .uniqueId()
                .attr('type', 'text')
                .attr('data-bind', 'textInput: ' + dataField)
                .addClass('form-control');
            state.label.attr('for', '#' + state.input.attr('id'));

            this.element
                .append(state.label)
                .append(state.input);
        },

        showValidationResult: function(/** VRS_WEBADMIN_VALIDATIONRESULT */ validationResult)
        {
            var state = this._getState();

            VRS.bootstrapFormHelper.setBootstrapValidationState(this.element, validationResult);

            if(validationResult === undefined) {
                if(state.validationText) {
                    state.validationText.remove();
                    state.input.attr('aria-describedby', null);
                    state.validationText = null;
                }
            } else {
                if(!state.validationText) {
                    state.validationText = $('<span />')
                        .uniqueId()     // for screen readers
                        .addClass('validation-message')
                        .appendTo(this.element);
                }
                state.validationText.text(validationResult.Message);
                var glyphIcon = $('<span />')
                    .addClass('glyphicon')
                    .prependTo(state.validationText);
                VRS.bootstrapFormHelper.setBootstrapValidationGlyphIcon(glyphIcon, validationResult);
                state.input.attr('aria-describedby', state.validationText.attr('id'));
            }
        }
    });
    //endregion

    //region bootstrapFieldNumeric
    // This uses the 3rd party library 'Bootstrap Touchspin', which is available here:
    //      http://www.virtuosoft.eu/code/bootstrap-touchspin/
    // and here:
    //      https://github.com/istvan-ujjmeszaros/bootstrap-touchspin
    VRS.bootstrapFormHelper.addFieldDataName('vrsBootstrapFieldNumeric');
    $.widget('vrs.bootstrapFieldNumeric', {
        /**
         * @typedef {{
         * label:           jQuery,
         * input:           jQuery,
         * suffix:          jQuery
         * }} VRS_BOOTSTRAP_FIELD_NUMERIC_STATE
         * @private
         */
        /**
         * @returns {VRS_BOOTSTRAP_FIELD_NUMERIC_STATE}
         * @private
         */
        _getState: function()
        {
            var key = 'vrs-bootstrap-field-numeric-state';
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
            VRS.bootstrapFormHelper.markPluginAsBuilt(this.element);

            var state = this._getState();
            var title = VRS.bootstrapFormHelper.getDataVrsTitleAttr(this.element);
            var dataField = VRS.bootstrapFormHelper.getDataVrsBindAttr(this.element);
            var suffix = VRS.bootstrapFormHelper.getDataVrsSuffixAttr(this.element, true);
            var minVal = Number(this.element.attr('data-vrs-min'));
            var maxVal = Number(this.element.attr('data-vrs-max'));
            var stepValText = this.element.attr('data-vrs-step');
            var stepVal = stepValText === undefined ? 1 : Number(stepValText);
            if(isNaN(minVal)) throw 'Numeric min value is missing or NaN';
            if(isNaN(maxVal)) throw 'Numeric max value is missing or NaN';
            if(isNaN(stepVal)) throw 'Numeric step value is NaN';

            this.element.addClass('form-group vrs-input-numeric');
            state.label = $('<label />')
                .text(title);
            state.input = $('<input />')
                .uniqueId()
                .attr('type', 'text')
                .attr('data-bind', 'textInput: ' + dataField);
            state.label.attr('for', '#' + state.input.attr('id'));

            this.element
                .append(state.label)
                .append(state.input);

            state.input.TouchSpin({
                min: minVal,
                max: maxVal,
                stepinterval: stepVal,
                postfix: suffix ? suffix : ''
            });
        }
    });
    //endregion

    //region bootstrapFieldCheckbox
    VRS.bootstrapFormHelper.addFieldDataName('vrsBootstrapFieldCheckbox');
    $.widget('vrs.bootstrapFieldCheckbox', {
        /**
         * @typedef {{
         * label:           jQuery,
         * input:           jQuery
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
            VRS.bootstrapFormHelper.markPluginAsBuilt(this.element);

            var state = this._getState();
            var title = VRS.bootstrapFormHelper.getDataVrsTitleAttr(this.element, true);
            var dataField = VRS.bootstrapFormHelper.getDataVrsBindAttr(this.element);

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

    //region bootstrapFieldSelect
    VRS.bootstrapFormHelper.addFieldDataName('vrsBootstrapFieldSelect');
    $.widget('vrs.bootstrapFieldSelect', {
        /**
         * @typedef {{
         * label:           jQuery,
         * select:          jQuery,
         * unset:           jQuery
         * }} VRS_BOOTSTRAP_FIELD_SELECT_STATE
         * @private
         */
        /**
         * @returns {VRS_BOOTSTRAP_FIELD_SELECT_STATE}
         * @private
         */
        _getState: function()
        {
            var key = 'vrs-bootstrap-field-select-state';
            var result = this.element.data(key);
            if(result === undefined) {
                result = {
                    label:      undefined,
                    select:     undefined,
                    unset:      undefined
                };
                this.element.data(key, result);
            }

            return result;
        },

        _create: function()
        {
            VRS.bootstrapFormHelper.markPluginAsBuilt(this.element);

            var state = this._getState();
            var flags = VRS.bootstrapFormHelper.getDataVrsFlags(this.element, {
                allowUnset: false
            });
            var title = VRS.bootstrapFormHelper.getDataVrsTitleAttr(this.element);
            var dataField = VRS.bootstrapFormHelper.getDataVrsBindAttr(this.element);
            var optionsBinding = VRS.bootstrapFormHelper.getAttributeValue(this.element, 'data-vrs-bind-options', true, true);
            var optionsCaption = VRS.bootstrapFormHelper.getAttributeValue(this.element, 'data-vrs-caption', false, false);
            var optionsText = VRS.bootstrapFormHelper.getAttributeValue(this.element, 'data-vrs-options-text', false, false);
            var optionsValue = VRS.bootstrapFormHelper.getAttributeValue(this.element, 'data-vrs-options-value', false, false);

            if(optionsText !== undefined && optionsText.indexOf('function') !== 0) {
                optionsText = 'function(item) { return ' + optionsText + '; }';
            }
            if(optionsValue !== undefined && optionsValue.indexOf('function') !== 0) {
                optionsValue = 'function(item) { return ' + optionsValue + '; }';
            }

            var dataBind = VRS.bootstrapFormHelper.glueDataBindings({
                value: dataField,
                options: optionsBinding,
                optionsCaption: optionsCaption,
                optionsText: optionsText,
                optionsValue: optionsValue,
                valueAllowUnset: flags.allowUnset
            });

            state.label = $('<label />')
                .text(title);
            state.select = $('<select />')
                .uniqueId()
                .addClass('form-control')
                .attr('data-bind', dataBind);

            this.element.append(state.label);
            state.label.append(state.select);
        }
    });
    //endregion

    //region bootstrapFieldList
    VRS.bootstrapFormHelper.addFieldDataName('vrsBootstrapFieldList');
    $.widget('vrs.bootstrapFieldList', {
        /**
         * @typedef {{
         * dataField:       string,
         * title:           string,
         * class:           string
         * }} VRS_BOOTSTRAP_FIELD_LIST_COLUMN
         *
         * @typedef {{
         * table:           jQuery,
         * detailElement:   jQuery
         * }} VRS_BOOTSTRAP_FIELD_LIST_STATE
         */

        /**
         * @returns {VRS_BOOTSTRAP_FIELD_LIST_STATE}
         * @private
         */
        _getState: function()
        {
            var key = 'vrs-bootstrap-field-list-state';
            var result = this.element.data(key);
            if(result === undefined) {
                result = {
                    table:  undefined
                };
                this.element.data(key, result);
            }

            return result;
        },

        _create: function()
        {
            VRS.bootstrapFormHelper.markPluginAsBuilt(this.element);

            var state = this._getState();
            var title = VRS.bootstrapFormHelper.getDataVrsTitleAttr(this.element);
            var dataField = VRS.bootstrapFormHelper.getDataVrsBindAttr(this.element);
            var columnsElement = $('[data-vrs-plugin="list-columns"]', this.element);
            var detailElement = $('[data-vrs-plugin="list-detail"]', this.element);

            var hasDetailElement = detailElement.length > 0;
            if(hasDetailElement) {
                detailElement.remove();
                detailElement.attr('data-vrs-plugin', null);
                detailElement.addClass('vrs-panel');

                state.detailElement = detailElement;
            }

            /** @type {VRS_BOOTSTRAP_FIELD_LIST_COLUMN[]} */
            var columns = [];
            $.each(columnsElement.children(), function(/** Number */ idx, /** jQuery */ columnElement) {
                var columnJQuery = $(columnElement);
                columns.push({
                    dataField:  VRS.bootstrapFormHelper.getDataVrsBindAttr(columnJQuery),
                    title:      VRS.bootstrapFormHelper.getDataVrsTitleAttr(columnJQuery),
                    class:      VRS.bootstrapFormHelper.getDataVrsClassAttr(columnJQuery)
                });
            });
            columnsElement.remove();

            this.element
                .addClass('table-responsive');
            state.table = $('<table />')
                .addClass('table table-condensed')
                .appendTo(this.element);
            if(hasDetailElement) {
                state.table.addClass('has-detail');
            }

            var head = $('<thead />').appendTo(state.table);
            var headRow = $('<tr />').appendTo(head);
            $.each(columns, function(/** Number */ idx, /** VRS_BOOTSTRAP_FIELD_LIST_COLUMN} */ column) {
                $('<th />')
                    .text(column.title)
                    .addClass(column.class)
                    .appendTo(headRow);
            });

            var body = $('<tbody />')
                .appendTo(state.table)
                .attr('data-bind', 'foreach: ' + dataField);
            var bodyRow = $('<tr />')
                .addClass('vrs-list-master')
                .appendTo(body);
            $.each(columns, function(/** Number */ idx, /** VRS_BOOTSTRAP_FIELD_LIST_COLUMN */ column) {
                $('<td />')
                    .attr('data-bind', 'text: ' + column.dataField)
                    .addClass(column.class)
                    .appendTo(bodyRow);
            });

            if(hasDetailElement) {
                var detailRow = $('<tr />')
                    .addClass('vrs-list-detail')
                    .hide()
                    .append($('<td />')
                        .attr('colspan', columns.length)
                        .html(state.detailElement.clone())
                    )
                    .appendTo(body);

                bodyRow.addClass('clickable');
                bodyRow.attr('data-bind',
                    'event: { click: function(data, event) { ' +
                        '$(event.currentTarget)' +
                            '.closest(\'[data-vrs-plugin=\"field-list-built\"]\')' +
                            '.bootstrapFieldList(\'rowClicked\', data, event.currentTarget);' +
                    '}, clickBubble: false }');
            }
        },

        rowClicked: function(data, targetRow)
        {
            var state = this._getState();
            var row = $(targetRow);
            var otherRows = $('.vrs-list-master').not(row);
            var detailRow = row.next();
            var otherDetailRows = $('.vrs-list-detail', this.element).not(detailRow);

            otherRows.removeClass('info');
            otherDetailRows.hide();
            detailRow.toggle();
            row.toggleClass('info');
        }
    });
    //endregion
}(window.VRS = window.VRS || {}, jQuery));
