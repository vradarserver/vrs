namespace VRS.WebAdmin.Settings
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Settings;

    interface Model extends ViewJson.IConfigurationModel_KO
    {
        SaveAttempted?:                     KnockoutObservable<boolean>;
        SaveSuccessful?:                    KnockoutObservable<boolean>;
        SavedMessage?:                      KnockoutObservable<string>;
        TestConnectionOutcome:              KnockoutObservable<ViewJson.ITestConnectionOutcomeModel>;

        CurrentUserName?:                   KnockoutObservable<string>;
        SelectedMergedFeed?:                KnockoutObservable<MergedFeedModel>;
        SelectedRebroadcastServer?:         KnockoutObservable<RebroadcastServerModel>;
        SelectedReceiver?:                  KnockoutObservable<ReceiverModel>;
        SelectedReceiverLocation?:          KnockoutObservable<ReceiverLocationModel>;
        SelectedUser?:                      KnockoutObservable<UserModel>;

        GeneralWrapUpValidation?:           IValidation_KC;
        MergedFeedWrapUpValidation?:        IValidation_KC;
        RebroadcastServerWrapUpValidation?: IValidation_KC;
        ReceiverWrapUpValidation?:          IValidation_KC;
        ReceiverLocationWrapUpValidation?:  IValidation_KC;
        UserWrapUpValidation?:              IValidation_KC;

        Feeds?:                             KnockoutObservableArray<FeedModel>;

        ComPortNames?:                      string[];
        VoiceNames?:                        string[];
        BaudRates?:                         number[];
        ConnectionTypes?:                   VirtualRadar.Interface.View.IEnumModel[];
        DataSources?:                       VirtualRadar.Interface.Listener.IReceiverFormatName[];
        DefaultAccesses?:                   VirtualRadar.Interface.View.IEnumModel[];
        DistanceUnits?:                     VirtualRadar.Interface.View.IEnumModel[];
        Handshakes?:                        VirtualRadar.Interface.View.IEnumModel[];
        HeightUnits?:                       VirtualRadar.Interface.View.IEnumModel[];
        Parities?:                          VirtualRadar.Interface.View.IEnumModel[];
        ProxyTypes?:                        VirtualRadar.Interface.View.IEnumModel[];
        RebroadcastFormats?:                VirtualRadar.Interface.Network.IRebroadcastFormatName[];
        ReceiverUsages?:                    VirtualRadar.Interface.View.IEnumModel[];
        SpeedUnits?:                        VirtualRadar.Interface.View.IEnumModel[];
        StopBits?:                          VirtualRadar.Interface.View.IEnumModel[];
    }

    interface AudioSettingsModel extends ViewJson.IAudioSettingsModel_KO
    {
        SetDefaultVoice?:   () => void;
        WrapUpValidation?:  IValidation_KC;
    }

    interface BaseStationSettingsModel extends ViewJson.IBaseStationSettingsModel_KO
    {
        WrapUpValidation?: IValidation_KC;
    }

    interface GoogleMapSettingsModel extends ViewJson.IGoogleMapSettingsModel_KO
    {
        WrapUpValidation?: IValidation_KC;
    }

    interface InternetClientSettingsModel extends ViewJson.IInternetClientSettingsModel_KO
    {
        WrapUpValidation?: IValidation_KC;
    }

    interface MergedFeedModel extends ViewJson.IMergedFeedModel_KO
    {
        FormattedReceiversCount?:   KnockoutComputed<string>;
        FormattedIcaoTimeout?:      KnockoutComputed<string>;
        FormattedIgnoreModeS?:      KnockoutComputed<string>;
        FormattedHidden?:           KnockoutComputed<string>;
        IcaoTimeoutSeconds?:        KnockoutComputed<number>;
        HideFromWebSite?:           KnockoutComputed<boolean>;
        WrapUpValidation?:          IValidation_KC;

        SelectRow?:                 (row: MergedFeedModel) => void;
        DeleteRow?:                 (row: MergedFeedModel) => void;
        IncludeReceiver?:           (receiver: ReceiverModel) => KnockoutComputed<boolean>;
        ReceiverIsMlat?:            (receiver: ReceiverModel) => KnockoutComputed<boolean>;
    }

    interface RawDecodingSettingsModel extends ViewJson.IRawDecodingSettingModel_KO
    {
        WrapUpValidation?: IValidation_KC;
    }

    interface RebroadcastServerModel extends ViewJson.IRebroadcastServerModel_KO
    {
        FormattedAccess?:           KnockoutComputed<string>;
        FormattedAddress?:          KnockoutComputed<string>;
        FormatDescription?:         KnockoutComputed<string>;
        Feed?:                      KnockoutComputed<FeedModel>;
        SendIntervalSeconds?:       KnockoutComputed<number>;
        WrapUpValidation?:          IValidation_KC;

        SelectRow?:                 (row: RebroadcastServerModel) => void;
        DeleteRow?:                 (row: RebroadcastServerModel) => void;
    }

    interface ReceiverModel extends ViewJson.IReceiverModel_KO
    {
        FormattedConnectionType?:   KnockoutComputed<string>;
        FormattedDataSource?:       KnockoutComputed<string>;
        FormattedHandshake?:        KnockoutComputed<string>;
        FormattedParity?:           KnockoutComputed<string>;
        FormattedReceiverUsage?:    KnockoutComputed<string>;
        FormattedStopBits?:         KnockoutComputed<string>;
        ConnectionParameters?:      KnockoutComputed<string>;
        Location?:                  KnockoutComputed<ReceiverLocationModel>;
        IdleTimeoutSeconds?:        KnockoutComputed<number>;
        WrapUpValidation?:          IValidation_KC;

        SelectRow?:                 (row: ReceiverModel) => void;
        DeleteRow?:                 (row: ReceiverModel) => void;
        ResetLocation?:             (row: ReceiverModel) => void;
        TestConnection?:            (row: ReceiverModel) => void;
    }

    interface ReceiverLocationModel extends ViewJson.IReceiverLocationModel_KO
    {
        FormattedLatitude?:     KnockoutComputed<string>;
        FormattedLongitude?:    KnockoutComputed<string>;
        WrapUpValidation?:      IValidation_KC;

        SelectRow?:             (row: ReceiverLocationModel) => void;
        DeleteRow?:             (row: ReceiverLocationModel) => void;
    }

    interface UserModel extends ViewJson.IUserModel_KO
    {
        IsCurrentUser?:             KnockoutComputed<boolean>;
        IsWebSiteUser?:             KnockoutComputed<boolean>;
        IsAdminUser?:               KnockoutComputed<boolean>;
        LoginNameAndCurrentUser?:   KnockoutComputed<string>;
        WrapUpValidation?:          IValidation_KC;
        SelectRow?:                 (row: UserModel) => void;
        DeleteRow?:                 (row: UserModel) => void;
    }

    interface WebServerSettingsModel extends ViewJson.IWebServerSettingsModel_KO
    {
        WrapUpValidation?:                              IValidation_KC;
        WebServerAndInternetClientWrapUpValidation?:    IValidation_KC;
    }

    interface FeedModel
    {
        UniqueId:               KnockoutObservable<number>;
        Name:                   KnockoutObservable<string>;
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId: ViewId;
        private _SaveAttempted = false;
        private _AccessEditor = new AccessEditor();

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('Settings', viewId);

            $('#edit-receiver').on('hidden.bs.modal', () => {
                if(this._Model && this._Model.TestConnectionOutcome) {
                    this._Model.TestConnectionOutcome(null);
                }
            });

            this.refreshState();
        }

        refreshState()
        {
            this.showFailureMessage(null);

            this._ViewId.ajax('GetState', {
                success: (data: IResponse<ViewJson.IViewModel>) => {
                    this.applyState(data);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            }, false);
        }

        private sendAndApplyConfiguration(methodName: string, success: (state: IResponse<ViewJson.IViewModel>) => void, applyConfigurationFirst: boolean = true)
        {
            var settings = this.buildAjaxSettingsForSendConfiguration();
            settings.success = (data: IResponse<ViewJson.IViewModel>) => {
                if(applyConfigurationFirst) {
                    this.applyState(data);
                    success(data);
                } else {
                    success(data);
                    this.applyState(data);
                }
            };

            this._ViewId.ajax(methodName, settings);
        }

        private buildAjaxSettingsForSendConfiguration() : JQueryAjaxSettings
        {
            var configuration = ko.viewmodel.toModel(this._Model);
            var result = {
                method: 'POST',
                data: {
                    configurationModel: JSON.stringify(configuration)
                },
                dataType: 'json',
                error: (jqXHR: JQueryXHR, textStatus: string, errorThrown: string) => {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                }
            };

            return result;
        }

        save()
        {
            this._Model.SaveAttempted(false);

            this.sendAndApplyConfiguration('RaiseSaveClicked', (state: IResponse<ViewJson.IViewModel>) => {
                if(state.Response && state.Response.Outcome) {
                    this._Model.SaveAttempted(true);
                    this._Model.SaveSuccessful(state.Response.Outcome === "Saved");
                    switch(state.Response.Outcome || "") {
                        case "Saved":               this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved); break;
                        case "FailedValidation":    this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Validation_Failed); break;
                        case "ConflictingUpdate":   this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update); break;
                    }
                }
            });
        }

        createAndEditMergedFeed()
        {
            this._Model.SelectedMergedFeed(null);

            this.sendAndApplyConfiguration('CreateNewMergedFeed', (state: IResponse<ViewJson.IViewModel>) => {
                this._Model.MergedFeeds.unshiftFromModel(state.Response.NewMergedFeed);
                this._Model.SelectedMergedFeed(this._Model.MergedFeeds()[0]);

                $('#edit-merged-feed').modal('show');
            });
        }

        createAndEditRebroadcastServer()
        {
            this._Model.SelectedRebroadcastServer(null);

            this.sendAndApplyConfiguration('CreateNewRebroadcastServer', (state: IResponse<ViewJson.IViewModel>) => {
                this._Model.RebroadcastSettings.unshiftFromModel(state.Response.NewRebroadcastServer);
                this._Model.SelectedRebroadcastServer(this._Model.RebroadcastSettings()[0]);

                $('#edit-rebroadcast-server').modal('show');
            });
        }

        createAndEditReceiver()
        {
            this._Model.SelectedReceiver(null);

            this.sendAndApplyConfiguration('CreateNewReceiver', (state: IResponse<ViewJson.IViewModel>) => {
                this._Model.Receivers.unshiftFromModel(state.Response.NewReceiver);
                this._Model.SelectedReceiver(this._Model.Receivers()[0]);

                $('#edit-receiver').modal('show');
            });
        }

        testConnection(receiver: ReceiverModel)
        {
            this._Model.TestConnectionOutcome(null);
            var settings = this.buildAjaxSettingsForSendConfiguration();
            settings.data.receiverId = receiver.UniqueId();
            settings.success = (outcome: IResponse<ViewJson.ITestConnectionOutcomeModel>) => {
                this._Model.TestConnectionOutcome(outcome.Response);
            };

            this._ViewId.ajax('TestConnection', settings);
        }

        createAndEditReceiverLocation()
        {
            this._Model.SelectedReceiverLocation(null);

            this.sendAndApplyConfiguration('CreateNewReceiverLocation', (state: IResponse<ViewJson.IViewModel>) => {
                this._Model.ReceiverLocations.unshiftFromModel(state.Response.NewReceiverLocation);
                this._Model.SelectedReceiverLocation(this._Model.ReceiverLocations()[0]);

                $('#edit-receiver-location').modal('show');
            });
        }

        createAndEditUser()
        {
            this._Model.SelectedUser(null);

            this.sendAndApplyConfiguration('CreateNewUser', (state: IResponse<ViewJson.IViewModel>) => {
                this._Model.Users.unshiftFromModel(state.Response.NewUser);
                this._Model.SelectedUser(this._Model.Users()[0]);

                $('#edit-user').modal('show');
            });
        }

        useIcaoRawDecodingSettings()
        {
            this.sendAndApplyConfiguration('RaiseUseIcaoRawDecodingSettingsClicked', (state: IResponse<ViewJson.IViewModel>) => { });
        }

        useRecommendedRawDecodingSettings()
        {
            this.sendAndApplyConfiguration('RaiseUseRecommendedRawDecodingSettingsClicked', (state: IResponse<ViewJson.IViewModel>) => { });
        }

        updateLocationsFromBaseStation()
        {
            this.sendAndApplyConfiguration('RaiseReceiverLocationsFromBaseStationDatabaseClicked', (state: IResponse<ViewJson.IViewModel>) => { });
        }

        private showFailureMessage(message: string)
        {
            var alert = $('#failure-message');
            if(message && message.length) {
                alert.text(message || '').show();
            } else {
                alert.hide();
            }
        }

        /**
         * Called when items are added to or removed from Receivers and MergedFeeds.
         */
        private feedlistChanged()
        {
            this.synchroniseFeeds();
            this.removeDeletedReceiversFromMergedFeeds();
        }

        /**
         * Keeps the Feeds array in sync with the content of the Receivers and MergedFeeds arrays.
         */
        private synchroniseFeeds()
        {
            var allFeeds: FeedModel[] = [];
            $.each(this._Model.Receivers(),   (idx, feed) => allFeeds.push({ UniqueId: feed.UniqueId, Name: feed.Name }));
            $.each(this._Model.MergedFeeds(), (idx, feed) => allFeeds.push({ UniqueId: feed.UniqueId, Name: feed.Name }));

            var feeds = this._Model.Feeds();

            // Delete items in Feeds that no longer appear in Receivers or MergedFeeds
            for(let i = feeds.length - 1;i >= 0;--i) {
                var feed = feeds[i];
                if(!VRS.arrayHelper.findFirst(allFeeds, r => r.UniqueId() == feed.UniqueId())) {
                    this._Model.Feeds.splice(i, 1);
                }
            }

            // Add items to Feeds if they only appear in Receivers or MergedFeeds
            var addList = VRS.arrayHelper.except(allFeeds, feeds, (lhs, rhs) => lhs.UniqueId() === rhs.UniqueId());
            for(let i = 0;i < addList.length;++i) {
                this._Model.Feeds.push(addList[i]);
            }
        }

        /**
         * Removes deleted receiver identifiers from merged feeds.
         */
        private removeDeletedReceiversFromMergedFeeds()
        {
            var receiverIds: number[] = [];
            $.each(this._Model.Receivers(), (idx, receiver) => receiverIds.push(receiver.UniqueId()));
            $.each(this._Model.MergedFeeds(), (idx, mergedFeed) => {
                for(var i = mergedFeed.ReceiverIds().length;i >= 0;--i) {
                    var receiverId = mergedFeed.ReceiverIds()[i];
                    if(receiverIds.indexOf(receiverId) === -1) {
                        mergedFeed.ReceiverIds.splice(i, 1);

                        var flagsIndex = VRS.arrayHelper.indexOfMatch(mergedFeed.ReceiverFlags(), r => r.UniqueId() === receiverId);
                        if(flagsIndex !== -1) {
                            mergedFeed.ReceiverFlags.splice(flagsIndex, 1);
                        }
                    }
                }
            });
        }

        private applyState(state: IResponse<ViewJson.IViewModel>)
        {
            if(state.Exception) {
                this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, state.Exception));
            } else {
                this.showFailureMessage(null);

                if(this._Model) {
                    ko.viewmodel.updateFromModel(this._Model, state.Response.Configuration);
                } else {
                    this._Model = ko.viewmodel.fromModel(state.Response.Configuration, {
                        arrayChildId: {
                            '{root}.MergedFeeds':                               'UniqueId',
                            '{root}.RebroadcastSettings':                       'UniqueId',
                            '{root}.RebroadcastSettings[i].Access.Addresses':   'Cidr',
                            '{root}.ReceiverLocations':                         'UniqueId',
                            '{root}.Receivers':                                 'UniqueId',
                            '{root}.Receivers[i].Access.Addresses':             'Cidr',
                            '{root}.Users':                                     'UniqueId'
                        },

                        extend: {
                            '{root}': (root: Model) =>
                            {
                                root.SaveAttempted = ko.observable(false);
                                root.SaveSuccessful = ko.observable(false);
                                root.SavedMessage = ko.observable("");

                                root.TestConnectionOutcome = <KnockoutObservable<ViewJson.ITestConnectionOutcomeModel>> ko.observable(null);
                                root.CurrentUserName = ko.observable(state.Response.CurrentUserName);
                                root.SelectedMergedFeed = <KnockoutObservable<MergedFeedModel>> ko.observable(null);
                                root.SelectedRebroadcastServer = <KnockoutObservable<RebroadcastServerModel>> ko.observable(null);
                                root.SelectedReceiver = <KnockoutObservable<ReceiverModel>> ko.observable(null);
                                root.SelectedReceiverLocation = <KnockoutObservable<ReceiverLocationModel>> ko.observable(null);
                                root.SelectedUser = <KnockoutObservable<UserModel>> ko.observable(null);

                                root.ComPortNames =         state.Response.ComPortNames;
                                root.VoiceNames =           state.Response.VoiceNames;
                                root.ConnectionTypes =      state.Response.ConnectionTypes;
                                root.DataSources =          state.Response.DataSources;
                                root.DefaultAccesses =      state.Response.DefaultAccesses;
                                root.DistanceUnits =        state.Response.DistanceUnits;
                                root.Handshakes =           state.Response.Handshakes;
                                root.HeightUnits =          state.Response.HeightUnits;
                                root.Parities =             state.Response.Parities;
                                root.ProxyTypes =           state.Response.ProxyTypes;
                                root.RebroadcastFormats =   state.Response.RebroadcastFormats;
                                root.ReceiverUsages =       state.Response.ReceiverUsages;
                                root.SpeedUnits =           state.Response.SpeedUnits;
                                root.StopBits =             state.Response.StopBits;
                            },

                            '{root}.AudioSettings': (model: AudioSettingsModel) =>
                            {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.SetDefaultVoice = () => {
                                    model.VoiceName(null);
                                };
                            },

                            '{root}.BaseStationSettings': (model: BaseStationSettingsModel) =>
                            {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(
                                    this._ViewId.findValidationProperties(model, (name: string, value: VirtualRadar.Interface.View.IValidationModelField_KO) => {
                                        return value !== model.AutoSavePolarPlotsMinutesValidation &&       // Shown in General
                                               value !== model.DisplayTimeoutSecondsValidation &&           // Shown in General
                                               value !== model.TrackingTimeoutSecondsValidation &&          // Shown in General
                                               value !== model.SatcomDisplayTimeoutMinutesValidation &&     // Shown in General
                                               value !== model.SatcomTrackingTimeoutMinutesValidation;      // Shown in General
                                    })
                                );
                            },

                            '{root}.GoogleMapSettings': (model: GoogleMapSettingsModel) =>
                            {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(
                                    this._ViewId.findValidationProperties(model, (name: string, value: VirtualRadar.Interface.View.IValidationModelField_KO) => {
                                        return value !== model.ClosestAircraftReceiverIdValidation &&       // Shown in Receivers
                                               value !== model.FlightSimulatorXReceiverIdValidation &&      // Shown in Receivers
                                               value !== model.ShortTrailLengthSecondsValidation &&         // Shown in General
                                               value !== model.WebSiteReceiverIdValidation;                 // Shown in Receivers
                                    })
                                );
                            },

                            '{root}.InternetClientSettingsModel': (model: InternetClientSettingsModel) =>
                            {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));
                            },

                            '{root}.MergedFeeds[i]': (model: MergedFeedModel) =>
                            {
                                model.FormattedReceiversCount = ko.computed(() => VRS.stringUtility.formatNumber(model.ReceiverIds().length, 'N0'));
                                model.FormattedIcaoTimeout = ko.computed(() => VRS.stringUtility.formatNumber(model.IcaoTimeout() / 1000, 'N2'));
                                model.FormattedIgnoreModeS = ko.computed(() => model.IgnoreAircraftWithNoPosition() ? VRS.$$.Yes : VRS.$$.No);
                                model.FormattedHidden = ko.computed(() => model.ReceiverUsage() !== 0 ? VRS.$$.Yes : VRS.$$.No);
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.HideFromWebSite = ko.pureComputed({
                                    read: () => {
                                        return model.ReceiverUsage() != 0;
                                    },
                                    write: (value) => {
                                        model.ReceiverUsage(value ? 1 : 0);
                                    },
                                    owner: this
                                });
                                model.IcaoTimeoutSeconds = ko.pureComputed({
                                    read: () => {
                                        return model.IcaoTimeout() / 1000;
                                    },
                                    write: (value) => {
                                        model.IcaoTimeout(value * 1000);
                                    },
                                    owner: this
                                });
                                model.SelectRow = (row: MergedFeedModel) => {
                                    this._Model.SelectedMergedFeed(row);
                                };
                                model.DeleteRow = (row: MergedFeedModel) => {
                                    var index = VRS.arrayHelper.indexOfMatch(this._Model.MergedFeeds(), r => r.UniqueId == row.UniqueId);
                                    this._Model.MergedFeeds.splice(index, 1);
                                };

                                model.IncludeReceiver = (receiver: ReceiverModel) => {
                                    return ko.pureComputed({
                                        read: () => {
                                            return model.ReceiverIds().indexOf(receiver.UniqueId()) !== -1;
                                        },
                                        write: (value) => {
                                            var index = model.ReceiverIds().indexOf(receiver.UniqueId());
                                            if(value && index === -1) {
                                                model.ReceiverIds.pushFromModel(receiver.UniqueId());
                                            } else if(!value && index !== -1) {
                                                model.ReceiverIds.removeAtToModel(index, receiver.UniqueId());
                                            }
                                        },
                                        owner: this
                                    });
                                };
                                model.ReceiverIsMlat = (receiver: ReceiverModel) => {
                                    return ko.pureComputed({
                                        read: () => {
                                            var flags = VRS.arrayHelper.findFirst(model.ReceiverFlags(), r => r.UniqueId() == receiver.UniqueId());
                                            return flags && flags.IsMlatFeed();
                                        },
                                        write: (value) => {
                                            var index = VRS.arrayHelper.indexOfMatch(model.ReceiverFlags(), r => r.UniqueId() == receiver.UniqueId());
                                            if(index === -1) {
                                                var blank: ViewJson.IMergedFeedReceiverModel = {
                                                    UniqueId: receiver.UniqueId(),
                                                    IsMlatFeed: false,
                                                };
                                                model.ReceiverFlags.unshiftFromModel(blank);
                                                index = 0;
                                            }
                                            var flags = model.ReceiverFlags()[index];
                                            flags.IsMlatFeed(value);
                                        },
                                        owner: this
                                    });
                                };
                            },

                            '{root}.RawDecodingSettings': (model: RawDecodingSettingsModel) =>
                            {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));
                            },

                            '{root}.RebroadcastSettings[i]': (model: RebroadcastServerModel) =>
                            {
                                model.FormattedAccess = ko.computed(() => this._ViewId.describeEnum(model.Access.DefaultAccess(), state.Response.DefaultAccesses));
                                model.FormattedAddress = ko.computed(() => VRS.stringUtility.format('{0}:{1}', (model.TransmitAddress() ? model.TransmitAddress() : ':'), model.Port()));
                                model.FormatDescription = ko.computed(() => {
                                    var rebroadcastFormat = VRS.arrayHelper.findFirst(state.Response.RebroadcastFormats, r => r.UniqueId === model.Format());
                                    return rebroadcastFormat ? rebroadcastFormat.ShortName : VRS.Server.$$.Unknown;
                                });
                                model.Feed = ko.pureComputed({
                                    read: () => {
                                        let feedId = model.ReceiverId();
                                        let feed = feedId ? VRS.arrayHelper.findFirst(this._Model.Feeds(), r => r.UniqueId() === feedId) : null;
                                        return <FeedModel>feed;
                                    },
                                    write: (value) => {
                                        model.ReceiverId(value ? value.UniqueId() : 0);
                                    },
                                    owner: this
                                });
                                model.SendIntervalSeconds = ko.pureComputed({
                                    read: () => {
                                        return Math.floor(model.SendIntervalMilliseconds() / 1000);
                                    },
                                    write: (value) => {
                                        model.SendIntervalMilliseconds(value * 1000);
                                    },
                                    owner: this
                                });

                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.SelectRow = (row: RebroadcastServerModel) => {
                                    this._Model.SelectedRebroadcastServer(row);
                                };
                                model.DeleteRow = (row: RebroadcastServerModel) => {
                                    var index = VRS.arrayHelper.indexOfMatch(this._Model.RebroadcastSettings(), r => r.UniqueId == row.UniqueId);
                                    this._Model.RebroadcastSettings.splice(index, 1);
                                };
                            },

                            '{root}.RebroadcastSettings[i].Access': (model: AccessModel) => {
                                this._AccessEditor.BuildAccessModel(model);
                            },
                            '{root}.RebroadcastSettings[i].Access.Addresses[i]': (model: AccessCidrModel) => {
                                this._AccessEditor.BuildAccessCidrModel(model);
                            },

                            '{root}.Receivers[i]': (model: ReceiverModel) =>
                            {
                                model.FormattedConnectionType = ko.computed(() => this._ViewId.describeEnum(model.ConnectionType(), state.Response.ConnectionTypes));
                                model.FormattedHandshake = ko.computed(() => this._ViewId.describeEnum(model.Handshake(), state.Response.Handshakes));
                                model.FormattedParity = ko.computed(() => this._ViewId.describeEnum(model.Parity(), state.Response.Parities));
                                model.FormattedReceiverUsage = ko.computed(() => this._ViewId.describeEnum(model.ReceiverUsage(), state.Response.ReceiverUsages));
                                model.FormattedStopBits = ko.computed(() => this._ViewId.describeEnum(model.StopBits(), state.Response.StopBits));
                                model.FormattedDataSource = ko.computed(() => {
                                    var receiverFormat = VRS.arrayHelper.findFirst(state.Response.DataSources, r => r.UniqueId === model.DataSource());
                                    return receiverFormat ? receiverFormat.ShortName : VRS.Server.$$.Unknown;
                                });
                                model.ConnectionParameters = ko.computed(() => {
                                    let connectionParameters = '';
                                    switch(model.ConnectionType()) {
                                        case 1:     // COM
                                            connectionParameters = VRS.stringUtility.format('{0}, {1}, {2}/{3}, {4}, {5}, "{6}", "{7}"',
                                                model.ComPort(),
                                                model.BaudRate(),
                                                model.DataBits(),
                                                this._ViewId.describeEnum(model.StopBits(), state.Response.StopBits),
                                                this._ViewId.describeEnum(model.Parity(), state.Response.Parities),
                                                this._ViewId.describeEnum(model.Handshake(), state.Response.Handshakes),
                                                model.StartupText(),
                                                model.ShutdownText()
                                            );
                                            break;
                                        case 0:     // TCP
                                            connectionParameters = VRS.stringUtility.format("{0}:{1}",
                                                model.Address(),
                                                model.Port()
                                            );
                                            break;
                                    }
                                    return connectionParameters;
                                });

                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.IdleTimeoutSeconds = ko.pureComputed({
                                    read: () => {
                                        return Math.floor(model.IdleTimeoutMilliseconds() / 1000);
                                    },
                                    write: (value) => {
                                        model.IdleTimeoutMilliseconds(value * 1000);
                                    },
                                    owner: this
                                });
                                model.Location = ko.pureComputed({
                                    read: () => {
                                        let receiverLocationId = model.ReceiverLocationId();
                                        let receiverLocation = receiverLocationId ? VRS.arrayHelper.findFirst(this._Model.ReceiverLocations(), r => r.UniqueId() === receiverLocationId) : null;
                                        return <ReceiverLocationModel>receiverLocation;
                                    },
                                    write: (value) => {
                                        model.ReceiverLocationId(value ? value.UniqueId() : 0);
                                    },
                                    owner: this
                                });

                                model.SelectRow = (row: ReceiverModel) => {
                                    this._Model.SelectedReceiver(row);
                                };
                                model.DeleteRow = (row: ReceiverModel) => {
                                    var index = VRS.arrayHelper.indexOfMatch(this._Model.Receivers(), r => r.UniqueId == row.UniqueId);
                                    this._Model.Receivers.splice(index, 1);
                                };
                                model.ResetLocation = (row: ReceiverModel) => {
                                    row.ReceiverLocationId(0);
                                };
                                model.TestConnection = (row: ReceiverModel) => {
                                    this.testConnection(row);
                                };
                            },

                            '{root}.Receivers[i].Access': (model: AccessModel) => {
                                this._AccessEditor.BuildAccessModel(model);
                            },
                            '{root}.Receivers[i].Access.Addresses[i]': (model: AccessCidrModel) => {
                                this._AccessEditor.BuildAccessCidrModel(model);
                            },

                            '{root}.ReceiverLocations[i]': (model: ReceiverLocationModel) =>
                            {
                                model.FormattedLatitude = ko.computed(() => VRS.stringUtility.formatNumber(model.Latitude(), 'N6'));
                                model.FormattedLongitude = ko.computed(() => VRS.stringUtility.formatNumber(model.Longitude(), 'N6'));
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.SelectRow = (row: ReceiverLocationModel) => {
                                    this._Model.SelectedReceiverLocation(row);
                                };
                                model.DeleteRow = (row: ReceiverLocationModel) => {
                                    var index = VRS.arrayHelper.indexOfMatch(this._Model.ReceiverLocations(), r => r.UniqueId == row.UniqueId);
                                    this._Model.ReceiverLocations.splice(index, 1);
                                };
                            },

                            '{root}.Users[i]': (model: UserModel) =>
                            {
                                model.IsCurrentUser = ko.pureComputed(() => VRS.stringUtility.equals(this._Model.CurrentUserName(), model.LoginName(), true));
                                model.IsAdminUser = ko.pureComputed({
                                    read: () => {
                                        return VRS.arrayHelper.indexOf(this._Model.WebServerSettings.AdministratorUserIds(), model.UniqueId()) !== -1;
                                    },
                                    write: (value) => {
                                        var index = VRS.arrayHelper.indexOf(this._Model.WebServerSettings.AdministratorUserIds(), model.UniqueId());
                                        if(value && index === -1) {
                                            this._Model.WebServerSettings.AdministratorUserIds.push(model.UniqueId());
                                        } else if(!value && index !== -1) {
                                            this._Model.WebServerSettings.AdministratorUserIds.splice(index, 1);
                                        }
                                    },
                                    owner: this
                                });
                                model.IsWebSiteUser = ko.pureComputed({
                                    read: () => {
                                        return VRS.arrayHelper.indexOf(this._Model.WebServerSettings.BasicAuthenticationUserIds(), model.UniqueId()) !== -1;
                                    },
                                    write: (value) => {
                                        var index = VRS.arrayHelper.indexOf(this._Model.WebServerSettings.BasicAuthenticationUserIds(), model.UniqueId());
                                        if(value && index === -1) {
                                            this._Model.WebServerSettings.BasicAuthenticationUserIds.push(model.UniqueId());
                                        } else if(!value && index !== -1) {
                                            this._Model.WebServerSettings.BasicAuthenticationUserIds.splice(index, 1);
                                        }
                                    },
                                    owner: this
                                });
                                model.LoginNameAndCurrentUser = ko.pureComputed({
                                    read: () => model.LoginName(),
                                    write: (value) => {
                                        if(model.IsCurrentUser()) {
                                            this._Model.CurrentUserName(value);
                                        }
                                        model.LoginName(value);
                                    },
                                    owner: this
                                });
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.SelectRow = (row: UserModel) => {
                                    this._Model.SelectedUser(row);
                                };
                                model.DeleteRow = (row: UserModel) => {
                                    model.IsWebSiteUser(false);
                                    model.IsAdminUser(false);
                                    var index = VRS.arrayHelper.indexOfMatch(this._Model.Users(), r => r.UniqueId == row.UniqueId);
                                    this._Model.Users.splice(index, 1);
                                };
                            },

                            '{root}.WebServerSettings': (model: WebServerSettingsModel) =>
                            {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));
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
                    this._Model.MergedFeedWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.MergedFeeds, (r) => (<MergedFeedModel>r).WrapUpValidation);
                    this._Model.RebroadcastServerWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.RebroadcastSettings, (r) => (<RebroadcastServerModel>r).WrapUpValidation);
                    this._Model.ReceiverWrapUpValidation = this._ViewId.createArrayWrapupValidation(
                        this._Model.Receivers,
                        (r) => (<ReceiverModel>r).WrapUpValidation,
                        this._Model.GoogleMapSettings.WebSiteReceiverIdValidation,
                        this._Model.GoogleMapSettings.ClosestAircraftReceiverIdValidation,
                        this._Model.GoogleMapSettings.FlightSimulatorXReceiverIdValidation
                    );
                    this._Model.ReceiverLocationWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.ReceiverLocations, (r) => (<ReceiverLocationModel>r).WrapUpValidation);
                    this._Model.UserWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.Users, (r) => (<UserModel>r).WrapUpValidation);

                    this._Model.Feeds = ko.observableArray([]);
                    this._Model.Receivers.subscribe(this.feedlistChanged, this);
                    this._Model.MergedFeeds.subscribe(this.feedlistChanged, this);
                    this.synchroniseFeeds();

                    var webServerAndInternetClientValidationFields = this._ViewId.findValidationProperties(this._Model.WebServerSettings);
                    this._ViewId.findValidationProperties(this._Model.InternetClientSettings, null, webServerAndInternetClientValidationFields);
                    (<WebServerSettingsModel>this._Model.WebServerSettings).WebServerAndInternetClientWrapUpValidation = this._ViewId.createWrapupValidation(webServerAndInternetClientValidationFields);

                    ko.applyBindings(this._Model);
                }
            }
        }
    }
}