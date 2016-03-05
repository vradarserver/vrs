var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Queues;
        (function (Queues) {
            var PageHandler = (function () {
                function PageHandler() {
                    this._ViewId = new WebAdmin.ViewId('Queues');
                    this.refreshState();
                }
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
                    }, false);
                };
                PageHandler.prototype.applyState = function (state) {
                    if (this._Model) {
                        ko.viewmodel.updateFromModel(this._Model, state.Response);
                    }
                    else {
                        this._Model = ko.viewmodel.fromModel(state.Response, {
                            arrayChildId: {
                                '{root}.Queues': 'Name'
                            },
                            extend: {
                                "{root}.Queues[i]": function (model) {
                                    model.FormattedCountQueuedItems = ko.computed(function () { return VRS.stringUtility.formatNumber(model.CountQueuedItems(), 'N0'); });
                                    model.FormattedPeakQueuedItems = ko.computed(function () { return VRS.stringUtility.formatNumber(model.PeakQueuedItems(), 'N0'); });
                                    model.FormattedCountDroppedItems = ko.computed(function () { return VRS.stringUtility.formatNumber(model.CountDroppedItems(), 'N0'); });
                                }
                            }
                        });
                        ko.applyBindings(this._Model);
                    }
                };
                return PageHandler;
            })();
            Queues.PageHandler = PageHandler;
        })(Queues = WebAdmin.Queues || (WebAdmin.Queues = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=Queues.js.map