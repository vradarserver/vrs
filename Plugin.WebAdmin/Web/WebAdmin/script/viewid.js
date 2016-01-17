var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var ViewId = (function () {
            function ViewId(viewName, viewId) {
                this._HeartbeatTimerId = null;
                this._LostContact = false;
                this._ViewName = viewName;
                this._Id = viewId;
                this.sendHeartbeat();
            }
            Object.defineProperty(ViewId.prototype, "Id", {
                get: function () {
                    return this._Id;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(ViewId.prototype, "ViewName", {
                get: function () {
                    return this._ViewName;
                },
                enumerable: true,
                configurable: true
            });
            ViewId.prototype.setHeartbeatTimer = function () {
                var _this = this;
                this._HeartbeatTimerId = setTimeout(function () {
                    _this.sendHeartbeat();
                }, 10000);
            };
            ViewId.prototype.sendHeartbeat = function () {
                var _this = this;
                this.ajax({
                    url: this._ViewName + '/BrowserHeartbeat',
                    success: function () {
                        _this.setHeartbeatTimer();
                    },
                    error: function () {
                        _this._LostContact = true;
                        var modalBackdrop = $('<div />')
                            .addClass('modal-alert')
                            .appendTo($('body'));
                        $('<div />')
                            .addClass('alert alert-danger text-center')
                            .text(VRS.WebAdmin.$$.WA_Lost_Contact)
                            .appendTo(modalBackdrop);
                    }
                });
            };
            ViewId.prototype.ajax = function (settings) {
                if (!this._LostContact) {
                    var data = settings.data || {};
                    data.__ViewId = this._Id;
                    settings.data = data;
                    return $.ajax(settings);
                }
            };
            return ViewId;
        })();
        WebAdmin.ViewId = ViewId;
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=viewid.js.map