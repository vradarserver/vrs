var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Settings;
        (function (Settings) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    var _this = this;
                    this._SaveAttempted = false;
                    this._ViewId = new WebAdmin.ViewId('Settings', viewId);
                    $('#edit-receiver').on('hidden.bs.modal', function () {
                        if (_this._Model && _this._Model.TestConnectionOutcome) {
                            _this._Model.TestConnectionOutcome(null);
                        }
                    });
                    this.refreshState();
                }
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
                    });
                };
                PageHandler.prototype.sendAndApplyConfiguration = function (methodName, success, applyConfigurationFirst) {
                    var _this = this;
                    if (applyConfigurationFirst === void 0) { applyConfigurationFirst = true; }
                    var settings = this.buildAjaxSettingsForSendConfiguration();
                    settings.success = function (data) {
                        if (applyConfigurationFirst) {
                            _this.applyState(data);
                            success(data);
                        }
                        else {
                            success(data);
                            _this.applyState(data);
                        }
                    };
                    this._ViewId.ajax(methodName, settings);
                };
                PageHandler.prototype.buildAjaxSettingsForSendConfiguration = function () {
                    var _this = this;
                    var configuration = ko.viewmodel.toModel(this._Model);
                    var result = {
                        method: 'POST',
                        data: {
                            configurationModel: JSON.stringify(configuration)
                        },
                        dataType: 'json',
                        error: function (jqXHR, textStatus, errorThrown) {
                            _this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                        }
                    };
                    return result;
                };
                PageHandler.prototype.save = function () {
                    var _this = this;
                    this._Model.saveAttempted(false);
                    this.sendAndApplyConfiguration('RaiseSaveClicked', function (state) {
                        if (state.Response && state.Response.Outcome) {
                            _this._Model.saveAttempted(true);
                            _this._Model.saveSuccessful(state.Response.Outcome === "Saved");
                            switch (state.Response.Outcome || "") {
                                case "Saved":
                                    _this._Model.savedMessage(VRS.WebAdmin.$$.WA_Saved);
                                    break;
                                case "FailedValidation":
                                    _this._Model.savedMessage(VRS.WebAdmin.$$.WA_Validation_Failed);
                                    break;
                                case "ConflictingUpdate":
                                    _this._Model.savedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update);
                                    break;
                            }
                        }
                    });
                };
                PageHandler.prototype.createAndEditReceiver = function () {
                    var _this = this;
                    this._Model.SelectedReceiver(null);
                    this.sendAndApplyConfiguration('CreateNewReceiver', function (state) {
                        _this._Model.Receivers.unshiftFromModel(state.Response.NewReceiver);
                        _this._Model.SelectedReceiver(_this._Model.Receivers()[0]);
                        $('#edit-receiver').modal('show');
                    });
                };
                PageHandler.prototype.testConnection = function (receiver) {
                    var _this = this;
                    this._Model.TestConnectionOutcome(null);
                    var settings = this.buildAjaxSettingsForSendConfiguration();
                    settings.data.receiverId = receiver.UniqueId();
                    settings.success = function (outcome) {
                        _this._Model.TestConnectionOutcome(outcome.Response);
                    };
                    this._ViewId.ajax('TestConnection', settings);
                };
                PageHandler.prototype.createAndEditReceiverLocation = function () {
                    var _this = this;
                    this._Model.selectedReceiverLocation(null);
                    this.sendAndApplyConfiguration('CreateNewReceiverLocation', function (state) {
                        _this._Model.ReceiverLocations.unshiftFromModel(state.Response.NewReceiverLocation);
                        _this._Model.selectedReceiverLocation(_this._Model.ReceiverLocations()[0]);
                        $('#edit-receiver-location').modal('show');
                    });
                };
                PageHandler.prototype.updateLocationsFromBaseStation = function () {
                    this.sendAndApplyConfiguration('RaiseReceiverLocationsFromBaseStationDatabaseClicked', function (state) {
                    });
                };
                PageHandler.prototype.showFailureMessage = function (message) {
                    var alert = $('#failure-message');
                    if (message && message.length) {
                        alert.text(message || '').show();
                    }
                    else {
                        alert.hide();
                    }
                };
                PageHandler.prototype.applyState = function (state) {
                    var _this = this;
                    if (state.Exception) {
                        this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, state.Exception));
                    }
                    else {
                        this.showFailureMessage(null);
                        if (this._Model) {
                            ko.viewmodel.updateFromModel(this._Model, state.Response.Configuration);
                        }
                        else {
                            this._Model = ko.viewmodel.fromModel(state.Response.Configuration, {
                                arrayChildId: {
                                    '{root}.ReceiverLocations': 'UniqueId',
                                    '{root}.Receivers': 'UniqueId'
                                },
                                extend: {
                                    '{root}': function (root) {
                                        root.saveAttempted = ko.observable(false);
                                        root.saveSuccessful = ko.observable(false);
                                        root.savedMessage = ko.observable("");
                                        root.ConnectionTypes = state.Response.ConnectionTypes;
                                        root.DataSources = state.Response.DataSources;
                                        root.DefaultAccesses = state.Response.DefaultAccesses;
                                        root.Handshakes = state.Response.Handshakes;
                                        root.Parities = state.Response.Parities;
                                        root.ReceiverUsages = state.Response.ReceiverUsages;
                                        root.StopBits = state.Response.StopBits;
                                        root.TestConnectionOutcome = ko.observable(null);
                                        root.SelectedReceiver = ko.observable(null);
                                        root.selectedReceiverLocation = ko.observable(null);
                                    },
                                    '{root}.BaseStationSettingsModel': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                    },
                                    '{root}.Receivers[i]': function (model) {
                                        model.FormattedConnectionType = ko.computed(function () { return _this._ViewId.describeEnum(model.ConnectionType(), state.Response.ConnectionTypes); });
                                        model.FormattedDataSource = ko.computed(function () { return _this._ViewId.describeEnum(model.DataSource(), state.Response.DataSources); });
                                        model.FormattedHandshake = ko.computed(function () { return _this._ViewId.describeEnum(model.Handshake(), state.Response.Handshakes); });
                                        model.FormattedParity = ko.computed(function () { return _this._ViewId.describeEnum(model.Parity(), state.Response.Parities); });
                                        model.FormattedReceiverUsage = ko.computed(function () { return _this._ViewId.describeEnum(model.ReceiverUsage(), state.Response.ReceiverUsages); });
                                        model.FormattedStopBits = ko.computed(function () { return _this._ViewId.describeEnum(model.StopBits(), state.Response.StopBits); });
                                        model.ConnectionParameters = ko.computed(function () {
                                            var connectionParameters = '';
                                            switch (model.ConnectionType()) {
                                                case 1:
                                                    connectionParameters = VRS.stringUtility.format('{0}, {1}, {2}/{3}, {4}, {5}, "{6}", "{7}"', model.ComPort(), model.BaudRate(), model.DataBits(), _this._ViewId.describeEnum(model.StopBits(), state.Response.StopBits), _this._ViewId.describeEnum(model.Parity(), state.Response.Parities), _this._ViewId.describeEnum(model.Handshake(), state.Response.Handshakes), model.StartupText(), model.ShutdownText());
                                                    break;
                                                case 0:
                                                    connectionParameters = VRS.stringUtility.format("{0}:{1}", model.Address(), model.Port());
                                                    break;
                                            }
                                            return connectionParameters;
                                        });
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.IdleTimeoutSeconds = ko.pureComputed({
                                            read: function () {
                                                return Math.floor(model.IdleTimeoutMilliseconds() / 1000);
                                            },
                                            write: function (value) {
                                                model.IdleTimeoutMilliseconds(value * 1000);
                                            },
                                            owner: _this
                                        });
                                        model.Location = ko.pureComputed({
                                            read: function () {
                                                var receiverId = model.ReceiverLocationId();
                                                var receiver = receiverId ? VRS.arrayHelper.findFirst(_this._Model.ReceiverLocations(), function (r) { return r.UniqueId() === receiverId; }) : null;
                                                return receiver;
                                            },
                                            write: function (value) {
                                                model.ReceiverLocationId(value ? value.UniqueId() : 0);
                                            },
                                            owner: _this
                                        });
                                        model.SelectRow = function (row) {
                                            _this._Model.SelectedReceiver(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            var index = VRS.arrayHelper.indexOfMatch(_this._Model.Receivers(), function (r) { return r.UniqueId == row.UniqueId; });
                                            _this._Model.Receivers.splice(index, 1);
                                        };
                                        model.ResetLocation = function (row) {
                                            row.ReceiverLocationId(0);
                                        };
                                        model.TestConnection = function (row) {
                                            _this.testConnection(row);
                                        };
                                    },
                                    '{root}.ReceiverLocations[i]': function (model) {
                                        model.FormattedLatitude = ko.computed(function () { return VRS.stringUtility.formatNumber(model.Latitude(), 'N6'); });
                                        model.FormattedLongitude = ko.computed(function () { return VRS.stringUtility.formatNumber(model.Longitude(), 'N6'); });
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.SelectRow = function (row) {
                                            _this._Model.selectedReceiverLocation(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            var index = VRS.arrayHelper.indexOfMatch(_this._Model.ReceiverLocations(), function (r) { return r.UniqueId == row.UniqueId; });
                                            _this._Model.ReceiverLocations.splice(index, 1);
                                        };
                                    }
                                }
                            });
                            this._Model.ReceiverWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.Receivers, function (r) { return r.WrapUpValidation; });
                            this._Model.ReceiverLocationWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.ReceiverLocations, function (r) { return r.WrapUpValidation; });
                            ko.applyBindings(this._Model);
                        }
                    }
                };
                return PageHandler;
            })();
            Settings.PageHandler = PageHandler;
        })(Settings = WebAdmin.Settings || (WebAdmin.Settings = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=Settings.js.map