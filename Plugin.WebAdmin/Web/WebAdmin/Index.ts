namespace VRS.WebAdmin.Index
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View;

    interface Model extends ViewJson.IMainView_KO
    {
        HasFailedPlugins?:      KnockoutComputed<boolean>;
        FailedPluginsMessage?:  KnockoutComputed<string>;
    }

    interface FeedModel extends VirtualRadar.Interface.View.IFeedStatus_KO
    {
        FormattedMsgs?:     KnockoutComputed<string>;
        FormattedBadMsgs?:  KnockoutComputed<string>;
        FormattedTracked?:  KnockoutComputed<string>;
    }

    interface RequestModel extends VirtualRadar.Interface.View.IServerRequest_KO
    {
        FormattedBytes?:    KnockoutComputed<string>;
    }

    interface RebroadcastServerModel extends VirtualRadar.Interface.IRebroadcastServerConnection_KO
    {
        FormattedBuffered:  KnockoutObservable<string>;
        FormattedWritten:   KnockoutObservable<string>;
        FormattedDiscarded: KnockoutObservable<string>;
    }

    export class PageHandler
    {
        private _Model: Model;

        constructor()
        {
            this.refreshState();
        }

        refreshState()
        {
            $.ajax({
                url: 'Index/GetState',
                success: (data: IResponse<ViewJson.IMainView>) => {
                    this.applyState(data);
                    setTimeout(() => this.refreshState(), 1000);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            });
        }

        private applyState(state: IResponse<ViewJson.IMainView>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(this._Model, state.Response);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response, {
                    arrayChildId: {
                        "{root}.Feeds":             'Id',
                        "{root}.Requests":          'RemoteAddr',
                        "{root}.Rebroadcasters":    'Id'
                    },
                    extend: {
                        "{root}": function(root: Model)
                        {
                            root.HasFailedPlugins = ko.computed(() => root.BadPlugins() > 0);
                            root.FailedPluginsMessage = ko.computed(() => VRS.stringUtility.format(VRS.Server.$$.CountPluginsCouldNotBeLoaded, root.BadPlugins()));
                        },
                        "{root}.Feeds[i]": function(feed: FeedModel)
                        {
                            feed.FormattedMsgs = ko.computed(() => VRS.stringUtility.format('{0:N0}', feed.Msgs()));
                            feed.FormattedBadMsgs = ko.computed(() => VRS.stringUtility.format('{0:N0}', feed.BadMsgs()));
                            feed.FormattedTracked = ko.computed(() => VRS.stringUtility.format('{0:N0}', feed.Tracked()));
                        },
                        "{root}.Requests[i]": function(request: RequestModel)
                        {
                            request.FormattedBytes = ko.computed(() => VRS.stringUtility.format('{0:N0}', request.Bytes()));
                        },
                        "{root}.Rebroadcasters[i]": function(rebroadcast: RebroadcastServerModel)
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
