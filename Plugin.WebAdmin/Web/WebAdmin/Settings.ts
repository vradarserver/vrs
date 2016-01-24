namespace VRS.WebAdmin.Settings
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Settings;

    interface Model extends ViewJson.IConfigurationModel_KO
    {
        saveAttempted?:     KnockoutObservable<boolean>;
        saveSuccessful?:    KnockoutObservable<boolean>;
        savedMessage?:      KnockoutObservable<string>;

        selectedReceiverLocation?:  KnockoutObservable<ReceiverLocationModel>;

        ReceiverLocationWrapUpValidation?:  IValidation_KC;
    }

    interface BaseStationSettingsModel extends ViewJson.IBaseStationSettingsModel_KO
    {
        WrapUpValidation?:      IValidation_KC;
    }

    interface ReceiverLocationModel extends ViewJson.IReceiverLocationModel_KO
    {
        FormattedLatitude?:     KnockoutComputed<string>;
        FormattedLongitude?:    KnockoutComputed<string>;
        WrapUpValidation?:      IValidation_KC;

        SelectRow?:             (row: ReceiverLocationModel) => void;
        DeleteRow?:             (row: ReceiverLocationModel) => void;
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId: ViewId;
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

        private sendAndApplyConfiguration(methodName: string, success: (state: IResponse<ViewJson.IViewModel>) => void, applyConfigurationFirst: boolean = true)
        {
            var configuration = ko.viewmodel.toModel(this._Model);
            this._ViewId.ajax(methodName, {
                method: 'POST',
                data: {
                    configurationModel: JSON.stringify(configuration)
                },
                dataType: 'json',
                success: (data: IResponse<ViewJson.IViewModel>) => {
                    if(applyConfigurationFirst) {
                        this.applyState(data);
                        success(data);
                    } else {
                        success(data);
                        this.applyState(data);
                    }
                },
                error: (jqXHR: JQueryXHR, textStatus: string, errorThrown: string) => {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                }
            });
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
                            '{root}.ReceiverLocations': 'UniqueId'
                        },

                        extend: {
                            '{root}': (root: Model) =>
                            {
                                root.saveAttempted = ko.observable(false);
                                root.saveSuccessful = ko.observable(false);
                                root.savedMessage = ko.observable("");

                                root.selectedReceiverLocation = <KnockoutObservable<ReceiverLocationModel>> ko.observable(null);
                            },

                            '{root}.BaseStationSettingsModel': (model: BaseStationSettingsModel) => {
                                model.WrapUpValidation = this._ViewId.createWrapupValidation(this._ViewId.findValidationProperties(model));
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

                    this._Model.ReceiverLocationWrapUpValidation = this._ViewId.createArrayWrapupValidation(this._Model.ReceiverLocations, (r) => (<ReceiverLocationModel>r).WrapUpValidation);

                    ko.applyBindings(this._Model);
                }
            }
        }
    }
}