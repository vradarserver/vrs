var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var AircraftDetailLookupLog;
        (function (AircraftDetailLookupLog) {
            var PageHandler = (function () {
                function PageHandler() {
                    this._ViewId = new WebAdmin.ViewId('AircraftDetailLookupLog');
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
                                '{root}.LogEntries': 'Icao'
                            }
                        });
                        ko.applyBindings(this._Model);
                    }
                };
                return PageHandler;
            })();
            AircraftDetailLookupLog.PageHandler = PageHandler;
        })(AircraftDetailLookupLog = WebAdmin.AircraftDetailLookupLog || (WebAdmin.AircraftDetailLookupLog = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=AircraftDetailLookupLog.js.map