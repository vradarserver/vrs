var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var ConnectorActivityLog;
        (function (ConnectorActivityLog) {
            var PageHandler = (function () {
                function PageHandler() {
                    var _this = this;
                    this.refreshState(function () {
                        var selectConnector = $.url().param('connectorName');
                        if (selectConnector) {
                            var connector = VRS.arrayHelper.findFirst(_this._Model.Connectors(), function (connector) {
                                return connector.Name() === selectConnector;
                            });
                            if (connector) {
                                _this._Model.SelectedConnector(connector);
                            }
                        }
                    });
                }
                PageHandler.prototype.refreshState = function (callback) {
                    var _this = this;
                    if (callback === void 0) { callback = null; }
                    $.ajax({
                        url: 'ConnectorActivityLog/GetState',
                        success: function (data) {
                            _this.applyState(data);
                            if (callback !== null) {
                                callback();
                            }
                            setTimeout(function () { return _this.refreshState(); }, 1000);
                        },
                        error: function () {
                            setTimeout(function () { return _this.refreshState(callback); }, 5000);
                        }
                    });
                };
                PageHandler.prototype.applyState = function (state) {
                    var _this = this;
                    if (this._Model) {
                        ko.viewmodel.updateFromModel(this._Model, state.Response);
                    }
                    else {
                        this._Model = ko.viewmodel.fromModel(state.Response, {
                            arrayChildId: {
                                '{root}.Events': 'Id',
                                '{root}.Connectors': 'Name'
                            },
                            extend: {
                                '{root}': function (root) {
                                    root.SelectedConnector = ko.observable();
                                },
                                '{root}.Events[i]': function (event) {
                                    event.IsSelectedConnector = ko.computed({
                                        read: function () { return _this.IsSelectedConnector(event); },
                                        deferEvaluation: true
                                    });
                                }
                            }
                        });
                        ko.applyBindings(this._Model);
                    }
                };
                PageHandler.prototype.IsSelectedConnector = function (event) {
                    var result = true;
                    var selectedConnector = this._Model.SelectedConnector();
                    if (selectedConnector) {
                        result = selectedConnector.Name() === event.ConnectorName();
                    }
                    return result;
                };
                return PageHandler;
            })();
            ConnectorActivityLog.PageHandler = PageHandler;
        })(ConnectorActivityLog = WebAdmin.ConnectorActivityLog || (WebAdmin.ConnectorActivityLog = {}));
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=ConnectorActivityLog.js.map