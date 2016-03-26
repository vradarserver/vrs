/// <reference path="../../typings/webadmin.d.ts" />
/// <reference path="../../typings/DatabaseWriterPluginModels.d.ts" />
/// <reference path="../../typings/translations-pluginstrings.d.ts" />
var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var DatabaseWriterPluginOptions;
        (function (DatabaseWriterPluginOptions) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    this._ViewId = new WebAdmin.ViewId('DatabaseWriterPluginOptions', viewId);
                    this.refreshState();
                }
                PageHandler.prototype.showFailureMessage = function (message) {
                    var alert = $('#failure-message');
                    if (message && message.length) {
                        alert.text(message || '').show();
                    }
                    else {
                        alert.hide();
                    }
                };
                PageHandler.prototype.refreshState = function () {
                    var _this = this;
                    this.showFailureMessage(null);
                    this._ViewId.ajax('GetState', {
                        success: function (data) {
                            _this.applyState(data);
                        },
                        error: function () {
                            setTimeout(function () { return _this.refreshState(); }, 5000);
                        }
                    }, false);
                };
                PageHandler.prototype.useDefaultFileName = function () {
                    var _this = this;
                    var settings = this.buildAjaxSettingsForSendModel();
                    settings.success = function (viewModel) {
                        _this.applyState(viewModel);
                    };
                    this._ViewId.ajax('UseDefaultFileName', settings);
                };
                PageHandler.prototype.createDatabase = function () {
                    var _this = this;
                    this._Model.CreateDatabaseOutcomeMessage('');
                    this._Model.CreateDatabaseOutcomeTitle('');
                    var settings = this.buildAjaxSettingsForSendModel();
                    settings.success = function (outcome) {
                        if (outcome.Exception) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, outcome.Exception));
                        }
                        else {
                            _this.showFailureMessage(null);
                            _this._Model.CreateDatabaseOutcomeMessage(outcome.Response.Message);
                            _this._Model.CreateDatabaseOutcomeTitle(outcome.Response.Title);
                            ko.viewmodel.updateFromModel(_this._Model, outcome.Response.ViewModel);
                            $('#create-database-outcome').modal('show');
                        }
                    };
                    this._ViewId.ajax('CreateDatabase', settings);
                };
                PageHandler.prototype.save = function () {
                    var _this = this;
                    this._Model.SaveAttempted(false);
                    this._Model.SavedMessage('');
                    var settings = this.buildAjaxSettingsForSendModel();
                    settings.success = function (outcome) {
                        if (outcome.Exception) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, outcome.Exception));
                        }
                        else {
                            _this.showFailureMessage(null);
                            _this._Model.SaveAttempted(true);
                            switch (outcome.Response.Outcome) {
                                case 'Saved':
                                    _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved);
                                    break;
                                case 'ConflictingUpdate':
                                    _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update);
                                    break;
                                default:
                                    _this._Model.SavedMessage(VRS.stringUtility.format('Unexpected response "{0}"', outcome.Response.Outcome));
                                    break;
                            }
                            ko.viewmodel.updateFromModel(_this._Model, outcome.Response.ViewModel);
                        }
                    };
                    this._ViewId.ajax('Save', settings);
                };
                PageHandler.prototype.buildAjaxSettingsForSendModel = function () {
                    var _this = this;
                    var configuration = ko.viewmodel.toModel(this._Model);
                    var result = {
                        method: 'POST',
                        data: {
                            viewModel: JSON.stringify(configuration)
                        },
                        dataType: 'json',
                        error: function (jqXHR, textStatus, errorThrown) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                        }
                    };
                    return result;
                };
                PageHandler.prototype.applyState = function (state) {
                    var _this = this;
                    if (state.Exception) {
                        this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, state.Exception));
                    }
                    else {
                        this.showFailureMessage(null);
                        if (this._Model) {
                            ko.viewmodel.updateFromModel(this._Model, state.Response);
                        }
                        else {
                            this._Model = ko.viewmodel.fromModel(state.Response, {
                                arrayChildId: {
                                    CombinedFeeds: 'UniqueId'
                                },
                                extend: {
                                    '{root}': function (root) {
                                        root.SaveAttempted = ko.observable(false);
                                        root.SavedMessage = ko.observable('');
                                        root.CreateDatabaseOutcomeTitle = ko.observable('');
                                        root.CreateDatabaseOutcomeMessage = ko.observable('');
                                        root.OverwriteDetailsMessage = ko.computed(function () {
                                            return root.RefreshOutOfDateAircraft() ? VRS.DatabaseWriterPlugin.$$.WriteOnlineLookupsNoticeAllAircraft
                                                : VRS.DatabaseWriterPlugin.$$.WriteOnlineLookupsNoticeJustNew;
                                        });
                                        root.OnlyUpdateDatabasesCreatedByPlugin = ko.computed({
                                            read: function () { return !root.AllowUpdateOfOtherDatabases(); },
                                            write: function (value) { return root.AllowUpdateOfOtherDatabases(!value); },
                                            owner: _this
                                        });
                                        root.SelectedFeed = ko.computed({
                                            read: function () { return VRS.arrayHelper.findFirst(root.CombinedFeeds(), function (r) { return r.UniqueId() === root.ReceiverId(); }); },
                                            write: function (value) { return root.ReceiverId(value ? value.UniqueId() : -1); },
                                            owner: _this
                                        });
                                    }
                                }
                            });
                            ko.applyBindings(this._Model);
                        }
                    }
                };
                return PageHandler;
            })();
            DatabaseWriterPluginOptions.PageHandler = PageHandler;
        })(DatabaseWriterPluginOptions = WebAdmin.DatabaseWriterPluginOptions || (WebAdmin.DatabaseWriterPluginOptions = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
