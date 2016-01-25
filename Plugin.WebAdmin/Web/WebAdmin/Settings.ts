namespace VRS.WebAdmin.Settings
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Settings;

    interface Model extends ViewJson.IConfigurationModel_KO
    {
        saveAttempted?:         KnockoutObservable<boolean>;
        saveSuccessful?:        KnockoutObservable<boolean>;
        savedMessage?:          KnockoutObservable<string>;
        TestConnectionOutcome:  KnockoutObservable<ViewJson.ITestConnectionOutcomeModel>;

        SelectedMergedFeed?:        KnockoutObservable<MergedFeedModel>;
        SelectedReceiver?:          KnockoutObservable<ReceiverModel>;
        selectedReceiverLocation?:  KnockoutObservable<ReceiverLocationModel>;

        Feeds?:                 KnockoutObservableArray<FeedModel>;

        MergedFeedWrapUpValidation?:        IValidation_KC;
        ReceiverWrapUpValidation?:          IValidation_KC;
        ReceiverLocationWrapUpValidation?:  IValidation_KC;

        ComPortNames?:      string[];
        BaudRates?:         number[];
        ConnectionTypes?:   VirtualRadar.Interface.View.IEnumModel[];
        DataSources?:       VirtualRadar.Interface.View.IEnumModel[];
        DefaultAccesses?:   VirtualRadar.Interface.View.IEnumModel[];
        DistanceUnits?:     VirtualRadar.Interface.View.IEnumModel[];
        Handshakes?:        VirtualRadar.Interface.View.IEnumModel[];
        HeightUnits?:       VirtualRadar.Interface.View.IEnumModel[];
        Parities?:          VirtualRadar.Interface.View.IEnumModel[];
        ProxyTypes?:        VirtualRadar.Interface.View.IEnumModel[];
        ReceiverUsages?:    VirtualRadar.Interface.View.IEnumModel[];
        SpeedUnits?:        VirtualRadar.Interface.View.IEnumModel[];
        StopBits?:          VirtualRadar.Interface.View.IEnumModel[];
    }

    interface BaseStationSettingsModel extends ViewJson.IBaseStationSettingsModel_KO
    {
        WrapUpValidation?:      IValidation_KC;
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
            });
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
            this._Model.saveAttempted(false);

            this.sendAndApplyConfiguration('RaiseSaveClicked', (state: IResponse<ViewJson.IViewModel>) => {
                if(state.Response && state.Response.Outcome) {
                    this._Model.saveAttempted(true);
                    this._Model.saveSuccessful(state.Response.Outcome === "Saved");
                    switch(state.Response.Outcome || "") {
                        case "Saved":               this._Model.savedMessage(VRS.WebAdmin.$$.WA_Saved); break;
                        case "FailedValidation":    this._Model.savedMessage(VRS.WebAdmin.$$.WA_Validation_Failed); break;
                        case "ConflictingUpdate":   this._Model.savedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update); break;
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
            this._Model.selectedReceiverLocation(null);

            this.sendAndApplyConfiguration('CreateNewReceiverLocation', (state: IResponse<ViewJson.IViewModel>) => {
                this._Model.ReceiverLocations.unshiftFromModel(state.Response.NewReceiverLocation);
                this._Model.selectedReceiverLocation(this._Model.ReceiverLocations()[0]);

                $('#edit-receiver-location').modal('show');
            });
        }

        updateLocationsFromBaseStation()
        {
            this.sendAndApplyConfiguration('RaiseReceiverLocationsFromBaseStationDatabaseClicked', (state: IResponse<ViewJson.IViewModel>) => {
            });
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
                            '{root}.MergedFeeds':       'UniqueId',
                            '{root}.ReceiverLocations': 'UniqueId',
                            '{root}.Receivers':         'UniqueId'
                        },

                        extend: {
                            '{root}': (root: Model) =>
                            {
                                root.saveAttempted = ko.observable(false);
                                root.saveSuccessful = ko.observable(false);
                                root.savedMessage = ko.observable("");

                                root.ComPortNames =     state.Response.ComPortNames;
                                root.ConnectionTypes =  state.Response.ConnectionTypes;
                                root.DataSources =      state.Response.DataSources;
                                root.DefaultAccesses =  state.Response.DefaultAccesses;
                                root.DistanceUnits =    state.Response.DistanceUnits;
                                root.Handshakes =       state.Response.Handshakes;
                                root.HeightUnits =      state.Response.HeightUnits;
                                root.Parities =         state.Response.Parities;
                                root.ProxyTypes =       state.Response.ProxyTypes;
                                root.ReceiverUsages =   state.Response.ReceiverUsages;
                                root.SpeedUnits =       state.Response.SpeedUnits;
                                root.StopBits =         state.Response.StopBits;

                                root.TestConnectionOutcome = <KnockoutObservable<ViewJson.ITestConnectionOutcomeModel>> ko.observable(null);
                                root.SelectedMergedFeed = <KnockoutObservable<MergedFeedModel>> ko.observable(null);
                                root.SelectedReceiver = <KnockoutObservable<ReceiverModel>> ko.observable(null);
                                root.selectedReceiverLocation = <KnockoutObservable<ReceiverLocationModel>> ko.observable(null);
                            },

                            '{root}.BaseStationSettings': (model: BaseStationSettingsModel) =>
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

                            '{root}.Receivers[i]': (model: ReceiverModel) =>
                            {
                                model.FormattedConnectionType = ko.computed(() => this._ViewId.describeEnum(model.ConnectionType(), state.Response.ConnectionTypes));
                                model.FormattedDataSource = ko.computed(() => this._ViewId.describeEnum(model.DataSource(), state.Response.DataSources));
                                model.FormattedHandshake = ko.computed(() => this._ViewId.describeEnum(model.Handshake(), state.Response.Handshakes));
                                model.FormattedParity = ko.computed(() => this._ViewId.describeEnum(model.Parity(), state.Response.Parities));
                                model.FormattedReceiverUsage = ko.computed(() => this._ViewId.describeEnum(model.ReceiverUsage(), state.Response.ReceiverUsages));
                                model.FormattedStopBits = ko.computed(() => this._ViewId.describeEnum(model.StopBits(), state.Response.StopBits));
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
                                        case 0:         // TCP
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
                                        let receiverId = model.ReceiverLocationId();
                                        let receiver = receiverId ? VRS.arrayHelper.findFirst(this._Model.ReceiverLocations(), r => r.UniqueId() === receiverId) : null;
                                        return <ReceiverLocationModel>receiver;
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

                            '{root}.ReceiverLocations[i]': (model: ReceiverLocationModel) =>
                            {
                                model.FormattedLatitude = ko.computed(() => VRS.stringUtility.formatNumber(model.Latitude(), 'N6'));
                                model.FormattedLongitude = ko.computed(() => VRS.stringUtility.formatNumber(model.Longitude(), 'N6'));
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.SelectRow = (row: ReceiverLocationModel) => {
                                    this._Model.selectedReceiverLocation(row);
                                };
                                model.DeleteRow = (row: ReceiverLocationModel) => {
                                    var index = VRS.arrayHelper.indexOfMatch(this._Model.ReceiverLocations(), r => r.UniqueId == row.UniqueId);
                                    this._Model.ReceiverLocations.splice(index, 1);
                                };
                            }
                        }
                    });

                    this._Model.MergedFeedWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.MergedFeeds, (r) => (<MergedFeedModel>r).WrapUpValidation);
                    this._Model.ReceiverWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.Receivers, (r) => (<ReceiverModel>r).WrapUpValidation);
                    this._Model.ReceiverLocationWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.ReceiverLocations, (r) => (<ReceiverLocationModel>r).WrapUpValidation);

                    this._Model.Feeds = ko.observableArray([]);
                    this._Model.Receivers.subscribe(this.feedlistChanged, this);
                    this._Model.MergedFeeds.subscribe(this.feedlistChanged, this);
                    this.synchroniseFeeds();

                    ko.applyBindings(this._Model);
                }
            }
        }
    }
}