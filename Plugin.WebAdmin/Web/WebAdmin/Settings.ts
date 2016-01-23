namespace VRS.WebAdmin.Settings
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Settings;

    interface Model extends ViewJson.IViewModel_KO
    {
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId: ViewId;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('Settings', viewId);

            this.refreshState();
        }

        refreshState()
        {
            this.showFailureMessage(null);

            this._ViewId.ajax('GetState', {
                success: (data: IResponse<ViewJson.IConfigurationModel>) => {
                    this.applyState(data);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            });
        }

        save()
        {
            var configuration = ko.viewmodel.toModel(this._Model.Configuration);
            this._ViewId.ajax('RaiseSaveClicked', {
                method: 'POST',
                data: {
                    configurationModel: JSON.stringify(configuration)
                },
                dataType: 'json',
                success: (data: IResponse<ViewJson.IConfigurationModel>) => {
                    this.applyState(data);
                },
                error: (jqXHR: JQueryXHR, textStatus: string, errorThrown: string) => {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                }
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

        private applyState(state: IResponse<ViewJson.IConfigurationModel>)
        {
            if(state.Exception) {
                this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, state.Exception));
            } else {
                this.showFailureMessage(null);

                if(this._Model) {
                    ko.viewmodel.updateFromModel(this._Model, state.Response);
                } else {
                    this._Model = ko.viewmodel.fromModel(state.Response, {
                        extend: {
                            '{root}': function(root: Model)
                            {
                            }
                        }
                    });
                    ko.applyBindings(this._Model);
                }
            }
        }
    }
}