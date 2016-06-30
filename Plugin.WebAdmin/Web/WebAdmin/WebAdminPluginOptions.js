var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var WebAdminPluginOptions;
        (function (WebAdminPluginOptions) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    this._AccessEditor = new WebAdmin.AccessEditor();
                    this._ViewId = new WebAdmin.ViewId('WebAdminPluginOptions', viewId);
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
                PageHandler.prototype.save = function () {
                    var _this = this;
                    this._Model.SaveAttempted(false);
                    var ajaxSettings = this.buildAjaxSettingsForSendConfiguration();
                    ajaxSettings.success = function (data) {
                        if (data.Exception) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, data.Exception));
                            _this._Model.SaveSuccessful(false);
                        }
                        else {
                            if (data.Response && data.Response.Outcome) {
                                _this._Model.SaveAttempted(true);
                                _this._Model.SaveSuccessful(data.Response.Outcome === "Saved");
                                switch (data.Response.Outcome || "") {
                                    case "Saved":
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved);
                                        break;
                                    case "FailedValidation":
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Validation_Failed);
                                        break;
                                    case "ConflictingUpdate":
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update);
                                        break;
                                }
                            }
                            ko.viewmodel.updateFromModel(_this._Model, data.Response.ViewModel);
                        }
                    };
                    this._ViewId.ajax('Save', ajaxSettings);
                };
                PageHandler.prototype.buildAjaxSettingsForSendConfiguration = function () {
                    var _this = this;
                    var viewModel = ko.viewmodel.toModel(this._Model);
                    var result = {
                        method: 'POST',
                        data: {
                            viewModel: JSON.stringify(viewModel)
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
                                arrayChildId: {},
                                extend: {
                                    '{root}': function (root) {
                                        root.SaveAttempted = ko.observable(false);
                                        root.SaveSuccessful = ko.observable(false);
                                        root.SavedMessage = ko.observable('');
                                        root.DefaultAccesses = state.Response.EnumDefaultAccesses;
                                    },
                                    '{root}.Access': function (model) {
                                        _this._AccessEditor.BuildAccessModel(model);
                                    },
                                    '{root}.Access.Addresses[i]': function (model) {
                                        _this._AccessEditor.BuildAccessCidrModel(model);
                                    }
                                }
                            });
                            ko.applyBindings(this._Model);
                        }
                    }
                };
                return PageHandler;
            }());
            WebAdminPluginOptions.PageHandler = PageHandler;
        })(WebAdminPluginOptions = WebAdmin.WebAdminPluginOptions || (WebAdmin.WebAdminPluginOptions = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=WebAdminPluginOptions.js.map