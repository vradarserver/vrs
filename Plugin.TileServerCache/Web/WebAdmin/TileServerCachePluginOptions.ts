namespace VRS.WebAdmin.TileServerCachePluginOptions
{
    import ViewJson = VirtualRadar.Plugin.TileServerCache.WebAdmin;

    interface ViewModel extends ViewJson.IViewModel_KO
    {
        SaveAttempted?:     KnockoutObservable<boolean>;
        SaveSuccessful?:    KnockoutObservable<boolean>;
        SavedMessage?:      KnockoutObservable<string>;
    }

    export class PageHandler
    {
        private _Model: ViewModel;
        private _ViewId: ViewId;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('TileServerCachePluginOptions', viewId);
            this.refreshState();
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

        save()
        {
            this._Model.SaveAttempted(false);

            var ajaxSettings = this.buildAjaxSettingsForSendConfiguration();
            ajaxSettings.success = (data: IResponse<ViewJson.ISaveOutcomeModel>) => {
                if(data.Exception) {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, data.Exception));
                    this._Model.SaveSuccessful(false);
                } else {
                    if(data.Response && data.Response.Outcome) {
                        this._Model.SaveAttempted(true);
                        this._Model.SaveSuccessful(data.Response.Outcome === "Saved");
                        switch(data.Response.Outcome || "") {
                            case "Saved":               this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved); break;
                            case "FailedValidation":    this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Validation_Failed); break;
                            case "ConflictingUpdate":   this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update); break;
                        }
                    }
                    ko.viewmodel.updateFromModel(this._Model, data.Response.ViewModel);
                }
            };

            this._ViewId.ajax('Save', ajaxSettings);
        }

        private buildAjaxSettingsForSendConfiguration() : JQueryAjaxSettings
        {
            var viewModel = ko.viewmodel.toModel(this._Model);
            var result = {
                method: 'POST',
                data: {
                    viewModel: JSON.stringify(viewModel)
                },
                dataType: 'json',
                error: (jqXHR: JQueryXHR, textStatus: string, errorThrown: string) => {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                }
            };

            return result;
        }

        private applyState(state: IResponse<ViewJson.IViewModel>)
        {
            if(state.Exception) {
                this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, state.Exception));
            } else {
                this.showFailureMessage(null);

                if(this._Model) {
                    ko.viewmodel.updateFromModel(this._Model, state.Response);
                } else {
                    this._Model = ko.viewmodel.fromModel(state.Response, {
                        arrayChildId: {
                        },

                        extend: {
                            '{root}': (root: ViewModel) =>
                            {
                                root.SaveAttempted = ko.observable(false);
                                root.SaveSuccessful = ko.observable(false);
                                root.SavedMessage = ko.observable('');
                            }
                        }
                    });

                    ko.applyBindings(this._Model);
                }
            }
        }
    }
}
