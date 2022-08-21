var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var VatsimPluginOptions;
        (function (VatsimPluginOptions) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    this._ViewId = new WebAdmin.ViewId('VatsimPluginOptions', viewId);
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
                PageHandler.prototype.createAndEditGeofencedFeed = function () {
                    this._Model.GeofencedFeeds.pushFromModel({
                        FeedName: 'New Feed',
                        CentreOn: 0,
                        DistanceUnit: 0,
                        Height: 0.0,
                        Width: 0.0,
                        Latitude: 0.0,
                        Longitude: 0.0,
                        PilotCid: null,
                        AirportCode: null,
                        ID: null
                    });
                    var length = this._Model.GeofencedFeeds().length;
                    var newModel = this._Model.GeofencedFeeds()[length - 1];
                    this._Model.SelectedGeofencedFeed(newModel);
                    $('#edit-geofenced-feed').modal('show');
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
                                _this._Model.SaveSuccessful(data.Response.Outcome === 'Saved');
                                switch (data.Response.Outcome || '') {
                                    case 'Saved':
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved);
                                        break;
                                    case 'FailedValidation':
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Validation_Failed);
                                        break;
                                    case 'ConflictingUpdate':
                                        _this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update);
                                        break;
                                }
                            }
                            ko.viewmodel.updateFromModel(_this._Model, data.Response.Options);
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
                            optionsModel: JSON.stringify(viewModel)
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
                                        root.SelectedGeofencedFeed = ko.observable();
                                    },
                                    '{root}.GeofencedFeeds[i]': function (model) {
                                        model.FormattedCentreOn = ko.computed(function () { return _this._ViewId.describeEnum(model.CentreOn(), state.Response.CentreOnTypes); });
                                        model.FormattedDistanceUnit = ko.computed(function () { return _this._ViewId.describeEnum(model.DistanceUnit(), state.Response.DistanceUnitTypes); });
                                        model.ConditionalLatitude = ko.computed(function () { return model.CentreOn() != 0 ? null : model.Latitude(); });
                                        model.ConditionalLongitude = ko.computed(function () { return model.CentreOn() != 0 ? null : model.Longitude(); });
                                        model.ConditionalPilotCid = ko.computed(function () { return model.CentreOn() != 1 ? null : model.PilotCid(); });
                                        model.ConditionalAirportCode = ko.computed(function () { return model.CentreOn() != 2 ? null : (model.AirportCode() || '').toUpperCase(); });
                                        model.SelectRow = function (row) {
                                            _this._Model.SelectedGeofencedFeed(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            var index = _this._Model.GeofencedFeeds().indexOf(row);
                                            if (index !== -1) {
                                                _this._Model.GeofencedFeeds.splice(index, 1);
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
            VatsimPluginOptions.PageHandler = PageHandler;
        })(VatsimPluginOptions = WebAdmin.VatsimPluginOptions || (WebAdmin.VatsimPluginOptions = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=VatsimPluginOptions.js.map