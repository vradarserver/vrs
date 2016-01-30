namespace VRS.WebAdmin.Statistics
{
    import ViewJson = VirtualRadar.Plugin.WebAdmin.View.Statistics;

    interface Model extends ViewJson.IViewModel_KO
    {
        FormattedBytesReceived?:                        KnockoutComputed<string>;
        FormattedReceiverThroughput?:                   KnockoutComputed<string>;
        FormattedReceiverBadChecksum?:                  KnockoutComputed<string>;
        FormattedCurrentBufferSize?:                    KnockoutComputed<string>;
        FormattedBaseStationMessages?:                  KnockoutComputed<string>;
        FormattedBadlyFormedBaseStationMessages?:       KnockoutComputed<string>;
        FormattedModeSMessageCount?:                    KnockoutComputed<string>;
        FormattedModeSNoAdsbPayload?:                   KnockoutComputed<string>;
        FormattedModeSShortFrame?:                      KnockoutComputed<string>;
        FormattedModeSShortFrameUnusable?:              KnockoutComputed<string>;
        FormattedModeSLongFrame?:                       KnockoutComputed<string>;
        FormattedModeSWithPI?:                          KnockoutComputed<string>;
        FormattedModeSPIBadParity?:                     KnockoutComputed<string>;
        FormattedAdsbMessages?:                         KnockoutComputed<string>;
        FormattedAdsbRejected?:                         KnockoutComputed<string>;
        FormattedPositionSpeedCheckExceeded?:           KnockoutComputed<string>;
        FormattedPositionsReset?:                       KnockoutComputed<string>;
        FormattedPositionsOutOfRange?:                  KnockoutComputed<string>;
    }

    interface ModeSDFCountModel extends ViewJson.IModeSDFCountModel_KO
    {
        FormattedVal?: KnockoutComputed<string>;
    }

    interface AdsbMessageTypeCountModel extends ViewJson.IAdsbMessageTypeCountModel_KO
    {
        FormattedVal?: KnockoutComputed<string>;
    }

    interface AdsbMessageFormatCountModel extends ViewJson.IAdsbMessageFormatCountModel_KO
    {
        FormattedVal?: KnockoutComputed<string>;
    }

    export class PageHandler
    {
        private _Model: Model;
        private _ViewId: ViewId;
        private _FeedId: number = -1;

        constructor(viewId: string)
        {
            this._ViewId = new ViewId('Statistics', viewId);

            var feedId = Number($.url().param('feedId'));
            if(!isNaN(feedId)) {
                this._FeedId = feedId;
            }

            this.registerFeedId();
        }

        private registerFeedId()
        {
            this._ViewId.ajax('RegisterFeedId', {
                data: {
                    feedId: this._FeedId
                },
                success: () => {
                    this.refreshState();
                },
                error: () => {
                    setTimeout(() => this.registerFeedId(), 5000);
                }
            });
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

        resetCounters()
        {
            this._ViewId.ajax('RaiseResetCountersClicked');
        }

        private applyState(state: IResponse<ViewJson.IViewModel>)
        {
            if(this._Model) {
                ko.viewmodel.updateFromModel(this._Model, state.Response);
            } else {
                this._Model = ko.viewmodel.fromModel(state.Response, {
                    arrayChildId: {
                        '{root}.ModeSDFCount':              'DF',
                        '{root}.AdsbMessageTypeCount':      'N',
                        '{root}.AdsbMessageFormatCount':    'Fmt'
                    },

                    extend: {
                        '{root}': function(root: Model)
                        {
                            root.FormattedBytesReceived = ko.computed(() => VRS.stringUtility.formatNumber(root.BytesReceived(), 'N0'));
                            root.FormattedReceiverThroughput = ko.computed(() => VRS.stringUtility.format('{0:N2} {1}', root.ReceiverThroughput(), VRS.Server.$$.AcronymKilobytePerSecond));
                            root.FormattedReceiverBadChecksum = ko.computed(() => VRS.stringUtility.formatNumber(root.ReceiverBadChecksum(), 'N0'));
                            root.FormattedCurrentBufferSize = ko.computed(() => VRS.stringUtility.formatNumber(root.CurrentBufferSize(), 'N0'));
                            root.FormattedBaseStationMessages = ko.computed(() => VRS.stringUtility.formatNumber(root.BaseStationMessages(), 'N0'));
                            root.FormattedBadlyFormedBaseStationMessages = ko.computed(() => VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.BadlyFormedBaseStationMessages(), root.BadlyFormedBaseStationMessagesRatio() * 100));
                            root.FormattedModeSMessageCount = ko.computed(() => VRS.stringUtility.formatNumber(root.ModeSMessageCount(), 'N0'));
                            root.FormattedModeSNoAdsbPayload = ko.computed(() => VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.ModeSNoAdsbPayload(), root.ModeSNoAdsbPayloadRatio() * 100));
                            root.FormattedModeSShortFrame = ko.computed(() => VRS.stringUtility.formatNumber(root.ModeSShortFrame(), 'N0'));
                            root.FormattedModeSShortFrameUnusable = ko.computed(() => VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.ModeSShortFrameUnusable(), root.ModeSShortFrameUnusableRatio() * 100));
                            root.FormattedModeSLongFrame = ko.computed(() => VRS.stringUtility.formatNumber(root.ModeSLongFrame(), 'N0'));
                            root.FormattedModeSWithPI = ko.computed(() => VRS.stringUtility.formatNumber(root.ModeSWithPI(), 'N0'));
                            root.FormattedModeSPIBadParity = ko.computed(() => VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.ModeSPIBadParity(), root.ModeSPIBadParityRatio() * 100));
                            root.FormattedAdsbMessages = ko.computed(() => VRS.stringUtility.formatNumber(root.AdsbMessages(), 'N0'));
                            root.FormattedAdsbRejected = ko.computed(() => VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.AdsbRejected(), root.AdsbRejectedRatio() * 100));
                            root.FormattedPositionSpeedCheckExceeded = ko.computed(() => VRS.stringUtility.formatNumber(root.PositionSpeedCheckExceeded(), 'N0'));
                            root.FormattedPositionsReset = ko.computed(() => VRS.stringUtility.formatNumber(root.PositionsReset(), 'N0'));
                            root.FormattedPositionsOutOfRange = ko.computed(() => VRS.stringUtility.formatNumber(root.PositionsOutOfRange(), 'N0'));
                        },

                        '{root}.ModeSDFCount[i]' : function(model: ModeSDFCountModel)
                        {
                            model.FormattedVal = ko.computed(() => VRS.stringUtility.formatNumber(model.Val(), 'N0'));
                        },

                        '{root}.AdsbMessageTypeCount[i]' : function(model: AdsbMessageTypeCountModel)
                        {
                            model.FormattedVal = ko.computed(() => VRS.stringUtility.formatNumber(model.Val(), 'N0'));
                        },

                        '{root}.AdsbMessageFormatCount[i]' : function(model: AdsbMessageFormatCountModel)
                        {
                            model.FormattedVal = ko.computed(() => VRS.stringUtility.formatNumber(model.Val(), 'N0'));
                        }
                    }
                });
                ko.applyBindings(this._Model);
            }
        }
    }
}
