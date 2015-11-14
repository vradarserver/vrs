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
var DatabaseEditor;
(function (DatabaseEditor) {
    var Index;
    (function (Index) {
        var Translations = (function () {
            function Translations() {
            }
            return Translations;
        })();
        Index.Translations = Translations;
        var PageHandler = (function () {
            function PageHandler(translations) {
                this._CriteriaPanel = $('#criteria-panel');
                this._RecordPanel = $('#record-panel');
                this._SearchButton = $('#search');
                this._SearchError = $('#search-error');
                this._SaveButton = $('.save');
                this._SaveError = $('.save-error');
                this._SaveSuccess = $('.save-success');
                this._SearchModel = new SearchModel();
                var self = this;
                this._Translations = translations;
                this._RecordPanel.hide();
                this._SearchError.hide();
                this._SaveSuccess.hide();
                DatabaseEditor.BootstapUtility.formatHorizontalFormInputs($('body'));
                $('input').on('input change', function () { self._SaveSuccess.hide(); });
                $('textarea').on('input propertychange', function () { self._SaveSuccess.hide(); });
                this._SearchButton.on('click', function () { self.SearchButton_Clicked(); });
                this._SaveButton.on('click', function () { self.SaveButton_Clicked(); });
                ko.applyBindings(this._SearchModel, this._CriteriaPanel[0]);
                var pageUrl = $.url();
                this._SearchModel.icao(pageUrl.param('icao') || '');
                if (this._SearchModel.icao().length == 6) {
                    this.SearchButton_Clicked();
                }
            }
            PageHandler.prototype.SearchButton_Clicked = function () {
                var self = this;
                this._SearchError.hide();
                this._RecordPanel.hide();
                this._SaveError.hide();
                this._SaveSuccess.hide();
                VRS.pageHelper.showModalWaitAnimation(true);
                $.ajax({
                    url: 'SingleAircraftSearch.json',
                    cache: false,
                    complete: function () {
                        VRS.pageHelper.showModalWaitAnimation(false);
                    },
                    data: {
                        icao: this._SearchModel.icao()
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        self._SearchError.text(VRS.stringUtility.format(self._Translations.xhrFailedFormat, errorThrown));
                        self._SearchError.show();
                    },
                    success: function (data) {
                        if (data.Exception !== null) {
                            self._SearchError.text(VRS.stringUtility.format(self._Translations.serverReportedExceptionFormat, data.Exception));
                            self._SearchError.show();
                        }
                        else if (data.Aircraft === null) {
                            self._SearchError.text(self._Translations.noDatabaseRecord);
                            self._SearchError.show();
                        }
                        else {
                            if (self._RecordModel) {
                                ko.mapping.fromJS(data.Aircraft, RecordModel.mapping, self._RecordModel);
                            }
                            else {
                                self._RecordModel = ko.mapping.fromJS(data.Aircraft, RecordModel.mapping);
                                ko.applyBindings(self._RecordModel, self._RecordPanel[0]);
                            }
                            self._RecordPanel.show();
                        }
                    }
                });
            };
            PageHandler.prototype.SaveButton_Clicked = function () {
                var self = this;
                VRS.pageHelper.showModalWaitAnimation(true);
                var data = ko.mapping.toJS(this._RecordModel);
                $.ajax({
                    url: 'SingleAircraftSave.json',
                    cache: false,
                    method: 'POST',
                    contentType: "application/json",
                    data: JSON.stringify(data),
                    complete: function () {
                        VRS.pageHelper.showModalWaitAnimation(false);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        self._SaveError.text(VRS.stringUtility.format(self._Translations.xhrFailedFormat, errorThrown));
                        self._SaveError.show();
                    },
                    success: function (data) {
                        if (data.Exception !== null) {
                            self._SaveError.text(VRS.stringUtility.format(self._Translations.serverReportedExceptionFormat, data.Exception));
                            self._SaveError.show();
                        }
                        else {
                            ko.mapping.fromJS(data.Aircraft, RecordModel.mapping, self._RecordModel);
                            self._SaveSuccess.show();
                            self._SaveError.hide();
                        }
                    }
                });
            };
            return PageHandler;
        })();
        Index.PageHandler = PageHandler;
        var SearchModel = (function () {
            function SearchModel() {
                this.icao = ko.observable();
            }
            return SearchModel;
        })();
        var RecordModel = (function () {
            function RecordModel() {
            }
            RecordModel.mapping = {
                'ICAOTypeCode': { create: function (options) { return ko.observable(options.data).extend({ uppercase: true }); } },
                'OperatorIcao': { create: function (options) { return ko.observable(options.data).extend({ uppercase: true }); } },
                'Registration': { create: function (options) { return ko.observable(options.data).extend({ uppercase: true }); } }
            };
            return RecordModel;
        })();
    })(Index = DatabaseEditor.Index || (DatabaseEditor.Index = {}));
})(DatabaseEditor || (DatabaseEditor = {}));
//# sourceMappingURL=index.js.map