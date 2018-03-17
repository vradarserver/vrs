var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var CustomContentPluginOptions;
        (function (CustomContentPluginOptions) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    this._ViewId = new WebAdmin.ViewId('CustomContentPluginOptions', viewId);
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
                PageHandler.prototype.createAndEditInjectSettings = function () {
                    this._Model.InjectSettings.pushFromModel({
                        Enabled: true,
                        PathAndFile: '/desktop.html',
                        PathAndFileValidation: {
                            IsError: false,
                            IsValid: true,
                            IsWarning: false,
                            Message: ''
                        },
                        InjectionLocation: 0 /* Head */,
                        Start: false,
                        File: '',
                        FileValidation: {
                            IsError: false,
                            IsValid: true,
                            IsWarning: false,
                            Message: ''
                        }
                    });
                    var length = this._Model.InjectSettings().length;
                    var newModel = this._Model.InjectSettings()[length - 1];
                    this._Model.SelectedInjectSettings(newModel);
                    $('#edit-inject-settings').modal('show');
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
                                        root.SelectedInjectSettings = ko.observable();
                                    },
                                    '{root}.InjectSettings[i]': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.FormattedStart = ko.computed(function () {
                                            return model.Start() ? VRS.CustomContentPlugin.$$.Start : VRS.CustomContentPlugin.$$.End;
                                        });
                                        model.StartAsStringValue = ko.pureComputed({
                                            read: function () {
                                                return model.Start() ? "start" : "end";
                                            },
                                            write: function (value) {
                                                model.Start(value === "start");
                                            },
                                            owner: _this
                                        });
                                        model.FormattedInjectionLocation = ko.computed(function () {
                                            switch (model.InjectionLocation()) {
                                                case 1 /* Body */:
                                                    return VRS.CustomContentPlugin.$$.Body;
                                                case 0 /* Head */:
                                                    return VRS.CustomContentPlugin.$$.Head;
                                                default:
                                                    return VRS.Server.$$.Unknown;
                                            }
                                        });
                                        model.SelectRow = function (row) {
                                            _this._Model.SelectedInjectSettings(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            var index = _this._Model.InjectSettings().indexOf(row);
                                            if (index !== -1) {
                                                _this._Model.InjectSettings.splice(index, 1);
                                            }
                                        };
                                    }
                                }
                            });
                            ko.applyBindings(this._Model);
                        }
                    }
                };
                return PageHandler;
            }());
            CustomContentPluginOptions.PageHandler = PageHandler;
        })(CustomContentPluginOptions = WebAdmin.CustomContentPluginOptions || (WebAdmin.CustomContentPluginOptions = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=CustomContentPluginOptions.js.map