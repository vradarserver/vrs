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
 * @fileoverview The JavaScript for the DatabaseEditor index page.
 */

(function(VRS, $, undefined)
{
    VRS.FeedFilterIndexPageHandler = function(translations)
    {
        // DOM elements
        var _FilterConfigurationPanel = $('#filter-configuration-panel');
        var _SaveResultsPanel = $('#save-results-panel');
        var _FetchErrorPanel = $('#fetch-error');
        var _SaveErrorPanel = $('#save-error');
        var _SaveDuplicatesPanel = $('#save-duplicate');
        var _SaveInvalidPanel = $('#save-invalid');
        var _SavedPanel = $('#saved');
        var _SaveButton = $('#save');

        // Models
        var _FilterConfigurationModel;

        this.initialise = function()
        {
            _SaveErrorPanel.hide();
            _FetchErrorPanel.hide();
            _SavedPanel.hide();
            _SaveDuplicatesPanel.hide();
            _SaveInvalidPanel.hide();

            fetchFilterConfiguration();

            $('input').on('change', function() { _SavedPanel.hide(); });
            $('textarea').on('input propertychange', function() { _SavedPanel.hide(); });

            _SaveButton.on('click', saveClicked);
        };

        function fetchFilterConfiguration()
        {
            _FetchErrorPanel.hide();
            _SavedPanel.hide();
            _SaveButton.prop('disabled', true);
            VRS.pageHelper.showModalWaitAnimation(true);

            $.ajax({
                url: 'FetchFilterConfiguration.json',
                cache: false,
                complete: function() {
                    VRS.pageHelper.showModalWaitAnimation(false);
                },
                error: function(jqXHR, textStatus, errorThrown) {
                    _FetchErrorPanel.text(VRS.stringUtility.format(translations.xhrFailedFormat, errorThrown));
                    _FetchErrorPanel.show();
                },
                success: function(data) {
                    if(data.Exception !== null) {
                        _FetchErrorPanel.text(VRS.stringUtility.format(translations.serverReportedExceptionFormat, data.Exception));
                        _FetchErrorPanel.show();
                    } else {
                        applyFilterConfigurationToModel(data);
                        _SaveButton.prop('disabled', false);
                    }
                }
            });
        }

        function saveClicked()
        {
            saveFilterConfiguration();
        }

        function saveFilterConfiguration()
        {
            _SaveErrorPanel.hide();
            _SavedPanel.hide();
            _SaveDuplicatesPanel.hide();
            _SaveInvalidPanel.hide();
            _SaveButton.prop('disabled', true);

            VRS.pageHelper.showModalWaitAnimation(true);

            $.ajax({
                url: 'SaveFilterConfiguration.json',
                cache: false,
                method: 'POST',
                //contentType: 'application/x-www-form-urlencoded',       // required for VRS 2.2.0 and below, those versions can't handle the "; charset=UTF8" suffix
                data: {
                    DataVersion:        _FilterConfigurationModel.DataVersion(),
                    ProhibitMlat:       _FilterConfigurationModel.ProhibitMlat(),
                    ProhibitedIcaos:    _FilterConfigurationModel.ProhibitedIcaos()
                },
                complete: function() {
                    VRS.pageHelper.showModalWaitAnimation(false);
                },
                error: function(jqXHR, textStatus, errorThrown) {
                    _SaveErrorPanel.text(VRS.stringUtility.format(translations.xhrFailedFormat, errorThrown));
                    _SaveErrorPanel.show();
                },
                success: function(data) {
                    if(data.Exception !== null) {
                        _SaveErrorPanel.text(VRS.stringUtility.format(translations.serverReportedExceptionFormat, data.Exception));
                        _SaveErrorPanel.show();
                    } else if(data.WasStaleData) {
                        _SaveErrorPanel.text(translations.couldNotSaveOutOfDate);
                        _SaveErrorPanel.show();
                    } else {
                        if(data.DuplicateProhibitedIcaos.length > 0) {
                            showWarningListPanel(_SaveDuplicatesPanel, translations.foundDuplicates, data.DuplicateProhibitedIcaos);
                        }
                        if(data.InvalidProhibitedIcaos.length > 0) {
                            showWarningListPanel(_SaveInvalidPanel, translations.foundInvalidIcaos, data.InvalidProhibitedIcaos);
                        }

                        applyFilterConfigurationToModel(data);
                        _SaveButton.prop('disabled', false);
                    }
                    _SavedPanel.show();
                }
            });
        }

        function showWarningListPanel(jqPanel, prefixMessage, icaosList)
        {
            var message = prefixMessage + ': ';
            var length = icaosList.length;
            for(var i = 0;i < length;++i) {
                if(i) message += ', ';
                message += icaosList[i];
            }

            jqPanel.text(message);
            jqPanel.show();
        }

        function applyFilterConfigurationToModel(filterConfiguration)
        {
            if(_FilterConfigurationModel) {
                ko.mapping.fromJS(filterConfiguration, _FilterConfigurationModel);
            } else {
                _FilterConfigurationModel = ko.mapping.fromJS(filterConfiguration);
                ko.applyBindings(_FilterConfigurationModel, _FilterConfigurationPanel[0]);
            }
        }
    };
}(window.VRS = window.VRS || {}, jQuery));