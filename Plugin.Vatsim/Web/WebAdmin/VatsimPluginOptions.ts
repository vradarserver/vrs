namespace VRS.WebAdmin.VatsimPluginOptions
{
    import ViewJson = VirtualRadar.Plugin.Vatsim.WebAdmin;

    interface ViewModel extends ViewJson.IOptionsModel_KO
    {
        SaveAttempted?:         KnockoutObservable<boolean>;
        SaveSuccessful?:        KnockoutObservable<boolean>;
        SavedMessage?:          KnockoutObservable<string>;
        SelectedGeofencedFeed?: KnockoutObservable<GeofenceFeedModel>;
    }

    interface GeofenceFeedModel extends ViewJson.IGeofenceFeedOptionModel_KO
    {
        FormattedCentreOn?:         KnockoutComputed<string>;
        FormattedDistanceUnit?:     KnockoutComputed<string>;
        ConditionalLatitude?:       KnockoutComputed<number>;
        ConditionalLongitude?:      KnockoutComputed<number>;
        ConditionalAirportCode?:    KnockoutComputed<string>;
        ConditionalPilotCid?:       KnockoutComputed<number>;

        SelectRow?:             (row: GeofenceFeedModel) => void;
        DeleteRow?:             (row: GeofenceFeedModel) => void;
    }

    export class PageHandler
    {
        private _Model: ViewModel;
        private _ViewId: ViewId;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('VatsimPluginOptions', viewId);
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
                success: (data: IResponse<ViewJson.IOptionsModel>) => {
                    this.applyState(data);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            }, false);
        }

        createAndEditGeofencedFeed()
        {
            this._Model.GeofencedFeeds.pushFromModel(<ViewJson.IGeofenceFeedOptionModel> {
                FeedName: 'New Feed',
                CentreOn: 0,
                DistanceUnit: 0,
                Height: 0.0,
                Width: 0.0,
                Latitude: 0.0,
                Longitude: 0.0,
                PilotCid: null,
                AirportCode: null,
                ID: null
            });
            var length = this._Model.GeofencedFeeds().length;
            var newModel = this._Model.GeofencedFeeds()[length - 1];
            this._Model.SelectedGeofencedFeed(newModel);

            $('#edit-geofenced-feed').modal('show');
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
                        this._Model.SaveSuccessful(data.Response.Outcome === 'Saved');
                        switch(data.Response.Outcome || '') {
                            case 'Saved':               this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved); break;
                            case 'FailedValidation':    this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Validation_Failed); break;
                            case 'ConflictingUpdate':   this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update); break;
                        }
                    }
                    ko.viewmodel.updateFromModel(this._Model, data.Response.Options);
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
                    optionsModel: JSON.stringify(viewModel)
                },
                dataType: 'json',
                error: (jqXHR: JQueryXHR, textStatus: string, errorThrown: string) => {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Send_Failed, errorThrown));
                }
            };

            return result;
        }

        private applyState(state: IResponse<ViewJson.IOptionsModel>)
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
                                root.SelectedGeofencedFeed = <KnockoutObservable<GeofenceFeedModel>>ko.observable();
                            },

                            '{root}.GeofencedFeeds[i]': (model: GeofenceFeedModel) =>
                            {
                                model.FormattedCentreOn = ko.computed(() => this._ViewId.describeEnum(model.CentreOn(), state.Response.CentreOnTypes));
                                model.FormattedDistanceUnit = ko.computed(() => this._ViewId.describeEnum(model.DistanceUnit(), state.Response.DistanceUnitTypes));

                                model.ConditionalLatitude = ko.computed(() => model.CentreOn() != 0 ? null : model.Latitude());
                                model.ConditionalLongitude = ko.computed(() => model.CentreOn() != 0 ? null : model.Longitude());
                                model.ConditionalPilotCid = ko.computed(() => model.CentreOn() != 1 ? null : model.PilotCid());
                                model.ConditionalAirportCode = ko.computed(() => model.CentreOn() != 2 ? null : (model.AirportCode() || '').toUpperCase());

                                model.SelectRow = (row: GeofenceFeedModel) => {
                                    this._Model.SelectedGeofencedFeed(row);
                                };
                                model.DeleteRow = (row: GeofenceFeedModel) => {
                                    var index = this._Model.GeofencedFeeds().indexOf(row);
                                    if(index !== -1) {
                                        this._Model.GeofencedFeeds.splice(index, 1);
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
