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
 * @fileoverview Objects that describe a form that can translate between a JSON object and the content of a form.
 */
(function(VRS, $, undefined)
{
    VRS.WebAdmin = VRS.WebAdmin || {};

    //region Form
    /**
     * Describes a form.
     * @param {VRS_WEBADMIN_FORM} settings
     * @constructor
     */
    VRS.WebAdmin.Form = function(settings)
    {
        var _Pages = [];

        $.each(settings.pages, function(/** number */ idx, /** VRS_WEBADMIN_FORM_PAGE */ pageSettings) {
            var page = new VRS.WebAdmin.FormPage(pageSettings);
            _Pages.push(page);
        });

        /** @returns {VRS.WebAdmin.FormPage[]} */   this.getPages = function() { return _Pages.slice(0); };
    };
    //endregion

    //region FormPage
    /**
     * Describes a page of fields on a form.
     * @param {VRS_WEBADMIN_FORM_PAGE} settings
     * @constructor
     */
    VRS.WebAdmin.FormPage = function(settings)
    {
        var _Fields = [];

        $.each(settings.fields, function(/** number */ idx, /** VRS_WEBADMIN_FORM_FIELD */ fieldSettings) {
            var field = new VRS.WebAdmin.FormField(fieldSettings);
            _Fields.push(field);
        });

        /** @returns {string} */                    this.getTitle = function() { return settings.title; };
        /** @returns {VRS.WebAdmin.FormField[]} */  this.getFields = function() { return _Fields.slice(0); };
    };
    //endregion

    //region FormField
    /**
     * Describes a field.
     * @param {VRS_WEBADMIN_FORM_FIELD} settings
     * @constructor
     */
    VRS.WebAdmin.FormField = function(settings)
    {
        /** @returns {string} */    this.getLabel = function() { return settings.label; };
        /** @returns {string} */    this.getType = function() { return settings.type; };

        /**
         * Returns the property name for the field.
         * @returns {string}
         */
        this.getPropertyName = function()
        {
            return settings.property;
        };

        /**
         * Returns a new jQuery element suitable for housing the field.
         * @returns {jQuery}
         */
        this.createFieldContainer = function()
        {
            return $('<div />');
        };
    };
    //endregion

    //region FormPresenter
    /**
     * Manages the relationship between specification and UI.
     * @param {VRS_WEBADMIN_FORM_PRESENTER} settings
     * @constructor
     */
    VRS.WebAdmin.FormPresenter = function(settings)
    {
        var that = this;

        var _FormPlugin = VRS.jQueryUIHelper.getWebAdminFormProcessorPlugin(settings.formUI);
        var _Data = undefined;

        settings.formUI.on('formprocessorsaveclicked', $.proxy(function(event) { form_saveClicked(event); }, this));

        this.refreshContent = function(data)
        {
            _Data = data;

            var flattenedData = flattenData(_Data);
            var fieldInstances = _FormPlugin.getAllFieldInstances();
            $.each(flattenedData, function(/** Number */ idx, /** VRS_WEBADMIN_NAMEVALUE */ formData) {
                var fieldInstance = fieldInstances.hasOwnProperty(formData.name) ? fieldInstances[formData.name] : null;
                if(fieldInstance !== null) {
                    fieldInstance.plugin.setValue(formData.value);
                }
            });
        };

        /**
         * Flattens the object passed across into an array of properties and values.
         * @param {VRS_WEBADMIN_VALIDATIONRESULTS} validationResults
         */
        this.refreshValidation = function(validationResults)
        {
            var fieldInstances = _FormPlugin.getAllFieldInstances();
            for(var propertyName in fieldInstances) {
                if(fieldInstances.hasOwnProperty(propertyName)) {
                    var fieldInstance = fieldInstances[propertyName];
                    var validationResult = validationResults == null ? null : VRS.arrayHelper.findFirst(validationResults.Results, function(/** VRS_WEBADMIN_VALIDATIONRESULT */ candidate) {
                        return candidate.FieldId === fieldInstance.propertyName;
                    });
                    fieldInstance.plugin.showValidationResult(validationResult);
                }
            }
        };

        /**
         * Collects together the values entered by the user and sends them back to the server
         * for saving. The server performs the validation.
         */
        this.saveData = function()
        {
            if(_Data && _Data.DataVersion) {
                var fieldInstances = _FormPlugin.getAllFieldInstances();
                for(var propertyName in fieldInstances) {
                    if(fieldInstances.hasOwnProperty(propertyName)) {
                        var fieldInstance = fieldInstances[propertyName];
                        var value = fieldInstance.plugin.getValue();
                    }
                }

                $.ajax({
                    url:        settings.saveURL,
                    type:       'POST',
                    cache:      false,
                    data:       _Data,
                    dataType:   'json',
                    error:      saveFailed,
                    success:    saveAcknowledged
                });
            }
        };

        /**
         * Called when the call to saveData fails.
         * @param {jqXHR}   jqXHR
         * @param {String}  textStatus
         * @param {String}  errorThrown
         */
        function saveFailed(jqXHR, textStatus, errorThrown)
        {
            alert('Could not save the form: ' + textStatus + ', error thrown: ' + errorThrown);
        }

        /**
         * Called when the call to save data returns a JSON object. The save may not have worked, we
         * can't tell until we look at the data that's come back.
         * @param data
         */
        function saveAcknowledged(data)
        {
            ;
        }

        /**
         * Flattens the object passed across into an array of properties and values.
         * @param {object} data
         * @returns {VRS_WEBADMIN_NAMEVALUE[]}
         */
        function flattenData(data)
        {
            var result = [];
            recursivelyFlattenData('', data, result);

            return result;
        }

        /**
         * Expands all of the properties of the object into the nameValues list and then recurses
         * into any objects within the object.
         * @param {string}                      parentName
         * @param {object}                      obj
         * @param {VRS_WEBADMIN_NAMEVALUE[]}    nameValues
         */
        function recursivelyFlattenData(parentName, obj, nameValues)
        {
            if(obj !== null && obj !== undefined && $.isPlainObject(obj)) {
                for(var propertyName in obj) {
                    if(obj.hasOwnProperty(propertyName)) {
                        var value = obj[propertyName];
                        var fullName = parentName === '' ? propertyName : parentName + '.' + propertyName;

                        nameValues.push({
                            name: fullName,
                            value: value
                        });

                        recursivelyFlattenData(fullName, value, nameValues);
                    }
                }
            }
        }

        /**
         * Called when the user clicks the save button on the form.
         * @param {Event} event
         */
        function form_saveClicked(event)
        {
            that.saveData();
        }
    };
    //endregion
}(window.VRS = window.VRS || {}, jQuery));