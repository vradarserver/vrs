namespace VRS.WebAdmin.Settings
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Settings;

    interface Model extends ViewJson.IConfigurationModel_KO
    {
        saveAttempted?:     KnockoutObservable<boolean>;
        saveSuccessful?:    KnockoutObservable<boolean>;
        savedMessage?:      KnockoutComputed<string>;
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId: ViewId;
        private _Validation = new Validation();
        private _SaveAttempted = false;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('Settings', viewId);

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

        save()
        {
            this._Model.saveAttempted(false);

            var configuration = ko.viewmodel.toModel(this._Model);
            this._ViewId.ajax('RaiseSaveClicked', {
                method: 'POST',
                data: {
                    configurationModel: JSON.stringify(configuration)
                },
                dataType: 'json',
                success: (data: IResponse<ViewJson.IViewModel>) => {
                    this.applyState(data);
                    this._Model.saveAttempted(true);
                    this._Model.saveSuccessful(!this._Validation.HasError);
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

        private applyState(state: IResponse<ViewJson.IViewModel>)
        {
            if(state.Exception) {
                this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, state.Exception));
            } else {
                this.showFailureMessage(null);
                this._Validation.updateValidationResults(state.Response.ValidationResults);

                if(this._Model) {
                    ko.viewmodel.updateFromModel(this._Model, state.Response.Configuration);
                } else {
                    this._Model = ko.viewmodel.fromModel(state.Response.Configuration, {
                        extend: {
                            '{root}': (root: Model) =>
                            {
                                root.saveAttempted = ko.observable(false);
                                root.saveSuccessful = ko.observable(false);
                                root.savedMessage = ko.computed(() => root.saveSuccessful() ? VRS.WebAdmin.$$.WA_Saved : VRS.WebAdmin.$$.WA_Validation_Failed);
                            }
                        }
                    });
                    ko.applyBindings(this._Model);
                }

                this._Validation.updateFields();
            }
        }
    }
}