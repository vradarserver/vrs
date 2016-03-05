var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var Log;
        (function (Log) {
            var PageHandler = (function () {
                function PageHandler() {
                    this._ViewId = new WebAdmin.ViewId('Log');
                    this._ScrollToEnd = $('#scrollToEnd');
                    this._ScrollToTop = $('#scrollToTop');
                    this._ScrollToEnd.on('click', function () {
                        $('html, body').animate({
                            scrollTop: $(document).height()
                        }, 'fast');
                        return false;
                    }).hide();
                    this._ScrollToTop.on('click', function () {
                        $('html, body').animate({
                            scrollTop: 0
                        }, 'fast');
                        return false;
                    }).hide();
                    this.refreshState();
                }
                PageHandler.prototype.refreshState = function () {
                    var _this = this;
                    this._ViewId.ajax('GetState', {
                        success: function (data) {
                            _this.applyState(data);
                            _this._ScrollToEnd.show();
                            _this._ScrollToTop.show();
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
                        this._Model = ko.viewmodel.fromModel(state.Response);
                        ko.applyBindings(this._Model);
                    }
                };
                return PageHandler;
            })();
            Log.PageHandler = PageHandler;
        })(Log = WebAdmin.Log || (WebAdmin.Log = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
