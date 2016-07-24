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
                    this._AccessEditor = new WebAdmin.AccessEditor();
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
                    }, false);
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
                    this._Model.SaveAttempted(false);
                    this.sendAndApplyConfiguration('RaiseSaveClicked', function (state) {
                        if (state.Response && state.Response.Outcome) {
                            _this._Model.SaveAttempted(true);
                            _this._Model.SaveSuccessful(state.Response.Outcome === "Saved");
                            switch (state.Response.Outcome || "") {
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
                    });
                };
                PageHandler.prototype.createAndEditMergedFeed = function () {
                    var _this = this;
                    this._Model.SelectedMergedFeed(null);
                    this.sendAndApplyConfiguration('CreateNewMergedFeed', function (state) {
                        _this._Model.MergedFeeds.unshiftFromModel(state.Response.NewMergedFeed);
                        _this._Model.SelectedMergedFeed(_this._Model.MergedFeeds()[0]);
                        $('#edit-merged-feed').modal('show');
                    });
                };
                PageHandler.prototype.createAndEditRebroadcastServer = function () {
                    var _this = this;
                    this._Model.SelectedRebroadcastServer(null);
                    this.sendAndApplyConfiguration('CreateNewRebroadcastServer', function (state) {
                        _this._Model.RebroadcastSettings.unshiftFromModel(state.Response.NewRebroadcastServer);
                        _this._Model.SelectedRebroadcastServer(_this._Model.RebroadcastSettings()[0]);
                        $('#edit-rebroadcast-server').modal('show');
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
                    this._Model.SelectedReceiverLocation(null);
                    this.sendAndApplyConfiguration('CreateNewReceiverLocation', function (state) {
                        _this._Model.ReceiverLocations.unshiftFromModel(state.Response.NewReceiverLocation);
                        _this._Model.SelectedReceiverLocation(_this._Model.ReceiverLocations()[0]);
                        $('#edit-receiver-location').modal('show');
                    });
                };
                PageHandler.prototype.createAndEditUser = function () {
                    var _this = this;
                    this._Model.SelectedUser(null);
                    this.sendAndApplyConfiguration('CreateNewUser', function (state) {
                        _this._Model.Users.unshiftFromModel(state.Response.NewUser);
                        _this._Model.SelectedUser(_this._Model.Users()[0]);
                        $('#edit-user').modal('show');
                    });
                };
                PageHandler.prototype.useIcaoRawDecodingSettings = function () {
                    this.sendAndApplyConfiguration('RaiseUseIcaoRawDecodingSettingsClicked', function (state) { });
                };
                PageHandler.prototype.useRecommendedRawDecodingSettings = function () {
                    this.sendAndApplyConfiguration('RaiseUseRecommendedRawDecodingSettingsClicked', function (state) { });
                };
                PageHandler.prototype.updateLocationsFromBaseStation = function () {
                    this.sendAndApplyConfiguration('RaiseReceiverLocationsFromBaseStationDatabaseClicked', function (state) { });
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
                PageHandler.prototype.feedlistChanged = function () {
                    this.synchroniseFeeds();
                    this.removeDeletedReceiversFromMergedFeeds();
                };
                PageHandler.prototype.synchroniseFeeds = function () {
                    var allFeeds = [];
                    $.each(this._Model.Receivers(), function (idx, feed) { return allFeeds.push({ UniqueId: feed.UniqueId, Name: feed.Name }); });
                    $.each(this._Model.MergedFeeds(), function (idx, feed) { return allFeeds.push({ UniqueId: feed.UniqueId, Name: feed.Name }); });
                    var feeds = this._Model.Feeds();
                    for (var i = feeds.length - 1; i >= 0; --i) {
                        var feed = feeds[i];
                        if (!VRS.arrayHelper.findFirst(allFeeds, function (r) { return r.UniqueId() == feed.UniqueId(); })) {
                            this._Model.Feeds.splice(i, 1);
                        }
                    }
                    var addList = VRS.arrayHelper.except(allFeeds, feeds, function (lhs, rhs) { return lhs.UniqueId() === rhs.UniqueId(); });
                    for (var i = 0; i < addList.length; ++i) {
                        this._Model.Feeds.push(addList[i]);
                    }
                };
                PageHandler.prototype.removeDeletedReceiversFromMergedFeeds = function () {
                    var receiverIds = [];
                    $.each(this._Model.Receivers(), function (idx, receiver) { return receiverIds.push(receiver.UniqueId()); });
                    $.each(this._Model.MergedFeeds(), function (idx, mergedFeed) {
                        for (var i = mergedFeed.ReceiverIds().length; i >= 0; --i) {
                            var receiverId = mergedFeed.ReceiverIds()[i];
                            if (receiverIds.indexOf(receiverId) === -1) {
                                mergedFeed.ReceiverIds.splice(i, 1);
                                var flagsIndex = VRS.arrayHelper.indexOfMatch(mergedFeed.ReceiverFlags(), function (r) { return r.UniqueId() === receiverId; });
                                if (flagsIndex !== -1) {
                                    mergedFeed.ReceiverFlags.splice(flagsIndex, 1);
                                }
                            }
                        }
                    });
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
                                    '{root}.MergedFeeds': 'UniqueId',
                                    '{root}.RebroadcastSettings': 'UniqueId',
                                    '{root}.RebroadcastSettings[i].Access.Addresses': 'Cidr',
                                    '{root}.ReceiverLocations': 'UniqueId',
                                    '{root}.Receivers': 'UniqueId',
                                    '{root}.Receivers[i].Access.Addresses': 'Cidr',
                                    '{root}.Users': 'UniqueId'
                                },
                                extend: {
                                    '{root}': function (root) {
                                        root.SaveAttempted = ko.observable(false);
                                        root.SaveSuccessful = ko.observable(false);
                                        root.SavedMessage = ko.observable("");
                                        root.TestConnectionOutcome = ko.observable(null);
                                        root.CurrentUserName = ko.observable(state.Response.CurrentUserName);
                                        root.SelectedMergedFeed = ko.observable(null);
                                        root.SelectedRebroadcastServer = ko.observable(null);
                                        root.SelectedReceiver = ko.observable(null);
                                        root.SelectedReceiverLocation = ko.observable(null);
                                        root.SelectedUser = ko.observable(null);
                                        root.ComPortNames = state.Response.ComPortNames;
                                        root.VoiceNames = state.Response.VoiceNames;
                                        root.ConnectionTypes = state.Response.ConnectionTypes;
                                        root.DataSources = state.Response.DataSources;
                                        root.DefaultAccesses = state.Response.DefaultAccesses;
                                        root.DistanceUnits = state.Response.DistanceUnits;
                                        root.Handshakes = state.Response.Handshakes;
                                        root.HeightUnits = state.Response.HeightUnits;
                                        root.Parities = state.Response.Parities;
                                        root.ProxyTypes = state.Response.ProxyTypes;
                                        root.RebroadcastFormats = state.Response.RebroadcastFormats;
                                        root.ReceiverUsages = state.Response.ReceiverUsages;
                                        root.SpeedUnits = state.Response.SpeedUnits;
                                        root.StopBits = state.Response.StopBits;
                                    },
                                    '{root}.AudioSettings': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.SetDefaultVoice = function () {
                                            model.VoiceName(null);
                                        };
                                    },
                                    '{root}.BaseStationSettings': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model, function (name, value) {
                                            return value !== model.AutoSavePolarPlotsMinutesValidation &&
                                                value !== model.DisplayTimeoutSecondsValidation &&
                                                value !== model.TrackingTimeoutSecondsValidation &&
                                                value !== model.SatcomDisplayTimeoutMinutesValidation &&
                                                value !== model.SatcomTrackingTimeoutMinutesValidation;
                                        }));
                                    },
                                    '{root}.GoogleMapSettings': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model, function (name, value) {
                                            return value !== model.ClosestAircraftReceiverIdValidation &&
                                                value !== model.FlightSimulatorXReceiverIdValidation &&
                                                value !== model.ShortTrailLengthSecondsValidation &&
                                                value !== model.WebSiteReceiverIdValidation;
                                        }));
                                    },
                                    '{root}.InternetClientSettingsModel': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                    },
                                    '{root}.MergedFeeds[i]': function (model) {
                                        model.FormattedReceiversCount = ko.computed(function () { return VRS.stringUtility.formatNumber(model.ReceiverIds().length, 'N0'); });
                                        model.FormattedIcaoTimeout = ko.computed(function () { return VRS.stringUtility.formatNumber(model.IcaoTimeout() / 1000, 'N2'); });
                                        model.FormattedIgnoreModeS = ko.computed(function () { return model.IgnoreAircraftWithNoPosition() ? VRS.$$.Yes : VRS.$$.No; });
                                        model.FormattedHidden = ko.computed(function () { return model.ReceiverUsage() !== 0 ? VRS.$$.Yes : VRS.$$.No; });
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.HideFromWebSite = ko.pureComputed({
                                            read: function () {
                                                return model.ReceiverUsage() != 0;
                                            },
                                            write: function (value) {
                                                model.ReceiverUsage(value ? 1 : 0);
                                            },
                                            owner: _this
                                        });
                                        model.IcaoTimeoutSeconds = ko.pureComputed({
                                            read: function () {
                                                return model.IcaoTimeout() / 1000;
                                            },
                                            write: function (value) {
                                                model.IcaoTimeout(value * 1000);
                                            },
                                            owner: _this
                                        });
                                        model.SelectRow = function (row) {
                                            _this._Model.SelectedMergedFeed(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            var index = VRS.arrayHelper.indexOfMatch(_this._Model.MergedFeeds(), function (r) { return r.UniqueId == row.UniqueId; });
                                            _this._Model.MergedFeeds.splice(index, 1);
                                        };
                                        model.IncludeReceiver = function (receiver) {
                                            return ko.pureComputed({
                                                read: function () {
                                                    return model.ReceiverIds().indexOf(receiver.UniqueId()) !== -1;
                                                },
                                                write: function (value) {
                                                    var index = model.ReceiverIds().indexOf(receiver.UniqueId());
                                                    if (value && index === -1) {
                                                        model.ReceiverIds.pushFromModel(receiver.UniqueId());
                                                    }
                                                    else if (!value && index !== -1) {
                                                        model.ReceiverIds.removeAtToModel(index, receiver.UniqueId());
                                                    }
                                                },
                                                owner: _this
                                            });
                                        };
                                        model.ReceiverIsMlat = function (receiver) {
                                            return ko.pureComputed({
                                                read: function () {
                                                    var flags = VRS.arrayHelper.findFirst(model.ReceiverFlags(), function (r) { return r.UniqueId() == receiver.UniqueId(); });
                                                    return flags && flags.IsMlatFeed();
                                                },
                                                write: function (value) {
                                                    var index = VRS.arrayHelper.indexOfMatch(model.ReceiverFlags(), function (r) { return r.UniqueId() == receiver.UniqueId(); });
                                                    if (index === -1) {
                                                        var blank = {
                                                            UniqueId: receiver.UniqueId(),
                                                            IsMlatFeed: false,
                                                        };
                                                        model.ReceiverFlags.unshiftFromModel(blank);
                                                        index = 0;
                                                    }
                                                    var flags = model.ReceiverFlags()[index];
                                                    flags.IsMlatFeed(value);
                                                },
                                                owner: _this
                                            });
                                        };
                                    },
                                    '{root}.RawDecodingSettings': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                    },
                                    '{root}.RebroadcastSettings[i]': function (model) {
                                        model.FormattedAccess = ko.computed(function () { return _this._ViewId.describeEnum(model.Access.DefaultAccess(), state.Response.DefaultAccesses); });
                                        model.FormattedAddress = ko.computed(function () { return VRS.stringUtility.format('{0}:{1}', (model.TransmitAddress() ? model.TransmitAddress() : ':'), model.Port()); });
                                        model.FormatDescription = ko.computed(function () {
                                            var rebroadcastFormat = VRS.arrayHelper.findFirst(state.Response.RebroadcastFormats, function (r) { return r.UniqueId === model.Format(); });
                                            return rebroadcastFormat ? rebroadcastFormat.ShortName : VRS.Server.$$.Unknown;
                                        });
                                        model.Feed = ko.pureComputed({
                                            read: function () {
                                                var feedId = model.ReceiverId();
                                                var feed = feedId ? VRS.arrayHelper.findFirst(_this._Model.Feeds(), function (r) { return r.UniqueId() === feedId; }) : null;
                                                return feed;
                                            },
                                            write: function (value) {
                                                model.ReceiverId(value ? value.UniqueId() : 0);
                                            },
                                            owner: _this
                                        });
                                        model.SendIntervalSeconds = ko.pureComputed({
                                            read: function () {
                                                return Math.floor(model.SendIntervalMilliseconds() / 1000);
                                            },
                                            write: function (value) {
                                                model.SendIntervalMilliseconds(value * 1000);
                                            },
                                            owner: _this
                                        });
                                        model.IdleTimeoutSeconds = ko.pureComputed({
                                            read: function () {
                                                return Math.floor(model.IdleTimeoutMilliseconds() / 1000);
                                            },
                                            write: function (value) {
                                                model.IdleTimeoutMilliseconds(value * 1000);
                                            },
                                            owner: _this
                                        });
                                        model.IsWholeListFeed = ko.computed(function () { return model.Format() === 'AircraftListJson'; });
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.SelectRow = function (row) {
                                            _this._Model.SelectedRebroadcastServer(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            var index = VRS.arrayHelper.indexOfMatch(_this._Model.RebroadcastSettings(), function (r) { return r.UniqueId == row.UniqueId; });
                                            _this._Model.RebroadcastSettings.splice(index, 1);
                                        };
                                    },
                                    '{root}.RebroadcastSettings[i].Access': function (model) {
                                        _this._AccessEditor.BuildAccessModel(model);
                                    },
                                    '{root}.RebroadcastSettings[i].Access.Addresses[i]': function (model) {
                                        _this._AccessEditor.BuildAccessCidrModel(model);
                                    },
                                    '{root}.Receivers[i]': function (model) {
                                        model.FormattedConnectionType = ko.computed(function () { return _this._ViewId.describeEnum(model.ConnectionType(), state.Response.ConnectionTypes); });
                                        model.FormattedHandshake = ko.computed(function () { return _this._ViewId.describeEnum(model.Handshake(), state.Response.Handshakes); });
                                        model.FormattedParity = ko.computed(function () { return _this._ViewId.describeEnum(model.Parity(), state.Response.Parities); });
                                        model.FormattedReceiverUsage = ko.computed(function () { return _this._ViewId.describeEnum(model.ReceiverUsage(), state.Response.ReceiverUsages); });
                                        model.FormattedStopBits = ko.computed(function () { return _this._ViewId.describeEnum(model.StopBits(), state.Response.StopBits); });
                                        model.FormattedDataSource = ko.computed(function () {
                                            var receiverFormat = VRS.arrayHelper.findFirst(state.Response.DataSources, function (r) { return r.UniqueId === model.DataSource(); });
                                            return receiverFormat ? receiverFormat.ShortName : VRS.Server.$$.Unknown;
                                        });
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
                                                var receiverLocationId = model.ReceiverLocationId();
                                                var receiverLocation = receiverLocationId ? VRS.arrayHelper.findFirst(_this._Model.ReceiverLocations(), function (r) { return r.UniqueId() === receiverLocationId; }) : null;
                                                return receiverLocation;
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
                                    '{root}.Receivers[i].Access': function (model) {
                                        _this._AccessEditor.BuildAccessModel(model);
                                    },
                                    '{root}.Receivers[i].Access.Addresses[i]': function (model) {
                                        _this._AccessEditor.BuildAccessCidrModel(model);
                                    },
                                    '{root}.ReceiverLocations[i]': function (model) {
                                        model.FormattedLatitude = ko.computed(function () { return VRS.stringUtility.formatNumber(model.Latitude(), 'N6'); });
                                        model.FormattedLongitude = ko.computed(function () { return VRS.stringUtility.formatNumber(model.Longitude(), 'N6'); });
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.SelectRow = function (row) {
                                            _this._Model.SelectedReceiverLocation(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            var index = VRS.arrayHelper.indexOfMatch(_this._Model.ReceiverLocations(), function (r) { return r.UniqueId == row.UniqueId; });
                                            _this._Model.ReceiverLocations.splice(index, 1);
                                        };
                                    },
                                    '{root}.Users[i]': function (model) {
                                        model.IsCurrentUser = ko.pureComputed(function () { return VRS.stringUtility.equals(_this._Model.CurrentUserName(), model.LoginName(), true); });
                                        model.IsAdminUser = ko.pureComputed({
                                            read: function () {
                                                return VRS.arrayHelper.indexOf(_this._Model.WebServerSettings.AdministratorUserIds(), model.UniqueId()) !== -1;
                                            },
                                            write: function (value) {
                                                var index = VRS.arrayHelper.indexOf(_this._Model.WebServerSettings.AdministratorUserIds(), model.UniqueId());
                                                if (value && index === -1) {
                                                    _this._Model.WebServerSettings.AdministratorUserIds.push(model.UniqueId());
                                                }
                                                else if (!value && index !== -1) {
                                                    _this._Model.WebServerSettings.AdministratorUserIds.splice(index, 1);
                                                }
                                            },
                                            owner: _this
                                        });
                                        model.IsWebSiteUser = ko.pureComputed({
                                            read: function () {
                                                return VRS.arrayHelper.indexOf(_this._Model.WebServerSettings.BasicAuthenticationUserIds(), model.UniqueId()) !== -1;
                                            },
                                            write: function (value) {
                                                var index = VRS.arrayHelper.indexOf(_this._Model.WebServerSettings.BasicAuthenticationUserIds(), model.UniqueId());
                                                if (value && index === -1) {
                                                    _this._Model.WebServerSettings.BasicAuthenticationUserIds.push(model.UniqueId());
                                                }
                                                else if (!value && index !== -1) {
                                                    _this._Model.WebServerSettings.BasicAuthenticationUserIds.splice(index, 1);
                                                }
                                            },
                                            owner: _this
                                        });
                                        model.LoginNameAndCurrentUser = ko.pureComputed({
                                            read: function () { return model.LoginName(); },
                                            write: function (value) {
                                                if (model.IsCurrentUser()) {
                                                    _this._Model.CurrentUserName(value);
                                                }
                                                model.LoginName(value);
                                            },
                                            owner: _this
                                        });
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                        model.SelectRow = function (row) {
                                            _this._Model.SelectedUser(row);
                                        };
                                        model.DeleteRow = function (row) {
                                            model.IsWebSiteUser(false);
                                            model.IsAdminUser(false);
                                            var index = VRS.arrayHelper.indexOfMatch(_this._Model.Users(), function (r) { return r.UniqueId == row.UniqueId; });
                                            _this._Model.Users.splice(index, 1);
                                        };
                                    },
                                    '{root}.WebServerSettings': function (model) {
                                        model.WrapUpValidation = _this._ViewId.createWrapupValidation(_this._ViewId.findValidationProperties(model));
                                    },
                                }
                            });
                            this._Model.GeneralWrapUpValidation = this._ViewId.createWrapupValidation([
                                this._Model.VersionCheckSettings.CheckPeriodDaysValidation,
                                this._Model.BaseStationSettings.DisplayTimeoutSecondsValidation,
                                this._Model.BaseStationSettings.TrackingTimeoutSecondsValidation,
                                this._Model.BaseStationSettings.SatcomDisplayTimeoutMinutesValidation,
                                this._Model.BaseStationSettings.SatcomTrackingTimeoutMinutesValidation,
                                this._Model.GoogleMapSettings.ShortTrailLengthSecondsValidation,
                                this._Model.BaseStationSettings.AutoSavePolarPlotsMinutesValidation
                            ]);
                            this._Model.MergedFeedWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.MergedFeeds, function (r) { return r.WrapUpValidation; });
                            this._Model.RebroadcastServerWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.RebroadcastSettings, function (r) { return r.WrapUpValidation; });
                            this._Model.ReceiverWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.Receivers, function (r) { return r.WrapUpValidation; }, this._Model.GoogleMapSettings.WebSiteReceiverIdValidation, this._Model.GoogleMapSettings.ClosestAircraftReceiverIdValidation, this._Model.GoogleMapSettings.FlightSimulatorXReceiverIdValidation);
                            this._Model.ReceiverLocationWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.ReceiverLocations, function (r) { return r.WrapUpValidation; });
                            this._Model.UserWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.Users, function (r) { return r.WrapUpValidation; });
                            this._Model.Feeds = ko.observableArray([]);
                            this._Model.Receivers.subscribe(this.feedlistChanged, this);
                            this._Model.MergedFeeds.subscribe(this.feedlistChanged, this);
                            this.synchroniseFeeds();
                            var webServerAndInternetClientValidationFields = this._ViewId.findValidationProperties(this._Model.WebServerSettings);
                            this._ViewId.findValidationProperties(this._Model.InternetClientSettings, null, webServerAndInternetClientValidationFields);
                            this._Model.WebServerSettings.WebServerAndInternetClientWrapUpValidation = this._ViewId.createWrapupValidation(webServerAndInternetClientValidationFields);
                            ko.applyBindings(this._Model);
                        }
                    }
                };
                return PageHandler;
            }());
            Settings.PageHandler = PageHandler;
        })(Settings = WebAdmin.Settings || (WebAdmin.Settings = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=Settings.js.map