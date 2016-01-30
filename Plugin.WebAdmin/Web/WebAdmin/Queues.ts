namespace VRS.WebAdmin.Queues
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Queues;

    interface Model extends ViewJson.IViewModel_KO
    {
    }

    interface QueueModel extends ViewJson.IQueueModel_KO
    {
        FormattedCountQueuedItems?:     KnockoutComputed<string>;
        FormattedPeakQueuedItems?:      KnockoutComputed<string>;
        FormattedCountDroppedItems?:    KnockoutComputed<string>;
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId = new ViewId('Queues');

        constructor()
        {
            this.refreshState();
        }

        refreshState()
        {
            this._ViewId.ajax('GetState', {
                success: (data: IResponse<ViewJson.IViewModel>) => {
                    this.applyState(data);
                    setTimeout(() => this.refreshState(), 1000);
                },
                error: () => {
                    setTimeout(() => this.refreshState(), 5000);
                }
            }, false);
        }

        private applyState(state: IResponse<ViewJson.IViewModel>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(this._Model, state.Response);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response, {
                    arrayChildId: {
                        '{root}.Queues': 'Name'
                    },

                    extend: {
                        "{root}.Queues[i]": function(model: QueueModel)
                        {
                            model.FormattedCountQueuedItems = ko.computed(() => VRS.stringUtility.formatNumber(model.CountQueuedItems(), 'N0'));
                            model.FormattedPeakQueuedItems = ko.computed(() => VRS.stringUtility.formatNumber(model.PeakQueuedItems(), 'N0'));
                            model.FormattedCountDroppedItems = ko.computed(() => VRS.stringUtility.formatNumber(model.CountDroppedItems(), 'N0'));
                        }
                    }
                });
                ko.applyBindings(this._Model);
            }
        }
    }
}
