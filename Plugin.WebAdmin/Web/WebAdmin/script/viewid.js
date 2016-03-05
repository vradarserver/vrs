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
                this._ModalOverlay = $('<div />').addClass('modal-alert').hide().appendTo('body');
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
                                _this._ModalOverlay
                                    .empty()
                                    .append($('<div />')
                                    .addClass('alert alert-danger text-center')
                                    .text(VRS.WebAdmin.$$.WA_Lost_Contact))
                                    .show();
                            }
                        }
                    }, false);
                }
            };
            ViewId.prototype.showModalOverlay = function (show) {
                if (show) {
                    this._ModalOverlay.show();
                }
                else {
                    this._ModalOverlay.hide();
                }
            };
            ViewId.prototype.isModalOverlayVisible = function () {
                return this._ModalOverlay.is(':visible');
            };
            ViewId.prototype.ajax = function (methodName, settings, showModalOverlay, keepOverlayWhenFinished) {
                var _this = this;
                if (settings === void 0) { settings = {}; }
                if (showModalOverlay === void 0) { showModalOverlay = true; }
                if (keepOverlayWhenFinished === void 0) { keepOverlayWhenFinished = false; }
                if (!this._LostContact) {
                    if (methodName && !settings.url) {
                        settings.url = this.buildMethodUrl(methodName);
                    }
                    this.addViewIdToSettings(settings);
                    if (showModalOverlay) {
                        if (!this.isModalOverlayVisible()) {
                            this._ShowModalOverlayTimer = setTimeout(function () {
                                _this._ShowModalOverlayTimer = undefined;
                                _this.showModalOverlay(true);
                            }, 100);
                        }
                    }
                    var removeOverlay = function () {
                        if (!keepOverlayWhenFinished) {
                            if (_this._ShowModalOverlayTimer !== undefined) {
                                clearTimeout(_this._ShowModalOverlayTimer);
                                _this._ShowModalOverlayTimer = undefined;
                            }
                            _this.showModalOverlay(false);
                        }
                    };
                    var success = settings.success || $.noop;
                    settings.success = function (response, textStatus, jqXHR) {
                        if (_this.isDeferredExecutionResponse(response)) {
                            _this.fetchDeferredExecutionResponse(response.Response.JobId, success, 200, removeOverlay);
                        }
                        else {
                            removeOverlay();
                            success(response, textStatus, jqXHR);
                        }
                    };
                    var error = settings.error || $.noop;
                    settings.error = function (jqXHR, textStatus, errorThrown) {
                        if (showModalOverlay) {
                            _this.showModalOverlay(false);
                        }
                        error(jqXHR, textStatus, errorThrown);
                    };
                    return $.ajax(settings);
                }
            };
            ViewId.prototype.buildMethodUrl = function (methodName) {
                return this._ViewName + '/' + methodName;
            };
            ViewId.prototype.addViewIdToSettings = function (settings) {
                var data = settings.data || {};
                if (this._Id) {
                    data.__ViewId = this._Id;
                }
                settings.data = data;
            };
            ViewId.prototype.isDeferredExecutionResponse = function (response) {
                return response && response.Response && response.Response.DeferredExecution && response.Response.JobId;
            };
            ViewId.prototype.fetchDeferredExecutionResponse = function (jobId, success, interval, removeOverlay) {
                var _this = this;
                if (!this._LostContact) {
                    setTimeout(function () { return _this.sendRequestForDeferredExecutionResponse(jobId, success, removeOverlay); }, interval);
                }
            };
            ViewId.prototype.sendRequestForDeferredExecutionResponse = function (jobId, success, removeOverlay) {
                var _this = this;
                var settings = {
                    url: this.buildMethodUrl('GetDeferredResponse'),
                    data: {
                        jobId: jobId
                    },
                    success: function (response, textStatus, jqXHR) {
                        if (!_this._LostContact) {
                            if (_this.isDeferredExecutionResponse(response)) {
                                _this.fetchDeferredExecutionResponse(jobId, success, 1000, removeOverlay);
                            }
                            else {
                                removeOverlay();
                                success(response, textStatus, jqXHR);
                            }
                        }
                    },
                    error: function () {
                        if (!_this._LostContact) {
                            _this.fetchDeferredExecutionResponse(jobId, success, 5000, removeOverlay);
                        }
                    }
                };
                this.addViewIdToSettings(settings);
                $.ajax(settings);
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
                var includeValidations = [];
                for (var _i = 2; _i < arguments.length; _i++) {
                    includeValidations[_i - 2] = arguments[_i];
                }
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
                        if (isValid && includeValidations.length) {
                            $.each(includeValidations, function (idx, item) {
                                if (!item.IsValid()) {
                                    isValid = false;
                                }
                                return isValid;
                            });
                        }
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
                        if (!isWarning && includeValidations.length) {
                            $.each(includeValidations, function (idx, item) {
                                if (item.IsWarning()) {
                                    isWarning = true;
                                }
                                return !isWarning;
                            });
                        }
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
                        if (!isError && includeValidations.length) {
                            $.each(includeValidations, function (idx, item) {
                                if (item.IsError()) {
                                    isError = true;
                                }
                                return !isError;
                            });
                        }
                        return isError;
                    })
                };
                return result;
            };
            ViewId.prototype.findValidationProperties = function (model, filter, appendToArray) {
                if (filter === void 0) { filter = null; }
                if (appendToArray === void 0) { appendToArray = []; }
                var result = appendToArray;
                $.each(model, function (name, value) {
                    if (value && typeof value === 'object' &&
                        value.hasOwnProperty('IsValid') &&
                        value.hasOwnProperty('IsWarning') &&
                        value.hasOwnProperty('IsError') &&
                        value.hasOwnProperty('Message')) {
                        var validationField = value;
                        if (!filter || filter(name, validationField)) {
                            result.push(validationField);
                        }
                    }
                });
                return result;
            };
            ViewId.prototype.describeEnum = function (enumValue, enumModels) {
                var enumModel = VRS.arrayHelper.findFirst(enumModels, function (r) { return r.Value === enumValue; });
                return enumModel ? enumModel.Description : null;
            };
            return ViewId;
        })();
        WebAdmin.ViewId = ViewId;
    })(WebAdmin = VRS.WebAdmin || (VRS.WebAdmin = {}));
})(VRS || (VRS = {}));
//# sourceMappingURL=viewid.js.map