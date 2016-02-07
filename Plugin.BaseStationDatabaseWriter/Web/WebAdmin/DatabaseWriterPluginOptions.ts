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