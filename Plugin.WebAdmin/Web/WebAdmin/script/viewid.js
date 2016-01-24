var VRS;
(function (VRS) {
    var WebAdmin;
    (function (WebAdmin) {
        var ViewId = (function () {
            function ViewId(viewName, viewId) {
                if (viewId === void 0) { viewId = null; }
                this._LostContact = false;
                this._FailedAttempts = 0;
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
            ViewId.prototype.setHeartbeatTimer = function (pauseInterval) {
                var _this = this;
                if (pauseInterval === void 0) { pauseInterval = 10000; }
                if (this._Id) {
                    setTimeout(function () {
                        _this.sendHeartbeat();
                    }, pauseInterval);
                }
            };
            ViewId.prototype.sendHeartbeat = function () {
                var _this = this;
                if (this._Id) {
                    this.ajax('BrowserHeartbeat', {
                        success: function () {
                            _this._FailedAttempts = 0;
                            _this.setHeartbeatTimer();
                        },
                        error: function () {
                            if (++_this._FailedAttempts <= 5) {
                                _this.setHeartbeatTimer(1000);
                            }
                            else {
                                _this._LostContact = true;
                                var modalBackdrop = $('<div />')
                                    .addClass('modal-alert')
                                    .appendTo($('body'));
                                $('<div />')
                                    .addClass('alert alert-danger text-center')
                                    .text(VRS.WebAdmin.$$.WA_Lost_Contact)
                                    .appendTo(modalBackdrop);
                            }
                        }
                    });
                }
            };
            ViewId.prototype.ajax = function (methodName, settings) {
                var _this = this;
                if (settings === void 0) { settings = {}; }
                if (!this._LostContact) {
                    if (methodName && !settings.url) {
                        settings.url = this._ViewName + '/' + methodName;
                    }
                    var data = settings.data || {};
                    if (this._Id) {
                        data.__ViewId = this._Id;
                    }
                    settings.data = data;
                    var success = settings.success || $.noop;
                    settings.success = function (response, textStatus, jqXHR) {
                        if (_this.isDeferredExecutionResponse(response)) {
                            _this.fetchDeferredExecutionResponse(response.Response.JobId, success, 200);
                        }
                        else {
                            success(response, textStatus, jqXHR);
                        }
                    };
                    return $.ajax(settings);
                }
            };
            ViewId.prototype.isDeferredExecutionResponse = function (response) {
                return response && response.Response && response.Response.DeferredExecution && response.Response.JobId;
            };
            ViewId.prototype.fetchDeferredExecutionResponse = function (jobId, success, interval) {
                var _this = this;
                if (!this._LostContact) {
                    setTimeout(function () { return _this.sendRequestForDeferredExecutionResponse(jobId, success); }, interval);
                }
            };
            ViewId.prototype.sendRequestForDeferredExecutionResponse = function (jobId, success) {
                var _this = this;
                this.ajax('GetDeferredResponse', {
                    data: {
                        jobId: jobId
                    },
                    success: function (response, textStatus, jqXHR) {
                        if (_this.isDeferredExecutionResponse(response)) {
                            _this.fetchDeferredExecutionResponse(jobId, success, 1000);
                        }
                        else {
                            success(response, textStatus, jqXHR);
                        }
                    },
                    error: function () {
                        _this.fetchDeferredExecutionResponse(jobId, success, 5000);
                    }
                });
            };
            ViewId.prototype.createWrapupValidation = function (validationFields) {
                var result = {
                    IsValid: ko.computed(function () {
                        var isValid = true;
                        $.each(validationFields, function (idx, validationField) {
                            if (!validationField.IsValid()) {
                                isValid = false;
                            }
                            return isValid;
                        });
                        return isValid;
                    }),
                    IsWarning: ko.computed(function () {
                        var isWarning = false;
                        $.each(validationFields, function (idx, validationField) {
                            if (validationField.IsWarning()) {
                                isWarning = true;
                            }
                            return !isWarning;
                        });
                        return isWarning;
                    }),
                    IsError: ko.computed(function () {
                        var isError = false;
                        $.each(validationFields, function (idx, validationField) {
                            if (validationField.IsError()) {
                                isError = true;
                            }
                            return !isError;
                        });
                        return isError;
                    })
                };
                return result;
            };
            ViewId.prototype.createArrayWrapupValidation = function (array, getWrapUp) {
                var result = {
                    IsValid: ko.computed(function () {
                        var isValid = true;
                        $.each(array(), function (idx, item) {
                            var wrapUp = getWrapUp(item);
                            if (!wrapUp.IsValid()) {
                                isValid = false;
                            }
                            return isValid;
                        });
                        return isValid;
                    }),
                    IsWarning: ko.computed(function () {
                        var isWarning = false;
                        $.each(array(), function (idx, item) {
                            var wrapUp = getWrapUp(item);
                            if (wrapUp.IsWarning()) {
                                isWarning = true;
                            }
                            return !isWarning;
                        });
                        return isWarning;
                    }),
                    IsError: ko.computed(function () {
                        var isError = false;
                        $.each(array(), function (idx, item) {
                            var wrapUp = getWrapUp(item);
                            if (wrapUp.IsError()) {
                                isError = true;
                            }
                            return !isError;
                        });
                        return isError;
                    })
                };
                return result;
            };
            ViewId.prototype.findValidationProperties = function (model) {
                var result = [];
                $.each(model, function (name, value) {
                    if (value && typeof value === 'object' &&
                        value.hasOwnProperty('IsValid') &&
                        value.hasOwnProperty('IsWarning') &&
                        value.hasOwnProperty('IsError') &&
                        value.hasOwnProperty('Message')) {
                        result.push(value);
                    }
                });
                return result;
            };
            return ViewId;
        })();
        WebAdmin.ViewId = ViewId;
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=viewid.js.map