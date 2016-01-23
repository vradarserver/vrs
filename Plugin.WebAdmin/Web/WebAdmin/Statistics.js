var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Statistics;
        (function (Statistics) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    this._FeedId = -1;
                    this._ViewId = new WebAdmin.ViewId('Statistics', viewId);
                    var feedId = Number($.url().param('feedId'));
                    if (!isNaN(feedId)) {
                        this._FeedId = feedId;
                    }
                    this.registerFeedId();
                }
                PageHandler.prototype.registerFeedId = function () {
                    var _this = this;
                    this._ViewId.ajax('RegisterFeedId', {
                        data: {
                            feedId: this._FeedId
                        },
                        success: function () {
                            _this.refreshState();
                        },
                        error: function () {
                            setTimeout(function () { return _this.registerFeedId(); }, 5000);
                        }
                    });
                };
                PageHandler.prototype.refreshState = function () {
                    var _this = this;
                    this._ViewId.ajax('GetState', {
                        success: function (data) {
                            _this.applyState(data);
                            setTimeout(function () { return _this.refreshState(); }, 1000);
                        },
                        error: function () {
                            setTimeout(function () { return _this.refreshState(); }, 5000);
                        }
                    });
                };
                PageHandler.prototype.resetCounters = function () {
                    this._ViewId.ajax('RaiseResetCountersClicked');
                };
                PageHandler.prototype.applyState = function (state) {
                    if (this._Model) {
                        ko.viewmodel.updateFromModel(this._Model, state.Response);
                    }
                    else {
                        this._Model = ko.viewmodel.fromModel(state.Response, {
                            arrayChildId: {
                                '{root}.ModeSDFCount': 'DF',
                                '{root}.AdsbMessageTypeCount': 'N',
                                '{root}.AdsbMessageFormatCount': 'Fmt'
                            },
                            extend: {
                                '{root}': function (root) {
                                    root.FormattedBytesReceived = ko.computed(function () { return VRS.stringUtility.formatNumber(root.BytesReceived(), 'N0'); });
                                    root.FormattedReceiverThroughput = ko.computed(function () { return VRS.stringUtility.format('{0:N2} {1}', root.ReceiverThroughput(), VRS.Server.$$.AcronymKilobytePerSecond); });
                                    root.FormattedReceiverBadChecksum = ko.computed(function () { return VRS.stringUtility.formatNumber(root.ReceiverBadChecksum(), 'N0'); });
                                    root.FormattedCurrentBufferSize = ko.computed(function () { return VRS.stringUtility.formatNumber(root.CurrentBufferSize(), 'N0'); });
                                    root.FormattedBaseStationMessages = ko.computed(function () { return VRS.stringUtility.formatNumber(root.BaseStationMessages(), 'N0'); });
                                    root.FormattedBadlyFormedBaseStationMessages = ko.computed(function () { return VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.BadlyFormedBaseStationMessages(), root.BadlyFormedBaseStationMessagesRatio() * 100); });
                                    root.FormattedModeSMessageCount = ko.computed(function () { return VRS.stringUtility.formatNumber(root.ModeSMessageCount(), 'N0'); });
                                    root.FormattedModeSNoAdsbPayload = ko.computed(function () { return VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.ModeSNoAdsbPayload(), root.ModeSNoAdsbPayloadRatio() * 100); });
                                    root.FormattedModeSShortFrame = ko.computed(function () { return VRS.stringUtility.formatNumber(root.ModeSShortFrame(), 'N0'); });
                                    root.FormattedModeSShortFrameUnusable = ko.computed(function () { return VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.ModeSShortFrameUnusable(), root.ModeSShortFrameUnusableRatio() * 100); });
                                    root.FormattedModeSLongFrame = ko.computed(function () { return VRS.stringUtility.formatNumber(root.ModeSLongFrame(), 'N0'); });
                                    root.FormattedModeSWithPI = ko.computed(function () { return VRS.stringUtility.formatNumber(root.ModeSWithPI(), 'N0'); });
                                    root.FormattedModeSPIBadParity = ko.computed(function () { return VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.ModeSPIBadParity(), root.ModeSPIBadParityRatio() * 100); });
                                    root.FormattedAdsbMessages = ko.computed(function () { return VRS.stringUtility.formatNumber(root.AdsbMessages(), 'N0'); });
                                    root.FormattedAdsbRejected = ko.computed(function () { return VRS.stringUtility.format('{0:N0} ({1:N2}%)', root.AdsbRejected(), root.AdsbRejectedRatio() * 100); });
                                    root.FormattedPositionSpeedCheckExceeded = ko.computed(function () { return VRS.stringUtility.formatNumber(root.PositionSpeedCheckExceeded(), 'N0'); });
                                    root.FormattedPositionsReset = ko.computed(function () { return VRS.stringUtility.formatNumber(root.PositionsReset(), 'N0'); });
                                    root.FormattedPositionsOutOfRange = ko.computed(function () { return VRS.stringUtility.formatNumber(root.PositionsOutOfRange(), 'N0'); });
                                },
                                '{root}.ModeSDFCount[i]': function (model) {
                                    model.FormattedVal = ko.computed(function () { return VRS.stringUtility.formatNumber(model.Val(), 'N0'); });
                                },
                                '{root}.AdsbMessageTypeCount[i]': function (model) {
                                    model.FormattedVal = ko.computed(function () { return VRS.stringUtility.formatNumber(model.Val(), 'N0'); });
                                },
                                '{root}.AdsbMessageFormatCount[i]': function (model) {
                                    model.FormattedVal = ko.computed(function () { return VRS.stringUtility.formatNumber(model.Val(), 'N0'); });
                                }
                            }
                        });
                        ko.applyBindings(this._Model);
                    }
                };
                return PageHandler;
            })();
            Statistics.PageHandler = PageHandler;
        })(Statistics = WebAdmin.Statistics || (WebAdmin.Statistics = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=Statistics.js.map