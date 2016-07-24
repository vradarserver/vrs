namespace VRS.WebAdmin
{
    export class ViewId
    {
        private _LostContact = false;
        private _FailedAttempts = 0;
        private _ModalOverlay: JQuery;
        private _ShowModalOverlayTimer: number;

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
            this._ModalOverlay = $('<div />').addClass('modal-alert').hide().appendTo('body');

            this.configureAffixes();

            this.sendHeartbeat();
        }

        configureAffixes()
        {
            $('[data-spy="affix"]').each(function() {
                $(this).affix({
                    offset: {
                        top: $(this).offset().top
                    }
                });
            });
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

                            this._ModalOverlay
                                .empty()
                                .append($('<div />')
                                    .addClass('alert alert-danger text-center')
                                    .text(VRS.WebAdmin.$$.WA_Lost_Contact)
                                )
                                .show();
                        }
                    }
                }, false, true);
            }
        }

        /**
         * Shows or hides a modal overlay that prevents interaction with the page.
         */
        showModalOverlay(show: boolean)
        {
            if(show) {
                this._ModalOverlay.show();
            } else {
                this._ModalOverlay.hide();
            }
        }

        /**
         * Returns true if the modal overlay that prevents interaction with the page is visible.
         */
        isModalOverlayVisible()
        {
            return this._ModalOverlay.is(':visible');
        }

        /**
         * Sends an AJAX request to the view. Handles deferred execution responses automatically.
         */
        ajax(methodName: string, settings: JQueryAjaxSettings = {}, showModalOverlay = true, keepOverlayWhenFinished = false) : JQueryXHR
        {
            if(!this._LostContact) {
                if(methodName && !settings.url) {
                    settings.url = this.buildMethodUrl(methodName);
                }
                this.addViewIdToSettings(settings);

                if(showModalOverlay) {
                    if(!this.isModalOverlayVisible()) {
                        this._ShowModalOverlayTimer = setTimeout(() => {
                            this._ShowModalOverlayTimer = undefined;
                            this.showModalOverlay(true);
                        }, 100);
                    }
                }

                var removeOverlay = () => {
                    if(!keepOverlayWhenFinished) {
                        if(this._ShowModalOverlayTimer !== undefined) {
                            clearTimeout(this._ShowModalOverlayTimer);
                            this._ShowModalOverlayTimer = undefined;
                        }
                        this.showModalOverlay(false);
                    }
                };

                var success = settings.success || $.noop;
                settings.success = (response: any, textStatus: string, jqXHR: JQueryXHR) => {
                    if(this.isDeferredExecutionResponse(response)) {
                        this.fetchDeferredExecutionResponse(response.Response.JobId, success, 200, removeOverlay);
                    } else {
                        removeOverlay();
                        success(response, textStatus, jqXHR);
                    }
                };

                var error = settings.error || $.noop;
                settings.error = (jqXHR: JQueryXHR, textStatus: string, errorThrown: string) => {
                    if(showModalOverlay) {
                        this.showModalOverlay(false);
                    }
                    error(jqXHR, textStatus, errorThrown);
                };

                return $.ajax(settings);
            }
        }

        private buildMethodUrl(methodName: string) : string
        {
            return this._ViewName + '/' + methodName;
        }

        private addViewIdToSettings(settings: JQueryAjaxSettings)
        {
            var data = settings.data || {};
            if(this._Id) {
                data.__ViewId = this._Id;
            }
            settings.data = data;
        }

        private isDeferredExecutionResponse(response: any)
        {
            return response && response.Response && response.Response.DeferredExecution && response.Response.JobId;
        }

        private fetchDeferredExecutionResponse(jobId: string, success: (response: any, textStatus: string, jqXHR: JQueryXHR) => void, interval: number, removeOverlay: () => void)
        {
            if(!this._LostContact) {
                setTimeout(() => this.sendRequestForDeferredExecutionResponse(jobId, success, removeOverlay), interval);
            }
        }

        private sendRequestForDeferredExecutionResponse(jobId: string, success: (response: any, textStatus: string, jqXHR: JQueryXHR) => void, removeOverlay: () => void)
        {
            var settings: JQueryAjaxSettings = {
                url: this.buildMethodUrl('GetDeferredResponse'),
                data: {
                    jobId: jobId
                },
                success: (response: any, textStatus: string, jqXHR: JQueryXHR) => {
                    if(!this._LostContact) {
                        if(this.isDeferredExecutionResponse(response)) {
                            this.fetchDeferredExecutionResponse(jobId, success, 1000, removeOverlay);
                        } else {
                            removeOverlay();
                            success(response, textStatus, jqXHR);   // This may need a bit of adjusting if anything's expecting to see their original XHR...
                        }
                    }
                },
                error: () => {
                    if(!this._LostContact) {
                        this.fetchDeferredExecutionResponse(jobId, success, 5000, removeOverlay);
                    }
                }
            };
            this.addViewIdToSettings(settings);

            $.ajax(settings);
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
         * Can optionally also include an open list of standalone validation fields.
         */
        createArrayWrapupValidation<T>(array: KnockoutObservableArray<T>, getWrapUp: (item: T) => IValidation_KC, ...includeValidations: IValidation_KC[]) : IValidation_KC
        {
            var result = {
                IsValid: ko.computed(() => {
                    var isValid = true;
                    if(array) {
                        $.each(array(), (idx, item) => {
                            var wrapUp = getWrapUp(item);
                            if(!wrapUp.IsValid()) {
                                isValid = false;
                            }
                            return isValid;
                        });
                    }
                    if(isValid && includeValidations.length) {
                        $.each(includeValidations, (idx, item) => {
                            if(!item.IsValid()) {
                                isValid = false;
                            }
                            return isValid;
                        });
                    }
                    return isValid;
                }),

                IsWarning: ko.computed(() => {
                    var isWarning = false;
                    if(array) {
                        $.each(array(), (idx, item) => {
                            var wrapUp = getWrapUp(item);
                            if(wrapUp.IsWarning()) {
                                isWarning = true;
                            }
                            return !isWarning;
                        });
                    }
                    if(!isWarning && includeValidations.length) {
                        $.each(includeValidations, (idx, item) => {
                            if(item.IsWarning()) {
                                isWarning = true;
                            }
                            return !isWarning;
                        });
                    }
                    return isWarning;
                }),

                IsError: ko.computed(() => {
                    var isError = false;
                    if(array) {
                        $.each(array(), (idx, item) => {
                            var wrapUp = getWrapUp(item);
                            if(wrapUp.IsError()) {
                                isError = true;
                            }
                            return !isError;
                        });
                    }
                    if(!isError && includeValidations.length) {
                        $.each(includeValidations, (idx, item) => {
                            if(item.IsError()) {
                                isError = true;
                            }
                            return !isError;
                        });
                    }
                    return isError;
                })
            };

            return result;
        }

        /**
         * Returns an array of all properties of the model that look like they are validation model field objects.
         */
        findValidationProperties(model: Object,
                                 filter: (name: string, value: VirtualRadar.Interface.View.IValidationModelField_KO) => boolean = null,
                                 appendToArray: VirtualRadar.Interface.View.IValidationModelField_KO[] = []
                                ) : VirtualRadar.Interface.View.IValidationModelField_KO[]
        {
            var result = appendToArray;

            $.each(model, (name: string, value: Object) => {
                if(value && typeof value === 'object' &&
                    value.hasOwnProperty('IsValid') &&
                    value.hasOwnProperty('IsWarning') &&
                    value.hasOwnProperty('IsError') &&
                    value.hasOwnProperty('Message')
                ) {
                    var validationField = <VirtualRadar.Interface.View.IValidationModelField_KO>value;
                    if(!filter || filter(name, validationField)) {
                        result.push(validationField);
                    }
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