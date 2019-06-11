var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var TileServerCacheRecentRequests;
        (function (TileServerCacheRecentRequests) {
            var PageHandler = (function () {
                function PageHandler(viewId) {
                    this._ViewId = new WebAdmin.ViewId('TileServerCacheRecentRequests', viewId);
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
                                '{root}.RecentRequests': 'ID'
                            }
                        });
                        ko.applyBindings(this._Model);
                    }
                };
                return PageHandler;
            }());
            TileServerCacheRecentRequests.PageHandler = PageHandler;
        })(TileServerCacheRecentRequests = WebAdmin.TileServerCacheRecentRequests || (WebAdmin.TileServerCacheRecentRequests = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=TileServerCacheRecentRequests.js.map