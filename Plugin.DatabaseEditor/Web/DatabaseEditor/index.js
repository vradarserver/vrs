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
    VRS.DatabaseEditorIndexPageHandler = function(translations)
    {
        // DOM elements
        var _CriteriaPanel = $('#criteria-panel');
        var _RecordPanel = $('#record-panel');
        var _SearchButton = $('#search');
        var _SearchError = $('#search-error');
        var _SaveButton = $('#save');
        var _SaveError = $('#save-error');
        var _SaveSuccess = $('#save-success');

        // Models
        var _SearchModel = new VRS.DatabaseEditorSearchModel();
        var _RecordModel;

        // Model mapping
        var _RecordModelMapping = {
            'ICAOTypeCode': { create: function(options) { return ko.observable(options.data).extend({ uppercase: true }); } },
            'OperatorIcao': { create: function(options) { return ko.observable(options.data).extend({ uppercase: true }); } },
            'Registration': { create: function(options) { return ko.observable(options.data).extend({ uppercase: true }); } }
        };

        /**
         * Initialises the page.
         */
        this.initialise = function() {
            _RecordPanel.hide();
            _SearchError.hide();
            _SaveSuccess.hide();

            $('input').on('input change', function() { _SaveSuccess.hide(); });
            $('textarea').on('input propertychange', function() { _SaveSuccess.hide(); });

            _SearchButton.on('click', SearchButton_Clicked);
            _SaveButton.on('click', SaveButton_Clicked);

            ko.applyBindings(_SearchModel, _CriteriaPanel[0]);

            var pageUrl = $.url();
            _SearchModel.icao(pageUrl.param('icao') || '');
            if(_SearchModel.icao().length == 6) {
                SearchButton_Clicked();
            }
        };

        /**
         * Called when the search button is clicked.
         */
        function SearchButton_Clicked()
        {
            _SearchError.hide();
            _RecordPanel.hide();
            _SaveError.hide();
            _SaveSuccess.hide();

            VRS.pageHelper.showModalWaitAnimation(true);

            $.ajax({
                url: 'SingleAircraftSearch.json',
                cache: false,
                complete: function() {
                    VRS.pageHelper.showModalWaitAnimation(false);
                },
                data: {
                    icao: _SearchModel.icao()
                },
                error: function(jqXHR, textStatus, errorThrown) {
                    _SearchError.text(VRS.stringUtility.format(translations.xhrFailedFormat, errorThrown));
                    _SearchError.show();
                },
                success: function(data) {
                    if(data.Exception !== null) {
                        _SearchError.text(VRS.stringUtility.format(translations.serverReportedExceptionFormat, data.Exception));
                        _SearchError.show();
                    } else if(data.Aircraft === null) {
                        _SearchError.text(translations.noDatabaseRecord);
                        _SearchError.show();
                    } else {
                        if(_RecordModel) {
                            ko.mapping.fromJS(data.Aircraft, _RecordModelMapping, _RecordModel);
                        } else {
                            _RecordModel = ko.mapping.fromJS(data.Aircraft, _RecordModelMapping);
                            ko.applyBindings(_RecordModel, _RecordPanel[0]);
                        }
                        _RecordPanel.show();
                    }
                }
            });
        }

        /**
         * Called when the save button is clicked.
         */
        function SaveButton_Clicked()
        {
            VRS.pageHelper.showModalWaitAnimation(true);
            var data = ko.mapping.toJS(_RecordModel);

            $.ajax({
                url: 'SingleAircraftSave.json',
                cache: false,
                method: 'POST',
                contentType: "application/json",
                data: JSON.stringify(data),
                complete: function() {
                    VRS.pageHelper.showModalWaitAnimation(false);
                },
                error: function(jqXHR, textStatus, errorThrown) {
                    _SaveError.text(VRS.stringUtility.format(translations.xhrFailedFormat, errorThrown));
                    _SaveError.show();
                },
                success: function(data) {
                    if(data.Exception !== null) {
                        _SaveError.text(VRS.stringUtility.format(translations.serverReportedExceptionFormat, data.Exception));
                        _SaveError.show();
                    } else {
                        ko.mapping.fromJS(data.Aircraft, _RecordModelMapping, _RecordModel);
                        _SaveSuccess.show();
                        _SaveError.hide();
                    }
                }
            });
        }
    };
}(window.VRS = window.VRS || {}, jQuery));