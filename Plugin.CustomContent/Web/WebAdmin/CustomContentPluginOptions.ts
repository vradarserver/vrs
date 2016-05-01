namespace VRS.WebAdmin.CustomContentPluginOptions
{
    import ViewJson = VirtualRadar.Plugin.CustomContent.WebAdmin;

    interface ViewModel extends ViewJson.IViewModel_KO
    {
        SaveAttempted?:             KnockoutObservable<boolean>;
        SaveSuccessful?:            KnockoutObservable<boolean>;
        SavedMessage?:              KnockoutObservable<string>;
        SelectedInjectSettings?:    KnockoutObservable<InjectSettingsModel>;
    }

    interface InjectSettingsModel extends ViewJson.IInjectSettingsModel_KO
    {
        WrapUpValidation?:              IValidation_KC;
        StartAsStringValue?:            KnockoutComputed<string>;
        FormattedStart?:                KnockoutComputed<string>;
        FormattedInjectionLocation?:    KnockoutComputed<string>;

        SelectRow?:                     (row: InjectSettingsModel) => void;
        DeleteRow?:                     (row: InjectSettingsModel) => void;
    }

    export class PageHandler
    {
        private _Model: ViewModel;
        private _ViewId: ViewId;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('CustomContentPluginOptions', viewId);
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

        createAndEditInjectSettings()
        {
            this._Model.InjectSettings.pushFromModel(<ViewJson.IInjectSettingsModel> {
                Enabled: true,
                PathAndFile: '/desktop.html',
                PathAndFileValidation: {
                    IsError: false,
                    IsValid: true,
                    IsWarning: false,
                    Message: ''
                },
                InjectionLocation: VirtualRadar.Plugin.CustomContent.InjectionLocation.Head,
                Start: false,
                File: '',
                FileValidation: {
                    IsError: false,
                    IsValid: true,
                    IsWarning: false,
                    Message: ''
                }
            });
            var length = this._Model.InjectSettings().length;
            var newModel = this._Model.InjectSettings()[length - 1];
            this._Model.SelectedInjectSettings(newModel);

            $('#edit-inject-settings').modal('show');
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
                                root.SelectedInjectSettings = <KnockoutObservable<InjectSettingsModel>>ko.observable();
                            },

                            '{root}.InjectSettings[i]': (model: InjectSettingsModel) =>
                            {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));

                                model.FormattedStart = ko.computed(() => {
                                    return model.Start() ? VRS.CustomContentPlugin.$$.Start : VRS.CustomContentPlugin.$$.End;
                                });
                                model.StartAsStringValue = ko.pureComputed({
                                    read: () => {
                                        return model.Start() ? "start" : "end";
                                    },
                                    write: (value: string) => {
                                        model.Start(value === "start");
                                    },
                                    owner: this
                                });
                                model.FormattedInjectionLocation = ko.computed(() => {
                                    switch(model.InjectionLocation()) {
                                        case VirtualRadar.Plugin.CustomContent.InjectionLocation.Body:
                                            return VRS.CustomContentPlugin.$$.Body;
                                        case VirtualRadar.Plugin.CustomContent.InjectionLocation.Head:
                                            return VRS.CustomContentPlugin.$$.Head;
                                        default:
                                            return VRS.Server.$$.Unknown;
                                    }
                                });

                                model.SelectRow = (row: InjectSettingsModel) => {
                                    this._Model.SelectedInjectSettings(row);
                                };
                                model.DeleteRow = (row: InjectSettingsModel) => {
                                    var index = this._Model.InjectSettings().indexOf(row);
                                    if(index !== -1) {
                                        this._Model.InjectSettings.splice(index, 1);
                                    }
                                };
                            }
                        }
                    });

                    ko.applyBindings(this._Model);
                }
            }
        }
    }
}
