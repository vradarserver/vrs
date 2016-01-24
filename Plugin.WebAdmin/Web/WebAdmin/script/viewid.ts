namespace VRS.WebAdmin
{
    export class ViewId
    {
        private _LostContact = false;
        private _FailedAttempts = 0;

        private _Id: string;
        get Id() : string
        {
            return this._Id;
        }

        private _ViewName: string;
        get ViewName() : string
        {
            return this._ViewName;
        }

        constructor(viewName: string, viewId: string = null)
        {
            this._ViewName = viewName;
            this._Id = viewId;

            this.sendHeartbeat();
        }

        private setHeartbeatTimer(pauseInterval: number = 10000)
        {
            if(this._Id) {
                setTimeout(() => {
                    this.sendHeartbeat();
                }, pauseInterval);
            }
        }

        private sendHeartbeat()
        {
            if(this._Id) {
                this.ajax('BrowserHeartbeat', {
                    success: () => {
                        this._FailedAttempts = 0;
                        this.setHeartbeatTimer();
                    },
                    error: () => {
                        if(++this._FailedAttempts <= 5) {
                            this.setHeartbeatTimer(1000);
                        } else {
                            this._LostContact = true;

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
        }

        /**
         * Sends an AJAX request to the view. Handles deferred execution responses automatically.
         */
        ajax(methodName: string, settings: JQueryAjaxSettings = {}) : JQueryXHR
        {
            if(!this._LostContact) {
                if(methodName && !settings.url) {
                    settings.url = this._ViewName + '/' + methodName;
                }

                var data = settings.data || {};
                if(this._Id) {
                    data.__ViewId = this._Id;
                }
                settings.data = data;

                var success = settings.success || $.noop;
                settings.success = (response: any, textStatus: string, jqXHR: JQueryXHR) => {
                    if(this.isDeferredExecutionResponse(response)) {
                        this.fetchDeferredExecutionResponse(response.Response.JobId, success, 200);
                    } else {
                        success(response, textStatus, jqXHR);
                    }
                };

                return $.ajax(settings);
            }
        }

        private isDeferredExecutionResponse(response: any)
        {
            return response && response.Response && response.Response.DeferredExecution && response.Response.JobId;
        }

        private fetchDeferredExecutionResponse(jobId: string, success: (response: any, textStatus: string, jqXHR: JQueryXHR) => void, interval: number)
        {
            if(!this._LostContact) {
                setTimeout(() => this.sendRequestForDeferredExecutionResponse(jobId, success), interval);
            }
        }

        private sendRequestForDeferredExecutionResponse(jobId: string, success: (response: any, textStatus: string, jqXHR: JQueryXHR) => void)
        {
            this.ajax('GetDeferredResponse', {
                data: {
                    jobId: jobId
                },
                success: (response: any, textStatus: string, jqXHR: JQueryXHR) => {
                    if(this.isDeferredExecutionResponse(response)) {
                        this.fetchDeferredExecutionResponse(jobId, success, 1000);
                    } else {
                        success(response, textStatus, jqXHR);   // This may need a bit of adjusting if anything's expecting to see their original XHR...
                    }
                },
                error: () => {
                    this.fetchDeferredExecutionResponse(jobId, success, 5000);
                }
            });
        }

        /**
         * Creates a wrap-up validation field that reports on the state of a number of other validation fields.
         */
        createWrapupValidation(validationFields: VirtualRadar.Interface.View.IValidationModelField_KO[]) : IValidation_KC
        {
            var result = {
                IsValid: ko.computed(() => {
                    var isValid = true;
                    $.each(validationFields, (idx, validationField) => {
                        if(!validationField.IsValid()) {
                            isValid = false;
                        }
                        return isValid;
                    });
                    return isValid;
                }),
                IsWarning: ko.computed(() => {
                    var isWarning = false;
                    $.each(validationFields, (idx, validationField) => {
                        if(validationField.IsWarning()) {
                            isWarning = true;
                        }
                        return !isWarning;
                    });
                    return isWarning;
                }),
                IsError: ko.computed(() => {
                    var isError = false;
                    $.each(validationFields, (idx, validationField) => {
                        if(validationField.IsError()) {
                            isError = true;
                        }
                        return !isError;
                    });
                    return isError;
                })
            };

            return result;
        }

        /**
         * Creates a wrap-up validation field that reports on the state of every element in an array that contains other wrap-up fields.
         */
        createArrayWrapupValidation<T>(array: KnockoutObservableArray<T>, getWrapUp: (item: T) => IValidation_KC) : IValidation_KC
        {
            var result = {
                IsValid: ko.computed(() => {
                    var isValid = true;
                    $.each(array(), (idx, item) => {
                        var wrapUp = getWrapUp(item);
                        if(!wrapUp.IsValid()) {
                            isValid = false;
                        }
                        return isValid;
                    });
                    return isValid;
                }),
                IsWarning: ko.computed(() => {
                    var isWarning = false;
                    $.each(array(), (idx, item) => {
                        var wrapUp = getWrapUp(item);
                        if(wrapUp.IsWarning()) {
                            isWarning = true;
                        }
                        return !isWarning;
                    });
                    return isWarning;
                }),
                IsError: ko.computed(() => {
                    var isError = false;
                    $.each(array(), (idx, item) => {
                        var wrapUp = getWrapUp(item);
                        if(wrapUp.IsError()) {
                            isError = true;
                        }
                        return !isError;
                    });
                    return isError;
                })
            };

            return result;
        }

        /**
         * Returns an array of all properties of the model that look like they are validation model field objects.
         */
        findValidationProperties(model: Object) : VirtualRadar.Interface.View.IValidationModelField_KO[]
        {
            var result = [];

            $.each(model, (name: string, value: Object) => {
                if(value && typeof value === 'object' &&
                    value.hasOwnProperty('IsValid') &&
                    value.hasOwnProperty('IsWarning') &&
                    value.hasOwnProperty('IsError') &&
                    value.hasOwnProperty('Message')
                ) {
                    result.push(value);
                }
            });

            return result;
        }

        /**
         * Returns a description of an enum value.
         */
        describeEnum(enumValue: number, enumModels: VirtualRadar.Interface.View.IEnumModel[]) : string
        {
            var enumModel = VRS.arrayHelper.findFirst(enumModels, r => r.Value === enumValue);
            return enumModel ? enumModel.Description : null;
        }
    }
} 