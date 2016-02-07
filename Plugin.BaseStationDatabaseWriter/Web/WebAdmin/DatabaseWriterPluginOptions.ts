/// <reference path="../../typings/webadmin.d.ts" />
/// <reference path="../../typings/DatabaseWriterPluginModels.d.ts" />
/// <reference path="../../typings/translations-pluginstrings.d.ts" />

namespace VRS.WebAdmin.DatabaseWriterPluginOptions
{
    import ViewJson = VirtualRadar.Plugin.BaseStationDatabaseWriter.WebAdmin;

    interface ViewModel extends ViewJson.IViewModel_KO
    {
        OnlyUpdateDatabasesCreatedByPlugin?:    KnockoutComputed<boolean>;
        SelectedFeed?:                          KnockoutComputed<VirtualRadar.Plugin.BaseStationDatabaseWriter.ICombinedFeed_KO>;
        SaveAttempted?:                         KnockoutObservable<boolean>;
        SavedMessage?:                          KnockoutObservable<string>;
        OverwriteDetailsMessage?:               KnockoutComputed<string>;
        CreateDatabaseOutcomeTitle?:            KnockoutObservable<string>;
        CreateDatabaseOutcomeMessage?:          KnockoutObservable<string>;
    }

    export class PageHandler
    {
        private _Model: ViewModel;
        private _ViewId: ViewId;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('DatabaseWriterPluginOptions', viewId);
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

        useDefaultFileName()
        {
            var settings = this.buildAjaxSettingsForSendModel();
            settings.success = (viewModel: IResponse<ViewJson.IViewModel>) =>
            {
                this.applyState(viewModel);
            };
            this._ViewId.ajax('UseDefaultFileName', settings);
        }

        createDatabase()
        {
            this._Model.CreateDatabaseOutcomeMessage('');
            this._Model.CreateDatabaseOutcomeTitle('');

            var settings = this.buildAjaxSettingsForSendModel();
            settings.success = (outcome: IResponse<ViewJson.ICreateDatabaseOutcomeModel>) =>
            {
                if(outcome.Exception) {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, outcome.Exception));
                } else {
                    this.showFailureMessage(null);
                    this._Model.CreateDatabaseOutcomeMessage(outcome.Response.Message);
                    this._Model.CreateDatabaseOutcomeTitle(outcome.Response.Title);
                    ko.viewmodel.updateFromModel(this._Model, outcome.Response.ViewModel);
                    $('#create-database-outcome').modal('show');
                }
            };
            this._ViewId.ajax('CreateDatabase', settings);
        }

        save()
        {
            this._Model.SaveAttempted(false);
            this._Model.SavedMessage('');

            var settings = this.buildAjaxSettingsForSendModel();
            settings.success = (outcome: IResponse<ViewJson.ISaveOutcomeModel>) => {
                if(outcome.Exception) {
                    this.showFailureMessage(VRS.stringUtility.format(VRS.WebAdmin.$$.WA_Exception_Reported, outcome.Exception));
                } else {
                    this.showFailureMessage(null);
                    this._Model.SaveAttempted(true);
                    switch(outcome.Response.Outcome) {
                        case 'Saved':               this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Saved); break;
                        case 'ConflictingUpdate':   this._Model.SavedMessage(VRS.WebAdmin.$$.WA_Conflicting_Update); break;
                        default:                    this._Model.SavedMessage(VRS.stringUtility.format('Unexpected response "{0}"', outcome.Response.Outcome)); break;
                    }
                    ko.viewmodel.updateFromModel(this._Model, outcome.Response.ViewModel);
                }
            };
            this._ViewId.ajax('Save', settings);
        }

        private buildAjaxSettingsForSendModel() : JQueryAjaxSettings
        {
            var configuration = ko.viewmodel.toModel(this._Model);
            var result = {
                method: 'POST',
                data: {
                    viewModel: JSON.stringify(configuration)
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
                            CombinedFeeds: 'UniqueId'
                        },

                        extend: {
                            '{root}': (root: ViewModel) =>
                            {
                                root.SaveAttempted = ko.observable(false);
                                root.SavedMessage = ko.observable('');
                                root.CreateDatabaseOutcomeTitle = ko.observable('');
                                root.CreateDatabaseOutcomeMessage = ko.observable('');

                                root.OverwriteDetailsMessage = ko.computed(() => {
                                    return root.RefreshOutOfDateAircraft() ? VRS.DatabaseWriterPlugin.$$.WriteOnlineLookupsNoticeAllAircraft
                                                                           : VRS.DatabaseWriterPlugin.$$.WriteOnlineLookupsNoticeJustNew;
                                });
                                root.OnlyUpdateDatabasesCreatedByPlugin = ko.computed({
                                    read: () => !root.AllowUpdateOfOtherDatabases(),
                                    write: (value: boolean) => root.AllowUpdateOfOtherDatabases(!value),
                                    owner: this
                                });
                                root.SelectedFeed = ko.computed({
                                    read: () => VRS.arrayHelper.findFirst(root.CombinedFeeds(), r => r.UniqueId() === root.ReceiverId()),
                                    write: (value: VirtualRadar.Plugin.BaseStationDatabaseWriter.ICombinedFeed_KO) => root.ReceiverId(value ? value.UniqueId() : -1),
                                    owner: this
                                });
                            }
                        }
                    });

                    ko.applyBindings(this._Model);
                }
            }
        }
    }
}