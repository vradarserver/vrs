namespace VRS.WebAdmin.TileServerCacheRecentRequests
{
    import ViewJson = VirtualRadar.Plugin.TileServerCache.WebAdmin;

    interface Model extends ViewJson.IRecentRequestsViewModel_KO
    {
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId: ViewId;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('TileServerCacheRecentRequests', viewId);
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

        private applyState(state: IResponse<ViewJson.IViewModel>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(this._Model, state.Response);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response, {
                    arrayChildId: {
                        '{root}.RecentRequests': 'ID'
                    }
                });
                ko.applyBindings(this._Model);
            }
        }
    }
}
