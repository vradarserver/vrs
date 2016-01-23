namespace VRS.WebAdmin.ConnectorActivityLog
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.ConnectorActivityLog;

    interface Model extends ViewJson.IViewModel_KO
    {
        SelectedConnector?: KnockoutObservable<ViewJson.IConnectorModel_KO>;
    }

    interface EventModel extends ViewJson.IEventModel_KO
    {
        IsSelectedConnector?: KnockoutComputed<boolean>;
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId = new ViewId('ConnectorActivityLog');

        constructor()
        {
            this.refreshState(() => {
                var connectorName = $.url().param('connectorName');
                if(connectorName) {
                    var connector = this.findConnectorByName(connectorName);
                    if(connector) {
                        this._Model.SelectedConnector(connector);
                    }
                }
            });
        }

        private findConnectorByName(name: string) : ViewJson.IConnectorModel_KO
        {
            return VRS.arrayHelper.findFirst(this._Model.Connectors(), (connector) => {
                return connector.Name() === name;
            });
        }

        refreshState(callback: () => void = null)
        {
            this._ViewId.ajax('GetState', {
                success: (data: IResponse<ViewJson.IViewModel>) => {
                    this.applyState(data);
                    if(callback !== null) {
                        callback();
                    }
                    setTimeout(() => this.refreshState(), 1000);
                },
                error: () => {
                    setTimeout(() => this.refreshState(callback), 5000);
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
                        '{root}.Events':        'Id',
                        '{root}.Connectors':    'Name'
                    },

                    extend: {
                        '{root}': function(root: Model)
                        {
                            root.SelectedConnector = <KnockoutObservable<ViewJson.IConnectorModel_KO>> ko.observable();
                        },

                        '{root}.Events[i]': (event: EventModel) =>
                        {
                            event.IsSelectedConnector = ko.computed({
                                read: () => this.IsSelectedConnector(event),
                                deferEvaluation: true
                            });
                        }
                    }
                });
                ko.applyBindings(this._Model);
            }
        }

        private IsSelectedConnector(event: EventModel) : boolean
        {
            var result = true;
            var selectedConnector = this._Model.SelectedConnector();
            if(selectedConnector) {
                result = selectedConnector.Name() === event.ConnectorName();
            }

            return result;
        }
    }
}
