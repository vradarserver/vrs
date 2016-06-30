namespace VRS.WebAdmin.Index
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Main;

    interface Model extends ViewJson.IViewModel_KO
    {
        HasFailedPlugins?:              KnockoutComputed<boolean>;
        FailedPluginsMessage?:          KnockoutComputed<string>;
        UpnpStatus?:                    KnockoutComputed<string>;
        UpnpButtonText?:                KnockoutComputed<string>;
        UpnpButtonDisabled?:            KnockoutComputed<boolean>;

        AddressPerspectives?:           AddressPerspective[];
        AddressPages?:                  AddressPage[];
        SelectedAddressPerspective?:    KnockoutObservable<AddressPerspective>;
        SelectedAddressPage?:           KnockoutObservable<AddressPage>;
        AddressUrl?:                    KnockoutComputed<string>;

        SelectedFeed?:                  KnockoutObservable<FeedModel>;
        SelectFeed?:                    (feed: FeedModel) => void;
    }

    interface FeedModel extends ViewJson.IFeedStatusModel_KO
    {
        StatusClass?:               KnockoutComputed<string>;
        FormattedMsgs?:             KnockoutComputed<string>;
        FormattedBadMsgs?:          KnockoutComputed<string>;
        FormattedTracked?:          KnockoutComputed<string>;
        ConnectorActivityLogUrl?:   KnockoutComputed<string>;
        StatisticsUrl?:             KnockoutComputed<string>;
    }

    interface RequestModel extends ViewJson.IServerRequestModel_KO
    {
        FormattedBytes?:    KnockoutComputed<string>;
    }

    interface RebroadcastServerModel extends ViewJson.IRebroadcastServerConnectionModel_KO
    {
        FormattedBuffered:  KnockoutComputed<string>;
        FormattedWritten:   KnockoutComputed<string>;
        FormattedDiscarded: KnockoutComputed<string>;
    }

    interface AddressPerspective
    {
        perspective: string;
        description: string;
    }

    interface AddressPage
    {
        page: string;
        description: string;
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId = new ViewId('Index');

        constructor()
        {
            this.refreshState();
        }

        refreshState()
        {
            this._ViewId.ajax('GetState', {
                success: (data: IResponse<ViewJson.IViewModel>) => {
                    this.applyState(data);
                    setTimeout(() => this.refreshState(), 1000);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            }, false);
        }

        toggleUPnpStatus()
        {
            this._ViewId.ajax('RaiseToggleUPnpStatus');
        }

        resetPolarPlot()
        {
            var feed = this._Model.SelectedFeed();
            this._ViewId.ajax('RaiseResetPolarPlot', {
                data: {
                    feedId: feed.Id()
                }
            });
        }

        reconnectFeed()
        {
            var feed = this._Model.SelectedFeed();
            this._ViewId.ajax('RaiseReconnectFeed', {
                data: {
                    feedId: feed.Id()
                }
            });
        }

        private applyState(state: IResponse<ViewJson.IViewModel>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(this._Model, state.Response);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response, {
                    arrayChildId: {
                        '{root}.Feeds':             'Id',
                        '{root}.Requests':          'RemoteAddr',
                        '{root}.Rebroadcasters':    'Id'
                    },

                    extend: {
                        '{root}': function(root: Model)
                        {
                            root.HasFailedPlugins = ko.computed(() => root.BadPlugins() > 0);
                            root.FailedPluginsMessage = ko.computed(() => VRS.stringUtility.format(VRS.Server.$$.CountPluginsCouldNotBeLoaded, root.BadPlugins()));
                            root.UpnpStatus = ko.computed(() => {
                                   return !root.Upnp() && root.UpnpOn() ? VRS.Server.$$.UPnpActiveWhileDisabled :
                                          !root.Upnp()                  ? VRS.Server.$$.UPnpDisabled :
                                          !root.UpnpRouter()            ? VRS.Server.$$.UPnPNotPresent :
                                          !root.UpnpOn()                ? VRS.Server.$$.UPnpInactive :
                                          VRS.Server.$$.UPnpActive;
                            });
                            root.UpnpButtonText = ko.computed(() => root.UpnpOn() ? VRS.Server.$$.TakeOffInternet : VRS.Server.$$.PutOnInternet);
                            root.UpnpButtonDisabled = ko.computed(() => !root.UpnpRouter() || !root.Upnp());

                            root.AddressPerspectives = [
                                { perspective: 'local',     description: VRS.Server.$$.ShowLocalAddress },
                                { perspective: 'network',   description: VRS.Server.$$.ShowNetworkAddress },
                                { perspective: 'public',    description: VRS.Server.$$.ShowInternetAddress },
                            ];
                            root.AddressPages = [
                                { page: '/',                description: VRS.Server.$$.DefaultVersion },
                                { page: '/desktop.html',    description: VRS.Server.$$.DesktopVersion },
                                { page: '/mobile.html',     description: VRS.Server.$$.MobileVersion },
                                { page: '/fsx.html',        description: VRS.Server.$$.FlightSimVersion },
                                { page: '/GoogleMap.htm',   description: VRS.Server.$$.DesktopVersionOld },
                                { page: '/iPhoneMap.htm',   description: VRS.Server.$$.MobileVersionOld },
                                { page: '/FlightSim.htm',   description: VRS.Server.$$.FlightSimVersionOld },
                                { page: '/settings.html',   description: VRS.Server.$$.SettingsPage },
                            ];
                            var initialPerspective = 0;
                            var currentUrl = window.location.href.toLowerCase();
                            if(currentUrl.indexOf(root.LanRoot().toLowerCase()) !== -1) {
                                initialPerspective = 1;
                            } else if(currentUrl.indexOf(root.PublicRoot().toLowerCase()) !== -1) {
                                initialPerspective = 2;
                            }
                            root.SelectedAddressPerspective = ko.observable(root.AddressPerspectives[initialPerspective]);
                            root.SelectedAddressPage = ko.observable(root.AddressPages[initialPerspective]);
                            root.AddressUrl = ko.computed(() => {
                                var result = '#';
                                if(root.SelectedAddressPerspective() && root.SelectedAddressPage()) {
                                    switch(root.SelectedAddressPerspective().perspective) {
                                        case 'local':   result = root.LocalRoot(); break;
                                        case 'network': result = root.LanRoot(); break;
                                        case 'public':  result = root.PublicRoot(); break;
                                    }
                                    result += root.SelectedAddressPage().page;
                                }
                                return result;
                            });

                            root.SelectedFeed = <KnockoutObservable<FeedModel>>ko.observable({});
                            root.SelectFeed = (feed: FeedModel) => {
                                root.SelectedFeed(feed);
                            }
                        },

                        '{root}.Feeds[i]': function(feed: FeedModel)
                        {
                            feed.FormattedMsgs = ko.computed(() => VRS.stringUtility.format('{0:N0}', feed.Msgs()));
                            feed.FormattedBadMsgs = ko.computed(() => VRS.stringUtility.format('{0:N0}', feed.BadMsgs()));
                            feed.FormattedTracked = ko.computed(() => VRS.stringUtility.format('{0:N0}', feed.Tracked()));
                            feed.StatusClass = ko.computed(() => feed.ConnDesc() === VRS.Server.$$.Connected ? '' : 'bg-danger');
                            feed.ConnectorActivityLogUrl = ko.computed(() => feed.Merged() ? '' : 'ConnectorActivityLog.html?connectorName=' + encodeURIComponent(feed.Name()));
                            feed.StatisticsUrl = ko.computed(() => feed.Merged() ? '' : 'Statistics.html?feedId=' + encodeURIComponent(String(feed.Id())));
                        },

                        '{root}.Requests[i]': function(request: RequestModel)
                        {
                            request.FormattedBytes = ko.computed(() => VRS.stringUtility.format('{0:N0}', request.Bytes()));
                        },

                        '{root}.Rebroadcasters[i]': function(rebroadcast: RebroadcastServerModel)
                        {
                            rebroadcast.FormattedBuffered = ko.computed(() => VRS.stringUtility.format('{0:N0}', rebroadcast.Buffered()));
                            rebroadcast.FormattedDiscarded = ko.computed(() => VRS.stringUtility.format('{0:N0}', rebroadcast.Discarded()));
                            rebroadcast.FormattedWritten = ko.computed(() => VRS.stringUtility.format('{0:N0}', rebroadcast.Written()));
                        }
                    }
                });
                ko.applyBindings(this._Model);
            }
        }
    }
}
