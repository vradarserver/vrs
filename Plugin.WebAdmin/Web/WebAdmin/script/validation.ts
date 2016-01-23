namespace VRS.WebAdmin
{
    import ViewInterface = VirtualRadar.Interface.View;

    class ValidationField
    {
        validationResult:   ViewInterface.IValidationResultModel;

        constructor(public element: JQuery, public fieldName: string)
        {
        }

        getInputElement() : JQuery
        {
            var result = $('input,select,textarea', this.element);
            if(result.length == 0) {
                result = $(':checkbox', this.element)
            }

            return result;
        }
    }

    /**
     * Manages the display of validation results from the server.
     */
    export class Validation
    {
        private _WarningIcon = 'glyphicon-exclamation-sign';
        private _ErrorIcon = 'glyphicon-minus-sign';

        private _ValidationResults: ViewInterface.IValidationResultsModel;
        get ValidationResults()
        {
            return this._ValidationResults || {
                IsPartialValidation: false,
                PartialValidationFields: [],
                Results: []
            };
        }

        get HasError()
        {
            return VRS.arrayHelper.findFirst(this.ValidationResults.Results, (r) => !r.IsWarning) != null;
        }

        get HasWarning()
        {
            return VRS.arrayHelper.findFirst(this.ValidationResults.Results, (r) => r.IsWarning) != null;
        }

        applyValidationResults(validationResults: ViewInterface.IValidationResultsModel) : boolean
        {
            var result = this.updateValidationResults(validationResults);
            this.updateFields();

            return result;
        }

        updateValidationResults(validationResults: ViewInterface.IValidationResultsModel) : boolean
        {
            this._ValidationResults = validationResults;
            return !this.HasError;
        }

        updateFields()
        {
            this.clearWrapupParents();

            var fields = this.getFields();
            $.each(fields, (idx, field) => {
                var validationResult = field.validationResult;
                if(validationResult && validationResult.Message) {
                    this.addHelpBlockAndFeedback(field);
                } else {
                    this.removeHelpBlockAndFeedback(field);
                }
            });
        }

        private getFields() : ValidationField[]
        {
            var result = <ValidationField[]>[];

            $.each($('[data-vrs-val]'), (idx, htmlElement) => {
                var element = $(htmlElement);
                var fieldName = element.data('vrs-val');

                var field = new ValidationField(element, fieldName);
                field.validationResult = VRS.arrayHelper.findFirst(this.ValidationResults.Results, (r) => r.FieldName == fieldName);

                result.push(field);
            });

            return result;
        }

        private addHelpBlockAndFeedback(field: ValidationField)
        {
            var inputElement = field.getInputElement();
            var inputParent = inputElement.parent();
            var wrapupParents = this.getWrapupParents(field);

            var helpBlock = this.getHelpBlock(field);
            if(!helpBlock) {
                helpBlock = $('<span />')
                    .addClass('help-block')
                    .insertAfter(inputElement);
            }

            var feedbackBlock = this.getFeedbackBlock(field);
            if(!feedbackBlock) {
                feedbackBlock = $('<span />')
                    .addClass('form-control-feedback glyphicon')
                    .attr('aria-hidden', 'true')
                    .insertAfter(inputElement);
            }

            inputParent.addClass('has-feedback');
            if(field.validationResult.IsWarning) {
                feedbackBlock.removeClass(this._ErrorIcon).addClass(this._WarningIcon);
                inputParent.removeClass('has-error').addClass('has-warning');
                $.each(wrapupParents, (idx, wrapup) => this.setWrapupParentIcon(wrapup, true));
            } else {
                feedbackBlock.addClass(this._ErrorIcon).removeClass(this._WarningIcon);
                inputParent.addClass('has-error').removeClass('has-warning');
                $.each(wrapupParents, (idx, wrapup) => this.setWrapupParentIcon(wrapup, false));
            }
            helpBlock.text(field.validationResult.Message);
        }

        private removeHelpBlockAndFeedback(field: ValidationField)
        {
            var feedbackBlock = this.getFeedbackBlock(field);
            if(feedbackBlock) {
                feedbackBlock.remove();
            }

            var helpBlock = this.getHelpBlock(field);
            if(helpBlock) {
                helpBlock.remove();
            }

            var parent = field.getInputElement().parent();
            parent.removeClass('has-feedback has-error has-warning');
        }

        private getFeedbackBlock(field: ValidationField) : JQuery
        {
            var result = $('.form-control-feedback', field.element);
            return result.length > 0 ? result : null;
        }

        private getHelpBlock(field: ValidationField) : JQuery
        {
            var result = $('.help-block', field.element);
            return result.length > 0 ? result : null;
        }

        private getWrapupParents(field: ValidationField) : JQuery[]
        {
            var result = [];

            var parents = field.element.parents();
            $.each(parents.filter('[data-vrs-val-icon]'), (idx, parentElement) => {
                result.push($(parentElement));
            });
            $.each(parents.filter('.panel-collapse,.panel-body').siblings('.panel-heading'), (idx, panelHeadingElement) => {
                $.each($(panelHeadingElement).find('[data-vrs-val-icon]'), (innerIdx, wrapupElement) => {
                    result.push($(wrapupElement));
                });
            });

            return result;
        }

        private clearWrapupParents()
        {
            $('[data-vrs-val-icon]').removeClass('glyphicon').removeClass(this._WarningIcon).removeClass(this._ErrorIcon);
        }

        private setWrapupParentIcon(wrapup: JQuery, isWarning: boolean)
        {
            wrapup.addClass('glyphicon');
            if(!isWarning) {
                wrapup.removeClass(this._WarningIcon).addClass(this._ErrorIcon);
            } else {
                if(!wrapup.hasClass(this._ErrorIcon)) {
                    wrapup.addClass(this._WarningIcon);
                }
            }
        }
    }
} 