var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var About;
        (function (About) {
            var PageHandler = (function () {
                function PageHandler() {
                    this._ViewId = new WebAdmin.ViewId('About');
                    this.refreshState();
                }
                PageHandler.prototype.refreshState = function () {
                    var _this = this;
                    this._ViewId.ajax('GetState', {
                        success: function (data) {
                            _this.applyState(data);
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
                            extend: {
                                "{root}": function (root) {
                                    root.Environment = ko.computed(function () { return VRS.stringUtility.format(root.IsMono() ? VRS.Server.$$.EnvironmentMono : VRS.Server.$$.EnvironmentDotNet, root.Is64BitProcess ? '64' : '32'); });
                                    root.FormattedDescription = ko.computed(function () { return root.Description().replace(/(?:\r\n|\r|\n)/g, '<br />'); });
                                }
                            }
                        });
                        ko.applyBindings(this._Model);
                    }
                };
                return PageHandler;
            }());
            About.PageHandler = PageHandler;
        })(About = WebAdmin.About || (WebAdmin.About = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=About.js.map