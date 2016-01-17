namespace VRS.WebAdmin.AircraftDetailLookupLog
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.AircraftOnlineLookupLog;

    interface Model extends ViewJson.IViewModel_KO
    {
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
                url: 'AircraftDetailLookupLog/GetState',
                success: (data: IResponse<ViewJson.IViewModel>) => {
                    this.applyState(data);
                    setTimeout(() => this.refreshState(), 1000);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
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
                        '{root}.LogEntries': 'Icao'
                    }
                });
                ko.applyBindings(this._Model);
            }
        }
    }
}
